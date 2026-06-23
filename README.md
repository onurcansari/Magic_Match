# Match3 Game in Unity

![Match3gif](/GameDemo/Match3.gif)

Unity (6000.3.11f1) ile geliştirilmiş, fantastik temalı bir **Match-3** bulmaca oyunu. Klasik "hamle sınırlı / hedef skor" seviyelerinin yanında, düşman düğümlerini güç biriktirerek aşındırdığınız bir **Battle Path** (sefer haritası) modu içerir. Proje; grid tabanlı oyun tahtası, kalıtım üzerine kurulu farklı taş tipleri (renkli, donmuş, engelli), kademeli (cascade) skor sistemi, yıldız/hedef skor mekaniği, sahne geçiş efektleri, ses yönetimi ve ödüllü reklam entegrasyonu (AdMob'a hazır, SDK kurulmadan da çalışır) gibi tam bir mobil oyunun parçalarını barındırır.

![Match3](/GameDemo/match3.png)

![Match3 - Finished Level](/GameDemo/match3Final.png)

## İçindekiler

- [Oyun Hakkında](#oyun-hakkında)
- [Oyun Modları](#oyun-modları)
- [Öne Çıkan Özellikler](#öne-çıkan-özellikler)
- [Proje Yapısı](#proje-yapısı)
- [Script Mimarisi](#script-mimarisi)
- [Sahneler](#sahneler)
- [Veri Kalıcılığı (PlayerPrefs)](#veri-kalıcılığı-playerprefs)
- [Reklam Entegrasyonu](#reklam-entegrasyonu)
- [Kurulum ve Çalıştırma](#kurulum-ve-çalıştırma)
- [Geliştirme Notları](#geliştirme-notları)

## Oyun Hakkında

Match-3 oyunlarında amaç, oyun tahtasındaki en az üç aynı objeyi eşleştirerek temizlemek ve puan toplamaktır. Candy Crush ve Bejeweled gibi oyunlardan ilham alan bu proje, klasik mekaniği Unity üzerinde sıfırdan inşa eder: grid yönetimi, parça hareketleri, eşleşme tespiti, kademeli (cascade) doldurma, donmuş/engelli hücreler ve seviye ilerleme sistemi gibi konuları kapsar.

## Oyun Modları

### 1. Klasik Seviyeler (`Level1` – `Level20`)

- Her seviye `LevelMoves` script'i ile yönetilir: sabit bir **hamle sayısı** (`numMoves`) ve bir **hedef skor** (`targetScore`) içerir.
- Hamle sayısı tükendiğinde skor hedefin altındaysa oyun kaybedilir, üstündeyse kazanılır.
- Kazanılan her seviyede skor eşiklerine göre **1-3 yıldız** kazanılır (`score1Star`, `score2Star`, `score3Star`).
- Kaybedilen seviyelerde, ödüllü reklam izleyerek **+5 bonus hamle** alınıp oyuna devam edilebilir (`GameOver.OnWatchAdForMovesClicked`).
- `LevelSelect` sahnesinde seviyeler kilitlidir; bir seviye en az 1 yıldızla tamamlanmadan sonraki seviyenin kilidi açılmaz. İsteğe bağlı olarak reklam izleyerek bir sonraki kilitli seviye açılabilir.

### 2. Battle Path (Sefer Haritası)

- `BattlePath` sahnesinde, zigzag bir yol üzerine dizilmiş düşman düğümleri (`BattlePathNodeView`) bulunur.
- Her düğümün bir can puanı (HP) vardır; her 5. düğüm bir "kilometre taşı" (milestone) olup %50 daha fazla HP'ye sahiptir ve yol üzerinde görsel bir bölüm ayraciyla işaretlenir.
- Klasik seviyelerde elde edilen **en yüksek skor rekoru** kırıldığında, skor farkı "güç" (`power`) puanına çevrilir (`GameOver.ApplyScorePower`, 20 skor = 1 güç).
- Bu biriken güç, Battle Path sahnesine girildiğinde mevcut düğümün HP'sinden otomatik olarak düşülür (`ErodeRoutine`); düğüm HP'si sıfırlanınca bir sonraki düğüme geçilir.
- Ödüllü reklam izleyerek anında **+300 güç** kazanılabilir.
- İlerleme (`currentNodeIndex`, kalan HP) `PlayerPathProgress` üzerinden `PlayerPrefs`'e kaydedilir, böylece sahneye her girişte ilerleme korunur.

## Öne Çıkan Özellikler

- **Grid tabanlı tahta:** `xDim x yDim` boyutunda dinamik olarak kurulan, engelli/boş/normal hücre tiplerini destekleyen bir oyun tahtası (`Grid.cs`).
- **Bileşen tabanlı parça mimarisi:** Her oyun parçası (`GamePiece`) `MovablePiece` (hareket), `ColorPiece` (renk/eşleşme) ve `ClearablePiece` (temizlenme + animasyon) bileşenlerinin birleşimiyle davranış kazanır.
- **Buzlu (donmuş) parçalar:** `IcePiece` bileşeni bir parçayı hareketsiz ve eşleşmeye kapalı hale getirir; parça temizlenmeye çalışıldığında önce "çatlar", bir sonraki geçişte tamamen kırılır.
- **Engelli hücreler:** `blockedCells` listesiyle tahtada sabit, asla hareket etmeyen taş blokları tanımlanabilir; üstlerindeki parçalar köşegen (diagonal) olarak yan hücrelere akarak boşluğu doldurur.
- **Kademeli (cascade) skor:** Oyuncunun doğrudan yaptığı takasla oluşan eşleşmeler tam puan verir; yer çekimiyle zincirleme oluşan sonraki eşleşmeler `cascadeDepth` arttıkça azalan bir çarpanla puanlanır, böylece tek bir şanslı zincir skoru patlatmaz.
- **İlk doldurmada bedava eşleşme yok:** Seviye başında tahta doldurulurken oluşan rastgele eşleşmeler puan vermeden, sadece renk yeniden atanarak (`RerollColorUntilNoMatch`) temizlenir.
- **Sahne geçiş efekti:** `SceneFader`, sahneler arası geçişte ekranı tema rengiyle fade-in/fade-out yaparak yumuşak bir geçiş sağlar.
- **Ses yönetimi:** `AudioManager` (singleton, `DontDestroyOnLoad`) müzik/efekt seslerini tek noktadan açıp kapatır; ayar `PlayerPrefs` ile kalıcıdır, `SettingsPanel` üzerinden değiştirilir.
- **Duraklatma menüsü:** `PauseMenu`, `Time.timeScale` ile oyunu durdurur, yeniden başlatma ve ana menüye dönüş seçenekleri sunar.
- **UI dokunuş geri bildirimi:** `UIButtonPunch`, butonlara basıldığında küçük bir "punch" (ölçek) animasyonu uygular.
- **Duyarlı arka plan:** `ScreenBackgroundFit`, arka plan sprite'ını kameranın ortografik görüş alanına göre otomatik ölçekler.

## Proje Yapısı

```
Assets/
├── Animations/          # Parça temizlenme ve UI animasyon klipleri
├── Prefabs/              # Oyun parçaları, UI öğeleri, BattlePath/LevelSelect düğümleri
│   └── Tiles/            # Temalı taş prefabları (Kılıç, Kalkan, Kalp, Zehir, İksir)
├── Resources/
├── Scenes/               # MainScene, LevelSelect, BattlePath, Level1-20
├── Scripts/              # Tüm C# oyun mantığı (aşağıda detaylı)
├── TextMesh Pro/         # TMP fontları ve kaynakları
└── Textures/             # Sprite ve UI görselleri
GameDemo/                 # README'de kullanılan tanıtım görselleri/GIF
```

## Script Mimarisi

### Tahta ve Parça Mantığı

| Script | Görevi |
|---|---|
| `Grid.cs` | Oyun tahtasının kurulumu, doldurma/yer çekimi simülasyonu (`FillStep`), eşleşme tespiti (`GetMatch`), takas mantığı, kademeli skor için `cascadeDepth` takibi, köşegen doldurma ve buzlu hücre uygulaması. |
| `GamePiece.cs` | Bir tahta hücresindeki parçanın konumu, tipi ve bileşenlere (`MovableComponent`, `ColorComponent`, `ClearableComponent`, `IceComponent`) erişimi; mouse etkileşimlerini (`OnMouseDown/Enter/Up`) `Grid`'e iletir. |
| `ColorPiece.cs` | Parçanın rengini (`RED/GREEN/BLUE/YELLOW/ANY`) ve buna karşılık gelen sprite'ı yönetir; eşleşme kontrolü rengi referans alır. |
| `ClearablePiece.cs` | Parça temizlenirken oynatılacak animasyonu yönetir, `Level.OnPieceCleared` çağrısıyla skor eklenmesini tetikler, animasyon bitince nesneyi yok eder. |
| `MovablePiece.cs` | Parçanın bir hücreden diğerine `Lerp` ile yumuşak hareketini sağlar. |
| `IcePiece.cs` | Parçanın üstüne buz görünümü (overlay) ekler; parça donuk olduğu sürece hareket/eşleşme kabul etmez, `Crack()` ile kırılır. |

### Seviye ve Skor

| Script | Görevi |
|---|---|
| `Level.cs` | Tüm seviye tiplerinin temel sınıfı; skor hesaplama (renk başına temel puan + cascade çarpanı), kazanma/kaybetme akışı, HUD güncellemeleri. |
| `LevelMoves.cs` | Klasik (hamle sınırlı / hedef skorlu) seviye tipi; hamle sayacı, bonus hamle ekleme. |
| `BattleLevel.cs` | `Level`'den türeyen, hamle sınırı olmayan basit varyant (`OnMove` boş bırakılmış). |
| `Hud.cs` | Skor, hedef, kalan hamle ve yıldız göstergelerinin UI güncellemesi; skor değişiminde sayaç animasyonu. |
| `GameOver.cs` | Kazanma/kaybetme ekranı, yıldızların sırayla açılma animasyonu, reklamla bonus hamle alma, en yüksek skor kaydı ve Battle Path için "güç" puanı üretimi. |

### Battle Path

| Script | Görevi |
|---|---|
| `BattlePathController.cs` | Düğümleri prosedürel olarak zigzag bir yola dizer, kilometre taşı düğümlerini işaretler, biriken gücü düğüm HP'sinden aşındırır (`ErodeRoutine`), mevcut düğüme otomatik kaydırma yapar. |
| `BattlePathNodeView.cs` | Tek bir düğümün HP barını, simge rengini ve durum etiketini (`Buradasın` / `Geçildi` / `Bekliyor`) günceller. |
| `PlayerPathProgress.cs` | Düğüm indeksi, mevcut düğüm HP'si ve bekleyen güç miktarını `PlayerPrefs` ile kalıcı hale getiren statik yardımcı sınıf. |
| `PathScrollDrag.cs` | Hem Battle Path hem Level Select ekranlarında kullanılan, sürükleyerek kaydırma (drag-to-scroll) davranışı. |

### Menüler ve Genel UI

| Script | Görevi |
|---|---|
| `MainMenu.cs` | Ana menü buton aksiyonları (Oyna, Battle Path, Çıkış) ve banner reklam yerleşimi. |
| `LevelSelect.cs` | Seviye düğümlerini prosedürel olarak oluşturur, yıldız/kilit durumunu `PlayerPrefs`'ten okur, reklamla kilit açma. |
| `PauseMenu.cs` | Oyunu duraklatma/devam ettirme, yeniden başlatma, ana menüye dönüş. |
| `SettingsPanel.cs` | Ses açma/kapama ayar paneli. |
| `AudioManager.cs` | Müzik/efekt ses kaynaklarını yöneten singleton, sessize alma durumunu kalıcı tutar. |
| `SceneFader.cs` | Sahneler arası fade efektiyle geçiş yapan, sahneler arasında yaşayan singleton. |
| `UIButtonPunch.cs` | Butonlara basma anında küçük ölçek animasyonu. |
| `ScreenBackgroundFit.cs` | Arka plan sprite'ını ekrana/kameraya göre otomatik ölçekler. |
| `AdsManager.cs` | Google Mobile Ads Unity Plugin için ince bir sarmalayıcı; SDK kurulmadan da (test modunda ödülü anında vererek) çalışır. |

## Sahneler

| Sahne | Açıklama |
|---|---|
| `MainScene.unity` | Ana menü. |
| `LevelSelect.unity` | Klasik seviye seçim ekranı (1-20). |
| `Level1.unity` … `Level20.unity` | `LevelMoves` ile yapılandırılmış, hamle/skor hedefli klasik seviyeler. |
| `BattlePath.unity` | Düşman düğümlerinin dizildiği sefer haritası. |
| `Scene.unity` | Yardımcı/deneme sahnesi. |

## Veri Kalıcılığı (PlayerPrefs)

Proje, harici bir veritabanı yerine `PlayerPrefs` kullanır:

- `Level<N>` → İlgili seviyede kazanılan en yüksek yıldız sayısı (seviye kilidini de belirler).
- `Level<N>_BestScore` → İlgili seviyedeki en yüksek skor (Battle Path "güç" üretimi için referans).
- `PathNodeIndex`, `PathNodeHP`, `PathPendingPower` → Battle Path ilerleme durumu (`PlayerPathProgress`).
- `AudioMuted` → Ses açık/kapalı tercihi.

## Reklam Entegrasyonu

`AdsManager.cs`, Google Mobile Ads Unity Plugin'i `ADS_ENABLED` derleme sembolü arkasında sarmalar:

- SDK kurulu **değilken** proje normal şekilde derlenir/çalışır; `ShowRewardedAd` çağrıldığında ödül anında verilir (test/geliştirme akışını bloklamaz).
- Gerçek reklamları etkinleştirmek için Google Mobile Ads Unity Plugin paketini içeri aktarın ve **Project Settings > Player > Scripting Define Symbols** alanına `ADS_ENABLED` ekleyin.
- Varsayılan reklam birimi ID'leri Google'ın resmi **test ID'leridir** (rewarded ve banner için ayrı Android/iOS ID'leri).

## Kurulum ve Çalıştırma

1. **Unity Hub** üzerinden `6000.3.11f1` (veya uyumlu bir 6000.x) sürümünü kurun.
2. Bu depoyu klonlayın ve Unity Hub'dan proje klasörünü açın.
3. `Assets/Scenes/MainScene.unity` sahnesini açıp **Play** tuşuna basın.
4. Tüm seviyeler arasında gezinmek için `LevelSelect` sahnesinden, sefer haritasını denemek için `BattlePath` sahnesinden başlayabilirsiniz.

## Geliştirme Notları

- Renk eşleşmesi yalnızca yatay/dikey yönde, en az 3 ardışık aynı renkli parça ile tetiklenir (`Grid.GetMatch`).
- Doldurma döngüsü (`Fill`) eşleşme kalmayana kadar tekrarlanır; her tekrar `cascadeDepth`'i bir artırarak sonraki temizlemelerin skor çarpanını düşürür.
- Buzlu parçalar bir eşleşmenin parçası olabilir ama hemen temizlenmez: önce çatlar (`Crack`), bir sonraki temizleme geçişinde tamamen kaldırılır.
- Yeni bir klasik seviye eklemek için mevcut bir `LevelX.unity` sahnesini çoğaltıp `Grid` (boyut, engelli/buzlu hücreler) ve `LevelMoves` (hamle sayısı, hedef skor, yıldız eşikleri) ayarlarını düzenlemek yeterlidir.
