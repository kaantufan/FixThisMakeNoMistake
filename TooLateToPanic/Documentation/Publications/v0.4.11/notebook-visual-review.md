# Notebook — native-v5 son dar inceleme

17 Ekim 2026. **R2 hedef kusurları incelenen gerçek native karelerde kapandı.** Yeni üretim değişikliği gerektiren P1/P2 görsel engel görülmedi. Bu worker yalnız mevcut kaynak/receipt/PNG okudu ve bu dosyayı yazdı; Unity/player/pencere/OS girişi veya üretim kaynağı değiştirmedi.

Kanıt kökü: `manager/native-v5-146e6e2f-a4bd-4852-ae34-965b5538d5e7/`. `revision-smoke.json` tamamlandıktan sonra 11 defter PNG'sinin tamamı görüntülendi. Static altı sayfa, rooms/inventory ana kareleri, iki gerçek dönüş örneği ve 960×540 dahil.

## Kimlik ve native kayıt sonucu

- Unity **6000.0.73f1**, Built-In Windows; `Home-V4.11-r5-20261017` DLL SHA256 **45BD98BB645140A00A65D15742B1A048F5C3BDB607EAD776275F1D7580D7E08B**. Build dosyası SHA256 bağımsız okundu ve `build-identity.json` ile eşleşti.
- Canlı `PanicNotebookOverlay.cs` SHA256 **EF9FAE4636C9B2C9B065B5EE53C44999B31CA4074CB84994F14EDC951504917D**; teslim edilmiş R2 delta ile aynı.
- `revision-smoke.json`: **complete=true, passed=true; 153/153 satır PASS, runtime errors=0; 33,897 sn**. Bu toplam bütün V4.11 değişiklik paketidir; hepsi notebook testi değildir.
- Notebook alt kümesi **32/32 PASS**: public API açma/kapanma/aynı-kare input latch, model clock gate, canlı bütçe/malzeme/kapasiteler, veri/skor/cheat değişmezliği, altı sayfa settle, gerçek dönüş içi fixture/ters pose/sonuç, latest queue, close-reopen ve altı static render settle kontrolü.
- Bu çalıştırmada notebook helper normal serbest oyun/FPS çanta aşamasındadır (`bagProgress=1`). Notebook'a özel short-flow/Jury clock row'u çağrılmamıştır. Ayrı pause.shortflow satırını notebook-Jury native kabulü gibi sunma.
- Receipt SHA256 `424A87820776C7B9872BE5FD70568F326058EAC4F3CAF4631D3239DC8FE05F61`; build identity SHA256 `0191F096C331A4C263C72BACE17AAC200D9D631D2C84D81B79B565F852082850`. `player.log` ve telemetry'de hata görülmedi.

## Görsel kapanış

**Çizgi/matrix kusuru: kapandı, bu kare kapsamıyla.** Rooms haritasındaki ev dış çerçevesinin dört tarafı görülüyor. Ev/sel/deprem/yangın/fırtına/çanta ikonlarının geometrisi tekrar anlamlı ve kendi alanında. Önceki statik ve turn karelerindeki kağıt dışına kaçan rastgele ink parçaları görülmüyor. Raster çizimde hafif basamaklı/noktalı ince kenarlar kalabilir; bu karelerde kullanılabilirliği durduran bir kusur değildir.

**Başlık/footer/close kontrastı ve satır kesilmesi: kapandı.** `HAZIRLIK DEFTERİ`, alt yön/Tab talimatı ve `KAPAT · J / ESC` koyu arka planda açık renkle okunuyor. Kapat metni ana ve 960×540 karelerde tek satır; ESC kesilmiyor. Kağıt içindeki ana metin, sayım ve envanter düzeni taşmıyor. 960×540 ikincil metin küçüktür; insan okunabilirlik kabulü yapılmış sayılmaz.

**Altı sabit sayfa ve eski callback yarışı: kapandı.** Her `notebook-page-*` görüntüsünde doğru başlık, dolu içerik, sayfa numarası ve eşleşen sarı ayraç var. Receipt her biri için `turning=False`, `progress=0` kaydetmiş. V4'te bir sonraki sayfanın boş p0 karesini yakalayan dosyalar yeni static kabulün yerine kullanılmıyor.

