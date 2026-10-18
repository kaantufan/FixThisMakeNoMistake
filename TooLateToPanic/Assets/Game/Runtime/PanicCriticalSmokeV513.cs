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
    // Dedicated opt-in native production API/framebuffer driver. Pose, clock, phase and
    // detached-model fixtures are recorded. No physical OS input or human acceptance.
    [DefaultExecutionOrder(10000)]
    public sealed partial class PanicCriticalSmokeV513 : MonoBehaviour
    {
        const string Flag = "--critical-v513-smoke";
        const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id, detail; public bool pass; }
        [Serializable] public sealed class Shot { public string path, sha256, context; public int width, height, nonDark; }
        [Serializable] public sealed class Suite { public string name; public int checks; }
        [Serializable] public sealed class AnimationSample { public string name, hazards; public int wave, mask; public float age; public Rect bounds; }
        [Serializable] public sealed class Report
        {
            public string utc, dllSha256, scorePath, realScoreBefore, realScoreAfter, error;
            public string layer = "CURRENT_DLL_NATIVE_PRODUCTION_API_AND_FRAMEBUFFER_WITH_DECLARED_FIXTURES";
            public bool complete, passed, OSInput, humanAcceptance;
            public float seconds;
            public int productionStarts, setupWarps, productionKeyCalls, finiteSourceCalls, workBegins, worldShelterCalls;
            public List<Row> checks = new List<Row>();
            public List<Shot> captures = new List<Shot>();
            public List<Suite> suites = new List<Suite>();
            public List<AnimationSample> animation = new List<AnimationSample>();
            public List<string> fixtures = new List<string>();
            public List<string> errors = new List<string>();
        }
        readonly Report report = new Report();
        PanicHomeWorld w; PanicHomeHud hud;
        float began; bool finished; string realScore;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (world) world.gameObject.AddComponent<PanicCriticalSmokeV513>();
        }
        void Start()
        {
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); began = Time.realtimeSinceStartup;
            if (!w || !hud || string.IsNullOrEmpty(w.EvidencePath))
            { Debug.LogError("CRITICAL_V513 requires isolated evidence/World/HUD"); Application.Quit(2); return; }
            report.utc = DateTime.UtcNow.ToString("o"); report.dllSha256 = Hash(typeof(PanicModel).Assembly.Location);
            realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json"); report.realScoreBefore = Hash(realScore);
            // World.Awake first selects its already validated fresh generic QA profile.
            // Every model constructed by this driver uses a second, uniquely named local profile.
            string initial = PanicModel.ScorePathOverride;
            if (initial != Path.Combine(w.EvidencePath, "isolated-home-scores.json"))
            { Debug.LogError("CRITICAL_V513 refuses nonisolated initial score profile"); Application.Quit(2); return; }
            PanicModel.ScorePathOverride = Path.Combine(w.EvidencePath, "isolated-critical-v513-scores-" + Guid.NewGuid().ToString("D") + ".json");
            report.scorePath = PanicModel.ScorePathOverride;
            Application.logMessageReceived += Log; Application.runInBackground = true;
            StartCoroutine(Guard(Run()));
        }
        void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Check(string id, bool pass, string detail = "")
        { report.checks.Add(new Row { id = id, pass = pass, detail = detail }); Debug.Log("CRITICAL_V513 " + (pass ? "PASS " : "FAIL ") + id + " " + detail); }
        static string Hash(string path)
        { if (!File.Exists(path)) return "absent"; using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""); }
        static void Set(object target, string name, object value) { target.GetType().GetField(name, Members).SetValue(target, value); }
        static T Read<T>(object target, string name)
        { var type = target.GetType(); var p = type.GetProperty(name, Members); return (T)(p != null ? p.GetValue(target) : type.GetField(name, Members).GetValue(target)); }
        static object Call(object target, string name, params object[] args)
        { return target.GetType().GetMethods(Members).First(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(target, args); }
        static bool Near(float a, float b) { return Mathf.Abs(a - b) < .003f; }
        void RunSuite(string name, Action<Action<string, bool, string>> action, int minimum)
        {
            int before = report.checks.Count; action(Check); int count = report.checks.Count - before;
            report.suites.Add(new Suite { name = name, checks = count });
            Check("suite." + name + ".executed", count >= minimum, "Current DLL rows=" + count + "; minimum=" + minimum);
        }
        IEnumerator Wait(float seconds)
        { float until = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < until) yield return null; }
        IEnumerator Resolution(int width, int height)
        { Screen.SetResolution(width, height, FullScreenMode.Windowed); yield return Wait(.3f); Check("resolution." + width, Screen.width == width && Screen.height == height, "Actual framebuffer=" + Screen.width + "x" + Screen.height); }
        IEnumerator Capture(string name, string context)
        {
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (!texture) { Check("capture." + name, false, "Missing framebuffer"); yield break; }
            string path = Path.Combine(w.EvidencePath, name + ".png"); File.WriteAllBytes(path, texture.EncodeToPNG());
            var pixels = texture.GetPixels32(); int nonDark = 0; byte min = 255, max = 0;
            for (int i = 0; i < pixels.Length; i += 137)
            { var p = pixels[i]; byte l = (byte)((p.r + p.g + p.b) / 3); if (l > 12) nonDark++; min = Math.Min(min, l); max = Math.Max(max, l); }
            Check("capture." + name, nonDark > 80 && max - min > 18, "Actual framebuffer=" + texture.width + "x" + texture.height + ";nonDark=" + nonDark);
            report.captures.Add(new Shot { path = path, sha256 = Hash(path), context = context, width = texture.width, height = texture.height, nonDark = nonDark }); Destroy(texture);
        }
        IEnumerator Run()
        {
            Check("launch.isolated-registered", PanicTestLaunchRules.Drivers.Contains(Flag) &&
                Path.GetDirectoryName(report.scorePath) == Path.GetFullPath(w.EvidencePath) && !File.Exists(report.scorePath), report.scorePath);
            if (!report.checks.Last().pass) throw new InvalidOperationException("Fresh isolated registered driver required.");
            report.fixtures.Add("Graphical isolated V513 process. FullFlowScriptedInput suppresses ordinary physical input sampling; HandleKey invokes the real production key API. No prior-player input, OS input, human readability/listening/comfort or natural traversal acceptance.");
            report.fixtures.Add("Four current-DLL suites retain their documented detached model/threat/budget/tier/RNG fixtures. Extra repair scenario explicitly seeds a model, funds its budget and sets existing structural tiers; actual resolution, finite stock, recipe, cancel and charge APIs remain authoritative.");
            report.fixtures.Add("World actor/facing poses search up to16 angles around actual targets with the enabled original controller and real range/floor/path/center-ray gates. Preparation clocks39s and unresolved Disaster phase8s are explicit fixtures. No controller, ray, collision, shelter or cost bypass.");
            report.fixtures.Add("First-wave unsheltered NoLoss inspection is deliberately rewardless and score-excluded, used only to reach real FinishInspection. Wave2 hazards, popup mask/text/timing and restock come from the actual transition. Announcement sample ages are observed from real animation; pause must freeze both clocks.");
            w.FullFlowScriptedInput = true;
            RunSuite("roof-wall-v511", PanicRoofWallChecksV511.Run, 136);
            RunSuite("presentation-v512", PanicPresentationChecksV512.Run, 54);
            RunSuite("critical-v513", PanicCriticalTasksChecksV513.Run, 100);
            RunSuite("downgrade-v513", PanicUpgradeDowngradeChecksV513.Run, 24);
            RepairChecks();
            yield return Resolution(1600, 900);
            yield return BeginNormal();
            yield return GuidanceToggleChecks();
            yield return FirstWaveLayout();
            yield return InteractionChecks();
            yield return FpsSupply();
            yield return FollowingWave();
            yield return UpperShelterAndCompound();
            Check("runtime.no-errors", report.errors.Count == 0, string.Join("\n", report.errors)); report.complete = true;
        }
        IEnumerator BeginNormal()
        {
            w.InputLocked = false; hud.SetPaused(false); hud.ResetKeyboardHelpPresentationV56();
            if (w.Notebook != null) w.Notebook.ForceClosed();
            w.ReturnToMenu(); w.StartGame(false); report.productionStarts++; w.FullFlowScriptedInput = true; yield return null;
            if (hud.FreeplayOnboardingOpenV414) hud.ConfirmFreeplayOnboardingV414(); yield return null;
            w.SkipIntroStoryV414(); yield return Wait(.2f);
            Check("world.normal-ready", w.Model.Phase == PanicPhase.Preparing && w.Model.Wave == 1 && !w.Model.IsPaused && !w.InputLocked &&
                !w.CinematicActive && w.PlayerControllerEnabled && w.Model.Budget == 1500 && !w.Model.CheatedRun && !w.Model.ReachedShelterV5,
                "Real normal StartGame, onboarding confirmation and intro skip; current90+40 timing/1500 budget.");
        }
        string GuidanceModelStateV513()
        {
            return w.Model.TimeRemaining.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "|" +
                w.Model.Budget + "|" + w.Model.Wave + "|" + w.Model.Phase + "|" + w.Model.ReachedShelterV5 + "|" +
                string.Join(";", w.Model.CriticalTasksV513.Select(t => t.Id + ":" + t.Complete + ":" + t.Resolved)) + "|" +
                w.FirstWaveOnboardingRequiredV510 + ":" + w.FirstWaveOnboardingV510.Step;
        }
        void SetGuidanceCheckedV513(bool expanded, string id)
        {
            string before = GuidanceModelStateV513();
            hud.SetGuidanceExpandedV513(expanded);
            Check("guidance.model-unchanged." + id, before == GuidanceModelStateV513() && hud.GuidanceExpandedV513 == expanded,
                "Synchronous real toggle API preserves exact preparation clock, budget, wave/phase, shelter and mandatory/critical objective state.");
        }
        IEnumerator GuidanceToggleChecks()
        {
            report.fixtures.Add("R7 guidance toggle uses the same public setter as the production button callback. No OS click claim. Model-state equality is measured synchronously around each setter; natural frame time remains live during Repaint screenshots.");
            yield return new WaitForEndOfFrame();
            Check("guidance.default-collapsed", !hud.GuidanceExpandedV513 && hud.GuidanceToggleVisibleV513 &&
                hud.CriticalCardEligibleV513 && hud.FirstWaveCardEligibleV510 && !hud.FirstWaveCardVisibleV510 &&
                string.IsNullOrEmpty(hud.CriticalRenderedTextV513),
                "Default hides both left panels while mandatory model objectives remain required/eligible and one toggle stays visible.");
            Rect toggle = hud.GuidanceToggleBoundsV513;
            Check("guidance.toggle-bounds.1600", toggle.width > 0 && toggle.height > 0 && Contains(hud.LogicalCanvasBounds, toggle),
                "Actual toggle Repaint bounds=" + toggle);
            yield return Capture("guidance-collapsed-1600", "R7 actual default collapsed guidance; both left panels hidden, single toggle retained; model objective rules unchanged.");
            SetGuidanceCheckedV513(true, "expand"); yield return new WaitForEndOfFrame();
            Check("guidance.expand-both", hud.GuidanceToggleVisibleV513 && hud.FirstWaveCardVisibleV510 &&
                !string.IsNullOrEmpty(hud.CriticalRenderedTextV513) && hud.CriticalCardBoundsV513.width > 0,
                "One setter expands both actual rendered left panels; existing expanded1600/960 captures follow.");
            SetGuidanceCheckedV513(false, "collapse"); yield return new WaitForEndOfFrame();
            Check("guidance.collapse-both", !hud.FirstWaveCardVisibleV510 && string.IsNullOrEmpty(hud.CriticalRenderedTextV513) &&
                hud.GuidanceToggleVisibleV513 && w.FirstWaveOnboardingRequiredV510,
                "Collapse changes presentation only; mandatory first-wave objective remains active.");
            yield return Resolution(960, 540); yield return new WaitForEndOfFrame();
            toggle = hud.GuidanceToggleBoundsV513;
            Check("guidance.toggle-bounds.960", !hud.GuidanceExpandedV513 && hud.GuidanceToggleVisibleV513 && toggle.width > 0 &&
                Contains(hud.LogicalCanvasBounds, toggle), "Actual960x540 Repaint with collapsed panels and logical toggle bounds=" + toggle);
            yield return Capture("guidance-collapsed-960", "R7 actual960x540 collapsed guidance and one retained toggle; expanded960 screenshot remains in original layout smoke.");
            SetGuidanceCheckedV513(true, "expand-before-modal"); yield return new WaitForEndOfFrame();
            hud.SetPaused(true); yield return new WaitForEndOfFrame();
            Check("guidance.hidden-in-pause", !hud.GuidanceToggleVisibleV513 && !hud.FirstWaveCardVisibleV510 &&
                !hud.CriticalCardEligibleV513 && w.FirstWaveOnboardingRequiredV510,
                "Real pause modal hides toggle and cards while the model objective remains mandatory.");
            hud.SetPaused(false); yield return new WaitForEndOfFrame();
            Check("guidance.expanded-restored-after-modal", hud.GuidanceExpandedV513 && hud.GuidanceToggleVisibleV513 &&
                hud.FirstWaveCardVisibleV510 && !string.IsNullOrEmpty(hud.CriticalRenderedTextV513),
                "Closing modal restores the chosen expanded presentation without resetting objective state.");
            yield return Resolution(1600, 900);
        }
        IEnumerator FirstWaveLayout()
        {
            hud.SetGuidanceExpandedV513(true);
            w.SetAssistantEnabledV55(false);
            foreach (int width in new[] { 1600, 960 })
            {
                yield return Resolution(width, width == 1600 ? 900 : 540); yield return new WaitForEndOfFrame();
                Rect critical = hud.CriticalCardBoundsV513, mandatory = hud.FirstWaveCardBoundsV510;
                Check("layout.critical-and-firstwave." + width, hud.CriticalCardEligibleV513 && !string.IsNullOrEmpty(hud.CriticalRenderedTextV513) &&
                    hud.FirstWaveCardVisibleV510 && critical.width > 0 && mandatory.width > 0 && !critical.Overlaps(mandatory) &&
                    Contains(hud.LogicalCanvasBounds, critical) && Contains(hud.LogicalCanvasBounds, mandatory) &&
                    hud.FirstWaveContentHeightV510 <= mandatory.height,
                    "Actual Repaint logical bounds critical=" + critical + ";firstwave=" + mandatory + ";optional helper OFF; human readability separate.");
                Check("layout.compact-panel-widths." + width, critical.width > 0 && critical.width <= 310.01f && mandatory.width > 0 && mandatory.width <= 310.01f,
                    "Actual Repaint critical/first-wave widths=" + critical.width + "/" + mandatory.width + "; both compact panels remain within310 logical pixels.");
                string criticalText = hud.CriticalRenderedTextV513 ?? string.Empty;
                Check("text.critical-labels-only." + width, criticalText.Length > 0 &&
                    criticalText.IndexOf("koşu biter", StringComparison.OrdinalIgnoreCase) < 0 &&
                    criticalText.IndexOf("Ceza:", StringComparison.OrdinalIgnoreCase) < 0 &&
                    criticalText.IndexOf("Kritik ek hasar", StringComparison.OrdinalIgnoreCase) < 0,
                    "Actual rendered critical text contains task labels and completion markers, without old consequence/footer copy. Text=" + criticalText);
                string firstText = hud.FirstWaveRenderedTextV510 ?? string.Empty;
                Check("text.firstwave-initial-compact." + width, w.FirstWaveOnboardingV510.Step == PanicFirstWaveStepV510.Materials &&
                    firstText.Length > 0 && firstText.Length <= 220 &&
                    firstText.IndexOf("İLK DALGA · DEPREM", StringComparison.OrdinalIgnoreCase) < 0 &&
                    firstText.IndexOf("Tesisat 0/3", StringComparison.OrdinalIgnoreCase) < 0 &&
                    firstText.IndexOf("Su 0/1", StringComparison.OrdinalIgnoreCase) < 0 &&
                    firstText.IndexOf("Gıda 0/1", StringComparison.OrdinalIgnoreCase) < 0,
                    "Actual initial materials guidance is at most220 characters with no old duplicated status paragraph; length=" + firstText.Length + ";text=" + firstText);
                var tasks = w.Model.CriticalTasksV513;
                Check("tasks.firstwave-authoritative." + width, tasks.Count == 4 && tasks.Any(t => t.Id == "anchor" && !t.Complete) &&
                    tasks.Any(t => t.Id == "water" && !t.Complete) && tasks.Any(t => t.Id == "food" && !t.Complete) &&
                    tasks.First(t => t.Id == "shelter").Complete == w.Model.ReachedShelterV5,
                    "Mandatory card survives helper OFF; missing supplies/anchor and literal actual shelter truth.");
                yield return Capture("critical-firstwave-" + width, "Actual normal first-wave critical tasks and mandatory first-wave card; live logical bounds checked for overlap and canvas containment.");
            }
            yield return Resolution(1600, 900);
        }
        static bool Contains(Rect outer, Rect inner)
        { return inner.xMin >= outer.xMin && inner.yMin >= outer.yMin && inner.xMax <= outer.xMax && inner.yMax <= outer.yMax; }
        string Focus { get { return Read<string>(w, "focusId"); } }
        bool Position(string id, out Vector3 position)
        {
            object[] args = { id, Vector3.zero }; bool found = (bool)Call(w, "PositionOf", args); position = (Vector3)args[1]; return found;
        }
        IEnumerator Reach(string id, bool fps = false, Vector3? aim = null)
        {
            Vector3 target; if (!Position(id, out target)) throw new InvalidOperationException("INVALID_SETUP missing actual target:" + id);
            w.CancelWork(); yield return null;
            for (int a = 0; a < 16; a++)
            {
                float angle = a * Mathf.PI / 8f;
                int floor = Mathf.Clamp(Mathf.FloorToInt((target.y + .15f) / 3.2f), -1, 1);
                Vector3 pose = new Vector3(target.x + Mathf.Cos(angle) * 1.25f, floor * 3.2f + .05f, target.z + Mathf.Sin(angle) * 1.25f);
                w.Warp(pose); report.setupWarps++; Physics.SyncTransforms();
                for (int n = 0; n < 6; n++) { w.SmokeMove(Vector2.zero, .025f); yield return null; }
                if (fps)
                {
                    for (int n = 0; n < 3; n++) { Aim(aim ?? target + Vector3.up * .12f); yield return null; Call(w, "RefreshBagRayFocus"); }
                }
                else
                {
                    var hero = Read<Transform>(w, "hero"); Vector3 toward = target - w.PlayerPosition; toward.y = 0;
                    if (toward.sqrMagnitude > .001f) { float yaw = Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg; hero.rotation = Quaternion.Euler(0, yaw, 0); Set(w, "facingYaw", yaw); }
                    w.SetFocus(id); yield return null;
                }
                if (Focus == id && w.CanInteract && w.ActiveFloor == floor && w.PlayerControllerEnabled)
                { report.fixtures.Add("ACTOR_POSE " + id + "=" + w.PlayerPosition + ";angle=" + a + ";FPS=" + fps); yield return Wait(.18f); yield break; }
            }
            throw new InvalidOperationException("INVALID_SETUP no reachable real target after16 poses:" + id + ";actor=" + w.PlayerPosition + ";focus=" + Focus);
        }
        void Key(KeyCode key) { report.productionKeyCalls++; w.HandleKey(key); }
        void CheckPrompt(string id, string expectedKey)
        {
            string key, verb; Vector3 point; bool observed = w.TryInteractionPromptV513(out key, out verb, out point);
            Rect r = hud.InteractionPromptBoundsV513;
            Check("prompt." + id, observed && key == expectedKey && !string.IsNullOrWhiteSpace(verb) &&
                hud.InteractionPromptTextV513.StartsWith(expectedKey + " · ", StringComparison.Ordinal) &&
                r.width > 0 && r.width <= 250.01f && r.height <= 48.01f && Contains(hud.LogicalCanvasBounds, r) && !r.Overlaps(hud.CriticalCardBoundsV513) &&
                (!hud.FirstWaveCardVisibleV510 || !r.Overlaps(hud.FirstWaveCardBoundsV510)),
                "Actual range/path/floor accepted focus=" + Focus + ";API=" + key + "/" + verb + ";rendered=" + hud.InteractionPromptTextV513 + ";bounds=" + r);
        }
        bool HingeOpen(string id)
        { var hinges = (IDictionary)Read<object>(w, "hinges"); return Read<bool>(hinges[id], "open"); }
        IEnumerator InteractionChecks()
        {
            var source = w.Expedition.Sources.FirstOrDefault(s => s.Id == "pantry") ?? w.Expedition.Sources[0];
            yield return Reach(source.Id); yield return new WaitForEndOfFrame(); CheckPrompt("source-e", "E");
            yield return Capture("interaction-source-e", "Real reachable closed finite material source; E prompt and authoritative production interaction below.");
            bool wasOpen = source.Open; Key(KeyCode.E); yield return null;
            Check("action.source-e-once", source.Open != wasOpen && source.AutoLooted, "Actual E opens and consumes the source's once-only finite stock.");
            int revision = w.Expedition.LootLayoutRevisionV5; string stock = Stock(w.Expedition);
            Key(KeyCode.E); yield return null; Key(KeyCode.E); yield return null;
            Check("action.source-no-duplicate", source.AutoLooted && Stock(w.Expedition) == stock && revision == w.Expedition.LootLayoutRevisionV5,
                "Actual close/reopen does not regenerate stock or reroll layout.");
            if (source.Open) { Key(KeyCode.E); yield return null; }
            foreach (var s in w.Expedition.Sources)
            { if (!s.AutoLooted) { w.Expedition.ToggleSource(s.Id); report.finiteSourceCalls++; } if (s.Open) { w.Expedition.ToggleSource(s.Id); report.finiteSourceCalls++; } }

            yield return Reach("door_inner_ground"); yield return new WaitForEndOfFrame(); CheckPrompt("door-e", "E");
            yield return Capture("interaction-door-e", "Real same-floor transit door, production E prompt and unchanged physical range/path rules.");
            bool doorBefore = HingeOpen("door_inner_ground"); Key(KeyCode.E); yield return Wait(.2f);
            Check("action.door-e", HingeOpen("door_inner_ground") != doorBefore, "Actual E toggles the retained hinge owner.");

            yield return Reach("table"); yield return new WaitForEndOfFrame(); CheckPrompt("furniture-1", "1");
            yield return Capture("interaction-furniture-1", "Actual reachable table shows primary1 sabitleme; stocked materials came from finite source APIs.");
            int cash = w.Model.Budget; stock = Stock(w.Expedition); Key(KeyCode.Alpha1); report.workBegins++; yield return new WaitForEndOfFrame();
            string key, verb; Vector3 point;
            Check("prompt.hidden-in-work", w.WorkActive && !w.TryInteractionPromptV513(out key, out verb, out point), "Real key1 opened the actual two-screw work; world prompt API is hidden.");
            w.CancelWork(); yield return null;
            Check("action.work-cancel-free", !w.WorkActive && cash == w.Model.Budget && Stock(w.Expedition) == stock && !w.Model.Items.Find(i => i.Id == "table").Anchored,
                "Actual world cancellation preserves budget/materials/tier/anchor.");
            w.SetFocus("table"); yield return new WaitForEndOfFrame(); int paints = hud.InteractionPromptRepaintsV513;
            bool modal = hud.OpenKeyboardHelpV56(); yield return Wait(.12f);
            Check("prompt.hidden-in-modal", modal && w.Model.KeyboardHelpOpenV56 && hud.InteractionPromptRepaintsV513 == paints,
                "Actual F1 modal: prompt emits no additional Repaint; stale diagnostic text does not count as a rendered prompt.");
            hud.ResetKeyboardHelpPresentationV56(); yield return null;
            w.Warp(new Vector3(0, .05f, -10)); report.setupWarps++; yield return null; w.SetFocus("table"); yield return new WaitForEndOfFrame();
            Check("prompt.hidden-out-of-range", !w.CanInteract && !w.TryInteractionPromptV513(out key, out verb, out point), "Explicit far pose; no range or path bypass.");

            var upper = w.Model.Items.First(i => i.Kind == ItemKind.Wall && Mathf.FloorToInt((i.Position.y + .15f) / 3.2f) == 1);
            yield return Reach(upper.Id); yield return new WaitForEndOfFrame(); CheckPrompt("wall-3", "3");
            yield return Capture("interaction-upper-wall-3", "Actual reachable upper wall shows key3 and combined upper-wall/roof upgrade, using the existing transaction.");
            cash = w.Model.Budget; stock = Stock(w.Expedition); Key(KeyCode.Alpha3); report.workBegins++; yield return null;
            Check("action.wall-3", w.WorkActive && w.Expedition.ActiveFloorWallUpgradeV414 != null && w.Expedition.ActiveFloorWallUpgradeV414.IncludesRoof,
                "Actual3 opens the combined real work; roof cannot independently acquire a prompt.");
            w.CancelWork(); yield return null;
            Check("action.wall-cancel-free", cash == w.Model.Budget && Stock(w.Expedition) == stock, "No charge until real completion.");
            // The carried bag intentionally has no physical floor station since V5.1.
            // Use the actual installed utility as the remaining proximity interaction.
            yield return Reach(PanicUtilitySafetyV414.TargetForAction("poweroff")); yield return new WaitForEndOfFrame(); CheckPrompt("utility-e", "E");
            yield return Capture("interaction-utility-e", "Actual installed main switch shows E for its free manual utility work; no legacy bag station is fabricated.");
            Key(KeyCode.E); yield return null;
            Check("action.utility-e", w.WorkActive && w.Expedition.Work.IsUtility, "Actual E opens the existing main-switch work.");
            w.CancelWork(); yield return null;
            Key(KeyCode.Tab); yield return null;
            Check("action.inventory-tab", w.InventoryOpen && !w.TryInteractionPromptV513(out key, out verb, out point), "Global actual TAB opens the shared carried inventory and hides prompt.");
            Key(KeyCode.Tab); yield return null;
        }
        IEnumerator FpsSupply()
        {
            w.Model.TimeRemaining = w.Model.PhaseTime = 39f; w.TickPreparationStages(); yield return Wait(2.3f);
            Check("fps.real-stage", w.BagFirstPersonActive && w.PlayerControllerEnabled && !w.Model.ReachedShelterV5, "Declared39s clock fixture uses the actual camera handoff.");
            var supply = w.Expedition.FindWorldSupply("supply_water_kitchen") ?? w.Expedition.WorldSupplies.First(s => s.ItemId == "water" && s.Count > 0);
            yield return Reach(supply.Id, true, supply.Position + Vector3.up * .12f); yield return new WaitForEndOfFrame(); CheckPrompt("fps-supply-e", "E");
            yield return Capture("interaction-fps-supply-e", "Actual first-person center-ray water target and E prompt; enabled original controller, finite stock and real ray owner.");
            int count = supply.Count, before = w.Expedition.Count("water"); Key(KeyCode.E); yield return Wait(.1f);
            Check("action.fps-finite-supply-e", count > 0 && supply.Count == 0 && w.Expedition.Count("water") == before + count,
                "Actual fresh center-ray E consumes the existing supply once into the one shared inventory.");
            before = w.Expedition.Count("water"); Key(KeyCode.E); yield return null;
            Check("action.fps-no-duplicate", before == w.Expedition.Count("water") && !w.Model.ReachedShelterV5, "RepeatedE neither recreates supplies nor invents shelter admission.");
        }
        void Advance(float seconds)
        { w.Model.SetPaused(false); w.Model.Tick(seconds); }
        IEnumerator FollowingWave()
        {
            w.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss, true);
            w.InputLocked = true;
            w.Model.Phase = PanicPhase.Disaster; w.Model.PhaseTime = 8f;
            // Let actual World.Update observe each changed phase: it owns bag reset,
            // consumed-inventory sync, effects, camera, restock and previousPhase.
            yield return null;
            report.fixtures.Add("UNSHELTERED_PHASE_FIXTURE: first-wave Disaster8s, without setting ReachedShelter; real Model.Tick resolves, NoLoss produces only rewardless inspection.");
            Advance(8.001f); yield return null;
            Check("critical.real-unsheltered-resolution", w.Model.Phase == PanicPhase.Aftermath && !w.Model.ReachedShelterV5 &&
                w.Model.CriticalTasksV513.First(t => t.Id == "shelter").Complete == w.Model.ReachedShelterV5 &&
                w.Model.CriticalPenaltyDamageV513 > 0 && w.Model.CriticalPenaltyDamageV513 <= 40.003f,
                "Actual resolution applies missing-anchor points, preserves false physical arrival and freezes its task snapshot.");
            Advance(4.001f); yield return null;
            Check("wave1.rewardless-inspection", w.Model.Phase == PanicPhase.Inspection && w.Model.LastReward == 0 && w.Model.CompletedWaves == 0 && !w.Model.ReachedShelterV5,
                "NoLoss is declared; no successful wave/reward or shelter claim.");
            w.Model.SetWaveLedgerOpenV56(true); w.Model.SetWaveLedgerOpenV56(false);
            int oldWave = w.Model.Wave, announcements = hud.WaveAnnouncementCountV513, revision = w.Expedition.LootLayoutRevisionV5;
            w.Model.FinishInspection(); w.InputLocked = false; yield return null;
            Check("nextwave.real-finish-inspection", w.Model.Wave == oldWave + 1 && w.Model.Phase == PanicPhase.Preparing && w.Model.ActiveDisasterMaskV54 == 10 &&
                w.Model.Disaster == DisasterType.Flood && !w.Model.ReachedShelterV5 && w.Expedition.LootLayoutRevisionV5 == revision + 1,
                "Actual FinishInspection/BeginWave selected Flood+Storm(mask10), reset arrival and restocked once; no future-wave/mask fixture.");
            yield return Wait(.08f); yield return new WaitForEndOfFrame();
            Check("popup.actual-hazards-once", hud.WaveAnnouncementActiveV513 && hud.WaveAnnouncementCountV513 == announcements + 1 &&
                hud.WaveAnnouncementTextV513 == w.Model.ActiveDisasterLabelV54,
                "Rendered label=" + hud.WaveAnnouncementTextV513 + ";actual=" + w.Model.ActiveDisasterLabelV54);
            Rect center = hud.WaveAnnouncementBoundsV513;
            Check("popup.center", center.width >= 690 && center.x > 400 && center.y > 250, "Actual rendered center bounds=" + center);
            SampleAnimation("center"); yield return Capture("nextwave-actual-center", "Actual second wave chosen by FinishInspection; central Flood+Storm popup, real animation age recorded.");
            float age = Read<float>(hud, "announcementAgeV513"), clock = w.Model.TimeRemaining;
            hud.SetPaused(true); yield return Wait(.3f);
            Check("popup.pause-freezes-animation-and-model", Near(Read<float>(hud, "announcementAgeV513"), age) && Near(w.Model.TimeRemaining, clock),
                "Actual pauseAPI freezes announcement and preparation, without simulated time advancement.");
            hud.SetPaused(false); yield return WaitForAnnouncementAge(1.57f); yield return new WaitForEndOfFrame();
            Rect middle = hud.WaveAnnouncementBoundsV513;
            Check("popup.moves-toward-header", middle.width < center.width && middle.width > hud.WaveCardBounds.width && middle.y < center.y,
                "Actual interpolated middle bounds=" + middle);
            SampleAnimation("middle"); yield return Capture("nextwave-actual-middle", "Same real second wave popup while it moves toward the retained header; no animation-age fixture.");
            yield return WaitForAnnouncementAge(2f); yield return new WaitForEndOfFrame();
            Rect header = hud.WaveAnnouncementBoundsV513;
            Check("popup.settled-header", !hud.WaveAnnouncementActiveV513 && header == hud.WaveCardBounds && hud.WaveAnnouncementCountV513 == announcements + 1 &&
                hud.WaveAnnouncementTextV513 == w.Model.ActiveDisasterLabelV54, "Actual persistent header bounds=" + header);
            SampleAnimation("header"); yield return Capture("nextwave-actual-header", "Actual same-wave label has settled into the original header; count and chosen mask unchanged.");
            yield return Wait(.1f); yield return new WaitForEndOfFrame();
            hud.SetGuidanceExpandedV513(true); yield return new WaitForEndOfFrame();
            Check("tasks.compound-visible", hud.CriticalCardEligibleV513 && !string.IsNullOrEmpty(hud.CriticalRenderedTextV513) &&
                w.Model.CriticalTasksV513.Count == 5 && w.Model.CriticalTasksV513.Any(t => t.Id == "poweroff" && !t.Complete) &&
                w.Model.CriticalTasksV513.Any(t => t.Id == "roof" && !t.Complete), "Actual Flood+Storm core tasks: electricity, combined upper/roof, water, food, physical upper shelter.");
            yield return Capture("critical-compound-wave2", "Actual next-wave Flood+Storm critical panel and settled hazard header, missing power and roof tasks visible.");
        }
        IEnumerator WaitForAnnouncementAge(float required)
        {
            float until = Time.realtimeSinceStartup + 5;
            while (Read<float>(hud, "announcementAgeV513") < required && Time.realtimeSinceStartup < until) yield return null;
            Check("popup.real-age." + required, Read<float>(hud, "announcementAgeV513") >= required, "Observed real unscaled animation age=" + Read<float>(hud, "announcementAgeV513"));
        }
        void SampleAnimation(string name)
        { report.animation.Add(new AnimationSample { name = name, wave = w.Model.Wave, mask = w.Model.ActiveDisasterMaskV54, hazards = hud.WaveAnnouncementTextV513, age = Read<float>(hud, "announcementAgeV513"), bounds = hud.WaveAnnouncementBoundsV513 }); }
        IEnumerator UpperShelterAndCompound()
        {
            w.Model.TimeRemaining = w.Model.PhaseTime = 39f; w.TickPreparationStages(); yield return Wait(2.3f);
            foreach (string id in new[] { "water", "food" })
            {
                var supply = w.Expedition.WorldSupplies.FirstOrDefault(s => s.ItemId == id && s.Count > 0);
                Check("compound.finite-supply." + id, supply != null && w.Expedition.TryCollectWorldSupply(supply.Id), "Direct finite supplyAPI; no physical collection-route claim for this setup.");
            }
            bool entered = false;
            foreach (Vector3 pose in new[] { new Vector3(4.2f, 3.24f, -.65f), new Vector3(3.7f, 3.24f, -.65f), new Vector3(4.7f, 3.24f, -.65f) })
            {
                w.Warp(pose); report.setupWarps++;
                for (int n = 0; n < 8; n++) { w.SmokeMove(Vector2.zero, .025f); yield return null; }
                for (int n = 0; n < 3; n++) { Aim(w.ActiveShelterPositionV5 + Vector3.up * 1.35f); yield return null; Call(w, "RefreshBagRayFocus"); }
                w.SetFocus("@panicroom"); report.worldShelterCalls++;
                if (w.TryEnterActiveShelterV5()) { entered = true; break; }
            }
            Check("shelter.real-upper-admission", entered && w.Model.ReachedShelterV5 && w.Model.EnteredShelterV5 == PanicShelterKindV5.PanicRoom &&
                w.Model.CriticalTasksV513.First(t => t.Id == "shelter").Complete == w.Model.ReachedShelterV5,
                "Only World.TryEnterActiveShelterV5, actual grounded/floor/center-ray/path gate; no admission field write. " + w.Model.Message);
            if (!entered) throw new InvalidOperationException("INVALID_SETUP physical upper entry refused; no synthetic arrival substituted.");
            yield return null; w.InputLocked = true; Advance(8.001f); yield return null;
            Check("compound.resolution-penalty-cap-and-conservation", w.Model.Phase == PanicPhase.Aftermath &&
                w.Model.CriticalPenaltiesV513.Count == 2 && w.Model.CriticalPenaltyDamageV513 > 0 && w.Model.CriticalPenaltyDamageV513 <= 40.003f &&
                w.Model.CriticalPenaltiesV513.All(p => p.Id == "poweroff" || p.Id == "roof") && w.Model.TotalDamage <= 100.003f &&
                Near(w.Model.Items.Sum(i => i.Contribution), w.Model.TotalDamage) && Near(Enumerable.Range(0, 4).Sum(n => w.Model.HazardDamageContributionV54((DisasterType)n)), w.Model.TotalDamage),
                "Actual compound resolution: two attributed missing tasks share40 cap, final total100 cap; item and hazard ledgers conserve.");
            Check("compound.packed-entry-snapshot", w.Model.CriticalTasksV513.Where(t => t.Id == "water" || t.Id == "food" || t.Id == "shelter").All(t => t.Complete && t.Resolved),
                "Real finite supplies and real upper entry remain complete in the frozen pre-consumption snapshot.");
            Advance(4.001f); yield return null;
            var ledger = w.Model.LastWaveLedgerV56;
            Check("compound.actual-ledger-splits-penalties", ledger != null && ledger.Wave == 2 && Near(ledger.Rows.Sum(r => r.DamageContribution), w.Model.TotalDamage) &&
                Near(ledger.Rows.Where(r => r.Id.StartsWith("critical:", StringComparison.Ordinal)).Sum(r => r.DamageContribution), w.Model.CriticalPenaltyDamageV513),
                "Actual production outcome captures the ledger with critical task causes once; no manual ledger capture.");
        }
        void Aim(Vector3 target)
        {
            Vector3 direction = target - (w.PlayerPosition + Vector3.up * 1.62f); var angles = Quaternion.LookRotation(direction).eulerAngles;
            float yaw = Read<float>(w, "bagYaw"), pitch = Read<float>(w, "bagPitch");
            w.StepBagLook(new Vector2(Mathf.DeltaAngle(yaw, angles.y) / (2 * w.FpsLookMultiplierV414),
                (pitch - Mathf.DeltaAngle(0, angles.x)) / (1.7f * w.FpsLookMultiplierV414)));
        }
        static string Stock(PanicExpedition e) { return string.Join("|", PanicExpedition.MaterialIds.Select(id => id + ":" + e.Count(id))); }
        void RepairChecks()
        {
            var e = PanicEconomyChecksV59.New(51331); var m = e.Model;
            var upper = m.Items.Where(i => i.Installed && (i.Kind == ItemKind.Roof || i.Kind == ItemKind.Wall && Mathf.FloorToInt((i.Position.y + .15f) / 3.2f) == 1)).ToArray();
            foreach (var i in m.Items) { if (i.Kind != ItemKind.Furniture) i.Tier = 2; if (i.Kind == ItemKind.Furniture && i.Movable) i.Anchored = true; }
            m.UtilitySafety.Complete("gasoff"); m.UtilitySafety.Complete("poweroff"); m.Budget = 100000;
            var roll = typeof(PanicModel).GetMethod("UpgradeDowngradeRollV513", BindingFlags.Static | BindingFlags.NonPublic);
            uint seed = 1; while (seed < 100000 && (float)roll.Invoke(null, new object[] { seed, 1, "floor:1" }) >= .10f) seed++;
            Set(m, "compoundSeedV54", seed); PanicEconomyChecksV59.Gather(e, 14, 1);
            foreach (string item in new[] { "water", "food" }) { var supply = e.WorldSupplies.First(s => s.ItemId == item && s.Count > 0); e.TryCollectWorldSupply(supply.Id); }
            bool entered = m.TryEnterShelterV5(m.RequiredShelterV5); m.Tick(8f);
            Check("repair.actual-resolution-drops-upper-roof", entered && m.Phase == PanicPhase.Aftermath && upper.All(i => i.Tier == 1) &&
                m.UpgradeDowngradeOutcomesV513.Count(o => o.GroupId == "floor:1") == upper.Length,
                "Declared seeded/funded/T2 structural MODEL fixture; actual TryEnterShelter/Tick resolution drops real linked upper group once.");
            string tiers = string.Join("|", m.Items.Select(i => i.Id + ":" + i.Tier)); int count = m.UpgradeDowngradeOutcomesV513.Count, rng = m.SecondaryRollCountV5;
            for (int n = 0; n < 20; n++) { var a = m.UpgradeDowngradeOutcomesV513; var b = m.UpgradeDowngradeSummaryV513; var c = m.CriticalTasksV513; }
            Call(m, "ResolveDisaster");
            Check("repair.reads-and-repeat-resolution-idempotent", tiers == string.Join("|", m.Items.Select(i => i.Id + ":" + i.Tier)) && count == m.UpgradeDowngradeOutcomesV513.Count && rng == m.SecondaryRollCountV5,
                "Repeated committed-outcome/task/UI reads plus actual resolver guard cannot reroll or lose another tier.");
            m.Tick(4f); m.Tick(3f); m.SetWaveLedgerOpenV56(true); m.SetWaveLedgerOpenV56(false); m.FinishInspection(); e.OnNewWave(); PanicEconomyChecksV59.Gather(e, 14, 2);
            PanicFloorWallUpgradePlanV414 quote; bool quoted = e.TryGetFloorWallUpgradeV414(upper.First(i => i.Kind == ItemKind.Wall).Id, out quote);
            Check("repair.actual-tier-real-quote", m.Phase == PanicPhase.Preparing && m.Wave == 2 && quoted && quote.TargetTier == 2 && quote.IncludesRoof &&
                quote.BudgetCost == upper.Sum(i => PanicExpedition.WorkCost("upgrade", i)),
                "Actual next-wave quote uses nowT1 members and T1→T2 recipe; no priorT2 discount or independent roof work.");
            if (!quoted) throw new InvalidOperationException("Repair quote unavailable after actual next-wave progression.");
            int cash = m.Budget; var stock = PanicExpedition.MaterialIds.ToDictionary(id => id, id => e.Count(id)); string idWall = upper.First(i => i.Kind == ItemKind.Wall).Id;
            bool began = e.BeginValidatedWorkV414("upgrade", idWall, false); if (began) e.Work.PressHammer(); e.Work.Cancel(); e.CancelFloorWallUpgradeV414();
            Check("repair.partial-cancel-free", began && cash == m.Budget && stock.All(p => e.Count(p.Key) == p.Value) && upper.All(i => i.Tier == 1),
                "Actual partial repair cancellation is free and cannot restore any member.");
            bool done = PanicEconomyChecksV59.Complete(e, "upgrade", idWall);
            Check("repair.normal-cost-exact-once", done && upper.All(i => i.Tier == 2) && m.Budget == cash - quote.BudgetCost &&
                stock.All(p => e.Count(p.Key) == p.Value - quote.MaterialCosts.Where(c => c.Id == p.Key).Sum(c => c.Required)) && !m.CheatedRun && !m.DeveloperAnyEnabled,
                "Actual completion charges real post-drop combined recipe once; funded budget fixture uses normal money/material rules.");
            cash = m.Budget; tiers = Stock(e);
            Check("repair.repeat-commit-no-charge", !e.TryCompleteWork("upgrade", idWall) && m.Budget == cash && Stock(e) == tiers,
                "Duplicate repair completion is rejected without another debit.");
        }
        IEnumerator Guard(IEnumerator body)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(body);
            while (stack.Count > 0 && !finished)
            {
                bool more; object current = null;
                try { more = stack.Peek().MoveNext(); if (more) current = stack.Peek().Current; }
                catch (Exception error) { report.error = error.ToString(); break; }
                if (!more) { stack.Pop(); continue; } if (current is IEnumerator nested) stack.Push(nested); else yield return current;
            }
            Finish();
        }
        void Update() { if (!finished && Time.realtimeSinceStartup - began > 180f) { report.error = "Bounded180s driver timeout"; Finish(); } }
        void Finish()
        {
            if (finished) return; finished = true;
            try { hud.SetPaused(false); w.ReturnToMenu(); } catch (Exception error) { report.errors.Add("Cleanup:" + error); }
            report.realScoreAfter = Hash(realScore); Check("profile.real-score-unchanged", report.realScoreBefore == report.realScoreAfter);
            report.seconds = Time.realtimeSinceStartup - began;
            report.passed = report.complete && string.IsNullOrEmpty(report.error) && report.errors.Count == 0 && report.captures.Count >= 14 && report.checks.All(c => c.pass);
            Application.logMessageReceived -= Log;
            File.WriteAllText(Path.Combine(w.EvidencePath, "critical-v513-smoke.json"), JsonUtility.ToJson(report, true));
            Debug.Log("CRITICAL_V513_RESULT " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " captures=" + report.captures.Count);
            Application.Quit(report.passed ? 0 : 2);
        }
    }
}
