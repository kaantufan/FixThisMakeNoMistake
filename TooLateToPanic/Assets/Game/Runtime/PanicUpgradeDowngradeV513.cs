using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Immutable committed change. Damage remains the just-resolved, pre-downgrade damage.</summary>
    public sealed class PanicUpgradeDowngradeOutcomeV513
    {
        public int Wave { get; }
        public string GroupId { get; }
        public string GroupLabel { get; }
        public string ItemId { get; }
        public string ItemLabel { get; }
        public int BeforeTier { get; }
        public int AfterTier { get; }
        public float DamageContribution { get; }
        public float Chance { get; }
        public string Detail { get { return ItemLabel + " · T" + BeforeTier + " → T" + AfterTier; } }
        internal PanicUpgradeDowngradeOutcomeV513(int wave, string groupId, string groupLabel,
            PanicItem item, int before, float chance)
        {
            Wave = wave; GroupId = groupId; GroupLabel = groupLabel; ItemId = item.Id;
            ItemLabel = item.Label; BeforeTier = before; AfterTier = item.Tier;
            DamageContribution = item.Contribution; Chance = chance;
        }
    }

    public sealed partial class PanicModel
    {
        readonly HashSet<int> upgradeResolvedWavesV513 = new HashSet<int>();
        readonly List<PanicUpgradeDowngradeOutcomeV513> upgradeDowngradesV513 = new List<PanicUpgradeDowngradeOutcomeV513>();
        IReadOnlyList<PanicUpgradeDowngradeOutcomeV513> upgradeDowngradeViewV513;
        public IReadOnlyList<PanicUpgradeDowngradeOutcomeV513> UpgradeDowngradeOutcomesV513
        { get { return upgradeDowngradeViewV513 ?? (upgradeDowngradeViewV513 = upgradeDowngradesV513.AsReadOnly()); } }
        public int UpgradeDowngradeWaveV513 { get; private set; }
        public string UpgradeDowngradeSummaryV513 { get; private set; } = string.Empty;

        void ResetUpgradeDowngradesV513()
        {
            upgradeResolvedWavesV513.Clear(); upgradeDowngradesV513.Clear();
            UpgradeDowngradeWaveV513 = 0; UpgradeDowngradeSummaryV513 = string.Empty;
        }

        sealed class UpgradeDamageGroupV513
        {
            internal string Id, Label;
            internal readonly List<PanicItem> Items = new List<PanicItem>();
            internal float Contribution, WeightedDamage;
        }

        /// <summary>
        /// Call ONLY from ResolveDisaster, after final composed damage/critical-task snapshot,
        /// while Phase is Disaster and waveResolved is true. Never call from CalculateDamage.
        /// It changes next-wave Tier only: no budget, anchors, current damage or score mutations.
        /// </summary>
        void ResolveUpgradeDowngradesV513()
        {
            if (!waveResolved || Phase != PanicPhase.Disaster || Wave < 1 || IsTutorial || IsSimulation || IsJuryDemo ||
                DeveloperNoLoss || DeveloperNoLossUsedThisWave || DeveloperWaveSkipProtectionV51 ||
                upgradeResolvedWavesV513.Contains(Wave)) return;
            upgradeResolvedWavesV513.Add(Wave);
            upgradeDowngradesV513.Clear(); UpgradeDowngradeSummaryV513 = string.Empty;
            UpgradeDowngradeWaveV513 = Wave;
            if (Items == null) return;

            var groups = new Dictionary<string, UpgradeDamageGroupV513>(StringComparer.Ordinal);
            var seenItems = new HashSet<PanicItem>();
            foreach (var item in Items)
            {
                // Membership follows the actual upgrade group. Damage qualifies the group,
                // not each member: an undamaged installed roof stays linked to its upper walls.
                if (item == null || !seenItems.Add(item) || !item.Installed || !IsStructure(item) || item.Tier < 0 || item.Tier > 2 ||
                    string.IsNullOrEmpty(item.Id)) continue;
                string id, label;
                if (item.Kind == ItemKind.Wall || item.Kind == ItemKind.Roof)
                {
                    if (float.IsNaN(item.Position.y) || float.IsInfinity(item.Position.y)) continue;
                    int floor = item.Kind == ItemKind.Roof ? 1 : Mathf.FloorToInt((item.Position.y + .15f) / 3.2f);
                    id = "floor:" + floor;
                    label = floor == 1 ? "Üst kat duvarları ve çatı" : floor == 0 ? "Zemin kat duvarları" :
                        floor == -1 ? "Bodrum duvarları" : floor + ". kat duvarları";
                }
                else { id = "item:" + item.Id; label = item.Label; }
                UpgradeDamageGroupV513 group;
                if (!groups.TryGetValue(id, out group))
                { group = new UpgradeDamageGroupV513 { Id = id, Label = label }; groups.Add(id, group); }
                if (item.Tier > 0) group.Items.Add(item);
                if (FinitePositiveUpgradeDamageV513(item.Contribution) && FinitePositiveUpgradeDamageV513(item.Damage))
                {
                    group.Contribution += item.Contribution;
                    group.WeightedDamage += Mathf.Clamp(item.Damage, 0f, 100f) * item.Contribution;
                }
            }

            var keys = new List<string>(groups.Keys); keys.Sort(StringComparer.Ordinal);
            var summary = new StringBuilder();
            foreach (string key in keys)
            {
                UpgradeDamageGroupV513 group = groups[key];
                // No installed upgrades, or no actual positive group damage, means no roll/drop.
                if (group.Items.Count == 0 || !FinitePositiveUpgradeDamageV513(group.Contribution)) continue;
                float actualDamage = group.WeightedDamage / group.Contribution;
                float chance = .10f + .20f * Mathf.Clamp01(actualDamage / 100f);
                // A group has ONE stable draw independent from iteration order, Unity RNG or UI reads.
                if (UpgradeDowngradeRollV513(compoundSeedV54, Wave, group.Id) >= chance) continue;
                group.Items.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
                int minBefore = 2, maxBefore = 0, minAfter = 2, maxAfter = 0;
                foreach (var item in group.Items)
                {
                    int before = item.Tier; item.Tier = Math.Max(0, before - 1);
                    minBefore = Math.Min(minBefore, before); maxBefore = Math.Max(maxBefore, before);
                    minAfter = Math.Min(minAfter, item.Tier); maxAfter = Math.Max(maxAfter, item.Tier);
                    upgradeDowngradesV513.Add(new PanicUpgradeDowngradeOutcomeV513(Wave, group.Id, group.Label, item, before, chance));
                }
                if (summary.Length > 0) summary.Append(" · ");
                summary.Append(group.Label).Append(": ").Append(UpgradeTierRangeV513(minBefore, maxBefore))
                    .Append(" → ").Append(UpgradeTierRangeV513(minAfter, maxAfter));
            }
            if (summary.Length > 0)
                UpgradeDowngradeSummaryV513 = "GÜÇLENDİRME HASARI: " + summary + ". Sonraki dalga için yeniden güçlendirebilirsin.";
        }

        static bool FinitePositiveUpgradeDamageV513(float value)
        { return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f; }
        static string UpgradeTierRangeV513(int low, int high)
        { return low == high ? "T" + low : "T" + low + "–T" + high; }
        static float UpgradeDowngradeRollV513(uint seed, int wave, string group)
        {
            unchecked
            {
                uint hash = 2166136261u ^ seed ^ ((uint)wave * 0x9E3779B9u) ^ 0x513D07A1u;
                for (int i = 0; i < group.Length; i++) hash = (hash ^ group[i]) * 16777619u;
                hash ^= hash >> 16; hash *= 0x7FEB352Du; hash ^= hash >> 15;
                hash *= 0x846CA68Bu; hash ^= hash >> 16;
                return (hash & 0x00FFFFFFu) / 16777216f;
            }
        }
    }
}
