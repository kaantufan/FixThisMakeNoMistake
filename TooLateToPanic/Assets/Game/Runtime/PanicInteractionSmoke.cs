using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    // Root attaches only after Awake assigns the isolated score path and constructs Model.
    public sealed class PanicInteractionSmoke : MonoBehaviour
    {
        [Serializable] sealed class Check { public string name, detail; public bool passed; }
        [Serializable] sealed class DoorProbe
        {
            public string id, side; public bool openedAway, blocked, leafStopped, logicalClosePending, noPush, resumed;
            public Vector3 setup, arcPosition, afterHold, clearTarget, afterClear;
            public float expectedAngle, openAngle, blockedAngle, angleRange, maxActorDrift, maxPenetration;
            public int blockedFrames; public List<Vector3> stationarySamples = new List<Vector3>();
            public List<float> stationaryAngles = new List<float>();
        }
        [Serializable] sealed class Walk
        {
            public string id; public Vector3 start, target, end; public bool reached;
            public float seconds; public int frames;
        }
        [Serializable] sealed class Requirement
        {
            public string id; public int required, owned, missing;
        }
        [Serializable] sealed class Report
        {
            public string startedUtc, finishedUtc, profileId, scorePath;
            public string evidenceType = "Focused native API input: actual E/F semantics, production CharacterController movement, rendered leaf inspection, independent exact-size capsule-versus-leaf penetration query, production pointer screen-ray/confirm APIs, canonical material payload and real HUD screenshots.";
            public string limitations = "Automatic semantic input, not physical OS mouse/keyboard or manual acceptance. One counted setup warp per door; no warp between a blocked close and walking clear. Other independent fixtures are counted separately. InputLocked prevents unscripted input/model clock advancement as in prior smoke; no timer/cost/collision/controller rule changes. V4.2 stair route acceptance is carried, with permanent rail/controller invariants inspected only. No campaign/FPS/save compatibility acceptance.";
            public bool completed, passed; public float runtimeSeconds;
            public int doorSetupWarps, otherSetupWarps;
            public List<Check> checks = new List<Check>();
            public List<DoorProbe> doors = new List<DoorProbe>();
            public List<Walk> walks = new List<Walk>();
            public List<Requirement> requirementsBefore = new List<Requirement>(), requirementsAfter = new List<Requirement>();
            public List<string> screenshots = new List<string>(), errors = new List<string>();
        }
        sealed class Hinge
        {
            public Transform leaf, frame; public Quaternion closed, rotation;
            public bool open, blocked; public float angle;
        }
        const int ExpectedChecks = 11;
        const float HardCap = 85;
        static float Dt { get { return Mathf.Clamp(Time.unscaledDeltaTime, .008f, .04f); } }
        static readonly BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        readonly Report report = new Report();
        PanicHomeWorld w; PanicHomeHud hud; CharacterController body; CapsuleCollider actorQuery; FieldInfo hinges;
        Coroutine running; float started; bool finished; string watchedDoor; float walkPenetration;

        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--interaction-smoke") < 0) { enabled = false; return; }
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>();
            if (!w || string.IsNullOrEmpty(w.EvidencePath)) { Debug.LogError("INTERACTION_SMOKE requires --home-evidence before Model construction."); enabled = false; return; }
            started = Time.realtimeSinceStartup; report.startedUtc = DateTime.UtcNow.ToString("o");
            report.profileId = Path.GetFileName(Path.GetFullPath(w.EvidencePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            report.scorePath = PanicModel.ScorePathOverride;
            Application.logMessageReceived += Log; Application.runInBackground = true; w.InputLocked = true;
            hinges = typeof(PanicHomeWorld).GetField("hinges", PrivateInstance);
            body = FindObjectsByType<CharacterController>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Survivor");
            if (body)
            {
                var query = new GameObject("Smoke independent actor capsule query"); query.layer = 2;
                query.transform.position = new Vector3(500, 500, 500); query.transform.localScale = body.transform.lossyScale;
                actorQuery = query.AddComponent<CapsuleCollider>(); actorQuery.direction = 1;
                actorQuery.center = body.center; actorQuery.radius = body.radius; actorQuery.height = body.height;
                actorQuery.isTrigger = true; actorQuery.enabled = true; Physics.SyncTransforms();
            }
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
            Guid guid; string name = report.profileId;
            bool isolated = name.Length >= 36 && Guid.TryParse(name.Substring(name.Length - 36), out guid) &&
                !string.IsNullOrEmpty(report.scorePath) && string.Equals(expected, Path.GetFullPath(report.scorePath), StringComparison.OrdinalIgnoreCase) &&
                !File.Exists(expected) && !File.Exists(Path.Combine(w.EvidencePath, "native-interaction-smoke.json")) && w.Model.Highscores.Count == 0;
            CheckIt("fresh GUID and score override established before StartGame", isolated, name + "; score=" + report.scorePath);
            if (!isolated) yield break;
            if (!body || !actorQuery || !hud || hinges == null) throw new InvalidOperationException("Missing production body/HUD/hinge map or independent capsule query.");
            yield return Settle(.12f); w.StartGame(false); yield return Settle(.2f);
            var bath = w.Model.Items.Single(i => i.Id == "bathroom_door");
            Fixture(new Vector3(3.25f, 3.28f, 2.4f), true); yield return Settle(.3f);
            RaycastHit oldWest;
            bool westSolid = Physics.Raycast(new Vector3(2.8f, 4.05f, 2.5f), Vector3.left, out oldWest, 1.6f, ~(1 << 30), QueryTriggerInteraction.Ignore) &&
                Mathf.Abs(oldWest.point.x - 2) < .18f && oldWest.collider.GetComponentInParent<HomePick>() == null;
            var rails = FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(c => c.name.StartsWith("Stair rail ", StringComparison.Ordinal)).ToArray();
            bool geometry = Vector3.Distance(bath.Position, new Vector3(3.25f, 3.2f, 1.8f)) < .01f && Mathf.Abs(bath.Rotation) < .01f &&
                !w.Model.Items.Any(i => i.Id == "door_inner_upper") && westSolid && ControllerContract() && rails.Length == 64 && rails.All(c => c.enabled && c.gameObject.activeInHierarchy);
            CheckIt("south bathroom doorway, continuous former west entrance and V42 physics invariants", geometry,
                "bath=" + bath.Position + "; yaw=" + bath.Rotation + "; west=" + (westSolid ? oldWest.collider.name : "missing") + "; permanent rails=" + rails.Length);
            // The geometry fixture above is the single bathroom setup warp.
            yield return DoorFixture("bathroom_door", true, true);
            yield return DoorFixture("door", false, false);
            CheckIt("both representative doors open away through E from both sides", report.doors.Count == 4 && report.doors.All(p => p.openedAway),
                string.Join("; ", report.doors.Select(p => p.id + "/" + p.side + "=" + p.openAngle.ToString("F1")).ToArray()));
            CheckIt("closing pauses with logical close pending and no player push or leaf penetration", report.doors.Count == 4 && report.doors.All(p => p.blocked && p.leafStopped && p.logicalClosePending && p.noPush && p.maxPenetration < .006f),
                string.Join("; ", report.doors.Select(p => p.id + "/" + p.side + ": blocked=" + p.blockedFrames + ", drift=" + p.maxActorDrift.ToString("F4") + ", overlap=" + p.maxPenetration.ToString("F4") + ", angleRange=" + p.angleRange.ToString("F2")).ToArray()));
            CheckIt("actual walking clears the arc and closure resumes without another E", report.doors.All(p => p.resumed),
                "two directions per door; no fixture warp or hinge mutation during hold/clear");
            yield return PlacementFixture();
            yield return RequirementsFixture();
            report.completed = true;
        }
        IEnumerator DoorFixture(string id, bool alreadyPlaced, bool capture)
        {
            Hinge fixture = ReadHinge(id); Vector3 origin = fixture.frame.position;
            Vector3 normal = fixture.frame.TransformDirection(fixture.closed * Vector3.forward).normalized;
            if (!alreadyPlaced) { Fixture(origin + normal * .6f + Vector3.up * .08f, true); yield return Settle(.25f); }
            for (int side = 1; side >= -1; side -= 2)
            {
                var p = new DoorProbe { id = id, side = side > 0 ? "positive" : "negative", setup = w.PlayerPosition, expectedAngle = side * 100 };
                report.doors.Add(p); w.SetFocus(id); bool canOpen = w.CanInteract;
                w.HandleKey(KeyCode.E); yield return Settle(.58f);
                Hinge opened = ReadHinge(id); p.openAngle = Angle(opened);
                p.openedAway = canOpen && opened.open && Mathf.Abs(Mathf.DeltaAngle(p.expectedAngle, p.openAngle)) < 3.5f;
                yield return WalkTo(id + " cross into closing arc " + p.side, origin - normal * side * .55f);
                bool crossed = report.walks.Last().reached; yield return Settle(.25f);
                p.arcPosition = w.PlayerPosition; w.SetFocus(id); bool canClose = w.CanInteract;
                w.HandleKey(KeyCode.E); float until = Time.realtimeSinceStartup + .78f, minAngle = 999, maxAngle = -999;
                bool logical = true, lastBlocked = false; int index = 0;
                while (Time.realtimeSinceStartup < until)
                {
                    w.SmokeMove(Vector2.zero, Dt); Hinge state = ReadHinge(id);
                    float drift = Vector3.Distance(p.arcPosition, w.PlayerPosition);
                    p.maxActorDrift = Mathf.Max(p.maxActorDrift, drift); p.maxPenetration = Mathf.Max(p.maxPenetration, LeafOverlap(state));
                    logical &= !state.open;
                    if (state.blocked)
                    {
                        if (!lastBlocked) p.blockedAngle = Angle(state);
                        p.blockedFrames++; minAngle = Mathf.Min(minAngle, Angle(state)); maxAngle = Mathf.Max(maxAngle, Angle(state));
                    }
                    lastBlocked = state.blocked;
                    if (index++ % 4 == 0) { p.stationarySamples.Add(w.PlayerPosition); p.stationaryAngles.Add(Angle(state)); }
                    yield return null;
                }
                p.afterHold = w.PlayerPosition; p.angleRange = maxAngle - minAngle;
                p.blocked = canClose && crossed && lastBlocked && p.blockedFrames >= 5 && Mathf.Abs(p.blockedAngle) > 12;
                p.leafStopped = p.blocked && p.angleRange < .25f;
                p.logicalClosePending = logical; p.noPush = p.maxActorDrift < .025f && Supported(origin.y);
                if (capture && side == 1) yield return Shot("bathroom-close-paused-actual-render");
                p.clearTarget = origin - normal * side * 1.45f; watchedDoor = id; walkPenetration = 0;
                yield return WalkTo(id + " physically leave closing arc " + p.side, p.clearTarget);
                p.maxPenetration = Mathf.Max(p.maxPenetration, walkPenetration); watchedDoor = null;
                bool clear = report.walks.Last().reached; yield return Settle(.62f);
                Hinge closed = ReadHinge(id); p.afterClear = w.PlayerPosition;
                p.resumed = clear && !closed.open && !closed.blocked && Quaternion.Angle(closed.closed, closed.rotation) < 3.5f && Supported(origin.y);
                // This physical return supplies the next side's opening fixture.
                yield return WalkTo(id + " restore closed-side approach " + p.side, origin - normal * side * .6f);
                p.resumed &= report.walks.Last().reached;
            }
        }
        IEnumerator PlacementFixture()
        {
            var item = w.Model.Items.Single(i => i.Id == "up_cabinet");
            var source = w.Expedition.Sources.Single(s => s.Id == item.Id);
            Vector3 original = item.Position; float rotation = item.Rotation; int budget = w.Model.Budget;
            Fixture(new Vector3(3.1f, 3.28f, -3.25f), false); yield return Settle(.45f);
            var visual = FindObjectsByType<HomePick>(FindObjectsSortMode.None).Single(p => p.Id == item.Id).gameObject;
            var renderers = visual.GetComponentsInChildren<Renderer>(true); bool[] rendererStates = renderers.Select(r => r.enabled).ToArray();
            var colliders = visual.GetComponentsInChildren<Collider>(true); bool[] colliderStates = colliders.Select(c => c.enabled).ToArray();
            Vector3 pickScreen = w.View.WorldToScreenPoint(item.Position + Vector3.up * item.Size.y * .5f);
            bool mouseBegan = StartMousePlacement(new Vector2(pickScreen.x, pickScreen.y));
            bool pickedSource = w.Selected != null && w.Selected.Id == item.Id && w.NearestSource != null && w.NearestSource.Id == source.Id;
            bool sameFrameRejected = !ConfirmPlacement(); yield return null;
            Vector3 valid = new Vector3(4.05f, original.y, -4.5f);
            bool actorInvalid = !Pointer(w.PlayerPosition, false) && !ConfirmPlacement();
            string reason; bool voidInvalid = !w.ValidatePlacement(item, new Vector3(0, original.y, 2.5f), rotation, out reason);
            bool wallInvalid = !w.ValidatePlacement(item, new Vector3(5, original.y, 1.8f), rotation, out reason);
            bool unchanged = item.Position == original && item.Rotation == rotation && source.Position == original && w.Model.Budget == budget &&
                visual.activeInHierarchy && colliders.Select((c, i) => c.enabled == colliderStates[i]).All(x => x);
            CheckIt("mouse ray selects storage source; actor/wall/stair-void candidates reject without commit", mouseBegan && pickedSource && w.IsMoving && sameFrameRejected && actorInvalid && voidInvalid && wallInvalid && unchanged, w.PlacementMessage);
            bool preview = Pointer(valid, false); w.RotatePlacement(45);
            preview &= w.PlacementValid && Mathf.Abs(w.PlacementFloorY - original.y) < .001f && Mathf.Abs(w.PlacementCandidate.y - original.y) < .001f;
            yield return Settle(.18f); yield return Shot("mouse-placement-preview-actual-render");
            Vector3 candidate = w.PlacementCandidate;
            bool overUi = !w.UpdatePlacementPointer(w.View.WorldToScreenPoint(candidate), true) && !ConfirmPlacement() &&
                !w.PlacementValid && w.IsMoving && w.PlacementCandidate == candidate && item.Position == original && w.Model.Budget == budget;
            CheckIt("HUD-overclick blocks placement confirmation and retains preview candidate", preview && overUi, "candidate=" + candidate + "; floor=" + w.PlacementFloorY);
            w.CancelMove(); yield return null;
            bool canceled = !w.IsMoving && item.Position == original && item.Rotation == rotation && source.Position == original &&
                visual.transform.position == original && renderers.Select((r, i) => r.enabled == rendererStates[i]).All(x => x) &&
                colliders.Select((c, i) => c.enabled == colliderStates[i]).All(x => x);
            CheckIt("cancel restores original furniture and storage source without cost", canceled && w.Model.Budget == budget, "original=" + original);
            w.SetFocus(item.Id); StartPlacement(); yield return null;
            bool pointer = Pointer(valid, false); w.RotatePlacement(45); bool commit = ConfirmPlacement();
            Vector3 committed = item.Position; float committedRotation = item.Rotation; bool duplicate = ConfirmPlacement();
            CheckIt("valid screen-ray placement commits once on captured floor and synchronizes source", pointer && commit && !duplicate && !w.IsMoving &&
                Vector3.Distance(committed, valid) < .035f && Mathf.Abs(committed.y - original.y) < .001f && Mathf.Abs(Mathf.DeltaAngle(rotation + 45, committedRotation)) < .01f &&
                source.Position == committed && source.Floor == 1 && visual.transform.position == committed && item.Position == committed && item.Rotation == committedRotation && w.Model.Budget == budget,
                "world=" + committed + "; source=" + source.Position + "; yaw=" + committedRotation);
        }
        // InputLocked isolates the fixture clock. No frame or automatic pointer update runs unlocked.
        bool StartMousePlacement(Vector2 screen) { bool locked = w.InputLocked; w.InputLocked = false; try { return w.BeginMousePlacement(screen, false); } finally { w.InputLocked = locked; } }
        void StartPlacement() { bool locked = w.InputLocked; w.InputLocked = false; try { w.BeginMove(); } finally { w.InputLocked = locked; } }
        bool ConfirmPlacement() { bool locked = w.InputLocked; w.InputLocked = false; try { return w.ConfirmMove(); } finally { w.InputLocked = locked; } }
        IEnumerator RequirementsFixture()
        {
            int budget = w.Model.Budget; string stock = Stock(); int fences = w.Model.FenceCount; long score = w.Model.Score;
            Fixture(new Vector3(0, .08f, 8.8f), false); yield return Settle(.35f);
            w.SetFocus("fence_north"); bool reachable = w.CanInteract; w.HandleKey(KeyCode.E); yield return Settle(.18f);
            report.requirementsBefore = Requirements();
            var canonical = PanicExpedition.MaterialCosts("fence", null);
            bool exact = canonical.Count == 3 && report.requirementsBefore.Count == 3 &&
                report.requirementsBefore.All(r => canonical.Any(c => c.Id == r.id && c.Required == r.required) && r.owned == 0 && r.missing == r.required) &&
                report.requirementsBefore.Any(r => r.id == "plank" && r.missing == 3) &&
                report.requirementsBefore.Any(r => r.id == "screws" && r.missing == 2) &&
                report.requirementsBefore.Any(r => r.id == "sandbag" && r.missing == 1);
            bool icons = report.requirementsBefore.All(r => PanicResidenceArt.Icon(r.id) != null);
            CheckIt("actual failed E action exposes canonical shortfalls and real icon HUD with no cost", reachable && exact && icons && hud.RequirementsCardVisible &&
                w.Expedition.HasMissingRequirements && w.Expedition.RequirementAction == "fence" && w.Expedition.RequirementTargetId == "fence_north" &&
                w.Expedition.RequirementBudgetCost == PanicExpedition.WorkCost("fence", null) && w.Expedition.RequirementBudgetOwned == budget &&
                w.Expedition.RequirementBudgetMissing == 0 && !w.WorkActive && Stock() == stock && w.Model.Budget == budget && w.Model.FenceCount == fences && w.Model.Score == score,
                "requirements=" + string.Join(",", report.requirementsBefore.Select(r => r.id + ":" + r.owned + "/" + r.required + " missing=" + r.missing).ToArray()));
            yield return Shot("missing-material-icons-actual-hud");
            Fixture(new Vector3(-4, -3.12f, 3), false); yield return Settle(.35f);
            w.SetFocus("basement_pantry"); bool canGather = w.CanInteract; w.HandleKey(KeyCode.E); yield return Settle(.15f);
            var pantry = w.Expedition.Sources.Single(s => s.Id == "basement_pantry");
            int index = pantry.Contents.FindIndex(s => s.Id == "plank"); int available = index >= 0 ? pantry.Contents[index].Count : 0;
            if (index >= 0) w.SelectLoot(index); string selected = w.SelectedLootId;
            w.HandleKey(KeyCode.F); yield return Settle(.18f); w.HandleKey(KeyCode.E); yield return Settle(.12f);
            report.requirementsAfter = Requirements();
            bool updated = report.requirementsAfter.Any(r => r.id == "plank" && r.required == 3 && r.owned == available && r.missing == 0) &&
                w.Expedition.MissingRequirements.Count == 2 && w.Expedition.MissingRequirements.All(r => r.Id == "screws" && r.Missing == 2 || r.Id == "sandbag" && r.Missing == 1);
            CheckIt("real E/F pantry gathering immediately reduces dynamic deficits without starting work", canGather && selected == "plank" && available == 4 &&
                w.GatherItemId == "plank" && w.Expedition.Count("plank") == 4 && updated && hud.RequirementsCardVisible &&
                !w.WorkActive && w.Model.Budget == budget && w.Model.FenceCount == fences && w.Model.Score == score,
                "gathered plank=" + w.Expedition.Count("plank") + "; remaining missing rows=" + w.Expedition.MissingRequirements.Count);
        }
        List<Requirement> Requirements()
        {
            return w.Expedition.CurrentRequirements.Select(r => new Requirement { id = r.Id, required = r.Required, owned = r.Owned, missing = r.Missing }).ToList();
        }
        string Stock() { return string.Join("|", w.Expedition.Inventory.Select(s => s.Id + ":" + s.Count).ToArray()); }
        bool Pointer(Vector3 candidate, bool overUi)
        {
            Vector3 screen = w.View.WorldToScreenPoint(candidate);
            return screen.z > 0 && w.UpdatePlacementPointer(new Vector2(screen.x, screen.y), overUi);
        }
        void Fixture(Vector3 p, bool door)
        {
            w.Warp(p); if (door) report.doorSetupWarps++; else report.otherSetupWarps++; Physics.SyncTransforms();
        }
        IEnumerator WalkTo(string id, Vector3 target)
        {
            var walk = new Walk { id = id, start = w.PlayerPosition, target = target, seconds = Time.realtimeSinceStartup };
            report.walks.Add(walk); float deadline = Time.realtimeSinceStartup + 3.2f;
            while (Time.realtimeSinceStartup < deadline)
            {
                Vector3 delta = target - w.PlayerPosition; delta.y = 0;
                if (delta.magnitude < .035f) { walk.reached = true; break; }
                Vector3 direction = Vector3.ClampMagnitude(delta / .35f, 1);
                Vector3 forward = w.View.transform.forward; forward.y = 0; forward.Normalize();
                Vector3 right = w.View.transform.right; right.y = 0; right.Normalize();
                w.SmokeMove(new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)), Dt);
                if (watchedDoor != null) walkPenetration = Mathf.Max(walkPenetration, LeafOverlap(ReadHinge(watchedDoor)));
                walk.frames++; yield return null;
            }
            yield return Settle(.15f); walk.end = w.PlayerPosition; walk.seconds = Time.realtimeSinceStartup - walk.seconds;
            Vector3 difference = walk.end - target; difference.y = 0; walk.reached &= difference.magnitude < .13f;
        }
        IEnumerator Settle(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) { w.SmokeMove(Vector2.zero, Dt); yield return null; }
        }
        Hinge ReadHinge(string id)
        {
            object value = ((IDictionary)hinges.GetValue(w))[id];
            if (value == null) throw new InvalidOperationException("Missing production hinge " + id);
            Type t = value.GetType(); var leaf = (Transform)t.GetField("leaf").GetValue(value);
            return new Hinge { leaf = leaf, frame = (Transform)t.GetField("frame").GetValue(value), rotation = leaf.localRotation,
                closed = (Quaternion)t.GetField("closed").GetValue(value), open = (bool)t.GetField("open").GetValue(value),
                blocked = (bool)t.GetField("closeBlocked").GetValue(value), angle = (float)t.GetField("openAngle").GetValue(value) };
        }
        static float Angle(Hinge h) { return Mathf.DeltaAngle(h.closed.eulerAngles.y, h.rotation.eulerAngles.y); }
        float LeafOverlap(Hinge h)
        {
            Physics.SyncTransforms(); float maximum = 0;
            foreach (var c in h.leaf.GetComponentsInChildren<Collider>())
            {
                if (!c.enabled || c.isTrigger || !c.gameObject.activeInHierarchy) continue;
                Vector3 direction; float distance;
                if (Physics.ComputePenetration(c, c.transform.position, c.transform.rotation, actorQuery, body.transform.position, body.transform.rotation, out direction, out distance))
                    maximum = Mathf.Max(maximum, distance);
            }
            return maximum;
        }
        bool ControllerContract()
        {
            return body.enabled && Mathf.Abs(body.radius - .25f) < .001f && Mathf.Abs(body.height - 1.78f) < .001f &&
                Mathf.Abs(body.stepOffset - .25f) < .001f && Mathf.Abs(body.skinWidth - .025f) < .001f && Mathf.Abs(body.slopeLimit - 48) < .001f;
        }
        bool Supported(float y) { return body.enabled && w.IsGrounded && Mathf.Abs(w.PlayerPosition.y - y) < .16f; }
        IEnumerator Shot(string id)
        {
            yield return new WaitForEndOfFrame(); string path = Path.Combine(w.EvidencePath, id + ".png");
            ScreenCapture.CaptureScreenshot(path); report.screenshots.Add(path);
            long previous = -1; int stable = 0; float deadline = Time.realtimeSinceStartup + 1.5f;
            while (Time.realtimeSinceStartup < deadline)
            {
                long size = File.Exists(path) ? new FileInfo(path).Length : 0;
                stable = size >= 256 && size == previous ? stable + 1 : 0;
                if (stable >= 2) yield break; previous = size; yield return null;
            }
            report.errors.Add("Actual screenshot did not finish writing: " + path);
        }
        void CheckIt(string name, bool passed, string detail)
        {
            report.checks.Add(new Check { name = name, passed = passed, detail = detail });
            Debug.Log("INTERACTION_CHECK " + (passed ? "PASS " : "FAIL ") + name + " " + detail);
        }
        void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack);
        }
        void Update()
        {
            if (finished || !w || Time.realtimeSinceStartup - started <= HardCap) return;
            report.errors.Add("Focused interaction smoke exceeded 85 second cap; unfinished checks are not accepted.");
            if (running != null) StopCoroutine(running); Finish();
        }
        void Finish()
        {
            if (finished) return; finished = true; report.finishedUtc = DateTime.UtcNow.ToString("o"); report.runtimeSeconds = Time.realtimeSinceStartup - started;
            foreach (string p in report.screenshots) if (!File.Exists(p) || new FileInfo(p).Length < 256) report.errors.Add("Missing/empty actual render: " + p);
            report.passed = report.completed && report.errors.Count == 0 && report.checks.Count == ExpectedChecks && report.checks.All(c => c.passed) && report.screenshots.Count == 3;
            if (actorQuery) Destroy(actorQuery.gameObject);
            Application.logMessageReceived -= Log;
            try { File.WriteAllText(Path.Combine(w.EvidencePath, "native-interaction-smoke.json"), JsonUtility.ToJson(report, true)); }
            catch (Exception ex) { Debug.LogError("INTERACTION_SMOKE report write failed: " + ex); }
            Debug.Log("INTERACTION_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " errors=" + report.errors.Count + " seconds=" + report.runtimeSeconds.ToString("F1"));
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}
