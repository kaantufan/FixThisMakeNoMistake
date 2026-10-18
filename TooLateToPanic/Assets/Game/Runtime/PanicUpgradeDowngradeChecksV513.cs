using System;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in isolated model/API fixtures. No controller/OS/human acceptance claim.</summary>
    public static class PanicUpgradeDowngradeChecksV513
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static void Call(PanicModel m, string method) { typeof(PanicModel).GetMethod(method, Hidden).Invoke(m, null); }
        static void Field(PanicModel m, string name, object value) { typeof(PanicModel).GetField(name, Hidden).SetValue(m, value); }
        static float Roll(uint seed, int wave, string group)
        { return (float)typeof(PanicModel).GetMethod("UpgradeDowngradeRollV513", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { seed, wave, group }); }
        static uint WinningSeed()
        {
            for (uint seed = 1; seed < 100000; seed++)
                if (Roll(seed, 1, "floor:1") < .10f && Roll(seed, 1, "floor:0") >= .30f &&
                    Roll(seed, 1, "item:window") < .10f && Roll(seed, 1, "item:door") < .10f) return seed;
            throw new InvalidOperationException("No bounded deterministic fixture seed.");
        }
        static PanicItem Add(PanicModel m, string id, ItemKind kind, int tier, float y, float damage = 50f, float contribution = 2f)
        {
            var i = new PanicItem { Id = id, Label = id, Kind = kind, ArtId = kind == ItemKind.Furniture ? "table" : "wall",
                Tier = tier, Position = new Vector3(0, y, 0), Damage = damage, Contribution = contribution, Installed = true };
            m.Items.Add(i); return i;
        }
        static PanicModel Fixture(uint seed)
        {
            var m = new PanicModel(); m.StartRun(false); Field(m, "compoundSeedV54", seed);
            m.Items.Clear(); m.Phase = PanicPhase.Disaster; Field(m, "waveResolved", true); m.TotalDamage = 20;
            Add(m, "misleading_ground_id", ItemKind.Wall, 2, 3.2f);
            Add(m, "upper_b", ItemKind.Wall, 1, 3.2f); Add(m, "roof", ItemKind.Roof, 2, 6.4f, 0, 0);
            Add(m, "misleading_upper_id", ItemKind.Wall, 2, 0);
            Add(m, "window", ItemKind.Window, 2, 3.2f); Add(m, "door", ItemKind.Door, 2, 0);
            Add(m, "undamaged", ItemKind.Wall, 2, 3.2f, 0, 0);
            Add(m, "base", ItemKind.Wall, 0, 3.2f);
            Add(m, "uninstalled", ItemKind.Wall, 2, 3.2f).Installed = false;
            var furniture = Add(m, "furniture", ItemKind.Furniture, 2, 3.2f); furniture.Anchored = furniture.AnchorOwned = true;
            return m;
        }
        static PanicItem Item(PanicModel m, string id) { return m.Items.Find(i => i.Id == id); }
        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) return;
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride))
            { check("v513.downgrade.isolated", false, "NOT_RUN: isolated ScorePathOverride required."); return; }
            uint seed = WinningSeed(); var m = Fixture(seed);
            m.Phase = PanicPhase.Preparing; Call(m, "ResolveUpgradeDowngradesV513");
            check("v513.downgrade.preparation-pure", Item(m, "roof").Tier == 2 && m.UpgradeDowngradeWaveV513 == 0, "No downgrade before actual resolution.");
            m.Phase = PanicPhase.Disaster; Field(m, "waveResolved", false); Call(m, "ResolveUpgradeDowngradesV513");
            check("v513.downgrade.requires-resolution", Item(m, "roof").Tier == 2, "Uncommitted damage cannot mutate upgrades.");
            Field(m, "waveResolved", true); int budget = m.Budget; long score = m.Score;
            Call(m, "ResolveUpgradeDowngradesV513");
            check("v513.downgrade.upper-roof-group", Item(m, "misleading_ground_id").Tier == 1 && Item(m, "upper_b").Tier == 0 && Item(m, "roof").Tier == 1 && Item(m, "roof").Damage == 0, "Actual floor, one shared draw, including undamaged installed roof; each upgraded member loses exactly one.");
            check("v513.downgrade.actual-floor-isolation", Item(m, "misleading_upper_id").Tier == 2, "ID prefix does not choose floor or share upper roll.");
            check("v513.downgrade.standalone-structures", Item(m, "window").Tier == 1 && Item(m, "door").Tier == 1, "Windows and doors are separate valid upgrade groups.");
            check("v513.downgrade.eligibility", Item(m, "undamaged").Tier == 1 && Item(m, "uninstalled").Tier == 2 && Item(m, "base").Tier == 0, "Undamaged upgraded member follows damaged group; uninstalled/T0 cannot drop.");
            check("v513.downgrade.anchor-preserved", Item(m, "furniture").Tier == 2 && Item(m, "furniture").Anchored && Item(m, "furniture").AnchorOwned, "No immunity or anchor removal added.");
            check("v513.downgrade.damage-economy-preserved", m.TotalDamage == 20 && Item(m, "roof").Damage == 0 && Item(m, "roof").Contribution == 0 && Item(m, "upper_b").Damage == 50 && Item(m, "upper_b").Contribution == 2 && m.Budget == budget && m.Score == score, "Committed current damage, reward basis and money unchanged.");
            int count = m.UpgradeDowngradeOutcomesV513.Count; string summary = m.UpgradeDowngradeSummaryV513;
            Call(m, "ResolveUpgradeDowngradesV513");
            check("v513.downgrade.exactly-once", Item(m, "roof").Tier == 1 && count == m.UpgradeDowngradeOutcomesV513.Count && summary == m.UpgradeDowngradeSummaryV513, "Repeated resolution cannot lose another tier or reroll.");
            check("v513.downgrade.outcomes", count == 6 && m.UpgradeDowngradeOutcomesV513[0].BeforeTier > m.UpgradeDowngradeOutcomesV513[0].AfterTier && summary.Contains("GÜÇLENDİRME HASARI") && summary.Contains("T2 → T1"), "Immutable before/after data and Turkish next-wave explanation.");
            var reordered = Fixture(seed); reordered.Items.Reverse(); Call(reordered, "ResolveUpgradeDowngradesV513");
            check("v513.downgrade.stable-group-rng", reordered.UpgradeDowngradeSummaryV513 == summary && Item(reordered, "roof").Tier == 1, "Stable seed/wave/group; independent of item enumeration/UI.");
            var noDamage = Fixture(seed); foreach (var i in noDamage.Items) { i.Damage = i.Contribution = 0; }
            Call(noDamage, "ResolveUpgradeDowngradesV513");
            check("v513.downgrade.undamaged-group-excluded", Item(noDamage, "roof").Tier == 2 && noDamage.UpgradeDowngradeOutcomesV513.Count == 0, "A group with no real positive damage cannot downgrade.");
            var baseDamage = Fixture(seed); foreach (var i in baseDamage.Items) { i.Damage = i.Contribution = 0; }
            Item(baseDamage, "base").Damage = 50; Item(baseDamage, "base").Contribution = 2;
            Call(baseDamage, "ResolveUpgradeDowngradesV513");
            check("v513.downgrade.group-damage-not-member-damage", Item(baseDamage, "roof").Tier == 1 && Item(baseDamage, "base").Tier == 0, "Actual damage anywhere in installed group qualifies shared draw; T0 floor retained.");
            var budgetDev = Fixture(seed); budgetDev.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, true);
            budgetDev.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true); Call(budgetDev, "ResolveUpgradeDowngradesV513");
            check("v513.downgrade.budget-developer-still-eligible", Item(budgetDev, "roof").Tier == 1 && budgetDev.CheatedRun, "Non-no-loss budget/material options retain their score exclusion and allow actual downgrade.");
            for (int mode = 0; mode < 6; mode++)
            {
                var excluded = Fixture(seed);
                if (mode == 0) excluded.IsTutorial = true;
                if (mode == 1) typeof(PanicModel).GetProperty("IsSimulation").SetValue(excluded, true);
                if (mode == 2) typeof(PanicModel).GetProperty("IsJuryDemo").SetValue(excluded, true);
                if (mode == 3) excluded.SetDeveloperOption(PanicDeveloperOption.NoLoss, true);
                if (mode == 4) Field(excluded, "<DeveloperNoLossUsedThisWave>k__BackingField", true);
                if (mode == 5) { excluded.Phase = PanicPhase.Preparing; excluded.TryDeveloperSkipToDisasterV51(); }
                Call(excluded, "ResolveUpgradeDowngradesV513");
                check("v513.downgrade.excluded-mode-" + mode, Item(excluded, "roof").Tier == 2 && excluded.UpgradeDowngradeOutcomesV513.Count == 0, "Tutorial/simulation/jury/no-loss/F3 fixture preserves no-award policy.");
            }
            var preview = Fixture(seed); Call(preview, "CalculateDamage"); Call(preview, "CalculateDamage");
            check("v513.downgrade.calculation-pure", Item(preview, "roof").Tier == 2 && preview.UpgradeDowngradeOutcomesV513.Count == 0, "Preview/repeated CalculateDamage has no tier side effects.");
            var hooked = Fixture(seed); Field(hooked, "waveResolved", false); hooked.PhaseTime = .001f; hooked.Tick(.01f);
            check("v513.downgrade.real-resolution-hook", hooked.Phase == PanicPhase.Aftermath && hooked.UpgradeDowngradeWaveV513 == 1 && Item(hooked, "roof").Tier == 1, "Actual production Tick/ResolveDisaster hook; no helper invocation.");
            m.Phase = PanicPhase.Title; m.StartRun(false);
            check("v513.downgrade.reset-hook", m.UpgradeDowngradeWaveV513 == 0 && m.UpgradeDowngradeOutcomesV513.Count == 0 && m.UpgradeDowngradeSummaryV513.Length == 0, "New run clears prior outcomes and committed-wave guard.");
            int hits = 0; for (uint n = 1; n <= 10000; n++) if (Roll(n, 1, "floor:1") < .20f) hits++;
            check("v513.downgrade.bounded-roll-sample", hits > 1750 && hits < 2250, "10000 deterministic seeds, 20% threshold at 50% actual damage; sanity sample only.");
        }
    }
}
