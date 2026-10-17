# Too Late to Panic! — güncel eksik asset ve tamamlanma listesi

İnceleme: 17 Ekim 2026, 20:23 (Europe/Bucharest). Bu belge o andaki kaynak, teslim ve görünür geliştirme sohbetlerinin durumunu kaydeder. Otomatik güncellenmez. Canlı Assets, Unity, build, açık oyuncu ve kayıtlar değiştirilmedi.

Son teslim indeksi Home V4.13'ü gösteriyor. V4.14-r4 oynanış kaydı ve V4.14-r5 build başlangıç kaydı ayrıca mevcut; bunlar LATEST'in yerine geçmiş nihai teslim sayılmadı. R3 görselleri yalnız R3 görünümünü değerlendirmek için kullanıldı.

İncelenen oynanış kapsamında temel eşya/oda/karakter/çanta/araç/ikon kaynakları bulunuyor. Başlıca ihtiyaç yeni bir büyük asset paketi değil; afetlerin belirgin sunumu, mevcut paketlerin entegrasyon kabulü ve aşağıdaki sınırlı görsel/ses çeşitliliği. Prosedürel üretilmiş bir karşılık da gerçek mevcut assettir; tek başına FBX olmaması eksik sayılmadı.

## 1. Önce kapanması gereken ve geliştirmesi süren işler

| Öncelik | Asset / sunum ihtiyacı | Şu an bulunan karşılık | Açık kalan iş | Takip edilen çalışma |
|---|---|---|---|---|
| P0 | Dış çekimde açıkça sarsılan bina; cephe/çatı kenarı tozu ve döküntü | Deprem çatlak/toz/moloz sistemi, eşya tepkisi ve yeni salon yakın planı var. | Son kullanıcı isteği özellikle dışarıdan görünen binanın hareketsizliğine ilişkin. Salon planı tek başına bu isteği kapatmaz. Bina görsel tepkisi ve dış kadrajda döküntü görülmeli. | Ana sohbet “Düzelt afet akışı ve etkileşimler”; “V4.14 — Karakter ve sinematik”. Yeni dış bina isteği sonuçlanmış değil. |
| P0 | Evle aynı kadrajda belirgin hortum/girdap | `PanicStormArt` içinde dönen huni mesh'i, uçuşan parçalar ve yağmur mevcut; R3 karesinde ev arkasında ince bir huni görülüyor. | Görünür siluet, kontrast, ölçek ve kamera yerleşimi. Asset sıfırdan eksik değil; mevcut görüntü kullanıcı talebini karşılayacak kadar belirginleştirilmeli. | “V4.14 — Ev planı ve çevre” son isteği incelemeye başladı. Yeni paket/oyun içi kabul bekleniyor. |
| P1 | Sivil karakter görünümünün son küçük cilası | Çalışan eski humanoid/UAL klipleri ve sivil kıyafet uyarlaması mevcut. Ceket dikişi/ayakkabı detayı paketi hazır. | Hazır cilanın alınması, yeniden bake ve yakın görsel kontrol. Yeni model veya yürüyüş animasyonu siparişi gerekmez. | `CIVILIAN-POLISH-HANDOFF.md`: hazır staging; bu incelemede yeni cila için intake/native kanıtı görülmedi. |
| P1 | Yeni ses paketi ve doğru mekânsal miks | 14 yeni efekt + 2 müzik fiziksel olarak hazır. Root kaynak alımı kaydı var. | Ortak build ve oyun içinde dinleme: sakin fon, FPS gerilimi/nabız, doğru zemin, bunker filtresi, mesafeli motor/siren/rotor. | “ElevenLabs ile sesleri hazırlama”; `audio-intake-v5.json`, r5 build başlangıcı. Yeniden üretim kuyruğuna alınmaz. |
| P1 | Bunker/çanta/el temaslarının görünür tamamlanması | Yeni kapak/kilit, gerçek çanta görseli, atma/atlama/kapatma sırası, eklemli FPS elleri mevcut. | Son adayda el–nesne teması, çantanın kapaktan geçişi, baş/beden açıklığı ve yakın kareler. Bu bir model yokluğu değil, son sunum kabulüdür. | Karakter/sinematik ve ana entegrasyon çalışması. |

P0, mevcut açık kullanıcı isteğinin kapanış önceliğidir. Bu işler için ikinci bir üretim başlatmak, devam eden paketleri çoğaltır.

## 2. Ayrı asset üretimi yapılacaksa anlamlı kalan liste

