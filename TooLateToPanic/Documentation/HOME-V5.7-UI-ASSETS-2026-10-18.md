# Home V5.7 — UI ve birleşik görsel kaynaklar

18 Ekim 2026. Yeni ücret, abonelik, sağlayıcı işi veya kredi kullanımı yoktur. Bu belge çalıştırıldığı anda seçilen gerçek son kaynaklardan dinamik SHA256 üretir; native/OS/insan kabulü üretmez. Hashlenen kaynak proje: `C:\Users\kaant\Desktop\civilgamejam\TooLateToPanic\TooLateToPanic`. Son kimlik ve durum [güncel teslim](HOME-V5.7-UI-DELIVERY-2026-10-18.md) ve [LATEST](../LATEST-BUILD.json) ile bağlanır.

## Özgün arayüz

33 çizgi simge `PanicUiSymbolsV57.cs` içinde proje için özgün geometridir.32 birim koordinat sistemiyle çizilir,128px alfa dokularında cache edilir; font, emoji veya dış ikon paketi gerekmez. `shelter`, `shield` çiziminin takma adıdır. Ortak HUD sahibi kapanırken kendi doku cache'ini bırakır. Metin, sayı ve odak çizgisi kritik anlamı simgeyle birlikte taşır. Yeni kod için üçüncü taraf ikon lisansı yoktur; bu çalışma ayrı bir proje kod lisansı tanımlamaz.

Menü/HUD sunumu `PanicHomeHudMenusV57.cs` ve `PanicHudDesignV57.cs` özgün C# çizimidir. Mevcut Türkçe eylemler, gerçek model/snapshot/tarif ve oyun logosu kullanılır. Ortak sabit düz çerçeve ve görünür odak dili korunur. Yeni UI framework, shader veya renderer göçü yapılmaz. F1 ve envanterin son kaynak/native kapsamı aşağıda ayrı kanıt katmanlarıyla yazılıdır.

## Korunan font ve logo

Mevcut çevrimdışı Oswald, Barlow Condensed ve Barlow fontları SIL OFL1.1 lisanslıdır. Lisans metinleri `Documentation/Licenses/HUD/oswald-OFL.txt`, `barlowcondensed-OFL.txt`, `barlow-OFL.txt` içinde korunur ve aşağıda hashlenir. Son taşınabilir paketteki lisans byte kimliği paket receipt'iyle ayrıca doğrulanmalıdır; kaynakta bulunmaları yeni ZIP'in kontrolü değildir. Mevcut `TltpLogo.png` yeniden kullanılır; köken kaydı [önceki marka tesliminde](HOME-V4.13-BRAND-DELIVERY-2026-10-17.md) korunur. Yeni logo üretimi veya görsel indirmesi yoktur.

## Paralel birleşik kapsam ve provenance

Yarkın karşılaştırma branch'i `834ee55b01fd23b2effd0ef8f48e2fbffbe87edb` içindeki seçili proje sanatı yetkili birleşim kapsamıdır. Dolu hortum çekirdeği yeni dış kabuk/bant/döküntüyle birlikte tutulur; depremin dış toz/döküntü ve yangının sınırlı büyütülmüş alev sunumu kaynak/devralma kayıtlarıyla bağlanır. Küçük hasar izleri ve komşu görev aksesuarları çalışma zamanı kaynaklarına aittir. Ana karakter görünümü, retained humanoid/UAL, ev/çatı/collider kökü ve gerçek ses sahipleri korunur. Ayrıntılı önce/sonra hash, GUID ve dar hook devri `TLTP-YARKIN-HYBRID-20261018/manager/integration/FREEZE.json` ve `ROOT-HANDOFF.md` içindedir; bunlar yeni DLL'nin native kabulü değildir. Lisansı çelişkili dokuz donor Foley WAV alınmaz.

V5.6'nın19 özgün sentez PCM klibi ve önceki asset/lisansları [V5.6 varlık kaydında](HOME-V5.6-ASSETS-2026-10-18.md) korunur. Eski V5.6 testleri eski DLL'ye bağlıdır; durdurulmuş Windows turu veya tamamlanmamış eski ZIP/LATEST yeni adaya taşınmış kabul sayılmaz. Bu aktarım yeni ses üretimi ya da insan dinleme kabulü değildir.

## Güncel kapsam ve kabul sınırı

F1'in önceki karo/kart tasarımı insan kararıyla reddedildi. Son **r4** kaynak ve DLL'sinde eski tek fiziksel klavye korunur:56 kapak,34 kullanılan tuş,22 dış açıklama grubu ve temsilci tuşlardan çıkan106 ortogonal parça. Gerçek font ölçümleri ve iki çözünürlükte actual Repaint geometri kontrolü yeni DLL'de geçti. Bu, insan okunurluk/beğeni kabulü değildir.

