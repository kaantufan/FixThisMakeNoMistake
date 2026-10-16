# Too Late to Panic! — çalışma düzeni

## Güncel oyun ve çalışma alanı

Oyunun kesin adı **Too Late to Panic!**; tema **Afete Hazırlık ve Önlem**. Kullanıcının 16 Ekim 2026 tarihli kararıyla bu çalışma alanı yalnız bu oyuna aittir. İlgisiz kaynaklar, build'ler, belgeler ve eski oyun yönergeleri kaldırılır; klasör ve aktif belge adları bu oyuna göre düzenlenir. Başka bir oyunun hedeflerini yeniden kapsam sayma.

- `TooLateToPanic/`: aktif izometrik oyun; güncel geliştirme Home V4.5.
- `TooLateToPanicFPS/`: kullanıcının 16 Ekim son kararıyla geliştirmesi durdurulmuş bağımsız FPS denemesi; kaynakları korunur, yeni görev başlatılmaz.
- `Docs/TOO-LATE-TO-PANIC-GDD.md`: ortak temel oyun tasarımı; Home V3 ve FPS farkları kendi sözleşmelerinde.
- `Docs/TOO-LATE-TO-PANIC-TEAM.md`: ekip ve teslim düzeni.
- `Tools/GamejamOrchestra/`: bu oyunun görev metinleri, şablonları ve deklaratif politikası.
- `Evidence/GamejamOrchestra/TLTP-*/`: bu oyunun staging, kaynak manifesti, derleme ve test kayıtları.

Teknik hedef Unity **6000.0.73f1**, **Built-In**, Windows ve çevrimdışı yerel oyundur. Gereksiz framework, renderer, controller veya paket göçü başlatma. Her görevde ilgili projenin yerel `AGENTS.md`, güncel sözleşmesi ve gerçek ayarlarını oku.

16 Ekim 2026 son kullanıcı açıklaması: **FPS geliştirmesi askıdadır; tamamen iptal edilmemiştir ve kullanıcı istediğinde yeniden dönülebilir.** Aktif üretim ve birleşik raporun kabul/asset planı yalnız izometrik oyundur; FPS uyarlaması veya testi izometrik teslimin bağımlılığı değildir. FPS kaynakları, mevcut build/paket, lisanslar, bağımsız kayıt kimliği, gerçek kayıtlar ve kanıtlar korunur; askıya alma temizleme/birleştirme yetkisi vermez. Dönüşte aktarılacak değişiklikler ayrıca seçilir. Güncel rapor `Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md`.

17 Ekim jüri süresi kararı: kullanıcı jüri üyelerinin **en fazla 5 dakika** oynayabileceğini bildirdi. Birleşik rapor Bölüm 5.1’de ayrı izometrik jüri senaryosu için **270 sn içerik + 30 sn yedek**, bir tam hazırlık–afet–sonuç döngüsü ve sel/yangında kısa oynanır örnekler planlanır. İş tekrarı/lojistik/öğrenme yükü düşürülür; ana kampanya kuralları, gerçek girdi, maliyet/iptal ve skor yalıtımı korunur. Bu bir plan kararıdır; uygulanmış veya insanla süre kabulü yapılmış sayılmaz. Önceki daha uzun demo önerisi geçersizdir; FPS askıda kalır.

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

