# Home V5.7 — UI ve birleşik görsel kaynaklar

18 Ekim 2026. Yeni ücret, abonelik, sağlayıcı işi veya kredi kullanımı yoktur. Tam kaynak/metadata kimliği son kaynak manifestinde, çalışan oyun ve paket kimliği [teslimde](HOME-V5.7-UI-DELIVERY-2026-10-18.md) ve [LATEST](../LATEST-BUILD.json) içindedir. Kaynak kökeni tek başına native/OS/insan kabulü değildir.

## Özgün arayüz

33 çizgi simge `PanicUiSymbolsV57.cs` içinde proje için özgün geometridir. 32 birim koordinat sistemiyle tutarlı kalınlıkta çizilir,128px alfa dokularında cache edilir; font, emoji veya dış ikon paketi gerekmez. `shelter`, `shield` çiziminin takma adıdır. Ortak HUD sahibi kapanırken doku cache'i bırakılır. Metin, sayı ve odak çizgisi kritik anlamı simgeyle birlikte taşır. Yeni kod için üçüncü taraf ikon lisansı yoktur; ayrı bir proje kod lisansı bu çalışma tarafından tanımlanmadı.

Menü/HUD sunumu `PanicHomeHudMenusV57.cs` ve `PanicHudDesignV57.cs` özgün C# çizimidir. Mevcut Türkçe eylemler, gerçek model/snapshot/recipe, oyun logosu ve Built-In çizimi korunur. Yeni UI framework, shader veya renderer göçü yoktur. F1'de kullanılan34 tuşun tamamı ve kısa ortogonal açıklama hatları yetkili akran `keyboard-help-r3` paketinden gelir; bağımsız geometri checker'ı aynı son kaynak manifestine bağlıdır.

## Korunan font ve logo

Mevcut çevrimdışı Oswald, Barlow Condensed ve Barlow fontları SIL OFL1.1 lisanslıdır. Lisans metinleri `Documentation/Licenses/HUD/oswald-OFL.txt`, `barlowcondensed-OFL.txt`, `barlow-OFL.txt` içindedir; paketleyici bunları `Licenses/HUD` altına byte kimliğiyle taşır. Mevcut `TltpLogo.png` yeniden kullanılır; önceki köken kaydı `HOME-V4.13-BRAND-DELIVERY-2026-10-17.md` içinde korunur. Yeni logo üretimi veya görsel indirmesi yapılmadı.

## Paralel birleşik kapsam

Yarkın karşılaştırma branch'i `834ee55b01fd23b2effd0ef8f48e2fbffbe87edb` içindeki seçili proje sanatı kullanıcı yetkisiyle alındı. Mevcut dolu hortum çekirdeği yeni dış kabuk/bant/döküntüyle birlikte kalır; depremde yalnız dış toz/döküntü eklenir; mevcut yangın alevleri sınırlı büyütülür. Yeni küçük hasar izleri ve komşu görev aksesuarları çalışma zamanında üretilir. Ana karakter görünümü, retained humanoid/UAL, ev/çatı/collider kökü ve gerçek ses sahipleri korunur. Ayrıntılı önce/sonra hash, GUID, üç dar hook ve kaynak temizleme sahipliği `TLTP-YARKIN-HYBRID-20261018/manager/integration/FREEZE.json` ve `ROOT-HANDOFF.md` içindedir. Lisansı çelişkili dokuz donor Foley WAV alınmadı.

V5.6'nın19 özgün sentez PCM klibi ve önceki asset/lisansları korunur. Kaynak/provenance bilgisi [HOME-V5.6-ASSETS](HOME-V5.6-ASSETS-2026-10-18.md) içindedir. Önceki V5.6 native388 kontrolü eski DLL'ye aittir; Windows turu fiziksel Esc ile durdu, V5.6 ZIP/LATEST tamamlanmadı. Bu aktarım yeni ses üretimi veya yeni DLL için insan dinleme kabulü değildir.

## Kaynak kimlikleri

| Kaynak | SHA256 |
|---|---|
| `Assets/Game/Runtime/PanicUiSymbolsV57.cs` | `2A1A1F59ABFE69E2B7BA7899871D16C9437BEE5CB3420C35D8463475C9EA6FEA` |
| `Assets/Game/Runtime/PanicHomeHudMenusV57.cs` | `F8065330F3674017DAB9F9AD757471543316708456ABBB47DD67562B53DAFA3E` |
| `Assets/Game/Runtime/PanicHudDesignV57.cs` | `154FABD1F6FB3F262FEE44316A7735AC4702F9B0029DF2B46A62A610022B64A2` |
| `Assets/Game/Runtime/PanicKeyboardHelpV56.cs` | `80D862819AD1FC8D21DD4ACBA24CAA072966B20575278C24DF8509FA1ED457C5` |
| `Assets/Game/Runtime/PanicKeyboardHelpGeometryChecksV57.cs` | `F0CBEB5F7526D69CF09599A1B1CF752E3F20FB03C1FF0B323A472A0E872080BB` |
| `Assets/Game/Resources/HUD/Oswald.ttf` | `BA14F719BDE660524CCD7ED17693862FF5E081EF4CA7D54CFC1AF8505C4BFA9F` |
| `Assets/Game/Resources/HUD/BarlowCondensed.ttf` | `7B619D14BC2327509A9EF32B0890F709626F7ECC9FF61191C2A4314C5499D2D9` |
| `Assets/Game/Resources/HUD/Barlow.ttf` | `95AA02C7C43096E0DD44D787BA6216864A67157E402ADAB59B35572E0C1577EA` |
| `Assets/Game/Resources/HUD/TltpLogo.png` | `9438D546EB81BAC5AB0D196F099B1C485167FC96205A3A7B499B29DC38322D79` |

Araştırma ve uygulama eşlemesi: [UI araştırması](HOME-V5.7-UI-RESEARCH-2026-10-18.md). Seçili renk çiftlerinin kaynak hesabı, gerçek font/framebuffer ve insan anlamlandırması ayrı katmanlardır; genel XAG/WCAG uygunluğu iddia edilmez.
