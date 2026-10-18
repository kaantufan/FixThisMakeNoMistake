# Too Late to Panic! — Oyun Referans ve Mimari Rehberi (v0.4.8 / Home V4.8)

> **Son Güncelleme:** 17 Ekim 2026 (`origin/main` @ `9e09445`, Tag: `v0.4.8`)  
> **GitHub Deposu:** `https://github.com/kaantufan/FixThisMakeNoMistake.git`  
> **Oyun Adı:** *Too Late to Panic!*  
> **Motor & Perspektif:** Unity 3D (Aktif sürüm: **İzometrik 3D Kesit Görünümlü Ev & Bahçe Simülasyonu**; FPS modu askıya alınmıştır)

---

## 1. Depo (Repository) ve Dal (Branch) Yapısı

Projede şu an iki farklı dal (branch) ve dizin yapısı bulunmaktadır:

1. **`origin/main` (Arkadaşının Pushladığı Ana Oyun Dalı - `v0.4.5`):**
   - Ana Unity projesi `TooLateToPanic/` alt klasöründe yer alır (`TooLateToPanic/Assets/Game/...`, `TooLateToPanic/ProjectSettings/...`, `TooLateToPanic/Packages/...`).
   - Orkestrasyon ve prompt araçları `Tools/GamejamOrchestra/` altındadır.
   - Ana çalıştırılabilir sahne: `TooLateToPanic/Assets/Game/Scenes/TooLateToPanicResidence.unity` (Home V4.5).
2. **`yarkin-assets` (Mevcut Yerel Çalışma Dalı):**
   - Kök dizinde (`c:\GameJam\Assets\_YarkinAssets\...`) Blender ile prosedürel üretilen 3D afet hazırlık modelleri (`.fbx`, `.blend`), ikonlar (`.png`), prefablar (`PF_*.prefab`) ve `DisasterPrepItem.cs` / `YarkinAssetPipelineBuilder.cs` araçları yer alır.
   - **Önemli Entegrasyon Notu:** `yarkin-assets` üzerindeki modeller ana oyuna (`TooLateToPanic/Assets/Game/...`) entegre edilirken `TooLateToPanic/` alt klasör yapısına ve `PanicModel` / `PanicExpedition` / `PanicHomeWorld` ID sistemine dikkat edilmelidir.

---

## 2. Oyunun Konsepti ve Temel Oynanış Döngüsü (Core Loop)

**Too Late to Panic!**, yaklaşan doğal afetlere (**Deprem, Sel, Yangın**) karşı çok katlı bir evi ve bahçesini süre dolmadan güçlendirdiğimiz, evdeki dolaplardan malzeme toplayıp **Afet Çantası (Emergency Bag)** hazırladığımız ve son saniyelerde bahçedeki **Sığınağa (Bunker)** yetişmeye çalıştığımız dalga (wave) tabanlı bir hayatta kalma ve afet hazırlık oyunudur.

### Adım Adım Dalga Döngüsü:
1. **Brifing & Hazırlık Başlangıcı:** Dalga başında yaklaşan afet türü (Deprem, Sel veya Yangın) ve o dalga için sığınakta bulunması gereken zorunlu **Su** ve **Gıda** miktarı bildirilir.
2. **Arama & Toplama (Gathering):** Evin katlarında (Bodrum, Zemin Kat, Üst Kat) ve garajda dolaşılarak dolaplar/kutular `E` ile açılır; içindeki 3D malzemeler `Z/X` ile seçilip `F` ile envantere (12 yuva) toplanır.
3. **İlk Yardım ve Afet Çantası Hazırlığı:** Envanter ekranında (`Tab` veya `B`) **2 Bandaj + 1 Antiseptik** birleştirilerek (`F`) **İlk Yardım Seti** üretilir. Su, gıda ve destek ekipmanları (Fener+Pil, Radyo+Pil, Battaniye, İlk Yardım Seti) `Enter` ile 8 yuvalı afet çantasına yerleştirilir.
4. **Ev & Bahçe Güçlendirme İşleri (Active Work Minigames):**
   - Evdeki mobilyalar taşınır (` Sağ Tık ` veya `4`), döndürülür (`R` / Fare Tekerleği) ve duvara/zemine **ankrajlanır** (`1`).
   - Duvarlar, pencereler, kapılar ve çatı **T0 → T1 → T2** kademelerine güçlendirilir (`3`).
   - Pencerelere **sel bariyeri** (`2`), mobilya/odalara **yangın tüpü** (`2`) kurulur.
   - Bahçede **drenaj kanalı**, **sel çiti**, **fidan dikimi** ve **yangın hortumu** işleri tamamlanır.
