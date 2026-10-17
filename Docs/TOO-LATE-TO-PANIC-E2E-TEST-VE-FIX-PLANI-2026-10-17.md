# Too Late to Panic! — E2E test, deneyim incelemesi ve düzeltme planı

**17 Ekim 2026 · Plan v1 · Durum: yürütülmedi.** Bu belge test etme → kanıt toplama → raporlama → düzeltme tasarlama → uygulama → bağımsız tekrar → teslim zincirinin çalışma sözleşmesidir. Bu turda oyun testleri veya oyun düzeltmeleri yapılmış sayılmaz. Planın kendi dosya/şema kontrolleri oyun kabulünden ayrıdır.

**Aday hedef:** [Home V4.9 Direct Actions sözleşmesi](../Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md). **Doğrulanmış geri dönüş tabanı:** [Home V4.8 teslimi](../TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md). Üretim sürerken planın hazırlanması kaynak dondurma veya Unity sahipliği devri değildir. İlk yürütmede aktif üretim sahibiyle tek sahiplik ve kaynak dondurma sağlanır.

[İnteraktif plan ve aranabilir vaka kataloğu](QA-E2E-2026-10-17/index.html) · [CSV katalog](QA-E2E-2026-10-17/test-cases.csv) · [JSON katalog](QA-E2E-2026-10-17/test-cases.json) · [Tam ayrıntılı vaka metinleri](QA-E2E-2026-10-17/TEST-KATALOGU.md) · [Şablonlar](QA-E2E-2026-10-17/SABLONLAR.md)

## 1. Hangi soruya cevap vereceğiz?

Oyun yalnız hatasız açılıyor mu değil; ilk kez oynayan biri ne yapacağını anlayabiliyor mu, yaptığı hazırlığın afetteki karşılığını görebiliyor mu, kısa akıştan devam etme isteğiyle çıkıyor mu ve gördüğü ürün tamamlanmış hissi veriyor mu? Her mekanik hem geliştirici hem QA bakışıyla sınanır. Her ekran, kadraj ve önemli geçiş de satın almayı düşünen bir oyuncunun gözüyle değerlendirilir.

Her önemli özellik için altı ayrı sonuç yazılır:

1. **Kural doğruluğu:** model, maliyet, hasar, hedef ve faz doğru mu?
2. **Gerçek kullanım:** aynı davranış gerçek klavye/fareyle erişilebilir mi?
3. **Geri bildirim:** dünya, animasyon, HUD ve ses aynı sonucu anlatıyor mu?
4. **Hikâye ve amaç:** oyuncu bu işi neden yaptığını ve sonraki amacını biliyor mu?
5. **Tema:** hazırlık ve önlem anlamlı mı; yanlış veya yanıltıcı afet davranışı öğretiliyor mu?
6. **Ürün kalitesi:** kadraj, okunurluk, ses, hareket ve tempo devam etme isteğini destekliyor mu?

Bu sonuçlar birbirinin yerine geçmez. Başarılı API çağrısı fare tıklamasının çalıştığını; güzel kare de işin doğru maliyet ödediğini kanıtlamaz. Yeni hikâye, karakter, görev veya afet zinciri icat etmek bu planın bağımlılığı değildir. Mevcut ev, amaçlar, hazırlık ve sonuçlar üzerinden anlatı tutarlılığı aranır.

## 2. Başlangıçta bilinen durum ve kanıt sınırı

| Kaynak | Gerçek kayıt | Bu plana etkisi |
|---|---|---|
| V4.8 teslimi | 149/149 kesintisiz üretim API rotası; 189/189 regresyon; kısa akış 49,67 sn | Aynı DLL'a bağlı tarihsel kanıt. Yeni fare akışını, insan hızını ve keyfi kabul etmez. |
| V4.8 görsel QC | 1600×900, incelenen 11 kare; dış incelemede iki hasar etiketi çakışması P2 | Güncel adayda tekrar üretilecek mevcut kayıt; tüm kadrajların kabulü değildir. |
| Etkileşim audit V2 | 616/760, FAIL; alt kümelerde model 392/392, hedef 96/96 | Başarısız koşu korunur. Ürün kusuru ve geçersiz fixture ayrı sınıflanır; alt küme PASS bütün koşuyu geçirmez. |
| Etkileşim dar UI V3 | 138/138; mantıksal pointer fixture, gerçek OnGUI callback'leri | Donanım, OS koordinat dönüşümü ve sonraki genişletilmiş test kaynağı kabulü değildir. |
| Bu planın salt okunur başlangıç kontrolü | 61 kayıtlı kaynakta 4 değişiklik + manifest dışı 5 audit kaynağı; yerel V4.8 EXE/DLL/ZIP hashleri LATEST ile aynı | Canlı kaynak ile doğrulanmış paket ayrı tutulur. Bu kontrol oyun çalıştırmamıştır. Üretim devam ediyorsa fark sayısı yürütmede yeniden ölçülür. |
| V4.9 sözleşmesi | Yeni doğrudan eylemler üretim hedefidir | Kaynağı, importu, DLL'ı ve oynanabilir kabulü ayrı ayrı doğrulanacaktır. |

Başlangıç receipt'i: [planning-baseline.json](../Evidence/GamejamOrchestra/TLTP-E2E-MASTERPLAN-20261017/manager/planning-baseline.json). V4.8 DLL: `5683E713C90F2FD070DB819D14526EBA74ACC25B65C6581DABD6FB602F6BD17E`. ZIP: `018D311651CBFCC9053D1CE659430508E7829B2700E83F36B2F850BD9307D9FD`.

Etkileşim devri: [REPORT](../Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/REPORT.md), [BUGS](../Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/BUGS.md), [sahiplik ve sürüm ayrımı](../Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md). Buradaki başka sohbetin üretim hedefleri bu turda uygulama veya Unity sahipliği yetkisi değildir.

## 3. Kapsam ve sürüme göre beklenen davranış

Unity 6000.0.73f1, Built-In, Windows, çevrimdışı, tek oyuncu izometrik ürün. FPS askıda kalır; dosyaları, kayıtları ve paketleri bu çalışmanın test/fix alanına girmez. Renderer/framework/controller göçü, ücretli sağlayıcı, yeni ses kredisi, çok oyunculu ve zincirleme afet kapsam dışıdır. Kapsam dışı alan test raporunda gerekçeli `NOT_APPLICABLE` olur; test edilmemiş bir özellik PASS yapılmaz.

| Alan | V4.9 adayının kabulü | V4.8'e özgü tarihsel regresyon |
|---|---|---|
| Sabitleme | Yakın plandaki iki gerçek vidaya sırayla tıkla; her biri 0,60 sn oturur; beklerken çift girdi ilerletmez/harcamaz | A/D–Enter–Space mini oyunu yeni adayın şartı değildir. |
| Diğer kurulumlar | Eşyaya uygun bağlamsal işlem, çoğunlukla 0,60 sn; karakter hareketi görünür | Eski Ratchet/Timing/Balance aileleri yalnız eski sürümde beklenir. |
| Eylem sahipliği | Uzun/dengesiz mobilya sabitlenir; yapı güçlenir; desteklenmeyen eylem HUD, kısayol ve API'de reddedilir | Eskiden izin verilen genel mobilya yangın eylemi yeni beklenti değildir. |
| Yangın hazırlığı | Alt katta ayrı istasyon; envanterdeki gerçek söndürücüyü tüketir; kısa rota burayı gösterir | Yatağa/koltuğa tüp takmak kabul edilemez. |
| Güçlendirme | Türe/kademeye göre gerçek geometri, 0,5–0,7 sn montaj; açıklıklar/kapı dönüşü/kesit/collider korunur | Yalnız renk değiştirme yeterli değildir. |
| Hareket | Yürüme 3,645; koşma 5,67 m/sn; animasyon uyumlu; controller ölçüleri korunur | Eski 2,7/4,2 hızı yeni adayın değişmezi değildir. |
| Bahçe | Üç bölgede birer yetişkin ağaç sırası; kit/isim/adet tutarlı; yol ve bunker açık | Tek fidan/tohum/sulama sunumu yeni adaya taşınmaz. |
| Karakter | Korunan CC0 rig/avatar/controller/kliplerle yüz, oran, kıyafet ve hareket cilası | Yeni insan modeli veya rig göçü kabul bağımlılığı değildir. |
| Değişmeyenler | Sonlu ekonomi, atomik commit, iptalde harcamama, gerçek hasar/kayıp, skor yalıtımı, kapı güvenliği, kayıt kimliği | Hata düzeltmek bu kuralları gevşetme yetkisi vermez. |

