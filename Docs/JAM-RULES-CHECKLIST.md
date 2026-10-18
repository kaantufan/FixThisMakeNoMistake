# Too Late to Panic! - yarışma ve teslim kontrolü

18 Ekim 2026. **Yerel hedef Home V5.9-r7 /0.5.9. Henüz hiçbir yarışma teslimi yapılmadı.** Bu liste GitHub/belge düzenlemesinin parçasıdır; form gönderimi veya uygunluk kararı değildir.

## Güncel dayanak ve eski bilgi ayrımı

Güncel teslim kuralları, kullanıcının **18 Ekim'de doğrudan ilettiği resmî organizasyon duyurusundan** alınmıştır. Resmî adres: [game.ktimo.com.tr](https://game.ktimo.com.tr/). Kullanıcı metninin kanıt türü doğrudan iletilmiş organizasyon duyurusudur; bağımsız olarak form hesabına girildiği veya yükleme denendiği iddia edilmez.

| Konu | Güncel duyuru | Eski v05 PDF ile fark |
|---|---|---|
| Teslim aralığı | **18 Ekim 2026 12.00-18.15 UTC+3**, takım PIN'iyle | PDF s.5 madde 6.1'deki 18.00 kapanışı bu iş için kullanılmaz |
| Oynanabilir sürüm | Windows oyunu **ZIP olarak yüklenir**; web build için web adresi | PDF'nin genel oynanabilir HTTPS bağlantısı ifadesi Windows dosya yüklemesinin yerine geçirilmez |
| Video | **En fazla 1 dakika**, oyun görüntüsü veya fragman, **MP4/AVI**; ZIP'ten ayrı yükleme | PDF s.4/9'daki en az 1 dakika ve video HTTPS bağlantısı talimatı güncel yükleme şeklinin yerine geçirilmez |
| Kaynak | **Public Git kaynak repo** | Kaynak/build eşleşmesi ve tam commit alanları final formda kontrol edilir |
| AI | **Görsel / Ses / Kod / Metin / Diğer** için ayrı Kullanıldı/Kullanılmadı seçimi ve en küçük kullanımın ayrıntısı | Genel tek paragraf beyan yeterli alan doldurma sayılmaz |
| Hazır kaynak | Üçüncü taraf assetler ayrıca beyan edilir | AI tablosu, lisans ve hazır kaynak tablosunun yerine geçmez |
| Sahne | **Video dahil toplam 3 dakika**; performans puanlanmaz, jüri oyunu izler ve oynar | PDF s.6/9'daki 5 dakika sunum bu iş için kullanılmaz |
| Son kontrol | Yüklemelerden sonra doğrulama ve **makbuz kontrolü** | Yerel dosya veya GitHub yayını yapılmış teslim değildir |

