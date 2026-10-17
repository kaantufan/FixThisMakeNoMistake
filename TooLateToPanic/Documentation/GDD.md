# Too Late to Panic! — Oyun Tasarım Dokümanı

**Sürüm 2.4 · 18 Ekim 2026 · Home V5**

V5 öncelikli güncel kurallar:120sn izometrik önlem+45sn FPS çanta; selde üst kat panik odası, diğer afetlerde bodrum. Süreye yetişmemek gerçek konumda ayrı kayıptır; sığınmış gibi gösterilmez.14malzeme kaynağı/16açık erzak noktası her dalgada bir kez sonlu hafif RNG ile yenilenir. Tekrar dolap açmak loot üretmez. Ana elektrik şalteri ev devresini/ekranı/elektrikli garajı keser; açık tesisat afete göre tek küçük RNG ikincil hasara yol açabilir.1su+1gıda,75% kayıp, atomik maliyet ve ücretsiz iptal korunur. F1 küçük köşe tuş rehberi, J canlı verili spiral defterdir. V5 karakter/araç/meslek/raf/dolap sanatı ve pencere/deprem/oda kararları [teslimde](HOME-V5-DELIVERY-2026-10-18.md), kesin build/test kimliği [LATEST-BUILD.json](../LATEST-BUILD.json) içindedir. Aşağıdaki eski30sn ve yalnız-bodrum anlatımları tarihsel sürümlere aittir; bu paragraf güncel önceliktir.

Tema **Afete Hazırlık ve Önlem**. Tek oyunculu, tamamen çevrimdışı Windows oyunu. İki yaşanabilir katı, bodrumu, balkonu, banyosu, bitişik garajı, bahçesi ve bunkeri olan müstakil evde geçer. Dolaşımda karakteri izleyen, yönü sabit izometrik ortografik kamera; afet ve sonuç sahnelerinde gerçek sinematik kamera kullanılır. Teknik hedef Unity 6000.0.73f1, Built-In ve Windows'tur.

Bu belge Home V4.12 tasarımını ve kaynakta bulunan akışları tanımlar. Derleme, kısa native kontrol, elle oynanış/işitme ve son görsel kabul ayrı kanıtlardır. Güncel kaynak/build/native kimliği LATEST-BUILD.json ve [Home V4.12 teslim kaydında](HOME-V4.12-DELIVERY-2026-10-17.md) bulunur. Önceki sürümlerin sonuçları tarihsel kayıttır ve yeni DLL kabulü yerine kullanılamaz. [Home V2 GDD](GDD-HOME-V2.md) tarihsel olarak korunur. [Home V4.3 sözleşmesi](../../Evidence/GamejamOrchestra/TLTP-INTERACTION-V43-20261016/CONTRACT.md) son etkileşim ve banyo kararını; [Home V4 sözleşmesi](../../Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/CONTRACT.md) temel sanat ve iş kapsamını belirler. Bağımsız FPS projesinin kendi sözleşmesi o sürümün farklarını tanımlar.

## Home V4.12 güncel kararlar

Hazırlık izometrik önlemlerle başlar. Son30saniyede aynı oyuncu/controller konumundan1.1sn kamera geçişiyle FPS çanta aşaması otomatik açılır. Öncesinde çanta erzakları gizlidir; son aşamada yapı malzemesi toplanmaz, dolaplar kapalı kalır ve önlem işleri başlatılmaz. Yalnız açıkta duran erzaklar ve geçiş kapıları kullanılır. G ile önceki aşamaya dönülmez. Çanta için her dalgada1su+1gıda yeterli; alınan temel birim gerçek çantaya otomatik yerleşir, artanlar envantere gider. İsteğe bağlı destek ekipmanı zorunlu değildir.