**Dönüş örnekleri: dar görsel PASS.** Quarter frame **710**, progress **0,2573563**; middle frame **721**, progress **0,5051061**, ikisi de `turning=True`. Eski Rooms sayfasının kıvrılan arka yüzü ve altında Sel sayfası görülüyor; bağımsız çizgi/text parçaları kağıt dışına kaçmıyor. Quarter'da yükselen kağıdın üst ekran sınırını aşması dönüş geometrisiyle tutarlı; alttaki checklist ve sağ durum sayfası korunuyor. Bu iki örnek bütün zaman aralığının kesintisiz video kabulü değildir. Ters dönüşün tamamlanması ayrıca native API/state row'larında PASS; ters dönüş için ayrı görsel dizi yok.

## Gerçek PNG / frame / durum / SHA256

Boyutlar PNG IHDR'den okundu ve receipt actual framebuffer değerleriyle eşleşti. V4'ün 1078×904 sınırı bu R5 karelerine taşınmaz.

| PNG | Boyut | Frame / durum | SHA256 |
|---|---:|---|---|
| notebook-rooms.png | 1600×900 | 698, Rooms settled | 0A2F5C14DA0DCBF97C1F704D70AB8BF42B7197F03E6876923FA5307355F94831 |
| notebook-turn-quarter.png | 1600×900 | 710, turning p=0,2573563 | 9DCC05F774DB8D4B079D18BBEBC0DF40348685BB896C815AE146135FFAC4862B |
| notebook-turn-middle.png | 1600×900 | 721, turning p=0,5051061 | 9EDD2A048CA8CB1247B14AE856E354273A396B08F56A66369445ED6999128D40 |
| notebook-inventory.png | 1600×900 | 785, Inventory settled | B8DFF024FACD6F34F22ACB6D0E4B30828B724EC61253DF6AC6A2C7D5D7609601 |
| notebook-page-Rooms.png | 1600×900 | 1337, Rooms settled | 6CAEE6E50436780235E00D951452ECB64DCAA21ADEE6E00E5EE6C2D1D184BB51 |
| notebook-page-Flood.png | 1600×900 | 1383, Flood settled | 879BDFC02420C0BBF94D13B260A1ECB2D7AC475D1112D50D4BFE3D6CF61D4E47 |
| notebook-page-Earthquake.png | 1600×900 | 1426, Earthquake settled | 09B1BD783A78DCEA93E57B117218281239A41223FFBFF1127D0DB26E3AB16465 |
| notebook-page-Fire.png | 1600×900 | 1470, Fire settled | 0998F3930A6CDABDFE0ED77E598614D0E2879337D2BA3CA46143DB8EDFB68ABB |
| notebook-page-Storm.png | 1600×900 | 1512, Storm settled | A3746FDFE6C1B84F055BAF23312A0CAB9088306FA6D76287B62B1733EE336C41 |
| notebook-page-Inventory.png | 1600×900 | 1555, Inventory settled | B8DFF024FACD6F34F22ACB6D0E4B30828B724EC61253DF6AC6A2C7D5D7609601 |
| notebook-960x540.png | 960×540 | 1665, Inventory settled | 10927A9AB51BA3083D484BEBA6A68EB11D79DA22315115E64FE8F055B79E9074 |

## Kabul sınırı

**Native API/data/state:** Yukarıdaki 32 notebook satırı bu DLL/izole fixture için PASS. **Native render:** Altı static sayfa + iki dönüş örneği + ana ve küçük çözünürlük kareleri bu incelemede PASS; R2 hedef görsel kusuru kapanmış. **OS/hardware:** **NOT_RUN**. `build-identity.json` `SW_SHOWNOACTIVATE`, `OSInput=false` kaydeder; J/launcher/MouseDown→MouseUp/F2 ve aynıframe hardware öncelikleri bu public API driver'ıyla fiziksel giriş kabulü değildir. **İnsan/keyif/tüm kampanya:** **UNVERIFIED**. Dikey/ultrawide oranlar, notebook-Jury native clock ve kesintisiz bütün animasyon/video burada yeniden çalıştırılmadı.

Genel 33,96 sn telemetry medyanı 16,67 ms, p95 17,25 ms; iki >100ms, bir >500ms aralık var. Bunlar tüm enstrümante batch'e aittir; yalnız raster notebook için ayrı GPU/performance sertifikası değildir. Kalan bu kanıt sınırları yeni bir üretim değişikliği ihtiyacı olarak yorumlanmadı.
