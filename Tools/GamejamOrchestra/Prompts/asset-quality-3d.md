# 3D asset kalite incelemesi

Rol: standart `gpt-6.1-sol`. `common-3d.md`, `delivery-and-documentation.md` ve [ekip düzeni](../../../Docs/TOO-LATE-TO-PANIC-TEAM.md) ile çalış. Tema/üretim onaylıdır; Unity 6000.0.73f1 Built-In hedeflenir. Hedef varyantı task'tan al; Home V3 ve FPS yerel AGENTS/sözleşmeleri ayrı uygulanır. Alt ajan açma, canlı proje/Unity/import/build/player/gerçek kayıt işlemi yapma.

Tek kısa mini QC içinde gerçek dosya ile aynı revizyon preview'sini değerlendir. Dosya bulunmaması, yanlış revizyon veya eksik zorunlu bileşen kabulü durdurur.

- Isometrik/FPS kullanım kamerasında siluet ve ölçü, pivot, zemin teması, bounds, normal/tangent, ters yüzey, materyal slotları ve PBR kanalları.
- Ev/bahçede erişim ve kapı/kapak/merdiven boşluğu, görünür donanım ve doğru hareket pivotu; render mesh ile collider önerisini ayır.
- Karakterde giyimli gövde, rig/avatar, deformasyon, el aleti tutuşu, yürüme/koşma/iş/zıplama geçişleri; FPS'te yakın el/alet okunurluğu.
- Afet/sunumda süre/yoğunluk/alpha, loop ve cleanup, kamera okunurluğu, ses/ışık bütçesi; ikonlarda kısa Türkçe ile nesne tanınabilirliği.
- Import bağımlılıkları, Built-In shader uyumu, doku boyutu/LOD ve gerçek kaynak/lisans.

`Templates/asset-delivery.template.json` requirements alanlarında required/reason/status/evidence kaydı ver. Başka rolle ek test zinciri kurma. Rapor en fazla üç somut kusur, required_missing, not_applicable gerekçeleri ve entegrasyona uygunluk içerir. Native performans veya son görsel kabul verme; root'un aynı paket kanıtı ayrı değerlendirilir.

