using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in model checks. Explicit fixtures, no world/player/OS acceptance claim.</summary>
    public static class PanicRevisionV412ModelChecks
    {
        public static PanicModel CreateDamageBoundaryModel(float damage)
        {
            string wallId = damage == 74.99f ? "boundary-wall-11981" : damage == 75f ?
                "boundary-wall-45988" : damage == 75.01f ? "boundary-wall-51684" : null;
            if (wallId == null) throw new ArgumentOutOfRangeException(nameof(damage));
            var model = new PanicModel(); model.StartSimulation(DisasterType.Earthquake);
            model.Items.Clear();
            // Default run seed, Wave1, 13 exposed items: actual production risk/normalization
            // sums exactly to the requested IEEE single value. TotalDamage is never assigned.
            for (int i = 0; i < 12; i++) model.Items.Add(new PanicItem {
                Id = "boundary-25-table-" + i, ArtId = "table", Kind = ItemKind.Furniture,
                Movable = true, Installed = true, Label = "Boundary table " + i });
            model.Items.Add(new PanicItem { Id = wallId, ArtId = "wall", Kind = ItemKind.Wall,
                Installed = true, Label = "Boundary wall" });
            return model;
        }

        public static IReadOnlyList<string> Run()
        {
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride))
                throw new InvalidOperationException("V412 checks require an isolated ScorePathOverride.");
            var passed = new List<string>();
            Action<bool,string> check = (ok, name) => {
                if (!ok) throw new InvalidOperationException("V412 check failed: " + name);
                passed.Add(name);
            };
            CheckCampaignTiming(check);
            foreach (float edge in new[] { 74.99f, 75f, 75.01f })
            {
                var boundary = CreateDamageBoundaryModel(edge);
                check(boundary.EnterBunker(), "damage " + edge + " enters real disaster path");
                boundary.Tick(8f);
                bool fatal = edge >= 75f;
                check(boundary.TotalDamage == edge && boundary.Phase == PanicPhase.Aftermath &&
                    (boundary.FailureReason.Length > 0) == fatal,
                    "damage " + edge + " actual risk sum and failure reason");
                boundary.Tick(4f);
                check(boundary.Phase == (fatal ? PanicPhase.GameOver : PanicPhase.Inspection),
                    "damage " + edge + " actual aftermath chooses survival/loss");
            }
            foreach (int wave in new[] { 1, 4, 7, 10, 100 })
            {
                var m = NewRun(); m.Phase = PanicPhase.Inspection; m.Wave = wave - 1;
                m.FinishInspection(); m.PackedWater = m.PackedFood = 1;
                check(m.Phase == PanicPhase.Preparing && m.TimeRemaining == 165f && m.PhaseTime == 165f,
                    "wave " + wave + " starts with 120 precaution seconds plus45 bag seconds");
                check(m.Wave == wave && m.RequiredDays == 1 && m.BagReady && m.PackedSupportMask == 0,
                    "wave " + wave + " only needs one water and food, no support");
                m.PackedWater = 0; check(!m.BagReady, "wave " + wave + " missing water rejects");
                m.PackedWater = 1; m.PackedFood = 0; check(!m.BagReady, "wave " + wave + " missing food rejects");
            }
            var model = NewRun(); var e = new PanicExpedition(model);
            check(e.TryCollectWorldSupply("supply_water_kitchen") && model.PackedWater == 1 &&
                e.Count("water") == 1 && e.FindWorldSupply("supply_water_kitchen").Taken,
                "finite water pickup packs one actual unit and keeps remainder loose");
            check(e.TryCollectWorldSupply("supply_food_kitchen") && model.PackedFood == 1 &&
                e.Count("food") == 1 && model.BagReady && model.PackedSupportMask == 0 && !model.CheatedRun,
                "finite food pickup completes minimal bag without optional equipment");
            check(!e.TryCollectWorldSupply("supply_water_kitchen") && e.Count("water") == 1,
                "essential auto pack cannot duplicate depleted world stock");
            check(e.TryCollectWorldSupply("supply_firstaid_bath") && e.Count("firstaid") == 1 &&
                model.PackedSupportMask == 0, "optional equipment remains loose until actually packed");
            model = NewRun(); e = new PanicExpedition(model);
            for (int i = 0; i < e.Inventory.Count; i++) e.Inventory[i] = new SupplySlot("firstaid", 1);
            string before = Stock(e);
            check(!e.TryCollectWorldSupply("supply_water_kitchen") && before == Stock(e) &&
                e.FindWorldSupply("supply_water_kitchen").Count == 2 && model.PackedWater == 0,
                "insufficient remainder capacity rejects entire collection atomically");
            model = NewRun(); e = new PanicExpedition(model);
            string[] fullBag = { "food", "firstaid", "flashlight", "radio", "blanket", "battery", "battery", "battery" };
            for (int i = 0; i < fullBag.Length; i++) e.Bag[i] = new SupplySlot(fullBag[i], 1);
            model.PackedFood = 1; model.PackedSupportMask = 15;
            check(e.TryCollectWorldSupply("supply_water_kitchen") && model.PackedWater == 0 && e.Count("water") == 2,
                "full real bag keeps collected essentials loose when inventory can hold them");

            string[] actions = { "anchorwall", "anchorfloor", "upgrade", "upgrade", "fire", "plant", "drain", "hose", "fence" };
            string[] targets = { "bookshelf", "bookshelf", "wall_north", "wall_north", "fire_station", "plant_east", "drain_east", "hose_station", "fence_east" };
            for (int recipe = 0; recipe < actions.Length; recipe++)
            {
                model = NewRun(); e = new PanicExpedition(model);
                foreach (var source in e.Sources) e.ToggleSource(source.Id);
                string action = actions[recipe], target = targets[recipe];
                var item = model.Items.Find(i => i.Id == target);
                if (recipe == 3) item.Tier = 1;
                if (action == "fence")
                {
                    before = Stock(e); int fenceBudget = model.Budget;
                    bool denied = !e.CanBeginWork(action, target);
                    e.Work.Begin(action, target); Install(e.Work);
                    check(denied && !e.TryCompleteWork(action, target) && model.FenceCount == 0 &&
                        model.Budget == fenceBudget && before == Stock(e), "removed fence API cannot spend or install");
                    continue;
                }
                if (!PanicItemActionRules.Allows(action, item, target)) continue;
                var costs = PanicExpedition.MaterialCosts(action, item);
                var owned = new Dictionary<string,int>(); foreach (var cost in costs) owned[cost.Id] = e.Count(cost.Id);
                int budget = model.Budget, budgetCost = PanicExpedition.WorkCost(action, item); before = Stock(e);
                check(e.CanBeginWork(action, target) && before == Stock(e) && budget == model.Budget,
                    action + " recipe " + recipe + " reads actual finite numeric pool without inventory slots");
                e.Work.Begin(action, target); e.Work.TryClick(0); e.Work.Tick(.1f, 0, false, false); e.Work.Cancel();
                check(before == Stock(e) && budget == model.Budget, action + " recipe " + recipe + " cancel is free");
                e.Work.Begin(action, target); Install(e.Work);
                check(e.TryCompleteWork(action, target) && model.Budget == budget - budgetCost,
                    action + " recipe " + recipe + " completion spends budget once");
                bool exact = true; foreach (var cost in costs) exact &= e.Count(cost.Id) == owned[cost.Id] - cost.Required;
                before = Stock(e); budget = model.Budget;
                check(exact && !e.TryCompleteWork(action, target) && before == Stock(e) && budget == model.Budget,
                    action + " recipe " + recipe + " spends exact material recipe once");
            }
            model = new PanicModel(); e = new PanicExpedition(model); var flow = new PanicJuryDemo(model, e);
            check(flow.Start(), "short flow starts");
            flow.Tick(.1f, PanicJuryDemo.SpawnPosition + new Vector3(1,0,0), true, false);
            check(flow.Stage == PanicJuryStage.Preparation && !flow.BagFinalStageActive &&
                (flow.TargetId == "bookshelf" || flow.TargetId == "fridge"), "short guide selects anchor before water");
            var shortWall = model.Items.Find(i => i.Id == "wall_north"); shortWall.Tier = 1;
            check(!e.CanBeginWork("upgrade", "wall_north") && e.HasMissingRequirements,
                "short shortage reports the actual optional recipe");
            check(!e.CanBeginWork("invalid", "wall_north") && !e.HasMissingRequirements &&
                e.RequirementAction.Length == 0, "invalid phase action clears unrelated stale shortage");
            shortWall.Tier = 0;
            check(e.ToggleSource("up_cabinet") && e.Count("screws") >= 3 && e.Count("plank") >= 2 &&
                e.CanBeginWork("upgrade", "wall_north"), "short optional upgrade uses retained finite container stock");
            model.Items.Find(i => i.Id == "fridge").Anchored = true; // Explicit setup fixture for temporal gate.
            e.TryCollectWorldSupply("supply_water_kitchen");
            flow.Tick(.1f, PanicJuryDemo.QuakeCoverPosition, true, false);
            check(model.BagReady && flow.ShelterKind == PanicJuryShelter.None &&
                !flow.TryReachShelter(PanicJuryShelter.QuakeCover, PanicJuryDemo.QuakeCoverPosition, true),
                "ready bag and anchor cannot begin earthquake before final45");
            flow.Tick(34.8f, PanicJuryDemo.QuakeCoverPosition, true, false);
            check(flow.BagFinalStageActive && Math.Abs(model.TimeRemaining - 45f) < .001f &&
                flow.ShelterKind == PanicJuryShelter.QuakeCover && !e.CanBeginWork("upgrade", "wall_north"),
                "final45 clock permits shelter and closes construction");
            check(flow.TryReachShelter(PanicJuryShelter.QuakeCover, PanicJuryDemo.QuakeCoverPosition, true) &&
                flow.Outcomes.Count == 1 && flow.Outcomes[0].WaterConsumed == 1 && flow.Outcomes[0].FoodConsumed == 1,
                "short earthquake consumes exactly one actual water and food");
            check(PanicJuryDemo.ContentLimitSeconds == 270f && PanicJuryDemo.HardLimitSeconds == 300f,
                "short flow 270 plus30 contract remains");
            int minimumRows = int.MaxValue, maximumRows = 0;
            bool finiteRows = true, allRegionsBuildable = true;
            for (int sample = 0; sample < 128; sample++)
            {
                model = NewRun(); e = new PanicExpedition(model);
                foreach (var source in e.Sources) e.ToggleSource(source.Id);
                int rows = e.Count("seedling");
                minimumRows = Math.Min(minimumRows, rows); maximumRows = Math.Max(maximumRows, rows);
                foreach (var source in e.Sources) { e.ToggleSource(source.Id); e.ToggleSource(source.Id); }
                e.OnNewWave();
                finiteRows &= e.Count("seedling") == rows;
                foreach (string region in new[] { "plant_west1", "plant_west2", "plant_east" })
                {
                    if (!e.CanBeginWork("plant", region)) { allRegionsBuildable = false; break; }
                    e.Work.Begin("plant", region); Install(e.Work);
                    allRegionsBuildable &= e.TryCompleteWork("plant", region);
                }
                allRegionsBuildable &= model.TreeCount == 3 && model.Budget == 910 && e.Count("seedling") == rows - 3;
            }
            check(minimumRows >= 3 && maximumRows <= 4, "128 finite resets always yield three to four actual tree-row packages");
            check(finiteRows, "128 finite resets cannot reroll or refill tree-row packages on reopen or new wave");
            check(allRegionsBuildable, "128 finite resets can commit all three regional row recipes with actual costs");
            return passed.AsReadOnly();
        }
        public static void CheckShortFlowTimingR7(Action<bool,string> check)
        {
            var model=new PanicModel();var expedition=new PanicExpedition(model);var flow=new PanicJuryDemo(model,expedition);
            check(flow.Start(),"short45 starts a fresh isolated real short scenario");
            flow.Tick(.1f,PanicJuryDemo.SpawnPosition+new Vector3(1,0,0),true,false);
            flow.Tick(.1f,PanicJuryDemo.QuakeCoverPosition,true,false);
            check(flow.Stage==PanicJuryStage.Preparation&&!flow.BagFinalStageActive&&Math.Abs(model.TimeRemaining-79.8f)<.001f&&model.CanBeginJuryWork("anchorwall","bookshelf"),
                "short45 first .1+.1 leaves79.8 and keeps actual precaution gate open");
            flow.Tick(34.8f,PanicJuryDemo.QuakeCoverPosition,true,false);
            check(flow.Stage==PanicJuryStage.Preparation&&flow.BagFinalStageActive&&Math.Abs(model.TimeRemaining-45f)<.001f&&!model.CanBeginJuryWork("anchorwall","bookshelf"),
                "short45 .1+.1+34.8 is35 elapsed of80 and reaches exact45 bag gate");
            check(PanicJuryDemo.ContentLimitSeconds==270f&&PanicJuryDemo.ReserveSeconds==30f&&PanicJuryDemo.HardLimitSeconds==300f,
                "short45 keeps270 content plus30 reserve, hard limit300");
        }
        public static void CheckCampaignTiming(Action<bool,string> check)
        {
            var m = NewRun();
            check(m.Phase == PanicPhase.Preparing && m.TimeRemaining == 165f && m.PhaseTime == 165f,
                "campaign starts with 165 total preparation seconds");
            check(m.Budget == 1000 && m.PackedWater == 0 && m.PackedFood == 0 && !m.CheatedRun,
                "shorter campaign clock does not grant budget, supplies or cheat status");
            m.SetPaused(true); m.Tick(120f);
            check(m.TimeRemaining == 165f && m.PhaseTime == 165f && !m.CheatedRun,
                "campaign pause preserves the full preparation clock without marking a cheat");
            m.SetPaused(false); m.Tick(119f);
            check(m.Phase == PanicPhase.Preparing && m.TimeRemaining == 46f && m.PhaseTime == 46f,
                "119 elapsed campaign seconds leave one precaution second before final45");
            m.Tick(1f);
            check(m.Phase == PanicPhase.Preparing && m.TimeRemaining == 45f && m.PhaseTime == 45f,
                "120 elapsed campaign seconds reach the final45 bag boundary");
            m.Tick(44f);
            check(m.Phase == PanicPhase.Preparing && m.TimeRemaining == 1f,
                "campaign retains the whole45 seconds for bag collection and shelter");
            m.Tick(1f);
            check(m.Phase == PanicPhase.GameOver && m.TimeRemaining == 0f && !string.IsNullOrEmpty(m.FailureReason),
                "165 elapsed campaign seconds still cause the real unsheltered timeout loss");
            check(m.Budget == 1000 && m.PackedWater == 0 && m.PackedFood == 0 && m.CompletedWaves == 0 &&
                m.Score == 0 && m.LastReward == 0 && !m.CheatedRun,
                "campaign timeout preserves real budget, empty bag and zero rewards");
            m.StartRun(false);
            check(m.Phase == PanicPhase.Preparing && m.Wave == 1 && m.TimeRemaining == 165f && m.PhaseTime == 165f,
                "campaign retry resets the same165 second preparation clock");
            m.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, true); m.Tick(120f);
            check(m.TimeRemaining == 165f && m.PhaseTime == 165f && m.CheatedRun,
                "developer freeze still holds the new campaign clock and marks the run");
            m.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, false); m.Tick(120f);
            check(m.TimeRemaining == 45f && m.PhaseTime == 45f && m.CheatedRun && !m.DeveloperAnyEnabled,
                "unfreezing reaches final45 after120 active seconds and keeps cheat status sticky");
        }
        static PanicModel NewRun() { var m = new PanicModel(); m.StartRun(false); return m; }
        static void Install(PanicWorkSession work)
        { while (!work.Complete) { if (!work.TryClick(work.NextClickTarget)) throw new InvalidOperationException("Work click rejected"); for (int i=0;i<8;i++) work.Tick(.1f,0,false,false); } }
        static string Stock(PanicExpedition e)
        {
            var text = new StringBuilder(); foreach (var slot in e.Inventory) text.Append(slot.Id).Append(':').Append(slot.Count).Append('|');
            text.Append('/'); foreach (var slot in e.Bag) text.Append(slot.Id).Append(':').Append(slot.Count).Append('|');
            foreach (var id in PanicExpedition.MaterialIds) text.Append(id).Append(':').Append(e.Count(id)).Append('|'); return text.ToString();
        }
    }
}
