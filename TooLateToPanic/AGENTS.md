# Too Late to Panic! — izometrik proje çalışma kuralları

## 16 Ekim Home V4.4 — aktif hedef ve kapı sıkışması

Kullanıcı yalnız izometrik sürümden devam etmeyi seçti. FPS geliştirmesi durduruldu; kaynaklar korunur, yeni açık istek olmadan FPS işi başlatılmaz. Son sözleşme ../Evidence/GamejamOrchestra/TLTP-DOOR-JAM-V44-20261016/CONTRACT.md. Home V4.3 kapanma kontrolü açılmayı korumadığı ve yarı kapanmış kapının E ile açılma yönünü değiştirdiği için yeni sıkışma bildirimi aldı. Yeni üretim kapı güvenliği BOTH açılma/kapanmada küçük ara pozlarla gerçek karakter kapsülünü denetler. Yarı açık kapı tekrar açılırken aynı kolu izler; oyuncudan uzağa yeni yön yalnız fiziksel olarak kapandıktan sonra seçilir. Güvenlik payı içinde mevcut temas varsa yalnız teması azaltan hareket kabul edilir; fizik kapatma, karakter itme veya kaçış Warp'ı yoktur. Ön girişte ayakkabılık (1.25,0,-4.4), radyo/masa (1.25,0/-veya.61,-3.3), askılık(-1.8,0,-4.5) geçiş/dönüş alanını açar. Controller ölçüleri ve oyun kuralları korunur. Yeni --door-jam-smoke taze GUID --home-evidence ile kısa ilgili kontroldür. Derleme sürümü0.4.4 Home-V4.4-20261016; runtime-only onarım aynı104 V4.3 prefab cache ile bake tekrarı gerektirmez. Nihai kabul LATEST-BUILD.json içinde kaydedilir; eski11/11 yeni sıkışma senaryosunun kanıtı değildir.

## 16 Ekim Home V4.3 — güncel etkileşimler

Son sözleşme ../Evidence/GamejamOrchestra/TLTP-INTERACTION-V43-20261016/CONTRACT.md. Geçiş kapısı kapanmadan önce kapsül ile ara açılarda denetlenir; arada karakter varsa closeBlocked ile bekler, yol açılınca kendiliğinden devam eder. Gerçek çarpışmayı kapatma veya karakteri itip/ışınlayıp test geçirme. Fareyle taşıma PanicHomePlacement partial dosyasındadır: sağ tık/4başla, sol tıkbırak, teker/Rdöndür, sağtık/Esciptal; HUD/sameframe/kat/zemin/duvar/kapı/aktör denetimleri korunur. MaterialCosts tek tariftir; CanBeginWork ve completion/craft/HUD aynı kesin sayımları kullanır. Eksik kartı ikonlu Var/Gerek/Eksik değerlerini gösterir. Banyo kapısı artık (3.25,3.2,1.8)yaw0 güney duvarındadır, batı duvarıx2 kesintisizdir. Önceki batı kapı koordinatları geçersizdir. --interaction-smoke yalnız yeni izole GUID --home-evidence ile yeni kontrol; eski bathroom/doors-stairs fixtureları yeni kapı konumu için kabul değildir. Builder0.4.3 Home-V4.3-20261016, art değiştiği için ilk tam bake gerekir.

## 16 Ekim Home V4.2 — korunan kapı yönü ve fiziksel korkuluk

Son ek sözleşme ../Evidence/GamejamOrchestra/TLTP-DOORS-STAIRS-V42-20261016/CONTRACT.md. Gerçek geçiş kapısı açılırken oyuncudan uzağa ±100 derece yön seçer; açıkken taraf değişince dönmez. Önceki V4.1 sabit içeri açma kararı bu kullanıcı isteğiyle güncellenmiştir. Dolap/çekmece/bunker kapağı yön seçmez. İki merdivenin basamaklarını izleyen yan colliderlar ve kat boşluğu korkulukları kalıcı fizik kökündedir; görsel kat kesiti bunları kapatmaz. Zemin güneyi yukarı merdiven girişi olduğu için açık, üst kat güney boşluk ucu korkulukludur. Karakter fiziğini/stepOffset'i değiştirme. Yeni kısa opt-in kontrol --doors-stairs-smoke ve taze izole --home-evidence ile çalışır. Eski banyo smoke sabit açılma ve korkuluksuz yaklaşım fikstürü içerir; yeni kapı/merdiven kabulü yerine kullanma. Builder 0.4.2 Home-V4.2-20261016; sanat değiştiği için ilk build tam bake gerektirir.

