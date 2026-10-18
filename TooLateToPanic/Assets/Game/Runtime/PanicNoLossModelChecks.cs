using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    [Serializable]
    public sealed class PanicNoLossCheck
    {
        public string name;
        public bool pass;
        public string detail;
        public PanicNoLossCheck(string name, bool pass, string detail = "") { this.name = name; this.pass = pass; this.detail = detail; }
    }

    /// <summary>
    /// Optional root-native check payload. Every model has a unique isolated score
    /// path beneath the supplied QA directory. Toy item lists deliberately exercise
    /// existing CalculateDamage, not world geometry/normal input or visual acceptance.
    /// No private completion flags, pendingSuccess or damage totals are overwritten.
    /// </summary>
    public static class PanicNoLossModelChecks
    {
        public static List<PanicNoLossCheck> Run(string isolatedEvidenceDirectory)
        {
            if (string.IsNullOrWhiteSpace(isolatedEvidenceDirectory)) throw new ArgumentException("Explicit isolated evidence directory required.");
            string folder = Path.Combine(Path.GetFullPath(isolatedEvidenceDirectory), "no-loss-model-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            string oldOverride = PanicModel.ScorePathOverride;
            var checks = new List<PanicNoLossCheck>();
            var modelPaths = new Dictionary<PanicModel, string>();
            Action<PanicModel, float> tick = (model, delta) => { PanicModel.ScorePathOverride = modelPaths[model]; model.Tick(delta); };
            Action<PanicModel> title = model => { PanicModel.ScorePathOverride = modelPaths[model]; model.ReturnToTitle(); };
            int fixture = 0;
            Action<string, bool, string> check = (name, pass, detail) => checks.Add(new PanicNoLossCheck(name, pass, detail));
            Func<bool, bool, bool, PanicModel> create = (enabled, supplies, lowRisk) =>
            {
                PanicModel.ScorePathOverride = Path.Combine(folder, "case-" + (++fixture) + ".json");
                var model = new PanicModel();
                modelPaths.Add(model, PanicModel.ScorePathOverride);
                model.SetDeveloperOption(PanicDeveloperOption.NoLoss, enabled);
                model.StartRun(false);
                // Declared model-only fixture: one low-risk furnishing or 20 exposed roofs.
                // Neither world placements nor any hidden outcome flags are fabricated.
                model.Items.Clear();
                int count = lowRisk ? 1 : 20;
                for (int i = 0; i < count; i++) model.Items.Add(new PanicItem {
                    Id = "qa_noloss_" + i, Label = "QA risk fixture", Installed = true,
                    Kind = lowRisk ? ItemKind.Furniture : ItemKind.Roof,
                    ArtId = lowRisk ? "bookshelf" : "roof", Size = Vector3.one
                });
                if (supplies) { model.BuyWater(); model.PackWater(); model.BuyFood(); model.PackFood(); }
                return model;
            };
            try
            {
                var normalDeadline = create(false, false, true);
                tick(normalDeadline, normalDeadline.TimeRemaining + .01f);
                check("NoLoss OFF: deadline still loses", normalDeadline.Phase == PanicPhase.GameOver, "No threshold/bag/deadline bypass with default options.");

                var deadline = create(true, true, true); int deadlineBudget = deadline.Budget;
                tick(deadline, deadline.TimeRemaining + .01f);
                check("NoLoss ON: timeout enters real disaster", deadline.Phase == PanicPhase.Disaster && deadline.DeveloperNoLossUsedThisWave && deadline.CheatedRun, deadline.Message);
                tick(deadline, 8f);
                check("protected timeout calculates real damage and consumes stock", deadline.Phase == PanicPhase.Aftermath && deadline.TotalDamage > 0 && deadline.PackedWater == 0 && deadline.PackedFood == 0, "Actual CalculateDamage + ConsumeWaveSupplies; no bag completion or immunity.");
                tick(deadline, 4f);
                check("protected timeout reaches inspection without fake reward", deadline.Phase == PanicPhase.Inspection && deadline.CompletedWaves == 0 && deadline.Score == 0 && deadline.LastReward == 0 && deadline.Budget == deadlineBudget, deadline.Message);

                var highNormal = create(false, true, false); highNormal.EnterBunker(); tick(highNormal, 8f);
                float actualHigh = highNormal.TotalDamage;
                var highProtected = create(true, true, false); int highBudget = highProtected.Budget;
                highProtected.EnterBunker(); tick(highProtected, 8f);
                check("NoLoss keeps exact actual high damage", actualHigh > 65f && highProtected.TotalDamage == actualHigh, "Normal=" + actualHigh + "; protected=" + highProtected.TotalDamage);
                tick(highNormal, 4f); tick(highProtected, 4f);
                check("OFF high damage loses; ON high damage inspects", highNormal.Phase == PanicPhase.GameOver && highProtected.Phase == PanicPhase.Inspection, "Real existing damage adjudication.");
                check("protected high damage keeps failure evidence, zero award", !string.IsNullOrEmpty(highProtected.FailureReason) && highProtected.LastReward == 0 && highProtected.LastWaveScore == 0 && highProtected.Score == 0 && highProtected.CompletedWaves == 0 && highProtected.Budget == highBudget, highProtected.Message);

                var emptyNormal = create(false, false, true); emptyNormal.EnterBunker(); tick(emptyNormal, 12f);
                var emptyProtected = create(true, false, true); emptyProtected.EnterBunker(); tick(emptyProtected, 12f);
                check("OFF missing bag loses; ON missing bag inspects", emptyNormal.Phase == PanicPhase.GameOver && emptyProtected.Phase == PanicPhase.Inspection, emptyProtected.FailureReason);
                check("NoLoss never fabricates packed supplies", emptyProtected.PackedWater == 0 && emptyProtected.PackedFood == 0 && emptyProtected.WaterStock == 0 && emptyProtected.FoodStock == 0, "Empty stays empty.");

                var success = create(false, true, true); int successBudget = success.Budget; success.EnterBunker(); tick(success, 12f);
                check("normal actual success retains normal reward", success.Phase == PanicPhase.Success && success.CompletedWaves == 1 && success.Score > 0 && success.LastReward > 0 && success.Budget > successBudget, "Default OFF path unchanged.");
                var protectedSuccess = create(true, true, true); int protectedBudget = protectedSuccess.Budget;
                protectedSuccess.EnterBunker(); tick(protectedSuccess, 12f);
                check("NoLoss success does not masquerade as earned wave", protectedSuccess.Phase == PanicPhase.Inspection && protectedSuccess.CompletedWaves == 0 && protectedSuccess.Score == 0 && protectedSuccess.LastReward == 0 && protectedSuccess.Budget == protectedBudget, protectedSuccess.Message);

                var late = create(false, false, true); late.EnterBunker(); tick(late, 8f);
                late.SetDeveloperOption(PanicDeveloperOption.NoLoss, true); tick(late, 4f);
                check("enabling in aftermath protects pending failure", late.Phase == PanicPhase.Inspection && late.CheatedRun && late.LastReward == 0, late.Message);
                var offDeadline = create(true, false, true); offDeadline.SetDeveloperOption(PanicDeveloperOption.NoLoss, false); tick(offDeadline, offDeadline.TimeRemaining + .01f);
                check("OFF restores deadline loss while cheated remains sticky", offDeadline.Phase == PanicPhase.GameOver && offDeadline.CheatedRun && !offDeadline.DeveloperNoLoss, "NoLoss usage remains recorded.");
                var offFailure = create(true, true, false); offFailure.EnterBunker(); tick(offFailure, 8f);
                offFailure.SetDeveloperOption(PanicDeveloperOption.NoLoss, false); tick(offFailure, 4f);
                check("OFF before failure adjudication restores GameOver", offFailure.Phase == PanicPhase.GameOver && offFailure.CheatedRun, "Does not retroactively clear CheatedRun.");

                var toggled = create(true, true, true); int toggleBudget = toggled.Budget;
                tick(toggled, toggled.TimeRemaining + .01f); toggled.SetDeveloperOption(PanicDeveloperOption.NoLoss, false); tick(toggled, 12f);
                check("used protection cannot earn reward by toggling OFF", toggled.Phase == PanicPhase.Inspection && toggled.CheatedRun && toggled.CompletedWaves == 0 && toggled.Score == 0 && toggled.Budget == toggleBudget, toggled.Message);

                var reset = create(true, false, true); tick(reset, reset.TimeRemaining + .01f); tick(reset, 12f);
                title(reset);
                check("menu reset retains process-local option, clears wave latch", reset.DeveloperNoLoss && !reset.DeveloperNoLossUsedThisWave && reset.DeveloperNoLossReason == string.Empty && reset.Phase == PanicPhase.Title, "Matches other developer option persistence.");
                reset.SetDeveloperOption(PanicDeveloperOption.NoLoss, false); reset.StartRun(false);
                check("fresh OFF run resets latch and cheated state", !reset.DeveloperNoLoss && !reset.DeveloperNoLossUsedThisWave && !reset.CheatedRun && reset.Budget == 1000 && reset.PackedWater == 0 && reset.PackedFood == 0, "Clean new run, default actual stock.");

                var scoreGuard = create(true, true, true);
                string scorePath = PanicModel.ScorePathOverride;
                File.WriteAllText(scorePath, "{\"schema\":1,\"entries\":[]}");
                string before = Hash(scorePath);
                scoreGuard.EnterBunker(); tick(scoreGuard, 12f); title(scoreGuard);
                check("NoLoss never changes seeded isolated score bytes", before == Hash(scorePath) && !File.Exists(scorePath + ".tmp"), "Explicit QA path beneath supplied isolated evidence.");

                PanicModel.ScorePathOverride = Path.Combine(folder, "jury.json");
                var juryModel = new PanicModel(); juryModel.SetDeveloperOption(PanicDeveloperOption.NoLoss, true);
                juryModel.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, true);
                var jury = new PanicJuryDemo(juryModel, new PanicExpedition(juryModel)); jury.Start();
                jury.Tick(300f, PanicJuryDemo.SpawnPosition, false, true);
                bool truthful = true; foreach (var goal in jury.Snapshot.Goals) truthful &= goal.State != PanicJuryGoalState.Complete;
                check("NoLoss does not extend jury 270/300 second cap", jury.Stage == PanicJuryStage.Complete && jury.ElapsedSeconds <= 270.01f && jury.ElapsedSeconds <= 300f, "Injected wall-time fixture; no human timing acceptance.");
                check("NoLoss does not fabricate jury goals", truthful && juryModel.CheatedRun && juryModel.Score == 0 && juryModel.CompletedWaves == 0, "NoLoss remains orthogonal to truthful jury controller.");
            }
            catch (Exception ex) { checks.Add(new PanicNoLossCheck("NoLoss model QA exception", false, ex.ToString())); }
            finally { PanicModel.ScorePathOverride = oldOverride; }
            return checks;
        }

        static string Hash(string path)
        {
            using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }
    }
}
