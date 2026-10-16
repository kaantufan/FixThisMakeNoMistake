using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>
    /// Authored survivor workwear for the existing CC0 Quaternius humanoid.
    /// Dress once in the unanimated import/bind pose, then persist generated MeshFilters AND SkinnedMeshRenderer meshes/materials.
    /// SetTaskPose belongs in LateUpdate AFTER Animator evaluation. Empty task removes our last pose.
    /// No controller, root motion, movement, collider, gameplay state or source asset is changed.
    /// </summary>
    public static class PanicCharacterArt
    {
        const string Tag = "PanicWorkwear_V2";
        const float Tau = 6.28318530718f;
        static readonly ConditionalWeakTable<GameObject, PoseState> Poses = new ConditionalWeakTable<GameObject, PoseState>();

        public static void Dress(GameObject existingModel, Animator humanoid)
        {
            if (existingModel == null || existingModel.transform.Find(Tag) != null) return;
            Transform model = existingModel.transform;
            Transform hips = Bone(model, humanoid, HumanBodyBones.Hips, "pelvis");
            Transform chest = Bone(model, humanoid, HumanBodyBones.UpperChest, "spine_03");
            if (chest == null) chest = Bone(model, humanoid, HumanBodyBones.Chest, "spine_02");
            Transform neck = Bone(model, humanoid, HumanBodyBones.Neck, "neck_01");
            Transform head = Bone(model, humanoid, HumanBodyBones.Head, "Head");
            if (hips == null || chest == null || head == null) return;

            // Mark only after validating the skeleton; this survives prefab serialization/instantiation.
            new GameObject(Tag).transform.SetParent(model, false);
            var art = new Batch(model);
            Material jacket = Mat("Indigo canvas jacket", new Color(.115f, .155f, .205f), .13f);
            Material seams = Mat("Faded indigo seams", new Color(.225f, .27f, .31f), .12f);
            Material trousers = Mat("Olive ripstop trousers", new Color(.21f, .235f, .165f), .10f);
            Material olive = Mat("Olive pocket flaps", new Color(.275f, .285f, .205f), .10f);
            Material rust = Mat("Rust reinforcement", new Color(.35f, .205f, .135f), .17f);
            Material leather = Mat("Charcoal leather and rubber", new Color(.065f, .072f, .075f), .24f);
            Material metal = Mat("Brushed worn hardware", new Color(.36f, .365f, .34f), .34f, .55f);
            Material shirt = Mat("Warm grey undershirt", new Color(.31f, .31f, .285f), .08f);
            Material skin = Mat("Natural warm skin", new Color(.55f, .385f, .285f), .23f);
            Material hair = Mat("Soft charcoal hair", new Color(.07f, .057f, .047f), .12f);
            Material eyes = Mat("Dark brown eyes", new Color(.055f, .043f, .033f), .3f);
            // R2: author a continuous skinned workwear underlayer directly on the retained body.
            // Skin is restricted to actual head/neck and hands, so articulated shell overlap can
            // never reveal a naked torso/arm/leg. Imported mesh, weights and Avatar stay untouched.
            foreach (var r in existingModel.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                string n = r.name.ToLowerInvariant();
                bool eye = n.Contains("eye") && !n.Contains("brow"), browOrHair = n.Contains("brow") || n.Contains("hair");
                if (eye || browOrHair)
                {
                    var ms = r.sharedMaterials; for (int i = 0; i < ms.Length; i++) ms[i] = eye ? eyes : hair;
                    r.sharedMaterials = ms;
                }
                else SegmentBodyWorkwear(r, model, hips, neck, skin, jacket, trousers, leather);
                r.updateWhenOffscreen = true;
                r.shadowCastingMode = ShadowCastingMode.On;
            }

            Vector3 h = Local(model, hips), c = Local(model, chest), nck = neck != null ? Local(model, neck) : Local(model, head) - Vector3.up * .08f;
            Transform lu = Bone(model, humanoid, HumanBodyBones.LeftUpperArm, "upperarm_l");
            Transform ru = Bone(model, humanoid, HumanBodyBones.RightUpperArm, "upperarm_r");
            float halfWidth = lu != null && ru != null ? Mathf.Clamp(Mathf.Abs(Local(model, ru).x - Local(model, lu).x) * .5f + .043f, .24f, .33f) : .275f;
            float torsoDepth = halfWidth * .75f;
            float shoulderY = lu != null && ru != null ? (Local(model, lu).y + Local(model, ru).y) * .5f : nck.y - .12f;
            Vector3 centre = new Vector3(h.x, 0, (h.z + c.z) * .5f);

            // Overlapping, tapered ring sections give a tailored shoulder/waist silhouette.
            art.Rings("jacket body", chest, jacket, centre,
                new[] { h.y + .06f, h.y + .19f, shoulderY - .10f, shoulderY + .025f, Mathf.Max(nck.y - .004f, shoulderY + .035f) },
                new[] { halfWidth * .86f, halfWidth * .84f, halfWidth, halfWidth * .94f, .105f },
                new[] { torsoDepth * .92f, torsoDepth * .91f, torsoDepth, torsoDepth * .87f, .095f });
            art.Rings("jacket hem", hips, jacket, centre,
                new[] { h.y - .11f, h.y - .055f, h.y + .105f },
                new[] { halfWidth * .86f, halfWidth * .9f, halfWidth * .87f },
                new[] { torsoDepth * .92f, torsoDepth * .96f, torsoDepth * .94f });
            art.Rings("collar", chest, shirt, centre,
                new[] { nck.y - .052f, nck.y + .015f }, new[] { .103f, .082f }, new[] { .097f, .079f });
            float front = centre.z + torsoDepth + .008f;
            art.Box("front zip", chest, metal, new Vector3(centre.x, (h.y + .12f + nck.y - .07f) * .5f, front), new Vector3(.012f, nck.y - h.y - .19f, .012f));
            art.Box("zip pull", chest, metal, new Vector3(centre.x, nck.y - .10f, front + .012f), new Vector3(.025f, .036f, .012f));
            for (int side = -1; side <= 1; side += 2)
            {
                float px = centre.x + side * halfWidth * .50f;
                art.Box("chest pocket", chest, seams, new Vector3(px, shoulderY - .135f, front - .004f), new Vector3(.112f, .115f, .025f));
                art.Box("chest pocket flap", chest, jacket, new Vector3(px, shoulderY - .085f, front + .012f), new Vector3(.12f, .036f, .022f));
                art.Box("pocket snap", chest, metal, new Vector3(px, shoulderY - .085f, front + .027f), new Vector3(.012f, .012f, .008f));
                art.Box("hem welt pocket", hips, seams, new Vector3(centre.x + side * halfWidth * .48f, h.y + .005f, centre.z + torsoDepth * .96f), new Vector3(.12f, .022f, .018f));
            }
            art.Box("small woven preparedness patch", chest, rust, new Vector3(centre.x - halfWidth * .5f, shoulderY - .126f, front + .026f), new Vector3(.062f, .027f, .006f));

            // A closed pelvis garment covers the hip articulation underneath the jacket hem.
            art.Rings("trouser seat", hips, trousers, centre,
                new[] { h.y - .19f, h.y - .11f, h.y + .045f },
                new[] { halfWidth * .68f, halfWidth * .79f, halfWidth * .78f },
                new[] { .13f, .145f, .135f });
            art.Rings("belt", hips, leather, centre,
                new[] { h.y - .025f, h.y + .025f }, new[] { halfWidth * .905f, halfWidth * .905f }, new[] { torsoDepth * .98f, torsoDepth * .98f });
            art.Box("belt buckle", hips, metal, new Vector3(centre.x, h.y, centre.z + torsoDepth + .017f), new Vector3(.077f, .058f, .023f));
            art.Box("buckle inset", hips, leather, new Vector3(centre.x, h.y, centre.z + torsoDepth + .031f), new Vector3(.047f, .029f, .008f));
            art.Box("utility pouch", hips, rust, new Vector3(centre.x + halfWidth + .012f, h.y - .07f, centre.z + .012f), new Vector3(.105f, .16f, .10f));
            art.Box("pouch closure", hips, leather, new Vector3(centre.x + halfWidth + .012f, h.y - .01f, centre.z + .071f), new Vector3(.10f, .036f, .018f));
            art.Box("belt radio", hips, leather, new Vector3(centre.x - halfWidth - .012f, h.y - .045f, centre.z + .012f), new Vector3(.07f, .14f, .065f));
            art.Segment("radio antenna", hips, leather, new Vector3(centre.x - halfWidth - .012f, h.y + .015f, centre.z), new Vector3(centre.x - halfWidth - .012f, h.y + .13f, centre.z), .008f, .007f);

            DressSide(art, model, humanoid, true, jacket, seams, trousers, olive, rust, leather, metal);
            DressSide(art, model, humanoid, false, jacket, seams, trousers, olive, rust, leather, metal);
            Vector3 hp = Local(model, head);
            art.HairCap(head, hair, hp + new Vector3(0, .112f, -.006f));
            // A few broad clumps create a short swept silhouette instead of an untextured sphere.
            art.Box("swept forelock", head, hair, hp + new Vector3(-.035f, .225f, .045f), new Vector3(.15f, .06f, .12f), Quaternion.Euler(0, -12, -13));
            art.Box("side part", head, hair, hp + new Vector3(.075f, .18f, .015f), new Vector3(.045f, .12f, .15f), Quaternion.Euler(0, 0, -12));
            art.Flush();
        }

        static void DressSide(Batch art, Transform model, Animator a, bool left, Material jacket, Material seam, Material pants, Material olive, Material rust, Material leather, Material metal)
        {
            string s = left ? "_l" : "_r"; float sign = left ? -1 : 1;
            Transform upper = Bone(model, a, left ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm, "upperarm" + s);
            Transform lower = Bone(model, a, left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm, "lowerarm" + s);
            Transform hand = Bone(model, a, left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand, "hand" + s);
            Transform thigh = Bone(model, a, left ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg, "thigh" + s);
            Transform calf = Bone(model, a, left ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg, "calf" + s);
            Transform foot = Bone(model, a, left ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot, "foot" + s);
            if (upper != null && lower != null && hand != null)
            {
                Vector3 u = Local(model, upper), e = Local(model, lower), w = Local(model, hand);
                art.Segment("upper canvas sleeve" + s, upper, jacket, Vector3.LerpUnclamped(u, e, -.13f), Vector3.LerpUnclamped(u, e, 1.09f), .124f, .092f);
                art.Segment("forearm canvas sleeve" + s, lower, jacket, Vector3.LerpUnclamped(e, w, -.10f), Vector3.Lerp(e, w, .98f), .098f, .067f);
                art.Segment("stitched cuff" + s, lower, rust, Vector3.Lerp(e, w, .86f), Vector3.LerpUnclamped(e, w, 1.025f), .065f, .063f);
                art.Box("elbow reinforcement" + s, lower, seam, e + new Vector3(0, 0, -.063f), new Vector3(.105f, .12f, .025f));
                Transform finger = Named(model, "middle_01" + s);
                Vector3 end = finger != null ? Local(model, finger) : w + (w - e).normalized * .11f;
                art.Segment("fingerless work glove" + s, hand, leather, w, Vector3.LerpUnclamped(w, end, 1.11f), .055f, .052f, .72f);
                art.Segment("glove knuckle panel" + s, hand, rust, Vector3.Lerp(w, end, .56f) + new Vector3(0, .018f, 0), end + new Vector3(0, .018f, 0), .047f, .044f, .68f);
            }
            if (thigh == null || calf == null || foot == null) return;
            Vector3 t = Local(model, thigh), k = Local(model, calf), f = Local(model, foot);
            art.Segment("cargo thigh" + s, thigh, pants, Vector3.LerpUnclamped(t, k, -.12f), Vector3.LerpUnclamped(t, k, 1.05f), .135f, .102f, .93f);
            art.Segment("cargo lower leg" + s, calf, pants, Vector3.LerpUnclamped(k, f, -.065f), Vector3.Lerp(k, f, .91f), .105f, .076f, .94f);
            art.Box("reinforced knee" + s, calf, rust, k + new Vector3(0, -.035f, .083f), new Vector3(.125f, .145f, .038f));
            Vector3 pocket = Vector3.Lerp(t, k, .43f) + new Vector3(sign * .119f, 0, .009f);
            art.Box("cargo side pocket" + s, thigh, olive, pocket, new Vector3(.055f, .17f, .135f));
            art.Box("cargo flap" + s, thigh, pants, pocket + new Vector3(sign * .012f, .073f, 0), new Vector3(.06f, .038f, .145f));
            art.Box("cargo snap" + s, thigh, metal, pocket + new Vector3(sign * .043f, .07f, .038f), new Vector3(.009f, .013f, .013f));
            Transform toe = Bone(model, a, left ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes, "ball" + s);
            Vector3 fw = toe != null ? Local(model, toe) - f : Vector3.forward;
            fw.y = 0; if (fw.sqrMagnitude < .001f) fw = Vector3.forward; fw.Normalize();
            Quaternion bootRotation = Quaternion.LookRotation(fw, Vector3.up);
            Vector3 boot = f + fw * .065f;
            art.Box("thick rubber sole" + s, foot, leather, boot + Vector3.down * .065f, new Vector3(.18f, .055f, .32f), bootRotation);
            art.Box("leather work boot" + s, foot, rust, boot + Vector3.up * .005f, new Vector3(.168f, .13f, .30f), bootRotation);
            art.Box("rubber toe cap" + s, foot, leather, boot + fw * .105f - Vector3.up * .008f, new Vector3(.173f, .105f, .09f), bootRotation);
            art.Segment("boot shaft" + s, foot, rust, f - Vector3.up * .015f, f + Vector3.up * .18f, .085f, .087f, .89f);
            art.Segment("padded boot collar" + s, foot, leather, f + Vector3.up * .145f, f + Vector3.up * .19f, .092f, .092f, .9f);
            for (int i = 0; i < 3; i++)
                art.Box("boot lace" + s, foot, seam, f + fw * (.052f + i * .022f) + Vector3.up * (.079f - i * .017f), new Vector3(.095f, .009f, .009f), bootRotation);
        }

        /// <summary>Apply after Animator, every active work frame. Passing null/empty/idle/walk clears.</summary>
        public static void SetTaskPose(GameObject model, Animator animator, string task, float normalizedProgress)
        {
            if (model == null) return;
            PoseState st = Poses.GetValue(model, x => new PoseState(x.transform, animator));
            st.Restore();
            string job = (task ?? "").ToLowerInvariant();
            if (job.Length == 0 || job == "idle" || job == "walk" || job == "none") return;
            st.Capture();
            Transform root = model.transform;
            float phase = Time.time * 8.5f + Mathf.Clamp01(normalizedProgress) * Tau * 2;
            float wave = Mathf.Sin(phase), beat = Mathf.Sin(phase * .5f);
            bool ground = job.Contains("drain") || job.Contains("plant") || job.Contains("floor");
            bool fence = job.Contains("fence");
            bool hose = job.Contains("hose") || job.Contains("fire");
            float lean = ground ? 29f + 3f * beat : fence ? 13f + 4f * wave : hose ? 9f : 11f + 2f * wave;
            st.Lean(lean, wave * (ground ? 3 : 1.5f));
            Vector3 hip = st.Hips != null ? root.InverseTransformPoint(st.Hips.position) : new Vector3(0, .95f, 0);
            float height = ground ? hip.y - .35f + .035f * wave : hip.y + (hose ? .20f : .34f) + .025f * wave;
            float reach = ground ? .40f : hose ? .50f : .43f;
            Vector3 right = new Vector3(.16f, height, hip.z + reach + (fence ? .07f * wave : .015f * beat));
            Vector3 left = new Vector3(-.16f, height + (ground ? .025f : -.045f), hip.z + reach - .045f);
            if (job.Contains("drain")) { right.y += .09f * wave; left.y += .055f * wave; right.z += .09f * beat; left.z += .09f * beat; }
            if (job.Contains("plant")) { right.x += .025f * wave; left.z += .04f * beat; }
            st.Reach(false, root.TransformPoint(right), root.TransformPoint(new Vector3(.48f, hip.y + .12f, hip.z + .08f)));
            st.Reach(true, root.TransformPoint(left), root.TransformPoint(new Vector3(-.48f, hip.y + .12f, hip.z + .08f)));
            // Visible turning wrist and supporting hand while fasteners are torqued.
            if (st.RightHand != null && !hose)
                st.RightHand.rotation = Quaternion.AngleAxis(wave * (fence ? 20 : 32), root.forward) * st.RightHand.rotation;
            if (st.Head != null) st.Head.rotation = Quaternion.AngleAxis(ground ? 16 : 6, root.right) * st.Head.rotation;
            st.Commit();
        }

        public static void ClearTaskPose(GameObject model)
        {
            if (model != null && Poses.TryGetValue(model, out PoseState state)) state.Restore();
        }

        // Deliberately leaves controller/idle/walk evaluation to the owner. Supplied for API compatibility.
        public static void UpdateSecondary(GameObject model, float speed, float dt) { }

        /// <summary>Optional visual: child under right hand, local +Z is the tool working direction.</summary>
        public static GameObject CreateHandTool(string task)
        {
            string job = (task ?? "").ToLowerInvariant();
            var go = new GameObject("Panic work tool " + job);
            var b = new Batch(go.transform);
            Material grip = Mat("Tool rust grip", new Color(.35f, .19f, .115f), .19f);
            Material steel = Mat("Tool worn steel", new Color(.34f, .36f, .36f), .38f, .65f);
            if (job.Contains("plant") || job.Contains("drain"))
            {
                b.Segment("wood handle", go.transform, grip, new Vector3(0, 0, -.10f), new Vector3(0, 0, .22f), .022f, .018f);
                b.Box("spade blade", go.transform, steel, new Vector3(0, -.012f, .29f), new Vector3(.12f, .018f, .16f), Quaternion.Euler(-15, 0, 0));
            }
            else if (job.Contains("hose") || job.Contains("fire"))
            {
                b.Segment("nozzle", go.transform, steel, Vector3.zero, Vector3.forward * .22f, .035f, .025f);
                b.Box("nozzle grip", go.transform, grip, new Vector3(0, -.05f, .035f), new Vector3(.045f, .12f, .05f), Quaternion.Euler(-15, 0, 0));
            }
            else
            {
                b.Segment("driver grip", go.transform, grip, new Vector3(0, 0, -.07f), new Vector3(0, 0, .07f), .029f, .025f);
                b.Segment("driver shaft", go.transform, steel, new Vector3(0, 0, .07f), new Vector3(0, 0, .23f), .008f, .006f);
            }
            b.Flush(); return go;
        }

        /// <summary>
        /// Make a private, fully skinned material-region derivative of the licensed CC0 body.
        /// We preserve vertex streams, bindposes, weights, triangles and shape; only submesh indices
        /// and their materials change. Persist this renderer.sharedMesh before saving a prefab.
        /// </summary>
        static void SegmentBodyWorkwear(SkinnedMeshRenderer renderer, Transform model, Transform hips, Transform neck,
            Material skin, Material jacket, Material trousers, Material boots)
        {
            Mesh source = renderer.sharedMesh;
            if (source == null) return;
            Vector3[] vertices;
            BoneWeight[] weights;
            try { vertices = source.vertices; weights = source.boneWeights; }
            catch (Exception ex)
            {
                throw new InvalidOperationException("HomeV2 workwear requires readable body mesh during editor bake. Set source ModelImporter.isReadable=true, SaveAndReimport, then load source/Dress; no naked fallback is permitted.", ex);
            }
            Transform[] bones = renderer.bones;
            var groups = new int[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                string name = bones[i] != null ? bones[i].name.ToLowerInvariant() : "";
                if (name == "head" || name.StartsWith("neck")) groups[i] = 1;
                else if (name.StartsWith("hand") || name.StartsWith("thumb") || name.StartsWith("index") || name.StartsWith("middle") || name.StartsWith("ring") || name.StartsWith("pinky")) groups[i] = 2;
                else if (name.StartsWith("foot") || name.StartsWith("ball") || name.Contains("toe")) groups[i] = 4;
                else if (name.StartsWith("pelvis") || name.StartsWith("thigh") || name.StartsWith("calf")) groups[i] = 3;
            }
            var triangles = new[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
            float hipY = Local(model, hips).y, neckY = neck != null ? Local(model, neck).y : hipY + .58f;
            var scores = new float[5];
            int originalTriangles = 0;
            for (int sub = 0; sub < source.subMeshCount; sub++)
            {
                int[] indices = source.GetTriangles(sub);
                originalTriangles += indices.Length / 3;
                for (int t = 0; t + 2 < indices.Length; t += 3)
                {
                    int a = indices[t], b = indices[t + 1], c = indices[t + 2];
                    Array.Clear(scores, 0, scores.Length);
                    if (weights.Length == vertices.Length)
                    {
                        ScoreBoneWeight(weights[a], groups, scores);
                        ScoreBoneWeight(weights[b], groups, scores);
                        ScoreBoneWeight(weights[c], groups, scores);
                    }
                    Vector3 point = model.InverseTransformPoint(renderer.transform.TransformPoint((vertices[a] + vertices[b] + vertices[c]) / 3));
                    int region = 1; // Continuous indigo fabric is the safe default, never whole-body skin.
                    if ((scores[1] > 1.35f && point.y >= neckY - .008f) || scores[2] > 1.5f) region = 0;
                    else if (scores[4] > 1.05f) region = 3;
                    else if (scores[3] + scores[4] > 1.35f || (point.y < hipY + .018f && scores[2] < .9f)) region = 2;
                    triangles[region].Add(a); triangles[region].Add(b); triangles[region].Add(c);
                }
            }
            if (triangles[0].Count == 0)
                throw new InvalidOperationException("HomeV2 body region classification found no head/hands. Inspect renderer bones/readable weights before baking; refusing to recolor face as fabric.");
            var mesh = UnityEngine.Object.Instantiate(source);
            mesh.name = "PanicWear R2 continuous skinned body regions " + renderer.name;
            mesh.subMeshCount = 4;
            for (int i = 0; i < triangles.Length; i++) mesh.SetTriangles(triangles[i], i, false);
            mesh.RecalculateBounds();
            renderer.sharedMesh = mesh;
            renderer.sharedMaterials = new[] { skin, jacket, trousers, boots };
            Debug.Log("PANIC_WORKWEAR_R2 renderer=" + renderer.name + " sourceTriangles=" + originalTriangles
                + " skin=" + triangles[0].Count / 3 + " jacket=" + triangles[1].Count / 3
                + " trousers=" + triangles[2].Count / 3 + " boots=" + triangles[3].Count / 3);
        }
        static void ScoreBoneWeight(BoneWeight weight, int[] groups, float[] scores)
        {
            if ((uint)weight.boneIndex0 < (uint)groups.Length) scores[groups[weight.boneIndex0]] += weight.weight0;
            if ((uint)weight.boneIndex1 < (uint)groups.Length) scores[groups[weight.boneIndex1]] += weight.weight1;
            if ((uint)weight.boneIndex2 < (uint)groups.Length) scores[groups[weight.boneIndex2]] += weight.weight2;
            if ((uint)weight.boneIndex3 < (uint)groups.Length) scores[groups[weight.boneIndex3]] += weight.weight3;
        }

        static Transform Bone(Transform model, Animator a, HumanBodyBones id, string name)
        {
            if (a != null && a.avatar != null && a.isHuman)
            {
                Transform t = a.GetBoneTransform(id); if (t != null) return t;
            }
            return Named(model, name);
        }
        static Transform Named(Transform model, string name)
        {
            foreach (Transform t in model.GetComponentsInChildren<Transform>(true))
                if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase)) return t;
            return null;
        }
        static Vector3 Local(Transform model, Transform bone) { return model.InverseTransformPoint(bone.position); }
        static Material Mat(string name, Color color, float smoothness, float metallic = 0)
        {
            var m = new Material(Shader.Find("Standard")) { name = "PanicWear " + name, color = color };
            m.SetFloat("_Glossiness", smoothness); m.SetFloat("_Metallic", metallic); return m;
        }

        sealed class PoseState
        {
            readonly Transform Root, Chest, LeftUpper, LeftLower, LeftHand, RightUpper, RightLower;
            public readonly Transform Hips, Head, RightHand;
            readonly List<Transform> joints = new List<Transform>();
            readonly List<Quaternion> bases = new List<Quaternion>(), applied = new List<Quaternion>();
            bool posed;
            public PoseState(Transform root, Animator a)
            {
                Root = root;
                Hips = Bone(root, a, HumanBodyBones.Hips, "pelvis");
                Chest = Bone(root, a, HumanBodyBones.Chest, "spine_02");
                Head = Bone(root, a, HumanBodyBones.Head, "Head");
                LeftUpper = Bone(root, a, HumanBodyBones.LeftUpperArm, "upperarm_l");
                LeftLower = Bone(root, a, HumanBodyBones.LeftLowerArm, "lowerarm_l");
                LeftHand = Bone(root, a, HumanBodyBones.LeftHand, "hand_l");
                RightUpper = Bone(root, a, HumanBodyBones.RightUpperArm, "upperarm_r");
                RightLower = Bone(root, a, HumanBodyBones.RightLowerArm, "lowerarm_r");
                RightHand = Bone(root, a, HumanBodyBones.RightHand, "hand_r");
                foreach (Transform t in new[] { Chest, Head, LeftUpper, LeftLower, LeftHand, RightUpper, RightLower, RightHand })
                    if (t != null && !joints.Contains(t)) { joints.Add(t); bases.Add(t.localRotation); applied.Add(t.localRotation); }
            }
            public void Restore()
            {
                if (!posed) return;
                for (int i = 0; i < joints.Count; i++)
                    if (joints[i] != null && Quaternion.Angle(joints[i].localRotation, applied[i]) < .025f)
                        joints[i].localRotation = bases[i];
                posed = false;
            }
            public void Capture() { for (int i = 0; i < joints.Count; i++) bases[i] = joints[i].localRotation; }
            public void Commit() { for (int i = 0; i < joints.Count; i++) applied[i] = joints[i].localRotation; posed = true; }
            public void Lean(float degrees, float twist)
            {
                if (Chest != null) Chest.rotation = Quaternion.AngleAxis(twist, Root.up) * Quaternion.AngleAxis(degrees, Root.right) * Chest.rotation;
            }
            public void Reach(bool left, Vector3 target, Vector3 pole)
            {
                Transform u = left ? LeftUpper : RightUpper, l = left ? LeftLower : RightLower, hand = left ? LeftHand : RightHand;
                if (u == null || l == null || hand == null) return;
                Vector3 origin = u.position;
                float upper = Vector3.Distance(origin, l.position), lower = Vector3.Distance(l.position, hand.position);
                Vector3 delta = target - origin; float raw = delta.magnitude;
                if (raw < .0001f || upper < .0001f || lower < .0001f) return;
                float d = Mathf.Clamp(raw, Mathf.Abs(upper - lower) + .005f, upper + lower - .008f);
                Vector3 direction = delta / raw;
                Vector3 bend = Vector3.ProjectOnPlane(pole - origin, direction).normalized;
                if (bend.sqrMagnitude < .1f) bend = Vector3.ProjectOnPlane(-Root.up, direction).normalized;
                float x = (upper * upper + d * d - lower * lower) / (2 * d);
                float y = Mathf.Sqrt(Mathf.Max(0, upper * upper - x * x));
                Vector3 elbow = origin + direction * x + bend * y;
                u.rotation = Quaternion.FromToRotation(l.position - origin, elbow - origin) * u.rotation;
                l.rotation = Quaternion.FromToRotation(hand.position - l.position, origin + direction * d - l.position) * l.rotation;
            }
        }

        // Geometry is batched by bone/material: new closed meshes, outward triangles, UV0, normals,
        // tangents and bounds. Bone-local vertices preserve fitting after editor bake/instantiation.
        sealed class Batch
        {
            readonly Transform model;
            readonly Dictionary<string, Part> parts = new Dictionary<string, Part>();
            public Batch(Transform model) { this.model = model; }
            Part Get(Transform bone, Material m)
            {
                string key = bone.GetInstanceID() + ":" + m.GetInstanceID();
                if (!parts.TryGetValue(key, out Part p)) { p = new Part(bone, m, model); parts.Add(key, p); }
                return p;
            }
            public void Box(string name, Transform bone, Material m, Vector3 center, Vector3 size, Quaternion? rotation = null)
            {
                // Eight-sided chamfered prism, with bevelled corners visible on pockets/soles.
                Quaternion q = rotation ?? Quaternion.identity;
                var pts = new Vector3[16]; float bevel = Mathf.Min(size.x, size.z) * .16f;
                Vector2[] outline = { new Vector2(-size.x*.5f+bevel,-size.z*.5f), new Vector2(size.x*.5f-bevel,-size.z*.5f), new Vector2(size.x*.5f,-size.z*.5f+bevel), new Vector2(size.x*.5f,size.z*.5f-bevel), new Vector2(size.x*.5f-bevel,size.z*.5f), new Vector2(-size.x*.5f+bevel,size.z*.5f), new Vector2(-size.x*.5f,size.z*.5f-bevel), new Vector2(-size.x*.5f,-size.z*.5f+bevel) };
                for (int r = 0; r < 2; r++) for (int i = 0; i < 8; i++) pts[r * 8 + i] = center + q * new Vector3(outline[i].x, (r == 0 ? -.5f : .5f) * size.y, outline[i].y);
                Get(bone, m).ClosedRings(pts, 8, 2);
            }
            public void Rings(string name, Transform bone, Material m, Vector3 center, float[] ys, float[] xs, float[] zs)
            {
                const int sides = 12; var pts = new Vector3[ys.Length * sides];
                for (int r = 0; r < ys.Length; r++) for (int i = 0; i < sides; i++)
                { float angle = Tau * i / sides; pts[r * sides + i] = new Vector3(center.x + Mathf.Sin(angle) * xs[r], ys[r], center.z + Mathf.Cos(angle) * zs[r]); }
                // Reverse the angular order so it matches the box's counterclockwise XZ outline.
                for (int r = 0; r < ys.Length; r++) Array.Reverse(pts, r * sides, sides);
                Get(bone, m).ClosedRings(pts, sides, ys.Length);
            }
            public void Segment(string name, Transform bone, Material m, Vector3 start, Vector3 end, float r0, float r1, float flatten = 1)
            {
                Vector3 axis = end - start; if (axis.sqrMagnitude < .000001f) return;
                Quaternion q = Quaternion.FromToRotation(Vector3.up, axis.normalized);
                const int sides = 10; const int rings = 4; var pts = new Vector3[sides * rings];
                for (int r = 0; r < rings; r++)
                {
                    float f = r / 3f, radius = Mathf.Lerp(r0, r1, f) * (r == 0 || r == 3 ? .91f : 1);
                    for (int i = 0; i < sides; i++) { float angle = -Tau * i / sides; pts[r * sides + i] = Vector3.Lerp(start, end, f) + q * new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius * flatten); }
                }
                Get(bone, m).ClosedRings(pts, sides, rings);
            }
            public void HairCap(Transform bone, Material m, Vector3 center)
            {
                const int sides = 16, rings = 6; var pts = new Vector3[sides * rings];
                for (int r = 0; r < rings; r++) for (int i = 0; i < sides; i++)
                {
                    float a = -Tau * i / sides;
                    float theta = Mathf.Lerp(1.73f, .045f, r / (rings - 1f));
                    float back = Mathf.Max(0, -Mathf.Cos(a));
                    pts[r * sides + i] = center + new Vector3(Mathf.Sin(a) * Mathf.Sin(theta) * .118f, Mathf.Cos(theta) * .147f - back * (1-r/(rings-1f)) * .06f, Mathf.Cos(a) * Mathf.Sin(theta) * .119f);
                }
                Get(bone, m).ClosedRings(pts, sides, rings);
            }
            public void Flush()
            {
                foreach (Part p in parts.Values)
                {
                    var go = new GameObject("PanicWear_" + p.Material.name); go.transform.SetParent(p.Bone, false);
                    var mesh = new Mesh { name = "PanicWear authored " + p.Bone.name + " " + p.Material.name };
                    if (p.Vertices.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;
                    mesh.SetVertices(p.Vertices); mesh.SetTriangles(p.Indices, 0); mesh.SetUVs(0, p.UVs);
                    mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = p.Material;
                    r.shadowCastingMode = ShadowCastingMode.On; r.receiveShadows = true;
                }
            }
        }
        sealed class Part
        {
            public readonly Transform Bone; public readonly Material Material; readonly Transform Model;
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<Vector2> UVs = new List<Vector2>(); public readonly List<int> Indices = new List<int>();
            public Part(Transform bone, Material material, Transform model) { Bone = bone; Material = material; Model = model; }
            void Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                int start = Vertices.Count;
                Vertices.Add(Bone.InverseTransformPoint(Model.TransformPoint(a)));
                Vertices.Add(Bone.InverseTransformPoint(Model.TransformPoint(b)));
                Vertices.Add(Bone.InverseTransformPoint(Model.TransformPoint(c)));
                UVs.Add(new Vector2(0, 0)); UVs.Add(new Vector2(1, 0)); UVs.Add(new Vector2(1, 1));
                Indices.Add(start); Indices.Add(start + 1); Indices.Add(start + 2);
            }
            public void ClosedRings(Vector3[] points, int sides, int rings)
            {
                for (int r = 0; r < rings - 1; r++) for (int i = 0; i < sides; i++)
                {
                    int j = (i + 1) % sides;
                    Vector3 a = points[r * sides + i], b = points[r * sides + j], c = points[(r + 1) * sides + j], d = points[(r + 1) * sides + i];
                    Tri(a, c, b); Tri(a, d, c);
                }
                Vector3 bottom = Vector3.zero, top = Vector3.zero;
                for (int i = 0; i < sides; i++) { bottom += points[i]; top += points[(rings - 1) * sides + i]; }
                bottom /= sides; top /= sides;
                for (int i = 0; i < sides; i++) { int j = (i + 1) % sides; Tri(bottom, points[i], points[j]); Tri(top, points[(rings - 1) * sides + j], points[(rings - 1) * sides + i]); }
            }
        }
    }
}
