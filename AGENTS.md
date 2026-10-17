# Too Late to Panic! — çalışma düzeni

## Güncel oyun ve çalışma alanı

Oyunun kesin adı **Too Late to Panic!**; tema **Afete Hazırlık ve Önlem**. Kullanıcının 16 Ekim 2026 tarihli kararıyla bu çalışma alanı yalnız bu oyuna aittir. İlgisiz kaynaklar, build'ler, belgeler ve eski oyun yönergeleri kaldırılır; klasör ve aktif belge adları bu oyuna göre düzenlenir. Başka bir oyunun hedeflerini yeniden kapsam sayma.

- `TooLateToPanic/`: aktif izometrik oyun; güncel geliştirme Home V4.9.
- `TooLateToPanicFPS/`: kullanıcının 16 Ekim son kararıyla geliştirmesi durdurulmuş bağımsız FPS denemesi; kaynakları korunur, yeni görev başlatılmaz.
- `Docs/TOO-LATE-TO-PANIC-GDD.md`: ortak temel oyun tasarımı; Home V3 ve FPS farkları kendi sözleşmelerinde.
- `Docs/TOO-LATE-TO-PANIC-TEAM.md`: ekip ve teslim düzeni.
- `Tools/GamejamOrchestra/`: bu oyunun görev metinleri, şablonları ve deklaratif politikası.
- `Evidence/GamejamOrchestra/TLTP-*/`: bu oyunun staging, kaynak manifesti, derleme ve test kayıtları.

Teknik hedef Unity **6000.0.73f1**, **Built-In**, Windows ve çevrimdışı yerel oyundur. Gereksiz framework, renderer, controller veya paket göçü başlatma. Her görevde ilgili projenin yerel `AGENTS.md`, güncel sözleşmesi ve gerçek ayarlarını oku.

16 Ekim 2026 son kullanıcı açıklaması: **FPS geliştirmesi askıdadır; tamamen iptal edilmemiştir ve kullanıcı istediğinde yeniden dönülebilir.** Aktif üretim ve birleşik raporun kabul/asset planı yalnız izometrik oyundur; FPS uyarlaması veya testi izometrik teslimin bağımlılığı değildir. FPS kaynakları, mevcut build/paket, lisanslar, bağımsız kayıt kimliği, gerçek kayıtlar ve kanıtlar korunur; askıya alma temizleme/birleştirme yetkisi vermez. Dönüşte aktarılacak değişiklikler ayrıca seçilir. Güncel rapor `Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md`.

17 Ekim jüri süresi kararı: kullanıcı jüri üyelerinin **en fazla 5 dakika** oynayabileceğini bildirdi. Birleşik rapor Bölüm 5.1’de ayrı izometrik jüri senaryosu için **270 sn içerik + 30 sn yedek**, bir tam hazırlık–afet–sonuç döngüsü ve sel/yangında kısa oynanır örnekler planlanır. İş tekrarı/lojistik/öğrenme yükü düşürülür; ana kampanya kuralları, gerçek girdi, maliyet/iptal ve skor yalıtımı korunur. Bu bir plan kararıdır; uygulanmış veya insanla süre kabulü yapılmış sayılmaz. Önceki daha uzun demo önerisi geçersizdir; FPS askıda kalır.

17 Ekim ek rapor kararı: sade geri bildirim kapsamı kabul edildi; ilk **180 sn** içinde küçük kazanım, gerçek önlem ve görünür afet sonucu hedeflenir. Seçili hedef vurgusu, yalnız küçük pickup’ta hafif hareket, hızlı toplama yayı, gerçek işe bağlı braket ve iki eşyalı kontrollü afet tepkisi mevcut sürelerin içine alınır; ek görev/bekleme oluşturmaz. Vana kolu ikincil cila, zincirleme afetler sonraki kapsamdır. Toplam **270+30 sn** korunur; akıcılık/keyif/devam isteği henüz ölçülmüş kabul değildir. Birleşik rapor5.2 kapsamı kaydeder; ayrıntılı uygulama planı kullanıcı sonraki isteğinde hazırlanacaktır.

## İki bağımsız projenin sahipliği

