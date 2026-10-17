# Too Late to Panic! — Güncel Tasarım Kararları, Hata Düzeltmeleri ve İmplementasyon Planı

> **Hedef:** Takım İçi Geliştirme & İmplementasyon Dokümanı (Hand-off)  
> **Kapsam:** Kesinleşmiş Oynanış/Mekanik Değişiklikleri, Kritik Bugfix'ler, Seviye Tasarımı (Level Design), UI/UX ve Görsel Revizyonlar  
> **İlgili Kod Tabanı:** `Assets/Game/Runtime/` (`PanicModel`, `PanicExpedition`, `PanicHomeWorld`, `PanicHomeLife`, `PanicHomeHud`, `PanicJuryDemo`) & `Assets/Game/Art/` (`PanicHomeArt`, `PanicResidenceArt`) & `Assets/_YarkinAssets/`

---

## 1. Kesinleşen Ana Tasarım ve Mekanik Kararları (Core Gameplay Pivot)

Toplantı ve alınan notlar doğrultusunda envanter, eşya toplama (looting), üretim (crafting) ve sığınak (bunker) mekaniklerinde aşağıdaki **kesin kararlar** alınmıştır:

### 1.1. Eşya Toplama (Looting) Ayrımı: FPS Açık Dünya Eşyaları vs. Dolap İçi Auto-Loot
Eski sistemde hem afet çantası erzaklarının hem de inşaat malzemelerinin dolap içlerinden (`LootSource` / `lootTray`) `Z/X` ve `F` tuşlarıyla seçilerek toplanması iptal edilmiştir. Yeni yapı ikiye ayrılmıştır:

1. **Afet Çantası Eşyaları (FPS Modunda & Açıkta / Zeminde / Masa Üstünde):**
   * **Kapsam:** `Su (water)`, `Gıda (food)`, `İlk Yardım Seti (firstaid)`, `El Feneri (flashlight)`, `Pil (battery)`, `Radyo (radio)`, `Battaniye (blanket)` vb.
   * **Yerleşim:** Bu eşyalar artık dolapların içinde **bulunmayacak**. Evin çeşitli odalarında **zeminde, masaların üzerinde, mutfak tezgâhında, raflarda veya koltuk/yatak üzerinde** fiziksel 3D objeler olarak açıkta duracak (`Assets/_YarkinAssets/Prefabs/PF_*.prefab` modelleri hazır).
   * **Toplama Mekaniği:** Son karar değişikliği ile afet çantası için toplanacak bu eşyalar **FPS modunda** doğrudan dünyadaki fiziksel objeyle etkileşime girilerek toplanacak.

2. **İnşaat ve Güçlendirme Malzemeleri (Dolap İçlerinden Auto-Loot & Currency Havuzu):**
   * **Kapsam:** `Vida (screws)`, `L-Braket (bracket)`, `Tahta/Kalas (plank)`, `Kum Torbası (sandbag)`, `Fidan (seedling)` vb. yapı/ankraj malzemeleri.
   * **Currency (Kaynak Havuzu) Statüsü:** Bu materyaller artık 12 slotluk envanterde yer kaplayan birer `Inventory Item` **olmayacak**. Bütçe (`Budget`) gibi oyuncunun üzerinde/HUD'da sayaç olarak tutulan **Currency (Malzeme Havuzu)** haline gelecek (`🔩 Vida: X`, `📐 Braket: Y`, `🪵 Tahta: Z` vb.).
   * **Auto-Loot (Otomatik Toplama) Mantığı:** İnşaat malzemeleri etraftaki dolapların, çekmecelerin ve alet kutularının içinden çıkacak. Oyuncu dolapla etkileşime girip kapağı açtığında (`E`), ayrı bir seçim menüsü açılmadan **belirli bir RNG (rastgele miktar) ile otomatik olarak (auto-collect)** oyuncunun malzeme havuzuna eklenecek ve ekranda toplama bildirimi (`+3 Vida, +1 L-Braket` vb.) gösterilecek.

---

