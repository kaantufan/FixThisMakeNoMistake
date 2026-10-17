# Too Late to Panic! — Home V4.7 teslimi

17 Ekim 2026. Windows/Unity6000.0.73f1 Built-In, izometrik. FPS askıda korunur. Mevcut HUD oturumu tamamlanmış V4.6 üzerinden alındı; kesilmedi.

## Oyundaki değişiklikler

- **HAZIRLIĞA BAŞLA:** hareket → gerçek su toplama/çantaya koyma → iki adımlı gerçek sabitleme → depremde korunma/sonuç → ayrı sel ve yangın eylemleri → inceleme/sonuç. Sonlu stok, gerçek tarifler ve 12/8 kapasite korunur. 270sn içerik/300sn mutlak sınır; menüde kısa akış saati devam eder. Eksikler tamamlanmış sayılmaz. Serbest oyun normal kampanyadır; kısa akış skor dışıdır. Zincirleme afet yok.
- Seçili küçük malzemede hafif görsel hareket ve turuncu ipucu, .30sn collider'sız toplama yayı, gerçek sabitleme durumuna bağlı donanım yerleşmesi, iki büyük eşyada kontrollü görsel deprem tepkisi. Fizik kökü/kapak/loot konumu taşınmaz.
- Yarkın'ın yarkin-assets/eb05475ee0533b86031b36f156f6022521118a15 kaynağı seçilerek alındı: 32FBX,32ikon,29materyal ve atlas. Gerçek LFS içerikleri doğrulandı. Sekiz malzeme ailesi, çanta istasyonu, indigo koltuk ve balkon sandalyesi oyuna uyarlandı. Eski daha detaylı mobilyalar daha basit branch modelleriyle otomatik değiştirilmedi.
- Ayrı sanat yönetimi geçişinde 12 prop geliştirildi: çanta istasyonu, kitaplık, TV konsolu, mutfak dolabı, alet kutusu, ecza dolabı, termosifon, metal raf, indigo koltuk, balkon sandalyesi, lavabo, alet duvarı. Çökük lavabo çanağı, farklı alet geometrileri, konstrüksiyon/bağlantılar, kapakla hareket eden detaylar ve kumaş birleşimleri eklendi. 12/12 dış ölçü/menteşe korundu. Prosedürel alt küme 47.928→57.552 üçgen; bu sayı toplam oyun veya FBX toplamı değildir. Diğer aileler kalite matrisiyle incelendi; bütün kütüphane yeniden modellenmiş değildir.
- Bunker takılması: HUD'nin sinematikte iki saati birden kilitleyen InputLocked yazımı kaldırıldı. Doğal faz geçişi açıldı. **SAHNEYİ ATLA · ENTER** gerçek hasar/çanta hesabını tek kez çalıştırıp gerçek inceleme/kayıp sonucuna geçer; kamerayı ve karakter kontrolünü geri verir.
- **F2 → Kaybetmeme** geliştirici seçeneği: gerçek afet/hasar/erzak tüketimi sürer, kayıp yerine inceleme açılır; skor/dalga/bütçe ödülü verilmez. Kullanım koşuyu kalıcı skor dışı işaretler. Kapatmak o koşuyu temizlemez. Kısa akış zaman sınırını uzatmaz.
- Oyuncuya görünen metin ve konuşmalarda jüri/demo sunum dili kullanılmaz. Gizlenen üst katla bağsız global çatlak şeritleri giderildi; zemin çatlakları, gerçek hasar ve enkaz/toz korundu.

## Ses ve lisans

Chrome eklentisiyle ElevenLabs mevcut Creator hesabından 7 kısa Türkçe kayıt +3 kısa efekt üretildi, indirildi, işlenip gerçek olaylara bağlandı. Mia / Eleven Multilingualv2; toplam konuşma15.73sn. Tek konuşma, altyazı, tekrar önleme, öncelik ve afet sesi kısma kullanılır; bekleme eklenmez. Eski Tolga kayıtları arşivde kalır.