Katalogdaki eski iş ailesine bağlı vaka yalnız ilgili tarihsel profil için uygulanır. F0 sırasında `behavior-profile.json` her vaka için `active`, `legacy_only`, `superseded`, `not_applicable` kararını ve sözleşme gerekçesini kaydeder. Eski beklentinin kasıtlı değişmesi regresyon hatası diye açılmaz; yeni davranışın eksikliği de eski PASS ile örtülmez.

## 4. Test katmanları ve sonuç sözlüğü

| Katman | Nasıl çalışır? | Ne iddia edebilir? |
|---|---|---|
| SOURCE | Kod, scene/prefab, ayarlar, tarif, metin, lisans ve bağlantı incelemesi | Kaynakta kural/bağlantı var veya şüpheli; çalışma kabulü vermez. |
| MODEL | İzole durum, sabit seed ve sınır verileriyle iş kuralı | Tam ilgili model davranışı; input, kamera veya fizik kabulü vermez. |
| NATIVE_API | Gerçek derlenmiş oyunda üretim eylemleri, collider/controller/render | Gerçek runtime davranışı. Warp/Tick/fixture kullanımı ayrı alanlarda sayılır. |
| OS_E2E | Normal player penceresinde gerçek klavye/fare; callback veya model alanı yazılmaz | Görülen gerçek kullanıcı yolu ve girdi; insanın anlamasını kanıtlamaz. |
| HUMAN | Oyunu bilmeyen kişi; yardım verilmeden, dış saat ve gözlem | Anlama, ilk deneyim, süre, ses, görsel değerlendirme; küçük örneklem sınırlıdır. |

Vaka sonuçları: `NOT_RUN`, `PASS`, `FAIL`, `BLOCKED`, `NOT_APPLICABLE`, `INVALID_TEST`. Katalog tasarım durumu `PLANNED` tutulur; bu bir sonuç değildir. Başarısız fixture `INVALID_TEST` gerekçesiyle yenilenebilir, eski FAIL dosyası silinmez. `PASS_WITH_LIMITATIONS` yalnız rapor özeti olabilir; alt vaka sonuçlarını birleştirip gizlemez. `FLAKY` ayrı etiket olup son deneme PASS olsa da araştırma kapanmaz.

**Her vakanın zorunlu inceleme alanları:** developer kararı (doğru state/hesap/commit ve kök neden); QA kararı (gerçek adım, negatif/sınır, tekrar oranı ve bağımsız kanıt); alıcı yorumu (görünen ekranın anlaşılması/kalitesi); hikâye–tema kararı (amaç ve sonuç bağı). Salt model/kaynak vakasında render yoksa alıcı alanı gerekçeli N/A ve bağlı OS/görsel vaka kimliği alır. Sessizce boş bırakılmaz. Görsel vakada teknik PASS yanında alıcı/tema sorunu açık kalabilir; tek yeşil kutu bütün bakışları örtmez.

İlerleme yüzdesi = yürütülmüş geçerli vaka / uygulanabilir vaka. Kalite oranı = PASS / yürütülmüş geçerli vaka; BLOCKED, NOT_RUN ve bilinen açıklar ayrıca görünür. Eski assert sayıları, yeni vaka sayıları ve taranan buton sayıları tek paydaya toplanmaz. Bir vakanın kapsadığı çoklu hedef/adım tek bir PASS satırıyla kaybolmaz; hedef alt satırları tutulur.

## 5. Test edilen tabanı sabitleme — F0

1. Aktif izometrik üretim sahibini ve mevcut aday hedefini doğrula; aynı projede ikinci Unity sahibi oluşturma. Açık kullanıcı oyunu kapatılmaz veya test girdisi hedefi yapılmaz.
2. En güncel AGENTS, sözleşme, GDD, README, LATEST ve üretim devrini oku. Çelişen davranışı `behavior-profile.json` içinde çöz; bu karar açıklanmadan test oracle'ını değiştirme.
3. Küçük kaynak snapshotı; SHA256 kaynak, metadata, settings/scenes, import girdisi ve retained prefab manifesti al. Var olan kullanıcının çalışmalarını geri alma.
4. Var olan V4.8 paketinin kimliğini geri dönüş kaydı olarak al. Yeni aday henüz derlenmediyse kaynak snapshotı ve planlanan build hedefini kaydet; final binary/ZIP kimliği F3 sonunda `build-identity.json` ile bağlanır. Build sonrası canlı değişiklik varsa yeni snapshot/build kimliği aç.
5. Test donanımı/CPU/GPU/RAM/Windows/driver/monitör/DPI/çözünürlük/kalite/VSync/odak bilgisi kaydet. Desteklenen minimum donanım tanımlı değilse varsayma; referans test makinesi olarak yaz.
6. Gerçek skor dosyası yolunu yalnız kayıt koruma için tespit et; içeriklerini rapora dökme. Her otomatik koşu yeni GUID `--home-evidence` profili kullanır. Önce/sonra gerçek dosya hashleri kontrol edilir.
7. Otomatik E2E sayacında başlangıç spawnı, oyun sahipli sinematik yer değiştirmesi, test Warp, enjekte Tick, kaynak bağışı ve dev seçeneği ayrı alanlardır. OS rotasında test Warp/Tick/cheat sıfır olmalıdır.
8. Test/binary/screenshot klasörünün yazılabilirliğini ve log watchdog'unu doğrula. Raporsuz kapanan süreç PASS değildir; `completed`, sonuç dosyası, log ve exit code birlikte incelenir.
9. `quality-policy.json` içinde tek eşik setini kaydet: bu planın başlangıç önerileri en az üç yeni kişi, referans cihazda p95≤33,3 ms, 30–60 dk ve en az10 tekrar döngüsü. Daha geniş örneklem/soak ayrı kapsam olur. Sözleşmedeki 180/270/300 sn ile 0,60 sn kurulum/hız gibi ürün kararları öneri eşiğine karıştırılmaz. Katalog/eklerde daha sıkı öneri çıkarsa bu tek profile göre normalize edilir; test sonrası sonucu geçirmek için eşik değiştirilmez.

**F0 çıkışı:** tek kaynak/contract kimliği, davranış ve kalite profilleri, izole kayıt, test sahibi ve açık aday build hedefi hazır. Yeni adayın binary/ZIP receipt'i F3'te tamamlanır ve F4 OS testinden önce kapanır; henüz yapılmamış build F0 önkoşulu değildir. Sözleşmesi belirsiz alan BLOCKED olur; bağımsız salt okunur alanlar ilerler.

## 6. Yürütme sırası ve teslim edilen ara raporlar

| Faz | İş | Somut çıktı ve geçiş kapısı |
|---|---|---|
| F0 | Sahiplik, güncellik, kaynak ve profil sabitleme | Kimlik receipt'i; sürüme uygun oracle; gerçek kayıt korunur. |
| F1 | Kaynak ve mevcut test kapsamı denetimi | Gereksinim–fonksiyon–hedef–test–kanıt eşlemesi; doğrulanmış açık / hipotez ayrımı. |
| F2 | Değişmiş kuralların model/entegrasyon kontrolleri | Eylem izinleri, tık/kurulum state'i, maliyet/iptal/reset/saat/skor sınırları. |
| F3 | Tek birleşik import/bake/build ve 3–5 dk ilgili smoke | Yeni DLL, hata/uyarı kaydı, normal açılış; blocker varsa önce dar onarım. |
| F4 | Gerçek OS kesintisiz uçtan uca yollar | Kısa akış, dört simülasyon, kampanya dilimleri, fail/retry/menü/quit video ve zaman çizelgesi. |
| F5 | Sistemli ekran/kadraj/ses ve alıcı incelemesi | Tam ekran envanteri, puanlı görsel rapor, neden–sonuç–tema matrisi. F4 sırasında kareler toplanabilir. |
| F6 | İlk oyuncu denemeleri | En az üç yeni kişi; dış saat; yardım/yanlış anlama/duygu/ilk kazanım/bitirme kayıtları. |
| F7 | Tek birleştirilmiş triage ve fix planı | Kanıtlı sorunlar; etki/öncelik/kök neden; sınırlı paketler ve kabul testi. |
| F8 | Sıralı düzeltme paketleri | Staging QC → root entegrasyonu → bir birleşik build → 3–5 dk ilgili smoke → bağımsız tekrar. |
| F9 | Kapanış regresyonu ve taşınabilir teslim | Yeni DLL'a bağlı gerekli E2E, insan/görsel tekrar; LATEST/launcher/ZIP/doküman ve açık sınırlar. |

