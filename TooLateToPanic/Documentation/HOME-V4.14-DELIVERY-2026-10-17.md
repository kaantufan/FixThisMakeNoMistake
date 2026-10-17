> Public publication copy: the selected source manifest differs in scope from the full local build manifest. Native results below describe the recorded package only. OS testing state is a snapshot and may still be in progress. [Download](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.14-r8), [source/build comparison](Publications/Snapshots/20261017T212502Z/source-vs-build.json). Local-only historical evidence links remain local references.

# Home V4.14-r8 — 17 Ekim 2026 teslimi

**Kullanıcı oynanış testine hazır aday; tam fiziksel OS turu doğrulanmadı.**

Son kullanıcı kararıyla **120 saniye önlem + 45 saniye FPS çanta = 165 saniye**. Önceki 30 saniye çanta hedefi güncel değildir. Gerekenler 1 su + 1 gıda; yüzde75 ve üzeri hasarda kayıp. Kısa hazırlık senaryosunun270 saniye içerik+30 saniye yedek sınırı korunur, kendi son45 saniyesi çantadır.

## Oyuncuya görünen değişiklikler

- Ana ve varsayılan menü SERBEST OYNA. İlk normal girişte saat, karakter ve hikâye durur. Q düzeni klavye şemasında hareket, koşma, zıplama, etkileşim, toplama, envanter, defter, Q hedef seçimi ve iptal tuşları yerleri ve işlevleriyle gösterilir. Enter/Esc ya da görünür düğmeyle bir kez onaylanınca giriş hikâyesi ve normal tur başlar.
- Evin zemin katında salon, mutfak, WC, kiler, garaj ve merdiven; üst katta ebeveyn/çocuk yatak odaları, çalışma odası, banyo, hol ve balkon; bodrumda belirgin sığınak bulunur. Çanta merdiven yanında, pencereler sekiz adettir. Çatı erişimi gerçek colliderlarla kapatılır; balkon/garaj/merdiven geçişleri korunur.
- Çalışan eski humanoid Avatar ve UAL animasyonları normal hareketin tek sahibidir. Yeni sivil yeşil ceketli görünüm aynı rig üzerindedir. NPC ve araçların gerçek mesh ön yönü hareketle eşleştirilir. Kullanıcının reddettiği deneme rig'i aktif karakter değildir.
- İki gerçek vida saat yönünde fare basılı döndürülür. Çekiç dört ayrı Space/tıklama vuruşudur. Gaz/su çeyrek turlu vanalarında açık/kapalı kol yönü tutarlıdır; şalter aşağı sürüklenir. Eşdeğer sağ/alt ok girdileri vardır. İptal ücretsiz, tamamlamada tek maliyet/commit korunur.
- Duvar yükseltmesi seçili katın duvarlarına tek işlemde uygulanır. Gerçek etkilenen adet, hedef seviye, toplam bütçe ve malzeme önce görünür. Dört T0 duvarın T1 maliyeti480 bütçe+4 tahta+8 vidadır; pencereler/kapılar/çatı bu batch'e girmez. Değişen veya yetersiz durumda kısmi yükseltme olmaz.
- Asset seçimi gerçek yüzey/siluet üzerindedir; boş zemin sınırı yoktur. Toplama nesnesi hafif parlar. Malzeme kazanımı karakter yanında gerçek asset görseli ve gerçek +adet ile görünür; kaynak HUD'siyle kesişmesi ayrılır. Önlemde yapı malzemeleri, çanta aşamasında açıkta erzaklar görünür; FPS'de dolaplar açılmaz.
- Son45 saniyede aynı oyuncu ve gerçek konumdan FPS'ye geçilir; hafif karartı, sarsıntı, nabız/nefes ve sınırlı hareket/bakış zorlaşması vardır. El/kol hareketleri gerçek yürüme, koşma, zıplama ve toplama durumuna bağlıdır.
- Gerçek Yarkın bunker kapağı önce açılır, çanta açık şafttan atılır, karakter atlar ve baş açıklığı geçtikten sonra kapak kapanır. Dış deprem çekiminde çatılı ev birlikte sarsılır; fizik/Model geometrisi sabittir. Sel, yangın, hortum ve enkaz efektleri; afet öncesi sivil trafik ve sonrasında dört hizmet aracı/ekibi bulunur.
- Köprü korkulukları ve dere sınırları fiziksel engeldir. Çit inşası ve etkileşimsiz ayaklı lamba kaldırılmıştır. Malzeme eksikleri seçili gerçek tariften hesaplanır. Konuşma yerine bip/altyazı, afet/iş/ortam efektleri ve müzikler kullanılır.

## Araştırma ve kapsam

