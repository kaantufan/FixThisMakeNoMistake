# Home V4.6 — HUD konsepti teslimi

17 Ekim 2026. Kullanıcının HTML HUD konsepti aktif izometrik oyuna entegre edildi. Başlık, ayrı dalga/süre/kaynak kartları, kesin malzeme açıkları, toplama, çanta, üç iş ailesi, duraklat/geliştirici, afet/kayıp/sonuç ve hasar gezisi aynı kömür/krem/petrol/amber/pas/alarm tasarımını kullanır. HTML ve görselleri korunur; canlı sahnenin yerine ekran görüntüsü konmadı.

1600×900 mantıksal tuval gerçek pencereye sığar; eski .65 minimum ölçek kaldırıldı. 4:3/görece küçük pencerelerde merkezlenir; sahne tüm viewportta kalır. Çanta/işte gerçek saat görünür; envanter saati durdurmaz. F2/önceki duraklatma, hileli koşu/skor yalıtımı, gerçek E/F ve 1/2/3/4 eylemleri korunur. İkonlar mevcut oyundan; fontlar çevrimdışı Oswald/Barlow/Barlow Condensed, [tam lisanslarıyla](Licenses/HUD/README.md) paketlidir. Yeni ses/ücretli sağlayıcı yok.

## Gerçek doğrulama

- Unity 6000.0.73f1 Built-In Windows 0.4.6: **0 hata, 0 uyarı**. Mevcut 3D art cache yeniden bake edilmedi; fontlar import edildi. Son kaynak snapshotı 39 C#/shader/font dosyası, 74 prefab hash'i; build sonrası kaynak ve prefab sapması 0.
- **75/75 kısa native API/fixture kontrolü**,0 runtime hata, 50 gerçek framebuffer PNG, **12.16 saniye**. 1600×900,1280×720,1024×768,1920×1080,960×540. Tuval/işaretli rect sınırları, büyük panel çakışmaları, canlı eksik adetleri, gerçek loot/packing, üç işte hizala/uygula sunumu ve ücretsiz iptal, envanter/developer/pause saati, afetler ve gerçek timeout→kayıp→sinematik→sonuç geçti.
- Ayrı izole profilde **fiziksel OS girdisi**: Enter ile başlat; Tab çanta; fareyle Geri; Esc duraklat; F2 aç/kapat; önceki 4:43 duraklatma saati korundu; fareyle başlığa dön/çık. Hiçbir hile açılmadı. Bu, kısa girdi kontrolüdür; insanın tam kampanya veya bütün mini oyunları bitirme kabulü değildir.
- Root ve bağımsız QA temsilî gerçek kareleri açıp inceledi. NativeV2 otomatik geçtiği halde küçük ölçekte dönen çizgiler kaydı; gerçek render incelemesiyle bulundu. DrawLine üst GUI matrisi×yerel dönüşüm şeklinde düzeltildi; V3 küçük pencere karelerinde halka/saat yerine oturdu. Önceki iki kaynak bulgusu (yuvarlak panelde geçici diziler, uyarı/sayaç örtüşmesi) kapatıldı.
- Gerçek kişisel skor SHA256 önce/sonra/son teslim **DF2355AADB092174A5167460E436FD64F83B3A280C3B05B87F2E28EBA47A40AD**. Native/OS durum fixture'ları ayrı GUID kayıt alanlarını kullandı.
- Son normal oyuncu PID53800 başlatıldı; HOME_V46_READY görüldü, ilk logda exception0. Kullanıcı fiziksel Esc ile Computer Use'ı durdurduğu için son yeni pencereye yeniden UI kontrolü uygulanmadı. Aynı DLL'nin önceki native ve fiziksel girdi kontrolü geçti.

## Build ve kanıt kimliği

- Build: `Builds/Home-V4.6-20261017/TooLateToPanic.exe`; tek güncel [başlatıcı](../Play-TooLateToPanic.cmd).
- DLL SHA256: `6E270494D3B663867DA5F0E190AB5A13D6A0E9779EE3ED742400795EF7261705`.
- ZIP: `Builds/TooLateToPanic-Home-V4.6-20261017-portable.zip`; 107,700,882 bayt,149entry. ZIP içi DLL test edilen DLL ile eşleşti; CRC kontrolü geçti. SHA256 `6E2699CB238A6203FC3564F685B29EA46078EA6B9C7682D71821D857680D7F5F`.
- Kaynak/build: `../../Evidence/GamejamOrchestra/TLTP-HUD-V46-20261017/source-build-v3/`.
- Native: `../../Evidence/GamejamOrchestra/TLTP-HUD-V46-20261017/native-v3-e3882617-c03a-4fc6-b8bc-907a2cb52037/native-hud-concept-smoke.json`.
- Girdi: `../../Evidence/GamejamOrchestra/TLTP-HUD-V46-20261017/os-input-verification.json`.
- İnceleme: `../../Evidence/GamejamOrchestra/TLTP-HUD-V46-20261017/review-staging/HUD-SOURCE-QA.md`.
- Güncel indeks: [LATEST-BUILD.json](../LATEST-BUILD.json). Kontrol/manifest/receipt kayıtları küçük tarihsel kanıttır; eski build yolları tarihsel metinlerde değiştirilmedi.

## Açık sınırlar

960×540'a sığar; küçük soluk açıklamalar ufak kalır. Rahat okuma için 1280×720 veya1600×900 tercih edilir. Beş çözünürlük kontrol edildi; ultrawide ve bütün 50karenin tek tek nihai sanat onayı yapılmadı. Hasar gezisi fixture'ı999 sn ile tutulur; HUD gerçek PhaseTime'ı gösterir, bu 20 sn insan süre kabulü değildir. Gerçek kampanya/kapı-merdiven/üç mini oyunu tamamlama/işitsel kabul yeniden yapılmadı; oyun kuralları, fizik ve ses kaynakları değiştirilmedi.

Yeni build/ZIP doğrulandıktan sonra kullanımda olmayan V4.2–V4.5 buildleri ve eski ZIP Geri Dönüşüm Kutusu'na alındı. İşlem öncesi mutlak Builds hedefleri/süreçler kontrol edildi; açık eski oyuncu yoktu. Kaynak/lisans/gerçek kayıt ve FPS projesi korunur. Bu HUD güncellemesinde uzak yayın yapılmadı.
