# Too Late to Panic! — Oyun Tasarım Dokümanı

**Güncel tasarım:** [GDD v2.7 / Home V5.3](../TooLateToPanic/Documentation/GDD.md) ve [V5 teslimi](../TooLateToPanic/Documentation/HOME-V5-DELIVERY-2026-10-18.md).120sn izometrik önlem, ardından45sn otomatik FPS çanta;1su+1gıda ve75% kayıp sınırı. Selde üst kat panik odası, diğer afetlerde bodrum kullanılır; yetişememe sahte giriş üretmez. Her dalgada gerçek sonlu loot hafif RNG ile yenilenir. Aynı aktif Model/Expedition/controller ve bağımsız eski FPS kaynak/kayıt koruması devam eder. Aşağıdaki Home V2 metni tarihsel tabandır; güncel kurallar bağlantıdaki proje belgesindedir.

**Sürüm 1.3 · 16 Ekim 2026 · Home V2**

Tema **Afete Hazırlık ve Önlem**. Tek oyunculu, tamamen çevrimdışı Windows oyunu. Bu sürüm, kullanıcının yeni yönlendirmesiyle önceki tek oda kapsamını değiştirir. İki yaşanabilir katı, bahçesi ve bunkeri olan müstakil ev; 3D assetler; karakteri izleyen, yönü sabit izometrik ortografik kamera kullanılır.

Oyun adı kesin olarak **Too Late to Panic!** olarak belirlenmiştir. Home V3 revizyonu `Evidence/GamejamOrchestra/TLTP-HOME-V3-20261016/CONTRACT.md`, bağımsız FPS revizyonu kendi proje tasarımı ve sözleşmesindedir. Aşağıdaki v1.3 metni Home V2 temel kurallarını korur.

Bu belge tasarımı tanımlar. Derleme, native smoke ve görsel kabul ayrı kanıtlardır; güncel teslim kaydı bunları ayrıca belirtir. v1.2 kaynak yedeği `Evidence/GamejamOrchestra/TLTP-HOME-V2-20261016/before/GDD-v1.2.md` içinde korunmuştur.

## Temel döngü

1. Yaklaşan afet ve gerekli bunker erzağı öğrenilir.
2. Evde ve bahçede dolaşılır; dolaplar açılır, malzemeler toplanır, envanter düzenlenir.
3. Mevcut duvar, kapı, çatı ve pencereler güçlendirilir. Mobilyalar taşınır, döndürülür ve duvara veya zemine sabitlenir.
4. Bahçede drenaj, sel çiti ve fidan; evde yangın söndürücü ve bahçede hortum hazırlanabilir.
5. Son 60 saniyede çanta uyarısı gelir. Çanta hazırlama arayüzü zorunlu değildir; yeterli erzakla bunkera yetişmek zorunludur.
6. Afet sırası, afet sonrası, başarı veya başarısızlık sunumu oynar.
7. Başarıda bütçe ve puan verilir. Oyuncu en fazla 20 saniyelik hasar gezisinde evi inceler veya erken devam eder.
8. Yeni dalgada hasar sıfırlanır; kalıcı önlemler ve eşya yerleşimi korunur. Su ve gıda tüketilir.

## Dünya, görünüş ve karakter

Ev 16×12 metre oturuma, 3.2 metre kat aralığına ve fiziksel merdivene sahiptir. Bahçe yaklaşık 48×48 metredir; yol, komşu evler ve bitkiler dış dünyayı görünür tutar. İçeri girince çatı ve kamerayı engelleyen ön duvarlar kesit görünümüne geçer; alt kattayken üst kat gizlenir. Merdiven bir kat değiştirme ışınlanması değildir.

Camlar şeffaftır ve gerçek duvar boşluklarına oturur. Dışarıyı camlardan görmek mümkündür. Görünüş hedefi: ceviz ahşap, kömür/indigo, keten/taupe, zeytin yeşili, sınırlı pas ve amber. Aşırı parlak aydınlatma yerine loş bulutlu akşam, soğuk dış ışık ve sıcak yerel ev ışıkları kullanılır.

Tek oyuncu için mevcut CC0 humanoid ve animasyon kaynağı korunur; özenli iş ceketi, pantolon, bot, eldiven ve saç geometrisi eklenir. Durma/yürüme/koşma karışımı ile çalışma ve diz çökme pozları kullanılır. Yeni sakinler, çocuklar ve nüfus sistemi bu sürümde yoktur.

## Kontroller ve arayüz

