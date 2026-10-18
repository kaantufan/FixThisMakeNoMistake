# Too Late to Panic! - CIVIL GAME JAM teslim beyanı

18 Ekim 2026. **Güncel yerel aday: Home V5.14-r3 /0.5.14; GitHub güncellemesi hazırlanıyor.** Yarışma formu gönderilmedi. Temsilci, üretim zamanı ve kaynak izinlerine ilişkin açık alanlar ayrıca doğrulanmalıdır.

Güncel teslim dayanağı, kullanıcının **18 Ekim 2026'da doğrudan ilettiği resmî organizasyon duyurusu** ve [etkinlik sitesi](https://game.ktimo.com.tr/)'dir. Duyuruya göre teslim **18 Ekim 12.00-18.15 UTC+3**, takım PIN'iyle yapılır. Video **en fazla 1 dakika**, oyun görüntüsü veya fragman olabilir; **MP4/AVI** olarak oynanabilir **ZIP'ten ayrı yüklenir**. Toplam sahne süresi **video dahil 3 dakika**; sunum performansı puanlanmaz, jüri oyunu izler ve oynar. Public Git kaynak reposu gerekir; web bağlantısı yalnız web build içindir. AI'nin görsel/ses/kod/metin/diğer kategorileri ayrı Kullanıldı/Kullanılmadı seçimi ve ayrıntıyla bildirilir; hazır kaynaklar ayrıca beyan edilir. Yüklemelerden sonra doğrulama ve makbuz kontrol edilir.

Resmî v05 PDF de incelendi: SHA256 `D8F6F54137C0EE9A4ADFEC9BBF15206F10C8DD5DF536560752D18FFE86DA761B`. PDF'nin **en az 1 dakika video /5 dakika sunum /18.00 kapanış** bilgileri güncel duyuruyla çelişir ve bu teslim hazırlığında uygulanmaz. Değişmeyen kaynak/lisans/AI/commit maddeleri için aşağıdaki sayfa numaraları tarihsel başvurudur; güncel form ve duyurunun yerine geçmez. İlk iletilen sekiz sayfalık Murat Oktay konuşması kural kitabı değildir.

## Oyun adı ve kısa açıklama

**Too Late to Panic!**

> Afet gelmeden önce evini hazırladığın bir gerilim ve kaynak yönetimi oyunu. KKTC, Güzelyurt, Kalkanlı'daki aile evinde sınırlı zaman ve bütçeyle mobilyaları sabitle, tesisatı güvene al ve yapıyı güçlendir. Son 40 saniyede aynı evin içinden birinci şahıs bakışla su ve gıda topla; afete uygun güvenli odaya yetiş. Aldığın önlemlerin sonucunu aynı ev ve eşyalar üzerinde gör.

Yer gerçektir; aile, kişiler ve afet olayları kurmacadır. Oyun, fiziksel afet eğitimi veya gerçek bina güvenlik değerlendirmesi olarak doğrulanmış değildir.

## Deneyimin ayırt edici yönü

Oyuncu önce evi yukarıdan okuyup hangi riske para ve zaman ayıracağını seçer; ardından kendi hazırladığı evin içinde erzak toplar. Aynı nesnenin önlemden önceki ve afet sonrasındaki durumu kararın sonucunu gösterir. Sabitleme, tesisat ve kaynak yönetimi bu iki bakış açısını birbirine bağlar. Bu tasarım açıklamasıdır; özgünlük derecesi veya insanlarda ölçülmüş öğrenme etkisi iddiası değildir.

## Platform, kurulum ve ilk adım

