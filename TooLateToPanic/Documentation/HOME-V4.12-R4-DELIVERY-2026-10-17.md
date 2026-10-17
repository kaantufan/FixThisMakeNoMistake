> Source documentation update. The current promoted playable package remains [Home V4.12-r4](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.12-r4). R5 is a separate candidate until its own LATEST/build promotion and publication. Local Evidence links below refer to the original development archive; this documentation push runs no new build/native/OS tests.

# Home V4.12-r4 — yüzey vurgusu, pickup parıltısı ve 2 dakika hazırlık

17 Ekim 2026. Unity 6000.0.73f1, Built-In, Windows64, sürüm0.4.12. Güncel çıktı `Builds/Home-V4.12-r4-20261017/TooLateToPanic.exe`; başlatıcı ve LATEST bu adayı gösterir. V4.13 numarası sonraki logo teslimine ayrılmıştır.

Normal kampanyanın her dalgasında 120 saniye güçlendirme ve ardından30 saniye FPS afet çantası vardır: toplam150 saniye. HUD ilk aşamada2:00, sonra ayrı FPS ekranında30sn gösterir. Pause ve ilerleme çubuğu aynı aşama saatini kullanır. Ayrı kısa akış, öğretici ve açık afet simülasyonu kendi sürelerini korur. Kayıp sınırı75, gerçek stok, maliyet, iptal ve skor kuralları değişmedi.

Seçili gerçek meshlerin görünür yüzeyine yaklaşık yüzde20 turuncu/altın vurgu ve ince siluet uygulanır. Nesnenin dışındaki boş alanı kaplayan tel kutu ve ayrı yerleştirme zemin çizgisi kaldırıldı. Duvar seçimi en yakın gerçek authored segmente bağlanır. Gerçek derinlik, materyal alfa kesimi ve kat görünürlüğü korunur. Geçerli/geçersiz taşınan eşyanın kendi ghost yüzeyi yeşil/kırmızı görünür.

Yalnız son çanta aşamasında görünür ve stoklu gerçek WorldSupply meshleri küçük, aralıklı sıcak beyaz yüzey parıltısı alır. Yaklaşık3.6sn aralıklı tarama ve küçük çekirdek kullanılır; sahneye ışık, sprite veya büyük beacon eklenmedi. Toplama veya faz değişimi mevcut kaynak sahipliğinden gelir. Önlem malzemeleri ve dolaplar bu parıltıyı almaz.

## Kaynak, derleme ve native katmanı

DLL SHA256: `F5C34832B432FD4583E77CAC099F3B53CD403F52CD924FDD180C23B176B01AEE`.

Derleme0hata/0uyarı. Son görünür native test **223/223 PASS,0runtime hata,24.31sn**:91 model ve132 dünya/işlem/frame/render kontrolü.15konum/saat/hazır fikstürü kullanılır; OS veya insan kabulü değildir. [Son native receipt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/silhouette-intake/native-r4-0486a86d-47de-4d03-b8bd-126840cf79e0/revision412-smoke.json).

R3'te ilk test penceresi gizli başlatıldığı için bazı kareler çizilmedi ve test reddedildi. Aynı R3 DLL görünür pencerede207/207 geçti; yeni süreyi içeren R4 sonucu bu ayrı223kontroldür. Eski başarısız kanıt silinmedi ve son sonuca eklenmedi. Gerçek FPS su şişesi ve seçili pencere karelerinde boş kutu yerine gerçek asset yüzeyi gözlendi. Parıltının zaman içindeki okunurluğu/tadı insan kabulü değildir.

Kaynak manifesti34543dosya: [r4-source-manifest.json](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/silhouette-intake/r4-source-manifest.json). ZIP SHA256 `0F652F68AF06263D05CDCDC8BFFCA0F22671332E19F99B00D35B3FC20883E750`;184817461bayt. ZIP içindeki DLL son native DLL ile eşleşti. [Portable receipt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/silhouette-intake/r4-portable-receipt.json). Yeni ücret/sağlayıcı/kredi kullanımı yok; mevcut asset ve lisans kaynakları korundu.

## Gerçek Windows girdisi

Kullanıcı SendInput ile basılı tuş ve fareyi açıkça yetkilendirdi. Gerçek hareket, E/F, tıklama, kamera ve normal pause kullanılır; hile, teleport veya doğrudan Model mutasyonu kullanılmaz. R4 normal oyuncusu PID64564, yalnız skor dosyası izole; smoke/test sürücüsü yoktur.

Önceki R2 turunda kitaplık iki gerçek vida ile sabitlendi:1000→940bütçe,2vida ve1ankraj tüketimi. Yapım kaynakları alındı, gerçek merdivenler yüründü ve son30 FPS'de su/gıda1/1toplandı. Sığınak kapağına gelindi, fakat E etkileşimi tamamlanmadan süre doldu; başarılı tur sayılmadı. [R2 gerçek girdi receipt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/os-playtest/sendinput-fullround-20261017/RECEIPT.json).

R4 ilk denemesinde gözlenen oyun durumu, gönderilen girdilerin dışında yeniden başlamış/yer değiştirmiş göründü; kullanıcı kontrolü netleştirdi ve testi tamamlamayı root'a bıraktı. Son temiz R4 turunda 120 saniye hazırlık ardından son30 FPS'de gerçek F ile su ve gıda toplandı,11 saniye kala gerçek E ile sığınağa girildi. Deprem, erzak tüketimi,53.2% hasar ve1000→1294 ödül görüldü. Normal pause kullanıldı; kesintisiz duvar saati koşusu veya tüm kampanya değildir. Sonraki dalga inceleme sırasında boşta kaldığı için zaman aşımına uğradı; ilk dalga başarı kaydı ayrı tutuldu. [Gerçek girdi receipt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/os-playtest/sendinput-final-r4-7a0bd941-7571-4516-9f99-d4b4e8142491/RECEIPT.json).

Bu turda kapağın yukarı kalkmak yerine zeminin altına döndüğü bulundu. Açıklık oyuncudan önce görünür hale gelse de yükselmiş panel görünmedi. R4 native testinin mutlak açı kontrolü dönüş yönünü yakalamıyordu. Bu iki satırlık yön düzeltmesi ve yeni yönlü regresyon kontrolü ayrı R5 teslimine aittir; R4 DLL'si bu düzeltmeyi içermez. Güncel teslim için LATEST'i izleyin.

Gerçek OS turu tam75sınırını, tüm ağaç sıralarını veya tüm afet çatı çekimlerini kapsamaz. Bunların native fikstür kontrolü ayrı katmandır. Normal oyunda mutfak pencere seçimi ve iç kapı/korkuluk rotasının okunurluğu önceki R2 denemesinde zorlayıcı bulundu; yeni kozmetik/timing değişikliği bunları ayrıca çözmüş iddiası taşımaz.
