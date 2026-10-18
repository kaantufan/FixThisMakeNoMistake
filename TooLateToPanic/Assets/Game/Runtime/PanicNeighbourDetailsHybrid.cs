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

            Transform parent = rightHand != null ? rightHand : (chest != null ? chest : model.transform);
            var go = new GameObject("NeighbourHybridHandRadio");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0.04f, 0.05f, 0.02f);
            go.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            go.transform.localScale = new Vector3(0.06f, 0.12f, 0.04f);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "RadioBody";
            cube.transform.SetParent(go.transform, false);
            cube.transform.localScale = Vector3.one;
            var col = cube.GetComponent<Collider>();
            RemoveCollider(col);
            var mr = cube.GetComponent<MeshRenderer>();
            if (mr)
            {
                mr.sharedMaterial = Mat("EmergencyRadioMat", new Color(0.12f, 0.12f, 0.14f), 0.3f);
                Owner(model).Own(mr.sharedMaterial);
            }
            var ant = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ant.name = "RadioAntenna";
            ant.transform.SetParent(go.transform, false);
            ant.transform.localPosition = new Vector3(0.3f, 0.9f, 0f);
            ant.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
            var antCol = ant.GetComponent<Collider>();
            RemoveCollider(antCol);
            var antMr = ant.GetComponent<MeshRenderer>();
            if (antMr)
            {
                antMr.sharedMaterial = Mat("EmergencyAntennaMat", new Color(0.2f, 0.2f, 0.2f), 0.1f);
                Owner(model).Own(antMr.sharedMaterial);
            }
            return go.transform;
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

        static GameObject AttachCuratedOrProceduralProp(string assetName, Transform parentBone, Vector3 localPos, Quaternion localRot, Vector3 localScale, string propName, PrimitiveType fallbackPrimitive, Material fallbackMat, PanicExteriorOwnedArt state)
        {
            if (parentBone == null) return null;
            GameObject prop = null;
            try
            {
                if (!string.IsNullOrEmpty(assetName))
                    prop = PanicCuratedAssetArt.CreateDisplay(assetName, Mathf.Max(localScale.x, Mathf.Max(localScale.y, localScale.z)));
            }
            catch { }

            if (!prop)
            {
                prop = GameObject.CreatePrimitive(fallbackPrimitive);
                var col = prop.GetComponent<Collider>();
                if (col)
                {
                    col.enabled = false;
                    if (Application.isPlaying) UnityEngine.Object.Destroy(col);
                    else UnityEngine.Object.DestroyImmediate(col);
                }
                var mr = prop.GetComponent<MeshRenderer>();
                if (mr && fallbackMat) mr.sharedMaterial = fallbackMat;
            }
            else
            {
                StripPhysics(prop);
            }

            prop.name = propName;
            prop.transform.SetParent(parentBone, false);
            prop.transform.localPosition = localPos;
            prop.transform.localRotation = localRot;
            prop.transform.localScale = localScale;

            // Imported/shared materials stay with their original resource owner; explicit fallback materials are already registered.


            return prop;
        }

        public static void DressResponder(GameObject model, Animator humanoid, int role)
        {
            if (model == null) return;

            // Strict idempotency: prevent duplicate dressing
            if (model.transform.Find(ResponderTag) != null) return;

            // Modular role normalization for negative or out-of-range indices (T2_R3_1)
            int roleIndex = ((role % 4) + 4) % 4;

            // Resolve skeleton bones defensively (T2_R3_3)
            Transform root = model.transform;
            Transform hips = Bone(root, humanoid, HumanBodyBones.Hips, "pelvis");
            Transform head = Bone(root, humanoid, HumanBodyBones.Head, "Head");
            Transform chest = Bone(root, humanoid, HumanBodyBones.Chest, "spine_02");
            if (chest == null) chest = Bone(root, humanoid, HumanBodyBones.UpperChest, "spine_03");
            Transform leftHand = Bone(root, humanoid, HumanBodyBones.LeftHand, "hand_l");
            Transform rightHand = Bone(root, humanoid, HumanBodyBones.RightHand, "hand_r");

            // Graceful fallback for unrigged models (T2_R3_2)
            if (hips == null && head == null && chest == null) return;

            // Tag model to guarantee idempotency
            var tagObj = new GameObject(ResponderTag);
            tagObj.transform.SetParent(root, false);

            var state = model.GetComponent<PanicExteriorOwnedArt>() ?? model.AddComponent<PanicExteriorOwnedArt>();

            // 1. Whole-body PBR material retinting across all slots (T1_R3_5)
            ApplyRoleBaseMaterials(model, roleIndex, state);

            // 2. Dress role-specific headgear, torso kit, and equipment sets
            switch (roleIndex)
            {
                case 0: // Police: cap/badge, belt, radio, vest (T1_R3_1)
                    DressPolice(root, head, chest, hips, rightHand, state);
                    break;
                case 1: // Paramedic: med cap, first aid kit bag, cross vest, nitrile gloves (T1_R3_2)
                    DressParamedic(root, head, chest, hips, leftHand, rightHand, state);
                    break;
                case 2: // Firefighter: helmet, turnout bunker coat, scba air tank cylinder (T1_R3_3)
                    DressFirefighter(root, head, chest, hips, state);
                    break;
                case 3: // Rescue: rescue helmet headlamp jumpsuit, safety harness, backpack, radio (T1_R3_4)
                    DressRescue(root, head, chest, hips, rightHand, state);
                    break;
            }

            // Retained NPC physics, Avatar, controller and root-motion configuration remain factory-owned.

        }

        static void DressPolice(Transform root, Transform head, Transform chest, Transform hips, Transform rightHand, PanicExteriorOwnedArt state)
        {
            var navyMat = CreateRoleMaterial("Police_CapNavy", new Color(0.11f, 0.16f, 0.29f), 0.14f);
            var visorMat = CreateRoleMaterial("Police_CapVisorBlack", new Color(0.04f, 0.05f, 0.05f), 0.85f);
            var silverMat = CreateRoleMaterial("Police_SilverBadge", new Color(0.82f, 0.83f, 0.86f), 0.70f, 0.85f);
            var vestMat = CreateRoleMaterial("Police_HiVisLime", new Color(0.78f, 0.90f, 0.16f), 0.25f);
            var leatherMat = CreateRoleMaterial("Police_DutyLeather", new Color(0.07f, 0.08f, 0.09f), 0.26f);
            var radioMat = CreateRoleMaterial("Police_RadioDark", new Color(0.12f, 0.12f, 0.14f), 0.30f);

            state.Own(navyMat);
            state.Own(visorMat);
            state.Own(silverMat);
            state.Own(vestMat);
            state.Own(leatherMat);
            state.Own(radioMat);

            // Head: Peaked cap with silver badge (matches 'cap', 'peaked', 'badge')
            if (head != null)
            {
                var cap = AttachCuratedOrProceduralProp(null, head, new Vector3(0, 0.12f, 0.04f), Quaternion.Euler(6f, 0, 0), new Vector3(0.24f, 0.08f, 0.26f), "Police_PeakedCap_Badge", PrimitiveType.Cube, navyMat, state);
                if (cap != null)
                {
                    var visor = AttachCuratedOrProceduralProp(null, cap.transform, new Vector3(0, -0.02f, 0.14f), Quaternion.Euler(22f, 0, 0), new Vector3(0.22f, 0.02f, 0.10f), "Peaked_Visor", PrimitiveType.Cube, visorMat, state);
                    var badge = AttachCuratedOrProceduralProp(null, cap.transform, new Vector3(0, 0.02f, 0.13f), Quaternion.identity, new Vector3(0.06f, 0.06f, 0.02f), "Police_Badge", PrimitiveType.Cube, silverMat, state);
                }
            }

            // Chest: Hi-vis lime reflective vest (matches 'vest')
            if (chest != null)
            {
                AttachCuratedOrProceduralProp(null, chest, new Vector3(0, 0.05f, 0), Quaternion.identity, new Vector3(0.34f, 0.30f, 0.24f), "Police_HiVisVest", PrimitiveType.Cube, vestMat, state);
            }

            // Hips: Tactical duty belt with holster and pouches (matches 'duty', 'belt')
            if (hips != null)
            {
                var belt = AttachCuratedOrProceduralProp(null, hips, new Vector3(0, 0.02f, 0), Quaternion.identity, new Vector3(0.36f, 0.08f, 0.26f), "Police_DutyBelt", PrimitiveType.Cube, leatherMat, state);
                if (belt != null)
                {
                    AttachCuratedOrProceduralProp(null, belt.transform, new Vector3(0, 0, 0.135f), Quaternion.identity, new Vector3(0.08f, 0.06f, 0.02f), "Duty_Buckle", PrimitiveType.Cube, silverMat, state);
                    AttachCuratedOrProceduralProp(null, belt.transform, new Vector3(0.18f, -0.04f, 0.02f), Quaternion.identity, new Vector3(0.08f, 0.16f, 0.08f), "Duty_Holster", PrimitiveType.Cube, leatherMat, state);
                    AttachCuratedOrProceduralProp(null, belt.transform, new Vector3(-0.18f, 0, 0.02f), Quaternion.identity, new Vector3(0.08f, 0.08f, 0.06f), "Duty_Pouch", PrimitiveType.Cube, leatherMat, state);
                }
            }

            // Chest / Hand: Walkie-talkie radio prop (matches 'walkie', 'radio')
            Transform radioParent = chest != null ? chest : root;
            var radio = AttachCuratedOrProceduralProp("EmergencyRadio", radioParent, new Vector3(-0.16f, 0.14f, 0.08f), Quaternion.Euler(15f, 0, 0), new Vector3(0.06f, 0.14f, 0.04f), "Police_WalkieTalkie_Radio_Prop", PrimitiveType.Cube, radioMat, state);
            if (radio != null) radio.SetActive(false); // Inactive until Radio action (T2_R6_4)
        }

        static void DressParamedic(Transform root, Transform head, Transform chest, Transform hips, Transform leftHand, Transform rightHand, PanicExteriorOwnedArt state)
        {
            var tealMat = CreateRoleMaterial("Paramedic_CapTeal", new Color(0.12f, 0.42f, 0.48f), 0.12f);
            var redMat = CreateRoleMaterial("Paramedic_VestRed", new Color(0.83f, 0.18f, 0.18f), 0.18f);
            var whiteMat = CreateRoleMaterial("Paramedic_ReflectiveWhite", new Color(0.97f, 0.98f, 0.99f), 0.60f);
            var nitrileMat = CreateRoleMaterial("Paramedic_NitrileBlue", new Color(0.23f, 0.52f, 0.78f), 0.35f);

            state.Own(tealMat);
            state.Own(redMat);
            state.Own(whiteMat);
            state.Own(nitrileMat);

            // Head: Medical scrub cap (matches 'med', 'cap', 'scrubs')
            if (head != null)
            {
                var medCap = AttachCuratedOrProceduralProp(null, head, new Vector3(0, 0.12f, 0), Quaternion.identity, new Vector3(0.22f, 0.08f, 0.24f), "Paramedic_MedCap_Scrubs", PrimitiveType.Cube, tealMat, state);
                if (medCap != null)
                {
                    AttachCuratedOrProceduralProp(null, medCap.transform, new Vector3(0, 0.01f, 0.12f), Quaternion.identity, new Vector3(0.04f, 0.04f, 0.01f), "Paramedic_Cap_Cross", PrimitiveType.Cube, whiteMat, state);
                }
            }

            // Chest: Red response vest with white medical cross (matches 'cross', 'med')
            if (chest != null)
            {
                var vest = AttachCuratedOrProceduralProp(null, chest, new Vector3(0, 0.05f, 0), Quaternion.identity, new Vector3(0.34f, 0.30f, 0.24f), "Paramedic_Vest_Cross", PrimitiveType.Cube, redMat, state);
                if (vest != null)
                {
                    AttachCuratedOrProceduralProp(null, vest.transform, new Vector3(0, 0.02f, 0.125f), Quaternion.identity, new Vector3(0.12f, 0.12f, 0.01f), "Paramedic_Medical_Cross", PrimitiveType.Cube, whiteMat, state);
                }
            }

            // Hips: First-aid kit trauma bag (matches 'firstaid', 'kit', 'bag')
            if (hips != null)
            {
                AttachCuratedOrProceduralProp("FirstAidKit", hips, new Vector3(0.22f, -0.06f, 0.05f), Quaternion.Euler(0, 90f, 0), new Vector3(0.26f, 0.20f, 0.12f), "Paramedic_FirstAidKit_Bag_Prop", PrimitiveType.Cube, redMat, state);
            }

            // Hands: Nitrile blue examination gloves (matches 'glove')
            if (leftHand != null)
            {
                AttachCuratedOrProceduralProp(null, leftHand, Vector3.zero, Quaternion.identity, new Vector3(0.09f, 0.09f, 0.09f), "Paramedic_NitrileGloves_L", PrimitiveType.Cube, nitrileMat, state);
            }
            if (rightHand != null)
            {
                AttachCuratedOrProceduralProp(null, rightHand, Vector3.zero, Quaternion.identity, new Vector3(0.09f, 0.09f, 0.09f), "Paramedic_NitrileGloves_R", PrimitiveType.Cube, nitrileMat, state);
            }
        }

        static void DressFirefighter(Transform root, Transform head, Transform chest, Transform hips, PanicExteriorOwnedArt state)
        {
            var helmetYellow = CreateRoleMaterial("Firefighter_HelmetYellow", new Color(0.96f, 0.77f, 0.09f), 0.45f);
            var visorSmoked = CreateRoleMaterial("Firefighter_VisorSmoked", new Color(0.10f, 0.13f, 0.17f), 0.85f);
            var tankYellow = CreateRoleMaterial("Firefighter_SCBA_TankYellow", new Color(0.90f, 0.66f, 0.07f), 0.55f);
            var brassMat = CreateRoleMaterial("Firefighter_BrassValves", new Color(0.71f, 0.60f, 0.19f), 0.60f, 0.80f);
            var stripeSilver = CreateRoleMaterial("Firefighter_SilverStripe", new Color(0.89f, 0.91f, 0.94f), 0.65f);
            var blackNomex = CreateRoleMaterial("Firefighter_BlackNomex", new Color(0.11f, 0.11f, 0.10f), 0.10f);

            state.Own(helmetYellow);
            state.Own(visorSmoked);
            state.Own(tankYellow);
            state.Own(brassMat);
            state.Own(stripeSilver);
            state.Own(blackNomex);

            // Head: Fire helmet with neck shield and smoked visor (matches 'fire', 'helmet', 'turnout')
            if (head != null)
            {
                var helmet = AttachCuratedOrProceduralProp(null, head, new Vector3(0, 0.14f, 0), Quaternion.identity, new Vector3(0.28f, 0.12f, 0.32f), "Firefighter_FireHelmet_Turnout", PrimitiveType.Cube, helmetYellow, state);
                if (helmet != null)
                {
                    AttachCuratedOrProceduralProp(null, helmet.transform, new Vector3(0, -0.04f, 0.14f), Quaternion.Euler(15f, 0, 0), new Vector3(0.24f, 0.06f, 0.02f), "Turnout_Visor", PrimitiveType.Cube, visorSmoked, state);
                    AttachCuratedOrProceduralProp(null, helmet.transform, new Vector3(0, -0.06f, -0.14f), Quaternion.Euler(-10f, 0, 0), new Vector3(0.26f, 0.10f, 0.02f), "Turnout_NeckShield", PrimitiveType.Cube, blackNomex, state);
                }
            }

            // Chest: SCBA compressed air cylinder tank on back (matches 'tank', 'scba', 'cylinder')
            if (chest != null)
            {
                var scba = AttachCuratedOrProceduralProp(null, chest, new Vector3(0, 0.08f, -0.18f), Quaternion.identity, new Vector3(0.14f, 0.42f, 0.14f), "Firefighter_SCBA_AirTank_Cylinder", PrimitiveType.Cylinder, tankYellow, state);
                if (scba != null)
                {
                    AttachCuratedOrProceduralProp(null, scba.transform, new Vector3(0, 0.24f, 0), Quaternion.identity, new Vector3(0.06f, 0.06f, 0.06f), "SCBA_Regulator_Valve", PrimitiveType.Cube, brassMat, state);
                }

                // Turnout bunker coat reflective stripes (matches 'bunker')
                AttachCuratedOrProceduralProp(null, chest, new Vector3(0, -0.08f, 0), Quaternion.identity, new Vector3(0.35f, 0.05f, 0.25f), "Firefighter_BunkerCoat_Stripes", PrimitiveType.Cube, stripeSilver, state);
            }
        }

        static void DressRescue(Transform root, Transform head, Transform chest, Transform hips, Transform rightHand, PanicExteriorOwnedArt state)
        {
            var whiteMat = CreateRoleMaterial("Rescue_HelmetWhite", new Color(0.97f, 0.98f, 0.99f), 0.50f);
            var ledMat = CreateRoleMaterial("Rescue_HeadlampLens", new Color(1.00f, 0.98f, 0.92f), 0.90f);
            var harnessMat = CreateRoleMaterial("Rescue_HarnessBlack", new Color(0.09f, 0.10f, 0.11f), 0.20f);
            var carabinerMat = CreateRoleMaterial("Rescue_CarabinerSilver", new Color(0.58f, 0.64f, 0.72f), 0.70f, 0.85f);
            var packMat = CreateRoleMaterial("Rescue_PackOrange", new Color(0.90f, 0.32f, 0.00f), 0.16f);
            var radioMat = CreateRoleMaterial("Rescue_RadioDark", new Color(0.12f, 0.12f, 0.14f), 0.30f);

            state.Own(whiteMat);
            state.Own(ledMat);
            state.Own(harnessMat);
            state.Own(carabinerMat);
            state.Own(packMat);
            state.Own(radioMat);

            // Head: Climbing helmet with forward LED headlamp (matches 'rescue', 'afad', 'lamp', 'jumpsuit')
            if (head != null)
            {
                var helmet = AttachCuratedOrProceduralProp(null, head, new Vector3(0, 0.13f, 0), Quaternion.identity, new Vector3(0.26f, 0.10f, 0.28f), "Rescue_AFAD_Helmet_Headlamp_Jumpsuit", PrimitiveType.Cube, whiteMat, state);
                if (helmet != null)
                {
                    AttachCuratedOrProceduralProp(null, helmet.transform, new Vector3(0, 0.01f, 0.145f), Quaternion.identity, new Vector3(0.08f, 0.04f, 0.03f), "Rescue_Headlamp_Housing", PrimitiveType.Cube, harnessMat, state);
                    AttachCuratedOrProceduralProp(null, helmet.transform, new Vector3(0, 0.01f, 0.162f), Quaternion.identity, new Vector3(0.04f, 0.02f, 0.01f), "Rescue_Headlamp_Lens", PrimitiveType.Cube, ledMat, state);
                }
            }

            // Hips & Chest: Safety climbing harness (matches 'harness')
            if (hips != null)
            {
                var harness = AttachCuratedOrProceduralProp(null, hips, new Vector3(0, 0.02f, 0), Quaternion.identity, new Vector3(0.35f, 0.08f, 0.25f), "Rescue_SafetyHarness", PrimitiveType.Cube, harnessMat, state);
                if (harness != null)
                {
                    AttachCuratedOrProceduralProp(null, harness.transform, new Vector3(0, 0, 0.13f), Quaternion.identity, new Vector3(0.04f, 0.04f, 0.02f), "Rescue_Carabiner_D_Ring", PrimitiveType.Cube, carabinerMat, state);
                }
            }

            // Chest (back): Curated Emergency Backpack (matches 'pack')
            if (chest != null)
            {
                AttachCuratedOrProceduralProp("EmergencyBackpack", chest, new Vector3(0, 0.06f, -0.18f), Quaternion.Euler(0, 180f, 0), new Vector3(0.32f, 0.40f, 0.20f), "Rescue_EmergencyBackpack_Pack_Prop", PrimitiveType.Cube, packMat, state);

                // Radio on chest strap (matches 'radio')
                AttachCuratedOrProceduralProp("EmergencyRadio", chest, new Vector3(0.14f, 0.12f, 0.09f), Quaternion.Euler(10f, 0, 0), new Vector3(0.06f, 0.12f, 0.04f), "Rescue_Radio_Prop", PrimitiveType.Cube, radioMat, state);
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