Her sohbetin root'u yalnız kendi projesinin Unity/editor/import/build/player sahibidir. Aynı projenin eşzamanlı iki sahibi olamaz. Kullanıcı artık yalnız izometrik sürümden devam etmektedir; önceki iki sürümü karşılaştırma hedefi geçersizdir. FPS kaynakları korunur; yeni açık kullanıcı isteği olmadan FPS geliştirmesi veya ajan görevi başlatılmaz. Projeler arasında ortak kaynak/junction, paylaşılan Library/Temp/Logs/Builds, build üzerine yazma veya gerçek kayıtları karıştırma yoktur. FPS değişiklikleri izometrik kaynaklara otomatik uygulanmaz.

FPS sözleşmesi `Evidence/GamejamOrchestra/TLTP-FPS-FORK-20261016/FPS-HANDOFF.md`; FPS yerel `AGENTS.md` kamera, asset, bağımsız ürün/kayıt kimliği ve kabul sınırını belirler. İzometrik kesit geometrisi FPS için zorunlu değildir. Mevcut açık kullanıcı oyunu kapatılmaz.

## İzometrik Home V3 kapsamı

Güncel izometrik sözleşme `Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/CONTRACT.md`. Home V3 tabanı korunur. Home V2'nin iki yaşanabilir kat, görünür bahçe/çevre, karakter, malzeme toplama, envanter/afet çantası, aktif iş mini oyunları, duvar/zemin ankrajı, açılır kapaklar, bahçe önlemleri ve afet döngüsü korunur.

Home V3: kısa ve kolay Türkçe, nesne odaklı görsel toplama, daha dolu ve özenli renkli ev, balkon, banyo, bodrum ve bitişik garaj, zıplama, gerçek Türkçe radyo anonsları, afet sonrası kurtarma helikopteri/atmosfer, kompakt HUD ve gerçek kamera/sahne sinematikleri. Süre bittiğinde kayıp geri bildirimi başarısızlık sinematiğinden önce görünür.

Tuş sözleşmesi: WASD/ok hareket, Shift koşu, Space zıpla; E aç/kapat/ana etkileşim, F seçili malzemeyi al; Tab envanter, Enter onay/çantaya koy, Backspace geri/iptal. İş sırasında Space iş eylemidir. Diğer tuş ve mimari ayrıntıları Home V3 sözleşmesinden al. Üretimde olan özellikleri uygulanmış veya native kabul edilmiş sayma.

Temel oyun kuralları afet/bütçe/dalga/çanta/bunker/kayıp ve yerel highscore döngüsüdür. FPS bunları korur; kamera, fiziksel erişim ve etkileşimi birinci şahsa göre tasarlar. Kaynak, derleme, kısa native smoke ve son görsel/oynanış kabulü ayrı kanıtlardır.

## Ekip ve dosya sahipliği

- Yönetim ve kritik inceleme: `gpt-6.1-sol`, `ultra`.
- Sanat, model/doku/rig/animasyon/VFX/SFX/UI ve kısa asset QC: standart `gpt-6.1-sol`. Fast zorunlu değildir; sessiz model ikamesi yapılmaz.
- Entegrasyon: `gpt-6-luna`, `xhigh`; onaylı sanatın mevcut API'ye bağlanması. Yeni sanat, klip, efekt, ses veya shader tasarımı üretmez.
- Paket QA incelemesi: `gpt-6.1-sol`, `ultra`; Unity/player işlemi yalnız ilgili proje root'undadır.

Bu dosya sohbetin modelini değiştirmez; gerçek uygunluğu görev başında doğrula. Root dahil en fazla dört eşzamanlı ajan, yani en fazla üç alt ajan vardır. Root dışında ajan çoğaltılmaz. Model/thinking override görevleri `fork_turns: "none"` ve açık taşınmış bağlamla açılır.

Alt ajanlar yalnız root'un atadığı benzersiz staging alanına yazar. Ortak dosyanın aynı başlangıç sürümü iki coder'a verilmez. Belgeler/araçlar için root tek yazarı olan açık bir dosya alanı atayabilir. Sanat üretimi, mini QC ve entegrasyon rolling queue ile ilerler; sonraki staging paketi önceki paketin native testini beklemek zorunda değildir.

