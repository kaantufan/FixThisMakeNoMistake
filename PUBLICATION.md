# Too Late to Panic! — 17 Ekim 2026 GitHub yayını

[Windows V4.8 indir](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.8). ZIP'i çıkarıp TooLateToPanic.exe dosyasını yan klasörleriyle birlikte çalıştırın. Unity kaynak projesi TooLateToPanic/ klasöründedir; Unity 6000.0.73f1 / Built-In kullanılır.

Bu commit aktif izometrik çalışma alanının güncel kaynaklarını, HUD/çevre/afet/akış geliştirmelerini ve oturumda bulunan son değişiklikleri baytları değiştirilmeden aktarır. Canlı oyun kaynakları, ayarlar, LATEST ve teslim belgeleri yayın için düzenlenmedi. FPS askıdadır ve yerelde korunur. Cache, gerçek kayıt/skor, anahtarlar, tarihsel Evidence ve eski Tolga WAV arşivi yayın kapsamı dışındadır. Gerekli generated assetler statik GUID bağımlılık kapanımıyla seçilir; eski bake geçmişi yerelde korunur.

Mevcut V4.8 Windows ZIP'i yeniden derlenmeden yayımlanır. Bu build'in kayıtlı sonucu149/149 kesintisiz gerçek zamanlı üretim API rotası ve189/189 regresyon kontrolü,0runtime hatadır; OS/insan oynanışı, dinleme veya uzun kampanya kabulü değildir.

Doğrulanmış build manifestinden sonra değişmiş kaynaklar aşağıdadır. Bu değişiklikler aynen korunmuştur; mevcut ZIP'in kabulü bu kaynak değişiklikleri için yeni build/native kabulü sayılmaz:

- `TooLateToPanic/Assets/Game/Editor/PanicResidenceBuild.cs`
- `TooLateToPanic/Assets/Game/Runtime/PanicHomeHud.cs`
- `TooLateToPanic/Assets/Game/Runtime/PanicHomeWorld.cs`
- `TooLateToPanic/Assets/Game/Scenes/TooLateToPanicResidence.unity`
- `TooLateToPanic/Assets/Game/Runtime/PanicInteractionAudit.cs`
- `TooLateToPanic/Assets/Game/Runtime/PanicInteractionFrameAudit.cs`
- `TooLateToPanic/Assets/Game/Runtime/PanicInteractionModelAudit.cs`
- `TooLateToPanic/Assets/Game/Runtime/PanicInteractionWorldAudit.cs`

[Yayın kimliği ve ZIP hash'i](TooLateToPanic/Documentation/RELEASE-v0.4.8.json), [baytları korunan kaynak manifesti](TooLateToPanic/Documentation/Publications/v0.4.8/source-sha256.json), [V4.8 teslim kaydı](TooLateToPanic/Documentation/HOME-V4.8-DELIVERY-2026-10-17.md).

Mevcut README/LATEST/teslim dosyalarındaki yerel Builds/Evidence/FPS bağlantıları ve yayından önceki durum ifadeleri orijinal anlık kaydın parçasıdır. GitHub yayınının güncel durumu bu ayrı kayıt ve Release ile belirlenir. Yerel başlatıcılar kaynak arşivi içinde build indirmez; hazır oyun için yukarıdaki Release ZIP'ini kullanın. Asset/font/ses provenansı ve mevcut lisans metinleri kaynaklarla birlikte korunur; yeni bir genel lisans atanmaz.
