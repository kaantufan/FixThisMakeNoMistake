# E2E ayrıntılı test kataloğu

17 Ekim 2026 · 257 çekirdek vaka · Hepsi PLANNED; oyun testi sonucu değildir.

Aktif aday V4.9 DirectActions; V4.8 legacy beklentileri yalnız ilgili davranış profilinde uygulanır. Her yürütme build/contract/DLL ve kanıt katmanına bağlanır. Aynı vaka farklı hedef/çözünürlük/seed ile koşulursa alt sonuçlar ayrı tutulur.

## MEC-001 — İlk açılıştan üç afet ve temiz tekrar başlangıcına tam kısa rota

**Alan:** Kesintisiz kısa akış · **Öncelik:** P0 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Kısa hazırlık akışı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Tüm geliştirici seçenekleri kapalı; kayıt dosyası başlangıç hashı alınmış.
- İzlenecek hedefler ve kameraya göre WASD yönü yürütme öncesinde ekranda tanımlanmış; rota sırasında koordinata ışınlama yok.

### Adımlar

1. Başlıkta HAZIRLIĞA BAŞLA düğmesine fareyle tıkla; kronometreyi tıklamada başlat. Menüde kullanılan tıklamanın dünyada ikinci iş başlatmadığını kaydet.
2. WASD ile en az .85 m gerçek yürüyüş yap; hareket hedefinin tamamlanmasını ve sonraki su yönlendirmesini gör.
3. Buzdolabına fiziksel yaklaş; gerekiyorsa Q ile onu seç. E ile aç, görünür suyu Z/X ile seç ve Fye bir kez bas. Başlangıç gıdasını kendi toplaman gibi sayma.
4. Tab aç; oklarla gerçek su yuvasını seç, Entera bir kez bas; çanta su=1/gıda=1 ve boşalan envanter yuvasını gör. Tab ile kapat.
5. Uygun yüksek buzdolabını seç; gerekirse T ile geçerli ankraj yüzeyini seçip1e bas. Yakın plandaki ilk gerçek vidaya sol tıkla ve0.60sn oturmasını bekle; sonra ikinci gerçek vidaya tıkla ve0.60sn bekle. A/D,Enter veyaSpace mini oyunu kullanma.
6. Bütçe ve braket/vida azalmasını bir kez kaydet; görsel bağlantı oluşunca WASD ile işaretli çök–kapan–tutun noktasına yürü. Zeminde E ile sığın.
7. Depremi ve sonuç ekranını doğal süresiyle izle; hiçbir Enter/Atla kullanma. Gerçek eşya hasarı, su/gıda tüketimi ve Flood aşamasına tek geçişi kaydet.
8. Bahçe girişine fiziksel yürü;kapıyı E ile aç,drain_east hedefini seç ve E ile doğrudan kurulumu başlat. Dünya üstünde0.60sn karakter yapım pozuyla tamamlanmasını izle.
9. Kum torbası=2, tahta=1 ve 50 bütçe tüketildiğini gözle; ön kapıdan içeri dön, gerçek merdivenden üst kata yürü.
10. Üst güvenli noktanın içinde zeminde durup E bas. Alt kat konumu veya boşluk üzerinden hedef tamamlandı varsayma.
11. Fire yönlendirmesiyle üst kattan gerçek merdivenle alt kattaki dedicated yangın istasyonuna yürü. Gerçek envanter tüpüyle görünür kurulum eylemini başlat;0.60sn yapımı ve tek tüp/75bütçe giderini kaydet.
12. Gerçek merdivenden alt kata in, E ile bahçe kapısını aç, işaretli dış toplanma alanına yürü ve zeminde E bas.
13. İsteğe bağlı incelemede birkaç adım yürü; Devam düğmesiyle sonucu aç. On hedefin durumunu ve üç ayrı afet sonucunu ekrandan kaydet; puan/ödül bekleme.
14. TEKRAR DENEye tıkla; yeni koşuda su yeniden dolapta, 1 gıda çantada, önlemler temiz, bütçe1000 ve aşama Onboarding olmalı. Başlığa dön ve izole skor hashını tekrar al.

### Beklenen sonuç

- Tek kesintisiz gerçek giriş rotası bütün hedefleri gerçekten tamamlar; başarı, maliyet, sığınma ve sonuç bayrakları birbirine uygundur.
- 270 sn içerik tavanı/300 sn mutlak sınırı aşılmaz; ilk180 kilometre taşları ölçülür ama insan keyfi bu vakadan çıkarılmaz.
- Kısa akış skora yazmaz, tekrar başlangıcı önceki hedef/hasar/loot durumunu taşımamalıdır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Su alma, çantaya koyma, ilk gerçek önlem, ilk görünür afet, üst sığınma ve dış çıkış için ayrı zaman damgaları.

### Regresyon

- V4.8 FullFlow 149 kontrolün semantik rotası OS girdisiyle tamamlanacak; geçmiş49.67sn insan ölçümü değildir.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:97
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:169
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:236
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:303
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:379
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:536
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:134
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530

## MEC-002 — Kitaplık ve zemin ankrajı seçimiyle deprem atlama ve fiziksel devam

**Alan:** Kesintisiz kısa akış · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Kısa hazırlık akışı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Yeni kısa koşu; hile kapalı; MEC-001den ayrı GUID.

### Adımlar

1. Başlıkta Enterla HAZIRLIĞA BAŞLA; WASD ile hareket hedefini aç.
2. Buzdolabına yürü, E-F ile tek suyu al; Tab-ok-Enter ile çantala ve Tab ile kapat.
3. Kitaplığa yürüyerek git;Q ile uygun uzun mobilyayı seç. T yüzey seçimi final adaptörde korunuyorsa Zemin seç;1 ile ankrajı aç. Birinci gerçek vidaya tıkla0.60sn bekle,ikinci vidaya tıkla0.60sn bekle;iki vida tamamlanınca maliyeti kaydet.
4. Braket1/vida2/60 bütçe bir kere tüketilirken kitaplığın zemin bağlantısını ve buzdolabının sabit kalmadığını kaydet.
5. Güvenli noktaya yürü ve E; deprem başladıktan bir saniye sonra Entera bir kez bas.
6. Bir kare aralıkla Enterı tekrar dene; atlamanın sonucu bir kez üretmesini, quake_observed için Skipped durumunun korunmasını ve su/gıda iki kez düşmemesini kaydet.
7. Sonuçta toplam hasar ve iki eşyanın gerçek karşılaştırmasını oku; Devama tek tıkla Flooda geç.
8. Ön kapıya yürü,E ile aç;drenajı E ile başlatıp0.60sn dünya kurulumu bitince stok/sayaç farkını kaydet.
9. Merdivenden üst kata yürü; sığınma alanında E bas; Firea geçiş bir kez olur.
10. Fire markerını izleyip alt kattaki dedicated yangın istasyonuna yürü;gerçek tüple0.60sn kurulum yap. Ön kapıdan çık,dış toplanma alanına yürü veE bas.
11. Sonuca ilerle; ATLANDI ile TAMAM ayrımını oku. TEKRAR DENE ve sonra BAŞLIĞA DÖNü kullan.

### Beklenen sonuç

- Alternatif eşya ve zemin ankrajı gerçek işi kullanır; atlama hareket kontrolünü ve doğal sonraki rotayı bozmamalıdır.
- Deprem hesabı ve tüketim bir kez yapılır; atlanan gözlem başarıya çevrilmez; skor dosyası değişmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:193
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:238
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:346
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:535
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs:48
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530

## MEC-003 — Hiç önlem almadan doğal sürelerin bitişi ve dürüst eksik sonucu

**Alan:** Kesintisiz kısa akış · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Kısa hazırlık akışı / süre sınırı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Yeni kısa koşu; hile kapalı; görüntü ve monoton gerçek zaman sayacı açık.

### Adımlar

1. HAZIRLIĞA BAŞLAya tıkla ve dünya girdisini bırak; ilk20sn hareket hedefini tamamlamadan bekle.
2. Preparation boyunca su toplama/çantaya koyma/ankraj/sığınma yapma; 80sn dolunca otomatik depreme geçişi izle.
3. Deprem8sn ve sonuç4sn boyunca Enter kullanma; eksik hedeflerin tamamlanmış görünmediğini kaydet.
4. Flood50sn boyunca drenaj veya üst sığınma yapma; Firea doğal geçişi izle.
5. Fire70sn boyunca tüp veya çıkış yapma; Inspection18sn ve Result20sn akışını bekle.
6. 270sn civarında Complete durumunu ve 300sn mutlak sınıra rağmen yeni görev doğmadığını kaydet; render gecikmesini ayrıca ölç.
7. Sonuçtaki on hedefin EKSİK/TAMAM/ATLANDI sayılarını ekranla eşleştir; başarısız hazırlık ödülü olmamalı.
8. TEKRAR DENEye tıkla; WASD, E-F, Tab-Enter ile bu kez su hedeflerini gerçekten tamamla.
9. Esc ile duraklat ve BAŞLIĞA DÖN; kalan süre/önlem ve skor dosyasının yeni başlangıca sızmadığını denetle.

### Beklenen sonuç

- Hiçbir hareketsiz hedef Completee çevrilmez; sadece gerçekten sunulan deprem gözlemi ayrı değerlendirilebilir.
- 270 içerik/300 mutlak güvenlik sınırı tutulur; durmadan kalan saat veya sonsuz döngü oluşmamalıdır.
- Bu olumsuz rota oyuncu başarısı olarak raporlanmaz; tekrar kullanılabilir kalır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:145
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:216
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:271
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:296
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:303
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:449

## MEC-004 — Deprem simülasyonu: seçimden doğal sonuç, inceleme ve aynı afetle tekrar

**Alan:** Kesintisiz bağımsız afet · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Bağımsız simülasyon / Deprem

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Yeni başlık; tüm geliştirici seçenekleri kapalı; ayrı GUID;1su/1gıda başlangıçtır.

### Adımlar

1. Başlıkta FELAKET SİMÜLASYONU OYNAya tıkla; Deprem kartını fareyle seç.90sn ve sonlu hazırlık stokunu kaydet.
2. WASD ile ön kapıya yürü; E ile aç. Gerçek controller ve kat geometrisinden geçerek hedefe yaklaş; kamera yönüne göre tuşları çevir.
3. Garaj alet kutusuna yürüyerek eriş; gerekliyse E ile aç ve Z/X-F ile braket/vida al. Kaynak toplama ile hazır başlangıç stokunu ayrı yaz.
4. Buzdolabına geri yürü;uygun yüzeyi seçip1 ile ankraj aç. İlk vidayı tıklayıp otururken Backspace ile iptal et;gider olmamalı. Yeniden aç,iki gerçek vidayı sırayla tıklayıp her birinin0.60sn oturmasını bekle;tek commit olmalı.
5. Tab ile envanter/çanta aç; başlangıç1su/1gıdanın ve işte harcanan stokların gerçek değerlerini kaydet, Tab ile kapat.
6. Fareyi AFETİ BAŞLAT düğmesine götür ve tek tıkla. Çift tıklamanın ikinci afet yaratmamasını kaydet.
7. Enter/Atla kullanmadan8sn afet+4sn aftermathı doğal izle; loss varsa1.35sn bildirim ve6sn kayıp sinematiği bitmeden hareket deneme.
8. Sonucun gerçek terminalini oku. Hasar>%65 ise kaybı başarı sayma; HASARI İNCELE düğmesi sunulduğunda tıkla.
9. İncelemede WASD ile en az.5m yürü, hasar etiketini ve önlemin kalmasını gör; su/gıda artık0olmalı, ödül/puan0 kalmalı.
10. TEKRAR DENEye tıkla; aynı afet,90sn,1000bütçe,1su/1gıda, taze stok ve sıfır hasar dönüşünü kaydet.
11. Esc-BAŞLIĞA DÖN ile menüye git; skor dosyasının byte hashını karşılaştır.

### Beklenen sonuç

- Seçilen afetin tek gerçek çözümü vardır; başarılı inceleme veya kayıp sonucu kurallarla açıklanır.
- Ordinary loss incelemesi kayıp nedenini/hasarı korur; tekrar yeni bağımsız koşudur. Skor/ödül ve otomatik sonraki dalga yoktur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Başlangıç, iş bitişi, afet başlatma, doğal terminal, yürünebilir inceleme ve tekrar kareleri; gerçek delta/süre.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:7
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:18
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:559
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:801
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1077
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:481
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:553
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:916

## MEC-005 — Sel simülasyonu: seçimden doğal sonuç, inceleme ve aynı afetle tekrar

**Alan:** Kesintisiz bağımsız afet · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Bağımsız simülasyon / Sel

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Yeni başlık; tüm geliştirici seçenekleri kapalı; ayrı GUID;1su/1gıda başlangıçtır.

### Adımlar

1. Başlıkta FELAKET SİMÜLASYONU OYNAya tıkla; Sel kartını fareyle seç.90sn ve sonlu hazırlık stokunu kaydet.
2. WASD ile ön kapıya yürü; E ile aç. Gerçek controller ve kat geometrisinden geçerek hedefe yaklaş; kamera yönüne göre tuşları çevir.
3. Drenajın yanına yürü;E ile doğrudan kurulumu başlat.0.60sn yapım ve karakter pozunu izle;50bütçe,2kum torbası,1tahta tek kez düşer.
4. Ön kapıdan içeri dön; merdivenden üst kata çık, inişe dön ve yeniden yukarı çık. Bağımsız modda kısa akışa ait sığınma hedefi veya E başarısı varsayma.
5. Tab ile envanter/çanta aç; başlangıç1su/1gıdanın ve işte harcanan stokların gerçek değerlerini kaydet, Tab ile kapat.
6. Fareyi AFETİ BAŞLAT düğmesine götür ve tek tıkla. Çift tıklamanın ikinci afet yaratmamasını kaydet.
7. Enter/Atla kullanmadan8sn afet+4sn aftermathı doğal izle; loss varsa1.35sn bildirim ve6sn kayıp sinematiği bitmeden hareket deneme.
8. Sonucun gerçek terminalini oku. Hasar>%65 ise kaybı başarı sayma; HASARI İNCELE düğmesi sunulduğunda tıkla.
9. İncelemede WASD ile en az.5m yürü, hasar etiketini ve önlemin kalmasını gör; su/gıda artık0olmalı, ödül/puan0 kalmalı.
10. TEKRAR DENEye tıkla; aynı afet,90sn,1000bütçe,1su/1gıda, taze stok ve sıfır hasar dönüşünü kaydet.
11. Esc-BAŞLIĞA DÖN ile menüye git; skor dosyasının byte hashını karşılaştır.

### Beklenen sonuç

- Seçilen afetin tek gerçek çözümü vardır; başarılı inceleme veya kayıp sonucu kurallarla açıklanır.
- Ordinary loss incelemesi kayıp nedenini/hasarı korur; tekrar yeni bağımsız koşudur. Skor/ödül ve otomatik sonraki dalga yoktur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Başlangıç, iş bitişi, afet başlatma, doğal terminal, yürünebilir inceleme ve tekrar kareleri; gerçek delta/süre.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:7
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:18
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:559
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:801
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1077
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:481
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:486
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:61
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530

## MEC-006 — Yangın simülasyonu: seçimden doğal sonuç, inceleme ve aynı afetle tekrar

**Alan:** Kesintisiz bağımsız afet · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Bağımsız simülasyon / Yangın

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Yeni başlık; tüm geliştirici seçenekleri kapalı; ayrı GUID;1su/1gıda başlangıçtır.

### Adımlar

1. Başlıkta FELAKET SİMÜLASYONU OYNAya tıkla; Yangın kartını fareyle seç.90sn ve sonlu hazırlık stokunu kaydet.
2. WASD ile ön kapıya yürü; E ile aç. Gerçek controller ve kat geometrisinden geçerek hedefe yaklaş; kamera yönüne göre tuşları çevir.
3. Dedicated alt kat yangın istasyonuna yürü;üstteki mobilyaya tüp uygulama. Gerçek envanter tüpünü görünür uygun eylemle istasyonda kur.
4. 0.60sn dünya kurulumunu izle;A/D hizalama veyaSpace tutma gerekmemeli.1tüp/75bütçe tek kez düşer;gerçek merdivenden üst kata çıkıp geri dönerek hareketi kontrol et.
5. Tab ile envanter/çanta aç; başlangıç1su/1gıdanın ve işte harcanan stokların gerçek değerlerini kaydet, Tab ile kapat.
6. Fareyi AFETİ BAŞLAT düğmesine götür ve tek tıkla. Çift tıklamanın ikinci afet yaratmamasını kaydet.
7. Enter/Atla kullanmadan8sn afet+4sn aftermathı doğal izle; loss varsa1.35sn bildirim ve6sn kayıp sinematiği bitmeden hareket deneme.
8. Sonucun gerçek terminalini oku. Hasar>%65 ise kaybı başarı sayma; HASARI İNCELE düğmesi sunulduğunda tıkla.
9. İncelemede WASD ile en az.5m yürü, hasar etiketini ve önlemin kalmasını gör; su/gıda artık0olmalı, ödül/puan0 kalmalı.
10. TEKRAR DENEye tıkla; aynı afet,90sn,1000bütçe,1su/1gıda, taze stok ve sıfır hasar dönüşünü kaydet.
11. Esc-BAŞLIĞA DÖN ile menüye git; skor dosyasının byte hashını karşılaştır.

### Beklenen sonuç

- Seçilen afetin tek gerçek çözümü vardır; başarılı inceleme veya kayıp sonucu kurallarla açıklanır.
- Ordinary loss incelemesi kayıp nedenini/hasarı korur; tekrar yeni bağımsız koşudur. Skor/ödül ve otomatik sonraki dalga yoktur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Başlangıç, iş bitişi, afet başlatma, doğal terminal, yürünebilir inceleme ve tekrar kareleri; gerçek delta/süre.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:7
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:18
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:559
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:801
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1077
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:481
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:475
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:916

## MEC-007 — Fırtına simülasyonu: seçimden doğal sonuç, inceleme ve aynı afetle tekrar

**Alan:** Kesintisiz bağımsız afet · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Bağımsız simülasyon / Fırtına

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Yeni başlık; tüm geliştirici seçenekleri kapalı; ayrı GUID;1su/1gıda başlangıçtır.

### Adımlar

1. Başlıkta FELAKET SİMÜLASYONU OYNAya tıkla; Fırtına kartını fareyle seç.90sn ve sonlu hazırlık stokunu kaydet.
2. WASD ile ön kapıya yürü; E ile aç. Gerçek controller ve kat geometrisinden geçerek hedefe yaklaş; kamera yönüne göre tuşları çevir.
3. Çatıyı erişilebilir hedef noktasında Q ile seç;3 ile uygun yapı güçlendirmesini başlat.0.60sn dünya montajında gerçek tier geometrisi ve karakter yapım pozunu izle;alakasız workbench açılmamalı.
4. Ön kapıyı seç; kalan tahta/vida yeterli değilse gerçek dolap/bodrum kaynaklarını E-Z/X-F ile ara. Yeni malzeme edinmeden ikinci yükseltme olmuş sayma.
5. Uygun bir mobilyayı yalnız kalan sonlu malzemeyle sabitle veya gerçek eksik kartını kaydet; bu rotada yüksek hasarlı kayıp kabul edilebilir terminaldir.
6. Tab ile envanter/çanta aç; başlangıç1su/1gıdanın ve işte harcanan stokların gerçek değerlerini kaydet, Tab ile kapat.
7. Fareyi AFETİ BAŞLAT düğmesine götür ve tek tıkla. Çift tıklamanın ikinci afet yaratmamasını kaydet.
8. Enter/Atla kullanmadan8sn afet+4sn aftermathı doğal izle; loss varsa1.35sn bildirim ve6sn kayıp sinematiği bitmeden hareket deneme.
9. Sonucun gerçek terminalini oku. Hasar>%65 ise kaybı başarı sayma; HASARI İNCELE düğmesi sunulduğunda tıkla.
10. İncelemede WASD ile en az.5m yürü, hasar etiketini ve önlemin kalmasını gör; su/gıda artık0olmalı, ödül/puan0 kalmalı.
11. TEKRAR DENEye tıkla; aynı afet,90sn,1000bütçe,1su/1gıda, taze stok ve sıfır hasar dönüşünü kaydet.
12. Esc-BAŞLIĞA DÖN ile menüye git; skor dosyasının byte hashını karşılaştır.

### Beklenen sonuç

- Seçilen afetin tek gerçek çözümü vardır; başarılı inceleme veya kayıp sonucu kurallarla açıklanır.
- Ordinary loss incelemesi kayıp nedenini/hasarı korur; tekrar yeni bağımsız koşudur. Skor/ödül ve otomatik sonraki dalga yoktur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Başlangıç, iş bitişi, afet başlatma, doğal terminal, yürünebilir inceleme ve tekrar kareleri; gerçek delta/süre.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:7
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:18
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:559
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:801
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1077
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:481
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:921
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:538
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:536

## MEC-008 — Normal1dalga: evden toplama, önlem, bunker, ödül ve2dalga

**Alan:** Kesintisiz kampanya · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Serbest oyun / normal kampanya

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Önceden SOURCE/MODELde maliyet ve beklenen hasarı hesaplanmış sonlu, hilesiz1dalga hazırlık rotası.
- Yeni GUID, geliştirici kapalı; kesintisiz kayıt menüden başlar.

### Adımlar

1. SERBEST OYUNa fareyle tıkla; Wave1/Deprem/300sn/1000bütçe ve boş çantayı kaydet.
2. Ön kapıya WASD ile yürü ve E ile aç; buzdolabına yürü, E ile aç. Z/X-F ile gerçek su ve gıda yığınlarını al.
3. Tab-ok-Enter ile her birinden1gün çantala; görsel çanta ve model sayaçlarını ekranla kaydet, Tab kapat.
4. Garaja yürüyerek git, kapısını E ile aç; alet kutusunu E ve Z/X-F ile kullan, braket/vida/tüp stokunu gör.
5. Buzdolabına geri dön;uygun ankraj yüzeyini seçip1e bas. İki gerçek vida hedefini sırasıyla tıkla;her0.60sn oturma bittiğinde ilerle. Toplam60bütçe ve1braket/2vida yalnız finalde düşer.
6. Maliyet planındaki ikinci savunmayı yalnız gerçek stok ve bakiye yeterliyse yap; iptal edilmiş denemeye masraf yazma.
7. Su/gıdayı son kez Tab ile doğrula; tüm panelleri kapat.Ön kapıdan dışarı yürü, kapıyı arkadan E ile kapat.
8. Bunker kapağına normal zeminden yürü; yakın hedef E ile gerçek EnterBunker yolunu başlat.
9. Enter kullanmadan8afet+4aftermath+3başarı geçişini izle; su/gıda birer gün azalsın.
10. Hasar≤65veçanta yeterliyse LastReward/LastWaveScore, bütçe ve tamamlanan dalgayı bir kere kaydet; beklenen hesapla karşılaştır.
11. Hasar incelemesi açılınca WASD ile hasarlı eşyaya yürü; hareket ve kapı etkileşimi geri dönsün, hazırlık işi yapılamasın.
12. N ile bir kez sonraki dalgaya geç; Wave2deprem,300sn, korunmuş ankraj ve tüketilmiş su/gıda/yenilenmiş sarf kaynaklarını gör.
13. Esc-BAŞLIĞA DÖNle koşuyu bitir; yerel tabloda1koşu kaydı olsun.Başlıkta tekrar SERBEST OYUNa gir; yeni koşu1000ve temiz evle başlamalı.

### Beklenen sonuç

- Hilesiz gerçek hazırlık ve bunker rotası en az bir başarı/ödül/inceleme/yeni dalgayı tamamlar; ekonomik fark hesapla eşleşir.
- Doğal kayıp oluşursa terminal dürüst kaydedilir ve bu olumlu rota tamamlandı sayılmaz; sebep kural/rota/ürün kusuru olarak kanıtla ayrılır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:181
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:683
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:801
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1059
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:631
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:178

## MEC-009 — Wave1den en azWave12ye gerçek ekonomi ve kalıcı önlem yolculuğu

**Alan:** Kesintisiz uzun kampanya · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Serbest oyun / uzun kampanya

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Hile/fixture/zaman enjeksiyonu yok; gerçek kaynaklardan1–12dalga için uygulanabilir bütçe/malzeme rotası salt kaynak/model aşamasında hesaplanır.
- En az60dk kayıt alanı ve saat; insan mola verirse pause süreleri ayrı kaydedilir; aynı koşu restart edilmez.

### Adımlar

1. SERBEST OYUNla başla; başlangıç dosya hashı ve Wave1bakiye/çanta/ev durumunu al.
2. MEC-008gibi gerçek E-F toplama,Tab-Enter çantalama,iki vida tıklama/doğrudan0.60sn kurulum ve fiziksel bunker girişiyle1dalga tamamla.
3. Wave2ve3te yenilenebilir su/gıda/sarfı dolaplardan yeniden topla; mevcut ankrajı/bağlantıyı ve tüketilmeyen destek ekipmanını kontrol et.
4. Her başarıda Nyi yalnız bir kez kullan; dalga artışı1, hasar temizliği ve tek ödül muhasebesini satır satır tut.
5. Wave3sonunda2gün ihtiyaca hazırlık için ekstra su/gıdayı gerçek kaynaktan edin;4dalga2su+2gıda ve270sn sınırını gör.
6. Wave4–6selde gerçek stok/ödenekle drenaj,bariyer ve bölgesel yetişkin ağaç sırası/çit kur. Her uygun iş0.60sn dünya kurulumu kullanır;üç ağaç bölgesini aşma;fidan/sulama işi uydurma.
7. Her bunkera giriş öncesi çanta gününü ve kalan süreyi kaydet; su/gıda tüketimi ihtiyaca eşit olsun, destekler korunsun.
8. Wave6dan7ye3gün/240snveYangın geçişini gör; gerekli3su/3gıdayı Enterla tek tek çantala;3günüstünü deneme yalnız ayrı vakada yapılır.
9. Wave7–9yangında gerçek sonlu tüp/hortum ve yapı önlemlerini kullan; stok ve global koruma tavanını anlık muhasebele.
10. Wave10a ulaş; artık üç temel afet arasındaki seçimleri kaydet. Fırtınanın sıraya otomatik eklendiğini varsayma.
11. Wave10–12de aynı afetin üç ardışık seçilmediğini ve3gün/240sn korunmasını gör; her doğal sonucu ve bütçe artışını kaydet.
12. Çift ödül, tekrarlanan sabit bahçe işi, yenilenmemesi gereken yapı stoğunun yenilenmesi ve önceki hasarın sonraki dalgaya kalması için her geçişte karşılaştır.
13. Gerçek kayıp oluşursa neden/sonuç/sıfır ödülü kaydet; kaybı düzeltmek için F2açma, kaydı yeniden başlatma veya hasarı değiştirme.
14. 12+dalga tamamlandıysa incelemede serbest hareket et; menüye dönerek tek koşu kaydını kalıcılaştır; uygulamayı yalnız root izinli izole örnekte normal kapatıp yeniden aç, tabloyu denetle.

### Beklenen sonuç

- Gerçek hilesiz uzun koşu dalga4/7/10sınırlarını ve12+dalga durumunu kapsar; kısa49.67sn rota veya model fixture bunun yerine geçmez.
- Doğal kayıp oyun kuralıdır; hedefe ulaşılamayan rota tam kampanya PASS değildir. Kusur yalnız beklenen kural ile gözlenen veri uyuşmazlığıyla açılır.
- Kalıcı savunma, sonlu yapı malzemesi, yenilenen sarf,3gün tavanı ve tek skor muhasebesi korunur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Her dalga için afet/RequiredDays/süre/önlem/çanta/LooseInventory/bütçe/hasar/ödül tablosu; aralıksız video ve skor dosyası önce-sonra hashı.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:683
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:702
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1059
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1406
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:178
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:617

## MEC-010 — Bunkera yetişmeme, kayıp bildirimi ve tamamen temiz restart

**Alan:** Kesintisiz olumsuz kampanya · **Öncelik:** P0 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Serbest oyun / zaman kaybı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Yeni normal koşu, hile kapalı; doğal300sn bekleme kayıt kapsamındadır.

### Adımlar

1. SERBEST OYUNla başla; kapıya yürü,Eaç; buzdolabından E-File su al,Tab-Enterla çantala.
2. Garajdan malzeme al ve bir gerçek sabitleme tamamla;önce/sonra bütçe ve eşyayı kaydet.
3. Bunker dışındaki geçerli zeminde bekle; son60snuyarısını ve geriye sayımı gör.
4. Son saniyede bunkerle uzaktan Eyi dene; uzaktan giriş başarılı sayılmamalı.
5. Süre0olduğunda kayıp açıklamasının sinematikten önce1.35sn görünmesini kaydet.
6. Kayıp sahnesini doğal izle; son ekran sabit kalsın, W/A/S/D/E/F/Space ile hazırlığa geri dönmesin.
7. TEKRAR DENEyi kullan; yeni normal koşuWave1/300sn/1000veboş çanta olmalı.
8. Eski ankraj,hasar,tüketim,kapak açık durumu,çalışma rigı,taşıma hayaleti ve geliştirici bayrağı kalmadığını gör.
9. Kapıya W-Eile gidip buzdolabını E-File yeniden kullan; fiziksel hareket/etkileşim kilidi çözülmüş olsun.
10. Menüye dön; koşu kaydı teklenir, hayali başarılı dalga/ödül yoktur.

### Beklenen sonuç

- Süre kaybı çanta veya hasar kaybı diye etiketlenmez; sıfır yeni ödül ve sabit GameOver vardır.
- Restart bütün koşu durumunu sıfırlar; uygulama veya gerçek kullanıcı playerı zorla kapatılmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:245
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1109
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs:143
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:631
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:223

## MEC-011 — Öğreticide topla/iş iptal et/afet gör ve gerçek koşuya temiz geç

**Alan:** Kesintisiz öğretici · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Nasıl oynanır / öğretici → kampanya

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Hile kapalı; öğreticide180sn göstergesinin normal kampanya saati olmadığı bilinir.

### Adımlar

1. Başlıkta NASIL OYNANIRı seç; yardım metnini oku ve Escile yalnız yardım katmanını kapat.
2. WASDile ev içine yürü; kapıları Eile aç; dolapta E-Fayrımını kullan.
3. Tabile envanteri aç; oklar,EnterveFüretim tuşlarının bağlamını göster; boş/eksik giriş retlerini kaydet.
4. Uygun yüksek eşyada1 ile ankraj aç;ilk gerçek vidaya tıkla ve oturma sırasında Backspace ile iptal et;gider olmamalı.
5. Aynı işi yeniden başlat;iki gerçek vidayı sırayla tıklayıp her birinin0.60sn oturmasını bekle;yalnız bu final gider doğursun.
6. Bir eşyada4ile taşıma aç, fareyle geçerli yere götür,Rile döndür ve Escile iptal et.
7. İlk öğreticiye ayrılan180sngeçse de kendi kendine süre kaybı olmadığını kaydet; bu öğreticiye özgü kuraldır.
8. Bunker yolunu fiziksel yürü,Eile afet denemesini başlat; doğal sonucu ve incelemeyi gör.
9. N/Devamla öğreticiye dönebildiğini gör; ÖĞRETİCİYİ BİTİR·Neyalnız boş dünya bağlamında bas.
10. Gerçek Wave1/1000bütçe/temiz stok/boş çanta/temiz savunma başlangıcını kaydet.
11. Tekrar E-FveTab-Enterla küçük bir görev yap; öğretici kilidi veya eski gider kalmadığını doğrula; menüye dön.

### Beklenen sonuç

- Öğretici para/önlem/skor gerçek koşuya taşınmaz;180snöğretici kendi kendine bitmez.
- Niş/taşıma/envanterde öğreticiyi bitirmez; gerçek bağlamda tek başlangıç oluşturur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:181
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:230
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:293
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:753
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:1297
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:638

## MEC-012 — F2 Kaybetmeme ile gerçek afet, ödülsüz inceleme ve temiz hilesiz restart

**Alan:** Kesintisiz geliştirici · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / Serbest oyun / geliştirici koruması

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.
- Rota hızı yeni3.645/5.67 değerleriyle ölçülür. Eski A/D mini oyun protokolü veya keyfi mobilya fire hedefi bu adayda kullanılmaz. F0da istasyon/bölge/API kimlikleri ve güncel maliyetler final snapshotla bağlanır.
- Ayrı GUID; F2seçenekleri başlangıçta kapalı; bu koşu skora yazılmayacaktır.

### Adımlar

1. SERBEST OYUNa gir;WASDile birkaç adım yürü;F2aç ve seçenek değiştirmeden kapat. Menü açma tek başına HİLEbayrağı yapmamalı.
2. Yeniden F2aç;Kaybetmeme satırına oklarla gidip Enterla aç;diğer üç seçeneği kapalı bırak.
3. F2ile kapat;bu kapanış karesinde basılıWveEnin dünyada ek eylem üretmediğini kaydet.
4. Çantayı eksik bırak;buzdolabına git Eile aç,File su al ama çantaya koyma.
5. Hazırlık sayacının doğal300snbitişini bekle;Freezeaçma,zaman enjeksiyonu kullanma.
6. Süre bittiğinde afetin8snveaftermathın4sndoğal çözümünü izle;gerçek hasar ve eldeki erzak tüketimi değişmesin.
7. GameOveryerine geliştirici açıklamalı inceleme açıldığını,ödül/LastWaveScore=0veCompletedWavesartmadığını kaydet.
8. İncelemede WASDile hareket et;F2aç ve Kaybetmemeyi kapat;CheatedRunuyarısı aynı koşuda kalmalı.
9. Nile sonraki dalgaya geç;bu dalga normal kayba açık olmalı ama koşu skor dışı kalmalı.
10. Esc-BAŞLIĞA DÖN;skor dosyası başlangıç hashıyla eşleşmeli.
11. Tüm seçenekler kapalıyken SERBEST OYUNa yeniden gir;temiz koşuda hile işareti kalmamalı.
12. Bu kez boş çantayla fiziksel bunkeraEile gir;normal kayıp gelsin,Kaybetmemekapalıyken ödülsüz koruma incelemesi açılmamalı.
13. Kayıp/menü dönüşünü bitir;kayıtlarıniki koşuyu karıştırmadığını kaydet.

### Beklenen sonuç

- Kaybetmeme gerçek hasar/erzak tüketimini korur;başarı veya ödül üretmez;hile işareti o koşuda yapışkandır.
- Panel açmak masumdur;seçenek kullanmak skor dışıdır;kapalı yeni koşu normal kayıp kurallarını geri getirir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.
- Bu rota DirectActions adayına uyarlanmıştır;retained V4.8de yürütülecekse eski input/station/speed eşlemesi ayrı varyant olarak yazılır.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:207
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1087
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1109
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:132
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:1044
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:325

## MEC-013 — Teslim DLLsi ile canlı dört dosyanın F0 ayrımı

**Alan:** Kimlik ve kaynak · **Öncelik:** P0 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Tüm modlar

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. LATEST-BUILD ve V4.8 source-build-v3 içindeki61kaynak SHA256ını karşılaştır.
2. World,Life,Hud,ResidenceBuild farklarını ayrı diff ve snapshotta koru;aynı DLLden gelmiş sayma.
3. Yürütülecek retainedV4.8 veya yeni aday kimliğini F0receiptinde açık seç.

### Beklenen sonuç

- Eski kabul yalnız5683E713...DLLsine aittir;drift yeni kusur veya kabul iddiası olmadan görünürdür.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Satır referanslı salt okunur inceleme, snapshot SHA256 ve karşılaştırma raporu; çalıştırma/PASS iddiası yok.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/manager/source-build-v3/source-manifest.json
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:325
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:40
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:423

## MEC-014 — Başlık çift Enter ve mod başlangıcının teklenmesi

**Alan:** Başlatma ve giriş · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kısa / simülasyon / kampanya ayrı koşular

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Her modu ayrı GUIDde başlıktan iki yakın Enter basışıyla aç.
2. Başlangıç bütçesi,çanta ve mod başlığını kaydet.
3. Menüye dönüp başka modu başlat.

### Beklenen sonuç

- Tek koşu başlar;modlar stok,hedef,kamera veya sonuç devralmaz;başlangıç girişi dünyada tekrar tüketilmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:423
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:181
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:559
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:580

## MEC-015 — Dört simülasyon kartı ok/Enter/fare ve geri seçimi

**Alan:** Başlatma ve giriş · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Bağımsız dört simülasyon

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Seçiciyi aç;oklarla dört kartı dolaş,Backspace ve Esc ile ayrı denemelerde geri çık.
2. Yeniden açıp her afeti Enterla ayrı başlat.
3. Fareyle kart seçimini de ayrı koşulda dene.