Gerekli materyal/gölge/ışık, rig/animasyon, collider, etkileşim, VFX/SFX ve mekanik bağlantıları değerlendirilir. Gereksiz özellik gerekçeli `not_applicable`, gereken eksik ise açık eksik olarak kaydedilir. Sağlayıcı önizlemesi, indirme, import, kaynak entegrasyonu ve native kabul birbirinin yerine geçmez. Ücretli sağlayıcı işi veya kredi kullanımı kullanıcı onayı olmadan başlatılmaz; lisanslar korunur.

## Teslim, temizlik ve doğrulama

Paket başına bir birleşik import/build ve tek 3–5 dakikalık ilgili mekanik smoke yeterlidir. Kaynaklar bu sırada dondurulur. Somut hata için dar düzeltme ve gerekli tekrar yapılır; geniş tur gün sonunda veya somut yeni riskte çalıştırılır. Crash, veri kaybı veya temel döngüyü durduran hata açıkken geçti deme. Küçük mesh kusurları için saatlerce mikro onarım yapma; kayıp/collision/bütçe sınırlarını test geçsin diye gevşetme.

Her paket ilgili projenin Documentation, README ve güncel build indeksinde değişiklikler, kontroller, asset/lisans, kaynak/build hashleri, gerçek test sonucu ve kalan sınırlarla belgelenir. Non-Git çalışma alanında küçük kaynak snapshotları, SHA256 manifestleri ve build receipt'leri izlenebilirlik sağlar.

Her projenin Builds alanında yalnız son doğrulanmış çalışır build ve tek güncel taşınabilir ZIP tutulur. Önce yeni build açılır ve kısa smoke geçer, sonra eski çıktı temizlenir. Tek güncel launcher ve `LATEST-BUILD.json` güncellenir. Temizlik ilgili projenin Builds sınırını, kaynakları/lisansları, kardeş projeyi, gerçek kayıtları ve açık kullanıcı player'ını korur. Recursive silme/taşıma öncesi çözülmüş mutlak hedefleri doğrula. Kullanıcının açık çalışma alanı temizleme/yeniden adlandırma talebi normal build temizliğinden ayrı yetkilendirmedir.

TLTP geçmişinin küçük metin/hash/receipt kayıtları korunur. Klasör taşınırken tarihsel manifestlerin içindeki özgün yollar/hashler toplu değiştirilmez; eski-yeni yol eşlemesi ayrı receipt ile kaydedilir. Aktif belge ve başlatıcılarda geçerli relative yollar kullanılır.

16 Ekim son kullanıcı ses kararı: izometrik Home V4'te yalnız afet sesleri etkin; genel/AI seslendirme en son ElevenLabs aşamasına ertelendi. Kayıtlar kaynak arşivinde korunur, yeni build'de konuşma çalınmaz. Aktif promptlarda bu son karar eski radyo konuşması hedefini geçersiz kılar. Harcama/sağlayıcı işi henüz yetkili değildir.

16 Ekim Home V4.1 ek isteği: üst banyo dar koridorsuz ayrı oda, tek kapı. Yeni dar kapsam sözleşmesi Evidence/GamejamOrchestra/TLTP-BATHROOM-V41-20261016/CONTRACT.md; Home V4 temel kapsamı/ses politikası korunur. Güncel derleme ve kısa banyo kontrolü LATEST-BUILD.json ile bağlanır. Tarihsel Home V4 12/12 sonucu yeni DLL kanıtı değildir.

16 Ekim Home V4.2: kapılar açılışta oyuncudan uzağa yön seçer; merdiven ve boşluk korkulukları fiziksel engeldir. Son sözleşme Evidence/GamejamOrchestra/TLTP-DOORS-STAIRS-V42-20261016/CONTRACT.md. Home V4.1 sabit banyo açılış yönü geçersizdir; tek girişli oda korunur. Güncel kabul yeni build/DLL/kapı-merdiven native kanıtıyla yapılır.

16 Ekim Home V4.3 son sözleşme Evidence/GamejamOrchestra/TLTP-INTERACTION-V43-20261016/CONTRACT.md: güvenli dur/devam kapı kapanışı, fareyle eşya taşıma, ikonlu kesin eksik adetleri, güney duvarında yeni banyo girişi. Eski batı kapısı geri getirilmez. Kaynak/build/native kabul yeni LATEST-BUILD.json ile bağlanır; eski11/11 bu yeni DLL kabulü değildir.

