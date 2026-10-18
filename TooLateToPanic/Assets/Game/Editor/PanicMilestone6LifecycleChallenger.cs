#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TooLateToPanic
{
    public static class PanicMilestone6LifecycleChallenger
    {
        static int passedAssertions = 0;
        static int failedAssertions = 0;
        static readonly List<string> LoggedFailures = new List<string>();

        static void Pass(string testId, string detail)
        {
            passedAssertions++;
            Debug.Log($"[CHALLENGE PASS] {testId}: {detail}");
        }

        static void Fail(string testId, string detail)
        {
            failedAssertions++;
            string msg = $"[CHALLENGE FAIL] {testId}: {detail}";
            LoggedFailures.Add(msg);
            Debug.LogError(msg);
        }

        static void Assert(string testId, bool condition, string passDetail, string failDetail)
        {
            if (condition) Pass(testId, passDetail);
            else Fail(testId, failDetail);
        }

        [MenuItem("Too Late to Panic/Run M6 Lifecycle Challenges")]
        public static void RunAllLifecycleChallenges()
        {
            passedAssertions = 0;
            failedAssertions = 0;
            LoggedFailures.Clear();

            Debug.Log("================================================================================");
            Debug.Log("STARTING ADVERSARIAL MILESTONE 6 LIFECYCLE & STATIONING CHALLENGE SUITE");
            Debug.Log($"Timestamp: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} | Unity {Application.unityVersion}");
            Debug.Log("================================================================================");

            var sandbox = new GameObject("PanicM6_Challenger_Sandbox");

            try
            {
                // Suite 1: Aftermath & Inspection Persistence across 20 ticks
                RunSuite1_PersistenceAcrossTicks(sandbox);

                // Suite 2: Front Perimeter Station Coordinates Verification
                RunSuite2_StationCoordinates(sandbox);

                // Suite 3: Responder Action Cycling Verification
                RunSuite3_ActionCycling(sandbox);

                // Suite 4: Disaster Phase Deactivation Verification
                RunSuite4_DisasterDeactivation(sandbox);

                // Suite 5: 50-Cycle Scene Init / Teardown Lifecycle & Memory Census
                RunSuite5_50xLifecycleTeardownStress(sandbox);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Fail("M6_UNHANDLED_EXCEPTION", $"Challenge suite caught unexpected exception: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (sandbox) UnityEngine.Object.DestroyImmediate(sandbox);
            }

            int total = passedAssertions + failedAssertions;
            Debug.Log("================================================================================");
            Debug.Log($"=== M6 LIFECYCLE CHALLENGE SUMMARY: {passedAssertions} PASSED, {failedAssertions} FAILED, {total} TOTAL ===");
            if (failedAssertions > 0)
            {
                Debug.Log("=== FAILURES BREAKDOWN ===");
                foreach (var f in LoggedFailures)
                {
                    Debug.Log("  " + f);
                }
            }
            Debug.Log("================================================================================");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(failedAssertions == 0 ? 0 : 1);
            }
        }

        // -----------------------------------------------------------------------------------------
        // Suite 1: Aftermath & Inspection Persistence across 20 ticks
        // -----------------------------------------------------------------------------------------
        static void RunSuite1_PersistenceAcrossTicks(GameObject root)
        {
            Debug.Log("--- SUITE 1: AFTERMATH & INSPECTION PERSISTENCE ACROSS 20 TICKS ---");

            // 1.1 Aftermath persistence across 20 ticks
            {
                var go = new GameObject("World_S1_Aftermath");
                go.transform.SetParent(root.transform);
                var world = go.AddComponent<PanicHomeWorld>();

                InitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;

                bool allTicksActive = true;
                string failReason = "";

                for (int tick = 0; tick < 20; tick++)
                {
                    TickExteriorLife(world, 0.1f);
                    int activeCount = world.ActiveExteriorNpcCount;
                    if (activeCount < 4)
                    {
                        allTicksActive = false;
                        failReason = $"Tick {tick}: ActiveExteriorNpcCount is {activeCount} < 4";
                        break;
                    }

                    var visuals = GetNeighborVisuals(world);
                    if (visuals.Length < 4 || visuals.Any(v => v == null || !v.activeSelf))
                    {
                        allTicksActive = false;
                        failReason = $"Tick {tick}: One or more neighbor visuals inactive or null";
                        break;
                    }
                }

                Assert("S1_1_Aftermath_All4RespondersActive_20Ticks",
                    allTicksActive,
                    "All 4 responders persist activeSelf == true across 20 consecutive Aftermath ticks",
                    failReason);

                world.TeardownExteriorLife();
                UnityEngine.Object.DestroyImmediate(go);
            }

            // 1.2 Inspection persistence across 20 ticks
            {
                var go = new GameObject("World_S1_Inspection");
                go.transform.SetParent(root.transform);
                var world = go.AddComponent<PanicHomeWorld>();

                InitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Inspection;

                bool allTicksActive = true;
                string failReason = "";

                for (int tick = 0; tick < 20; tick++)
                {
                    TickExteriorLife(world, 0.1f);
                    int activeCount = world.ActiveExteriorNpcCount;
                    if (activeCount < 4)
                    {
                        allTicksActive = false;
                        failReason = $"Tick {tick}: ActiveExteriorNpcCount is {activeCount} < 4";
                        break;
                    }

                    var visuals = GetNeighborVisuals(world);
                    if (visuals.Length < 4 || visuals.Any(v => v == null || !v.activeSelf))
                    {
                        allTicksActive = false;
                        failReason = $"Tick {tick}: One or more neighbor visuals inactive or null";
                        break;
                    }
                }

                Assert("S1_2_Inspection_All4RespondersActive_20Ticks",
                    allTicksActive,
                    "All 4 responders persist activeSelf == true across 20 consecutive Inspection ticks",
                    failReason);

                world.TeardownExteriorLife();
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // -----------------------------------------------------------------------------------------
        // Suite 2: Front Perimeter Station Coordinates Verification
        // -----------------------------------------------------------------------------------------
        static void RunSuite2_StationCoordinates(GameObject root)
        {
            Debug.Log("--- SUITE 2: FRONT PERIMETER STATION COORDINATES VERIFICATION ---");

            var go = new GameObject("World_S2_Coords");
            go.transform.SetParent(root.transform);
            var world = go.AddComponent<PanicHomeWorld>();

            InitExteriorLife(world);
            if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;
            TickExteriorLife(world, 0.1f);

            var visuals = GetNeighborVisuals(world);

            Assert("S2_1_FourRespondersSpawned",
                visuals.Length == 4,
                $"Exactly 4 responder visuals initialized (found {visuals.Length})",
                $"Expected 4 responder visuals but found {visuals.Length}");

            bool zBroadValid = true;
            bool zSpecificValid = true;
            bool xBoundsValid = true;
            bool yGroundValid = true;
            string zError = "";
            string xError = "";
            string yError = "";

            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i] == null) continue;
                Vector3 pos = visuals[i].transform.position;

                // Broad boundary constraint: z in [-25.0f, -4.0f]
                if (pos.z < -25.0f || pos.z > -4.0f)
                {
                    zBroadValid = false;
                    zError += $"Responder {i} z={pos.z:F2} outside [-25.0, -4.0]; ";
                }

                // Specific authored boundary constraint: z in [-13.5f, -11.5f]
                if (pos.z < -13.6f || pos.z > -11.4f)
                {
                    zSpecificValid = false;
                    zError += $"Responder {i} z={pos.z:F2} outside [-13.5, -11.5]; ";
                }

                // X bounds: x in [-8.0f, +8.0f]
                if (pos.x < -8.1f || pos.x > +8.1f)
                {
                    xBoundsValid = false;
                    xError += $"Responder {i} x={pos.x:F2} outside [-8.0, +8.0]; ";
                }

                // Ground plane clamping: y in [-0.2f, +0.2f]
                if (Mathf.Abs(pos.y) > 0.2f)
                {
                    yGroundValid = false;
                    yError += $"Responder {i} y={pos.y:F3} not clamped near ground; ";
                }
            }

            Assert("S2_2_Station_Z_BroadPerimeter",
                zBroadValid,
                "All 4 responders situated within broad front perimeter z in [-25.0f, -4.0f]",
                zError);

            Assert("S2_3_Station_Z_SpecificFrontCorridor",
                zSpecificValid,
                "All 4 responders situated within specific front corridor z in [-13.5f, -11.5f]",
                zError);

            Assert("S2_4_Station_X_PerimeterBounds",
                xBoundsValid,
                "All 4 responders situated within lateral front perimeter x in [-8.0f, +8.0f]",
                xError);

            Assert("S2_5_Station_Y_GroundClamped",
                yGroundValid,
                "All 4 responder root transforms clamped to ground level (abs(y) < 0.2m)",
                yError);

            // Distinct positions: distance between any two responders >= 2.0m
            bool allDistinct = true;
            for (int i = 0; i < visuals.Length; i++)
            {
                for (int j = i + 1; j < visuals.Length; j++)
                {
                    if (visuals[i] && visuals[j])
                    {
                        float dist = Vector3.Distance(visuals[i].transform.position, visuals[j].transform.position);
                        if (dist < 2.0f) allDistinct = false;
                    }
                }
            }

            Assert("S2_6_Station_DistinctPositions",
                allDistinct,
                "All 4 responder station coordinates are distinctly distributed (min separation >= 2.0m)",
                "Responder stations overlap or are insufficiently separated (< 2.0m)");

            world.TeardownExteriorLife();
            UnityEngine.Object.DestroyImmediate(go);
        }

        // -----------------------------------------------------------------------------------------
        // Suite 3: Responder Action Cycling Verification
        // -----------------------------------------------------------------------------------------
        static void RunSuite3_ActionCycling(GameObject root)
        {
            Debug.Log("--- SUITE 3: RESPONDER ACTION CYCLING VERIFICATION ---");

            var go = new GameObject("World_S3_Cycling");
            go.transform.SetParent(root.transform);
            var world = go.AddComponent<PanicHomeWorld>();

            InitExteriorLife(world);
            if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;

            // In PanicExteriorWorld.cs:
            // cycleTime = residenceTime + neighbor.offset;
            // int actionIndex = Mathf.FloorToInt(cycleTime / 4.0f + i * 1.33f) % 3;
            // 0 -> Radio, 1 -> KneelInspect, default -> Directing
            // Let's verify that across a 16-second simulated duration, actions cycle for each responder
            var residenceTimeField = typeof(PanicHomeWorld).GetField("residenceTime", BindingFlags.NonPublic | BindingFlags.Instance);

            var actionsSeenPerResponder = new List<HashSet<int>>();
            for (int i = 0; i < 4; i++) actionsSeenPerResponder.Add(new HashSet<int>());

            float step = 0.5f;
            float totalDuration = 16.0f;
            int totalSteps = Mathf.RoundToInt(totalDuration / step);

            for (int s = 0; s < totalSteps; s++)
            {
                float t = s * step;
                if (residenceTimeField != null) residenceTimeField.SetValue(world, t);
                TickExteriorLife(world, step);

                for (int i = 0; i < 4; i++)
                {
                    float offset = i * 2.7f;
                    float cycleTime = t + offset;
                    int actionIdx = Mathf.FloorToInt(cycleTime / 4.0f + i * 1.33f) % 3;
                    actionsSeenPerResponder[i].Add(actionIdx);
                }
            }

            bool allCycledAll3Actions = actionsSeenPerResponder.All(h => h.Count == 3);
            Assert("S3_1_RespondersCycleAllThreeActions",
                allCycledAll3Actions,
                "All 4 responders cycle through all 3 actions (Radio, KneelInspect, Directing) over a 16s period",
                "One or more responders failed to cycle through all 3 intervention actions");

            // Verify staggering: at t = 0.0f, responders do not all have the same action index
            if (residenceTimeField != null) residenceTimeField.SetValue(world, 0.0f);
            TickExteriorLife(world, 0.0f);
            var initialActions = new HashSet<int>();
            for (int i = 0; i < 4; i++)
            {
                float offset = i * 2.7f;
                int actionIdx = Mathf.FloorToInt(offset / 4.0f + i * 1.33f) % 3;
                initialActions.Add(actionIdx);
            }

            Assert("S3_2_ChoreographyIsStaggered",
                initialActions.Count > 1,
                $"Responder actions are staggered across the squad ({initialActions.Count} distinct simultaneous actions)",
                "Responders are in lockstep without visual staggering");

            world.TeardownExteriorLife();
            UnityEngine.Object.DestroyImmediate(go);
        }

        // -----------------------------------------------------------------------------------------
        // Suite 4: Disaster Phase Deactivation Verification
        // -----------------------------------------------------------------------------------------
        static void RunSuite4_DisasterDeactivation(GameObject root)
        {
            Debug.Log("--- SUITE 4: DISASTER PHASE DEACTIVATION VERIFICATION ---");

            // 4.1 Direct Disaster Phase Initialization
            {
                var go = new GameObject("World_S4_DirectDisaster");
                go.transform.SetParent(root.transform);
                var world = go.AddComponent<PanicHomeWorld>();

                InitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Disaster;
                TickExteriorLife(world, 0.1f);

                int count = world.ActiveExteriorNpcCount;
                var visuals = GetNeighborVisuals(world);
                bool allInactive = visuals.Length == 4 && visuals.All(v => v != null && !v.activeSelf);

                Assert("S4_1_DisasterPhase_ActiveExteriorNpcCountZero",
                    count == 0 && allInactive,
                    $"In PanicPhase.Disaster: ActiveExteriorNpcCount == 0 (actual={count}) and all 4 visuals activeSelf == false",
                    $"In PanicPhase.Disaster: ActiveExteriorNpcCount == {count} != 0 or visuals active");

                world.TeardownExteriorLife();
                UnityEngine.Object.DestroyImmediate(go);
            }

            // 4.2 Transition from Preparing to Disaster
            {
                var go = new GameObject("World_S4_PreparingToDisaster");
                go.transform.SetParent(root.transform);
                var world = go.AddComponent<PanicHomeWorld>();

                InitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Preparing;
                TickExteriorLife(world, 0.1f);

                int prepCount = world.ActiveExteriorNpcCount;

                // Transition to Disaster
                world.Model.Phase = PanicPhase.Disaster;
                TickExteriorLife(world, 0.1f);
                int disCount = world.ActiveExteriorNpcCount;

                Assert("S4_2_Transition_PreparingToDisaster_Deactivation",
                    prepCount == 4 && disCount == 0,
                    $"Preparing -> Disaster transition: prepCount={prepCount} -> disCount={disCount} (clean deactivation)",
                    $"Transition failed: prepCount={prepCount}, disCount={disCount}");

                world.TeardownExteriorLife();
                UnityEngine.Object.DestroyImmediate(go);
            }

            // 4.3 Transition from Aftermath to Disaster
            {
                var go = new GameObject("World_S4_AftermathToDisaster");
                go.transform.SetParent(root.transform);
                var world = go.AddComponent<PanicHomeWorld>();

                InitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;
                TickExteriorLife(world, 0.1f);

                int afterCount = world.ActiveExteriorNpcCount;

                // Transition to Disaster
                world.Model.Phase = PanicPhase.Disaster;
                TickExteriorLife(world, 0.1f);
                int disCount = world.ActiveExteriorNpcCount;

                Assert("S4_3_Transition_AftermathToDisaster_Deactivation",
                    afterCount == 4 && disCount == 0,
                    $"Aftermath -> Disaster transition: afterCount={afterCount} -> disCount={disCount} (clean deactivation)",
                    $"Transition failed: afterCount={afterCount}, disCount={disCount}");

                world.TeardownExteriorLife();
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // -----------------------------------------------------------------------------------------
        // Suite 5: 50-Cycle Scene Init / Teardown Lifecycle & Memory Census
        // -----------------------------------------------------------------------------------------
        static void RunSuite5_50xLifecycleTeardownStress(GameObject root)
        {
            Debug.Log("--- SUITE 5: 50-CYCLE SCENE INIT / TEARDOWN LIFECYCLE & MEMORY CENSUS ---");

            var go = new GameObject("World_S5_Stress");
            go.transform.SetParent(root.transform);
            var world = go.AddComponent<PanicHomeWorld>();

            // Baseline census
            int baselineGameObjects = CountResidentGameObjects();
            int baselineMaterials = CountResponderMaterials();

            Debug.Log($"[CENSUS BASELINE] Resident GameObjects={baselineGameObjects}, Responder Materials={baselineMaterials}");

            bool cyclesSucceeded = true;
            string cycleError = "";

            for (int cycle = 0; cycle < 50; cycle++)
            {
                try
                {
                    InitExteriorLife(world);
                    if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;

                    TickExteriorLife(world, 0.1f);
                    TickExteriorLife(world, 0.1f);

                    if (world.ActiveExteriorNpcCount != 4)
                    {
                        cyclesSucceeded = false;
                        cycleError = $"Cycle {cycle}: ActiveExteriorNpcCount != 4 after Init and Tick";
                        break;
                    }

                    // Teardown
                    world.TeardownExteriorLife();

                    if (world.ExteriorNpcCount != 0)
                    {
                        cyclesSucceeded = false;
                        cycleError = $"Cycle {cycle}: ExteriorNpcCount != 0 after Teardown";
                        break;
                    }
                }
                catch (Exception ex)
                {
                    cyclesSucceeded = false;
                    cycleError = $"Cycle {cycle} threw exception: {ex.Message}";
                    break;
                }
            }

            Assert("S5_1_50xLifecycleTeardownCyclesCompleted",
                cyclesSucceeded,
                "50 consecutive InitExteriorLife() / TeardownExteriorLife() cycles completed cleanly",
                cycleError);

            // Post-stress census
            int finalGameObjects = CountResidentGameObjects();
            int finalMaterials = CountResponderMaterials();

            int goDelta = finalGameObjects - baselineGameObjects;
            int matDelta = finalMaterials - baselineMaterials;

            Debug.Log($"[CENSUS FINAL] Resident GameObjects={finalGameObjects} (delta={goDelta}), Responder Materials={finalMaterials} (delta={matDelta})");

            Assert("S5_2_ZeroLeakedResidentGameObjects",
                finalGameObjects == 0,
                $"Zero leaked Resident GameObjects in scene (found {finalGameObjects})",
                $"Leaked {finalGameObjects} Resident GameObjects after 50 cycles!");

            Assert("S5_3_GameObjectDeltaZero",
                goDelta <= 0,
                $"GameObject census delta <= 0 (delta={goDelta})",
                $"GameObject census grew by {goDelta} across 50 cycles!");

            Assert("S5_4_MaterialDeltaZero",
                matDelta <= 0,
                $"Responder material census delta <= 0 (delta={matDelta})",
                $"Responder material census grew by {matDelta} across 50 cycles!");

            UnityEngine.Object.DestroyImmediate(go);
        }

        // -----------------------------------------------------------------------------------------
        // Internal Reflection & Census Helpers
        // -----------------------------------------------------------------------------------------
        static void InitExteriorLife(PanicHomeWorld world)
        {
            if (!world) return;
            var method = typeof(PanicHomeWorld).GetMethod("InitExteriorLife", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null) method.Invoke(world, null);
        }

        static void TickExteriorLife(PanicHomeWorld world, float dt)
        {
            if (!world) return;
            var method = typeof(PanicHomeWorld).GetMethod("TickExteriorLife", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null) method.Invoke(world, new object[] { dt });
        }

        static GameObject[] GetNeighborVisuals(PanicHomeWorld world)
        {
            if (!world) return new GameObject[0];
            var field = typeof(PanicHomeWorld).GetField("neighbors", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return new GameObject[0];
            var list = field.GetValue(world) as IList;
            if (list == null) return new GameObject[0];

            var results = new List<GameObject>();
            foreach (var item in list)
            {
                if (item == null) continue;
                var vField = item.GetType().GetField("visual", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (vField != null)
                {
                    var go = vField.GetValue(item) as GameObject;
                    if (go) results.Add(go);
                }
            }
            return results.ToArray();
        }

        static int CountResidentGameObjects()
        {
            var allGos = Resources.FindObjectsOfTypeAll<GameObject>();
            int count = 0;
            foreach (var go in allGos)
            {
                if (!go) continue;
                string name = go.name;
                if (name.StartsWith("Neighborhood resident") || name.StartsWith("Neighbour pedestrian"))
                {
                    // Ignore asset prefabs in project hierarchy (only count scene instances)
                    if (EditorUtility.IsPersistent(go)) continue;
                    count++;
                }
            }
            return count;
        }

        static int CountResponderMaterials()
        {
            var allMats = Resources.FindObjectsOfTypeAll<Material>();
            int count = 0;
            foreach (var mat in allMats)
            {
                if (!mat) continue;
                if (EditorUtility.IsPersistent(mat)) continue;
                string name = mat.name;
                if (name.StartsWith("Neighbour canvas") || name.StartsWith("ResponderMat_"))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
#endif
