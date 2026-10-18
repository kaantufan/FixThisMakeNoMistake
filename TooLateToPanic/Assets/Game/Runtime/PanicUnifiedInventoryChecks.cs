using System;
using System.Collections.Generic;
using System.Text;

namespace TooLateToPanic
{
    // Source/model fixtures only. Never claims native/controller/OS/human acceptance.
    // Compile alongside the real staged model and expedition, then invoke Run.
    public static class PanicUnifiedInventoryChecks
    {
        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) return;
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride))
            { check("unified.isolated-profile", false, "NOT_RUN: caller must set a fresh isolated ScorePathOverride."); return; }
            Case(check, "canonical", () => {
                var e = New();
                Check(check, "canonical.20-slots-one-instance", e.Inventory.Count == 20 && ReferenceEquals(e.Inventory, e.Bag));
                Take(e, "water", 7); Take(e, "food", 6);
                Check(check, "canonical.all-supplies-are-ready", e.Count("water") == 7 && e.Count("food") == 6 && e.Model.PackedWater == 7 && e.Model.PackedFood == 6 && e.Model.BagReady);
                Check(check, "canonical.real-stack-limits", MaxStack(e, "water") <= 5 && MaxStack(e, "food") <= 5);
                string before = Slots(e); int water = e.Model.PackedWater;
                Check(check, "canonical.cross-flags-same-slot-noop", !e.TransferSlots(false, 0, true, 0) && !e.TransferSlots(true, 0, false, 0) && before == Slots(e) && water == e.Model.PackedWater);
                int source = Find(e, "water"), empty = Empty(e);
                Check(check, "canonical.flag-agnostic-reorder", e.TransferSlots(true, source, false, empty) && e.Count("water") == 7 && e.Model.PackedWater == 7 && ReferenceEquals(e.Inventory, e.Bag));
            });
            Case(check, "capacity", () => {
                var e = New(); Fill(e, "food", 5); e.Inventory[0] = new SupplySlot("water", 4); e.SynchronizeInventoryModel();
                var s = Supply(e, "water"); s.Count = 4;
                Check(check, "capacity.partial-one-keeps-three", e.TryCollectWorldSupply(s.Id) && e.Count("water") == 5 && s.Count == 3 && e.LastWorldSupplyCollectedCountV5 == 1 && e.Model.PackedWater == 5);
                string before = Stock(e);
                Check(check, "capacity.full-reject-is-atomic", !e.TryCollectWorldSupply(s.Id) && before == Stock(e) && s.Count == 3 && e.LastWorldSupplyCollectedCountV5 == 0 && e.LastWorldSupplyBatteryCostV51 == 0);
                e.Model.SetPaused(true); before = Stock(e);
                Check(check, "capacity.pause-reject-is-atomic", !e.TryCollectWorldSupply(Supply(e, "food").Id) && before == Stock(e));
            });
            Case(check, "drag", () => {
                var e = New(); Take(e, "water", 7); Take(e, "food", 2);
                int a = e.Inventory.FindIndex(s => s.Id == "water" && s.Count == 5), b = e.Inventory.FindIndex(s => s.Id == "water" && s.Count == 2);
                Check(check, "drag.merge-to-limit-keeps-remainder", e.TransferSlots(false, a, true, b) && e.Inventory[a].Count == 2 && e.Inventory[b].Count == 5 && e.Count("water") == 7 && e.Model.PackedWater == 7);
                int food = Find(e, "food"); string before = Stock(e);
                Check(check, "drag.partial-different-type-reject-atomic", !e.TransferSlots(true, a, false, food, 1) && before == Stock(e));
                Check(check, "drag.full-stack-swap-conserves-unified-stock", e.TransferSlots(true, a, false, food) && e.Count("water") == 7 && e.Count("food") == 2 && e.Model.PackedWater == 7 && e.Model.PackedFood == 2);
            });
            Case(check, "devices-battery-first", () => {
                var e = New(); Take(e, "battery", 3); Take(e, "flashlight", 1);
                Check(check, "devices.first-charge-exactly-one", e.Count("battery") == 2 && (e.Model.PackedSupportMask & 2) != 0 && e.LastWorldSupplyBatteryCostV51 == 1);
                Take(e, "radio", 1);
                Check(check, "devices.second-category-costs-one", e.Count("battery") == 1 && (e.Model.PackedSupportMask & 6) == 6 && e.Model.SupportCount == 2);
                string before = Stock(e);
                Check(check, "devices.exhausted-world-no-repeat-charge", !e.TryCollectWorldSupply(Supply(e, "radio").Id) && before == Stock(e) && e.LastWorldSupplyBatteryCostV51 == 0);
                int i = Find(e, "flashlight"), empty = Empty(e);
                Check(check, "devices.reorder-no-charge", e.TransferSlots(false, i, true, empty) && e.Count("battery") == 1 && (e.Model.PackedSupportMask & 6) == 6);
                e.PackSlot(Find(e, "radio")); e.UnpackSlot(Find(e, "radio")); e.SynchronizeInventoryModel();
                Check(check, "devices.compat-pack-unpack-do-not-charge-or-disable", e.Count("battery") == 1 && (e.Model.PackedSupportMask & 6) == 6);
                Take(e, "radio", 1);
                Check(check, "devices.duplicate-retained-effect-unique", e.Count("radio") == 2 && e.Count("battery") == 1 && e.Model.SupportCount == 2);
            });
            Case(check, "devices-device-first", () => {
                var e = New(); Take(e, "flashlight", 1); Take(e, "radio", 1);
                Check(check, "devices.unpowered-retained-no-support", e.Count("flashlight") == 1 && e.Count("radio") == 1 && e.Model.PackedSupportMask == 0 && e.Model.SupportCount == 0);
                Take(e, "battery", 1);
                Check(check, "devices.one-battery-one-category", e.Count("battery") == 0 && e.Model.SupportCount == 1 && e.LastWorldSupplyBatteryCostV51 == 1);
                Take(e, "battery", 2);
                Check(check, "devices.second-category-later", e.Count("battery") == 1 && (e.Model.PackedSupportMask & 6) == 6 && e.Model.SupportCount == 2 && e.LastWorldSupplyBatteryCostV51 == 1);
                Take(e, "firstaid", 1); Take(e, "blanket", 1);
                Check(check, "devices.nonpowered-support-no-battery", e.Count("battery") == 1 && (e.Model.PackedSupportMask & 15) == 15 && e.Model.SupportCount == 4);
            });
            Case(check, "last-slot-battery-first", () => {
                var e = New(); Fill(e, "water", 5); e.Inventory[19] = new SupplySlot("battery", 1); e.SynchronizeInventoryModel();
                var s = Supply(e, "flashlight"); s.Count = 1;
                Check(check, "last-slot.existing-battery-frees-slot", e.TryCollectWorldSupply(s.Id) && e.Count("battery") == 0 && e.Count("flashlight") == 1 && Used(e) == 20 && s.Count == 0 && e.LastWorldSupplyBatteryCostV51 == 1);
            });
            Case(check, "last-slot-device-first", () => {
                var e = New(); Fill(e, "water", 5); e.Inventory[19] = new SupplySlot("flashlight", 1); e.SynchronizeInventoryModel();
                var s = Supply(e, "battery"); s.Count = 3;
                Check(check, "last-slot.incoming-battery-direct-install", e.TryCollectWorldSupply(s.Id) && e.Count("battery") == 0 && e.Count("flashlight") == 1 && Used(e) == 20 && s.Count == 2 && e.LastWorldSupplyCollectedCountV5 == 1 && e.LastWorldSupplyBatteryCostV51 == 1 && (e.Model.PackedSupportMask & 2) != 0);
                string before = Stock(e);
                Check(check, "last-slot.charged-device-no-extra-acceptance", !e.TryCollectWorldSupply(s.Id) && before == Stock(e) && s.Count == 2);
            });
            Case(check, "resolution", () => {
                var e = New(); Take(e, "water", 7); Take(e, "food", 6); Take(e, "battery", 1); Take(e, "flashlight", 1);
                var m = e.Model; m.TryEnterShelterV5(m.RequiredShelterV5); m.Tick(8f); e.SyncConsumedBagSupplies();
                Check(check, "resolution.one-per-type", m.Phase == PanicPhase.Aftermath && e.Count("water") == 6 && e.Count("food") == 5 && m.PackedWater == 6 && m.PackedFood == 5);
                string before = Slots(e); int rolls = m.SecondaryRollCountV5;
                e.SyncConsumedBagSupplies(); e.SynchronizeInventoryModel(); e.SyncConsumedBagSupplies();
                Check(check, "resolution.repeated-sync-idempotent", before == Slots(e) && m.PackedWater == 6 && m.PackedFood == 5 && m.SecondaryRollCountV5 == rolls && (m.PackedSupportMask & 2) != 0);
                m.Tick(.01f); e.SyncConsumedBagSupplies();
                Check(check, "resolution.remaining-aftermath-no-second-consumption", e.Count("water") == 6 && e.Count("food") == 5 && (m.PackedSupportMask & 2) != 0);
            });
            Case(check, "deadline", () => {
                var e = New(); Take(e, "water", 2); Take(e, "food", 2);
                Check(check, "shelter.wrong-destination-reject", !e.Model.TryEnterShelterV5(PanicShelterKindV5.PanicRoom) && !e.Model.ReachedShelterV5 && e.Model.Phase == PanicPhase.Preparing);
                e.Model.Tick(PanicModel.NormalPreparationSeconds + 8f); e.SyncConsumedBagSupplies();
                Check(check, "deadline.no-fake-shelter", !e.Model.ReachedShelterV5 && e.Model.Phase == PanicPhase.GameOver && e.Count("water") == 2 && e.Count("food") == 2 && e.Model.FailureReason.Contains("zamanında"));
                Check(check, "deadline.contract-boundaries", PanicModel.NormalPrecautionSeconds == 120f && PanicModel.NormalBagSeconds == 45f && PanicModel.NormalPreparationSeconds == 165f && !PanicModel.DamageCausesLoss(74.99f) && PanicModel.DamageCausesLoss(75f));
                e = New(); Take(e, "water", 2); Take(e, "food", 2); e.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss, true);
                e.Model.Tick(PanicModel.NormalPreparationSeconds + 8f); e.SyncConsumedBagSupplies();
                Check(check, "deadline.no-loss-real-consumption-no-fake-shelter", !e.Model.ReachedShelterV5 && e.Model.CheatedRun && e.Count("water") == 1 && e.Count("food") == 1);
                e.Model.Tick(4f); e.SyncConsumedBagSupplies();
                Check(check, "deadline.no-loss-score-isolated", e.Model.Score == 0 && e.Model.LastWaveScore == 0 && e.Model.LastReward == 0 && e.Count("water") == 1 && e.Count("food") == 1);
            });
            Case(check, "restock", () => {
                var e = New(); Take(e, "water", 2); Take(e, "flashlight", 1); int rev = e.LootLayoutRevisionV5;
                string before = Stock(e); e.OnNewWave(); e.OnNewWave();
                Check(check, "restock.same-wave-no-reroll", before == Stock(e) && e.LootLayoutRevisionV5 == rev);
                e.Model.Wave++; e.OnNewWave(); int nextRev = e.LootLayoutRevisionV5; before = Stock(e); e.OnNewWave();
                Check(check, "restock.next-wave-once", nextRev == rev + 1 && before == Stock(e) && e.StockedWaveV5 == e.Model.Wave && e.LootLayoutRevisionV5 == nextRev && e.Count("water") == 2);
                bool none = true; foreach (var s in e.WorldSupplies) if (s.ItemId == "flashlight" && s.Count > 0) none = false;
                Check(check, "restock.retained-device-not-duplicated", none && e.Count("flashlight") == 1 && (e.Model.PackedSupportMask & 2) == 0);
            });
            Case(check, "materials", () => {
                var e = New(); var s = e.Sources[0]; e.ToggleSource(s.Id); int used = Used(e), seq = e.GatherSequence; string counts = Materials(e);
                e.ToggleSource(s.Id); e.ToggleSource(s.Id);
                Check(check, "materials.finite-unchanged-by-unification", e.Sources[0].AutoLooted && used == 0 && Used(e) == 0 && counts == Materials(e) && seq == e.GatherSequence);
                string before = Slots(e); int budget = e.Model.Budget;
                Check(check, "materials.ready-firstaid-no-craft-shortcut", !e.CraftFirstAid() && before == Slots(e) && budget == e.Model.Budget);
            });
            Case(check, "reset", () => {
                var e = New(); Take(e, "water", 7); Take(e, "food", 2); Take(e, "battery", 1); Take(e, "radio", 1);
                e.Model.ReturnToTitle(); e.Model.StartRun(false); e.Reset();
                Check(check, "reset.no-stale-stock-charge-or-score", Used(e) == 0 && e.Model.PackedWater == 0 && e.Model.PackedFood == 0 && e.Model.PackedSupportMask == 0 && !e.Model.CheatedRun && e.Model.Score == 0 && ReferenceEquals(e.Inventory, e.Bag));
                Take(e, "radio", 1);
                Check(check, "reset.retained-new-radio-needs-battery-again", e.Count("radio") == 1 && (e.Model.PackedSupportMask & 4) == 0);
            });
            Case(check, "separate-flow-seeds", () => {
                var m = new PanicModel(); var e = new PanicExpedition(m); m.StartSimulation(DisasterType.Flood); e.ResetForSimulation();
                Check(check, "simulation.real-unified-seed", ReferenceEquals(e.Inventory, e.Bag) && e.Count("water") == 1 && e.Count("food") == 1 && m.BagReady && m.IsSimulation);
                m = new PanicModel(); m.StartSimulation(DisasterType.Fire); e = new PanicExpedition(m);
                Check(check, "simulation.constructor-preserves-real-seed", ReferenceEquals(e.Inventory, e.Bag) && e.Count("water") == 1 && e.Count("food") == 1 && m.BagReady && m.IsSimulation);
                m = new PanicModel(); m.StartJuryDemo(); e = new PanicExpedition(m); e.ResetForJuryDemo();
                Check(check, "shortflow.real-food-seed", ReferenceEquals(e.Inventory, e.Bag) && e.Count("water") == 0 && e.Count("food") == 1 && m.PackedWater == 0 && m.PackedFood == 1 && !m.CheatedRun);
            });
        }

        static void Case(Action<string, bool, string> check, string id, Action action)
        { try { action(); } catch (Exception ex) { check("unified." + id + ".exception", false, ex.GetType().Name + ": " + ex.Message); } }
        static void Check(Action<string, bool, string> check, string id, bool ok)
        { check("unified." + id, ok, "Deterministic source/model fixture; no Unity/controller/OS input."); }
        static PanicExpedition New() { var m = new PanicModel(); m.StartRun(false); return new PanicExpedition(m); }
        static PanicWorldSupply Supply(PanicExpedition e, string id)
        { foreach (var s in e.WorldSupplies) if (s.ItemId == id) return s; throw new Exception("No authored supply " + id); }
        static void Take(PanicExpedition e, string id, int count)
        { var s = Supply(e, id); s.Count = count; if (!e.TryCollectWorldSupply(s.Id)) throw new Exception("Fixture collect rejected: " + id + ": " + e.Message); }
        static int Find(PanicExpedition e, string id) { return e.Inventory.FindIndex(s => s != null && s.Id == id && s.Count > 0); }
        static int Empty(PanicExpedition e) { return e.Inventory.FindIndex(s => s == null || s.Count <= 0 || string.IsNullOrEmpty(s.Id)); }
        static int Used(PanicExpedition e) { int n = 0; foreach (var s in e.Inventory) if (s != null && s.Count > 0 && !string.IsNullOrEmpty(s.Id)) n++; return n; }
        static int MaxStack(PanicExpedition e, string id) { int n = 0; foreach (var s in e.Inventory) if (s != null && s.Id == id) n = Math.Max(n, s.Count); return n; }
        static void Fill(PanicExpedition e, string id, int count) { for (int i = 0; i < e.Inventory.Count; i++) e.Inventory[i] = new SupplySlot(id, count); }
        static string Slots(PanicExpedition e) { var b = new StringBuilder(); foreach (var s in e.Inventory) b.Append(s == null ? "<null>" : s.Id + ":" + s.Count).Append('|'); return b.ToString(); }
        static string Materials(PanicExpedition e) { var b = new StringBuilder(); foreach (var id in PanicExpedition.MaterialIds) b.Append(id).Append(':').Append(e.Count(id)).Append('|'); return b.ToString(); }
        static string Stock(PanicExpedition e) { var b = new StringBuilder(Slots(e)); foreach (var s in e.WorldSupplies) b.Append(s.Id).Append(':').Append(s.Count).Append('|'); var m = e.Model; b.Append("M:").Append(m.PackedWater).Append(':').Append(m.PackedFood).Append(':').Append(m.PackedSupportMask).Append(':').Append(m.SupportOwnedMask).Append(':').Append(m.Budget).Append(':').Append(m.Score).Append(':').Append(m.CheatedRun); return b.ToString(); }
    }
}
