# V4.9 Jury adaptörü ve gerçek OS testleri

Güncelleme: 2026-10-17T08:45:39.536797+00:00. Kullanıcının yeni doğrudan isteğiyle canlı adaptör uygulandı, tek Unity build alındı ve gerçek klavye/fareyle otomatik yapılabilen alt senaryolar yürütüldü. FPS ve açık V4.8 PID44264 korundu.

Jury regresyonu **189/189 PASS**, runtime error0, gerçek **exit0**. Önceki gizli pencere denemesi186/188 veexit1 ham olarak korunur: iki HUDpreview kontrolü siyah render fixtureında yoktu. AynıDLL görünür pencereyle geçti; ürün kuralı/eşik gevşetilmedi. Güncel modelde **5510/5510 MODEL** kontrol de tekrarlandı; UnityModelShim kullanan bu sonuç native değildir.

DLL SHA256: `A29F0DF6DE2D1E14FF2958382BDF314E6CFAC3F2B7130FD8C267FE1E55387895`. Kaynak166dosya/build-sonrası drift0; lisans12dosya birebir. Portable CRC ve içDLL eşleşti; ZIP SHA256 `8F40F65570ED16085BA16E37CAB72A1C8E0A2A242DBA70ECEA85B9BABB93C4D9`.

## Gerçek OS kapsamı

`@oai/sky` SendInput tuş/fare girdileri, zaman damgalı ekranPNGleri ve opt-in salt gözlemci UnityInput/state JSONL birlikte kaydedildi. Gameplay API/HandleKey/Warp/Tick enjeksiyonlu harness kullanılmadı. Motor stage renderları ile gerçek OS ekranları ayrı dosyalardır. Kesintisiz video ve insan oynanış/dinleme kabulü yoktur.

- Dört afet klavye/Enter başlangıcı, kart/fare başlangıcı, ok sarma ve Backspace/Esc dönüşü.
- Deprem52.9522, Sel63.6093, Yangın52.2092 ve Fırtına74.2262 gerçek doğal hasarı. Her birinde erişilebilir kapı3 montajı120bütçe; bu önlem drenaj/yangın istasyonu/çatı diye sayılmadı. Fırtına>%65 gerçek kayıp; hasar incelemesi ve aynı-afet retry temiz.
- Sel doğal ve çift tık sahne atlama aynı63.609291hasar/tek su-gıda tüketimi/0ödül verdi. Sonradan Enter yalnız inceleme noktasını ilerletti.
-90sn simülasyon hazırlığı müdahalesiz bittiğinde açık süre-kaybı ve inceleme; kısa akışın doğal süre/eksik hedefleri; F2 menü ve Kaybetmeme alt kontrolleri ham ledger ve zaman geçişleriyle bağlıdır. Ayrıntılı zamanlar [os-analysis.json](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/os-analysis.json) içindedir.

BasılıW/D desteklenmedi: down/up aynıUnityframe ve yaklaşık0.02m hareket. Tam toplama/ankraj/merdiven/sığınma/yangın istasyonu ve12dalga rotaları **tamamlanmadı**. OSK UIAccess engeli aşılmadı; kullanıcı yardımcı pencereyi kapattı. Başarılı yürüyüş veya insan ilk180sn keyif kabulü üretilmedi. Exact last-frame yarış/basılıEnter açık kalır.

## Atanmış28vaka

Sonuçlar: {'PARTIAL': 13, 'NOT_RUN': 5, 'PASS': 10}. PARTIAL tam vaka kabulü değildir. MEC118/SYS020 eski ikiDLL FullFlow incelemesidir; A29DLLde yeniden çalıştırılmış sayılmaz. Kaynak ikiPASS alt katman incelemesidir.

