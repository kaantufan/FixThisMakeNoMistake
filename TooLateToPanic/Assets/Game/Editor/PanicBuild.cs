#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
    public static class PanicBuild
    {
        static readonly string[] ArtIds = { "bookshelf", "sofa", "table", "chair", "fridge", "cabinet", "bed", "plant", "tvstand", "bagstation", "bunker", "window", "door", "wall", "roof", "fireunit", "water", "food", "firstaid", "flashlight", "radio", "blanket" };

        [MenuItem("Too Late to Panic/Build MVP")]
        public static void Build()
        {
            try
            {
                Configure();
                PrepareExistingPlayer();
                BakeArt();
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("Too Late to Panic — Game").AddComponent<PanicWorld>();
                EditorSceneManager.SaveScene(scene, "Assets/Game/Scenes/TooLateToPanic.unity");
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Game/Scenes/TooLateToPanic.unity", true) };
                AssetDatabase.SaveAssets();
                string project = Directory.GetParent(Application.dataPath).FullName;
                string output = Path.Combine(project, "Builds", "MVP-20261016", "TooLateToPanic.exe");
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Game/Scenes/TooLateToPanic.unity" },
                    locationPathName = output,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                });
                File.WriteAllText(Path.Combine(project, "Builds", "MVP-20261016", "build-receipt.json"), JsonUtility.ToJson(new Receipt
                {
                    utc = DateTime.UtcNow.ToString("o"), result = report.summary.result.ToString(), errors = report.summary.totalErrors,
                    warnings = report.summary.totalWarnings, bytes = (long)report.summary.totalSize, executable = output,
                    artKinds = ArtIds.Length, humansGenerated = 0, nativeObserved = false
                }, true));
                Debug.Log("PANIC_BUILD " + report.summary.result + " bytes=" + report.summary.totalSize + " errors=" + report.summary.totalErrors);
                if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(2);
            }
            catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(3); }
        }

        static void Configure()
        {
            PlayerSettings.companyName = "Civil Game Jam";
            PlayerSettings.productName = "Too Late to Panic!";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.usePlayerLog = true;
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Disabled);
            QualitySettings.antiAliasing = 4;
            QualitySettings.pixelLightCount = 4;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowDistance = 45;
            QualitySettings.vSyncCount = 0;
            if (!AssetDatabase.IsValidFolder("Assets/Game/Resources/Generated")) AssetDatabase.CreateFolder("Assets/Game/Resources", "Generated");
        }

        static void PrepareExistingPlayer()
        {
            string source = "Assets/Vendor/Player/Superhero_Male_FullBody.fbx";
            var body = AssetDatabase.LoadAssetAtPath<GameObject>(source);
            if (body == null) throw new Exception("Existing licensed character source missing.");
            var root = new GameObject("Existing survivor");
            var model = UnityEngine.Object.Instantiate(body, root.transform);
            model.name = "Quaternius Standard existing model";
            model.transform.localPosition = Vector3.zero;
            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            var avatar = AssetDatabase.LoadAllAssetsAtPath(source).OfType<Avatar>().FirstOrDefault();
            if (avatar != null) animator.avatar = avatar;
            var clips = AssetDatabase.LoadAllAssetsAtPath("Assets/Vendor/Player/UAL1_Standard.fbx").OfType<AnimationClip>().Where(x => !x.name.StartsWith("__preview")).ToArray();
            Debug.Log("PANIC_EXISTING_CLIPS " + string.Join(", ", clips.Select(c => c.name)));
            var idle = clips.FirstOrDefault(c => c.name.EndsWith("|Idle_Loop", StringComparison.OrdinalIgnoreCase)) ?? clips.FirstOrDefault(c => c.name == "Idle_Loop");
            var walk = clips.FirstOrDefault(c => c.name.IndexOf("Walk_Fwd", StringComparison.OrdinalIgnoreCase) >= 0) ?? clips.FirstOrDefault(c => c.name.IndexOf("Walk", StringComparison.OrdinalIgnoreCase) >= 0);
            var ac = AnimatorController.CreateAnimatorControllerAtPath("Assets/Game/Resources/Player/ExistingSurvivor.controller");
            ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var sm = ac.layers[0].stateMachine;
            var idleState = sm.AddState("Idle"); idleState.motion = idle;
            var walkState = sm.AddState("Walk"); walkState.motion = walk;
            sm.defaultState = idleState;
            var transition = idleState.AddTransition(walkState); transition.hasExitTime = false; transition.duration = .12f; transition.AddCondition(AnimatorConditionMode.Greater, .1f, "Speed");
            transition = walkState.AddTransition(idleState); transition.hasExitTime = false; transition.duration = .12f; transition.AddCondition(AnimatorConditionMode.Less, .1f, "Speed");
            animator.runtimeAnimatorController = ac; animator.applyRootMotion = false;
            var mat = new Material(Shader.Find("Standard")); mat.name = "Existing player — dark teal"; mat.color = new Color(.13f, .31f, .32f); mat.SetFloat("_Glossiness", .18f);
            string matPath = "Assets/Game/Resources/Player/PlayerTeal.mat";
            var existingMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existingMat == null) { AssetDatabase.CreateAsset(mat, matPath); existingMat = mat; } else UnityEngine.Object.DestroyImmediate(mat);
            var existingTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Vendor/Player/Textures/T_Superhero_Male_Dark.png");
            if (existingTexture != null) { existingMat.mainTexture = existingTexture; existingMat.color = Color.white; }
            foreach (var r in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var mats = r.sharedMaterials; for (int i = 0; i < mats.Length; i++) mats[i] = existingMat; r.sharedMaterials = mats;
                r.updateWhenOffscreen = true;
            }
            // Existing CC0 source is reused unchanged; no new body, rig or residents generated.
            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Game/Resources/Player/ExistingSurvivor.prefab");
            UnityEngine.Object.DestroyImmediate(root);
        }

        static void BakeArt()
        {
            var items = new List<string>(ArtIds); items.Add("room"); items.Add("floodguard"); items.Add("anchor_hardware");
            foreach (string sign in new[] { "bag", "bunker", "earthquake", "flood", "fire" }) items.Add("sign_" + sign);
            int meshes = 0, materials = 0;
            foreach (string id in items)
            {
                GameObject go;
                if (id == "room") go = PanicArt.CreateRoom();
                else if (id == "floodguard") go = PanicAttachmentArt.CreateFloodGuard(1.37f, .75f);
                else if (id == "anchor_hardware") go = PanicAttachmentArt.CreateAnchors(new Vector3(1.4f, 1.65f, .6f));
                else if (id.StartsWith("sign_")) go = PanicAttachmentArt.CreateSafetySign(id.Substring(5));
                else go = PanicArt.Create(id);
                foreach (var filter in go.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null || AssetDatabase.Contains(filter.sharedMesh)) continue;
                    string path = "Assets/Game/Resources/Generated/" + id + "-mesh-" + meshes++ + ".asset";
                    AssetDatabase.CreateAsset(filter.sharedMesh, path);
                }
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                    foreach (var mat in renderer.sharedMaterials)
                    {
                        if (mat == null || AssetDatabase.Contains(mat)) continue;
                        AssetDatabase.CreateAsset(mat, "Assets/Game/Resources/Generated/material-" + materials++ + ".mat");
                    }
                PrefabUtility.SaveAsPrefabAsset(go, "Assets/Game/Resources/Art/" + id + ".prefab");
                UnityEngine.Object.DestroyImmediate(go);
            }
            Debug.Log("PANIC_ART_BAKED kinds=" + ArtIds.Length + " meshes=" + meshes + " materials=" + materials);
        }

        [Serializable] class Receipt { public string utc, result, executable; public int errors, warnings; public long bytes; public int artKinds, humansGenerated; public bool nativeObserved; }
    }
}
#endif
