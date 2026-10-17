# V4.9 E2E — koordinatör bulgu kuyruğu

Bu dosya E2E yürütmesinin koordinatör kayıt alanıdır. Üretim root'unun kendi native/fix kayıtlarıyla karıştırılmaz. Koordinatör staging/read-only sınırında kalmıştır. Kaynak/önceki aday kanıtı final adayın native kabulü değildir.

Güncelleme: canlı Unity/source sahibi mevcut interaction QA sohbetidir. Koordinatör canlıya yazmadı; aşağıdaki üç onarım o sahip tarafından candidate-v2 DLL81868C12… üzerinde native tekrar edildi. Henüz final yayımlanmış paket/LATEST kabulüyle birleştirilmez. Diğer kategori onarımları staging'dedir.

| Sıra | Bulgu | Öncelik | Güncel sonuç |
|---|---|---|---|
| 1 | E2E-005 ağaç gövdesi actor içinde kuruluyor | P1 | Candidate native fix/retest; fizik24/24, geçerli kurulumdirect44/44 |
| 2 | E2E-004 ücretli ankraj çözülemiyor | P2 | Candidate native fix/retest; mekanik338/338 |
| 3 | E2E-001 simülasyonda bunker uyarısı | P2 | Candidate native model60sn matrisi geçti; doğal HUD/OS ayrı |
| 4 | VIS49-001 hasar katkısı etiketleri çakışıyor | P2 | Final rendered bug; bounded HUD patch ve companion hazır |
| 5 | E2E-002 vida işinde hedef adı yok | P2 | Final rendered omission; aynı HUD paketinde hedef etiketi |
| 6 | UXA-003 evreye uymayan eylem satırı | P2 | Final3/3ilgiliPNG; copy-only patch hazır |
| 7 | E2E-003 kanopi karakteri örtüyor | P2 | Final rendered bug; hareket/kamera atlası ve dar sanat fix'i açık |
| 8 | E2E-006 T tercihi eziliyor | P2 adayı | Sözleşme/oracle ve gerçek Work.Action gözlemi açık |
| 9 | SYS-F002 eski QA bayraklarının profil/tek sürücü açığı | P1 harness | Kaynakla doğrulanan QA altyapı açığı; güvensiz çağrı yapılmadı |
| 10 | SYS-F001 desteklenmeyen skor şeması | P2 adayı | .NET193/194→staging194/194; gerçek Unity JSON açık |
| 11 | UXA-001 karne10adımı10önlem diyor | P3 | Final rendered + model; literal patch hazır |
| 12 | UXA-002 oyuncu metninde wave kalıntısı | P3 | Source+model;6literal patch hazır |

Bu tablo257vaka geçiş oranı değildir. Her yeni üretim DLL/paketine source/hash ve yeniden test bağı gerekir. Altı kategori raporu kalan görsel tercihleri, tema risklerini ve insanla ölçülecek soruları ayrı tutar.

## E2E-001 — Simülasyonun son60sn uyarısı kampanya bunker hedefini söylüyor

