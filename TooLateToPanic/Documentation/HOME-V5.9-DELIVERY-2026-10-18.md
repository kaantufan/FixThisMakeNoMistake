> Public publication copy: the selected source manifest differs in scope from the full local build manifest. Native results below describe the recorded package only. OS testing state is a snapshot and may still be in progress. [Download](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.5.9-r5), [source/build comparison](Publications/v0.5.9-r5/source-vs-build.json). Local-only historical evidence links remain local references.

# Home V5.9 — birleşik kullanıcı testi adayı

18 Ekim 2026 · **Home V5.9-r5 /0.5.9**. Menü, ekonomi, süre, dalga sırası, defter işareti, bit altyazı ve ses düzenlemesi tek pakette; V5.8 garaj/NPC/sel düzeltmeleri korunur. Windows derlemesi başarılıdır. Son kullanıcı yalnız yeni V5.9 testini açmayı onayladı. Aynı DLL üzerinde **943/943 native API/render/ses kontrolü**, **90 gerçek görüntü** ve 0 runtime hatası kaydedildi. Fiziksel OS girdisi ve insan kabulü yapılmadı. Kaynak ve Editor kontrolü oyun ekranı, dinleme veya insan kabulü yerine sunulmaz.

[Başlatıcı](../Play-TooLateToPanic.cmd) · [Güncel indeks](../LATEST-BUILD.json) · [Taşınabilir paket](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.5.9-r5)

## Uygulanan davranış

- Ana menüde **Oyna**, **Kısa Oyun**, **Afeti Dene**, **Nasıl Oynanır**; daha kısa, genel yönlendirmeler.
- Başlangıç bütçesi **1500**. Normal önlem **90 sn**, son erzak/FPS **40 sn**; toplam **130 sn**. Model, gerçek aşama kapısı, HUD ve açıklamalar aynı sabitleri kullanır. Kısa akış ve simülasyon ayrı mevcut akışlardır.
- **1.dalga Deprem**, **2.dalga Sel+Fırtına** (Sel birincil),3. dalgadan itibaren eski RNG seçimi. Sel içeren dalgada gerçek üst panik odası gerekir.
- Defterden seçilen mevcut malzeme/erzak hedefi, uygun aşamada gerçek dünya sınırları çevresinde sıcak vurgu ve **BURADA** kartı alır. Ekran dışında yön, başka katta kat bilgisi gösterilir. Modal/iş/sinematikte gizlenir; etkileşim, konum veya fizik değiştirmez.
- Defterin altı sayfası, giriş açıklaması ve yardımcı metinleri Türkçe anlam ve uzunluk açısından incelendi. Türkçe büyük harf dönüşümü **Ev planı → EV PLANI** ve **Fırtına → FIRTINA** üretir. Önlem durumu **Yapılmadı / Kısmen hazır / Hazır**; stok durumu **Toplanmadı / Toplandı / Tükendi** olarak gerçek veriden türetilir. Gerekmeyen bir işte **Gerekli değil** gösterilir. Kaynak durumu için ayrılan100pxalan gerçek fontla doğrulanır. Harita, stok, görev sırası ve buton davranışları korunur; metinler kısaltılır. Bütün sayfalar iki çözünürlükte görüntülenir.
- Şalter/vanada **AŞAĞI İNDİR /90°SAAT YÖNÜ** ve **KAPALI KONUM** gerçek font ölçüleriyle ayrı çerçevelerde yerleşir. Gerçek tutamak, kapalı nokta ve yön halkasının çevresi korunur; çerçeveler birbirini kesmez ve görüntüden taşmaz. Fare yakalama ve iş tamamlama kuralları değişmez.
- Bütün manuel işlerde görsel fare/tuş kapakları bulunur. Çekiçte işaretli yere sol tık veya ayrı **SPACE** basışı; vana, vida ve hortumda tut/çevir veya **→** basılı tutma; şalterde tut/aşağı sürükle veya **↓** basılı tutma; iptalde **ESC** kapağı gösterilir. Gerçek tıklama bölgeleri korunur, basılı SPACE tekrarı eklenmez.
- Normal aşamada düşük seviyeli gerilim; FPS'te daha yoğun fon. Tek müzik sahibi,2 sn geçiş ve6500 Hz yumuşatma kullanır. Daha duyulur nabız ve yeni nefes nefese kalma döngüsü, kısa fade ve mevcut ses ayarlarına bağlıdır. Önceki sabit FPS kamera davranışı korunur.
- Konuşma yerine kısa, sözsüz bit sesleri; metin hemen tamamı görünür, mevcut HUD font/renk/çerçevesine uygun, iki satırlık altyazı alanında gösterilir. Modal/pause/mute ve tekrar önleme tek sahipte kalır.

## Ekonomi ve afet hesabı

Başarılı dalga temel ödülü 1600+100×(dalga−1); mevcut gerçek hasar kesintisi ve destek bonusu ardından uygulanır. İlk güçlendirme30 bütçe+1 tahta+1 vida; ikinci duvarda 375, diğerlerinde 200bütçe+2 tahta+4 vida. Evde bütün kalıcı işleri tamamlamak 9705bütçe,162 vida ve92 tahta gerektirir. Sonlu vida/tahta yığınlarına 2 birim eklenir; aynı dalga yeniden stok yaratmaz. İlk dalga bütçesi bütün evi bitiremez.

Kullanıcının açık ek yetkisiyle çoklu afet toplamı, en yüksek gerçek risk + her ek afet için ağırlıklı ortalamanın%3katkısı olarak dengelenir; katkı defteri aynı ortak ölçekle korunur. Tek afet riski, dalgayla artanV5.2 eğrisi ve%75 kayıp sınırı korunur. Ödül/erzak bir kez çözülür; F2/F3 skor dışı ve ödülsüz kuralları korunur.

