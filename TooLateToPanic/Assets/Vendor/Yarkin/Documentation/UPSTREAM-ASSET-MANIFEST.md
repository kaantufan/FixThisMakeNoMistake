# Too Late to Panic! — Hazır Kaynak ve AI Beyan Kaydı (`Assets/_YarkinAssets`)

Bu belge, **CIVIL GAME JAM 2026** Kural Kitapçığı **Madde 3.4, Madde 4.2 ve EK A.4 / A.5** şablonuna birebir uygun olarak tutulmaktadır.

---

## 1. Hazır Kaynak Beyanı (EK A.4 Şablonu)

*(Not: `Assets/_YarkinAssets` altında dışarıdan hazır 3D model veya görsel indirilmemiştir; tüm 32 model, 256×256 semantik renk atlası, 28 materyal, 3D L-braket sabitleme aparatları ve 32 envanter ikonu etkinlik süresince prosedürel/script tabanlı olarak sıfırdan üretilmiştir. Ana projedeki karakter/ses kaynakları birleştirme aşamasında eklenecektir.)*

| Kategori | Durum (Kullanıldı / Kullanılmadı) | Kaynak Adı & Üretici | Kaynak Bağlantısı | Lisans & Bağlantısı | Oyundaki Yeri & Yapılan Değişiklik | Paylaşım Kısıtı & Kurulum |
|---|---|---|---|---|---|---|
| **Görsel / 3D Model** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | Tüm 32 adet 3D eşya modeli, `T_DisasterColorSwatch_256.png` renk atlası ve 2D envanter ikonları jam süresince Blender 5.1 + Unity 6 içinde üretilmiştir. | Kısıt yok, repoda tam kaynaklarıyla mevcuttur. |
| **Ses** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | — | — |
| **Kod** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | — | — |
| **Metin / Font** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | — | — |
| **Diğer** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — | — | — |

---

## 2. Yapay Zeka (AI) Beyanı (EK A.5 Şablonu)

| Kategori | Durum (Kullanıldı / Kullanılmadı) | Kullanılan AI Aracı | Kullanım Amacı | Oyundaki Kullanım Yeri |
|---|---|---|---|---|
| **Görsel** | Kullanıldı | Antigravity AI Agent (Blender Python & Unity Editor otomasyonu) | 32 adet afet hazırlık eşyasının ve tesisat/fikstür varlığının 3D izometrik mesh geometrisini (`SM_*.fbx`), semantik renk atlasını (`T_DisasterColorSwatch_256.png`), materyallerini (`M_*.mat`) ve 2D envanter ikonlarını (`T_Icon_*.png`) üreten scriptlerin oluşturulması | `Assets/_YarkinAssets/Art/Meshes`, `Materials`, `Textures/Packed`, `Icons` ve `Prefabs` altındaki 32 eşya |
| **Ses** | Kullanılmadı (`_YarkinAssets` içinde) | — | — | — |
| **Kod** | Kullanıldı | Antigravity AI Agent | `DisasterPrepItem.cs`, `IsometricItemMotion.cs`, `DisasterWaveEvaluator.cs`, `IsometricStylizedOutline.shader`, `YarkinAssetPipelineBuilder.cs` ve `generate_18_assets.py` scriptlerinin yazılması | `Assets/_YarkinAssets/Scripts/`, `Art/Materials/` ve `Tools~/` |
| **Metin** | Kullanıldı | Antigravity AI Agent | Eşya Türkçe isimleri, kategorileri, İMO YOTA hazırlık puanları ve dokümantasyon/manifest tablolarının düzenlenmesi | `DisasterPrepItem` bileşen verileri ve bu belge |
| **Diğer** | Kullanılmadı | — | — | — |

---

## 3. Üretilen 32 İzometrik 3D Asset Envanteri (`Assets/_YarkinAssets`)

Tüm 32 model **İzometrik "Toy-Box" ölçek hiyerarşisi** ve **Chunky/Heroic silüet** prensipleriyle, **taban-merkez (bottom-center) pivotlu**, **4 kademeli Semantik Afet Renk Paletli (`M_*` + `T_DisasterColorSwatch_256.png`)**, **iki katmanlı collider mimarili** (Root fizik collider + Child `InteractionTrigger`) ve **çoklu görsel durum (`SecuredBracket`, `ValveState`, `DamagedOrToppled`)** destekli olarak üretilmiştir.

### Bölüm A: Yenilenmiş 18 Ana Afet Hazırlık Eşyası

