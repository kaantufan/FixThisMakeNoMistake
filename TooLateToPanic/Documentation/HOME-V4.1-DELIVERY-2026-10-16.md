# Too Late to Panic! — Home V4.1 banyo düzeltmesi

16 Ekim 2026 · Unity 6000.0.73f1 Built-In · çevrimdışı Windows · ürün sürümü 0.4.1.

Kullanıcının son banyo görüntüsünde eski üst kat bölmesi ile ayrı banyo duvarı arasında dar bir geçit ve iki etkileşimli kapı bulunuyordu. Üst kattaki eski bölme/çerçeve/kapı kaldırıldı. Banyo batı duvarı x3 yerine x2 oldu; oda mevcut kuzey/doğu dış kabuğuna birleşti. Önceki geçit kesintisiz banyo fayansına katıldı. Sahanlığa doğrudan açılan tek kapı bulunur; kapı banyoya doğru açılarak merdiven yaklaşımını açık bırakır. Ecza dolabı yeni batı duvarına taşındı. Alt katın kapısı ve bölmesi korunur.

Bu bir yerleşim düzeltmesidir. Üç aktif iş ailesi, renkli mobilyalar, envanter/çanta, afet döngüsü ve yalnız afet sesi kararı Home V4 tabanından korunur. Yeni insan, üçüncü taraf asset veya ücretli sağlayıcı işi yoktur. Windows productName V4 kimliğinde tutulur; yama normal yerel highscore klasörünü değiştirmez. Bağımsız FPS projesine dokunulmadı.

## Kanıt

Güncel durum ve hashler LATEST-BUILD.json içindedir. Kaynak, art staging, entegrasyon notları, derleme ve native kayıtlar üst çalışma alanındaki Evidence/GamejamOrchestra/TLTP-BATHROOM-V41-20261016 klasöründedir. Home V4 12/12 sonucu tarihsel kanıttır; bu yeni DLL için ayrıca kısa banyo kontrolü yapılır.

Kontrol --bathroom-smoke --home-evidence <yeni izole GUID klasörü> ile çalışır. Yalnız başlangıç yerleştirmesinde bir ışınlanma vardır; kapalı kapıya yaklaşım, açık kapıdan iki yönlü geçiş, yeniden giriş, eski geçit zemini, duvar ve dolaba yaklaşım gerçek CharacterController hareketidir. Otomatik hedef seçimi ve normal Q/E/F API girdileri kullanılır; hedef zorla atanmaz, fizik/çarpışma/erzak/kayıp sınırları değiştirilmez. Bu elle oynanış, tüm ev rotası, uzun kampanya veya nihai görsel kabul değildir.

Son build başarılı: 0 hata / 0 uyarı. Son DLL ile 16/16 PASS; 0 çalışma hatası, 13,91 saniye, yaklaşık 49,62 FPS. Gerçek kapı eşiği doğuya iki, batıya bir kez geçildi; toplam bir başlangıç ışınlanması vardır. Banyo kapısı ve dolabı otomatik seçildi (Q gerektirmedi). Dört gerçek native kare incelendi. Normal test-argsız açılış HOME_V41_READY ile gözlendi. Kaynak manifestinde 42 dosya ve 104 prefab cache kontrolünde sıfır fark bulundu. İlk 14/16 koşu kabul edilmedi: yalnız testte bake materyal adı soneki, hareket frenlemesi ve asenkron PNG bekleme düzeltildi; oyun fiziği/kuralları değiştirilmedi. İkinci koşu son DLL kanıtıdır. Test gerçek kayıt yolunu kullanmadı; mevcut kullanıcı oyunu kapatılmadı.

## Kaynaklar

PanicHomeArt.BuildFloor, PanicResidenceArt.BuildBathroom, PanicHomeLife.ConfigureResidenceModel, PanicExpedition banyo kaynağı, PanicHomeWorld kapı menteşesi ve opt-in PanicBathroomSmoke. Ekipte sanat/geometri ve kısa QA ayrı staging alanlarında çalıştı; canlı entegrasyon, Unity, native oyuncu ve teslim yalnız root tarafından yönetildi. Mevcut asset/lisans kayıtları ASSET-CREDITS.md ve Existing-Player-Provenance.md içinde korunur.


Assembly-CSharp SHA256: 5231FAC344E9DEE03C4CF3C35589A803A5DBC5CC5DBAAEF2FAC4338685E50B33. Son kanıt profili: native-v2-dba44e40-8e2b-4cce-8232-d85196caa8c0. Kaynak/build kaydı source-build-v2; tam art bake source-build-v1 içindedir. Native JSON/screenshots çalışma alanının Evidence klasöründedir; taşınabilir paket bu kaynak kanıt ağacını içermez.

Temizlik: kullanılmayan Home V3 klasörü ve eski V4 ZIP doğrulanmış Builds sınırından Geri Dönüşüm Kutusu'na alındı. Açık eski V4 oyuncusu PID33696 ve klasörü korundu. Yeni normal V4.1 PID32700 açıldı; launcher yalnız V4.1'i gösterir. Bunlar teslim anı PID'leridir; sonraki temizlikte yeniden kontrol edilir.
