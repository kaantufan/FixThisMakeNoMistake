> Source documentation update. The current promoted playable package remains [Home V4.12-r4](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.12-r4). R5 is a separate candidate until its own LATEST/build promotion and publication. Local Evidence links below refer to the original development archive; this documentation push runs no new build/native/OS tests.

# Home V4.12-r5 — gerçek oynanışta bulunan kapak yönü düzeltmesi

17 Ekim2026. Unity6000.0.73f1/Built-In/Windows64/0.4.12. R4'ün120sn güçlendirme+30sn FPS çanta, gerçek asset yüzeyi seçimi, aralıklı pickup parıltısı ve önceki V4.12 mekanikleri korunur. V4.13 sonraki logo işi için ayrılmıştır.

R4'te gerçek Windows girdisiyle bir normal dalga tamamlandı: son30 saniyede su/gıda F ile toplandı,11sn kala sığınağa E ile girildi; deprem,53.2% hasar, erzak tüketimi ve ödül görüldü. Bu sırada kapak zeminin altına dönüyordu. Kuzey menteşesinden local-Z yönüne uzanan panel için açılış X dönüşü-105 yerine+105 yapıldı. Genel menteşe animatörü aynı yönü kullanır; oyuncunun iniş zamanlaması korunur. Native kontrol artık yalnız mutlak açıya bakmaz, iniş boyunca gerçek panel merkezinin menteşeden en az0.45m yukarıda olmasını da sınar.

## Derleme ve bağımsız native katmanı

Build0hata/0uyarı. DLL SHA256 `49AB7DF4F0695BC8762F70F55597F98AF7D0102F01F9D685F5604791A2312C05`. Yeni görünür native test224/224 PASS,24.14sn; gerçek skor hash'i korunur.15konum/saat/hazırlık fikstürü kullanılır. Bu test OS veya insan kabulü değildir. [Native kayıt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/hatch-upward-intake/native-r5-5436d019-512f-4ac6-aee6-ad720dff8b61/revision412-smoke.json).

Tam kaynak34544 dosya/1548425418bayt; R5 build'inin yeni172bayt bake meta dosyası dahildir. [Paket receipt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/hatch-upward-intake/packaging/packaging-receipt.json) kaynak/build manifestlerini bağlar. ZIP184817636bayt; SHA256 `7EE999C9799C4CDCDE89630FFEDC42B577BC09AC3A187776F7FDC2D0BC260355`. İç DLL üretilen DLL ile eşleşir. Yeni sağlayıcı/kredi/GitHub yayını yok; bağımsız eski FPS projesine dokunulmadı.

## Gerçek Windows girdisi

R5 normal player PID47320; yalnız skor yolu izole, test sürücüsü yok. SendInput basılı WASD/Shift, göreli fare, E/F, gerçek vida tıklamaları ve normal ESC pause kullanıldı. Hile, teleport, Model mutasyonu veya saat değişimi0.

İlk R5 denemesinde gerçek kitaplık açılışı2vida+1ankraj sağladı. İki vida tıklaması sabitlemeyi bitirdi: bütçe1000→940, stok2vida/1ankraj→0/0 ve ÇÖZ seçeneği görüldü. Kitaplığın yanındaki duvar ve pencere ayrı ayrı gerçek mesh yüzeyiyle seçildi. Sıfır stokta eksik ekranı1tahta/2vida gösterdi;12+8 envanter boşluğu ve harcanmış stok gerçek ekranda görüldü. Başarılı malzemeli duvar/pencere güçlendirme bu OS denemesinin kapsamı değildir. Son30FPS'de su/gıda1/1 toplandı; gıdaya geri dönüş sebebiyle sığınak E öncesi süre doldu. Bu deneme tam tur PASS sayılmaz.

Doğrudan rotayla ikinci R5 turu şu an sürüyor; bu paragraf final OS receipt ile güncellenecektir.

Tam75hasar sınırı, tüm ağaç sıralarının her gövdesi, tüm afet çatı çekimleri ve bütün kampanya OS/insan kabulü değildir. Bunlar native fikstürleriyle ayrı değerlendirilir. İnceleme için normal pause kullanıldığından kesintisiz gerçek zamanlı koşu iddiası yoktur.
