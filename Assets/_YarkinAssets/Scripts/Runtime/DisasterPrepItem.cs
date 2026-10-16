using UnityEngine;

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

        [Header("Çoklu Görsel Durum (Multi-State) Referansları")]
        public AssetVisualState currentState = AssetVisualState.DefaultIntact;
        [Tooltip("Ana görsel model (Visual)")]
        public Transform visualRoot;
        [Tooltip("Sabitlendiğinde aktifleşen 3D Çelik L-Braket / Gergi Kayışı objesi (Varsayılan: Kapalı)")]
        public GameObject securedBracketObject;
        [Tooltip("Vana/Şalter kolu veya göstergesi (Kapatıldığında 90 derece döner veya renk değiştirir)")]
        public Transform valveHandleTransform;

        [Header("Görsel & UI Referansı")]
        public Sprite inventoryIcon;

        private Vector3 _initialVisualLocalPos;
        private Quaternion _initialVisualLocalRot;
        private Vector3 _initialVisualLocalScale;
        private bool _cachedInitialTransform;

        private void Awake()
        {
            CacheInitialTransform();
            ApplyVisualState(currentState);
        }

        private void CacheInitialTransform()
        {
            if (_cachedInitialTransform || visualRoot == null) return;
            _initialVisualLocalPos = visualRoot.localPosition;
            _initialVisualLocalRot = visualRoot.localRotation;
            _initialVisualLocalScale = visualRoot.localScale;
            _cachedInitialTransform = true;
        }

        /// <summary>
        /// Takım arkadaşınızın ana oyun kodundan tek satırla sabitleme / vana kapatma çağrısı.
        /// </summary>
        public void SetSecuredOrClosed(bool secured)
        {
            isSecuredOrClosed = secured;
            ApplyVisualState(secured ? AssetVisualState.SecuredOrIsolated : AssetVisualState.DefaultIntact);
        }

        /// <summary>
        /// Afet Cutscene / Dalga değerlendirmesi sırasında eşyanın durumunu (Sağlam, Sabitlenmiş, Devrilmiş/Hasarlı, Tüketilmiş) günceller.
        /// </summary>
        public void ApplyVisualState(AssetVisualState newState)
        {
            CacheInitialTransform();
            currentState = newState;

            if (securedBracketObject != null)
            {
                bool showBracket = (newState == AssetVisualState.SecuredOrIsolated) || isSecuredOrClosed;
                securedBracketObject.SetActive(showBracket);
            }

            if (valveHandleTransform != null && isUtilityValve)
            {
                valveHandleTransform.localRotation = (newState == AssetVisualState.SecuredOrIsolated || isSecuredOrClosed)
                    ? Quaternion.Euler(0f, 0f, -90f)
                    : Quaternion.identity;
            }

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
                        // If it wasn't secured before the wave hit, topple it forward/sideways in the cutscene aftermath
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