### Beklenen sonuç

- Dört seçim wrap yapar;iptal yeni koşu açmaz;gösterilen afet ile başlayan model eşleşir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicSimulationHud.cs:9
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:559

## MEC-016 — Aynı kare E+F dünya eylemi en fazla bir kez

**Alan:** Girdi sahipliği · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Gerçek kaynak yakınına kontrollü kurulum yap;fixture ve Warpı etiketle.
2. DispatchWorldInputa E,F,Tab aynı listede ve primaryClick=true ver.
3. Sonraki karelerde F ve Tabı ayrı gönder.

### Beklenen sonuç

- İlk karede yalnız bir ana eylem işlenir;kapak açma ve pickup aynı karede çift uygulanmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:428
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:1285

## MEC-017 — Kapalı veya uzaktaki kaynakta F toplamaz

**Alan:** Girdi sahipliği · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Kapalı buzdolabı yakınına yürü ve F bas.
2. E ile aç;2.4m erişim sınırını geçerek uzaklaş ve F bas.
3. Yeniden yaklaş ve F bas.

### Beklenen sonuç

- Kapalı/uzak durumda envanter,kaynak ve bütçe değişmez;yalnız geçerli açık kaynakta tek yığın alınır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:33
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:22
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:170

## MEC-018 — Q adayları ve seçili hedef göstergesi

**Alan:** Hedefleme · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / kısa

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. İki erişilebilir nesne yakınına yürü;Q ile birkaç tur dolaş.
2. Seçim vurgusuyla E/1/2 eylemlerinin hedefini eşleştir.
3. Uzaklaşınca odak yenilenmesini ve geçersiz hedef reddini gözle.

### Beklenen sonuç

- Yalnız kurulu,uygun kat/mesafe/açık yol adayları dolaşır;vurgulanan hedef ve eylem hedefi aynıdır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:440
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:483
- TooLateToPanic/Assets/Game/Runtime/PanicRefinement.cs:9

## MEC-019 — Kat sınırı ve başka kattaki dolaba erişim reddi

**Alan:** Hedefleme · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Alt ve üst katta aynıXZ çevresine gerçek merdivenle çık.
2. Y farkı.9 eşik çevresinde odak ve CanInteractı gözle.
3. Diğer kattaki kaynağa F veya1 denemesi yap.

### Beklenen sonuç

- SameLevel,gerçek mesafe ve ışın engeli uygulanır;başka kattaki loot uzaktan alınmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:33
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:440
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:455
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:458
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:39

## MEC-020 — Kapalı duvar/kapı arkasına E/F/iş reddi

**Alan:** Hedefleme · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / kısa

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Banyo batı duvarı dışından ecza dolabına Q/E/F dene.
2. Güney kapısından E ile girip dolaba yaklaş.
3. E ile aç ve F ile gerçek malzemeyi al.

### Beklenen sonuç

- Duvar arkasından toplama veya iş başlatma olmaz;güney tek fiziksel giriş korunur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:458
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:553
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:40

## MEC-021 — WASD/ok eşdeğerliği, diyagonal ve Shift hız sınırı

**Alan:** Hareket · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. Açık alanda aynı süre W,UpArrow,W+D veShiftW rotalarını kaydet.
2. Kamera yönünü değiştirip aynı girişleri tekrarla.
3. Ters yönlerin birlikte tutulmasını ayrı dene.

### Beklenen sonuç

- YalnızV4.8 için2.7/4.2 hedef hızları ve diyagonal normalizasyon beklenir;V4.9 yeni hız ayrı MEC-113tedir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:379
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:381

## MEC-022 — Space zıplama ve meşgul bağlam korumaları

**Alan:** Hareket · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / kısa

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Zeminde Space bas;havada ikinci Space ve.3sn cooldown öncesi basışı dene.
2. Envanter,iş,taşıma ve sinematikte Space bas.
3. Normal zemine dönüp yeniden zıpla.

### Beklenen sonuç

- Tek zemin zıplaması vardır;havada ve meşgul bağlamda yanlış zıplama olmaz;iş girdisi iki kez işlenmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:166
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:572
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:112

## MEC-023 — E kapak aç/kapat; boş mobilyada gizli iş başlamaz

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Kaynağı E ile aç;envanter ve içerik sayısını kaydet.
2. E ile kapat;ayrı boş sandalyede E bas.
3. Yalnız açık kaynakta ayrı F basışıyla malzemeyi al.

### Beklenen sonuç

- E malzeme taşımaz ve boş mobilyada gizli ankraj başlatmaz;F yalnız seçili gerçek yığını alır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:495
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:167
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:170

## MEC-024 — Z/X wrap ve boş yuvayı atlayarak kesin seçili yığın

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Üç içerikli dolabı aç;Z/X ile ileri ve geri dolaş.
2. Ortadaki yığını F ile al;yeniden Z/X ile boş yuvayı atla.
3. İkon/adet ile alınan gerçek Idyi karşılaştır.

### Beklenen sonuç

- Seçim wrap yapar;boş yuvalar atlanır;bir F yalnız seçili dolu yığını alır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:169
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:170
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:243

## MEC-025 — Gerçek pickup nesnesine fareyle seçme ve toplama

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Açık dolapta seçili olmayan gerçek pickup nesnesine tıkla.
2. Seçim ve ölçek değişimini kaydet;aynı nesneye yeniden tıkla.
3. HUD üstündeki tıklamanın loot almadığını dene.

### Beklenen sonuç

- PickMouseun final snapshot davranışıyla gerçek içerik indeksi eşleşir;UI tıklaması dünyaya sızmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- İlk tıklama/ikinci tıklama davranışı kaynak snapshotındaki PickMouse satırlarıyla ayrıca doğrulanır; görünüşten toplama sonucu çıkarılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:648
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:180
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:1285

## MEC-026 — Dolu envanterde bütün yığın toplama atomik reddi

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / envanter fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. 12yuvayı izole fixtureda doldur;aynıId birleştirmesinde yer yetersiz olsun.
2. Açık kaynaktan2lik yığını GatherAt ile almayı dene;sonra yeterli tek yuva açıp tekrarla.
3. Kaynak ve envanter toplamlarını karşılaştır.

### Beklenen sonuç

- Tam yığın sığmazsa hiçbir kısmı alınmaz;başarıda yığın tek kez aktarılır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:243
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:628

## MEC-027 — 12 yuva, beşli yığın ve taşan yeni yuva eşdeğerlikleri

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / kapasite fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Boş,11dolu ve12dolu envanter varyantlarında water4+1,4+2 ve5+1 dene.
2. GatherAt sonucu ve slot dağılımını kaydet.
3. Countın çanta içeriğini dahil etmediğini ölç.

### Beklenen sonuç

- 12loose yuva vardır;normal yığın≤5;aktarılacak tam yığın tek uygun yuvaya sığmalıdır;Bag ve Count ayrıdır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:68
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:353
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:622

## MEC-028 — Tekli destek/alet yığınları taşma ve korunma

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / envanter fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Fener,radyo,battaniye,ilk yardım,hortum,tüp için1ve2adet GatherAt fixtureı kur.
2. Dolu ve boş yuva varyantlarını ayır.
3. Başarısız aktarımda kaynak içeriğini karşılaştır.

### Beklenen sonuç

- Tekli Idler bir yuvada en fazla1olur;2lik fixture reddi gerçek normal kaynaktan alınan tekli yığına genellenmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:622
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:628

## MEC-029 — Sekiz çanta yuvası kapasitesi ve reddin atomikliği

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / çanta fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Bag8yuvayı doldur;packable yeni bir Idyi loose envantere koy.
2. PackSlot dene;sonra uygun yuva açıp aynı işlemi tekrarla.
3. Model maskesi,stock,loose veBag farkını karşılaştır.

### Beklenen sonuç

- Dolu redde stok,pil,mask veyaBag değişmez;yer açılınca tek birim aktarılır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:271
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:628

## MEC-030 — Üç günlük su/gıda tavanı ve geri alma

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / gün sınırı fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. PackWater/PackFood vePackSlot ile2→3 ve3→4 kollarını ayrı kur.
2. Üç gün dolu çantaya dördüncü Enter eşdeğerini dene.
3. Başarısız PackSlotta geçici stock artışının geri alındığını gör.

### Beklenen sonuç

- Üç gün başarır,dört reddolur;başarısızlıkta bütün envanter/Bag/model stoğu korunur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:416
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:427
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:303

## MEC-031 — Boş envanter/yanlış indeks/sıfır stokta paketleme retleri

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / negatif fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. PackSlot−1,12,boşCount0 ve boşId yuvaları dene.
2. Stock0 iken PackWater/PackFood çağır.
3. Geçerli tek birim kontrolünü ayrı çalıştır.

### Beklenen sonuç

- Geçersiz girişler false döner;negatif stok,gün veya hayali destek oluşmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:271
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:416
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:427

## MEC-032 — Fener ve radyo için pil gerekli ve bir kez tüketilir

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Kitaplığı E ile aç;Z/X-F ile yalnız feneri al,pili alma.
2. Tabda Enter ile paketlemeyi dene;pil1eksik kartını gör.
3. Gerçek pili alıp tekrar paketle;tek pil giderini kaydet.

### Beklenen sonuç

- Pilsiz ret atomiktir;pil geldiğinde bir destek ve bir pil gerçek maliyetle aktarılır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:286
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:346
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530

## MEC-033 — Aynı destek kategorisini ikinci kez paketleme reddi

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / destek fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Her0–3 destek Idsi için ilk PackSlotu yap.
2. Aynı Id ve yeterli pil ile ikinci kez dene.
3. Bag,mask,loose ve pil durumunu karşılaştır.

### Beklenen sonuç

- Kategori bir kez paketlenir;ikinci istekte pil/envanter harcanmaz;SupportCount4ü aşmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:298
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:438
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:452

## MEC-034 — Hazırlık malzemesini çantaya koyma reddi

**Alan:** Loot ve çanta · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Vida,braket,tahta,kum torbası,fidan/row kit,hortum,tüp ve pili gerçek kaynaktan edin.
2. Tabda her birini Enter ile çantalamayı dene.
3. Sonra gerçek su veya gıdayı Enter ile çantala.

### Beklenen sonuç

- Hazırlık malzemesi loose kalır;ret Bag/bütçe değiştirmez;su/gıda paketleme çalışır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:289
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530

## MEC-035 — İlk yardım tarifi ve tüketim sonrası boşalan yuva

**Alan:** Üretim · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / üretim fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Bandaj1/2/3 ve antiseptik0/1 tarif sınırlarında CraftFirstAid dene.
2. 12dolu envanterde tüketimle yuva boşalan ve hiç boşalmayan iki kol kur.
3. Önce/sonra stok ve ürünü karşılaştır.

### Beklenen sonuç

- Yalnız2bandaj+1antiseptik ile1ilk yardım atomik üretilir;ürün sığmazsa malzemeler korunur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:355
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:547
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:723

## MEC-036 — Taşınan kaynak mobilyanın içerik/koordinat/kat tutarlılığı

**Alan:** Loot ve yerleşim · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Buzdolabını aç,bir yığını F ile al ve kapa;üretim placement APIleriyle aynı katta geçerli yere taşı.
2. Eski konumdan E/F dene;yeni konuma gerçek controllerla yürü,E-F dene.
3. Kalan içerik,source Position veFlooru karşılaştır.

### Beklenen sonuç

- Kaynak mobilyayla birlikte taşınır;loot çoğalmaz veya eski yerde kalmaz;bu API rota OS kabulü değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:168
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:479
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:663

## MEC-037 — Yeni dalgada yalnız yenilenebilir kaynaklar dolar

**Alan:** Kalıcı durum · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / dalga fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Yenilenebilir beş sarf ve yapı malzemelerinin bir kısmını topla.
2. Yeni dalga OnNewWave yolunu uygula;başlangıç blueprintle karşılaştır.
3. Çağrıyı tekrarlayıp üstüne yığma olmadığını gör.

### Beklenen sonuç

- Yalnız su,gıda,bandaj,antiseptik,pil başlangıç sayısına döner;yapı malzemeleri ve tüpler yenilenmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:178
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:617
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:332

## MEC-038 — Doğal afet ve atlamada çanta yuvaları modelle aynı azalır

**Alan:** Tüketim · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / dört simülasyon ayrı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. PackedWater/Food>RequiredDays fixtureını açık etiketle.
2. Bir koşuda doğal afet,diğerinde üretim SkipResidenceCinematic yolunu izle.
3. Packed sayaçlarla Bag yuvalarını observer sonrası karşılaştır.

### Beklenen sonuç

- RequiredDays bir kez düşer;sıfır Bag yuvası temizlenir;atlama ve observer aynı tüketimi tekrar uygulamaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:994
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:166
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:333
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs:48

## MEC-039 — Tek tarif kaynağı: başlangıç, tamamlama ve eksik kartı

**Alan:** İş ekonomisi · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Tüm hazırlık işleri

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. MaterialCosts, WorkCost, CanBeginWork, TryCompleteWork ve HUD veri yolunu karşılaştır.
2. Her action/target için gereken, sahip ve eksik sayılarının aynı loose envanterden geldiğini izle.
3. Aday DirectActions tarif değişirse F0 karar manifestinde eski/yeni tabloyu ayrı kaydet.

### Beklenen sonuç

- HUD tarifi ayrı kopyalamaz; maliyet bir tek tanımdan gelir. Eski model Buy/Upgrade doğrudan maliyetleri, Expedition iş tarifleriyle aynı sanılmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Satır referanslı salt okunur inceleme, snapshot SHA256 ve karşılaştırma raporu; çalıştırma/PASS iddiası yok.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:367
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:384
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:570

## MEC-040 — Bütçede maliyet eksi1 / tam maliyet / artı1 sınırları

**Alan:** İş ekonomisi · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / tarif matrisi

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Her action için güncel WorkCost değerini al; bütçeyi cost−1, cost, cost+1 olarak ayrı fixturelarda kur.
2. Yeterli gerçek tarif malzemesiyle başlangıç ve gerçek iş tamamlama protokolünü uygula.
3. Ret ve başarıdaki bütçe/loose/target farkını karşılaştır.

### Beklenen sonuç

- Eksik1 reddedilir; tam maliyet0 bakiyeyle başarır; artı1de1 kalır. Başlamada gider yok, tamamlamada tek gider vardır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:367
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:384
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:553

## MEC-041 — Malzemede kesin eksik adet ve tam tarif sınırı

**Alan:** İş ekonomisi · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / tarif matrisi

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Her tarif malzemesini sırayla required−1, required ve required+1 kur; diğerlerini yeterli tut.
2. CanBeginWork ve CurrentRequirements/MissingRequirementsı al.
3. Gerçek tamamlama protokolüyle sonuç ve tüketimi karşılaştır.

### Beklenen sonuç

- Eksik=max(0,gerek−var) olur; bir malzeme eksiği tüm işi reddeder; yeterli tarif tek sefer tam miktarda tüketilir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:781
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:804

## MEC-042 — İşe başlamak para veya stok harcamaz

**Alan:** İş ekonomisi · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / güncel iş protokolü

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Geçerli hedef ve tarifle işi üretim BeginWork yolundan başlat.
2. İlk kabul edilmiş hareket/tıklama sonrası son tamamlamadan önce bütçe ve stoku gözle.
3. Son aşamayı tamamla ve ertesi kare tekrar gözle.

### Beklenen sonuç

- Ön izleme ve kısmi işte bütçe/loose/target sabittir; tamamlanınca atomik tek commit olur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:553
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:586
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:384

## MEC-043 — İptalde bütün maliyetler ve hedef durumu korunur

**Alan:** İş ekonomisi · **Öncelik:** P0 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / her iş ailesi

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Ankraj, yükseltme, sel, yangın ve bahçe için geçerli maliyetli iş başlat.
2. Başlangıçta, ortada ve son committen bir kare önce ayrı denemelerde Backspace/Esc iptal et.
3. Aynı koşuda işi yeniden başlatıp gerçekten bitir.

### Beklenen sonuç

- İptal maliyet ve savunma yaratmaz; yeniden başlatma temiz ilerler; biten tek iş yalnız bir gider doğurur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:575
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:603
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:384

## MEC-044 — Çift CompleteWork ve geç gelen callback gideri tekrarlamaz

**Alan:** İş ekonomisi · **Öncelik:** P0 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / tamamlanmış iş fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Etiketli tamamlanmış oturum fixtureıyla TryCompleteWork çağır.
2. Aynı action/target ile ikinci çağrı ve Work.Cancel sonrası gecikmiş çağrı yap.
3. Bütçe, malzeme, Tier ve bahçe sayaçlarını karşılaştır.

### Beklenen sonuç

- Yalnız ilk commit etkili olur; oturum kimliği/Complete/target ve site tekrar kontrolü ikinciyi reddeder. Fixture mini oyun veya OS kabulü değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:384
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:425

## MEC-045 — Tamamlamada değişen hedef/stok/faz yeniden doğrulanır

**Alan:** İş ekonomisi · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / yarış fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. İşi geçerli başlat; tamamlamadan önce hedefi korumalı, stoku eksik veya fazı Aftermath yaparak ayrı fixturelar kur.
2. Üretim TryCompleteWorku gerçek oturum action/target ile çağır.
3. Ret durumunda önce/sonra bütün ekonomiyi karşılaştır.

### Beklenen sonuç

- Eski preflight sonucu yetmez; geçersiz tamamlanma hiçbir gider/koruma oluşturmaz. Dünyadaki faz observerı oturumu da temizler.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:384
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:438
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:333

## MEC-046 — İlk satın alma, çözme ve ücretsiz yeniden sabitleme

**Alan:** Ankraj · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / uygun yüksek mobilya

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. AnchorOwned=false uygun eşyanın ilk ankrajını gerçek iş protokolüyle tamamla.
2. ToggleAnchor ile çöz; AnchorOwned ve tarif tablosunu al.
3. Aynı eşya ve diğer yüzeyde yeniden sabitle; başka eşya ile karşılaştır.

### Beklenen sonuç

- İlk yetki mevcut tarif kadar gider oluşturur; çözme AnchorOwnedu korur; aynı eşyanın yeniden ankrajı bütçe/malzeme istemez. Yeni eşya ilk tarife tabidir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:325
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:534
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:557

## MEC-047 — Duvara yakınlık ve T ile zemin seçiminin farkı

**Alan:** Ankraj · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / uygun yüksek mobilya

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Taşınabilir uygun mobilyayı duvardan uzak geçerli zemine koy.
2. T göstergesi Duvar iken1 dene; sonra T ile Zemin seçip1 dene.
3. Duvar yakınına taşıyıp aynı kolları gerçek maliyetle karşılaştır.

### Beklenen sonuç

- Uzak eşya duvara sabitlenmez; zemin seçimi geçerliyse ayrı yüzey stateini kullanır. DirectActions uygunluk filtresi ayrıca geçerlidir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:535
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:540
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:551

## MEC-048 — T0→T1→T2 ve T2 üstünü reddetme

**Alan:** Yapı · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Yapı yükseltme / version koşullu

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Uygun yapı için T0/T1/T2 ve Installed=false fixtureları kur.
2. Her kademede güncel malzeme/bütçe tarifiyle gerçek tamamlama protokolünü uygula.
3. T2de tekrar ve kurulu olmayan pencereyi yükseltme denemesi yap.

### Beklenen sonuç

- Yalnız iki kademe vardır; T2 ve kurulu olmayan hedef retlerinde gider yoktur; kapı/pencere açıklığı sanat kabulünde ayrıca gözlenir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:462
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:538
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:559

## MEC-049 — Sabit alan ikinci commit ve sayaç üst sınırı

**Alan:** Bahçe · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / V4.8 legacy bahçe / adayda karar manifesti

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. Aynı plant/drain/fence/hose siteını iki kez bitirmeyi dene.
2. Farklı IDlerle Tree5/Drain4/Fence4/FireTool3 tavanlarının alt/tam/üst kollarını kur.
3. DirectActions üç bölge kuralını bu legacy tavan yerine ayrı uygula.

### Beklenen sonuç

- Legacy committedWork aynı siteı yinelemez; sayaç tavanı giderden önce kontrol edilir. Yeni üç bölge davranışının kabulü bu legacy vakadan çıkarılmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:433
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:483
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:494

## MEC-050 — Drenaj, sel çiti ve hortumun ayrı maliyet ve etkileri

**Alan:** Bahçe · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya hazırlık / site bazlı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Bodrum malzemelerine gerçek merdivenden eriş; E-Z/X-F ile yeterli tarifi topla.
2. Drenaj, çit, hortumu ayrı gerçek sitelerinde güncel protokolle kur.
3. Önlem görseli, dar geçişler, sayaç ve maliyet farklarını kaydet.

### Beklenen sonuç

- Siteler birbiri yerine tamamlanmaz; drenaj/çit sel katsayısını, hortum yangın hazırlığını etkiler; gerçek actor/kapı yolu korunur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:510
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:592
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:835

## MEC-051 — Pencere/kapı bariyeri ve diğer hedeflerde ret

**Alan:** Sel önlemi · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / ortak action kuralları

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Window,Door,Furniture,Roof ve Installed=false hedeflerle flood başlatmayı dene.
2. Uygun hedefte işi bitir; aynı hedefte tekrarla.
3. Bariyer statei, gider ve global koruma katkısını karşılaştır.

### Beklenen sonuç

- Yalnız kurulu pencere/kapı uygundur; ikinci kurulum ve uygunsuz tür retleri ekonomiyi korur. Aday shared rules daha daraltıyorsa karar manifesti esas alınır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:468
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:835

## MEC-052 — Eski keyfi fire-target davranışı yalnız legacy kaynak incelemesi

**Alan:** Yangın önlemi · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. V4.8 ValidateWorkTarget fire kolunu ve FireTool3 tavanını oku.
2. Item=null/unknown hedefin model katmanındaki etkisini, World erişim kapısından ayır.
3. Aday dedicated fire station sözleşmesiyle değişen sahipliği not et.

### Beklenen sonuç

- Legacy kaynak adayı gerçek oyuncu istismarı diye yazılmaz; aday V4.9da yatak/koltuğa tüp veya bilinmeyen hedef başarı beklentisi bulunmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Satır referanslı salt okunur inceleme, snapshot SHA256 ve karşılaştırma raporu; çalıştırma/PASS iddiası yok.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:475
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:553
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## MEC-053 — Normal Ratchet dört aşama ve üç dönüşümlü hareket

**Alan:** Legacy mini oyun · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. Yalnız retained V4.8 protokolüyle anchorwall Begin yap.
2. Her aşamada A/D hizala, Enter, üç istenen dönüşümlü yön ve Space gönder.
3. Dördüncü settle sonrası Complete ve toplam ilerlemeyi kaydet.

### Beklenen sonuç

- Dört aşama gerçekten tamamlanmadan commit yoktur; aday doğrudan tıklama için bu protokol artık kabul gereği değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:916
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:1020
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:1077

## MEC-054 — Kısa Ratchet iki aşama ve iki gerçek hareket

**Alan:** Legacy mini oyun · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. Begin(action,target,true) ve normal false oturumu ayrı kur.
2. İki doğru A/D hareketinden önce Space dene; ardından iki hareket ve Space yap.
3. Kısa2/normal4 aşama ve RequiredStrokes2/3 değerlerini karşılaştır.

### Beklenen sonuç

- Legacy kısa modun kolaylığı gerçek iki hareketle sağlanır; HUD kutu sayısı modele eşittir. Bu eski protokol V4.9a taşınmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:916
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:965
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs:40

## MEC-055 — Ratchet yanlış yön, tutma ve Space spam tamamlamaz

**Alan:** Legacy mini oyun · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. Hizalanmış legacy oturumda aynı yönü tut ve Space pulse spamı gönder.
2. Cooldown dolmadan zıt yön dene; sonra gerçek dönüşümlü giriş ver.
3. Strokes/CompletedSteps/bütçe değişimini kaydet.

### Beklenen sonuç

- Yanlış yön veya erken Space başarı üretmez; bekleme kendi kendine işi bitirmez; maliyet yalnız gerçek final committe olur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:1020
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:1077

## MEC-056 — Timing bant kenarı ve yanlış darbe

**Alan:** Legacy mini oyun · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. Timing action için hizala ve Enter gönder.
2. TargetWidth sınırında, hemen içinde ve dışında ayrı pulse fixtureları uygula.
3. Yanlış pulse sonrası düzeltip bitir.

### Beklenen sonuç

- Yalnız mevcut bant içi pulse aşamayı oturtur; ret ilerlemeyi tamamlamaz. Hassas dt fixtureı OS zamanlama kabulü değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:1098
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:955

## MEC-057 — Timing bekleme veya Space tutma otomatik başarı sayılmaz

**Alan:** Legacy mini oyun · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. Hizalanmış drain/timing oturumunda girdisiz gerçek dt ile birkaç tarama geçir.
2. Held=true fakat yeni pulse=false durumunu sürdür.
3. Yeni doğru pulse ile aşamayı ayrı bitir.

### Beklenen sonuç

- İlerleme beklemekten veya tuşu tutmaktan tamamlanmaz; yeni pulse gerekir. Aday direkte bu mekanik uygulanmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:1098

## MEC-058 — Balance bant içi tutma, drift ve yeniden toparlanma

**Alan:** Legacy mini oyun · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. fire/hose Balance Begin ve hizalama yap.
2. Space tutulurken bant dışına çıkar; ilerleme azalsın, sonra A/D ile düzelt.
3. Normal.95sn/kısa.65sn aktif bant tutma farkını kaydet.

### Beklenen sonuç

- Denge ve aktif tutma gerekir; yanlış basınç ilerlemeyi tamamlamaz. Dedicated station DirectActions adayında hold gereği yoktur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:916
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:1112

## MEC-059 — İş delta ve yön verisi geçersizliği

**Alan:** Legacy mini oyun · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS / V4.8_RETAINED_REGRESSION / NOT_APPLICABLE_ON_DIRECT_ACTIONS

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- Yalnız retained V4.8 veya o protokolün dondurulmuş kaynak incelemesi;V4.9 DirectActions için NOT_APPLICABLE_ON_DIRECT_ACTIONS. Sonuç yeni tasarıma hata/kabul olarak aktarılmaz.

### Adımlar

1. Aktif işte dt0/negatif/NaN/Infinity ve direction NaN/Infinity ver.
2. Büyük dt ile.1s üretim capini küçük adımlı referansla karşılaştır.
3. Cancel sonrası Tick gönder.

### Beklenen sonuç

- Geçersiz dt ilerleme yaratmaz; yön sonlu/−1..1 aralığına alınır; iptal oturumu dirilmez. Yeni direct click Tick protokolü ayrı vaka ister.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:1001
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:1165

## MEC-060 — 4 ve sağ tıkla başlama, sabit eşyada ret

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / taşıma

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Yakın ve taşınabilir eşyada4 ile, ayrı koşuda sağ tıkla taşıma başlat.
2. Sabitlenmiş, kurulu olmayan, yapısal ve uzaktaki hedefte tekrarla.
3. Envanter/iş açıkken başlatmayı dene.

### Beklenen sonuç

- Yalnız uygun yakın serbest mobilya taşınır; gider yoktur; modal sahiplik korunur. Aday action rules ayrıca uygunsuz hedefi gizler/reddeder.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:23
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:73

## MEC-061 — Tek tıklama aynı karede başlatıp bırakmaz

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / yerleşim

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Üretim BeginMousePlacement çağrısının karesini kaydet.
2. Aynı kare ConfirmMove çağır; sonraki kare geçerli pointer ile tekrar çağır.
3. Fiziksel tuş basılı kalma ve arming durumunu OS alt vakasıyla eşleştir.

### Beklenen sonuç

- Begin karesinde commit reddedilir; sonraki bağımsız onay geçerli olduğunda tek taşınma olur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:83
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:168

## MEC-062 — Esc/Backspace/sağ tık iptali tam geri dönüş

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / taşıma

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Üç ayrı denemede eşyayı al, döndür ve farklı aday konuma götür.
2. Sırayla Esc, Backspace ve sağ tıkla iptal et.
3. Orijinal renderer/collider, model konumu, kaynak içeriği ve hayaletin temizliğini gör.

### Beklenen sonuç

- İptal model konumu ve ekonomiyi değiştirmez; fiziksel orijinal hacim korunur, görünürlük geri gelir; aynı Esc pause açmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:90
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:112
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:209
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:218

## MEC-063 — Geçerli bırakma ve R/ShiftR/teker 45° dönüş

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / taşıma

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Boş sağlam zeminde eşyayı taşı; R, ShiftR ve tekeri ayrı kullan.
2. Sol tıkla bırak; başka denemede Enterla bırak.
3. Konum/rotation ve varsa kaynak Floorunu kaydet.

### Beklenen sonuç

- Dönüş45° adımlıdır ve360° wrap yapar; onay model ve kaynak görselini bir kez eşler; bütçe/stok değişmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:112
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:159
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:168

## MEC-064 — Eşyayı alındığı katta tutma ve.025m sınırı

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / kat fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Alt/üst/bodrum eşya fixturelarını ayrı etiketle.
2. ValidatePlacementa floorY±.024/±.026 ve farklı katYsi ver.
3. Gerçek pointer ışınının hep orijinal planee kesiştiğini karşılaştır.

### Beklenen sonuç

- Katlar arası taşıma onaylanmaz; orijinal kat düzlemi ve tolerans tutarlıdır; API MoveItem tek başına dünya collision kabulü değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:133
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:231

## MEC-065 — Döndürülmüş mobilya ile çakışma ve.06m pay

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / yerleşim matrisi

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. 0/45/90° eşyaları bir mobilyaya yaklaşan aday konumlarda tara.
2. Ayrık, pay sınırı ve kesişen ayak izlerini bağımsız hesapla.
3. Onay öncesi geometri değişirse ConfirmMove tekrarını izle.

### Beklenen sonuç

- Kesişen hacim ve emniyet payı reddedilir; geçerli aday son anda yeniden doğrulanır; test geçsin diye hacim küçültülmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:231
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:253
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:168

## MEC-066 — Kapı iki yön süpürmesi, çıkış ve merdiven koridoru

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / geçiş emniyeti

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Dört ev kapısının açık/kapalı pozlarında her iki açılma alanına eşya bırakmayı dene.
2. Girişin güney koridoruna ve merkezi merdiven geçişine aday ver.
3. Gerçek geçerli yan konumda bırakıp CharacterControllerla geç.

### Beklenen sonuç

- Kapının iki olası kolu ve çıkış/merdiven koridoru korunur; açık yaprak anlık pozuna bakıp alan işgaline izin verilmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:241
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:260
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:271

## MEC-067 — Duvarı ve kapıyı aradan geçirerek taşıma reddi

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / swept path

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Başlangıç ve son konumu ayrı ayrı boş fakat arada duvar/kapı olacak şekilde kur.
2. Cutaway ile görünmeyen authored box kollarını da dene.
3. Açık fiziksel yoldaki kontrol adayını karşılaştır.

### Beklenen sonuç

- Sadece bitiş boşluğu yetmez; swept SAT aradaki yapı/kapıyı korur; görünmeyen duvar kutusu fiziksel olmaya devam eder.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:283
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:399

## MEC-068 — Merdiven boşluğu ve bütün ayak izinin zemin desteği

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / floor sampling

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Küçük/büyük eşyayı üst kat boşluk kenarına0/45° yerleştirmeyi dene.
2. Tek köşe, kenar ve orta örneği desteksiz olan adayları ayrı kaydet.
3. Tam slab üstündeki aynı eşya kontrolünü uygula.

### Beklenen sonuç

- Bütün footprint örnekleri sağlam aynı kat slabında olmalı; boşluğa veya dış sınıra kısmi sarkma reddedilir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:245
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:330

## MEC-069 — Oyuncu kapsülü üstüne bırakma ve margin sınırı

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / actor safety

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Oyuncu center/height/radiusundan gerçek kapsülü salt gözlemle al.
2. 0/45° eşyanın kapsül üstüne, kenar emniyet payına ve uzağına aday ver.
3. İptalde eski hacmin oyuncuya geri spawn edilmediğini kontrol et.

### Beklenen sonuç

- Actor üstüne veya radius+.08m payına bırakma reddedilir; controller centerı kök poz sayılmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:90
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:364
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:310

## MEC-070 — HUD dışında, pencere dışında ve geçersiz pointer girdisi

**Alan:** Fare yerleşimi · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / pointer

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. UpdatePlacementPointera HUD üstü, ekran dışı,NaN/Infinity ve geçerli pixel ver.
2. HUD üstünde Enter/sol tık/teker onay denemelerini ayrı uygula.
3. Pencereye dönüp yeni geçerli pointerla bırak.

### Beklenen sonuç

- Geçersiz pointer kırmızı/geçersiz kalır; son eski uygun konumu gizlice onaylamaz; UI çevirme veya dünya tıklaması sızdırmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:133
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:159
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:168

## MEC-071 — Dört geçiş kapısında iki taraftan uzağa açılma

**Alan:** Kapı emniyeti · **Öncelik:** P0 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / door,inner ground,balcony,bathroom

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Her kapıya iki taraftan gerçek yürüyüşle yaklaş; E ile aç ve geç.
2. Açıkken karşı tarafa geçip odakla; yaprak yön değiştirmemeli.
3. Tam kapatıp karşı taraftan yeniden aç; yeni yön seçimini kaydet.

### Beklenen sonuç

- Tam kapalı durumda oyuncudan uzağa±100° kol seçilir; dolap/çekmece/bunker menteşesi bu davranışa zorlanmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:514
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:49

## MEC-072 — Kapanırken kapsülde durma ve çekilince otomatik devam

**Alan:** Kapı emniyeti · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / dört geçiş kapısı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Açık kapının kapanma yayında gerçek yürüyüşle dur; E ile kapat.
2. İki saniye hareketsiz kal; kapsül itilmesi veya geçiş kaybını kaydet.
3. Gerçek WASD ile güvenli alana çekil; E basmadan kapanmanın devamını izle.

### Beklenen sonuç

- Kapanma emniyet payında durur, actor itilmez; alan açılınca kendiliğinden sürer; collider kapatılmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicDoorSafety.cs:12
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:514

## MEC-073 — Açılırken kapsül koruması ve E ile tersine dönebilme

**Alan:** Kapı emniyeti · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / dört geçiş kapısı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Kapı açılma yayına çapraz yaklaş; E ile açma hareketini başlat.
2. Kapsül temas payında durmayı gözle; ardından E ile yönü tersine çevir.
3. WASD ile kapıdan iki yönde çık.

### Beklenen sonuç

- Koruma açılmada da çalışır; mevcut temas yalnız azalacak yönde çözülür; kaçış Warpı ve fizik devre dışı kalması yoktur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicDoorSafety.cs:12
- TooLateToPanic/Assets/Game/Runtime/PanicDoorSafety.cs:43
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:514

## MEC-074 — Yarı kapanmış kapıda E aynı açılma kolunu izler

**Alan:** Kapı emniyeti · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / giriş ve banyo

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Açık kapıyı kapatırken yarı açıda yaklaş ve kapanmayı engelle.
2. Henüz fiziksel kapanmadan E ile yeniden aç.
3. Önceki kolun sürmesini, kapının karşıya süpürmemesini ve actor geçişini kaydet.

### Beklenen sonuç

- Yeni±100° yön yalnız tam kapalıya yakın pozda seçilir; yarı kapanma tekrar açılmasında aynı kol korunur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:519
- TooLateToPanic/Assets/Game/Runtime/PanicDoorSafety.cs:12

## MEC-075 — Son küçük açı adımlarında Equals no-motion kontrolü

**Alan:** Kapı emniyeti · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Tüm kapılar / hassas regresyon

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. SafeDoorPosedaki no-motion kestirmesini incele.
2. Quaternion.Angle0yu hareket yok varsayan eski yaklaşımın geri gelmediğini doğrula.
3. Native adayında tam kapanma pozunu bağımsız quaternion bileşeniyle ölçmeyi planla.

### Beklenen sonuç

- Kestirme current.Equals(next) kullanır; küçük açı yuvarlaması nedeniyle son derece açık kalma riski ayrı ölçümle denetlenir. Kaynak incelemesi native PASS değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Satır referanslı salt okunur inceleme, snapshot SHA256 ve karşılaştırma raporu; çalıştırma/PASS iddiası yok.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicDoorSafety.cs:12

## MEC-076 — Margin içinde mevcut temas ve bağımsız kapsül oracleı

**Alan:** Kapı emniyeti · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kapı / sınır fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Sadece kurulum için kapsülü margin içinde farklı yaklaşım yönlerine yerleştir;Warpı say.
2. Aç/kapat adımlarını çalıştır;aktif standart CapsuleCollider sanal poz oracleıyla mesafeyi karşılaştır.
3. Sonuca ulaşmak için gerçek CharacterController kökünü ComputePenetrationa verme.

