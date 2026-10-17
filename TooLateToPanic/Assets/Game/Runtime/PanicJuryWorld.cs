using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Small host adapter. World/Cinema/HUD hooks are explicitly listed in the staging handoff.</summary>
    public sealed partial class PanicHomeWorld
    {
        public PanicJuryDemo JuryDemo { get; private set; }
        public PanicJurySnapshot JurySnapshot { get { return JuryDemo != null ? JuryDemo.Snapshot : null; } }
        public bool JuryActive { get { return Model != null && Model.IsJuryDemo; } }
        public bool JuryAllowsMovement { get { return !JuryActive || JuryDemo != null && JuryDemo.AllowsMovement; } }

        public void StartJuryDemo()
        {
            ResetJuryPresentation(); ConfigurePresentationAudio(true);
            CancelWork(); CancelMove();
            if (Model.Phase != PanicPhase.Title && Model.Phase != PanicPhase.GameOver) Model.ReturnToTitle();
            JuryDemo = new PanicJuryDemo(Model, Expedition);
            if (!JuryDemo.Start()) { JuryDemo = null; return; }
            ConfigureResidenceModel(); ResetResidenceLife(); ClearGarden();
            previousWave = Model.Wave; previousPhase = Model.Phase;
            focusId = null; InventoryOpen = false;
            Warp(PanicJuryDemo.SpawnPosition); SyncVisuals(true);
            if (effects) effects.OnPhase(Model.Phase);
            UpdateContext();
            RefreshPreparationVisibility();
        }

        /// <summary>Advance the short flow only while the shared pause/notebook clock gate is open.</summary>
        void TickJuryDemo(bool inputBlocked)
        {
            if (!JuryActive || JuryDemo == null || Model.IsPaused || NotebookBlocksInput) return;
            var before = JuryDemo.Stage;
            JuryDemo.Tick(Time.unscaledDeltaTime, PlayerPosition, controller && controller.isGrounded, inputBlocked);
            if (before != JuryDemo.Stage) RefreshJuryPresentation();
        }

        void RefreshJuryPresentation()
        {
            CancelWork(); CancelMove(); InventoryOpen = false;
            ResetResidenceCinematic(); // This demo never teleports its actor into the campaign bunker.
            SyncVisuals(true); UpdateContext();
        }

        /// <summary>Call at the very beginning of Interact, before the ordinary focus/CanInteract gate.</summary>
        bool TryJuryInteract()
        {
            if (!JuryActive || JuryDemo == null || !JuryDemo.IsRunning || WorkActive || IsMoving) return false;
            var snapshot = JuryDemo.Snapshot;
            if (snapshot.ShelterKind == PanicJuryShelter.None) return false;
            Vector3 delta = PlayerPosition - snapshot.TargetPosition;
            if (new Vector2(delta.x, delta.z).magnitude > 1.5f || Mathf.Abs(delta.y) > .6f) return false;
            if (!InteractionPathClear(snapshot.TargetId, snapshot.TargetPosition))
            {
                Model.Notify("İşaretli alana açık yoldan yaklaş."); return true;
            }
            bool reached = JuryDemo.TryReachShelter(snapshot.ShelterKind, PlayerPosition, controller && controller.isGrounded);
            if (reached) { PlayPresentationCue("objective_tick"); RefreshJuryPresentation(); }
            else Model.Notify("Güvenli noktada dur ve E'ye bas.");
            return true;
        }

        public void AdvanceJuryDemo()
        {
            if (JuryActive && JuryDemo != null && JuryDemo.Advance()) RefreshJuryPresentation();
        }

        public void SkipJurySegment()
        {
            if (JuryActive && JuryDemo != null && JuryDemo.SkipCurrentSegment()) RefreshJuryPresentation();
        }

        // StartGame/ReturnToMenu own normal model/expedition resets. Drop this controller
        // before those existing methods run; do not reset the model a second time here.
        void ReleaseJuryController() { JuryDemo = null; }
    }
}
