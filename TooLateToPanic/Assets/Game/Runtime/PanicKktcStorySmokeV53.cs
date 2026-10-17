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
    public sealed class PanicKktcStorySmokeV53 : MonoBehaviour
    {
        const string Flag = "--v53-smoke";
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
            public string evidence = "Native production API and live graphical ScreenCapture framebuffer files. Two ordinary-frame 16-second openings; explicit hazard assignments and one separate notebook model fixture. No active-world clock/age assignment, manual Model.Tick, direct Warp or Camera.Render. No OS input, human acceptance, listening or full campaign acceptance.";
            public string captureLimit = "PNG presence and dimensions prove framebuffer capture only. Text fit, legibility and final visual acceptance require owner inspection of the captured pixels.";
            public bool complete, passed, osInput, humanAcceptance, listening;
            public float seconds;
            public int productionStartGameCalls, productionJuryStartCalls, hazardAssignments, separateModelFixtures;
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
            if (world) world.gameObject.AddComponent<PanicKktcStorySmokeV53>();
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
            if (!finished && Time.realtimeSinceStartup - began > 65f)
            { report.error = "65-second story smoke hard cap reached"; Finish(); }
        }
        void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                report.errors.Add(message + "\n" + stack);
        }
        void Check(string id, bool pass, string detail = "", string layer = "NATIVE_API_FRAME")
        {
            report.checks.Add(new Row { id = id, status = pass ? "PASS" : "FAIL", detail = detail, layer = layer, frame = Time.frameCount });
            Debug.Log("V53_CHECK " + (pass ? "PASS " : "FAIL ") + id + " " + detail);
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
        IEnumerator NaturalOpening(DisasterType hazard)
        {
            yield return BeginNormal(hazard);
            float clock = w.Model.TimeRemaining, start = Time.realtimeSinceStartup;
            Vector3 actor = w.PlayerPosition;
            int resolutions = w.IntroStoryResolutionCountV414, nextBeat = 0;
            var trace = new Timeline { hazard = hazard.ToString() }; report.timelines.Add(trace);
            bool controllerHeld = true;
            while (w.IntroStoryActiveV414 && Time.realtimeSinceStartup - start < 23f)
            {
                float age = StoryAge;
                trace.lastObservedAge = Mathf.Max(trace.lastObservedAge, age); trace.observedFrames++;
                trace.maxClockDrift = Mathf.Max(trace.maxClockDrift, Mathf.Abs(w.Model.TimeRemaining - clock));
                trace.maxActorDrift = Mathf.Max(trace.maxActorDrift, Vector3.Distance(w.PlayerPosition, actor));
                controllerHeld &= !w.PlayerControllerEnabled && w.Model.Phase == PanicPhase.Preparing;
                if (nextBeat < 4 && age >= nextBeat * 4f + .65f)
                {
                    int beat = nextBeat++; trace.observedBeats++;
                    string expected = PanicKktcStoryV53.OpeningCaption(age, hazard == DisasterType.Flood);
                    Check("opening.beat." + hazard + "." + beat,
                        age < beat * 4f + 3.8f && w.IntroStoryTextV414 == expected && w.CinematicTitle == expected,
                        "Natural age=" + age.ToString("F3") + "; " + w.IntroStoryTextV414);
                    yield return Capture("opening-" + hazard.ToString().ToLowerInvariant() + "-beat" + beat,
                        "Natural Opening beat=" + beat + "; age=" + age.ToString("F3") + "; text=" + w.IntroStoryTextV414);
                }
                yield return null;
            }
            trace.wallSeconds = Time.realtimeSinceStartup - start; trace.naturalExit = !w.IntroStoryActiveV414;
            Check("opening.clock-actor-held." + hazard,
                controllerHeld && trace.observedFrames > 100 && trace.maxClockDrift < .005f && trace.maxActorDrift < .005f,
                "frames=" + trace.observedFrames + "; clock drift=" + trace.maxClockDrift + "; actor drift=" + trace.maxActorDrift);
            Check("opening.sixteen-seconds-four-beats." + hazard,
                trace.observedBeats == 4 && trace.lastObservedAge >= 15.85f && trace.wallSeconds >= 15.5f && trace.wallSeconds < 23f,
                "Observed last age=" + trace.lastObservedAge + "; wall=" + trace.wallSeconds + "; no age/Tick assignment.");
            Check("opening.natural-resolves-once." + hazard,
                trace.naturalExit && !w.CinematicActive && w.IntroStoryResolutionCountV414 == resolutions + 1,
                "Before=" + resolutions + "; after=" + w.IntroStoryResolutionCountV414);
            float resumed = w.Model.TimeRemaining;
            yield return Wait(1f);
            Check("opening.control-and-clock-resume." + hazard,
                w.PlayerControllerEnabled && !w.InputLocked && !w.CinematicActive && w.Model.TimeRemaining < resumed - .05f &&
                w.IntroStoryResolutionCountV414 == resolutions + 1 && w.Model.TimeRemaining > PanicModel.NormalBagSeconds,
                "Original controller and ordinary preparation clock resume once.");
        }
        void CatalogChecks()
        {
            string[] copy = {
                PanicKktcStoryV53.MenuLocation, PanicKktcStoryV53.MenuPremise, PanicKktcStoryV53.LocationBadge,
                PanicKktcStoryV53.FictionNotice, PanicKktcStoryV53.DiaryTitle, PanicKktcStoryV53.DiaryPersonalNote,
                PanicKktcStoryV53.OpeningPlaceCaption(), PanicKktcStoryV53.OpeningPreparationCaption(),
                PanicKktcStoryV53.OpeningBagCaption(), PanicKktcStoryV53.OpeningShelterCaption(false),
                PanicKktcStoryV53.OpeningShelterCaption(true), PanicKktcStoryV53.ShortFlowPremise()
            };
            Check("story.kktc-guzelyurt-kalkanli-fiction", PanicKktcStoryV53.MenuLocation.Contains("KKTC") &&
                PanicKktcStoryV53.MenuLocation.Contains("Güzelyurt") && PanicKktcStoryV53.MenuLocation.Contains("Kalkanlı") && PanicKktcStoryV53.FictionNotice.Contains("kurmaca"),
                PanicKktcStoryV53.MenuLocation + "; " + PanicKktcStoryV53.FictionNotice, "NATIVE_CATALOG_API");
            Check("story.family-premise-no-vadidere", PanicKktcStoryV53.MenuPremise.Contains("Ailen güvenli") &&
                copy.All(s => !string.IsNullOrWhiteSpace(s) && s.IndexOf("Vadidere", StringComparison.OrdinalIgnoreCase) < 0),
                "Checks catalog strings used by opening/menu/diary/short-flow; source scan remains separate.", "NATIVE_CATALOG_API");
            Check("story.model-timings-and-supplies", PanicModel.NormalPrecautionSeconds == 120f && PanicModel.NormalBagSeconds == 45f &&
                PanicKktcStoryV53.OpeningPreparationCaption().Contains("120") && PanicKktcStoryV53.OpeningBagCaption().Contains("45") &&
                PanicKktcStoryV53.OpeningBagCaption().Contains("Bir su ve bir gıda"), "Existing model constants feed actual copy.", "NATIVE_CATALOG_API");
            Check("story.shelter-copy-two-routes", PanicKktcStoryV53.OpeningShelterCaption(true).Contains("üst kat panik odası") &&
                !PanicKktcStoryV53.OpeningShelterCaption(true).Contains("bodrum") && PanicKktcStoryV53.OpeningShelterCaption(false).Contains("bodrum sığınağı"),
                "Flood route is upper room; other campaign route is basement.", "NATIVE_CATALOG_API");
        }
        bool LiveCounts(PanicNotebookSnapshot data)
        {
            if (data == null || data.Materials == null || data.Inventory == null || data.Bag == null) return false;
            if (data.Budget != (w.Model.InfiniteBudget ? "SINIRSIZ" : w.Model.Budget.ToString("N0")) ||
                data.PackedWater != w.Model.PackedWater || data.PackedFood != w.Model.PackedFood ||
                data.RequiredDays != w.Model.RequiredDays || data.BagReady != w.Model.BagReady ||
                data.Inventory.Length != w.Expedition.Inventory.Count || data.Bag.Length != w.Expedition.Bag.Count) return false;
            for (int i = 0; i < PanicNotebookData.MaterialIds.Length; i++)
                if (data.Materials[i] != w.Expedition.Count(PanicNotebookData.MaterialIds[i])) return false;
            for (int i = 0; i < data.Inventory.Length; i++)
                if (data.Inventory[i].Count != w.Expedition.Inventory[i].Count || data.Inventory[i].Id != w.Expedition.Inventory[i].Id) return false;
            for (int i = 0; i < data.Bag.Length; i++)
                if (data.Bag[i].Count != w.Expedition.Bag[i].Count || data.Bag[i].Id != w.Expedition.Bag[i].Id) return false;
            return true;
        }
        IEnumerator Notebook()
        {
            string before = PanicNotebookVerification.GameplayFingerprint(w); float clock = w.Model.TimeRemaining;
            Check("notebook.opens-production", w.Notebook && w.Notebook.RequestOpen());
            yield return Wait(.5f); w.Notebook.RefreshSnapshot();
            Check("notebook.story-and-live-note-separate", w.Notebook.Snapshot.StoryNote == PanicKktcStoryV53.DiaryPersonalNote &&
                w.Notebook.Snapshot.StoryLocation == PanicKktcStoryV53.LocationBadge && w.Notebook.Snapshot.StoryTitle == PanicKktcStoryV53.DiaryTitle &&
                w.Notebook.Snapshot.Note != w.Notebook.Snapshot.StoryNote, "Story fields and dynamic Note retain separate owners.");
            Check("notebook.live-model-expedition-counts", LiveCounts(w.Notebook.Snapshot), "Live storage slots, materials, budget and packed supplies.");
            Check("notebook.real-capacities", w.Notebook.Snapshot.InventoryCapacity == w.Expedition.Inventory.Count &&
                w.Notebook.Snapshot.BagCapacity == w.Expedition.Bag.Count && w.Expedition.Inventory.Count == 12 && w.Expedition.Bag.Count == 8);
            yield return Capture("notebook-rooms-1600", "Production notebook Rooms; personal story footer plus real live state.");
            w.Notebook.RequestPage(PanicNotebookPage.Inventory); yield return Wait(.9f);
            Check("notebook.inventory-page", !w.Notebook.Animation.Turning && w.Notebook.Animation.CurrentPage == PanicNotebookPage.Inventory && LiveCounts(w.Notebook.Snapshot));
            yield return Capture("notebook-inventory-1600", "Production Inventory page; counts are Model/Expedition values.");
            yield return Resolution(960, 540);
            yield return Capture("notebook-inventory-960", "Same production Inventory page at actual 960x540 framebuffer.");
            w.Notebook.RequestPage(PanicNotebookPage.Flood); yield return Wait(.9f);
            Check("notebook.flood-page", !w.Notebook.Animation.Turning && w.Notebook.Animation.CurrentPage == PanicNotebookPage.Flood && LiveCounts(w.Notebook.Snapshot));
            yield return Capture("notebook-flood-960", "Production Flood page; story footer and real preparation checks.");
            Check("notebook.readonly-and-clock-held", PanicNotebookVerification.GameplayFingerprint(w) == before &&
                Mathf.Abs(w.Model.TimeRemaining - clock) < .005f && w.NotebookBlocksInput, "Navigation did not mutate stock/budget/score/cheat state.");
            w.Notebook.RequestClose(); yield return Wait(.55f);
            Check("notebook.releases-own-gate", !w.Notebook.IsVisible && !w.NotebookBlocksInput && w.PlayerControllerEnabled && !w.InputLocked);
        }
        void NotebookModelFixture()
        {
            report.separateModelFixtures++;
            var model = new PanicModel(); model.StartRun(false); var expedition = new PanicExpedition(model);
            model.Disaster = DisasterType.Flood; model.TimeRemaining = PanicModel.NormalBagSeconds;
            model.Budget = 1234; model.PackedWater = 2; model.PackedFood = 1;
            expedition.Inventory[0] = new SupplySlot("firstaid", 2); expedition.SyncConsumedBagSupplies();
            var flood = PanicNotebookData.Capture(model, expedition, null, 1);
            Check("notebook.fixture-flood-ready-upper-room", flood.BagReady && flood.Note.IndexOf("üst kat panik odası", StringComparison.OrdinalIgnoreCase) >= 0 &&
                flood.Note.IndexOf("bodrum", StringComparison.OrdinalIgnoreCase) < 0,
                "Separate model fixture only: Flood, final45s, water2/food1; " + flood.Note, "NATIVE_MODEL_FIXTURE");
            Check("notebook.fixture-nonzero-live-counts", flood.Budget == model.Budget.ToString("N0") && flood.PackedWater == 2 && flood.PackedFood == 1 &&
                flood.Inventory[0].Id == "firstaid" && flood.Inventory[0].Count == 2 && flood.Bag[0].Count > 0,
                "Positive fixture values rule out static demonstration counts.", "NATIVE_MODEL_FIXTURE");
            model.Disaster = DisasterType.Earthquake;
            var quake = PanicNotebookData.Capture(model, expedition, null, 0);
            Check("notebook.fixture-other-ready-basement", quake.Note.IndexOf("bodrum", StringComparison.OrdinalIgnoreCase) >= 0 && quake.BagReady,
                "Separate model fixture; " + quake.Note, "NATIVE_MODEL_FIXTURE");
        }
        IEnumerator Run()
        {
            yield return Wait(.35f);
            Check("launch.driver-registered", PanicTestLaunchRules.Drivers.Contains(Flag));
            string profile = Path.GetFileName((w.EvidencePath ?? "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)); Guid guid;
            Check("launch.fresh-guid-score-isolated", profile.Length >= 36 && Guid.TryParseExact(profile.Substring(profile.Length - 36), "D", out guid) && ScoreIsIsolated());
            Check("launch.live-graphical-background", !Application.isBatchMode && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null && Application.runInBackground,
                "Graphics=" + SystemInfo.graphicsDeviceType + "; background=" + Application.runInBackground, "NATIVE_GRAPHICAL_SURFACE");
            if (!PanicTestLaunchRules.Drivers.Contains(Flag) || !ScoreIsIsolated() || profile.Length < 36 ||
                !Guid.TryParseExact(profile.Substring(profile.Length - 36), "D", out guid)) yield break;
            CatalogChecks();
            w.ReturnToMenu();
            yield return Resolution(1600, 900);
            yield return Capture("title-1600", "Production title; KKTC/Zeytinova location and family premise require pixel review.");
            yield return Resolution(960, 540);
            yield return Capture("title-960", "Production title at actual 960x540; pixel review required.");
            yield return Resolution(1600, 900);
            yield return NaturalOpening(DisasterType.Earthquake);
            yield return NaturalOpening(DisasterType.Flood);
            yield return BeginNormal(DisasterType.Fire);
            int before = w.IntroStoryResolutionCountV414;
            w.SkipIntroStoryV414(); w.SkipIntroStoryV414();
            Check("opening.explicit-skip-idempotent-same-frame", !w.IntroStoryActiveV414 && w.IntroStoryResolutionCountV414 == before + 1 && w.CancelFrame == Time.frameCount,
                "Two production skip calls resolve once and consume the same input frame.");
            yield return Wait(.25f); w.SkipIntroStoryV414();
            Check("opening.explicit-skip-idempotent-later", !w.CinematicActive && w.PlayerControllerEnabled && !w.InputLocked &&
                w.IntroStoryResolutionCountV414 == before + 1 && w.Model.TimeRemaining > 164f);
            yield return Notebook();
            NotebookModelFixture();
            w.ReturnToMenu(); w.StartJuryDemo(); report.productionJuryStartCalls++; w.FullFlowScriptedInput = true; yield return null;
            Check("shortflow.localized-heading-retains-movement", w.JurySnapshot != null && w.JurySnapshot.Heading == PanicKktcStoryV53.OpeningTitle &&
                w.JurySnapshot.Hint.Contains("WASD") && w.JurySnapshot.Hint.Contains("45") &&
                w.JurySnapshot.Hint.IndexOf("Vadidere", StringComparison.OrdinalIgnoreCase) < 0,
                w.JurySnapshot == null ? "Missing snapshot" : w.JurySnapshot.Heading + "; " + w.JurySnapshot.Hint);
            Check("finish.runtime-errors", report.errors.Count == 0, string.Join("\n", report.errors));
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
                report.checks.Count <= 70 && report.checks.All(row => row.status == "PASS");
            Application.logMessageReceived -= Log;
            if (!string.IsNullOrEmpty(w.EvidencePath)) File.WriteAllText(Path.Combine(w.EvidencePath, "v53-smoke.json"), JsonUtility.ToJson(report, true));
            Debug.Log("V53_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " seconds=" + report.seconds);
            Application.Quit(report.passed ? 0 : 2);
        }
    }
}
