# Home V5.5-r2 — root ses testi ve teknik eksikler

18 Ekim 2026. **Testi root gerçekleştirdi: dört afet simülasyonu, doğal FPS aşaması, müzik açık/kapalı, pause ve defter dönüşü Windows çıkışından kaydedildi.** Kod/asset/build onarımı bu turun kapsamında yapılmadı. Aşağıdaki bulgular teknik ölçüm ve kaynak incelemesidir; algısal dinleme/insan ses kalitesi kabulü verilmedi.

## Test kimliği ve yöntem

- Çalışan sürüm Home V5.5-r2 /0.5.5, PID35640. DLL SHA256 `82B570D390EAE53F740944C1A6D7560E2E61E1B2D2FE0F5344B66A489676A789`. Dondurulmuş 162 C#/shader dosyasında fark 0.
- Mevcut normal üretim UI süreci, ayrı `--home-evidence` skor hedefiyle kullanıldı. Bu tur Unity, yeni build, smoke driver, ışınlama, model API girdisi veya saat fikstürü kullanılmadı. Dört afet ürünün kendi simülasyon seçicisi ve AFETİ BAŞLAT düğmesiyle çalıştırıldı. Simülasyonlar normal kampanyada gerçek bodrum girişinin yerine geçmez.
- Windows arayüzü computer-use klavye/fare ile işletildi. Gerçek default render endpoint'inin WASAPI loopback kaydı alındı; fiziksel mikrofon açılmadı. [SoundCard API](https://soundcard.readthedocs.io/en/latest/) ve [Pycaw Core Audio API](https://github.com/AndreMiras/pycaw) temelinde yalnız Evidence altında kayıt/okuma yardımcıları kuruldu; OS ses ayarı değiştirilmedi.
- HyperX Cloud III S Wireless endpoint sessizde değildi; scalar `.660000026`. Oyun oturumu sessizde değildi ve scalar `1.0` idi. Bunlar okundu; ayar yazılmadı.
- 6 WAV kaydı toplam **310 saniye**: başlık6, deprem34, sel30, yangın30, fırtına30, FPS/geçiş180. Hepsi 48kHz, stereo, PCM16 olarak saklandı; analiz ham float capture değerlerini de kaydetti. Kayıtlar endpoint'in karışık çıkışıdır, proses başına izole değildir. Diğer session peak'inin de yükseldiği görüldü; bunun kayıt stream'inin yansıması olduğu doğrulanmadığından karışım sınırı korunur.
- FPS için aynı anda 1488 salt okunur session-meter örneği alındı. UI zamanları girişten sonraki ekran gözlemine aittir; engine kare hassasiyetinde değildir. Geçiş/tık seslerinden uzak kararlı aralıklar ayrıca ölçüldü.

## Yeni ölçüm sonuçları

| OS kaydı | Süre | Tüm kayıt RMS / peak, dBFS | Sonuç |
| --- | ---: | ---: | --- |
| Deprem simülasyonu | 34sn | −36,79 / −17,01 | Çıkış sıfır değil |
| Sel simülasyonu | 30sn | −45,46 / −20,59 | Çıkış sıfır değil |
| Yangın simülasyonu | 30sn | −41,66 / −19,99 | Çıkış sıfır değil |
| Fırtına simülasyonu | 30sn | −46,53 / −20,92 | Çıkış sıfır değil |
| FPS ve modal geçişler | 180sn | −42,51 / −14,93 | Açık/kapalı/durma/dönüş pencereleri aşağıda |
| İlk başlık referansı | 6sn | dijital sıfır | Başlangıç referansı; tek başına hata sayılmadı |

Bu değerler yalnız afet kanalının seviyeleri değildir: kayıtlarda önlem, müzik, UI, afet ve çevre birlikte bulunur. Tablodaki RMS farklarını doğrudan afet miks dengesi hatası olarak yorumlamak doğru olmaz. Altı kayıtta `abs >= .999` örnek sayısı **0**, capture uyarısı **0**; bu, sayısal kırpılma taramasıdır, işitsel kalite kararı değildir.

FPS kararlı pencereleri:

| Durum | Ölçülen pencere | RMS | Sonuç |
| --- | ---: | ---: | --- |
| Müzik açık | 6,817sn | −37,96dBFS | Sıfır olmayan çıkış; oyun-session peak pozitif |
| Pause | 5sn | 0 PCM örneği | Dünya/panik/müzik askısı kararlı pencerede sessiz |
| Müzik kapalı, FPS aktif | 7,617sn | −41,43dBFS | Müzik dışı sesler sürüyor; bu master mute değildir |
| Defter açık | 6,25sn | 0 PCM örneği | Oyun-session peak de 0 |
| Defter dönüşü, müzik kapalı | 8,117sn | −40,76dBFS | Ses geri geliyor |
| İkinci pause | 5sn | 0 PCM örneği | Kararlı pencere sessiz |
| Müzik yeniden açık | 1,5sn | −36,15dBFS | Kısa son-süre örneğinde çıkış geri geldi |