Toplam hasar yüzde75ve üzerindeyse kayıp. Çit inşası kaldırıldı; üç bölgede dörder olgun ağaçtan tam sıralar kurulur. Yakın duvar/pencere seçimi gerçek yüzey mesafesine bağlıdır, seçili hedef komşu nesne yüzünden kendiliğinden değişmez. Seçili nesne gerçek3D sınırlarıyla vurgulanır. İki etkileşimsiz ayaklı lamba kaldırıldı. Sığınak kapağı açılmadan oyuncu şafttan inmez; dış sinematik çekimlerde çatı ve gökyüzü görünür, iç planlarda kesit korunur. FPS HUD süre,1/1temel ihtiyaçlar, gerçek en yakın hedef ve etkileşim bilgisini ayrı kartlarda gösterir.

Kısa akış ilk olarak önlem/sabitlemeyi, hazırlık süresinin son30saniyesinde çantayı ister. Tek zorunlu sabitleme korunur; diğer geçerli önlemler bu süre içinde isteğe bağlıdır. Yapı malzemeleri dolaplarda sonlu, tek seferliktir. Yanlış kapsam işlemi önceki hedefin eksik kartını bırakmaz. Eski “çanta hazır, şimdi sabitle” konuşması yeni sıraya uymadığı için kısa onay efektiyle değiştirilir; yeni sağlayıcı işi yapılmaz.

## Home V4.7 tarihsel tabanı

Başlıkta HAZIRLIĞA BAŞLA sonlu stoklu270sn kısa hazırlık akışını, SERBEST OYUN normal kampanyayı açar. Kısa akışta hareket/iki vidalı sabitleme/son30saniyede su toplama, depremde çök–kapan–tutun alanı ve gerçek sonuç, ayrı sel drenajı/yüksek hedef ve yangın hazırlığı/dış toplanma alanı vardır. Başlangıç gıdası açıkça baseline'dır. Başarı uydurulmaz; tamam/eksik/atlandı ayrılır, skor yazılmaz. İlk180sn keyif ve toplam300sn üst sınır tasarım hedefidir; insan kabulü ayrı yapılır. Zincirleme afetler ertelenmiştir. Esc duraklatması ve defter her iki akışın saatini durdurur.

SAHNEYİ ATLA/Enter gerçek model çözümünü bir kez bitirip kamera/karakteri iade eder. Bunker sinematiğinde HUD'nin saatleri birlikte kilitlemesi kaldırılmıştır. F2 Kaybetmeme gerçek afet/hasar/erzak tüketimini korur, kayıp yerine incelemeye geçer; bu dalga başarı/skor/ödül sayılmaz ve koşu sticky skor dışıdır.

Yarkın seçili modelleri ve12prop konstrüksiyon/malzeme/biçim geçişi, hafif seçili pickup hareketi, .30sn görsel toplama yayı ve gerçek sabitlemeye bağlı donanım eklenmiştir. Yalnız kısa akış için7ElevenLabs cümle+3kısa efekt son kullanıcı isteğiyle yetkilendirilmiştir. Konuşma15.73sn toplam, altyazılı ve tek seslidir; eski Tolga/serbest oyun konuşması ertelemesi sürer. Aşağıdaki HomeV4 yalnız-afet paragrafları tarihsel tabanı açıklar. Kaynak/build/native ve işitsel/insan kabul sınırı güncel teslimdedir.

## Temel döngü

1. Yaklaşan afet ve gerekli bunker erzağı öğrenilir.
2. Evde ve bahçede dolaşılır; dolaplar açılır, malzemeler toplanır, envanter düzenlenir.
3. Mevcut duvar, kapı, çatı ve pencereler güçlendirilir. Mobilyalar taşınır, döndürülür ve duvara veya zemine sabitlenir.
4. Bahçede drenaj ve ağaç sıraları; evde yangın söndürücü ve bahçede hortum hazırlanabilir.
5. Son60saniye uyarısından sonra, son30saniyede FPS çanta aşaması otomatik başlar.1su+1gıdayla bodrumdaki sığınağa girilir.
6. Afet sırası, afet sonrası, başarı veya başarısızlık sunumu oynar.
7. Başarıda bütçe ve puan verilir. Oyuncu en fazla 20 saniyelik hasar gezisinde evi inceler veya erken devam eder.
8. Yeni dalgada hasar sıfırlanır; kalıcı önlemler ve eşya yerleşimi korunur. Su ve gıda tüketilir.