Home V4.3 teslim edildi: LATEST-BUILD.json / Documentation/HOME-V4.3-DELIVERY-2026-10-16.md. 11/11 kısa ilgili native kontrol; 2 temsilî kapı/4 taraf, mouse yerleşim API'leri, gerçek malzeme açıkları ve yeni güney banyo girişi. Kaynak/prefab farkı0; bu sonuç manuel/tüm kampanya kabulü değildir. Üretim kapı güvenliği gerçek controller.center ve boyutlarla analitik dikey kapsül–Y ekseninde dönen kutu uzaklığıdır; bağımsız QA standart etkin CapsuleCollider sanal poz sorgusu kullanır. Önceki devre dışı proxy veya gerçek CharacterController kök pozunu ComputePenetration'a verme yaklaşımlarına dönme: native'de itme tespit edildi. Güncel launcher V4.3; açık kullanıcı V4.2 build'i korunur, sonraki temizlemede süreçler yeniden kontrol edilir.

Home V4.4 nihai kabul: source-build-v4 / native-v4-1064ed5d-bef0-4b34-ad55-dddb39c46e4e,9/9 PASS,0 runtime hata,51.94s. DLL SHA256 85D5F293848E873B82F0AFCE43733F2CC2F40CA031F079DBC73FBF4B91EF9BFF. Sekiz iki yönlü çapraz ön giriş/dönüş, iki yarı kapanma→E tekrar açma kolu ve güney banyo yakın giriş rotası geçti. 11 kurulum +1 StartGame sayıldı, rota Warp0; controller/bütçe korunur. Normal açılış ve portable DLL eşleşmesi görüldü. Manuel/tüm kampanya/OS giriş kabulü değildir. Önemli: Quaternion.Angle(current,next) küçük adımları0 sayabildiğinden no-motion kestirmesinde kullanma; tam current.Equals(next) kullan. autoSyncTransforms0 olduğundan controller.Move öncesi Physics.SyncTransforms korunur. Güncel build/ZIP ve açık eski klasör istisnaları LATEST-BUILD.json içindedir. FPS geliştirmesi durdurulmuş kararı geçerlidir.

17 Ekim Home V4.5 son istek: F2/görünür geliştirici paneli; bağımsız sınırsız bütçe, iş/üretim malzemesi harcamama, hazırlık saati dondurma; +1000 bütçe. Model.SetDeveloperOption ve AddDeveloperBudget tek mutasyon yoludur. Menü açmak hile sayılmaz; kullanım CheatedRun sticky işaretini o koşu boyunca tutar ve skor yazımını engeller. Kapalı seçeneklerde normal maliyet/fizik/kayıp kuralları korunur. DeveloperMenuOpen/DeveloperInputBlockedThisFrame hem Model.Tick hem dünya girişi için kapıdır; ProcessDeveloperHotkeys Update başında çağrılır. Önceki pause/lock/help sahipliği korunur. Yeni sözleşme Evidence/GamejamOrchestra/TLTP-DEVELOPER-V45-20261017/CONTRACT.md; builder0.4.5/Builds/Home-V4.5-20261017, aynıartcache. Yeni --developer-smoke izoleGUIDprofilidir; eskikapıtesti buDLLkabulü değildir. Kullanıcı GitHub kaantufan/FixThisMakeNoMistake deposuna kaynak ve oynanabilir paket yükleme yetkisi verdi; forcepush yok, cache/kişisel kayıt/anahtarlar gitdışı; büyükWindowsZIP Releaseassetidir. FPS askıda ve korunur. Nihai yerel/uzak kanıt LATEST/teslim kaydında.


Home V4.5 yerel kabul: source-build-v1/native-v1-a11f71de-8b26-4eeb-ac4e-218de111a073,12/12 PASS,0 hata; DLL 8399ADBAE91D43604CD28936C84927CC425412FF2B2E940090F4193AC690EE4C. Normal açılış, iki HUD karesi, izole skor hash korunması ve portable eşleşmesi görüldü. Durum fixture'lı API smoke'dur; manuel/minigame/tüm kampanya kabulü değildir. Güncel LATEST ve HOME-V4.5-DELIVERY-2026-10-17.md yollarını kullan.