## 16 Ekim Home V4.1 — korunan tek girişli banyo

Son ek sözleşme: ../Evidence/GamejamOrchestra/TLTP-BATHROOM-V41-20261016/CONTRACT.md. Üst residence katında legacy east_south/east_north/door_jamb ve door_inner_upper yoktur. Banyo batı duvarı x2, güney z1.8, dış kabuk x8/z6; tek kapı (2,3.2,2.5) ve içeriye -100 derece açılan yapraktır. Ecza dolabı (2.24,3.2,4.15) ve görsel y+1.08/yaw270. Eski çift kapılı dar koridoru geri getirme. Ground bölmesi korunur. Builder 0.4.1 Home-V4.1-20261016; tam art bake gerekir. --bathroom-smoke yalnız izole --home-evidence ile kısa ilgili kontroldür; kaynak/build/native kanıtı yeni teslim kaydında tutulur.

## 16 Ekim Home V4 — korunan temel karar

Güncel sözleşme Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/CONTRACT.md. Home V4, ekran görüntülerindeki banyo/halı/gri mobilya/PC/alet kutusu kusurlarını düzeltir. Yeni kaynaklar hazırdır; son build/native kimliği LATEST-BUILD.json ve HOME-V4-DELIVERY-2026-10-16.md ile bağlanır. Eski Home V3 kanıtı yeni DLL kabulü değildir.

Şimdilik yalnız afet sesleri çalar. Genel/AI seslendirme, radyo konuşması, anlatıcı, UI/adım/rotor/oda sesleri devre dışıdır. Eski Tolga kayıtları Assets/Game/Deferred/Radio-legacy içindedir, Resources dışında tutulur ve yeni pakete dahil edilmez. Seslendirme en son ElevenLabs ile ele alınacak; henüz sağlayıcı işi/abonelik/kredi/harcama yetkisi yoktur. Önceki aşağıdaki radyo seslendirme hedefi bu kararla ertelenmiştir.

Üç aktif iş ailesi: Ratchet vidalamada A/D hizala + Enter, dönüşümlü A/D ve Space ile oturt; Timing fidan/drenaj/bariyerde Enter sonrası doğru anda Space; Balance hortum/yangında A/D basıncı dengele ve Space ile uygula. E yalnız açık/kapalı kapak ve dünya ana etkileşimidir; boş mobilyada gizlice sabitleme başlatmaz. 1/2/3/4 eylemleri görünür, maliyet/iptal atomik kalır. Güncel kısa test --refinement-smoke --home-evidence <izole-klasör>; eski --residence-smoke bu sessiz ses politikasını doğrulayamaz.

Yeni builder sürümü 0.4.0, çıktı Builds/Home-V4-20261016; malzeme dosya adları HomeV4_Screen gibi semantik adları korur. Yeni art değiştiğinde tam bake şarttır. Önceki derleme dosya adlarının malzeme adlarını değiştirdiğini hatırla; generic material-N isimleriyle başarılı renk/power kanıtı iddia etme. Güncel renklendirme authored çocuk mesh'lerini broad paletle yeniden boyamaz.


## Güncel kapsam ve sahiplik

Bu proje izometriktir. Güncel Home V3 sözleşmesi üst çalışma alanındaki `Evidence/GamejamOrchestra/TLTP-HOME-V3-20261016/CONTRACT.md` dosyasıdır. Kolay ve kısa Türkçe, nesne odaklı görsel toplama, dolu/renkli/detaylı ev, iki yaşanabilir kat, balkon, banyo, bodrum, bitişik garaj, zıplama, gerçek Türkçe radyo konuşması, afet sonrası kurtarma helikopteri, kompakt HUD ve gerçek kamera/sahne sinematikleri kapsamda kalır. Süre dolduğunda açık kayıp geri bildirimi başarısızlık sinematiğinden ÖNCE görünür. Tek oda ve yeni karakter üretmeme kısıtları güncel kapsam değildir.

