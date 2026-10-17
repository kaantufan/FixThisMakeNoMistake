# V4.14 asset teslim / credits notları

Bu not root'un README/credits/teslim matrisine alması içindir. Kaynak intakesi, yeni build'e bake edilmesi, native render/oynanış ve insan kabulü ayrı durumlardır. Bu worker Unity/player/native açmadı. Son native kabul ve güncel DLL/ZIP bağlantısını yalnız root LATEST/teslim kaydında doldurur.

## Değişmez dış kaynak kimlikleri

| Kaynak | Doğrulanan commit | Bu paketin gerçek intakesi |
|---|---|---|
| [defter-mockup](https://github.com/kaantufan/FixThisMakeNoMistake/commit/00377265c740fdc973039d0266cefc8c913633b9) | `00377265c740fdc973039d0266cefc8c913633b9` | `Meshy_AI_survival_protagonist_1017123545_image-to-3d-texture_fbx/` içindeki gerçek FBX ve seçili harici dokular |
| [yarkin-assets](https://github.com/kaantufan/FixThisMakeNoMistake/commit/29bf309fa0c7ccfdafd0a935e93512a44f550c9e) | `29bf309fa0c7ccfdafd0a935e93512a44f550c9e` | `_YarkinAssets/Art/Meshes/SM_BunkerHatch.fbx` ve gereken materyal/atlas bağımlılık kapanımı |

Branch adının daha sonra ilerlemesi bu teslimin kaynak kimliğini değiştirmez. Bağımsız eski FPS checkout/kayıtları kaynak havuzu yapılmadı. Başka branch'in script/package/oyun sistemi topluca alınmadı. Diğer retained CuratedYarkin eşyalarının tümünü bu yeni bunker commit'inden yeniden alınmış gibi etiketleme.

## Gerçek karakter mesh/doku ve rig kaydı

- Asıl yükleme Meshy adını taşıyan FBX7400 dosyasıdır:19,584,135byte,1mesh geometry,3,257 kaynak kontrol noktası,1materyal,5Texture/5Video kaydı. Kaynakta deformer/iskelet/animation stack **yoktur**. Unity'nin UV/normal köşe bölmesi sonrası vertex sayısı kaynak kontrol noktası sayısından farklı olabilir.
- Live kaynak kopyası `Assets/Game/Resources/CuratedCharacter/DefterSurvivor.fbx`; SHA256 `56C18BF9071C0694B54C669DA092B14F02DE11D268146FD576D8660EA10F37F1`.
- Gerçek harici BaseColor, Normal ve Roughness PNG'leri byte korunarak kopyalandı. `DefterSurvivor_BaseColor.png` materyale bağlı; sRGB. `DefterSurvivor_Normal.png` normal-map/linear; materyale bağlı ve yeni mesh'in tangent'i yerel hesaplanır. Roughness linear kaynak arşividir, şu an Standard materyale bağlı değildir; smoothness=.18 kullanılır. Emission/metallic harici dosyaları staged özgün checkout'ta kalır, runtime materyal bunları kullanmaz. FBX'in kendi beş gömülü Video Content payloadı da byte-identical FBX içinde korunur; bunları harici PNG dosyalarının birebir kopyası olarak tanımlama.
- Importer kaynak ayarı max2048, compression/mipmap açık; BaseColor sRGB, Normal/Roughness linear. Gerçek platform formatı/bellek boyutu root'un Unity import kanıtıyla yazılmalıdır. Kaynak FBX bake sırasında readable'dır; hiçbir provider rigging işi/ücret/kredi kullanımı yoktur.
- Yerel uyarlama:1.80m taban-merkez fit,19adlı deform kemiği,vertex başına normalize iki etki,özgün geometri/UV ve kaynak dokuları. Idle/walk/run/jump/land/work/gather/throw/jump-to-shelter hareketleri mevcut authoritative motor/iş fazından sürülen yerel prosedürel rig/IK çalışmasıdır. **Meshy'den hazır rig/klip veya Quaternius'tan aktarılmış yeni animasyon klibi değildir.**
- `Player/SurvivorV2.prefab` yolu yalnız mevcut runtime uyumluluk kimliğidir; yeni baked içerik CuratedDefterSurvivor_V414 olmalıdır. Yol adından V2/Quaternius/CC0 kaynak sonucu çıkarma. Root builder intakesi ve yeni prefab/native kare doğrulaması tamamlanmadan yeni karakter oyuncuda kabul edildi yazma.

## Bunker ve FPS el kaydı

- Gerçek `SM_BunkerHatch.fbx` SHA256 `1DCAE78A93D6B0AAE490AAE54FF943B6082C7D10A6494C42AC59FCCA264C1C07`. Kaynaktaki HatchLid_Hinge/LockWheel meshleri mevcut kuzey DoorLeaf altında kullanılır. Eski açık çerçeve/şaft/fizik/etkileşim korunur; upstream Base/Portal/DeepShaft renderleri ve eski lid renderı kalıcı gizlidir. Upstream cutscene/controller/item evaluator/shader/physics scriptleri taşınmaz. Materyaller Built-In Standard'a uyarlanmış,GUID'ler V414 namespace'ine yeniden eşlenmiştir.
- Kare çerçeveden dairesel kapağa yerel açık geçiş ring'i özgün kod/mesh uyarlamasıdır; merkezde yüzey/collider yoktur. Runtime Mesh ve yalnız fallback Material açık instance sahibince bırakılır; shared FBX/Resources materyali sahiplenilmez.
- FPS el/kol modeli dış sağlayıcı veya branch'ten indirilmiş bir el asset'i değildir. Mevcut yerel el çalışmasının V4.14 uyarlamasıdır:iki tarafta el/palm ve parmak başına MCP/PIP/DIP zinciri; palmiye/finger taper,omuz-dirsek-bilek oranı,toplama/kapak tutuşu,yürüme/koşma/zıplama/iniş tepkisi yerel prosedürel mesh/eklem hareketidir. Third-person Meshy kaynak el/kolu olarak kredilendirme. El sistemi kamerayı veya gameplay state'i sahiplenmez.

## Yerel ses ve dinamik efekt authorship

- Yeni heartbeat/breath:tamamen yerel sayısal sentez;22,050Hz mono,1s çift-atım döngüsü ve3s nonverbal filtreli gürültü nefesi. İnsandan kayıt,AI seslendirme/provider çağrısı,voice clone veya yeni ElevenLabs işi yoktur. Önceki yetkili ses kayıtları/lisansları kendi retained kaydıyla ayrı korunur. Yeni ses denklemlerinde source-math finite/no-clipping kontrolü vardır; bu duyulabilirlik/mix/insan dinleme kabulü değildir.
- FPS radial veil,iki eksende≤.20derece aim sway,.10s damping ve1→.94 kontrol çarpanları yerel kod uyarlamasıdır. HUD metinlerinin arkasında çizilir; global kamera tek root'tadır. Native okunurluk/rahatlık henüz ayrı kabul ister.
- Gerçek malzeme kazanım kartları yerel presentation kodudur. İsim/adet gerçek GatherSequence/LastGatherRewards.Id/Required kaydından gelir; ikon yerine aynı gerçek world pickup mesh factory'sinin bounded128px renderı kullanılır. Shared asset provenance değişmez; yeni eşya veya tahmini miktar üretilmez.
- `hazards-street` paketinin yeni deprem impulse/toz/moloz,directional sel/foam/eddy,mesh flame tongues,storm funnel/orbital debris/rain ve procedural service-vehicle aftermath çalışmaları **özgün yerel/prosedürel kaynak uyarlamalarıdır**; yeni dış VFX pack/provider indirilmemiştir. Bu ayrı sanat worker'ının source/detached kanıtı native kabul yerine geçmez. Ana Model hasar/önlem kararı ve fizik bu görsel koddan bağımsız root/model tarafındadır.
- Civilian car geometrisi retained PanicResidenceArt fabrikası; yeni servis aracı varyantı/ışık barı/hasar overlay'i yerel çalışmadır. NPC/responder mevcut `Player/SurvivorV2` yolunu klonlar. Yeni prefab Meshy olduğunda onların gövde/doku provenance'ı da Meshy+yerel19bone olur. Hazards-street handoff'undaki eski **"retained CC0 SurvivorV2 rigs/clips" cümlesi bu yeni prefab için geçersizdir**; README'ye taşınmamalıdır.

## İstek / root bağlantısı / artist sınırı matrisi

| İstek | Gerçek asset/presentation yolu | Root bağlantısı ve kalan kabul |
|---|---|---|
| Ana karakterin yeni branch modeliyle değişmesi | CuratedCharacter FBX+PNG; CreateCuratedCharacter;19bone rig | BakePlayer→uyumluluk prefabı; yeni source mesh identity/native ön-arka yüz ve ayak yüksekliği gerekir |
| Yürüme/koşma/zıplama/iniş/iş/toplama | SetMotion→SetTaskPose; gerçek motor/Work.Progress | Root LateUpdate; kaynak birim metres/sn,placeholder humanoid klip sayısı kabul ölçütü değildir |
| FPS doğal eklemli el/kol | PanicBagHandsArt.Create/SetLocomotion/Animate | Aynı player/controller/Model; anatomi/eklem kırılması/temas native yakın kare gerekir |
| Yeni sığınak kapağı ve çanta-atla-kapat | Actual sourced lid/wheel+open ring; gerçek çanta render kopyası | Root Story/Cinema/route/hinge;0–1yaklaşım,1–1.65açma,1.65–2.5atma,2.5–4.05atlama,3.8kapanış; native görünür temas kabulü ayrı |
| FPS baskı ve hafif zorlaşma | Yerel veil/sway/synth heartbeat/breath | Ortak ApplyBagPose;root saat/pause/UI;kulakla mix ve keyif/rahatlık ölçülmedi |
| Gerçek toplama geri bildirimi | Reward kayıtları+actual material RenderTexture | Tick-only event/adet;OnGUI draw-only;native text/mesh görünümü gerekir |
| Afetlerin dinamik görünmesi | Yerel DisasterArt/StormArt factory'leri | Root hazard zaman/fazı/camera/foliage hooks;gerçek ev açıklıklarıyla native kare gerekir |
| Dış dünya trafiği ve yardım ekipleri | Retained car+yerel service variants;NPC prefab klonları | StreetLife ve mevcut ExteriorLife ayrı;her kopya yeni rig SetMotion almalı;araç/ekip gelişinin native zaman kanıtı gerekir |
| Ev/çocuk odası/mutfak/merdiven yerleşimi | Ayrı layout/house factory paketi | Bu not onların teslim hashlerini veya native kabulünü sahiplenmez;root layout matrisiyle birleştirir |
| Gerçek tesisat/manuel iş etkisi | Ayrı mechanics Model/Expedition/work paketi | Sanat root API'ye bağlanır;malzeme/bütçe/iptal/hasar/score etkisinin kabulü mechanics+root'tadır |
| Hikâye,skip ve sonucu tek çözme | Root Story/Cinema/Model | Asset katmanı sonucu değiştirmez;clock/controller/skip/model testleri root kaydıdır |

## Kısa artist sınırlamaları ve yayın lisansı

1. Yeni karakterin yüklenmiş Meshy klasöründe bağımsız lisans grant'i veya generation/provider hesabı receipt'i yoktur. Kullanıcı bu kaynağın oyun içi intakesini açıkça istedi; bu, kamuya dağıtım lisansının doğrulandığı anlamına gelmez. **Yeni Meshy karakteri,harici dokuları ve aynı gövdeyi kullanan NPC/responder kopyaları için yayın/yeniden dağıtım lisansı: UNVERIFIED. CC0 etiketi konmaz.** Eski Quaternius lisansları yalnız gerçekten retained Quaternius dosyaları içindir.
2. Yarkin'in verilen `ASSET_MANIFEST_AND_LICENSES.md` beyanı jam sırasında prosedürel/AI destekli üretim ve paylaşım kısıtı olmadığını bildirir. Özgün beyan `Resources/Licenses/Yarkin-V414-SuppliedDeclaration.txt` olarak korunur; bu bağımsız hak incelemesi veya tüm repo için genel lisans grant'i diye genişletilmez.
3. Yerel19bone skin/prosedürel hareket source-math ve detached C# doğrulamasına sahiptir; facial rig/mocap/humanoid retarget üretildi iddiası yoktur. FPS el yakın planı,skin deformasyonu,temas,baş/kapak açıklığı ve kıyafet örtüşmesi native kare bekler. NPC'lerin aynı kaynak texture'ı paylaşması özgün ayrı üniforma/karakter modeli teslim edildi demek değildir.
4. Kaynak matematiğinde3.8s kapanış başlangıcında muhafazakâr baş-ring dikey açıklığı≈.481m. Bu native collision/render kabulü değildir.3.35–3.42s kısa kol/el-ring geçişi ve yaklaşım yürüyüş hızı native karede izlenecek sınırdır.
5. Ücret/provider işi/paket veya renderer göçü yoktur. ROOT yeni build/DLL/ZIP ve native kanıtı eklemeden bu not yayınlanmış/oynanarak kabul edilmiş teslim sayılmaz.

README/credits'e alınabilecek kısa kaynak cümlesi:

> Ana karakter görseli kullanıcı tarafından verilen FixThisMakeNoMistake/defter-mockup00377265 Meshy yüklemesinden; yerel19bone rig,prosedürel hareket,FPS elleri ve adaptörler bu oyun için AI destekli yerel kaynak çalışmasıdır. Sığınak kapağı yarkin-assets29bf309 kaynağından uyarlanmıştır. Yeni heartbeat/nefes ve dinamik görsel efektler yerel prosedürel üretimdir. Yeni Meshy kaynaklarının kamuya dağıtım lisansı henüz doğrulanmamıştır; eski Quaternius CC0 lisansı bu modelin lisansı değildir.
