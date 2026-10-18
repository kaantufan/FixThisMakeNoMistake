using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    public enum PanicPhase
    {
        Title,
        Tutorial,
        Preparing,
        Disaster,
        Aftermath,
        Success,
        Inspection,
        GameOver
    }

    public enum DisasterType
    {
        Earthquake,
        Flood,
        Fire,
        Storm = 3
    }

    public enum ItemKind
    {
        Furniture,
        Wall,
        Door,
        Roof,
        Window
    }

    [Serializable]
    public sealed class PanicItem
    {
        public string Id;
        public string Label;
        public string ArtId;
        public ItemKind Kind;
        public Vector3 Position;
        public Vector3 Size;
        public float Rotation;
        public bool Movable;
        public bool Anchored;
        public bool AnchorOwned;
        /// <summary>Installed restraint surface: "wall" or "floor". Empty keeps legacy behavior.</summary>
        public string AnchorSurface;
        public bool FloodProtected;
        public bool FireProtected;
        public bool Installed = true;
        public int Tier;
        public float Damage;
        public float Contribution;
    }

    [Serializable]
    public sealed class HighscoreEntry
    {
        public int waves;
        public long score;
        public string date;
    }

    /// <summary>
    /// Pure run state and rules for Too Late to Panic. World placement, proximity,
    /// interaction timing and presentation are owned by PanicWorld/PanicHud.
    /// </summary>
    public sealed partial class PanicModel
    {
        private const int StartingBudget = PanicEconomyV59.StartingBudget;
        private const int WaterCost = 50;
        private const int FoodCost = 50;
        private const int AnchorCost = 50;
        private const int FloodGuardCost = 150;
        private const int FireProtectionCost = 150;
        private const int SupportCost = 100;
        private const int MaxPackedDays = 3;
        private const int SupportMask = 15;
        public const float DamageLossLimit = 75f;
        public const int RequiredSupplyPerWave = 1;
        public const float NormalPrecautionSeconds = 90f;
        public const float NormalBagSeconds = 40f;
        public const float NormalPreparationSeconds = NormalPrecautionSeconds + NormalBagSeconds;
        private const int ScoreTableSchema = 1;

        /// <summary>
        /// Optional full JSON file path for isolated smoke runs. Set before constructing
        /// PanicModel. Null/empty uses a product-specific subfolder of persistentDataPath.
        /// </summary>
        public static string ScorePathOverride;

        public PanicPhase Phase;
        public DisasterType Disaster;
        public int Wave;
        public int CompletedWaves;
        public int Budget;
        public bool IsPaused { get; private set; }
        public void SetPaused(bool paused) { IsPaused = paused; }
        public int RequiredDays;
        public int WaterStock;
        public int FoodStock;
        public int PackedWater;
        public int PackedFood;
        public int SupportOwnedMask;
        public int PackedSupportMask;
        public int TreeCount;
        public int DrainCount;
        public int FenceCount;
        public int FireToolCount;
        public long Score;
        public long LastWaveScore;
        public float TimeRemaining;
        public float PhaseTime;
        public float TotalDamage;
        public int LastReward;
        public string Message;
        public string FailureReason;
        public List<PanicItem> Items;
        public List<HighscoreEntry> Highscores;
        public bool IsTutorial;
        public PanicUtilitySafetyState UtilitySafety { get; } = new PanicUtilitySafetyState();
        public bool IsJuryDemo { get; private set; }
        public bool IsSimulation { get; private set; }
        public bool SimulationComplete { get; private set; }
        internal string JuryActionScope { get; private set; } = string.Empty;

        private bool prepWarningShown;
        private bool waveResolved;
        private bool pendingSuccess;
        private bool awardApplied;
        private bool bagWasReadyForWave;
        private bool runResultRecorded;
        private bool scoresPersisted = true;
        private bool cheatedScoreRemovalPending;
        private HighscoreEntry recordedRunHighscore;
        private uint runSeed;

        [Serializable]
        private sealed class HighscoreFile
        {
            public int schema = ScoreTableSchema;
            public List<HighscoreEntry> entries = new List<HighscoreEntry>();
        }

        public bool BagReady
        {
            get { return PackedWater >= RequiredSupplyPerWave && PackedFood >= RequiredSupplyPerWave; }
        }

        public static bool DamageCausesLoss(float damage) { return damage >= DamageLossLimit; }

        public PanicDeveloperOptions Developer { get; private set; }
        public bool InfiniteBudget { get { return Developer != null && Developer.InfiniteBudget; } }
        public bool FreeMaterials { get { return Developer != null && Developer.FreeMaterials; } }
        public bool FreezePreparationTime { get { return Developer != null && Developer.FreezePreparationTime; } }
        public bool DeveloperNoLoss { get { return Developer != null && Developer.NoLoss; } }
        public bool DeveloperNoLossUsedThisWave { get; private set; }
        public string DeveloperNoLossReason { get; private set; } = string.Empty;
        public bool DeveloperAnyEnabled { get { return InfiniteBudget || FreeMaterials || FreezePreparationTime || DeveloperNoLoss; } }
        public bool CheatedRun { get; private set; }

        public int SupportCount
        {
            get { return CountBits(PackedSupportMask & SupportMask); }
        }

        public PanicModel()
        {
            Phase = PanicPhase.Title;
            RequiredDays = 1;
            Items = CreateInitialItems();
            Highscores = new List<HighscoreEntry>();
            Developer = new PanicDeveloperOptions();
            Message = "Afet başlamadan önce evini hazırla.";
            LoadScores();
        }

        public void StartRun(bool tutorial)
        {
            if (Phase != PanicPhase.Title && Phase != PanicPhase.GameOver)
            {
                Refuse("Yeni koşu yalnızca başlık ekranından veya koşu sonundan başlatılabilir.");
                return;
            }

            PersistPendingCheatedScoreRemoval();
            ResetRunState(tutorial);
            BeginWaveOne();
            if (tutorial)
            {
                Phase = PanicPhase.Tutorial;
                TimeRemaining = 180f;
                PhaseTime = 180f;
                Message = "Öğretici: bu deneme bütçesi geçicidir. Evi hazırla, çantayı dene ve bunkera gir.";
            }
            else
            {
                Phase = PanicPhase.Preparing;
                Message = BuildWaveBrief();
            }
            MarkRunCheatedIfOptionsEnabled();
        }

        public void SetDeveloperOption(PanicDeveloperOption option, bool enabled)
        {
            if (Developer == null) Developer = new PanicDeveloperOptions();
            Developer.SetOption(option, enabled);
            if (option == PanicDeveloperOption.NoLoss && enabled && !IsJuryDemo &&
                (Phase == PanicPhase.Preparing || Phase == PanicPhase.Disaster || Phase == PanicPhase.Aftermath))
                LatchDeveloperNoLoss("Bu dalgada kaybetmeme modu kullanıldı.");
            if (enabled && Phase != PanicPhase.Title)
                MarkRunCheated();
        }

        public bool AddDeveloperBudget(int amount)
        {
            if (amount <= 0 || (Phase != PanicPhase.Tutorial && Phase != PanicPhase.Preparing &&
                Phase != PanicPhase.Inspection))
                return false;

            MarkRunCheated();
            Budget = SaturatingAdd(Budget, amount);
            Message = "+" + amount + " geliştirici bütçesi eklendi.";
            return true;
        }

        public void Tick(float delta)
        {
            if (IsPaused || WaveLedgerOpenV56 || KeyboardHelpOpenV56) return;
            // The bounded demo has its own unscaled clock; campaign phase transitions
            // and score awards must never run alongside that controller.
            if (IsJuryDemo || (IsSimulation && SimulationComplete && Phase == PanicPhase.Inspection)) return;
            if (float.IsNaN(delta) || float.IsInfinity(delta) || delta <= 0f)
                return;

            float remainingDelta = delta;
            int transitions = 0;
            while (remainingDelta > 0f && transitions < 8)
            {
                if (Phase == PanicPhase.Inspection && WaveLedgerAwaitingReviewV56) return;
                if (Phase == PanicPhase.Tutorial)
                    return;

                if (Phase == PanicPhase.Preparing)
                {
                    if (FreezePreparationTime) return;
                    float before = TimeRemaining;
                    float step = Mathf.Min(remainingDelta, TimeRemaining);
                    TimeRemaining = Mathf.Max(0f, TimeRemaining - step);
                    PhaseTime = TimeRemaining;
                    remainingDelta -= step;

                    if (!prepWarningShown && before > 60f && TimeRemaining <= 60f)
                    {
                        prepWarningShown = true;
                        Message = IsSimulation ? "Son 60 saniye: hazırlık süresi bitmeden önlemlerini tamamla." : "Son 60 saniye: önlemlerini tamamla. Son 40 saniyede çanta hazırlığı başlayacak.";
                    }

                    if (TimeRemaining <= 0f)
                    {
                        FailRun(IsSimulation ? "Hazırlık süresi doldu; önlemlerini zamanında tamamlayamadın." : "Hazırlık süresi doldu; " + RequiredShelterDestinationV5 + " zamanında ulaşamadın.");
                        transitions++;
                        continue;
                    }

                    return;
                }

                if (Phase == PanicPhase.Disaster || Phase == PanicPhase.Aftermath ||
                    Phase == PanicPhase.Success || Phase == PanicPhase.Inspection ||
                    Phase == PanicPhase.GameOver)
                {
                    if (Phase == PanicPhase.GameOver && PhaseTime <= 0f)
                        return;

                    float before = PhaseTime;
                    float step = Mathf.Min(remainingDelta, PhaseTime);
                    PhaseTime = Mathf.Max(0f, PhaseTime - step);
                    remainingDelta -= step;
                    if (before > 0f && PhaseTime > 0f)
                        return;

                    transitions++;
                    AdvanceTimedPhase();
                    continue;
                }

                return;
            }
        }

        public void FinishTutorial()
        {
            if (!IsTutorial)
            {
                Refuse("Şu anda etkin bir öğretici yok.");
                return;
            }

            ResetRunState(false);
            BeginWaveOne();
            Phase = PanicPhase.Preparing;
            Message = "Öğretici tamamlandı. Gerçek koşu 1. dalga ve 1.500 bütçeyle başladı.";
            MarkRunCheatedIfOptionsEnabled();
        }

        public bool Upgrade(string id)
        {
            if (!CanPrepare()) return RefuseBool("Yükseltme yalnızca hazırlıkta yapılabilir.");
            PanicItem item = FindItem(id);
            if (item == null) return RefuseBool("Yükseltilecek parça bulunamadı.");
            if (item.Kind == ItemKind.Roof) return RefuseBool("Çatı, üst kat duvarlarıyla birlikte güçlendirilir.");
            if (!item.Installed) return RefuseBool("Önce bu pencereyi tak.");
            if (!IsStructure(item)) return RefuseBool("Bu parça yükseltilemez.");
            if (item.Tier >= 2) return RefuseBool(item.Label + " zaten en yüksek kademe olan T2.");

            int cost = PanicEconomyV59.UpgradeBudgetCost(item);
            if (!CanAfford(cost)) return RefuseBool("Bütçe yetersiz: bu yükseltme " + cost + " bütçe gerektiriyor.");
            SpendBudget(cost);
            item.Tier++;
            Message = item.Label + " T" + item.Tier + " seviyesine yükseltildi. Bu kademe sonraki dalgalarda korunur.";
            return true;
        }

        public bool ToggleAnchor(string id)
        {
            if (!CanPrepare()) return RefuseBool("Sabitleme yalnızca hazırlıkta yapılabilir.");
            PanicItem item = FindItem(id);
            if (item == null) return RefuseBool("Sabitleyecek eşya bulunamadı.");
            if (!item.Movable) return RefuseBool("Bu eşya taşınamadığı için sabitlenemez.");

            if (item.Anchored)
            {
                item.Anchored = false;
                Message = item.Label + " serbest bırakıldı. Satın alınmış sabitleme yetkisi korunuyor.";
                return true;
            }

            if (!item.AnchorOwned)
            {
                if (!CanAfford(AnchorCost)) return RefuseBool("İlk sabitleme yetkisi için 50 bütçe gerekiyor.");
                SpendBudget(AnchorCost);
                item.AnchorOwned = true;
            }

            item.Anchored = true;
            item.AnchorSurface = item.Movable ? "floor" : "wall";
            Message = item.Label + " sabitlendi.";
            return true;
        }

        public bool AddFloodGuard(string id)
        {
            if (!CanPrepare()) return RefuseBool("Sel koruması yalnızca hazırlıkta takılabilir.");
            PanicItem item = FindItem(id);
            if (item == null) return RefuseBool("Korunacak pencere bulunamadı.");
            if (item.Kind != ItemKind.Window) return RefuseBool("Sel koruması yalnızca pencerelere takılabilir.");
            if (!item.Installed) return RefuseBool("Önce bu pencereyi tak.");
            if (item.FloodProtected) return RefuseBool(item.Label + " zaten sel korumalı.");
            if (!CanAfford(FloodGuardCost)) return RefuseBool("Sel koruması için 150 bütçe gerekiyor.");

            SpendBudget(FloodGuardCost);
            item.FloodProtected = true;
            Message = item.Label + " için sel koruması takıldı.";
            return true;
        }

        public bool AddFireProtection(string id)
        {
            if (!CanPrepare()) return RefuseBool("Yangın önlemi yalnızca hazırlıkta kurulabilir.");
            PanicItem item = FindItem(id);
            if (item == null) return RefuseBool("Yangın önlemi kurulacak parça bulunamadı.");
            if (!item.Installed) return RefuseBool("Önce bu pencereyi tak.");
            if (item.Id == "bunker" || item.Id == "bagstation")
                return RefuseBool("Yangın önlemi bu noktaya kurulamaz.");
            if (item.FireProtected) return RefuseBool(item.Label + " zaten yangın korumalı.");
            if (!CanAfford(FireProtectionCost)) return RefuseBool("Yangın önleyici ekipman için 150 bütçe gerekiyor.");

            SpendBudget(FireProtectionCost);
            item.FireProtected = true;
            Message = item.Label + " çevresine yangın önleyici ekipman kuruldu.";
            return true;
        }

        public bool InstallWindow(string id)
        {
            if (!CanPrepare()) return RefuseBool("Pencere yalnızca hazırlıkta takılabilir.");
            PanicItem item = FindItem(id);
            if (item == null || item.Kind != ItemKind.Window || item.Installed) return RefuseBool("Bu noktaya yeni pencere takılamaz.");
            if (!CanAfford(100)) return RefuseBool("Pencere için 100 bütçe gerekiyor.");
            SpendBudget(100); item.Installed = true;
            Message = "Yeni pencere takıldı. Artık güçlendirebilir veya sel bariyeri ekleyebilirsin.";
            return true;
        }

        public bool BuyWater()
        {
            return RefuseBool("Suyu evdeki fiziksel nesnelerden topla.");
        }

        public bool BuyFood()
        {
            return RefuseBool("Gıdayı evdeki fiziksel nesnelerden topla.");
        }

        public bool PackWater()
        {
            if (!CanPrepare()) return RefuseBool("Su yalnızca hazırlık aşamasında çantaya konabilir.");
            if (WaterStock <= 0) return RefuseBool("Çantaya koyacak su paketi yok; önce su al.");
            if (PackedWater >= MaxPackedDays) return RefuseBool("Çantada en fazla üç günlük su taşınabilir.");
            WaterStock--;
            PackedWater++;
            Message = "Bir günlük su çantaya kondu.";
            return true;
        }

        public bool PackFood()
        {
            if (!CanPrepare()) return RefuseBool("Gıda yalnızca hazırlık aşamasında çantaya konabilir.");
            if (FoodStock <= 0) return RefuseBool("Çantaya koyacak gıda paketi yok; önce gıda al.");
            if (PackedFood >= MaxPackedDays) return RefuseBool("Çantada en fazla üç günlük gıda taşınabilir.");
            FoodStock--;
            PackedFood++;
            Message = "Bir günlük gıda çantaya kondu.";
            return true;
        }

        public bool BuySupport(int index)
        {
            return RefuseBool("Destek ekipmanını evdeki fiziksel nesnelerden topla.");
        }

        public bool PackSupport(int index)
        {
            if (!CanPrepare()) return RefuseBool("Destek ekipmanı yalnızca hazırlıkta çantaya konabilir.");
            if (!IsValidSupportIndex(index)) return RefuseBool("Bilinmeyen destek ekipmanı.");
            int bit = 1 << index;
            if ((SupportOwnedMask & bit) == 0) return RefuseBool("Önce bu destek ekipmanını satın al.");
            if ((PackedSupportMask & bit) != 0) return RefuseBool("Bu destek ekipmanı zaten çantada ve sonraki dalgalarda korunuyor.");

            PackedSupportMask |= bit;
            Message = SupportName(index) + " çantaya kondu. Her başarılı dalga bütçesine %5 ekler.";
            return true;
        }

        public bool EnterBunker()
        {
            // Compatibility entry used by existing simulation/controller code. Physical
            // destination validation remains World-owned; new callers use the typed API.
            return TryEnterShelterV5(RequiredShelterV5);
        }

        public bool InspectSimulationDamage()
        {
            if (!IsSimulation || !SimulationComplete || Phase != PanicPhase.GameOver) return false;
            // Retain the failed result and real damage; this only opens the walkable review.
            Phase = PanicPhase.Inspection;
            PhaseTime = TimeRemaining = 0f;
            Message = "Hasar " + FormatPercent(TotalDamage) + ". " + FailureReason + " Önlemlerini inceleyip yeniden deneyebilirsin.";
            return true;
        }

        public void FinishInspection()
        {
            if (WaveLedgerOpenV56 || WaveLedgerAwaitingReviewV56 || KeyboardHelpOpenV56) return;
            if (IsSimulation)
            {
                if (Phase == PanicPhase.Inspection) { SimulationComplete = true; PhaseTime = 0f; }
                return;
            }
            if (IsJuryDemo) { Notify("Devam düğmesini kullan."); return; }
            if (Phase != PanicPhase.Inspection)
            {
                Refuse("Hasar incelemesi şu anda açık değil.");
                return;
            }

            ClearWaveDamage();
            PhaseTime = 0f;
            if (IsTutorial)
            {
                ResetWaveLedgerV56();
                Phase = PanicPhase.Tutorial;
                TimeRemaining = 180f;
                PhaseTime = 180f;
                Message = "Öğretici denemesi bitti. İstediğin zaman öğreticiyi tamamlayıp gerçek koşuya geçebilirsin.";
                return;
            }

            Wave = SaturatingAdd(Wave, 1);
            BeginWave();
            Phase = PanicPhase.Preparing;
            Message = BuildWaveBrief();
        }

        public void ReturnToTitle()
        {
            PersistPendingCheatedScoreRemoval();
            if (!IsSimulation && !IsJuryDemo && !IsTutorial && CompletedWaves > 0 && !runResultRecorded)
                RecordRunOnce();
            else if (!IsSimulation && !IsJuryDemo && !CheatedRun && runResultRecorded && !scoresPersisted)
                scoresPersisted = SaveScoresAtomic();

            IsTutorial = false;
            ResetRunState(false);
            Phase = PanicPhase.Title;
            Message = "Afet başlamadan önce evini hazırla.";
        }

        public bool MoveItem(string id, Vector3 position, float rotation)
        {
            if (!CanPrepare()) return RefuseBool("Eşyalar yalnızca hazırlıkta taşınabilir.");
            PanicItem item = FindItem(id);
            if (item == null) return RefuseBool("Taşınacak eşya bulunamadı.");
            if (!item.Movable) return RefuseBool("Bu parça taşınamaz.");
            if (item.Anchored) return RefuseBool("Taşımadan önce eşyanın sabitlemesini kaldır.");
            if (!IsFinite(position.x) || !IsFinite(position.y) || !IsFinite(position.z) || !IsFinite(rotation))
                return RefuseBool("Geçersiz yerleşim verisi.");

            // Room bounds, collisions, bunker route and snap validation belong to PanicWorld.
            item.Position = position;
            item.Rotation = rotation;
            Message = item.Label + " yeni konumuna taşındı.";
            return true;
        }

        public void Notify(string message)
        {
            Message = message ?? string.Empty;
        }

        /// <summary>One selectable, unranked disaster using the real existing resolution path.</summary>
        public bool StartSimulation(DisasterType type)
        {
            if (Phase != PanicPhase.Title && Phase != PanicPhase.GameOver) return false;
            if (type != DisasterType.Earthquake && type != DisasterType.Flood && type != DisasterType.Fire && type != DisasterType.Storm) return false;
            PersistPendingCheatedScoreRemoval();
            ResetRunState(false);
            IsSimulation = true;
            Disaster = type;
            Wave = 1;
            RequiredDays = 1;
            PackedWater = PackedFood = 1;
            Phase = PanicPhase.Preparing;
            TimeRemaining = PhaseTime = 90f;
            DeveloperNoLossUsedThisWave = DeveloperNoLoss;
            DeveloperNoLossReason = DeveloperNoLoss ? "Bu dalgada kaybetmeme modu kullanıldı." : string.Empty;
            MarkRunCheatedIfOptionsEnabled();
            Message = DisasterName(type) + " için hazırlan. Çantanda başlangıç suyu ve gıdası hazır.";
            return true;
        }

        /// <summary>Starts an isolated, finite jury scenario. No campaign award is applied.</summary>
        public bool StartJuryDemo()
        {
            if (Phase != PanicPhase.Title && Phase != PanicPhase.GameOver) return false;
            PersistPendingCheatedScoreRemoval();
            ResetRunState(false);
            IsJuryDemo = true;
            Wave = 1;
            RequiredDays = 1;
            Disaster = DisasterType.Earthquake;
            Phase = PanicPhase.Preparing;
            TimeRemaining = PhaseTime = 100f;
            MarkRunCheatedIfOptionsEnabled();
            Message = "Önce evini hazırla. Son 40 saniyede suyu al; çantanda 1 gıda hazır.";
            return true;
        }

        internal void SetJuryPresentation(PanicPhase phase, DisasterType disaster, float remaining, string message, string actionScope)
        {
            if (!IsJuryDemo) return;
            Phase = phase;
            Disaster = disaster;
            TimeRemaining = PhaseTime = float.IsNaN(remaining) || float.IsInfinity(remaining) ? 0f : Mathf.Max(0f, remaining);
            JuryActionScope = actionScope ?? string.Empty;
            if (!string.IsNullOrEmpty(message)) Message = message;
        }

        internal void ResolveJuryDisaster(DisasterType disaster, bool consumeSupplies)
        {
            if (!IsJuryDemo) return;
            Disaster = disaster;
            ClearWaveDamage();
            CalculateDamage();
            if (consumeSupplies) ConsumeWaveSupplies();
            LastReward = 0;
            LastWaveScore = 0;
            FailureReason = string.Empty;
        }

        internal bool CanBeginJuryWork(string action, string targetId)
        {
            if (!IsJuryDemo) return true;
            if (JuryActionScope == "prepare")
                // Optional valid precautions share the real target/recipe validator.
                // The short flow still requires only one hero anchor; no extra goal/time.
                return action == "anchorwall" || action == "anchorfloor" || action == "upgrade" ||
                    action == "flood" || action == "plant" || action == "drain" || action == "hose" || action == "fire" || PanicUtilitySafetyV414.IsAction(action);
            if (JuryActionScope == "drain")
                return action == "drain" && (targetId == "drain_west" || targetId == "drain_east");
            return JuryActionScope == "fire" && action == "fire";
        }

        private void ResetRunState(bool tutorial)
        {
            ResetKeyboardHelpV56();
            ResetWaveLedgerV56();
            UtilitySafety.Reset();
            ResetSystemsV5();
            IsJuryDemo = false;
            IsSimulation = false;
            SimulationComplete = false;
            JuryActionScope = string.Empty;
            ResetDeveloperWaveSkipProtectionV51();
            DeveloperNoLossUsedThisWave = false;
            DeveloperNoLossReason = string.Empty;
            IsTutorial = tutorial;
            Wave = 1;
            CompletedWaves = 0;
            IsPaused = false;
            Budget = StartingBudget;
            RequiredDays = 1;
            WaterStock = 0;
            FoodStock = 0;
            PackedWater = 0;
            PackedFood = 0;
            SupportOwnedMask = 0;
            PackedSupportMask = 0;
            TreeCount = 0;
            DrainCount = 0;
            FenceCount = 0;
            FireToolCount = 0;
            Score = 0;
            LastWaveScore = 0;
            LastReward = 0;
            TimeRemaining = 0f;
            PhaseTime = 0f;
            TotalDamage = 0f;
            FailureReason = string.Empty;
            prepWarningShown = false;
            waveResolved = false;
            pendingSuccess = false;
            awardApplied = false;
            bagWasReadyForWave = false;
            runResultRecorded = false;
            scoresPersisted = true;
            CheatedRun = false;
            recordedRunHighscore = null;
            runSeed = 0x20261016u;
            ResetCompoundDisastersV54();
            ResetUpgradeDowngradesV513();
            Items = CreateInitialItems();
        }

        private void BeginWaveOne()
        {
            Wave = 1;
            CompletedWaves = 0;
            BeginWave();
        }

        private void BeginWave()
        {
            ResetWaveLedgerV56();
            ResetDeveloperWaveSkipProtectionV51();
            BeginWaveSystemsV5();
            DeveloperNoLossUsedThisWave = DeveloperNoLoss;
            DeveloperNoLossReason = DeveloperNoLoss ? "Bu dalgada kaybetmeme modu kullanıldı." : string.Empty;
            RequiredDays = RequiredSupplyPerWave;
            LastWaveScore = 0;
            LastReward = 0;
            FailureReason = string.Empty;
            TotalDamage = 0f;
            prepWarningShown = false;
            waveResolved = false;
            pendingSuccess = false;
            awardApplied = false;
            bagWasReadyForWave = false;
            SelectDisasterForWave();
            TimeRemaining = PreparationSeconds();
            PhaseTime = TimeRemaining;
        }

        private void SelectDisasterForWave()
        { SelectCompoundDisastersV54(); }

        private void AdvanceTimedPhase()
        {
            if (Phase == PanicPhase.Disaster)
            {
                ResolveDisaster();
                return;
            }

            if (Phase == PanicPhase.Aftermath)
            {
                if (IsSimulation)
                {
                    if (pendingSuccess)
                    {
                        if (DeveloperNoLoss || DeveloperNoLossUsedThisWave) BeginDeveloperNoLossInspection(FailureReason);
                        else CompleteSimulationInspection();
                    }
                    else if (DeveloperNoLoss) BeginDeveloperNoLossInspection(FailureReason);
                    else FailRun(FailureReason);
                    return;
                }
                if (IsTutorial)
                {
                    Phase = PanicPhase.Success;
                    PhaseTime = 3f;
                    Message = pendingSuccess
                        ? "Öğretici afeti tamamlandı. Sonucu incele, sonra gerçek koşuya geç."
                        : "Öğretici sonucu: " + FailureReason + " Hasarı inceleyip hazırlığı tekrar deneyebilirsin.";
                    CaptureWaveLedgerV56("Öğretici sonucu", "Öğretici denemesinde bütçe ödülü verilmez.", Budget);
                }
                else if (DeveloperNoLoss || DeveloperWaveSkipProtectionV51 || (DeveloperNoLossUsedThisWave && pendingSuccess))
                {
                    BeginDeveloperNoLossInspection(FailureReason);
                }
                else if (pendingSuccess)
                {
                    AwardSuccessfulWaveOnce();
                    Phase = PanicPhase.Success;
                    PhaseTime = 3f;
                    Message = "Wave " + Wave + " tamamlandı. +" + LastReward + " bütçe, +" + LastWaveScore + " puan.";
                }
                else
                {
                    FailRun(FailureReason);
                }
                return;
            }

            if (Phase == PanicPhase.Success)
            {
                Phase = PanicPhase.Inspection;
                PhaseTime = 20f;
                Message = "Evi incele: hasar toplamı " + FormatPercent(TotalDamage) + ". Hazır olduğunda yeni dalgaya geç.";
                return;
            }

            if (Phase == PanicPhase.Inspection)
            {
                FinishInspection();
                return;
            }

            // GameOver's four-second result presentation ends in a stable result screen.
            if (Phase == PanicPhase.GameOver)
            {
                PhaseTime = 0f;
                return;
            }
        }

        private void ResolveDisaster()
        {
            if (waveResolved) return;
            waveResolved = true;

            CalculateDamage();
            ApplyCriticalTaskPenaltyV513();
            CaptureCriticalTasksV513();
            ResolveUpgradeDowngradesV513();
            bagWasReadyForWave = BagReady;
            FailureReason = BuildFailureReason(bagWasReadyForWave, PackedWater, PackedFood);
            ConsumeWaveSupplies();
            pendingSuccess = bagWasReadyForWave && !DamageCausesLoss(TotalDamage);
            Phase = PanicPhase.Aftermath;
            PhaseTime = 4f;
            Message = BuildDamageSummary();
            if (DeveloperNoLoss || DeveloperWaveSkipProtectionV51)
            {
                if (DeveloperNoLoss) LatchDeveloperNoLoss(FailureReason);
                Message += " Geliştirici dalga koruması açık; gerçek hasar ve erzak tüketimi korunur, bu geçişe ödül verilmez.";
            }
        }

        private void CalculateSingleDisasterDamageV54(bool deferCapV54 = false)
        {
            float globalFloodGuard = 0f;
            float globalFireProtection = 0f;
            int floodGuardCount = 0;
            int fireProtectionCount = 0;
            int exposedItems = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].FloodProtected) floodGuardCount++;
                if (Items[i].FireProtected) fireProtectionCount++;
                if (BaseRisk(Items[i], Disaster) > 0f) exposedItems++;
            }

            if (Disaster == DisasterType.Flood)
                globalFloodGuard = Mathf.Min(0.42f,
                    floodGuardCount * 0.06f + Mathf.Max(0, DrainCount) * 0.10f +
                    Mathf.Max(0, FenceCount) * 0.07f + Mathf.Max(0, TreeCount) * 0.015f);
            else if (Disaster == DisasterType.Fire)
                globalFireProtection = Mathf.Min(0.42f,
                    fireProtectionCount * 0.08f + Mathf.Max(0, FireToolCount) * 0.11f);

            float severity = DisasterSeverityV52;
            float sum = 0f;
            for (int i = 0; i < Items.Count; i++)
            {
                PanicItem item = Items[i];
                float baseRisk = BaseRisk(item, Disaster);
                if (baseRisk <= 0f)
                {
                    item.Damage = 0f;
                    item.Contribution = 0f;
                    continue;
                }

                float risk = baseRisk * severity * StableVariation(item.Id, Wave, Disaster, runSeed);
                risk *= StructureMultiplier(item, Disaster);
                risk *= AnchorMultiplier(item, Disaster);
                risk *= UtilitySafety.DamageMultiplier(Disaster, item);
                if (Disaster == DisasterType.Flood)
                {
                    risk *= 1f - globalFloodGuard;
                    if (item.FloodProtected) risk *= 0.25f;
                }
                else if (Disaster == DisasterType.Fire)
                {
                    risk *= 1f - globalFireProtection;
                    if (item.FireProtected) risk *= 0.25f;
                }

                item.Contribution = Mathf.Max(0f, risk);
                sum += item.Contribution;
            }

            // Keep the risk a typical exposed household item contributes roughly stable as
            // rooms gain the second floor and additional furnishings. Late-wave severity and
            // missing preparation can still exceed the loss threshold; raw item count cannot.
            float normalization = exposedItems > 0 ? Mathf.Min(1f, 13f / exposedItems) : 0f;
            float normalizedTotal = 0f;
            for (int i = 0; i < Items.Count; i++)
            {
                PanicItem item = Items[i];
                item.Contribution *= normalization;
                float baseRisk = BaseRisk(item, Disaster);
                item.Damage = baseRisk <= 0f ? 0f : Mathf.Clamp01(item.Contribution / baseRisk) * 100f;
                normalizedTotal += item.Contribution;
            }

            if (normalizedTotal > 100f && !deferCapV54)
            {
                // Higher waves can saturate all hazards. Preserve every item's share
                // and the exact 0..100 total rather than subtracting from just one item.
                float scale = 100f / normalizedTotal;
                for (int i = 0; i < Items.Count; i++)
                {
                    Items[i].Contribution *= scale;
                    float baseRisk = BaseRisk(Items[i], Disaster);
                    Items[i].Damage = baseRisk <= 0f ? 0f : Mathf.Clamp01(Items[i].Contribution / baseRisk) * 100f;
                }
                normalizedTotal = 100f;
            }
            TotalDamage = normalizedTotal;
            ApplySecondaryDamageV5(deferCapV54);
        }

        private static float BaseRisk(PanicItem item, DisasterType disaster)
        {
            if (item == null || !item.Installed || item.Id == "bunker" || item.Id == "bagstation") return 0f;
            if (disaster == DisasterType.Storm)
            {
                if (item.Kind == ItemKind.Roof) return 12f;
                if (item.Kind == ItemKind.Window) return 10f;
                if (item.Kind == ItemKind.Door) return 7f;
                if (item.Kind == ItemKind.Wall) return 4f;
            }
            if (item.Kind == ItemKind.Wall)
                return disaster == DisasterType.Earthquake ? 3.25f : disaster == DisasterType.Flood ? 6.25f : 3f;
            if (item.Kind == ItemKind.Door)
                return disaster == DisasterType.Earthquake ? 4f : 4f;
            if (item.Kind == ItemKind.Roof)
                return disaster == DisasterType.Earthquake ? 8f : disaster == DisasterType.Flood ? 2.5f : 8f;
            if (item.Kind == ItemKind.Window)
                return disaster == DisasterType.Earthquake ? 3.5f : disaster == DisasterType.Flood ? 8f : 5f;

            string riskArtId = CanonicalRiskArtId(item.ArtId);
            if (disaster == DisasterType.Storm)
            {
                // Stylized game risk values, not structural engineering predictions.
                switch (riskArtId)
                {
                    case "bookshelf": return 6f;
                    case "sofa": return 3f;
                    case "table": return 3f;
                    case "chair": return 2f;
                    case "fridge": return 4f;
                    case "cabinet": return 5f;
                    case "bed": return 2f;
                    case "plant": return 5f;
                    case "tvstand": return 3f;
                    default: return 0f;
                }
            }
            switch (riskArtId)
            {
                case "bookshelf": return disaster == DisasterType.Earthquake ? 8f : disaster == DisasterType.Flood ? 2f : 6f;
                case "sofa": return disaster == DisasterType.Earthquake ? 7f : disaster == DisasterType.Flood ? 2f : 8f;
                case "table": return disaster == DisasterType.Earthquake ? 6f : disaster == DisasterType.Flood ? 1.5f : 3f;
                case "chair": return disaster == DisasterType.Earthquake ? 4f : disaster == DisasterType.Flood ? 1f : 3f;
                case "fridge": return disaster == DisasterType.Earthquake ? 5f : disaster == DisasterType.Flood ? 5f : 1.2f;
                case "cabinet": return disaster == DisasterType.Earthquake ? 5f : disaster == DisasterType.Flood ? 3f : 3f;
                case "bed": return disaster == DisasterType.Earthquake ? 4f : disaster == DisasterType.Flood ? 2f : 8f;
                case "plant": return disaster == DisasterType.Earthquake ? 1f : disaster == DisasterType.Flood ? 1f : 3f;
                case "tvstand": return disaster == DisasterType.Earthquake ? 2f : disaster == DisasterType.Flood ? 1f : 2f;
                default: return 0f;
            }
        }

        private static string CanonicalRiskArtId(string artId)
        {
            return artId == "sofa_indigo" || artId == "sofa_olive" ? "sofa" : artId;
        }

        private static float StructureMultiplier(PanicItem item, DisasterType disaster)
        {
            if (!IsStructure(item)) return 1f;
            float perTier = disaster == DisasterType.Storm ? 0.20f : disaster == DisasterType.Earthquake ? 0.22f :
                disaster == DisasterType.Flood ? 0.14f : 0.12f;
            return Mathf.Max(0.35f, 1f - Mathf.Clamp(item.Tier, 0, 2) * perTier);
        }

        private static float AnchorMultiplier(PanicItem item, DisasterType disaster)
        {
            if (!item.Anchored) return 1f;
            bool wallMounted = string.Equals(item.AnchorSurface, "wall", StringComparison.Ordinal);
            if (string.IsNullOrEmpty(item.AnchorSurface)) wallMounted = !item.Movable;
            if (disaster == DisasterType.Earthquake) return wallMounted ? 0.48f : 0.42f;
            if (disaster == DisasterType.Flood) return wallMounted ? 0.86f : 0.92f;
            if (disaster == DisasterType.Storm && item.Kind == ItemKind.Furniture) return wallMounted ? 0.70f : 0.62f;
            return 1f;
        }

        private void ConsumeWaveSupplies()
        {
            PackedWater = Mathf.Max(0, PackedWater - RequiredSupplyPerWave);
            PackedFood = Mathf.Max(0, PackedFood - RequiredSupplyPerWave);
        }

        private string BuildFailureReason(bool bagWasReady, int packedWaterAtEntry, int packedFoodAtEntry)
        {
            List<string> causes = new List<string>();
            if (!IsSimulation && !IsJuryDemo && !ReachedShelterV5)
                causes.Add(RequiredShelterDestinationV5 + " zamanında ulaşılamadı.");
            if (!bagWasReady)
            {
                int waterMissing = Mathf.Max(0, RequiredSupplyPerWave - packedWaterAtEntry);
                int foodMissing = Mathf.Max(0, RequiredSupplyPerWave - packedFoodAtEntry);
                causes.Add("Çanta yetersiz: bu dalga için 1 su ve 1 gıda gerekli; " +
                    waterMissing + " su ve " + foodMissing + " gıda eksik.");
            }
            if (DamageCausesLoss(TotalDamage))
                causes.Add("Toplam hasar " + FormatPercent(TotalDamage) + ". %75 ve üzeri hasarda ev kaybedilir.");
            return causes.Count == 0 ? string.Empty : string.Join(" ", causes.ToArray());
        }

        private string BuildDamageSummary()
        {
            List<PanicItem> top = TopDamageSources(3);
            StringBuilder summary = new StringBuilder();
            summary.Append("Toplam hasar: ").Append(FormatPercent(TotalDamage)).Append('.');
            if (!string.IsNullOrEmpty(SecondaryEventSummaryV5)) summary.Append(" ").Append(SecondaryEventSummaryV5);
            if (top.Count > 0)
            {
                summary.Append(" En büyük kaynaklar: ");
                for (int i = 0; i < top.Count; i++)
                {
                    if (i > 0) summary.Append("; ");
                    summary.Append(top[i].Label).Append(" ").Append(FormatPoints(top[i].Contribution)).Append(" yüzde puan");
                }
                summary.Append('.');
            }
            if (!string.IsNullOrEmpty(FailureReason))
                summary.Append(" ").Append(FailureReason);
            else if (bagWasReadyForWave)
                summary.Append(IsSimulation ? " Çantadaki su ve gıda yeterliydi." : " Çanta bu dalga için yeterli.");
            if (!string.IsNullOrEmpty(CriticalTaskPenaltySummaryV513)) summary.Append(" ").Append(CriticalTaskPenaltySummaryV513);
            if (UpgradeDowngradeWaveV513 == Wave && !string.IsNullOrEmpty(UpgradeDowngradeSummaryV513))
                summary.Append(" ").Append(UpgradeDowngradeSummaryV513);
            return summary.ToString();
        }

        private List<PanicItem> TopDamageSources(int count)
        {
            List<PanicItem> ordered = new List<PanicItem>();
            for (int i = 0; i < Items.Count; i++)
            {
                PanicItem candidate = Items[i];
                if (candidate.Contribution <= 0.05f) continue;
                int insertAt = ordered.Count;
                for (int j = 0; j < ordered.Count; j++)
                {
                    if (candidate.Contribution > ordered[j].Contribution)
                    {
                        insertAt = j;
                        break;
                    }
                }
                ordered.Insert(insertAt, candidate);
                if (ordered.Count > count) ordered.RemoveAt(ordered.Count - 1);
            }
            return ordered;
        }

        private void AwardSuccessfulWaveOnce()
        {
            if (IsSimulation || DeveloperNoLoss || DeveloperNoLossUsedThisWave || DeveloperWaveSkipProtectionV51) return;
            if (awardApplied) return;
            awardApplied = true;
            CompletedWaves = SaturatingAdd(CompletedWaves, 1);
            float waveMultiplier = 1f + 0.25f * Mathf.Max(0, Wave - 1);
            float scoreValue = 1000f * waveMultiplier * (1f - TotalDamage / 100f);
            LastWaveScore = Math.Max(0L, (long)Mathf.Round(scoreValue));
            Score = SaturatingAdd(Score, LastWaveScore);

            float baseReward = PanicEconomyV59.BaseWaveReward(Wave);
            float damageMultiplier = 1f - 0.5f * (TotalDamage / 100f);
            float supportMultiplier = 1f + 0.05f * SupportCount;
            LastReward = Math.Max(0, Mathf.RoundToInt(baseReward * damageMultiplier * supportMultiplier));
            int budgetBefore = Budget;
            Budget = SaturatingAdd(Budget, LastReward);
            int basis = Mathf.RoundToInt(baseReward);
            int afterDamage = Mathf.RoundToInt(baseReward * damageMultiplier);
            CaptureWaveLedgerV56("Dalga tamamlandı", "Hasar kesintisi dalga ödülünden düşer. Destek ekipmanı kalan ödülü artırır.",
                budgetBefore, basis, basis - afterDamage, LastReward - afterDamage);
        }

        private void CompleteSimulationInspection()
        {
            SimulationComplete = true;
            Phase = PanicPhase.Inspection;
            TimeRemaining = PhaseTime = 0f;
            LastReward = 0;
            LastWaveScore = 0;
            Message = "Sonuç hazır. Hasar " + FormatPercent(TotalDamage) + ". Önlemlerini inceleyebilirsin.";
            CaptureWaveLedgerV56("Simülasyon sonucu", "Simülasyonda bütçe ve skor ödülü verilmez.", Budget);
        }

        private void LatchDeveloperNoLoss(string reason)
        {
            DeveloperNoLossUsedThisWave = true;
            if (!string.IsNullOrEmpty(reason)) DeveloperNoLossReason = reason;
            MarkRunCheated();
        }

        private void BeginDeveloperNoLossInspection(string reason)
        {
            LatchDeveloperNoLoss(reason);
            LastWaveScore = 0;
            LastReward = 0;
            TimeRemaining = 0f;
            Phase = PanicPhase.Inspection;
            PhaseTime = IsSimulation ? 0f : 20f;
            if (IsSimulation) SimulationComplete = true;
            string explanation = !string.IsNullOrEmpty(reason) ? reason :
                !string.IsNullOrEmpty(DeveloperNoLossReason) ? DeveloperNoLossReason : "Bu dalgada kaybetmeme modu kullanıldı.";
            Message = "Geliştirici koruması: " + explanation + " Gerçek hasar " + FormatPercent(TotalDamage) +
                ". İnceleme açık; bu geçiş başarı, skor veya bütçe ödülü sayılmaz.";
            CaptureWaveLedgerV56("Ödülsüz inceleme", "Geliştirici dalga koruması kullanıldı; bütçe ödülü verilmez.", Budget);
        }

        private void FailRun(string reason)
        {
            if (Phase == PanicPhase.GameOver) return;
            if (DeveloperNoLoss && !IsJuryDemo && Phase == PanicPhase.Preparing)
            {
                LatchDeveloperNoLoss(reason);
                // Continue through the existing 8s disaster and real damage/supply
                // resolution. No automatic bag completion or damage immunity.
                BeginUnshelteredDisasterV5();
                Message = "Geliştirici koruması: " + reason +
                    " Afet yine uygulanıyor; çanta ve hasar değişmez, bu geçişe ödül verilmez.";
                return;
            }
            if ((DeveloperNoLoss || DeveloperWaveSkipProtectionV51) && !IsJuryDemo && Phase == PanicPhase.Aftermath)
            {
                BeginDeveloperNoLossInspection(reason);
                return;
            }
            FailureReason = string.IsNullOrEmpty(reason) ? "Koşu sona erdi." : reason;
            LastWaveScore = 0;
            LastReward = 0;
            Phase = PanicPhase.GameOver;
            if (IsSimulation) SimulationComplete = true;
            PhaseTime = 4f;
            TimeRemaining = 0f;
            Message = IsSimulation ? "Hazırlık tamamlanamadı. " + FailureReason + " Önlemlerini değiştirip yeniden deneyebilirsin." : "Koşu sona erdi. " + FailureReason + " Tamamlanan dalga: " + CompletedWaves + "; toplam puan: " + Score + ".";
            if (!IsTutorial)
                RecordRunOnce();
            CaptureWaveLedgerV56("Koşu sona erdi", "Başarısız dalgada bütçe ödülü verilmez. Kayıp nedeni sonuç ekranında.", Budget);
        }

        private void RecordRunOnce()
        {
            if (IsSimulation || IsJuryDemo || IsTutorial || runResultRecorded) return;
            runResultRecorded = true;
            if (CheatedRun)
            {
                scoresPersisted = true;
                return;
            }

            recordedRunHighscore = new HighscoreEntry
            {
                waves = CompletedWaves,
                score = Score,
                date = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
            Highscores.Add(recordedRunHighscore);
            StableSortHighscores();
            scoresPersisted = SaveScoresAtomic();
            if (scoresPersisted) cheatedScoreRemovalPending = false;
            if (!scoresPersisted)
            {
                Debug.LogWarning("Too Late to Panic: local highscore could not be atomically written; the entry remains in memory.");
                Message += " Yerel skor dosyası yazılamadı.";
            }
        }

        private bool SaveScoresAtomic()
        {
            if (IsSimulation || IsJuryDemo) return false;
            string path = GetScorePath();
            string tempPath = path + ".tmp";
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                HighscoreFile data = new HighscoreFile();
                data.entries = Highscores ?? new List<HighscoreEntry>();
                string json = JsonUtility.ToJson(data, true);
                using (FileStream stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush();
                }

                if (File.Exists(path))
                    File.Replace(tempPath, path, null);
                else
                    File.Move(tempPath, path);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Too Late to Panic: highscore save failed: " + exception.Message);
                return false;
            }
        }

        private void LoadScores()
        {
            try
            {
                string path = GetScorePath();
                if (!File.Exists(path)) return;
                string json = File.ReadAllText(path, Encoding.UTF8);
                HighscoreFile data = JsonUtility.FromJson<HighscoreFile>(json);
                if (data == null || data.entries == null) return;
                if (data.schema != ScoreTableSchema)
                { Debug.LogWarning("Too Late to Panic: unsupported highscore schema; original file retained."); return; }
                if (!PanicScoreShape.TryEntries(json, out var validShapes) || validShapes.Count != data.entries.Count)
                { Debug.LogWarning("Too Late to Panic: invalid highscore field types; original file retained."); return; }
                Highscores.Clear();
                for (int i = 0; i < data.entries.Count; i++)
                {
                    HighscoreEntry entry = data.entries[i];
                    if (!validShapes[i] || entry == null || entry.waves < 0 || entry.score < 0) continue;
                    Highscores.Add(entry);
                }
                StableSortHighscores();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Too Late to Panic: highscore load failed: " + exception.Message);
            }
        }

        private string GetScorePath()
        {
            if (!string.IsNullOrEmpty(ScorePathOverride))
                return Path.GetFullPath(ScorePathOverride);
            return Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
        }

        private void StableSortHighscores()
        {
            // Stable insertion sort keeps the earlier record first when both keys tie.
            for (int i = 1; i < Highscores.Count; i++)
            {
                HighscoreEntry key = Highscores[i];
                int j = i - 1;
                while (j >= 0 && CompareHighscores(key, Highscores[j]) < 0)
                {
                    Highscores[j + 1] = Highscores[j];
                    j--;
                }
                Highscores[j + 1] = key;
            }
        }

        private static int CompareHighscores(HighscoreEntry left, HighscoreEntry right)
        {
            if (left.waves != right.waves) return right.waves.CompareTo(left.waves);
            return right.score.CompareTo(left.score);
        }

        private bool CanPrepare()
        {
            return !IsPaused && !WaveLedgerOpenV56 && !KeyboardHelpOpenV56 && (Phase == PanicPhase.Preparing || Phase == PanicPhase.Tutorial);
        }

        private bool CanAfford(int cost)
        {
            return InfiniteBudget || Budget >= cost;
        }

        private void SpendBudget(int cost)
        {
            if (!InfiniteBudget) Budget -= cost;
        }

        private void MarkRunCheatedIfOptionsEnabled()
        {
            if (DeveloperAnyEnabled) MarkRunCheated();
        }

        private void MarkRunCheated()
        {
            if (CheatedRun) return;
            CheatedRun = true;

            if (recordedRunHighscore == null) return;
            Highscores.Remove(recordedRunHighscore);
            recordedRunHighscore = null;
            StableSortHighscores();
            cheatedScoreRemovalPending = true;
            PersistPendingCheatedScoreRemoval();
        }

        private void PersistPendingCheatedScoreRemoval()
        {
            if (!cheatedScoreRemovalPending) return;
            scoresPersisted = SaveScoresAtomic();
            if (scoresPersisted) cheatedScoreRemovalPending = false;
        }

        private bool RefuseBool(string reason)
        {
            Refuse(reason);
            return false;
        }

        private void Refuse(string reason)
        {
            Message = reason;
        }

        private PanicItem FindItem(string id)
        {
            if (string.IsNullOrEmpty(id) || Items == null) return null;
            for (int i = 0; i < Items.Count; i++)
                if (string.Equals(Items[i].Id, id, StringComparison.Ordinal)) return Items[i];
            return null;
        }

        private string BuildWaveBrief()
        {
            return "Dalga " + Wave + " · " + ActiveDisasterLabelV54 + " · Çantaya 1 su ve 1 gıda koy. Diğer ekipmanlar isteğe bağlı.";
        }

        private void ClearWaveDamage()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Damage = 0f;
                Items[i].Contribution = 0f;
            }
            TotalDamage = 0f;
        }

        private static List<PanicItem> CreateInitialItems()
        {
            List<PanicItem> items = new List<PanicItem>(40);

            // Perimeter shell IDs are stable. Positions mark each floor's base elevation;
            // PanicHomeArt owns the actual wall segments, openings and structural colliders.
            AddItem(items, "wall_north", "Kuzey alt duvarı", "wall", ItemKind.Wall, new Vector3(0f, 0f, 5.91f), new Vector3(16f, 3.2f, .18f), false);
            AddItem(items, "wall_west", "Batı alt duvarı", "wall", ItemKind.Wall, new Vector3(-7.91f, 0f, 0f), new Vector3(12f, 3.2f, .18f), false, 270f);
            AddItem(items, "wall_south", "Güney alt duvarı", "wall", ItemKind.Wall, new Vector3(0f, 0f, -5.91f), new Vector3(16f, 3.2f, .18f), false);
            AddItem(items, "wall_east", "Doğu alt duvarı", "wall", ItemKind.Wall, new Vector3(7.91f, 0f, 0f), new Vector3(12f, 3.2f, .18f), false, 90f);
            AddItem(items, "wall_north_upper", "Kuzey üst duvarı", "wall", ItemKind.Wall, new Vector3(0f, 3.2f, 5.91f), new Vector3(16f, 3.2f, .18f), false);
            AddItem(items, "wall_west_upper", "Batı üst duvarı", "wall", ItemKind.Wall, new Vector3(-7.91f, 3.2f, 0f), new Vector3(12f, 3.2f, .18f), false, 270f);
            AddItem(items, "wall_south_upper", "Güney üst duvarı", "wall", ItemKind.Wall, new Vector3(0f, 3.2f, -5.91f), new Vector3(16f, 3.2f, .18f), false);
            AddItem(items, "wall_east_upper", "Doğu üst duvarı", "wall", ItemKind.Wall, new Vector3(7.91f, 3.2f, 0f), new Vector3(12f, 3.2f, .18f), false, 90f);

            AddItem(items, "door", "Bahçe giriş kapısı", "door", ItemKind.Door, new Vector3(0f, 0f, -6f), new Vector3(1.2f, 2.8f, .18f), false);
            AddItem(items, "door_inner_ground", "Alt kat iç kapısı", "door", ItemKind.Door, new Vector3(2f, 0f, 1f), new Vector3(1f, 2.4f, .14f), false, 90f);
            AddItem(items, "door_inner_upper", "Üst kat iç kapısı", "door", ItemKind.Door, new Vector3(2f, 3.2f, 1f), new Vector3(1f, 2.4f, .14f), false, 90f);
            AddItem(items, "roof", "Çatı", "roof", ItemKind.Roof, new Vector3(0f, 6.4f, 0f), new Vector3(16f, .2f, 12f), false);

            AddItem(items, "window_north", "Kuzey alt penceresi doğu", "window", ItemKind.Window, new Vector3(4f, 0f, 5.82f), new Vector3(1.8f, 1.4f, .3f), false);
            AddItem(items, "window_spare", "Kuzey alt penceresi batı", "window", ItemKind.Window, new Vector3(-4f, 0f, 5.82f), new Vector3(1.8f, 1.4f, .3f), false);
            AddItem(items, "window_west", "Batı alt penceresi güney", "window", ItemKind.Window, new Vector3(-7.82f, 0f, -3f), new Vector3(1.8f, 1.4f, .3f), false, 270f);
            AddItem(items, "window_west_north", "Batı alt penceresi kuzey", "window", ItemKind.Window, new Vector3(-7.82f, 0f, 3f), new Vector3(1.8f, 1.4f, .3f), false, 270f);
            AddItem(items, "window_north_upper_east", "Kuzey üst penceresi doğu", "window", ItemKind.Window, new Vector3(4f, 3.2f, 5.82f), new Vector3(1.8f, 1.4f, .3f), false);
            AddItem(items, "window_north_upper_west", "Kuzey üst penceresi batı", "window", ItemKind.Window, new Vector3(-4f, 3.2f, 5.82f), new Vector3(1.8f, 1.4f, .3f), false);
            AddItem(items, "window_west_upper_south", "Batı üst penceresi güney", "window", ItemKind.Window, new Vector3(-7.82f, 3.2f, -3f), new Vector3(1.8f, 1.4f, .3f), false, 270f);
            AddItem(items, "window_west_upper_north", "Batı üst penceresi kuzey", "window", ItemKind.Window, new Vector3(-7.82f, 3.2f, 3f), new Vector3(1.8f, 1.4f, .3f), false, 270f);

            // Ground floor: west living area, clear central stair/hall, east kitchen.
            AddItem(items, "bookshelf", "Kitaplık", "bookshelf", ItemKind.Furniture, new Vector3(-6f, 0f, 4.8f), new Vector3(1.54f, 2.37f, .53f), true);
            AddItem(items, "sofa", "Koltuk", "sofa", ItemKind.Furniture, new Vector3(-4f, 0f, -1f), new Vector3(2.35f, 1.12f, 1.1f), true);
            AddItem(items, "table", "Masa", "table", ItemKind.Furniture, new Vector3(-3.6f, 0f, -3f), new Vector3(1.55f, 1.05f, .94f), true);
            AddItem(items, "chair", "Sandalye", "chair", ItemKind.Furniture, new Vector3(-1.8f, 0f, -3f), new Vector3(.62f, 1.1f, .66f), true);
            AddItem(items, "fridge", "Buzdolabı", "fridge", ItemKind.Furniture, new Vector3(6.5f, 0f, 4.6f), new Vector3(.88f, 1.96f, .9f), true);
            AddItem(items, "cabinet", "Mutfak dolabı", "cabinet", ItemKind.Furniture, new Vector3(4.5f, 0f, 4.7f), new Vector3(1.48f, .94f, .67f), true);
            AddItem(items, "tvstand", "TV sehpası", "tvstand", ItemKind.Furniture, new Vector3(-6f, 0f, -4.6f), new Vector3(1.7f, 1.58f, .55f), true, 180f);
            AddItem(items, "plant", "Salon bitkisi", "plant", ItemKind.Furniture, new Vector3(-6.5f, 0f, 1.5f), new Vector3(.85f, 1.3f, .85f), true);

            // Upper floor furnishings use floor-base Y=3.2 and stay outside the stair well.
            AddItem(items, "bed", "Yatak", "bed", ItemKind.Furniture, new Vector3(-4f, 3.2f, 3.8f), new Vector3(1.64f, 1.42f, 2.16f), true);
            AddItem(items, "desk_upper", "Çalışma masası", "table", ItemKind.Furniture, new Vector3(4f, 3.2f, -2.5f), new Vector3(1.55f, 1.05f, .94f), true);
            AddItem(items, "up_cabinet", "Üst kat dolabı", "cabinet", ItemKind.Furniture, new Vector3(6f, 3.2f, 4.5f), new Vector3(1.48f, .94f, .67f), true);
            AddItem(items, "up_bookshelf", "Üst kat kitaplığı", "bookshelf", ItemKind.Furniture, new Vector3(-6f, 3.2f, -3.8f), new Vector3(1.54f, 2.37f, .53f), true);

            AddItem(items, "bunker", "Bodrum sığınağı", "bunker", ItemKind.Furniture, new Vector3(-4.5f, -3.2f, 3.2f), new Vector3(1.86f, .34f, 1.66f), false);
            AddItem(items, "bagstation", "Depo dolabı", "bagstation", ItemKind.Furniture, new Vector3(5.4f, 0f, -3.8f), new Vector3(1.74f, 1.52f, .76f), false);
            return items;
        }

        private static void AddItem(List<PanicItem> items, string id, string label, string artId,
            ItemKind kind, Vector3 position, Vector3 size, bool movable, float rotation = 0f)
        {
            items.Add(new PanicItem
            {
                Id = id,
                Label = label,
                ArtId = artId,
                Kind = kind,
                Position = position,
                Size = size,
                Rotation = rotation,
                Movable = movable,
                Anchored = false,
                AnchorOwned = false,
                AnchorSurface = string.Empty,
                FloodProtected = false,
                FireProtected = false,
                Tier = 0,
                Damage = 0f,
                Contribution = 0f
            });
        }

        private static bool IsStructure(PanicItem item)
        {
            return item != null && (item.Kind == ItemKind.Wall || item.Kind == ItemKind.Door ||
                item.Kind == ItemKind.Roof || item.Kind == ItemKind.Window);
        }

        private static float PreparationSeconds()
        {
            // Every campaign wave preserves 90 seconds for precautions, then 40 for the bag.
            return NormalPreparationSeconds;
        }

        private static bool IsValidSupportIndex(int index)
        {
            return index >= 0 && index < 4;
        }

        private static string SupportName(int index)
        {
            switch (index)
            {
                case 0: return "İlk yardım seti";
                case 1: return "El feneri ve pil seti";
                case 2: return "Pilli radyo";
                default: return "Koruyucu giysi ve battaniye seti";
            }
        }

        private static string DisasterName(DisasterType disaster)
        {
            switch (disaster)
            {
                case DisasterType.Earthquake: return "Deprem";
                case DisasterType.Flood: return "Sel";
                case DisasterType.Storm: return "Fırtına";
                default: return "Yangın";
            }
        }

        private static string FormatPercent(float value)
        {
            return "%" + value.ToString("0.0", CultureInfo.InvariantCulture);
        }

        private static string FormatPoints(float value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture);
        }

        private static int CountBits(int mask)
        {
            int count = 0;
            while (mask != 0)
            {
                count += mask & 1;
                mask >>= 1;
            }
            return count;
        }

        private static int SaturatingAdd(int value, int addition)
        {
            long result = (long)value + addition;
            return result >= int.MaxValue ? int.MaxValue : result <= int.MinValue ? int.MinValue : (int)result;
        }

        private static long SaturatingAdd(long value, long addition)
        {
            if (addition > 0 && value > long.MaxValue - addition) return long.MaxValue;
            if (addition < 0 && value < long.MinValue - addition) return long.MinValue;
            return value + addition;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float StableVariation(string itemId, int wave, DisasterType disaster, uint seed)
        {
            // Keep household variation stable across waves: progression must not be hidden by a fresh risk roll.
            uint hash = seed ^ 0x85EBCA6Bu ^ ((uint)disaster * 0xC2B2AE35u);
            if (itemId != null)
                for (int i = 0; i < itemId.Length; i++)
                    hash = (hash ^ itemId[i]) * 16777619u;
            hash = NextUInt(ref hash);
            float unit = (hash & 0x00FFFFFFu) / 16777215f;
            return 0.94f + unit * 0.12f;
        }

        private static uint NextUInt(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }
    }
}
