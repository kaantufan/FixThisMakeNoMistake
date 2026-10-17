using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Opt-in root-native model checks. Explicit state and memo fixtures are labelled below.
    /// No world/controller/OS/render/listening or human acceptance is claimed by this helper.
    /// </summary>
    public static class PanicCompoundChecksV54
    {
        const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        static int fixtureIndex;
        static string fixtureFolder;

        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) return;
            if (string.IsNullOrWhiteSpace(PanicModel.ScorePathOverride))
            {
                check("v5.4.compound.isolated-profile", false, "NOT_RUN: root must supply a fresh isolated ScorePathOverride before fixtures.");
                return;
            }
            string previous = PanicModel.ScorePathOverride;
            string parent = Path.GetDirectoryName(Path.GetFullPath(previous));
            fixtureFolder = Path.Combine(parent, "compound-model-fixtures-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(fixtureFolder);
            fixtureIndex = 0;
            try
            {
                check("v5.4.compound.isolated-profile", true, "Model/API fixtures only. Each model gets a fresh score target below the root's isolated evidence directory.");
                CheckSelection(check);
                CheckDamageComposition(check);
                CheckShelterAndOnce(check);
                CheckSingleModes(check);
            }
            catch (Exception ex)
            {
                check("v5.4.compound.exception", false, ex.ToString());
            }
            finally
            {
                PanicModel.ScorePathOverride = previous;
                fixtureFolder = null;
            }
        }

        static PanicModel NewModel()
        {
            PanicModel.ScorePathOverride = Path.Combine(fixtureFolder, "model-" + (++fixtureIndex) + ".json");
            return new PanicModel();
        }

        static void CheckSelection(Action<string, bool, string> check)
        {
            var campaign = NewModel(); campaign.StartRun(false);
            Set(campaign, "compoundSeedV54", 0x54123456u);
            campaign.Items = new List<PanicItem> { Item("qa_natural_rng_low_risk", ItemKind.Furniture, "chair", true) };
            CloseUtilities(campaign);
            bool first = campaign.Wave == 1 && campaign.Disaster == DisasterType.Earthquake && campaign.ActiveDisasterMaskV54 == 1 && campaign.ActiveDisasterCountV54 == 1;
            int union = 0, counts = 0, firstNineUnion = 0;
            bool valid = true, stable = true, actualProgression = true;
            for (int wave = 1; wave <= 128; wave++)
            {
                int mask = campaign.ActiveDisasterMaskV54;
                int count = campaign.ActiveDisasterCountV54;
                var primary = campaign.Disaster;
                valid &= mask > 0 && (mask & ~15) == 0 && Count(mask) == count && count >= 1 && count <= 3 && campaign.HasActiveDisasterV54(primary);
                if (wave > 1) { union |= mask; counts |= 1 << count; if (wave <= 9) firstNineUnion |= mask; }
                float clock = campaign.TimeRemaining;
                for (int read = 0; read < 32; read++)
                {
                    string label = campaign.ActiveDisasterLabelV54;
                    stable &= campaign.ActiveDisasterMaskV54 == mask && campaign.ActiveDisasterCountV54 == count && campaign.Disaster == primary && campaign.TimeRemaining == clock && !string.IsNullOrEmpty(label);
                }
                if (wave == 2)
                {
                    campaign.SetPaused(true); campaign.Tick(30); campaign.SetPaused(false);
                    stable &= campaign.TimeRemaining == clock && campaign.ActiveDisasterMaskV54 == mask;
                    Call(campaign, "SelectCompoundDisastersV54");
                    stable &= campaign.ActiveDisasterMaskV54 == mask && campaign.Disaster == primary;
                }
                campaign.PackedWater = campaign.PackedFood = 1;
                actualProgression &= campaign.TryEnterShelterV5(campaign.RequiredShelterV5);
                campaign.Tick(15f);
                actualProgression &= campaign.Phase == PanicPhase.Inspection && campaign.CompletedWaves == wave;
                campaign.FinishInspection();
                actualProgression &= campaign.Phase == PanicPhase.Preparing && campaign.Wave == wave + 1;
                if (!actualProgression) break;
            }
            check("v5.4.rng.wave1-single-earthquake", first, "Actual normal StartRun path.");
            check("v5.4.rng.wave2plus-four-type-pool", union == 15 && (firstNineUnion & 8) != 0, "128 real resolver/reward/next-wave transitions with one explicitly secured low-risk item and packed supplies; fixed RNG seed fixture. Storm is observed before wave10.");
            check("v5.4.rng.unique-valid-1to3-hazards", valid && counts == 14 && actualProgression, "Bit membership excludes duplicate types; fixed campaign observes singles, doubles and triples. Natural campaign state transitions, explicit low-risk inventory fixture.");
            check("v5.4.rng.stable-read-pause-and-same-wave-selection", stable, "Labels/membership reads and paused Tick do not reroll; repeated same-wave selector yields identical mask/primary.");
            check("v5.4.rng.invalid-enum-membership-rejected", !campaign.HasActiveDisasterV54((DisasterType)(-1)) && !campaign.HasActiveDisasterV54((DisasterType)4), "No shift wrapping for invalid types.");

            var seeds = new HashSet<uint>();
            bool nonzero = true;
            for (int i = 0; i < 8; i++)
            {
                var run = NewModel(); run.StartRun(false);
                uint seed = (uint)Get(run, "compoundSeedV54"); seeds.Add(seed); nonzero &= seed != 0;
            }
            check("v5.4.rng.actual-per-run-seeds-differ", nonzero && seeds.Count > 1, "Observed private per-run seed state from eight actual starts; no claim about cryptographic security or statistical distribution.");
        }

        static void CheckDamageComposition(Action<string, bool, string> check)
        {
            const int all = 15;
            var actual = Fixture(DisasterType.Storm, all, 50, true, false, true);
            float[] expectedItems = new float[actual.Items.Count];
            float[] expectedHazards = new float[4];
            float raw = 0;
            bool rawAboveCap = true;
            for (int n = 0; n < 4; n++)
            {
                var single = Fixture((DisasterType)n, 1 << n, 50, true, false, true);
                Call(single, "CalculateSingleDisasterDamageV54", true);
                rawAboveCap &= single.TotalDamage > 100;
                float weight = n == (int)DisasterType.Storm ? 1f : .45f;
                expectedHazards[n] = single.TotalDamage * weight; raw += expectedHazards[n];
                for (int i = 0; i < single.Items.Count; i++) expectedItems[i] += single.Items[i].Contribution * weight;
            }
            Call(actual, "CalculateDamage");
            float scale = raw > 100 ? 100 / raw : 1;
            bool shares = true;
            float itemSum = 0, hazardSum = 0;
            for (int i = 0; i < actual.Items.Count; i++)
            {
                var item = actual.Items[i]; itemSum += item.Contribution;
                shares &= Finite(item.Contribution) && item.Contribution >= 0 && Finite(item.Damage) && item.Damage >= 0 && item.Damage <= 100;
                shares &= Near(item.Contribution, expectedItems[i] * scale);
            }
            for (int n = 0; n < 4; n++)
            { float value = actual.HazardDamageContributionV54((DisasterType)n); hazardSum += value; shares &= Near(value, expectedHazards[n] * scale); }
            check("v5.4.damage.raw-weighted-before-one-cap", rawAboveCap && Near(actual.TotalDamage, 100) && shares, "Explicit all-four, wave50, roof-heavy fixture with utilities isolated. Expected composition samples raw single helper, then weights primary1/secondary0.45 and applies one cap. Catches per-hazard premature100 caps.");
            check("v5.4.damage.contribution-sums-primary-restored", Near(itemSum, actual.TotalDamage) && Near(hazardSum, actual.TotalDamage) && actual.Disaster == DisasterType.Storm && actual.ActiveDisasterMaskV54 == all, "Actual item and per-hazard contribution sums agree; temporary helper disaster selection cannot leak out.");

            var plain = Fixture(DisasterType.Earthquake, all, 2, false, false, true);
            var secured = Fixture(DisasterType.Earthquake, all, 2, false, true, true);
            Call(plain, "CalculateDamage"); Call(secured, "CalculateDamage");
            bool eachReduced = plain.TotalDamage < 100 && secured.TotalDamage > 0 && secured.TotalDamage < plain.TotalDamage;
            for (int n = 0; n < 4; n++) eachReduced &= secured.HazardDamageContributionV54((DisasterType)n) < plain.HazardDamageContributionV54((DisasterType)n);
            check("v5.4.damage.real-mitigation-still-reduces-each-hazard", eachReduced, "Same IDs, stable damage seed, wave and mask; actual anchor/tier/flood/fire guards changed in explicit fixture. Below cap, preventing saturation from hiding mitigation.");

            var repeated = Fixture(DisasterType.Fire, 7, 2, false, false, false);
            Call(repeated, "CalculateDamage"); int rolls = repeated.SecondaryRollCountV5;
            float damage = repeated.TotalDamage, secondary = repeated.SecondaryDamageV5;
            int events = repeated.SecondaryEventsV5.Count;
            Call(repeated, "CalculateDamage");
            check("v5.4.damage.secondary-rolls-once-per-wave-type", rolls == 3 && repeated.SecondaryRollCountV5 == rolls && Near(repeated.TotalDamage, damage) && Near(repeated.SecondaryDamageV5, secondary) && repeated.SecondaryEventsV5.Count == events, "Actual open-utility per-hazard memoized rolls evaluated twice; outcome points/membership remain unchanged.");

            var memo = Fixture(DisasterType.Storm, all, 50, true, false, false);
            var cache = (Dictionary<string, PanicSecondaryEventV5>)Get(memo, "secondaryRollsV5");
            for (int n = 0; n < 4; n++) cache[memo.Wave + ":" + n] = new PanicSecondaryEventV5(PanicSecondaryEventKindV5.GasFlare, "qa_compound_bookshelf", 2f, "QA fixed memoized small event");
            Call(memo, "CalculateDamage");
            float eventSum = 0;
            foreach (var incident in memo.SecondaryEventsV5) eventSum += incident.DamagePoints;
            check("v5.4.damage.deferred-raw-keeps-small-secondary-events", Near(memo.TotalDamage, 100) && memo.SecondaryEventsV5.Count == 4 && memo.SecondaryDamageV5 > 0 && Near(eventSum, memo.SecondaryDamageV5), "Explicit memo cache fixture: four positive2-point events, each hazard raw>100. The small event must enter raw weighting before the unified cap, rather than vanish at the old per-hazard100 limit.");
            check("v5.4.damage.loss-boundary-stays75", !PanicModel.DamageCausesLoss(74.999f) && PanicModel.DamageCausesLoss(75) && PanicModel.DamageCausesLoss(100), "No loss-tolerance change.");
        }

        static void CheckShelterAndOnce(Action<string, bool, string> check)
        {
            var wave = Fixture(DisasterType.Fire, 6, 2, false, true, true);
            wave.Items = new List<PanicItem> { Item("qa_once", ItemKind.Furniture, "chair", true) };
            wave.PackedWater = wave.PackedFood = 3;
            int budget = wave.Budget;
            bool rejectsBasement = !wave.TryEnterShelterV5(PanicShelterKindV5.Basement) && !wave.ReachedShelterV5 && wave.Phase == PanicPhase.Preparing;
            bool correctEntry = wave.TryEnterShelterV5(PanicShelterKindV5.PanicRoom);
            check("v5.4.shelter.secondary-flood-requires-real-upper-admission", rejectsBasement && correctEntry && wave.ReachedShelterV5 && wave.RequiredShelterV5 == PanicShelterKindV5.PanicRoom && wave.EnteredShelterV5 == PanicShelterKindV5.PanicRoom && wave.Disaster == DisasterType.Fire, "Model admission boundary only; physical controller/floor/ray/path are root's separate world smoke.");
            wave.Tick(8f); float damage = wave.TotalDamage;
            Call(wave, "ResolveDisaster"); Call(wave, "ResolveDisaster");
            check("v5.4.once.compound-resolves-and-consumes-once", wave.Phase == PanicPhase.Aftermath && wave.PackedWater == 2 && wave.PackedFood == 2 && Near(wave.TotalDamage, damage) && damage > 0 && wave.ActiveDisasterMaskV54 == 6, "Two active hazards and repeated resolver calls still consume exactly one of each supply.");
            wave.Tick(4f);
            int reward = wave.LastReward; long score = wave.Score;
            bool firstAward = wave.Phase == PanicPhase.Success && wave.CompletedWaves == 1 && reward > 0 && score > 0 && wave.Budget == budget + reward;
            Call(wave, "AwardSuccessfulWaveOnce"); Call(wave, "AwardSuccessfulWaveOnce");
            check("v5.4.once.compound-success-awards-once", firstAward && wave.CompletedWaves == 1 && wave.Budget == budget + reward && wave.Score == score && wave.LastReward == reward, "Normal production success path; repeated guarded award calls cannot multiply reward by hazard count.");

            var skipped = Fixture(DisasterType.Storm, 10, 2, false, true, true);
            skipped.PackedWater = skipped.PackedFood = 3;
            int originalBudget = skipped.Budget;
            bool started = skipped.TryDeveloperSkipToDisasterV51();
            float clock = skipped.PhaseTime;
            skipped.TryDeveloperSkipToDisasterV51(); skipped.TryDeveloperSkipToDisasterV51();
            bool noRestart = skipped.PhaseTime == clock && skipped.ActiveDisasterMaskV54 == 10;
            skipped.Tick(8f); float realDamage = skipped.TotalDamage;
            skipped.TryDeveloperSkipToDisasterV51(); skipped.TryDeveloperSkipToDisasterV51(); skipped.Tick(4f);
            check("v5.4.once.F3-compound-has-one-consumption-no-award", started && noRestart && skipped.Phase == PanicPhase.Inspection && skipped.PackedWater == 2 && skipped.PackedFood == 2 && realDamage > 0 && skipped.TotalDamage == realDamage && skipped.CompletedWaves == 0 && skipped.LastReward == 0 && skipped.LastWaveScore == 0 && skipped.Score == 0 && skipped.Budget == originalBudget && skipped.CheatedRun && !skipped.ReachedShelterV5, "Actual F3 action, explicit two-hazard inventory/mask fixture. Unsheltered status remains honest; protection does not fake entry or success.");
            skipped.FinishInspection();
            check("v5.4.once.F3-protection-clears-next-wave", skipped.Wave == 3 && skipped.Phase == PanicPhase.Preparing && !skipped.DeveloperWaveSkipProtectionV51 && skipped.CheatedRun && skipped.CompletedWaves == 0, "One wave protection ends; sticky score exclusion remains.");
        }

        static void CheckSingleModes(Action<string, bool, string> check)
        {
            for (int n = 0; n < 4; n++)
            {
                var type = (DisasterType)n; var sim = NewModel(); bool started = sim.StartSimulation(type);
                Set(sim, "compoundMaskV54", 15); Set(sim, "compoundWaveV54", 1); Set(sim, "compoundPrimaryV54", type);
                bool single = sim.ActiveDisasterMaskV54 == (1 << n) && sim.ActiveDisasterCountV54 == 1 && sim.DisasterSeverityV52 == 1 && sim.DisasterPresentationScaleV52 == 1;
                sim.Items = new List<PanicItem> { Item("qa_sim_single", ItemKind.Furniture, "chair", true) }; CloseUtilities(sim);
                sim.TryEnterShelterV5(sim.RequiredShelterV5); sim.Tick(12f);
                float secondaryHazards = 0;
                for (int other = 0; other < 4; other++) if (other != n) secondaryHazards += sim.HazardDamageContributionV54((DisasterType)other);
                check("v5.4.single.simulation." + type, started && single && sim.SimulationComplete && sim.Phase == PanicPhase.Inspection && sim.Wave == 1 && sim.Score == 0 && sim.CompletedWaves == 0 && sim.LastReward == 0 && secondaryHazards == 0 && sim.HazardDamageContributionV54(type) > 0, "Even an explicit stale all-four cache fixture cannot leak compound membership into a selected single simulation.");
            }
            var tutorial = NewModel(); tutorial.StartRun(true); Set(tutorial, "compoundMaskV54", 15);
            check("v5.4.single.tutorial", tutorial.IsTutorial && tutorial.ActiveDisasterCountV54 == 1 && tutorial.ActiveDisasterMaskV54 == 1 && tutorial.DisasterSeverityV52 == 1, "Actual tutorial start; stale cache fixture ignored.");
            var jury = NewModel(); jury.StartJuryDemo(); Set(jury, "compoundMaskV54", 15);
            jury.SetJuryPresentation(PanicPhase.Disaster, DisasterType.Flood, 4, "QA selected single", string.Empty);
            check("v5.4.single.short-flow", jury.IsJuryDemo && jury.ActiveDisasterCountV54 == 1 && jury.ActiveDisasterMaskV54 == 2 && jury.DisasterSeverityV52 == 1 && jury.DisasterPresentationScaleV52 == 1, "Actual short-flow API selects one Flood vignette; no campaign compound mask leaks.");
        }

        static PanicModel Fixture(DisasterType primary, int mask, int wave, bool high, bool secured, bool closeUtilities)
        {
            var model = NewModel(); model.StartRun(false); model.Wave = wave; model.Disaster = primary;
            Set(model, "compoundMaskV54", mask); Set(model, "compoundWaveV54", wave); Set(model, "compoundPrimaryV54", primary);
            Set(model, "secondarySeedV5", 0x54115522u);
            model.Items = new List<PanicItem>();
            if (high) for (int i = 0; i < 20; i++) model.Items.Add(Item("qa_compound_roof_" + i, ItemKind.Roof, "roof", secured));
            else
            {
                model.Items.Add(Item("qa_compound_roof", ItemKind.Roof, "roof", secured));
                model.Items.Add(Item("qa_compound_window", ItemKind.Window, "window", secured));
            }
            model.Items.Add(Item("qa_compound_bookshelf", ItemKind.Furniture, "bookshelf", secured));
            if (closeUtilities) CloseUtilities(model);
            return model;
        }
        static PanicItem Item(string id, ItemKind kind, string art, bool secured)
        { return new PanicItem { Id = id, Label = "QA compound fixture", Kind = kind, ArtId = art, Installed = true, Movable = kind == ItemKind.Furniture, Tier = secured ? 2 : 0, Anchored = secured && kind == ItemKind.Furniture, AnchorSurface = secured ? "floor" : string.Empty, FloodProtected = secured, FireProtected = secured, Size = Vector3.one }; }
        static void CloseUtilities(PanicModel model)
        { model.UtilitySafety.Complete("gasoff"); model.UtilitySafety.Complete("wateroff"); model.UtilitySafety.Complete("poweroff"); }
        static bool Near(float a, float b) { return Finite(a) && Finite(b) && Mathf.Abs(a - b) < .002f; }
        static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        static int Count(int mask) { int count = 0; for (int n = 0; n < 4; n++) if ((mask & (1 << n)) != 0) count++; return count; }
        static object Get(PanicModel model, string field)
        { var f = typeof(PanicModel).GetField(field, PrivateInstance); if (f == null) throw new MissingFieldException(field); return f.GetValue(model); }
        static void Set(PanicModel model, string field, object value)
        { var f = typeof(PanicModel).GetField(field, PrivateInstance); if (f == null) throw new MissingFieldException(field); f.SetValue(model, value); }
        static void Call(PanicModel model, string name, params object[] arguments)
        { var method = typeof(PanicModel).GetMethod(name, PrivateInstance); if (method == null) throw new MissingMethodException(name); method.Invoke(model, arguments); }
    }
}
