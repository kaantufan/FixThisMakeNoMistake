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
    /// <summary>Opt-in V4.9 edge fixtures. This is not OS input or a continuous player route.
    /// Root integrates and runs; worker only stages/compiles. Never assigns Work.Complete/progress.</summary>
    public sealed class PanicE2EMechanicsAudit : MonoBehaviour
    {
        const string Flag = "--e2e-mechanics-audit";
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id, cases, layer, detail; public bool pass; }
        [Serializable] public sealed class Capture { public string path, cases, sha256, phase, inventory; public int frame, budget; }
        [Serializable] public sealed class Report
        {
            public string startedUtc, finishedUtc, profile, scorePath, scoreBefore, scoreAfter, realScoreBefore, realScoreAfter, error;
            public string evidenceType = "Isolated model fixtures + native World production adapters + logical IMGUI callback fixture; NOT OS/HUMAN";
            public string limitations = "Stock, phase, budget and timing fixtures explicitly counted. Setup Warps and InputLocked freeze isolate edge tests. No human timing/fun, physical OS, full campaign or final visual acceptance.";
            public bool completed, passed;
            public int checks, passedChecks, modelFixtures, stateFixtureWrites, testWarps, injectedWorkTicks, developerMutations, worldStarts;
            public float seconds;
            public List<Row> rows = new List<Row>();
            public List<string> errors = new List<string>();
            public List<Capture> captures = new List<Capture>();
        }
        readonly Report report = new Report();
        PanicHomeWorld w; PanicHomeHud hud; PanicInteractionAudit pointer;
        string output, ownLog, realScore, callerScore; float began; bool finished, pending;
        Vector2 click; Exception guiError;
        static readonly FieldInfo ActivePointer = typeof(PanicInteractionAudit).GetField("activeGuiFixture", Hidden | BindingFlags.Static);
        static readonly FieldInfo LogicalPointer = typeof(PanicInteractionAudit).GetField("queuedLogicalPointer", Hidden);
        static readonly MethodInfo HudGui = typeof(PanicHomeHud).GetMethod("OnGUI", Hidden);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, Flag) < 0) return;
            if (args.Any(a => a != Flag && (a.EndsWith("-smoke", StringComparison.Ordinal) || a.EndsWith("-audit", StringComparison.Ordinal))))
            { Debug.LogError("E2E_MECHANICS requires one audit component per process."); Application.Quit(1); return; }
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (!world) { Debug.LogError("E2E_MECHANICS missing real PanicHomeWorld."); Application.Quit(1); return; }
            Application.runInBackground = true;
            world.gameObject.AddComponent<PanicE2EMechanicsAudit>();
        }
        IEnumerator Start()
        {
            began = Time.realtimeSinceStartup; w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>();
            string leaf = w && !string.IsNullOrEmpty(w.EvidencePath) ? Path.GetFileName(Path.GetFullPath(w.EvidencePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) : "";
            bool guid = leaf.Length >= 36 && Guid.TryParse(leaf.Substring(leaf.Length - 36), out _);
            if (!w || !hud || !guid || !Path.IsPathRooted(w.EvidencePath) ||
                Path.GetFullPath(PanicModel.ScorePathOverride ?? "") != Path.GetFullPath(Path.Combine(w.EvidencePath, "isolated-home-scores.json")))
            { Debug.LogError("E2E_MECHANICS requires --home-evidence <absolute fresh GUID directory> and isolated score override."); Application.Quit(1); yield break; }
            output = Path.Combine(w.EvidencePath, "e2e-mechanics.json");
            ownLog = Path.Combine(w.EvidencePath, "e2e-mechanics-checks.log");
            realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            callerScore = PanicModel.ScorePathOverride;
            report.startedUtc = DateTime.UtcNow.ToString("o"); report.profile = leaf; report.scorePath = callerScore;
            report.scoreBefore = Hash(callerScore); report.realScoreBefore = Hash(realScore);
            Application.logMessageReceived += Log;
            if (report.scoreBefore != "absent" || File.Exists(output))
            {
                string rejection = Guid.NewGuid().ToString();
                output = Path.Combine(w.EvidencePath, "e2e-mechanics-rejected-" + rejection + ".json");
                ownLog = Path.Combine(w.EvidencePath, "e2e-mechanics-rejected-" + rejection + ".log");
                report.error = "Profile reused; fresh GUID required. Previous report preserved."; Finish(); yield break;
            }
            w.FullFlowScriptedInput = true; w.InputLocked = true; // Explicit timer freeze; no dev option.
            pointer = gameObject.AddComponent<PanicInteractionAudit>(); pointer.enabled = false;
            var stack = new Stack<IEnumerator>(); stack.Push(Run());
            while (stack.Count > 0 && !finished)
            {
                bool more = false; object current = null;
                try { more = stack.Peek().MoveNext(); if (more) current = stack.Peek().Current; }
                catch (Exception ex) { report.error = ex.ToString(); C("harness.exception", "SYS-022", false, ex.Message); break; }
                if (!more) { stack.Pop(); continue; }
                if (current is IEnumerator nested) stack.Push(nested); else yield return current;
            }
            report.completed = string.IsNullOrEmpty(report.error); Finish();
        }
        IEnumerator Run()
        {
            C("isolation.fresh", "SYS-017", report.scoreBefore == "absent", callerScore);
            while (!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            ModelCases();
            PanicModel.ScorePathOverride = callerScore;
            yield return WorldCases();
            w.ReturnToMenu(); report.worldStarts++;
            C("world.reset", "MEC-043,MEC-045,SYS-017", !w.WorkActive && !w.IsMoving && !w.InventoryOpen && !w.HasConstructionPreview && w.Model.TreeCount == 0 && w.Model.FireToolCount == 0);
        }
        void ModelCases()
        {
            Test("session", SessionCases);
            Test("inventory", InventoryCases);
            Test("bag", BagCases);
            Test("craft", CraftCases);
            foreach (string action in new[] { "anchorwall", "anchorfloor", "upgrade", "fire", "plant", "drain", "fence", "hose" })
            { string copy = action; Test("economy-" + action, () => EconomyCases(copy)); }
            Test("revalidation", RevalidationCases);
            Test("rules", RuleCases);
            Test("regions", RegionCases);
        }
        void Test(string name, Action test)
        { try { test(); } catch (Exception ex) { C(name + ".exception", "SYS-022", false, ex.ToString()); } }
        sealed class Fixture { public PanicModel M; public PanicExpedition E; }
        Fixture New()
        {
            report.modelFixtures++; report.stateFixtureWrites++;
            PanicModel.ScorePathOverride = Path.Combine(w.EvidencePath, "model-fixtures", report.modelFixtures.ToString("000") + ".json");
            var m = new PanicModel(); m.StartRun(false); return new Fixture { M = m, E = new PanicExpedition(m) };
        }
        void SessionCases()
        {
            foreach (string action in new[] { "anchorwall", "anchorfloor", "upgrade", "fire", "plant", "drain", "fence", "hose" })
            {
                var s = new PanicWorkSession(); s.Begin(action, "fixture");
                bool anchor = action.StartsWith("anchor", StringComparison.Ordinal);
                C(action + ".shape", "MEC-101,MEC-104,SYS-017", s.Active && s.UsesDirectClick && s.TotalSteps == (anchor ? 2 : 1) && s.IsWorldConstruction != anchor && s.InstallSeconds == .60f, "MODEL fixture, no target validation until Expedition commit.");
                for (int i = 0; i < 30; i++) Tick(s, .1f, true);
                C(action + ".legacy-inert", "MEC-106", s.Progress == 0 && s.CompletedSteps == 0 && !s.IsInstalling);
                C(action + ".wrong-indices", "MEC-104,SYS-017", !s.TryClick(-1) && !s.TryClick(99) && !s.TryClick(1));
                C(action + ".first-once", "MEC-104,SYS-017", s.TryClick(0) && !s.TryClick(0) && !s.TryClick(1));
                foreach (float dt in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity }) Tick(s, dt);
                C(action + ".invalid-delta", "MEC-059", s.Progress == 0 && s.CompletedSteps == 0 && s.IsInstalling);
                Tick(s, 1000f);
                C(action + ".hitch-cap", "MEC-059", s.CompletedSteps == 0 && s.StageProgress > .16f && s.StageProgress < .17f);
                for (int i = 0; i < 4; i++) Tick(s, .1f);
                Tick(s, .099f);
                C(action + ".pre-600ms", "MEC-104,SYS-017", s.CompletedSteps == 0 && !s.Complete && s.IsInstalling, "Injected work time=.599s; production .1s per-call cap retained.");
                Tick(s, .0011f);
                C(action + ".600ms-boundary", "MEC-104,SYS-017", s.CompletedSteps == 1 && s.Complete == !anchor);
                if (anchor)
                {
                    C(action + ".second-required", "MEC-104", !s.TryClick(0) && s.NextClickTarget == 1 && s.TryClick(1));
                    for (int i = 0; i < 6; i++) Tick(s, .1f);
                }
                C(action + ".terminal", "MEC-104,SYS-017", s.Complete && s.Progress == 1 && !s.TryClick(0) && !s.TryClick(1));
                s.Cancel(); Tick(s, .1f);
                C(action + ".cancel-reset", "MEC-043,SYS-017", !s.Active && !s.Complete && !s.IsInstalling && s.Action == "" && s.TargetId == "" && s.TotalSteps == 0 && s.Progress == 0 && !s.TryClick(0));
            }
            var bad = new PanicWorkSession(); bad.Begin("unknown", "x");
            C("session.unknown", "MEC-059", !bad.Active); bad.Begin("upgrade", "");
            C("session.empty-target", "MEC-059", !bad.Active);
        }
        void InventoryCases()
        {
            var f = New(); var e = f.E;
            C("inventory.capacity", "MEC-027,MEC-029", e.Inventory.Count == 12 && e.Bag.Count == 8);
            var source = new LootSource { Id = "fixture-source", Open = false, Contents = new List<SupplySlot> { new SupplySlot("screws", 5) } }; e.Sources.Add(source); report.stateFixtureWrites++;
            C("inventory.closed", "MEC-026", !e.GatherAt(source.Id, 0) && e.Count("screws") == 0 && source.Contents[0].Count == 5);
            source.Open = true; report.stateFixtureWrites++;
            C("inventory.stack5-once", "MEC-027", e.GatherAt(source.Id, 0) && !e.GatherAt(source.Id, 0) && e.Count("screws") == 5);
            source.Contents[0] = new SupplySlot("screws", 1); report.stateFixtureWrites++;
            C("inventory.stack6-splits", "MEC-027", e.GatherAt(source.Id, 0) && e.Inventory[0].Count == 5 && e.Inventory[1].Count == 1);
            Fill(e.Inventory); source.Contents[0] = new SupplySlot("screws", 2); report.stateFixtureWrites++;
            string before = Stock(e.Inventory);
            C("inventory.full-atomic", "MEC-026", !e.GatherAt(source.Id, 0) && Stock(e.Inventory) == before && source.Contents[0].Count == 2);
            Clear(e.Inventory);
            foreach (string id in new[] { "flashlight", "radio", "blanket", "firstaid", "hose", "extinguisher" })
            {
                source.Contents[0] = new SupplySlot(id, 2); report.stateFixtureWrites++;
                C("inventory.singleton-" + id, "MEC-028", !e.GatherAt(source.Id, 0) && e.Count(id) == 0, "Oversize artificial two-item stack; normal source has one.");
                source.Contents[0] = new SupplySlot(id, 1); report.stateFixtureWrites++;
                C("inventory.legal-single-" + id, "MEC-028", e.GatherAt(source.Id, 0) && e.Count(id) == 1);
            }
            C("inventory.invalid-index", "MEC-031", !e.GatherAt(source.Id, -1) && !e.GatherAt(source.Id, 99) && !e.GatherAt("missing", 0));
            f = New(); foreach (var s in f.E.Sources) foreach (var v in s.Contents) { v.Count = 0; report.stateFixtureWrites++; }
            f.E.OnNewWave();
            C("inventory.renewables-only", "MEC-037", f.E.Sources.SelectMany(s => s.Contents).All(v => new[] { "water", "food", "bandage", "antiseptic", "battery" }.Contains(v.Id) ? v.Count > 0 : v.Count == 0));
        }
        void BagCases()
        {
            var f = New(); var e = f.E; Put(e, 0, "water", 5);
            C("bag.three-water", "MEC-030", e.PackSlot(0) && e.PackSlot(0) && e.PackSlot(0) && f.M.PackedWater == 3 && Count(e.Bag, "water") == 3);
            string before = Stock(e.Inventory) + Stock(e.Bag); int water = f.M.WaterStock;
            C("bag.fourth-rollback", "MEC-030", !e.PackSlot(0) && before == Stock(e.Inventory) + Stock(e.Bag) && f.M.WaterStock == water && e.Count("water") == 2);
            C("bag.bad-indices", "MEC-031", !e.PackSlot(-1) && !e.PackSlot(99) && !e.PackSlot(11));
            f = New(); e = f.E; Fill(e.Bag); Put(e, 0, "flashlight", 1); Put(e, 1, "battery", 2); before = Stock(e.Inventory) + Stock(e.Bag);
            C("bag.full-atomic", "MEC-029", !e.PackSlot(0) && before == Stock(e.Inventory) + Stock(e.Bag) && f.M.PackedSupportMask == 0 && f.M.SupportOwnedMask == 0);
            Clear(e.Bag); Put(e, 1, "battery", 0);
            C("bag.battery-required", "MEC-032", !e.PackSlot(0) && e.Count("flashlight") == 1 && f.M.PackedSupportMask == 0);
            Put(e, 1, "battery", 2);
            C("bag.battery-once", "MEC-032", e.PackSlot(0) && e.Count("battery") == 1 && Count(e.Bag, "flashlight") == 1);
            Put(e, 0, "flashlight", 1); before = Stock(e.Inventory) + Stock(e.Bag);
            C("bag.duplicate-support", "MEC-033", !e.PackSlot(0) && before == Stock(e.Inventory) + Stock(e.Bag));
            foreach (string id in new[] { "battery", "screws", "bracket", "plank", "sandbag", "seedling", "hose", "extinguisher" })
            { Put(e, 0, id, 1); before = Stock(e.Inventory) + Stock(e.Bag); C("bag.material-" + id, "MEC-034", !e.PackSlot(0) && before == Stock(e.Inventory) + Stock(e.Bag)); }
        }
        void CraftCases()
        {
            var f = New(); Fill(f.E.Inventory); Put(f.E, 0, "bandage", 3); Put(f.E, 1, "antiseptic", 2);
            string before = Stock(f.E.Inventory);
            C("craft.no-output-space", "MEC-035", !f.E.CraftFirstAid() && before == Stock(f.E.Inventory));
            Put(f.E, 0, "bandage", 2); Put(f.E, 1, "antiseptic", 1);
            C("craft.consumption-releases-slot", "MEC-035", f.E.CraftFirstAid() && f.E.Count("bandage") == 0 && f.E.Count("antiseptic") == 0 && f.E.Count("firstaid") == 1);
        }
        string Target(Fixture f, string action)
        {
            if (action == "upgrade") return f.M.Items.First(i => i.Installed && i.Kind == ItemKind.Wall).Id;
            return action == "fire" ? PanicItemActionRules.FireStationId : action == "plant" ? "plant_west1" : action == "drain" ? "drain_west" : action == "fence" ? "fence_west" : action == "hose" ? "hose_station" : "bookshelf";
        }
        void Recipe(Fixture f, string action, string target)
        {
            Clear(f.E.Inventory); var item = f.M.Items.Find(i => i.Id == target); int n = 0;
            foreach (var cost in PanicExpedition.MaterialCosts(action, item)) Put(f.E, n++, cost.Id, cost.Required);
        }
        void Drive(PanicWorkSession session)
        {
            int remaining = 40;
            while (session.Active && !session.Complete && remaining-- > 0)
            { if (!session.IsInstalling) session.TryClick(session.NextClickTarget); Tick(session, .1f); }
            if (!session.Complete) throw new InvalidOperationException("Direct work failed bounded production Tick driver.");
        }
        void EconomyCases(string action)
        {
            foreach (int offset in new[] { -1, 0, 1 })
            {
                var f = New(); string target = Target(f, action); Recipe(f, action, target);
                int cost = PanicExpedition.WorkCost(action, f.M.Items.Find(i => i.Id == target)); f.M.Budget = cost + offset; report.stateFixtureWrites++;
                string before = Stock(f.E.Inventory); bool begin = f.E.CanBeginWork(action, target);
                C(action + ".budget-boundary-" + offset, "MEC-040,MEC-041", begin == (offset >= 0) && before == Stock(f.E.Inventory) && f.M.Budget == cost + offset);
                if (!begin) continue;
                f.E.Work.Begin(action, target); Drive(f.E.Work);
                C(action + ".no-early-charge-" + offset, "MEC-042", before == Stock(f.E.Inventory) && f.M.Budget == cost + offset);
                bool done = f.E.TryCompleteWork(action, target);
                C(action + ".exact-commit-" + offset, "MEC-040,MEC-044", done && f.M.Budget == offset && f.E.Inventory.All(s => s.Count == 0));
                before = Stock(f.E.Inventory);
                C(action + ".duplicate-inert-" + offset, "MEC-044", !f.E.TryCompleteWork(action, target) && f.M.Budget == offset && before == Stock(f.E.Inventory));
            }
            var shortage = New(); string id = Target(shortage, action); Recipe(shortage, action, id); shortage.E.Inventory[0].Count--; report.stateFixtureWrites++;
            string stock = Stock(shortage.E.Inventory);
            C(action + ".material-minus-one", "MEC-041", !shortage.E.CanBeginWork(action, id) && shortage.E.MissingRequirements.Sum(v => v.Missing) == 1 && Stock(shortage.E.Inventory) == stock);
        }
        void RevalidationCases()
        {
            foreach (string mutation in new[] { "phase", "stock", "budget", "installed" })
            {
                var f = New(); Recipe(f, "anchorwall", "bookshelf"); f.E.Work.Begin("anchorwall", "bookshelf"); Drive(f.E.Work);
                var item = f.M.Items.Find(i => i.Id == "bookshelf");
                if (mutation == "phase") f.M.Phase = PanicPhase.Disaster;
                if (mutation == "stock") f.E.Inventory[0].Count = 0;
                if (mutation == "budget") f.M.Budget = 59;
                if (mutation == "installed") item.Installed = false;
                report.stateFixtureWrites++; string before = Stock(f.E.Inventory); int budget = f.M.Budget;
                C("revalidate." + mutation, "MEC-045", !f.E.TryCompleteWork("anchorwall", "bookshelf") && !item.Anchored && budget == f.M.Budget && before == Stock(f.E.Inventory));
                f.E.Work.Cancel();
            }
            var own = New(); Recipe(own, "anchorwall", "bookshelf"); own.E.Work.Begin("anchorwall", "bookshelf"); Drive(own.E.Work); bool first = own.E.TryCompleteWork("anchorwall", "bookshelf");
            bool released = own.M.ToggleAnchor("bookshelf"); int money = own.M.Budget; Clear(own.E.Inventory); own.E.Work.Begin("anchorwall", "bookshelf"); Drive(own.E.Work);
            C("reanchor.model-free", "MEC-046", first && released && own.E.TryCompleteWork("anchorwall", "bookshelf") && own.M.Budget == money && own.E.Inventory.All(s => s.Count == 0), "MODEL ToggleAnchor release; World keyboard path separately checked.");
            var up = New(); string id = Target(up, "upgrade"); var itemUp = up.M.Items.Find(i => i.Id == id);
            for (int tier = 0; tier < 2; tier++) { Recipe(up, "upgrade", id); up.E.Work.Begin("upgrade", id); Drive(up.E.Work); C("tier.commit-" + tier, "MEC-048", up.E.TryCompleteWork("upgrade", id) && itemUp.Tier == tier + 1); }
            int cash = up.M.Budget;
            C("tier.cap", "MEC-048", !up.E.CanBeginWork("upgrade", id) && itemUp.Tier == 2 && up.M.Budget == cash);
        }
        void RuleCases()
        {
            var f = New(); int n = 0;
            foreach (var item in f.M.Items)
            {
                string primary = PanicItemActionRules.PrimaryAction(item); int shortcut = PanicItemActionRules.PrimaryShortcut(item);
                C("rules.primary-" + n, "MEC-107,SYS-053", string.IsNullOrEmpty(primary) ? shortcut == 0 : shortcut > 0 && PanicItemActionRules.Allows(primary, item, item.Id));
                C("rules.no-furniture-fire-" + n++, "MEC-052,MEC-107", !f.E.CanBeginWork("fire", item.Id));
            }
            C("rules.station-only", "MEC-107", PanicItemActionRules.Allows("fire", null, PanicItemActionRules.FireStationId) && !f.E.CanBeginWork("fire", "missing"));
            C("rules.flood-superseded", "MEC-051", !PanicItemActionRules.Allows("flood", f.M.Items.First(i => i.Kind == ItemKind.Window), f.M.Items.First(i => i.Kind == ItemKind.Window).Id), "V4.9 shared rule intentionally hides generic flood shortcut; drain/fence remain.");
        }
        void RegionCases()
        {
            var f = New(); int money = f.M.Budget;
            foreach (string id in new[] { "plant_west1", "plant_west2", "plant_east" })
            {
                Recipe(f, "plant", id); f.E.Work.Begin("plant", id); Drive(f.E.Work);
                C("region.commit-" + id, "MEC-115", f.E.TryCompleteWork("plant", id) && f.E.IsWorkCommitted("plant", id));
                int cash = f.M.Budget;
                C("region.duplicate-" + id, "MEC-049,MEC-115", !f.E.CanBeginWork("plant", id) && f.M.Budget == cash);
            }
            C("region.total", "MEC-115", f.M.TreeCount == 3 && f.M.Budget == money - 90 && !f.E.CanBeginWork("plant", "unknown-region"));
            f.M.ReturnToTitle(); f.M.StartRun(false); f.E.Reset(); report.stateFixtureWrites++;
            C("region.reset", "MEC-115", f.M.TreeCount == 0 && !f.E.IsWorkCommitted("plant", "plant_west1"));
        }
        IEnumerator Place(Vector3 position, string focus)
        {
            w.Warp(position); report.testWarps++;
            for (int i = 0; i < 12; i++) { w.SmokeMove(Vector2.zero, .02f); yield return null; }
            w.SetFocus(focus); yield return null;
            C("fixture.reachable-" + focus, "SYS-017", w.CanInteract && w.PlayerControllerEnabled, w.PlayerPosition.ToString());
        }
        void StartSimulation(DisasterType type)
        { w.ReturnToMenu(); w.StartDisasterSimulation(type); report.worldStarts++; w.InputLocked = true; }
        void NativeStep(float dt, bool cancel = false)
        { report.injectedWorkTicks++; w.ProcessWorkInput(dt, 0, false, false, false, cancel); }
        bool NativeClick(int index)
        { bool old = w.InputLocked; w.InputLocked = false; try { return w.ClickWorkTarget(index); } finally { w.InputLocked = old; } }
        IEnumerator WorldCases()
        {
            StartSimulation(DisasterType.Earthquake); yield return Place(new Vector3(6.5f, .05f, 3.25f), "fridge");
            string inv = Stock(w.Expedition.Inventory); var source = w.NearestSource;
            bool locked = w.InputLocked; w.InputLocked = false;
            w.DispatchWorldInput(new[] { KeyCode.E, KeyCode.F }, false, false); w.InputLocked = locked;
            C("world.same-frame-EF", "MEC-016,SYS-016", source.Open && inv == Stock(w.Expedition.Inventory) && w.DiscreteInputFrame == Time.frameCount, "One production dispatcher call; E wins; semantic input, NOT OS.", "NATIVE_API");
            yield return Shot("01-loot-open", "MEC-016,MEC-023,MEC-025,SYS-031");
            yield return null;
            w.HandleKey(KeyCode.F);
            C("world.selected-loot", "MEC-023,MEC-025", w.Expedition.Count("water") > 0 && w.Expedition.Count("food") == 0, "Production TakeLoot from real open fridge; not mouse/OS claim.", "NATIVE_API");
            w.ToggleInventory(); yield return Shot("02-inventory-real-loot", "MEC-025,MEC-027,SYS-031"); w.ToggleInventory();
            w.HandleKey(KeyCode.E); yield return null;
            w.BeginWork("anchorwall", "fridge");
            C("world.anchor-began", "MEC-042,SYS-017", w.WorkActive && !w.Expedition.Work.IsWorldConstruction && w.Model.Budget == 1000 && w.Expedition.Count("screws") == 2, w.Model.Message, "NATIVE_API");
            if (!w.WorkActive) { report.error = "Anchor fixture could not establish; remaining World cases NOT_RUN."; yield break; }
            Rect a = hud.WorkClickTargetBounds(0), b = hud.WorkClickTargetBounds(1);
            C("world.two-hitboxes", "MEC-102,MEC-103", a.width == 76 && b.width == 76 && !a.Overlaps(b), a + "; " + b, "NATIVE_API");
            yield return Shot("03-two-real-screws", "MEC-102,MEC-103,SYS-032");
            yield return GuiClick(new Vector2(106, 200));
            C("world.wrong-click", "MEC-104", !w.Expedition.Work.IsInstalling && w.Expedition.Work.Progress == 0, "Actual OnGUI with logical pointer fixture outside screw hitboxes.", "NATIVE_API");
            yield return GuiClick(b.center);
            C("world.future-click", "MEC-104", !w.Expedition.Work.IsInstalling && w.Expedition.Work.CompletedSteps == 0, "Inactive screw IMGUI region cannot begin.", "NATIVE_API");
            yield return GuiClick(a.center);
            C("world.first-click", "MEC-104,SYS-017", w.Expedition.Work.IsInstalling, "Actual production HUD callback, logical coordinates, no OS input.", "NATIVE_API");
            NativeStep(.1f); NativeStep(.1f); NativeStep(.1f);
            yield return Shot("04-screw-installing", "MEC-104,SYS-017");
            C("world.busy-spam", "MEC-104,SYS-017", !NativeClick(0) && !NativeClick(1) && !NativeClick(-1), "World API busy branch.", "NATIVE_API");
            for (int i = 0; i < 3; i++) NativeStep(.1f);
            C("world.first-no-charge", "MEC-042,MEC-104", w.WorkActive && w.Expedition.Work.CompletedSteps == 1 && w.Model.Budget == 1000 && w.Expedition.Count("screws") == 2, "Injected ticks explicitly counted.", "NATIVE_API");
            yield return null;
            C("world.second-click", "MEC-104", NativeClick(1), "World API second screw.", "NATIVE_API");
            for (int i = 0; i < 6; i++) NativeStep(.1f);
            var fridge = w.Model.Items.Find(i => i.Id == "fridge");
            C("world.commit-once", "MEC-044,MEC-104,SYS-017", !w.WorkActive && fridge.Anchored && w.Model.Budget == 940 && w.Expedition.Count("screws") == 0 && w.Expedition.Count("bracket") == 0, "World CompleteWork and exact finite recipe.", "NATIVE_API");
            yield return Shot("05-anchor-committed", "MEC-044,MEC-046,MEC-104");
            w.CompleteWork(); C("world.duplicate-callback", "MEC-044", w.Model.Budget == 940, "Late callback inert.", "NATIVE_API");
            yield return null;
            w.QuickAction(1);
            C("world.release-anchor", "MEC-046", !fridge.Anchored && fridge.AnchorOwned && w.Model.Budget == 940, "Player shortcut must release owned restraint; SOURCE_CANDIDATE if it remains anchored. Message=" + w.Model.Message, "NATIVE_API");
            yield return null;
            yield return GuiClick(new Vector2(1337,247));
            C("world.reanchor-offers-two-screws", "MEC-046,MEC-104", w.WorkActive&&w.Expedition.Work.TotalSteps==2&&w.Expedition.Work.NextClickTarget==0,"Owned anchor reuses its actual two-click work without new material.","NATIVE_API");
            NativeClick(0);for(int i=0;i<6;i++)NativeStep(.1f);yield return null;NativeClick(1);for(int i=0;i<6;i++)NativeStep(.1f);yield return null;
            C("world.reanchor-free-owned", "MEC-046", fridge.Anchored&&fridge.AnchorOwned&&w.Model.Budget==940&&w.Expedition.Count("screws")==0&&w.Expedition.Count("bracket")==0,"Two real timed screws, no duplicate recipe or budget.","NATIVE_API");
            yield return GuiClick(new Vector2(1337,247));
            C("world.release-anchor-GUI", "MEC-046", !fridge.Anchored&&fridge.AnchorOwned&&w.Model.Budget==940,"Actual contextual ÇÖZ OnGUI callback releases once without refund.","NATIVE_API");

            foreach (bool anchor in new[] { true, false })
            {
                StartSimulation(anchor ? DisasterType.Earthquake : DisasterType.Fire);
                yield return Place(anchor ? new Vector3(6.5f, .05f, 3.25f) : new Vector3(.9f, .05f, -2.2f), anchor ? "fridge" : PanicItemActionRules.FireStationId);
                string action = anchor ? "anchorwall" : "fire", target = anchor ? "fridge" : PanicItemActionRules.FireStationId;
                w.BeginWork(action, target); yield return null;
                bool began = w.WorkActive;
                if (anchor) { NativeClick(0); for (int i = 0; i < 6; i++) NativeStep(.1f); yield return null; NativeClick(1); }
                for (int i = 0; i < 5; i++) NativeStep(.1f); NativeStep(.099f);
                inv = Stock(w.Expedition.Inventory); int cash = w.Model.Budget;
                C("world.cancel-preboundary-" + action, "MEC-105", began && w.WorkActive && !w.Expedition.Work.Complete, "Explicit .599s pending final stage.", "NATIVE_API");
                NativeStep(.005f, true);
                C("world.cancel-priority-" + action, "MEC-043,MEC-105,SYS-017", !w.WorkActive && inv == Stock(w.Expedition.Inventory) && cash == w.Model.Budget && !w.Expedition.IsWorkCommitted(action, target) && !w.HasConstructionPreview && !w.Model.Items.Find(i => i.Id == "fridge").Anchored && w.Model.FireToolCount == 0, "Cancel and completion-crossing dt supplied in one ProcessWorkInput call; cancel wins.", "NATIVE_API");
                yield return Shot("cancel-" + action, "MEC-043,MEC-105,SYS-017");
            }
            StartSimulation(DisasterType.Fire); yield return Place(new Vector3(.9f, .05f, -2.2f), PanicItemActionRules.FireStationId);
            w.BeginWork("fire", PanicItemActionRules.FireStationId);
            C("world.station-world-operation", "MEC-108,MEC-110", w.WorkActive && w.WorldConstructionVisible && w.HasConstructionPreview && w.Expedition.Count("extinguisher") == 1, "Dedicated actual station; fixture start inventory.", "NATIVE_API");
            for (int i = 0; i < 3; i++) NativeStep(.1f);
            yield return Shot("08-station-installing", "MEC-108,MEC-110");
            for (int i = 0; i < 3; i++) NativeStep(.1f);
            C("world.station-real-stock", "MEC-108", !w.WorkActive && w.Expedition.Count("extinguisher") == 0 && w.Model.FireToolCount == 1 && w.Model.Budget == 925, "Current production recipe consumes one real loose extinguisher.", "NATIVE_API");
            yield return Shot("09-station-committed", "MEC-108,MEC-110");
            yield return null;
            StartSimulation(DisasterType.Storm); yield return Place(new Vector3(0, .05f, 7.1f), "wall_north");
            w.BeginWork("upgrade", "wall_north");
            for (int i = 0; i < 3; i++) NativeStep(.1f);
            yield return Shot("10-structure-installing", "MEC-110,MEC-111");
            for (int i = 0; i < 3; i++) NativeStep(.1f);
            yield return Shot("11-structure-T1", "MEC-111,MEC-112");
            w.BeginWork("upgrade", "wall_north");
            C("world.requirements-T2", "MEC-039,MEC-041", !w.WorkActive && w.Expedition.HasMissingRequirements && w.Expedition.CurrentRequirements.Any(r => r.Missing > 0), "Second-tier recipe after actual finite T1 consumption.", "NATIVE_API");
            yield return Shot("12-exact-missing-costs", "MEC-039,MEC-041");
            StartSimulation(DisasterType.Earthquake); yield return Place(new Vector3(-4.5f, 3.25f, 2.9f), "bed");
            inv = Stock(w.Expedition.Inventory); int budget = w.Model.Budget;
            var bed = w.Model.Items.Find(i => i.Id == "bed");
            C("world.bed-rule-hidden", "MEC-107,SYS-053", bed != null && PanicItemActionRules.PrimaryShortcut(bed) == 0 && string.IsNullOrEmpty(PanicItemActionRules.PrimaryAction(bed)), "Shared HUD rule returns no preparation button.", "NATIVE_API");
            w.QuickAction(1); w.QuickAction(2); w.QuickAction(3); w.BeginWork("fire", "bed");
            C("world.bed-key-api-reject", "MEC-107,SYS-053", !w.WorkActive && !bed.Anchored && !bed.FireProtected && bed.Tier == 0 && budget == w.Model.Budget && inv == Stock(w.Expedition.Inventory), "Keyboard adapter/API reject; actual OnGUI visibility also source-reviewed.", "NATIVE_API");
            yield return Shot("13-bed-no-inappropriate-action", "MEC-107,SYS-053");
        }
        IEnumerator Shot(string id, string cases)
        {
            int startFrame = Time.frameCount;
            while (Time.frameCount <= startFrame + 1) yield return null;
            yield return new WaitForEndOfFrame();
            string path = Path.Combine(w.EvidencePath, id + ".png");
            ScreenCapture.CaptureScreenshot(path);
            float until = Time.realtimeSinceStartup + 3;
            while (!File.Exists(path) && Time.realtimeSinceStartup < until) yield return null;
            var capture = new Capture { path = path, cases = cases, sha256 = Hash(path), frame = Time.frameCount, phase = w.Model.Phase.ToString(), budget = w.Model.Budget, inventory = Stock(w.Expedition.Inventory) };
            report.captures.Add(capture);
            C("capture." + id, cases, File.Exists(path) && new FileInfo(path).Length > 1000 && Time.frameCount > startFrame, "PNG=" + path + "; SHA256=" + capture.sha256 + "; monotonic frame=" + capture.frame + "; annotated by JSON, original image untouched.", "NATIVE_API");
        }
        IEnumerator GuiClick(Vector2 position)
        {
            if (ActivePointer == null || LogicalPointer == null || HudGui == null) throw new InvalidOperationException("Logical IMGUI fixture metadata missing.");
            click = position; pending = true; float until = Time.realtimeSinceStartup + 2;
            while (pending && Time.realtimeSinceStartup < until) yield return null;
            C("gui.dispatch-" + report.rows.Count, "SYS-022", !pending && guiError == null, guiError == null ? "Semantic OnGUI callback; not OS." : guiError.ToString(), "NATIVE_API");
            yield return null;
        }
        void OnGUI()
        {
            if (!pending || Event.current.type != EventType.Repaint) return;
            pending = false; Event old = new Event(Event.current); Matrix4x4 matrix = GUI.matrix; bool oldLock = w.InputLocked;
            try
            {
                w.InputLocked = false; GUI.matrix = Matrix4x4.identity;
                LogicalPointer.SetValue(pointer, click); ActivePointer.SetValue(null, pointer);
                Event.current = new Event { type = EventType.MouseDown, button = 0, mousePosition = click }; HudGui.Invoke(hud, null);
            }
            catch (Exception ex) { guiError = ex; }
            finally { ActivePointer.SetValue(null, null); w.InputLocked = oldLock; Event.current = old; GUI.matrix = matrix; }
        }
        void Tick(PanicWorkSession session, float dt, bool legacy = false)
        { report.injectedWorkTicks++; session.Tick(dt, legacy ? 1 : 0, legacy, legacy, legacy); }
        void Put(PanicExpedition e, int slot, string id, int count) { e.Inventory[slot] = new SupplySlot(id, count); report.stateFixtureWrites++; }
        void Clear(List<SupplySlot> slots) { for (int i = 0; i < slots.Count; i++) slots[i] = new SupplySlot(); report.stateFixtureWrites++; }
        void Fill(List<SupplySlot> slots) { for (int i = 0; i < slots.Count; i++) slots[i] = new SupplySlot("fixture-filler", 5); report.stateFixtureWrites++; }
        static int Count(List<SupplySlot> slots, string id) { return slots.Where(s => s != null && s.Id == id).Sum(s => s.Count); }
        static string Stock(List<SupplySlot> slots) { return string.Join("|", slots.Select(s => s == null ? "null" : s.Id + ":" + s.Count)); }
        void C(string id, string cases, bool pass, string detail = "", string layer = "MODEL")
        {
            report.rows.Add(new Row { id = id, cases = cases, pass = pass, layer = layer, detail = detail });
            string message = "E2E_MECHANICS_CHECK " + (pass ? "PASS " : "FAIL ") + id + " cases=" + cases + " layer=" + layer + " " + detail;
            Debug.Log(message); if (ownLog != null) File.AppendAllText(ownLog, message + Environment.NewLine); Flush();
        }
        void Update() { if (!finished && began > 0 && Time.realtimeSinceStartup - began > 250) { report.error = "250 second watchdog"; Finish(); } }
        void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception) report.errors.Add(message + "\n" + stack); }
        void Flush() { if (output != null) File.WriteAllText(output, JsonUtility.ToJson(report, true)); }
        void Finish()
        {
            if (finished) return; finished = true; pending = false;
            PanicModel.ScorePathOverride = callerScore;
            report.scoreAfter = Hash(callerScore); report.realScoreAfter = Hash(realScore);
            C("score.bytes-preserved", "SYS-017", report.scoreBefore == report.scoreAfter && report.realScoreBefore == report.realScoreAfter, "Isolated and real score hashes only, no private score contents.");
            report.checks = report.rows.Count; report.passedChecks = report.rows.Count(r => r.pass); report.seconds = Time.realtimeSinceStartup - began; report.finishedUtc = DateTime.UtcNow.ToString("o");
            report.passed = report.completed && report.checks == report.passedChecks && report.errors.Count == 0 && string.IsNullOrEmpty(report.error);
            Application.logMessageReceived -= Log; Flush();
            Debug.Log("E2E_MECHANICS_DONE " + report.passedChecks + "/" + report.checks + " passed=" + report.passed + " completed=" + report.completed);
            Application.Quit(report.passed ? 0 : 1);
        }
        static string Hash(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return "absent";
            using (var stream = File.OpenRead(path)) using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
        }
    }
}
