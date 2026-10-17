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
    /// <summary>
    /// Opt-in HUD/state-fixture smoke. World must isolate ScorePathOverride BEFORE constructing Model.
    /// No production rule is weakened: setup assignments are recorded and work is cancelled, never auto-completed.
    /// </summary>
    public sealed class PanicHudConceptSmoke : MonoBehaviour
    {
        const string Flag = "--hud-concept-smoke";
        const string ReportName = "native-hud-concept-smoke.json";
        const float HardCap = 240f;
        static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] sealed class Check { public string name, detail; public bool passed; }
        [Serializable] sealed class Capture
        {
            public string id, path, phase, workMode, overlapBasis;
            public int requestedWidth, requestedHeight, width, height, markedRects, outsideRects, dominantOverlaps, dominantRegions;
            public float scale, offsetX, offsetY, canvasWidth, canvasHeight;
            public bool canvasFits, passed;
            public List<string> layoutIssues = new List<string>();
        }
        [Serializable] sealed class Report
        {
            public string startedUtc, finishedUtc, profileId, scorePath, realScorePath, realScoreBefore, realScoreAfter, themeVersion;
            public string evidenceType = "Native production HUD rendered to actual framebuffer; explicit state fixtures plus focused production API invariants.";
            public string limitations = "Not a full campaign, earned score, physical OS input, minigame completion, door/collision or final art acceptance. Screenshot states use explicit inventory/budget/time/phase/damage fixtures. Disaster cue fixtures bypass the cinematic via private ResetResidenceCinematic and previousPhase synchronization; production weather API is called. Actual deadline/loss sequence is observed separately. Bounds cover retained HUD marked rectangles and logical canvas; containment and fullscreen overlays are intentional. Internal glyph clipping, arrow/check/diamond symbols and visual quality require human screenshot review. Dominant overlap checks prefer explicit top-level HUD regions, falling back to marked rectangles if no regions are registered.";
            public bool completed, passed;
            public float runtimeSeconds, inventoryClockBefore, inventoryClockAfter, menuClockBefore, menuClockAfter;
            public int setupWarps, cancelledWorkFixtures, plannedCaptures;
            public List<Check> checks = new List<Check>();
            public List<Capture> captures = new List<Capture>();
            public List<string> screenshots = new List<string>(), fixtures = new List<string>(), fonts = new List<string>(), errors = new List<string>();
        }

        readonly Report report = new Report();
        PanicHomeWorld w;
        PanicHomeHud hud;
        PanicHomeEffects effects;
        MethodInfo setPaused, synchronize, resetCinema;
        FieldInfo paused, scaleField, offsetField, rectsField, priorPhase;
        Coroutine running;
        float started;
        bool finished, quitRequested;

        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), Flag) < 0) { enabled = false; return; }
            started = Time.realtimeSinceStartup;
            report.startedUtc = DateTime.UtcNow.ToString("o");
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); effects = GetComponent<PanicHomeEffects>();
            if (!w || !hud || string.IsNullOrEmpty(w.EvidencePath))
            {
                Debug.LogError("HUD_CONCEPT_SMOKE requires attached production World/HUD and pre-Model --home-evidence isolation.");
                Quit(1); enabled = false; return;
            }
            report.profileId = Path.GetFileName(Path.GetFullPath(w.EvidencePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            report.scorePath = PanicModel.ScorePathOverride;
            report.realScorePath = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.realScoreBefore = Hash(report.realScorePath);
            setPaused = typeof(PanicHomeHud).GetMethod("SetPaused", Private);
            synchronize = typeof(PanicHomeHud).GetMethod("SynchronizeHudPhase", Private);
            resetCinema = typeof(PanicHomeWorld).GetMethod("ResetResidenceCinematic", Private);
            paused = typeof(PanicHomeHud).GetField("_paused", Private);
            scaleField = typeof(PanicHomeHud).GetField("_scale", Private);
            offsetField = typeof(PanicHomeHud).GetField("_offset", Private);
            rectsField = typeof(PanicHomeHud).GetField("_uiRects", Private);
            priorPhase = typeof(PanicHomeWorld).GetField("previousPhase", Private);
            Application.logMessageReceived += Log;
            Application.runInBackground = true;
            w.InputLocked = true;
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
            Guid guid;
            string expected = Path.GetFullPath(Path.Combine(w.EvidencePath, "isolated-home-scores.json"));
            bool fresh = report.profileId.Length >= 36 && Guid.TryParse(report.profileId.Substring(report.profileId.Length - 36), out guid) &&
                !string.IsNullOrEmpty(report.scorePath) && string.Equals(expected, Path.GetFullPath(report.scorePath), StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(Path.GetFullPath(report.realScorePath), expected, StringComparison.OrdinalIgnoreCase) &&
                !File.Exists(expected) && !File.Exists(expected + ".tmp") && !File.Exists(Path.Combine(w.EvidencePath, ReportName)) &&
                Directory.GetFiles(w.EvidencePath, "hud-*.png").Length == 0 && w.Model.Highscores.Count == 0;
            CheckIt("fresh GUID profile isolated before model construction", fresh && !w.Model.CheatedRun && !w.Model.DeveloperAnyEnabled,
                "profile=" + report.profileId + "; override=" + report.scorePath + "; real save read/hash only.");
            if (!fresh) yield break;
            if (setPaused == null || synchronize == null || resetCinema == null || paused == null || scaleField == null || offsetField == null || rectsField == null || priorPhase == null)
                throw new InvalidOperationException("Required existing private HUD layout/phase fixture fields missing; no substitute public API is assumed.");

            int[,] sizes = { {1600,900}, {1280,720}, {1024,768}, {1920,1080}, {960,540} };
            for (int n = 0; n < sizes.GetLength(0); n++)
            {
                int width = sizes[n, 0], height = sizes[n, 1];
                w.ReturnToMenu(); w.InputLocked = true; SyncHud();
                yield return Shot("title", width, height);
                if (n == 0) VerifyFonts();
                NewRun();
                yield return Shot("preparation", width, height);
                w.ToggleInventory();
                SeedInventory(); w.InventorySelect(0);
                yield return Shot("inventory", width, height);
                w.ToggleInventory();
                w.BeginWork("anchorwall", "bookshelf");
                if (!w.WorkActive) throw new InvalidOperationException("Cannot create funded Ratchet work fixture through World.BeginWork.");
                yield return Shot("work-ratchet", width, height);
                w.CancelWork(); report.cancelledWorkFixtures++;
                yield return null;
                w.InputLocked = false; hud.SetDeveloperMenuOpen(true);
                bool open = hud.DeveloperMenuOpen;
                yield return Shot("developer", width, height);
                hud.SetDeveloperMenuOpen(false); w.InputLocked = true;
                CheckIt("developer opens clean at " + width + "x" + height, open && !w.Model.CheatedRun && !w.Model.DeveloperAnyEnabled,
                    "Opening and rendering use production menu API; options never enabled.");
            }

            yield return Resize(1600, 900);
            NewRun();
            bool normalTimer = !ReadBooleanProperty("TimerAlarmActive");
            w.Model.TimeRemaining = w.Model.PhaseTime = 59f;
            report.fixtures.Add("World model preparation clock assigned 59 seconds for last-minute HUD; InputLocked holds fixture during render.");
            yield return Shot("last-60-seconds", 1600, 900);
            CheckIt("timer alarm reads actual preparation clock", normalTimer && ReadBooleanProperty("TimerAlarmActive"),
                "Normal production run is not alarmed; explicit59-second fixture activates concept alarm presentation.");
            yield return RequirementsAndLoot();
            yield return InventoryAndPacking();
            yield return WorkFamilies();
            yield return PauseAndDeveloper();
            yield return DisasterAndResults();
            report.realScoreAfter = Hash(report.realScorePath);
            CheckIt("real personal highscore unchanged", report.realScoreBefore == report.realScoreAfter &&
                string.Equals(Path.GetFullPath(PanicModel.ScorePathOverride), expected, StringComparison.OrdinalIgnoreCase),
                "before=" + report.realScoreBefore + "; after=" + report.realScoreAfter + "; isolated score path remains active.");
            CheckIt("normal options and clean run preserved", !w.Model.DeveloperAnyEnabled && !w.Model.CheatedRun,
                "No developer mutation API was invoked; state fixtures never enable cheats.");
            report.completed = true;
        }

        IEnumerator RequirementsAndLoot()
        {
            NewRun(); Clear(w.Expedition.Inventory);
            int budget = w.Model.Budget;
            bool refused = !w.Expedition.CanBeginWork("fence", "hud-fence-fixture");
            var r = w.Expedition.CurrentRequirements;
            bool exact = Required(r, "plank", 3, 0, 3) && Required(r, "screws", 2, 0, 2) && Required(r, "sandbag", 1, 0, 1);
            yield return Shot("requirements-three-missing", 1600, 900);
            w.Expedition.Inventory[0] = new SupplySlot("plank", 2);
            w.Expedition.Inventory[1] = new SupplySlot("screws", 2);
            r = w.Expedition.CurrentRequirements;
            bool updated = Required(r, "plank", 3, 2, 1) && Required(r, "screws", 2, 2, 0) && Required(r, "sandbag", 1, 0, 1);
            yield return Shot("requirements-dynamic-exact-counts", 1600, 900);
            CheckIt("requirements remain live exact recipe counts without spending", refused && exact && updated && hud.RequirementsCardVisible && w.Model.Budget == budget,
                "Fence fixture: plank 0/3->2/3, screws 0/2->2/2, sandbag 0/1; real CurrentRequirements drives the rendered card.");
            w.Model.Budget = 0;
            yield return Shot("requirements-budget-and-materials", 1024, 768);
            CheckIt("budget shortage is exact and normal costs retained", w.Expedition.RequirementBudgetCost == 60 && w.Expedition.RequirementBudgetMissing == 60 && !w.Expedition.CanBeginWork("fence", "hud-fence-fixture"),
                "Budget=0 explicit fixture; normal fence cost remains 60.");
            w.ToggleInventory();
            yield return Shot("inventory-with-requirements", 1024, 768);
            w.ToggleInventory();
            NewRun();
            var source = w.Expedition.Sources.First(s => s.Id == "cabinet");
            w.Warp(source.Position + new Vector3(0f, .05f, -1.2f)); report.setupWarps++; w.SetFocus(source.Id); Physics.SyncTransforms();
            bool nearby = w.CanInteract;
            w.ToggleContainer();
            yield return Shot("loot-open", 1600, 900);
            int selected = w.LootIndex;
            string selectedId = w.SelectedLootId;
            int sourceBefore = selected >= 0 && selected < source.Contents.Count ? source.Contents[selected].Count : 0;
            int ownedBefore = w.Expedition.Count(selectedId);
            int lootBudget = w.Model.Budget;
            bool taken = w.TakeLoot();
            yield return Shot("loot-after-real-take", 1280, 720);
            CheckIt("actual selected loot transfers without budget change", nearby && taken && !string.IsNullOrEmpty(selectedId) &&
                sourceBefore > 0 && w.Expedition.Count(selectedId) == ownedBefore + sourceBefore && source.Contents[selected].Count == 0 && w.Model.Budget == lootBudget,
                "One setup Warp, production ToggleContainer/TakeLoot transfers complete stack; selected=" + selectedId + "; source " + sourceBefore + "->" + source.Contents[selected].Count);
            foreach (var slot in source.Contents) slot.Count = 0;
            report.fixtures.Add("Open cabinet remaining source counts assigned zero solely to render the explicit empty-loot state.");
            yield return Shot("loot-empty", 1024, 768);
            w.ToggleContainer();
        }

        IEnumerator InventoryAndPacking()
        {
            NewRun(); Clear(w.Expedition.Inventory); Clear(w.Expedition.Bag);
            w.Expedition.Inventory[0] = new SupplySlot("water", 2);
            w.Expedition.Inventory[1] = new SupplySlot("food", 2);
            w.Expedition.Inventory[2] = new SupplySlot("flashlight", 1);
            w.Expedition.Inventory[3] = new SupplySlot("battery", 1);
            report.fixtures.Add("Inventory water2/food2/flashlight1/battery1 assigned for real production PackSlot transactions; bag starts empty.");
            w.InventorySelect(0); w.ToggleInventory();
            w.InputLocked = false;
            report.inventoryClockBefore = w.Model.TimeRemaining;
            yield return Hold(.35f);
            report.inventoryClockAfter = w.Model.TimeRemaining;
            w.InputLocked = true;
            CheckIt("inventory retains existing live preparation clock", w.InventoryOpen && report.inventoryClockAfter < report.inventoryClockBefore - .05f &&
                report.inventoryClockAfter > report.inventoryClockBefore - 2f,
                "clock=" + report.inventoryClockBefore + "->" + report.inventoryClockAfter + "; no pause added by redesign.");
            int budget = w.Model.Budget;
            w.InventoryUse(); w.InventorySelect(1); w.InventoryUse(); w.InventorySelect(2); w.InventoryUse();
            bool packed = w.Model.PackedWater == 1 && w.Model.PackedFood == 1 && w.Expedition.Count("flashlight") == 0 && w.Expedition.Count("battery") == 0 &&
                w.Expedition.Bag.Any(s => s.Id == "flashlight" && s.Count == 1);
            yield return Shot("inventory-after-real-packing", 1600, 900);
            CheckIt("packing uses real stock battery and eight-slot bag", packed && w.Expedition.Inventory.Count == 12 && w.Expedition.Bag.Count == 8 && w.Model.Budget == budget,
                "Production InventoryUse/Packing: one water and food, real flashlight+battery consumed; normal slot capacities preserved.");
            w.ToggleInventory();
        }

        IEnumerator WorkFamilies()
        {
            NewRun(); SeedInventory();
            string[] actions = { "anchorwall", "plant", "hose" };
            string[] targets = { "bookshelf", "hud-plant-fixture", "hud-hose-fixture" };
            PanicWorkMode[] modes = { PanicWorkMode.Ratchet, PanicWorkMode.Timing, PanicWorkMode.Balance };
            for (int i = 0; i < actions.Length; i++)
            {
                int budget = w.Model.Budget; string stock = Stock(w.Expedition.Inventory);
                w.BeginWork(actions[i], targets[i]);
                bool began = w.WorkActive && w.Expedition.Work.Mode == modes[i] && w.WorkTexture != null;
                yield return Shot("work-" + modes[i].ToString().ToLowerInvariant() + "-align", 1600, 900);
                // Explicit cursor state fixture enters the real second stage without completing any work.
                w.Expedition.Work.Cursor = w.Expedition.Work.Target;
                w.StepWork(.016f, 0f, false, true, false);
                bool applying = w.Expedition.Work.IsApplying;
                report.fixtures.Add("Work " + actions[i] + ": funded World.BeginWork; cursor assigned Target once, production StepWork Enter locks alignment; no completion/progress override.");
                yield return Shot("work-" + modes[i].ToString().ToLowerInvariant() + "-apply", i == 2 ? 1024 : 1280, i == 2 ? 768 : 720);
                w.CancelWork(); report.cancelledWorkFixtures++;
                CheckIt(modes[i] + " renders align/apply and cancel is free", began && applying && !w.WorkActive && w.Model.Budget == budget && Stock(w.Expedition.Inventory) == stock,
                    "Work cancel keeps exact budget/stock; this is a presentation/cancel test, not completed minigame acceptance.");
                yield return null;
            }
        }

        IEnumerator PauseAndDeveloper()
        {
            NewRun(); w.InputLocked = false;
            setPaused.Invoke(hud, new object[] { true });
            float before = w.Model.TimeRemaining;
            yield return Hold(.25f);
            yield return Shot("pause", 1600, 900);
            bool pauseHeld = IsPaused() && w.InputLocked && Mathf.Abs(before - w.Model.TimeRemaining) < .001f;
            setPaused.Invoke(hud, new object[] { false });
            bool restored = !IsPaused() && !w.InputLocked;
            CheckIt("pause holds clock and restores prior unlocked world", pauseHeld && restored,
                "Existing private SetPaused constructs pause fixture; actual production world Tick gate observed.");
            var source = w.Expedition.Sources.First(s => s.Id == "cabinet");
            w.Warp(source.Position + new Vector3(0f, .05f, -1.2f)); report.setupWarps++; w.SetFocus(source.Id); Physics.SyncTransforms();
            bool openBefore = source.Open;
            hud.SetDeveloperMenuOpen(true);
            report.menuClockBefore = w.Model.TimeRemaining;
            Vector3 position = w.PlayerPosition;
            w.HandleKey(KeyCode.E); w.HandleKey(KeyCode.Tab);
            yield return Hold(.3f);
            report.menuClockAfter = w.Model.TimeRemaining;
            yield return Shot("developer-clean-clock-input-hold", 1024, 768);
            bool held = hud.DeveloperMenuOpen && w.InputLocked && !w.InventoryOpen && source.Open == openBefore &&
                Vector3.Distance(position, w.PlayerPosition) < .005f && Mathf.Abs(report.menuClockBefore - report.menuClockAfter) < .001f;
            hud.SetDeveloperMenuOpen(false);
            w.HandleKey(KeyCode.E); w.HandleKey(KeyCode.Tab);
            bool closeProtected = hud.DeveloperInputBlockedThisFrame && !w.InventoryOpen && source.Open == openBefore;
            w.InputLocked = true;
            CheckIt("developer opening is clean and clock/input held", held && closeProtected && !w.Model.CheatedRun && !w.Model.DeveloperAnyEnabled,
                "Semantic E/Tab API probes while menu owns input and same-frame close gate; no physical keyboard claim.");
            yield return null;
        }

        IEnumerator DisasterAndResults()
        {
            foreach (DisasterType disaster in Enum.GetValues(typeof(DisasterType)))
            {
                NewRun(); w.Model.Disaster = disaster;
                PhaseFixture(PanicPhase.Disaster, 7f);
                effects.OnPhase(PanicPhase.Disaster);
                yield return Hold(.3f);
                yield return Shot("disaster-" + disaster.ToString().ToLowerInvariant(), 1600, 900);
                CheckIt(disaster + " cue renders with disaster phase/weather", w.Model.Phase == PanicPhase.Disaster && !w.CinematicActive && !w.LossAnnounced,
                    "Declared phase fixture; production effects OnPhase. No simulated disaster survival or audio acceptance.");
            }
            NewRun();
            w.Model.Items[0].Damage = 42f; w.Model.Items[0].Contribution = 24f;
            w.Model.Items[1].Damage = 18f; w.Model.Items[1].Contribution = 9f;
            w.Model.TotalDamage = 33f; w.Model.CompletedWaves = 1; w.Model.Score = 670;
            PhaseFixture(PanicPhase.Inspection, 999f);
            report.fixtures.Add("Inspection explicit two damaged items (contribution24,9), total damage33%, completed waves1, score670; no earned campaign claim.");
            yield return Shot("inspection", 1600, 900);
            PhaseFixture(PanicPhase.Success, 999f);
            yield return Shot("success-result", 1280, 720);
            NewRun(); w.Model.TimeRemaining = w.Model.PhaseTime = .05f; w.InputLocked = false;
            report.fixtures.Add("Deadline clock assigned .05 seconds; actual World.Update/Model.Tick failure and real loss reveal/cinematic observed; score writes isolated only.");
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!w.LossAnnounced && Time.realtimeSinceStartup < deadline) yield return null;
            w.InputLocked = true;
            bool revealed = w.Model.Phase == PanicPhase.GameOver && w.LossAnnounced && w.LossRevealTime > 0f && !w.CinematicActive;
            yield return Shot("deadline-loss-before-cinematic", 1600, 900);
            CheckIt("actual deadline reveals loss before cinematic", revealed,
                "Real production timeout after explicit short clock setup; LossAnnounced/positive reveal time precedes CinematicActive.");
            w.InputLocked = false;
            deadline = Time.realtimeSinceStartup + 3f;
            while (!w.CinematicActive && Time.realtimeSinceStartup < deadline) yield return null;
            w.InputLocked = true;
            bool cinematic = w.CinematicActive && w.Model.Phase == PanicPhase.GameOver;
            yield return Shot("deadline-failure-cinematic", 1280, 720);
            w.InputLocked = false;
            deadline = Time.realtimeSinceStartup + 8f;
            while ((w.CinematicActive || w.LossAnnounced) && Time.realtimeSinceStartup < deadline) yield return null;
            w.InputLocked = true;
            yield return Shot("gameover-result", 1024, 768);
            CheckIt("deadline cinematic reaches visible end result", cinematic && !w.CinematicActive && !w.LossAnnounced && w.Model.Phase == PanicPhase.GameOver,
                "Actual production failure sequence observed at native frame rate; no cinematic fast-forward mutation.");
        }

        void NewRun()
        {
            hud.SetDeveloperMenuOpen(false);
            if (IsPaused()) setPaused.Invoke(hud, new object[] { false });
            w.StartGame(false); w.InputLocked = true; SyncHud();
            report.fixtures.Add("Production World.StartGame(false) resets next HUD scenario; initial player placement is production StartGame setup.");
        }
        void SeedInventory()
        {
            Clear(w.Expedition.Inventory);
            string[] ids = { "bracket", "screws", "seedling", "hose", "plank", "sandbag", "water", "food", "extinguisher", "bandage", "antiseptic", "battery" };
            for (int i = 0; i < ids.Length; i++) w.Expedition.Inventory[i] = new SupplySlot(ids[i], i == 1 ? 4 : 2);
            report.fixtures.Add("12-slot funded inventory fixture assigned for UI/work setup; normal budget and recipes remain active.");
        }
        void PhaseFixture(PanicPhase phase, float seconds)
        {
            w.CancelWork(); if (w.InventoryOpen) w.ToggleInventory();
            resetCinema.Invoke(w, null);
            w.Model.Phase = phase; w.Model.PhaseTime = seconds;
            priorPhase.SetValue(w, phase); SyncHud(); w.InputLocked = true;
            report.fixtures.Add("HUD-only phase fixture " + phase + "; private previousPhase synchronized and production cinematic reset so intended HUD branch is visible.");
        }
        void SyncHud() { synchronize.Invoke(hud, null); }
        bool IsPaused() { return (bool)paused.GetValue(hud); }
        bool ReadBooleanProperty(string name)
        {
            var property = typeof(PanicHomeHud).GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (property == null) throw new InvalidOperationException("Concept HUD diagnostic property missing: " + name);
            return (bool)property.GetValue(hud, null);
        }
        void VerifyFonts()
        {
            const string glyphs = "ÇĞİÖŞÜçğıöşü₺";
            bool all = true;
            string[] names = { "_displayFont", "_condensedFont", "_bodyFont" };
            string[] expectedNames = { "Oswald", "BarlowCondensed", "Barlow" };
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                var field = typeof(PanicHomeHud).GetField(name, Private);
                var font = field == null ? null : field.GetValue(hud) as Font;
                string missing = "";
                if (font)
                {
                    font.RequestCharactersInTexture(glyphs, 24, FontStyle.Normal);
                    foreach (char glyph in glyphs) if (!font.HasCharacter(glyph)) missing += glyph;
                }
                bool packaged = font && string.Equals(font.name.Replace(" ", ""), expectedNames[i], StringComparison.OrdinalIgnoreCase);
                all &= packaged && missing.Length == 0;
                report.fonts.Add(name + "=" + (font ? font.name : "MISSING") + "; missing=" + missing);
            }
            var version = typeof(PanicHomeHud).GetProperty("ThemeVersion", BindingFlags.Public | BindingFlags.Instance);
            report.themeVersion = version == null ? "MISSING" : Convert.ToString(version.GetValue(hud, null));
            CheckIt("packaged concept fonts contain Turkish and currency glyphs", all && report.themeVersion != "MISSING",
                string.Join("; ", report.fonts.ToArray()) + "; theme=" + report.themeVersion + "; checks glyph availability, screenshots require visual review.");
        }
        static bool Required(IReadOnlyList<PanicMaterialRequirement> list, string id, int need, int owned, int missing)
        { for (int i = 0; i < list.Count; i++) if (list[i].Id == id) return list[i].Required == need && list[i].Owned == owned && list[i].Missing == missing; return false; }
        static void Clear(List<SupplySlot> list) { for (int i = 0; i < list.Count; i++) list[i] = new SupplySlot(); }
        static string Stock(List<SupplySlot> list) { return string.Join("|", list.Select(s => s.Id + ":" + s.Count).ToArray()); }
        IEnumerator Hold(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
        IEnumerator Resize(int width, int height)
        {
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
            float deadline = Time.realtimeSinceStartup + 3f;
            do { yield return null; } while ((Screen.width != width || Screen.height != height) && Time.realtimeSinceStartup < deadline);
            yield return null; yield return new WaitForEndOfFrame();
        }
        IEnumerator Shot(string id, int width, int height)
        {
            report.plannedCaptures++;
            if (Screen.width != width || Screen.height != height) yield return Resize(width, height);
            yield return null; yield return new WaitForEndOfFrame();
            var capture = InspectLayout(id, width, height);
            capture.path = Path.Combine(w.EvidencePath, "hud-" + id + "-" + width + "x" + height + ".png");
            if (File.Exists(capture.path)) throw new InvalidOperationException("Refusing to overwrite HUD evidence " + capture.path);
            // ReadPixels after the rendered GUI frame proves actual framebuffer dimensions and avoids asynchronous screenshot races.
            var pixels = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            try
            {
                pixels.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
                pixels.Apply(false, false);
                File.WriteAllBytes(capture.path, pixels.EncodeToPNG());
            }
            finally { Destroy(pixels); }
            report.captures.Add(capture); report.screenshots.Add(capture.path);
            CheckIt("layout " + id + " " + width + "x" + height, capture.passed,
                "actual=" + capture.width + "x" + capture.height + "; canvasFit=" + capture.canvasFits + "; outside=" + capture.outsideRects + "; dominant overlaps=" + capture.dominantOverlaps);
        }
        Capture InspectLayout(string id, int width, int height)
        {
            float canvasWidth = Constant("CanvasWidth", 1600f), canvasHeight = Constant("CanvasHeight", 900f);
            float scale = (float)scaleField.GetValue(hud); Vector2 offset = (Vector2)offsetField.GetValue(hud);
            var rects = rectsField.GetValue(hud) as List<Rect>;
            var regionProperty = typeof(PanicHomeHud).GetProperty("VisibleHudRegions", BindingFlags.Public | BindingFlags.Instance);
            var regions = regionProperty == null ? null : regionProperty.GetValue(hud, null) as IReadOnlyList<Rect>;
            IReadOnlyList<Rect> overlapRects = regions != null && regions.Count > 0 ? regions : rects;
            var c = new Capture { id = id, requestedWidth = width, requestedHeight = height, width = Screen.width, height = Screen.height,
                phase = w.Model.Phase.ToString(), workMode = w.WorkActive ? w.Expedition.Work.Mode.ToString() : "none",
                scale = scale, offsetX = offset.x, offsetY = offset.y, canvasWidth = canvasWidth, canvasHeight = canvasHeight, markedRects = rects == null ? 0 : rects.Count,
                dominantRegions = overlapRects == null ? 0 : overlapRects.Count, overlapBasis = regions != null && regions.Count > 0 ? "VisibleHudRegions" : "marked-rectangle fallback" };
            c.canvasFits = Finite(scale) && scale > 0 && offset.x >= -.75f && offset.y >= -.75f &&
                offset.x + canvasWidth * scale <= Screen.width + .75f && offset.y + canvasHeight * scale <= Screen.height + .75f;
            if (rects != null)
            {
                for (int i = 0; i < rects.Count; i++)
                {
                    Rect r = rects[i];
                    if (!Finite(r.x) || !Finite(r.y) || !Finite(r.width) || !Finite(r.height) || r.width < 0 || r.height < 0 ||
                        offset.x + r.xMin * scale < -.75f || offset.y + r.yMin * scale < -.75f ||
                        offset.x + r.xMax * scale > Screen.width + .75f || offset.y + r.yMax * scale > Screen.height + .75f)
                    { c.outsideRects++; c.layoutIssues.Add("outside rect " + i + " " + r); }
                }
            }
            if (overlapRects != null)
            {
                for (int i = 0; i < overlapRects.Count; i++)
                {
                    Rect a = overlapRects[i]; if (!Dominant(a, canvasWidth * canvasHeight)) continue;
                    for (int j = i + 1; j < overlapRects.Count; j++)
                    {
                        Rect b = overlapRects[j];
                        if (!Dominant(b, canvasWidth * canvasHeight) || Contains(a, b) || Contains(b, a)) continue;
                        float overlap = Mathf.Max(0, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin)) *
                            Mathf.Max(0, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));
                        if (overlap <= 600f || overlap / Mathf.Min(a.width * a.height, b.width * b.height) <= .12f) continue;
                        c.dominantOverlaps++; c.layoutIssues.Add("dominant overlap " + i + "/" + j + ": " + a + " vs " + b);
                    }
                }
            }
            c.passed = c.width == width && c.height == height && c.canvasFits && c.outsideRects == 0 && c.dominantOverlaps == 0 && c.markedRects > 0;
            return c;
        }
        static bool Dominant(Rect r, float canvasArea) { return r.width >= 200 && r.height >= 60 && r.width * r.height >= 18000 && r.width * r.height < canvasArea * .90f; }
        static bool Contains(Rect a, Rect b) { return a.xMin <= b.xMin + .5f && a.yMin <= b.yMin + .5f && a.xMax >= b.xMax - .5f && a.yMax >= b.yMax - .5f; }
        static bool Finite(float n) { return !float.IsNaN(n) && !float.IsInfinity(n); }
        static float Constant(string name, float fallback)
        { var f = typeof(PanicHomeHud).GetField(name, BindingFlags.Static | BindingFlags.NonPublic); return f == null ? fallback : Convert.ToSingle(f.GetRawConstantValue()); }
        static string Hash(string path)
        {
            if (!File.Exists(path)) return "MISSING";
            using (var sha = SHA256.Create()) using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }
        void CheckIt(string name, bool passed, string detail)
        { report.checks.Add(new Check { name = name, passed = passed, detail = detail }); Debug.Log("HUD_CHECK " + (passed ? "PASS " : "FAIL ") + name + " " + detail); }
        void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Update()
        {
            if (finished || !w || Time.realtimeSinceStartup - started <= HardCap) return;
            report.errors.Add("HUD smoke exceeded 240 second cap; unfinished scenarios are not accepted.");
            if (running != null) StopCoroutine(running); Finish();
        }
        void Finish()
        {
            if (finished) return; finished = true;
            if (w) w.InputLocked = true;
            report.realScoreAfter = Hash(report.realScorePath);
            if (report.realScoreBefore != report.realScoreAfter) report.errors.Add("Real personal score file hash changed during isolated smoke.");
            foreach (string path in report.screenshots) if (!File.Exists(path) || new FileInfo(path).Length < 256) report.errors.Add("Missing/empty native render: " + path);
            report.finishedUtc = DateTime.UtcNow.ToString("o"); report.runtimeSeconds = Time.realtimeSinceStartup - started;
            report.passed = report.completed && report.errors.Count == 0 && report.checks.Count >= 55 && report.checks.All(c => c.passed) &&
                report.captures.Count == report.plannedCaptures && report.captures.Count == 50;
            Application.logMessageReceived -= Log;
            try { File.WriteAllText(Path.Combine(w.EvidencePath, ReportName), JsonUtility.ToJson(report, true)); }
            catch (Exception ex) { Debug.LogError("HUD_CONCEPT_SMOKE report write failed: " + ex); report.passed = false; }
            Debug.Log("HUD_CONCEPT_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " errors=" + report.errors.Count + " captures=" + report.captures.Count + " seconds=" + report.runtimeSeconds.ToString("F2"));
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--keep-home-smoke") < 0) Quit(report.passed ? 0 : 1);
            else if (w) { w.ReturnToMenu(); SyncHud(); w.InputLocked = false; enabled = false; }
        }
        void Quit(int code) { if (quitRequested) return; quitRequested = true; Application.Quit(code); }
    }
}
