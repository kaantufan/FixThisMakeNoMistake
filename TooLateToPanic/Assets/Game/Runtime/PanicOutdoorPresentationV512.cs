using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        // Short flow Flood/Fire can be playable while Model.Phase is Preparing.
        // Its actual stage remains a hazard until its real outcome has been committed.
        bool ShortFlowHazardV512
        {
            get { return JuryActive && JuryDemo != null &&
                (JuryDemo.Stage == PanicJuryStage.Earthquake || JuryDemo.Stage == PanicJuryStage.Flood || JuryDemo.Stage == PanicJuryStage.Fire); }
        }
        public bool OutdoorDisasterActiveV512
        { get { return Model != null && PanicOutdoorSafetyV512.HazardActive(Model.Phase, ShortFlowHazardV512); } }
        public bool RecoveryResponseAllowedV512
        { get { return Model != null && PanicOutdoorSafetyV512.RecoveryAllowed(Model.Phase, ShortFlowHazardV512); } }
        bool ResidenceRescueAllowedV512
        { get { return Model != null && PanicOutdoorSafetyV512.RescueAllowed(Model.Phase, ShortFlowHazardV512, residenceAftermathWaveV56 == Model.Wave); } }

        void ApplyOutdoorSafetyV512()
        {
            if (Model == null) return;
            // Only the four explicitly owned exterior roots. Never touch the player or sheltered family.
            if (OutdoorDisasterActiveV512 || Model.Phase == PanicPhase.Title)
                foreach (var neighbor in neighbors)
                    if (neighbor.visual) neighbor.visual.SetActive(false);
            if (streetLifeV414) streetLifeV414.ApplySafetyV512();
            if (helicopter && !ResidenceRescueAllowedV512)
            {
                helicopter.SetActive(false);
                if (rotorAudio) { rotorAudio.volume = 0; rotorAudio.Stop(); }
                if (effects) effects.StopRescueAudioV512();
            }
        }
    }
}
