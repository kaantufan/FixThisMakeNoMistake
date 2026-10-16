# Too Late to Panic! — ekip ve teslim düzeni

Bu alan yalnız **Too Late to Panic!** içindir. Tema **Afete Hazırlık ve Önlem**; implementasyon ve asset üretimi kullanıcı tarafından onaylıdır. Hedef Unity **6000.0.73f1**, Built-In, çevrimdışı Windows oyunudur. Bu belge rol ve teslim sözleşmesidir; çalışan scheduler veya uygulamanın global model/system ayarı değildir.

## Aktif izometrik sürüm ve korunan FPS denemesi

- Aktif izometrik proje: [TooLateToPanic](../TooLateToPanic/AGENTS.md). Güncel geliştirme Home V4.4; [tasarım](TOO-LATE-TO-PANIC-GDD.md), proje talimatı ve [kapı sıkışması sözleşmesi](../Evidence/GamejamOrchestra/TLTP-DOOR-JAM-V44-20261016/CONTRACT.md) birlikte okunur. Build kabulü LATEST-BUILD.json içindedir; geliştirme hedefi kabul sonucu değildir.
- FPS projesinin geliştirmesi kullanıcının 16 Ekim son kararıyla durduruldu. Kaynak/build/kayıtlar korunur; yeniden açık istek olmadan FPS işi veya ajanı başlatılmaz. Tarihsel [FPS handoff](../Evidence/GamejamOrchestra/TLTP-FPS-FORK-20261016/FPS-HANDOFF.md) aktif üretim hedefi değildir.
- Her sohbetin root'u yalnız kendi Unity projesinin canlı kaynak, import, build, player ve test profili sahibidir. Aynı projenin iki eşzamanlı sahibi olamaz. Projeler gerçek dosyaları, Library/Temp/Logs/Builds ve skor/kayıt kimliklerini paylaşmaz; değişiklikler otomatik birleştirilmez.

Home V3; kısa ve kolay Türkçe, nesne üzerinden görsel toplama, E aç/kapat, F al/üret, Enter onayla/çantaya koy ve Space zıpla düzenini izler. İş sırasında Space iş eylemidir. Balkon, banyo, bodrum ve garaj, özenli ev/karakter, Türkçe radyo anonsları, kurtarma helikopteri, kompakt HUD ve sinematikler güncel kapsamdadır. Süre kaybı geri bildirimi başarısızlık sinematiğinden önce görünür. Ayrıntılı API ve koordinatlar görev sözleşmesinden alınır; bu özet tamamlanma iddiası değildir.

Her varyant ev/bahçe hazırlığı, kaynak toplama, 12 yuvalı envanter/8 yuvalı çanta, aktif işler, duvar/zemin sabitleme, eşya yerleştirme, deprem/sel/yangın, bunker/çanta/süre/hasar koşulları, hasar gezisi, kalıcı önlemler ve yerel highscore döngüsünü korur. Kontroller ve görsel sunum varyantın güncel yerel sözleşmesine göre uygulanır.

## Model ve kapasite

| İş | Model ve görev |
| --- | --- |
| Yönetim, içerik kararı, kritik inceleme, paket QA | `gpt-6.1-sol`, `ultra` |
| Model, doku, rig, klip, VFX/SFX, ışık, UI sanatı ve mini görsel QC | Standart `gpt-6.1-sol`; proje/görev gerekçelendirme ayarına uyulur |
| Onaylı içerik/kod bağlantısı ve dar optimizasyon | `gpt-6-luna`, `xhigh` |

Fast zorunlu değildir. Model/araç uygunluğu gerçek çağrı öncesinde doğrulanır; sessiz model ikamesi yapılmaz. Bu metin ana sohbetin model ayarını değiştirmez. Root dahil en fazla **4 eşzamanlı ajan**, en fazla **3 alt ajan** vardır. Alt ajan yeni ajan açmaz. Root bağımlılıklara göre üç slotu kaynak/eşya, ev, bahçe, afet, sunum, karakter ve QA işleri arasında çevirir; kategoriler eşzamanlı slot sözü değildir. Model/thinking override çağrıları `fork_turns: "none"` ve açık taşınmış bağlam kullanır.

## Görev ve dosya sahipliği