Klavye tasarımı [Microsoft XAG101](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/101), [102](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/102), [103](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/103) ve [107](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/107) önerilerinden yararlanır: okunur metin/glif, opak kontrastlı panel, renk yanında şekil/yazı ve tutarlı giriş. Bu tam erişilebilirlik sertifikasyonu değildir. Q fiziksel tuş şeması dinamik OS yerelleştirmesi veya yeniden tuş atama özelliği sayılmaz. Mini oyun incelemesinde XAG107/109 ve [Swagelok vana işlevleri](https://edmontonsolutions.swagelok.com/blog/bid/343278/matching-type-to-function-a-swagelok-valve-tutorial) kullanıldı; 90 derece bütün vana türleri için genellenmez.

## Son adayın kanıtları

| Katman | Gerçek sonuç ve sınırı |
| --- | --- |
| Unity6000.0.73f1 / Built-In Windows | Succeeded; 0 hata, 0 uyarı |
| Son DLL native rehber/giriş/çözünürlük | 41 kontrol geçti, 0 runtime hata; yalnız rehber kapsamı, tam mekanik bu DLL üzerinde yeniden çalıştırılmış değildir |
| Korunan R7 mekanik regresyonu | 431/431,0 runtime hata; ayrı R7 DLL. UI ve QA kapsam seçicisi dışındaki125kod/shader dosyası birebir aynı; yeni DLL tam mekanik kabulü yerine sunulmaz |
| Native ses | 206 kontrol geçti; import/PCM/AudioSource/yüzey seçimi, insan dinleme NOT_RUN |
| Native deprem sekansı | 55 kontrol, 22 gerçek görünüm karesi; pause/skip/reset ve render proxy temizliği |
| 50 dalga | 50/50 tamamlandı; genel rapor FAIL: yalnız değişmemiş normal havuzdaki fırtına eksikliği. Hızlandırılmış native Model/Expedition girdisidir; 50 fiziksel OS turu değildir |
| Gerçek OS turu | **TAMAMLANMADI.** Tekrarlayan Windows foreground kayıpları ve bir büyüyen JSONL kayıt okuma hatası giriş betiklerini durdurdu. Denemeler saklandı; tam tur PASS yok. Aynı R8 DLL ile ayrı denemelerde rehberin gerçek girdiyi/saati durdurması, Q seçimi ve iki fiziksel fare vidasıyla60 bütçelik sabitleme gözlendi. Bunlar tam tur kabulü değildir. Tarihsel R4 tam OS turu yeni DLL yerine kullanılmaz. |
| Gerçek kayıt | Bütün QA ve OS önce/sonra skor hashleri aynı; izole profiller |
| Taşınabilir ZIP | Test edilen EXE ve DLL byte/hash eşleşmesi; lisans/provenance metinleri içinde |

DLL SHA256: `4740C599AB0CBEC9EC1FA7D01DFCB8E61308A1F71CA888F15C9869FE89BDA16A`.

Kaynak/build manifestleri, rapor yolları, süreler ve ZIP SHA256 [LATEST-BUILD.json](../LATEST-BUILD.json) içindedir. Kod/shader dondurulmuş dosya sayısı 127; tam kaynak manifesti Assets/Packages/ProjectSettings kapsamındadır. Final normal pencere ayrıca açılır. Eski sonuçlar yeni DLL kabulü yerine kullanılmadı. R6'nın ilk gizli pencere testi siyah backbuffer nedeniyle INVALID_RENDER_SURFACE olarak ayrı tutuldu; aynı R6 DLL görünür koşuda geçti. Tarihsel başarısız/iptal edilmiş denemeler korunur.

## Bilinen sınırlar ve kullanıcı testi

Normal dalgalar1–3 deprem,4–6 sel,7–9 yangın, sonra bu üç türün karışımıdır. Fırtına/hortum AFET SİMÜLASYONU üzerinden oynanır; normal RNG havuzuna eklenmedi. İlk fırtınayı doğrudan havuza eklemek mevcut sonlu kaynaklarla75% sınırını aşan kayıp üretebildiği için maliyet/hasar/fizik test geçsin diye gevşetilmedi. Bu eksiklik50 dalga raporunda açık FAIL kalır.

Teknik geçiş insan yürüyüş, el anatomisi, ses miksajı, rahatlık/keyif veya bütün kampanya kabulü değildir. AI görsel incelemede bükülen dizde küçük sert kıyafet birleşimi notu kalır; rig/gait yeniden tasarlanmadı. Kullanıcı son paketi kendisi deneyip yeni sorunları bildirecektir. Kaynaklar, gerçek kayıtlar ve açık kullanıcı oyunu korunur; önceki reddedilmiş build temizliği yeniden denenmedi. Bu root GitHub yayını yapmadı. Bağımsız eski FPS projesi değiştirilmedi.

Asset kaynakları/lisanslar ve yetkili5003 mevcut ElevenLabs kredisi [asset kaydında](HOME-V4.14-ASSETS-2026-10-17.md); yeni ödeme yok. Meshy kaynak arşivinin kamu yeniden dağıtım lisansı doğrulanmış sayılmaz.