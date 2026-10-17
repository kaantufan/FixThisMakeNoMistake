> Public publication copy: the selected source manifest differs in scope from the full local build manifest. Native results below describe the recorded package only. OS testing state is a snapshot and may still be in progress. [Download](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.13), [source/build comparison](Publications/Snapshots/20261017T162430Z/source-vs-build.json). Local-only historical evidence links remain local references.

# Home V4.13 — logo ve Windows EXE ikonu teslimi

17 Ekim 2026. Unity 6000.0.73f1 / Built-In / Windows64 / 0.4.13. Güncel aday `Home-V4.13-20261017`.

[Başlatıcı](../Play-TooLateToPanic.cmd) · [Portable ZIP](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.13) · [LATEST](../LATEST-BUILD.json).

V4.12-r5 kaynakları üzerine onaylı logo paketi alındı. Başlıkta saydam logo, 1600×900 mantıksal canvas içinde oranı korunarak çizilir; doku yüklenirse eski metin/fırça çizilmez. Yalnız doku yokluğunda eski başlık fallback'i kalır. Windows ikonu küçük boyutlarda çatlaksız, büyük boyutlarda tam amblemdir. Tasarım kaynağı [HUD-Concept/logo](HUD-Concept/logo/) ve [logo.html](HUD-Concept/logo.html); harfler yerel geometriyle üretilmiştir, logo için harici font bağımlılığı yoktur. Kaynak PNG/SVG ve lisans arşivleri korunur. Amblemler runtime Resources dışında kalır, EXE ikon kaynaklarına gömülür.

## Korunan güncel taban

Diğer root'un [R5 devir kaydı](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/hatch-upward-intake/HANDOFF-READY.json) sahipliği bıraktığını ve normal turu tamamladığını doğrular. Normal kampanyada120sn izometrik önlem +30sn otomatik FPS çanta,1su+1gıda, yüzde75hasar sınırı, gerçek asset yüzeyinde seçim/pickup parıltısı ve yukarı açılan sığınak kapağı korunur. Diğer98C# kaynak byte olarak değişmedi. Yalnız HUD ve builder değişti;9dosya intake edildi. HUD eski paket baseline'ı yerine yeni150sn HUD tabanına dar olarak yeniden uygulandı. Ürün/şirket adı, persistentDataPath kimliği, skorlar, controller, Model/Expedition ve ses politikası değiştirilmedi. Beş kritik sanat cache prefabında hash kayması0; `-reuseResidenceArt` kullanıldı, sanat/karakter yeniden bake edilmedi.

## Son DLL'ye bağlı kontroller

Tek birleşik build:0hata/0uyarı. Log: `HOME_BRAND logo+icon sizes=1024,512,256,128,64,48,32,16`.

Yeni GUID skor profilinde **224/224 native PASS,0runtime hata,24.12sn**. [Receipt](Publications/Snapshots/20261017T162430Z/native-summary.json), [build kimliği](../../Evidence/GamejamOrchestra/TLTP-LOGO-20261017/manager-v413/native-v1-b152ef32-46e1-480a-9a44-7bd6eb7223e9/build-identity.json), [1600×900 başlık](Publications/Snapshots/20261017T162430Z/screenshots/title-native.png). Test15konum/saat/hazır fikstürü kullanır; üretim API/frame/render kanıtıdır. OS tuş/fare, insan keyfi/dinleme veya bütün kampanya kabulü değildir. R5'teki başarılı normal OS turu önceki DLL'ye aittir; V4.13'te tam tur yeniden oynanmış sayılmaz.

1600×900 gerçek native başlık, onaylı kompozit önizlemeyle root ve bağımsız incelemede karşılaştırıldı: logo görünür, oran doğru; kicker, slogan ve menüyle çakışma/kırpılma veya eski metin/fırça çift çizimi yok. [Bağımsız inceleme](../../Evidence/GamejamOrchestra/TLTP-LOGO-20261017/visual-review-v413/TITLE-CAPTURE-REVIEW.json).

