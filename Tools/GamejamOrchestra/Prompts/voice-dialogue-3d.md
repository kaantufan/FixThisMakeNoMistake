# Güncel kullanıcı ses politikası — 16 Ekim 2026 / Home V4

**Şimdi konuşma/anlatım üretilmez veya oynatılmaz; yalnız afet sesleri etkindir.** Radyo anonsları ve diğer ortam/arayüz/işlem sesleri ertelenmiştir. Mevcut Tolga kayıtları izometrik projede `Assets/Game/Deferred/Radio-legacy/` altında Resources dışında tarihsel kaynak olarak korunur; Home V4 player dağıtımına eklenmez.

Seslendirme oyunun son aşamasında **ElevenLabs** ile ele alınacaktır. Şimdiki görev ElevenLabs veya başka bir sağlayıcı kullanımı, hesap/API işlemi, satın alma ya da kredi harcaması yetkisi vermez. Son aşamadaki ayrı görevde maliyet, resmi sağlayıcı bağlantısı ve voice/lisans koşulları gösterilir; ücretli işlemden önce kullanıcı onayı alınır. Bu politika önceki Home V3 Türkçe radyo üretim talebinin önüne geçer.

Aşağıdaki üretim şablonu yalnız konuşma işinin kullanıcı tarafından yeniden etkinleştirildiği son aşamada uygulanır. Şimdiki paket için yeni konuşma dosyası, konuşma importu veya native konuşma kabulü istenmez.

---

# Türkçe radyo anonsları — ertelenmiş üretim şablonu

Rol: standart `gpt-6.1-sol`. `common-3d.md`, `delivery-and-documentation.md` ve [ekip düzeni](../../../Docs/TOO-LATE-TO-PANIC-TEAM.md) ile çalış. Tema/üretim onaylıdır; Unity 6000.0.73f1 Built-In hedeflenir. Hedef varyantı task'tan al; Home V3 ve FPS yerel AGENTS/sözleşmeleri ayrı uygulanır. Alt ajan açma, canlı proje/Unity/import/build/player/gerçek kayıt işlemi yapma.

Atanmış haber/afet/sonrası/süre radyo metinlerini kısa, anlaşılır Türkçeyle gerçek konuşma dosyalarına hazırla. Girdi: satır/cue ID, tam metin, olay/faz/öncelik, speaker/locale, oynatma ve ağ/lisans koşulu. Onaysız metin genişletme; oyuncuya oyunda var olmayan eylem tavsiye etme.

Kullanıcı Home V3'te duyulur Türkçe radyo istemiştir. Yetkili ücretsiz/yerel ses yolu kullan; gizli API anahtarı, ücretli hizmet veya sistem/registry değişikliği yapma. Türkçe üretim engelliyse somut engel bildir; başka dili Türkçe teslim diye sunma. Doğal hız/telaffuz, baş/son sessizlik, clipping ve anlaşılabilirlik için tek kısa dinleme yap.

Gerçek WAV/uygun ses dosyası, transcript/cue eşlemesi, süre/sample rate, kaynak/sağlayıcı/voice lisansı, üretimde gereken ağ ile runtime offline oynatmayı ayır. Radyo aç/kapa, kesilme/öncelik/pause/cleanup ve asset-retention ihtiyacını Luna/root'a açıkla. Yüz/lipsync üretimi görevde yoksa not_applicable; ek bağımlılık kurma.

Teslim yalnız atanmış asset staging'de dosyalar/hash/byte, preview ve canonical delivery.json/HANDOFF.md. Gerçek oyunda duyulma root'un aynı paket smoke kanıtıdır; dosya üretimi native kabul değildir.