| Vaka | Sonuç | Gözlenen katman |
|---|---|---|
| MEC-001 | PARTIAL | OS_E2E_PARTIAL |
| MEC-002 | NOT_RUN | SOURCE |
| MEC-003 | PARTIAL | OS_E2E_PARTIAL |
| MEC-004 | PARTIAL | OS_E2E_PARTIAL |
| MEC-005 | PARTIAL | OS_E2E_PARTIAL |
| MEC-006 | PARTIAL | OS_E2E_PARTIAL |
| MEC-007 | PARTIAL | OS_E2E_PARTIAL |
| MEC-008 | NOT_RUN | SOURCE |
| MEC-009 | NOT_RUN | SOURCE |
| MEC-010 | PARTIAL | OS_E2E_PARTIAL |
| MEC-011 | PARTIAL | OS_E2E_PARTIAL |
| MEC-012 | PARTIAL | OS_E2E_PARTIAL |
| MEC-013 | PASS | SOURCE |
| MEC-014 | NOT_RUN | SOURCE |
| MEC-015 | PARTIAL | OS_E2E_PARTIAL |
| MEC-082 | PASS | MODEL |
| MEC-083 | PASS | MODEL |
| MEC-084 | PASS | MODEL |
| MEC-085 | PASS | MODEL |
| MEC-086 | PASS | MODEL |
| MEC-117 | PASS | SOURCE |
| MEC-118 | PASS | NATIVE_API |
| SYS-020 | PASS | NATIVE_API |
| SYS-021 | PASS | NATIVE_API |
| SYS-037 | PARTIAL | OS_E2E_PARTIAL |
| SYS-038 | PARTIAL | OS_E2E_PARTIAL |
| SYS-039 | PARTIAL | OS_E2E_PARTIAL |
| SYS-040 | NOT_RUN | SOURCE |

[Jury ham rapor](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/jury-v2-1e06a094-8785-4743-a0d0-8ef70e6797ba/jury-smoke.json) · [OS analiz/zamanlar](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/os-analysis.json) · [kaynak/koruma denetimi](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/current-audit.json) · [Portable receipt](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/portable-receipt.json) · [katalog sonuçları](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/../case-results.json)

## Son doğal süre ve koruma kayıtları

- Ayrı girdisiz kısa akış **269.975sn** içinde Complete;300snsonrası aynı hedef/ödül sonucu. Başlatma tıklaması sonrası dünya girdisi0;9eksik/1doğal gözlem tamam,0atlandı,0ödül/puan.
- Öğretici **195.549sn** boyunca Tutorial/preparation180kaldı; doğal kayıp yok. N boş dünya bağlamında temiz Wave1/300sn/1000/boş çanta açtı; envanter açıkken N bitirmedi.
- Kaybetmeme ile doğal300snsonrası gerçek53.2669hasar,0ödül/skor/tamamlanan dalga ve kontrol açık inceleme. Seçenek kapalıyken stickyCheatedRun korundu; Wave2nin doğal300snsonu normal bunker-süre kaybı oldu. Yeniden başlatma hileyi/hasarı/stoğu temizledi.
- İki izole OS process normal kapandı: exit0/exit0. Real/isolated skor hashları korundu; runtime error0. Ham gözlemler için30kanıt-tutarlılık kontrolü geçti; bu30kontrol28katalog vakasıyla toplanmaz. [Kontrol ayrıntıları](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/os-evidence-verification.json).
-75kaynak/74prefab/248input asset/27792Resources/72metadata/24sahne+ayar manifesti bağlandı. ÖncekiQAya göre Jury kaynak değişikliği ve yeni observer+meta dışında asset farkı0. Unity sahnesinin üç yerelObjectIDsi yeniden üretildi: ham scenehashfarkı1, normalize semantik fark0. Buildden sonrasına166dosya snapshot farkı0. [Tam manifest farkı](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/full-manifest-receipt.json).
- Sonuç ekranı hâlâ gözlem dahil10adıma “Tamamlanan önlemler” diyor; önceki UXA001 metin bulgusu kapanmadı. Davranış doğru hedef durumlarıyla kaydedildi; bu görevde ek ürün metni değişikliği yapılmadı.
- EskiQA build/ZIP silmesi otomatik onay incelemesince “blocked by policy” ile reddedildi; hiçbir çıktı silinmedi. Güncel launcher/ZIP ayrı; eskiQA çıktıları ve açıkV4.8 korundu. [Temizlik durumu](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/cleanup-receipt.json).

Güncel launcher/portable paketi aynıDLLyi açar. Önceki QA tüm-suite sayıları tarihsel kayıttadır; buDLLye devralınmaz. Gerçek kayıt hashları aynı, açık V4.8 PID44264 ve FPS korundu. Yeni GitHub yayını yok.
