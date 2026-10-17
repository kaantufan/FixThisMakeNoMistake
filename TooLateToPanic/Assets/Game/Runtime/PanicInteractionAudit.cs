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
    // Root-owned, opt-in QA. Ordinary players never attach this component.
    public sealed class PanicInteractionAudit : MonoBehaviour
    {
        [Serializable] public sealed class Row { public string id, name, layer, detail; public bool pass; }
        [Serializable] public sealed class Report
        {
            public string startedUtc, finishedUtc, profile, scorePath, realScorePath, realScoreBefore, realScoreAfter;
            public string evidenceType = "Native model + explicit world fixtures + real IMGUI Event dispatch, distinct from physical OS input";
            public string limitations = "World setup Warps and model time fixtures are explicit; no exhaustive human/OS/campaign/fun acceptance. IMGUI mouse/key events are injected inside an actual OnGUI callback. They test production hit testing and handlers, not hardware polling.";
            public bool completed, passed; public int checks, passedChecks; public float wallSeconds;
            public List<Row> rows = new List<Row>(); public List<string> errors = new List<string>(); public List<string> screenshots = new List<string>();
        }
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static readonly MethodInfo HudGui = typeof(PanicHomeHud).GetMethod("OnGUI", Private);
        readonly Report report = new Report();
        PanicHomeWorld w; PanicHomeHud hud; float began; bool finished; string layer = "native-imgui";
        Event queued; bool pending, quitObserved; int eventFrame = -1; Exception eventError;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--interaction-audit") < 0) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (world) world.gameObject.AddComponent<PanicInteractionAudit>();
        }
        void Start()
        {
            began = Time.realtimeSinceStartup; w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>();
            if (!w || !hud || string.IsNullOrEmpty(w.EvidencePath)) { Debug.LogError("AUDIT isolation missing"); Application.Quit(2); return; }
            report.profile = Path.GetFileName(w.EvidencePath); report.startedUtc = DateTime.UtcNow.ToString("o");
            report.scorePath = PanicModel.ScorePathOverride;
            report.realScorePath = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.realScoreBefore = Hash(report.realScorePath);
            Application.runInBackground = true; w.FullFlowScriptedInput = true;
            Application.logMessageReceived += Log;
            StartCoroutine(Guarded(Run()));
        }
        IEnumerator Guarded(IEnumerator root)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(root);
            while (stack.Count > 0 && !finished)
            {
                bool more = false; object current = null;
                try { more = stack.Peek().MoveNext(); if (more) current = stack.Peek().Current; }
                catch (Exception ex) { report.errors.Add(ex.ToString()); Check("harness exception", false, ex.Message); break; }
                if (!more) { stack.Pop(); continue; }
                if (current is IEnumerator nested) stack.Push(nested); else yield return current;
            }
            Finish();
        }
        void Update() { if (!finished && Time.realtimeSinceStartup - began > 420f) { report.errors.Add("420 second watchdog"); Finish(); } }
        void OnGUI()
        {
            if (!pending || Event.current.type != EventType.Repaint || eventFrame == Time.frameCount) return;
            eventFrame = Time.frameCount; pending = false;
            var previous = new Event(Event.current); var matrix = GUI.matrix;
            try { GUI.matrix = Matrix4x4.identity; Event.current = queued; HudGui.Invoke(hud, null); }
            catch (Exception ex) { eventError = ex; }
            finally { Event.current = previous; GUI.matrix = matrix; }
        }
        void Check(string name, bool pass, string detail = "")
        {
            report.checks++; if (pass) report.passedChecks++;
            report.rows.Add(new Row { id = "AUD-" + report.checks.ToString("D4"), name = name, pass = pass, detail = detail, layer = layer });
            Debug.Log("AUDIT_CHECK " + (pass ? "PASS " : "FAIL ") + name + " " + detail);
        }
        IEnumerator Run()
        {
            Guid g; bool fresh = report.profile.Length >= 36 && Guid.TryParse(report.profile.Substring(report.profile.Length - 36), out g) &&
                !File.Exists(report.scorePath) && Path.GetFullPath(report.scorePath) == Path.GetFullPath(Path.Combine(w.EvidencePath, "isolated-home-scores.json"));
            Check("fresh GUID isolated before model construction", fresh, report.scorePath); if (!fresh) yield break;
            while (!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            yield return Wait(.3f); yield return Shot("00-title");
            // This probe reproduces event-coordinate hit testing without OS cursor repositioning.
            yield return Click(278, 710);
            Check("mouse event coordinates open SERBEST OYUN", w.Model.Phase == PanicPhase.Preparing && !w.Model.IsJuryDemo,
                "MouseDown event=(278,710), hardware pointer=" + Input.mousePosition + "; phase=" + w.Model.Phase);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--audit-probe") >= 0) { report.completed = true; yield break; }
            yield return UiSuite();
            layer = "pure-model-fixture";
            InvokeModelAudit();
            layer = "native-world-fixture";
            var helper = typeof(PanicHomeWorld).Assembly.GetType("TooLateToPanic.PanicInteractionWorldAudit");
            if (helper == null) Check("world audit helper installed", false);
            else yield return (IEnumerator)helper.GetMethod("Run").Invoke(null, new object[] { w, hud, (Action<string,bool,string>)Check });
            var frameHelper = typeof(PanicHomeWorld).Assembly.GetType("TooLateToPanic.PanicInteractionFrameAudit");
            if (frameHelper == null) Check("frame audit helper installed", false);
            else yield return (IEnumerator)frameHelper.GetMethod("Run").Invoke(null, new object[] { w, hud, (Action<string,bool,string>)Check });
            var targetHelper = typeof(PanicHomeWorld).Assembly.GetType("TooLateToPanic.PanicInteractionTargetAudit");
            if (targetHelper == null) Check("target audit helper installed", false);
            else yield return (IEnumerator)targetHelper.GetMethod("Run").Invoke(null, new object[] { w, hud, (Action<string,bool,string>)Check });
            layer = "native-flow-fixture";
            yield return FlowSuite();
            w.ReturnToMenu(); yield return Wait(.05f);
            Application.wantsToQuit += ObserveQuit;
            yield return Click(278,774); yield return Wait(.08f);
            Check("title EXIT reaches native Application.Quit",quitObserved,"Native wantsToQuit intercept keeps this isolated audit alive until report flush");
            Application.wantsToQuit -= ObserveQuit;
            report.completed = true;
        }
        bool ObserveQuit() { quitObserved=true; return false; }
        void InvokeModelAudit()
        {
            var helper = typeof(PanicModel).Assembly.GetType("TooLateToPanic.PanicInteractionModelAudit");
            if (helper == null) { Check("model audit helper installed", false); return; }
            helper.GetMethod("Run").Invoke(null, new object[] { (Action<string,bool,string>)Check });
        }
        IEnumerator UiSuite()
        {
            int[,] sizes = { {1600,900},{1280,720},{1024,768},{960,540},{1920,1080} };
            for (int i=0; i<sizes.GetLength(0);i++)
            {
                Screen.SetResolution(sizes[i,0], sizes[i,1], false); yield return Wait(.15f);
                w.ReturnToMenu(); w.InputLocked=false; yield return Wait(.05f);
                string size=Screen.width+"x"+Screen.height;
                yield return Click(20,450); Check("blank title area no action "+size,w.Model.Phase==PanicPhase.Title);
                Vector2 rightPoint = new Vector2(278,710)*hud.CanvasScale+hud.CanvasOffset;
                yield return Send(new Event{type=EventType.MouseDown,button=1,mousePosition=rightPoint});
                yield return Send(new Event{type=EventType.MouseUp,button=0,mousePosition=rightPoint});
                Check("right click and mouse up cannot activate title button "+size,w.Model.Phase==PanicPhase.Title);
                yield return Click(450,642); Check("mouse selector opens "+size,hud.SimulationSelectorOpen);
                yield return Click(417,717); Check("mouse selector back "+size,!hud.SimulationSelectorOpen&&w.Model.Phase==PanicPhase.Title);
                yield return Click(278,710); Check("mouse free game "+size,w.Model.Phase==PanicPhase.Preparing&&!w.Model.IsJuryDemo);
                yield return Click(278,710); Check("repeated title click cannot reset running state "+size,w.Model.Phase==PanicPhase.Preparing&&w.Model.Budget==1000&&!w.InventoryOpen);
                yield return Wait(.05f); w.ToggleInventory(); yield return Wait(.03f);
                for(int slot=0;slot<12;slot++)
                {
                    yield return Click(198+(slot%4)*156,336+(slot/4)*113);
                    Check("inventory mouse slot "+slot+" "+size,w.InventoryIndex==slot);
                }
                yield return Click(642,784); Check("inventory mouse back "+size,!w.InventoryOpen);
                hud.SetDeveloperMenuOpen(true); yield return Wait(.05f);
                yield return Click(1081,736); yield return Wait(.03f);
                Check("developer mouse close "+size,!hud.DeveloperMenuOpen&&!w.InputLocked);
                yield return Shot("ui-"+size);
                Rect canvas=hud.LogicalCanvasBounds;
                Check("logical canvas scales and fits "+size,hud.CanvasScale>0 && canvas.width*hud.CanvasScale<=Screen.width+.1f && canvas.height*hud.CanvasScale<=Screen.height+.1f);
            }
            Screen.SetResolution(1600,900,false); yield return Wait(.15f);
            w.ReturnToMenu(); yield return Wait(.05f);
            yield return Click(450,570); Check("mouse title short preparation starts",w.JuryActive);
            w.ReturnToMenu(); yield return Wait(.05f);
            for(int choice=0;choice<4;choice++)
            {
                hud.OpenSimulationSelector(); yield return Wait(.03f);
                yield return Click(527+(choice%2)*538,328+(choice/2)*180);
                Check("mouse selects disaster "+choice,w.Model.IsSimulation&&(int)w.Model.Disaster==choice,"actual="+w.Model.Disaster);
                w.ReturnToMenu(); yield return Wait(.03f);
            }
            // Real key events traverse the same OnGUI title navigation branch.
            SetField(hud,"_menuChoice",0); yield return Key(KeyCode.UpArrow);
            Check("title up wraps to exit",(int)GetField(hud,"_menuChoice")==4);
            yield return Key(KeyCode.DownArrow); yield return Key(KeyCode.Return);
            Check("title keyboard Enter starts short flow",w.JuryActive);
            w.ReturnToMenu(); yield return Wait(.03f);
            yield return Click(616,710); Check("NASIL OYNANIR starts tutorial",w.Model.IsTutorial&&w.Model.Phase==PanicPhase.Tutorial);
            yield return Shot("tutorial-help");
            yield return Click(1348,781); yield return Wait(.03f);
            Check("tutorial mouse finish starts clean real run",!w.Model.IsTutorial&&w.Model.Phase==PanicPhase.Preparing&&w.Model.Budget==1000&&w.Model.Score==0);
            w.ReturnToMenu(); yield return Wait(.03f); yield return Click(616,710); yield return Key(KeyCode.N);
            Check("tutorial N matches mouse finish",!w.Model.IsTutorial&&w.Model.Phase==PanicPhase.Preparing&&w.Model.Budget==1000);
            CallHud("SetPaused",true); yield return Wait(.03f);
            yield return Click(1170,461); Check("pause restart opens clean free game",!w.Model.IsTutorial&&w.Model.Phase==PanicPhase.Preparing&&!w.InputLocked);
            yield return Wait(.03f); CallHud("SetPaused",true); yield return Wait(.03f);
            yield return Click(1170,381); Check("pause developer opens without cheat",hud.DeveloperMenuOpen&&!w.Model.CheatedRun);
            yield return Wait(.03f);
            int budget=w.Model.Budget;
            yield return Click(556,631); Check("developer budget button adds exactly1000",w.Model.Budget==budget+1000&&w.Model.CheatedRun,"before="+budget+" after="+w.Model.Budget);
            yield return Click(1081,736); yield return Wait(.03f);
            Check("developer preserves underlying pause",w.InputLocked);
            yield return Click(1166,297); Check("pause resume releases lock",!w.InputLocked);
            CallHud("SetPaused",true); yield return Wait(.03f); yield return Click(1170,541);
            Check("pause title clears run",w.Model.Phase==PanicPhase.Title&&!w.InventoryOpen&&!w.IsMoving&&!w.WorkActive&&!w.InputLocked);
            // Guarded modal must consume clicks at the old title action position.
            hud.SetDeveloperMenuOpen(true); yield return Wait(.03f); yield return Click(278,710);
            Check("developer overlay blocks title click through",hud.DeveloperMenuOpen&&w.Model.Phase==PanicPhase.Title);
            hud.SetDeveloperMenuOpen(false); yield return Wait(.03f);
            w.StartGame(false); yield return Wait(.05f); hud.SetDeveloperMenuOpen(true); yield return Wait(.05f);
            for(int option=0;option<4;option++)
            {
                yield return Click(800,319+option*80);
                bool enabled=DeveloperFlag(option);
                Check("developer mouse toggle "+option,enabled&&w.Model.CheatedRun);
                SetField(hud,"_developerChoice",option); yield return Key(KeyCode.Return);
                Check("developer Enter toggles same option off "+option,!DeveloperFlag(option)&&w.Model.CheatedRun);
            }
            hud.SetDeveloperMenuOpen(false); yield return Wait(.03f); w.ReturnToMenu(); yield return Wait(.03f);
            hud.SetDeveloperMenuOpen(true); yield return Wait(.03f); int titleBudget=w.Model.Budget;
            yield return Click(556,631); Check("disabled title budget button cannot mutate",w.Model.Budget==titleBudget);
            hud.SetDeveloperMenuOpen(false); yield return Wait(.03f);
        }
        bool DeveloperFlag(int option) { return option==0?w.Model.InfiniteBudget:option==1?w.Model.FreeMaterials:option==2?w.Model.FreezePreparationTime:w.Model.DeveloperNoLoss; }
        IEnumerator FlowSuite()
        {
            foreach(var type in new[]{DisasterType.Earthquake,DisasterType.Flood,DisasterType.Fire,DisasterType.Storm})
            {
                w.InputLocked=false; w.StartDisasterSimulation(type); yield return Wait(.08f);
                yield return Click(1300,750); yield return Wait(.06f);
                Check("simulation mouse launch "+type,w.Model.Phase==PanicPhase.Disaster&&w.CinematicActive);
                int water=w.Model.PackedWater; yield return Click(1300,750);
                Check("stale launch click cannot double launch "+type,w.Model.Phase==PanicPhase.Disaster&&w.Model.PackedWater==water);
                yield return Key(KeyCode.Return); yield return Wait(.06f);
                Check("disaster Enter advances once "+type,w.Model.Phase==PanicPhase.Aftermath);
                yield return Click(1390,64); yield return Wait(.06f);
                if(w.LossAnnounced) { yield return Wait(1.8f); if(w.CinematicActive) { yield return Key(KeyCode.Return); yield return Wait(.06f); } }
                Check("outcome resolves once "+type,w.Model.SimulationComplete&&w.Model.LastReward==0&&w.Model.Score==0,"phase="+w.Model.Phase+" damage="+w.Model.TotalDamage+" reason="+w.Model.FailureReason);
                if(w.Model.Phase==PanicPhase.GameOver)
                {
                    yield return Click(468,679); yield return Wait(.08f);
                    Check("HASARI INCELE restores real controller "+type,w.Model.Phase==PanicPhase.Inspection&&w.PlayerControllerEnabled);
                }
                if(w.Model.Phase==PanicPhase.Inspection)
                {
                    yield return Click(1170,750); yield return Wait(.06f);
                    Check("inspection retry resets simulation "+type,w.Model.Phase==PanicPhase.Preparing&&w.Model.IsSimulation&&!w.Model.SimulationComplete&&w.Model.PackedWater==1);
                }
                w.ReturnToMenu(); yield return Wait(.05f);
            }
            // Short segment skip remains honest about missing preparation.
            w.StartJuryDemo(); yield return Wait(.06f);
            for(int i=0;i<8&&w.JuryDemo.Stage!=PanicJuryStage.Complete;i++)
            {
                var before=w.JuryDemo.Stage;
                if(w.JurySnapshot.CanSkipSegment) { yield return Click(235,390); }
                else if(w.JurySnapshot.CanAdvance||before==PanicJuryStage.Earthquake) { yield return Key(KeyCode.Return); }
                else { w.JuryDemo.Tick(80f,w.PlayerPosition,true,false); }
                yield return Wait(.06f);
            }
            Check("short skip reaches truthful result",w.JuryDemo.Stage==PanicJuryStage.Result||w.JuryDemo.Stage==PanicJuryStage.Complete,"stage="+w.JuryDemo.Stage);
            yield return Shot("short-skipped-result");
            yield return Click(546,698); yield return Wait(.06f);
            Check("short result retry resets goals/stock",w.JuryDemo!=null&&w.JuryDemo.Stage==PanicJuryStage.Onboarding&&w.Model.PackedWater==0&&w.Expedition.Count("bracket")==1);
            w.ReturnToMenu(); yield return Wait(.03f);
        }
        IEnumerator Send(Event e)
        {
            if(pending)throw new InvalidOperationException("overlapping queued event");
            queued=e; pending=true; eventError=null; float end=Time.realtimeSinceStartup+3f;
            while(pending&&Time.realtimeSinceStartup<end)yield return null;
            if(pending)throw new TimeoutException("OnGUI event queue");
            if(eventError!=null)throw eventError;
            yield return null;
        }
        IEnumerator Click(float x,float y)
        {
            Vector2 p=new Vector2(x,y)*hud.CanvasScale+hud.CanvasOffset;
            yield return Send(new Event{type=EventType.MouseDown,button=0,mousePosition=p});
        }
        IEnumerator Key(KeyCode key) { yield return Send(new Event{type=EventType.KeyDown,keyCode=key}); }
        IEnumerator Wait(float seconds) { float end=Time.realtimeSinceStartup+seconds; while(Time.realtimeSinceStartup<end)yield return null; }
        IEnumerator Shot(string id)
        {
            yield return new WaitForEndOfFrame(); var texture=ScreenCapture.CaptureScreenshotAsTexture();
            string path=Path.Combine(w.EvidencePath,id+".png"); File.WriteAllBytes(path,texture.EncodeToPNG()); Destroy(texture); report.screenshots.Add(path);
        }
        static object GetField(object o,string name) { return o.GetType().GetField(name,Private).GetValue(o); }
        static void SetField(object o,string name,object value) { o.GetType().GetField(name,Private).SetValue(o,value); }
        void CallHud(string name,params object[] values) { typeof(PanicHomeHud).GetMethod(name,Private).Invoke(hud,values); }
        void Log(string msg,string trace,LogType type) { if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(msg+"\n"+trace); }
        static string Hash(string path) { if(!File.Exists(path))return "missing"; using(var s=SHA256.Create())return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(path))).Replace("-",""); }
        void Finish()
        {
            if(finished)return; finished=true; Application.logMessageReceived-=Log;
            report.realScoreAfter=Hash(report.realScorePath); Check("real personal score bytes unchanged",report.realScoreBefore==report.realScoreAfter);
            report.finishedUtc=DateTime.UtcNow.ToString("o"); report.wallSeconds=Time.realtimeSinceStartup-began;
            report.passed=report.completed&&report.passedChecks==report.checks&&report.errors.Count==0;
            File.WriteAllText(Path.Combine(w.EvidencePath,"interaction-audit.json"),JsonUtility.ToJson(report,true));
            Debug.Log("INTERACTION_AUDIT_DONE "+report.passedChecks+"/"+report.checks+" errors="+report.errors.Count);
            Application.Quit(report.passed?0:1);
        }
    }
}