### 1.2. Bandaj ve Antiseptik Kaldırıldı — Sadece Hazır "İlk Yardım Seti" Olacak
* **Mevcut Durum:** Dolaplardan `2 Bandaj + 1 Antiseptik` toplanıp envanterde `F` tuşuyla `İlk Yardım Seti` üretiliyordu (`craftfirstaid`).
* **Yeni Karar:** Oyunda karakter can barı (HP) bulunmadığı için tekil `bandage` ve `antiseptic` eşyaları ile ilk yardım craft mekaniği **tamamen kaldırılacak**.
* **Uygulama:** Oyunda yalnızca doğrudan toplanabilir hazır **İlk Yardım Seti (`firstaid` / `PF_FirstAidKit`)** bulunacak (ör. banyo tezgâhı üzerinde, yatak odası komodininde vb. açıkta).
* **Kod Karşılığı:**
  * `PanicExpedition.cs`: `LootSource` tablolarından `bandage` ve `antiseptic` çıkarılacak; `CraftFirstAid()` metodu ve `PanicHomeHud.cs` içindeki `"F - İLK YARDIM"` butonu devre dışı bırakılacak.

---

### 1.3. Bunker (Sığınak) Girişi Sadece Bodrum Kata Taşınacak
* **Mevcut Durum:** Bunker kapağı bahçede `(11, 0, -7)` koordinatında bulunuyor.
* **Yeni Karar:** Bahçedeki sığınak girişi **kaldırılacak**. Bunker kapısı **yalnızca Bodrum Kata (`Level -1`, `y = -3.2`)** taşınacak (yeri tamamen bodruma alınacak).
* **Uygulama:**
  * Bahçedeki dış bunker objesi silinecek, bodrum katın uygun bir duvarına veya zeminine sığınak giriş kapısı yerleştirilecek.
  * Son 60 saniye kaçış hedefi ve kamera sinematik odak noktası (`BunkerPosition`) bodrumdaki yeni konuma güncellenecek.
  * **Kod Karşılığı:** `PanicHomeWorld.cs` (`BunkerPosition`), `PanicModel.cs` (bunker objesi koordinatı), `PanicJuryDemo.cs` (hedef sığınak noktası).

---

### 1.4. Afet Çantası Fiziki Olarak Eve Eklenecek
* **Mevcut Durum:** Afet çantası yalnızca `Tab / B` ile açılan soyut bir UI sekmesiydi.
* **Yeni Karar:** Afet çantası (`PF_EmergencyBackpack`) evin içine (ör. giriş holüne, askılık/vestiyer yanına veya masa üstüne) **fiziki 3D nesne** olarak eklenecek.
* **Uygulama:** Oyuncu fiziksel afet çantası ile etkileşime girerek çantayı alabilecek veya topladığı erzakları (Su, Gıda, İlk Yardım Seti, Fener, Radyo, Battaniye) çantaya yerleştirebilecek.

---

### 1.5. Envanter: Sürükle-Bırak (Drag & Drop) ve Yığınlanma (Stacking) Düzeltmesi
* **Drag & Drop Envanter:**
  * Envanter yuvaları ile Afet Çantası yuvaları arasında fare ile **Sürükle ve Bırak (Drag & Drop)** desteği eklenecek (`PanicHomeHud.cs`).
* **Envanterde Stacklenme (Yığınlanma) Problemi:**
  * **Sorun (`PanicExpedition.cs` -> `FindStorageSlot`):** Mevcut kod `slot.Count + count <= limit` kontrolü yaptığı için, slotta 4 eşya varken 2 eşya daha alındığında (`4 + 2 = 6 > 5`) mevcut slotu 5'e tamamlamak yerine yeni bir boş slot arıyor; boş slot yoksa eşyayı hiç almıyor, varsa aynı eşyayı parçalı slotlara bölüyor.
  * **Çözüm:** Klasik *Partial Fill (Kısmi Doldurma)* algoritmasına geçilecek. Önce mevcut aynı türdeki yığın `StackLimit` değerine tamamlanacak, kalan miktar (overflow) yeni slota aktarılacak. Ayrıca Drag & Drop ile iki yarım yığın üst üste bırakıldığında birleşecek.

---

## 2. Kritik Hata Düzeltmeleri (Bugfixes & Collisions)

### 2.1. Pause Menüsünde Zamanın Akmaya Devam Etmesi
* **Sorun:** Oyun duraklatıldığında (`Esc`), özellikle `HAZIRLIĞA BAŞLA` (`PanicJuryDemo`) akışında hazırlık süresi ve afet geri sayımı arkada akmaya devam ediyor.
* **Kök Neden:** `PanicHomeHud.SetPaused(true)` yalnızca `World.InputLocked = true` yapıyor; ancak `PanicHomeWorld.Update()` içindeki `TickJuryDemo()` her karede `Time.unscaledDeltaTime` ile çalışmaya devam ediyor (`PanicJuryDemo.cs:149`).
* **Çözüm:**
  * Pause menüsü açıkken (`hud.Paused == true`) hem `Model.Tick(dt)` hem de `TickJuryDemo()` kesin olarak durdurulacak.
  * Pause ekranındaki `"KONTROLLER DURDU · SÜRE DEVAM EDİYOR"` ibaresi kaldırılıp tüm modlarda süre gerçekten dondurulacak.

