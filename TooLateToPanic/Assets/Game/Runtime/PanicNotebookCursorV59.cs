using UnityEngine;

namespace TooLateToPanic
{
    public enum PanicNotebookCursorModeV59 { Here, Offscreen, OtherFloor }

    // The same pure policy is callable by a headless Editor probe; no scene/player is required.
    public static class PanicNotebookCursorPolicyV59
    {
        public static bool CanShow(PanicPhase phase, bool manualTarget, bool blocked)
        { return manualTarget && !blocked && (phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial || phase == PanicPhase.Inspection); }
        public static bool TargetVisibleInStage(bool isSource, bool isSupply, bool bagStage)
        { return isSource && !bagStage || isSupply && bagStage; }
        public static PanicNotebookCursorModeV59 Mode(int floor, int activeFloor, bool onScreen)
        { return floor != activeFloor ? PanicNotebookCursorModeV59.OtherFloor : onScreen ? PanicNotebookCursorModeV59.Here : PanicNotebookCursorModeV59.Offscreen; }
        public static string Heading(PanicNotebookCursorModeV59 mode, int floor)
        { return mode == PanicNotebookCursorModeV59.Here ? "BURADA" : mode == PanicNotebookCursorModeV59.OtherFloor ? floor < 0 ? "BODRUMDA" : floor > 0 ? "ÜST KATTA" : "ALT KATTA" : "BU YÖNDE"; }
        public static Vector2 EdgeAnchor(Vector2 projected, bool inFront, Rect safe, out Vector2 direction)
        {
            direction = projected - safe.center;
            if (!inFront) direction = -direction;
            if (direction.sqrMagnitude < .001f) direction = Vector2.up;
            direction.Normalize();
            Vector2 half = safe.size * .5f;
            float x = Mathf.Abs(direction.x) < .001f ? float.PositiveInfinity : half.x / Mathf.Abs(direction.x);
            float y = Mathf.Abs(direction.y) < .001f ? float.PositiveInfinity : half.y / Mathf.Abs(direction.y);
            return safe.center + direction * Mathf.Min(x, y);
        }
        // Coordinated with the caption worker's public DialogueSubtitleBoundsV59.
        // Conservative clearance whenever a subtitle exists also covers its fade/suspension transitions.
        public static Rect CaptionClearance { get { return new Rect(440f, 116f, 690f, 94f); } }
    }

    // Presentation snapshot from the existing manual notebook pin. It never chooses a nearby target.
    public struct PanicNotebookCursorLayoutV59
    {
        public string Id, Label, Heading, Detail;
        public int Floor;
        public Bounds WorldBounds;
        public Rect Outline;
        public Vector2 Anchor, Direction;
        public PanicNotebookCursorModeV59 Mode;
    }

    public sealed partial class PanicHomeWorld
    {
        public bool TryGetNotebookCursorTargetV59(out string id, out string label, out Bounds bounds, out int floor)
        {
            id = label = string.Empty; bounds = new Bounds(); floor = 0;
            if (!HasNotebookTargetV57 || Expedition == null) return false;
            string targetId = NotebookTargetIdV57;
            id = targetId; label = NotebookTargetLabelV57;
            var source = Expedition.Sources.Find(value => value != null && value.Id == targetId);
            var supply = source == null ? Expedition.FindWorldSupply(id) : null;
            if (source == null && supply == null) return false;
            // Preparation materials disappear in the FPS supply stage, and supplies are hidden before it.
            // Keep the pin's existing lifetime; only its locator is suppressed when that object is hidden.
            if (!PanicNotebookCursorPolicyV59.TargetVisibleInStage(source != null, supply != null, BagFinalStageActive)) return false;
            // Real authored floor metadata, never the height of a tall object's centre.
            floor = source != null ? source.Floor : supply.Floor;
            return TryGetInteractionBounds(id, out bounds) && bounds.size.sqrMagnitude > .000001f;
        }
    }