5. **Son 60 Saniye & Sığınağa Kaçış:** Süre 60 saniyenin altına indiğinde uyarı verilir. Süre bitmeden yeterli erzakla bahçedeki **Bunker** kapağından (`(11, 0, -7)`) içeri girilmelidir.
6. **Afet & Sonuç Sunumu:**
   - **Afet Sırası (8 sn):** Kamera evi dışarıdan gösterir; afet efektleri (sarsıntı/toz, sel suları, alev/duman) ve afet sesleri oynar.
   - **Afet Sonrası / Aftermath (4 sn):** Hasara göre enkaz/çamur/is atmosferi ve arama ışıklı kurtarma helikopteri görünür.
   - **Başarı (3 sn) veya Kayıp Sinematiği (1.35 sn uyarı + 6 sn sinematik):**
     - Toplam ev hasarı **%65'i aşarsa**, süre biterse veya çantadaki zorunlu su/gıda eksikse koşu kaybedilir (`GameOver`).
     - Başarılı olunursa sığınak içi başarı sahnesi oynar, ödül bütçe ve puan verilir.
7. **Hasar Gezisi (Inspection - Maks. 20 sn):** Oyuncu afetten çıkan evi gezip hangi eşyanın/duvarın ne kadar hasar aldığına bakar veya erken devam ederek sonraki dalgaya geçer.
8. **Yeni Dalga:** Ev hasarı sıfırlanır, kalıcı güçlendirmeler/ankrajlar ve eşya yerleşimi korunur, çantadaki su ve gıda tüketilir, sarf malzemeleri kaynaklarda yenilenir.

---

## 3. Afet Türleri, Dalga Kuralları ve Kayıp Koşulları

| Dalga (Wave) | Afet Türü | Hazırlık Süresi | Zorunlu Çanta Erzağı (Su + Gıda) |
| :--- | :--- | :--- | :--- |
| **Dalga 1–3** | **Deprem (Earthquake)** | 300 sn | 1 Günlük Su + 1 Günlük Gıda |
| **Dalga 4–6** | **Sel (Flood)** | 270 sn | 2 Günlük Su + 2 Günlük Gıda |
| **Dalga 7–9** | **Yangın (Fire)** | 240 sn | 3 Günlük Su + 3 Günlük Gıda |
| **Dalga 10+** | **Kontrollü RNG** (Üç afet arasından; aynı afet üst üste en fazla 2 kez) | 240 sn | 3 Günlük Su + 3 Günlük Gıda |

### Afetlere Göre Risk Azaltma:
- **Deprem:** Mobilyaların duvara/zemine sabitlenmesi (ankraj) ve yapıların (duvar/çatı/kapı/pencere) T1/T2 kademesine yükseltilmesi hasarı düşürür.
- **Sel:** Pencere sel bariyerleri, bahçe drenajı, sel çiti ve (sınırlı katkı sağlayan) fidanlar sel hasarını düşürür.
- **Yangın:** Ev içi yangın söndürücüler ve bahçe yangın hortumu yangın hasarını düşürür.
- **Kayıp Koşulları (Fail Conditions):**
  1. Hazırlık süresi (`TimeRemaining`) dolmadan bunkera girememek.
  2. Bunkera girildiğinde çantada o dalganın gerektirdiği miktarda su veya gıdanın bulunmaması.
  3. Afet sonunda normalize edilmiş toplam ev hasarının **%65.0** eşiğini aşması (`DamageLimit = 65f`).

---

