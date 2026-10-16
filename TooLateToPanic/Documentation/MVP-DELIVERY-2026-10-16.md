# Too Late to Panic! — MVP 0.1 teslim kaydı

16 Ekim 2026. Tek odalı, izometrik, çevrimdışı Windows ilk oynanabilir sürüm.

## Çıktı

- Unity kaynak projesi: `TooLateToPanic/` (bu belgenin iki üst klasörü).
- Çalıştırıcı: `Play-TooLateToPanic.cmd`.
- Windows klasörü: `Builds/MVP-20261016/`.
- Motor: Unity 6000.0.73f1 Built-In, Mono Windows x64.
- Son build: **Succeeded, 0 hata, 0 uyarı**, 94.308.605 byte build içeriği.
- Yeni insan/çocuk/sakin üretimi: **0**. Tek oyuncu için mevcut Quaternius CC0 gövde ve animasyon kaynağı yeniden kullanıldı; bu karakter görseli son sanat kabulü değildir.

## Uygulanan kapsam

| Sistem | Durum ve kanıt sınırı |
| --- | --- |
| Tek oda, sabit izometrik kamera, döşenmiş ev | Native hazırlık görüntüsünde görüldü. |
| Mobilya/yapı seçimi ve işlem paneli | Entegre. Native smoke programatik dünya API'siyle seçti; normal fare tıklaması ayrıca doğrulanmadı. |
| T0/T1/T2 güçlendirme, ücret ve görünür donanım | Entegre; süreli çatı yükseltmesi native geçti, diğer parçalar aynı model kuralına bağlı. |
| Sabitleme ve sökme, kalıcı yetki | Entegre; süreli sabitleme ve wave sonrası korunma native geçti. |
| Taşıma, döndürme, önizleme, çakışma ve bunker yolu | Entegre. Native geçerli/geçersiz aday ve veri taşınması kontrol edildi; gerçek fare sürükleme/iptal akışı elle doğrulanmadı. |
| Pencere kurulum noktası, sel bariyeri ve yangın ekipmanı | Entegre. Bariyer/yangın önlemiyle ilgili afetler native geçti; ek pencerenin fareyle kurulum adımı elle denenmedi. |
| Su/gıda satın alma ve paketleme | Native süreli alışveriş/paketleme ve her wave tüketimi geçti. |
| Dört kalıcı destek kategorisi | Satın alma/paketleme ve bütçe bonusuna bağlı; dört kategorinin bütün kombinasyonları test edilmedi. |
| Deprem, Sel, Yangın | Native 1–9 wave akışında üçü de görüldü. Sonraki RNG source-integrated, uzun koşu testi yapılmadı. |
| Bunker, çanta yetersizliği, >%65 hasar ve geç kalma | Başarı ve üç kayıp nedeni native kısa kontrolde geçti. Tam %65 sınırı kaynakta <=65 olarak uygulanır; ayrıca sınır fixture'ı çalıştırılmadı. |
| Dört kısa gerçek zamanlı cutscene türü | Afet, sonrası, başarı ve başarısızlık aşamaları entegre; film dosyası veya sesli oyunculuk değildir. |
| Hasar gezisi ve dünya içi belirteçler | Native üç afetin inceleme görüntüleri oluştu; toplam katkı ve reset native geçti. Yoğun hasarda belirteçler üst üste gelebilir. |
| Öğreticinin gerçek koşudan ayrılması | Bütçe, malzeme, puan reseti native geçti. Ayrıntılı öğretici kullanıcı testi yapılmadı. |
| Yerel puan tablosu | Dosyaya kayıt ve yeni model nesnesinde yeniden okuma native geçti. Öncelik wave, sonra puan. |
| WASD/ok tuşu hareket, kamera yakınlaştırma | Kaynakta entegre; normal klavye kontrolüyle uzun oynanış doğrulanmadı. |

## Asset teslimi

22 temel asset türü ve oda kabuğu üretildi: kitaplık, koltuk, masa, sandalye, buzdolabı, dolap, yatak, bitki, TV sehpası, çanta masası, bunker, pencere, kapı, duvar, çatı, yangın ünitesi, su, gıda, ilk yardım, fener, radyo, battaniye.

Ayrı Sol işi sel bariyerini, sabitleme donanımını ve beş güvenlik piktogramını üretti. Kütüphane Unity `.asset` mesh/materyal ve `.prefab` dosyalarına dönüştürüldü: **172 mesh, 26 ortak materyal**. Sahne içindeki görsel yükseltmeler ve hasar, ayrıca runtime mesh katmanları kullanır.

Sekiz gerçek PCM16 WAV üretildi: dört ahşap adım varyantı, bunker mekanizması, inşa, alışveriş ve 24 saniyelik ortam döngüsü. Dosya düzeyi clipping/format kontrolü var; insan kulağıyla mix onayı verilmedi. Afet efektleri ve ek sesler özgün yerel kaynakla üretilir. Yeni sağlayıcı satın alımı veya üretim kredisi kullanılmadı.

## Kısa doğrulama sonucu

Son native rapor: **65 kontrol geçti, 0 runtime hata, 9 tamamlanan wave**. Koşu toplamı 9.652 puandır; bu denge önerisi değil, izole smoke sonucudur.

Bu kontrol gerçek Windows player içinde çalıştı; faz geçişleri hızlandırıldı ve bazı eylemler doğrudan dünya/model API'siyle çağrıldı. Normal giriş zamanlaması ve tam kullanıcı oynanışı yerine geçmez. İlk build'in native kontrolünde görülen iş başlangıcı/odak ve UI sorunları dar bir onarım build'inde düzeltildi; eski başarısız/eksik kanıtlar korunmuştur.

**Kullanıcı fiziksel Esc ile Computer Use'u durdurdu.** Bu sinyalden sonra yeni pencere/fare/klavye otomasyonu yapılmadı. Devam eden native player'ın oluşturduğu dosyalar yalnızca okundu. Normal fare/klavye ile son kontrol tamamlanmadı. Açık test penceresi izole skor profili kullanır; normal kullanım için çalıştırıcı test bayrakları olmadan açar.

Son renderlerin kaynakları:

- `Evidence/GamejamOrchestra/TLTP-MVP-20261016/native-final/02-prepared-room.png`
- `Evidence/GamejamOrchestra/TLTP-MVP-20261016/native-final/disaster-Earthquake.png`
- `Evidence/GamejamOrchestra/TLTP-MVP-20261016/native-final/disaster-Flood.png`
- `Evidence/GamejamOrchestra/TLTP-MVP-20261016/native-final/disaster-Fire.png`
- Aynı klasörde üç afetin `inspection-*` görüntüleri ve `native-smoke.json`.

Bu yollar parent çalışma alanına göredir. Source/build hash manifestleri aynı Evidence paketindedir. Yeni insan/oda sistemi ve geniş kapsamlı test ilk MVP'ye dahil edilmedi. Sayısal hasar/ödül/süre dengesi ilk oyuncu denemelerinde ayarlanabilir.
