# Too Late to Panic! — Home V4.9

17 Ekim 2026 · Unity 6000.0.73f1 · Built-In · çevrimdışı Windows. Aktif ürün izometriktir; FPS askıda korunur.

## Yapılan değişiklikler

- Sabitleme yakın planındaki gerçek iki vida başına fareyle tıklanır. Her biri 0,60 sn içinde otomatik takılır; A/D, Space, zamanlama veya ritim gerekmez. Meşgul/tekrarlı/yanlış vida tıklaması işi atlatmaz. İki vida bitince gerçek malzeme ve bütçe bir kez harcanır; yarıda iptal ücretsizdir.
- Duvar/pencere/kapı/çatı güçlendirmesi, drenaj, bariyer, hortum ve ağaç sırası uygun dünya hedefinde kısa kurulumdur. Mobilyaya ilgisiz tüp takılmaz. Yangın tüpü zemin kattaki ayrı ekipman noktasına yerleştirilir; kısa akışın hedefi, işareti ve simülasyon rotası bu noktaya uyarlandı.
- Duvar/pencerede gerçek katmanlı güçlendirme geometrisi ve 0,60 sn parça montajı, aynı anda karakterin hedefe dönük montaj hareketi vardır. Cam/kapı açıklıkları ve fizik korunur. Üst kat/duvar kesitleri yeni eklerde uygulanır.
- Yürüme 2,7→3,645m/sn; koşma 4,2→5,67m/sn. İkisi de ×1,35; hareket klibi hızı da ×1,35, iş/zıplama hızı değiştirilmez.
- Tohum/fidan/sulama sunumu kaldırıldı. Üç bölgenin her biri tek işlemle dört olgun ağaç kurar: toplam 12. Geçmiş `seedling`/`plant` kod kimlikleri uyumluluk için korunur; görünür isim/ikon/3D malzeme ağaç sırası paketidir. Her gövdede dar fiziksel çarpışma vardır, bölgeyi kapatan büyük kutu yoktur.
- Dış kurulum bölgenin kendi tarafından kısa kamera planıyla görünür; işlem süresine bekleme eklemez. Sadece uygulanabilir tek önlem eylemi gösterilir; tamamlanan kurulumun Başla/Hazırla istemi kalkar. Taşıma ve depo açma ayrı geçerli etkileşimlerdir.
- Kullanıcının karakter oturumundaki son kararıyla görünüm güzelleştirmesi geri alındı. Eski V2 saç/yüz/giysi/geometri/materyal görünümü korunur; yalnız istenen montaj hareketi ve el aleti temizliği bağlandı. V3/repair-r1 görünümü nihai teslim değildir.
- Ayrı buton QA oturumunun giriş sırası, iptal/aynı kare koruması, pointer callback ve reset düzeltmeleri korunur. Yeni iş sözleşmesi o oturuma iletildi; eski minigame dizilerini bekleyen testler güncel kabul sayılmaz.

## Kanıt

Yeni doğrudan iş kontrolleri **44/44**, kesintisiz kısa akış+dört afet **141/141**, runtime hata: 0. Derleme hata/uyarı: 0. Altı kaynak/prefab/asset/metadata/settings kategorisinde fark: 0; gerçek ve izole skor hashleri korundu. Toplam 45 gerçek render kaydı. Doğrudan iş testinde açık hedef konumu fixture'ları vardır; kesintisiz rotada test Warp: 0, zaman enjeksiyonu: 0, geliştirici değişikliği: 0.

| Kısa akış olayı | Başlangıçtan süre |
|---|---|
| İlk malzeme alındı | 2.79 sn |
| Su çantaya kondu | 3.05 sn |
| İlk gerçek önlem tamamlandı | 4.41 sn |
| Deprem başladı | 9.12 sn |
| Drenaj hazırlandı | 26.20 sn |
| Üst kattaki güvenli noktaya ulaşıldı | 31.40 sn |
| Yangın ekipmanı hazırlandı | 34.47 sn |
| Üç afet sonucu görüldü | 38.62 sn |

| Simülasyon | Gerçek terminal | Hasar | Doğal afet/sonuç |
|---|---|---|---|
| Deprem | Hasar gezisi | %52.33 | 12.00 sn |
| Sel | Hasar gezisi | %57.42 | 12.00 sn |
| Yangın | Hasar gezisi | %46.62 | 12.00 sn |
| Fırtına | Kayıp | %74.23 | 19.34 sn |

Kısa akış **38,99 sn**. Bu hazırlanmış otomatik rotanın süresidir; ilk kez oynayan kişinin hızı/keyfi değildir. Gerçek kayıp sonucu başarılı hayatta kalma sayılmadı; hasar/kayıp eşiği değiştirilmeden inceleme/menü/tekrar doğrulandı.

DLL `4761CB2672AA3D21B4084AB6725FB327BCF39D2BC39467B6F6DFB7672E0A364B`. EXE `0E1B64A3C0EBA3482F4C7B57F36CF8811997C0CE3BBED914C116CB9D4799FC7C`. Güncel tek başlatıcı ve taşınabilir ZIP LATEST-BUILD.json ile bağlıdır.

Doğrudan test: `Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/manager/direct-v5-551a90db-795e-4194-8bf7-8e73a5417a9e`. Rota: `Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/manager/fullflow-v5-8aa847b6-1799-4182-9680-b476dcba7444`. Manifest: `Evidence/GamejamOrchestra/TLTP-DIRECT-ACTIONS-20261017/manager/source-build-v5/source-manifest.json`.

## Kaynak ve sınırlar

Üç ayrı fork staging paketi ve root entegrasyonu kullanıldı. Yeni özgün montaj/vida/ağaç sanatı prosedürel proje kaynağıdır. Yangın ekipmanı mevcut lisans/provenanslı kaynakları kullanır; Yarkın provenansı ve orijinal CC0 humanoid/animasyon lisansları korunur. Yeni indirme/ücret/kredi/sağlayıcı işi veya ses üretimi yapılmadı. Önceki ses politikası değişmedi.

V1 direkt test 31/40 başarısızdı ve korunur. Sabitleme fixture'ı yere oturmadan Onboarding'de kalmış, duvar fixture'ı merdiven korkuluğu arkasında seçilmişti; dar fixture onarımı yapıldı, fizik/kurallar gevşetilmedi. Gerçek kalan istem, dış kurulum görünürlüğü ve iptal önkoşulu da düzeltildi. Ayrıntı manager/REPAIR-NOTES.md.

OS girdi ve ilk 180 sn insan anlaşılabilirlik/keyif/dinleme kabulü, uzun kampanya, bütün assetlerin son sanat kabulü verilmedi. Önceki küçük hasar etiketi çakışması bu paketin kapsamı dışında kalır. Kullanıcının açık V4.8 player'ı kapatılmadı; kullandığı klasör korunur. Ayrı QA'nın kapsamlı test planı devam eder; bu sonuç bütün olası etkileşimlerin kusursuz olduğu iddiası değildir. Yeni uzak yayın yapılmadı.