[policy.json](../Tools/GamejamOrchestra/policy.json) deklaratif kayıttır. Root görevde [common-3d.md](../Tools/GamejamOrchestra/Prompts/common-3d.md), ilgili uzman promptu, [teslim kuralı](../Tools/GamejamOrchestra/Prompts/delivery-and-documentation.md) ve doldurulmuş task sözleşmesini birlikte verir. Root merkezi task/batch kayıtlarının tek yazarıdır.

Her görev hedef varyant, görünür oyun sonucu, bağımlılıklar, tek dosya sahibi, `ownedTargetPaths`, başlangıç SHA256, benzersiz `stagingPath`, gerçek API ve kabul ölçütü taşır. Alt ajan yalnız atanmış staging alanına yazar:

`Evidence/GamejamOrchestra/<batchId>/assets/<category>/<assetId>/<revision>/`

`Evidence/GamejamOrchestra/<batchId>/integration/<taskId>/payload/`

Yeni belge/prompt görevi için root başka yazarı olmayan açık bir Tools/Docs alanını atayabilir. Bu istisna canlı oyun kaynaklarına veya Unity işlemlerine yetki vermez. Başlangıç hash'i değişmiş hedefe yazılmaz; `source_conflict` bildirilir. Mevcut Unity GUID'leri korunur. Root intake sonrası dahil kaynakları build/smoke boyunca dondurur; sonraki paketin bağımsız staging üretimi sürebilir.

## Üretim ve hafif kontrol

Sanatçı gerçek dosya, byte/SHA256, kullanım noktası, ölçek/pivot, bağımlılık ve kaynak/lisans kaydı teslim eder. Onaylı içerik Luna tarafından gerçek API'ye bağlanır; eksik model/klip/efekt/ses Luna tarafından üretilmez. Sağlayıcı önizlemesi indirilmiş dosya değildir. Aynı sağlayıcı işi task ID ile izlenir; tek browser sahibi korunur. Bu görev yetkisi yeni satın alma veya kredi harcaması içermez.

Her asset tek kısa mini QC'den geçer; belirgin kusur için bir hızlı revizyon, uzayan kusur için yönetici kararı kullanılır. Gerekli bileşen `required_missing`, uygulanmayan bileşen gerekçeli `not_applicable` kalır. Küçük mesh kusurlarında uzun onarım döngüsü kurulmaz.

Paketin tüm dahil işleri entegre edilip dondurulduktan sonra root **bir birleşik import/build ve tek 3–5 dakikalık etkilenen mekanik smoke** yapar. Uzman gözlemleri bu tek kanıtta birleşir; ayrı player/test zincirleri açılmaz. Somut hatada dar düzeltme ve gerekli tekrar yapılır. Geniş tur gün sonunda veya somut yeni riskte yapılır. Crash, veri kaybı ve temel akışı durduran hata açıkken kabul verilmez; çanta, bütçe, süre, hasar, collision veya kayıt koşulları testi geçirmek için gevşetilmez.

## Dokümantasyon ve son build

Her paket ilgili projenin Documentation, README ve tek güncel durum indeksinde tasarım değişikliği, yapılan işler, kontroller, asset/lisans, dosya sahibi, build/sahne/profil/hash, gerçek kontrol sonucu ve açık eksiklerle belgelenir. Kaynak hazırlığı, derleme, native kontrol, insan oynanışı ve son görsel kabul ayrı sonuçlardır. Eski kanıt yeni kaynağa aktarılmaz.

Yeni çıktı gerçekten açılıp kısa etkilenen kontrolü geçtikten sonra yalnız ilgili projenin `Builds/` altındaki eski build/portable ZIP adayları temizlenir. Her varyantta tek güncel çalışır çıktı, tek portable paket ve açık launcher/`LATEST-BUILD.json` tutulur. Windows recursive delete/move öncesinde çözülmüş absolute hedeflerin ilgili projenin Builds sınırında kaldığı doğrulanır; değişkenle geniş hedef silinmez. Kullanıcının açık oyuncusu kapatılmaz. Kaynaklar, lisanslar, gerçek kayıtlar ve küçük tarihsel metin/hash/receipt kanıtları korunur; gereksiz eski tam binary kopyaları biriktirilmez.

Tarihsel evidence klasör kimlikleri, manifest/hash/receipt içerikleri ve eski mutlak yollar kanıtın parçasıdır; toplu metin değiştirmeyle güncelmiş gibi yeniden yazılmaz. Güncel yollar yeni durum indeksinde gösterilir.

