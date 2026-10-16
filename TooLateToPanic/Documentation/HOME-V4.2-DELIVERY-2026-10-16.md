# Too Late to Panic! — Home V4.2 kapı ve merdiven düzeltmesi

16 Ekim 2026 · Unity 6000.0.73f1 Built-In · Windows · 0.4.2.

Kullanıcı kapının her seferinde geçiş yönüne açılmasını ve merdiven korkuluklarından geçip kat değiştirirken düşmenin düzeltilmesini istedi.

Dört gerçek geçiş kapısı açılma anında oyuncunun kapalı kapı düzleminin hangi tarafında olduğuna göre ±100 derece yön seçer. Kapı oyuncudan uzağa açılır; açıkken karakter öbür tarafa geçince yönünü ters çevirmez. Kapanış E ile aynı menteşenin kapalı pozuna döner; yeniden açıldığında güncel taraf kullanılır. Dolap, çekmece ve bunker kapağı bu yön seçimine girmez.

Merdiven korkuluklarının eski görsellerine karşılık fizik yoktu. Her uçuşun 16 basamağı boyunca iki yanda toplam 32 dar panel eklendi; basamak yüksekliğini takip eder. İki varış katında merdiven boşluğunun yanları, üst katta da güney ucu korunur. Zemin güneyi bir üst uçuşun girişi olduğu için açık tutuldu. Sahanlık/kuzey girişleri ve 1.5m basamak genişliği korunur. Büyük birleşik merdiven colliderı yapılmadı; alttaki uçuşun baş mesafesi kapanmaz. Basamak ve korkuluk fizik kökü görünürlük listesinden ayrıldı; kat kesiti fizik engellerini kapatmaz. Zemin kat boşluğunun eksik görsel korkulukları ve üst güney korkuluğu da mevcut paletle tamamlandı.

Karakter yarıçapı, yerçekimi, stepOffset, afet/bütçe/erzak/kayıp kuralları değişmedi. Home V4.1 tek girişli banyo korunur; sabit içeri açılma tercihini son dinamik açılma isteği günceller. Yalnız afet sesi etkin kalır; yeni dış asset/sağlayıcı/harcama/karakter üretimi yoktur. Windows ürün/kayıt kimliği V4 olarak korunur. FPS projesine veya gerçek kayıtlara dokunulmaz.

## Kanıt

Kaynak, staging ve native kanıt: Evidence/GamejamOrchestra/TLTP-DOORS-STAIRS-V42-20261016. Yeni sanat için tam art bake gerekir. Kaynak derlemesi, son DLL/EXE, kaynak manifesti ve opt-in --doors-stairs-smoke sonucu LATEST-BUILD.json ile eşlenir. Normal test-argsız açılış ayrıca kontrol edilir. Önceki V4.1 16/16 sonucu bu yeni DLL'nin kanıtı değildir.

Kontrol yalnız kapı yönü, açık yaprağın kararlılığı, çarpışmalı korkuluklar ve fiziksel merdiven çıkış/inişi kapsamındadır. Taze GUID skor profili normal kayıt yolundan ayrılır. Kurulum yerleştirmeleri raporda sayılır; kat geçişi diye ışınlanma kullanılmaz. API girdileri ve gerçek CharacterController hareketidir; manuel oynanış, uzun kampanya veya nihai görsel kabul değildir. Son Windows build başarılı: 0 hata/0 uyarı. Son DLL ile 11/11 PASS, 0 çalışma hatası, 57,82 saniye. Dört kapı/sekiz taraf gerçek E ve fiziksel eşik geçişiyle doğrulandı. Dört kapı başlangıç yerleştirmesi ve tek bodrum yerleştirmesi vardır; bundan sonraki bodrum–zemin–üst kat–zemin–bodrum rotası bütünüyle fiziksel yürüyüştür. İki native render incelendi; normal test-argsız HOME_V42_READY açılışı gözlendi. İlk 7/11 koşu kabul edilmedi: test, kapı kapanma yayı içinde duran karakteri sonraki ters taraf sanıyordu; yalnız test rotası kapanma öncesi yürüyerek güvenli alana çıkıp sonra tekrar yaklaşacak şekilde düzeltildi. Oyun fiziği/kuralları ve yön hesabı test için değiştirilmedi. İkinci koşu kabul kanıtıdır.

## Dosya ve sahiplik

PanicHomeWorld: kapı açılış yönü ve opt-in test bağlantısı. PanicHomeLife.BuildStairFlight: kalıcı fizik ve dar korkuluk colliderları. PanicHomeArt.BuildFloor: mevcut stile uygun görsel korkuluklar. Art/kolizyon ve QA ayrı staging ajanlarında çalıştı; root canlı entegrasyon, Unity/build/player ve teslim sahibidir. Mevcut ASSET-CREDITS.md ve Existing-Player-Provenance.md lisansları korunur.


Assembly-CSharp SHA256: B728B979129F8D747B477377C31EDBB6D6BD0DA61B9F5822EA460B0A89E6C855. Son profil native-v2-5512a13c-0d42-4237-a130-23cadb7eb904. Kaynak/build: source-build-v2; tam art bake source-build-v1. Kaynak ve 104 prefab cache manifesti sıfır farkla doğrulandı. Native test raporu/screenshotları çalışma alanı Evidence ağacındadır; portable paket bu kaynak kanıt ağacını içermez.

Güncel yollar ve temizlik receipt'i LATEST-BUILD.json içindedir. Açık kullanıcı oyuncuları kapatılmadı. Kullanılmayan eski çıktılar Geri Dönüşüm Kutusu'na alındı; kullanımda olan eski klasör korunur ve bir sonraki temizlikte süreçler yeniden kontrol edilir.