### Beklenen sonuç

- Teması artıran adım durur; azaltan geçer; controller.center ve boyutlar korunur. Fixture OS veya doğal yaklaşım yerine geçmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicDoorSafety.cs:24
- TooLateToPanic/Assets/Game/Runtime/PanicDoorSafety.cs:43
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:310

## MEC-077 — Girişte sekiz iki yönlü çapraz yürüyüş ve dönüş

**Alan:** Kapı ve rota · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / ön giriş

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. İç/dış iki başlangıç ve soldan/sağdan çapraz yaklaşımı iki yönde yürüyerek sekiz rotaya ayır.
2. Her rotada E aç, geç, dön, E kapat; kısmi kapanma üstünde yeniden açma kolunu iki kez ekle.
3. Ayakkabılık/radyo/askılık yakınında Shiftli ve normal dönüşü karşılaştır.

### Beklenen sonuç

- Hiçbir rota kapsül itmesi, kaçış kilidi veya controller değişikliği istemez; gerçek girdi ve kesintisiz video gerekir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:381
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:514
- TooLateToPanic/Assets/Game/Runtime/PanicDoorSafety.cs:12

## MEC-078 — Bodrum→zemin→üst→banyo→balkon→geri fizik rotası

**Alan:** Merdiven · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / üç kat

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Normal starttan merdivenle bodruma in;pantry E-F kullan.
2. Merdivenden zemine, sonra üst kata çık; güney tek banyo kapısını E ile kullan,ecza dolabını aç.
3. Balkona çıkıp geri dön ve bodruma aynı gerçek rotayla in.

### Beklenen sonuç

- ActiveFloor−1/0/1 geçişi görsel kesitle uyuşur; basamak ve korkuluk colliderları kaybolmaz; eski batı banyo girişi yoktur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:40
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:61
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:360

## MEC-079 — Korkuluklarda yan itme ve kat kesiti sınırı

**Alan:** Merdiven · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / merdiven ve üst boşluk

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Her iki merdivende çıkarken/inerkenA/D ile yan korkuluğa yürü.
2. Üst boşluğun kuzey/güney uçlarına normal veShiftli yaklaş;kesit değişirken sürdür.
3. Aynı noktadaSpace denemesini ayrı kaydet;istenen oyuncu davranışıyla doğal collisionı ayır.

### Beklenen sonuç

- Yan yürüyüşle boşluğa düşme olmaz;kalıcı fizik kökü aktif kalır;controller.stepOffset.25/slope48 ve kapsül boyutları korunur. Zıplamayla kasıtlı aşma ayrı tasarım sorusudur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:61
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:310

## MEC-080 — Model delta0/negatif/NaN/Infinity ve büyük delta

**Alan:** Saat ve faz · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Normal kampanya / simülasyon

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Her timed fazda geçersiz delta kollarını çağır.
2. Sonlu büyük delta ile en fazla8 geçiş korumasını ve stable GameOverı ölç.
3. Jury model Tickinin bağımsız kısa controllera müdahale etmediğini karşılaştır.

### Beklenen sonuç

- Geçersiz delta state değiştirmez;sonsuz döngü yoktur;kısa akış kendi saatinden ilerler;fixture duvar saati testi değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:230
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:732

## MEC-081 — Son60sn uyarısı bir kez ve doğru mod metni

**Alan:** Saat ve faz · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / simülasyon

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Preparing61den59a geç;ardından küçük deltalarla ilerle.
2. Yeni dalga ve yeni koşuda tekrar aynı eşiği geç.
3. Sıfır süredeki mod bazlı failure açıklamasını karşılaştır.

### Beklenen sonuç

- Bir dalgada bir kez uyarı olur;yeni dalgada yeniden kurulur;kampanya bunkera yetişememe ve simülasyon hazırlık süresi metni ayrıdır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:245
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:683

## MEC-082 — Eksik su/gıda ile gerçek afet ve doğru kayıp nedeni

**Alan:** Bunker ve sonuç · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Normal kampanya / bağımsız simülasyon

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. PackedWater/food RequiredDays−1, tam ve fazla eşdeğerliklerini ayrı kur.
2. EnterBunker ve doğal model timed çözümünü çağır;başlangıç bag stateini kaydet.
3. Bir veya iki eksik tür ve ağır hasar birleşiminde FailureReasonu karşılaştır.

### Beklenen sonuç

- Çanta yeterliliği tüketimden önce değerlendirilir;eksik tür/adet doğru açıklanır;erzak bir kez düşer ve başarısızlık ödül üretmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:465
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:801
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1000

## MEC-083 — Hasar%65 tam eşik ve üst/alt tarafın bağımsız oracleı

**Alan:** Sonuç sınırı · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / kontrollü risk fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Hasarı doğrudan set etmek yerine CalculateDamagea giren risk fixtureını bağımsız hesapla.
2. Toplam64.99/65.00/65.01 verecek deterministik kontrollü fixtureları etiketle;çantayı yeterli tut.
3. Gerçek ResolveDisaster/Aftermath yolunu çağır;yuvarlanan HUDu karar oracleı sayma.

### Beklenen sonuç

- Gerçek float toplam≤65 başarılı,>65 kayıptır;gösterim yuvarlaması kuralı değiştirmez;fixture doğal kampanya kabulü değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:87
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:801
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:821

## MEC-084 — Önlem etkisi, katkı toplamı ve%0–100 aralığı

**Alan:** Hasar modeli · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Deprem / sel / yangın / fırtına ayrı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Aynı seed/wave/eşyada korunmamış ve uygun önlemli çift fixture kur.
2. Toplam katkıyı bağımsız yeniden topla;muaf bunker/bagstation/uninstalled hedefleri izle.
3. Global sel/yangın.42 tavanı ve Storm normalize kolunu sınırda karşılaştır.

### Beklenen sonuç

- Gerçek önlem ilgili hasarı azaltır;TotalDamage katkılarla tutarlıdır;her item damage sonludur;Storma özgü ölçek diğer afetlere taşınmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:821
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:918
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:975
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:983

## MEC-085 — Başarı ödülü, destek katsayısı ve tek commit

**Alan:** Ödül ve dalga · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Normal kampanya / puan

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Dalga1/4/7,SupportCount0/4 ve hasar0/65 için bağımsız formülü hesapla.
2. Disaster→Aftermath→Success,tekrarTick/Skip/FinishInspection varyantlarını çağır.
3. Score,Budget,CompletedWaves ve LastReward farkını karşılaştır.

### Beklenen sonuç

- Formül ve yuvarlama source ile eşleşir;ödül yalnız bir kezdir;simülasyon/kısa/NoLoss dalgasında uygulanmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1059
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:779
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs:48

## MEC-086 — 3/4,6/7,9/10 sınırları ve ilk üç afet seçimi

**Alan:** Dalga kuralları · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Normal kampanya / uzun sınır fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. 1–12 dalga geçişini model fixture protokolüyle izole simüle et;hangi hazırlığın fixture olduğunu yaz.
2. RequiredDays1/2/3,süre300/270/240 ve afet sırasını kaydet.
3. 10+deterministik%3seçiminde üç ardışık aynı afeti ve Storm eklenmesini kontrol et.

### Beklenen sonuç

- Wave1–3deprem,4–6sel,7–9yangındır;10+üç temel afet arasındadır;Storm standalone simülasyondur;üç ardışık aynı seçim engellenir. Bu12dalga OS oynama değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:683
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:702
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1406

## MEC-087 — F2 aç/kapat tek başına koşuyu hileli yapmaz

**Alan:** Geliştirici paneli · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Başlık / hazırlık / inceleme

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. İzinli her fazda F2 aç;hiç seçenek değiştirme.
2. F2 ve ayrı denemede Esc ile kapa.
3. Temiz koşu göstergesi ve skor hakkını gözle.

### Beklenen sonuç

- Menü kullanımı tek başına CheatedRun yaratmaz;seçenek açmak veya para eklemek ayrı mutasyondur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:132
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:149
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:207

## MEC-088 — Açık ve geçiş karesinde model/dünya girdi kilidi

**Alan:** Geliştirici paneli · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / frame ownership

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. F2 açarken/kaparken aynı kareW,E,F,Tab,SpaceveUIclick gönder.
2. Bir sonraki kare hiçbir seçenek değiştirmeden normal girdi ver.
3. Kampanya saati ile kısa unscaled saati ayrı gözle.

### Beklenen sonuç

- DeveloperMenuOpen ve DeveloperInputBlockedThisFrame hem Model.Tick hem dünya girişini korur;bir kare sonraki normal etkileşim geri gelir;kısa global saat akmaya devam eder.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:325
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:430
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:179

## MEC-089 — İş/envanter/taşıma/sinematikte F2 retleri

**Alan:** Geliştirici paneli · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Tüm modlar / busy states

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. İş,envanter,taşıma,sinematik,LossAnnounced ve aynı kare iptal durumlarını ayrı oluştur.
2. Her birinde F2bas;modal değişimi ve giderleri gözle.
3. Meşgul durumdan çıkınca yeni F2bas.

### Beklenen sonuç

- Panel bağlamı çalmaz;ret kısa doğru açıklamayla gelir;orijinal iş/taşıma korunur;sonra geçerli fazda açılır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:182

## MEC-090 — Önceki pause/help/input lock sahipliği geri yüklenir

**Alan:** Geliştirici paneli · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık / öğretici / inceleme

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Pause açık ve help açık farklı fixturelar kur;F2aç/kapat.
2. ÖncedenInputLocked=true fakat pause sahibi olmayan durumda paneli dene.
3. Faz değişirken açık panelin temiz kapandığını gözle.

### Beklenen sonuç

- Panel kendi aldığı kilidi geri verir;pause/help durumunu uydurmaz;sinematik modeli HUD lock nedeniyle donmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:149
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:182
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:199

## MEC-091 — Sınırsız bütçe ve malzeme harcama seçenekleri bağımsız

**Alan:** Geliştirici seçenekleri · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Hazırlık /2×2 seçenek matrisi

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. İki seçenek için00/01/10/11kombinasyonlarını ayrı modelde kur.
2. Bütçe/stok eksikliğiyle aynı geçerli hedefte gerçek iş protokolünü tamamla.
3. İlk yardım ve fener pili kollarını da aynı seçenek sahipliğiyle karşılaştır.

### Beklenen sonuç

- InfiniteBudget yalnız parayı,FreeMaterials yalnız gereken/harcanan malzemeyi etkiler;ikisi de hedef/fizik/kapasite kuralını geçersiz kılmaz;her kullanım sticky hilelidir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:207
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:367
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:402
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:355

## MEC-092 — Hazırlık dondurma yalnız doğru saati etkiler

**Alan:** Geliştirici seçenekleri · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / simülasyon / kısa ayrı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. FreezePreparationTimeaçıkken Preparing ve DisasterfazlarındaTick2 ver.
2. Aynı modelde kapatıp tekrarTick2 ver.
3. Kısa controller unscaledTickini menü ve Freeze açıkken ayrıca ölç.

### Beklenen sonuç

- Normal hazırlık durur;afet/sonuç saati durmaz;kısa270/300saati hileyle uzamaz;aktif seçenek hileyi işaretler.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:230
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:145
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:330

## MEC-093 — Kaybetmeme açık/kapatılmış dalgada gerçek kayıp semantiği

**Alan:** Geliştirici seçenekleri · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / dört simülasyon

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Süre kaybı,eksik çanta,ağır hasar ve normal başarı için NoLoss açık fixtureları kur.
2. Bir kolda hazırlıkta açıp afet öncesi kapat;diğerinde sonradan kapat.
3. Hasar,tüketim,UsedThisWave,ödül ve terminali karşılaştır.

### Beklenen sonuç

- Gerçek hasar/tüketim korunur;koruma gerçek başarıya dönüştürülmez;ödül0ve sticky skor dışı kalır;kapalı kayıp kolu GameOverı koruyabilir. Ayrıntı güncel NoLoss guardlarına bağlanır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:207
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:740
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1087
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1109

## MEC-094 — +1000 bütçe faz izni, saturation ve yeni koşu yapışkanlığı

**Alan:** Geliştirici seçenekleri · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Başlık / hazırlık / inceleme / restart

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. AddDeveloperBudgetı0/negatif/1000veint sınırına yakın değerlerle fazlara göre çağır.
2. Bir seçeneği açıp kapa;menüye dönmeden CheatedRunı gözle.
3. Yeni koşuyu önce tüm seçenekler kapalı,sonra biri açık başlat.

### Beklenen sonuç

- Ekleme yalnızTutorial/Preparing/Inspectionvepozitif miktarda olur;taşma olmaz;aynı koşuda hile silinmez;yeni koşu seçenekler kapalıysa temiz,açıksa yeniden hilelidir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:218
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:629
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1266
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1459

## MEC-095 — GUID skor yolu model kurulmadan önce yalıtılır

**Alan:** Skor ve kayıt · **Öncelik:** P0 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Otomatik bütün test profilleri

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Awake/test argümanı sırasını incele;ScorePathOverride model ctorundan önce kurulmalı.
2. Profil mutlak yolunun yeni GUID alanında kaldığını ve normal kayda gitmediğini doğrula.
3. OS testinde desteklenen profil argümanı yoksa normal kaydı değiştirmeyen ayrı OS kullanıcı/çalışma stratejisini root kararıyla açık yaz.

### Beklenen sonuç

- Gerçek kayıt dosyası asla fixturea dönüşmez;override varsayımı doğrulanmadan otomatik yürütme açılmaz;üretim profilini seçen komut normal kullanıcı oyununa bağlanmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Satır referanslı salt okunur inceleme, snapshot SHA256 ve karşılaştırma raporu; çalıştırma/PASS iddiası yok.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:61
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:94
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:170
- TooLateToPanic/Assets/Game/Runtime/PanicDeveloperSmoke.cs:12

## MEC-096 — Bozuk dosya, geçersiz satır ve sıralama yükleme

**Alan:** Skor ve kayıt · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / İzole skor dosyası fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Yok,boş,bozukJSON,entries=nullvegeçersiz negatif kayıt dosyalarını ayrı GUIDlerde hazırla.
2. Modelctorunu çalıştır;uyarıları ve sağlam satırları oku.
3. Eşit wave/score kayıtlarında önceki sıra ve date alanını gözle.

### Beklenen sonuç

- Crash yerine kontrollü yükleme uyarısı;negatif satır atlanır;wave azalan,score azalan ve eşitte stabil sıra korunur;gerçek kayıt kullanılmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1199
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1229
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1245

## MEC-097 — Atomic skor yazımı hatası ve sonradan tekrar deneme

**Alan:** Skor ve kayıt · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / İzole salt-okunur/engellenmiş dosya fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Yalnız GUID test alanında yazımı geçici engelle;normal koşu sonucunu üret.
2. SaveScoresAtomic başarısızlığında bellek girişi ve mevcut dosya bütünlüğünü al.
3. Engeli kaldır;ReturnToTitle/sonraki mutasyon yolunda retryı izle.

### Beklenen sonuç

- Mevcut skor truncate edilmez;uyarı doğru kaydedilir;bellek sonucu kaybolmaz;yeniden denemede aynı koşu iki kez eklenmez. Test dizini dışına izin/ACL değişimi yoktur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1139
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1166
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:522

## MEC-098 — Normal bitiş, menü ve hile sonrası skorun tekliği

**Alan:** Skor ve kayıt · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Normal kampanya / sonuç fixture

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. CompletedWaves/Score fixtureını açık etiketle;timeout ile gerçek FailRun/RecordRunOnce çağır.
2. ReturnToTitle ve yeni run kollarını tekrarla;aynı girişin çoğalmadığını gözle.
3. Kaydedilmiş aynı koşuya sonradan geliştirici mutasyonu uygula;hileli skorun kaldırılmasını dene.

### Beklenen sonuç

- Normal koşu tek kayıt olur;sonradan hile aynı koşunun kaydını kaldırır/atomic retryı tutar;başka koşular korunur. Bu gerçek uzun kampanya kanıtı değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1139
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1271
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1284

## MEC-099 — Kısa, simülasyon ve öğretici kalıcı skor yazmaz

**Alan:** Skor ve kayıt · **Öncelik:** P0 · **Katman:** MODEL

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Üç puansız mod ayrı GUIDler

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Başlangıç izole dosya bayt hashını al;her puansız modda doğal/kayıp/atlama/menü yollarını tamamla.
2. RecordRunOnce ve SaveScoresAtomic guardlarını gözle.
3. Normal kampanya fixtureını kontrol kolu olarak ayrı profilde çalıştır.

### Beklenen sonuç

- Kısa/simülasyon/öğretici skor dosyasını ve tabloyu kirletmez;normal koşu kaydı ayrı çalışır;API fixtureı OS/HUMAN kabulü değildir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:522
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1139
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1166

## MEC-100 — Oturum kaydı ile yerel highscore özelliğini karıştırmama

**Alan:** Skor ve kayıt · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** KORUNAN_KURAL / sürüm-adaptör koşullu / Kampanya / ürün sınırı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.8de mevcut kural regresyonu;V4.9da F0final adaptör/tarif/hedef matrisiyle tekrar bağlanır. Eski mini oyun,arbitrary fire-target,hız veya seed-site beklentisi otomatik taşınmaz.

### Adımlar

1. Kaynakta dosya yazan yolları tara;highscores dışında mid-run save/resume API var mı belgeyle karşılaştır.
2. Restartın run state resetini ve kalıcı puan dosyasını ayrı haritala.
3. Yeni save/resume gereksinimi yoksa olmayan özelliği kayıp veri kusuru diye açma.

### Beklenen sonuç

- Mevcut kayıt kabulü highscore bütünlüğü ve koşu resetidir;otomatik kaldığın yerden devam davranışı uydurulmaz;belgelenmiş başka kayıt varsa F0da plana eklenir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Satır referanslı salt okunur inceleme, snapshot SHA256 ve karşılaştırma raporu; çalıştırma/PASS iddiası yok.

### Regresyon

- İlgili mevcut V4.8 API kanıtıyla kapsam karşılaştırılır; farklı DLL veya daha düşük kanıt katmanı bu vakanın PASS yerine geçmez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:629
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1166
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1222

## MEC-101 — F0da yeni iş adaptörleri ve tek action kural kaynağını doğrula

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / intake sözleşmesi

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Final intake snapshotında PanicItemActionRules ile Work.UsesDirectClick/IsWorldConstruction/IsInstalling/NextClickTarget/TryClick/InstallSeconds imzalarını bul.
2. GetClickTargetPosition, WorkTargetViewport ve ClickWorkTarget planlanan adaptörlerinin gerçek sınıf/dosya/yolunu bağla.
3. Eski giriş testlerini hangi sürüme ait olduklarıyla ayır; bütün yeni action/target ve recipe tablosunu karar manifestine yaz.

### Beklenen sonuç

- Sözleşme APIleri uygulanmış gibi varsayılmaz; eksik adaptör yeni test engelidir. Final referanslar olmadan native kabul başlatılmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Satır referanslı salt okunur inceleme, snapshot SHA256 ve karşılaştırma raporu; çalıştırma/PASS iddiası yok.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:367
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:553

## MEC-102 — Ankraj iki gerçek vida tıklaması ve her biri0.60sn oturma

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P0 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / ankraj

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Uygun yüksek mobilyayı seç ve ankraj eylemini aç; iki açıkça görünen vida hedefini kaydet.
2. İlk gerçek vidaya fareyle tıkla; kurulumu gerçek monoton saat ve video kareleriyle ölç.İkinci hedefi oturma bitmeden deneme ayrı vakadır.
3. İlk vida oturunca ikinci gerçek vidaya tıkla; ikinci0.60sn sonunda iş ve hedef durumunu izle.
4. A/D hizalama, yön dizisi veya Space tutma kullanmadan bütçe/braket/vida farkını kaydet.

### Beklenen sonuç

- İki belirgin vida sıralı kurulmalıdır; her kabul edilen vida0.60sn oturur; son vida bitince atomik tek gider ve gerçek ankraj oluşur. Render/scheduler toleransı F0da belirtilir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Her click koordinatı, vida indexi, kabul anı, IsInstalling başlangıç/bitişi, commit zamanı; API telemetrisi salt gözlem olarak etiketlenir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:384
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:586

## MEC-103 — Vida hit alanı, yanlış tık ve beş çözünürlükte gerçek fare

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / ankraj / pointer

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. 960×540,1280×720,1600×900,1920×1080 ve seçilen geniş ekranı ayrı GUID alt koşul yap.
2. Vida merkezini, görünen kenarını ve hemen dışını fiziksel fareyle tıkla; boş yüzey/HUD üstü tıklamayı ekle.
3. Yakın plandaki projeksiyonla gerçek model vida transformunun eşleşmesini kaydet.

### Beklenen sonuç

- Yalnız uygun gerçek vida hedefi kurulumu başlatır; HUD veya boş yüzey gizlice işi bitirmez; ölçeklenmiş logical pointer fixtureı bu OS testi yerine geçmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Her çözünürlükte vida ekran recti, raw OS cursor ve render karesi; DPI ve pencere modu.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs:1285

## MEC-104 — Kurulum sırasında spam, çift tık ve ters vida sırası tek maliyeti korur

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / ankraj / busy

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. İlk vida kabulünden sonra oturma.00/.20/.59sn noktalarında aynı/ikinci/geçersiz index tıklamaları gönder.
2. Oturma bittiğinde uygun sonraki vidayı tıkla; final callback sonrası tekrar dene.
3. NextClickTarget, kabul sayısı, iki vida durumu ve ekonomi günlüğünü karşılaştır.

### Beklenen sonuç

- Busy input vidayı atlamaz veya ikinci kere tüketmez; geçersiz index hiçbir state değiştirmez; iki vida sonunda yalnız bir atomik iş commit olur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:384
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:586

## MEC-105 — Vida veya direkt kurulumda son kare iptal önceliği

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P0 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / ankraj / world installation

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. İlk vida otururken, ikinci vida.59sn ve final tamamlanma karesinde ayrı cancel-first girdileri kur.
2. Drenaj/yapı/yangın istasyonu direkt kurulumlarında aynı sınırları tekrarla.
3. ProcessWorkInput/cancel adaptörünün committen önce çalışmasını ve görsel sökümünü kaydet.

### Beklenen sonuç

- İptal edilen iş bütçe/stok/koruma bırakmaz; aynı kare cancel tamamlama karşısında önceliklidir; tamamlanmış önceki gerçek iş geri alınmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:575
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:603
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:384

## MEC-106 — Eski A/D–Enter–Space dizileri yeni işi kendiliğinden bitirmez

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / doğrudan iş girdisi

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Ankraj yakın planında hiçbir vidaya tıklamadan eski A/D hizalama,Enter ve Space dizisini uygula.
2. Drenaj/yapı/tüp kurulum bağlamında Space tutma veya timing spamını dene.
3. Sonra gerçek vidayı veya güncel doğrudan kurulum eylemini kullan.

### Beklenen sonuç

- Eski mini oyun girdisi yeni işi tamamlamaz veya gider oluşturmaz; Space dünyada yanlış zıplama/çift eylem üretmez; yeni geçerli yol çalışır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:572
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:916

## MEC-107 — Uygun eylem matrisi HUD, klavye ve APIde aynı reddi verir

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P0 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / bütün hedef aileleri

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Uzun/dengesiz mobilya,alçak sehpa,yatak,koltuk,duvar,pencere,kapı,çatı,container,station ve unknown ID için F0 matrisini kur.
2. HUDda görünür eylemleri ortak rule sonucuyla karşılaştır;1/2/3/4 kısayol ve doğrudan CanBeginWork/TryCompleteWork kollarını dene.
3. Uygunsuz eylemde stok/bütçe/state, mesaj ve görünür butonları kaydet.

### Beklenen sonuç

- Desteksiz eylem gizlidir ve arka klavye/API yolu da reddedilir; yatak/koltuğa tüp ve keyfi mobilyaya yapı güçlendirme yoktur; geçerli taşıma/kapak ayrı kalır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.
- TargetID×action×HUD×keyboard×API karar CSVsi; fixture ile gerçek erişim kapsamı ayrı işaretlenir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:438
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:536

## MEC-108 — Ayrı alt kat yangın istasyonu gerçek envanter tüpünü tüketir

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P0 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / yangın hazırlığı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Alt kat dedicated stationa gerçek WASD ve kapılarla yaklaş; görünür eylemi seç.
2. Tüp yokken başlatmayı dene; ardından gerçek garaj/bodrum kaynağından E-Z/X-F ile tüp alıp dön.
3. Güncel doğrudan eylemi başlat,.60sn kurulumda bekle;FireToolCount ve tek tüp/maliyet farkını kaydet.
4. Yatak ve koltukta2ye basarak karşı kontrol yap.

### Beklenen sonuç

- Tüpsüz veya hayali tüple fire readiness verilmez; gerçek tüp istasyonda kurulur; yatak/koltuk üstüne genel tüp eklenmez; stok/ücret tek committe düşer.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:536

## MEC-109 — Kısa akış Fire hedefi ve merdiven yönlendirmesi istasyona gider

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / kısa akış / Fire bölümü

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. MEC-001in yeni direkt kurulum sürümüyle üst sel sığınmasına fiziksel ulaş.
2. Fire başladığında hedef IDsi, başlık, kısa metin, marker ve ses altyazısını kaydet; üstteki yatağı keyfi tüp hedefi sanma.
3. Yönlendirmeyle alt kata in, dedicated stationda gerçek tüp kur; sonra dış toplanma alanına fiziksel çık ve E bas.

### Beklenen sonuç

- Bütün yönlendirme tek gerçek stationa bağlanır; gereksiz üst mobilya eylemi yoktur;fire_ready ancak gerçek istasyon işiyle tamamlanır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:425
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:471
- TooLateToPanic/Assets/Game/Runtime/PanicJuryWorld.cs:43
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs:40

## MEC-110 — Ankraj dışı iş dünya üstünde0.60sn kurulur; ilgisiz workbench yok

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / drenaj / bariyer / yapı / bahçe

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Her uygun eylemi kendi gerçek hedefinde görünür buton/kısayolla başlat.
2. .60sn dünya kurulumu boyunca karakter build pozunu, hedef ilerlemesini ve kamera davranışını kaydet.
3. Cancel veya final tamamlanma sonrası tekrar hareket et; ilgisiz hizalama penceresi açılmamalı.

### Beklenen sonuç

- Ankraj dışı kurulum target0 üzerinden sürer;kurulum0.60sn civarındadır;alakasız Ratchet/Timing/Balance workbench açılmaz;iptal atomikliği korunur.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:553
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:586
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:603

## MEC-111 — Duvar ve pencerede gerçek T1/T2 geometri ve.5–.7sn yapım

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / yapı güçlendirme

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Duvar ve pencerenin T0 mesh/material/collider snapshotını al.
2. T1 ve T2 kurulumlarını gerçek protokolle bitir;ilerleme ile eklenen tier geometrisini kare kare kaydet.
3. Sadece renk veya UI text değişimini gerçek geometri yerine sayma;bağımsız mesh/transform gözlemine bağla.

### Beklenen sonuç

- Tierlere ait fiziksel görünen detay vardır;montaj.5–.7sn,target.60sn aralığına uyar;geometri yanlış açıklığı kapatmaz;ekonomi yalnız finalde değişir.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.
- Önce/kurulum ortası/T1/T2 aynı kamerada PNG ve mesh/renderer sayısı;montaj süresi gerçek clock.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:462
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:586

## MEC-112 — Yapı görseli kapı menteşesi, collision, cutaway, hasar ve reseti korur

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / yapı güçlendirme / tam rota

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Güçlendirilmiş giriş/iç kapıyı iki taraftan E ile kullan;dar geçişten WASD/Shift ile geç.
2. Güçlendirilmiş pencere/duvarın önünde katta gez;cutaway değişimi ve collider engelini gözle.
3. Gerçek afet sonrası hasar incelemesine geç;aynı tier detayları ve hasar görselini kaydet.
4. TEKRAR DENEde yeni koşu tier0veek geometri temizliği;sonraki dalgada kalıcı tier korunması kollarını ayrı dene.

### Beklenen sonuç

- Açıklık,hinge ve gerçek collider korunur;collision küçültme/hasar eşiği gevşetme yoktur;reset ile new-wave persistence farklıdır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Kapı açıklık ölçüleri/controller ölçüleri;afet öncesi-sonrası ve restart aynı açı kareleri.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicDoorSafety.cs:12
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:260
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:491
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:223

## MEC-113 — Gerçek yürüyüş3.645 ve koşu5.67 hızları aynı1.35 çarpanlıdır

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / hareket / performans

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Düz açık alanda W ve ShiftW için en az3sn sabit hız bölümünü ölç;ivmelenme başlangıcını ayrı tut.
2. W+D diyagonal,ok tuşu ve kamera yön değişikliği alt kollarını aynı zemin üzerinde kaydet.
3. Controller ölçülerini ve basamak/slope sınırlarını read-only telemetriyle eski tabanla karşılaştır.

### Beklenen sonuç

- Walk3.645/run5.67m/s hedefleri aynı1.35 katsayıyı korur;diyagonal avantaj,teleport,step/slope büyütme yoktur. Frame cap/damping nedeniyle kabul toleransı F0da ölçüm tasarımına bağlanır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:310
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:381

## MEC-114 — Hareket animasyonu yeni hızı izler; rig/clip/root-motion korunur

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / karakter / mekanik bağ

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Avatar/controller/klip adları ve root-motion politikasını F0snapshotında al.
2. Yeni yürüyüş/koşu,kurulum,ankraj tıklama,merdiven ve zıplama geçişlerini doğal motionla kaydet.
3. State/task pozu bitince controller hareketi ve animator hızının resetini gözle.

### Beklenen sonuç

- Retained CC0 rig/klipler çalışır;applyRootMotion=false ve controller sahipliği korunur;build pozu görünür;hız artışıyla bariz kayma ve takılı çalışma pozu olmamalı. İnsan sanat kabulü ayrı kalır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:307
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:371
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs:166

## MEC-115 — Üç bahçe bölgesi tek sıra kurulum ve ağaç kiti stoğu

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / bahçe / bölge matrisi

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. F0manifestinden üç bölge IDsi,kit tarif sayısı ve display adlarını al.
2. Her bölgeyi bir kez kur;aynı bölgeyi tekrar dene;dördüncü/bilinmeyen bölgeyi ret olarak dene.
3. Sayaç,kit tüketimi ve mature-row stateini karşılaştır;legacy seedling IDsi varsa yalnız uyumluluk kimliği diye etiketle.

### Beklenen sonuç

- Üç açık bölgenin her biri tek.60sn işlemle bir sıra yetişkin ağaç kurar;aynı bölge çift sayılmaz;ekranda fidan/tohum/sulama görevi kalmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- İzole modelin başlangıç fixture JSONu, her çağrı ve önce/sonra değerleri; bu sonuç OS veya native fizik kabulü sayılmaz.
- BölgeID×kitcost×state tablosu;legacy iç ID ile oyuncuya görünen ad ayrı alanlar.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:433
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs:530

## MEC-116 — Yetişkin ağaç sırası yol/garaj/bunker/kapı/oyuncuyu engellemez

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / bahçe / fiziksel rota

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Üç bölgeyi gerçek kit ve eylemlerle tek tek kur;kurulum öncesi/sonrası top-down ve actor kamerasını kaydet.
2. Her sıranın ardından yol,garaj girişi,bunker yolu ve ön kapıdan iki yönde WASD/Shift ile geç.
3. Oyuncu kurulum kapsül alanına yakınken ve aynı bölgeyi yeniden kurarken ret/yerleşim davranışını gözle.

### Beklenen sonuç

- Birkaç yetişkin ağaç görünür;dar kaçış yolu ve kapsül alanı bloke olmaz;sırayı geçirmek için oyuncu ışınlanmaz veya collider kapatılmaz.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Kesintisiz ekran videosu ve zaman damgalı gerçek klavye/fare kaydı; API girdisi, Warp, model Tick enjeksiyonu ve fixture sayısı sıfır. Salt gözlem telemetrisi varsa ayrıca etiketlenir.
- Üç bölge ağaç transform/collider listesi ve geçiş videosu;dekor görünümü tek başına collision kanıtı değildir.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:310
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs:381
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs:364

## MEC-117 — Yeni semantik kısa akış harnessı gerçek tıklamaları ve dört afeti kullanır

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / otomasyon uyarlaması

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. PanicFullFlowSmoke eski StepWork/A-D yön stratejisini tara;DirectActions click-target protokolüne uyarlama gereksinimini tek tek çıkar.
2. Doğal saat,CharacterController,E/F/Tab-Enter,harcama ve dört afet terminalini koruyan yeni rota taslağını kontrol et.
3. Eski149/189 ve audit UI138 model/pointerfixture sonuçlarının yeni script veya DLLye taşınmadığını raporla.

### Beklenen sonuç

- Otomasyon sözleşmesi tıklama0.60sn ve gerçek stationa bağlanır;testWarp/Tick/hile sayacı0rotada kalır;API rota OS/HUMAN kabulüne yükseltilmez.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Satır referanslı salt okunur inceleme, snapshot SHA256 ve karşılaştırma raporu; çalıştırma/PASS iddiası yok.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs:28
- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs:246

## MEC-118 — Dört yeni simülasyonda doğru kayıp/inceleme/tekrar ve0ödül

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / deprem / sel / yangın / fırtına ayrı

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Her afet için yeni direct inputla bir geçerli sonlu önlem uygula;özellikle yangını istasyonda kur.
2. AFETİ BAŞLATtan doğal sonucu bekle;gerçek kayıp olursa Hasarı İncele yolunu seç.
3. İncelemede hareket et,TEKRAR DENE ve ANA MENÜyü üretim callbackleriyle uygula;verilen bütün fixture/semantic inputu açık say.

### Beklenen sonuç

- Her afet tek gerçek terminaldir;kayıp hasarı/failure korunur;simülasyon puan/ödül yazmaz;90sn/1su1gıda/1000bütçeyle aynı afet temiz tekrar başlar.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Gerçek player render/log ve üretim API çağrı günlüğü; tüm kurulum Warp/fixture/zaman enjeksiyonları ayrı sayılır. OS girdi kabulü değildir.
- Afet başına mode,phase,hasar,failure,stock,budget,repeat state günlüğü ve native render.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:7
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs:18
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:481
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1077

## MEC-119 — İlk180sn insan: küçük kazanım, gerçek önlem ve görünür sonuç anlaşılır mı

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / kısa hazırlık / ilk deneyim

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Oyunu daha önce oynamamış katılımcıya yalnız başlıktaki metinleri ver;gözlemci rota/tuş öğretmesin.
2. İlk180sni kesintisiz kaydet;ilk pickup,gerçek vida/kurulum ve görünür afet sonucu anlarını işaretle.
3. Durmadan önce katılımcının hangi önlemin neyi değiştirdiğini kendi sözüyle açıklamasını ve devam isteğini kaydet.
4. Yanlış tıklamalar,istasyona yönelim,HUD okuma,merdiven/çıkış arama ve yardım isteme sayısını ölç;sonradan görüş al.

### Beklenen sonuç

- Anlama,keyif ve devam isteği katılımcı kanıtıyla değerlendirilir;hızlı deterministik API49.67sn veya seyredilen video insan PASS sayılmaz. Hedefe ulaşamama gerekçesi gözlemle ayrılır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Katılımcı kodu, ön deneyim, kesintisiz video, gerçek süre ve kendi ifadeleri; gözlem ile yorum ayrı yazılır.
- Katılımcı başına gerçek süre/olay tablosu,algılanan önlem-sonuç açıklaması,devam isteği ve gözlemci yardımı0/var kaydı.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:97
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs:371

## MEC-120 — İnsan uzun koşu: tekrar yükü, doğrudan işler ve sesle yön bulma

**Alan:** V4.9 doğrudan eylemler · **Öncelik:** P2 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS_ADAYI / normal kampanya / uzun deneyim

### Önkoşullar

- F0: Yürütme gününde seçilen EXE/DLL, kaynak snapshotı, sahne, profil ve manifest kimliği dondurulur. Canlı World/Life/Hud/Builder için V4.8 kabulü varsayılmaz.
- Yalnız root tarafından açılan izole GUID test profili; gerçek kayıt ve açık kullanıcı playerı korunur. Bu katalog hiçbir vakayı çalıştırmaz.
- V4.9 DirectActions ADAYI: sözleşme var; üretim/intake sürüyor. F0da final kaynak/adaptör/tarif/hedef matrisi dondurulmadan uygulanmış sayılmaz; V4.8 DLLsi bu kabulü sağlamaz.

