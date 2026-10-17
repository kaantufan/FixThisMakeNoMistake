# Home V4.10 — birleşik arka plan QA teslimi

17 Ekim 2026. Bu rapor **Home-V4.10-Combined-r7-20261017 / 0.4.10** adlı dondurulmuş tarihsel pakete aittir. Güncel V4.11'in kaynaklarını veya kabulünü kapsamaz. Aktif LATEST, iki README, iki başlatıcı ve atlas sahipliği V4.11 sahibine devredildi; bu teslim bunları geriye yazmadı.

Computer Use, fiziksel klavye/fare girdisi ve pencere aktivasyonu kullanılmadı. Testler Unity 6000.0.73f1, Built-In Windows player içinde, ayrı GUID profillerinde yürütüldü. Kaynak, model, native API, render, OS ve insan kabulü ayrı tutuldu.

## Sonuç

Fonksiyonel kontroller ve uzun yaşam döngüsü tamamlandı. Player normal biçimde **exit 0** ile kapandı; çalışma zamanı hatası yok. Gerçek skor dosyasının SHA256 değeri değişmedi. **Bellek artışının kök nedeni açık**, ses örtüşmelerinin kaynağı belirlenmemiştir; bu teslim sızıntısızlık kabulü vermez.

| Katman | Geçen / rapor satırı | Exit / çalışma zamanı hatası |
|---|---:|---:|
| Model / skor biçimi | 200/200 | 0 / 0 |
| Ses yönlendirmesi ve teknik ses kontrolleri | 160/160 | 0 / 0 |
| Doğrudan işler | 44/44 | 0 / 0 |
| Görsel ve metin | 193/193 | 0 / 0 |
| Kısa akış regresyonu | 189/189 | 0 / 0 |
| Kesintisiz rota | 141/141 | 0 / 0 |
| Mekanik sınırlar | 338/338 | 0 / 0 |
| Fizik | 24/24 | 0 / 0 |
| Uzun yaşam döngüsü | 201/201 | 0 / 0 |
| Kısa yaşam döngüsü | 9/9 | 0 / 0 |

Bu sayılar toplanmaz. Mechanics içindeki 338 satırın **295'i MODEL, 43'ü NATIVE_API** katmanındadır; tamamı fizik testi değildir. Model fixture'ları, kurulum Warp'ları, iş Tick'leri ve mantıksal IMGUI pointer girdileri ilgili ham raporlarda açıklanır. Beklenen dört negatif CLI reddi ayrı geçti; bunlar pozitif native kabulüne eklenmedi. NaN girdisinin kasıtlı FALSE satırı ve exit 1 kaydı, diğer üç reddin exit 2 kayıtları korunur.

Kesintisiz fullflow rotası 141/141 geçti. Kısa akış 39,03 saniyede tamamlandı; açık Warp, enjekte Tick ve geliştirici mutasyonu sıfırdı. Bu üretim API rotasıdır; fiziksel OS girdisi veya insan oynanışı değildir.

## Uzun yaşam döngüsü

Rapor süresi **1870.87 saniye**; sürücünün duvar süresi 1874,15 saniyedir. Döngü süresi kontrolü 1869,542 saniyeyi doğruladı. **18 tam döngü, 72 doğal afet sonucu, 72 gerçek inceleme geçişi ve normal kampanyada 18 boş çanta kaybı** görüldü. Hazırlık hedefleri başarılmış sayılmadı: bu soak rotası kısa akışın uygun bölümlerini açıkça atladı, sonra deprem/sel/yangın/fırtına sonuçlarını doğal saatlerle bekledi.

201 doğru assertion satırında **200 benzersiz kontrol kimliği** vardır. Son 18. döngünün census örneği iki kez yazılmıştır; bu tekrar bağımsız senaryo sayısını artırmaz. Ham rapor değiştirilmedi. Bellek eğimi hesaplanırken aynı döngünün son örneği kullanıldı.

Yaşam döngüsünde test sürücüsünün Warp, enjekte Tick ve geliştirici mutasyonu sıfırdı. Gerçek hasar, başarısızlık nedeni, sıfır ödül ve kayıp kuralları korundu. İzole ana profile meşru sıfır dalgalı sonuç yazılabilir.

Gerçek skor SHA256, önce ve sonra:

`DF2355AADB092174A5167460E436FD64F83B3A280C3B05B87F2E28EBA47A40AD`

## Açık bulgular

**PERF410-001 — OPEN:** Isınma sonrası Unity allocated bellek 174.611.448 → 211.837.951 byte yükseldi; doğrusal eğim yaklaşık **2,53 MiB/döngü**. Windows dış örnekleyicisi 371 pozitif working-set/private ölçümü aldı; Mono'nun sıfır process-memory alanları ölçüm sayılmadı.

Isınma sonrası gözlenen sayılar sabitti: 20 AudioSource, 2338 GameObject, 5748 Component, 44 Texture, 7 RenderTexture, bir müzik bileşeni, iki müzik kaynağı ve 43 karakter görünürlük renderer'ı. Bu sabitlik bellek artışını açıklamaz. QA raporları, sınır GC'leri ve eşzamanlı V4.11 editor/build yükü ayrıştırılmadı. Bağımsız GPU/performance testi ve bütün Mesh/Material/event-delegate census'u yapılmadı.