Son90+40ve sabit ikinci dalgayla 200 detached gerçek-model senaryosu yeniden çalıştırıldı. Dalga başına 10/14 kaynak ve2 destek fixture'ında 40/40 senaryo 7. dalgayı kayıpsız bitirdi;11/14 için 40/40. Tam toplamada 32/40 senaryo 7, 8/40 senaryo 6 sonunda bütün kalıcı işleri tamamladı. Düşük toplamada eksik ilerleme ve gerçek kayıp görülür. Bunlar üretim model API'leriyle, uzak toplama/sığınak ve sayısal motor fixture'larıyla hesaplanan ekonomik ufuktur;90 sn fiziksel rota veya insan başarı garantisi değildir. Kaynak: `staging/economy/final-policy-addendum/FINAL-POLICY-RECEIPT.json` ve bağlı detachedmodel raporu.

## Yeni kontrol ve paket kimliği

Gerçek Unity 6000.0.73f1 headless Editor'de **474/474** kontrol geçti: yeni saat sınırları, dalga 1→2, 64 sabit ikinci dalga tohumu, 1152 dalga 3+ RNG karşılaştırması, ekonomi, çoklu afet, katkı defteri, 20 yuva envanter, V5 tesisat ve V5.2 risk regresyonları, gerçek ses importları ve defter yerleşimi. Alt kontroller bu 474 başlığın içindedir; sayılar bağımsız testler gibi üst üste toplanmaz.

Yazı düzeltmesinin gerçek BarlowCondensed fontu ve gerçek authored vana/şalter art'ıyla **273 pozda 2185/2185** projeksiyon/metin/çakışma sınırı geçti;960/1280/1600/1920 boyutları hesaplandı. İç içe geçmeme geometri kanıtıdır; framebuffer ve insan okunurluğu değildir. V5.8'in aynı retained art'ı üzerinde68/68 geometri/Avatar/sel kontrolü yeniden geçti.

Derleme:0 hata,0 uyarı. 212 C#/shader ve97 ses dosyası donduruldu; kayma 0. Paket167 dosya içerir; ZIP'in her dosyası build manifestiyle byte/hash eşleşir. Tüm kaynak manifesti41922 dosyadır. Gerçek skor/ses tercihi ve korunan oyuncu gözlemleri manager receipt'lerinde ayrıdır; eski açık oyuncuya girdi/sonlandırma gönderilmez.

| Öğe | SHA256 |
|---|---|
| Assembly-CSharp.dll | `1823D53AE0516C48A36085884377C0705EDBB7401FDF6AADE7CD2DB990729274` |
| EXE | `D26B0F57817DB378557C445524C48D28EE559FB2BE53CA5DC63E37C1E4AD22F5` |
| ZIP | `4738918CAF0BC26150ACC5F72C331D2648633661EB32708677F75D67DF9EAFBF` |
| Kaynak manifesti | `9C70B4A44E73A8AD93DB7CF7E81857CF91FEA08D31A3245722730F86422AB098` |
| Build manifesti | `4C5FFEACE4C6E28B09B6B4664BBCAFAF514FE2F07D874403C919032AD940D3A4` |

Yeni ses üretimi:11 gerçek ElevenLabs WAV kaynağı,21 işlenmiş runtime WAV,**457 mevcut kredi**; yeni ödeme/abonelik/overage yok. Önceki gerçek gerilim müzikleri yeniden mikslenir. Klip PCM ve stream import metadata/offline PCM ölçümleri, çalınan çıktı ve insan dinlemesinden ayrıdır. [Ses/asset kaydı](HOME-V5.9-AUDIO-ASSETS-2026-10-18.md).

Eski native PASS sayısı bu DLL'ye aktarılmaz. V56'nın saat fixture'ı kanonik 40 saniyeye, audio oracle'ları gerçek iki saniyelik geçişe göre güncellendi; güncel üç suite bu DLL üzerinde yeniden çalıştı. Eski V51/V54/V55 ve Wave50 suite'ları çağrılmadı. Fiziksel OS girdisi, insan dinleme/konfor/okunurluk ve 90 saniyelik fiziksel 6–7 dalga kampanya kabulü **NOT_RUN**. Eski bağımsız FPS projesi ve açık oyun korunur. Önceki build/ZIP'ler yeni native kabul olmadan temizlenmez. GitHub yayını yapılmadı.

## Aynı DLL native kontrolleri

| Suite | Kontrol | Görüntü |
|---|---:|---:|
| ui-v57-smoke | 527 | 68 |
| v56-smoke | 207 | 21 |
| audio-smoke | 209 | 1 |

Her suite taze GUID skor/ses profili kullanır. Menü ve envanter GUI çağrıları, gerçek iş/session API girişleri, açık konum/saat/afet/render fixture'ları ve kaynak/listener PCM ölçümleri ayrı kaydedilir. Bu test doğal yürüyüş veya insan dinlemesi değildir. Üç tesisat ekranında 960×540 ve 1600×900 gerçek Repaint etiketleri, kapalı nokta, yön halkası ve tutamak ayrılığı incelendi. Raporlara bağlı bütün framebuffer görüntüleri root tarafından incelendi. Defterdeki Kısmen hazır/Hazır durumları bu sıfır ilerleme ekranlarında görünmez; durum hesabı Editor kontrolleriyle doğrulanır. Önceki açık kullanıcı oyuncusuna girdi/sonlandırma gönderilmedi. Kaynak ve build/ZIP kimliği aynı yeni DLL'ye bağlıdır.