### Adımlar

1. Yeni tasarımı öğrenmiş katılımcıyla gerçek hilesiz uzun kampanya başlat;12+dalga hedefi ve doğal kayıp terminalini açık kaydet.
2. Tekrarlı toplama/çantala/kurulum yükünü,yanlış action beklentisini,yorgunluk ve hareket kontrolünü dalga1/4/7/10da gözle.
3. Kısa akışta izinli konuşmaların anlaşılmasını ayrı oturumda dinlet;normal kampanyada yalnız yetkili afet sesleri kuralını kontrol et.
4. Katılımcı hangi sonucu adil/anlaşılmaz bulduğunu kendi sözüyle açıklasın;model hesaplarıyla bağımsız karşılaştır.

### Beklenen sonuç

- İnsan akıcılık/keyif/işitsel kalite kabulü gerçek deneyime dayanır;model sınırı doğruysa doğal kayıp otomatik kusur değildir;öneri ile kabul ayrı yazılır.

### Kanıt

- Seçili build/DLL SHA256 ve kaynak manifesti; koşu GUID, mod, çözünürlük, başlangıç/bitiş zamanı.
- Katılımcı kodu, ön deneyim, kesintisiz video, gerçek süre ve kendi ifadeleri; gözlem ile yorum ayrı yazılır.

### Regresyon

- V4.8 stok/çanta/cost/cancel/kapı/skor değişmezleri korunur; eski Ratchet/Timing/Balance girdileri bu adayda kabul gereği değildir.
- Beklenen adaptörler sözleşmede planlanmıştır; F0da gerçek imza ve dosya satırlarıyla bağlanır. Yeni sonuçlar yalnız yeni DLLye aittir.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:683
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs:1059
- TooLateToPanic/Assets/Game/Runtime/PanicPresentationAudio.cs:7

## EXP-001 — F0 sözleşme, kaynak, build ve davranış profilini kilitle

**Alan:** kanıt-kapısı · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** TÜMÜ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Yalnız okuma; henüz bu planın hiçbir vakası yürütülmedi.
- Root'un aday snapshot ve build seçimi beklenir.

### Adımlar

1. LATEST/V4.8 fallback kimliğini, V4.9 yeni sözleşmesini, aday snapshot ve DLL kimliklerini aynı F0 davranış manifestine bağla.
2. Root'un bildirdiği dört canlı farkı ve devam eden V4.9 staging/intake durumunu inceleme kuyruğuna yaz; farkı kusur veya uygulanmış özellik diye sınıflandırma.
3. V4.9 için iki vida/0.60sn dünya işi/yangın istasyonu/ağaç bölgeleri/1.35 hız/retained humanoid davranışlarını kaynak, build ve kanıt katmanına göre ayrı işaretle.
4. Her ekran/kadrajın hangi profile ait olduğu kesinleşmeden execution veya PASS devralma.

### Beklenen sonuç

- V4.9 üretim hedefidir; tamamlanma/derleme/native kabulü henüz varsayılmaz. V4.8 DLL5683E713... yalnız kendi manifesti için fallback kanıtıdır.
- Yeni sözleşmede geçersiz kılınmış A/D–Timing–Balance beklentileri yeni adayın evrensel gate'i olmaz.
- Dört canlı drift ve yeni staging farkları tek tek kaynak otoritesine bağlanır; eski PNG güncel HUD kabulü değildir.

### Kanıt

- Aday kimlik formu; manifest/diff listesi; screen-case eşlemesi; eksik kimlik alanları.

### Regresyon

- Her kaynak veya baked asset değişiminde yalnız etkilenen ekranlar yeni kimliğe taşınır; tarihsel kayıtlar değiştirilmez.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- AGENTS.md
- TooLateToPanic/AGENTS.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-002 — Ekran ve modal önceliği erişim haritası

**Alan:** envanter · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** TÜMÜ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Aday kaynak snapshot seçilmiş; durumlar plan rubriği envanterine eşlenmiş.

### Adımlar

1. PanicHomeHud.OnGUI ve Jury/Simulation partial dallarını seçili F0 snapshot'ından çıkar.
2. V4.9'da direct-click yakın planı, dünya yapım progress'i, action eligibility ve ground-floor yangın istasyonu state'lerini ayrı envantere ekle; henüz kaynakta yoksa PENDING_IMPLEMENTATION yaz.
3. Her state için giriş/çıkış, modal öncelik, girdi sahibi ve profile uygun eylemi yaz; V4.8 work ailesi atlasını yalnız fallback başlığına al.

### Beklenen sonuç

- Her gerçek ekran ve veri varyantı profile göre kapsanır; kaynakta görülmeyen V4.9 ekran varmış gibi kabul edilmez.
- Yürütülecek adayın eski mini oyun state'lerine dönmesi beklenmez; gerekiyorsa uyumluluk alanları oyuncu davranışı diye sunulmaz.

### Kanıt

- Ekran durum tablosu; metot referansları; N/A gerekçeleri.

### Regresyon

- Yeni panel eklendiğinde envanter ve ekran örtüşme vakaları birlikte güncellenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-003 — Başlıkta ürün vaadi ve ilk beş saniye

**Alan:** ilk-alıcı · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** BAŞLIK

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- En az üç oyunu bilmeyen gönüllü; aday normal başlıkta; geliştirici açıklama yapmaz.
- İlk görünüm videosu ve alıntı kaydı için katılımcı rızası alınmış.

### Adımlar

1. İlk görünümden beş saniye sonra 'Bu nasıl bir oyun, senden ne yapmanı bekliyor?' diye sor.
2. Katılımcının temayı, evi ve ana çağrıyı kendi sözleriyle anlatmasını kaydet.
3. Puanlamadan sonra logosu, arka ev, renkler ve güven hissi için rubrik 0–4 değerlendir.

### Beklenen sonuç

- Afete hazırlık ve önlem amacı menü üzerinden anlaşılabilir; görsel vaat gerçek oyundaki evi temsil eder.
- Yanlış beklenti, beğeni ve gözlenen okuma güçlüğü ayrı kaydedilir; üç kişi genel pazar sonucu sayılmaz.

### Kanıt

- Beş saniyelik ilk izlenim cevabı; başlık karesi; bakış/tereddüt notu; rubrik ham puanı.

### Regresyon

- Başlık metni, arka kamera veya logo değişirse aynı test yeni katılımcıyla tekrarlanır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Documentation/GDD.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-004 — Üç oyun modunun amacı seçimden önce anlaşılıyor mu

**Alan:** ilk-alıcı · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** BAŞLIK / KISA / SERBEST / SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Oyunu bilmeyen kişi; öğretici veya sözlü mod açıklaması verilmez.

### Adımlar

1. Kişiden kısa deneyim, belirli bir afet denemesi ve uzun koşu için hangi düğmeyi seçeceğini söylemesini iste.
2. HAZIRLIĞA BAŞLA, FELAKET SİMÜLASYONU OYNA ve SERBEST OYUN seçim beklentilerini kaydet.
3. Seçim sonrası gerçek süre/baseline/skor durumunu beklentiyle karşılaştır.

### Beklenen sonuç

- Kısa akış kampanya skor koşusu gibi; simülasyon gerçek fizik sertifikası gibi sunulmaz.
- Oyuncuya görünen UI/seste jüri/demo üretim dili kullanılmaz; yanlış seçim varsa metin/yerleşim bulgusu açılır.

### Kanıt

- Seçim ve gerekçe dökümü; tıklama videosu; beklenti-gerçek tablosu.

### Regresyon

- Mod etiketi veya giriş ekranı değişince seçim testi ve geri dönüş testleri etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-005 — Mevcut mekânsal amaçla tutarlı anlatım kapsamı

**Alan:** hikaye · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** TÜMÜ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- GDD, güncel kararlar ve live snapshot birbirinden tarihlenmiş.

### Adımlar

1. Mevcut ev/oyuncu/erzak/önlem/sığınak/komşu anlatımını ve V4.9 kullanıcı kararlarını birlikte çıkar.
2. Her görsel/konuşma/sonucu amaç–eylem–etki zincirine bağla; yatak/koltuğa tüp takma ve fidan sulama eski vaadinin yeni profile sızmasını ara.
3. Yeni görev, NPC konuşması, araç sürme, nüfus veya zincirleme afet vaatlerini kapsam kararına gönder.

### Beklenen sonuç

- Hikâye mevcut evde hazırlığın sonucudur; yeni karakter/misyon icat edilmez.
- V4.9 yangın hazırlığı gerçek ground-floor istasyona, ağaç işi bölgesel olgun sıra kitine bağlanır; eski generic action dili yeni davranışa taşınmaz.

### Kanıt

- Vaad envanteri; mevcut/ertelenmiş/kapsam dışı etiketleri; metin kaynak referansı.

### Regresyon

- Her yeni tutorial cümlesi, görsel işaret veya sonuç kopyası aynı nedensellik matrisiyle incelenir.

### Kaynak referansları

- AGENTS.md
- TooLateToPanic/AGENTS.md
- TooLateToPanic/Documentation/GDD.md
- TooLateToPanic/Assets/Game/Runtime/PanicExteriorWorld.cs
- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-006 — Başlangıç stoğu ve oyuncunun emeği ayrımı

**Alan:** ilk-alıcı · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA / SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Temiz kısa akış ve ayrı yeni simülasyon oturumu; hileler kapalı.

### Adımlar

1. Katılımcıdan başlangıçta çantada ve envanterde neyin hazır olduğunu söylemesini iste.
2. Suyu gerçekten aldıktan ve çantaya koyduktan sonra hangi işi kendisinin yaptığını sor.
3. Simülasyonda hazır 1 su/1 gıda ile sonuçtaki tüketimi görmesini sağla; kısa akışla karıştırıp karıştırmadığını kaydet.

### Beklenen sonuç

- Hazır gıda, aletler ve simülasyon çantası oyuncunun tamamladığı başarı olarak anlatılmaz.
- Toplama, envantere girme ve çantaya koyma ayrı ama kısa ve anlaşılır karşılıklara sahiptir.

### Kanıt

- Başlangıç/son çanta karesi; oyuncu açıklaması; gerçek model delta notu.

### Regresyon

- Başlangıç paketi veya HUD sayaçları değişince kısa sonuç ve simülasyon sonuç doğruluğu yeniden ele alınır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationHud.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-007 — Sözlü yardım olmadan ilk hedef ve ilk gerçek kazanım

**Alan:** ilk-alıcı · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Yeni kişi, normal başlangıç; dış saat başlatılmış; önceden izlenmiş rota yok.

### Adımlar

1. İlk hareket, ilk hedefe yaklaşma, E açma, seçili suyu F alma ve Tab/Enter anlarını zaman damgasıyla kaydet.
2. Katılımcı durduğunda ipucu verme; beş saniyeyi aşan hedefsiz duruşun yerini, görülen ipucunu ve tekrar girdisini yaz.
3. İlk su kazanımından sonra 'Şu anda ne değişti?' sorusunu oturum sonuna bırak; ana koşuyu bölme.

### Beklenen sonuç

- E ve F ayrımı keşfedilir; pickup sadece uçan dekor izlenimi vermez.
- Anlaşılabilirlik gözlemle raporlanır; hızlı API rotası kişi yerine konmaz. Takılma kişiyi suçlayan bir sonuç olmaz.

### Kanıt

- Kesintisiz ekran+girdi videosu; ilk hareket/ilk etkileşim/ilk kazanım zaman çizelgesi; yardım miktarı.

### Regresyon

- Hedef vurgusu, ilk metin, kaynak seçimi veya çanta işaretleri değişince yeni kişiyle tekrarlanır.

### Kaynak referansları

- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryPresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-008 — İlk 180 saniyede gerçek hazırlık–afet–sonuç

**Alan:** insan-süre · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- En az üç oyunu bilmeyen kişi, hedef sunum bilgisayarı; temiz profil; hile yok; dış duvar saati.
- Kişi başlık öncesinde ısındırılmaz; oyun bilgisi verilen kişi ayrı raporlanır.

### Adımlar

1. Menü seçiminden gerçek sonuç görünene kadar konuşmadan gözle.
2. Su alma/çantaya koyma, tamamlanan sabitleme, deprem ve gerçek hasar karşılığının zamanını yaz.
3. Normal rotada drenaj ve fiziksel üst kat güvenli noktasının da 180 saniye içine girip girmediğini kaydet.
4. 180 saniye sonrası oturumu kesmeden devam ettir; zorlanma ve atlamaları dürüstçe etiketle.

### Beklenen sonuç

- İnsan kabul hedefi: ilk tam döngü ≤180sn; normal rotada sel örneği de aynı pencereye sığar.
- Süre geçse bile kişi yaptığı önlemin etkisini açıklayamıyorsa his/anlama hedefi geçti sayılmaz.

### Kanıt

- Kişi bazlı ham zamanlar, ekran/girdi kaydı, tamamlandı/eksik/atlandı durumu, sonuç sonrası nedensellik cevabı.

### Regresyon

- Rota, iş adımı, menü, hedef metni, kamera veya ses sürelerini etkileyen değişiklikte insan zamanı yeniden ölçülür.

### Kaynak referansları

- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-009 — 270 saniye içerik hedefi ve kontrol geri dönüşü

**Alan:** insan-süre · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- EXP-008 oturumu devam eder; kişi seçimini kendi yapar; hızlandırılmış oyun saati yok.

### Adımlar

1. Başlık, okuma, hareket, yanlışlar, envanter, işler, sinematikler, sel, yangın, inceleme ve karneyi aynı dış saatle izle.
2. Her aşamada görünür ana hedefin değişimini ve gereksiz beklemeyi kaydet.
3. Sonuç ekranına normal denemede ≤270sn erişimi ve bundan sonra tekrar/başlığa seçeneğini sınat.

### Beklenen sonuç

- İçerik hedefi tüm yerel yönerge, yürüyüş ve geçişleri içerir; gizli ek süre sayılmaz.
- Hızlı bitene boş bekleme/ek görev verilmez; eksik hedefler sonuçta doğru kalır.

### Kanıt

- Bölüm bazlı zaman çizelgesi; 270sn anı karesi; karne ve input sahibi kaydı.

### Regresyon

- Süre kazandıran düzeltmede ana maliyet/kayıp kuralları korunur; normal ve yavaş insan rotası tekrar örneklenir.

### Kaynak referansları

- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-010 — 300 saniye sınırında yavaş, atlayan ve mola veren kişi

**Alan:** insan-süre · **Öncelik:** P0 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Root'un izole OS QA oturumu; gerçek kişinin süresini taklit eden önceden yazılmış yavaş rota; gerçek profil korunur.

### Adımlar

1. Bir koşuda ilk işi kaçır; diğerinde uzun envanter okumaları ve Esc molası kullan; üçüncüde eksiklerle devam et.
2. Dış saatte 270 ve 300 noktalarında görünen ekranı, kısa akış saatini ve kalan girdi erişimini kaydet.
3. Süre sonunda mevcut durumu gösteren karneye, tekrar ve başlığa gerçek tuş/tıklama ile ulaş.

### Beklenen sonuç

- Ortak yedek toplam30sn'dir; duraklama veya F2 kısa akış dış saatini dondurmuş gibi sunulmaz.
- 300sn üst sınırında oturum uygulama kapanmadan sonuçla sonlanır; atlanan eylem tamamlandı olmaz. Erken sonucun kabulü ayrıca not edilir.

### Kanıt

- Kesintisiz OS videosu, dış saat overlay/yan kayıt, her deadline'da model snapshot ve sonuç hedefleri.

### Regresyon

- Pause/dev/inventory saat sahipliği veya deadline geçişi değişince üç rota tekrar edilir.

### Kaynak referansları

- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-011 — Soğuk açılış ve sunum zamanının dürüst hesabı

**Alan:** insan-süre · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** NORMAL BAŞLATMA / KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Root tarafından seçilmiş kapalı QA kopyası; kullanıcının açık player'ı korunur; çevrimdışı hedef makine.

### Adımlar

1. EXE/launcher başlatmasından ilk etkileşimli başlığa ve ilk oyuna kadar dış süre ölç.
2. Yükleme, siyah kare, pencere odaklanma ve çözünürlük yerleşimini ayrı zamanla.
3. Sunum başlıkta hazır başlatılacaksa bu önkoşulu kayda geçir; soğuk açılışla hazır başlık süresini ayrı göster.

### Beklenen sonuç

- Kişiye ayrılan pencereye yükleme giriyorsa 300sn hesabına dahildir.
- Ölçümden yükleme çıkarılarak süre kabulü üretilmez; çevrimdışı eksik font/clip/atlas beklemesi yoktur.

### Kanıt

- Açılış videosu; makine/çözünürlük; launcher/EXE/DLL kimliği; cold/warm ayrı ham süre.

### Regresyon

- Paketleme veya Resources/font/başlık değişikliği sonrası soğuk açılış yeniden ölçülür.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-012 — Karne sonrası devam isteği ve tekrar vaadi

**Alan:** ilk-alıcı · **Öncelik:** P2 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA SONUÇ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Oturum doğal sonuna gelmiş; katılımcıya başarı telkini verilmez.

### Adımlar

1. 'Bir örnek daha oynamak ister miydin, neden?' ve 'Bir sonraki denemede neyi değiştirirdin?' sor.
2. TEKRAR DENE ve BAŞLIĞA DÖN etiketlerine göre beklenen ilerlemeyi kaydet.
3. Yanıtları gerçek önlem farkı, merak, tekrar yükü ve belirsizlik olarak gerekçeli kodla; olumlu/olumsuz ham ifadeyi koru.

### Beklenen sonuç

- Mevcut mekaniklerden doğan merak ile yeni görev/karakter beklentisi ayrılır.
- Üç kişinin isteği elde tutma oranı veya genel eğlence kanıtı olarak pazarlanmaz.

### Kanıt

- İnsan alıntıları, 1–5 öznel istek puanı, nedenler ve gözlenen tekrar seçimi.

### Regresyon

- Sonuç dili veya tekrar başlangıcı değişince küçük yeni katılımcı örneklemiyle karşılaştır.

### Kaynak referansları

- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/Documentation/GDD.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-013 — Üç seviye ve tüm ek mekânların görsel yön haritası

**Alan:** mekan · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SERBEST / İNCELEME

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Temiz izole QA koşusu; kontrol açık; gerçek fizik rotası; ekran/kadraj envanteri hazır.

### Adımlar

1. Ön bahçe→zemin giriş/salon/mutfak→fiziksel merdiven→üst yatak/çalışma/banyo/balkon→zemin→bodrum→garaj rotasını yürü.
2. Her yeni mekânda giriş, orta ve çıkış karesini al; kat değişimini ekranda doğrula.
3. Bir odanın dekoru/duvar kesiti diğer odanın görev veya kapısı gibi göründüğü noktaları işaretle.

### Beklenen sonuç

- Oyuncu iki yaşanabilir kat, bodrum, balkon ve bitişik garajı ayırt eder; kat değişimi görünür.
- Ev içi ölçek, yollar ve kamera kopuk bir maket gezisi hissi üretirse öznel puan ile fizik kusuru ayrılır.

### Kanıt

- Tam rota videosu; kat/oda sıra listesi; giriş-orta-çıkış PNG; kamera ve player pozları.

### Regresyon

- Geometri, cutaway, kamera veya eşya yerleşimi değişen mekân ve komşu geçişler yeniden yürünür.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-014 — Ön girişin okunurluğu ve yaşanmış ev duygusu

**Alan:** mekan · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST / KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- İlk oyuncu; ön kapıya bahçeden ve içeriden yaklaşım; varsayılan yakınlık ve bir orbit varyantı.

### Adımlar

1. Kapı, ayakkabılık, askılık ve radyo masasını bakışta ayırt ettir; geçiş yönünü sözlü yardım olmadan seçmesini izle.
2. Ön kapı çevresinde ileri/geri dönme, açılma ve güvenli bekleme anını görüntüle.
3. Giriş süslerinin hedef işaretini veya çıkış yolunu örttüğü noktayı kaydet.

### Beklenen sonuç

- Evin sıcak kimliği ve çıkış amacı birlikte okunur; dekor oyun hedefi gibi yanlış vurgulanmaz.
- Ön girişten dönmek görsel olarak anlaşılır; kapı sıkışması gözlenirse mekanik vaka sahibine somut adımla aktarılır.

### Kanıt

- Kapı kapalı/açılır/açık karşılaştırma kareleri; katılımcı yol kararı; occlusion notu.

### Regresyon

- Giriş prop/kamera/kapı menteşesi değişince hem sanat hem kapı güvenliği rotaları tetiklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-015 — Salonun eşya hiyerarşisi ve iki kahraman eşya

**Alan:** mekan · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA / SERBEST / DEPREM İNCELEME

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Kitaplık, indigo koltuk, TV, masa, şömine ve buzdolabı çevresi görünebilir; önlem öncesi durum.

### Adımlar

1. Salon genelini ve etkileşime yakın kadrajı incelet; kişi hangi eşyanın sabitlenebilir olduğunu söylesin.
2. Kitaplık/buzdolabı önlemi öncesi, iş sırası ve deprem sonrası aynı kamera ile kaydet.
3. Şömine dekor alevi ile afet yangını veya turuncu hedef vurgusunun karışıp karışmadığını sor.

### Beklenen sonuç

- İşlevsel ana nesne seçilebilir; oda doluluğu hedefi boğmaz.
- Bir eşyanın az sallanması mutlak korunma garantisi şeklinde yorumlanırsa tema/geri bildirim bulgusu açılır.

### Kanıt

- Genel/yakın karşılaştırma; oyuncunun seçtiği nesne/gerekçe; renk ve malzeme rubrik puanı.

### Regresyon

- Salon yerleşimi, model varyantı veya efekt tonu değişince hedef seçimi ve risk kimliği kontrol edilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-016 — Mutfak ve zemin doğu bölümünde kaynakların anlamı

**Alan:** mekan · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA / SERBEST

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Gerçek açık/kapalı buzdolabı ve mutfak dolabı; sonlu stok; E/F ayrımı etkin.

### Adımlar

1. Mutfak genelinde yenebilir/iş malzemesi depolarını ve gerçek su nesnesini bulmasını izle.
2. İki kapaktan art arda açıp yalnız seçili malzemeyi al; doluluk görselini kontrol et.
3. Depolama rafı/dekor ile loot kaynağı ayrımı ve dolap içi ikon/3D model uyumunu sor.

### Beklenen sonuç

- Kaynak içeriği anlaşılır; seçili su ile stok sayısı aynı malzemeyi anlatır.
- Duvar/paravan ve mobilyalar eylem etiketini gizlemez; dekoru toplama beklentisi kayıt altına alınır.

### Kanıt

- Mutfak giriş/kapak/son stok görüntüleri; yanlış kaynak denemeleri; model/HUD delta.

### Regresyon

- Kaynak düzeni, pickup modeli veya dolap detayı değişince açık/dolu/boş ve ilk su rotası tekrar edilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Art/PanicCuratedAssetArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-017 — Üst kat yatak, çalışma ve depoların kimliği

**Alan:** mekan · **Öncelik:** P2 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST / YANGIN / İNCELEME

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Merdivenle üst kata gerçek erişim; yatak/halı/desk_upper bilgisayar/up_cabinet görünür.

### Adımlar

1. Yatak/halı/çalışma köşesini üç kadrajda incele; bilgisayar açık/kapalı görünüşünü ve etkileşim yazısını kaydet.
2. V4.9'da yatak/koltukta söndürücü eylemi sunulmadığını, yangın hedefinin alt kattaki gerçek istasyona yöneldiğini gör; V4.8 fallback'te eski yakın-mobilya hedefini tarihsel davranış olarak etiketle.
3. Üst kat→merdiven→ground-floor istasyon→dış toplanma rotasında kat ve hedef metnini gözle.

### Beklenen sonuç

- Üst kat yaşanabilir ayrı mekân olarak okunur; halı/PC/material görsel kalitesi profile göre incelenir.
- V4.9 oyuncusu yatağa tüp takmaya yönlendirilmez; hedef işareti ve anlamı istasyona bağlıdır. Fallback karşılığı yeni kabul diye sunulmaz.

### Kanıt

- Üst kat üç kare; ekran açık/kapalı; hedef/kat etiketi videosu; rubrik.

### Regresyon

- Üst oda eşya/kamera değişikliğinde yangın hedefi, banyo kapısı ve merdiven görüşü de örneklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-018 — Banyoda tek güney giriş ve nesne işlevi

**Alan:** mekan · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SERBEST

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Üst kata merdivenle varılmış; bathroom_door güneyde; ana kullanıcı player'ı korunur.

### Adımlar

1. Güney kapıya iki yandan yaklaş, aç ve gir; batı duvarını görsel ve yol beklentisi açısından incele.
2. Lavabo çanağı, tuvalet, küvet, paspas, duş ve ecza dolabını genel ve yakın planlarda kaydet.
3. Kapak/menteşe hareketlerini, matın kapı yayında kalmamasını ve ecza malzemesi seçimini uygula.

### Beklenen sonuç

- Banyo dar çift koridor izlenimi vermez; batıda hayali giriş yönlendirmesi yoktur.
- Lavabo çöküklüğü ve seramik/metal ayrımı oyun içi ışıkta okunur; tek giriş geçişi dekorla gizlenmez.

### Kanıt

- Güney giriş öncesi/sırası/sonrası video; genel/yakın PNG; kapak kaynak kimliği.

### Regresyon

- Banyo prop/duvar/kapı değişirse tek giriş ve fiziksel kapı regresyonu root kuyruğuna eklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- AGENTS.md
- TooLateToPanic/AGENTS.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-019 — Balkonun evle bağı ve güvenli sınırın okunması

**Alan:** mekan · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST / AFET SONRASI

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Üst katta fiziksel balkon kapısı; deck ve korkuluk; açık hava kadrajı.

### Adımlar

1. Balkona çıkıp içeri geri dön; kapalı/açık kapıda ön duvar kesitini izle.
2. Sandalye, masa ve saksıların dolaşım alanı ve korkuluk çizgisini kapatmasını değerlendir.
3. Kamera sağ/sol orbitte sınırı ve zemini okumayı dene; güvenli nokta ile karıştırıldığını sor.

### Beklenen sonuç

- Balkon bağlantısı, zemini ve sınırı anlaşılır; içine düşülüyormuş görüntü veren kesit boşluğu yoktur.
- Dekoratif balkon yangın toplanma noktası/sel hedefi diye yanlış anlatılmaz.

### Kanıt

- İç→eşik→balkon karesi; iki yaw video; katılımcı sınır açıklaması.

### Regresyon

- Balkon geometri/renderer veya chair değişikliğinde korkuluk fizik vakasıyla birlikte etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicCameraOrbit.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-020 — Bodrum servis alanının kimliği ve sel mesajıyla uyum

**Alan:** mekan · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST / SEL

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Bodrum gerçek merdiven rotası; çamaşır makinesi/termosifon/raf/tezgâh görünür.

### Adımlar

1. Bodrumun girişinden servis köşelerine kadar gez; sıcak lamba ve koyu duvarlarda nesne ayrımını değerlendir.
2. Çamaşır kapağı ve servis dekorunun etkileşimini dene; gerçek loot ile dekoru ayırt et.
3. Sel akışında 'Bodruma inme' yönlendirmesi görülürken alt kata yanlış götüren işaret olup olmadığını gözle.

### Beklenen sonuç

- Bodrum ayrı servis mekânıdır; gerekli nesneler duvar/zeminle kaynaşmaz.
- Kısa sel hedefi üst kattadır; bodrum varlığı evrensel güvenli sığınak anlamına gelmez.

### Kanıt

- Merdiven inişi ve servis genel/yakın; sel yönergesi/rota çifti; insan yorumu.

### Regresyon

- Bodrum ışığı/raf/heater veya kat yönlendirmesi değişince sel mesajı da kontrol edilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryPresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-021 — Garajda model ölçeği, kapı ve yürüme alanı

**Alan:** mekan · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SERBEST / İNCELEME

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Garaj kapısı, araba, bisiklet, alet duvarı ve raf; izole QA rota oturumu.

### Adımlar

1. Bahçeden garaj kapağını açıp gerçek controller ile içeri gir; arabanın çevresinde mümkün rotayı izle.
2. Genel, alet duvarı ve metal raf yakın planlarını farklı pitch/yaw ile kaydet.
3. Arabanın dekor olduğu, araç sürüşü veya tahliye görevi sunulmadığını UI/etkileşimle incele.

### Beklenen sonuç

- Kapı rayları ve prop ölçekleri birbiriyle tutarlı; sanat geçiş yolu varmış gibi yanıltmaz.
- Garaj girişinde bunker/çit/kapı işaretleri doğru nesneye aittir; kullanılmayan sanat özelliği gerekçeli N/A kalır.

### Kanıt

- Dış giriş→garaj içi video; üç kadraj; hedef kimliği ve collider bilgi kaydı.

### Regresyon

- Garaj artı/driveway/kapak değişirse rota ve kapı çevresi yeniden yürünür.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-022 — Merdiven ve sahanlık kat değişiminin görsel devamlılığı

**Alan:** kamera-mekan · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KISA / SERBEST / SEL / YANGIN

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Üç seviye erişilebilir; root'un normal controller ile OS girdisi; hile/warp yok.

### Adımlar

1. Bodrum↔zemin ve zemin↔üst uçuşlarını iki yönde yürü; sahanlıkta yavaşla ve yana bak.
2. ActiveFloor değişiminden önce, eşikte ve sonra player/merdiven/zemin/korkuluk karelerini al.
3. Selde yukarı, yangında aşağı rehberinin hangi girişe götürdüğünü takip et; aynı yerde tekrar gir-çık yap.

### Beklenen sonuç

- Kat kesiti oyuncuyu/zemini aniden görünmez bırakmaz; fiziksel korkuluğun varlığı görselle uyumludur.
- Doğru yön ilk kez oynayan kişiye açık olmalıdır; API geçişi OS yön algısı yerine sayılmaz.

### Kanıt

- Kesintisiz iki yön video; eşik PNG üçlüsü; player/kamera/ActiveFloor kayıtları.

### Regresyon

- Kat görünürlüğü, kamera veya merdiven/kapı geometri değişince P0 geçiş grubu yeniden açılır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryPresentation.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-023 — Üç olgun ağaç bölgesi ve bahçe önlemlerinin rota hiyerarşisi

**Alan:** mekan · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SERBEST / KISA SEL

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- V4.9 üç mature-tree region; iki drenaj, üç çit, hortum istasyonu; legacy plant IDs yalnız iç kimlik olabilir. V4.8 fallback'te üç tekil fidan ayrı tarihsel profil.

### Adımlar

1. V4.9 üç bölgenin yapılmamış işaretini, tree-row kit ad/stok/tarifini, tek bölgesel0.60sn işlemi ve birkaç olgun ağaçtan oluşan sonucunu sırayla incele.
2. Her bölgede yapım öncesi/sırası/sonrası kadraj al; seed/plant/watering metni veya sulama mini oyunu kalıntısı ara.
3. Ağaç sıralarının yol, garaj/bunker, kapı ve oyuncu kapsülüne göre konumunu; drain_east/fence_east/driveway ayrımını gerçek rotada kontrol et.
4. V4.8 fallback seçilmişse yalnız kendi eski site envanteri ölçülür;3bölgesel sıra tamamlandı diye yazılmaz.

### Beklenen sonuç

- V4.9'da bölge sayısı açık3'tür; her bölge bir işlemle bir olgun sıra kurar; tek tek fidan ekme tekrarı yoktur.
- Görsel yeşillenme amacı anlaşılır ve yolları kapatmaz; ağaç işi drenajın mutlak ikamesi gibi anlatılmaz.
- Yeni API/import bekleniyorsa case PLANNED kalır; var olmayan render kabul edilmez.

### Kanıt

- Dokuz site için önce/yakın/tamamlanmış kare listesi; rota videosu; action/target eşlemesi.

### Regresyon

- Site yerleşimi veya yapım görseli değişen sitenin maliyet/erişim vakası root planında tetiklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Documentation/GDD.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-024 — Bunker kapağı, iniş ve ayrı iç sahnenin mekânsal sürekliliği

**Alan:** mekan · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SERBEST / SİNEMATİK

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Yeterli gerçek çantalı kampanya ve ayrı eksik çantalı QA koşusu; cinematic doğal/skip çiftleri.

### Adımlar

1. Bunker kapağına yaklaşım, E eylemi, kapak açılması/iniş ve iç mekânı kesintisiz kaydet.
2. İç sığınakta oyuncu, erzak kasası, sıcak ışık ve kesit duvarlarını incele.
3. Kamerada görülen sığınak ile açık dünyada yürünebilir alan iddiasını ayır; içeride helikopter/komşu mesh sızıntısı ara.

### Beklenen sonuç

- Bunker eyleminin amacı anlaşılır; iç setin görsel geçişi evden kopuk/boş veya hatalı görünmez.
- Eksik çanta kasası ve başarısızlık, hazır erzaklı başarı karesiyle çelişmez; görsel saha yeni hasar otoritesi değildir.

### Kanıt

- Kapak→iniş→iç plan video; hazır/eksik çanta karşılaştırma; kamera türü ve sahne/set adı.

### Regresyon

- Bunker, erzak önizleme veya sinematik planı değişince doğal/atla ve üç kayıp türü etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-025 — S garaj bağlantısı, çit kapısı ve ana yol

**Alan:** dis-dunya · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST / YANGIN TOPLANMA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Garaj yolu ve açık çit geçidi görünür; yeni kişi yolu önceden bilmez.

### Adımlar

1. Kişiden sokaktan garaja yürünebilir yolu ve evden yangın toplanma alanını bulmasını iste.
2. S yolu, kaldırım kesintisi, bunker kapağı ve doğu çit işi çevresini izle.
3. Kamera değiştirip görsel yolun gerçekten yürünebilen yolla uyumunu yorumla; yanlış yola gitme sayısını yaz.

### Beklenen sonuç

- Yol bunker kapağı üstünden zorunlu geçiş gibi görünmez; dekoratif asfalt/çit açıklığı yanlış rota vaadi vermez.
- Dış dünya evin bağlamını güçlendirir; kısa yangın hedefi mahalle keşfine dönüşmez.

### Kanıt

- Gerçek yol videosu; yeni kişinin rota seçimleri; bunker/çit marker çakışma karesi.

### Regresyon

- Driveway, gate, fence_east veya camera follow değişince garaj ve yangın rotaları etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicExteriorArt.cs
- TooLateToPanic/Assets/Game/Art/PanicHomeArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-026 — Dere yatağı, kıyı, menfez ve selde akış yönü

**Alan:** dis-dunya · **Öncelik:** P2 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST / SEL SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Batı çit dışındaki dere; normal ve sel sonrası aynı kamera/yön.

### Adımlar

1. Kıyı, su yüzeyi, menfez ve foam akışını genel/yakın kadrajda izle.
2. Normal→sel→yeniden başlatma görünüşlerini karşılaştır; kıyıya yapışmayan su/levitasyon/UV kaymasını işaretle.
3. Kişinin su yönünü ve sel bağlamını okuyup okumadığını sor.

### Beklenen sonuç

- Yönlü akış ve selde seviye farkı görülebilir; bu görünüm mühendislik tahmini veya CFD gibi sunulmaz.
- Su kıyıdan kopuk levha hissi verirse görsel kusur; renk tercihi ise öznel görüş kaydedilir.

### Kanıt

- Üç durum video kesiti, aynı kadraj PNG, akış yönü insan cevabı.

### Regresyon

- Stream mesh/texture/shader değişirse sel sinematiği ve sınır kadrajları da kontrol edilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicExteriorArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExteriorWorld.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-027 — Yedi komşu evin silueti ve afet izleri

**Alan:** dis-dunya · **Öncelik:** P2 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** DÖRT SİMÜLASYON / İNCELEME

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Dondurulmuş aday; yedi NeighbourSite kimliği; planlı kamera yaklaşımları yalnız kanıt yakalama için kayıtlı.

### Adımlar

1. Her siteyi ön cephe, çatı ve sokak mesafesinde kadraja al; kamera koordinatlarını kaydet.
2. Dört afet öncesi/sırası/sonrası aynı siteye dön; çatlak, çamur, yanık ve çatı izlerinin yüzeye oturmasını incele.
3. Evlerin varyasyonunu ve tekrar desenlerini rubrikle puanla; sahne gezisinin gerçek oyuncu erişimi olmadığını açık yaz.

### Beklenen sonuç

- İzler gerçek cephe/çatı düzlemine oturur; havada asılı hasar veya afetler arası yanlış renk/malzeme kalıntısı yoktur.
- Komşu hasarı dekoratif anlatımdır; oyuncunun toplam riskine ikinci kez katkı diye gösterilmez.