| Tuş | İşlev |
| --- | --- |
| WASD / oklar | Kameraya göre hareket |
| Shift | Koşu |
| E | Yakındaki hedefi aç, topla veya etkileş |
| C | Kapak veya kapıyı kapat |
| Q | Yakındaki hedefler arasında geçiş |
| T | Duvar / zemin ankrajı seçimi |
| 1 | Sabitle veya sabitlemeyi kaldır |
| 2 | Pencere/kapıda sel koruması, diğer hedefte yangın önlemi |
| 3 | Yapı güçlendirme |
| 4 | Eşya taşıma |
| R / Shift+R | Taşırken 45° döndürme |
| Tab / B | Envanter ve afet çantası |
| F | Envanterde ilk yardım üretimi |
| PageUp / PageDown | Kamera yakınlığı |
| Escape | Duraklat; etkin işte iptal |

Kalıcı büyük yan menüler yerine ince üst durum şeridi ve alt bağlam ipuçları bulunur. Fare seçimi ve düğmeler ikincil kolaylıktır. Yerleştirmede ayak izi, uygun/uygunsuz renk ve somut hata metni kullanılır. Merdiven, çıkış, duvar ve diğer eşyalarla çakışma engellenir.

## Toplama, envanter ve çanta

Envanter **12 yuva**, afet çantası **8 yuva**. Sarf malzemeleri beşli yığınlanır; aletler tekli taşınır. Buzdolabı, mutfak dolabı, kitaplık, ilk yardım dolabı, üst kat depoları ve bahçe kasalarında farklı malzemeler bulunur. Kapak önce açılır; ardından her etkileşim bir malzeme yığını alır. Aynı yığın kopyalanamaz. Taşınan dolabın toplama noktası da taşınır.

İlk yardım seti bir dükkândan alınmaz: **2 bandaj + 1 antiseptik** ile üretilir. Fener ve radyo için pil gerekir. Battaniye diğer destek ekipmanıdır. Su ve gıda zorunlu hayatta kalma kaynaklarıdır; destek ekipmanı ek ödül sağlar. Yetersiz çantayla bunkera girmek engellenmez, ancak sonuçta koşu kaybedilir.

| Dalga | Gerekli su ve gıda |
| --- | --- |
| 1–3 | Her birinden 1 günlük paket |
| 4–6 | Her birinden 2 günlük paket |
| 7+ | Her birinden 3 günlük paket |

Bu, gerçek afet hazırlığı için sayısal tavsiye değil, oyun dengesi birimidir. Sarf kaynakları yeni dalgada yenilenir; benzersiz ekipmanların sonsuz kopyası üretilmez. Çantadaki su/gıda dalgada tüketilir; destek ekipmanları ve güçlendirmeler koşu boyunca kalıcıdır.

## Aktif işler ve eşya mekanikleri

Sabitleme, güçlendirme, sel koruması, yangın ekipmanı ve bahçe işleri yakın plan gerçek 3D düzenekle oynanır. **A/D ile hizala → E ile kilitle → Space basılı tutarak uygula**. Dört bağlantı/adım tamamlanır. Alet dönüşü, vida ilerlemesi ve işe göre hareket görünür; karakter de uygun ayakta/diz çöken iş pozuna girer. İş sırasında hazırlık saati devam eder. İptalde malzeme ve para tüketilmez; başarılı tamamlamada atomik olarak tüketilir.

Duvar ankrajı mobilyanın arkasına; zemin ankrajı ayağına uygulanır. Duvar ankrajı için mobilya duvara yakın olmalıdır. Ankrajlı eşya taşınmadan önce sökülür. Bir kez alınmış ankraj ekipmanı yeniden kullanılır; yeniden sabitleme yine aktif iş gerektirir. Yapılar T0→T1→T2 ilerler; yeni duvar eklenmez. Her kademenin ve korumanın görünür donanımı bulunur.

## Afetler ve hasar

| Dalga | Afet |
| --- | --- |
| 1–3 | Deprem |
| 4–6 | Sel |
| 7–9 | Yangın |
| 10+ | Üç afet arasında kontrollü RNG; aynı afet en fazla iki kez üst üste |

Depremde ankraj ve yapı kademesi; selde pencere/kapı bariyeri, drenaj ve çit; yangında yangın ekipmanı ilgili riski düşürür. Fidanın sel katkısı sınırlıdır; drenajın yerine geçmez. Ev büyüdüğünde yalnız eşya sayısı arttı diye koşunun otomatik kaybedilmemesi için toplam risk normalize edilir.

**Toplam hasar %65'i aşarsa**, hazırlık süresinde bunkera girilmezse veya çanta yetersizse kayıp olur. Hasar bir sonraki dalgaya birikmez. Sonuçta toplam hasar, hangi parçaların katkı verdiği, eşya üzerindeki izler ve hasar belirteçleri görünür. Hasar gezisi hazırlık süresinden düşmez.

Afet sunumları gerçek fizik simülasyonu sertifikası taşımaz: deprem toz/çatlak/enkaz, sel hareketli su/yağmur/köpük/sürüklenen parçalar, yangın alev/duman/kıvılcım katmanlarıyla okunur. Oyun kaybı ve koruma hesabı görsel efektlerden bağımsız kurallarla hesaplanır.

