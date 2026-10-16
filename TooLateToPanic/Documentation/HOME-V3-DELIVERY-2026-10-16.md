# Too Late to Panic! — Home V3 teslim kaydı

16 Ekim 2026. İzometrik Windows sürümü; Unity 6000.0.73f1, Built-In, çevrimdışı. Home V3 derlendi, normal açılışı gözlendi ve son DLL ile 18/18 kısa native kontrol geçti: 35,17 saniye, ortalama 55,57 FPS, sıfır çalışma hatası. Bu otomatik API kontrolüdür; elle klavye/fare, ses anlaşılabilirliği ve uzun kampanya kabulü değildir. Önceki sürüm kanıtı bu sonuç yerine kullanılmamıştır.

## Kaynakta bulunan değişiklikler

İki yaşanabilir kat ve bahçe korunur; fiziksel merdivenli bodrum, üst kat balkonu, lavabo/klozet/küvetli banyo ve ayrı girişli bitişik garaj eklenir. Garajdaki araç dekor/engel modelidir; sürüş mekaniği değildir. Kesit görünümü çatı/ön duvarları gizlerken bağımsız oda collider'ları korunur. Ev; kumaş, kitap, bitki, lamba, bilgisayar, şömine, çamaşır/teknik depo ve garaj eşyalarıyla genişletilir. Yeni malzemeler daha renkli fakat tutarlı indigo, zeytin, keten, ahşap ve terracotta tonları kullanır.

E kapak/kapı açıp kapatır; malzeme üretmez veya toplamaz. Açık kaynağın gerçek 3D içerikleri görünür; Z/X ile seçilen yığın F ile atomik olarak envantere alınır ve ele doğru görsel hareket oynar. 12 envanter/8 çanta, 2 bandaj + 1 antiseptikle ilk yardım, pilli destek ekipmanları, bütçe/malzeme isteyen dört adımlı işler, ankraj, taşıma, yapı/bahçe önlemleri korunur. HUD kısa etiketlere, ikonlu yuvalara ve bağlam tuşlarına indirgenir. Zıplama gerçek CharacterController/gravity yoluna bağlıdır.

Radyo açma/kapama ve altı Türkçe anons kaynağı; şömine, afete göre hasar atmosferi, hareketli rotorlu kurtarma helikopteri ve rotor sesi kaynakta bulunur. Afet, hasar gezisi, 3D sığınak başarısı ve kayıp nedenlerine göre gerçek kamera/sahne dizileri eklenir. Süre kaybında önce yaklaşık 1,35 saniye açık kayıp geri bildirimi, ardından yaklaşık 6 saniye başarısızlık sinematiği ve kararlı sonuç ekranı tasarlanmıştır. Afet/bütçe/dalga/çanta/bunker/kayıp/yerel highscore kuralları değiştirilmez.

## Kontroller

| Tuş | Home V3 işlemi |
| --- | --- |
| WASD / oklar, Shift | Yürü, koş |
| Space | Zıpla; etkin işte uygula |
| E | Aç/kapat; radyo, bunker, bahçe işi veya ana etkileşim |
| Z / X, F | Açık kaynaktaki malzemeyi seç, seçili yığını al |
| Tab / B | Envanter ve çantayı aç/kapat |
| Envanterde oklar, Enter, F | Seç, çantaya koy, ilk yardım üret |
| Q, T | Yakındaki hedefi değiştir, duvar/zemin ankrajını seç |
| 1 / 2 / 3 / 4 | Ankraj, koruma, güçlendirme, taşıma |
| Taşırken R / Enter / Backspace | 45° döndür, geçerli konumu bırak, iptal |
| İşte A-D / Enter / Space | Hizala, kilitle, uygula |
| Backspace / Escape | Geri/iptal; normal oyunda duraklat |
| PageUp / PageDown | Kamera yakınlığı |

## Asset kaynağı ve sahiplik