## 4. Dünya Mimarisi ve Kat Planı (`PanicHomeWorld`)

- **Ev Oturumu:** `16×12 metre`, kat yüksekliği `3.2 metre`.
- **Bahçe Oturumu:** Yaklaşık `48×48 metre` (komşu evler, yol, ağaçlar, çitler ve `(11, 0, -7)` konumunda bahçe bunkeri).
- **3 Fiziksel Kat (Işınlanma yoktur, 16 basamaklı fiziksel merdivenlerle çıkılır/inilir):**
  - **Bodrum Kat (`y = -3.2`, Level -1):** Teknik depo, jeneratör, çalışma tezgâhı, çamaşır alanı ve **Bodrum Kileri (`basement_pantry`)**.
  - **Zemin Kat (`y = 0.0`, Level 0):** Batıda salon (koltuk, masa, sandalye, TV sehpası, salon bitkisi, şömine, **Salon Kitaplığı**), ortada giriş holü ve merdiven, doğuda mutfak (**Buzdolabı**, **Mutfak Dolabı**, depo dolabı) ve evin doğusuna bitişik **Garaj** (dışarıdan girilir; araç, bisiklet, alet duvarı ve **Garaj Alet Kutusu** içerir).
  - **Üst Kat (`y = 3.2`, Level 1):** Yatak odası / çalışma alanı (yatak, üzerinde bilgisayar olan çalışma masası, **Üst Kat Dolabı**, **Üst Kat Kitaplığı**), güneyde kapıdan çıkılan **Balkon**, kuzeydoğuda güney duvarındaki kapıdan girilen **Banyo** (küvet, klozet, lavabo ve batı duvarında **Banyo Ecza Dolabı**).
- **Kamera & Kesit Görünümü:** Takip eden ortografik izometrik kamera (`orthographicSize` `PageUp`/`PageDown` ile `4..10` arası yakınlaştırılabilir). Oyuncu eve girdiğinde çatı ve ön duvarlar otomatik kesit (cutaway) görünümüne geçer; alt kattayken üst kat görsel olarak gizlenir ama fiziksel collider'lar korunur.
- **Akıllı Kapı Güvenliği (`PanicDoorSafety`):** Kapılar menteşelidir (hinged), oyuncunun bulunduğu tarafın tersine (geçiş yönüne) açılır. Açılırken veya kapanırken oyuncu kapı süpürme alanında durursa kapı oyuncuya çarpmadan durur (`closeBlocked`), oyuncu çekilince hareketini tamamlar.

---

## 5. Kontroller ve Arayüz (HUD)

| Tuş / Girdi | İşlev |
| :--- | :--- |
| **WASD / Ok Tuşları** | İzometrik kameraya göre hareket |
| **Left Shift** | Koşu (`2.7 m/s` → `4.2 m/s`) |
| **Space** | Zıplama (Etkin bir iş mini oyunundayken zıplatmaz, mini oyun girdisi olur) |
| **E** | Kapı/dolap aç-kapat, bahçe işini başlat, sığınağa (bunker) gir |
| **Z / X** | Açık dolaptaki malzeme yığınları arasında seçim yap |
| **F** | Seçili malzemeyi dolaptan al / Envanter açıksa **İlk Yardım Seti** üret |
| **Q** | Yakındaki etkileşim hedefleri arasında geçiş yap |
| **T** | Sabitleme yüzeyini değiştir (`Duvar Ankrajı` ↔ `Zemin Ankrajı`) |
| **1** | Seçili eşyayı sabitle (ankrajla) veya sabitlemeyi sök |
| **2** | Pencerede sel bariyeri kur / Diğer hedeflerde yangın önlemi kur |
| **3** | Yapıyı güçlendir (`T0 → T1 → T2`) |
| **4** veya **Sağ Tık** | Eşya taşıma modunu başlat |
| **Taşırken Fare / R / Sol Tık** | Fareyle konum seç, `R` veya tekerlekle `45°` döndür, `Sol Tık` veya `Enter` ile bırak, `Sağ Tık`/`Esc`/`Backspace` ile iptal et |
| **Tab / B** | Envanter (12 yuva) ve Afet Çantası (8 yuva) panelini aç/kapat |
| **PageUp / PageDown** | İzometrik kamerayı yakınlaştır / uzaklaştır |
| **F2** | **Geliştirici (Developer / Hile) Menüsünü** aç/kapat |
| **Escape** | Duraklatma menüsü / Etkin işi veya taşımayı iptal et |

