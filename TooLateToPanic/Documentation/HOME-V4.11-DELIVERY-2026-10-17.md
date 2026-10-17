# Home V4.11 teslimi — 17 Ekim 2026

Revizyon planının zorunlu1.1–5.3 maddeleri aktif oyuna uyarlandı. Kullanıcının son açıklamasıyla FPS yalnız afet çantası hazırlığına döndü: aynı oyuncu, controller, Model ve Expedition kullanılır; kamera mevcut konumdan1.1sn sinematikle geçer. G, oyuncuyu taşımadan aynı yerde izometrik önlemlere döndürür. Ayrı eski FPS projesi, build ve kayıtları korunur; yeni bir genel FPS kampanyası yapılmadı.

## Uygulananlar

| Alan | Son davranış |
| --- | --- |
| Toplama | Dolap E ile açılınca bir defa RNG miktarında inşaat malzemesi kaynak havuzuna geçer; yeniden açma tekrar ödül vermez. Erzaklar evde açık3D nesnelerdir, FPS merkez ışınıyla E/F kullanılarak alınır. Duvar/nesne ilk engel olmaya devam eder. |
| Depolama | Kaynaklar12envanter/8çanta yuvasından ayrıldı. Kısmi yığın doldurma, birleştirme, takas, çantaya sürükleme ve geri alma atomik işlemlerdir. Pil/cihaz ve destek sınırları korunur. Bandaj/antiseptik ve ilk yardım üretimi kaldırıldı; hazır kit kullanılır. |
| Çanta / sığınak | Evde tek fiziksel çanta bulunur. Sığınak girişi yalnız bodrumdadır; hedef/kamera noktası güncellendi. Drenaj/yangın kısa akışının güvenli alanları afet türüne uygun kalır. |
| Mimari / fizik | Garajın iç kapısı ve gerçek duvar boşlukları eklendi. Mutfak/banyo bölmeleri3.2m fiziksel engeldir; FPS'de görsel yükseklikleri de tamdır. İzometrik görünümde garaj/üst katın dört cephe kesiti yönle değişir; fizik kapatılmaz. |
| Atmosfer | Bodrumda ortam/güneş ışığı yumuşak biçimde kararır ve çıkışta geri gelir. Sığınak ikaz ışığı eklendi. Yeni kemerli3D şöminenin ocak ağzı odaya bakar; ateş/ışık yerel noktaya bağlıdır, söndürülmüş alev sonraki görünürlük güncellemesinde açılmaz. |
| Arayüz | Esc duraklatması normal/kısa akış saatini gerçekten dondurur. Ana menü yatay gezinir; pause WASD/oklar ve Enter/KeypadEnter ile çalışır. Yangın tüpü metinleri netleştirildi. FPS'de nişangâh, eşya/etkileşim ve G ipucu görünür. |

Orijinal rapor değiştirilmedi. [Madde madde kaynak/kabul haritası](../../Evidence/GamejamOrchestra/TLTP-REVISION-V411-20261017/report/acceptance-map.md), ön final kaynak incelemesidir; son adayın test kapanışı aşağıdaki r7 receipt ve manifestlerle yapılır.

## Assetler ve defter

`yarkin-assets` kaynak sürümü `11f12b4cf1f6bf676dcf3dc5055e1b66c8b1ff57`:31FBX, temizlenmiş prefablar,14klip/7mekanizma ve Built-In materyaller alındı. Eşdeğeri olan mobilya/erzak/yangın istasyonu görselleri değiştirildi. Mevcut oynanış kimliği olmayan modeller yeni zorunlu görev sayılmadı. İçe aktarılan girdi/fizik/model scriptleri kullanılmaz; gerçek oyun kuralları yetkilidir. Kliplerin eğrileri, olayları ve GUID'leri korunarak oyuncuda pasif örnekleme için Legacy bayrağı düzeltildi. Sıfır quaternion üretimi giderildi.

Yeni eldivenli eklemli el/kol modeli FPS toplama/açma hareketini gösterir; eski üçüncü şahıs V2 görünümü korunur. Parmakların her karede tümüyle okunması veya eşyayı fiziksel olarak kavrama/handle-wrap bu kanıtın iddiası değildir.

`defter-mockup` kaynak sürümü `07ecbccb097ea796067afd8a35af615563857d43`: HTML tasarımı Unity'ye uyarlandı; örnek değerler taşınmadı. J ile açılan altı sayfa ev planı, sel, deprem, yangın, fırtına ve envanterdir. Kat seçimi, kaynak pinleri, hedef seçimi, gerçek stok/önlem/bütçe kayıtları, sayfa dönüşü, son isteği kuyruğa alma, ters dönüş ve azaltılmış hareket vardır. Açık defter oyunun saatini ve dünya girdisini durdurur. Matris/clip nedeniyle sayfa dışına kaçan çizgiler ve koyu fondaki metin kontrastı native karelerle düzeltilmiştir.