### 2.2. İç Mutfak Duvarının Üstünden Zıplanabilmesi
* **Sorun:** Zemin katta koridor/salon ile mutfağı ayıran iç bölme duvarının üstünden `Space` ile zıplanarak geçilebiliyor.
* **Kök Neden:** `PanicHomeArt.cs:235` içindeki `Partition(divisions, "east_north", ...)` ve `"east_south"` duvarları kesit görünümü için `height = 0.88f` (88 cm) olarak üretilmiş. Karakterin zıplama tepe noktası (`gravitySpeed = 5.8f`) bu yüksekliği aşıyor.
* **Çözüm:** Duvarın görsel mesh yüksekliği kesit görünümü için korunacak (veya kamera açısına göre dinamik kesilecek), ancak fiziksel `BoxCollider` yüksekliği kat tavanına kadar (`height = 3.2f`, `center.y = 1.6f`) uzatılacak.

### 2.3. Tuvalet / Banyo Duvarının Bozuk Olması ve Üstünden Zıplanması
* **Sorun:** Üst kattaki banyo/tuvalet duvarlarının üstünden zıplanarak odaya girilebiliyor.
* **Kök Neden:** `PanicResidenceArt.cs:100` (`BuildBathroom`) içinde `bath_west`, `bath_south_left` ve `bath_south_right` duvarları `height = 1.16f` olarak tanımlanmış ve `PanicHomeLife.cs:101` içindeki collider ataması yalnızca `HomeV3_` / `HomeV4_` isimli alt objeleri taradığı için `Partitions` grubundaki banyo duvarları tam boy fiziksel engele sahip değil.
* **Çözüm:** Banyo duvarlarının (`bath_west`, `bath_south_left`, `bath_south_right`) fiziksel collider'ları tavana kadar (`3.2m`) tam boy `BoxCollider` olarak bağlanacak.

---

## 3. Bina Mimarisi, Kamera Kesiti (Cutaway) ve Seviye Tasarımı

### 3.1. Garajın Ev İçi Kapısı Eklenecek
* **Mevcut Durum:** Garaja yalnızca evden dışarı çıkıp bahçeden dolaşarak girilebiliyor.
* **Yeni Karar:** Evin doğu duvarı ile garajın batı duvarı (`x = 8.0 .. 8.2`) arasına (mutfak/koridor bağlantısına) **ev içi geçiş kapısı** eklenecek.
* **Uygulama:**
  * `PanicHomeArt.BuildFloor(0)` doğu duvarında ve `PanicResidenceArt.BuildGarage()` batı duvarında (`solid_garage_west`) kapı boşluğu (aperture) açılacak ve menteşeli iç kapı (`hinges`) yerleştirilecek.

### 3.2. Garaj Arka Duvarının Kamera Açısı ile Transparana Dönüşmemesi
* **Sorun:** Kamera açısı döndürüldüğünde (`PanicCameraOrbit`), garajın arka (kuzey) duvarı (`solid_garage_north`) hep opak kalıyor ve garaj içini görmeyi engelliyor.
* **Kök Neden:** `PanicHomeLife.cs:156` (`UpdateResidenceVisibility`) içinde sadece `Roof`, `FrontHighWalls` ve `solid_garage_west` gizleniyor; `BackWalls` altındaki `solid_garage_north` kamera açısına (`cameraSide`) göre kontrol edilmiyor.
* **Çözüm:** Garajın tüm dış duvarları (`north`, `east`, `south`, `west`) kameranın bakış yönü vektörüne (`View.transform.position - player.position`) bağlanarak kamerayla oyuncu arasına girdiğinde transparan/kesit (cutaway) durumuna geçecek.