| # | Türkçe Adı | Prefab (`Prefabs/`) | FBX Mesh (`Art/Meshes/`) | İkon (`Art/Icons/`) | Kategori | Toy-Box Ölçek | Poligon | Toplanabilir | Sabitlenebilir (L-Braket) | Tetiklediği İkincil Afet |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | **Yangın Tüpü** | `PF_FireExtinguisher` | `SM_FireExtinguisher.fbx` | `T_Icon_FireExtinguisher.png` | `FireSafety` | 1.20x | 5,264 | Evet | **Evet (`SecuredBracket`)** | `FireWave` |
| 2 | **Battaniye** | `PF_Blanket` | `SM_Blanket.fbx` | `T_Icon_Blanket.png` | `SurvivalGear` | 1.15x | 5,814 | Evet | Hayır | `None` |
| 3 | **Snack Bar** | `PF_SnackBar` | `SM_SnackBar.fbx` | `T_Icon_SnackBar.png` | `FoodAndWater` | 1.75x | 2,064 | Evet | Hayır | `None` |
| 4 | **Elma** | `PF_Apple` | `SM_Apple.fbx` | `T_Icon_Apple.png` | `FoodAndWater` | 1.60x | 1,632 | Evet | Hayır | `None` |
| 5 | **Konserve** | `PF_CannedFood` | `SM_CannedFood.fbx` | `T_Icon_CannedFood.png` | `FoodAndWater` | 1.38x | 3,686 | Evet | Hayır | `None` |
| 6 | **Su Şişesi** | `PF_WaterBottle` | `SM_WaterBottle.fbx` | `T_Icon_WaterBottle.png` | `FoodAndWater` | 1.42x | 3,690 | Evet | Hayır | `None` |
| 7 | **Enerji İçeceği** | `PF_EnergyDrink` | `SM_EnergyDrink.fbx` | `T_Icon_EnergyDrink.png` | `FoodAndWater` | 1.48x | 2,078 | Evet | Hayır | `None` |
| 8 | **Batarya İstasyonu** | `PF_PowerBattery` | `SM_PowerBattery.fbx` | `T_Icon_PowerBattery.png` | `PowerAndLight` | 1.22x | 1,764 | Evet | Hayır | `None` |
| 9 | **İkili Kalem Pil** | `PF_SmallBattery` | `SM_SmallBattery.fbx` | `T_Icon_SmallBattery.png` | `PowerAndLight` | 1.80x | 1,712 | Evet | Hayır | `None` |
| 10 | **El Telsizi** | `PF_WalkieTalkie` | `SM_WalkieTalkie.fbx` | `T_Icon_WalkieTalkie.png` | `Communication` | 1.50x | 2,014 | Evet | Hayır | `None` |
| 11 | **Televizyon** | `PF_Television` | `SM_Television.fbx` | `T_Icon_Television.png` | `HouseholdFixture` | 1.00x | 1,450 | Hayır | **Evet (`SecuredBracket`)** | `FallingTrauma` |
| 12 | **Halı** | `PF_Rug` | `SM_Rug.fbx` | `T_Icon_Rug.png` | `HouseholdFixture` | 1.00x | 1,092 | Hayır | Hayır | `None` |
| 13 | **Kıyafet** | `PF_FoldedClothes` | `SM_FoldedClothes.fbx` | `T_Icon_FoldedClothes.png` | `SurvivalGear` | 1.20x | 2,984 | Evet | Hayır | `None` |
| 14 | **Afet Çantası** | `PF_EmergencyBackpack` | `SM_EmergencyBackpack.fbx` | `T_Icon_EmergencyBackpack.png` | `SurvivalGear` | 1.25x | 5,042 | Evet | Hayır | `None` |
| 15 | **İlk Yardım Kiti** | `PF_FirstAidKit` | `SM_FirstAidKit.fbx` | `T_Icon_FirstAidKit.png` | `MedicalAndHygiene` | 1.30x | 3,124 | Evet | Hayır | `None` |
| 16 | **El Feneri** | `PF_Flashlight` | `SM_Flashlight.fbx` | `T_Icon_Flashlight.png` | `PowerAndLight` | 1.50x | 1,996 | Evet | Hayır | `None` |
| 17 | **Acil Durum Radyosu** | `PF_EmergencyRadio` | `SM_EmergencyRadio.fbx` | `T_Icon_EmergencyRadio.png` | `Communication` | 1.35x | 5,292 | Evet | Hayır | `None` |
| 18 | **Duvar Kliması** | `PF_AirConditioner` | `SM_AirConditioner.fbx` | `T_Icon_AirConditioner.png` | `HouseholdFixture` | 1.00x | 2,328 | Hayır | **Evet (`SecuredBracket`)** | `FallingTrauma` |

### Bölüm B: 14 Yeni KTMMOB İMO Afet & Zincirleme Dalga Varlığı

