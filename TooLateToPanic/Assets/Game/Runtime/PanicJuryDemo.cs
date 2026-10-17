using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public enum PanicJuryStage { Inactive, Onboarding, Preparation, Earthquake, EarthquakeResult, Flood, Fire, Inspection, Result, Complete }
    public enum PanicJuryGoalState { Pending, Complete, Missed, Skipped }
    public enum PanicJuryShelter { None, QuakeCover, FloodHighGround, FireAssembly }

    public readonly struct PanicJuryGoal
    {
        public string Id { get; }
        public string Label { get; }
        public PanicJuryGoalState State { get; }
        public PanicJuryGoal(string id, string label, PanicJuryGoalState state) { Id = id; Label = label; State = state; }
    }

    /// <summary>Copies actual model state; it is never a second damage calculation.</summary>
    public readonly struct PanicJuryItemOutcome
    {
        public string Id { get; }
        public bool Anchored { get; }
        public float Damage { get; }
        public float Contribution { get; }
        public PanicJuryItemOutcome(PanicItem item)
        {
            Id = item.Id; Anchored = item.Anchored; Damage = item.Damage; Contribution = item.Contribution;
        }
    }

    public sealed class PanicJuryDisasterOutcome
    {
        public DisasterType Disaster { get; }
        public float TotalDamage { get; }
        public bool PreparationComplete { get; }
        public bool ShelterReached { get; }
        public int WaterConsumed { get; }
        public int FoodConsumed { get; }
        public IReadOnlyList<PanicJuryItemOutcome> HeroItems { get; }
        internal PanicJuryDisasterOutcome(PanicModel model, bool prepared, bool sheltered, int waterConsumed, int foodConsumed)
        {
            Disaster = model.Disaster; TotalDamage = model.TotalDamage;
            PreparationComplete = prepared; ShelterReached = sheltered;
            WaterConsumed = waterConsumed; FoodConsumed = foodConsumed;
            HeroItems = PanicJuryDemo.CopyHeroes(model);
        }
    }

    /// <summary>Immutable UI/presentation payload. No HUD class or mutable model list escapes here.</summary>
    public sealed class PanicJurySnapshot
    {
        public PanicJuryStage Stage { get; }
        public bool Active { get; }
        public bool Complete { get; }
        public float ElapsedSeconds { get; }
        public float StageRemainingSeconds { get; }
        public float HardRemainingSeconds { get; }
        public string Heading { get; }
        public string Hint { get; }
        public string TargetId { get; }
        public Vector3 TargetPosition { get; }
        public PanicJuryShelter ShelterKind { get; }
        public bool HasWorldTarget { get; }
        public bool CanAdvance { get; }
        public bool CanSkipSegment { get; }
        public bool AllowsMovement { get; }
        public bool StartingFoodIsBaseline { get { return true; } }
        public bool ModifiedByDeveloper { get; }
        public string Summary { get; }
        public IReadOnlyList<PanicJuryGoal> Goals { get; }
        public IReadOnlyList<PanicJuryItemOutcome> HeroItems { get; }
        public IReadOnlyList<PanicJuryDisasterOutcome> Outcomes { get; }

        internal PanicJurySnapshot(PanicJuryDemo demo)
        {
            Stage = demo.Stage; Active = demo.IsRunning; Complete = Stage == PanicJuryStage.Complete;
            ElapsedSeconds = demo.ElapsedSeconds; StageRemainingSeconds = demo.StageRemainingSeconds;
            HardRemainingSeconds = Mathf.Max(0, PanicJuryDemo.HardLimitSeconds - ElapsedSeconds);
            Heading = demo.Heading; Hint = demo.Hint;
            TargetId = demo.TargetId; TargetPosition = demo.TargetPosition; ShelterKind = demo.ShelterKind;
            HasWorldTarget = !string.IsNullOrEmpty(TargetId); CanAdvance = demo.CanAdvance;
            CanSkipSegment = demo.CanSkipSegment; AllowsMovement = demo.AllowsMovement;
            ModifiedByDeveloper = demo.Model.CheatedRun; Summary = demo.BuildSummary();
            Goals = demo.CopyGoals(); HeroItems = PanicJuryDemo.CopyHeroes(demo.Model);
            Outcomes = new List<PanicJuryDisasterOutcome>(demo.Outcomes).AsReadOnly();
        }
    }

    /// <summary>
    /// Bounded jury scenario: real pickup/pack/atomic work, three explicitly separate
    /// vignettes, immutable outcomes. Tick with unscaled wall time, except while genuinely paused. It never warps the player, grants success, replenishes stock or saves scores.
    /// </summary>
    public sealed class PanicJuryDemo
    {
        public const float ContentLimitSeconds = 270f;
        public const float HardLimitSeconds = 300f;
        public const float ReserveSeconds = 30f;
        public static readonly Vector3 SpawnPosition = new Vector3(3.1f, .05f, -3.4f);
        public static readonly Vector3 QuakeCoverPosition = new Vector3(-2.8f, .05f, -2.1f);
        public static readonly Vector3 FloodRefugePosition = new Vector3(0f, 3.25f, 5.15f);
        public static readonly Vector3 FireAssemblyPosition = new Vector3(-7f, .05f, -11.7f);
        public static readonly Vector3 FireStationPosition = new Vector3(1.72f, .05f, -2.2f);

        internal PanicModel Model { get; }
        readonly PanicExpedition expedition;
        readonly List<PanicJuryDisasterOutcome> outcomes = new List<PanicJuryDisasterOutcome>(3);
        readonly Dictionary<string, PanicJuryGoalState> goals = new Dictionary<string, PanicJuryGoalState>();
        readonly string[] goalIds = { "move", "water_gathered", "water_packed", "anchor", "quake_cover", "quake_observed", "flood_drain", "flood_refuge", "fire_ready", "fire_exit" };
        readonly string[] goalLabels = { "İlk adımlar", "Suyu al", "Suyu çantaya koy", "Bir eşyayı sabitle", "Çök–kapan–tutun alanı", "Önlemin sonucunu gör", "Drenajı tamamla", "Üst kattaki güvenli noktaya çık", "Yangın söndürücüyü hazırla", "Dışarıdaki toplanma alanına çık" };
        Vector3 startPosition, lastActorPosition;
        float elapsed, stageAge, stageDuration;
        bool running, dirty = true, quakeCaptured, floodCaptured, fireCaptured, quakePresented;
        PanicJurySnapshot cached;

        public PanicJuryStage Stage { get; private set; } = PanicJuryStage.Inactive;
        public bool IsRunning { get { return running && Model.IsJuryDemo; } }
        public float ElapsedSeconds { get { return elapsed; } }
        public float StageRemainingSeconds { get { return Mathf.Max(0, stageDuration - stageAge); } }
        public IReadOnlyList<PanicJuryDisasterOutcome> Outcomes { get { return outcomes.AsReadOnly(); } }
        public PanicJurySnapshot Snapshot { get { if (dirty || cached == null) { cached = new PanicJurySnapshot(this); dirty = false; } return cached; } }
        public bool CanAdvance { get { return Stage == PanicJuryStage.EarthquakeResult || Stage == PanicJuryStage.Inspection || Stage == PanicJuryStage.Result; } }
        public bool CanSkipSegment { get { return Stage == PanicJuryStage.Preparation || Stage == PanicJuryStage.Flood || Stage == PanicJuryStage.Fire; } }
        public bool AllowsMovement { get { return Stage == PanicJuryStage.Onboarding || Stage == PanicJuryStage.Preparation || Stage == PanicJuryStage.Flood || Stage == PanicJuryStage.Fire || Stage == PanicJuryStage.Inspection; } }

        public PanicJuryDemo(PanicModel model, PanicExpedition expedition)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            this.expedition = expedition ?? throw new ArgumentNullException(nameof(expedition));
            if (!ReferenceEquals(expedition.Model, model)) throw new ArgumentException("Jury model and expedition must share the same state.");
        }

        public bool Start() { return Start(SpawnPosition); }
        public bool Start(Vector3 actorPosition)
        {
            if (!Finite(actorPosition) || !Model.StartJuryDemo()) return false;
            if (!expedition.ResetForJuryDemo()) { Model.ReturnToTitle(); return false; }
            goals.Clear(); for (int i = 0; i < goalIds.Length; i++) goals.Add(goalIds[i], PanicJuryGoalState.Pending);
            outcomes.Clear(); elapsed = stageAge = 0; startPosition = lastActorPosition = actorPosition;
            quakeCaptured = floodCaptured = fireCaptured = quakePresented = false;
            running = true; BeginStage(PanicJuryStage.Onboarding);
            return true;
        }

        public void Tick(float unscaledDeltaTime, Vector3 actorPosition, bool grounded, bool inputBlocked)
        {
            if (!IsRunning || Model.IsPaused) return;
            if (!Finite(unscaledDeltaTime) || unscaledDeltaTime <= 0) return;
            if (Finite(actorPosition)) lastActorPosition = actorPosition;
            if (Stage == PanicJuryStage.Earthquake && !inputBlocked) quakePresented = true;
            // Do not clamp away hitches. Model.IsPaused is the explicit shared pause gate.
            // Objective evaluation happens once using the real current actor state;
            // a long frame cannot manufacture movement through several future stages.
            EvaluateObjectives(actorPosition, grounded, inputBlocked);
            AdvanceCompletedObjectives();
            float remaining = Mathf.Min(unscaledDeltaTime, Mathf.Max(0, HardLimitSeconds - elapsed));
            for (int guard = 0; guard < 16 && remaining > 0f && IsRunning; guard++)
            {
                float consumed = Mathf.Min(remaining, StageRemainingSeconds);
                elapsed += consumed; stageAge += consumed; remaining -= consumed;
                if (StageRemainingSeconds <= .00001f) AdvanceAtDeadline(false);
                if (elapsed >= ContentLimitSeconds && IsRunning) FinishAtHardLimit();
            }
            if (elapsed >= HardLimitSeconds && IsRunning) FinishAtHardLimit();
            if (IsRunning) RefreshModelClock();
            dirty = true;
        }

        public bool TryReachShelter(PanicJuryShelter kind, Vector3 actorPosition, bool grounded)
        {
            if (!IsRunning || Model.IsPaused || !grounded || !Finite(actorPosition)) return false;
            if (kind == PanicJuryShelter.QuakeCover && Stage == PanicJuryStage.Preparation && PreparationReady && At(actorPosition, QuakeCoverPosition, 1.25f, .6f))
            {
                SetComplete("quake_cover"); BeginEarthquake(); return true;
            }
            if (kind == PanicJuryShelter.FloodHighGround && Stage == PanicJuryStage.Flood && Done("flood_drain") && At(actorPosition, FloodRefugePosition, 1.25f, .55f))
            {
                SetComplete("flood_refuge"); CaptureFlood(); BeginStage(PanicJuryStage.Fire); return true;
            }
            if (kind == PanicJuryShelter.FireAssembly && Stage == PanicJuryStage.Fire && Done("fire_ready") && At(actorPosition, FireAssemblyPosition, 1.5f, .6f))
            {
                SetComplete("fire_exit"); CaptureFire(); BeginStage(PanicJuryStage.Inspection); return true;
            }
            return false;
        }

        public bool Advance()
        {
            if (!IsRunning || Model.IsPaused || !CanAdvance) return false;
            AdvanceAtDeadline(false); return true;
        }

        public bool SkipCinematic()
        {
            if (!IsRunning || Model.IsPaused || Stage != PanicJuryStage.Earthquake) return false;
            goals["quake_observed"] = PanicJuryGoalState.Skipped;
            BeginStage(PanicJuryStage.EarthquakeResult);
            return true;
        }

        public bool SkipCurrentSegment()
        {
            if (!IsRunning || Model.IsPaused || !CanSkipSegment) return false;
            AdvanceAtDeadline(true); return true;
        }

        /// <summary>Lifecycle only. World must cancel any tool/move UI as it does for ReturnToMenu.</summary>
        public void Stop()
        {
            expedition.Work.Cancel();
            if (Model.IsJuryDemo) Model.ReturnToTitle();
            expedition.Reset();
            running = false; Stage = PanicJuryStage.Inactive; stageDuration = stageAge = 0; dirty = true;
        }

        void EvaluateObjectives(Vector3 actorPosition, bool grounded, bool blocked)
        {
            if (blocked || !Finite(actorPosition)) return;
            if (Stage == PanicJuryStage.Onboarding && grounded && FlatDistance(actorPosition, startPosition) >= .85f)
                SetComplete("move");
            if (Stage == PanicJuryStage.Onboarding || Stage == PanicJuryStage.Preparation)
            {
                if (expedition.JuryWaterGatheredCount > 0) SetComplete("water_gathered");
                UpdateLiveGoal("water_packed", expedition.JuryWaterGatheredCount > 0 && Model.PackedWater >= 1);
                UpdateLiveGoal("anchor", ActualAnchoredHero());
            }
            if (Stage == PanicJuryStage.Flood && Model.DrainCount > 0) SetComplete("flood_drain");
            if (Stage == PanicJuryStage.Fire && Model.FireToolCount > 0) SetComplete("fire_ready");
        }

        void AdvanceCompletedObjectives()
        {
            if (Stage == PanicJuryStage.Onboarding && Done("move")) BeginStage(PanicJuryStage.Preparation);
        }

        bool PreparationReady { get { return Done("water_gathered") && Model.PackedWater >= 1 && ActualAnchoredHero(); } }

        void BeginEarthquake()
        {
            UpdateLiveGoal("water_packed", expedition.JuryWaterGatheredCount > 0 && Model.PackedWater >= 1);
            UpdateLiveGoal("anchor", ActualAnchoredHero());
            ClosePending(new[] { "move", "water_gathered", "water_packed", "anchor", "quake_cover" }, false);
            if (!quakeCaptured)
            {
                int water = Model.PackedWater, food = Model.PackedFood;
                bool prepared = PreparationReady && Model.BagReady;
                Model.ResolveJuryDisaster(DisasterType.Earthquake, true);
                expedition.SyncJuryBagAfterDisaster();
                outcomes.Add(new PanicJuryDisasterOutcome(Model, prepared, Done("quake_cover"), Mathf.Max(0, water - Model.PackedWater), Mathf.Max(0, food - Model.PackedFood)));
                quakeCaptured = true;
            }
            BeginStage(PanicJuryStage.Earthquake);
        }

        void CaptureFlood()
        {
            if (floodCaptured) return;
            Model.ResolveJuryDisaster(DisasterType.Flood, false);
            outcomes.Add(new PanicJuryDisasterOutcome(Model, Done("flood_drain"), Done("flood_refuge"), 0, 0));
            floodCaptured = true;
        }

        void CaptureFire()
        {
            if (fireCaptured) return;
            Model.ResolveJuryDisaster(DisasterType.Fire, false);
            outcomes.Add(new PanicJuryDisasterOutcome(Model, Done("fire_ready"), Done("fire_exit"), 0, 0));
            fireCaptured = true;
        }

        void AdvanceAtDeadline(bool skipped)
        {
            switch (Stage)
            {
                case PanicJuryStage.Onboarding:
                    ClosePending(new[] { "move" }, skipped); BeginStage(PanicJuryStage.Preparation); break;
                case PanicJuryStage.Preparation:
                    ClosePending(new[] { "water_gathered", "water_packed", "anchor", "quake_cover" }, skipped); BeginEarthquake(); break;
                case PanicJuryStage.Earthquake:
                    if (quakePresented) SetComplete("quake_observed");
                    else ClosePending(new[] { "quake_observed" }, false);
                    BeginStage(PanicJuryStage.EarthquakeResult); break;
                case PanicJuryStage.EarthquakeResult:
                    BeginStage(PanicJuryStage.Flood); break;
                case PanicJuryStage.Flood:
                    ClosePending(new[] { "flood_drain", "flood_refuge" }, skipped); CaptureFlood(); BeginStage(PanicJuryStage.Fire); break;
                case PanicJuryStage.Fire:
                    ClosePending(new[] { "fire_ready", "fire_exit" }, skipped); CaptureFire(); BeginStage(PanicJuryStage.Inspection); break;
                case PanicJuryStage.Inspection:
                    BeginStage(PanicJuryStage.Result); break;
                case PanicJuryStage.Result:
                    BeginStage(PanicJuryStage.Complete); running = false; break;
            }
        }

        void FinishAtHardLimit()
        {
            // A deadline is a truthful finish, never an inferred successful preparation.
            ClosePending(goalIds, false); expedition.Work.Cancel();
            BeginStage(PanicJuryStage.Complete); running = false;
        }

        void BeginStage(PanicJuryStage stage)
        {
            expedition.Work.Cancel(); // No completion, so no material or money mutation.
            Stage = stage; stageAge = 0f;
            switch (stage)
            {
                case PanicJuryStage.Onboarding: stageDuration = 20f; break;
                case PanicJuryStage.Preparation: stageDuration = 80f; break;
                case PanicJuryStage.Earthquake: stageDuration = 8f; break;
                case PanicJuryStage.EarthquakeResult: stageDuration = 4f; break;
                case PanicJuryStage.Flood: stageDuration = 50f; break;
                case PanicJuryStage.Fire: stageDuration = 70f; break;
                case PanicJuryStage.Inspection: stageDuration = 18f; break;
                case PanicJuryStage.Result: stageDuration = 20f; break;
                default: stageDuration = 0f; break;
            }
            RefreshModelClock(true); dirty = true;
        }

        void RefreshModelClock(bool announce = false)
        {
            PanicPhase phase = Stage == PanicJuryStage.Inspection ? PanicPhase.Inspection : AllowsMovement ? PanicPhase.Preparing :
                Stage == PanicJuryStage.Earthquake ? PanicPhase.Disaster :
                Stage == PanicJuryStage.EarthquakeResult || Stage == PanicJuryStage.Inspection ? PanicPhase.Aftermath : PanicPhase.Success;
            DisasterType disaster = Stage <= PanicJuryStage.EarthquakeResult ? DisasterType.Earthquake : Stage == PanicJuryStage.Flood ? DisasterType.Flood : DisasterType.Fire;
            string scope = Stage == PanicJuryStage.Preparation ? "anchor" : Stage == PanicJuryStage.Flood ? "drain" : Stage == PanicJuryStage.Fire ? "fire" : string.Empty;
            Model.SetJuryPresentation(phase, disaster, StageRemainingSeconds, announce ? Hint : null, scope);
        }

        bool Done(string id) { return goals.TryGetValue(id, out var state) && state == PanicJuryGoalState.Complete; }
        void SetComplete(string id) { if (goals.TryGetValue(id, out var state) && state == PanicJuryGoalState.Pending) { goals[id] = PanicJuryGoalState.Complete; dirty = true; } }
        void UpdateLiveGoal(string id, bool completed)
        {
            if (!goals.TryGetValue(id, out var state) || state == PanicJuryGoalState.Missed || state == PanicJuryGoalState.Skipped) return;
            var next = completed ? PanicJuryGoalState.Complete : PanicJuryGoalState.Pending;
            if (next != state) { goals[id] = next; dirty = true; }
        }
        void ClosePending(IEnumerable<string> ids, bool skipped)
        {
            foreach (string id in ids) if (goals.TryGetValue(id, out var state) && state == PanicJuryGoalState.Pending)
                goals[id] = skipped ? PanicJuryGoalState.Skipped : PanicJuryGoalState.Missed;
            dirty = true;
        }
        bool ActualAnchoredHero()
        {
            for (int i = 0; i < Model.Items.Count; i++)
                if ((Model.Items[i].Id == "bookshelf" || Model.Items[i].Id == "fridge") && Model.Items[i].Anchored) return true;
            return false;
        }
        static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        static bool Finite(Vector3 value) { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
        static float FlatDistance(Vector3 a, Vector3 b) { return new Vector2(a.x - b.x, a.z - b.z).magnitude; }
        static bool At(Vector3 p, Vector3 target, float radius, float heightTolerance) { return Mathf.Abs(p.y - target.y) <= heightTolerance && FlatDistance(p, target) <= radius; }

        internal static IReadOnlyList<PanicJuryItemOutcome> CopyHeroes(PanicModel model)
        {
            List<PanicJuryItemOutcome> result = new List<PanicJuryItemOutcome>(2);
            for (int i = 0; i < model.Items.Count; i++)
                if (model.Items[i].Id == "bookshelf" || model.Items[i].Id == "fridge") result.Add(new PanicJuryItemOutcome(model.Items[i]));
            return result.AsReadOnly();
        }
        internal IReadOnlyList<PanicJuryGoal> CopyGoals()
        {
            List<PanicJuryGoal> result = new List<PanicJuryGoal>(goalIds.Length);
            for (int i = 0; i < goalIds.Length; i++) result.Add(new PanicJuryGoal(goalIds[i], goalLabels[i], goals.TryGetValue(goalIds[i], out var state) ? state : PanicJuryGoalState.Pending));
            return result.AsReadOnly();
        }

        public string Heading
        {
            get
            {
                switch (Stage)
                {
                    case PanicJuryStage.Onboarding: return "Bir önlemle başla";
                    case PanicJuryStage.Preparation: return !Done("water_gathered") ? "Tezgâhtaki suyu al" : !Done("water_packed") ? "Suyu çantana koy" : !Done("anchor") ? "Bir eşyayı sabitle" : "Güvenli noktaya geç";
                    case PanicJuryStage.Earthquake: return "Önlemin farkı";
                    case PanicJuryStage.EarthquakeResult: return "Deprem sonucu";
                    case PanicJuryStage.Flood: return Done("flood_drain") ? "Merdivenden üst kata çık" : "Suya karşı önlem al";
                    case PanicJuryStage.Fire: return Done("fire_ready") ? "Güvenli alana çık" : "Yangın tüpünü hazırla";
                    case PanicJuryStage.Inspection: return "Önlemlerine bak";
                    case PanicJuryStage.Result: case PanicJuryStage.Complete: return "Hazırlığın görünür sonucu";
                    default: return "Hazırlık zamanı";
                }
            }
        }
        public string Hint
        {
            get
            {
                if (Stage == PanicJuryStage.Onboarding) return "WASD / oklarla yürü. 1 gıda çantada hazır; suyu sen alacaksın.";
                if (Stage == PanicJuryStage.Preparation)
                {
                    if (!Done("water_gathered")) return "Mutfak tezgâhındaki suya yaklaş · E ile al.";
                    if (!Done("water_packed")) return "Tab envanter · suyu seç · Enter çantaya koy.";
                    if (!Done("anchor")) return "G ile önlemlere dön · buzdolabı ya da kitaplığı seç · 1 ile vidaları tak.";
                    return "İşaretli alana yaklaş · E ile çök–kapan–tutun. Sarsıntıda koşma.";
                }
                if (Stage == PanicJuryStage.Earthquake) return Done("quake_cover") ? "Sarsıntı sürüyor. Seçtiğin eşyanın bağlantısına bak." : "Hazırlık süresi bitti. Eksikler sonuçta gösterilecek; tamamlanmış sayılmadı.";
                if (Stage == PanicJuryStage.EarthquakeResult)
                {
                    string summary = "Evde toplam hasar: %" + Model.TotalDamage.ToString("0.#") + ". ";
                    foreach (var hero in CopyHeroes(Model))
                        summary += (hero.Id == "fridge" ? "Buzdolabı" : "Kitaplık") + (hero.Anchored ? " sabit" : " sabit değil") + ": %" + hero.Damage.ToString("0.#") + ". ";
                    return summary;
                }
                if (Stage == PanicJuryStage.Flood) return Done("flood_drain") ? "Merdivenden üst kata çık · işaretli güvenli noktada E. Bodruma inme." : "Bahçe girişinin yanındaki drenajı E ile tamamla; sonra üst kata çık.";
                if (Stage == PanicJuryStage.Fire) return Done("fire_ready") ? "Bahçe kapısından çık · toplanma alanında E." : "Alt kattaki yangın ekipmanı noktasına git · E ile yangın tüpünü yerleştir.";
                if (Stage == PanicJuryStage.Inspection) return InspectionOutcomeHint();
                return BuildSummary();
            }
        }
        public PanicJuryShelter ShelterKind
        {
            get
            {
                if (Stage == PanicJuryStage.Preparation && PreparationReady) return PanicJuryShelter.QuakeCover;
                if (Stage == PanicJuryStage.Flood && Done("flood_drain")) return PanicJuryShelter.FloodHighGround;
                if (Stage == PanicJuryStage.Fire && Done("fire_ready")) return PanicJuryShelter.FireAssembly;
                return PanicJuryShelter.None;
            }
        }
        public string TargetId
        {
            get
            {
                if (ShelterKind != PanicJuryShelter.None) return "@jury_" + ShelterKind;
                if (Stage == PanicJuryStage.Onboarding) return "supply_water_kitchen";
                if (Stage == PanicJuryStage.Preparation) return Done("water_packed") ? ClosestAnchorCandidate() : "supply_water_kitchen";
                if (Stage == PanicJuryStage.Flood) return "drain_east";
                if (Stage == PanicJuryStage.Fire) return PanicItemActionRules.FireStationId;
                return string.Empty;
            }
        }
        public Vector3 TargetPosition
        {
            get
            {
                if (ShelterKind == PanicJuryShelter.QuakeCover) return QuakeCoverPosition;
                if (ShelterKind == PanicJuryShelter.FloodHighGround) return FloodRefugePosition;
                if (ShelterKind == PanicJuryShelter.FireAssembly) return FireAssemblyPosition;
                if (TargetId == "drain_east") return new Vector3(5, .05f, -8.5f);
                if (TargetId == PanicItemActionRules.FireStationId) return FireStationPosition;
                PanicWorldSupply supply = expedition.FindWorldSupply(TargetId);
                if (supply != null) return supply.Position;
                for (int i = 0; i < Model.Items.Count; i++) if (Model.Items[i].Id == TargetId) return Model.Items[i].Position;
                return Vector3.zero;
            }
        }
        string InspectionOutcomeHint()
        {
            string text = "";
            foreach (var outcome in outcomes)
            {
                if (outcome.Disaster != DisasterType.Flood && outcome.Disaster != DisasterType.Fire) continue;
                bool flood = outcome.Disaster == DisasterType.Flood;
                text += (flood ? "Sel · Drenaj " : "Yangın · Ekipman ") + (outcome.PreparationComplete ? "hazır" : "eksik") + " · Hasar %" + outcome.TotalDamage.ToString("0.#") + "\n";
            }
            return text + "İnceleme isteğe bağlı; Devam ile sonucu aç.";
        }

        internal string BuildSummary()
        {
            int done = 0, missed = 0, skipped = 0;
            foreach (var state in goals.Values)
                if (state == PanicJuryGoalState.Complete) done++; else if (state == PanicJuryGoalState.Missed) missed++; else if (state == PanicJuryGoalState.Skipped) skipped++;
            return "Tamamlanan adımlar: " + done + "/" + goalIds.Length + " · Eksik: " + missed + " · Atlanan: " + skipped +
                ". Daha iyi bir hazırlık için yeniden deneyebilirsin." + (Model.CheatedRun ? " Geliştirici seçeneği kullanıldı." : string.Empty);
        }

        string ClosestAnchorCandidate()
        {
            string chosen = "fridge"; float nearest = float.PositiveInfinity;
            for (int i = 0; i < Model.Items.Count; i++)
            {
                var item = Model.Items[i];
                if ((item.Id != "fridge" && item.Id != "bookshelf") || item.Anchored) continue;
                float distance = FlatDistance(lastActorPosition, item.Position);
                if (distance < nearest) { nearest = distance; chosen = item.Id; }
            }
            return chosen;
        }

        string ClosestFireCandidate()
        {
            string chosen = "fridge"; float nearest = float.PositiveInfinity;
            foreach (var item in Model.Items)
            {
                if (!item.Installed || !item.Movable || item.Kind == ItemKind.Door || item.Kind == ItemKind.Window) continue;
                float distance = Vector3.Distance(lastActorPosition, item.Position);
                if (distance < nearest) { nearest = distance; chosen = item.Id; }
            }
            return chosen;
        }
    }
}
