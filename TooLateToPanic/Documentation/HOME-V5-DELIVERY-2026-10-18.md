> Public publication copy: the selected source manifest differs in scope from the full local build manifest. Native results below describe the recorded package only. OS testing state is a snapshot and may still be in progress. [Download](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.5.0-r5), [source/build comparison](Publications/v0.5.0-r5/source-vs-build.json). Local-only historical evidence links remain local references.

# Too Late to Panic! — Home V5

Taşınabilir ZIP219371265bayt; SHA256 `D0C03FDB543D9E75C15E511A7786F1D69A39A79DAC3CBD46A928E2CDCE792C36`. ZIP içindeki EXE ve DLL test edilen dosyalarla byte/hash eşleşir. Normal, test bayraksız r5 oyunu açıldı; hazır logu ve0runtime hata gözlendi, PID29716 kullanıcıya bırakıldı. Başlatıcı ve LATEST r5'i gösterir. Bu görevin kendi r1–r4 adayları ve r4 ZIP'i doğrulanmış mutlak Builds sınırında temizlendi; bir V5 build ve bir V5 ZIP kaldı. Önceki reddedilmiş V4 temizlik hedeflerine dokunulmadı. Son birleşik kayıt `Evidence/GamejamOrchestra/TLTP-V5-20261018/manager/FINAL-RECEIPT.json`.

18 Ekim2026. **Home V5-r5 /0.5.0, kullanıcı oyun testine hazır.** Kullanıcının iki ekran görüntüsü ve hata/asset listesi üç aynı-dizin fork sohbetinde uygulandı; ana sohbet Unity/import/build/player entegrasyonunu yönetti. [LATEST-BUILD.json](../LATEST-BUILD.json) test edilen derleme, kaynak manifesti ve taşınabilir ZIP kimliğini bağlar. Önceki V4.14 sonuçları V5 kabulü değildir.

## Oynanış ve ortam

- Banyonun kuzeydoğu penceresi oyun hedefiyle birlikte kaldırıldı; mimari açıklığı ve fiziksel boşluk kapandı. Deprem dış çekiminde yedi komşu ev de kendi temeli çevresinde sarsılır; yollar/ağaçlar ve oyuncunun gerçek fiziği hareket ettirilmez.
- Sel güvenli yeri üst kattaki panik odasıdır. Mevcut çalışma odasının fiziksel kapısı korunur; yükseltilmiş kuru oturma alanı, battaniyeler, görünür güvenlik işareti ve bataryalı acil ışık eklendi. Diğer afetler bodrum sığınağını kullanır. Giriş doğru kat, gerçek ışın/yol, erişim ve controller denetiminden geçer. Yanlış yer sığınmış sayılmaz.
- Zamanında yetişemeyen oyuncunun gerçek konumunda ayrı kayıp sahnesi oynar; modelin ReachedShelterV5 durumu gerçek kabulün tek kaynağıdır. Geliştirici kaybetmeme koruması bile sahte sığınak girişi üretmez. Başarılı sel sahnesi üst katta bekler; bodrum animasyonu/ışınlama kullanmaz.
-14 gerçek malzeme kaynağı ve16 açık erzak noktası vardır. Altı eski dekor depolama gerçek sonlu loot kaynağı oldu. Dalga başına bir kez küçük adet değişimi ve aynı kattaki kaynaklar arasında sınırlı stok dağılımı yapılır; dolabı yeniden açmak stok üretmez. Sonraki dalga yapı malzemelerini de yeniler. Üç ağaç sırasının asgari paketi korunur.
- Battaniye ve üst yatak gıdası gerçek örtü yüzeyinin üstündedir. Yatağın tüm hacmini kapatan collider yerine yatak/gövde ve başlık ayrı fizik parçalarıdır. Pickup seçme hacmi gerçek görseli kapsar; engel delerek toplama yoktur. Envantere sığan gerçek miktar kadar kısmi toplama sonlu stoktan düşer.
- Ana şalter ev ışıklarını, TV/cihaz ışımalarını, bilgisayar ekranını ve elektrikli garaj kapısını kapatır. Bataryalı acil işaretler ve ateş ayrı fiziksel kaynaklardır. Kapalı vana/şalter ilgili ikincil olayı engeller. Açık tesisatta afete göre en fazla bir küçük1.2–2.8 yüzde puanlık RNG olay gerçek hasar ve sonuç metnine eklenir; aynı dalgada yeniden çekilmez.
- Depolama ve masa/sandalye yönleri okunur oda erişimine göre düzeltildi. Yeni çok kapaklı dolapların açılma/kapanması controller kapsülüne yaklaşınca durur, oyuncuyu itmez. Sırf boş dekor için gathering çağrısı üretilmez.

