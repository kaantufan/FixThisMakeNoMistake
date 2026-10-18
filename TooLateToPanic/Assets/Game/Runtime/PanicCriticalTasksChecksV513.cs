using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in production pure-model API checks. Explicit threat/tier/supply fixtures;
    /// no World, natural route, controller, render, OS or human acceptance.</summary>
    public static class PanicCriticalTasksChecksV513
    {
        static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            if (string.IsNullOrWhiteSpace(PanicModel.ScorePathOverride)) throw new InvalidOperationException("Fresh isolated score path required.");
            for (int type = 0; type < 4; type++)
            {
                var solo = New(1 << type);
                solo.Items = new List<PanicItem> { new PanicItem { Id = "critical_fixture", Label = "Explicit low-risk target",
                    ArtId = type == 3 ? "roof" : "chair", Kind = type == 3 ? ItemKind.Roof : ItemKind.Furniture, Movable = true, Installed = true } };
                Resolve(solo);
                float expected = type == 0 ? 20 : 25;
                check("critical.exact-points." + type, Near(solo.CriticalPenaltyDamageV513, expected) && solo.TotalDamage < 100,
                    "Explicit single low-risk target avoids total saturation: missing core task adds exactly20/25 points.");
            }
            for (int mask = 1; mask <= 15; mask++)
            {
                var m = New(mask); int budget = m.Budget, water = m.PackedWater, food = m.PackedFood;
                int rolls = m.SecondaryRollCountV5; float clock = m.TimeRemaining;
                string before = string.Join("|", m.Items.Select(i => i.Id + ":" + i.Tier + ":" + i.Anchored));
                for (int n = 0; n < 12; n++) { var rows = m.CriticalTasksV513; string summary = m.CriticalTaskPenaltySummaryV513; }
                check("critical.read-only." + mask, m.Budget == budget && m.PackedWater == water && m.PackedFood == food &&
                    m.SecondaryRollCountV5 == rolls && m.TimeRemaining == clock && before == string.Join("|", m.Items.Select(i => i.Id + ":" + i.Tier + ":" + i.Anchored)),
                    "Repeated task reads do not mutate budget, supplies, RNG, clock, tier or anchors.");
                Call(m, "CalculateDamage");
                float baseline = m.TotalDamage; int previewRolls = m.SecondaryRollCountV5;
                Call(m, "CalculateDamage");
                check("critical.analytical-unmodified." + mask, Near(m.CriticalPenaltyDamageV513, 0) && Near(m.TotalDamage, baseline) && m.SecondaryRollCountV5 == previewRolls,
                    "Analytical CalculateDamage keeps baseline semantics and never charges critical tasks.");
                Resolve(m);
                var hazardRows = m.CriticalTasksV513.Where(r => r.PenaltyPoints > 0).ToArray();
                check("critical.unique-active." + mask, hazardRows.Length == Bits(mask) && hazardRows.Select(r => r.Id).Distinct().Count() == hazardRows.Length,
                    "Exact active mask fixture; one core task per unique hazard.");
                check("critical.real-damage-cap." + mask, m.CriticalPenaltyDamageV513 > 0 && m.CriticalPenaltyDamageV513 <= 40.001f && m.TotalDamage <= 100.001f,
                    "20/25 point task penalties, shared40 cap and shared100 total cap.");
                float itemTotal = m.Items.Sum(i => i.Contribution), hazardTotal = 0;
                for (int n = 0; n < 4; n++) hazardTotal += m.HazardDamageContributionV54((DisasterType)n);
                check("critical.conservation." + mask, Near(itemTotal, m.TotalDamage) && Near(hazardTotal, m.TotalDamage) &&
                    Near(m.SecondaryEventsV5.Sum(e => e.DamagePoints), m.SecondaryDamageV5), "Actual item, hazard and utility attribution conserve final totals.");
                var totals = m.Items.Select(i => i.Contribution).ToArray(); float damage = m.TotalDamage;
                int finalRolls = m.SecondaryRollCountV5;
                Call(m, "ResolveDisaster");
                check("critical.resolution-idempotent." + mask, Near(damage, m.TotalDamage) && m.Items.Select((i, n) => Near(i.Contribution, totals[n])).All(v => v) && m.SecondaryRollCountV5 == finalRolls,
                    "Repeated actual resolver exits through its guard; no second penalty, RNG roll or damage recomputation.");
                Call(m, "CalculateDamage");
                check("critical.committed-calculation-read." + mask, Near(damage, m.TotalDamage) && m.Items.Select((i, n) => Near(i.Contribution, totals[n])).All(v => v) && m.SecondaryRollCountV5 == finalRolls,
                    "Analytical read after commitment preserves the final critical/utility ledger and pre-wear damage exactly.");
            }
            var all = New(15);
            foreach (var item in all.Items) { if (item.Kind == ItemKind.Roof) item.Tier = 1; if (item.Kind == ItemKind.Furniture && item.Movable) item.Anchored = true; }
            all.UtilitySafety.Complete("gasoff"); all.UtilitySafety.Complete("poweroff");
            Resolve(all);
            check("critical.completed-no-penalty", Near(all.CriticalPenaltyDamageV513, 0) && all.CriticalTasksV513.Where(r => r.PenaltyPoints > 0).All(r => r.Complete), "All completed tasks suppress their exact penalties.");
            all.Wave++;
            check("critical.utilities-persist", all.UtilitySafety.PowerOff && all.UtilitySafety.GasClosed, "Task reads and waves never reset completed isolation.");
            var exact = New(1); exact.PackedWater = exact.PackedFood = 1;
            bool entered = exact.TryEnterShelterV5(exact.RequiredShelterV5); Call(exact, "ResolveDisaster");
            float final = exact.TotalDamage, critical = exact.CriticalPenaltyDamageV513; int packed = exact.PackedWater;
            bool entryTasks = exact.CriticalTasksV513.Where(r => r.Id == "water" || r.Id == "food" || r.Id == "shelter").All(r => r.Complete && r.Resolved);
            Call(exact, "ResolveDisaster");
            check("critical.once-and-entry-snapshot", entered && entryTasks && Near(final, exact.TotalDamage) && Near(critical, exact.CriticalPenaltyDamageV513) && packed == exact.PackedWater,
                "Existing resolver guard consumes once. Frozen tasks retain true packed-at-entry counts after consumption.");
            typeof(PanicModel).GetMethod("CaptureWaveLedgerV56", Private).Invoke(exact, new object[] { "Critical fixture", "No award", exact.Budget, 0, 0, 0 });
            check("critical.ledger-no-duplicate", exact.LastWaveLedgerV56 != null && Near(exact.LastWaveLedgerV56.Rows.Sum(r => r.DamageContribution), exact.TotalDamage) &&
                Near(exact.LastWaveLedgerV56.Rows.Where(r => r.Id.StartsWith("critical:")).Sum(r => r.DamageContribution), exact.CriticalPenaltyDamageV513),
                "Actual ledger splits critical points out of ordinary item rows and lists their task causes exactly once.");
            foreach (string mode in new[] { "tutorial", "simulation", "jury", "f3" })
            {
                var m = new PanicModel();
                if (mode == "tutorial") m.StartRun(true);
                else if (mode == "simulation") m.StartSimulation(DisasterType.Fire);
                else if (mode == "jury") m.StartJuryDemo();
                else { m.StartRun(false); m.TryDeveloperSkipToDisasterV51(); }
                Call(m, "CalculateDamage");
                Call(m, "ApplyCriticalTaskPenaltyV513");
                check("critical.excluded." + mode, Near(m.CriticalPenaltyDamageV513, 0) && !m.CriticalTaskPenaltiesEnabledV513 && !m.ReachedShelterV5,
                    "Explicit analytical+penalty-hook fixture: excluded mode retains existing rules; no critical extra damage or synthetic shelter.");
            }
            var protectedRun = New(1); protectedRun.SetDeveloperOption(PanicDeveloperOption.NoLoss, true);
            Resolve(protectedRun);
            check("critical.noloss-retains-real-damage", protectedRun.CheatedRun && protectedRun.DeveloperNoLoss && protectedRun.CriticalPenaltyDamageV513 > 0,
                "NoLoss only overrides outcome; it does not exempt critical damage. CheatedRun alone never filters the charge.");
            check("critical.loss75-retained", !PanicModel.DamageCausesLoss(74.99f) && PanicModel.DamageCausesLoss(75f), "Original hard loss threshold unchanged.");
            var roof = New(8); var installed = roof.Items.First(i => i.Kind == ItemKind.Roof); installed.Tier = 1;
            bool ready = roof.CriticalTasksV513.First(r => r.Id == "roof").Complete; installed.Tier = 0;
            check("critical.roof-drop-read-live", ready && !roof.CriticalTasksV513.First(r => r.Id == "roof").Complete, "Explicit next-wave downgrade fixture; task reads actual current tier.");
        }
        static PanicModel New(int mask)
        {
            var m = new PanicModel(); m.StartRun(false); m.Wave = 3;
            m.Disaster = (DisasterType)Enumerable.Range(0, 4).First(n => (mask & (1 << n)) != 0);
            Set(m, "compoundMaskV54", mask); Set(m, "compoundWaveV54", 3); Set(m, "compoundPrimaryV54", m.Disaster);
            return m;
        }
        static int Bits(int mask) { int count = 0; for (int n = 0; n < 4; n++) if ((mask & (1 << n)) != 0) count++; return count; }
        static void Resolve(PanicModel m)
        {
            if (!m.TryEnterShelterV5(m.RequiredShelterV5)) throw new InvalidOperationException("Explicit model fixture could not enter its required shelter.");
            Call(m, "ResolveDisaster");
        }
        static bool Near(float a, float b) { return Mathf.Abs(a - b) < .002f; }
        static void Set(PanicModel m, string field, object value) { typeof(PanicModel).GetField(field, Private).SetValue(m, value); }
        static void Call(PanicModel m, string method) { typeof(PanicModel).GetMethod(method, Private).Invoke(m, null); }
    }
}