Bu bölümdeki adetler üretim önerisidir; verilmiş sipariş veya kabul edilmiş yeni kapsam değildir. Mevcut oyun karşılıkları çalışmaya devam eder.

| Öncelik | Önerilen teslim | Gerçek mevcut durum / eksiklik | En küçük yararlı paket |
|---|---|---|---|
| P1 | Mesleği siluetten anlaşılan yardım ekibi kıyafetleri ve ekipmanları | Dört hizmet aracı mevcut. Personel aynı çalışan insan prefabının ceket renk varyantları; ayrı polis/sağlık/itfaiye/kurtarma kıyafet meshleri ve rol ekipmanları yok. | Aynı rig'e uygun 4 görünüm seti: polis; sağlık görevlisi; kask/koruyucu kıyafetli itfaiyeci; reflektörlü yelek/kasklı kurtarmacı. Yeni dört iskelet gerekmez. |
| P1 | Manuel işlere özgü kısa Foley | Vida için `screw.wav` var. Mevcut `Work(action)` eşlemesinde ayrı çekiç, gaz/su vanası ve elektrik şalteri ses kimlikleri yok; diğer işler genel ailelere düşüyor. Yeni 14 efekt bu üç özel işi kapsamıyor. | Çekiç darbesi, vana dönüşü/son dayama, şalter klik sesi: 3 aile. Mevcut metal/ahşap sesleriyle yeterli sonuç alınırsa yeni dosya üretmeden eşleme yapılabilir. Her ses gerçek girdiye bağlanmalı. |
| P2 | Afet sonrası yüzey hasarı için çeşitlilik | Prosedürel çatlak, yanık, çamur ve araç hasarı karşılıkları var. Bu, tamamen eksik hasar sistemi değildir. | İstenirse 3 küçük atlas/set: sıva/duvar çatlağı, kurum/yanık, su izi/çamur. Mevcut Built-In materyallerle; gerçek Model sonucuna bağlı kullanım. |
| P2 | Yardım ekiplerinin işi anlatan kısa hareketleri | Personelin mevcut dolaşımı yürüyüş/idle tabanlı. `PanicStreetLifeV414` yürüyen ekiplerde `Working=false` kullanıyor; role özgü müdahale klibi bu akışta yok. | İstenirse mevcut rig üzerinde 3 kozmetik hareket: telsizle haberleşme, eğilip kontrol, yönlendirme. Yeni kurtarma mekaniği veya görev eklemez. |
| P2 | Afet döküntüsünde daha tanınır parça çeşitleri | Yerel moloz ve uçuşan parça meshleri zaten var. | P0 düzeltmesi mevcut parçalarla okunur olmazsa küçük çatı/sıva/tuğla/tahta parça seti. Hasar/fizik kurallarını değiştiren bina yıkım paketi gerekmez. |

Yeni asset üretimine ayrılacak en küçük anlamlı iş listesi: **4 personel görünüm seti ve 3 işe özgü ses ailesi**. Önce devam eden P0 sunum düzeltmeleri bitirilir; P2 çeşitlilikleri sonraya bırakılabilir. Bunların hiçbiri mevcut temel döngüyü durduran eksik dosya olarak raporlanmıyor.

## 3. Yeniden eksik diye yazılmaması gerekenler

| Grup | Mevcut karşılık |
|---|---|
| Afet çantası ve erzak | Fiziksel çanta, su, konserve/elma/bar gıda varyantları, hazır ilk yardım, fener, pil, radyo ve battaniye kaynakları var. |
| Önlem/montaj | Vida, L braket, tahta, kum torbası, fidan, hortum, yangın tüpü/istasyonu, iş aletleri ve vana/şalter görselleri var. |
| Ev ve çevre | Salon, mutfak, WC, kiler, iki yatak odası/çalışma/banyo/balkon, bodrum, garaj, merdiven, köprü/dere, komşu evler ve ağaç kaynakları var. Son yerleşim/erişim kabulü ayrıca izlenir. |
| Ana karakter | Çalışan eski humanoid ve UAL Idle/Walk/Jog/Jump/Interact/Fixing klipleri mevcut. Reddedilen Meshy procedural yürüyüşünü yeniden üretim hedefi yapma. |
| FPS elleri | Yerel eklemli el/kol, toplama/açma ve hareket tepkisi var. Yakın görünüm kalitesi ayrı konudur. |
| Bunker | Yarkın kapak/kilit kaynağı ve yerel geçiş halkası var; ayrı yeni bunker modeli zorunlu değil. |
| Araçlar | Sivil otomobil, polis/ambulans/itfaiye/kurtarma araçları ve kurtarma helikopteri var. Yeni siren/rotor/motor sesleri de hazır. |
| HUD/marka | İkonlar, çevrimdışı fontlar, gerçek verili defter, logo ve Windows EXE ikonları var. |
| Ses | Dört afet, adımlar, kapılar, pickup/UI, ortam ve iş seslerinin mevcut kütüphanesi var. Yeni 14 efekt/2 müzik paketini bunların yokluğu gibi listeleme. |