Home V3 oda/prop/pickup/ikon geometrisi ile atmosfer/VFX/sentetik efekt kaynakları bu proje için özgün prosedürel kodla yazılmıştır; yeni insan modeli üretilmemiştir. Mevcut oyuncu/animasyon tabanı ve ilgili lisanslar korunur: [asset kayıtları](ASSET-CREDITS.md), [oyuncu provenansı](Existing-Player-Provenance.md). Bu dosyalar kaynak/provenans kaydıdır; native görsel veya işitsel kabul değildir.

Türkçe WAV'lar bilgisayarda önceden kurulu **Microsoft Tolga – Turkish (Turkey)** OneCore sesi (`MSTTS_V110_trTR_Tolga`) seçilerek yerel SAPI ile üretilmiştir; hafif radyo filtresi/normalizasyon sonrasında mono PCM16/22050 Hz kaydedilmiştir. Üretim ağ servisi, API anahtarı veya ücretli kredi kullanmamıştır. Microsoft ses motoru/verisi dağıtılmaz. Sentezlenmiş çıktının ticari dağıtım şartları bağımsız olarak doğrulanmamıştır; Microsoft sese CC0 veya açık lisans atfedilmez. Ayrıntı ve transkriptler [atmosfer handoff](../../Evidence/GamejamOrchestra/TLTP-HOME-V3-20261016/assets/atmosphere/r1/HANDOFF.md) içinde tutulur.

Dosya sahipliği: root canlı `PanicHomeWorld`, `PanicHomeLife`, effects, builder, import ve player sahibidir. Sanat staging'i `PanicHomeArt`, `PanicResidenceArt`, `PanicAtmosphereArt`; UX staging'i `PanicExpedition`/`PanicHomeHud`; sinematik staging'i `PanicHomeCinema`; QA staging'i `PanicResidenceSmoke` teslim eder. Kaynak manifesti root'un entegre ettiği gerçek dosyaları bağlamalıdır. FPS projesi bağımsızdır; bu teslim onun kaynak/build/kayıtlarını doğrulamaz.

## Güncel çıktı ve gerçek kanıt

Aşağıdaki çıktı yolları `TooLateToPanic/` Unity proje köküne göredir. Tam EXE/DLL/ZIP hashleri ve güncel durum `LATEST-BUILD.json` içindedir.

| Kayıt | Değer |
| --- | --- |
| EXE | `Builds/Home-V3-20261016/TooLateToPanic.exe` |
| Tek taşınabilir paket | `Builds/TooLateToPanic-Home-V3-20261016-portable.zip` |
| Launcher / indeks | `Play-TooLateToPanic.cmd` / `LATEST-BUILD.json` |
| Sahne / giriş | `Assets/Game/Scenes/TooLateToPanicResidence.unity` / `PanicHomeWorld` |
| Kaynak snapshot/manifest | `TLTP-HOME-V3-20261016/before`, `source-build-v3/source-manifest.json` ve `prefab-cache-manifest.json` |
| Build receipt / zaman | `Builds/Home-V3-20261016/build-receipt.json`, 16 Ekim 2026 18:24:18 UTC |
| Son Assembly-CSharp SHA256 | `839053E81AFD8BB718343D82298D7D7FCDB74A8E22375111F1F28B9FF762C3B1` |
| Import/build | `source-build-v3/unity-build.log`, başarılı, 0 hata / 0 uyarı; aynı V3 bake cache'i kullanıldı |
| İzole native kanıt / profil | `native-v3/`, yalnız bu klasörün `isolated-home-scores.json` dosyası |
| Native sonuç | `native-v3/native-residence-smoke.json`, PASS 18/18, 0 hata, 55,57 FPS, 35,17 saniye |
| Normal açılış | `native-v3/normal-open.json` ve `normal-player.log`; özel test argümanı yok, `HOME_V3_READY` gözlendi |
| Görüntü / ses | Son 3D loot ve kurtarma karesi incelendi; sığınak, kayıp bildirimi ve kamera kareleri de kaydedildi. Altı ses yükleniyor/playback aktif; elle dinleme yapılmadı |
| Sınırlar | Tek dalga, bir aktif ankraj, fiziksel iki merdiven ve zıplama; bütün oda kapı rotaları, her eşya işlemi, üç afetin tüm kombinasyonları ve uzun kampanya doğrulanmadı |

