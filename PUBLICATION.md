# Kaynak ve oynanabilir paket durumu

Güncel yerel ve GitHub oynanabilir paket **Home V5.9-r7 /0.5.9**: [v0.5.9-r7 ön sürümü](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.5.9-r7) · [Windows portable ZIP](https://github.com/kaantufan/FixThisMakeNoMistake/releases/download/v0.5.9-r7/TooLateToPanic-Home-V5.9-r7-20261018-portable.zip). R7, `requiresUserPlaytest: true` durumuyla yayımlanmış ön sürümdür. [LATEST-BUILD.json](TooLateToPanic/LATEST-BUILD.json) ve [R7 teslim kaydı](TooLateToPanic/Documentation/HOME-V5.9-LOCAL-GLOW-DELIVERY-2026-10-18.md) yerel teknik kabulün kaydıdır.

| Kimlik | Değer |
| --- | --- |
| R7 DLL | `68BB4573E7ECC81250E2EB17488DF92987F3B0CFF2129C0CCA21C8B954AB5F29` |
| R7 portable ZIP | `7A9031DBE96EA6FD291FED2911A0FCE1825F015A4997D017BEB1F93F4EC3D3A9` |
| R7 portable ZIP boyutu | 215.324.804 bayt |

R7 paketi ve `v0.5.9-r7` etiketi, kabul edilmiş kaynak commit'i [`a1cb8e2b6a790d3a16df8c2b0146bf316d74eee9`](https://github.com/kaantufan/FixThisMakeNoMistake/commit/a1cb8e2b6a790d3a16df8c2b0146bf316d74eee9) ile bağlıdır. GitHub'daki ZIP'in SHA256 değeri ve boyutu yukarıdaki mevcut yerel paketle eşleşir; [R7 yayın makbuzu](TooLateToPanic/Documentation/Publications/v0.5.9-r7/release.json) ayrı kaydedilir. Bu dosyayı içeren sonraki belge commit'i kaynak commit'inden farklı olabilir; `git rev-parse HEAD` tek başına R7 paketinin kaynak kimliği değildir. Belge commit'i yeni oyun build'i veya yeniden test anlamına gelmez. [Önceki kaynak/build kimlik kaydı](TooLateToPanic/Documentation/Publications/repository-layout-2026-10-18/source-and-build.json), 213 C#/shader dosyasını R7'nin dondurulmuş kaynak hashleriyle bağlayan tarihsel kaynak snapshot kaydı olarak değiştirilmeden korunur.

Önceki [v0.5.9-r5](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.5.9-r5), commit `873422231245baf6f354c8a3a80220e6006e2c54`, ZIP SHA256 `4738918CAF0BC26150ACC5F72C331D2648633661EB32708677F75D67DF9EAFBF` olarak tarihsel kayıtta korunur. R5 release/tag/asset'i değiştirilmez. R7 ön sürüm olarak yayımlandığı için önceki stable/latest [v0.4.13](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.13) seçimi korunur. [Release listesi](https://github.com/kaantufan/FixThisMakeNoMistake/releases).

R7'nin 472 Editor /524 native UI kontrolü ve 69 görüntüsü mevcut sürüme bağlı teknik kayıttır; bu yayın/belge işlemi yeni build/native/OS/insan testi değildir. R5'in 943 kontrolü R7 DLL'ine aktarılmaz. Fiziksel OS girdisi, insan oynanışı/dinleme/okunurluk/konfor ve 6–7 dalga fiziksel kampanya kabulü NOT_RUN kalır. Yarışma sistemine gönderim yapılmamıştır; video oluşturma/yükleme ve sunum provası yapılmış sayılmaz. Kaynak ve asset lisansları kendi dosyalarında korunur; yeni genel lisans verilmez.