    /// <summary>Cosmetic, non-interactive cursor for a manually pinned notebook target only.
    /// The main HUD owns the canvas, modal precedence and final blackout. No material, transform,
    /// selection/focus, RNG, model, physics, movement, collection or input ownership is changed.</summary>
    public sealed partial class PanicHomeHud
    {
        GUIStyle notebookCursorTitleV59, notebookCursorLabelV59, notebookCursorDetailV59;
        public int NotebookCursorDrawFrameV59 { get; private set; } = -1;
        public int NotebookCursorDrawCountV59 { get; private set; }
        public string NotebookCursorRenderedIdV59 { get; private set; }
        public string NotebookCursorRenderedTextV59 { get; private set; }
        public PanicNotebookCursorModeV59 NotebookCursorRenderedModeV59 { get; private set; }
        public Rect NotebookCursorRenderedOutlineV59 { get; private set; }
        public Rect NotebookCursorRenderedLabelV59 { get; private set; }
        public float NotebookCursorRenderedGlowV59 { get; private set; }
        public bool NotebookCursorRenderedV59 { get { return NotebookCursorDrawFrameV59 == Time.frameCount; } }
        public bool NotebookCursorVisibleV59
        {
            get
            {
                if (!World || World.Model == null || !World.View) return false;
                bool blocked = _paused || World.Model.IsPaused || _developerMenuOpen || _helpVisible ||
                    DeveloperInputBlockedThisFrame || FreeplayOnboardingInputBlockedV414 ||
                    KeyboardHelpBlockingV56 || WaveLedgerInputBlockedV56 || SimulationSelectorOpen ||
                    World.NotebookBlocksInput || World.CinematicActive || World.LossAnnounced ||
                    World.InputLocked || World.InventoryOpen || World.WorkActive || World.IsMoving ||
                    World.LootVisible || World.BagCameraTransitionActive || !World.JuryAllowsMovement;
                return PanicNotebookCursorPolicyV59.CanShow(World.Model.Phase, World.HasNotebookTargetV57, blocked);
            }
        }

        static bool CursorFiniteV59(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }

        public bool TryGetNotebookCursorLayoutV59(out PanicNotebookCursorLayoutV59 layout)
        {
            layout = new PanicNotebookCursorLayoutV59();
            if (!NotebookCursorVisibleV59 || _scale <= 0f ||
                !World.TryGetNotebookCursorTargetV59(out var id, out var label, out var bounds, out var floor)) return false;
            var camera = World.View;
            Vector3 projected = camera.WorldToScreenPoint(bounds.center);
            if (!CursorFiniteV59(projected.x) || !CursorFiniteV59(projected.y) || !CursorFiniteV59(projected.z)) return false;
            Vector2 point = (new Vector2(projected.x, Screen.height - projected.y) - _offset) / _scale;
            // Keep locator labels clear of the status strip, bottom controls and notebook helper.
            Rect safe = Rect.MinMaxRect(32f, 160f, CanvasWidth - 32f, 716f);
            Vector2 edgeAnchor = PanicNotebookCursorPolicyV59.EdgeAnchor(point, projected.z > camera.nearClipPlane, safe, out var direction);

            float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
            bool fullyInFront = true;
            for (int mask = 0; mask < 8; mask++)
            {
                Vector3 corner = bounds.center + Vector3.Scale(bounds.extents,
                    new Vector3((mask & 1) == 0 ? -1 : 1, (mask & 2) == 0 ? -1 : 1, (mask & 4) == 0 ? -1 : 1));
                Vector3 p = camera.WorldToScreenPoint(corner);
                if (p.z <= camera.nearClipPlane || !CursorFiniteV59(p.x) || !CursorFiniteV59(p.y)) { fullyInFront = false; break; }
                Vector2 logical = (new Vector2(p.x, Screen.height - p.y) - _offset) / _scale;
                minX = Mathf.Min(minX, logical.x); maxX = Mathf.Max(maxX, logical.x);
                minY = Mathf.Min(minY, logical.y); maxY = Mathf.Max(maxY, logical.y);
            }
            Rect outline = fullyInFront ? Rect.MinMaxRect(minX - 6f, minY - 6f, maxX + 6f, maxY + 6f) : new Rect();
            bool onScreen = fullyInFront && projected.z > camera.nearClipPlane && safe.Contains(point) &&
                outline.width > 0f && outline.height > 0f;
            PanicNotebookCursorModeV59 mode = PanicNotebookCursorPolicyV59.Mode(floor, World.ActiveFloor, onScreen);
            if (mode == PanicNotebookCursorModeV59.Here)
            {
                // Clip only at viewport edges; never shift the outline off the actual object.
                outline = Rect.MinMaxRect(Mathf.Max(12f, outline.xMin), Mathf.Max(142f, outline.yMin),
                    Mathf.Min(CanvasWidth - 12f, outline.xMax), Mathf.Min(CanvasHeight - 74f, outline.yMax));
                point = new Vector2(Mathf.Clamp(outline.center.x, safe.xMin, safe.xMax), Mathf.Clamp(outline.yMin, safe.yMin, safe.yMax));
            }
            else
            {
                outline = new Rect();
                // Same-screen other-floor targets retain their physical projection. Others use a bounded edge cue.
                if (!onScreen)
                {
                    point = edgeAnchor;
                }
                point.x = Mathf.Clamp(point.x, safe.xMin, safe.xMax); point.y = Mathf.Clamp(point.y, safe.yMin, safe.yMax);
            }
            int meters = Mathf.CeilToInt(Vector3.Distance(World.PlayerPosition, World.NotebookTargetPositionV57));
            layout = new PanicNotebookCursorLayoutV59 {
                Id = id, Label = label, Floor = floor, WorldBounds = bounds, Outline = outline,
                Anchor = point, Direction = direction, Mode = mode,
                Heading = PanicNotebookCursorPolicyV59.Heading(mode, floor),
                Detail = PanicNotebookData.FloorName(floor) + " · " + meters + " m"
            };
            return true;
        }