Son kısa etkilenen native koşu `--residence-smoke --home-evidence <yeni-izole-klasör>` kullanır. Skor yolu bu klasöre ayrılır. Harness; gerçek fiziksel merdiven/zıplama, E/F ayrımı, seçili yığın/GatherAt uyumu, üretim/çanta, bir ankraj işi, bir gerçek 8/4/3 saniyelik afet-sonrası-başarı döngüsü, kurtarma/ses playback durumu ve kayıp-önce-sinematik sırasını kontrol eder; JSON, hata/FPS ve ekran kaydeder. Oda ekranları setup warp'ları kullanır. Bu API-driven koşu manuel klavye/fare turu, ses anlaşılabilirliği, dokuz dalga kampanyası, uzun denge, cold-load/kayıt veya son sanat kabulü değildir. Gerçek kaynak/derleme/native/elle oynanış/görsel kabul ayrı yazılır.

## Kalıcı teslim ve temizlik düzeni

Yeni çıktı gerçekten açılıp kısa ilgili kontrol tamamlanmadan çalışan önceki çıktı kaldırılmaz. Ardından yalnız bu projenin `Builds/` sınırındaki eski build/ZIP adayları temizlenir; tek doğrulanmış build, tek portable ZIP, tek launcher ve `LATEST-BUILD.json` tutulur. Açık kullanıcı player'ı, kaynaklar, lisanslar, kardeş proje ve gerçek kayıtlar korunur. Recursive işlemden önce çözülmüş mutlak hedefler doğrulanır.

Güncel kararlar, tuşlar, dosya sahipliği, asset/lisans, kaynak/build kimliği ve gerçek test sınırları README/Documentation/durum indeksine kaydedilir. AGENTS ve `Tools/GamejamOrchestra/` görev metinleri bu düzeni kalıcı olarak taşır; uygulamanın global system mesajını değiştirmez. Küçük tarihsel TLTP metin/hash/receipt kanıtları korunur; taşınan klasörler için eski-yeni yol eşlemesi ayrı receipt'e yazılır, tarihsel manifest içindeki özgün yollar/hashler topluca değiştirilmez.

## Paket revizyonları ve temizlik sonucu

İlk kaynak kontrolü Roslyn ile geçti. Taşıma sonrası ilk Unity denemesi eski mutlak Bee yolu yüzünden durdu; yalnız Library/Bee geri alınabilir yedek adına taşındı. V1 build sonrası native 17/18 geçti: kullanılmamış ses verisinin Unloaded olması düzeltildi. V1 karelerinde sığınak kamera geçişi, çatı örtüşmesi ve dar/düşük kontrastlı tuşlar onarıldı. V2 18/18 geçti. Kare incelemesinde garaj yan duvarı ve helikopterin sinema bandında kalması düzeltildi. V3 son DLL ile tekrar 18/18 geçti; bütün tarihsel sonuçlar kendi evidence dizininde korunur, son sonuç bunlarla karıştırılmaz.

Normal V3 oyuncusu PID 47912 ile açıldı. Eski MVP klasörü ve iki eski ZIP çalışma alanından Geri Dönüşüm Kutusu'na alındı (183.162.459 bayt). Kalıcı silme otomatik politika denetiminde engellendi; geri alınabilir temizlik başarıyla uygulandı. Bu değer diskte boşalan alan iddiası değildir. Kullanıcının açık Home V2 oyuncusu PID 46944 ve kullandığı Home-V2-20261016 klasörü korunmuştur; bu, yalnız son build kuralının kullanımda dosya istisnasıdır. Oyun kapandıktan sonraki yetkili temizlikte kaldırılır. Eski başlatıcı metni küçük before snapshot'ına taşındı; güncel başlatıcı yalnız Home V3'ü açar. FPS projesine dokunulmadı; kendi root'u aynı teslim kuralıyla çalışır.