---

## 6. Malzemeler, Kaynak Noktaları ve Ekonomi

### Envanter ve Yığın Kuralları:
- **Envanter:** 12 yuva. **Afet Çantası:** 8 yuva.
- **Tekli taşınanlar (Stack Limit = 1):** `flashlight` (El feneri), `radio` (Acil radyo), `blanket` (Battaniye), `firstaid` (İlk yardım seti), `hose` (Yangın hortumu), `extinguisher` (Yangın tüpü).
- **Beşli yığınlananlar (Stack Limit = 5):** `water` (Su), `food` (Konserve gıda), `bandage` (Bandaj), `antiseptic` (Antiseptik), `battery` (Pil), `screws` (Vida kutusu), `bracket` (L braket), `plank` (Ahşap tahta), `sandbag` (Kum torbası), `seedling` (Fidan).
- **Yenilenebilir (Her dalgada tekrar çıkan) sarflar:** `water`, `food`, `bandage`, `antiseptic`, `battery`.

### Evdeki 8 Ana Malzeme Kaynağı (`LootSource`):
1. **Buzdolabı (`fridge` - Zemin Kat Mutfak):** 2× Su, 2× Gıda
2. **Mutfak Dolabı (`cabinet` - Zemin Kat Mutfak):** 1× Gıda, 1× Su, 1× Bandaj
3. **Salon Kitaplığı (`bookshelf` - Zemin Kat Salon):** 1× El Feneri, 2× Pil, 1× Battaniye
4. **Üst Kat Dolabı (`up_cabinet` - Üst Kat):** 1× Gıda, 1× Su, 1× Bandaj, 1× Antiseptik
5. **Üst Kat Kitaplığı (`up_bookshelf` - Üst Kat):** 1× Radyo, 1× Pil, 1× Battaniye, 2× Vida
6. **Banyo Dolabı (`bath_cabinet` - Üst Kat Banyo):** 3× Bandaj, 2× Antiseptik, 1× Pil
7. **Garaj Alet Kutusu (`garage_tools` - Zemin Kat Garaj):** 4× Vida, 3× Braket, 1× Yangın Tüpü
8. **Bodrum Kileri (`basement_pantry` - Bodrum Kat):** 2× Su, 2× Gıda, 4× Kum Torbası, 4× Tahta, 4× Fidan, 1× Hortum, 1× Yangın Tüpü

### İş Maliyetleri ve Aktif İş Mini Oyunları (`PanicWorkSession`):
Başlangıç bütçesi **1000**'dir. Her iş 4 adımdan (bağlantıdan) oluşur. Önce `A/D` ile ibre yeşil hedefe hizalanıp `Enter` ile alet yerleştirilir, ardından işin ailesine göre uygulama yapılır:

