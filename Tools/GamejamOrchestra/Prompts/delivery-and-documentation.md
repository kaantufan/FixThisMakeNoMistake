# Too Late to Panic! — kalıcı teslim ve dokümantasyon

İzometrik ve FPS varyantları ayrı kaynak/build/skor kimlikleriyle sürer. Her proje root'u yalnız kendi projesinin canlı dosya, Unity/import/build/player ve test profili sahibidir. Alt ajan yalnız açıkça atanmış staging veya belge alanına yazar. [Ekip düzeni](../../../Docs/TOO-LATE-TO-PANIC-TEAM.md) ve seçilen projenin güncel AGENTS/task sözleşmesi uygulanır.

Her değişiklik paketi ilgili projenin Documentation, README ve tek güncel durum indeksine kaydedilir: tasarım kararı, yapılan işler, değişen dosyalar ve sahibi, kontroller/tuşlar, asset kaynağı/lisansı, build/sahne/profil/hash, gerçek kontrol kapsamı/sonucu ve bilinen eksikler. Ajan kısa handoff ve dosya SHA256 teslim eder. Kaynak hazırlığı, derleme, native kontrol, insan oynanışı ve görsel kabul ayrıdır; gözlenmeyen davranış tamamlanmış sayılmaz.

Yeni build gerçekten açılıp kısa etkilenen smoke geçtikten sonra root yalnız kendi projesinin Builds dizinindeki eski build ve portable ZIP adaylarını temizler. Her varyantta tek güncel çalışır çıktı, tek portable paket ve açık launcher/LATEST-BUILD.json kalır. Önceki çalışır çıktı yeni aday doğrulanmadan kaldırılmaz. Kaynaklar, lisanslar, gerçek kayıtlar ve küçük tarihsel metin/hash/receipt kanıtları korunur; gereksiz eski tam binary kopyaları biriktirilmez.

Windows recursive delete/move öncesinde çözülmüş absolute hedeflerin ilgili `TooLateToPanic/Builds/` veya `TooLateToPanicFPS/Builds/` sınırında kaldığı doğrulanır. Değişkenle geniş hedef silinmez, kardeş projenin dosyaları temizlenmez. Kullanıcının açık oyuncusu kapatılmaz; kullanımda dosya varsa güncel çalışır build korunarak durum bildirilir. Alt ajan temizlik veya Unity/player işlemi yapmaz. FPS/izometrik kaynaklar otomatik birleştirilmez.

Paketin tüm dahil işleri entegre ve frozen olduğunda root bir birleşik import/build ile tek 3–5 dakikalık ilgili smoke yapar. Somut hata onarımında gerekli dar tekrar uygulanır; geniş tur gün sonu veya yeni somut risk içindir. Tarihsel evidence kimlikleri ve eski path/hash/receipt içerikleri toplu değiştirilmez. Bu politika kalıcı proje/ajan talimatıdır; uygulamanın global system mesajını değiştirdiği iddia edilmez.

