# Home V5.7 — UI araştırması ve kanıt sınırları

18 Ekim 2026. Başlangıç araştırması [ilk araştırma kaydında](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/staging/research/RESEARCH.md>) saklanır. Eski canlı kaynak bulguları başlangıç sürümüne aittir; aşağıdaki güncel kullanıcı kararları ve sürüm sınırıyla birlikte okunur. Ana menü, yedi duraklatma seçeneği/ses sayfası ve HUD; aynı ikon ailesi, görünür Türkçe etiket, sabit odak ve ortak çerçeve diliyle ele alınır. Kesin son kaynak kimlikleri [UI varlık belgesinde](HOME-V5.7-UI-ASSETS-2026-10-18.md) çalıştırma anındaki dosyalardan üretilir.

## Rehberler ve uygulama karşılığı

| Birincil rehber | Uygulama yönü ve sınırı |
|---|---|
| [Xbox XAG101 — Text display](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/101) | Küçük çözünürlükte açık etiket ve metin hiyerarşisi; gerçek kullanılan font/CalcSize/CalcHeight ve görülen kare ayrı kontrol edilir. |
| [Xbox XAG102 — Contrast](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/102) | Metin/zemin ve odak ayrımı; kaynak RGB hesabı gerçek native piksel ölçümü olarak sunulmaz. |
| [Xbox XAG103 — Additional channels](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/103) | Eksik/hazır ve seçim durumları simge, sayı, sözcük ve odak çizgisiyle birlikte anlatılır; renk tek bilgi kanalı olmaz. |
| [Xbox XAG107 — Input](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/107) | Görünür seçim ve tutarlı klavye/fare eylemi; enjekte GUI geri çağrısı fiziksel OS girişi değildir. |
| [Nielsen Norman Group — Icon Usability](https://www.nngroup.com/articles/icon-usability/) | Sade ve tutarlı ikonlar görünür sözcüklerle desteklenir; anlam hover'a veya yalnız renge bırakılmaz. İnsan tanıma/okunurluk kabulü ayrıca gerekir. |
| [W3C — Contrast minimum](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html) | Renk hesabında bağıl parlaklık ve yuvarlamasız eşik kullanılır; bu web rehberi Unity oyununa genel uygunluk sertifikası vermez. |

## Son tasarımda uygulanan kararlar

Ev sahnesi, mevcut oyun logosu ve hazırlık defteri oyunun kimliğini taşır. Menüde kırık beyaz kâğıt ile mat koyu yüzey kullanılır; sıcak vurgu seçili eylemi gösterir. Çerçeveler düz ve sabittir. Dekoratif parlama, hareketli düğme ve rastgele simge aileleri eklenmez.

| Yüzey | Görünür değişiklik | Oyuncuya yararı |
|---|---|---|
| Ana menü | Solda oyun eylemleri, sağda üç kısa hazırlık adımı ve yerel skorlar; belirgin Serbest Oyna odağı | Oyuna giriş ile bilgi alanının sırası anlaşılır; ilk koşunun boş skor durumu kısa açıklamayla gösterilir. |
| Duraklatma | Kalan süre solda, eylemler ve ses/hareket seçenekleri sağda; aynı odak çerçevesi | Görsel sıra ile klavye seçimi eşleşir; Devam Et ve Başlığa Dön birbirinden ayrılır. |
| Ses ayarları | Ortak çizgide üç slider, tutamaç, yüzde ve açık/kapalı durum sözcükleri | Genel, müzik ve efekt ayarı görünür kalır; çift olumsuz etiket azalır. |
| Oyun HUD'si | Afet/aşama, süre, hasar ve bütçe ayrı okunur bloklarda; eksik malzemelerde Var/Gerek/Eksik sütunları | Oyuncu tahmini maliyet yerine gerçek stok açığını ve sonraki eylemi görür. |
| FPS ve envanter | Su damlası/gıda simgesi + 0/1 veya 1/1 + Eksik/Hazır; tek 20 yuvalı ızgara | Renk yanında sayı ve metin de bilgi verir; ayrı paketleme ekranı gerekmez. |
| Sonuç ve döküm | Hasar katkıları ile bütçe hesabı ayrılır; sayfa ve geri dönüş eylemi görünürdür | Ödül, kesinti ve son bakiyenin nasıl oluştuğu izlenebilir. |

33 özgün simge aynı çizgi diliyle çizilir ve görünür Türkçe etiketle desteklenir. Simge tanıma, insan okunurluğu ve beğeni henüz ölçülmüş kabul değildir. Son ajan incelemesi 81 gerçek PNG'yi kapsar; bunun 59'u ayrıca özgün boyutunda incelenmiştir. Kesin dosya/hash ve örnek renk ölçümleri [son görsel kayıtta](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/visual-review-r4.json>) bulunur.

## R1 kaynak denetimi ve R2 gerçek piksel incelemesi

Eski **25/25 guard** sonucu yalnız R1 başlangıç kaynak/tarif/renk denetiminin tarihsel kaydıdır. Son UI'nin genel uygunluğu, R2 native sonucu, yeni klasik F1 ya da ortak envanter kabulü diye kullanılmaz. [R2 kaynak incelemesi](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/staging/research/REVIEW-R2-SOURCE.md>) de kendi dosya hashleriyle sınırlıdır; kaynak palet oranı gözlenmiş framebuffer oranı değildir.

R2'nin bağımsız incelemesi root tarafından üretilen **20 kritik PNG** ile sınırlıdır; bütün kareler, OS girdisi veya insan beğenisi değildir. R2 ledger'da gerçek opak metin çekirdeği/panel çiftleri için ölçülen oranlar **Rust5,566116:1**, **Sea6,076374:1**, **Muted5,882591:1**'dir. Yalnız bu seçilmiş native renk çiftleri için4,5 hedefiyle karşılaştırılır; anti-alias kenarlarının tamamı, bütün UI ve genel XAG/WCAG uyumu için sonuç çıkarılmaz. [R2 native piksel inceleme kaydı](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/staging/research/FINAL-R2-PIXEL-REVIEW.json>); SHA256 `783DDDD4425C461B34749EE3DA5BCE2CF444FE09216EC380A40339AA41AC706F`, DLL `9E67626099ED0BAE07F9E0AB79A83BB97026771DD3C5F11849B66BA4A5F42B7B`. Kaydın kendi durumu `PIXEL_FIXES_CONFIRMED_SCORE_HEIGHT_NATIVE_FAIL_OPEN`; yeni adayın kabulü değildir.

[Ayrıntılı R2 piksel notu](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/staging/research/FINAL-R2-PIXEL-REVIEW.md>) yatay skor kırpılmasının düzeldiğini, ancak o DLL'nin gerçek skor metni yüksekliği kontrolünün açık kaldığını ayırır. R2 raporunun kendi sonucu yeni DLL'ye yükseltilmez. Aynı şekilde R2 tile F1'in teknik geometri sonucu, sonraki açık insan reddinin yerine geçmez. Yeni font sığma, yeni klasik klavye ve ortak envanter için son kaynaklardan yeni DLL/native kanıtı gerekir.

## Güncel kapsam ve kabul sınırı

F1'in önceki karo/kart tasarımı insan kararıyla reddedildi. Son **r4** kaynak ve DLL'sinde eski tek fiziksel klavye korunur:56 kapak,34 kullanılan tuş,22 dış açıklama grubu ve temsilci tuşlardan çıkan106 ortogonal parça. Gerçek font ölçümleri ve iki çözünürlükte actual Repaint geometri kontrolü yeni DLL'de geçti. Bu, insan okunurluk/beğeni kabulü değildir.

**Tek ortak20 yuvalı envanter** son kaynaklara alındı. Su/gıda doğrudan hazırdır; gerçek World dalga çözümü aynı listeden birer birim tüketir. İki bölmeli ve elle paketleme sunumu kaldırıldı.42 bağımsız model kontrolü,20 gerçek hücre/klavye/dolu görüntü/font ölçümü ve World tüketim/tekrar-sync kontrolleri yeni DLL'de geçti; doğal toplama rotası ve OS drag kabulü diye sunulmaz.

R3'te açık zemin üzerindeki ENVANTER başlığı gerçek PNG'de1,3273:1 bulundu; eski Brush swatch'ları koyu zemin ve tek vurgu çizgisiyle düzeltildi. R4 gerçek iki çözünürlük ölçümü **11,84967:1**'dir; bu seçili opak glyph/zemin çiftiyle sınırlıdır. Diğer bütün UI için genel uygunluk çıkarılmaz. Son DLL `BA49CD61AE45BB29CA68AF29EEEEB8BDED9574C8D7B5B2D48B3EC477D64FD394`; ajan piksel incelemesi `visual-review-r4.json` ve güncel teslimle bağlıdır.

120sn önlem +45sn çanta, gerçek maliyet/ücretsiz iptal, sonlu RNG, %75 kayıp, skor yalıtımı ve gerçek controller/sığınak kapısı korunur. Selde üst panik odası, diğer afetlerde bodrum; gerçek girişin kaynağı `Model.ReachedShelterV5` ve `TryEnterActiveShelterV5` üretim kapısıdır. Görsel hazır etiketi veya sayaç fikstürü fiziksel giriş kabulü değildir.

Kaynak, derleme, native API, framebuffer/piksel, fiziksel OS girdisi ve insan/işitme kabulü ayrı katmanlardır. Yeni sürümün native/OS sonuçları, yeni DLL ve kaynak/ZIP kimliği [güncel teslim](HOME-V5.7-UI-DELIVERY-2026-10-18.md) ve [LATEST](../LATEST-BUILD.json) ile birlikte incelenir. Bu üretici **humanAcceptance=false** ve **listeningAcceptance=false** sınırını korur; kaynak hashinden, eski testten veya kanıt dosyasının varlığından yeni **PASS** sonucu kurmaz.

- Son native kanıtı: [ui-v57-smoke.json](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-ui-9a7e8b18-c4c8-4fa1-902a-671002ae874e/ui-v57-smoke.json>) — SHA256 `0FD31E226BADE6DE7F54B3FBEBDE04385F597FA510050A25C62524ED61F0DB4B`; dosya varlığı veya hash tek başına geçti sonucu üretmez.
- Son native kanıtı: [hybrid-report.json](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-hybrid-6ddcd34c-407e-4e24-940a-7df8056e6f16/hybrid-report.json>) — SHA256 `DB05D77F58507A07A864DC98443E78DE535ABFCB3D8CDA38F0396622BE13EE38`; dosya varlığı veya hash tek başına geçti sonucu üretmez.
- Son native kanıtı: [v56-smoke.json](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-v56-574175bd-8aff-4e99-80f6-37660e7f6c58/v56-smoke.json>) — SHA256 `419407357348AEC2DFDAC7A99E5494C51C9DA4F0F4F282605E645E9EF970E4A7`; dosya varlığı veya hash tek başına geçti sonucu üretmez.
- Son native kanıtı: [wave-ledger-v56-smoke.json](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-ledger-ca1fb961-b7c8-4c30-b190-e783e38974dd/wave-ledger-v56-smoke.json>) — SHA256 `358B230DAF8EA1453BF6ED17EC84F290A71849DACEA71F0AC6E97E8AD31E6AC6`; dosya varlığı veya hash tek başına geçti sonucu üretmez.
- Son OS kanıtı: Bu çalıştırmaya yeni kanıt yolu verilmedi; durum ve kesin DLL bağı teslimde açıkça yazılmalıdır.

## Son değerlendirme

Bu üretici kaynak/hash ve araştırma referansı yazar; test çalıştırmaz, oyuncuya girdi göndermez, build/ZIP/LATEST yayımlamaz ve kabul kararı vermez. Kesin test kapsamı, açık hatalar, yeni native/OS kanıtları ve son teslim kararı [güncel teslim](HOME-V5.7-UI-DELIVERY-2026-10-18.md) içindedir. İnsan okunurluğu/beğeni ve dinleme kabulü bu belgede **false** kalır; yeni bir insan kaydı varsa teslim sahibi onu ayrı ve açıkça bağlamalıdır.

Son teslim sahibinin eki: Windows turu 18 Ekim08:43:43UTC fiziksel Esc ile durduruldu. Kısmi gerçek girdi gözlemleri korunur; dört afet simülasyonu ve yeni normal global profil açılışı tamamlanmış sayılmaz. [Kesin durdurma ve kapsam kaydı](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/os-review-r4.json>) `complete=false`, `passed=false` durumunu korur. Native981/981 ve81 kare sonucu ayrı geçerlidir; durdurma sonrasında GUI/girdi/oyuncu başlatma/kapama yapılmadı.
