using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Opt-in isolated native fixture audit. Root alone integrates/runs this file.
    /// Uses normal StartGame(false), production E/F dispatch, real CharacterController
    /// settlement and ordinary costs. Setup Warps are explicit; this is not a route,
    /// OS-input, IMGUI-click or human-play acceptance test.
    /// </summary>
    public static class PanicInteractionTargetAudit
    {
        const float MaxWallSeconds = 39.5f;
        const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        static readonly string[] SourceIds = { "fridge", "cabinet", "bookshelf", "up_cabinet", "up_bookshelf", "bath_cabinet", "garage_tools", "basement_pantry" };
        static readonly string[] GardenIds = { "plant_west1", "plant_west2", "plant_east", "drain_west", "drain_east", "fence_west", "fence_east", "fence_north", "hose_station" };
        static readonly string[] DecorIds = { "@fireplace", "@radio", "@computer", "@garage", "@laundry", "@toilet", "@sink" };
        // Try ordinary cardinal and diagonal approach points, never the target centre.
        static readonly Vector2[] Directions = {
            Vector2.down, Vector2.left, Vector2.right, Vector2.up,
            new Vector2(-1,-1).normalized, new Vector2(1,-1).normalized,
            new Vector2(-1,1).normalized, new Vector2(1,1).normalized
        };
        static readonly float[] Radii = { .9f, 1.25f, 1.6f };

        sealed class Setup
        {
            public bool Established;
            public int Attempts, Warps;
            public string Detail;
        }

        public static IEnumerator Run(PanicHomeWorld w, PanicHomeHud hud, Action<string, bool, string> check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            if (!w || !hud || w.Model == null || w.Expedition == null)
            { check("target.setup.dependencies", false, "FIXTURE_CANNOT_ESTABLISH: world/HUD/model/expedition missing"); yield break; }

            bool priorScripted = w.FullFlowScriptedInput;
            bool priorLock = w.InputLocked;
            bool[] priorOptions = { w.Model.InfiniteBudget, w.Model.FreeMaterials, w.Model.FreezePreparationTime, w.Model.DeveloperNoLoss };
            float began = Time.realtimeSinceStartup;
            float deadline = began + MaxWallSeconds;
            int testedTargets = 0, fixtureFailures = 0, totalWarps = 0;

            try
            {
                // Restore options only after returning to Title in finally. Disabling
                // them uses the one legal model mutation path and never grants stock.
                hud.SetDeveloperMenuOpen(false);
                SetPauseOff(hud);
                w.ReturnToMenu();
                for (int i = 0; i < 4; i++) w.Model.SetDeveloperOption((PanicDeveloperOption)i, false);
                w.FullFlowScriptedInput = true;
                w.InputLocked = false;
                yield return null;

                foreach (string id in SourceIds)
                {
                    Fresh(w, hud);
                    yield return null;
                    LootSource source = w.Expedition.Sources.Find(s => s.Id == id);
                    if (source == null)
                    { check("target.source." + id + ".fixture", false, "FIXTURE_CANNOT_ESTABLISH: current source ID absent"); fixtureFailures++; testedTargets++; continue; }
                    var setup = new Setup();
                    yield return Establish(w, id, source.Position, deadline, setup);
                    totalWarps += setup.Warps;
                    check("target.source." + id + ".fixture", setup.Established, setup.Detail);
                    if (!setup.Established) { fixtureFailures++; testedTargets++; continue; }

                    string inventoryBefore = Slots(w.Expedition.Inventory);
                    string stocksBefore = Slots(source.Contents);
                    int budgetBefore = w.Model.Budget;
                    w.HandleKey(KeyCode.E);
                    yield return null;
                    check("target.source." + id + ".E.open.no_cost", source.Open && inventoryBefore == Slots(w.Expedition.Inventory) && stocksBefore == Slots(source.Contents) && w.Model.Budget == budgetBefore && !w.WorkActive,
                        "semantic E; source.Open=" + source.Open + "; inventory/source/budget unchanged; " + Mode(w));

                    int selected = w.LootIndex;
                    string gatheredId = selected >= 0 && selected < source.Contents.Count ? source.Contents[selected].Id : string.Empty;
                    int gatheredCount = selected >= 0 && selected < source.Contents.Count ? source.Contents[selected].Count : 0;
                    int stacksBefore = Nonempty(source.Contents);
                    int allSourceUnitsBefore = Units(source.Contents);
                    int allInventoryUnitsBefore = Units(w.Expedition.Inventory);
                    int sameItemBefore = w.Expedition.Count(gatheredId);
                    w.HandleKey(KeyCode.F);
                    yield return null;
                    bool oneStack = !string.IsNullOrEmpty(gatheredId) && gatheredCount > 0 &&
                        Nonempty(source.Contents) == stacksBefore - 1 &&
                        Units(source.Contents) == allSourceUnitsBefore - gatheredCount &&
                        Units(w.Expedition.Inventory) == allInventoryUnitsBefore + gatheredCount &&
                        w.Expedition.Count(gatheredId) == sameItemBefore + gatheredCount &&
                        w.GatherCount == gatheredCount && w.GatherItemId == gatheredId &&
                        w.Model.Budget == budgetBefore && !w.Model.CheatedRun;
                    check("target.source." + id + ".F.exactly_one_stack", oneStack,
                        "semantic F; index=" + selected + "; id=" + gatheredId + "; wholeStack=" + gatheredCount + "; nonempty=" + stacksBefore + "->" + Nonempty(source.Contents) + "; " + Mode(w));

                    string inventoryAfterF = Slots(w.Expedition.Inventory);
                    string stocksAfterF = Slots(source.Contents);
                    w.HandleKey(KeyCode.E);
                    yield return null;
                    check("target.source." + id + ".E.close.no_extra_gather", !source.Open && inventoryAfterF == Slots(w.Expedition.Inventory) && stocksAfterF == Slots(source.Contents) && w.Model.Budget == budgetBefore,
                        "semantic E close; source/inventory/budget unchanged after the single F");
                    testedTargets++;
                }

                foreach (string id in GardenIds)
                {
                    Fresh(w, hud);
                    yield return null;
                    Vector3 position; string action;
                    if (!Garden(w, id, out position, out action))
                    { check("target.garden." + id + ".fixture", false, "FIXTURE_CANNOT_ESTABLISH: current garden ID/position/action absent"); fixtureFailures++; testedTargets++; continue; }
                    var setup = new Setup();
                    yield return Establish(w, id, position, deadline, setup);
                    totalWarps += setup.Warps;
                    check("target.garden." + id + ".fixture", setup.Established, setup.Detail);
                    if (!setup.Established) { fixtureFailures++; testedTargets++; continue; }

                    // Explicit legal inventory fixture, matching the authoritative recipe.
                    // No FreeMaterials/InfiniteBudget, target-state or work completion writes.
                    bool materialFixture = GiveExactRecipe(w, action);
                    check("target.garden." + id + ".legal_material_fixture", materialFixture && !w.Model.DeveloperAnyEnabled && !w.Model.CheatedRun,
                        "SETUP_FIXTURE: exact MaterialCosts recipe inserted into ordinary empty inventory slots; budget=" + w.Model.Budget + "; action=" + action);
                    if (!materialFixture) { fixtureFailures++; testedTargets++; continue; }
                    string inventoryBefore = Slots(w.Expedition.Inventory);
                    int budgetBefore = w.Model.Budget;
                    int[] gardenBefore = GardenCounts(w);
                    w.HandleKey(KeyCode.E);
                    yield return null;
                    var expected = action == "hose" ? PanicWorkMode.Balance : action == "fence" ? PanicWorkMode.Ratchet : PanicWorkMode.Timing;
                    bool beganCorrectly = w.WorkActive && w.Expedition.Work.Action == action && w.Expedition.Work.TargetId == id && w.Expedition.Work.Mode == expected && inventoryBefore == Slots(w.Expedition.Inventory) && w.Model.Budget == budgetBefore;
                    check("target.garden." + id + ".E.correct_work_family", beganCorrectly,
                        "semantic E; expected=" + action + "/" + expected + "; actual=" + w.Expedition.Work.Action + "/" + w.Expedition.Work.Mode + "; begin has zero cost");
                    w.CancelWork();
                    yield return null;
                    check("target.garden." + id + ".cancel.zero_cost", !w.WorkActive && inventoryBefore == Slots(w.Expedition.Inventory) && w.Model.Budget == budgetBefore && Equal(gardenBefore, GardenCounts(w)) && !w.Model.CheatedRun,
                        "production CancelWork; budget/materials/TreeCount/DrainCount/FenceCount/FireToolCount unchanged; " + Mode(w));
                    testedTargets++;
                }

                foreach (string id in DecorIds)
                {
                    Fresh(w, hud);
                    yield return null;
                    GameObject target = Decor(w, id);
                    if (!target)
                    { check("target.decor." + id + ".fixture", false, "FIXTURE_CANNOT_ESTABLISH: current residenceTargets object absent"); fixtureFailures++; testedTargets++; continue; }
                    Vector3 position = target.transform.position;
                    position.y = Mathf.Round(position.y / 3.2f) * 3.2f;
                    var setup = new Setup();
                    yield return Establish(w, id, position, deadline, setup);
                    totalWarps += setup.Warps;
                    check("target.decor." + id + ".fixture", setup.Established, setup.Detail);
                    if (!setup.Established) { fixtureFailures++; testedTargets++; continue; }

                    string inventoryBefore = Slots(w.Expedition.Inventory);
                    int budgetBefore = w.Model.Budget;
                    bool prior = false;
                    string stateField = id == "@fireplace" ? "hearthEnabled" : id == "@computer" ? "computerEnabled" : id == "@garage" ? "garageOpen" : null;
                    object hinge = null;
                    bool hasState = stateField != null ? ReadBool(w, stateField, out prior) : id == "@radio" || TryHinge(w, id, out hinge) && ReadBool(hinge, "open", out prior);
                    check("target.decor." + id + ".bound_action", hasState,
                        hasState ? "current authored object has production state/message action" : "DECOR_NO_BOUND_ACTION: established reachable target has no expected production field/hinge; investigate source/art binding separately");
                    w.HandleKey(KeyCode.E);
                    yield return null;
                    bool current = false;
                    bool changed;
                    if (id == "@radio")
                        changed = w.NotificationVisible && w.ShortNotice == "Radyo daha sonra" && !w.RadioEnabled && !w.RadioSpeaking;
                    else if (stateField != null)
                        changed = ReadBool(w, stateField, out current) && current != prior;
                    else
                        changed = hinge != null && ReadBool(hinge, "open", out current) && current != prior;
                    check("target.decor." + id + ".E.expected_state_or_notice", hasState && changed && inventoryBefore == Slots(w.Expedition.Inventory) && w.Model.Budget == budgetBefore && !w.Model.CheatedRun,
                        "semantic E; state=" + prior + "->" + current + "; notice=" + (w.ShortNotice ?? "") + "; zero budget/material cost; " + Mode(w));
                    if (id != "@radio" && hasState)
                    {
                        w.HandleKey(KeyCode.E);
                        yield return null;
                        bool returned = stateField != null ? ReadBool(w, stateField, out current) : ReadBool(hinge, "open", out current);
                        check("target.decor." + id + ".E.reverse", returned && current == prior && inventoryBefore == Slots(w.Expedition.Inventory) && w.Model.Budget == budgetBefore,
                            "second semantic E restores boolean target state; does not assert complete animation/visual acceptance");
                    }
                    testedTargets++;
                }

                check("target.audit.summary", testedTargets == 24 && fixtureFailures == 0 && !w.Model.CheatedRun && !w.Model.DeveloperAnyEnabled,
                    "targets=" + testedTargets + "/24; fixtureFailures=" + fixtureFailures + "; setupWarps=" + totalWarps + "; seconds=" + (Time.realtimeSinceStartup - began).ToString("F2") + "; evidence=normal-preparation native target API/physics fixtures; NOT continuous route/OS/IMGUI/human acceptance");
            }
            finally
            {
                // Do not leave a work/placement/cinematic or altered options in the caller.
                hud.SetDeveloperMenuOpen(false);
                SetPauseOff(hud);
                w.ReturnToMenu();
                for (int i = 0; i < 4; i++) w.Model.SetDeveloperOption((PanicDeveloperOption)i, priorOptions[i]);
                w.FullFlowScriptedInput = priorScripted;
                w.InputLocked = priorLock;
            }
        }

        static void Fresh(PanicHomeWorld w, PanicHomeHud hud)
        {
            hud.SetDeveloperMenuOpen(false); SetPauseOff(hud);
            w.StartGame(false); w.FullFlowScriptedInput = true; w.InputLocked = false;
        }

        static IEnumerator Establish(PanicHomeWorld w, string id, Vector3 target, float deadline, Setup setup)
        {
            float localDeadline = Mathf.Min(deadline, Time.realtimeSinceStartup + 1.35f);
            string last = "no candidate attempted";
            foreach (float radius in Radii)
            foreach (Vector2 direction in Directions)
            {
                if (Time.realtimeSinceStartup >= localDeadline)
                { setup.Detail = "FIXTURE_CANNOT_ESTABLISH: bounded approach search expired; target=" + id + "; attempts=" + setup.Attempts + "; " + last; yield break; }
                Vector3 candidate = target + new Vector3(direction.x * radius, .045f, direction.y * radius);
                w.Warp(candidate); setup.Warps++; setup.Attempts++;
                Physics.SyncTransforms();
                // One frame observes ActiveFloor after Warp. Actual neutral controller moves
                // settle gravity against the ordinary floor; no synthetic Model.Tick is used.
                yield return null;
                w.SmokeMove(Vector2.zero, FrameDt());
                // High-FPS hidden players may render two frames in <.01s. Give
                // gravity a real bounded .025s interval instead of repeating Warps
                // before the capsule has had time to reach the ordinary floor.
                float settleBegan = Time.realtimeSinceStartup;
                yield return new WaitForSecondsRealtime(.025f);
                w.SmokeMove(Vector2.zero, Mathf.Clamp(Time.realtimeSinceStartup - settleBegan, .001f, .08f));
                Physics.SyncTransforms();
                w.SetFocus(id);
                bool path = PathClear(w, id, target);
                bool clear = ActorClear(w);
                bool ground = w.IsGrounded;
                bool sameFloor = Mathf.Abs(w.PlayerPosition.y - target.y) < .35f;
                last = "candidate=" + candidate.ToString("F3") + "; actual=" + w.PlayerPosition.ToString("F3") + "; floor=" + w.ActiveFloor + "; grounded=" + ground + "; sameFloor=" + sameFloor + "; actorClear=" + clear + "; CanInteract=" + w.CanInteract + "; path=" + path;
                if (w.CanInteract && path && clear && ground && sameFloor && w.PlayerControllerEnabled)
                {
                    setup.Established = true;
                    setup.Detail = "SETUP_FIXTURE: target=" + id + "; attempts=" + setup.Attempts + "; setupWarps=" + setup.Warps + "; radius=" + radius + "; " + last;
                    yield break;
                }
            }
            setup.Detail = "FIXTURE_CANNOT_ESTABLISH: all 8 directions x3 radii rejected; target=" + id + "; attempts=" + setup.Attempts + "; " + last;
        }

        static float FrameDt() { return Mathf.Clamp(Time.unscaledDeltaTime, .001f, .08f); }
        static string Mode(PanicHomeWorld w) { return "phase=" + w.Model.Phase + "; tutorial=" + w.Model.IsTutorial + "; simulation=" + w.Model.IsSimulation + "; short=" + w.Model.IsJuryDemo + "; cheated=" + w.Model.CheatedRun; }
        static FieldInfo Field(object obj, string name) { return obj == null ? null : obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); }
        static object Value(object obj, string name) { var field = Field(obj, name); return field == null ? null : field.GetValue(obj); }
        static bool ReadBool(object obj, string name, out bool value) { object raw = Value(obj, name); value = raw is bool && (bool)raw; return raw is bool; }
        static void SetPauseOff(PanicHomeHud hud) { var method = typeof(PanicHomeHud).GetMethod("SetPaused", PrivateInstance); if (method != null) method.Invoke(hud, new object[] { false }); }

        static bool PathClear(PanicHomeWorld w, string id, Vector3 p)
        {
            var method = typeof(PanicHomeWorld).GetMethod("InteractionPathClear", PrivateInstance);
            return method != null && (bool)method.Invoke(w, new object[] { id, p });
        }

        static bool ActorClear(PanicHomeWorld w)
        {
            var body = Value(w, "controller") as CharacterController;
            if (!body || !body.enabled) return false;
            Vector3 scale = body.transform.lossyScale;
            float radius = body.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float height = body.height * Mathf.Abs(scale.y);
            Vector3 centre = body.transform.TransformPoint(body.center);
            float segment = Mathf.Max(0, height * .5f - radius);
            // A small inset excludes ordinary floor/skin contact, while refusing
            // fixture Warps inside a furniture or wall collision volume.
            foreach (var c in Physics.OverlapCapsule(centre + Vector3.up * segment, centre - Vector3.up * segment, Mathf.Max(.01f, radius - .035f), ~(1 << 30), QueryTriggerInteraction.Ignore))
            {
                if (!c || c == body || c.transform.IsChildOf(body.transform) || c.GetComponentInParent<ResidenceLootPick>()) continue;
                return false;
            }
            return true;
        }

        static GameObject Decor(PanicHomeWorld w, string id)
        {
            var targets = Value(w, "residenceTargets") as IDictionary;
            return targets != null && targets.Contains(id) ? targets[id] as GameObject : null;
        }
        static bool TryHinge(PanicHomeWorld w, string id, out object hinge)
        {
            var hinges = Value(w, "hinges") as IDictionary;
            hinge = hinges != null && hinges.Contains(id) ? hinges[id] : null;
            return hinge != null;
        }
        static bool Garden(PanicHomeWorld w, string id, out Vector3 p, out string action)
        {
            p = Vector3.zero; action = null;
            var sites = Value(w, "gardenSites") as IEnumerable;
            if (sites == null) return false;
            foreach (var site in sites)
            {
                if (!string.Equals(Value(site, "id") as string, id, StringComparison.Ordinal)) continue;
                object rawPosition = Value(site, "p"); action = Value(site, "action") as string;
                if (!(rawPosition is Vector3) || string.IsNullOrEmpty(action)) return false;
                p = (Vector3)rawPosition; return true;
            }
            return false;
        }
        static bool GiveExactRecipe(PanicHomeWorld w, string action)
        {
            var costs = PanicExpedition.MaterialCosts(action, null);
            foreach (var cost in costs)
            {
                if (cost.Required <= 0 || cost.Required > 5) return false;
                int index = w.Expedition.Inventory.FindIndex(s => s != null && (s.Count <= 0 || string.IsNullOrEmpty(s.Id)));
                if (index < 0) return false;
                w.Expedition.Inventory[index].Id = cost.Id;
                w.Expedition.Inventory[index].Count = cost.Required;
            }
            return true;
        }
        static string Slots(List<SupplySlot> slots)
        {
            var s = new StringBuilder();
            foreach (var slot in slots) s.Append(slot == null ? "null" : (slot.Id ?? "") + ":" + slot.Count).Append('|');
            return s.ToString();
        }
        static int Nonempty(List<SupplySlot> slots) { int n = 0; foreach (var s in slots) if (s != null && s.Count > 0 && !string.IsNullOrEmpty(s.Id)) n++; return n; }
        static int Units(List<SupplySlot> slots) { int n = 0; foreach (var s in slots) if (s != null && s.Count > 0 && !string.IsNullOrEmpty(s.Id)) n += s.Count; return n; }
        static int[] GardenCounts(PanicHomeWorld w) { return new[] { w.Model.TreeCount, w.Model.DrainCount, w.Model.FenceCount, w.Model.FireToolCount }; }
        static bool Equal(int[] a, int[] b) { for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false; return true; }
    }
}
