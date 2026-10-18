using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in checks. Caller MUST supply an isolated score path first; no Unity/player startup.</summary>
    public static class PanicRevisionModelChecks
    {
        public static IReadOnlyList<string> Run()
        {
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride))
                throw new InvalidOperationException("Revision checks require an isolated ScorePathOverride.");
            var passed=new List<string>();
            Action<bool,string> check=(ok,name)=>{if(!ok)throw new InvalidOperationException("Revision check failed: "+name);passed.Add(name);};
            PanicModel m=NewRun();PanicExpedition e=new PanicExpedition(m);
            check(Used(e.Inventory)==0 && e.Inventory.Count==12 && e.Bag.Count==8,"12 inventory / 8 bag reset empty");
            bool materialOnly=true;
            foreach(var source in e.Sources)foreach(var item in source.Contents)materialOnly &= PanicExpedition.IsConstructionMaterial(item.Id);
            check(materialOnly,"containers contain only construction materials");
            bool forbidden=false;
            foreach(var supply in e.WorldSupplies)forbidden |= supply.ItemId=="bandage"||supply.ItemId=="antiseptic"||PanicExpedition.IsConstructionMaterial(supply.ItemId);
            check(!forbidden && e.WorldSupplies.Count==12,"world supplies omit removed ingredients and construction stock");
            int budget=m.Budget;
            check(e.ToggleSource("cabinet") && e.Count("screws")>=2 && e.Count("bracket")>=1,"first opening awards bounded RNG stock");
            check(m.Budget==budget && Used(e.Inventory)==0,"auto loot uses currency without slots or budget");
            int screws=e.Count("screws"),bracket=e.Count("bracket");
            e.ToggleSource("cabinet");e.ToggleSource("cabinet");
            check(e.Count("screws")==screws && e.Count("bracket")==bracket,"reopen cannot roll or pay twice");
            e.OnNewWave();e.ToggleSource("cabinet");e.ToggleSource("cabinet");
            check(e.Count("screws")==screws && e.Count("bracket")==bracket,"new wave cannot repay construction containers");
            check(!e.GatherAt("cabinet",0) && !e.CraftFirstAid(),"legacy gathering and first aid craft do not produce stock");
            check(!m.BuyWater()&&!m.BuyFood()&&!m.BuySupport(0)&&m.Budget==budget,"erzak cannot be bought through old model APIs");
            e.Reset();check(e.Count("screws")==0 && !e.Sources[0].AutoLooted,"reset clears construction balance and loot identity");
            check(e.TryCollectWorldSupply("supply_water_kitchen") && e.Count("water")==2,"finite world pickup enters inventory");
            check(!e.TryCollectWorldSupply("supply_water_kitchen") && e.Count("water")==2,"world pickup cannot duplicate");
            e.Reset();Fill(e.Inventory,"firstaid",1);e.Inventory[0]=new SupplySlot("water",4);
            string before=Stock(e);int remaining=e.FindWorldSupply("supply_water_kitchen").Count;
            check(!e.TryCollectWorldSupply("supply_water_kitchen") && before==Stock(e) && remaining==e.FindWorldSupply("supply_water_kitchen").Count,"insufficient partial capacity rejects entire world pickup");
            e.Inventory[11]=new SupplySlot();
            check(e.TryCollectWorldSupply("supply_water_kitchen") && e.Inventory[0].Count==5 && e.Inventory[11].Id=="water"&&e.Inventory[11].Count==1,"4 plus 2 partial fill becomes 5 plus 1");
            check(e.ToggleSource("garage_tools")&&e.Count("bracket")>=2&&Used(e.Inventory)==12,"full inventory still auto collects materials");

            e.Reset();e.Inventory[0]=new SupplySlot("water",4);e.Inventory[1]=new SupplySlot("water",2);
            check(e.TransferSlots(false,1,false,0)&&e.Inventory[0].Count==5&&e.Inventory[1].Count==1,"exact slot merge preserves overflow in source");
            e.Inventory[2]=new SupplySlot("food",2);
            check(e.TransferSlots(false,1,false,2)&&e.Inventory[1].Id=="food"&&e.Inventory[2].Id=="water","different inventory stacks swap");
            before=Stock(e);
            check(!e.TransferSlots(false,-1,true,0)&&!e.TransferSlots(false,0,true,99)&&!e.TransferSlots(false,0,true,0,-1)&&before==Stock(e),"invalid transfers have no stock mutation");
            e.Reset();e.Inventory[0]=new SupplySlot("water",2);e.Inventory[1]=new SupplySlot("food",1);
            check(e.TransferSlots(false,0,true,0)&&m.PackedWater==2&&e.Count("water")==0,"packing stack publishes model and bag together");
            check(e.TransferSlots(false,1,true,0)&&m.PackedFood==1&&m.PackedWater==0&&e.Inventory[1].Id=="water","cross boundary swap synchronizes both water food counters");
            check(e.TransferSlots(true,0,false,2)&&m.PackedFood==0&&e.Inventory[2].Id=="food","unpack restores loose supply and clears bag counter");
            e.Reset();Fill(e.Bag,"firstaid",1);e.Inventory[0]=new SupplySlot("water",1);before=Stock(e);
            check(!e.PackSlot(0)&&before==Stock(e),"full bag denies packing atomically");

            e.Reset();e.Inventory[0]=new SupplySlot("flashlight",1);before=Stock(e);
            check(!e.PackSlot(0)&&before==Stock(e)&&m.PackedSupportMask==0,"device without battery is fully rejected");
            e.Inventory[1]=new SupplySlot("battery",2);
            check(e.PackSlot(0)&&e.Count("battery")==1&&m.PackedSupportMask==2,"device packing consumes exactly one battery");
            check(e.TransferSlots(true,0,true,3)&&e.Count("battery")==1&&m.PackedSupportMask==2,"reorder packed device does not consume another battery");
            check(e.UnpackSlot(3)&&m.PackedSupportMask==0&&e.Count("flashlight")==1,"unpacking device clears packed support bit");
            int flashlight=Find(e.Inventory,"flashlight");
            check(e.PackSlot(flashlight)&&e.Count("battery")==0,"repacking device uses a new real battery");
            e.Reset();e.Inventory[0]=new SupplySlot("battery",2);
            check(e.TransferSlots(false,0,true,0)&&e.Bag[0].Id=="battery"&&e.Bag[0].Count==2,"spare batteries can be carried in bag");
            e.Reset();e.Bag[0]=new SupplySlot("water",3);m.PackedWater=3;e.Inventory[0]=new SupplySlot("water",2);before=Stock(e);
            check(!e.TransferSlots(false,0,true,0)&&before==Stock(e)&&m.PackedWater==3,"three day water cap retained without partial loss");
            e.Reset();e.Bag[0]=new SupplySlot("firstaid",1);m.PackedSupportMask=1;e.Inventory[0]=new SupplySlot("firstaid",1);before=Stock(e);
            check(!e.TransferSlots(false,0,true,1)&&before==Stock(e),"support category remains unique");

            m=NewRun();e=new PanicExpedition(m);e.ToggleSource("garage_tools");
            PanicItem target=m.Items.Find(i=>i.Id=="bookshelf");budget=m.Budget;screws=e.Count("screws");bracket=e.Count("bracket");
            check(e.CanBeginWork("anchorwall","bookshelf"),"work preflight reads numeric material balance");
            e.Work.Begin("anchorwall","bookshelf");e.Work.Cancel();
            check(m.Budget==budget&&e.Count("screws")==screws&&e.Count("bracket")==bracket,"cancelled work spends neither budget nor materials");
            e.Work.Begin("anchorwall","bookshelf");Install(e.Work,0);Install(e.Work,1);
            check(e.TryCompleteWork("anchorwall","bookshelf")&&target.Anchored&&m.Budget==budget-60&&e.Count("screws")==screws-2&&e.Count("bracket")==bracket-1,"completed anchor spends canonical currency recipe exactly once");
            before=Stock(e);budget=m.Budget;
            check(!e.TryCompleteWork("anchorwall","bookshelf")&&budget==m.Budget&&before==Stock(e),"repeated completion cannot double spend");

            m=NewRun();e=new PanicExpedition(m);float time=m.TimeRemaining;m.SetPaused(true);m.Tick(40);
            check(m.TimeRemaining==time&&!m.CheatedRun,"campaign pause freezes time without cheat marking");
            before=Stock(e);check(!e.ToggleSource("cabinet")&&!e.TryCollectWorldSupply("supply_water_kitchen")&&before==Stock(e),"paused expedition mutations reject");
            m.SetPaused(false);m.Tick(1);check(Math.Abs(m.TimeRemaining-time+1)<.001f,"campaign resumes without catching up paused time");
            m=NewRun();m.Phase=PanicPhase.Disaster;m.PhaseTime=7;m.SetPaused(true);m.Tick(2);
            check(m.PhaseTime==7&&m.Phase==PanicPhase.Disaster,"disaster pause freezes phase countdown");
            m.Phase=PanicPhase.Aftermath;m.PhaseTime=6;m.Tick(2);
            check(m.PhaseTime==6&&m.Phase==PanicPhase.Aftermath,"aftermath pause freezes phase countdown");
            m=new PanicModel();check(m.StartSimulation(DisasterType.Flood),"simulation starts");time=m.TimeRemaining;m.SetPaused(true);m.Tick(10);
            check(m.TimeRemaining==time,"simulation clock respects real pause");
            m=new PanicModel();e=new PanicExpedition(m);var flow=new PanicJuryDemo(m,e);check(flow.Start(),"short flow starts with physical supply stock");
            check(e.WorldSupplies.FindSupplyCount("supply_water_kitchen")==1&&e.Sources.TrueForAll(s=>s.Contents.Count==0),"short flow water exists only in world");
            m.SetPaused(true);time=flow.ElapsedSeconds;float stage=flow.StageRemainingSeconds;
            flow.Tick(50,new Vector3(10,0,0),true,false);
            check(flow.ElapsedSeconds==time&&flow.StageRemainingSeconds==stage&&flow.Stage==PanicJuryStage.Onboarding,"short flow pause freezes clock and objectives");
            check(!flow.SkipCurrentSegment()&&!flow.Advance()&&!flow.TryReachShelter(PanicJuryShelter.QuakeCover,PanicJuryDemo.QuakeCoverPosition,true),"paused flow rejects skip advance and shelter mutation");
            m.SetPaused(false);flow.Tick(1,PanicJuryDemo.SpawnPosition,true,false);
            check(Math.Abs(flow.ElapsedSeconds-time-1)<.001f,"short flow resumes from unchanged clock");
            check(e.TryCollectWorldSupply("supply_water_kitchen")&&e.JuryWaterGatheredCount==1,"short flow genuine world pickup sets objective provenance");
            m=NewRun();m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials,true);m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials,false);
            check(m.CheatedRun&&!m.FreeMaterials,"developer cheat marking remains sticky");
            m.Tick(1000);check(m.Highscores.Count==0,"cheated run cannot persist a highscore");
            return passed.AsReadOnly();
        }
        static PanicModel NewRun(){var m=new PanicModel();m.StartRun(false);return m;}
        static void Fill(List<SupplySlot> slots,string id,int count){for(int i=0;i<slots.Count;i++)slots[i]=new SupplySlot(id,count);}
        static int Used(List<SupplySlot> slots){int n=0;foreach(var s in slots)if(s.Count>0)n++;return n;}
        static int Find(List<SupplySlot> slots,string id){return slots.FindIndex(s=>s.Id==id&&s.Count>0);}
        static string Stock(PanicExpedition e)
        {
            var s=new StringBuilder();foreach(var slot in e.Inventory)s.Append(slot.Id).Append(':').Append(slot.Count).Append('|');
            s.Append('/');foreach(var slot in e.Bag)s.Append(slot.Id).Append(':').Append(slot.Count).Append('|');
            foreach(var id in PanicExpedition.MaterialIds)s.Append(id).Append(':').Append(e.Count(id)).Append('|');return s.ToString();
        }
        static void Install(PanicWorkSession work,int index)
        {if(!work.TryClick(index))throw new InvalidOperationException("Work click rejected");for(int i=0;i<8;i++)work.Tick(.1f,0,false,false,false);}
        static int FindSupplyCount(this IReadOnlyList<PanicWorldSupply> supplies,string id)
        {foreach(var supply in supplies)if(supply.Id==id)return supply.Count;return -1;}
    }
}
