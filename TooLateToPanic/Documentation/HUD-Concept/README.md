# HUD konsepti (HTML)

16 Ekim 2026. "Panik Modu Vurguları" referansıyla hazırlanan, animasyonlu ve etkileşimli HUD prototipi. Unity kaynağı, sahne veya build değişmedi.

## Açma

`index.html` göreli görsel yolları kullanır; yerel sunucu ile açın:

```
python -m http.server 8765 --directory TooLateToPanic/Documentation/HUD-Concept
```

Ardından `http://localhost:8765`. Üst sekmeler veya `[` / `]` ekran değiştirir; diğer tuşlar oyundaki gibidir (E aç, F al, Tab çanta, Esc duraklat, 1/2/3 iş, A/D + Enter + Space iş adımları).

## İçerik

- 10 ekran: Başlık, Hazırlık, Toplama, Çanta, Aktif iş, Son 60 sn, Duraklat, Afet (deprem/sel/yangın), Kayıp, Sonuç + hasar gezisi.
- Sahne 1600×900; `PanicHomeHud` canvas'ıyla aynı ölçü, doğrudan taşınabilir.
- Palet: krem #F6F1E8, kömür #292625, petrol #256A6C, amber #F2A93B, pas #C65B3D, taş #D8D2C8, alarm #E4472D. Unity `Color` değerleri sayfanın altındaki stil rehberinde.
- Yazı tipleri: Oswald, Barlow Condensed, Barlow (Google Fonts, SIL OFL). Sayfa açılırken çevrimiçi yüklenir; oyuna alınırsa Font asset olarak eklenmeli.

## Görsel kaynakları

`img/` altındakiler projenin kendi kanıt karelerinden türetildi; eski HUD şeridi ve sinematik bantları kırpıldı.

| Dosya | Kaynak |
| --- | --- |
| bg-ground, bg-garage, bg-balcony, bg-rescue, bg-disaster | `Evidence/GamejamOrchestra/TLTP-HOME-V3-20261016/native-v3/` |
| bg-upper, bg-bunker, work-ratchet, work-timing, work-balance | `Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/native-v3/` |
| icons/*.png | `Evidence/GamejamOrchestra/TLTP-HOME-V4-20261016/assets/icon-check/` (`PanicResidenceArt.Icon` çıktıları) |

Başlık ve duraklatma sahnelerindeki orman, ev, helikopter ve fırça darbeleri sayfada SVG olarak çizilir.

## Logo (17 Ekim 2026, v2)

Sunum: `logo.html` (animasyonlu tanıtım, ilkeler ve küçük boyut testi, varyantlar, oyun karesinde açılış denemesi). HUD başlık ekranı `logo/logo.svg` kullanır.

- Bütün harfler `logo/build_logo.py` içinde geometriyle çizilir; yazı tipi bağımlılığı ve lisansı yoktur.
- "PANIC" içindeki I harfi doğrudan bir kum saati silüetidir (kapaklar, belde daralan iki hazne, koyu cam ve kum); kelime fay hattında kırılıp kayar (deprem), alt bantta su çizgileri (sel), amber→pas ısı gradyanı (yangın). Ünlemin noktası damla.
- "TOO LATE" kalın, dolu deco harfler ve pas gölgeyle; ikinci O 23:59 gösteren saat. "TO" yırtık, hafif eğik bir pas uyarı şeridinin üstünde.
- Amblem v2, ikon rehberlerindeki ortak kurallara göre yeniden çizildi: tek odak (kum saati + fay), dolu şekiller, tek renk / gri tonda sınandı, 16–32 px için çatlaksız `mark-small.svg`.

| Dosya | Kullanım |
| --- | --- |
| `logo/logo.svg`, `png/logo-2000.png`, `png/logo-1000.png` | Ana logo, koyu zemin |
| `logo/logo-light.svg`, `png/logo-light-2000.png` | Açık zemin |
| `logo/logo-mono.svg`, `png/logo-mono-2000.png` | Tek renk (krem); baskı ve damga |
| `logo/mark.svg`, `png/mark-512.png`, `png/mark-256.png` | Amblem, 48 px ve üstü / exe ikonu |
| `logo/mark-small.svg`, `png/mark-32.png`, `png/mark-16.png` | 16–32 px simge |
| `logo/mark-light.svg`, `logo/mark-mono.svg` ve PNG'leri | Açık ve tek renk amblem |

Yeniden üretmek için `logo/` içinde `python build_logo.py`, ardından `python render_png.py` (saydam PNG için yerel Chrome kullanır).
