using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in accelerated native Model/Expedition campaign. No OS, actor route or rendered FPS claim.</summary>
    public sealed class PanicWave50Smoke : MonoBehaviour
    {
        public const string Flag = "--wave50-smoke";
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        const string Seed = "0x20261016";
        [Serializable] public sealed class CheckRow { public string id,status,detail; }
        [Serializable] public sealed class ScheduleRow { public string seed,disaster; public int wave; }
        [Serializable] public sealed class WaveRow
        {
            public string seed,disaster,phasechain,bagBefore,bagAfter,stockBefore,stockAfter,materialsBefore,materialsAfter,failure,precautions;
            public int wave,completedWaves,budgetBefore,budgetAfter,reward,finitePickupCount,workCommits,runtimeErrors;
            public long score,lastWaveScore;
            public float preparationSeconds,damage;
            public bool bagStageAt45,cheated,stalled;
        }
        [Serializable] public sealed class WorkRow
        { public string action,target,materialsBefore,materialsAfter; public int wave,budgetBefore,budgetAfter; public bool committed; }
        [Serializable] public sealed class Receipt
        {
            public string startedUtc,finishedUtc,dllSha256,scorePath,realScoreBefore,realScoreAfter,error,earlyStop;
            public string layer="NATIVE_ACCELERATED_MODEL_EXPEDITION_API";
            public string limits="World.Update is suspended after the genuine StartGame/guide/intro actions. All phase clocks use production Model.Tick; world pickups and work use production Expedition APIs without proximity. No physical route, OS input, rendered FPS camera, real-time duration or human 50-wave acceptance.";
            public bool complete,passed,completed50,osInput=false,humanAcceptance=false;
            public int startGameCalls,onboardingConfirmCalls,introSkipCalls,worldUpdateSuspensions,productionStartWarps=1,setupWarps=0,scheduleWaveFixtures,modelTickCalls,workInputSamples,sourceOpenCalls,sourceReopens,finitePickupCalls,inventoryPackCalls,newWaveCalls,counterFixtures;
            public float elapsedSeconds,simulatedSeconds;
            public List<CheckRow> checks=new List<CheckRow>();
            public List<WaveRow> waves=new List<WaveRow>();
            public List<ScheduleRow> schedule=new List<ScheduleRow>();
            public List<WorkRow> work=new List<WorkRow>();
            public List<string> errors=new List<string>();
        }
        readonly Receipt report=new Receipt();
        PanicHomeWorld world; PanicModel model; PanicExpedition expedition;
        float began; bool finished,worldWasEnabled; string realScore;
        Coroutine run;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Attach()
        {
            if(!Environment.GetCommandLineArgs().Contains(Flag))return;
            var target=FindFirstObjectByType<PanicHomeWorld>();
            if(!target)throw new InvalidOperationException("Wave50 requires the production Residence world.");
            if(!target.GetComponent<PanicWave50Smoke>())target.gameObject.AddComponent<PanicWave50Smoke>();
        }
        void Start()
        {
            if(!Environment.GetCommandLineArgs().Contains(Flag)){enabled=false;return;}
            world=GetComponent<PanicHomeWorld>(); began=Time.realtimeSinceStartup;
            report.startedUtc=DateTime.UtcNow.ToString("o");
            report.dllSha256=Hash(typeof(PanicModel).Assembly.Location);
            report.scorePath=PanicModel.ScorePathOverride;
            realScore=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");
            report.realScoreBefore=Hash(realScore);
            Application.runInBackground=true;Application.logMessageReceived+=OnLog;
            run=StartCoroutine(Guard(Run()));
        }
        void Update()
        {
            if(!finished&&Time.realtimeSinceStartup-began>240f)
            {report.error="240s watchdog; remaining campaign waves NOT_RUN.";if(run!=null)StopCoroutine(run);Finish();}
        }
        void OnLog(string message,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(message+"\n"+trace);}
        IEnumerator Guard(IEnumerator body)
        {
            var stack=new Stack<IEnumerator>();stack.Push(body);
            while(stack.Count>0)
            {
                bool more;object value=null;
                try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;}
                catch(Exception e){report.error=e.ToString();break;}
                if(!more){stack.Pop();continue;}
                if(value is IEnumerator nested)stack.Push(nested);else yield return value;
            }
            report.complete=string.IsNullOrEmpty(report.error);Finish();
        }
        void Check(string id,bool ok,string detail="")
        {report.checks.Add(new CheckRow{id=id,status=ok?"PASS":"FAIL",detail=detail});Debug.Log("WAVE50 "+(ok?"PASS ":"FAIL ")+id+" "+detail);}
        void Tick(float delta)
        {model.Tick(delta);report.modelTickCalls++;report.simulatedSeconds+=delta;}
        static string Hash(string path)
        {if(string.IsNullOrEmpty(path)||!File.Exists(path))return "MISSING";using(var sha=SHA256.Create())using(var input=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(input)).Replace("-","");}
        static string Bag(PanicModel m){return "water="+m.PackedWater+";food="+m.PackedFood+";support="+m.PackedSupportMask;}
        static string Materials(PanicExpedition e){return string.Join(";",PanicExpedition.MaterialIds.Select(id=>id+"="+e.Count(id)));}
        static string Stock(PanicExpedition e){return string.Join(";",e.WorldSupplies.Select(s=>s.Id+"="+s.Count))+"|inventory:"+string.Join(";",e.Inventory.Where(s=>s.Count>0).Select(s=>s.Id+"="+s.Count));}
        static string Precautions(PanicModel m)
        {return string.Join(";",m.Items.Where(i=>i.Anchored||i.Tier>0||i.FloodProtected||i.FireProtected).Select(i=>i.Id+":"+i.Anchored+":"+i.Tier+":"+i.FloodProtected+":"+i.FireProtected))+"|trees="+m.TreeCount+";drains="+m.DrainCount+";fire="+m.FireToolCount+";utilities="+m.UtilitySafety.CompletedCount;}

        IEnumerator Run()
        {
            if(!world||string.IsNullOrEmpty(world.EvidencePath)||string.IsNullOrEmpty(report.scorePath)||
                Path.GetFullPath(report.scorePath)!=Path.GetFullPath(Path.Combine(world.EvidencePath,"isolated-home-scores.json")))
                throw new InvalidOperationException("Wave50 refuses non-isolated score state.");
            Check("launch.isolated",true,report.scorePath);
            RunScheduleChecks();
            RunCounterChecks();
            world.StartGame(false);report.startGameCalls++;
            yield return null;
            var guide=world.GetComponent<PanicHomeHud>();
            var guideOpen=guide?guide.GetType().GetProperty("FreeplayOnboardingOpenV414"):null;
            if(guideOpen!=null&&Equals(guideOpen.GetValue(guide),true))
            {
                var confirm=guide.GetType().GetMethod("ConfirmFreeplayOnboardingV414");
                if(confirm==null)throw new MissingMethodException("Guide requires its genuine confirm action.");
                report.onboardingConfirmCalls++;
                Check("guide.confirm",Equals(confirm.Invoke(guide,null),true));
                yield return null;
                Check("guide.closed",!Equals(guideOpen.GetValue(guide),true));
            }
            else report.checks.Add(new CheckRow{id="guide.absent_or_already_closed",status="NOT_RUN",detail="No guide field was written; baseline R4 may have no R5 onboarding."});
            if(world.IntroStoryActiveV414){world.SkipIntroStoryV414();report.introSkipCalls++;yield return null;}
            model=world.Model;expedition=world.Expedition;
            Check("campaign.normal_start",model.Phase==PanicPhase.Preparing&&!model.IsSimulation&&!model.IsJuryDemo&&!model.IsTutorial&&!model.DeveloperAnyEnabled&&!model.CheatedRun&&!model.IsPaused);
            if(model.IsPaused)throw new InvalidOperationException("Genuine guide confirmation did not release model pause.");
            worldWasEnabled=world.enabled;world.enabled=false;report.worldUpdateSuspensions++;
            Check("campaign.initial165",Mathf.Abs(model.TimeRemaining-165f)<.2f,"One launch frame may elapse before driver owns the model clock.");
            Check("duration45.canonical-normal-contract",PanicModel.NormalPrecautionSeconds==120f&&PanicModel.NormalBagSeconds==45f&&PanicModel.NormalPreparationSeconds==165f&&PanicHomeWorld.BagPreparationSeconds==45f,
                "Exact normal120precaution+45bag=165; no real-time/camera claim in accelerated campaign.");
            foreach(var source in expedition.Sources)
            {
                Check("source.open."+source.Id,expedition.ToggleSource(source.Id));report.sourceOpenCalls++;
                string pool=Materials(expedition);
                expedition.ToggleSource(source.Id);expedition.ToggleSource(source.Id);report.sourceReopens+=2;
                Check("source.oneshot."+source.Id,pool==Materials(expedition)&&source.AutoLooted&&source.Contents.Count==0);
            }
            for(int number=1;number<=50;number++)
            {
                if(model.Phase!=PanicPhase.Preparing||model.Wave!=number)
                {report.earlyStop="Unexpected phase/wave at next preparation: "+model.Phase+"/"+model.Wave;break;}
                var row=new WaveRow{seed=Seed,wave=number,disaster=model.Disaster.ToString(),preparationSeconds=model.TimeRemaining,budgetBefore=model.Budget,stockBefore=Stock(expedition),materialsBefore=Materials(expedition),phasechain=model.Phase.ToString()};
                int pickupBefore=report.finitePickupCalls,workBefore=report.work.Count;
                PrepareWithFiniteResources();
                row.workCommits=report.work.Count-workBefore;
                Tick(Mathf.Max(.001f,model.TimeRemaining-45f));
                row.bagStageAt45=world.BagFinalStageActive;
                Check("wave."+number+".bag_boundary",row.bagStageAt45&&Mathf.Abs(model.TimeRemaining-45f)<.01f,"Logical final-45s gate; rendered camera NOT_RUN.");
                PackEssential("water");PackEssential("food");
                row.bagBefore=Bag(model);row.finitePickupCount=report.finitePickupCalls-pickupBefore;
                row.precautions=Precautions(model);
                Check("wave."+number+".finite_bag",model.BagReady&&expedition.Bag.Where(s=>s.Id=="water").Sum(s=>s.Count)==model.PackedWater&&expedition.Bag.Where(s=>s.Id=="food").Sum(s=>s.Count)==model.PackedFood);
                int beforeCompleted=model.CompletedWaves;long beforeScore=model.Score;
                Check("wave."+number+".bunker",model.EnterBunker());row.phasechain+=">"+model.Phase;
                Tick(8f);row.phasechain+=">"+model.Phase;
                expedition.SyncConsumedBagSupplies();row.bagAfter=Bag(model);row.damage=model.TotalDamage;
                Check("wave."+number+".aftermath",model.Phase==PanicPhase.Aftermath);
                Tick(4f);row.phasechain+=">"+model.Phase;
                if(model.Phase==PanicPhase.Success)
                {
                    row.reward=model.LastReward;row.lastWaveScore=model.LastWaveScore;
                    Check("wave."+number+".award_once",model.CompletedWaves==beforeCompleted+1&&model.Score==beforeScore+model.LastWaveScore&&model.LastReward>0);
                    Tick(3f);row.phasechain+=">"+model.Phase;
                    long scoreOnce=model.Score;int completedOnce=model.CompletedWaves,budgetOnce=model.Budget;
                    Tick(.1f);
                    Check("wave."+number+".no_repeat_award",model.Score==scoreOnce&&model.CompletedWaves==completedOnce&&model.Budget==budgetOnce);
                    Check("wave."+number+".damage_bounds",!float.IsNaN(model.TotalDamage)&&model.TotalDamage>=0&&model.TotalDamage<PanicModel.DamageLossLimit&&model.Items.All(i=>i.Damage>=0&&i.Damage<=100.001f));
                    string retained=Precautions(model),finite=Materials(expedition);
                    model.FinishInspection();row.phasechain+=">"+model.Phase;
                    Check("wave."+number+".next_wave",model.Wave==number+1&&model.Phase==PanicPhase.Preparing&&model.TimeRemaining==165f&&model.RequiredDays==1&&model.TotalDamage==0&&model.Items.All(i=>i.Damage==0f&&i.Contribution==0f));
                    Check("wave."+number+".persistent_precautions",retained==Precautions(model));
                    expedition.OnNewWave();report.newWaveCalls++;
                    Check("wave."+number+".refill",finite==Materials(expedition)&&expedition.WorldSupplies.Where(s=>s.ItemId=="water"||s.ItemId=="food"||s.ItemId=="battery").All(s=>s.Count==s.InitialCount));
                    int next=model.Wave;DisasterType nextType=model.Disaster;
                    model.FinishInspection();
                    Check("wave."+number+".continue_once",model.Wave==next&&model.Disaster==nextType&&model.Phase==PanicPhase.Preparing);
                }
                else report.earlyStop="Real campaign ended at wave "+number+": "+model.FailureReason;
                row.failure=model.FailureReason;row.completedWaves=model.CompletedWaves;row.score=model.Score;
                row.budgetAfter=model.Budget;row.stockAfter=Stock(expedition);row.materialsAfter=Materials(expedition);
                row.cheated=model.CheatedRun;row.runtimeErrors=report.errors.Count;
                row.stalled=model.Phase!=PanicPhase.Preparing&&model.Phase!=PanicPhase.GameOver;
                report.waves.Add(row);
                Check("wave."+number+".normal_rules",!model.DeveloperAnyEnabled&&!model.CheatedRun&&!model.DeveloperNoLossUsedThisWave&&model.Budget>=0);
                Flush();
                if(!string.IsNullOrEmpty(report.earlyStop))break;
                yield return null;
            }
            report.completed50=model.CompletedWaves==50&&model.Wave==51&&report.waves.Count==50;
            Check("campaign.completed50",report.completed50,report.earlyStop);
            Check("campaign.finite_only",report.setupWarps==0&&!model.CheatedRun&&!model.DeveloperAnyEnabled,"StartGame's normal spawn counted separately; no inventory, bag, damage, tier, budget or CompletedWaves fields were assigned by the campaign driver.");
            Check("campaign.sequence_matches",report.waves.All(row=>report.schedule.Any(s=>s.wave==row.wave&&s.disaster==row.disaster)));
            Check("scores.real_untouched",report.realScoreBefore==Hash(realScore));
            yield return null;
        }
        void PrepareWithFiniteResources()
        {
            foreach(var action in new[]{"gasoff","wateroff","poweroff"})TryWork(action,PanicUtilitySafetyV414.TargetForAction(action));
            // Finite authored stock is allocated once. Persisting installations are never re-granted.
            foreach(var item in model.Items.OrderByDescending(i=>Risk(i,DisasterType.Earthquake)))
            {string action=expedition.PrimaryActionV414(item);if(action=="anchorwall"||action=="anchorfloor")TryWork(action,item.Id);}
            TryWork("plant","plant_west1");TryWork("plant","plant_west2");TryWork("plant","plant_east");
            TryWork("fire","fire_station");TryWork("hose","hose_station");
            TryWork("drain","drain_west");TryWork("drain","drain_east");
            foreach(var item in model.Items.OrderByDescending(i=>Risk(i,DisasterType.Storm)))
                for(int tier=0;tier<2;tier++)if(expedition.PrimaryActionV414(item)=="upgrade")TryWork("upgrade",item.Id);
        }
        static float Risk(PanicItem item,DisasterType type)
        {return (float)typeof(PanicModel).GetMethod("BaseRisk",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{item,type});}
        void TryWork(string action,string target)
        {
            if(!expedition.CanBeginWork(action,target))return;
            var item=model.Items.Find(i=>i.Id==target);
            int budget=model.Budget,cost=expedition.WorkBudgetCostV414(action,target);
            var costs=expedition.WorkMaterialCostsV414(action,target).ToArray();
            var owned=costs.ToDictionary(c=>c.Id,c=>expedition.Count(c.Id));
            var row=new WorkRow{wave=model.Wave,action=action,target=target,budgetBefore=budget,materialsBefore=Materials(expedition)};
            if(!expedition.BeginValidatedWorkV414(action,target,false))return;
            for(int guard=0;guard<64&&expedition.Work.Active&&!expedition.Work.Complete;guard++)
            {
                if(expedition.Work.UsesDirectClick)
                {
                    expedition.Work.TryClick(expedition.Work.CompletedSteps);
                    if(expedition.Work.UsesLinearDrag)expedition.Work.ApplyLinearDisplacement(.5f,true);
                    else expedition.Work.ApplyRotation(60f*expedition.Work.ExpectedDirection,true);
                }
                else expedition.Work.PressHammer();
                report.workInputSamples++;
            }
            row.committed=expedition.TryCompleteWork(action,target);
            row.budgetAfter=model.Budget;row.materialsAfter=Materials(expedition);report.work.Add(row);
            Check("work."+model.Wave+"."+action+"."+target,row.committed&&model.Budget==budget-cost&&costs.All(c=>expedition.Count(c.Id)==owned[c.Id]-c.Required));
        }
        void PackEssential(string id)
        {
            int packed=id=="water"?model.PackedWater:model.PackedFood;
            if(packed>=1)return;
            int index=expedition.Inventory.FindIndex(s=>s.Id==id&&s.Count>0);
            if(index>=0){Check("pack."+model.Wave+"."+id,expedition.PackSlot(index));report.inventoryPackCalls++;return;}
            var supply=expedition.WorldSupplies.FirstOrDefault(s=>s.ItemId==id&&!s.Taken);
            if(supply==null){Check("pickup."+model.Wave+"."+id,false,"No finite world supply available.");return;}
            int count=supply.Count;
            report.finitePickupCalls++;
            bool collected=expedition.TryCollectWorldSupply(supply.Id);
            Check("pickup."+model.Wave+"."+id,collected&&count>0&&supply.Count==0);
            string bag=Bag(model),stock=Stock(expedition);
            Check("pickup."+model.Wave+".duplicate."+id,!expedition.TryCollectWorldSupply(supply.Id)&&Bag(model)==bag&&Stock(expedition)==stock);
        }
        void RunScheduleChecks()
        {
            var select=typeof(PanicModel).GetMethod("SelectDisasterForWave",Hidden);
            var a=new PanicModel();a.StartRun(false);
            var b=new PanicModel();b.StartRun(false);
            for(int wave=1;wave<=51;wave++)
            {
                if(wave>1){a.Wave=b.Wave=wave;select.Invoke(a,null);select.Invoke(b,null);report.scheduleWaveFixtures+=2;}
                report.schedule.Add(new ScheduleRow{seed=Seed,wave=wave,disaster=a.Disaster.ToString()});
                Check("sequence."+wave+".deterministic",a.Disaster==b.Disaster);
                if(wave<=9)Check("sequence."+wave+".intro",a.Disaster==(wave<=3?DisasterType.Earthquake:wave<=6?DisasterType.Flood:DisasterType.Fire));
                if(wave>=10)Check("sequence."+wave+".no_triple",!(report.schedule[wave-1].disaster==report.schedule[wave-2].disaster&&report.schedule[wave-2].disaster==report.schedule[wave-3].disaster));
            }
            Check("sequence.four_disasters",report.schedule.Take(50).Select(r=>r.disaster).Distinct().Count()==4,"Baseline %3 is expected to FAIL this check; staged %4 must include Storm.");
            a.ReturnToTitle();a.StartRun(false);
            Check("restart.reset",a.Wave==1&&a.CompletedWaves==0&&a.Score==0&&a.Budget==1000&&a.TimeRemaining==165f&&a.PackedWater==0&&a.PackedFood==0&&!a.CheatedRun&&a.Disaster==DisasterType.Earthquake&&a.Items.All(i=>i.Damage==0));
        }
        void RunCounterChecks()
        {
            var methods=typeof(PanicModel).GetMethods(BindingFlags.Static|BindingFlags.NonPublic).Where(m=>m.Name=="SaturatingAdd").ToArray();
            foreach(var method in methods)
            {
                bool ints=method.GetParameters()[0].ParameterType==typeof(int);
                object value=method.Invoke(null,ints?new object[]{int.MaxValue-1,7}:new object[]{long.MaxValue-1,7L});report.counterFixtures++;
                Check("counter.saturates."+(ints?"int":"long"),ints?(int)value==int.MaxValue:(long)value==long.MaxValue,"Separate static arithmetic fixture; campaign counters untouched.");
            }
            Check("loss.threshold",!PanicModel.DamageCausesLoss(74.999f)&&PanicModel.DamageCausesLoss(75f));
        }
        static string Csv(object value)
        {return "\""+Convert.ToString(value,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
        void Flush()
        {
            File.WriteAllText(Path.Combine(world.EvidencePath,"wave50-report.json"),JsonUtility.ToJson(report,true));
            var csv=new StringBuilder("seed,wave,disaster,preparationseconds,bag_stage_at45,phasechain,bagbefore,bagafter,stockbefore,stockafter,materials_before,materials_after,finitepickupcount,completedWaves,reward,score,budgetbefore,budgetafter,damage,runtimeerrors,stall,cheated,failure\n");
            foreach(var r in report.waves)csv.AppendLine(string.Join(",",new object[]{r.seed,r.wave,r.disaster,r.preparationSeconds,r.bagStageAt45,r.phasechain,r.bagBefore,r.bagAfter,r.stockBefore,r.stockAfter,r.materialsBefore,r.materialsAfter,r.finitePickupCount,r.completedWaves,r.reward,r.score,r.budgetBefore,r.budgetAfter,r.damage,r.runtimeErrors,r.stalled,r.cheated,r.failure}.Select(Csv)));
            File.WriteAllText(Path.Combine(world.EvidencePath,"campaign-waves.csv"),csv.ToString());
            File.WriteAllText(Path.Combine(world.EvidencePath,"schedule-only.csv"),"seed,wave,disaster,layer\n"+string.Join("\n",report.schedule.Select(r=>r.seed+","+r.wave+","+r.disaster+",NATIVE_SCHEDULE_REFLECTION_FIXTURE")));
        }
        void Finish()
        {
            if(finished)return;finished=true;
            report.finishedUtc=DateTime.UtcNow.ToString("o");report.elapsedSeconds=Time.realtimeSinceStartup-began;
            report.realScoreAfter=Hash(realScore);report.passed=report.complete&&report.completed50&&report.errors.Count==0&&report.checks.All(c=>c.status!="FAIL")&&report.realScoreBefore==report.realScoreAfter;
            if(world&&worldWasEnabled)world.enabled=true;
            Application.logMessageReceived-=OnLog;
            if(world&&!string.IsNullOrEmpty(world.EvidencePath))Flush();
            Debug.Log("WAVE50_DONE passed="+report.passed+" completed50="+report.completed50+" rows="+report.waves.Count+" error="+report.error+" stop="+report.earlyStop);
            Application.Quit(report.passed?0:2);
        }
    }
}
