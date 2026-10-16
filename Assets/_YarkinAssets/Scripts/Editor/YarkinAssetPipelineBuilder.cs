#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using YarkinAssets.Runtime;

namespace YarkinAssets.Editor
{
    public static class YarkinAssetPipelineBuilder
    {
        private const string RootDir = "Assets/_YarkinAssets";
        private const string MeshesDir = RootDir + "/Art/Meshes";
        private const string MaterialsDir = RootDir + "/Art/Materials";
        private const string IconsDir = RootDir + "/Art/Icons";
        private const string PrefabsDir = RootDir + "/Prefabs";
        private const string ScenesDir = RootDir + "/Scenes";

        private struct PaletteDef
        {
            public string Key;
            public Color Color;
            public float Metallic;
            public float Smoothness;
            public bool Emissive;

            public PaletteDef(string key, float r, float g, float b, float metallic, float roughness, bool emissive = false)
            {
                Key = key;
                Color = new Color(r, g, b, 1f);
                Metallic = metallic;
                Smoothness = Mathf.Clamp01(1f - roughness);
                Emissive = emissive;
            }
        }

        private enum ColliderKind { Box, Capsule }

        private struct AssetSpec
        {
            public string CodeName;
            public string ItemId;
            public string DisplayNameTr;
            public DisasterItemCategory Category;
            public float WeightKg;
            public int Score;
            public bool IsCollectible;
            public bool IsSecurableFixture;
            public ColliderKind Collider;

            public AssetSpec(string codeName, string itemId, string displayNameTr, DisasterItemCategory cat,
                float weightKg, int score, bool collectible, bool securable, ColliderKind col)
            {
                CodeName = codeName;
                ItemId = itemId;
                DisplayNameTr = displayNameTr;
                Category = cat;
                WeightKg = weightKg;
                Score = score;
                IsCollectible = collectible;
                IsSecurableFixture = securable;
                Collider = col;
            }
        }

        private static readonly PaletteDef[] Palette = new[]
        {
            new PaletteDef("Red",         0.82f, 0.12f, 0.10f, 0.10f, 0.35f),
            new PaletteDef("DarkRed",     0.45f, 0.06f, 0.05f, 0.00f, 0.55f),
            new PaletteDef("White",       0.92f, 0.92f, 0.90f, 0.05f, 0.40f),
            new PaletteDef("OffWhite",    0.84f, 0.82f, 0.77f, 0.00f, 0.65f),
            new PaletteDef("DarkMetal",   0.16f, 0.17f, 0.19f, 0.75f, 0.35f),
            new PaletteDef("SilverMetal", 0.72f, 0.75f, 0.78f, 0.85f, 0.25f),
            new PaletteDef("Brass",       0.85f, 0.64f, 0.18f, 0.80f, 0.30f),
            new PaletteDef("Copper",      0.78f, 0.42f, 0.20f, 0.75f, 0.30f),
            new PaletteDef("Yellow",      0.95f, 0.76f, 0.12f, 0.10f, 0.40f),
            new PaletteDef("Orange",      0.92f, 0.42f, 0.10f, 0.10f, 0.45f),
            new PaletteDef("Blue",        0.12f, 0.45f, 0.85f, 0.10f, 0.35f),
            new PaletteDef("WaterBlue",   0.28f, 0.68f, 0.95f, 0.00f, 0.15f),
            new PaletteDef("NavyBlue",    0.10f, 0.18f, 0.36f, 0.00f, 0.75f),
            new PaletteDef("Green",       0.18f, 0.62f, 0.24f, 0.05f, 0.45f),
            new PaletteDef("OliveGreen",  0.25f, 0.35f, 0.20f, 0.00f, 0.75f),
            new PaletteDef("NeonGreen",   0.35f, 0.90f, 0.15f, 0.20f, 0.30f),
            new PaletteDef("BrownWood",   0.38f, 0.22f, 0.11f, 0.00f, 0.65f),
            new PaletteDef("ChocBrown",   0.26f, 0.14f, 0.07f, 0.05f, 0.50f),
            new PaletteDef("FabricBeige", 0.76f, 0.68f, 0.55f, 0.00f, 0.85f),
            new PaletteDef("FabricBlue",  0.20f, 0.36f, 0.58f, 0.00f, 0.85f),
            new PaletteDef("RubberBlack", 0.08f, 0.08f, 0.09f, 0.00f, 0.75f),
            new PaletteDef("ScreenBlack", 0.03f, 0.04f, 0.06f, 0.20f, 0.15f),
            new PaletteDef("GlowWarm",    1.00f, 0.92f, 0.65f, 0.00f, 0.10f, true),
            new PaletteDef("GlowGreen",   0.20f, 0.95f, 0.35f, 0.00f, 0.15f, true),
            new PaletteDef("RugBurgundy", 0.52f, 0.12f, 0.16f, 0.00f, 0.88f),
            new PaletteDef("RugGold",     0.82f, 0.66f, 0.28f, 0.00f, 0.82f),
        };