- Tür: UX / THEME / FUNCTIONAL_COPY. Öncelik P2; kategori03 gerçek üretim C# modelinde tekrar üretti.
- Kanıt: `categories/03-physics/model/results-baseline.json`:75 alt kontrolün71'i geçti, dört simülasyonun60sn uyarısı aynı yanlış bunker hedefini verdi. `results-fixed.json`:aynı75 kontrolün75'i staging mesaj ayrımıyla geçti. Unity model shim kullanıldı; native zaman/görüntü kanıtı değildir.
- Etki: dört bağımsız simülasyonda asıl eylem Afeti Başlat olduğu hâlde oyuncu fiziksel bunkera yönlendirilebilir; simülasyon ile kampanya amaçları karışır.
- Fix planı: yalnız IsSimulation metnini uygun hazırlık/afet başlatma eylemine ayır; normal kampanya uyarısı,90sn süre, hasar, bunker ve sonuç kuralları korunur.
- Retest: aynı60sn model geçişi önce/sonra; doğal90sn simülasyonda30sn sonra gerçek HUD uyarısı; normal kampanyada240sn sonra değişmeyen bunker uyarısı; dört afet ve deadline sebepleri.
- Durum: FIXED_ON_QA_CANDIDATE / SOURCE+MODEL_RETEST_REVIEWED. Native `matrix-v2-10453c33-7356-40bc-8059-cebe20c095a8/direct-matrix.json` supplementary satırlarında dört simülasyon ve kampanya60sn fixture sonucu PASS yazıyor;338mekanik assertion listesinde bu kontroller yok. Supplementary satırları string kaydıdır, doğal30snHUD/OS görüntüsü veya ayrı338assertion gibi sayılmaz. Final Model hash'i75/75staging fix ile eşleşir. Yeni test DLL81868C12…; teslim DLL4761CB26… üzerine aktarılmaz.

## E2E-002 — Sabitleme yakın planında hedefin kimliği görünmüyor

- Tür: UX / VISUAL / STORY_CAUSE_EFFECT. Öncelik P2.
- Baseline kanıtı: eski direct-v2 incelemesine ek olarak koordinatör final direct-v5 `01-click-screws.png` karesini tekrar gördü; DLL4761CB26…; mevcut DrawWork kaynak yolu. Final karede de eşya adı ve duvar/zemin yöntemi yok.
- Gözlem: vida tık hedefi okunur, ancak iki genel bağlantı aparatı görünen yakın plan seçilen buzdolabı/kitaplık adını göstermiyor. Mekanik çalışırken “hangi eşyayı, nereye sabitliyorum?” bağlantısı zayıf.
- Fix planı: seçili gerçek Work.TargetId'nin Model.Items etiketini sidebar'da ve duvar/zemin yöntemini görünür yap. Vida geometrisi/hit alanı/maliyet/hız değişmez. Küçük kaynak patch'i `manager/staging/work-target-context.patch`; henüz uygulanmadı.
- Retest: buzdolabı ve kitaplık/duvar-zemin ayrımı;960×540,1280×720,1600×900; aynı vida noktası ve iptal/tamam maliyeti; aynı kadraj önce–sonra. Final baseline'da yeniden doğrulanacak.
- Durum: CONFIRMED_FINAL_RENDER / FIX_STAGED; düzeltilmiş native render ve OS retest bekliyor.

## E2E-003 — Yeni ağaç kanopisi bazı kadrajlarda karakteri örtüyor

- Tür: VISUAL / UX. Öncelik adayı P2.
- Baseline kanıtı: eski direct-v2 incelemesine ek olarak koordinatör final direct-v5 `tree-plant_west1.png` karesini tekrar gördü; DLL4761CB26…. Karakterin başı ve gövdesinin çoğu üçüncü ağacın arkasında. Kategori04 bağımsız final kare incelemesi de aynı kusuru bildirdi.
- Gözlem: kurulum sonrası dış kamera planında karakter kanopinin arkasında büyük oranda görünmez. Normal kameraya dönüş/orbit, hareket ve fiziksel yol henüz bu E2E tarafından denenmedi; kalıcı hareket engeli iddia edilmez.
- Triage: kategori04 final renderı ve kısa hareket klibiyle tekrar üretir. Yalnız geçici kadraj kusuruysa dar kamera/okunurluk fix'i; fiziksel colliderları kaldırma/küçültme yok. Karakterin görünümünü güzelleştirme iptali bu kusurun gerekçesiyle geri açılmaz.
- Durum: CONFIRMED_FINAL_RENDER; normal hareket/kamera devamlılığı ve dar onarım bekliyor. Tek kareden fiziksel sıkışma sonucu çıkarılmaz.

