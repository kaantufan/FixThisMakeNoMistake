> **Windows V4.11:** [Oynanabilir ZIP / Release](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.11). Kaynak checkout'unda `Builds` bulunmaz; hazır oyun için ZIP'i çıkarıp EXE'yi yan klasörleriyle çalıştırın. Unity kaynak projesi `TooLateToPanic/`, Unity6000.0.73f1 / Built-In.

# Too Late to Panic!

Home V4.11: afet çantası hazırlığında FPS, diğer önlemlerde izometrik oynanış. HAZIRLIĞA BAŞLA kısa akışı, SERBEST OYUN normal kampanyayı açar. Kamera mevcut oyuncu konumundan sinematikle geçer; G aynı konumda izometrik görünüme döndürür.

Dolaplar inşaat malzemelerini bir kez doğrudan kaynak havuzuna verir. Su, gıda, hazır ilk yardım, fener, pil, radyo ve battaniye evin içinde fiziksel eşyalardır. Envanter/çanta sürükle-bırak, birleştirme ve kısmi yığın doldurma kullanır. Bandaj/antiseptik üretimi kaldırıldı. Fiziksel çanta evde, sığınak yalnız bodrumda; garaj iç kapısı, tam boy duvar fiziği, dört yönlü kesit, bodrum ışığı ve şömine güncellendi.

Yarkın dalındaki eşdeğer31 model/14klip kaynakları alındı; yeni el/kol ve gerçek oyun verisiyle çalışan altı sayfalı defter eklendi. [Asset/lisanslar](Documentation/ASSET-CREDITS.md), [eksik/eşdeğer listesi](Documentation/MISSING-ASSETS-V411.md).

`Play-TooLateToPanic.cmd` güncel Windows build'ini açar. [Taşınabilir ZIP](Builds/TooLateToPanic-Home-V4.11-20261017-portable.zip) içindeki EXE'yi yan klasörleriyle birlikte tutun. [Build kimliği](LATEST-BUILD.json), [teslim ve kabul sınırları](Documentation/HOME-V4.11-DELIVERY-2026-10-17.md).

| Tuş | İşlem |
| --- | --- |
| WASD/ok, Shift, Space | Yürü, koş, zıpla |
| FPS'de E/F | Baktığın erzagı al; E kap/kapı açar |
| G | Çanta hazırlığını bitir, aynı yerde izometrik önlemlere dön |
| Tab/B, fare | Envanter/çanta aç; sürükle-bırak ile taşı/birleştir/geri al |
| Enter, Backspace | Çantaya koy; geri/iptal |
| J | Canlı hazırlık defterini aç/kapat; sayfa ayraçları ve oklarla gezin |
| Sol tık | Sabitlemede iki vidayı sırayla tak |
| Sağ tuş sürükle, kısa sağ tık | İzometrik kamera döndür; eşya taşı |
| Esc, H, F2 | Duraklat/geri, yardım, geliştirici |

Esc duraklatması ve açık defter hazırlık/afet/kısa akış saatini dondurur. Eski maliyet, iptal, kayıp ve skor kuralları korunur. Geliştirici menüsünü açmak hile değildir; seçenek kullanımı koşuyu kalıcı skor dışı yapar. Kısa akış270+30sn hedefini korur; yeni insan süre/keyif kabulü yapılmadı.

Son adayda 156/156 izole native API/render kontrolü,0runtime hata. Test10açık konum fikstürü kullanır; OS tuş/fare, insan oynanış/dinleme ve tüm kampanya kabulü değildir. Önceki sürümlerin testleri yeni DLL kanıtı sayılmaz. Ayrı eski FPS checkout'u ve kayıtları korunur; bu teslim ortak oyun içindeki çanta aşamasını uygular.
