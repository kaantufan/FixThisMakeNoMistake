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
        HouseholdFixture
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
        [Tooltip("Afet hazırlık puan katkısı")]
        public int preparednessScore = 10;
        [Tooltip("Oyuncu tarafından çantaya/envantere alınabilir mi?")]
        public bool isCollectible = true;
        [Tooltip("Deprem öncesinde duvara/zemine sabitlenebilir mi? (TV, Klima vb.)")]
        public bool isSecurableFixture = false;

        [Header("Görsel Referans")]
        public Sprite inventoryIcon;
    }
}
