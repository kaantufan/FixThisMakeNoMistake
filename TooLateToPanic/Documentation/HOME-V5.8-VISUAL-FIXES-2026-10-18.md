# Home V5.8 — garaj, NPC ve sel düzeltmeleri

Üç doğrudan kullanıcı düzeltmesi kaynaklara ve Windows aday derlemesine uygulandı.

- Garaj yolunun eski sığınak etrafındaki beş parçalı zikzağı kaldırıldı. Yol, garaj kapısı ve kaldırım açıklığıyla aynı x=11.7 ekseninde 3.2 m genişliğinde düzdür. Gerçek HomeArt/landscape cache'i yeniden bake edildi; kapı/fizik/controller değişmedi.
- Komşu NPC'lere ters kemik ekseninde eklenen sarı/kırmızı/beyaz plaka, çubuk ve ekipman blokları kaldırıldı. Dört kafa ölçülerine oturan ayrı saç silueti var: yandan ayrılmış kısa, dalgalı, kırçıllı kısa ve geriye taralı küçük topuz. Ana karakter prefabı, yüz/gözler, humanoid Avatar/UAL ve mevcut hareket/poz sahipliği korunur. Geçici el telsizi aynı poz/etkinlik yolunda küçük ve doğru eksende kalır.
- Selin tek yönlü 46 m şeridi yerine evin dört yanı, garaj önü, sokak ve komşuları kapsayan 84 m sınır içinde kademeli taşma alanı kullanılır. Hasar tespiti ve sonraki dalga geçişinde yüzey kesilmeden çekilir; kuyruk en geç 12 saniyede temizlenir. Ev/garajın kesit içi ve üst panik odası kaplanmaz. Diğer afetler, hasar/RNG, sığınak, süre, bütçe ve skor değişmedi.

## Gerçek kontroller

Unity 6000.0.73f1 `-batchmode -nographics` ile hedefli landscape bake ve Windows build: Succeeded, 0 hata, 0 uyarı. Yeni kaynak 192 C#/shader dosyasıyla donduruldu; derleme sonrası kayma 0.

Gerçek Unity Editor nesne/mesh/Avatar verilerinde **68/68** kontrol geçti. Cache'lenmiş beton yolun 18 yüzey noktası düz yolu doğruladı. Dört NPC'nin bağımsız kafa/siluet mesh'leri, özgün kemik TRS/Avatar/controller/klipleri ve göz/orta yüz önündeki boşluk korundu. Selde gerçek üçgenlerden ölçülen 16 dış bölgenin 0/2/8 saniyedeki suya çıkışı 0/6/16'dır; çekilmenin ilk karesinde sıçrama yoktur, 12 saniye sonunda yüzey toprak altındadır. Bu kontroller görüntü veya insan okunurluk/beğeni kabulü değildir.

Grafik player, Windows girdisi, canlı oynanış/görüntü ve dinleme bu aday için **NOT_RUN**. Önceki fiziksel Esc durdurmasına uyuldu; eski açık oyunlara girdi/kapatma gönderilmedi. V5.7-r4'ün 981 native sonucu bu yeni DLL'ye taşınmaz.

## Kimlik ve tek sürüm entegrasyonu

Yeni DLL SHA256: `D215C249D0BC55624D65295E61A2AB6698BF8BF8AB47AABA20AD6B1ADBD673FC`.

Kesin kaynak/build kimlikleri ve kontroller `Evidence/GamejamOrchestra/TLTP-GARAGE-NPC-FLOOD-V58-20261018/manager/FINAL-RECEIPT.json` içindedir. Yeni C#/shader snapshot ve değişen landscape cache/üretilmiş sanat kopyaları aynı manager altında korunur.

Kullanıcı diğer sohbette tek sürüm için koordinasyonu açıkça onayladı. Bu ara adayın üç düzeltmesi `Güncelle menü, bütçe ve sesleri` sohbetindeki V5.9 menü/ekonomi/ses değişikliklerine aktarılır. V5.8 grafik olarak doğrulanmış güncel sürüm diye tanıtılmaz; mevcut V5.7 launcher/LATEST korunur. Tek taşınabilir ZIP ve sonraki güncel indeks, V5.9 son adayının kendi doğrulamasıyla hazırlanır. Ek sağlayıcı/ödeme/kredi veya harici asset yoktur; var olan lisanslar korunur.
