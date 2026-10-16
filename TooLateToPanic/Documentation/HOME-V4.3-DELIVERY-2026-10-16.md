# Too Late to Panic! — Home V4.3 etkileşim teslimi

16 Ekim 2026 · Unity6000.0.73f1 Built-In · Windows0.4.3. Bu paket kullanıcının kapı sıkışması, fareyle eşya taşıma, ikonlu malzeme açıkları ve banyo girişini diğer duvara taşıma isteklerini birlikte ele alır.

Geçiş kapıları kapanma sırasında karakteri sorgular; sonraki dönüşü uygulamadan önce en fazla 1.25 derece ara pozlar denetlenir. Karakterin bulunduğu bölüme girmeden durur, kapanma isteği beklemede kalır. Karakter çekilince ikinci E olmadan devam eder. Dikey karakter kapsülü ile gelecekteki kapı kutusu arasındaki uzaklık, gerçek controller.center ve boyutlarla doğrudan hesaplanır; .035 m güvenlik payı bırakılır. Fizik kapatılmaz, karakter itilmez/ışınlanmaz. V4.2'nin oyuncudan uzağa açılma yönü korunur.

Eşyaya sağ tık taşıma başlatır (4 yedektir). Fare, eşyanın alındığı kat düzleminde yeşil/kırmızı önizlemeyi yönetir. Sol tık bırakır; tekerlek45derece/R döndürür; sağ tık/Esc/Backspace iptal eder. WASD karakteri yürütür. HUD tıklaması, aynı kare başlat/bırak, karakter/merdiven/kapı/duvar/eşya alanı veya sağlam zeminsiz nokta kabul edilmez. Özgün görünüm/çarpışma iptalde geri gelir. Onay model ve gerçek loot kaynağını bir kez aynı konuma taşır; stok klonlanmaz. Doğrudan eski-yeni konum arasında duvardan geçecek yerleşimler muhafazakâr biçimde reddedilir.

Malzeme başarısızlığında mevcut96px ikonlar80px satırlarda gösterilir. Her satırda ad, Eksik adet ve Var/Gerek sayımı vardır; eksik bütçe ayrıdır. Kart bütün gerçek eksikleri gösterir ve evden toplamayla güncellenir. İşe başlama, tamamlanma, ilk yardım ve pil tarifleri aynı canonical MaterialCosts üzerinden hesaplanır. Mevcut maliyetler/tüketimler değişmez; başarısızlık ve iptal tüketmez.

Banyo tek girişli odadır. Kapı merdiven yanındaki batı duvarından ön/güney duvara taşındı: (3.25,3.2,1.8),yaw0, açıklıkx2.65..3.85. Batı duvarıx2 boyunca kapalıdır. Ecza dolabı önceki duvar konumunda, mevcut lavabo/klozet/küvet ve karo zemini korunur. Kalıcı merdiven korkulukları, karakter fiziği ve afet/çanta/bütçe/hasar kuralları aynı kalır. Yalnız afet sesi; yeni insan/dış sağlayıcı/harcama yok. Windows ürün/kayıt kimliği V4 olarak korunur; FPS projesine dokunulmaz.

Bağımsız QA ölçümü [Unity6 Physics.ComputePenetration](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics.ComputePenetration.html) ile gerçek yaprağı, karakter boyutlarını birebir taşıyan standart etkin CapsuleCollider ile sanal karakter pozunda sorgular. Sorgu colliderı trigger/IgnoreRaycast olarak (500,500,500) konumunda durur; gerçek karaktere ve kapıya müdahale etmez. Üretim uzaklık hesabı bu API sorgusuna bağımlı değildir.

## Kanıt ve sınır

Evidence/GamejamOrchestra/TLTP-INTERACTION-V43-20261016 kaynak snapshotı, üç staging paketi, manifest, derleme ve native kayıtlarını içerir. İlk build tam art bake kullanır. --interaction-smoke yalnız taze GUID --home-evidence ile izole skor profilinde çalışır. Kapı bekleme/çekilince devam, ekran ışınıyla mouse API yerleşimi, tam malzeme açığı sayımı ve yeni banyo girişi kısa etkilenmiş kapsamdır. Kurulum warpları raporda sayılır; bloklanan kapıdan çekilme gerçek CharacterController hareketidir. Fiziksel OS fare/klavye, manuel oynanış, uzun kampanya veya FPS kabulü değildir. Nihai sonuç aşağıdaki kabul bölümünde source-build-v3/native-v3 kimliğiyle verilmiştir.

PanicDoorSafety, PanicHomePlacement yeni partial kaynakları; World giriş/gate/hook, Life banyo modeli, ResidenceArt oda geometrisi, Expedition tek maliyet/payload ve HomeHud kart/tuşlar. Üç alt ajan ayrı staging yazdı; root canlı kaynak, Unity, player ve teslim sahibidir. ASSET-CREDITS/Existing-Player-Provenance lisansları korunur.


