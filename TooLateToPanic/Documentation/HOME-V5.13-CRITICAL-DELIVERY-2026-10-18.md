# Home V5.13-r6 — kritik önlemler ve yapı güçlendirme kaybı

18 Ekim 2026. Son kullanıcı isteğiyle sağ alttaki yinelenen eylem düğmesi kaldırıldı; yeni küçük nesne tuş balonu aynen korunur. Normal oyun başlatma otomatik tam ekrana geçer. Büyük etkileşim istemi, nesnenin üzerinde küçük yuvarlatılmış ve gerçek eylem tuşunu taşıyan balonla değiştirildi. Bu son kullanıcı isteği önceki büyük kart sunumunu günceller; görev, fizik, gerçek girdi ve oyun saati değişmez.

Üst fiziksel panik odası mevcut bank/yelek/erzak/erişim kapısını koruyarak çelik kapı kaplaması, bağlantılı koruma kuşağı, ilk yardım bölmesi, acil aydınlatma gövdesi, mevcut radyo ve oda levhasıyla belirginleştirildi. Yeni collider/pickup veya sahte sığınak kabulü yoktur.

Masa, sandalye, koltuk (mevcut renkleri), çalışma masası ve yetişkin/çocuk yataklarına gerçek iki vidalı sabitleme eklendi. Kullanıcının açık düzeltmesiyle hasar bağışıklığı eklenmedi; gerçek temel eşya riskleri korunur. Bu sürümde aşağıdaki kritik görev hasarı ayrıca gerçek modele eklenir. Asıl malzeme ve bütçe tamamlamada bir kez alınır; iptal ücretsizdir. Genel kelime yakalamayla çıkan yanlış malzeme popup'ı kaldırıldı: gerçek seçili işin eksikleri gösterilir, hedef değiştiğinde veya eksikler çözüldüğünde eski uyarı temizlenir.

Açılış, deprem, afet sonrası ve sonuç kameralarında farklı kadrajlar ve plan içi kısa hareket vardır; planlar arası geçiş kesmeyle yapılır. Mevcut16s açılış, afet/sonuç süreleri, skip, gerçek sığınak kabulü ve model saati korunur. Dış siviller gerçek afet ve kısa akış afet aşamalarında gizlenir. Önceki dalganın kurtarma kökleri/helikopteri sonraki hazırlık/afette kapanır ve rotor durur. Ekipler gerçek dalga sonucundan sonra gelir; aktif fırtına görsel/sesi önce söner, selin çekilmesi korunur.

Önceki V5.11 yerel çatı işi bu pakete dahildir: üst kat duvarları ve kurulu çatı tek güçlendirme; çatı ayrı seçilmez. Zemin/bodrum yalnız kendi duvarlarını güçlendirir. Toplam gerçek tarif, atomik harcama, ücretsiz iptal ve değişen üye/seviyede harcamasız ret korunur. V5.11 ayrıca GitHub'a yayımlanmadı. V5.12-r2 Editor/native/render doğrulanmış ara kaynak/adaydır; paketlenmedi, LATEST yapılmadı ve yayımlanmadı. Bu final sürüm onun panik odası, sabitleme, sinematik ve afet sonrası yaşam döngüsü değişikliklerini içerir. Aşağıdaki kontroller yalnız yeni V5.13 DLL'ye aittir.

Afete özel kritik önlemler gerçek ankraj, ana şalter, gaz vanası ve üst kat/çatı güçlendirmesi durumundan okunur. Eksik afet görevi gerçek ek hasara katkı verir; çoklu görevler ortak sınırla ve tüm katkıları koruyan son hasar ölçeğiyle çözülür. Su, gıda ve gerçek fiziksel sığınak koşulları ayrıca korunur. Görev tamamlanma durumu, dalga sonucu ve erzak tüketiminden önce yakalanır; sunum kendi başına kabul veya hasar üretmez.

Gerçek afet sonunda hasar almış yapı grubunun RNG ile bir kademe güçlendirme kaybetme ihtimali vardır. Üst kat duvarları ve kurulu çatı tek grup olarak düşer; her kurulu T1/T2 üye en fazla bir kademe iner, T0 korunur. Aynı dalga/koşu tekrarında veya UI okumasında yeniden zar atılmaz. Mevcut dalganın çözümlenmiş hasarı değiştirilmez; düşen seviye sonraki dalgayı etkiler. Öğretici/simülasyon/kısa akış ve F3/kaybetmeme koruması bu düşüşü tetiklemez.

