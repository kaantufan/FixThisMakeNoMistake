namespace TooLateToPanic
{
    public sealed partial class PanicModel
    {
        /// <summary>One-wave no-loss gate used only by the explicit developer skip.</summary>
        public bool DeveloperWaveSkipProtectionV51 { get; private set; }

        /// <summary>
        /// F3 is available during an unresolved campaign wave or its inspection.
        /// Simulation, tutorial and jury flows retain their own clocks and rules.
        /// </summary>
        public bool CanDeveloperSkipWaveV51
        {
            get
            {
                return !IsSimulation && !IsJuryDemo && !IsTutorial && !KeyboardHelpOpenV56 && !WaveLedgerOpenV56 && !WaveLedgerAwaitingReviewV56 &&
                    (Phase == PanicPhase.Preparing || Phase == PanicPhase.Disaster ||
                     Phase == PanicPhase.Aftermath || Phase == PanicPhase.Inspection);
            }
        }

        /// <summary>
        /// Marks this run cheated and protects the current wave. From preparation it
        /// enters the ordinary unsheltered disaster path, retaining real damage and
        /// supply consumption without manufacturing a shelter admission.
        /// From inspection it finishes that screen and starts the next wave's disaster
        /// in the same call. Repeated calls during a disaster/aftermath do not restart it.
        /// </summary>
        public bool TryDeveloperSkipToDisasterV51()
        {
            if (!CanDeveloperSkipWaveV51) return false;

            if (Phase == PanicPhase.Inspection)
            {
                int inspectedWave = Wave;
                MarkRunCheated();
                FinishInspection();
                int expectedWave = inspectedWave == int.MaxValue ? inspectedWave : inspectedWave + 1;
                if (Phase != PanicPhase.Preparing || Wave != expectedWave) return false;
            }

            if (DeveloperWaveSkipProtectionV51)
                return Phase == PanicPhase.Disaster || Phase == PanicPhase.Aftermath;

            DeveloperWaveSkipProtectionV51 = true;
            DeveloperNoLossUsedThisWave = true;
            DeveloperNoLossReason = "Bu dalga geliştirici dalga atlama koruması ile başlatıldı.";
            MarkRunCheated();

            if (Phase == PanicPhase.Preparing)
            {
                BeginUnshelteredDisasterV5();
                Message = "Geliştirici: hazırlık atlandı. Afet sinematiği başladı; gerçek hasar ve erzak tüketimi korunur, bu dalgaya skor veya ödül verilmez.";
            }

            return true;
        }

        private void ResetDeveloperWaveSkipProtectionV51()
        {
            DeveloperWaveSkipProtectionV51 = false;
        }
    }
}
