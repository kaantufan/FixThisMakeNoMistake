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
        const string Version = "0.5.3";

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
                // Preserve the shaders used by runtime-authored work, weather, icons and cutaways.
                foreach (var shaderName in new[] { "Standard", "Sprites/Default", "Particles/Standard Unlit", "Skybox/Procedural", "Unlit/Color", "Panic/SoftWeather" })
                {
                    var shader = Shader.Find(shaderName); if (shader == null) continue;
                    var mat = new Material(shader);
                    AssetDatabase.CreateAsset(mat, Generated + "/shader-" + serial++ + ".mat");
                }

                BakePlayer();
                foreach (var id in new[] { "window", "door", "fridge", "cabinet", "tree", "drain", "fence", "hose", "toolbox", "medicinecabinet", "kitchencabinet", "locker", "supplycrate", "stairs", "workbench", "generator" })
                    Bake(HomeArt, id, PanicHomeArt.Create(id));

                // Explicit V3 overloads keep the basement stair hole and upper balcony doorway.
                Bake(HomeArt, "landscape", PanicHomeArt.BuildLandscape());
                Bake(HomeArt, "floor0", PanicHomeArt.BuildFloor(0, true));
                Bake(HomeArt, "floor1", PanicHomeArt.BuildFloor(1, true));
                Bake(HomeArt, "roof", PanicHomeArt.BuildRoof());
                foreach (var id in ResidenceIds) Bake(ResidenceArt, id, PanicResidenceArt.Create(id));
                Bake(ResidenceArt, "basement", PanicResidenceArt.BuildBasement());
                Bake(ResidenceArt, "balcony", PanicResidenceArt.BuildBalcony());
                Bake(ResidenceArt, "bathroom", PanicResidenceArt.BuildBathroom());
                Bake(ResidenceArt, "garage", PanicResidenceArt.BuildGarage());
                Bake(ResidenceArt, "bunker_interior", PanicResidenceArt.BuildBunkerInterior());
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