### 3.3. Üst Kat Sağ ve Arka Cephelere Duvar Eklenmesi (Kamera Yön Değişimi İçin)
* **Sorun:** Üst katta sağ (doğu) ve arka (kuzey) cephelerde duvar/kesit yapısı kamera döndürüldüğünde transparanlık efektinin devam etmesini sağlamıyor.
* **Çözüm:** Üst katın (`floor = 1`) sağ ve arka cephe duvarları tam olarak eklenecek ve kamera hangi yöne dönerse dönsün kameraya yakın olan cephe duvarları dinamik transparanlık / kesit efektine girecek, arkada kalan cepheler ise opak duvar olarak görünmeye devam edecek (`ApplyFloorVisibility` revizyonu).

---

## 4. Görsel, Atmosfer ve Asset İyileştirmeleri

### 4.1. Bodruma Girince Etrafın Kararması
* **Sorun:** Oyuncu bodrum kata (`ActiveFloor == -1`) indiğinde üst katlar gizlendiği için dış ortamın güneş ışığı (`Overcast dusk sun`) ve açık gökyüzü ambiyansı bodrumu gündüz gibi aydınlatıyor.
* **Çözüm:**
  * `ActiveFloor == -1` olduğunda `RenderSettings.ambientSkyColor`, `ambientEquatorColor` ve ana Directional Light şiddeti (`sun.intensity`) yumuşak bir geçişle (`Mathf.Lerp`) karartılacak, kamera arka planı (`View.backgroundColor`) koyu tona çekilecek.
  * Bodrum kat yalnızca içerideki sıcak nokta ışıklarla (point light / çalışma lambası) loş ve atmosferik biçimde aydınlanacak.

### 4.2. Şöminenin Düzeltilmesi ve Daha İyi Asset Kullanılması
* **Sorun:** Salondaki şömine (`PanicResidenceArt.cs:427` -> `Fireplace(Builder b)`) basit prosedürel kutulardan oluşuyor ve görsel kalitesi düşük.
* **Çözüm:** Prosedürel kutu yığını yerine daha detaylı ve kaliteli bir 3D şömine modeli/asset'i kullanılacak, ateş/kor aydınlatması (`hearthLight` / `hearthFlame`) yeni modele oturtulacak.

---

## 5. Arayüz (UI), Menü Kontrolleri ve Metin Düzeltmeleri

### 5.1. Ana Menüde WASD ve Sağ/Sol Ok Tuşları ile Hareket
* **Sorun:** Ana menüde (`PanicHomeHud.cs:475` -> `DrawTitle`) `SERBEST OYUN` (sol) ve `NASIL OYNANIR` (sağ) butonları yan yana durmasına rağmen menüde sadece `W / S` ve `Yukarı / Aşağı` ok tuşları çalışıyor; `A / D` ve `Sol / Sağ` ok tuşları çalışmıyor.
* **Çözüm:** Ana menü buton geçişlerine `KeyCode.A`, `KeyCode.D`, `KeyCode.LeftArrow` ve `KeyCode.RightArrow` eklenecek; yan yana olan butonlar arasında sağ/sol hareketi, alt/üst satırlar arasında yukarı/aşağı hareketi doğal çalışacak.

### 5.2. Pause Menüsünün WASD ve Enter ile Gezilebilir Olması
* **Sorun:** Pause menüsü (`PanicHomeHud.cs:1020` -> `DrawPause`) açıldığında oyuncu fare (mouse) kullanmaya mecbur kalıyor; klavyeden gezilemiyor ve `Enter` ile seçilemiyor.
* **Çözüm:**
  * Pause menüsüne `_pauseChoice` seçim durumu eklenecek.
  * `W / A / S / D` ve `Ok Tuşları` ile `"DEVAM ET"`, `"GELİŞTİRİCİ · F2"`, `"YENİDEN BAŞLA"`, `"BAŞLIĞA DÖN"` butonları arasında gezilebilecek.
  * `Enter` (`Return` / `KeypadEnter`) tuşu ile seçili buton tetiklenecek.

### 5.3. "Tüpü Yerleştir / Hazırla" İbaresinin Düzeltilmesi
* **Sorun:** Etkileşim ve görev metinlerindeki `"Tüpü yerleştir"` / `"Tüpü hazırla"` ifadesi mutfak tüpü gibi algılanıyor, yeterince net değil.
* **Çözüm:** Tüm HUD, görev ve bağlam ipucu metinlerinde (`PanicJuryDemo.cs`, `PanicJuryPresentation.cs`, `PanicExpedition.cs`, `PanicHomeHud.cs`) bu ifade **"Yangın tüpünü yerleştir"** veya **"Yangın söndürücüyü hazırla"** olarak güncellenecek.

---

