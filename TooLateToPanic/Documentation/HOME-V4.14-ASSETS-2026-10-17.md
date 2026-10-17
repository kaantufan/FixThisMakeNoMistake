# Home V4.14 — etkin assetler ve kaynakları

Güncel derleme ve test kimliği [LATEST-BUILD.json](../LATEST-BUILD.json) ve [teslim kaydı](HOME-V4.14-DELIVERY-2026-10-17.md) içindedir. Kaynak intakesi, Unity derlemesi, native görüntü/oynanış ve insan kabulü ayrı kanıtlardır. Bu belge kaynak/provenance kaydıdır.

## Karakter ve animasyon

Ana karakter, NPC ve kurtarma gövdeleri mevcut Quaternius `Superhero_Male_FullBody.fbx` humanoid Avatar ve UAL1 Standard Idle/Walk/Jog/Jump/Interact/Fixing kliplerini kullanır. Normal hareketin tek yazarı mevcut MovePlayer/SetMoveAnimation ve Animator'dır. İkinci LateUpdate Speed/Airborne yazımı kaldırıldı. Kullanıcının istediği sivil görünüm aynı çalışan kemiklerle koyu yeşil açık ceket, açık tişört, koyu pantolon, günlük kahverengi ayakkabı, çıplak eller ve kısa saç/sakal olarak yerel uyarlandı. Eski karakter fiziği ve ağırlıkları korunur.

[defter-mockup 00377265](https://github.com/kaantufan/FixThisMakeNoMistake/commit/00377265c740fdc973039d0266cefc8c913633b9) içindeki Meshy FBX/dokular görünüm referansıdır. Bu kaynak rig/klip içermiyordu. Kullanıcının reddettiği yerel 19 kemikli deneme aktif oyuncu/NPC değildir; kaynak dosyaları ve eski aday kanıtları korunur. FBX SHA256: `56C18BF9071C0694B54C669DA092B14F02DE11D268146FD576D8660EA10F37F1`. Quaternius CC0 metinleri mevcut kaynaklar için geçerlidir; Meshy arşivinin kamuya yeniden dağıtım lisansı ayrıca doğrulanmış değildir.

FPS el/kol görünümü ayrı yerel prosedürel mesh ve eklemlerdir. Parmak MCP/PIP/DIP zincirleri, toplama/tutuş, yürüme/koşma/zıplama/iniş tepkileri gerçek motor ve iş durumuna bağlıdır. Yeni üçüncü şahıs Meshy assetinin elleri olarak kredilendirilmez. Anatomi ve rahatlık konusunda insan kabulü teknik test sayılarıyla ilan edilmez.

## Sığınak ve ev

[yarkin-assets 29bf309](https://github.com/kaantufan/FixThisMakeNoMistake/commit/29bf309fa0c7ccfdafd0a935e93512a44f550c9e) kaynaklı gerçek `SM_BunkerHatch.fbx` kapak/menteşe/kilit görselleri mevcut açık şaft ve fizik üzerine uyarlandı. FBX SHA256: `1DCAE78A93D6B0AAE490AAE54FF943B6082C7D10A6494C42AC59FCCA264C1C07`. Kaynak controller/cutscene/package/shader sistemleri topluca alınmadı. Built-In Standard materyaller ve ayrı V414 GUID'leri kullanılır. Sağlanan beyan `Resources/Licenses/Yarkin-V414-SuppliedDeclaration.txt` içinde korunur.

Kapak açma, gerçek çanta görselini atma, açık şafttan atlama ve baş temizlendikten sonra kapağı kapatma mevcut üretim sinematiğine bağlıdır. Geçici çanta ve açık geçiş halkası yerel uyarlamadır; shared kaynak mesh/materyaller yok edilmez.

İki yatak odası, çalışma odası, banyo/hol/balkon, salon/mutfak/WC/kiler/garaj ve bodrum mevcut ev fabrikalarının yerel düzenlemesidir. Gerçek geçişler, merdiven ve köprü korkulukları root tarafından fizik katmanında doğrulanır. Bunlar dışarıdan alınmış yeni bir ev paketi değildir.

## Ses ve efektler

Kullanıcının ayrı ses sohbetindeki açık yetkisiyle ElevenLabs üzerinde 14 efekt + 2 müzik üretildi: **5003 mevcut kredi, yeni ödeme yok**. Konuşma yerine bip ve altyazı kullanılır. Ses manifesti, ham dosyalar, üretim kayıtları ve provenance `Evidence/GamejamOrchestra/TLTP-AUDIO-REVISION-V414-20261017/` altında; kaynak metinler `Resources/Audio/Foley/REVISION-V414-PROVENANCE.md` ve `Resources/Audio/Music/REVISION-V414-PROVENANCE.md` içindedir. Bunlar CC0 diye yeniden etiketlenmez. Önceki yerel heartbeat üretimi tarihsel fallback'tir; etkin nabız imported klibi kullanır. Düşük nefes fallback'i yerel sayısal sentezdir. Yeni kök oturum sağlayıcı çağrısı yapmadı.

Motor/siren/rotor gerçek oyuncu mesafesine göre azalır; sığınak içeride filtre uygular. Adım yüzeyi gerçek cached prefab mesh/materyallerinden 11 cm temas alanıyla seçilir; 6 mm derzlerde alttaki betonun kaplamayı bastırması giderildi. PCM/AudioSource veya zemin sorgusu kontrolü insan dinleme ve miks kabulü değildir.

Deprem dış cephe sarsıntısı, toz/kırıntı; sel akış/köpük/yüzen parçalar; yangın alev/duman; fırtına hortum/bulut/yer tozu/dönen enkaz yerel kod/mesh/particle çalışmalarıdır. Dış VFX paketi satın alınmadı. Depremde gerçek ev collider ve Model dönüşümleri korunur; geçici render kopyaları tek pivotta birlikte hareket eder ve pause/skip/reset ile temizlenir. Hortumun gerçek kamera görünürlüğü native karelerle ayrıca değerlendirilir.

Malzeme kazanımında +adet ve görsel, gerçek `LastGatherRewards` stok farkından ve aynı dünya asset fabrikasının renderından gelir. Yeni ödül veya tahmini adet üretilmez. Seçim vurgusu asset yüzeyi/silueti üzerindedir; boş alana zemin kutusu eklenmez.

Sanat/entegrasyon staging paketleri ve ilk aday ayrıntıları `Evidence/GamejamOrchestra/TLTP-OVERHAUL-V414-20261017/` altında hashleriyle korunur. Tarihsel adayların teknik geçişi son sürüm veya kullanıcı kabulü gibi yeniden etiketlenmez. Bu belge kamuya yayın yapılmış olduğunu söylemez.