F4–F6 geniş ilk değerlendirmedir; her küçük mesh düzeltmesinde baştan tekrarlanmaz. Geniş tur gün sonunda, yayın adayında veya somut yeni riskte yapılır. Her fix paketinde etkilenen mekanik smoke'u ve seçilmiş komşu regresyonlar yeterlidir; süre sınırı bir arızayı gizleme gerekçesi değildir.

Çalışma tahmini sonuç yerine konmaz. F0–F3 yaklaşık yarım iş günü; F4–F6 bir kapsamlı değerlendirme günü + katılımcı erişimi; F7 1–2 saatlik triage; F8 bulguya göre paketlenir; F9 son aday üzerinde yürütülür. Bunlar planlama aralığıdır, teslim sözü değildir. Süre daralırsa düşük riskli P3 sanat işi ertelenir; test edilmemiş kritik alana PASS yazılmaz.

## 7. Gereksinimden kanıta izlenebilirlik

Her satır için `requirement_id → contract/source → active version → target/state → case_id → run_id → evidence → issue_id → fix_batch → retest` zinciri tutulur. Katalog bu planla birlikte 257 çekirdek vaka içerir; gerçek hedef/çözünürlük/seed alt matrisi bu sayının dışında açık satırlar üretir.

| Gereksinim ailesi | Vaka ailesi | Asgari kanıt |
|---|---|---|
| R01 Paket/açılış/offline/kayıt | SYS + MEC | Temiz klasörden açılış, izolasyon, hash/log. |
| R02 Kısa akış ve 180/270/300 sn | MEC + EXP | Kesintisiz OS rota + yeni insan dış saat. |
| R03 Deprem/sel/yangın/fırtına simülasyonları | MEC + EXP | Afet başına hazırlık–olay–gerçek sonuç–inceleme–tekrar. |
| R04 Kampanya dalga, erzak, hasar ve highscore | MEC + SYS | Sınır model testi + doğal kampanya dilimleri + diskten skor tekrar okuma. |
| R05 E/F/toplama/envanter/tarif | MEC | Gerçek kaynağa erişim, stok/kapasite/iptal/çift girdi. |
| R06 Doğrudan tık/kurulum/uygun eylem | MEC + EXP | V4.9 gerçek vida hit alanı, 0,60 sn state ve maliyet commit. |
| R07 Kapı/merdiven/hareket/yerleşim | MEC + SYS + EXP | Doğal rotalar, çapraz giriş, geçersiz konum ve okunur kamera. |
| R08 HUD/modallar/tuş önceliği | MEC + SYS + EXP | Fiziksel giriş, DPI/resize, aynı kare olumsuz testleri. |
| R09 Sanat/karakter/VFX/ses | EXP + SYS | Gerçek aday renderı, hareket videosu, insan dinleme. |
| R10 Hikâye/tema/amaç/karne doğruluğu | EXP + MEC | Oyuncunun açıklaması + dünya/model/sonuç karşılaştırması. |
| R11 Teknik performans/tekrar/temizlik | SYS | Referans cihazda ölçüm; drift/sızıntı/kayıt kanıtı. |

Kod kapsamı yüzdesi tek başına hedef değildir. Oyuncuya açık her düğme, bağlam eylemi, gerçek kaynak, yerleşebilir eşya ailesi, iş ailesi/yeni kurulum türü ve terminal faz envantere girer. Tıklanabilir görünüp işlem yapmayan dekor da ayrıca incelenir. Kaynakta yeni kontrol eklenince envanter ve matrise eklenmeden kapsam tamamlandı denmez.

Katalogdaki P0–P3 **test yürütme sırasıdır**, bulunacak bir hatanın severity/priority kararı değildir. İlk P0 kapısı kimlik, kayıt, açılış, temel transaction ve terminal risklerine dar tutulur; diğer önemli kabul vakaları P1'de kalır. Bir P2 testinde P0 ürün kusuru bulunabilir. Bir kesintisiz rota birden fazla vakanın kanıtını sağlayabilir; aynı mekanik sırf üç değerlendirme bakışı var diye üç defa çalıştırılmaz. Aynı run/capture birden fazla case_id'ye açık gözlem eşlemesiyle bağlanır.

## 8. Ana E2E yollar — gerçek oyuncu girdisi

Bu yollar API suite'inden ayrı koşulur. Her adımda yönlendirme, dünya hareketi, HUD, maliyet ve sonuç izlenir. Video veya kısa klip hareket/çift giriş/süre için; PNG durum ve görünüş için kullanılır. Eylem tamamlaması doğrudan state yazılarak elde edilmez.

| Yol | Başlangıç ve rota | Beklenen son ve özellikle incelenecekler |
|---|---|---|
| E2E-01 İlk normal açılış | ZIP'i boş Unicode/boşluklu dizine aç → exe → başlık → yardım → geri | Offline, font/ikon/ses eksik değil; gerçek skor değişmez; açık kullanıcı oyunu etkilenmez. |
| E2E-02 İlk kısa akış | HAZIRLIĞA BAŞLA → buzdolabı E → su Z/X/F veya gerçek fare → Tab/Enter çanta → uygun mobilya → iki vidaya tık → deprem güvenli alanı → sonuç | Sonlu stok, iki gerçek tık, doğru maliyet, görünür donanım ve aynı olayda farklı eşya tepkisi. |
| E2E-03 Kısa akışın devamı | Gerçek drenaj kurulumu → fiziksel üst kat sel hedefi → afet → yangın istasyonu/gerçek tüp → dış hedef → karne | Yeni hedef/hint/işaret aynı istasyonu gösterir; üç afet gerçek eylemlerle; karne yapılmayanı tamam saymaz. |
| E2E-04 Eksik/yavaş kısa akış | Bir malzemeyi unut → yanlış vidaya tıkla → kurulumu iptal et → Esc/çanta açık bekle → ilerle | İlk hedef kaçırılabilir; sahte başarı yok; kısa akış saati sözleşmeye uygun, özet ≤300 sn. |
| E2E-05 Deprem simülasyonu | Seçici → deprem → sonlu hazırlık → uygun sabitleme → Afeti Başlat → doğal olay → sonuç/inceleme → aynı afetle tekrar | Doğru seçili afet; gerçek hasar/erzak; temiz retry; ödül/highscore yok. |
| E2E-06 Sel simülasyonu | Sel → bariyer/drenaj → geçerli yüksek hedef → olay/inceleme/menü | Suyun yönü/seviyesi anlaşılır; bunker/sel mesajı güvenlik açısından ayrıca değerlendirilir. |
| E2E-07 Yangın simülasyonu | Yangın → ayrı istasyonda gerçek tüp kullan → güvenli yönlendirme → olay → kayıp veya inceleme | Tüp yatak/koltuğa bağlanmaz; yangın/duman geçişi okunur; incelemede aktif efekt temizlenir. |
| E2E-08 Fırtına simülasyonu | Fırtına → uygun hazırlık → doğal olay → ağır hasar kolu → Hasarı İncele → retry | Fırtına kısa akışa zorunlu dördüncü afet olmaz; ağır kayıp dürüstçe korunur. |
| E2E-09 Kampanya ilk başarı | SERBEST OYUN → gerçek çanta/iş → bunkera yürüyerek yetiş → olay/ödül/inceleme → sonraki dalga | Maliyet bir defa, gerçek erzak tüketimi, kalıcı donanım, yeni dalgada temiz hasar. |
| E2E-10 Kampanya üç kayıp | Ayrı profillerde süreyi kaçır / çantayı eksik tut / %65 üstü hasar üret | Her kol doğru neden, geri bildirim ve kararlı sonuç; restart/menü çalışır; başarı ödülü yok. |
| E2E-11 Dalga sınırları | Doğal oynanışta 3→4, 6→7, 9→10; ayrı model testinde 65 eşiği ve RNG | İhtiyaç 1/2/3, süre 300/270/240, afet sırası, skor sırası ve kalıcı önlemler doğru. |
| E2E-12 Geliştirici ve skor | F2 yalnız aç/kapat → temiz koşu; sonra her seçenek ve +1000 → kapat → sonuç → yeni koşu | Menüyü açmak temiz; kullanım sticky skor dışı; NoLoss gerçek hasar/tüketim ve sıfır ödülle inceleme. |
| E2E-13 Kapı ve katlar | Bodrum → giriş → üst kat → güney banyo → balkon → dış kapı → garaj/bunker; çapraz gir/geri dön | Açılış/kapanış karakteri itmez/sıkıştırmaz; görünür kat kesiti fiziği kaldırmaz. |
| E2E-14 Taşıma ve kamera | Sağ tık kısa taşı → geçerli konum/teker → bırak → geçersiz kapı/merdiven/kapsül → iptal; sağ basılı sürükle kamera | Kamera jesti taşıma açmaz; taşınmış kaynak içeriği doğru yerde; HUD tıklaması dünyaya sızmaz. |
| E2E-15 Yapı ve ağaçlar | Gerçek malzemeyle yapı T0→T1→T2 → üç ayrı bölgeye ağaç sırası → ilgili afet → yeniden başlat | Her kademede görünür geometri; 0,5–0,7 sn; açıklık/kesit/kapı/collider ve yollar açık; bölge sayısı doğru. |
| E2E-16 Oturum ve disk | Temiz kampanya skoru üret → normal çık → yeniden aç → tablo → kısa/sim/dev koşusu → tekrar aç | Yalnız uygun temiz skor kalır; öğretici/sim/kısa/dev kayıt sızıntısı yok. Diskten tam kampanya devamı vaat edilmez. |

