> Source documentation snapshot. The promoted playable package at this snapshot is [Home V4.13 /0.4.13](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.13). This push changes documentation only; draft labels and team-confirmation fields remain authoritative. Local Evidence links refer to the original development archive. No form submission, new build/native/OS test or playable release is performed. Historical version-specific observations remain as originally recorded.

# Too Late to Panic! — Dış araştırma, best practice karşılaştırması ve objektif eleştiri

**Tarih:** 17 Ekim 2026 · **Kapsam:** yalnız aktif izometrik oyun (Home V4.12–V4.14 kaynakları, LATEST = V4.13) · **Tür:** araştırma ve değerlendirme. Kod, build veya kayıt değiştirilmedi.

**Dayanaklar:** resmî CIVIL GAME JAM Kural Kitapçığı v04 (PDF, 10 sayfa, yerel kopya tam okundu); `Docs/` ve `TooLateToPanic/Documentation/` belgeleri; `PanicJuryDemo.cs`, `PanicModel.cs`; web'de oyuncu/eleştirmen yorumları, geliştirici konuşmaları, jam postmortem'leri, resmî afet kılavuzları ve akademik kaynaklar.

**Yöntem sınırı:** Reddit, kullandığım arama aracına kapalı çıktı (domain erişimi reddedildi). Bu yüzden forum kanıtı Steam toplulukları, itch.io yorumları/postmortem'leri, oyun eleştirileri, GDC/Game Developer ve ders analizlerinden geldi. Oyunu bu oturumda **ben oynamadım**; oynanış yorumları kaynak kodu, belgeler ve kayıtlı test raporlarına dayanıyor. Puan tahminleri benim yargımdır, jüri verisi değildir.

---

## 0. Kısa hüküm

Oyunun **çekirdek fikri güçlü ve temaya doğrudan oturuyor**: afet *öncesi* evi hazırla, sonucu aynı evin nesneleri üzerinde gör. Bu, türün en çok övülen iki örneğinin (60 Seconds!'ın “kısa telaşlı hazırlık → kalıcı sonuç” bağı ve Stop Disasters!'ın “sınırlı bütçeyle önlem seçimi”) iyi taraflarını birleştiriyor. Mobilya sabitlemenin merkezde olması da bilimsel olarak savunulabilir bir seçim (FEMA E-74 yapısal olmayan hasarı ayrı bir risk alanı olarak ele alır).

Ama 5 dakikalık jüri oturumunda puanı belirleyecek üç zayıflık var:

1. **Tema doğruluğunda iç çelişki.** Kısa akış selde “Bodruma inme, üst kata çık”, yangında “dışarıdaki toplanma alanına çık” diyor (doğru). Normal kampanya ise sel ve yangın dalgalarında da oyuncuyu **bodrum sığınağına** gönderiyor. Resmî sel kılavuzları bodrumda mahsur kalmamayı ve yüksek kata çıkmayı, yangın kılavuzları “çık ve geri girme” ilkesini söylüyor. İnşaat Mühendisleri Odası'nın düzenlediği bir jamde bu, “Temayla ilişki” (25 puan + ilk eşitlik bozucu) kriterinde en kolay görülen hata olur.
2. **Bilişsel yük ve kontrol sayısı jüri süresine göre fazla.** GDD'de 20'den fazla tuş/eylem var (WASD, Shift, Space, E, F, J, Q, T, 1–4, R/Shift+R, Enter, Backspace, Tab/B, PageUp/Down, Esc, F2, sağ sürükleme, sol tık vida…). Üstüne son 30 saniyede otomatik **izometrik → FPS** kamera değişimi geliyor. Forumlardaki ortak gözlem: jüri talimat okumuyor, kamera geçişinde yön kaybı yaşanıyor.
3. **İnsanla doğrulama yok.** 224/224, 204/204, 338/338 gibi bütün kabuller API fikstürü ve render kontrolü. LATEST-BUILD.json'da açıkça `humanAcceptance: false`, `OSInput: false`. Oyunu ilk kez gören biriyle süre tutularak yapılmış tek bir oynanış kaydı yok. 24 saatte 14 sürüm çıktı, ama hiçbirinin “eğlenceli mi, anlaşılıyor mu” sorusuna verisi yok.

Ayrıca **diskalifiye riski taşıyan teslim maddeleri** var (Bölüm 1.2). Bunlar puandan önce gelir.

---

## 1. Jam yönergesi: oyun neye göre puanlanacak?

### 1.1 Resmî puanlama (Kural Kitapçığı v04, md. 7)

| Ölçüt | Puan | Odak |
|---|---|---|
| Oynanabilirlik | 25 | Kontroller, anlaşılabilirlik, oyun deneyimi |
| Temayla ilişki | 25 | Temanın oynanış ve tasarımla ilişkisi |
| Yaratıcılık | 20 | Fikir ve uygulamada özgün yaklaşım |
| Görsel/işitsel bütünlük | 15 | Görsel ve ses öğelerinin oyunla uyumu |
| Teknik bütünlük | 15 | Çalışırlık, kararlılık, bütünleşme |

**Eşitlik bozma sırası:** Tema → Oynanabilirlik → Yaratıcılık. Puanın yarısı ve iki eşitlik bozucu, **anlaşılırlık** ile **tema doğruluğu** üzerinde. Teknik bütünlük (bugüne kadarki emeğin büyük kısmının gittiği alan) yalnız 15 puan.

