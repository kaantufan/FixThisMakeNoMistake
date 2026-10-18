using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    [Serializable] public sealed class PanicStormModelCheck
    {
        public string name; public bool pass; public string detail;
        public PanicStormModelCheck(string name, bool pass, string detail = "") { this.name = name; this.pass = pass; this.detail = detail; }
    }

    /// <summary>Root-native model fixtures. No world/OS/art/camera or engineering acceptance.</summary>
    public static class PanicStormModelChecks
    {
        public static List<PanicStormModelCheck> Run(string isolatedEvidenceDirectory)
        {
            if (string.IsNullOrWhiteSpace(isolatedEvidenceDirectory)) throw new ArgumentException("Isolated evidence directory required.");
            string folder = Path.Combine(Path.GetFullPath(isolatedEvidenceDirectory), "storm-model-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            string previous = PanicModel.ScorePathOverride;
            int index = 0;
            var checks = new List<PanicStormModelCheck>();
            Action<string, bool, string> check = (name, pass, detail) => checks.Add(new PanicStormModelCheck(name, pass, detail));
            Func<DisasterType, PanicModel> create = type =>
            {
                PanicModel.ScorePathOverride = Path.Combine(folder, "model-" + (++index) + ".json");
                var model = new PanicModel(); model.StartSimulation(type); return model;
            };
            Func<DisasterType, PanicItem, bool, float> damage = (type, item, guards) =>
            {
                var model = create(type); model.Items = new List<PanicItem> { item };
                if (guards) { model.DrainCount = 4; model.FenceCount = 4; model.TreeCount = 5; model.FireToolCount = 3; item.FloodProtected = item.FireProtected = true; }
                model.EnterBunker(); model.Tick(8f); return model.TotalDamage;
            };
            try
            {
                check("legacy ordinals unchanged, Storm appended", (int)DisasterType.Earthquake == 0 && (int)DisasterType.Flood == 1 && (int)DisasterType.Fire == 2 && (int)DisasterType.Storm == 3, "Save/old dispatch ordinal compatibility.");
                foreach (DisasterType type in new[] { DisasterType.Earthquake, DisasterType.Flood, DisasterType.Fire, DisasterType.Storm })
                {
                    var model = create(type);
                    check("explicit clean selection " + type, model.IsSimulation && !model.IsJuryDemo && !model.SimulationComplete && model.Disaster == type && model.Wave == 1 && model.RequiredDays == 1 && model.TimeRemaining == 90f && model.Budget == 1000 && model.PackedWater == 1 && model.PackedFood == 1, "Initial food/water is baseline, not pickup achievement.");
                    model.Items = new List<PanicItem> { Item("stable-fixture", ItemKind.Furniture, "chair") };
                    model.EnterBunker(); model.Tick(8f);
                    float actual = model.TotalDamage;
                    check("single real damage and consumption " + type, model.Phase == PanicPhase.Aftermath && actual > 0 && model.PackedWater == 0 && model.PackedFood == 0, "Toy one-item risk fixture; existing production resolver used.");
                    model.Tick(4f); model.Tick(1000f); model.FinishInspection();
                    check("one stable unranked outcome " + type, model.SimulationComplete && model.Phase == PanicPhase.Inspection && model.PhaseTime == 0 && model.Wave == 1 && model.TotalDamage == actual && model.CompletedWaves == 0 && model.Score == 0 && model.LastReward == 0 && model.Budget == 1000 && !model.EnterBunker(), "No automatic next wave, repeat damage, award or cleared evidence.");
                }

                float roof = damage(DisasterType.Storm, Item("same-risk", ItemKind.Roof, "roof"), false);
                float roofTier = damage(DisasterType.Storm, Item("same-risk", ItemKind.Roof, "roof", 2), false);
                float window = damage(DisasterType.Storm, Item("same-risk", ItemKind.Window, "window"), false);
                float windowTier = damage(DisasterType.Storm, Item("same-risk", ItemKind.Window, "window", 2), false);
                check("storm roof/window tier reduces actual risk", roof > roofTier && window > windowTier && roofTier > 0 && windowTier > 0, "T2 reduces but never makes them immune.");
                float loose = damage(DisasterType.Storm, Item("same-risk", ItemKind.Furniture, "fridge"), false);
                float anchored = damage(DisasterType.Storm, Item("same-risk", ItemKind.Furniture, "fridge", 0, true), false);
                float unrelated = damage(DisasterType.Storm, Item("same-risk", ItemKind.Furniture, "fridge"), true);
                check("storm furniture anchoring reduces risk without immunity", anchored > 0 && anchored < loose, "Existing actual anchor state only.");
                check("water/fire guards grant no wind bonus", Mathf.Abs(loose - unrelated) < .0001f, "No accidental flood/fire fallback.");

                foreach (DisasterType type in new[] { DisasterType.Earthquake, DisasterType.Flood, DisasterType.Fire, DisasterType.Storm })
                {
                    float plain = damage(type, Item("same-sofa", ItemKind.Furniture, "sofa"), false);
                    float indigo = damage(type, Item("same-sofa", ItemKind.Furniture, "sofa_indigo"), false);
                    float olive = damage(type, Item("same-sofa", ItemKind.Furniture, "sofa_olive"), false);
                    check("color aliases retain canonical sofa risk " + type, plain > 0 && Mathf.Abs(plain - indigo) < .0001f && Mathf.Abs(plain - olive) < .0001f, "Same ID/seed; source art name cannot erase risk.");
                }

                var high = create(DisasterType.Storm); high.Items.Clear();
                for (int i = 0; i < 20; i++) high.Items.Add(Item("roof-risk-" + i, ItemKind.Roof, "roof"));
                high.EnterBunker(); high.Tick(8f);
                bool finite = Finite(high.TotalDamage) && high.TotalDamage > 65f && high.TotalDamage <= 100.001f;
                float contribution = 0;
                foreach (var item in high.Items) { finite &= Finite(item.Damage) && item.Damage >= 0 && item.Damage <= 100f && Finite(item.Contribution) && item.Contribution >= 0; contribution += item.Contribution; }
                check("high wind damage finite and coherent percentage", finite && Mathf.Abs(contribution - high.TotalDamage) < .001f, "New type cap does not change legacy 0..2 formulas.");
                high.Tick(4f);
                check("normal storm still loses above65", high.Phase == PanicPhase.GameOver && high.SimulationComplete && !string.IsNullOrEmpty(high.FailureReason) && high.Score == 0 && high.CompletedWaves == 0, high.FailureReason);

                var protectedRun = create(DisasterType.Storm); protectedRun.Items.Clear();
                for (int i = 0; i < 20; i++) protectedRun.Items.Add(Item("roof-risk-" + i, ItemKind.Roof, "roof"));
                protectedRun.SetDeveloperOption(PanicDeveloperOption.NoLoss, true); protectedRun.EnterBunker(); protectedRun.Tick(12f);
                check("NoLoss simulation keeps same real damage and stable inspection", protectedRun.TotalDamage == high.TotalDamage && protectedRun.Phase == PanicPhase.Inspection && protectedRun.SimulationComplete && protectedRun.PhaseTime == 0 && protectedRun.Score == 0 && protectedRun.CompletedWaves == 0 && protectedRun.Budget == 1000, protectedRun.Message);

                var missed = create(DisasterType.Flood); missed.PackedWater = missed.PackedFood = 0;
                missed.Items = new List<PanicItem> { Item("empty-bag-fixture", ItemKind.Furniture, "chair") };
                missed.EnterBunker(); missed.Tick(12f);
                check("simulation missing bag remains truthful GameOver", missed.Phase == PanicPhase.GameOver && missed.SimulationComplete && missed.PackedWater == 0 && missed.PackedFood == 0, "Declared missing-bag state fixture, no completion flag edits.");

                var expired = create(DisasterType.Storm); expired.Tick(90.01f);
                check("simulation preparation deadline remains real loss", expired.Phase == PanicPhase.GameOver && expired.SimulationComplete && expired.Score == 0 && expired.CompletedWaves == 0, expired.FailureReason);
                var protectedDeadline = create(DisasterType.Storm); protectedDeadline.SetDeveloperOption(PanicDeveloperOption.NoLoss, true);
                protectedDeadline.Tick(90.01f); protectedDeadline.Tick(12f);
                check("NoLoss simulation deadline uses real disaster then stable result", protectedDeadline.SimulationComplete && protectedDeadline.Phase == PanicPhase.Inspection && protectedDeadline.PhaseTime == 0 && protectedDeadline.TotalDamage > 0 && protectedDeadline.PackedWater == 0 && protectedDeadline.PackedFood == 0 && protectedDeadline.Score == 0 && protectedDeadline.CompletedWaves == 0, protectedDeadline.Message);

                var reset = create(DisasterType.Storm);
                check("second active Start cannot leak another setup", !reset.StartSimulation(DisasterType.Fire) && reset.Disaster == DisasterType.Storm && reset.TimeRemaining == 90f && reset.PackedWater == 1 && reset.PackedFood == 1, "Existing start gate.");
                reset.ReturnToTitle(); reset.StartRun(false);
                check("simulation to normal clears all mode/baseline flags", !reset.IsSimulation && !reset.SimulationComplete && !reset.IsJuryDemo && reset.PackedWater == 0 && reset.PackedFood == 0 && reset.Disaster == DisasterType.Earthquake, "No baseline supplies leak.");
                reset.ReturnToTitle(); reset.StartJuryDemo();
                check("simulation to short flow clears independent flags", !reset.IsSimulation && !reset.SimulationComplete && reset.IsJuryDemo, "Original short-flow controller contract unaffected.");
                reset.ReturnToTitle();
                check("invalid selection leaves title clean", !reset.StartSimulation((DisasterType)99) && !reset.IsSimulation && !reset.SimulationComplete && reset.Phase == PanicPhase.Title && reset.PackedWater == 0, "No invalid enum enters resolver.");

                var scores = create(DisasterType.Storm); scores.Items = new List<PanicItem> { Item("score-fixture", ItemKind.Furniture, "chair") };
                string path = PanicModel.ScorePathOverride; File.WriteAllText(path, "{\"schema\":1,\"entries\":[]}"); string hash = Hash(path);
                scores.EnterBunker(); scores.Tick(12f); scores.Tick(1000f); scores.ReturnToTitle();
                check("clean simulation never alters seeded score bytes", hash == Hash(path) && !File.Exists(path + ".tmp"), "Mode exclusion works without CheatedRun.");

                PanicModel.ScorePathOverride = Path.Combine(folder, "campaign.json");
                var campaign = new PanicModel(); campaign.StartRun(false); campaign.Items = new List<PanicItem> { Item("campaign-fixture", ItemKind.Furniture, "chair") };
                bool firstNine = true, randomThree = true;
                for (int wave = 1; wave <= 18; wave++)
                {
                    if (wave <= 9) firstNine &= campaign.Disaster == (wave <= 3 ? DisasterType.Earthquake : wave <= 6 ? DisasterType.Flood : DisasterType.Fire);
                    else randomThree &= campaign.Disaster != DisasterType.Storm;
                    for (int day = 0; day < campaign.RequiredDays; day++) { campaign.BuyWater(); campaign.PackWater(); campaign.BuyFood(); campaign.PackFood(); }
                    campaign.EnterBunker(); campaign.Tick(15f); campaign.FinishInspection();
                }
                check("campaign1to9 remains original three types", firstNine && campaign.CompletedWaves == 18, "Actual resolver/normal reward path over declared low-risk item fixture.");
                check("campaign random pool still excludes new selectable type", randomThree, "Existing seeded %3 remains unchanged.");
            }
            catch (Exception ex) { checks.Add(new PanicStormModelCheck("storm model QA exception", false, ex.ToString())); }
            finally { PanicModel.ScorePathOverride = previous; }
            return checks;
        }

        static PanicItem Item(string id, ItemKind kind, string art, int tier = 0, bool anchored = false)
        { return new PanicItem { Id = id, Label = "QA risk fixture", Kind = kind, ArtId = art, Size = Vector3.one, Installed = true, Movable = kind == ItemKind.Furniture, Tier = tier, Anchored = anchored, AnchorSurface = anchored ? "floor" : string.Empty }; }
        static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        static string Hash(string path)
        { using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""); }
    }
}
