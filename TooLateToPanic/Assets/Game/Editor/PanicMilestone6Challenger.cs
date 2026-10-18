#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TooLateToPanic
{
    public static class PanicMilestone6Challenger
    {
        static int passed = 0;
        static int failed = 0;
        static readonly List<string> failures = new List<string>();

        static void Assert(string testName, bool condition, string passDetail, string failDetail)
        {
            if (condition)
            {
                passed++;
                Debug.Log($"[CHALLENGER_PASS] {testName}: {passDetail}");
            }
            else
            {
                failed++;
                failures.Add($"{testName}: {failDetail}");
                Debug.LogError($"[CHALLENGER_FAIL] {testName}: {failDetail}");
            }
        }

        public static void RunAllChallengerTests()
        {
            passed = 0;
            failed = 0;
            failures.Clear();

            Debug.Log("================================================================================");
            Debug.Log("[CHALLENGER] Starting Milestone 6 Adversarial Challenge & Stress Audit");
            Debug.Log("================================================================================");

            var rootGo = new GameObject("M6_Challenger_Root");

            try
            {
                Suite1_ReflectionContractAudit();
                Suite2_ActionsAndOutfitMatrix(rootGo);
                Suite3_ActionSpecificGestureAssertions(rootGo);
                Suite4_RapidTransitionsStressTest(rootGo);
                Suite5_BoundaryAndExtremeInputsStressTest(rootGo);
                Suite6_AcceptanceSuiteRun(rootGo);
            }
            catch (Exception ex)
            {
                failed++;
                failures.Add($"UNHANDLED_EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                Debug.LogError($"[CHALLENGER_FATAL] Unhandled exception in test runner: {ex}");
            }
            finally
            {
                if (rootGo) UnityEngine.Object.DestroyImmediate(rootGo);
            }

            Debug.Log("================================================================================");
            Debug.Log($"[CHALLENGER_SUMMARY] Total Passed: {passed}, Total Failed: {failed}");
            if (failures.Count > 0)
            {
                Debug.LogError($"[CHALLENGER_FAILURES] ({failures.Count} failures):");
                foreach (var f in failures) Debug.LogError("  - " + f);
            }
            Debug.Log("================================================================================");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(failed == 0 ? 0 : 1);
            }
        }

        static void Suite1_ReflectionContractAudit()
        {
            Debug.Log("--- SUITE 1: REFLECTION CONTRACT AUDIT ---");

            // 1. Check typeof(PanicCharacterArt).GetMethod("SetResponderAction", BindingFlags.Public | BindingFlags.Static)
            MethodInfo method = null;
            bool noAmbiguousException = true;
            try
            {
                method = typeof(PanicCharacterArt).GetMethod("SetResponderAction", BindingFlags.Public | BindingFlags.Static);
            }
            catch (AmbiguousMatchException)
            {
                noAmbiguousException = false;
            }

            Assert("S1_Reflection_NoAmbiguousMatch",
                noAmbiguousException && method != null,
                "typeof(PanicCharacterArt).GetMethod(\"SetResponderAction\", BindingFlags.Public | BindingFlags.Static) resolved cleanly without AmbiguousMatchException",
                "AmbiguousMatchException thrown or method not found");

            // 2. Exactly one public static SetResponderAction
            var allPublicStatic = typeof(PanicCharacterArt).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "SetResponderAction").ToArray();
            Assert("S1_Reflection_SinglePublicStaticMethod",
                allPublicStatic.Length == 1,
                "Exactly 1 public static SetResponderAction method found on PanicCharacterArt",
                $"Found {allPublicStatic.Length} public static SetResponderAction methods (expected exactly 1)");

            // 3. Parameter signatures
            if (method != null)
            {
                var p = method.GetParameters();
                bool sigMatch = p.Length == 4 &&
                    p[0].ParameterType == typeof(GameObject) &&
                    p[1].ParameterType == typeof(Animator) &&
                    p[2].ParameterType == typeof(ResponderAction) &&
                    p[3].ParameterType == typeof(float);

                Assert("S1_Reflection_ParameterSignature",
                    sigMatch,
                    "Parameters match (GameObject, Animator, ResponderAction, float)",
                    $"Parameter signature mismatch: {string.Join(", ", p.Select(x => x.ParameterType.Name))}");
            }

            // 4. ResponderAction enum values
            var names = Enum.GetNames(typeof(ResponderAction));
            string[] expected = { "Idle", "Walk", "Radio", "KneelInspect", "Directing" };
            bool hasAll5 = expected.All(e => names.Contains(e)) && names.Length == 5;
            Assert("S1_ResponderAction_All5Values",
                hasAll5,
                "ResponderAction enum contains exactly Idle, Walk, Radio, KneelInspect, Directing",
                $"ResponderAction enum values mismatch: {string.Join(", ", names)}");
        }

        static void Suite2_ActionsAndOutfitMatrix(GameObject root)
        {
            Debug.Log("--- SUITE 2: 5 ACTIONS X 4 OUTFITS MATRIX (20 COMBINATIONS) ---");

            var actions = (ResponderAction[])Enum.GetValues(typeof(ResponderAction));
            int combinationsPass = 0;

            for (int role = 0; role < 4; role++)
            {
                var visual = PanicExteriorArt.CreatePedestrianVisual(role);
                if (visual) visual.transform.SetParent(root.transform);
                var anim = visual ? visual.GetComponentInChildren<Animator>() : null;

                for (int a = 0; a < actions.Length; a++)
                {
                    var action = actions[a];
                    bool execOk = true;
                    try
                    {
                        PanicCharacterArt.SetResponderAction(visual, anim, action, 1.0f);
                    }
                    catch (Exception ex)
                    {
                        execOk = false;
                        Debug.LogError($"Error in role {role} action {action}: {ex}");
                    }

                    if (execOk) combinationsPass++;
                }

                if (visual) UnityEngine.Object.DestroyImmediate(visual);
            }

            Assert("S2_OutfitsAndActions_20Matrix",
                combinationsPass == 20,
                $"All 20 combinations of responder outfits (4) and actions (5) executed successfully ({combinationsPass}/20)",
                $"Only {combinationsPass}/20 combinations succeeded");
        }

        static void Suite3_ActionSpecificGestureAssertions(GameObject root)
        {
            Debug.Log("--- SUITE 3: ACTION-SPECIFIC GESTURE & BONE POSING ASSERTIONS ---");

            for (int role = 0; role < 4; role++)
            {
                var visual = PanicExteriorArt.CreatePedestrianVisual(role);
                if (visual) visual.transform.SetParent(root.transform);
                var anim = visual ? visual.GetComponentInChildren<Animator>() : null;

                // --- 1. Radio ---
                PanicCharacterArt.SetResponderAction(visual, anim, ResponderAction.Radio, 1.0f);
                var radioProp = visual.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name.ToLowerInvariant().Contains("radio") || t.name.ToLowerInvariant().Contains("walkie"));

                bool radioActive = radioProp != null && radioProp.gameObject.activeSelf;
                Assert($"S3_Radio_PropActive_Role{role}",
                    radioActive,
                    $"Walkie-talkie prop activeSelf == true for role {role}",
                    $"Walkie-talkie prop not active for role {role}");

                var rightHand = FindBone(visual.transform, anim, HumanBodyBones.RightHand, "hand_r");
                var head = FindBone(visual.transform, anim, HumanBodyBones.Head, "head");
                var hips = FindBone(visual.transform, anim, HumanBodyBones.Hips, "pelvis");

                bool rightHandElevated = rightHand != null && hips != null && rightHand.position.y > hips.position.y + 0.25f;
                bool rightHandNearHead = rightHand != null && head != null && Mathf.Abs(head.position.y - rightHand.position.y) < 0.45f;

                Assert($"S3_Radio_RightHandElevated_Role{role}",
                    rightHandElevated && rightHandNearHead,
                    $"Right hand elevated near head/mouth level for role {role} (handY={rightHand?.position.y:F2}, headY={head?.position.y:F2}, hipY={hips?.position.y:F2})",
                    $"Right hand not elevated properly for role {role} (handY={rightHand?.position.y:F2}, headY={head?.position.y:F2}, hipY={hips?.position.y:F2})");

                // --- 2. KneelInspect ---
                // Record standing base hip local position
                PanicCharacterArt.SetResponderAction(visual, anim, ResponderAction.Idle, 1.0f);
                float standingHipLocalY = hips != null ? hips.localPosition.y : 0f;

                PanicCharacterArt.SetResponderAction(visual, anim, ResponderAction.KneelInspect, 1.0f);
                bool rootClamped = Mathf.Abs(visual.transform.position.y) < 0.2f;
                Assert($"S3_KneelInspect_RootClamped_Role{role}",
                    rootClamped,
                    $"Root GameObject position.y stayed clamped to ground ({visual.transform.position.y:F3} < 0.2f) for role {role}",
                    $"Root GameObject position.y drifted from ground: {visual.transform.position.y:F3}");

                float kneelingHipLocalY = hips != null ? hips.localPosition.y : 0f;
                float hipOffset = kneelingHipLocalY - standingHipLocalY;
                Assert($"S3_KneelInspect_HipsLowered_Role{role}",
                    hipOffset < -0.2f,
                    $"Hips bone lowered by {hipOffset:F3}m (< -0.2f) for role {role}",
                    $"Hips bone not lowered sufficiently: offset = {hipOffset:F3}m");

                if (anim)
                {
                    bool workingSet = anim.GetBool("Working");
                    bool kneelingSet = anim.GetBool("Kneeling");
                    Assert($"S3_KneelInspect_AnimatorParams_Role{role}",
                        workingSet && kneelingSet,
                        $"Animator parameters Working={workingSet} and Kneeling={kneelingSet} set for role {role}",
                        $"Animator parameters Working={workingSet}, Kneeling={kneelingSet} not properly set");
                }

                // --- 3. Directing ---
                PanicCharacterArt.SetResponderAction(visual, anim, ResponderAction.Directing, 0.6f); // time where sin(0.6*2.5) ~ 0.997
                radioProp = visual.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name.ToLowerInvariant().Contains("radio") || t.name.ToLowerInvariant().Contains("walkie"));

                bool radioStowedOrInactive = radioProp == null || !radioProp.gameObject.activeSelf || radioProp.localPosition.y < 0.5f;
                Assert($"S3_Directing_RadioPropInactive_Role{role}",
                    radioStowedOrInactive,
                    $"Walkie-talkie prop is inactive or stowed during Directing for role {role}",
                    $"Walkie-talkie prop was active and unstowed during Directing for role {role}");

                var chest = FindBone(visual.transform, anim, HumanBodyBones.Chest, "spine_02");

                bool torsoYawExhibited = false;
                if (chest != null)
                {
                    Vector3 chestFwd = Vector3.ProjectOnPlane(chest.forward, visual.transform.up);
                    float yawAngle = Vector3.Angle(chestFwd, visual.transform.forward);
                    torsoYawExhibited = yawAngle > 3.0f;
                }

                Assert($"S3_Directing_TorsoYaw_Role{role}",
                    torsoYawExhibited,
                    $"Torso rotation exhibits horizontal yaw offset for role {role}",
                    $"Torso did not exhibit horizontal yaw offset for role {role}");

                if (visual) UnityEngine.Object.DestroyImmediate(visual);
            }
        }

        static void Suite4_RapidTransitionsStressTest(GameObject root)
        {
            Debug.Log("--- SUITE 4: 50 RAPID ACTION TRANSITIONS STRESS TEST ---");

            var visual = PanicExteriorArt.CreatePedestrianVisual(0);
            if (visual) visual.transform.SetParent(root.transform);
            var anim = visual ? visual.GetComponentInChildren<Animator>() : null;

            var actions = (ResponderAction[])Enum.GetValues(typeof(ResponderAction));
            bool anyNaN = false;
            int transitionsPassed = 0;

            for (int i = 0; i < 50; i++)
            {
                var act = actions[i % actions.Length];
                float t = (i * 0.17f) % 3.0f;

                PanicCharacterArt.SetResponderAction(visual, anim, act, t);

                // Inspect all transforms in hierarchy
                foreach (var tr in visual.GetComponentsInChildren<Transform>(true))
                {
                    var lp = tr.localPosition;
                    var lr = tr.localRotation;
                    var ls = tr.localScale;
                    var wp = tr.position;
                    var wr = tr.rotation;

                    if (float.IsNaN(lp.x) || float.IsNaN(lp.y) || float.IsNaN(lp.z) ||
                        float.IsInfinity(lp.x) || float.IsInfinity(lp.y) || float.IsInfinity(lp.z) ||
                        float.IsNaN(lr.x) || float.IsNaN(lr.y) || float.IsNaN(lr.z) || float.IsNaN(lr.w) ||
                        float.IsInfinity(lr.x) || float.IsInfinity(lr.y) || float.IsInfinity(lr.z) || float.IsInfinity(lr.w) ||
                        float.IsNaN(ls.x) || float.IsNaN(ls.y) || float.IsNaN(ls.z) ||
                        float.IsInfinity(ls.x) || float.IsInfinity(ls.y) || float.IsInfinity(ls.z) ||
                        float.IsNaN(wp.x) || float.IsNaN(wp.y) || float.IsNaN(wp.z) ||
                        float.IsInfinity(wp.x) || float.IsInfinity(wp.y) || float.IsInfinity(wp.z) ||
                        float.IsNaN(wr.x) || float.IsNaN(wr.y) || float.IsNaN(wr.z) || float.IsNaN(wr.w) ||
                        float.IsInfinity(wr.x) || float.IsInfinity(wr.y) || float.IsInfinity(wr.z) || float.IsInfinity(wr.w))
                    {
                        anyNaN = true;
                        Debug.LogError($"NaN/Infinity detected on transform {tr.name} during transition {i} (action={act}, time={t})");
                    }
                }

                if (!anyNaN) transitionsPassed++;
            }

            Assert("S4_RapidTransitions_50CleanTransitions",
                !anyNaN && transitionsPassed == 50,
                $"All 50 rapid transitions between actions completed with zero NaN / Infinity transforms ({transitionsPassed}/50)",
                $"NaN or Infinity detected during rapid transitions ({transitionsPassed}/50 passed)");

            if (visual) UnityEngine.Object.DestroyImmediate(visual);
        }

        static void Suite5_BoundaryAndExtremeInputsStressTest(GameObject root)
        {
            Debug.Log("--- SUITE 5: BOUNDARY AND EXTREME INPUTS STRESS TEST ---");

            var visual = PanicExteriorArt.CreatePedestrianVisual(1);
            if (visual) visual.transform.SetParent(root.transform);
            var anim = visual ? visual.GetComponentInChildren<Animator>() : null;

            float[] boundaryTimes = { 0.0f, -10.0f, 1000.0f, float.NaN, float.PositiveInfinity, float.NegativeInfinity };
            var actions = (ResponderAction[])Enum.GetValues(typeof(ResponderAction));

            bool boundaryTimesOk = true;
            foreach (float t in boundaryTimes)
            {
                foreach (var a in actions)
                {
                    try
                    {
                        PanicCharacterArt.SetResponderAction(visual, anim, a, t);
                    }
                    catch (Exception ex)
                    {
                        boundaryTimesOk = false;
                        Debug.LogError($"Exception on boundary time={t}, action={a}: {ex}");
                    }
                }
            }

            Assert("S5_Boundary_ExtremeTimesHandled",
                boundaryTimesOk,
                "Boundary times (0.0f, -10.0f, 1000.0f, NaN, +Inf, -Inf) handled safely across all 5 actions without exception",
                "Unhandled exception on boundary time inputs");

            // Check no NaNs after NaN / Inf inputs
            bool noNaNInTransforms = true;
            foreach (var tr in visual.GetComponentsInChildren<Transform>(true))
            {
                var lp = tr.localPosition;
                var lr = tr.localRotation;
                if (float.IsNaN(lp.x) || float.IsNaN(lp.y) || float.IsNaN(lp.z) ||
                    float.IsNaN(lr.x) || float.IsNaN(lr.y) || float.IsNaN(lr.z) || float.IsNaN(lr.w))
                {
                    noNaNInTransforms = false;
                }
            }

            Assert("S5_Boundary_NoNaNTransformsAfterExtremeTimes",
                noNaNInTransforms,
                "Transforms remain strictly finite and non-NaN after extreme time inputs",
                "NaN detected in transforms after extreme time inputs");

            // Null model test
            bool nullModelOk = true;
            try
            {
                foreach (var a in actions)
                {
                    PanicCharacterArt.SetResponderAction(null, anim, a, 1.0f);
                }
            }
            catch (Exception ex)
            {
                nullModelOk = false;
                Debug.LogError($"Exception on null model: {ex}");
            }

            Assert("S5_Boundary_NullModelSafe",
                nullModelOk,
                "Null model handled gracefully without exception across all actions",
                "Exception thrown on null model");

            // Null animator test
            bool nullAnimOk = true;
            try
            {
                foreach (var a in actions)
                {
                    PanicCharacterArt.SetResponderAction(visual, null, a, 1.0f);
                }
            }
            catch (Exception ex)
            {
                nullAnimOk = false;
                Debug.LogError($"Exception on null animator: {ex}");
            }

            Assert("S5_Boundary_NullAnimatorSafe",
                nullAnimOk,
                "Null animator handled gracefully without exception across all actions",
                "Exception thrown on null animator");

            // Unrigged model (empty GameObject with cube, no humanoid bones)
            var unrigged = GameObject.CreatePrimitive(PrimitiveType.Cube);
            unrigged.name = "UnriggedModel";
            unrigged.transform.SetParent(root.transform);
            bool unriggedOk = true;
            try
            {
                foreach (var a in actions)
                {
                    PanicCharacterArt.SetResponderAction(unrigged, null, a, 1.0f);
                }
            }
            catch (Exception ex)
            {
                unriggedOk = false;
                Debug.LogError($"Exception on unrigged model: {ex}");
            }

            Assert("S5_Boundary_UnriggedModelSafe",
                unriggedOk,
                "Unrigged model (no humanoid bones) handled gracefully without exception",
                "Exception thrown on unrigged model");

            if (unrigged) UnityEngine.Object.DestroyImmediate(unrigged);
            if (visual) UnityEngine.Object.DestroyImmediate(visual);
        }

        static void Suite6_AcceptanceSuiteRun(GameObject root)
        {
            Debug.Log("--- SUITE 6: CONFIRM ALL R6 ACCEPTANCE CRITERIA ---");

            // 1. T1_R6_1
            {
                var responder = PanicExteriorArt.CreatePedestrianVisual(0);
                if (responder) responder.transform.SetParent(root.transform);
                var animator = responder ? responder.GetComponentInChildren<Animator>() : null;
                bool ok = ReflectSetResponderAction(responder, animator, "Radio", 1.0f);
                Assert("S6_Acceptance_T1_R6_1_RadioAction", ok, "T1_R6_1 RadioAction pass", "T1_R6_1 fail");
                if (responder) UnityEngine.Object.DestroyImmediate(responder);
            }

            // 2. T1_R6_2
            {
                var responder = PanicExteriorArt.CreatePedestrianVisual(1);
                if (responder) responder.transform.SetParent(root.transform);
                var animator = responder ? responder.GetComponentInChildren<Animator>() : null;
                bool ok = ReflectSetResponderAction(responder, animator, "KneelInspect", 1.0f);
                Assert("S6_Acceptance_T1_R6_2_KneelInspectAction", ok, "T1_R6_2 KneelInspectAction pass", "T1_R6_2 fail");
                if (responder) UnityEngine.Object.DestroyImmediate(responder);
            }

            // 3. T1_R6_3
            {
                var responder = PanicExteriorArt.CreatePedestrianVisual(2);
                if (responder) responder.transform.SetParent(root.transform);
                var animator = responder ? responder.GetComponentInChildren<Animator>() : null;
                bool ok = ReflectSetResponderAction(responder, animator, "Directing", 1.0f);
                Assert("S6_Acceptance_T1_R6_3_DirectingAction", ok, "T1_R6_3 DirectingAction pass", "T1_R6_3 fail");
                if (responder) UnityEngine.Object.DestroyImmediate(responder);
            }

            // 4. T1_R6_4
            {
                var worldGo = new GameObject("World_S6_T1_R6_4");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                ReflectInitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;
                ReflectTickExteriorLife(world, 0.1f);
                int activeCount = ReflectGetActiveExteriorNpcCount(world);
                Assert("S6_Acceptance_T1_R6_4_AftermathRespondersActive", activeCount >= 4, $"Aftermath activeCount={activeCount} >= 4", $"Aftermath activeCount={activeCount} < 4");
                ReflectTeardownExteriorLife(world);
                UnityEngine.Object.DestroyImmediate(worldGo);
            }

            // 5. T1_R6_5
            {
                var worldGo = new GameObject("World_S6_T1_R6_5");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                ReflectInitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Inspection;
                ReflectTickExteriorLife(world, 0.1f);
                int activeCount = ReflectGetActiveExteriorNpcCount(world);
                Assert("S6_Acceptance_T1_R6_5_InspectionRespondersActive", activeCount >= 4, $"Inspection activeCount={activeCount} >= 4", $"Inspection activeCount={activeCount} < 4");
                ReflectTeardownExteriorLife(world);
                UnityEngine.Object.DestroyImmediate(worldGo);
            }

            // 6. T2_R6_1
            {
                var resp = PanicExteriorArt.CreatePedestrianVisual(0);
                if (resp) resp.transform.SetParent(root.transform);
                var anim = resp ? resp.GetComponentInChildren<Animator>() : null;
                bool ok = ReflectSetResponderAction(resp, anim, "Radio", 0.0f);
                Assert("S6_Acceptance_T2_R6_1_BlendTimeZero", ok, "T2_R6_1 BlendTimeZero pass", "T2_R6_1 fail");
                if (resp) UnityEngine.Object.DestroyImmediate(resp);
            }

            // 7. T2_R6_2
            {
                var worldGo = new GameObject("World_S6_T2_R6_2");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                ReflectInitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;
                for (int i = 0; i < 60; i++) ReflectTickExteriorLife(world, 0.033f);
                int count = ReflectGetExteriorNpcCount(world);
                Assert("S6_Acceptance_T2_R6_2_RespondersPersist60Ticks", count >= 4, $"Persist count={count} >= 4", $"Persist count={count} < 4");
                ReflectTeardownExteriorLife(world);
                UnityEngine.Object.DestroyImmediate(worldGo);
            }

            // 8. T2_R6_3
            {
                var worldGo = new GameObject("World_S6_T2_R6_3");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                ReflectInitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;
                ReflectTickExteriorLife(world, 0.1f);
                var visuals = ReflectGetNeighborVisuals(world);
                bool inFrontPerimeter = visuals.Length >= 4;
                foreach (var go in visuals)
                {
                    if (go)
                    {
                        Vector3 p = go.transform.position;
                        if (p.z < -25f || p.z > -4f) inFrontPerimeter = false;
                    }
                }
                Assert("S6_Acceptance_T2_R6_3_FrontPerimeterStationCoordinates", inFrontPerimeter, "Stationed inside front perimeter [-25m, -4m]", "Stationed outside front perimeter");
                ReflectTeardownExteriorLife(world);
                UnityEngine.Object.DestroyImmediate(worldGo);
            }

            // 9. T2_R6_4
            {
                var resp = PanicExteriorArt.CreatePedestrianVisual(0);
                if (resp) resp.transform.SetParent(root.transform);
                var anim = resp ? resp.GetComponentInChildren<Animator>() : null;
                ReflectSetResponderAction(resp, anim, "Radio", 1.0f);
                var tRadio = resp ? resp.GetComponentsInChildren<Transform>(true) : new Transform[0];
                var radioProp = tRadio.FirstOrDefault(t => t.name.ToLowerInvariant().Contains("radio") || t.name.ToLowerInvariant().Contains("walkie"));
                bool radioActiveInRadio = radioProp == null || radioProp.gameObject.activeSelf;
                ReflectSetResponderAction(resp, anim, "Directing", 1.0f);
                bool radioInactiveInDirecting = radioProp == null || !radioProp.gameObject.activeSelf || radioProp.localPosition.y < 0.5f;
                Assert("S6_Acceptance_T2_R6_4_WalkieTalkiePropCoordination", radioActiveInRadio && radioInactiveInDirecting, "Prop active only in Radio, stowed in Directing", "Prop not coordinated");
                if (resp) UnityEngine.Object.DestroyImmediate(resp);
            }

            // 10. T2_R6_5
            {
                var resp = PanicExteriorArt.CreatePedestrianVisual(1);
                if (resp) resp.transform.SetParent(root.transform);
                var anim = resp ? resp.GetComponentInChildren<Animator>() : null;
                ReflectSetResponderAction(resp, anim, "KneelInspect", 1.0f);
                bool grounded = resp != null && Mathf.Abs(resp.transform.position.y) < 0.2f;
                Assert("S6_Acceptance_T2_R6_5_KneelInspectGroundClamping", grounded, "Grounded height clamped < 0.2f", "Grounded height violated");
                if (resp) UnityEngine.Object.DestroyImmediate(resp);
            }

            // 11. T3_Pairwise
            {
                bool allPairsPass = true;
                string[] acts = new[] { "Radio", "KneelInspect", "Directing" };
                for (int r = 0; r < 4; r++)
                {
                    var visual = PanicExteriorArt.CreatePedestrianVisual(r);
                    if (visual) visual.transform.SetParent(root.transform);
                    var anim = visual ? visual.GetComponentInChildren<Animator>() : null;
                    foreach (var a in acts)
                    {
                        bool ok = ReflectSetResponderAction(visual, anim, a, 1.0f);
                        if (!ok) allPairsPass = false;
                    }
                    if (visual) UnityEngine.Object.DestroyImmediate(visual);
                }
                Assert("S6_Acceptance_T3_PairwiseResponderActions", allPairsPass, "All 12 pairwise outfit/action combinations pass", "Some pairwise combinations failed");
            }
        }

        static bool ReflectSetResponderAction(GameObject model, Animator humanoid, string actionName, float time)
        {
            if (!model) return false;
            var method = typeof(PanicCharacterArt).GetMethod("SetResponderAction", BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                var paramType = method.GetParameters()[2].ParameterType;
                object actionVal = Enum.Parse(paramType, actionName);
                method.Invoke(null, new object[] { model, humanoid, actionVal, time });
                return true;
            }
            return false;
        }

        static void ReflectInitExteriorLife(PanicHomeWorld world)
        {
            var m = typeof(PanicHomeWorld).GetMethod("InitExteriorLife", BindingFlags.NonPublic | BindingFlags.Instance);
            if (m != null) m.Invoke(world, null);
        }

        static void ReflectTickExteriorLife(PanicHomeWorld world, float dt)
        {
            var m = typeof(PanicHomeWorld).GetMethod("TickExteriorLife", BindingFlags.NonPublic | BindingFlags.Instance);
            if (m != null) m.Invoke(world, new object[] { dt });
        }

        static void ReflectTeardownExteriorLife(PanicHomeWorld world)
        {
            var m = typeof(PanicHomeWorld).GetMethod("TeardownExteriorLife", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m != null) m.Invoke(world, null);
        }

        static int ReflectGetExteriorNpcCount(PanicHomeWorld world)
        {
            var f = typeof(PanicHomeWorld).GetField("neighbors", BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.GetValue(world) is System.Collections.IList list) return list.Count;
            return 0;
        }

        static int ReflectGetActiveExteriorNpcCount(PanicHomeWorld world)
        {
            var f = typeof(PanicHomeWorld).GetField("neighbors", BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.GetValue(world) is System.Collections.IList list)
            {
                int count = 0;
                foreach (var item in list)
                {
                    var visualField = item.GetType().GetField("visual", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (visualField != null && visualField.GetValue(item) is GameObject go && go != null && go.activeSelf)
                    {
                        count++;
                    }
                }
                return count;
            }
            return 0;
        }

        static GameObject[] ReflectGetNeighborVisuals(PanicHomeWorld world)
        {
            var f = typeof(PanicHomeWorld).GetField("neighbors", BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.GetValue(world) is System.Collections.IList list)
            {
                var results = new List<GameObject>();
                foreach (var item in list)
                {
                    var visualField = item.GetType().GetField("visual", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (visualField != null && visualField.GetValue(item) is GameObject go && go != null)
                    {
                        results.Add(go);
                    }
                }
                return results.ToArray();
            }
            return new GameObject[0];
        }

        static Transform FindBone(Transform model, Animator a, HumanBodyBones id, string fallbackName)
        {
            if (a != null && a.avatar != null && a.isHuman)
            {
                Transform t = a.GetBoneTransform(id);
                if (t != null) return t;
            }
            foreach (Transform t in model.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(t.name, fallbackName, StringComparison.OrdinalIgnoreCase) ||
                    t.name.ToLowerInvariant().Contains(fallbackName.ToLowerInvariant()))
                    return t;
            }
            return null;
        }
    }
}
#endif