## Dünya, görünüş ve karakter

Ev 16×12 metre oturuma ve 3.2 metre kat aralığına sahiptir. Ground y=0, üst kat y=3.2, bodrum y=-3.2'dir. Bodrum–ground ve ground–üst kat arasında fiziksel basamaklar bulunur; merdiven kat değiştirme ışınlanması değildir. Home V4.2 merdiven uçuşlarının yan korkulukları ve kat boşluğu korkulukları fiziksel engeldir. Basamak, sahanlık ve iki kat arasındaki hareket açık kalır; görsel kat kesiti bu colliderları kapatmaz. Bahçe yaklaşık 48×48 metredir; yol, komşu evler ve bitkiler dış dünyayı görünür tutar. İçeri girince çatı ve kamerayı engelleyen ön duvarlar kesit görünümüne geçer; alt kattayken üst kat gizlenir. Görsel kesit fiziksel oda collider'larını kaldırmaz; bodrumun üstünü kapatacak zemin/toprak yüzeyi merdiven boşluğunu örtmez.

Üst güneyde kapıdan çıkılan balkon, üst kuzeydoğuda lavabo/klozet/küvetli banyo bulunur. Banyo tek girişli ayrı bir odadır; eski çift kapılı dar geçit kaldırılıp banyo zeminine katılmıştır. Home V4.3'te banyo kapısı (3.25,3.2,1.8), yaw0 konumunda güney duvarına taşınır; önceki batı açıklığı kapanır. Kapı, her açışta oyuncunun bulunduğu tarafın tersine, geçiş yönüne doğru açılır; açıkken yön değiştirmez. Ecza dolabı batı duvarına asılıdır. Alt kat bölmesi korunur. Bodrum teknik depo, çamaşır alanı ve işlevli kileri barındırır. Doğudaki bitişik garaja dış girişinden ulaşılır; hareketli garaj kapısı, araç, bisiklet, alet duvarı ve raflar vardır. Garaj aracı dekor ve fiziksel engeldir; araç sürüşü bu sürümün mekaniği değildir.

Camlar şeffaftır ve gerçek duvar boşluklarına oturur. Dışarıyı camlardan görmek mümkündür. Görünüş hedefi: ceviz ahşap, kömür/indigo, keten/taupe, zeytin yeşili, terracotta, sınırlı pas ve amber. Loş bulutlu akşam, soğuk dış ışık ve sıcak yerel ev ışıkları kullanılır. Kumaşlar, kitaplar, bitkiler, lambalar, bilgisayar, şömine ve servis eşyaları evi dolduran gerçek 3D parçalardır. Home V3/V4 oda/prop/pickup/ikon ve atmosfer kaynakları özgün prosedürel üretimdir; son görünüş native karelerde ayrıca değerlendirilir.

Home V4, oyun başlangıcı ve durum yenilemesinde mobilyanın özgün renkli materyallerini korur. Kitaplık renkli kitaplar ve arka panel; masa sıcak ahşap ve kumaş; TV sehpası açık raf ve ekran; çalışma masası klavye, fare, kasa ve ekran detayları taşır. Bilgisayar gerçek masa yüzeyine oturur. Banyoda lavabo, klozet, küvet ve duvara asılı malzeme dolabı giriş/dolaşım alanını açık bırakacak biçimde yerleşir; dar banyo paspası küvet yanındadır. Yatak ve ayrı yatak halısı yürünebilir açıklığı koruyacak biçimde düzenlenir. Alet kutusunun kapağı, iç tepsisi ve renkli aletleri görünür parçalardır.

Tek oyuncu için mevcut CC0 humanoid ve animasyon kaynağı, iş ceketi, pantolon, bot, eldiven ve saç düzenlemeleriyle korunur. Home V4 yeni insan modeli üretmez. Durma/yürüme/koşma karışımı ile çalışma ve diz çökme pozları kullanılır. Space ile zıplama gerçek CharacterController ve yerçekimi hareketine bağlıdır. Yeni sakinler, çocuklar ve nüfus sistemi bu sürümde yoktur.

