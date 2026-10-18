# Kaynak, build ve yayın güncelleme düzeni

Bu düzen bütün sonraki güncellemeler için geçerlidir. Yarışmaya gönderim ayrı bir kullanıcı işlemidir; GitHub güncellemesi gönderim veya teslim makbuzu sayılmaz.

## Sabit klasörler ve sorumluluk

`TooLateToPanic/Assets`, `Packages`, `ProjectSettings` aktif kaynak kökleridir. `Documentation` sürüm/asset/lisans kayıtlarını; kök `Docs` tasarım ve yarışma belgelerini; `Tools` tekrar kullanılabilir araçları tutar. Yerel `Evidence` kanıt/staging/yayın kayıtları, `Builds` Windows dağıtımı içindir. Eski FPS projesi, gerçek kayıtlar ve açık oyuncular korunur. Kaynak klasörlerini yalnız görünümü sadeleştirmek için taşımayın.

## Bir güncellemeyi teslim etme

1. **Başlangıcı kaydet.** Son LATEST, source/DLL/ZIP hashleri ve açık Unity/player süreçlerini belirle. İlgili sözleşmede yeni davranışı, dosya sahibini ve kabul sınırını yaz. Alt görevlerin staging alanları farklı olsun.
2. **Dar değişikliği tamamla.** Yeni asset ve AI kullanımını kaynak/lisans beyanına ekle. Gerçek maliyet, iptal, controller, stok/RNG, skor ve sığınak kurallarını test geçsin diye gevşetme.
3. **Kaynağı dondur.** Dahil kaynakların SHA256 manifestini al. Birleşik import/build'i yeni ve benzersiz çıktı klasörüne yap. Başarısız/ara aday güncel paket olmaz.
4. **İlgili kontrolü çalıştır.** Tek ilgili kısa smoke ve gerekli Editor/native kontrolleri yeterlidir. Somut hata veya yeni risk olmadıkça geniş test turunu tekrarlama. Kanıtı DLL, profil, süre ve hash ile bağla. OS/insan kontrolü yapılmadıysa açık yaz.
5. **Paketle ve eşleştir.** EXE, veri klasörleri ve lisanslarla ZIP üret. Build manifestindeki her dosya ile ZIP içeriğini byte/hash eşleştir. ZIP ve DLL SHA256 değerlerini kaydet.
6. **Güncel girişleri birlikte değiştir.** Kök README, proje README, launcher, LATEST, sürüm teslim notu, asset/AI beyanı ve yayın durumunda aynı kimlik gösterilsin. LATEST eski native sonuçlarını yeni DLL'ye taşımamalı.
   `python Tools/Repository/check_layout.py --local-build` ile güncel bağlantılar, README/LATEST, launcher ve tek build/ZIP kimliği kontrol edilir. Public kaynak kopyasında `--local-build` kullanılmaz.
7. **Ayrı kopyadan GitHub'a yayınla.** Canlı non-Git çalışma alanında `git init/reset/clean` çalıştırma. Ayrı snapshot/clone kullan; Windows'ta `core.longpaths=true`. Kaynak export'unda GUID referans kapanımı ve runtime `Shader.Find` materyalleri korunmalı. Cache/build/Evidence/FPS/kayıt/secret taraması ve index byte kontrolü yapılmalı. Uzak main değiştiyse yeniden incele; force push yapma.
8. **Kaynak ve paket ilişkisini belirt.** Kaynak commit'i, varsa release tag commit'i ve ZIP digest'ini ayrı yaz. Belge güncellemesi veya ilerideki kaynak snapshot'ı mevcut paketin yeniden test edildiği anlamına gelmez. Etiketli oyun paketi immutable kalır.
9. **Sonra eski çıktıyı temizle.** Yeni paket açılıp ilgili kontrol geçtikten sonra yalnız artık kullanılmayan build/ZIP'leri kaldır. Önce mutlak hedefleri, süreç bağımlılıklarını ve reparse noktalarını doğrula; küçük metin/hash/receipt/lisans kayıtlarını koru. Çalışan player'ın klasörü istisnadır. Normal durumda Builds'te tek güncel build + tek portable ZIP kalır.

Oynanabilir paket yayımlanırken önce draft release oluşturulur ve asset digest/boyutu yerel ZIP ile eşleştirilir; sonra uygun ön sürüm/stable durumu ile görünür yapılır. `requiresUserPlaytest` açık olan paket ön sürüm olarak işaretlenir. README, PUBLICATION ve LATEST gerçek doğrulanmış indirme bağlantısıyla birlikte güncellenir. Etiket kabul edilmiş kaynak commit'inde sabit kalır; daha sonraki belge commit'i için taşınmaz. Eski release/tag/asset korunur.

## Belge güncellemesi

Yalnız README/yarışma/dizin değiştiyse yeni oyun build'i gerekmez. Mevcut kaynak, DLL ve ZIP kimlikleri korunur; belge değişikliği ayrı commit ve makbuzla izlenir. Manifestin kapsadığı bir kaynak dosyası değişirse eski kaynak eşleşmesi yeni dosyaya aktarılmaz.

## Yarışma paketi ayrı tutulur

18 Ekim 2026 son duyurusu: en fazla 60 saniye MP4/AVI video, build ZIP'inden ayrı yüklenir; public Git repo gerekir. Üç dakikalık sunuma video süresi dahildir. AI kategorileri ve hazır kaynak beyanları her sürümde yeniden gözden geçirilir. Teslim penceresi 12.00–18.15 UTC+3; PIN forma, issue'ya veya depoya yazılmaz. Kullanıcı gönderim istemeden yarışma formuna dosya/alan yüklenmez. Gelecekte teslim yapılırsa yükleme doğrulaması ve teslim makbuzu ayrıca kaydedilir.