        // One call from the existing main HUD after normal cards, before the final blackout.
        public void DrawNotebookCursorV59()
        {
            if (!TryGetNotebookCursorLayoutV59(out var layout)) return;
            if (notebookCursorTitleV59 == null)
            {
                notebookCursorTitleV59 = MakeStyle(_condensedFont, 22, FontStyle.Bold, Brass, TextAnchor.MiddleLeft);
                notebookCursorLabelV59 = MakeStyle(_bodyFont, 22, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
                notebookCursorDetailV59 = MakeStyle(_condensedFont, 20, FontStyle.Normal, SoftText, TextAnchor.MiddleLeft);
            }
            bool reduced = World.ReducedFpsMotionV54 || World.Notebook && World.Notebook.Animation.ReducedMotion;
            float shimmer = reduced ? .85f : .85f + .15f * Mathf.Sin(Time.unscaledTime * 2.1f);
            Color ink = new Color(Brass.r, Brass.g, Brass.b, .86f + .14f * shimmer);
            if (layout.Mode == PanicNotebookCursorModeV59.Here)
            {
                CursorCornersV59(layout.Outline, new Color(Brass.r, Brass.g, Brass.b, .12f * shimmer), 10f);
                CursorCornersV59(layout.Outline, new Color(Brass.r, Brass.g, Brass.b, .23f * shimmer), 6f);
                CursorCornersV59(layout.Outline, ink, 2f);
            }

            const float width = 304f, height = 84f;
            Rect proposed = new Rect(Mathf.Clamp(layout.Anchor.x + 22f, 16f, CanvasWidth - width - 16f),
                Mathf.Clamp(layout.Anchor.y - height - 20f, 148f, 716f - height), width, height);
            if (!TryPlaceNotebookCursorCardV59(proposed, out var card)) return;
            card = ReserveWorldMarkerBounds(card, "Defter hedefi · " + layout.Label);
            // Visual-only leader/cursor. This does not register a button, steal a click or focus an object.
            Vector2 end = new Vector2(Mathf.Clamp(layout.Anchor.x, card.xMin, card.xMax), Mathf.Clamp(layout.Anchor.y, card.yMin, card.yMax));
            if (Vector2.Distance(end, layout.Anchor) > 8f) CursorLineV59(end, layout.Anchor, new Color(Brass.r, Brass.g, Brass.b, .65f), 1.5f);
            Vector2 pointer = layout.Mode == PanicNotebookCursorModeV59.Here ? Vector2.up : layout.Direction;
            Vector2 side = new Vector2(-pointer.y, pointer.x);
            Vector2 tip = layout.Anchor + pointer * (reduced ? 0f : shimmer * 2f);
            CursorLineV59(tip - pointer * 11f + side * 7f, tip, ink, 3f);
            CursorLineV59(tip - pointer * 11f - side * 7f, tip, ink, 3f);

            Frame(card, new Color(Night.r, Night.g, Night.b, .95f), new Color(Brass.r, Brass.g, Brass.b, .72f), 1f);
            Fill(new Rect(card.x, card.y, 3f, card.height), Brass);
            Label(new Rect(card.x + 14f, card.y + 4f, 118f, 25f), layout.Heading, notebookCursorTitleV59);
            Label(new Rect(card.x + 132f, card.y + 4f, 158f, 25f), "DEFTER HEDEFİ", notebookCursorDetailV59);
            FittedLabel(new Rect(card.x + 14f, card.y + 29f, width - 28f, 28f), layout.Label, notebookCursorLabelV59, MainText, 20);
            Label(new Rect(card.x + 14f, card.y + 57f, width - 28f, 22f), layout.Detail, notebookCursorDetailV59);
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                if (NotebookCursorDrawFrameV59 != Time.frameCount) NotebookCursorDrawCountV59++;
                NotebookCursorDrawFrameV59 = Time.frameCount;
                NotebookCursorRenderedIdV59 = layout.Id; NotebookCursorRenderedModeV59 = layout.Mode;
                NotebookCursorRenderedTextV59 = layout.Heading + " · DEFTER HEDEFİ\n" + layout.Label + "\n" + layout.Detail;
                NotebookCursorRenderedOutlineV59 = layout.Outline; NotebookCursorRenderedLabelV59 = card;
                NotebookCursorRenderedGlowV59 = shimmer;
            }
        }