Md. 7.3: Final sunumları jüri önünde yapılıyor. Yani oyunun **sunum anında da okunabilir** olması gerekiyor (Bölüm 7).

### 1.2 Uygunluk riskleri (puandan bağımsız diskalifiye nedenleri)

| Kural | Durum | Risk |
|---|---|---|
| 5.2 / 6.5: public repo + **tam commit kimliği**; 3.7: oynanabilir sürüm ile kaynak uyumlu | **Düzeltme (17 Ekim 19:50):** İlk sürümdeki “yayımlanmadı” yargısı eskimiş `LATEST-BUILD.json` alanına dayanıyordu. GitHub'da `v0.4.13` release'i var: tag commit'i `b551edfc…`, ZIP digest'i yerel ZIP ile aynı, kaynak–build farkı 0. Asıl risk başka: otomatik yayın `main`'e her 10 dakikada build'den ileride kaynak snapshot'ları itiyor. Forma `main` HEAD'i yazılırsa md. 3.7 ihlal edilir. | **Orta.** Forma yalnız teslim edilen release tag'inin commit'i yazılmalı. Ayrıntı: `TOO-LATE-TO-PANIC-JAM-TESLIM-BEYANI.md` §1. |
| 4.2 / 4.4: AI kullanımı araç + amaç + yer olarak beyan edilir; eksik/yanlış beyan diskalifiye | Kod ve belgelerin büyük kısmı AI ajanlarıyla üretildi; Meshy AI karakter kaynağı, ElevenLabs konuşma/SFX var. Proje içinde **tek, konsolide bir AI beyanı belgesi bulamadım**; parçalar ASSET-CREDITS ve kanıt klasörlerine dağılmış. | **Yüksek.** Formda eksiksiz yazılmazsa en iyi oyun bile elenir (md. 10.1). |
| 3.4 / 3.6: her hazır kaynak beyan edilir; yeniden dağıtımı kısıtlı ham asset public repoya konmaz | ASSET-CREDITS kendisi “Korunan Meshy kaynakları ve yeni bunker için kamuya yeniden dağıtım lisansı ayrıca doğrulanmış değildir” diyor. | **Orta–yüksek.** Lisansı doğrulanamayan dosya repoya girmemeli ya da kaldırılmalı. |
| 5.3 / A.1: bağlantı hesapsız açılmalı | ZIP 178 MB. GitHub Release asset uygundur. Çıkış yapılmış tarayıcıda denenmeli. | Düşük, ama test edilmeli. |
| 6.3: Git desteği için son başvuru **18 Ekim 12.00** | — | Takvim riski. Yayın son saate bırakılmamalı. |

**Öneri:** Bu beş madde için bir kişi sorumlu olsun ve teslimden en az 6 saat önce kapatılsın. Oyuna eklenecek her yeni özellikten önce gelir.

---

## 2. Benzer oyunlardan ve forumlardan çıkan dersler

Her satırda: dış kanıt → bizde durum → karar.

### 2.1 60 Seconds! (en yakın tür akrabası)

