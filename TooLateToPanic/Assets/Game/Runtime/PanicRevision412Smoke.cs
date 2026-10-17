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
    /// <summary>Opt-in V4.12 native API/render driver. Root alone imports/builds/runs it.</summary>
    public sealed class PanicRevision412Smoke : MonoBehaviour
    {
        public const string Flag = "--revision412-smoke";
        const float HardCapSeconds = 180f;
        const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id, detail; public bool pass; }
        [Serializable] public sealed class HitProbe
        {
            public string collider, hierarchy, pickId, pickKind;
            public float distance;
            public Vector3 point, normal;
            public Bounds bounds;
            public bool trigger, enabled, active;
        }
        [Serializable] public sealed class InteractionProbe
        {
            public string label, target, focus, phase, message;
            public int frame;
            public bool canInteract, pathClear, hasSurface, hasBounds;
            public float surfaceDistance;
            public Vector3 actor, eye, surface, artCenter, itemPosition, itemSize;
            public Bounds targetBounds;
            public List<HitProbe> targetColliders = new List<HitProbe>(), surfaceRay = new List<HitProbe>(), artCenterRay = new List<HitProbe>(), highEyeRay = new List<HitProbe>();
        }
        [Serializable] public sealed class Frame
        {
            public string label, phase, kind, focus;
            public int frame, width, height, supplyRoots, supplyColliders, supplyRenderers, roofRenderers;
            public float remaining, cinematicAge, lidAngle;
            public Vector3 actor, camera;
        }
        [Serializable] public sealed class Receipt
        {
            public string startedUtc, finishedUtc, error, realScoreBefore, realScoreAfter, scorePath;
            public string evidence = "Native production APIs, explicit setup fixtures, actual frame waits, enabled colliders and rendered captures. No OS input or human acceptance.";
            public bool complete, passed;
            public float seconds;
            public int setupWarps, startGameCalls, simulationStartCalls, manualWorkTickFrames, modelFixtureTicks;
            public List<Row> rows = new List<Row>();
            public List<string> fixtures = new List<string>(), errors = new List<string>(), captures = new List<string>();
            public List<Frame> frames = new List<Frame>();
            public List<InteractionProbe> interactionProbes = new List<InteractionProbe>();
        }
        readonly Receipt report = new Receipt();
        PanicHomeWorld w;
        PanicHomeHud hud;
        CharacterController controller;
        CapsuleCollider capsuleQuery;
        Coroutine sequence;
        float began;
        bool finished;
        string realScore;
        static float Dt { get { return Mathf.Max(.0001f, Mathf.Min(Time.unscaledDeltaTime, .08f)); } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Attach()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), Flag) < 0) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (!world) throw new InvalidOperationException("V4.12 smoke needs the production Residence World.");
            world.gameObject.AddComponent<PanicRevision412Smoke>();
        }
        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), Flag) < 0) { enabled = false; return; }
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>();
            controller = Field<CharacterController>("controller");
            began = Time.realtimeSinceStartup; report.startedUtc = DateTime.UtcNow.ToString("o");
            realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.realScoreBefore = Hash(realScore); report.scorePath = PanicModel.ScorePathOverride;
            Application.runInBackground = true; Application.logMessageReceived += OnLog;
            sequence = StartCoroutine(Guarded(Run()));
        }
        IEnumerator Guarded(IEnumerator body)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(body);
            while (stack.Count > 0 && !finished)
            {
                bool more; object next = null;
                try { more = stack.Peek().MoveNext(); if (more) next = stack.Peek().Current; }
                catch (Exception ex) { report.error = ex.ToString(); Check("harness.exception", false, ex.Message); break; }
                if (!more) { stack.Pop(); continue; }
                if (next is IEnumerator nested) stack.Push(nested); else yield return next;
            }
            report.complete = string.IsNullOrEmpty(report.error); Finish();
        }
        void Update()
        {
            if (!finished && Time.realtimeSinceStartup - began > HardCapSeconds)
            {
                report.error = "180-second watchdog. Unexecuted checks are NOT_RUN.";
                if (sequence != null) StopCoroutine(sequence); Finish();
            }
        }
        void OnLog(string message, string trace, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + trace); }
        void Check(string id, bool pass, string detail = "")
        { report.rows.Add(new Row { id = id, pass = pass, detail = detail }); Debug.Log("REVISION412 " + (pass ? "PASS " : "FAIL ") + id + " " + detail); }
        T Field<T>(string name) { return (T)typeof(PanicHomeWorld).GetField(name, Instance).GetValue(w); }
        object Call(string name, params object[] args)
        {
            var method = typeof(PanicHomeWorld).GetMethod(name, Instance);
            if (method == null) throw new MissingMethodException("PanicHomeWorld", name);
            return method.Invoke(w, args);
        }
        bool FinalStage { get { return (bool)typeof(PanicHomeWorld).GetProperty("BagFinalStageActive", Instance).GetValue(w); } }
        string Focus { get { return Field<string>("focusId"); } }
        Dictionary<string, GameObject> SupplyRoots { get { return Field<Dictionary<string, GameObject>>("worldSupplyObjects"); } }
        void Fixture(string detail) { report.fixtures.Add(detail); }
        IEnumerator Place(Vector3 position, string focus, string purpose)
        {
            w.Warp(position); report.setupWarps++; Fixture("Warp setup: " + purpose + "; " + position);
            float until = Time.realtimeSinceStartup + 2.5f; int frames = 0;
            do { w.SmokeMove(Vector2.zero, Dt); frames++; yield return null; }
            while ((!w.IsGrounded || frames < 3) && Time.realtimeSinceStartup < until);
            if (!w.IsGrounded) throw new InvalidOperationException("Real controller did not ground for: " + purpose);
            if (focus != null) w.SetFocus(focus);
            yield return null;
        }
        IEnumerator Wait(float seconds)
        { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
        IEnumerator WaitWork(float maximum = 3f)
        {
            float end = Time.realtimeSinceStartup + maximum;
            while (w.WorkActive && Time.realtimeSinceStartup < end)
            { w.StepWork(Dt, 0, false, false, false); report.manualWorkTickFrames++; yield return null; }
            if (w.WorkActive) throw new InvalidOperationException("Real frame-driven installation did not complete.");
        }
        IEnumerator SettleBag()
        {
            float end = Time.realtimeSinceStartup + 5f;
            while ((!w.BagFirstPersonActive || w.BagCameraTransitionActive) && Time.realtimeSinceStartup < end) yield return null;
            if (!w.BagFirstPersonActive || w.BagCameraTransitionActive) throw new InvalidOperationException("Final30 camera did not settle.");
            yield return null;
        }
        IEnumerator Shot(string label)
        {
            Observe(label); yield return new WaitForEndOfFrame();
            string path = Path.Combine(w.EvidencePath, label + ".png"); ScreenCapture.CaptureScreenshot(path);
            float end = Time.realtimeSinceStartup + 2;
            while ((!File.Exists(path) || new FileInfo(path).Length < 256) && Time.realtimeSinceStartup < end) yield return null;
            Check("capture." + label, File.Exists(path) && new FileInfo(path).Length >= 256, path);
            report.captures.Add(path);
        }
        void Observe(string label)
        {
            var roots = SupplyRoots.Values.Where(g => g).ToArray();
            report.frames.Add(new Frame { label = label, frame = Time.frameCount, width = Screen.width, height = Screen.height,
                phase = w.Model.Phase.ToString(), kind = w.CinematicKind, focus = Focus, remaining = w.Model.TimeRemaining,
                actor = w.PlayerPosition, camera = w.View.transform.position, cinematicAge = Field<float>("residenceCinematicAge"),
                lidAngle = LidAngle(), supplyRoots = roots.Count(g => g.activeInHierarchy),
                supplyColliders = roots.SelectMany(g => g.GetComponentsInChildren<Collider>(true)).Count(c => c.enabled && c.gameObject.activeInHierarchy),
                supplyRenderers = roots.SelectMany(g => g.GetComponentsInChildren<Renderer>(true)).Count(r => r.enabled && r.gameObject.activeInHierarchy),
                roofRenderers = RoofRenderers().Count(r => r.enabled && r.gameObject.activeInHierarchy) });
        }
        IEnumerator Run()
        {
            if (!w || string.IsNullOrEmpty(w.EvidencePath) || string.IsNullOrEmpty(PanicModel.ScorePathOverride))
                throw new InvalidOperationException("Fresh isolated --home-evidence must be applied before model creation.");
            string profile = Path.GetFileName(w.EvidencePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)); Guid id;
            Check("launch.fresh-guid", profile.Length >= 36 && Guid.TryParseExact(profile.Substring(profile.Length - 36), "D", out id));
            Check("launch.score-isolated", Path.GetFullPath(PanicModel.ScorePathOverride) == Path.Combine(Path.GetFullPath(w.EvidencePath), "isolated-home-scores.json") && !File.Exists(PanicModel.ScorePathOverride));
            Check("launch.driver-registered", PanicTestLaunchRules.Drivers.Contains(Flag));
            while (!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            var modelChecks = typeof(PanicModel).Assembly.GetType("TooLateToPanic.PanicRevisionV412ModelChecks", true);
            foreach (string passed in (IEnumerable<string>)modelChecks.GetMethod("Run", BindingFlags.Public | BindingFlags.Static).Invoke(null, null))
                Check("model412." + passed, true, "Mechanics-owned model fixture check; not an OS/world-route observation");
            ModelBoundaries();
            MakeCapsuleQuery();
            w.ReturnToMenu(); w.StartGame(false); report.startGameCalls++; w.FullFlowScriptedInput = true;
            yield return Wait(.1f);
            Check("phase.initial-isometric", !FinalStage && !w.BagCameraOwnsView && w.View.orthographic);
            Check("phase.early-manual-entry-rejected", !w.BeginBagPreparation() && !w.BagCameraOwnsView);
            Call("RefreshPreparationVisibility"); CheckSupplies(false, "building");
            Check("supplies.data-separation", w.Expedition.WorldSupplies.All(s => !PanicExpedition.IsConstructionMaterial(s.ItemId)) &&
                w.Expedition.Sources.All(s => s.Contents.All(c => PanicExpedition.IsConstructionMaterial(c.Id))));
            int looseBefore = w.Expedition.Count("water");
            w.SetFocus("supply_water_kitchen"); Check("supplies.building-cannot-collect", !w.TakeWorldSupply() && w.Expedition.Count("water") == looseBefore);

            // One real cabinet opening proves finite currency loot; further openings below are declared setup.
            yield return Place(new Vector3(4.1f, .04f, 3.1f), "cabinet", "real construction-container transaction");
            int screws = w.Expedition.Count("screws"); w.HandleKey(KeyCode.E); yield return Wait(.15f);
            Check("materials.real-container-loot", w.Expedition.Sources.Find(s => s.Id == "cabinet").Open && w.Expedition.Count("screws") > screws && !w.LootVisible);
            string paid = Stock(); w.ToggleContainer(); w.ToggleContainer();
            Check("materials.no-reroll", paid == Stock());
            foreach (var source in w.Expedition.Sources)
                if (!source.AutoLooted) { w.Expedition.ToggleSource(source.Id); Fixture("Direct finite source opening setup: " + source.Id); }
            yield return AnchorTransaction();
            yield return StableStructures();
            yield return TreesAndFence();
            var floorLamps = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(t => t.name == "decor_lounge_lamp" || t.name == "decor_upper_lamp").ToArray();
            Check("floorlamp.fixed-or-removed", floorLamps.All(t => t.gameObject.activeInHierarchy && !t.GetComponent<HomePick>() && !t.GetComponent<Rigidbody>()),
                "retained fixed decor count=" + floorLamps.Length + "; visual shape requires capture review");

            // Exact boundary values are explicit time fixtures. Camera and pause clocks use real frames.
            yield return Place(new Vector3(4.1f, .04f, 3.1f), "cabinet", "same-position automatic final30 handoff");
            w.Model.TimeRemaining = 31f; Fixture("TimeRemaining=31; no model Tick injection"); Call("TickPreparationStages");
            Check("phase.31-seconds-building", !FinalStage && !w.BeginBagPreparation() && !w.BagCameraOwnsView);
            hud.SetPaused(true); w.Model.TimeRemaining = 30f; Fixture("Paused TimeRemaining=30 boundary setup");
            Vector3 actor = w.PlayerPosition; var body = controller; Call("TickPreparationStages"); yield return Wait(.3f);
            Check("phase.paused-30-no-entry", FinalStage && !w.BagCameraOwnsView && Mathf.Abs(w.Model.TimeRemaining - 30f) < .001f);
            hud.SetPaused(false); Call("TickPreparationStages");
            Check("phase.30-automatic-transition", w.BagCameraTransitionActive && w.BagCameraOwnsView);
            yield return SettleBag();
            Check("phase.same-player-controller", controller == body && w.PlayerControllerEnabled && Vector3.Distance(actor, w.PlayerPosition) < .00001f);
            Check("phase.final30-perspective", w.BagFirstPersonActive && !w.View.orthographic && Mathf.Abs(w.View.fieldOfView - 62f) < .01f);
            Check("phase.no-g-return", !w.FinishBagPreparation()); w.HandleKey(KeyCode.G); yield return null;
            Check("phase.g-keeps-forced-stage", w.BagFirstPersonActive && !w.BagCameraTransitionActive);
            Call("RefreshPreparationVisibility"); CheckSupplies(true, "final30");
            Check("containers.closed-on-transition", w.Expedition.Sources.All(s => !s.Open));
            yield return Shot("06-fps-clean-hud");
            yield return FpsTransactions();
            yield return FpsBunkerAndCinematic();
            foreach (DisasterType type in new[] { DisasterType.Flood, DisasterType.Fire, DisasterType.Storm })
            {
                w.ReturnToMenu(); w.StartDisasterSimulation(type); report.simulationStartCalls++; w.FullFlowScriptedInput = true;
                Fixture("Production StartDisasterSimulation(" + type + ") initializes its own actor/bag; direct EnterBunker for roof-only presentation coverage");
                w.Model.EnterBunker(); yield return Wait(2.9f);
                Check("roof.exterior." + type, ExteriorRoofVisible(), RoofSummary());
                yield return Shot("roof-" + type.ToString().ToLowerInvariant());
            }
            yield return ShortFlowBoundary();
            w.ReturnToMenu(); yield return Wait(.15f);
            Check("phase.exit-restores-isometric", !w.BagCameraOwnsView && w.View.orthographic && !w.Model.CheatedRun);
            Call("RefreshPreparationVisibility"); CheckSupplies(false, "title");
            yield return Shot("99-title");
        }
        void ModelBoundaries()
        {
            Fixture("Isolated model fixtures: new models use already-isolated ScorePathOverride; exact count/time fields are setup, not campaign playthrough.");
            var m = new PanicModel(); m.StartRun(false); var e = new PanicExpedition(m);
            Check("bag.basic-one-of-each", m.RequiredDays == 1 && !m.BagReady);
            m.PackedWater = m.PackedFood = 1; Check("bag.basic-ready", m.BagReady && m.PackedSupportMask == 0);
            m.PackedFood = 0; Check("bag.food-required", !m.BagReady); m.PackedFood = 1; m.PackedWater = 0;
            Check("bag.water-required", !m.BagReady);
            m = new PanicModel(); m.StartRun(false); e = new PanicExpedition(m);
            Check("materials.empty-pool-blocks", !e.CanBeginWork("anchorwall", "bookshelf") && e.HasMissingRequirements);
            Check("materials.requirement-count-authority", e.CurrentRequirements.All(r => r.Owned == e.Count(r.Id) && r.Missing == Mathf.Max(0, r.Required - e.Count(r.Id))));
            e.ToggleSource("garage_tools");
            Check("materials.numeric-preflight", e.Count("screws") > 0 && e.CanBeginWork("anchorwall", "bookshelf") && !e.HasMissingRequirements && e.Inventory.All(s => s.Count == 0));
            Check("fence.model-rejected", !e.CanBeginWork("fence", "fence_west") && !e.TryCompleteWork("fence", "fence_west") && m.FenceCount == 0);
            foreach (float expected in new[] { 74.99f, 75f, 75.01f })
            {
                m = DamageFixture(expected); Fixture("Actual damage fixture " + expected.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + ": 12 movable table furniture + one stable-ID wall, Earthquake simulation, no TotalDamage override.");
                bool entered = m.EnterBunker(); m.Tick(8f); report.modelFixtureTicks++;
                Check("damage.actual." + expected.ToString("F2", System.Globalization.CultureInfo.InvariantCulture), entered && m.Phase == PanicPhase.Aftermath && Mathf.Abs(m.TotalDamage - expected) < .002f,
                    "calculated=" + m.TotalDamage.ToString("R") + "; reason=" + m.FailureReason);
                m.Tick(4f); report.modelFixtureTicks++;
                Check("damage.result." + expected.ToString("F2", System.Globalization.CultureInfo.InvariantCulture), expected >= 75f ? m.Phase == PanicPhase.GameOver : m.Phase == PanicPhase.Inspection,
                    "phase=" + m.Phase + "; calculated=" + m.TotalDamage.ToString("R") + "; reason=" + m.FailureReason);
            }
        }
        static PanicModel DamageFixture(float expected)
        {
            var m = new PanicModel(); m.StartSimulation(DisasterType.Earthquake); m.Items.Clear();
            for (int i = 0; i < 12; i++) m.Items.Add(new PanicItem { Id = "boundary-25-table-" + i, Label = "Boundary table", ArtId = "table", Kind = ItemKind.Furniture, Installed = true, Movable = true, Size = Vector3.one });
            string wall = expected < 75f ? "boundary-wall-11981" : expected == 75f ? "boundary-wall-45988" : "boundary-wall-51684";
            m.Items.Add(new PanicItem { Id = wall, Label = "Boundary wall", ArtId = "wall", Kind = ItemKind.Wall, Installed = true, Size = Vector3.one });
            return m;
        }
        IEnumerator AnchorTransaction()
        {
            yield return Place(new Vector3(6.5f, .04f, 3.25f), "fridge", "real two-screw numeric-material work");
            var item = w.Model.Items.Find(i => i.Id == "fridge");
            string before = Stock(); int budget = w.Model.Budget;
            w.BeginWork("anchorwall", "fridge"); yield return null;
            Check("work.real-preflight", w.WorkActive && w.Expedition.Work.TotalSteps == 2, w.Model.Message);
            Check("work.first-click", w.ClickWorkTarget(0));
            float end = Time.realtimeSinceStartup + .22f;
            while (w.WorkActive && Time.realtimeSinceStartup < end) { w.StepWork(Dt, 0, false, false, false); report.manualWorkTickFrames++; yield return null; }
            w.CancelWork(); yield return null;
            Check("work.cancel-free", !w.WorkActive && !item.Anchored && w.Model.Budget == budget && Stock() == before);
            var costs = PanicExpedition.MaterialCosts("anchorwall", item).ToArray(); var owned = costs.Select(c => w.Expedition.Count(c.Id)).ToArray();
            int cost = PanicExpedition.WorkCost("anchorwall", item);
            w.BeginWork("anchorwall", "fridge"); yield return null;
            Check("work.restart-real", w.WorkActive && w.ClickWorkTarget(0));
            end = Time.realtimeSinceStartup + 2;
            while (w.WorkActive && w.Expedition.Work.CompletedSteps == 0 && Time.realtimeSinceStartup < end)
            { w.StepWork(Dt, 0, false, false, false); report.manualWorkTickFrames++; yield return null; }
            Check("work.second-click", w.WorkActive && w.ClickWorkTarget(1)); yield return WaitWork();
            Check("work.exact-recipe", item.Anchored && w.Model.Budget == budget - cost && costs.Select((c, i) => w.Expedition.Count(c.Id) == owned[i] - c.Required).All(v => v));
            before = Stock(); budget = w.Model.Budget; w.CompleteWork();
            Check("work.commit-once", Stock() == before && w.Model.Budget == budget);
        }
        IEnumerator StableStructures()
        {
            foreach (var sample in new[] { new { id = "wall_north", p = new Vector3(-5, .04f, 7.1f) }, new { id = "window_spare", p = new Vector3(-5, .04f, 7.1f) } })
            {
                yield return Place(sample.p, sample.id, "bounds-based stable structure selection away from origin");
                for (int i = 0; i < 12; i++) { Call("RefreshFocus", false); yield return null; }
                if (sample.id == "window_spare") ProbeInteraction("window-spare-after-stable-refresh", sample.id);
                Check("selection.stable." + sample.id, Focus == sample.id && w.CanInteract, "focus=" + Focus + "; actor=" + w.PlayerPosition);
                yield return CheckOutline(sample.id);
                if (sample.id == "window_spare")
                {
                    object[] blockerBoundsArgs = { sample.id, new Bounds() }; Call("TryGetInteractionBounds", blockerBoundsArgs);
                    Vector3 targetCenter = ((Bounds)blockerBoundsArgs[1]).center;
                    var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube); blocker.name = "Revision412 explicit window LOS blocker fixture";
                    blocker.transform.position = Vector3.Lerp(w.PlayerPosition + Vector3.up * .9f, targetCenter, .5f);
                    blocker.transform.localScale = new Vector3(.3f, .3f, .2f); Physics.SyncTransforms();
                    Fixture("R2 negative LOS setup: enabled default-layer solid cube halfway along torso-to-window-art-center ray; no target/controller collision disabled.");
                    string blockedStock = Stock(); int blockedBudget = w.Model.Budget;
                    bool blocked = !w.CanInteract; w.BeginWork("upgrade", sample.id);
                    Check("selection.window-real-obstacle-rejects", blocked && !w.WorkActive && Stock() == blockedStock && w.Model.Budget == blockedBudget,
                        "Actual enabled solid fixture blocks CanInteract/BeginWork without cost; diagnostic contains fresh hit hierarchy.");
                    ProbeInteraction("window-spare-explicit-solid-obstruction", sample.id);
                    blocker.SetActive(false); Destroy(blocker); Physics.SyncTransforms(); yield return null;
                    Check("selection.window-obstacle-removal-restores", w.CanInteract && Focus == sample.id);
                }
                var item = w.Model.Items.Find(i => i.Id == sample.id);
                int tier = item.Tier, budget = w.Model.Budget, cost = PanicExpedition.WorkCost("upgrade", item);
                var costs = PanicExpedition.MaterialCosts("upgrade", item).ToArray(); var owned = costs.Select(c => w.Expedition.Count(c.Id)).ToArray();
                string stock = Stock(); w.BeginWork("upgrade", sample.id); yield return null;
                if (sample.id == "window_spare" && !w.WorkActive) ProbeInteraction("window-spare-upgrade-begin-rejected", sample.id);
                Check("selection.real-upgrade-begins." + sample.id, w.WorkActive && w.HasConstructionPreview, w.Model.Message);
                w.CancelWork(); yield return null;
                Check("selection.upgrade-cancel-free." + sample.id, item.Tier == tier && w.Model.Budget == budget && Stock() == stock);
                w.BeginWork("upgrade", sample.id); yield return WaitWork();
                Check("selection.upgrade-exact-recipe." + sample.id, item.Tier == tier + 1 && w.Model.Budget == budget - cost &&
                    costs.Select((c, i) => w.Expedition.Count(c.Id) == owned[i] - c.Required).All(v => v));
            }
            yield return Place(new Vector3(1.5f, .04f, -7.3f), "wall_south", "wall and adjacent real transit door");
            Call("RefreshFocus", false); Check("selection.south-wall-kept", Focus == "wall_south");
            w.SetFocus("door"); w.ToggleDoor(); yield return Wait(.25f);
            Check("selection.adjacent-door-still-interacts", HingeOpen("door"));
            w.SetFocus("wall_south"); Call("RefreshFocus", false);
            Check("selection.return-to-wall", Focus == "wall_south" && w.CanInteract);
            yield return Shot("03-selected-structure-bounds");
        }
        IEnumerator CheckOutline(string id)
        {
            object[] args = { id, new Bounds() }; bool got = (bool)Call("TryGetInteractionBounds", args); var declared = (Bounds)args[1];
            Call("UpdateInteractionOutline"); yield return null;
            var silhouette = w.View.GetComponent<PanicSelectionSilhouette>();
            var selectedMeshes = silhouette ? (List<Renderer>)typeof(PanicSelectionSilhouette).GetField("selected", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(silhouette) : new List<Renderer>();
            bool geometry = silhouette && silhouette.PassReady && selectedMeshes.Count > 0 && selectedMeshes.All(r => r && r.enabled && r.gameObject.activeInHierarchy && (r is MeshRenderer || r is SkinnedMeshRenderer));
            bool noBoundsLines = !w.GetComponentsInChildren<LineRenderer>(true).Any(r => r.name == "Selected interaction bounds" && r.gameObject.activeInHierarchy);
            Check("highlight.actual-mesh-pass." + id, geometry && noBoundsLines, "visible mesh count=" + selectedMeshes.Count + "; pass ready=" + (silhouette && silhouette.PassReady));
            Check("highlight.surface-scope." + id, !id.StartsWith("wall_", StringComparison.Ordinal) || selectedMeshes.Count == 1, "wall must select only one authored mesh, never a floor parent");
            if (!id.StartsWith("wall_", StringComparison.Ordinal))
            {
                GameObject root = null;
                if (!SupplyRoots.TryGetValue(id, out root)) Field<Dictionary<string, GameObject>>("items").TryGetValue(id, out root);
                var visuals = root ? root.GetComponentsInChildren<Renderer>(true).Where(r => !(r is LineRenderer) && r.gameObject.activeInHierarchy).ToArray() : Array.Empty<Renderer>();
                Bounds measured = new Bounds();
                if (visuals.Length > 0) { measured = visuals[0].bounds; foreach (var renderer in visuals.Skip(1)) measured.Encapsulate(renderer.bounds); }
                Check("highlight.actual-renderers." + id, got && visuals.Length > 0 && Vector3.Distance(declared.center, measured.center) < .02f && Vector3.Distance(declared.size, measured.size) < .03f,
                    "independently unioned actual renderer count=" + visuals.Length + "; native measured=" + measured + "; API=" + declared);
            }
            if (id.StartsWith("wall_", StringComparison.Ordinal))
            {
                var item = w.Model.Items.Find(i => i.Id == id);
                var physical = FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c => c.enabled && !c.isTrigger && c.name.StartsWith("Collision_", StringComparison.Ordinal) &&
                    Mathf.Abs(c.bounds.center.z - item.Position.z) < .55f && c.bounds.min.y >= -.02f && c.bounds.max.y <= 3.3f && c.bounds.size.x > .2f).ToArray();
                Check("highlight.wall-physical-extent", physical.Length > 0 && physical.Any(c => declared.Intersects(c.bounds)) && declared.size.y > 2.8f,
                    "independent permanent segment candidates=" + physical.Length + "; actual wall volume=" + declared);
            }
        }
        IEnumerator TreesAndFence()
        {
            var sites = ((IEnumerable)CallField("gardenSites")).Cast<object>().ToArray();
            Check("fence.no-sites", sites.All(s => Site<string>(s, "action") != "fence"));
            var trees = sites.Where(s => Site<string>(s, "action") == "plant").ToArray();
            Check("trees.three-regions", trees.Length == 3);
            foreach (var site in trees)
            {
                string id = Site<string>(site, "id"); Vector3 position = Site<Vector3>(site, "p");
                int variant = id == "plant_west1" ? 0 : id == "plant_west2" ? 1 : 2;
                var seats = PanicConstructionArt.TreeTrunkSeats(variant).ToArray();
                if (id == "plant_west1")
                {
                    yield return Place(position + seats[0] + Vector3.up * .04f, id, "prospective tree-trunk collision rejection");
                    string stock = Stock(); int budget = w.Model.Budget; w.BeginWork("plant", id); yield return null;
                    Check("trees.actor-seat-reject-free", !w.WorkActive && !w.Expedition.IsWorkCommitted("plant", id) && Stock() == stock && w.Model.Budget == budget);
                }
                Vector3 safe = position + (variant == 2 ? Vector3.back : Vector3.right) * 1.1f + Vector3.up * .04f;
                yield return Place(safe, id, "safe whole-row installation");
                var costs = PanicExpedition.MaterialCosts("plant", null).ToArray(); var owned = costs.Select(c => w.Expedition.Count(c.Id)).ToArray();
                int before = w.Model.Budget, cost = PanicExpedition.WorkCost("plant", null);
                w.BeginWork("plant", id); Check("trees.real-install-begins." + id, w.WorkActive, w.Model.Message); yield return WaitWork();
                var built = Site<GameObject>(site, "built");
                var trunks = built ? built.GetComponentsInChildren<CapsuleCollider>().Where(c => c.name == "Tree trunk collision" && c.enabled && !c.isTrigger).ToArray() : Array.Empty<CapsuleCollider>();
                Check("trees.four-solid-trunks." + id, w.Expedition.IsWorkCommitted("plant", id) && trunks.Length == 4 && trunks.All(t => t.gameObject.activeInHierarchy));
                Check("trees.exact-transaction." + id, w.Model.Budget == before - cost && costs.Select((c, i) => w.Expedition.Count(c.Id) == owned[i] - c.Required).All(v => v));
                float maximumOverlap = TrunkOverlap(); Vector3 actor = w.PlayerPosition;
                for (int i = 0; i < 8; i++) { w.SmokeMove(Vector2.zero, Dt); maximumOverlap = Mathf.Max(maximumOverlap, TrunkOverlap()); yield return null; }
                Check("trees.controller-clear." + id, maximumOverlap < .006f && Vector3.Distance(actor, w.PlayerPosition) < .025f, "independent enabled CapsuleCollider query overlap=" + maximumOverlap);
                Check("trees.row-extent." + id, trunks.Length == 4 && (variant == 2 ? trunks.Max(t => t.transform.position.x) - trunks.Min(t => t.transform.position.x) > 12f : trunks.Max(t => t.transform.position.z) - trunks.Min(t => t.transform.position.z) > 8f));
            }
            Check("trees.twelve-physical-trunks", w.InstalledTreeRows == 3 && FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None).Count(c => c.name == "Tree trunk collision") == 12);
            yield return Shot("05-whole-tree-rows");
        }
        object CallField(string name) { return typeof(PanicHomeWorld).GetField(name, Instance).GetValue(w); }
        static T Site<T>(object site, string field) { return (T)site.GetType().GetField(field, Instance).GetValue(site); }
        IEnumerator FpsTransactions()
        {
            string stock = Stock(); var source = w.Expedition.Sources.Find(s => s.Id == "fridge");
            yield return Place(new Vector3(6.5f, .04f, 3.1f), null, "FPS cabinet denial, same actor");
            AimAt(w.Model.Items.Find(i => i.Id == "fridge").Position + Vector3.up * 1.05f);
            w.HandleKey(KeyCode.E); yield return Wait(.15f); w.ToggleContainer(); yield return null;
            Check("fps.cabinet-e-rejected", !source.Open && Stock() == stock, "fresh eye ray E and direct world ToggleContainer leave cabinet/materials unchanged");
            Check("fps.no-construction-work", !w.WorkActive); w.BeginWork("upgrade", "window_north");
            Check("fps.begin-work-rejected", !w.WorkActive && Stock() == stock);
            yield return Place(new Vector3(3.1f, .04f, 1), null, "FPS real transit door");
            object[] args = { "door_inner_ground", new Bounds() }; Call("TryGetInteractionBounds", args); AimAt(((Bounds)args[1]).center);
            w.HandleKey(KeyCode.E); yield return Wait(.25f);
            Check("fps.transit-door-e", HingeOpen("door_inner_ground"), "fresh production center ray E");
            yield return Place(new Vector3(4.1f, .04f, 3.1f), null, "FPS physical basic supplies");
            foreach (string id in new[] { "supply_water_kitchen", "supply_food_kitchen" })
            {
                var supply = w.Expedition.FindWorldSupply(id); int count = supply.Count;
                int before = w.Expedition.Count(supply.ItemId) + (supply.ItemId == "water" ? w.Model.PackedWater : w.Model.PackedFood);
                AimAt(supply.Position + Vector3.up * .12f); yield return null;
                Check("fps.fresh-focus." + id, Focus == id && w.CanInteract, RaySummary());
                if (id == "supply_water_kitchen")
                {
                    yield return CheckOutline(id); yield return Shot("fps-supply-bounds");
                    yield return HudLayout("fps-hud-1600", 1600, 900);
                    yield return HudLayout("fps-hud-960", 960, 540);
                    Screen.SetResolution(1600, 900, false); yield return Wait(.15f);
                    AimAt(supply.Position + Vector3.up * .12f); yield return null;
                }
                w.HandleKey(KeyCode.E); yield return Wait(.12f);
                int after = w.Expedition.Count(supply.ItemId) + (supply.ItemId == "water" ? w.Model.PackedWater : w.Model.PackedFood);
                Check("fps.finite-pickup-autopack." + id, supply.Count == 0 && after == before + count && (supply.ItemId == "water" ? w.Model.PackedWater : w.Model.PackedFood) == 1, RaySummary());
                w.HandleKey(KeyCode.F); yield return null;
                Check("fps.no-duplicate." + id, after == w.Expedition.Count(supply.ItemId) + (supply.ItemId == "water" ? w.Model.PackedWater : w.Model.PackedFood));
            }
            Check("fps.basic-bag-ready", w.Model.BagReady && w.Model.PackedWater == 1 && w.Model.PackedFood == 1);
            hud.SetPaused(true); float time = w.Model.TimeRemaining; yield return Wait(.25f);
            Check("fps.pause-freezes-clock", Mathf.Abs(time - w.Model.TimeRemaining) < .001f); hud.SetPaused(false); yield return null;
            w.ToggleInventory(); yield return Wait(.1f); yield return Shot("07-fps-actual-inventory");
            Check("fps.inventory-real-capacities", w.InventoryOpen && w.Expedition.Inventory.Count == 12 && w.Expedition.Bag.Count == 8);
            w.ToggleInventory(); yield return null;
        }
        IEnumerator FpsBunkerAndCinematic()
        {
            yield return Place(PanicHomeWorld.BunkerPosition + new Vector3(0, .04f, -1.65f), null, "FPS bunker eye ray and real prepared bag");
            AimAt(PanicHomeWorld.BunkerPosition + Vector3.up * .14f); yield return null;
            w.HandleKey(KeyCode.E); yield return null;
            Check("fps.bunker-e-starts-disaster", w.Model.Phase == PanicPhase.Disaster && !w.BagCameraOwnsView, RaySummary());
            float end = Time.realtimeSinceStartup + 4f; bool descent = false, lidSafe = true, exterior = false; int samples = 0;
            while (w.Model.Phase == PanicPhase.Disaster && Time.realtimeSinceStartup < end)
            {
                float age = Field<float>("residenceCinematicAge");
                if (w.CinematicActive && age >= 1.35f && age < 2.5f)
                { descent = true; samples++; lidSafe &= LidAngle() > 90f; Observe("hatch-descent"); }
                if (age >= 2.7f) { exterior = true; break; }
                yield return null;
            }
            Check("bunker.lid-open-before-descent", descent && samples >= 2 && lidSafe, "actual post-AnimateHinges frame samples=" + samples);
            Check("roof.exterior.Earthquake", exterior && ExteriorRoofVisible(), RoofSummary());
            yield return Shot("08-disaster-exterior-roof");
        }
        void AimAt(Vector3 target)
        {
            w.View.transform.position = w.PlayerPosition + Vector3.up * 1.62f;
            Vector3 direction = (target - w.View.transform.position).normalized;
            float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, pitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
            w.StepBagLook(new Vector2(Mathf.DeltaAngle(Field<float>("bagYaw"), yaw) / 2f, (Field<float>("bagPitch") - pitch) / 1.7f));
            Call("RefreshBagRayFocus");
        }
        string RaySummary()
        {
            var hits = Physics.RaycastAll(w.View.ViewportPointToRay(new Vector3(.5f, .5f)), 2.4f, ~((1 << 28) | (1 << 30)), QueryTriggerInteraction.Collide);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            return "actor=" + w.PlayerPosition + "; focus=" + Focus + "; CanInteract=" + w.CanInteract + "; ray=" +
                string.Join(";", hits.Select(h => h.collider.name + ":" + (h.collider.GetComponentInParent<HomePick>()?.Id ?? "-") + ":" + h.distance));
        }
        void CheckSupplies(bool expected, string phase)
        {
            var roots = SupplyRoots.Values.Where(g => g).ToArray();
            Check("visibility." + phase, roots.Length > 0 && roots.All(g => g.activeInHierarchy == expected) && roots.All(g =>
                g.GetComponentsInChildren<Collider>(true).Any(c => c.enabled && c.gameObject.activeInHierarchy) == expected &&
                g.GetComponentsInChildren<Renderer>(true).Any(r => r.enabled && r.gameObject.activeInHierarchy) == expected),
                "roots=" + roots.Length + "; active=" + roots.Count(g => g.activeInHierarchy) + "; actual renderer/collider states in frames");
            Observe("supplies-" + phase);
        }
        IEnumerator ShortFlowBoundary()
        {
            w.ReturnToMenu(); w.StartJuryDemo(); w.FullFlowScriptedInput = true; yield return Wait(.1f);
            Check("shortflow.starts-isometric-anchor-first", !w.BagCameraOwnsView && !FinalStage &&
                (w.JurySnapshot.TargetId == "fridge" || w.JurySnapshot.TargetId == "bookshelf"));
            Check("shortflow.finite-material-sources-retained", w.Expedition.Sources.Any(s => s.Contents.Count > 0) &&
                w.Expedition.Sources.All(s => s.Contents.All(c => PanicExpedition.IsConstructionMaterial(c.Id))));
            float onboarding = w.JuryDemo.StageRemainingSeconds;
            w.JuryDemo.Tick(onboarding + .01f, w.PlayerPosition, w.IsGrounded, true);
            Fixture("Short-flow time fixture: expire onboarding with inputBlocked=true at actual actor pose; no movement/anchor goal granted.");
            Check("shortflow.preparation-anchor-before-water", w.JuryDemo.Stage == PanicJuryStage.Preparation &&
                (w.JurySnapshot.TargetId == "fridge" || w.JurySnapshot.TargetId == "bookshelf"));
            w.JuryDemo.Tick(Mathf.Max(0, w.JuryDemo.StageRemainingSeconds - 31f), w.PlayerPosition, w.IsGrounded, true);
            Fixture("Short-flow time fixture: actual JuryDemo.Tick to Preparation31, no resource or successful objective mutation.");
            Call("TickPreparationStages"); Check("shortflow.31-no-fps", !FinalStage && !w.BagCameraOwnsView && Mathf.Abs(w.Model.TimeRemaining - 31) < .01f);
            hud.SetPaused(true); float elapsed = w.JuryDemo.ElapsedSeconds; w.JuryDemo.Tick(1f, w.PlayerPosition, w.IsGrounded, true); yield return Wait(.15f);
            Check("shortflow.pause-holds-boundary", w.JuryDemo.ElapsedSeconds == elapsed && !w.BagCameraOwnsView && Mathf.Abs(w.Model.TimeRemaining - 31) < .01f);
            hud.SetPaused(false); w.JuryDemo.Tick(1f, w.PlayerPosition, w.IsGrounded, true); Call("TickPreparationStages");
            Check("shortflow.30-automatic-fps", FinalStage && w.BagCameraTransitionActive && w.JurySnapshot.TargetId == "supply_water_kitchen");
            yield return SettleBag(); Check("shortflow.no-fabricated-success", w.JuryDemo.Outcomes.Count == 0 && w.Model.CompletedWaves == 0 && w.Model.Score == 0 && !w.Model.CheatedRun &&
                !w.Model.Items.Any(i => (i.Id == "fridge" || i.Id == "bookshelf") && i.Anchored));
            yield return Shot("shortflow-final30");
            // Explicit readiness fixture only. This does not claim a played anchor route.
            var readyHero = w.Model.Items.Find(i => i.Id == "fridge");
            readyHero.Anchored = readyHero.AnchorOwned = true; readyHero.AnchorSurface = "wall";
            Fixture("Short-shelter R2 setup: directly mark existing fridge Anchored/AnchorOwned/wall; this is NOT an actual fastening route. Water is collected by the finite production supply transaction, without grant/replenish.");
            bool picked = w.Expedition.TryCollectWorldSupply("supply_water_kitchen");
            Call("SyncVisuals", true); Call("RefreshPreparationVisibility");
            w.JuryDemo.Tick(.001f, w.PlayerPosition, w.IsGrounded, false);
            Check("shortflow.shelter-ready-fixture", picked && w.Model.BagReady && w.Model.PackedWater == 1 && w.Expedition.JuryWaterGatheredCount == 1 &&
                w.JurySnapshot.ShelterKind == PanicJuryShelter.QuakeCover && w.BagFirstPersonActive,
                "Explicit anchored-fridge fixture plus finite real supply pickup; no success/outcome field assigned");
            yield return Place(PanicJuryDemo.QuakeCoverPosition, null, "R2 short-flow grounded safe-cover setup; not a walked route");
            w.SetFocus(null); w.StepBagLook(new Vector2(60, 0)); Call("RefreshBagRayFocus"); yield return null;
            string beforeFocus = Focus; int beforeOutcomes = w.JuryDemo.Outcomes.Count;
            w.HandleKey(KeyCode.E); yield return Wait(.08f);
            Check("shortflow.fps-e-enters-real-shelter", beforeOutcomes == 0 && w.JuryDemo.Stage == PanicJuryStage.Earthquake &&
                w.Model.Phase == PanicPhase.Disaster && w.JuryDemo.Outcomes.Count == 1 && w.JuryDemo.Outcomes[0].ShelterReached &&
                w.JuryDemo.Outcomes[0].PreparationComplete && !w.BagCameraOwnsView,
                "Real HandleKey(E)→Interact→InteractBagStage→TryJuryInteract at grounded cover; prior eye-ray focus=" + beforeFocus + "; stage=" + w.JuryDemo.Stage);
            w.HandleKey(KeyCode.E); yield return null;
            Check("shortflow.shelter-outcome-once", w.JuryDemo.Outcomes.Count == 1 && w.Model.CompletedWaves == 0 && w.Model.Score == 0);
        }
        void ProbeInteraction(string label, string target)
        {
            Physics.SyncTransforms();
            Vector3 eye = w.PlayerPosition + Vector3.up * .9f;
            object[] surfaceArgs = { target, eye, Vector3.zero };
            bool hasSurface = (bool)Call("InteractionSurfacePoint", surfaceArgs); Vector3 surface = (Vector3)surfaceArgs[2];
            object[] boundsArgs = { target, new Bounds() };
            bool hasBounds = (bool)Call("TryGetInteractionBounds", boundsArgs); Bounds bounds = (Bounds)boundsArgs[1];
            var item = w.Model.Items.Find(i => i.Id == target);
            Vector3 fallback = item != null ? item.Position : bounds.center;
            var probe = new InteractionProbe { label = label, target = target, focus = Focus, phase = w.Model.Phase.ToString(), frame = Time.frameCount,
                actor = w.PlayerPosition, eye = eye, surface = surface, artCenter = bounds.center, targetBounds = bounds, hasSurface = hasSurface, hasBounds = hasBounds,
                surfaceDistance = hasSurface ? Vector3.Distance(eye, surface) : -1, canInteract = w.CanInteract,
                pathClear = (bool)Call("InteractionPathClear", target, fallback), message = w.Model.Message,
                itemPosition = item != null ? item.Position : Vector3.zero, itemSize = item != null ? item.Size : Vector3.zero };
            var root = (GameObject)Call("InteractionObject", target);
            if (root) foreach (var collider in root.GetComponentsInChildren<Collider>(true)) probe.targetColliders.Add(ColliderProbe(collider, 0, Vector3.zero, Vector3.zero));
            if (hasSurface) probe.surfaceRay = ProbeRay(eye, surface, .025f);
            if (hasBounds)
            {
                probe.artCenterRay = ProbeRay(eye, bounds.center, .025f);
                probe.highEyeRay = ProbeRay(w.PlayerPosition + Vector3.up * 1.62f, bounds.center, .025f);
            }
            report.interactionProbes.Add(probe);
            Debug.Log("REVISION412_INTERACTION_PROBE " + JsonUtility.ToJson(probe));
        }
        static List<HitProbe> ProbeRay(Vector3 from, Vector3 to, float extension)
        {
            Vector3 delta = to - from; float distance = delta.magnitude;
            if (distance < .0001f) return new List<HitProbe>();
            var hits = Physics.RaycastAll(from, delta / distance, distance + extension, ~(1 << 30), QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            return hits.Select(h => ColliderProbe(h.collider, h.distance, h.point, h.normal)).ToList();
        }
        static HitProbe ColliderProbe(Collider collider, float distance, Vector3 point, Vector3 normal)
        {
            var pick = collider.GetComponentInParent<HomePick>();
            return new HitProbe { collider = collider.name, hierarchy = TransformPath(collider.transform), pickId = pick ? pick.Id : null,
                pickKind = pick ? pick.Kind : null, distance = distance, point = point, normal = normal, bounds = collider.bounds,
                trigger = collider.isTrigger, enabled = collider.enabled, active = collider.gameObject.activeInHierarchy };
        }
        static string TransformPath(Transform value)
        { var names = new List<string>(); for (var part = value; part; part = part.parent) names.Add(part.name); names.Reverse(); return string.Join("/", names); }
        IEnumerator HudLayout(string label, int width, int height)
        {
            Screen.SetResolution(width, height, false); yield return Wait(.2f); yield return Shot(label);
            var regionsProperty = typeof(PanicHomeHud).GetProperty("VisibleHudRegions", Instance);
            if (regionsProperty == null) { Check("hud.regions-api." + label, false, "VisibleHudRegions unavailable"); yield break; }
            var regions = ((IEnumerable)regionsProperty.GetValue(hud)).Cast<Rect>().ToArray();
            bool inside = regions.All(r => r.xMin >= -.5f && r.yMin >= -.5f && r.xMax <= 1600.5f && r.yMax <= 900.5f);
            bool overlap = false;
            for (int i = 0; i < regions.Length; i++) for (int j = i + 1; j < regions.Length; j++)
                if (regions[i].Overlaps(regions[j])) overlap = true;
            Check("hud.regions-inside." + label, Screen.width == width && Screen.height == height && regions.Length >= 4 && inside,
                "Native render " + Screen.width + "x" + Screen.height + "; logical1600x900 regions=" + string.Join(";", regions.Select(r => r.ToString())));
            Check("hud.regions-separate." + label, !overlap, "Top-level VisibleHudRegions only; pixel text readability requires capture review.");
        }
        bool HingeOpen(string id)
        { var hinges = (IDictionary)CallField("hinges"); return hinges.Contains(id) && (bool)hinges[id].GetType().GetField("open", Instance).GetValue(hinges[id]); }
        float LidAngle()
        { var leaf = Field<Transform>("bunkerLeaf"); return leaf ? Quaternion.Angle(Field<Quaternion>("bunkerClosed"), leaf.localRotation) : -1; }
        Renderer[] RoofRenderers()
        {
            var main = Field<GameObject>("roof"); var garage = Field<GameObject>("residenceGarage");
            var a = main ? main.GetComponentsInChildren<Renderer>(true) : Array.Empty<Renderer>();
            var b = garage ? garage.GetComponentsInChildren<Renderer>(true).Where(r => ParentNamed(r.transform, "Roof", garage.transform)).ToArray() : Array.Empty<Renderer>();
            return a.Concat(b).Distinct().ToArray();
        }
        bool ExteriorRoofVisible()
        { var all = RoofRenderers(); return w.CinematicActive && all.Length > 0 && all.All(r => r.enabled && r.gameObject.activeInHierarchy); }
        string RoofSummary() { var all = RoofRenderers(); return "kind=" + w.CinematicKind + "; actor=" + w.PlayerPosition + "; roofs=" + all.Length + "; enabled=" + all.Count(r => r.enabled && r.gameObject.activeInHierarchy); }
        static bool ParentNamed(Transform child, string name, Transform stop)
        { for (var t = child; t && t != stop; t = t.parent) if (t.name == name) return true; return false; }
        static bool Contains(Bounds bounds, Vector3 point, float tolerance)
        { bounds.Expand(tolerance * 2); return bounds.Contains(point); }
        void MakeCapsuleQuery()
        {
            var root = new GameObject("Revision412 independent enabled capsule query"); root.transform.position = new Vector3(700, 700, 700);
            root.layer = 2; capsuleQuery = root.AddComponent<CapsuleCollider>(); capsuleQuery.radius = controller.radius;
            capsuleQuery.height = controller.height; capsuleQuery.center = controller.center; capsuleQuery.direction = 1; capsuleQuery.isTrigger = true;
            Check("controller.contract", controller.enabled && Mathf.Abs(controller.height - 1.78f) < .001f && Mathf.Abs(controller.radius - .25f) < .001f && Mathf.Abs(controller.skinWidth - .025f) < .001f);
        }
        float TrunkOverlap()
        {
            Physics.SyncTransforms(); float maximum = 0;
            foreach (var trunk in FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None).Where(c => c.name == "Tree trunk collision" && c.enabled && !c.isTrigger))
                if (Physics.ComputePenetration(trunk, trunk.transform.position, trunk.transform.rotation, capsuleQuery, controller.transform.position, controller.transform.rotation, out _, out float distance)) maximum = Mathf.Max(maximum, distance);
            return maximum;
        }
        string Stock()
        { return string.Join("|", PanicExpedition.MaterialIds.Select(id => id + ":" + w.Expedition.Count(id))) + "/" + string.Join("|", w.Expedition.Inventory.Select(s => s.Id + ":" + s.Count)) + "/" + string.Join("|", w.Expedition.Bag.Select(s => s.Id + ":" + s.Count)); }
        static string Hash(string path)
        { if (string.IsNullOrEmpty(path) || !File.Exists(path)) return "absent"; using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""); }
        void Finish()
        {
            if (finished) return; finished = true;
            report.realScoreAfter = Hash(realScore); Check("scores.real-file-unchanged", report.realScoreBefore == report.realScoreAfter);
            report.seconds = Time.realtimeSinceStartup - began; report.finishedUtc = DateTime.UtcNow.ToString("o");
            report.passed = report.complete && string.IsNullOrEmpty(report.error) && report.errors.Count == 0 && report.rows.Count > 70 && report.rows.All(r => r.pass);
            if (capsuleQuery) Destroy(capsuleQuery.gameObject); Application.logMessageReceived -= OnLog;
            if (w && !string.IsNullOrEmpty(w.EvidencePath)) File.WriteAllText(Path.Combine(w.EvidencePath, "revision412-smoke.json"), JsonUtility.ToJson(report, true));
            Debug.Log("REVISION412_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.rows.Count + " seconds=" + report.seconds);
            Application.Quit(report.passed ? 0 : 2);
        }
    }
}
