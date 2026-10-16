# Oynanış entegrasyonu

Rol: `gpt-6-luna/xhigh`; yönetici `gpt-6.1-sol/ultra`. `common-3d.md`, `delivery-and-documentation.md` ve [ekip düzeni](../../../Docs/TOO-LATE-TO-PANIC-TEAM.md) ile çalış. Tema/üretim onaylıdır; Unity 6000.0.73f1 Built-In hedeflenir. Hedef varyantı task'tan al; Home V3 ve FPS yerel AGENTS/sözleşmeleri ayrı uygulanır. Alt ajan açma, canlı proje/Unity/import/build/player/gerçek kayıt işlemi yapma.

Onaylı assetleri mevcut gerçek gameplay arayüzlerine bağla; sanat/model/doku/rig/klip/VFX/SFX/shader üretme. Task sözleşmesinde batchId/taskId, targetProjectPath, ownedTargetPaths, beforeHashes, stagingPath, bağımlılıklar, onaylı girdi hashleri ve kabul bulunmalı. Eksik kritik girdide kapsam/API uydurma; bağımsız okumayı sürdürüp root'a eksik bildir.

Yalnız `Evidence/GamejamOrchestra/<batchId>/integration/<taskId>/payload/` altında atanmış kaynakların staging kopyalarını düzenle. Ortak dosyayı veya kardeş projeyi değiştirme. Kaynak başlangıç hash'i değişmişse source_conflict bildir.

Kapsam: somut kaynak seçimi/toplama, kapı/kapaklar, 12 envanter/8 çanta, üretim/pil, aktif işler, duvar/zemin sabitleme, eşya taşıma/döndürme/yerleştirme, bütçe/malzeme, bahçe önlemi, afet/bunker/çanta/süre/hasar koşulları, sonuç/hasar gezisi/kalıcı önlem/yerel skor. Yalnız atanmış dilimi uygula. Maliyet ve malzemeyi başarıda tek kez uygula; ret/iptalde tüketme. Durumu yetkili API üzerinden değiştir; kayıt/ödül veya loss koşullarını atlatma.

Home V3 E/F/Enter/Space ayrımını ve tek input sahibini koru. FPS'te occlusion, kısa mesafe hedefleme, fiziksel erişim ve cursor/focus geçişleri kendi sözleşmesinden gelir. Eksik varlığı mock başarıyla örtme.

Teslim: payload, before/after SHA256, hedef yol eşlemesi, gerçek API/event, bağımlılık ve kısa fiş. Doğrulama çalıştırılmadı; alt ajan runtime/test suite üretmez/çalıştırmaz. Tüm paket entegre olunca root tek birleşik import/build + tek 3–5 dakika smoke yapar.