| İş Eylemi (`action`) | Mini Oyun Ailesi | Bütçe Maliyeti | Gerekli Malzemeler |
| :--- | :--- | :--- | :--- |
| **Duvar/Zemin Ankrajı** (`anchorwall` / `anchorfloor`) | **Ratchet** (`A/D` ile 3 dönüş + `Space`) | İlk kez: **60** (Sonraki taşımalarda 0) | İlk kez: `1 Braket + 2 Vida` (Söküp takmada 0) |
| **Yapı Güçlendirme T0→T1** (`upgrade`) | **Ratchet** | **120** | `1 Tahta + 2 Vida` |
| **Yapı Güçlendirme T1→T2** (`upgrade`) | **Ratchet** | **180** | `2 Tahta + 3 Vida` |
| **Sel Çiti** (`fence`) | **Ratchet** | **60** | `3 Tahta + 2 Vida + 1 Kum Torbası` |
| **Sel Bariyeri** (`flood` - Pencere) | **Timing** (Yeşilde `Space` dokunuşu) | **80** | `2 Kum Torbası + 1 Tahta` |
| **Drenaj Kanalı** (`drain` - Bahçe) | **Timing** | **50** | `2 Kum Torbası + 1 Tahta` |
| **Fidan Dikimi** (`plant` - Bahçe) | **Timing** | **30** | `1 Fidan` |
| **Yangın Söndürücü** (`fire` - Ev İçi) | **Balance** (`A/D` ile basıncı yeşil bantta tutarken `Space` basılı tut) | **75** | `1 Yangın Tüpü` |
| **Yangın Hortumu** (`hose` - Bahçe) | **Balance** | **80** | `1 Yangın Hortumu` |
| **İlk Yardım Üretimi** (`craftfirstaid` - Envanterde `F`) | Anlık Üretim | **0** | `2 Bandaj + 1 Antiseptik` |
| **Fener / Radyo Çantaya Koyma** (`packbattery`) | Envanterde `Enter` | **0** | `1 Pil` |

> **Atomik Tüketim Kuralı:** İş devam ederken iptal edilirse (`Esc`/`Backspace`) hiçbir bütçe veya malzeme harcanmaz. Yalnızca 4 adım başarıyla bittiğinde atomik olarak düşülür. Eksik malzeme varsa HUD üzerinde ikonlu **Eksik / Var / Gerek** kartı açılır.

---

## 7. Geliştirici / Hile Menüsü (Home V4.5 - `PanicDeveloperOptions`)

- `F2` tuşuyla veya Başlık/Duraklatma menüsündeki **Geliştirici** düğmesiyle açılır.
- Menü açıkken oyun içi hareket ve hazırlık saati durur.
- **Seçenekler:**
  1. **Sınırsız Bütçe (`InfiniteBudget`):** Harcamalarda para düşmez.
  2. **Malzeme Harcamama (`FreeMaterials`):** İşlerde, ilk yardım üretiminde ve fener/radyo pilinde malzeme aranmaz/tüketilmez.
  3. **Hazırlık Süresini Dondur (`FreezePreparationTime`):** Hazırlık sayacını durdurur (afet/sonuç sayaçları normal ilerler).
  4. **+1000 Bütçe Ekle (`AddDeveloperBudget`):** Tek tıklamayla +1000 bütçe verir.
- **Adil Skor Koruması (`CheatedRun`):** Menüyü açmak hile sayılmaz; ancak herhangi bir seçeneği etkinleştirmek veya bütçe eklemek o koşuyu kalıcı olarak **`HİLELİ KOŞU`** işaretler ve normal yerel yüksek skor tablosuna (`Highscores`) yazılmasını engeller.

---

## 8. Home V4.8 Yenilikleri ve Modlar (v0.4.8 Sürümü)

1. **Yeni Başlangıç Menüsü & Oyun Modları:**
   - **`HAZIRLIĞA BAŞLA` (Kısa Hazırlık Akışı):** Hızlı ve odaklı afet hazırlık senaryosu (ortalama ~50-90 saniye hazırlık).
   - **`SERBEST OYUN` (Sandbox / Serbest Mod):** Klasik uzun dalgalı hazırlık ve ev düzenleme.
   - **`FELAKET SİMÜLASYONU OYNA`:** 4 farklı afeti doğrudan test etme imkânı:
     - 🌋 **Earthquake (Deprem)**
     - 🌊 **Flood (Sel)**
     - 🔥 **Fire (Yangın)**
     - 🌪️ **Storm (Fırtına)**
2. **Sinematik ve Kontrol İyileştirmeleri:**
   - Afet sinematiklerinde **`Enter`** ile kamera sekansını atlama desteği.
   - Doğal afet veya atlama sonrasında tüketilen su ve gıdanın çanta yuvalarından görsel ve mantıksal olarak düşmesi.
   - Yangın sinematiğinde üst kat penceresinden çıkan gerçekçi duman partikülleri.
   - Ağır hasar (%65 aşımı), çanta erzak eksiği ve hazırlık süresi bitimi durumlarında ayrıntılı ve açıklayıcı kayıp panelleri.