- **Övülen:** Kısa, telaşlı toplama turu ve bu turdaki kararların sonraki aşamada uzun süreli sonuç doğurması. Geliştiriciler oyuncuyu bilerek “düşünmeye zaman bırakmayan” bir avla başlatmak istemiş ([Pocket Gamer röportajı](https://www.pocketgamer.biz/interview/64163/the-making-of-60-seconds/), [GameLuster](https://gameluster.com/60-seconds-review/)).
- **Eleştirilen:** “Tank gibi” dönen karakter ve kötü kontroller, toplama sahnesindeki zayıf 3D modeller, şansa aşırı bağımlılık ([Dread Central](https://www.dreadcentral.com/reviews/105448/60-seconds-video-game/), [Vaporlens özet](https://vaporlens.app/app/368360/60_seconds)). OpenCritic eleştirmen ortalaması 58 ([OpenCritic](https://opencritic.com/game/3298/view)). Kanca güçlü, uygulama zayıf.
- **Bizde:** Son 30 saniyede FPS çanta toplama, 60 Seconds'ın en iyi anını yeniden üretme girişimi. Ancak 60 Seconds bu telaşı oyunun **başına** koyar; biz bütün önlem öğrenme yükünden sonra, **perspektifi de değiştirerek** koyuyoruz. Kısa akışta başlangıç gıdası zaten hazır (playtest kaydı bunu açıkça yazıyor). Telaş anında toplanacak tek temel şey su kalıyor ve gerilim zayıflıyor.
- **Karar:** Son anda telaş fikri **korunmalı**. Ama ya (a) kamera değişimi kaldırılıp telaş izometrikte yaşanmalı, ya da (b) geçiş 2–3 saniyelik açık bir “ÇANTANI HAZIRLA!” uyarısıyla haber verilip FPS'te yön kaybını önlemek için çantanın konumu ekranda işaretlenmeli. Karar insan testiyle verilmeli (Bölüm 6).

### 2.2 Stop Disasters! (UNDRR), resmî afet eğitimi oyunu

- **Övülen:** Sınırlı para/zamanla öncelik seçimi; kararların görünür sonucu ([UNDRR](https://www.undrr.org/news/stop-disasters-game-turns-disaster-prevention-real-life-challenge), [Common Sense Media](https://www.commonsensemedia.org/game-reviews/stop-disasters)).
- **Eleştirilen** (ders analizi ve oyuncu yazısı):
  - Önlemlerin etkisi oyun sırasında görünmüyor; ancak simülasyon bitince anlaşılıyor ([Mechanics of Magic, 2025](https://mechanicsofmagic.com/2025/09/29/today-i-stopped-disasters-poorly/)).
  - Sonuç ekranı yüksek kayıpta bile “tebrik ediyor”; oyuncu iyi mi kötü mü yaptığını anlamıyor.
  - Eğitim yok, ikon/yüzdelerin anlamı belirsiz.
  - Önemli bilgi gizli, gereksiz bilgi kalabalık; aciliyet hissi yok; senaryolar birbirine benziyor ([WPI IMGD analizi](https://web.cs.wpi.edu/~rich/courses/imgd4600-c15/analyses/stop-disasters/index.html)).
- **Bizde:** Sonuç ekranında katkı/eksik ayrımı ve “başarı uydurulmaz” ilkesi var. Bu iyi, Stop Disasters'ın en büyük hatasından kaçınıyor. Ama önlemin etkisi **afet anında nesne üzerinde** ne kadar okunuyor? Rapor 5.2'de planlanan “iki eşyalı kontrollü afet tepkisi” (sabitli kitaplık ayakta, sabitsiz olan devriliyor) tam olarak bu boşluğu kapatan özellik. **Oyunun en önemli 5 saniyesi bu.**
- **Karar:** Bu tepki yoksa veya zayıfsa ilk öncelik olmalı. Sonuç ekranında tek cümlelik, kaynaklı bir öğrenme notu (örn. AFAD) eklenmeli. Yüzde/sayı yerine nesne üzerindeki görsel sonuç öne çıkarılmalı.

### 2.3 Cities: Skylines – Natural Disasters

- **Eleştirilen:** Bazı afetler hasarsız ya da “yarım” hissettiriyor (deprem “amatörce” bulunmuş). Afetler oyuncuya meydan okuma değil “sıkıntı” gibi geliyor. Erken dönemde aşırı yıkıcı ve adil hissettirmeyen sonuçlar var ([Wccftech](https://wccftech.com/review/cities-skylines-natural-disasters-review/), [Vaporlens](https://vaporlens.app/app/515191/cities_skylines_natural_disasters)).
- **Ders:** Afet sunumu görsel olarak etkileyici olabilir, ama oyuncunun **kendi önleminin sonucunu** göremediği yıkım adaletsiz hissettirir. Bizde hasar normalize ediliyor ve %75 eşiği var. Ancak oyuncu kaybettiğinde “hangi önlemi alsaydım kurtulurdum” sorusunun cevabı ekranda olmalı.

### 2.4 Into the Breach (telgraflama)

- Düşman hamlelerini önceden göstermek her turu çözülecek bir bulmacaya çeviriyor ([Game Developer, Road to the IGF](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-)).
- **Ders:** Afet öncesi “bu evde neler devrilir / su nereden girer / yangın nerede başlar” tehlike önizlemesi (risk haritası), hem oyun tasarımı hem afet eğitimi açısından en güçlü eklentilerden biri olurdu. Gerçek hayattaki “evinde risk avı” egzersizinin karşılığı. Önceki araştırmanın “Hazırlık planı” önerisiyle örtüşüyor. **Not:** Telgraflama gerçek deprem tahmini gibi sunulmamalı; “riskli nesne” olarak sunulmalı (USGS ayrımı).

### 2.5 Overcooked! (zaman baskısı)

- Zaman baskısı + değişken mekân eğlencenin kaynağı. Ama aynı baskı bir kısım oyuncuyu bunaltıyor. Team17 sonradan süreyi uzatan/kapatan Assist Mode ekledi ([The Sixth Axis](https://www.thesixthaxis.com/2016/08/01/overcooked-review/), [Kotaku](https://kotaku.com/i-cant-convince-my-friends-that-overcooked-2-is-fun-no-1837838614)).
- **Ders:** Basit görevler zaman baskısı altında yeterince zorlaşır. Bize karmaşık iş mini oyunları değil, **basit ama zamanlı** işler gerekiyor. V4.9'da A/D–Enter–Space mini oyunlarını 0,60 sn otomatik montaja indirmek bu açıdan **doğru karardı**.

### 2.6 Jam postmortem'leri ve jüri yorumları (itch.io / Game Developer)

- En sık hata kapsamı fazla büyütmek. Önce çekirdek döngü, sonra cila. “Kapsamı %50 küçült, sonra bir daha küçült” ([itch.io postmortem derlemesi](https://jakerutter.itch.io/johnny-dreams-bad/devlog/247040/ludum-dare-48-my-first-jam), [Otter postmortem](https://pingu-pox.itch.io/untitled-otter-game/devlog/164731/mfgj-postmortem)).
- Jüri yardım/kontrol ekranı okumaz; oyun kendini oynarken öğretmeli ([Game Developer — Unity Game Jam Manila](https://www.gamedeveloper.com/game-platforms/unity-game-jam-manila-2012-part-3)).
- Mekanikleri tek tek tanıt, sonra birleştir. İlk 5 dakikada her şeyi öğretmeye çalışan tutorial'lar playtest'te çöküyor ([Game Developer — introducing mechanics](https://www.gamedeveloper.com/design/game-design-introducing-mechanics)).
- Jüri yol haritasını değil teslim edilen sürümü puanlar ([IGDC kılavuzu](https://indiagdc.com/?p=7473)).

### 2.7 “Juice” / oyun hissi

- “Juice it or lose it” (GDC 2012): kuralları değiştirmeyen kısa flaş, sarsıntı, yüzen metin, ses ve partikül, sıradan bir oyunu tatmin edici yapabilir. Ama tepkisellik önce gelir, fazla geri bildirim de kafa karıştırır ([itch.io blog](https://itch.io/blog/1059831/making-a-game-feel-juicy-with-simple-effects), [Wayline](https://www.wayline.io/blog/the-seductive-squeeze-when-juice-in-game-development-becomes-a-crutch)).
- **Bizde:** Normal kampanyada ses politikası “yalnız afet sesleri”. Toplama, vida takma, iş tamamlama ve UI **sessiz**. Kısa akışta yalnız 3 SFX var. Görsel/işitsel kriteri 15 puan. Sessiz bir “vida takıldı” anı, jüri gözünde yarım hissettirir. (Bu kullanıcı kararıdır. Burada yalnız maliyeti belirtiyorum, Bölüm 8.)

### 2.8 Afet eğitimi oyunları literatürü

- 2007–2025 arası 132 yayını tarayan 2026 ön-baskı sistematik derleme: oyunların gerçek etkinliği hakkında az şey biliniyor, yalnız 42 yayında oyuncu etkisi ölçülmüş ([EGUsphere 2026-2372](https://egusphere.copernicus.org/preprints/2026/egusphere-2026-2372/)).
- Afet temalı casual oyunlar en çok “ustalık deneyimi” üzerinden öz-yeterlik sağlıyor. Öz-yeterlik de hazırlıklı olmanın önemli bir yordayıcısı ([AMCIS 2025](https://aisel.aisnet.org/amcis2025/intelfuture/intelfuture/14)).
- “Çikolata kaplı brokoli” sorunu: öğrenme mekaniğe gömülmezse ilgi düşer. Öğretilen şey çekirdek mekanik olduğunda öğrenme daha yüksek çıkıyor (210 katılımcılı ön-kayıtlı çalışma, [Springer](https://link.springer.com/chapter/10.1007%2F978-3-030-34644-7_2)).
- **Bizde:** Sabitleme ve önlem **mekaniğin kendisi**. Bu, “içsel entegrasyon” açısından çok iyi bir konum ve jüriye anlatılması gereken bir güç. Buna karşılık çanta (su/gıda) bölümü daha “dışsal”: herhangi bir toplama oyununda da olabilir.

### 2.9 Türk sahnesinden doğrudan rakipler

- **Ekspres Çanta** (jamden çıkmış, ücretli itch.io sürümü) deprem çantasını ve efsane kırmayı merkeze alıyor ([itch.io](https://imdiestudios.itch.io/ekspres-canta)). **Earthquake Simulator** (24 saatlik jam, Türkçe) öncesi/anı/sonrasını çanta kategorileriyle işliyor ([itch.io](https://betulkara.itch.io/earthquake-simulator)). İTÜ'den mobilya sabitleme odaklı bir VR ciddi oyun çalışması var ([İTÜ](https://research.itu.edu.tr/en/publications/enhancing-earthquake-preparedness-and-awareness-a-virtual-reality/)).
- **Sonuç:** “Deprem çantası hazırla” Türk jam sahnesinde **çok işlenmiş** bir fikir. Jüri muhtemelen benzerini gördü. Özgünlük puanı çantadan değil, **evin fiziksel olarak hazırlanması ve sonucun nesnede görülmesinden** gelecek. Sunum bunu öne çıkarmalı; çanta ikincil kalmalı.

---

## 3. Tema doğruluğu: en kritik eleştiri

Jam'in ana organizatörü KTMMOB İnşaat Mühendisleri Odası (Kitapçık s.1). Jüride oyun sektöründen isimler olduğu biliniyor; tamamının mühendis olduğu doğrulanmadı. Ama bir İMO etkinliğinde **yanlış güvenlik davranışı öğretmek**, “Temayla ilişki” kriterinde ağır bir hata olur.

### 3.1 Bodrum sığınağı her afet için doğru hedef değil (kampanya ↔ kısa akış çelişkisi)

| Afet | Resmî kılavuz | Kısa akış (`PanicJuryDemo`) | Normal kampanya (`PanicModel`) |
|---|---|---|---|
| Deprem | Sarsıntıda koşma; çök–kapan–tutun. Sonra toplanma alanı. Kapı eşiği, dışarı koşma ve “yaşam üçgeni” önerilmiyor ([King County OEM](https://www.kentreporter.com/?p=20725), [Yaşam üçgeni efsanesi](https://en.wikipedia.org/wiki/Triangle_of_Life)) | ✔ `QuakeCover` + “Sarsıntıda koşma” | ✖ Son saniyelerde bodrum bunkerine koşmak |
| Sel | Yüksek zemine/üst kata çık, bodrumda mahsur kalma, sele girme ([Ready.gov Floods](https://www.ready.gov/floods), [FEMA bilgi notu](https://taylorcountywv.gov/document_center_uploads/po_flood-information-sheet.pdf)) | ✔ `FloodHighGround` + “Bodruma inme” | ✖ **Bodruma in** |
| Yangın | “Dışarı çık, dışarıda kal”, dumanda alçal ([Kızılhaç notu](https://deltasac.org/wp-content/uploads/2022/09/Be-Red-Cross-Ready_Fire-Safety-Floods_09.22.22.pdf)) | ✔ `FireAssembly` (dış toplanma alanı) | ✖ **Bodruma in** |

Kısa akış doğruyu öğretiyor, kampanya tersini yapıyor. Aynı oyunda iki çelişkili mesaj var. “SERBEST OYUN”a geçen bir jüri üyesi 4. dalgaya ulaşmasa bile oyunun **temel kuralı** (“bunkera yetiş”) her afette bodrumu ödüllendiriyor.

**Öneri (yeniden kullanım yüksek):** `PanicJuryDemo` içindeki üç hedef (`QuakeCoverPosition`, `FloodRefugePosition`, `FireAssemblyPosition`) ve doğrulama mantığı zaten var. Kampanyanın “güvenli hedef” kavramı afet türüne göre bu üç hedefe bağlanmalı, “bunker” adı da “güvenli nokta” olmalı. Bu yapılamıyorsa en azından kampanyadaki sığınak **deprem için de** “sarsıntı öncesi tatbikat noktası” diye yeniden adlandırılmalı ve sel/yangın dalgalarında bodrum hedefi kapatılmalı. **Bu raporun bir numaralı tasarım önerisi bu.**

### 3.2 “Bunker” kavramı yerel afet kültürüne yabancı

Türkiye/KKTC afet eğitiminde müstakil evde “bunker” yok; vurgu toplanma alanı, sabitleme ve 72 saatlik çantada. “Bunker”, Soğuk Savaş ve ABD hortum kültürü çağrışımı taşıyor (60 Seconds!'ın nükleer teması). Hedef jüri için daha doğru dil: **“Toplanma alanı / güvenli nokta / tatbikat noktası”.**

### 3.3 Çanta: “1 su + 1 gıda yeterli”

GDD bunu “gerçek tavsiye değil, oyun dengesi birimi” diye not ediyor. Oyuncu ise bu notu görmüyor. AFAD kaynaklı yönlendirmeler en az **72 saat** yetecek su ve gıdayı öneriyor ([Karar, AFAD önerisi](https://www.karar.com/guncel-haberler/deprem-cantasini-ilk-72-saate-gore-hazirlayin-1982570), [Milliyet](https://www.milliyet.com.tr/gundem/deprem-cantasi-icindekiler-nasil-hazirlanir-deprem-cantasi-listesinde-neler-olmali-canta-nerede-durmali-6365448)). **Ucuz düzeltme:** İkon/etiket “Su (3 günlük)” ve “Gıda (3 günlük)” olsun. Mekanik değişmez, mesaj doğru olur.

### 3.4 Geri sayım = deprem tahmini mi?

Önceki araştırma bunu zaten işaretledi: USGS'ye göre deprem tarih/saatiyle tahmin edilemez. Oyun içi metinde “tatbikat” çerçevesi bulamadım. **Ucuz düzeltme:** Başlıkta ya da ilk mesajda “Tatbikat: deprem şimdi olsaydı hazır mıydın?” gibi bir çerçeve. Hem dürüst hem jüriye temayı (“Hazırlık ve Önlem”) tek cümlede anlatıyor.

### 3.5 Güçlü yanlar (korunmalı)

- Mobilya sabitleme, yapısal olmayan riski azaltmada gerçek ve önemli bir eylem ([FEMA E-74](https://www.fema.gov/sites/default/files/2020-07/fema_earthquakes_reducing-the-risks-of-nonstructural-earthquake-damage-a-practical-guide-fema-e-74.pdf)). İnşaat mühendisi bir jüri üyesi bunu doğru bulur.
- Kısa akıştaki üç farklı afet davranışı (çök–kapan–tutun / yüksek kat / dış toplanma) oyunun **en güçlü eğitsel anı**. Sunumda mutlaka gösterilmeli.
- “Başarı uydurulmaz; tamam/eksik/atlandı ayrılır” ilkesi, Stop Disasters'ın “kayıpta tebrik” hatasından kaçınıyor.

---

## 4. Oynanabilirlik (25 puan): bilişsel yük

### 4.1 Kontrol sayısı

GDD tablosunda ~25 ayrı tuş/eylem var; birkaçı bağlama göre anlam değiştiriyor (Space = zıpla / iş; Esc = duraklat / iptal; E = aç / sığınak / FPS'te geçiş). Ayrıca F2 geliştirici paneli ve görünür menü düğmesi var.

**Best practice:** Jamde temel kontroller WASD + fare + tek eylem tuşuyla sınırlı tutulmalı; ekranda her an yalnız o an geçerli 1–2 tuş görünmeli. Mekanik tek tek tanıtılmalı.

**Somut öneriler:**
- Kısa akışta Q, T, 1–4, R, PageUp/Down **hiç gösterilmesin ve öğretilmesin**. Seçili hedefte tek bir “E: Sabitle / Güçlendir / Drenaj kur” bağlam eylemi yeterli. (V4.9'da “yalnız geçerli seçenek gösterilir” kararı bu yönde; aynı sadeleştirme tuş ipuçlarına da uygulanmalı.)
- **F2 / görünür geliştirici düğmesi jüri build'inde görünmemeli.** Hile işareti skor yazmasa da jüri ekranında “geliştirici” düğmesi görmek bitmemiş ürün izlenimi verir. Tuş kısayolu kalsın, buton gizlensin.
- **J (defter):** Oyunun kendisi öğretiyorsa defter fazla. Jüri onu açmayacak.

### 4.2 Perspektif değişimi (izometrik → FPS)

Forum gözlemleri: hızlı kamera geçişleri yön kaybı ve bazı oyuncularda baş dönmesi yaratıyor; geçişten sonra oyuncu nereye gideceğini bilemiyor ([itch.io yorumları](https://itch.io/post/6649199), [itch.io](https://itch.io/post/10604735)). Geçişin bizdeki bağlamı en kötü an: son 30 saniye, en yüksek zaman baskısı. Oyuncu 4 dakika boyunca izometrik kontrolü öğreniyor, sonra 1,1 saniyede FPS kontrolüne atılıyor. Bu süreçte WASD'nin anlamı (kameraya göre → bakış yönüne göre) değişiyor, fare kamerayı döndürmeye başlıyor.

**Objektif değerlendirme:** Bu özellik “Yaratıcılık” puanına küçük bir artı, “Oynanabilirlik” puanına muhtemelen daha büyük bir eksi getiriyor. Sunumda göze çarpar. Ama jüri kendisi oynarken kafası karışırsa bu yalnız zarar olur. **İki seçenek:**
- (A) Kalıcı karar: FPS çanta aşamasını kaldırıp toplamayı izometrikte yapmak (60 Seconds modeli).
- (B) Tutmak, ama geçişten önce 3 saniyelik açık uyarı + FPS'te çantaya yön oku + yalnız 2 hedef (su, gıda) göstermek.

Hangisinin doğru olduğu **3 yeni oyuncuyla 10 dakikalık bir testle** belirlenebilir. Test yapılmadan karar verilmemeli.

### 4.3 Anlaşılırlık

Kısa akış ilk 180 saniyede bir tam döngü hedefliyor. Doğru hedef, ama ölçülmedi. Kayıtlı OS denemesi kısa tuş basışlarıyla yapıldı, kesintisiz yürüyüş rotası ve insan keyif kabulü yok.

---

## 5. Yaratıcılık (20 puan)

- **Özgün olan:** Evin kendisinin oyun alanı olması; önlemin nesnede görünmesi; tek koşuda üç farklı afet davranışı; iki perspektifli hazırlık.
- **Özgün olmayan:** Deprem çantası hazırlama (Türk jam sahnesinde çok işlenmiş, Bölüm 2.9); sığınağa koşma (60 Seconds).
- **Eksik fırsat, en yaratıcı ve eğitsel ek:** “**Önlemsiz olsaydı?**” karşılaştırması. Afet sonrası aynı odanın sabitlenmemiş halinin 2–3 saniyelik tekrarı ya da yan yana kare. Eğitim literatüründeki neden–sonuç ve “içsel entegrasyon” ilkesini tek anda gösterir. **Uyarı:** Önceki araştırmanın da belirttiği gibi bu, hesaplanmamış bir sayı (“%25 kurtardın”) olarak değil, **aynı tohumla yan etkisiz yeniden çözülmüş gerçek sonuç** olarak gösterilmeli. Bu yapılamıyorsa iki eşyalı kontrollü tepki (biri sabitli, biri değil) aynı işi daha ucuza görür.

---

## 6. Teknik bütünlük (15 puan) ve süreç

### 6.1 Ne iyi?

Çökme ve runtime hata kaydı sıfır. Maliyet ve iptal atomikliği, controller/kapı güvenliği, skor yalıtımı titizlikle test edilmiş. Bu 15 puanlık kriterde muhtemelen üst dilimdesiniz.

### 6.2 Objektif eleştiri: emek yanlış yere akıyor

- **24 saatte 14 sürüm** (V4.1→V4.14, alt revizyonlarıyla `Builds/` altında 26 build klasörü ve 8 ZIP, toplam **21 GB**). Çalışma düzeni “yalnız son doğrulanmış build tutulur” diyor. Temizlik bir kez politika gereği reddedilmiş ve tekrarlanmamış. Disk dolarsa son build'in kendisi tehlikeye girer.
- **Doğrulama dengesi ters.** Yüzlerce otomatik API/render kontrolü var, sıfır insan oynanış testi var. Jüri bir API değil. Bu oyunun puanını belirleyecek iki kriter (oynanabilirlik ve tema) otomatik testle ölçülemiyor.
- **Karar çalkantısı.** FPS önce ayrı proje, sonra askıya alındı, sonra çanta aşaması olarak geri geldi, sonra otomatik ve geri dönüşsüz hale getirildi. Ses politikası en az üç kez değişti. Kayıp eşiği %65'ten %75'e çıktı. Kontrol sözleşmesi A/D–Enter–Space'ten fare vidasına geçti. Bunların her biri tek başına makul. Toplamda ise kilitlenmiş bir vizyon olmadığını gösteriyor. `AGENTS.md`'nin kendisi bile karar günlüğüne dönmüş (aynı paragrafta birbirini geçersiz kılan kurallar var). Bu, yeni katılan bir ajanın ya da insanın yanlış kuralla çalışma riskini artırıyor.
- **Belgeleme ile oyun arasındaki oran.** Birleşik rapor tek başına 140 KB. Jüri bunun hiçbirini görmeyecek. Teslim formunun istediği yalnızca kısa açıklama, kurulum/kontrol talimatı, asset beyanı ve AI beyanı.

**Öneri:** Kalan süre için **sürüm dondurma kuralı** konulmalı. Belirli bir saatten sonra (öneri: 18 Ekim 10:00) yalnız tema düzeltmesi, metin ve kritik hata düzeltmesi yapılsın; yeni özellik eklenmesin. Bundan önce en az bir insan testi yapılsın.

---

## 7. Görsel/işitsel bütünlük (15 puan) ve sunum

- **Görsel:** Renk paleti tutarlı tanımlanmış, ev dolu, HUD konseptten uyarlanmış, logo/ikon eklenmiş. Ancak görsel kabul ekran görüntüsü/render kontrolüdür; son sanat kabulü insanla yapılmadı.
- **Karakter:** İki yeni hareket denemesi reddedilmiş, eski humanoid korunmuş. Pragmatik ve doğru bir karar: bozuk animasyon, eski ama çalışan animasyondan kötüdür.
- **Ses:** Bölüm 2.7. 15 puanın yaklaşık yarısı “ses öğelerinin oyunla uyumu”. Etkileşim seslerinin olmaması en kolay kaybedilen puanlardan biri. Lisansı temiz CC0 bir tık/vida/onay seti, yeni sağlayıcı harcaması gerektirmeden bu boşluğu kapatabilir. (Ses politikası kullanıcı kararıdır. Değiştirilmesi açık onay ister.)

**Final sunumu (md. 7.3):** Sunum yapılacağı kesin.
- Canlı demo çökerse diye yedek bir 60–90 saniyelik **oynanış videosu** hazır olsun.
- Akış: tek cümlelik kanca (“Afet anında değil, öncesinde oynanan bir oyun”) → sabitleme → deprem → sabitli/sabitsiz farkı → sel/yangında farklı doğru davranış → sonuç karnesi → asset ve AI beyanının tek slaytta özeti.
- Jüri yol haritasını değil teslim edilen sürümü puanlar. “Sonra ekleyeceğiz” denmemeli.

---

## 8. Ne yapılmalı, ne yapılmamalı, ne kaldırılmalı

Öncelik, kalan süreye (son teslim 18 Ekim 18:00, Git desteği 12:00) göre sıralandı. Maliyet tahminleri görecelidir, ölçülmüş değildir.

### 8.1 Mutlaka (puandan önce: uygunluk)

1. Teslim formuna `main` HEAD'i değil, teslim edilen ZIP'in **release tag commit'ini** yaz (bugün: `v0.4.13` → `b551edfc…`); çıkış yapılmış tarayıcıda indirme testini yap.
2. Tek bir **AI beyanı** yaz: ajan tabanlı kod/belge üretimi, Antigravity (Yarkın), ElevenLabs (konuşma, efekt, müzik), Meshy (yalnız V4.14+). Her biri araç / amaç / oyundaki yer olarak. Taslak: `TOO-LATE-TO-PANIC-JAM-TESLIM-BEYANI.md`.
3. Asset beyanı: Quaternius CC0, OFL fontlar, Yarkın kaynakları, Meshy. Lisansı doğrulanamayan ham dosyaları public repodan çıkar (md. 3.6).

### 8.2 Yüksek değer / düşük–orta maliyet (tema ve oynanabilirlik)

4. **Kampanyada afete göre doğru güvenli hedef.** Kısa akıştaki üç hedefi yeniden kullan; sel/yangında bodrumu ödüllendirmeyi bırak (Bölüm 3.1). En azından jüriye kampanya gösterilmeyecekse kampanyada bu sorunun bilindiği teslim notunda yazsın.
5. Metin düzeltmeleri: “Bunker” yerine “güvenli nokta/sığınak”, su/gıdaya “3 günlük” etiketi, “tatbikat” çerçevesi.
6. Kısa akışta görünür tuşları 4–5'e indir; F2 düğmesini jüri build'inde gizle.
7. **3 yeni oyuncuyla süreli test** (her biri ≤5 dk, hiç yardım etmeden): Nerede takıldı? FPS geçişinde yön kaybetti mi? İlk 180 saniyede bir sonuç gördü mü? Bu tek test, otomatik kontrollerin yüzlercesinden daha çok puan bilgisi verir.

### 8.3 Değerli ama süre izin verirse

8. İki eşyalı kontrollü afet tepkisi (sabitli ayakta, sabitsiz devrik). Yoksa en önemli eksik budur.
9. Sonuç ekranında kaynaklı tek cümlelik öğrenme notu (AFAD/FEMA).
10. Minimal etkileşim sesleri (kullanıcı onayıyla).
11. Sunum için yedek video.

### 8.4 Yapılmamalı (bu jam için)

- Zincirleme afetler, bakım/aşınma ekonomisi, FRP/çapraz/mantolama ağacı, yük/burulma modeli (önceki araştırmanın kararıyla aynı).
- Yeni karakter/animasyon denemeleri (iki kez reddedildi; süre yok).
- Yeni oda/mekân. Ev zaten büyük. Jüri 5 dakikada garaj/balkon/bodrumu gezmeyecek.
- Daha fazla otomatik test turu. Mevcut kapsam teknik kriter için yeterli; marjinal değeri düşük.

### 8.5 Kaldırılması/gizlenmesi düşünülmeli

- **Görünür geliştirici paneli düğmesi** (jüri build'inde).
- **J defteri** (kısa akışta). Bilgi oyunun içinde verilmeli.
- **Kısa akışta Q/T/1–4/R/PageUp ipuçları.**
- **Otomatik FPS geçişi** (yalnız insan testi yön kaybını doğrularsa; Bölüm 4.2 A seçeneği).
- **Kampanyada sel/yangın için bodrum hedefi.**

---

## 9. Kriter bazında tahmini konum (yargı, ölçüm değil)

| Ölçüt | Bugünkü tahmin | Bölüm 8.1–8.2 sonrası | Gerekçe |
|---|---|---|---|
| Oynanabilirlik (25) | 13–17 | 17–20 | Çekirdek döngü sağlam; kontrol yükü ve perspektif değişimi riskli; insan testi yok |
| Temayla ilişki (25) | 16–20 | 20–23 | Tema mekaniğin içinde (güçlü). Kampanya bodrum çelişkisi ve “1 su” mesajı puan kırdırabilir |
| Yaratıcılık (20) | 12–15 | 13–16 | Ev hazırlığı + nesnede sonuç özgün; çanta fikri yaygın |
| Görsel/işitsel (15) | 9–11 | 10–12 | Dolu ev ve HUD iyi; etkileşim sesleri yok |
| Teknik (15) | 12–14 | 12–14 | Çok iyi test edilmiş; asıl risk teslim/repo eşleşmesi |

Bu tablo, aynı jamdeki diğer takımlarla karşılaştırma yapmaz. Yalnız oyunun kendi kriterlerdeki boşluklarını sıralar.

---

## Kaynaklar

**Resmî/kılavuz:** CIVIL GAME JAM Kural Kitapçığı v04 (yerel PDF, `Preparation/rule_source.json`) · [Ready.gov Floods](https://www.ready.gov/floods) · [FEMA sel bilgi notu (Taylor County)](https://taylorcountywv.gov/document_center_uploads/po_flood-information-sheet.pdf) · [Kızılhaç yangın/sel notu](https://deltasac.org/wp-content/uploads/2022/09/Be-Red-Cross-Ready_Fire-Safety-Floods_09.22.22.pdf) · [King County OEM — Drop, Cover, Hold](https://www.kentreporter.com/?p=20725) · [Triangle of Life](https://en.wikipedia.org/wiki/Triangle_of_Life) · [FEMA E-74](https://www.fema.gov/sites/default/files/2020-07/fema_earthquakes_reducing-the-risks-of-nonstructural-earthquake-damage-a-practical-guide-fema-e-74.pdf) · [Karar — AFAD 72 saat](https://www.karar.com/guncel-haberler/deprem-cantasini-ilk-72-saate-gore-hazirlayin-1982570) · [Milliyet — deprem çantası](https://www.milliyet.com.tr/gundem/deprem-cantasi-icindekiler-nasil-hazirlanir-deprem-cantasi-listesinde-neler-olmali-canta-nerede-durmali-6365448)

**Benzer oyunlar / eleştiriler:** [60 Seconds! — Pocket Gamer](https://www.pocketgamer.biz/interview/64163/the-making-of-60-seconds/) · [GameLuster](https://gameluster.com/60-seconds-review/) · [Dread Central](https://www.dreadcentral.com/reviews/105448/60-seconds-video-game/) · [OpenCritic](https://opencritic.com/game/3298/view) · [Vaporlens 60 Seconds](https://vaporlens.app/app/368360/60_seconds) · [UNDRR Stop Disasters](https://www.undrr.org/news/stop-disasters-game-turns-disaster-prevention-real-life-challenge) · [WPI analizi](https://web.cs.wpi.edu/~rich/courses/imgd4600-c15/analyses/stop-disasters/index.html) · [Mechanics of Magic](https://mechanicsofmagic.com/2025/09/29/today-i-stopped-disasters-poorly/) · [Common Sense Media](https://www.commonsensemedia.org/game-reviews/stop-disasters) · [Wccftech — C:S Natural Disasters](https://wccftech.com/review/cities-skylines-natural-disasters-review/) · [Vaporlens C:S ND](https://vaporlens.app/app/515191/cities_skylines_natural_disasters) · [Into the Breach — Game Developer](https://gamedeveloper.com/game-platforms/road-to-the-igf-subset-games-i-into-the-breach-i-) · [Overcooked — The Sixth Axis](https://www.thesixthaxis.com/2016/08/01/overcooked-review/) · [Kotaku](https://kotaku.com/i-cant-convince-my-friends-that-overcooked-2-is-fun-no-1837838614) · [Ekspres Çanta](https://imdiestudios.itch.io/ekspres-canta) · [Earthquake Simulator](https://betulkara.itch.io/earthquake-simulator) · [İTÜ VR ciddi oyun](https://research.itu.edu.tr/en/publications/enhancing-earthquake-preparedness-and-awareness-a-virtual-reality/)

**Jam / tasarım pratiği:** [Unity Game Jam Manila](https://www.gamedeveloper.com/game-platforms/unity-game-jam-manila-2012-part-3) · [Introducing mechanics](https://www.gamedeveloper.com/design/game-design-introducing-mechanics) · [LD48 postmortem](https://jakerutter.itch.io/johnny-dreams-bad/devlog/247040/ludum-dare-48-my-first-jam) · [MFGJ postmortem](https://pingu-pox.itch.io/untitled-otter-game/devlog/164731/mfgj-postmortem) · [IGDC kılavuzu](https://indiagdc.com/?p=7473) · [itch.io juice](https://itch.io/blog/1059831/making-a-game-feel-juicy-with-simple-effects) · [Wayline — juice crutch](https://www.wayline.io/blog/the-seductive-squeeze-when-juice-in-game-development-becomes-a-crutch) · [itch.io kamera geçişi yorumları](https://itch.io/post/6649199), [2](https://itch.io/post/10604735)

**Akademik:** [EGUsphere 2026 sistematik derleme](https://egusphere.copernicus.org/preprints/2026/egusphere-2026-2372/) · [AMCIS 2025 öz-yeterlik](https://aisel.aisnet.org/amcis2025/intelfuture/intelfuture/14) · [Habgood — içsel entegrasyon](https://link.springer.com/chapter/10.1007%2F978-3-030-34644-7_2)
