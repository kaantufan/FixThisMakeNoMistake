> Source documentation snapshot. The promoted playable package at this snapshot is [Home V4.13 /0.4.13](https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.13). This push changes documentation only; draft labels and team-confirmation fields remain authoritative. Local Evidence links refer to the original development archive. No form submission, new build/native/OS test or playable release is performed. Historical version-specific observations remain as originally recorded.

# Too Late to Panic! — CIVIL GAME JAM teslim formu hazırlığı

**Hazırlanma:** 17 Ekim 2026, 19:55 (UTC+3) · **Dayanak:** Kural Kitapçığı v04, md. 3.4, 3.6, 3.7, 4.2, 5.2 ve Ek A.1–A.5 · **Durum:** taslak. `[ONAY]` ile işaretli alanlar takım tarafından doğrulanmadan forma girilmemeli.

Bu belge web formundaki alanlara kopyalanacak metni ve formdan önce yapılacak kontrolleri toplar. Form tek başına uygunluk onayı değildir (md. 5.5). Yanlış veya eksik AI/hazır kaynak beyanı diskalifiye nedenidir (md. 4.4, 10.1).

---

## 1. Repo – commit – oynanabilir sürüm eşleşmesi

### 1.1 Bugünkü doğrulanmış çift (17 Ekim 19:50 kontrolü)

| Alan | Değer | Nasıl doğrulandı |
|---|---|---|
| Public repo | https://github.com/kaantufan/FixThisMakeNoMistake | GitHub API: `visibility: public` |
| Release | `v0.4.13` — Home V4.13 | `gh release view` |
| **Teslim edilecek commit** | **`b551edfc6c39b9704f3a663be2d0cfe6e6c3836a`** | `v0.4.13` tag'inin işaret ettiği commit |
| Oyun bağlantısı | https://github.com/kaantufan/FixThisMakeNoMistake/releases/tag/v0.4.13 | Release sayfası; asset hesapsız indirilebilir olmalı |
| ZIP SHA256 | `E381BD36A9ABD96EF99E0051F0EC3392AFAEA3EC35C0156184E459964A08CCA7` (178.211.598 bayt) | Release asset digest = yerel `LATEST-BUILD.json` değeri |
| Kaynak ↔ build | 100 C# dosyasında 0 fark, seçili asset'lerde 0 fark | Repoda `TooLateToPanic/Documentation/Publications/v0.4.13/source-vs-build.json` |

**Kritik uyarı:** Forma **`main` dalının son commit'i yazılmamalı.** Otomatik yayın her 10 dakikada `main`'e kaynak snapshot'ı itiyor (şu an `945f9239…`, V4.14 ara kaynakları). Bu snapshot'lar yayımlanmış ZIP'ten **ileride**, yani md. 3.7'ye göre oynanabilir sürümle uyumsuz. Forma her zaman **teslim edilen ZIP'in release tag'inin commit'i** yazılmalı. Ek A.2 de dal adı veya etiket değil, tam commit kimliği istiyor.

### 1.2 Son teslimden önce tekrar çalıştırılacak kontrol

Son build V4.14 veya sonrası olursa, yeni release yayımlandıktan sonra aşağıdaki komutlarla aynı kontrol yeniden yapılmalı (`v0.4.13` yerine yeni tag yazılır):

```bash
gh api repos/kaantufan/FixThisMakeNoMistake/git/ref/tags/v0.4.13 --jq .object.sha
```

```bash
gh release view v0.4.13 -R kaantufan/FixThisMakeNoMistake --json assets --jq ".assets[] | .name + \"  \" + .digest"
```

```bash
gh api repos/kaantufan/FixThisMakeNoMistake --jq .visibility
```

Beklenen sonuç: tag commit'i forma yazılacak commit'le aynı; digest yerel `LATEST-BUILD.json` → `portableZipSha256` ile aynı; görünürlük `public`. Ayrıca indirme bağlantısı **çıkış yapılmış (gizli) tarayıcı penceresinde** açılıp ZIP'in indiği görülmeli (Ek A.1).

### 1.3 Zamanlama