İzometrik root yalnız bu projenin Unity/import/build/player sahibidir. Kardeş `TooLateToPanicFPS/` ayrı root'a aittir; kaynak/build/launcher/kayıt/oyuncusuna dokunma, sonuçları otomatik geri uygulama. Aynı proje iki eşzamanlı Unity sahibi kullanamaz; Library/Temp/Logs/Builds paylaşılmaz. Alt ajanlar yalnız atanmış benzersiz staging alanlarında çalışır, Unity/build/player açmaz ve ajan çoğaltmaz. Root dahil en fazla 4 eşzamanlı ajan vardır.

Yönetim/inceleme Sol 6.1 ultra; sanat, Meshy promptları, model/doku/rig/animasyon/VFX/SFX/UI ve mini sanat QC Sol 6.1; Home V3 sanat görevleri ultra. Fast zorunlu değildir, Standard kullanılır. Entegrasyon Luna xhigh; sanat üretmez, eksik bileşeni sanat kuyruğuna bildirir. Sessiz model ikamesi yapma. Root nihai entegrasyon ve kabul sahibidir. Ücretli sağlayıcı işi/abonelik/kredi harcaması için önceden kullanıcı onayı gerekir; kimlik bilgilerini kaydetme veya yazdırma.

## Home V3 kontrolleri ve değişmeyen oyun kuralları

- WASD/oklar hareket, Shift koşu, Space zıplama.
- E aç/kapat ve ana etkileşimdir: kapı/kapak/radyo/bunker/bahçe işi. Açık dolapta tekrar E kapatır; E malzeme toplamaz.
- F görsel olarak seçili malzemeyi alır; envanterde ilk yardım üretir. Açık kapta Z/X malzemeyi seçer. Gerçek içerik nesnesine fare tıklamasıyla seçim/toplama desteklenir.
- Tab envanter, B aynı çanta penceresi; oklar seçim, Enter çantaya koy, F üret, Backspace geri.
- Q ana hedefi seçer; T duvar/zemin ankrajı; 1 ankraj, 2 koruma, 3 güçlendirme, 4 taşıma. Taşırken R 45° döndürür, Enter bırakır, Backspace iptal eder.
- İşte A/D hizala, Enter kilitle, Space uygula; Space iş sırasında zıplatmaz. Esc duraklat/geri. HUD ve world aynı tuşu iki kez işlemez.

12 envanter/8 çanta, evden toplama, ilk yardım/pil, aktif işler, duvar/zemin ankrajı, eşya taşıma/döndürme/geçerli yerleşim, yapı yükseltmeleri, bahçe önlemleri, üç afet, son süre/bunker/yeterli çanta/%65 hasar sınırı, başarı/ödül, hasar gezisi, kalıcı önlemler, sonraki dalga ve yerel highscore korunur. Kayıp, collision veya bütçe kurallarını testi geçirmek için gevşetme.

## Kaynak ve kanıt durumu

Unity 6000.0.73f1 Built-In Windows, çevrimdışı. Güncel sahne `Assets/Game/Scenes/TooLateToPanicResidence.unity`, runtime `PanicHomeWorld`. `Documentation/GDD.md` v1.5 Home V4 kurallarıdır; sözleşme oda/etkileşim/atmosfer kapsamını belirler.

Tarihsel Home V3 kabul kaydı (güncel V4 için LATEST-BUILD.json): `Documentation/HOME-V3-DELIVERY-2026-10-16.md`, `LATEST-BUILD.json`, `source-build-v3/native-v3` kanıtı; son DLL `839053E8...` ile 18/18 kısa native kontrol, 0 hata, yaklaşık 55,57 FPS ve normal açılış. Bu otomatik API kontrolüdür; manuel oynanış, işitsel kabul ve uzun kampanya değildir. Tarihsel MVP/Home V2 builder ve kanıtlarını güncel kabul sayma. Güncel builder `TooLateToPanic.PanicResidenceBuild.Build`, sahne Residence, çıktı `Builds/Home-V3-20261016/TooLateToPanic.exe`. Yalnız runtime onarımında daha önce aynı V3 art bake doğrulandıysa `-reuseResidenceArt` kullanılabilir; sanat değiştiğinde tam bake gerekir.

Güncel testte `--refinement-smoke --home-evidence <izole-klasör>` ve yeni GUID profili kullan; normal oyunda test argümanı kullanma. Kaynak manifesti, son `Assembly-CSharp.dll` hash'i, sahne/profil, build/log ve render karelerini aynı teslim kaydına bağla. Kaynak/import/derleme/native API kontrolü/elle oynanış/nihai görsel kabul ayrı sonuçlardır.

