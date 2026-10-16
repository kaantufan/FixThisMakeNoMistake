# Too Late to Panic! — Hazır Kaynak ve AI Beyan Kaydı (Assets/_YarkinAssets)

Bu belge, **CIVIL GAME JAM 2026** Kural Kitapçığı **Madde 3.4, Madde 4.2 ve EK A.4 / A.5** şablonuna birebir uygun olarak tutulmaktadır.

---

## 1. Hazır Kaynak Beyanı (EK A.4 Şablonu)

*(Not: `Assets/_YarkinAssets` altında dışarıdan hazır 3D model veya görsel indirilmemiştir; tüm 18 model, materyal ve ikon etkinlik süresince prosedürel/script tabanlı olarak sıfırdan üretilmiştir. Ana projedeki karakter/ses kaynakları birleştirme aşamasında eklenecektir.)*

| Kategori | Durum (Kullanıldı / Kullanılmadı) | Kaynak Adı & Üretici | Kaynak Bağlantısı | Lisans & Bağlantısı | Oyundaki Yeri & Yapılan Değişiklik | Paylaşım Kısıtı & Kurulum |
|---|---|---|---|---|---|---|
| **Görsel / 3D Model** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | Tüm 18 3D eşya modeli ve 2D envanter ikonu jam süresince Blender 5.1 + Unity 6 içinde üretilmiştir. | Kısıt yok, repoda tam kaynaklarıyla mevcuttur. |
| **Ses** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | — | — |
| **Kod** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | — | — |
| **Metin / Font** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | — | — |
| **Diğer** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | — | — |

---

## 2. Yapay Zeka (AI) Beyanı (EK A.5 Şablonu)

| Kategori | Durum (Kullanıldı / Kullanılmadı) | Kullanılan AI Aracı | Kullanım Amacı | Oyundaki Kullanım Yeri |
|---|---|---|---|---|
| **Görsel** | Kullanıldı | Antigravity AI Agent (Blender Python & Unity Editor otomasyonu) | 18 adet afet hazırlık eşyasının 3D low-poly mesh geometrisini (`SM_*.fbx`), materyallerini (`M_*.mat`) ve 2D envanter ikonlarını (`T_Icon_*.png`) üreten scriptlerin oluşturulması | `Assets/_YarkinAssets/Art/Meshes`, `Materials`, `Icons` ve `Prefabs` altındaki 18 eşya |
| **Ses** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — |
| **Kod** | Kullanıldı | Antigravity AI Agent | `DisasterPrepItem.cs` veri bileşeni, `YarkinAssetPipelineBuilder.cs` Unity Editor otomasyon aracı ve `generate_18_assets.py` Blender üretim scriptinin yazılması | `Assets/_YarkinAssets/Scripts/` ve `Tools~/` |
| **Metin** | Kullanıldı | Antigravity AI Agent | Eşya Türkçe isimleri, kategorileri ve dokümantasyon/manifest tablolarının düzenlenmesi | `DisasterPrepItem` bileşen verileri ve bu belge |
| **Diğer** | Kullanılmadı | — | — | — |

---

## 3. Üretilen 18 Asset Envanteri (`Assets/_YarkinAssets`)

Tüm modeller **1 Unity birimi = 1 metre** gerçek dünya ölçeğinde, **taban-merkez (bottom-center) pivotlu**, **Built-in Standard** ortak materyal paletli (`M_*`) ve **256×256 Sprite envanter ikonlu (`T_Icon_*`)** olarak üretilmiştir.