- Git destek talebi için son saat **18 Ekim 12:00** (md. 6.3).
- Önerilen sürüm dondurma: **18 Ekim 15:00**. Son release, kontroller ve form kaydı en geç **17:00**'de bitsin. Sunucu kaydı 18:00'de kapanıyor; yalnız düğmeye basmak yetmiyor (md. 6.1).
- Formdan sonra makbuzdaki sürüm, saat ve bağlantılar kontrol edilmeli (md. 5.5).

---

## 2. Forma girilecek metinler

### 2.1 Oyun adı ve kısa açıklama

**Too Late to Panic!**

> Afet gelmeden önce oynanan bir hazırlık oyunu. İki katlı evini ve bahçeni gez; dolaplardan malzeme topla, mobilyaları duvara sabitle, duvar ve pencereleri güçlendir, drenaj kur. Süre bitmeden afet çantanı hazırla ve güvenli noktaya ulaş. Deprem, sel ve yangında aldığın önlemlerin sonucunu evin kendisinde gör.

### 2.2 Platform

Windows (64-bit).

### 2.3 Kurulum, çalıştırma ve kontroller (Ek A.3)

- **Gereksinim:** Windows 10/11 64-bit. Ek yazılım gerekmez, internet bağlantısı gerekmez.
- **Başlatma:** Release sayfasından `TooLateToPanic-Home-V4.13-20261017-portable.zip` dosyasını indirin. ZIP'i bir klasöre tamamen çıkarın. `TooLateToPanic.exe` dosyasını, yanındaki veri klasörleriyle birlikte, o klasörde çalıştırın. Windows SmartScreen uyarısında “Ek bilgi → Yine de çalıştır” seçilebilir (imzasız jam build'i).
- **Oynanış:** Başlıkta **HAZIRLIĞA BAŞLA** kısa (yaklaşık 5 dakikalık) hazırlık–afet–sonuç akışını açar. **SERBEST OYUN** dalgalı normal kampanyayı açar. Amaç süre bitmeden evi hazırlamak, çantaya en az 1 su + 1 gıda koymak ve güvenli noktaya ulaşmak.
- **Temel kontroller:** WASD/oklar hareket · Shift koşu · Space zıpla · E aç/kapat/etkileşim · Sol tık seç/vida tak · Sağ tık eşya taşı · Tab envanter/çanta · Enter onayla · Esc duraklat. Son 30 saniyede çanta aşaması birinci şahıs kameraya geçer: fare bakış, E/F erzak al.
- **Bilinen sınırlar:** `[ONAY]` Son insan oynanış testinden sonra doldurun. Örnek: 960×540 çözünürlükte küçük açıklamalar küçük kalır; normal kampanyada sel/yangın dalgalarında güvenli hedef bodrum sığınağıdır (kısa akışta afete göre ayrı hedefler vardır).

### 2.4 Hazır kaynak beyanı (md. 3.4, Ek A.4)

| Kaynak ve sağlayıcı | Kaynak bağlantısı | Lisans / kullanım izni | Oyundaki yeri ve değişiklik | Repoda bulunmayan dosyalar |
|---|---|---|---|---|
| Quaternius — Universal Base Characters (ücretsiz Standard) | https://quaternius.com | CC0 1.0 (repoda `Assets/Game/Resources/Licenses/Quaternius-Base.txt`) | Oyuncu, NPC ve kurtarma ekibi gövdesi/iskeleti. Kıyafet, saç ve renkler yerel kodla değiştirildi. | Yok |
| Quaternius — Universal Animation Library (Standard) | https://quaternius.com | CC0 1.0 (`Quaternius-Animation.txt`) | Idle/Walk/Jog/Jump/Interact/Fixing animasyonları | Yok |
| Google Fonts — Oswald, Barlow, Barlow Condensed | https://fonts.google.com | SIL Open Font License 1.1 (`TooLateToPanic/Documentation/Licenses/HUD/`) | HUD ve menü yazı tipleri, çevrimdışı Font asset'i olarak paketlendi | Yok |
| ElevenLabs — Text to Speech, Sound Effects, Music (üretilmiş çıktı; ayrıca AI beyanında) | https://elevenlabs.io | Ücretli Creator planı çıktısı, ElevenLabs ticari kullanım koşulları ([Creative Terms](https://elevenlabs.io/creative-terms), [Music Terms](https://elevenlabs.io/music-terms)). CC0/MIT olarak yeniden lisanslanmadı. | `Assets/Game/Resources/Audio/Jury` (7 Türkçe cümle + 3 efekt), `Audio/Foley` (efektler), `Audio/Music` (2 enstrümantal parça). Format/ses düzeyi dönüştürüldü, kısaltıldı. | Sağlayıcının orijinal indirmeleri repo dışında, kanıt arşivinde |

