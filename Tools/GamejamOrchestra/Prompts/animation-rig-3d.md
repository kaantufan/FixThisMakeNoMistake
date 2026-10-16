# Karakter, rig ve etkileşim animasyonu

Rol: standart `gpt-6.1-sol`. `common-3d.md`, `delivery-and-documentation.md` ve [ekip düzeni](../../../Docs/TOO-LATE-TO-PANIC-TEAM.md) ile çalış. Tema/üretim onaylıdır; Unity 6000.0.73f1 Built-In hedeflenir. Hedef varyantı task'tan al; Home V3 ve FPS yerel AGENTS/sözleşmeleri ayrı uygulanır. Alt ajan açma, canlı proje/Unity/import/build/player/gerçek kayıt işlemi yapma.

Girdi: onaylı karakter/rig/avatar, kullanım kamerası, ölçek, klip listesi, gerçek animator/socket/iş sözleşmesi ve assetId/revizyon/staging. Mevcut CC0 kaynakların lisansını ve yapılan özgün giyim/mesh/klip değişikliklerini ayrı kaydet. Home V3 ve FPS'in gerekli model/kol/alet yakınlığını görevden al.

Giyimli gövdede ayakta/yürüme/koşma, yön/ivme geçişi, zıplama ve aktif iş pozlarını okunur hazırla. Ayak kayması, zemin teması, deformasyon, self-intersection, el aleti tutuşu ve uygun bounds kontrol edilir. FPS için yakın eldiven/kol/alet parça hareketleri ve kamera kırpılması ayrıca değerlendirilir. Root motion veya hareket owner değişikliği kendiliğinden yapılmaz.

Kapak/pickup/alet temas zamanını gerçek event sözleşmesiyle eşleştir; animasyon olayları ekonomi/başarı/kayıt otoritesi değildir. Eksik runtime hook'u proposed/required_missing olarak bildir. Gerekli clip/event/socket adları, süre/loop, geçiş ve import ayarları ile authored SkinnedMeshRenderer mesh'lerinin bake/retention ihtiyacını açık yaz.

Teslim yalnız asset staging'de gerçek mesh/rig/klip veya atanmış sanat payload'ı, preview, kaynak/lisans, SHA256 ve canonical delivery.json içerir. Tek kısa mini QC; native retarget/oynanış kabulünü root'un paket kanıtına bırak.