İlk native-v1 kabul edilmedi (6/11): devre dışı proxy kapsülünün sorgusu native oyuncuda çalışmadı. Fare başlangıcında görünmez kesit duvarı seçimi engelliyordu; yalnız karşılık gelen görseli gizli mimari colliderlar fare rayinde atlanır, oyuncu fiziğinde etkin kalır. Native-v2 9/11: fare/malzeme/banyo geçti, fakat gerçek CharacterController'a verilen kök pozla sorgu kapanma temasını güvenilir ölçmedi; kapı karakteri itmeye devam etti. Üretim güvenliği açık merkezli analitik kapsül-kutu uzaklığına, bağımsız QA ise standart etkin kapsül sorgusuna geçirildi. Kriterler gevşetilmedi; nihai tekrar ayrı source-build-v3/native-v3 kimliğine bağlanır.

## Nihai kabul — source-build-v3 / native-v3

Build UTC 2026-10-16T19:51:58.5998738Z; Succeeded, 0 hata/0 uyarı, 486.897.785 build baytı. Sahne Assets/Game/Scenes/TooLateToPanicResidence.unity, ürün sürümü 0.4.3. İlk source-build-v1 tam art bake; sonraki iki onarım yalnız runtime olduğu için aynı 104 prefab yeniden kullanıldı. Son kaynak manifesti 46 dosya; kaynak ve prefab farkı 0. final-source küçük kaynak snapshotı saklanır.

- EXE SHA256: 0E1B64A3C0EBA3482F4C7B57F36CF8811997C0CE3BBED914C116CB9D4799FC7C
- Assembly-CSharp.dll SHA256: EE9B8A269FF8D73718D358F3BBE2B39E17086EBBA6115FE10D45A75CBE6710E1
- Yeni GUID profil: native-v3-e0365b78-a92e-4f59-8252-3b646eadae2e. Rapor: aynı Evidence partisindeki bu klasörün native-interaction-smoke.json dosyası.
- **11/11 PASS, 0 runtime hata, 19,8825 saniye.** İki temsilî kapı, iki yönden: banyo ve dış kapı; 2 kapı + 3 bağımsız fixture başlangıç yerleştirmesi. Beklerken en fazla karakter sapması 0,0000004768 m; yaprak açı aralığı 0 derece ve bağımsız örtüşme 0 m. Her dört probda bekleme ve ikinci E olmadan fiziksel yürüyüşle çekilince kapanma geçti. Giriş yönlü açılma da geçti. Diğer iki geçiş kapısı aynı üretim kodunu kullanır; bu yeni koşuda ayrıca denenmedi.
- Mouse ekran ışınıyla gerçek depo seçimi, aynı kare/HUD/karakter/duvar/merdiven boşluğu reddi, iptalde dönüş/görünüm/stok/kaynak konumu/maliyet korunması, geçerli 45 derece yerleşimin bir kez model ve loot kaynağına işlenmesi geçti.
- Gerçek E başarısız sel çiti işlemi kartta tahta 0/3 eksik3, vida 0/2 eksik2, kum torbası 0/1 eksik1 gösterdi. Gerçek E/F kiler toplamasından sonra tahta 4/3 eksik0, kalan gerçek eksik satır2 oldu; iş/kaynak tüketimi başlamadı.
- Yeni güney banyo girişi, eski batı açıklığındaki fiziksel duvar, obsolete üst kapının yokluğu ve 64 kalıcı korkuluk/controller değişmezleri geçti. Tam merdiven rotası bu pakette tekrar koşulmadı.
- Üç gerçek native PNG açılıp incelendi: bathroom-close-paused-actual-render.png, mouse-placement-preview-actual-render.png, missing-material-icons-actual-hud.png. Bunlar render kanıtıdır; manuel etkileşim/kampanya kabulü değildir.
- Normal test argümansız oyuncu PID10688 açıldı; HOME_V43_READY görüldü, ilk kontrolde runtime exception0. Açık eski kullanıcı oyuncusu PID37808 korunur. PID'ler tarihsel gözlemdir; gelecekte yeniden doğrulanır.

Başlatıcılar Play-TooLateToPanic.cmd ve proje aynası artık V4.3'ü açar. LATEST-BUILD.json, iki README, GDDv1.8, ortak GDD güncel sürüm bağlantısı, iki AGENTS ve asset kayıtları güncellendi. Tam kampanya/elle oynanış/OS fare-klavye/işitsel/FPS kabulü yapılmadı. Gerçek save uyumluluğu yeni test kapsamı değildir; ürün/kayıt kimliği ve gerçek kayıtlar değiştirilmedi.

Tek portable ZIP: TooLateToPanic-Home-V4.3-20261016-portable.zip; 111.733.698 bayt; SHA256 5A52A20FB3D2A03C1C97BA8A4558C8E2AB2E7DF666CDCA9DA3A29277B19F4194. ZIP içi Assembly-CSharp.dll SHA256 native test edilen DLL ile eşleştirildi. Quaternius yerel lisansları, mevcut asset/provenans açıklamaları ve kısa tuş dosyası eklendi.

cleanup-receipt.json: Kullanılmayan Home-V4-20261016 ve eski V4.2 ZIP Geri Dönüşüm Kutusu'na alındı. Açık PID37808 kullandığı Home-V4.2-20261016 klasörü geçici istisna olarak korundu. Güncel tek ZIP V4.3'tür. Kaynak/lisans/gerçek skorlar ve bağımsız FPS projesine dokunulmadı; Geri Dönüşüm Kutusu boşaltılmadan alan kazanıldığı iddia edilmez.

