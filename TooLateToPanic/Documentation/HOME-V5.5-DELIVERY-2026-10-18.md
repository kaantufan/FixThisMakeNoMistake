> Public publication copy: the selected source manifest differs in scope from the full local build manifest. Native results below describe the recorded package only. OS testing state is a snapshot and may still be in progress. [Download](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.5.5-r2), [source/build comparison](Publications/Snapshots/20261018T065511Z/source-vs-build.json). Local-only historical evidence links remain local references.

# Home V5.5 teslimi

Güncel **Home V5.5-r2 /0.5.5**. Defter ilk açılışında amacını ve kullanımı anlatır; gerçek malzeme → sabitleme → tesisat → su → gıda → sığınak adımları önerilir. Küçük yardımcı mevcut HUD font/renk/çerçevesiyle J defter düğmesinin üzerindedir; defterden kapatılabilir. Turkuaz dünya halkası ve havada yön oku kaldırıldı. Kroki gerçek sahne/kapı/eşya/oyuncu geometrisini aynı ölçekte gösterir.

FPS'te özgün gerilim fon müziği, sürekli sınırlı kamera sarsıntısı ve7.2s yumuşak kararma/ferahlama döngüsü vardır. Azaltılmış hareket seçeneği korunur. Deprem sinematiğinin sığınak filtresi ve askıdan yeni klip başlatması düzeltildi; gerçek rumble/yapısal sesler kullanılır. Oluşan vana/elektrik RNG olayı, neden ve ek hasarla belirgin TESİSAT UYARISI bandında görünür.120+45s,75%kayıp, RNG/çoklu afetler ve KKTC · Güzelyurt · Kalkanlı korunur.

## Mekanik ve giriş sahipliği

Yardımcı bir görev kilidi oluşturmaz. Her dalgada sabit bir gerçek mobilya hedefi seçilir; malzeme tarifi mevcut Expedition maliyetlerinden, tamamlanma gerçek Anchored/UtilitySafety/PackedWater/PackedFood/ReachedShelterV5 durumundan gelir. Malzeme harcanırsa tamamlanmamış iş için tekrar ihtiyaç gösterilir. Çanta aşamasında önceki eksik önlemler askıya alınır; süre eklenmez. Alternatif önlem, maliyet, bütçe, ücretsiz iptal, gerçek sığınak ve skor kuralları değişmez. Defter açıklaması kendiliğinden açılmaz; aynı World oturumunda tekrar gösterilmez. J/Esc kapanışı ve Enter devamı mevcut modal/saat/girdi kapısına aittir. Türkçe ilk açıklama ve görevler sıcak kağıt üzerinde, oyun içi kart mevcut Barlow/Night/Line paletindedir.

Kroki gerçek collision kutularından döşeme/duvar/ara bölme/merdiven/balkon/garaj, gerçek hinge kanatlarından kapı, canlı eşyalardan ayak izi, kaynak ve oyuncu pozundan pin üretir. Tek ölçekli X/Z dönüşümü kullanır. Kapalı defterde ağır geometri taraması yapılmaz. Çizim hiçbir collider/renderer/input/kamera durumunu değiştirmez. Bu gerçek geometri projeksiyonudur; tüm kesişim/kat kombinasyonlarında insan okunabilirlik kabulü ayrıca yapılacaktır.

FPS müziği özgün16s yerel PCM üretimidir; eski iki kaynak FPS sahipliği boyunca susar. Müzik pause/defter/envanter/dev/sinematik/çanta dışı aşamada durur; açılışta.25s fade-in, doğal deadline'da.30s fade-out kullanır. Gerçek hareket nominal input yönünü, seçme ışını gösterilen kamerayı kullanır. Vinyet kenarı.22..88, merkez okunabilir; azaltılmış harekette sabit.58 ve sıfır kamera/FOV hareketidir. Tam siyah geçiş.84s/2s korunur.

Deprem içeren sinematikte rumble.55, yapısal katman.18 taban gain kullanır; mevcut V5.2 şiddet/duck/eşit güç bütçesi uygulanır. Gerçek earthquake klibi ve mevcut impact_wood/impact_metal korunur. Asıl kaynakta sığınak çarpanı.22*.18=.0396 ve askıda atanmış klibin yalnız UnPause ile başlatılamaması saptandı. Yeni miks dış sinematikte sığınak filtresini uygulamaz; master mute korunur. Teknik GetOutputData RMS dinleme kabulü değildir.