## İlk denge değerleri

Başlangıç bütçesi **1000**. Hazırlık süreleri **300 / 270 / 240 saniye** (1–3 / 4–6 / 7+). Bunlar evin büyüklüğüne göre ilk oynanışta değiştirilebilir.

| İş | Bütçe + envanter malzemesi |
| --- | --- |
| İlk ankraj | 60 + 1 braket + 2 vida |
| T0→T1 | 120 + 1 tahta + 2 vida |
| T1→T2 | 180 + 2 tahta + 3 vida |
| Sel bariyeri | 80 + 2 kum torbası + 1 tahta |
| Yangın söndürücü | 75 + 1 söndürücü |
| Fidan | 30 + 1 fidan |
| Drenaj | 50 + 2 kum torbası + 1 tahta |
| Sel çiti | 60 + 3 tahta + 2 vida + 1 kum torbası |
| Hortum | 80 + 1 hortum |

Ödül dalga ilerledikçe artar; az hasar ve iyi çanta daha iyi ödül verir. Highscore sıralaması önce tamamlanan dalga, ardından toplam puandır. Skor yalnız çalıştırılan makinede tutulur. Öğretici skor tablosuna girmez ve gerçek koşu başlangıcında kaynakları sıfırlanır.

Afet sırası 8 sn, afet sonrası 4 sn, başarı 3 sn, başarısızlık sunumu 4 sn; hasar gezisi en fazla 20 sn. İlk yardım/gathering etkileşimi sürekli bekleme düğmesine dönüşmez. Duraklatmada hazırlık saati durur.

## Sonraki kapsam ve doğrulama

Yeni insanların gelişi, buna bağlı oda açılışları, oda başına üç kişi, dört yetişkin ve iki çocuk görünüşü sonraki aşamadadır. İki katın bu sürümde açık olması nüfus sistemini başlatmaz. Online skor, çok oyunculu ve çevrimiçi zorunluluk yoktur.

48 saatlik jam için test yaklaşımı: birleşik import/build, etkilenen akışlarda kısa native smoke ve görünür kare kontrolü. Uzun kampanya dengesi ile nihai sanat kabulü insan oynanışıyla ayrıca değerlendirilir. Dar derlemenin geçmesi bütün hedeflerin görsel olarak kabul edildiği anlamına gelmez.




Home V4.8 güncel akış kabulü: TLTP-FULLFLOW-QA-20261017; 149/149 gerçek zamanlı kesintisiz rota + 189/189 regresyon,0runtime hata. Kısa akış49.67sn, dört doğal simülasyon; test Warp/Tick/hile0. Üretim API girdisi, OS/insan keyif kabulü değildir. LATEST ve HOME-V4.8-DELIVERY-2026-10-17.md yeniDLL/manifesti bağlar; eskiV4.7 açıkoyun korunur.


## Home V4.9 güncel iş sözleşmesi —17Ekim

Son kullanıcı kararı eski A/D–Enter–Space Ratchet/Timing/Balance zorunluluğunu geçersiz kılar. Sabitlemede iki gerçek vida fareyle seçilir, her biri0,60sn otomatik takılır. Diğer uygun hedefler0,60sn dünya montajıdır. Duvar/pencere gerçek güçlendirme parçası ve karakter build hareketi; yangın tüpü yalnız fire_station;3bölge×4olgun ağaç; hareket×1,35. Yalnız geçerli/tamamlanmamış önlem seçeneği gösterilir. Malzeme/bütçe tamamlamada bir kez, iptal ücretsiz. Son karakter fork mesajıyla görünüm güzelleştirmesi iptal edildi; eskiV2 görünümü korunur, gerekli montaj hareketi kalır. FPS askıda korunur. YeniDLL kabulü LATEST ve HOME-V4.9-DELIVERY-2026-10-17.md ile bağlıdır: 44/44 direkt iş +141/141 kesintisiz rota,0runtime hata. OS/insan/tüm kampanya kabulü değildir. Eski sayılar yeniDLLkanıtı sayılmaz.


## Home V5.3 —18Ekim2026

Son kullanıcı düzeltmesiyle hikâye KKTC · Güzelyurt · Kalkanlı olarak uyarlandı. Yer gerçektir; kişiler ve afet olayları kurmacadır. Aile güvenli alanda, oyuncu aile evini hazırlar. Açılış16sn/dört4snçekim,120+45sn, dalgalarla artan V5.2 hasarı ve gerçek maliyet/fizik/skor kuralları korunur. Menü/sinematik/defter/kısa akış ortak katalog kullanır; selde hazır çanta defter notu üst panik odasını gösterir. Yeni konuşma/sağlayıcı işi yoktur. Güncel kimlik LATEST ve HOME-V5.3-KKTC-DELIVERY-2026-10-18.md'dir. Dar native hikâye/API/render kontrolü OS/insan/tüm kampanya kabulü değildir.