Pause boyunca bütün aralığın sessiz olması beklenmedi: menüde müzik düğmesine tıklama kısa UI sesi üretebilir. Önce geniş pause penceresine bu tık karıştı; ölçümde girişten2sn sonraki5sn kararlı pencere açıkça seçildi. Bu bir ses sızıntısı hatası diye kaydedilmedi.

FPS müziğini genel nabız/nefes sinyalinden ayırmak için ayrıca mevcut deterministik 16sn kompozisyonun bağımsız NumPy kaynak portuyla dalga eşleştirmesi yapıldı. İlk açık pencerede6sn normalize korelasyon yaklaşık **|r|=0,799**, kapalı pencerede **0,053**, kapalı defter dönüşünde **0,045**. Açık durumun2sn parçaları0,767–0,851 aralığında; en iyi şablon fazları16sn modülünde tam2sn ilerliyor. Bu, özgün müzik bileşeninin kayda ulaştığını destekliyor. Sabit pedal nedeniyle ters zamanlı kontrol de0,718 eşleşti; tek korelasyon değeri benzersiz bir ses parmak izi sayılmaz. Portun gerçek runtime klibiyle byte paritesi doğrulanmadı, resmi kabul eşiği tanımlanmadı ve bu bir insan dinleme sonucu değildir. Ayrıntılı filtre/resample/arama parametreleri ham offline analizde kayıtlıdır.

## Teknik eksikler — önerilen düzeltme sırası

### 1. A55-01 · P2 · Yangın/fırtına dış sinematiğinde bodrum filtresi sürüyor

**Kaynakla doğrulandı; bu özel gerçek-bodrum koşulu OS'ta yeniden üretilmedi.** Oyuncu bodrum sığınağındayken kamera4,7sn sonrası dışarı çıkar. Ses filtresi hâlâ oyuncunun bodrum konumundan hesaplanır. Depreme özel dış çekim istisnası vardır; yangın/fırtınada yoktur.

`PanicHomeEffects.cs:194–212`: tam sığınak karışımında genel `.22` taban gain `.22*.18=.0396` olur; ek kısılma yaklaşık **14,9dB**, kapak kapalıyken low-pass **650Hz**. Şiddet, duck ve çoklu afet gain'i bunun üzerine gelir. Ekran dış afeti gösterirken ses durumu içeride kalır. Sel üst oda kullandığı için bu bodrum tetikleyicisi bütün sellere genellenmez.

**Dar düzeltme:** Dış afet kamera durumunu bütün aktif afet kanallarına taşı; iç sığınak çekiminde filtreyi koru. Gerçek bodrum Yangın/Fırtına ve çoklu afet için kaynak+Windows çıkış karşılaştırması yap.

### 2. A55-02 · P2 · Dört montaj darbesi gerçek vuruşlarla seslenmiyor

**Kaynak çağrı akışında doğrulandı; dört darbe OS kaydı alınmadı.** `PanicExpedition.cs:781,814–820`, `PanicHomeWorld.cs:746–775`, `PanicHomeAudio.cs:42–49`. İş başlayınca `installing=true` olduğu için ilk boş karede montaj sesi gelir. Sonraki gerçek vuruşlar `CompletedStrokes` artırır; ses router'ı bu sayacı okumaz. İlk üç Space darbesinin ayrı montaj sesi yoktur; son darbe genel tamamlanma tick'i üretir. Mouse düğmesinin UI tıkı darbe Foley'si değildir.

**Dar düzeltme:** Yalnız kabul edilmiş gerçek darbe artışını seslendir; açılışta hayali vuruş üretme. Mouse ve Space aynı ses yoluna bağlansın. Gerçek vuruş, iptal, maliyet ve tamamlanma kuralları korunmalı.

### 3. A55-03 · P2 ses geri bildirimi boşluğu · RNG tesisat olaylarının kendine özgü sesi yok

**Kaynakla doğrulandı.** `PanicRevisionWorldV5.cs:87–124`, `PanicSecondaryAlertV55.cs:24–48`, `PanicHomeAudio.cs:59–68`. Gaz alevlenmesi, boru sızıntısı ve kısa devre gerçek hasar/görsel/HUD üretir. Olay türüne bağlı ses çağrısı yoktur. Genel ahşap/metal/cam hasar sesi bazen çalabilir; bu, olayın ayrı sesi değildir.

**Dar düzeltme:** Zaten commit edilmiş olayı dalga/olay anahtarıyla bir kez seslendir. Yeni RNG çekme, hasarı tekrar çözme. Bu, V5.5'in görsel uyarı sözleşmesinin ihlali olarak değil, mevcut ses kapsamı boşluğu olarak kaydedildi. Önceki OS testinde uyarının hasar etiketiyle örtüşmesi ayrıca doğrulandığından, sesli olay bildirimi ve okunur neden birlikte ele alınmalı.

