# Too Late to Panic! — Home V4 teslimi

16 Ekim 2026 · Unity 6000.0.73f1 Built-In · çevrimdışı Windows. Kullanıcının ekran görüntülerine göre görsel kalite, yerleşim ve aktif etkileşim paketi. Güncel build kimliği ve doğrulama LATEST-BUILD.json içindedir. **Son Windows build başarılı: 0 hata / 0 uyarı. Son DLL ile kısa native kontrol 12/12 PASS, 0 çalışma hatası, 36,01 saniye, yaklaşık 55,73 FPS.**

## Görüntülerdeki sorunlar ve sonuç

| İstek | Yapılan değişiklik |
| --- | --- |
| Banyo yerleşimi | Zemindeki ilaç dolabı duvara alındı; giriş ve orta yürüme alanı açıldı. Lavabo, klozet, küvet, duş rayı, dar paspas ve küçük malzemeler düzenlendi. Oda collider'ları korundu. |
| Halı | Yatak duvara yaklaştırıldı. Halı yataktan ayrılıp ayak ucuna hizalandı; yaklaşık 2,31×1,27 metreye küçültüldü. |
| Renksiz eşyalar | Eski genel paletin yeni malzemeleri griye çevirmesi kaldırıldı. Unity bake dosyaları semantik malzeme adlarını korur. Kitaplık, kitaplar, koltuk/yatak, mutfak/depo dolapları ve TV konsolu ayrı renk/malzeme detaylarını korur. |
| Bilgisayar | Daha büyük ekran, manzara/belge görünümü, açık renk klavye ve tuşlar, fare, lacivert kasa ve düzenli masa yeniden üretildi. Masa yüksekliğiyle bilgisayarın oturduğu yüzey eşleştirildi. Ekran ışığı kasadan ayrı açılıp kapanır. |
| Asset detayları | Ahşap çıtalar, raf içeriği, renkli kitap/arşiv kutuları, dolap kapakları, çekmece, kulplar, PC çevre birimleri ve alet kutusu iç tepsisi eklendi/iyileştirildi. Yeni insan modeli veya dış sağlayıcı üretimi yoktur. |
| Alet kutusu / görseller | Mercan renkli kapaklı kutu; vida, ankraj ve tahta için okunur ikonlar. HUD yalnız gerçek dolu yuvaları büyük ikon/ad/adetle gösterir. Tüm malzeme alınınca tek “Boş / Malzemeler alındı” görünümü gelir; hayali stok veya otomatik doldurma yoktur. |

E açık kapak/kapıyı tekrar kapatır; F seçili gerçek yığını alır; Z/X malzemeyi seçer. Duvardan veya ilgisiz eşyadan geçerek etkileşim engellenir. E düz mobilyada kendiliğinden sabitleme başlatmaz; 1/2/3/4 ayrı sabitle/koru/güçlendir/taşı eylemleridir. Çalışma masasının çekmecesi gerçekten dışarı kayar. Seçilen nesnenin yerde görsel işareti vardır.

## Üç aktif iş ailesi

| Aile | İşler | Oyuncu girdisi |
| --- | --- | --- |
| Vidalama | Duvar/zemin ankrajı, güçlendirme, çit | A/D hizala, Enter oturt; işaretlenen A ve D yönlerini sırayla uygula, Space ile bağlantıyı bitir. |
| Zamanlama | Fidan, drenaj, sel bariyeri | A/D hizala, Enter yerleştir; hareketli gösterge yeşil alandayken Space'e bas. |
| Basınç dengesi | Hortum, yangın ekipmanı | A/D ile hedef basıncı takip et, Space ile uygula. Pasif şekilde Space tutmak tek başına bitirmez. |

Dört kısa adımın adı, araç hareketi ve geri bildirimi işe göre değişir. Vida dönüşü/oturma, kazma-fidan-sulama ve vana/akış/ibresi gerçek 3D iş görünümüne bağlıdır. Başarılı tamamlamada malzeme/bütçe bir kez düşer; iptal tüketmez. Hazırlık saati devam eder. 12 envanter/8 çanta ve afet/hasar/bütçe/kayıp/puan kuralları korunur.

## Ses kararı

