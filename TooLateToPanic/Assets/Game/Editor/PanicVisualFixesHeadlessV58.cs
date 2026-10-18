#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TooLateToPanic
{
    /// <summary>One-shot actual Editor geometry/Avatar validation; never renders or opens a player.</summary>
    public static class PanicVisualFixesHeadlessV58
    {
        [Serializable] public sealed class CheckRow { public string id, phase, detail; public bool pass; }
        [Serializable] public sealed class SurfaceRow { public string id; public float age, wetness, y; public bool triangle; public Vector3 position; }
        [Serializable] public sealed class MeshRow { public string id, digest; public int vertices, triangles, degenerate; public Bounds bounds; public bool finite, normals; }
        [Serializable] public sealed class Report
        {
            public string utc, unity, layer = "ACTUAL_UNITY_EDITOR_HEADLESS_MESH_AVATAR_DATA_NOT_RENDER_NATIVE_PLAYER_OS_OR_HUMAN", error;
            public bool complete, passed, graphicsRendered, playerLaunched, humanAcceptance;
            public List<CheckRow> checks = new List<CheckRow>();
            public List<SurfaceRow> surfaces = new List<SurfaceRow>();
            public List<MeshRow> meshes = new List<MeshRow>();
            public List<string> notes = new List<string>();
        }
        static Report report; static string phase; static readonly List<GameObject> roots = new List<GameObject>();
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        static void Check(string id, bool pass, string detail)
        { report.checks.Add(new CheckRow { id = id, phase = phase, pass = pass, detail = detail }); Debug.Log("V58_GEOMETRY " + (pass ? "PASS " : "FAIL ") + id + " " + detail); }
        static bool Finite(float x) { return !float.IsNaN(x) && !float.IsInfinity(x); }
        static bool Finite(Vector3 v) { return Finite(v.x) && Finite(v.y) && Finite(v.z); }
        static string F(float x) { return x.ToString("R", Inv); }
        static string V(Vector3 v) { return F(v.x) + "," + F(v.y) + "," + F(v.z); }
        static string Q(Quaternion v) { return F(v.x) + "," + F(v.y) + "," + F(v.z) + "," + F(v.w); }
        static string Digest(string s) { using (var h = SHA256.Create()) return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", ""); }
        static string Trs(Transform t) { return V(t.localPosition) + "/" + Q(t.localRotation) + "/" + V(t.localScale); }
        static GameObject Track(GameObject o) { if (o) { roots.Add(o); o.hideFlags = HideFlags.HideAndDontSave; } return o; }
        static Vector3[] ModelVertices(MeshFilter f, Transform root)
        { return f.sharedMesh.vertices.Select(v => root.InverseTransformPoint(f.transform.TransformPoint(v))).ToArray(); }
        static MeshRow MeshData(string id, Mesh mesh, Vector3[] vertices = null)
        {
            var v = vertices ?? mesh.vertices; var t = mesh.triangles; var n = mesh.normals; var b = new Bounds(v.Length > 0 ? v[0] : Vector3.zero, Vector3.zero);
            bool finite = v.Length > 0 && v.All(Finite), normals = n.Length == v.Length && n.All(x => Finite(x) && x.sqrMagnitude > .5f && x.sqrMagnitude < 1.5f);
            int degenerate = 0; bool indices = t.Length > 0 && t.Length % 3 == 0 && t.All(i => i >= 0 && i < v.Length);
            foreach (var p in v) b.Encapsulate(p);
            if (indices) for (int i = 0; i < t.Length; i += 3) if (Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]).sqrMagnitude < 1e-16f) degenerate++;
            var row = new MeshRow { id = id, vertices = v.Length, triangles = t.Length / 3, degenerate = degenerate, bounds = b, finite = finite && indices, normals = normals,
                digest = Digest(string.Join("|", v.Select(V)) + "/" + string.Join(",", t)) }; report.meshes.Add(row); return row;
        }
        // Independent XZ barycentric interpolation of actual mesh triangles, not production wetness math.
        static bool Height(Vector3[] vertices, int[] triangles, float x, float z, out float y)
        {
            y = float.NegativeInfinity; bool found = false;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var a = vertices[triangles[i]]; var b = vertices[triangles[i + 1]]; var c = vertices[triangles[i + 2]];
                float d = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z); if (Mathf.Abs(d) < 1e-9f) continue;
                float u = ((b.z - c.z) * (x - c.x) + (c.x - b.x) * (z - c.z)) / d;
                float v = ((c.z - a.z) * (x - c.x) + (a.x - c.x) * (z - c.z)) / d; float w = 1 - u - v;
                if (u < -.0001f || v < -.0001f || w < -.0001f) continue;
                y = Mathf.Max(y, u * a.y + v * b.y + w * c.y); found = true;
            }
            return found;
        }
        static void Pavement()
        {
            phase = "cached-landscape"; var prefab = Resources.Load<GameObject>("HomeArt/landscape");
            Check("garage.cached-resource-exists", prefab && EditorUtility.IsPersistent(prefab), "Actual baked Resources/HomeArt/landscape asset required, not a newly built factory substitute."); if (!prefab) return;
            var root = Track(Object.Instantiate(prefab)); root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var concrete = root.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh && f.GetComponent<MeshRenderer>() && f.GetComponent<MeshRenderer>().enabled && f.gameObject.activeInHierarchy &&
                f.GetComponent<MeshRenderer>().sharedMaterials.Any(m => m && m.name.IndexOf("Concrete", StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
            Check("garage.actual-concrete-render-mesh", concrete.Length > 0 && concrete.All(f => f.sharedMesh.isReadable), "Enabled cached render MeshFilters with concrete materials=" + concrete.Length + "; no shader-support/render claim.");
            var geometries = concrete.Where(f => f.sharedMesh.isReadable).Select(f => new { vertices = f.sharedMesh.vertices.Select(f.transform.TransformPoint).ToArray(), triangles = f.sharedMesh.triangles }).ToArray();
            foreach (float x in new[] { 10.12f, 11.7f, 13.28f }) foreach (float z in new[] { -12f, -10f, -8f, -6f, -4f, -2f })
            {
                bool found = false; float top = float.NegativeInfinity;
                foreach (var g in geometries) if (Height(g.vertices, g.triangles, x, z, out var y) && y >= .009f && y <= .018f) { found = true; top = Mathf.Max(top, y); }
                report.surfaces.Add(new SurfaceRow { id = "garage", position = new Vector3(x, 0, z), triangle = found, y = found ? top : -999 });
                Check("garage.cached-triangle." + F(x) + "." + F(z), found, "Actual concrete triangle covers XZ at authored drive surface y .009..018; top=" + (found ? F(top) : "absent"));
            }
            Check("garage.cached-bounds-sane", geometries.Length > 0 && geometries.All(g => g.vertices.All(p => Finite(p) && Mathf.Abs(p.x) < 200 && Mathf.Abs(p.z) < 200 && Mathf.Abs(p.y) < 100)), "Cached concrete vertex bounds finite and within neighbourhood;18 independent coverage points prove the baked straight strip.");
        }
        static bool RayTriangle(Vector3 origin, Vector3 direction, float limit, Vector3 a, Vector3 b, Vector3 c)
        {
            var e1 = b - a; var e2 = c - a; var p = Vector3.Cross(direction, e2); float d = Vector3.Dot(e1, p); if (Mathf.Abs(d) < 1e-9f) return false;
            float inv = 1 / d; var tv = origin - a; float u = Vector3.Dot(tv, p) * inv; if (u < 0 || u > 1) return false;
            var q = Vector3.Cross(tv, e1); float v = Vector3.Dot(direction, q) * inv; if (v < 0 || u + v > 1) return false;
            float distance = Vector3.Dot(e2, q) * inv; return distance >= 0 && distance < limit;
        }
        static List<Vector3> HeadPoints(GameObject root, Transform head)
        {
            var result = new List<Vector3>();
            foreach (var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                string name = skin.name.ToLowerInvariant(); if (name.Contains("eye") || name.Contains("brow") || name.Contains("hair") || !skin.sharedMesh || !skin.sharedMesh.isReadable) continue;
                var verts = skin.sharedMesh.vertices; var weights = skin.sharedMesh.boneWeights; var bones = skin.bones;
                for (int i = 0; i < verts.Length && i < weights.Length; i++)
                {
                    var w = weights[i]; float influence = 0;
                    if (w.boneIndex0 < bones.Length && bones[w.boneIndex0] == head) influence += w.weight0;
                    if (w.boneIndex1 < bones.Length && bones[w.boneIndex1] == head) influence += w.weight1;
                    if (w.boneIndex2 < bones.Length && bones[w.boneIndex2] == head) influence += w.weight2;
                    if (w.boneIndex3 < bones.Length && bones[w.boneIndex3] == head) influence += w.weight3;
                    if (influence >= .5f) result.Add(root.transform.InverseTransformPoint(skin.transform.TransformPoint(verts[i])));
                }
            }
            return result;
        }
        static void Neighbours()
        {
            phase = "actual-neighbour-clones"; var source = Resources.Load<GameObject>("Player/SurvivorV2");
            Check("npc.retained-source-present", source && EditorUtility.IsPersistent(source), "Borrow existing SurvivorV2 prefab; no source prefab write."); if (!source) return;
            string sourcePath = AssetDatabase.GetAssetPath(source), sourceBefore = AssetDatabase.GetAssetDependencyHash(sourcePath).ToString();
            var ownerObject = Track(new GameObject("V58 inactive QA owner")); ownerObject.SetActive(false); var world = ownerObject.AddComponent<PanicHomeWorld>();
            Check("npc.dummy-world-no-awake-game", world.Model == null && world.View == null, "Inactive edit-mode component; no game Awake/Start, no scene/player/controller simulation.");
            var signatures = new List<string>();
            for (int role = 0; role < 4; role++)
            {
                var root = Track(PanicExteriorArt.CreatePedestrianVisual(role)); if (!root) { Check("npc.clone." + role, false, "Factory returned null"); continue; }
                var animator = root.GetComponentInChildren<Animator>(true); var avatar = animator ? animator.avatar : null; var controller = animator ? animator.runtimeAnimatorController : null;
                var clips = controller ? controller.animationClips.Select(c => c ? c.GetInstanceID() : 0).OrderBy(x => x).ToArray() : new int[0];
                var transforms = root.GetComponentsInChildren<Transform>(true).ToDictionary(t => t, Trs);
                var renderers = root.GetComponentsInChildren<Renderer>(true).ToDictionary(r => r, r => r.enabled);
                var skins = root.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToDictionary(r => r, r => r.sharedMesh);
                var oldCaps = renderers.Keys.OfType<MeshRenderer>().Where(r => r.name.StartsWith("PanicWear_", StringComparison.Ordinal) && r.name.IndexOf("short dark hair and beard", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                var head = animator && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Head");
                var points = head ? HeadPoints(root, head) : new List<Vector3>(); var headBounds = new Bounds(points.Count > 0 ? points[0] : Vector3.zero, Vector3.zero); foreach (var p in points) headBounds.Encapsulate(p);
                var detail = root.GetComponent<PanicNeighbourDetailsHybrid>() ?? root.AddComponent<PanicNeighbourDetailsHybrid>(); detail.Bind(world, animator, role);
                Check("npc.avatar-controller-clips." + role, animator && avatar && avatar.isValid && avatar.isHuman && animator.avatar == avatar && animator.runtimeAnimatorController == controller && controller && !animator.applyRootMotion &&
                    clips.SequenceEqual(controller.animationClips.Select(c => c ? c.GetInstanceID() : 0).OrderBy(x => x)) && detail.RetainedAnimator == animator && detail.WorldOwner == world,
                    "Actual factory clone retains same Avatar/controller/clip instances and animator owner; role=" + role);
                Check("npc.original-transforms-skins." + role, transforms.All(p => p.Key && Trs(p.Key) == p.Value) && skins.Count > 0 && skins.All(p => p.Key && p.Key.sharedMesh == p.Value && p.Key.enabled == renderers[p.Key]), "Every original local bone/child TRS and skin mesh/enabled state unchanged by Bind.");
                var eyeSkins = skins.Keys.Where(s => s.name.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0 || s.name.IndexOf("brow", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                Check("npc.face-eye-skins-enabled." + role, eyeSkins.Length > 0 && eyeSkins.All(s => s.enabled && s.gameObject.activeInHierarchy) && skins.Keys.Any(s => s.enabled && s.gameObject.activeInHierarchy && s.bones.Contains(head) && s.name.IndexOf("eye", StringComparison.OrdinalIgnoreCase) < 0 && s.name.IndexOf("brow", StringComparison.OrdinalIgnoreCase) < 0), "Actual retained face/head skin and eye/brow skins remain present and enabled; no renderer-visibility substitution.");
                Check("npc.no-physical-components." + role, root.GetComponentsInChildren<Collider>(true).Length == 0 && root.GetComponentsInChildren<Rigidbody>(true).Length == 0 && root.GetComponentsInChildren<Joint>(true).Length == 0, "Actual NPC clone and cosmetics contain no collider/rigidbody/joint.");
                var added = root.GetComponentsInChildren<MeshRenderer>(true).Where(r => !renderers.ContainsKey(r)).ToArray();
                Check("npc.only-fitted-hair-added." + role, added.Length == 1 && added[0].name.StartsWith("NeighbourHair_" + role + "_", StringComparison.Ordinal) && added[0].enabled && added[0].gameObject.activeInHierarchy && added[0].transform.parent == head,
                    "Exactly one new fitted hair renderer; no new cap/vest/cross/belt/tank/plank/cube renderer.");
                Check("npc.only-inherited-cap-disabled." + role, oldCaps.Length > 0 && oldCaps.All(r => !r.enabled) && renderers.All(p => oldCaps.Contains(p.Key) || p.Key.enabled == p.Value), "Only exact retained short-hair/beard cap MeshRenderer disabled; face/eyes/brows SkinnedMeshRenderers remain enabled.");
                if (added.Length != 1 || !head || points.Count == 0) { Check("npc.head-geometry-available." + role, false, "Cannot independently validate actual weighted head/hair geometry."); continue; }
                var mf = added[0].GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh || !mf.sharedMesh.isReadable) { Check("npc.hair-readable." + role, false, "Missing readable fitted mesh."); continue; }
                var v = ModelVertices(mf, root.transform); var mesh = MeshData("npc-hair-" + role, mf.sharedMesh, v); signatures.Add(mesh.digest);
                var allowed = headBounds; allowed.Expand(Vector3.Scale(headBounds.size, new Vector3(.9f, .9f, 1f)) + Vector3.one * .015f);
                Check("npc.hair-geometry-bounded." + role, mesh.finite && mesh.normals && mesh.vertices >= 30 && mesh.triangles >= 20 && mesh.degenerate < mesh.triangles * .12f && v.All(allowed.Contains),
                    "Independent actual model-space hair bounds=" + mesh.bounds + "; measured >=50%-head-weight skin bounds=" + headBounds + "; triangles=" + mesh.triangles + "; degenerate=" + mesh.degenerate + "; no large imported-bone-axis slabs.");
                var eyePoints = new List<Vector3>();
                foreach (var eye in skins.Keys.Where(s => s.name.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0 && s.name.IndexOf("brow", StringComparison.OrdinalIgnoreCase) < 0 && s.sharedMesh && s.sharedMesh.isReadable))
                    eyePoints.AddRange(eye.sharedMesh.vertices.Select(p => root.transform.InverseTransformPoint(eye.transform.TransformPoint(p))));
                var targets = new List<Vector3>();
                foreach (bool left in new[] { true, false })
                { var side = eyePoints.Where(p => left ? p.x < headBounds.center.x : p.x >= headBounds.center.x).ToArray(); if (side.Length > 0) targets.Add(new Vector3(side.Average(p => p.x), side.Average(p => p.y), side.Max(p => p.z))); }
                targets.Add(new Vector3(headBounds.center.x, headBounds.min.y + headBounds.size.y * .42f, headBounds.max.z));
                targets.Add(new Vector3(headBounds.center.x, headBounds.min.y + headBounds.size.y * .58f, headBounds.max.z));
                int covered = 0; var triangles = mf.sharedMesh.triangles;
                foreach (var target in targets) for (int i = 0; i < triangles.Length; i += 3)
                    if (RayTriangle(target + Vector3.forward * .45f, Vector3.back, .448f, v[triangles[i]], v[triangles[i + 1]], v[triangles[i + 2]])) { covered++; break; }
                Check("npc.actual-eye-face-rays-clear." + role, eyePoints.Count > 0 && targets.Count == 4 && covered == 0, "Independent front rays to actual left/right eye geometry and lower/middle face; sampled=" + targets.Count + "; blocked by fitted hair=" + covered + ". Geometric clearance, not human visual acceptance.");
            }
            Check("npc.four-distinct-mesh-silhouettes", signatures.Count == 4 && signatures.Distinct().Count() == 4, "Actual transformed vertex/triangle SHA256 differs for all four hairstyles.");
            Check("npc.borrowed-source-dependencies-unchanged", AssetDatabase.GetAssetDependencyHash(sourcePath).ToString() == sourceBefore, "Borrowed SurvivorV2 asset/dependency identity unchanged; root separately binds file hashes.");
        }
        static readonly Vector2[] FloodPoints = { new Vector2(-8.4f,0), new Vector2(8.4f,-4), new Vector2(0,6.5f), new Vector2(0,-6.5f), new Vector2(-12,3), new Vector2(12,-6.7f), new Vector2(-12,-10), new Vector2(12,-10), new Vector2(-18.7f,-19), new Vector2(0,-19), new Vector2(26,-7), new Vector2(-25,-7), new Vector2(-11,21), new Vector2(12,23), new Vector2(-26,-27), new Vector2(27,-27) };
        static int FloodSnapshot(GameObject root, PanicDisasterArtState state, float age, string id)
        {
            var vertices = state.WaterMesh.vertices; var triangles = state.WaterMesh.triangles; int above = 0;
            foreach (var p in FloodPoints)
            {
                bool found = Height(vertices, triangles, p.x, p.y, out var y); if (found && y > .02f) above++;
                report.surfaces.Add(new SurfaceRow { id = id, age = age, position = new Vector3(p.x,0,p.y), triangle = found, y = found ? y : -999, wetness = PanicDisasterArt.SampleFloodWetnessV58(root,p.x,p.y) });
            }
            return above;
        }
        static void Flood()
        {
            phase = "actual-flood-mesh-lifecycle"; string rng = JsonUtility.ToJson(UnityEngine.Random.state);
            var root = Track(PanicDisasterArt.Create(DisasterType.Flood)); var state = root ? root.GetComponent<PanicDisasterArtState>() : null;
            Check("flood.actual-water-state", state && state.WaterMesh && state.WaterMesh.isReadable, "Actual production Create(Flood) water mesh required."); if (!state || !state.WaterMesh) return;
            Check("flood.no-physical-components", root.GetComponentsInChildren<Collider>(true).Length == 0 && root.GetComponentsInChildren<Rigidbody>(true).Length == 0, "Water presentation adds no colliders/rigidbodies.");
            var mesh = MeshData("flood-water", state.WaterMesh); var bounds = mesh.bounds;
            Check("flood.actual-extent", mesh.finite && mesh.vertices > 100 && bounds.min.x < -32 && bounds.max.x > 32 && bounds.min.z < -32 && bounds.max.z > 32 && bounds.size.x < 100 && bounds.size.z < 100,
                "Actual vertex extent covers four home facades, street and nearby neighbours=" + bounds + "; no manually declared mesh.bounds substitution.");
            PanicDisasterArt.Animate(root,0,1,1); int initial = FloodSnapshot(root,state,0,"arrival"); int intermediate = 0;
            for (int i=1;i<=80;i++) { PanicDisasterArt.Animate(root,i*.1f,1,1); if(i==20) intermediate=FloodSnapshot(root,state,2,"arrival"); if(i==40)FloodSnapshot(root,state,4,"arrival"); }
            int full = FloodSnapshot(root,state,8,"arrival");
            Check("flood.progressive-actual-surface", initial==0 && intermediate>0 && intermediate<FloodPoints.Length && full==FloodPoints.Length, "Independent actual triangle heights above ground .02: t0="+initial+",t2="+intermediate+",t8="+full+"/16; no instant global entry.");
            Check("flood.actual-house-garage-holes", !Height(state.WaterMesh.vertices,state.WaterMesh.triangles,0,0,out var ignored) && !Height(state.WaterMesh.vertices,state.WaterMesh.triangles,12,3,out ignored), "Actual triangle topology excludes main house and garage interior sample points.");
            var before = state.WaterMesh.vertices; bool began = PanicDisasterArt.BeginFloodRecessionV58(root); var immediate = state.WaterMesh.vertices;
            Check("flood.recession-entry-no-pop", began && PanicDisasterArt.IsFloodRecedingV58(root) && before.SequenceEqual(immediate) && PanicDisasterArt.SampleFloodRunoffElapsedV58(root)==0,
                "BeginRecession changes lifecycle only; same actual water vertices at first entry, elapsed0.");
            float maxFrameChange = 0; int midVisible = 0;
            for (int i=1;i<=120;i++)
            {
                PanicDisasterArt.Animate(root,8+i*.1f,0,1); var now=state.WaterMesh.vertices;
                for(int v=0;v<now.Length;v++)maxFrameChange=Mathf.Max(maxFrameChange,Mathf.Abs(now[v].y-before[v].y)); before=now;
                if(i==60)midVisible=FloodSnapshot(root,state,14,"runoff");
            }
            int dry = FloodSnapshot(root,state,20,"runoff");
            Check("flood.smooth-bounded-recession", maxFrameChange<.10f && midVisible>0 && dry==0 && PanicDisasterArt.SampleFloodRunoffElapsedV58(root)>=11.99f && state.WaterMesh.vertices.All(v=>v.y<-.20f),
                "Actual .1s age steps without edit-mode Time.deltaTime fallback: maxYstep="+F(maxFrameChange)+",midVisible="+midVisible+",finalAboveGround="+dry+",elapsed="+F(PanicDisasterArt.SampleFloodRunoffElapsedV58(root))+". Final mesh below ground; not CFD or visual safety acceptance.");
            foreach(var type in new[]{DisasterType.Earthquake,DisasterType.Fire,DisasterType.Storm})
            {
                var other=Track(PanicDisasterArt.Create(type)); int nodes=other.GetComponentsInChildren<Transform>(true).Length; var stamp=string.Join("|",other.GetComponentsInChildren<MeshFilter>(true).Select(f=>f.sharedMesh?f.sharedMesh.GetInstanceID()+":"+f.sharedMesh.vertexCount:"none"));
                Check("flood.recession-does-not-target-"+type,!PanicDisasterArt.BeginFloodRecessionV58(other)&&nodes==other.GetComponentsInChildren<Transform>(true).Length&&stamp==string.Join("|",other.GetComponentsInChildren<MeshFilter>(true).Select(f=>f.sharedMesh?f.sharedMesh.GetInstanceID()+":"+f.sharedMesh.vertexCount:"none")),"Non-flood actual root/node/mesh identity unchanged by recession request; no broad hazard retirement.");
            }
            Check("flood.unity-rng-preserved",rng==JsonUtility.ToJson(UnityEngine.Random.state),"Actual factory, Animate and recession calls draw no global Unity RNG; no gameplay model evaluated.");
        }
        static void Cleanup()
        {
            foreach(var root in roots.AsEnumerable().Reverse())
            {
                if(!root)continue;var owned=new List<Object>();
                foreach(var s in root.GetComponentsInChildren<PanicDisasterArtState>(true))owned.AddRange(s.Owned);
                foreach(var s in root.GetComponentsInChildren<PanicExteriorOwnedArt>(true)){owned.AddRange(s.Materials);owned.AddRange(s.Meshes);}
                foreach(var s in root.GetComponentsInChildren<PanicStormArtState>(true)){owned.AddRange(s.Materials);owned.Add(s.Soft);owned.Add(s.Leaf);owned.Add(s.YarkinHybridShell);owned.Add(s.YarkinHybridPuff);owned.Add(s.YarkinHybridShard);owned.Add(s.YarkinHybridTexture);}
                foreach(var s in root.GetComponentsInChildren<PanicStormMotionV414>(true)){owned.Add(s.Funnel);owned.Add(s.Debris);owned.Add(s.CloudBase);owned.Add(s.GroundDust);}
                Object.DestroyImmediate(root);
                foreach(var value in owned.Distinct())if(value&&!EditorUtility.IsPersistent(value))Object.DestroyImmediate(value);
            }
            roots.Clear();
        }
        public static bool ValidateAndSave(string absoluteEvidenceDir)
        {
            if(string.IsNullOrWhiteSpace(absoluteEvidenceDir)||!Path.IsPathRooted(absoluteEvidenceDir))throw new ArgumentException("Absolute -visualFixEvidence directory required");
            Directory.CreateDirectory(absoluteEvidenceDir); report=new Report{utc=DateTime.UtcNow.ToString("o"),unity=Application.unityVersion};
            report.notes.Add("Headless Editor engine object/mesh/Avatar data only. No render framebuffer, native player, OS input, listening, human readability/comfort or CFD claim. Root owns bake/build/test identity.");
            try { Pavement(); Neighbours(); Flood(); report.complete=true; }
            catch(Exception e){report.error=e.ToString();Check("editor.exception",false,report.error);}
            finally{try{Cleanup();}catch(Exception e){report.error+="\ncleanup:"+e;Check("editor.cleanup",false,e.ToString());}}
            report.passed=report.complete&&string.IsNullOrEmpty(report.error)&&report.checks.All(c=>c.pass);
            File.WriteAllText(Path.Combine(absoluteEvidenceDir,"visual-fixes-headless-v58.json"),JsonUtility.ToJson(report,true),new UTF8Encoding(false));
            File.WriteAllLines(Path.Combine(absoluteEvidenceDir,"visual-fixes-headless-v58-checks.log"),report.checks.Select(c=>(c.pass?"PASS ":"FAIL ")+c.phase+"/"+c.id+" "+c.detail),new UTF8Encoding(false));
            return report.passed;
        }
        public static void Run()
        {
            int exit=1;try{var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-visualFixEvidence");if(i<0||i+1>=args.Length)throw new ArgumentException("-visualFixEvidence <absolute-directory> required");exit=ValidateAndSave(args[i+1])?0:1;}catch(Exception e){Debug.LogException(e);}EditorApplication.Exit(exit);
        }
    }
}
#endif