- Platform: Windows x86-64; çevrimdışı tek oyuncu. Kaynak proje Unity **6000.0.73f1**, Built-In renderer kullanır. Windows 10/11 hedeflenir; minimum donanım ve farklı bilgisayarlarda uyumluluk ölçümü yapılmamıştır.
- Yerel paket: `TooLateToPanic/Builds/TooLateToPanic-Home-V5.13-r7-20261018-portable.zip`. ZIP'in tamamını bir klasöre çıkarın. `TooLateToPanic.exe` dosyasını yanındaki `TooLateToPanic_Data`, `MonoBleedingEdge` ve diğer dosyalarla birlikte tutarak çalıştırın. Ayrı Unity kurulumu gerekmez.
- İlk adım: ana menüden **Oyna** normal kampanyayı, **Kısa Oyun** kısa akışı, **Afeti Dene** simülasyonu açar; **Nasıl Oynanır** ve **F1** kontrolleri açıklar. **J** gerçek stok ve önlem durumunu gösteren defteri açar.
- Normal dalga: **90 sn önlem + 40 sn erzak/FPS =130 sn**, başlangıç bütçesi **1500**. Her dalgada en az **1 su +1 gıda** gerekir. Sel varsa üst panik odasına, diğer afetlerde bodrum sığınağına gerçek geçiş gerekir. **%75 ve üzeri hasar** kayıptır. İlk dalga Deprem, ikinci dalga Sel+Fırtına, daha sonraki dalgalar RNG'dir.
- Temel kontroller: WASD/oklar hareket, Shift koşu, Space zıplama; E ana etkileşim/aç-kapat, F seçili malzeme/erzak toplama; fare nesne seçimi ve FPS bakış; Tab envanter, J defter, F1 rehber, Esc duraklat/geri. El işlerinde ekranda gösterilen gerçek fare/SPACE/ok girdilerini kullanın; Esc ücretsiz iptal eder. F2 geliştirici seçeneklerinin kullanımı koşuyu skor dışı bırakır.

## Teslim sürümü ve yayın sınırı

[LATEST](../TooLateToPanic/LATEST-BUILD.json) ve [V5.14-r3 teslim kaydı](../TooLateToPanic/Documentation/HOME-V5.14-PANIC-ROOM-DELIVERY-2026-10-18.md) güncel yerel kimliktir.

| Alan | Güncel değer / açık durum |
|---|---|
| EXE | `TooLateToPanic/Builds/Home-V5.14-r3-20261018/TooLateToPanic.exe` |
| DLL SHA256 | `EE4581BCADE7FF44E3ACDA394391E561536761ACFAF0A365F4BEA5B6AA19A69D` |
| ZIP SHA256 / bayt | `E97C68B5BB9989776889BEFE473981C9C1B4A9EF2BE724CB8F85EA3B768EF6F7` / 215427119 |
| Public kaynak / oynanabilir paket | Yeni yayın hazırlanıyor; [yayın kaydı](../PUBLICATION.md) kaynak commit'i ve paket kimliğini ayırır |
| Önceki yayın | v0.5.13-r7 ve release/tag/asset'ler, stable v0.4.13 korunur |
| Teknik sonuç | Yeni Editor kontrolü NOT_RUN; 122 aynı DLL native API/render kontrolü, 15 incelenmiş kare. OS girdisi/insan oynanış, okunurluk, dinleme, konfor ve tam kampanya kabulü NOT_RUN. |
| Video /180s prova | Ayrı video önceki R7 paketiyle bağlıdır; bu DLL için video/prova kabulü NOT_RUN |
| Yarışma ZIP yüklemesi / makbuz | YAPILMADI |

OpenAI Codex ile üst panik odasının duvarındaki büyük "Panik odasını kilitle" butonu, üstünde yanıp sönen kırmızı alarm, gerçek tuş istemi, ilgili kod, Türkçe metin ve kontroller düzenlendi. Özgün derinlik testli alfa font shader ve Resources/HUD materyali ile beyaz yazı kontrastı düzeltildi. Yeni dış görsel/model/shader/ses kaydı, sağlayıcı kredisi veya ödeme yoktur. Mevcut asset/lisans/provenans kayıtları korunur. Üst panik odasının duvarındaki büyük "Panik odasını kilitle" butonu ve üstünde yanıp sönen kırmızı alarm gerçek E etkileşimini belirginleştirir. Önceki AI kategorileri ve hazır kaynak lisans/provenansı aşağıdaki tablolarda korunur.

## Hazır kaynak ve üretim beyanı

AI beyanı aşağıdaki lisans tablosundan ayrıdır. Yerel üretim, kendiliğinden CC0/MIT lisansı veya bütün ekip adına yayın izni anlamına gelmez. Sağlayıcı çıktısının oyuna eklenebilmesi, ham dosyasının public repoda yeniden dağıtılabilmesiyle ayrı değerlendirilir (v05 s.3 madde 3.3-3.6; s.7 madde 9.7).

