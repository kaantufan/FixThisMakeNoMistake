using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YarkinAssets.Runtime
{
    /// <summary>
    /// Sığınak Kapağı (PF_BunkerHatch) Cutscene Kontrolcüsü:
    /// 1. Karakter zemindeki ağır yuvarlak sığınak kapağının kilidini çözer ve kalın menteşesinden açar.
    /// 2. Acil durum çantasını (PF_EmergencyBackpack) balistik parabolik yörüngeyle karanlık kuyunun içine atar.
    /// 3. Karakter ön basamaktan sıçrayarak karanlık kuyunun içine atlar ve derinlikte gözden kaybolur.
    /// Hem Play Mode'da hem de Edit Mode'da (Scene View) tek tıkla test edilebilir.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BunkerCutsceneController : MonoBehaviour
    {
        [Header("Bunker Referansları")]
        public DisasterPrepItem bunkerHatchItem;
        public GameObject emergencyBackpackPrefab;

        [Header("Cutscene Noktaları (Local Anchor Points)")]
        [Tooltip("Karakterin kapağı açarken durduğu ön basamak noktası")]
        public Transform actorStandPoint;
        [Tooltip("Çantanın elden çıktığı fırlatma başlangıç noktası")]
        public Transform bagThrowStartPoint;
        [Tooltip("Yuvarlak kuyu ağzı merkez giriş noktası")]
        public Transform wellEntryPoint;
        [Tooltip("Karanlık kuyunun 1.8m altındaki derin hedef noktası")]
        public Transform wellDeepTargetPoint;

        [Header("Cutscene Zamanlama & Fizik Ayarları")]
        [Tooltip("1. Faz: Ağır kapağın kilidini çözme ve menteşeden açma süresi (sn)")]
        public float phase1OpenDuration = 1.65f;
        [Tooltip("2. Faz: Acil durum çantasını kuyuya fırlatma süresi (sn)")]
        public float phase2ThrowBagDuration = 1.05f;
        [Tooltip("3. Faz: Karakterin kuyunun içine sıçrayıp atlama süresi (sn)")]
        public float phase3CharacterJumpDuration = 1.15f;
        [Tooltip("Karakter kuyuya atladıktan sonra kapak içeriden kapansın mı?")]
        public bool closeHatchAfterJump = false;
        [Tooltip("4. Faz (Opsiyonel): Kapağın içeriden kapanma süresi (sn)")]
        public float phase4CloseDuration = 0.95f;

        private bool _isPlayingCutscene;
        private float _cutsceneElapsed;
        private double _lastUpdateRealtime;

        private Transform _activeActorRoot;
        private Transform _activeActorTorso;
        private Transform _activeActorLeftArm;
        private Transform _activeActorRightArm;
        private Transform _activeBackpack;
        private bool _spawnedTempObjects;

        private MaterialPropertyBlock _abyssPropBlock;
        private static readonly int ColorPropId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorPropId = Shader.PropertyToID("_BaseColor");

        public bool IsPlayingCutscene => _isPlayingCutscene;

        public float TotalCutsceneDuration =>
            phase1OpenDuration + phase2ThrowBagDuration + phase3CharacterJumpDuration + (closeHatchAfterJump ? phase4CloseDuration : 0f);

        private void OnEnable()
        {
            if (bunkerHatchItem == null)
            {
                bunkerHatchItem = GetComponent<DisasterPrepItem>();
            }
            EnsureAnchorPoints();
        }

        private void OnDisable()
        {
            CleanupSpawnedPreviewObjects();
        }

        public void EnsureAnchorPoints()
        {
            if (actorStandPoint == null)
            {
                actorStandPoint = FindOrCreateAnchor("ActorStandPoint", new Vector3(0f, 0f, 1.05f), Quaternion.Euler(0f, 180f, 0f));
            }
            if (bagThrowStartPoint == null)
            {
                bagThrowStartPoint = FindOrCreateAnchor("BagThrowStartPoint", new Vector3(0.18f, 0.94f, 0.72f), Quaternion.Euler(0f, 180f, 0f));
            }
            if (wellEntryPoint == null)
            {
                wellEntryPoint = FindOrCreateAnchor("WellEntryPoint", new Vector3(0f, 0.45f, 0f), Quaternion.identity);
            }
            if (wellDeepTargetPoint == null)
            {
                wellDeepTargetPoint = FindOrCreateAnchor("WellDeepTargetPoint", new Vector3(0f, -1.78f, 0f), Quaternion.identity);
            }
        }

        private Transform FindOrCreateAnchor(string anchorName, Vector3 localPos, Quaternion localRot)
        {
            Transform existing = transform.Find(anchorName);
            if (existing != null) return existing;

            GameObject go = new GameObject(anchorName);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            return go.transform;
        }

        /// <summary>
        /// Cutscene sekansını başlatır:
        /// 1) Karakter kapağı açar -> 2) Çantayı karanlık kuyuya atar -> 3) Kendisi kuyuya atlar.
        /// </summary>
        [ContextMenu("Test: Bunker Kaçış Cutscene Oynat (Aç -> Çanta At -> Atla)")]
        public void PlayCutscene()
        {
            PlayCutscene(null, null);
        }

        public void PlayCutscene(Transform customCharacter, Transform customBackpack)
        {
            EnsureAnchorPoints();
            if (bunkerHatchItem == null)
            {
                bunkerHatchItem = GetComponent<DisasterPrepItem>();
            }

            CleanupSpawnedPreviewObjects();

            if (customCharacter != null && customBackpack != null)
            {
                _activeActorRoot = customCharacter;
                _activeBackpack = customBackpack;
                _spawnedTempObjects = false;
            }
            else
            {
                SpawnPreviewCutsceneActors();
                _spawnedTempObjects = true;
            }

            PrepareSubterraneanRenderers(_activeActorRoot);
            PrepareSubterraneanRenderers(_activeBackpack);

            if (bunkerHatchItem != null)
            {
                bunkerHatchItem.SetCabinetDoorOpen(false, instant: true);
            }

            _cutsceneElapsed = 0f;
            _isPlayingCutscene = true;
            _lastUpdateRealtime = GetRealtime();
            EvaluateCutsceneTime(0f);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
#endif
        }

        [ContextMenu("Test: Cutscene Sıfırla & Temizle")]
        public void StopAndResetCutscene()
        {
            _isPlayingCutscene = false;
            CleanupSpawnedPreviewObjects();
            if (bunkerHatchItem != null)
            {
                bunkerHatchItem.SetCabinetDoorOpen(false, instant: true);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SceneView.RepaintAll();
            }
#endif
        }

        /// <summary>
        /// Belirli bir normalize cutscene anını (0..1) anında sahneye uygular (Önizleme / Thumbnail / Scrubbing için).
        /// </summary>
        public void PreviewCutsceneAtNormalizedTime(float normalizedTime)
        {
            EnsureAnchorPoints();
            if (bunkerHatchItem == null)
            {
                bunkerHatchItem = GetComponent<DisasterPrepItem>();
            }
            if (_activeActorRoot == null || _activeBackpack == null)
            {
                SpawnPreviewCutsceneActors();
                _spawnedTempObjects = true;
                PrepareSubterraneanRenderers(_activeActorRoot);
                PrepareSubterraneanRenderers(_activeBackpack);
            }

            _isPlayingCutscene = false;
            float t = Mathf.Clamp01(normalizedTime) * TotalCutsceneDuration;
            EvaluateCutsceneTime(t);
        }

        private void LateUpdate()
        {
            if (!_isPlayingCutscene) return;

            double now = GetRealtime();
            float dt = Mathf.Clamp((float)(now - _lastUpdateRealtime), 0.0001f, 0.05f);
            _lastUpdateRealtime = now;

            _cutsceneElapsed += dt;
            float total = TotalCutsceneDuration;

            if (_cutsceneElapsed >= total)
            {
                EvaluateCutsceneTime(total);
                _isPlayingCutscene = false;
                if (_spawnedTempObjects)
                {
                    // Keep hidden at bottom of well until reset or clean up automatically
                    CleanupSpawnedPreviewObjects();
                }
            }
            else
            {
                EvaluateCutsceneTime(_cutsceneElapsed);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                }
#endif
            }
        }

        private void EvaluateCutsceneTime(float t)
        {
            float t1End = phase1OpenDuration;
            float t2End = t1End + phase2ThrowBagDuration;
            float t3End = t2End + phase3CharacterJumpDuration;
            float t4End = t3End + (closeHatchAfterJump ? phase4CloseDuration : 0f);

            Vector3 standPos = actorStandPoint != null ? actorStandPoint.localPosition : new Vector3(0f, 0f, 1.05f);
            Vector3 bagStartPos = bagThrowStartPoint != null ? bagThrowStartPoint.localPosition : new Vector3(0.18f, 0.94f, 0.72f);
            Vector3 entryPos = wellEntryPoint != null ? wellEntryPoint.localPosition : new Vector3(0f, 0.45f, 0f);
            Vector3 deepPos = wellDeepTargetPoint != null ? wellDeepTargetPoint.localPosition : new Vector3(0f, -1.78f, 0f);

            // -----------------------------------------------------------------
            // 1. BUNKER HATCH LID & WHEEL POSE
            // -----------------------------------------------------------------
            if (bunkerHatchItem != null)
            {
                if (t <= t1End)
                {
                    float uOpen = Mathf.Clamp01(t / Mathf.Max(0.01f, phase1OpenDuration));
                    ApplyHatchProgressImmediate(uOpen);
                }
                else if (t <= t3End || !closeHatchAfterJump)
                {
                    ApplyHatchProgressImmediate(1f);
                }
                else
                {
                    float uClose = 1f - Mathf.Clamp01((t - t3End) / Mathf.Max(0.01f, phase4CloseDuration));
                    ApplyHatchProgressImmediate(uClose);
                }
            }

            // -----------------------------------------------------------------
            // 2. EMERGENCY BACKPACK TRAJECTORY (Phase 1 held -> Phase 2 thrown into well)
            // -----------------------------------------------------------------
            if (_activeBackpack != null)
            {
                if (t <= t1End)
                {
                    // Resting by character's right side while unlocking & heaving hatch
                    float u1 = Mathf.Clamp01(t / Mathf.Max(0.01f, phase1OpenDuration));
                    Vector3 sideHold = standPos + new Vector3(0.28f, 0.58f + 0.06f * Mathf.Sin(u1 * Mathf.PI), -0.10f);
                    _activeBackpack.position = transform.TransformPoint(sideHold);
                    _activeBackpack.rotation = transform.rotation * Quaternion.Euler(14f, 165f, -12f);
                    _activeBackpack.localScale = Vector3.one;
                    ApplyWellDepthDarkening(_activeBackpack, sideHold.y);
                }
                else if (t <= t2End)
                {
                    float u2 = Mathf.Clamp01((t - t1End) / Mathf.Max(0.01f, phase2ThrowBagDuration));
                    if (u2 < 0.22f)
                    {
                        // Wind-up swing from side to two-handed throw release point
                        float w = u2 / 0.22f;
                        float smoothW = w * w * (3f - 2f * w);
                        Vector3 sideHold = standPos + new Vector3(0.28f, 0.58f, -0.10f);
                        Vector3 posLocal = Vector3.Lerp(sideHold, bagStartPos, smoothW);
                        _activeBackpack.position = transform.TransformPoint(posLocal);
                        _activeBackpack.rotation = transform.rotation * Quaternion.Slerp(
                            Quaternion.Euler(14f, 165f, -12f),
                            Quaternion.Euler(-22f, 180f, 0f),
                            smoothW);
                        _activeBackpack.localScale = Vector3.one;
                        ApplyWellDepthDarkening(_activeBackpack, posLocal.y);
                    }
                    else
                    {
                        // Parabolic ballistic throw into the center of the dark circular well shaft
                        float tau = (u2 - 0.22f) / 0.78f;
                        // Horizontal (X,Z) reaches well center (0,0) quickly (by tau=0.42) so it cleanly clears the steel collar
                        float horizT = Mathf.Clamp01(tau / 0.42f);
                        float smoothHoriz = 1f - Mathf.Pow(1f - horizT, 2.2f);
                        float x = Mathf.Lerp(bagStartPos.x, deepPos.x, smoothHoriz);
                        float z = Mathf.Lerp(bagStartPos.z, deepPos.z, smoothHoriz);

                        // Vertical ballistic arc: upward toss apex (+1.28m) followed by 9.81 m/s^2 gravity plunge down to -1.78m
                        float y = Mathf.Lerp(bagStartPos.y, deepPos.y, tau * tau)
                                  + Mathf.Sin(tau * Mathf.PI) * 0.46f * (1f - tau * 0.5f);

                        Vector3 bagLocalPos = new Vector3(x, y, z);
                        _activeBackpack.position = transform.TransformPoint(bagLocalPos);
                        _activeBackpack.rotation = transform.rotation * Quaternion.Euler(
                            -22f + tau * 380f,
                            180f + tau * 140f,
                            tau * 65f);

                        // Slight perspective shrink deep in the shaft
                        float depthShrink = Mathf.Lerp(1f, 0.78f, Mathf.Clamp01((-y) / 1.8f));
                        _activeBackpack.localScale = Vector3.one * depthShrink;
                        ApplyWellDepthDarkening(_activeBackpack, y);
                    }
                }
                else
                {
                    // Bag is at the bottom of the dark well
                    _activeBackpack.position = transform.TransformPoint(deepPos);
                    ApplyWellDepthDarkening(_activeBackpack, deepPos.y);
                }
            }

            // -----------------------------------------------------------------
            // 3. CHARACTER TRAJECTORY (Phase 1 heave hatch -> Phase 2 throw -> Phase 3 jump into well)
            // -----------------------------------------------------------------
            if (_activeActorRoot != null)
            {
                Vector3 actorLocalPos = standPos;
                Quaternion actorLocalRot = Quaternion.Euler(0f, 180f, 0f);
                float torsoPitch = 0f;
                float armPitch = 0f;
                float actorScale = 1f;

                if (t <= t1End)
                {
                    float u1 = Mathf.Clamp01(t / Mathf.Max(0.01f, phase1OpenDuration));
                    if (u1 < 0.28f)
                    {
                        // Reaching forward to spin the central locking wheel (-360°)
                        float w = Mathf.Sin((u1 / 0.28f) * Mathf.PI * 0.5f);
                        actorLocalPos = standPos + new Vector3(0f, 0.04f, -0.14f * w);
                        torsoPitch = 34f * w;
                        armPitch = -65f * w;
                    }
                    else
                    {
                        // Heaving the heavy 240 kg steel lid upward and backward
                        float h = (u1 - 0.28f) / 0.72f;
                        float smoothH = h * h * (3f - 2f * h);
                        actorLocalPos = standPos + new Vector3(0f, 0.04f * (1f - smoothH), -0.14f * (1f - smoothH * 0.6f));
                        torsoPitch = Mathf.Lerp(34f, -8f, smoothH);
                        armPitch = Mathf.Lerp(-65f, -115f * Mathf.Sin(h * Mathf.PI), smoothH);
                    }
                }
                else if (t <= t2End)
                {
                    // Throwing the emergency backpack into the open well
                    float u2 = Mathf.Clamp01((t - t1End) / Mathf.Max(0.01f, phase2ThrowBagDuration));
                    float throwSwing = Mathf.Sin(Mathf.Clamp01(u2 / 0.55f) * Mathf.PI);
                    actorLocalPos = standPos + new Vector3(0f, 0f, -0.12f * u2);
                    torsoPitch = 18f * throwSwing;
                    armPitch = -95f * throwSwing;
                }
                else if (t <= t3End)
                {
                    // Jumping feet-first into the dark circular bunker well
                    float u3 = Mathf.Clamp01((t - t2End) / Mathf.Max(0.01f, phase3CharacterJumpDuration));
                    if (u3 < 0.24f)
                    {
                        // Step onto front concrete step & crouch for jump take-off
                        float c = u3 / 0.24f;
                        float smoothC = c * c * (3f - 2f * c);
                        actorLocalPos = Vector3.Lerp(
                            standPos + new Vector3(0f, 0f, -0.12f),
                            new Vector3(0f, 0.05f, 0.64f),
                            smoothC);
                        torsoPitch = 24f * smoothC;
                        armPitch = 35f * smoothC;
                    }
                    else
                    {
                        // Parabolic leap over the steel collar & vertical plunge down the dark well shaft
                        float j = (u3 - 0.24f) / 0.76f;
                        // Horizontal moves to center (0,0) in the first 38% of the leap so feet clear the collar
                        float horizJ = Mathf.Clamp01(j / 0.38f);
                        float smoothHorizJ = horizJ * horizJ * (3f - 2f * horizJ);
                        float z = Mathf.Lerp(0.64f, 0f, smoothHorizJ);

                        // Vertical leap curve: rises to +0.62m apex at j=0.26, then plunges to -1.82m
                        float y;
                        if (j < 0.28f)
                        {
                            float upT = j / 0.28f;
                            y = Mathf.Lerp(0.05f, 0.62f, 1f - (1f - upT) * (1f - upT));
                        }
                        else
                        {
                            float downT = (j - 0.28f) / 0.72f;
                            y = Mathf.Lerp(0.62f, deepPos.y - 0.12f, downT * downT);
                        }

                        actorLocalPos = new Vector3(0f, y, z);
                        torsoPitch = Mathf.Lerp(24f, 0f, horizJ);
                        armPitch = Mathf.Lerp(35f, -145f, Mathf.Clamp01(j * 1.4f));
                        actorScale = Mathf.Lerp(1f, 0.76f, Mathf.Clamp01((-y) / 1.85f));
                    }
                }
                else
                {
                    actorLocalPos = deepPos + new Vector3(0f, -0.15f, 0f);
                    actorScale = 0.75f;
                }

                _activeActorRoot.position = transform.TransformPoint(actorLocalPos);
                _activeActorRoot.rotation = transform.rotation * actorLocalRot;
                _activeActorRoot.localScale = Vector3.one * actorScale;

                if (_activeActorTorso != null)
                {
                    _activeActorTorso.localRotation = Quaternion.Euler(torsoPitch, 0f, 0f);
                }
                if (_activeActorLeftArm != null)
                {
                    _activeActorLeftArm.localRotation = Quaternion.Euler(armPitch, 0f, 0f);
                }
                if (_activeActorRightArm != null)
                {
                    _activeActorRightArm.localRotation = Quaternion.Euler(armPitch, 0f, 0f);
                }

                ApplyWellDepthDarkening(_activeActorRoot, actorLocalPos.y);
            }
        }

        private void ApplyHatchProgressImmediate(float uDoor)
        {
            if (bunkerHatchItem == null || bunkerHatchItem.cabinetDoorHinge == null) return;

            bunkerHatchItem.isCabinetDoorOpen = uDoor >= 0.5f;
            float openTargetAngleX = -Mathf.Abs(bunkerHatchItem.doorOpenAngleY > 1f ? bunkerHatchItem.doorOpenAngleY : 108f);
            float sLid = DisasterPrepItem.EvaluateBunkerHatchLidCurve(uDoor);
            float lidAngleX = sLid * openTargetAngleX;
            Quaternion lidRot = Quaternion.Euler(lidAngleX, 0f, 0f);
            bunkerHatchItem.cabinetDoorHinge.localRotation = lidRot;

            if (bunkerHatchItem.secondaryMovingPart != null)
            {
                float sWheel = DisasterPrepItem.EvaluateBunkerWheelSpinCurve(uDoor);
                float wheelSpinY = -360f * sWheel;
                float wheelLiftY = 0.018f * sWheel;
                Vector3 hingePivotPos = bunkerHatchItem.cabinetDoorHinge.localPosition;
                Vector3 wheelBasePos = new Vector3(0f, 0.535f, 0f);
                Vector3 wheelOffset = (wheelBasePos - hingePivotPos) + new Vector3(0f, wheelLiftY, 0f);
                bunkerHatchItem.secondaryMovingPart.localPosition = hingePivotPos + (lidRot * wheelOffset);
                bunkerHatchItem.secondaryMovingPart.localRotation = lidRot * Quaternion.Euler(0f, wheelSpinY, 0f);
            }

            if (bunkerHatchItem.tertiaryMovingPart != null)
            {
                float sPiston = Mathf.Clamp01(sLid);
                bunkerHatchItem.tertiaryMovingPart.localRotation = Quaternion.Euler(-36f * sPiston, 0f, 0f);
                bunkerHatchItem.tertiaryMovingPart.localPosition = new Vector3(0f, 0.455f + 0.042f * sPiston, -0.44f - 0.052f * sPiston);
            }
        }

        private readonly System.Collections.Generic.Dictionary<Material, Color> _originalMatColors =
            new System.Collections.Generic.Dictionary<Material, Color>();

        /// <summary>
        /// Ensures any object descending into the bunker well renders at Queue = 2005 (after BunkerPortalMask clears the y=0 floor depth at Queue = 2001)
        /// and progressively darkens into the pitch-black abyss as it falls deeper below the well rim while preserving its original palette colors.
        /// </summary>
        private void PrepareSubterraneanRenderers(Transform root)
        {
            if (root == null) return;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                Material[] mats = r.sharedMaterials;
                Material[] newMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null)
                    {
                        Color origCol = mats[i].HasProperty(ColorPropId) ? mats[i].color : Color.white;
                        Material instMat = new Material(mats[i])
                        {
                            name = mats[i].name + "_CutsceneInst",
                            hideFlags = HideFlags.DontSave,
                            renderQueue = 2005,
                            color = origCol
                        };
                        _originalMatColors[instMat] = origCol;
                        newMats[i] = instMat;
                    }
                }
                r.sharedMaterials = newMats;
            }
        }

        private void ApplyWellDepthDarkening(Transform root, float localY)
        {
            if (root == null) return;

            // Above y = +0.28m (collar top): 100% lit. Below +0.28m down to -1.65m: exponential darkening to pitch-black (#010204)
            float depthBelowCollar = Mathf.Max(0f, 0.28f - localY);
            float brightness = Mathf.Exp(-depthBelowCollar * 1.85f) * Mathf.Clamp01(1f - (depthBelowCollar / 1.75f));
            Color abyssCol = new Color(0.005f, 0.008f, 0.015f, 1f);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Material m = mats[i];
                    if (m != null && _originalMatColors.TryGetValue(m, out Color origCol))
                    {
                        m.color = Color.Lerp(abyssCol, origCol, brightness);
                    }
                }
            }
        }

        private void SpawnPreviewCutsceneActors()
        {
            CleanupSpawnedPreviewObjects();

            // 1. Spawn Emergency Backpack (from PF_EmergencyBackpack prefab if assigned, otherwise procedural pack)
            if (emergencyBackpackPrefab != null)
            {
                GameObject bagGo = Instantiate(emergencyBackpackPrefab, transform);
                bagGo.name = "__CutscenePreview_EmergencyBackpack";
                bagGo.hideFlags = HideFlags.DontSave;
                Collider col = bagGo.GetComponent<Collider>();
                if (col != null) col.enabled = false;
                _activeBackpack = bagGo.transform;
            }
            else
            {
                GameObject bagGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bagGo.name = "__CutscenePreview_EmergencyBackpack";
                bagGo.hideFlags = HideFlags.DontSave;
                bagGo.transform.SetParent(transform, false);
                bagGo.transform.localScale = new Vector3(0.36f, 0.46f, 0.22f);
                DestroyImmediateIfEditor(bagGo.GetComponent<Collider>());
                _activeBackpack = bagGo.transform;
            }

            // 2. Spawn Stylized Survivor Character Mannequin for immediate visual preview of the 3-stage cutscene
            GameObject actorGo = new GameObject("__CutscenePreview_SurvivorActor");
            actorGo.hideFlags = HideFlags.DontSave;
            actorGo.transform.SetParent(transform, false);
            _activeActorRoot = actorGo.transform;

            GameObject torsoPivot = new GameObject("TorsoPivot");
            torsoPivot.transform.SetParent(_activeActorRoot, false);
            torsoPivot.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            _activeActorTorso = torsoPivot.transform;

            Shader stdShader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            Material jacketMat = new Material(stdShader) { color = new Color(0.18f, 0.36f, 0.56f, 1f), renderQueue = 2005 };
            Material pantsMat = new Material(stdShader) { color = new Color(0.16f, 0.19f, 0.24f, 1f), renderQueue = 2005 };
            Material skinMat = new Material(stdShader) { color = new Color(0.92f, 0.74f, 0.60f, 1f), renderQueue = 2005 };
            Material helmetMat = new Material(stdShader) { color = new Color(0.95f, 0.72f, 0.12f, 1f), renderQueue = 2005 };

            // Legs
            CreateLimbPrimitive(_activeActorRoot, "LeftLeg", PrimitiveType.Capsule, new Vector3(-0.12f, 0.36f, 0f), new Vector3(0.14f, 0.36f, 0.14f), pantsMat);
            CreateLimbPrimitive(_activeActorRoot, "RightLeg", PrimitiveType.Capsule, new Vector3(0.12f, 0.36f, 0f), new Vector3(0.14f, 0.36f, 0.14f), pantsMat);

            // Torso & Head
            CreateLimbPrimitive(_activeActorTorso, "TorsoMesh", PrimitiveType.Capsule, new Vector3(0f, 0.26f, 0f), new Vector3(0.34f, 0.32f, 0.22f), jacketMat);
            CreateLimbPrimitive(_activeActorTorso, "HeadMesh", PrimitiveType.Sphere, new Vector3(0f, 0.72f, 0f), new Vector3(0.24f, 0.26f, 0.24f), skinMat);
            CreateLimbPrimitive(_activeActorTorso, "HardHat", PrimitiveType.Sphere, new Vector3(0f, 0.79f, 0f), new Vector3(0.28f, 0.16f, 0.29f), helmetMat);

            // Arms
            GameObject leftArmPivot = new GameObject("LeftArmPivot");
            leftArmPivot.transform.SetParent(_activeActorTorso, false);
            leftArmPivot.transform.localPosition = new Vector3(-0.23f, 0.46f, 0f);
            _activeActorLeftArm = leftArmPivot.transform;
            CreateLimbPrimitive(_activeActorLeftArm, "LeftArmMesh", PrimitiveType.Capsule, new Vector3(0f, -0.20f, 0.08f), new Vector3(0.11f, 0.24f, 0.11f), jacketMat);

            GameObject rightArmPivot = new GameObject("RightArmPivot");
            rightArmPivot.transform.SetParent(_activeActorTorso, false);
            rightArmPivot.transform.localPosition = new Vector3(0.23f, 0.46f, 0f);
            _activeActorRightArm = rightArmPivot.transform;
            CreateLimbPrimitive(_activeActorRightArm, "RightArmMesh", PrimitiveType.Capsule, new Vector3(0f, -0.20f, 0.08f), new Vector3(0.11f, 0.24f, 0.11f), jacketMat);
        }

        private static void CreateLimbPrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            DestroyImmediateIfEditor(go.GetComponent<Collider>());
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
        }

        public void CleanupSpawnedPreviewObjects()
        {
            if (_spawnedTempObjects)
            {
                if (_activeBackpack != null) DestroyImmediateIfEditor(_activeBackpack.gameObject);
                if (_activeActorRoot != null) DestroyImmediateIfEditor(_activeActorRoot.gameObject);
            }
            _activeBackpack = null;
            _activeActorRoot = null;
            _activeActorTorso = null;
            _activeActorLeftArm = null;
            _activeActorRightArm = null;
            _spawnedTempObjects = false;

            // Also clean up any leftover preview children by name
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != null && child.name.StartsWith("__CutscenePreview_"))
                {
                    DestroyImmediateIfEditor(child.gameObject);
                }
            }
        }

        private static void DestroyImmediateIfEditor(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        private static double GetRealtime()
        {
#if UNITY_EDITOR
            return EditorApplication.timeSinceStartup;
#else
            return Time.realtimeSinceStartupAsDouble;
#endif
        }
    }
}
