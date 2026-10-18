using UnityEngine;

namespace TooLateToPanic
{
    // Shared manual-pin stage/modal eligibility. The former screen locator is removed.
    public static class PanicNotebookCursorPolicyV59
    {
        public static bool CanShow(PanicPhase phase, bool manualTarget, bool blocked)
        { return manualTarget && !blocked && (phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial || phase == PanicPhase.Inspection); }
        public static bool TargetVisibleInStage(bool isSource, bool isSupply, bool bagStage)
        { return isSource && !bagStage || isSupply && bagStage; }
        public static Rect CaptionClearance { get { return new Rect(440f, 116f, 690f, 94f); } }
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

    public sealed partial class PanicHomeHud
    {
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

        // No card, brackets, leader, arrow, distance or other-floor locator is drawn.
        // The local actual-mesh renderer consumes the shared eligibility above.
        public void DrawNotebookCursorV59() { }
    }
}