120 saniye önlem+45 saniye çanta,1su+1gıda,75% kayıp, gerçek maliyet/ücretsiz iptal, eldeki humanoid UAL hareketi ve skor yalıtımı korunur. Bağımsız eski FPS denemesi bu çalışma kapsamında değiştirilmedi. Fırtınanın normal dalga havuzunda bulunmaması V4.14'ten gelen ayrı açık sınırdır; V5 tüm kampanya kabulü iddia etmez.

## Arayüz ve anlatım

HUD dalga/afet, aşama saati, bütçe ve tek bağlamsal hedefe indirildi. Büyük kaynak duvarı ve dünya üstünde tekrarlanan E kartları kaldırıldı; ayrıntılar Tab/J'dedir. Köşedeki küçük TUŞLAR düğmesi veya F1 rehberi açar/kapatır. H tam yardımı korur. Çanta aşaması eksik gerçek erzağı ve afete uygun güvenli yerin yön/kat/mesafesini gösterir.

Defter sıcak çizgili kâğıt, kenar çizgisi/dokusu, bez kapak, sayfa katmanları, ayraç ve çift kıvrımlı metal spiral ile güncellendi; veri gerçek Model/Expedition snapshot'ıdır. Başlangıç dört kısa çekimde neden hazırlandığımızı,120+45 aşamalarını, temel etkileşimleri ve doğru güvenli yeri anlatır. Intro hazırlık saatini ilerletmez. Çekim geçişleri yumuşar; Enter atlama ve bir kez sonuç çözümü korunur.

## Sanat ve sahiplik

Ambulans, polis sedanı, itfaiye, kurtarma kamyonu, zodiac; polis, paramedik, kurtarma personeli, itfaiyeci; büyük/küçük raf ve üç dolap yerel özgün prosedürel sanatla oluşturuldu. Araçlarda teker/kapı/ekipman, compound collider, kinematik güzergâh, gerçek uyarı ışıkları ve afet sonrası ekip bağlantıları vardır. Sel botu yalnız gözlenmiş sel sonrasında görünür; pervane, su çizgisi hareketi ve ekip vardır. Bunlar sürülebilir araç veya akışkan simülasyonu değildir.

Ana karakterin düz beyaz göğüs paneli kaldırıldı; indigo sivil ceket ve küçük okra yaka uygulandı. Humanoid/Avatar/UAL controller ve normal hareketin sahibi korunur. Görevli kıyafet/başlık/aksesuar renk ve siluetleri ana karakterden ayrılır.

Yeni ücretli servis, kredi veya indirme kullanılmadı. Yeni sanat kaydı [HOME-V5-ASSETS-2026-10-18.md](HOME-V5-ASSETS-2026-10-18.md); önceki vendor/font/ses lisansları korunur.

## Doğrulama

İlk r1 denemesi Unity native nesnesinin MonoBehaviour alan başlatıcısında oluşturulması hatası nedeniyle kabul edilmedi ve yalnız bu kökün build süreci durduruldu. Dar düzeltme native nesneleri kullanım aşamasında oluşturur; küçük rafın üst yüzeyindeki erzak konumu da düzeltildi. Bu başarısız kanıt korunur.

