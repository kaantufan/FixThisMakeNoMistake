# Home V5.6 ses ve sunum kaynakları

Önceki V5/V5.5 kaynakları, lisansları ve kayıtları korunur. V5.6 için yeni ücretli sağlayıcı, kredi, indirme veya konuşma üretimi yapılmadı.

`Assets/Game/Resources/Audio/Foley/v56_*.wav` altında19 özgün yerel PCM klibi vardır: montaj darbesi; gaz/su/elektrik için başlatma, hareket, tamamlama ve iptal; gaz alevlenmesi, boru sızıntısı, kısa devre; giysi; Zodiac motor ve su döngüleri. Üretici Python standart kitaplığıyla deterministik sentezdir; kaynak kayıt veya başka eser örneklenmez. Generator, seed, her klibin SHA256/duration/RMS/peak/seam ölçümü `Evidence/GamejamOrchestra/TLTP-V56-20261018/staging/audio/` altında korunur.19 klip monoPCM16/22050Hz; kaynakta tam ölçek clipping0, en yüksekpeak0.519989. Bu kaynak ölçümü tek başına oyun miksinin veya insan dinlemesinin kabulü değildir.

NPC ayakları mevcut gerçek yüzey adım kliplerini, giysisi yeni özgün klibi kullanır. Seated ekip için yürüyüş uydurulmaz. Hareket, ses kaynağı ve collider sahipliği ayrıdır. Mevcut deprem/yangın/sel/fırtına, helikopter, araç ve FPS gerilim kliplerinin provenance bilgileri önceki asset belgelerindedir.

Çatı çerçevesi özgün prosedürel ahşap kirişler, bağlantı plakaları ve cıvatalardan oluşur; mevcut Built-In Standard/unlit materyal sistemini kullanır. Yeni renderer pipeline, paket veya font eklenmez. Lintel gizleme yalnız render bileşenine uygulanır; fiziksel karşılığı korunur. Aydınlatma mevcut lambaları ve güneşi ayarlar; elektrik kesme sahibi değişmez.

F1 açıklamalı rehber, seçili hedef kartı, dalga hesap dökümü ve ses ayarları mevcut HUD çizimi/fontlarıyla üretilir. Gerçek maliyet, stok ve sonuç snapshot'larını okur. Ayrı bir UI framework veya dış görsel asset eklenmez.

Yeni assetlerin Unity import/native/Windows çıktı kanıtı final V5.6 teslim raporunda ve `LATEST-BUILD.json` içinde aynı DLL ile bağlanır. Kaynak ölçümü, gerçek ses çıkışı ve insan işitme/keyif kabulü ayrı katmanlardır.