Tarihsel kaynaklar: [resmî v05 PDF](https://game.ktimo.com.tr/assets/CIVIL_GAME_JAM_Kural_Kitapcigi_v05.pdf), 10 sayfa, 18 Ekim HTTP200, SHA256 `D8F6F54137C0EE9A4ADFEC9BBF15206F10C8DD5DF536560752D18FFE86DA761B`; önceki v04 SHA256 `BE463699B6148A37E9E837038383D415C698F20F76D513ADC6CBB2A73A36CEAC`. v05'in değişmeyen kaynak/AI/izin/commit maddelerinin sayfaları aşağıda referans olarak korunur; güncel duyuruyla çelişen şartlar kullanılmaz. İlk iletilen Murat Oktay konuşması 8 sayfalık yaratıcı değerlendirmedir; kural kitabı veya oyuna karınca/bardak su ekleme şartı değildir.

## Duyuru/madde - gerçek dosya - durum eşlemesi

**AÇIK** bir iş, organizatör tarafından verilmiş ihlal kararı değildir. Burada yalnız kapanması için gereken kanıtı belirtir.

| Kontrol | Dayanak | Gerçek proje kanıtı | Durum / gereken kanıt |
|---|---|---|---|
| Kesin takım, katılım ve temsilci | v05 s.2 madde 1.2, 2.1-2.4 | `Docs/TOO-LATE-TO-PANIC-TEAM.md` geliştirme rol kaydıdır; resmî katılım onayı değildir | **DOĞRULANMADI** - kesin takım ve yetkili temsilci teyidi |
| Oyuna özgü üretim zamanı / hazır kaynak ayrımı | v05 s.2 madde 1.3; s.3 madde 3.1 | Tarihli sözleşme/source manifest/receipt'ler | **AÇIK** - 16 Ekim 18.00 UTC+3 öncesi oyuna özgü çalışma varsa niteliğini açıklama; organizatör kararı varsayılmaz |
| Yerel Windows paketi | Güncel duyuru; v05 s.3 madde 3.2 | `TooLateToPanic/LATEST-BUILD.json`, r7 teslim kaydı ve portable ZIP | **YEREL KANIT VAR** - Windows build/API/render; farklı makine ve insan turu ayrı |
| Kurulum/kontrol/bilinen sınır metni | v05 s.9 A.3 | README, `TOO-LATE-TO-PANIC-JAM-TESLIM-BEYANI.md`, oyun içi F1 | **METİN HAZIR** - final paket adıyla son eşleştirme |
| Oynanabilir ZIP'in yüklenmesi | Güncel duyuru | r7 yerel ZIP/SHA256; r5 public release ayrı | **YAPILMADI** - kullanıcı hiçbir şey göndermediğini belirtti; yükleme bu işin kapsamı değil |
| Public Git kaynak repo / tam commit / aynı build | Güncel duyuru; v05 s.3 madde 3.6-3.7; s.9 A.2 | Önceki r5 yayın receipt'i; r7 kaynak/build manifestleri | **AÇIK** - seçilen final paketin eşleşen public tam commit'i ve hesapsız erişim kontrolü; dal/etiket kullanılmaz |
| En fazla 60 s MP4/AVI video; ayrı yükleme | Güncel duyuru | Bu görevde doğrulanmış final video yok; 69 görüntü video değildir | **HAZIRLIK/YÜKLEME YAPILMADI** - oyun görüntüsü/fragman; süre/format ve final sürümü kontrol edilir |
| Video dahil 180 s sahne planı | Güncel duyuru | `JAM-3-DAKIKA-SUNUM.md` taslağı | **PROVA NOT_RUN** - metin/prova tamamlandı demek değildir |
| Hazır kaynakların tam beyanı | Güncel duyuru; v05 s.3 madde 3.3-3.5; s.9 A.4 | `ASSET-CREDITS.md`, oyuncu/HUD/ses/Yarkın provenansı; teslim beyanı | **AÇIK** - final dosya listesinde her kaynak/izin/kullanım/değişiklik eşleştirilsin |
| Kısıtlı ham assetlerin public dağıtımı | v05 s.3 madde 3.6; s.7 madde 9.7 | ASSET-CREDITS Meshy/bunker yeniden dağıtım iznini açık bırakır; ElevenLabs CC0/MIT değildir | **AÇIK** - Meshy üreten kişi/plan, Yarkın hak sahibi izni, ses ham dosya koşulları; atıf izin yerine geçmez |
| Beş ayrı AI kategorisi ve ayrıntı | Güncel duyuru; v05 s.3 madde 4.1-4.4 | `TOO-LATE-TO-PANIC-JAM-TESLIM-BEYANI.md` beş kategori tablosu + upstream kayıtları | **TASLAK VAR** - bütün kategorilerde Kullanıldı; HUD HTML aracı ve ek takım araçları teyidi açık |
| Gizli bilgi/anahtar/kişisel dosyalar | v05 s.3 madde 3.7 | Final yayın allowlist ve ilgili secret scan receipt'i gerekir | **BU BELGEYLE DOĞRULANMADI** - final public ağaç kontrolü |
| Hak sahipleri ve temsilci yetkisi | v05 s.7 madde 9.1-9.8 | Provenance veya AI tablosu ekip adına izin vermez | **DOĞRULANMADI** - gerçek hak sahipleri/temsilci teyidi |
| Form doğrulama ve teslim makbuzu | Güncel duyuru; v05 s.4 madde 5.4-5.5 | Kullanıcı beyanına göre teslim yok | **YAPILMADI** - bu görev yükleme/form/mesaj işlemi yapmaz |
| Teslim sürümünü ve erişimi koruma | v05 s.4 madde 5.6 | Önceki otomatik yayın durdurma receipt'i | **AÇIK** - final teslim yapıldığında ilgili paket/commit/video/makbuz kimlikleri sabit tutulacak |

## Son teslimden önce temsilcinin yapacağı işler

- [ ] Kesin takım/temsilci ve hak sahibi izinleri ile üretim zamanı açıklamaları tamamlandı.
- [ ] Tek final oyun paketi seçildi; yerel r7 ile public r5 karıştırılmadı. Public Git'te aynı kaynak ve tam commit doğrulandı.
- [ ] Hazır kaynak tablosu, ham asset paylaşım koşulları ve son public ağacın gizli bilgi taraması tamamlandı.
- [ ] Görsel/ses/kod/metin/diğer AI alanlarının hepsi doğru seçim ve ayrıntıyla doldurulmaya hazır; bilinmeyen araçlar açıklığa kavuştu.
- [ ] Oynanabilir paket ZIP, ayrı video MP4/AVI; video en fazla 60 s ve oyun görüntüsü/fragman içeriyor. İkisinin sürümü kontrol edildi.
- [ ] Video dahil 180 s sunum prova edildi; insan oynanış/dinleme/okunurluk sonuçları teknik sayılardan ayrı tutuldu.
- [ ] Temsilci 12.00-18.15 UTC+3 arasında takım PIN'iyle resmî sitede yüklemeleri yaptı; doğrulama hatalarını giderdi.
- [ ] Başarılı makbuz alındı; numara, sürüm, sunucu saati, yüklenen dosyalar ve kaynak bilgileri kontrol edildi.
- [ ] Değerlendirilecek paket/commit/video ve erişim, teslim sonrasında korundu.

## Sürüm ve depo düzeni sonraki güncellemelerde

Güncel kabul `TooLateToPanic/LATEST-BUILD.json` ve işaret ettiği teslim belgesidir. README, teslim beyanı ve bu liste aynı yerel/yayın kimliğine bağlanmalıdır. Yeni sürüm çıktığında teknik sayılar, DLL/ZIP/hash/commit ve kalan insan kabulü alanları birlikte yenilenir; eski sonuçlar yeni DLL'e taşınmaz. Önceki küçük beyan/receipt kayıtları ayrı tarihsel arşivde korunur. Lisans, vendor provenansı, Unity GUID'leri, kaynak/kayıtlar ve bağımsız FPS projesi temizliğin kapsamına alınmaz. Gönderim/yayın yapılınca yalnız ilgili alan gerçek makbuzla güncellenir; yapılmış sayılmaz.

R7 indeksi 472 Editor, 524 native UI/yerel parlama ve 69 incelenmiş framebuffer karesi bildirir. R5'in 943 kontrolü tarihsel kalır. Fiziksel OS girdisi, insan dinleme/konfor/okunurluk ve 6-7 dalga fiziksel kampanya kabulü NOT_RUN. Bu sayılar yarışma uygunluğu, lisans izni, video, prova veya teslim makbuzu yerine geçmez.