### Kanıt

- 7site×4afet karşılaştırma contact sheet; kaynak/site kimliği; render ve model ayrımı.

### Regresyon

- Exterior art/after math değişince yedi site otomatik örneklenir; insan alıcı puanı ayrıca yenilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicExteriorArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExteriorWorld.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-028 — Dört çevre sakininin rig ve davranış beklentisi

**Alan:** dis-dunya · **Öncelik:** P2 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST / AFET / İNCELEME

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Dört mevcut CC0 rig varyantı; afet öncesi ve afet sırası doğal akış.

### Adımlar

1. Kaldırımda yürüme/durma/dönme ve kamera mesafesindeki kıyafetleri izle.
2. Afet/sinematik başlangıcında sakinlerin görünürlüğünü ve tekrar başlangıcında geri dönüşünü kaydet.
3. Kişiye bu sakinlerden görev/konuşma/gerçek kalabalık tahliyesi bekleyip beklemediğini sor.

### Beklenen sonuç

- Sakinler retained CC0 rig/controller/klipleri korurken doğal yüz/oran/materyal/kıyafet/motion polish'i oyun içi ışıkta değerlendirilir; T-pose/kayma/ters yüz yoktur.
- Afet/sinematik sırasında dışarıda yürümeye devam etmezler; dekor figürleri yeni quest/economy veya kalabalık tahliye simülasyonu değildir.

### Kanıt

- Dört varyant video ve yakın/genel kare; kaybolma/geri dönüş zamanları; beklenti cevabı.

### Regresyon

- NPC prefab/animator/tint veya hazard görünürlük kapısı değişince dört varyant kontrol edilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicExteriorArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExteriorWorld.cs
- TooLateToPanic/Assets/Game/Art/PanicCharacterArt.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-029 — Orbit yaw/pitch ve yakınlık uçlarında tüm mekânlar

**Alan:** kamera · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SERBEST / İNCELEME

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Ortografik kamera; sağ sürükleme; PageUp/PageDown; tüm oda envanteri.

### Adımlar

1. Her odada varsayılan ve yaklaşık90/180/270yaw bakışını, 28°/65° pitch sınırlarını ve yakın/uzak zoom uçlarını örnekle.
2. Ana hedef, kapı/merdiven, oyuncu silueti ve dünya sınırının kadrajda kalmasını izle.
3. Kamera tersine döndüğünde WASD beklentisi, clipping, iç yüz, boş zemin ve foreground kapanmasını kaydet.

### Beklenen sonuç

- Kameranın izin verdiği açılar oyunun okunmasını bozmaz; ortografik kontrol ve görsel alan yönü tutarlıdır.
- Kullanılabilir açı düşük puan alıyorsa görüş; oyuncu/hedef yok oluyorsa tekrar üretilebilir kusur ayrılır.

### Kanıt

- Kadraj matrisi; input videosu; yaw/pitch/size/player koordinatları; blokaj süresi.

### Regresyon

- Camera offset/limit/cutaway değişince kat, yerleştirme ve marker yerleşimi tekrar örneklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicCameraOrbit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-030 — Kesit görünümü ve gizli katın algısal tıklanabilirliği

**Alan:** kamera · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SERBEST / KISA / İNCELEME

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Alt/üst/bodrum eşyaları tanımlı; aynı ekran ışınında farklı kat nesneleri bulunabilir.

### Adımlar

1. Üst kata çıkınca alt kat meshlerinin gizlenmesini ve fizik zeminin korunmasını görsel olarak izle.
2. Görünmeyen alt eşyanın üstündeki ekrana tıkla; açık görünen üst hedefin seçildiğini kontrol et.
3. Merdiven eşiğinde ve dışarı adım atıldığında roof/front wall kat görünürlüğünü iki yönde tekrar et.

### Beklenen sonuç

- Gizli kat eşya seçimi ve hasar etiketi oyuncuyu görünmeyen hedefe çekmez.
- Kameranın kesiti fiziksel korkuluğu kapatmış gibi bir yol vaadi üretmez; kat farkı etiketle desteklenir.

### Kanıt

- Kesit öncesi/sırası/sonrası PNG; tıklanan ekran noktası; seçili ID/kat; video.

### Regresyon

- Floor visibility/picking/render toggle değişince bütün katlar ve orbit uçları kapsanır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExteriorWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicCameraOrbit.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-031 — Materyal, gölge ve ışık tutarlılığı

**Alan:** sanat · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** TÜM MEKÂNLAR / DÖRT AFET

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Aday baked prefab/material snapshot; oyun içi ışık; stüdyo render kabul yerine geçmez.

### Adımlar

1. Ahşap, kumaş, seramik, metal, cam, ekran ve suyu genel/yakın kadrajda tek tek kaydet.
2. Mutfak, banyo, yatak/halı, PC, toolbox ve küçük vida/braket ikonlarını açık/kapalı ışık bağlamında karşılaştır.
3. Missing/pink shader, generic gri, aşırı parlama, siyah siluet, shadow acne ve transparan sıralama kusurlarını konumla kaydet.

### Beklenen sonuç

- Semantik malzeme aileleri okunur; authored renkler broad tint ile kaybolmaz; detaylar oyun içi ışıkta görünür.
- Palet tercihi ile eksik materyal kusuru ayrı tutulur. Render geçmesi renk beğenisi kabulü değildir.

### Kanıt

- Malzeme/renderer listesi; ışık ve build kimliği; referanssız oyun içi PNG; kusur closeup'ı.

### Regresyon

- Shader, bake, palette, lighting veya import değişince bu matris ve dört afet alfa kontrolü tetiklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Assets/Game/Art/PanicCuratedAssetArt.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-032 — On iki detaylandırılmış propun oyun içi kalite geçişi

**Alan:** sanat · **Öncelik:** P2 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST / KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Çanta istasyonu, kitaplık, TV konsolu, mutfak dolabı, toolbox, ecza dolabı, termosifon, metal raf, indigo koltuk, balkon sandalyesi, lavabo, alet duvarı listesi tam.

### Adımlar

1. Her propu oyuncunun gerçek mesafesi ve etkileşime yakın mesafede iki açıdan incele.
2. Konstrüksiyon, birleşim, kumaş dikişi, alet çeşitliliği, lavabonun çanağı ve menteşeye bağlı detayları değerlendir.
3. Artış gösteren üçgen sayısını kalite puanı yerine kullanma; siluet/işlev/ışık altındaki okunurluğu somut notla puanla.

### Beklenen sonuç

- Her nesne tanınır ve evin aynı sanat diline uyar; detaylar siluet veya hareketi bozmaz.
- Statik prop için rig/VFX/SFX gerekmiyorsa gerekçeli N/A; gerçekten gereken collider/etkileşim eksiği açık kalır.

### Kanıt

- 12prop×2mesafe×2açı kare listesi; rubrik 0–4; kusur/görüş/N/A ayrı sütunları.

### Regresyon

- Değişen prop ve aynı materyali/menteşe üreticisini paylaşan en az bir komşu prop yeniden değerlendirilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-033 — Sekiz malzeme ailesi, on varyant ve ikon/model eşleşmesi

**Alan:** sanat · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** LOOT / ENVANTER / ÇANTA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Su, food3varyant, firstaid, flashlight, battery, radio, blanket, extinguisher; orijinal vida/braket/tahta/bandaj/antiseptik korunur.

### Adımlar

1. Her supplyID için gerçek pickup, loot ikonu, inventory ikonu, çanta ikonu ve varsa3D önizlemeyi yan yana yakala.
2. Food varyantlarını ayrı görüntüle; aynı kaynak kimliği/adet ve stok anlamını koruduğunu denetle.
3. Radyo ızgarası, tüp işareti yönü, dip pivotu, uniform ölçek ve atlas UV'sini farklı açıda incele.

### Beklenen sonuç

- İkon ve model aynı malzemeyi anlatır; vida braketle, destek malzemesi zorunlu su/gıdayla karışmaz.
- Varyant yalnız görseldir; miktar veya afet etkisi değişmiş gibi sunulmaz. Import lisans/provenansı ayrıca korunur.

### Kanıt

- SupplyID–variant–ikon eşleme contact sheet; stok delta; asset path ve materyal kimliği.

### Regresyon

- Pickup/ikon/import değişince loot seçim, çanta hazır ve eksik malzeme UI vakaları etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicCuratedAssetArt.cs
- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-034 — Kapak ve donanım detaylarının parça hareketine bağlılığı

**Alan:** sanat · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SERBEST / LOOT

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Buzdolabı/mutfak/medicine/toolbox/locker/çamaşır/tuvalet gibi gerçek menteşeli kaynak ve dekorlar; görevli sahibin seçtiği güvenli rota.

### Adımlar

1. Her gerçek kapak türünü kapalı→yarı→tam açık→kapanır olarak videola.
2. Kulp, dikiş, panel ve iç eşyanın doğru parçaya bağlı kalmasını; duvar/diğer kapakla z-fighting ve float durumunu ara.
3. Açık/kapalı durumun oyuncuya mevcut E eylemiyle tutarlı görünmesini dene.

### Beklenen sonuç

- Hareket eden parça detayı kökte asılı kalmaz; static mesh beklenmedik bükülmez.
- Görsel açık kapak loot erişimiyle tutarlıdır; sanat onarımı menteşe/collider ölçüsünü sessizce değiştirmez.

### Kanıt

- Kapak türü bazında video; yarı açık PNG; bound/hinge kimliği; UI eylemi.

### Regresyon

- Menteşe veya prop detay değişikliğinde kapı güvenliği ve loot açık/kapalı kontrolleri birlikte ele alınır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-035 — Korunan CC0 karakterde yüz, oran, kıyafet ve rig cilası

**Alan:** karakter · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** SERBEST / İŞ / SİNEMATİK

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- SurvivorV2 mevcut CC0 kaynak; baked skinned mesh ve generated kıyafet snapshot.

### Adımlar

1. Mevcut retained humanoid/Avatar/controller/klip/root-motion kimliğini F0 snapshot'ına bağla; V4.9 art staging preview'ını native kabul sayma.
2. Yüz ve vücut oranını ön/yan/üç çeyrek; kıyafet/saç/elleri idle/walk/run/jump/build/anchor pozlarında incele.
3. Oda, dış ve bunker ışığında skin-region, kıyafet kesişmesi, kemik kopması, ölçek ve doğal silueti puanla.

### Beklenen sonuç

- Retained kaynak/lisans/rig/klip/controller/root-motion politikası korunur; doğal görünüm somut rubrikle değerlendirilir.
- Kaynak-only preview veya güzel yüz closeup'ı hareket/native kabulünü doldurmaz; tüm gövde ten materyali geri gelmez.

### Kanıt

- Poz×ışık contact sheet; animator/mesh/material kimlikleri; semptom frame numarası.

### Regresyon

- Player prefab/rig/bake/material değişince yürüme, zıplama, iş ve bunker sinematiği etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicCharacterArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-036 — 1.35 hız çarpanına uygun hareket ve animasyon doğallığı

**Alan:** karakter · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST / KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Fiziksel OS girdi; varsayılan ve180yaw kamera; gerçek controller.

### Adımlar

1. V4.9'da walk3.645m/s,run5.67m/s hedefini ölçen mekanik vakaya görsel videoyu bağla; V4.8 fallback2.7/4.2m/s olarak ayrı etiketlenir.
2. Düz zemin, kapı, merdiven, bahçede yürü/koş/dön/zıpla; gerçek1.35 hız değişimine animasyon hızının eşlik ettiğini gözle.
3. Ayak kayması, acele/komik döngü, iniş sertliği ve camera-relative kontroldeki kopmayı0–4 somut anlarla kaydet.

### Beklenen sonuç

- Hız artışı teleport veya step/slope büyütmesiyle yapılmış gibi görünmez; gerçek controller politikası korunur.
- Animasyon yeni hızla uyumludur;1.35 yalnız sayısal hareket değil algılanan doğal kontrol açısından da yorumlanır.

### Kanıt

- Kesintisiz hareket videosu; girdi/çerçeve zamanları; kişinin kontrol puanı ve örnek anı.

### Regresyon

- Animator blend/controller speed/camera yönü değişince bu dört zemin ve iş Space çatışması örneklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicCharacterArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicCameraOrbit.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-037 — Sabitleme yakın planı ve dünya inşasında uygun insan pozu

**Alan:** karakter · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** V4.9 SABİTLEME / DÜNYA İNŞASI; V4.8 FALLBACK AYRI

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- V4.9 sabitleme yakın planı ve0.60sn dünya inşası; V4.8 Ratchet/Timing/Balance yalnız açık fallback profilinde; başlangıç/iptal/final varyantları.

### Adımlar

1. V4.9 iki vidalı anchor yakın planında gerçek screw click, seating ve eldeki alet/pozu izle.
2. Dünya reinforce/tree row/fire station işlerinde0.60sn build motion ve görünür world geometry progress'i kaydet; alakasız workbench açılmasını ara.
3. Başlamadan/kurulurken/iptal/tamam sonrası karakter ve alet resetini karşılaştır; V4.8 eski rig kontrolünü ayrı profile al.

### Beklenen sonuç

- Karakter pozu gerçek eylemin yerini ve anlamını anlatır; el aleti yüzmez;0.60sn işi gereksiz mini oyunla uzatmaz.
- Yakın rig yalnız anchor için sözleşmeye uygun kullanılır; build pose/geometry gerçek final commit'ten önce tamamlanmış önlem vaadi vermez.

### Kanıt

- Dünya+work texture iki görüntü; girdiyle senkron video; cancel/complete sonrası poz.

### Regresyon

- Task pose/tool/work rig değişince ilgili aile ve bir başka aile ortak reset kontrolüne girer.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicCharacterArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Art/PanicWorkArt.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-038 — Odak, küçük pickup hareketi ve .30sn toplama yayı

**Alan:** geri-bildirim · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KISA / SERBEST LOOT

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Açık gerçek kaynak; birden fazla stok; input hızlı tekrarlanabilir; collider/pick otoritesi üretimde.

### Adımlar

1. Tek küçük su/pil/vida seç; turuncu odak ve hafif hareketi kaydet; büyük mobilyanın zıplamadığını gözle.
2. Tek F ve hızlı F serisiyle pickup yayını/çantaya akışı videola; kaynak, envanter ve model adetlerini eşle.
3. Çanta dolu/kapalı/geçersiz kaynakta başarısız alımın aynı başarı uçuşunu üretip üretmediğini dene.

### Beklenen sonuç

- Geri bildirim küçük ve kısa kalır; toplama animasyon kuyruğunu bekletmez ve statik hitbox'ı oynatmaz.
- Görsel kazanım gerçek atomik alıma bağlıdır; başarısız alım kazanım gibi görünmez.

### Kanıt

- Alımdan önce/sıra/sonra üçlü; input/video; kaynak-envanter delta; uçan kozmetik sayısı.

### Regresyon

- Odak/pickup visual veya inventory otoritesi değişince loot, kapasite ve snapshot reset vakaları etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicRefinement.cs
- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-039 — Başarılı sabitleme donanımı ile yarım/iptal işi ayır

**Alan:** geri-bildirim · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KISA / SERBEST

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Sabitlenebilir mobilya; yeterli gerçek malzeme; aynı hedefte yeni koşular.

### Adımlar

1. V4.9'da uygun uzun/dengesiz mobilyayı seç; gerçek iki screw click öncesi görünüşü kaydet; V4.8 fallback eski iş yolunu ayrı uygular.
2. Birinci vida seating veya ikinci vida öncesi iptal et; yeni koşuda iki vidayı bitir; sök/yeniden sabitle sonucunu gör.
3. Braket/kayışın yerleşmesini model Anchored ve kesin maliyetle eşle; busy spam'in ek donanım/başarı üretmesini ara.

### Beklenen sonuç

- Yalnız tamamlanan gerçek önlem güvenli görünüş oluşturur; iptal edilende tam donanım kalmaz.
- Duvar/zemin ankrajı uygun konuma oturur; kısa büyüme efekti ikinci başarı veya ücretsiz önlem üretmez.

### Kanıt

- Önce/yarım/iptal/tamam/sök/yeniden kareleri; Anchored ve maliyet delta; video.

### Regresyon

- Hardware/art/state sync değişince model atomiklik ve hero deprem tepkisi birlikte örneklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Art/PanicAttachmentArt.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-040 — İki eşyalı deprem karşılığını oyuncu okuyabiliyor mu

**Alan:** neden-sonuc · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA DEPREM

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Bir gerçek sabitlenmiş ve bir sabitlenmemiş kahraman eşya; farklı gerçek hasar olasılığı açık; hile yok.

### Adımlar

1. İşten önce iki eşyayı ve bağını kaydet; doğal deprem sırasında her iki görsel tepkiyi izlet.
2. Sonuçta gerçek anchored/damage/contribution değerlerini ayrı teknik kayda al.
3. Oturum sonunda 'Hangi nesne neden farklı davrandı, bu kesin koruma mı?' sorusunu yönelt.

### Beklenen sonuç

- Tepki gerçek anchored ve önceden hesaplanmış hasarla tutarlıdır; görsel ikinci hasar hesabı olmaz.
- Kişi önlemin risk düşürdüğünü anlayabilir; sabitsiz her eşyanın mutlaka devrildiği veya sabitlinin hasarsızlığı garantisi verilmez.

### Kanıt

- Aynı kamera önce/sıra/sonra; gerçek outcome snapshot; yönlendirilmemiş cevap.

### Regresyon

- Hero IDs/amplitudes, anchor feedback veya hasar kopyası değişince iki eşya birlikte yeniden incelenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-041 — Deprem toz, çatlak, enkaz ve sarsıntı katmanları

**Alan:** afet-sanat · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** DEPREM KISA / SİMÜLASYON / KAMPANYA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Doğal deprem; aktif kat ve dış planlar; aynı önce-sonra kamera.

### Adımlar

1. Toz, çatlak ve enkazı tüm doğal planlarda genel/yakın izle.
2. Üst kat gizliyken havada global çatlak şeridi, opak parçacık kartı ve player/rota örtülmesi ara.
3. Farklı hasar sonucunda görsel şiddet ve sonuç metnini karşılaştır; sarsıntı rahatsızlığı kaydet.

### Beklenen sonuç

- Afet deprem olarak anlaşılır; gizli katla bağsız çatlak veya sürekli kritik örtüşme yoktur.
- VFX dramatikliği model hasarı diye okunmaz; gerçek hasar sayısı efekt yoğunluğundan türetilmez.

### Kanıt

- Doğal zamanlı video; plan başlangıç/tepe/bitiş kareleri; hasar snapshot; rahatsızlık notu.

### Regresyon

- DisasterArt/SoftWeather/camera shake değişince hem bu vaka hem hareket hassasiyeti örneklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicDisasterArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-042 — Sel su, yağmur, köpük ve drenaj kadrajı

**Alan:** afet-sanat · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SEL KISA / SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Drenaj öncesi/sonrası eşlenmiş koşular; ev/dere/dış kapı planları.

### Adımlar

1. Su seviyesi, dış yağmur, cam izleri, kapı/drenaj splash, köpük ve sürüklenen parçaları izle.
2. Kameranın sel detay planında gerçek drenaj veya dere ilişkisini gösterip göstermediğini kaydet.
3. İncelemede çamur/su kalıntısı ve üst kat güvenli noktasını değerlendir; bodrum yönergesine dikkat et.

### Beklenen sonuç

- Sel yangın/deprem fallback gibi görünmez; seviye ve akış yönü okunur; opak dikdörtgen yoktur.
- Drenaj gerçek önlemdir ama tüm suyu sihirli yok eden tek çözüm gibi sunulmaz.

### Kanıt

- Önce/sıra/sonra video+PNG; drain state; kullanıcı afet ve önlem yorumu.

### Regresyon

- Su/foam/shader/drenaj veya sel kadrajı değişince creek ve yönlendirme testleri birlikte etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicDisasterArt.cs
- TooLateToPanic/Assets/Game/Art/PanicExteriorArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-043 — Yangında pencere dumanı, alev ve incelemeye temizlik

**Alan:** afet-sanat · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** YANGIN KISA / SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Doğal yangın; üst pencere detail shot; erken atlama ve doğal bitiş iki ayrı QA koşusu.

### Adımlar

1. Genel alev/duman/kıvılcımı ve üst pencere/saçak dumanını doğal sürede kaydet.
2. İncelemeye geçince aktif alev/duman/üst kat yanık meshinin temizlenmesini; zemindeki gerçek izlerin kalmasını izle.
3. Üst kata çıkıp farklı yaw/pitchlerde asılı VFX/yanık/etiket ara; öteki afete geçince kalıntıyı tekrar kontrol et.

### Beklenen sonuç

- Yangın kadrajının kaynağı evle mekânsal bağlıdır; yumuşak alfa görünür, opak kartlar oluşmaz.
- İnceleme yürünebilir ve okunabilir; geçmiş hasar izi ile halen aktif tehlike görseli karışmaz.

### Kanıt

- Doğal+skip video; pencere detail start/mid/end; inceleme kat kareleri; outcome/phase.

### Regresyon

- Fire emitter/scorch/phase cleanup veya cinematic shot değişirse doğal ve atla yolları yeniden değerlendirilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicDisasterArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryPresentation.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-044 — Fırtınanın ayrı görsel/ses kimliği ve çatı amacı

**Alan:** afet-sanat · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** FIRTINA SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Fırtına seçili; gerçek sonlu önlemler; doğal afet ve gerçek ağır hasar ihtimali.

### Adımlar

1. Rüzgâr, eğik yağmur, yaprak/savrulan parça, çatı/pencere/kapı kadrajını izle.
2. Ses açık ve sessiz iki gözlemde afeti adlandırmasını iste; yangın fallback hissi veya opak kare ara.
3. Kayıp ekranından gerçek hasar incelemesine dön; çatı kat etiketini ve komşu çatı izini değerlendir.

### Beklenen sonuç

- Fırtına dört seçimden ayrı, anlaşılır bir türdür; yağmur sel eylemini zorunlu kılmış gibi karışmaz.
- Çatı/kapı/eşya önlemleri amaçla uyumludur; ağır hasar başarıya dönüştürülmez; kısa akışa ek zorunlu bölüm olmaz.

### Kanıt

- Doğal fırtına video; ses açık/kapalı afet cevabı; çatı kareleri; terminal hasar snapshot.

### Regresyon

- StormArt/Storm audio/risk hints veya simulation selector değişirse bu vaka ve dört mod dönüşü etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Art/PanicStormArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationHud.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-045 — Afet sonrası izlerin reset ve kalıcılık tutarlılığı

**Alan:** afet-sanat · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** TÜMÜ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Dört afetin önce/sonra durumları; yeni dalga, retry ve title geçişleri için izole örnekler.

### Adımlar

1. Hasar izlerini gerçek model sonucu ve floor visibility ile eşleştir.
2. Aynı afet retry, diğer afet seçimi, başlığa ve serbest oyuna dönüşte önceki su/yanık/komşu hasarı/helikopter kalıntısını denetle.
3. Kampanyada kalıcı önlem/eşya yerleşimi ile sıfırlanan hasarı görsel olarak ayrı kaydet.

### Beklenen sonuç

- Yeni koşu eski afetten kirli görünmez; kampanyada kurulan önlem kaybolmazken dalga hasarı resetlenir.
- Kozmetik restore model state'i veya collider/loot otoritesini değiştirmez.

### Kanıt

- Geçiş matrisi; önce/sonra render; model state/aktif efekt envanteri.

### Regresyon

- Reset/phase observer/visibility değişirse tüm afet ve mod çıkışlarından temsilî natural/skip çiftleri seçilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryPresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExteriorWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-046 — Güncel kısa ses kapsamı ve kampanya sessizlik politikası

**Alan:** ses · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** KISA / KAMPANYA / SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Snapshot; Resources Audio/Jury ve Deferred/Radio-legacy envanteri; lisans kayıtları.

### Adımlar

1. Yedi konuşma ve üç kısa SFX'nin hangi gerçek olaya bağlandığını çıkar; id/metin/öncelik/duration listesini rubriğe ekle.
2. Kısa akışa opt-in açılma ve başlığa/serbest oyuna kapanmayı izleyecek kontrol noktalarını tanımla.
3. Eski Tolga radyo ve genel konuşma kaynaklarının aktif paket/oynatma yoluna dönmediğini kaynakta denetle.

### Beklenen sonuç

- Seçili kısa akış konuşmaları güncel yetkilendirmedir; V4.9 station/tree/direct-action hedefleriyle artık yanlış hale gelen cue varsa metin/olay uyum bulgusu açılır.
- Normal kampanya konuşması ertelenmiştir; lisanslar korunur; yeni sağlayıcı işi/harcama planın önkoşulu değildir.

### Kanıt

- Cue/event tablosu; aktif/deferred asset paths; lisans/provenans referansı; eksik clip listesi.

### Regresyon

- Her cue, AudioSource veya mode configure değişiminde kısa/kampanya/simülasyon sınırları yeniden incelenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicPresentationAudio.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-047 — İnsanla Türkçe telaffuz, tını ve dengeli miks dinleme

**Alan:** ses · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA / DÖRT AFET

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Sessiz ortam; hedef hoparlör ve bir kulaklık; güvenli dinleme seviyesi katılımcı seçer; gerçek aday sesleri.

### Adımlar

1. Yedi konuşmayı gerçek tetik anlarında bir kez dinlet; anlaşılmayan kelime, vurgu, yapay kesim ve acele hissini yaz.
2. Afet sesi, konuşma ve3SFX aynı anda olduğunda konuşma anlaşılırlığını ve korkutucu/rahatsız edici ani tepeyi gözle.
3. Konuşma tınısı beğenisi ve teknik clipping/sessiz clip/bozuk kayıt kusurunu ayrı puanla.

### Beklenen sonuç

- Teknik clip/import kontrolü insan işitmesi yerine kullanılmaz.
- Türkçe amaç cümlesi anlaşılır, ses olayla eşzamanlıdır; beğeni sonucu model/tema kusuruyla karıştırılmaz.

### Kanıt

- Cihaz/OS ses seviyesi; cue bazlı ham dinleme notu; gerçek mix kayıt örneği; telaffuz metni.

### Regresyon

- Clip değişirse kendisi ve hemen öncesi/sonrasındaki konuşma, afet ducking ve altyazı tekrar değerlendirilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicPresentationAudio.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- TooLateToPanic/LATEST-BUILD.json
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-048 — Konuşma önceliği, altyazı süresi ve kesinti sonrası bağlam

**Alan:** ses-ui · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Yedi cue tetikleyen gerçek rota; pause/F2/inventory ve hızlı pickup/iş tamamlanması örnekleri.

### Adımlar

1. Konuşma sürerken daha yüksek öncelikli sonucu tetikle; eski cümlenin gecikmiş kuyruğa düşmesini dinle.
2. Altyazıyı başlangıç/bitiş/iptal halinde kaydet; duraklatıp devam et ve moda çık.
3. Hızlı pickup/anchor sonrası tek SFX kanalı ve ducking'in anlaşılır olup olmadığını değerlendir.

### Beklenen sonuç

- Aynı anda tek konuşma vardır; eski hedef konuşması yeni görev üzerine sonradan dönmez; aynı cue koşuda gereksiz tekrarlanmaz.
- Altyazı duyulan/güncel cümleye uyar, panel/uyarı ile çakışmaz; ses oyuncuyu bekleten kapı değildir.

### Kanıt

- Cue/event zaman çizelgesi; sistem sesi dahil video; subtitle frame üçlüsü; öncelik/reset kaydı.

### Regresyon

- Audio queue/priority/pause veya subtitle yeri değişince bu kesintiler ve sessiz oynama etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicPresentationAudio.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryPresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-049 — Sessiz oynama ile bütün amaç ve sonuçlar

**Alan:** erisilebilirlik · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA / DÖRT SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Normal OS sessize alınmış; oyun ayarında var olmayan mute düğmesi varsayılmaz; altyazı/render açık.

### Adımlar

1. Yeni katılımcı kısa akışı ses duymadan oynar; görev değişimi ve kazanımları videola.
2. Dört afetin kimliğini, uyarıyı, koruma sonucunu yalnız görseller/metinle nasıl çıkardığını sor.
3. Sessize alınmış oturumdan sonra gerekli bilgi kaybı ile atmosfer beğenisi kaybını ayır.

### Beklenen sonuç

- Önemli eylem, kaynak, deadline ve gerçek sonuç yalnız konuşma/SFX'ye bağlı değildir.
- Sessiz deneyim temel döngüyü anlaşılır tutar; bütün cümleyi kaçırmak hedefi kilitlemez.

### Kanıt

- OS mute durumu; kesintisiz video; kaçırılan bilgi listesi; sesli oturumla yalnız bağımsız nitel karşılaştırma.

### Regresyon

- Ses/metin/uyarı değişiminde sessiz rota en az ilk180sn ve sonuç için yeniden örneklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicPresentationAudio.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-050 — Bütün doğal sinematik planlarında alıcı kadrajı

**Alan:** sinema · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KAMPANYA / SİMÜLASYON / KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Rubrikteki C01–C18 shot envanteri; her doğal yol için gerçek state; zaman hızlandırma yok.

### Adımlar

1. Disaster yaklaşım/kapak/genel/detail, Aftermath iki plan, bunker Success, üç ayrı failure planını doğal sürede kaydet.
2. Dört afetin özel planlarında özne, neden-sonuç, ışık, foreground, hareket yönü ve cut sınırını puanla.
3. İç planlarda helikopteri gizleme, dış aftermath'te rotor/arama ışığını gösterme ve controller'a geri dönüşü gözle.

### Beklenen sonuç

- Her plan oyuncuya ilgili afet ve sonuç hakkında somut bilgi verir; boş duvar/kamera içi/zemin altı veya alakasız set yoktur.
- Süreler uzatılmadan sakin, anlaşılır planlar oluşur; VFX ile sonuç sayısı çelişmez.

### Kanıt

- Shot-ID start/mid/end PNG + kesintisiz video; phase/age/camera koordinatları; rubrik puanı.

### Regresyon

- Cinematic path/prop/visibility veya hazard art değişince ilgili shot ve iki komşu cut yeniden izlenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-051 — Her doğal ve atlanmış planın aynı gerçek sonucu

**Alan:** sinema · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** TÜM SİNEMATİK YOLLAR

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Eşlenmiş izole profiller ve aynı hazırlık durumu; biri doğal, biri başlangıç/orta/sona yakın Enter/tık ile atla.

### Adımlar

1. Kampanya ve dört simülasyonda Disaster/Aftermath/Success/failure, kısa deprem ve sonuç yollarını gerçek girdilerle geç.
2. Atlamadan önce ekrandaki vaat, atlama anı ve ilk tekrar kontrol alınan kareyi kaydet.
3. Gerçek hasar/tüketim/ödül/skor/terminali eşlenmiş koşuyla karşılaştır; tuşun sonraki paneli de yanlışlıkla geçmesini ara.

### Beklenen sonuç

- SAHNEYİ ATLA gerçek sonucu bir kez çözer; kayıp başarıya dönüşmez ve tüketim yinelenmez.
- Kamera/karakter/girdi geri gelir; doğal yolu görmemiş kişi de karneyle ne olduğunu anlayabilir.

### Kanıt

- OS input video; doğal-atla outcome diff; kontrol geri dönüş süresi; stage/event sayısı.

### Regresyon

- Skip/HUD Enter/phase observer değişince çift çözüm ve same-frame atlama kontrolleri P0'dır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-052 — Süre, erzak ve ağır hasar ayrı ve önce açıklanır

**Alan:** kayıp-anlatımı · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KAMPANYA / SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Üç gerçek kayıp nedeni; hile kapalı; kısa akış sonucu ayrı tutulur.

### Adımlar

1. Süre kaybında ilk loss reveal karelerini, sonra failure cinematic ve kararlı terminali kaydet.
2. Eksik çanta ve >%65 hasarda aynı kayıt üçlüsünü üret; simülasyondaki metin farkını incele.
3. Gönüllüye sonuçtan sonra neden kaybettiğini sor; teknik failure reason ile eşle.

### Beklenen sonuç

- Süre kaybı başarısızlık sinemasından önce görünür; erzak ve hasar nedeni aynı metne düşmez.
- Simülasyon 'Afeti Başlat' amacıyla tutarlı kalır; doğru kayıp korunur ve inceleme seçeneği yanıltıcı ödül üretmez.

### Kanıt

- Loss first-frame video; reason/TotalDamage/BagReady; terminal butonları; insan neden cevabı.

### Regresyon

- Failure copy/phase/skip/simulation endcard değişince üç neden ve NoLoss karşılığı yeniden incelenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-053 — Başlık boş/dolu yerel skorlar ve menü odağı

**Alan:** ui · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** BAŞLIK

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Ayrı boş skor fixture ve çok kayıtlı izole fixture; gerçek skor dosyası dokunulmaz.

### Adımlar

1. Başlıkta beş seçeneği mouse ve ok/W/S/Enter ile gez; görünür seçim, hover ve arka ev kadrajını kaydet.
2. Boş tabloyu ve1/6 kayıtlı uzun sayı/tarih varyantlarını görüntüle.
3. Title→geri→title ve modal kapama sonrasında seçimin nerede kaldığını izle.

### Beklenen sonuç

- Ana çağrılar kesilmez; yerel skorun bu bilgisayarda saklandığı anlaşılır; uzun sayı/tarih taşmaz.
- Kısa/simülasyon/hileli koşu gerçek skor kaydı vaat etmez; UI ve dünya aynı input'u paylaşmaz.

### Kanıt

- Boş/dolu başlık PNG; keyboard/mouse video; profil identity ve skor hash önce/sonra.

### Regresyon

- Başlık yerleşimi/font/score format değişince düşük çözünürlük ve seçim testi etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-054 — Dört afet seçim kartı ve başlangıç açıklaması

**Alan:** ui · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SİMÜLASYON SEÇİCİ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Başlıkta selector açılmış; snapshot ve gerçek input.

### Adımlar

1. Deprem/Sel/Yangın/Fırtına kartlarını fare ve ok/Enter ile seç; geri Esc/Backspace/tık yolunu uygula.
2. Her kartın amaç ipucunu, hazır çanta/sonlu malzeme/skor dışı satırını farklı çözünürlüklerde kaydet.
3. Seçimden sonra gerçek90sn, afet adı ve başlangıç çantasını kontrol et; geri dönüp diğerini seç.

### Beklenen sonuç

- Dört kart aynı okunabilir hiyerarşide; Fırtına ayrı seçimdir ve yanlış tipe geçmez.
- Hazır başlangıç ve skor dışı olma seçimden önce anlaşılır; geri girişi yanlış yeni koşu başlatmaz.

### Kanıt

- Dört kart PNG+seçim video; selected type/model start state; yanlış input notları.

### Regresyon

- Selector copy/order/hints değişince dört start/retry ve title dönüşü yeniden örneklenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicSimulationHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-055 — Dalga, faz, saat, bütçe ve çanta durum hiyerarşisi

**Alan:** ui · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KAMPANYA / KISA / SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Üç modda başlangıç, son60sn, work/inventory açık ve afet state örnekleri.

### Adımlar

1. Kişiden acele etmeden hangi afet, ne kadar süre ve neyin eksik olduğunu söylemesini iste.
2. Saat kartındaki BU BÖLÜM/GEÇTİ ile faz saati ayrımını kısa akışta incele.
3. Bütçe dolar gösterimi, su/gıda görseli, destek ve alarmın birbiriyle yarışmasını değerlendir.

### Beklenen sonuç

- Ana karar bilgisi ilk bakışta okunur; kaynak kartı hedefi gizlemez; sayılar gerçek modelle tutarlı kalır.
- 270/300 sunum süresi ile kampanya300/270/240 hazırlık süresi aynı kural gibi anlatılmaz.

### Kanıt

- Durum örneği PNG; beş saniyelik bilgi okuma cevabı; model/HUD karşılaştırma; rubrik.

### Regresyon

- HUD kartı/font/time formatting değişince üç mod ve low-res kapsamı etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/LATEST-BUILD.json
- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-056 — Bağlam kontrolleri, H yardım ve dünya hedefleri

**Alan:** ui · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** TÜM OYNANIR MODLAR

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Kapı/kapak/loot/bahçe/ankraj/placement gibi hedef türleri; H yardım state.

### Adımlar

1. Seçili F0 profilin ortak ItemActionRules/capability matrisini çıkar; uzun/dengesiz mobilya ankrajını, structure reinforce'ı ve container/move ayrımını örnekle.
2. Her hedefte o an sunulan eylemi UI'dan seçmesini izle; desteklenmeyen eylemin mouse/keyboard/API'da gizlenip reddedildiğini mekanik kanıtla eşle.
3. V4.9'da eski A/D/Timing/Balance, generic bed/sofa extinguisher veya plant/water hint kalıntısını ara; V4.8 eski ipuçlarını yalnız fallback altında değerlendir.