        private static readonly AssetSpec[] Specs = new[]
        {
            new AssetSpec("FireExtinguisher",  "fire_extinguisher",  "Yangın Tüpü",         DisasterItemCategory.FireSafety,       4.50f, 35, true,  true,  ColliderKind.Capsule),
            new AssetSpec("Blanket",           "blanket",            "Battaniye",           DisasterItemCategory.SurvivalGear,     1.20f, 20, true,  false, ColliderKind.Box),
            new AssetSpec("SnackBar",          "snack_bar",          "Snack Bar",           DisasterItemCategory.FoodAndWater,     0.08f, 10, true,  false, ColliderKind.Box),
            new AssetSpec("Apple",             "apple",              "Elma",                DisasterItemCategory.FoodAndWater,     0.15f,  8, true,  false, ColliderKind.Capsule),
            new AssetSpec("CannedFood",        "canned_food",        "Konserve",            DisasterItemCategory.FoodAndWater,     0.40f, 18, true,  false, ColliderKind.Capsule),
            new AssetSpec("WaterBottle",       "water_bottle",       "Su Şişesi",           DisasterItemCategory.FoodAndWater,     0.55f, 25, true,  false, ColliderKind.Capsule),
            new AssetSpec("EnergyDrink",       "energy_drink",       "Enerji İçeceği",      DisasterItemCategory.FoodAndWater,     0.35f, 12, true,  false, ColliderKind.Capsule),
            new AssetSpec("PowerBattery",      "power_battery",      "Batarya (Güç Kaynağı)",DisasterItemCategory.PowerAndLight,   2.80f, 30, true,  false, ColliderKind.Box),
            new AssetSpec("SmallBattery",      "small_battery",      "Küçük Pil",           DisasterItemCategory.PowerAndLight,    0.05f, 12, true,  false, ColliderKind.Capsule),
            new AssetSpec("WalkieTalkie",      "walkie_talkie",      "Telsiz",              DisasterItemCategory.Communication,    0.35f, 30, true,  false, ColliderKind.Box),
            new AssetSpec("Television",        "television",         "Televizyon",          DisasterItemCategory.HouseholdFixture, 8.50f, 25, false, true,  ColliderKind.Box),
            new AssetSpec("Rug",               "rug",                "Halı",                DisasterItemCategory.HouseholdFixture, 3.00f,  5, false, false, ColliderKind.Box),
            new AssetSpec("FoldedClothes",     "folded_clothes",     "Kıyafet",             DisasterItemCategory.SurvivalGear,     0.80f, 15, true,  false, ColliderKind.Box),
            new AssetSpec("EmergencyBackpack", "emergency_backpack", "Afet Çantası",        DisasterItemCategory.SurvivalGear,     1.10f, 50, true,  false, ColliderKind.Box),
            new AssetSpec("FirstAidKit",       "first_aid_kit",      "İlk Yardım Kiti",     DisasterItemCategory.MedicalAndHygiene,1.00f, 40, true,  false, ColliderKind.Box),
            new AssetSpec("Flashlight",        "flashlight",         "El Feneri",           DisasterItemCategory.PowerAndLight,    0.30f, 25, true,  false, ColliderKind.Box),
            new AssetSpec("EmergencyRadio",    "emergency_radio",    "Acil Durum Radyosu",  DisasterItemCategory.Communication,    0.65f, 30, true,  false, ColliderKind.Box),
            new AssetSpec("AirConditioner",    "air_conditioner",    "Klima",               DisasterItemCategory.HouseholdFixture,12.00f, 25, false, true,  ColliderKind.Box),
        };

