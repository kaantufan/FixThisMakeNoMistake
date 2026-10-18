using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public enum PanicShelterKindV5 { None, Basement, PanicRoom }
    public enum PanicSecondaryEventKindV5 { GasFlare, PipeLeak, ElectricalShort }

    /// <summary>An immutable result of one actual wave/disaster evaluation.</summary>
    public sealed class PanicSecondaryEventV5
    {
        public PanicSecondaryEventKindV5 Kind { get; }
        public string TargetId { get; }
        public float DamagePoints { get; }
        public string Label { get; }
        public PanicSecondaryEventV5(PanicSecondaryEventKindV5 kind, string target, float points, string label)
        { Kind = kind; TargetId = target; DamagePoints = points; Label = label; }
    }

    public sealed partial class PanicModel
    {
        public PanicShelterKindV5 RequiredShelterV5
        { get { return HasActiveDisasterV54(DisasterType.Flood) ? PanicShelterKindV5.PanicRoom : PanicShelterKindV5.Basement; } }
        public string RequiredShelterLabelV5
        { get { return HasActiveDisasterV54(DisasterType.Flood) ? "üst kattaki panik odası" : "bodrum sığınağı"; } }
        public string RequiredShelterDestinationV5
        { get { return HasActiveDisasterV54(DisasterType.Flood) ? "üst kattaki panik odasına" : "bodrum sığınağına"; } }
        public PanicShelterKindV5 EnteredShelterV5 { get; private set; }
        public bool ReachedShelterV5 { get { return EnteredShelterV5 != PanicShelterKindV5.None; } }
        public float SecondaryDamageV5 { get; private set; }
        public string SecondaryEventSummaryV5 { get; private set; } = string.Empty;
        public int SecondaryRollCountV5 { get; private set; }
        public IReadOnlyList<PanicSecondaryEventV5> SecondaryEventsV5 { get { return secondaryEventsV5.AsReadOnly(); } }

        readonly List<PanicSecondaryEventV5> secondaryEventsV5 = new List<PanicSecondaryEventV5>();
        readonly Dictionary<string, PanicSecondaryEventV5> secondaryRollsV5 = new Dictionary<string, PanicSecondaryEventV5>(StringComparer.Ordinal);
        uint secondarySeedV5;

        /// <summary>Caller owns real location, floor, path and input. Wrong disaster destinations never commit.</summary>
        public bool TryEnterShelterV5(PanicShelterKindV5 shelter)
        {
            if (IsJuryDemo) return RefuseBool("İşaretli güvenli alanı kullan.");
            if (!CanPrepare()) return RefuseBool("Güvenli alana yalnızca hazırlık sırasında girebilirsin.");
            if (shelter != RequiredShelterV5)
                return RefuseBool(HasActiveDisasterV54(DisasterType.Flood) ? "Sel için bodruma inme; üst kattaki panik odasına çık." : "Bu afet için bodrum sığınağını kullan.");
            EnteredShelterV5 = shelter;
            BeginShelterDisasterV5();
            return true;
        }

        void BeginShelterDisasterV5()
        {
            TimeRemaining = 0f;
            PhaseTime = 8f;
            Phase = PanicPhase.Disaster;
            waveResolved = pendingSuccess = awardApplied = false;
            Message = ActiveDisasterLabelV54 + " başladı. Hazırlık ve alınan önlemler değerlendiriliyor.";
        }

        void BeginUnshelteredDisasterV5()
        {
            EnteredShelterV5 = PanicShelterKindV5.None;
            BeginShelterDisasterV5();
        }

        void ResetSystemsV5()
        {
            secondaryRollsV5.Clear();
            secondarySeedV5 = unchecked((uint)Guid.NewGuid().GetHashCode());
            SecondaryRollCountV5 = 0;
            BeginWaveSystemsV5();
        }

        void BeginWaveSystemsV5()
        {
            EnteredShelterV5 = PanicShelterKindV5.None;
            secondaryEventsV5.Clear();
            SecondaryDamageV5 = 0f;
            SecondaryEventSummaryV5 = string.Empty;
        }

        void ApplySecondaryDamageV5(bool deferCapV54 = false)
        {
            secondaryEventsV5.Clear();
            SecondaryDamageV5 = 0f;
            SecondaryEventSummaryV5 = string.Empty;
            string key = Wave + ":" + (int)Disaster;
            PanicSecondaryEventV5 rolled;
            if (!secondaryRollsV5.TryGetValue(key, out rolled))
            {
                rolled = RollSecondaryEventV5();
                secondaryRollsV5[key] = rolled;
                SecondaryRollCountV5++;
            }
            if (rolled == null || SecondaryEventPreventedV5(rolled.Kind)) return;
            PanicItem target = FindItem(rolled.TargetId);
            if (target == null || !target.Installed || BaseRisk(target, Disaster) <= 0f) return;
            float points = deferCapV54 ? rolled.DamagePoints : Mathf.Min(rolled.DamagePoints, Mathf.Max(0f, 100f - TotalDamage));
            if (points <= 0f) return;
            // Add actual item contribution after ordinary normalization. Never reduce
            // existing primary damage, prices, collision rules or the 75% loss boundary.
            target.Contribution += points;
            target.Damage = Mathf.Clamp01(target.Contribution / BaseRisk(target, Disaster)) * 100f;
            TotalDamage += points;
            SecondaryDamageV5 = points;
            secondaryEventsV5.Add(new PanicSecondaryEventV5(rolled.Kind, rolled.TargetId, points, rolled.Label));
            SecondaryEventSummaryV5 = rolled.Label + " (+" + FormatPoints(points) + " yüzde puan).";
        }

        bool SecondaryEventPreventedV5(PanicSecondaryEventKindV5 kind)
        {
            return kind == PanicSecondaryEventKindV5.GasFlare ? UtilitySafety.GasClosed :
                kind == PanicSecondaryEventKindV5.PipeLeak ? UtilitySafety.WaterClosed : UtilitySafety.PowerOff;
        }

        PanicSecondaryEventV5 RollSecondaryEventV5()
        {
            // One per-run seed and a stable wave/type key. No RNG lives in Update,
            // render callbacks, item loops, captions, pauses or cutscene skip handlers.
            uint state = secondarySeedV5 ^ unchecked((uint)Wave * 0x9E3779B9u) ^ unchecked(((uint)Disaster + 1u) * 0x85EBCA6Bu);
            float gas = UtilitySafety.GasClosed ? 0f : Disaster == DisasterType.Earthquake ? .25f : Disaster == DisasterType.Fire ? .30f : 0f;
            float water = UtilitySafety.WaterClosed ? 0f : Disaster == DisasterType.Flood ? .32f : Disaster == DisasterType.Earthquake ? .12f : 0f;
            float power = UtilitySafety.PowerOff ? 0f : Disaster == DisasterType.Flood ? .23f : Disaster == DisasterType.Fire ? .18f : Disaster == DisasterType.Storm ? .25f : .10f;
            float total = gas + water + power;
            if (total <= 0f || RandomUnitV5(ref state) >= Mathf.Min(.55f, total)) return null;
            float selection = RandomUnitV5(ref state) * total;
            PanicSecondaryEventKindV5 kind = selection < gas ? PanicSecondaryEventKindV5.GasFlare :
                selection < gas + water ? PanicSecondaryEventKindV5.PipeLeak : PanicSecondaryEventKindV5.ElectricalShort;
            var targets = Items.FindAll(item => item != null && item.Installed && item.Kind == ItemKind.Furniture && BaseRisk(item, Disaster) > 0f);
            if (targets.Count == 0) return null;
            PanicItem target = targets[(int)(NextUInt(ref state) % (uint)targets.Count)];
            float damageRoll = RandomUnitV5(ref state);
            float points = kind == PanicSecondaryEventKindV5.GasFlare ? 6f + damageRoll * 4f :
                kind == PanicSecondaryEventKindV5.PipeLeak ? 4f + damageRoll * 3f :
                1.2f + damageRoll * 1.6f;
            string label = kind == PanicSecondaryEventKindV5.GasFlare ? "Gaz vanası açık kaldı: alevlenme hasarı oluştu" :
                kind == PanicSecondaryEventKindV5.PipeLeak ? "Su vanası açık kaldı: tesisatta su hasarı oluştu" :
                "Elektrik açık kaldı: küçük bir kısa devre oluştu";
            return new PanicSecondaryEventV5(kind, target.Id, points, label);
        }

        static float RandomUnitV5(ref uint state)
        { return (NextUInt(ref state) & 0x00FFFFFFu) / 16777216f; }
    }
}