### Beklenen sonuç

- Görünür eylem asset'e uygundur; desteklenmeyen eylem yalnız UI'dan gizlenip klavyeden çalışmaz.
- Oyuncuya tek güncel ana hedef gösterilir; yeni sözleşmeye aykırı eski tuş/iş dili kullanılmaz.

### Kanıt

- Hedef türü/IPUCU eşleme tablosu; insan yanlış tuşları; H açık/kapalı PNG.

### Regresyon

- ContextControls/marker/help copy değişince ilgili eylem ve keyboard erişim vakaları tekrarlanır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicRefinement.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryPresentation.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-057 — Kaynak şeridinin kapalı, dolu, seçili ve boş durumları

**Alan:** ui · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** LOOT

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Açılabilir farklı stoklu kaynaklar; boşaltılabilir sonlu stok; ayrı full inventory örneği.

### Adımlar

1. Kapalı kaynağı E aç; Z/X veya fare ile her gerçek3D malzemeyi seç, F ile al.
2. Son stok alınınca BOŞ/Malzemeler alındı durumunu ve yalnız kapatma eylemini gözle.
3. Dolu envanterde almayı dene; sayı, seçili ikon, toast ve yay geri bildirimini kontrol et.

### Beklenen sonuç

- Kaynak adı, seçili malzeme/adet ve eylem açık; tükenmiş stok hayalet ikon üretmez.
- Gerçek tıklanabilir içerik ve HUD seçimi eşleşir; başarısız alım görünür ama kazanım diye anlatılmaz.

### Kanıt

- Kapalı/dolu/seçili/son stok/boş/dolu envanter frame seti; gerçek input ve stok delta.

### Regresyon

- Loot tray/DrawLootStrip/selection değişince ilk su, icon mapping ve empty source vakaları etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-058 — 12 envanter ve8 çanta; üretim ve tüketimin görünür sonucu

**Alan:** ui · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** TÜM MODLAR

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Kısmen dolu/dolu/boş envanter-çanta örnekleri; craft için gerçek2bandaj+1antiseptik; test profili.

### Adımlar

1. Tab/B aç, ok/fare seç, Enter çantaya koy, F üret, Backspace/Tab/Esc kapat.
2. Çanta hazır/eksik ipucunu kısa/simülasyon/kampanya metinleriyle karşılaştır; gerçek consumed slotları afet sonrasında gör.
3. Dolu kapasite, eksik craft ve boş seçim hata geri bildirimlerini oku; stock/model sayılarını eşle.

### Beklenen sonuç

- İki alan ve kapasite farkı açıktır; çanta hazır olma hedefi moda uygundur; UI başlıkları yanıltmaz.
- Üretim veya afet tüketimi yalnız sayaçta değil slotlarda da görünür; temel kapasite kuralı değişmez.

### Kanıt

- Önce/son slot PNG; girdi videosu; inventory/bag/model delta; metin mode matrisi.

### Regresyon

- Inventory/craft/bag sync değişince tüketim, deadline bag failure ve ilk kişi su adımı etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-059 — Eksik malzeme kartı somut sayı ve doğru ikon verir

**Alan:** ui · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** İŞ / ÜRETİM EKSİĞİ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Her tarifte en az bir eksik ve kısmen yeterli stok; normal bütçe/hile kapalı.

### Adımlar

1. Seçili profile göre ankraj, tier, bariyer, drenaj, çit, hortum, dedicated fire station ve tree-row kit tariflerini eksik stokla dene; craft eksiğini de örnekle.
2. VAR/GEREK/EKSİK ve gerçek item/region kit ikonunu shared recipe/capability kararıyla karşılaştır.
3. Gerçek malzemeyi alıp canlı güncellemeyi ve inventory kart yerleşimini izle; V4.9'da seed/sapling eski isim kalıntısını kaydet.

### Beklenen sonuç

- Kesin eksik sayı, doğru ikon ve asset'e uygun iş adı görülür; bölgesel kit gerçek tarif/stok anlamını korur.
- UI, direct action validation ve final commit aynı maliyeti anlatır; unsupported action için yanıltıcı satın alma/eksik kartı açılmaz.

### Kanıt

- Tarif×eksik PNG listesi; gerçek requirement/model sayıları; pickup sonrası canlı değişim videosu.

### Regresyon

- MaterialCosts/requirements/action-eligibility/card/font değişince V4.9 screw/world-install/station/region-kit ve craft etkilenir; eski üç iş ailesi yalnız fallback profilindedir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-060 — İki gerçek vidaya tıklama, seating ve okunur hit alanı

**Alan:** ui-direkt-sabitleme · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS; V4.8_FALLBACK AYRI

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- V4.9 ClickWorkTarget/WorkTargetViewport ve iki gerçek screw transform hedefi root intake'inde mevcut olmalı; yoksa execution bekler.
- Uygun uzun/dengesiz mobilya; normal sonlu malzeme; keyboard/mouse novice.

### Adımlar

1. Yalnız UI ve yakın3D düzenekle iki açık gerçek vidayı tıklamasını iste; boş yüzeye/yanlış/aynı/tamamlanmış vidaya tıkla.
2. Kabul edilen click'in0.60sn seating'ini ve busy anında spam/ikinci vida girişini videola; hedefin hangi vida olduğunu işaretle.
3. Birinci/ikinci seating sırasında iptal ile tamamlanmayı ayrı örnekle; iki vida bitince tek final maliyet ve Anchored durumunu eşle.
4. Eski A/D hizalama/timing/hold/direction girdilerinin yeni iş bitiş yolu olmamasını incele; fallback'te eski vaka ayrı çalıştırılır.

### Beklenen sonuç

- V4.9'da iki gerçek vida görsel ve hitbox olarak bulunur; boş alana tıklamak ilerletmez; tıklanan vida otomatik oturur.
- Busy/invalid/repeat girdi vida atlamaz, çift tüketmez veya erken başarı göstermez; final tamamlanma maliyeti yalnız bir kez işler.
- Talimat gerçek click eylemini anlatır; input kutuları/rig eski yön mini oyununu zorunlu kılmaz.

### Kanıt

- Vida0/1 viewport ve hit alanı; raw yakın plan PNG; OS click/video; accepted/invalid/busy event sayısı; model maliyet diff.

### Regresyon

- Anchor closeup/HUD/raycast/seating değişince iki vida, spam, cancel, exact cost, hero reaction ve gerçek ilk180sn etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-061 — 0.60sn dünya işlemi, tier geometrisi ve yapım pozu

**Alan:** ui-dunya-insasi · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS; V4.8_FALLBACK AYRI

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Wall/window/door/roof reinforce ve garden region adapters seçili snapshot'ta doğrulanmış; hileler kapalı.

### Adımlar

1. Uygun structure'da reinforce başlat; dünya kadrajını terk etmeden0/orta/final anını kaydet; target0 direct-start adapter yolunu mekanik kayda bağla.
2. Wall/window tier-specific gerçek geometry ve karakter build motion'un0.5–0.7sn(target0.60) içinde assembly'sini izle.
3. Kapı/pencere açıklıkları, aktif menteşe, cutaway, damage ve stage reset görünüşünü karşılaştır; eski unrelated workbench'e gidişi ara.
4. İptal halinde geometry/progress/maliyet state'ini; final halinde kesin tier ve maliyeti kaydet.

### Beklenen sonuç

- V4.9'da yapı güçlendirme asset üstünde kısa ve anlaşılır kurulur; renk değişimi gerçek tier geometry yerine geçmez.
- Kapı/pencere boşluğu ve colliderları görselle uyumlu korunur; işlem mini oyun açarak kısa süre bütçesini büyütmez.
- Fallback Timing/Ratchet beklentileri yeni build için gate olmaz; adapter eksikse PENDING_IMPLEMENTATION kalır.

### Kanıt

- World action start/mid/final raw PNG+video; elapsed/install/tier state; geometry listesi; openings/hinge/cutaway karşılaştırma.

### Regresyon

- Construction factory, tier state/progress veya build pose değişince kapı/pencere geçişi, cancel/no-charge, tree rows ve kısa süre etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs
- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-062 — Zemin kattaki gerçek yangın hazırlık istasyonu

**Alan:** ui-yangin-istasyonu · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** V4.9_DIRECT_ACTIONS KISA / SİMÜLASYON / SERBEST

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Dedicated ground-floor station root integration'ında konum/kaynak/action kimliğiyle tanımlı; inventory'de gerçek extinguisher.

### Adımlar

1. Kısa yangın aşamasında heading/hint/marker'ın alt kattaki istasyona yönelmesini takip et; üst kat yatak/koltukta tüp eylemi ara.
2. Gerçek envanter tüpüyle istasyonda kısa contextual direct install başlat; build pozu ve0.60sn progress'i kaydet.
3. Tüp eksik/iptal/başarı tekrar denemesinde gerçek FireReadiness, tüketim ve feedback'i eşle.
4. İstasyon sonrası dış toplanma yolunu yeni oyuncuya takip ettir; bu eylemi hazırlık mı aktif yangına müdahale mi sandığını sor.

### Beklenen sonuç

- Tüp yatağa/koltuğa generic protection olarak takılmaz; unsupported mouse/key/API yolları reddedilir.
- İstasyon gerçek inventory extinguisher tüketir ve gerçek fire readiness durumuna bağlıdır; görsel/stage tamamlanması birlikte olur.
- Yönerge önlem hazırlığı ve sonra dışarı çıkışı anlatır; tehlike içinde gereksiz bekleme/mini oyun mesajı üretmez.

### Kanıt

- StationID/konum, upper→station→exit OS video, önce/sıra/sonra frame, inventory/readiness/cost diff, insan yorumları.

### Regresyon

- Station/action rules/hints/marker veya fire state değişince short270sn, four simulation, unsupported bed/sofa ve gerçek tüketim etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-063 — Taşıma önizlemesi, döndürme ve geçersiz neden

**Alan:** ui-yerlesim · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SERBEST / İNCELEME UYGUN STATE

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Taşınabilir sabitsiz eşya; fiziksel engeller; aynı kat ve diğer kat hedef ekran noktaları.

### Adımlar

1. Kısa sağ tık/4 ile taşıma başlat; yeri değiştir; sol tık/Enter bırak ve teker/R döndür.
2. Geçerli, duvar/kapı/merdiven/aktör/başka kat/HUD üzeri geçersiz örnekleri kaydet.
3. Sağ tık/Esc/Backspace iptal ve basılı sağ sürükleme orbit ayrımını deneyim açısından izle.

### Beklenen sonuç

- Ayak izi ve hata metni yerleştirilebilirliği birlikte anlatır; sadece kırmızı/yeşile dayanmaz.
- Yanlış click dünyada gizli işlem yapmaz; iptalde eşya/loot yeri tutarlıdır; kamera ve taşıma jesti tek eylem sahiplenir.

### Kanıt

- Geçerli/invalid reason matrisi; OS gesture video; önizleme ve gerçek poz delta.

### Regresyon

- Placement/orbit/input/GUI region değişince bütün geçersiz nedenler ve iki jest tekrarlanır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs
- TooLateToPanic/Assets/Game/Runtime/PanicCameraOrbit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-064 — Mola ve F2 ekranının zaman/skor açıklaması

**Alan:** ui-modal · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** BAŞLIK / KAMPANYA / KISA / SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Dört mod; hileler varsayılan kapalı; izole skor hash; önceki pause durumları biliniyor.

### Adımlar

1. Esc ve F2'yi farklı sırada aç/kapat; menüde görünen saat, toggle ve önceki pause dönüşünü videola.
2. Yalnız menüyü aç ve kapat; sonra bir seçenek/+1000 kullanıp tekrar kapat.
3. Kısa akışta 'süre devam ediyor', kampanyada 'oyun durdu', NoLoss ve sticky skor dışı metnini modelle eşle.

### Beklenen sonuç

- Menü açmak hile kullanımı gibi sunulmaz; kullanımdan sonra kalıcı skor dışı durumu görünürdür.
- Süre durdurma, kısa akış dış saatini uzatacağı vaadi üretmez; seçenekler gerçek hasarı/erzak tüketimini gizlemez.

### Kanıt

- Mode×modal PNG; gerçek saat ve CheatedRun before/after; input odağı video; skor hash.

### Regresyon

- Dev/pause copy, modal priority veya clock gate değişince score/NoLoss ve300sn sınır vakaları P0 öncelikle ele alınır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicDeveloperOptions.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-065 — Kısa akışın her aşamasında hedef ve panel varyantları

**Alan:** ui-kisa · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** KISA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Onboarding,Preparation,Earthquake,EarthquakeResult,Flood,Fire,Inspection,Result,Complete ayrı snapshotlar; gerçek erişim yolu sonradan OS doğrulanır.

### Adımlar

1. Her kısa aşamayı selected F0 source snapshot'ından renderla; su/ankraj/drain/fire completion alt durumlarını al.
2. V4.9'da iki vida closeup, world install, ground-floor fire station ve region tree state'leri gerçekten erişiliyorsa atlasla; eksik implementasyonu render kabulü diye doldurma.
3. Üst-alt kat/dış bahçe yönlendirmesi ve inventory/pause/F2/subtitle overlay modal önceliğini incele; V4.8 work atlasını fallback olarak sakla.

### Beklenen sonuç

- Güncel heading/hint/target sözleşmenin gerçek eylem ve konumuna bağlıdır; yatak yakınında generic tüp veya eski iş yönleri kalmaz.
- Atla/Devam/Eksiklerimle devam stage'e uygundur; kaynak fixture'ı OS veya insan kabulü sayılmaz.

### Kanıt

- Stage/alt-durum×modal ekran atlası; kaynak state; gösterilen/hedeflenen ID ve metin.

### Regresyon

- Jury stage/hints/HUD veya guide routing değişince stage atlası ve insan ilk180sn etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryPresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-066 — Kısa karne gerçek tamamlanan, eksik ve atlananı ayırır

**Alan:** ui-sonuc · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KISA SONUÇ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Tamamlanan, kaçırılan, segment atlanan, cinematic atlanan ve developer kullanılmış bağımsız koşular.

### Adımlar

1. Her koşuyu doğal sonucu ile bitir; karne on hedef etiketini ve summary sayılarını oku.
2. Gerçek pickup/pack/anchor/shelter/outcome kaydıyla TAMAM/EKSİK/ATLANDI durumlarını karşılaştır.
3. TEKRAR DENE ve BAŞLIĞA DÖN sonrası eski karne, speech ve stok kalıntısını kontrol et.

### Beklenen sonuç

- Başlangıç baseline'ı veya atlanan sinema tamamlanan emek gibi sunulmaz; gerçek hedef durumu korunur.
- Sonuç puanı/prevented damage iddiası türetmez; oyuncu neyi değiştirebileceğini okuyabilir.

### Kanıt

- Karne PNG; hedef/outcome event tablosu; retry/title OS videosu; skor dosyası önce/sonra hash.

### Regresyon

- Goal evaluation/summary/skip/developer veya reset değişince bu beş koşu karşılaştırılır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-067 — Kampanya başarı, hasar gezisi ve koşu bitişi

**Alan:** ui-sonuc · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SERBEST KAMPANYA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Gerçek kurallı başarı dalgası ve kayıp koşusu; hasar gezisi20sn; insan gözlemci.

### Adımlar

1. Başarıda bunker sunumu→hasar gezisi→sonraki dalga yolunu ve erken devamı izle.
2. Hasar gezisinde en etkili nesneye odak/kat eki, toplam hasar ve son dalgadan kalan önlemleri gör.
3. Terminalde dalga/puan/hasar/FailureReason ve hileli skor kaydedilmez metnini karşılaştır.

### Beklenen sonuç

- Kampanya sonuç ve inceleme amacı anlaşılır; hasar gezisi hazırlık saatini tüketmiş gibi gösterilmez.
- EV GÜVENDE gibi başarı dili gerçek kazanım ile sınırlı okunur; hasar ve bakiye gizlenmez.

### Kanıt

- Doğal faz videosu; terminal+inspection PNG; model/ödül/score delta; insan yorumları.

### Regresyon

- Success/inspection/endcard veya next-wave reset değişince bir başarı ve üç nedenli kayıp örneği seçilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-068 — Tek afet simülasyonunda sonuç, inceleme ve tekrar

**Alan:** ui-sonuc · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** DEPREM / SEL / YANGIN / FIRTINA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Dört seçili afet; her biri doğal çözüm ve gerçek terminal; hazır baseline açık.

### Adımlar

1. 90sn hazırlık/AFETİ BAŞLAT yolunu uygula; sonra doğal hasarı ve consumed çantayı incele.
2. Inspection sonucunda evi/çevreyi gez; GameOver sonucunda HASARI İNCELE ile gerçek hareketi geri al.
3. TEKRAR DENE aynı afeti korusun; ANA MENÜ→SERBEST OYUN modu/stock/cinematic/speech durumlarını temizlesin.

### Beklenen sonuç

- Kayıp simülasyon otomatik başarıya dönmez; inceleme ödül/skor/dalga vermez ve kendi kendine yeni dalga açmaz.
- Gerçek sonucunun ardından kişi karşılaştırma yapmak için aynı afeti tekrar seçebilir.

### Kanıt

- Dört afet route video; terminal veinspection PNG; retry disaster ID; skor hash; mode reset state.

### Regresyon

- Simulation completion/retry/HUD veya gameover inspection değişince dört afet doğal+atla kapsanır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicSimulationHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-069 — Dünya etiketlerinin HUD, birbirleri ve özneyle çakışması

**Alan:** ui-kadraj · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** TÜM MODLAR / İNCELEME

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Tarihsel V4.8 P2 sağ üst iki label örtüşmesi yalnız referans; canlı adayda tekrar doğrulanacak.

### Adımlar

1. En çok hasarlı nesne gruplarında ve kısa dış incelemede varsayılan+orbit uçlarından etiketleri kaydet.
2. Hedef, hasar, güvenli nokta, loot, subtitle,toast,requirements ve üst kartlar birlikte görünürken görünür alanı incele.
3. Ekran dışı/arkadaki hedef, farklı kat ve uzun Türkçe etiket varyantlarını dene.

### Beklenen sonuç

- Ana hedef/buton/oyuncu rotası örtülmez; kat eki ve sayılar okunur; etiket hangi nesneye ait belli olur.
- Tarihsel P2 yeni adayda kendiliğinden açık/kapalı sayılmaz; gerçek repro ve karelerle sınıflandırılır.

### Kanıt

- Overlap bbox/ID tablosu; doğal ve orbit PNG; kaynak/snapshot/DLL bağı; P2 repro kararı.

### Regresyon

- ClearWorldMarkerBounds/UI rect/font/camera veya label sayısı değişince bu matris tekrarlanır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryPresentation.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/visual-qc/VISUAL-QC-V3.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-070 — Türkçe, işaretler, sayı ve terminoloji tutarlılığı

**Alan:** dil · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** TÜM EKRANLAR / SES

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Kaynak snapshot ve ekran envanteri; karakter/fontlar offline; resmi oyun adı kesin.

### Adımlar

1. Seçili F0 snapshot'ın oyuncu stringlerini, supply/region kit adlarını, ItemActionRules labels ve speech cue'larını state ile çıkar.
2. V4.9'da iki vidaya tıklama, kısa dünya inşası, istasyon ve olgun ağaç sıra diliyle uyuşmayan eski hizala/timing/balance/fidan/sula/yatakta tüp kalıntısını ara.
3. İ/ı/ş/ğ/ç/ö/ü, uppercase, yüzde/dolar/binlik/ondalık, Enter/Space/Tab etiketlerini gerçek renderla eşle; üretim jüri/demo/debug dilini bağlamla incele.

### Beklenen sonuç

- Kısa Türkçe doğrudan gerçek yeni eylemi anlatır; yeni station/tree-row recipe anlamı gizlenmez.
- Tarihsel GDD ve V4.8 input metni V4.9 kararını geçersiz kılamaz; fallback'te eski dil yalnız o profile göre yorumlanır.

### Kanıt

- String/state envanteri; metin değişikliği önerisi ve gerekçe; font glyph PNG görevi listesi.

### Regresyon

- Yeni oyuncu metni veya fontta değişiklikte düşük çözünürlük ve human instruction anlama kontrolü etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicPresentationAudio.cs
- TooLateToPanic/Documentation/GDD.md
- AGENTS.md
- TooLateToPanic/AGENTS.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-071 — Afete göre farklı korunma davranışı ve genelleme riski

**Alan:** tema-guvenlik · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KISA / DÖRT SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Oturum sonrası yönlendirilmemiş görüşme; gerçek hayat güvenlik cümlesi yayımlanacaksa uygun resmi kaynak/uzman incelemesi ayrı kapıdır.

### Adımlar

1. Deprem güvenli alan, sel üst kat/bodrum uyarısı ve yangın istasyon→dış toplanma hedefinin yorumunu sor.
2. 'Aynı yeri her afette kullanır mıydın?' ve 'Oyunda gördüğünü gerçek hayata taşırken neyi ayrıca öğrenmen gerekir?' sorularını yönelt.
3. V4.9 direct-action istasyonunun ekipman hazırlığı olduğunu; tree-row işleminin anlık ağaç büyütme/gerçek mühendislik tavsiyesi olmadığını UI/sesle karşılaştır.

### Beklenen sonuç

- Oyun tek evrensel bunker davranışını bütün afetler için gerçek hayat kuralı gibi öğretmez.
- Skor veya basitleştirilmiş malzeme gün sayısı resmi güvenlik/mühendislik tavsiyesi sayılmaz; belirsiz iddia düzeltilmek üzere kaynak incelemesine gider.

### Kanıt

- Ham cevaplar; riskli yorum–tetikleyen metin/kadraj eşlemesi; resmi doğrulama bekleyen iddia listesi.

### Regresyon

- Güvenli nokta/shelter hint/cue/cinematic anlamı değişince tema değerlendirmesi ve ilgili afet rotası birlikte açılır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Documentation/GDD.md
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationHud.cs
- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-072 — Dört afetin hazırlık eylemi ve hasar karşılığının tutarlılığı

**Alan:** neden-sonuc · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** DÖRT SİMÜLASYON

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Eşlenmiş baseline ve tek önlem farkı içeren finite-stock koşular; gerçek model değerlendiricisi.

### Adımlar

1. Deprem ankraj/tier, sel drain/barrier/fence/region-tree-row, yangın dedicated station/hose, storm roof/door/window/furniture zincirlerini seçili profile eşle.
2. V4.9 uygunluk kurallarını HUD ve API paths için model otoritesiyle karşılaştır; yatak/koltuk fire action reddini kanıtla.
3. Tek değişkenli eşlenmiş koşulda gerçek effect/hardware/tier geometry/Contribution çıktısını kaydet; sofa_indigo/olive canonical risk kimliğini de koru.

### Beklenen sonuç

- Doğrudan işin kolaylaşması koruma hesabını çoğaltmaz; final state gerçek uygun afet etkisine bağlıdır.
- Yeni görsel/tier/tree/station state ikinci evaluator olmaz; unsupported eylem risk kazanımı yaratmaz; eşlenmemiş RNG causal kanıt sayılmaz.

### Kanıt

- Tek değişken A/B setup/outcome tablosu; item risk ID; önce/sonra PNG; state diff.

### Regresyon

- Risk mapping/protection/hardware/metin değişince etkilenen afet ve canonical varyantlar birlikte regresyona girer.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-073 — Puan ve hasar dili mutlak kurtarma vaadi üretmez

**Alan:** neden-sonuc · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SONUÇ / İNCELEME / SKOR

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Gerçek model hasar ve contribution kaydı; normal, hileli, kısa ve simülasyon sonuçları.

### Adımlar

1. Kişiden toplam hasar, eşya hasarı ve katkı etiketinin ne anlama geldiğini anlatmasını iste.
2. Skorun yerel oluşu ve kısa/sim/hileli koşu dışında kalmasını anlayıp anlamadığını sor.
3. 'Ne kadar hasarı önledin?' gibi ek hesap veya yanlış yüzde vaadi UI'da var mı kaynak ve görüntüyle incele.

### Beklenen sonuç

- Contribution önlenen hasar yüzdesi diye çevrilmez; puan gerçek afet hazırlığı beceri sertifikası gibi görünmez.
- Normal başarı ödülü ile ödülsüz inceleme ayrımı görünür; yanlış çıkarım ham cevapla belgelenir.

### Kanıt

- Metin/sayı kareleri; teknik metric sözlüğü; insan açıklaması; iddia kontrol listesi.

### Regresyon

- Score/damage labels/result copy değişince dört modun sonuç ve skor yazım vakaları etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-074 — Ev sıcaklığı, tehlike rengi ve dekor/afet ayrımı

**Alan:** sanat-tema · **Öncelik:** P2 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** TÜM MEKÂNLAR / DÖRT AFET

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Normal akşam ışığı; hearth toggle; hazard ve sonrası aynı kamera.

### Adımlar

1. Şömine, sıcak lambalar, turuncu seçili nesne, pas UI ve gerçek yangını birlikte karşılaştır.
2. Kişiye hangi şeyin etkileşim, hangisinin dekor, hangisinin aktif tehlike olduğunu sor.
3. Indigo/olive/taupe/ceviz ile hava/afet paletinin alıcı beklentisini ve odalar arası bütünlüğünü puanla.

### Beklenen sonuç

- Turuncu her yerde aynı yanlış tehlike veya başarı anlamını taşımaz; kritik bilgi şekil/metinle desteklenir.
- Görsel çekicilik hakkındaki tercih, mekanik yanlış bilgi kusurundan ayrı kaydedilir.

### Kanıt

- Aynı kamera state contact sheet; sınıflandırma cevapları; rubrik renk/tema puanı.

### Regresyon

- Palette/lighting/VFX/outline değişince renk körlüğü ve sessiz afet tanıma kontrolleri etkilenir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Art/PanicResidenceArt.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- TooLateToPanic/Assets/Game/Art/PanicDisasterArt.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-075 — Renk körlüğü ve gri tonda kritik doğrudan eylem ayrımları

**Alan:** erisilebilirlik · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** UI / İŞ / PLACEMENT / HEDEF

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Aday render; ekranın filtreli QA görünümü veya rızalı gerçek renk görme farklılığı katılımcısı; filtre gerçek kişi yerine kabul edilmez.

### Adımlar

1. Protan/deutan/tritan/gri ton QA görünümünde seçili slot, eksik malzeme, geçerli placement ve iki gerçek vida next/busy/seated durumunu incele.
2. V4.9 dünya construction progress/tier/region complete ve station readiness'te renk dışı metin/şekil/sayı ipuçlarını kontrol et.
3. V4.8 fallback seçildiyse Timing/Balance meter ayrımları yalnız eski profile uygulanır; filtre gerçek katılımcı yerine kabul edilmez.

### Beklenen sonuç

- V4.9 kritik click hedefi, busy/tamam ve bölge/istasyon durumu yalnız renkten çıkarılmaz.
- Filtre görsel kontrolü erişilebilirlik sertifikası değildir; gerçek kişinin yanlış kararı ham veri olarak tutulur.

### Kanıt

- Filtre türü ve ham PNG; kritik ayrım tablosu; kişi/filtre kanıt türü ayrımı; yanlış kararlar.

### Regresyon

- Renk, border, icon veya work meter değişince ilgili erişilebilirlik matrisi tekrar çalıştırılır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicRefinement.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-076 — Düşük çözünürlük, en-boy oranı ve Windows DPI

**Alan:** erisilebilirlik · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** BÜTÜN UI STATE'LERİ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- En az960x540,1280x720,1600x900,1920x1080,1920x1200; hedef destek politikasına göre ultrawide; Windows100/125/150DPI; native screenshot.

### Adımlar

1. Ekran atlasını her boyutta tam renderla; başlık, selector, requirements, work, karne ve fail panelinde uzun metin örnekle.
2. Buton hitbox ile çizimin aynı yerde kaldığını mouse ve keyboard odağıyla kontrol et.
3. Küçük font, letterbox, ekrandışı tooltip/marker ve sayısal clipping'i gerçek ekran ölçeğinde insan okuyucuyla değerlendir.

### Beklenen sonuç

- Tüm kritik metin/düğme/ikon görünür ve erişilebilir; scale-to-fit ana hedefi çok küçük hale getiriyorsa sınır açık kaydedilir.
- Tarihsel960x540 küçük açıklama sınırı yeni adayda yeniden ölçülür; beş eski render güncel PASS olmaz.

### Kanıt

- Resolution/DPI/state grid; gerçek boyut PNG; hitbox tıklama video; okunmayan metin/piksel yüksekliği.

### Regresyon

- Canvas scale/font/FittedLabel veya OS pencere değişikliği bütün kritik modal setini etkiler.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationHud.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-077 — Hareket hassasiyeti, kamera sarsıntısı ve kısa animasyonlar

**Alan:** erisilebilirlik · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** KAMERA / DEPREM / SİNEMA / PICKUP

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Rızalı katılımcı; rahatsızlıkta derhal durma; erişilebilir seçenek varmış gibi varsayılmaz; şiddetli maruziyet zorlanmaz.

### Adımlar

1. Önce orbit, yürüme ve kısa pickup; sonra gönüllü uygun görürse deprem/sinematik izle.
2. Kamera yaw/pitch, shake, cinematic cut, flashing alarm ve hover hareketinin rahatsızlık anını0–4 kaydet.
3. Enter ile atlama ve menü/oyun dışı mola erişimini dene; eksik azaltılmış hareket seçeneği varsa tasarım adayı olarak yaz.

### Beklenen sonuç

- Doğru çalışan efekt rahatsızlıksız kabul edilmez; ölçülmeyen tolerans herkes için güvenli diye sunulmaz.
- Rahatsızlık nedeniyle bırakma test başarısızlığı gizlenmeden kaydedilir; yeni seçenek uygulama kararı root'a gider.

### Kanıt

- Rızalı özbildirim, tetikleyen shot-ID/timecode, durdurma nedeni, skip erişim video.

### Regresyon

- Camera/shake/flash/pickup motion/cut değişince etkilenen düşük maruziyet örneği yeniden değerlendirilir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicCameraOrbit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomePresentation.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-078 — Klavye ve fare yollarında okunabilir odak ve tuş beklentisi

**Alan:** erisilebilirlik · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** TÜM UI / DÜNYA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Bir keyboard ağırlıklı ve bir mouse ağırlıklı gönüllü; aynı aday; gerçek Windows girdi.

### Adımlar

1. Menü/selector/loot/çanta/pause/dev/sonuç ve profile uygun direct-action/placement yollarını gerçek mevcut girdilerle dene.
2. V4.9 actual screw click'in fare gereksinimi ve dünya install eyleminin tuş/fare affordance'ını sor; keyboard-only tam oynama desteğini varsayma.
3. Odağın nerede olduğunu ve Enter/Esc/Backspace/Space'in şimdi ne yapacağını kaydet; eski minigame keyboard ipucu kalıntısını ara.

### Beklenen sonuç

- Mevcut desteklenen yollar anlaşılır; tek input UI/world çift eylemini tetiklemez.
- V4.9 click-only iş için keyboard-only erişim eksikliği tasarım adayı olarak açık yazılır; eski klavye mini oyunu zorunlu fallback çözümü sayılmaz.

### Kanıt

- Yol×girdi matrisi; kişinin beklediği/gerçek tuş; odak kareleri ve yanlış çift input.

### Regresyon

- Input/context/modal değişince ilgili iki yol ve aynı-kare güvenliği tekrar açılır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicCameraOrbit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-079 — Odak kaybı ve kısa kesintiden sonra amaç korunur

**Alan:** erisilebilirlik · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** KISA / SERBEST / DIRECT-CLICK / WORLD INSTALL / TAŞIMA

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- QA penceresi; root ana kullanıcı player'ına dokunmaz; kontrollü AltTab/focus switch; gerçek profil yalıtılmış.

### Adımlar

1. V4.9 screw seating/world0.60sn install, yürüme/orbit/taşıma/çanta/konuşmada kontrollü kısa focus kaybı ve dönüş uygula.
2. Hayalet click/held input, busy progress ve final cost/geometry state'ini dönüşte kaydet; eski stage/target kopyasını ara.
3. Kısa dış saat kesintide akıyorsa güncel stage/karne ve controller geri dönüşünü incele; V4.8 work kesinti setini yalnız fallback olarak uygula.

### Beklenen sonuç

- Geri dönüşte hayalet sürükleme veya istemsiz dünya eylemi olmaz; güncel amaç anlaşılır.
- Kesinti dış saatten çıkarılmaz; pause/clock politikası görünüşle çelişmez. Yetkisiz kilit ekranı aşma planı yoktur.

### Kanıt

- Odak/video/timecode; input pressed/held release kaydı; önce/son hedef ve süre snapshot.

### Regresyon

- Application focus/input cancel/clock veya gesture değişince kesinti seti tekrarlanır.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicCameraOrbit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-080 — Alıcı sanat rubriği ve yönlendirmesiz görüşme oturumu

**Alan:** insan-rubrik · **Öncelik:** P2 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** BÜTÜN DENEYİM

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- visual-story-rubric.md protokolü; en az üç ilk oyuncu; gözlemci ile sanat değerlendiricisi puanlarını bağımsız yazar.

### Adımlar

1. Her ekran/kadraj için0–4 somut anchor kullan; puan yanına shot-ID ve gerekçe ekle.
2. Oturum sonrası amaç, en güzel/en zayıf kare, önlemin etkisi, kontrol hissi, ses, devam isteği ve güvenlik yorumunu nötr sırayla sor.
3. Ölçülen davranış, teknik kusur ve estetik tercihi ayrı kaydet; anlaşmazlığı görüş birliğine zorlamadan göster.

### Beklenen sonuç

- Tek 'güzel/kötü' not yerine eyleme dönüşebilir kanıt ve gerekçe oluşur.
- Az kişilik örneklem genelleştirilmez; ilk180sn hedefi, tam kampanya, erişilebilirlik ve nihai sanat kabulü ayrı sonuçlardır.

### Kanıt

- Kişi bazlı görüşme dökümü; bağımsız puanlar; ekran başına kanıt linki; ortak/ayrı görüş tablosu.

### Regresyon

- Büyük sanat/UX paketinden sonra aynı görev protokolüyle yeni katılımcı ve kör değerlendirme tercih edilir.

### Kaynak referansları

- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- TooLateToPanic/Documentation/HOME-V4.7-DELIVERY-2026-10-17.md
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-081 — Görsel/anlatım bulgusunu tek değişkenli düzeltme adayına çevir

**Alan:** duzeltme-plani · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** TÜMÜ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Bu plan yürütüldüğünde açılacak bulgu kuyruğu; hiçbir öneri şu an uygulanmış değildir.

### Adımlar

1. Her bulguya tekrar üretim, mevcut/istenen davranış, ilgili state/shot, severity ve kanıt kimliği ekle.
2. En dar mevcut API/art/metin düzeltmesini ve etkilediği komşu state'leri yaz; yeni misyon/karakter/mimariyi varsayılan çözüm yapma.
3. A/B kıyası gerekiyorsa kamera/ışık/model koşullarını sabitle; root onarımı sonrası aynı vaka ve gerekli regresyonu planla.

### Beklenen sonuç

- V4.9 yeni davranışla çelişen eski beklenti ürün kusuru gibi önceliklendirilmez; F0 profile göre gerçek regresyon tanımlanır.
- P0/P1 görünürlük, yanlış sonuç ve yanlış davranış mesajı önce gelir; P2/P3 cila döngüyü geciktirmez.
- Düzeltme collision/bütçe/kayıp/controller/step/slope sınırlarını gevşetmez; art spend/new scope ayrıca somut karara gider.

### Kanıt

- Bulgu→düzeltme→beklenen değişim→regresyon zinciri; A/B setup ve karar gerekçesi.

### Regresyon

- Birleştirilen sanat paketinden sonra tek import/build + ilgili3–5dk smoke politikası root tarafından korunur.

### Kaynak referansları

- AGENTS.md
- TooLateToPanic/AGENTS.md
- Docs/TOO-LATE-TO-PANIC-BIRLESIK-RAPOR-2026-10-16.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## EXP-082 — Screenshot atlası ve kapanışta deneyim kabul ayrımı

**Alan:** kanıt · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** TÜMÜ

### Önkoşullar

- F0 davranış manifesti contract+snapshot+DLL için V4.9_DIRECT_ACTIONS adayını veya açıkça seçilmiş V4.8_FALLBACK profilini belirtir; iki profil karıştırılmaz.
- Plan yürütüldükten sonra root aday evidence klasörü; bütün ekran/kadraj ID'leri.