17 Ekim GitHub yayını: https://github.com/kaantufan/FixThisMakeNoMistake maincommit7b30510e44f7e5d5e91ba9bbc7a7ab30a2bfe771,releasev0.4.5; remoteZIPdigest yerelCE63B560... ileeşleşti. Yayıncheckout'u Evidence/GamejamOrchestra/TLTP-DEVELOPER-V45-20261017/publication/repository, asılworkspace nonGit. Gelecekpush'ta uzakmain'i kontrol et, aynıbatch github/Prepare-GitHubSource.ps1 allowlist/GUIDkapanımını tekrarçalıştır; cache/saves/Evidence/FPS/DeferredRadioWAV gitdışı; forcepushyok. Yerel build/native ve public staticGUIDkanıtı ayrı tutulur. Kaynak43runtime/shader byteeşleşmesi ve2894takipli dosya doğrulandı.


Yayın sonrasında canlı World/Builder kaynaklarında HomeV4.6 HUD/concept değişikliği gözlendi. Bunlar bu V4.5 tesliminden sonradır; geri alınmadı ve doğrulanmadan GitHub'a eklenmedi. V4.5 kabulü final-source49dosya, yayınlanan43runtime/shader, DLL8399ADBA... ve remoteZIPCE63B560... ile sabittir. Güncel canlı kaynak ile bu yayımlanmış paket aynı kabul edilmez; post-publication-live-source-observation.json ayrımı kaydeder.



17 Ekim Home V4.6 HUD teslimi: HTML konsepti yeni IMGUI HUD'ye uyarlandı; çevrimdışı OFL fontlar, ayrı dalga/süre/kaynak kartları, canlı eksikler ve ortak çanta/iş/sonuç ekranları. Güncel sözleşme Evidence/GamejamOrchestra/TLTP-HUD-V46-20261017/CONTRACT.md, builder0.4.6/Builds/Home-V4.6-20261017. source-build-v3 / native-v3-e3882617-c03a-4fc6-b8bc-907a2cb52037,75/75PASS,0hata,50render,12.16s; beş çözünürlük. DLL6E270494D3B663867DA5F0E190AB5A13D6A0E9779EE3ED742400795EF7261705. Ek dar fiziksel OS Enter/Tab/Esc/F2 ve fare kontrolü geçti; tam kampanya/mini oyun tamamlama değildir. 960x540 küçük açıklamalar ufak kalır. Kayıt hash'i korundu. Son normal süreç başlatıldı/readylog görüldü; kullanıcı Esc ile Computer Use'ı durdurduğu için yeni normal pencereye ek UI kontrolü yapılmadı. LATEST-BUILD.json ve Documentation/HOME-V4.6-HUD-DELIVERY-2026-10-17.md tek güncel teslimi bağlar. Geçmiş build metin yolları değiştirilmez; FPS askıda korunur.


## Home V4.7 güncel teslim kararı —17Ekim

Yeni kullanıcı isteğiyle izometrik kısa hazırlık akışı, seçili Yarkın importları,12prop sanat detay geçişi, görsel pickup/sabitleme/deprem tepkisi ve yalnız bu akışta7kısa ElevenLabs konuşma+3SFX yetkilendirildi.537mevcut kredi kullanıldı; yeni ödeme/abonelik/overage yok. Önceki yalnız-afet politikası seçili kısa akış sesleri için güncellendi; eski Tolga ve normal kampanya konuşması ertelenir. FPS askıda korunur; zincirleme afet ertelenir.

Oyuncuya görünen metin ve seste jüri/demo sunum dili yoktur. HAZIRLIĞA BAŞLA kısa akış, SERBEST OYUN normal kampanya.270sn içerik/300sn sınır; ilk180sn keyif insanla doğrulanacak hedeftir. SAHNEYİ ATLA/Enter model sonucunu tek kez çözer ve kontrolü geri verir. F2 Kaybetmeme gerçek hasar/erzak tüketimini korur, kayıp yerine ödülsüz inceleme açar; sticky skor dışı. HUD sinematik saatini InputLocked ile durdurmaz.

