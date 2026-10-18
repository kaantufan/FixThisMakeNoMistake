using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Opt-in model/API fixtures for the V5.2 wave progression. Run only from the
    /// root's isolated native smoke; this does not claim OS input or campaign play.
    /// </summary>
    public static class PanicWaveV52ModelChecks
    {
        static readonly int[] Waves = { 1, 2, 5, 10, 20, 50 };
        static readonly DisasterType[] Hazards = {
            DisasterType.Earthquake, DisasterType.Flood, DisasterType.Fire, DisasterType.Storm
        };
        const float Epsilon = .001f;
        static readonly MethodInfo Calculate = typeof(PanicModel).GetMethod(
            "CalculateDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo SecondarySeed = typeof(PanicModel).GetField(
            "secondarySeedV5", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) return;
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride))
            {
                check("v5.2.wave.requires-isolated-profile", false,
                    "NOT_RUN: root must set a fresh isolated ScorePathOverride before these model fixtures.");
                return;
            }
            check("v5.2.wave.model-fixture-scope", Calculate != null && SecondarySeed != null,
                "Explicit Wave, house, bag and RNG fixtures; actual model evaluation/resolution. No controller, OS input, human balance or uninterrupted campaign claim.");
            if (Calculate == null || SecondarySeed == null) return;

            float[] factors = { 1f, 1.08f, 1.32f, 1.72f, 2.17f, 3.52f };
            float previousPresentation = 0f;
            for (int i = 0; i < Waves.Length; i++)
            {
                int wave = Waves[i];
                float damage = PanicWaveDifficultyV52.DamageMultiplierForWave(wave);
                float presentation = PanicWaveDifficultyV52.PresentationMultiplierForWave(wave);
                check("v5.2.curve.checkpoint." + wave, Near(damage, factors[i]),
                    "Expected damage factor " + factors[i] + "; actual " + damage + ".");
                check("v5.2.presentation.bounded-rise." + wave,
                    Finite(presentation) && presentation >= 1f && presentation <= 1.6501f &&
                    (i == 0 ? Near(presentation, 1f) : presentation > previousPresentation),
                    "Shared wave progression remains increasing at checkpoints and bounded to 1..1.65.");
                previousPresentation = presentation;
            }
            check("v5.2.curve.nonpositive-wave-floor",
                Near(PanicWaveDifficultyV52.DamageMultiplierForWave(0), 1f) &&
                Near(PanicWaveDifficultyV52.DamageMultiplierForWave(int.MinValue), 1f) &&
                Near(PanicWaveDifficultyV52.PresentationMultiplierForWave(int.MinValue), 1f),
                "Invalid nonpositive fixture values cannot create negative severity or overflow.");

            foreach (DisasterType hazard in Hazards)
            {
                CheckFirstWaveBaseline(check, hazard);
                CheckClosedUtilityProgression(check, hazard);
                CheckOpenUtilityProgression(check, hazard);
                CheckMitigation(check, hazard);
                CheckMaximumWave(check, hazard);
            }
            CheckSecondaryMemoization(check);
            CheckUnscaledModes(check);
            CheckResolutionAndDeveloperSkip(check);
        }

        static PanicModel Campaign(DisasterType hazard, bool closeUtilities)
        {
            var model = new PanicModel();
            model.StartRun(false);
            model.Disaster = hazard;
            if (closeUtilities) CloseUtilities(model);
            return model;
        }

        static void CloseUtilities(PanicModel model)
        {
            model.UtilitySafety.Complete("gasoff");
            model.UtilitySafety.Complete("wateroff");
            model.UtilitySafety.Complete("poweroff");
        }

        static void Evaluate(PanicModel model) { Calculate.Invoke(model, null); }

        static void CheckFirstWaveBaseline(Action<string, bool, string> check, DisasterType hazard)
        {
            // Frozen oracle from the pre-V5.2 wave1 hash with seed0x20261016,
            // the real bookshelf BaseRisk and the retained closed-utility factors.
            // Constants avoid calculating the expectation using the new hash code.
            float[] legacy = { 7.3749268f, 1.7466037f, 4.7603611f, 6.1739470f };
            var model = Campaign(hazard, true);
            model.Items.RemoveAll(item => item.Id != "bookshelf");
            Evaluate(model);
            check("v5.2.damage.first-wave-baseline." + hazard,
                model.Wave == 1 && Math.Abs(model.TotalDamage - legacy[(int)hazard]) < .0001f && SoundTotals(model),
                "Frozen historical wave1 single-bookshelf expectation " + legacy[(int)hazard] +
                "; actual " + model.TotalDamage + ". Constant wave1 salt retains the initial damage variation.");
        }

        static void CheckClosedUtilityProgression(Action<string, bool, string> check, DisasterType hazard)
        {
            var model = Campaign(hazard, true);
            float first = 0f, previous = -1f;
            float[] previousItems = null;
            foreach (int wave in Waves)
            {
                model.Wave = wave;
                Evaluate(model);
                if (wave == 1) first = model.TotalDamage;
                float expected = Mathf.Min(100f,
                    first * PanicWaveDifficultyV52.DamageMultiplierForWave(wave));
                bool itemMonotonic = previousItems == null || model.Items.Select(
                    (item, index) => item.Contribution + Epsilon >= previousItems[index]).All(ok => ok);
                bool growsUntilSaturated = previous < 0f || previous >= 100f - Epsilon ||
                    model.TotalDamage > previous + Epsilon;
                check("v5.2.damage.same-house." + hazard + "." + wave,
                    Near(model.TotalDamage, expected) && model.TotalDamage + Epsilon >= previous &&
                    growsUntilSaturated && itemMonotonic && model.SecondaryDamageV5 == 0f && SoundTotals(model),
                    "Same run/house/hazard/protection, all utilities closed; total " + model.TotalDamage +
                    ", expected capped baseline scaling " + expected + ". Item shares cannot fall as severity rises.");
                previous = model.TotalDamage;
                previousItems = model.Items.Select(item => item.Contribution).ToArray();
            }
        }

        static void CheckOpenUtilityProgression(Action<string, bool, string> check, DisasterType hazard)
        {
            var model = Campaign(hazard, false);
            SecondarySeed.SetValue(model, 11u);
            float firstPrimary = 0f, previousPrimary = -1f;
            foreach (int wave in Waves)
            {
                model.Wave = wave;
                Evaluate(model);
                float primary = model.TotalDamage - model.SecondaryDamageV5;
                if (wave == 1) firstPrimary = primary;
                float expected = Mathf.Min(100f,
                    firstPrimary * PanicWaveDifficultyV52.DamageMultiplierForWave(wave));
                int rolls = model.SecondaryRollCountV5;
                float total = model.TotalDamage;
                string target = model.SecondaryEventsV5.Count == 0 ? string.Empty : model.SecondaryEventsV5[0].TargetId;
                Evaluate(model);
                string repeatedTarget = model.SecondaryEventsV5.Count == 0 ? string.Empty : model.SecondaryEventsV5[0].TargetId;
                bool secondaryBound = model.SecondaryDamageV5 >= 0f && BoundedSecondary(model) &&
                    model.SecondaryDamageV5 <= 100f - primary + Epsilon && model.SecondaryEventsV5.Count <= 1;
                check("v5.2.damage.open-utilities." + hazard + "." + wave,
                    Near(primary, expected) && primary + Epsilon >= previousPrimary && secondaryBound &&
                    rolls == model.SecondaryRollCountV5 && Near(total, model.TotalDamage) && target == repeatedTarget && SoundTotals(model),
                    "Primary total " + primary + " rises to its 100 cap. One memoized RNG event adds only kind-specific gas6..10/pipe4..7/electric1.2..2.8 points; complete totals need not rise when an event disappears.");
                previousPrimary = primary;
            }
        }

        static void CheckMitigation(Action<string, bool, string> check, DisasterType hazard)
        {
            var bare = Campaign(hazard, true);
            var prepared = Campaign(hazard, true);
            bare.Wave = prepared.Wave = 10;
            foreach (PanicItem item in prepared.Items)
            {
                if (item.Kind != ItemKind.Furniture) item.Tier = 2;
                string action = PanicItemActionRules.PrimaryAction(item);
                if (action == "anchorwall" || action == "anchorfloor")
                {
                    item.Anchored = item.AnchorOwned = true;
                    item.AnchorSurface = action == "anchorwall" ? "wall" : "floor";
                }
            }
            prepared.TreeCount = 3;
            prepared.DrainCount = 2;
            prepared.FireToolCount = 2;
            Evaluate(bare); Evaluate(prepared);
            check("v5.2.protection.prepared-house." + hazard,
                prepared.TotalDamage + Epsilon < bare.TotalDamage && SoundTotals(prepared),
                "Explicit prepared-house fixture at wave10: T2 shell, eligible anchors, three tree rows, two drains, two fire tools. " +
                "No unreachable arbitrary-furniture fire/flood flags. Bare " + bare.TotalDamage + "; prepared " + prepared.TotalDamage + ".");

            // A single genuine model item isolates mitigation at wave50, avoiding a
            // whole-house 100% saturation that legitimately hides marginal benefit.
            var isolatedBare = Campaign(hazard, true);
            var isolatedPrepared = Campaign(hazard, true);
            string targetId = hazard == DisasterType.Earthquake ? "bookshelf" : "roof";
            isolatedBare.Items.RemoveAll(item => item.Id != targetId);
            isolatedPrepared.Items.RemoveAll(item => item.Id != targetId);
            isolatedBare.Wave = isolatedPrepared.Wave = 50;
            var target = isolatedPrepared.Items[0];
            if (target.Kind == ItemKind.Furniture)
            { target.Anchored = target.AnchorOwned = true; target.AnchorSurface = "floor"; }
            else target.Tier = 2;
            Evaluate(isolatedBare); Evaluate(isolatedPrepared);
            check("v5.2.protection.high-wave-target." + hazard,
                isolatedPrepared.TotalDamage > 0f && isolatedPrepared.TotalDamage + Epsilon < isolatedBare.TotalDamage &&
                SoundTotals(isolatedBare) && SoundTotals(isolatedPrepared),
                "Wave50, same real item and hazard: valid anchor or T2 mitigation still reduces damage; no prices, input or construction completion claim.");
        }

        static void CheckMaximumWave(Action<string, bool, string> check, DisasterType hazard)
        {
            var model = Campaign(hazard, true);
            model.Wave = int.MaxValue;
            Evaluate(model);
            check("v5.2.damage.maximum-wave." + hazard,
                Finite(model.DisasterSeverityV52) && model.DisasterSeverityV52 > 3.52f &&
                Finite(model.DisasterPresentationScaleV52) && model.DisasterPresentationScaleV52 >= 1f &&
                model.DisasterPresentationScaleV52 <= 1.6501f && Near(model.TotalDamage, 100f) && SoundTotals(model),
                "Explicit int.MaxValue wave fixture: finite severity, bounded presentation, proportional contribution sum100 and per-item display in 0..100.");
        }

        static void CheckSecondaryMemoization(Action<string, bool, string> check)
        {
            PanicModel incident = null;
            for (uint seed = 1; seed <= 64 && incident == null; seed++)
            {
                var candidate = Campaign(DisasterType.Flood, false);
                candidate.Wave = 5;
                SecondarySeed.SetValue(candidate, seed);
                Evaluate(candidate);
                if (candidate.SecondaryEventsV5.Count == 1 && candidate.SecondaryDamageV5 > 0f) incident = candidate;
            }
            check("v5.2.secondary.actual-bounded-incident", incident != null &&
                incident.SecondaryDamageV5 > 0 && BoundedSecondary(incident) && SoundTotals(incident),
                "Explicit deterministic seed search over at most64 fixtures confirms an actual event rather than only empty-event bounds.");
            if (incident == null) return;
            int rolls = incident.SecondaryRollCountV5;
            float total = incident.TotalDamage;
            float secondary = incident.SecondaryDamageV5;
            PanicSecondaryEventV5 actual = incident.SecondaryEventsV5[0];
            for (int i = 0; i < 4; i++) Evaluate(incident);
            check("v5.2.secondary.memoized-no-accumulation", rolls == incident.SecondaryRollCountV5 &&
                Near(total, incident.TotalDamage) && Near(secondary, incident.SecondaryDamageV5) &&
                incident.SecondaryEventsV5.Count == 1 && actual.TargetId == incident.SecondaryEventsV5[0].TargetId &&
                actual.Kind == incident.SecondaryEventsV5[0].Kind && SoundTotals(incident),
                "Four repeated evaluations neither reroll nor accumulate secondary damage; the chosen target and kind remain fixed.");
            CloseUtilities(incident);
            Evaluate(incident);
            check("v5.2.secondary.closed-prevents-existing-event", incident.SecondaryDamageV5 == 0f &&
                incident.SecondaryEventsV5.Count == 0 && incident.SecondaryRollCountV5 == rolls && SoundTotals(incident),
                "Closing the matching real utility flags suppresses the memoized event without another draw.");
        }

        static void CheckUnscaledModes(Action<string, bool, string> check)
        {
            foreach (string mode in new[] { "simulation", "tutorial", "short-flow" })
            {
                var model = new PanicModel();
                bool started;
                if (mode == "simulation") started = model.StartSimulation(DisasterType.Earthquake);
                else if (mode == "tutorial") { model.StartRun(true); started = model.IsTutorial; }
                else started = model.StartJuryDemo();
                CloseUtilities(model);
                bool stable = started;
                foreach (DisasterType hazard in Hazards)
                {
                    model.Disaster = hazard;
                    model.Wave = 1; Evaluate(model);
                    float baseline = model.TotalDamage;
                    model.Wave = 50; Evaluate(model);
                    stable &= Near(model.DisasterSeverityV52, 1f) && Near(model.DisasterPresentationScaleV52, 1f) &&
                        Near(model.TotalDamage, baseline) && SoundTotals(model);
                }
                check("v5.2.mode.no-campaign-escalation." + mode,
                    stable && !model.TryDeveloperSkipToDisasterV51(),
                    "A deliberately altered Wave fixture cannot scale this mode's damage/presentation or enable campaign F3.");
            }
        }

        static void CheckResolutionAndDeveloperSkip(Action<string, bool, string> check)
        {
            check("v5.2.rules.exact-loss-boundary", !PanicModel.DamageCausesLoss(74.99f) &&
                PanicModel.DamageCausesLoss(75f) && PanicModel.NormalPrecautionSeconds == 90f &&
                PanicModel.NormalBagSeconds == 40f,
                "75% inclusive loss and90+40s final timing policy.");

            var unsafeHouse = Campaign(DisasterType.Earthquake, true);
            unsafeHouse.Wave = 50;
            unsafeHouse.PackedWater = unsafeHouse.PackedFood = 1;
            int budget = unsafeHouse.Budget;
            bool entered = unsafeHouse.TryEnterShelterV5(unsafeHouse.RequiredShelterV5);
            unsafeHouse.Tick(8f);
            bool realAftermath = unsafeHouse.Phase == PanicPhase.Aftermath && unsafeHouse.ReachedShelterV5 &&
                unsafeHouse.TotalDamage >= PanicModel.DamageLossLimit && unsafeHouse.PackedWater == 0 && unsafeHouse.PackedFood == 0;
            unsafeHouse.Tick(4f);
            check("v5.2.rules.high-wave-normal-loss", entered && realAftermath &&
                unsafeHouse.Phase == PanicPhase.GameOver && unsafeHouse.FailureReason.Contains("%75") &&
                unsafeHouse.Score == 0 && unsafeHouse.CompletedWaves == 0 && unsafeHouse.LastReward == 0 && unsafeHouse.Budget == budget,
                "Typed model shelter and a real full bag cannot rescue a50th-wave house over75%; actual resolution consumes supplies and applies normal loss, without developer options.");

            var safeTarget = Campaign(DisasterType.Earthquake, true);
            safeTarget.Items.RemoveAll(item => item.Id != "bookshelf");
            safeTarget.Wave = 50;
            safeTarget.PackedWater = safeTarget.PackedFood = 1;
            bool safeEntry = safeTarget.TryEnterShelterV5(safeTarget.RequiredShelterV5);
            safeTarget.Tick(8f); safeTarget.Tick(4f);
            check("v5.2.rules.high-wave-below-limit-success", safeEntry &&
                safeTarget.TotalDamage > 0f && safeTarget.TotalDamage < PanicModel.DamageLossLimit &&
                safeTarget.Phase == PanicPhase.Success && safeTarget.CompletedWaves == 1 &&
                safeTarget.LastReward > 0 && safeTarget.LastWaveScore > 0 && !safeTarget.CheatedRun,
                "Explicit single real low-risk item fixture at wave50 demonstrates successful normal resolution below the same threshold; no campaign survival claim.");

            var skipped = Campaign(DisasterType.Earthquake, true);
            skipped.Wave = 50;
            skipped.PackedWater = skipped.PackedFood = 1;
            int skippedBudget = skipped.Budget;
            bool started = skipped.TryDeveloperSkipToDisasterV51();
            float time = skipped.PhaseTime;
            skipped.TryDeveloperSkipToDisasterV51();
            bool noRestart = skipped.PhaseTime == time && skipped.Wave == 50;
            skipped.Tick(8f);
            float actualDamage = skipped.TotalDamage;
            skipped.TryDeveloperSkipToDisasterV51();
            skipped.Tick(4f);
            check("v5.2.devskip.high-wave-real-unrewarded-result", started && noRestart &&
                skipped.Phase == PanicPhase.Inspection && actualDamage >= PanicModel.DamageLossLimit &&
                Near(skipped.TotalDamage, actualDamage) && skipped.PackedWater == 0 && skipped.PackedFood == 0 &&
                !skipped.ReachedShelterV5 && skipped.CheatedRun && skipped.DeveloperWaveSkipProtectionV51 &&
                skipped.CompletedWaves == 0 && skipped.Score == 0 && skipped.LastWaveScore == 0 &&
                skipped.LastReward == 0 && skipped.Budget == skippedBudget && !skipped.DeveloperNoLoss,
                "F3 at wave50 preserves escalated damage and consumption, invents no shelter admission, avoids GameOver only for its protected wave and grants no award.");
            skipped.SetWaveLedgerOpenV56(true); skipped.SetWaveLedgerOpenV56(false); skipped.FinishInspection();
            check("v5.2.devskip.high-wave-protection-scope", skipped.Wave == 51 &&
                skipped.Phase == PanicPhase.Preparing && !skipped.DeveloperWaveSkipProtectionV51 && skipped.CheatedRun,
                "The next wave clears one-wave protection while retaining sticky score exclusion.");
        }

        static bool BoundedSecondary(PanicModel model)
        {
            if(model.SecondaryEventsV5.Count>1||model.SecondaryDamageV5<0)return false;
            foreach(var e in model.SecondaryEventsV5)
            {
                float maximum=e.Kind==PanicSecondaryEventKindV5.GasFlare?10:e.Kind==PanicSecondaryEventKindV5.PipeLeak?7:2.8f;
                if(e.DamagePoints<0||e.DamagePoints>maximum+.0001f)return false;
            }
            return Near(model.SecondaryDamageV5,model.SecondaryEventsV5.Sum(e=>e.DamagePoints));
        }
        static bool SoundTotals(PanicModel model)
        {
            return model != null && Finite(model.TotalDamage) && model.TotalDamage >= 0f && model.TotalDamage <= 100.001f &&
                Math.Abs(model.Items.Sum(item => item.Contribution) - model.TotalDamage) < .002f &&
                model.Items.All(item => Finite(item.Contribution) && item.Contribution >= 0f &&
                    Finite(item.Damage) && item.Damage >= 0f && item.Damage <= 100.001f);
        }
        static bool Near(float a, float b) { return Math.Abs(a - b) < Epsilon; }
        static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
