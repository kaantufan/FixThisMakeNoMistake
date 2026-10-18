# Home V5.5-r2 — oynanış, hikâye ve geçiş incelemesi

18 Ekim 2026. **Sonuç: kullanıcı testine açık; dört öncelikli sunum/yönlendirme sorunu var. Tam oynanış kabulü verilmedi.** Bu incelemede oyun kodu veya build değiştirilmedi.

Kullanıcının açık isteğiyle eski V5.4-r3 PID34104, Windows Alt+F4 ile kapatıldı. Ardından V5.5-r2 PID35640 açıldı. EXE yolu `Builds/Home-V5.5-r2-20261018/TooLateToPanic.exe`; DLL SHA256 `82B570D390EAE53F740944C1A6D7560E2E61E1B2D2FE0F5344B66A489676A789`. Dondurulmuş 162 C#/shader dosyası yeniden karşılaştırıldı: kayma 0. Test sonunda aynı V5.5 ana menüde açık bırakıldı; SERBEST OYNA yeni koşu başlatır.

## Yöntem ve gerçek kapsam

Root, computer-use ile gerçek Windows klavye/fare girdisi kullandı. Normal üretim arayüzü `--home-evidence` ile yeni, ayrı skor dosyasına açıldı; otomatik smoke sürücüsü, ışınlama, model API girdisi veya saat fikstürü kullanılmadı. 06:14–06:23 UTC arasında 34 ekran gözlemi/eylem kaydı alındı. Bu sayılar test assertion sayısı değildir.

İlk normal koşu, önlem/erzak/sığınak tamamlanmadan doğal süre sonuna bırakıldı: önce açık “SIĞINAĞA YETİŞEMEDİN” bildirimi, ardından nedenini yazan sonuç ekranı görüldü. İkinci koşuda açılış Enter ile atlandı; J defteri, ilk açıklama, üç kat planı, E ön kapı, küçük W hareketi, F1 aç/kapat, pause/devam ve doğal FPS aşaması kontrol edildi. İlk açılış doğal oynadı; dört çekimin her bir karesi ayrı incelenmiş sayılmaz. Defterde geçirilen sürenin hazırlık saatine eklenmediği gözlendi.

İkinci koşunun FPS gözlemlerinden **sonra** F3 kullanıldı. Bundan sonraki rota açıkça geliştirici korumalı, ödülsüz ve skor dışıdır; normal kazanma kanıtı değildir. Dalga1 Deprem → hasar gezisi; dalga2 doğal RNG Sel+Deprem → Enter ile geçiş → hasar gezisi; dalga3 Sel hazırlığı görüldü. Gerçek sığınak girişi uydurulmadı; sinematikte “Sığınağa ulaşamadın. Afet seni bulunduğun yerde yakaladı.” metni ve oyuncunun mevcut dış konumu görüldü. İzole skor dosyasında yalnız ilk normal 0 dalga/0 puan kaydı kaldı.

Üç ayrı salt okunur kaynak incelemesi hikâye, oyun mantığı ve sinematik akışını değerlendirdi. Kaynak bulguları aşağıda fiziksel ekran gözlemlerinden ayrıdır. Önceki 113/113 native API/model/fizik/render/teknik ses raporu tarihsel olarak aynı DLL'ye aittir; bu turun OS veya insan kabul sayısına eklenmedi.

## Beş alanda değerlendirme

