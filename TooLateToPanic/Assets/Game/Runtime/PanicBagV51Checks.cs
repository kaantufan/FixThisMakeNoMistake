using System;
using System.Collections.Generic;
using System.Text;

namespace TooLateToPanic
{
    /// <summary>Opt-in pure model/API fixtures for root's isolated native driver.
    /// These checks do not launch Unity, move a controller or claim OS/human acceptance.</summary>
    public static class PanicBagV51Checks
    {
        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) return;
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride))
            { check("v51.bag.requires-isolated-profile", false, "NOT_RUN: set a fresh isolated ScorePathOverride first."); return; }
            check("v51.bag.isolated-model-fixtures", true, "Explicit source-count/slot fixtures; not played collection, OS input or human acceptance.");
            var ids = new[] { "water", "food", "firstaid", "flashlight", "battery", "radio", "blanket" };
            foreach (string id in ids)
            {
                var e = NewExpedition();
                var s = Supply(e, id); s.Count = 1;
                if (id == "flashlight" || id == "radio") e.Inventory[0] = new SupplySlot("battery", 1);
                bool collected = e.TryCollectWorldSupply(s.Id);
                check("v51.bag.autopack." + id,
                    PanicExpedition.IsEmergencyBagItemV51(id) && collected && Count(e.Bag, id) == 1 && e.Count(id) == 0 && s.Count == 0 &&
                    e.LastWorldSupplyPackedCountV51 == 1 && e.LastWorldSupplyInventoryCountV51 == 0,
                    "One real world unit enters a real bag slot automatically; powered devices use one existing battery.");
            }
            {
                var e = NewExpedition(); var s = Supply(e, "water"); s.Count = 3;
                bool collected = e.TryCollectWorldSupply(s.Id);
                check("v51.bag.water-not-only-missing-first-unit", collected && e.Model.PackedWater == 3 && e.Count("water") == 0 && s.Count == 0,
                    "Three collected water units fill the existing three-day cap, beyond the one required per wave.");
                s = Supply(e, "food"); s.Count = 3; collected = e.TryCollectWorldSupply(s.Id);
                check("v51.bag.food-not-only-missing-first-unit", collected && e.Model.PackedFood == 3 && e.Count("food") == 0 && e.Model.BagReady,
                    "Three food units auto pack; production readiness still needs only one water plus one food.");
                s = Supply(e, "firstaid"); s.Count = 3; collected = e.TryCollectWorldSupply(s.Id);
                check("v51.bag.unique-support-overflow-to-inventory", collected && Count(e.Bag, "firstaid") == 1 && e.Count("firstaid") == 2 && s.Count == 0,
                    "Existing support uniqueness is preserved; excess real units use inventory.");
            }
            {
                var e = NewExpedition(); var battery = Supply(e, "battery"); battery.Count = 2;
                bool tookBattery = e.TryCollectWorldSupply(battery.Id);
                var light = Supply(e, "flashlight"); light.Count = 1;
                bool tookLight = e.TryCollectWorldSupply(light.Id);
                check("v51.bag.packed-battery-powers-device", tookBattery && tookLight && Count(e.Bag, "battery") == 1 && Count(e.Bag, "flashlight") == 1 && e.Count("battery") == 0 && e.LastWorldSupplyBatteryCostV51 == 1,
                    "A packed real battery is consumed exactly once when the flashlight enters the bag.");
                var radio = Supply(e, "radio"); radio.Count = 1;
                bool tookRadio = e.TryCollectWorldSupply(radio.Id);
                check("v51.bag.second-device-consumes-last-battery", tookRadio && Count(e.Bag, "battery") == 0 && Count(e.Bag, "radio") == 1 && e.LastWorldSupplyBatteryCostV51 == 1,
                    "Radio consumes the second finite battery; no free or duplicate charge.");
                string before = Snapshot(e);
                check("v51.bag.exhausted-pickup-does-not-duplicate-or-charge", !e.TryCollectWorldSupply(radio.Id) && before == Snapshot(e) && e.LastWorldSupplyBatteryCostV51 == 0,
                    "Repeated collection of consumed stock mutates neither bag, inventory nor batteries.");
                int lightIndex = Find(e.Bag, "flashlight"), emptyIndex = FindEmpty(e.Bag);
                check("v51.bag.device-reorder-has-no-second-battery-cost", emptyIndex >= 0 && e.TransferSlots(true, lightIndex, true, emptyIndex) && Count(e.Bag, "flashlight") == 1 && Count(e.Bag, "battery") == 0,
                    "Moving an already packed device within the bag has zero additional battery cost.");
            }
            {
                var e = NewExpedition(); var s = Supply(e, "radio"); s.Count = 1;
                check("v51.bag.uncharged-device-falls-back-to-inventory", e.TryCollectWorldSupply(s.Id) && Count(e.Bag, "radio") == 0 && e.Count("radio") == 1 && e.LastWorldSupplyBatteryCostV51 == 0,
                    "Without a real battery the device is safely retained in inventory.");
                var battery = Supply(e, "battery"); battery.Count = 1; e.TryCollectWorldSupply(battery.Id);
                check("v51.bag.manual-pack-uses-same-packed-battery-rule", e.PackSlot(Find(e.Inventory, "radio")) && Count(e.Bag, "radio") == 1 && Count(e.Bag, "battery") == 0,
                    "Inventory fallback can subsequently pack using the same real carried battery rule.");
            }
            {
                var e = NewExpedition(); FillFullLegalBag(e, true);
                var radio = Supply(e, "radio"); radio.Count = 1;
                check("v51.bag.last-battery-frees-eighth-slot", e.Bag.Count == 8 && Used(e.Bag) == 8 && e.TryCollectWorldSupply(radio.Id) && Used(e.Bag) == 8 && Count(e.Bag, "radio") == 1 && Count(e.Bag, "battery") == 0 && e.LastWorldSupplyBatteryCostV51 == 1,
                    "A full valid eight-slot bag replaces its final charged battery with the new device atomically.");
            }
            {
                var e = NewExpedition(); FillFullLegalBag(e, true); e.Bag[7].Count = 2;
                var radio = Supply(e, "radio"); radio.Count = 1;
                check("v51.bag.full-bag-does-not-charge-inventory-fallback", e.TryCollectWorldSupply(radio.Id) && Count(e.Bag, "radio") == 0 && e.Count("radio") == 1 &&
                    Count(e.Bag, "battery") == 2 && e.LastWorldSupplyBatteryCostV51 == 0,
                    "Consuming one of two batteries would not free a slot, so the candidate charge rolls back before inventory fallback.");
                radio = e.FindWorldSupply(radio.Id); radio.Count = 1; Fill(e.Inventory, "firstaid", 1); string before = Snapshot(e);
                check("v51.bag.failed-device-collection-does-not-consume-battery", !e.TryCollectWorldSupply(radio.Id) && before == Snapshot(e) && Count(e.Bag, "battery") == 2,
                    "No bag or inventory capacity means no battery cost and no loss of the finite device.");
            }
            {
                var e = NewExpedition(); e.Bag[0] = new SupplySlot("battery", 2); e.Inventory[0] = new SupplySlot("battery", 1);
                var light = Supply(e, "flashlight"); light.Count = 1;
                check("v51.bag.battery-cost-is-one-across-both-containers", e.TryCollectWorldSupply(light.Id) && e.Count("battery") == 0 && Count(e.Bag, "battery") == 2 &&
                    Count(e.Bag, "flashlight") == 1 && e.LastWorldSupplyBatteryCostV51 == 1,
                    "Loose battery is used first; the same new device never charges both loose and packed stock.");
            }
            {
                var e = NewExpedition(); FillFullLegalBag(e, false); Fill(e.Inventory, "firstaid", 1);
                var s = Supply(e, "water"); s.Count = 2; string before = Snapshot(e);
                check("v51.bag.full-bag-full-inventory-no-loss", !e.TryCollectWorldSupply(s.Id) && before == Snapshot(e) && s.Count == 2 && e.LastWorldSupplyCollectedCountV5 == 0,
                    "Full real bag plus full inventory safely rejects with finite world stock intact.");
                e.Inventory[0] = new SupplySlot("water", 4); s.Count = 4;
                bool partial = e.TryCollectWorldSupply(s.Id);
                check("v51.bag.partial-overflow-leaves-finite-remainder", partial && Count(e.Bag, "water") == 3 && e.Count("water") == 5 && s.Count == 3 && e.LastWorldSupplyCollectedCountV5 == 1,
                    "One inventory capacity moves one world unit; the other three stay on their real supply seat.");
                before = Snapshot(e);
                check("v51.bag.partial-repeat-cannot-duplicate", !e.TryCollectWorldSupply(s.Id) && before == Snapshot(e) && s.Count == 3,
                    "Repeated full-capacity collection retains the exact remainder.");
            }
            {
                var e = NewExpedition(); var s = Supply(e, "water"); s.Count = 5;
                e.Bag[0] = new SupplySlot("water", 2); Fill(e.Inventory, "firstaid", 1); e.Inventory[0] = new SupplySlot("water", 4);
                check("v51.bag.combined-partial-capacity-atomic", e.TryCollectWorldSupply(s.Id) && Count(e.Bag, "water") == 3 && e.Count("water") == 5 && s.Count == 3 &&
                    e.LastWorldSupplyPackedCountV51 == 1 && e.LastWorldSupplyInventoryCountV51 == 1 && e.LastWorldSupplyCollectedCountV5 == 2,
                    "Candidate publication combines one real bag capacity and one inventory capacity without loss.");
            }
            {
                var e = NewExpedition(); var s = Supply(e, "water"); s.Count = 1; e.TryCollectWorldSupply(s.Id);
                int revision = e.LootLayoutRevisionV5; string before = Snapshot(e); e.OnNewWave();
                check("v51.bag.same-wave-callback-no-refill", e.LootLayoutRevisionV5 == revision && s.Count == 0 && before == Snapshot(e),
                    "Carried automatic storage does not reroll or refill finite world sources within a wave.");
                var source = e.Sources[0]; e.ToggleSource(source.Id);
                check("v51.bag.materials-stay-real-separate-stock", Used(e.Bag) == 1 && e.Sources[0].AutoLooted,
                    "Construction rewards retain the material pool; they do not consume emergency bag slots.");
                e.Model.SetPaused(true); s = Supply(e, "food"); int stock = s.Count; before = Snapshot(e);
                check("v51.bag.pause-blocks-collection", !e.TryCollectWorldSupply(s.Id) && stock == s.Count && before == Snapshot(e),
                    "Ordinary pause retains finite supplies and bag state.");
            }
            check("v51.bag.blackout-envelope", PanicHomeWorld.EvaluateBagBlackoutOpacityV51(0) == 0 &&
                PanicHomeWorld.EvaluateBagBlackoutOpacityV51(.24f) == 1 && PanicHomeWorld.EvaluateBagBlackoutOpacityV51(.55f) == 1 &&
                PanicHomeWorld.EvaluateBagBlackoutOpacityV51(.82f) == 1 && PanicHomeWorld.EvaluateBagBlackoutOpacityV51(2f) == 0,
                "Pure envelope evaluation only. Root must verify the actual rendered opaque interval and held real controller.");
        }
        static PanicExpedition NewExpedition() { var m = new PanicModel(); m.StartRun(false); return new PanicExpedition(m); }
        static PanicWorldSupply Supply(PanicExpedition e, string item) { foreach (var s in e.WorldSupplies) if (s.ItemId == item) return s; throw new InvalidOperationException("Missing authored item: " + item); }
        static int Count(List<SupplySlot> slots, string id) { int total = 0; foreach (var s in slots) if (s != null && s.Id == id) total += s.Count; return total; }
        static int Find(List<SupplySlot> slots, string id) { return slots.FindIndex(s => s != null && s.Id == id && s.Count > 0); }
        static int FindEmpty(List<SupplySlot> slots) { return slots.FindIndex(s => s == null || s.Count == 0 || string.IsNullOrEmpty(s.Id)); }
        static int Used(List<SupplySlot> slots) { int total = 0; foreach (var s in slots) if (s != null && s.Count > 0) total++; return total; }
        static void Fill(List<SupplySlot> slots, string id, int count) { for (int i = 0; i < slots.Count; i++) slots[i] = new SupplySlot(id, count); }
        static void FillFullLegalBag(PanicExpedition e, bool finalBattery)
        {
            for (int i = 0; i < 3; i++) e.Bag[i] = new SupplySlot("water", 1);
            e.Bag[3] = new SupplySlot("food", 3); e.Bag[4] = new SupplySlot("firstaid", 1);
            e.Bag[5] = new SupplySlot("flashlight", 1); e.Bag[6] = new SupplySlot("blanket", 1);
            e.Bag[7] = new SupplySlot(finalBattery ? "battery" : "radio", 1);
        }
        static string Snapshot(PanicExpedition e)
        {
            var b = new StringBuilder(); foreach (var slots in new[] { e.Inventory, e.Bag }) { foreach (var s in slots) b.Append(s.Id).Append(':').Append(s.Count).Append('|'); b.Append('/'); }
            foreach (var s in e.WorldSupplies) b.Append(s.Id).Append(':').Append(s.Count).Append('|');
            return b.ToString();
        }
    }
}
