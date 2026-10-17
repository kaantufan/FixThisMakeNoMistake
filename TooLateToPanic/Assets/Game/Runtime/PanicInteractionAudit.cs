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
            public string evidenceType = "Native model + explicit world fixtures + IMGUI semantic pointer/key dispatch, distinct from physical OS input";
            public string limitations = "World setup Warps and model time fixtures are explicit; no exhaustive human/OS/campaign/fun acceptance. Events are injected inside OnGUI. Pointer coordinates use a declared logical-position fixture because native GUI.matrix reprojects synthetic mouse coordinates from the platform repaint pointer. Production event-type, hit testing, modal priority and callbacks run; hardware polling and native raw mouse event conversion are not accepted.";
            public bool completed, passed; public int checks, passedChecks; public float wallSeconds;
            public List<Row> rows = new List<Row>(); public List<string> errors = new List<string>(); public List<string> screenshots = new List<string>();
        }
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static readonly MethodInfo HudGui = typeof(PanicHomeHud).GetMethod("OnGUI", Private);
        readonly Report report = new Report();
        PanicHomeWorld w; PanicHomeHud hud; float began; bool finished; string layer = "native-imgui";
        Event queued; Action queuedSetup; bool pending, quitObserved; int eventFrame = -1; Exception eventError;
        static PanicInteractionAudit activeGuiFixture;
        Vector2 queuedLogicalPointer;
        public static Vector2 GuiPointer(Vector2 eventPosition) { return activeGuiFixture ? activeGuiFixture.queuedLogicalPointer : eventPosition; }

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
            try
            {
                queuedSetup?.Invoke(); queuedSetup=null; GUI.matrix = Matrix4x4.identity; Event.current = queued;
                activeGuiFixture=this; HudGui.Invoke(hud, null);
            }
            catch (Exception ex) { eventError = ex; }
            finally { activeGuiFixture=null; Event.current = previous; GUI.matrix = matrix; }
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
            w.FullFlowScriptedInput=true; // This opt-in audit owns semantic input, never background hardware polling.
            yield return Wait(.3f); yield return Shot("00-title");
            // This probe reproduces event-coordinate hit testing without OS cursor repositioning.
            yield return Click(278, 710);
            Check("mouse event coordinates open SERBEST OYUN", w.Model.Phase == PanicPhase.Preparing && !w.Model.IsJuryDemo,
                "MouseDown event=(278,710), hardware pointer=" + Input.mousePosition + "; phase=" + w.Model.Phase);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--audit-probe") >= 0) { report.completed = true; yield break; }
            if (SelectedSection("ui")) yield return UiSuite();
            if (SelectedSection("ui")) yield return AdditionalUi();
            layer = "pure-model-fixture";
            if (SelectedSection("model")) InvokeModelAudit();
            layer = "native-world-fixture";
            // UI cases deliberately exercise options; independent world fixtures must
            // start with all options off, even if a prior UI assertion failed.
            w.ReturnToMenu(); foreach(PanicDeveloperOption option in Enum.GetValues(typeof(PanicDeveloperOption)))w.Model.SetDeveloperOption(option,false);
            w.StartGame(false); yield return Wait(.05f);
            var helper = typeof(PanicHomeWorld).Assembly.GetType("TooLateToPanic.PanicInteractionWorldAudit");
            if (SelectedSection("world"))
            {
                if (helper == null) Check("world audit helper installed", false);
                else yield return (IEnumerator)helper.GetMethod("Run").Invoke(null, new object[] { w, hud, (Action<string,bool,string>)Check });
            }
            var frameHelper = typeof(PanicHomeWorld).Assembly.GetType("TooLateToPanic.PanicInteractionFrameAudit");
            if (SelectedSection("frame"))
            {
                if (frameHelper == null) Check("frame audit helper installed", false);
                else yield return (IEnumerator)frameHelper.GetMethod("Run").Invoke(null, new object[] { w, hud, (Action<string,bool,string>)Check });
            }
            var targetHelper = typeof(PanicHomeWorld).Assembly.GetType("TooLateToPanic.PanicInteractionTargetAudit");
            if (SelectedSection("targets"))
            {
                if (targetHelper == null) Check("target audit helper installed", false);
                else yield return (IEnumerator)targetHelper.GetMethod("Run").Invoke(null, new object[] { w, hud, (Action<string,bool,string>)Check });
            }
            layer = "native-flow-fixture";
            if (SelectedSection("flow")) yield return FlowSuite();
            w.ReturnToMenu(); yield return Wait(.05f);
            Application.wantsToQuit += ObserveQuit;
            yield return Click(278,774); yield return Wait(.08f);
            Check("title EXIT reaches native Application.Quit",quitObserved,"Native wantsToQuit intercept keeps this isolated audit alive until report flush");
            Application.wantsToQuit -= ObserveQuit;
            report.completed = true;
        }
        bool ObserveQuit() { quitObserved=true; return false; }
        bool SelectedSection(string section)
        {
            var args=Environment.GetCommandLineArgs(); int at=Array.IndexOf(args,"--audit-section");
            return at<0 || at+1>=args.Length || args[at+1]==section;
        }
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
            w.StartGame(false); yield return Wait(.05f);
            w.Expedition.Inventory[0].Id="water"; w.Expedition.Inventory[0].Count=1; w.InventorySelect(0);
            Vector2 packPoint=new Vector2(230,784)*hud.CanvasScale+hud.CanvasOffset;
            yield return Send(new Event{type=EventType.MouseDown,button=0,mousePosition=packPoint},()=>w.DispatchWorldInput(new[]{KeyCode.Tab},false,false));
            Check("Tab and same-frame IMGUI click cannot pack through modal",w.InventoryOpen&&w.Model.PackedWater==0&&w.Expedition.Count("water")==1,"Legal one-water stock fixture; dispatcher plus actual OnGUI in same frame");
            yield return Click(230,784);
            Check("next-frame inventory click packs exactly once",w.Model.PackedWater==1&&w.Expedition.Count("water")==0);
            w.ToggleInventory(); w.ReturnToMenu(); yield return Wait(.03f);
            w.StartGame(false); w.InventorySelect(11); w.ToggleAnchorSurface(); w.SetFocus("fridge");
            w.ReturnToMenu(); yield return Wait(.03f); w.StartGame(false); yield return Wait(.03f);
            Check("new run resets inventory/anchor/loot selections",w.InventoryIndex==0&&w.AnchorMode=="wall"&&w.LootIndex==0,"Selections changed before ReturnToMenu/StartGame; costs and game rules unchanged");
            w.ReturnToMenu(); yield return Wait(.03f);
        }
        bool DeveloperFlag(int option) { return option==0?w.Model.InfiniteBudget:option==1?w.Model.FreeMaterials:option==2?w.Model.FreezePreparationTime:w.Model.DeveloperNoLoss; }
        IEnumerator AdditionalUi()
        {
            w.InputLocked=false; w.StartGame(false); yield return Wait(.06f);
            w.Expedition.Inventory[0].Id="bandage"; w.Expedition.Inventory[0].Count=2;
            w.Expedition.Inventory[1].Id="antiseptic"; w.Expedition.Inventory[1].Count=1;
            w.ToggleInventory(); yield return Wait(.03f); yield return Click(436,784);
            Check("inventory mouse craft consumes exact recipe",w.Expedition.Count("firstaid")==1&&w.Expedition.Count("bandage")==0&&w.Expedition.Count("antiseptic")==0,"Legal2bandage1antiseptic fixture");
            int kit=w.Expedition.Inventory.FindIndex(s=>s.Id=="firstaid"&&s.Count>0); if(kit>=0)w.InventorySelect(kit);
            yield return Click(230,784); int support=w.Model.SupportCount;
            yield return Click(230,784); yield return Click(900,330);
            Check("inventory empty repeat and read-only bag cannot duplicate",support==1&&w.Model.SupportCount==1&&w.Expedition.Count("firstaid")==0);
            yield return Click(642,784); w.StartGame(false); yield return Wait(.03f);
            w.Warp(new Vector3(-4,-3.12f,3)); yield return Wait(.15f); w.SetFocus("basement_pantry"); w.HandleKey(KeyCode.E); yield return Wait(.05f);
            var source=w.NearestSource; Check("loot mouse fixture reachable",source!=null&&source.Open&&w.LootVisible,"Pantry setup Warp, not a traversal claim");
            if(w.LootVisible)
            {
                int drawn=0, visible=source.Contents.Count(s=>s.Count>0&&!string.IsNullOrEmpty(s.Id));
                float width=Mathf.Max(570,visible*108+202), x=(1600-width)*.5f;
                for(int i=0;i<source.Contents.Count;i++)if(source.Contents[i].Count>0)
                {
                    yield return Click(x+16+drawn*108+50,695);
                    Check("loot mouse cell "+drawn+" selects exact source index",w.LootIndex==i,"source index="+i); drawn++;
                }
                string item=source.Contents[w.LootIndex].Id;int count=source.Contents[w.LootIndex].Count,before=w.Expedition.Count(item);
                yield return Click(x+16+visible*108+6+76,670);
                Check("loot mouse take commits one selected stack",w.Expedition.Count(item)==before+count&&w.GatherItemId==item,"id="+item+" count="+count);
                visible=source.Contents.Count(s=>s.Count>0&&!string.IsNullOrEmpty(s.Id));width=Mathf.Max(570,visible*108+202);x=(1600-width)*.5f;
                yield return Click(x+16+visible*108+6+76,718);
                Check("loot mouse close preserves remaining stock",!source.Open&&!w.LootVisible);
                w.HandleKey(KeyCode.E); for(int i=0;i<10&&source.Contents.Any(s=>s.Count>0);i++)w.TakeLoot();
                yield return Wait(.04f); yield return Click(1045,691);
                Check("empty loot mouse close uses empty-state control",!source.Open&&source.Contents.All(s=>s.Count==0));
            }
            w.StartGame(false); yield return Wait(.04f);
            w.Expedition.Inventory[0].Id="sandbag";w.Expedition.Inventory[0].Count=2;
            w.Expedition.Inventory[1].Id="plank";w.Expedition.Inventory[1].Count=1;
            w.Warp(new Vector3(-6,.08f,3.5f));yield return Wait(.15f);w.SetFocus("bookshelf");
            w.Expedition.Inventory[0].Id="bracket";w.Expedition.Inventory[0].Count=1;w.Expedition.Inventory[1].Id="screws";w.Expedition.Inventory[1].Count=2;
            w.BeginWork("anchorwall","bookshelf"); yield return Wait(.04f);
            int budget=w.Model.Budget;yield return Click(1314,802);yield return Click(1314,684);
            Check("work read-only progress/instruction areas are mouse-inert",w.WorkActive&&w.Expedition.Work.CompletedSteps==0&&w.Model.Budget==budget,"Keycaps are keyboard instructions, not buttons");
            w.CancelWork();yield return Wait(.04f);
            for(int i=0;i<40;i++)w.HandleKey(KeyCode.PageUp);
            Check("PageUp semantic zoom clamps at4",Mathf.Abs((float)GetField(w,"cameraSize")-4)<.001f);
            for(int i=0;i<40;i++)w.HandleKey(KeyCode.PageDown);
            Check("PageDown semantic zoom clamps at10",Mathf.Abs((float)GetField(w,"cameraSize")-10)<.001f);SetField(w,"cameraSize",5.7f);
            foreach(var phase in new[]{PanicPhase.GameOver,PanicPhase.Success})
            {
                w.StartGame(false);yield return Wait(.03f);w.Model.Phase=phase;SetField(w,"previousPhase",phase);yield return Wait(.03f);
                yield return Click(545,679);Check("end-card restart callback "+phase,w.Model.Phase==PanicPhase.Preparing&&w.Model.Budget==1000,"Explicit result phase fixture; no earned success claim");
                w.Model.Phase=phase;SetField(w,"previousPhase",phase);yield return Wait(.03f);
                yield return Click(1025,679);Check("end-card title callback "+phase,w.Model.Phase==PanicPhase.Title,"Explicit result phase fixture");
            }
            w.StartJuryDemo();yield return Wait(.04f);w.JuryDemo.Tick(300f,w.PlayerPosition,true,true);yield return Wait(.04f);
            yield return Click(1042,698);Check("short result title callback",w.Model.Phase==PanicPhase.Title&&!w.JuryActive,"300-second injected deadline fixture; not elapsed human play");
            hud.OpenSimulationSelector();yield return Wait(.03f);yield return Key(KeyCode.LeftArrow);yield return Key(KeyCode.KeypadEnter);
            Check("selector keyboard wraps left and keypad Enter selects storm",w.Model.IsSimulation&&w.Model.Disaster==DisasterType.Storm);
            w.ReturnToMenu();yield return Wait(.03f);hud.OpenSimulationSelector();yield return Key(KeyCode.Backspace);
            Check("selector Backspace closes",!hud.SimulationSelectorOpen&&w.Model.Phase==PanicPhase.Title);
        }
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
                Check("disaster Enter resolves once "+type,w.Model.SimulationComplete&&(w.Model.Phase==PanicPhase.Inspection||w.Model.Phase==PanicPhase.GameOver),"Full presentation skip resolves the ordinary model outcome; stale Aftermath-only oracle removed.");
                yield return Click(1390,64); yield return Wait(.06f);
                if(w.LossAnnounced) { yield return Wait(1.8f); if(w.CinematicActive) { yield return Key(KeyCode.Return); yield return Wait(.06f); } }
                Check("outcome resolves once "+type,w.Model.SimulationComplete&&w.Model.LastReward==0&&w.Model.Score==0,"phase="+w.Model.Phase+" damage="+w.Model.TotalDamage+" reason="+w.Model.FailureReason);
                if(w.Model.Phase==PanicPhase.GameOver)
                {
                    if(type==DisasterType.Flood)
                    {
                        yield return Click(782,679);yield return Wait(.06f);Check("simulation end-card retry callback",w.Model.IsSimulation&&w.Model.Phase==PanicPhase.Preparing&&!w.Model.SimulationComplete);
                        w.ReturnToMenu();yield return Wait(.04f);continue;
                    }
                    if(type==DisasterType.Fire)
                    {
                        yield return Click(1103,679);yield return Wait(.06f);Check("simulation end-card main-menu callback",w.Model.Phase==PanicPhase.Title&&!w.Model.IsSimulation);
                        continue;
                    }
                    yield return Click(468,679); yield return Wait(.08f);
                    Check("HASARI INCELE restores real controller "+type,w.Model.Phase==PanicPhase.Inspection&&w.PlayerControllerEnabled);
                }
                if(w.Model.Phase==PanicPhase.Inspection)
                {
                    if(type==DisasterType.Storm)
                    {yield return Click(1426,750);yield return Wait(.06f);Check("simulation inspection main-menu callback",w.Model.Phase==PanicPhase.Title);}
                    else
                    {yield return Click(1170,750);yield return Wait(.06f);Check("inspection retry resets simulation "+type,w.Model.Phase==PanicPhase.Preparing&&w.Model.IsSimulation&&!w.Model.SimulationComplete&&w.Model.PackedWater==1);}
                }
                w.ReturnToMenu(); yield return Wait(.05f);
            }
            // Short segment skip remains honest about missing preparation.
            w.StartJuryDemo(); yield return Wait(.06f);
            for(int i=0;i<8&&w.JuryDemo.Stage!=PanicJuryStage.Complete;i++)
            {
                var before=w.JuryDemo.Stage;
                if(w.JurySnapshot.CanSkipSegment) { yield return Click(235,390); }
                else if(w.JurySnapshot.CanAdvance||before==PanicJuryStage.Earthquake) { if(before==PanicJuryStage.EarthquakeResult)yield return Click(235,390);else yield return Key(KeyCode.Return); }
                else { w.JuryDemo.Tick(80f,w.PlayerPosition,true,false); }
                yield return Wait(.06f);
            }
            Check("short skip reaches truthful result",w.JuryDemo.Stage==PanicJuryStage.Result||w.JuryDemo.Stage==PanicJuryStage.Complete,"stage="+w.JuryDemo.Stage);
            yield return Shot("short-skipped-result");
            yield return Click(546,698); yield return Wait(.06f);
            Check("short result retry resets goals/stock",w.JuryDemo!=null&&w.JuryDemo.Stage==PanicJuryStage.Onboarding&&w.Model.PackedWater==0&&w.Expedition.Count("bracket")==1);
            w.ReturnToMenu(); yield return Wait(.03f);
        }
        IEnumerator Send(Event e, Action setup = null)
        {
            if(pending)throw new InvalidOperationException("overlapping queued event");
            queuedLogicalPointer=(e.mousePosition-hud.CanvasOffset)/hud.CanvasScale;
            queued=e; queuedSetup=setup; pending=true; eventError=null; float end=Time.realtimeSinceStartup+3f;
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