| Alan | Değerlendirme | Kanıt ve sınır |
| --- | --- | --- |
| Oynanış tarzı | Ev hazırlığı → 45sn FPS erzak → sığınak → afet/sonuç döngüsü açık. Defter açıklaması ve küçük yardımcı mevcut HUD ile uyumlu. | Normal UI, kapı ve defter/F1/pause çalıştı. Kesintisiz toplama–sabitleme–erzak–sığınak kazanma rotası tamamlanmadı; akıcılık/keyif insan kabulü açık. |
| Hikâye bütünlüğü | KKTC · Güzelyurt · Kalkanlı, aile güvenli alanda, aile evini hazırlama amacı menü/açılış/defterde tutarlı. | Ekran ve kaynak. Çoklu sel altyazısı ve kısa akış/kampanya hedef köprüsü aşağıda açık. Uzun kampanyada kişisel anlatı büyük ölçüde sabit. |
| Mantık bütünlüğü | 120+45sn, 1 su+1 gıda, gerçek giriş, %75 kayıp, sonlu dalga başına loot ve maliyet/iptal kuralları kaynakta tutarlı. | Normal timeout kaybı ve korumalı çoklu afet gözlendi. Bütçesiz yardımcı hedefi ve eksik çantayla erken giriş metni sorunlu. |
| Sinematik geçişler | Açılışın doğal bitişi/Enter atlaması, FPS geçişi, kayıp bildirimi ve korumalı afet→hasar gezisi kontrolü geri verdi. | Enter sonrası aftermath yan etkileri kaynakta eksik; tüm kamera/kapı/kat kombinasyonları denenmedi. FPS girişindeki .84sn tam siyah kare bu OS örneklerinde yakalanmadı. |
| Tema ile bütünlük | Toplama, gerçek mobilya sabitleme, tesisatı güvene alma, erzak ve güvenli alana geçiş hazırlık temasını destekliyor. Hasar nedenini öğrenmek döngünün parçası. | Oyun içi tutarlılık değerlendirmesi. Gerçek dünya afet eğitimi doğrulaması değildir. Bazı açıklama/etiket sorunları öğrenmeyi zayıflatıyor. |

## Öncelikli bulgular

1. **QA-01 · P2 · OS ekranında doğrulandı — tesisat uyarısının nedeni eşya hasar etiketiyle örtüşüyor.** F3 korumalı dalga2 Sel+Deprem, Disaster sırasında Enter, hasar gezisi. “Salon masası +3,5%” etiketi “Elektrik kesilmedi…” satırının üzerine çizildi. Uyarı büyük görünse de nedeni okunamıyor. Dünya hasar etiketleri uyarı alanından uzaklaştırılmalı veya uyarı en son çizilmeli. [Ekran kanıtı](../../Evidence/GamejamOrchestra/TLTP-V55-PLAYREVIEW-20261018/manager/captures/compound-secondary-alert-overlap.png).

2. **QA-02 · P2 · Kaynakta doğrulandı — bütçe 60 altına düşünce yardımcı ücretsiz tesisata geçemiyor.** Seçili mobilyanın malzemeleri hazır, ankrajı eksik ve bütçe yetersizse yardımcı ikinci adımda kalıyor; boş rota, ücretsiz tesisat `else-if` kolunu bastırıyor. Oyuncu başka işi elle yapabilir; core softlock değildir. `PanicNotebookAssistantV55.cs:101,151`. Uygun ücretsiz işi önermek ve bütçe nedenini göstermek gerekir. OS tekrarı yapılmadı.

3. **QA-03 · P2 · Kaynak çağrı akışında doğrulandı — Disaster'da Enter aftermath kurulumunu atlıyor.** `PanicHomeCinema.cs:62–75` model fazlarını çözüp yalnız sunum fazını çağırıyor. `PanicHomeLife.cs:266–271` içindeki aftermath çevresi/ilk kurtarma helikopteri kurulumu çalışmıyor. Sayısal sonuç çözülse de çevre sonucu eksik kalabilir. Bu tur korumalı Sel+Deprem Enter geçişi çalıştı; eski aftermath nesneleri mevcut olduğundan eksik kök/helikopter varlığı fiziksel olarak ölçülmedi. Doğal ve skip yolu için dalga başına tek çalışan ortak kurulum gerekir; hasar/erzak/ödül tekrar çözülmemeli.