Mevcut bakiye245691→245154: **537kredi** (516konuşma+21efekt). Yeni ödeme/abonelik/overage yok. 10/10 teknik dosya kontrolü, kırpılma0; native import ve AudioSource durumu doğrulandı. İnsan dinlemesi/telaffuz/tını tercihi kabulü yapılmadı. ElevenLabs sağlayıcı koşulları geçerlidir; çıktılara CC0/MIT atanmamıştır. Yarkın provenansı, Quaternius CC0 ve HUD OFL lisansları pakette korunur.

## Güncel kanıt

| Kanıt | Sonuç |
|---|---|
| Unity derleme | 0hata / 0uyarı |
| Etkilenen native/API kontrolü | **189/189PASS**,0runtime hata,54.71sn |
| Render | 55 gerçek PNG;1600×900/1280×720 arayüz ve12 ayrı baked prop +atlas |
| Kaynak/prefab/input farkı | 0 / 0 / 0 |
| İzole skor | önce/sonra aynı |
| DLL SHA256 | `94B946D8E2928DC9770E09C022448B926E68BA46F4F22A3280DF9E24E72A2D79` |
| EXE SHA256 | `0E1B64A3C0EBA3482F4C7B57F36CF8811997C0CE3BBED914C116CB9D4799FC7C` |

Manifest: `Evidence/GamejamOrchestra/TLTP-JURY-PRODUCTION-20261017/manager/source-build-v8/source-manifest.json`.
Native: `Evidence/GamejamOrchestra/TLTP-JURY-PRODUCTION-20261017/manager/native-v8-13edef84-e631-4f3b-9941-75cdcd50151c/jury-smoke.json`.
Sanat: `Evidence/GamejamOrchestra/TLTP-JURY-PRODUCTION-20261017/art-direction/`.

V1 siyah ekranları görsel kabul sayılmadı. V2 58/58 ve görüntüler ara kanıttı. V3'te doğal bunker akışı14.69sn'de geçti; sonraki test aracının yanlış controller araması NullReference üretti. Test aracı dar getter ve iç coroutine hata yakalama ile onarıldı; eski hatalı koşu güncel kabul değildir. Yukarıdaki son DLL ve yeni koşu esas alınır.

## Açık kabul sınırı

Gerçek maliyet/iş Tick'i, E/F, kamera, durum/reset ve bunker doğal zamanlaması denendi. Hedeflere test kurulum ışınlamaları ve bazı simülasyon zamanı girdileri açıktır. Sürekli ev rotası/OS klavye-fare, ilk kez oynayan kişinin180sn'de keyif alması, soğuk açılış dahil300sn, uzun kampanya, insan işitmesi ve tüm assetlerin nihai sanat kabulü doğrulanmış değildir. İlk üç dakika hedefi tasarım bütçesidir; gerçek oyuncularla ayrıca değerlendirilir. 12prop stüdyo kareleri oyun içi ışık/erişim kabulünün yerine geçmez.

## Son ek kapsam: dış dünya, kamera ve afet seçimi

Üst katta alt katların meshleri gizlenir; fiziksel zemin/duvar/balkon/merdiven çarpışmaları korunur. Gizli alt kat eşyaları fareyle seçilemez. Helikopter ev içi ve sığınak içi kamera planlarında gizlenir, dış afet sonrası planlarda görünür. Sağ tuş kısa bırakılırsa taşıma, basılı sürüklenirse yumuşak yaw/sınırlı pitch kamera dönüşüdür; UI/iş/çanta/sinematik bunu engeller. Ortografik dolaşım ve kameraya göre hareket korunur.

Garajdan ana yola S biçimli bağlantı bunker kapağını dolaşır; yol kapısı ve kaldırım kesintisi açılmıştır. Mevcut doğu çit işi bu yolun dışına alınmıştır. Komşu evlerin cephe/çatı/giriş ayrıntıları, dört mevcut CC0 rig kullanan mahalle sakini ve afete göre farklı çevre hasarı eklenmiştir. NPC'ler çevre görselidir; konuşma/quest/ekonomi veya gerçek kalabalık tahliye simülasyonu değildir. Afet/sinematik sırasında dışarıda yürümeyi sürdürmezler.

