# Too Late to Panic! — Home V2 teslimi

16 Ekim 2026. Tasarım v1.3. Güncel sahne `TooLateToPanicHome`, giriş `PanicHomeWorld`. Çıktı `Builds/Home-V2-20261016/TooLateToPanic.exe`; proje başlatıcısı `Play-TooLateToPanic.cmd`, üst çalışma alanı başlatıcısı `Play-TooLateToPanic-HomeV2.cmd`.

**Sonuç:** Unity Windows build Succeeded, 0 hata / 0 uyarı. Son DLL ile 14/14 kısa native kontrol geçti; runtime hata0, ölçülen oturum ortalaması53.58 FPS. Kaynak manifesti ile mevcut kaynak arasında fark0. Normal, test argümansız oyuncu ayrıca başlık ekranına açıldı. Bu sayılar uzun kampanya veya farklı makineler için performans garantisi değildir.

## Uygulananlar

- 16×12 metre, iki yaşanabilir kat; fiziksel merdiven, bahçe ve bunker; yol, komşu evler, çitler ve bitkiler. Karakteri izleyen, yakınlığı ayarlanabilen izometrik kamera. İçeride çatı/ön duvar/üst kat kesiti.
- Ceviz/indigo/olive/keten paleti, yıpranma detayları; soğuk loş dış ortam ve okunur sıcak iç ışık. Pencere boşlukları ve şeffaf cam materyali.
- Mevcut CC0 humanoid üzerinde yeni iş kıyafeti/saç/bot/eldiven geometrisi; kumaşla kaplı kesintisiz skinned gövde tabanı; durma/yürüme/koşma Animator karışımı, ayakta/diz çökerek iş pozları ve el aleti.
- WASD hareket, Shift koşu, E etkileşim, Q hedef seçimi, C kapak kapatma; büyük kalıcı yan paneller yerine durum ve bağlam şeritleri.
- 12 yuvalı envanter, 8 yuvalı çanta; iki kattaki dolaplardan ve bahçe depolarından toplama. Dolap/buzdolabı/kutu kapakları hareket eder. İlk yardım 2 bandaj + 1 antiseptikten üretilir; fener/radyo pil ister.
- Dokuz yakın plan 3D iş düzeneği: duvar/zemin ankrajı, güçlendirme, sel bariyeri, yangın ekipmanı, fidan, drenaj, çit ve hortum. Dört adımda A/D hizalama, E kilitleme, Space uygulama. Vida ve alet hareketi ilerlemeye bağlıdır; sadece bekleme çubuğu değildir.
- Duvar veya zemine ankraj; sahip olunan ankrajı ücretsiz yeniden kullanma; eşyayı klavyeyle taşıma ve 45° döndürme; merdiven/çıkış/duvar/eşya çakışma kontrolü. Yapılar iki kademe yükselir. Uygulanan donanım görünür kalır.
- Bahçe önlemleri gerçek hasar hesabına katkı verir. Tüm işlerde para/malzeme önce kontrol edilir; yalnız başarılı işte bir kez tüketilir. İptalde tüketim yoktur.
- Deprem çatlak/toz/enkaz; sel hareketli su/yağmur/köpük/sürüklenen parçalar; yangın alev/duman/kor ve yerel ışık. Afet sırası/sonrası ve başarı/kayıp sunumları.
- Hasar gezisi, parça bazında işaretler; dalga sonunda hasar sıfırlanırken önlemler ve yerleşim korunur. Su/gıda tüketilir. Yerel highscore; öğretici gerçek koşu ve skordan ayrıdır.

Başlangıç 1000 bütçe; hazırlık 300/270/240 saniye. İlk üç dalga deprem, sonraki üç sel, sonraki üç yangın; ardından kontrollü RNG. İlk su/gıda ihtiyacı birer günlük oyun birimi. Ayrıntılı kurallar ve maliyetler `GDD.md` içinde.

## Ekip ve izlenebilirlik

Sekiz uzman ajan paketi: ev/çevre, karakter, sistemler, HUD, iş sanatı, afet sanatı, kısa oynanış QA ve görsel mini QA. Sanat Sol 6.1 Ultra; entegrasyon Luna XHigh. Dört eşzamanlı slot kayan kuyrukla kullanıldı. Root Unity/bake/build/player ve nihai entegrasyonu yürüttü.

Art yerel özgün mesh/doku/shader kaynaklarıyla üretildi. Yeni ücretli sağlayıcı işi yoktur. Mevcut Quaternius CC0 rig ve animasyon lisansları `Assets/Game/Resources/Licenses` altında korunur. Yeni nüfus veya çocuk asseti oluşturulmadı.

Eski MVP çıktısı ve açık eski oyuncu süreci korunur. Kaynak önceki hali, staging teslimleri, SHA256 manifestleri, her derleme kaydı ve izole native profilleri üst dizindeki `Evidence/GamejamOrchestra/TLTP-HOME-V2-20261016` altındadır.

## Doğrulama sınırı

Son doğrulama kaydı `native-v4/native-home-smoke.json`, görseller aynı klasörde; build kaydı `unity-build-v4.log`, kaynak manifesti `source-build-v4.json`. Final hash/özet `delivery-receipt.json` ile bağlanır. Native test kaydı ve derleme kaydı ayrı tutulur.

Kısa test gerçek Windows player, gerçek CharacterController/merdiven çarpışmaları, runtime etkileşim API'leri ve gerçek render kareleri kullanır. Fiziksel merdiven, toplama, ilk yardım, çanta, ankraj işi, yerleştirme engeli, bahçede fidan, bir başarılı dalga, kalıcılık/sarf tüketimi ve iki kayıp koşulu kontrol edilir. Üç afet görünüşü aynı kısa oturumda gösterilir; dokuz dalgalık kampanya tekrar edilmez.

Bu kontrol elle tam klavye oynanışı, bütün dokuz işin tek tek kullanıcı kabulü, uzun koşu dengesi, tüm yürüyüş pozlarında temas veya nihai sanat beğenisi anlamına gelmez. Stilize afet sunumları fiziksel bina yıkımı/hidrolik/yanma simülasyonu değildir. Rutin mikro mesh düzeltmeleri için uzun test döngüsü kurulmadı; yalnız görülen kıyafet, ışık ve parçacık hataları onarıldı.
