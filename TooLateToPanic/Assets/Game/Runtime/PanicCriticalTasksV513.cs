using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicCriticalTaskV513
    {
        public string Id { get; }
        public string Label { get; }
        public string Consequence { get; }
        public bool Complete { get; }
        public bool Resolved { get; }
        public DisasterType Hazard { get; }
        public float PenaltyPoints { get; }
        internal PanicCriticalTaskV513(string id, string label, string consequence, bool complete,
            DisasterType hazard, float penalty, bool resolved)
        { Id = id; Label = label; Consequence = consequence; Complete = complete; Hazard = hazard; PenaltyPoints = penalty; Resolved = resolved; }
    }

    public sealed class PanicCriticalPenaltyV513
    {
        public string Id { get; }
        public string Label { get; }
        public string TargetId { get; }
        public DisasterType Hazard { get; }
        public float DamagePoints { get; }
        internal PanicCriticalPenaltyV513(string id, string label, string target, DisasterType hazard, float points)
        { Id = id; Label = label; TargetId = target; Hazard = hazard; DamagePoints = points; }
    }

    /// <summary>Read-only task snapshots; mutation is confined to the final damage calculation hook.</summary>
    public sealed partial class PanicModel
    {
        public const float CriticalPenaltyCapV513 = 40f;
        public const float CriticalAnchorPenaltyV513 = 20f;
        public const float CriticalHazardPenaltyV513 = 25f;
        readonly List<PanicCriticalPenaltyV513> criticalPenaltiesV513 = new List<PanicCriticalPenaltyV513>();
        IReadOnlyList<PanicCriticalTaskV513> criticalResolvedTasksV513;
        int criticalResolvedWaveV513 = -1;
        public bool CriticalTaskPenaltiesEnabledV513
        { get { return !IsTutorial && !IsSimulation && !IsJuryDemo && !DeveloperWaveSkipProtectionV51; } }
        public IReadOnlyList<PanicCriticalPenaltyV513> CriticalPenaltiesV513 { get { return criticalPenaltiesV513.AsReadOnly(); } }
        public float CriticalPenaltyDamageV513
        { get { float sum = 0; foreach (var row in criticalPenaltiesV513) sum += row.DamagePoints; return sum; } }
        public string CriticalTaskPenaltySummaryV513
        {
            get
            {
                if (criticalPenaltiesV513.Count == 0) return string.Empty;
                string text = "Kritik görev cezası: ";
                foreach (var row in criticalPenaltiesV513)
                { if (text != "Kritik görev cezası: ") text += " · "; text += row.Label + " (+" + FormatPoints(row.DamagePoints) + " yüzde puan)"; }
                return text + ".";
            }
        }
        public IReadOnlyList<PanicCriticalTaskV513> CriticalTasksV513
        {
            get
            {
                if (criticalResolvedWaveV513 == Wave && criticalResolvedTasksV513 != null &&
                    Phase != PanicPhase.Preparing && Phase != PanicPhase.Tutorial && Phase != PanicPhase.Title)
                    return criticalResolvedTasksV513;
                return BuildCriticalTasksV513(false).AsReadOnly();
            }
        }
        List<PanicCriticalTaskV513> BuildCriticalTasksV513(bool resolved)
        {
            var rows = new List<PanicCriticalTaskV513>(6);
            string failure = DeveloperNoLoss || DeveloperWaveSkipProtectionV51 ? "Eksikse ödülsüz inceleme" : "Eksikse koşu biter";
            rows.Add(new PanicCriticalTaskV513("water", "Su " + Mathf.Max(0, Mathf.Min(1, PackedWater)) + "/1", failure, PackedWater >= 1, Disaster, 0, resolved));
            rows.Add(new PanicCriticalTaskV513("food", "Gıda " + Mathf.Max(0, Mathf.Min(1, PackedFood)) + "/1", failure, PackedFood >= 1, Disaster, 0, resolved));
            rows.Add(new PanicCriticalTaskV513("shelter", RequiredShelterV5 == PanicShelterKindV5.PanicRoom ? "Üst panik odasına gir" : "Bodrum sığınağına gir",
                failure, ReachedShelterV5, Disaster, 0, resolved));
            bool anchored = false, roofReady = true, hasRoof = false;
            foreach (var item in Items)
            {
                if (item == null || !item.Installed) continue;
                if (item.Kind == ItemKind.Furniture && item.Movable && item.Anchored) anchored = true;
                if (item.Kind == ItemKind.Roof) { hasRoof = true; if (item.Tier < 1) roofReady = false; }
            }
            for (int n = 0; n < 4; n++)
            {
                var hazard = (DisasterType)n;
                if (!HasActiveDisasterV54(hazard)) continue;
                string id, label; bool complete; float penalty;
                switch (hazard)
                {
                    case DisasterType.Earthquake: id = "anchor"; label = "En az 1 eşyayı sabitle"; complete = anchored; penalty = CriticalAnchorPenaltyV513; break;
                    case DisasterType.Flood: id = "poweroff"; label = "Elektrik şalterini indir"; complete = UtilitySafety.PowerOff; penalty = CriticalHazardPenaltyV513; break;
                    case DisasterType.Fire: id = "gasoff"; label = "Gaz vanasını kapat"; complete = UtilitySafety.GasClosed; penalty = CriticalHazardPenaltyV513; break;
                    default: id = "roof"; label = "Üst kattan çatıyı güçlendir"; complete = hasRoof && roofReady; penalty = CriticalHazardPenaltyV513; break;
                }
                rows.Add(new PanicCriticalTaskV513(id, label, DeveloperWaveSkipProtectionV51 ? "F3: ek ceza kapalı" : "Yapılmazsa en çok +" + FormatPoints(penalty) + " hasar puanı", complete, hazard, penalty, resolved));
            }
            return rows;
        }

        // Call immediately after the critical penalty, before downgrade or supply consumption.
        void CaptureCriticalTasksV513()
        { criticalResolvedTasksV513 = BuildCriticalTasksV513(true).AsReadOnly(); criticalResolvedWaveV513 = Wave; }
        void ResetCriticalTasksV513()
        { criticalPenaltiesV513.Clear(); criticalResolvedTasksV513 = null; criticalResolvedWaveV513 = -1; }

        // Called only by ResolveDisaster immediately after CalculateDamage. Analytical CalculateDamage
        // callers retain baseline semantics. ResolveDisaster's waveResolved guard owns the single charge.
        // Capture the entry task state next, before downgrade or supply consumption.
        void ApplyCriticalTaskPenaltyV513()
        {
            criticalPenaltiesV513.Clear();
            if (!CriticalTaskPenaltiesEnabledV513) return;
            var tasks = BuildCriticalTasksV513(false);
            var targets = new Dictionary<string, PanicItem>(StringComparer.Ordinal);
            float requested = 0;
            foreach (var row in tasks)
            {
                if (row.Complete || row.PenaltyPoints <= 0) continue;
                PanicItem target = CriticalPenaltyTargetV513(row.Hazard);
                if (target == null) continue;
                targets[row.Id] = target; requested += row.PenaltyPoints;
            }
            if (requested <= 0) return;
            float taskScale = Mathf.Min(1f, CriticalPenaltyCapV513 / requested);
            foreach (var row in tasks)
            {
                PanicItem target;
                if (!targets.TryGetValue(row.Id, out target)) continue;
                float points = row.PenaltyPoints * taskScale;
                target.Contribution += points;
                hazardDamageV54[(int)row.Hazard] += points;
                criticalPenaltiesV513.Add(new PanicCriticalPenaltyV513(row.Id, row.Label, target.Id, row.Hazard, points));
            }
            float sum = 0;
            foreach (var item in Items) sum += item.Contribution;
            float capScale = sum > 100f ? 100f / sum : 1f;
            foreach (var item in Items)
            {
                item.Contribution *= capScale;
                float risk = 0;
                for (int n = 0; n < 4; n++) if (HasActiveDisasterV54((DisasterType)n))
                    risk += BaseRisk(item, (DisasterType)n) * ((DisasterType)n == Disaster ? 1f : .45f);
                item.Damage = risk <= 0 ? 0 : Mathf.Clamp01(item.Contribution / risk) * 100f;
            }
            for (int n = 0; n < 4; n++) hazardDamageV54[n] *= capScale;
            SecondaryDamageV5 *= capScale;
            SecondaryEventSummaryV5 = string.Empty;
            for (int n = 0; n < secondaryEventsV5.Count; n++)
            {
                var e = secondaryEventsV5[n];
                secondaryEventsV5[n] = new PanicSecondaryEventV5(e.Kind, e.TargetId, e.DamagePoints * capScale, e.Label);
                if (SecondaryEventSummaryV5.Length > 0) SecondaryEventSummaryV5 += " · ";
                SecondaryEventSummaryV5 += e.Label + " (+" + FormatPoints(e.DamagePoints * capScale) + " yüzde puan).";
            }
            for (int n = 0; n < criticalPenaltiesV513.Count; n++)
            {
                var e = criticalPenaltiesV513[n];
                criticalPenaltiesV513[n] = new PanicCriticalPenaltyV513(e.Id, e.Label, e.TargetId, e.Hazard, e.DamagePoints * capScale);
            }
            TotalDamage = Mathf.Min(100f, sum * capScale);
        }
        PanicItem CriticalPenaltyTargetV513(DisasterType hazard)
        {
            PanicItem target = null; float strongest = -1;
            foreach (var item in Items)
            {
                if (item == null || !item.Installed || hazard == DisasterType.Storm && item.Kind != ItemKind.Roof) continue;
                if (hazard != DisasterType.Storm && item.Kind != ItemKind.Furniture) continue;
                float risk = BaseRisk(item, hazard);
                if (risk > 0 && risk > strongest) { strongest = risk; target = item; }
            }
            return target;
        }
        float CriticalPenaltyForItemV513(string id)
        { float sum = 0; foreach (var e in criticalPenaltiesV513) if (e.TargetId == id) sum += e.DamagePoints; return sum; }
        void AppendCriticalPenaltyLedgerRowsV513(List<PanicWaveLedgerRowV56> rows)
        {
            foreach (var e in criticalPenaltiesV513)
                rows.Add(new PanicWaveLedgerRowV56("critical:" + e.Id, "Kritik görev: " + e.Label,
                    DisasterName(e.Hazard) + " · görev tamamlanmadı · " + e.TargetId, e.DamagePoints));
        }
    }
}
