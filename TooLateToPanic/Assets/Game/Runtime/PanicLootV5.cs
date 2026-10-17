using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicExpedition
    {
        public int StockedWaveV5 { get; private set; }
        public int LootLayoutRevisionV5 { get; private set; }

        void ResetWaveLootV5()
        {
            StockedWaveV5 = -1;
            LootLayoutRevisionV5 = 0;
            RestockWaveLootV5();
        }

        void RestockWaveLootV5()
        {
            // Only Reset or guarded OnNewWave calls this. All rolls become actual finite
            // stock now; world rendering and opening/closing a cupboard never roll dice.
            foreach (var source in Sources)
            {
                List<SupplySlot> blueprint;
                if (!sourceBlueprints.TryGetValue(source.Id, out blueprint)) continue;
                source.Contents = CloneSlots(blueprint);
                source.AutoLooted = source.Open = false;
                foreach (var slot in source.Contents)
                {
                    if (!HasStock(slot) || !IsConstructionMaterial(slot.Id)) continue;
                    int roll = lootRandom.Next(4);
                    int delta = roll == 0 ? -1 : roll == 3 ? 1 : 0;
                    // All three authored mature tree-row packages remain available.
                    slot.Count = slot.Id == "seedling" ? Mathf.Max(slot.Count, slot.Count + delta) : Mathf.Max(1, slot.Count + delta);
                }
            }

            // A small exchange among real containers on the same floor changes where
            // optional materials are found without moving colliders or blocking routes.
            foreach (int floor in new[] { -1, 0, 1 })
            {
                var candidates = Sources.FindAll(s => s.Floor == floor && s.Contents != null && s.Contents.Count > 0);
                if (candidates.Count < 2 || lootRandom.NextDouble() >= .5) continue;
                int a = lootRandom.Next(candidates.Count);
                int b = (a + 1 + lootRandom.Next(candidates.Count - 1)) % candidates.Count;
                LootSource first = candidates[a], second = candidates[b];
                int left = FindSwappableSlotV5(first.Contents), right = FindSwappableSlotV5(second.Contents);
                if (left < 0 || right < 0) continue;
                SupplySlot swap = first.Contents[left];
                first.Contents[left] = second.Contents[right];
                second.Contents[right] = swap;
            }

            foreach (var supply in worldSupplies)
            {
                // Small x/z offsets remain inside authored surfaces. Position's setter
                // always receives the support seat, so there is no per-frame drift.
                float x = ((float)lootRandom.NextDouble() - .5f) * .07f;
                float z = ((float)lootRandom.NextDouble() - .5f) * .07f;
                supply.SeatOffsetV5 = new Vector3(x, 0f, z);
                if (IsRenewable(supply.ItemId))
                    supply.Count = Mathf.Max(1, supply.InitialCount + lootRandom.Next(-1, 2));
                else
                    supply.Count = CountSlots(Inventory, supply.ItemId) + CountSlots(Bag, supply.ItemId) > 0 ? 0 : supply.InitialCount;
            }

            StockedWaveV5 = Model == null ? 0 : Model.Wave;
            LootLayoutRevisionV5++;
        }

        int FindSwappableSlotV5(List<SupplySlot> contents)
        {
            var candidates = new List<int>();
            for (int i = 0; i < contents.Count; i++)
                if (HasStock(contents[i]) && IsConstructionMaterial(contents[i].Id) && contents[i].Id != "seedling") candidates.Add(i);
            return candidates.Count == 0 ? -1 : candidates[lootRandom.Next(candidates.Count)];
        }
    }
}