Frozen kaynak incelemesi aktif HUD/ses yollarında sınırsız tutma göstermedi. Legacy PanicArt registry ve üretilen mesh sahipliği açıkları bulundu; bunların bu Home soak rotasında çalıştığı kanıtlanmadı. Bu nedenle artış o kodlara atfedilmedi. [Bellek grafiği](../../Evidence/GamejamOrchestra/TLTP-VISUAL-TEXT-QA-20261017/manager/soak-memory.png), [sayısal analiz](../../Evidence/GamejamOrchestra/TLTP-VISUAL-TEXT-QA-20261017/manager/soak-analysis.json), [kaynak incelemesi](../../Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/plan-direct-actions-v49/world-review/final-combined-review/V410-SOAK-RETENTION-SOURCE-REVIEW.md).

**AUD410-001 — UNATTRIBUTED:** Döngü 1, 2, 4 ve 14 sınırlarında duplicateAudio=true iken musicPlaying=1 görüldü. Bunlar iki müzik kaynağının crossfade'i olarak açıklanamaz. Sabit sekiz Foley kaynağında aynı klibin kısa aralıkla üst üste çalması mümkündür; örnekler klip/sahip ayrımını tutmaz. Bu boolean tek başına ses sızıntısını da kanıtlamaz.

## Uygulanan düzeltmeler

Hasar kartlarında gerçek hedef adı, kat ve katkı yüzdesi gösterilir; ayrıştırma ve lider çizgisi yerleşimi eklendi. Vida paneli gerçek hedefi ve duvar/zemin yüzeyini yazar. Terminal ekrandaki yanlış E istemi kaldırıldı; faz başlıkları ve “Tamamlanan adımlar / dalga” metinleri düzeltildi. Sel/yangın incelemesi gerçek hazırlık ve hasarı gösterir; yeni afet evresi veya ek bekleme üretilmedi.

Orijinal V2 karakter görünümü korundu. Derinlik/stencil işareti gizli beden bölümlerini görünür kılar; ayakkabı–zemin temasındaki yanlış vurgu giderildi. Son 1600×900 ve 960×540 açık zemin çiftlerinde piksel farkı sıfırdır. Bağımsız r7 incelemesinde 12 karedeki 33 hasar kartı kesişmedi; tüm ad/kat eşlemeleri frozen Model ile uyumlu. Ekran dışına sıkıştırılan kartların gerçek nesneye çizgi bağı **PARTIAL**; aynı kareye ait serialized item değerleriyle yüzdeyi yeniden hesaplama yapılmadı. [Görsel inceleme](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/04-visual/R7-INSPECTION-REVIEW.md).

JsonUtility'nin bazı yanlış/null numeric alanları sıfıra çevirmesine karşı dar skor biçim denetimi ve schema 1 kontrolü eklendi. Optional legacy alanlar korunur; bulunan alanların yanlış tipleri elenir. Bilinmeyen metadata için 32 derinlik sınırı vardır; tam sayı alanlarında lexical integer beklenir. Yükleme sırasında dosya yeniden yazılmaz. QA tek sürücü, yeni GUID ve mevcut dosya koruması Model oluşturulmadan önce çalışır.

54 Foley WAV, 42 ses ailesi, 16 adım varyasyonu ve iki adet 60 saniyelik müzik eklendi. Konuşma yerine retro blip ve yazı makinesi altyazısı kullanılır. Yedi önceki konuşma WAV'ı ve metadata'sı Resources dışında byte aynı arşivlendi. Müzik 1,2 saniyelik iki kaynak geçişi ve oturumluk pause menüsü aç/kapat seçimi içerir. Son 54 WAV'ın örnek sayısı, kanal, sample rate ve metadata GUID'leri korundu; clipping sıfırdır. Teknik AudioSource/PCM/import kanıtı dinleme kabulü değildir.

Sağlayıcı işi ayrı yetkili ses sohbetinde 1276 efekt + 5400 müzik = 6676 mevcut krediyle yapıldı; yeni ödeme yok. Üçüncü üretilen müzik indirilemedi ve NOT_INTEGRATED kaldı. [Asset/ses eki](HOME-V4.10-ASSET-ADDENDUM.md) ve paket LICENSES klasörü provenans/OFL kayıtlarını taşır. Yeni lisans icat edilmedi.

## Kimlik, paket ve korunma

- DLL SHA256: `107446B69C4C8F08FE011DF62EC2EE34A2EEB76281A80B4D1A26BFD8DF42AD7F`
- EXE SHA256: `0E1B64A3C0EBA3482F4C7B57F36CF8811997C0CE3BBED914C116CB9D4799FC7C`
- ZIP: `Builds/TooLateToPanic-Home-V4.10-Combined-20261017-portable.zip`
- ZIP SHA256: `B0C5A47843735A8E2356A0909282F0EE09D4889CD086DB5439D6483EEEEA5AFF`
- ZIP boyutu: 151,729,413 byte; 149 dosyada CRC ve çıkartılan bütün byte/hash eşleşmesi doğrulandı.

