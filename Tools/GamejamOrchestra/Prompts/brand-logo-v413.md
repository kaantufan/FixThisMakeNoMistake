# Görev: Güncel Too Late to Panic! oyununa logo ve Windows EXE ikonu paketini entegre et

Çalışma alanı: `C:\Users\kaant\Desktop\civilgamejam\TooLateToPanic`
Aktif Unity projesi: bu alanın içindeki `TooLateToPanic/`.
Paket: `Evidence/GamejamOrchestra/TLTP-LOGO-20261017/staging/brand/`.

Bu görev mevcut HUD'nin başlık ekranına onaylı logoyu ve Windows EXE'sine amblemi ekler. Mevcut oyun içi HUD, son 30 saniyelik FPS çanta arayüzü ve ekran akışları korunur. Genel HUD yeniden tasarımı veya bağımsız `TooLateToPanicFPS/` projesinde çalışma kapsamda değildir.

1. **Güncel tabanı ve sahipliği doğrula.**

   Önce çalışma alanı ve proje `AGENTS.md` dosyalarını, paketin `HANDOFF.md` / `TARGET-MAPPING.json` dosyalarını, proje `LATEST-BUILD.json` dosyasını, güncel teslim kaydını ve canlı `PanicHomeHud.DrawTitle` / `PanicResidenceBuild.Build` kodunu oku. Unity 6000.0.73f1, Built-In, Windows64 ve çevrimdışı çalışma hedefini koru. Unity/import/build/player işlemlerinin tek sahibi root'tur; başka aktif sahibin işini kesme ve eşzamanlı import/build başlatma. Görev başında gerçek model/rol uygunluğunu AGENTS kurallarıyla karşılaştır; sessiz model ikamesi yapma.

   Bu prompt hazırlanırken güncel sürüm `Home V4.12 / 0.4.12`, doğrulanmış çıktı `Builds/Home-V4.12-r2-20261017`, builder'ın varsayılan çıktısı ise `Builds/Home-V4.12-20261017` idi. Uygulamadan önce yeniden doğrula. Daha yeni bir teslim varsa onu geri alma; sonraki paket numarasını güncel tabandan belirle.

2. **Dokuz dosyayı hash kontrolüyle birlikte entegre et.**

   Mapping yolları çalışma alanı köküne göredir. Dokuz staged dosyanın SHA256 değerlerini `staged_sha256` ile doğrula; 2 değiştirme ve 7 eklemeyi, PNG'ler ve kararlı GUID taşıyan `.meta` dosyalarıyla birlikte al. Önce küçük kaynak snapshot'ı ve önce/sonra manifesti oluştur; orijinal staging paketini ve tarihsel hashlerini koru.

   Değiştirme öncesi canlı baseline değerlerini yeniden karşılaştır:

   - `TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs`: `918815574D10BBE887367405F5DC7C13CA94AEB20E33676C83827A62A81E260A`
   - `TooLateToPanic/Assets/Game/Editor/PanicResidenceBuild.cs`: `C9C76AC134105C8B5BBB700FF5D4F38A65B35A8B5825C389A79613A9AAC207B2`

   Bu incelemede iki baseline eşleşti, 9 staged hash doğruydu ve 7 ekleme hedefi henüz yoktu; bu durum sonraki çalıştırma için garanti değildir. Kaynak kaymışsa staged C# dosyasını üzerine yazma. HANDOFF'taki `_brandLogo`/`DrawTitle` ve `ConfigureBrand()` değişikliklerini güncel dosyaya dar diff olarak uygula. Ekleme hedefi zaten varsa hash/GUID karşılaştır; farklı içeriği körlemesine değiştirme.

   `DrawTitle` gerçek 1600×900 mantıksal canvas ve mevcut ölçekleme üzerinden `HUD/TltpLogo` dokusunu `Rect(160,160,580,290)` içinde oranını koruyarak çizer. Kicker, uyarı şeridi, slogan, menü, skor paneli ve girdi bölgeleri korunur. Logo başarıyla yüklendiğinde eski iki satırlı metin ve fırça çizilmez; yalnız doku yükleme hatasında mevcut metin fallback'i kalır.

3. **Son entegre kaynağı ayrık derlemeyle doğrula.**

   Paketteki `detached_compile.py`, iki C# dosyası için sabit staged override kullanır. Güncel dosyaya yeniden uygulanan değişiklikleri veya sonraki sürüm artışını kendiliğinden test etmez. Göreve ait ayrı bir manager alanında kopyasını uyarlayarak son entegre HUD ve builder'ı, güncel Runtime + Art + Editor kaynaklarıyla derle. Kullanılan kaynak yollarını ve hashleri kaydet. Hem runtime hem editor derlemesinin sonucunu ve tanılarını kontrol et; script süreç çıkışının tek başına başarılı görünmesine güvenme. Ayrık derleme Unity import/build veya native kabul değildir.

4. **Sürümü artır ve tek birleşik build al.**

   Taban hâlâ 0.4.12 ise hedef `Home V4.13 / 0.4.13` olsun. Builder `Version`, menü etiketi ve varsayılan çıktı klasörünü tutarlı güncelle; çıktı `Builds/Home-V4.13-20261017/TooLateToPanic.exe` olsun. Mevcut adayın üzerine yazma; gerekiyorsa builder'ın mevcut `-panicBuildDirectory` seçeneğiyle benzersiz aday klasörü kullan. Sürüm değişikliği dahil son kaynakla ayrık derlemeyi tamamla.

   `TooLateToPanic.PanicResidenceBuild.Build` üzerinden `-reuseResidenceArt` ile tek birleşik import/build yap. Karakter veya sahne sanatını yeniden bake etme. `ConfigureBrand()` reuse ve tam bake dallarından önce çalışmalı; logo için saydamlık/mipmap/trilinear, ikonlar için uygun import ayarları ve Standalone boyut eşlemesi uygulanmalı. ≤32 px `TltpMarkSmall`, daha büyük boyutlar `TltpMark` kullanmalı. Amblemler runtime Resources dışında tutulur; yine de EXE ikon kaynaklarına gömülür.

   Logda `HOME_BRAND logo+icon sizes=...`, başarılı BuildReport ve 0 hata bulunmalı. Uyarıları gerçek sayısıyla kaydet. Somut hata olursa dar düzeltme ve gerekli tekrar yap; başarısız adayı geçti sayma.

