using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Synchronous model/API audit. Call only from the isolated native audit entrypoint.
    /// Uses explicit stock/phase/threshold fixtures; never sends OS input, moves an actor,
    /// touches the world, or claims human gameplay acceptance. Work completes through Tick.
    /// Every fresh model gets a unique temporary score file; the caller override is restored.
    /// </summary>
    public static class PanicInteractionModelAudit
    {
        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            new Audit(check).Run();
        }

        sealed class Fixture
        {
            public PanicModel M;
            public PanicExpedition E;
            public string ScorePath;
        }

        sealed class Audit
        {
            readonly Action<string, bool, string> check;
            readonly string scoreRoot = Path.Combine(Path.GetTempPath(), "TooLateToPanic-model-audit", Guid.NewGuid().ToString("N"));
            int fixtureNumber;
            string group;
            int checkNumber;

            public Audit(Action<string, bool, string> check) { this.check = check; }

            public void Run()
            {
                string callerPath = PanicModel.ScorePathOverride;
                try
                {
                    Case("inventory", Inventory);
                    Case("bag", Bag);
                    Case("craft", Craft);
                    Case("requirements", Requirements);
                    Case("work-invalid-input", WorkInvalidInput);
                    Case("work-boundaries", WorkBoundaries);
                    foreach (string action in new[] { "anchorwall", "drain", "fire" })
                    {
                        string a = action;
                        Case("work-normal-" + a, () => WorkFamily(a, false));
                        Case("work-short-" + a, () => WorkFamily(a, true));
                    }
                    Case("all-work-actions", AllWorkActions);
                    Case("work-atomicity", WorkAtomicity);
                    Case("phase-guards", PhaseGuards);
                    for (int mask = 0; mask < 16; mask++)
                    {
                        int value = mask;
                        Case("developer-mask-" + value, () => DeveloperMask(value));
                    }
                    Case("developer-score", DeveloperScores);
                    Case("deadline-retry", DeadlineRetry);
                    Case("damage-threshold-fixture", DamageThreshold);
                    Case("production-disaster", ProductionDisaster);
                    Case("jury-deadline", JuryDeadline);
                    Case("jury-skip-retry", JurySkipRetry);
                }
                finally
                {
                    PanicModel.ScorePathOverride = callerPath;
                    check("model-audit.score-path-restored", PanicModel.ScorePathOverride == callerPath,
                        "Temporary score root: " + scoreRoot + "; fixture/API audit, no OS/world/human claims.");
                }
            }

            void Case(string name, Action action)
            {
                group = "model-audit." + name; checkNumber = 0;
                try { action(); }
                catch (Exception exception)
                {
                    Exception cause = exception is TargetInvocationException && exception.InnerException != null ? exception.InnerException : exception;
                    check(group + ".exception", false, cause.GetType().Name + ": " + cause.Message);
                }
            }

            void C(bool pass, string detail)
            {
                check(group + "." + (++checkNumber).ToString("00"), pass, detail);
            }

            Fixture New(bool start = true)
            {
                string path = Path.Combine(scoreRoot, (++fixtureNumber).ToString("000") + ".json");
                PanicModel.ScorePathOverride = path;
                var m = new PanicModel();
                if (start) m.StartRun(false);
                return new Fixture { M = m, E = new PanicExpedition(m), ScorePath = path };
            }

            static string Stock(List<SupplySlot> slots)
            {
                var parts = new List<string>(slots.Count);
                foreach (var slot in slots) parts.Add(slot == null ? "<null>" : slot.Id + ":" + slot.Count);
                return string.Join("|", parts.ToArray());
            }

            static int Count(List<SupplySlot> slots, string id)
            {
                int n = 0;
                foreach (var slot in slots) if (slot != null && slot.Id == id) n += slot.Count;
                return n;
            }

            static void Clear(List<SupplySlot> slots)
            {
                for (int i = 0; i < slots.Count; i++) slots[i] = new SupplySlot();
            }

            static void Fill(List<SupplySlot> slots, string id = "fixture-filler", int count = 5)
            {
                for (int i = 0; i < slots.Count; i++) slots[i] = new SupplySlot(id, count);
            }

            static PanicItem Item(Fixture f, string id)
            {
                foreach (var item in f.M.Items) if (item.Id == id) return item;
                throw new InvalidOperationException("Expected initial item missing: " + id);
            }

            static string Structure(Fixture f)
            {
                foreach (var item in f.M.Items) if (item.Installed && item.Kind == ItemKind.Wall) return item.Id;
                throw new InvalidOperationException("Initial installed wall missing");
            }

            static LootSource Source(Fixture f, params SupplySlot[] contents)
            {
                var source = new LootSource { Id = "audit-source", Open = true, Label = "Audit fixture", Contents = new List<SupplySlot>(contents) };
                f.E.Sources.Add(source);
                return source;
            }

            static void Costs(Fixture f, string action, string target)
            {
                Clear(f.E.Inventory);
                PanicItem item = null;
                foreach (var candidate in f.M.Items) if (candidate.Id == target) item = candidate;
                var costs = PanicExpedition.MaterialCosts(action, item);
                for (int i = 0; i < costs.Count; i++) f.E.Inventory[i] = new SupplySlot(costs[i].Id, costs[i].Required);
            }

            void Inventory()
            {
                var f = New();
                C(f.E.Inventory.Count == 12 && f.E.Bag.Count == 8, "Fresh capacities are 12 inventory slots and 8 bag slots.");
                var source = Source(f, new SupplySlot("screws", 5));
                source.Open = false;
                C(!f.E.GatherAt(source.Id, 0) && source.Contents[0].Count == 5 && f.E.Count("screws") == 0, "Closed source refuses gathering atomically.");
                f.E.ToggleSource(source.Id);
                C(f.E.GatherAt(source.Id, 0) && f.E.Count("screws") == 5 && source.Contents[0].Count == 0, "Exact five-unit stack transfers once through selected-index API.");
                C(!f.E.GatherAt(source.Id, 0) && f.E.Count("screws") == 5, "Gathering an exhausted selected slot cannot duplicate it.");
                source.Contents[0] = new SupplySlot("screws", 1);
                C(f.E.GatherAt(source.Id, 0) && f.E.Inventory[0].Count == 5 && f.E.Inventory[1].Count == 1, "Sixth unit creates a second stack; first stack remains at five.");
                Fill(f.E.Inventory);
                f.E.Inventory[11] = new SupplySlot();
                source.Contents[0] = new SupplySlot("bandage", 5);
                C(f.E.GatherAt(source.Id, 0) && f.E.Inventory[11].Id == "bandage" && f.E.Inventory[11].Count == 5, "Twelfth inventory slot accepts a complete legal stack.");
                source.Contents[0] = new SupplySlot("antiseptic", 1);
                string before = Stock(f.E.Inventory);
                C(!f.E.GatherAt(source.Id, 0) && source.Contents[0].Count == 1 && Stock(f.E.Inventory) == before, "Thirteenth occupied slot is refused without partial transfer.");
                source.Contents[0] = new SupplySlot("bandage", 6);
                Clear(f.E.Inventory);
                C(!f.E.GatherAt(source.Id, 0) && source.Contents[0].Count == 6 && f.E.Count("bandage") == 0, "Oversize six-unit source fixture is rejected; no illegal stack appears.");
                source.Contents[0] = new SupplySlot("flashlight", 2);
                C(!f.E.GatherAt(source.Id, 0) && f.E.Count("flashlight") == 0, "Single-item support stack limit is one.");
                C(!f.E.GatherAt(source.Id, -1) && !f.E.GatherAt(source.Id, 99) && !f.E.GatherAt("missing", 0), "Invalid source/index requests are bounded refusals.");
            }

            void Bag()
            {
                var f = New();
                f.E.Inventory[0] = new SupplySlot("water", 5);
                C(f.E.PackSlot(0) && f.E.PackSlot(0) && f.E.PackSlot(0) && f.M.PackedWater == 3 && Count(f.E.Bag, "water") == 3 && f.E.Count("water") == 2, "Packing moves one unit per call and preserves model/bag agreement.");
                string inv = Stock(f.E.Inventory), bag = Stock(f.E.Bag);
                C(!f.E.PackSlot(0) && f.M.PackedWater == 3 && f.M.WaterStock == 0 && Stock(f.E.Inventory) == inv && Stock(f.E.Bag) == bag, "Fourth daily water is refused with rollback of temporary WaterStock.");
                f.E.Inventory[1] = new SupplySlot("food", 1);
                C(f.E.PackSlot(1) && f.M.BagReady && f.M.PackedFood == 1 && f.E.Inventory[1].Count == 0 && f.E.Inventory[1].Id == "", "Last food packs, clears its slot, and satisfies one-day bag.");
                f.E.Inventory[2] = new SupplySlot("flashlight", 1);
                inv = Stock(f.E.Inventory); bag = Stock(f.E.Bag);
                C(!f.E.PackSlot(2) && Stock(f.E.Inventory) == inv && Stock(f.E.Bag) == bag && f.M.PackedSupportMask == 0, "Flashlight without battery is refused atomically.");
                f.E.Inventory[3] = new SupplySlot("battery", 1);
                C(f.E.PackSlot(2) && f.E.Count("battery") == 0 && f.E.Count("flashlight") == 0 && Count(f.E.Bag, "flashlight") == 1 && f.M.SupportCount == 1, "Battery-powered support consumes exactly one battery and one source support.");
                f.E.Inventory[2] = new SupplySlot("flashlight", 1); f.E.Inventory[3] = new SupplySlot("battery", 1);
                inv = Stock(f.E.Inventory); bag = Stock(f.E.Bag);
                C(!f.E.PackSlot(2) && Stock(f.E.Inventory) == inv && Stock(f.E.Bag) == bag && f.M.SupportCount == 1, "Duplicate support cannot consume a second battery.");
                f.E.Inventory[4] = new SupplySlot("bracket", 1);
                C(!f.E.PackSlot(4) && f.E.Count("bracket") == 1, "Preparation materials are not bag supplies.");

                f = New(); Fill(f.E.Bag, "fixture-occupied", 1); f.E.Inventory[0] = new SupplySlot("water", 1);
                inv = Stock(f.E.Inventory); bag = Stock(f.E.Bag);
                C(!f.E.PackSlot(0) && Stock(f.E.Inventory) == inv && Stock(f.E.Bag) == bag && f.M.PackedWater == 0, "Eight occupied bag-slot fixture rejects a ninth slot atomically.");
                f.E.Bag[7] = new SupplySlot();
                C(f.E.PackSlot(0) && f.E.Bag[7].Id == "water" && f.M.PackedWater == 1, "Eighth bag slot is writable (seven filler slots are explicit capacity fixtures).");
                C(!f.E.PackSlot(-1) && !f.E.PackSlot(12), "Invalid inventory slot refuses safely.");
            }

            void Craft()
            {
                var f = New();
                var recipe = PanicExpedition.MaterialCosts("craftfirstaid", null);
                C(recipe.Count == 2 && recipe[0].Id == "bandage" && recipe[0].Required == 2 && recipe[1].Id == "antiseptic" && recipe[1].Required == 1, "Canonical firstaid recipe is exactly 2 bandage and 1 antiseptic.");
                f.E.Inventory[0] = new SupplySlot("bandage", 1); f.E.Inventory[1] = new SupplySlot("antiseptic", 1);
                string before = Stock(f.E.Inventory); int budget = f.M.Budget;
                C(!f.E.CraftFirstAid() && Stock(f.E.Inventory) == before && f.M.Budget == budget && f.E.Count("firstaid") == 0, "One missing bandage prevents all consumption.");
                C(f.E.CurrentRequirements.Count == 2 && f.E.MissingRequirements.Count == 1 && f.E.MissingRequirements[0].Missing == 1, "Craft shortage payload exposes exact required/owned/missing quantities.");
                f.E.Inventory[2] = new SupplySlot("bandage", 1);
                C(f.E.CraftFirstAid() && f.E.Count("bandage") == 0 && f.E.Count("antiseptic") == 0 && f.E.Count("firstaid") == 1 && f.M.Budget == budget, "Split bandage stacks are consumed exactly; final ingredients yield one kit without money charge.");
                before = Stock(f.E.Inventory);
                C(!f.E.CraftFirstAid() && Stock(f.E.Inventory) == before, "Repeated craft cannot duplicate output after last ingredients.");

                f = New(); Fill(f.E.Inventory); f.E.Inventory[0] = new SupplySlot("bandage", 2); f.E.Inventory[1] = new SupplySlot("antiseptic", 1);
                C(f.E.CraftFirstAid() && f.E.Count("firstaid") == 1 && f.E.Count("bandage") == 0 && f.E.Count("antiseptic") == 0, "Full inventory may craft when consuming final ingredients releases an output slot.");
                f = New(); Fill(f.E.Inventory); f.E.Inventory[0] = new SupplySlot("bandage", 3); f.E.Inventory[1] = new SupplySlot("antiseptic", 2);
                before = Stock(f.E.Inventory);
                C(!f.E.CraftFirstAid() && Stock(f.E.Inventory) == before, "Full inventory cannot consume ingredients when neither recipe stack empties.");
                f.M.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true);
                before = Stock(f.E.Inventory);
                C(!f.E.CraftFirstAid() && Stock(f.E.Inventory) == before, "Free materials still respects output capacity.");
                f.E.Inventory[11] = new SupplySlot(); before = Stock(f.E.Inventory);
                C(f.E.CraftFirstAid() && f.E.Count("firstaid") == 1 && f.E.Count("bandage") == 3 && f.E.Count("antiseptic") == 2, "Free craft produces output without consuming recipe stock.");
            }

            void Requirements()
            {
                var f = New(); f.M.Budget = 59;
                f.E.Inventory[0] = new SupplySlot("bracket", 1); f.E.Inventory[1] = new SupplySlot("screws", 1);
                C(!f.E.CanBeginWork("anchorwall", "bookshelf") && f.E.HasMissingRequirements && f.E.RequirementBudgetCost == 60 && f.E.RequirementBudgetMissing == 1, "Anchor preflight reports exact one-budget shortage.");
                var req = f.E.CurrentRequirements;
                C(req.Count == 2 && req[0].Required == 1 && req[0].Owned == 1 && req[0].Missing == 0 && req[1].Required == 2 && req[1].Owned == 1 && req[1].Missing == 1, "Canonical anchor requirements distinguish owned and missing units.");
                f.M.Budget = 60; f.E.Inventory[1].Count = 2;
                C(!f.E.HasMissingRequirements && f.E.CanBeginWork("anchorwall", "bookshelf") && !f.E.Work.Active && f.M.Budget == 60 && f.E.Count("screws") == 2, "Preflight at exact budget/material boundary is read-only.");
                f.M.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, true); f.M.Budget = 0; Clear(f.E.Inventory);
                C(!f.E.CanBeginWork("anchorwall", "bookshelf") && f.E.RequirementBudgetMissing == 0 && f.E.MissingRequirements.Count == 2, "Infinite budget does not waive material shortages.");
                f.M.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true);
                C(f.E.CanBeginWork("anchorwall", "bookshelf"), "Both independent waivers allow empty-stock zero-budget preflight.");
            }

            void WorkInvalidInput()
            {
                var w = new PanicWorkSession(); w.Begin("anchorwall", "bookshelf");
                float cursor = w.Cursor;
                foreach (float dt in new[] { 0f, -1f, float.NaN, float.PositiveInfinity }) w.Tick(dt, 1, true, true, true);
                C(w.Cursor == cursor && !w.IsApplying && !w.Complete && w.CompletedSteps == 0, "Invalid/nonpositive work deltas never advance stages.");
                w.Tick(.01f, float.NaN, false, false, false);
                w.Tick(.01f, float.PositiveInfinity, false, false, false);
                C(w.Cursor == cursor && !float.IsNaN(w.Progress), "Nonfinite direction is neutralized.");
                w.Begin("unknown", "bookshelf");
                C(!w.Active && !w.Complete && w.TotalSteps == 0, "Unknown work action does not open a session.");
                w.Begin("anchorwall", "bookshelf"); w.Tick(1000f, 1, false, false, false);
                C(Mathf.Abs(w.Cursor - .582f) < .00001f && w.CompletedSteps == 0, "Large work delta is capped at .1s and cannot manufacture completion.");
                w.Cancel();
                C(!w.Active && !w.Complete && w.Action == "" && w.TargetId == "" && w.TotalSteps == 0 && w.Progress == 0, "Cancel resets every publicly observable session identifier/progress.");
            }

            void WorkBoundaries()
            {
                foreach (bool shortWork in new[] { false, true })
                {
                    float tolerance = shortWork ? .19f : .12f;
                    var w = new PanicWorkSession(); w.Begin("anchorwall", "bookshelf", shortWork);
                    w.Target = 0; w.Cursor = tolerance; // Explicit representable float boundary fixture.
                    w.Tick(.001f, 0, false, true, false);
                    C(w.IsApplying && w.TargetWidth == (shortWork ? .19f : .12f), (shortWork ? "Short" : "Normal") + " alignment accepts exact inclusive tolerance (Target=0 fixture).");
                    w.Begin("anchorwall", "bookshelf", shortWork); w.Target = 0; w.Cursor = tolerance + .0001f;
                    w.Tick(.001f, 0, false, true, false);
                    C(!w.IsApplying && w.CompletedSteps == 0, "Alignment just outside tolerance refuses without stage progress.");
                    w.Target = 0; w.Cursor = 0; w.Tick(.001f, 0, false, true, false);
                    int expected = w.ExpectedDirection;
                    for (int i = 0; i < 10; i++) w.Tick(.1f, -expected, true, false, true);
                    C(w.CompletedStrokes == 0 && w.CompletedSteps == 0 && !w.Complete, "Wrong ratchet direction and repeated Space cannot replace required alternating strokes.");
                    w.Tick(.01f, expected, false, false, false);
                    int strokes = w.CompletedStrokes;
                    w.Tick(.01f, expected, false, false, false);
                    C(strokes == 1 && w.CompletedStrokes == 1, "Same ratchet direction cannot count twice across cooldown/alternation guard.");

                    w.Begin("drain", "drain_east", shortWork); w.Target = w.Cursor; w.Tick(.001f, 0, false, true, false);
                    w.Tick(.001f, 0, false, false, true);
                    C(w.Mode == PanicWorkMode.Timing && w.StageProgress == 0 && w.CompletedSteps == 0, "Timing pulse outside band gives no completion.");
                    for (int i = 0; i < 400; i++) w.Tick(.01f, 0, false, false, false);
                    C(w.CompletedSteps == 0 && !w.Complete, "Timing wait without Space never seats a stage.");

                    w.Begin("drain", "drain_east", shortWork); w.Target = w.Cursor; w.Tick(.001f, 0, false, true, false);
                    float nextSweep = .04f + Mathf.PingPong(.01f * .50f, .92f);
                    w.Target = nextSweep - w.TargetWidth;
                    w.Tick(.01f, 0, false, false, true);
                    C(Mathf.Abs(w.Cursor - w.Target) == w.TargetWidth && w.StageProgress == 1 && w.CompletedSteps == 0,
                        "Exact representable inclusive timing-band boundary seats but still requires production settle.");
                    w.Tick(.1f, 0, false, false, false);
                    w.Tick(shortWork ? .059f : .1f, 0, false, false, false);
                    if (!shortWork) w.Tick(.049f, 0, false, false, false);
                    C(w.CompletedSteps == 0, "Timing stage remains pending immediately before settle duration.");
                    w.Tick(.0011f, 0, false, false, false);
                    C(w.CompletedSteps == 1 && !w.Complete, "Crossing .16s short / .25s normal settle advances exactly one stage.");
                    w.Begin("drain", "drain_east", shortWork); w.Target = w.Cursor; w.Tick(.001f, 0, false, true, false);
                    w.Target = nextSweep + w.TargetWidth + .0001f; w.Tick(.01f, 0, false, false, true);
                    C(w.StageProgress == 0 && w.CompletedSteps == 0, "Timing pulse just outside inclusive band cannot seat.");

                    w.Begin("fire", "bookshelf", shortWork); w.Target = w.Cursor; w.Tick(.001f, 0, false, true, false);
                    for (int i = 0; i < 100; i++) w.Tick(.01f, 0, false, false, true);
                    C(w.Mode == PanicWorkMode.Balance && w.StageProgress == 0 && w.CompletedSteps == 0, "Balance requires held Space; pulse alone cannot progress.");
                    for (int i = 0; i < 60; i++) { w.Cursor = 0; w.Target = 1; w.Tick(.01f, -1, true, false, false); }
                    C(w.CompletedSteps == 0 && !w.Complete, "Balance held outside real band cannot complete.");
                    w.Begin("fire", "bookshelf", shortWork); w.Target = w.Cursor; w.Tick(.001f, 0, false, true, false);
                    int heldTicks = 0;
                    for (; heldTicks < 150 && w.StageProgress < 1; heldTicks++)
                        w.Tick(.01f, Mathf.Clamp((w.Target - w.Cursor) / .007f, -1, 1), true, false, false);
                    int minimum = shortWork ? 65 : 95;
                    C(w.StageProgress == 1 && w.CompletedSteps == 0 && heldTicks >= minimum && heldTicks <= minimum + 1,
                        "In-band balance needs the full .65s short / .95s normal held duration (" + heldTicks + " ticks), then settle.");
                }
            }

            void WorkFamily(string action, bool shortWork)
            {
                var f = New(); string target = action == "drain" ? "drain_east" : "bookshelf";
                Costs(f, action, target); f.M.Budget = PanicExpedition.WorkCost(action, ItemOrNull(f, target));
                string before = Stock(f.E.Inventory); int budget = f.M.Budget;
                C(f.E.CanBeginWork(action, target), "Exact-cost preflight accepts " + action + ".");
                f.E.Work.Begin(action, target, shortWork);
                C(f.E.Work.TotalSteps == (shortWork ? 2 : 4) && f.E.Work.RequiredStrokes == (shortWork ? 2 : 3), "Short/normal stages and ratchet strokes match canonical rules.");
                C(!f.E.CanBeginWork(action, target) && Stock(f.E.Inventory) == before && f.M.Budget == budget, "Active work cannot start another session or spend early.");
                bool completed = Drive(f.E.Work, out int ticks);
                C(completed && f.E.Work.CompletedSteps == (shortWork ? 2 : 4) && f.E.Work.Progress == 1, "Production Tick driver completed " + action + " in " + ticks + " bounded ticks; no Complete assignment.");
                C(Stock(f.E.Inventory) == before && f.M.Budget == budget, "Finishing minigame alone does not commit any cost.");
                if (!completed) return;
                C(f.E.TryCompleteWork(action, target) && f.M.Budget == 0 && AllCostsZero(f, action, target), "Completion commits exact final material units and budget once.");
                before = Stock(f.E.Inventory); budget = f.M.Budget;
                C(!f.E.TryCompleteWork(action, target) && f.M.Budget == budget && Stock(f.E.Inventory) == before && !f.E.Work.Active, "Duplicate completion is refused after atomic cancellation.");
            }

            static PanicItem ItemOrNull(Fixture f, string target)
            {
                foreach (var item in f.M.Items) if (item.Id == target) return item;
                return null;
            }

            static bool AllCostsZero(Fixture f, string action, string target)
            {
                // Inputs were exact; inspect all inventory rather than recomputing a now-changed recipe.
                foreach (var slot in f.E.Inventory) if (slot.Count != 0) return false;
                return true;
            }

            /// <summary>Feedback controller using exposed production session state; bounded 120s simulated work.</summary>
            static bool Drive(PanicWorkSession w, out int ticks)
            {
                const float dt = .01f;
                for (ticks = 0; ticks < 12000 && w.Active && !w.Complete; ticks++)
                {
                    if (!w.IsApplying)
                    {
                        float error = w.Target - w.Cursor;
                        float direction = Mathf.Clamp(error / (.82f * dt), -1, 1);
                        w.Tick(dt, direction, false, Mathf.Abs(error) <= .01f, false);
                    }
                    else if (w.StageProgress >= 1)
                        w.Tick(dt, 0, false, false, false); // Let production settle finish.
                    else if (w.Mode == PanicWorkMode.Ratchet)
                        w.Tick(dt, w.ExpectedDirection, false, false, w.ExpectedDirection == 0);
                    else if (w.Mode == PanicWorkMode.Timing)
                        w.Tick(dt, 0, false, false, Mathf.Abs(w.Cursor - w.Target) < w.TargetWidth * .5f);
                    else
                        w.Tick(dt, Mathf.Clamp((w.Target - w.Cursor) / (.70f * dt), -1, 1), true, false, false);
                }
                return w.Complete;
            }

            void AllWorkActions()
            {
                foreach (string action in new[] { "anchorwall", "anchorfloor", "upgrade", "flood", "fire", "plant", "drain", "fence", "hose" })
                {
                    var f = New(); string target = action == "upgrade" ? Structure(f) : action == "flood" ? DoorOrWindow(f) :
                        action == "plant" || action == "drain" || action == "fence" || action == "hose" ? action + "_audit_site" : "bookshelf";
                    Costs(f, action, target); int cost = PanicExpedition.WorkCost(action, ItemOrNull(f, target)); f.M.Budget = cost;
                    bool begin = f.E.CanBeginWork(action, target);
                    f.E.Work.Begin(action, target);
                    bool complete = Drive(f.E.Work, out int ticks);
                    bool commit = complete && f.E.TryCompleteWork(action, target);
                    C(begin && complete && commit && f.M.Budget == 0 && AllCostsZero(f, action, target), action + ": complete production keyboard stages, consume exact recipe and exact money (" + ticks + " ticks).");
                    C(!f.E.TryCompleteWork(action, target) && f.M.Budget == 0, action + ": immediate duplicate cannot charge twice.");
                }
            }

            static string DoorOrWindow(Fixture f)
            {
                foreach (var item in f.M.Items) if (item.Installed && (item.Kind == ItemKind.Door || item.Kind == ItemKind.Window)) return item.Id;
                throw new InvalidOperationException("Installed door/window missing");
            }

            void WorkAtomicity()
            {
                var f = New(); Costs(f, "anchorwall", "bookshelf"); f.M.Budget = 60;
                f.E.Work.Begin("anchorwall", "bookshelf"); f.E.Work.Tick(.01f, 1, false, false, false);
                string before = Stock(f.E.Inventory); f.E.Work.Cancel();
                C(!f.E.TryCompleteWork("anchorwall", "bookshelf") && Stock(f.E.Inventory) == before && f.M.Budget == 60 && !Item(f, "bookshelf").Anchored, "Cancelling mid-stage consumes nothing and cannot later commit.");
                f.E.Work.Begin("anchorwall", "bookshelf"); bool complete = Drive(f.E.Work, out _);
                C(complete, "Completed session prepared through Tick for revalidation fixtures.");
                if (!complete) return;
                f.M.Budget = 59;
                C(!f.E.TryCompleteWork("anchorwall", "bookshelf") && Stock(f.E.Inventory) == before && f.M.Budget == 59 && !Item(f, "bookshelf").Anchored, "Budget removed after begin is revalidated without partial material spend.");
                f.M.Budget = 60; f.E.Inventory[1].Count = 1; before = Stock(f.E.Inventory);
                C(!f.E.TryCompleteWork("anchorwall", "bookshelf") && Stock(f.E.Inventory) == before && f.M.Budget == 60 && !Item(f, "bookshelf").Anchored, "Material removed after begin is revalidated without money spend.");
                C(!f.E.TryCompleteWork("anchorfloor", "bookshelf") && !f.E.TryCompleteWork("anchorwall", "fridge") && f.M.Budget == 60, "Completed session cannot commit another action or target.");
                f.E.Inventory[1].Count = 2;
                f.M.Phase = PanicPhase.Disaster; before = Stock(f.E.Inventory);
                C(!f.E.TryCompleteWork("anchorwall", "bookshelf") && Stock(f.E.Inventory) == before && f.M.Budget == 60, "Phase changed after begin blocks commit atomically.");
                f.M.Phase = PanicPhase.Preparing;
                C(f.E.TryCompleteWork("anchorwall", "bookshelf") && f.M.Budget == 0 && Item(f, "bookshelf").Anchored, "Unchanged completed session may commit once after valid phase/materials restored.");
                C(f.M.ToggleAnchor("bookshelf") && !Item(f, "bookshelf").Anchored && f.E.CanBeginWork("anchorfloor", "bookshelf"), "Owned anchor can be released and reinstalled without fresh stock/budget.");
                f.E.Work.Begin("anchorfloor", "bookshelf"); complete = Drive(f.E.Work, out _);
                C(complete && f.E.TryCompleteWork("anchorfloor", "bookshelf") && f.M.Budget == 0 && Item(f, "bookshelf").AnchorSurface == "floor", "Reinstall still needs genuine work and preserves zero repeat cost.");

                f = New(); Costs(f, "drain", "drain_east"); f.E.Work.Begin("drain", "drain_east"); complete = Drive(f.E.Work, out _);
                if (complete) f.E.TryCompleteWork("drain", "drain_east");
                C(complete && f.M.DrainCount == 1 && !f.E.CanBeginWork("drain", "drain_east"), "Fixed garden site cannot repeat after commit.");
            }

            void PhaseGuards()
            {
                foreach (PanicPhase phase in new[] { PanicPhase.Title, PanicPhase.Disaster, PanicPhase.Aftermath, PanicPhase.Success, PanicPhase.Inspection, PanicPhase.GameOver })
                {
                    var f = New(); Costs(f, "anchorwall", "bookshelf"); Source(f, new SupplySlot("water", 1));
                    f.E.Inventory[2] = new SupplySlot("water", 1); f.E.Inventory[3] = new SupplySlot("bandage", 2); f.E.Inventory[4] = new SupplySlot("antiseptic", 1);
                    f.M.Phase = phase; string before = Stock(f.E.Inventory); int budget = f.M.Budget;
                    C(!f.E.GatherAt("audit-source", 0) && !f.E.PackSlot(2) && !f.E.CraftFirstAid() && !f.E.CanBeginWork("anchorwall", "bookshelf") && !f.E.TryCompleteWork("anchorwall", "bookshelf") && !f.M.BuyWater() && !f.M.MoveItem("bookshelf", Vector3.one, 0) && Stock(f.E.Inventory) == before && f.M.Budget == budget, phase + " blocks preparation mutations atomically.");
                }
                var tutorial = New(false); tutorial.M.StartRun(true); tutorial.E.Inventory[0] = new SupplySlot("water", 1);
                C(tutorial.M.IsTutorial && tutorial.E.PackSlot(0), "Tutorial explicitly permits preparation interactions.");
                float time = tutorial.M.TimeRemaining; tutorial.M.Tick(1000);
                C(tutorial.M.TimeRemaining == time && tutorial.M.Phase == PanicPhase.Tutorial, "Tutorial clock does not run as campaign preparation.");
                tutorial.M.FinishTutorial();
                C(!tutorial.M.IsTutorial && tutorial.M.Phase == PanicPhase.Preparing && tutorial.M.Budget == 1000 && tutorial.M.PackedWater == 0, "Finishing tutorial resets provisional model economy.");
            }

            static void Options(PanicModel model, int mask)
            {
                for (int bit = 0; bit < 4; bit++) model.SetDeveloperOption((PanicDeveloperOption)bit, (mask & (1 << bit)) != 0);
            }

            void DeveloperMask(int mask)
            {
                var f = New(false); Options(f.M, mask);
                C(!f.M.CheatedRun && f.M.DeveloperAnyEnabled == (mask != 0), "Title options configure independent flags; no run has yet been marked.");
                f.M.StartRun(false); bool inf = (mask & 1) != 0, free = (mask & 2) != 0, freeze = (mask & 4) != 0, noLoss = (mask & 8) != 0;
                C(f.M.InfiniteBudget == inf && f.M.FreeMaterials == free && f.M.FreezePreparationTime == freeze && f.M.DeveloperNoLoss == noLoss && f.M.CheatedRun == (mask != 0), "All four developer bits retain their independent requested values on run start.");
                f.M.Budget = 60; Costs(f, "anchorwall", "bookshelf"); string inv = Stock(f.E.Inventory);
                bool begin = f.E.CanBeginWork("anchorwall", "bookshelf"); f.E.Work.Begin("anchorwall", "bookshelf"); bool completed = Drive(f.E.Work, out _);
                C(begin && completed && f.E.TryCompleteWork("anchorwall", "bookshelf") && f.M.Budget == (inf ? 60 : 0) && (free ? Stock(f.E.Inventory) == inv : AllCostsZero(f, "anchorwall", "bookshelf")), "Only budget/material bits affect their respective atomic work costs.");
                f.E.Inventory[4] = new SupplySlot("bandage", 2); f.E.Inventory[5] = new SupplySlot("antiseptic", 1); int budget = f.M.Budget;
                C(f.E.CraftFirstAid() && f.E.Count("firstaid") == 1 && f.E.Count("bandage") == (free ? 2 : 0) && f.E.Count("antiseptic") == (free ? 1 : 0) && f.M.Budget == budget, "Craft respects only FreeMaterials and never charges money.");
                float t = f.M.TimeRemaining; f.M.Tick(1);
                C(Mathf.Abs(f.M.TimeRemaining - (freeze ? t : t - 1)) < .0001f, "Only FreezePreparationTime gates preparation countdown.");
                C(f.M.EnterBunker(), "Developer options do not remove real bunker transition.");
                f.M.Tick(.5f);
                C(f.M.Phase == PanicPhase.Disaster && Mathf.Abs(f.M.PhaseTime - 7.5f) < .0001f, "Disaster time advances even when preparation freeze is enabled.");
                f.M.Tick(7.5f);
                C(f.M.Phase == PanicPhase.Aftermath && f.M.TotalDamage > 0 && !f.M.BagReady, "Real disaster damage and insufficient bag persist for every mask.");
                f.M.Tick(4);
                C(f.M.Phase == (noLoss ? PanicPhase.Inspection : PanicPhase.GameOver) && f.M.CompletedWaves == 0 && f.M.Score == 0 && f.M.LastReward == 0 && f.M.Budget == budget, "Only NoLoss opens unranked inspection; all failed masks earn zero reward.");
                C(f.M.Highscores.Count == (mask == 0 ? 1 : 0) && File.Exists(f.ScorePath) == (mask == 0), "Only clean normal failure may persist a score record in its isolated file.");
                Options(f.M, 0);
                C(f.M.CheatedRun == (mask != 0), "Disabling every option cannot clear sticky cheat history.");
                f.M.ReturnToTitle(); f.M.StartRun(false);
                C(!f.M.CheatedRun && !f.M.DeveloperAnyEnabled && f.M.Budget == 1000 && f.M.PackedWater == 0, "New clean run resets cheat marker and run economy.");
            }

            void DeveloperScores()
            {
                var f = New();
                C(!f.M.AddDeveloperBudget(0) && !f.M.AddDeveloperBudget(-1) && !f.M.CheatedRun, "Invalid budget grant neither spends nor marks cheat.");
                C(f.M.AddDeveloperBudget(1000) && f.M.Budget == 2000 && f.M.CheatedRun, "One +1000 action grants once and latches cheat.");
                f.M.Budget = int.MaxValue - 1;
                C(f.M.AddDeveloperBudget(1000) && f.M.Budget == int.MaxValue, "Developer budget addition saturates safely.");
                f.M.Phase = PanicPhase.Disaster; int b = f.M.Budget;
                C(!f.M.AddDeveloperBudget(1000) && f.M.Budget == b, "Budget action is phase guarded.");

                f = New(); f.M.TimeRemaining = f.M.PhaseTime = .1f; f.M.Tick(.1f);
                C(f.M.Phase == PanicPhase.GameOver && f.M.Highscores.Count == 1 && File.Exists(f.ScorePath), "Clean deadline failure writes exactly one isolated run entry.");
                f.M.Tick(100); C(f.M.Highscores.Count == 1, "Repeated result clock cannot duplicate a run record.");
                f.M.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true);
                C(f.M.CheatedRun && f.M.Highscores.Count == 0 && File.Exists(f.ScorePath), "Enabling developer option after recorded result removes that run entry atomically.");
                var reload = new PanicModel();
                C(reload.Highscores.Count == 0, "Removed cheated result stays absent after score reload.");
                f.M.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, false); f.M.StartRun(false); f.M.TimeRemaining = f.M.PhaseTime = .1f; f.M.Tick(.1f);
                C(!f.M.CheatedRun && f.M.Highscores.Count == 1 && new PanicModel().Highscores.Count == 1, "Clean retry can persist a new clean record after prior cheat removal.");

                f = New(); f.M.SetDeveloperOption(PanicDeveloperOption.NoLoss, true); f.M.SetDeveloperOption(PanicDeveloperOption.NoLoss, false);
                C(f.M.DeveloperNoLossUsedThisWave && f.M.CheatedRun, "NoLoss wave usage remains latched after toggling off.");
                // Use a completely protected explicit household fixture to guarantee a success branch.
                foreach (var item in f.M.Items) { item.Anchored = true; item.AnchorSurface = "floor"; item.Tier = 2; }
                f.M.PackedWater = f.M.PackedFood = 1; f.M.EnterBunker(); f.M.Tick(12);
                C(f.M.TotalDamage <= 65 && f.M.Phase == PanicPhase.Inspection && f.M.CompletedWaves == 0 && f.M.LastReward == 0 && f.M.Score == 0, "Even a successful real resolution gives no reward after NoLoss was used then disabled.");
            }

            void DeadlineRetry()
            {
                var f = New(); f.M.TimeRemaining = f.M.PhaseTime = 1;
                f.M.Tick(.999f); C(f.M.Phase == PanicPhase.Preparing && f.M.TimeRemaining > 0, "Just before deadline remains preparation.");
                f.M.Tick(.0011f); C(f.M.Phase == PanicPhase.GameOver && f.M.TimeRemaining == 0 && !string.IsNullOrEmpty(f.M.FailureReason), "Crossing preparation deadline produces explicit loss.");
                f.M.StartRun(false); f.E.Reset();
                C(f.M.Phase == PanicPhase.Preparing && f.M.Budget == 1000 && f.M.Score == 0 && f.M.CompletedWaves == 0 && f.M.TotalDamage == 0 && f.E.Inventory.Count == 12 && f.E.Count("water") == 0 && !f.E.Work.Active, "Retry resets model plus explicit expedition lifecycle state.");
                f.M.Budget = 123; f.M.StartRun(false);
                C(f.M.Budget == 123 && f.M.Phase == PanicPhase.Preparing, "StartRun cannot reset an active run.");
                foreach (float invalid in new[] { 0f, -1f, float.NaN, float.PositiveInfinity }) f.M.Tick(invalid);
                C(f.M.Budget == 123 && f.M.Phase == PanicPhase.Preparing && !float.IsNaN(f.M.TimeRemaining), "Invalid model Tick deltas leave active run finite and intact.");

                f = New(); f.M.SetDeveloperOption(PanicDeveloperOption.NoLoss, true); f.M.PackedWater = f.M.PackedFood = 1;
                f.M.TimeRemaining = f.M.PhaseTime = .1f; f.M.Tick(.1f);
                C(f.M.Phase == PanicPhase.Disaster && f.M.PhaseTime == 8 && f.M.PackedWater == 1, "NoLoss timeout enters actual timed disaster without manufacturing supplies or skipping damage.");
                f.M.Tick(12);
                C(f.M.Phase == PanicPhase.Inspection && f.M.PackedWater == 0 && f.M.PackedFood == 0 && f.M.TotalDamage > 0 && f.M.Score == 0 && f.M.LastReward == 0, "NoLoss deadline still consumes real packed rations and resolves real damage, unrewarded.");
            }

            void DamageThreshold()
            {
                var f = New();
                var failure = typeof(PanicModel).GetMethod("BuildFailureReason", BindingFlags.NonPublic | BindingFlags.Instance);
                C(failure != null, "Private production failure-reason predicate located for isolated exact-threshold fixture.");
                if (failure == null) return;
                f.M.TotalDamage = 65;
                string at = (string)failure.Invoke(f.M, new object[] { true, 1, 1 });
                f.M.TotalDamage = 65.0001f;
                string above = (string)failure.Invoke(f.M, new object[] { true, 1, 1 });
                C(at == "" && !string.IsNullOrEmpty(above), "Exact 65 is allowed; just above 65 is loss in production failure predicate. This fixture does not claim natural exact damage generation.");
                f.M.TotalDamage = 0;
                string bag = (string)failure.Invoke(f.M, new object[] { false, 0, 1 });
                C(bag.Contains("1 günlük su") && bag.Contains("0 günlük gıda"), "Insufficient bag reason exposes exact missing daily units independently of damage.");
            }

            void ProductionDisaster()
            {
                var f = New();
                foreach (var item in f.M.Items) { item.Anchored = true; item.AnchorSurface = "floor"; item.Tier = 2; }
                f.M.PackedWater = f.M.PackedFood = 1; int initial = f.M.Budget;
                C(f.M.EnterBunker(), "Prepared household enters production disaster.");
                f.M.Tick(8);
                C(f.M.Phase == PanicPhase.Aftermath && f.M.TotalDamage > 0 && f.M.TotalDamage <= 65 && f.M.PackedWater == 0 && f.M.PackedFood == 0 && f.M.Budget == initial, "Actual damage calculation and ration consumption occur before reward.");
                f.M.Tick(4);
                C(f.M.Phase == PanicPhase.Success && f.M.CompletedWaves == 1 && f.M.LastReward > 0 && f.M.Score > 0, "Valid production result awards one completed wave.");
                int awardedBudget = f.M.Budget; long score = f.M.Score;
                f.M.Tick(3);
                C(f.M.Phase == PanicPhase.Inspection && f.M.CompletedWaves == 1 && f.M.Budget == awardedBudget && f.M.Score == score, "Success-to-inspection cannot duplicate reward.");
                f.M.FinishInspection();
                C(f.M.Phase == PanicPhase.Preparing && f.M.Wave == 2 && f.M.TotalDamage == 0 && Item(f, "bookshelf").Anchored && f.M.Budget == awardedBudget, "Next wave clears damage and retains actual installed preparation.");

                f = New(); f.M.Wave = 200; f.M.PackedWater = f.M.PackedFood = 1; f.M.EnterBunker(); f.M.Tick(8);
                C(f.M.TotalDamage > 65 && !string.IsNullOrEmpty(f.M.FailureReason), "Explicit late-wave exposed-house fixture exercises actual calculated damage above loss limit.");
                f.M.Tick(4);
                C(f.M.Phase == PanicPhase.GameOver && f.M.CompletedWaves == 0 && f.M.LastReward == 0, "Actual damage above65 follows failure transition without award.");
            }

            void JuryDeadline()
            {
                var f = New(false); var jury = new PanicJuryDemo(f.M, f.E);
                C(jury.Start(), "Isolated short flow starts with its finite production fixture.");
                C(f.M.PackedFood == 1 && f.M.PackedWater == 0 && f.E.Count("screws") == 2 && f.E.Count("bracket") == 1, "Prepared food is baseline; required water is still genuine source pickup.");
                f.M.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, true);
                jury.Tick(20, PanicJuryDemo.SpawnPosition, true, true);
                C(jury.Stage == PanicJuryStage.Preparation && Mathf.Abs(jury.ElapsedSeconds - 20) < .001f, "Short flow uses unscaled clock despite input block and preparation-freeze option.");
                C(f.E.CanBeginWork("anchorwall", "bookshelf"), "Short preparation enables only scoped real anchor work.");
                f.E.Work.Begin("anchorwall", "bookshelf", true); f.E.Work.Tick(.01f, 1, false, false, false);
                string stock = Stock(f.E.Inventory); int budget = f.M.Budget;
                jury.Tick(80, PanicJuryDemo.SpawnPosition, true, true);
                C(jury.Stage == PanicJuryStage.Earthquake && !f.E.Work.Active && Stock(f.E.Inventory) == stock && f.M.Budget == budget && !Item(f, "bookshelf").Anchored, "Preparation deadline cancels unfinished work without cost/automatic anchoring.");
                C(jury.Outcomes.Count == 1 && !jury.Outcomes[0].PreparationComplete && !jury.Outcomes[0].ShelterReached, "Deadline outcome truthfully retains missing preparation and shelter.");
                int outcomes = jury.Outcomes.Count; float damage = f.M.TotalDamage;
                C(jury.SkipCinematic() && !jury.SkipCinematic() && jury.Outcomes.Count == outcomes && f.M.TotalDamage == damage, "Skip cinematic advances once; disaster result is not recalculated.");
                jury.Tick(1000, PanicJuryDemo.SpawnPosition, true, true);
                C(jury.Stage == PanicJuryStage.Complete && !jury.IsRunning && jury.ElapsedSeconds <= 270.001f && jury.Outcomes.Count == 3, "Large blocked-frame deadline closes all three truthful vignettes within content bound.");
                C(f.M.Score == 0 && f.M.LastReward == 0 && f.M.CompletedWaves == 0 && f.M.Highscores.Count == 0 && !File.Exists(f.ScorePath), "Short flow never awards campaign money/score or persists scoreboard.");
                foreach (var goal in jury.Snapshot.Goals) C(goal.State != PanicJuryGoalState.Complete, "Blocked/no-input deadline never fabricates completed objective: " + goal.Id);
            }

            void JurySkipRetry()
            {
                var f = New(false); var jury = new PanicJuryDemo(f.M, f.E);
                C(!jury.Start(new Vector3(float.NaN, 0, 0)) && f.M.Phase == PanicPhase.Title, "Nonfinite start position is refused before model mutation.");
                C(jury.Start() && !jury.Start(), "Short flow cannot restart while active.");
                jury.Tick(20, PanicJuryDemo.SpawnPosition, true, false);
                f.E.Work.Begin("anchorwall", "bookshelf", true); bool complete = Drive(f.E.Work, out _); int budget = f.M.Budget; string stock = Stock(f.E.Inventory);
                C(complete && jury.SkipCurrentSegment() && !f.E.TryCompleteWork("anchorwall", "bookshelf") && f.M.Budget == budget && Stock(f.E.Inventory) == stock, "Completed but uncommitted work cannot commit after skipped segment revokes scope.");
                C(jury.SkipCinematic() && jury.Advance() && jury.Stage == PanicJuryStage.Flood, "Cinematic skip then explicit result advance reaches flood once.");
                C(!f.E.CanBeginWork("anchorwall", "bookshelf") && f.E.CanBeginWork("drain", "drain_east"), "Flood scope rejects stale earthquake work and accepts actual drainage.");
                jury.Stop();
                C(f.M.Phase == PanicPhase.Title && !f.M.IsJuryDemo && !f.E.Work.Active && jury.Stage == PanicJuryStage.Inactive, "Stop restores clean menu/model/expedition lifecycle.");
                C(jury.Start() && f.M.PackedFood == 1 && f.M.PackedWater == 0 && f.E.Count("bracket") == 1 && jury.Outcomes.Count == 0, "Fresh retry restores finite initial fixture; old outcomes/work do not leak.");
                C(!jury.TryReachShelter(PanicJuryShelter.QuakeCover, PanicJuryDemo.QuakeCoverPosition, true), "Shelter cannot bypass stage/objectives even with exact target-position fixture.");
                jury.Stop();
            }
        }
    }
}
