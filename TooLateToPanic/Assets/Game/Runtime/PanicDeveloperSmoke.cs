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
    // Opt-in native API smoke. Awake must isolate ScorePathOverride before constructing any model.
    public sealed class PanicDeveloperSmoke : MonoBehaviour
    {
        [Serializable] sealed class Check { public string name, detail; public bool passed; }
        [Serializable] sealed class Report
        {
            public string startedUtc, finishedUtc, profileId, scorePath;
            public string evidenceType = "Focused native production API smoke with two actual rendered HUD captures and atomic writes to one fresh GUID score fixture.";
            public string limitations = "Automatic semantic F2/E/Tab input, not physical OS input or manual acceptance. Completed work is an explicit Work.Begin plus Work.Complete state fixture; the minigame is NOT accepted by this test. Inventory/budget, prior pause, disaster phase and completed-wave/score values are declared state fixtures. Clean score means a real production save of fixture values, not an earned campaign score. No doors, broad gameplay, long campaign or visual art acceptance.";
            public bool completed, passed;
            public float runtimeSeconds, menuClockBefore, menuClockAfter, preparationClockBefore, preparationClockAfter, disasterClockBefore, disasterClockAfter;
            public int completedWorkFixtures, localModelFixtures, playerSetupWarps;
            public string cleanScoreSha256, afterCheatScoreSha256;
            public List<Check> checks = new List<Check>();
            public List<string> fixtures = new List<string>(), screenshots = new List<string>(), errors = new List<string>();
        }
        const int ExpectedChecks = 12;
        const float HardCap = 24f;
        const string ReportName = "native-developer-smoke.json";
        static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        readonly Report report = new Report();
        PanicHomeWorld w;
        PanicHomeHud hud;
        Coroutine running;
        float started;
        bool finished;
        MethodInfo setPaused;
        FieldInfo paused;

        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--developer-smoke") < 0) { enabled = false; return; }
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>();
            if (!w || string.IsNullOrEmpty(w.EvidencePath))
            {
                Debug.LogError("DEVELOPER_SMOKE requires --home-evidence before Model construction.");
                enabled = false; Application.Quit(1); return;
            }
            started = Time.realtimeSinceStartup;
            report.startedUtc = DateTime.UtcNow.ToString("o");
            report.profileId = Path.GetFileName(Path.GetFullPath(w.EvidencePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            report.scorePath = PanicModel.ScorePathOverride;
            Application.logMessageReceived += Log; Application.runInBackground = true; w.InputLocked = true;
            setPaused = typeof(PanicHomeHud).GetMethod("SetPaused", PrivateInstance);
            paused = typeof(PanicHomeHud).GetField("_paused", PrivateInstance);
            running = StartCoroutine(Guarded(Run()));
        }

        IEnumerator Guarded(IEnumerator sequence)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(sequence);
            while (stack.Count > 0 && !finished)
            {
                bool next; object value = null;
                try { next = stack.Peek().MoveNext(); if (next) value = stack.Peek().Current; }
                catch (Exception ex) { report.errors.Add(ex.ToString()); break; }
                if (!next) { stack.Pop(); continue; }
                var nested = value as IEnumerator;
                if (nested != null) stack.Push(nested); else yield return value;
            }
            Finish();
        }

        IEnumerator Run()
        {
            string expected = Path.GetFullPath(Path.Combine(w.EvidencePath, "isolated-home-scores.json"));
            Guid guid;
            bool isolated = report.profileId.Length >= 36 && Guid.TryParse(report.profileId.Substring(report.profileId.Length - 36), out guid) &&
                !string.IsNullOrEmpty(report.scorePath) && string.Equals(expected, Path.GetFullPath(report.scorePath), StringComparison.OrdinalIgnoreCase) &&
                !File.Exists(expected) && !File.Exists(expected + ".tmp") && !File.Exists(Path.Combine(w.EvidencePath, ReportName)) &&
                !File.Exists(Path.Combine(w.EvidencePath, "developer-defaults-actual-render.png")) &&
                !File.Exists(Path.Combine(w.EvidencePath, "developer-all-enabled-actual-render.png")) && w.Model.Highscores.Count == 0;
            CheckIt("fresh GUID isolated score fixture and default cheats off", isolated && !w.Model.DeveloperAnyEnabled && !w.Model.CheatedRun,
                report.profileId + "; override=" + report.scorePath);
            if (!isolated) yield break;
            if (!hud || setPaused == null || paused == null) throw new InvalidOperationException("Missing attached production HUD or prior-pause fixture API.");

            w.HandleKey(KeyCode.F2);
            bool titleOpened = hud.DeveloperMenuOpen && w.InputLocked && !w.Model.CheatedRun && !w.Model.DeveloperAnyEnabled;
            yield return Shot("developer-defaults-actual-render");
            w.HandleKey(KeyCode.F2);
            CheckIt("F2 title open alone leaves run clean and restores prior lock", titleOpened && !hud.DeveloperMenuOpen && w.InputLocked && !w.Model.CheatedRun,
                "Actual attached HUD default panel captured; title fixture began locked.");

            w.StartGame(false); w.InputLocked = true;
            yield return null; yield return null; // Synchronize production HUD phase before opening.
            var source = w.Expedition.Sources.First(s => s.Id == "cabinet");
            w.Warp(source.Position + new Vector3(0, .05f, -1.2f)); report.playerSetupWarps++;
            w.SetFocus(source.Id); Physics.SyncTransforms();
            bool usableBefore = w.CanInteract, sourceBefore = source.Open;
            w.InputLocked = false; // No unlocked frame is yielded; production menu must acquire this lock.
            w.HandleKey(KeyCode.F2);
            report.menuClockBefore = w.Model.TimeRemaining;
            Vector3 position = w.PlayerPosition;
            w.HandleKey(KeyCode.E); w.HandleKey(KeyCode.Tab);
            yield return Hold(.18f);
            report.menuClockAfter = w.Model.TimeRemaining;
            bool menuBlocked = usableBefore && hud.DeveloperMenuOpen && w.InputLocked && !w.InventoryOpen && source.Open == sourceBefore &&
                Mathf.Abs(report.menuClockBefore - report.menuClockAfter) < .001f && Vector3.Distance(position, w.PlayerPosition) < .005f && !w.Model.CheatedRun;
            w.HandleKey(KeyCode.F2);
            bool restoredUnlocked = !hud.DeveloperMenuOpen && !w.InputLocked && !IsPaused() && hud.DeveloperInputBlockedThisFrame;
            w.HandleKey(KeyCode.E); w.HandleKey(KeyCode.Tab);
            bool transitionBlocked = !w.InventoryOpen && source.Open == sourceBefore;
            w.InputLocked = true;
            CheckIt("run panel pauses actual world clock/input; close restores lock and blocks same-frame E/Tab", menuBlocked && restoredUnlocked && transitionBlocked,
                "clock=" + report.menuClockBefore + " -> " + report.menuClockAfter + "; usable source=" + usableBefore + "; no unlocked yielded frame.");

            yield return null;
            // Private pause setter is used only to construct an existing-pause fixture.
            w.InputLocked = false; setPaused.Invoke(hud, new object[] { true });
            w.HandleKey(KeyCode.F2); bool priorPausedOpen = hud.DeveloperMenuOpen;
            w.HandleKey(KeyCode.F2); bool priorPauseRestored = !hud.DeveloperMenuOpen && IsPaused() && w.InputLocked;
            setPaused.Invoke(hud, new object[] { false }); bool priorLockRestored = !IsPaused() && !w.InputLocked;
            w.Expedition.Work.Begin("plant", "developer-busy-fixture");
            w.HandleKey(KeyCode.F2); bool busyDenied = !hud.DeveloperMenuOpen && w.WorkActive;
            w.Expedition.Work.Cancel(); w.ToggleInventory(); w.HandleKey(KeyCode.F2);
            bool inventoryDenied = w.InventoryOpen && !hud.DeveloperMenuOpen; w.ToggleInventory(); w.InputLocked = true;
            report.fixtures.Add("Prior pause constructed through private HUD.SetPaused; Work.Begin busy guard and inventory guard; all restored synchronously, no unlocked frame yielded.");
            CheckIt("prior pause/lock restored; work and inventory deny F2", priorPausedOpen && priorPauseRestored && priorLockRestored && busyDenied && inventoryDenied,
                "Pause and busy state fixtures only; work canceled before continuing.");

            yield return null;
            w.InputLocked = false; w.HandleKey(KeyCode.F2);
            SetAll(w.Model, true);
            bool selected = hud.DeveloperMenuOpen && w.Model.Developer.InfiniteBudget && w.Model.Developer.FreeMaterials && w.Model.Developer.FreezePreparationTime && w.Model.CheatedRun;
            yield return Shot("developer-all-enabled-actual-render");
            hud.SetDeveloperMenuOpen(false); w.InputLocked = true; w.StartGame(false); w.InputLocked = true;
            bool retained = w.Model.DeveloperAnyEnabled && w.Model.InfiniteBudget && w.Model.FreeMaterials && w.Model.FreezePreparationTime && w.Model.CheatedRun;
            SetAll(w.Model, false); w.ReturnToMenu(); w.InputLocked = true;
            CheckIt("all three selected in actual rendered HUD; process switches survive new run", selected && retained && !w.Model.DeveloperAnyEnabled && !w.Model.CheatedRun,
                "SetDeveloperOption API changes drive rendered selected states; no synthetic screenshot. Clean title restored after fixture.");

            Transactions();
            CraftAndPack();
            ClockFixture();
            ScoreFixture();
            report.completed = true;
        }

        void Transactions()
        {
            var m = Fresh(); var e = new PanicExpedition(m);
            m.Budget = 0; m.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, true);
            bool missingRefused = !e.CanBeginWork("plant", "infinite-missing");
            e.Inventory[0] = new SupplySlot("seedling", 1);
            bool infiniteWorked = Complete(e, "plant", "infinite-paid-material") && m.TreeCount == 1 && m.Budget == 0 && e.Count("seedling") == 0;
            CheckIt("infinite budget permits zero-budget work but still requires/consumes materials", missingRefused && infiniteWorked && m.InfiniteBudget && !m.FreeMaterials && !m.FreezePreparationTime,
                "Plant transaction committed; starting budget=0; real seedling 1 -> 0; completed-work fixture.");

            m = Fresh(); e = new PanicExpedition(m); m.Budget = 0; m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true);
            bool moneyRefused = !e.CanBeginWork("plant", "free-missing-budget");
            m.Budget = 100; string stock = Stock(e.Inventory);
            bool freeWorked = Complete(e, "plant", "free-paid-budget") && m.TreeCount == 1 && m.Budget == 70 && Stock(e.Inventory) == stock;
            CheckIt("free materials permits zero-stock work while budget charges independently", moneyRefused && freeWorked && m.FreeMaterials && !m.InfiniteBudget && !m.FreezePreparationTime,
                "Plant transaction committed with zero seedling; real budget 100 -> 70; completed-work fixture.");

            m = Fresh(); e = new PanicExpedition(m); m.Budget = 100;
            bool emptyRefused = !e.CanBeginWork("plant", "normal-empty");
            e.Inventory[0] = new SupplySlot("seedling", 1);
            bool normalWorked = Complete(e, "plant", "normal-paid") && m.Budget == 70 && e.Count("seedling") == 0 && m.TreeCount == 1 && !m.CheatedRun;
            bool repeatRefused = !e.CanBeginWork("plant", "normal-paid");
            int before = m.Budget; bool added = m.AddDeveloperBudget(1000);
            bool addTaints = added && m.Budget == before + 1000 && m.CheatedRun && !m.DeveloperAnyEnabled;
            CheckIt("normal work refuses missing stock and charges both; +1000 alone permanently taints", emptyRefused && normalWorked && repeatRefused && addTaints,
                "Normal plant: budget 100 -> 70 and seedling 1 -> 0; duplicate target rejected; one AddDeveloperBudget call.");
        }

        void CraftAndPack()
        {
            var m = Fresh(); var e = new PanicExpedition(m);
            bool normalCraftRefused = !e.CraftFirstAid();
            m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true);
            bool freeCraft = e.CraftFirstAid() && e.Count("firstaid") == 1 && e.Count("bandage") == 0 && e.Count("antiseptic") == 0;
            Fill(e.Inventory, "water", 5); string full = Stock(e.Inventory);
            bool fullRefused = !e.CraftFirstAid() && Stock(e.Inventory) == full && e.Inventory.Count == 12;
            CheckIt("free first aid crafts at zero materials but respects full 12-slot inventory", normalCraftRefused && freeCraft && fullRefused,
                "Inventory saturation is an explicit 12 real occupied-slot fixture, no capacity override.");

            m = Fresh(); e = new PanicExpedition(m); m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true);
            bool absentRefused = !e.PackSlot(0) && m.PackedSupportMask == 0;
            e.Inventory[0] = new SupplySlot("flashlight", 1); Fill(e.Bag, "water", 5); string bagBefore = Stock(e.Bag);
            bool fullBagRefused = !e.PackSlot(0) && e.Count("flashlight") == 1 && Stock(e.Bag) == bagBefore && m.PackedSupportMask == 0;
            Clear(e.Bag);
            bool freePacked = e.PackSlot(0) && e.Count("flashlight") == 0 && e.Bag.Any(s => s.Id == "flashlight" && s.Count == 1) && m.PackedSupportMask != 0;
            m = Fresh(); e = new PanicExpedition(m); e.Inventory[0] = new SupplySlot("flashlight", 1);
            bool normalBatteryRefused = !e.PackSlot(0) && e.Count("flashlight") == 1;
            e.Inventory[1] = new SupplySlot("battery", 1);
            bool normalPacked = e.PackSlot(0) && e.Count("flashlight") == 0 && e.Count("battery") == 0 && m.PackedSupportMask != 0;
            CheckIt("free battery packing still needs real flashlight/bag space; normal packing consumes battery", absentRefused && fullBagRefused && freePacked && normalBatteryRefused && normalPacked,
                "Explicit full 8-slot bag fixture; real flashlight consumed. Free mode supplies battery requirement only.");
        }

        void ClockFixture()
        {
            var m = Fresh(); m.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, true);
            report.preparationClockBefore = m.TimeRemaining; float phaseBefore = m.PhaseTime;
            m.Tick(2f); report.preparationClockAfter = m.TimeRemaining;
            bool frozen = m.Phase == PanicPhase.Preparing && m.TimeRemaining == report.preparationClockBefore && m.PhaseTime == phaseBefore && !m.InfiniteBudget && !m.FreeMaterials;
            m.Phase = PanicPhase.Disaster; m.PhaseTime = 10f;
            report.disasterClockBefore = m.PhaseTime; m.Tick(2f); report.disasterClockAfter = m.PhaseTime;
            bool disasterAdvanced = m.Phase == PanicPhase.Disaster && Mathf.Abs(m.PhaseTime - 8f) < .001f;
            m.Phase = PanicPhase.Preparing; m.TimeRemaining = 10f; m.PhaseTime = 10f;
            m.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, false); m.Tick(2f);
            bool normalAdvanced = Mathf.Abs(m.TimeRemaining - 8f) < .001f && m.CheatedRun && !m.DeveloperAnyEnabled;
            report.fixtures.Add("Local model phase fixture Disaster/PhaseTime=10, then Preparing/TimeRemaining=10; Tick(2) production clock only, no world phase transition acceptance.");
            CheckIt("freeze affects preparation only; disaster advances and disabling resumes preparation", frozen && disasterAdvanced && normalAdvanced,
                "Prep=" + report.preparationClockBefore + " -> " + report.preparationClockAfter + "; Disaster=" + report.disasterClockBefore + " -> " + report.disasterClockAfter + "; disabled prep=8.");
        }

        void ScoreFixture()
        {
            if (File.Exists(report.scorePath)) throw new InvalidOperationException("Score fixture must remain absent until clean-save check.");
            var m = Fresh(); m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true); m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, false);
            bool sticky = m.CheatedRun && !m.DeveloperAnyEnabled;
            Timeout(m, 4, 9000);
            bool excludedOnFailure = m.Phase == PanicPhase.GameOver && m.Highscores.Count == 0 && !File.Exists(report.scorePath);
            m.ReturnToTitle(); bool excludedOnTitle = m.Highscores.Count == 0 && !File.Exists(report.scorePath);
            m.StartRun(false); bool cleanNewRun = !m.CheatedRun && !m.DeveloperAnyEnabled;
            Timeout(m, 1, 1234); bool cleanWritten = m.Phase == PanicPhase.GameOver && m.Highscores.Count == 1 && File.Exists(report.scorePath);
            m.ReturnToTitle();
            report.cleanScoreSha256 = Hash(report.scorePath);
            m = Fresh(); bool reloaded = m.Highscores.Count == 1 && m.Highscores[0].score == 1234 && m.Highscores[0].waves == 1;
            m.AddDeveloperBudget(1000); Timeout(m, 9, 99999); m.ReturnToTitle();
            report.afterCheatScoreSha256 = Hash(report.scorePath);
            bool preserved = m.Highscores.Count == 1 && report.cleanScoreSha256 == report.afterCheatScoreSha256;
            CheckIt("sticky cheated run excluded after all off; clean new run saves; later cheat preserves file hash", sticky && excludedOnFailure && excludedOnTitle && cleanNewRun && cleanWritten && reloaded && preserved,
                "CompletedWaves/Score fixtures 4/9000 excluded, 1/1234 saved/reloaded, 9/99999 excluded; SHA256=" + report.cleanScoreSha256);
        }

        PanicModel Fresh()
        {
            if (!string.Equals(Path.GetFullPath(PanicModel.ScorePathOverride), Path.GetFullPath(report.scorePath), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Refusing local model construction after score path isolation changed.");
            report.localModelFixtures++;
            var model = new PanicModel(); model.StartRun(false); return model;
        }
        bool Complete(PanicExpedition e, string action, string target)
        {
            if (!e.CanBeginWork(action, target)) return false;
            e.Work.Begin(action, target); e.Work.Complete = true; report.completedWorkFixtures++;
            report.fixtures.Add("Completed-work STATE fixture: Work.Begin(" + action + ", " + target + "); Work.Complete=true; production TryCompleteWork commits costs/target. No minigame acceptance.");
            bool result = e.TryCompleteWork(action, target);
            return result && !e.Work.Active && !e.TryCompleteWork(action, target);
        }
        void Timeout(PanicModel m, int waves, long score)
        {
            m.CompletedWaves = waves; m.Score = score; m.TimeRemaining = .01f; m.PhaseTime = .01f;
            report.fixtures.Add("Score STATE fixture CompletedWaves=" + waves + ", Score=" + score + ", preparation time=.01; production Tick(.02) calls timeout FailRun; ReturnToTitle is called separately.");
            m.Tick(.02f);
        }
        static void SetAll(PanicModel m, bool value)
        {
            m.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, value);
            m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, value);
            m.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, value);
        }
        static void Fill(List<SupplySlot> slots, string id, int count) { for (int i = 0; i < slots.Count; i++) slots[i] = new SupplySlot(id, count); }
        static void Clear(List<SupplySlot> slots) { for (int i = 0; i < slots.Count; i++) slots[i] = new SupplySlot(); }
        static string Stock(List<SupplySlot> slots) { return string.Join("|", slots.Select(s => s.Id + ":" + s.Count).ToArray()); }
        bool IsPaused() { return (bool)paused.GetValue(hud); }
        static string Hash(string path)
        {
            if (!File.Exists(path)) return "MISSING";
            using (var sha = SHA256.Create()) using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }
        IEnumerator Hold(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
        IEnumerator Shot(string id)
        {
            yield return new WaitForEndOfFrame(); string path = Path.Combine(w.EvidencePath, id + ".png");
            ScreenCapture.CaptureScreenshot(path); report.screenshots.Add(path);
            long previous = -1; int stable = 0; float deadline = Time.realtimeSinceStartup + 1.5f;
            while (Time.realtimeSinceStartup < deadline)
            {
                long size = File.Exists(path) ? new FileInfo(path).Length : 0;
                stable = size >= 256 && size == previous ? stable + 1 : 0;
                if (stable >= 2) yield break;
                previous = size; yield return null;
            }
            report.errors.Add("Actual screenshot did not finish writing: " + path);
        }
        void CheckIt(string name, bool passed, string detail)
        {
            report.checks.Add(new Check { name = name, passed = passed, detail = detail });
            Debug.Log("DEVELOPER_CHECK " + (passed ? "PASS " : "FAIL ") + name + " " + detail);
        }
        void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack);
        }
        void Update()
        {
            if (finished || !w || Time.realtimeSinceStartup - started <= HardCap) return;
            report.errors.Add("Focused developer smoke exceeded 24 second cap; unfinished checks are not accepted.");
            if (running != null) StopCoroutine(running); Finish();
        }
        void Finish()
        {
            if (finished) return; finished = true;
            if (w) w.InputLocked = true;
            report.finishedUtc = DateTime.UtcNow.ToString("o"); report.runtimeSeconds = Time.realtimeSinceStartup - started;
            foreach (string path in report.screenshots) if (!File.Exists(path) || new FileInfo(path).Length < 256) report.errors.Add("Missing/empty actual render: " + path);
            report.passed = report.completed && report.errors.Count == 0 && report.checks.Count == ExpectedChecks && report.checks.All(c => c.passed) && report.screenshots.Count == 2;
            Application.logMessageReceived -= Log;
            try { File.WriteAllText(Path.Combine(w.EvidencePath, ReportName), JsonUtility.ToJson(report, true)); }
            catch (Exception ex) { Debug.LogError("DEVELOPER_SMOKE report write failed: " + ex); report.passed = false; }
            Debug.Log("DEVELOPER_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " errors=" + report.errors.Count + " seconds=" + report.runtimeSeconds.ToString("F2"));
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}
