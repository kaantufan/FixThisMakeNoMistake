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
    public static class PanicHomeBuild
    {
        static int serial;
        static readonly string Generated = "Assets/Game/Resources/HomeGenerated/bake-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        [MenuItem("Too Late to Panic/Build Home V2")]
        public static void Build()
        {
            try
            {
                Folder(Generated); Folder("Assets/Game/Resources/HomeArt"); Folder("Assets/Game/Resources/Player");
                PlayerSettings.companyName = "Civil Game Jam"; PlayerSettings.productName = "Too Late to Panic! — Home V2";
                PlayerSettings.bundleVersion = "0.2.0"; PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900;
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true; PlayerSettings.runInBackground = false;
                PlayerSettings.colorSpace = ColorSpace.Linear; PlayerSettings.usePlayerLog = true;
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
                PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
                PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Disabled);
                // Keep runtime-authored work rigs, weather and cutaway geometry shaders available.
                foreach (var shaderName in new[] { "Standard", "Sprites/Default", "Particles/Standard Unlit", "Skybox/Procedural", "Unlit/Color", "Panic/SoftWeather" })
                {
                    var shader = Shader.Find(shaderName); if (shader == null) continue;
                    var mat = new Material(shader); AssetDatabase.CreateAsset(mat, Generated + "/shader-" + serial++ + ".mat");
                }
                BakePlayer();
                foreach (var id in new[] { "window", "door", "fridge", "cabinet", "tree", "drain", "fence", "hose", "toolbox", "medicinecabinet", "kitchencabinet", "locker", "supplycrate", "stairs", "workbench", "generator" })
                {
                    var go = PanicHomeArt.Create(id); Persist(go); PrefabUtility.SaveAsPrefabAsset(go, "Assets/Game/Resources/HomeArt/" + id + ".prefab"); UnityEngine.Object.DestroyImmediate(go);
                }
                // Runtime factories remain source-of-truth; cache the expensive house/environment hierarchy.
                Bake("landscape", PanicHomeArt.BuildLandscape()); Bake("floor0", PanicHomeArt.BuildFloor(0)); Bake("floor1", PanicHomeArt.BuildFloor(1)); Bake("roof", PanicHomeArt.BuildRoof());
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("Too Late to Panic — Home V2").AddComponent<PanicHomeWorld>();
                string scenePath = "Assets/Game/Scenes/TooLateToPanicHome.unity"; EditorSceneManager.SaveScene(scene, scenePath);
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) }; AssetDatabase.SaveAssets();
                var project = Directory.GetParent(Application.dataPath).FullName; var outputDir = Path.Combine(project, "Builds", "Home-V2-20261016"); Directory.CreateDirectory(outputDir);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { scenePath }, locationPathName = Path.Combine(outputDir, "TooLateToPanic.exe"), target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
                File.WriteAllText(Path.Combine(outputDir, "build-receipt.json"), JsonUtility.ToJson(new Receipt { utc = DateTime.UtcNow.ToString("o"), result = report.summary.result.ToString(), errors = report.summary.totalErrors, warnings = report.summary.totalWarnings, bytes = (long)report.summary.totalSize, nativeObserved = false }, true));
                Debug.Log("HOME_BUILD " + report.summary.result + " errors=" + report.summary.totalErrors + " bytes=" + report.summary.totalSize);
                if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(2);
            }
            catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(3); }
        }
        static void Folder(string path) { if (AssetDatabase.IsValidFolder(path)) return; string parent = Path.GetDirectoryName(path).Replace('\\', '/'); Folder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path)); }
        static void Bake(string id, GameObject go) { Persist(go); PrefabUtility.SaveAsPrefabAsset(go, "Assets/Game/Resources/HomeArt/" + id + ".prefab"); UnityEngine.Object.DestroyImmediate(go); }
        static void Persist(GameObject root)
        {
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true)) if (filter.sharedMesh && !AssetDatabase.Contains(filter.sharedMesh)) AssetDatabase.CreateAsset(filter.sharedMesh, Generated + "/mesh-" + serial++ + ".asset");
            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)) if (renderer.sharedMesh && !AssetDatabase.Contains(renderer.sharedMesh)) AssetDatabase.CreateAsset(renderer.sharedMesh, Generated + "/skin-mesh-" + serial++ + ".asset");
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) foreach (var mat in renderer.sharedMaterials) if (mat && !AssetDatabase.Contains(mat)) AssetDatabase.CreateAsset(mat, Generated + "/material-" + serial++ + ".mat");
        }
        static void BakePlayer()
        {
            const string source = "Assets/Vendor/Player/Superhero_Male_FullBody.fbx";
            var importer = AssetImporter.GetAtPath(source) as ModelImporter;
            if (importer != null && !importer.isReadable) { importer.isReadable = true; importer.SaveAndReimport(); }
            var body = AssetDatabase.LoadAssetAtPath<GameObject>(source); if (!body) throw new Exception("Licensed player source missing");
            var root = UnityEngine.Object.Instantiate(body); root.name = "Survivor V2 — workwear";
            var animator = root.GetComponent<Animator>() ?? root.AddComponent<Animator>(); animator.avatar = AssetDatabase.LoadAllAssetsAtPath(source).OfType<Avatar>().FirstOrDefault();
            PanicCharacterArt.Dress(root, animator);
            string controllerPath = "Assets/Game/Resources/Player/SurvivorV2.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)) AssetDatabase.DeleteAsset(controllerPath);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            ac.AddParameter("Speed", AnimatorControllerParameterType.Float); ac.AddParameter("Working", AnimatorControllerParameterType.Bool); ac.AddParameter("Kneeling", AnimatorControllerParameterType.Bool);
            var clips = AssetDatabase.LoadAllAssetsAtPath("Assets/Vendor/Player/UAL1_Standard.fbx").OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToArray();
            AnimationClip Clip(string exact, string fallback) { return clips.FirstOrDefault(c => c.name.EndsWith("|" + exact, StringComparison.OrdinalIgnoreCase)) ?? clips.FirstOrDefault(c => c.name.IndexOf(fallback, StringComparison.OrdinalIgnoreCase) >= 0); }
            var sm = ac.layers[0].stateMachine;
            var locomotion = sm.AddState("Smooth locomotion"); var tree = new BlendTree { name = "Idle walk run", blendType = BlendTreeType.Simple1D, blendParameter = "Speed", useAutomaticThresholds = false }; AssetDatabase.AddObjectToAsset(tree, ac);
            tree.AddChild(Clip("Idle_Loop", "Idle_Loop"), 0); tree.AddChild(Clip("Walk_Loop", "Walk"), .64f); tree.AddChild(Clip("Jog_Fwd_Loop", "Jog"), 1); locomotion.motion = tree; sm.defaultState = locomotion;
            var work = sm.AddState("Standing work"); work.motion = Clip("Interact", "Interact") ?? Clip("Push_Loop", "Push");
            var kneel = sm.AddState("Ground work"); kneel.motion = Clip("Fixing_Kneeling", "Fixing") ?? work.motion;
            Transition(locomotion, work, "Working", true, "Kneeling", false); Transition(locomotion, kneel, "Working", true, "Kneeling", true);
            Transition(work, locomotion, "Working", false); Transition(kneel, locomotion, "Working", false);
            animator.runtimeAnimatorController = ac; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            foreach (var r in root.GetComponentsInChildren<SkinnedMeshRenderer>()) r.updateWhenOffscreen = true;
            Persist(root); PrefabUtility.SaveAsPrefabAsset(root, "Assets/Game/Resources/Player/SurvivorV2.prefab"); UnityEngine.Object.DestroyImmediate(root);
            Debug.Log("HOME_PLAYER_BAKED clips=" + string.Join(",", tree.children.Select(x => x.motion ? x.motion.name : "MISSING")));
        }
        static void Transition(AnimatorState from, AnimatorState to, string flag, bool value, string other = null, bool otherValue = false) { var t = from.AddTransition(to); t.hasExitTime = false; t.duration = .16f; t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, flag); if (other != null) t.AddCondition(otherValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, other); }
        [Serializable] class Receipt { public string utc, result; public int errors, warnings; public long bytes; public bool nativeObserved; }
    }
}
#endif