**Tek ortak20 yuvalı envanter** son kaynaklara alındı. Su/gıda doğrudan hazırdır; gerçek World dalga çözümü aynı listeden birer birim tüketir. İki bölmeli ve elle paketleme sunumu kaldırıldı.42 bağımsız model kontrolü,20 gerçek hücre/klavye/dolu görüntü/font ölçümü ve World tüketim/tekrar-sync kontrolleri yeni DLL'de geçti; doğal toplama rotası ve OS drag kabulü diye sunulmaz.

R3'te açık zemin üzerindeki ENVANTER başlığı gerçek PNG'de1,3273:1 bulundu; eski Brush swatch'ları koyu zemin ve tek vurgu çizgisiyle düzeltildi. R4 gerçek iki çözünürlük ölçümü **11,84967:1**'dir; bu seçili opak glyph/zemin çiftiyle sınırlıdır. Diğer bütün UI için genel uygunluk çıkarılmaz. Son DLL `BA49CD61AE45BB29CA68AF29EEEEB8BDED9574C8D7B5B2D48B3EC477D64FD394`; ajan piksel incelemesi `visual-review-r4.json` ve güncel teslimle bağlıdır.

120sn önlem +45sn çanta, gerçek maliyet/ücretsiz iptal, sonlu RNG, %75 kayıp, skor yalıtımı ve gerçek controller/sığınak kapısı korunur. Selde üst panik odası, diğer afetlerde bodrum; gerçek girişin kaynağı `Model.ReachedShelterV5` ve `TryEnterActiveShelterV5` üretim kapısıdır. Görsel hazır etiketi veya sayaç fikstürü fiziksel giriş kabulü değildir.

Kaynak, derleme, native API, framebuffer/piksel, fiziksel OS girdisi ve insan/işitme kabulü ayrı katmanlardır. Yeni sürümün native/OS sonuçları, yeni DLL ve kaynak/ZIP kimliği [güncel teslim](HOME-V5.7-UI-DELIVERY-2026-10-18.md) ve [LATEST](../LATEST-BUILD.json) ile birlikte incelenir. Bu üretici **humanAcceptance=false** ve **listeningAcceptance=false** sınırını korur; kaynak hashinden, eski testten veya kanıt dosyasının varlığından yeni **PASS** sonucu kurmaz.

- Son native kanıtı: [ui-v57-smoke.json](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-ui-9a7e8b18-c4c8-4fa1-902a-671002ae874e/ui-v57-smoke.json>) — SHA256 `0FD31E226BADE6DE7F54B3FBEBDE04385F597FA510050A25C62524ED61F0DB4B`; dosya varlığı veya hash tek başına geçti sonucu üretmez.
- Son native kanıtı: [hybrid-report.json](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-hybrid-6ddcd34c-407e-4e24-940a-7df8056e6f16/hybrid-report.json>) — SHA256 `DB05D77F58507A07A864DC98443E78DE535ABFCB3D8CDA38F0396622BE13EE38`; dosya varlığı veya hash tek başına geçti sonucu üretmez.
- Son native kanıtı: [v56-smoke.json](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-v56-574175bd-8aff-4e99-80f6-37660e7f6c58/v56-smoke.json>) — SHA256 `419407357348AEC2DFDAC7A99E5494C51C9DA4F0F4F282605E645E9EF970E4A7`; dosya varlığı veya hash tek başına geçti sonucu üretmez.
- Son native kanıtı: [wave-ledger-v56-smoke.json](<../../Evidence/GamejamOrchestra/TLTP-UI-REDESIGN-V57-20261018/manager/native-r4-ledger-ca1fb961-b7c8-4c30-b190-e783e38974dd/wave-ledger-v56-smoke.json>) — SHA256 `358B230DAF8EA1453BF6ED17EC84F290A71849DACEA71F0AC6E97E8AD31E6AC6`; dosya varlığı veya hash tek başına geçti sonucu üretmez.
- Son OS kanıtı: Bu çalıştırmaya yeni kanıt yolu verilmedi; durum ve kesin DLL bağı teslimde açıkça yazılmalıdır.

## Bu çalıştırmada okunan kaynak kimlikleri