Batı çitinin dışındaki dere yatağı gerçek kıyı/menfez ve yönü görülen UV/foam akışı içerir; selde su görünümü yükselir. Bu mesh/efekt tabanlı akıştır, CFD/akışkan çözücü veya mühendislik tahmini değildir. Yeni Fırtına ayrı çatı/pencere/kapı/mobilya riskleri, önlem etkileri, rüzgâr/yağmur/savrulan parça görselleri ve yerel prosedürel rüzgâr sesi kullanır. Yangın varsayılanına düşmez. [MGM rüzgâr hazırlığı](https://mgm.gov.tr/arastirma/dogal-afetler.aspx?s=kuvvetliruzgar) tasarımın gevşek eşya/çatı/kapı yönünü destekler; oyun risk katsayılarını doğrulamaz. Mevcut Unity6000 parçacık sistemi kullanılır; renderer/paket göçü yoktur. [Unity API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ParticleSystem.html)

Ana menüde FELAKET SİMÜLASYONU OYNA → Deprem/Sel/Yangın/Fırtına. Seçili tek afet,90sn hazırlık ve açık1su/1gıda başlangıç çantası; sonlu iş malzemesi vardır. AFETİ BAŞLAT gerçek hasar/erzak yolunu çalıştırır, karakteri fiziksel bunker başarısı kazanmış gibi göstermez. Sonuç incelemede kararlı kalır; skor/dalga/bütçe ödülü veya kayıt yoktur. Tekrar seçili afeti korur; başlığa/serbest oyuna dönüş modu temizler. Ana kampanyanın1–9 sırası ve eski üç tip rastgele seçimi korunur. Fırtına kısa270sn akışa ek zorunlu bölüm değildir.

Sinematiklerde faz süreleri uzatılmadı: daha kısa ve yavaş kamera hareketleri, plan sınırlarında temiz kesme, afete özel sel/çatı/cephe kadrajları ve kontrollü deprem sarsıntısı kullanılır. Atla gerçek sonucu korur. Yarkın'ın8malzeme ailesindeki10varyant incelendi;9'una toplam700üçgenlik sınırlı, paylaşımlı materyalli ek detay yapıldı; Apple yeterli görülüp korundu. İthal FBX/atlas/UV değiştirilmedi.

HUD son küçük polish: dolar gösterimi, hizalama ve mevcut gerçek su/gıda3D modellerinden önizlemeler; ekonomi değerleri değişmez. Görselden bağımsız model risk hatası: sofa_indigo/sofa_olive, canonical sofa riskine bağlandı. Hiçbir mobilya yalnız yeni görsel ID nedeniyle hasar hesabından kaçamaz.

Yeni kapsamın native sonucu yukarıdaki son DLL/rapora bağlıdır. Model/risk ve gesture testleri, dört seçeneğin durumları, doğal fırtına, dış NPC/hasar, kat-fizik ayrımı, helikopter ve gerçek ortografik orbit kontrol edilir. İnsan rota/oynanış/işitme sınırları devam eder.

Son dar görsel onarımlar: V5'teki iki opak Storm karesi mevcut sabit alfa SoftWeather shader'ıyla giderildi. Radyo ızgarası/tüp işaretinin yanlış tarafa düşmesi, yalnız ek detay vertexX yansıtması ve üçgen yüz yönüyle düzeltildi; ham FBX değişmedi. Son yeni baked istasyon, radyo ve tüp native kareleri bu düzeltmeyi gösterir. Üç ayrı gerçek komşu yakın planı Storm/Flood/Fire izlerini gerçek cephe/çatı üzerinde gösterir.

Garaj yolunun kabulü: bir test başlangıç yerleştirmesinden sonra sokak→açık çit→bunkerı yanından dolaşma→açık garaj içi gerçek CharacterController ile yüründü; yol sırasında Warp0. Otomatik girdi ve test saati yalıtımı açıktır; tüm ev rotası veya araç sürüşü kabulü değildir. Dört NPC'nin gerçek animasyon controller/klipleri ve kaldırımda yer değişimi son native'de kontrol edildi.

Normal oyuncu açıldı: PID49252; HOME_V47_READY gözlendi, test argümanı/kayıt yönlendirmesi yok. Yeni normal pencereye ayrıca OS giriş/görsel kabulü uygulanmadı. ZIP/DLL eşleşmesi doğrulandı. EskiBuilds temizliği manager/cleanup-receipt.json ile sınırlıdır; kullanımda oyuncu korunur.
