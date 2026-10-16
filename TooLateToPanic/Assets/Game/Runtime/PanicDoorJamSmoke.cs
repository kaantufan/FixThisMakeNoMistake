using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    // Root attaches only after Awake establishes the isolated score path and Model.
    // All locomotion after a counted independent fixture uses the production controller.
    public sealed class PanicDoorJamSmoke : MonoBehaviour
    {
        [Serializable] sealed class Check { public string name, detail; public bool passed; }
        [Serializable] sealed class Walk
        {
            public string id; public Vector3 start, target, end;
            public bool reached; public int frames; public float seconds, maxLeafPenetration;
        }
        [Serializable] sealed class DoorCase
        {
            public string id, side; public float offset, expectedAngle, openedAngle, maxOpenDrift, maxOpeningPenetration, setupClosedAngle, resetClosedAngle;
            public Vector3 setup, approach, afterOpen, across, returned;
            public bool setupClear, approachReached, eAccepted, openedAway, noPush, solidLeaf, crossed, walkedBack, resetClosed;
            public int movementWarps; public List<Vector3> openingActorSamples = new List<Vector3>();
            public List<float> openingAngles = new List<float>();
        }
        [Serializable] sealed class ReopenCase
        {
            public string id, initialSide; public float chosenBranch, blockedAngle, reopenedAngle, maxActorDrift, maxPenetration, blockedAngleRange, setupClosedAngle, resumedClosedAngle;
            public Vector3 arcPosition, afterHold, afterReopen, egressPosition;
            public bool reachedArc, closeBlocked, leafStopped, logicalClosePending, noPush, branchPreserved, reopened, reopenedPassage, egress, resumedWithoutE;
            public int blockedFrames, movementWarps;
            public List<Vector3> stationarySamples = new List<Vector3>(); public List<float> stationaryAngles = new List<float>();
        }
        [Serializable] sealed class Report
        {
            public string startedUtc, finishedUtc, profileId, scorePath;
            public string evidenceType = "Focused native semantic E input; production CharacterController navigation; enabled independent exact-size standard CapsuleCollider penetration query against actual rendered/solid door leaves.";
            public string limitations = "Automatic semantic input, not physical OS keyboard/manual acceptance. InputLocked isolates unscripted input and the model clock as in prior native harnesses; no production timer, cost, controller, collision or hinge state is changed. StartGame's built-in warp and each independent setup warp are counted. No warp is allowed between approach/open/cross/return, or between blocked close/reopen/egress. Bathroom fixture uses a counted upper-floor setup; no stairs/campaign/FPS/save compatibility acceptance.";
            public bool completed, passed; public float runtimeSeconds, modelTimeBefore, modelTimeAfter;
            public int startGameWarps, fixtureWarps, expectedFixtureWarps = 11, unexpectedMovementWarps, budgetBefore, budgetAfter;
            public List<Check> checks = new List<Check>(); public List<DoorCase> doors = new List<DoorCase>();
            public List<ReopenCase> reopenCases = new List<ReopenCase>(); public List<Walk> walks = new List<Walk>();
            public List<string> screenshots = new List<string>(), errors = new List<string>();
        }
        sealed class Hinge
        {
            public Transform leaf, frame; public Quaternion closed, rotation;
            public bool open, closeBlocked, motionBlocked; public float branch;
        }

        const string Flag = "--door-jam-smoke";
        const string ReportName = "native-door-jam-smoke.json";
        const int ExpectedChecks = 9;
        const float HardCapSeconds = 90;
        const float PenetrationLimit = .006f;
        static readonly BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        static float Dt { get { return Mathf.Clamp(Time.unscaledDeltaTime, .008f, .04f); } }
        readonly Report report = new Report();
        PanicHomeWorld w; CharacterController body; CapsuleCollider actorQuery; FieldInfo hinges;
        Coroutine running; float started; bool finished; string watchedDoor;

        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), Flag) < 0) { enabled = false; return; }
            w = GetComponent<PanicHomeWorld>();
            if (!w || string.IsNullOrEmpty(w.EvidencePath)) { Debug.LogError("DOOR_JAM_SMOKE requires --home-evidence before Model construction."); enabled = false; return; }
            started = Time.realtimeSinceStartup; report.startedUtc = DateTime.UtcNow.ToString("o");
            report.profileId = Path.GetFileName(Path.GetFullPath(w.EvidencePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            report.scorePath = PanicModel.ScorePathOverride;
            Application.logMessageReceived += Log; Application.runInBackground = true; w.InputLocked = true;
            hinges = typeof(PanicHomeWorld).GetField("hinges", PrivateInstance);
            body = FindObjectsByType<CharacterController>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Survivor");
            if (body)
            {
                var query = new GameObject("Door jam independent actor capsule query"); query.layer = 2;
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
            string score = Path.GetFullPath(Path.Combine(w.EvidencePath, "isolated-home-scores.json")); Guid guid;
            bool isolated = report.profileId.Length >= 36 && Guid.TryParse(report.profileId.Substring(report.profileId.Length - 36), out guid) &&
                !string.IsNullOrEmpty(report.scorePath) && string.Equals(score, Path.GetFullPath(report.scorePath), StringComparison.OrdinalIgnoreCase) &&
                !File.Exists(score) && !File.Exists(Path.Combine(w.EvidencePath, ReportName)) && w.Model.Highscores.Count == 0;
            CheckIt("fresh GUID profile and score override before StartGame", isolated, report.profileId + "; score=" + report.scorePath);
            if (!isolated) yield break;
            if (!body || !actorQuery || hinges == null) throw new InvalidOperationException("Missing actual controller, hinge map or enabled independent standard capsule.");
            yield return Settle(.1f); w.StartGame(false); report.startGameWarps++; yield return Settle(.18f);
            report.budgetBefore = w.Model.Budget; report.modelTimeBefore = w.Model.TimeRemaining;
            var entry = w.Model.Items.Single(i => i.Id == "door"); var bath = w.Model.Items.Single(i => i.Id == "bathroom_door");
            bool geometry = Vector3.Distance(entry.Position, new Vector3(0, 0, -6)) < .001f &&
                Vector3.Distance(bath.Position, new Vector3(3.25f, 3.2f, 1.8f)) < .001f && Mathf.Abs(bath.Rotation) < .001f;
            CheckIt("current front and south bathroom fixture geometry with unchanged production controller", geometry && ControllerContract(),
                "entry=" + entry.Position + "; bath=" + bath.Position + "; radius=" + body.radius + "; step=" + body.stepOffset + "; skin=" + body.skinWidth);

            foreach (int side in new[] { 1, -1 })
                foreach (float offset in new[] { -.65f, -.35f, .35f, .65f })
                    yield return DoorRoute("door", side, offset, side == 1 && offset == .65f);
            var front = report.doors.Where(p => p.id == "door").ToArray();
            CheckIt("eight real off-center approaches accept E and open away without actor push or penetration", front.Length == 8 && front.All(p => p.setupClear && p.approachReached && p.eAccepted && p.openedAway && p.noPush && p.solidLeaf && p.maxOpeningPenetration < PenetrationLimit), DoorDetails(front));
            CheckIt("eight physical diagonal crossings and returns stay clear with zero movement warps", front.Length == 8 && front.All(p => p.crossed && p.walkedBack && p.resetClosed && p.movementWarps == 0) && report.walks.Where(p => p.id.StartsWith("door route ", StringComparison.Ordinal)).All(p => p.reached && p.maxLeafPenetration < PenetrationLimit), DoorDetails(front));

            yield return ReopenRegression(1, true);
            yield return ReopenRegression(-1, false);
            CheckIt("front close pauses on both branches without pushing or penetrating actor", report.reopenCases.Count == 2 && report.reopenCases.All(p => p.reachedArc && p.closeBlocked && p.leafStopped && p.logicalClosePending && p.noPush && p.maxPenetration < PenetrationLimit), ReopenDetails());
            CheckIt("opposite-side E reopen preserves the branch and permits real passage", report.reopenCases.Count == 2 && report.reopenCases.All(p => p.branchPreserved && p.reopened && p.reopenedPassage && p.noPush && p.maxPenetration < PenetrationLimit), ReopenDetails());
            CheckIt("real egress clears the arc and pending close resumes without another E", report.reopenCases.Count == 2 && report.reopenCases.All(p => p.egress && p.resumedWithoutE && p.movementWarps == 0), ReopenDetails());

            yield return DoorRoute("bathroom_door", 1, -.35f, true);
            var bathroom = report.doors.Single(p => p.id == "bathroom_door");
            CheckIt("south bathroom near-leaf E opening and physical diagonal return remain safe", bathroom.setupClear && bathroom.approachReached && bathroom.eAccepted && bathroom.openedAway && bathroom.noPush && bathroom.solidLeaf && bathroom.maxOpeningPenetration < PenetrationLimit && bathroom.crossed && bathroom.walkedBack && bathroom.resetClosed && bathroom.movementWarps == 0 && report.walks.Where(p => p.id.StartsWith("bathroom_door route ", StringComparison.Ordinal)).All(p => p.reached && p.maxLeafPenetration < PenetrationLimit), DoorDetails(new[] { bathroom }));
            report.budgetAfter = w.Model.Budget; report.modelTimeAfter = w.Model.TimeRemaining;
            CheckIt("all setup warps accounted for and production controller, clock and budget unchanged", report.startGameWarps == 1 && report.fixtureWarps == report.expectedFixtureWarps && report.unexpectedMovementWarps == 0 && ControllerContract() && report.budgetBefore == report.budgetAfter && report.modelTimeBefore == report.modelTimeAfter,
                "StartGame=" + report.startGameWarps + "; independent fixtures=" + report.fixtureWarps + "; unexpected movement=" + report.unexpectedMovementWarps + "; budget=" + report.budgetBefore + "/" + report.budgetAfter + "; isolated clock=" + report.modelTimeBefore + "/" + report.modelTimeAfter);
            report.completed = true;
        }

        IEnumerator DoorRoute(string id, int side, float offset, bool capture)
        {
            Hinge h = ReadHinge(id); Vector3 origin = h.frame.position; Vector3 normal = Normal(h), right = Right(h);
            var p = new DoorCase { id = id, side = side > 0 ? "positive / inside" : "negative / outside", offset = offset, expectedAngle = side * 100 };
            report.doors.Add(p); watchedDoor = id;
            Fixture(origin + normal * side * 1.45f + Vector3.up * .08f); yield return Settle(.15f); p.setup = w.PlayerPosition;
            yield return CloseAtClearSetup(id); p.setupClosedAngle = ClosedAngle(id);
            p.setupClear = Supported(origin.y) && LeafOverlap(ReadHinge(id)) < PenetrationLimit && Closed(id);
            int fixturesBefore = report.fixtureWarps;
            string tag = id + " route " + side + "/" + offset.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            Vector3 approach = origin + right * offset + normal * side * .70f;
            yield return WalkTo(tag + " off-center approach", approach); p.approachReached = report.walks.Last().reached; p.approach = w.PlayerPosition;
            w.SetFocus(id); p.eAccepted = w.CanInteract && !ReadHinge(id).open; w.HandleKey(KeyCode.E);
            Vector3 actor = w.PlayerPosition; float until = Time.realtimeSinceStartup + .58f; int sample = 0;
            while (Time.realtimeSinceStartup < until)
            {
                w.SmokeMove(Vector2.zero, Dt); Hinge state = ReadHinge(id);
                p.maxOpenDrift = Mathf.Max(p.maxOpenDrift, Vector3.Distance(actor, w.PlayerPosition));
                p.maxOpeningPenetration = Mathf.Max(p.maxOpeningPenetration, LeafOverlap(state));
                if (sample++ % 4 == 0) { p.openingActorSamples.Add(w.PlayerPosition); p.openingAngles.Add(Angle(state)); }
                yield return null;
            }
            Hinge open = ReadHinge(id); p.openedAngle = Angle(open); p.afterOpen = w.PlayerPosition;
            p.openedAway = p.eAccepted && open.open && Mathf.Abs(Mathf.DeltaAngle(p.expectedAngle, p.openedAngle)) < 3.5f;
            p.noPush = p.maxOpenDrift < .025f && Supported(origin.y); p.solidLeaf = SolidLeaf(open);
            if (capture) yield return Shot(id == "door" ? "front-positive-x-open-actual-render" : "bathroom-near-leaf-open-actual-render");
            // This segment crosses the physical aperture diagonally, rather than warping or disabling collision.
            Vector3 across = origin - right * Mathf.Sign(offset) * .15f - normal * side * .90f;
            yield return WalkTo(tag + " diagonal through doorway", across); p.crossed = report.walks.Last().reached; p.across = w.PlayerPosition;
            yield return WalkTo(tag + " physical diagonal return", approach); p.walkedBack = report.walks.Last().reached; p.returned = w.PlayerPosition;
            yield return WalkTo(tag + " step clear before reset", origin + normal * side * 1.45f);
            bool resetReached = report.walks.Last().reached;
            w.SetFocus(id); bool closeAccepted = w.CanInteract && ReadHinge(id).open; if (closeAccepted) w.HandleKey(KeyCode.E);
            yield return WaitForFullyClosed(id); p.resetClosedAngle = ClosedAngle(id); p.resetClosed = resetReached && closeAccepted && Closed(id);
            p.movementWarps = report.fixtureWarps - fixturesBefore; watchedDoor = null;
        }

        IEnumerator ReopenRegression(int initialSide, bool capture)
        {
            const string id = "door"; Hinge h = ReadHinge(id); Vector3 origin = h.frame.position, normal = Normal(h);
            var p = new ReopenCase { id = id, initialSide = initialSide > 0 ? "inside" : "outside", chosenBranch = initialSide * 100 };
            report.reopenCases.Add(p); watchedDoor = id;
            Fixture(origin + normal * initialSide * 1.45f + Vector3.up * .08f); yield return Settle(.15f); yield return CloseAtClearSetup(id); p.setupClosedAngle = ClosedAngle(id);
            int fixturesBefore = report.fixtureWarps;
            yield return WalkTo("reopen " + initialSide + " approach", origin + normal * initialSide * .6f);
            w.SetFocus(id); bool openAccepted = w.CanInteract && !ReadHinge(id).open; w.HandleKey(KeyCode.E); yield return Settle(.55f);
            yield return WalkTo("reopen " + initialSide + " physically enter opposite-side closing arc", origin - normal * initialSide * .55f);
            p.reachedArc = openAccepted && report.walks.Last().reached; yield return Settle(.12f); p.arcPosition = w.PlayerPosition;
            w.SetFocus(id); bool closeAccepted = w.CanInteract && ReadHinge(id).open; w.HandleKey(KeyCode.E);
            float until = Time.realtimeSinceStartup + .7f, minimum = 999, maximum = -999; bool lastBlocked = false, logical = true;
            while (Time.realtimeSinceStartup < until)
            {
                w.SmokeMove(Vector2.zero, Dt); Hinge state = ReadHinge(id); Observe(p, state, p.arcPosition);
                logical &= !state.open; lastBlocked = state.closeBlocked;
                if (state.closeBlocked) { p.blockedFrames++; minimum = Mathf.Min(minimum, Angle(state)); maximum = Mathf.Max(maximum, Angle(state)); p.blockedAngle = Angle(state); }
                yield return null;
            }
            p.afterHold = w.PlayerPosition; p.blockedAngleRange = maximum - minimum;
            p.closeBlocked = closeAccepted && lastBlocked && p.blockedFrames >= 5 && Mathf.Abs(p.blockedAngle) > 12;
            p.leafStopped = p.closeBlocked && p.blockedAngleRange < .25f; p.logicalClosePending = logical;
            if (capture) yield return Shot("front-opposite-side-close-paused-actual-render");
            // Actor is now on the opposite side from the original opening command.
            // Reopening an unfinished close must retain its old branch instead of sweeping across zero.
            w.SetFocus(id); bool reopenAccepted = w.CanInteract && !ReadHinge(id).open; w.HandleKey(KeyCode.E);
            p.branchPreserved = reopenAccepted; until = Time.realtimeSinceStartup + .62f;
            while (Time.realtimeSinceStartup < until)
            {
                w.SmokeMove(Vector2.zero, Dt); Hinge state = ReadHinge(id); Observe(p, state, p.arcPosition);
                p.branchPreserved &= state.open && Mathf.Abs(Mathf.DeltaAngle(p.chosenBranch, state.branch)) < .01f && Mathf.Sign(Angle(state)) == initialSide;
                yield return null;
            }
            Hinge reopened = ReadHinge(id); p.afterReopen = w.PlayerPosition; p.reopenedAngle = Angle(reopened);
            p.reopened = reopened.open && Mathf.Abs(Mathf.DeltaAngle(p.chosenBranch, p.reopenedAngle)) < 3.5f;
            // Walk clear, then physically cross the same aperture after E reopening.
            // These production-controller segments distinguish escape from a stationary angle-only pass.
            yield return WalkTo("reopen " + initialSide + " physically clear reopened leaf", origin - normal * initialSide * 1.25f);
            p.reopenedPassage = report.walks.Last().reached; p.maxPenetration = Mathf.Max(p.maxPenetration, report.walks.Last().maxLeafPenetration);
            yield return WalkTo("reopen " + initialSide + " physical passage after opposite-side E", origin + normal * initialSide * .9f);
            p.reopenedPassage &= report.walks.Last().reached; p.maxPenetration = Mathf.Max(p.maxPenetration, report.walks.Last().maxLeafPenetration);
            yield return WalkTo("reopen " + initialSide + " physically return to closing arc", origin - normal * initialSide * .55f);
            p.reopenedPassage &= report.walks.Last().reached; p.maxPenetration = Mathf.Max(p.maxPenetration, report.walks.Last().maxLeafPenetration);
            // Retain the close/no-push/stop/automatic-resume regression after the reopen fix.
            Vector3 secondStationary = w.PlayerPosition;
            w.SetFocus(id); bool secondCloseAccepted = w.CanInteract; w.HandleKey(KeyCode.E); until = Time.realtimeSinceStartup + .5f;
            while (Time.realtimeSinceStartup < until) { w.SmokeMove(Vector2.zero, Dt); Observe(p, ReadHinge(id), secondStationary); yield return null; }
            bool blockedAgain = !ReadHinge(id).open && ReadHinge(id).closeBlocked;
            p.noPush = p.maxActorDrift < .025f && Supported(origin.y);
            yield return WalkTo("reopen " + initialSide + " physically egress from paused arc", origin - normal * initialSide * 1.45f);
            p.egressPosition = w.PlayerPosition; p.egress = report.walks.Last().reached;
            p.maxPenetration = Mathf.Max(p.maxPenetration, report.walks.Last().maxLeafPenetration); yield return WaitForFullyClosed(id); p.resumedClosedAngle = ClosedAngle(id);
            p.resumedWithoutE = secondCloseAccepted && blockedAgain && p.egress && Closed(id) && Supported(origin.y);
            p.movementWarps = report.fixtureWarps - fixturesBefore; watchedDoor = null;
        }

        void Observe(ReopenCase p, Hinge h, Vector3 stationary)
        {
            p.maxActorDrift = Mathf.Max(p.maxActorDrift, Vector3.Distance(stationary, w.PlayerPosition));
            p.maxPenetration = Mathf.Max(p.maxPenetration, LeafOverlap(h));
            if (Time.frameCount % 4 == 0) { p.stationarySamples.Add(w.PlayerPosition); p.stationaryAngles.Add(Angle(h)); }
        }
        IEnumerator CloseAtClearSetup(string id)
        {
            w.SetFocus(id); if (ReadHinge(id).open) { if (!w.CanInteract) throw new InvalidOperationException("Fixture cannot reach its open door to close through E: " + id); w.HandleKey(KeyCode.E); }
            // Logical close alone does not reset production's opening branch. Wait for
            // the real leaf to enter the same <0.5 degree fully-closed tolerance that
            // ToggleDoor requires before choosing a new side, including pending closes.
            yield return WaitForFullyClosed(id);
            if (!Closed(id)) throw new InvalidOperationException("Independent door fixture did not physically close before branch selection: " + id + "; angle=" + ClosedAngle(id).ToString("F4"));
        }
        IEnumerator WaitForFullyClosed(string id)
        {
            float deadline = Time.realtimeSinceStartup + 1.3f;
            while (Time.realtimeSinceStartup < deadline && !Closed(id)) { w.SmokeMove(Vector2.zero, Dt); yield return null; }
        }
        void Fixture(Vector3 p) { w.Warp(p); report.fixtureWarps++; Physics.SyncTransforms(); }
        IEnumerator WalkTo(string id, Vector3 target)
        {
            var p = new Walk { id = id, start = w.PlayerPosition, target = target, seconds = Time.realtimeSinceStartup };
            report.walks.Add(p); float deadline = Time.realtimeSinceStartup + 2.5f;
            while (Time.realtimeSinceStartup < deadline)
            {
                Vector3 delta = target - w.PlayerPosition; delta.y = 0;
                if (delta.magnitude < .035f) { p.reached = true; break; }
                Vector3 direction = Vector3.ClampMagnitude(delta / .35f, 1), before = w.PlayerPosition;
                Vector3 forward = w.View.transform.forward; forward.y = 0; forward.Normalize();
                Vector3 right = w.View.transform.right; right.y = 0; right.Normalize();
                w.SmokeMove(new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)), Dt);
                if (Vector3.Distance(before, w.PlayerPosition) > .35f) report.unexpectedMovementWarps++;
                if (watchedDoor != null) p.maxLeafPenetration = Mathf.Max(p.maxLeafPenetration, LeafOverlap(ReadHinge(watchedDoor)));
                p.frames++; yield return null;
            }
            yield return Settle(.10f); p.end = w.PlayerPosition; p.seconds = Time.realtimeSinceStartup - p.seconds;
            Vector3 error = p.end - target; error.y = 0; p.reached &= error.magnitude < .13f;
        }
        IEnumerator Settle(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) { w.SmokeMove(Vector2.zero, Dt); yield return null; }
        }
        Hinge ReadHinge(string id)
        {
            object value = ((IDictionary)hinges.GetValue(w))[id]; if (value == null) throw new InvalidOperationException("Missing production hinge " + id);
            Type type = value.GetType(); var leaf = (Transform)type.GetField("leaf").GetValue(value); var motion = type.GetField("motionBlocked");
            return new Hinge { leaf = leaf, frame = (Transform)type.GetField("frame").GetValue(value), closed = (Quaternion)type.GetField("closed").GetValue(value), rotation = leaf.localRotation,
                open = (bool)type.GetField("open").GetValue(value), closeBlocked = (bool)type.GetField("closeBlocked").GetValue(value), motionBlocked = motion != null && (bool)motion.GetValue(value), branch = (float)type.GetField("openAngle").GetValue(value) };
        }
        static Vector3 Normal(Hinge h) { return h.frame.TransformDirection(h.closed * Vector3.forward).normalized; }
        static Vector3 Right(Hinge h) { return h.frame.TransformDirection(h.closed * Vector3.right).normalized; }
        static float Angle(Hinge h) { return Mathf.DeltaAngle(h.closed.eulerAngles.y, h.rotation.eulerAngles.y); }
        float ClosedAngle(string id) { Hinge h = ReadHinge(id); return Quaternion.Angle(h.closed, h.rotation); }
        bool Closed(string id) { Hinge h = ReadHinge(id); return !h.open && !h.closeBlocked && Quaternion.Angle(h.closed, h.rotation) < .5f; }
        static bool SolidLeaf(Hinge h) { return h.leaf.GetComponentsInChildren<Collider>().Any(c => c.enabled && !c.isTrigger && c.gameObject.activeInHierarchy); }
        float LeafOverlap(Hinge h)
        {
            Physics.SyncTransforms(); float maximum = 0;
            foreach (var c in h.leaf.GetComponentsInChildren<Collider>())
            {
                if (!c.enabled || c.isTrigger || !c.gameObject.activeInHierarchy) continue;
                Vector3 direction; float distance;
                if (Physics.ComputePenetration(c, c.transform.position, c.transform.rotation, actorQuery, body.transform.position, body.transform.rotation, out direction, out distance)) maximum = Mathf.Max(maximum, distance);
            }
            return maximum;
        }
        bool ControllerContract()
        {
            return body.enabled && Mathf.Abs(body.radius - .25f) < .001f && Mathf.Abs(body.height - 1.78f) < .001f && Vector3.Distance(body.center, new Vector3(0, .9f, 0)) < .001f &&
                Mathf.Abs(body.stepOffset - .25f) < .001f && Mathf.Abs(body.skinWidth - .025f) < .001f && Mathf.Abs(body.slopeLimit - 48) < .001f && actorQuery.enabled && actorQuery.isTrigger && actorQuery.direction == 1 &&
                Vector3.Distance(actorQuery.center, body.center) < .001f && Mathf.Abs(actorQuery.radius - body.radius) < .001f && Mathf.Abs(actorQuery.height - body.height) < .001f;
        }
        bool Supported(float y) { return body.enabled && w.IsGrounded && Mathf.Abs(w.PlayerPosition.y - y) < .16f; }
        static string DoorDetails(IEnumerable<DoorCase> cases)
        {
            return string.Join("; ", cases.Select(p => p.id + "/" + p.side + "/x=" + p.offset.ToString("F2") + ":setupClosed=" + p.setupClosedAngle.ToString("F3") + ",resetClosed=" + p.resetClosedAngle.ToString("F3") + ",approach=" + p.approachReached + ",E=" + p.eAccepted + ",angle=" + p.openedAngle.ToString("F1") + ",drift=" + p.maxOpenDrift.ToString("F4") + ",overlap=" + p.maxOpeningPenetration.ToString("F4") + ",cross=" + p.crossed + ",return=" + p.walkedBack).ToArray());
        }
        string ReopenDetails()
        {
            return string.Join("; ", report.reopenCases.Select(p => p.initialSide + ":setupClosed=" + p.setupClosedAngle.ToString("F3") + ",resumedClosed=" + p.resumedClosedAngle.ToString("F3") + ",blocked=" + p.blockedFrames + ",range=" + p.blockedAngleRange.ToString("F2") + ",branch=" + p.chosenBranch.ToString("F1") + ",reopened=" + p.reopenedAngle.ToString("F1") + ",passage=" + p.reopenedPassage + ",drift=" + p.maxActorDrift.ToString("F4") + ",overlap=" + p.maxPenetration.ToString("F4") + ",egress=" + p.egress + ",resume=" + p.resumedWithoutE).ToArray());
        }
        IEnumerator Shot(string id)
        {
            if (report.screenshots.Count >= 3) throw new InvalidOperationException("Focused doorway harness allows at most three actual PNGs.");
            yield return new WaitForEndOfFrame(); string path = Path.Combine(w.EvidencePath, id + ".png"); ScreenCapture.CaptureScreenshot(path); report.screenshots.Add(path);
            long previous = -1; int stable = 0; float deadline = Time.realtimeSinceStartup + 1.5f;
            while (Time.realtimeSinceStartup < deadline)
            {
                long size = File.Exists(path) ? new FileInfo(path).Length : 0; stable = size >= 256 && size == previous ? stable + 1 : 0;
                if (stable >= 2) yield break; previous = size; yield return null;
            }
            report.errors.Add("Actual screenshot did not finish writing: " + path);
        }
        void CheckIt(string name, bool passed, string detail) { report.checks.Add(new Check { name = name, passed = passed, detail = detail }); Debug.Log("DOOR_JAM_CHECK " + (passed ? "PASS " : "FAIL ") + name + " " + detail); }
        void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Update()
        {
            if (finished || !w || Time.realtimeSinceStartup - started <= HardCapSeconds) return;
            report.errors.Add("Focused doorway smoke exceeded 90 second hard cap; unexecuted checks are not accepted."); if (running != null) StopCoroutine(running); Finish();
        }
        void Finish()
        {
            if (finished) return; finished = true; report.finishedUtc = DateTime.UtcNow.ToString("o"); report.runtimeSeconds = Time.realtimeSinceStartup - started;
            foreach (string p in report.screenshots) if (!File.Exists(p) || new FileInfo(p).Length < 256) report.errors.Add("Missing/empty actual render: " + p);
            report.passed = report.completed && report.errors.Count == 0 && report.checks.Count == ExpectedChecks && report.checks.All(c => c.passed) && report.screenshots.Count == 3;
            if (actorQuery) Destroy(actorQuery.gameObject); Application.logMessageReceived -= Log;
            try { File.WriteAllText(Path.Combine(w.EvidencePath, ReportName), JsonUtility.ToJson(report, true)); }
            catch (Exception ex) { Debug.LogError("DOOR_JAM_SMOKE report write failed: " + ex); }
            Debug.Log("DOOR_JAM_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " errors=" + report.errors.Count + " seconds=" + report.runtimeSeconds.ToString("F1"));
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}
