# E2E yürütme ve düzeltme şablonları

Bu dosya planın ekidir. Aşağıdaki alanlar doldurulmadan test yapılmış veya hata kapanmış sayılmaz. Örnek alan adları sonuç değildir. Her yeni koşuda özgün bir kopya kullanılır.

## 1. Davranış profili

```json
{
  "profile_id": "<yeni-kimlik>",
  "captured_utc": "<UTC>",
  "contract_path": "<aktif sözleşme>",
  "contract_sha256": "<SHA256>",
  "source_snapshot": "<manifest>",
  "source_sha256": "<SHA256>",
  "build_id": "<aday>",
  "dll_sha256": "<SHA256>",
  "behaviors": {
    "anchor": "<V4.9 iki gerçek vida/0.60 sn veya açık legacy profil>",
    "non_anchor_install": "<geçerli kural>",
    "item_action_rules": "<izin kaynağı>",
    "fire_station": "<geçerli hedef>",
    "tree_regions": "<geçerli sayım>",
    "walk_run": "<geçerli hızlar>",
    "cost_cancel": "<iptal/commit önceliği>",
    "short_flow_clock": "<içerik/mutlak süre davranışı>"
  },
  "legacy_only_case_ids": [],
  "superseded_case_ids": [],
  "not_applicable": [],
  "unresolved_contract_questions": []
}
```

## 2. Koşu başlık kaydı

| Alan | Doldurulacak |
|---|---|
| run_id / case_id / alt hedefler | |
| Testçi ve rol | |
| Başlangıç/bitiş UTC ve yerel saat | |
| Kanıt katmanı | SOURCE / MODEL / NATIVE_API / OS_E2E / HUMAN |
| Sözleşme / davranış profili / SHA256 | |
| Kaynak snapshotı / manifest hash | |
| EXE / DLL / asset cache / ZIP hash | |
| Mod / sahne / faz / hedef | |
| GUID test profili / seed / önkoşul | |
| Makine / kalite / çözünürlük / DPI | |
| Gerçek kayıt önce–sonra hashleri | |
| Test Warp / injected Tick / dev mutation sayısı | |
| Oyun sahipli spawn/sinematik yer değiştirmeleri | |
| Girdi yöntemi | Donanım / OS otomasyon / üretim API / GUI fixture |
| Ham log / video / PNG / JSON dosyaları | |
| Sonuç | NOT_RUN / PASS / FAIL / BLOCKED / NOT_APPLICABLE / INVALID_TEST |
| Sonuç gerekçesi ve sınır | |
| Developer kararı / kanıt | |
| Bağımsız QA kararı / kanıt | |
| Alıcı / görüntü yorumu | Render yoksa gerekçeli N/A + bağlı görsel/OS vakası |
| Hikâye–tema kararı | İlgili zincir veya gerekçeli N/A |

## 3. Ayrıntılı test vakası

```text
Vaka: <id — kısa davranış>
Gereksinim: <Rxx + aktif sözleşme satırı/bölümü>
Sürüm/uygulanabilirlik: <aktif/legacy/koşul>
Katman: <kanıt türü>
Önkoşul: <gerçek state; sonlu stok; faz; hedef; kamera>
Veri: <bütçe, gerekli/var malzeme, yuva, seed>

1. <gerçek girdi ve hedef>
   Beklenen: <durum + dünya + UI + maliyet>
2. <bir sonraki girdi>
   Beklenen: <ölçülebilir sonuç>
3. <iptal/tekrar/negatif kol>
   Beklenen: <aynı anda değişmemesi/değişmesi gereken alanlar>

Kontrol noktaları: <önce → işlem → commit → sonra → reset>
Kanıt: <klip zamanları, temiz PNG, JSON/log>
Regresyon: <komşu vaka kimlikleri>
Gerçek sonuç: <görülen, miktar ve süre>
Sonuç: <durum>
Issue: <varsa kimlik>
```

## 4. Bug / UX / görsel / tema kaydı

```text
issue_id:
Başlık: <tetik ve kullanıcı etkisi>
Tür: FUNCTIONAL / UX / VISUAL / AUDIO / THEME / PERFORMANCE / DATA / HARNESS
Kanıt durumu: CONFIRMED_ON_BUILD / SOURCE_CANDIDATE / HISTORICAL_REVERIFY / REVIEWER_OPINION
Durum: NEW / TRIAGED / FIX_PLANNED / IN_PROGRESS / FIXED_SOURCE / BUILT / RETEST_PASS / VERIFIED_CLOSED / REOPENED / DEFERRED / SUPERSEDED_BY_CONTRACT
Severity / priority:
Etkilenen mod / faz / hedef / katılımcı:
Contract / source / DLL / run_id / case_id:
Önkoşul:
Adımlar:
Beklenen:
Olan:
Tekrar oranı: <başarısız/denenen; seed ve değişen koşul>
Oyuncu etkisi: <ilerleyememe, para kaybı, yanlış anlama, güven/okunurluk>
Kanıt: <tam kare + klip anı + log/JSON>
Kök neden hipotezi: <kanıtlanmadıysa açıkça yaz>
Bağımsız doğrulama:
İlgili diğer hedefler:
Önerilen en küçük düzeltme:
Korunacak ekonomi/fizik/kayıp/kayıt değişmezleri:
Owner / batch:
Kabul vaka ve regresyonları:
Önce–sonra karşılaştırma:
Yeniden test sonucu:
Kalan sınır / erteleme gerekçesi:
```

## 5. Somut fix paketi