### 4. A55-04 · P3 · Vana ve şalter ahşap montaj sesi kullanıyor

**Kaynakla doğrulandı.** `PanicAudioBank.cs:44–54` içinde `gasoff/wateroff/poweroff` eşleşmesi yok; default `wood_build` dönüyor. Gerçek vana dönüşü, su kesilmesi ve elektrik şalter klik katmanı yok. Tamamen sessiz oldukları iddia edilmiyor.

**Dar düzeltme:** Üç eyleme anlamlı kısa mekanik sesler eşle; başlangıç, gerçek hareket ve son kapanış/iptal durumlarını ayır.

### 5. A55-05 · P3 · Zodiac bot ve NPC Foley katmanı eksik

**Kaynak/asset envanteriyle doğrulandı.** `PanicStreetLifeV414.cs:113–121,166–172`: botun görsel hareket/ekibi var, motor/su yarma sesi bağlanmıyor. Yürüyen NPC/ekip için ayak/giysi ses olayları yok. Kara araçları ve dört siren zaten mevcut; bunları eksik diye yeniden üretmek gerekmez. NPC konuşması bu öneriye dahil değildir.

### 6. A55-06 · P3 · Genel ses ve efekt seviyesi ayarı yok

**Kaynak ve pause UI'sında doğrulandı.** `PanicHomeHud.cs:1005–1024`, `PanicRevisionHud.cs:60–65`. Oyuncu yalnız müziği açıp kapatabiliyor; master/SFX seviyesi veya genel mute yok. Bu nedenle panik nabzı, nefes ve afetleri oyun içinden dengeleyemiyor. Müzik kapalıyken SFX'in sürmesi mevcut anahtarın doğru davranışıdır.

**Dar düzeltme:** Tek yetkili ve kalıcı master/müzik/SFX tercihleri; tüm ses sahiplerinin aynı ayarları kullanması. OS ayarını değiştirmeden oyunun kendi ses kontrolünü sun.

## Eksik sayılmayanlar ve diğer açık teknik sorunlar

89 ses dosyasının başlığı ve28 seçili PCM incelendi; dört temel afet dosyası var, seçili28 klip sessiz değil ve tam ölçek kırpılma örneği0. Nabız, yerel nefes, oyuncu adımları, kapılar, pickup, helikopter rotoru, dört kara aracı sireni ve genel UI/sonuç sesleri için asset+runtime yolu mevcut. Bu, her birinin bütün pozlarda/mikslerde OS'ta ayrı test edildiği anlamına gelmez.

Eski radyo/anlatıcı ve13 Deferred konuşma kaydı bilinçli ertelenmiş durumda; bunlar kayıp import hatası diye sayılmadı. Tüp için aktif püskürtme mekaniği bulunmadığından püskürtme sesi eksik bug olarak yazılmadı.

[Önceki oynanış incelemesinin](HOME-V5.5-PLAYREVIEW-2026-10-18.md) dört P2 bulgusu açık: tesisat bandı/hasar etiketi çakışması, bütçesiz yardımcı hedefi, Enter sonrasında aftermath/kurtarma kurulumunun atlanması, eksik çantayla sığınak girişinin sonucu açıklamaması. Küçük kroki yazı çakışmaları ve eski HomeV4 pencere etiketi de açık. Bu tur bunlara onarım yapılmadı.

## Teslim ve kalan sınır

Müzik tercihi başlangıçtaki **AÇIK** durumuna geri alındı. Aynı V5.5 ana menüde açık bırakıldı. Gerçek skor dosyası yerine mevcut izole skor hedefi kullanıldı. Kaynak/build, bağımsız FPS, lisanslar ve temizlik hedefleri değişmedi; ücretli ses/sağlayıcı/kredi işi yapılmadı.

Bu tur yeni Windows output kanıtı sağlar. İnsan duyumu, kulaklığın fiziksel akustik çıkışı, sesin doğallığı, loop rahatsızlığı ve tüm gerçek sığınak/kat/çoklu afet kombinasyonları için kabul vermez. Özellikle A55-01 gerçek bodrum dış çekimi ve A55-02 dört vuruş özel durumu kaynak kanıtıdır; yeniden üretilmiş OS bug diye sunulmaz.

Ham kayıtlar ve tekrar üretilebilir araçlar: `Evidence/GamejamOrchestra/TLTP-V55-AUDIOREVIEW-20261018/manager/`. `audio-measurements.json`, `os-audio-observations.json`, `session-meter.json`, `captures/*.wav`, capture metadata/sha256; `capture_output.py`, `meter_output.py`, `analyze_segments.py`. Staging/code ve staging/assets kaynak incelemelerini, staging/testdesign ise eski native ayrımı ile yeni offline dalga analizini içerir. Tam yeni kimlik/son süreç durumu `manager/final-audio-review.json` ve güncel LATEST ile bağlıdır.
