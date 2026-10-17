#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using YarkinAssets.Runtime;

namespace YarkinAssets.Editor
{
    [InitializeOnLoad]
    public static class YarkinAssetPipelineBuilder
    {
        static YarkinAssetPipelineBuilder()
        {
            EditorApplication.delayCall += AutoBuildIfNeeded;
        }

        private static void AutoBuildIfNeeded()
        {
            // Automatically trigger full 33-asset build if the new mechanical animation controllers & previews are not built yet
            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{AnimationsDir}/AC_WaterValve.controller") == null &&
                AssetDatabase.LoadAssetAtPath<GameObject>($"{MeshesDir}/SM_WaterValve.fbx") != null)
            {
                Debug.Log("[YarkinAssetPipelineBuilder] Auto-building all 33 prefabs with physically-based Valve/Faucet/Breaker/Door animations, AnimatorControllers, icons & isometric scene...");
                BuildAll();
            }
        }
        private const string RootDir = "Assets/_YarkinAssets";
        private const string MeshesDir = RootDir + "/Art/Meshes";
        private const string MaterialsDir = RootDir + "/Art/Materials";
        private const string AnimationsDir = RootDir + "/Art/Animations";
        private const string IconsDir = RootDir + "/Art/Icons";
        private const string PreviewsDir = RootDir + "/Art/Icons/Previews";
        private const string PackedTexDir = RootDir + "/Art/Textures/Packed";
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
            public bool IsUtilityValve;
            public SecondaryHazardType SecondaryHazard;
            public ColliderKind Collider;

