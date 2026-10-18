using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Interactive 3D Showcase and Gallery for all Curated 3D Assets.
    /// Assets are stably suspended in the air at eye level without vertical bobbing.
    /// Defaults to front-facing orientation towards the camera with interactive 360 mouse inspection.
    /// </summary>
    [ExecuteAlways]
    public sealed class PanicAssetShowcase : MonoBehaviour
    {
        public sealed class ItemEntry
        {
            public string AssetName;
            public string TurkishName;
            public string Category;
            public float Scale;
            public string Description;
            public GameObject Instance;
            public PanicCuratedMechanism Mechanism;
            public Transform DoorLeafTransform;
            public Quaternion DoorClosedRot;
            public Vector3 BasePosition;
            public Quaternion BaseRotation;
            public bool IsOpen;
        }

        static readonly ItemEntry[] Catalog = new[]
        {
            // Category: Erzak & Çanta
            new ItemEntry { AssetName = "WaterBottle", TurkishName = "Su Şişesi", Category = "Erzak & Çanta", Scale = 0.45f, Description = "Acil durum içme suyu şişesi (Temel hayatta kalma erzağı)." },
            new ItemEntry { AssetName = "CannedFood", TurkishName = "Konserve Gıda", Category = "Erzak & Çanta", Scale = 0.40f, Description = "Uzun ömürlü konserve yiyecek." },
            new ItemEntry { AssetName = "Apple", TurkishName = "Taze Elma", Category = "Erzak & Çanta", Scale = 0.32f, Description = "Acil durum çantası için hızlı enerji sağlayan meyve." },
            new ItemEntry { AssetName = "SnackBar", TurkishName = "Atıştırmalık Bar", Category = "Erzak & Çanta", Scale = 0.34f, Description = "Yüksek kalorili kompakt gıda barı." },
            new ItemEntry { AssetName = "EnergyDrink", TurkishName = "Enerji İçeceği", Category = "Erzak & Çanta", Scale = 0.36f, Description = "Hızlı sıvı ve elektrolit takviyesi." },
            new ItemEntry { AssetName = "FirstAidKit", TurkishName = "İlk Yardım Çantası", Category = "Erzak & Çanta", Scale = 0.52f, Description = "Sargı bezi, antiseptik ve medikal acil müdahale seti." },
            new ItemEntry { AssetName = "Flashlight", TurkishName = "El Feneri", Category = "Erzak & Çanta", Scale = 0.44f, Description = "Elektrik kesintisinde yol bulmak için el feneri." },
            new ItemEntry { AssetName = "EmergencyRadio", TurkishName = "Acil Durum Radyosu", Category = "Erzak & Çanta", Scale = 0.48f, Description = "AFAD ve resmi afet anonslarını dinlemek için radyo." },
            new ItemEntry { AssetName = "WalkieTalkie", TurkishName = "Telsiz", Category = "Erzak & Çanta", Scale = 0.42f, Description = "Hücresel şebekeler çöktüğünde yerel iletişim telsizi." },
            new ItemEntry { AssetName = "EmergencyWhistle", TurkishName = "Düdük", Category = "Erzak & Çanta", Scale = 0.30f, Description = "Enkaz altında veya yardım çağrısında ses duyurma düdüğü." },
            new ItemEntry { AssetName = "DustMask", TurkishName = "Toz Maskesi", Category = "Erzak & Çanta", Scale = 0.36f, Description = "Enkaz ve yangın dumanından korunmak için FFP2 maske." },
            new ItemEntry { AssetName = "WorkGloves", TurkishName = "İş / Koruma Eldiveni", Category = "Erzak & Çanta", Scale = 0.42f, Description = "Moloz ve cam kırıklarını taşırken elleri koruyan kalın eldiven." },
            new ItemEntry { AssetName = "DocumentPouch", TurkishName = "Evrak Dosyası", Category = "Erzak & Çanta", Scale = 0.46f, Description = "Kimlik, tapu, sigorta gibi kritik belgeleri içeren su geçirmez kılıf." },
            new ItemEntry { AssetName = "MultiTool", TurkishName = "Çok Amaçlı Çakı", Category = "Erzak & Çanta", Scale = 0.36f, Description = "Pense, bıçak, konserve açacağı içeren fonksiyonel çakı." },
            new ItemEntry { AssetName = "SmallBattery", TurkishName = "Küçük Pil (AA)", Category = "Erzak & Çanta", Scale = 0.26f, Description = "Radyo ve fener için yedek piller." },
            new ItemEntry { AssetName = "PowerBattery", TurkishName = "Güç Ünitesi / Akü", Category = "Erzak & Çanta", Scale = 0.54f, Description = "Yedek güç kaynağı ve şarj ünitesi." },

            // Category: Mobilya & Eşyalar
            new ItemEntry { AssetName = "EmergencyBackpack", TurkishName = "Afet Çantası", Category = "Mobilya & Eşyalar", Scale = 0.75f, Description = "Toplanan tüm malzemelerin konduğu taşınabilir sırt çantası." },
            new ItemEntry { AssetName = "Blanket", TurkishName = "Battaniye", Category = "Mobilya & Eşyalar", Scale = 0.60f, Description = "Deprem sonrası hipotermiden korunmak için kalın battaniye." },
            new ItemEntry { AssetName = "FoldedClothes", TurkishName = "Yedek Giysiler", Category = "Mobilya & Eşyalar", Scale = 0.55f, Description = "Mevsim şartlarına uygun katlanmış yedek kıyafet takımı." },
            new ItemEntry { AssetName = "Rug", TurkishName = "Halı / Kilim", Category = "Mobilya & Eşyalar", Scale = 0.95f, Description = "Zemin kaplaması ve dekoratif halı." },
            new ItemEntry { AssetName = "SturdyTable", TurkishName = "Sağlam Masa", Category = "Mobilya & Eşyalar", Scale = 1.35f, Description = "Deprem sarsıntısı sırasında Çök-Kapan-Tutun yapılabilecek masa." },
            new ItemEntry { AssetName = "Bookcase", TurkishName = "Kitaplık", Category = "Mobilya & Eşyalar", Scale = 1.85f, Description = "Duvara L braketle vidalanıp sabitlenmesi gereken ağır mobilya." },
            new ItemEntry { AssetName = "Television", TurkishName = "Televizyon", Category = "Mobilya & Eşyalar", Scale = 1.15f, Description = "Konsola veya duvara sabitlenmesi gereken ince ekran televizyon." },
            new ItemEntry { AssetName = "Refrigerator", TurkishName = "Buzdolabı", Category = "Mobilya & Eşyalar", Scale = 1.95f, Description = "Depremde devrilme riski yüksek, açılabilir kapaklı büyük beyaz eşya." },
            new ItemEntry { AssetName = "AirConditioner", TurkishName = "Klima Ünitesi", Category = "Mobilya & Eşyalar", Scale = 1.15f, Description = "Duvar montajlı iç ünite klima." },

            // Category: Tesisat, Güvenlik & Afet
            new ItemEntry { AssetName = "FireExtinguisher", TurkishName = "Yangın Söndürme Tüpü", Category = "Tesisat & Güvenlik", Scale = 0.80f, Description = "İlk yangın anında alevleri söndürmek için 6 kg'lık KKT tüp." },
            new ItemEntry { AssetName = "FireCabinet", TurkishName = "Yangın Dolabı", Category = "Tesisat & Güvenlik", Scale = 1.25f, Description = "Hortum ve yangın söndürme ekipmanını barındıran acil durum dolabı." },
            new ItemEntry { AssetName = "BreakerBox", TurkishName = "Sigorta / Şalter Panosu", Category = "Tesisat & Güvenlik", Scale = 0.85f, Description = "Yangın ve elektrik çarpmasını önlemek için ana şalter." },
            new ItemEntry { AssetName = "GasValve", TurkishName = "Gaz Vanası", Category = "Tesisat & Güvenlik", Scale = 0.75f, Description = "Patlama ve zehirlenmeyi önlemek için ana doğalgaz vanası." },
            new ItemEntry { AssetName = "WaterValve", TurkishName = "Su Vanası", Category = "Tesisat & Güvenlik", Scale = 0.75f, Description = "Boru patlaması ve su baskınını önlemek için ana vana." },
            new ItemEntry { AssetName = "WaterHeater", TurkishName = "Kombi / Su Isıtıcısı", Category = "Tesisat & Güvenlik", Scale = 1.35f, Description = "Duvara ankrajlanması ve vanalarının kapatılması gereken su ısıtıcısı." },
            new ItemEntry { AssetName = "bunkerhatch", TurkishName = "Sığınak Kapağı (Bunker Hatch)", Category = "Tesisat & Güvenlik", Scale = 1.48f, Description = "Zemine gömülü yuvarlak çelik sığınak kapağı. Kalın çift mafsallı menteşe, hidrolik amortisör, döner kilit çarkı ve derin karanlık kuyu (stencil portal) içerir." },
            new ItemEntry { AssetName = "RubblePile", TurkishName = "Moloz Yığını", Category = "Tesisat & Güvenlik", Scale = 1.35f, Description = "Deprem sarsıntısı sonrası çöken duvar kalıntıları." },
            new ItemEntry { AssetName = "ShatteredGlass", TurkishName = "Kırık Camlar", Category = "Tesisat & Güvenlik", Scale = 0.90f, Description = "Kırılan pencerelerden dökülen kesici cam parçaları." }
        };

        Camera _cam;
        int _selectedIndex = 0;
        int _selectedCategory = 0; // 0: Tümü, 1: Erzak & Çanta, 2: Mobilya & Eşyalar, 3: Tesisat & Güvenlik
        bool _autoRotate = false;  // Default: OFF, items remain facing the user
        bool _focusMode = false;
        Vector3 _targetCamPos;
        Quaternion _targetCamRot;
        float _camMoveSpeed = 4.5f;

        // Floating height in air (meters) - completely stationary without up/down bobbing
        float _floatHeight = 1.45f;

        // Free fly camera controls
        float _yaw, _pitch;
        float _flySpeed = 7f;

        // Interactive Bunker Hatch Cutscene Preview state
        bool _cutsceneActive;
        float _cutsceneAge;
        GameObject _cutsceneBag;
        GameObject _cutsceneActor;

        // UI Styles
        GUIStyle _headerStyle;
        GUIStyle _cardStyle;
        GUIStyle _buttonStyle;
        GUIStyle _activeTabStyle;
        GUIStyle _tabStyle;
        GUIStyle _floatingBadgeStyle;
        GUIStyle _badgeSubStyle;
        Texture2D _panelTex;
        Texture2D _cardTex;
        Texture2D _btnTex;
        Texture2D _btnActiveTex;

        void OnEnable()
        {
            SetupEnvironment();
            BuildShowcase();
            SetupCamera();
            FocusOnItem(0);
        }

        void SetupEnvironment()
        {
            var existingFloor = GameObject.Find("StudioFloor");
            if (!existingFloor)
            {
                var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "StudioFloor";
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(15, 1, 15);
                var floorRenderer = floor.GetComponent<MeshRenderer>();
                var floorMat = new Material(Shader.Find("Standard"));
                floorMat.color = new Color(0.10f, 0.12f, 0.16f);
                floorMat.SetFloat("_Glossiness", 0.35f);
                floorRenderer.material = floorMat;
            }

            var existingLight = GameObject.Find("StudioLight");
            if (!existingLight)
            {
                var lightObj = new GameObject("StudioLight");
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1.0f, 0.98f, 0.92f);
                light.intensity = 1.25f;
                light.shadows = LightShadows.Soft;
                lightObj.transform.rotation = Quaternion.Euler(50f, -40f, 0f);
            }

            var existingFill = GameObject.Find("FillLight");
            if (!existingFill)
            {
                var fillObj = new GameObject("FillLight");
                var fill = fillObj.AddComponent<Light>();
                fill.type = LightType.Directional;
                fill.color = new Color(0.6f, 0.75f, 0.95f);
                fill.intensity = 0.50f;
                fillObj.transform.rotation = Quaternion.Euler(30f, 140f, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.28f, 0.30f, 0.35f);
        }

        void SetupCamera()
        {
            _cam = Camera.main;
            if (!_cam)
            {
                var camObj = new GameObject("Main Camera");
                _cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            _cam.nearClipPlane = 0.1f;
            _cam.farClipPlane = 200f;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            if (!_cam.GetComponent<AudioListener>()) _cam.gameObject.AddComponent<AudioListener>();

            _cam.transform.position = new Vector3(0, _floatHeight, -6.5f);
            _cam.transform.rotation = Quaternion.identity;
            _targetCamPos = _cam.transform.position;
            _targetCamRot = _cam.transform.rotation;
            Vector3 euler = _cam.transform.rotation.eulerAngles;
            _yaw = euler.y;
            _pitch = euler.x;
        }

        void BuildShowcase()
        {
            // Clear any old spawned children to prevent duplication
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }

            float xSpacing = 2.4f;
            int countErzak = 0, countMobilya = 0, countTesisat = 0;

            for (int i = 0; i < Catalog.Length; i++)
            {
                var item = Catalog[i];
                float x = 0;
                float z = 0;

                if (item.Category == "Erzak & Çanta")
                {
                    x = (countErzak - 7.5f) * xSpacing;
                    z = 5.0f;
                    countErzak++;
                }
                else if (item.Category == "Mobilya & Eşyalar")
                {
                    x = (countMobilya - 4.0f) * (xSpacing * 1.35f);
                    z = 0.0f;
                    countMobilya++;
                }
                else
                {
                    x = (countTesisat - 4.0f) * (xSpacing * 1.45f);
                    z = -5.0f;
                    countTesisat++;
                }

                // Model display - Suspended stationary in mid-air facing camera (-Z)
                var display = PanicCuratedAssetArt.CreateDisplay(item.AssetName, item.Scale);
                if (!display)
                {
                    var resPrefab = Resources.Load<GameObject>("ResidenceArt/" + item.AssetName);
                    if (resPrefab)
                    {
                        display = Instantiate(resPrefab);
                    }
                    else
                    {
                        display = PanicResidenceArt.Create(item.AssetName);
                    }
                    if (display)
                    {
                        display.name = "Showcase_" + item.AssetName;
                        display.transform.localScale = Vector3.one * item.Scale;
                        foreach (var col in display.GetComponentsInChildren<Collider>(true))
                        {
                            if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
                        }
                    }
                }

                if (display)
                {
                    display.transform.SetParent(transform, true);
                    display.transform.position = new Vector3(x, _floatHeight, z);
                    // Floor-mounted assets lie completely flat and level with the ground
                    Quaternion initialRot = Quaternion.identity;
                    display.transform.rotation = initialRot;
                    item.Instance = display;
                    item.BasePosition = display.transform.position;
                    item.BaseRotation = initialRot;
                    item.Mechanism = display.GetComponent<PanicCuratedMechanism>();

                    var leaf = display.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "DoorLeaf");
                    if (leaf)
                    {
                        item.DoorLeafTransform = leaf;
                        item.DoorClosedRot = leaf.localRotation;
                    }
                }
            }
        }

        void Update()
        {
            float dt = Application.isPlaying ? Time.deltaTime : 0.016f;

            if (Application.isPlaying)
            {
                HandleKeyboardNavigation();
                HandleFreeCamLook();
                HandleMouseInspection();
            }

            // Keep assets stationary at their floating height (NO up/down bobbing)
            for (int i = 0; i < Catalog.Length; i++)
            {
                var item = Catalog[i];
                if (item != null && item.Instance)
                {
                    // Fixed position in air - completely still on Y axis
                    item.Instance.transform.position = item.BasePosition;

                    // If auto-rotate is toggled ON, spin slowly
                    if (_autoRotate)
                    {
                        item.Instance.transform.Rotate(Vector3.up, 25f * dt, Space.World);
                    }
                }
            }

            // Smooth camera glide when focusing
            if (_focusMode && _cam)
            {
                _cam.transform.position = Vector3.Lerp(_cam.transform.position, _targetCamPos, dt * _camMoveSpeed);
                _cam.transform.rotation = Quaternion.Slerp(_cam.transform.rotation, _targetCamRot, dt * _camMoveSpeed);
                if (Vector3.Distance(_cam.transform.position, _targetCamPos) < 0.02f)
                {
                    _focusMode = false;
                }
            }

            // Update mechanisms smoothly (doors, valves, LEDs, bunker hatch)
            for (int i = 0; i < Catalog.Length; i++)
            {
                var item = Catalog[i];
                if (item != null)
                {
                    if (item.Mechanism)
                    {
                        item.Mechanism.SampleState(item.IsOpen, true, dt);
                    }
                    else if (item.DoorLeafTransform)
                    {
                        Quaternion targetLeafRot = item.DoorClosedRot * Quaternion.Euler(item.IsOpen ? -105f : 0f, 0f, 0f);
                        item.DoorLeafTransform.localRotation = Quaternion.Slerp(item.DoorLeafTransform.localRotation, targetLeafRot, dt * 6f);
                    }
                }
            }

            if (_cutsceneActive)
            {
                TickBunkerCutscenePreview(dt);
            }
        }

        void HandleKeyboardNavigation()
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                SelectNext(1);
            }
            else if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SelectNext(-1);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleCurrentMechanism();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                StartBunkerCutscenePreview();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ResetCurrentRotation(); // Reset rotation to front-facing
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                _autoRotate = !_autoRotate; // Toggle continuous rotation
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                FocusOnItem(_selectedIndex);
            }
        }

        void StartBunkerCutscenePreview()
        {
            int bunkerIdx = Array.FindIndex(Catalog, c => c != null && c.AssetName == "bunkerhatch");
            if (bunkerIdx < 0) return;
            _selectedIndex = bunkerIdx;
            var item = Catalog[bunkerIdx];
            if (!item.Instance) return;

            StopBunkerCutscenePreview();
            _cutsceneActive = true;
            _cutsceneAge = 0f;
            item.IsOpen = false;
            if (item.Mechanism) item.Mechanism.SampleState(false, true, 10f);
            item.Instance.transform.rotation = Quaternion.identity;

            if (_cam)
            {
                Vector3 center = item.BasePosition + new Vector3(0f, 0.25f, 0f);
                _targetCamPos = item.BasePosition + new Vector3(1.35f, 1.65f, -2.45f);
                _targetCamRot = Quaternion.LookRotation(center - _targetCamPos);
                _focusMode = true;
            }

            _cutsceneBag = PanicCuratedAssetArt.CreateEmergencyBackpack();
            if (_cutsceneBag)
            {
                _cutsceneBag.name = "ShowcaseCutscene_Bag";
                _cutsceneBag.transform.SetParent(item.Instance.transform, false);
                _cutsceneBag.transform.localScale *= 0.72f;
            }

            var survivorPrefab = Resources.Load<GameObject>("Player/SurvivorV2");
            if (survivorPrefab)
            {
                _cutsceneActor = Instantiate(survivorPrefab, item.Instance.transform, false);
                _cutsceneActor.name = "ShowcaseCutscene_Survivor";
                _cutsceneActor.transform.localScale = Vector3.one * 0.78f;
                foreach (var a in _cutsceneActor.GetComponentsInChildren<Animator>(true)) a.applyRootMotion = false;
            }
        }

        void StopBunkerCutscenePreview()
        {
            _cutsceneActive = false;
            _cutsceneAge = 0f;
            if (_cutsceneBag) { if (Application.isPlaying) Destroy(_cutsceneBag); else DestroyImmediate(_cutsceneBag); _cutsceneBag = null; }
            if (_cutsceneActor) { if (Application.isPlaying) Destroy(_cutsceneActor); else DestroyImmediate(_cutsceneActor); _cutsceneActor = null; }
        }

        void TickBunkerCutscenePreview(float dt)
        {
            int bunkerIdx = Array.FindIndex(Catalog, c => c != null && c.AssetName == "bunkerhatch");
            if (bunkerIdx < 0 || !Catalog[bunkerIdx].Instance) { StopBunkerCutscenePreview(); return; }
            var item = Catalog[bunkerIdx];
            _cutsceneAge += dt;

            Vector3 localStand = new Vector3(0f, 0.02f, -0.78f);
            Vector3 localWellMouth = new Vector3(0f, 0.24f, 0f);
            Vector3 localWellDeep = new Vector3(0f, -1.25f, 0f);

            // Stage 1 (0.0s .. 1.35s): Unlock wheel spins & heavy hatch lid swings open
            if (_cutsceneAge < 1.35f)
            {
                if (_cutsceneAge >= 0.12f) item.IsOpen = true;
                if (_cutsceneActor)
                {
                    _cutsceneActor.SetActive(true);
                    _cutsceneActor.transform.localPosition = localStand;
                    _cutsceneActor.transform.localRotation = Quaternion.identity;
                }
                if (_cutsceneBag)
                {
                    _cutsceneBag.SetActive(true);
                    _cutsceneBag.transform.localPosition = localStand + new Vector3(0.14f, 0.68f, 0.22f);
                    _cutsceneBag.transform.localRotation = Quaternion.Euler(12f, 0f, -10f);
                    ApplyShowcaseWellDarkening(_cutsceneBag, _cutsceneBag.transform.localPosition.y);
                }
            }
            // Stage 2 (1.35s .. 2.25s): Throw emergency backpack on a ballistic arc into the dark well
            else if (_cutsceneAge < 2.25f)
            {
                float u = Mathf.Clamp01((_cutsceneAge - 1.35f) / 0.90f);
                if (_cutsceneActor)
                {
                    _cutsceneActor.transform.localPosition = localStand;
                    _cutsceneActor.transform.localRotation = Quaternion.Euler(Mathf.Sin(u * Mathf.PI) * 14f, 0f, 0f);
                }
                if (_cutsceneBag)
                {
                    Vector3 start = localStand + new Vector3(0.14f, 0.68f, 0.22f);
                    Vector3 pos = Vector3.Lerp(start, localWellDeep, u);
                    pos.y += 4f * u * (1f - u) * 0.52f;
                    _cutsceneBag.transform.localPosition = pos;
                    _cutsceneBag.transform.localRotation = Quaternion.Euler(u * 280f, 15f, -u * 60f);
                    ApplyShowcaseWellDarkening(_cutsceneBag, pos.y);
                    _cutsceneBag.SetActive(pos.y > -1.05f);
                }
            }
            // Stage 3 (2.25s .. 3.30s): Character crouches and jumps into the dark circular well
            else if (_cutsceneAge < 3.30f)
            {
                if (_cutsceneBag) _cutsceneBag.SetActive(false);
                float j = Mathf.Clamp01((_cutsceneAge - 2.25f) / 1.05f);
                if (_cutsceneActor)
                {
                    if (j < 0.18f)
                    {
                        float c = j / 0.18f;
                        _cutsceneActor.transform.localPosition = localStand + Vector3.down * (0.07f * Mathf.Sin(c * Mathf.PI));
                    }
                    else
                    {
                        float p = (j - 0.18f) / 0.82f;
                        Vector3 horiz = Vector3.Lerp(localStand, new Vector3(localWellMouth.x, localStand.y, localWellMouth.z), Mathf.Clamp01(p * 1.4f));
                        float y = Mathf.Lerp(localStand.y, localWellDeep.y, p * p) + 4f * p * (1f - p) * 0.48f;
                        Vector3 pos = new Vector3(horiz.x, y, horiz.z);
                        _cutsceneActor.transform.localPosition = pos;
                        ApplyShowcaseWellDarkening(_cutsceneActor, pos.y + 0.55f);
                        _cutsceneActor.SetActive(pos.y > -1.10f);
                    }
                }
            }
            else
            {
                StopBunkerCutscenePreview();
            }
        }

        static void ApplyShowcaseWellDarkening(GameObject target, float localY)
        {
            if (!target) return;
            float depthBelowRim = Mathf.Max(0f, 0.18f - localY);
            float brightness = Mathf.Exp(-depthBelowRim * 3.8f);
            var block = new MaterialPropertyBlock();
            foreach (var r in target.GetComponentsInChildren<Renderer>(true))
            {
                if (!r || !r.sharedMaterial || !r.sharedMaterial.HasProperty("_Color")) continue;
                r.GetPropertyBlock(block);
                Color c = r.sharedMaterial.color;
                block.SetColor("_Color", new Color(c.r * brightness, c.g * brightness, c.b * brightness, c.a));
                r.SetPropertyBlock(block);
            }
        }

        void HandleMouseInspection()
        {
            // Left click + drag anywhere in open space to rotate and inspect the focused model in 3D
            if (Input.GetMouseButton(0) && !IsMouseOverUI())
            {
                if (_selectedIndex >= 0 && _selectedIndex < Catalog.Length)
                {
                    var item = Catalog[_selectedIndex];
                    if (item != null && item.Instance)
                    {
                        float rotX = Input.GetAxis("Mouse X") * 5f;
                        float rotY = Input.GetAxis("Mouse Y") * 5f;
                        item.Instance.transform.Rotate(Vector3.up, -rotX, Space.World);
                        item.Instance.transform.Rotate(_cam.transform.right, rotY, Space.World);
                    }
                }
            }
        }

        bool IsMouseOverUI()
        {
            // Check if mouse is hovering over top bar or bottom cards
            Vector2 mouse = Input.mousePosition;
            if (mouse.y > Screen.height - 55) return true;
            if (mouse.y < 290)
            {
                if (mouse.x < 330 || mouse.x > Screen.width - 390) return true;
            }
            return false;
        }

        void HandleFreeCamLook()
        {
            if (!_cam) return;

            // Right-click or middle-click free look
            if (Input.GetMouseButton(1))
            {
                _focusMode = false;
                _yaw += Input.GetAxis("Mouse X") * 3f;
                _pitch -= Input.GetAxis("Mouse Y") * 3f;
                _pitch = Mathf.Clamp(_pitch, -80f, 80f);
                _cam.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
            }

            // WASD Free Fly movement
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                _focusMode = false;
                float speed = _flySpeed * (Input.GetKey(KeyCode.LeftShift) ? 2.5f : 1.0f);
                Vector3 move = (_cam.transform.right * h + _cam.transform.forward * v) * (speed * Time.deltaTime);
                _cam.transform.position += move;
            }

            // Elevation with PageUp/PageDown
            if (Input.GetKey(KeyCode.PageUp)) _cam.transform.position += Vector3.up * (_flySpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.PageDown)) _cam.transform.position -= Vector3.up * (_flySpeed * Time.deltaTime);
        }

        void SelectNext(int dir)
        {
            StopBunkerCutscenePreview();
            _selectedIndex = (_selectedIndex + dir + Catalog.Length) % Catalog.Length;
            FocusOnItem(_selectedIndex);
        }

        void FocusOnItem(int index)
        {
            if (index < 0 || index >= Catalog.Length) return;
            var item = Catalog[index];
            if (!item.Instance || !_cam) return;

            Vector3 center = item.BasePosition + Vector3.up * (item.Scale * 0.45f);
            float dist = Mathf.Max(1.4f, item.Scale * 2.1f);
            if (item.AssetName == "bunkerhatch")
            {
                // Elevate camera for floor-mounted circular hatch so the user looks down into it diagonally
                _targetCamPos = item.BasePosition + new Vector3(0, 1.75f, -dist * 0.95f);
                _targetCamRot = Quaternion.LookRotation((item.BasePosition + Vector3.up * 0.15f) - _targetCamPos);
            }
            else
            {
                _targetCamPos = center + new Vector3(0, 0.05f, -dist);
                _targetCamRot = Quaternion.LookRotation(center - _targetCamPos);
            }
            _focusMode = true;

            // Default: Front faces directly towards the camera!
            item.Instance.transform.rotation = item.BaseRotation;
        }

        void ResetCurrentRotation()
        {
            StopBunkerCutscenePreview();
            if (_selectedIndex >= 0 && _selectedIndex < Catalog.Length)
            {
                var item = Catalog[_selectedIndex];
                if (item != null && item.Instance)
                {
                    item.Instance.transform.rotation = item.BaseRotation;
                }
            }
        }

        void ToggleCurrentMechanism()
        {
            if (_selectedIndex < 0 || _selectedIndex >= Catalog.Length) return;
            var item = Catalog[_selectedIndex];
            if (item != null && (item.Mechanism != null || item.DoorLeafTransform != null))
            {
                item.IsOpen = !item.IsOpen;
            }
        }

        void EnsureStyles()
        {
            if (_headerStyle != null) return;

            _panelTex = MakeColorTex(new Color(0.08f, 0.10f, 0.14f, 0.95f));
            _cardTex = MakeColorTex(new Color(0.12f, 0.15f, 0.20f, 0.95f));
            _btnTex = MakeColorTex(new Color(0.18f, 0.22f, 0.30f, 0.95f));
            _btnActiveTex = MakeColorTex(new Color(0.85f, 0.65f, 0.20f, 0.95f));

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _headerStyle.normal.textColor = new Color(0.95f, 0.96f, 0.98f);

            _cardStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _cardTex },
                padding = new RectOffset(16, 16, 14, 14)
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { background = _btnTex, textColor = new Color(0.9f, 0.9f, 0.95f) },
                hover = { background = _btnActiveTex, textColor = Color.black }
            };

            _tabStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                normal = { background = _btnTex, textColor = new Color(0.7f, 0.75f, 0.85f) }
            };

            _activeTabStyle = new GUIStyle(_tabStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { background = _btnActiveTex, textColor = Color.black }
            };

            _floatingBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
            _floatingBadgeStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

            _badgeSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.UpperCenter
            };
            _badgeSubStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f, 0.85f);
        }

        Texture2D MakeColorTex(Color col)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        void OnGUI()
        {
            EnsureStyles();

            // Top Bar: Categories & Title
            DrawTopBar();

            // World Space Floating Badges for models
            DrawWorldBadges();

            // Bottom Right: Detailed Item Inspector Card
            DrawItemDetailsCard();

            // Bottom Left: Quick Control Instructions
            DrawControlsGuide();
        }

        void DrawTopBar()
        {
            GUI.Box(new Rect(0, 0, Screen.width, 50), GUIContent.none, _cardStyle);

            GUI.Label(new Rect(20, 10, 280, 30), "TOO LATE TO PANIC · ASSET VİTRİNİ", _headerStyle);

            string[] tabs = { "Tümü (34)", "Afet Çantası & Erzak (16)", "Mobilya & Eşyalar (9)", "Tesisat & Güvenlik (9)" };
            float startX = 320;
            float tabWidth = 175;

            for (int i = 0; i < tabs.Length; i++)
            {
                bool isSelected = _selectedCategory == i;
                if (GUI.Button(new Rect(startX + i * (tabWidth + 8), 10, tabWidth, 30), tabs[i], isSelected ? _activeTabStyle : _tabStyle))
                {
                    _selectedCategory = i;
                    FilterToCategory(i);
                }
            }

            // Auto-Rotate Toggle (Default: OFF)
            if (GUI.Button(new Rect(Screen.width - 240, 10, 105, 30), _autoRotate ? "Dönüş: AÇIK" : "Dönüş: KAPALI", _buttonStyle))
            {
                _autoRotate = !_autoRotate;
            }

            if (GUI.Button(new Rect(Screen.width - 125, 10, 105, 30), "Kuşbakışı", _buttonStyle))
            {
                _targetCamPos = new Vector3(0, 14f, -14f);
                _targetCamRot = Quaternion.Euler(45f, 0, 0);
                _focusMode = true;
            }
        }

        void FilterToCategory(int catIndex)
        {
            StopBunkerCutscenePreview();
            string filterCat = catIndex == 1 ? "Erzak & Çanta" : catIndex == 2 ? "Mobilya & Eşyalar" : catIndex == 3 ? "Tesisat & Güvenlik" : "";
            for (int i = 0; i < Catalog.Length; i++)
            {
                if (string.IsNullOrEmpty(filterCat) || Catalog[i].Category == filterCat)
                {
                    _selectedIndex = i;
                    FocusOnItem(i);
                    break;
                }
            }
        }

        void DrawWorldBadges()
        {
            if (!_cam) return;

            for (int i = 0; i < Catalog.Length; i++)
            {
                var item = Catalog[i];
                if (!item.Instance) continue;

                Vector3 headPos = item.BasePosition + Vector3.up * (item.Scale * 0.55f + 0.35f);
                Vector3 screenPos = _cam.WorldToScreenPoint(headPos);

                if (screenPos.z > 0 && screenPos.z < 25f)
                {
                    float dist = screenPos.z;
                    if (dist > 15f && i != _selectedIndex) continue;

                    float y = Screen.height - screenPos.y;
                    float width = 160;
                    Rect badgeRect = new Rect(screenPos.x - width / 2, y, width, 20);
                    Rect subRect = new Rect(screenPos.x - width / 2, y + 16, width, 18);

                    string title = (i == _selectedIndex ? "▶ " : "") + item.TurkishName;
                    GUI.Label(badgeRect, title, _floatingBadgeStyle);
                    GUI.Label(subRect, item.AssetName, _badgeSubStyle);
                }
            }
        }

        void DrawItemDetailsCard()
        {
            if (_selectedIndex < 0 || _selectedIndex >= Catalog.Length) return;
            var item = Catalog[_selectedIndex];

            float cardWidth = 360;
            float cardHeight = item.AssetName == "bunkerhatch" ? 286 : 250;
            Rect cardRect = new Rect(Screen.width - cardWidth - 20, Screen.height - cardHeight - 20, cardWidth, cardHeight);

            GUI.Box(cardRect, GUIContent.none, _cardStyle);

            GUILayout.BeginArea(new Rect(cardRect.x + 15, cardRect.y + 12, cardWidth - 30, cardHeight - 24));

            GUILayout.Label(item.TurkishName, _headerStyle);
            GUILayout.Space(2);
            string displayAsset = item.AssetName == "bunkerhatch" ? "BunkerHatch" : item.AssetName;
            GUILayout.Label("Model: SM_" + displayAsset + "  |  Prefab: PF_" + displayAsset, _badgeSubStyle);
            GUILayout.Label("Kategori: " + item.Category + "  |  Ölçek: " + item.Scale.ToString("F2") + "m", _badgeSubStyle);
            GUILayout.Space(4);

            GUILayout.Label(item.Description, GUI.skin.label);
            GUILayout.FlexibleSpace();

            // Mechanism animation button (if doors/valves/hatch exist)
            if (item.Mechanism != null || item.DoorLeafTransform != null)
            {
                string animLabel = item.IsOpen ? "Kapağı / Vanayı Kapat [Space]" : "Kapağı / Vanayı Aç [Space]";
                if (GUILayout.Button(animLabel, _buttonStyle, GUILayout.Height(30)))
                {
                    ToggleCurrentMechanism();
                }
            }

            if (item.AssetName == "bunkerhatch")
            {
                if (GUILayout.Button("Cutscene Oynat: Aç + Çanta At + Atla [C]", _buttonStyle, GUILayout.Height(28)))
                {
                    StartBunkerCutscenePreview();
                }
            }

            // Reset orientation button
            if (GUILayout.Button("Ön Yüze Sıfırla [R]", _buttonStyle, GUILayout.Height(26)))
            {
                ResetCurrentRotation();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◀ Önceki (Q)", _buttonStyle, GUILayout.Height(30))) SelectNext(-1);
            if (GUILayout.Button("Odaklan (F)", _buttonStyle, GUILayout.Height(30))) FocusOnItem(_selectedIndex);
            if (GUILayout.Button("Sonraki (E) ▶", _buttonStyle, GUILayout.Height(30))) SelectNext(1);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        void DrawControlsGuide()
        {
            float width = 310;
            float height = 175;
            Rect rect = new Rect(20, Screen.height - height - 20, width, height);

            GUI.Box(rect, GUIContent.none, _cardStyle);

            GUILayout.BeginArea(new Rect(rect.x + 12, rect.y + 10, width - 24, height - 20));
            GUILayout.Label("KONTROLLER", _headerStyle);
            GUILayout.Space(4);
            GUILayout.Label("• Sol Tık + Sürükle: Modeli 360° İncele", _badgeSubStyle);
            GUILayout.Label("• Q / E veya Sol / Sağ Ok: Eşyalar arası geçiş", _badgeSubStyle);
            GUILayout.Label("• R: Ön yüze sıfırla (Bana baksın)", _badgeSubStyle);
            GUILayout.Label("• Space: Kapak / Vana açma animasyonu", _badgeSubStyle);
            GUILayout.Label("• C: Sığınak Cutscene (Aç + Çanta At + Atla)", _badgeSubStyle);
            GUILayout.Label("• W, A, S, D + Sağ Tık: Serbest kamera", _badgeSubStyle);
            GUILayout.Label("• T: Otomatik döndürmeyi aç / kapat", _badgeSubStyle);
            GUILayout.EndArea();
        }
    }
}
