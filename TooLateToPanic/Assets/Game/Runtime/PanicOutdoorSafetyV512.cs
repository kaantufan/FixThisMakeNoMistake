namespace TooLateToPanic
{
    /// <summary>Read-only presentation policy. Hazard membership is independent of primary disaster type.</summary>
    public static class PanicOutdoorSafetyV512
    {
        public static bool HazardActive(PanicPhase phase, bool shortFlowHazard)
        { return phase == PanicPhase.Disaster || shortFlowHazard; }
        public static bool RecoveryAllowed(PanicPhase phase, bool shortFlowHazard)
        {
            if (HazardActive(phase, shortFlowHazard)) return false;
            return phase == PanicPhase.Aftermath || phase == PanicPhase.Success ||
                phase == PanicPhase.Inspection || phase == PanicPhase.GameOver;
        }
        public static bool RescueAllowed(PanicPhase phase, bool shortFlowHazard, bool outcomeCommittedThisWave)
        { return outcomeCommittedThisWave && RecoveryAllowed(phase, shortFlowHazard); }
    }
}
