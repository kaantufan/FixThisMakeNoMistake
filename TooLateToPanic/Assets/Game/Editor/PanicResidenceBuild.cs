#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TooLateToPanic
{
    public static class PanicResidenceBuild
    {
        static int serial;
        static readonly string Generated = "Assets/Game/Resources/ResidenceGenerated/bake-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        const string HomeArt = "Assets/Game/Resources/HomeArt";
        const string ResidenceArt = "Assets/Game/Resources/ResidenceArt";
        const string ScenePath = "Assets/Game/Scenes/TooLateToPanicResidence.unity";
        const string Version = "0.5.5";

        // Every explicit Create case in the residence r1 package; no default/fallback IDs.
        static readonly string[] ResidenceIds =
        {
            "fireplace", "computer", "desk", "radio", "tablelamp", "floorlamp", "walllamp",
            "toilet", "sink", "bathtub", "towelrack", "washingmachine", "waterheater",
            "storagerack", "car", "toolwall", "bicycle", "books", "plant", "houseplant",
            "shoe_rack", "coat_rack", "curtains", "wallart", "garagedoor", "sofa", "sofa_olive",
            "sofa_indigo", "armchair", "chair", "balconychair", "sidetable", "bed", "rug",
            "laundrybasket", "pendantlamp", "bunkerhatch", "bookshelf", "table", "tvstand", "toolbox", "cabinet", "kitchencabinet", "locker", "bagstation", "medicinecabinet", "supplycrate"
        };

        [MenuItem("Too Late to Panic/Bake All Assets")]
        public static void BakeAll()
        {
            BakeAll(Application.isBatchMode);
        }

        public static void BakeAll(bool exitInBatchMode)
        {
            try
            {
                Debug.Log("HOME_BAKE_ALL Starting full asset bake...");
                BakeAssets();
                Debug.Log("HOME_BAKE_ALL Succeeded");
                if (exitInBatchMode && Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (exitInBatchMode && Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                else throw;
            }
        }

        public static void BakeAssets()
        {
            Folder(Generated); Folder(HomeArt); Folder(ResidenceArt);
            Folder("Assets/Game/Resources/Player"); Folder("Assets/Game/Scenes");

            BakeShaders();
            BakePlayer();

            foreach (var id in new[] { "window", "door", "fridge", "cabinet", "tree", "drain", "fence", "hose", "toolbox", "medicinecabinet", "kitchencabinet", "locker", "supplycrate", "stairs", "workbench", "generator" })
                Bake(HomeArt, id, PanicHomeArt.Create(id));

            Bake(HomeArt, "landscape", PanicHomeArt.BuildLandscape());
            Bake(HomeArt, "floor0", PanicHomeArt.BuildFloor(0, true));
            Bake(HomeArt, "floor1", PanicHomeArt.BuildFloor(1, true));
            Bake(HomeArt, "roof", PanicHomeArt.BuildRoof());

            foreach (var id in ResidenceIds) Bake(ResidenceArt, id, PanicResidenceArt.Create(id));
            foreach (var v5Id in new[] { "v5_shelf_large", "v5_shelf_small", "v5_cabinet_pantry", "v5_cabinet_wardrobe", "v5_cabinet_utility", "v5_microwave" })
                Bake(ResidenceArt, v5Id, PanicV5FurnitureArt.Create(v5Id, true));
            Bake(ResidenceArt, "basement", PanicResidenceArt.BuildBasement());
            Bake(ResidenceArt, "balcony", PanicResidenceArt.BuildBalcony());
            Bake(ResidenceArt, "bathroom", PanicResidenceArt.BuildBathroom());
            Bake(ResidenceArt, "garage", PanicResidenceArt.BuildGarage());
            Bake(ResidenceArt, "bunker_interior", PanicResidenceArt.BuildBunkerInterior());
            BakeCuratedV5Prefabs();

            AssetDatabase.SaveAssets();
        }

        [MenuItem("Too Late to Panic/Bake Curated V5 Prefabs")]
        public static void BakeCuratedV5PrefabsMenu()
        {
            Folder(Generated); Folder(ResidenceArt);
            foreach (var v5Id in new[] { "v5_shelf_large", "v5_shelf_small", "v5_cabinet_pantry", "v5_cabinet_wardrobe", "v5_cabinet_utility", "v5_microwave" })
                Bake(ResidenceArt, v5Id, PanicV5FurnitureArt.Create(v5Id, true));
            BakeCuratedV5Prefabs();
            AssetDatabase.SaveAssets();
            PanicAssetVerification.RunAllTests();
        }

        public static void BakeCuratedV5Prefabs()
        {
            const string modelsDir = "Assets/Game/Resources/CuratedYarkin/Models";
            const string prefabsDir = "Assets/Game/Resources/CuratedYarkin/Prefabs";
            Folder(prefabsDir);

            Transform FindNode(Transform root, string nodeName)
            {
                return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == nodeName);
            }
            Light AddLightChild(Transform parent, string name, LightType type, Color color, float range, float intensity, float spotAngle = 48f, bool enabled = true)
            {
                if (!parent) return null;
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                var l = go.AddComponent<Light>();
                l.type = type; l.color = color; l.range = range; l.intensity = intensity;
                if (type == LightType.Spot) l.spotAngle = spotAngle;
                l.shadows = LightShadows.None; l.renderMode = LightRenderMode.ForceVertex;
                l.enabled = enabled;
                return l;
            }

            string[] allNames =
            {
                "Ambulance", "PoliceCar", "FireEngine", "RescueTruck", "ZodiacBoat",
                "Microwave", "ShelfLarge", "ShelfSmall",
                "CabinetPantry", "CabinetWardrobe", "CabinetUtility",
                "ResponderKit_Police", "ResponderKit_Firefighter", "ResponderKit_Paramedic", "ResponderKit_Rescue"
            };

            foreach (string assetName in allNames)
            {
                string fbxPath = modelsDir + "/SM_" + assetName + ".fbx";
                var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (!fbx)
                {
                    Debug.LogWarning("CURATED_V5_FBX_MISSING " + fbxPath);
                    continue;
                }
                var root = UnityEngine.Object.Instantiate(fbx);
                root.name = "PF_" + assetName;
                var visual = root.transform;

                if (assetName == "Ambulance" || assetName == "PoliceCar" || assetName == "FireEngine" || assetName == "RescueTruck")
                {
                    var rig = root.AddComponent<PanicV5VehicleRig>();
                    rig.ServiceId = assetName;
                    rig.Wheels = new[] { FindNode(visual, "Wheel_FL_Pivot"), FindNode(visual, "Wheel_FR_Pivot"), FindNode(visual, "Wheel_RL_Pivot"), FindNode(visual, "Wheel_RR_Pivot") }.Where(x => x != null).ToArray();
                    rig.SteeringWheel = FindNode(visual, "SteeringWheel_Pivot");
                    var doors = new System.Collections.Generic.List<Transform>();
                    var angles = new System.Collections.Generic.List<float>();
                    void TryDoor(string n, float a) { var d = FindNode(visual, n); if (d) { doors.Add(d); angles.Add(a); } }
                    TryDoor("Door_FrontLeft_Pivot", -64f);
                    TryDoor("Door_FrontRight_Pivot", 64f);
                    TryDoor("Door_RearLeft_Pivot", -74f);
                    TryDoor("Door_RearRight_Pivot", 74f);
                    TryDoor("Compartment_Left_Pivot", -68f);
                    TryDoor("Compartment_Right_Pivot", 68f);
                    rig.Doors = doors.ToArray();
                    rig.DoorAngles = angles.ToArray();

                    var lbL = FindNode(visual, "Lightbar_Left_Emitter");
                    var lbR = FindNode(visual, "Lightbar_Right_Emitter");
                    Color cL = new Color(1f, 0.18f, 0.14f);
                    Color cR = assetName == "RescueTruck" ? new Color(1f, 0.65f, 0.12f) : new Color(0.18f, 0.56f, 1f);
                    var flL = AddLightChild(lbL, "Beacon_Light_L", LightType.Point, cL, 7.5f, 2.0f);
                    var flR = AddLightChild(lbR, "Beacon_Light_R", LightType.Point, cR, 7.5f, 2.0f);
                    rig.FlashLights = new[] { flL, flR }.Where(x => x != null).ToArray();
                    rig.FlashRenderers = new[] { lbL ? lbL.GetComponent<Renderer>() : null, lbR ? lbR.GetComponent<Renderer>() : null }.Where(x => x != null).ToArray();
                    rig.FlashColors = new[] { cL, cR };

                    var hlL = FindNode(visual, "Headlight_Left_Emitter");
                    var hlR = FindNode(visual, "Headlight_Right_Emitter");
                    var spotL = AddLightChild(hlL, "Headlight_Spot_L", LightType.Spot, new Color(1f, 0.97f, 0.88f), 14f, 1.6f, 52f);
                    var spotR = AddLightChild(hlR, "Headlight_Spot_R", LightType.Spot, new Color(1f, 0.97f, 0.88f), 14f, 1.6f, 52f);
                    if (spotL) spotL.transform.localRotation = Quaternion.Euler(8f, 180f, 0f);
                    if (spotR) spotR.transform.localRotation = Quaternion.Euler(8f, 180f, 0f);
                    rig.Headlights = new[] { spotL, spotR }.Where(x => x != null).ToArray();
                    rig.HeadlightRenderers = new[] { hlL ? hlL.GetComponent<Renderer>() : null, hlR ? hlR.GetComponent<Renderer>() : null }.Where(x => x != null).ToArray();

                    var tlL = FindNode(visual, "Taillight_Left_Emitter");
                    var tlR = FindNode(visual, "Taillight_Right_Emitter");
                    var ptL = AddLightChild(tlL, "Taillight_Point_L", LightType.Point, new Color(1f, 0.14f, 0.10f), 3.2f, 0.85f);
                    var ptR = AddLightChild(tlR, "Taillight_Point_R", LightType.Point, new Color(1f, 0.14f, 0.10f), 3.2f, 0.85f);
                    rig.Taillights = new[] { ptL, ptR }.Where(x => x != null).ToArray();
                    rig.TaillightRenderers = new[] { tlL ? tlL.GetComponent<Renderer>() : null, tlR ? tlR.GetComponent<Renderer>() : null }.Where(x => x != null).ToArray();

                    if (assetName == "Ambulance")
                    {
                        var cabNode = FindNode(visual, "CabinInterior_Light_Emitter");
                        rig.InteriorLight = AddLightChild(cabNode, "CabinInterior_PointLight", LightType.Point, new Color(0.95f, 0.98f, 1f), 2.8f, 1.15f, 48f, false);
                        if (cabNode) rig.InteriorLightRenderer = cabNode.GetComponent<Renderer>();
                    }
                    else if (assetName == "FireEngine")
                    {
                        rig.LadderBasePivot = FindNode(visual, "Ladder_Turntable_Pivot");
                        rig.LadderExtendPivot = FindNode(visual, "Ladder_FlySection_Pivot");
                        rig.RoofMonitorPivot = FindNode(visual, "WaterMonitor_Pivot");
                    }
                    else if (assetName == "RescueTruck")
                    {
                        rig.WinchPivot = FindNode(visual, "FrontWinch_Drum_Pivot");
                        rig.CraneBoomPivot = FindNode(visual, "RoofCrane_Boom_Pivot");
                    }
                }
                else if (assetName == "ZodiacBoat")
                {
                    var rig = root.AddComponent<PanicV5VehicleRig>();
                    rig.ServiceId = "FloodZodiac";
                    rig.IsBoat = true;
                    rig.OutboardPivot = FindNode(visual, "OutboardMotor_Pivot");
                    rig.Propeller = FindNode(visual, "Propeller_Pivot");
                    rig.SteeringWheel = FindNode(visual, "ConsoleSteering_Pivot");
                    var bowNode = FindNode(visual, "BowSearchlight_Emitter") ?? FindNode(visual, "BowSearchlight_Pivot");
                    var bowSpot = AddLightChild(bowNode, "BowSearchlight_Spot", LightType.Spot, new Color(1f, 0.97f, 0.88f), 16f, 1.85f, 46f);
                    if (bowSpot) bowSpot.transform.localRotation = Quaternion.Euler(10f, 180f, 0f);
                    rig.Headlights = bowSpot ? new[] { bowSpot } : Array.Empty<Light>();
                    rig.HeadlightRenderers = bowNode && bowNode.GetComponent<Renderer>() ? new[] { bowNode.GetComponent<Renderer>() } : Array.Empty<Renderer>();
                    var sternNode = FindNode(visual, "SternBeacon_Emitter");
                    var sternPt = AddLightChild(sternNode, "SternBeacon_Point", LightType.Point, new Color(1f, 0.56f, 0.12f), 6.5f, 1.8f);
                    rig.FlashLights = sternPt ? new[] { sternPt } : Array.Empty<Light>();
                    rig.FlashRenderers = sternNode && sternNode.GetComponent<Renderer>() ? new[] { sternNode.GetComponent<Renderer>() } : Array.Empty<Renderer>();
                    rig.FlashColors = new[] { new Color(1f, 0.56f, 0.12f) };
                }
                else if (assetName == "Microwave" || assetName == "ShelfLarge" || assetName == "ShelfSmall" ||
                         assetName == "CabinetPantry" || assetName == "CabinetWardrobe" || assetName == "CabinetUtility")
                {
                    var rig = root.AddComponent<PanicV5FurnitureRig>();
                    rig.ArtId = assetName;
                    rig.TurntablePivot = FindNode(visual, "Turntable_Pivot");
                    var dSingle = FindNode(visual, "Door_Pivot");
                    var dLeft = FindNode(visual, "Door_Left_Pivot");
                    var dRight = FindNode(visual, "Door_Right_Pivot");
                    if (dSingle) { rig.Doors = new[] { dSingle }; rig.OpenAngles = new[] { -95f }; }
                    else if (dLeft && dRight) { rig.Doors = new[] { dLeft, dRight }; rig.OpenAngles = new[] { -96f, 96f }; }
                    var drTop = FindNode(visual, "Drawer_Top_Pivot");
                    var drMid = FindNode(visual, "Drawer_Mid_Pivot");
                    var drBot = FindNode(visual, "Drawer_Bot_Pivot");
                    var drawers = new[] { drTop, drMid, drBot }.Where(x => x != null).ToArray();
                    if (drawers.Length > 0)
                    {
                        rig.Drawers = drawers;
                        rig.DrawerDistances = drawers.Select((_, idx) => -0.22f + idx * 0.03f).ToArray();
                    }
                    var lampNode = FindNode(visual, "CavityLight_Emitter") ?? FindNode(visual, "InteriorLight_Emitter");
                    if (lampNode)
                    {
                        rig.InteriorLight = AddLightChild(lampNode, "Interior_PointLight", LightType.Point, new Color(1f, 0.88f, 0.62f), 1.45f, 0.58f, 48f, false);
                        rig.InteriorLightRenderer = lampNode.GetComponent<Renderer>();
                    }
                }
                else if (assetName == "ResponderKit_Firefighter" || assetName == "ResponderKit_Rescue")
                {
                    var hlNode = FindNode(visual, "Helmet_Headlamp_Emitter");
                    var spot = AddLightChild(hlNode, "Headlamp_SpotLight", LightType.Spot, new Color(1f, 0.96f, 0.82f), 5.8f, 0.88f, 46f);
                    if (spot) spot.transform.localRotation = Quaternion.Euler(8f, 180f, 0f);
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabsDir + "/PF_" + assetName + ".prefab");
                UnityEngine.Object.DestroyImmediate(root);
            }
            Debug.Log("HOME_V5_CURATED_PREFABS_BAKED count=" + allNames.Length);
        }

        static void BakeShaders()
        {
            foreach (var shaderName in new[] { "Standard", "Sprites/Default", "Particles/Standard Unlit", "Skybox/Procedural", "Unlit/Color", "Panic/SoftWeather" })
            {
                var shader = Shader.Find(shaderName); if (shader == null) continue;
                var mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, Generated + "/shader-" + serial++ + ".mat");
            }
        }

        [MenuItem("Too Late to Panic/Build Home V5")]
        public static void Build()
        {
            try
            {
                Folder(Generated); Folder(HomeArt); Folder(ResidenceArt);
                Folder("Assets/Game/Resources/Player"); Folder("Assets/Game/Scenes");
                PlayerSettings.companyName = "Civil Game Jam";
                // A layout patch keeps the V4 persistentDataPath and existing local highscores.
                PlayerSettings.productName = "Too Late to Panic! — Home V4";
                PlayerSettings.bundleVersion = Version;
                PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900;
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
                PlayerSettings.resizableWindow = true; PlayerSettings.runInBackground = false;
                PlayerSettings.colorSpace = ColorSpace.Linear; PlayerSettings.usePlayerLog = true;
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
                PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
                PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Disabled);
                ConfigureBrand();

                bool reuseArt=Environment.GetCommandLineArgs().Contains("-reuseResidenceArt");
                bool rebakeSurvivor=Environment.GetCommandLineArgs().Contains("-rebakeSurvivor");
                if(reuseArt)
                {
                    foreach(var path in new[]{HomeArt+"/floor0.prefab",HomeArt+"/floor1.prefab",ResidenceArt+"/garage.prefab",ResidenceArt+"/bunker_interior.prefab","Assets/Game/Resources/Player/SurvivorV2.prefab"})
                        if(!AssetDatabase.LoadAssetAtPath<GameObject>(path))throw new Exception("Cached V3 art missing: "+path);
                    Debug.Log("HOME_V48_REUSE_ART previously baked current residence art; sources, audio and offline fonts imported separately");
                    if (rebakeSurvivor) BakePlayer();
                }
                else
                {
                    BakeAll(exitInBatchMode: false);
                }

                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("Too Late to Panic — Home V5").AddComponent<PanicHomeWorld>();
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
                AssetDatabase.SaveAssets();
                var project = Directory.GetParent(Application.dataPath).FullName;
                var outputDir = Path.Combine(project, "Builds", "Home-V5.2-20261018");
                var buildArgs = Environment.GetCommandLineArgs();
                int auditOutput = Array.IndexOf(buildArgs, "-panicBuildDirectory");
                if (auditOutput >= 0)
                {
                    if (auditOutput + 1 >= buildArgs.Length) throw new ArgumentException("Missing -panicBuildDirectory value");
                    string buildsRoot = Path.GetFullPath(Path.Combine(project, "Builds")) + Path.DirectorySeparatorChar;
                    outputDir = Path.GetFullPath(Path.Combine(project, "Builds", buildArgs[auditOutput + 1]));
                    if (!outputDir.StartsWith(buildsRoot, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Candidate output must be within this project's Builds");
                }
                Directory.CreateDirectory(outputDir);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath }, locationPathName = Path.Combine(outputDir, "TooLateToPanic.exe"),
                    target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
                });
                File.WriteAllText(Path.Combine(outputDir, "build-receipt.json"), JsonUtility.ToJson(new Receipt
                {
                    utc = DateTime.UtcNow.ToString("o"), version = Version, scene = ScenePath,
                    result = report.summary.result.ToString(), errors = report.summary.totalErrors,
                    warnings = report.summary.totalWarnings, bytes = (long)report.summary.totalSize,
                    nativeObserved = false
                }, true));
                Debug.Log("HOME_V48_BUILD " + report.summary.result + " errors=" + report.summary.totalErrors + " bytes=" + report.summary.totalSize);
                if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(2);
            }
            catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(3); }
        }

        // Title logo and exe icon (Documentation/HUD-Concept/logo). Import settings are enforced here,
        // so the staged .meta files only need to carry stable GUIDs.
        static void ConfigureBrand()
        {
            Texture2D Import(string path, bool mips)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new Exception("Brand texture missing: " + path);
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = mips;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = mips ? FilterMode.Trilinear : FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            Import("Assets/Game/Resources/HUD/TltpLogo.png", true);
            var mark = Import("Assets/Game/Brand/TltpMark.png", false);
            var markSmall = Import("Assets/Game/Brand/TltpMarkSmall.png", false);
            // 16-32 px use the crack-free mark; larger sizes the full one.
            var sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Any);
            PlayerSettings.SetIcons(NamedBuildTarget.Standalone, sizes.Select(s => s <= 32 ? markSmall : mark).ToArray(), IconKind.Any);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { mark }, IconKind.Any);
            Debug.Log("HOME_BRAND logo+icon sizes=" + string.Join(",", sizes));
        }

        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            Folder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void Bake(string folder, string id, GameObject go)
        {
            Persist(go);
            PrefabUtility.SaveAsPrefabAsset(go, folder + "/" + id + ".prefab");
            UnityEngine.Object.DestroyImmediate(go);
        }

        static void Persist(GameObject root)
        {
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh && !AssetDatabase.Contains(filter.sharedMesh))
                    AssetDatabase.CreateAsset(filter.sharedMesh, Generated + "/mesh-" + serial++ + ".asset");
            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (renderer.sharedMesh && !AssetDatabase.Contains(renderer.sharedMesh))
                    AssetDatabase.CreateAsset(renderer.sharedMesh, Generated + "/skin-mesh-" + serial++ + ".asset");
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                foreach (var mat in renderer.sharedMaterials)
                    if (mat && !AssetDatabase.Contains(mat))
                    {
                        string materialName=mat.name;foreach(char invalid in Path.GetInvalidFileNameChars())materialName=materialName.Replace(invalid,'_');
                        AssetDatabase.CreateAsset(mat, Generated + "/" + materialName + "__" + serial++ + ".mat");
                    }
        }

        static void BakePlayer()
        {
            const string source = "Assets/Vendor/Player/Superhero_Male_FullBody.fbx";
            var importer = AssetImporter.GetAtPath(source) as ModelImporter;
            if (importer != null && !importer.isReadable) { importer.isReadable = true; importer.SaveAndReimport(); }
            var body = AssetDatabase.LoadAssetAtPath<GameObject>(source);
            if (!body) throw new Exception("Licensed player source missing");
            var root = UnityEngine.Object.Instantiate(body); root.name = "Survivor V414 — retained humanoid civilian";
            var animator = root.GetComponent<Animator>() ?? root.AddComponent<Animator>();
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(source).OfType<Avatar>().FirstOrDefault();
            PanicCharacterArt.Dress(root, animator);
            string controllerPath = "Assets/Game/Resources/Player/SurvivorV2.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)) AssetDatabase.DeleteAsset(controllerPath);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ac.AddParameter("Working", AnimatorControllerParameterType.Bool);
            ac.AddParameter("Kneeling", AnimatorControllerParameterType.Bool);
            ac.AddParameter("Airborne", AnimatorControllerParameterType.Bool);
            var clips = AssetDatabase.LoadAllAssetsAtPath("Assets/Vendor/Player/UAL1_Standard.fbx")
                .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToArray();
            AnimationClip Clip(string exact, string fallback)
            {
                return clips.FirstOrDefault(c => c.name.EndsWith("|" + exact, StringComparison.OrdinalIgnoreCase))
                    ?? clips.FirstOrDefault(c => c.name.IndexOf(fallback, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            var jumpClip = Clip("Jump_Loop", "Jump_Loop");
            if (!jumpClip || !Clip("Idle_Loop", "Idle_Loop") || !Clip("Walk_Loop", "Walk") || !Clip("Jog_Fwd_Loop", "Jog") ||
                !(Clip("Interact", "Interact") ?? Clip("Push_Loop", "Push")) || !Clip("Fixing_Kneeling", "Fixing"))
                throw new InvalidOperationException("Retained V4.13 humanoid requires real Idle/Walk/Jog/Jump/Interact/Fixing clips; placeholder fallback refused.");
            var sm = ac.layers[0].stateMachine;
            var locomotion = sm.AddState("Smooth locomotion");
            var tree = new BlendTree { name = "Idle walk run", blendType = BlendTreeType.Simple1D, blendParameter = "Speed", useAutomaticThresholds = false };
            AssetDatabase.AddObjectToAsset(tree, ac);
            tree.AddChild(Clip("Idle_Loop", "Idle_Loop"), 0);
            tree.AddChild(Clip("Walk_Loop", "Walk"), .64f);
            tree.AddChild(Clip("Jog_Fwd_Loop", "Jog"), 1);
            locomotion.motion = tree; sm.defaultState = locomotion;
            var jump = sm.AddState("Jump loop"); jump.motion = jumpClip;
            Transition(locomotion, jump, "Airborne", true, duration: .1f);
            Transition(jump, locomotion, "Airborne", false, duration: .1f);
            var work = sm.AddState("Standing work");
            work.motion = Clip("Interact", "Interact") ?? Clip("Push_Loop", "Push");
            var kneel = sm.AddState("Ground work");
            kneel.motion = Clip("Fixing_Kneeling", "Fixing") ?? work.motion;
            Transition(locomotion, work, "Working", true, "Kneeling", false);
            Transition(locomotion, kneel, "Working", true, "Kneeling", true);
            Transition(work, locomotion, "Working", false);
            Transition(kneel, locomotion, "Working", false);
            animator.runtimeAnimatorController = ac; animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            foreach (var r in root.GetComponentsInChildren<SkinnedMeshRenderer>()) r.updateWhenOffscreen = true;
            Persist(root);
            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Game/Resources/Player/SurvivorV2.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log("HOME_V414_RETAINED_V413_HUMANOID_BAKED clips=" + string.Join(",", tree.children.Select(x => x.motion ? x.motion.name : "MISSING")) + " jump=" + jumpClip.name);
        }

        static void Transition(AnimatorState from, AnimatorState to, string flag, bool value, string other = null, bool otherValue = false, float duration = .16f)
        {
            var t = from.AddTransition(to); t.hasExitTime = false; t.hasFixedDuration = true; t.duration = duration;
            t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, flag);
            if (other != null) t.AddCondition(otherValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, other);
        }

        [Serializable]
        class Receipt { public string utc, version, scene, result; public int errors, warnings; public long bytes; public bool nativeObserved; }
    }
}
#endif


