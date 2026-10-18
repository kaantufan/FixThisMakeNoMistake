> Public publication copy: the selected source manifest differs in scope from the full local build manifest. Native results below describe the recorded package only. OS testing state is a snapshot and may still be in progress. [Download](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.5.7-r4), [source/build comparison](Publications/Snapshots/20261018T095013Z/source-vs-build.json). Local-only historical evidence links remain local references.

# Home V5.7 — menü, HUD ve birleşik teslim

18 Ekim 2026. **Home V5.7-r4 /0.5.7**. Kaynak, build ve native görsel kontrolleri tamamlandı; dört afetin Windows turu fiziksel Esc nedeniyle eksik, kullanıcı testine hazır aday. Ana menü, duraklatma ve ses sayfası; mat koyu zemin/kırık beyaz kâğıt, sınırlı sıcak vurgu, görünür odak ve metinle desteklenen33 özgün çizgi simgeyle yenilendi. Süre, bütçe, hasar, su/gıda ve gerçek seçili hedef maliyetleri aynı görsel dili kullanır. Yeni sağlayıcı işi, ücret veya kredi kullanımı yoktur.

Başlatıcı: [Play-TooLateToPanic.cmd](../Play-TooLateToPanic.cmd). Açık PID **57064** izole Windows test/gözlemci penceresidir; normal global profil değildir. Fiziksel Esc ile durdurma sonrasında yeni girdi, başlatma veya kapatma yapılmadı. Yeni normal profil açılışı **NOT_RUN_USER_STOP** kaldı. Gerçek skor ve varsayılan ses tercihleri önce/sonra hashleri aynıdır. [Korunan süreç kaydı](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/current-player-preserved.json>)

## Birleşik davranış

- F1, kullanıcının son düzeltmesiyle eski tek fiziksel klavyeyi korur:56 kapak,34 sürekli vurgulanan kullanılan tuş ve22 dış açıklama grubu.106 ortogonal parça açıkça seçilmiş temsilci tuşları açıklama kartlarına bağlar. Önceki karo tasarımı reddedilmiştir; onun sonuçları yeni kabul diye kullanılmaz.
- Envanter ve afet çantası tek20yuvalı liste ve5×4 ızgaradır. Su/gıda toplandığında hazırdır; her gerçek dalga çözümü aynı stoktan birer birim tüketir. Elle paketle/geri al adımı kalktı. Destek türü başına katkı bir bittir; fener/radyo ilk etkinleştirmede birer pil kullanır. Fazla eşya gerçek yuvalarda kalır; dolu/kısmi toplama ve tekrar tüketim kontrolleri uygulanır.
- FPS hazırlık kamerası normal fare/hareket girdisi ve2sn yumuşak geçişi korur; ilave prosedürel yaw/pitch/roll/FOV salınımı yoktur. Panik mevcut nefes/kalp/gerilim sesi ve sınırlı yumuşak kenar kararmasıyla sürer; azaltılmış ayar korunur.
- Defterin manuel hedef işareti yakın etkileşim seçiminden ayrıdır. Yardımcı kapalıyken de mevcut küçük kartta hedef/mesafe/yol bilgisi gösterir; uzaktan etkileşim, otomatik yürüme veya kaldırılmış turkuaz dünya işareti üretmez.
- V5.6 ses, tesisat, geçiş, bütçe/hasar ve fizik düzeltmeleriyle seçili Yarkın hortum/dış deprem/yangın/detay paketi aynı adayda bulunur. Ana karakter/retained animasyon, gerçek ev/çatı/collider kökü ve ses sahipleri korunur.

120sn önlem +45sn erzak/FPS,75% kayıp, gerçek maliyet/ücretsiz iptal, sonlu stok/dalga RNG'si, fiziksel sığınak ve skor kuralları korunur. Selde üst panik odası; diğer afetlerde bodrum kullanılır. Ayrı eski FPS projesi ve gerçek kayıtlar değiştirilmedi.

## Yeni DLL kanıtı

981/981 aynı DLL üzerinde izole native/model/API/framebuffer gözlemi;0 runtime hatası, toplam81 gerçek PNG. Tablo farklı fixture katmanlarını toplar; tüm kampanya veya insan kabulü değildir.

| Suite | PASS kontrol | PNG | Süre | Kesin rapor |
|---|---:|---:|---:|---|
| ui-v57 | 474 | 54 | 24.45sn | [JSON](<Publications/Snapshots/20261018T095013Z/native-summary.json>) |
| yarkin-hybrid | 75 | 9 | 35.64sn | [JSON](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-hybrid-6ddcd34c-407e-4e24-940a-7df8056e6f16/hybrid-report.json>) |
| v56-regression | 151 | 10 | 41.73sn | [JSON](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-v56-574175bd-8aff-4e99-80f6-37660e7f6c58/v56-smoke.json>) |
| wave-ledger-v56-regression | 281 | 8 | 13.50sn | [JSON](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-ledger-ca1fb961-b7c8-4c30-b190-e783e38974dd/wave-ledger-v56-smoke.json>) |

