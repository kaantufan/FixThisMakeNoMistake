> Public publication copy: the selected source manifest differs in scope from the full local build manifest. Native results below describe the recorded package only. OS testing state is a snapshot and may still be in progress. [Download](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.12-r4), [source/build comparison](Publications/v0.4.12-r4/source-vs-build.json). Local-only historical evidence links remain local references.

# Home V4.12-r4 — yüzey vurgusu, pickup parıltısı ve 2 dakika hazırlık

17 Ekim 2026. Unity 6000.0.73f1, Built-In, Windows64, sürüm0.4.12. Güncel çıktı `Builds/Home-V4.12-r4-20261017/TooLateToPanic.exe`; başlatıcı ve LATEST bu adayı gösterir. V4.13 numarası sonraki logo teslimine ayrılmıştır.

Normal kampanyanın her dalgasında 120 saniye güçlendirme ve ardından30 saniye FPS afet çantası vardır: toplam150 saniye. HUD ilk aşamada2:00, sonra ayrı FPS ekranında30sn gösterir. Pause ve ilerleme çubuğu aynı aşama saatini kullanır. Ayrı kısa akış, öğretici ve açık afet simülasyonu kendi sürelerini korur. Kayıp sınırı75, gerçek stok, maliyet, iptal ve skor kuralları değişmedi.

Seçili gerçek meshlerin görünür yüzeyine yaklaşık yüzde20 turuncu/altın vurgu ve ince siluet uygulanır. Nesnenin dışındaki boş alanı kaplayan tel kutu ve ayrı yerleştirme zemin çizgisi kaldırıldı. Duvar seçimi en yakın gerçek authored segmente bağlanır. Gerçek derinlik, materyal alfa kesimi ve kat görünürlüğü korunur. Geçerli/geçersiz taşınan eşyanın kendi ghost yüzeyi yeşil/kırmızı görünür.

Yalnız son çanta aşamasında görünür ve stoklu gerçek WorldSupply meshleri küçük, aralıklı sıcak beyaz yüzey parıltısı alır. Yaklaşık3.6sn aralıklı tarama ve küçük çekirdek kullanılır; sahneye ışık, sprite veya büyük beacon eklenmedi. Toplama veya faz değişimi mevcut kaynak sahipliğinden gelir. Önlem malzemeleri ve dolaplar bu parıltıyı almaz.

## Kaynak, derleme ve native katmanı

DLL SHA256: `F5C34832B432FD4583E77CAC099F3B53CD403F52CD924FDD180C23B176B01AEE`.

Derleme0hata/0uyarı. Son görünür native test **223/223 PASS,0runtime hata,24.31sn**:91 model ve132 dünya/işlem/frame/render kontrolü.15konum/saat/hazır fikstürü kullanılır; OS veya insan kabulü değildir. [Son native receipt](Publications/v0.4.12-r4/native-summary.json).

R3'te ilk test penceresi gizli başlatıldığı için bazı kareler çizilmedi ve test reddedildi. Aynı R3 DLL görünür pencerede207/207 geçti; yeni süreyi içeren R4 sonucu bu ayrı223kontroldür. Eski başarısız kanıt silinmedi ve son sonuca eklenmedi. Gerçek FPS su şişesi ve seçili pencere karelerinde boş kutu yerine gerçek asset yüzeyi gözlendi. Parıltının zaman içindeki okunurluğu/tadı insan kabulü değildir.

Kaynak manifesti34543dosya: [r4-source-manifest.json](Publications/v0.4.12-r4/source-sha256.json). ZIP SHA256 `0F652F68AF06263D05CDCDC8BFFCA0F22671332E19F99B00D35B3FC20883E750`;184817461bayt. ZIP içindeki DLL son native DLL ile eşleşti. [Portable receipt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/silhouette-intake/r4-portable-receipt.json). Yeni ücret/sağlayıcı/kredi kullanımı yok; mevcut asset ve lisans kaynakları korundu.

## Gerçek Windows girdisi

Kullanıcı SendInput ile basılı tuş ve fareyi açıkça yetkilendirdi. Gerçek hareket, E/F, tıklama, kamera ve normal pause kullanılır; hile, teleport veya doğrudan Model mutasyonu kullanılmaz. R4 normal oyuncusu PID64564, yalnız skor dosyası izole; smoke/test sürücüsü yoktur.

Önceki R2 turunda kitaplık iki gerçek vida ile sabitlendi:1000→940bütçe,2vida ve1ankraj tüketimi. Yapım kaynakları alındı, gerçek merdivenler yüründü ve son30 FPS'de su/gıda1/1toplandı. Sığınak kapağına gelindi, fakat E etkileşimi tamamlanmadan süre doldu; başarılı tur sayılmadı. [R2 gerçek girdi receipt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/os-playtest/sendinput-fullround-20261017/RECEIPT.json).

R4 ilk denemesinde gözlenen oyun durumu, gönderilen girdilerin dışında yeniden başlamış/yer değiştirmiş göründü; kullanıcı kontrolü netleştirdi ve testi tamamlamayı root'a bıraktı. Son temiz R4 turu halen gerçek girdilerle yürütülüyor. Tam tur/sığınak başarısı henüz bu belgeyle ilan edilmez.

Gerçek OS turu tam75sınırını, tüm ağaç sıralarını veya tüm afet çatı çekimlerini kapsamaz. Bunların native fikstür kontrolü ayrı katmandır. Normal oyunda mutfak pencere seçimi ve iç kapı/korkuluk rotasının okunurluğu önceki R2 denemesinde zorlayıcı bulundu; yeni kozmetik/timing değişikliği bunları ayrıca çözmüş iddiası taşımaz.
