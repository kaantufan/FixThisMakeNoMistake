using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicModel
    {
        uint compoundSeedV54;
        int compoundMaskV54, compoundWaveV54;
        DisasterType compoundPrimaryV54;
        readonly float[] hazardDamageV54 = new float[4];
        public int ActiveDisasterMaskV54
        {
            get
            {
                if (!IsSimulation && !IsTutorial && !IsJuryDemo && compoundWaveV54 == Wave &&
                    compoundPrimaryV54 == Disaster && compoundMaskV54 != 0) return compoundMaskV54;
                return 1 << (int)Disaster;
            }
        }
        public bool HasActiveDisasterV54(DisasterType type)
        { return (int)type >= 0 && (int)type < 4 && (ActiveDisasterMaskV54 & (1 << (int)type)) != 0; }
        public int ActiveDisasterCountV54
        { get { int count = 0; for (int i = 0; i < 4; i++) if ((ActiveDisasterMaskV54 & (1 << i)) != 0) count++; return count; } }
        public string ActiveDisasterLabelV54
        {
            get
            {
                string label = DisasterName(Disaster);
                for (int i = 0; i < 4; i++) if ((DisasterType)i != Disaster && HasActiveDisasterV54((DisasterType)i))
                    label += " + " + DisasterName((DisasterType)i);
                return label;
            }
        }
        public float HazardDamageContributionV54(DisasterType type)
        { return (int)type >= 0 && (int)type < 4 ? hazardDamageV54[(int)type] : 0; }
        void ResetCompoundDisastersV54()
        {
            compoundSeedV54 = unchecked((uint)Guid.NewGuid().GetHashCode());
            if (compoundSeedV54 == 0) compoundSeedV54 = 0x54C1A0u;
            compoundMaskV54 = compoundWaveV54 = 0;
            Array.Clear(hazardDamageV54, 0, hazardDamageV54.Length);
        }
        void SelectCompoundDisastersV54()
        {
            Array.Clear(hazardDamageV54, 0, hazardDamageV54.Length);
            compoundWaveV54 = Wave;
            if (Wave <= 1 || IsTutorial || IsJuryDemo)
            { Disaster = compoundPrimaryV54 = DisasterType.Earthquake; compoundMaskV54 = 1; return; }
            // V5.9 final policy: second wave is the same taught combined threat.
            if (Wave == 2)
            {
                Disaster = compoundPrimaryV54 = DisasterType.Flood;
                compoundMaskV54 = (1 << (int)DisasterType.Flood) | (1 << (int)DisasterType.Storm);
                return;
            }
            uint state = compoundSeedV54 ^ unchecked((uint)Wave * 0x9E3779B9u);
            // Draw once per wave, independent from Unity/visual RNG and damage variation.
            int roll = (int)(NextUInt(ref state) % 100u);
            int count = roll < 50 ? 1 : roll < 85 ? 2 : 3;
            int[] pool = { 0, 1, 2, 3 };
            compoundMaskV54 = 0;
            for (int i = 0; i < count; i++)
            {
                int selected = i + (int)(NextUInt(ref state) % (uint)(4 - i));
                int value = pool[selected]; pool[selected] = pool[i]; pool[i] = value;
                if (i == 0) Disaster = compoundPrimaryV54 = (DisasterType)value;
                compoundMaskV54 |= 1 << value;
            }
        }
        void CalculateDamage()
        {
            Array.Clear(hazardDamageV54, 0, hazardDamageV54.Length);
            int mask = ActiveDisasterMaskV54;
            DisasterType primary = Disaster;
            if (ActiveDisasterCountV54 <= 1)
            {
                CalculateSingleDisasterDamageV54();
                hazardDamageV54[(int)primary] = TotalDamage;
                return;
            }
            var contributions = new float[Items.Count];
            var baseRisks = new float[Items.Count];
            var secondary = new List<PanicSecondaryEventV5>();
            float combined = 0, secondaryTotal = 0, strongest = 0, totalWeight = 0;
            int activeHazards = 0;
            try
            {
                for (int n = 0; n < 4; n++)
                {
                    DisasterType type = (DisasterType)n;
                    if ((mask & (1 << n)) == 0) continue;
                    Disaster = type;
                    float weight = type == primary ? 1f : .45f;
                    CalculateSingleDisasterDamageV54(true);
                    strongest = Mathf.Max(strongest, TotalDamage);
                    totalWeight += weight; activeHazards++;
                    hazardDamageV54[n] = TotalDamage * weight;
                    combined += hazardDamageV54[n];
                    for (int i = 0; i < Items.Count; i++)
                    {
                        contributions[i] += Items[i].Contribution * weight;
                        baseRisks[i] += BaseRisk(Items[i], type) * weight;
                    }
                    secondaryTotal += SecondaryDamageV5 * weight;
                    foreach (var e in secondaryEventsV5)
                        secondary.Add(new PanicSecondaryEventV5(e.Kind, e.TargetId, e.DamagePoints * weight,
                            DisasterName(type) + ": " + e.Label));
                }
            }
            finally { Disaster = primary; }
            // Simultaneous hazards hit one house. Preserve the strongest actual hazard,
            // then add 3% of the primary-weighted mean for each additional hazard.
            // This is always at least the strongest isolated risk (before the shared
            // 100 cap), while avoiding repeated full damage to the same household.
            // Keep the original primary/secondary item shares and utility attribution;
            // one common scale conserves their sum and the memoized event points.
            float mean = totalWeight > 0 ? combined / totalWeight : 0;
            float composed = strongest + .03f * Mathf.Max(0, activeHazards - 1) * mean;
            float scale = combined > 0 ? Mathf.Min(100f, composed) / combined : 0;
            float total = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Contribution = Mathf.Max(0, contributions[i] * scale);
                Items[i].Damage = baseRisks[i] <= 0 ? 0 : Mathf.Clamp01(Items[i].Contribution / baseRisks[i]) * 100f;
                total += Items[i].Contribution;
            }
            for (int i = 0; i < 4; i++) hazardDamageV54[i] *= scale;
            TotalDamage = Mathf.Clamp(total, 0, 100);
            secondaryEventsV5.Clear();
            SecondaryDamageV5 = secondaryTotal * scale;
            SecondaryEventSummaryV5 = string.Empty;
            foreach (var e in secondary)
            {
                float points = e.DamagePoints * scale;
                secondaryEventsV5.Add(new PanicSecondaryEventV5(e.Kind, e.TargetId, points, e.Label));
                if (SecondaryEventSummaryV5.Length > 0) SecondaryEventSummaryV5 += " · ";
                SecondaryEventSummaryV5 += e.Label + " (+" + FormatPoints(points) + " yüzde puan).";
            }
        }
    }
}