Güncel kanıt ve hashler LATEST-BUILD.json ile Documentation/HOME-V4.7-DELIVERY-2026-10-17.md içindedir. Eski native sonuçları yeni DLL kabulü değildir. Worker yalnız atanmış staging; root tek Unity/build/player sahibidir.


Home V4.8 güncel akış kabulü: TLTP-FULLFLOW-QA-20261017; 149/149 gerçek zamanlı kesintisiz rota + 189/189 regresyon,0runtime hata. Kısa akış49.67sn, dört doğal simülasyon; test Warp/Tick/hile0. Üretim API girdisi, OS/insan keyif kabulü değildir. LATEST ve HOME-V4.8-DELIVERY-2026-10-17.md yeniDLL/manifesti bağlar; eskiV4.7 açıkoyun korunur.


## Home V4.9 güncel iş sözleşmesi —17Ekim

Son kullanıcı kararı eski A/D–Enter–Space Ratchet/Timing/Balance zorunluluğunu geçersiz kılar. Sabitlemede iki gerçek vida fareyle seçilir, her biri0,60sn otomatik takılır. Diğer uygun hedefler0,60sn dünya montajıdır. Duvar/pencere gerçek güçlendirme parçası ve karakter build hareketi; yangın tüpü yalnız fire_station;3bölge×4olgun ağaç; hareket×1,35. Yalnız geçerli/tamamlanmamış önlem seçeneği gösterilir. Malzeme/bütçe tamamlamada bir kez, iptal ücretsiz. Son karakter fork mesajıyla görünüm güzelleştirmesi iptal edildi; eskiV2 görünümü korunur, gerekli montaj hareketi kalır. FPS askıda korunur. YeniDLL kabulü LATEST ve HOME-V4.9-DELIVERY-2026-10-17.md ile bağlıdır: 44/44 direkt iş +141/141 kesintisiz rota,0runtime hata. OS/insan/tüm kampanya kabulü değildir. Eski sayılar yeniDLLkanıtı sayılmaz.


17 Ekim Home V4.9 QA arka plan teslimi: Computer Use/OS girişi/aktivasyon0. Ankraj ÇÖZ ve ücretsiz iki vidalı yeniden takma, yeni ağaç gövdesi–gerçek controller çakışmasında harcamasız red/iptal, simülasyon60sn uyarısı düzeltildi. DLL471E6AFF3DBD054C3FE099FD20AABFB8A5498661C16325D3AC5217705106AC01; son adayda338/338 mekanik,24/24 fizik,44/44 direkt,141/141 kesintisiz,161/161 GUI,27/27 sonuç/skip,58/58 frame,11/11 kapı-merdiven ve11/11 yerleşim;0runtime hata. Matrix429gözlem kanıt katmanlarıyla geçti;505plan satırının hepsi tam native PASS değildir, kısmi/NOT_RUN/OS sınırları CSV/JSON içindedir. LATEST ve Documentation/HOME-V4.9-QA-DELIVERY-2026-10-17.md günceldir. Eski fixture FAIL kayıtları korunur; sadece test rota yaklaşımı yeni hıza uyarlanmıştır, collision/kayıp/maliyet toleransları gevşetilmedi. Kullanıcının açık V4.8 PID44264 ve DLL hash'i korundu; yeni normal pencere açılmadı. FPS askıda korunur, GitHub güncellenmedi.


## Home V4.11 son kullanıcı kararı ve teslim —17Ekim

Yeni açık kullanıcı isteğiyle FPS, yalnız afet çantası hazırlığı aşamasında aktif oyuna döndü: aynı oyuncu/controller/Model/Expedition, gerçek konumdan1.1sn kamera sinematiği, E/F ile açık3D erzak toplama, G ile aynı yerde izometrik önlemlere dönüş. Genel FPS kampanyası açılmaz; bağımsız eski FPS checkout/kayıtları korunur. Bu karar eski genel askı cümlelerini bu dar kapsam için günceller. Yeni FPS el/kol modeli üçüncü şahıs V2 görünümünü değiştirmez.

