using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Profiling;

namespace TooLateToPanic
{
    // Root-controlled opt-in component. It never auto-attaches in normal play.
    public sealed class PanicE2ESystemAudit : MonoBehaviour
    {
        [Serializable] public sealed class Row{public string id,detail;public bool passed;}
        [Serializable] public sealed class Cycle
        {
            public int cycle,simulations,skippedShortSegments;public float seconds;
            public long managedAfterGC,allocated,reserved,workingSet,privateBytes;
            public int audioSources,particles,gameObjects,textures,renderTextures,components,audioAll,particlesAll,gameObjectsAll,componentsAll;
            public int musicComponents,musicSources,musicPlaying,actorCueRenderers;
            public bool duplicateAudioObserved,workActive,moving;
            public string note="Post-menu sample; explicit GC only at boundaries. Global event listener multiplicity is not introspected; warnings/errors and duplicate result/cue symptoms are sampled.";
        }
        [Serializable] public sealed class Report
        {
            public string startedUtc,finishedUtc,evidenceType="NATIVE_API isolated model/file fixtures plus real-time lifecycle semantic APIs; no OS input",limitations;
            public bool finished;public float seconds,requestedSoakSeconds;public int minimumCycles=10,completedCycles,naturalSimulationOutcomes,campaignFailureOutcomes,skippedShortSegments;
            public int lifecycleInjectedTick=0,lifecycleRunnerWarp=0,lifecycleDeveloperMutation=0;
            public string isolatedScoreBefore,isolatedScoreAfter,modelReport,realScorePath,realScoreBefore,realScoreAfter;public List<Row> checks=new List<Row>();
            public List<Cycle> cycles=new List<Cycle>();public List<string> errors=new List<string>(),warnings=new List<string>();
        }
        PanicHomeWorld world;PanicHomeHud hud;Report report=new Report();string directory;float began;bool finished;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--e2e-system-audit")<0)return;
            var host=FindFirstObjectByType<PanicHomeWorld>();if(host)host.gameObject.AddComponent<PanicE2ESystemAudit>();
        }
        void Start(){StartCoroutine(Guarded(Run()));}
        IEnumerator Guarded(IEnumerator action)
        {
            var stack=new Stack<IEnumerator>();stack.Push(action);
            while(stack.Count>0&&!finished)
            {
                bool more=false;object current=null;
                try{more=stack.Peek().MoveNext();if(more)current=stack.Peek().Current;}
                catch(Exception e){report.errors.Add(e.ToString());Add("harness.unhandled",false,e.Message);if(!string.IsNullOrEmpty(directory))Finish();else Application.Quit(2);yield break;}
                if(!more){stack.Pop();continue;}
                if(current is IEnumerator child)stack.Push(child);else yield return current;
            }
        }
        void Update(){if(!finished&&began>0&&Time.realtimeSinceStartup-began>(report.requestedSoakSeconds>0?report.requestedSoakSeconds+300f:120f)){report.errors.Add("Bounded watchdog expired");Finish();}}
        IEnumerator Run()
        {
            world=GetComponent<PanicHomeWorld>();hud=GetComponent<PanicHomeHud>();directory=world?world.EvidencePath:null;
            var args=Environment.GetCommandLineArgs();Guid guid;string profile=string.IsNullOrEmpty(directory)?string.Empty:Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar));
            if(string.IsNullOrEmpty(directory)||profile.Length<36||!Guid.TryParse(profile.Substring(profile.Length-36),out guid)||
               args.Any(a=>a.EndsWith("-smoke",StringComparison.Ordinal)||a.EndsWith("-audit",StringComparison.Ordinal)&&a!="--e2e-system-audit"||a.StartsWith("--e2e-",StringComparison.Ordinal)&&a!="--e2e-system-audit"&&a!="--e2e-telemetry"&&a!="--e2e-soak-seconds"))
            {Debug.LogError("System audit requires a fresh GUID --home-evidence and sole gameplay driver.");enabled=false;Application.Quit(2);yield break;}
            if(File.Exists(Path.Combine(directory,"isolated-home-scores.json"))){Debug.LogError("System audit refuses pre-existing main score fixture.");enabled=false;Application.Quit(2);yield break;}
            Application.logMessageReceived+=OnLog;Application.runInBackground=true;
            world.FullFlowScriptedInput=true;
            began=Time.realtimeSinceStartup;report.startedUtc=DateTime.UtcNow.ToString("o");
            report.realScorePath=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");report.realScoreBefore=Hash(report.realScorePath);
            report.limitations="Model section injects fixtures/Tick in separate GUID subfolders using Unity JsonUtility; lifecycle section uses production UI/model/world APIs, natural8+4s simulation clocks and owned spawns, no TimeScale change or runner Warp. Short segments are deliberately skipped, not earned. No OS input, first-player enjoyment, independent GPU timings, event delegate census or full campaign acceptance.";
            int arg=Array.IndexOf(args,"--e2e-soak-seconds");float seconds=0;
            if(arg>=0&&(arg+1>=args.Length||!float.TryParse(args[arg+1],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out seconds)||float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<1800||seconds>3600))
            {Add("soak.invalid-parameter",false,"Explicit soak must be1800..3600 seconds; no short run labelled soak.");Finish();yield break;}
            report.requestedSoakSeconds=seconds;
            var models=PanicE2ESystemModelChecks.Run(Path.Combine(directory,"model-fixtures"),"MODEL_UNITY_NATIVE_JSON");
            report.modelReport=Path.Combine(directory,"system-model-checks.json");File.WriteAllText(report.modelReport,JsonUtility.ToJson(models,true));
            Add("model.completed",models.finished&&models.errors.Count==0,"Finished="+models.finished+"; pass="+models.passed+"; fail="+models.failed+". Failed rows remain failed.");
            Add("model.no-failed-assertions",models.failed==0,"Do not promote completed model report with failing assertions to PASS.");
            report.isolatedScoreBefore=Hash(Path.Combine(directory,"isolated-home-scores.json"));
            yield return null;
            world.StartGame(false);yield return Wait(.4f);
            bool initiallyClean=!world.Model.CheatedRun;
            hud.SetDeveloperMenuOpen(true);float clock=world.Model.TimeRemaining;yield return Wait(.3f);
            Add("developer.panel-no-cheat-freezes-model",hud.DeveloperMenuOpen&&initiallyClean&&!world.Model.CheatedRun&&Mathf.Abs(world.Model.TimeRemaining-clock)<.001f,"Open panel freezes normal model without marking cheat.");
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,"system-developer-panel.png"));yield return Wait(.2f);
            bool inventory=world.InventoryOpen;world.DispatchWorldInput(new[]{KeyCode.Tab,KeyCode.E},true,false);
            Add("developer.world-input-blocked",world.InventoryOpen==inventory&&!world.WorkActive,"World dispatch blocked while panel owns input.");
            hud.SetDeveloperMenuOpen(false);Add("developer.close-transition-blocked",hud.DeveloperInputBlockedThisFrame,"Close frame remains gated.");
            world.ReturnToMenu();yield return null;
            // Private UI fixture only: verify saved pause/help ownership without altering clocks, costs or physics.
            var paused=typeof(PanicHomeHud).GetField("_paused",BindingFlags.Instance|BindingFlags.NonPublic);
            var help=typeof(PanicHomeHud).GetField("_helpVisible",BindingFlags.Instance|BindingFlags.NonPublic);
            var pause=typeof(PanicHomeHud).GetMethod("SetPaused",BindingFlags.Instance|BindingFlags.NonPublic);
            help.SetValue(hud,true);pause.Invoke(hud,new object[]{true});hud.SetDeveloperMenuOpen(true);hud.SetDeveloperMenuOpen(false);
            Add("developer.prior-help-pause-restored",(bool)paused.GetValue(hud)&&(bool)help.GetValue(hud),"Private HUD fixture; prior help+pause retained by panel open/close.");
            help.SetValue(hud,false);pause.Invoke(hud,new object[]{false});yield return null;
            Sample(0);Flush();
            if(seconds<=0){report.isolatedScoreAfter=Hash(Path.Combine(directory,"isolated-home-scores.json"));Add("main-profile-no-score-write",report.isolatedScoreBefore==report.isolatedScoreAfter,"Model fixtures use independent GUID subfolders; UI panel check has no ranked outcome.");Finish();yield break;}
            float soakStart=Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup-soakStart<seconds||report.completedCycles<report.minimumCycles)
            {
                if(Time.realtimeSinceStartup-soakStart>seconds+180){Add("soak.watchdog",false,"Wall limit exceeded; incomplete cycle not counted.");break;}
                int cycle=report.completedCycles+1;
                world.StartJuryDemo();yield return Wait(.4f);int skips=0;
                float shortDeadline=Time.realtimeSinceStartup+65f;
                while(world.JuryDemo!=null&&world.JuryDemo.Stage!=PanicJuryStage.Complete&&Time.realtimeSinceStartup<shortDeadline)
                {
                    if(world.JuryDemo.CanSkipSegment){world.SkipJurySegment();skips++;}
                    else if(world.JuryDemo.CanAdvance)world.AdvanceJuryDemo();
                    // Onboarding and quake keep their natural deadlines; no fake movement,
                    // injected Tick or direct stage mutation. Pending goals may be missed.
                    yield return Wait(.4f);
                }
                bool shortClosed=world.JuryDemo!=null&&world.JuryDemo.Stage==PanicJuryStage.Complete;
                Add("cycle."+cycle+".short-skipped-lifecycle",shortClosed,"All short-flow segments explicitly skipped; no readiness/task success inferred. count="+skips);report.skippedShortSegments+=skips;
                world.ReturnToMenu();yield return Wait(.3f);
                foreach(DisasterType type in new[]{DisasterType.Earthquake,DisasterType.Flood,DisasterType.Fire,DisasterType.Storm})
                {
                    bool started=world.StartDisasterSimulation(type);yield return Wait(.4f);world.TriggerSimulation();
                    float until=Time.realtimeSinceStartup+45f;
                    while(Time.realtimeSinceStartup<until&&world.Model.Phase!=PanicPhase.GameOver&&world.Model.Phase!=PanicPhase.Inspection)yield return null;
                    bool terminal=world.Model.Phase==PanicPhase.GameOver||world.Model.Phase==PanicPhase.Inspection;
                    Add("cycle."+cycle+".simulation."+type,started&&terminal&&world.Model.SimulationComplete&&world.Model.LastReward==0&&world.Model.LastWaveScore==0,"Natural real-time outcome; damage="+world.Model.TotalDamage+"; phase="+world.Model.Phase+"; completed="+world.Model.SimulationComplete);
                    if(terminal)report.naturalSimulationOutcomes++;
                    until=Time.realtimeSinceStartup+12f;while(Time.realtimeSinceStartup<until&&(world.CinematicActive||world.LossAnnounced))yield return null;
                    float resolvedDamage=world.Model.TotalDamage;string resolvedFailure=world.Model.FailureReason;
                    bool reviewed=world.Model.Phase==PanicPhase.Inspection;
                    if(world.Model.Phase==PanicPhase.GameOver)reviewed=world.InspectSimulationDamage();
                    yield return Wait(.4f);
                    Add("cycle."+cycle+".review."+type,reviewed&&world.Model.Phase==PanicPhase.Inspection&&world.Model.SimulationComplete&&Mathf.Abs(world.Model.TotalDamage-resolvedDamage)<.0001f&&world.Model.FailureReason==resolvedFailure&&world.Model.LastReward==0&&world.Model.LastWaveScore==0,"Actual inspection preserves honest failure/damage/outcome; controller="+world.PlayerControllerEnabled);
                    world.ReturnToMenu();yield return Wait(.4f);
                    if(!terminal){Finish();yield break;}
                }
                world.StartGame(false);yield return Wait(.3f);world.Model.EnterBunker();
                float failureDeadline=Time.realtimeSinceStartup+45f;
                while(Time.realtimeSinceStartup<failureDeadline&&world.Model.Phase!=PanicPhase.GameOver)yield return null;
                bool failed=world.Model.Phase==PanicPhase.GameOver;
                Add("cycle."+cycle+".campaign-bag-failure",failed&&!world.Model.CheatedRun&&world.Model.CompletedWaves==0,"Natural campaign empty-bag failure; no progress fixture; legitimate0-wave result may persist in isolated main profile.");if(failed)report.campaignFailureOutcomes++;
                world.ReturnToMenu();yield return Wait(.4f);report.completedCycles++;
                if(cycle%2==0||cycle==1)Sample(cycle);Flush();
                if(!failed){Finish();yield break;}
            }
            Add("soak.duration-cycles",Time.realtimeSinceStartup-soakStart>=seconds&&report.completedCycles>=10,"actualSeconds="+(Time.realtimeSinceStartup-soakStart)+"; completedCycles="+report.completedCycles);
            Add("soak.no-active-work-moving",!world.WorkActive&&!world.IsMoving,"Post-menu transient work/move state.");
            report.isolatedScoreAfter=Hash(Path.Combine(directory,"isolated-home-scores.json"));Sample(report.completedCycles);Finish();
        }
        IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
        void Sample(int cycle)
        {
            GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();long working=0,priv=0;
            try{using(var p=System.Diagnostics.Process.GetCurrentProcess()){working=p.WorkingSet64;priv=p.PrivateMemorySize64;}}catch{}
            var all=FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            var row=new Cycle{cycle=cycle,seconds=Time.realtimeSinceStartup-began,managedAfterGC=GC.GetTotalMemory(true),allocated=Profiler.GetTotalAllocatedMemoryLong(),reserved=Profiler.GetTotalReservedMemoryLong(),workingSet=working,privateBytes=priv,audioSources=all.Length,particles=FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length,gameObjects=FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length,textures=Resources.FindObjectsOfTypeAll<Texture>().Length,renderTextures=Resources.FindObjectsOfTypeAll<RenderTexture>().Length,components=FindObjectsByType<Component>(FindObjectsSortMode.None).Length,workActive=world.WorkActive,moving=world.IsMoving,simulations=report.naturalSimulationOutcomes,skippedShortSegments=report.skippedShortSegments};
            row.audioAll=FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;row.particlesAll=FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;row.gameObjectsAll=FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;row.componentsAll=FindObjectsByType<Component>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
            row.duplicateAudioObserved=all.Where(a=>a&&a.isPlaying&&a.clip).GroupBy(a=>a.clip.GetInstanceID()).Any(g=>g.Count()>1);
            var music=FindObjectsByType<PanicBackgroundMusic>(FindObjectsInactive.Include,FindObjectsSortMode.None);row.musicComponents=music.Length;row.musicSources=music.Sum(m=>m.GetComponents<AudioSource>().Length);row.musicPlaying=music.Sum(m=>m.GetComponents<AudioSource>().Count(a=>a.isPlaying));
            row.actorCueRenderers=world.GetComponent<PanicActorOcclusion>().OverlayCount;
            Add("sample."+cycle+".owned-music-and-cue",row.musicComponents==1&&row.musicSources==2&&row.actorCueRenderers>0,"One music component/two crossfade sources; cue count="+row.actorCueRenderers+". Same-clip music overlap during1.2s loop/phase crossfade is intentional, not listener-leak proof.");
            report.cycles.Add(row);
        }
        static string Hash(string path){if(!File.Exists(path))return "ABSENT";using(var s=SHA256.Create())using(var f=File.OpenRead(path))return BitConverter.ToString(s.ComputeHash(f)).Replace("-","");}
        void Add(string id,bool ok,string detail){report.checks.Add(new Row{id=id,passed=ok,detail=detail});}
        void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(type+": "+message+"\n"+stack);else if(type==LogType.Warning)report.warnings.Add(message);}
        void Flush(){report.seconds=Time.realtimeSinceStartup-began;File.WriteAllText(Path.Combine(directory,"system-audit.json"),JsonUtility.ToJson(report,true));}
        void Finish()
        {
            if(finished)return;finished=true;int exit=1;
            try
            {
                report.realScoreAfter=Hash(report.realScorePath);Add("real-profile-preserved",report.realScoreBefore==report.realScoreAfter,"Hash-only real profile; all writes assigned GUID subfolders.");
                report.finished=true;report.finishedUtc=DateTime.UtcNow.ToString("o");Flush();
                exit=report.errors.Count==0&&report.checks.All(c=>c.passed)?0:1;
            }
            catch(Exception ex){report.errors.Add("Finalization: "+ex);Debug.LogError("SYSTEM_AUDIT_FINALIZATION_FAILED "+ex.Message);}
            finally{Application.logMessageReceived-=OnLog;Application.Quit(exit);}
        }
        void OnDestroy(){Application.logMessageReceived-=OnLog;}
    }
}
