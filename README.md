# Too Late to Panic!

**Afet gelmeden önce hangi önlemi alacaksın?** KKTC · Güzelyurt · Kalkanlı'daki aile evini sınırlı zaman ve bütçeyle hazırla. Mobilyaları sabitle, tesisatı kapat, su ve gıda topla; afetin ardından kararlarının evdeki sonucunu gör. Yer gerçektir; kişiler ve afet olayları kurmacadır.

Unity 6000.0.73f1 · Built-In Render Pipeline · Windows 64-bit · çevrimdışı tek oyunculu.

## Oyna

Güncel yerel paket **Home V5.9-r7 / 0.5.9**. Çalışma alanında [başlatıcıyı](Play-TooLateToPanic.cmd) kullanın. İndirilen Windows ZIP'ini tamamen çıkarıp `TooLateToPanic.exe` dosyasını veri klasörleriyle birlikte çalıştırın. Kaynak deposu oyun paketini içermez; yayımlanmış paketler [GitHub Releases](https://github.com/kaantufan/FixThisMakeNoMistake/releases) sayfasındadır. Her paketin sürümünü kendi sürüm notlarıyla eşleştirin.

Normal kampanyada **90 saniye önlem + 40 saniye erzak/FPS**, başlangıç bütçesi **1500**. İlk dalga Deprem, ikinci dalga Sel + Fırtına; sonraki dalgalarda afetler rastgele seçilir. Her dalgada ortak 20 yuvalı envanterden bir su ve bir gıda kullanılır. Sel varsa üst kattaki panik odasına, diğer afetlerde oyunun bodrum sığınağına fiziksel olarak ulaşın. %75 ve üzeri hasar kayıptır. Bu kurallar oyun tasarımıdır; gerçek afetlerde resmî kurumların yönlendirmesi izlenmelidir.

| Girdi | İşlev |
| --- | --- |
| WASD / oklar, Shift, Space | Hareket, koşma, zıplama |
| E / F / Q | Etkileşim / malzeme-erzak alma / hedef değiştirme |
| Tab / J | Envanter / hazırlık defteri |
| Fare ve ekrandaki iş tuşları | Seçme, montaj ve tesisat işleri |
| F1 / Esc | Tuş rehberi / duraklatma |

F2/F3 geliştirici seçenekleri kullanıldığında koşu skor dışı kalır. FPS yalnız erzak hazırlığı aşamasındadır; bağımsız eski FPS denemesi aktif ürün değildir.

## Sürüm ve doğrulama

[LATEST-BUILD.json](TooLateToPanic/LATEST-BUILD.json), [R7 teslim kaydı](TooLateToPanic/Documentation/HOME-V5.9-LOCAL-GLOW-DELIVERY-2026-10-18.md) ve [yayın kaydı](PUBLICATION.md) kaynak, yerel build ve GitHub paketinin kimliğini ayrı gösterir.

R7'de **472 Editor kontrolü** ve **524 aynı DLL native UI/yerel parlama kontrolü**, 69 incelenmiş görüntü ve 0 runtime hatası kayıtlıdır. Fiziksel OS girdisi, insan dinlemesi/okunurluğu ve 6–7 dalgalık insan kampanya kabulü **NOT_RUN**. Eski sürüm test sayıları bu pakete aktarılmaz.

## Kaynak düzeni

```text
TooLateToPanic/
  Assets/           Oyun kodu, sahne, sanat, ses ve kaynak lisansları
  Packages/         Unity paket tanımları
  ProjectSettings/  Unity proje ayarları
  Documentation/    Teslim, asset ve lisans kayıtları
  LATEST-BUILD.json  Güncel yerel paketin kimliği
Docs/               Tasarım, yarışma beyanı ve çalışma düzeni
Tools/              Üretim ve doğrulama araçları
```

Yerel `Builds/`, `Evidence/`, Unity cache'leri, kişisel kayıtlar ve korunan `TooLateToPanicFPS/` kaynak yayınına girmez. Yerel kaynaklar taşınmadan korunur; geçmiş manifestlerdeki yollar değiştirilmez.

Unity Hub'dan **TooLateToPanic/** klasörünü tam olarak **6000.0.73f1** ile açın. Ana sahne `Assets/Game/Scenes/TooLateToPanicResidence.unity`; mevcut Windows üreticisi `TooLateToPanic.PanicResidenceBuild.Build` (`Too Late to Panic/Build Home V5` menüsü). Üretici sahne/asset oluşturabilir; kabul edilmiş kaynak üzerinde deneme amacıyla çalıştırmayın. Yeni build ayrı çıktı klasörüne alınır ve kontrol edilmeden LATEST'e geçirilmez.

## Yarışma, AI ve kaynaklar

[Teslim beyanı](Docs/TOO-LATE-TO-PANIC-JAM-TESLIM-BEYANI.md) · [Güncel kurallar ve kontrol listesi](Docs/JAM-RULES-CHECKLIST.md) · [Asset/lisans kayıtları](TooLateToPanic/Documentation/ASSET-CREDITS.md) · [Belge dizini](Docs/README.md).

18 Ekim teslim duyurusuna göre video **en fazla 1 dakika**, MP4/AVI ve build ZIP'inden ayrı; sahne süresi video dahil **3 dakika**. Görsel, ses, kod, metin ve diğer alanlarının her birinde AI kullanımı ayrıca beyan edilir. Bu çalışma kapsamında yarışma formu gönderilmemiştir.

Kaynaklarda ve prosedürel üretimde OpenAI Codex; takım asset üretiminde Antigravity; seçili ses/müzikte ElevenLabs; araştırma ve belgelerde Claude kullanımı kayıtlıdır. Quaternius, fontlar ve takım içi kaynakların lisans/provenansı ilgili dosyalardadır. Depodaki tüm içeriğe ortak bir MIT/CC0 lisansı atanmaz; açık lisans teyitleri teslim beyanında görünür.

## Sonraki güncellemeler

[CONTRIBUTING.md](CONTRIBUTING.md) ve [sürüm güncelleme düzeni](Docs/RELEASE-WORKFLOW.md) her değişiklikte uygulanır: kaynak kimliği, ayrı build, ilgili kontrol, tek güncel launcher/LATEST, sonra ayrı Git yayın kopyası. Oynanabilir sürüm, kaynak snapshot'ı ve yarışmaya teslim durumu birbirinden bağımsız kaydedilir.