        [MenuItem("Tools/Yarkin Assets/Build All 18 Prefabs, Icons & Demo Scene")]
        public static void BuildAll()
        {
            EnsureFolders();
            AssetDatabase.Refresh();

            var materials = CreateOrUpdateMaterials();
            ConfigureModelImporters(materials);
            AssetDatabase.Refresh();

            var prefabs = new List<GameObject>();
            foreach (var spec in Specs)
            {
                var prefab = CreateOrUpdatePrefab(spec, materials);
                if (prefab != null)
                {
                    var iconSprite = RenderAndSaveIcon(spec, prefab);
                    var itemComp = prefab.GetComponent<DisasterPrepItem>();
                    if (itemComp != null && iconSprite != null)
                    {
                        itemComp.inventoryIcon = iconSprite;
                        EditorUtility.SetDirty(prefab);
                    }
                    prefabs.Add(prefab);
                }
            }

            AssetDatabase.SaveAssets();
            BuildDemoShowcaseScene(prefabs, materials);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[YarkinAssetPipelineBuilder] Successfully built all 18 materials, prefabs, icons, and Demo_YarkinAssets scene!");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(MaterialsDir);
            Directory.CreateDirectory(IconsDir);
            Directory.CreateDirectory(PrefabsDir);
            Directory.CreateDirectory(ScenesDir);
        }

        private static Dictionary<string, Material> CreateOrUpdateMaterials()
        {
            var dict = new Dictionary<string, Material>();
            Shader stdShader = Shader.Find("Standard");
            foreach (var p in Palette)
            {
                string matName = "M_" + p.Key;
                string matPath = $"{MaterialsDir}/{matName}.mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(stdShader);
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                mat.shader = stdShader;
                mat.color = p.Color;
                mat.SetFloat("_Metallic", p.Metallic);
                mat.SetFloat("_Glossiness", p.Smoothness);
                if (p.Emissive)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", p.Color * 0.65f);
                }
                EditorUtility.SetDirty(mat);
                dict[matName] = mat;
            }
            AssetDatabase.SaveAssets();
            return dict;
        }

        private static void ConfigureModelImporters(Dictionary<string, Material> materials)
        {
            foreach (var spec in Specs)
            {
                string fbxPath = $"{MeshesDir}/SM_{spec.CodeName}.fbx";
                ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"Missing FBX at {fbxPath}");
                    continue;
                }

                importer.globalScale = 1.0f;
                importer.useFileUnits = true;
                importer.bakeAxisConversion = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.animationType = ModelImporterAnimationType.None;
                importer.importAnimation = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;

