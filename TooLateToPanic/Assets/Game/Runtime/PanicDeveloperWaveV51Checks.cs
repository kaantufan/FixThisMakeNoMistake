using System;

namespace TooLateToPanic
{
    /// <summary>Opt-in model/API checks for the V5.1 developer wave skip.</summary>
    public static class PanicDeveloperWaveV51Checks
    {
        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) return;
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride))
            {
                check("v5.1.devskip.isolated-profile", false,
                    "NOT_RUN: root must set a fresh isolated ScorePathOverride before the model fixtures.");
                return;
            }

            check("v5.1.devskip.isolated-profile", true,
                "Model/API fixtures only; no physical controller, OS input or human acceptance.");

            var skipped = new PanicModel();
            skipped.StartRun(false);
            skipped.Items.Clear();
            skipped.Items.Add(new PanicItem { Id = "qa_devskip_low_risk", Label = "QA low-risk fixture", Kind = ItemKind.Furniture, ArtId = "bookshelf", Installed = true });
            int originalWave = skipped.Wave;
            DisasterType originalDisaster = skipped.Disaster;
            int originalBudget = skipped.Budget;
            bool started = skipped.TryDeveloperSkipToDisasterV51();
            check("v5.1.devskip.preparation-enters-current-disaster",
                started && skipped.Phase == PanicPhase.Disaster && skipped.PhaseTime == 8f &&
                skipped.TimeRemaining == 0f && skipped.Wave == originalWave && skipped.Disaster == originalDisaster,
                "Preparation is bypassed; the currently selected disaster uses its normal eight-second phase.");
            check("v5.1.devskip-no-false-shelter-or-ranked-run",
                !skipped.ReachedShelterV5 && skipped.EnteredShelterV5 == PanicShelterKindV5.None &&
                skipped.CheatedRun && skipped.DeveloperWaveSkipProtectionV51 && !skipped.DeveloperNoLoss,
                "No shelter admission or persistent NoLoss toggle is created; score exclusion is sticky.");

            float disasterTime = skipped.PhaseTime;
            bool repeated = skipped.TryDeveloperSkipToDisasterV51();
            check("v5.1.devskip-repeat-does-not-restart-disaster",
                repeated && skipped.Phase == PanicPhase.Disaster && skipped.PhaseTime == disasterTime && skipped.Wave == originalWave,
                "Repeated F3 is idempotent and cannot advance the wave.");

            skipped.Tick(8f);
            float resolvedDamage = skipped.TotalDamage;
            check("v5.1.devskip-resolves-real-damage-and-empty-bag",
                skipped.Phase == PanicPhase.Aftermath && resolvedDamage > 0f && skipped.PackedWater == 0 && skipped.PackedFood == 0,
                "The normal disaster evaluator runs; absent supplies remain absent and damage remains real.");
            float aftermathTime = skipped.PhaseTime;
            skipped.TryDeveloperSkipToDisasterV51();
            skipped.TryDeveloperSkipToDisasterV51();
            check("v5.1.devskip-repeat-during-aftermath-is-idempotent",
                skipped.Phase == PanicPhase.Aftermath && skipped.PhaseTime == aftermathTime && skipped.Wave == originalWave,
                "Calling the action during its existing aftermath cinematic does not restart or double-resolve it.");

            skipped.Tick(4f);
            check("v5.1.devskip-failure-becomes-unrewarded-inspection",
                skipped.Phase == PanicPhase.Inspection && skipped.FailureReason.Length > 0 &&
                skipped.TotalDamage == resolvedDamage && skipped.CompletedWaves == 0 && skipped.Score == 0 &&
                skipped.LastReward == 0 && skipped.LastWaveScore == 0 && skipped.Budget == originalBudget,
                "No bag/damage failure ends the run, while damage evidence remains and no wave reward is granted.");

            skipped.FinishInspection();
            int nextWave = originalWave + 1;
            check("v5.1.devskip-protection-clears-on-next-wave",
                skipped.Phase == PanicPhase.Preparing && skipped.Wave == nextWave &&
                !skipped.DeveloperWaveSkipProtectionV51 && skipped.CheatedRun,
                "Protection is scoped to one wave; the cheated marker remains sticky for the run.");
            skipped.FinishInspection();
            check("v5.1.devskip-cannot-double-advance",
                skipped.Phase == PanicPhase.Preparing && skipped.Wave == nextWave && skipped.CompletedWaves == 0,
                "A stale inspection action cannot advance the wave a second time.");

            var inspectionSkip = new PanicModel();
            inspectionSkip.StartRun(false);
            inspectionSkip.Items.Clear();
            inspectionSkip.Items.Add(new PanicItem { Id = "qa_devskip_low_risk", Label = "QA low-risk fixture", Kind = ItemKind.Furniture, ArtId = "bookshelf", Installed = true });
            // Explicit low-risk/full-bag model fixture; legacy shop APIs are disabled.
            inspectionSkip.PackedWater = inspectionSkip.PackedFood = 1;
            inspectionSkip.Disaster = DisasterType.Earthquake;
            inspectionSkip.EnterBunker();
            inspectionSkip.Tick(8f); inspectionSkip.Tick(4f); inspectionSkip.Tick(3f);
            int priorCompletedWaves = inspectionSkip.CompletedWaves;
            int priorBudget = inspectionSkip.Budget;
            long priorScore = inspectionSkip.Score;
            int priorWave = inspectionSkip.Wave;
            bool inspectionSkipStarted = inspectionSkip.TryDeveloperSkipToDisasterV51();
            float nextDisasterTime = inspectionSkip.PhaseTime;
            inspectionSkip.TryDeveloperSkipToDisasterV51();
            check("v5.1.devskip-inspection-advances-once-into-next-disaster",
                inspectionSkipStarted && inspectionSkip.Phase == PanicPhase.Disaster &&
                inspectionSkip.Wave == priorWave + 1 && inspectionSkip.PhaseTime == nextDisasterTime &&
                nextDisasterTime == 8f && inspectionSkip.DeveloperWaveSkipProtectionV51 &&
                !inspectionSkip.ReachedShelterV5 && inspectionSkip.CheatedRun &&
                inspectionSkip.CompletedWaves == priorCompletedWaves && inspectionSkip.Budget == priorBudget &&
                inspectionSkip.Score == priorScore,
                "Inspection action advances one wave and immediately starts its current disaster; repeated F3 cannot advance again.");
            inspectionSkip.Tick(12f);
            check("v5.1.devskip-next-wave-has-no-extra-award",
                inspectionSkip.Phase == PanicPhase.Inspection && inspectionSkip.Wave == priorWave + 1 &&
                inspectionSkip.CompletedWaves == priorCompletedWaves && inspectionSkip.Budget == priorBudget &&
                inspectionSkip.Score == priorScore && inspectionSkip.LastReward == 0 && inspectionSkip.LastWaveScore == 0,
                "The previous normally completed wave keeps its result; the F3-skipped wave adds no reward or score.");
            int highscoreCount = inspectionSkip.Highscores.Count;
            inspectionSkip.ReturnToTitle();
            check("v5.1.devskip-cheated-run-is-not-recorded",
                inspectionSkip.Phase == PanicPhase.Title && inspectionSkip.Highscores.Count == highscoreCount && inspectionSkip.CheatedRun == false,
                "ReturnToTitle performs the ordinary run reset; the skipped run adds no highscore entry.");

            var protectedSuccess = new PanicModel();
            protectedSuccess.StartRun(false);
            protectedSuccess.Items.Clear();
            protectedSuccess.Items.Add(new PanicItem { Id = "qa_devskip_success_risk", Label = "QA low-risk fixture", Kind = ItemKind.Furniture, ArtId = "bookshelf", Installed = true });
            // Explicit low-risk/full-bag model fixture; legacy shop APIs are disabled.
            protectedSuccess.PackedWater = protectedSuccess.PackedFood = 1;
            protectedSuccess.Disaster = DisasterType.Earthquake;
            protectedSuccess.EnterBunker();
            protectedSuccess.Tick(8f);
            bool successfulWaveWasResolved = protectedSuccess.Phase == PanicPhase.Aftermath &&
                protectedSuccess.TotalDamage < PanicModel.DamageLossLimit && protectedSuccess.PackedWater == 0 && protectedSuccess.PackedFood == 0;
            float successAftermathTime = protectedSuccess.PhaseTime;
            bool protectedExistingAftermath = protectedSuccess.TryDeveloperSkipToDisasterV51();
            protectedSuccess.Tick(4f);
            check("v5.1.devskip-existing-success-aftermath-suppresses-reward",
                successfulWaveWasResolved && protectedExistingAftermath && successAftermathTime == 4f && protectedSuccess.Phase == PanicPhase.Inspection &&
                protectedSuccess.CompletedWaves == 0 && protectedSuccess.Score == 0 && protectedSuccess.LastReward == 0 &&
                protectedSuccess.LastWaveScore == 0 && protectedSuccess.CheatedRun,
                "Invoking F3 before the existing aftermath resolves protects the wave and prevents its normal award.");

            var rejected = new PanicModel();
            bool titleRejected = !rejected.TryDeveloperSkipToDisasterV51();
            rejected.StartRun(false); rejected.Phase = PanicPhase.Success;
            int completedBefore = rejected.CompletedWaves;
            bool successRejected = !rejected.TryDeveloperSkipToDisasterV51();
            check("v5.1.devskip-rejects-outside-unresolved-wave",
                titleRejected && successRejected && rejected.Phase == PanicPhase.Success && rejected.Wave == 1 &&
                rejected.CompletedWaves == completedBefore && !rejected.DeveloperWaveSkipProtectionV51,
                "Title and already-awarded success presentation cannot be restarted or advanced by F3.");
        }
    }
}