        bool TryPlaceNotebookCursorCardV59(Rect proposed, out Rect best)
        {
            best = proposed; float nearest = float.PositiveInfinity;
            bool Clear(Rect candidate)
            {
                // Bottom controls own y>=716; selection, requirements and assistant register above.
                if (candidate.xMin < 16f || candidate.xMax > CanvasWidth - 16f || candidate.yMin < 148f || candidate.yMax > 716f || !WorldMarkerClear(candidate)) return false;
                if (!string.IsNullOrEmpty(World.PresentationSubtitle) && candidate.Overlaps(PanicNotebookCursorPolicyV59.CaptionClearance)) return false;
                if (_worldMarkerReservationFrame == Time.frameCount && Event.current != null && _worldMarkerReservationEvent == Event.current.type)
                    foreach (var prior in _worldMarkerReservations)
                    {
                        Rect padded = prior; padded.xMin -= 6f; padded.xMax += 6f; padded.yMin -= 6f; padded.yMax += 6f;
                        if (candidate.Overlaps(padded)) return false;
                    }
                return true;
            }
            if (Clear(proposed)) return true;
            // A small finite search keeps this optional locator outside actual drawn HUD panels.
            for (int row = 0; row < 6; row++) for (int col = 0; col < 5; col++)
            {
                Rect candidate = new Rect(16f + col * 316f, 148f + row * 94f, proposed.width, proposed.height);
                if (!Clear(candidate)) continue;
                float distance = (candidate.center - proposed.center).sqrMagnitude;
                if (distance < nearest) { nearest = distance; best = candidate; }
            }
            return !float.IsPositiveInfinity(nearest);
        }

        void CursorLineV59(Vector2 from, Vector2 to, Color color, float thickness)
        {
            // Never paint glowing corners or a leader over a higher-priority HUD/caption panel.
            // Omitting an obscured cosmetic segment is preferable to moving it off the real bounds.
            Rect region = Rect.MinMaxRect(Mathf.Min(from.x, to.x) - thickness, Mathf.Min(from.y, to.y) - thickness,
                Mathf.Max(from.x, to.x) + thickness, Mathf.Max(from.y, to.y) + thickness);
            if (!WorldMarkerClear(region) || !string.IsNullOrEmpty(World.PresentationSubtitle) && region.Overlaps(PanicNotebookCursorPolicyV59.CaptionClearance)) return;
            DrawLine(from, to, color, thickness);
        }

        void CursorCornersV59(Rect rect, Color color, float thickness)
        {
            if (rect.width <= 0f || rect.height <= 0f) return;
            float length = Mathf.Clamp(Mathf.Min(rect.width, rect.height) * .24f, 8f, 22f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 corner = new Vector2((i & 1) == 0 ? rect.xMin : rect.xMax, (i & 2) == 0 ? rect.yMin : rect.yMax);
                Vector2 horizontal = (i & 1) == 0 ? Vector2.right : Vector2.left;
                Vector2 vertical = (i & 2) == 0 ? Vector2.up : Vector2.down;
                CursorLineV59(corner, corner + horizontal * length, color, thickness);
                CursorLineV59(corner, corner + vertical * length, color, thickness);
            }
        }
    }
}