## E2E-004 — Sabitlenmiş uygun mobilyada çözme dalı erişilemiyor

- Tür: FUNCTIONAL / REVERSIBILITY. Öncelik adayı P2.
- Kaynak kanıtı: `PanicItemActionRules.PrimaryAction` anchored mobilyada boş döner; PrimaryShortcut0 olur. `PanicHomeWorld.QuickAction(1)` bu yüzden mevcut Model.ToggleAnchor dalına ulaşmadan reddedilir. HUD da shared helper üzerinden işlem göstermiyor.
- Model tekrar kanıtı: koordinatör `manager/anchor-candidate` altında gerçek PanicModel ve shared ActionRules ile674 alt kontrol çalıştırdı. Baseline657/674;17 arıza uygun anchored mobilyada shortcut/etiket ve çöz→yeniden sabitle zincirindedir. Dar shared-helper düzeltmesiyle aynı674/674 geçti. Shim yalnız Math/JSON sağlar; native World/HUD/OS çalıştırılmadı.
- Native baseline: mevcut Unity sahibi interaction QA'nın `execution-v49/mechanics-v1-b6142417-da16-46d3-af04-0b9f0ed04aaf/e2e-mechanics.json` raporunda333alt kontrol332PASS; tekFAIL `world.release-anchor`. Gerçek üretim adapteri `Bu nesnede uygulanabilir işlem yok` mesajıyla anchored kalıyor. Yeni test DLL4DC8A8BC…; teslim DLL4761CB26… ile karıştırılmaz.
- Durum: FIXED_ON_QA_CANDIDATE / NATIVE_RETEST_REVIEWED. `mechanics-v2-08e168ac-1b91-4caf-9733-fbc14ce719ac`338/338 ve hata boş; helper yalnız kurulu, appropriate, anchored, AnchorOwned mobilyada çözmeye izin veriyor. Worldguard ve görünen ÇÖZ·1 düğmesi aynıhelperı kullanıyor. Gerçek OS tık/girdi kabulü hâlâ ayrı. Koordinatörün alternatif model patch'i canlıya alınmış sayılmaz.
- Fix tasarımı: yalnız uygun, kurulu, anchored mobilyada ortak CanReleaseAnchor kuralı ile açık ANKRAJI ÇÖZ etiketi ve1kısayolu. İş tarifi olarak yeni action ekleme; mevcut maliyet/iade ve reusable AnchorOwned davranışı korunur. Desteklenmeyen nesnelere genel guard gevşetilmez.
- Retest: sabitle→çöz→taşı→yeniden sabitle; maliyet ve iki vida tekrarları; uygunsuz mobilya/yapı/istasyon yine reddedilir.

## E2E-005 — Yeni ağaç gövdesi oyuncunun içinde kurulabilir