Kaynak havuzu/teksefer RNG auto-loot,12+8 yuva drag-drop/partial-fill, hazır ilk yardım, bodrum sığınağı, fiziksel çanta, garaj iç kapısı,3.2m duvar fiziği, dört cephe kesiti, bodrum atmosferi, yerel kemerli şömine, pause/menu düzeltmeleri ve gerçek verili J defteri entegredir. Yarkın11f12b4/defter07ecbcc kaynaklarıyla provenance korunur; yeni ücret/kredi yok. Güncel build/ZIP/LATEST ve Documentation/HOME-V4.11-DELIVERY-2026-10-17.md aynı son adayı bağlar. 156/156native API/render kontrolü,0runtime hata;10konum fikstürü, OS/insan/tüm kampanya kabulü değildir. DLL34E3969FBBE5047A6400D9FE253ACE1B2177BEDD07ECBBDA3020D3745008E047. Önceki test sayıları bu DLL kabulü sayılmaz.


## Home V4.12 son kullanıcı kararı ve teslim —17Ekim

Hazırlık önce izometrik önlemler, son30saniyede aynı gerçek oyuncu/controller konumundan otomatik FPS çantadır; G ile geri dönüş yoktur. Her dalga1su+1gıda yeterli; gerçek sonlu toplama gereken ilk birimi çantaya, artanı envantere ekler. Önlem sırasında çanta erzakları görünmez; çanta sırasında yapı malzemesi toplama/önlem ve dolap açma yoktur. Transit kapıları ve gerçek sığınak E ile kullanılabilir; kısa akış güvenli alanı FPS E ile çözer. Yüzde75ve üzeri hasarda kayıp. Çit inşası ve iki etkileşimsiz ayaklı lamba kaldırıldı; dekoratif sınır çitleri korunur.3×4tam ağaç sırası ve sonlu kaynakta asgari3sıra paketi; kararlı gerçek yüzey/merkez pencere ışını; gerçek3D sınır vurgusu; tek sahibi olan açık bunker kapağı; dış sinematiklerde çatı/Skybox; sade FPS HUD.

Kısa akışın yalnız-ankraj ve boşaltılmış dolap stokları kısıtı kaldırıldı; geçerli isteğe bağlı işler yalnız önlem aşamasında açılır. Gerçek maliyet/iptal/controller/score kuralları korunur. Son kaynak99dosyada0kayma, DLL8E3DE7EEB0D3F52D4F77539B9CBE6D04CA7FCF40043803BACE073579484364CB;204/204native model/API/frame/render,0runtime hata,15konum/saat/hazır fikstürü. OS/insan/tüm kampanya kabulü değildir. LATEST ve Documentation/HOME-V4.12-DELIVERY-2026-10-17.md son r2adayını bağlar. Açık V4.11-r7oyuncusu/kayıtları korunur; bağımsız FPS projesi dokunulmaz. Yalnız başarısız kendi V4.12v1build klasörü silme komutu otomatik onay incelemesinde blocked by policy ile reddedildi; komut çalışmadı, alternatif denenmedi, klasör ve önceki reddedilmiş hedefler korunur.


## Home V4.13 logo ve EXE ikonu —17Ekim

V4.12-r5 son kaynakları korunarak başlık logosu ve boyuta bağlı Windows amblemi eklendi; normal120+30sn, gerçek yüzey/pickup ve yukarı açılan kapak korunur. Sadece HUD/builder C# değişti;98diğer kaynak aynı. DLL97996C5ED34A843FD4FA04D79761C6004A022EF1643DCE79257B4EFD3F83207E; yeni224/224native API/frame/render,0runtime hata,15kurulum fikstürü.1600native ve960renderer pencere başlığı kontrol edildi; gerçek PE16/32small48/256full ve9ikon payloadı doğrulandı. Explorer ek büyük görünümü kullanıcı fizikselEsc ile Computer Use'ı durdurduğundan NOT_RUN; sonrasındaCUgirdisi yok. Yeni DLL tam OS/insan/kampanya kabulü değildir; R5 başarılı OS turu tarihsel kalır. LATEST ve Documentation/HOME-V4.13-BRAND-DELIVERY-2026-10-17.md kaynak/build/ZIPhashlerini bağlar. R4açık kaldı; devirdekiR5süreci finalde görülmedi, bu root ona girdi/sonlandırma göndermedi, dosyaları aynı. Eski reddedilmiş temizlik tekrarlanmadı; bağımsızFPSkorunur.