Tesisat uyarısı yalnız üretim sonucuna yazılmış SecondaryEventsV5 listesini okur. Hasar/RNG çağırmaz, aynı dalga/sonuç anahtarını7s içinde tekrar başlatmaz. Gaz/su/elektrik nedeni ve gerçek ek hasar satırları büyük banttadır; modal/kontrol kilidi oluşturmaz. Pause/defterde görünmez ve süre durur.

## Kanıt

113/113 ilgili native API/model/fizik/kamera/ekran/teknik ses kontrolü; 0 runtime hata, 29.57s. Açık kaynak-toplama/iş/konum/saat/ikincil olay/listener fixture'ları vardır. OS/insan/işitme/tüm kampanya kabulü değildir.

Ham rapor `C:\Users\kaant\Desktop\civilgamejam\TooLateToPanic\Evidence\GamejamOrchestra\TLTP-V55-20261018\manager\native-r2-9fb73643-b3aa-475f-afe6-d8936e6a479b/v55-smoke.json`; PNG'ler aynı dizindedir. Piksel incelemesi `manager/owner-pixel-review-r2.json`. Tam fixture ve çağrı sayaçları ham rapordadır; eski V5.4 test sayıları yeni DLL kabulü sayılmaz. Teknik ses ölçümü izole kendi QA player'ında geçici AudioListener1 ile yapılır ve eski değer geri verilir; normal kullanıcı player'ına giriş/sonlandırma gönderilmez.

DLL `82B570D390EAE53F740944C1A6D7560E2E61E1B2D2FE0F5344B66A489676A789`. ZIP `3AB6A1EFB7C6BE69CB9031B18690CD7CCE9AAAB0A2306EA78F49F78D9A150988` (219457748bayt). Portable EXE/DLL byte eşleşmesi doğrulandı. Dondurulmuş162C#/shader,0kayma. Tam kaynak40938dosya/build157dosya manifestlidir. Skor hash önce `62D53735099315B02109CEAFBCC4836B6E6A33D4620C02F5E8395286A059B55C`, sonra `62D53735099315B02109CEAFBCC4836B6E6A33D4620C02F5E8395286A059B55C`; izole QA yazma hedefi kullanılır. Kesin yollar [LATEST](../LATEST-BUILD.json) ile bağlıdır. Normal launch/korunan eski player/temizlik son durumu aynı indeksin normalLaunch/retention alanındadır.

## Asset ve sınır

Sonraki kullanıcı isteğiyle [ses odaklı root testi](HOME-V5.5-AUDIOREVIEW-2026-10-18.md) yapıldı: dört afet ve doğal FPS/geçişlerden310sn Windows endpoint PCM, salt okunur session meter, müzik açık/kapalı ve pause/defter ölçümleri. Aynı DLL korunur; ses yönlendirmesi/semantik/olay katmanlarında açık eksikler var. Bu yeni teknik output kanıtı, önceki113 smoke sayısına eklenmez ve insan dinleme kabulüne dönüştürülmez.

18 Ekim ek inceleme: Kullanıcının eski oyunu kapatma ve V5.5'i açıp test etme isteği uygulandı. [Oynanış/hikâye raporu](HOME-V5.5-PLAYREVIEW-2026-10-18.md), 34 gerçek OS gözlemi/eylem kaydı, normal timeout kaybı, defter/kapı/F1/pause/doğal FPS ve sonradan açıkça skor dışı F3/Enter çoklu afet yolunu ayırır. Dört P2 bulgu açık; tam kazanma ve dinleme kabulü verilmedi. Kod/build aynı; V5.5 ana menüde açık. Aşağıdaki açık eski player koruması cümlesi ilk teslim zamanına aittir; bu ek turda V5.4 kullanıcı yetkisiyle kapatıldı.

[Kaynak/provenance](HOME-V5.5-ASSETS-2026-10-18.md). Yeni ücret/kredi/sağlayıcı/konuşma/GitHub yayını yoktur. Bağımsız FPS, açık kullanıcı oyunu/kayıtları, lisanslar ve tarihsel reddedilmiş V4 temizleme hedefleri korunur. V5.4 özgün zemin üçgen kusuru yeniden oluşmamıştı; kesin nedensel onarım kabulü hâlâ UNVERIFIED. Tüm kampanya/OS/insan konforu ve dinleme bu dar smoke kapsamının dışındadır.