## Kontroller ve arayüz

| Tuş | İşlev |
| --- | --- |
| WASD / oklar | Kameraya göre hareket |
| Shift | Koşu |
| Space | İş dışında zıpla |
| E | Aç/kapat; bunker, bahçe işi veya ana etkileşim; radyo konuşması ertelendi |
| FPS E / F | Baktığın açık erzağı al; E geçiş kapısı veya sığınak içindir |
| J | Gerçek verili hazırlık defterini aç/kapat |
| Q | Yakındaki hedefler arasında geçiş |
| T | Duvar / zemin ankrajı seçimi |
| 1 | Sabitle veya sabitlemeyi kaldır |
| 2 | Pencere/kapıda sel koruması, diğer hedefte yangın önlemi |
| 3 | Yapı güçlendirme |
| 4 | Eşya taşıma |
| R / Shift+R | Taşırken 45° döndürme |
| Taşırken Enter / Backspace | Geçerli konumu bırak / iptal |
| Tab / B | Envanter ve afet çantası |
| Envanterde oklar / Enter / Backspace | Seç / çantaya koy / geri |
| İşte sol tık | İki gerçek vidayı sırayla seç; her biri0.60sn otomatik takılır |
| PageUp / PageDown | Kamera yakınlığı |
| Escape | Duraklat/geri; etkin işte veya taşımada iptal |

HUD; kısa Türkçe etiketler, ince üst durum şeridi, ikonlu envanter/çanta ve birkaç bağlam tuşu gösterir. Uzun sürekli açıklamalar yerine kısa hata/başarı bildirimleri kullanılır. Seçili hazırlık hedefinde 1–4 iş kısayolları görünür. İşte 3D yakın görünüm, dört adım, adım adı ve işe göre yön/kuvvet/basınç göstergesi öne çıkar. Space etkin işte zıplatmaz; HUD ve world aynı tuşu iki kez işlemez. Fareyle gerçek içerik nesnesine tıklama seçim/toplama için ikincil kolaylıktır. Yerleştirmede ayak izi, uygun/uygunsuz renk ve somut hata metni kullanılır. Merdiven, çıkış, duvar ve diğer eşyalarla çakışma engellenir.

## Toplama, envanter ve çanta

Envanter12, çanta8yuvadır. Yapı malzemeleri bu yuvaları kullanmayan sayısal havuzdadır. İzometrik önlem aşamasında dolabı ilk kez açmak sonlu malzemeleri havuza ekler; tekrar açmak yeni stok üretmez. Son30saniyede dolaplar kapanır; yalnız açıkta duran su, gıda ve isteğe bağlı hazır ekipman toplanabilir. İlk yardım hazır kittir; bandaj/antiseptik üretimi yoktur. Fener/radyo pil kurallarını korur.

Her dalgada1su+1gıda zorunludur ve afet sonunda tüketilir. İhtiyaç duyulan ilk birim toplamadan sonra gerçek çantaya, artan yığın gerçek envantere gider. Kapasite yetmezse işlem stok kopyalamadan atomik reddedilir. Tab/B ve sürükle-bırak/Enter ile gerçek yuvalar düzenlenebilir. Destek ekipmanları isteğe bağlıdır; çantanın yeterlilik koşulu yalnız temel su ve gıdadır.

## Aktif işler ve eşya mekanikleri

Sabitlemede iki vida fareyle seçilir; her biri0.60sn otomatik takılır. Diğer geçerli önlemler0.60sn dünya montajıdır. Duvar/pencere/çatı gerçek güçlendirme donanımı taşır. Yangın tüpü yalnız yangın ekipmanı noktasında hazırlanır. Eski A/D–Enter–Space Ratchet/Timing/Balance zorunluluğu yoktur.

Maliyet/eksik bilgisi ve tamamlanma aynı MaterialCosts/WorkCost tarifini ve gerçek havuzu kullanır. İptal ücretsizdir; malzeme/bütçe yalnız tamamlamada bir kez tüketilir. Önce alınmış ankraj donanımı yeniden kullanılır. Son30saniyede başlayan çanta aşaması devam eden işi ücretsiz iptal eder ve yeni önlem işi başlatmaz. Fizik/controller/kapı/merdiven ve geçerli yerleşim kuralları korunur.