            public AssetSpec(string codeName, string itemId, string displayNameTr, DisasterItemCategory cat,
                float weightKg, int score, bool collectible, bool securable, bool utilityValve,
                SecondaryHazardType hazard, ColliderKind col)
            {
                CodeName = codeName;
                ItemId = itemId;
                DisplayNameTr = displayNameTr;
                Category = cat;
                WeightKg = weightKg;
                Score = score;
                IsCollectible = collectible;
                IsSecurableFixture = securable;
                IsUtilityValve = utilityValve;
                SecondaryHazard = hazard;
                Collider = col;
            }
        }

        // Redesigned Semantic 4-Tier Disaster Color Palette
        private static readonly PaletteDef[] Palette = new[]
        {
            new PaletteDef("SafetyOrange", 1.00f, 0.34f, 0.13f, 0.05f, 0.45f),
            new PaletteDef("Red",          0.85f, 0.16f, 0.16f, 0.08f, 0.40f),
            new PaletteDef("DarkRed",      0.52f, 0.08f, 0.08f, 0.05f, 0.55f),
            new PaletteDef("Yellow",       1.00f, 0.72f, 0.02f, 0.10f, 0.40f),
            new PaletteDef("Orange",       0.96f, 0.46f, 0.05f, 0.08f, 0.42f),

            new PaletteDef("WaterBlue",    0.02f, 0.70f, 0.86f, 0.00f, 0.18f),
            new PaletteDef("Blue",         0.10f, 0.44f, 0.82f, 0.08f, 0.35f),
            new PaletteDef("NavyBlue",     0.14f, 0.24f, 0.40f, 0.05f, 0.65f),
            new PaletteDef("Green",        0.16f, 0.72f, 0.48f, 0.05f, 0.40f),
            new PaletteDef("OliveGreen",   0.28f, 0.38f, 0.22f, 0.00f, 0.72f),
            new PaletteDef("NeonGreen",    0.25f, 0.92f, 0.20f, 0.15f, 0.28f),

            new PaletteDef("RubberBlack",  0.17f, 0.19f, 0.23f, 0.15f, 0.62f), // Tactical Slate (No black hole!)
            new PaletteDef("DarkMetal",    0.26f, 0.29f, 0.34f, 0.78f, 0.35f),
            new PaletteDef("SilverMetal",  0.80f, 0.84f, 0.87f, 0.88f, 0.24f),
            new PaletteDef("Brass",        0.88f, 0.65f, 0.24f, 0.82f, 0.28f),
            new PaletteDef("Copper",       0.82f, 0.45f, 0.22f, 0.78f, 0.30f),
            new PaletteDef("White",        0.96f, 0.97f, 0.98f, 0.05f, 0.38f),
            new PaletteDef("OffWhite",     0.88f, 0.86f, 0.81f, 0.00f, 0.60f),
            new PaletteDef("ScreenBlack",  0.08f, 0.10f, 0.14f, 0.25f, 0.15f),

            new PaletteDef("BrownWood",    0.42f, 0.26f, 0.15f, 0.00f, 0.58f),
            new PaletteDef("ChocBrown",    0.30f, 0.17f, 0.09f, 0.05f, 0.48f),
            new PaletteDef("FabricBeige",  0.78f, 0.71f, 0.58f, 0.00f, 0.80f),
            new PaletteDef("FabricBlue",   0.24f, 0.40f, 0.60f, 0.00f, 0.78f),
            new PaletteDef("RugBurgundy",  0.55f, 0.14f, 0.20f, 0.00f, 0.82f),
            new PaletteDef("RugGold",      0.86f, 0.68f, 0.28f, 0.00f, 0.75f),

            new PaletteDef("GlowWarm",     1.00f, 0.90f, 0.55f, 0.00f, 0.10f, true),
            new PaletteDef("GlowGreen",    0.05f, 0.95f, 0.48f, 0.00f, 0.12f, true),
            new PaletteDef("GlassCyan",    0.55f, 0.88f, 0.96f, 0.10f, 0.10f),
        };

        // Full 33-Asset Master Catalog (18 Upgraded Core + 15 IMO Expansion incl. Hinged FireCabinet)
        private static readonly AssetSpec[] Specs = new[]
        {
            // --- PART A: ORIGINAL 18 ASSETS ---
            // Note: FireExtinguisher is Collectible (taken from inside FireCabinet), NOT directly wall-bolted itself!
            new AssetSpec("FireExtinguisher",  "fire_extinguisher",  "Yangın Tüpü",          DisasterItemCategory.FireSafety,          4.50f, 35, true,  false, false, SecondaryHazardType.None,             ColliderKind.Capsule),
            new AssetSpec("Blanket",           "blanket",            "Battaniye",            DisasterItemCategory.SurvivalGear,        1.20f, 20, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("SnackBar",          "snack_bar",          "Snack Bar",            DisasterItemCategory.FoodAndWater,        0.08f, 10, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("Apple",             "apple",              "Elma",                 DisasterItemCategory.FoodAndWater,        0.15f,  8, true,  false, false, SecondaryHazardType.None,             ColliderKind.Capsule),
            new AssetSpec("CannedFood",        "canned_food",        "Konserve",             DisasterItemCategory.FoodAndWater,        0.40f, 18, true,  false, false, SecondaryHazardType.None,             ColliderKind.Capsule),
            new AssetSpec("WaterBottle",       "water_bottle",       "Su Şişesi",            DisasterItemCategory.FoodAndWater,        0.55f, 25, true,  false, false, SecondaryHazardType.None,             ColliderKind.Capsule),
            new AssetSpec("EnergyDrink",       "energy_drink",       "Enerji İçeceği",       DisasterItemCategory.FoodAndWater,        0.35f, 12, true,  false, false, SecondaryHazardType.None,             ColliderKind.Capsule),
            new AssetSpec("PowerBattery",      "power_battery",      "Batarya İstasyonu",    DisasterItemCategory.PowerAndLight,       2.80f, 30, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("SmallBattery",      "small_battery",      "İkili Kalem Pil",      DisasterItemCategory.PowerAndLight,       0.10f, 15, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("WalkieTalkie",      "walkie_talkie",      "El Telsizi",           DisasterItemCategory.Communication,       0.35f, 30, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("Television",        "television",         "Televizyon",           DisasterItemCategory.HouseholdFixture,    8.50f, 25, false, true,  false, SecondaryHazardType.FallingTrauma,    ColliderKind.Box),
            new AssetSpec("Rug",               "rug",                "Halı",                 DisasterItemCategory.HouseholdFixture,    3.00f,  5, false, false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("FoldedClothes",     "folded_clothes",     "Kıyafet",              DisasterItemCategory.SurvivalGear,        0.80f, 15, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("EmergencyBackpack", "emergency_backpack", "Afet Çantası",         DisasterItemCategory.SurvivalGear,        1.10f, 50, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("FirstAidKit",       "first_aid_kit",      "İlk Yardım Kiti",      DisasterItemCategory.MedicalAndHygiene,   1.00f, 40, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("Flashlight",        "flashlight",         "El Feneri",            DisasterItemCategory.PowerAndLight,       0.30f, 25, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("EmergencyRadio",    "emergency_radio",    "Acil Durum Radyosu",   DisasterItemCategory.Communication,       0.65f, 30, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("AirConditioner",    "air_conditioner",    "Duvar Kliması",        DisasterItemCategory.HouseholdFixture,   12.00f, 30, false, true,  false, SecondaryHazardType.FallingTrauma,    ColliderKind.Box),

            // --- PART B: 13 IMO DISASTER EXPANSION ASSETS (31 Total Assets) ---
            new AssetSpec("GasValve",          "gas_valve",          "Doğalgaz Ana Vanası",  DisasterItemCategory.UtilityValve,        0.00f, 45, false, false, true,  SecondaryHazardType.FireWave,         ColliderKind.Box),
            new AssetSpec("WaterValve",        "water_valve",        "Su Kesme Vanası",      DisasterItemCategory.UtilityValve,        0.00f, 40, false, false, true,  SecondaryHazardType.FloodWave,        ColliderKind.Box),
            new AssetSpec("BreakerBox",        "breaker_box",        "Elektrik Sigorta Panosu",DisasterItemCategory.UtilityValve,      0.00f, 40, false, false, true,  SecondaryHazardType.ElectricalShock,  ColliderKind.Box),
            new AssetSpec("Bookcase",          "bookcase",           "Büyük Ahşap Kitaplık (2 Bölmeli)", DisasterItemCategory.HouseholdFixture, 35.00f, 35, false, true, false, SecondaryHazardType.CorridorBlockage, ColliderKind.Box),
            new AssetSpec("Refrigerator",      "refrigerator",       "Mutfak Buzdolabı",     DisasterItemCategory.HouseholdFixture,   55.00f, 35, false, true,  false, SecondaryHazardType.CorridorBlockage, ColliderKind.Box),
            new AssetSpec("WaterHeater",       "water_heater",       "Banyo Termosifonu",    DisasterItemCategory.HouseholdFixture,   28.00f, 40, false, true,  false, SecondaryHazardType.FloodWave,        ColliderKind.Capsule),
            new AssetSpec("SturdyTable",       "sturdy_table",       "Sağlam Masa (Yaşam Üçgeni)",DisasterItemCategory.HouseholdFixture,22.0f,30, false, true,  false, SecondaryHazardType.FallingTrauma,    ColliderKind.Box),
            new AssetSpec("EmergencyWhistle",  "emergency_whistle",  "Deprem Düdüğü",        DisasterItemCategory.Communication,       0.03f, 35, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("WorkGloves",        "work_gloves",        "Ağır Hizmet Eldiveni", DisasterItemCategory.SurvivalGear,        0.25f, 20, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("MultiTool",         "multi_tool",         "Çok Amaçlı Pense/Çakı",DisasterItemCategory.SurvivalGear,        0.22f, 25, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("ShatteredGlass",    "shattered_glass",    "Kırık Cam Tehlikesi",  DisasterItemCategory.EnvironmentalHazard, 0.00f,  0, false, false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("RubblePile",        "rubble_pile",        "Dökülmüş Moloz Yığını",DisasterItemCategory.EnvironmentalHazard, 0.00f,  0, false, false, false, SecondaryHazardType.CorridorBlockage, ColliderKind.Box),
            new AssetSpec("FireCabinet",       "fire_cabinet",       "Yangın Tüpü Dolabı",   DisasterItemCategory.FireSafety,         14.00f, 35, false, true,  false, SecondaryHazardType.FireWave,         ColliderKind.Box),
        };

        [MenuItem("Tools/Yarkin Assets/Build All 33 Prefabs, Multi-State Brackets, Icons & Isometric Scene")]
        [MenuItem("Tools/Yarkin Assets/Build All 31 Prefabs, Multi-State Brackets, Icons & Isometric Scene")]
        [MenuItem("Tools/Yarkin Assets/Build All 32 Prefabs, Multi-State Brackets, Icons & Isometric Scene")]
        [MenuItem("Tools/Yarkin Assets/Build All 18 Prefabs, Icons & Demo Scene")]
        public static void BuildAll()
        {
            EnsureFolders();
            ConfigureQualityAndShadowSettings();
            ConfigureSwatchTextureImporter();
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
            Debug.Log($"[YarkinAssetPipelineBuilder] Successfully built {prefabs.Count} prefabs (with custom non-obstructing 3D brackets, hinged FireCabinet & Valve states), {Palette.Length} semantic materials, 33 icons, and True Isometric Demo_YarkinAssets scene!");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(MaterialsDir);
            Directory.CreateDirectory(AnimationsDir);
            Directory.CreateDirectory(IconsDir);
            Directory.CreateDirectory(PreviewsDir);
            Directory.CreateDirectory(PackedTexDir);
            Directory.CreateDirectory(PrefabsDir);
            Directory.CreateDirectory(ScenesDir);
        }

        private static void ConfigureQualityAndShadowSettings()
        {
            // Crisp isometric contact shadows: tighten shadowDistance from 150m to 24m and enable 4x MSAA
            QualitySettings.shadowDistance = 24.0f;
            QualitySettings.shadowCascades = 4;
            QualitySettings.antiAliasing = 4;
        }

        private static void ConfigureSwatchTextureImporter()
        {
            string swatchPath = $"{PackedTexDir}/T_DisasterColorSwatch_256.png";
            TextureImporter ti = AssetImporter.GetAtPath(swatchPath) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Default;
                ti.sRGBTexture = true;
                ti.filterMode = FilterMode.Point;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.mipmapEnabled = false;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.SaveAndReimport();
            }
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
                mat.enableInstancing = true;
                mat.SetFloat("_Metallic", p.Metallic);
                mat.SetFloat("_Glossiness", p.Smoothness);
                if (p.Emissive)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", p.Color * 0.75f);
                }
                EditorUtility.SetDirty(mat);
                dict[matName] = mat;
            }

            // Also create a shared Master Swatch Atlas Material for single-draw-call batching
            string atlasMatPath = $"{MaterialsDir}/M_MasterColorAtlas.mat";
            Material atlasMat = AssetDatabase.LoadAssetAtPath<Material>(atlasMatPath);
            if (atlasMat == null)
            {
                atlasMat = new Material(stdShader);
                AssetDatabase.CreateAsset(atlasMat, atlasMatPath);
            }
            Texture2D swatchTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PackedTexDir}/T_DisasterColorSwatch_256.png");
            if (swatchTex != null)
            {
                atlasMat.mainTexture = swatchTex;
            }
            atlasMat.color = Color.white;
            atlasMat.SetFloat("_Metallic", 0.15f);
            atlasMat.SetFloat("_Glossiness", 0.52f);
            atlasMat.enableInstancing = true;
            EditorUtility.SetDirty(atlasMat);

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
                importer.importNormals = ModelImporterNormals.Calculate;
                importer.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
                importer.normalSmoothingAngle = 65f;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;

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

            // 1. Tight physical collider on Root
            if (spec.Collider == ColliderKind.Capsule)
            {
                var cap = root.AddComponent<CapsuleCollider>();
                cap.center = combinedBounds.center;
                cap.height = combinedBounds.size.y;
                cap.radius = Mathf.Max(combinedBounds.extents.x, combinedBounds.extents.z);
                cap.direction = 1;
            }
            else
            {
                var box = root.AddComponent<BoxCollider>();
                box.center = combinedBounds.center;
                box.size = new Vector3(
                    Mathf.Max(0.025f, combinedBounds.size.x),
                    Mathf.Max(0.020f, combinedBounds.size.y),
                    Mathf.Max(0.025f, combinedBounds.size.z)
                );
            }

            // 2. Child Interaction Trigger Sphere so isometric clicking/proximity never conflicts with physics
            GameObject triggerChild = new GameObject("InteractionTrigger");
            triggerChild.transform.SetParent(root.transform, false);
            triggerChild.transform.localPosition = combinedBounds.center;
            SphereCollider triggerSphere = triggerChild.AddComponent<SphereCollider>();
            triggerSphere.isTrigger = true;
            triggerSphere.radius = Mathf.Max(0.35f, combinedBounds.extents.magnitude * 1.15f);

            // 3. If Securable Fixture -> Add custom, non-obstructing 3D Seismic Brackets (Default Inactive, toggled when player secures!)
            GameObject bracketRoot = null;
            if (spec.IsSecurableFixture)
            {
                bracketRoot = BuildSteelLBracketVisual(spec.CodeName, root.transform, combinedBounds, materials);
                bracketRoot.SetActive(false); // Hidden until player secures it!
            }

            // 4. Add DisasterPrepItem metadata & state controller
            var item = root.AddComponent<DisasterPrepItem>();
            item.itemId = spec.ItemId;
            item.displayNameTr = spec.DisplayNameTr;
            item.category = spec.Category;
            item.weightKg = spec.WeightKg;
            item.preparednessScore = spec.Score;
            item.isCollectible = spec.IsCollectible;
            item.isSecurableFixture = spec.IsSecurableFixture;
            item.isUtilityValve = spec.IsUtilityValve;
            item.triggersSecondaryHazard = spec.SecondaryHazard;
            item.visualRoot = visual.transform;
            item.securedBracketObject = bracketRoot;

            // 5. Wire Mechanical Pivots & Multi-Part Animation Transforms
            switch (spec.CodeName)
            {
                case "GasValve":
                    item.mechanismKind = ValveMechanismKind.QuarterTurnBallValve;
                    item.valveAnimationDuration = 0.58f;
                    item.valveHandleTransform = FindChildByKeyword(visual.transform, "ValveHandle_Pivot");
                    item.secondaryMovingPart = FindChildByKeyword(visual.transform, "GaugeNeedle_Pivot");
                    break;

                case "WaterValve":
                    item.mechanismKind = ValveMechanismKind.RisingStemWaterValve;
                    item.valveAnimationDuration = 0.92f;
                    item.valveHandleTransform = FindChildByKeyword(visual.transform, "ValveHandle_Pivot");
                    item.secondaryMovingPart = FindChildByKeyword(visual.transform, "FaucetTap_Pivot");
                    item.tertiaryMovingPart = FindChildByKeyword(visual.transform, "MeterTurbine_Pivot");
                    item.quaternaryMovingPart = FindChildByKeyword(visual.transform, "WaterDrop_Pivot");
                    break;

                case "BreakerBox":
                    item.mechanismKind = ValveMechanismKind.CircuitBreakerPanel;
                    item.valveAnimationDuration = 0.42f;
                    item.valveHandleTransform = FindChildByKeyword(visual.transform, "MainBreaker_Pivot");
                    item.quaternaryMovingPart = FindChildByKeyword(visual.transform, "StatusLED_Pivot");
                    break;

                case "WaterHeater":
                    item.mechanismKind = ValveMechanismKind.WaterHeaterIsolation;
                    item.valveAnimationDuration = 0.65f;
                    item.valveHandleTransform = FindChildByKeyword(visual.transform, "ValveHandle_Pivot");
                    item.secondaryMovingPart = FindChildByKeyword(visual.transform, "ThermostatKnob_Pivot");
                    item.quaternaryMovingPart = FindChildByKeyword(visual.transform, "StatusLED_Pivot");
                    break;

                case "AirConditioner":
                    item.mechanismKind = ValveMechanismKind.AirConditionerLouver;
                    item.valveAnimationDuration = 0.60f;
                    item.valveHandleTransform = FindChildByKeyword(visual.transform, "VentFlap_Pivot");
                    item.quaternaryMovingPart = FindChildByKeyword(visual.transform, "StatusLED_Pivot");
                    break;

                case "Refrigerator":
                    item.isContainerCabinet = true;
                    item.doorOpenAngleY = -95f;
                    item.doorAnimationDuration = 0.62f;
                    item.cabinetDoorHinge = FindChildByKeyword(visual.transform, "CabinetDoor_Hinge");
                    item.secondaryMovingPart = FindChildByKeyword(visual.transform, "UpperDoor_Hinge");
                    break;

                case "FireCabinet":
                    item.isContainerCabinet = true;
                    item.doorOpenAngleY = 105f;
                    item.doorAnimationDuration = 0.54f;
                    item.cabinetDoorHinge = FindChildByKeyword(visual.transform, "CabinetDoor_Hinge");

                    // Create Interior Mount Slot and instantiate PF_FireExtinguisher inside the cabinet!
                    GameObject mountSlot = new GameObject("ExtinguisherMountSlot");
                    mountSlot.transform.SetParent(root.transform, false);
                    mountSlot.transform.localPosition = new Vector3(0.015f, 0.044f, 0.0f);
                    item.containedItemSlot = mountSlot.transform;

                    GameObject extPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/PF_FireExtinguisher.prefab");
                    if (extPrefab != null)
                    {
                        GameObject mountedExt = (GameObject)PrefabUtility.InstantiatePrefab(extPrefab);
                        mountedExt.name = "PF_FireExtinguisher_Mounted";
                        mountedExt.transform.SetParent(mountSlot.transform, false);
                        mountedExt.transform.localPosition = Vector3.zero;
                        mountedExt.transform.localRotation = Quaternion.identity;
                        mountedExt.transform.localScale = Vector3.one;
                        item.containedItem = mountedExt.GetComponent<DisasterPrepItem>();
                    }
                    break;
            }

            // Capture exact FBX rest positions of all mechanical pivots BEFORE evaluating initial poses
            item.CaptureMechanicalRestPose();

            // 6. Generate Physics-Baked Unity AnimationClips (.anim) & AnimatorController (.controller) for mechanical assets
            if (item.mechanismKind != ValveMechanismKind.None || item.isContainerCabinet)
            {
                RuntimeAnimatorController ac = BuildMechanicalAnimatorAndClips(spec.CodeName, item);
                if (ac != null)
                {
                    Animator anim = root.AddComponent<Animator>();
                    anim.runtimeAnimatorController = ac;
                    anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    item.mechanicalAnimator = anim;
                }
            }

            // Initialize default open/intact mechanical pose (e.g., BreakerBox main switch UP at -48 deg, GasGauge needle at -38 deg)
            item.SetSecuredOrClosed(false, instant: true);
            if (item.isContainerCabinet)
            {
                item.SetCabinetDoorOpen(false, instant: true);
            }

            // 7. Add IsometricItemMotion component
            var motion = root.AddComponent<IsometricItemMotion>();
            motion.targetVisual = visual.transform;
            motion.enableIdleBobbing = false;

            string prefabPath = $"{PrefabsDir}/PF_{spec.CodeName}.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static Transform FindChildByKeyword(Transform parent, string keyword)
        {
            if (parent == null) return null;
            Transform direct = parent.Find(keyword);
            if (direct != null) return direct;
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Contains(keyword))
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// Generates physics-baked Unity .anim clips and .controller state machines in Assets/_YarkinAssets/Art/Animations
        /// so animations can also be inspected/scrubbed in the Unity Animation & Animator windows.
        /// </summary>
        private static RuntimeAnimatorController BuildMechanicalAnimatorAndClips(string codeName, DisasterPrepItem item)
        {
            string openClipPath = $"{AnimationsDir}/Anim_{codeName}_Open.anim";
            string closedClipPath = $"{AnimationsDir}/Anim_{codeName}_Close.anim";
            string controllerPath = $"{AnimationsDir}/AC_{codeName}.controller";

            AnimationClip openClip = new AnimationClip { name = $"Anim_{codeName}_Open", frameRate = 60f };
            AnimationClip closeClip = new AnimationClip { name = $"Anim_{codeName}_Close", frameRate = 60f };

            const int samples = 30;
            float duration = item.isContainerCabinet && item.mechanismKind == ValveMechanismKind.None
                ? item.doorAnimationDuration
                : item.valveAnimationDuration;
            if (duration < 0.2f) duration = 0.55f;

            switch (item.mechanismKind)
            {
                case ValveMechanismKind.QuarterTurnBallValve:
                {
                    BakeRotationCurveZ(closeClip, "Visual/ValveHandle_Pivot", duration, samples, u => DisasterPrepItem.EvaluateBallValveCurve(u) * 90f);
                    BakeRotationCurveZ(openClip, "Visual/ValveHandle_Pivot", duration, samples, u => (1f - DisasterPrepItem.EvaluateBallValveCurve(u)) * 90f);

                    BakeRotationCurveZ(closeClip, "Visual/GaugeNeedle_Pivot", duration, samples, u =>
                    {
                        float uG = Mathf.Clamp01((u - 0.25f) / 0.75f);
                        return Mathf.LerpUnclamped(-38f, 112f, DisasterPrepItem.EvaluateUnderdampedGaugeCurve(uG));
                    });
                    BakeRotationCurveZ(openClip, "Visual/GaugeNeedle_Pivot", duration, samples, u =>
                    {
                        float uG = Mathf.Clamp01((u - 0.15f) / 0.85f);
                        return Mathf.LerpUnclamped(112f, -38f, DisasterPrepItem.EvaluateUnderdampedGaugeCurve(uG));
                    });
                    break;
                }

                case ValveMechanismKind.RisingStemWaterValve:
                {
                    Vector3 basePos = item.valveHandleTransform != null ? item.valveHandleTransform.localPosition : Vector3.zero;
                    BakeRotationCurveZ(closeClip, "Visual/ValveHandle_Pivot", duration, samples, u => DisasterPrepItem.EvaluateMultiTurnWheelCurve(u) * 540f);
                    BakePositionCurveZ(closeClip, "Visual/ValveHandle_Pivot", duration, samples, basePos, u => basePos.z - 0.015f * DisasterPrepItem.EvaluateMultiTurnWheelCurve(u));

                    BakeRotationCurveZ(openClip, "Visual/ValveHandle_Pivot", duration, samples, u => (1f - DisasterPrepItem.EvaluateMultiTurnWheelCurve(u)) * 540f);
                    BakePositionCurveZ(openClip, "Visual/ValveHandle_Pivot", duration, samples, basePos, u => basePos.z - 0.015f * (1f - DisasterPrepItem.EvaluateMultiTurnWheelCurve(u)));

                    BakeRotationCurveZ(closeClip, "Visual/FaucetTap_Pivot", duration, samples, u => DisasterPrepItem.EvaluateBallValveCurve(Mathf.Clamp01((u - 0.1f) / 0.85f)) * 90f);
                    BakeRotationCurveZ(openClip, "Visual/FaucetTap_Pivot", duration, samples, u => (1f - DisasterPrepItem.EvaluateBallValveCurve(Mathf.Clamp01((u - 0.1f) / 0.85f))) * 90f);
                    break;
                }

                case ValveMechanismKind.CircuitBreakerPanel:
                {
                    BakeRotationCurveX(closeClip, "Visual/MainBreaker_Pivot", duration, samples, u => Mathf.LerpUnclamped(-48f, 48f, DisasterPrepItem.EvaluateBreakerSnapCurve(u)));
                    BakeRotationCurveX(openClip, "Visual/MainBreaker_Pivot", duration, samples, u => Mathf.LerpUnclamped(48f, -48f, DisasterPrepItem.EvaluateBreakerSnapCurve(u)));
                    break;
                }

                case ValveMechanismKind.WaterHeaterIsolation:
                {
                    BakeRotationCurveZ(closeClip, "Visual/ValveHandle_Pivot", duration, samples, u => DisasterPrepItem.EvaluateBallValveCurve(u) * 90f);
                    BakeRotationCurveZ(closeClip, "Visual/ThermostatKnob_Pivot", duration, samples, u => Mathf.SmoothStep(0f, 1f, u) * -120f);

                    BakeRotationCurveZ(openClip, "Visual/ValveHandle_Pivot", duration, samples, u => (1f - DisasterPrepItem.EvaluateBallValveCurve(u)) * 90f);
                    BakeRotationCurveZ(openClip, "Visual/ThermostatKnob_Pivot", duration, samples, u => (1f - Mathf.SmoothStep(0f, 1f, u)) * -120f);
                    break;
                }

                case ValveMechanismKind.AirConditionerLouver:
                {
                    BakeRotationCurveX(closeClip, "Visual/VentFlap_Pivot", duration, samples, u => Mathf.SmoothStep(0f, 1f, u) * -48f);
                    BakeRotationCurveX(openClip, "Visual/VentFlap_Pivot", duration, samples, u => (1f - Mathf.SmoothStep(0f, 1f, u)) * -48f);
                    break;
                }

                default:
                {
                    if (item.isContainerCabinet)
                    {
                        float maxAngle = item.doorOpenAngleY;
                        // For cabinets, openClip opens the door (0 -> maxAngle) and closeClip closes the door (maxAngle -> 0)
                        BakeRotationCurveY(openClip, "Visual/CabinetDoor_Hinge", duration, samples, u => DisasterPrepItem.EvaluateDoorHingeCurve(u) * maxAngle);
                        BakeRotationCurveY(closeClip, "Visual/CabinetDoor_Hinge", duration, samples, u => (1f - DisasterPrepItem.EvaluateDoorHingeCurve(u)) * maxAngle);
                        if (codeName == "Refrigerator")
                        {
                            BakeRotationCurveY(openClip, "Visual/UpperDoor_Hinge", duration, samples, u => DisasterPrepItem.EvaluateDoorHingeCurve(Mathf.Clamp01((u - 0.12f) / 0.88f)) * (maxAngle * 0.88f));
                            BakeRotationCurveY(closeClip, "Visual/UpperDoor_Hinge", duration, samples, u => (1f - DisasterPrepItem.EvaluateDoorHingeCurve(Mathf.Clamp01((u - 0.12f) / 0.88f))) * (maxAngle * 0.88f));
                        }
                    }
                    break;
                }
            }

            CreateOrReplaceClipAsset(openClip, openClipPath);
            CreateOrReplaceClipAsset(closeClip, closedClipPath);

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("IsClosed", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDoorOpen", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            bool isDoorOnly = item.isContainerCabinet && item.mechanismKind == ValveMechanismKind.None;

            AnimatorState defaultState = sm.AddState(isDoorOnly ? "Door_Closed" : "Valve_Open_Default");
            defaultState.motion = isDoorOnly ? closeClip : openClip;
            sm.defaultState = defaultState;

            AnimatorState activeState = sm.AddState(isDoorOnly ? "Door_Open" : "Valve_Closed_Secured");
            activeState.motion = isDoorOnly ? openClip : closeClip;

            string paramName = isDoorOnly ? "IsDoorOpen" : "IsClosed";
            AnimatorStateTransition tToActive = defaultState.AddTransition(activeState);
            tToActive.hasExitTime = false;
            tToActive.duration = 0.05f;
            tToActive.AddCondition(AnimatorConditionMode.If, 0f, paramName);

            AnimatorStateTransition tToDefault = activeState.AddTransition(defaultState);
            tToDefault.hasExitTime = false;
            tToDefault.duration = 0.05f;
            tToDefault.AddCondition(AnimatorConditionMode.IfNot, 0f, paramName);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void CreateOrReplaceClipAsset(AnimationClip clip, string path)
        {
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(clip, path);
            }
        }

        private static void BakeRotationCurveX(AnimationClip clip, string relPath, float duration, int samples, Func<float, float> angleEval)
        {
            AnimationCurve cx = new AnimationCurve();
            AnimationCurve cy = new AnimationCurve();
            AnimationCurve cz = new AnimationCurve();
            for (int i = 0; i <= samples; i++)
            {
                float u = i / (float)samples;
                float t = u * duration;
                cx.AddKey(t, angleEval(u));
                cy.AddKey(t, 0f);
                cz.AddKey(t, 0f);
            }
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "localEulerAnglesRaw.x"), cx);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "localEulerAnglesRaw.y"), cy);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "localEulerAnglesRaw.z"), cz);
        }

        private static void BakeRotationCurveY(AnimationClip clip, string relPath, float duration, int samples, Func<float, float> angleEval)
        {
            AnimationCurve cx = new AnimationCurve();
            AnimationCurve cy = new AnimationCurve();
            AnimationCurve cz = new AnimationCurve();
            for (int i = 0; i <= samples; i++)
            {
                float u = i / (float)samples;
                float t = u * duration;
                cx.AddKey(t, 0f);
                cy.AddKey(t, angleEval(u));
                cz.AddKey(t, 0f);
            }
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "localEulerAnglesRaw.x"), cx);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "localEulerAnglesRaw.y"), cy);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "localEulerAnglesRaw.z"), cz);
        }

        private static void BakeRotationCurveZ(AnimationClip clip, string relPath, float duration, int samples, Func<float, float> angleEval)
        {
            AnimationCurve cx = new AnimationCurve();
            AnimationCurve cy = new AnimationCurve();
            AnimationCurve cz = new AnimationCurve();
            for (int i = 0; i <= samples; i++)
            {
                float u = i / (float)samples;
                float t = u * duration;
                cx.AddKey(t, 0f);
                cy.AddKey(t, 0f);
                cz.AddKey(t, angleEval(u));
            }
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "localEulerAnglesRaw.x"), cx);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "localEulerAnglesRaw.y"), cy);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "localEulerAnglesRaw.z"), cz);
        }

        private static void BakePositionCurveZ(AnimationClip clip, string relPath, float duration, int samples, Vector3 basePos, Func<float, float> zEval)
        {
            AnimationCurve cx = new AnimationCurve();
            AnimationCurve cy = new AnimationCurve();
            AnimationCurve cz = new AnimationCurve();
            for (int i = 0; i <= samples; i++)
            {
                float u = i / (float)samples;
                float t = u * duration;
                cx.AddKey(t, basePos.x);
                cy.AddKey(t, basePos.y);
                cz.AddKey(t, zEval(u));
            }
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "m_LocalPosition.x"), cx);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "m_LocalPosition.y"), cy);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(relPath, typeof(Transform), "m_LocalPosition.z"), cz);
        }

        private static GameObject AddBracketPart(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat, Vector3 eulerRot = default)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(eulerRot);
            go.transform.localScale = scale;
            if (mat != null)
            {
                go.GetComponent<Renderer>().sharedMaterial = mat;
            }
            return go;
        }

        /// <summary>
        /// Builds asset-specific, physically logical seismic securing hardware that NEVER obstructs functional fronts
        /// (never crosses TV screen, Bookcase open shelves, Refrigerator doors, AC vents/LED, WaterHeater dial, or FireCabinet hinged door).
        /// Note: In Unity local space of imported FBX models, +Z is FRONT, -Z is BACK (Wall), +Y is UP, and ±X is LEFT/RIGHT.
        /// </summary>
        private static GameObject BuildSteelLBracketVisual(string codeName, Transform parent, Bounds b, Dictionary<string, Material> materials)
        {
            GameObject group = new GameObject("SecuredBracket");
            group.transform.SetParent(parent, false);
            group.transform.localPosition = Vector3.zero;

            materials.TryGetValue("M_SafetyOrange", out Material strapMat);
            materials.TryGetValue("M_SilverMetal", out Material steelMat);
            materials.TryGetValue("M_Yellow", out Material boltMat);

            float[] leftRight = { -1f, 1f };

            switch (codeName)
            {
                case "Television":
                {
                    // Television: Left & Right Outer Side-Bezel Clamps + Rear Wall Tension Struts + Base Stand Locks
                    // Screen is at |x| <= 0.445, z = +0.030. Clamps sit strictly at x = ±0.486 (outer edge) and extend to -Z (back wall).
                    foreach (float s in leftRight)
                    {
                        float xEdge = s * 0.486f;
                        // Outer side frame clamp bar
                        AddBracketPart(group.transform, PrimitiveType.Cube, "TV_SideBezelClamp",
                            new Vector3(xEdge, 0.37f, 0.0f), new Vector3(0.026f, 0.38f, 0.076f), strapMat);
                        // Upper & lower outer bezel grip tabs (only gripping outer 1.5cm bezel rim, 0% screen overlap!)
                        AddBracketPart(group.transform, PrimitiveType.Cube, "TV_SideGripTop",
                            new Vector3(s * 0.474f, 0.53f, 0.0f), new Vector3(0.032f, 0.042f, 0.080f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cube, "TV_SideGripBot",
                            new Vector3(s * 0.474f, 0.21f, 0.0f), new Vector3(0.032f, 0.042f, 0.080f), steelMat);
                        // Rear steel tension arms going backward (-Z) to the wall
                        AddBracketPart(group.transform, PrimitiveType.Cube, "TV_RearTensionArmTop",
                            new Vector3(xEdge, 0.50f, -0.085f), new Vector3(0.022f, 0.028f, 0.14f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cube, "TV_RearTensionArmBot",
                            new Vector3(xEdge, 0.24f, -0.085f), new Vector3(0.022f, 0.028f, 0.14f), steelMat);
                        // Rear wall mounting plate & bolts (-Z)
                        AddBracketPart(group.transform, PrimitiveType.Cube, "TV_RearWallPlate",
                            new Vector3(xEdge, 0.37f, -0.155f), new Vector3(0.058f, 0.34f, 0.016f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cylinder, "TV_WallBoltTop",
                            new Vector3(xEdge, 0.50f, -0.165f), new Vector3(0.028f, 0.014f, 0.028f), boltMat, new Vector3(90f, 0f, 0f));
                        AddBracketPart(group.transform, PrimitiveType.Cylinder, "TV_WallBoltBot",
                            new Vector3(xEdge, 0.24f, -0.165f), new Vector3(0.028f, 0.014f, 0.028f), boltMat, new Vector3(90f, 0f, 0f));
                        // Base stand anti-slip clamps on left/right edges of StandBase
                        AddBracketPart(group.transform, PrimitiveType.Cube, "TV_StandClamp",
                            new Vector3(s * 0.225f, 0.020f, -0.02f), new Vector3(0.045f, 0.026f, 0.18f), strapMat);
                        AddBracketPart(group.transform, PrimitiveType.Cylinder, "TV_StandBolt",
                            new Vector3(s * 0.225f, 0.032f, -0.07f), new Vector3(0.024f, 0.012f, 0.024f), boltMat);
                    }
                    break;
                }

                case "Bookcase":
                {
                    // Bookcase: 3 Top-Crown L-Brackets (Left, Center Divider, Right) + Exterior Left/Right Side-Wall Anchors.
                    // Front open shelves (+Z) of both vertical compartments remain 100% open!
                    float[] topXs = { -0.36f, 0.0f, 0.36f };
                    foreach (float x in topXs)
                    {
                        AddBracketPart(group.transform, PrimitiveType.Cube, "Bookcase_TopL_Horiz",
                            new Vector3(x, 1.812f, -0.12f), new Vector3(0.055f, 0.018f, 0.14f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cube, "Bookcase_TopL_Wall",
                            new Vector3(x, 1.87f, -0.188f), new Vector3(0.055f, 0.13f, 0.018f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cylinder, "Bookcase_TopBolt",
                            new Vector3(x, 1.89f, -0.198f), new Vector3(0.030f, 0.014f, 0.030f), boltMat, new Vector3(90f, 0f, 0f));
                    }
                    float[] shelfHeights = { 1.34f, 0.90f };
                    foreach (float s in leftRight)
                    {
                        foreach (float y in shelfHeights)
                        {
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Bookcase_SideStrap",
                                new Vector3(s * 0.490f, y, -0.09f), new Vector3(0.024f, 0.055f, 0.19f), strapMat);
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Bookcase_SideWallWing",
                                new Vector3(s * 0.525f, y, -0.188f), new Vector3(0.080f, 0.078f, 0.018f), steelMat);
                            AddBracketPart(group.transform, PrimitiveType.Cylinder, "Bookcase_SideWallBolt",
                                new Vector3(s * 0.538f, y, -0.198f), new Vector3(0.028f, 0.014f, 0.028f), boltMat, new Vector3(90f, 0f, 0f));
                        }
                    }
                    break;
                }

                case "Refrigerator":
                {
                    // Refrigerator: Top-Roof Heavy L-Brackets + Left/Right Side-Panel Rear Seismic Straps.
                    // Front Upper Freezer Door & Lower Fridge Door (+Z) are 100% clear to open!
                    foreach (float s in leftRight)
                    {
                        float xTop = s * 0.25f;
                        AddBracketPart(group.transform, PrimitiveType.Cube, "Fridge_TopL_Horiz",
                            new Vector3(xTop, 1.768f, -0.24f), new Vector3(0.060f, 0.018f, 0.22f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cube, "Fridge_TopL_Wall",
                            new Vector3(xTop, 1.83f, -0.356f), new Vector3(0.060f, 0.14f, 0.018f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cylinder, "Fridge_TopBolt",
                            new Vector3(xTop, 1.85f, -0.366f), new Vector3(0.032f, 0.015f, 0.032f), boltMat, new Vector3(90f, 0f, 0f));

                        float[] sideYs = { 1.38f, 0.78f };
                        foreach (float y in sideYs)
                        {
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Fridge_SideSeismicStrap",
                                new Vector3(s * 0.388f, y, -0.15f), new Vector3(0.024f, 0.065f, 0.42f), strapMat);
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Fridge_SideBuckle",
                                new Vector3(s * 0.390f, y, 0.05f), new Vector3(0.030f, 0.080f, 0.055f), steelMat);
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Fridge_RearWallWing",
                                new Vector3(s * 0.425f, y, -0.356f), new Vector3(0.090f, 0.090f, 0.020f), steelMat);
                            AddBracketPart(group.transform, PrimitiveType.Cylinder, "Fridge_SideWallBolt",
                                new Vector3(s * 0.438f, y, -0.366f), new Vector3(0.032f, 0.015f, 0.032f), boltMat, new Vector3(90f, 0f, 0f));
                        }
                    }
                    break;
                }

                case "AirConditioner":
                {
                    // AirConditioner: Left & Right Wall L-Console Support Arms + Side Cheek Clamps.
                    // Front vents and LED display (+Z) are 100% clear!
                    foreach (float s in leftRight)
                    {
                        float xSide = s * 0.436f;
                        AddBracketPart(group.transform, PrimitiveType.Cube, "AC_WallRail",
                            new Vector3(xSide, 0.14f, -0.125f), new Vector3(0.042f, 0.32f, 0.018f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cube, "AC_BottomConsoleArm",
                            new Vector3(xSide, -0.008f, -0.02f), new Vector3(0.042f, 0.020f, 0.21f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cube, "AC_SideCheekClamp",
                            new Vector3(xSide, 0.14f, -0.02f), new Vector3(0.024f, 0.22f, 0.16f), strapMat);
                        AddBracketPart(group.transform, PrimitiveType.Cylinder, "AC_WallBoltTop",
                            new Vector3(xSide, 0.27f, -0.135f), new Vector3(0.028f, 0.014f, 0.028f), boltMat, new Vector3(90f, 0f, 0f));
                        AddBracketPart(group.transform, PrimitiveType.Cylinder, "AC_WallBoltBot",
                            new Vector3(xSide, 0.03f, -0.135f), new Vector3(0.028f, 0.014f, 0.028f), boltMat, new Vector3(90f, 0f, 0f));
                    }
                    break;
                }

                case "WaterHeater":
                {
                    // WaterHeater: Upper & Mid-Height Side Seismic Clamp Rings & Rear Wall Struts.
                    // Front thermostat dial box (y=0.27, z=+0.23) and bottom pipes (y=0.03) are 100% clear!
                    float[] ringHeights = { 0.68f, 0.46f };
                    foreach (float y in ringHeights)
                    {
                        AddBracketPart(group.transform, PrimitiveType.Cube, "Heater_RearWallBeam",
                            new Vector3(0f, y, -0.255f), new Vector3(0.58f, 0.065f, 0.022f), steelMat);
                        foreach (float s in leftRight)
                        {
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Heater_SideClampPad",
                                new Vector3(s * 0.242f, y, -0.02f), new Vector3(0.028f, 0.058f, 0.22f), strapMat);
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Heater_RearTensionStrut",
                                new Vector3(s * 0.248f, y, -0.14f), new Vector3(0.024f, 0.042f, 0.23f), steelMat);
                            AddBracketPart(group.transform, PrimitiveType.Cylinder, "Heater_WallBolt",
                                new Vector3(s * 0.26f, y, -0.268f), new Vector3(0.030f, 0.015f, 0.030f), boltMat, new Vector3(90f, 0f, 0f));
                        }
                    }
                    break;
                }

                case "FireCabinet":
                {
                    // FireCabinet: Exterior Left/Right Side-Wall Flanges & Top-Rear L-Brackets.
                    // Front hinged door (CabinetDoor_Hinge at +Z) swings open +105 deg outward with 0% obstruction!
                    foreach (float s in leftRight)
                    {
                        float xTop = s * 0.14f;
                        AddBracketPart(group.transform, PrimitiveType.Cube, "Cabinet_TopL_Horiz",
                            new Vector3(xTop, 0.828f, -0.10f), new Vector3(0.045f, 0.016f, 0.12f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cube, "Cabinet_TopL_Wall",
                            new Vector3(xTop, 0.875f, -0.155f), new Vector3(0.045f, 0.10f, 0.016f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cylinder, "Cabinet_TopBolt",
                            new Vector3(xTop, 0.89f, -0.165f), new Vector3(0.026f, 0.014f, 0.026f), boltMat, new Vector3(90f, 0f, 0f));

                        float[] sideYs = { 0.66f, 0.18f };
                        foreach (float y in sideYs)
                        {
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Cabinet_SideClamp",
                                new Vector3(s * 0.226f, y, -0.07f), new Vector3(0.022f, 0.065f, 0.16f), strapMat);
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Cabinet_WallFlange",
                                new Vector3(s * 0.255f, y, -0.155f), new Vector3(0.075f, 0.080f, 0.018f), steelMat);
                            AddBracketPart(group.transform, PrimitiveType.Cylinder, "Cabinet_SideWallBolt",
                                new Vector3(s * 0.265f, y, -0.166f), new Vector3(0.028f, 0.014f, 0.028f), boltMat, new Vector3(90f, 0f, 0f));
                        }
                    }
                    break;
                }

                case "SturdyTable":
                {
                    // SturdyTable: 4 Floor-Anchored Foot Clamps around the bottom of the 4 legs (y = 0..0.08m).
                    // Tabletop and under-table Life Triangle / Shelter space remain 100% open!
                    float[] legXs = { -0.61f, 0.61f };
                    float[] legZs = { -0.35f, 0.35f };
                    foreach (float lx in legXs)
                    {
                        foreach (float lz in legZs)
                        {
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Table_LegCollar",
                                new Vector3(lx, 0.045f, lz), new Vector3(0.092f, 0.075f, 0.092f), strapMat);
                            AddBracketPart(group.transform, PrimitiveType.Cube, "Table_FloorFlange",
                                new Vector3(lx, 0.008f, lz), new Vector3(0.145f, 0.016f, 0.145f), steelMat);
                            AddBracketPart(group.transform, PrimitiveType.Cylinder, "Table_FloorBoltX",
                                new Vector3(lx + Mathf.Sign(lx) * 0.055f, 0.018f, lz), new Vector3(0.024f, 0.014f, 0.024f), boltMat);
                            AddBracketPart(group.transform, PrimitiveType.Cylinder, "Table_FloorBoltZ",
                                new Vector3(lx, 0.018f, lz + Mathf.Sign(lz) * 0.055f), new Vector3(0.024f, 0.014f, 0.024f), boltMat);
                        }
                    }
                    break;
                }

                default:
                {
                    // Fallback non-obstructing side-rear wall brackets
                    float topY = Mathf.Clamp(b.max.y * 0.85f, 0.15f, 1.75f);
                    float backZ = b.min.z - 0.012f;
                    foreach (float s in leftRight)
                    {
                        float xSide = s * (b.extents.x + 0.012f);
                        AddBracketPart(group.transform, PrimitiveType.Cube, "SideRearStrap",
                            new Vector3(xSide, topY, b.min.z + b.extents.z * 0.4f), new Vector3(0.024f, 0.06f, b.extents.z * 0.8f), strapMat);
                        AddBracketPart(group.transform, PrimitiveType.Cube, "RearWallPlate",
                            new Vector3(xSide + s * 0.025f, topY, backZ), new Vector3(0.07f, 0.08f, 0.016f), steelMat);
                        AddBracketPart(group.transform, PrimitiveType.Cylinder, "WallBolt",
                            new Vector3(xSide + s * 0.03f, topY, backZ - 0.01f), new Vector3(0.026f, 0.014f, 0.026f), boltMat, new Vector3(90f, 0f, 0f));
                    }
                    break;
                }
            }

            return group;
        }

        private static Sprite RenderAndSaveIcon(AssetSpec spec, GameObject prefab)
        {
            const int res = 256;
            string iconPath = $"{IconsDir}/T_Icon_{spec.CodeName}.png";

            Vector3 stageOrigin = new Vector3(500f, 500f, 500f);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = stageOrigin;
            instance.transform.rotation = Quaternion.Euler(0f, 6f, 0f);

            // For FireCabinet icon, open the door outward (+95 deg) so both the hinged door and the FireExtinguisher inside are clearly visible!
            if (spec.CodeName == "FireCabinet")
            {
                var itemComp = instance.GetComponent<DisasterPrepItem>();
                if (itemComp != null && itemComp.cabinetDoorHinge != null)
                {
                    itemComp.cabinetDoorHinge.localRotation = Quaternion.Euler(0f, 95f, 0f);
                }
            }

            Bounds b = CalculateBounds(instance);
            float maxExtent = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z));
            if (maxExtent < 0.01f) maxExtent = 0.1f;

            GameObject camGo = new GameObject("IconRenderCam");
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 0f);
            cam.orthographic = true;
            cam.orthographicSize = maxExtent * 1.22f;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 25f;

            Vector3 viewDir = new Vector3(-0.42f, 0.44f, 0.78f).normalized;
            camGo.transform.position = b.center + viewDir * 6f;
            camGo.transform.LookAt(b.center);

            Light[] sceneLights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            var enabledLights = new List<Light>();
            foreach (var sl in sceneLights)
            {
                if (sl != null && sl.enabled)
                {
                    sl.enabled = false;
                    enabledLights.Add(sl);
                }
            }

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

            foreach (var sl in enabledLights)
            {
                if (sl != null) sl.enabled = true;
            }

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
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. 3-Point Isometric Lighting Rig + Gradient Ambient
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.56f, 0.64f, 0.68f);     // Cool Slate Sky
            RenderSettings.ambientEquatorColor = new Color(0.38f, 0.49f, 0.55f); // Neutral Mid
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.22f, 0.20f);  // Warm Bounce

            GameObject keyLightGo = new GameObject("Directional_KeyLight_Sun");
            Light keyLight = keyLightGo.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(1.00f, 0.97f, 0.90f); // Warm 5500K Daylight
            keyLight.intensity = 1.20f;
            keyLight.shadows = LightShadows.Soft;
            keyLight.shadowStrength = 0.78f;
            keyLightGo.transform.rotation = Quaternion.Euler(48f, -38f, 0f);

            GameObject rimLightGo = new GameObject("Directional_RimLight_Back");
            Light rimLight = rimLightGo.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = new Color(0.85f, 0.95f, 1.00f); // Cool Rim Separation
            rimLight.intensity = 0.38f;
            rimLight.shadows = LightShadows.None;
            rimLightGo.transform.rotation = Quaternion.Euler(32f, 142f, 0f);

            // 2. Showcase Floor Plane (Warm Oak / Neutral Slate)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "ShowcaseFloor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2.6f, 1f, 2.2f);
            if (materials.TryGetValue("M_RubberBlack", out Material floorMat))
            {
                floor.GetComponent<Renderer>().sharedMaterial = floorMat;
            }

            // 3. 1.8m Human Scale Reference Capsule
            GameObject humanRef = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            humanRef.name = "HumanScaleRef_1.8m";
            humanRef.transform.position = new Vector3(-6.4f, 0.9f, 0f);
            humanRef.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
            if (materials.TryGetValue("M_SafetyOrange", out Material humanMat))
            {
                humanRef.GetComponent<Renderer>().sharedMaterial = humanMat;
            }

            // 4. Grid Layout for all 33 Prefabs + Secured Demo Row
            int cols = 9;
            float spacingX = 1.50f;
            float spacingZ = 1.85f;
            Vector3 startPos = new Vector3(-5.6f, 0f, -2.7f);

            GameObject container = new GameObject("DisasterPrepAssets_All33");
            container.AddComponent<DisasterWaveEvaluator>();
            for (int i = 0; i < prefabs.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;
                Vector3 pos = startPos + new Vector3(col * spacingX, 0f, row * spacingZ);

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[i]);
                instance.transform.SetParent(container.transform, false);
                instance.transform.position = pos;
            }

            // 5. Side-by-Side "Secured L-Bracket & Closed Valve Demo" row in front so user can immediately see all 10 Interactive/Securable Assets in scene!
            GameObject securedDemoGroup = new GameObject("Secured_LBracket_LiveComparison");
            string[] securableDemoNames = {
                "PF_GasValve",
                "PF_WaterValve",
                "PF_BreakerBox",
                "PF_FireCabinet",
                "PF_Television",
                "PF_Bookcase",
                "PF_Refrigerator",
                "PF_AirConditioner",
                "PF_WaterHeater",
                "PF_SturdyTable"
            };
            for (int i = 0; i < securableDemoNames.Length; i++)
            {
                GameObject pf = prefabs.Find(p => p.name == securableDemoNames[i]);
                if (pf != null)
                {
                    GameObject secInst = (GameObject)PrefabUtility.InstantiatePrefab(pf);
                    secInst.name = pf.name + "_SECURED_DEMO";
                    secInst.transform.SetParent(securedDemoGroup.transform, false);
                    secInst.transform.position = new Vector3(-6.2f + i * 1.40f, 0f, 5.2f);
                    var itemComp = secInst.GetComponent<DisasterPrepItem>();
                    if (itemComp != null)
                    {
                        itemComp.SetSecuredOrClosed(true, instant: true);
                        // Also swing open the FireCabinet & Refrigerator doors in the demo row so the user sees the animated doors + wall brackets!
                        if (securableDemoNames[i] == "PF_FireCabinet" || securableDemoNames[i] == "PF_Refrigerator")
                        {
                            itemComp.SetCabinetDoorOpen(true, instant: true);
                        }
                    }
                }
            }

            // 6. True Isometric Orthographic Camera (Pitch: 35.264, Yaw: 45)
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera mainCam = camGo.AddComponent<Camera>();
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5.6f;
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.11f, 0.13f, 0.16f, 1f);
            mainCam.nearClipPlane = -15f;
            mainCam.farClipPlane = 60f;
            camGo.transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
            Vector3 focusPoint = new Vector3(0.5f, 0.4f, 0.8f);
            camGo.transform.position = focusPoint - (camGo.transform.forward * 14f);

            string scenePath = $"{ScenesDir}/Demo_YarkinAssets.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
#endif