**Takım içi kaynaklar (hazır kaynak değil, takımın jam süresinde ürettiği; açıklık için beyan edilir):**

| Kaynak | Bağlantı | Oyundaki yeri |
|---|---|---|
| Yarkın — `yarkin-assets` dalı: 32 eşya modeli, ikon, materyal, sığınak kapağı | Repo commit'leri `eb05475e…`, `11f12b4c…`, `29bf309f…` | Su, gıda, ilk yardım, fener, pil, radyo, battaniye, yangın tüpü ve dekor modelleri (`Assets/Vendor/Yarkin`). Materyal renkleri ev paletine uyarlandı. |
| `defter-mockup` dalı: hazırlık defteri HTML tasarımı | Repo commit'leri `07ecbccb…`, `00377265…` | J defteri ekranının Unity uyarlaması |
| Ev, mobilya, iş düzenekleri, afet efektleri, logo, HUD | Bu repo, `Assets/Game/Art`, `Assets/Game/Runtime`, `Documentation/HUD-Concept` | Kodla prosedürel üretildi (AI beyanına bakın) |

**Beyan dışı tutulan, oyunda olmayan kaynaklar:** Microsoft Tolga (Windows TTS) ile üretilmiş eski 6 radyo kaydı build'e girmez ve repoya konmaz. V4.13 build'inde Meshy dosyası yoktur.

### 2.5 AI beyanı (md. 4.2, Ek A.5)

Kural gereği: araç, amaç, oyundaki kullanım yeri. AI kullanımı doğrudan puan etkisi doğurmaz (md. 4.3).

| Araç | Amaç | Oyundaki kullanım yeri |
|---|---|---|
| **OpenAI Codex ajanları** (GPT-6.1 Sol, GPT-6 Luna modelleri) `[ONAY: model/araç adlarını takım doğrulasın]` | Takım yönlendirmesiyle oyun kodunun yazılması; ev, mobilya, iş düzenekleri, afet efektleri ve ikonların prosedürel üretim kodu; Unity editör/build betikleri; HUD uyarlaması; logo üretim betiği; otomatik test ve oyun içi test sürüşü (Computer Use); tasarım ve teslim belgeleri | `TooLateToPanic/Assets/Game/**` (Runtime, Art, Editor), `Documentation/**`, `Documentation/HUD-Concept/logo` |
| **Antigravity AI Agent** (Yarkın) | 32 eşya modelini, renk atlasını, materyalleri ve envanter ikonlarını üreten Blender/Unity betiklerinin yazılması; eşya metinleri | `TooLateToPanic/Assets/Vendor/Yarkin/**` (beyan: `Assets/Vendor/Yarkin/Documentation/UPSTREAM-ASSET-MANIFEST.md`) |
| **ElevenLabs** — Text to Speech (Eleven Multilingual v2, hazır “Mia” sesi; ses klonlama yok) | Kısa hazırlık akışındaki 7 Türkçe yönlendirme cümlesinin seslendirilmesi. Metinler takım tarafından yazıldı. | `Assets/Game/Resources/Audio/Jury` |
| **ElevenLabs** — Sound Effects | Kısa akış efektleri, adım/etkileşim/arayüz/ortam efektleri | `Audio/Jury`, `Audio/Foley` |
| **ElevenLabs** — Music v2.5 (enstrümantal) | 2 arka plan müziği (hazırlık/menü ve afet gerilimi) | `Audio/Music` |
| **Meshy AI** (image-to-3D) `[ONAY: kim, hangi planla üretti?]` | Karakter görünüm referansı modeli. **V4.13 build'inde yok.** V4.14 kaynaklarında arşivde duruyor, aktif oyuncu/NPC değil. | Yalnız V4.14 ve sonrası teslim edilirse beyan edilir |
| **Anthropic Claude** (Claude Code) | Tür/forum araştırması, kural uyum kontrolü ve teslim beyanı taslağı | `Docs/TOO-LATE-TO-PANIC-ELESTIREL-ARASTIRMA-2026-10-17.md`, bu belge |
| `[ONAY]` HUD HTML konsepti | Hangi araçla hazırlandığı kaynakta yazmıyor; takım doğrulasın | `Documentation/HUD-Concept/index.html` → oyundaki HUD tasarımı |