Bilinen düzeltmeleri koru: SkinnedMeshRenderer ve MeshFilter mesh'lerini bake sırasında kalıcılaştır; gerekli shader'ı açıkça tut; tam gövde ten materyalini geri getirme; runtime-only parçacık alfa varyantına güvenme; alfa zarfında normalize edilmiş SmoothStep/InverseLerp kullan; iş maliyeti atomik olsun; restart bütün koşu durumunu sıfırlasın; taşınan depoların kaynak koordinatları güncellensin.

Paket başına tek birleşik import/build ve 3–5 dakikalık ilgili native smoke yap. Home V3 kontrolü odalar/fiziksel merdiven/zıplama, E-F ayrımı/seçili gerçek loot, envanter, bir aktif iş, radyo/kurtarma/sinematik ve süre kayıp işaretini kapsar. Somut hata varsa dar onarım/tekrar yap; geniş kampanya gün sonunda veya somut riskte. Crash, veri kaybı veya temel akış engeli açıkken geçti deme.

## Teslim, temizlik ve dokümantasyon

Her paket için tasarım kararlarını, yapılan işi, tuşları, asset kaynağı/lisansını, dosya sahipliğini, kaynak/build kimliğini, gerçek native kapsam/sonuçlarını ve eksikleri `Documentation` ile güncel README durum indeksine kaydet. Küçük tarihsel metin/hash/receipt kanıtını koru; gereksiz eski tam binary kopyaları ve çelişen launcher biriktirme.

Yeni build gerçekten açılıp kısa ilgili smoke'u geçince yalnız bu projenin `Builds/` altındaki eski build/portable ZIP adaylarını temizle. Tek son doğrulanmış çıktı, tek portable ZIP, tek açık launcher ve `LATEST-BUILD.json` tut. Önce çözülmüş absolute hedefin bu projenin `Builds/` dizini içinde kaldığını doğrula; değişkenle geniş recursive silme/taşıma yapma. Başka proje, kaynak, lisans, gerçek kayıt veya kullanımda olan build'i silme; kullanıcı oyuncusunu kapatma. Yeni aday geçmeden çalışan tabanı kaldırma. Bu kurallar kalıcı proje talimatıdır; uygulamanın global system mesajının değiştiği iddia edilmez.

16 Ekim Home V3 temizlik kaydı: MVP ve eski ZIP'ler Geri Dönüşüm Kutusu'na taşındı. Home V2 oyuncusu PID 46944 açık olduğu için kullandığı klasör korunuyor; güncel launcher yalnız V3'ü açar. Kullanımda olan build'i kapatmadan/bozmadan koruma kuralı geçerlidir.



Home V4 son kabul: native-v3/source-build-v3, 12/12 PASS, 0 hata, yaklaşık 55,73 FPS; Assembly-CSharp SHA256 670CA2F91082EAF78AC6A5508DAA05F06A6D1940103AD8A94E8D5FABF17ED56A. Normal açılış gözlendi. Aktif build/ZIP/temizlik istisnaları LATEST-BUILD.json içindedir; kullanımda olma durumunu her temizlik öncesi yeniden kontrol et. Tarihsel PID'yi güncel sanma.


Home V4.1 son kabul: source-build-v2 + native-v2-dba44e40-8e2b-4cce-8232-d85196caa8c0, 16/16 PASS, 0 hata, 13.91s, DLL SHA256 5231FAC344E9DEE03C4CF3C35589A803A5DBC5CC5DBAAEF2FAC4338685E50B33. Kaynak/prefab cache farkı 0; normal açılış gözlendi. Yalnız ilgili kapı/dolap fiziksel API smoke'udur, manuel/tüm kampanya değildir. Güncel yollar ve kullanımda korunan önceki build LATEST-BUILD.json içindedir.


Home V4.2 son kabul: source-build-v2 + native-v2-5512a13c-0d42-4237-a130-23cadb7eb904, 11/11 PASS, 0 hata, 57.82s. DLL SHA256 B728B979129F8D747B477377C31EDBB6D6BD0DA61B9F5822EA460B0A89E6C855. Dört kapı/sekiz taraf ve bodrum–üst kat–bodrum gerçek fizik rotası; 4kapı+1bodrum başlangıç yerleştirmesi. Manuel/tüm kampanya değildir. Normal açılış gözlendi; LATEST-BUILD.json kullanımda korunan eski buildleri ve güncel paketi gösterir.


