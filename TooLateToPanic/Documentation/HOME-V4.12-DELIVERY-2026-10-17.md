> Public copy: download links and evidence summaries are adapted for GitHub. Full local source counts/hashes describe the original build input; the linked public source manifest describes only the selected published subset. Statements about unpublished status below describe the original local delivery. See [publication notes](Publications/v0.4.12/ADAPTATION.md).

# Too Late to Panic! — Home V4.12 teslimi

17 Ekim 2026. Unity6000.0.73f1 / Built-In / Windows64 /0.4.12. Güncel aday `Home-V4.12-r2-20261017`. [Başlatıcı](../Play-TooLateToPanic.cmd), [taşınabilir ZIP](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.12), [LATEST](../LATEST-BUILD.json).

## Kullanıcı maddelerinin sonucu

| İstek | Uygulama |
| --- | --- |
| FPS son30saniye, sade çanta | Hazırlık izometrik başlar; son30saniyede gerçek konum/controller korunarak1.1sn kamera geçişi otomatik başlar. G ile önceki önlemlere dönüş kaldırıldı. |
| Temel ihtiyaçlar | Her dalgada1su+1gıda yeterli. Alınan gereken ilk birim gerçek çantaya, kalan yığın gerçek envantere yerleşir. Hazır ilk yardım/fener/pil/radyo/battaniye isteğe bağlıdır. Kapasite kuralları korunur. |
| Hasar sınırı | Yüzde75ve üzeri kayıp;74.99/75/75.01 gerçek CalculateDamage ve sonuç dallarıyla doğrulandı. |
| Çit inşası | Üç çit kurulum noktası ve defter görevi kaldırıldı; API de çit işini reddeder. Dekoratif sınır çitleri korunur. |
| Tam ağaç sıraları | Üç bölge×4olgun ağaç. Batıda iki10m şerit ve kuzeyde14m şerit; geometri/önizleme/zemin izi/trunk collider aynı koltukları kullanır. Mevcut assetlerden ayrı yerleşir. Tek kaynakta3paketin2ye düşmesine yol açan RNG alt sınırı düzeltildi; kaynak hâlâ sonlu ve tek seferliktir. |
| Duvar/pencere etkileşimi | Erişim nesne merkezinden değil gerçek yüzeyden ölçülür. Seçili hedef yakındaki nesne yüzünden kendiliğinden değişmez. Pencere ışını gerçek açıklığın merkezine gider; sill/jamb yanlış reddi giderildi, gerçek engel reddi korunur. |
| Yanlış eksik malzeme/başlamayan iş | Kısa akıştaki eski yalnız-ankraj kısıtı geçerli isteğe bağlı işleri engelliyordu; önlem aşamasında açıldı. Dolap stoklarını baştan boşaltan eski kısa akış davranışı kaldırıldı. Reddedilen işlem eski hedefin eksik kartını bırakmaz. MaterialCosts/WorkCost ve sayısal havuz otoritesi, ücretsiz iptal ve bir kez tüketim korunur. |
| Sığınak kapağı | Sinematik ve normal hinge animasyonu aynı open/eksen durumunu kullanır. Oyuncu önce açılmış şaftın üstüne gelir, sonra iner. Normal FPS’den E ile gerçek sığınağa, kısa akışta E ile gerçek güvenli alana girilir. |
| Çatı | Dört afetin dış sinematik çekimlerinde ana/garaj çatısı ve gökyüzü görünür. İç çekimlerde kesit korunur. |
| Toplama vurgusu | Alt çizgi yerine gerçek nesnenin3D sınırlarını çevreleyen tel kutu. Toplama nesnesinde büyük pick trigger yerine görünen assetin bounds'u esas alınır. |
| Aşamaların ayrılması | Önlem aşamasında fiziksel çanta erzaklarının renderer/collider kökleri kapalıdır. Son30saniyede bunlar açılır, yapı malzemesi toplama ve önlem işleri kapanır. Defter pinleri de aynı aşamaya uyar. |
| Dolaplar | FPS geçişinde kapanır; E ve doğrudan dünya ToggleContainer API'siyle açılamaz. Gerçek geçiş kapıları ve garaj kullanılabilir. |
| Ayaklı lambalar | Etkileşimsiz iki dekoratif lamba kaldırıldı; kaynak/lisans arşivi korunur. |
| FPS HUD | Ayrı sade kartlar: kalan süre,1/1su-gıda, gerçek en yakın eksik hedef ve yön/kat/mesafe, nişangâh, seçili nesne ve alt kontroller.1600×900 ve960×540 gerçek render ile kontrol edildi. |

## Son adaya bağlı kanıt

Derleme0hata/0uyarı. **204/204 PASS,0runtime hata,24.07sn**. [Native receipt](Publications/v0.4.12/native-summary.json). Bu toplam75model kontrolünü ve129dünya/işlem/frame/render/başlatma gözlemini içerir; ayrı detached75sonucu toplama yeniden eklenmez.