Uzun kampanya testinde doğal oynanmış dilim ve fixture ile atlanmış dalga açıkça ayrılır. Eşik/olasılık testinde seed veya model fixture kullanılabilir; bu kanıt tam doğal kampanya yerine yazılmaz. İlk geniş kampanya bir defa son aday üzerinde yürütülür; her cila paketinde 10 dalga tekrar zorunluluğu yoktur.

## 9. Mekanik sınırlar ve kötü kullanım matrisi

Her anlamlı eylem için başarı + geçersiz hedef + eksik birim + tam sınır + tekrar + iptal + faz değişimi + restart sınıfları değerlendirilir. Uygulanmayan sınıf gerekçeli N/A'dır.

- **Ekonomi:** maliyet−1 / maliyet / maliyet+1 bütçe; malzeme 0 / gerek−1 / tam / fazla; envanter/çanta son boş yuva; 4/5/6 yığın sınırı; hiçbir aşamada negatif/kopya stok yok.
- **Atomiklik:** kurulum başlamadan, ilk vidadan sonra, son oturma karesinde, tamamlandıktan hemen sonra iptal/çift tık; para+malzeme+hedef state'i birlikte bir defa commit olur. İptal önceliği gerçek tasarımla eşleştirilir.
- **Eylem kuralı:** UI'de gizli eylemin kısayol/API reddi; desteklenen eşyaya izin; kilitli/yanlış kat/duvar arkasındaki hedefe ret; red sebebi kullanıcıya yeterli açıklıkta.
- **Kısa tık/uzun sürükleme:** eşik−ε / eşik / eşik+ε süre ve piksel; kamera sağ sürüklemesinin sonunda taşıma başlaması; pencere dışına bırakma ve odak kaybı.
- **Faz:** menü, öğretici, hazırlık, iş/kurulum, taşıma, afet, sinematik, kayıp, inceleme ve sonuçta tuş/mouse önceliği; aynı tuş iki fazı birlikte tetiklemez.
- **Saat:** normal pause durur; kısa akışın üst bütçesi menüyle uzamaz; sinematik InputLocked model saatini yanlış dondurmaz; içerik ve mutlak sınır iki kez çözüm üretmez.
- **Hasar:** 64,99 / 65,00 / 65,01 temsilî model değerleri; kaynak gerçek `>65` kuralını doğrular; kayan nokta toleransı sonuç raporunda saklanmaz. Gerçek afetle en az bir kayıp ayrıca görülür.
- **Kapılar:** iki taraf × açık/kapalı/yarı × giriş/geri dönüş/koşu × dar yaklaşım. Kapsül merkezi/boyutu, güvenlik payı ve ara açılar korunur; quaternion küçük açı kestirmesi geri gelmez.
- **Yerleşim:** geçerli zemin; duvar/kapı/merdiven/aktör/başka eşya; kat dışı ve UI üstü; anchored ret; cancel geri yükleme; alınan kaynak konumunun güncellenmesi.
- **Reset:** her sonuçtan başlık/yeni koşu; kamera, kapı yönü, seçili kaynak, bag slotları, hile flagi, dev seçenekleri, ağaç bölgesi, iş rig'i, parçacık/ses ve hasar katmanı kontrol edilir.

Sonsuz kombinasyon taraması vaat edilmez. Eşdeğerlik sınıfları ve sınır değerler tam; kaynak/target/phase kapsaması tam; yüksek riskli aynı-kare çiftleri tam; kalan düşük riskli kombinasyonlar gerekçeli pairwise ile seçilir. Önceki 355 pairwise planı yürütülmüş kabul edilmez.

## 10. Klavye, fare, modallar ve erişilebilirlik

Kontrol matrisi satırları: WASD/ok/Shift/Space/E/F/Z/X/Q/T/1–4/R/teker/Tab/B/Enter/NumPad Enter/Backspace/Esc/H/F2/N ve yeni doğrudan fare kontrolleri. Sütunları: her faz/mod ve üstte açık modal. Her hücre `eylem`, `bilerek engelli`, `miras kontrol/superseded` kararı alır.

Özel denemeler: Tab+B, F+LMB, Enter+LMB, son kurulum karesinde Esc+LMB, F2+Escape; down/up/hold, çift tık, uzun hold, pencere dışına sürükle, Alt+Tab ile tuş bırakma, farklı çözünürlükte gerçek hit alanı. Sentetik aynı-kare testi NATIVE_API kalır; bununla fiziksel OS kabulü verilemez.

Sessiz oynama, yalnız klavye ile ulaşılabilen yollar, renk dışında şekil/metin geri bildirimi, düşük çözünürlük, küçük metin, hareket hassasiyeti ve altyazı denetlenir. Desteklenmeyen tam tuş yeniden atama/gamepad/ekran okuyucu özellikleri mevcutmuş gibi gereksinim yapılmaz; erişim engeli varsa açık ürün sınırlaması veya öneri kaydı açılır. Yeni seçenek ekleme ancak somut bulgunun kapsamlı fix kararıyla olur.

## 11. Her ekranı ve dünyayı sistemli görsel inceleme

"Her görüntü" her render karesini tek tek saymak değildir: kullanıcıya sunulan her ayrı ekran/faz, her oda/önemli rota ve her afetin öncesi–sırası–sonrası envanteri kapsanır. Hareketli sorunlar kliple değerlendirilir; tek güzel kare seçilerek kusur gizlenmez.

[Ekran envanteri](QA-E2E-2026-10-17/screen-inventory.csv) 90 inceleme birimi içerir: kaynak incelemesinden gelen40 UI/durum,27 mekân/kamera,18 sinematik plan ve5 ayrı karakter/hareket geçişi. Her satır ilgili EXP vakasına bağlıdır. Bir satırdaki boş/dolu/ara/iptal veya afet varyantları yürütmede ayrı capture_id alır;90 sayısı tüm alt varyantların tek görüntüye sığdığı anlamına gelmez.

**Zorunlu ekranlar:** başlık, yardım/öğretici ve çıkışı, simülasyon seçici, üç modun hazırlık HUD'leri, açık/boş/tam kaynak, envanter/çanta ve craft eksikleri, seçili/uygunsuz hedef, iki vida yakın planı, dünyada montaj, geçerli/geçersiz placement, pause, geliştirici seçenekleri, afet uyarısı, her afet/sinematik, skip, çanta/süre/hasar kayıpları, hasar incelemesi, kısa karne, kampanya sonucu/ödül, highscore ve yeniden başlangıç.

**Zorunlu dünya güzergâhı:** giriş ve dar dönüşler; salon/mutfak; tüm merdiven/sahanlıklar; üst odalar ve güney banyosu; balkon; bodrum/kiler/çamaşır; garaj/kapısı/yolu; bahçe üç ağaç bölgesi; bunker girişi; yol/çit; komşu evler; dere/menfez; çevre NPCleri. NPCler görev/sosyal simülasyon gibi yanlış vaat oluşturuyor mu ayrıca değerlendirilir.

**Kamera matrisi:** normal oyun zoom'u + en yakın/en uzak; desteklenen orbit ana yönleri; iç/dış ve kat eşikleri; iş yakın planı; tüm afet/sonuç kadrajı. Sadece kusur görünmeyen yön test edilmez. Görsel kat saklama ile collider sürekliliği birlikte kontrol edilir.