- Tür: PHYSICS / SOLID_PLACEMENT. Doğrulanırsa P1.
- Kaynak adayı: plant_west1 gelecekteki ikinci gövde merkezi(-10.94,0,2.95), site merkezine1.052m; etkileşim menzilinin içinde. BeginWork/CompleteWork yeni gövde–gerçek controller örtüşmesini denetlemiyor görünüyor.
- Native baseline: interaction QA `execution-v49/physics-v1-27581d50-8fc6-4a5e-8d3c-4400ffff2440/native-e2e-physics-audit.json` gerçek kurulumda0.2130195m overlap, sonraki sıfır girdi controller hareketinde0.2443323m itme kaydetti.4ağaç kiti3'e, bütçe1000'den970'e indi. Koordinatör ham rapor ve PNG'yi gördü. Exact seat reach alt kontrolü0.16m uzakta kalıp başarısızdır; o fixture kusuru mevcut gerçek örtüşmeyi geçersiz kılmaz.
- Fix: canlı sahibi BeginWork ve CompleteWork öncesi gerçek controller kapsülünün dört gövde ile mesafesini denetliyor; çatışmada commit öncesi maliyetsiz red. Trunk boyutu/collider/karakter ölçüsü/kit fiyatı korunuyor.
- Durum: FIXED_ON_QA_CANDIDATE / NATIVE_RETEST_REVIEWED. `physics-v2-ad89b519-5c3e-4eeb-afe7-bf8d507790ed`24/24; aynı actor(-11.10,-.01,2.95) konumunda beganFalse/committedFalse/overlap0; kit4/4, bütçe1000/1000. Sonraki controller drift0.00342083m: önceki0.244m itme yok, tam0 iddiası değil.24kontrolden precondition exactcenter yerine bağımsız capsule-overlapvolume olarak düzeltildi; ürün güvenlik eşiği gevşetilmedi. Geçerli normal ağaç kurulumları ayrı direct-v2 44/44 içinde sınandı. Teslim paketine terfi ve OS tam rota açık.
- Fix ilkesi: gerçek controller.center, boyut ve ölçekle gelecekteki solid capsule mesafesini başlatma ve commit öncesi doğrula; çatışma varsa maliyetsiz iptal ve açık mesaj. Collider kapatma/küçültme, actor Warp, bütçe veya fizik eşiğini gevşetme yok.

## E2E-006 — T yüzey tercihi doğrudan eylemde eziliyor olabilir

- Tür: INPUT / CONTRACT. Öncelik adayı P2.
- Kaynak: ToggleAnchorSurface tercihi değiştiriyor, QuickAction(1) sonrasında NearWall ile aynı AnchorMode tekrar atanıyor. Korunan T sözleşmesi ile güncel action helper otomatik seçimi birlikte değerlendirilmeli.
- Durum: SOURCE_CANDIDATE / ORACLE_REVIEW. V4.9 eski mini oyunları kaldırdı; tek başına T tercihini kaldırdığı sonucunu doğurmaz. Native gerçek Work.Action ve HUD yöntemi kaydı bekleniyor.

## Görsel ve tema paketinin somut devamı

- VIS49-001 / E2E-002: `categories/04-visual/staging/fixes/hud-visual.patch` + `PanicWorldMarkerLayout.cs`. Önceki manager target-label taslağının yerine geçer; ikisi birlikte uygulanmaz. Yer ayırma sayıyı/katkı hesabını değiştirmez. Kenarda taşınmış kartın gerçek hedef bağını koruması ve yoğunluk fallback'i native görüntüyle sınanmalı; etiket saklayarak PASS verilmez.
- UXA-003: `categories/05-story-ux-audio/staging/fixes/PanicJuryHud.cs.patch`; depremde SARSINTI SÜRÜYOR, sonuçta SONUCU İNCELE, incelemede EVİ İNCELE, onboardingde İLK ADIMLAR. Düğme/süre/oyun state'i değişmez.
- UXA-001/002: aynı kategori `PanicJuryDemo.cs.patch` ve `PanicModel.cs.patch`. Karne sayısı ve ledger korunur; metin adımlar/dalga olur. Tam stale Model dosyası kopyalanmaz; simulation-warning ile ayrı hunks birleşir.
- Yedi opt-in harness/observer kaynak eklemesi ve sekiz küçük patch, final V5'in63Runtime/Art kaynak kopyasında birlikte Unity managed referanslarıyla derlendi: `manager/combined-managed/{baseline,fixed}.receipt.json`, ikisi exit0. İki ek companion kaynak ve tüm compile girdilerinin hashleri aynı manifesttedir. Unity import/build/native sonucu değildir. Bu birleşik kopya, QA sahibinin yeni tree/AnchorOwned düzeltmelerinin üzerine bütün dosya olarak kopyalanmaz.
- SYS-F002 için `manager/staging/PanicTestLaunchRules.cs` ve `test-launch-guard.patch` hazır. Model kurulmadan önce tek QA sürücüsü, tek evidence argümanı, GUID ve absent isolated score şartı; normal oyuncu/evidence fixture politikası korunur. Saf validator535/535 kontrol geçti; gerçek dosya sistemi yerine açık existence predicate kullanıldı, Unity/player başlatılmadı. Native negatif argümanda modelin kurulmadığı ve exit2sonucu ayrı sınanmalı. Bu source hook henüz canlı değildir.
- E2E-003 için ayrı normal kamera, yaw/pitch ve hareket devamlılığı atlası hazır; canopy/trunk ortak renderer olduğundan rendererı komple kapatmak önerilmez. Karakter görünümünü güzelleştirme iptali korunur.

