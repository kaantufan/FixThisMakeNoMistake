# Home V4.9 — background QA delivery

Computer Use, OS girişi ve pencere etkinleştirme kullanılmadı. Açık V4.8 oyunu korundu. FPS değişmedi.

## Düzeltilen ürün hataları

- P1: yeni ağaç gövdesinin oyuncunun içine kurulması. Başlatma ve son commit kapsül çakışma kontrolü; iptal/red ücretsiz. Native önce0.213m penetration/0.244m itme; sonra0penetration,~0.0034m zemin oturması.
- P2: sabitlenmiş eşyada çözme yolunun kilitlenmesi. Görünür ÇÖZ ve1shortcut; satın alınmış ankraj hakkı korunur, tekrar takma iki vidalı ve ücretsiz.
- P2: dört simülasyonda yanlış bunker uyarısı. Simülasyon hazırlık metni ayrıldı; kampanya bunker uyarısı korundu.

## Aynı son DLL üzerinde kontroller

| Tur | Sonuç | Süre |
|---|---:|---:|
| matrix | 429/429 | 39.96sn |
| mechanics | 338/338 | 4.14sn |
| physics | 24/24 | 6.12sn |
| direct | 44/44 | 10.18sn |
| fullflow | 141/141 | 113.49sn |
| ui | 161/161 | 10.79sn |
| flow | 27/27 | 4.14sn |
| frame | 58/58 | 12.26sn |
| doors | 11/11 | 59.17sn |
| placement | 11/11 | 21.02sn |

0runtime hata. Son tüm turlar kendi bildirdikleri kapsamda geçti. Matrix sayısı farklı kanıt katmanlarında429gözlemdir; aynı sayıya tam görsel/insan kabulü yüklenmez. CSV veJSON satırları model-only/partial/source-only/not-run ayrımını korur; tur sayıları aynı vaka gözlemlerini kesiştirir, toplanarak bağımsız test sayısı üretilmez.

## 505satırın durumu

| Durum | Adet |
|---|---:|
| NOT_RUN | 17 |
| NOT_RUN_INVALID_RENDER_FIXTURE | 5 |
| NOT_RUN_USER_BACKGROUND_ONLY | 1 |
| PARTIAL_API_ONLY | 6 |
| PARTIAL_CLOCK_FIXTURE | 1 |
| PARTIAL_CONTINUOUS | 1 |
| PARTIAL_NATIVE_PHYSICS | 2 |
| PARTIAL_NATIVE_RENDER | 3 |
| PARTIAL_NATIVE_ROUTE | 1 |
| PASS | 138 |
| PASS_API_ONLY | 20 |
| PASS_API_RENDER_PARTIAL | 33 |
| PASS_CALLBACK_ONLY | 66 |
| PASS_CONTINUOUS | 11 |
| PASS_MODEL_ONLY | 195 |
| PASS_NATIVE_ISOLATION | 1 |
| PASS_RESET_ONLY | 3 |
| SOURCE_ONLY_NOT_RUN | 1 |

Kalan NOT_RUN satırları ayrıntılı CSV/JSON içindedir. Fiziksel Shift/run ölçümü, bazı birleşik modal/aynı kare kombinasyonları, geçersiz renderer durumları, tam kampanya, insan ilk180sn keyif ve dinleme kabulü verilmedi. Kaynakta korunan viewport savunması kaynak incelemesidir; engine-invalid zero-size texture fixture native kabul değildir.

## Kimlik ve izlenebilirlik

- DLL SHA256: `471E6AFF3DBD054C3FE099FD20AABFB8A5498661C16325D3AC5217705106AC01`
- ZIP SHA256: `4319441CB26CA7699F2499F84DAE1E8A9D6CA38C1FC87D8F957DF25F12E2F388`
- Build: `Builds/Home-V4.9-QA-20261017`
- Portable: `Builds/TooLateToPanic-Home-V4.9-QA-20261017-portable.zip`
- Son source/resource/input/metadata/settings snapshot farkı0; lisans kopya farkı0; ZIP CRC ve iç DLL doğrulandı.
- Eski FAIL ve fixture hata kayıtları silinmedi; FIX-LEDGER hangi ürün düzeltmesi ve hangi test uyarlaması olduğunu ayırır.
- Native API/GUI/world fixture yerleştirmeleri, stok ve zaman mutasyonları raporlarda belirtilir. Kesintisiz fullflow test Warp0/enjekteTick0/developer0; kısa rota~39sn. OS/HUMAN değildir.

[505vaka CSV](../../Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/execution-v49/case-results-v3.csv) · [JSON/ham turlar](../../Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/execution-v49/results-v3.json) · [Hata/fixture defteri](../../Evidence/GamejamOrchestra/TLTP-INTERACTION-AUDIT-20261017/execution-v49/FIX-LEDGER.md)

Yeni normal pencere açılmadı. GitHub/Release güncellenmedi; önceki yayının kimliği ayrı tutulur.
