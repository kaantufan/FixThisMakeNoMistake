using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Opt-in, isolated V5.7 UI callbacks/layout/framebuffer checks. No OS input or human acceptance.</summary>
    public sealed class PanicUiDesignSmokeV57 : MonoBehaviour
    {
        const string Flag = "--ui-v57-smoke";
        const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id, status, detail, layer; public int frame; }
        [Serializable] public sealed class Fixture { public string kind, detail; public int frame; }
        [Serializable] public sealed class Shot
        {
            public string path, sha256, context, surface;
            public int width, height, frame, samples, nonDarkSamples, luminanceRange;
        }
        [Serializable] public sealed class Receipt
        {
            public string utc, finishedUtc, dllSha256, scorePath, realScoreBefore, realScoreAfter, error;
            public string globalPreferencesBefore, globalPreferencesAfter, preferencePrefix, windowHandle;
            public string evidence = "Native production GUI callbacks, semantic APIs, actual recipes/stock, current registered logical rectangles, wall clocks and ScreenCapture framebuffer PNGs. Declared in-memory score display, actor placement, preparation clock, immediately restored packed-count fixtures, view-only Notebook Rooms screenshots, a typed model shelter gate, and a committed actual wave ledger. No OS keyboard/mouse, human readability/fun, listening, physical shelter traversal or campaign acceptance.";
            public string renderLimit = "Pixel screening rejects missing, black or uniform graphical surfaces as INVALID_RENDER_SURFACE. Nonblack PNGs and rectangle arithmetic do not certify visual quality, text fit or human readability. Sole live owner must inspect every supplied capture.";
            public bool complete, passed, invalidRenderSurface, osInput, humanAcceptance, listening, preferencesRestored;
            public float seconds;
            public int productionStarts, guiCallbacks, repaintProbes, fixtureCount;
            public int actualLedgerCaptures, productionModelShelterCalls, productionSkipCalls;
            public int productionRequirementsPreflights, productionInventoryOpens, packedCounterFixtures;
            public List<Row> checks = new List<Row>();
            public List<Fixture> fixtures = new List<Fixture>();
            public List<Shot> captures = new List<Shot>();
            public List<string> errors = new List<string>();
        }
        readonly Receipt report = new Receipt();
        static readonly List<string> BootErrors = new List<string>();
        PanicHomeWorld w; PanicHomeHud hud; PanicInteractionAudit pointer;
        float began, oldMaster, oldMusic, oldSfx; string realScore;
        bool finished, preferencesHeld, oldMuted, oldMusicEnabled, oldMotion, pending, probe;
        EventType queuedType; KeyCode queuedKey; Vector2 queuedPoint;
        List<HighscoreEntry> scoreDisplayBefore;
        static readonly MethodInfo HudGui = typeof(PanicHomeHud).GetMethod("OnGUI", Instance);
        static readonly FieldInfo ActivePointer = typeof(PanicInteractionAudit).GetField("activeGuiFixture", Static);
        static readonly FieldInfo LogicalPointer = typeof(PanicInteractionAudit).GetField("queuedLogicalPointer", Instance);
        static readonly Rect[] TitleActions = {
            new Rect(112,488,760,66), new Rect(112,565,760,66), new Rect(112,642,374,58),
            new Rect(498,642,374,58), new Rect(112,711,320,58)
        };
        static readonly Rect[] PauseActions = {
            new Rect(620,181,830,68), new Rect(620,267,405,64), new Rect(1041,267,409,64),
            new Rect(620,349,830,64), new Rect(620,523,405,64), new Rect(1041,523,409,64), new Rect(620,605,830,64)
        };
        static readonly Rect[] AudioRows = {
            new Rect(288,280,1004,73), new Rect(288,371,1004,73), new Rect(288,462,1004,73),
            new Rect(288,567,494,60), new Rect(800,567,492,60), new Rect(288,694,348,58)
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void WatchBoot()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            BootErrors.Clear(); Application.logMessageReceived += BootLog;
        }
        static void BootLog(string message, string stack, LogType type)
        { if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)BootErrors.Add(message+"\n"+stack); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            var owner = FindFirstObjectByType<PanicHomeWorld>();
            if (owner && !owner.GetComponent<PanicUiDesignSmokeV57>()) owner.gameObject.AddComponent<PanicUiDesignSmokeV57>();
        }
        void Start()
        {
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); began = Time.realtimeSinceStartup;
            realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.utc = DateTime.UtcNow.ToString("o"); report.dllSha256 = Hash(typeof(PanicModel).Assembly.Location);
            report.scorePath = PanicModel.ScorePathOverride; report.realScoreBefore = Hash(realScore);
            report.globalPreferencesBefore = GlobalPreferenceFingerprint();
            try { report.windowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle.ToInt64().ToString(CultureInfo.InvariantCulture); }
            catch (Exception ex) { report.windowHandle = "unavailable:" + ex.GetType().Name; }
            report.errors.AddRange(BootErrors); Application.logMessageReceived -= BootLog;
            Application.logMessageReceived += Log; Application.runInBackground = true;
            StartCoroutine(Guard(Run()));
        }
        void Update() { if (!finished && Time.realtimeSinceStartup - began > 180f) { if(report.guiCallbacks==0)report.invalidRenderSurface=true; report.error = "180s bounded V5.7 UI smoke cap"; Finish(); } }
        void OnDisable() { Restore(); }
        void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Check(string id, bool pass, string detail = "", string layer = "NATIVE_API_FRAME")
        {
            report.checks.Add(new Row { id = id, status = pass ? "PASS" : "FAIL", detail = detail, layer = layer, frame = Time.frameCount });
            Debug.Log("UI_V57_CHECK " + (pass ? "PASS " : "FAIL ") + id + " " + detail);
        }
        void Declare(string kind, string detail)
        { report.fixtures.Add(new Fixture { kind = kind, detail = detail, frame = Time.frameCount }); report.fixtureCount++; }
        static object Member(object owner, string name)
        {
            var property = owner.GetType().GetProperty(name, Instance); if (property != null) return property.GetValue(owner);
            var field = owner.GetType().GetField(name, Instance); if (field != null) return field.GetValue(owner);
            throw new MissingMemberException(owner.GetType().FullName, name);
        }
        static T Read<T>(object owner, string name) { return (T)Member(owner, name); }
        static void Set(object owner, string name, object value) { owner.GetType().GetField(name, Instance).SetValue(owner, value); }
        static object Call(object owner, string name, params object[] args)
        { return owner.GetType().GetMethods(Instance).First(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(owner, args); }
        IEnumerator Guard(IEnumerator body)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(body);
            while (stack.Count > 0 && !finished)
            {
                bool more; object current = null;
                try { more = stack.Peek().MoveNext(); if (more) current = stack.Peek().Current; }
                catch (Exception ex) { report.error = ex.ToString(); break; }
                if (!more) { stack.Pop(); continue; }
                if (current is IEnumerator nested) stack.Push(nested); else yield return current;
            }
            if (!finished) { report.complete = string.IsNullOrEmpty(report.error); Finish(); }
        }
        IEnumerator Wait(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
        bool Isolated()
        {
            if (!w || string.IsNullOrWhiteSpace(w.EvidencePath) || string.IsNullOrWhiteSpace(PanicModel.ScorePathOverride)) return false;
            string path = Path.GetFullPath(w.EvidencePath), leaf = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            Guid id; return leaf.Length >= 36 && Guid.TryParseExact(leaf.Substring(leaf.Length - 36), "D", out id) &&
                string.Equals(Path.GetFullPath(PanicModel.ScorePathOverride), Path.Combine(path, "isolated-home-scores.json"), StringComparison.OrdinalIgnoreCase);
        }
        string Stamp()
        {
            var m = w.Model;
            return PanicNotebookVerification.GameplayFingerprint(w) + "|" + m.Phase + "|" + m.TimeRemaining.ToString("R", CultureInfo.InvariantCulture) +
                "|" + m.PhaseTime.ToString("R", CultureInfo.InvariantCulture) + "|" + m.TotalDamage + "|" + m.CompletedWaves + "|" +
                m.LastReward + "|" + m.LastWaveScore + "|" + m.SecondaryRollCountV5 + "|" + m.SecondaryEventSummaryV5 + "|" + m.ReachedShelterV5;
        }
        // Explicit GUI event fixture, following the repository's existing audit adapter. Does not send OS events.
        void OnGUI()
        {
            if (!pending || finished || Event.current.type != EventType.Repaint) return;
            pending = false; Event previous = new Event(Event.current); Matrix4x4 matrix = GUI.matrix;
            Color color = GUI.color, content = GUI.contentColor; bool enabledBefore = GUI.enabled;
            try
            {
                LogicalPointer.SetValue(pointer, queuedPoint); ActivePointer.SetValue(null, pointer);
                Event.current = new Event { type = queuedType, keyCode = queuedKey, mousePosition = queuedPoint, button = 0 };
                string state = probe ? Stamp() : null, rng = probe ? JsonUtility.ToJson(UnityEngine.Random.state) : null;
                HudGui.Invoke(hud, null); report.guiCallbacks++;
                if (probe)
                {
                    report.repaintProbes++;
                    Check("render.readonly." + report.repaintProbes, state == Stamp() && rng == JsonUtility.ToJson(UnityEngine.Random.state),
                        "Synchronous production OnGUI Repaint before/after: clock/model/inventory/items/RNG unchanged. Outside-frame effects are excluded.", "NATIVE_SYNCHRONOUS_REPAINT");
                }
            }
            catch (Exception ex) { report.error = ex.ToString(); }
            finally { ActivePointer.SetValue(null, null); Event.current = previous; GUI.matrix = matrix; GUI.color = color; GUI.contentColor = content; GUI.enabled = enabledBefore; }
        }
        IEnumerator Gui(EventType type, KeyCode key = KeyCode.None, Vector2? point = null, bool readonlyProbe = false)
        {
            queuedType = type; queuedKey = key; queuedPoint = point ?? new Vector2(-100, -100); probe = readonlyProbe; pending = true;
            float until = Time.realtimeSinceStartup + 2f; while (pending && Time.realtimeSinceStartup < until) yield return null;
            if (pending) { pending = false; report.invalidRenderSurface=true; throw new InvalidOperationException("Actual GUI Repaint callback was unavailable; INVALID_RENDER_SURFACE."); }
            if (!string.IsNullOrEmpty(report.error)) throw new InvalidOperationException(report.error);
            yield return null;
        }
        IEnumerator Resolution(int width, int height)
        {
            Screen.SetResolution(width, height, FullScreenMode.Windowed); float until = Time.realtimeSinceStartup + 2f;
            while ((Screen.width != width || Screen.height != height) && Time.realtimeSinceStartup < until) yield return null;
            yield return Wait(.15f); Check("framebuffer." + width, Screen.width == width && Screen.height == height, Screen.width + "x" + Screen.height, "NATIVE_GRAPHICAL_SURFACE");
        }
        bool Inside(Rect r) { return r.width > 0 && r.height > 0 && r.xMin >= 0 && r.yMin >= 0 && r.xMax <= 1600.01f && r.yMax <= 900.01f; }
        static bool SameRect(Rect a, Rect b) { return Mathf.Abs(a.x-b.x)<.1f && Mathf.Abs(a.y-b.y)<.1f && Mathf.Abs(a.width-b.width)<.1f && Mathf.Abs(a.height-b.height)<.1f; }
        void Regions(string name, Rect[] expected)
        {
            var registered = Read<List<Rect>>(hud, "_uiRects"); bool disjoint = true;
            for (int i=0;i<expected.Length;i++) for (int j=i+1;j<expected.Length;j++) disjoint &= !expected[i].Overlaps(expected[j]);
            Check("layout."+name+".within", expected.All(Inside), string.Join("; ", expected.Select(r=>r.ToString())), "NATIVE_GUI_LAYOUT");
            Check("layout."+name+".disjoint", disjoint, "Action/row regions only; container panels and their child text legitimately overlap.", "NATIVE_GUI_LAYOUT");
            Check("layout."+name+".actually-registered", expected.All(r=>registered.Any(actual=>SameRect(actual,r))), "Current production OnGUI registered these exact regions.", "NATIVE_GUI_LAYOUT");
            Check("layout."+name+".physical", expected.All(r=>hud.CanvasOffset.x+r.xMin*hud.CanvasScale>=-.1f && hud.CanvasOffset.y+r.yMin*hud.CanvasScale>=-.1f && hud.CanvasOffset.x+r.xMax*hud.CanvasScale<=Screen.width+.5f && hud.CanvasOffset.y+r.yMax*hud.CanvasScale<=Screen.height+.5f), "Actual canvas transform at "+Screen.width+"x"+Screen.height, "NATIVE_GUI_LAYOUT");
        }
        IEnumerator Hold(string id, float seconds=.24f)
        { string before=Stamp(); yield return Wait(seconds); Check("clock.hold."+id, before==Stamp(), "Real wall wait="+seconds+"s; no fixture Tick, clock, budget, stock, damage or RNG changes."); }
        IEnumerator Capture(string name, string context)
        {
            yield return new WaitForEndOfFrame(); string path=Path.Combine(w.EvidencePath,name+".png");
            var shot=new Shot { path=path, context=context, frame=Time.frameCount, width=Screen.width, height=Screen.height };
            ScreenCapture.CaptureScreenshot(path); float until=Time.realtimeSinceStartup+3f; byte[] bytes=null;
            while (!CompletePng(path,out bytes) && Time.realtimeSinceStartup<until) yield return null;
            bool valid=false; Texture2D texture=null;
            try
            {
                if(bytes!=null)
                {
                    texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
                    if(ImageConversion.LoadImage(texture,bytes,false))
                    {
                        int expectedWidth=shot.width, expectedHeight=shot.height; shot.width=texture.width;shot.height=texture.height;
                        Color32[] pixels=texture.GetPixels32(); int min=255,max=0,stride=Mathf.Max(1,pixels.Length/16384);
                        for(int i=0;i<pixels.Length;i+=stride){var p=pixels[i];int lum=(p.r*54+p.g*183+p.b*19)/256;min=Mathf.Min(min,lum);max=Mathf.Max(max,lum);if(lum>18)shot.nonDarkSamples++;shot.samples++;}
                        shot.luminanceRange=max-min;valid=shot.width==expectedWidth&&shot.height==expectedHeight&&shot.samples>0&&shot.nonDarkSamples>shot.samples/50&&shot.luminanceRange>18;
                    }
                }
            }
            finally { if(texture)Destroy(texture); }
            shot.sha256=Hash(path);shot.surface=valid?"NATIVE_NONBLACK_FRAMEBUFFER_PENDING_OWNER_INSPECTION":"INVALID_RENDER_SURFACE";report.captures.Add(shot);
            if(!valid)report.invalidRenderSurface=true;
            report.checks.Add(new Row { id="capture."+name,status=valid?"PASS":"INVALID_RENDER_SURFACE",detail=shot.width+"x"+shot.height+"; nonDark="+shot.nonDarkSamples+"/"+shot.samples+"; range="+shot.luminanceRange+"; "+context,layer="NATIVE_FRAMEBUFFER_FILE_PIXEL_SCREENING",frame=Time.frameCount });
        }
        IEnumerator Title(int width)
        {
            scoreDisplayBefore=w.Model.Highscores; w.Model.Highscores=new List<HighscoreEntry>();
            Declare("score-display-only","Replace isolated in-memory Highscores list with empty list; no score persistence API or external score data.");
            Set(hud,"_menuChoice",0);yield return Wait(.06f);yield return Gui(EventType.Repaint,readonlyProbe:true);
            Regions("title."+width,TitleActions);yield return Capture("title-empty-"+width,"Empty isolated in-memory local-score display.");
            var entries=new List<HighscoreEntry>();for(int i=0;i<6;i++)entries.Add(new HighscoreEntry{waves=50-i*8,score=9999999999L-i*111111111L,date="2026-10-18"});
            w.Model.Highscores=entries;Declare("score-display-only","Six synthetic long-score rows in isolated model.Highscores; original list restored before gameplay; never written to disk.");
            yield return Gui(EventType.Repaint,readonlyProbe:true);
            Check("title.full-exact-score-fit."+width,Read<bool>(hud,"MenuScoresFitV57")&&Read<int>(hud,"MenuScoreRowCountV57")==6&&Time.frameCount-Read<int>(hud,"MenuScoreMeasurementFrameV57")<=1,Read<string>(hud,"MenuScoreFitDetailV57"),"NATIVE_ACTUAL_USED_STYLE_MEASUREMENT");
            for(int choice=0;choice<5;choice++)
            {
                Set(hud,"_menuChoice",choice);Declare("display-selection","title _menuChoice="+choice+"; selects highlight only, no activation.");
                yield return Wait(.05f);yield return Capture("title-full-choice"+choice+"-"+width,"Six declared display rows; title selection="+choice+". Inspect long scores, all fields and control focus.");
            }
            Set(hud,"_menuChoice",0);
            foreach(var step in new[]{new KeyValuePair<KeyCode,int>(KeyCode.DownArrow,1),new KeyValuePair<KeyCode,int>(KeyCode.DownArrow,2),new KeyValuePair<KeyCode,int>(KeyCode.RightArrow,3),new KeyValuePair<KeyCode,int>(KeyCode.LeftArrow,2),new KeyValuePair<KeyCode,int>(KeyCode.DownArrow,4),new KeyValuePair<KeyCode,int>(KeyCode.DownArrow,0)})
            {yield return Gui(EventType.KeyDown,step.Key);Check("title.callback-nav."+width+"."+report.guiCallbacks,hud.TitleChoice==step.Value,"Injected production IMGUI key callback; no OS input.","NATIVE_GUI_EVENT_CALLBACK");}
            w.Model.Highscores=scoreDisplayBefore;scoreDisplayBefore=null;
        }
        IEnumerator Begin()
        {
            hud.SetPaused(false);w.ReturnToMenu();w.StartGame(false);report.productionStarts++;w.FullFlowScriptedInput=true;yield return null;
            if(hud.FreeplayOnboardingOpenV414)hud.ConfirmFreeplayOnboardingV414();yield return null;w.SkipIntroStoryV414();yield return Wait(.16f);
            Check("world.normal-start."+report.productionStarts,w.Model.Phase==PanicPhase.Preparing&&!w.CinematicActive&&!w.Model.CheatedRun);
        }
        IEnumerator NotebookRooms(int width)
        {
            var book=w.Notebook;
            bool opened=book&&book.RequestOpen();
            Check("notebook.production-open."+width,opened&&book.IsVisible&&book.BlocksInput,"Actual PanicNotebookOverlay.RequestOpen owns its own modal gate; no OS input.","NATIVE_PRODUCTION_MODAL_API");
            if(!book||!opened)yield break;
            yield return Wait(.40f);
            if(book.IntroVisibleV55)
            {
                bool dismissed=book.ProcessAssistantIntroKeyV55(KeyCode.Return);
                Check("notebook.production-intro-dismiss."+width,dismissed&&!book.IntroVisibleV55,"Production semantic Enter handler dismisses first-open intro; no OS input.","NATIVE_PRODUCTION_MODAL_API");
            }
            else Check("notebook.production-intro-dismiss."+width,book.IntroSeenV55,"Intro was dismissed on the first resolution; this resolution starts directly on the normal notebook page.","NATIVE_PRODUCTION_MODAL_API");
            Declare("notebook-room-plan-view","Select Rooms page and floor0/1 through actual notebook presentation APIs to capture the settled real scene sketch and Kiler/Tuvalet labels; view-only UI state.");
            book.RequestPage(PanicNotebookPage.Rooms);book.SelectFloor(0);
            var plan=book.PlanSnapshotV55;
            Check("notebook.plan-snapshot."+width,plan!=null,"Production Rooms plan snapshot is available before selecting a capture floor.","NATIVE_NOTEBOOK_PLAN_API");
            if(plan==null){book.RequestClose();yield return Wait(.40f);yield break;}
            string planLabels=string.Join("|",plan.Shapes.Where(shape=>shape.Kind==PanicNotebookPlanKindV55.Door||shape.Kind==PanicNotebookPlanKindV55.Floor).Select(shape=>(shape.Label??"").Replace(" iç kapısı","").Replace(" kapısı","")));
            int planFloor=0;
            if(!planLabels.Contains("Kiler")||!planLabels.Contains("Tuvalet"))
            {book.SelectFloor(1);planFloor=1;plan=book.PlanSnapshotV55;planLabels=plan==null?"":string.Join("|",plan.Shapes.Where(shape=>shape.Kind==PanicNotebookPlanKindV55.Door||shape.Kind==PanicNotebookPlanKindV55.Floor).Select(shape=>(shape.Label??"").Replace(" iç kapısı","").Replace(" kapısı","")));}
            Check("notebook.plan-floor-selected."+width,planFloor==0||planFloor==1,"Choose a populated floor0/1 if the Kiler/Tuvalet labels are present.","NATIVE_NOTEBOOK_PLAN_API");
            float until=Time.realtimeSinceStartup+2f;
            while((book.Animation.Turning||book.Animation.Openness<1f||book.Animation.CurrentPage!=PanicNotebookPage.Rooms)&&Time.realtimeSinceStartup<until)yield return null;
            yield return Wait(.05f);yield return new WaitForEndOfFrame();
            object[] roomLabels=((IEnumerable)Member(book,"roomLabelsV56")).Cast<object>().ToArray();
            string labelText=string.Join("|",roomLabels.Select(label=>Read<string>(label,"Text")));
            Rect[] labelRects=Read<Rect[]>(book,"MapRoomLabelRectsV56");
            Check("notebook.settled-room-sketch."+width,book.Animation.TargetOpen&&book.Animation.Openness==1f&&!book.Animation.Turning&&book.Animation.CurrentPage==PanicNotebookPage.Rooms&&book.PlanSnapshotV55!=null&&book.PlanSnapshotV55.Floor==planFloor&&Read<int>(book,"MapRoomLabelDrawFrameV56")==Time.frameCount&&labelRects.Length>0,"Settled production Rooms floor"+planFloor+" sketch Repaint frame="+Read<int>(book,"MapRoomLabelDrawFrameV56")+"; labels="+labelText,"NATIVE_ACTUAL_REPAINT_TELEMETRY");
            Check("notebook.kiler-tuvalet-labels."+width,labelText.Contains("Kiler")&&labelText.Contains("Tuvalet"),"Actual floor"+planFloor+" plan labels: "+labelText,"NATIVE_ACTUAL_REPAINT_TELEMETRY");
            yield return Gui(EventType.Repaint,readonlyProbe:true);
            yield return Capture("notebook-room-plan-"+width,"Settled actual Notebook Rooms page at floor"+planFloor+" with production scene sketch and Kiler/Tuvalet labels; sole live owner inspects final glyph fit.");
            book.RequestClose();yield return Wait(.40f);
            Check("notebook.close-releases."+width,!book.IsVisible&&!book.BlocksInput&&!w.InputLocked,"Production notebook close releases only its modal gate.","NATIVE_PRODUCTION_MODAL_API");
        }
        IEnumerator Audio(int width)
        {
            yield return Gui(EventType.KeyDown,KeyCode.Return);Check("audio.open-callback."+width,hud.AudioSettingsOpenV56&&hud.Paused&&w.Model.IsPaused,"Pause choice6 Enter owns audio opening.","NATIVE_GUI_EVENT_CALLBACK");
            yield return Wait(.06f);Regions("audio."+width,AudioRows);yield return Hold("audio."+width);
            foreach(int index in new[]{0,1,2})
            {
                Call(hud,"SetAudioVolumeV56",index,.35f+index*.15f);Declare("audio-real-mutator","Production SetAudioVolumeV56 index="+index+"; isolated evidence-derived preference prefix only.");
                Set(hud,"audioChoiceV56",index);yield return Gui(EventType.KeyDown,KeyCode.RightArrow);
                float expected=.40f+index*.15f,actual=index==0?PanicAudioSettingsV56.MasterVolume:index==1?PanicAudioSettingsV56.MusicVolume:PanicAudioSettingsV56.SfxVolume;
                Check("audio.volume-callback."+width+"."+index,Mathf.Abs(actual-expected)<.001f,"Actual persisted isolated volume="+actual,"NATIVE_GUI_EVENT_CALLBACK");
            }
            yield return Gui(EventType.Repaint,readonlyProbe:true);yield return Capture("audio-sliders-"+width,"All three real isolated volume mutators and production right-arrow callbacks; visual slider values.");
            bool mute=PanicAudioSettingsV56.Muted;yield return Gui(EventType.MouseDown,point:AudioRows[3].center);yield return Gui(EventType.MouseUp,point:AudioRows[3].center);
            Check("audio.mouse-mute."+width,PanicAudioSettingsV56.Muted!=mute,"Production menu pointer callback, logical coordinates; no OS mouse.","NATIVE_GUI_EVENT_CALLBACK");
            yield return Capture("audio-muted-"+width,"Actual mute callback state; explicit SES AÇIK/KAPALI label.");PanicAudioSettingsV56.SetMuted(mute);
            hud.CloseAudioSettingsV56();yield return null;Check("audio.close-preserves-pause."+width,!hud.AudioSettingsOpenV56&&hud.Paused&&w.Model.IsPaused);yield return Hold("audio-close."+width);
        }
        string RequirementRows()
        { return string.Join("; ",w.Expedition.CurrentRequirements.Select(r=>r.Id+": owned="+r.Owned+", required="+r.Required+", missing="+r.Missing)); }
        IEnumerator InventoryAndRequirements(int width)
        {
            const string action="anchorfloor",target="bookshelf";
            Vector3 actorBefore=w.PlayerPosition;string focusBefore=Read<string>(w,"focusId");
            bool priorPaused=hud.Paused,priorModelPause=w.Model.IsPaused,priorLock=w.InputLocked;
            bool priorDeveloper=hud.DeveloperMenuOpen,priorHelp=Read<bool>(hud,"KeyboardHelpBlockingV56");
            string gameplayBefore=PanicNotebookVerification.GameplayFingerprint(w);
            try
            {
                // The real inventory shortage branch requires a genuinely reachable selected target.
                // Production target, recipe, stock, budget, collision and visibility gates remain unchanged.
                Vector3 position=new Vector3(-6f,.05f,3.5f);w.Warp(position);
                Declare("actor-placement","Place actual controller at "+position.ToString("F3")+" near existing bookshelf to satisfy normal World.CanInteract; physical traversal is not accepted.");
                for(int i=0;i<6;i++){w.SmokeMove(Vector2.zero,.025f);yield return null;}
                w.SetFocus(target);yield return null;
                var costs=w.Expedition.WorkMaterialCostsV414(action,target).ToArray();int budget=w.Model.Budget;
                bool allowed=w.Expedition.CanBeginWork(action,target);report.productionRequirementsPreflights++;
                Declare("production-recipe-target",action+"/"+target+" uses existing installed item and canonical recipe; cost=$"+w.Expedition.WorkBudgetCostV414(action,target)+"; "+RequirementRows()+"; no work is started and no cost/stock fixture is used.");
                bool exact=w.Expedition.CurrentRequirements.Count==costs.Length&&costs.All(c=>w.Expedition.CurrentRequirements.Any(r=>r.Id==c.Id&&r.Required==c.Required&&r.Owned==w.Expedition.Count(c.Id)&&r.Missing==Mathf.Max(0,c.Required-r.Owned)));
                Check("requirements.real-preflight-rejects."+width,!allowed&&w.Expedition.HasMissingRequirements&&w.Expedition.RequirementAction==action&&w.Expedition.RequirementTargetId==target&&exact&&w.Expedition.MissingRequirements.Count>0&&!w.WorkActive&&w.Model.Budget==budget&&gameplayBefore==PanicNotebookVerification.GameplayFingerprint(w),RequirementRows(),"NATIVE_PRODUCTION_PREFLIGHT");
                Check("requirements.real-reach."+width,w.SelectedTargetIdV56==target&&w.CanInteract,"Real selected target, controller distance and collision/path gate; no permissive visibility bypass.","NATIVE_WORLD_INTERACTION_GATE");
                yield return Wait(.06f);
                bool selectedVisible=Read<bool>(hud,"SelectedTargetHudVisibleV56");
                Check("selected-target.requirements-visible."+width,selectedVisible&&Read<int>(hud,"SelectedTargetHudDrawFramesV56")>0,"Actual production selected-target visibility and Repaint counter; existing reachable bookshelf fixture only.","NATIVE_REPAINT_COUNTER");
                if(selectedVisible)
                {
                    Rect selectedBounds=Read<Rect>(hud,"SelectedTargetHudBoundsV56");
                    Regions("selected-target-requirements."+width,new[]{selectedBounds});
                    yield return Capture("selected-target-requirements-"+width,"Actual visible production bookshelf selected-target card with canonical missing recipe rows; V57 visual restyle, retained V56 snapshot/visibility/bounds API. No additional actor fixture or visibility bypass.");
                }
                w.ToggleInventory();report.productionInventoryOpens++;yield return Wait(.08f);
                Check("inventory.real-open."+width,w.InventoryOpen&&hud.RequirementsCardVisible&&!Read<bool>(hud,"SelectedTargetHudVisibleV56"),"Inventory naturally suppresses selected-target card and selects the real inventory shortage branch.","NATIVE_PRODUCTION_MODAL_API");
                yield return Gui(EventType.Repaint,readonlyProbe:true);
                Regions("inventory-requirements."+width,new[]{new Rect(824,647,660,100)});
                yield return Capture("inventory-requirements-"+width,"Real failed bookshelf floor-anchor preflight; exact current recipe owned/required/missing rows in production DrawInventoryRequirements. V57 compact-world shortage branch remains suppressed by the retained selected-target card.");
                w.Expedition.ClearRequirements();yield return null;
                Check("requirements.semantic-clear."+width,!hud.RequirementsCardVisible&&!w.Expedition.HasMissingRequirements&&w.Expedition.CurrentRequirements.Count==0,"Production ClearRequirements clears presentation only; costs and gameplay stock unchanged.");
                yield return Gui(EventType.Repaint,readonlyProbe:true);
                var slots=new List<Rect>();for(int i=0;i<12;i++)slots.Add(hud.InventorySlotBounds(false,i));for(int i=0;i<8;i++)slots.Add(hud.InventorySlotBounds(true,i));
                Regions("inventory-slots."+width,slots.ToArray());
                yield return Capture("inventory-normal-"+width,"Real normal World.ToggleInventory view:12 inventory slots,8 bag slots, shared frame/key buttons and ordinary optional-equipment panel.");
                float clock=w.Model.TimeRemaining;yield return Wait(.16f);
                Check("inventory.live-clock-preserved."+width,w.InventoryOpen&&w.Model.TimeRemaining<clock-.05f,"Retains existing inventory live-clock semantics.");
                hud.SetPaused(true);yield return null;w.ToggleInventory();yield return null;
                Check("inventory.close-retains-pause-owner."+width,!w.InventoryOpen&&hud.Paused&&w.Model.IsPaused&&w.InputLocked&&!hud.DeveloperMenuOpen&&!Read<bool>(hud,"KeyboardHelpBlockingV56"),"Production inventory close while a later pause owns the lock does not resume the game.","NATIVE_PRODUCTION_MODAL_API");
                yield return Hold("inventory-close-pause."+width);hud.SetPaused(false);yield return null;
                Check("inventory.original-owners-restored."+width,!w.InventoryOpen&&hud.Paused==priorPaused&&w.Model.IsPaused==priorModelPause&&w.InputLocked==priorLock&&hud.DeveloperMenuOpen==priorDeveloper&&Read<bool>(hud,"KeyboardHelpBlockingV56")==priorHelp&&gameplayBefore==PanicNotebookVerification.GameplayFingerprint(w),"No stock, budget, item, score or gameplay change from failed preflight and inventory inspection.");
            }
            finally
            {
                w.Expedition.ClearRequirements();if(w.InventoryOpen)w.ToggleInventory();
                w.Warp(actorBefore);w.SetFocus(focusBefore);
                Declare("actor-placement-restore","Restore actual controller to "+actorBefore.ToString("F3")+" and previous focus after isolated UI fixture; no physical traversal claim.");
            }
            yield return null;
        }
        IEnumerator BagCounterDisplay(int width,int water,int food,string name)
        {
            int oldWater=w.Model.PackedWater,oldFood=w.Model.PackedFood;
            string gameplay=PanicNotebookVerification.GameplayFingerprint(w);
            try
            {
                w.Model.PackedWater=water;w.Model.PackedFood=food;report.packedCounterFixtures++;
                Declare("bag-display-counts","Display-only isolated Model.PackedWater="+water+", PackedFood="+food+"; Expedition bag slots/stock untouched. No shelter/disaster/score action. Restore immediately after this PNG.");
                yield return Gui(EventType.Repaint,readonlyProbe:true);
                Check("bag.display-counts."+name+"."+width,w.Model.PackedWater==water&&w.Model.PackedFood==food&&w.Model.BagReady==(water>=1&&food>=1),"Explicit display fixture only; not packing or shelter acceptance.","NATIVE_DECLARED_DISPLAY_FIXTURE");
                yield return Capture(name+"-"+width,"Explicit immediate-restore model display counts: SU "+Mathf.Min(1,water)+"/1, GIDA "+Mathf.Min(1,food)+"/1; no stock or packing acceptance.");
            }
            finally {w.Model.PackedWater=oldWater;w.Model.PackedFood=oldFood;}
            Check("bag.display-restored."+name+"."+width,gameplay==PanicNotebookVerification.GameplayFingerprint(w),"Original packed counts, bag slots, inventory, budget, score and item state restored after each PNG.");
        }
        IEnumerator ActualLedgerDisplay(int width)
        {
            var m=w.Model;
            Declare("ledger.utility-reset","Reset the actual run's utility safety state before the declared memoized gas event; no independent Model or production start.");
            m.UtilitySafety.Reset();
            Declare("ledger.protection-items","Temporary actual-world fixture: every installed/current item Tier=2, Anchored iff Furniture, FloodProtected=true and FireProtected=true; isolated run is reset by production ReturnToMenu.");
            foreach(var item in m.Items){item.Tier=2;item.Anchored=item.Kind==ItemKind.Furniture;item.FloodProtected=true;item.FireProtected=true;}
            Declare("ledger.finite-water-food","Set actual WaterStock=1 and FoodStock=1, then pack each through production PackWater and PackFood; display/reward fixture only.");
            m.WaterStock=1;m.FoodStock=1;
            Check("ledger.real-pack-stock."+width,m.PackWater()&&m.PackFood()&&m.BagReady,"Finite actual stock packed through production APIs; no campaign supply acceptance.","NATIVE_PRODUCTION_MODEL_API");
            Declare("ledger.finite-support","Set SupportOwnedMask=1 then pack support index0 through production PackSupport(0); no purchase, budget mutation, or campaign supply acceptance.");
            m.SupportOwnedMask=1;
            Check("ledger.real-pack-support."+width,m.PackSupport(0)&&m.SupportCount==1,"Declared owned-bit fixture followed by actual production packing API.","NATIVE_PRODUCTION_MODEL_API");
            var target=m.Items.First(i=>i.Installed&&i.Kind==ItemKind.Furniture&&i.ArtId=="bookshelf");
            Declare("ledger.memoized-gas-flare","Memoize one 7.5-point GasFlare against the real installed bookshelf for the current first-wave earthquake key; no secondary RNG evaluation is injected.");
            var memo=Read<Dictionary<string,PanicSecondaryEventV5>>(m,"secondaryRollsV5");
            memo[m.Wave+":"+(int)DisasterType.Earthquake]=new PanicSecondaryEventV5(PanicSecondaryEventKindV5.GasFlare,target.Id,7.5f,"Gaz vanası açık kaldı: küçük bir alevlenme oldu");
            int budgetBefore=m.Budget;
            Declare("ledger.typed-model-shelter-gate","Call typed Model.TryEnterShelterV5(RequiredShelterV5) directly. This is an explicit model-gate fixture and makes no physical route, ray, location, or traversal claim.");
            report.productionModelShelterCalls++;
            Check("ledger.production-model-shelter."+width,m.TryEnterShelterV5(m.RequiredShelterV5)&&m.Phase==PanicPhase.Disaster,"Typed production model transition only; no physical shelter-path acceptance.","NATIVE_PRODUCTION_MODEL_API");
            yield return Wait(.12f);
            Check("ledger.no-premature-result."+width,!hud.WaveLedgerVisibleV56&&m.Phase==PanicPhase.Disaster&&m.LastWaveLedgerV56==null,"Disaster cinematic remains the production resolution owner before a committed result.");
            Declare("ledger.production-cinematic-skip","Use actual World.SkipResidenceCinematic after its normal phase transition to resolve current damage/reward exactly once; no Model.Tick or fixture damage call.");
            report.productionSkipCalls++;
            w.SkipResidenceCinematic();
            float until=Time.realtimeSinceStartup+4f;
            while((m.Phase!=PanicPhase.Inspection||w.CinematicActive||!hud.WaveLedgerVisibleV56)&&Time.realtimeSinceStartup<until)yield return null;
            Check("ledger.auto-open-stable-inspection."+width,m.Phase==PanicPhase.Inspection&&!w.CinematicActive&&hud.WaveLedgerVisibleV56,"Actual committed result auto-opens in stable production Inspection.","NATIVE_PRODUCTION_MODAL_API");
            var ledger=m.LastWaveLedgerV56;
            Check("ledger.actual-negative-bonus-credit."+width,ledger!=null&&!ledger.IsSimulation&&ledger.HasResolvedDamage&&ledger.DamageDeduction>0&&ledger.SupportBonus>0&&ledger.ActualCredited>0&&ledger.BudgetBeforeReward==budgetBefore&&ledger.BudgetAfterReward==m.Budget,"Actual ledger has negative damage deduction, positive support bonus and credited budget rows.","NATIVE_PRODUCTION_MODEL_API");
            Check("ledger.actual-item-and-utility-rows."+width,ledger!=null&&ledger.Rows.Any(r=>r.Id==target.Id)&&ledger.Rows.Any(r=>r.Id.StartsWith("utility:",StringComparison.Ordinal)),"Actual committed damage includes a real item and separate memoized gas contribution.","NATIVE_PRODUCTION_MODEL_API");
            float modalClock=m.PhaseTime;int modalBudget=m.Budget;long modalScore=m.Score,waveScore=m.LastWaveScore;int modalRolls=m.SecondaryRollCountV5;
            string modalRng=JsonUtility.ToJson(UnityEngine.Random.state),modalScoreFile=Hash(PanicModel.ScorePathOverride);
            yield return Gui(EventType.Repaint,readonlyProbe:true);
            Check("ledger.replay-readonly."+width,m.PhaseTime==modalClock&&m.Budget==modalBudget&&m.Score==modalScore&&m.LastWaveScore==waveScore&&m.SecondaryRollCountV5==modalRolls&&modalRng==JsonUtility.ToJson(UnityEngine.Random.state)&&modalScoreFile==Hash(PanicModel.ScorePathOverride),"Read-only production Repaint leaves phase clock, budget, score, secondary roll count, Unity RNG and isolated score file unchanged.","NATIVE_SYNCHRONOUS_REPAINT");
            string rendered=Read<string>(hud,"WaveLedgerRenderedTextV56")??"";
            Check("ledger.rendered-budget-rows."+width,rendered.Contains("Hasardan ödül kesintisi")&&rendered.Contains("Çanta destek bonusu")&&rendered.Contains("Hesaba geçen ödül"),"Current production Repaint rendered the actual negative, bonus and credited accounting rows.","NATIVE_ACTUAL_REPAINT_TELEMETRY");
            yield return Hold("ledger-budget."+width,.50f);
            Check("ledger.wallhold-budget-score-rng."+width,m.PhaseTime==modalClock&&m.Budget==modalBudget&&m.Score==modalScore&&m.LastWaveScore==waveScore&&m.SecondaryRollCountV5==modalRolls&&modalRng==JsonUtility.ToJson(UnityEngine.Random.state)&&modalScoreFile==Hash(PanicModel.ScorePathOverride),"Modal wall hold leaves phase clock, budget, score, secondary roll count, Unity RNG and isolated score file unchanged.","NATIVE_WALL_CLOCK_MODAL_HOLD");
            yield return Capture("ledger-budget-"+width,"Actual committed first-wave damage/reward ledger with declared temporary fixtures; production negative, bonus and credited rows at this resolution.");
            report.actualLedgerCaptures++;
            Call(hud,"CloseWaveLedgerV56");
            Check("ledger.production-close."+width,!hud.WaveLedgerVisibleV56&&!m.WaveLedgerOpenV56,"Close through production CloseWaveLedgerV56; inspection model remains available.","NATIVE_PRODUCTION_MODAL_API");
        }
        IEnumerator Gameplay(int width)
        {
            yield return Begin();yield return Gui(EventType.Repaint,readonlyProbe:true);
            Regions("status."+width,new[]{hud.WaveCardBounds,hud.TimerCardBounds,hud.ResourceCardBounds,Read<Rect>(hud,"DamageCardBoundsV57")});
            yield return Capture("gameplay-status-"+width,"Normal production preparation; real budget, wave, timer and damage.");
            float clock=w.Model.TimeRemaining;yield return Wait(.18f);Check("clock.normal-advances."+width,w.Model.TimeRemaining<clock-.05f);
            yield return NotebookRooms(width);
            yield return InventoryAndRequirements(width);
            hud.SetPaused(true);yield return null;yield return Hold("pause."+width);Regions("pause."+width,PauseActions);
            for(int choice=0;choice<7;choice++)
            {
                Check("pause.callback-focus."+width+"."+choice,hud.PauseChoice==choice);
                yield return Capture("pause-choice"+choice+"-"+width,"Real pause focus="+choice+" from production key callbacks; world clock held.");
                if(choice<6)yield return Gui(EventType.KeyDown,KeyCode.DownArrow);
            }
            yield return Gui(EventType.Repaint,readonlyProbe:true);yield return Audio(width);
            bool music=PanicAudioSettingsV56.MusicEnabled,motion=w.ReducedFpsMotionV54;
            yield return Gui(EventType.MouseDown,point:PauseActions[4].center);yield return Gui(EventType.MouseUp,point:PauseActions[4].center);
            Check("pause.mouse-music."+width,PanicAudioSettingsV56.MusicEnabled!=music,"Actual MenuActionV57 music callback.","NATIVE_GUI_EVENT_CALLBACK");PanicAudioSettingsV56.MusicEnabled=music;
            yield return Gui(EventType.MouseDown,point:PauseActions[5].center);yield return Gui(EventType.MouseUp,point:PauseActions[5].center);
            Check("pause.mouse-motion."+width,w.ReducedFpsMotionV54!=motion,"Actual MenuActionV57 reduced motion callback.","NATIVE_GUI_EVENT_CALLBACK");w.SetReducedFpsMotionV54(motion);
            Set(hud,"_pauseChoice",0);Declare("display-selection","pause _pauseChoice=0 for non-destructive resume action; restart/return/exit are not activated.");
            yield return Gui(EventType.KeyDown,KeyCode.Return);Check("pause.resume-callback."+width,!hud.Paused&&!w.Model.IsPaused);
            clock=w.Model.TimeRemaining;yield return Wait(.18f);Check("pause.clock-resumes."+width,w.Model.TimeRemaining<clock-.05f);
            bool helpPriorLock=w.InputLocked,helpPriorModelPause=w.Model.IsPaused;
            bool opened=(bool)Call(hud,"OpenKeyboardHelpV56");Check("f1.retained-production-open."+width,opened);yield return Wait(.4f);yield return Hold("f1."+width);
            Check("f1.retained-visible."+width,Read<bool>(hud,"KeyboardHelpVisibleV56")&&Read<int>(hud,"KeyboardHelpDrawFramesV56")>0);
            yield return new WaitForEndOfFrame();
            bool geometryPassed=PanicKeyboardHelpGeometryChecksV57.ValidateCurrentDraw(hud,(id,pass,detail)=>Check(id,pass,detail));
            var geometryReceipt=PanicKeyboardHelpGeometryChecksV57.LastReceipt;
            string geometryPath=Path.Combine(w.EvidencePath,"keyboard-geometry-v57-"+width+".json");
            File.WriteAllText(geometryPath,JsonUtility.ToJson(geometryReceipt,true));
            Check("f1.geometry.receipt."+width,geometryPassed&&geometryReceipt!=null&&geometryReceipt.width==width&&geometryReceipt.height==(width==1600?900:540)&&File.Exists(geometryPath),geometryPath,"NATIVE_ACTUAL_REPAINT_TELEMETRY");
            yield return Capture("f1-retained-"+width,"Actual settled F1 framebuffer with all used keys; includes read-only Repaint geometry receipt. No OS F1 or human-legibility claim.");
            float helpClockAtClose=w.Model.TimeRemaining;KeyCode closeHelpKey=width==1600?KeyCode.F1:KeyCode.Escape;
            bool closeConsumed=(bool)Call(hud,"ProcessKeyboardHelpKeyV56",closeHelpKey);
            Check("f1.production-close-consumed."+width,closeConsumed,"Production ProcessKeyboardHelpKeyV56("+closeHelpKey+") consumed by the modal; no OS input.","NATIVE_PRODUCTION_MODAL_API");
            yield return Wait(.1f);
            Check("f1.close-first-100ms-holds-clock."+width,Read<bool>(hud,"KeyboardHelpBlockingV56")&&w.Model.KeyboardHelpOpenV56&&w.Model.IsPaused==helpPriorModelPause&&w.InputLocked&&Mathf.Abs(w.Model.TimeRemaining-helpClockAtClose)<.0001f,"First0.10s of production close animation retains the independent keyboard-help owner, original pause state, input lock and exact game clock.","NATIVE_PRODUCTION_MODAL_API");
            yield return Wait(.3f);
            Check("f1.retained-close."+width,!Read<bool>(hud,"KeyboardHelpBlockingV56")&&!w.Model.IsPaused&&w.InputLocked==helpPriorLock&&w.Model.IsPaused==helpPriorModelPause&&w.Model.TimeRemaining<helpClockAtClose-.005f,"After the next0.30s the close animation releases its original lock owner and the game clock advances.","NATIVE_PRODUCTION_MODAL_API");
            w.Model.TimeRemaining=PanicHomeWorld.BagPreparationSeconds-1f;Declare("preparation-clock","Set remaining preparation to bag threshold minus1, then production BeginBagPreparation; no campaign timing acceptance.");
            Check("bag.production-begin."+width,w.BeginBagPreparation());float until=Time.realtimeSinceStartup+4f;while(w.BagCameraTransitionActive&&Time.realtimeSinceStartup<until)yield return null;
            Check("bag.real-view."+width,w.BagFirstPersonActive&&!w.BagCameraTransitionActive);yield return Gui(EventType.Repaint,readonlyProbe:true);
            Regions("bag."+width,new[]{new Rect(24,20,484,76),hud.TimerCardBounds,Read<Rect>(hud,"BagNeedsBoundsV57")});
            yield return Capture("bag-needs-"+width,"Real FPS bag stage with actual water/food counts; explicit timer fixture.");
            yield return BagCounterDisplay(width,1,0,"bag-water-only");
            yield return BagCounterDisplay(width,1,1,"bag-full");
            hud.SetPaused(false);yield return ActualLedgerDisplay(width);
            hud.SetPaused(false);w.ReturnToMenu();yield return null;
        }
        IEnumerator Run()
        {
            bool registered=PanicTestLaunchRules.Drivers.Contains(Flag);Check("launch.registered-before-model-driver",registered,"Required owner hook: add flag to PanicTestLaunchRules.Drivers.","NATIVE_LAUNCH_GUARD");
            Check("launch.isolated-guid",Isolated()&&!File.Exists(PanicModel.ScorePathOverride),PanicModel.ScorePathOverride,"NATIVE_FILE_PATH");
            Check("launch.one-driver",Environment.GetCommandLineArgs().Count(a=>PanicTestLaunchRules.Drivers.Contains(a))==1,"Exactly one registered driver.","NATIVE_LAUNCH_GUARD");
            Check("launch.graphical",!Application.isBatchMode&&SystemInfo.graphicsDeviceType!=GraphicsDeviceType.Null,"Use visible graphical player; no -batchmode or -nographics.","NATIVE_GRAPHICAL_SURFACE");
            if(!registered||!Isolated()||File.Exists(PanicModel.ScorePathOverride))throw new InvalidOperationException("Refusing UI fixture outside a fresh registered GUID profile.");
            if(Application.isBatchMode||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null){report.invalidRenderSurface=true;throw new InvalidOperationException("INVALID_RENDER_SURFACE: graphical player required.");}
            while(!SplashScreen.isFinished)yield return null;
            Check("hud.version",Read<string>(hud,"UiDesignVersionV57")=="5.7");
            Check("hud.status-contract",SameRect(hud.WaveCardBounds,new Rect(24,20,430,76))&&SameRect(hud.TimerCardBounds,new Rect(660,20,208,76))&&SameRect(hud.ResourceCardBounds,new Rect(1332,20,244,76)));
            var symbols=typeof(PanicModel).Assembly.GetType("TooLateToPanic.PanicUiSymbolsV57",true);var has=symbols.GetMethod("Has",Static);
            foreach(string key in new[]{"play","continue","book","bag","settings","exit","home","back","pause","sound","motion","check","clock","warning","budget","damage","shield","earthquake","flood","fire","storm","water","food","tools","shelter","help"})Check("symbol."+key,(bool)has.Invoke(null,new object[]{key}),"Font-independent original symbol key.","NATIVE_SYMBOL_API");
            Check("symbol.unknown-refused",!(bool)has.Invoke(null,new object[]{"not-a-ui-symbol"}));
            report.preferencePrefix=PanicAudioSettingsV56.ProfilePreferencePrefix;Check("audio.isolated-prefix",PanicAudioSettingsV56.IsolatedProfile&&report.preferencePrefix.StartsWith("TLTP.Audio.V56.QA.",StringComparison.Ordinal));
            if(!PanicAudioSettingsV56.IsolatedProfile)throw new InvalidOperationException("Refusing audio mutations outside isolated QA preferences.");
            oldMaster=PanicAudioSettingsV56.MasterVolume;oldMusic=PanicAudioSettingsV56.MusicVolume;oldSfx=PanicAudioSettingsV56.SfxVolume;oldMuted=PanicAudioSettingsV56.Muted;oldMusicEnabled=PanicAudioSettingsV56.MusicEnabled;oldMotion=w.ReducedFpsMotionV54;preferencesHeld=true;
            pointer=gameObject.AddComponent<PanicInteractionAudit>();pointer.enabled=false;Declare("gui-adapter","Disabled existing PanicInteractionAudit supplies logical pointer to explicitly injected production OnGUI callbacks; does not own an independent audit run.");
            foreach(int width in new[]{1600,960}){yield return Resolution(width,width==1600?900:540);hud.SetPaused(false);w.ReturnToMenu();yield return null;yield return Title(width);yield return Gameplay(width);}
            Check("runtime.no-errors",report.errors.Count==0,string.Join("\n",report.errors));
        }
        void Restore()
        {
            if(scoreDisplayBefore!=null&&w&&w.Model!=null){w.Model.Highscores=scoreDisplayBefore;scoreDisplayBefore=null;}
            if(!preferencesHeld)return;PanicAudioSettingsV56.SetVolumes(oldMaster,oldMusic,oldSfx);PanicAudioSettingsV56.SetMuted(oldMuted);PanicAudioSettingsV56.MusicEnabled=oldMusicEnabled;
            if(w)w.SetReducedFpsMotionV54(oldMotion);
            report.preferencesRestored=Mathf.Abs(PanicAudioSettingsV56.MasterVolume-oldMaster)<.001f&&Mathf.Abs(PanicAudioSettingsV56.MusicVolume-oldMusic)<.001f&&Mathf.Abs(PanicAudioSettingsV56.SfxVolume-oldSfx)<.001f&&PanicAudioSettingsV56.Muted==oldMuted&&PanicAudioSettingsV56.MusicEnabled==oldMusicEnabled&&w&&w.ReducedFpsMotionV54==oldMotion;
            preferencesHeld=false;
        }
        void Finish()
        {
            if(finished)return;finished=true;pending=false;
            try{Restore();}catch(Exception ex){report.error+="\nrestore: "+ex;}
            report.realScoreAfter=Hash(realScore);Check("score.real-file-unchanged",report.realScoreBefore==report.realScoreAfter,report.realScoreBefore+" -> "+report.realScoreAfter,"NATIVE_FILE_HASH");
            report.globalPreferencesAfter=GlobalPreferenceFingerprint();Check("audio.global-preferences-unchanged",report.globalPreferencesBefore==report.globalPreferencesAfter,"Default preference namespace read-only existence/value fingerprint.","NATIVE_GLOBAL_PREFERENCE_READS");
            Check("audio.preferences-restored",report.preferencesRestored);Check("score.remains-isolated",Isolated(),PanicModel.ScorePathOverride,"NATIVE_FILE_PATH");
            report.seconds=Time.realtimeSinceStartup-began;report.finishedUtc=DateTime.UtcNow.ToString("o");
            report.passed=report.complete&&!report.invalidRenderSurface&&report.errors.Count==0&&report.productionStarts==2&&report.productionRequirementsPreflights==2&&report.productionInventoryOpens==2&&report.packedCounterFixtures==4&&report.productionModelShelterCalls==2&&report.productionSkipCalls==2&&report.actualLedgerCaptures==2&&report.repaintProbes>=22&&report.captures.Count>=50&&report.checks.All(r=>r.status=="PASS");
            Application.logMessageReceived-=Log;if(Isolated())File.WriteAllText(Path.Combine(w.EvidencePath,"ui-v57-smoke.json"),JsonUtility.ToJson(report,true));
            Debug.Log("UI_V57_SMOKE "+(report.passed?"PASS":report.invalidRenderSurface?"INVALID_RENDER_SURFACE":"FAIL")+" checks="+report.checks.Count+" captures="+report.captures.Count+" fixtures="+report.fixtureCount+" seconds="+report.seconds);Application.Quit(report.passed?0:2);
        }
        static bool CompletePng(string path,out byte[] bytes)
        {
            bytes=null;try{if(!File.Exists(path))return false;var value=File.ReadAllBytes(path);int n=value.Length;
                if(n<32||value[0]!=137||value[1]!=80||value[2]!=78||value[3]!=71||value[n-8]!=73||value[n-7]!=69||value[n-6]!=78||value[n-5]!=68)return false;bytes=value;return true;
            }catch(IOException){return false;}
        }
        static string Hash(string path){if(string.IsNullOrEmpty(path)||!File.Exists(path))return "absent";using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        static string GlobalPreferenceFingerprint()
        {
            const string prefix="TLTP.Audio.V56.";string text="";
            foreach(string key in new[]{"master","music","sfx"})text+=key+":"+PlayerPrefs.HasKey(prefix+key)+":"+PlayerPrefs.GetFloat(prefix+key,-999f).ToString("R",CultureInfo.InvariantCulture)+"|";
            foreach(string key in new[]{"mute","musicEnabled"})text+=key+":"+PlayerPrefs.HasKey(prefix+key)+":"+PlayerPrefs.GetInt(prefix+key,-999)+"|";return text;
        }
    }
}