| Kaynak / sağlayıcı | Kaynak ve lisans bağlantısı | Kullanım ve değişiklik | Kanıt / açık sınır |
|---|---|---|---|
| Quaternius Universal Base Characters Standard ve Universal Animation Library Standard | [Quaternius](https://quaternius.com/); [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Oyuncu/NPC/kurtarma humanoid tabanı ve mevcut Idle/Walk/Jog/Jump/Interact/Fixing klipleri; yerel sivil kıyafet/renk uyarlamaları | [Oyuncu provenansı](../TooLateToPanic/Documentation/Existing-Player-Provenance.md), `Assets/Game/Resources/Licenses/Quaternius-Base.txt` ve `Quaternius-Animation.txt`. CC0 yalnız ilgili dış kaynaklara aittir. |
| Oswald, Barlow, Barlow Condensed / Google Fonts proje yazarları | [Oswald](https://github.com/google/fonts/tree/main/ofl/oswald), [Barlow](https://github.com/google/fonts/tree/main/ofl/barlow), [Barlow Condensed](https://github.com/google/fonts/tree/main/ofl/barlowcondensed); SIL OFL1.1 | Çevrimdışı HUD/menü/defter fontları; Oswald sabit ağırlığa dönüştürüldü | [HUD lisans kaydı ve tam metinler](../TooLateToPanic/Documentation/Licenses/HUD/README.md). |
| ElevenLabs Sound Effects ve Music | [ElevenLabs](https://elevenlabs.io/), [Creative Terms](https://elevenlabs.io/creative-terms), [Music Terms](https://elevenlabs.io/music-terms) | Foley, afet/araç/tesisat/arayüz efektleri; sözsüz bit, nefes; preparation/fps_tension/disaster müzikleri. Format, süre ve ses düzeyi düzenlemeleri | [V5.9 ses kaydı](../TooLateToPanic/Documentation/HOME-V5.9-AUDIO-ASSETS-2026-10-18.md) ve `Assets/Game/Resources/Audio/{Foley,Music,Jury}/*PROVENANCE.md`. Üretim kayıtları Creator/mevcut kredi kullanımını bildirir. **Ham seslerin public yeniden dağıtım kapsamı son paket için ayrıca kontrol edilecek; CC0/MIT değildir.** |
| Yarkın eşya/ikon/atlas/materyal ve mekanizma kaynakları | Repo dalları `yarkin-assets`; seçili kaynak commit'leri `eb05475ee0533b86031b36f156f6022521118a15`, `11f12b4cf1f6bf676dcf3dc5055e1b66c8b1ff57`, `29bf309fa0c7ccfdafd0a935e93512a44f550c9e` | `Assets/Vendor/Yarkin`; mevcut oynanışa uyarlanan seçili kaynaklar, bütün dalın merge'i değildir | [Upstream beyanı](../TooLateToPanic/Assets/Vendor/Yarkin/Documentation/UPSTREAM-ASSET-MANIFEST.md), [asset kredileri](../TooLateToPanic/Documentation/ASSET-CREDITS.md), `Resources/Licenses/Yarkin-V414-SuppliedDeclaration.txt`. Beyan farklı tarihlerde 31/32 sayıları içerir; final paketin dosya manifesti esas alınmalı. Takım üyeliği/üretim zamanı/hak sahibi izni ayrıca doğrulanacak. |
| Defter HTML tasarımı | `defter-mockup`, `07ecbccb097ea796067afd8a35af615563857d43`; sonraki `00377265c740fdc973039d0266cefc8c913633b9` kaydı ayrıca karakter referansı içerir | J defterinin yerel Unity uyarlaması; gerçek Model/Expedition verileri | [Asset kredileri](../TooLateToPanic/Documentation/ASSET-CREDITS.md). Tasarımın araç ve hak sahibi beyanı tamamlanacak. |
| Meshy karakter referansı / kaynak arşivi | Kaynak commit `00377265c740fdc973039d0266cefc8c913633b9`; sağlayıcı [Meshy](https://www.meshy.ai/) | Görünüm referansı; aktif oyuncu/NPC retained Quaternius humanoid tabanıdır | [V4.14 kaynak notu](../TooLateToPanic/Documentation/HOME-V4.14-ASSETS-2026-10-17.md). **Üreten kişi, plan ve kamuya yeniden dağıtım izni doğrulanmadı.** Atıf eklemek tek başına izin açığını kapatmaz; final public dosya listesi ayrıca incelenecek. |
| Yerel kod/sanat/metin/efektler | Bu proje; `Assets/Game/Runtime`, `Assets/Game/Editor`, `Assets/Game/Art` | Ev, önlem işleri, FPS elleri, araç/afet görselleri, HUD/defter, logo ve oyun sistemleri | [Asset kredileri](../TooLateToPanic/Documentation/ASSET-CREDITS.md). AI destekli üretim aşağıda açıklanır; bu belge yeni genel lisans atamaz. |
| Unity motoru ve yerleşik modüller | [Unity](https://unity.com/); `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json` | 6000.0.73f1 Built-In; Animation/Audio/IMGUI/Physics vb. motor modülleri | Üçüncü taraf motorun koşulları korunur; proje kodunun lisansı gibi gösterilmez. |

Yedi eski ElevenLabs TTS konuşması `Assets/Game/Deferred/Spoken-V47`, eski Microsoft Tolga radyo WAV'ları `Assets/Game/Deferred/Radio-legacy` altında tarihsel kaynaktır. Güncel sözsüz router bunları çağırmaz; eski “yedi konuşma aktif” beyanı kullanılmamalıdır. Final public repo/ZIP listesinde bulunup bulunmadıkları manifestle kontrol edilir; runtime dışında tutulmaları kendi başına yayın izni sağlamaz. Tolga çıktılarına doğrulanmamış dağıtım izni atfedilmez.

## AI beyanı

| Araç | Amaç | Kullanım yeri / kanıt |
|---|---|---|
| OpenAI Codex ajanları | Kullanıcı yönlendirmesiyle oyun ve prosedürel sanat kodu, Unity araçları, HUD/defter, test otomasyonu, araştırma ve belgeler | `TooLateToPanic/Assets/Game/{Runtime,Editor,Art}`, `Documentation`, `Docs`, `Tools`; [ekip kaydı](TOO-LATE-TO-PANIC-TEAM.md). Model aileleri iş kayıtlarında GPT-6.1 Sol/GPT-6 Luna olarak geçer; tüm geçmiş araç/model listesini temsilci doğrulamalıdır. |
| Antigravity AI Agent | Blender/Unity üretim scriptleri, Yarkın eşya/ikon/materyal ve metinleri | `Assets/Vendor/Yarkin`; korunmuş upstream AI beyanı. Kaynak dalının bütün scriptleri runtime'a alınmış sayılmaz. |
| ElevenLabs Sound Effects ve Music | Efekt, sözsüz bit/nefes ve enstrümantal müzik üretimi | `Assets/Game/Resources/Audio`; yukarıdaki provenance kayıtları. V5.9'da 11 kaynak ailesinden 21 runtime WAV, 457 mevcut kredi kaydı vardır; yeni abonelik/ödeme/aşım yoktur. |
| ElevenLabs TTS, Eleven Multilingual v2, hazır Mia sesi | V4.7 kısa akışın tarihsel yedi Türkçe konuşması; özel klon yok | Güncel runtime dışında `Deferred/Spoken-V47`; dağıtım listesine giriyorsa kaynak/AI beyanında ayrıca yer almalı. |
| Meshy AI | Karakter görünüm referansı modeli | Korunan kaynak arşivi; aktif rig/klip değildir. Üreten kişi/plan/dağıtım izni açık. |
| Anthropic Claude / Claude Code | Önceki dış araştırma ve ilk teslim beyanı taslağı | `Docs/TOO-LATE-TO-PANIC-ELESTIREL-ARASTIRMA-2026-10-17.md`, arşivlenmiş 17 Ekim teslim taslağı. Eski belgenin araç beyanı korunur; bu bakım görevi Codex ile yapılmıştır. |
| HUD HTML konsepti / ek takım araçları | Tasarım referansları | `Documentation/HUD-Concept` ve defter upstream'i: **araç/üreten kişi teyidi bekliyor**. Bilinmeyen araç için “AI kullanılmadı” seçilmez. |

### Formdaki beş AI kategorisi

Güncel duyuru en küçük kullanımı da ayrıntılı beyan ister. Aşağıdaki **beş kategoride Kullanıldı** seçimi, eldeki üretim kayıtlarına dayanır. Temsilci ek araçları ve final paketin kapsamını tamamlamalıdır; kaynak arşivi ile aktif oyunun kullanım yeri ayrı yazılmalıdır.

| Kategori | Seçim | Araca, amaca ve kullanım yerine ilişkin forma hazır ayrıntı |
|---|---|---|
| Görsel | **Kullanıldı** | Codex ile ev/eşya/araç/afet görselleri, HUD ve FPS elleri için prosedürel Unity kodu yazıldı. Yarkın eşya/ikon/materyal scriptlerinde Antigravity AI Agent kullanımı upstream'de kayıtlıdır (`Assets/Vendor/Yarkin`). Meshy karakter modeli görünüm referansı olarak kaynak geçmişindedir; aktif rig retained Quaternius'tur. HUD HTML konseptinin araç/üreten kişi teyidi açıktır. |
| Ses | **Kullanıldı** | ElevenLabs Sound Effects ve Music ile efekt, sözsüz bit/nefes ve enstrümantal fonlar üretildi (`Assets/Game/Resources/Audio`). V4.7'nin hazır Mia/Multilingual v2 TTS konuşmaları güncel runtime dışında `Deferred/Spoken-V47` içindedir. Güncel V5.9'da anlaşılır yeni konuşma veya ses klonu üretilmedi. |
| Kod | **Kullanıldı** | Kullanıcı yönlendirmesiyle Codex ajanları runtime, prosedürel sanat, Editor/build araçları ve otomatik test kodunu yazmak/düzeltmek için kullanıldı. Antigravity'nin Yarkın üretim scriptleri kaynak provenansında kayıtlıdır; bütün upstream oyun kodu runtime'a alınmadı. |
| Metin | **Kullanıldı** | Codex ile Türkçe oyun/defter/arayüz metinleri, altyazılar ve belgeler düzenlendi. Yarkın eşya metinlerinde Antigravity; önceki eleştirel araştırma ve ilk teslim taslağında Claude Code kullanımı kayıtlıdır. Yer: `Runtime`, `Documentation`, `Docs`, vendor beyanları. |
| Diğer | **Kullanıldı** | Codex ajanları tasarım seçenekleri, test otomasyonu/Computer Use, kaynak-hash incelemesi ve depo/belge düzenlemesi için kullanıldı. Bu otomasyon insan oynanış veya dinleme kabulü değildir. |

Forma kopyalanabilecek genel özet (beş ayrı kategori alanının yerine geçmez; açık araç alanları tamamlandıktan sonra):

> Kod, prosedürel oyun görselleri, arayüz, araçlar ve testler kullanıcı yönlendirmesiyle OpenAI Codex ajanları kullanılarak geliştirildi. Yarkın kaynaklarının model/ikon/materyal üretim scriptlerinde Antigravity AI Agent kullanıldı. Ses efektleri ve enstrümantal müzikler ElevenLabs ile üretildi; güncel sürüm anlaşılır konuşma yerine sözsüz bit ve altyazı kullanır. Meshy karakter modeli görünüm referansı olarak kaynak geçmişinde yer alır. Önceki araştırma/teslim taslağında Claude Code kullanımı kayıtlıdır. Hazır kaynaklar, AI kullanım yerleri ve dağıtım sınırları bu belgede ayrı açıklanmıştır.

## Teknik kanıt ve kalan sınırlar

R7'nin güncel indeksi **472/472 gerçek headless Unity Editor kontrolü**, **524/524 native UI/yerel parlama kontrolü**, **69 framebuffer görüntüsü** ve sıfır runtime hatası bildirir. Bütün sağlanan görüntüler inceleme kaydına bağlıdır. Bunlar otomatik API/render ve fixture kanıtıdır. Fiziksel OS girdisi; insan oynanışı, okunurluk, dinleme/konfor; fiziksel 6-7 dalga kampanya kabulü **NOT_RUN**. R5'in 943 sonucu yeni DLL'e aktarılmaz. R7'de ayrı V56/audio suite'ları tekrarlanmadı.

Kesin takım, temsilci, katılım onayı ve oyuna özgü çalışmaların resmî 48 saat içinde üretildiği bu belgeyle doğrulanmadı. Özellikle 16 Ekim 18.00 UTC+3 öncesi kayıtlı TLTP üretimi varsa genel araç/izinli hazır kaynak ile oyuna özgü içerik ayrımı yapılmalı; organizasyonun proje özelindeki kararı varsayılmamalıdır (v05 s.2 madde 1.3 ve 2; s.3 madde 3.1).

Son kontroller [JAM-RULES-CHECKLIST.md](JAM-RULES-CHECKLIST.md), üç dakikalık prova metni [JAM-3-DAKIKA-SUNUM.md](JAM-3-DAKIKA-SUNUM.md)'dedir. **Henüz yarışmaya hiçbir şey gönderilmedi.** Bu belge hak sahipleri adına kabul vermez ve kendi başına GitHub yayını kanıtı değildir.