Yakın hedefte gerçek eylem tuşu gösterilir. Yeni dalganın duyurusu gerçek Model.Wave artışından sonra seçilmiş afet adlarını kullanır ve mevcut dalga kartına yerleşir; afet seçimini veya oyun saatini yönetmez.

90+40 sn, 1500 bütçe, %75 kayıp, gerçek maliyet/iptal/fizik/controller, sonlu stok ve skor kuralları korunur. Yeni dış model/ses/shader/sağlayıcı/kredi/ödeme yoktur. Prosedürel oda görseli, kamera ve AI kod/metin kullanımı ASSET-CREDITS içinde kayıtlıdır.

Gerçek Unity6000.0.73f1 Editor **810/810 PASS**, build hatası0/uyarı0. Bu DLL için ayrı GUID profilde **405/405 native kontrol PASS**, **12 incelenmiş framebuffer**, runtime hata0. Aynı DLL raporunda kayıtlı kritik görev/gerçek hasar, grup RNG/kademe kaybı, eylem tuşları ve dalga duyurusu kontrolleri çalıştırıldı; tam kontrol satırları rapor içindedir. Önceki çatı/duvar ve sunum özellikleri yalnız bu rapordaki ilgili regresyon satırları kadar doğrulanmıştır. Native sahnede farklı kamera planları, gerçek oda dekoru/kapı, dış NPC ve eski response köklerinin afet/sonraki dalgada kapanması ve rotor/fırtına durumları kontrol edildi. Model/phase/age/aktör/kamera ve mevcut geliştirici kaybetmeme fikstürleri açıktır; API girdisi doğal rota veya OS girdisi değildir. Üst başarı kamera örneği varsa gerçek fiziksel girişe bağlıdır, kazanılmış dalga sonucu sayılmaz; fiziksel giriş reddedilirse bu ek kamera örnekleri NOT_RUN kalır. Tüm kaynak paketi manifesti ve ZIP'in167dosyası byte/hash eşleşir; kaynak kayması0.

Fiziksel OS girdisi, insan oynanış/okunurluk/dinleme/konfor ve tüm kampanya kabulü NOT_RUN. Açık eski kullanıcı player'ına girdi/sonlandırma gönderilmedi. Bağımsız FPS projesi ve kayıtları korunur. Önceki V5.11 ve V5.12 test sayıları yeni DLL'e taşınmaz. Yarışma formu gönderilmedi.

| Kimlik | SHA256 |
|---|---|
| DLL | `C81E6105417E5D140ABE2E4D1FC60CD082AEB897E6F81E91CF004362B502136D` |
| ZIP | `E0537B8C867A5698EDC1DAC6AB1FAF17058905D020FACB733096552CCC752B3A` |
| Kaynak manifesti | `0F38F2E5AE90B14B295C2711AD59E7CFD6AFBDF6D4E9712F4E69AD7BAD1519FC` |
| Build manifesti | `BC175356E79D4B5560A95E937FE58C57180C324B27D7C40E443277A8C65075A6` |

[Başlatıcı](../Play-TooLateToPanic.cmd) · [Portable ZIP](../Builds/TooLateToPanic-Home-V5.13-r6-20261018-portable.zip) · [Güncel indeks](../LATEST-BUILD.json)

Aynı final DLL üzerinde ayrıca **291/291 sunum/regresyon kontrolü PASS**, **12 ek özgün kare incelendi**. Bu ikinci tur dört açılış planını, iki deprem planını, gerçek afet/sonrası aktör ve ses kapılarını, masa sabitlemesini ve fiziksel üst oda girişi/iki başarı kadrajını içerir. İki native tur toplamı696 çalıştırılmış kontrol satırıdır; ortak model regresyonları iki turda da bulunur, benzersiz test sayısı iddiası değildir.

Normal otomatik tam ekran açılışı ayrıca aynı DLL üzerinde **10/10 gözlem kontrolü**, **1 root tarafından incelenmiş kare** ile bağlandı. Bu normal başlangıç gözlemidir; OS girdisi veya insan kabulü değildir. R4 ayrı native adayının başarısız popup zamanlama kontrolleri korunur; paket/LATEST/yayın yapılmadı. R6 kaynak/native kimliği ayrıdır; eski sayılar aktarılmaz. Önceki r3 ve r5 yalnız yerel kabul edilmiş paketler ve push yapılmamış hazırlanmış indexler idi; yayımlanmadı, eski public yayın V5.10-r2 olarak kalır.
