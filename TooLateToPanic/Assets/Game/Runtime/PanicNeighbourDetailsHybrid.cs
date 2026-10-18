using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TooLateToPanic
{
    public enum PanicNeighbourActionHybrid { Idle, Walk, Radio, KneelInspect, Directing }

    /// <summary>Owned cosmetic pose on an actual neighbour only. World still owns route, root and Speed.</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(900)]
    public sealed class PanicNeighbourDetailsHybrid : MonoBehaviour
    {
        PanicHomeWorld world;
        Animator animator;
        PanicNeighbourActionHybrid action;
        float clock, actionAge;
        public int Role { get; private set; }
        public PanicHomeWorld WorldOwner { get { return world; } }
        public Animator RetainedAnimator { get { return animator; } }
        public int PoseApplyCount { get; private set; }
        public int LastAppliedFrame { get; private set; } = -1;
        public PanicNeighbourActionHybrid Action { get { return action; } }

        public void Bind(PanicHomeWorld owner, Animator retainedAnimator, int role)
        {
            if (!owner || world && world != owner) return;
            world = owner; animator = retainedAnimator; Role = Mathf.Clamp(role, 0, 3);
            PanicNeighbourDetailsHybridArt.DressResponder(gameObject, animator, Role);
        }
        public void QueuePose(PanicNeighbourActionHybrid next, float phaseClock, float dt)
        {
            if (action != next) { PanicNeighbourDetailsHybridArt.ClearPose(gameObject); actionAge = 0; }
            action = next;
            clock = float.IsNaN(phaseClock) || float.IsInfinity(phaseClock) ? 0 : phaseClock;
            actionAge += float.IsNaN(dt) || float.IsInfinity(dt) ? 0 : Mathf.Clamp(dt, 0, .1f);
            if (next == PanicNeighbourActionHybrid.Idle || next == PanicNeighbourActionHybrid.Walk)
                PanicNeighbourDetailsHybridArt.ClearPose(gameObject);
        }
        void LateUpdate()
        {
            if (!world || !isActiveAndEnabled || action == PanicNeighbourActionHybrid.Idle || action == PanicNeighbourActionHybrid.Walk) return;
            // Clock and phase are queued by World.Update only. No independent time, Animator parameters,
            // SampleAnimation, controller, root transform or locomotion mutation lives here.
            PanicNeighbourDetailsHybridArt.ApplyPose(gameObject, animator, action, clock,
                Mathf.SmoothStep(0, 1, Mathf.Clamp01(actionAge / .35f)));
            PoseApplyCount++; LastAppliedFrame = Time.frameCount;
        }
        void OnDisable() { PanicNeighbourDetailsHybridArt.ClearPose(gameObject); }
        void OnDestroy() { PanicNeighbourDetailsHybridArt.ReleasePose(gameObject); world = null; animator = null; }
    }

    // Yarkin 834ee55 role art isolated from PanicCharacterArt's main survivor/UAL pose table.
    static class PanicNeighbourDetailsHybridArt
    {
        const string ResponderTag = "PanicNeighbourResponderWear_Hybrid";
        static readonly ConditionalWeakTable<GameObject, PoseState> Poses = new ConditionalWeakTable<GameObject, PoseState>();
        static PanicExteriorOwnedArt Owner(GameObject root)
        { return root.GetComponent<PanicExteriorOwnedArt>() ?? root.AddComponent<PanicExteriorOwnedArt>(); }
        static void RemoveCollider(Collider value)
        {
            if (!value) return;
            value.enabled = false;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value); else UnityEngine.Object.DestroyImmediate(value);
        }
        public static void ClearPose(GameObject model)
        {
            if (!model) return;
            if (Poses.TryGetValue(model, out var state)) state.Restore();
            var radio = FindRadioProp(model); if (radio) radio.gameObject.SetActive(false);
        }
        public static void ReleasePose(GameObject model)
        {
            if (ReferenceEquals(model, null)) return;
            ClearPose(model); Poses.Remove(model);
        }
        static Transform FindRadioProp(GameObject model)
        {
            if (!model) return null;
            foreach (var t in model.GetComponentsInChildren<Transform>(true))
            {
                if (!t) continue;
                if (t.name == "NeighbourHybridHandRadio") return t;
            }
            return null;
        }

        static Transform FindOrEnsureRadioProp(GameObject model, Transform rightHand, Transform chest)
        {
            var existing = FindRadioProp(model);
            if (existing) return existing;
            Transform parent = rightHand != null ? rightHand : chest;
            if (!parent) return null;

            // Metres and upright axes belong to the model, not the imported hand's rotated frame.
            // Bake into the hand once; the existing hand pose then carries this small real radio.
            Transform root = model.transform;
            Vector3 grip = root.InverseTransformPoint(parent.position);
            var mesh = new FittedCosmeticMesh(root, parent);
            mesh.Box(grip + new Vector3(.015f, .025f, .025f), new Vector3(.052f, .105f, .033f));
            mesh.Box(grip + new Vector3(.031f, .100f, .025f), new Vector3(.007f, .050f, .007f));
            var material = Mat("Neighbour compact hand radio", new Color(.075f, .08f, .09f), .24f);
            var owner = Owner(model); owner.Own(material);
            var go = mesh.Attach("NeighbourHybridHandRadio", material, owner);
            return go ? go.transform : null;
        }

        public static void ApplyPose(GameObject model, Animator humanoid, PanicNeighbourActionHybrid action, float time, float blendWeight)
        {
            if (!model) return;
            if (float.IsNaN(time) || float.IsInfinity(time)) time = 0f;
            blendWeight = Mathf.Clamp01(blendWeight);

            var st = Poses.GetValue(model, x => new PoseState(x.transform, humanoid));
            st.Restore();

            if (action == PanicNeighbourActionHybrid.Radio)
            {
                var radioProp = FindOrEnsureRadioProp(model, st.RightHand, st.Chest);
                if (radioProp) radioProp.gameObject.SetActive(true);
            }
            else
            {
                var existingRadio = FindRadioProp(model);
                if (existingRadio) existingRadio.gameObject.SetActive(false);
            }

            if (blendWeight <= 0.0001f || action == PanicNeighbourActionHybrid.Idle || action == PanicNeighbourActionHybrid.Walk)
            {
                st.Commit();
                return;
            }

            st.Capture();
            Transform root = model.transform;
            Vector3 hip = st.Hips != null ? root.InverseTransformPoint(st.Hips.position) : new Vector3(0, 0.95f, 0);

            if (action == PanicNeighbourActionHybrid.Radio)
            {
                Vector3 headPos = st.Head != null ? root.InverseTransformPoint(st.Head.position) : new Vector3(0, hip.y + 0.65f, 0);
                float vocalBob = Mathf.Sin(time * 6f) * 0.02f;
                Vector3 handGoal = root.TransformPoint(new Vector3(0.08f, headPos.y - 0.08f + vocalBob, headPos.z + 0.16f));
                Vector3 rightPole = root.TransformPoint(new Vector3(0.35f, hip.y + 0.35f, hip.z + 0.10f));
                st.ReachBlended(false, handGoal, rightPole, blendWeight);
                if (st.RightHand != null)
                {
                    st.RightHand.rotation = Quaternion.AngleAxis(-25f * blendWeight, root.right) * Quaternion.AngleAxis(35f * blendWeight, root.forward) * st.RightHand.rotation;
                }
                if (st.Head != null)
                {
                    st.Head.rotation = Quaternion.AngleAxis(8f * blendWeight, root.forward) * Quaternion.AngleAxis(5f * blendWeight, root.right) * st.Head.rotation;
                }
                Vector3 leftRelaxed = root.TransformPoint(new Vector3(-0.22f, hip.y + 0.10f, hip.z + 0.05f));
                st.ReachBlended(true, leftRelaxed, root.TransformPoint(new Vector3(-0.35f, hip.y + 0.10f, hip.z)), blendWeight * 0.5f);
            }
            else if (action == PanicNeighbourActionHybrid.KneelInspect)
            {
                st.LowerHips(0.44f, blendWeight);
                st.FlexLegsForKneeling(blendWeight);
                st.Lean(25f * blendWeight, 0f);
                float reachY = hip.y - 0.22f;
                Vector3 rightGoal = root.TransformPoint(new Vector3(0.14f, reachY, hip.z + 0.35f));
                Vector3 leftGoal = root.TransformPoint(new Vector3(-0.14f, reachY, hip.z + 0.35f));
                st.ReachBlended(false, rightGoal, root.TransformPoint(new Vector3(0.35f, reachY + 0.15f, hip.z + 0.10f)), blendWeight);
                st.ReachBlended(true, leftGoal, root.TransformPoint(new Vector3(-0.35f, reachY + 0.15f, hip.z + 0.10f)), blendWeight);
                if (st.Head != null)
                {
                    st.Head.rotation = Quaternion.AngleAxis(20f * blendWeight, root.right) * st.Head.rotation;
                }
            }
            else if (action == PanicNeighbourActionHybrid.Directing)
            {
                float sweep = Mathf.Sin(time * 2.5f);
                float yawAngle = sweep * 20f * blendWeight;
                st.Lean(0f, yawAngle);
                float extend = 0.40f + sweep * 0.15f;
                Vector3 rightDir = root.TransformPoint(new Vector3(0.28f + sweep * 0.18f, hip.y + 0.35f + sweep * 0.08f, hip.z + extend));
                Vector3 rightPole = root.TransformPoint(new Vector3(0.45f, hip.y + 0.40f, hip.z));
                st.ReachBlended(false, rightDir, rightPole, blendWeight);
                Vector3 leftHalt = root.TransformPoint(new Vector3(-0.32f, hip.y + 0.32f, hip.z + 0.38f));
                Vector3 leftPole = root.TransformPoint(new Vector3(-0.45f, hip.y + 0.30f, hip.z));
                st.ReachBlended(true, leftHalt, leftPole, blendWeight);
                if (st.Head != null)
                {
                    st.Head.rotation = Quaternion.AngleAxis(sweep * 12f * blendWeight, root.up) * st.Head.rotation;
                }
            }

            st.Commit();
        }

        public static void StripPhysics(GameObject root)
        {
            if (!root) return;

            // 1. Joint constraints severed FIRST to avoid PhysX dependency deadlocks
            foreach (var j in root.GetComponentsInChildren<Joint>(true))
            {
                if (!j) continue;
                try { j.connectedBody = null; } catch { }
                if (Application.isPlaying) UnityEngine.Object.Destroy(j);
                else UnityEngine.Object.DestroyImmediate(j);
            }

            // 2. Colliders disabled and destroyed SECOND
            foreach (var c in root.GetComponentsInChildren<Collider>(true))
            {
                if (!c) continue;
                c.enabled = false;
                if (Application.isPlaying) UnityEngine.Object.Destroy(c);
                else UnityEngine.Object.DestroyImmediate(c);
            }

            // 3. Rigidbodies neutralized and destroyed LAST
            foreach (var r in root.GetComponentsInChildren<Rigidbody>(true))
            {
                if (!r) continue;
                r.isKinematic = true;
                r.detectCollisions = false;
                if (Application.isPlaying) UnityEngine.Object.Destroy(r);
                else UnityEngine.Object.DestroyImmediate(r);
            }
        }

        public static Material CreateRoleMaterial(string name, Color color, float smoothness = 0.12f, float metallic = 0.0f)
        {
            var mat = new Material(Shader.Find("Standard"))
            {
                name = "PanicWear_" + name,
                color = color
            };
            mat.SetFloat("_Glossiness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            return mat;
        }

        static void ApplyRoleBaseMaterials(GameObject root, int role, PanicExteriorOwnedArt state)
        {
            if (root == null) return;
            int r = ((role % 4) + 4) % 4;

            var replacements = new Dictionary<Material, Material>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var original = mats[i];
                    if (original == null) continue;

                    if (!replacements.TryGetValue(original, out var newMat))
                    {
                        string n = original.name.ToLowerInvariant();
                        newMat = GetRoleReplacementMaterial(r, n, original);
                        replacements[original] = newMat;
                        if (state != null && newMat != null && newMat != original) state.Own(newMat);
                    }

                    if (newMat != null && mats[i] != newMat)
                    {
                        mats[i] = newMat;
                        changed = true;
                    }
                }
                if (changed) renderer.sharedMaterials = mats;
            }
        }

        static Material GetRoleReplacementMaterial(int role, string matName, Material original)
        {
            if (matName.Contains("jacket"))
            {
                switch (role)
                {
                    case 0: return CreateRoleMaterial("Police_Navy_Uniform", new Color(0.11f, 0.16f, 0.29f), 0.14f);
                    case 1: return CreateRoleMaterial("Paramedic_Teal_Scrubs", new Color(0.12f, 0.42f, 0.48f), 0.12f);
                    case 2: return CreateRoleMaterial("Firefighter_Anthracite_Turnout", new Color(0.14f, 0.13f, 0.12f), 0.10f);
                    case 3: default: return CreateRoleMaterial("Rescue_Orange_Jumpsuit", new Color(0.90f, 0.32f, 0.00f), 0.16f);
                }
            }
            if (matName.Contains("trouser"))
            {
                switch (role)
                {
                    case 0: return CreateRoleMaterial("Police_Navy_Trousers", new Color(0.11f, 0.16f, 0.29f), 0.14f);
                    case 1: return CreateRoleMaterial("Paramedic_Teal_Pants", new Color(0.12f, 0.42f, 0.48f), 0.12f);
                    case 2: return CreateRoleMaterial("Firefighter_Anthracite_BunkerPants", new Color(0.14f, 0.13f, 0.12f), 0.10f);
                    case 3: default: return CreateRoleMaterial("Rescue_Orange_JumpsuitLegs", new Color(0.90f, 0.32f, 0.00f), 0.16f);
                }
            }
            if (matName.Contains("pocket"))
            {
                switch (role)
                {
                    case 0: return CreateRoleMaterial("Police_Navy_Pocket", new Color(0.07f, 0.12f, 0.21f), 0.12f);
                    case 1: return CreateRoleMaterial("Paramedic_Teal_Pocket", new Color(0.09f, 0.33f, 0.37f), 0.12f);
                    case 2: return CreateRoleMaterial("Firefighter_Bunker_Pocket", new Color(0.10f, 0.10f, 0.09f), 0.10f);
                    case 3: default: return CreateRoleMaterial("Rescue_Slate_Pocket", new Color(0.18f, 0.19f, 0.21f), 0.12f);
                }
            }
            if (matName.Contains("seam"))
            {
                switch (role)
                {
                    case 0: return CreateRoleMaterial("Police_Navy_Seam", new Color(0.08f, 0.12f, 0.22f), 0.12f);
                    case 1: return CreateRoleMaterial("Paramedic_Teal_Seam", new Color(0.10f, 0.35f, 0.40f), 0.12f);
                    case 2: return CreateRoleMaterial("Firefighter_Yellow_Trim", new Color(0.96f, 0.77f, 0.09f), 0.40f);
                    case 3: default: return CreateRoleMaterial("Rescue_Reflective_Seam", new Color(0.89f, 0.91f, 0.94f), 0.65f);
                }
            }
            if (matName.Contains("reinforce") || matName.Contains("rust"))
            {
                switch (role)
                {
                    case 0: return CreateRoleMaterial("Police_Leather_Reinforce", new Color(0.07f, 0.08f, 0.09f), 0.26f);
                    case 1: return CreateRoleMaterial("Paramedic_Nitrile_Blue_GloveTrim", new Color(0.23f, 0.52f, 0.78f), 0.35f);
                    case 2: return CreateRoleMaterial("Firefighter_Bunker_BootsKnees", new Color(0.07f, 0.07f, 0.08f), 0.18f);
                    case 3: default: return CreateRoleMaterial("Rescue_Slate_AbrasionPads", new Color(0.18f, 0.19f, 0.21f), 0.12f);
                }
            }
            if (matName.Contains("leather"))
            {
                switch (role)
                {
                    case 0: return CreateRoleMaterial("Police_Black_DutyLeather", new Color(0.07f, 0.08f, 0.09f), 0.26f);
                    case 1: return CreateRoleMaterial("Paramedic_Clinical_BootsRubber", new Color(0.12f, 0.13f, 0.14f), 0.20f);
                    case 2: return CreateRoleMaterial("Firefighter_Heat_GauntletsSoles", new Color(0.07f, 0.07f, 0.08f), 0.18f);
                    case 3: default: return CreateRoleMaterial("Rescue_Nylon_HarnessWebbing", new Color(0.09f, 0.10f, 0.11f), 0.20f);
                }
            }
            if (matName.Contains("hardware"))
            {
                switch (role)
                {
                    case 0: return CreateRoleMaterial("Police_Silver_BadgeHardware", new Color(0.82f, 0.83f, 0.86f), 0.70f, 0.85f);
                    case 1: return CreateRoleMaterial("Paramedic_Medical_ChromeHardware", new Color(0.89f, 0.91f, 0.94f), 0.60f, 0.45f);
                    case 2: return CreateRoleMaterial("Firefighter_Brass_ValvesHardware", new Color(0.71f, 0.60f, 0.19f), 0.60f, 0.80f);
                    case 3: default: return CreateRoleMaterial("Rescue_Anodized_CarabinerHardware", new Color(0.58f, 0.64f, 0.72f), 0.70f, 0.85f);
                }
            }
            if (matName.Contains("undershirt"))
            {
                switch (role)
                {
                    case 0: return CreateRoleMaterial("Police_Dark_InnerShirt", new Color(0.05f, 0.07f, 0.09f), 0.10f);
                    case 1: return CreateRoleMaterial("Paramedic_Scrub_VNeck", new Color(0.10f, 0.35f, 0.40f), 0.10f);
                    case 2: return CreateRoleMaterial("Firefighter_Nomex_Balaclava", new Color(0.11f, 0.11f, 0.10f), 0.10f);
                    case 3: default: return CreateRoleMaterial("Rescue_Technical_Baselayer", new Color(0.12f, 0.16f, 0.23f), 0.10f);
                }
            }
            return original;
        }

        public static void DressResponder(GameObject model, Animator humanoid, int role)
        {
            if (!model || model.transform.Find(ResponderTag)) return;
            int roleIndex = ((role % 4) + 4) % 4;
            Transform root = model.transform;
            Transform head = Bone(root, humanoid, HumanBodyBones.Head, "Head");
            Transform hips = Bone(root, humanoid, HumanBodyBones.Hips, "pelvis");
            Transform chest = Bone(root, humanoid, HumanBodyBones.Chest, "spine_02");
            if (!head && !hips && !chest) return;
            var tag = new GameObject(ResponderTag); tag.transform.SetParent(root, false);
            var owner = Owner(model);
            ApplyRoleBaseMaterials(model, roleIndex, owner);

            // The previous cube caps, vests, crosses, belts and tanks used raw bone-local axes.
            // They became the sideways yellow/red/white slabs reported by the player. The fitted
            // retained clothes already cover the body: no extra torso/head/equipment slabs are added.
            DressIndividualHair(model, head, roleIndex, owner);
            // Avatar, skin weights, animation owner, locomotion, colliders and World stay untouched.
        }

        static void DressIndividualHair(GameObject model, Transform head, int style, PanicExteriorOwnedArt owner)
        {
            if (!head) return;
            var inheritedCaps = new List<MeshRenderer>();
            foreach (var renderer in head.GetComponentsInChildren<MeshRenderer>(true))
            {
                // SurvivorV2's fitted cap is a separate authored mesh child. Never disable the
                // SkinnedMeshRenderer carrying the face, brows, eyes or the actual humanoid head.
                if (!renderer.name.StartsWith("PanicWear_", StringComparison.Ordinal)) continue;
                if (renderer.name.IndexOf("short dark hair and beard", StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (renderer.GetComponent<MeshFilter>()) inheritedCaps.Add(renderer);
            }
            if (!TryHeadBounds(model.transform, model, head, out var bounds, out var headPoints))
            {
                Debug.LogWarning("NPC_APPEARANCE_HEAD_FIT_UNAVAILABLE " + model.name);
                return; // Keep the existing cap if the actual retained head cannot be fitted safely.
            }
            string[] names = { "Short side part", "Medium wavy sweep", "Close textured crop", "Swept back low bun" };
            Color[] colors = { new Color(.105f,.077f,.053f), new Color(.17f,.087f,.045f),
                new Color(.29f,.275f,.245f), new Color(.06f,.047f,.038f) };
            var material = Mat("Neighbour hair " + names[style], colors[style], .16f);
            var mesh = new FittedCosmeticMesh(model.transform, head);
            mesh.Hair(bounds, headPoints, style);
            if (style == 3)
            {
                // A soft knot intersects the swept nape shell; it is sized from the real head,
                // not a cube or an object-scale multiplier in the imported bone frame.
                Vector3 knot = new Vector3(bounds.center.x, bounds.min.y + bounds.size.y*.71f,
                    bounds.min.z - bounds.size.z*.16f);
                mesh.Ellipsoid(knot, Vector3.Scale(bounds.size, new Vector3(.20f,.155f,.24f)));
            }
            owner.Own(material);
            var hair = mesh.Attach("NeighbourHair_" + style + "_" + names[style], material, owner);
            if (hair) foreach (var cap in inheritedCaps) cap.enabled = false;
        }

        static bool TryHeadBounds(Transform root, GameObject model, Transform head, out Bounds bounds, out List<Vector3> headPoints)
        {
            // Bind is called once directly after cloning SurvivorV2, before the animation tick.
            // This is the retained head's actual weighted reference geometry, in model space.
            bounds = new Bounds(); headPoints = new List<Vector3>(); bool found = false;
            foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                string name = renderer.name.ToLowerInvariant();
                if (name.Contains("eye") || name.Contains("brow") || name.Contains("hair")) continue;
                var mesh = renderer.sharedMesh;
                if (!mesh || !mesh.isReadable) continue;
                var vertices = mesh.vertices; var weights = mesh.boneWeights; var bones = renderer.bones;
                for (int i=0; i<vertices.Length && i<weights.Length; i++)
                {
                    var w = weights[i]; float influence = 0;
                    if (w.boneIndex0 < bones.Length && bones[w.boneIndex0] == head) influence += w.weight0;
                    if (w.boneIndex1 < bones.Length && bones[w.boneIndex1] == head) influence += w.weight1;
                    if (w.boneIndex2 < bones.Length && bones[w.boneIndex2] == head) influence += w.weight2;
                    if (w.boneIndex3 < bones.Length && bones[w.boneIndex3] == head) influence += w.weight3;
                    if (influence < .35f) continue;
                    Vector3 p = root.InverseTransformPoint(renderer.transform.TransformPoint(vertices[i]));
                    headPoints.Add(p);
                    if (!found) { bounds = new Bounds(p, Vector3.zero); found = true; } else bounds.Encapsulate(p);
                }
            }
            return found && bounds.size.x > .05f && bounds.size.y > .08f && bounds.size.z > .05f;
        }

        // Cosmetic geometry uses one explicit model-to-bone conversion for every vertex. This
        // cancels imported bone rotations/scales and model/world rotation at the moment of fitting.
        // Children have identity TRS; their only motion comes from the retained parent bone.
        sealed class FittedCosmeticMesh
        {
            readonly Transform root, bone;
            readonly List<Vector3> vertices = new List<Vector3>();
            readonly List<Vector2> uvs = new List<Vector2>();
            readonly List<int> triangles = new List<int>();
            public FittedCosmeticMesh(Transform root, Transform bone) { this.root=root; this.bone=bone; }
            void Vertex(Vector3 position, Vector2 uv) { vertices.Add(position); uvs.Add(uv); }
            void Tri(int a,int b,int c) { triangles.Add(a); triangles.Add(b); triangles.Add(c); }
            public void Hair(Bounds h, List<Vector3> headPoints, int style)
            {
                const int sides=32, rings=9;
                int first=vertices.Count;
                float width=h.size.x, height=h.size.y, depth=h.size.z;
                for (int ring=0; ring<rings; ring++)
                {
                    float v=ring/(float)rings, theta=(1-v)*Mathf.PI*.5f;
                    float radial=Mathf.Sin(theta), rise=Mathf.Cos(theta);
                    for (int side=0; side<sides; side++)
                    {
                        float angle=side*Mathf.PI*2/sides, sx=Mathf.Sin(angle), cz=Mathf.Cos(angle);
                        float front=Mathf.Max(0,cz), back=Mathf.Max(0,-cz), flank=Mathf.Abs(sx);
                        float line=.76f+.055f*front-.075f*back, crown=1.035f;
                        float thickness=style==1?.070f:style==2?.014f:style==3?.032f:.025f;
                        float offsetX=0, volume=0;
                        if (style==0)
                        {
                            // Short clipped sides, asymmetric swept crown and a shallow off-centre part.
                            line=.77f+.047f*front-.072f*back+.012f*flank;
                            crown=1.055f;
                            volume=.070f*Mathf.Max(0,-sx)*radial*rise;
                            float x=sx*radial;
                            volume-=.027f*Mathf.Exp(-(x-.27f)*(x-.27f)/.008f)*rise*front;
                            offsetX=-width*.085f*rise*rise;
                        }
                        else if (style==1)
                        {
                            // Longer rippling silhouette. Wavy locks are part of the scalp shell.
                            line=.75f+.042f*front-.13f*back-.022f*flank;
                            crown=1.10f;
                            float wave=Mathf.Sin(angle*5+v*8)*.5f+Mathf.Cos(angle*3-v*5)*.5f;
                            volume=.032f*wave*radial;
                            thickness+=.018f*wave*radial;
                            offsetX=width*.04f*rise;
                        }
                        else if (style==2)
                        {
                            // A close salt-and-pepper crop with a higher/receding front hairline.
                            line=.78f+.065f*front+.026f*flank*front-.035f*back;
                            crown=1.018f;
                            volume=.008f*Mathf.Sin(angle*9+v*14)*radial*rise;
                        }
                        else
                        {
                            // Brushed-back crown with longer fitted sides/nape, ending in the low bun.
                            line=.77f+.067f*front-.17f*back-.014f*flank;
                            crown=1.052f;
                            volume=.013f*Mathf.Cos(angle*7)*radial*rise;
                        }
                        float y=h.min.y+height*(Mathf.Lerp(line,crown,rise)+volume);
                        // Whole-head bounds include the ears and nose: using those as a fixed
                        // crown radius was what made the inherited cap read as a broad bowl.
                        // Fit each shell height against the actual upper skull envelope instead.
                        float skinY=Mathf.Min(h.max.y-height*.018f,y-height*(crown-1)-height*thickness);
                        Bounds scalp=UpperSkullEnvelope(headPoints,skinY,h);
                        float rx=scalp.extents.x+width*thickness;
                        float rz=scalp.extents.z+depth*thickness;
                        Vertex(new Vector3(scalp.center.x+sx*rx+offsetX,y,
                            scalp.center.z+cz*rz),new Vector2(side/(float)sides,v));
                    }
                }
                for (int ring=0; ring<rings-1; ring++) for (int side=0; side<sides; side++)
                {
                    int next=(side+1)%sides,a=first+ring*sides+side,b=first+ring*sides+next,
                        c=first+(ring+1)*sides+next,d=first+(ring+1)*sides+side;
                    Tri(a,b,c); Tri(a,c,d);
                }
                int top=vertices.Count;
                float topY=style==1?1.10f:style==2?1.018f:style==0?1.055f:1.052f;
                float topX=style==0?-width*.085f:style==1?width*.04f:0;
                Bounds tip=UpperSkullEnvelope(headPoints,h.max.y-height*.018f,h);
                Vertex(new Vector3(tip.center.x+topX,h.min.y+height*topY,tip.center.z),new Vector2(.5f,1));
                for(int side=0;side<sides;side++) Tri(first+(rings-1)*sides+side,first+(rings-1)*sides+(side+1)%sides,top);
                int bottom=vertices.Count; Vector3 center=Vector3.zero;
                for(int side=0;side<sides;side++) center+=vertices[first+side];
                Vertex(center/sides,new Vector2(.5f,0));
                for(int side=0;side<sides;side++) Tri(bottom,first+(side+1)%sides,first+side);
            }
            static Bounds UpperSkullEnvelope(List<Vector3> points,float minimumY,Bounds fallback)
            {
                Bounds envelope=new Bounds(); bool found=false;
                foreach(Vector3 point in points)
                {
                    if(point.y<minimumY) continue;
                    if(!found) { envelope=new Bounds(point,Vector3.zero); found=true; }
                    else envelope.Encapsulate(point);
                }
                return found?envelope:fallback;
            }
            public void Ellipsoid(Vector3 center, Vector3 radii)
            {
                const int sides=20, rings=11; int start=vertices.Count;
                for(int ring=0;ring<=rings;ring++) for(int side=0;side<sides;side++)
                {
                    float polar=Mathf.PI*ring/rings,angle=Mathf.PI*2*side/sides;
                    Vertex(center+Vector3.Scale(radii,new Vector3(Mathf.Sin(polar)*Mathf.Sin(angle),
                        -Mathf.Cos(polar),Mathf.Sin(polar)*Mathf.Cos(angle))),new Vector2(side/(float)sides,ring/(float)rings));
                }
                for(int ring=0;ring<rings;ring++) for(int side=0;side<sides;side++)
                {
                    int next=(side+1)%sides,a=start+ring*sides+side,b=start+ring*sides+next,
                        c=start+(ring+1)*sides+next,d=start+(ring+1)*sides+side;
                    if(ring>0) Tri(a,b,c); if(ring<rings-1) Tri(a,c,d);
                }
            }
            public void Box(Vector3 center, Vector3 size)
            {
                Vector3 e=size*.5f;
                Vector3[] p={new Vector3(-e.x,-e.y,-e.z),new Vector3(e.x,-e.y,-e.z),
                    new Vector3(e.x,e.y,-e.z),new Vector3(-e.x,e.y,-e.z),new Vector3(-e.x,-e.y,e.z),
                    new Vector3(e.x,-e.y,e.z),new Vector3(e.x,e.y,e.z),new Vector3(-e.x,e.y,e.z)};
                int[] faces={0,3,2,1,4,5,6,7,0,4,7,3,1,2,6,5,3,7,6,2,0,1,5,4};
                for(int face=0;face<6;face++)
                {
                    int n=vertices.Count;
                    for(int i=0;i<4;i++) Vertex(center+p[faces[face*4+i]],new Vector2(i==1||i==2?1:0,i>=2?1:0));
                    Tri(n,n+1,n+2); Tri(n,n+2,n+3);
                }
            }
            public GameObject Attach(string name, Material material, PanicExteriorOwnedArt owner)
            {
                if(!bone || vertices.Count==0) return null;
                for(int i=0;i<vertices.Count;i++) vertices[i]=bone.InverseTransformPoint(root.TransformPoint(vertices[i]));
                var mesh=new Mesh { name="NPC fitted cosmetic "+name };
                mesh.SetVertices(vertices); mesh.SetUVs(0,uvs); mesh.SetTriangles(triangles,0);
                mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
                var go=new GameObject(name); go.transform.SetParent(bone,false);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.AddComponent<MeshRenderer>(); renderer.sharedMaterial=material;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On; renderer.receiveShadows=true;
                owner.Own(mesh); return go;
            }
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

        static Material Mat(string name, Color color, float smoothness, float metallic = 0)
        {
            var m = new Material(Shader.Find("Standard")) { name = "PanicWear " + name, color = color };
            m.SetFloat("_Glossiness", smoothness); m.SetFloat("_Metallic", metallic); return m;
        }

        sealed class PoseState
        {
            public readonly Transform Root, Chest, LeftUpper, LeftLower, LeftHand, RightUpper, RightLower;
            public readonly Transform Hips, Head, RightHand, LeftThigh, RightThigh, LeftCalf, RightCalf;
            readonly List<Transform> joints = new List<Transform>();
            readonly List<Quaternion> bases = new List<Quaternion>(), applied = new List<Quaternion>();
            Vector3 hipBaseLocalPos, hipAppliedLocalPos;
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
                LeftThigh = Bone(root, a, HumanBodyBones.LeftUpperLeg, "thigh_l");
                RightThigh = Bone(root, a, HumanBodyBones.RightUpperLeg, "thigh_r");
                LeftCalf = Bone(root, a, HumanBodyBones.LeftLowerLeg, "calf_l");
                RightCalf = Bone(root, a, HumanBodyBones.RightLowerLeg, "calf_r");
                foreach (Transform t in new[] { Chest, Head, LeftUpper, LeftLower, LeftHand, RightUpper, RightLower, RightHand, LeftThigh, RightThigh, LeftCalf, RightCalf })
                    if (t != null && !joints.Contains(t)) { joints.Add(t); bases.Add(t.localRotation); applied.Add(t.localRotation); }
                if (Hips != null) { hipBaseLocalPos = Hips.localPosition; hipAppliedLocalPos = Hips.localPosition; }
            }
            public void Restore()
            {
                if (!posed) return;
                for (int i = 0; i < joints.Count; i++)
                    if (joints[i] != null && Quaternion.Angle(joints[i].localRotation, applied[i]) < .025f)
                        joints[i].localRotation = bases[i];
                if (Hips != null && Vector3.Distance(Hips.localPosition, hipAppliedLocalPos) < .025f)
                    Hips.localPosition = hipBaseLocalPos;
                posed = false;
            }
            public void Capture()
            {
                for (int i = 0; i < joints.Count; i++)
                    if (joints[i] != null) bases[i] = joints[i].localRotation;
                if (Hips != null) hipBaseLocalPos = Hips.localPosition;
            }
            public void Commit()
            {
                for (int i = 0; i < joints.Count; i++)
                    if (joints[i] != null) applied[i] = joints[i].localRotation;
                if (Hips != null) hipAppliedLocalPos = Hips.localPosition;
                posed = true;
            }
            public void LowerHips(float yOffset, float weight)
            {
                if (Hips != null)
                {
                    Hips.localPosition = hipBaseLocalPos - Vector3.up * (yOffset * Mathf.Clamp01(weight));
                }
            }
            public void FlexLegsForKneeling(float weight)
            {
                float w = Mathf.Clamp01(weight);
                if (LeftThigh != null) LeftThigh.localRotation = Quaternion.AngleAxis(-65f * w, Vector3.right) * LeftThigh.localRotation;
                if (LeftCalf != null) LeftCalf.localRotation = Quaternion.AngleAxis(110f * w, Vector3.right) * LeftCalf.localRotation;
                if (RightThigh != null) RightThigh.localRotation = Quaternion.AngleAxis(-85f * w, Vector3.right) * RightThigh.localRotation;
                if (RightCalf != null) RightCalf.localRotation = Quaternion.AngleAxis(120f * w, Vector3.right) * RightCalf.localRotation;
            }
            public void Lean(float degrees, float twist)
            {
                if (Chest != null) Chest.rotation = Quaternion.AngleAxis(twist, Root.up) * Quaternion.AngleAxis(degrees, Root.right) * Chest.rotation;
            }
            public void ReachBlended(bool left, Vector3 target, Vector3 pole, float strength)
            {
                Transform hand = left ? LeftHand : RightHand;
                if (hand == null) return;
                Reach(left, Vector3.Lerp(hand.position, target, Mathf.Clamp01(strength)), pole);
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
    }
}