Portable doğrulama dosya bütünlüğüdür; çıkartılan paketten yeni player başlatılmadı. Ayrı temiz Windows kurulumu veya kullanıcı çift tık kabulü verilmez.

90 source/shader/font girdisi, mevcut 74 prefab ve OS observer bağı korundu. 28.432 girdi toplam 1.263.284.279 byte bağımsız kopya olarak donduruldu; tamamı manifestle eşleşti. Native 139 dosya test öncesi/sonrası aynıydı. README ve lisans ekleriyle ZIP 149 dosya oldu. [Kaynak manifesti](../../Evidence/GamejamOrchestra/TLTP-VISUAL-TEXT-QA-20261017/manager/source-combined-v9/source-manifest.json).

Aktif kaynak/Unity sahipliği V4.11 köküne devredildikten sonra bu QA canlı kaynak, Unity/import/build veya yeni player işlemi yapmadı. Aktif V4.11 LATEST/README/başlatıcı/atlas dosyalarının altı hash'i tarihsel paketlemeden önce ve sonra aynıydı. V4.10 için yalnız ayrı build indeksi ve tarihli belgeler yazıldı.

Kullanıcı V4.8 süreci PID 54856 bu QA tarafından kapatılmadı veya aktive edilmedi. 11:09 UTC gözleminde artık listede değildi; dış neden/aktör UNKNOWN olarak kaydedildi. Yeniden açılmadı. V4.8 DLL SHA256 `5683E713C90F2FD070DB819D14526EBA74ACC25B65C6581DABD6FB602F6BD17E` korundu. Son gözlemde QA player'ı da normal çıkmıştı; süreçler ve korunma kayıtları manager alanındadır.

## Kabul sınırları ve retained tanılar

OS/HUMAN/dinleme, ilk 180 saniyenin öğrenme/keyif kabulü, tam kampanya, tüm poz/transparent/animasyon matrisi, ekran dışı lider uçları, bağımsız GPU/performance ve temiz/offline Windows **NOT_RUN veya PARTIAL** kalır. Eski 505 satır, 5510 managed-shim sonucu ve önceki DLL'lerin OS kanıtı bu DLL'ye taşınmaz.

Önceki başarısız adaylar korundu: eşzamanlı kaynak drift'i, eski Unity importer preload API'si, texture alanı olmayan materyalde QA sorgusu, ayakkabı temas pikselleri, eski pause fixture'ı, altyazı ilk harf gecikmesi, portre ekranda 1080 genişliğe sıkışma ve ilk shader gecikmesinde kurulumun yere oturmaması. Gerekli dar düzeltmelerden sonra son r7 tekrar test edildi; maliyet, collision, hasar veya kayıp eşikleri gevşetilmedi.

Eski çıktı temizliği otomatik onay incelemesinde **“blocked by policy”** ile reddedildi. Silme veya alternatif silme denenmedi; çıktılar korundu. Yeni GitHub yayını yapılmadı.

[Nihai receipt](../../Evidence/GamejamOrchestra/TLTP-VISUAL-TEXT-QA-20261017/manager/FINAL-RECEIPT.json), [V4.10 bağımsız build indeksi](../../Evidence/GamejamOrchestra/TLTP-VISUAL-TEXT-QA-20261017/manager/V410-BUILD-INDEX.json), [katman özeti](../../Evidence/GamejamOrchestra/TLTP-VISUAL-TEXT-QA-20261017/manager/aggregate-final-r7/SUMMARY.md), [root doğrulaması](../../Evidence/GamejamOrchestra/TLTP-VISUAL-TEXT-QA-20261017/manager/FINAL-POSTPROCESSING-VERIFICATION.json).

QA ekran seçimi [Unity'nin monitor parametresi](https://docs.unity3d.com/6000.0/Documentation/Manual/PlayerCommandLineArguments.html) ile yapıldı; masaüstü ayarı veya kullanıcı penceresi değiştirilmedi.

## Bağımsız final inceleme

[06-system final incelemesi](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/06-system/FINAL-V410-SOAK-INDEPENDENT-REVIEW.md) aynı DLL, frozen kaynak, exit ve rapor hashlerini yeniden bağladı. 18 döngü/72 doğal sonuç/72 inceleme/18 kaybı ve açık bellek/ses bulgularını doğruladı.

Kategorinin eski plan/vaka indeksindeki 16 PASS, 1 kaynak FAIL, 25 NOT_RUN ve 1 BLOCKED sayıları bu native assertion sayılarıyla değiştirilmedi. SYS-004 kaynak FAIL kaydı eski legacy launch-guard açığının tarihsel bulgusudur; staged düzeltme incelemesi ve son V4.10 negatif launch kanıtı ayrı kayıtlıdır. Bu rapor bütün atanmış OS/E2E vakalarının geçtiği iddiasını vermez.