Gerçek iki-vida tamamlama/iptal ve kesin maliyet; duvar+pencere güçlendirmesi; pencereye eklenen gerçek engelin reddi ve kaldırılınca geri erişim;12etkin ağaç gövdesi ve bağımsız etkin CapsuleCollider ile overlap0;31/30saniye sınırı ve pause; kamera konum/controller korunması; erzakların gerçek renderer/collider ayrımı; FPS dolap reddi/geçiş kapısı/sonlu toplama/otomatik çanta; normal FPS sığınak girişi; kapak105derece açıkken69kare iniş gözlemi; dört afet dış çatısı;1600ve960HUD renderı; kısa akış son30ve E güvenli alan çözümü kontrol edildi.

Bu sürücü **15konum fikstürü**,31/30doğrudan saat kurulumu, kısa akışta açık Tick zaman fikstürleri ve son kısa-sığınak kontrolünde açık ankraj-hazır fikstürü kullanır. İşler gerçek kare delta ve üretim API'leriyle yürür. Kısa sığınak hazır kurulumu oynanmış ankraj rotası sayılmaz. **OS klavye/fare, insan süre/keyif/dinleme ve tam kampanya kabulü değildir.**128yeni model koşusunda sonlu3ağaç bölgesi stoku ayrıca kontrol edildi; kaynak geometri27/27sonucu native geometri kabulü diye sunulmaz. Önceki sürümlerin sayıları yeni DLL kanıtına eklenmez.

İlk aday196gözlemde iki kök hata nedeniyle reddedildi: pencere alt/yan çerçevesine yönelen ışın ve seedling stok RNG'sinin3ü2ye indirmesi. Önceki başarısız receipt, log ve kaynak snapshot'ı korunur. Son aday bütün dar sürücüyü yeni DLL ve taze GUID profilinde yeniden geçti.

## Paket ve izlenebilirlik

| Kayıt | SHA256 / sonuç |
| --- | --- |
| Assembly-CSharp.dll | `8E3DE7EEB0D3F52D4F77539B9CBE6D04CA7FCF40043803BACE073579484364CB` |
| ZIP | `480B7F488D5147655B55786DA45562D96CB3FC2A4C229983B9EE086E47B3201C` |
| ZIP boyutu | 177669754byte |
| ZIP DLL eşleşmesi | Native DLL ile byte aynı |
| Tam kaynak manifesti | 34537dosya, `CA32E053031C1ABC20B54462099E1C74958B7EA15201CC3EE588D37B5230D666` |
| Build manifesti | 153dosya, `60013A5B3B2ED343ECE5AB5E4EC20ADE6E7381E88C9BD1CCEB3CE2F516ED8D3B` |
| C# kaynak kayması |99dosyada0 |
| Gerçek skorlar |3dosyanın önce/sonra SHA256değerleri aynı |

[Kaynak manifesti](Publications/v0.4.12/source-sha256.json), [build manifesti](Publications/v0.4.12/local-build-file-sha256.json), [paket receipt](Publications/v0.4.12/local-build-receipt.json). Küçük C#snapshot'ları ve worker önce/sonra hashleri korunur. Yeni ücret/kredi/model/ses sağlayıcısı yok; mevcut font/karakter/asset/ses lisansları taşınabilir paketin Licenses klasörüne kopyalandı. Yalnız eski sıra ile çelişen “çanta hazır, şimdi sabitle” tetiklemesi mevcut onay efektiyle değiştirilir; ses kaynağı silinmez.

Açık kullanıcı V4.11-r7 PID2536ve DLL34E3969F...korundu; yeni normal oyuncu penceresi açılmadı. Kardeş eski FPS projesine ve gerçek kayıtlara dokunulmadı. GitHub yayını yapılmadı.

Temizlik sınırı: yalnız bu görevdeki başarısız ilk `Home-V4.12-20261017` build klasörünü kaldırma komutu otomatik onay incelemesinde **blocked by policy** ile reddedildi; daha ayrıntılı gerekçe verilmedi. Komutun hiçbir adımı çalışmadı, klasör korunuyor ve alternatif silme/taşıma denenmedi. Önceki reddedilmiş V4.11/V4.10 temizlik hedefleri de korunur. Güncel r2/ZIP/başlatıcı teslimi etkilenmez. [Kayıt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V412-20261017/manager/own-failed-v1-cleanup-blocked.json).


## Sonraki oyun içi takip denemesi

Kullanıcı isteğiyle normal V4.12açıldı. Dar klavye/fare altkapsamı ayrıca doğrulandı; aynı DLL’de ayrı204/204native tekrar,0runtime hata. Orijinal native tur OSgirdisi sayılmaz. [Takip denemesi ve sınırlar](HOME-V4.12-PLAYTEST-2026-10-17.md).
