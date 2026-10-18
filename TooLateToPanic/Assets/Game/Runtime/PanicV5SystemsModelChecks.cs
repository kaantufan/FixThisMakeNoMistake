using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in model/API checks. Root's isolated native smoke calls Run; never runs on ordinary launch.</summary>
    public static class PanicV5SystemsModelChecks
    {
        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) return;
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride))
            { check("v5.systems.requires-isolated-profile", false, "NOT_RUN: root must set a fresh isolated ScorePathOverride before these fixture checks."); return; }
            check("v5.systems.isolated-profile", true, "Model/API fixtures only; no physical controller, OS input or human acceptance.");

            var model = new PanicModel(); model.StartRun(false);
            var expedition = new PanicExpedition(model);
            check("v5.loot.more-real-sources", expedition.Sources.Count == 14, "8 retained sources + 6 formerly decorative storage sources.");
            check("v5.loot.more-supply-seats", expedition.WorldSupplies.Count == 16, "12 retained seats + 4 extra finite seats; geometry acceptance belongs to root.");
            check("v5.loot.tree-row-minimum", expedition.Sources.SelectMany(s => s.Contents).Where(s => s.Id == "seedling").Sum(s => s.Count) >= 3, "Three mature row packages remain present despite quantity RNG.");
            var source = expedition.Sources[0];
            expedition.ToggleSource(source.Id);
            int materialTotal = PanicExpedition.MaterialIds.Sum(id => expedition.Count(id));
            expedition.ToggleSource(source.Id); expedition.ToggleSource(source.Id);
            check("v5.loot.reopen-does-not-duplicate", PanicExpedition.MaterialIds.Sum(id => expedition.Count(id)) == materialTotal && source.AutoLooted && source.Contents.Count == 0, "Repeated close/open uses the same consumed finite snapshot.");
            int revision = expedition.LootLayoutRevisionV5;
            expedition.OnNewWave();
            check("v5.loot.same-wave-idempotent", expedition.LootLayoutRevisionV5 == revision && source.AutoLooted && source.Contents.Count == 0, "Repeated callback in the same wave cannot refill an emptied source.");
            // Explicit public-state fixture: check restock logic without claiming a physically played wave.
            model.Wave++;
            expedition.OnNewWave();
            check("v5.loot.new-wave-restocks-materials", expedition.StockedWaveV5 == model.Wave && expedition.LootLayoutRevisionV5 == revision + 1 && expedition.Sources.All(s => !s.Open && !s.AutoLooted && s.Contents.Count > 0), "Wave increment fixture; actual OnNewWave restores finite construction stock once.");
            expedition.OnNewWave();
            check("v5.loot.new-wave-callback-once", expedition.LootLayoutRevisionV5 == revision + 1, "A second callback cannot roll a second inventory.");

            var blanket = expedition.FindWorldSupply("supply_blanket_bed");
            bool tookBlanket = expedition.TryCollectWorldSupply(blanket.Id);
            check("v5.blanket.model-collects", tookBlanket && expedition.Count("blanket") == 1 && blanket.Count == 0 && (model.SupportOwnedMask & 8) != 0, "Actual supply transaction and owned support mask; physical bed ray is a separate root check.");
            int blanketSlot = expedition.Inventory.FindIndex(s => s.Id == "blanket" && s.Count > 0);
            check("v5.blanket.packs-support", blanketSlot >= 0 && expedition.PackSlot(blanketSlot) && (model.PackedSupportMask & 8) != 0, "The collected real blanket transfers into a real bag slot.");

            var partialModel = new PanicModel(); partialModel.StartRun(false);
            var partial = new PanicExpedition(partialModel);
            for (int i = 0; i < partial.Inventory.Count; i++) partial.Inventory[i] = new SupplySlot("food", 5);
            partial.Inventory[0] = new SupplySlot("water", 4);
            partial.SynchronizeInventoryModel(); // One canonical20-slot inventory; no second Bag store.
            var water = partial.FindWorldSupply("supply_water_kitchen"); water.Count = 3;
            bool collected = partial.TryCollectWorldSupply(water.Id);
            check("v5.supply.partial-stays-finite", collected && partial.LastWorldSupplyCollectedCountV5 == 1 && water.Count == 2 && partial.Count("water") == 5, "Explicit nearly-full inventory fixture: one actual unit moves, two remain.");
            int remaining = water.Count;
            check("v5.supply.full-capacity-atomic-reject", !partial.TryCollectWorldSupply(water.Id) && water.Count == remaining && partial.LastWorldSupplyCollectedCountV5 == 0, "No capacity changes neither the world stack nor actual inventory.");
            Vector3 seat = new Vector3(3, 4, 5), expected = seat + water.SeatOffsetV5;
            for (int i = 0; i < 20; i++) water.Position = seat;
            check("v5.supply.support-seat-does-not-drift", (water.Position - expected).sqrMagnitude < .000001f && Mathf.Abs(water.SeatOffsetV5.x) <= .0351f && Mathf.Abs(water.SeatOffsetV5.z) <= .0351f, "Repeated support alignment retains one tiny snapshot offset.");

            var flood = new PanicModel(); flood.StartRun(false); flood.Disaster = DisasterType.Flood;
            check("v5.shelter.flood-requires-upper-room", flood.RequiredShelterV5 == PanicShelterKindV5.PanicRoom && !flood.TryEnterShelterV5(PanicShelterKindV5.Basement) && !flood.ReachedShelterV5 && flood.Phase == PanicPhase.Preparing, "Disaster-selection fixture; wrong typed destination commits nothing.");
            bool entered = flood.TryEnterShelterV5(PanicShelterKindV5.PanicRoom);
            check("v5.shelter.correct-entry-once", entered && flood.ReachedShelterV5 && flood.EnteredShelterV5 == PanicShelterKindV5.PanicRoom && flood.Phase == PanicPhase.Disaster && !flood.TryEnterShelterV5(PanicShelterKindV5.PanicRoom), "Existing 8s production resolution begins exactly once; physical gate remains World-owned.");
            var missed = new PanicModel(); missed.StartRun(false); missed.TimeRemaining = .01f; missed.Tick(.02f);
            check("v5.shelter.timeout-no-false-arrival", missed.Phase == PanicPhase.GameOver && !missed.ReachedShelterV5 && missed.EnteredShelterV5 == PanicShelterKindV5.None, "Timer fixture; failure cannot publish an arrival for the cinematic.");
            var protectedMiss = new PanicModel(); protectedMiss.StartRun(false);
            protectedMiss.SetDeveloperOption(PanicDeveloperOption.NoLoss, true); protectedMiss.TimeRemaining = .01f; protectedMiss.Tick(.02f);
            check("v5.shelter.no-loss-no-false-arrival", protectedMiss.Phase == PanicPhase.Disaster && !protectedMiss.ReachedShelterV5 && protectedMiss.CheatedRun, "Developer no-loss still applies the disaster without inventing shelter arrival.");

            var isolated = new PanicModel(); isolated.StartRun(false); isolated.Disaster = DisasterType.Flood;
            isolated.UtilitySafety.Complete("gasoff"); isolated.UtilitySafety.Complete("wateroff"); isolated.UtilitySafety.Complete("poweroff");
            isolated.EnterBunker(); isolated.Tick(8f);
            check("v5.secondary.isolation-prevents-events", isolated.SecondaryDamageV5 == 0 && isolated.SecondaryEventsV5.Count == 0, "All three real utility safety flags prevent their corresponding secondary event.");

            // Seed fixtures remove probabilistic test failure; this never grants stock,
            // changes prices, normal damage, collision or the production loss threshold.
            FieldInfo seed = typeof(PanicModel).GetField("secondarySeedV5", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo calculate = typeof(PanicModel).GetMethod("CalculateDamage", BindingFlags.Instance | BindingFlags.NonPublic);
            PanicModel incident = null;
            for (uint trial = 1; trial <= 64 && incident == null; trial++)
            {
                var candidate = new PanicModel(); candidate.StartRun(false); candidate.Disaster = DisasterType.Flood;
                if (seed != null) seed.SetValue(candidate, trial);
                candidate.EnterBunker(); candidate.Tick(8f);
                if (candidate.SecondaryEventsV5.Count > 0) incident = candidate;
            }
            check("v5.secondary.real-small-event", incident != null && incident.SecondaryDamageV5 > 0 && incident.SecondaryEventsV5.All(e=>e.DamagePoints>=0&&e.DamagePoints<=(e.Kind==PanicSecondaryEventKindV5.GasFlare?10.0001f:e.Kind==PanicSecondaryEventKindV5.PipeLeak?7.0001f:2.8001f)) && incident.SecondaryEventsV5.Count == 1, "At most one kind-specific gas6..10/pipe4..7/electric1.2..2.8 percentage-point event, using an explicit RNG-seed fixture.");
            if (incident != null && calculate != null)
            {
                int rolls = incident.SecondaryRollCountV5;
                float total = incident.TotalDamage;
                var actual = incident.SecondaryEventsV5[0];
                calculate.Invoke(incident, null);
                check("v5.secondary.repeat-no-reroll-or-double-damage", incident.SecondaryRollCountV5 == rolls && Mathf.Abs(incident.TotalDamage - total) < .0001f && incident.SecondaryEventsV5.Count == 1 && incident.SecondaryEventsV5[0].TargetId == actual.TargetId, "Repeated evaluation restores primary contributions and reapplies the same memoized event once.");
                check("v5.secondary.contributions-match-total", Mathf.Abs(incident.Items.Sum(item => item.Contribution) - incident.TotalDamage) < .001f && incident.SecondaryEventSummaryV5.Length > 0, "Event changes actual item contributions and the normal result text.");
            }
            check("v5.rules.loss-boundary-retained", !PanicModel.DamageCausesLoss(74.99f) && PanicModel.DamageCausesLoss(75f) && PanicModel.NormalPreparationSeconds == 130f, "75% loss and 90+40s source of truth retained.");
        }
    }
}
