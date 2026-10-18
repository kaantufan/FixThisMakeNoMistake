# Kaynak yayını araçları

Bu araçlar Unity açmaz, build almaz veya GitHub'a push yapmaz. Kaynak/GUID seçimi ile kabul edilmiş Windows paketinin doğrulaması farklı işlemlerdir.

## Güncel girişleri kontrol etme

`python Tools/Repository/check_layout.py` kaynak deposundaki güncel belgeleri, bağlantıları, README/LATEST sürümünü ve ignore kurallarını kontrol eder. Yerel tam çalışma alanında `--local-build` eklemek tek build/ZIP düzenini, launcher hedeflerini ve DLL/ZIP SHA256 eşleşmesini de doğrular. İnsan oynanışı veya yeni native kabulü sağlamaz. Python 3.11 ve üstü kullanılır.

## Kaynak seçimi

Kök dizinde PowerShell 7 ile:

```powershell
pwsh -NoProfile -File Tools/Repository/Export-Source.ps1 -AuditDirectory Evidence/RepositoryAudit -HashFiles
```

`source-publish-manifest.json` ve dosya listesi üretir. `-Destination <ayrı-boş-klasör>` ile seçili kaynakları kopyalar. Vendor/sahne bağımlılıkları ve altı runtime shader materyali korunur; tüm eski generated bake klasörlerini körlemesine yayına katmaz. Kök README, AGENTS, CONTRIBUTING, PUBLICATION, `.gitignore` ve `.gitattributes` dahildir. Cache/Builds/Evidence/FPS/kullanıcı verisi kapsam dışıdır. Araç canlı kaynaktan dosya silmez.

## Index kontrolü

Kaynak dondurulup ayrı clone'a incelenen değişiklikler kopyalanır ve stage edilir. Sonra Python 3 ile:

```text
python Tools/Repository/verify_source_index.py --repository <ayrı-clone> --source-root <dondurulmuş-kopya> --manifest <source-publish-manifest.json> --base <origin-main-tam-commit> --build-source-manifest <final-source-manifest.json> --output <index-check.json>
```

Kaynak/index/çalışma ağacı byte eşleşmesi, GUID kapanımı, shader materyalleri, geçmiş yolların korunması, secret/kişisel dosya/cache/binary/LFS taramaları yapılır. Eski yayında dönüştürülmüş tarihsel dokümanları korumak için `--prior-state` ve `--prior-manifest` birlikte verilebilir. Yeni kaynak/paket durum metni gibi bilinçli bir uyarlama yalnız dar `--allow-adaptation <dosya>` ile listelenir; uyarlama JSON receipt içinde görünür ve ayrıca incelenir. Araç bütün farklara genel onay vermez.

Bu kontrolün geçmesi Unity/OS/insan oynanış veya lisans uygunluğu kabulü değildir. Uzak main/tag ve varsa release ZIP digest'i ayrıca doğrulanır. Normal push öncesi [güncelleme düzeni](../../Docs/RELEASE-WORKFLOW.md) tamamlanır; yarışma formuna gönderim yapılmaz.