960×540 renderer, ayrı izole normal QA penceresinde E2E telemetry ile doğrulandı. [Windows pencere karesi](../../Evidence/GamejamOrchestra/TLTP-LOGO-20261017/manager-v413/title-960-076103fa-d8f3-4147-a2ee-db61b42927ad/windows-title-960.png)643×391mantıksal pencere pikselidir;960×540framebuffer PNG'si diye sunulmaz. Logo ve tüm menü yerleşimi çakışmadan görüldü. İlk gözlem beklenmedik biçimde kısa akıştaydı; başlık kabulünden çıkarıldı. Root yalnız kendi izole QA penceresinde Esc ve normal BAŞLIĞA DÖN düğmesini kullanarak başlığı kontrol etti; yalnız bu QA süreci62696kapatıldı. [UI receipt](Publications/Snapshots/20261017T162430Z/BRAND-UI-RECEIPT.json).

EXE'nin gerçek `RT_GROUP_ICON`103/lang1033kaynağında9bağımsız32bpp DIB:16/24/32/48/64/96/128/192/256px. Boyut/bayt/decode ve beklenen varyant9/9doğru.16/32px küçük çatlaksız,48/256px tam amblem; gerçek payloadlar çıkarılıp görseller incelendi. `ExtractAssociatedIcon`32px seçti; çıktısı gerçek gömülü32px RGBA ile aynı. EXE iki araç öncesi/sonrası değişmedi. [İkon QA](../../Evidence/GamejamOrchestra/TLTP-LOGO-20261017/qa-v413/FINAL-ICON-QA.md), [envanter](../../Evidence/GamejamOrchestra/TLTP-LOGO-20261017/qa-v413/pe-final-v1/icon-inventory.json).

Explorer ayrıntı listesinde yeni EXE küçük amblemi görüldü. Ek büyük simge görünümü tamamlanmadan Computer Use fiziksel Esc ile kullanıcı tarafından durduruldu; sonrasında Computer Use çağrısı/girdisi yapılmadı. Bu ek kontrol **NOT_RUN_USER_STOPPED**; PE ve ilişkilendirilmiş ikon API kanıtının yerine geçtiği iddia edilmez. Bir `no monitor found` araç hatası taze pencere seçimiyle giderildi; başarısız çağrı ve Explorer sınırı UI receipt'te tutulur.

## Paket kimliği

| Kayıt | SHA256 / sonuç |
| --- | --- |
| Assembly-CSharp.dll | `97996C5ED34A843FD4FA04D79761C6004A022EF1643DCE79257B4EFD3F83207E` |
| EXE | `D26B0F57817DB378557C445524C48D28EE559FB2BE53CA5DC63E37C1E4AD22F5` |
| ZIP | `E381BD36A9ABD96EF99E0051F0EC3392AFAEA3EC35C0156184E459964A08CCA7` |
| ZIP boyutu | 178211598bayt |
| Tam kaynak manifesti | 34552dosya; `A464CBE226830C5D49B7560FD9C8CBF51D6780CF99218EF4EB402B1C583BA3F2` |
| Build manifesti | 154dosya; `C21FBE6464B1BBF7716FAB31A893290032D73AFAA753131A4475CA7839EF3273` |
| Kaynak kayması |100C#dosyada0; diğer98kaynak değişmedi |
| Gerçek skorlar |3dosyanın önce/sonra SHA256değerleri aynı |

[Kaynak manifesti](Publications/Snapshots/20261017T162430Z/source-sha256.json), [build manifesti](Publications/Snapshots/20261017T162430Z/local-build-file-sha256.json), [paket receipt](../../Evidence/GamejamOrchestra/TLTP-LOGO-20261017/manager-v413/PACKAGE-RECEIPT.json), [intake](../../Evidence/GamejamOrchestra/TLTP-LOGO-20261017/manager-v413/INTAKE-RECEIPT.json). ZIP içindeki EXE ve DLL test edilmiş dosyalarla hash olarak eşleşir. Font/asset/ses lisansları ve yeni marka provenance metni paketin Licenses klasöründedir. Yeni ücret/kredi veya GitHub yayını yoktur.

R4 PID64564teslim kontrolünde açık ve dosyaları aynı kaldı. Devirde açık olan R5 PID47320son kontrolde görülmedi; bu root R5'e girdi/sonlandırma göndermedi, sona erme nedeni bilinmiyor ve disk dosyaları aynı. [Süreç/dosya receipt'i](Publications/Snapshots/20261017T162430Z/PLAYERS-PRESERVATION-RECEIPT.json). Bağımsız eski FPS projesine dokunulmadı. Mevcut build/ZIP'ler silinmedi veya taşınmadı; önceki reddedilmiş temizlik hedefleri yeniden denenmedi.
