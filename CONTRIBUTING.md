# Katkı ve güncelleme düzeni

Bu depo **Too Late to Panic!** içindir. Önce kök/proje `AGENTS.md`, güncel `LATEST-BUILD.json` ve ilgili son sözleşme okunur. Son kullanıcı kararları eski sürüm notlarından önceliklidir.

- Unity 6000.0.73f1 / Built-In korunur. Kaynak kökü `TooLateToPanic/`; sahne, GUID, kayıt kimliği veya klasörler görev gerektirmeden değiştirilmez.
- Canlı Unity/import/build/player işlemlerinin tek sahibi vardır. Alt görevler benzersiz staging alanında çalışır; açık kullanıcı oyuncusuna girdi/sonlandırma gönderilmez.
- `TooLateToPanicFPS/` bağımsız korunan arşivdir. Aktif oyundaki erzak FPS aşaması bu eski projeyi geliştirme yetkisi vermez.
- Kod/sanat/ses/metin değişikliğinde ilgili AI ve üçüncü taraf kaynak beyanı güncellenir. Lisans metinleri ve upstream kimlikleri korunur. Açık kaynak kodu ile üçüncü taraf asset lisansı aynı kabul edilmez.
- İlgili kontroller sürüm/DLL/source hash ile kaydedilir. Kaynak kontrolü, Editor, native API/render, OS girdisi ve insan kabulü ayrı yazılır; çalıştırılmayan kontrol `NOT_RUN` kalır.
- Build ve ZIP Git kaynak ağacına eklenmez. Cache, Evidence, özel anahtar, kullanıcı kaydı ve bağımsız FPS dışarıda tutulur. Gerekli generated Unity asset'leri yalnız referans/GUID kapanımı kontrol edilerek kaynak yayınına seçilir.
- Kabul edilmiş kaynak ve paket üzerinde sonradan değişiklik yapılmaz. Belge güncellemesi yeni oyun testi sayılmaz. Geçmiş receipt/manifest içerikleri yeniden yazılmaz.

Her teslimde [RELEASE-WORKFLOW.md](Docs/RELEASE-WORKFLOW.md) uygulanır. Normal push kullanılır; force push, otomatik yarışma teslimi veya yeni sağlayıcı harcaması bu düzenin parçası değildir.
