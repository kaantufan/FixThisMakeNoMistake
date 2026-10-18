using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    // Opt-in only: root attaches this AFTER World Awake has set ScorePathOverride,
    // constructed Model/Expedition, built the scene, and initialized residence life.
    public sealed class PanicBathroomSmoke : MonoBehaviour
    {
        [Serializable] sealed class Check { public string name, utc, detail; public bool passed; }
        [Serializable] sealed class Walk
        {
            public string id; public Vector3 start, target, end;
            public int frames, forwardCrossings, reverseCrossings;
            public bool reached; public float seconds, minY = 999, maxY = -999;
            public List<Vector3> samples = new List<Vector3>();
        }
        [Serializable] sealed class HingeProbe
        {
            public string stage, id; public int leafInstanceId;
            public bool open; public float signedYawFromClosed, angleFromClosed;
        }
        [Serializable] sealed class Report
        {
            public string startedUtc, finishedUtc, profileId, scorePath;
            public string evidenceType = "Native bathroom affected smoke: automatic nearest focus, semantic Q/E/F, actual hinge state and rendered rotation, real CharacterController collision/traversal, live room mesh vertices, and rendered inside/outside screenshots.";
            public string limitations = "API-driven, not manual keyboard/mouse or final aesthetic acceptance. One setup warp places the actor outside the upper bathroom; every bathroom crossing and cabinet approach then uses SmokeMove/CharacterController.Move. Reflection only invokes the existing automatic focus refresh and reads private hinges. No colliders, controller settings, model timers, costs, stock, thresholds or production focus behavior are changed. No campaign or cold-save coverage.";
            public bool passed; public int setupWarps, forwardCrossings, reverseCrossings, cabinetQPresses;
            public float runtimeSeconds, meanFps;
            public List<Check> checks = new List<Check>();
            public List<Walk> walks = new List<Walk>();
            public List<HingeProbe> hinges = new List<HingeProbe>();
            public List<string> focusTrail = new List<string>();
            public List<string> errors = new List<string>();
            public List<string> screenshots = new List<string>();
        }
        sealed class HingeState
        {
            public string id; public bool open; public Transform leaf;
            public Quaternion closed, rotation;
        }

        const int ExpectedChecks = 16;
        const float HardCapSeconds = 43f; // Strictly below the requested 45 second cap.
        const float FloorY = 3.2f;
        static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        PanicHomeWorld w;
        CharacterController body;
        MethodInfo refreshFocus;
        FieldInfo hingeField;
        readonly Report report = new Report();
        Coroutine running;
        float started, frameSeconds;
        int frames;
        bool finished;

        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--bathroom-smoke") < 0) { enabled = false; return; }
            w = GetComponent<PanicHomeWorld>();
            if (!w || string.IsNullOrEmpty(w.EvidencePath)) { Debug.LogError("BATHROOM_SMOKE requires PanicHomeWorld and --home-evidence set before Model construction."); enabled = false; return; }
            started = Time.realtimeSinceStartup;
            report.startedUtc = DateTime.UtcNow.ToString("o");
            report.profileId = Path.GetFileName(Path.GetFullPath(w.EvidencePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            report.scorePath = PanicModel.ScorePathOverride;
            Application.logMessageReceived += Log;
            Application.runInBackground = true;
            w.InputLocked = true;
            refreshFocus = typeof(PanicHomeWorld).GetMethod("RefreshFocus", PrivateInstance);
            hingeField = typeof(PanicHomeWorld).GetField("hinges", PrivateInstance);
            body = FindObjectsByType<CharacterController>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Survivor");
            running = StartCoroutine(Guarded(Run()));
        }

        IEnumerator Guarded(IEnumerator bodyEnumerator)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(bodyEnumerator);
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
            string score = Path.GetFullPath(Path.Combine(w.EvidencePath, "isolated-home-scores.json"));
            bool isolated = !string.IsNullOrEmpty(PanicModel.ScorePathOverride) &&
                string.Equals(Path.GetFullPath(PanicModel.ScorePathOverride), score, StringComparison.OrdinalIgnoreCase) &&
                !File.Exists(score) && w.Model.Highscores.Count == 0;
            CheckIt("fresh isolated score profile refuses an unsafe/reused run", isolated, "profile=" + report.profileId + "; score=" + PanicModel.ScorePathOverride);
            if (!isolated) yield break;
            if (refreshFocus == null || hingeField == null || !body) throw new InvalidOperationException("Missing existing focus refresh, hinge inspection or real Survivor CharacterController API.");
            yield return new WaitForSecondsRealtime(.3f);
            w.StartGame(false);
            yield return null;
            var baseline = Hinges("initial");
            CheckIt("one relocated bathroom door and unchanged ground door", DoorContract(), "bath=" + ItemDetail("bathroom_door") + "; ground=" + ItemDetail("door_inner_ground"));

            var upper = GameObject.Find("House floor 1");
            var ground = GameObject.Find("House floor 0");
            var oldUpper = upper ? upper.GetComponentsInChildren<Transform>(true).Where(t =>
                t.name.IndexOf("east_south", StringComparison.OrdinalIgnoreCase) >= 0 ||
                t.name.IndexOf("east_north", StringComparison.OrdinalIgnoreCase) >= 0 ||
                t.name.IndexOf("door_jamb", StringComparison.OrdinalIgnoreCase) >= 0).Select(t => t.name).ToArray() : new[] { "missing upper floor" };
            bool groundPartition = ground && ground.GetComponentsInChildren<Transform>(true).Any(t => t.name == "solid_east_south") &&
                ground.GetComponentsInChildren<Transform>(true).Any(t => t.name == "solid_east_north") &&
                ground.GetComponentsInChildren<Transform>(true).Any(t => t.name == "door_jamb" || t.name.StartsWith("door_jamb_", StringComparison.Ordinal));
            bool obsoleteDoor = w.Model.Items.Any(i => i.Id == "door_inner_upper") ||
                FindObjectsByType<HomePick>(FindObjectsInactive.Include, FindObjectsSortMode.None).Any(p => p.Id == "door_inner_upper") || baseline.ContainsKey("door_inner_upper");
            CheckIt("obsolete upper partition, jamb and duplicate door removed; ground partition retained", oldUpper.Length == 0 && !obsoleteDoor && groundPartition,
                "upper remnants=" + string.Join(",", oldUpper) + "; obsoleteDoor=" + obsoleteDoor + "; groundPartition=" + groundPartition);

            string geometryDetail;
            CheckIt("bathroom west boundary, south seal and tile mesh include old corridor", RoomContract(out geometryDetail), geometryDetail);
            w.Warp(new Vector3(1.05f, FloorY + .08f, 2.5f)); report.setupWarps++;
            yield return Settle(10);
            AutoFocus();
            CheckIt("automatic entry focus actually chooses the bathroom door", FocusId() == "bathroom_door" && w.CanInteract && w.ActiveFloor == 1 && body.enabled &&
                Mathf.Abs(body.radius - .25f) < .001f && Mathf.Abs(body.stepOffset - .25f) < .001f,
                "focus=" + FocusId() + "; player=" + w.PlayerPosition + "; controller radius=" + body.radius + "; step=" + body.stepOffset);
            yield return new WaitForSecondsRealtime(.45f); yield return Shot("bathroom-outside-closed");

            Vector3 closedStart = w.PlayerPosition;
            yield return Push("closed door east collision", Vector3.right, .65f);
            bool closedBlocked = w.PlayerPosition.x > closedStart.x + .1f && w.PlayerPosition.x < 1.85f && Supported();
            CheckIt("closed leaf physically blocks CharacterController at doorway", closedBlocked, "start=" + closedStart + "; stopped=" + w.PlayerPosition);
            AutoFocus(); w.HandleKey(KeyCode.E); yield return SettleFor(.65f);
            var opened = Hinges("E open");
            w.HandleKey(KeyCode.E); yield return SettleFor(.65f);
            var closed = Hinges("E close");
            bool toggle = baseline.TryGetValue("bathroom_door", out var initialBath) && opened.TryGetValue("bathroom_door", out var openBath) && closed.TryGetValue("bathroom_door", out var closedBath) &&
                !initialBath.open && openBath.open && !closedBath.open && initialBath.leaf == openBath.leaf && initialBath.leaf == closedBath.leaf &&
                Quaternion.Angle(initialBath.closed, openBath.rotation) > 85 && Mathf.DeltaAngle(initialBath.closed.eulerAngles.y, openBath.rotation.eulerAngles.y) < -85 &&
                Quaternion.Angle(initialBath.closed, closedBath.rotation) < 2 && OtherHingesUnchanged(baseline, opened, "bathroom_door") && OtherHingesUnchanged(baseline, closed, "bathroom_door");
            CheckIt("E closed/open/closed moves only the same inward bathroom hinge", toggle, "focus=" + FocusId() + "; all other hinge states/rotations compared");

            w.HandleKey(KeyCode.E); yield return SettleFor(.65f);
            yield return WalkTo("enter through open aperture", new Vector3(3.4f, FloorY, 2.5f));
            var firstEntry = report.walks.Last();
            CheckIt("real CharacterController walks through open doorway into bath", firstEntry.reached && firstEntry.forwardCrossings == 1 && w.PlayerPosition.x > 3.1f && Supported(), WalkDetail(firstEntry));
            yield return new WaitForSecondsRealtime(.35f); yield return Shot("bathroom-inside-open");
            yield return WalkTo("leave through same open aperture", new Vector3(1.1f, FloorY, 2.5f));
            var exit = report.walks.Last();
            CheckIt("real CharacterController walks back through the same doorway", exit.reached && exit.reverseCrossings == 1 && w.PlayerPosition.x < 1.4f && Supported(), WalkDetail(exit));
            yield return new WaitForSecondsRealtime(.35f); yield return Shot("bathroom-outside-open");

            // Re-enter physically. Keep east of the inward leaf tip before walking north.
            yield return WalkTo("re-enter for wall and cabinet", new Vector3(3.4f, FloorY, 2.5f));
            yield return WalkTo("clear open leaf tip", new Vector3(3.4f, FloorY, 3.65f));
            yield return WalkTo("walk former corridor tile", new Vector3(2.9f, FloorY, 3.65f));
            var corridor = report.walks.Last();
            CheckIt("old x2-to-x3 corridor is supported open bathroom floor", corridor.reached && w.PlayerPosition.x > 2.55f && w.PlayerPosition.x < 3.05f && Supported(), WalkDetail(corridor));
            yield return WalkTo("walk west wall north of medicine cabinet", new Vector3(2.9f, FloorY, 5.1f));
            bool reachedWallProbe = report.walks.Last().reached;
            string wallHit = FirstObstacle(new Vector3(2.9f, FloorY + .8f, 5.1f), Vector3.left, 1.3f);
            yield return Push("west wall outside aperture collision", Vector3.left, .6f);
            CheckIt("west wall still physically blocks outside the door aperture", reachedWallProbe && wallHit.IndexOf("bath_west_north", StringComparison.OrdinalIgnoreCase) >= 0 &&
                w.PlayerPosition.x > 2.25f && w.PlayerPosition.x < 2.55f && Mathf.Abs(w.PlayerPosition.z - 5.1f) < .18f && Supported(), "ray first=" + wallHit + "; stopped=" + w.PlayerPosition);

            yield return WalkTo("step away from north wall", new Vector3(2.95f, FloorY, 5.1f));
            yield return WalkTo("physically approach medicine cabinet", new Vector3(2.95f, FloorY, 4.15f));
            var cabinet = w.Expedition.Sources.Find(s => s.Id == "bath_cabinet");
            var cabinetObject = FindObjectsByType<HomePick>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(p => p.Id == "bath_cabinet");
            AutoFocus();
            // Q is the game's normal target-cycle input. No SetFocus/SmokeFocus is used.
            for (int i = 0; i < 12 && FocusId() != "bath_cabinet"; i++) { w.HandleKey(KeyCode.Q); report.cabinetQPresses++; }
            bool cabinetReach = report.walks.Last().reached && cabinet != null && cabinetObject &&
                Vector3.Distance(cabinet.Position, new Vector3(2.24f, FloorY, 4.15f)) < .02f &&
                Vector3.Distance(cabinetObject.transform.position, cabinet.Position + Vector3.up * 1.08f) < .02f &&
                Mathf.Abs(Mathf.DeltaAngle(cabinetObject.transform.eulerAngles.y, 270)) < .1f && FocusId() == "bath_cabinet" && w.CanInteract;
            CheckIt("relocated medicine cabinet is physically reachable through normal targeting", cabinetReach, "focus=" + FocusId() + "; Q presses=" + report.cabinetQPresses + "; source=" + (cabinet == null ? "missing" : cabinet.Position.ToString()) + "; player=" + w.PlayerPosition);
            if (cabinet == null) throw new InvalidOperationException("Missing bathroom medicine source.");
            string inventoryBefore = Slots(w.Expedition.Inventory), contentsBefore = Slots(cabinet.Contents);
            var beforeCabinet = Hinges("before cabinet E");
            w.HandleKey(KeyCode.E); yield return SettleFor(.5f); AutoFocus();
            var afterCabinetOpen = Hinges("cabinet E open");
            bool cabinetOpened = cabinet.Open && w.OpenSource == cabinet && w.CanInteract && FocusId() == "bath_cabinet" &&
                inventoryBefore == Slots(w.Expedition.Inventory) && contentsBefore == Slots(cabinet.Contents) && OtherHingesUnchanged(beforeCabinet, afterCabinetOpen, "bath_cabinet");
            CheckIt("cabinet E opens without gathering or stealing bathroom door focus", cabinetOpened, "focus=" + FocusId() + "; open=" + cabinet.Open + "; visible=" + w.LootVisible);
            int loot = cabinet.Contents.FindIndex(s => s.Count > 0 && !string.IsNullOrEmpty(s.Id));
            if (loot < 0) throw new InvalidOperationException("Fresh cabinet has no actual supplies.");
            w.SelectLoot(loot); string lootId = cabinet.Contents[loot].Id; int stock = cabinet.Contents[loot].Count, held = w.Expedition.Count(lootId);
            w.HandleKey(KeyCode.F); yield return SettleFor(.35f); AutoFocus();
            var afterTake = Hinges("cabinet F take");
            CheckIt("cabinet F takes exact selected stack while the same bathroom hinge stays open", FocusId() == "bath_cabinet" && cabinet.Open && w.Expedition.Count(lootId) == held + stock &&
                cabinet.Contents[loot].Count == 0 && w.GatherItemId == lootId && w.GatherCount == stock && OtherHingesUnchanged(beforeCabinet, afterTake, "bath_cabinet"),
                "focus=" + FocusId() + "; loot=" + lootId + "; amount=" + stock + "; held=" + w.Expedition.Count(lootId));
            w.HandleKey(KeyCode.E); yield return SettleFor(.45f); AutoFocus();
            var afterCabinetClose = Hinges("cabinet E close");
            bool cabClosed = !cabinet.Open && FocusId() == "bath_cabinet" && OtherHingesUnchanged(beforeCabinet, afterCabinetClose, "bath_cabinet") &&
                afterCabinetClose.TryGetValue("bath_cabinet", out var shutCabinet) && beforeCabinet.TryGetValue("bath_cabinet", out var originalCabinet) &&
                shutCabinet.leaf == originalCabinet.leaf && Quaternion.Angle(shutCabinet.rotation, originalCabinet.closed) < 3;
            CheckIt("cabinet E closes its own leaf and preserves the bathroom door", cabClosed, "focus=" + FocusId() + "; cabinet open=" + cabinet.Open);
            yield return new WaitForSecondsRealtime(.25f); yield return Shot("bathroom-inside-cabinet");
            CheckIt("one setup warp and all doorway crossings remain grounded", report.setupWarps == 1 && report.forwardCrossings >= 2 && report.reverseCrossings >= 1 &&
                report.walks.All(t => t.minY > FloorY - .12f && t.maxY < FloorY + .18f) && w.Model.Phase == PanicPhase.Preparing && w.Model.Score == 0,
                "warps=" + report.setupWarps + "; crossings east=" + report.forwardCrossings + ", west=" + report.reverseCrossings + "; phase=" + w.Model.Phase);
        }

        bool DoorContract()
        {
            var bathroom = w.Model.Items.Where(i => i.Id == "bathroom_door").ToArray();
            var ground = w.Model.Items.Where(i => i.Id == "door_inner_ground").ToArray();
            return bathroom.Length == 1 && bathroom[0].Kind == ItemKind.Door && Vector3.Distance(bathroom[0].Position, new Vector3(2, FloorY, 2.5f)) < .01f &&
                Mathf.Abs(bathroom[0].Size.x - 1.1f) < .01f && Mathf.Abs(Mathf.DeltaAngle(bathroom[0].Rotation, 90)) < .1f &&
                ground.Length == 1 && Vector3.Distance(ground[0].Position, new Vector3(2, 0, 1)) < .01f && Mathf.Abs(Mathf.DeltaAngle(ground[0].Rotation, 90)) < .1f;
        }
        string ItemDetail(string id) { var i = w.Model.Items.Find(x => x.Id == id); return i == null ? "missing" : i.Position + "/yaw=" + i.Rotation + "/size=" + i.Size; }
        bool RoomContract(out string detail)
        {
            var room = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name == "HomeV4_Bathroom");
            int tileVertices = 0;
            if (room) foreach (var mf in room.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!mf.sharedMesh || mf.name.IndexOf("BathTiles", StringComparison.OrdinalIgnoreCase) < 0) continue;
                foreach (Vector3 local in mf.sharedMesh.vertices)
                {
                    Vector3 p = mf.transform.TransformPoint(local);
                    if (p.x > 2.1f && p.x < 2.99f && p.z > 2 && p.z < 5.9f && p.y > FloorY + .005f && p.y < FloorY + .045f) tileVertices++;
                }
            }
            var north = room ? room.GetComponentsInChildren<BoxCollider>(true).FirstOrDefault(c => c.name == "Collision_solid_bath_west_north") : null;
            var south = room ? room.GetComponentsInChildren<BoxCollider>(true).FirstOrDefault(c => c.name == "Collision_solid_bath_south") : null;
            detail = "corridor tile vertices=" + tileVertices + "; west=" + (north ? north.bounds.ToString() : "missing") + "; south=" + (south ? south.bounds.ToString() : "missing");
            return tileVertices > 20 && north && south && Mathf.Abs(north.bounds.center.x - 2) < .03f && north.bounds.max.z > 5.9f &&
                Mathf.Abs(south.bounds.center.z - 1.8f) < .03f && south.bounds.min.x < 2.1f && south.bounds.max.x > 7.9f;
        }

        IEnumerator WalkTo(string id, Vector3 target)
        {
            var walk = NewWalk(id, target); float end = Time.realtimeSinceStartup + 2.6f;
            while (Time.realtimeSinceStartup < end && walk.frames < 170)
            {
                Vector3 delta = target - w.PlayerPosition; delta.y = 0;
                if (delta.magnitude < .035f) { walk.reached = true; break; }
                // Use the real analogue input range to brake before reaching the target.
                // Full speed until .075m caused an inertial drift into the next wall probe.
                Step(walk, Vector3.ClampMagnitude(delta / .35f, 1)); yield return null;
            }
            yield return Settle(10); EndWalk(walk);
        }
        IEnumerator Push(string id, Vector3 direction, float seconds)
        {
            var walk = NewWalk(id, w.PlayerPosition + direction * 2.7f * seconds);
            int count = Mathf.CeilToInt(seconds / .016f);
            for (int i = 0; i < count; i++) { Step(walk, direction); yield return null; }
            yield return Settle(5); EndWalk(walk);
        }
        Walk NewWalk(string id, Vector3 target)
        {
            var walk = new Walk { id = id, start = w.PlayerPosition, target = target, seconds = Time.realtimeSinceStartup };
            walk.samples.Add(walk.start); report.walks.Add(walk); return walk;
        }
        void Step(Walk walk, Vector3 worldDirection)
        {
            Vector3 previous = w.PlayerPosition;
            Vector3 f = w.View.transform.forward; f.y = 0; f.Normalize();
            Vector3 r = w.View.transform.right; r.y = 0; r.Normalize();
            w.SmokeMove(new Vector2(Vector3.Dot(worldDirection, r), Vector3.Dot(worldDirection, f)), .016f);
            Vector3 current = w.PlayerPosition;
            walk.frames++; walk.minY = Mathf.Min(walk.minY, current.y); walk.maxY = Mathf.Max(walk.maxY, current.y);
            if (walk.frames % 6 == 0) walk.samples.Add(current);
            if (Mathf.Abs(current.z - 2.5f) < .25f && current.y > 3.08f && current.y < 3.38f)
            {
                if (previous.x <= 2 && current.x > 2) { walk.forwardCrossings++; report.forwardCrossings++; }
                if (previous.x >= 2 && current.x < 2) { walk.reverseCrossings++; report.reverseCrossings++; }
            }
            if (walk.frames % 8 == 0) AutoFocus();
        }
        void EndWalk(Walk walk)
        {
            walk.end = w.PlayerPosition; walk.seconds = Time.realtimeSinceStartup - walk.seconds; walk.samples.Add(walk.end);
            Vector3 delta = walk.target - walk.end; delta.y = 0; walk.reached &= delta.magnitude < .2f;
        }
        static string WalkDetail(Walk walk) { return walk.id + ": " + walk.start + " -> " + walk.end + "; reached=" + walk.reached + "; east=" + walk.forwardCrossings + "; west=" + walk.reverseCrossings + "; y=" + walk.minY + ".." + walk.maxY; }
        bool Supported() { return body.enabled && w.IsGrounded && Mathf.Abs(w.PlayerPosition.y - FloorY) < .15f; }
        IEnumerator Settle(int count) { for (int i = 0; i < count; i++) { w.SmokeMove(Vector2.zero, .016f); yield return null; } }
        IEnumerator SettleFor(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) { w.SmokeMove(Vector2.zero, .016f); yield return null; }
        }
        void AutoFocus() { refreshFocus.Invoke(w, new object[] { false }); report.focusTrail.Add(Time.realtimeSinceStartup.ToString("F2") + ":" + FocusId() + "@" + w.PlayerPosition); }
        string FocusId() { return w.NearestSource != null ? w.NearestSource.Id : w.Selected != null ? w.Selected.Id : "decor:" + w.FocusLabel; }
        string FirstObstacle(Vector3 start, Vector3 direction, float distance)
        {
            Physics.SyncTransforms();
            var hits = Physics.RaycastAll(start, direction, distance, ~(1 << 30), QueryTriggerInteraction.Ignore).OrderBy(h => h.distance);
            foreach (var hit in hits) if (hit.collider != body && !hit.transform.IsChildOf(body.transform)) return hit.collider.name;
            return "none";
        }
        Dictionary<string, HingeState> Hinges(string stage)
        {
            var result = new Dictionary<string, HingeState>();
            var map = hingeField.GetValue(w) as IDictionary;
            if (map == null) throw new InvalidOperationException("Private hinge map unavailable.");
            foreach (DictionaryEntry pair in map)
            {
                object value = pair.Value; Type type = value.GetType();
                var leaf = (Transform)type.GetField("leaf").GetValue(value);
                var state = new HingeState { id = (string)pair.Key, open = (bool)type.GetField("open").GetValue(value), leaf = leaf, closed = (Quaternion)type.GetField("closed").GetValue(value), rotation = leaf.localRotation };
                result[state.id] = state;
                if (state.id == "bathroom_door" || state.id == "bath_cabinet" || state.id == "door_inner_ground") report.hinges.Add(new HingeProbe {
                    stage = stage, id = state.id, open = state.open, leafInstanceId = leaf.GetInstanceID(),
                    angleFromClosed = Quaternion.Angle(state.closed, state.rotation), signedYawFromClosed = Mathf.DeltaAngle(state.closed.eulerAngles.y, state.rotation.eulerAngles.y) });
            }
            return result;
        }
        static bool OtherHingesUnchanged(Dictionary<string, HingeState> before, Dictionary<string, HingeState> after, string exception)
        {
            return before.Count == after.Count && before.All(p => p.Key == exception || after.TryGetValue(p.Key, out var next) &&
                p.Value.leaf == next.leaf && p.Value.open == next.open && Quaternion.Angle(p.Value.rotation, next.rotation) < 2);
        }
        static string Slots(List<SupplySlot> slots) { return string.Join("|", slots.Select(s => (s.Id ?? "") + ":" + s.Count).ToArray()); }
        IEnumerator Shot(string id)
        {
            yield return new WaitForEndOfFrame(); string path = Path.Combine(w.EvidencePath, id + ".png");
            ScreenCapture.CaptureScreenshot(path); report.screenshots.Add(path);
            // CaptureScreenshot writes asynchronously; a final capture must finish before Quit.
            float deadline = Time.realtimeSinceStartup + 1;
            long previousLength = -1; int stableFrames = 0;
            while (Time.realtimeSinceStartup < deadline)
            {
                long length = File.Exists(path) ? new FileInfo(path).Length : 0;
                stableFrames = length >= 256 && length == previousLength ? stableFrames + 1 : 0;
                if (stableFrames >= 2) yield break;
                previousLength = length; yield return null;
            }
            report.errors.Add("Native capture did not finish writing within 1s: " + path);
        }
        void CheckIt(string name, bool pass, string detail = "")
        {
            report.checks.Add(new Check { name = name, passed = pass, utc = DateTime.UtcNow.ToString("o"), detail = detail ?? "" });
            Debug.Log("BATHROOM_CHECK " + (pass ? "PASS " : "FAIL ") + name + " " + detail);
        }
        void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Update()
        {
            if (finished || !w) return;
            frames++; frameSeconds += Time.unscaledDeltaTime;
            if (Time.realtimeSinceStartup - started > HardCapSeconds)
            {
                report.errors.Add("Bathroom smoke exceeded its " + HardCapSeconds + " second hard cap; remaining checks did not execute.");
                if (running != null) StopCoroutine(running); Finish();
            }
        }
        void Finish()
        {
            if (finished) return; finished = true;
            report.finishedUtc = DateTime.UtcNow.ToString("o"); report.runtimeSeconds = Time.realtimeSinceStartup - started;
            report.meanFps = frames / Mathf.Max(.01f, frameSeconds);
            foreach (string path in report.screenshots) if (!File.Exists(path) || new FileInfo(path).Length < 256) report.errors.Add("Missing/empty actual render: " + path);
            report.passed = report.errors.Count == 0 && report.checks.Count == ExpectedChecks && report.checks.All(c => c.passed) && report.screenshots.Count == 4;
            Application.logMessageReceived -= Log;
            try { File.WriteAllText(Path.Combine(w.EvidencePath, "native-bathroom-smoke.json"), JsonUtility.ToJson(report, true)); }
            catch (Exception ex) { Debug.LogError("BATHROOM_SMOKE report write failed: " + ex); }
            Debug.Log("BATHROOM_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " errors=" + report.errors.Count + " seconds=" + report.runtimeSeconds.ToString("F1"));
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}
