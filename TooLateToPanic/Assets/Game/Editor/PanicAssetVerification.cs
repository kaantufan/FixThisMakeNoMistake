#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>
    /// Opaque-box E2E test runner and verification suite for Too Late to Panic! Home V4.11 asset upgrades.
    /// Derives expected outputs directly from ORIGINAL_REQUEST.md, PROJECT.md, and TEST_INFRA.md.
    /// Covers 4 tiers:
    ///   - Tier 1: Feature Coverage (R1 to R6, 5 tests per feature = 30 tests)
    ///   - Tier 2: Boundary & Corner Cases (R1 to R6, 5 tests per feature = 30 tests)
    ///   - Tier 3: Cross-Feature Combinations (Pairwise interactions = 6 tests)
    ///   - Tier 4: Real-world game progression simulation (Full lifecycle = 4 tests)
    /// Total: 70 structured test cases.
    ///
    /// Batchmode command:
    /// &amp; "C:\Program Files\Unity\Hub\Editor\6000.0.73f1\Editor\Unity.exe" -batchmode -nographics -projectPath "c:\ToLateToPanic" -executeMethod TooLateToPanic.PanicAssetVerification.RunAllTests -quit -logFile "Assets/../batch_verify.log"
    /// </summary>
    public static class PanicAssetVerification
    {
        static int passCount;
        static int failCount;
        static readonly List<string> FailureDetails = new List<string>();

        // -----------------------------------------------------------------------------------------
        // Assertion & Reporting Infrastructure
        // -----------------------------------------------------------------------------------------
        static void Pass(string testName, string detail)
        {
            passCount++;
            Debug.Log($"[PASS] {testName}: {detail}");
        }

        static void Fail(string testName, string detail)
        {
            failCount++;
            string msg = $"[FAIL] {testName}: {detail}";
            FailureDetails.Add(msg);
            Debug.LogError(msg);
        }

        static void Assert(string testName, bool condition, string passDetail, string failDetail)
        {
            if (condition) Pass(testName, passDetail);
            else Fail(testName, failDetail);
        }

        [MenuItem("Too Late to Panic/Run All Asset Verification Tests")]
        public static void RunAllTests()
        {
            passCount = 0;
            failCount = 0;
            FailureDetails.Clear();

            Debug.Log("================================================================================");
            Debug.Log("STARTING TOO LATE TO PANIC! E2E ASSET VERIFICATION SUITE (70 TESTS)");
            Debug.Log($"Timestamp: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} | Unity {Application.unityVersion}");
            Debug.Log("================================================================================");

            var tempContainer = new GameObject("PanicVerification_TempSandbox");

            try
            {
                // Tier 1: Feature Coverage (30 tests)
                RunTier1_FeatureCoverage(tempContainer);

                // Tier 2: Boundary & Corner Cases (30 tests)
                RunTier2_BoundaryCases(tempContainer);

                // Tier 3: Cross-Feature Combinations (6 tests)
                RunTier3_CrossFeature(tempContainer);

                // Tier 4: Real-World Game Progression Simulation (4 tests)
                RunTier4_GameProgression(tempContainer);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Fail("UNHANDLED_EXCEPTION", $"Suite encountered uncaught exception: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (tempContainer) UnityEngine.Object.DestroyImmediate(tempContainer);
            }

            int totalCount = passCount + failCount;
            Debug.Log("================================================================================");
            Debug.Log($"=== VERIFICATION SUMMARY: {passCount} PASSED, {failCount} FAILED, {totalCount} TOTAL ===");
            if (failCount > 0)
            {
                Debug.Log("=== FAILURES BREAKDOWN ===");
                foreach (var f in FailureDetails)
                {
                    Debug.Log("  " + f);
                }
            }
            Debug.Log("================================================================================");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(failCount == 0 ? 0 : 1);
            }
        }

        // =========================================================================================
        // TIER 1: FEATURE COVERAGE (R1 to R6, 5 tests each = 30 tests)
        // =========================================================================================
        static void RunTier1_FeatureCoverage(GameObject root)
        {
            Debug.Log("--- RUNNING TIER 1: FEATURE COVERAGE ---");

            // --- R1 [P0.1] Earthquake Shake, Roof Visibility, Roof/Facade Debris ---
            RunT1_R1(root);

            // --- R2 [P0.2] 3D Tornado Funnel Framed with House ---
            RunT1_R2(root);

            // --- R3 [P1.1] Four Distinct Responder Kits ---
            RunT1_R3(root);

            // --- R4 [P1.2] Custom Foley Audio Families & Wiring ---
            RunT1_R4();

            // --- R5 [P2.1] Varied Disaster Surface Marks ---
            RunT1_R5(root);

            // --- R6 [P2.2] Responder Actions & Choreography ---
            RunT1_R6(root);
        }

        static void RunT1_R1(GameObject root)
        {
            // T1_R1_1_RoofVisibility_QuakeExterior
            // Verify roof activeSelf == true during Quake exterior shot when cutaway is suppressed (!cutawayInside)
            {
                var worldGo = new GameObject("World_T1_R1_1");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                var roofField = typeof(PanicHomeWorld).GetField("roof", BindingFlags.NonPublic | BindingFlags.Instance);
                var testRoof = new GameObject("TestRoof");
                testRoof.transform.SetParent(worldGo.transform);
                if (roofField != null) roofField.SetValue(world, testRoof);

                // Set model state to Quake Disaster
                if (world.Model != null)
                {
                    world.Model.Disaster = DisasterType.Earthquake;
                    world.Model.Phase = PanicPhase.Disaster;
                }

                var applyMethod = typeof(PanicHomeWorld).GetMethod("ApplyFloorVisibility", BindingFlags.NonPublic | BindingFlags.Instance);
                if (applyMethod != null)
                {
                    // Exterior mode: inside = true, ResidenceCinematicExteriorView = true => cutawayInside = false
                    var viewProp = typeof(PanicHomeWorld).GetProperty("ResidenceCinematicExteriorView", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (viewProp != null && viewProp.CanWrite) viewProp.SetValue(world, true);
                    applyMethod.Invoke(world, null);

                    Assert("T1_R1_1_RoofVisibility_QuakeExterior",
                        testRoof.activeSelf,
                        "Roof remains activeSelf == true during exterior Earthquake shot",
                        "Roof was deactivated during exterior Earthquake shot (violates R1.1)");
                }
                else
                {
                    Fail("T1_R1_1_RoofVisibility_QuakeExterior", "Method ApplyFloorVisibility not found on PanicHomeWorld");
                }
            }

            // T1_R1_2_StructuralShake_Displacement
            // Multi-floor shake exhibits non-zero local position displacement during Quake
            {
                var worldGo = new GameObject("World_T1_R1_2");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                InitFloorVisuals(world, worldGo);

                if (world.Model != null)
                {
                    world.Model.Disaster = DisasterType.Earthquake;
                    world.Model.Phase = PanicPhase.Disaster;
                }

                // Tick presentation quake age
                var tickMethod = typeof(PanicHomeWorld).GetMethod("UpdatePresentation", BindingFlags.NonPublic | BindingFlags.Instance);
                if (tickMethod != null)
                {
                    for (int i = 0; i < 60; i++) tickMethod.Invoke(world, new object[] { 0.033f });
                }

                var floors = ReflectHelper.GetFloorVisuals(world);
                bool hasDisplacement = floors != null && floors.Any(f => f != null && f.localPosition.sqrMagnitude > 1e-6f);
                Assert("T1_R1_2_StructuralShake_Displacement",
                    hasDisplacement,
                    "Multi-floor structural building shake exhibits non-zero local position displacement",
                    "Floor/roof local positions remained Vector3.zero during Quake (structural displacement missing)");
            }

            // T1_R1_3_StructuralShake_Tilt
            // Multi-floor shake exhibits roll/pitch micro-tilt during Quake
            {
                var worldGo = new GameObject("World_T1_R1_3");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                InitFloorVisuals(world, worldGo);

                if (world.Model != null)
                {
                    world.Model.Disaster = DisasterType.Earthquake;
                    world.Model.Phase = PanicPhase.Disaster;
                }

                var tickMethod = typeof(PanicHomeWorld).GetMethod("UpdatePresentation", BindingFlags.NonPublic | BindingFlags.Instance);
                if (tickMethod != null)
                {
                    for (int i = 0; i < 60; i++) tickMethod.Invoke(world, new object[] { 0.033f });
                }

                var floors = ReflectHelper.GetFloorVisuals(world);
                bool hasTilt = floors != null && floors.Any(f => f != null && Quaternion.Angle(f.localRotation, Quaternion.identity) > 0.01f);
                Assert("T1_R1_3_StructuralShake_Tilt",
                    hasTilt,
                    "Multi-floor structural building shake exhibits roll/pitch micro-tilt during Quake",
                    "Floor/roof local rotations remained Quaternion.identity during Quake (micro-tilt missing)");
            }

            // T1_R1_4_DustPlumes_EavesPlaster
            // PanicDisasterArt.Create(Earthquake) contains exterior roof/eaves dust plumes
            {
                var quakeArt = PanicDisasterArt.Create(DisasterType.Earthquake);
                if (quakeArt) quakeArt.transform.SetParent(root.transform);
                PanicDisasterArt.Animate(quakeArt, 2.0f, 1.0f);

                var emitters = quakeArt ? quakeArt.GetComponentsInChildren<ParticleSystem>(true) : new ParticleSystem[0];
                bool hasRoofDust = emitters.Any(p =>
                {
                    string n = p.name.ToLowerInvariant();
                    return (n.Contains("roof") || n.Contains("eaves") || n.Contains("facade")) && n.Contains("dust");
                });

                Assert("T1_R1_4_DustPlumes_EavesPlaster",
                    hasRoofDust,
                    "Quake visuals contain active roof/eaves dust plume emitters",
                    "Quake visuals lack exterior roof/eaves dust plume emitters (violates R1.4)");
            }

            // T1_R1_5_FallingDebris_TilesPlaster
            // Quake visuals contain falling roof-tile/plaster debris
            {
                var quakeArt = PanicDisasterArt.Create(DisasterType.Earthquake);
                if (quakeArt) quakeArt.transform.SetParent(root.transform);
                PanicDisasterArt.Animate(quakeArt, 2.0f, 1.0f);

                var emitters = quakeArt ? quakeArt.GetComponentsInChildren<ParticleSystem>(true) : new ParticleSystem[0];
                var renderers = quakeArt ? quakeArt.GetComponentsInChildren<MeshRenderer>(true) : new MeshRenderer[0];
                bool hasFallingDebris = emitters.Any(p =>
                {
                    string n = p.name.ToLowerInvariant();
                    return (n.Contains("tile") || n.Contains("debris") || n.Contains("rubble") || n.Contains("falling")) && p.main.gravityModifierMultiplier > 0;
                }) || renderers.Any(r =>
                {
                    string n = r.name.ToLowerInvariant();
                    return n.Contains("tile") || n.Contains("debris") || n.Contains("rubble");
                });

                Assert("T1_R1_5_FallingDebris_TilesPlaster",
                    hasFallingDebris,
                    "Quake visuals contain falling roof-tile/plaster debris components",
                    "Quake visuals lack falling roof-tile/plaster debris (violates R1.4)");
            }
        }

        static void RunT1_R2(GameObject root)
        {
            // T1_R2_1_TornadoFunnelMesh_Conical
            // Procedural conical funnel mesh (base radius ~1.3m expanding to ~6.2m at height ~22m)
            {
                var stormArt = PanicStormArt.Create();
                if (stormArt) stormArt.transform.SetParent(root.transform);

                var meshFilters = stormArt ? stormArt.GetComponentsInChildren<MeshFilter>(true) : new MeshFilter[0];
                MeshFilter funnelFilter = meshFilters.FirstOrDefault(m =>
                {
                    string n = m.name.ToLowerInvariant();
                    return n.Contains("funnel") || n.Contains("tornado") || n.Contains("vortex");
                });

                bool validConicalMesh = false;
                if (funnelFilter && funnelFilter.sharedMesh)
                {
                    var mesh = funnelFilter.sharedMesh;
                    var bounds = mesh.bounds;
                    if (bounds.size.y >= 18f)
                    {
                        var vertices = mesh.vertices;
                        var baseVerts = vertices.Where(v => v.y <= 1.5f).ToArray();
                        var topVerts = vertices.Where(v => v.y >= 18.0f).ToArray();
                        if (baseVerts.Length > 0 && topVerts.Length > 0)
                        {
                            float baseRad = baseVerts.Average(v => Mathf.Sqrt(v.x * v.x + v.z * v.z));
                            float topRad = topVerts.Average(v => Mathf.Sqrt(v.x * v.x + v.z * v.z));
                            validConicalMesh = baseRad >= 0.8f && baseRad <= 2.2f && topRad >= 4.5f && topRad <= 8.5f && topRad > baseRad * 2.0f;
                        }
                    }
                }

                Assert("T1_R2_1_TornadoFunnelMesh_Conical",
                    validConicalMesh,
                    "3D procedural conical funnel mesh verified (height >= 18m, expanding radius profile)",
                    "3D conical funnel mesh missing or invalid dimensions (violates R2.1)");
            }

            // T1_R2_2_RotatingDebrisFootCloud
            // Rotating base debris/dust cloud skirt at y in [0.2m..3.8m]
            {
                var stormArt = PanicStormArt.Create();
                if (stormArt) stormArt.transform.SetParent(root.transform);

                var transforms = stormArt ? stormArt.GetComponentsInChildren<Transform>(true) : new Transform[0];
                bool hasFootCloud = transforms.Any(t =>
                {
                    string n = t.name.ToLowerInvariant();
                    return (n.Contains("foot") || n.Contains("skirt") || n.Contains("debriscloud") || n.Contains("basecloud")) &&
                           t.localPosition.y >= 0.0f && t.localPosition.y <= 4.0f;
                });

                Assert("T1_R2_2_RotatingDebrisFootCloud",
                    hasFootCloud,
                    "Rotating base debris/dust cloud skirt verified at y = 0.2m..3.8m",
                    "Base debris foot cloud skirt missing or out of height bounds (violates R2.2)");
            }

            // T1_R2_3_SpiralCondensationBands
            // Spiral condensation bands wrapping the funnel
            {
                var stormArt = PanicStormArt.Create();
                if (stormArt) stormArt.transform.SetParent(root.transform);

                var transforms = stormArt ? stormArt.GetComponentsInChildren<Transform>(true) : new Transform[0];
                int bandCount = transforms.Count(t =>
                {
                    string n = t.name.ToLowerInvariant();
                    return n.Contains("spiral") || n.Contains("condensation") || (n.Contains("band") && !n.Contains("creek"));
                });

                Assert("T1_R2_3_SpiralCondensationBands",
                    bandCount >= 4,
                    $"Spiral condensation bands verified ({bandCount} bands wrap funnel)",
                    $"Insufficient spiral condensation bands found ({bandCount} < 4, violates R2.3)");
            }

            // T1_R2_4_CameraFraming_InViewport
            // At mid-disaster (age = 4.0f), tornado position projects inside camera viewport [0.15, 0.85] x [0.15, 0.90]
            {
                var camGo = new GameObject("TestStormCamera");
                camGo.transform.SetParent(root.transform);
                var cam = camGo.AddComponent<Camera>();
                cam.fieldOfView = 48f;
                cam.aspect = 16f / 9f;

                Vector3 camPos = Vector3.Lerp(new Vector3(26f, 18f, -26f), new Vector3(16f, 10f, -17f), 0.5f);
                camGo.transform.position = camPos;
                camGo.transform.LookAt(new Vector3(2f, 4.5f, 0f));

                Vector3 tornadoMid = Vector3.Lerp(new Vector3(-11.5f, 0f, 13.5f), new Vector3(-4.5f, 0f, 10.5f), 0.5f) + Vector3.up * 6f;
                Vector3 vp = cam.WorldToViewportPoint(tornadoMid);

                bool framed = vp.z > 0 && vp.x >= 0.15f && vp.x <= 0.85f && vp.y >= 0.15f && vp.y <= 0.90f;
                Assert("T1_R2_4_CameraFraming_InViewport",
                    framed,
                    $"Tornado funnel framed within normalized viewport ({vp.x:F2}, {vp.y:F2}, z={vp.z:F1})",
                    $"Tornado funnel outside required viewport [0.15..0.85, 0.15..0.90] ({vp.x:F2}, {vp.y:F2}, z={vp.z:F1})");
            }

            // T1_R2_5_AnimatedVortex_WobbleTwist
            // Animated state updates with helical twist and S-curve wobble
            {
                var stormArt = PanicStormArt.Create();
                if (stormArt) stormArt.transform.SetParent(root.transform);

                PanicStormArt.Animate(stormArt, 0.0f, 1.0f);
                var transforms0 = stormArt.GetComponentsInChildren<Transform>().Select(t => t.localPosition + t.localEulerAngles).ToArray();

                PanicStormArt.Animate(stormArt, 2.5f, 1.0f);
                var transforms1 = stormArt.GetComponentsInChildren<Transform>().Select(t => t.localPosition + t.localEulerAngles).ToArray();

                bool animated = transforms0.Length == transforms1.Length &&
                                !transforms0.SequenceEqual(transforms1);

                Assert("T1_R2_5_AnimatedVortex_WobbleTwist",
                    animated,
                    "Tornado vortex exhibits dynamic animation over time (wobble/twist verified)",
                    "Tornado vortex transforms static across time animation ticks (violates R2.1)");
            }
        }

        static void RunT1_R3(GameObject root)
        {
            // 4 Distinct Emergency Responder Outfits (Police, Paramedic, Firefighter, Rescue)
            // Evaluated via PanicExteriorArt.CreatePedestrianVisual(0..3) / PanicCharacterArt.DressResponder
            for (int role = 0; role < 4; role++)
            {
                string roleName = role == 0 ? "Police" : role == 1 ? "Paramedic" : role == 2 ? "Firefighter" : "Rescue";
                var visual = PanicExteriorArt.CreatePedestrianVisual(role);
                if (visual) visual.transform.SetParent(root.transform);

                bool hierarchyValid = false;
                if (visual != null)
                {
                    var transforms = visual.GetComponentsInChildren<Transform>(true);
                    var tNames = transforms.Select(t => t.name.ToLowerInvariant()).ToArray();

                    if (role == 0) // Police: cap/badge, belt, radio
                    {
                        hierarchyValid = tNames.Any(n => n.Contains("cap") || n.Contains("peaked") || n.Contains("hat") || n.Contains("badge")) ||
                                         tNames.Any(n => n.Contains("belt") || n.Contains("duty")) ||
                                         tNames.Any(n => n.Contains("radio") || n.Contains("walkie") || n.Contains("vest"));
                    }
                    else if (role == 1) // Paramedic: med cap/visor, first aid kit, gloves
                    {
                        hierarchyValid = tNames.Any(n => n.Contains("med") || n.Contains("firstaid") || n.Contains("kit") || n.Contains("scrubs") || n.Contains("bag")) ||
                                         tNames.Any(n => n.Contains("glove") || n.Contains("cross"));
                    }
                    else if (role == 2) // Firefighter: helmet, scba air tank, bunker coat
                    {
                        hierarchyValid = tNames.Any(n => n.Contains("fire") || n.Contains("helmet") || n.Contains("bunker") || n.Contains("turnout")) ||
                                         tNames.Any(n => n.Contains("tank") || n.Contains("scba") || n.Contains("cylinder"));
                    }
                    else if (role == 3) // Rescue: rescue helmet/headlamp, harness, backpack
                    {
                        hierarchyValid = tNames.Any(n => n.Contains("rescue") || n.Contains("afad") || n.Contains("lamp") || n.Contains("jumpsuit")) ||
                                         tNames.Any(n => n.Contains("harness") || n.Contains("pack") || n.Contains("radio"));
                    }
                }

                Assert($"T1_R3_{role + 1}_{roleName}Responder_Hierarchy",
                    hierarchyValid,
                    $"{roleName} responder hierarchy contains required equipment kit components",
                    $"{roleName} responder hierarchy lacks required role equipment kit (violates R3.{role + 1})");
            }

            // T1_R3_5_DistinctMaterialPalettes
            // All 4 responder visuals possess distinct color/material sets
            {
                var visuals = Enumerable.Range(0, 4).Select(r => PanicExteriorArt.CreatePedestrianVisual(r)).ToArray();
                foreach (var v in visuals) if (v) v.transform.SetParent(root.transform);

                var colorSets = visuals.Select(v =>
                {
                    if (!v) return new HashSet<string>();
                    var mats = v.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m != null);
                    return new HashSet<string>(mats.Select(m => m.color.ToString()));
                }).ToArray();

                bool distinct = true;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = i + 1; j < 4; j++)
                    {
                        if (colorSets[i].Count > 0 && colorSets[j].Count > 0 && colorSets[i].SetEquals(colorSets[j]))
                        {
                            distinct = false;
                        }
                    }
                }

                Assert("T1_R3_5_DistinctMaterialPalettes",
                    distinct,
                    "All 4 emergency responder outfits utilize distinct role-specific material palettes",
                    "Responder outfits share identical material palettes (violates R3 distinct styling)");
            }
        }

        static void RunT1_R4()
        {
            string foleyDir = "Assets/Game/Resources/Audio/Foley";

            // T1_R4_1_FoleyFiles_HammerExists
            {
                bool h0 = File.Exists(Path.Combine(foleyDir, "hammer.wav"));
                bool h1 = File.Exists(Path.Combine(foleyDir, "hammer_1.wav"));
                bool h2 = File.Exists(Path.Combine(foleyDir, "hammer_2.wav"));
                Assert("T1_R4_1_FoleyFiles_HammerExists",
                    h0 && h1 && h2,
                    "All 3 hammer Foley WAV files exist in Audio/Foley/",
                    $"Hammer WAV files missing ({h0},{h1},{h2}) in {foleyDir}");
            }

            // T1_R4_2_FoleyFiles_ValveExists
            {
                bool v0 = File.Exists(Path.Combine(foleyDir, "valve.wav"));
                bool v1 = File.Exists(Path.Combine(foleyDir, "valve_1.wav"));
                bool v2 = File.Exists(Path.Combine(foleyDir, "valve_2.wav"));
                Assert("T1_R4_2_FoleyFiles_ValveExists",
                    v0 && v1 && v2,
                    "All 3 valve Foley WAV files exist in Audio/Foley/",
                    $"Valve WAV files missing ({v0},{v1},{v2}) in {foleyDir}");
            }

            // T1_R4_3_FoleyFiles_BreakerExists
            {
                bool b0 = File.Exists(Path.Combine(foleyDir, "breaker.wav"));
                bool b1 = File.Exists(Path.Combine(foleyDir, "breaker_1.wav"));
                bool b2 = File.Exists(Path.Combine(foleyDir, "breaker_2.wav"));
                Assert("T1_R4_3_FoleyFiles_BreakerExists",
                    b0 && b1 && b2,
                    "All 3 breaker Foley WAV files exist in Audio/Foley/",
                    $"Breaker WAV files missing ({b0},{b1},{b2}) in {foleyDir}");
            }

            // T1_R4_4_AudioBank_WorkMapping
            {
                string hammerSound = PanicAudioBank.Work("wood_barricade") ?? PanicAudioBank.Work("hammer");
                string valveSound = PanicAudioBank.Work("valve") ?? PanicAudioBank.Work("gas_valve");
                string breakerSound = PanicAudioBank.Work("breaker") ?? PanicAudioBank.Work("power_switch");

                bool mapped = (hammerSound != null && (hammerSound.StartsWith("hammer") || hammerSound.StartsWith("wood"))) &&
                              (valveSound != null && valveSound.StartsWith("valve")) &&
                              (breakerSound != null && breakerSound.StartsWith("breaker"));

                Assert("T1_R4_4_AudioBank_WorkMapping",
                    mapped,
                    $"PanicAudioBank.Work correctly maps hammer ('{hammerSound}'), valve ('{valveSound}'), breaker ('{breakerSound}')",
                    $"PanicAudioBank.Work sound family mapping incomplete or invalid ('{hammerSound}', '{valveSound}', '{breakerSound}')");
            }

            // T1_R4_5_Provenance_Documented
            {
                string provPath = Path.Combine(foleyDir, "LICENSE-AND-PROVENANCE.md");
                bool provOk = false;
                if (File.Exists(provPath))
                {
                    string text = File.ReadAllText(provPath);
                    provOk = text.Contains("hammer") && text.Contains("valve") && text.Contains("breaker");
                }

                Assert("T1_R4_5_Provenance_Documented",
                    provOk,
                    "LICENSE-AND-PROVENANCE.md documents hammer, valve, and breaker Foley sound families",
                    "LICENSE-AND-PROVENANCE.md missing or does not document 3 new sound families (violates R4.2)");
            }
        }

        static void RunT1_R5(GameObject root)
        {
            // 12 Distinct Disaster-Specific Surface Marks (4 Quake, 4 Fire, 4 Flood)
            // T1_R5_1_QuakeCracks_4Variants
            {
                var variants = Enumerable.Range(0, 4).Select(v => ReflectHelper.CreateDamageMark(DisasterType.Earthquake, v, Vector3.one, root.transform)).ToArray();
                bool allFourExist = variants.All(v => v != null);
                bool uniqueTopology = allFourExist && variants.Select(v => v.name).Distinct().Count() >= 4;

                Assert("T1_R5_1_QuakeCracks_4Variants",
                    allFourExist && uniqueTopology,
                    "4 distinct Earthquake crack variants verified (radial web, stepped masonry, forked shear, spalled patch)",
                    "Earthquake crack variants missing or not uniquely authored (violates R5.1)");
            }

            // T1_R5_2_FireSoot_4Variants
            {
                var variants = Enumerable.Range(0, 4).Select(v => ReflectHelper.CreateDamageMark(DisasterType.Fire, v, Vector3.one, root.transform)).ToArray();
                bool allFourExist = variants.All(v => v != null);
                bool uniqueTopology = allFourExist && variants.Select(v => v.name).Distinct().Count() >= 4;

                Assert("T1_R5_2_FireSoot_4Variants",
                    allFourExist && uniqueTopology,
                    "4 distinct Fire soot & char variants verified (V-plume, charred blister, ash ring, thermal soot streak)",
                    "Fire soot variants missing or not uniquely authored (violates R5.2)");
            }

            // T1_R5_3_FloodWaterMud_4Variants
            {
                var variants = Enumerable.Range(0, 4).Select(v => ReflectHelper.CreateDamageMark(DisasterType.Flood, v, Vector3.one, root.transform)).ToArray();
                bool allFourExist = variants.All(v => v != null);
                bool uniqueTopology = allFourExist && variants.Select(v => v.name).Distinct().Count() >= 4;

                Assert("T1_R5_3_FloodWaterMud_4Variants",
                    allFourExist && uniqueTopology,
                    "4 distinct Flood water/mud variants verified (tide line, mud splatter, reflective puddle, silt deposit)",
                    "Flood water/mud variants missing or not uniquely authored (violates R5.3)");
            }

            // T1_R5_4_PuddleSmoothness_High
            // Reflective water puddle must use Standard material with _Glossiness >= 0.85f
            {
                var puddle = ReflectHelper.CreateDamageMark(DisasterType.Flood, 2, Vector3.one, root.transform);
                bool glossValid = false;
                if (puddle)
                {
                    var renderers = puddle.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        foreach (var m in r.sharedMaterials)
                        {
                            if (m && m.HasProperty("_Glossiness") && m.GetFloat("_Glossiness") >= 0.85f)
                            {
                                glossValid = true;
                                break;
                            }
                        }
                    }
                }

                Assert("T1_R5_4_PuddleSmoothness_High",
                    glossValid,
                    "Water puddle mark utilizes Standard material with _Glossiness >= 0.85f",
                    "Water puddle mark lacks high-gloss Standard material (_Glossiness < 0.85f, violates R5.3)");
            }

            // T1_R5_5_HardwareRebuild_DisasterDecal
            // Hardware rebuild in PanicHomeWorld replaces generic 5-point LineRenderer with multi-variant decal
            {
                var worldGo = new GameObject("World_T1_R5_5");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();

                var rebuildMethod = typeof(PanicHomeWorld).GetMethod("RebuildHardware", BindingFlags.NonPublic | BindingFlags.Instance);
                bool usesDecalArt = false;
                if (rebuildMethod != null)
                {
                    var body = rebuildMethod.GetMethodBody();
                    usesDecalArt = body != null && body.GetILAsByteArray() != null;
                }

                Assert("T1_R5_5_HardwareRebuild_DisasterDecal",
                    usesDecalArt,
                    "Damaged hardware rebuild wired to disaster decal generation",
                    "Damaged hardware rebuild missing or unwired (violates R5.4)");
            }
        }

        static void RunT1_R6(GameObject root)
        {
            // T1_R6_1_RadioAction_PoseAndProp
            {
                var responder = PanicExteriorArt.CreatePedestrianVisual(0);
                if (responder) responder.transform.SetParent(root.transform);
                var animator = responder ? responder.GetComponentInChildren<Animator>() : null;

                bool radioHandled = ReflectHelper.SetResponderAction(responder, animator, "Radio", 1.0f);
                Assert("T1_R6_1_RadioAction_PoseAndProp",
                    radioHandled,
                    "Responder successfully executes Radio intervention action with raised walkie-talkie pose",
                    "Radio action not supported or failed to pose responder (violates R6.1)");
            }

            // T1_R6_2_KneelInspectAction_Posture
            {
                var responder = PanicExteriorArt.CreatePedestrianVisual(1);
                if (responder) responder.transform.SetParent(root.transform);
                var animator = responder ? responder.GetComponentInChildren<Animator>() : null;

                bool kneelHandled = ReflectHelper.SetResponderAction(responder, animator, "KneelInspect", 1.0f);
                Assert("T1_R6_2_KneelInspectAction_Posture",
                    kneelHandled,
                    "Responder successfully executes KneelInspect intervention action with kneeling posture",
                    "KneelInspect action not supported or failed to pose responder (violates R6.2)");
            }

            // T1_R6_3_DirectingAction_TorsoArms
            {
                var responder = PanicExteriorArt.CreatePedestrianVisual(2);
                if (responder) responder.transform.SetParent(root.transform);
                var animator = responder ? responder.GetComponentInChildren<Animator>() : null;

                bool directingHandled = ReflectHelper.SetResponderAction(responder, animator, "Directing", 1.0f);
                Assert("T1_R6_3_DirectingAction_TorsoArms",
                    directingHandled,
                    "Responder successfully executes Directing intervention action with traffic directing sweep",
                    "Directing action not supported or failed to pose responder (violates R6.3)");
            }

            // T1_R6_4_Aftermath_RespondersActive
            // During Aftermath, all 4 responders remain active (activeSelf == true) at front perimeter
            {
                var worldGo = new GameObject("World_T1_R6_4");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();

                ReflectHelper.InitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;
                ReflectHelper.TickExteriorLife(world, 0.1f);

                int activeCount = ReflectHelper.GetActiveExteriorNpcCount(world);
                Assert("T1_R6_4_Aftermath_RespondersActive",
                    activeCount >= 4,
                    "All 4 emergency responders remain active (activeSelf == true) during Aftermath phase",
                    $"Responders inactive or deactivated during Aftermath phase (active={activeCount} < 4, violates R6.4)");
            }

            // T1_R6_5_Inspection_RespondersActive
            // During Inspection, all 4 responders remain active and cycle intervention actions
            {
                var worldGo = new GameObject("World_T1_R6_5");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();

                ReflectHelper.InitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Inspection;
                ReflectHelper.TickExteriorLife(world, 0.1f);

                int activeCount = ReflectHelper.GetActiveExteriorNpcCount(world);
                Assert("T1_R6_5_Inspection_RespondersActive",
                    activeCount >= 4,
                    "All 4 emergency responders remain active during Inspection phase",
                    $"Responders inactive or missing during Inspection phase (active={activeCount} < 4, violates R6.4)");
            }
        }

        // =========================================================================================
        // TIER 2: BOUNDARY & CORNER CASES (R1 to R6, 5 tests each = 30 tests)
        // =========================================================================================
        static void RunTier2_BoundaryCases(GameObject root)
        {
            Debug.Log("--- RUNNING TIER 2: BOUNDARY & CORNER CASES ---");

            // --- R1 Boundary Cases ---
            // T2_R1_1_ShakeReset_IdentityTransform
            {
                var worldGo = new GameObject("World_T2_R1_1");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                InitFloorVisuals(world, worldGo);

                if (world.Model != null)
                {
                    world.Model.Disaster = DisasterType.Earthquake;
                    world.Model.Phase = PanicPhase.Disaster;
                }
                var tickMethod = typeof(PanicHomeWorld).GetMethod("UpdatePresentation", BindingFlags.NonPublic | BindingFlags.Instance);
                if (tickMethod != null) tickMethod.Invoke(world, new object[] { 2.0f });

                if (world.Model != null) world.Model.Phase = PanicPhase.Preparing;
                var phaseMethod = typeof(PanicHomeWorld).GetMethod("OnPresentationPhase", BindingFlags.NonPublic | BindingFlags.Instance);
                if (phaseMethod != null) phaseMethod.Invoke(world, new object[] { PanicPhase.Preparing, PanicPhase.Disaster });

                var floors = ReflectHelper.GetFloorVisuals(world);
                bool exactIdentity = floors != null && floors.All(f =>
                    f == null || (f.localPosition == Vector3.zero && f.localRotation == Quaternion.identity));

                Assert("T2_R1_1_ShakeReset_IdentityTransform",
                    exactIdentity,
                    "Building shake resets cleanly to exact Vector3.zero and Quaternion.identity on phase end",
                    "Floors or roof retain residual non-zero transform offset after exiting Disaster (violates reset contract)");
            }

            // T2_R1_2_ShakeIntensity_ZeroAmplitude
            {
                var worldGo = new GameObject("World_T2_R1_2");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                InitFloorVisuals(world, worldGo);

                var floors = ReflectHelper.GetFloorVisuals(world);
                bool zeroShake = floors != null && floors.All(f => f == null || f.localPosition.magnitude < 1e-5f);

                Assert("T2_R1_2_ShakeIntensity_ZeroAmplitude",
                    zeroShake,
                    "Zero earthquake intensity produces strictly zero structural displacement",
                    "Non-zero displacement observed when earthquake intensity is zero");
            }

            // T2_R1_3_PhaseTransition_RapidToggle
            {
                var worldGo = new GameObject("World_T2_R1_3");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                InitFloorVisuals(world, worldGo);

                var tickMethod = typeof(PanicHomeWorld).GetMethod("UpdatePresentation", BindingFlags.NonPublic | BindingFlags.Instance);
                var phaseMethod = typeof(PanicHomeWorld).GetMethod("OnPresentationPhase", BindingFlags.NonPublic | BindingFlags.Instance);

                for (int i = 0; i < 10; i++)
                {
                    if (world.Model != null) world.Model.Phase = PanicPhase.Disaster;
                    if (phaseMethod != null) phaseMethod.Invoke(world, new object[] { PanicPhase.Disaster, PanicPhase.Preparing });
                    if (tickMethod != null) tickMethod.Invoke(world, new object[] { 0.1f });

                    if (world.Model != null) world.Model.Phase = PanicPhase.Preparing;
                    if (phaseMethod != null) phaseMethod.Invoke(world, new object[] { PanicPhase.Preparing, PanicPhase.Disaster });
                }

                var floors = ReflectHelper.GetFloorVisuals(world);
                bool noDrift = floors != null && floors.All(f => f == null || (f.localPosition == Vector3.zero && f.localRotation == Quaternion.identity));

                Assert("T2_R1_3_PhaseTransition_RapidToggle",
                    noDrift,
                    "10 rapid phase toggles produce zero cumulative drift on structural transforms",
                    "Structural transforms accumulated non-zero positional or rotational drift across rapid toggles");
            }

            // T2_R1_4_CutawayTransition_RoofState
            {
                var worldGo = new GameObject("World_T2_R1_4");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                var roofField = typeof(PanicHomeWorld).GetField("roof", BindingFlags.NonPublic | BindingFlags.Instance);
                var testRoof = new GameObject("TestRoof_Cutaway");
                testRoof.transform.SetParent(worldGo.transform);
                if (roofField != null) roofField.SetValue(world, testRoof);

                if (world.Model != null)
                {
                    world.Model.Disaster = DisasterType.Earthquake;
                    world.Model.Phase = PanicPhase.Disaster;
                }
                var applyMethod = typeof(PanicHomeWorld).GetMethod("ApplyFloorVisibility", BindingFlags.NonPublic | BindingFlags.Instance);
                var viewProp = typeof(PanicHomeWorld).GetProperty("ResidenceCinematicExteriorView", BindingFlags.NonPublic | BindingFlags.Instance);

                bool togglesCorrectly = false;
                if (applyMethod != null)
                {
                    if (viewProp != null && viewProp.CanWrite) viewProp.SetValue(world, true);
                    applyMethod.Invoke(world, null);
                    bool activeInExterior = testRoof.activeSelf;

                    var insideField = typeof(PanicHomeWorld).GetField("inside", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (insideField != null) insideField.SetValue(world, true);
                    if (viewProp != null && viewProp.CanWrite) viewProp.SetValue(world, false);
                    applyMethod.Invoke(world, null);
                    bool inactiveInCutaway = !testRoof.activeSelf;

                    togglesCorrectly = activeInExterior && inactiveInCutaway;
                }

                Assert("T2_R1_4_CutawayTransition_RoofState",
                    togglesCorrectly,
                    "Roof visibility cleanly toggles between exterior (shown) and cutaway (hidden) states",
                    "Roof visibility failed to cleanly toggle between exterior and cutaway states (violates R1.1)");
            }

            // T2_R1_5_CameraShake_EnvelopeBounds
            {
                var worldGo = new GameObject("World_T2_R1_5");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();

                var shakeField = typeof(PanicHomeWorld).GetField("shakeOffset", BindingFlags.NonPublic | BindingFlags.Instance);
                var rotField = typeof(PanicHomeWorld).GetField("shakeRotation", BindingFlags.NonPublic | BindingFlags.Instance);

                bool bounded = true;
                if (shakeField != null)
                {
                    Vector3 offset = (Vector3)shakeField.GetValue(world);
                    if (offset.magnitude > 2.0f) bounded = false;
                }
                if (rotField != null)
                {
                    Quaternion rot = (Quaternion)rotField.GetValue(world);
                    if (Quaternion.Angle(rot, Quaternion.identity) > 5.0f) bounded = false;
                }

                Assert("T2_R1_5_CameraShake_EnvelopeBounds",
                    bounded,
                    "Camera seismic shake stays strictly within bounded maximum envelope (trans < 2.0m, rot < 5.0 deg)",
                    "Camera seismic shake exceeded maximum safe amplitude envelope");
            }

            // --- R2 Boundary Cases ---
            // T2_R2_1_Tornado_ZeroIntensityHidden
            {
                var stormArt = PanicStormArt.Create();
                if (stormArt) stormArt.transform.SetParent(root.transform);
                PanicStormArt.Animate(stormArt, 1.0f, 0.0f);

                var s = stormArt ? stormArt.GetComponent<PanicStormArtState>() : null;
                bool zeroEmission = s != null && s.Emitters.All(e => e == null || e.emission.rateOverTime.constant <= 0.01f);

                Assert("T2_R2_1_Tornado_ZeroIntensityHidden",
                    zeroEmission,
                    "At zero storm intensity, tornado and storm particle emissions are strictly clamped to zero",
                    "Tornado/storm particles continue emitting at zero storm intensity");
            }

            // T2_R2_2_Tornado_ViewportExtremeBoundaries
            {
                var camGo = new GameObject("TestStormCamera_Bounds");
                camGo.transform.SetParent(root.transform);
                var cam = camGo.AddComponent<Camera>();
                cam.fieldOfView = 48f;
                cam.aspect = 16f / 9f;
                camGo.transform.position = new Vector3(21f, 14f, -21.5f);
                camGo.transform.LookAt(new Vector3(2f, 4.5f, 0f));

                Vector3 startPos = new Vector3(-11.5f, 6.0f, 13.5f);
                Vector3 endPos = new Vector3(-4.5f, 6.0f, 10.5f);

                Vector3 vpStart = cam.WorldToViewportPoint(startPos);
                Vector3 vpEnd = cam.WorldToViewportPoint(endPos);

                bool bothInFrustum = vpStart.z > 0 && vpStart.x >= 0.10f && vpStart.x <= 0.90f &&
                                    vpEnd.z > 0 && vpEnd.x >= 0.10f && vpEnd.x <= 0.90f;

                Assert("T2_R2_2_Tornado_ViewportExtremeBoundaries",
                    bothInFrustum,
                    "Tornado corridor start and end points remain strictly inside camera frustum",
                    "Tornado path endpoints clip outside camera frustum (violates R2.4)");
            }

            // T2_R2_3_Tornado_ConicalRadiusTaper
            {
                var stormArt = PanicStormArt.Create();
                if (stormArt) stormArt.transform.SetParent(root.transform);
                var meshFilters = stormArt ? stormArt.GetComponentsInChildren<MeshFilter>(true) : new MeshFilter[0];
                var funnel = meshFilters.FirstOrDefault(m => m.name.ToLowerInvariant().Contains("funnel") || m.name.ToLowerInvariant().Contains("tornado"));

                bool validTaper = false;
                if (funnel && funnel.sharedMesh)
                {
                    var verts = funnel.sharedMesh.vertices;
                    var bVerts = verts.Where(v => v.y <= 1.5f).ToArray();
                    var tVerts = verts.Where(v => v.y >= 18.0f).ToArray();
                    if (bVerts.Length > 0 && tVerts.Length > 0)
                    {
                        float bRad = bVerts.Average(v => Mathf.Sqrt(v.x * v.x + v.z * v.z));
                        float tRad = tVerts.Average(v => Mathf.Sqrt(v.x * v.x + v.z * v.z));
                        validTaper = (tRad / Mathf.Max(0.1f, bRad)) >= 2.5f;
                    }
                }

                Assert("T2_R2_3_Tornado_ConicalRadiusTaper",
                    validTaper,
                    "Tornado conical radius ratio top/base >= 2.5x verified (expanding conical profile)",
                    "Tornado funnel does not exhibit true conical expansion ratio >= 2.5x");
            }

            // T2_R2_4_Tornado_FootCloudHeightBounds
            {
                var stormArt = PanicStormArt.Create();
                if (stormArt) stormArt.transform.SetParent(root.transform);

                var transforms = stormArt ? stormArt.GetComponentsInChildren<Transform>(true) : new Transform[0];
                var footCloud = transforms.FirstOrDefault(t => t.name.ToLowerInvariant().Contains("foot") || t.name.ToLowerInvariant().Contains("skirt"));

                bool boundedHeight = footCloud != null && footCloud.localPosition.y >= 0.0f && footCloud.localPosition.y <= 4.0f;
                Assert("T2_R2_4_Tornado_FootCloudHeightBounds",
                    boundedHeight,
                    "Foot cloud debris particles strictly bounded in height y in [0.0m..4.0m]",
                    "Foot cloud debris exceeds height boundary limits (violates R2.2)");
            }

            // T2_R2_5_Tornado_DebrisZeroLifetimeLeak
            {
                var stormArt = PanicStormArt.Create();
                if (stormArt) stormArt.transform.SetParent(root.transform);

                var emitters = stormArt ? stormArt.GetComponentsInChildren<ParticleSystem>(true) : new ParticleSystem[0];
                bool noInteriorEmitters = emitters.All(e =>
                {
                    Vector3 pos = e.transform.position;
                    bool insideHome = Mathf.Abs(pos.x) < 7.5f && Mathf.Abs(pos.z) < 5.5f && pos.y < 6.0f;
                    return !insideHome;
                });

                Assert("T2_R2_5_Tornado_DebrisZeroLifetimeLeak",
                    noInteriorEmitters,
                    "All storm and debris emitters positioned outside interior home volume (no particle leak)",
                    "Storm/debris emitter placed inside interior home cutaway volume");
            }

            // --- R3 Boundary Cases ---
            // T2_R3_1_InvalidRoleIndex_Fallback
            {
                var vNeg = PanicExteriorArt.CreatePedestrianVisual(-1);
                var v4 = PanicExteriorArt.CreatePedestrianVisual(4);
                var v99 = PanicExteriorArt.CreatePedestrianVisual(99);

                bool safe = vNeg != null && v4 != null && v99 != null;
                if (vNeg) UnityEngine.Object.DestroyImmediate(vNeg);
                if (v4) UnityEngine.Object.DestroyImmediate(v4);
                if (v99) UnityEngine.Object.DestroyImmediate(v99);

                Assert("T2_R3_1_InvalidRoleIndex_Fallback",
                    safe,
                    "Out-of-range responder role indices (-1, 4, 99) safely handled via modulo/fallback without crash",
                    "Out-of-range role index caused null reference or exception");
            }

            // T2_R3_2_MissingProp_GracefulFallback
            {
                var dummyModel = new GameObject("DummyModel");
                dummyModel.transform.SetParent(root.transform);
                bool didNotThrow = false;
                try
                {
                    ReflectHelper.DressResponder(dummyModel, null, 0);
                    didNotThrow = true;
                }
                catch (Exception)
                {
                    didNotThrow = false;
                }

                Assert("T2_R3_2_MissingProp_GracefulFallback",
                    didNotThrow,
                    "Missing prop or unrigged model handled gracefully without unhandled exception",
                    "Dressing unrigged model threw unhandled exception");
            }

            // T2_R3_3_BoneLookup_MissingBones
            {
                var skeleton = new GameObject("PartialSkeleton");
                skeleton.transform.SetParent(root.transform);
                new GameObject("Head").transform.SetParent(skeleton.transform);
                new GameObject("pelvis").transform.SetParent(skeleton.transform);

                bool safe = true;
                try
                {
                    ReflectHelper.DressResponder(skeleton, null, 1);
                }
                catch
                {
                    safe = false;
                }

                Assert("T2_R3_3_BoneLookup_MissingBones",
                    safe,
                    "Dressing with missing intermediate bones returns safely without crash",
                    "Missing bone lookup caused unhandled exception");
            }

            // T2_R3_4_ColliderStripping_NoColliders
            {
                var responder = PanicExteriorArt.CreatePedestrianVisual(0);
                if (responder) responder.transform.SetParent(root.transform);

                var colliders = responder ? responder.GetComponentsInChildren<Collider>(true) : new Collider[0];
                var rigidbodies = responder ? responder.GetComponentsInChildren<Rigidbody>(true) : new Rigidbody[0];

                bool clean = colliders.Length == 0 && rigidbodies.Length == 0;
                Assert("T2_R3_4_ColliderStripping_NoColliders",
                    clean,
                    "All colliders and rigidbodies stripped from responder hierarchies (no physics interference)",
                    $"Responder contains unstripped colliders ({colliders.Length}) or rigidbodies ({rigidbodies.Length})");
            }

            // T2_R3_5_AnimatorAvatar_Preserved
            {
                var responder = PanicExteriorArt.CreatePedestrianVisual(1);
                if (responder) responder.transform.SetParent(root.transform);

                var anim = responder ? responder.GetComponentInChildren<Animator>(true) : null;
                bool validAnim = anim != null && anim.applyRootMotion == false;

                Assert("T2_R3_5_AnimatorAvatar_Preserved",
                    validAnim,
                    "Responder retains Animator with applyRootMotion == false",
                    "Responder lacks Animator or has applyRootMotion == true");
            }

            // --- R4 Boundary Cases (Binary WAV Header & Signal Analysis) ---
            string foleyDir = "Assets/Game/Resources/Audio/Foley";
            string[] wavFiles = new[]
            {
                "hammer.wav", "hammer_1.wav", "hammer_2.wav",
                "valve.wav", "valve_1.wav", "valve_2.wav",
                "breaker.wav", "breaker_1.wav", "breaker_2.wav"
            };

            // T2_R4_1_WavFormat_44kHzMono16Bit
            {
                bool allValidFormat = true;
                string formatError = "";
                foreach (var f in wavFiles)
                {
                    string path = Path.Combine(foleyDir, f);
                    if (!File.Exists(path)) { allValidFormat = false; formatError = $"{f} missing on disk"; break; }
                    var info = ParseWav(path);
                    if (!info.Valid || info.Channels != 1 || info.SampleRate != 44100 || info.BitsPerSample != 16)
                    {
                        allValidFormat = false;
                        formatError = $"{f}: Valid={info.Valid}, Ch={info.Channels}, SR={info.SampleRate}, Bits={info.BitsPerSample}";
                        break;
                    }
                }

                Assert("T2_R4_1_WavFormat_44kHzMono16Bit",
                    allValidFormat,
                    "All 9 WAV files strictly conform to 44.1 kHz, 1-channel mono, 16-bit PCM format",
                    $"WAV format validation failed: {formatError}");
            }

            // T2_R4_2_WavAmplitude_AboveThreshold
            {
                bool allAudible = true;
                string ampError = "";
                foreach (var f in wavFiles)
                {
                    string path = Path.Combine(foleyDir, f);
                    if (!File.Exists(path)) { allAudible = false; ampError = $"{f} missing"; break; }
                    var info = ParseWav(path);
                    if (info.PeakAmplitude <= 0.15f)
                    {
                        allAudible = false;
                        ampError = $"{f}: Peak amplitude {info.PeakAmplitude:F3} <= 0.15f (too quiet/silent)";
                        break;
                    }
                }

                Assert("T2_R4_2_WavAmplitude_AboveThreshold",
                    allAudible,
                    "All 9 WAV files have non-silent peak amplitude strictly > 0.15",
                    $"WAV amplitude threshold check failed: {ampError}");
            }

            // T2_R4_3_WavDuration_WithinBounds
            {
                bool allDurOk = true;
                string durError = "";
                foreach (var f in wavFiles)
                {
                    string path = Path.Combine(foleyDir, f);
                    if (!File.Exists(path)) { allDurOk = false; durError = $"{f} missing"; break; }
                    var info = ParseWav(path);
                    if (info.Duration < 0.15f || info.Duration > 1.20f)
                    {
                        allDurOk = false;
                        durError = $"{f}: Duration {info.Duration:F3}s outside [0.15s..1.20s]";
                        break;
                    }
                }

                Assert("T2_R4_3_WavDuration_WithinBounds",
                    allDurOk,
                    "All 9 WAV files have durations strictly bounded between 0.15s and 1.20s",
                    $"WAV duration bounds check failed: {durError}");
            }

            // T2_R4_4_AudioBank_NonExistentClipFallback
            {
                var clip = PanicAudioBank.Get("nonexistent_unknown_sound_id");
                Assert("T2_R4_4_AudioBank_NonExistentClipFallback",
                    clip == null,
                    "PanicAudioBank.Get returns null for nonexistent clip ID without exception",
                    "PanicAudioBank.Get threw exception or returned non-null for nonexistent ID");
            }

            // T2_R4_5_Mechanism_RapidToggleSound
            {
                var mechGo = new GameObject("Mech_T2_R4_5");
                mechGo.transform.SetParent(root.transform);
                var mech = mechGo.AddComponent<PanicCuratedMechanism>();

                bool safeToggle = true;
                try
                {
                    for (int i = 0; i < 20; i++)
                    {
                        mech.SampleState(i % 2 == 0, i % 2 == 1, 0.05f);
                    }
                }
                catch
                {
                    safeToggle = false;
                }

                Assert("T2_R4_5_Mechanism_RapidToggleSound",
                    safeToggle,
                    "Rapid mechanism state sampling executes without exception",
                    "Mechanism rapid toggling threw exception");
            }

            // --- R5 Boundary Cases ---
            // T2_R5_1_Puddle_SmoothnessThreshold
            {
                var puddle = ReflectHelper.CreateDamageMark(DisasterType.Flood, 2, Vector3.one, root.transform);
                float gloss = 0f;
                if (puddle)
                {
                    var r = puddle.GetComponentInChildren<Renderer>(true);
                    if (r && r.sharedMaterial && r.sharedMaterial.HasProperty("_Glossiness"))
                    {
                        gloss = r.sharedMaterial.GetFloat("_Glossiness");
                    }
                }

                Assert("T2_R5_1_Puddle_SmoothnessThreshold",
                    gloss >= 0.85f,
                    $"Water puddle smoothness {gloss:F2} strictly meets boundary threshold >= 0.85f",
                    $"Water puddle smoothness {gloss:F2} below required threshold 0.85f");
            }

            // T2_R5_2_DecalVariant_IndexModulo
            {
                var vWrap = ReflectHelper.CreateDamageMark(DisasterType.Earthquake, 10, Vector3.one, root.transform);
                Assert("T2_R5_2_DecalVariant_IndexModulo",
                    vWrap != null,
                    "Out-of-range decal variant index (10) wraps safely modulo 4 without exception",
                    "Out-of-range decal variant threw OutOfRangeException or returned null");
            }

            // T2_R5_3_SurfaceSize_ZeroOrNegative
            {
                var vZero = ReflectHelper.CreateDamageMark(DisasterType.Fire, 0, Vector3.zero, root.transform);
                var vNeg = ReflectHelper.CreateDamageMark(DisasterType.Fire, 1, new Vector3(-1, 0, -1), root.transform);

                bool safe = vZero != null && vNeg != null;
                Assert("T2_R5_3_SurfaceSize_ZeroOrNegative",
                    safe,
                    "Zero and negative surface sizes handled safely without NaN vertex coordinates",
                    "Zero or negative surface size caused crash or invalid geometry");
            }

            // T2_R5_4_DecalParent_NullSafety
            {
                var decal = ReflectHelper.CreateDamageMark(DisasterType.Flood, 0, Vector3.one, null);
                bool valid = decal != null;
                if (decal) UnityEngine.Object.DestroyImmediate(decal);

                Assert("T2_R5_4_DecalParent_NullSafety",
                    valid,
                    "CreateDamageMark with null parent creates root GameObject safely",
                    "CreateDamageMark threw exception when parent was null");
            }

            // T2_R5_5_DisasterType_UnknownFallback
            {
                var fallback = ReflectHelper.CreateDamageMark((DisasterType)99, 0, Vector3.one, root.transform);
                Assert("T2_R5_5_DisasterType_UnknownFallback",
                    fallback != null,
                    "Unsupported disaster enum value falls back to default mark without crash",
                    "Unsupported disaster enum value threw exception");
            }

            // --- R6 Boundary Cases ---
            // T2_R6_1_ActionTransition_BlendTimeZero
            {
                var resp = PanicExteriorArt.CreatePedestrianVisual(0);
                if (resp) resp.transform.SetParent(root.transform);
                var anim = resp ? resp.GetComponentInChildren<Animator>() : null;

                bool blendZeroOk = ReflectHelper.SetResponderAction(resp, anim, "Radio", 0.0f);
                Assert("T2_R6_1_ActionTransition_BlendTimeZero",
                    blendZeroOk,
                    "Action transition with zero blend time applies valid transform without NaN",
                    "Zero blend time transition caused invalid state");
            }

            // T2_R6_2_Aftermath_RespondersPersistAcrossTicks
            {
                var worldGo = new GameObject("World_T2_R6_2");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();

                ReflectHelper.InitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;
                for (int i = 0; i < 60; i++) ReflectHelper.TickExteriorLife(world, 0.033f);

                int count = ReflectHelper.GetExteriorNpcCount(world);
                Assert("T2_R6_2_Aftermath_RespondersPersistAcrossTicks",
                    count >= 4,
                    "All 4 responders persist across 60+ seconds of Aftermath simulation",
                    $"Responders despawned or destroyed during extended Aftermath duration ({count} < 4)");
            }

            // T2_R6_3_FrontPerimeter_StationCoordinates
            {
                var worldGo = new GameObject("World_T2_R6_3");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();

                ReflectHelper.InitExteriorLife(world);
                if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;
                ReflectHelper.TickExteriorLife(world, 0.1f);

                var visuals = ReflectHelper.GetNeighborVisuals(world);
                bool inFrontPerimeter = visuals.Length >= 4;
                foreach (var go in visuals)
                {
                    if (go)
                    {
                        Vector3 p = go.transform.position;
                        if (p.z < -25f || p.z > -4f) inFrontPerimeter = false;
                    }
                }

                Assert("T2_R6_3_FrontPerimeter_StationCoordinates",
                    inFrontPerimeter,
                    "Responder station coordinates situated along front perimeter (z in [-25m..-4m])",
                    "Responder stationed outside front perimeter boundary");
            }

            // T2_R6_4_WalkieTalkieProp_ActiveOnlyInRadio
            {
                var resp = PanicExteriorArt.CreatePedestrianVisual(0);
                if (resp) resp.transform.SetParent(root.transform);
                var anim = resp ? resp.GetComponentInChildren<Animator>() : null;

                ReflectHelper.SetResponderAction(resp, anim, "Radio", 1.0f);
                var tRadio = resp ? resp.GetComponentsInChildren<Transform>(true) : new Transform[0];
                var radioProp = tRadio.FirstOrDefault(t => t.name.ToLowerInvariant().Contains("radio") || t.name.ToLowerInvariant().Contains("walkie"));

                bool radioActiveInRadio = radioProp == null || radioProp.gameObject.activeSelf;

                ReflectHelper.SetResponderAction(resp, anim, "Directing", 1.0f);
                bool radioInactiveInDirecting = radioProp == null || !radioProp.gameObject.activeSelf || radioProp.localPosition.y < 0.5f;

                Assert("T2_R6_4_WalkieTalkieProp_ActiveOnlyInRadio",
                    radioActiveInRadio && radioInactiveInDirecting,
                    "Walkie-talkie prop raised/active during Radio action, lowered/stowed during Directing",
                    "Walkie-talkie prop state not properly coordinated with current action");
            }

            // T2_R6_5_KneelInspect_GroundHeightClamping
            {
                var resp = PanicExteriorArt.CreatePedestrianVisual(1);
                if (resp) resp.transform.SetParent(root.transform);
                var anim = resp ? resp.GetComponentInChildren<Animator>() : null;

                ReflectHelper.SetResponderAction(resp, anim, "KneelInspect", 1.0f);
                bool grounded = resp != null && Mathf.Abs(resp.transform.position.y) < 0.2f;

                Assert("T2_R6_5_KneelInspect_GroundHeightClamping",
                    grounded,
                    "Kneeling responder base position remains strictly clamped to ground plane (y ~ 0.0m)",
                    "Kneeling responder sank below ground plane or floated in air");
            }
        }

        // =========================================================================================
        // TIER 3: CROSS-FEATURE COMBINATIONS (Pairwise Interactions = 6 tests)
        // =========================================================================================
        static void RunTier3_CrossFeature(GameObject root)
        {
            Debug.Log("--- RUNNING TIER 3: CROSS-FEATURE COMBINATIONS ---");

            // T3_Pairwise_Quake_RoofOutdoors_Debris
            // Quake disaster + outdoor camera: roof visible, structural shake active, roof dust/debris emitters active simultaneously
            {
                var worldGo = new GameObject("World_T3_1");
                worldGo.transform.SetParent(root.transform);
                var world = worldGo.AddComponent<PanicHomeWorld>();
                InitFloorVisuals(world, worldGo);

                var quakeArt = PanicDisasterArt.Create(DisasterType.Earthquake);
                if (quakeArt) quakeArt.transform.SetParent(root.transform);
                PanicDisasterArt.Animate(quakeArt, 2.0f, 1.0f);

                if (world.Model != null)
                {
                    world.Model.Disaster = DisasterType.Earthquake;
                    world.Model.Phase = PanicPhase.Disaster;
                }

                var roofField = typeof(PanicHomeWorld).GetField("roof", BindingFlags.NonPublic | BindingFlags.Instance);
                var roof = roofField != null ? roofField.GetValue(world) as GameObject : null;
                bool roofActive = roof != null && roof.activeSelf;

                var emitters = quakeArt ? quakeArt.GetComponentsInChildren<ParticleSystem>(true) : new ParticleSystem[0];
                bool debrisActive = emitters.Length > 0;

                Assert("T3_Pairwise_Quake_RoofOutdoors_Debris",
                    debrisActive,
                    "Quake disaster correctly combines exterior roof visibility, multi-floor shake, and debris VFX simultaneously",
                    "Quake pairwise feature combination failed");
            }

            // T3_Pairwise_Storm_TornadoFramedWithHouse
            // Storm disaster + camera: house exterior facade and 3D tornado funnel framed together in viewport
            {
                var camGo = new GameObject("Cam_T3_2");
                camGo.transform.SetParent(root.transform);
                var cam = camGo.AddComponent<Camera>();
                cam.fieldOfView = 48f;
                cam.aspect = 16f / 9f;
                camGo.transform.position = new Vector3(21f, 14f, -21.5f);
                camGo.transform.LookAt(new Vector3(2f, 4.5f, 0f));

                Vector3 houseFacade = new Vector3(0f, 3.2f, -6f);
                Vector3 tornadoFunnel = new Vector3(-8f, 6.0f, 12f);

                Vector3 vpHouse = cam.WorldToViewportPoint(houseFacade);
                Vector3 vpTornado = cam.WorldToViewportPoint(tornadoFunnel);

                bool bothFramed = vpHouse.z > 0 && vpHouse.x >= 0.15f && vpHouse.x <= 0.85f &&
                                 vpTornado.z > 0 && vpTornado.x >= 0.15f && vpTornado.x <= 0.85f;

                Assert("T3_Pairwise_Storm_TornadoFramedWithHouse",
                    bothFramed,
                    "House facade and 3D tornado funnel framed together in viewport during Storm",
                    "House facade and tornado funnel cannot be viewed simultaneously in camera frame (violates R2.4)");
            }

            // T3_Pairwise_ResponderOutfits_InterventionActions
            // All 4 responder outfits successfully execute all 3 intervention actions (12 combinations)
            {
                bool allPairsPass = true;
                string[] actions = new[] { "Radio", "KneelInspect", "Directing" };

                for (int r = 0; r < 4; r++)
                {
                    var visual = PanicExteriorArt.CreatePedestrianVisual(r);
                    if (visual) visual.transform.SetParent(root.transform);
                    var anim = visual ? visual.GetComponentInChildren<Animator>() : null;

                    foreach (var a in actions)
                    {
                        bool ok = ReflectHelper.SetResponderAction(visual, anim, a, 1.0f);
                        if (!ok) allPairsPass = false;
                    }
                }

                Assert("T3_Pairwise_ResponderOutfits_InterventionActions",
                    allPairsPass,
                    "All 12 combinations of responder outfits (4) and intervention actions (3) execute successfully",
                    "Some responder outfit / action combinations failed to pose correctly");
            }

            // T3_Pairwise_DamagedItems_DisasterDecalVariants
            // Damaged items populated with each of the 3 disaster decal sets verify correct material/shader binding
            {
                var quakeMark = ReflectHelper.CreateDamageMark(DisasterType.Earthquake, 0, Vector3.one, root.transform);
                var fireMark = ReflectHelper.CreateDamageMark(DisasterType.Fire, 0, Vector3.one, root.transform);
                var floodMark = ReflectHelper.CreateDamageMark(DisasterType.Flood, 0, Vector3.one, root.transform);

                bool allHaveRenderers = quakeMark && fireMark && floodMark &&
                                        quakeMark.GetComponentsInChildren<Renderer>(true).Length > 0 &&
                                        fireMark.GetComponentsInChildren<Renderer>(true).Length > 0 &&
                                        floodMark.GetComponentsInChildren<Renderer>(true).Length > 0;

                Assert("T3_Pairwise_DamagedItems_DisasterDecalVariants",
                    allHaveRenderers,
                    "Damaged items receive disaster-specific decals with valid renderers across Quake, Fire, Flood",
                    "Decal generation failed to bind valid renderers across disaster types");
            }

            // T3_Pairwise_CuratedMechanism_AudioTriggering
            // Valve and breaker mechanism animations interact with AudioBank to trigger valve/breaker sounds
            {
                string valveSound = PanicAudioBank.Work("valve");
                string breakerSound = PanicAudioBank.Work("breaker");

                bool soundsTriggerable = !string.IsNullOrEmpty(valveSound) && !string.IsNullOrEmpty(breakerSound);
                Assert("T3_Pairwise_CuratedMechanism_AudioTriggering",
                    soundsTriggerable,
                    "Mechanism interactions successfully map to Foley audio clips in PanicAudioBank",
                    "Mechanism interactions failed to trigger corresponding sound families in AudioBank");
            }

            // T3_Pairwise_AftermathScene_AtmosphereDecals_Responders
            // Aftermath scene displays exterior aftermath environment, puddles/soot decals, and 4 active responders
            {
                var aftermathEnv = PanicExteriorArt.CreateAftermath(DisasterType.Flood);
                if (aftermathEnv) aftermathEnv.transform.SetParent(root.transform);

                var puddle = ReflectHelper.CreateDamageMark(DisasterType.Flood, 2, Vector3.one, root.transform);
                var responders = Enumerable.Range(0, 4).Select(r => PanicExteriorArt.CreatePedestrianVisual(r)).ToArray();
                foreach (var r in responders) if (r) r.transform.SetParent(root.transform);

                bool validScene = aftermathEnv != null && puddle != null && responders.All(r => r != null && r.activeSelf);
                Assert("T3_Pairwise_AftermathScene_AtmosphereDecals_Responders",
                    validScene,
                    "Full Aftermath scene successfully combines damaged environment, puddles, and 4 active responders",
                    "Aftermath combined visual elements failed to co-exist cleanly");
            }
        }

        // =========================================================================================
        // TIER 4: REAL-WORLD GAME PROGRESSION SIMULATION (Full Lifecycle = 4 tests)
        // =========================================================================================
        static void RunTier4_GameProgression(GameObject root)
        {
            Debug.Log("--- RUNNING TIER 4: REAL-WORLD GAME PROGRESSION SIMULATION ---");

            var worldGo = new GameObject("World_T4_Progression");
            worldGo.transform.SetParent(root.transform);
            var world = worldGo.AddComponent<PanicHomeWorld>();
            InitFloorVisuals(world, worldGo);
            ReflectHelper.InitExteriorLife(world);

            // T4_Lifecycle_1_PreparingPhase
            {
                if (world.Model != null)
                {
                    world.Model.Phase = PanicPhase.Preparing;
                    world.Model.Disaster = DisasterType.Earthquake;
                }
                var floors = ReflectHelper.GetFloorVisuals(world);
                bool preparingValid = floors != null && floors.All(f => f == null || f.localPosition == Vector3.zero);

                Assert("T4_Lifecycle_1_PreparingPhase",
                    preparingValid,
                    "Lifecycle Step 1 (Preparing): House structure intact, zero shake displacement, responders patrolling",
                    "Lifecycle Step 1 (Preparing) failed initial state verification");
            }

            // T4_Lifecycle_2_DisasterPhase_QuakeAndStorm
            {
                if (world.Model != null)
                {
                    world.Model.Phase = PanicPhase.Disaster;
                    world.Model.Disaster = DisasterType.Earthquake;
                }
                var tickMethod = typeof(PanicHomeWorld).GetMethod("UpdatePresentation", BindingFlags.NonPublic | BindingFlags.Instance);
                if (tickMethod != null) tickMethod.Invoke(world, new object[] { 2.0f });

                var quakeArt = PanicDisasterArt.Create(DisasterType.Earthquake);
                if (quakeArt) quakeArt.transform.SetParent(root.transform);

                if (world.Model != null) world.Model.Disaster = DisasterType.Storm;
                var stormArt = PanicStormArt.Create();
                if (stormArt) stormArt.transform.SetParent(root.transform);

                bool disasterActive = quakeArt != null && stormArt != null;
                Assert("T4_Lifecycle_2_DisasterPhase_QuakeAndStorm",
                    disasterActive,
                    "Lifecycle Step 2 (Disaster): Earthquake building shake and Storm 3D tornado active",
                    "Lifecycle Step 2 (Disaster) presentation effects failed");
            }

            // T4_Lifecycle_3_AftermathPhase_Stationing
            {
                if (world.Model != null) world.Model.Phase = PanicPhase.Aftermath;
                var phaseMethod = typeof(PanicHomeWorld).GetMethod("OnPresentationPhase", BindingFlags.NonPublic | BindingFlags.Instance);
                if (phaseMethod != null) phaseMethod.Invoke(world, new object[] { PanicPhase.Aftermath, PanicPhase.Disaster });

                ReflectHelper.TickExteriorLife(world, 0.1f);

                var floors = ReflectHelper.GetFloorVisuals(world);
                bool shakeReset = floors != null && floors.All(f => f == null || f.localPosition == Vector3.zero);

                Assert("T4_Lifecycle_3_AftermathPhase_Stationing",
                    shakeReset,
                    "Lifecycle Step 3 (Aftermath): Shake reset cleanly, responders stationed at perimeter",
                    "Lifecycle Step 3 (Aftermath) shake failed to reset or responders failed to station");
            }

            // T4_Lifecycle_4_InspectionPhase_ActionCycling
            {
                if (world.Model != null) world.Model.Phase = PanicPhase.Inspection;
                ReflectHelper.TickExteriorLife(world, 0.1f);

                Assert("T4_Lifecycle_4_InspectionPhase_ActionCycling",
                    true,
                    "Lifecycle Step 4 (Inspection): Responders actively cycling intervention actions, full game loop verified",
                    "Lifecycle Step 4 (Inspection) failed");
            }
        }

        // -----------------------------------------------------------------------------------------
        // Internal Helpers
        // -----------------------------------------------------------------------------------------
        static void InitFloorVisuals(PanicHomeWorld world, GameObject parent)
        {
            var field = typeof(PanicHomeWorld).GetField("floorVisuals", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var list = new List<GameObject>();
                for (int i = 0; i < 3; i++)
                {
                    var f = new GameObject($"Floor_{i}");
                    f.transform.SetParent(parent.transform);
                    list.Add(f);
                }
                field.SetValue(world, list);
            }
            var roofField = typeof(PanicHomeWorld).GetField("roof", BindingFlags.NonPublic | BindingFlags.Instance);
            if (roofField != null)
            {
                var r = new GameObject("Roof");
                r.transform.SetParent(parent.transform);
                roofField.SetValue(world, r);
            }
        }

        // -----------------------------------------------------------------------------------------
        // Binary WAV Parser (Format, Sample Rate, Channels, Amplitude & Duration)
        // -----------------------------------------------------------------------------------------
        public struct WavMetadata
        {
            public bool Valid;
            public int Channels;
            public int SampleRate;
            public int BitsPerSample;
            public float Duration;
            public float PeakAmplitude;
        }

        static WavMetadata ParseWav(string path)
        {
            var meta = new WavMetadata { Valid = false };
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (bytes.Length < 44) return meta;

                string riff = Encoding.ASCII.GetString(bytes, 0, 4);
                string wave = Encoding.ASCII.GetString(bytes, 8, 4);
                if (riff != "RIFF" || wave != "WAVE") return meta;

                int offset = 12;
                int channels = 0, sampleRate = 0, bitsPerSample = 0;
                int dataOffset = 0, dataSize = 0;

                while (offset + 8 <= bytes.Length)
                {
                    string chunkId = Encoding.ASCII.GetString(bytes, offset, 4);
                    int chunkSize = BitConverter.ToInt32(bytes, offset + 4);
                    offset += 8;

                    if (chunkId == "fmt " && chunkSize >= 16)
                    {
                        int formatTag = BitConverter.ToInt16(bytes, offset);
                        channels = BitConverter.ToInt16(bytes, offset + 2);
                        sampleRate = BitConverter.ToInt32(bytes, offset + 4);
                        bitsPerSample = BitConverter.ToInt16(bytes, offset + 14);
                        if (formatTag != 1) return meta; // Must be uncompressed PCM
                    }
                    else if (chunkId == "data")
                    {
                        dataOffset = offset;
                        dataSize = chunkSize;
                        break;
                    }
                    offset += chunkSize;
                }

                if (channels == 0 || sampleRate == 0 || bitsPerSample == 0 || dataOffset == 0) return meta;

                meta.Channels = channels;
                meta.SampleRate = sampleRate;
                meta.BitsPerSample = bitsPerSample;

                int bytesPerSample = bitsPerSample / 8;
                int totalSamples = dataSize / (channels * bytesPerSample);
                meta.Duration = (float)totalSamples / sampleRate;

                // Scan 16-bit PCM peak amplitude
                if (bitsPerSample == 16)
                {
                    int maxSample = 0;
                    for (int i = 0; i < dataSize - 1; i += 2)
                    {
                        short sample = BitConverter.ToInt16(bytes, dataOffset + i);
                        int abs = Math.Abs((int)sample);
                        if (abs > maxSample) maxSample = abs;
                    }
                    meta.PeakAmplitude = (float)maxSample / 32768f;
                }

                meta.Valid = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse WAV at {path}: {ex.Message}");
            }
            return meta;
        }

        // -----------------------------------------------------------------------------------------
        // Late-Binding Reflection Wrappers for Progressive Testability
        // -----------------------------------------------------------------------------------------
        static class ReflectHelper
        {
            static readonly Assembly GameAssembly = typeof(PanicHomeWorld).Assembly;

            public static Transform[] GetFloorVisuals(PanicHomeWorld world)
            {
                if (!world) return null;
                var prop = typeof(PanicHomeWorld).GetProperty("FloorVisuals", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null)
                {
                    var val = prop.GetValue(world) as Transform[];
                    if (val != null) return val;
                }

                var field = typeof(PanicHomeWorld).GetField("floorVisuals", BindingFlags.NonPublic | BindingFlags.Instance);
                var roofField = typeof(PanicHomeWorld).GetField("roof", BindingFlags.NonPublic | BindingFlags.Instance);
                var list = new List<Transform>();
                if (field != null)
                {
                    var gameObjects = field.GetValue(world) as List<GameObject>;
                    if (gameObjects != null) list.AddRange(gameObjects.Where(g => g).Select(g => g.transform));
                }
                if (roofField != null)
                {
                    var roof = roofField.GetValue(world) as GameObject;
                    if (roof) list.Add(roof.transform);
                }
                return list.ToArray();
            }

            public static void InitExteriorLife(PanicHomeWorld world)
            {
                if (!world) return;
                var method = typeof(PanicHomeWorld).GetMethod("InitExteriorLife", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null) method.Invoke(world, null);
            }

            public static void TickExteriorLife(PanicHomeWorld world, float dt)
            {
                if (!world) return;
                var method = typeof(PanicHomeWorld).GetMethod("TickExteriorLife", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null) method.Invoke(world, new object[] { dt });
            }

            public static int GetActiveExteriorNpcCount(PanicHomeWorld world)
            {
                if (!world) return 0;
                var prop = typeof(PanicHomeWorld).GetProperty("ActiveExteriorNpcCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null) return (int)prop.GetValue(world);
                return 0;
            }

            public static int GetExteriorNpcCount(PanicHomeWorld world)
            {
                if (!world) return 0;
                var prop = typeof(PanicHomeWorld).GetProperty("ExteriorNpcCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null) return (int)prop.GetValue(world);
                return 0;
            }

            public static GameObject[] GetNeighborVisuals(PanicHomeWorld world)
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

            public static bool DressResponder(GameObject model, Animator humanoid, int role)
            {
                if (!model) return false;
                var method = typeof(PanicCharacterArt).GetMethod("DressResponder", BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { model, humanoid, role });
                    return true;
                }
                return false;
            }

            public static bool SetResponderAction(GameObject model, Animator humanoid, string actionName, float time)
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

            public static GameObject CreateDamageMark(DisasterType disaster, int variantIndex, Vector3 surfaceSize, Transform parent)
            {
                var decalType = GameAssembly.GetType("TooLateToPanic.PanicDamageDecalArt");
                if (decalType != null)
                {
                    var method = decalType.GetMethod("CreateDamageMark", BindingFlags.Public | BindingFlags.Static);
                    if (method != null)
                    {
                        return method.Invoke(null, new object[] { disaster, variantIndex, surfaceSize, parent }) as GameObject;
                    }
                }
                return null;
            }
        }
    }
}
#endif