Şimdilik yalnız deprem, sel ve yangın sesleri etkin. Genel AI/radyo anlatımı, adım/UI/oda/rotor sesleri kapatıldı. Eski Tolga WAV'ları Assets/Game/Deferred/Radio-legacy kaynağında korunur; Resources dışındadır ve yeni pakete girmez. ElevenLabs seslendirmesi son aşamaya bırakıldı; bu pakette servis çağrısı, ses üretimi, abonelik veya kredi harcaması yapılmadı. Görsel radyo/helikopter/şömine yerinde kalır.

## Kanıt ve sınırlar

- Güncel sahne: Assets/Game/Scenes/TooLateToPanicResidence.unity; giriş PanicHomeWorld; builder PanicResidenceBuild.Build; ürün 0.4.0.
- Build: Builds/Home-V4-20261016/TooLateToPanic.exe; tek portable ZIP aynı Builds alanında TooLateToPanic-Home-V4-20261016-portable.zip.
- Kaynak snapshot/manifest: TLTP-HOME-V4-20261016/before ve source-build-v3/source-manifest.json; son kaynak snapshot'ı aynı build kaydındadır.
- Gerçek kaynak derlemesi/import/build, native kontrol, elle oynanış ve son görsel beğeni ayrı kabul seviyeleridir. Native koşu API ile hareket/iş olaylarını çağırır, kaynak ve oda kurulumunda açık setup warp'ı kullanır. Fiziksel bütün kapı rotaları, uzun kampanya, kayıt yükleme ve elle ses/klavye/fare kabulü değildir.
- Kontrolün oda, ikon ve iş görüntüleri native-v3 dizinindedir. İlk koşuda gerçek renkler ve işler görünürken bake malzeme adları/oda keşfi kontrolü başarısız oldu; semantik ad ve oda adı düzeltilip yeniden üretildi. Önceki başarısız kanıt silinmedi.
- FPS projesi ve gerçek oyuncu kayıtları bu paketin yazma/test kapsamı dışındadır.

## Kaynak ve sahiplik

Art: PanicResidenceArt / PanicHomeArt. İş/UX: PanicExpedition / PanicWorkArt / PanicHomeHud. Root: PanicHomeWorld / PanicHomeLife / PanicRefinement / effects / builder ve canlı entegrasyon. QA: PanicRefinementSmoke. Alt ajanlar benzersiz staging alanında çalıştı; Unity ve player yalnız root tarafından çalıştırıldı. Tüm yeni geometri ve ikonlar yerel özgün prosedürel koddur. Mevcut CC0 oyuncu/animasyon kaynağı ve lisans dosyaları korunur; [asset kaydı](ASSET-CREDITS.md) ve [oyuncu provenansı](Existing-Player-Provenance.md) geçerlidir.

Son test: native-v3/native-refinement-smoke.json: 12/12 PASS, 0 hata, 36,01 saniye, 55,73 FPS, seslendirme çalma karesi 0. SHA256 Assembly-CSharp: 670CA2F91082EAF78AC6A5508DAA05F06A6D1940103AD8A94E8D5FABF17ED56A. Kaynak dondurma doğrulamasında sıfır fark var.

Normal açılış / temizlik: Özel test argümanı olmadan Home V4 açıldı; normal-player.log HOME_V4_READY kaydı içeriyor. Eski Home V2 ve Home V3 ZIP Geri Dönüşüm Kutusu'na alındı (267.289.530 bayt; diskte boşalan alan iddiası değildir). Açık Home V3 oyuncusu PID 47912 ve kullandığı klasör korundu. Güncel başlatıcı yalnız Home V4 açar. Tek güncel portable ZIP var. Ayrıntı cleanup-receipt.json ve LATEST-BUILD.json içinde.

## Görüntü kayıtları

[Gösterilen banyo](../../Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/native-v3/refinement-bathroom-layout.png), [halı/yatak](../../Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/native-v3/refinement-bed-and-rug.png), [bilgisayar](../../Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/native-v3/refinement-desktop-details.png), [dolu alet kutusu](../../Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/native-v3/refinement-toolbox-stocked.png), [boş kutu](../../Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/native-v3/refinement-toolbox-empty.png), [aktif iş](../../Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/native-v3/refinement-work-ratchet-anchor.png).

Son küçük revizyonda tuş etiketlerinin ölçümüne güvenli genişlik payı eklendi; boş kapta F/boş uyarısı tekrar gösterilmiyor. Son v3 kontrolü bu kaynakla yapıldı. Builder v2 art cache'i semantik materyal isimleriyle aynen kullanıldı.
