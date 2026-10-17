# Too Late to Panic! — Home V4.8 akış kontrolü

17 Ekim 2026. Unity6000.0.73f1 / Built-In / çevrimdışı Windows. FPS korunur ve askıdadır. Beş mevcut üretim oturumunun son turları tamamlanmış/idle olarak ayrı ayrı doğrulandı; kayıt manager/sessions-check.json.

## Düzeltilenler

- Kısa depremde Enter ile atlama; sıradaki göreve göre başlık ve üst/alt kat merdiven/ön kapı işareti.
- İki hareket isteyen kısa sabitlemede iki doğru hareket kutusu; modeldeki gerçek hareket sayısı kullanılır.
- Doğal afet ve atlama sonunda tüketilen su/gıda artık çanta slotlarında da düşer.
- Ağır hasar, çanta eksiği ve süre kaybı ayrı doğru metinlerle açıklanır. Simülasyon uyarısı Afeti Başlat eylemini gösterir.
- Ağır hasarla biten simülasyonda Hasarı İncele açılır; kayıp nedeni, gerçek hasar, sıfır ödül/skor ve %65 sınırı korunur.
- Yangın sinematiğinde görünür üst pencere dumanı; iki sınırlı sistem/192 ek parçacık kapasitesi, mevcut soft-alpha shader. Sağlayıcı harcaması yok.
- Yangında yakın pencere planı, incelemede aktif yangın/üst kat yanık meshinin temizlenmesi ve en etkili üç hasar etiketi. Çanta yönlendirmesi kısa akış/simülasyona uygun; üst katta yakındaki mobilyayla yangın hazırlığı yapılır.

## Yeni kanıt

Kesintisiz kısa akış **49.67 sn**. Üretim başlangıç konumu dışında test ışınlanması0, zaman enjeksiyonu0, geliştirici değişikliği0. Gerçek kapı/merdiven/CharacterController, E/F, çanta ve sonlu maliyetli işler kullanıldı. Bütün kısa hedefler tamamlandı. Hazırlanmış deterministik rota; ilk kez oynayan insan hızı veya keyfi değildir.

| İlk akış olayı | Başlangıçtan süre |
|---|---|
| first-pickup | 3.50 sn |
| water-packed | 3.76 sn |
| first-real-prevention | 4.89 sn |
| first-visible-disaster | 10.56 sn |
| flood-prevention | 30.00 sn |
| physical-flood-refuge | 36.73 sn |
| fire-prevention | 39.85 sn |
| all-three-disasters-visible | 49.30 sn |

| Simülasyon | Gerçek terminal | Hasar | Doğal afet/sonuç süresi |
|---|---|---|---|
| Earthquake | Inspection | %52.33 | 12.00 sn |
| Flood | Inspection | %57.42 | 12.00 sn |
| Fire | Inspection | %42.16 | 12.00 sn |
| Storm | GameOver | %74.23 | 19.60 sn |

Kayıplar başarılı sayılmadı. Her birinde gerçek sonuç inceleme/menü/aynı afetle tekrar başlangıcı denendi. Yeni rota kontrolleri **149/149**, regresyon **189/189**; runtime hata0. Yeni rota PNG29. Derleme hata/uyarı0. Altı kaynak/asset/settings manifest kategorisinde fark0; izole skor baytları değişmedi.

DLL SHA256 `5683E713C90F2FD070DB819D14526EBA74ACC25B65C6581DABD6FB602F6BD17E`. EXE SHA256 `0E1B64A3C0EBA3482F4C7B57F36CF8811997C0CE3BBED914C116CB9D4799FC7C`.

Native rota: `Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/manager/native-v3-bf5a8289-bcfd-453a-92f9-331be7feb582`. Regresyon: `Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/manager/regression-v3-26951ed6-54be-4e05-bc5a-6ecc7c657ff6`. Manifest: `Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/manager/source-build-v3/source-manifest.json`.

Ara koşular nihai kabul değildir: V1'de test, kayıp ekranında controller açık olmasını yanlış bekledi; gerçek inceleme geçişinde controller/hareket zaten geri geliyordu. V2'de149/149 eylem kontrolü geçti ancak testin sıfır hareket beklemesi, faz observer'ından bir kare önce kapalı controller'a çağrı yaptı. Test yürüyüş kapısı gerçek controller.enabled koşulunu da bekleyecek şekilde düzeltildi. Yukarıdaki son DLL ve hata0 koşusu bu iki ara sonucu yerine geçer; ara JSON/loglar korunur.

## Sınırlar

Windows kilit ekranı doğrudan OS girdi/görsel kontrolünü engelledi; kilit aşılmadı. Ekranlar oyunun gerçek render çıktılarıdır, girdi üretim API'si üzerinden verildi. İnsan ilk180sn anlaşılabilirlik/keyif, dinleyerek ses kalitesi, uzun kampanya ve bütün assetlerin nihai sanat kabulü verilmedi. Önceki189 sayısı bu yeni rota yerine kullanılmadı. Açık kullanıcı V4.7 oyunu kapatılmadı; kullanımda olduğu sürece eski build klasörü korunur. Yeni uzak yayın yapılmadı.

Kalan küçük görsel kusur: kısa dış inceleme karesinde iki hasar etiketi sağ üstte çakışabiliyor (P2); ana hedef/buton/rota engeli değil. 1600×900 statik QC kapsamı, bütün kamera konumlarında çakışmasız yerleşim kabulü değildir. Son V4.8 normal profille açıldı, HOME_V48_READY ve yanıt verir durum gözlendi; yeni normal pencere OS ile incelenmedi. Güncel ZIP doğrulandıktan sonra eski V4.7 ZIP Geri Dönüşüm Kutusuna alındı; kullanımda olan V4.7 klasörü ve oyunu korundu.

Son bağımsız görsel QC: visual-qc/VISUAL-QC-V3.json / .md; aynıDLL ve11gerçekPNG SHA ile bağlı, kritikblokaj0, yukarıdakiP2 açık. V2görsel kabulü yeniDLL yerine kullanılmadı.
