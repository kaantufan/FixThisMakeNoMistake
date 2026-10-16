# Too Late to Panic! — oyuncu ve animasyon provenansı

16 Ekim 2026 tarihinde bu projedeki yerel lisans metinleri ve kaynak yolları kontrol edildi. Bu kayıt yalnız kullanılan Quaternius oyuncu/animasyon tabanını açıklar; önceki oyunların asset envanteri veya kabul sonuçlarını bu projeye taşımaz.

| Kaynak | Projedeki dosya | Yerel lisans |
| --- | --- | --- |
| Quaternius Universal Base Characters Kit, ücretsiz Standard | `Assets/Vendor/Player/Superhero_Male_FullBody.fbx`, `Hair_SimpleParted.fbx` ve `Textures/` | [Quaternius-Base.txt](../Assets/Game/Resources/Licenses/Quaternius-Base.txt) |
| Quaternius Universal Animation Library, Standard | `Assets/Vendor/Player/UAL1_Standard.fbx` | [Quaternius-Animation.txt](../Assets/Game/Resources/Licenses/Quaternius-Animation.txt) |

İki yerel metin de **CC0 1.0 Universal / Public Domain Dedication** ve Quaternius adını içerir. Base metni bunun ücretsiz Standard alt kümesi olduğunu açıkça söyler. Ücretli SOURCE/Pro paketi, özel engine örneği veya tüm pazarlanan kliplerin kullanıldığı iddia edilmez. Metinlerin içerdiği CC0 adresi `https://creativecommons.org/publicdomain/zero/1.0/`'dır; bu kontrolde dış web sayfası yeniden doğrulanmadı.

| Yerel lisans | 16 Ekim 2026 SHA256 |
| --- | --- |
| `Assets/Game/Resources/Licenses/Quaternius-Base.txt` | `0F4BEAF0FE360A7732E58BBE3DBF60A2422367FBEA60CB9EA4ADD968F383268E` |
| `Assets/Game/Resources/Licenses/Quaternius-Animation.txt` | `6D01F55C6E4C49A2C9963E147E561945AE2C83958C8CA667D90A6BFFDBFAC061` |

`PanicHomeBuild.BakePlayer` erkek gövdesini yükler; Standard animasyon dosyasından Idle/Walk/Jog ile ayakta/diz çökerek iş kliplerini seçip `Assets/Game/Resources/Player/SurvivorV2.prefab` ve Animator üretir. İş kıyafeti/saç/bot/eldiven düzenlemesi yerel `PanicCharacterArt` çalışmasıdır. Kaynakta dosya/klip bulunması her klibin native kabul gördüğü anlamına gelmez.

Home V2'nin gerçek build/native kapsamı [teslim kaydında](HOME-V2-DELIVERY-2026-10-16.md) bulunur. Home V3 kaynak çalışmasının kabulü ayrı kaydedilmelidir. Lisans metinlerini, ilgili vendor dosyalarını ve küçük hash/receipt izini koru.