## Afetler ve hasar

| Dalga | Afet |
| --- | --- |
| 1–3 | Deprem |
| 4–6 | Sel |
| 7–9 | Yangın |
| 10+ | Üç afet arasında kontrollü RNG; aynı afet en fazla iki kez üst üste |

Depremde ankraj ve yapı kademesi; selde pencere/kapı bariyeri ve drenaj; yangında yangın ekipmanı ilgili riski düşürür. Fidanın sel katkısı sınırlıdır; drenajın yerine geçmez. Ev büyüdüğünde yalnız eşya sayısı arttı diye koşunun otomatik kaybedilmemesi için toplam risk normalize edilir.

**Toplam hasar %75 veya üzerindeyse**, hazırlık süresinde bunkera girilmezse veya çanta yetersizse kayıp olur. Hasar bir sonraki dalgaya birikmez. Sonuçta toplam hasar, hangi parçaların katkı verdiği, eşya üzerindeki izler ve hasar belirteçleri görünür. Hasar gezisi hazırlık süresinden düşmez.

Afet sunumları gerçek fizik simülasyonu sertifikası taşımaz: deprem toz/çatlak/enkaz, sel hareketli su/yağmur/köpük/sürüklenen parçalar, yangın alev/duman/kıvılcım katmanlarıyla okunur. Oyun kaybı ve koruma hesabı görsel efektlerden bağımsız kurallarla hesaplanır.

## Ses, kurtarma ve sinematik sunum

Home V4'te kullanıcının güncel kararıyla **yalnız afet sesleri** etkindir. Türkçe radyo anlatımı, radyo gürültüsü, ortam/arayüz/işlem sesleri ve rotor sesi ertelenmiştir. Evdeki radyo görsel nesne olarak kalır; etkileşimi “Radyo daha sonra” geri bildirimi verir. Afet türüne bağlı deprem, sel veya yangın sesi afet ve ilgili başarısızlık sunumunda oynatılır. Ses, hasar veya kayıp hesabını değiştirmez.

Afet sonrasında hasara uygun enkaz/çamur/yanık atmosferi, dönen ana ve kuyruk rotorlu kurtarma helikopteri ve arama ışığı sunulur. Şömine alevi ve sıcak ışığı ev atmosferinin görsel parçasıdır. Bu sunum öğeleri yeni koruma puanı veya hasar hesabı oluşturmaz.

Gerçek kamera dizisi bunker yaklaşımı/inişi, ev dışı afet planları, hasar gezisi ve ayrı bir 3D sığınak içindeki başarı sahnesini kullanır. Süre dolunca önce **SIĞINAĞA YETİŞEMEDİN** kayıp geri bildirimi yaklaşık **1.35 saniye** görünür; sonra yaklaşık **6 saniyelik** başarısızlık sinematiği başlar. Süre, eksik çanta ve ev hasarı farklı kayıp sunumlarıdır. Sinematikten sonra sonuç ekranı kararlı kalır. Kamera/sahne sunumu koşu sonucunu, bütçeyi veya hasar eşiğini değiştirmez.

Home V3'te yerel Microsoft Tolga OneCore/SAPI ile üretilen altı Türkçe WAV, `Assets/Game/Deferred/Radio-legacy/` altında tarihsel kaynak olarak korunur; Resources dışında ve Home V4 dağıtım kapsamı dışındadır. Ses motoru/verisi dağıtılmaz; sentez çıktısının ticari dağıtım şartları bağımsız doğrulanmamıştır. Konuşma oyunun son aşamasında ElevenLabs ile ele alınacaktır; bu paket sağlayıcı kullanımı, hesap/API işlemi veya kredi harcaması yapmaz. Ayrıntılı kaynak ve kullanım sınırları [asset kayıtlarında](ASSET-CREDITS.md) bulunur.

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