**Başlangıç çözünürlük seti:** 960×540, 1280×720, 1600×900, 1920×1080; 1366×768 dizüstü ve mümkünse 2560×1440/ultrawide örneği. Windows 100/125/150% DPI; pencereyi çalışma sırasında büyüt/küçült. Bunlar yeni garanti edilen destek listesi değil, önerilen test kapsamıdır. Gerçek destek hedefi F0'da kararlaştırılır. Tüm ekranlar referans 1600×900'de; kritik metin, modal, hit alanı ve rota UI'leri küçük/diğer oranlarda kontrol edilir; her sanat karesini tüm çarpımlarda çekmek gerekmez.

### Görüntü başına yorum formu

1. İlk bakışta odak nerede? Oyuncu/hedef/tehlike/sonraki eylem üç saniyede bulunabiliyor mu?
2. Eşya ve oda ne? Normal oyun zoom'unda malzeme/biçim/işlev ayrılıyor mu?
3. Bitmemiş görünüş yaratan kusur ne? Mesh kesişimi, yüzeyde boşluk, havada parça, tekrar, düşük ayrıntı, yanlış ölçek, geçersiz UV/normal/pembe shader, ışık/gölge/alpha sorunu var mı?
4. Mekanikle uyuşuyor mu? Braket gerçekten tamamlanınca mı var; güçlendirme açıklığı kapatıyor mu; tıklanan vida görseli/hit alanı aynı mı?
5. Duygu ve tema ne anlatıyor? Ev yaşanmış mı; afet ciddi ve anlaşılır mı; efekt bilgiye hizmet ediyor mu?
6. Alıcı yorumu: bu kareden sonra devam etmek ister miyim; hangi somut sebep güveni azaltıyor? Bu görüş hangi kanıt/katılımcı sayısına dayanıyor?
7. En küçük uygun düzeltme ve başarısını gösterecek aynı kadraj tekrar ölçütü nedir?

Her kayda temiz tam kare PNG + açıklamalı kopya; sorun bölgesi koordinatı; kaynak/build/DLL; çözünürlük/DPI/kamera/faz/hedef; önce–sonra eşleşen kadraj eklenir. Orijinal görüntü değiştirilmez. Düzeltme sonrası güzel bir crop, tam kare kabulü yerine geçmez.

### Alıcı değerlendirme puanı — önerilen eşik

Her boyut 0–4: **0** işlevi anlayamıyorum/ekran kullanılamıyor; **1** belirgin prototip/kırık görünüş, sık müdahale; **2** çalışıyor fakat birden fazla belirgin kusur/tereddüt; **3** anlaşılır ve tutarlı, küçük cila açıkları; **4** özenli, hızlı okunur, güçlü bağlam ve geri bildirim. Boyutlar: odak/okunurluk, malzeme/biçim, ışık/renk, animasyon/temas, UI uyumu, afet anlatımı, hazırlığın görünür karşılığı.

Önerilen yayın kalite kapısı: kritik rota ekranlarının tümünde okunurluk ve eylem açıklığı ≥3; hiçbir kritik boyut 0/1 değil. Ortalama puan bir blocker'ı gizleyemez. Bu puan piyasa/satış tahmini değildir; değerlendirici kimliği ve gerekçesiyle raporlanan yargıdır. İnsanla doğrulanmadıysa `reviewer_opinion`, kullanıcıdan geldiyse `participant_observation` olarak ayrılır.

## 12. Mekanik–hikâye–tema nedensellik matrisi

Her satırda `amaç → gerçek eylem → model state'i → dünyadaki iz → afet sonucu → sonuç cümlesi → oyuncunun kendi açıklaması` tutulur. En az şu zincirler:

| Zincir | Anlamlı bağlantı | Yanlış/eksik bağa örnek | Kabul gözlemi |
|---|---|---|---|
| Su ve çanta | Gerçek kaynaktan alınan su çantaya gider; afet sonrası gereken erzak tüketilir | Envanterde su var diye çanta tamam veya boşalan slot dolu gösterilir | Önce/sonra gerçek adet + oyuncu neden çantaya koyduğunu açıklar. |
| Mobilya sabitleme | Uygun hedefte iki vida; maliyet; görünür braket; deprem risk etkisi | Tık dekoratif, state değişmiyor veya donanım iptalden sonra kalıyor | Aynı state dünyanın/raporun/hasarın üçünde tutarlı. |
| Yapı güçlendirme | Kademe geometriyle okunur ve ilgili kurala bağlanır | Mobilyaya yapı eylemi veya açıklığı kapatan kaplama | Uygun hedef, görünen kademe, açık geçiş ve hesaplanan etki. |
| Drenaj/sel | Önceden gerçek kurulum; sel etkisinin anlaşılır farkı | Su dokusu var ama önlem açıklaması yok; uydurma mühendislik yüzdesi | Görünür konum/çıkış ve gerçek sonuçla sınırlı anlatım. |
| Ağaç bölgesi | Üç bölgede yetişkin sıra kurulumu ve sınırlı katkı | Tohum etiketi/yetişkin ağaç çelişkisi; drenaj yerine mutlak çözüm | Kit, bölge sayısı, görünür sıra ve katkı iddiası tutarlı. |
| Yangın istasyonu | Gerçek söndürücüyle uygun hazırlık; açık dış hedef | Yatakta tüp veya oyuncuyu yangının içine yönlendiren ipucu | İstasyon/hint/işaret aynı; hazırlık ve olay sırası açık. |
| Güvenli hedef | Afet türüne göre neden oraya gidildiği anlaşılır | Her afet için yeraltını evrensel güvenlik önerisi gibi gösterme | Oyun soyutlaması ve tehlike türüne özgü mesaj değerlendirilir. |
| Korunmuş/korunmamış eşya | Aynı olayda uygun farklı tepki | Her sabitsiz eşya kesin devrilir; sabit mutlak hasarsızlık gösterir | Tepki gerçek state'e bağlı; kesinlik vaat edilmez. |
| Kayıp ve tekrar | Süre/erzak/hasar nedeni bir sonraki kararı öğretir | Sadece KAYBETTİN; oyuncu neyi düzelteceğini bilmiyor | Oyuncu neden ve bir sonraki önlemi kendi sözleriyle söyler. |
| Karne/ödül | Yapılan, eksik, atlanan ve gerçek hasar | Atlanmış eyleme tamam; Contribution'ı önlenen hasar yüzdesi sunma | Karne satırları gerçek olay ledger'ıyla birebir eşleşir. |
| Ev ve çevre | Kiler/alet/garaj/komşular mekânsal bağlam sağlar | Dekorun görev vaadi, mantıksız ölçü/oda erişimi | Yeni görev eklemeden işlev/erişim okunur. |
| Üç mod | Kısa öğrenme, serbest kampanya, tek afet denemesi ayrı amaç taşır | Sim kaybını kampanya başarısı; kısa karneyi highscore gibi gösterme | Başlangıç ve sonuçta mod amacı doğru anlaşılır. |

**Anlatı incelemesi:** "Kimim, burada neden hazırlık yapıyorum, şimdi ne yapmalıyım, bunun afette karşılığı ne, bir sonraki koşuda neyi değiştireceğim?" cevapları metin yığını olmadan mevcut amaç/dünya/sonuçtan çıkıyor mu? Eksik anlam için önce hedef adı, sıralama, kadraj, nesne geri bildirimi veya sonuç cümlesi düzeltilir. Yeni sinematik ya da lore ilk çözüm değildir.

Kısa modda oyuncuya görünen metin/seste jüri/demo sunum dili aranmaz; ürün dili HAZIRLIĞA BAŞLA, SERBEST OYUN ve ilgili simülasyondur. Planın iç yönetim terimleri oyun HUD'sine eklenmez.

## 13. Afet temasının doğruluğu ve sınırları

Tema denetimi gerçek dünyada teknik sertifika veya eğitim etkinliği iddiası vermez. Oyun dengesi olan günlük erzak birimi, %65 hasar eşiği, hasar katsayısı, güçlendirme kademesi ve VFX bilimsel doğruluk diye anlatılmamalıdır. Sabitleme işi uzman mühendislik uygulaması gibi vaat edilmez.