5. **Başlık ekranını ve güncel kısa smoke'u aynı adayda kontrol et.**

   Taze GUID adlı izole kanıt/profil diziniyle `--revision412-smoke --home-evidence <yeni-GUID-dizini>` çalıştır. Bu, canlı kaynakta mevcut V4.12 sürücüsüdür; sürüm artışı nedeniyle testi yeniden adlandırma veya eski HUD/minigame beklentili smoke'u ikame etme. Önceki 204/204 sonucunu yeni DLL kabulü sayma; yeni receipt, log, süre, gerçek kontrol sayısı ve DLL SHA256 değerini kaydet. Gerçek skorların önce/sonra hashlerini koru.

   Yeni adaydan 1600×900 başlık karesi al; paketteki `preview-title-1600x900.png` ile karşılaştır. Bu PNG kompozit önizlemedir, native kanıt değildir. Logo görünür ve oranı doğru olmalı; kicker, slogan ve menüyle çakışmamalı; başarılı logo yüklemesinde eski metin/fırça görünmemeli. 960×540 başlık görünümünde de kırpılma/çakışmayı dar olarak kontrol et. Mevcut FPS çanta HUD'si, F2, pause, defter, envanter ve `PointerOverUi` sözleşmeleri korunmalı. Native API/render, OS girdisi ve insan kabulünü ayrı raporla.

6. **EXE ikonunun gerçek boyut temsillerini doğrula.**

   Son EXE'nin gömülü `RT_GROUP_ICON` / `RT_ICON` kaynaklarında 16/32/48/256 px temsilleri denetle ve çıkarılan görselleri kaydet. 16/32 px çatlaksız küçük amblem; 48/256 px tam amblem olmalı. Tek bir bitmap'i dört boyuta yeniden ölçeklemek bu kontrolün yerine geçmez.

   `System.Drawing.Icon.ExtractAssociatedIcon` çağrısını ek ilişkilendirilmiş ikon kontrolü olarak kullan; API boyut seçimi almadığından bunu dört gömülü temsilin kanıtı sayma. Explorer görünümünü ayrıca OS/shell katmanında doğrula. Araç erişimi yoksa bu kısmı `NOT_RUN`/`BLOCKED` olarak açıkça kaydet; kaynak incelemesiyle geçmiş sayma. Sistem geneli ikon önbelleğini temizleme veya Explorer'ı yeniden başlatma.

7. **Yeni paketi teslim et ve izlenebilirliği tamamla.**

   Kontrollerden geçen adaydan tek güncel portable ZIP üret. EXE ve `Assembly-CSharp.dll` dosyalarının ZIP içindeki kopyalarla hash eşleşmesini doğrula. Proje README'sini, `LATEST-BUILD.json` dosyasını, `Documentation/HOME-V4.13-BRAND-DELIVERY-2026-10-17.md` teslim kaydını ve güncel başlatıcıyı birlikte güncelle; üst başlatıcının doğru proje başlatıcısına yöneldiğini kontrol et. Başlangıçta daha yeni sürüm bulunduysa bu adları seçilen yeni sürüme uyarla.

   Tasarım/provenance kaynağı olarak `TooLateToPanic/Documentation/HUD-Concept/logo/` ve `logo.html` belirt. Kaynak/build manifestleri, önce/sonra hashleri, son EXE/DLL/ZIP SHA256 değerleri, import/build logu, native receipt/kareler, ikon çıkarımları ve gerçek kalan sınırları aynı teslim kimliğine bağla. Önceki V4.12 kanıtını yeni sürümün sonucu gibi taşımadan tarihsel olarak koru.

   Ürün ve şirket adı, `persistentDataPath`, sahne tasarımı/giriş sözleşmesi, controller, Model/Expedition verileri, skorlar ve güncel ses politikası değişmez. V4.12'nin izometrik önlem → son 30 saniyede otomatik FPS çanta akışı, G ile dönüş olmaması, 1 su + 1 gıda ve %75 kayıp sınırı korunur. Yeni ses/asset sağlayıcısı veya kredi kullanımı başlatma.

   Açık kullanıcı oyununu kapatma. Yeni aday açılıp kısa kontrol geçmeden eski çalışan build'i kaldırma. Temizlik öncesi süreçleri yeniden kontrol et; tarihsel PID'lere güvenme. Yalnız aktif projenin çözülmüş `Builds/` sınırındaki kullanımda olmayan ve temizlenmesine izin verilen çıktılar değerlendirilebilir. Önceden otomatik onay incelemesinde reddedilmiş temizlik hedeflerini tekrar veya alternatif yolla silme/taşıma; koruma istisnalarını kaydet. Kaynaklar, lisanslar, gerçek kayıtlar, tarihsel küçük kanıtlar ve bağımsız FPS projesi korunur.

Son raporda uygulanan değişiklikleri, son paket yollarını, gerçekten yapılan kontrolleri ve eksik kalan kabul katmanlarını kısa ve açık yaz.

Teknik referans: [Microsoft — Icon.ExtractAssociatedIcon](https://learn.microsoft.com/en-us/dotnet/api/system.drawing.icon.extractassociatedicon).