### Adımlar

1. Her vaka için önce/sıra/sonra raw PNG, video timecode, build/profile/source identity ve girdi türü var mı denetle.
2. Geçmiş PNG'yi referans etiketiyle, yeni adayı kendi hash'iyle ayır; crop/annotation türevini raw üzerine yazma.
3. SOURCE/NATIVE_API/OS_E2E/HUMAN sonuçlarını ayrı raporla; açık kusur ve ölçülmemiş rubrik alanını kabul tablosunda sakla.

### Beklenen sonuç

- Bir sahne karesi hareket/ses/OS input veya insan keyif kanıtı sayılmaz; makine testi sanat beğenisini doldurmaz.
- Bütün alanlar PLANNED olarak başlar; yürütülmeyen, engelli, N/A, başarısız ve geçen durumlar gerçek kanıta göre ayrı güncellenir.

### Kanıt

- Coverage atlas; orphan/missing evidence list; skor/kayıt koruma kaydı; sonuç/limit tablosu.

### Regresyon

- Aday DLL/source/asset değişirse kapanış kapısı yeniden açılır; eski PASS yeni build'e topluca kopyalanmaz.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- AGENTS.md
- TooLateToPanic/AGENTS.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## SYS-001 — F0: yayımlanan DLL, canlı kaynak ve test harness ayrımı

**Alan:** Kaynak tabanı · **Öncelik:** P0 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Unity/player başlatılmadan; root tek sahip; önceki snapshotlar korunur.

### Adımlar

1. LATEST, V4.8 manifesti ve planning-baseline receipt'ini oku.
2. 61 kayıtlı kaynağın SHA256/byte değerlerini ve eklenen/silinen dosyaları yeniden çıkar.
3. Dört değişmiş kaynak ile beş yeni audit partial'ını ayrı listele; güncel iş sahibini kaydet.
4. Hedef aday için sabit snapshot, değişiklik listesi ve test manifestini imzala; derleme başlayınca kaynakları dondur.

### Beklenen sonuç

- V4.8 149/189 kabulü yalnız 5683E713... DLL'a bağlı doğrulanmış fallback'tir.
- Canlı dört değişmiş dosya ve beş yeni audit partial eski native kabulünü devralmaz;rollback yapılmaz.
- F0 V48_FALLBACK veya V49_DIRECTACTIONS profilini ve güncel sözleşme SHA'sını seçer;aktif üretim bitmeden aday dondurulmaz.
- Kaynak değişirse downstream test STALE olur;eski mini oyun beklentileri V4.9 gereksinimi değildir.

### Kanıt

- F0-before/after hash JSON
- snapshot/diff
- owner receipt

### Regresyon

- Her fix batch öncesi/sonrası; tüm downstream PASS kimliği buna bağlanır.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- Evidence/GamejamOrchestra/TLTP-E2E-MASTERPLAN-20261017/manager/planning-baseline.json
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- TooLateToPanic/Assets/Game/Editor/PanicResidenceBuild.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md

## SYS-002 — Unity, sahne, renderer, input ve bağımlılık kapanımı

**Alan:** Ayar ve kapanım · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- F0 snapshot hazır.

### Adımlar

1. ProjectVersion, ProjectSettings, EditorBuildSettings ve Packages içeriğini kaydet.
2. Residence sahnesi/Bootstrap/Resources referanslarının GUID kapanımını incele.
3. Built-In shader dahil etme, sahne aktifliği, Windows mimarisi ve Input Manager yollarını doğrula.
4. Eksik script/shader/font/prefab/asset ve FPS kaynak karışması taraması yap.

### Beklenen sonuç

- Unity 6000.0.73f1, Residence sahnesi ve mevcut Built-In hedef korunur.
- Eski Home/legacy sahne testi aktif Residence kabulü sayılmaz.
- Kayıp referans yok; değişen ayarlar risk kaydına girer.

### Kanıt

- settings/scene hash listesi
- GUID closure raporu
- build config receipt

### Regresyon

- Paket/asset/sahne değişiminde; her adayda settings hash.

### Kaynak referansları

- TooLateToPanic/ProjectSettings/ProjectSettings.asset
- TooLateToPanic/Packages/manifest.json
- TooLateToPanic/Assets/Game/Editor/PanicResidenceBuild.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/ProjectSettings/ProjectVersion.txt

## SYS-003 — Çevrimdışı içerik, lisans ve ertelenen ses kapanımı

**Alan:** Lisans ve kapsam · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Asset envanteri ve lisans dosyaları salt okunur.

### Adımlar

1. Resources içeriği ile build'e giren seçili Yarkın/font/ses kaynaklarını eşleştir.
2. 7 kısa konuşma ve 3 SFX kimliğini; kampanya ses kısıtını çıkar.
3. Deferred/Radio-legacy, cache/kişisel kayıt/anahtar/FPS kaynaklarının paket dışılığını kontrol et.
4. Eksik lisans veya yeni sağlayıcı gerektiren işi açık iş olarak kaydet; kredi kullanma.

### Beklenen sonuç

- Yetkili kısa akış sesleri ve afet sesleri dışında eski konuşma yanlışlıkla çalmaz.
- Gerekli lisans/attribution pakette bulunur.
- Yeni ödeme/kredi/sağlayıcı işi başlatılmaz.

### Kanıt

- asset-to-build mapping
- license manifest
- audio route list

### Regresyon

- Ses/asset/import/bake batch'inde.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs
- TooLateToPanic/Packages/manifest.json
- AGENTS.md

## SYS-004 — Gerçek smoke bayrakları, tek test ve izolasyon sözleşmesi

**Alan:** Test aracı · **Öncelik:** P0 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Test

### Önkoşullar

- Komutlar yalnız planlanır; bu vakada player açılmaz.

### Adımlar

1. HomeWorld Awake ile RuntimeInitializeOnLoad audit Boot kaydını eşleştir.
2. Her bayrak için rapor adı, watchdog, bitiş/quit davranışı ve gereken profili kaydet.
3. Tek smoke + yeni GUID kuralını uygula; audit-probe ve audit-section sonuçlarını dar kapsam diye etiketle.
4. Bilinmeyen bayrak/eksik argüman ve birlikte bayrak riskini yeni izolasyon testine dönüştür.

### Beklenen sonuç

- --fullflow-smoke ve --jury-smoke karıştırılmaz.
- --interaction-audit kaynakta mevcut olsa bile yeni DLL'da derlenmeden çalışır sayılmaz.
- 245 sn harness watchdog ile 270/300 sn oyun sınırı birbirine karışmaz.

### Kanıt

- smoke-command-inventory.md bölümü
- flag-to-report tablosu
- kaynak satırı referansları

### Regresyon

- Harness veya boot değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionAudit.cs

## SYS-005 — Eski PASS sayısını satırlar, hata listesi ve kimlikle tekrar hesaplama

**Alan:** Kanıt denetimi · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: V4.8 tarihsel

### Önkoşullar

- Tarihsel JSON/PNG/loglara yazılmaz.

### Adımlar

1. fullflow tamamlandı/geçti,149 satır ve149 true, boş error/errors değerlerini ayrı hesapla.
2. jury-smoke int passed/total=189 ve189 true satırı ayrıca hesapla.
3. JSON hash, build-identity DLL hash ve screenshot dosya varlığı/hashlerini bağla.
4. Ara V1/V2 başarısız koşuları ve final V3'ü ayır; kopya/mükerrer assertion'ı vaka sayısı diye sunma.

### Beklenen sonuç

- 149 rota assertion'ı ve189 fixture assertion'ı gerçekte dosyada bulunur.
- 149.3159 sn toplam,49.6669 sn kısa akış;0 test Warp/Tick/dev yalnız bu harness için geçerlidir.
- 189 regresyonun açık Warp ve zaman fixture'ları saklanır; sonuçlar OS/insan/kampanya kabulüne yükseltilmez.

### Kanıt

- evidence-read-audit.json
- orijinal rapor SHA256
- satır toplamı ve kapsam sınırlaması

### Regresyon

- Her teslim indeksinin sayısal özeti için.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs
- Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/visual-qc/VISUAL-QC-V3.json

## SYS-006 — Aynı seed/aynı eylem sonucu tekrar üretimi

**Alan:** Deterministik model · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya + simülasyon

### Önkoşullar

- İzole model; gerçek kayda erişim yok.

### Adımlar

1. Kaynak runSeed=0x20261016 ve StableVariation yolunu kaydet; var olmayan --seed bayrağı uydurma.
2. Aynı afet/dalga/eşya/önlem dizisini üç temiz modelde uygula.
3. Hasar, tüketim, maliyet ve terminal sonucu bit/tolerans sözleşmesiyle karşılaştır.
4. Dalga/afet/önlemden yalnız birini değiştir; beklenen farkı ayrı kaydet.

### Beklenen sonuç

- Aynı model girdisi aynı sonuçtur.
- Rastgele görsel/partikül/fps süresi ile model determinismi karıştırılmaz.
- Fark yeni ürün bug'ı veya oracle farkı olarak sınıflanır; seed sonucu keyfi değiştirilmez.

### Kanıt

- seed/action ledger
- 3 karşılaştırma JSON
- tolerans/oracle kaydı

### Regresyon

- Hasar/afet/model değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs

## SYS-007 — Geçerli highscore yükleme, sıralama ve sınır değerler

**Alan:** Profil · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya

### Önkoşullar

- ScorePathOverride ilk modelden önce yalnız fixture klasörüne atanır.

### Adımlar

1. Boş dosya yok, geçerli boş liste,1 kayıt ve eşit skorlu çok kayıt fixture'ları hazırla.
2. Negatif score/wave, null entry ve Türkçe ad içeren fixture'ları ayrı yükle.
3. Sıralama/filtreleme ve restart sonrası aynı sonuçları karşılaştır.
4. Gerçek kişisel skor hash'ini öncesi/sonrası salt okunur doğrula.

### Beklenen sonuç

- Yalnız geçerli kayıtlar beklenen stabil sıradadır.
- Yükleme testi gerçek oyuncu highscore dosyasını değiştirmez.
- Kampanya devam kaydı vaat edilmez; mevcut persistence kapsamı highscore'dur.

### Kanıt

- fixture hashes
- yüklenen entry JSON
- real-score hash receipt

### Regresyon

- Skor/yükleme/model reset değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs

## SYS-008 — Bozuk, kesik, yanlış şema ve makul büyük skor dosyası

**Alan:** Profil hata yolu · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya

### Önkoşullar

- Ayrı fixture profili; fresh-smoke absent-file kuralı bu negatif profile uygulanmaz.

### Adımlar

1. Kesik JSON, çöp UTF8, null/eksik entries ve yanlış alan tipleri üret.
2. Her fixture'ı yeni modelde yükle; exception/warning ve boş güvenli durumunu kaydet.
3. En fazla1MiB fixture ile yükleme süresini ölç; sınır kararını ürün beklentisine bağla.
4. Yeni normal koşu başlayabilmesini ve dosyanın yalnız bilinçli skor kaydında değişmesini gözle.

### Beklenen sonuç

- Unhandled exception/crash/yanlış kişisel kayıt erişimi yok.
- Hatalı dosya yükleme uyarısı anlaşılır; sahte skor eklenmez.
- Kurtarma davranışı belgesizse PASS varsayılmaz, karar/bug açılır.

### Kanıt

- fixture byte SHA
- warning log
- load duration
- önce/sonra file hash

### Regresyon

- Save/load onarımında; SYS-007/SYS-009.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs

## SYS-009 — Kilitli, salt okunur skor ve artık tmp güvenliği

**Alan:** Atomik kayıt · **Öncelik:** P0 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya

### Önkoşullar

- Yalnız test klasörü; gerçek disk doldurulmaz.

### Adımlar

1. Önce geçerli scoreboard ve ayrı .tmp bırak; yeni model yükle.
2. Fixture dosyasını kilitle veya test klasöründe erişimi reddet; tek meşru skor yazımı dene.
3. Eski skorun baytlarını ve hata mesajını kaydet; kilidi kaldırıp tekrar dene.
4. Yazım arası hata enjeksiyonu için staging adapter öner; üretim dosyasına rastgele kill yapma.

### Beklenen sonuç

- Başarısız kayıt eski geçerli dosyayı bozmaz; başarısız yazım başarı diye raporlanmaz.
- Geçici dosya kaynak/kişisel kayda taşmaz.
- İzin düzelince tekrar kayıt yeni geçerli JSON üretir; kalan tmp durumu belgelenir.

### Kanıt

- before/after SHA
- I/O failure log
- fixture izin receipt
- recovery JSON

### Regresyon

- SaveScoresAtomic veya platform değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs

## SYS-010 — Test, normal profil ve modların skor yolu yalıtımı

**Alan:** İzolasyon · **Öncelik:** P0 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Kişisel skor salt okunur; tek root test sahibi.

### Adımlar

1. --home-evidence normal UI profilinde score override'ın modelden önce atanmasını izle.
2. Smoke için yeni GUID ve absent isolated score zorunluluğunu denetle.
3. Kısa akış/simülasyon/developer denemelerini ayrı profillerde bitir.
4. Normal kişisel dosya hash'i ile test dışındaki hiçbir profilin değişmediğini doğrula.

### Beklenen sonuç

- Test dosyası yalnız atanmış GUID klasöründedir.
- Kısa akış/simülasyon kalıcı skor yazmaz; normal profille karışmaz.
- İzolasyon başarısızsa sonraki tüm player testleri durur.

### Kanıt

- absolute score path receipt
- personal before/after SHA
- mode/write ledger

### Regresyon

- Her test koşusunda; SYS-041.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionAudit.cs

## SYS-011 — Developer kullanımından sonra sticky skor yasağı

**Alan:** Skor uygunluğu · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya

### Önkoşullar

- Temiz model; menü açma ile seçenek kullanma ayrı olay.

### Adımlar

1. Yalnız F2 panelini aç/kapat; CheatedRun false kalmalı.
2. Her seçeneği tek tek true sonra false yap; +1000 bütçeyi ayrıca uygula.
3. Meşru sonuç üret; skor kaydı ve ödül alanlarını incele.
4. Yeni koşuda temiz başlangıç ve önceki geçmişin değişmediğini doğrula.

### Beklenen sonuç

- Panel açmak hile değildir; kullanım o koşu boyunca sticky'dir.
- Seçeneği kapatmak skor hakkını geri vermez.
- NoLoss hasarı/erzak tüketimini korur, başarısızlığa ödül vermez.

### Kanıt

- option mutation ledger
- score before/after
- outcome JSON

### Regresyon

- Developer/ödül/skor değişiminde; SYS-014/SYS-041.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicDeveloperSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs

## SYS-012 — Süre, çanta ve %65 hasar sınırlarının tam kenarı

**Alan:** Kayıp sınırı · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya + simülasyon

### Önkoşullar

- Doğru kurulum fixture'ı açık yazılır; sınırları gevşetmek yasak.

### Adımlar

1. Hazırlık saatinde ε önce/tam0/ε sonra davranışlarını test et.
2. Gereken su/gıda adedinin bir eksik/tam/bir fazla ve 8slot sınırını dene.
3. Hasarı %65 sınırının altı/tamı/üstünde kaynakta kullanılan karşılaştırmaya göre doğrula.
4. Eksik çanta+ağır hasar+süre aynı olayda oluşunca tek terminal ve doğru öncelikli nedeni doğrula.

### Beklenen sonuç

- Yanlış başarı/çifte sonuç/çifte tüketim yok.
- Tam eşitliğin sözleşmesi kaynak karşılaştırmasıyla kayıtlıdır.
- Kayıp geri bildirimi başarısızlık sinematiğinden önce görünür; NoLoss ayrı açık seçenek koşusudur.

### Kanıt

- boundary table
- before/outcome state
- assertion oracle

### Regresyon

- Hasar/sağkalım/timeout değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs

## SYS-013 — Sonlu malzeme ve bütçenin tek commit atomikliği

**Alan:** Ekonomi · **Öncelik:** P0 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya + kısa akış

### Önkoşullar

- Developer seçenekleri kapalı.

### Adımlar

1. F0'da V48_FALLBACK veya V49_DIRECTACTIONS profilini seç;V4.9 için yalnız kabul edilmiş iki screw/kurulum completion commit'ini kullan.
2. MaterialCosts tarifini CanBeginWork/HUD/craft/completion ile çapraz eşleştir.
3. Yetersizbütçe/tekeksikmalzeme/kapasitedolu/unsupportedaction kurulumlarını dene.
4. Başla→iptal,doğrubitir,tekrarcompletion,busyclick ve hedefkaybını karşılaştır.
5. Envanter+çanta+kaynak toplamlarını eylem başına muhasebeleştir.

### Beklenen sonuç

- Başarısız/iptal eylem harcamaz; başarı tam bir kez harcar.
- HUD Var/Gerek/Eksik sayıları modelle aynıdır.
- Malzeme yaratma/çift pickup/eksi stok/çifte bütçe kesimi yok.

### Kanıt

- transaction ledger
- recipe/HUD/model diff
- duplicate callback result

### Regresyon

- İş/craft/loot/HUD değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionModelAudit.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## SYS-014 — 16 seçenek birleşimi ve +1000 kapsamı

**Alan:** Developer matrisi · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya

### Önkoşullar

- 4bool için16mask; her mask ayrı temiz fixture.

### Adımlar

1. InfiniteBudget,FreeMaterials,FreezePreparationTime,NoLoss bütün masklerini üret.
2. Her maskte bir ücretli iş, tarif craft, doğal timeout ve gerçek hasar örneği uygula.
3. Yalnız aktif seçeneğin izin verdiği farkı ölç; bütçe eklemeyi ayrı eylem yap.
4. Kapatma/yeniden başlatma ve score eligibility'yi karşılaştır.

### Beklenen sonuç

- Bağımsız seçenekler birbirinin davranışını örtük açmaz.
- Freeze yalnız hazırlık saatini dondurur; kısa akış üst sınırı/sinematik kilidiyle karışmaz.
- NoLoss bağışıklık/ücretsiz stok/ödül üretmez.

### Kanıt

- 16mask matrix JSON
- option before/after
- cheat-score ledger

### Regresyon

- Developer veya clock değişiminde; eski 392 model testi yeni DLL kanıtı değildir.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicInteractionModelAudit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicDeveloperSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs

## SYS-015 — Menü/yeniden başla/öğretici bittiğinde durum sıfırlaması

**Alan:** Reset · **Öncelik:** P1 · **Katman:** MODEL

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Sıcak durumda inventory seçimi/anchor mode/focus/iş/taşıma/afet izi oluşmuş.

### Adımlar

1. Her moddan ReturnToMenu,Restart,FinishTutorial ve retry üretim eylemini çağır.
2. Seçili afetin retry'da korunması ile kampanya start resetini ayır.
3. Kaplar/loot/çanta/iş/taşıma/kamera/cue/aftermath geçici alanlarını karşılaştır.
4. Aynı geçişi üç kez tekrarla.

### Beklenen sonuç

- Yeni koşu stok,bütçe,seçim,faz,focus ve geçici sunumda doğru tabandır.
- Retry seçilen afetle başlar; eski mod/ödül/hasar sızmaz.
- Kalıcı normal highscore resetlenmez.

### Kanıt

- reset field diff
- 3-cycle timeline
- profile hash

### Regresyon

- Lifecycle/hud/world değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionAudit.cs

## SYS-016 — Tek karede tek ayrık eylem ve modal sahipliği

**Alan:** Girdi katmanları · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Yeni audit dosyaları aday DLL'a alınmış; açık fixture tanımı.

### Adımlar

1. Tab+B,Tab+E,F+LMB,F2+E ve iptal+fare kombinasyonlarını gerçek dispatcher/OnGUI callback'leriyle aynı karede gönder.
2. Model stok/faz/modallarını öncesi/sonrası kaydet.
3. Sonraki karede aynı geçerli eylemin çalıştığını doğrula.
4. InputLocked,cinema,LossAnnounced ve DeveloperInputBlockedThisFrame kapılarını ayrı uygula.

### Beklenen sonuç

- Tek kare ikinci dünya/UI eylemi üretmez.
- Engel kalkınca girdi kalıcı kilitlenmez.
- Mantıksal pointer fixture gerçek OS mouse dönüşümü diye raporlanmaz.

### Kanıt

- frame-index event ledger
- native assertion JSON
- before/after state

### Regresyon

- Input/HUD değişiminde; SYS-034 fiziksel tekrar.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionFrameAudit.cs
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/BUGS.md

## SYS-017 — V4.9 iki gerçek vida, 0.60 sn oturma ve busy/cancel atomikliği

**Alan:** İş iptali · **Öncelik:** P0 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** V49_DIRECTACTIONS; V4.8 mini oyun oracle'ları bu adayda deprecated

### Önkoşullar

- DirectActions sözleşmesi sabit; iki gerçek vida transform'u mevcut; test Cost/Complete alanına yazmaz.

### Adımlar

1. İlk vidanın görünür viewport noktasını ClickWorkTarget/Work.TryClick ile kabul ettir.
2. 0.60 sn oturma sırasında aynı, diğer ve geçersiz vida click'lerini aynı ve farklı karelerde dene.
3. İkinci vida yalnız sırası geldiğinde kabul edilsin; iki oturmayı gerçek frame delta ile tamamla.
4. İlk vida sırasında, vidalar arasında ve son commit sınırında Esc/Backspace iptali ile LMB+Enter kombinasyonunu dene.
5. Eski A/D, timing, hold ve direction girdilerinin işi tamamlamadığını doğrula.

### Beklenen sonuç

- Yalnız doğru ve sıradaki gerçek hedef click'i ilerler; boş, geçersiz veya busy click ilerlemez.
- Kabul edilen her vida 0.60 sn hedefte oturur; final completion maliyet ve önlemi bir kez commit eder.
- Commit öncesi iptal maliyet veya önlem bırakmaz; kare sahipliği korunur.
- V4.8 mini oyun aşaması beklentisi V4.9'a taşınmaz.

### Kanıt

- Vida viewport/hit area JSON
- Busy/cancel kare zaman çizelgesi
- Bütçe ve malzeme öncesi/sonrası
- Aday DLL kimliği

### Regresyon

- Doğrudan eylem, iş, iptal, HUD veya input değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionFrameAudit.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## SYS-018 — Kapı sıkışması, iki merdiven ve güney banyo rotası

**Alan:** Fizik · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya

### Önkoşullar

- Güncel kapı/merdiven sözleşmesine uyarlanmış fixture; gerçek controller boyutları.

### Adımlar

1. F0 hızprofili seç:V4.8 2.7/4.2 veyaV4.9 3.645/5.67m/s;controller ölçüleri aynı kalır.
2. Dört geçiş kapısına iki taraftan çapraz yaklaş;açılma/kapanma ve yarıkapan→E dene.
3. Collider açıkken temasazaltma/bekleme/devamı bağımsız etkinCapsuleCollider oracle ile ölç.
4. Bodrum→üstkat→bodrum ve güney banyo tek giriş rotasını gerçekyürüyüşle tamamla.
5. Yeni güçlendirme geometrisi ve ağaçsırası aktifken kapı,merdiven,bunker/garajrotasını tekrarla.

### Beklenen sonuç

- İtme,duvar kaçışı,Warp,çarpışma kapatma ve kalıcı sıkışma yok.
- Yarı açık kapı aynı açılma kolunda kalır.
- Quaternion.Angle==0 kestirmesi ve kapalı proxy oracle geri getirilmez; Physics.SyncTransforms korunur.

### Kanıt

- route video/positions
- independent collision distances
- native-door result

### Regresyon

- Controller/door/layout/collider değişiminde; SYS-033.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicDoorJamSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicDoorsStairsSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## SYS-019 — Taşıma API: kat, HUD, zemin, duvar, kapı ve aktör reddi

**Alan:** Yerleşim · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya

### Önkoşullar

- Taşınabilir gerçek hedef; maliyet/yerleşim kuralı değişmez.

### Adımlar

1. Üretim ekran ışınıyla taşıma başlat;45derece döndür ve geçerli yerde bırak.
2. Yanlış kat/boşluk/duvar/kapı açıklığı/oyuncu üstü noktalarını dene.
3. HUD üstünde LMB ve sağtık/Esc/Backspace iptalini dene.
4. Kaynak taşıyınca loot/etkileşim koordinatlarının yeni yere geldiğini doğrula.

### Beklenen sonuç

- Geçersiz bırakma kaynak/collider kaybettirmez.
- İptal önceki kesin pozu geri verir.
- Fare API ışını kabulü OS polling/hit-test kabulünün yerine geçmez.

### Kanıt

- placement validation table
- pose/collider before/after
- native PNG

### Regresyon

- Placement/camera/layout değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionWorldAudit.cs

## SYS-020 — Yeni adayda doğal saatli fullflow yürüyüşü

**Alan:** Kesintisiz rota · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kısa akış +4simülasyon

### Önkoşullar

- F0 temiz; yeni GUID; tüm dev seçenekleri kapalı; root player sahibi.

### Adımlar

1. V4.9 seçilmişse harness'in eski iş ve yangın mobilyası rotasını vida, istasyon ve doğrudan kurulum girdisine uyarla; freeze manifestine al.
2. --fullflow-smoke tek bayrak ve yeni GUID ile çalıştır; aday DLL kimliğini kaydet.
3. Normal spawn'dan gerçek CharacterController, E/F, çanta ve doğrudan eylem rotasını doğal saatle yürüt.
4. Dört afeti doğal saatle sonuçlandır; gerçek kayıpta hasar inceleme, retry ve menü yollarını dene.
5. Yeni requirement-to-check haritasıyla satırları say; 149 tarihsel referanstır, evrensel sabit hedef değildir.

### Beklenen sonuç

- Test Warp=0, injected Tick=0 ve developer mutation=0 korunur.
- Gerçek kayıp başarı sayılmaz; dört afet terminali ve yeni sözleşmenin zorunlu check kümesi tamamdır.
- V4.8 149 PASS yeni harness kabulü yerine geçmez.
- Hazırlanmış API rotası insan hızı kabulü değildir.

### Kanıt

- fullflow-smoke.json
- build-identity
- player.log
- 29referans render + aday timeline

### Regresyon

- Faz/rota/iş/afet değişiminde; tek birleşik batch smoke çekirdeği.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- TooLateToPanic/LATEST-BUILD.json
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## SYS-021 — 189 tarihsel fixture kapsamını sözleşmeye göre yeniden haritalama

**Alan:** Regresyon · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- F0 temiz; ayrı GUID; fullflow ile aynı anda değil.

### Adımlar

1. V4.8 fallback için --jury-smoke ayrı GUID ile tarihsel 189 kontrolü yeniden çalıştırabilir.
2. V4.9'da eski mini oyun, mobilyada yangın, sabit hız ve tekil fidan assertion'larını DEPRECATED_BY_CONTRACT olarak haritala.
3. Yerlerine iki vida, 0.60 sn kurulum, yangın istasyonu, üç bölge ve 1.35 hız kontrollerini ekle.
4. Warp, .04/.08 dt ve 8/80/300 Tick fixture kayıtlarını sakla; doğal rota ile karıştırma.
5. Rapor yazıldıktan sonra yalnız root'un test PID'sini kapat; bu harness kendi Quit çağrısını garanti etmez.

### Beklenen sonuç

- Yeni zorunlu check kümesi ve requirement kapsamı tamdır; sayı değişimi gerekçelidir.
- Eski 189 PASS yalnız V4.8 DLL'a bağlı kalır.
- Model fixture, doğal saatli rota ve insan kabulü ayrı raporlanır.

### Kanıt

- jury-smoke.json
- fixtures list
- candidate DLL
- owned process receipt

### Regresyon

- Model/sunum/simülasyon değişimi; geniş regresyon batch sonunda bir kez.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs
- TooLateToPanic/LATEST-BUILD.json
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## SYS-022 — Interaction audit bölümlerinin derleme ve kapsam haritası

**Alan:** Yeni audit · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Yeni5partial derlenmiş; ek kaynakcompile geçmişi native kabul sayılmaz.

### Adımlar

1. V4.9 davranış manifestini üret; eski iş, hız ve fidan oracle'larını adaydan ayır.
2. ui, model, world, frame, targets ve flow bölümlerini ayrı GUID'lerde veya tek tam audit'te planla.
3. 392 model, 96 hedef ve 138 UI eski sonucunu tarihsel tut; 355 pairwise satırı yürütülmüş değildir.
4. Yeni assertion adlarını, gözlenen boyutları ve skip/deprecated nedenlerini say.
5. Fixture sızıntısına karşı seçenekler kapalı temiz StartGame denetimi koy.

### Beklenen sonuç

- Tüm bölüm başlangıç/bitişleri kayıtlıdır; hiç çalışmayan bölüm PASS olmaz.
- 616/760FAIL geçmişi görünür kalır;138UI dar PASS geniş suite PASS'ına çevrilmez.
- OS/focus testleri bu araçtan kabul edilmez.

### Kanıt

- interaction-audit.json
- section coverage map
- fixture/skip reasons
- compile+DLL receipt

### Regresyon

- HUD/input/iş değişiminde riskli bölümler; tam suite gün sonu.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicInteractionAudit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionFrameAudit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionModelAudit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionTargetAudit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionWorldAudit.cs
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/REPORT.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/manager/OWNER-HANDOFF-TO-MAIN.md

## SYS-023 — Beş çözünürlükte gerçek render ve semantik hit testi

**Alan:** HUD · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Yeni aday; UI callback fixture'ı açık; okunurluk insan kabulünden ayrı.

### Adımlar

1. 960x540,1280x720,1600x900,1920x1080,2560x1440 boyutlarında menü/iş/çanta/geliştirici/sonuç render al.
2. Kenar/boş/sağtuş/MouseUp olaylarıyla button hit bölgesini dene.
3. Canlı Var/Gerek/Eksik sayısı ve 3Dpreview cache sayısını karşılaştır.
4. 1600x900 dış incelemede bilinen etiket çakışmasını ayrı açık bug'a bağla.

### Beklenen sonuç

- Ana butonlar erişilir; panel viewport dışına taşmaz.
- Semantik pointer fixture ham OS koordinatı kabulünü devralmaz.
- 960x540 küçük metin riski çözüldü varsayılmaz.

### Kanıt

- native-hud-concept-smoke.json
- 5resolution PNG/contact sheet
- UIhit table

### Regresyon

- HUD/font/layout değişiminde; SYS-036/SYS-050.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHudConceptSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/visual-qc/VISUAL-QC-V3.json

## SYS-024 — Normal OS açılışı, menü, çıkış ve smoke'suz davranış

**Alan:** Başlatma · **Öncelik:** P0 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Normal

### Önkoşullar

- Kilidi açık masaüstü; root test penceresini seçmiş; ayrı --home-evidence profili; test harness flag yok.

### Adımlar

1. Gerçek launcher ve EXE'yi ayrı aç; tam path/PID/window/args kaydet.
2. İlk hazır frame'i bekle; Enter/mouse ile menüden kısa akış/serbest oyuna gir.
3. Esc→menü ve görünür Çıkış ile kapat.
4. QAcomponent/otomatik rota/gizli test kilidi/score override dışı yazım olmadığını doğrula.

### Beklenen sonuç

- Paket normal oyuncu gibi açılır ve kapanır; asılı süreç yok.
- Sadece HOME_V48_READY logu UI kabulü yerine geçmez.
- Açık kullanıcı V4.7/V4.8 penceresi kapatılmaz.

### Kanıt

- OS video
- window/PID/args receipt
- ready+exit log
- profile hashes

### Regresyon

- Her aday paket; root tek sahip.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs

## SYS-025 — Portable ZIP çıkarma, tüm dosya hashleri ve lisans

**Alan:** Paket · **Öncelik:** P0 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Portable

### Önkoşullar

- Root yeni test extraction klasörü; mevcut build'e açılmaz.

### Adımlar

1. ZIP SHA/byte değerini LATEST ile karşılaştır.
2. Arşiv yollarını mutlak/..kaçış veya duplicate entry açısından tarat; güvenli yeni klasöre çıkar.
3. EXE,DLL,UnityPlayer,Data,Resources/Managed ve lisansları bütün dosya manifestiyle eşleştir.
4. Çıkarılmış klasörden normal UI kısa smoke çalıştır.

### Beklenen sonuç

- ZIP'teki DLL tek aday native DLL ile eşleşir; eksik bağımlılık yok.
- Arşiv hash eşleşmesi tek başına çıkarılmış oyunun açıldığını kanıtlamaz.
- Önce paket çalışır; eski çıktı ancak sonra ve kullanımda kontrolüyle temizlenir.

### Kanıt

- zip hash
- extracted full manifest
- archive path audit
- normal launch video

### Regresyon

- Her portable üretimi.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Editor/PanicResidenceBuild.cs
- AGENTS.md

## SYS-026 — Temiz Windows ortamında Unity'siz çevrimdışı oynama

**Alan:** Çevrimdışı · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Portable

### Önkoşullar

- Ayrı izinli Windows test kullanıcısı/VM veya temiz PC; ağ kapalı; Unity/Editor/asset cache yok.

### Adımlar

1. Yalnız portable klasörü kopyala; launcher/EXE başlat.
2. Kısa akış ve her afetin ses/görsel kaynağı için örnek geçiş yap.
3. Yazılabilir test profilinde skor yolu ve logu doğrula.
4. Ağ istemi/çalışırken indirme/eksik font/shader/materyal var mı gözle.

### Beklenen sonuç

- Oyun çevrimdışı yerel hedefte bağımlılık indirmeden çalışır.
- Kaynak makinedeki Library/Resources önbelleğine dayanmaz.
- Temiz ortam yoksa vaka BLOCKED kalır, PASS varsayılmaz.

### Kanıt

- machine environment receipt
- network-state proof
- startup/flow video
- player.log

### Regresyon

- Paket/paketbağımlılığı/Resources değişiminde.

### Kaynak referansları

- TooLateToPanic/Packages/manifest.json
- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Editor/PanicResidenceBuild.cs

## SYS-027 — Boşluk, Türkçe karakter, uzun yol ve farklı çalışma dizini

**Alan:** Yollar · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Portable

### Önkoşullar

- Yalnız staging extraction kopyaları; gerçek kaynak taşınmaz.

### Adımlar

1. Kısa ASCII, 'Oyun Denemesi', 'Çağrı Öçİş' ve makul uzun path profilleri oluştur.
2. EXE'yi exe dışı cwd ile başlat; relative launcher yollarını kontrol et.
3. Score/log/evidence konumunun doğru mutlak test kökünde kaldığını doğrula.
4. Her konumda menü→kısa başlangıç→çıkış yap.

### Beklenen sonuç

- Geçerli Windows yollarında asset/launcher erişimi kırılmaz.
- Uzun yol platform sınırı açık yazılır; destek sınırı belirsizse ürün kararı gerekir.
- Kaynak ve kardeş FPS klasörü yeniden adlandırılmaz.

### Kanıt

- path matrix
- PID/cwd/args
- per-path startup log

### Regresyon

- Launcher/package/path kodu değişiminde.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- AGENTS.md

## SYS-028 — Eksik dosyalı veya bozuk arşivde güvenli başarısızlık

**Alan:** Paket hata yolu · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Portable kopyası

### Önkoşullar

- Yalnız bağımsız test extraction kopyası; canlı paket değişmez.

### Adımlar

1. Kopyada DLL/UnityPlayer/Data eksikliği ve kesik ZIP'i ayrı deney olarak hazırla.
2. Başlatma veya extraction hata çıktısını kaydet; hasarlı paketten eski çalışan pakete dön.
3. LATEST/launcher'ın bozuk adaya çevrilmediğini kontrol et.
4. Hata sırasında kişisel/test skor baytlarını karşılaştır.

### Beklenen sonuç

- Eksik paket teslim PASS'ı almaz; eski çalışan aday korunur.
- Kullanıcı profili/veri kaybı ve yanlış otomatik onarım/silme yok.
- Platform hata iletişimi ürünün sorumluluğu ölçüsünde belgelenir.

### Kanıt

- corruption fixture receipt
- exit/error capture
- unchanged latest/profile SHA

### Regresyon

- Paket otomasyonu değişiminde.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Editor/PanicResidenceBuild.cs
- AGENTS.md

## SYS-029 — Cold/warm ready, takılma, crash ve sonlandırma

**Alan:** Başlatma sınırı · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Normal + portable

### Önkoşullar

- Test makinesi özellikleri ve cold/warm tanımı kayıtlı; sadece owned PID.

### Adımlar

1. Cold açılışı3, warm açılışı3 kez ölç; EXE start→ilk kullanılabilir frame zamanını kaydet.
2. Ana menü Çıkış ve OS pencere X yollarını ayrı dene.
3. Crash/hang olursa10sn örneklem/final log/minidump talebini kaydet; yalnız owned PID'yi bitir.
4. Yeni açılışta geçici UI/score dosyası bozulması var mı kontrol et.

