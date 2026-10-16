16 Ekim 2026 son kullanıcı kararı: aktif oyun izometrik TooLateToPanic'tir. FPS geliştirmesi durduruldu; yeni açık istek olmadan FPS görev/asset/build üretme. Mevcut FPS dosyalarını koru. Her işte ilgili proje AGENTS.md ve son sözleşme geçerlidir.

# Too Late to Panic! — ortak görev sözleşmesi

Tema **Afete Hazırlık ve Önlem**; üretim onaylıdır. Hedef Unity **6000.0.73f1 / Built-In**, çevrimdışı Windows. [Ekip düzeni](../../../Docs/TOO-LATE-TO-PANIC-TEAM.md), [tasarım](../../../Docs/TOO-LATE-TO-PANIC-GDD.md) ve [delivery-and-documentation.md](delivery-and-documentation.md) bu promptun parçasıdır.

İzometrik hedef `TooLateToPanic/`, güncel sözleşme `Evidence/GamejamOrchestra/TLTP-HOME-V3-20261016/CONTRACT.md` ve proje AGENTS'tir. FPS hedefi `TooLateToPanicFPS/`; kendi AGENTS ve `Evidence/GamejamOrchestra/TLTP-FPS-FORK-20261016/FPS-HANDOFF.md` kuralları geçerlidir. Görev tek varyantı açıkça seçer; kaynak/build/kayıt kimlikleri paylaşılmaz, sonuçlar otomatik birleştirilmez.

Yönetici ve QA `gpt-6.1-sol/ultra`; sanat/mini QC standart `gpt-6.1-sol`; entegrasyon `gpt-6-luna/xhigh`. Fast zorunlu değildir; model sessizce değiştirilmez. Root dahil 4 slot, en fazla 3 alt ajan; alt ajan yeni ajan açmaz. Model/thinking override çağrısı `fork_turns: "none"` ve açık görev bağlamı kullanır. Bu dosya global model/system ayarı değildir.

## Sahiplik ve girdiler

`Tools/GamejamOrchestra/Templates/task.template.json` alanlarıyla taskId/batchId, targetProjectPath, görünür hedef, ownedTargetPaths, beforeHashes, stagingPath, bağımlılık ve kabul gerekir. Yeni hedefin henüz bulunmadığı belirtilir. Mevcut hedef başlangıç hash'inden farklıysa source_conflict bildir; üzerine yazma veya dosya sahipliğini genişletme.

Alt ajan yalnız atanmış benzersiz staging'e yazar; canlı Assets/.meta/sahne/prefab/proje ayarı, import/build/player ve gerçek kayıtlarla işlem yapmaz. Root görev/batch kayıtlarının ve kendi varyantının canlı intake/Unity süreçlerinin tek sahibidir. Kaynaklar birleşik build ve smoke boyunca frozen kalır. Mevcut GUID'leri, kaynakları/lisansları ve gerçek kayıtları koru; kullanıcının açık oyuncusunu kapatma.

## Gerçek içerik ve bağlantı

Görev kapsamı kaynak/eşya, ev/bahçe, afet, karakter, sunum ve QA ihtiyaçlarından gelir. İlgili assette PBR/gölge/ışık, rig/klip/olay, collider/etkileşim, kapak/pickup/iş animasyonu, VFX/SFX, el tutuşu, yerleştirme ve mekanik/kayıt bağını değerlendir. Birim/eksen/pivot/bounds ve materyal slotlarını kaydet; hedef 1 birim=1 metre, Y yukarı/Z ileri. Gerekli olmayan bileşen gerekçeli not_applicable, eksik zorunlu bileşen required_missing olur.

Sanatçı gerçek dosya üretir; Luna yalnız onaylı içeriği görevde doğrulanmış mevcut API'ye bağlar. Eksik sanatı veya sahte başarı/placeholder akışını entegrasyon diye sunma. Asset-delivery şablonunun requirements/status/evidence alanlarını kullan; kaynak lisansı ile model/doku/ses lisansını aynı varsayma. Projeye gereksiz pipeline/framework göçü yapma. Yeni satın alma/kredi harcaması bu görev yetkisinde değildir. Sağlayıcı işini task ID ile izle; tek browser kontrol sahibi bulunur.

## Kontrol ve teslim

Asset başına tek kısa dosya/preview mini QC; belirgin kusurda bir hızlı revizyon. Paket tümüyle entegre ve frozen olunca root bir birleşik import/build + tek 3–5 dakikalık etkilenen mekanik smoke yapar. QA aynı kanıtı inceler. Somut hatada dar düzeltme/gerekli tekrar; geniş tur gün sonu veya somut yeni riskte. Alt ajan runtime/test suite çalıştırmaz.

Collision, maliyet/malzeme, çanta, süre/bunker, hasar/kayıp, ilerleme, kayıt ve performans koşullarını testi geçirmek için gevşetme. Gerçek dosya/hash, hedef yol, arayüz, kaynak/lisans, kısa fiş, kontrol kapsamı ve açık eksik teslim et. Hazırlanan/üretilen/indirilen/import edilen/entegre/native gözlenen/insan oynanışı/son görsel kabul sonuçlarını ayır. Resmi 48 saat başlangıç/teslim saatleri doğrulanmamışsa unknown bırak; görev veya paket saati bunları belirlemez.


İzometrik Home V4 güncel ek sözleşmesi Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/CONTRACT.md: görsel kalite/yerleşim/PC/gerçek stok ve üç aktif mini oyun ailesi. Son kullanıcı kararıyla şimdilik yalnız afet sesi; konuşma üretimi/AI radyo/ElevenLabs entegrasyonu en sona ertelenir. Eski kaynak sesini yeni pakette etkinleştirme; sağlayıcı harcaması yapma.


