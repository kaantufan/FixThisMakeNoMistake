# Yarkın — Too Late to Panic! görsel uyarlaması

Kaynak: takım arkadaşının `yarkin-assets` dalı, commit `eb05475ee0533b86031b36f156f6022521118a15`, [GitHub](https://github.com/kaantufan/FixThisMakeNoMistake/tree/eb05475ee0533b86031b36f156f6022521118a15/Assets/_YarkinAssets).

Upstream beyanı, 32 modelin ve ikonların jam sırasında Blender/Unity ile prosedürel olarak üretildiğini, Antigravity AI Agent kullanıldığını ve dış hazır model alınmadığını bildirir. Beyan `UPSTREAM-ASSET-MANIFEST.md` içinde özgün haliyle korunur. Bu bir kaynak beyanıdır; etkinlik kuralları veya üçüncü taraf hakları bağımsız olarak doğrulanmış sayılmaz. Bu uyarlama dosyalara CC0 veya yeni bir açık kaynak lisansı atamaz. Kullanıcı takım arkadaşının assetlerini bu oyuna alma talimatı vermiştir.

Uyarlama: Codex / Too Late to Panic! ekibi, 17 Ekim 2026. Dış model, font veya ücretli sağlayıcı eklenmedi. FBX ve PNG içerikleri upstream SHA256 ile byte düzeyinde aynıdır. 12 Standard materyalin renkleri Home V4'ün sıcak ev paletine yaklaştırılmıştır; metal/polimer ayrımı korunmuştur. Kaynakta emission rengi atanıp keyword'ü açılmamış iki Glow materyalinde `_EMISSION` etkinleştirilmiştir; ek dinamik ışık üretilmez. Kaynak atlas ve model import meta dosyalarının GUID referansları korunur.

`PanicCuratedAssetArt` ham FBX'yi yalnız görsel olarak yükler, +Z ön yüzü Home'un -Z standardına çevirir, uniform ölçekte metreye ve taban-merkez pivota uyarlar. Yeni skor, afet, envanter, interaction veya rigidbody sistemi getirmez. Upstream runtime scriptleri, otomatik editor builder'ı, prefab colliderları ve zincirleme afet değerlendiricisi içe aktarılmamıştır. Kaynaklar staging'deki ayrı Git checkout'unda ve binary manifestinde korunur.

Aktif eşya kimlikleri aynıdır: su, yiyecek, ilk yardım, fener, pil, radyo, battaniye ve yangın tüpü görselleri. Yiyecek için konserve/elma/snack varyantları aynı supplyId'nin kozmetik biçimleridir; adet/ağırlık/bütçe/skor değişmez. Çanta, evrak ve tezgâh elması salt dekor olarak kullanılır. Kalan modeller görsel kütüphanededir; mevcut işlevli mobilyaları otomatik değiştirmez.

32 modelde rig/clip yoktur. Elde taşınan veya çevrede duran katı eşyalar için rig gerekli değildir; pickup motion dış wrapper üzerinde uygulanır. Upstream buzdolabı, vana, kitaplık ve TV tek mesh'tir; var olan kapak/menteşe/sabitleme davranışının yerine kullanılmaları ayrıca parçalama ve işlevsel kabul gerektirir. Mevcut açık buzdolabı, TV konsolu ve dolu kitaplık bu nedenle korunur. Oyuna yeni afet veya güvenli alan öğretisi eklenmez.

Kaynak indirme, offline geometri incelemesi ve statik bağımlılık kontrolü yapılmıştır. Unity import, Built-In sahne gölgesi, yeni C# derlemesi, native kamera/yerleşim/performans ve insanla okunurluk kabulü ilgili paket root'unun işidir; bu belge bunları geçti iddiası taşımaz.