Son r5 Unity6000.0.73f1 Windows derlemesi **Succeeded,0 hata,0 uyarı**. Aynı yeni DLL üzerinde **95/95 ilgili kontrol,0 runtime hata,59.43sn**: gerçek controller/ışın, üst oda kabulü/yanlış bodrum reddi, gerçek konumda deadline, battaniye işlemi ve dünya nesnesinin kaybolması, elektrik/emisyon, komşu hareket/reset, doğal16sn intro/saat, defter ve köşe rehberi,14kaynak/16erzak, RNG/partial stok/skor kuralları, araç/ekip/boat rig ve26render kaydı. Bunlar API ve açık kurulum fikstürleridir;6konum yerleştirmesi,3saat fikstürü ve bağımsız model seed/wave/durum fikstürleri vardır.16sn açılış gerçek zamanlı gözlendi.1600x900 gameplay/defter/sel ve960x540 sonuç görüntüsü alındı; her HUD'nin her çözünürlükte fiziksel OS testi yapılmadı.

DLL SHA256: `D4BEAD82997A0E6AE2318A81D0D4A2E33BC2FD71EC93758195DA70558CEAD82D`. Dondurulmuş138C#/shader dosyasıyla kaynak farkı0. Profil `native-r5-7fa017a5-b669-459a-94b5-545070a1aa82`; rapor `Evidence/GamejamOrchestra/TLTP-V5-20261018/manager/.../v5-smoke.json`. Gerçek skor dosyasının önce/sonra hash'i aynı.

R4 ayrıca94/94 geçti; onun yakın çekim AI görsel incelemesi alt yüzün önünde küçük dikdörtgen sakal parçasını gösterdi. R5 bu parçayı kaldırıp yalnız SurvivorV2'yi yeniden bake etti; ağız önü yeni karede temizdir. Personel çekimleri araç tarafı yerine kaldırım tarafından alındı. Meslek renk/aksesuarları ve araç türleri görülür; kurtarma personelinin tek karesi park etmiş araçla kısmen örtülür, bütün açılarda insan/animasyon kabulü iddia edilmez. Küçük boyun/yaka ve kıyafet birleşim sertlikleri görsel sınır olarak kalır; hareket/controller bunları saklamak için değiştirilmedi.

R2 otomatik test başlangıç rehberinin aynı-kare korumasını yanlış sırada çağırdığı için INVALID_TEST'dir. R3'te kamera yumuşaması/rehber geçişi tamamlanmadan doğrulama ve son ekip ulaşmadan erken sayım FAIL kayıtları korundu; hedefe gerçek ışın hâlâ zorunludur, tolerans/fizik/maliyet gevşetilmedi. R4, kamera yerleşmesini ve tüm ekibin gerçek varışını bekler; ikinci bayrak veya sahte kabul kullanmaz. Görsel incelemede üst sel odası kamerası karakterin başını kesiyordu; kadraj genişletildi. Defter kapağı ortak alpha'ya bağlandı; sonuç kısayol yardımındaki kullanılamayan tuş iddiaları çıkarıldı.

Kaynak incelemesi, import, derleme, native API/gerçek fizik/render, OS girişi ve insan kabulü ayrı katmanlardır. Bu sonuç fiziksel OS girişi, insanın ses/animasyon/keyif onayı veya tüm kampanya kabulü değildir. Bu çalışma eski test sayılarını yeni DLL için kullanmaz. Gerçek kayıtlar izole test profilleriyle korunur; önceki reddedilmiş build temizliği tekrarlanmaz. Başlangıçta gözlenen V4.14 kullanıcı sürecine root girdi veya sonlandırma göndermedi; son gözlemde o PID artık bulunmadı. Root yalnız kendi hatalı Unity ve izole QA süreçlerini durdurdu. Yeni ödeme veya GitHub yayını yapılmadı.