[Eksik/eşdeğer asset listesi](MISSING-ASSETS-V411.md): çözümsüz zorunlu kaynak açığı yok. Şömine/el-kol yerel üretim, dalda eşdeğeri olmayan yapı malzemeleri ve diğer mobilyalar mevcut kaynaklardan korunur. İsteğe bağlı özel metalik loot sesi ve merdiven başındaki ikinci beacon seçilmedi; kap sesi/HUD bildirimi ve sığınak ışığı vardır. Yeni ücretli sağlayıcı, kredi veya ses üretimi yapılmadı; [lisanslar/provenans](ASSET-CREDITS.md) korundu.

## Son adaya bağlı doğrulama

Build: `Home-V4.11-r7-20261017` / Unity6000.0.73f1 / Built-In / Windows64 /0.4.11. Derleme0hata/0uyarı. r3 tam sanat bake'i üzerine yalnız kaynak/yerleşim/GUI düzeltmeleri mevcut bake ile derlendi.

**156/156 PASS,0runtime hata, 34.24sn.** [Native receipt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V411-20261017/manager/native-v7-b46fe7d3-0687-4edb-b5af-fdbf6988a43b/revision-smoke.json). Tüm kontroller aynı son DLL ile yeniden çalıştı:49gerçek Unity model/envanter işlemi,7mekanizma açılış/ters kapanış örneklemesi, gerçek FPS ışını/toplama, kamera/oyuncu konumu, controller duvar/garaj rotası, pause, HUD OnGUI olayları, çanta drag-drop ve defter kontrolleri. Garaj/üst katta4yön×2gerçek renderer kontrolü ve şömineyi söndürme/yakma dahildir.

29native kare; ana boyut1600×900, ek defter960×540. Altı statik sayfa kendi dönüşü bittikten sonra yakalandı; dönüş karelerinin frame/progress bilgisi receipt içindedir. [Defter görsel incelemesi](../../Evidence/GamejamOrchestra/TLTP-REVISION-V411-20261017/notebook/NATIVE-V5-REVIEW.md) aynı OverlayR2 kaynağını; [son şömine incelemesi](../../Evidence/GamejamOrchestra/TLTP-REVISION-V411-20261017/assets/qc/NATIVE-V7-REVIEW.md) r7 kare/DLL bağını açıklar.

Bu testler **10 açık konum fikstürü**, üretim API'leri, gerçek kare saatleri/ışın/collider ve gerçek OnGUI içinde sentetik olay kullanır. OS klavye/fare girdisi, insan keyif/okunurluk/dinleme kabulü, tam kampanya veya270+30sn insan süre kabulü değildir. Defterin normal hazırlıkta saat kilidi native'dir; aynı defterin kısa akışa özel ayrı saat fikstürü çalıştırılmadı. Önceki V4.9/V4.10 sayıları yeni DLL'ye eklenmedi. Uzun süreli bellek/soak kabulü bu batch'in iddiası değildir.

## Paket ve izlenebilirlik

| Kayıt | SHA256 / sonuç |
| --- | --- |
| Assembly-CSharp.dll | `34E3969FBBE5047A6400D9FE253ACE1B2177BEDD07ECBBDA3020D3745008E047` |
| Taşınabilir ZIP | `8C3B199D261B1AEFB470D2896CAB883C2F4FDD06BB265843514BB753E03F2E1D` |
| ZIP boyutu | 177633082byte |
| Paket DLL | Native DLL ile byte aynı |
| Tam kaynak manifesti | 34529dosya, `578A58373614BAA5CE09ADB9D7B077C53F6CB2E73664CCB5E88488C0FB737E9B` |
| Build manifesti | 149dosya, `D9191215FCFE4CBE9AABA9C97591A1A42DD5F0C6AB81CFC3ABB62D26E5ECF534` |
| C# kaynak kayması |96dosyada0 |
| Gerçek skor kayıtları |4dosyanın önce/sonra hashleri aynı: True |

[Tam kaynak manifesti](../../Evidence/GamejamOrchestra/TLTP-REVISION-V411-20261017/manager/final-full-source-manifest.json), [build manifesti](../../Evidence/GamejamOrchestra/TLTP-REVISION-V411-20261017/manager/final-build-manifest.json), [birleşik receipt](../../Evidence/GamejamOrchestra/TLTP-REVISION-V411-20261017/manager/FINAL-RECEIPT.json). Küçük C# kaynak snapshot'ı ve önceki başarısız denemelerin log/receiptleri korunur. Önceki açık V4.8 süreci dışarıdan kapanmış gözlendi; bu görev kapatmadı, kapanış nedeni bilinmiyor; dosya/kayıtları korunur. Önceki QA'nın V4.10 çalışması bağımsız tarihsel adaydır.

Güncel [başlatıcı](../Play-TooLateToPanic.cmd), [ZIP](../Builds/TooLateToPanic-Home-V4.11-20261017-portable.zip), [LATEST](../LATEST-BUILD.json). GitHub'a yeni yayın yapılmadı.


Temizlik sınırı: yalnız bu görevde üretilen altı eski V4.11 aday build klasörünü silme komutu otomatik onay incelemesinde `blocked by policy` ile reddedildi; ayrıntılı neden dönmedi. Komutun arşivleme/silme adımları çalışmadı, alternatif silme denenmedi, klasörler korunuyor. Güncel r7/ZIP/başlatıcı teslimi bundan etkilenmez. Kayıt: `manager/own-candidate-cleanup-blocked.json`.