Düdük, enerji içeceği, ayrı telsiz/eşya mekanikleri, zincirleme afet, eski bağımsız FPS kampanyası ve genel seslendirme bu listeyle yeni zorunlu kapsam yapılmaz. Son ses kararı bip/altyazıdır.

## 4. Asset dışındaki açıklar

- Fırtınanın normal Serbest Oyna rastgele havuzunda bulunmaması model/denge konusudur. Yeni hortum modeli bu eksikliği çözmez. 50 dalga çalışması bunu ayrıca raporluyor.
- Son yeni seslerin dinleme kabulü ve karakter/afetlerin kullanıcı görsel kabulü henüz bu incelemeyle verilmedi. Kaynak veya derleme başarısı bunların yerine geçmez.
- Korunmuş Meshy kaynakları ve yeni bunker kaynağında kamuya yeniden dağıtım hakları asset notlarında `UNVERIFIED`. Bu, yeni model ihtiyacından ayrı bir kaynak/lisans kaydı açığıdır; yeni sağlayıcı işi başlatılmadı.

## 5. Kullanılan güncel kanıtlar

- [Son teslim indeksi](../TooLateToPanic/LATEST-BUILD.json)
- [V4.14 sözleşmesi ve son kararlar](../Evidence/GamejamOrchestra/TLTP-OVERHAUL-V414-20261017/CONTRACT.md)
- [V4.14 asset/provenans notları](../TooLateToPanic/Documentation/HOME-V4.14-ASSETS-2026-10-17.md)
- [Ses paketi](../Evidence/GamejamOrchestra/TLTP-AUDIO-REVISION-V414-20261017/HANDOFF.md), [kaynak alımı](../Evidence/GamejamOrchestra/TLTP-OVERHAUL-V414-20261017/manager/audio-intake-v5.json)
- [Karakter cila paketi](../Evidence/GamejamOrchestra/TLTP-OVERHAUL-V414-20261017/sessions/character-cinema/CIVILIAN-POLISH-HANDOFF.md)
- [Deprem yakın plan paketi](../Evidence/GamejamOrchestra/TLTP-OVERHAUL-V414-20261017/sessions/character-cinema/quake-readability/HANDOFF.md), [kaynak alımı](../Evidence/GamejamOrchestra/TLTP-OVERHAUL-V414-20261017/manager/quake-intake-v5.json)
- [R5 build başlangıcı](../Evidence/GamejamOrchestra/TLTP-OVERHAUL-V414-20261017/manager/build-v5-process.json)
- Kaynak incelemesi: `PanicDisasterArt`, `PanicStormArt`, `PanicStreetLifeArtV414`, `PanicStreetLifeV414`, `PanicBagHandsArt`, `PanicCuratedAssetArt`, `PanicResidenceArt`, `PanicAtmosphereArt`, `PanicWorkArt`, `PanicAudioBank`, `PanicHomeAudio`, `PanicQuakeReadabilityV414`.
- Salt okunur R3 görüntü incelemesi: `05-ground-room-overview.png`, `15-fps-kitchen-hands.png`, `20-disaster-storm.png`, `23-street-emergency-response.png`; `manager/native-v3-22e47912-8536-4604-b994-4e6633a709a4/` altında. Bunlar yeni r5 veya sonraki dış bina/hortum düzeltmesinin kabulü değildir.
- Görünür sohbetlerin son kullanıcı istekleri ve teslim açıklamaları okundu: “Düzelt afet akışı ve etkileşimler”, “V4.14 — Ev planı ve çevre”, “V4.14 — Karakter ve sinematik”, “V4.14 — Mekanikler ve QA”, “ElevenLabs ile sesleri hazırlama”, “Serbest Oyna — 50 dalga sırası ve dayanıklılık…”. Diğer sohbetlere mesaj gönderilmedi.