| # | Türkçe Adı | Prefab (`Prefabs/`) | FBX Mesh (`Art/Meshes/`) | İkon (`Art/Icons/`) | Kategori | Toy-Box Ölçek | Poligon | Toplanabilir | Sabitleme / Vana | Tetiklediği İkincil Afet |
|---|---|---|---|---|---|---|---|---|---|---|
| 19 | **Doğalgaz Ana Vanası** | `PF_GasValve` | `SM_GasValve.fbx` | `T_Icon_GasValve.png` | `UtilityValve` | 1.25x | 2,156 | Hayır | **Vana (Açık/Kapalı)** | `FireWave` (Yangın Dalgası) |
| 20 | **Su Kesme Vanası** | `PF_WaterValve` | `SM_WaterValve.fbx` | `T_Icon_WaterValve.png` | `UtilityValve` | 1.25x | 1,746 | Hayır | **Vana (Açık/Kapalı)** | `FloodWave` (Sel Dalgası) |
| 21 | **Elektrik Sigorta Panosu**| `PF_BreakerBox` | `SM_BreakerBox.fbx` | `T_Icon_BreakerBox.png` | `UtilityValve` | 1.15x | 1,398 | Hayır | **Şalter (Açık/Kapalı)** | `ElectricalShock` (Elektrik) |
| 22 | **Büyük Ahşap Kitaplık** | `PF_Bookcase` | `SM_Bookcase.fbx` | `T_Icon_Bookcase.png` | `HouseholdFixture` | 1.00x | 1,800 | Hayır | **Evet (`SecuredBracket`)** | `CorridorBlockage` |
| 23 | **Mutfak Buzdolabı** | `PF_Refrigerator` | `SM_Refrigerator.fbx` | `T_Icon_Refrigerator.png` | `HouseholdFixture` | 1.00x | 1,940 | Hayır | **Evet (`SecuredBracket`)** | `CorridorBlockage` |
| 24 | **Banyo Termosifonu** | `PF_WaterHeater` | `SM_WaterHeater.fbx` | `T_Icon_WaterHeater.png` | `HouseholdFixture` | 1.05x | 2,702 | Hayır | **Evet (`SecuredBracket`)** | `FloodWave` |
| 25 | **Sağlam Masa (Yaşam Üçgeni)**| `PF_SturdyTable` | `SM_SturdyTable.fbx` | `T_Icon_SturdyTable.png` | `HouseholdFixture` | 1.00x | 968 | Hayır | Güvenli Sığınak Alanı | `None` |
| 26 | **Deprem Düdüğü** | `PF_EmergencyWhistle` | `SM_EmergencyWhistle.fbx`| `T_Icon_EmergencyWhistle.png`| `Communication` | 1.85x | 1,322 | Evet | Hayır | `None` |
| 27 | **Evrak & DASK Dosyası** | `PF_DocumentPouch` | `SM_DocumentPouch.fbx` | `T_Icon_DocumentPouch.png` | `SurvivalGear` | 1.35x | 1,464 | Evet | Hayır | `None` |
| 28 | **Ağır Hizmet Eldiveni** | `PF_WorkGloves` | `SM_WorkGloves.fbx` | `T_Icon_WorkGloves.png` | `SurvivalGear` | 1.45x | 3,528 | Evet | Hayır | `None` |
| 29 | **Çok Amaçlı Pense/Çakı** | `PF_MultiTool` | `SM_MultiTool.fbx` | `T_Icon_MultiTool.png` | `SurvivalGear` | 1.65x | 968 | Evet | Hayır | `None` |
| 30 | **FFP3 Toz Maskesi** | `PF_DustMask` | `SM_DustMask.fbx` | `T_Icon_DustMask.png` | `MedicalAndHygiene` | 1.55x | 1,500 | Evet | Hayır | `None` |
| 31 | **Kırık Cam Tehlikesi** | `PF_ShatteredGlass` | `SM_ShatteredGlass.fbx` | `T_Icon_ShatteredGlass.png` | `EnvironmentalHazard`| 1.15x | 106 | Hayır | Çevresel Hasar | `None` |
| 32 | **Dökülmüş Moloz Yığını** | `PF_RubblePile` | `SM_RubblePile.fbx` | `T_Icon_RubblePile.png` | `EnvironmentalHazard`| 1.10x | 2,012 | Hayır | Çevresel Hasar | `CorridorBlockage` |
| 33 | **Bunker Kapısı / Kapağı (Ağır Sığınak Kapağı)** | `PF_BunkerHatch` | `SM_BunkerHatch.fbx` | `T_Icon_BunkerHatch.png` | `HouseholdFixture` | 1.00x | 6,420 | Hayır | **Menteşeli Ağır Kapak + Kilit Çarkı + Karanlık Kuyu (`HatchLid_Hinge`, `LockWheel_Pivot`, `HydraulicPiston_Pivot`, `WellPortal_Disc`, `WellShaft_Deep`)** | `None` (Güvenli Sığınak Girişi) |