UI kontrolü1600×900 ve960×540 gerçek font ölçümlerini, uzun skorları, menü/ses geri çağrılarını, F1 geometri/süre sahipliğini, defteri ve dolu20yuva görüntüsünü içerir.42 tek envanter model kontrolü bağımsız modeller ve nested izole profil fixture'larıdır; aktif World için ayrıca gerçek sinematik sonucu stoktan birer tüketim ve tekrar-sync idempotence gözlenir. Konum/saat/sayaç/memoize olay fikstürleri raporlarda tek tek açıklanır. Bu işlemler OS girişi veya doğal fiziksel rota kabulü değildir.

Tarihsel V5.6'nın143 ve ledger245 kontrol kimlikleri yeni raporlarda sıralı korunur. İki eski dünya stok/paketleme fixture'ı yeni kanonik sonlu toplama sözleşmesine açıkça uyarlanmıştır; model-only ledger sınırları değiştirilmez. Eski DLL'nin PASS sayısı yeni DLL yerine sunulmaz. [QA kaynak ve oracle bağları](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/qa-extension-bindings.json>).

Gerçek PNG'ler ajan tarafından incelendi; [görsel inceleme](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/visual-review-r4.json>) aynı DLL'ye bağlıdır. [Fiziksel Windows girdi kaydı](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/os-review-r4.json>) yeni izole oyuncu PID'siyle sınırlıdır; ham gözlem/PCM kaynakları o kayıtta belirtilir. Gerçek Windows girdisiyle yardımcı kapalıyken mutfak hedefi işaretleme/koruma/kaldırma, F1 duraklat/devam, Tab tek envanter, doğal FPS geçişi ve27,86sn sabit konum gözlemi yapıldı. Ses karşılaştırmasında daha sonra yalnız izole skor dışı koşuda F2 saat dondurma kullanıldı. F3 gerçek sığınağa ulaşılmadan ödülsüz dökümle çözüldü. Dört afetin menüden Windows turu ve yeni normal profil açılışı fiziksel Esc nedeniyle tamamlanmadı; reddedilen son tıklama yalnız deneme kaydıdır. Karma masaüstü loopback veya PID ile ilişkili oturum peak ölçeri tek başına bu sürece yalıtılmış PCM yakalama değildir. Üretim UI durumu, native kaynak/Listener kazançları, ilişkilendirilmiş oturum ölçeri ve insan dinlemesi ayrı tutulur. İnsan dinleme/beğeni/okunurluk/konfor ve tüm kampanya kabulü **yapılmadı**.

## Kimlik ve sınırlar

| Öğe | SHA256 |
|---|---|
| Assembly-CSharp.dll | `BA49CD61AE45BB29CA68AF29EEEEB8BDED9574C8D7B5B2D48B3EC477D64FD394` |
| EXE | `D26B0F57817DB378557C445524C48D28EE559FB2BE53CA5DC63E37C1E4AD22F5` |
| Taşınabilir ZIP | `E853DE2D63ED843E963777A3F61046047A98D7E06CC5C2603D8CBC2D041EB12C` |
| Kaynak manifesti | `C7383D8694C3BCFDDDD14B5B7EA10074BC69C95FA5AFCF0DD69C47F159F15DA7` |
| Build manifesti | `ED067508FAA958E8A71641A0720D0255A8ADB52D1A412FE54F74287C61B66515` |

191 C#/shader kaynağı dondurulmuştur; kayma0. ZIP'in bütün dosyaları build manifestiyle byte/hash eşleşmesi gösterir. [Paket receipt](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/package-r4.json>). Güncel yürütülebilir ve ZIP yolları [LATEST](../LATEST-BUILD.json) içindedir. V5.5 önceki teslimi ve tamamlanmamış V5.6 kaynak/native başlangıcı ayrı saklanır. Açık eski oyuncuya girdi/sonlandırma gönderilmedi; bu teslim GitHub yayını iddia etmez.

Eski zemin üçgeni karşılaştırmada yeniden üretilemediği için kesin nedensel onarım UNVERIFIED kalır.960×540'ta küçük yardımcı/kroki yazıları daha büyük pencerede daha rahat okunur. Yangın dış çekiminin bazı açılarında alevler çatı arkasında kalabilir. Tüm12decal/eşya konumunun ayrı görsel insan kabulü verilmedi.

Yeni normal açılış durdurma nedeniyle yapılmadığı için yalnız bu root'un eski R1/R2/R3 adaylarını temizleme adımı da çalıştırılmadı. Önceki çıktılar, küçük kaynak/hash/test kayıtları ve tarihsel reddedilmiş hedefler korundu; [temizlik durumu](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/cleanup-status-r4.json>) ayrı kayıtlıdır. Güncel launcher ve paket kimliği yalnız R4'e bağlıdır.

[Araştırma ve uygulama eşlemesi](HOME-V5.7-UI-RESEARCH-2026-10-18.md) · [Özgün simge/font/asset/lisans kaydı](HOME-V5.7-UI-ASSETS-2026-10-18.md).
