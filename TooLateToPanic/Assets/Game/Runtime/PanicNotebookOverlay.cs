using System;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Offline IMGUI journal adapted from defter-mockup. Bind explicitly from World.
    /// World/HUD must include BlocksInput in their clock/input gates; this component never owns InputLocked.
    /// </summary>
    [DefaultExecutionOrder(-800)]
    public sealed class PanicNotebookOverlay : MonoBehaviour
    {
        static readonly Color Ink = new Color32(58, 69, 68, 255);
        static readonly Color Green = new Color32(79, 115, 101, 255);
        static readonly Color Paper = new Color32(253, 248, 232, 255);
        static readonly Color PaperBack = new Color32(237, 229, 207, 255);
        static readonly Color Yellow = new Color32(239, 205, 126, 255);
        static readonly Color Olive = new Color32(135, 154, 120, 255);
        static readonly Color Alarm = new Color32(188, 71, 73, 255);
        static readonly Color Rule = new Color(47 / 255f, 62 / 255f, 70 / 255f, .13f);
        readonly PanicNotebookAnimation animationState = new PanicNotebookAnimation();
        readonly PanicNotebookStrip[] strips = new PanicNotebookStrip[PanicNotebookPaper.StripCount];
        readonly int[] stripOrder = new int[PanicNotebookPaper.StripCount];
        PanicHomeWorld world;
        Func<bool> modalBlocked;
        PanicNotebookSnapshot snapshot, turningSnapshot;
        GUIStyle titleStyle, headingStyle, bodyStyle, smallStyle, tinyStyle, centerStyle, valueStyle;
        int processedFrame = -1, transitionFrame = -1, floor, sourceOffset;
        float nextSnapshot;
        bool pointerOwned, pendingInventory;
        string selectedSource, pendingFocus;
        PanicPhase pendingPhase;
        int pendingWave;
        float scale;
        Vector2 offset;
        bool compact;
        Matrix4x4 canvasMatrix;
        Texture2D spiralTextureV5;

        public PanicNotebookAnimation Animation { get { return animationState; } }
        public PanicNotebookSnapshot Snapshot { get { return snapshot; } }
        public bool IsVisible { get { return isActiveAndEnabled && animationState.Visible; } }
        public bool BlocksInput { get { return isActiveAndEnabled && (animationState.Visible || pointerOwned || pendingInventory || !string.IsNullOrEmpty(pendingFocus) || transitionFrame == Time.frameCount); } }
        public bool InputBlockedThisFrame { get { return transitionFrame == Time.frameCount; } }
        public bool CanOpen { get { return isActiveAndEnabled && CanOpenNow(); } }
        public Rect LauncherScreenBounds { get { Layout(); return ScreenRect(LauncherRect); } }
        public Rect BookScreenBounds { get { Layout(); return ScreenRect(new Rect(Book.x, Book.y - 65, Book.width + (compact ? 121 : 147), Book.height + (compact ? 172 : 164))); } }
        public Rect CloseScreenBounds { get { Layout(); return ScreenRect(CloseRect); } }
        public Rect PageScreenBounds(PanicNotebookPage page) { Layout(); return ScreenRect(TabRect((int)page)); }

        float CanvasWidth { get { return compact ? 760 : 1600; } }
        float CanvasHeight { get { return compact ? 1100 : 900; } }
        Rect Book { get { return compact ? new Rect(24, 170, 590, 820) : new Rect(114, 146, 1080, 650); } }
        float PageWidth { get { return compact ? Book.width : Book.width * .5f; } }
        Rect LauncherRect { get { return new Rect(CanvasWidth - 168, CanvasHeight - 62, 144, 38); } }
        Rect CloseRect { get { return new Rect(Book.x + Book.width - 208, Book.y - 59, 208, 42); } }
        Rect MotionRect { get { return new Rect(Book.x, Book.y + Book.height + 19, 240, 36); } }
        Rect InventoryRect { get { return new Rect(28, Book.height - 92, PageWidth - 56, 47); } }
        Rect TabRect(int index)
        {
            float height = compact ? 126 : 98;
            return new Rect(Book.xMax + 9, Book.y + 8 + index * (height + 7), compact ? 112 : 138, height);
        }
        Rect FloorRect(int index) { return new Rect(28 + index * ((PageWidth - 56) / 3), 101, (PageWidth - 56) / 3 - 5, 38); }
        Rect PlanRect { get { return new Rect(28, 155, PageWidth - 56, compact ? 268 : 209); } }
        Rect SourceRow(int index) { return new Rect(28, PlanRect.yMax + 17 + index * 43, PageWidth - 56, 39); }
        int VisibleSourceRows { get { return compact ? 4 : 3; } }

        public void Bind(PanicHomeWorld owner, Func<bool> isModalBlocked)
        {
            ForceClosed(); world = owner; modalBlocked = isModalBlocked;
            floor = world ? world.ActiveFloor : 0; RefreshSnapshot();
        }
        void Update() { ProcessInput(); }
        void OnDisable() { ForceClosed(); pointerOwned = false; }
        void OnDestroy() { if (spiralTextureV5) Destroy(spiralTextureV5); }

        /// <summary>Call at the start of World.Update before clocks, and before HUD.Update shortcuts. Idempotent per frame.</summary>
        public void ProcessInput()
        {
            if (!isActiveAndEnabled || !world || world.Model == null || processedFrame == Time.frameCount) return;
            processedFrame = Time.frameCount; Layout();
            if (pointerOwned)
            {
                transitionFrame = Time.frameCount;
                if (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0)) pointerOwned = false;
            }
            bool blocked = ExternalModalBlocked();
            // F2 keeps developer ownership. No saved pause/lock state can be restored by this overlay.
            if (Input.GetKeyDown(KeyCode.F2)) { if (animationState.Visible) ForceClosed(); return; }
            if (blocked || !PhaseAllowsNotebook())
            {
                if (animationState.Visible || pendingInventory || !string.IsNullOrEmpty(pendingFocus)) ForceClosed();
                return;
            }
            if (animationState.Visible && (world.WorkActive || world.IsMoving || world.InventoryOpen || world.InputLocked))
            { ForceClosed(); return; }
            animationState.Tick(Time.unscaledDeltaTime);
            if (snapshot == null || Time.unscaledTime >= nextSnapshot) RefreshSnapshot();
            if (!animationState.Turning) turningSnapshot = null;

            if (!animationState.Visible && (pendingInventory || !string.IsNullOrEmpty(pendingFocus)))
            {
                if (world.Model.Phase != pendingPhase || world.Model.Wave != pendingWave) { ClearPending(); return; }
                if (CanOpenNow() && transitionFrame != Time.frameCount)
                {
                    bool openBag = pendingInventory; string focus = pendingFocus; ClearPending();
                    transitionFrame = Time.frameCount;
                    if (openBag) world.ToggleInventory(); else if (!string.IsNullOrEmpty(focus)) world.SetFocus(focus);
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.J)) { if (animationState.TargetOpen) RequestClose(); else RequestOpen(); return; }
            if (animationState.Visible)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) { RequestClose(); return; }
                if (!animationState.TargetOpen) return;
                if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.B)) { RequestInventory(); return; }
                int delta = (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.PageDown) ? 1 : 0) -
                    (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.PageUp) ? 1 : 0);
                if (delta != 0) { RequestPage((PanicNotebookPage)(((int)animationState.RequestedPage + delta + 6) % 6)); return; }
                if (!animationState.Turning && animationState.CurrentPage == PanicNotebookPage.Rooms && snapshot != null && Mathf.Abs(Input.mouseScrollDelta.y) > .1f)
                    sourceOffset = Mathf.Clamp(sourceOffset - (int)Mathf.Sign(Input.mouseScrollDelta.y), 0, Mathf.Max(0, snapshot.Pins.Length - VisibleSourceRows));
                if (Input.GetMouseButtonDown(0)) ProcessPointer(LogicalPointer());
            }
            else if (CanOpenNow() && Input.GetMouseButtonDown(0) && LauncherRect.Contains(LogicalPointer()))
            { pointerOwned = true; RequestOpen(); }
        }

        public bool RequestOpen()
        {
            if (!CanOpenNow()) return false;
            ClearPending(); transitionFrame = Time.frameCount;
            if (!animationState.Visible) { floor = world.ActiveFloor; selectedSource = null; }
            animationState.SetOpen(true); RefreshSnapshot(); return true;
        }
        public void RequestClose()
        {
            if (!animationState.Visible && !animationState.TargetOpen) return;
            transitionFrame = Time.frameCount; animationState.SetOpen(false);
        }
        public void ForceClosed()
        {
            bool owned = animationState.Visible || pendingInventory || !string.IsNullOrEmpty(pendingFocus);
            animationState.ForceClosed(); ClearPending(); turningSnapshot = null;
            if (owned) transitionFrame = Time.frameCount;
        }
        public void RequestPage(PanicNotebookPage page)
        {
            if (!animationState.TargetOpen) return;
            if (!animationState.Turning) turningSnapshot = snapshot;
            animationState.RequestPage(page);
        }
        public void SetReducedMotion(bool value) { animationState.SetReducedMotion(value); turningSnapshot = null; }
        public bool RequestInventory()
        {
            if (!animationState.TargetOpen || !world || (world.Model.Phase != PanicPhase.Preparing && world.Model.Phase != PanicPhase.Tutorial)) return false;
            pendingInventory = true; pendingFocus = null; pendingPhase = world.Model.Phase; pendingWave = world.Model.Wave;
            RequestClose(); return true;
        }
        public void SelectFloor(int value) { floor = Mathf.Clamp(value, -1, 1); sourceOffset = 0; selectedSource = null; RefreshSnapshot(); }
        public void RefreshSnapshot() { snapshot = PanicNotebookData.Capture(world, floor); nextSnapshot = Time.unscaledTime + .2f; }

        bool PhaseAllowsNotebook()
        {
            if (!world || world.Model == null || world.CinematicActive || world.LossAnnounced || !world.JuryAllowsMovement) return false;
            var phase = world.Model.Phase;
            return phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial || phase == PanicPhase.Inspection;
        }
        bool ExternalModalBlocked() { return modalBlocked != null && modalBlocked(); }
        bool CanOpenNow()
        {
            return world && world.Expedition != null && PhaseAllowsNotebook() && !ExternalModalBlocked() &&
                !world.InputLocked && !world.WorkActive && !world.IsMoving && !world.InventoryOpen;
        }
        void ClearPending() { pendingInventory = false; pendingFocus = null; }

        void ProcessPointer(Vector2 point)
        {
            pointerOwned = true; transitionFrame = Time.frameCount;
            if (CloseRect.Contains(point)) { RequestClose(); return; }
            if (MotionRect.Contains(point)) { SetReducedMotion(!animationState.ReducedMotion); return; }
            for (int i = 0; i < 6; i++) if (TabRect(i).Contains(point)) { RequestPage((PanicNotebookPage)i); return; }
            if (animationState.Turning || animationState.Openness < 1f) return;
            Vector2 local = point - Book.position;
            if (animationState.CurrentPage == PanicNotebookPage.Rooms)
            {
                for (int i = 0; i < 3; i++) if (FloorRect(i).Contains(local)) { SelectFloor(i - 1); return; }
                if (snapshot == null) return;
                for (int i = 0; i < snapshot.Pins.Length; i++)
                {
                    bool rowVisible = i >= sourceOffset && i < sourceOffset + VisibleSourceRows;
                    if (PinRect(snapshot.Pins[i]).Contains(local) || rowVisible && SourceRow(i - sourceOffset).Contains(local))
                    {
                        selectedSource = snapshot.Pins[i].Id; return;
                    }
                }
                if (InventoryRect.Contains(local) && !string.IsNullOrEmpty(selectedSource))
                {
                    pendingFocus = selectedSource; pendingInventory = false; pendingPhase = world.Model.Phase; pendingWave = world.Model.Wave; RequestClose();
                }
            }
            else if (animationState.CurrentPage == PanicNotebookPage.Inventory && InventoryRect.Contains(local)) RequestInventory();
        }

        void Layout()
        {
            compact = Screen.height > Screen.width * .95f;
            scale = Mathf.Max(.01f, Mathf.Min(Screen.width / CanvasWidth, Screen.height / CanvasHeight));
            offset = new Vector2((Screen.width - CanvasWidth * scale) * .5f, (Screen.height - CanvasHeight * scale) * .5f);
            canvasMatrix = Matrix4x4.TRS(new Vector3(offset.x, offset.y, 0), Quaternion.identity, new Vector3(scale, scale, 1));
        }
        Rect ScreenRect(Rect rect) { return new Rect(offset.x + rect.x * scale, offset.y + rect.y * scale, rect.width * scale, rect.height * scale); }
        Vector2 LogicalPointer() { return (new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y) - offset) / scale; }

        void OnGUI()
        {
            if (!world || world.Model == null || ExternalModalBlocked()) return;
            bool visible = animationState.Visible;
            if (!visible && !CanOpenNow()) return;
            Layout(); EnsureStyles();
            Matrix4x4 oldMatrix = GUI.matrix; Color oldColor = GUI.color, oldContent = GUI.contentColor; int oldDepth = GUI.depth;
            GUI.depth = -180; GUI.matrix = canvasMatrix; GUI.color = Color.white; GUI.contentColor = Color.white;
            try
            {
                if (!visible)
                {
                    DrawButton(LauncherRect, "J  ·  DEFTER", Green, Paper, true);
                    return;
                }
                if (snapshot == null) RefreshSnapshot();
                if (snapshot == null) return;
                float ease = animationState.OpenEase;
                Fill(new Rect(-offset.x / scale, -offset.y / scale, Screen.width / scale, Screen.height / scale), new Color(.045f, .065f, .065f, .88f * ease));
                GUI.color = new Color(1, 1, 1, Mathf.Max(.05f, ease));
                Label(new Rect(Book.x, Book.y - 62, Book.width - 228, 38), "HAZIRLIK DEFTERİ", headingStyle, Paper);
                DrawButton(CloseRect, "KAPAT  ·  J / ESC", Ink, Paper, true);
                Label(compact ? new Rect(Book.x, 17, Book.width, 24) : new Rect(Book.x + 320, Book.y - 49, Book.width - 548, 25), snapshot.StoryLocation, tinyStyle, Paper);
                if (compact && snapshot != null) Label(new Rect(Book.x, 48, Book.width, 52), snapshot.Disaster + "  ·  " + snapshot.Clock + "     BÜTÇE  " + snapshot.Budget + "\nHASAR  %" + snapshot.Damage.ToString("0") + "     ÇANTA  " + snapshot.BagUsed + "/" + snapshot.BagCapacity, smallStyle, Paper);
                Matrix4x4 oldBookMatrix = GUI.matrix;
                GUI.matrix = canvasMatrix * Matrix4x4.TRS(new Vector3(Book.x, Book.y + 16 * (1 - ease), 0), Quaternion.identity, Vector3.one);
                // V5 warm paper, cloth cover, punched holes and actual curved wire binding.
                DrawBookCoverV5();
                for (int i = 3; i > 0; i--) Frame(new Rect(i * 3, i * 4, Book.width, Book.height), PaperBack, new Color(.15f, .2f, .18f, .38f));
                Frame(new Rect(0, 0, Book.width, Book.height), Paper, Ink);
                PaperGrid(new Rect(0, 0, Book.width, Book.height));
                if (!compact) { Fill(new Rect(PageWidth - 1, 34, 1, Book.height - 62), new Color(.32f, .40f, .35f, .18f)); DrawRightPage(snapshot); }
                DrawLeftPage(animationState.Turning ? animationState.ToPage : animationState.CurrentPage, snapshot);
                if (animationState.Turning && Event.current.type == EventType.Repaint) DrawTurningLeaf();
                DrawSpiral();
                GUI.matrix = oldBookMatrix;
                DrawTabs();
                DrawButton(MotionRect, animationState.ReducedMotion ? "HAREKET: AZ" : "HAREKET: TAM", PaperBack, Ink, true);
                Label(new Rect(Book.x + 256, Book.yMax + 25, Book.width - 255, 31), compact ? "← / →  SAYFA   ·   TAB  ÇANTA" : "← / →  SAYFA    ·    TAB  ÇANTAYI DÜZENLE    ·    DEFTER AÇIKKEN SÜRE DURUR", tinyStyle, Paper);
                Label(new Rect(Book.x, Book.yMax + 63, Book.width, compact ? 42 : 34), snapshot.StoryNote, tinyStyle, Paper);
            }
            finally { GUI.matrix = oldMatrix; GUI.color = oldColor; GUI.contentColor = oldContent; GUI.depth = oldDepth; }
        }

        void DrawLeftPage(PanicNotebookPage page, PanicNotebookSnapshot data)
        {
            if (data == null) return;
            GUI.BeginGroup(new Rect(0, 0, PageWidth, Book.height));
            Label(new Rect(28, 25, PageWidth - 56, 43), PanicNotebookData.PageName(page).ToUpperInvariant(), titleStyle, Ink);
            string sub = page == PanicNotebookPage.Rooms ? "GERÇEK DEPO NOKTALARI  ·  " + PanicNotebookData.FloorName(floor) :
                page == PanicNotebookPage.Inventory ? "ENVANTER VE AFET ÇANTASI" : "OYUNDA TAMAMLANAN ÖNLEMLER";
            Label(new Rect(28, 72, PageWidth - 56, 25), sub, tinyStyle, Green);
            Fill(new Rect(28, 97, PageWidth - 56, 2), Green);
            if (page == PanicNotebookPage.Rooms) DrawPlan(data);
            else if (page == PanicNotebookPage.Inventory) DrawInventory(data);
            else DrawChecks(page, data);
            Label(new Rect(28, Book.height - 30, PageWidth - 56, 22), "KAYIT: CANLI OYUN     ·     S. " + ((int)page + 1).ToString("00"), tinyStyle, Green);
            GUI.EndGroup();
        }
        void DrawChecks(PanicNotebookPage page, PanicNotebookSnapshot data)
        {
            Rect art = new Rect(28, 112, PageWidth - 56, compact ? 155 : 108);
            Frame(art, new Color(.89f, .93f, .87f, 1), Rule); DrawHazard(art, page);
            var rows = data.Pages[(int)page];
            float y = art.yMax + 13, rowH = compact ? 77 : 57;
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i]; var rect = new Rect(28, y + i * rowH, PageWidth - 56, rowH - 6);
                Frame(rect, row.Complete ? new Color(.88f, .92f, .85f, 1) : Color.white, Rule);
                Frame(new Rect(rect.x + 11, rect.y + 12, 23, 23), row.Complete ? Green : Paper, Ink);
                if (row.Complete) Check(new Rect(rect.x + 16, rect.y + 15, 14, 15), Paper);
                Label(new Rect(rect.x + 45, rect.y + 5, rect.width - 123, 28), row.Label, smallStyle, Ink);
                Label(new Rect(rect.x + 45, rect.y + 30, rect.width - 63, rowH - 34), row.Detail, tinyStyle, Green);
                Label(new Rect(rect.xMax - 72, rect.y + 10, 62, 29), row.Total > 0 ? row.Done + "/" + row.Total : "—", centerStyle, row.Complete ? Green : Ink);
            }
            Label(new Rect(28, Book.height - 62, PageWidth - 56, 31), "Tikler gerçek iş tamamlanınca güncellenir.", tinyStyle, Ink);
        }
        void DrawPlan(PanicNotebookSnapshot data)
        {
            for (int i = 0; i < 3; i++) DrawButton(FloorRect(i), PanicNotebookData.FloorName(i - 1), floor == i - 1 ? Ink : PaperBack, floor == i - 1 ? Paper : Ink, true);
            Rect map = PlanRect; Frame(map, Color.white, Ink);
            // Coordinates are a schematic projection of actual source positions, including attached garage.
            Line(MapPoint(new Vector3(-8, 0, -6)), MapPoint(new Vector3(8, 0, -6)), Ink, 2);
            Line(MapPoint(new Vector3(8, 0, -6)), MapPoint(new Vector3(8, 0, 6)), Ink, 2);
            Line(MapPoint(new Vector3(8, 0, 6)), MapPoint(new Vector3(-8, 0, 6)), Ink, 2);
            Line(MapPoint(new Vector3(-8, 0, 6)), MapPoint(new Vector3(-8, 0, -6)), Ink, 2);
            if (floor == 0) { var garage = MapRect(new Vector3(8, 0, -1), new Vector3(17, 0, 6)); Frame(garage, new Color(.91f, .94f, .90f), Rule); }
            Label(new Rect(map.x + 8, map.y + 6, map.width - 16, 26), "K", tinyStyle, Green);
            for (int i = 0; i < data.Pins.Length; i++)
            {
                var pin = data.Pins[i]; Rect rect = PinRect(pin);
                DrawButton(rect, (i + 1).ToString(), pin.Id == selectedSource ? Yellow : pin.Empty ? Olive : Ink, pin.Id == selectedSource || pin.Empty ? Ink : Paper, true);
            }
            int actorFloor = Mathf.Clamp(Mathf.FloorToInt((data.ActorPosition.y + .65f) / 3.2f), -1, 1);
            if (actorFloor == floor) { Vector2 p = MapPoint(data.ActorPosition); Frame(new Rect(p.x - 6, p.y - 6, 12, 12), Alarm, Paper); }
            int empty = 0;
            for (int i = 0; i < data.Pins.Length; i++)
            {
                var pin = data.Pins[i]; if (pin.Empty) empty++;
                if (i < sourceOffset || i >= sourceOffset + VisibleSourceRows) continue;
                string state = pin.Empty ? "BOŞ" : pin.Open ? "AÇIK" : "KAPALI";
                Rect row = SourceRow(i - sourceOffset);
                Frame(row, pin.Id == selectedSource ? new Color(.96f, .93f, .72f) : PaperBack, Rule);
                Label(new Rect(row.x + 9, row.y + 7, row.width - 105, 27), (i + 1) + "  " + pin.Label, smallStyle, Ink);
                Label(new Rect(row.xMax - 91, row.y + 8, 82, 25), state, tinyStyle, pin.Empty ? Green : Ink);
            }
            Label(new Rect(28, Book.height - 132, PageWidth - 56, 36), empty + " / " + data.Pins.Length + " kaynak boş. Kırmızı işaret sensin." + (data.Pins.Length > VisibleSourceRows ? " Liste: tekerlek." : ""), tinyStyle, Green);
            DrawButton(InventoryRect, string.IsNullOrEmpty(selectedSource) ? "PLAN ÜZERİNDEN BİR HEDEF SEÇ" : "SEÇİLİ HEDEFİ İŞARETLE", string.IsNullOrEmpty(selectedSource) ? PaperBack : Green, string.IsNullOrEmpty(selectedSource) ? Ink : Paper, true);
        }
        Vector2 MapPoint(Vector3 position)
        {
            var map = PlanRect; return new Vector2(map.x + 18 + (position.x + 9) / 27 * (map.width - 36), map.y + 25 + (7 - position.z) / 15 * (map.height - 45));
        }
        Rect MapRect(Vector3 a, Vector3 b) { Vector2 aa = MapPoint(a), bb = MapPoint(b); return Rect.MinMaxRect(Mathf.Min(aa.x, bb.x), Mathf.Min(aa.y, bb.y), Mathf.Max(aa.x, bb.x), Mathf.Max(aa.y, bb.y)); }
        Rect PinRect(PanicNotebookPin pin) { Vector2 p = MapPoint(pin.Position); return new Rect(p.x - 15, p.y - 15, 30, 30); }

        void DrawInventory(PanicNotebookSnapshot data)
        {
            float slot = Mathf.Min(compact ? 83 : 66, (PageWidth - 56 - 5 * 7) / 6);
            Label(new Rect(28, 111, PageWidth - 56, 27), "ENVANTER   " + data.InventoryUsed + "/" + data.InventoryCapacity, smallStyle, Ink);
            DrawSlots(data.Inventory, 28, 147, slot, 6, false);
            float bagY = 147 + 2 * (slot + 8) + 15;
            Label(new Rect(28, bagY, PageWidth - 56, 28), "AFET ÇANTASI   " + data.BagUsed + "/" + data.BagCapacity, smallStyle, Ink);
            DrawSlots(data.Bag, 28, bagY + 38, slot, 4, true);
            float bottom = bagY + 38 + 2 * (slot + 8) + 12;
            Label(new Rect(28, bottom, PageWidth - 56, 35), "SU  " + data.PackedWater + "/" + data.RequiredDays + "     GIDA  " + data.PackedFood + "/" + data.RequiredDays + (data.BagReady ? "    HAZIR" : "    EKSİK"), smallStyle, data.BagReady ? Green : Alarm);
            if (compact) DrawMaterialRows(data, new Rect(28, bottom + 36, PageWidth - 56, 112));
            DrawButton(InventoryRect, "ÇANTAYI DÜZENLE  ·  TAB", Green, Paper, true);
        }
        void DrawSlots(PanicNotebookSlot[] slots, float x, float y, float size, int columns, bool bag)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                Rect rect = new Rect(x + i % columns * (size + 7), y + i / columns * (size + 8), size, size);
                var item = slots[i]; bool has = item.Count > 0 && !string.IsNullOrEmpty(item.Id);
                Frame(rect, has ? Color.white : PaperBack, has ? Ink : Rule);
                Label(new Rect(rect.x + 4, rect.y + 2, 26, 19), (i + 1).ToString("00"), tinyStyle, Green);
                if (has)
                {
                    var icon = PanicResidenceArt.Icon(item.Id);
                    if (icon) GUI.DrawTexture(new Rect(rect.x + 12, rect.y + 9, size - 24, size - 35), icon, ScaleMode.ScaleToFit, true);
                    Label(new Rect(rect.x + 3, rect.yMax - 27, size - 6, 24), ShortName(item.Id), tinyStyle, Ink);
                    Label(new Rect(rect.xMax - 29, rect.y + 1, 26, 24), item.Count.ToString(), centerStyle, Ink);
                }
                else if (bag) Line(new Vector2(rect.x + 10, rect.yMax - 11), new Vector2(rect.xMax - 10, rect.yMax - 11), Rule, 2);
            }
        }
        static string ShortName(string id)
        { switch (id) { case "firstaid": return "İlk yardım"; case "flashlight": return "Fener"; case "extinguisher": return "Söndürücü"; case "seedling": return "Ağaç sırası"; case "sandbag": return "Kum torbası"; case "bracket": return "Braket"; default: return PanicExpedition.DisplayName(id); } }
        void DrawRightPage(PanicNotebookSnapshot data)
        {
            if (data == null) return;
            float x = PageWidth + 28, width = PageWidth - 56;
            Label(new Rect(x, 25, width, 43), data.StoryTitle.ToUpperInvariant(), titleStyle, Ink);
            Label(new Rect(x, 74, width, 24), "DALGA " + data.Wave + "  ·  " + data.Disaster + "  ·  " + data.Phase, tinyStyle, Green);
            Fill(new Rect(x, 101, width, 2), Green);
            Stat(new Rect(x, 118, width, 97), "HASAR", "%" + data.Damage.ToString("0"), Alarm);
            Fill(new Rect(x + 14, 201, width - 28, 5), Rule); Fill(new Rect(x + 14, 201, (width - 28) * data.Damage / 100, 5), Alarm);
            float half = (width - 12) / 2;
            Stat(new Rect(x, 228, half, 99), "BÜTÇE", data.Budget, Ink);
            Stat(new Rect(x + half + 12, 228, half, 99), "ÇANTA YUVALARI", data.BagUsed + " / " + data.BagCapacity, Ink);
            Label(new Rect(x, 340, width, 27), "MALZEME KAYDI", smallStyle, Green);
            DrawMaterialRows(data, new Rect(x, 374, width, 111));
            Rect note = new Rect(x, 493, width, 109); Fill(new Rect(note.x + 3, note.y + 4, note.width, note.height), new Color(.15f, .18f, .14f, .1f)); Fill(note, new Color(.96f, .92f, .66f));
            bool selectedWork = !string.IsNullOrEmpty(data.SelectedAction);
            Label(new Rect(x + 12, note.y + 9, width - 24, 24), selectedWork ? "SEÇİLİ HEDEF: " + data.SelectedLabel : "BU DALGANIN NOTU", tinyStyle, Green);
            string noteText = data.Note;
            if (selectedWork)
            {
                noteText = (data.SelectedAction == "upgrade" ? "Güçlendirme" : "Sabitleme") + " bütçesi: " + data.SelectedBudgetCost;
                for (int i = 0; i < data.SelectedRequirements.Length; i++)
                {
                    var requirement = data.SelectedRequirements[i];
                    noteText += "\n" + ShortName(requirement.Id) + ": " + requirement.Owned + "/" + requirement.Required + "  ·  Eksik " + requirement.Missing;
                }
            }
            Label(new Rect(x + 12, note.y + 34, width - 24, 66), noteText, selectedWork ? tinyStyle : smallStyle, Ink);
            Label(new Rect(x, Book.height - 29, width, 22), data.CheatedRun ? "SKOR DIŞI KOŞU" : "DEFTER AÇIK  ·  SÜRE DURDU", tinyStyle, Green);
        }
        void DrawMaterialRows(PanicNotebookSnapshot data, Rect rect)
        {
            for (int i = 0; i < data.Materials.Length; i++)
            {
                float width = rect.width * .5f, y = rect.y + i / 2 * 27;
                string text = ShortName(PanicNotebookData.MaterialIds[i]) + "  " + data.Materials[i];
                Label(new Rect(rect.x + i % 2 * width, y, width - 7, 26), text, tinyStyle, Ink);
            }
        }
        void Stat(Rect rect, string label, string value, Color color)
        {
            Frame(rect, new Color(.93f, .95f, .91f), Rule);
            Label(new Rect(rect.x + 14, rect.y + 10, rect.width - 28, 24), label, tinyStyle, Ink);
            Label(new Rect(rect.x + 14, rect.y + 39, rect.width - 28, 53), value, valueStyle, color);
        }
        void DrawTabs()
        {
            for (int i = 0; i < 6; i++)
            {
                var page = (PanicNotebookPage)i; Rect rect = TabRect(i); bool chosen = animationState.RequestedPage == page;
                Frame(rect, chosen ? Yellow : PaperBack, Ink);
                DrawHazard(new Rect(rect.x + 23, rect.y + 7, rect.width - 46, rect.height - 45), page);
                Label(new Rect(rect.x + 4, rect.yMax - 32, rect.width - 8, 27), PanicNotebookData.PageName(page), centerStyle, Ink);
            }
        }

        void DrawTurningLeaf()
        {
            float height = Book.height, width = PageWidth;
            PanicNotebookPaper.Sample(animationState.TurnProgress, height, strips);
            float step = height / strips.Length;
            Matrix4x4 bookMatrix = GUI.matrix;
            var data = turningSnapshot ?? snapshot;
            // Painter order follows depth rather than strip index, including the curled reversal.
            for (int i = 0; i < stripOrder.Length; i++)
            {
                int at = i;
                while (at > 0 && strips[stripOrder[at - 1]].Z > strips[i].Z)
                { stripOrder[at] = stripOrder[at - 1]; at--; }
                stripOrder[at] = i;
            }
            for (int ordered = 0; ordered < strips.Length; ordered++)
            {
                int i = stripOrder[ordered];
                float start, end;
                if (PanicNotebookPaper.HiddenInterval(strips[i], step + .5f, out start, out end))
                {
                    if (start > .0001f) DrawStripPart(i, 0, Mathf.Min(1, start), step, width, bookMatrix, data);
                    if (end < .9999f) DrawStripPart(i, Mathf.Max(0, end), 1, step, width, bookMatrix, data);
                }
                else DrawStripPart(i, 0, 1, step, width, bookMatrix, data);
            }
            GUI.matrix = bookMatrix;
        }
        void DrawStripPart(int index, float begin, float end, float step, float width, Matrix4x4 bookMatrix, PanicNotebookSnapshot data)
        {
            if (end <= begin) return;
            var point = strips[index]; float angle = point.Angle * Mathf.Deg2Rad;
            float y1 = point.Y + step * begin * Mathf.Cos(angle), z1 = point.Z + step * begin * Mathf.Sin(angle);
            float y2 = point.Y + step * end * Mathf.Cos(angle), z2 = point.Z + step * end * Mathf.Sin(angle);
            float projected1 = PanicNotebookPaper.Project(y1, z1), projected2 = PanicNotebookPaper.Project(y2, z2);
            float h = step * (end - begin), sy = (projected2 - projected1) / h;
            if (Mathf.Abs(sy) < .004f) return;
            float sx = PanicNotebookPaper.Project(1, (z1 + z2) * .5f);
            float top = Mathf.Min(projected1, projected2), sourceY = index * step + step * begin;
            // Every strip draws a clipped, inert copy; no controls or event handlers are cloned.
            GUI.matrix = bookMatrix * Matrix4x4.TRS(new Vector3((width - width * sx) * .5f, top, 0), Quaternion.identity, new Vector3(sx, Mathf.Abs(sy), 1));
            GUI.BeginGroup(new Rect(0, 0, width, h + .6f));
            Fill(new Rect(0, 0, width, h + .6f), sy > 0 ? Paper : PaperBack);
            if (sy > 0)
            {
                Matrix4x4 before = GUI.matrix;
                GUI.matrix = before * Matrix4x4.Translate(new Vector3(0, -sourceY, 0));
                DrawLeftPage(animationState.FromPage, data);
                GUI.matrix = before;
            }
            else for (float y = 0; y < h; y += 23) Fill(new Rect(0, y, width, 1), Rule);
            Fill(new Rect(0, 0, width, h + .6f), new Color(0, 0, 0, Mathf.Clamp01(1 - point.Light) * .65f));
            GUI.EndGroup();
        }
        void DrawSpiral()
        {
            EnsureSpiralTextureV5();
            int count = compact ? 12 : 22;
            float spacing = (Book.width - 70) / (count - 1);
            Color dark = new Color(.22f, .25f, .24f, 1);
            for (int i = 0; i < count; i++)
            {
                float x = 35 + i * spacing;
                // Recessed holes and their soft paper lip anchor every coil to the page.
                RoundPaperV5(new Rect(x - 7, 7, 14, 10), new Color(.20f, .25f, .22f, .12f), 5);
                RoundPaperV5(new Rect(x - 4, 8, 8, 6), dark, 3);
                RoundPaperV5(new Rect(x - 6, 17, 13, 3), new Color(1, 1, 1, .65f), 2);
                GUI.DrawTexture(new Rect(x - 13, -19, 30, 42), spiralTextureV5, ScaleMode.StretchToFill, true);
            }
        }
        void EnsureSpiralTextureV5()
        {
            if (spiralTextureV5) return;
            const int width = 72, height = 104;
            var pixels = new Color[width * height];
            for (int wire = 0; wire < 2; wire++)
            {
                Vector2 previous = Vector2.zero;
                for (int step = 0; step <= 24; step++)
                {
                    float angle = (-145 + step * (290f / 24)) * Mathf.Deg2Rad;
                    Vector2 point = new Vector2(29 + wire * 9 + Mathf.Cos(angle) * 15.5f, 45 + Mathf.Sin(angle) * 35);
                    if (step > 0)
                    {
                        SpiralStrokeV5(pixels, width, height, previous + new Vector2(4, 7), point + new Vector2(4, 7), 8.5f, new Color(0, 0, 0, .18f));
                        SpiralStrokeV5(pixels, width, height, previous, point, 8, new Color(.22f, .25f, .24f, 1));
                        SpiralStrokeV5(pixels, width, height, previous + new Vector2(-.7f, -.7f), point + new Vector2(-.7f, -.7f), 5.7f, new Color(.68f, .57f, .39f, 1));
                        if (step > 11 && step < 21) SpiralStrokeV5(pixels, width, height, previous + new Vector2(-1.7f, -1.7f), point + new Vector2(-1.7f, -1.7f), 1.8f, new Color(.98f, .89f, .65f, 1));
                    }
                    previous = point;
                }
            }
            spiralTextureV5 = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "V5 notebook double wire binding", hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            spiralTextureV5.SetPixels(pixels); spiralTextureV5.Apply(false, true);
        }
        static void SpiralStrokeV5(Color[] pixels, int width, int height, Vector2 a, Vector2 b, float thickness, Color color)
        {
            float radius = thickness * .5f;
            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, b.x) - radius - 1));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(a.x, b.x) + radius + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, b.y) - radius - 1));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(a.y, b.y) + radius + 1));
            Vector2 line = b - a;
            for (int y = minY; y <= maxY; y++) for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new Vector2(x + .5f, y + .5f);
                float t = Mathf.Clamp01(Vector2.Dot(p - a, line) / Mathf.Max(.0001f, line.sqrMagnitude));
                float alpha = color.a * Mathf.Clamp01(radius + .6f - Vector2.Distance(p, a + line * t));
                if (alpha <= 0) continue;
                int index = (height - y - 1) * width + x;
                Color old = pixels[index]; float combined = alpha + old.a * (1 - alpha);
                pixels[index] = new Color((color.r * alpha + old.r * old.a * (1 - alpha)) / combined, (color.g * alpha + old.g * old.a * (1 - alpha)) / combined, (color.b * alpha + old.b * old.a * (1 - alpha)) / combined, combined);
            }
        }
        static void RoundPaperV5(Rect rect, Color color, float radius)
        {
            Color previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color * previous, 0, radius);
            GUI.color = previous;
        }

        void DrawBookCoverV5()
        {
            RoundPaperV5(new Rect(-5, 7, Book.width + 26, Book.height + 25), new Color(0, 0, 0, .22f), 12);
            RoundPaperV5(new Rect(-13, -13, Book.width + 29, Book.height + 29), Green, 12);
            RoundPaperV5(new Rect(-9, -9, Book.width + 21, Book.height + 21), new Color(.34f, .49f, .42f, 1), 10);
            // Short cloth stitches and a warm fabric bookmark give the cover a kept-at-home feel.
            for (float x = 9; x < Book.width - 8; x += 16)
                Fill(new Rect(x, Book.height + 8, 7, 1), new Color(.86f, .87f, .71f, .55f));
            Fill(new Rect(Book.width - 68, Book.height - 3, 24, 33), new Color(.78f, .47f, .31f, 1));
            Line(new Vector2(Book.width - 67, Book.height + 30), new Vector2(Book.width - 56, Book.height + 22), Green, 2);
            Line(new Vector2(Book.width - 56, Book.height + 22), new Vector2(Book.width - 44, Book.height + 30), Green, 2);
        }

        void DrawHazard(Rect rect, PanicNotebookPage page)
        {
            Vector2 c = rect.center; float s = Mathf.Min(rect.width, rect.height) * .8f;
            Vector2 a = new Vector2(c.x - s * .35f, c.y + s * .25f), b = new Vector2(c.x + s * .35f, c.y + s * .25f);
            if (page == PanicNotebookPage.Inventory)
            {
                Frame(new Rect(c.x - s * .30f, c.y - s * .25f, s * .60f, s * .63f), new Color(1, 1, 1, 0), Ink);
                Line(new Vector2(c.x - s * .15f, c.y - s * .25f), new Vector2(c.x - s * .15f, c.y - s * .40f), Ink, 2);
                Line(new Vector2(c.x - s * .15f, c.y - s * .40f), new Vector2(c.x + s * .15f, c.y - s * .40f), Ink, 2);
                Line(new Vector2(c.x + s * .15f, c.y - s * .40f), new Vector2(c.x + s * .15f, c.y - s * .25f), Ink, 2);
                Line(new Vector2(c.x - s * .3f, c.y + s * .05f), new Vector2(c.x + s * .3f, c.y + s * .05f), Ink, 2); return;
            }
            if (page == PanicNotebookPage.Fire)
            {
                Vector2[] flame = { new Vector2(c.x, c.y - s * .43f), new Vector2(c.x + s * .07f, c.y - s * .08f), new Vector2(c.x + s * .23f, c.y - s * .19f), new Vector2(c.x + s * .29f, c.y + s * .17f), new Vector2(c.x + s * .12f, c.y + s * .38f), new Vector2(c.x - s * .19f, c.y + s * .34f), new Vector2(c.x - s * .31f, c.y + s * .07f), new Vector2(c.x, c.y - s * .43f) };
                for (int i = 1; i < flame.Length; i++) Line(flame[i - 1], flame[i], Ink, 2.5f); return;
            }
            if (page == PanicNotebookPage.Storm)
            {
                for (int i = 0; i < 5; i++) { float w = s * (.8f - .13f * i), y = c.y - s * .35f + s * i * .17f; Line(new Vector2(c.x - w / 2, y), new Vector2(c.x + w / 2, y + s * .04f), Ink, 2.5f); } return;
            }
            Line(a, new Vector2(a.x, c.y - s * .13f), Ink, 2); Line(new Vector2(a.x, c.y - s * .13f), new Vector2(c.x, c.y - s * .42f), Ink, 2);
            Line(new Vector2(c.x, c.y - s * .42f), new Vector2(b.x, c.y - s * .13f), Ink, 2); Line(new Vector2(b.x, c.y - s * .13f), b, Ink, 2); Line(a, b, Ink, 2);
            if (page == PanicNotebookPage.Flood)
                for (int i = 0; i < 8; i++) { float x = c.x - s * .47f + i * s * .12f; Line(new Vector2(x, c.y + s * .35f), new Vector2(x + s * .06f, c.y + s * (.35f + (i % 2 == 0 ? .05f : -.05f))), Ink, 2); }
            else if (page == PanicNotebookPage.Earthquake)
            { Line(new Vector2(c.x, c.y - s * .3f), new Vector2(c.x - s * .1f, c.y), Ink, 3); Line(new Vector2(c.x - s * .1f, c.y), new Vector2(c.x + s * .07f, c.y + s * .1f), Ink, 3); Line(new Vector2(c.x + s * .07f, c.y + s * .1f), new Vector2(c.x, c.y + s * .3f), Ink, 3); }
        }
        void EnsureStyles()
        {
            if (titleStyle != null) return;
            Font body = Resources.Load<Font>("HUD/Barlow"), heading = Resources.Load<Font>("HUD/BarlowCondensed");
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!body) body = fallback; if (!heading) heading = body;
            titleStyle = Style(heading, 37, FontStyle.Bold); headingStyle = Style(heading, 30, FontStyle.Bold);
            bodyStyle = Style(body, 24); smallStyle = Style(body, 21); tinyStyle = Style(body, 17);
            centerStyle = Style(heading, 21, FontStyle.Bold); centerStyle.alignment = TextAnchor.MiddleCenter;
            valueStyle = Style(heading, 40, FontStyle.Bold);
        }
        static GUIStyle Style(Font font, int size, FontStyle weight = FontStyle.Normal)
        {
            var style = new GUIStyle { font = font, fontSize = size, fontStyle = weight, wordWrap = true, clipping = TextClipping.Clip, alignment = TextAnchor.UpperLeft, padding = new RectOffset() };
            // GUI.contentColor multiplies this base color; a default black base erases requested light ink.
            style.normal.textColor = Color.white;
            return style;
        }
        static void Label(Rect rect, string text, GUIStyle style, Color color)
        { Color old = GUI.contentColor; GUI.contentColor = color; GUI.Label(rect, text ?? string.Empty, style); GUI.contentColor = old; }
        static void Fill(Rect rect, Color color)
        { Color old = GUI.color; GUI.color = old * color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        static void Frame(Rect rect, Color fill, Color edge)
        { Fill(rect, fill); Fill(new Rect(rect.x, rect.y, rect.width, 1), edge); Fill(new Rect(rect.x, rect.yMax - 1, rect.width, 1), edge); Fill(new Rect(rect.x, rect.y, 1, rect.height), edge); Fill(new Rect(rect.xMax - 1, rect.y, 1, rect.height), edge); }
        void DrawButton(Rect rect, string text, Color fill, Color ink, bool enabled)
        { Frame(rect, fill, Ink); Label(new Rect(rect.x + 4, rect.y + 3, rect.width - 8, rect.height - 6), text, centerStyle, enabled ? ink : Green); }
        static void Line(Vector2 a, Vector2 b, Color color, float width)
        {
            Vector2 d = b - a;
            if (width <= 0 || float.IsNaN(width) || float.IsInfinity(width) ||
                float.IsNaN(d.x) || float.IsNaN(d.y) || float.IsInfinity(d.x) || float.IsInfinity(d.y)) return;
            float length = d.magnitude;
            if (length < .01f || float.IsInfinity(length)) return;
            // Keep every fill in the caller's local group. No pivot/matrix mutation can escape
            // a nested page/strip clip. Axis-aligned strokes need only one rectangle.
            if (Mathf.Abs(d.x) < .001f)
            { Fill(new Rect(a.x - width * .5f, Mathf.Min(a.y, b.y), width, Mathf.Abs(d.y)), color); return; }
            if (Mathf.Abs(d.y) < .001f)
            { Fill(new Rect(Mathf.Min(a.x, b.x), a.y - width * .5f, Mathf.Abs(d.x), width), color); return; }
            float span = Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.y));
            float spacing = Mathf.Max(.75f, width * .5f);
            int count = Mathf.Clamp(Mathf.CeilToInt(Mathf.Min(512, span / spacing)), 1, 512);
            float padX = Mathf.Abs(d.y / length) * width * .5f, padY = Mathf.Abs(d.x / length) * width * .5f;
            Vector2 previous = a;
            for (int i = 1; i <= count; i++)
            {
                Vector2 next = a + d * ((float)i / count);
                Fill(Rect.MinMaxRect(Mathf.Min(previous.x, next.x) - padX, Mathf.Min(previous.y, next.y) - padY,
                    Mathf.Max(previous.x, next.x) + padX, Mathf.Max(previous.y, next.y) + padY), color);
                previous = next;
            }
        }
        static void Check(Rect rect, Color color)
        { Line(new Vector2(rect.x, rect.y + rect.height * .5f), new Vector2(rect.x + rect.width * .4f, rect.yMax), color, 3); Line(new Vector2(rect.x + rect.width * .4f, rect.yMax), new Vector2(rect.xMax, rect.y), color, 3); }
        static void PaperGrid(Rect rect)
        {
            // Fine ruled stock with a faint red margin, sparse fibres and a bound top shadow.
            for (float y = rect.y + 28; y < rect.yMax; y += 23)
                Fill(new Rect(rect.x + 12, y, rect.width - 24, 1), new Color(.35f, .48f, .48f, .075f));
            Fill(new Rect(rect.x + 20, rect.y + 18, 1, rect.height - 36), new Color(.78f, .37f, .34f, .18f));
            for (int i = 0; i < 64; i++)
            {
                float x = rect.x + 12 + ((i * 137) % 997) / 997f * (rect.width - 24);
                float y = rect.y + 22 + ((i * 193) % 991) / 991f * (rect.height - 40);
                Fill(new Rect(x, y, i % 3 == 0 ? 2 : 1, 1), new Color(.43f, .35f, .21f, .06f));
            }
            for (int i = 0; i < 8; i++)
                Fill(new Rect(rect.x, rect.y + i * 2, rect.width, 2), new Color(.28f, .22f, .13f, .025f * (1 - i / 8f)));
        }
    }
}
