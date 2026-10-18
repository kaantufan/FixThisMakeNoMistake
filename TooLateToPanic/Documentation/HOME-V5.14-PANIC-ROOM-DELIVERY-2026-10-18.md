# Home V5.14-r3 — Panik odasını kilitle duvar butonu ve alarm

Üst panik odasının duvarındaki büyük "Panik odasını kilitle" butonu ve üstünde yanıp sönen kırmızı alarm gerçek E etkileşimini belirginleştirir. Gerçek girişin tek kaynağı Model.ReachedShelterV5 ve fiziksel kapısı TryEnterActiveShelterV5 olarak kalır. Buton ve fiziksel marker gerçek duvar konumuna taşınır; grounded/kat/ışın/2.4m controller kabul koşulları korunur. Başarılı E sonrası mevcut afet aşamasına geçiş davranışı sürer; oyuncuya görünen eylem yalnız "Panik odasını kilitle"dir. 90+40 sn, 1500 bütçe, %75 kayıp, gerçek maliyet/iptal/controller/stok/RNG/skor kuralları korunur. Küçük nesne tuş balonu, otomatik tam ekran, ortak kompakt rehber, gerçek kritik görev/ek hasar ve diğer V5.13-r7 özellikleri korunur.

Yeni Editor kontrolü NOT_RUN; 122 aynı DLL native API/render kontrolü, 15 incelenmiş kare. OS girdisi/insan oynanış, okunurluk, dinleme, konfor ve tam kampanya kabulü NOT_RUN. Dar smoke'un kendi raporunda belirtilen konum/afet/durum fikstürleri ve API eylemleri doğal fiziksel rota veya insan kabulü değildir. Önceki geniş V5.13-r7 native/sunum/tam ekran sayıları bu DLL'ye aktarılmaz.

Build hatası 0, uyarı 0; bu smoke'ta runtime hata 0; kaynak kayması 0. Kaynak freeze, tam kaynak manifesti, DLL ve rapor kimlikleri LATEST içindeki sourceVersionBoundaries alanında ayrılır.

| Kimlik | SHA256 |
|---|---|
| DLL | `EE4581BCADE7FF44E3ACDA394391E561536761ACFAF0A365F4BEA5B6AA19A69D` |
| ZIP | `E97C68B5BB9989776889BEFE473981C9C1B4A9EF2BE724CB8F85EA3B768EF6F7` |
| Kaynak manifesti | `DB3AFBB9BFEFF1EE03A4FA17C56B1D62DE8E3DA46AFFF83323FE91CCE641170B` |
| Build manifesti | `AF4379890173E2D051BA7CF8DCD317D83189357D77DB66050718D0BA6AD94DEF` |

[Başlatıcı](../Play-TooLateToPanic.cmd) · [Güncel ZIP](../Builds/TooLateToPanic_FixThisMakeNoMistake.zip) · [LATEST](../LATEST-BUILD.json) · [AI/asset/lisans](ASSET-CREDITS.md).

Önceki public v0.5.13-r7, kaynak `a0b531679e0b292f74ba227fda53b19fecb10c08` ve release/tag/asset kimlikleri, stable v0.4.13, açık eski oyuncular, kayıtlar, bağımsız FPS ve eşzamanlı yerel video korunur. Root tarafından tamamlanan eski build temizliği LATEST maintenance kaydı ve cleanup makbuzuyla bağlıdır; küçük metadata kayıtları ve immutable yayın kimlikleri korunur. [GitHub ön sürümü](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.5.14-r3) · [Windows ZIP](https://github.com/kaantufan/FixThisMakeNoMistake/releases/download/v0.5.14-r3/TooLateToPanic-Home-V5.14-r3-20261018-portable.zip).

OpenAI Codex ile üst panik odasının duvarındaki büyük "Panik odasını kilitle" butonu, üstünde yanıp sönen kırmızı alarm, gerçek tuş istemi, ilgili kod, Türkçe metin ve kontroller düzenlendi. Özgün derinlik testli alfa font shader ve Resources/HUD materyali ile beyaz yazı kontrastı düzeltildi. Yeni dış görsel/model/shader/ses kaydı, sağlayıcı kredisi veya ödeme yoktur. Mevcut asset/lisans/provenans kayıtları korunur. Yarışma sistemine gönderim yoktur.

[v0.5.14-r3](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.5.14-r3) ve [doğrulanmış ZIP](https://github.com/kaantufan/FixThisMakeNoMistake/releases/download/v0.5.14-r3/TooLateToPanic-Home-V5.14-r3-20261018-portable.zip) yayımlandı. Kabul edilmiş kaynak commit’i `c4e003f74205e0efb05a7fb3eb71fb2d7a97a523`; GitHub digest/boyutu yerel ZIP ile eşleşir. Belge güncellemeleri yeni native test değildir. Önceki release/tag/asset ve stable v0.4.13 korunur.