Model fazları afet sırası **8 sn**, afet sonrası **4 sn**, başarı **3 sn**, kayıp sonuç fazı **4 sn**; hasar gezisi en fazla **20 sn** olarak korunur. Home V4 kayıp görünümü ayrıca önce 1.35 sn geri bildirim, ardından 6 sn sinematik kullanır; modelin 4 sn sonuç sayacı bu sunumun süresi olarak yorumlanmaz. İlk yardım/toplama etkileşimi sürekli bekleme düğmesine dönüşmez. Duraklatmada hazırlık saati durur.

## Sonraki kapsam ve doğrulama

Yeni insanların gelişi ve nüfusa bağlı oda açılışları bu sürümün kapsamında değildir. İki katın ve ek mekânların açık olması nüfus sistemini başlatmaz. Online skor, çok oyunculu ve çevrimiçi zorunluluk yoktur. Kullanıcı 16 Ekim son kararıyla yalnız izometrikten devam eder. FPS geliştirmesi durdurulmuştur; mevcut kaynak/build/kayıtlar korunur ve yeni açık istek olmadan FPS işi başlatılmaz.

48 saatlik jam için test yaklaşımı: paket başına tek birleşik import/build, etkilenen akışlarda kısa native smoke ve görünür kare kontrolü. Home V4 kısa kontrolü oyun başlangıcından sonra renkli materyaller, banyo/yatak halısı/kitaplık/bilgisayar kareleri, vida/braket ikonları, kararlı E-F ve dolu/boş kaynak, iptalde maliyet, tek ankraj işlemi, üç iş ailesi, konuşmanın sessizliği/afet sesi ve kayıp-önce-sinematik sırasına odaklanır. Home V3'teki odalar/merdiven/zıplama kapsamının tamamı bu dar kontrolün kabul iddiasına eklenmez. API-driven koşu, elle klavye/fare oynanışı ve ses dinleme kabulünden ayrı kaydedilir. Uzun kampanya dengesi ile nihai sanat kabulü ayrıca değerlendirilir; kaynak veya dar derleme kanıtı bunların yerine geçmez. Gerçek build kimliği, hashler, test sonucu ve kalan eksikler root'un teslim kaydındadır.



## Home V4.3 etkileşim ek kararı

Kapanan kapı karakter yolunu fiziksel olarak sorgular; aradaysa durur ve çekilince devam eder. Eşyaya sağ tık taşıma başlatır; fareyle aynı katta yer seçilir, sol tıkla bırakılır, teker/R ile döndürülür, sağ tık/Esc ile iptal edilir. 4 ve Enter yedek kontrollerdir; WASD karakteri yürütür. Geçersiz/HUD/aynı kare yerleşimleri engellenir. Eksik malzeme kartı gerçek ikonlar ve Eksik/Var/Gerek sayılarıyla gösterilir; işe başlama, tamamlanma, üretim ve HUD aynı maliyet tarifini kullanır. Mevcut tüketim ve atomiklik korunur.

## Home V4.4 kapı ve aktif sürüm kararı

Açılan kapı da kapanan kapı gibi karaktere temas etmeden durur. Yarı kapanmış kapı E ile tekrar açılınca mevcut açılma yönünde geri çekilir; yeni oyuncudan uzağa yön yalnız kapı kapandıktan sonra seçilir. Ön girişin ayakkabılık, askılık ve radyo masası geçiş/dönüş alanından geriye alınır. Controller ve oyun kuralları değişmez. Aktif geliştirme yalnız izometriktir; FPS denemesi korunur, geliştirilmez. Yeni dar kabul güncel teslim ve LATEST-BUILD.json içindedir.


## Home V4.5 geliştirici seçenekleri

F2 veya görünür menü düğmesi geliştirici panelini açar. Sınırsız bütçe, iş/üretim malzemesi harcamama ve hazırlık süresini dondurma bağımsızdır; +1000 bütçe tek eylemdir. Menü açmak hile sayılmaz. Seçenek kullanılması koşuyu kalıcı hileli işaretler ve normal skor yazımını engeller; seçenek kapatmak bu koşuyu temizlemez. Yeni temiz koşu skor tutabilir. Seçimler süreç içinde korunur, diske ayar yazılmaz. Menü oyun girişini/model saatini kilitler, önceki duraklatma durumunu geri getirir. Kapasite/hedef/minigame/fizik kuralları ve afet saatleri korunur. GitHub güncel kaynak ve oynanabilir paket teslimi bu sürüm kapsamındadır.