- Deprem mesajlarını, hazırlık ile sarsıntı anındaki eylemi ayırarak denetle; oyunun oyuncuyu sarsıntı sırasında merdivene/çıkışa koşturan bir hedefle ödüllendirip ödüllendirmediğini araştır. Referans: [AFAD deprem öncesi, anı ve sonrası](https://afad.gov.tr/deprem-oncesi-ani-ve-sonrasi-alabileceginiz-onlemleri-biliyor-musunuz).
- Selde güvenli yüksek hedef ve akan sudan kaçınma iletişimini; özellikle bodrum/bunker yönlendirmesiyle çelişki olasılığını kontrol et. Referans: [AFAD Bursa — Sel](https://bursa.afad.gov.tr/sel).
- Yangında ekipman hazırlığının tahliye amacını gölgelemesini ve genel hortum/su mesajının her yangına uygunmuş gibi anlaşılmasını kontrol et. Referans: [AFAD AFADEM — Yangın](https://afad.gov.tr/afadem/yangin).
- Fırtınada dış eşya sabitleme/korunma ile olay sırasında dışarı çıkmaya teşvik arasındaki farkı denetle. Referans: [MGM — Rüzgâr Fırtınasından Korunma](https://mgm.gov.tr/arastirma/dogal-afetler.aspx?s=kuvvetliruzgar).

Bu resmi kaynaklar 17 Ekim 2026 araştırmasında referans olarak bulundu; AFAD sayfalarının bir kısmında doğrudan sayfa açma hatası oluştu, ilgili metin arama görünümünden kontrol edildi. Yürütmede kritik tema bulgusu açılırsa tam ilgili resmi metin yeniden açılır ve iddiaya bağlanır. MGM sayfası tam açıldı. Kaynakların desteklemediği oyun katsayısı/performansı onlara atfedilmez.

Olası çelişki bu planda doğrulanmış ürün hatası sayılmaz. Tespit edilirse `THEME_RISK` ve ilgili ekran/ses/zaman bilgisiyle raporlanır. İlk fix yanlış yönlendirmeyi ve metni düzeltmek olabilir; ana kayıp/bunker/hasar kuralları bu bahaneyle sessiz değiştirilemez. Mekanik değişimi gerekiyorsa somut etki ve sözleşme kararı ayrı kaydedilir.

## 14. Ses, karakter, hareket ve efekt incelemesi

Kaynakta ses dosyası olması dinleme kabulü değildir. Kısa akışın seçili yedi konuşma ve üç SFX'i; kampanyanın afet sesi politikası; eski ertelenen konuşmaların sızmaması ayrı incelenir. Her klip için tetik anı, bir kez çalma, iptal/skip/tekrar, altyazı, Türkçe telaffuz, anlaşılabilirlik ve diğer sesle maskeleme kaydedilir. Hoparlör ve kulaklık; düşük/normal ses; sessiz durumda bilgi kaybı kontrol edilir. Yeni ücretli kayıt bu planla yetkilendirilmez.

Karakteri yalnız portrede değil normal zoom, yürüme/koşma/zıplama/merdiven, iki vida işi ve dünya montajında gör. Yüz/oran/kıyafet, el-alet-vida teması, ayak kayması/zemine gömülme, dönüş, klip geçişi, root-motion politikası ve hız×1,35 animasyon uyumu incelenir. Yeni rig veya controller boyut değişimi fix kestirmesi değildir.

Afetlerde tehlikenin ilk okunması, kamera sarsıntısı, toz/su/duman katmanı, alpha sıralaması, yağmur/foam akışı ve mekânla temas incelenir. İnceleme ve restart sonrası aktif alev/dumanın uygunsuz kalıntısı; büyüyen parçacık/ışık/ses sayısı; helikopterin iç evde görünmesi kontrol edilir. Dere fiziksel akışkan solver, NPCler görev simülasyonu gibi raporlanmaz.

## 15. İlk oyuncu ve alıcı denemeleri

En az üç oyunu bilmeyen kişi; mümkünse biri seyrek oyuncu, biri klavye/fareye alışık, biri benzer oyunlara alışık. Geliştiricinin kendi hızlı rotası bu örneklemin yerine geçmez. Katılımcılar otomatik ajan tarafından taklit edilmez. Erişim yoksa insan kapısı BLOCKED, kalan testler yürür.

**Tarafsız giriş metni:** "Bu oyunu ilk kez açtığını düşün. Ekrandaki bilgilerle başlayıp hazırlık akışını oyna. Takıldığında ne aradığını söyleyebilirsin. Ben yönlendirmeyeceğim; bırakmak istersen bırakabilirsin." Zamanlamalı ilk koşuda sürekli sesli düşünme zorunlu tutulmaz; süreyi yapay uzatabilir. Ayrıntılı görüşme koşudan sonra yapılır.

**Dış saat olayları:** exe/başlık hazır zamanı, start tıklaması, ilk amaç okuma, ilk kontrol, ilk gerçek etkileşim ≤30 sn hedefi, küçük kazanım yaklaşık 45–60 sn, ilk biten önlem yaklaşık 100 sn, ilk anlamlı afet sonucu ≤130 sn, ilk tam döngü ve normal rotada sel örneği ≤180 sn, karne hedef ≤270 sn, mutlak kapanış ≤300 sn. Hedefler tasarım bütçesidir; aşılırsa bug/UX bulgusu açılır, kayda müdahale edilmez.

Her duraksamada süre/ekran/aranan şey/yanlış varsayım/yardım isteği tutulur. Yardım verilirse an ve tam söz kaydedilir; koşu `ASSISTED` olur, yardımsız kabul sayılmaz. Menü, okuma, kapı, iş, hata ve geçişler süreye dahildir. Soğuk açılış dışarı alındıysa ayrı ölçüm ve gerekçe yazılır. Hızlı oyuncu bekletilmez.

**Koşu sonrası sorular:** Amacın neydi? En çok neyi aradın? Yaptığın hangi önlemin sonucunu gördün? Kaybettiysen neden? Hangi hareket gereksizdi? Deprem/sel/yangın hazırlıkları nasıl ayrılıyordu? Tekrar oynasan neyi değiştirirsin? Görüntü/ses ürün hissini nerede bozdu? Devam etmek ister misin; neden? Cevap yönlendirilmez; önce serbest cevap sonra 1–5 değerlendirme alınır.

Rapor: kişi başına gerçek süre, tamamlanan/atlanan eylem, yardım, duraksama, neden–sonuç açıklaması, görsel/ses sorunu ve devam isteği. 3 kişinin 2'sinin beğenmesi pazar kabulü değildir. Önerilen küçük örneklem kapısı: üç kişinin tümü yardımsız ilk amacı bulabilsin ve bir gerçek önlemi açıklayabilsin; başarısız kişi varsa etkisi açık kalır. İlk180 sn hedefi ve 300 sn kapanış ayrı ölçülür.

## 16. Performans, oturum dayanıklılığı ve paket

Önce referans cihaz/kalite ayarı kaydedilir; henüz tanımlı olmayan minimum sistem gereksinimi uydurulmaz. Önerilen başlangıç hedefi referans cihazda normal oyunda 60 FPS'e yakın akıcılık, kritik yolda p95 frame süresi ≤33,3 ms; 100 ms üzeri takılmalar an/zaman/olayla raporlanır. Bunlar mevcut onaylı SLA değil, ölçülüp kararlaştırılacak kalite hedefleridir. Ortalama FPS tek başına yetmez; p50/p95/p99, en kötü kare, yükleme, GC ve bellek trendi ayrılır.

Başlık/ilk dolaşım, yakın kurulum, yoğun afet, dış çevre/dere/NPC, üst kat ve sonuç geçişi örneklenir. 30–60 dk dayanıklılık ve 10 kısa retry/menü döngüsünde bellek/nesne/ses/parçacık trendi kontrol edilir; bu geniş ilk/son değerlendirmedir, her fix paketinde tekrarlanmaz. Development/profiler ek yükü normal portable paketten ayrıdır. Ekran kaydının etkisi ölçümde not edilir.

Temiz Unicode/boşluklu dizin; paketten normal exe/launcher; eksik bağımlılık; lisans/fontlar; offline açılış; Windows odak kaybı ve resize/DPI; bozuk/boş/okunamayan test skor dosyası; normal quit/tekrar açma kontrol edilir. Gerçek kayıt üzerinde bozma/izin deneyi yapılmaz. Bozuk test profili kontrollü kopyada hazırlanır.

ZIP içindeki DLL ile native test edilen DLL eşleşir. Yerel paket testi uzak GitHub yayınına kabul değildir. Bu plan yayın yapmaz. Yürütme sonunda son doğrulanmış build/tek portable ZIP politikası uygulanırken açık kullanıcı süreçleri tekrar kontrol edilir; yeni aday geçmeden çalışan taban temizlenmez.

## 17. Kanıt dosyası düzeni ve rapor standardı

Her yürütme için yeni `Evidence/GamejamOrchestra/TLTP-E2E-RUN-YYYYMMDD-HHMM/` alanı:

```text
00-baseline/    contract, behavior-profile, source manifest, build identity, machine
01-source/      gereksinim ve hedef envanteri, kaynak inceleme notları
02-model/       seed/fixture tanımları ve gerçek sonuçlar
03-native/      GUID profilleri, JSON, player/build logları
04-os-e2e/      normal input rota, video, olay çizelgesi, screenshots
05-visual/      ekran manifesti, temiz/işaretli PNG, puan ve önce-sonra
06-human/       anonim kişi kodu, süre ve gözlem, yardım kayıtları
07-issues/      bug ledger, tekrar dosyaları, tema/UX/görüş ayrımı
08-fixes/      paket planı, değişiklik receipt'i, staging QC, retest
09-release/    kapanış, manifest, ZIP kimliği, açıklar ve teslim kararı
```

Kanıt referansı `run_id + case_id + build_id + dosya + SHA256 + timestamp` içerir. Ekran görüntüsü adı örneği `EXP-023_fire_after_1600x900_camera03_before.png`; after karşılığı aynı koşulla çekilir. Hash görüntünün özgünlüğünü takip eder, içeriğin doğru olduğunu tek başına kanıtlamaz.

**Her günlük/ara raporun sırası:** test edilen tam sürüm → kapsanan/kapsanmayan alan → kritik bulgular → vaka sonuç tablosu → görsel/tema/insan gözlemi → önerilen fix paketleri → yeniden test → açık sınırlar. Kırmızı/yeşil toplamın yanında BLOCKED/INVALID/NOT_RUN görünür. Birinci denemede bulunan hata, fix sonrası rapordan silinmez.

## 18. Bug kaydı ve önceliklendirme

Her bulgu tek issue_id ile kaydedilir: kısa başlık; tür (`FUNCTIONAL`, `UX`, `VISUAL`, `AUDIO`, `THEME`, `PERFORMANCE`, `DATA`, `HARNESS`); kanıt durumu (`CONFIRMED_ON_BUILD`, `SOURCE_CANDIDATE`, `HISTORICAL_REVERIFY`, `REVIEWER_OPINION`); build/DLL/contract; test profili/seed; önceki durum; gerçek adımlar; beklenen/olan; tekrar oranı; görsel/klip/log; kullanıcı etkisi; severity; priority; kök neden hipotezi; owner; fix ve retest kimliği.

**Severity** arızanın etkisi; **priority** hangi sırada düzeltileceğidir. Öneri:

| Seviye | Etki örneği | İş sırası |
|---|---|---|
| S0 / P0 | Crash, gerçek veri kaybı, açılmama, temel akış tamamen kilitli | Diğer üretim durur; gerçek kayıt/çalışan build korunur. |
| S1 / P1 | Hedef bitmiyor, çift maliyet, yanlış skor/hasar/kayıp, ana tuşa erişilemiyor, ciddi yanıltıcı tema mesajı | Teslimi engeller; önce bu aile düzeltilir. |
| S2 / P2 | Yan rota, okunurluk, etiket çakışması, belirgin görsel/ses/tempo sorunu; geçici yol var | İlk180 sn/ana kadrajda etkisine göre erkene alınır. |
| S3 / P3 | Küçük mesh/ışık/metin cilası, akışı durdurmuyor | Paketlenir; saatlerce mikro onarım yapılmaz. |

Aynı kök nedenin yüzlerce tekrarını kopya issue üretmek yerine etkilenen hedef listesine bağla. Farklı kullanıcı etkilerini tek bulanık "UI bozuk" maddesine gömme. Önemli görsel kusur işlevsel olmadığı için otomatik P3 yapılmaz. Alıcı güvenini/eylem okunurluğunu bozan kusur P1/P2 olabilir.

## 19. Ön fix kuyruğu — bulgu doğrulandıktan sonra somutlaştırılacak

Bu tablo uygulanan düzeltmeler değildir. İlk gerçek testten sonra her paket dosya listesi, doğrulanmış kök neden, kabul vakası, rollback ve tahmini riskle doldurulur.

| Paket | Öncelik/koşul | Düzeltme odağı | Kapanış kanıtı |
|---|---|---|---|
| FX00 | Her durumda | Kaynak/build/oracle uyuşmazlığı ve test harness yanlış pozitifleri | Snapshot–DLL–kanıt tutarlı; geçersiz test düzelir, eski FAIL korunur. |
| FX01 | Kanıtlı P0/P1 varsa | Açılış, faz/skip/timeout, sonuç, kayıt/skor ve maliyet atomikliği | Aynı tekrar başarısızlıktan PASS'e; komşu sonuç/ekonomi testleri. |
| FX02 | V4.9 kabul boşluğu varsa | İki vida hit alanı, 0,60 sn kurulum, uygunsuz eylem reddi, yangın istasyonu, ağaç bölgeleri | Gerçek tık ve sonlu stokla maliyet/state/görsel eşleşir. |
| FX03 | Fizik/girdi kusuru varsa | Kapı/merdiven/yerleşim/kamera/modal önceliği | Aynı rota ve aynı-kare negatifleri; kapsül/kurallar değişmez. |
| FX04 | İlk180 sn hedefi kaçıyorsa | Hedef işareti/metin/mesafe/tekrar yükü ve neden–sonuç | Yeni oyuncuyla tekrar; ≤180/270/300 ayrı rapor. Hız veya hasar eşiği kestirmesi yok. |
| FX05 | Görsel/tema/audio bulgusu varsa | Ana kadraj, etiket çakışması, insan/animasyon, donanım/geometri ve yanlış anlatım | Aynı tam kadraj önce–sonra, hareket klibi/dinleme, ilgili model state kontrolü. |
| FX06 | Ölçülmüş performans sorunu varsa | Efekt/nesne/mesh/GC darboğazı; hedefli iyileştirme | Aynı cihaz/kalite/rota frame ve bellek karşılaştırması; görsel kayıp ayrıca incelenir. |
| FX07 | Son aday | Paket, launcher, doküman, lisans ve tek ZIP | Final manifest/normal açılış/portable eşleşmesi ve açık sorun listesi. |

İlk devirde bekleyen somut maddeler: V4.8 dış hasar etiket çakışmasını yeni adayda tekrar üret; eski etkileşim B01–B05 onarımlarının yeni girdi tasarımında korunmasını doğrula; ham OS pointer/NumPad Enter/son-karesinde iptal ve yeni doğrudan kurulumları yeni DLL'da dene. Bunların hepsini yeni doğrulanmış bug diye işaretleme.

## 20. Her fix paketinin değişmez çalışma döngüsü

1. **Tekrar üret:** QA somut vaka ve kanıtı verir. Ürün kusuru / test kusuru / yeni beklenti / tercih ayrılır.
2. **Kök neden:** developer hangi state/olay/hesap/mesh bağlantısının kırıldığını ve en küçük onarımı yazar. Sadece semptomu saklama veya test assert'ini zayıflatma onarım değildir.
3. **Paket sözleşmesi:** etkilenen dosya/API, korunacak kural, önce–sonra davranış, kabul vakaları, görsel tekrar kadrajı, rollback snapshotı yazılır.
4. **Staging:** benzersiz alanda tek yazar; sanat kaynak/UV/rig/lisans QC; entegrasyon mevcut API'ye bağlar. Aynı ortak dosya iki coder'a verilmez.
5. **Root entegrasyonu:** kaynaklar birleştirilir ve dondurulur; tek import/bake gerekiyorsa bake, tek build. Runtime-only değişimde gereksiz bütün art rebuild'i yoktur.
6. **3–5 dk ilgili smoke:** etkilenen E2E + maliyet/iptal/reset + komşu riskli yol. Somut hata varsa dar düzeltme/tekrar; geniş tur risk varsa yapılır.
7. **Bağımsız QA:** developer'ın önerdiği mutlu yolun yanında negatif ve önceki tekrar adımları; farklı kişi incelemesi mümkün değilse bağımsız gözlem/veri yöntemi ve sınır yazılır.
8. **Görsel/hissel kapanış:** aynı build ve kadrajla önce–sonra; hareket sorunu videoyla; ses insan dinlemesiyle; ilk oyuncu sorunu yeni denemeyle. Yalnız kaynak fix'i CLOSED değildir.
9. **Kayıt:** `FIXED_SOURCE → BUILT → RETEST_PASS → VERIFIED_CLOSED` aşamaları; başarısız tekrar `REOPENED`. Artık geçerli olmayan beklenti `SUPERSEDED_BY_CONTRACT`; ürün bug'ı kapanmış gibi sayılmaz.

Fix'in başarı ölçütü örneği: "Etiket iyi görünüyor" yerine "Aynı dış inceleme kamerasında en etkili üç hasar etiketi, 1280×720 ve 1600×900'de birbirini/ana eylemi örtmüyor; hedefle bağ çizgisi doğru; hasar sayıları modele eşit." Önce–sonra ve ilgili regresyon linkleri zorunludur.

## 21. Ekip ve sahiplik

Root tek canlı izometrik Unity/editor/import/build/player sahibi; toplam en fazla dört ajan. Model görevi başında gerçek model/thinking uygunluğu kaydedilir; sessiz ikame yapılmaz. Bu belge çalışan scheduler değildir.

- Yönetim/kritik QA: `gpt-6.1-sol / ultra`; kontrat, risk, kabul ve sürüm bağını yönetir.
- Sanat/UI/animasyon/VFX/ses ve kısa QC: standart `gpt-6.1-sol`; yeni sanat ve dosya/lisans sorumluluğu.
- Entegrasyon: `gpt-6-luna / xhigh`; onaylı sanatın mevcut API'ye bağlanması, yeni sanat/klip/shader tasarlamaz.
- Paket QA: `gpt-6.1-sol / ultra`; bağımsız inceleme; canlı çalıştırma yine root'a aittir.

En fazla üç alt ajan; onlar yeni ajan açmaz. Rolling queue ile sonraki staging paketi öncekinin native smoke'unu beklemeden hazırlanabilir; root canlı dosyaya yalnız tek sıralı entegrasyon yapar. Diğer sohbetin aktif üretimi varsa bu plan yeni Unity sahibi açmaz. İnsan katılımcı ve dinleyici yerini ajan puanı alamaz.

## 22. Durdurma, yeniden açma ve kapsam kontrolü

Crash/veri kaybı/temel döngü kilidi/gerçek kayıt değişimi varsa kritik koşu durur, kanıt korunur ve P0/P1 açılır. Kilitli OS oturumu veya gerçek kullanıcı penceresine girdi riski varsa OS testi BLOCKED olur; kilit aşılmaz. Araç erişimi yoksa sentetik callback fiziksel test diye etiketlenmez.

Planlı testin kendisi state'i bozduysa ürün kusuru kararı verilmeden fixture onarılır. Non-deterministik olay aynı seed/başlangıçla yeniden denenir; tekrar oranı saklanmaz. Süre sınırı geçince başarısız adımın yeri/harcanan süre raporlanır. Tekrar testinin geçmesi için collision, hasar/kayıp sınırı, malzeme veya bütçe kuralı gevşetilmez.

FPS, gerçek kayıt, açık kullanıcı oyunu ve kullanımda build klasörü korunur. Tarihsel manifestlerin özgün yol/hashleri toplu değiştirilmez. Planın hazırlanması mevcut kaynakların temizlenmesine veya uzak yayın yapılmasına yetki değildir.

## 23. Nihai kabul kapıları

Her kapı ayrı `PASS / FAIL / BLOCKED` alır; toplam assert sayısı yeterli kabul değildir.

| Kapı | Geçiş ölçütü |
|---|---|
| G0 İzlenebilirlik | Sözleşme/aktif davranış profili, final kaynak ve DLL/asset/ZIP bağı tek; açıklanmayan drift yok. |
| G1 Teknik doğruluk | Açık P0/P1 yok; uygulanabilir kritik model/entegrasyon/ekonomi/kayıt/girdi testleri PASS; bilinen başarısız raporlar görünür. |
| G2 Gerçek E2E | Kısa akış ve dört simülasyonun OS yolları; gerçek kapı/merdiven/çanta/kurulum/sonuç/tekrar; test Warp/Tick/cheat yok. |
| G3 Kampanya | İlk başarı/üç kayıp/kalıcı state ve dalga sınırları; doğal oynanan ve fixture ile doğrulanan kısımlar ayrı belirtilir. |
| G4 Süre ve anlama | İnsan denemelerinde ilk180 sn ve 270/300 sn sonuçları ölçülmüş; yardım ve başarısız katılımcı gizlenmez. Ölçülmediyse BLOCKED. |
| G5 Görsel ve işitsel | Ekran/oda/afet envanteri tamam; kritik okunurluk engeli yok; ana kadrajlar aynı adayda incelenmiş; dinleyerek ses kabulü yapılmış. |
| G6 Hikâye/tema | Önlem–sonuç–karne tutarlı; yanlış güvenlik mesajı açık P1 olarak kalmıyor; hesaplanmamış başarı/önlenen hasar iddiası yok. |
| G7 Performans/paket | Referans cihaz sonuçları ve açık limitler; temiz offline portable açılış; normal launcher ve aynı DLL; gerçek kayıt hashleri korunur. |
| G8 Fix kapanışı | Her kapalı bulguda fix kimliği + aynı tekrar PASS + ilgili regresyon + gerekiyorsa önce–sonra/human tekrar var. |

P2/P3 açıklar sahip, etki, kanıt ve sonraki sıra ile teslim raporunda görünür. "Kaynak tamam", "native smoke geçti", "OS E2E geçti", "insan anladı", "sanat/ses kabul edildi" ayrı satırlardır. İnsan kapısı tamamlanmadan "tam oynanış kabulü" yazılmaz. Yayına hazır kararı bu kapıların sonuçlarına dayanır; bu planın yazılmasıyla verilmez.

## 24. Yürütmeyi devralacak kişiye iş emri

> Bu planı ve güncel AGENTS/sözleşmeleri oku. Önce aktif üretim sahibinden tek Unity sahipliğini doğrula, kaynak/DLL/contract baseline'ını ve sürüme uygun davranış profilini oluştur. Katalogdaki uygulanabilir testleri geliştirici, QA ve alıcı bakışlarıyla yürüt; gerçek OS ve insan kontrollerini API fixture'ından ayır. Her bulguyu kaynak/build kimliği ve özgün kanıtla raporla. Kanıtlı bulgular için somut fix paketleri hazırla; mevcut kuralları koruyarak staging'de düzelt, root ile birleştir, paket başına tek birleşik build ve 3–5 dk ilgili smoke uygula. Aynı hatayı bağımsız tekrar, komşu regresyon ve görsel/hissel önce–sonra ile kapat. Açık P0/P1 varken teslim geçti deme. İnsan/ses/uzun kampanya veya OS erişimi yoksa ilgili kapıyı açık bırak ve nedenini yaz. FPS, gerçek kayıtlar ve açık kullanıcı oyunu korunur.

## 25. Eklerin kullanımı

- `TEST-KATALOGU.md`: her çekirdek vakanın önkoşulu, gerçek adımları, beklenen sonucu, kanıtı ve regresyon bağı.
- `test-cases.json` / `.csv`: vaka kaynağı; `PLANNED` durumundadır. Uygulanabilirlik ve sonuçlar ayrı koşu dosyasında tutulur.
- Nihai vaka öncelikleri bu birleşik katalogdadır; staging kataloglarının öncelikleri root'un ilk22 P0 yürütme kapısına göre gözden geçirilmiştir. Karar receipt'i `manager/priority-review.json` içindedir; bug severity kararı değildir.
- `SABLONLAR.md`: koşu, bug, fix, görüntü yorumu, insan oturumu ve kapanış şablonları.
- `run-results-template.csv`: boş sonuç kaydı; PASS örnekleriyle doldurulmamıştır.
- `fix-backlog-template.csv`: boş düzeltme takibi; doğrulanmamış kusur oluşturmaz.
- `quality-policy-template.json`: F0'da seçilecek tek kalite eşik seti; sözleşme kararları ve önerilen insan/performans/soak eşikleri ayrıdır.
- `index.html`: offline arama/filtre, vaka ayrıntıları, aşamalar, kalite kapıları ve kanıt ayrımı; web servisi veya harici font gerektirmez.
- `PLAN-VALIDATION.json`: yalnız plan dosyalarının tutarlılık kontrolü. Oyunun test sonucu değildir.

Kaynak incelemeleri: [mekanik](../Evidence/GamejamOrchestra/TLTP-E2E-MASTERPLAN-20261017/mechanics-plan/source-audit.md), [deneyim ve hikâye rubriği](../Evidence/GamejamOrchestra/TLTP-E2E-MASTERPLAN-20261017/experience-plan/visual-story-rubric.md), [kapsam ve test aracı](../Evidence/GamejamOrchestra/TLTP-E2E-MASTERPLAN-20261017/coverage-plan/coverage-audit.md). Üretim geliştikçe bu ekler tarihli plan snapshotı olarak kalır; final adayın oracle'ı yürütme başlangıcında yenilenir.