```text
batch_id / başlık:
Çözülecek doğrulanmış issue_id'ler:
Kullanıcıya görünen önce → sonra:
Kök neden ve kanıt:
Dosya/asset/API alanı, tek yazar:
Alınacak staging paketi ve kaynak hashleri:
Müdahale sınırı ve korunacak kurallar:
Uygulama adımları:
Test aracı değişikliği gerekiyorsa ayrı gerekçe:
Risk ve komşu mekanikler:
Rollback kaynak snapshotı / retained çalışan build:
Kabul vakaları ve ölçütleri:
3–5 dk ilgili smoke rotası:
Görsel önce–sonra tam kare / hareket klibi / dinleme protokolü:
İnsan tekrarının gerekliliği:
Tek birleşik import/bake/build kimliği:
QA bağımsız tekrarı:
Sonuç / açıklar / yeni risk:
```

Örnek planlama maddesi: V4.8 tesliminde kayıtlı etiket çakışması **HISTORICAL_REVERIFY** olarak alınır. Önce yeni adayda aynı kadrajda üretilir. Üretilirse etiket yerleşim/öncelik/kenar payı kök nedeni doğrulanır; sadece etiketleri kaldırmak başarı değildir. Sayılar ve hedef bağlantısı korunarak ilgili çözünürlük/kamera tekrarları seçilir. Bu örnek bu turda yapılmış fix veya yeni bug kabulü değildir.

## 6. Görüntü ve alıcı inceleme kaydı

| Alan | Doldurulacak |
|---|---|
| screen_id / case_id / build / DLL / PNG SHA256 | |
| Faz / mod / hedef / kamera / zoom / kat | |
| Çözünürlük / DPI / kalite | |
| Temiz tam kare / işaretli kopya / hareket klibi | |
| İlk 3 saniyede gördüğüm odak | |
| Anladığım eylem ve tehlike | |
| Nesne/oda kimliği ve malzeme | |
| Somut kusur ve ekran koordinatı | |
| Model/state ile görsel uyuşması | |
| Hikâye/amaç/tema yorumu | |
| Alıcı olarak devam etme isteği ve nedeni | |
| Görüş türü | reviewer_opinion / participant_observation / objective_defect |
| 0–4 puanlar ve gerekçe | Odak, biçim, ışık, hareket, UI, afet, önlem |
| En küçük düzeltme | |
| Aynı kadraj retest ölçütü | |

Tek ekran için görüntü ile anlaşılamayan ses/zaman/animasyon kısmı `UNVERIFIED` kalır. Screenshot'a bakarak "hareket akıcı" veya "ses kaliteli" kararı verilmez.

## 7. Hikâye ve tema zinciri kaydı

```text
chain_id / ilgili özellik:
Oyuncunun mevcut amacı:
Gerçek eylem:
Modeldeki state/maliyet değişimi:
Dünyada görünen kalıcı iz:
Afetin gerçek çözümü ve görseli:
Sonuç/karne cümlesi:
Oyuncunun kendi sözleriyle açıklaması:
Olası yanlış öğrenme/çelişki:
Resmi kaynak URL + erişim tarihi + tam ilgili iddia:
Oyun soyutlaması ve hesaplanmayan iddia:
Sonuç: TUTARLI / EKSİK_BAĞ / ÇELİŞKİ / HENÜZ_İNSANLA_DOĞRULANMADI
Issue ve düzeltme ölçütü:
```

## 8. İlk oyuncu oturumu

```text
participant_id: <anonim kod>
Oyuncu deneyimi / oyunu daha önce gördü mü:
Makine / build / dil / ses ayarı:
Koşu: UNASSISTED / ASSISTED / ABANDONED
Dış saat başlangıcı: <exe veya başlık/start; kapsam açık>
İlk eylem / pickup / çantaya koyma / önlem / afet sonucu / sel / yangın / karne:
Her kilometre taşının gerçek saniyesi:
Duraksamalar: <başlangıç, süre, ekran, aranan şey>
Hatalar: <yanlış hedef/tuş/varsayım>
Yardım: <an, sorulan, verilen tam söz>
Atlanan/eksik eylemler:
180 / 270 / 300 sn ölçütü:
Amacı ve önlemin karşılığını kendi açıklaması:
Görüntü/ses/tempo yorumları:
Devam isteği 1–5 + serbest gerekçe:
Hangi sonuç gözlem, hangi sonuç yorum:
Önerilen düzeltme ve tekrar sorusu:
```

## 9. Sonuç ve teslim raporu

1. Final contract/source/DLL/ZIP kimliği ve doğrulama tarihi.
2. Uygulanabilir / PASS / FAIL / BLOCKED / NOT_RUN / INVALID_TEST sayıları; yöntem başına ayrı dağılım.
3. P0–P3 açık ve kapalı sorunlar; test aracı kusurları ayrı.
4. Gerçek OS yolları ve doğal/fixture kampanya ayrımı.
5. İnsan süreleri/yardım/keyif/anlama; dinleme ve görsel puan gerekçeleri.
6. Mekanik–hikâye–tema zincirleri; resmi kaynağa bağlı çelişkiler.
7. Fix paketleri, gerçek retest ve önce–sonra bağlantıları.
8. Performans/test cihazı, paket/kayıt/lisans kontrolü.
9. G0–G8 kapılarının ayrı kararları ve kalan sınırlar.
10. Kaynak / build / smoke / OS / insan / sanat / ses için ayrı kabul cümlesi.

## 10. Teslimde kullanılmayacak ifadeler

"Her şey test edildi", "sıfır hata", "oyuncular kesin sever", "tam gerçekçi afet", "yeni kaynak eski149/149 ile geçti" veya "screenshot varsa OS girdi kabulü var" yazılmaz. Bunun yerine hangi sürümde hangi yolun hangi yöntemle ölçüldüğü ve neyin açık kaldığı belirtilir.
