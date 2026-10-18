# Home V5.9 — ses ve imleç kaynakları

18 Ekim2026 doğrudan kullanıcı talebiyle ElevenLabs mevcut hesabında11ses ailesi üretildi. Dört varyantın1numaralı gerçek WAV dosyaları indirildi; özgün kaynaklar ve SHA256 kayıtları `Evidence/GamejamOrchestra/TLTP-MENU-ECONOMY-AUDIO-V59-20261018/manager/provider` altında korunur. Mevcut kredi233475→233018, toplam457. Yeni abonelik, ödeme veya ücretli aşım yoktur. İnsan konuşması/TTS veya ses klonu üretilmedi.

Yeni kaynaklar: kısa retro bit, çekiç darbesi, gaz vanası, su vanası, ana şalter, küçük gaz parlaması, boru kaçağı, elektrik arızası, bot motoru, botun su sesi ve nefes.21mono44.1kHz PCM16 çalışma dosyası vardır:19V56geçici yerel ses dosyası aynıUnityGUID'leriyle güncellendi; `Audio/V59/dialogue_bit`0.110saniye ve `Audio/V59/panting`4.88saniye eklendi. Vana/şalter tutuş ve iptallerinde uygun mevcut ElevenLabs kumaş/metal kaydı kısa düzenlendi; iptal başarı sesi üretmez. Kaynaklar, düzenlemeler ve farklı dosya hashleri ayrı izlenir.

Önceden ElevenLabs kaynaklı54Foley, üç müzik kaydı ve ilgili lisans/provenance belgeleri korunur. Yeni müzik üretimi yapılmadı. Mevcut `preparation.ogg`, `fps_tension.ogg`, `disaster.ogg` tek müzik sahibinde kullanılır; eski FPS prosedürel skor sahibi ikinci bir fon başlatmaz.2saniye geçiş, düşük geçiren filtre ve sınırlı kaynak seviyeleri kullanılır. Normal fon ölçülü; FPS fonu, nabız ve nefes daha belirgindir. Mevcut master/müzik/efekt tercihleri, sessiz, pause/focus ve azaltılmış etki ayarı korunur.

Bit ve altyazı:132karakter sınırı, iki satır, en fazla üç kısa bit; mevcut Barlow fontu ve koyu/sarı HUD çerçevesi. Girdi tutmaz, oyunu durdurmaz ve fizik/RNG/bütçe/etkileşim seçimi değiştirmez. Modal ekranlarda görünmez/ses durur; yeni koşuda eski metin taşınmaz. Elle işaretlenen defter hedefinin imleci özgün C#HUD çizimidir; yeni shader/font/model/lisans yoktur. Gerçek nesne sınırlarına ve katına bağlıdır; uzaktan iş veya otomatik yürüyüş sağlamaz.

PCM/hash/import kontrolü, gerçek hoparlör çıkışı, oyun içi miks ve insan dinleme kabulü farklıdır. Güncel Editor/build/native sınırları ana teslim belgesi ve LATEST kaydında açıkça belirtilir. Bu kaynak kaydı kendi başına işitsel kalite veya kulak yorgunluğu kabulü değildir. Yeni bir lisans yaratmaz; sağlayıcının mevcut hesap ve üretim koşulları geçerlidir.

Kesin üretim/işleme kayıtları: `manager/provider/audio-production-receipt.json`, `panting-addendum.json`, `provider-final-session.json`, `PROVENANCE-V59.md`. Önceki yerel19dosyanın baytları `manager/baseline-v58` içinde korunur.