                // Remap embedded FBX material names to our shared Built-in Standard materials
                var map = importer.GetExternalObjectMap();
                foreach (var kv in materials)
                {
                    var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), kv.Key);
                    importer.AddRemap(id, kv.Value);
                }

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }

        private static GameObject CreateOrUpdatePrefab(AssetSpec spec, Dictionary<string, Material> materials)
        {
            string fbxPath = $"{MeshesDir}/SM_{spec.CodeName}.fbx";
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (modelAsset == null)
            {
                Debug.LogError($"Could not load model at {fbxPath}");
                return null;
            }

            GameObject root = new GameObject($"PF_{spec.CodeName}");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            // Ensure shared materials are applied on all renderers
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
            bool hasBounds = false;

            foreach (var r in renderers)
            {
                var sharedMats = r.sharedMaterials;
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    if (sharedMats[i] != null)
                    {
                        string cleanName = sharedMats[i].name.Split(' ')[0];
                        if (materials.TryGetValue(cleanName, out Material mapped))
                        {
                            sharedMats[i] = mapped;
                        }
                    }
                }
                r.sharedMaterials = sharedMats;

                if (!hasBounds)
                {
                    combinedBounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(r.bounds);
                }
            }

            if (!hasBounds)
            {
                combinedBounds = new Bounds(new Vector3(0f, 0.1f, 0f), new Vector3(0.2f, 0.2f, 0.2f));
            }

            // Add primitive collider on root
            if (spec.Collider == ColliderKind.Capsule)
            {
                var cap = root.AddComponent<CapsuleCollider>();
                cap.center = combinedBounds.center;
                cap.height = combinedBounds.size.y;
                cap.radius = Mathf.Max(combinedBounds.extents.x, combinedBounds.extents.z);
                cap.direction = 1; // Y-axis
            }
            else
            {
                var box = root.AddComponent<BoxCollider>();
                box.center = combinedBounds.center;
                box.size = new Vector3(
                    Mathf.Max(0.02f, combinedBounds.size.x),
                    Mathf.Max(0.015f, combinedBounds.size.y),
                    Mathf.Max(0.02f, combinedBounds.size.z)
                );
            }

            // Add DisasterPrepItem metadata component
            var item = root.AddComponent<DisasterPrepItem>();
            item.itemId = spec.ItemId;
            item.displayNameTr = spec.DisplayNameTr;
            item.category = spec.Category;
            item.weightKg = spec.WeightKg;
            item.preparednessScore = spec.Score;
            item.isCollectible = spec.IsCollectible;
            item.isSecurableFixture = spec.IsSecurableFixture;

            string prefabPath = $"{PrefabsDir}/PF_{spec.CodeName}.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static Sprite RenderAndSaveIcon(AssetSpec spec, GameObject prefab)
        {
            const int res = 256;
            string iconPath = $"{IconsDir}/T_Icon_{spec.CodeName}.png";

            // Create temporary isolated rig far away
            Vector3 stageOrigin = new Vector3(500f, 500f, 500f);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = stageOrigin;
            instance.transform.rotation = Quaternion.Euler(0f, -28f, 0f);

            Bounds b = CalculateBounds(instance);
            float maxExtent = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z));
            if (maxExtent < 0.01f) maxExtent = 0.1f;

            GameObject camGo = new GameObject("IconRenderCam");
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 0f); // Transparent alpha
            cam.orthographic = true;
            cam.orthographicSize = maxExtent * 1.25f;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 25f;

            Vector3 viewDir = new Vector3(-0.42f, 0.44f, 0.78f).normalized;
            camGo.transform.position = b.center + viewDir * 5f;
            camGo.transform.LookAt(b.center);

            GameObject keyLightGo = new GameObject("IconKeyLight");
            Light keyLight = keyLightGo.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.25f;
            keyLight.color = new Color(1f, 0.97f, 0.92f);
            keyLightGo.transform.rotation = Quaternion.Euler(45f, 155f, 0f);

            RenderTexture rt = new RenderTexture(res, res, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, res, res), 0, 0);
            tex.Apply();

            byte[] pngBytes = tex.EncodeToPNG();
            File.WriteAllBytes(iconPath, pngBytes);

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(keyLightGo);
            UnityEngine.Object.DestroyImmediate(camGo);
            UnityEngine.Object.DestroyImmediate(instance);

            AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);
            TextureImporter ti = AssetImporter.GetAtPath(iconPath) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaIsTransparency = true;
                ti.mipmapEnabled = false;
                ti.maxTextureSize = 256;
                ti.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        }

        private static Bounds CalculateBounds(GameObject go)
        {
            Renderer[] rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one * 0.2f);
            Bounds b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }

        private static void BuildDemoShowcaseScene(List<GameObject> prefabs, Dictionary<string, Material> materials)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Floor plane
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "ShowcaseFloor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(1.5f, 1f, 1.2f);
            if (materials.TryGetValue("M_DarkMetal", out Material floorMat))
            {
                floor.GetComponent<Renderer>().sharedMaterial = floorMat;
            }

            // 1.8m human scale reference capsule
            GameObject humanRef = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            humanRef.name = "HumanScaleRef_1.8m";
            humanRef.transform.position = new Vector3(-4.5f, 0.9f, 0f);
            humanRef.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);

            // Grid layout for the 18 prefabs (6 columns x 3 rows)
            int cols = 6;
            float spacingX = 1.45f;
            float spacingZ = 1.65f;
            Vector3 startPos = new Vector3(-3.0f, 0f, -1.65f);

            GameObject container = new GameObject("DisasterPrepAssets_18");
            for (int i = 0; i < prefabs.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;
                Vector3 pos = startPos + new Vector3(col * spacingX, 0f, row * spacingZ);

                // Small pedestal for tiny items so they are easy to inspect
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[i]);
                instance.transform.SetParent(container.transform, false);
                instance.transform.position = pos;
            }

            // Position Main Camera for isometric/overview inspection
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0.5f, 4.2f, -4.8f);
                mainCam.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            }

            string scenePath = $"{ScenesDir}/Demo_YarkinAssets.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
#endif
