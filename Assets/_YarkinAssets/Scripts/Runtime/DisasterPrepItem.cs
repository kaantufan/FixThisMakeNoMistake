using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YarkinAssets.Runtime
{
    public enum DisasterItemCategory
    {
        FoodAndWater,
        MedicalAndHygiene,
        PowerAndLight,
        Communication,
        SurvivalGear,
        FireSafety,
        HouseholdFixture,
        UtilityValve,
        EnvironmentalHazard
    }

    public enum SecondaryHazardType
    {
        None = 0,
        FireWave = 1,
        FloodWave = 2,
        ElectricalShock = 3,
        CorridorBlockage = 4,
        FallingTrauma = 5
    }

    public enum AssetVisualState
    {
        DefaultIntact = 0,
        SecuredOrIsolated = 1,
        DamagedOrToppled = 2,
        ConsumedOrEmpty = 3
    }

    /// <summary>
    /// Fizik kurallarına ve mekanik mühendislik prensiplerine uygun açma/kapama mekanizma tipleri.
    /// </summary>
    public enum ValveMechanismKind
    {
        None = 0,
        /// <summary>Doğalgaz Ana Vanası: Koaksiyel mil etrafında +90° çeyrek tur küresel vana + Bourdon tüplü manometre ibresi.</summary>
        QuarterTurnBallValve = 1,
        /// <summary>Su Kesme Vanası & Musluk: Sağ el vida kuralına uygun +540° dönerek alçalan helisel salmastralı volan + +90° tahliye musluğu + akışkan sönümlü su sayacı türbini + yüzey gerilimli su damlası.</summary>
        RisingStemWaterValve = 2,
        /// <summary>Elektrik Sigorta Panosu: İç yatay mil (X ekseni) etrafında -34° (Yukarı/AÇIK) ile +34° (Aşağı/KAPALI) arasında bistabil devrilme yayı (over-center snap) fiziğiyle çalışan ana şalter ve kademeli tali sigortalar.</summary>
        CircuitBreakerPanel = 3,
        /// <summary>Banyo Termosifonu: Alt soğuk su giriş kelebek musluğu (+90°) + ön termostat komütatör düğmesi (-120°) + pilot LED.</summary>
        WaterHeaterIsolation = 4,
        /// <summary>Duvar Kliması: Yatay menteşe ekseni etrafında kapanan motorlu üfleme kanatçığı (louver flap) + LED ekran.</summary>
        AirConditionerLouver = 5,
        /// <summary>Zemin Sığınak Kapağı (Bunker Hatch): Merkez kilit volanı (-360°), kauçuk conta kurtulma popu, yüksek eylemsizlikli 240 kg çelik kapak menteşe açılışı (-108° X ekseni) ve çift hidrolik amortisör takibi.</summary>
        HeavyBunkerHatch = 6
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class DisasterPrepItem : MonoBehaviour
    {
        [Header("Kimlik Bilgileri")]
        public string itemId = "item_id";
        public string displayNameTr = "Eşya Adı";
        public DisasterItemCategory category = DisasterItemCategory.SurvivalGear;

        [Header("Oynanış & Envanter Değerleri")]
        [Tooltip("Envanter / Afet çantası ağırlığı (kg)")]
        public float weightKg = 0.5f;
        [Tooltip("Afet hazırlık / İMO YOTA önlem puan katkısı")]
        public int preparednessScore = 10;
        [Tooltip("Oyuncu tarafından çantaya/envantere alınabilir mi?")]
        public bool isCollectible = true;

        [Header("Sabitleme (YOTA) & Tesisat Vana Mekaniği")]
        [Tooltip("Deprem öncesinde duvara/zemine L-braket ile sabitlenebilir mi? (TV, Klima, Kitaplık, Şofben, Buzdolabı vb.)")]
        public bool isSecurableFixture = false;
        [Tooltip("Kapatılabilir tesisat vanası/şalteri mi? (Doğalgaz, Su, Elektrik Panosu)")]
        public bool isUtilityValve = false;
        [Tooltip("Şu an sabitlenmiş veya vanası kapatılmış (güvenli) durumda mı?")]
        public bool isSecuredOrClosed = false;
        [Tooltip("Sabitlenmezse veya vana kapatılmazsa Cutscene sonrasında tetikleyeceği ikincil afet dalgası")]
        public SecondaryHazardType triggersSecondaryHazard = SecondaryHazardType.None;

        [Header("Çoklu Görsel Durum (Multi-State) & Mekanik Animasyon Referansları")]
        public AssetVisualState currentState = AssetVisualState.DefaultIntact;
        public ValveMechanismKind mechanismKind = ValveMechanismKind.None;
        [Tooltip("Ana görsel model (Visual)")]
        public Transform visualRoot;
        [Tooltip("Sabitlendiğinde aktifleşen 3D Çelik L-Braket / Gergi Kayışı objesi (Varsayılan: Kapalı)")]
        public GameObject securedBracketObject;
        [Tooltip("Ana Vana Kolu / Volan / Ana Şalter Mandalı (ValveHandle_Pivot veya MainBreaker_Pivot)")]
        public Transform valveHandleTransform;
        [Tooltip("İkincil hareketli parça (Manometre İbresi, Tahliye Musluğu, Tali Sigorta 1, Termostat Düğmesi veya Üst Dondurucu Kapağı)")]
        public Transform secondaryMovingPart;
        [Tooltip("Üçüncül hareketli parça (Su Sayacı Türbin Çarkı veya Tali Sigorta 2)")]
        public Transform tertiaryMovingPart;
        [Tooltip("Dördüncül hareketli parça (Musluk Su Damlası veya Durum LED Göstergesi)")]
        public Transform quaternaryMovingPart;
        [Tooltip("Unity Animator bileşeni (Mecanim .anim klipleri ile senkronize çalışır)")]
        public Animator mechanicalAnimator;
        [Tooltip("Vana / Musluk / Sigorta açma-kapama animasyon süresi (saniye)")]
        public float valveAnimationDuration = 0.68f;

        [Header("Dolap & İçerik Mekaniği (Örn: Yangın Tüpü Dolabı & Buzdolabı)")]
        [Tooltip("Bu eşya kapağı açılabilir bir dolap mı? (Örn: PF_FireCabinet, PF_Refrigerator)")]
        public bool isContainerCabinet = false;
        [Tooltip("Dolap kapağının menteşe (Hinge) Transform referansı (CabinetDoor_Hinge)")]
        public Transform cabinetDoorHinge;
        [Tooltip("Dolap kapağı şu an açık mı?")]
        public bool isCabinetDoorOpen = false;
        [Tooltip("Kapak açıldığında Y ekseninde döneceği açı (derece)")]
        public float doorOpenAngleY = 105f;
        [Tooltip("Dolap kapağı açma-kapama animasyon süresi (saniye)")]
        public float doorAnimationDuration = 0.52f;
        [Tooltip("Dolap içindeki eşyanın (Örn: PF_FireExtinguisher) yerleştirildiği yuva")]
        public Transform containedItemSlot;
        [Tooltip("Dolap içindeki alınabilir eşya referansı (Örn: Yangın Tüpü)")]
        public DisasterPrepItem containedItem;

        [Header("Görsel & UI Referansı")]
        public Sprite inventoryIcon;

        [SerializeField, HideInInspector] private Vector3 _initialVisualLocalPos;
        [SerializeField, HideInInspector] private Quaternion _initialVisualLocalRot = Quaternion.identity;
        [SerializeField, HideInInspector] private Vector3 _initialVisualLocalScale = Vector3.one;

        [SerializeField, HideInInspector] private Vector3 _handleBasePos;
        [SerializeField, HideInInspector] private Quaternion _handleBaseRot = Quaternion.identity;
        [SerializeField, HideInInspector] private Vector3 _secondaryBasePos;
        [SerializeField, HideInInspector] private Quaternion _secondaryBaseRot = Quaternion.identity;
        [SerializeField, HideInInspector] private Vector3 _tertiaryBasePos;
        [SerializeField, HideInInspector] private Quaternion _tertiaryBaseRot = Quaternion.identity;
        [SerializeField, HideInInspector] private Vector3 _quaternaryBasePos;
        [SerializeField, HideInInspector] private Vector3 _quaternaryBaseScale = Vector3.one;
        [SerializeField, HideInInspector] private Quaternion _doorBaseRot = Quaternion.identity;
        [SerializeField, HideInInspector] private bool _hasCapturedRestPose;

        private bool _cachedInitialTransform;
        private float _valveAnimProgress;
        private float _targetValveProgress;
        private bool _isAnimatingValve;

        private float _doorAnimProgress;
        private float _targetDoorProgress;
        private bool _isAnimatingDoor;

        private float _bracketAnimProgress;
        private float _FlowPhase;
        private float _turbineAngleY;
        private double _lastRealtime;

        public bool IsAnimating => _isAnimatingValve || _isAnimatingDoor;
        public float CurrentValveProgress => _valveAnimProgress;
        public float CurrentDoorProgress => _doorAnimProgress;

        private void Awake()
        {
            if (visualRoot == null) return;
            CacheInitialTransform();
            _valveAnimProgress = isSecuredOrClosed ? 1f : 0f;
            _targetValveProgress = _valveAnimProgress;
            _bracketAnimProgress = _valveAnimProgress;
            _doorAnimProgress = isCabinetDoorOpen ? 1f : 0f;
            _targetDoorProgress = _doorAnimProgress;
            _lastRealtime = GetRealtime();

            ApplyVisualState(currentState, instant: true);
            if (isContainerCabinet || cabinetDoorHinge != null)
            {
                SetCabinetDoorOpen(isCabinetDoorOpen, instant: true);
            }
        }

        private void OnEnable()
        {
            if (visualRoot == null) return;
            CacheInitialTransform();
            _lastRealtime = GetRealtime();
            EvaluateMechanicalPose(_valveAnimProgress, _doorAnimProgress, 0f);
        }

        private static double GetRealtime()
        {
#if UNITY_EDITOR
            return EditorApplication.timeSinceStartup;
#else
            return Time.realtimeSinceStartupAsDouble;
#endif
        }

        /// <summary>
        /// Captures the exact FBX-imported local positions of all mechanical moving pivots before any pose evaluation modifies them.
        /// </summary>
        public void CaptureMechanicalRestPose()
        {
            if (visualRoot != null)
            {
                _initialVisualLocalPos = visualRoot.localPosition;
                _initialVisualLocalRot = visualRoot.localRotation;
                _initialVisualLocalScale = visualRoot.localScale;
            }
            if (valveHandleTransform != null)
            {
                _handleBasePos = valveHandleTransform.localPosition;
                _handleBaseRot = Quaternion.identity;
            }
            if (secondaryMovingPart != null)
            {
                _secondaryBasePos = secondaryMovingPart.localPosition;
                _secondaryBaseRot = Quaternion.identity;
            }
            if (tertiaryMovingPart != null)
            {
                _tertiaryBasePos = tertiaryMovingPart.localPosition;
                _tertiaryBaseRot = Quaternion.identity;
            }
            if (quaternaryMovingPart != null)
            {
                _quaternaryBasePos = quaternaryMovingPart.localPosition;
                _quaternaryBaseScale = Vector3.one;
            }
            if (cabinetDoorHinge != null)
            {
                _doorBaseRot = Quaternion.identity;
            }
            _hasCapturedRestPose = true;
            _cachedInitialTransform = true;
        }

        private void CacheInitialTransform()
        {
            if (_cachedInitialTransform || _hasCapturedRestPose)
            {
                _cachedInitialTransform = true;
                return;
            }
            if (visualRoot == null) return;
            CaptureMechanicalRestPose();
        }

        private void LateUpdate()
        {
            CacheInitialTransform();

            double now = GetRealtime();
            float dt = Mathf.Clamp((float)(now - _lastRealtime), 0.0001f, 0.05f);
            _lastRealtime = now;

            bool needsRepaint = false;

            if (_isAnimatingValve)
            {
                float dur = Mathf.Max(0.12f, valveAnimationDuration);
                _valveAnimProgress = Mathf.MoveTowards(_valveAnimProgress, _targetValveProgress, dt / dur);
                _bracketAnimProgress = _valveAnimProgress;
                if (Mathf.Approximately(_valveAnimProgress, _targetValveProgress))
                {
                    _valveAnimProgress = _targetValveProgress;
                    _isAnimatingValve = false;
                }
                needsRepaint = true;
            }

            if (_isAnimatingDoor)
            {
                float dur = Mathf.Max(0.12f, doorAnimationDuration);
                _doorAnimProgress = Mathf.MoveTowards(_doorAnimProgress, _targetDoorProgress, dt / dur);
                if (Mathf.Approximately(_doorAnimProgress, _targetDoorProgress))
                {
                    _doorAnimProgress = _targetDoorProgress;
                    _isAnimatingDoor = false;
                }
                needsRepaint = true;
            }

            // Continuous hydrodynamic water meter turbine & droplet physics while water valve is open in Play Mode
            bool isLiveWaterFlow = Application.isPlaying &&
                                   mechanismKind == ValveMechanismKind.RisingStemWaterValve &&
                                   _valveAnimProgress < 0.995f;

            if (needsRepaint || isLiveWaterFlow)
            {
                EvaluateMechanicalPose(_valveAnimProgress, _doorAnimProgress, dt);
#if UNITY_EDITOR
                if (!Application.isPlaying && needsRepaint)
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                }
#endif
            }
        }

        /// <summary>
        /// Takım arkadaşınızın ana oyun kodundan tek satırla sabitleme / vana-musluk-sigorta açma-kapatma çağrısı.
        /// Varsayılan olarak fizik tabanlı yumuşak animasyon oynatır.
        /// </summary>
        public void SetSecuredOrClosed(bool secured)
        {
            SetSecuredOrClosed(secured, instant: false);
        }

        public void SetSecuredOrClosed(bool secured, bool instant)
        {
            CacheInitialTransform();
            isSecuredOrClosed = secured;
            currentState = secured ? AssetVisualState.SecuredOrIsolated : AssetVisualState.DefaultIntact;
            _targetValveProgress = secured ? 1f : 0f;
            _lastRealtime = GetRealtime();

            if (securedBracketObject != null)
            {
                securedBracketObject.SetActive(secured);
            }

            if (mechanicalAnimator != null && mechanicalAnimator.isActiveAndEnabled && mechanicalAnimator.runtimeAnimatorController != null)
            {
                mechanicalAnimator.SetBool("IsClosed", secured);
            }

            if (instant)
            {
                _valveAnimProgress = _targetValveProgress;
                _bracketAnimProgress = _targetValveProgress;
                _isAnimatingValve = false;
                EvaluateMechanicalPose(_valveAnimProgress, _doorAnimProgress, 0f);
            }
            else
            {
                _isAnimatingValve = true;
                if (secured && securedBracketObject != null)
                {
                    _bracketAnimProgress = 0f;
                }
                EvaluateMechanicalPose(_valveAnimProgress, _doorAnimProgress, 0f);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                }
#endif
            }
        }

        [ContextMenu("Test: Vana-Şalter Kapat-Aç / Sabitle (Animasyonlu)")]
        public void ToggleSecuredOrClosed()
        {
            SetSecuredOrClosed(!isSecuredOrClosed, instant: false);
        }

        /// <summary>
        /// Dolap kapağını (Örn: Yangın Tüpü Dolabı veya Buzdolabı - CabinetDoor_Hinge) fiziksel menteşe ataletiyle açar veya kapatır.
        /// Sabitleme aparatları (SecuredBracket) yan ve üst duvarlarda olduğu için kapağın açılmasını asla engellemez.
        /// </summary>
        public void SetCabinetDoorOpen(bool open)
        {
            SetCabinetDoorOpen(open, instant: false);
        }

        public void SetCabinetDoorOpen(bool open, bool instant)
        {
            CacheInitialTransform();
            isCabinetDoorOpen = open;
            _targetDoorProgress = open ? 1f : 0f;
            _lastRealtime = GetRealtime();

            if (mechanicalAnimator != null && mechanicalAnimator.isActiveAndEnabled && mechanicalAnimator.runtimeAnimatorController != null)
            {
                mechanicalAnimator.SetBool("IsDoorOpen", open);
            }

            if (instant)
            {
                _doorAnimProgress = _targetDoorProgress;
                _isAnimatingDoor = false;
                EvaluateMechanicalPose(_valveAnimProgress, _doorAnimProgress, 0f);
            }
            else
            {
                _isAnimatingDoor = true;
                EvaluateMechanicalPose(_valveAnimProgress, _doorAnimProgress, 0f);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                }
#endif
            }
        }

        [ContextMenu("Test: Dolap Kapağını Aç / Kapat (Animasyonlu)")]
        public void ToggleCabinetDoor()
        {
            SetCabinetDoorOpen(!isCabinetDoorOpen, instant: false);
        }

        /// <summary>
        /// Karakter dolaptan yangın tüpünü almak istediğinde çağrılır.
        /// Kapak kapalıysa otomatik açar ve içindeki Yangın Tüpü (DisasterPrepItem) referansını döndürür.
        /// </summary>
        public DisasterPrepItem TakeContainedItem(bool autoOpenDoor = true)
        {
            if (!isContainerCabinet || containedItem == null) return null;
            if (autoOpenDoor && !isCabinetDoorOpen)
            {
                SetCabinetDoorOpen(true, instant: false);
            }
            DisasterPrepItem item = containedItem;
            containedItem = null;
            return item;
        }

        /// <summary>
        /// Afet Cutscene / Dalga değerlendirmesi sırasında eşyanın durumunu (Sağlam, Sabitlenmiş, Devrilmiş/Hasarlı, Tüketilmiş) günceller.
        /// </summary>
        public void ApplyVisualState(AssetVisualState newState, bool instant = false)
        {
            CacheInitialTransform();
            currentState = newState;

            bool isClosedOrSecured = (newState == AssetVisualState.SecuredOrIsolated) || isSecuredOrClosed;
            if (securedBracketObject != null)
            {
                securedBracketObject.SetActive(isClosedOrSecured);
            }

            _targetValveProgress = isClosedOrSecured ? 1f : 0f;
            if (instant)
            {
                _valveAnimProgress = _targetValveProgress;
                _isAnimatingValve = false;
            }
            else if (!Mathf.Approximately(_valveAnimProgress, _targetValveProgress))
            {
                _isAnimatingValve = true;
                _lastRealtime = GetRealtime();
            }

            EvaluateMechanicalPose(_valveAnimProgress, _doorAnimProgress, 0f);

            if (visualRoot != null)
            {
                switch (newState)
                {
                    case AssetVisualState.DefaultIntact:
                    case AssetVisualState.SecuredOrIsolated:
                        visualRoot.localPosition = _initialVisualLocalPos;
                        visualRoot.localRotation = _initialVisualLocalRot;
                        visualRoot.localScale = _initialVisualLocalScale;
                        break;

                    case AssetVisualState.DamagedOrToppled:
                        visualRoot.localPosition = _initialVisualLocalPos + new Vector3(0f, 0.04f, -0.15f);
                        visualRoot.localRotation = Quaternion.Euler(78f, 12f, 8f);
                        break;

                    case AssetVisualState.ConsumedOrEmpty:
                        visualRoot.localScale = new Vector3(_initialVisualLocalScale.x * 0.92f, _initialVisualLocalScale.y * 0.65f, _initialVisualLocalScale.z * 0.92f);
                        break;
                }
            }
        }

        /// <summary>
        /// Fizik kurallarına birebir uygun mekanik kinematik denklemlerini hesaplar ve alt parçalara uygular.
        /// </summary>
        private void EvaluateMechanicalPose(float uValve, float uDoor, float dt)
        {
            _FlowPhase += dt;

            // 1. HINGED CABINET, APPLIANCE & HEAVY FLOOR BUNKER HATCH DOORS (FireCabinet, Refrigerator, BunkerHatch)
            if (cabinetDoorHinge != null)
            {
                if (mechanismKind == ValveMechanismKind.HeavyBunkerHatch)
                {
                    // HEAVY ARMORED FLOOR BUNKER HATCH (240 kg Yuvarlak Zemin Sığınak Kapağı):
                    // Stage 1 (u = 0..0.24): Central 6-spoked locking handwheel spins -360° and rises +18mm on its threaded spindle to retract 4 radial dog bars.
                    // Stage 2 (u = 0.22..0.34): Compressed EPDM rubber seal pops (-4.5°) followed by rotational inertia hesitation (I = 165 kg*m^2).
                    // Stage 3 (u = 0.34..0.88): Heavy rotational heave around the Ø90mm rear steel hinge pin to -111.5° (compressing rear rubber stop bumpers).
                    // Stage 4 (u = 0.88..1.00): Damped rebound off the rear forged stanchion stops, settling at -108°.
                    float openTargetAngleX = -Mathf.Abs(doorOpenAngleY > 1f ? doorOpenAngleY : 108f);
                    float sLid = EvaluateBunkerHatchLidCurve(uDoor);
                    float lidAngleX = sLid * openTargetAngleX;
                    Quaternion lidRot = Quaternion.Euler(lidAngleX, 0f, 0f);
                    cabinetDoorHinge.localRotation = _doorBaseRot * lidRot;

                    if (secondaryMovingPart != null)
                    {
                        float sWheel = EvaluateBunkerWheelSpinCurve(uDoor);
                        float wheelSpinY = -360f * sWheel;
                        float wheelLiftY = 0.018f * sWheel;
                        Vector3 hingePivotPos = cabinetDoorHinge.localPosition;
                        Vector3 wheelOffsetFromHinge = (_secondaryBasePos - hingePivotPos) + new Vector3(0f, wheelLiftY, 0f);
                        secondaryMovingPart.localPosition = hingePivotPos + (lidRot * wheelOffsetFromHinge);
                        secondaryMovingPart.localRotation = lidRot * _secondaryBaseRot * Quaternion.Euler(0f, wheelSpinY, 0f);
                    }

                    if (tertiaryMovingPart != null)
                    {
                        float sPiston = Mathf.Clamp01(sLid);
                        tertiaryMovingPart.localRotation = _tertiaryBaseRot * Quaternion.Euler(-36f * sPiston, 0f, 0f);
                        tertiaryMovingPart.localPosition = _tertiaryBasePos + new Vector3(0f, 0.042f * sPiston, -0.052f * sPiston);
                    }
                }
                else
                {
                    float sDoor = EvaluateDoorHingeCurve(uDoor);
                    cabinetDoorHinge.localRotation = _doorBaseRot * Quaternion.Euler(0f, sDoor * doorOpenAngleY, 0f);

                    // If Refrigerator has an Upper Freezer Door assigned to secondaryMovingPart, open it with a 0.08s stagger
                    if (isContainerCabinet && mechanismKind == ValveMechanismKind.None && secondaryMovingPart != null)
                    {
                        float uUpper = Mathf.Clamp01((uDoor - 0.12f) / 0.88f);
                        float sUpper = EvaluateDoorHingeCurve(uUpper);
                        secondaryMovingPart.localRotation = _secondaryBaseRot * Quaternion.Euler(0f, sUpper * (doorOpenAngleY * 0.88f), 0f);
                    }
                }
            }

            // 2. SEISMIC BRACKET BOLT-IN SPRING EFFECT
            if (securedBracketObject != null && securedBracketObject.activeSelf && _isAnimatingValve)
            {
                float sBracket = EvaluateStopLugSettleCurve(Mathf.Clamp01(_bracketAnimProgress / 0.7f));
                securedBracketObject.transform.localScale = Vector3.one * Mathf.Max(0.01f, sBracket);
            }
            else if (securedBracketObject != null && securedBracketObject.activeSelf)
            {
                securedBracketObject.transform.localScale = Vector3.one;
            }

            // 3. VALVE / FAUCET / CIRCUIT BREAKER / APPLIANCE MECHANISMS
            switch (mechanismKind)
            {
                case ValveMechanismKind.QuarterTurnBallValve:
                {
                    // GAS VALVE (Doğalgaz Ana Vanası):
                    // Coaxial stem rotation (+90° Clockwise around local Z: 0° Vertical/Parallel to pipe = OPEN -> +90° Horizontal/Perpendicular to pipe = CLOSED)
                    if (valveHandleTransform != null)
                    {
                        float sBall = EvaluateBallValveCurve(uValve);
                        valveHandleTransform.localRotation = _handleBaseRot * Quaternion.Euler(0f, 0f, sBall * 90f);
                    }
                    // Bourdon-tube Manometer Needle (GaugeNeedle_Pivot):
                    // Active line pressure (-38° in green zone) drops to 0-bar stop pin (+112°) once ball port closes past 35%
                    if (secondaryMovingPart != null)
                    {
                        float uGauge = Mathf.Clamp01((uValve - 0.25f) / 0.75f);
                        float sGauge = EvaluateUnderdampedGaugeCurve(uGauge);
                        float needleAngleZ = Mathf.LerpUnclamped(-38f, 112f, sGauge);
                        secondaryMovingPart.localRotation = _secondaryBaseRot * Quaternion.Euler(0f, 0f, needleAngleZ);
                    }
                    break;
                }

                case ValveMechanismKind.RisingStemWaterValve:
                {
                    // WATER VALVE & FAUCET (Yana Bakan Su Kesme Vanası, Sayacı & Tahliye Musluğu):
                    // 1. Main Sideways Rising-Stem Handwheel (ValveHandle_Pivot):
                    // Right-Hand Screw Rule: Rotates +540° (1.5 clockwise turns around horizontal +Z stem axis)
                    // AND translates inward toward the pipe (-0.015m along horizontal Z stem axis) following helical screw pitch deltaZ = -p * (theta / 360)
                    float sWheel = EvaluateMultiTurnWheelCurve(uValve);
                    if (valveHandleTransform != null)
                    {
                        valveHandleTransform.localRotation = _handleBaseRot * Quaternion.Euler(0f, 0f, sWheel * 540f);
                        valveHandleTransform.localPosition = _handleBasePos + new Vector3(0f, 0f, -0.015f * sWheel);
                    }

                    // 2. Sideways Brass Drain Faucet / Butterfly Tap (FaucetTap_Pivot):
                    // Rotates +90° clockwise around its horizontal bonnet stem (+Z) and seats -0.0035m inward
                    if (secondaryMovingPart != null)
                    {
                        float uTap = Mathf.Clamp01((uValve - 0.10f) / 0.85f);
                        float sTap = EvaluateBallValveCurve(uTap);
                        secondaryMovingPart.localRotation = _secondaryBaseRot * Quaternion.Euler(0f, 0f, sTap * 90f);
                        secondaryMovingPart.localPosition = _secondaryBasePos + new Vector3(0f, 0f, -0.0035f * sTap);
                    }

                    // 3. Front-Facing Water Meter Impeller Turbine (MeterTurbine_Pivot):
                    // Orifice flow rate Q ~ (1 - sWheel)^1.6; spins around Z axis when water flows, decelerates via fluid drag to 0 when shut off
                    if (tertiaryMovingPart != null)
                    {
                        float flowRate = Mathf.Pow(Mathf.Clamp01(1f - sWheel), 1.6f);
                        if (dt > 0f)
                        {
                            _turbineAngleY = Mathf.Repeat(_turbineAngleY + dt * 280f * flowRate, 360f);
                        }
                        float shutoffSpin = (1f - flowRate) * 140f;
                        tertiaryMovingPart.localRotation = _tertiaryBaseRot * Quaternion.Euler(0f, 0f, _turbineAngleY + shutoffSpin);
                    }

                    // 4. Downward Faucet Spout Water Droplet (WaterDrop_Pivot):
                    // Surface-tension pinch-off when closed; gentle gravity drip pulse when open
                    if (quaternaryMovingPart != null)
                    {
                        float openFactor = Mathf.Clamp01(1f - (uValve * 1.25f));
                        float dripPulse = Application.isPlaying ? (1f + 0.12f * Mathf.Sin(_FlowPhase * 6.5f)) : 1f;
                        quaternaryMovingPart.localScale = _quaternaryBaseScale * (openFactor * dripPulse);
                        quaternaryMovingPart.localPosition = _quaternaryBasePos + new Vector3(0f, -0.006f * (1f - openFactor), 0f);
                    }
                    break;
                }

                case ValveMechanismKind.CircuitBreakerPanel:
                {
                    // ELECTRICAL CIRCUIT BREAKER PANEL (Tek Büyük Ana Şalterli Elektrik Sigorta Panosu):
                    // Large industrial knife/toggle switch pivots around its internal horizontal X-axis axle:
                    // ON (Elektrik Açık / I - Yeşil Bölge) = -48° (Pointing HIGH UPWARD)
                    // OFF (Şalter İndirildi / O - Kırmızı Bölge) = +48° (Pointing LOW DOWNWARD, 96° total travel)
                    // Bistable over-center spring snap for crisp tactile & visual verification during gameplay
                    if (valveHandleTransform != null)
                    {
                        float sMain = EvaluateBreakerSnapCurve(uValve);
                        float angleX = Mathf.LerpUnclamped(-48f, 48f, sMain);
                        valveHandleTransform.localRotation = _handleBaseRot * Quaternion.Euler(angleX, 0f, 0f);
                    }
                    // Oversized Green Status Dome Light extinguishes the instant the main contacts break at the over-center point (u >= 0.45)
                    if (quaternaryMovingPart != null)
                    {
                        float ledScale = uValve < 0.45f ? 1f : Mathf.Clamp01(1f - (uValve - 0.45f) * 5f);
                        quaternaryMovingPart.localScale = _quaternaryBaseScale * ledScale;
                    }
                    break;
                }

                case ValveMechanismKind.WaterHeaterIsolation:
                {
                    // WATER HEATER (Banyo Termosifonu):
                    // 1. Cold-water inlet butterfly tap on bottom pipe rotates +90° clockwise to shut off water feed
                    if (valveHandleTransform != null)
                    {
                        float sTap = EvaluateBallValveCurve(uValve);
                        valveHandleTransform.localRotation = _handleBaseRot * Quaternion.Euler(0f, 0f, sTap * 90f);
                    }
                    // 2. Front rotary thermostat dial turns -120° counter-clockwise to 0°C / OFF
                    if (secondaryMovingPart != null)
                    {
                        float sKnob = Mathf.SmoothStep(0f, 1f, uValve);
                        secondaryMovingPart.localRotation = _secondaryBaseRot * Quaternion.Euler(0f, 0f, -120f * sKnob);
                    }
                    // 3. Heating pilot LED turns off when isolated
                    if (quaternaryMovingPart != null)
                    {
                        quaternaryMovingPart.localScale = _quaternaryBaseScale * Mathf.Clamp01(1f - uValve * 1.4f);
                    }
                    break;
                }

                case ValveMechanismKind.AirConditionerLouver:
                {
                    // AIR CONDITIONER (Duvar Kliması):
                    // Motorized front airflow louver blade rotates around horizontal X-axis hinge to close vent slot
                    if (valveHandleTransform != null)
                    {
                        float sFlap = Mathf.SmoothStep(0f, 1f, uValve);
                        valveHandleTransform.localRotation = _handleBaseRot * Quaternion.Euler(-48f * sFlap, 0f, 0f);
                    }
                    if (quaternaryMovingPart != null)
                    {
                        quaternaryMovingPart.localScale = _quaternaryBaseScale * Mathf.Clamp01(1f - uValve * 1.4f);
                    }
                    break;
                }

                case ValveMechanismKind.HeavyBunkerHatch:
                {
                    // Handled above via cabinetDoorHinge + secondaryMovingPart + tertiaryMovingPart
                    break;
                }

                default:
                {
                    if (valveHandleTransform != null && isUtilityValve)
                    {
                        float s = EvaluateBallValveCurve(uValve);
                        valveHandleTransform.localRotation = _handleBaseRot * Quaternion.Euler(0f, 0f, s * 90f);
                    }
                    break;
                }
            }
        }

        // =====================================================================
        // PHYSICALLY-BASED KINEMATIC CURVE FUNCTIONS
        // =====================================================================

        /// <summary>
        /// Küresel Vana (Quarter-Turn Ball Valve) Tork Eğrisi:
        /// 1. PTFE conta statik sürtünme yenme (Breakaway torque, u = 0..0.18)
        /// 2. Dinamik kayma ivmelenmesi (u = 0.18..0.86)
        /// 3. 90° pirinç mekanik stoper teması ve mikro elastik oturma (u = 0.86..1.0)
        /// </summary>
        public static float EvaluateBallValveCurve(float u)
        {
            u = Mathf.Clamp01(u);
            if (u <= 0f) return 0f;
            if (u >= 1f) return 1f;

            if (u < 0.18f)
            {
                float t = u / 0.18f;
                return 0.10f * t * t * t;
            }
            if (u < 0.86f)
            {
                float t = (u - 0.18f) / 0.68f;
                float smooth = t * t * (3f - 2f * t);
                return Mathf.Lerp(0.10f, 1.016f, smooth);
            }
            else
            {
                float t = (u - 0.86f) / 0.14f;
                return Mathf.Lerp(1.016f, 1.0f, t * (2f - t));
            }
        }

        /// <summary>
        /// Çok Turlu Salmastralı Su Vanası Volanı (Rising-Stem Handwheel) Biyomekanik El Çevirme Eğrisi:
        /// 1.5 tam tur (540°) boyunca 3 yarım turluk bilek kavrama/çevirme darbesi (hiçbir noktada geri dönmez, ds/du >= 0)
        /// ve son çeyrek turda kauçuk salmastra contasının sıkışma direnci (seating compression deceleration).
        /// </summary>
        public static float EvaluateMultiTurnWheelCurve(float u)
        {
            u = Mathf.Clamp01(u);
            if (u <= 0f) return 0f;
            if (u >= 1f) return 1f;

            float baseSmooth = u * u * (3f - 2f * u);
            float wristCadence = 0.024f * Mathf.Sin(u * Mathf.PI * 6f) * (1f - u);
            return Mathf.Clamp01(baseSmooth + wristCadence);
        }

        /// <summary>
        /// Otomatik Sigorta Şalteri (MCB Bistable Over-Center Spring Snap) Eğrisi:
        /// 1. u = 0..0.38: İç devrilme yayını ölü noktaya (dead-center) kadar parmak kuvvetiyle sıkıştırma (0 -> 0.30)
        /// 2. u = 0.38..0.68: Ölü noktayı aşan yayın potansiyel enerjisini boşaltarak mandalı hızla alt stopere çarptırması (0.30 -> 1.035)
        /// 3. u = 0.68..1.00: Termoplastik alt stoperde mikro sönümlenme (1.035 -> 1.00)
        /// </summary>
        public static float EvaluateBreakerSnapCurve(float u)
        {
            u = Mathf.Clamp01(u);
            if (u <= 0f) return 0f;
            if (u >= 1f) return 1f;

            if (u < 0.38f)
            {
                float t = u / 0.38f;
                return 0.30f * Mathf.Pow(t, 1.75f);
            }
            if (u < 0.68f)
            {
                float t = (u - 0.38f) / 0.30f;
                float snap = 1f - Mathf.Pow(1f - t, 3.2f);
                return Mathf.Lerp(0.30f, 1.035f, snap);
            }
            else
            {
                float t = (u - 0.68f) / 0.32f;
                return Mathf.Lerp(1.035f, 1.0f, t * (2f - t));
            }
        }

        /// <summary>
        /// Bourdon Tüplü Manometre İbresi Sönümlü Yay Eğrisi.
        /// </summary>
        public static float EvaluateUnderdampedGaugeCurve(float u)
        {
            u = Mathf.Clamp01(u);
            if (u <= 0f) return 0f;
            if (u >= 1f) return 1f;
            return 1f - Mathf.Exp(-5.5f * u) * Mathf.Cos(7.5f * u);
        }

        /// <summary>
        /// Menteşeli Dolap Kapağı (Hinged Door) Eylemsizlik & Mandal Eğrisi:
        /// Açılırken mandal kurtulması + açısal momentum + menteşe sönümlemesi; kapanırken conta üzerinde < 0° taşmadan tam duruş.
        /// </summary>
        public static float EvaluateDoorHingeCurve(float u)
        {
            u = Mathf.Clamp01(u);
            if (u <= 0f) return 0f;
            if (u >= 1f) return 1f;

            float smooth = u * u * (3f - 2f * u);
            float checkStrapSettle = Mathf.Sin(u * Mathf.PI) * 0.025f * u;
            return Mathf.Clamp(smooth + checkStrapSettle, 0f, 1.02f);
        }

        /// <summary>
        /// 240 kg Zırhlı Zemin Sığınak Kapağı (Heavy Bunker Hatch) Merkez Kilit Volanı Dönüş Eğrisi:
        /// İlk %24'lük fazda (u = 0..0.24) 1 tam tur (-360°) dönerek 4 yönlü çelik sürgüleri geri çeker.
        /// </summary>
        public static float EvaluateBunkerWheelSpinCurve(float u)
        {
            u = Mathf.Clamp01(u);
            if (u <= 0f) return 0f;
            if (u >= 0.24f) return 1f;
            return EvaluateMultiTurnWheelCurve(u / 0.24f);
        }

        /// <summary>
        /// 240 kg Zırhlı Zemin Sığınak Kapağı (Heavy Bunker Hatch) 4 Fazlı Fiziksel Menteşe Eğrisi:
        /// - Faz 1 (u = 0.00..0.22): Kilit volanı çözülürken kapak sabit (0.00).
        /// - Faz 2 (u = 0.22..0.34): EPDM vakum/conta kurtulma sıçraması (0 -> 0.042 = ~4.5°) ve ağır kütle eylemsizlik duraksaması.
        /// - Faz 3 (u = 0.34..0.88): Torsiyon yayı + hidrolik amortisör destekli ağır açısal ivmelenme ve arka kauçuk stoper sıkışması (0.042 -> 1.032 = ~111.5°).
        /// - Faz 4 (u = 0.88..1.00): Arka döküm stoperde sönümlü sekme ve tam -108° denge açısına oturma (1.032 -> 0.990 -> 1.000).
        /// </summary>
        public static float EvaluateBunkerHatchLidCurve(float u)
        {
            u = Mathf.Clamp01(u);
            if (u <= 0.22f) return 0f;
            if (u >= 1f) return 1f;

            if (u < 0.29f)
            {
                // Seal pop-off (0 -> 0.042)
                float t = (u - 0.22f) / 0.07f;
                float pop = 1f - (1f - t) * (1f - t);
                return 0.042f * pop;
            }
            if (u < 0.35f)
            {
                // Heavy dead-weight hesitation before main heave (0.042 -> 0.052)
                float t = (u - 0.29f) / 0.06f;
                return Mathf.Lerp(0.042f, 0.052f, t * t);
            }
            if (u < 0.88f)
            {
                // High-inertia rotational heave with torsion spring counterbalance (0.052 -> 1.032)
                float t = (u - 0.35f) / 0.53f;
                // Quartic ease-in transitioning into strong angular momentum
                float heave = t < 0.55f
                    ? 2.2f * t * t * t
                    : 1f - Mathf.Pow(1f - t, 2.6f) * 0.82f;
                heave = Mathf.Clamp01(t * t * (3f - 2f * t) * 0.65f + heave * 0.35f);
                return Mathf.Lerp(0.052f, 1.032f, heave);
            }
            if (u < 0.95f)
            {
                // Rebound off rear forged-steel rubber stop block (1.032 -> 0.991)
                float t = (u - 0.88f) / 0.07f;
                float smooth = t * t * (3f - 2f * t);
                return Mathf.Lerp(1.032f, 0.991f, smooth);
            }
            else
            {
                // Final settle onto stop block (0.991 -> 1.000)
                float t = (u - 0.95f) / 0.05f;
                return Mathf.Lerp(0.991f, 1.0f, t * (2f - t));
            }
        }

        private static float EvaluateStopLugSettleCurve(float u)
        {
            u = Mathf.Clamp01(u);
            float inv = 1f - u;
            return 1f - inv * inv * inv;
        }

        /// <summary>
        /// Cutscene sonunda bu eşyanın ikincil bir afet dalgası (Yangın, Sel, Elektrik, Koridor Kapanması) tetikleyip tetiklemediğini döndürür.
        /// </summary>
        public bool DoesTriggerSecondaryWave(out SecondaryHazardType hazard)
        {
            if ((isSecurableFixture || isUtilityValve) && !isSecuredOrClosed && triggersSecondaryHazard != SecondaryHazardType.None)
            {
                hazard = triggersSecondaryHazard;
                return true;
            }
            hazard = SecondaryHazardType.None;
            return false;
        }
    }
}