4. **QA-04 · P2 UX · Kaynakta doğrulandı — eksik çantayla sığınak E metni hazırlığın biteceğini söylemiyor.** Doğru sığınakta “Güvenli alanda bekle” yazıyor; E kalan süreyi hemen bitiriyor. 1 su/1 gıda eksikse kayıp kaçınılmaz. Çanta eksikleri üst HUD'de mevcut; girişin sonucunu nesne metni de açıklamalı. `PanicRevisionWorldV5.cs:248–250`, `PanicShelterAndSecondaryV5.cs:40–58`. OS tekrarı yapılmadı.

## Küçük sunum ve anlatı sorunları

- **P3, OS gözlemi:** Pencere başlığı hâlâ “Home V4”. Çalışan EXE/DLL V5.5-r2 olarak doğrulandı. Görünen sürüm etiketi, kayıt yolu kimliğini değiştirmeden düzeltilmeli.
- **P3, OS gözlemi:** Yaklaşık 1069×630 pencere görüntüsünde zemin kat krokisindeki bazı oda adları üst üste biniyor. Kat düğmeleri, merdiven, kaynak pinleri, balkon ve üst panik odası görüldü; bütün geometri noktaları ölçülmedi.
- **P3, kaynak:** Sel+Yangın veya Sel+Fırtına üst oda sinematiğinde 4,7sn sonrası ana altyazı yalnız seli anlatıyor (`PanicShelterPresentationV5.cs:50`). Üst afet listesi doğru; hasar hesabı kusuru değildir. Çoklu afet altyazısı kullanılmalı.
- **P3, kaynak:** Kısa akışın örnek güvenli alanları ile normal kampanyanın sığınak kuralı farklı; mode geçişinde kısa açıklama eksik. Bu kasıtlı ayrım sözleşme hatası sayılmadı.
- **P3, kaynak:** Eksik çanta+yüksek hasar metni 131–132 karakter olabilir; sonuç kartının 130 sınırı son kelimeyi keser. Kritik neden/%75 eşiği korunur. İlk üçlü neden örneği erişilebilir olmadığı için incelemede geri çekildi; önemli neden saklanıyor iddiası yoktur.

## FPS, ses ve kalan kabul sınırı

Doğal FPS aşamasında eller, çanta eksikleri ve küçük yardımcı görüldü. İki kaydedilmiş karede kamera eğimi ve görüş perdesi farklı; bu, bütün hareket döngüsünün konforunu ölçmez. Gerçek el/zemin geometrisinde eski üçgen kusurunun aynı pozu yeniden üretilmedi. Kalıcı onarım kabulü UNVERIFIED kalır.

Müzik açık ayarı görüldü. FPS müziği ve deprem sesi bu tur dinlenerek değerlendirilmedi; önceki teknik PCM/RMS kanıtı işitme kabulüne yükseltilmedi. Tam kazanma, kesintisiz fiziksel 45sn erzak/sığınak rotası, tüm afetlerde gerçek sığınak, uzun dalga dengesi, insan eğlenme/okunabilirlik/konfor kabulü açık.

Son player.log taramasında runtime exception/error eşleşmesi 0. Bu, oynanış sorunlarının bulunmadığı anlamına gelmez. Bağımsız eski FPS projesi, lisanslar, gerçek skor dosyaları ve önceki temizlik hedeflerine dokunulmadı; yeni ücret/sağlayıcı/Unity build/GitHub yayını yok.

## Kayıtlar

- `Evidence/GamejamOrchestra/TLTP-V55-PLAYREVIEW-20261018/manager/interactive-launch.json`: PID, exe, izole skor hedefi ve DLL.
- Aynı manager'da `os-observations.json`, `identity-verification.json`, `final-review.json`, `captures/` ve `interactive-97825cf1-fd32-4db2-baad-7e002d55d3e3/player.log`.
- Aynı inceleme kökünde `staging/gameplay/REVIEW.md`, `staging/story/REVIEW.md`, `staging/cinema/REVIEW.md`: kaynak ayrıntıları ve ayrı kanıt sınırları.