## Home V4.6 HUD

HTML konsepti aktif oyuna uyarlanmıştır. Kömür/krem/petrol/amber/pas paleti, çevrimdışı Oswald/Barlow fontları, ayrı dalga/süre/kaynak kartları, canlı malzeme açıkları ve ortak çanta/iş/sonuç tasarımı kullanılır. 1600×900 mantıksal tuval ekran içine sığar; çanta ve işte gerçek sayaç görünür. Hazırlık son60alarmı, F2 ve sticky hileli koşu/skor davranışı korunur. 75/75 kısa fixture/API kontrolü, beş çözünürlük,50render ve dar fiziksel OS giriş kontrolü vardır; tam kampanya/mini oyun tamamlama kabulü değildir.


## V4.7 dış dünya ve seçili afet ekleri

Ayrı seçili tek-afet simülasyonu dört tip içerir;90sn hazırlık ve hazır başlangıç çantası açıkça belirtilir, skor/ödül yoktur. Sonuç kararlı Inspection veya gerçek kayıptır; yeni otomatik dalga açılmaz. Storm yeni3enumdur; eski0–2 ve kampanya sırası korunur. Roof/window/door riski ayrı, ankraj/tier etkisi vardır; mühendislik hesabı değildir. Dere yönlü animasyonlu mesh/foam, selde seviye değişimi; komşu hasarı yalnız sunumdur. Dört CC0 çevre NPCsi görev/ekonomi/kalabalık simülasyonu oluşturmaz. Sgaraj yolu bunker kapağını dolaşır; çit işinin konumu yolu kapatmaz.

Basılı sağ sürükleme ortografik yaw/pitch, kısa sağ bırakma taşıma; UI/iş/çanta/sinematik engelleri vardır. Üst katta alt meshler gizlenir, fiziksel colliderlar kalır. Helikopter içkamera planlarında gizlenir. Sinematikler aynı süre içinde afete özel kadraj ve sakin hareket kullanır. HUD dolar ve gerçek su/gıda3D görselleri kullanır.


Home V4.8 güncel akış kabulü: TLTP-FULLFLOW-QA-20261017; 149/149 gerçek zamanlı kesintisiz rota + 189/189 regresyon,0runtime hata. Kısa akış49.67sn, dört doğal simülasyon; test Warp/Tick/hile0. Üretim API girdisi, OS/insan keyif kabulü değildir. LATEST ve HOME-V4.8-DELIVERY-2026-10-17.md yeniDLL/manifesti bağlar; eskiV4.7 açıkoyun korunur.


## Home V4.9 güncel iş sözleşmesi —17Ekim

Son kullanıcı kararı eski A/D–Enter–Space Ratchet/Timing/Balance zorunluluğunu geçersiz kılar. Sabitlemede iki gerçek vida fareyle seçilir, her biri0,60sn otomatik takılır. Diğer uygun hedefler0,60sn dünya montajıdır. Duvar/pencere gerçek güçlendirme parçası ve karakter build hareketi; yangın tüpü yalnız fire_station;3bölge×4olgun ağaç; hareket×1,35. Yalnız geçerli/tamamlanmamış önlem seçeneği gösterilir. Malzeme/bütçe tamamlamada bir kez, iptal ücretsiz. Son karakter fork mesajıyla görünüm güzelleştirmesi iptal edildi; eskiV2 görünümü korunur, gerekli montaj hareketi kalır. FPS askıda korunur. YeniDLL kabulü LATEST ve HOME-V4.9-DELIVERY-2026-10-17.md ile bağlıdır: 44/44 direkt iş +141/141 kesintisiz rota,0runtime hata. OS/insan/tüm kampanya kabulü değildir. Eski sayılar yeniDLLkanıtı sayılmaz.
