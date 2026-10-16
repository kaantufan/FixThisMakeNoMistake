#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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
            // Automatically trigger full 32-asset build if the new 32nd prefab (PF_RubblePile) is not built yet
            if (AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/PF_RubblePile.prefab") == null &&
                AssetDatabase.LoadAssetAtPath<GameObject>($"{MeshesDir}/SM_RubblePile.fbx") != null)
            {
                Debug.Log("[YarkinAssetPipelineBuilder] Auto-building all 32 upgraded prefabs, L-brackets, icons & isometric scene...");
                BuildAll();
            }
        }
        private const string RootDir = "Assets/_YarkinAssets";
        private const string MeshesDir = RootDir + "/Art/Meshes";
        private const string MaterialsDir = RootDir + "/Art/Materials";
        private const string IconsDir = RootDir + "/Art/Icons";
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

        // Full 32-Asset Master Catalog (18 Upgraded Core + 14 IMO Expansion)
        private static readonly AssetSpec[] Specs = new[]
        {
            // --- PART A: ORIGINAL 18 ASSETS ---
            new AssetSpec("FireExtinguisher",  "fire_extinguisher",  "Yangın Tüpü",          DisasterItemCategory.FireSafety,          4.50f, 35, true,  true,  false, SecondaryHazardType.FireWave,         ColliderKind.Capsule),
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

            // --- PART B: 14 NEW IMO DISASTER EXPANSION ASSETS ---
            new AssetSpec("GasValve",          "gas_valve",          "Doğalgaz Ana Vanası",  DisasterItemCategory.UtilityValve,        0.00f, 45, false, false, true,  SecondaryHazardType.FireWave,         ColliderKind.Box),
            new AssetSpec("WaterValve",        "water_valve",        "Su Kesme Vanası",      DisasterItemCategory.UtilityValve,        0.00f, 40, false, false, true,  SecondaryHazardType.FloodWave,        ColliderKind.Box),
            new AssetSpec("BreakerBox",        "breaker_box",        "Elektrik Sigorta Panosu",DisasterItemCategory.UtilityValve,      0.00f, 40, false, false, true,  SecondaryHazardType.ElectricalShock,  ColliderKind.Box),
            new AssetSpec("Bookcase",          "bookcase",           "Büyük Ahşap Kitaplık", DisasterItemCategory.HouseholdFixture,   35.00f, 35, false, true,  false, SecondaryHazardType.CorridorBlockage, ColliderKind.Box),
            new AssetSpec("Refrigerator",      "refrigerator",       "Mutfak Buzdolabı",     DisasterItemCategory.HouseholdFixture,   55.00f, 35, false, true,  false, SecondaryHazardType.CorridorBlockage, ColliderKind.Box),
            new AssetSpec("WaterHeater",       "water_heater",       "Banyo Termosifonu",    DisasterItemCategory.HouseholdFixture,   28.00f, 40, false, true,  false, SecondaryHazardType.FloodWave,        ColliderKind.Capsule),
            new AssetSpec("SturdyTable",       "sturdy_table",       "Sağlam Masa (Yaşam Üçgeni)",DisasterItemCategory.HouseholdFixture,22.0f,30, false, false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("EmergencyWhistle",  "emergency_whistle",  "Deprem Düdüğü",        DisasterItemCategory.Communication,       0.03f, 35, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("DocumentPouch",     "document_pouch",     "Evrak & DASK Dosyası", DisasterItemCategory.SurvivalGear,        0.20f, 30, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("WorkGloves",        "work_gloves",        "Ağır Hizmet Eldiveni", DisasterItemCategory.SurvivalGear,        0.25f, 20, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("MultiTool",         "multi_tool",         "Çok Amaçlı Pense/Çakı",DisasterItemCategory.SurvivalGear,        0.22f, 25, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("DustMask",          "dust_mask",          "FFP3 Toz Maskesi",     DisasterItemCategory.MedicalAndHygiene,   0.05f, 20, true,  false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("ShatteredGlass",    "shattered_glass",    "Kırık Cam Tehlikesi",  DisasterItemCategory.EnvironmentalHazard, 0.00f,  0, false, false, false, SecondaryHazardType.None,             ColliderKind.Box),
            new AssetSpec("RubblePile",        "rubble_pile",        "Dökülmüş Moloz Yığını",DisasterItemCategory.EnvironmentalHazard, 0.00f,  0, false, false, false, SecondaryHazardType.CorridorBlockage, ColliderKind.Box),
        };

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
            Debug.Log($"[YarkinAssetPipelineBuilder] Successfully built {prefabs.Count} prefabs (with 3D L-Brackets & Valve states), {Palette.Length} semantic materials, 32 icons, and True Isometric Demo_YarkinAssets scene!");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(MaterialsDir);
            Directory.CreateDirectory(IconsDir);
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

            // 3. If Securable Fixture -> Add 3D Steel L-Bracket & Safety Straps child object (Default Inactive, toggled when player secures!)
            GameObject bracketRoot = null;
            if (spec.IsSecurableFixture)
            {
                bracketRoot = BuildSteelLBracketVisual(root.transform, combinedBounds, materials);
                bracketRoot.SetActive(false); // Hidden until player secures it with [F]!
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

            // 5. Add IsometricItemMotion component
            var motion = root.AddComponent<IsometricItemMotion>();
            motion.targetVisual = visual.transform;
            motion.enableIdleBobbing = false;

            string prefabPath = $"{PrefabsDir}/PF_{spec.CodeName}.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static GameObject BuildSteelLBracketVisual(Transform parent, Bounds b, Dictionary<string, Material> materials)
        {
            GameObject group = new GameObject("SecuredBracket");
            group.transform.SetParent(parent, false);
            group.transform.localPosition = Vector3.zero;

            materials.TryGetValue("M_SafetyOrange", out Material strapMat);
            materials.TryGetValue("M_SilverMetal", out Material steelMat);
            materials.TryGetValue("M_Yellow", out Material boltMat);

            float topY = Mathf.Clamp(b.max.y * 0.82f, 0.15f, 1.65f);
            float backZ = b.max.z + 0.015f;
            float offsetX = Mathf.Max(0.06f, b.extents.x * 0.62f);

            float[] sides = new[] { -offsetX, offsetX };
            foreach (float x in sides)
            {
                // Steel L-plate vertical arm
                GameObject vPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vPlate.name = "LBracket_WallPlate";
                UnityEngine.Object.DestroyImmediate(vPlate.GetComponent<Collider>());
                vPlate.transform.SetParent(group.transform, false);
                vPlate.transform.localPosition = new Vector3(x, topY + 0.04f, backZ);
                vPlate.transform.localScale = new Vector3(0.045f, 0.11f, 0.015f);
                if (steelMat != null) vPlate.GetComponent<Renderer>().sharedMaterial = steelMat;

                // Steel L-plate horizontal brace
                GameObject hPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hPlate.name = "LBracket_TopArm";
                UnityEngine.Object.DestroyImmediate(hPlate.GetComponent<Collider>());
                hPlate.transform.SetParent(group.transform, false);
                hPlate.transform.localPosition = new Vector3(x, topY, backZ - 0.04f);
                hPlate.transform.localScale = new Vector3(0.045f, 0.016f, 0.09f);
                if (steelMat != null) hPlate.GetComponent<Renderer>().sharedMaterial = steelMat;

                // High-vis Safety Strap wrapping the fixture
                GameObject strap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strap.name = "SafetyStrap_Band";
                UnityEngine.Object.DestroyImmediate(strap.GetComponent<Collider>());
                strap.transform.SetParent(group.transform, false);
                strap.transform.localPosition = new Vector3(x, b.center.y, b.center.z);
                strap.transform.localScale = new Vector3(0.036f, b.size.y * 0.75f, b.size.z + 0.022f);
                if (strapMat != null) strap.GetComponent<Renderer>().sharedMaterial = strapMat;

                // Heavy Bolt Anchor
                GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bolt.name = "WallAnchorBolt";
                UnityEngine.Object.DestroyImmediate(bolt.GetComponent<Collider>());
                bolt.transform.SetParent(group.transform, false);
                bolt.transform.localPosition = new Vector3(x, topY + 0.055f, backZ + 0.008f);
                bolt.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                bolt.transform.localScale = new Vector3(0.026f, 0.014f, 0.026f);
                if (boltMat != null) bolt.GetComponent<Renderer>().sharedMaterial = boltMat;
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
            instance.transform.rotation = Quaternion.Euler(0f, -28f, 0f);

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

            GameObject keyLightGo = new GameObject("IconKeyLight");
            Light keyLight = keyLightGo.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.30f;
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
            floor.transform.localScale = new Vector3(2.4f, 1f, 2.0f);
            if (materials.TryGetValue("M_RubberBlack", out Material floorMat))
            {
                floor.GetComponent<Renderer>().sharedMaterial = floorMat;
            }

            // 3. 1.8m Human Scale Reference Capsule
            GameObject humanRef = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            humanRef.name = "HumanScaleRef_1.8m";
            humanRef.transform.position = new Vector3(-6.2f, 0.9f, 0f);
            humanRef.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
            if (materials.TryGetValue("M_SafetyOrange", out Material humanMat))
            {
                humanRef.GetComponent<Renderer>().sharedMaterial = humanMat;
            }

            // 4. Grid Layout for all 32 Prefabs (8 columns x 4 rows) + Secured Demo Pair
            int cols = 8;
            float spacingX = 1.55f;
            float spacingZ = 1.85f;
            Vector3 startPos = new Vector3(-4.8f, 0f, -2.7f);

            GameObject container = new GameObject("DisasterPrepAssets_All32");
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

            // Also place a Side-by-Side "Secured L-Bracket Demo" row in front so user can immediately see State 1 (Secured) in scene!
            GameObject securedDemoGroup = new GameObject("Secured_LBracket_LiveComparison");
            string[] securableDemoNames = { "PF_Television", "PF_AirConditioner", "PF_Bookcase", "PF_Refrigerator", "PF_WaterHeater" };
            for (int i = 0; i < securableDemoNames.Length; i++)
            {
                GameObject pf = prefabs.Find(p => p.name == securableDemoNames[i]);
                if (pf != null)
                {
                    GameObject secInst = (GameObject)PrefabUtility.InstantiatePrefab(pf);
                    secInst.name = pf.name + "_SECURED_DEMO";
                    secInst.transform.SetParent(securedDemoGroup.transform, false);
                    secInst.transform.position = new Vector3(-3.6f + i * 1.95f, 0f, 5.2f);
                    var itemComp = secInst.GetComponent<DisasterPrepItem>();
                    if (itemComp != null)
                    {
                        itemComp.SetSecuredOrClosed(true);
                    }
                }
            }

            // 5. True Isometric Orthographic Camera (Pitch: 35.264, Yaw: 45)
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera mainCam = camGo.AddComponent<Camera>();
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5.4f;
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
