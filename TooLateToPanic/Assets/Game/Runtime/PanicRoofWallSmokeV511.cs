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
    // Isolated production API/framebuffer driver, derived from PanicOnboardingSmokeV510.
    // Actor/camera fixtures are declared; this is not an OS-input or natural traversal claim.
    [DefaultExecutionOrder(10000)]
    public sealed class PanicRoofWallSmokeV511 : MonoBehaviour
    {
        const string Flag = "--roof-wall-v511-smoke";
        const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public class Row { public string id, detail; public bool pass; }
        [Serializable] public class Shot { public string path, sha256, context; public int width, height, nonDark; }
        [Serializable] public class Report
        {
            public string utc, dllSha256, scorePath, realScoreBefore, realScoreAfter, error;
            public string layer = "CURRENT_DLL_NATIVE_PRODUCTION_API_AND_FRAMEBUFFER_WITH_DECLARED_FIXTURES";
            public bool complete, passed, OSInput, humanAcceptance;
            public float seconds;
            public int productionStarts, setupWarps, sourceCalls, workBegins, hammerInputs, workCommits, modelApiRows;
            public List<Row> checks = new List<Row>();
            public List<Shot> captures = new List<Shot>();
            public List<string> fixtures = new List<string>();
            public List<string> errors = new List<string>();
        }
        readonly Report report = new Report();
        PanicHomeWorld w; PanicHomeHud hud;
        float began; bool finished, fixedExteriorCamera; string realScore;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (world) world.gameObject.AddComponent<PanicRoofWallSmokeV511>();
        }
        void Start()
        {
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); began = Time.realtimeSinceStartup;
            if (!w || !hud || string.IsNullOrEmpty(w.EvidencePath)) { Debug.LogError("ROOF_WALL_V511 isolated evidence/World/HUD required"); Application.Quit(2); return; }
            report.utc = DateTime.UtcNow.ToString("o"); report.dllSha256 = Hash(typeof(PanicModel).Assembly.Location);
            report.scorePath = PanicModel.ScorePathOverride; realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.realScoreBefore = Hash(realScore);
            Application.logMessageReceived += Log; Application.runInBackground = true;
            StartCoroutine(Guard(Run()));
        }
        void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Check(string id, bool pass, string detail = "")
        { report.checks.Add(new Row { id = id, pass = pass, detail = detail }); Debug.Log("ROOF_WALL_V511 " + (pass ? "PASS " : "FAIL ") + id + " " + detail); }
        static string Hash(string path)
        { if (!File.Exists(path)) return "absent"; using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""); }
        static void Set(object target, string name, object value) { target.GetType().GetField(name, Fields).SetValue(target, value); }
        static T Read<T>(object target, string name)
        { var type = target.GetType(); var property = type.GetProperty(name, Fields); return (T)(property != null ? property.GetValue(target) : type.GetField(name, Fields).GetValue(target)); }
        static object Call(object target, string name, params object[] args)
        { return target.GetType().GetMethods(Fields).First(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(target, args); }
        IEnumerator Wait(float seconds) { float until = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < until) yield return null; }
        IEnumerator Resolution(int width, int height)
        { Screen.SetResolution(width, height, FullScreenMode.Windowed); yield return Wait(.3f); Check("resolution." + width, Screen.width == width && Screen.height == height, "Actual framebuffer=" + Screen.width + "x" + Screen.height); }
        IEnumerator Capture(string name, string context)
        {
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (!texture) { Check("capture." + name, false, "Missing framebuffer"); yield break; }
            string path = Path.Combine(w.EvidencePath, name + ".png"); File.WriteAllBytes(path, texture.EncodeToPNG());
            var pixels = texture.GetPixels32(); int nonDark = 0; byte min = 255, max = 0;
            for (int i = 0; i < pixels.Length; i += 137) { var p = pixels[i]; byte l = (byte)((p.r + p.g + p.b) / 3); if (l > 12) nonDark++; min = Math.Min(min, l); max = Math.Max(max, l); }
            Check("capture." + name, nonDark > 80 && max - min > 18, "Actual framebuffer " + texture.width + "x" + texture.height + "; nonDark=" + nonDark);
            report.captures.Add(new Shot { path = path, sha256 = Hash(path), context = context, width = texture.width, height = texture.height, nonDark = nonDark }); Destroy(texture);
        }
        IEnumerator BeginNormal()
        {
            hud.SetPaused(false); w.ReturnToMenu(); w.StartGame(false); report.productionStarts++; w.FullFlowScriptedInput = true; yield return null;
            if (hud.FreeplayOnboardingOpenV414) hud.ConfirmFreeplayOnboardingV414(); yield return null;
            w.SkipIntroStoryV414(); yield return Wait(.25f);
            Check("world.normal-ready", !w.Model.IsPaused && !w.InputLocked && !w.CinematicActive && w.Model.Phase == PanicPhase.Preparing &&
                w.PlayerControllerEnabled && !w.Model.CheatedRun && !w.Model.DeveloperAnyEnabled,
                "Actual normal StartGame, onboarding acknowledgement and intro skip; original controller/cost gates retained.");
        }
        IEnumerator Run()
        {
            bool isolated = report.scorePath == Path.Combine(w.EvidencePath, "isolated-home-scores.json");
            Check("isolated", isolated, report.scorePath);
            if (!isolated) throw new InvalidOperationException("Fresh isolated score profile required before model/API tests.");
            report.fixtures.Add("Fresh GUID profile; production normal StartGame/onboarding acknowledgement/intro skip. FullFlowScriptedInput suppresses physical input. No prior player, OS input, human acceptance or natural route claim.");
            report.fixtures.Add("Production Model/Expedition regression suite uses its documented budget/tier/stale-state fixtures; those detached models are separate from the live World below.");
            report.fixtures.Add("Live World uses only its real1500 starting budget, actual finite ToggleSource14 collection, normal work cost/hammer/commit/cancel. No World budget/material/tier/time grants, developer options, controller/range/path bypass or fake roof upgrade.");
            report.fixtures.Add("Live actor setup searches at most16 angles at1.3m around each real wall, with production Warp + enabled-controller settling. Exterior setup=(0,.05,-9). Final camera alone is fixed in LateUpdate to show actual roof hardware; original visibility API still controls scene meshes.");
            w.FullFlowScriptedInput = true;
            int before = report.checks.Count; PanicRoofWallChecksV511.Run(Check); report.modelApiRows = report.checks.Count - before;
            yield return Resolution(1600, 900);
            yield return FeatureChecks();
            Check("runtime.no-errors", report.errors.Count == 0, string.Join("\n", report.errors)); report.complete = true;
        }
        IEnumerator FeatureChecks()
        {
            yield return BeginNormal();
            foreach (var source in w.Expedition.Sources)
            {
                bool collected = w.Expedition.ToggleSource(source.Id); report.sourceCalls++;
                Check("world.finite-collect." + source.Id, collected && source.AutoLooted, "Actual finite source API; not a controller traversal claim.");
            }
            foreach (var source in w.Expedition.Sources) if (source.Open) { w.Expedition.ToggleSource(source.Id); report.sourceCalls++; }
            var upper = w.Model.Items.First(i => i.Id == "wall_south_upper");
            yield return ReachWall(upper);
            if (w.Selected != upper || !w.CanInteract) throw new InvalidOperationException("INVALID_SETUP: no reachable upper-wall pose found within16 angles.");
            var roofIds = w.Model.Items.Where(i => i.Kind == ItemKind.Roof && i.Installed).Select(i => i.Id).ToArray();
            string roofId = roofIds[0]; w.SetFocus(roofId);
            Check("world.roof-explicit-focus-rejected", w.Selected == null && Read<string>(w, "focusId") == null,
                "Actual SetFocus sanitizes the real installed roof ID.");
            Call(w, "ResetAutoFocusV510"); var automatic = w.RefreshAutoFocusV510(false);
            Check("world.roof-auto-excluded", automatic.InputAllowed && automatic.Candidates.All(c => !roofIds.Contains(c.Id)) && !roofIds.Contains(automatic.FocusId),
                "Actual native auto-focus candidate construction; roof never enters the candidate set.");
            int candidates = automatic.Candidates.Count(c => c.Eligible); bool noRoof = true;
            for (int n = 0; n < candidates + 1; n++) { var cycle = w.RefreshAutoFocusV510(true); noRoof &= !roofIds.Contains(cycle.FocusId) && cycle.Candidates.All(c => !roofIds.Contains(c.Id)); }
            Check("world.roof-q-excluded", noRoof && candidates > 0, "Actual Q selector cycles all eligible local candidates; count=" + candidates);
            w.SetFocus(upper.Id); yield return Wait(.16f);
            PanicFloorWallUpgradePlanV414 quote;
            bool quoted = w.Expedition.TryGetFloorWallUpgradeV414(upper.Id, out quote);
            Check("world.upper-combined-quote", quoted && quote.Floor == 1 && quote.TargetTier == 1 && quote.RoofCount == roofIds.Length &&
                quote.RoofIds.OrderBy(i => i).SequenceEqual(roofIds.OrderBy(i => i)) && quote.ChangedMemberCount == quote.WallCount + quote.RoofCount &&
                quote.BudgetCost == quote.ChangedMemberCount * 30 && quote.Label.Contains("Çatı") && w.CanInteract,
                "Actual selected upper wall quotes installed walls plus roof at their actual T0 recipe/cash price.");
            if (!quoted) throw new InvalidOperationException("Combined upper quote unavailable.");
            yield return Capture("upper-wall-roof-quote-1600", "Real selected reachable upper-wall preview and combined roof+wall cost,1600x900; original1500 budget and finite gathered stock.");

            int budget = w.Model.Budget; var stock = Stock(); var tiers = w.Model.Items.ToDictionary(i => i.Id, i => i.Tier);
            w.BeginWork("upgrade", upper.Id); report.workBegins++; yield return null;
            Check("world.upper-real-work", w.WorkActive && w.Expedition.Work.UsesHammer && w.Expedition.ActiveFloorWallUpgradeV414 != null &&
                w.Model.Budget == budget && EqualStock(stock) && TiersEqual(tiers),
                "Production World.BeginWork accepted original enabled-controller floor/range/path/preflight gates.");
            if (!w.WorkActive) throw new InvalidOperationException("INVALID_SETUP: World.BeginWork refused reachable upper wall: " + w.Model.Message);
            w.StepWork(.025f, 0, false, false, true); report.hammerInputs++; yield return null;
            Check("world.partial-hammer-unspent", w.WorkActive && w.Expedition.Work.CompletedStrokes == 1 && !w.Expedition.Work.Complete &&
                w.Model.Budget == budget && EqualStock(stock) && TiersEqual(tiers), "One separate real hammer input; no member installed early.");
            yield return Capture("upper-wall-roof-hammer-1600", "Actual in-world hammer construction after1of4 production strokes; combined member plan remains uncommitted.");
            w.CancelWork(); yield return null;
            Check("world.partial-cancel-free", !w.WorkActive && w.Expedition.ActiveFloorWallUpgradeV414 == null &&
                w.Model.Budget == budget && EqualStock(stock) && TiersEqual(tiers), "Actual World.CancelWork preserves every budget/material/tier.");
            w.BeginWork("upgrade", upper.Id); report.workBegins++; yield return null;
            Check("world.upper-restart-same-cost", w.WorkActive && w.Expedition.ActiveFloorWallUpgradeV414 != null &&
                w.Expedition.ActiveFloorWallUpgradeV414.BudgetCost == quote.BudgetCost, "No partial-work discount or independent roof work.");
            for (int n = 0; n < 4 && w.WorkActive; n++) { w.StepWork(.025f, 0, false, false, true); report.hammerInputs++; yield return null; }
            var included = new HashSet<string>(quote.WallIds.Concat(quote.RoofIds));
            bool exactTiers = w.Model.Items.All(i => i.Tier == (included.Contains(i.Id) ? Math.Max(tiers[i.Id], quote.TargetTier) : tiers[i.Id]));
            bool exactStock = stock.All(pair => w.Expedition.Count(pair.Key) == pair.Value - quote.MaterialCosts.Where(c => c.Id == pair.Key).Sum(c => c.Required));
            bool committed = !w.WorkActive && exactTiers && exactStock && w.Model.Budget == budget - quote.BudgetCost;
            if (committed) report.workCommits++;
            Check("world.upper-roof-atomic-exact-once", committed && !w.Model.CheatedRun && !w.Model.DeveloperAnyEnabled,
                "Four actual strokes trigger World.CompleteWork; all upper walls and roof reachT1 in one normal-cost transaction, unrelated tiers preserved.");
            var committedStock = Stock(); int committedBudget = w.Model.Budget; w.CompleteWork(); yield return null;
            Check("world.repeat-complete-no-charge", committedBudget == w.Model.Budget && EqualStock(committedStock) && exactTiers,
                "Repeated actual World completion cannot debit the combined transaction again.");
            PanicFloorWallUpgradePlanV414 next;
            Check("world.next-tier-real-budget-gate", w.Expedition.TryGetFloorWallUpgradeV414(upper.Id, out next) && next.TargetTier == 2 &&
                next.BudgetCost == 1700 && !w.Expedition.CanBeginWork("upgrade", upper.Id),
                "Normal first-wave budget cannot fundT2; detached model checks cover fundedT2 without changing this World budget.");

            yield return Resolution(960, 540); var lower = w.Model.Items.First(i => i.Id == "wall_north"); yield return ReachWall(lower);
            PanicFloorWallUpgradePlanV414 lowerQuote;
            bool lowerQuoted = w.Expedition.TryGetFloorWallUpgradeV414(lower.Id, out lowerQuote);
            Check("world.lower-wall-only-quote", lowerQuoted && lowerQuote.Floor == 0 && lowerQuote.RoofCount == 0 && lowerQuote.RoofIds.Count == 0 &&
                lowerQuote.ChangedRoofCount == 0 && lowerQuote.BudgetCost == lowerQuote.ChangedWallCount * 30 && !lowerQuote.Label.Contains("Çatı") &&
                w.Selected == lower && w.CanInteract && w.Model.Items.Where(i => included.Contains(i.Id)).All(i => i.Tier == 1),
                "Actual ground-floor quote excludes the already reinforced roof and leaves the upper bundle intact.");
            yield return Wait(.18f); yield return Capture("lower-wall-quote-960", "Real reachable ground-wall preview,960x540; wall-only total, upgraded upper floor/roof retained.");

            yield return Resolution(1600, 900); w.SetFocus(null); w.Warp(new Vector3(0, .05f, -9)); report.setupWarps++;
            yield return null; for (int n = 0; n < 3; n++) { w.SmokeMove(Vector2.zero, .025f); yield return null; }
            fixedExteriorCamera = true; Call(w, "SyncVisuals", true); Call(w, "ApplyFloorVisibility"); yield return Wait(.2f);
            var hardware = Read<Dictionary<string, GameObject>>(w, "hardware"); GameObject holder;
            var frame = hardware.TryGetValue(roofId, out holder) && holder ? holder.GetComponentsInChildren<Renderer>(true).Where(r => r.name.StartsWith("Roof_", StringComparison.Ordinal)).ToArray() : new Renderer[0];
            int visibleFrame = frame.Count(r => r && r.enabled && r.gameObject.activeInHierarchy && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(w.View), r.bounds));
            Check("world.roof-paid-hardware-rendered", w.Model.Items.Find(i => i.Id == roofId).Tier == 1 && frame.Length >= 5 && visibleFrame > 0,
                "Real paidT1 roof hardware; frame renderers=" + frame.Length + "; enabled active frustum-intersecting=" + visibleFrame + ". Pixel review remains separate.");
            Check("world.roof-barriers-retained", w.RoofAccessR6 && w.RoofAccessR6.EnabledBarrierCount == 2 && w.RoofAccessR6.LayerCollisionEnabled &&
                !w.RoofAccessR6.MainBarrier.isTrigger && !w.RoofAccessR6.GarageBarrier.isTrigger && w.PlayerControllerEnabled,
                "Both permanent actual roof guards and enabled original controller remain in force.");
            yield return Capture("reinforced-exterior-roof-1600", "Actual paidT1 roof reinforcement from exterior actor(0,.05,-9); declared fixed presentation camera; real visibility and both permanent roof barriers preserved.");
            fixedExteriorCamera = false;
        }
        IEnumerator ReachWall(PanicItem item)
        {
            w.CancelWork();
            for (int a = 0; a < 16; a++)
            {
                float angle = a * Mathf.PI / 8;
                Vector3 pose = item.Position + new Vector3(Mathf.Cos(angle) * 1.3f, .05f, Mathf.Sin(angle) * 1.3f);
                report.setupWarps++; w.Warp(pose); Physics.SyncTransforms(); yield return null;
                for (int n = 0; n < 3; n++) { w.SmokeMove(Vector2.zero, .025f); yield return null; }
                var hero = Read<Transform>(w, "hero"); Vector3 toward = item.Position - w.PlayerPosition; toward.y = 0;
                if (toward.sqrMagnitude > .001f) { float yaw = Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg; hero.rotation = Quaternion.Euler(0, yaw, 0); Set(w, "facingYaw", yaw); }
                w.SetFocus(item.Id); yield return null;
                if (w.Selected == item && w.CanInteract && w.ActiveFloor == Mathf.FloorToInt((item.Position.y + .15f) / 3.2f))
                { report.fixtures.Add("Reachable wall pose " + item.Id + " angleIndex=" + a + " actualActor=" + w.PlayerPosition); yield break; }
            }
            Check("world.reachable-pose." + item.Id, false, "INVALID_SETUP: all16 real range/floor/path pose tests rejected; no bypass attempted.");
        }
        Dictionary<string, int> Stock() { return PanicExpedition.MaterialIds.ToDictionary(id => id, id => w.Expedition.Count(id)); }
        bool EqualStock(Dictionary<string, int> expected) { return expected.All(pair => w.Expedition.Count(pair.Key) == pair.Value); }
        bool TiersEqual(Dictionary<string, int> expected) { return w.Model.Items.All(i => i.Tier == expected[i.Id]); }
        void LateUpdate()
        {
            if (!fixedExteriorCamera || !w || !w.View) return;
            Vector3 aim = new Vector3(0, 4.6f, 0); w.View.transform.position = aim + new Vector3(13, 10, -19); w.View.transform.LookAt(aim);
            w.View.orthographic = true; w.View.orthographicSize = 10.5f; Call(w, "ApplyFloorVisibility");
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
            if (finished) return; finished = true; fixedExteriorCamera = false; hud.SetPaused(false); w.ReturnToMenu();
            report.realScoreAfter = Hash(realScore); Check("profile.real-score-unchanged", report.realScoreBefore == report.realScoreAfter);
            report.seconds = Time.realtimeSinceStartup - began;
            report.passed = report.complete && string.IsNullOrEmpty(report.error) && report.errors.Count == 0 && report.captures.Count >= 4 && report.checks.All(c => c.pass);
            Application.logMessageReceived -= Log; File.WriteAllText(Path.Combine(w.EvidencePath, "roof-wall-v511-smoke.json"), JsonUtility.ToJson(report, true));
            Debug.Log("ROOF_WALL_V511_RESULT " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " captures=" + report.captures.Count); Application.Quit(report.passed ? 0 : 2);
        }
    }
}