## 6. Ekstra Öneriler (İmplementasyon Sırasında Değerlendirilebilecek İyileştirmeler)

> *Not: Aşağıdaki maddeler zorunlu değildir; yukarıdaki revizyonlar kodlanırken teknik uyumu kolaylaştıracak ve oyun hissiyatını (game feel) güçlendirecek tamamlayıcı önerilerdir.*

1. **Auto-Loot Görsel & İşitsel Geri Bildirimi (Floating Loot Toast):**
   * Dolap açıldığında inşaat materyalleri otomatik olarak `Currency` havuzuna eklendiği için, oyuncunun dolaptan ne kazandığını net görmesi adına dolabın üzerinde veya HUD'da kısa süreli `+3 Vida, +1 L-Braket` şeklinde kayan bir bildirim ve metalik toplama sesi oynatılması (oyuncunun "dolap boş mu çıktı?" diye şüphelendirmemesi için).
2. **FPS Toplama Modu ile Sabitleme/İnşa Etkileşimlerinin Netleştirilmesi:**
   * Afet çantası eşyaları FPS modunda yerden/masadan toplanırken ekranın ortasında küçük bir nişangâh (crosshair/reticle) ve üzerine bakılan eşyanın adını gösteren temiz bir `[E] Su Şişesini Al` etiketi gösterilmesi.
3. **Bodrumdaki Bunker Kapısı İçin Yönlendirici İpucu (Son 60 Saniye):**
   * Bunker bahçeden bodruma alındığı için, son 60 saniye uyarısı girdiğinde bodrum merdiven girişinde ve bodrumdaki sığınak kapısında hafif yanıp sönen bir acil durum ikaz lambası (beacon) olması oyuncunun yeni sığınak yerini anında bulmasını sağlar.

---

## 7. Hızlı İmplementasyon Kontrol Listesi (Checklist)

- [ ] **Envanter & Loot Devrimi:**
  - [ ] Vida, braket, tahta, kum torbası vb. inşaat materyallerini envanter slotlarından çıkarıp HUD üzerinde `Currency` (sayısal kaynak havuzu) yap.
  - [ ] Dolap açıldığında (`ToggleContainer`) içindeki inşaat materyallerini RNG ile direkt `Currency` havuzuna **Auto-Loot** olarak ekle.
  - [ ] Su, gıda, ilk yardım seti, fener, pil, radyo, battaniyeyi dolap içlerinden çıkarıp evin içinde zemine/masalara fiziksel obje olarak yerleştir; **FPS modunda** toplanmasını sağla.
  - [ ] `Bandaj` ve `Antiseptik` itemlarını ve `F` ile ilk yardım üretimini kaldır; sadece hazır `İlk Yardım Seti` bırak.
  - [ ] Fiziki `Afet Çantası (PF_EmergencyBackpack)` objesini eve ekle.
  - [ ] Envanterde **Drag & Drop** ve **Partial Stack (Kısmi Yığınlama)** hatasını düzelt.
- [ ] **Seviye Tasarımı & Duvar/Kamera Hataları:**
  - [ ] **Bunker** girişini bahçeden kaldırıp sadece **Bodrum Kata** ekle.
  - [ ] **Garaj** ile ev arasına **ev içi kapı** ekle.
  - [ ] **İç mutfak duvarı** ve **tuvalet/banyo duvarı** collider yüksekliklerini tavana (`3.2m`) çıkar (üstünden zıplanmasını engelle).
  - [ ] **Garaj arka duvarı** ve **üst kat sağ/arka cephe duvarları** için kamera açısına göre dinamik transparanlık (cutaway) uygula.
- [ ] **Atmosfer & Görsel:**
  - [ ] **Bodruma girince** etrafın kararmasını (ambient & directional light kısılması) sağla.
  - [ ] **Şömineyi** daha kaliteli bir 3D asset ile değiştir.
- [ ] **UI / Kontroller / Zaman:**
  - [ ] **Pause menüsünde** zamanın akmasını tüm modlarda (`JuryDemo` dahil) durdur.
  - [ ] **Pause menüsüne** `W/A/S/D` ile gezinme ve `Enter` ile etkileşim ekle.
  - [ ] **Ana menüye** `A/D` ve `Sol/Sağ` ok tuşları ile yatay buton geçişi ekle.
  - [ ] `"Tüpü yerleştir"` ibaresini `"Yangın tüpünü yerleştir / Yangın söndürücüyü hazırla"` olarak düzelt.
