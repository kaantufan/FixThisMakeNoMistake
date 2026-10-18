# Home V5.9-r7 — defter hedefinde yerel parlama

18 Ekim 2026. Son kullanıcı ekranındaki global hedef gösterimi kaldırıldı. **BURADA / DEFTER HEDEFİ kartı, mesafe, köşe braketleri ve ekran dışı/başka kat yön oku artık çizilmez.** Sağ alt köşedeki eski küçük İŞARETLİ HEDEF kartı da kaldırıldı; elle hedef işaretliyken yardımcı dünya kartı çizmez, mevcut yardımcı tercihi değişmez. Defterden elle seçilen gerçek nesnenin görünen yüzeyi ve mesh konturu hafif sıcak renkle parlar.

Parlama yalnız oyuncu ve hedef evin içindeyken (bitişik garaj dahil), aynı gerçek katta, en fazla 8 metre yakında ve kamera görüşündeyken uygundur. Mevcut derinlik testli gerçek mesh maskesi, duvar/nesne arkasındaki yüzeyleri eler. Uzak/ev dışı/başka kat için ekran işareti üretilmez. Pause, defter, envanter, iş ve sinematik kapıları uygulanır; seçim temizlenince ve yeni koşuda parlama kalkar. Azaltılmış harekette parlama sabittir.

Mevcut yakın etkileşim vurgusu ve toplama parıltıları bağımsız kalır. Özgün shared materyaller, property block, shader, model, RNG, bütçe, stok, saat, controller ve kayıtlar değiştirilmez. R5'in basit metin, tuş gösterimleri, 1500 bütçe, 90+40 saniye, ilk Deprem / ikinci Sel+Fırtına / sonrası RNG, ekonomi ve ses kapsamı korunur. Yeni sağlayıcı/kredi harcaması yoktur.

## Bu DLL için kontrol

Gerçek Unity 6000.0.73f1 Editor: **472/472 PASS**; 0 derleme hatası ve 0 uyarı. Yerel aşama/kat/ev içi/8 metre sınırı; mevcut model, ekonomi, V5.2 risk, envanter, ses importu, font, iş ve V5.8 geometri kontrolleri bu kaynakta yeniden çalıştı.

Yeni izole native UI/yerel parlama suite'ı: **524/524 PASS**, **69 gerçek framebuffer görüntüsü**, 0 runtime hatası. Native maske çizim komutları, gerçek hedef kimliği, modal/uzak/dışarı/başka kat/yeniden giriş/temizleme, azaltılmış hareket ve salt okunur sorgular incelendi. Seçili/seçilmemiş buzdolabı kareleri ve 960×540 / 1600×900 görüntüleri karşılaştırıldı. Raporlara bağlı bütün görüntüler root tarafından incelendi.

Aktörün ev içi/uzak/dışarı yerleşimleri ve tek senkron, anında geri alınan kamera dönüşü açık QA fixture'larıdır. Mesh komutu göndermek tek başına görünür piksel kanıtı sayılmaz. Bu kısa kontrol doğal yürüme, insan okunurluğu veya kampanya kabulü değildir. R5'in 943 native sonucu tarihsel kalır; R7'ye aktarılmaz. R7'de ayrı V56/audio suite'ları tekrar çalıştırılmadı. Fiziksel OS girdisi, insan dinlemesi ve fiziksel 6–7 dalga kabulü NOT_RUN.

**213 C#/shader + 97 ses** donduruldu; kaynak kayması 0. ZIP'in 167 dosyası build manifestiyle tam byte/hash eşleşir. Gerçek skor ve varsayılan ses tercihleri değişmedi. Önceden açık kullanıcı V5.9-r5 oyuncusuna girdi veya sonlandırma gönderilmedi. Eski build/ZIP temizliği ve GitHub yayını yapılmadı.

| Kimlik | SHA256 |
|---|---|
| DLL | `68BB4573E7ECC81250E2EB17488DF92987F3B0CFF2129C0CCA21C8B954AB5F29` |
| ZIP | `7A9031DBE96EA6FD291FED2911A0FCE1825F015A4997D017BEB1F93F4EC3D3A9` |
| Kaynak manifesti | `4AE9900AF4C8033D9132E4785AA52EDB93F44F0F2E426C426514D44624F0335D` |
| Build manifesti | `AD84524481C0C508C0963D5CB53F1D71B57851BEE1EA60C81DD2E908FE5EC2AF` |

[Başlatıcı](../Play-TooLateToPanic.cmd) · [Paket](../Builds/TooLateToPanic-Home-V5.9-r7-20261018-portable.zip) · [Önceki birleşik kapsam](HOME-V5.9-DELIVERY-2026-10-18.md)