| Kaynak | SHA256 |
|---|---|
| `Assets/Game/Resources/HUD/Barlow.ttf` | `95AA02C7C43096E0DD44D787BA6216864A67157E402ADAB59B35572E0C1577EA` |
| `Assets/Game/Resources/HUD/BarlowCondensed.ttf` | `7B619D14BC2327509A9EF32B0890F709626F7ECC9FF61191C2A4314C5499D2D9` |
| `Assets/Game/Resources/HUD/Oswald.ttf` | `BA14F719BDE660524CCD7ED17693862FF5E081EF4CA7D54CFC1AF8505C4BFA9F` |
| `Assets/Game/Resources/HUD/TltpLogo.png` | `9438D546EB81BAC5AB0D196F099B1C485167FC96205A3A7B499B29DC38322D79` |
| `Assets/Game/Runtime/PanicCompactHudV5.cs` | `F99CCFBA3D08516EE8DC163098860F6E070C6F1C72AB74E34DC74F23DAFEDBA5` |
| `Assets/Game/Runtime/PanicExpedition.cs` | `0B43ED32ADE620E2753D754B8EAB95225F1AADB02E05ECFACBF2C78717EDE8B8` |
| `Assets/Game/Runtime/PanicFinalBagHud.cs` | `C3E140F26F0FFB01BC3567C83B69B0A10B5D5800E88BE93AA6046AC1B085063A` |
| `Assets/Game/Runtime/PanicHomeHud.cs` | `0797EC1B091F0C899AE52B8CE9D1F499EB957CBD38D98C0B53DB0701F90C5CCD` |
| `Assets/Game/Runtime/PanicHomeHudMenusV57.cs` | `467032569FB0F0D7AC7339E5EACD27FB23AF6535ECBD20AB9B7EFE85178A26C9` |
| `Assets/Game/Runtime/PanicHomeWorld.cs` | `A23657ABFB1205B52E8939AEE0189D240B6A59629011BEA258A7C7B9D9B23539` |
| `Assets/Game/Runtime/PanicHudDesignV57.cs` | `154FABD1F6FB3F262FEE44316A7735AC4702F9B0029DF2B46A62A610022B64A2` |
| `Assets/Game/Runtime/PanicKeyboardHelpGeometryChecksV57.cs` | `8A05EC76DF7E1AF7D10520E9E1007798CAE996D60621ABA29BDE416CB7020433` |
| `Assets/Game/Runtime/PanicKeyboardHelpV56.cs` | `6BC8892DDFE05A05EA40F557A7BD850D22D4196C8232EECBD8DD254E28CE5697` |
| `Assets/Game/Runtime/PanicModel.cs` | `5843612F94D29129DB53174EB978B3830BB2FFAD25F0A30354A09CB008F8D830` |
| `Assets/Game/Runtime/PanicNotebookOverlay.cs` | `B69FF1E3742D3D95F7E074BFDDE94FDBEDC74BDC49CDAD28F92485C4D81BAF31` |
| `Assets/Game/Runtime/PanicRevisionHud.cs` | `FCBE42D02457FBEFB4EE6BEFB15C147B0FBF0A8B001AE4FCC3325E047838CF07` |
| `Assets/Game/Runtime/PanicRevisionInventory.cs` | `58B7B2D2F4E48DDB9D73F63D76E20B20813946E7248B258021821534FECF57DE` |
| `Assets/Game/Runtime/PanicSelectedTargetHudV56.cs` | `69510EA1952C7F297F06DEA2D0211C4625351F1A2EF0D561C155FE29572CDC10` |
| `Assets/Game/Runtime/PanicUiSymbolsV57.cs` | `2A1A1F59ABFE69E2B7BA7899871D16C9437BEE5CB3420C35D8463475C9EA6FEA` |
| `Assets/Game/Runtime/PanicUnifiedInventoryChecks.cs` | `ACB79B32DECB8CE7A20A117B28878D4551C40F3ADC198E45C02397D0FE3559E7` |
| `Assets/Game/Runtime/PanicWaveLedgerHudV56.cs` | `B2AA634A378F81A5EC8B38F46340E3F9833B1EEC200A6DA1569955E0683CCFC9` |
| `Documentation/Licenses/HUD/barlow-OFL.txt` | `186D750EB496A4C17A76385F82BE6AEA2AC1CF2DE074A811D63786CF374EA73F` |
| `Documentation/Licenses/HUD/barlowcondensed-OFL.txt` | `186D750EB496A4C17A76385F82BE6AEA2AC1CF2DE074A811D63786CF374EA73F` |
| `Documentation/Licenses/HUD/oswald-OFL.txt` | `0FD731A904B729A4E02EAF5E8EBD06783EDD9ABE400E8882760160230675B652` |

Araştırma ve uygulama eşlemesi: [UI araştırması](HOME-V5.7-UI-RESEARCH-2026-10-18.md). Tam kaynak/metadata ve son paket kimliği yalnız son manifest/teslim ile tamamlanır. Hash listesi GUID kapanımı, import, native render veya genel erişilebilirlik uygunluğu yerine geçmez.