**Forma yazılabilecek kısa özet metin:**

> Oyunun kodu, prosedürel 3D ev/eşya/efekt üretim kodu, HUD ve belgeleri takımın yönlendirmesiyle OpenAI Codex ajanlarıyla yazıldı; otomatik testlerde de kullanıldı. Eşya modelleri ve ikonları Antigravity AI Agent ile yazılan Blender/Unity betikleriyle üretildi. Kısa akıştaki 7 Türkçe cümle (ElevenLabs Text to Speech, hazır ses, klon yok), ses efektleri (ElevenLabs Sound Effects) ve 2 enstrümantal müzik (ElevenLabs Music) AI ile üretildi. Araştırma ve teslim belgeleri için Anthropic Claude kullanıldı. Ayrıntılı tablo: repo `Docs/TOO-LATE-TO-PANIC-JAM-TESLIM-BEYANI.md`.

---

## 3. Repo içeriği ve lisans riski (md. 3.6)

| Kontrol | Sonuç |
|---|---|
| `v0.4.13` commit ağacında “meshy” geçen dosya | Yok (3.627 dosya tarandı) |
| `main` ağacında “meshy” geçen dosya | Yok (7.626 dosya tarandı). `defter-mockup` dalında Meshy FBX'i ayrıca public durumda. |
| Lisans metinleri repoda | Quaternius ×2 (CC0), OFL ×3, ElevenLabs Audio ×3 provenans, Yarkın beyanı |
| ElevenLabs çıktıları | Ücretli planla üretildiği kayıtlı. Ticari kullanım izinli; repoda paylaşım sağlayıcı koşullarına tabi. |

**Meshy riski:** Meshy ücretsiz planda üretilen modeller **CC BY 4.0**'dır; kullanım serbesttir ama **Meshy'ye atıf zorunludur**. Ücretli planda çıktı kullanıcıya aittir ([Meshy fiyat/lisans](https://docs.meshy.ai/en/webapp/pricing), [Meshy yardım](https://intercom.help/meshy/en/articles/16102098-can-i-use-meshy-assets-commercially)). Model ücretsiz planla üretildiyse V4.14 teslim edilecekse beyana “Meshy, CC BY 4.0” atfı eklenmeli. Hangi planla üretildiği bilinmiyorsa en güvenlisi atfı eklemektir.

**Not:** “Yaşam üçgeni” etiketi Yarkın'ın upstream eşya listesinde (`PF_SturdyTable`) geçiyor; oyun kodunda bu metni bulmadım. Bu ifade çürütülmüş bir öneri olduğu için oyuncuya gösterilmemeli.

---

## 4. Teslim günü kontrol listesi (Kitapçık s.10'dan, bu projeye göre)

- [ ] Son build seçildi (V4.13 mü, sonraki bir aday mı) ve dondurma saati ilan edildi.
- [ ] Son release yayımlandı; tag commit'i, ZIP digest'i ve public görünürlük §1.2 komutlarıyla doğrulandı.
- [ ] İndirme bağlantısı gizli pencerede, hesapsız açıldı.
- [ ] Forma `main` değil **release tag commit'i** yazıldı.
- [ ] §2.4 hazır kaynak beyanı ve §2.5 AI beyanı `[ONAY]` alanları doldurularak girildi.
- [ ] Meshy dosyası içeren bir sürüm teslim ediliyorsa Meshy satırı ve gerekiyorsa CC BY 4.0 atfı eklendi.
- [ ] Hak sahipleri kullanım iznini (md. 9) biliyor; temsilci yetkili.
- [ ] Makbuz alındı; sürüm, saat ve bağlantılar kontrol edildi.
- [ ] 18:00'den sonra release ve tag değiştirilmeyecek; otomatik yayın sadece `main`'e kaynak itse bile teslim tag'ine dokunmayacak.
