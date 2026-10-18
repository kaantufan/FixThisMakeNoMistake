using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TooLateToPanic
{
    // Opt-in graphical evidence only. Production simulation APIs are used for four views.
    // Offscreen factory fixtures test cosmetics and teardown; they do not certify shelter traversal.
    public sealed class PanicYarkinHybridSmoke : MonoBehaviour
    {
        const string Flag = "--yarkin-hybrid-smoke";
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id, status, detail; public int frame; }
        [Serializable] public sealed class Shot { public string path, context, phase; public int frame, width, height; public Vector3 camera; }
        [Serializable] public sealed class Receipt
        {
            public string utc, dllSha256, realScoreBefore, realScoreAfter, error;
            public string layer = "Native graphical production simulation API/render plus explicitly offscreen cosmetic factory fixtures. No OS input, human appearance, listening, physical route, score or full campaign acceptance.";
            public bool complete, passed, osInput, humanAcceptance;
            public float seconds, maximumHousePositionDrift, maximumHouseRotationDrift;
            public List<Row> checks = new List<Row>(); public List<Shot> captures = new List<Shot>(); public List<string> errors = new List<string>();
        }
        struct Pose { public Transform t; public Vector3 position, scale; public Quaternion rotation; }
        readonly Receipt report = new Receipt();
        readonly List<GameObject> fixtures = new List<GameObject>();
        PanicHomeWorld w; PanicHomeHud hud; float began; bool finished; string realScore;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        { if (Environment.GetCommandLineArgs().Contains(Flag)) { var w = FindFirstObjectByType<PanicHomeWorld>(); if (w) w.gameObject.AddComponent<PanicYarkinHybridSmoke>(); } }
        void Start()
        {
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); began = Time.realtimeSinceStartup;
            realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.utc = DateTime.UtcNow.ToString("o"); report.dllSha256 = Hash(typeof(PanicModel).Assembly.Location); report.realScoreBefore = Hash(realScore);
            Application.logMessageReceived += Log; StartCoroutine(Guard(Run()));
        }
        void Update() { if (!finished && Time.realtimeSinceStartup - began > 180) { report.error = "180s bounded hybrid smoke cap"; Finish(); } }
        void Log(string message, string stack, LogType kind)
        { if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Check(string id, bool pass, string detail = "")
        { report.checks.Add(new Row { id = id, status = pass ? "PASS" : "FAIL", detail = detail, frame = Time.frameCount }); Debug.Log("HYBRID_CHECK " + (pass ? "PASS " : "FAIL ") + id + " " + detail); }
        static string Hash(string path) { if (!File.Exists(path)) return "ABSENT"; using (var sha = SHA256.Create()) using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""); }
        static T Field<T>(object owner, string name) { return (T)owner.GetType().GetField(name, Flags).GetValue(owner); }
        static bool Finite(Vector3 value) { return !float.IsNaN(value.x + value.y + value.z) && !float.IsInfinity(value.x + value.y + value.z); }
        static float StableAngle(Quaternion a, Quaternion b)
        {
            a = a.normalized; b = b.normalized;
            double dx = (double)a.x - b.x, dy = (double)a.y - b.y, dz = (double)a.z - b.z, dw = (double)a.w - b.w;
            double sx = (double)a.x + b.x, sy = (double)a.y + b.y, sz = (double)a.z + b.z, sw = (double)a.w + b.w;
            double chord = Math.Sqrt(Math.Min(dx * dx + dy * dy + dz * dz + dw * dw, sx * sx + sy * sy + sz * sz + sw * sw));
            return (float)(4 * Math.Asin(Math.Min(1, chord * .5)) * 180 / Math.PI);
        }
        string Fingerprint() { return PanicNotebookVerification.GameplayFingerprint(w) + "|" + w.Model.SecondaryRollCountV5 + "|" + w.Model.SecondaryEventSummaryV5; }
        IEnumerator Guard(IEnumerator body)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(body);
            while (stack.Count > 0 && !finished)
            {
                bool next; object current = null;
                try { next = stack.Peek().MoveNext(); if (next) current = stack.Peek().Current; }
                catch (Exception ex) { report.error = ex.ToString(); break; }
                if (!next) { stack.Pop(); continue; }
                if (current is IEnumerator nested) stack.Push(nested); else yield return current;
            }
            if (!finished) { report.complete = string.IsNullOrEmpty(report.error); Finish(); }
        }
        IEnumerator Wait(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
        IEnumerator Frames() { yield return null; yield return null; yield return null; }
        GameObject Fixture(GameObject root) { root.transform.position = new Vector3(1000, 0, 1000); fixtures.Add(root); return root; }
        static List<Object> Owned(GameObject root)
        {
            var values = new List<Object>();
            foreach (var s in root.GetComponentsInChildren<PanicExteriorOwnedArt>(true)) { values.AddRange(s.Materials); values.AddRange(s.Meshes); }
            foreach (var s in root.GetComponentsInChildren<PanicDisasterArtState>(true)) values.AddRange(s.Owned);
            foreach (var s in root.GetComponentsInChildren<PanicStormArtState>(true))
            { values.AddRange(s.Materials); values.Add(s.Soft); values.Add(s.Leaf); values.Add(s.YarkinHybridShell); values.Add(s.YarkinHybridPuff); values.Add(s.YarkinHybridShard); values.Add(s.YarkinHybridTexture); }
            foreach (var f in root.GetComponentsInChildren<MeshFilter>(true)) if (f.sharedMesh && !values.Contains(f.sharedMesh)) values.Add(f.sharedMesh);
            return values.Where(x => x).Distinct().ToList();
        }
        IEnumerator FactoryChecks()
        {
            string before = Fingerprint();
            var storm = Fixture(PanicStormArt.Create()); var s = storm.GetComponent<PanicStormArtState>(); var motion = storm.GetComponent<PanicStormMotionV414>();
            PanicStormArt.Animate(storm, .1f, 1, 1.65f);
            Check("storm.retained-and-added", motion && s.YarkinHybridRoot && s.YarkinHybridShell && s.YarkinHybridBands.Count == 3 && storm.GetComponentsInChildren<MeshFilter>(true).Any(x => x.sharedMesh && x.sharedMesh.name.Contains("Coherent rotating tapered")), "Retained core and new shell are actual mesh instances together");
            Check("storm.cosmetic-no-physics", storm.GetComponentsInChildren<Collider>(true).Length == 0 && storm.GetComponentsInChildren<Rigidbody>(true).Length == 0);
            Check("storm.bounded-finite", s.YarkinHybridShell.vertices.All(v => Finite(v) && v.x >= -24.2f && v.x <= -14.2f && v.y >= -.001f && v.y <= 11.501f && v.z >= 6 && v.z <= 16), "New shell fits retained 10x16x10 mesh volume");
            float clock = motion.Clock; PanicStormArt.Animate(storm, .1f, 1, 99);
            Check("storm.clock-and-scale", Mathf.Abs(motion.Clock - clock) < .00001f && Mathf.Abs(motion.PresentationScaleV52 - 1.65f) < .00001f, "Same caller age does not advance private clock; presentation clamps to1.65");
            PanicStormArt.Animate(storm, .1f, 0, 1); Check("storm.zero-hides-shell", !s.YarkinHybridRoot.activeSelf);
            var resources = Owned(storm); Destroy(storm); yield return Frames(); Check("storm.owned-teardown", resources.Count > 8 && resources.All(x => !x), "Actual tracked meshes/materials/textures destroyed");
            var quake = Fixture(PanicDisasterArt.Create(DisasterType.Earthquake)); var q = quake.GetComponent<PanicDisasterArtState>();
            PanicDisasterArt.Animate(quake, .1f, 1, 1.65f);
            Check("quake.exterior-art", q.QuakeVisualsObject && q.RoofEavesDust && q.FacadeDust && q.ExteriorDebris != null && q.ExteriorDebris.Length == 48);
            Check("quake.cosmetic-no-physics", quake.GetComponentsInChildren<Collider>(true).Length == 0 && quake.GetComponentsInChildren<Rigidbody>(true).Length == 0);
            var childResources = new Object[] { q.RoofEavesDustMesh, q.FacadeDustMesh, q.ExteriorDebrisMesh, q.ExteriorDebrisMaterial };
            Destroy(q.QuakeVisualsObject); yield return Frames(); PanicDisasterArt.Animate(quake, .2f, 1, 1);
            Check("quake.child-teardown-safe", childResources.All(x => !x) && !q.RoofEavesDust && !q.FacadeDust && !q.ExteriorDebrisMesh, "Independent visual-child removal, then actual parent Animate call");
            resources = Owned(quake); Destroy(quake); yield return Frames(); Check("quake.owned-teardown", resources.Count > 0 && resources.All(x => !x));
            var fire = Fixture(PanicDisasterArt.Create(DisasterType.Fire)); var f = fire.GetComponent<PanicDisasterArtState>();
            Check("fire.modest-authored-emission", f.Emitters.Any(e => Mathf.Abs(e.Rate - 166) < .001f) && f.FlameSites.Length == 4, "Outer authored rate145→166, retained four sites with five tongues each");
            PanicDisasterArt.Animate(fire, .1f, 1, 1.65f); Check("fire.scale-capped", Mathf.Abs(f.PresentationScaleV52 - 1.65f) < .00001f && f.FlameVertices.All(Finite));
            resources = Owned(fire); Destroy(fire); yield return Frames(); Check("fire.owned-teardown", resources.All(x => !x));
            // Actual current pedestrian factory, with an explicit borrowed resource/collider fixture.
            // This exercises the two ownership regressions caught in source review, outside gameplay.
            var npc = Fixture(PanicExteriorArt.CreatePedestrianVisual(0)); var animator = npc.GetComponentInChildren<Animator>(true);
            var avatar = animator ? animator.avatar : null; var controller = animator ? animator.runtimeAnimatorController : null;
            bool rootMotion = animator && animator.applyRootMotion;
            var probe = GameObject.CreatePrimitive(PrimitiveType.Cube); probe.name = "Borrowed shared resource probe"; probe.transform.SetParent(npc.transform, false);
            var originalCollider = probe.GetComponent<Collider>(); var primitiveMesh = probe.GetComponent<MeshFilter>().sharedMesh;
            var borrowed = new Material(Shader.Find("Standard")) { name = "Hybrid QA borrowed original" }; probe.GetComponent<Renderer>().sharedMaterial = borrowed;
            var details = npc.AddComponent<PanicNeighbourDetailsHybrid>(); details.Bind(w, animator, 0);
            var owner = npc.GetComponent<PanicExteriorOwnedArt>();
            Check("neighbour.preserve-existing-configuration", originalCollider && originalCollider.enabled && animator && animator.avatar == avatar && animator.runtimeAnimatorController == controller && animator.applyRootMotion == rootMotion, "Offscreen current factory plus explicit preexisting collider; no live player mutations");
            Check("neighbour.borrowed-never-owned", owner && !owner.Materials.Contains(borrowed) && !owner.Meshes.Contains(primitiveMesh));
            var ownedNpc = new List<Object>(); if (owner) { ownedNpc.AddRange(owner.Materials); ownedNpc.AddRange(owner.Meshes); }
            details.QueuePose(PanicNeighbourActionHybrid.Radio, 1, .1f); yield return Frames();
            Check("neighbour.fixture-late-pose", details.PoseApplyCount > 0 && details.LastAppliedFrame >= Time.frameCount - 2 && originalCollider && originalCollider.enabled);
            if (owner) { ownedNpc.AddRange(owner.Materials); ownedNpc.AddRange(owner.Meshes); }
            Destroy(npc); yield return Frames();
            Check("neighbour.owned-release-borrowed-survive", ownedNpc.Count > 0 && ownedNpc.All(x => !x) && borrowed && primitiveMesh, "Tracked runtime role/radio resources released; foreign material and shared Unity primitive mesh retained");
            Destroy(borrowed); yield return Frames();
            foreach (var type in new[] { DisasterType.Earthquake, DisasterType.Fire, DisasterType.Flood }) for (int variant = 0; variant < 4; variant++)
            {
                var decal = Fixture(PanicDamageDecalArt.CreateDamageMark(type, variant, new Vector3(.9f, 1.1f, .3f), null)); resources = Owned(decal);
                Check("decal." + type + "." + variant, decal.GetComponentsInChildren<Renderer>(true).Length > 0 && decal.GetComponentsInChildren<Collider>(true).Length == 0 && decal.GetComponentsInChildren<MeshFilter>(true).All(m => m.sharedMesh && m.sharedMesh.vertices.All(Finite)) && resources.Count > 0, "Explicit factory fixture; world selection still uses committed item damage");
                Destroy(decal); yield return Frames(); Check("decal.release." + type + "." + variant, resources.All(x => !x));
            }
            Check("factories.model-rng-unchanged", before == Fingerprint(), "Title model fingerprint before/after cosmetic calls; no elapsed gameplay");
        }
        List<Pose> HouseSnapshot()
        {
            var roots = new List<GameObject>(Field<List<GameObject>>(w, "floorVisuals")); roots.Add(Field<GameObject>(w, "roof"));
            return roots.Where(x => x).SelectMany(x => new[] { x.transform }.Concat(x.GetComponentsInChildren<Collider>(true).Select(c => c.transform))).Distinct().Select(t => new Pose { t = t, position = t.position, rotation = t.rotation, scale = t.localScale }).ToList();
        }
        bool HouseFixed(List<Pose> poses)
        {
            bool okay = poses.Count > 4;
            foreach (var p in poses)
            {
                if (!p.t) { okay = false; continue; }
                float distance = Vector3.Distance(p.t.position, p.position), angle = StableAngle(p.t.rotation, p.rotation);
                report.maximumHousePositionDrift = Mathf.Max(report.maximumHousePositionDrift, distance); report.maximumHouseRotationDrift = Mathf.Max(report.maximumHouseRotationDrift, angle);
                okay &= distance < .00001f && angle < .005f && Vector3.Distance(p.t.localScale, p.scale) < .00001f;
            }
            return okay;
        }
        IEnumerator Capture(string name, string context)
        {
            yield return new WaitForEndOfFrame(); string path = Path.Combine(w.EvidencePath, name + ".png");
            report.captures.Add(new Shot { path = path, context = context, phase = w.Model.Phase.ToString(), frame = Time.frameCount, width = Screen.width, height = Screen.height, camera = w.View.transform.position });
            ScreenCapture.CaptureScreenshot(path); float end = Time.realtimeSinceStartup + 2; while (!File.Exists(path) && Time.realtimeSinceStartup < end) yield return null;
            Check("capture." + name, File.Exists(path) && new FileInfo(path).Length > 1024, "Native production camera framebuffer; root must inspect pixels");
        }
        IEnumerator ProductionView(DisasterType type)
        {
            Check("production.start." + type, w.StartDisasterSimulation(type), "Production simulation sets own bag/actor; not shelter route acceptance"); yield return null;
            var house = HouseSnapshot(); w.TriggerSimulation(); yield return null;
            Check("production.trigger." + type, w.Model.Phase == PanicPhase.Disaster && w.Model.IsSimulation && !w.Model.CheatedRun);
            bool fixedHouse = true; float start = Time.realtimeSinceStartup; bool first = false, second = false;
            while (w.Model.Phase == PanicPhase.Disaster && Time.realtimeSinceStartup - start < 24)
            {
                yield return new WaitForEndOfFrame(); if (type == DisasterType.Earthquake) fixedHouse &= HouseFixed(house);
                float age = Field<float>(w, "residenceCinematicAge");
                if (!first && age >= 4.9f) { first = true; yield return Capture("hybrid-" + type + "-5s", "Actual production simulation camera around5s"); }
                if (!second && age >= 6.1f) { second = true; yield return Capture("hybrid-" + type + "-6s", "Actual production simulation camera around6s"); }
            }
            Check("production.capture-window." + type, first && second);
            Check("production.natural-aftermath." + type, w.Model.Phase == PanicPhase.Aftermath, "Bounded real-time wait for actual production phase; no Tick or completion fixture");
            if (type == DisasterType.Earthquake) Check("quake.actual-house-colliders-fixed", fixedHouse && HouseFixed(house), "Actual floor/roof roots plus descendant real colliders; renderer proxies may shake");
            yield return Wait(.5f);
            if (type == DisasterType.Earthquake && w.Model.Phase != PanicPhase.Disaster)
            {
                var neighbours = FindObjectsByType<PanicNeighbourDetailsHybrid>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                Check("neighbour.actual-four", neighbours.Length == 4 && w.NeighbourDetailsHybridCount == 4);
                Check("neighbour.retained-humanoid", neighbours.All(n => n.GetComponentInChildren<Animator>(true) && n.GetComponentInChildren<Animator>(true).isHuman));
                Check("neighbour.late-pose-applied", neighbours.All(n => n.PoseApplyCount > 0 && n.LastAppliedFrame >= Time.frameCount - 3), "LateUpdate cosmetic action ran on actual four neighbours after production aftermath");
                yield return Capture("hybrid-neighbour-response", "Actual neighbour role art in production aftermath");
            }
            w.ReturnToMenu(); yield return Frames();
        }
        IEnumerator Run()
        {
            string[] args = Environment.GetCommandLineArgs(); string reason;
            bool registered = PanicTestLaunchRules.Drivers.Contains(Flag), valid = PanicTestLaunchRules.Validate(args, File.Exists, out reason);
            string leaf = Path.GetFileName((w.EvidencePath ?? "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)); Guid guid;
            bool isolated = !string.IsNullOrEmpty(w.EvidencePath) && leaf.Length >= 36 && Guid.TryParseExact(leaf.Substring(leaf.Length - 36), "D", out guid) && !string.IsNullOrEmpty(PanicModel.ScorePathOverride) && Path.GetFullPath(PanicModel.ScorePathOverride) == Path.Combine(Path.GetFullPath(w.EvidencePath), "isolated-home-scores.json") && !File.Exists(PanicModel.ScorePathOverride);
            Check("launch.registered-isolated", registered && valid && isolated, reason); if (!registered || !valid || !isolated) yield break;
            Application.runInBackground = true; float end = Time.realtimeSinceStartup + 10; while (!UnityEngine.Rendering.SplashScreen.isFinished && Time.realtimeSinceStartup < end) yield return null;
            Check("launch.graphical-surface", w.View && w.View.enabled && !w.View.targetTexture && Screen.width >= 960 && Screen.height >= 540);
            w.FullFlowScriptedInput = true; yield return FactoryChecks();
            foreach (var type in new[] { DisasterType.Earthquake, DisasterType.Storm, DisasterType.Fire, DisasterType.Flood }) yield return ProductionView(type);
            Check("final.no-cheats", !w.Model.CheatedRun && !w.Model.DeveloperAnyEnabled);
        }
        void Finish()
        {
            if (finished) return; finished = true; Application.logMessageReceived -= Log;
            foreach (var root in fixtures) if (root) Destroy(root);
            report.seconds = Time.realtimeSinceStartup - began; report.realScoreAfter = Hash(realScore); Check("real-score.unchanged", report.realScoreBefore == report.realScoreAfter);
            report.passed = report.complete && string.IsNullOrEmpty(report.error) && report.errors.Count == 0 && report.checks.Count > 0 && report.checks.All(x => x.status == "PASS");
            if (!string.IsNullOrEmpty(w.EvidencePath)) { Directory.CreateDirectory(w.EvidencePath); File.WriteAllText(Path.Combine(w.EvidencePath, "hybrid-report.json"), JsonUtility.ToJson(report, true)); }
            Debug.Log("HYBRID_FINISHED " + report.passed); Application.Quit(report.passed ? 0 : 1);
        }
    }
}
