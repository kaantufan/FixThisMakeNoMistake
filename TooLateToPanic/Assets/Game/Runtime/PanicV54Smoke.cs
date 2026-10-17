using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Opt-in, bounded native story/API/framebuffer evidence. Never OS or human acceptance.</summary>
    public sealed class PanicV54Smoke : MonoBehaviour
    {
        const string Flag = "--v54-smoke";
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id, status, detail, layer; public int frame; }
        [Serializable] public sealed class Shot { public string path, context; public int width, height, frame; }
        [Serializable] public sealed class Timeline
        {
            public string hazard; public float wallSeconds, lastObservedAge, maxClockDrift, maxActorDrift;
            public int observedFrames, observedBeats; public bool naturalExit;
        }
        [Serializable] public sealed class Receipt
        {
            public string utc, dllSha256, realScoreBefore, realScoreAfter, error, scorePath;
            public string evidence = "Native production API/model/physics/camera/effect and actual ScreenCapture. Explicit wave/mask/seed/stock/time/position fixtures. No OS input, listening, human comfort or full campaign acceptance.";
            public string captureLimit = "PNG presence and dimensions prove framebuffer capture only. Text fit, legibility and final visual acceptance require owner inspection of the captured pixels.";
            public bool complete, passed, osInput, humanAcceptance, listening;
            public float seconds;
            public int setupWarps,timeFixtures; public int productionStartGameCalls, productionJuryStartCalls, hazardAssignments, separateModelFixtures;
            public int activeClockAssignments, cinematicAgeAssignments, directWarpCalls, manualTickCalls, cameraRenderCalls;
            public List<Row> checks = new List<Row>();
            public List<Shot> captures = new List<Shot>();
            public List<Timeline> timelines = new List<Timeline>();
            public List<string> errors = new List<string>();
        }
        readonly Receipt report = new Receipt();
        PanicHomeWorld w; PanicHomeHud hud; float began; string realScore; bool finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (world) world.gameObject.AddComponent<PanicV54Smoke>();
        }
        void Start()
        {
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); began = Time.realtimeSinceStartup;
            realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.utc = DateTime.UtcNow.ToString("o"); report.dllSha256 = Hash(typeof(PanicModel).Assembly.Location);
            report.realScoreBefore = Hash(realScore); report.scorePath = PanicModel.ScorePathOverride;
            Application.logMessageReceived += Log;
            StartCoroutine(Guard(Run()));
        }
        void Update()
        {
            if (!finished && Time.realtimeSinceStartup - began > 180f)
            { report.error = "180-second relevant feature smoke hard cap reached"; Finish(); }
        }
        void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                report.errors.Add(message + "\n" + stack);
        }
        void Check(string id, bool pass, string detail = "", string layer = "NATIVE_API_FRAME")
        {
            report.checks.Add(new Row { id = id, status = pass ? "PASS" : "FAIL", detail = detail, layer = layer, frame = Time.frameCount });
            Debug.Log("V54_CHECK " + (pass ? "PASS " : "FAIL ") + id + " " + detail);
        }
        IEnumerator Guard(IEnumerator body)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(body);
            while (stack.Count > 0 && !finished)
            {
                bool next; object value = null;
                try { next = stack.Peek().MoveNext(); if (next) value = stack.Peek().Current; }
                catch (Exception e) { report.error = e.ToString(); break; }
                if (!next) { stack.Pop(); continue; }
                if (value is IEnumerator nested) stack.Push(nested); else yield return value;
            }
            if (!finished) { report.complete = string.IsNullOrEmpty(report.error); Finish(); }
        }
        IEnumerator Wait(float seconds)
        { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
        float StoryAge { get { return (float)w.GetType().GetField("residenceCinematicAge", Flags).GetValue(w); } }
        bool ScoreIsIsolated()
        {
            if (string.IsNullOrEmpty(w.EvidencePath) || string.IsNullOrEmpty(PanicModel.ScorePathOverride)) return false;
            return string.Equals(Path.GetFullPath(PanicModel.ScorePathOverride),
                Path.Combine(Path.GetFullPath(w.EvidencePath), "isolated-home-scores.json"), StringComparison.OrdinalIgnoreCase);
        }
        IEnumerator Resolution(int width, int height)
        {
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
            float deadline = Time.realtimeSinceStartup + 2f;
            while ((Screen.width != width || Screen.height != height) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return Wait(.35f);
            Check("resolution." + width + "x" + height, Screen.width == width && Screen.height == height,
                "Actual graphical framebuffer=" + Screen.width + "x" + Screen.height, "NATIVE_GRAPHICAL_SURFACE");
        }
        IEnumerator Capture(string name, string context)
        {
            yield return new WaitForEndOfFrame();
            string path = Path.Combine(w.EvidencePath, name + ".png");
            int width = Screen.width, height = Screen.height, frame = Time.frameCount;
            ScreenCapture.CaptureScreenshot(path);
            float deadline = Time.realtimeSinceStartup + 2f;
            int pngWidth = 0, pngHeight = 0;
            while (!PngSize(path, out pngWidth, out pngHeight) && Time.realtimeSinceStartup < deadline) yield return null;
            bool valid = pngWidth == width && pngHeight == height;
            report.captures.Add(new Shot { path = path, context = context, width = pngWidth, height = pngHeight, frame = frame });
            Check("capture." + name, valid, "Captured=" + pngWidth + "x" + pngHeight + "; " + context, "NATIVE_FRAMEBUFFER_FILE");
        }
        IEnumerator BeginNormal(DisasterType hazard)
        {
            w.ReturnToMenu(); w.StartGame(false); report.productionStartGameCalls++;
            w.FullFlowScriptedInput = true;
            w.Model.Disaster = hazard; report.hazardAssignments++;
            yield return null;
            if (hud.FreeplayOnboardingOpenV414) hud.ConfirmFreeplayOnboardingV414();
            yield return null;
            Check("opening.started." + hazard + "." + report.productionStartGameCalls,
                w.IntroStoryActiveV414 && !hud.FreeplayOnboardingOpenV414 && !w.Model.IsPaused &&
                w.Model.Phase == PanicPhase.Preparing && w.Model.Disaster == hazard,
                "Production StartGame resets its own actor; smoke assigns only hazard, never clock/age/actor.");
        }
        void Set(object obj,string name,object value) { obj.GetType().GetField(name,Flags).SetValue(obj,value); }
        T Get<T>(object obj,string name) { return (T)obj.GetType().GetField(name,Flags).GetValue(obj); }
        object Call(object obj,string name,params object[] args) { return obj.GetType().GetMethods(Flags).First(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(obj,args); }
        void Compound(DisasterType primary,int mask)
        {
            w.Model.Wave=2; w.Model.Disaster=primary;
            Set(w.Model,"compoundWaveV54",2);Set(w.Model,"compoundPrimaryV54",primary);Set(w.Model,"compoundMaskV54",mask);
            report.hazardAssignments++;report.timeFixtures++;
        }
        IEnumerator Place(Vector3 p)
        {
            w.Warp(p+Vector3.up*.04f); report.setupWarps++;
            for(int i=0;i<12;i++){w.SmokeMove(Vector2.zero,.025f);yield return null;}
            Check("physics.grounded."+report.setupWarps,w.IsGrounded,p.ToString(),"NATIVE_PHYSICS_FIXTURE");
        }
        void Look(float yaw,float pitch)
        {
            Set(w,"bagYaw",yaw);Set(w,"bagPitch",pitch);w.ApplyBagPoseV414();
        }
        IEnumerator Guide(string label)
        {
            // Allow the sliced search to finish; do not mistake an interim iterator for no route.
            float until=Time.realtimeSinceStartup+8;
            yield return Wait(.15f);
            while((w.GuidancePlanningV54||!w.GuidanceRouteValidV54)&&Time.realtimeSinceStartup<until)yield return null;
            Check("guide.reachable."+label,w.GuidanceVisibleV54&&w.GuidanceRouteValidV54&&!w.GuidancePlanningV54,
                w.GuidanceObjectiveIdV54+" / "+w.GuidanceObjectiveLabelV54+" / "+w.GuidanceCheckpointLabelV54+" / nodes="+w.GuidanceExpandedNodesV54);
            Check("guide.bounded."+label,w.GuidanceExpandedNodesV54<=20800&&w.GuidancePeakSliceMillisecondsV54<100,
                "Plan CPU ms="+w.LastGuidancePlanMillisecondsV54+"; peak Update slice ms="+w.GuidancePeakSliceMillisecondsV54);
            Check("guide.checkpoint-finite."+label,!float.IsNaN(w.GuidanceCheckpointPositionV54.x)&&w.GuidanceWaypointCountV54>0,
                "Checkpoint="+w.GuidanceCheckpointPositionV54+"; route points="+w.GuidanceWaypointCountV54);
        }
        IEnumerator BlackFrame()
        {
            yield return new WaitForEndOfFrame();
            var tex=ScreenCapture.CaptureScreenshotAsTexture();
            float max=0;foreach(var c in tex.GetPixels32())max=Mathf.Max(max,Mathf.Max(c.r,Mathf.Max(c.g,c.b)));
            string path=Path.Combine(w.EvidencePath,"fps-full-black.png");File.WriteAllBytes(path,tex.EncodeToPNG());Destroy(tex);
            report.captures.Add(new Shot{path=path,context="Actual ScreenCapture at full blackout; max RGB="+max,width=Screen.width,height=Screen.height,frame=Time.frameCount});
            Check("fps.actual-opaque-black-frame",max<=1,"Actual framebuffer max byte="+max,"NATIVE_FRAMEBUFFER_PIXELS");
        }
        IEnumerator Run()
        {
            yield return Wait(.4f);
            Check("launch.score-isolation",ScoreIsIsolated()&&PanicTestLaunchRules.Drivers.Contains(Flag));
            Check("launch.graphical",!Application.isBatchMode&&SystemInfo.graphicsDeviceType!=GraphicsDeviceType.Null);
            if(!ScoreIsIsolated())yield break;
            Check("pause.reduced-motion-keyboard",PanicHomeHud.NavigatePauseChoice(4,KeyCode.DownArrow)==5&&PanicHomeHud.NavigatePauseChoice(5,KeyCode.DownArrow)==0);
            PanicCompoundChecksV54.Run((name,pass,detail)=>Check(name,pass,detail,"NATIVE_MODEL_FIXTURE"));
            yield return Resolution(1600,900);
            yield return BeginNormal(DisasterType.Earthquake);
            w.SkipIntroStoryV414();yield return Wait(.15f);
            w.Model.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime,true);
            yield return Guide("initial-precaution");
            Check("guide.real-source-or-work",!string.IsNullOrEmpty(w.GuidanceObjectiveIdV54)&&
                (w.Expedition.Sources.Exists(s=>s.Id==w.GuidanceObjectiveIdV54&&!s.AutoLooted)||w.Model.Items.Exists(i=>i.Id==w.GuidanceObjectiveIdV54)||w.GuidanceObjectiveActionV54.EndsWith("off")),w.GuidanceObjectiveLabelV54);
            yield return Capture("guidance-precautions","Actual first-wave unfinished precaution or finite material source.");
            Check("floor.scoped-shadow-repair",w.FloorFinishRepairCountV54>0&&w.FloorFinishSelfShadowSuppressedV54,"renderers="+w.FloorFinishRepairCountV54,"NATIVE_RENDER_STATE");
            w.Model.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime,false);
            w.Model.TimeRemaining=45;report.timeFixtures++;
            float deadline=Time.realtimeSinceStartup+2;
            while(!w.BagHandoffActiveV51&&Time.realtimeSinceStartup<deadline)yield return null;
            Vector3 actor=w.PlayerPosition;float clock=w.Model.TimeRemaining;
            while(w.BagHandoffActiveV51&&w.BagHandoffElapsedV51<.36f)yield return null;
            Check("fps.full-black-interval-active",w.BagFullBlackIntervalV51,"age="+w.BagHandoffElapsedV51);
            yield return BlackFrame();
            while(w.BagCameraTransitionActive&&Time.realtimeSinceStartup<deadline+3)yield return null;
            Check("fps.handoff-real-clock-position-held",w.BagFirstPersonActive&&Mathf.Abs(w.Model.TimeRemaining-clock)<.02f&&Vector3.Distance(w.PlayerPosition,actor)<.005f,
                "clock delta="+(w.Model.TimeRemaining-clock)+"; actor delta="+Vector3.Distance(w.PlayerPosition,actor));
            Check("fps.full-black-drawn-duration",w.BagHandoffFullBlackDrawFramesV51>0&&Mathf.Abs(w.LastBagHandoffFullBlackSecondsV51-.84f)<.006f,
                "frames="+w.BagHandoffFullBlackDrawFramesV51+"; seconds="+w.LastBagHandoffFullBlackSecondsV51);
            yield return Wait(.25f);
            Check("fps.clock-resumed",w.Model.TimeRemaining<clock-.1f);
            w.Model.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime,true);
            yield return Guide("water");
            var water=w.Expedition.FindWorldSupply(w.GuidanceObjectiveIdV54);
            Check("guide.live-water",water!=null&&water.Count>0&&water.ItemId=="water",w.GuidanceObjectiveIdV54);
            yield return Capture("fps-water-guide","Continued panic veil + route checkpoint at actual missing water.");
            float minRoll=100,maxRoll=-100,minFov=100,maxFov=0;
            Quaternion input=w.FpsInputRotationV54;Vector3 position=w.PlayerPosition;
            float end=Time.realtimeSinceStartup+3;
            while(Time.realtimeSinceStartup<end)
            {float roll=Mathf.DeltaAngle(0,w.View.transform.eulerAngles.z);minRoll=Mathf.Min(minRoll,roll);maxRoll=Mathf.Max(maxRoll,roll);minFov=Mathf.Min(minFov,w.View.fieldOfView);maxFov=Mathf.Max(maxFov,w.View.fieldOfView);yield return null;}
            Check("fps.actual-dizzy-roll-fov",maxRoll-minRoll>.3f&&maxFov-minFov>.05f,"roll range="+(maxRoll-minRoll)+"; fov range="+(maxFov-minFov),"NATIVE_CAMERA_FRAMES");
            Check("fps.cosmetics-do-not-move-input-actor",Quaternion.Angle(input,w.FpsInputRotationV54)<.001f&&Vector3.Distance(position,w.PlayerPosition)<.01f);
            Check("fps.visible-veil",w.FpsVeilOpacityV414>=.57f&&w.FpsVeilOpacityV414<=.75f&&w.FpsStressVisibleV414,"opacity="+w.FpsVeilOpacityV414);
            // Actual finite production collect API; direct world-position fixture is disclosed by Place.
            if(water!=null){Check("gather.actual-finite-water",w.Expedition.TryCollectWorldSupply(water.Id));}
            yield return Guide("food");
            var food=w.Expedition.FindWorldSupply(w.GuidanceObjectiveIdV54);
            Check("guide.water-completion-advances-food",w.Model.PackedWater>=1&&food!=null&&food.ItemId=="food"&&food.Count>0,w.GuidanceObjectiveIdV54);
            if(food!=null)Check("gather.actual-finite-food",w.Expedition.TryCollectWorldSupply(food.Id));
            yield return Guide("basement");
            Check("guide.correct-basement",w.Model.BagReady&&w.GuidanceObjectiveActionV54=="shelter"&&w.GuidanceObjectivePositionV54.y<0);
            yield return Capture("fps-basement-route","Actual ready bag targets basement via checkpoint chain.");
            w.SetReducedFpsMotionV54(true);Look(90,58);
            yield return Place(new Vector3(2.65f,3.2f,4.6f));yield return Wait(.2f);
            var floors=w.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("floor_finish_")&&r.name.Contains("Walnut")).ToArray();
            foreach(var r in floors)r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            yield return Capture("upper-floor-legacy-self-shadow","Counterfactual old micro-board shadow casting; same actor/camera, reduced motion, real floor.");
            foreach(var r in floors)r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            yield return Capture("upper-floor-fixed","Repaired floor micro-board self-shadow; closed glove shells at downward pitch.");
            Check("floor.restored-fixed-state",w.FloorFinishSelfShadowSuppressedV54);
            Quaternion still=w.View.transform.rotation;float stillFov=w.View.fieldOfView;
            yield return Wait(.4f);
            Check("fps.reduced-motion-steady",Quaternion.Angle(still,w.View.transform.rotation)<.001f&&Mathf.Abs(stillFov-w.View.fieldOfView)<.001f&&w.FpsVeilOpacityV414>=.57f);
            Compound(DisasterType.Fire,6);yield return Guide("flood-fire-upper");
            Check("compound.secondary-flood-guides-upper",w.UsesUpperPanicRoomV5&&w.GuidanceObjectiveIdV54=="@panicroom",w.GuidanceObjectiveIdV54);
            yield return Capture("fps-upper-safe-route","Flood secondary to Fire still routes to physical upper panic room.");
            w.Model.SetPaused(true);yield return Wait(.1f);
            Check("guide.pause-hide",!w.FpsStressVisibleV414);w.Model.SetPaused(false);
            yield return Resolution(960,540);yield return Capture("fps-guide-960","Actual narrow framebuffer, guidance and hands.");
            yield return Resolution(1600,900);
            yield return BeginNormal(DisasterType.Fire);w.SkipIntroStoryV414();yield return Wait(.12f);
            Compound(DisasterType.Fire,7);w.Model.PackedWater=w.Model.PackedFood=1;w.Expedition.SyncConsumedBagSupplies();
            Check("compound.dev-cinema-start",w.DeveloperSkipWaveV51());yield return Wait(.5f);
            var fx=w.GetComponent<PanicHomeEffects>();
            Check("compound.three-visuals-and-audio-slots",fx.ActiveHazardVisualCountV54==3&&fx.ActiveHazardAudioCountV54==3,"visuals="+fx.ActiveHazardVisualCountV54+"; audio="+fx.ActiveHazardAudioCountV54,"NATIVE_EFFECT_STATE");
            Check("compound.all-active-types-present",fx.HasHazardVisualV54(DisasterType.Earthquake)&&fx.HasHazardVisualV54(DisasterType.Flood)&&fx.HasHazardVisualV54(DisasterType.Fire));
            yield return Wait(5.2f);yield return Capture("compound-quake-flood-fire","Same natural8s disaster phase: quake/flood/fire, shared severity; actual unsheltered F3 camera.");
            float until=Time.realtimeSinceStartup+10;
            while(w.Model.Phase!=PanicPhase.Inspection&&Time.realtimeSinceStartup<until)yield return null;
            Check("compound.once-supplies-no-dev-reward",w.Model.Phase==PanicPhase.Inspection&&w.Model.PackedWater==0&&w.Model.PackedFood==0&&w.Model.LastReward==0&&w.Model.LastWaveScore==0&&w.Model.CheatedRun);
            Check("compound.no-fake-shelter",!w.Model.ReachedShelterV5);
            yield return BeginNormal(DisasterType.Storm);w.SkipIntroStoryV414();yield return Wait(.12f);Compound(DisasterType.Storm,12);
            Check("compound.storm-fire-start",w.DeveloperSkipWaveV51());yield return Wait(5.4f);
            Check("compound.storm-fire-both-visible",fx.ActiveHazardVisualCountV54==2&&fx.HasHazardVisualV54(DisasterType.Storm)&&fx.HasHazardVisualV54(DisasterType.Fire));
            yield return Capture("compound-storm-fire","Both storm/fire play together in normal campaign mask, not simulation.");
            w.SkipResidenceCinematic();yield return Wait(.1f);
            Check("finish.runtime-errors",report.errors.Count==0,string.Join("\n",report.errors));
        }
        static bool PngSize(string path, out int width, out int height)
        {
            width = height = 0;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length < 2048) return false;
                byte[] bytes = File.ReadAllBytes(path);
                if (bytes.Length < 24 || bytes[0] != 137 || bytes[1] != 80 || bytes[2] != 78 || bytes[3] != 71 ||
                    bytes[bytes.Length - 8] != 73 || bytes[bytes.Length - 7] != 69 || bytes[bytes.Length - 6] != 78 || bytes[bytes.Length - 5] != 68) return false;
                width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
                height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
                return width > 0 && height > 0;
            }
            catch (IOException) { return false; }
        }
        static string Hash(string path)
        {
            if (!File.Exists(path)) return "absent";
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", "");
        }
        void Finish()
        {
            if (finished) return; finished = true;
            report.realScoreAfter = Hash(realScore);
            Check("scores.real-score-unchanged", report.realScoreBefore == report.realScoreAfter,
                "Before=" + report.realScoreBefore + "; after=" + report.realScoreAfter, "NATIVE_FILE_HASH");
            Check("scores.write-target-remains-isolated", ScoreIsIsolated(), PanicModel.ScorePathOverride, "NATIVE_FILE_PATH");
            report.seconds = Time.realtimeSinceStartup - began;
            report.passed = report.complete && report.errors.Count == 0 && report.checks.Count >= 40 &&
                 report.checks.All(row => row.status == "PASS");
            Application.logMessageReceived -= Log;
            if (!string.IsNullOrEmpty(w.EvidencePath)) File.WriteAllText(Path.Combine(w.EvidencePath, "v54-smoke.json"), JsonUtility.ToJson(report, true));
            Debug.Log("V54_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " seconds=" + report.seconds);
            Application.Quit(report.passed ? 0 : 2);
        }
    }
}