### Beklenen sonuç

- Başlatma için önerilen hedef cold≤15sn,warm≤8sn test donanımında; onaylı hedef değilse risk raporu olarak kalır.
- Yanıt vermeyen süreç PASS sayılmaz.
- Kill/quit kullanıcı player'ına yönelmez.

### Kanıt

- startup timing CSV
- PID tree
- exit codes/log
- profile SHA

### Regresyon

- Her aday paket; donanım değişiminde karşılaştırılabilir seri.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/ProjectSettings/ProjectSettings.asset

## SYS-030 — Tuş eşlemeleri, basılı tutma, tekrar ve iki Enter

**Alan:** Fiziksel girdi · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Normal

### Önkoşullar

- Ayrı foreground pencere; gerçek OS klavye/fare aracı; semantik dispatch yok.

### Adımlar

1. F0 kontrol manifestini seç; WASD/ok, Shift, Space, E/F, Tab/B, Q/T, 1–4, Z/X, R, Backspace, Esc ve F2'yi ilgili bağlamda dene.
2. V4.9 ankrajında gerçek vida LMB; diğer uygun eşyada doğrudan kurulum kullan; eski A/D/hold/timing işi bitirmemeli.
3. Return ve NumPad Enter'ı desteklenen menü, çanta ve sinematik yollarında karşılaştır; eski iş polling adayını ayrı tut.
4. Basılı tutma, auto-repeat, release ve hızlı iki basış davranışını kaydet.
5. Görünür yönerge ile davranışı karşılaştır; desteklenmeyen eylem güvenle reddedilsin.

### Beklenen sonuç

- Kontrol sözleşmesi ile gerçek davranış uyumludur.
- NumPad Enter iş/çanta polling kaynağında aday boşluktur; kanıtlanmadan ürün bug'ı veya PASS denmez.
- Space iş sırasında zıplatmaz; modal tuşu dünya eylemi çalıştırmaz.

### Kanıt

- OS key down/up video
- control matrix
- state before/after

### Regresyon

- Input/HUD değişiminde; kaynak adayı OS doğrulaması.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/BUGS.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## SYS-031 — E/F ayrımı, seçili nesne, kapasite ve çanta muhasebesi

**Alan:** Fiziksel loot · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kısa + kampanya

### Önkoşullar

- Cheat kapalı; gerçek yürüyüşle kapsayıcıya ulaş.

### Adımlar

1. E aç/kapat;Z/X veya nesne click ile malzeme seç;F ve görünür Al butonunu ayrı kullan.
2. 12envanter ve8çanta slotunun sınırında pickup/pack/backspace dene.
3. Var/Gerek/Eksik kartının gerçek stokla güncellendiğini izle.
4. İlk yardım craft ve su/gıda tüketimi sonrası slotları karşılaştır.

### Beklenen sonuç

- E gizli pickup/iş yapmaz;F seçili gerçek içerik alır.
- Dolu kapasite stok düşürmez; aynı kare iki pickup olmaz.
- Çantaya koyma ve doğal afet tüketimi tam bir kez görünür muhasebe üretir.

### Kanıt

- continuous OS video
- inventory/bag counts
- HUD captures
- profile hash

### Regresyon

- Loot/inventory/craft/UI değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs

## SYS-032 — V4.9 gerçek OS vida tıklama ve varlığa uygun doğrudan kurulum

**Alan:** Fiziksel iş · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V49_DIRECTACTIONS; eski iş aileleri yalnız V48_FALLBACK için geçerli

### Önkoşullar

- Developer kapalı; gerçek malzeme ve bütçe; closeup mouse ve world installation yolları; F0 freeze.

### Adımlar

1. İki belirgin gerçek vidayı sırayla LMB ile 0.60 sn oturt.
2. Vida dışına, yanlış hedefe ve busy anında çift tıkla; A/D, Space ve Enter ile eski iş tamamlama yolunu dene.
3. Duvar, pencere, kapı ve çatıda uygun güçlendirmeyi dünyada 0.5–0.7 sn karakter inşa pozu ile tamamla.
4. Zemin kattaki yangın istasyonunda gerçek envanter tüpünü tüket; kısa akış işaretçisi ve metni aynı istasyonu göstermeli.
5. Başlangıç, ara ve son commit sınırında Esc/Backspace iptali dene; stok, bütçe, görsel ve sonraki input'u karşılaştır.

### Beklenen sonuç

- İki doğru click ve iki oturma; finalde tek maliyet/önlem commit'i.
- World upgrade alakasız workbench oyunu açmaz; yatak veya kanepeye yangın tüpü bağlanmaz.
- İptal, yanlış ve busy click harcamaz; başarı kesin tarifi bir kez tüketir.
- Yangın istasyonu actual inventory extinguisher tüketir; readiness, hint ve hedef tutarlıdır.

### Kanıt

- OS vida hedefi videosu
- Dünya kurulumu ve istasyon videosu
- Bütçe/malzeme ledger'ı
- Geometri öncesi/sonrası

### Regresyon

- Doğrudan eylem, UI, iş, asset veya kısa akış hint değişiminde.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs

## SYS-033 — Fare taşıma, döndürme, kapı/merdiven geçişi ve iptal

**Alan:** Fiziksel taşıma · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya

### Önkoşullar

- Gerçek OS girdisi; hedefe doğal rota; controller ve collider korunur.

### Adımlar

1. Sağ tık/4 ile taşı; LMB ile bırak; R/teker ile döndür; sağ tık/Esc ile iptal et.
2. Zemin, duvar, merdiven boşluğu, kapı, oyuncu, yanlış kat ve HUD üzerinde bırakmayı dene.
3. Dört kapıyı ve iki merdiveni normal/çapraz rotada yürüyerek geç.
4. Taşınan kapta E/F'nin yeni konumda çalıştığını doğrula.

### Beklenen sonuç

- Geçersiz yerleşim oyuncuyu veya eşyayı kilitlemez; iptal önceki pozu geri getirir.
- Kapı oyuncuyu itmez; güney banyo tek girişinden doğal çıkış mümkündür.
- Orbit ve taşıma aynı fare girdisiyle yanlışlıkla birlikte başlamaz.

### Kanıt

- OS video
- pose/collision screenshots
- route path
- input ledger

### Regresyon

- Placement/door/camera/layout değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomePlacement.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicDoorJamSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicDoorsStairsSmoke.cs

## SYS-034 — HUD,modal ve aynı kare kombinasyonlarının gerçek OS tekrarı

**Alan:** Fiziksel katman · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- İzole foreground pencere; OS olay zamanı kaydedilir.

### Adımlar

1. Tab+B, Tab+E, F+LMB, F2+E ve Esc+click kombinasyonlarını dene.
2. Çanta açılış karesine paketleme click'i; iş iptal karesine dünya click'i yerleştir.
3. Pause→F2→kapat sıralarında önceki pause/help/lock sahipliğini izle.
4. Aynı geçerli eylemi sonraki temiz karede tekrar dene.

### Beklenen sonuç

- Tek olay arka dünya veya HUD tarafından ikinci kez tüketilmez.
- Geçiş karesinde click-through, bütçe veya pickup sızıntısı yoktur.
- Engel kalkınca kalıcı input kilidi oluşmaz.

### Kanıt

- high frame-rate OS video
- event timestamps
- state ledger

### Regresyon

- SYS-016 native kapısından sonra; input veya modal değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionFrameAudit.cs

## SYS-035 — AltTab,focus kaybı ve basılı tuşun takılı kalmaması

**Alan:** Focus · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya + kısa

### Önkoşullar

- Yalnız root'un test penceresi; normal runInBackground=0 ve smoke=true farkı kayıtlı.

### Adımlar

1. Hareket, Space, A/D veya taşıma girdisi basılıyken AltTab yap; dışarıda bırakıp geri dön.
2. İş, pause, çanta, F2 ve sinematik sırasında focus değiştir.
3. Normal focus dışı 30 sn boyunca clock davranışını ölç; ayar ve sözleşmeyle karşılaştır.
4. Geri dönüşte ilk click'in pencere seçimi veya oyun eylemi etkisini kaydet.

### Beklenen sonuç

- Takılı tuş, ücretli tekrar veya istem dışı click-through yoktur.
- Normal ve smoke background saat farkı açık belgelenir.
- Focus saat politikası belirsizse tasarım kararı bekler; smoke sonucu normal kabul sayılmaz.

### Kanıt

- focus/OS video
- clock wall-time table
- runInBackground receipt

### Regresyon

- Input/clock/window ayarı değişiminde.

### Kaynak referansları

- TooLateToPanic/ProjectSettings/ProjectSettings.asset
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs

## SYS-036 — Resize,DPI,letterbox ve ekran değişiminde hit-test

**Alan:** Pencere · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Normal

### Önkoşullar

- Windows %125/%150 DPI ve varsa ikinci monitor; erişilemeyen koşullar BLOCKED.

### Adımlar

1. 960×540–2560×1440 aralığına ek 4:3 ve ultrawide pencere örneği dene.
2. Pencereyi taşı, büyüt, küçült; desteklenen fullscreen/window geçişini kullan.
3. Gerçek pointer ile buton içi, kenarı, dışı ve letterbox boşluğuna tıkla.
4. Çanta, iş, F2 ve sonuç açıkken resize/monitor geçişi yap.

### Beklenen sonuç

- Görsel rect ve gerçek hit bölgesi hizalıdır; boşluk click'i eylem yapmaz.
- Ana butonlar görünür ve erişilirdir; küçük metin insan testinde değerlendirilir.
- Synthetic GUI.matrix pointer fixture bu vakaya PASS vermez.

### Kanıt

- OS screen video
- DPI/resolution matrix
- click coordinate log

### Regresyon

- HUD/font/canvas/window değişiminde.

### Kaynak referansları

- TooLateToPanic/ProjectSettings/ProjectSettings.asset
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionAudit.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHudConceptSmoke.cs

## SYS-037 — OS ile doğal kısa rota ve müdahalesiz 270/300 sn sınırı

**Alan:** Doğal kısa akış · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kısa akış

### Önkoşullar

- Gerçek foreground giriş; developer kapalı; ayrı no-input koşusu; harness yok.

### Adımlar

1. HAZIRLIĞA BAŞLA'dan pickup, çanta, gerçek önlem, deprem, sel, yangın, inceleme ve sonuca oyna.
2. Milestone saatlerini başlangıçtan kaydet; menü beklemesi ve aktif içerik süresini ayır.
3. Ayrı koşuda ilerlemeden doğal saatle 270 ve 300 sn sınırlarını bekle.
4. Eksik hedeflerin sahte tamama dönüşmediğini; tek sonuç, menü ve retry yolunu doğrula.

### Beklenen sonuç

- 270 sn içerik hedefi ve 300 sn üst sınırı doğal wall-clock ile ölçülür.
- API 49.67 sn sonucu gerçek OS veya insan rota kabulü değildir.
- Timeout başarı, önlem veya çanta uydurmaz; tek doğru sonuca gider.

### Kanıt

- continuous OS video with timer
- milestone CSV
- timeout state/log

### Regresyon

- Kısa akış/clock/input değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs

## SYS-038 — Doğal bitiş,Enter/atla ve yarışta tek sonuç

**Alan:** Sinematik · **Öncelik:** P0 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kısa + sim + kampanya

### Önkoşullar

- Her afet için doğal bitiş/skip ayrı koşu; gerçek Return, Keypad Enter ve LMB.

### Adımlar

1. Sinematik başlangıcında, son karede ve reveal sırasında SAHNEYİ ATLA/Enter dene.
2. Çift click ve basılı Enter ile aynı sonucu iki kez çözmeye çalış.
3. Doğal bitişle hasar, tüketim ve ödülü karşılaştır.
4. Kontrolün geri geldiğini; controller, input lock, HUD saati ve ses kuyruğunu izle.

### Beklenen sonuç

- Sonuç, tüketim ve ödül tam bir kez çözülür.
- Skip sonrası kamera/input kilidi veya afet sesi loop'u takılı kalmaz.
- InputLocked HUD sinematik saatini dondurmaz; kayıp metni önce görünür.

### Kanıt

- per-disaster OS clips
- outcome state compare
- audio tail
- timer ledger

### Regresyon

- Cinema/skip/clock değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeCinema.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs

## SYS-039 — Dört doğal simülasyon, gerçek kayıp, inceleme ve aynı afetle retry

**Alan:** Simülasyon · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: 4bağımsız simülasyon

### Önkoşullar

- Deprem, sel, yangın ve fırtına ayrı temiz profil; sonlu 90 sn hazırlık.

### Adımlar

1. Menüde dört afeti sırayla gerçek mouse ile seç ve başlat.
2. Bir gerçek önlem yap; Afeti Başlat ile ayrı no-input hazırlık sonu yolunu dene.
3. Doğal afet ve sonucu bekle; sağkalım ve gerçek kayıp örneklerini ayır.
4. Hasarı İncele→yürü→menü ve aynı afetle retry başlangıcını dene.

### Beklenen sonuç

- Dört seçim doğru afeti üretir; sonuç stabildir.
- Fırtına %74.23 gibi gerçek kayıp başarı sayılmaz; hasar/neden korunur, ödül sıfırdır.
- Tek dalga unranked mod kalıcı skor veya sonraki kampanya dalgası üretmez.

### Kanıt

- 4continuous OS videos
- damage/supply outcomes
- retry selector capture

### Regresyon

- Simülasyon/afet/menü değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs

## SYS-040 — Normal kampanyada çok dalga ve meşru başarı/kayıp

**Alan:** Kampanya · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Serbest oyun

### Önkoşullar

- Developer kapalı; öğreticisiz serbest oyun; ayrı test skoru.

### Adımlar

1. Doğal toplama, üretim, önlem, çanta ve bunker ile en az üç tam dalga oyna; gün sonu geniş turda beş dalga hedefle.
2. Başarı ödülü, kalıcı önlem, sonraki dalga hasarı, şiddet ve stok kararlarını muhasebeleştir.
3. Süre, bunker/çanta ve %65 hasar kayıplarını ayrı doğal koşulda tetikle.
4. Hasar gezisi→devam→menü→yeniden açılış ve skor etkisini doğrula.

### Beklenen sonuç

- Temel kampanya kısa akış/sim fixture'ından bağımsız çalışır.
- Kayıp eşikleri, maliyet ve fizik korunur; meşru ödül yalnız doğru yoldan gelir.
- Eski fullCampaign=false sınırı ancak yeni kanıtın gerçek kapsamıyla değiştirilir.

### Kanıt

- wave-by-wave video
- economy/damage ledger
- score JSON
- new run ID

### Regresyon

- Kampanya, model veya ekonomi değişiminde; geniş tur gün sonunda veya somut yeni riskte.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md

## SYS-041 — Gerçek OS meşru skor yazımı,developer yasağı ve yeniden açılış

**Alan:** Kalıcı skor · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Serbest oyun

### Önkoşullar

- Yazılabilir izole fixture profili; kişisel skor korunur.

### Adımlar

1. Meşru kampanya sonucu üret; skor ekranı ve JSON'u kaydet.
2. Normal kapatıp yeniden aç; liste ve sıralamayı karşılaştır.
3. F2 yalnız açma, seçenek kullanıp kapatma, +1000 ve NoLoss sonuçlarını ayrı dene.
4. Kısa akış ve simülasyon sonunda dosyanın değişmediğini doğrula.

### Beklenen sonuç

- Meşru normal skor kalır; panel açmak tek başına skor yasağı değildir.
- Developer kullanımı sticky skor dışıdır; kısa akış ve simülasyon skor yazmaz.
- OS kabulü gerçek girdiyle üretilmiş sonuca bağlanır.

### Kanıt

- OS result/restart video
- before/after fixture SHA
- score eligibility table

### Regresyon

- Skor/developer/lifecycle değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicDeveloperSmoke.cs
- TooLateToPanic/LATEST-BUILD.json

## SYS-042 — Bozuk profil ve yazılamayan hedefte oynanabilir toparlanma

**Alan:** OS profil hata yolu · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Normal

### Önkoşullar

- Yalnız izole --home-evidence veya ayrı Windows test kullanıcısı; öncesi hash alınır.

### Adımlar

1. Bozuk skor fixture'ı ile harness olmadan normal EXE'yi aç.
2. Geçerli fixture'ı kilitle veya salt okunur yap; meşru skor kaydı dene.
3. Oyuncuya görünen hata ve logu kaydet; izni düzeltip temiz tekrar yap.
4. Yeniden açılışta eski geçerli kayıt korunuyor mu doğrula.

### Beklenen sonuç

- Crash veya kalıcı açılış engeli yoktur; kaydedilemediği anlaşılır.
- Gerçek profil korunur; gerçek diski doldurmak yerine kontrollü I/O reddi kullanılır.
- Model testi UI/toparlanma OS kabulünün yerine geçmez.

### Kanıt

- OS error/recovery video
- fixture hashes
- player warnings

### Regresyon

- SYS-008/SYS-009fix sonrası.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicModel.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs

## SYS-043 — Frame time, cold start, hitch ve afet parçacık bütçesi

**Alan:** Performans · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Donanım, driver, çözünürlük ve güç modu sabit; ölçüm aracının ek maliyeti kayıtlı; aktif eşik F0 quality-policy.json içindedir.

### Adımlar

1. 1600×900 ve 1920×1080'de menü, hazırlık, üst kat, afet ve incelemeden 60 sn örnekler al.
2. CPU/GPU frame p50/p95/p99, 1% low, >100 ms hitch ve working/private memory ölç.
3. 3D preview cache, render texture ve yangın VFX sıcak başlangıcını izle.
4. Aynı rotayı instrumentation kapalı kısa koşuyla karşılaştır.

### Beklenen sonuç

- Ana plan varsayılan önerisi referans donanımında 60 FPS'e yakın akıcılık ve p95 frame süresi ≤33.3 ms; kesin aktif hedef F0 quality-policy.json ile sabitlenir.
- Tekrarlı >500 ms input/afet takılması veya sürekli leak P1 adayıdır; ortalama FPS tek başına kabul değildir.
- Eski donanımdaki tarihsel 55 FPS yeni adayın kabulüne taşınmaz.

### Kanıt

- frame-time CSV/plot
- machine receipt
- memory samples
- OS video

### Regresyon

- VFX, UI preview, renderer veya model performansı değişiminde.

### Kaynak referansları

- TooLateToPanic/ProjectSettings/ProjectSettings.asset
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs
- TooLateToPanic/LATEST-BUILD.json

## SYS-044 — Tekrarlı mod değişiminde bellek, ses ve event sızıntısı

**Alan:** Dayanıklılık · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Sabit test makinesi; önce üç warm cycle; F0 quality-policy.json temel süre ve cycle sayısını sabitler.

### Adımlar

1. Kısa akış, simülasyon, retry, menü ve serbest oyun değişiminde temel seri olarak 30–60 dakika ve en az 10 retry/menü cycle çalıştır.
2. 20 veya 50 cycle yalnız somut leak/tekrar riski için genişletilmiş seri olarak ayrıca planlanır; temel geçit değildir.
3. Temel seride her iki cycle'da memory, AudioSource, texture, event callback ve aktif iş sayısını ölç.
4. Son cycle'da doğal afet ve quit yap; tek sonuç/ses/buton etkisini doğrula.
5. GC sonrası monoton büyüme varsa tekrar üreten rotayı daralt; gerçek süre ve cycle sayısını raporla.

### Beklenen sonuç

- Çift cue, event listener, eksik destroy veya giderek azalan FPS yoktur.
- Temel seri ana planla aynıdır: 30–60 dakika +10 retry/menü cycle; aktif karar F0 quality-policy.json içindedir.
- 20/50 cycle yalnız gerekli riskte genişletilmiş kanıttır; çalıştırılmadıysa temel gate başarısızlığı sayılmaz.
- Gerçek yürütülen cycle/süre raporlanır; planlanan sayı PASS yapılmaz.

### Kanıt

- cycle ledger
- memory/object CSV
- audio/event captures
- final log

### Regresyon

- Lifecycle/audio/texturecache değişiminde;günsonu geniştur.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs

## SYS-045 — Mod başına doğru cue,tek çalma ve sahne sonrası temizleme

**Alan:** Ses yönlendirme · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kısa + serbest + sim

### Önkoşullar

- Sistem ses kaydı mümkün; volume ve çıktı cihazı kaydedilir.

### Adımlar

1. Yedi kısa konuşma, üç SFX ve afet sesini cue ID/milestone ile eşleştir.
2. Kampanyada eski genel konuşmanın çalmadığını ve afet sesinin çaldığını dene.
3. Skip, retry, pause ve menüde overlap, tekrar veya loop kalıntısını kaydet.
4. Mute, volume=0 ve cihaz değişiminde oyun/input/clock devamını gözle.

### Beklenen sonuç

- Kısa akış cue'ları doğru anda bir kez çalar; kampanya politikası korunur.
- Import ve süre PASS'ı duyulurluk/kalite kabulü değildir.
- Ses cihazı yokluğu temel oyun döngüsünü durdurmaz.

### Kanıt

- systemaudio/video
- cue timeline
- audio route source mapping

### Regresyon

- Audio/cinema/reset değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs
- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryWorld.cs

## SYS-046 — V4.8 iki hasar etiketi çakışması yeniden üretim ve dar düzeltme

**Alan:** Bilinen görsel bulgu · **Öncelik:** P2 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kısa inceleme

### Önkoşullar

- Geçmiş bulgu short_inspection_tag_overlap; 08-short-inspection.png; 1600×900; DLL 5683E713... .

### Adımlar

1. Kısa dış incelemeye gerçek OS rotasıyla ulaş; aynı kamera açısını yakala.
2. Beş çözünürlük ve orbit açılarında etiket rect çakışmasını ölç.
3. Dar fix varsa etiket önceliği, kat, kamera arkası ve ekran kenarını karşılaştır.
4. Hedef metni, ana CTA ve gerçek hasar bilgisinin kaybolmadığını doğrula.

### Beklenen sonuç

- Geçmişte görsel olarak gözlenmiş P2 ayrı tutulur; bu snapshot'ta yeniden üretildi denmez.
- Fix sonrası etiket okunur; hasar ve sonuç modeli değişmez.
- Diğer hipotezler bu tarihsel gözlemle aynı kanıt statüsüne konmaz.

### Kanıt

- same-camera before/after PNG
- rect overlap table
- OS video
- bug-ledger row

### Regresyon

- HUD/camera/damage-label fix sonrasıSYS-023/SYS-036/SYS-038.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/visual-qc/VISUAL-QC-V3.json
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md

## SYS-047 — Materyal,gölge,VFX,rig,collider ve etkileşim birlikte QC

**Alan:** Görsel bağlama · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- Yeni aday build/asset manifesti; stüdyo kareleri final sahne kabulü değildir.

### Adımlar

1. V4.9 karakter cilasını retained CC0 humanoid, rig, Avatar, clip, root motion, controller ve lisansla birlikte incele.
2. İki kat, balkon, banyo, bodrum, garaj, bahçe ve 12 prop'u doğal rota/ışıkta incele.
3. Seçili vurgu, pickup hareketi/yayı, vida braketleri, kurulum geometrisi/karakter pozu ve iki kontrollü afet tepkisini videoda izle.
4. Materyal, gölge, animasyon, collider, etkileşim, VFX ve SFX için gerekli/not_applicable/eksik kaydı tut.
5. Yangın sonrası asılı alev/yanık mesh; cutaway, güçlendirme reset ve collider tutarlılığını dene.

### Beklenen sonuç

- Pink/eksik mesh, okunmaz hedef veya temel rota engeli yoktur.
- not_applicable gerekçelidir; gereken eksik açık bulgudur.
- 32 FBX ve 12 stüdyo karesi bütün kütüphane sanat kabulü değildir.

### Kanıt

- scene video/contact sheets
- asset QC matrix
- collider/interaction evidence

### Regresyon

- Asset/light/material/VFX batch'inde.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/visual-qc/VISUAL-QC-V3.json
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Art/PanicCharacterArt.cs

## SYS-048 — İlk oyuncuda 180 sn kazanım ve 270+30 sn kısa akış

**Alan:** İnsan süre ve keyif · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kısa akış

### Önkoşullar

- En az üç ilk oyuncu; beş kişi isteğe bağlı örneklem artırımıdır; önceden geliştirici rota öğretimi yok; aynı aday ve kilidi açık Windows; aktif insan politikası F0 quality-policy.json içindedir.

### Adımlar

1. Yalnız görünür başlangıç metniyle oynamayı iste; gereksiz yardım verme ve zamanlı koşuyu soru için durdurma.
2. İlk pickup, çanta, gerçek önlem, görünür afet ve anlamlı sonuç saatlerini dış saat/video ile kaydet.
3. 180. saniyedeki anlama durumunu koşu sonrasında ilgili klibi yeniden gösterip açık soruyla değerlendir; zamanlı rotaya soru molası ekleme.
4. 270/300 sn bitiş, yardım, duraksama, akıcılık, keyif ve devam isteği 1–5 ile serbest gerekçeleri ayrı topla.

### Beklenen sonuç

- 49.67 sn hazırlanmış API hızı insan hedefi yerine kullanılmaz.
- Ana planın küçük örneklem önerisi: üç kişinin tümü yardımsız ilk amacı bulur ve gerçek bir önlemin nedenini açıklayabilir; başarısız kişi ve etkisi gizlenmez.
- İlk 180 sn kazanım/anlama, 270 sn içerik ve 300 sn kapanış ayrı ölçülür; aktif başarı kararı F0 quality-policy.json ile sabitlenir.
- Beş kişi isteğe bağlı örneklem artırımıdır; sabit 4/5 geçidi yoktur. Anlama, süre ve keyif ayrı verilir; örneklem tüm kitleyi temsil etmez.

### Kanıt

- consented session timings
- anonymized notes
- task success table
- enjoyment answers

### Regresyon

- İlkakış/hedefmetni/işzorluğudeğişiminde;final kabul.

### Kaynak referansları

- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/LATEST-BUILD.json

## SYS-049 — Türkçe yedi cue, üç SFX ve afet sesi için insan kabulü

**Alan:** İnsan ses kabulü · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kısa + kampanya

### Önkoşullar

- Aynı aday; kulaklık ve hoparlörde makul sistem volume; metinle karşılaştırma.

### Adımlar

1. Yedi cue'yu oyun içinde doğru zamanda dinle; telaffuz, kesilme, clipping ve tempo sorunlarını kaydet.
2. Afet sesi altında yönerge anlaşılması ve SFX rahatsızlığını değerlendir.
3. Skip/retry sonrası ses örtüşmesini ve kampanyada eski konuşmayı kontrol et.
4. Ses kapalıyken görünür bilginin yeterli olduğunu dene.

### Beklenen sonuç

- İnsan dinlemeden humanListeningAccepted=true olmaz.
- Ses ve HUD çelişmez; kritik yönerge yalnız sese bağlı kalmaz.
- Yeni ücret veya yeniden üretim ihtiyacı ayrı iş/onaydır; otomatik kredi kullanılmaz.

### Kanıt

- human listening checklist
- in-game audio timestamps
- device/volume notes

### Regresyon

- Sesmix/cue/metindeğişiminde.

### Kaynak referansları

- TooLateToPanic/LATEST-BUILD.json
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs

## SYS-050 — Küçük ekran,öğretici,iş yönergeleri ve hareket rahatlığı

**Alan:** İnsan okunurluk · **Öncelik:** P1 · **Katman:** HUMAN

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm

### Önkoşullar

- 960×540 ve 1280×720 dahil en az iki boyut; ilk kez fare/klavye kullanan oyuncu.

### Adımlar

1. V4.9 yönergesini seç; eski A/D, timing ve hold metni görünmemeli.
2. Ana hedef, kesin eksik adetler, iki vida/kurulum aşaması, kayıp nedeni ve sonraki adımı yardım olmadan anlattır.
3. Renk tek başına bilgi, küçük metin, kontrast, focus göstergesi ve button hedef boyutunu incele.
4. Kamera, deprem ve pickup hareketinde rahatsızlığı sor.
5. Öğretici bitirme, N, Esc/Backspace ve desteklenen Enter yollarını yönergelerle eşleştir.

### Beklenen sonuç

- Piksel olarak sığması okunurluk kabulü değildir.
- Kritik bilgi okunur; erişilebilirlik veya hareket rahatsızlığı açık bulgu kalır.
- Cila uğruna ek görev veya bekleme yaratılmaz.

### Kanıt

- task comprehension notes
- annotated screenshots
- motion discomfort feedback

### Regresyon

- HUD/font/tutorial/input yönergesi değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHudConceptSmoke.cs
- Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/REPORT.md
- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md

## SYS-051 — Gerçek ürün hatası,test aracı hatası ve çevre blokajını ayırma

**Alan:** Oracle ve hata sınıflama · **Öncelik:** P1 · **Katman:** SOURCE

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Tüm testler

### Önkoşullar

- Her koşuda runner, oracle, ürün DLL kimliği ve çevre bilgisi vardır.

### Adımlar

1. Eksik check, boş rapor, error listesi, timeout ve yanlış controller-enabled beklentisini reddeden validator hazırla.
2. V1 yanlış terminal controller oracle'ı ve V2 observer race geçmişini oku.
3. Yeni FAIL'de ürün durumunu bağımsız assertion veya OS gözlemiyle karşılaştır.
4. Başarısız koşuyu silmeden ürün, test aracı veya çevre olarak sınıflandır; retest ID bağla.

### Beklenen sonuç

- Harness düzeltmesi ürün fix'i gibi sunulmaz; ürün bug'ı test gevşetilerek geçirilmez.
- Windows kilit ekranı BLOCKED çevredir; rota/kampanya PASS değildir.
- Tamamlanmış zorunlu check kümesi ve aynı DLL'a ait log olmadan ileri kabul verilmez.

### Kanıt

- result validator rules
- failure triage row
- original/retest reports

### Regresyon

- Her FAIL/timeout ve her harness değişiminde.

### Kaynak referansları

- TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md
- TooLateToPanic/Assets/Game/Runtime/PanicFullFlowSmoke.cs
- TooLateToPanic/Assets/Game/Runtime/PanicInteractionAudit.cs
- Evidence/GamejamOrchestra/TLTP-FULLFLOW-QA-20261017/logic-audit/V2-HARNESS-RACE.md

## SYS-052 — Pause, F2, Freeze, focus ve sinematik saat sahipliği

**Alan:** Saat sahipliği · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** SHARED, F0 davranış profili seçilerek: Kampanya + kısa + sim

### Önkoşullar

- Aynı aday; wall-clock video; normal runInBackground politikası önce kararlı.

### Adımlar

1. Hazırlıkta Esc pause, F2 paneli ve Freeze'ı ayrı aç/kapat; 30 sn sonra hazırlık, kısa üst limit, iş ve sinematik saatlerini ölç.
2. InputLocked sinematikte HUD countdown, skip ve doğal bitiş ilerlemesini izle.
3. Timeout/NoLoss açık-kapalı sonucu ve panel geçiş karesi input blokajını dene.
4. SYS-035 focus testini ilişkilendir; geri dönüşte önceki pause/lock sahipliğini doğrula.

### Beklenen sonuç

- Panel önceki pause/help/lock sahipliğini bozmaz.
- Freeze yalnız yetkili hazırlık saatini etkiler; InputLocked HUD sinematik saatini dondurmaz.
- Kısa akış 270/300 sn sınırı cheat freeze ile sonsuzlaşmaz; belirsizlik F0 ürün kararıdır.

### Kanıt

- clock ownership matrix
- OS timed video
- before/after model timers

### Regresyon

- Clock/input/F2/cinema değişiminde.

### Kaynak referansları

- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJuryDemo.cs
- TooLateToPanic/Assets/Game/Runtime/PanicSimulationWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicJurySmoke.cs

## SYS-053 — V4.9 ortak eylem kuralı: HUD, klavye ve API aynı ret

**Alan:** Eylem sahipliği · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** V49_DIRECTACTIONS

### Önkoşullar

- Yeni ortak action rules adayda derlenmiş; F0 manifesti sabit; sonlu bütçe.

### Adımlar

1. Uzun/dengesiz mobilya, duvar/pencere/kapı/çatı, yatak/kanepe ve container için eylem census çıkar.
2. HUD'da desteklenmeyen eylemin gizlendiğini doğrula.
3. Aynı yasak eylemi klavye ve Expedition/World API yollarından dene.
4. Geçerli eylemin begin, completion ve cancel yollarında aynı tarifi kullandığını doğrula.
5. Yatak/kanepe yangın eylemini reddet; istasyonda gerçek envanter tüpü ile başarı denetle.

### Beklenen sonuç

- HUD gizlemesi tek sınır değildir; keyboard/API uygunsuz eylemi de reddeder.
- Ret durumunda stok, bütçe, readiness ve görsel geometri değişmez.
- Uygun container açma/taşıma ayrı kalır; yanlış eşya yangın hazırlığı üretmez.

### Kanıt

- Hedef×eylem×input matrisi
- Native ret JSON
- Sonlu maliyet ledger'ı

### Regresyon

- Rules, Expedition, HUD veya hedef değişiminde; fullflow ve dört simülasyonda etkilenen yollar.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs

## SYS-054 — V4.9 üç bahçe bölgesinde olgun ağaç sırası ve geçiş güvenliği

**Alan:** Bölgesel ağaçlar · **Öncelik:** P1 · **Katman:** OS_E2E

**Mod / uygulanabilirlik:** V49_DIRECTACTIONS

### Önkoşullar

- Üç bölgenin kit kimliği/adetleri F0 manifestinde; normal maliyet ve gerçek kapsül.

### Adımlar

1. Her bölgeye doğal rota ile ulaş; tek 0.60 sn kurulumda birkaç olgun ağaçtan oluşan sıra üret.
2. İptal, duplicate click ve eksik kitte bölge sayısı/stok muhasebesini karşılaştır.
3. Yol, garaj, bunker, kapı ve oyuncu kapsülü sınırlarını yürü/koş ile dene.
4. Bölge adı, tarif ve stok sunumunda ekim/fidan/sulama görev yükü kalmadığını doğrula.
5. Afet, menü, retry ve yeni koşuda geometri/state resetini karşılaştır.

### Beklenen sonuç

- Her bölge bir kez tamamlanır ve birden fazla olgun ağaç gösterir.
- Tekil fidan/sulama görevi geri dönmez; uyumluluk için legacy internal ID kalabilir.
- Ağaçlar kritik rota veya kapsülü engellemez; stok finalde bir kez tüketilir.

### Kanıt

- Üç bölge OS videosu
- Sıra/tarif adetleri
- Rota/collision kareleri
- Reset karşılaştırması

### Regresyon

- Ağaç factory, bölge, tarif, layout veya reset değişiminde.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicExpedition.cs

## SYS-055 — V4.9 1.35 hız, animasyon ve katmanlı güçlendirme geometrisi

**Alan:** Hız ve inşa geometri · **Öncelik:** P1 · **Katman:** NATIVE_API

**Mod / uygulanabilirlik:** V49_DIRECTACTIONS

### Önkoşullar

- CC0 rig, Avatar, controller ve clip temel hashleri; düz ölçüm rotası; controller boyutları kayıtlı.

### Adımlar

1. Gerçek frame movement ile walk 3.645 ve run 5.67 m/s ölç; 2.7/4.2 tabanına karşı 1.35 oranını hesapla.
2. Animasyon rate, ayak kayması ve build pozunu üretim yollarından ölç/gözle.
3. Duvar/pencere tier 1/2 güçlendirmede 0.5–0.7 sn, hedef 0.60 sn gerçek geometri ilerlemesini doğrula.
4. Kapı/pencere açıklığı, hinge, collider, cutaway, damage ve reseti doğal rota ile dene.
5. StepOffset, slope, height, radius, root motion ve clip kümesi farkını değişmez manifestiyle karşılaştır.

### Beklenen sonuç

- Gerçek hız 1.35 artar; Warp, daha büyük step/slope veya collider küçültme yoktur.
- Hareket animasyonu yeni hıza uyar; retained rig/clip/root-motion politikası korunur.
- Tier farkı gerçek geometridir; fizik, cutaway, damage ve reset tutarlıdır.

### Kanıt

- Hız/zaman CSV
- Controller/rig hash farkı
- Geometri progress kareleri
- Rota/collider oracle

### Regresyon

- Hız, karakter, construction art, controller veya layout değişiminde; SYS-018/033/043.

### Kaynak referansları

- Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/CONTRACT.md
- TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs
- TooLateToPanic/Assets/Game/Runtime/PanicHomeLife.cs
- TooLateToPanic/Assets/Game/Art/PanicCharacterArt.cs
