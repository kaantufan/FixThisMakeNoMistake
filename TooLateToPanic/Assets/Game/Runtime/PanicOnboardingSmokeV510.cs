using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    // Explicit isolated API/render driver. Fixtures are recorded; no OS input or natural-route claim.
    public sealed partial class PanicOnboardingSmokeV510 : MonoBehaviour
    {
        const string Flag = "--onboarding-v510-smoke";
        const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public class Row { public string id, detail; public bool pass; }
        [Serializable] public class Shot { public string path, sha256, context; public int width, height, nonDark; }
        [Serializable] public class Report
        {
            public string utc, dllSha256, scorePath, realScoreBefore, realScoreAfter, error;
            public string layer = "CURRENT_DLL_NATIVE_PRODUCTION_API_AND_FRAMEBUFFER_WITH_DECLARED_FIXTURES";
            public bool complete, passed, OSInput, humanAcceptance;
            public float seconds;
            public List<Row> checks = new List<Row>();
            public List<Shot> captures = new List<Shot>();
            public List<string> fixtures = new List<string>();
            public List<string> errors = new List<string>();
        }
        readonly Report report = new Report();
        PanicHomeWorld w; PanicHomeHud hud;
        float began; bool finished, priorMotion; string realScore;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (world) world.gameObject.AddComponent<PanicOnboardingSmokeV510>();
        }
        void Start()
        {
            w=GetComponent<PanicHomeWorld>();hud=GetComponent<PanicHomeHud>();began=Time.realtimeSinceStartup;
            report.utc=DateTime.UtcNow.ToString("o");report.dllSha256=Hash(typeof(PanicModel).Assembly.Location);
            report.scorePath=PanicModel.ScorePathOverride;realScore=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");
            report.realScoreBefore=Hash(realScore);priorMotion=w.ReducedFpsMotionV54;
            Application.logMessageReceived+=Log;Application.runInBackground=true;
            StartCoroutine(Guard(Run()));
        }
        void Log(string message,string stack,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(message+"\n"+stack);}
        void Check(string id,bool pass,string detail="")
        {report.checks.Add(new Row{id=id,pass=pass,detail=detail});Debug.Log("ONBOARDING_V510 "+(pass?"PASS ":"FAIL ")+id+" "+detail);}
        static string Hash(string path)
        {if(!File.Exists(path))return "absent";using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        static void Set(object target,string name,object value){target.GetType().GetField(name,Fields).SetValue(target,value);}
        static T Read<T>(object target,string name)
        {var type=target.GetType();var property=type.GetProperty(name,Fields);return (T)(property!=null?property.GetValue(target):type.GetField(name,Fields).GetValue(target));}
        static object Call(object target,string name,params object[] args)
        {return target.GetType().GetMethods(Fields).First(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(target,args);}
        IEnumerator Wait(float seconds){float until=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<until)yield return null;}
        IEnumerator Capture(string name,string context)
        {
            yield return new WaitForEndOfFrame();
            var texture=ScreenCapture.CaptureScreenshotAsTexture();
            if(!texture){Check("capture."+name,false,"Missing framebuffer");yield break;}
            string path=Path.Combine(w.EvidencePath,name+".png");byte[] bytes=texture.EncodeToPNG();File.WriteAllBytes(path,bytes);
            var pixels=texture.GetPixels32();int nonDark=0;byte min=255,max=0;
            for(int i=0;i<pixels.Length;i+=137){var p=pixels[i];byte l=(byte)((p.r+p.g+p.b)/3);if(l>12)nonDark++;min=Math.Min(min,l);max=Math.Max(max,l);}
            Check("capture."+name,nonDark>80&&max-min>18,"Actual framebuffer "+texture.width+"x"+texture.height+"; nonDark="+nonDark);
            report.captures.Add(new Shot{path=path,sha256=Hash(path),context=context,width=texture.width,height=texture.height,nonDark=nonDark});Destroy(texture);
        }
        IEnumerator Resolution(int width,int height){Screen.SetResolution(width,height,FullScreenMode.Windowed);yield return Wait(.3f);}
        IEnumerator BeginNormal()
        {
            hud.SetPaused(false);w.ReturnToMenu();w.StartGame(false);w.FullFlowScriptedInput=true;yield return null;
            if(hud.FreeplayOnboardingOpenV414)hud.ConfirmFreeplayOnboardingV414();yield return null;w.SkipIntroStoryV414();yield return Wait(.25f);
        }
        IEnumerator Run()
        {
            Check("isolated",!string.IsNullOrEmpty(w.EvidencePath)&&report.scorePath==Path.Combine(w.EvidencePath,"isolated-home-scores.json"),report.scorePath);
            report.fixtures.Add("Fresh GUID profile; production StartGame/intro skip and UI modal APIs. FullFlowScriptedInput prevents physical keyboard sampling. Actor poses, facing, preparation time and selected test state are explicitly set, never OS input or natural traversal.");
            report.fixtures.Add("Actual stocked source ToggleSource, work completion APIs and finite supply collection are used. Temporary stock-empty/ranking/wave fixtures are restored or discarded by a fresh normal run. No real profile or old-player input.");
            yield return Resolution(1600,900);
            yield return FeatureChecks();
            Check("runtime.no-errors",report.errors.Count==0,string.Join("\n",report.errors));report.complete=true;
        }
        IEnumerator FeatureChecks()
        {
            PanicAutoFocusChecksV510.Run(Check);
            var detached=new LootSource{Id="detached",Contents=new List<SupplySlot>{new SupplySlot("plank",1)}};
            Check("loot.policy.positive",PanicLootSourceShimmerPolicyV510.HasUncollectedStock(detached));
            detached.AutoLooted=true;Check("loot.policy.collected-off",!PanicLootSourceShimmerPolicyV510.HasUncollectedStock(detached));
            detached.AutoLooted=false;detached.Contents[0].Count=0;Check("loot.policy.empty-off",!PanicLootSourceShimmerPolicyV510.HasUncollectedStock(detached));
            Check("firstwave.policy.exclusions",!PanicFirstWavePolicyV510.IsRequired(true,2,PanicPhase.Preparing,false,false,false)&&
                !PanicFirstWavePolicyV510.IsRequired(true,1,PanicPhase.Preparing,false,true,false)&&
                !PanicFirstWavePolicyV510.IsRequired(true,1,PanicPhase.Preparing,true,false,false)&&
                !PanicFirstWavePolicyV510.IsRequired(true,1,PanicPhase.Preparing,false,false,true));
            w.StartGame(false);w.FullFlowScriptedInput=true;yield return null;
            Check("firstwave.real-start",w.FirstWaveOnboardingRequiredV510&&hud.FreeplayOnboardingOpenV414&&w.Model.Budget==1500&&Mathf.Abs(w.Model.TimeRemaining-130)<.01f);
            float clock=w.Model.TimeRemaining;yield return Wait(.2f);
            Check("intro.existing-pause",w.Model.IsPaused&&Mathf.Abs(clock-w.Model.TimeRemaining)<.001f);
            yield return Capture("first-wave-purpose-1600","Actual first normal start, mandatory purpose/control modal; clock paused by existing onboarding UI.");
            hud.ConfirmFreeplayOnboardingV414();yield return null;w.SkipIntroStoryV414();yield return Wait(.4f);
            w.SetAssistantEnabledV55(false);yield return new WaitForEndOfFrame();
            Check("firstwave.cannot-hide",w.FirstWaveOnboardingRequiredV510&&hud.FirstWaveCardEligibleV510&&hud.FirstWaveCardVisibleV510&&!w.AssistantEnabledV55);
            Check("firstwave.real-gather-required",!w.FirstWaveOnboardingV510.GatherComplete&&w.FirstWaveOnboardingV510.Step==PanicFirstWaveStepV510.Materials);
            yield return Capture("first-wave-materials-1600","Persistent first-wave action card remains with optional helper OFF.");
            w.Warp(new Vector3(4,.05f,1.5f));yield return Wait(.5f);
            Check("loot.all14-independent",w.Expedition.Sources.Count==14&&w.Expedition.Sources.All(s=>w.IsLootSourceGlowEligibleV510(s.Id)),"Every stocked source, including unselected/out-of-range IDs.");
            Check("loot.real-mesh-submission",w.LootShimmerEligibleSourceCountV510==14&&w.LootShimmerMeshDrawCountV510>0&&w.LootShimmerDrawFrameV510>=Time.frameCount-1);
            yield return Capture("all-stocked-glow-1600","Actual visible stocked kitchen/storage meshes; first-wave intended target glow and nearby selection have independent owners.");
            w.SetReducedFpsMotionV54(true);yield return Wait(.1f);
            Check("loot.reduced-motion",w.LootShimmerReducedMotionV510&&w.LootShimmerMeshDrawCountV510>0);
            yield return Capture("stocked-reduced-motion-1600","Time-independent source shader under existing reduced-motion preference.");w.SetReducedFpsMotionV54(priorMotion);
            foreach(var source in w.Expedition.Sources)
            {
                Check("loot.collect."+source.Id,w.Expedition.ToggleSource(source.Id)&&source.AutoLooted&&!w.LootSourceStockedForShimmerV510(source.Id),"Actual finite material collection API, no controller/path claim.");
            }
            yield return Wait(.15f);
            Check("loot.all-collected-off",w.LootShimmerEligibleSourceCountV510==0&&w.LootShimmerMeshDrawCountV510==0);
            int material=w.Expedition.Count("plank");var first=w.Expedition.Sources[0];w.Expedition.ToggleSource(first.Id);w.Expedition.OnNewWave();
            Check("loot.no-reopen-reroll",!w.LootSourceStockedForShimmerV510(first.Id)&&material==w.Expedition.Count("plank"));
            Check("firstwave.real-loot-progress",w.FirstWaveOnboardingV510.GatherComplete&&w.FirstWaveOnboardingV510.Step==PanicFirstWaveStepV510.Anchor);
            yield return Capture("all-looted-anchor-step-1600","Same world after actual source consumption: storage shimmer off, real anchor objective active.");
            foreach(var source in w.Expedition.Sources) if(source.Open) w.Expedition.ToggleSource(source.Id);
            // Facing/pose are explicit scene fixtures; the original collider/floor/ray queries still decide eligibility.
            var hero=Read<Transform>(w,"hero");bool checkedScene=false;
            foreach(var position in new[]{new Vector3(4,.05f,1.5f),new Vector3(-4,.05f,-2),new Vector3(0,.05f,-5.5f)})
            {
                w.Warp(position);yield return null;
                for(int yaw=0;yaw<360;yaw+=45)
                {
                    hero.rotation=Quaternion.Euler(0,yaw,0);Set(w,"facingYaw",(float)yaw);w.SetFocus(null);Call(w,"ResetAutoFocusV510");
                    var probe=w.RefreshAutoFocusV510(false);string expected=PanicAutoFocusPolicyV510.ChooseNearestFacing(probe.Candidates);
                    Check("focus.scene-rank."+position.x+"."+yaw,probe.InputAllowed&&probe.FocusId==expected,"Actual native surface distances and obstruction queries, candidate count="+probe.Candidates.Length);
                    if(expected==null)continue;
                    checkedScene=true;var manual=w.RefreshAutoFocusV510(true);string chosen=manual.FocusId;
                    var held=w.RefreshAutoFocusV510(false);Check("focus.q-held."+position.x+"."+yaw,held.ManualSelectionHeld&&held.FocusId==chosen);
                    hero.rotation=Quaternion.Euler(0,yaw+90,0);Set(w,"facingYaw",(float)yaw+90);
                    var changed=w.RefreshAutoFocusV510(false);Check("focus.turn-releases."+position.x+"."+yaw,!changed.ManualSelectionHeld&&changed.FocusId==changed.AutomaticCandidateId);
                }
            }
            Check("focus.actual-frontal-candidates",checkedScene);
            var anchor=w.FirstWaveOnboardingV510;var item=w.Model.Items.Find(i=>i.Id==anchor.TargetId);
            bool beganWork=false;
            if(item!=null)
            {
                for(int a=0;a<16&&!beganWork;a++)
                {
                    float angle=a*Mathf.PI/8;w.Warp(item.Position+new Vector3(Mathf.Cos(angle)*1.3f,.05f,Mathf.Sin(angle)*1.3f));
                    yield return null;w.BeginWork(PanicItemActionRules.PrimaryAction(item),item.Id);beganWork=w.WorkActive;
                }
            }
            Check("anchor.real-work-open",beganWork,"Declared actor pose search; original floor/range/line-of-sight and costs unchanged.");
            if(beganWork)
            {
                yield return Capture("mandatory-work-step-1600","Real two-screw work view with persistent compact first-wave step strip.");
                int budget=w.Model.Budget;int expectedCost=PanicExpedition.WorkCost(PanicItemActionRules.PrimaryAction(item),item);
                for(int i=0;i<2;i++){Check("anchor.click."+i,w.Expedition.Work.TryClick(i));Check("anchor.rotation."+i,w.Expedition.Work.ApplyRotation(w.Expedition.Work.RequiredDegrees,true));}
                w.CompleteWork();yield return null;
                Check("anchor.real-commit-once",item.Anchored&&w.Model.Budget==budget-expectedCost&&w.FirstWaveOnboardingV510.AnchorComplete);
                Check("firstwave.utility-next",w.FirstWaveOnboardingV510.Step==PanicFirstWaveStepV510.Utilities);
            }
            yield return Resolution(960,540);yield return new WaitForEndOfFrame();
            Check("firstwave.card-layout-960",hud.FirstWaveCardVisibleV510&&hud.FirstWaveCardBoundsV510.yMax<800&&hud.FirstWaveContentHeightV510<=hud.FirstWaveCardBoundsV510.height);
            yield return Capture("first-wave-guidance-960","Actual 960x540 first-wave card and location/action text; logical layout checks are separate from human readability.");
            w.Model.TimeRemaining=w.Model.PhaseTime=39f;w.TickPreparationStages();yield return Wait(2.4f);
            Check("fps.real-stage",w.BagFirstPersonActive&&w.FirstWaveOnboardingV510.Step==PanicFirstWaveStepV510.Water&&w.LootShimmerEligibleSourceCountV510==0);
            yield return Capture("first-wave-water-960","Real FPS phase entered by declared clock fixture; water instruction and storage-shimmer stage gate.");
            var water=w.Expedition.WorldSupplies.First(s=>s.ItemId=="water"&&s.Count>0);
            Check("water.finite-api",w.Expedition.TryCollectWorldSupply(water.Id)&&w.FirstWaveOnboardingV510.WaterComplete);
            Check("firstwave.food-next",w.FirstWaveOnboardingV510.Step==PanicFirstWaveStepV510.Food);
            yield return Capture("first-wave-food-960","Actual inventory water count advances required guidance to food.");
            var food=w.Expedition.WorldSupplies.First(s=>s.ItemId=="food"&&s.Count>0);
            Check("food.finite-api",w.Expedition.TryCollectWorldSupply(food.Id)&&w.FirstWaveOnboardingV510.FoodComplete);
            Check("firstwave.actual-shelter-next",w.FirstWaveOnboardingV510.Step==PanicFirstWaveStepV510.Shelter&&!w.Model.ReachedShelterV5);
            yield return Capture("first-wave-shelter-960","Actual supplies collected; instructions require real basement entry. No fabricated successful shelter admission.");
            w.Model.Wave=2;yield return null;Check("later-wave-normal-helper",!w.FirstWaveOnboardingRequiredV510&&!hud.FirstWaveCardEligibleV510);
            yield return BeginNormal();w.Model.TimeRemaining=w.Model.PhaseTime=9f;w.TickPreparationStages();yield return Wait(2.3f);
            Check("deadline.honest-fallback",w.FirstWaveOnboardingV510.Step==PanicFirstWaveStepV510.Shelter&&w.FirstWaveOnboardingV510.Fallback&&!w.FirstWaveOnboardingV510.WaterComplete&&!w.Model.ReachedShelterV5);
            w.ReturnToMenu();w.StartGame(true);yield return null;Check("tutorial.excluded",!w.FirstWaveOnboardingRequiredV510);
            w.FinishTutorial();yield return null;
            Check("tutorial.normal-wave-rearmed",w.FirstWaveOnboardingRequiredV510&&hud.FreeplayOnboardingOpenV414&&w.Model.Wave==1);
            hud.ConfirmFreeplayOnboardingV414();yield return null;
            w.enabled=false;yield return new WaitForEndOfFrame();
            Check("disabled.no-stale-presentation",!w.NotebookGlowActiveV59&&w.LootShimmerMeshDrawCountV510==0&&!hud.FirstWaveCardEligibleV510);
            w.enabled=true;yield return null;
            w.ReturnToMenu();w.StartGame(false);yield return null;Check("restart.required-again",w.FirstWaveOnboardingRequiredV510&&hud.FreeplayOnboardingOpenV414&&!w.FirstWaveOnboardingV510.GatherComplete);
            hud.ConfirmFreeplayOnboardingV414();yield return null;w.SkipIntroStoryV414();yield return Wait(.1f);
            w.Model.Wave=2;w.Expedition.OnNewWave();Check("loot.nextwave-restocked",w.Expedition.Sources.All(s=>w.LootSourceStockedForShimmerV510(s.Id)),"Declared wave2 fixture invokes original once-per-wave finite stock API.");
            w.Expedition.ToggleSource(w.Expedition.Sources[0].Id);w.Expedition.OnNewWave();Check("loot.restocks-once",!w.LootSourceStockedForShimmerV510(w.Expedition.Sources[0].Id));
            hud.SetPaused(true);yield return new WaitForEndOfFrame();Check("modal.no-stale-draws",w.LootShimmerMeshDrawCountV510==0);hud.SetPaused(false);
        }
        IEnumerator Guard(IEnumerator body)
        {
            var stack=new Stack<IEnumerator>();stack.Push(body);
            while(stack.Count>0&&!finished)
            {
                bool more;object current=null;
                try{more=stack.Peek().MoveNext();if(more)current=stack.Peek().Current;}
                catch(Exception error){report.error=error.ToString();break;}
                if(!more){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;
            }
            Finish();
        }
        void Update(){if(!finished&&Time.realtimeSinceStartup-began>180f){report.error="Bounded180s driver timeout";Finish();}}
        void Finish()
        {
            if(finished)return;finished=true;w.SetReducedFpsMotionV54(priorMotion);hud.SetPaused(false);w.ReturnToMenu();
            report.realScoreAfter=Hash(realScore);Check("profile.real-score-unchanged",report.realScoreBefore==report.realScoreAfter);
            report.seconds=Time.realtimeSinceStartup-began;
            report.passed=report.complete&&string.IsNullOrEmpty(report.error)&&report.errors.Count==0&&report.captures.Count>=6&&report.checks.All(c=>c.pass);
            Application.logMessageReceived-=Log;File.WriteAllText(Path.Combine(w.EvidencePath,"onboarding-v510-smoke.json"),JsonUtility.ToJson(report,true));
            Debug.Log("ONBOARDING_V510_RESULT "+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count+" captures="+report.captures.Count);Application.Quit(report.passed?0:2);
        }
    }
}