Home V4.3 son kabul: source-build-v3 / native-v3-e0365b78-a92e-4f59-8252-3b646eadae2e, 11/11 PASS, 0 hata, 19.88s. DLL SHA256 EE9B8A269FF8D73718D358F3BBE2B39E17086EBBA6115FE10D45A75CBE6710E1. İki temsilî kapının iki tarafında itmeden bekleme ve gerçek yürüyüşle çekilince otomatik kapanma, mouse ekran ışını/yerleştirme API'leri, ikonlu malzeme açıklarının gerçek E/F toplamayla güncellenmesi ve yeni banyo geometrisi geçti. Diğer iki kapı/tam merdiven rotası yeniden denenmedi; kalıcı korkuluk/controller değişmezleri kontrol edildi. Kaynak/prefab farkı0; normal açılış görüldü. Manuel/OS giriş/tüm kampanya/FPS kabulü değildir. Güncel build/ZIP ve kullanımda korunan V4.2 istisnası LATEST-BUILD.json içindedir.


Home V4.4 nihai kabul: source-build-v4 / native-v4-1064ed5d-bef0-4b34-ad55-dddb39c46e4e,9/9 PASS,0 runtime hata,51.94s. DLL SHA256 85D5F293848E873B82F0AFCE43733F2CC2F40CA031F079DBC73FBF4B91EF9BFF. Sekiz iki yönlü çapraz ön giriş/dönüş, iki yarı kapanma→E tekrar açma kolu ve güney banyo yakın giriş rotası geçti. 11 kurulum +1 StartGame sayıldı, rota Warp0; controller/bütçe korunur. Normal açılış ve portable DLL eşleşmesi görüldü. Manuel/tüm kampanya/OS giriş kabulü değildir. Önemli: Quaternion.Angle(current,next) küçük adımları0 sayabildiğinden no-motion kestirmesinde kullanma; tam current.Equals(next) kullan. autoSyncTransforms0 olduğundan controller.Move öncesi Physics.SyncTransforms korunur. Güncel build/ZIP ve açık eski klasör istisnaları LATEST-BUILD.json içindedir. FPS geliştirmesi durdurulmuş kararı geçerlidir.

17 Ekim Home V4.5 son istek: F2/görünür geliştirici paneli; bağımsız sınırsız bütçe, iş/üretim malzemesi harcamama, hazırlık saati dondurma; +1000 bütçe. Model.SetDeveloperOption ve AddDeveloperBudget tek mutasyon yoludur. Menü açmak hile sayılmaz; kullanım CheatedRun sticky işaretini o koşu boyunca tutar ve skor yazımını engeller. Kapalı seçeneklerde normal maliyet/fizik/kayıp kuralları korunur. DeveloperMenuOpen/DeveloperInputBlockedThisFrame hem Model.Tick hem dünya girişi için kapıdır; ProcessDeveloperHotkeys Update başında çağrılır. Önceki pause/lock/help sahipliği korunur. Yeni sözleşme ../Evidence/GamejamOrchestra/TLTP-DEVELOPER-V45-20261017/CONTRACT.md; builder0.4.5/Builds/Home-V4.5-20261017, aynıartcache. Yeni --developer-smoke izoleGUIDprofilidir; eskikapıtesti buDLLkabulü değildir. Kullanıcı GitHub kaantufan/FixThisMakeNoMistake deposuna kaynak ve oynanabilir paket yükleme yetkisi verdi; forcepush yok, cache/kişisel kayıt/anahtarlar gitdışı; büyükWindowsZIP Releaseassetidir. FPS askıda ve korunur. Nihai yerel/uzak kanıt LATEST/teslim kaydında.

Home V4.5 yerel kabul: source-build-v1/native-v1-a11f71de-8b26-4eeb-ac4e-218de111a073,12/12 PASS,0 hata; DLL 8399ADBAE91D43604CD28936C84927CC425412FF2B2E940090F4193AC690EE4C. Normal açılış, iki HUD karesi, izole skor hash korunması ve portable eşleşmesi görüldü. Durum fixture'lı API smoke'dur; manuel/minigame/tüm kampanya kabulü değildir. Güncel LATEST ve HOME-V4.5-DELIVERY-2026-10-17.md yollarını kullan.