## Tema riskleri: ürün arızası ile alıcı yorumu ayrımı

UXA-004 kısa sel/yangın etaplarında gerçek outcome çözülüyor, fakat aktif sel/alev olay fazı gösterilmiyor. Final FullFlow `all-three-disasters-visible` ifadesi üçoutcome kaydını sayıyor; üçaktif olay videosu sayılmaz. Bağımsız simülasyon görüntüleri kısa akış kabulüne taşınmaz. Önce gerçek outcome'a bağlı kısa sonuç cümlesi/aynı süre içindeki mevcut kadraj değerlendirilir; yeni görev veya zincirleme afet açılmaz.

UXA-005 çök–kapan–tutun alanına erişim gerçek, fakat hareket/baş koruma pozu bu PNGlerde görünmüyor. Oyuncuya o davranışın öğretildiği kabulü verilmez; mevcut animasyon görünürlüğü/kamera ve insan açıklaması açık. Görünüm güzelleştirmesi geri açılmaz.

Başlıkta bunker çağrısı ile ana kısa modun farklı hedefleri, terminal modalın dışında kalan E ipucu ve sel yüzeyindeki baskın parlaklık alıcı riskleri olarak kayıtlıdır. Gerçek yanlış tıklama, insan anlamaması veya shader arızası ölçülmedi; bu yüzden teyit edilmiş fonksiyonel hata gibi sunulmaz.

## Uzun koşu ve insan kabulü

SystemAudit + native Unity JsonUtility fixture'ları ve1800sn/10döngü soak staging'de hazır. Henüz yürütülmedi. Telemetry-v1 kısa fizik koşusunda p95≈16.67ms olsa da max2625ms/2hitch vardı; bu6.69snfixture turu oyun performans kabulü değildir. Mono process workingSet0 döndürdüğü için bu değer UNKNOWN'dur; observer-v2 unavailable=-1/availableflag ve faza bağlı hitchledger ekler.

HUMAN44katalog vakası, ilk180sn/270+30sninsan süresi, keyif, tekrar isteği ve dinleme için gerçek katılımcı gerektirir. Katılımcı tercihi ve mevcut Unity sahibiyle doğrudan mesaj koordinasyonu kullanıcıya soruldu; yanıt gelmedi. Bu soruların varsayılan seçeneği onay sayılmaz. Koordinatör bu sırada kaynak/model/görüntü ve mevcut sahibin native kayıt incelemesini sürdürdü.


## Güncel Jury adaptörü — SYS021 / FLOW-HARNESS001

CLOSED_NATIVE_API: güncelV4.9 iki vida/doğrudan montaj/dedicated yangın istasyonu adaptörü canlıya alındı. DLL`A29F0DF6DE2D1E14FF2958382BDF314E6CFAC3F2B7130FD8C267FE1E55387895` üzerinde189/189,0runtime error,gerçekexit0. MODEL5510 yeniden koşuldu. [Güncel kanıt ve OS sınırları](../../Evidence/GamejamOrchestra/TLTP-E2E-V49-20261017/categories/02-flow/execution-jury-os-20261017/REPORT.md). Basılı yürüyüş/video/HUMAN tam kabulü açık; bu kapanış yalnız Jury harness regresyonudur.
