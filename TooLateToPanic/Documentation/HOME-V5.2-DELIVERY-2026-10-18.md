# Home V5.2 —18Ekim2026

Güncel **Home V5.2-r1 /0.5.2**. Dalgalar ilerledikçe normal kampanyada temel hasar ve afet şiddeti artar.1.dalga ×1.00,5.dalga ×1.32,10.dalga ×1.72,20.dalga ×2.17,50.dalga ×3.52. Önlem/ankraj/yükseltme indirimleri ve75% kayıp sınırı korunur. Aynı ev/afet için temel risk varyasyonu sabittir; küçük tesisat olayları sonlu RNG kalır. Toplam hasar100'de doyar.

Deprem ev/komşu sarsıntısı, yağmur/sel suyu, ateş/duman ve afet sesi aynı modelden artar. Görsel ölçek1..1.65ile sınırlıdır; parçacık bütçeleri korunur. Simülasyon/öğretici/kısa akış başlangıç şiddetindedir.120+45sn, otomatik görünmez çanta, F1klavye/F2menü/F3ödülsüz kayıpsız sinematik geçişi korunur.

## Uygulama

Tek kaynak PanicWaveDifficultyV52 ve modelin DisasterSeverityV52/DisasterPresentationScaleV52 özellikleridir. Eski+0.008/dalga ve×1.25tavanı kaldırıldı.10.dalgaya kadar mutlak+0.08, sonrasında+0.045hasar çarpanı artar; bileşik yüzde değildir. İlk dalganın eski item/afet varyasyon tuzu korunmuştur. Aynı ev, aynı afet ve aynı önlem için temel hasar100'e kadar artar. Önlemler ve yükseltmeler çarpan sonrasında aynı oranda etkilidir. Yeni RNG çekimi temel artışı maskelemez; kaynak RNG'si ve0..2.8puanlık memoize küçük tesisat olayı korunur. Bu küçük olay kaybolursa toplamda hafif iniş olabilir; farklı afetlerin toplamları da doğrudan eşit karşılaştırma değildir.

Tüm afetlerde100üstü katkılar orantılı ölçeklenir. Eski ilk eşyanın katkısından bütün fazlalığı çıkarma hatası böylece giderildi; katkıların toplamı gerçek hasarla eşleşir, hiçbir eşya negatif olmaz. Kaybın75sınırı, stok tüketimi, gerçek sığınak ve F3korumasının tek dalga/ödülsüz/skor dışı kapsamı değişmez.

Görsel/afet sesi modeli:1+0.65×(1−exp(−0.8×(hasarÇarpanı−1))). Mevcut ev ve yedi komşu aynı görsel ölçekle sarsılır; kamera darbesi, parçacık emisyon/boyut, sel yüksekliği/dalgası, ateş alev/dumanı ve afet ambiyansı kontrollü artar. Collider/controller ve küresel zaman değişmez; yeni mesh/partikül kapasitesi/ses kaydı/sağlayıcı yoktur. Üç argümanlı eski art çağrısı varsayılan1kalır.

## Kanıt

147/147 ilgili native model/API/gerçek sahne/render kontrolü, 0 runtime hata, 48.83sn. Açık dalga/afet/ev/erzak/RNG fixture'ları kullanıldı; OS girdisi, insan oynanış/işitme veya tam kampanya kabulü değildir.

Native rapor `C:\Users\kaant\Desktop\civilgamejam\TooLateToPanic\Evidence\GamejamOrchestra\TLTP-V52-20261018\manager\native-r1-f7ae8cda-7a4c-417b-b5f5-6018c9e900a4/v52-smoke.json`. Modelde dört afet ve1/2/5/10/20/50/intMaxdalga fixture'ları, tüm/tek eşya katkıları, erişilebilir önlemler, gerçek kayıp/başarı ve F3değerlendirildi. Native sahnede deprem/sel/yangın1ve20.dalga kareleri ve gerçek effect/AudioSource değerleri karşılaştırıldı.20.dalga depremde8+4sdoğal sinematik koruması izlendi; diğer görsel vakalar aynı guardedskipyoluyla çözüldü. Storm normal havuz kabulü değil, ayrı simülasyon temel şiddet kontrolüdür. Erken/geç görsel farklar görüntü/APIkanıtıdır; insan denge/konfor/işitme onayı bekler.

DLL `10657090B2000EACC9F379F49612847E2C40B846B638A6F82C79E3531813066B`. ZIP `CB4DC0D12D4B03912EE3459FDC08F798E0DB5EAE91CE61531AD7FBDB9AE31954`, 219393865bayt; portableEXE/DLLbyteeşleşmesi doğrulandı. Dondurulan148C#/shader,0kayma; tam kaynak40902dosya, build156dosya. Kesin yollar/hashler [LATEST](../LATEST-BUILD.json) ve manager manifestlerindedir.

Gerçek skor gözlemi önce `62D53735099315B02109CEAFBCC4836B6E6A33D4620C02F5E8395286A059B55C`, sonra `62D53735099315B02109CEAFBCC4836B6E6A33D4620C02F5E8395286A059B55C`. QAyalnız taze izoleScorePathOverridehedefine yazdı; açık kullanıcı oyunu bağımsızdır. Çalışan kullanıcı player'ına girdi veya sonlandırma gönderilmedi. Yeni build ayrı klasördedir. Bağımsız FPS projesi, kaynak/lisanslar, eski reddedilmiş temizlik hedefleri korunur. Yeni GitHub yayını/ücret/kredi yoktur. İki worker yalnız atanan staging'e yazdı; roottekUnity/build/player sahibiydi.

Son doğrulama:148kod/shader ve156build dosyasında0kayma. Önceki V5.1-r2 PID50736hâlâ açık; eski DLLhashi aynı ve build/ZIPkorundu. Yeni normal pencere açılmadı; grafik izole native başlatma doğrulandı. Başlatıcı V5.2-r1yolunu açar. İncelenen yerel kareler framebufferdır; kalıtılan genel receipt metninin kapsam düzeltmesi manager/EVIDENCE-SCOPE.md içindedir.
