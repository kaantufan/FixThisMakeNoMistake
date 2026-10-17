using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in continuous production-controller route. Never attach in a normal session.</summary>
    public sealed class PanicFullFlowSmoke : MonoBehaviour
    {
        [Serializable] public sealed class Check
        { public string name, detail, section, phase, stage; public bool pass; public float wallSeconds, sectionSeconds; public Vector3 position; }
        [Serializable] public sealed class Sample
        { public float wallSeconds, sectionSeconds, preparationRemaining; public string section, phase, stage; public Vector3 position; public bool controllerEnabled, grounded, cheated; }
        [Serializable] public sealed class Route
        { public string id, section, obstacle; public Vector3 start, target, end; public bool reached; public int frames; public float wallSeconds, seconds, movementDeltaSum; public List<Vector3> samples = new List<Vector3>(); }
        [Serializable] public sealed class Work
        { public string section, action, target, mode; public bool began, complete; public int frames, budgetBefore, budgetAfter; public float seconds, deltaSum; }
        [Serializable] public sealed class Milestone
        { public string id, stage, phase; public float shortFlowSeconds, wallSeconds; public Vector3 position; public bool before180Seconds; }
        [Serializable] public sealed class Outcome
        { public string disaster, terminalPhase, failureReason; public bool simulationComplete, ordinaryLoss, preparationApplied, cheated; public float seconds, cinemaSeconds, damage, waterConsumed, foodConsumed; public int budget; }
        [Serializable] public sealed class Report
        {
            public string startedUtc, finishedUtc, profileId, scorePath, scoreBefore, scoreAfter, error;
            public string evidenceType = "Continuous real wall-clock native semantic-input route using production CharacterController/colliders, E/F pickup, inventory packing, real manual work and natural disaster clocks.";
            public string limitations = "Semantic API input, not OS keyboard/mouse or human playability/fun acceptance. Menu/selector/start/retry, inventory selection/use and N-equivalent FinishInspection call the exact production action methods. Scripted-input gate suppresses only unscripted input; model, short-flow and cinema clocks stay live. SmokeMove and StepWork use actual frame delta with production safety caps, once per rendered frame; no injected Tick, skip, actor Warp, stock grants, damage edits or developer options. Game-owned start spawns and any game-owned cinematic repositioning are recorded as lifecycle behavior. A completed ordinary simulation loss is recorded as loss, then its production damage-review action opens walkable Inspection while retaining failure/damage and zero reward; it is never reported as survival. Retry reset is checked without repeating every full disaster. Human first-three-minute enjoyment is not measured.";
            public bool completed, passed; public float wallSeconds, shortFlowSeconds; public int passedChecks, totalChecks, explicitWarps, injectedTicks, developerMutations;
            public List<Check> checks = new List<Check>(); public List<Sample> timeline = new List<Sample>();
            public List<Route> routes = new List<Route>(); public List<Work> works = new List<Work>();
            public List<Milestone> firstThreeMinutes = new List<Milestone>(); public List<Outcome> simulations = new List<Outcome>();
            public List<string> screenshots = new List<string>(), errors = new List<string>();
        }

        const float HardCapSeconds = 245f;
        readonly Report report = new Report();
        PanicHomeWorld world; PanicHomeHud hud; CharacterController body;
        Coroutine running; bool finished, routeOK = true; float began, sectionBegan, shortBegan, nextSample;
        string section = "startup", lastPhase, lastStage, reportPath;
        static float MoveDt { get { return Mathf.Min(Time.unscaledDeltaTime, .04f); } }
        static float WorkDt { get { return Mathf.Min(Time.deltaTime, .08f); } }
        string Stage { get { return world && world.JuryDemo != null ? world.JuryDemo.Stage.ToString() : "none"; } }
        bool CanWalk { get { return world && world.PlayerControllerEnabled && !world.InputLocked && !world.CinematicActive && !world.LossAnnounced && !world.WorkActive && !world.InventoryOpen && world.JuryAllowsMovement && (world.Model.Phase == PanicPhase.Preparing || world.Model.Phase == PanicPhase.Inspection); } }

        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--fullflow-smoke") < 0) { enabled = false; return; }
            world = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>();
            if (!world || string.IsNullOrEmpty(world.EvidencePath)) { Debug.LogError("FULLFLOW_SMOKE missing isolated --home-evidence or world."); enabled = false; return; }
            began = sectionBegan = Time.realtimeSinceStartup; report.startedUtc = DateTime.UtcNow.ToString("o");
            report.profileId = Path.GetFileName(Path.GetFullPath(world.EvidencePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            report.scorePath = PanicModel.ScorePathOverride; report.scoreBefore = Hash(report.scorePath);
            reportPath = Path.Combine(world.EvidencePath, "fullflow-smoke.json");
            body = FindObjectsByType<CharacterController>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Survivor");
            Application.logMessageReceived += OnLog; Application.runInBackground = true;
            running = StartCoroutine(Guarded(Run()));
        }

        IEnumerator Guarded(IEnumerator sequence)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(sequence);
            while (stack.Count > 0 && !finished)
            {
                bool next = false; object current = null;
                try { next = stack.Peek().MoveNext(); if (next) current = stack.Peek().Current; }
                catch (Exception ex) { report.error = ex.ToString(); CheckIt("unhandled harness exception", false, ex.Message); break; }
                if (!next) { stack.Pop(); continue; }
                var nested = current as IEnumerator;
                if (nested != null) stack.Push(nested); else yield return current;
            }
            Finish();
        }

        IEnumerator Run()
        {
            Guid guid; string name = report.profileId;
            string expected = Path.GetFullPath(Path.Combine(world.EvidencePath, "isolated-home-scores.json"));
            bool isolated = name.Length >= 36 && Guid.TryParse(name.Substring(name.Length - 36), out guid) && !string.IsNullOrEmpty(report.scorePath) &&
                string.Equals(Path.GetFullPath(report.scorePath), expected, StringComparison.OrdinalIgnoreCase) && !File.Exists(expected) && !File.Exists(reportPath) && world.Model.Highscores.Count == 0;
            CheckIt("fresh GUID and isolated score path before any run", isolated, "profile=" + name + "; score=" + report.scorePath);
            CheckIt("production character and controller contract", body && body.enabled && Mathf.Abs(body.radius - .25f) < .001f && Mathf.Abs(body.height - 1.78f) < .001f && Mathf.Abs(body.stepOffset - .25f) < .001f && Mathf.Abs(body.slopeLimit - 48f) < .001f, "Survivor real CharacterController");
            if (!isolated || !body || !hud) yield break;
            while (!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            world.FullFlowScriptedInput = true;
            yield return Wait(.25f);
            yield return Shot("01-title");
            yield return ShortFlow();
            world.ReturnToMenu(); yield return Wait(.15f);
            foreach (var type in new[] { DisasterType.Earthquake, DisasterType.Flood, DisasterType.Fire, DisasterType.Storm })
            {
                yield return Simulation(type);
                world.ReturnToMenu(); yield return Wait(.15f);
            }
            CheckIt("all four selectable simulations executed", report.simulations.Count == 4, "outcomes=" + report.simulations.Count);
            CheckIt("no harness teleport, injected time or developer mutation", report.explicitWarps == 0 && report.injectedTicks == 0 && report.developerMutations == 0 && !world.Model.CheatedRun && !world.Model.DeveloperAnyEnabled, "Only game-owned start spawns are used.");
            yield return Shot("99-final-menu");
            report.completed = true;
        }

        IEnumerator ShortFlow()
        {
            BeginSection("short-flow"); shortBegan = Time.realtimeSinceStartup;
            world.StartJuryDemo(); yield return Wait(.2f);
            CheckIt("short flow starts at production spawn without cheats", world.JuryDemo != null && world.Model.IsJuryDemo && !world.Model.CheatedRun && !world.Model.DeveloperAnyEnabled, "spawn=" + world.PlayerPosition);
            if (world.JuryDemo == null) yield break;
            yield return Shot("02-short-start");
            yield return PathTo("kitchen", new Vector3(3.5f,0,-1), new Vector3(5,0,1), new Vector3(6.5f,0,3.25f));
            if (!routeOK) yield break;
            CheckIt("real movement completes onboarding", world.JuryDemo.Stage == PanicJuryStage.Preparation);
            yield return Key("fridge", KeyCode.E); yield return Shot("03-fridge-open");
            world.HandleKey(KeyCode.F); yield return Wait(.15f);
            bool gathered = world.Expedition.JuryWaterGatheredCount == 1 && world.Expedition.Count("water") == 1;
            CheckIt("real E open and F selected water pickup", gathered, world.Model.Message); if (gathered) Mark("first-pickup");
            int water = world.Expedition.Inventory.FindIndex(s => s.Id == "water" && s.Count > 0);
            if (!gathered || water < 0) yield break;
            world.HandleKey(KeyCode.Tab); yield return Wait(.1f); world.InventorySelect(water); world.InventoryUse();
            yield return Shot("04-water-packed-inventory"); world.HandleKey(KeyCode.Tab); yield return Wait(.1f);
            CheckIt("inventory pack consumes one real pickup", world.Model.PackedWater == 1 && world.Expedition.Count("water") == 0 && !world.InventoryOpen); if (world.Model.PackedWater == 1) Mark("water-packed");
            yield return Key("fridge", KeyCode.Alpha1); yield return DriveWork(PanicWorkMode.Ratchet);
            bool anchored = Item("fridge").Anchored && world.Expedition.Count("bracket") == 0 && world.Expedition.Count("screws") == 0 && world.Model.Budget == 940;
            CheckIt("real short anchor commits exact stock and budget", anchored, "budget=" + world.Model.Budget); if (anchored) Mark("first-real-prevention");
            yield return Shot("05-fridge-secured"); if (!anchored) yield break;
            yield return PathTo("inner-door-east", new Vector3(4.3f,0,2.2f), new Vector3(2.85f,0,1)); if (!routeOK) yield break;
            yield return Key("door_inner_ground", KeyCode.E); yield return Wait(.65f);
            yield return PathTo("quake-cover", new Vector3(1.5f,0,1), new Vector3(1.5f,0,-.65f), new Vector3(0,0,-.85f), new Vector3(-1.2f,0,-1.7f), PanicJuryDemo.QuakeCoverPosition); if (!routeOK) yield break;
            world.HandleKey(KeyCode.E); yield return Wait(.1f);
            CheckIt("actual grounded cover E starts quake", world.JuryDemo.Stage == PanicJuryStage.Earthquake, "actor=" + world.PlayerPosition); if (world.JuryDemo.Stage == PanicJuryStage.Earthquake) Mark("first-visible-disaster");
            float quakeBegan = Time.realtimeSinceStartup;
            yield return Wait(2f); yield return Shot("06-short-natural-earthquake");
            yield return WaitStage(PanicJuryStage.EarthquakeResult, 7f);
            yield return Shot("06b-short-real-damage-result");
            yield return WaitStage(PanicJuryStage.Flood, 5f);
            CheckIt("natural 8 second quake and 4 second result advance", world.JuryDemo.Stage == PanicJuryStage.Flood && Time.realtimeSinceStartup - quakeBegan >= 11.5f, "wall=" + (Time.realtimeSinceStartup - quakeBegan).ToString("F2"));
            bool quakePrepared = world.JuryDemo.Outcomes.Count >= 1 && world.JuryDemo.Outcomes[0].PreparationComplete && world.JuryDemo.Outcomes[0].ShelterReached;
            CheckIt("quake outcome records actual completed preparation", quakePrepared); if (world.JuryDemo.Stage != PanicJuryStage.Flood) yield break;
            yield return PathTo("front-door-inside", new Vector3(-1.2f,0,-1.7f), new Vector3(0,0,-3.8f), new Vector3(0,0,-4.9f)); if (!routeOK) yield break;
            yield return Key("door", KeyCode.E); yield return Wait(.65f);
            yield return PathTo("drain", new Vector3(0,0,-7.2f), new Vector3(3,0,-8.5f), new Vector3(4,0,-8.5f)); if (!routeOK) yield break;
            yield return Key("drain_east", KeyCode.E); yield return DriveWork(PanicWorkMode.Timing);
            CheckIt("real drain consumes exact finite recipe", world.Model.DrainCount == 1 && world.Expedition.Count("sandbag") == 0 && world.Expedition.Count("plank") == 0 && world.Model.Budget == 890); Mark("flood-prevention");
            yield return Shot("06c-drain-to-upstairs-guidance");
            yield return PathTo("stair-bottom", new Vector3(0,0,-7.2f), new Vector3(0,0,-4.9f), new Vector3(0,0,-.65f)); if (!routeOK) yield break;
            yield return PathTo("physical-upper-stairs", new Vector3(0,1.6f,2.25f), new Vector3(0,3.2f,5.35f)); if (!routeOK) yield break;
            yield return Shot("07-real-upper-refuge"); world.HandleKey(KeyCode.E); yield return Wait(.15f);
            CheckIt("actual physical stair route and refuge E starts fire", world.JuryDemo.Stage == PanicJuryStage.Fire && world.PlayerPosition.y > 3.05f); Mark("physical-flood-refuge");
            if (world.JuryDemo.Stage != PanicJuryStage.Fire) yield break;
            yield return PathTo("upper-fire-preparation", new Vector3(-1.6f,3.2f,5.35f), new Vector3(-2.8f,3.2f,4.5f)); if (!routeOK) yield break;
            CheckIt("fire guidance points to nearby upper-floor furniture", world.JurySnapshot.TargetId == "bed", "target=" + world.JurySnapshot.TargetId);
            yield return Key("bed", KeyCode.Alpha2); yield return DriveWork(PanicWorkMode.Balance);
            CheckIt("actual fire tool preparation consumes extinguisher", world.Model.FireToolCount == 1 && world.Expedition.Count("extinguisher") == 0 && world.Model.Budget == 815); Mark("fire-prevention");
            yield return PathTo("fire-egress", new Vector3(-1.6f,3.2f,5.35f), new Vector3(0,3.2f,5.35f), new Vector3(0,0,-.65f), new Vector3(0,0,-3.8f), new Vector3(0,0,-7.2f), new Vector3(-3,0,-9.5f), PanicJuryDemo.FireAssemblyPosition); if (!routeOK) yield break;
            world.HandleKey(KeyCode.E); yield return Wait(.15f); yield return Shot("08-short-inspection");
            CheckIt("physical fire egress E reaches inspection", world.JuryDemo.Stage == PanicJuryStage.Inspection && world.JuryDemo.Outcomes.Count == 3); Mark("all-three-disasters-visible");
            bool goals = world.JurySnapshot.Goals.All(g => g.State == PanicJuryGoalState.Complete);
            CheckIt("continuous short route completes every actual goal", goals, string.Join(", ", world.JurySnapshot.Goals.Select(g => g.Id + "=" + g.State).ToArray()));
            world.FinishInspection(); yield return Wait(.15f); yield return Shot("09-short-result");
            world.AdvanceJuryDemo(); yield return Wait(.15f);
            report.shortFlowSeconds = Time.realtimeSinceStartup - shortBegan;
            CheckIt("short flow reaches bounded truthful complete result", world.JuryDemo.Stage == PanicJuryStage.Complete && report.shortFlowSeconds <= 270f && !world.Model.CheatedRun && world.Model.Score == 0 && world.Model.LastReward == 0, "wall=" + report.shortFlowSeconds.ToString("F2"));
            CheckIt("pickup prevention and visible result all inside first 180 seconds", HasMilestone("first-pickup") && HasMilestone("first-real-prevention") && HasMilestone("first-visible-disaster"), "Measured route checkpoints; enjoyment remains unmeasured.");
            world.StartJuryDemo(); yield return Wait(.15f);
            CheckIt("short retry returns to fresh stock and onboarding", world.JuryDemo.Stage == PanicJuryStage.Onboarding && world.Expedition.Count("bracket") == 1 && world.Model.PackedWater == 0 && !Item("fridge").Anchored);
        }

        IEnumerator Simulation(DisasterType type)
        {
            BeginSection("simulation-" + type); hud.OpenSimulationSelector(); yield return Wait(.1f);
            CheckIt("title simulation selector " + type, hud.SimulationSelectorOpen && world.Model.Phase == PanicPhase.Title);
            bool started = world.StartDisasterSimulation(type); yield return Wait(.2f);
            CheckIt("selected finite simulation starts " + type, started && world.Model.IsSimulation && world.Model.Disaster == type && world.Model.PackedWater == 1 && world.Model.PackedFood == 1 && !world.Model.CheatedRun && !world.Model.DeveloperAnyEnabled && !hud.SimulationSelectorOpen);
            if (!started) yield break;
            bool prepared = false;
            if (type == DisasterType.Flood)
            {
                yield return PathTo("sim-drain", new Vector3(3,0,-8.5f), new Vector3(4,0,-8.5f));
                if (routeOK) { yield return Key("drain_east", KeyCode.E); yield return DriveWork(PanicWorkMode.Timing); }
                prepared = world.Model.DrainCount == 1;
            }
            else if (type == DisasterType.Storm)
            {
                yield return Key("door", KeyCode.Alpha3); yield return DriveWork(PanicWorkMode.Ratchet);
                prepared = Item("door").Tier == 1;
            }
            else
            {
                yield return Key("door", KeyCode.E); yield return Wait(.65f);
                yield return PathTo("sim-inner-west", new Vector3(0,0,-4.9f), new Vector3(0,0,-1.2f), new Vector3(1.5f,0,-.65f), new Vector3(1.5f,0,1));
                if (routeOK) { yield return Key("door_inner_ground", KeyCode.E); yield return Wait(.65f); yield return PathTo("sim-fridge", new Vector3(2.85f,0,1), new Vector3(5,0,1), new Vector3(6.5f,0,3.25f)); }
                if (routeOK) { yield return Key("fridge", type == DisasterType.Earthquake ? KeyCode.Alpha1 : KeyCode.Alpha2); yield return DriveWork(type == DisasterType.Earthquake ? PanicWorkMode.Ratchet : PanicWorkMode.Balance); }
                prepared = type == DisasterType.Earthquake ? Item("fridge").Anchored : world.Model.FireToolCount == 1;
            }
            CheckIt("genuine relevant preparation " + type, prepared, "budget=" + world.Model.Budget + "; phase=" + world.Model.Phase + "; " + world.Model.Message);
            yield return Shot("sim-" + type + "-prepared");
            int water = world.Model.PackedWater, food = world.Model.PackedFood;
            float cinemaBegan = Time.realtimeSinceStartup; world.TriggerSimulation(); yield return null;
            CheckIt("production simulation start enters real disaster " + type, world.Model.Phase == PanicPhase.Disaster, "No Model.Tick/skip call.");
            yield return Wait(2.1f); yield return Shot("sim-" + type + "-natural-disaster");
            yield return Wait(3.7f); yield return Shot("sim-" + type + "-natural-disaster-detail");
            float deadline = Time.realtimeSinceStartup + 19f;
            while (Time.realtimeSinceStartup < deadline && (world.Model.Phase == PanicPhase.Disaster || world.Model.Phase == PanicPhase.Aftermath || world.Model.Phase == PanicPhase.Success || world.CinematicActive || world.LossAnnounced)) yield return null;
            var outcome = new Outcome { disaster = type.ToString(), terminalPhase = world.Model.Phase.ToString(), simulationComplete = world.Model.SimulationComplete, ordinaryLoss = world.Model.Phase == PanicPhase.GameOver,
                failureReason = world.Model.FailureReason, preparationApplied = prepared, cheated = world.Model.CheatedRun, seconds = Time.realtimeSinceStartup - sectionBegan,
                cinemaSeconds = Time.realtimeSinceStartup - cinemaBegan, damage = world.Model.TotalDamage, waterConsumed = water - world.Model.PackedWater, foodConsumed = food - world.Model.PackedFood, budget = world.Model.Budget };
            report.simulations.Add(outcome);
            CheckIt("natural disaster and aftermath resolve once " + type, outcome.cinemaSeconds >= 11.5f && !world.CinematicActive && !world.LossAnnounced && !world.InputLocked && (world.Model.Phase == PanicPhase.Inspection && world.PlayerControllerEnabled && world.View.orthographic || world.Model.Phase == PanicPhase.GameOver) && outcome.waterConsumed == 1 && outcome.foodConsumed == 1, "wall=" + outcome.cinemaSeconds.ToString("F2") + "; phase=" + outcome.terminalPhase + "; damage=" + outcome.damage.ToString("F2"));
            bool honestTerminal = world.Model.Phase == PanicPhase.Inspection && string.IsNullOrEmpty(outcome.failureReason) || outcome.ordinaryLoss && !string.IsNullOrEmpty(outcome.failureReason) && outcome.damage > 65f;
            CheckIt("prepared simulation reaches truthful no-cheat terminal " + type, prepared && world.Model.SimulationComplete && honestTerminal && !world.Model.CheatedRun && !world.Model.DeveloperAnyEnabled, "ordinaryLoss=" + outcome.ordinaryLoss + "; " + outcome.failureReason);
            CheckIt("unranked simulation preserves wave reward and score " + type, world.Model.Wave == 1 && world.Model.CompletedWaves == 0 && world.Model.Score == 0 && world.Model.LastReward == 0);
            CheckIt("live bag slots reflect natural supply consumption " + type, world.Model.PackedWater == 0 && world.Model.PackedFood == 0 && world.Expedition.Bag.Where(s => s.Id == "water" || s.Id == "food").Sum(s => s.Count) == 0, "packed=" + world.Model.PackedWater + "/" + world.Model.PackedFood + "; bag=" + string.Join(",", world.Expedition.Bag.Select(s => s.Id + ":" + s.Count).ToArray()));
            yield return Shot("sim-" + type + "-result-" + world.Model.Phase);
            if (outcome.ordinaryLoss)
            {
                string reason = world.Model.FailureReason; float damage = world.Model.TotalDamage; int budget = world.Model.Budget;
                bool inspect = world.InspectSimulationDamage(); yield return Wait(.15f);
                CheckIt("ordinary loss opens damage review without success mutation " + type, inspect && world.Model.Phase == PanicPhase.Inspection && world.Model.SimulationComplete && world.Model.TotalDamage == damage && world.Model.FailureReason == reason && world.Model.Budget == budget && world.Model.Score == 0 && world.Model.LastReward == 0 && !world.Model.CheatedRun, reason);
                yield return Shot("sim-" + type + "-failed-damage-review");
            }
            if (world.Model.Phase == PanicPhase.Inspection)
            {
                CheckIt("inspection restores playable camera and controller " + type, world.PlayerControllerEnabled && world.View.orthographic && !world.CinematicActive && !world.InputLocked);
                Vector3 inspectStart = world.PlayerPosition; Vector3 inspectTarget = inspectStart + (type == DisasterType.Flood ? Vector3.left : Vector3.back) * .6f;
                yield return MoveTo("inspection-controller-restored-" + type, inspectTarget, .12f);
                CheckIt("inspection actual movement available " + type, routeOK && Vector3.Distance(inspectStart, world.PlayerPosition) > .3f);
                world.FinishInspection(); yield return Wait(.1f);
                CheckIt("N-equivalent inspection finish returns to menu " + type, world.Model.Phase == PanicPhase.Title && !world.Model.IsSimulation);
            }
            else CheckIt("inspection is available after terminal " + type, false, "phase=" + world.Model.Phase + "; no no-loss option enabled");
            world.StartDisasterSimulation(type); yield return Wait(.15f);
            CheckIt("result retry resets selected simulation " + type, world.Model.IsSimulation && world.Model.Disaster == type && world.Model.Phase == PanicPhase.Preparing && world.Model.Budget == 1000 && world.Model.TotalDamage == 0 && world.Model.PackedWater == 1 && world.Model.PackedFood == 1 && !world.Model.SimulationComplete && !world.Model.CheatedRun && world.Expedition.Count("bracket") == 1, "retry uses the same StartDisasterSimulation action as the UI");
        }

        IEnumerator Key(string focus, KeyCode key)
        {
            world.SmokeFocus(focus); yield return null;
            CheckIt("reachable real action " + focus + "/" + key, world.CanInteract, "position=" + world.PlayerPosition + "; " + world.ContextHint);
            if (world.CanInteract) world.HandleKey(key);
            yield return null;
        }

        IEnumerator DriveWork(PanicWorkMode expected)
        {
            var record = new Work { section = section, action = world.Expedition.Work.Action, target = world.Expedition.Work.TargetId, mode = expected.ToString(), began = world.WorkActive, budgetBefore = world.Model.Budget };
            report.works.Add(record); float start = Time.realtimeSinceStartup;
            CheckIt("production work began " + expected, world.WorkActive && world.Expedition.Work.Mode == expected);
            if (!world.WorkActive) yield break;
            while (world.WorkActive && Time.realtimeSinceStartup - start < 12f)
            {
                float dt = WorkDt; var work = world.Expedition.Work;
                if (!work.IsApplying)
                {
                    float offset = work.Target - work.Cursor;
                    world.StepWork(dt, Mathf.Abs(offset) <= work.TargetWidth * .65f ? 0 : Mathf.Sign(offset), false, Mathf.Abs(offset) <= work.TargetWidth * .65f, false);
                }
                else if (work.Mode == PanicWorkMode.Ratchet) world.StepWork(dt, work.ExpectedDirection, false, false, work.ExpectedDirection == 0);
                else if (work.Mode == PanicWorkMode.Timing) world.StepWork(dt, 0, false, false, Mathf.Abs(work.Cursor - work.Target) <= work.TargetWidth * .65f);
                else world.StepWork(dt, Mathf.Clamp((work.Target - work.Cursor) * 8f, -1, 1), true, false, false);
                record.frames++; record.deltaSum += dt; yield return null;
            }
            record.complete = !world.WorkActive && world.Model.Budget < record.budgetBefore;
            record.seconds = Time.realtimeSinceStartup - start; record.budgetAfter = world.Model.Budget;
            CheckIt("work completes through real frame input " + expected, record.complete, "wall=" + record.seconds.ToString("F2") + "; dtSum=" + record.deltaSum.ToString("F2") + "; budget=" + record.budgetBefore + "->" + record.budgetAfter);
            if (world.WorkActive) world.CancelWork();
            yield return Wait(.1f);
        }

        IEnumerator PathTo(string name, params Vector3[] points)
        {
            for (int i = 0; i < points.Length && routeOK; i++) yield return MoveTo(name + "/" + i, points[i]);
        }

        IEnumerator MoveTo(string id, Vector3 target, float tolerance = .18f)
        {
            var route = new Route { id = id, section = section, start = world.PlayerPosition, target = target, wallSeconds = Time.realtimeSinceStartup - began };
            report.routes.Add(route); route.samples.Add(route.start);
            float start = Time.realtimeSinceStartup, lastProgress = start, best = FlatDistance(world.PlayerPosition, target);
            while (Time.realtimeSinceStartup - start < 8f)
            {
                Vector3 delta = target - world.PlayerPosition; delta.y = 0;
                if (delta.magnitude <= tolerance && Mathf.Abs(world.PlayerPosition.y - target.y) < .32f) { route.reached = true; break; }
                if (!CanWalk) { route.obstacle = "movement unavailable: " + world.Model.Phase + "/" + Stage; break; }
                if (delta.magnitude < best - .035f) { best = delta.magnitude; lastProgress = Time.realtimeSinceStartup; }
                if (Time.realtimeSinceStartup - lastProgress > 2.5f) { route.obstacle = FirstObstacle(target); break; }
                Vector3 direction = delta.normalized, forward = world.View.transform.forward, right = world.View.transform.right;
                forward.y = right.y = 0; forward.Normalize(); right.Normalize();
                float dt = MoveDt; float scale = Mathf.Clamp01(delta.magnitude / .45f);
                world.SmokeMove(new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)) * scale, dt);
                route.frames++; route.movementDeltaSum += dt;
                if (route.frames % 15 == 0) route.samples.Add(world.PlayerPosition);
                yield return null;
            }
            if (CanWalk) world.SmokeMove(Vector2.zero, MoveDt);
            route.end = world.PlayerPosition; route.samples.Add(route.end); route.seconds = Time.realtimeSinceStartup - start;
            routeOK = route.reached;
            CheckIt("continuous route " + id, route.reached, "target=" + target + "; actual=" + route.end + "; wall=" + route.seconds.ToString("F2") + "; obstacle=" + route.obstacle);
            yield return null; // Do not spend another frame's movement delta at a waypoint boundary.
            if (!routeOK) { yield return Shot("blocked-" + report.routes.Count); Flush(); }
        }

        IEnumerator Wait(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) { if (CanWalk) world.SmokeMove(Vector2.zero, MoveDt); yield return null; }
        }
        IEnumerator WaitStage(PanicJuryStage target, float seconds)
        { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end && world.JuryDemo != null && world.JuryDemo.Stage != target) yield return null; }
        IEnumerator Shot(string id)
        {
            yield return new WaitForEndOfFrame();
            string path = Path.Combine(world.EvidencePath, id + ".png"); ScreenCapture.CaptureScreenshot(path); report.screenshots.Add(path);
            float end = Time.realtimeSinceStartup + 1.2f;
            while (Time.realtimeSinceStartup < end && (!File.Exists(path) || new FileInfo(path).Length < 256)) yield return null;
            if (!File.Exists(path) || new FileInfo(path).Length < 256) report.errors.Add("Missing screenshot " + path);
            Flush();
        }
        void BeginSection(string value) { section = value; sectionBegan = Time.realtimeSinceStartup; routeOK = true; SampleNow(); }
        void Mark(string id)
        { float elapsed = Time.realtimeSinceStartup - shortBegan; report.firstThreeMinutes.Add(new Milestone { id = id, stage = Stage, phase = world.Model.Phase.ToString(), shortFlowSeconds = elapsed, wallSeconds = Time.realtimeSinceStartup - began, position = world.PlayerPosition, before180Seconds = elapsed <= 180f }); }
        bool HasMilestone(string id) { return report.firstThreeMinutes.Any(m => m.id == id && m.before180Seconds); }
        PanicItem Item(string id) { return world.Model.Items.Find(i => i.Id == id); }
        static float FlatDistance(Vector3 a, Vector3 b) { return new Vector2(a.x - b.x, a.z - b.z).magnitude; }
        string FirstObstacle(Vector3 target)
        {
            Vector3 origin = world.PlayerPosition + Vector3.up * .9f, delta = target + Vector3.up * .9f - origin;
            foreach (var hit in Physics.RaycastAll(origin, delta.normalized, Mathf.Min(delta.magnitude, 3), ~(1 << 30), QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
            { if (hit.collider == body || hit.transform.IsChildOf(body.transform) || hit.collider.GetComponentInParent<ResidenceLootPick>()) continue; var pick = hit.collider.GetComponentInParent<HomePick>(); return pick ? pick.Id : hit.collider.name; }
            return "none in center ray; inspect captured collider route";
        }
        void CheckIt(string name, bool pass, string detail = "")
        {
            report.checks.Add(new Check { name = name, pass = pass, detail = detail, section = section, wallSeconds = Time.realtimeSinceStartup - began, sectionSeconds = Time.realtimeSinceStartup - sectionBegan, position = world ? world.PlayerPosition : Vector3.zero, phase = world && world.Model != null ? world.Model.Phase.ToString() : "none", stage = Stage });
            Debug.Log("FULLFLOW_CHECK " + (pass ? "PASS " : "FAIL ") + name + " " + detail);
        }
        void SampleNow()
        {
            if (!world || world.Model == null) return;
            report.timeline.Add(new Sample { wallSeconds = Time.realtimeSinceStartup - began, sectionSeconds = Time.realtimeSinceStartup - sectionBegan, section = section, phase = world.Model.Phase.ToString(), stage = Stage, position = world.PlayerPosition, preparationRemaining = world.Model.TimeRemaining, controllerEnabled = world.PlayerControllerEnabled, grounded = body && body.isGrounded, cheated = world.Model.CheatedRun });
        }
        void Update()
        {
            if (finished || !world) return;
            string phase = world.Model.Phase.ToString(), stage = Stage;
            if (Time.realtimeSinceStartup >= nextSample || phase != lastPhase || stage != lastStage) { SampleNow(); nextSample = Time.realtimeSinceStartup + .5f; lastPhase = phase; lastStage = stage; }
            if (Time.realtimeSinceStartup - began <= HardCapSeconds) return;
            report.errors.Add("Real wall-clock watchdog " + HardCapSeconds + "s exceeded; unfinished sections are not accepted.");
            if (running != null) StopCoroutine(running); Finish();
        }
        void OnLog(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Flush() { if (!string.IsNullOrEmpty(reportPath)) File.WriteAllText(reportPath, JsonUtility.ToJson(report, true)); }
        void Finish()
        {
            if (finished) return; finished = true;
            report.scoreAfter = Hash(report.scorePath); CheckIt("isolated score bytes unchanged", report.scoreBefore == report.scoreAfter, report.scoreBefore + "->" + report.scoreAfter);
            report.finishedUtc = DateTime.UtcNow.ToString("o"); report.wallSeconds = Time.realtimeSinceStartup - began;
            report.totalChecks = report.checks.Count; report.passedChecks = report.checks.Count(c => c.pass);
            report.passed = report.completed && report.errors.Count == 0 && report.checks.All(c => c.pass);
            if (world) world.FullFlowScriptedInput = false;
            Application.logMessageReceived -= OnLog; Flush();
            Debug.Log("FULLFLOW_SMOKE_DONE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.passedChecks + "/" + report.totalChecks + " wall=" + report.wallSeconds.ToString("F2") + " " + reportPath);
            Application.Quit(report.passed ? 0 : 1);
        }
        static string Hash(string path)
        { if (string.IsNullOrEmpty(path) || !File.Exists(path)) return "absent"; using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""); }
    }
}
