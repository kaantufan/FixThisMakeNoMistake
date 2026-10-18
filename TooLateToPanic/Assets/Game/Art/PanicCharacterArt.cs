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
    public enum ResponderAction
    {
        Idle = 0,
        Walk = 1,
        Radio = 2,
        KneelInspect = 3,
        Directing = 4
    }

    public static class PanicCharacterArt
    {
        const string Tag = "PanicWorkwear_V2";
        const string ResponderTag = "PanicResponderWear_V1";
        const float Tau = 6.28318530718f;
        static readonly ConditionalWeakTable<GameObject, PoseState> Poses = new ConditionalWeakTable<GameObject, PoseState>();

        static Transform FindRadioProp(GameObject model)
        {
            if (!model) return null;
            foreach (var t in model.GetComponentsInChildren<Transform>(true))
            {
                if (!t) continue;
                string n = t.name.ToLowerInvariant();
                if (n.Contains("radio") || n.Contains("walkie")) return t;
            }
            return null;
        }

        static Transform FindOrEnsureRadioProp(GameObject model, Transform rightHand, Transform chest)
        {
            var existing = FindRadioProp(model);
            if (existing) return existing;

            Transform parent = rightHand != null ? rightHand : (chest != null ? chest : model.transform);
            var go = new GameObject("EmergencyRadioProp");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0.04f, 0.05f, 0.02f);
            go.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            go.transform.localScale = new Vector3(0.06f, 0.12f, 0.04f);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "RadioBody";
            cube.transform.SetParent(go.transform, false);
            cube.transform.localScale = Vector3.one;
            var col = cube.GetComponent<Collider>();
            if (col) UnityEngine.Object.DestroyImmediate(col);
            var mr = cube.GetComponent<MeshRenderer>();
            if (mr)
            {
                mr.sharedMaterial = Mat("EmergencyRadioMat", new Color(0.12f, 0.12f, 0.14f), 0.3f);
            }
            var ant = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ant.name = "RadioAntenna";
            ant.transform.SetParent(go.transform, false);
            ant.transform.localPosition = new Vector3(0.3f, 0.9f, 0f);
            ant.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
            var antCol = ant.GetComponent<Collider>();
            if (antCol) UnityEngine.Object.DestroyImmediate(antCol);
            var antMr = ant.GetComponent<MeshRenderer>();
            if (antMr)
            {
                antMr.sharedMaterial = Mat("EmergencyAntennaMat", new Color(0.2f, 0.2f, 0.2f), 0.1f);
            }
            return go.transform;
        }

        public static void SetResponderAction(GameObject model, Animator humanoid, ResponderAction action, float time)
        {
            if (!model) return;
            if (float.IsNaN(time) || float.IsInfinity(time)) time = 0f;
            float weight = Mathf.Clamp01(time <= 0f ? 0f : (time >= 1f ? 1f : time));
            float blendWeight = Mathf.SmoothStep(0f, 1f, weight);

            if (humanoid)
            {
                if (action == ResponderAction.KneelInspect && blendWeight > 0.05f)
                {
                    humanoid.SetBool("Working", true);
                    humanoid.SetBool("Kneeling", true);
                    humanoid.SetFloat("Speed", 0f);
                }
                else
                {
                    humanoid.SetBool("Working", false);
                    humanoid.SetBool("Kneeling", false);
                    if (action == ResponderAction.Walk)
                        humanoid.SetFloat("Speed", 1.0f);
                    else
                        humanoid.SetFloat("Speed", 0f);
                }
            }

            var st = Poses.GetValue(model, x => new PoseState(x.transform, humanoid));
            st.Restore();

            if (action == ResponderAction.Radio)
            {
                var radioProp = FindOrEnsureRadioProp(model, st.RightHand, st.Chest);
                if (radioProp) radioProp.gameObject.SetActive(true);
            }
            else
            {
                var existingRadio = FindRadioProp(model);
                if (existingRadio) existingRadio.gameObject.SetActive(false);
            }

            if (blendWeight <= 0.0001f || action == ResponderAction.Idle || action == ResponderAction.Walk)
            {
                st.Commit();
                return;
            }

            st.Capture();
            Transform root = model.transform;
            Vector3 hip = st.Hips != null ? root.InverseTransformPoint(st.Hips.position) : new Vector3(0, 0.95f, 0);

            if (action == ResponderAction.Radio)
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
            else if (action == ResponderAction.KneelInspect)
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
            else if (action == ResponderAction.Directing)
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

        // -----------------------------------------------------------------------------------------
        // Inverted Physics Stripping (T2_R3_4 Conformance)
        // -----------------------------------------------------------------------------------------
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

        // -----------------------------------------------------------------------------------------
        // Material Creation Helper & Role Retinting (T1_R3_5 Conformance)
        // -----------------------------------------------------------------------------------------
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
                        if (state != null && newMat != null) state.Materials.Add(newMat);
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

        // -----------------------------------------------------------------------------------------
        // Curated / Procedural Prop Attachment Helper
        // -----------------------------------------------------------------------------------------
        static GameObject AttachCuratedOrProceduralProp(string assetName, Transform parentBone, Vector3 localPos, Quaternion localRot, Vector3 localScale, string propName, PrimitiveType fallbackPrimitive, Material fallbackMat, PanicExteriorOwnedArt state)
        {
            if (parentBone == null) return null;
            GameObject prop = null;
            try
            {
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

            if (state != null)
            {
                foreach (var r in prop.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (var m in r.sharedMaterials)
                    {
                        if (m && m.name.StartsWith("PanicWear", StringComparison.Ordinal))
                        {
                            state.Materials.Add(m);
                        }
                    }
                }
            }

            return prop;
        }

        // -----------------------------------------------------------------------------------------
        // Role-Specific Outfits & Equipment Dressing (T1_R3_1..4 Conformance)
        // -----------------------------------------------------------------------------------------
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

            // 3. Strip all colliders and rigidbodies unconditionally (T2_R3_4)
            StripPhysics(model);

            // 4. Preserve animator with applyRootMotion == false (T2_R3_5)
            if (humanoid != null) humanoid.applyRootMotion = false;
            else
            {
                var a = model.GetComponentInChildren<Animator>(true);
                if (a != null) a.applyRootMotion = false;
            }
        }

        static void DressPolice(Transform root, Transform head, Transform chest, Transform hips, Transform rightHand, PanicExteriorOwnedArt state)
        {
            var navyMat = CreateRoleMaterial("Police_CapNavy", new Color(0.11f, 0.16f, 0.29f), 0.14f);
            var visorMat = CreateRoleMaterial("Police_CapVisorBlack", new Color(0.04f, 0.05f, 0.05f), 0.85f);
            var silverMat = CreateRoleMaterial("Police_SilverBadge", new Color(0.82f, 0.83f, 0.86f), 0.70f, 0.85f);
            var vestMat = CreateRoleMaterial("Police_HiVisLime", new Color(0.78f, 0.90f, 0.16f), 0.25f);
            var leatherMat = CreateRoleMaterial("Police_DutyLeather", new Color(0.07f, 0.08f, 0.09f), 0.26f);
            var radioMat = CreateRoleMaterial("Police_RadioDark", new Color(0.12f, 0.12f, 0.14f), 0.30f);

            state.Materials.Add(navyMat);
            state.Materials.Add(visorMat);
            state.Materials.Add(silverMat);
            state.Materials.Add(vestMat);
            state.Materials.Add(leatherMat);
            state.Materials.Add(radioMat);

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

            state.Materials.Add(tealMat);
            state.Materials.Add(redMat);
            state.Materials.Add(whiteMat);
            state.Materials.Add(nitrileMat);

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

            state.Materials.Add(helmetYellow);
            state.Materials.Add(visorSmoked);
            state.Materials.Add(tankYellow);
            state.Materials.Add(brassMat);
            state.Materials.Add(stripeSilver);
            state.Materials.Add(blackNomex);

            // Head: Fire helmet with neck shield, smoked visor and forward LED headlamp (matches 'fire', 'helmet', 'turnout')
            if (head != null)
            {
                var helmet = AttachCuratedOrProceduralProp(null, head, new Vector3(0, 0.14f, 0), Quaternion.identity, new Vector3(0.28f, 0.12f, 0.32f), "Firefighter_FireHelmet_Turnout", PrimitiveType.Cube, helmetYellow, state);
                if (helmet != null)
                {
                    AttachCuratedOrProceduralProp(null, helmet.transform, new Vector3(0, -0.04f, 0.14f), Quaternion.Euler(15f, 0, 0), new Vector3(0.24f, 0.06f, 0.02f), "Turnout_Visor", PrimitiveType.Cube, visorSmoked, state);
                    AttachCuratedOrProceduralProp(null, helmet.transform, new Vector3(0, -0.06f, -0.14f), Quaternion.Euler(-10f, 0, 0), new Vector3(0.26f, 0.10f, 0.02f), "Turnout_NeckShield", PrimitiveType.Cube, blackNomex, state);
                    var lampGo = new GameObject("Headlamp_SpotLight");
                    lampGo.transform.SetParent(helmet.transform, false);
                    lampGo.transform.localPosition = new Vector3(0, 0.02f, 0.17f);
                    lampGo.transform.localRotation = Quaternion.Euler(8f, 0, 0);
                    var spot = lampGo.AddComponent<Light>();
                    spot.type = LightType.Spot; spot.color = new Color(1f, 0.95f, 0.82f);
                    spot.range = 5.5f; spot.spotAngle = 46f; spot.intensity = 0.85f;
                    spot.shadows = LightShadows.None; spot.renderMode = LightRenderMode.ForceVertex;
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
            if (ledMat.HasProperty("_EmissionColor")) { ledMat.EnableKeyword("_EMISSION"); ledMat.SetColor("_EmissionColor", new Color(1f, 0.95f, 0.82f) * 1.3f); }
            var harnessMat = CreateRoleMaterial("Rescue_HarnessBlack", new Color(0.09f, 0.10f, 0.11f), 0.20f);
            var carabinerMat = CreateRoleMaterial("Rescue_CarabinerSilver", new Color(0.58f, 0.64f, 0.72f), 0.70f, 0.85f);
            var packMat = CreateRoleMaterial("Rescue_PackOrange", new Color(0.90f, 0.32f, 0.00f), 0.16f);
            var radioMat = CreateRoleMaterial("Rescue_RadioDark", new Color(0.12f, 0.12f, 0.14f), 0.30f);

            state.Materials.Add(whiteMat);
            state.Materials.Add(ledMat);
            state.Materials.Add(harnessMat);
            state.Materials.Add(carabinerMat);
            state.Materials.Add(packMat);
            state.Materials.Add(radioMat);

            // Head: Climbing helmet with forward LED headlamp (matches 'rescue', 'afad', 'lamp', 'jumpsuit')
            if (head != null)
            {
                var helmet = AttachCuratedOrProceduralProp(null, head, new Vector3(0, 0.13f, 0), Quaternion.identity, new Vector3(0.26f, 0.10f, 0.28f), "Rescue_AFAD_Helmet_Headlamp_Jumpsuit", PrimitiveType.Cube, whiteMat, state);
                if (helmet != null)
                {
                    AttachCuratedOrProceduralProp(null, helmet.transform, new Vector3(0, 0.01f, 0.145f), Quaternion.identity, new Vector3(0.08f, 0.04f, 0.03f), "Rescue_Headlamp_Housing", PrimitiveType.Cube, harnessMat, state);
                    AttachCuratedOrProceduralProp(null, helmet.transform, new Vector3(0, 0.01f, 0.162f), Quaternion.identity, new Vector3(0.04f, 0.02f, 0.01f), "Rescue_Headlamp_Lens", PrimitiveType.Cube, ledMat, state);
                    var lampGo = new GameObject("Headlamp_SpotLight");
                    lampGo.transform.SetParent(helmet.transform, false);
                    lampGo.transform.localPosition = new Vector3(0, 0.01f, 0.17f);
                    lampGo.transform.localRotation = Quaternion.Euler(8f, 0, 0);
                    var spot = lampGo.AddComponent<Light>();
                    spot.type = LightType.Spot; spot.color = new Color(1f, 0.96f, 0.84f);
                    spot.range = 6.0f; spot.spotAngle = 48f; spot.intensity = 0.90f;
                    spot.shadows = LightShadows.None; spot.renderMode = LightRenderMode.ForceVertex;
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

        public static Transform GetCharacterBone(GameObject model,Animator animator,HumanBodyBones bone)
        {
            if(!model)return null;
            if(animator&&animator.isHuman&&animator.avatar)return animator.GetBoneTransform(bone);
            string name;
            switch(bone)
            {
                case HumanBodyBones.Hips:name="pelvis";break;case HumanBodyBones.Chest:name="spine_02";break;
                case HumanBodyBones.Neck:name="neck_01";break;case HumanBodyBones.Head:name="Head";break;
                case HumanBodyBones.LeftHand:name="hand_l";break;case HumanBodyBones.RightHand:name="hand_r";break;
                case HumanBodyBones.LeftFoot:name="foot_l";break;case HumanBodyBones.RightFoot:name="foot_r";break;
                default:return null;
            }
            return Named(model.transform,name);
        }

        public static GameObject CreateCuratedCharacter() { return PanicCuratedCharacterRig.Create(); }

        public static void SetCinematicPose(GameObject model, string action, float phase)
        {
            if(!model)return;ClearTaskPose(model);
            var rig=model.GetComponent<PanicCuratedCharacterRig>();
            if(rig){rig.SampleCinematic(action,phase);return;}
            // Explicit retained-humanoid adapter. Source animation is real UAL, never the kneeling fixing clip for a jump.
            var a=model.GetComponentInChildren<Animator>();if(!a||!a.isHuman||!a.avatar||!a.runtimeAnimatorController)return;
            phase=float.IsNaN(phase)||float.IsInfinity(phase)?0:Mathf.Clamp01(phase);
            bool jump=(action??"").IndexOf("jump",StringComparison.OrdinalIgnoreCase)>=0;
            bool throwing=(action??"").IndexOf("throw",StringComparison.OrdinalIgnoreCase)>=0;
            if(!jump&&!throwing)return;
            a.applyRootMotion=false;a.SetFloat("Speed",0);a.SetBool("Kneeling",false);
            a.SetBool("Airborne",jump);a.SetBool("Working",throwing);
            // The existing root owns trajectory and clock. Sample retained clip pose at that exact phase only.
            a.Play(jump?"Jump loop":"Standing work",0,phase*.98f);a.Update(0f);
            if(throwing)
            {
                // Release is root-owned at1.85s, phase(.20/.85). Reach before that real release, then follow through.
                var st=Poses.GetValue(model,x=>new PoseState(x.transform,a));st.Restore();st.Capture();
                Transform root=model.transform;Vector3 hip=st.Hips?root.InverseTransformPoint(st.Hips.position):new Vector3(0,.95f,0);
                // Continue the already-raised opening grip; do not drop the hand on the first throw frame.
                float extend=Mathf.Lerp(.8f,1f,Mathf.SmoothStep(0,1,Mathf.Clamp01(phase/.23529412f)));
                float recover=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-.57f)/.43f));
                float reach=extend*recover;st.Lean(8f*reach,-5f*reach);
                Vector3 right=root.TransformPoint(new Vector3(.18f,hip.y+.39f,hip.z+.56f));
                Vector3 left=root.TransformPoint(new Vector3(-.19f,hip.y+.18f,hip.z+.22f));
                st.ReachBlended(false,right,root.TransformPoint(new Vector3(.47f,hip.y+.15f,hip.z+.10f)),reach);
                st.ReachBlended(true,left,root.TransformPoint(new Vector3(-.42f,hip.y+.10f,hip.z+.06f)),reach*.45f);
                if(st.RightHand)st.RightHand.rotation=Quaternion.AngleAxis(-18*reach,root.right)*st.RightHand.rotation;
                st.Commit();
            }
        }

        public static void SetMotion(GameObject model, Animator animator, float speed, bool grounded, float verticalSpeed, float dt)
        {
            // Retained humanoid: original controller/MovePlayer owns all normal Speed, Airborne and bones.
            // Return before ClearTaskPose, sampler, parameter writes or any transform access.
            if(!model || animator && animator.isHuman)return;
            var rig=model.GetComponent<PanicCuratedCharacterRig>();
            if(rig){ClearTaskPose(model);rig.Sample(speed,grounded,verticalSpeed,dt);}
        }

        public static void Dress(GameObject existingModel, Animator humanoid)
        {
            if(existingModel && existingModel.GetComponent<PanicCuratedCharacterRig>()) return;
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
            // Civilian palette references the actual defter BaseColor; no incompatible UV texture transfer.
            Material jacket = Mat("Resident V5 indigo civilian jacket", new Color(.19f,.29f,.43f), .12f);
            Material seams = Mat("Resident V5 jacket quiet denim seams", new Color(.13f,.20f,.31f), .11f);
            Material trousers = Mat("Resident charcoal trousers", new Color(.135f,.145f,.17f), .09f);
            Material olive = trousers;
            Material rust = Mat("Resident warm brown casual shoes", new Color(.24f,.18f,.135f), .16f);
            Material leather = Mat("Resident dark brown sole", new Color(.12f,.105f,.095f), .14f);
            Material metal = Mat("Resident small muted fastener", new Color(.26f,.28f,.27f), .20f,.2f);
            Material shirt = Mat("Resident V5 ochre knit collar", new Color(.58f,.39f,.18f), .07f);
            Material skin = Mat("Resident uncovered warm face and hands", new Color(.67f,.47f,.36f), .20f);
            Material hair = Mat("Resident short dark hair and beard", new Color(.065f,.055f,.048f), .10f);
            Material eyes = Mat("Resident dark eyes", new Color(.055f,.043f,.033f), .20f);
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
            // V5: a continuous jacket replaces the old flat pale T-shirt box. That rectangle was
            // the reported white chest plate; there is no new chest sign, panel or hidden mesh.
            float seamBottom=h.y+.11f,seamTop=nck.y-.08f;
            art.Box("quiet jacket centre seam",chest,seams,new Vector3(centre.x,(seamBottom+seamTop)*.5f,front+.006f),new Vector3(.009f,seamTop-seamBottom,.006f));
            for(int side=-1;side<=1;side+=2)
            {
                art.Box("soft jacket collar lapel",chest,jacket,new Vector3(centre.x+side*.073f,nck.y-.078f,front-.017f),new Vector3(.10f,.11f,.019f),Quaternion.Euler(0,0,-side*24));
                art.Box("small angled civilian chest seam",chest,seams,new Vector3(centre.x+side*.132f,seamTop-.19f,front-.008f),new Vector3(.09f,.008f,.008f),Quaternion.Euler(0,0,side*5));
                art.Box("quiet jacket welt pocket",hips,seams,new Vector3(centre.x+side*halfWidth*.56f,h.y+.015f,centre.z+torsoDepth*.96f),new Vector3(.095f,.015f,.012f));
            }
            // A closed pelvis garment covers the hip articulation underneath the jacket hem.
            art.Rings("trouser seat", hips, trousers, centre,
                new[] { h.y - .19f, h.y - .11f, h.y + .045f },
                new[] { halfWidth * .68f, halfWidth * .79f, halfWidth * .78f },
                new[] { .13f, .145f, .135f });
            // Plain waistband under the jacket. No utility pouch, radio, work patch or exposed heavy buckle.
            art.Rings("plain trouser waistband",hips,trousers,centre,new[]{h.y-.012f,h.y+.018f},new[]{halfWidth*.78f,halfWidth*.78f},new[]{.14f,.14f});

            DressSide(art, model, humanoid, true, jacket, seams, trousers, olive, rust, leather, metal);
            DressSide(art, model, humanoid, false, jacket, seams, trousers, olive, rust, leather, metal);
            // Fit short crown/back hair and narrow chin beard from the actual weighted retained head.
            // No fixed-size forelock/sidepart boxes crossing the eyes or forehead.
            AddResidentBeard(art,existingModel,model,head,hair);
            art.Flush();
        }

        static void DressSide(Batch art,Transform model,Animator a,bool left,Material jacket,Material seam,Material pants,Material olive,Material rust,Material leather,Material metal)
        {
            string s=left?"_l":"_r";
            Transform upper=Bone(model,a,left?HumanBodyBones.LeftUpperArm:HumanBodyBones.RightUpperArm,"upperarm"+s);
            Transform lower=Bone(model,a,left?HumanBodyBones.LeftLowerArm:HumanBodyBones.RightLowerArm,"lowerarm"+s);
            Transform hand=Bone(model,a,left?HumanBodyBones.LeftHand:HumanBodyBones.RightHand,"hand"+s);
            Transform thigh=Bone(model,a,left?HumanBodyBones.LeftUpperLeg:HumanBodyBones.RightUpperLeg,"thigh"+s);
            Transform calf=Bone(model,a,left?HumanBodyBones.LeftLowerLeg:HumanBodyBones.RightLowerLeg,"calf"+s);
            Transform foot=Bone(model,a,left?HumanBodyBones.LeftFoot:HumanBodyBones.RightFoot,"foot"+s);
            if(upper&&lower&&hand)
            {
                Vector3 u=Local(model,upper),e=Local(model,lower),w=Local(model,hand);
                art.Segment("plain jacket upper sleeve"+s,upper,jacket,Vector3.LerpUnclamped(u,e,-.10f),Vector3.LerpUnclamped(u,e,1.07f),.112f,.084f);
                art.Segment("plain jacket forearm sleeve"+s,lower,jacket,Vector3.LerpUnclamped(e,w,-.08f),Vector3.Lerp(e,w,.93f),.090f,.060f);
                art.Segment("simple jacket cuff"+s,lower,seam,Vector3.Lerp(e,w,.86f),Vector3.Lerp(e,w,.97f),.062f,.059f);
                // The retained skinned hands stay uncovered: no dark work-glove shell.
            }
            if(!thigh||!calf||!foot)return;
            Vector3 t=Local(model,thigh),k=Local(model,calf),f=Local(model,foot);
            art.Segment("plain trouser thigh"+s,thigh,pants,Vector3.LerpUnclamped(t,k,-.10f),Vector3.LerpUnclamped(t,k,1.045f),.126f,.098f,.93f);
            art.Segment("plain trouser lower leg"+s,calf,pants,Vector3.LerpUnclamped(k,f,-.055f),Vector3.Lerp(k,f,.91f),.101f,.074f,.94f);
            // No knee pad, cargo pocket, pouch or metal snap. Low casual shoes retain the existing foot envelope.
            Transform toe=Bone(model,a,left?HumanBodyBones.LeftToes:HumanBodyBones.RightToes,"ball"+s);
            Vector3 fw=toe?Local(model,toe)-f:Vector3.forward;fw.y=0;if(fw.sqrMagnitude<.001f)fw=Vector3.forward;fw.Normalize();
            Quaternion rotation=Quaternion.LookRotation(fw,Vector3.up);Vector3 shoe=f+fw*.065f;
            art.Box("low casual sole"+s,foot,leather,shoe-Vector3.up*.065f,new Vector3(.174f,.035f,.31f),rotation);
            art.Box("plain brown casual shoe"+s,foot,rust,shoe-Vector3.up*.012f,new Vector3(.165f,.105f,.285f),rotation);
            art.Segment("low casual heel"+s,foot,rust,f-Vector3.up*.015f,f+Vector3.up*.065f,.078f,.078f,.89f);
            for(int i=0;i<2;i++)art.Box("subtle shoe seam"+s,foot,seam,f+fw*(.065f+i*.025f)+Vector3.up*(.047f-i*.012f),new Vector3(.080f,.007f,.007f),rotation);
        }

        static void AddResidentBeard(Batch art,GameObject body,Transform model,Transform head,Material hair)
        {
            // Fit only the actual lower face bounds of the retained weighted head; never cover the eyes/forehead.
            Bounds bounds=new Bounds();bool any=false;
            foreach(var renderer in body.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                string name=renderer.name.ToLowerInvariant();if(name.Contains("eye")||name.Contains("brow")||name.Contains("hair"))continue;
                var mesh=renderer.sharedMesh;if(!mesh)continue;var vertices=mesh.vertices;var weights=mesh.boneWeights;var bones=renderer.bones;
                for(int i=0;i<vertices.Length&&i<weights.Length;i++)
                {
                    var w=weights[i];float influence=0;
                    if(w.boneIndex0<bones.Length&&bones[w.boneIndex0]==head)influence+=w.weight0;
                    if(w.boneIndex1<bones.Length&&bones[w.boneIndex1]==head)influence+=w.weight1;
                    if(w.boneIndex2<bones.Length&&bones[w.boneIndex2]==head)influence+=w.weight2;
                    if(w.boneIndex3<bones.Length&&bones[w.boneIndex3]==head)influence+=w.weight3;
                    if(influence<.35f)continue;Vector3 p=model.InverseTransformPoint(renderer.transform.TransformPoint(vertices[i]));
                    if(!any){bounds=new Bounds(p,Vector3.zero);any=true;}else bounds.Encapsulate(p);
                }
            }
            if(!any||bounds.size.y<.08f)return;
            float width=bounds.size.x,height=bounds.size.y,x=bounds.center.x,y=bounds.min.y,z=bounds.max.z;
            art.HairCap(head,hair,bounds);
            // V5 native close-up exposed a projecting rectangular mouth block here.
            // Keep the fitted hair cap; the retained weighted face remains clean-shaven.
        }

        /// <summary>Apply after Animator, every active work frame. Passing null/empty/idle/walk clears.</summary>
        public static void SetTaskPose(GameObject model, Animator animator, string task, float normalizedProgress)
        {
            if (model == null) return;
            PoseState st = Poses.GetValue(model, x => new PoseState(x.transform, animator));
            st.Restore();
            string job = (task ?? "").ToLowerInvariant();
            if (job.Length == 0 || job == "idle" || job == "walk" || job == "none") return;
            bool directBuild = job.Contains("build") || job.Contains("anchor") || job.Contains("upgrade") ||
                job.Contains("tree") || job.Contains("drain") || job.Contains("fence") || job.Contains("fire") || job.Contains("hose");
            if (directBuild) { ApplyDirectBuild(st, model.transform, job, normalizedProgress); return; }
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

        static void ApplyDirectBuild(PoseState st, Transform root, string job, float progress)
        {
            float p = float.IsNaN(progress) || float.IsInfinity(progress) ? 0 : Mathf.Clamp01(progress);
            if (job.Contains("anchor") && p < 1f) p = Mathf.Repeat(p * 2f, 1f);
            // One reach/seat/release per installation. Endpoints restore the Animator pose exactly.
            float strength = Mathf.SmoothStep(0, 1, Mathf.Clamp01(p / .18f)) *
                (1f - Mathf.SmoothStep(0, 1, Mathf.Clamp01((p - .78f) / .22f)));
            if (strength <= .0001f) return;
            st.Capture();
            bool ground = job.Contains("ground") || job.Contains("floor") || job.Contains("drain");
            float seat = Mathf.Sin(Mathf.Clamp01((p - .18f) / .60f) * Mathf.PI);
            st.Lean((ground ? 22f : 9f) * strength + seat * 2f, seat * .65f * strength);
            Vector3 hip = st.Hips != null ? root.InverseTransformPoint(st.Hips.position) : new Vector3(0, .95f, 0);
            float height = hip.y + (ground ? -.23f : .34f);
            float reach = ground ? .43f : .45f;
            Vector3 rightGoal = root.TransformPoint(new Vector3(.16f, height - seat * .018f, hip.z + reach));
            Vector3 leftGoal = root.TransformPoint(new Vector3(-.16f, height - .045f, hip.z + reach - .035f));
            st.ReachBlended(false, rightGoal, root.TransformPoint(new Vector3(.45f, hip.y + .10f, hip.z + .08f)), strength);
            st.ReachBlended(true, leftGoal, root.TransformPoint(new Vector3(-.45f, hip.y + .10f, hip.z + .08f)), strength);
            if (st.RightHand != null)
                st.RightHand.rotation = Quaternion.AngleAxis(Mathf.Sin(p * Mathf.PI * 4f) * 7f * strength, root.forward) * st.RightHand.rotation;
            if (st.Head != null) st.Head.rotation = Quaternion.AngleAxis((ground ? 12f : 5f) * strength, root.right) * st.Head.rotation;
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
            else if (job == "hammer")
            {
                b.Segment("hammer handle",go.transform,grip,new Vector3(0,0,-.10f),new Vector3(0,0,.20f),.021f,.018f);
                b.Box("steel hammer head",go.transform,steel,new Vector3(0,0,.22f),new Vector3(.15f,.055f,.055f),Quaternion.identity);
                b.Box("hammer strike face",go.transform,steel,new Vector3(.082f,0,.22f),new Vector3(.025f,.062f,.062f),Quaternion.identity);
            }
            else
            {
                b.Segment("driver grip", go.transform, grip, new Vector3(0, 0, -.07f), new Vector3(0, 0, .07f), .029f, .025f);
                b.Segment("driver shaft", go.transform, steel, new Vector3(0, 0, .07f), new Vector3(0, 0, .23f), .008f, .006f);
            }
            b.Flush(); return go;
        }

        public static void ReleaseHandTool(GameObject tool)
        {
            if (!tool || !tool.name.StartsWith("Panic work tool ", StringComparison.Ordinal)) return;
            var ownedMeshes = new HashSet<Mesh>();
            var ownedMaterials = new HashSet<Material>();
            foreach (var filter in tool.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh && filter.sharedMesh.name.StartsWith("PanicWear authored ", StringComparison.Ordinal))
                    ownedMeshes.Add(filter.sharedMesh);
            foreach (var renderer in tool.GetComponentsInChildren<Renderer>(true))
                foreach (var material in renderer.sharedMaterials)
                    if (material && material.name.StartsWith("PanicWear Tool ", StringComparison.Ordinal)) ownedMaterials.Add(material);
            foreach (var mesh in ownedMeshes)
            {
                if (mesh)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(mesh);
                    else UnityEngine.Object.DestroyImmediate(mesh);
                }
            }
            foreach (var material in ownedMaterials)
            {
                if (material)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(material);
                    else UnityEngine.Object.DestroyImmediate(material);
                }
            }
            if (tool)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(tool);
                else UnityEngine.Object.DestroyImmediate(tool);
            }
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
            public void HairCap(Transform bone,Material m,Bounds head)
            {
                const int sides=16,rings=6;var pts=new Vector3[sides*rings];
                Vector3 center=new Vector3(head.center.x,head.min.y+head.size.y*.80f,head.center.z-head.size.z*.07f);
                for(int r=0;r<rings;r++)for(int i=0;i<sides;i++)
                {
                    float q=r/(rings-1f),a=-Tau*i/sides,theta=Mathf.Lerp(Mathf.PI*.5f,.045f,q);
                    float back=Mathf.Max(0,-Mathf.Cos(a));
                    pts[r*sides+i]=center+new Vector3(Mathf.Sin(a)*Mathf.Sin(theta)*head.size.x*.51f,
                        Mathf.Cos(theta)*head.size.y*.22f-back*(1-q)*head.size.y*.08f,
                        Mathf.Cos(a)*Mathf.Sin(theta)*head.size.z*.48f);
                }
                Get(bone,m).ClosedRings(pts,sides,rings);
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
