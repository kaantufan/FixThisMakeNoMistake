# Too Late to Panic!

Afete Hazırlık ve Önlem temalı, tek oyunculu ve çevrimdışı **3D izometrik** game jam oyunu. Evi ve bahçeyi güçlendir, malzeme topla, afet çantanı hazırla ve süre bitmeden sığınağa yetiş. Deprem, sel ve yangın dalgalarında evini koru.

**Civil GameJam 2026 October · Home V4.5 / 0.4.5**

## Oyna

[Windows paketini indir](https://github.com/kaantufan/FixThisMakeNoMistake/releases/download/v0.4.5/TooLateToPanic-Home-V4.5-20261017-portable.zip) · [Sürüm sayfası](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.5)

ZIP'i çıkar ve `TooLateToPanic.exe` dosyasını aç. EXE, `_Data`, `MonoBleedingEdge` ve diğer yan dosyaları birlikte tut. Kurulum veya internet bağlantısı gerekmez. Highscore yalnız çalıştırılan makinede tutulur.

## Geliştirici seçenekleri

**F2** veya başlık/duraklat menüsündeki **Geliştirici** düğmesiyle aç:

- Sınırsız bütçe
- İş/üretim malzemesi harcamama
- Hazırlık süresini durdurma
- Tek seferlik +1.000 bütçe

Yukarı/aşağı + Enter/Space veya fareyle kullan; F2/Esc kapatır. Menü açıkken oyun duraklar. Hileli koşular normal skor tablosuna yazılmaz; seçenekleri sonradan kapatmak o koşuyu temizlemez. Yeni temiz koşu için bütün seçenekleri kapatıp yeniden başla.

## Kontroller

| Tuş | İşlem |
| --- | --- |
| WASD / oklar · Shift · Space | Yürü · koş · zıpla |
| E · F | Aç/kapat/etkileş · seçili malzemeyi al |
| Q · Z/X | Hedef değiştir · kaptaki malzemeyi seç |
| Tab/B · oklar · Enter | Envanter/çanta · seç · çantaya koy |
| 1/2/3 · T | Sabitle/koru/güçlendir · duvar/zemin ankrajı |
| Sağ tık · fare · sol tık | Eşya taşımaya başla · yer seç · bırak |
| Taşırken teker/R · sağ tık/Esc | Döndür · iptal |
| İşte A/D · Enter · Space | Hizala/basınç · yerleştir · uygula |
| Esc · H · F2 | Duraklat/geri · yardım · geliştirici |

## Unity kaynakları

Unity **6000.0.73f1**, Built-In, Windows64. Unity Hub'da üst depo yerine **`TooLateToPanic/`** klasörünü proje olarak aç. Ana sahne `Assets/Game/Scenes/TooLateToPanicResidence.unity`; editör menüsü **Too Late to Panic → Build Home V4.5** ile tam build alınabilir. Komut satırı yöntemi `TooLateToPanic.PanicResidenceBuild.Build`.

`Assets`, `Packages`, `ProjectSettings`, tasarım belgeleri ve ajan görev metinleri dahildir. Library/Temp/Logs/Builds, kişisel kayıtlar, yerel çalışma kanıtları ve kullanılmayan tarihsel bake kopyaları Git'e dahil değildir. Mevcut `.meta` kimlikleri ve gerekli asset bağımlılıkları korunur. Windows çıktısı Releases içindedir. FPS denemesi askıdadır; bu depo aktif izometrik kaynak teslimidir.

[Tasarım dokümanı](TooLateToPanic/Documentation/GDD.md) · [Sürüm kanıtı](TooLateToPanic/Documentation/RELEASE-v0.4.5.json) · [Asset kaynakları](TooLateToPanic/Documentation/ASSET-CREDITS.md) · [Oyuncu/animasyon provenansı](TooLateToPanic/Documentation/Existing-Player-Provenance.md)

Yerel V4.5 Windows build 0 hata/uyarı; 12/12 kısa native API kontrolü geçti. Bu, durum fikstürleriyle maliyet/kapasite/saat/kayıt ve geliştirici HUD kontrolüdür; manuel, mini oyun veya tam kampanya kabulü değildir. Public kaynak asset GUID kapanımı 0 eksik referans içerir; public kopyada ayrıca temiz Unity import/build çalıştırılmadı. Tarihsel geliştirme belgelerindeki yerel Evidence yolları Git'e dahil değildir; güncel public kanıt yukarıdadır.

Üçüncü taraf Quaternius karakter/animasyonları korunmuş CC0 lisanslarıyla gelir. Projenin kendi kod/sanatına bu depo yeni bir lisans atamaz; kaynak ayrıntıları Asset kaynakları belgesindedir. Şimdilik yalnız afet sesleri etkindir; tarihsel, kullanılmayan Tolga konuşma WAV'ları public kaynak paketine alınmamıştır.