3. **Geliştirici Seçeneklerine Ek:**
   - `F2` menüsüne eklenen **`NoLoss`** (Hasar/Kayıp Koruması) seçeneği (koşuyu kaybetmeden hasarları inceleme olanağı).

---

## 9. Kod Mimarisi ve Önemli Dosyalar (`TooLateToPanic/Assets/Game/`)

### Runtime (`Assets/Game/Runtime/`)
- **`PanicModel.cs`:** Oyunun saf veri ve kural motoru (`PanicPhase`, `DisasterType`, `ItemKind`, dalga ilerleyişi, bütçe, hasar hesaplaması `CalculateDamage`, sığınak/çanta kuralları, yerel yüksek skor kaydı).
- **`PanicExpedition.cs`:** Envanter (12 slot), Afet Çantası (8 slot), 8 `LootSource` kaynağı, malzeme tarifleri (`MaterialCosts`), bahçe görevleri ve 3'lü mini oyun motoru (`PanicWorkSession`: `Ratchet`, `Timing`, `Balance`).
- **`PanicHomeWorld.cs` (Partial Class):** Ana `MonoBehaviour`. Sahneyi, katları, merdivenleri, eşyaları ve bahçeyi kurar (`BuildWorld`), oyuncu hareketini (`CharacterController`), izometrik kamerayı ve etkileşim odaklamasını (`RefreshFocus`, `Interact`) yönetir.
- **`PanicHomeLife.cs` (Partial):** Kap/dolap açma, içindeki 3D eşyaları seçme/toplama, zıplama, bildirimler ve radyo etkileşimi.
- **`PanicHomePlacement.cs` (Partial):** Mobilyaları fare veya klavyeyle taşıma, 45° döndürme ve çarpışma/geçerlilik kontrolü (`ValidatePlacement`).
- **`PanicDoorSafety.cs` (Partial):** Kapıların oyuncuya çarpmasını önleyen fiziksel süpürme/engel kontrolü.
- **`PanicHomeCinema.cs` (Partial):** Sığınağa giriş, afet dış çekimleri, kurtarma helikopteri ve kayıp/başarı kamera sekansları.
- **`PanicHomeHud.cs`:** Tüm oyun içi arayüz, üst durum şeridi, envanter/çanta ekranı, yakın plan 3D iş mini oyunu penceresi (`WorkTexture`), eksik malzeme kartı ve `F2` Geliştirici Menüsü.
- **`PanicHomeEffects.cs`:** Yalnızca afet seslerini (deprem, sel, yangın, fırtına) ve atmosferik hava/efekt geçişlerini yönetir.
- **`PanicDeveloperOptions.cs`:** V4.5 / V4.8 geliştirici/hile seçenekleri veri sınıfı.
- **`PanicSimulationWorld.cs` & `PanicSimulationHud.cs`:** V4.8 ile eklenen afet simülasyonu mantığı ve arayüzü.

### Prosedürel Sanat ve Bake Sistemi (`Assets/Game/Art/` & `Editor/`)
- **`PanicHomeArt.cs`, `PanicResidenceArt.cs`, `PanicWorkArt.cs`, `PanicAtmosphereArt.cs`, `PanicCharacterArt.cs`, `PanicDisasterArt.cs`, `PanicAttachmentArt.cs`:** Evin katlarını, mobilyaları, 16 malzemenin 3D modellerini/96px ikonlarını, iş aletlerini (tornavida, kürek, sulama kabı, vana) ve afet efektlerini tamamen kodla (prosedürel olarak) üreten sınıflar.
- **`Assets/Game/Resources/ResidenceGenerated/` & `ResidenceArt/`:** Prosedürel üreticilerden önceden fırınlanmış (baked) 104 prefab ve mesh önbelleği.
- **`PanicResidenceBuild.cs`:** Windows çalıştırılabilir paketini (`TooLateToPanic.exe`) derleyen Editor build betiği.