| # | Türkçe Adı | Prefab (`Prefabs/`) | FBX Mesh (`Art/Meshes/`) | İkon (`Art/Icons/`) | Kategori (`DisasterItemCategory`) | Boyut (G×D×Y m) | Poligon | Collider | Toplanabilir | Sabitlenebilir |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | **Yangın Tüpü** | `PF_FireExtinguisher.prefab` | `SM_FireExtinguisher.fbx` | `T_Icon_FireExtinguisher.png` | `FireSafety` | 0.16 × 0.16 × 0.52 | 342 | `CapsuleCollider` | Evet | Evet |
| 2 | **Battaniye** | `PF_Blanket.prefab` | `SM_Blanket.fbx` | `T_Icon_Blanket.png` | `SurvivalGear` | 0.44 × 0.33 × 0.13 | 378 | `BoxCollider` | Evet | Hayır |
| 3 | **Snack Bar** | `PF_SnackBar.prefab` | `SM_SnackBar.fbx` | `T_Icon_SnackBar.png` | `FoodAndWater` | 0.14 × 0.05 × 0.025 | 126 | `BoxCollider` | Evet | Hayır |
| 4 | **Elma** | `PF_Apple.prefab` | `SM_Apple.fbx` | `T_Icon_Apple.png` | `FoodAndWater` | 0.09 × 0.09 × 0.10 | 280 | `CapsuleCollider` | Evet | Hayır |
| 5 | **Konserve** | `PF_CannedFood.prefab` | `SM_CannedFood.fbx` | `T_Icon_CannedFood.png` | `FoodAndWater` | 0.10 × 0.10 × 0.11 | 116 | `CapsuleCollider` | Evet | Hayır |
| 6 | **Su Şişesi** | `PF_WaterBottle.prefab` | `SM_WaterBottle.fbx` | `T_Icon_WaterBottle.png` | `FoodAndWater` | 0.08 × 0.08 × 0.25 | 116 | `CapsuleCollider` | Evet | Hayır |
| 7 | **Enerji İçeceği** | `PF_EnergyDrink.prefab` | `SM_EnergyDrink.fbx` | `T_Icon_EnergyDrink.png` | `FoodAndWater` | 0.065 × 0.065 × 0.16 | 116 | `CapsuleCollider` | Evet | Hayır |
| 8 | **Batarya (Güç Kaynağı)** | `PF_PowerBattery.prefab` | `SM_PowerBattery.fbx` | `T_Icon_PowerBattery.png` | `PowerAndLight` | 0.22 × 0.14 × 0.18 | 152 | `BoxCollider` | Evet | Hayır |
| 9 | **Küçük Pil** | `PF_SmallBattery.prefab` | `SM_SmallBattery.fbx` | `T_Icon_SmallBattery.png` | `PowerAndLight` | 0.036 × 0.036 × 0.095 | 76 | `CapsuleCollider` | Evet | Hayır |
| 10 | **Telsiz** | `PF_WalkieTalkie.prefab` | `SM_WalkieTalkie.fbx` | `T_Icon_WalkieTalkie.png` | `Communication` | 0.07 × 0.045 × 0.24 | 154 | `BoxCollider` | Evet | Hayır |
| 11 | **Televizyon** | `PF_Television.prefab` | `SM_Television.fbx` | `T_Icon_Television.png` | `HouseholdFixture` | 0.96 × 0.22 × 0.63 | 132 | `BoxCollider` | Hayır | Evet |
| 12 | **Halı** | `PF_Rug.prefab` | `SM_Rug.fbx` | `T_Icon_Rug.png` | `HouseholdFixture` | 1.86 × 1.18 × 0.016 | 78 | `BoxCollider` | Hayır | Hayır |
| 13 | **Kıyafet** | `PF_FoldedClothes.prefab` | `SM_FoldedClothes.fbx` | `T_Icon_FoldedClothes.png` | `SurvivalGear` | 0.36 × 0.28 × 0.15 | 216 | `BoxCollider` | Evet | Hayır |
| 14 | **Afet Çantası** | `PF_EmergencyBackpack.prefab` | `SM_EmergencyBackpack.fbx` | `T_Icon_EmergencyBackpack.png` | `SurvivalGear` | 0.36 × 0.24 × 0.53 | 328 | `BoxCollider` | Evet | Hayır |
| 15 | **İlk Yardım Kiti** | `PF_FirstAidKit.prefab` | `SM_FirstAidKit.fbx` | `T_Icon_FirstAidKit.png` | `MedicalAndHygiene` | 0.28 × 0.22 × 0.11 | 192 | `BoxCollider` | Evet | Hayır |
| 16 | **El Feneri** | `PF_Flashlight.prefab` | `SM_Flashlight.fbx` | `T_Icon_Flashlight.png` | `PowerAndLight` | 0.07 × 0.22 × 0.07 | 112 | `BoxCollider` | Evet | Hayır |
| 17 | **Acil Durum Radyosu** | `PF_EmergencyRadio.prefab` | `SM_EmergencyRadio.fbx` | `T_Icon_EmergencyRadio.png` | `Communication` | 0.26 × 0.11 × 0.32 | 244 | `BoxCollider` | Evet | Hayır |
| 18 | **Klima** | `PF_AirConditioner.prefab` | `SM_AirConditioner.fbx` | `T_Icon_AirConditioner.png` | `HouseholdFixture` | 0.86 × 0.23 × 0.28 | 126 | `BoxCollider` | Hayır | Evet |
