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
