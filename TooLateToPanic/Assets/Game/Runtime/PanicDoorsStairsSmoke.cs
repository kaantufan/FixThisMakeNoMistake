using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    // Opt-in only. Root attaches AFTER Awake sets ScorePathOverride and constructs the world.
    public sealed class PanicDoorsStairsSmoke : MonoBehaviour
    {
        [Serializable] sealed class Check { public string name, utc, detail; public bool passed; }
        [Serializable] sealed class Walk
        {
            public string id; public Vector3 start, target, end; public bool reached;
            public int frames; public float seconds, minX = 999, maxX = -999, minY = 999, maxY = -999;
            public List<Vector3> samples = new List<Vector3>();
        }
        [Serializable] sealed class DoorProbe
        {
            public string id, side, closedFirstObstacle; public Vector3 actor, normal, stoppedAtClosed;
            public bool canInteract, closedBlocked, opensAway, frozenAcrossOpening, closes, repeatStable, othersUnchanged;
            public Vector3 closeRetreat, actorAfterClose, nextClosedSide;
            public float expectedAngle, actualAngle, angleAfterCrossing, tipDisplacementAlongNormal;
        }
        [Serializable] sealed class Report
        {
            public string startedUtc, finishedUtc, profileId, scorePath, repeatRepresentativeId;
            public string evidenceType = "Affected native API smoke: actual E interaction, CanInteract and independent occlusion ray, closed-leaf CharacterController collision, signed rendered DoorLeaf rotation from both sides, physical basement/ground/upper stair traversal and side pushes, rendered door and stair captures.";
            public string limitations = "Automatic API input, not manual keyboard/mouse or final visual acceptance. Door fixture setup warps and one basement route setup warp are explicit. All doorway crossings and the complete basement-to-upper-and-back stair route use the production SmokeMove/CharacterController.Move. Reflection reads hinge state only. No direct hinge mutation, collider/controller relaxation, timer/score/cost changes, campaign or cold-save coverage.";
            public bool completed, passed; public float runtimeSeconds; public int doorSetupWarps, stairSetupWarps;
            public List<Check> checks = new List<Check>(); public List<Walk> walks = new List<Walk>();
            public List<DoorProbe> doors = new List<DoorProbe>(); public List<string> geometry = new List<string>();
            public List<string> errors = new List<string>(); public List<string> screenshots = new List<string>();
        }
        sealed class HingeState
        {
            public string id; public bool open, directional, lid, drawer; public float angle;
            public Transform leaf, frame; public Quaternion closed, rotation; public Vector3 position;
        }

        const float HardCapSeconds = 85;
        // Match production's real elapsed frame time and its .04s safety clamp.
        static float MoveDt { get { return Mathf.Clamp(Time.unscaledDeltaTime, .008f, .04f); } }
        static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        readonly Report report = new Report();
        PanicHomeWorld w; CharacterController body; FieldInfo hingeField; Coroutine running;
        float started; bool finished; bool doorAccess = true, doorAway = true, doorFrozen = true, doorOthers = true;
        bool railWest, railEast; Dictionary<string, HingeState> lockedHinges; string movingDoor;
        bool movingDoorStable;

        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--doors-stairs-smoke") < 0) { enabled = false; return; }
            w = GetComponent<PanicHomeWorld>();
            if (!w || string.IsNullOrEmpty(w.EvidencePath)) { Debug.LogError("DOORS_STAIRS_SMOKE requires PanicHomeWorld and --home-evidence set before Model construction."); enabled = false; return; }
            started = Time.realtimeSinceStartup; report.startedUtc = DateTime.UtcNow.ToString("o");
            report.profileId = Path.GetFileName(Path.GetFullPath(w.EvidencePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            report.scorePath = PanicModel.ScorePathOverride;
            Application.logMessageReceived += Log; Application.runInBackground = true; w.InputLocked = true;
            hingeField = typeof(PanicHomeWorld).GetField("hinges", PrivateInstance);
            body = FindObjectsByType<CharacterController>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Survivor");
            running = StartCoroutine(Guarded(Run()));
        }

        IEnumerator Guarded(IEnumerator sequence)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(sequence);
            while (stack.Count > 0 && !finished)
            {
                bool next; object current = null;
                try { next = stack.Peek().MoveNext(); if (next) current = stack.Peek().Current; }
                catch (Exception ex) { report.errors.Add(ex.ToString()); break; }
                if (!next) { stack.Pop(); continue; }
                var nested = current as IEnumerator;
                if (nested != null) stack.Push(nested); else yield return current;
            }
            Finish();
        }

        IEnumerator Run()
        {
            string expectedScore = Path.GetFullPath(Path.Combine(w.EvidencePath, "isolated-home-scores.json"));
            string name = report.profileId; Guid profileGuid;
            bool guid = name.Length >= 36 && Guid.TryParse(name.Substring(name.Length - 36), out profileGuid);
            bool isolated = guid && !string.IsNullOrEmpty(PanicModel.ScorePathOverride) &&
                string.Equals(Path.GetFullPath(PanicModel.ScorePathOverride), expectedScore, StringComparison.OrdinalIgnoreCase) &&
                !File.Exists(expectedScore) && !File.Exists(Path.Combine(w.EvidencePath, "native-doors-stairs-smoke.json")) && w.Model.Highscores.Count == 0;
            CheckIt("fresh GUID profile and isolated score path before StartGame", isolated, "profile=" + name + "; score=" + report.scorePath);
            if (!isolated) yield break;
            if (!body || hingeField == null) throw new InvalidOperationException("Missing production Survivor CharacterController or hinge inspection field.");
            yield return Settle(4); w.StartGame(false); yield return Settle(6);

            var initial = Hinges(); var doors = w.Model.Items.Where(i => i.Kind == ItemKind.Door).ToArray();
            bool classified = doors.Length > 0 && doors.All(i => initial.ContainsKey(i.Id) && initial[i.Id].directional && initial[i.Id].leaf.name == "DoorLeaf") &&
                initial.Values.Where(h => !doors.Any(d => d.Id == h.id)).All(h => !h.directional && Mathf.Abs(h.angle - 100) < .01f);
            CheckIt("unchanged production controller and permanent solid rail/void geometry", ControllerContract() && GeometryContract() && classified,
                "doors=" + string.Join(",", doors.Select(i => i.Id).ToArray()) + "; directional classification=" + classified + "; controller=" + body.radius + "/" + body.stepOffset + "/" + body.slopeLimit);

            foreach (var door in doors) yield return DoorFixture(door, door.Id == doors[doors.Length - 1].Id);
            CheckIt("every model door is reachable from both sides and its closed leaf blocks real movement", doorAccess && report.doors.Count == doors.Length * 2,
                "fixtures=" + doors.Length + "; side probes=" + report.doors.Count + "; setup warps=" + report.doorSetupWarps);
            CheckIt("every model door opens away from the actor on both closed-frame sides", doorAway,
                string.Join(";", report.doors.Select(p => p.id + "/" + p.side + ": expected=" + p.expectedAngle + ", actual=" + p.actualAngle.ToString("F1") + ", tip=" + p.tipDisplacementAlongNormal.ToString("F2")).ToArray()));
            CheckIt("open leaves keep their sign across actual crossings, close and repeat; other hinges stay unchanged", doorFrozen && doorOthers && OtherHingesUnchanged(initial, Hinges(), null),
                "frozen/close/repeat=" + doorFrozen + "; all non-target leaves/lids/drawers/hatch unchanged=" + doorOthers);

            // Only setup warp for the complete stair route. No warp occurs until that route ends.
            w.Warp(new Vector3(0, -3.12f, -.65f)); report.stairSetupWarps++; yield return Settle(10);
            yield return Ascend(-1);
            CheckIt("basement to ground uses physical treads; both flight rails block side pushes", LastReached(0, 5.35f) && railWest && railEast,
                "arrival=" + w.PlayerPosition + "; west=" + railWest + "; east=" + railEast);

            yield return WalkTo("ground west north landing", new Vector3(-1.5f, 0, 5.35f));
            yield return WalkTo("ground west void approach", new Vector3(-1.5f, 0, 1.5f));
            Vector3 groundStart = w.PlayerPosition; yield return Push("ground west floor-void push east", Vector3.right, .42f);
            var groundPush = report.walks.Last();
            CheckIt("ground west floor-void guard blocks the actor without a fall", groundPush.maxX <= -1.22f && groundPush.minY > -.15f && Supported(0) && w.PlayerPosition.x > groundStart.x + .06f,
                WalkDetail(groundPush));
            yield return WalkTo("ground west side recovery", new Vector3(-1.5f, 0, 1.5f));
            yield return WalkTo("ground west floor route to next flight", new Vector3(-1.5f, 0, -.65f));
            yield return WalkTo("ground next flight bottom", new Vector3(0, 0, -.65f));
            yield return Ascend(0);
            CheckIt("ground to upper uses physical treads; both flight rails block side pushes", LastReached(3.2f, 5.35f) && railWest && railEast,
                "arrival=" + w.PlayerPosition + "; west=" + railWest + "; east=" + railEast);

            yield return WalkTo("upper east north landing", new Vector3(1.5f, 3.2f, 5.35f));
            yield return WalkTo("upper east void approach", new Vector3(1.5f, 3.2f, 3.9f));
            Vector3 upperStart = w.PlayerPosition; yield return Push("upper east floor-void push west", Vector3.left, .42f);
            var upperPush = report.walks.Last();
            CheckIt("upper east floor-void guard blocks the actor without a fall", upperPush.minX >= 1.22f && upperPush.minY > 3.05f && Supported(3.2f) && w.PlayerPosition.x < upperStart.x - .06f,
                WalkDetail(upperPush));
            yield return WalkTo("upper east side recovery", new Vector3(1.5f, 3.2f, 3.9f));
            yield return Shot("physical-stair-rail-and-upper-opening");
            yield return WalkTo("upper east return landing", new Vector3(1.5f, 3.2f, 5.35f));
            yield return WalkTo("upper stair return entrance", new Vector3(0, 3.2f, 5.35f));
            yield return WalkTo("upper flight physical descent", new Vector3(0, 0, -.65f));
            var upperDown = report.walks.Last();
            CheckIt("upper to ground descends the actual flight", upperDown.reached && LastReached(0, -.65f) && upperDown.minY > -.15f && upperDown.maxY < 3.4f && upperDown.minX > -.61f && upperDown.maxX < .61f,
                WalkDetail(upperDown));
            yield return WalkTo("ground west bottom return path", new Vector3(-1.5f, 0, -.65f));
            yield return WalkTo("ground west floor return to basement entrance", new Vector3(-1.5f, 0, 5.35f));
            yield return WalkTo("basement flight top return entrance", new Vector3(0, 0, 5.35f));
            yield return WalkTo("basement flight physical descent", new Vector3(0, -3.2f, -.65f));
            var basementDown = report.walks.Last();
            CheckIt("returns to basement using one setup warp and continuous supported floor/stair route", basementDown.reached && LastReached(-3.2f, -.65f) &&
                basementDown.minY > -3.35f && basementDown.maxY < .2f && basementDown.minX > -.61f && basementDown.maxX < .61f &&
                report.stairSetupWarps == 1 && report.walks.All(t => t.id.Contains("closed collision") || t.id.Contains("push") || t.reached) &&
                ControllerContract() && w.Model.Phase == PanicPhase.Preparing && w.Model.Score == 0,
                WalkDetail(basementDown) + "; stair setup warps=" + report.stairSetupWarps + "; phase=" + w.Model.Phase);
            report.completed = true;
        }

        IEnumerator DoorFixture(PanicItem door, bool capture)
        {
            var fixture = Hinges()[door.Id]; Vector3 normal = fixture.frame.TransformDirection(fixture.closed * Vector3.forward).normalized;
            Vector3 latchDirection = fixture.frame.TransformDirection(fixture.closed * Vector3.right).normalized;
            Vector3 origin = fixture.frame.position;
            w.Warp(origin + normal * .6f + Vector3.up * .08f); report.doorSetupWarps++; yield return Settle(8);
            for (int sideIndex = 0; sideIndex < 2; sideIndex++)
            {
                float side = sideIndex == 0 ? 1 : -1;
                var p = new DoorProbe { id = door.Id, side = side > 0 ? "positive" : "negative", normal = normal, actor = w.PlayerPosition, expectedAngle = side * 100 };
                report.doors.Add(p); w.SetFocus(door.Id); Physics.SyncTransforms();
                p.closedFirstObstacle = FirstObstacle(door.Position, door.Id);
                p.canInteract = w.CanInteract && w.Selected != null && w.Selected.Id == door.Id && p.closedFirstObstacle == door.Id && Supported(door.Position.y) &&
                    side * Vector3.Dot(p.actor - origin, normal) > .45f && Mathf.Abs(Vector3.Dot(p.actor - origin, latchDirection)) < .15f;
                // .30s deliberately commands enough travel to cross the closed plane if collision were absent.
                yield return Push(door.Id + " " + p.side + " closed collision", normal * -side, .30f);
                p.stoppedAtClosed = w.PlayerPosition;
                float before = Vector3.Dot(p.actor - origin, normal), after = Vector3.Dot(p.stoppedAtClosed - origin, normal);
                p.closedBlocked = before * after > 0 && Mathf.Abs(after) > .13f && Mathf.Abs(before) - Mathf.Abs(after) > .06f && Supported(door.Position.y);
                var beforeOpen = Hinges(); w.SetFocus(door.Id); w.HandleKey(KeyCode.E); yield return SettleFor(.48f);
                var open = Hinges(); var h = open[door.Id]; p.actualAngle = SignedAngle(h);
                Vector3 closedTip = h.frame.TransformPoint(h.position + h.closed * (Vector3.right * .95f));
                Vector3 openTip = h.leaf.TransformPoint(Vector3.right * .95f);
                p.tipDisplacementAlongNormal = Vector3.Dot(openTip - closedTip, normal);
                p.opensAway = h.open && Mathf.Abs(Mathf.DeltaAngle(p.expectedAngle, p.actualAngle)) < 3.5f && side * p.tipDisplacementAlongNormal < -.65f;
                p.othersUnchanged = OtherHingesUnchanged(beforeOpen, open, door.Id);
                lockedHinges = open; movingDoor = door.Id; movingDoorStable = true;
                yield return WalkTo(door.Id + " " + p.side + " actual open doorway crossing", origin - normal * side * .6f);
                var crossed = Hinges(); p.angleAfterCrossing = SignedAngle(crossed[door.Id]);
                p.frozenAcrossOpening = report.walks.Last().reached && movingDoorStable && crossed[door.Id].open &&
                    Mathf.Abs(Mathf.DeltaAngle(p.actualAngle, p.angleAfterCrossing)) < 3.5f && Mathf.Abs(crossed[door.Id].angle - p.expectedAngle) < .01f;
                lockedHinges = null; movingDoor = null;
                if (capture && sideIndex == 1) { yield return SettleFor(.2f); yield return Shot("door-away-from-actor-both-sides"); }
                // The opposite-side arrival is inside the closing leaf's sweep. Physically leave
                // that sweep, close, then walk back to the exact closed-side staging point.
                // West inner-door approaches cannot retreat normally into the guarded stair void.
                float arrivalSide = -side;
                Vector3 stagingPoint = origin + normal * arrivalSide * .6f;
                bool narrowWest = arrivalSide < 0 && (door.Id == "door_inner_ground" || door.Id == "bathroom_door");
                p.closeRetreat = narrowWest ? stagingPoint + latchDirection * 1.05f : origin + normal * arrivalSide * 1.45f;
                yield return WalkTo(door.Id + " " + p.side + " leave closing sweep", p.closeRetreat);
                bool safeRetreat = report.walks.Last().reached && Supported(door.Position.y);
                Vector3 beforeClosePosition = w.PlayerPosition;
                w.SetFocus(door.Id); bool canClose = w.CanInteract; w.HandleKey(KeyCode.E); yield return SettleFor(.48f);
                p.actorAfterClose = w.PlayerPosition;
                var closed = Hinges(); p.closes = safeRetreat && canClose && !closed[door.Id].open &&
                    Quaternion.Angle(closed[door.Id].closed, closed[door.Id].rotation) < 3.5f && Vector3.Distance(beforeClosePosition, p.actorAfterClose) < .12f;
                yield return WalkTo(door.Id + " " + p.side + " restore opposite closed-side fixture", stagingPoint);
                p.nextClosedSide = w.PlayerPosition;
                p.closes &= report.walks.Last().reached && arrivalSide * Vector3.Dot(p.nextClosedSide - origin, normal) > .45f &&
                    Mathf.Abs(Vector3.Dot(p.nextClosedSide - origin, latchDirection)) < .15f;
                p.othersUnchanged &= OtherHingesUnchanged(beforeOpen, closed, door.Id);
                doorAccess &= p.canInteract && p.closedBlocked; doorAway &= p.opensAway; doorFrozen &= p.frozenAcrossOpening && p.closes; doorOthers &= p.othersUnchanged;
            }
            // One representative repeat after a full two-sided cycle keeps this smoke focused.
            if (capture)
            {
                report.repeatRepresentativeId = door.Id;
                w.SetFocus(door.Id); bool canRepeat = w.CanInteract; w.HandleKey(KeyCode.E); yield return SettleFor(.48f);
                var repeat = Hinges(); bool stable = canRepeat && repeat[door.Id].open && Mathf.Abs(Mathf.DeltaAngle(100, SignedAngle(repeat[door.Id]))) < 3.5f;
                yield return WalkTo(door.Id + " representative repeat leave closing sweep", origin + normal * 1.45f);
                stable &= report.walks.Last().reached; Vector3 beforeClosePosition = w.PlayerPosition;
                w.SetFocus(door.Id); stable &= w.CanInteract; w.HandleKey(KeyCode.E); yield return SettleFor(.48f);
                var final = Hinges(); stable &= !final[door.Id].open && Quaternion.Angle(final[door.Id].closed, final[door.Id].rotation) < 3.5f && Vector3.Distance(beforeClosePosition, w.PlayerPosition) < .12f;
                yield return WalkTo(door.Id + " representative repeat restore closed fixture", origin + normal * .6f);
                stable &= report.walks.Last().reached;
                report.doors[report.doors.Count - 1].repeatStable = stable; doorFrozen &= stable;
            }
        }

        IEnumerator Ascend(int level)
        {
            float baseY = level * 3.2f;
            yield return WalkTo("flight " + level + " physical lower half", new Vector3(0, baseY + 1.6f, 2.25f));
            Vector3 mid = w.PlayerPosition;
            yield return Push("flight " + level + " west rail push", Vector3.left, .43f);
            var west = report.walks.Last(); railWest = west.minX > -.61f && west.minX < -.38f && west.minY > mid.y - .15f && Supported(mid.y);
            yield return Push("flight " + level + " east rail push", Vector3.right, .86f);
            var east = report.walks.Last(); railEast = east.maxX < .61f && east.maxX > .38f && east.minY > mid.y - .15f && Supported(mid.y);
            yield return WalkTo("flight " + level + " rail recenter", new Vector3(0, mid.y, 2.25f));
            yield return WalkTo("flight " + level + " physical upper half", new Vector3(0, baseY + 3.2f, 5.35f));
        }

        bool GeometryContract()
        {
            bool valid = true;
            foreach (int level in new[] { -1, 0 })
            {
                var root = GameObject.Find("Stair flight " + level);
                if (!root) { report.geometry.Add("missing flight " + level); valid = false; continue; }
                var boxes = root.GetComponentsInChildren<BoxCollider>(true);
                bool complete = root.activeInHierarchy && boxes.Count(c => c.name.StartsWith("Step ", StringComparison.Ordinal)) == 16 &&
                    boxes.Count(c => c.name.StartsWith("Stair rail west ", StringComparison.Ordinal)) == 16 && boxes.Count(c => c.name.StartsWith("Stair rail east ", StringComparison.Ordinal)) == 16 &&
                    boxes.Count(c => c.name == "Stair opening rail west") == 1 && boxes.Count(c => c.name == "Stair opening rail east") == 1 &&
                    boxes.Count(c => c.name == "Stair opening rail south") == (level == 0 ? 1 : 0) && boxes.All(c => c.enabled && !c.isTrigger && c.gameObject.activeInHierarchy);
                report.geometry.Add("flight " + level + ": active=" + root.activeInHierarchy + "; boxes=" + boxes.Length + "; complete=" + complete);
                valid &= complete;
            }
            return valid;
        }
        bool ControllerContract() { return body && body.enabled && Mathf.Abs(body.radius - .25f) < .001f && Mathf.Abs(body.stepOffset - .25f) < .001f && Mathf.Abs(body.height - 1.78f) < .001f && Mathf.Abs(body.slopeLimit - 48) < .001f && Mathf.Abs(body.skinWidth - .025f) < .001f; }
        bool Supported(float y) { return body.enabled && w.IsGrounded && Mathf.Abs(w.PlayerPosition.y - y) < .16f; }
        bool LastReached(float y, float z) { return report.walks.Last().reached && Supported(y) && Mathf.Abs(w.PlayerPosition.z - z) < .16f && Mathf.Abs(w.PlayerPosition.x) < .16f; }

        IEnumerator WalkTo(string id, Vector3 target)
        {
            var walk = NewWalk(id, target); float deadline = Time.realtimeSinceStartup + 4.7f;
            while (Time.realtimeSinceStartup < deadline && walk.frames < 275)
            {
                Vector3 delta = target - w.PlayerPosition; delta.y = 0;
                if (delta.magnitude < .035f) { walk.reached = true; break; }
                Step(walk, Vector3.ClampMagnitude(delta / .8f, 1)); yield return null;
            }
            yield return Settle(10); EndWalk(walk);
        }
        IEnumerator Push(string id, Vector3 direction, float seconds)
        {
            var walk = NewWalk(id, w.PlayerPosition + direction * 2.7f * seconds);
            float deadline = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < deadline) { Step(walk, direction); yield return null; }
            yield return Settle(5); EndWalk(walk);
        }
        Walk NewWalk(string id, Vector3 target)
        {
            var walk = new Walk { id = id, start = w.PlayerPosition, target = target, seconds = Time.realtimeSinceStartup };
            Sample(walk); report.walks.Add(walk); return walk;
        }
        void Step(Walk walk, Vector3 direction)
        {
            Vector3 forward = w.View.transform.forward; forward.y = 0; forward.Normalize();
            Vector3 right = w.View.transform.right; right.y = 0; right.Normalize();
            w.SmokeMove(new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)), MoveDt); walk.frames++; Sample(walk);
            if (lockedHinges != null)
            {
                var now = Hinges();
                movingDoorStable &= OtherHingesUnchanged(lockedHinges, now, movingDoor) && now[movingDoor].open &&
                    Mathf.Abs(now[movingDoor].angle - lockedHinges[movingDoor].angle) < .01f && Quaternion.Angle(now[movingDoor].rotation, lockedHinges[movingDoor].rotation) < 3.5f;
            }
        }
        void Sample(Walk walk)
        {
            Vector3 p = w.PlayerPosition; walk.minX = Mathf.Min(walk.minX, p.x); walk.maxX = Mathf.Max(walk.maxX, p.x); walk.minY = Mathf.Min(walk.minY, p.y); walk.maxY = Mathf.Max(walk.maxY, p.y);
            if (walk.frames % 6 == 0) walk.samples.Add(p);
        }
        void EndWalk(Walk walk)
        {
            walk.end = w.PlayerPosition; walk.seconds = Time.realtimeSinceStartup - walk.seconds; Sample(walk); walk.samples.Add(walk.end);
            Vector3 delta = walk.target - walk.end; delta.y = 0; walk.reached &= delta.magnitude < .16f;
        }
        static string WalkDetail(Walk walk) { return walk.id + ": " + walk.start + " -> " + walk.end + "; reached=" + walk.reached + "; x=" + walk.minX.ToString("F2") + ".." + walk.maxX.ToString("F2") + "; y=" + walk.minY.ToString("F2") + ".." + walk.maxY.ToString("F2"); }
        IEnumerator Settle(int count) { for (int i = 0; i < count; i++) { w.SmokeMove(Vector2.zero, MoveDt); yield return null; } }
        IEnumerator SettleFor(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) { w.SmokeMove(Vector2.zero, MoveDt); yield return null; } }
        string FirstObstacle(Vector3 target, string id)
        {
            Vector3 start = w.PlayerPosition + Vector3.up * .9f, delta = target + Vector3.up * .8f - start;
            var hits = Physics.RaycastAll(start, delta.normalized, delta.magnitude, ~(1 << 30), QueryTriggerInteraction.Ignore).OrderBy(h => h.distance);
            foreach (var hit in hits)
            {
                if (hit.collider == body || hit.transform.IsChildOf(body.transform) || hit.collider.GetComponentInParent<ResidenceLootPick>()) continue;
                var pick = hit.collider.GetComponentInParent<HomePick>(); return pick ? pick.Id : "obstacle:" + hit.collider.name;
            }
            return "none";
        }
        Dictionary<string, HingeState> Hinges()
        {
            var result = new Dictionary<string, HingeState>(); var map = hingeField.GetValue(w) as IDictionary;
            if (map == null) throw new InvalidOperationException("Private production hinge map unavailable.");
            foreach (DictionaryEntry pair in map)
            {
                object value = pair.Value; Type type = value.GetType(); var leaf = (Transform)type.GetField("leaf").GetValue(value);
                string id = (string)pair.Key; var source = w.Expedition.Sources.Find(s => s.Id == id);
                result[id] = new HingeState { id = id, leaf = leaf, frame = (Transform)type.GetField("frame").GetValue(value),
                    closed = (Quaternion)type.GetField("closed").GetValue(value), position = (Vector3)type.GetField("closedPosition").GetValue(value), rotation = leaf.localRotation,
                    open = source != null ? source.Open : (bool)type.GetField("open").GetValue(value), directional = (bool)type.GetField("directionalDoor").GetValue(value),
                    lid = (bool)type.GetField("lid").GetValue(value), drawer = (bool)type.GetField("drawer").GetValue(value), angle = (float)type.GetField("openAngle").GetValue(value) };
            }
            return result;
        }
        static float SignedAngle(HingeState h) { return Mathf.DeltaAngle(h.closed.eulerAngles.y, h.rotation.eulerAngles.y); }
        static bool OtherHingesUnchanged(Dictionary<string, HingeState> before, Dictionary<string, HingeState> after, string exception)
        {
            return before.Count == after.Count && before.All(p => p.Key == exception || after.TryGetValue(p.Key, out var next) && p.Value.leaf == next.leaf && p.Value.open == next.open &&
                p.Value.directional == next.directional && (!p.Value.directional ? Mathf.Abs(p.Value.angle - next.angle) < .01f : true) && Quaternion.Angle(p.Value.rotation, next.rotation) < 3.5f);
        }
        IEnumerator Shot(string id)
        {
            yield return new WaitForEndOfFrame(); string path = Path.Combine(w.EvidencePath, id + ".png"); ScreenCapture.CaptureScreenshot(path); report.screenshots.Add(path);
            float deadline = Time.realtimeSinceStartup + 1; long previousLength = -1; int stable = 0;
            while (Time.realtimeSinceStartup < deadline)
            {
                long length = File.Exists(path) ? new FileInfo(path).Length : 0; stable = length >= 256 && length == previousLength ? stable + 1 : 0;
                if (stable >= 2) yield break; previousLength = length; yield return null;
            }
            report.errors.Add("Screenshot did not finish writing within 1s: " + path);
        }
        void CheckIt(string name, bool passed, string detail) { report.checks.Add(new Check { name = name, passed = passed, utc = DateTime.UtcNow.ToString("o"), detail = detail }); Debug.Log("DOORS_STAIRS_CHECK " + (passed ? "PASS " : "FAIL ") + name + " " + detail); }
        void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Update()
        {
            if (finished || !w) return;
            if (Time.realtimeSinceStartup - started > HardCapSeconds) { report.errors.Add("Doors/stairs smoke exceeded its " + HardCapSeconds + " second hard cap; remaining checks did not execute."); if (running != null) StopCoroutine(running); Finish(); }
        }
        void Finish()
        {
            if (finished) return; finished = true; report.finishedUtc = DateTime.UtcNow.ToString("o"); report.runtimeSeconds = Time.realtimeSinceStartup - started;
            foreach (string path in report.screenshots) if (!File.Exists(path) || new FileInfo(path).Length < 256) report.errors.Add("Missing/empty actual render: " + path);
            report.passed = report.completed && report.errors.Count == 0 && report.checks.All(c => c.passed) && report.screenshots.Count == 2;
            Application.logMessageReceived -= Log;
            try { File.WriteAllText(Path.Combine(w.EvidencePath, "native-doors-stairs-smoke.json"), JsonUtility.ToJson(report, true)); }
            catch (Exception ex) { Debug.LogError("DOORS_STAIRS_SMOKE report write failed: " + ex); }
            Debug.Log("DOORS_STAIRS_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " errors=" + report.errors.Count + " seconds=" + report.runtimeSeconds.ToString("F1"));
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}
