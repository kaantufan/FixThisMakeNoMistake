using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in model/API checks. Host must set an isolated ScorePathOverride.</summary>
    public static class PanicEconomyChecksV59
    {
        static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        public static PanicExpedition New(int seed)
        {
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride)) throw new InvalidOperationException("Isolated score path required.");
            var m = new PanicModel(); m.StartRun(false);
            m.Items.RemoveAll(i => i.Id == "door_inner_upper" || i.Id == "window_north_upper_east");
            foreach (var d in PanicHouseLayoutV414.Doors)
                if (!m.Items.Exists(i => i.Id == d.Id)) m.Items.Add(new PanicItem { Id=d.Id, Label=d.Label, Kind=ItemKind.Door, ArtId="door", Position=d.Position, Installed=true });
            for (int n=0;n<4;n++) m.Items.Add(new PanicItem { Id="wall_"+new[]{"north","west","south","east"}[n]+"_basement", Label="Bodrum", Kind=ItemKind.Wall, ArtId="wall", Position=new Vector3(0,-3.2f,0), Installed=true });
            m.Items.Add(new PanicItem { Id="bed_child",Label="Çocuk yatağı",Kind=ItemKind.Furniture,ArtId="bed",Movable=true,Installed=true });
            PanicHouseLayoutV414.ApplyModelLayout(m);
            typeof(PanicModel).GetField("compoundSeedV54",Private).SetValue(m,(uint)seed);
            typeof(PanicModel).GetField("secondarySeedV5",Private).SetValue(m,(uint)(seed+11));
            var e = new PanicExpedition(m);
            typeof(PanicExpedition).GetField("lootRandom",Private).SetValue(e,new System.Random(seed));
            typeof(PanicExpedition).GetMethod("ResetWaveLootV5",Private).Invoke(e,null);
            return e;
        }
        public static void Gather(PanicExpedition e, int sourceCount, int seed)
        {
            var r=new System.Random(seed);
            var sources=e.Sources.OrderBy(s=>r.Next()).Take(sourceCount).ToList();
            foreach(var s in sources) e.ToggleSource(s.Id);
        }
        public static bool Complete(PanicExpedition e,string action,string id)
        {
            if(!e.BeginValidatedWorkV414(action,id,false))return false;
            var w=e.Work;
            while(!w.Complete)
            {
                if(w.UsesHammer) w.PressHammer();
                else { w.TryClick(w.CompletedSteps); if(w.UsesLinearDrag) w.ApplyLinearDisplacement(1,true); else w.ApplyRotation(10000*w.ExpectedDirection,true); }
            }
            return e.TryCompleteWork(action,id);
        }
        public static void Run(Action<string,bool,string> check)
        {
            var e=New(5901);var m=e.Model;
            check("start1500",m.Budget==1500,"Production reset.");
            int fullBudget=0;var materialTotal=new Dictionary<string,int>();
            foreach(var item in m.Items.Where(i=>i.Kind!=ItemKind.Furniture))
            {
                for(int tier=0;tier<2;tier++)
                {
                    item.Tier=tier;fullBudget+=PanicExpedition.WorkCost("upgrade",item);
                    foreach(var cost in PanicExpedition.MaterialCosts("upgrade",item)){if(!materialTotal.ContainsKey(cost.Id))materialTotal[cost.Id]=0;materialTotal[cost.Id]+=cost.Required;}
                }
                item.Tier=0;
            }
            check("full-structure-quote",fullBudget==9000&&materialTotal["screws"]==150&&materialTotal["plank"]==90,"Actual 30 structures, both production recipes; fixed/anchor total adds705 cash,12screws,2planks.");
            check("first-wave-cannot-finish",m.Budget<fullBudget,"No wave lock: full completion is unaffordable.");
            int screwsAvailable=e.Sources.Sum(source=>source.Contents.Where(slot=>slot.Id=="screws").Sum(slot=>slot.Count));
            int planksAvailable=e.Sources.Sum(source=>source.Contents.Where(slot=>slot.Id=="plank").Sum(slot=>slot.Count));
            check("bounded-useful-stock",screwsAvailable>=34&&screwsAvailable<=52&&planksAvailable>=22&&planksAvailable<=34,"Retained plus/minus one roll per authored stack; seedling guarantee unchanged.");
            check("timing-loss-inventory",m.TimeRemaining==130&&PanicModel.NormalPrecautionSeconds==90&&PanicModel.NormalBagSeconds==40&&PanicModel.DamageCausesLoss(75)&&!PanicModel.DamageCausesLoss(74.999f)&&e.Inventory.Count==20&&ReferenceEquals(e.Inventory,e.Bag),"Existing contracts.");
            Gather(e,14,1);int budget=m.Budget;string stock=Stock(e);
            check("begin-does-not-charge",e.BeginValidatedWorkV414("anchorfloor","bookshelf",false)&&m.Budget==budget&&Stock(e)==stock,"Read-only preflight.");
            e.Work.Cancel();e.CancelFloorWallUpgradeV414();
            check("cancel-free",m.Budget==budget&&Stock(e)==stock&&!m.Items.Find(i=>i.Id=="bookshelf").AnchorOwned,"Actual cancellation.");
            check("complete-once",Complete(e,"anchorfloor","bookshelf")&&m.Budget==budget-60&&e.Count("screws")==int.Parse(stock.Split('|')[0])-2,"Charges only completed work.");
            budget=m.Budget;stock=Stock(e);
            check("duplicate-no-charge",!e.TryCompleteWork("anchorfloor","bookshelf")&&m.Budget==budget&&Stock(e)==stock,"Repeated commit rejects.");
            m.ToggleAnchor("bookshelf");
            check("owned-reanchor-free",Complete(e,"anchorfloor","bookshelf")&&m.Budget==budget&&Stock(e)==stock,"Retained anchor ownership.");
            int revision=e.LootLayoutRevisionV5;stock=Stock(e);e.OnNewWave();
            foreach(var s in e.Sources){e.ToggleSource(s.Id);e.ToggleSource(s.Id);}
            check("same-wave-no-reroll",e.LootLayoutRevisionV5==revision&&Stock(e)==stock,"One finite snapshot.");
            e=New(5902);m=e.Model;Gather(e,14,1);m.Budget=0;budget=m.Budget;stock=Stock(e);
            var upper = m.Items.First(i => i.Kind == ItemKind.Wall && Mathf.FloorToInt((i.Position.y + .15f) / 3.2f) == 1);
            check("insufficient-budget-atomic",!e.BeginValidatedWorkV414("upgrade",upper.Id,false)&&m.Budget==budget&&Stock(e)==stock&&m.Items.Find(i=>i.Id=="roof").Tier==0,"Upper walls+roof cannot partially consume.");
            m.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget,true);m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials,true);
            check("f2-free-score-isolated",Complete(e,"upgrade",upper.Id)&&m.Items.Find(i=>i.Id=="roof").Tier==1&&m.Budget==0&&Stock(e)==stock&&m.CheatedRun,"Production developer mutation and actual bundled free work.");
            check("full-target-count",m.Items.Count(i=>i.Kind!=ItemKind.Furniture)==30,"Physical residence layout fixture, no meshes.");
            e=New(5903);m=e.Model;Gather(e,14,1);budget=m.Budget;stock=Stock(e);
            check("floor-start-no-charge",e.BeginValidatedWorkV414("upgrade","wall_north",false)&&m.Budget==budget&&Stock(e)==stock,"Batch preflight.");
            m.Items.Find(i=>i.Id=="wall_west").Tier=1;
            while(!e.Work.Complete)e.Work.PressHammer();
            check("stale-floor-no-charge",!e.TryCompleteWork("upgrade","wall_north")&&m.Budget==budget&&Stock(e)==stock,"Changed live plan must reject atomically.");
            e.Work.Cancel();e.CancelFloorWallUpgradeV414();stock=Stock(e);revision=e.LootLayoutRevisionV5;m.Wave++;e.OnNewWave();
            check("new-wave-stock-and-retained-material",e.LootLayoutRevisionV5==revision+1&&Stock(e)==stock&&e.Sources.All(source=>!source.AutoLooted),"Finite once-per-new-wave restock preserves earned materials.");
            revision=e.LootLayoutRevisionV5;e.OnNewWave();
            check("new-wave-hook-idempotent",e.LootLayoutRevisionV5==revision,"Repeated transition cannot restock twice.");
            e=New(5904);m=e.Model;
            foreach(var supply in e.WorldSupplies.Where(s=>s.ItemId=="water"||s.ItemId=="food").ToArray())if(e.Count(supply.ItemId)==0)e.TryCollectWorldSupply(supply.Id);
            budget=m.Budget;m.TryEnterShelterV5(m.RequiredShelterV5);m.Tick(8);m.Tick(4);m.Tick(3);
            int rewarded=m.Budget;
            check("actual-success-ledger",m.Phase==PanicPhase.Inspection&&m.CompletedWaves==1&&m.LastReward>0&&m.LastWaveLedgerV56.ActualCredited==rewarded-budget&&m.LastWaveLedgerV56.BaseReward==1600,"Real first-wave resolution and current ledger formula.");
            m.Tick(20);m.Tick(20);
            check("reward-idempotent",m.Budget==rewarded&&m.CompletedWaves==1,"Repeated result ticks cannot farm cash.");
            e=New(5905);m=e.Model;m.SetDeveloperOption(PanicDeveloperOption.NoLoss,true);budget=m.Budget;m.TryEnterShelterV5(m.RequiredShelterV5);m.Tick(8);m.Tick(4);m.Tick(3);
            check("protected-no-award",m.LastReward==0&&m.Budget==budget&&m.CompletedWaves==0&&m.CheatedRun,"No-loss preserves no-award / score isolation.");
        }
        static string Stock(PanicExpedition e) { return string.Join("|",PanicExpedition.MaterialIds.Select(id=>e.Count(id).ToString()).ToArray()); }
    }
}
