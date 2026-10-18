using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    [Serializable]
    public struct PanicAutoFocusCandidateV510
    {
        public string Id;
        public float Distance, FacingDot;
        public bool Eligible;
        public PanicAutoFocusCandidateV510(string id, float distance, float facingDot, bool eligible)
        { Id = id; Distance = distance; FacingDot = facingDot; Eligible = eligible; }
    }

    // Pure ranking/latch policy. Eligibility is supplied by the existing physical world queries.
    public static class PanicAutoFocusPolicyV510
    {
        public const float MinimumFacingDot = .15f;
        public const float ManualMoveMeters = .06f;
        public const float ManualTurnDot = .99756405f; // cos(4 degrees)
        public static string ChooseNearestFacing(IList<PanicAutoFocusCandidateV510> candidates)
        {
            string best = null; float distance = float.MaxValue, facing = -1f;
            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (!candidate.Eligible || string.IsNullOrEmpty(candidate.Id) || !Finite(candidate.Distance) ||
                    !Finite(candidate.FacingDot) || candidate.Distance < 0 || candidate.FacingDot < MinimumFacingDot) continue;
                // True nearest reachable surface first; alignment breaks equal-distance ties, then ordinal ID.
                if (candidate.Distance < distance || candidate.Distance == distance &&
                    (candidate.FacingDot > facing || candidate.FacingDot == facing && string.CompareOrdinal(candidate.Id, best) < 0))
                { best = candidate.Id; distance = candidate.Distance; facing = candidate.FacingDot; }
            }
            return best;
        }
        public static bool KeepManual(bool valid, bool sameContext, bool targetEligible,
            float displacementSquared, float characterFacingDot, float cameraFacingDot)
        {
            return valid && sameContext && targetEligible && Finite(displacementSquared) &&
                Finite(characterFacingDot) && Finite(cameraFacingDot) &&
                displacementSquared < ManualMoveMeters * ManualMoveMeters &&
                characterFacingDot >= ManualTurnDot && cameraFacingDot >= ManualTurnDot;
        }
        static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    [Serializable]
    public sealed class PanicAutoFocusProbeV510
    {
        public string FocusId, AutomaticCandidateId, Reason, Phase;
        public bool InputAllowed, ManualSelectionHeld, FirstPerson;
        public int Floor, Wave, Frame;
        public uint ContextSignature;
        public Vector3 PlayerPosition, CharacterForward;
        public PanicAutoFocusCandidateV510[] Candidates;
    }

    public sealed partial class PanicHomeWorld
    {
        readonly List<PanicAutoFocusCandidateV510> autoCandidatesV510 = new List<PanicAutoFocusCandidateV510>();
        string manualFocusV510, observedOpenSourceV510;
        Vector3 manualPositionV510, manualFacingV510, manualViewV510;
        uint manualContextV510, autoContextV510;
        bool manualValidV510, settingAutomaticFocusV510;
        int lastAutoFocusFrameV510 = -1;
        PanicAutoFocusProbeV510 autoFocusProbeV510;

        public PanicAutoFocusProbeV510 ProbeAutoFocusV510()
        {
            // Returned data never drives gameplay; callers cannot alter the internal candidate list.
            if (autoFocusProbeV510 == null) autoFocusProbeV510 = MakeAutoFocusProbeV510(false, false, null, "not-refreshed");
            return new PanicAutoFocusProbeV510 {
                FocusId = autoFocusProbeV510.FocusId, AutomaticCandidateId = autoFocusProbeV510.AutomaticCandidateId,
                Reason = autoFocusProbeV510.Reason, Phase = autoFocusProbeV510.Phase,
                InputAllowed = autoFocusProbeV510.InputAllowed, ManualSelectionHeld = autoFocusProbeV510.ManualSelectionHeld,
                FirstPerson = autoFocusProbeV510.FirstPerson, Floor = autoFocusProbeV510.Floor, Wave = autoFocusProbeV510.Wave,
                Frame = autoFocusProbeV510.Frame, ContextSignature = autoFocusProbeV510.ContextSignature,
                PlayerPosition = autoFocusProbeV510.PlayerPosition, CharacterForward = autoFocusProbeV510.CharacterForward,
                Candidates = autoCandidatesV510.ToArray() };
        }

        // Same selector used by RefreshFocus. Explicit semantic API for root-owned native fixtures.
        // The caller must unlock gameplay/close modals; this API never bypasses those gates.
        public PanicAutoFocusProbeV510 RefreshAutoFocusV510(bool cycle)
        { RefreshAutomaticFocusCoreV510(cycle); return ProbeAutoFocusV510(); }

        // Production hook uses the void core, avoiding diagnostic object/array allocations each frame.
        void RefreshAutomaticFocusCoreV510(bool cycle)
        {
            if (!AutoFocusInputAllowedV510())
            {
                manualValidV510 = false; lastAutoFocusFrameV510 = -1;
                autoFocusProbeV510 = MakeAutoFocusProbeV510(false, false, null, "input-owner");
                return;
            }
            if (BagFirstPersonActive)
            {
                manualValidV510 = false; observedOpenSourceV510 = null;
                autoCandidatesV510.Clear();
                RefreshBagRayFocus(); // Existing eye ray remains the sole FPS focus/occlusion authority.
                autoFocusProbeV510 = MakeAutoFocusProbeV510(true, false, focusId, "fps-center-ray");
                return;
            }
            // Work, inventory and modal owners skip the production tick. A gap releases old manual intent.
            if (lastAutoFocusFrameV510 >= 0 && Time.frameCount > lastAutoFocusFrameV510 + 1) manualValidV510 = false;
            lastAutoFocusFrameV510 = Time.frameCount;
            BuildAutoFocusCandidatesV510();
            string automatic = PanicAutoFocusPolicyV510.ChooseNearestFacing(autoCandidatesV510);

            // An opened source owns E/F/Z/X until the actor moves/looks away or its context changes.
            var open = OpenSource; string openId = open != null ? open.Id : null;
            if (openId != observedOpenSourceV510)
            {
                observedOpenSourceV510 = openId;
                if (openId != null) CaptureManualFocusV510();
            }
            if (cycle)
            {
                int current = -1, eligibleCount = 0;
                for (int i = 0; i < autoCandidatesV510.Count; i++)
                    if (autoCandidatesV510[i].Eligible) { if (autoCandidatesV510[i].Id == focusId) current = eligibleCount; eligibleCount++; }
                if (eligibleCount > 0)
                {
                    int wanted = (current + 1) % eligibleCount, seen = 0;
                    for (int i = 0; i < autoCandidatesV510.Count; i++) if (autoCandidatesV510[i].Eligible && seen++ == wanted)
                    { SetAutomaticFocusV510(autoCandidatesV510[i].Id); CaptureManualFocusV510(); break; }
                }
                else { manualValidV510 = false; SetAutomaticFocusV510(null); }
                autoFocusProbeV510 = MakeAutoFocusProbeV510(true, manualValidV510, automatic, "manual-cycle");
                return;
            }
            bool eligible = false;
            for (int i = 0; i < autoCandidatesV510.Count; i++)
                if (autoCandidatesV510[i].Id == manualFocusV510 && autoCandidatesV510[i].Eligible) { eligible = true; break; }
            bool hold = focusId == manualFocusV510 && PanicAutoFocusPolicyV510.KeepManual(manualValidV510,
                manualContextV510 == autoContextV510, eligible, (player.position - manualPositionV510).sqrMagnitude,
                Vector3.Dot(CharacterForwardV510(), manualFacingV510), Vector3.Dot(ViewForwardV510(), manualViewV510));
            if (hold) UpdateContext();
            else { manualValidV510 = false; SetAutomaticFocusV510(automatic); }
            autoFocusProbeV510 = MakeAutoFocusProbeV510(true, hold, automatic, hold ? "manual-held" : "nearest-facing");
        }

        bool AutoFocusInputAllowedV510()
        {
            return player && Model != null && Expedition != null &&
                (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial || Model.Phase == PanicPhase.Inspection) &&
                JuryAllowsMovement && !InputLocked && !Model.IsPaused && !Model.WaveLedgerOpenV56 && !Model.KeyboardHelpOpenV56 &&
                !NotebookBlocksInput && !BagCameraTransitionActive && !CinematicActive && !LossAnnounced &&
                !WorkActive && !InventoryOpen && !IsMoving && CancelFrame != Time.frameCount &&
                !(hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame || hud.AudioSettingsOpenV56 || hud.SimulationSelectorOpen));
        }

        void BuildAutoFocusCandidatesV510()
        {
            Physics.SyncTransforms();
            focusCandidates.Clear();
            // Roof upgrades belong to the upper-wall transaction; physical reach rules remain unchanged.
            foreach (var item in Model.Items)
                if (item.Id != "bagstation" && item.Kind != ItemKind.Roof && item.Installed && SameLevel(item.Position) &&
                    !(BagFinalStageActive && Expedition.Sources.Exists(source => source.Id == item.Id)) && InteractionDistance(item.Id) < 2.8f) focusCandidates.Add(item.Id);
            if (!BagFinalStageActive) foreach (var source in Expedition.Sources)
                if (SameLevel(source.Position) && InteractionDistance(source.Id) < 2.8f && !focusCandidates.Contains(source.Id)) focusCandidates.Add(source.Id);
            if (!BagFinalStageActive) foreach (var site in gardenSites)
                if (!site.done && InteractionDistance(site.id) < 2.8f) focusCandidates.Add(site.id);
            foreach (var pair in residenceTargets)
                if (pair.Value && SameLevel(ResidenceFocusPosition(pair.Value)) && InteractionDistance(pair.Key) < 2.8f && !focusCandidates.Contains(pair.Key)) focusCandidates.Add(pair.Key);
            if (!items.ContainsKey("bunker") && InteractionDistance("@bunker") < 2.8f) focusCandidates.Add("@bunker");
            if (BagFinalStageActive) foreach (var supply in Expedition.WorldSupplies)
                if (supply.Count > 0 && supply.Floor == ActiveFloor && InteractionDistance(supply.Id) < 2.4f) focusCandidates.Add(supply.Id);
            // Preserve familiar Q order, including its roof/wall penalties. Automatic rank uses real distance.
            focusCandidates.Sort((a, b) => { int score = FocusScore(a).CompareTo(FocusScore(b)); return score != 0 ? score : string.CompareOrdinal(a, b); });
            autoCandidatesV510.Clear();
            Vector3 start = player.position + Vector3.up * .9f, forward = CharacterForwardV510();
            uint membership = 0;
            for (int i = 0; i < focusCandidates.Count; i++)
            {
                string id = focusCandidates[i];
                bool positioned = PositionOf(id, out var point);
                bool clear = positioned && InteractionPathClear(id, point);
                Vector3 direction = InteractionSurfacePoint(id, start, out var surface) ? Flat(surface - start) : Flat(point - start);
                // A collider containing the chest has no horizontal closest-point direction; use its real bounds center.
                if (direction.sqrMagnitude < .0001f && TryGetInteractionBounds(id, out var bounds)) direction = Flat(bounds.center - start);
                float dot = direction.sqrMagnitude > .0001f ? Vector3.Dot(forward, direction.normalized) : 1f;
                autoCandidatesV510.Add(new PanicAutoFocusCandidateV510(id, InteractionDistance(id), dot, clear));
                membership ^= AutoFocusHashV510(id) * (clear ? 16777619u : 2166136261u);
            }
            // No clock or model mutation: only eligibility/context changes release the manual choice.
            unchecked
            {
                autoContextV510 = membership ^ (uint)focusCandidates.Count * 374761393u ^ (uint)ActiveFloor * 668265263u ^
                    (uint)Model.Phase * 2246822519u ^ (uint)Model.Wave * 3266489917u ^
                    (uint)Expedition.LootLayoutRevisionV5 * 1597334677u ^ (BagFinalStageActive ? 0x52414753u : 0u);
                var source = NearestSource;
                if (source != null && source.Open) autoContextV510 ^= AutoFocusHashV510(source.Id) ^ 0x4F50454Eu;
            }
        }

        static uint AutoFocusHashV510(string text)
        { unchecked { uint hash = 2166136261u; for (int i = 0; i < text.Length; i++) hash = (hash ^ text[i]) * 16777619u; return hash; } }
        Vector3 CharacterForwardV510() { return hero ? Flat(hero.forward).normalized : Quaternion.Euler(0, facingYaw, 0) * Vector3.forward; }
        Vector3 ViewForwardV510() { return View ? View.transform.forward.normalized : Vector3.forward; }
        void SetAutomaticFocusV510(string id)
        {
            if (focusId == id) { UpdateContext(); return; }
            settingAutomaticFocusV510 = true;
            try { SetFocus(id); } finally { settingAutomaticFocusV510 = false; }
        }
        // Root hook: call at the end of SetFocus. Explicit mouse/API selections share Q's latch.
        void ObserveExplicitFocusV510()
        {
            if (settingAutomaticFocusV510 || BagFirstPersonActive || !player || Model == null || Expedition == null) return;
            CaptureManualFocusV510();
        }
        void CaptureManualFocusV510()
        {
            // Q can change NearestSource/OpenSource. Sample the new context after the actual SetFocus.
            BuildAutoFocusCandidatesV510();
            manualFocusV510 = focusId; manualValidV510 = !string.IsNullOrEmpty(focusId);
            manualPositionV510 = player.position; manualFacingV510 = CharacterForwardV510(); manualViewV510 = ViewForwardV510();
            manualContextV510 = autoContextV510;
            lastAutoFocusFrameV510 = Time.frameCount;
        }
        void ResetAutoFocusV510()
        {
            manualValidV510 = settingAutomaticFocusV510 = false; manualFocusV510 = observedOpenSourceV510 = null;
            lastAutoFocusFrameV510 = -1; autoFocusProbeV510 = null; autoCandidatesV510.Clear();
        }
        PanicAutoFocusProbeV510 MakeAutoFocusProbeV510(bool allowed, bool held, string automatic, string reason)
        {
            var result = autoFocusProbeV510 ?? new PanicAutoFocusProbeV510();
            result.FocusId = focusId; result.AutomaticCandidateId = automatic; result.Reason = reason; result.InputAllowed = allowed;
            result.ManualSelectionHeld = held; result.FirstPerson = BagFirstPersonActive;
            result.Phase = Model == null ? "" : Model.Phase.ToString(); result.Floor = ActiveFloor;
            result.Wave = Model == null ? 0 : Model.Wave; result.Frame = Time.frameCount; result.ContextSignature = autoContextV510;
            result.PlayerPosition = PlayerPosition; result.CharacterForward = CharacterForwardV510();
            return result;
        }
    }
}