17 Ekim GitHub yayını: https://github.com/kaantufan/FixThisMakeNoMistake maincommit7b30510e44f7e5d5e91ba9bbc7a7ab30a2bfe771,releasev0.4.5; remoteZIPdigest yerelCE63B560... ileeşleşti. Yayıncheckout'u Evidence/GamejamOrchestra/TLTP-DEVELOPER-V45-20261017/publication/repository, asılworkspace nonGit. Gelecekpush'ta uzakmain'i kontrol et, aynıbatch github/Prepare-GitHubSource.ps1 allowlist/GUIDkapanımını tekrarçalıştır; cache/saves/Evidence/FPS/DeferredRadioWAV gitdışı; forcepushyok. Yerel build/native ve public staticGUIDkanıtı ayrı tutulur. Kaynak43runtime/shader byteeşleşmesi ve2894takipli dosya doğrulandı.



17 Ekim Home V4.6 HUD teslimi: HTML konsepti yeni IMGUI HUD'ye uyarlandı; çevrimdışı OFL fontlar, ayrı dalga/süre/kaynak kartları, canlı eksikler ve ortak çanta/iş/sonuç ekranları. Güncel sözleşme Evidence/GamejamOrchestra/TLTP-HUD-V46-20261017/CONTRACT.md, builder0.4.6/Builds/Home-V4.6-20261017. source-build-v3 / native-v3-e3882617-c03a-4fc6-b8bc-907a2cb52037,75/75PASS,0hata,50render,12.16s; beş çözünürlük. DLL6E270494D3B663867DA5F0E190AB5A13D6A0E9779EE3ED742400795EF7261705. Ek dar fiziksel OS Enter/Tab/Esc/F2 ve fare kontrolü geçti; tam kampanya/mini oyun tamamlama değildir. 960x540 küçük açıklamalar ufak kalır. Kayıt hash'i korundu. Son normal süreç başlatıldı/readylog görüldü; kullanıcı Esc ile Computer Use'ı durdurduğu için yeni normal pencereye ek UI kontrolü yapılmadı. LATEST-BUILD.json ve Documentation/HOME-V4.6-HUD-DELIVERY-2026-10-17.md tek güncel teslimi bağlar. Geçmiş build metin yolları değiştirilmez; FPS askıda korunur.


## Home V4.7 güncel teslim kararı —17Ekim

Yeni kullanıcı isteğiyle izometrik kısa hazırlık akışı, seçili Yarkın importları,12prop sanat detay geçişi, görsel pickup/sabitleme/deprem tepkisi ve yalnız bu akışta7kısa ElevenLabs konuşma+3SFX yetkilendirildi.537mevcut kredi kullanıldı; yeni ödeme/abonelik/overage yok. Önceki yalnız-afet politikası seçili kısa akış sesleri için güncellendi; eski Tolga ve normal kampanya konuşması ertelenir. FPS askıda korunur; zincirleme afet ertelenir.

Oyuncuya görünen metin ve seste jüri/demo sunum dili yoktur. HAZIRLIĞA BAŞLA kısa akış, SERBEST OYUN normal kampanya.270sn içerik/300sn sınır; ilk180sn keyif insanla doğrulanacak hedeftir. SAHNEYİ ATLA/Enter model sonucunu tek kez çözer ve kontrolü geri verir. F2 Kaybetmeme gerçek hasar/erzak tüketimini korur, kayıp yerine ödülsüz inceleme açar; sticky skor dışı. HUD sinematik saatini InputLocked ile durdurmaz.

Güncel kanıt ve hashler LATEST-BUILD.json ile Documentation/HOME-V4.7-DELIVERY-2026-10-17.md içindedir. Eski native sonuçları yeni DLL kabulü değildir. Worker yalnız atanmış staging; root tek Unity/build/player sahibidir.


Home V4.8 güncel akış kabulü: TLTP-FULLFLOW-QA-20261017; 149/149 gerçek zamanlı kesintisiz rota + 189/189 regresyon,0runtime hata. Kısa akış49.67sn, dört doğal simülasyon; test Warp/Tick/hile0. Üretim API girdisi, OS/insan keyif kabulü değildir. LATEST ve HOME-V4.8-DELIVERY-2026-10-17.md yeniDLL/manifesti bağlar; eskiV4.7 açıkoyun korunur.
