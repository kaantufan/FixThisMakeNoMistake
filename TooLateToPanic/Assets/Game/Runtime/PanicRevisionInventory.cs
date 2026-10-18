using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Finite world stock. World owns visibility, reach, ray tests and actual prop placement.</summary>
    public sealed class PanicWorldSupply
    {
        public string Id { get; private set; }
        public string ItemId { get; private set; }
        public int Count { get; internal set; }
        public int InitialCount { get; private set; }
        Vector3 seatPositionV5;
        public Vector3 SeatOffsetV5 { get; internal set; }
        // Setter stores the real support seat. The small run-owned offset is read once,
        // so repeated AlignSupplySupport calls cannot accumulate random position drift.
        public Vector3 Position { get { return seatPositionV5 + SeatOffsetV5; } set { seatPositionV5 = value; } }
        public int Floor { get; private set; }
        public bool Taken { get { return Count <= 0; } }
        internal PanicWorldSupply(string id, string item, int count, Vector3 position, int floor)
        { Id = id; ItemId = item; Count = InitialCount = count; Position = position; Floor = floor; }
    }

    public sealed partial class PanicExpedition
    {
        readonly Dictionary<string, int> materials = new Dictionary<string, int>(StringComparer.Ordinal);
        readonly List<PanicWorldSupply> worldSupplies = new List<PanicWorldSupply>();
        System.Random lootRandom;
        const int PoweredDeviceMask = (1 << 1) | (1 << 2);
        int chargedDeviceMask;
        public int ChargedDeviceMask { get { return chargedDeviceMask; } }
        public int LastWorldSupplyCollectedCountV5 { get; private set; }
        public int LastWorldSupplyPackedCountV51 { get; private set; }
        public int LastWorldSupplyInventoryCountV51 { get; private set; }
        public int LastWorldSupplyBatteryCostV51 { get; private set; }
        public static readonly string[] MaterialIds = { "screws", "bracket", "plank", "sandbag", "seedling", "hose", "extinguisher" };
        public IReadOnlyDictionary<string, int> Materials { get { return new ReadOnlyDictionary<string, int>(materials); } }
        public IReadOnlyList<PanicWorldSupply> WorldSupplies { get { return worldSupplies.AsReadOnly(); } }
        public static bool IsConstructionMaterial(string id)
        {
            return id == "screws" || id == "bracket" || id == "plank" || id == "sandbag" ||
                id == "seedling" || id == "hose" || id == "extinguisher";
        }
        static bool HasStock(SupplySlot slot) { return slot != null && slot.Count > 0 && !string.IsNullOrEmpty(slot.Id); }
        static bool IsSupply(string id)
        {
            return id == "water" || id == "food" || id == "firstaid" || id == "flashlight" ||
                id == "battery" || id == "radio" || id == "blanket";
        }
        public static bool IsEmergencyBagItemV51(string id) { return IsSupply(id); }
        int MaterialCount(string id) { int count; return id != null && materials.TryGetValue(id, out count) ? count : 0; }
        void AddMaterial(string id, int count)
        { if (count > 0 && IsConstructionMaterial(id)) materials[id] = (int)Math.Min(int.MaxValue, (long)MaterialCount(id) + count); }

        void ResetRevisionStock()
        {
            materials.Clear();
            chargedDeviceMask = 0;
            LastWorldSupplyCollectedCountV5 = 0;
            LastWorldSupplyPackedCountV51 = LastWorldSupplyInventoryCountV51 = LastWorldSupplyBatteryCostV51 = 0;
            lootRandom = new System.Random(Guid.NewGuid().GetHashCode());
            worldSupplies.Clear();
            // Positions are authored visible prop centers, not floor-projected container positions.
            Supply("supply_water_kitchen", "water", 2, new Vector3(4.08f,1.05f,4.7f),0);
            Supply("supply_food_kitchen", "food", 2, new Vector3(4.82f,1.05f,4.7f),0);
            Supply("supply_firstaid_bath", "firstaid", 1, new Vector3(4.8f,4.15f,4.8f),1);
            Supply("supply_flashlight_living", "flashlight",1,new Vector3(-3.9f,1.12f,-3),0);
            Supply("supply_battery_living", "battery",2,new Vector3(-3.3f,1.1f,-3),0);
            Supply("supply_radio_upper", "radio",1,new Vector3(4,4.3f,-2.5f),1);
            Supply("supply_battery_upper", "battery",1,new Vector3(4.55f,4.3f,-2.5f),1);
            Supply("supply_blanket_bed", "blanket",1,new Vector3(-4,4.1f,3.8f),1);
            Supply("supply_water_basement", "water",2,new Vector3(-5,-2.15f,4),-1);
            Supply("supply_food_basement", "food",2,new Vector3(-4.35f,-2.15f,4),-1);
            Supply("supply_water_upper", "water",2,new Vector3(3.6f,4.3f,-2.5f),1);
            Supply("supply_food_upper", "food",2,new Vector3(-4.6f,4.1f,3.8f),1);
            Supply("supply_water_pantry_v5", "water",2,new Vector3(6.3f,1.05f,-5.4f),0);
            Supply("supply_food_pantry_v5", "food",2,new Vector3(7.42f,1.05f,-4.15f),0);
            Supply("supply_battery_child_v5", "battery",1,new Vector3(-6.9f,3.81f,-3.8f),1);
            Supply("supply_firstaid_basement_v5", "firstaid",1,new Vector3(4,-2.391f,-3.8f),-1);
        }
        void Supply(string id,string item,int count,Vector3 position,int floor)
        { worldSupplies.Add(new PanicWorldSupply(id,item,count,position,floor)); }
        void PrepareShortFlowSupplies()
        {
            foreach (var supply in worldSupplies) supply.Count = supply.Id == "supply_water_kitchen" ? 1 : 0;
        }
        public PanicWorldSupply FindWorldSupply(string id)
        { return worldSupplies.Find(s => s.Id == id); }
        public bool TryCollectWorldSupply(string id)
        {
            LastWorldSupplyCollectedCountV5 = 0;
            LastWorldSupplyPackedCountV51 = LastWorldSupplyInventoryCountV51 = LastWorldSupplyBatteryCostV51 = 0;
            if (!CanPrepare()) return Fail("Erzakları hazırlıkta toplayabilirsin.");
            PanicWorldSupply supply = FindWorldSupply(id);
            if (supply == null || supply.Taken || !IsSupply(supply.ItemId)) return Fail("Bu eşya alınamıyor.");
            SynchronizeInventoryModel();
            // One candidate owns both accepted units and any installed batteries. A full
            // inventory can accept a device if its installed battery frees a slot, or
            // accept an arriving battery directly into a retained unpowered device.
            int requested = supply.Count;
            var candidateInventory = CloneSlots(Inventory);
            int candidateChargedMask = chargedDeviceMask;
            int batteryCost = 0;
            int directlyInstalled = 0;
            if (IsPoweredBagDeviceV51(supply.ItemId))
            {
                int bit = 1 << SupportIndexFor(supply.ItemId);
                if ((candidateChargedMask & bit) == 0 &&
                    (Model.FreeMaterials || CountSlots(candidateInventory, "battery") > 0))
                {
                    if (!Model.FreeMaterials) { ConsumeFromSlots(candidateInventory, "battery", 1); batteryCost++; }
                    candidateChargedMask |= bit;
                }
            }
            else if (supply.ItemId == "battery")
            {
                foreach (string device in new[] { "flashlight", "radio" })
                {
                    int bit = 1 << SupportIndexFor(device);
                    if ((candidateChargedMask & bit) != 0 || CountSlots(candidateInventory, device) <= 0) continue;
                    if (!Model.FreeMaterials)
                    {
                        if (CountSlots(candidateInventory, "battery") > 0) ConsumeFromSlots(candidateInventory, "battery", 1);
                        else if (directlyInstalled < requested) directlyInstalled++;
                        else continue;
                        batteryCost++;
                    }
                    candidateChargedMask |= bit;
                }
            }
            int stored = Mathf.Min(requested - directlyInstalled, AvailableSupplyCapacityV5(candidateInventory, supply.ItemId));
            int amount = directlyInstalled + stored;
            if (amount <= 0) return Fail("Envanter dolu; eşya yerinde kaldı.");
            if (stored > 0 && !TryAddPartial(candidateInventory, supply.ItemId, stored))
                return Fail("Envanter dolu; eşya yerinde kaldı.");
            string reason;
            if (!ValidInventory(candidateInventory, out reason)) return Fail(reason);
            PublishSlots(Inventory, candidateInventory);
            chargedDeviceMask = candidateChargedMask;
            supply.Count -= amount;
            LastWorldSupplyCollectedCountV5 = amount;
            LastWorldSupplyPackedCountV51 = 0;
            LastWorldSupplyInventoryCountV51 = amount;
            LastWorldSupplyBatteryCostV51 = batteryCost;
            if (Model.IsJuryDemo && supply.ItemId == "water") JuryWaterGatheredCount += amount;
            Message = DisplayName(supply.ItemId) + " ×" + amount + " alındı.";
            if (batteryCost > 0) Message += " " + batteryCost + " pil cihaza takıldı.";
            if (supply.Count > 0) Message += " Kalan " + supply.Count + " yerinde.";
            NotifyModel(); return true;
        }

        static bool IsPoweredBagDeviceV51(string id) { return id == "flashlight" || id == "radio"; }

        bool ActivateSelectedDevice(string id)
        {
            if (!IsPoweredBagDeviceV51(id)) return true;
            int bit = 1 << SupportIndexFor(id);
            if ((chargedDeviceMask & bit) != 0) return true;
            if (!Model.FreeMaterials && CountSlots(Inventory, "battery") <= 0) return false;
            if (!Model.FreeMaterials) ConsumeFromSlots(Inventory, "battery", 1);
            chargedDeviceMask |= bit;
            return true;
        }
        string AutoCollectMaterials(LootSource source)
        {
            if (source.AutoLooted) return "Açıldı · Buradaki malzemeleri aldın.";
            // The wave snapshot already owns the roll. Reopening cannot reroll or duplicate it.
            var rewards = new List<SupplySlot>();
            foreach (var slot in source.Contents)
                if (HasStock(slot) && IsConstructionMaterial(slot.Id))
                {
                    rewards.Add(new SupplySlot(slot.Id, slot.Count));
                }
            source.AutoLooted = true;
            var labels = new List<string>();
            foreach (var reward in rewards)
            { AddMaterial(reward.Id,reward.Count); labels.Add("+"+reward.Count+" "+DisplayName(reward.Id)); }
            source.Contents.Clear();
            return labels.Count == 0 ? "Açıldı · Boş." : string.Join(" · ",labels.ToArray());
        }

        static int AvailableSupplyCapacityV5(List<SupplySlot> slots, string id)
        {
            int limit = StackLimitFor(id), capacity = 0;
            foreach (var slot in slots)
                capacity += !HasStock(slot) ? limit : slot.Id == id ? Mathf.Max(0, limit - slot.Count) : 0;
            return capacity;
        }

        /// <summary>Preflight all partial stacks before mutating; no capacity means zero changes.</summary>
        static bool TryAddPartial(List<SupplySlot> slots,string id,int count)
        {
            if (!IsSupply(id) || count <= 0) return false;
            int limit = StackLimitFor(id), capacity = 0;
            foreach (var slot in slots)
                capacity += !HasStock(slot) ? limit : slot.Id == id ? Mathf.Max(0,limit-slot.Count) : 0;
            if (capacity < count) return false;
            for (int i=0;i<slots.Count && count>0;i++)
            {
                SupplySlot slot=slots[i];
                if (!HasStock(slot)||slot.Id!=id) continue;
                int amount=Mathf.Min(count,Mathf.Max(0,limit-slot.Count));slot.Count+=amount;count-=amount;
            }
            for (int i=0;i<slots.Count && count>0;i++)
            {
                if (HasStock(slots[i])) continue;
                int amount=Mathf.Min(count,limit);slots[i]=new SupplySlot(id,amount);count-=amount;
            }
            return true;
        }

        public bool UnpackSlot(int index)
        {
            if (!CanPrepare()) return Fail("Envanteri hazırlıkta düzenleyebilirsin.");
            SynchronizeInventoryModel();
            if (Inventory == null || index < 0 || index >= Inventory.Count || !HasStock(Inventory[index]) || !IsSupply(Inventory[index].Id))
                return Fail("Bu envanter yuvası boş.");
            ClearRequirements();
            Message = "Eşya aynı envanterde kaldı.";
            NotifyModel();
            return true;
        }

        /// <summary>Exact-slot drop: merge to limit and leave remainder, or swap complete stacks.
        /// Legacy bag flags address the same inventory. Clone and publish exactly once.</summary>
        public bool TransferSlots(bool fromBag,int fromIndex,bool toBag,int toIndex,int count=0)
        {
            if (!CanPrepare()) return Fail("Envanteri hazırlıkta düzenleyebilirsin.");
            if (Inventory == null || fromIndex<0||fromIndex>=Inventory.Count||toIndex<0||toIndex>=Inventory.Count||count<0)
                return Fail("Yuva seçimi geçersiz.");
            if (fromIndex==toIndex) return false;
            SynchronizeInventoryModel();
            if (!HasStock(Inventory[fromIndex]) || !IsSupply(Inventory[fromIndex].Id)) return Fail("Bu yuva boş.");
            var candidateInventory=CloneSlots(Inventory);
            SupplySlot moving=candidateInventory[fromIndex], target=candidateInventory[toIndex];
            int requested=count==0?moving.Count:count;
            if (requested>moving.Count) return Fail("Yığında yeterli eşya yok.");
            int moved=requested;
            if (!HasStock(target)||target.Id==moving.Id)
            {
                moved=Mathf.Min(requested,StackLimitFor(moving.Id)-(HasStock(target)?target.Count:0));
                if (moved<=0) return Fail("Bu yığın dolu.");
                if (!HasStock(target)) candidateInventory[toIndex]=new SupplySlot(moving.Id,moved);else target.Count+=moved;
                moving.Count-=moved;if(moving.Count==0)moving.Id=string.Empty;
            }
            else
            {
                if (requested!=moving.Count) return Fail("Farklı eşya ile yer değiştirmek için tüm yığını taşı.");
                candidateInventory[fromIndex]=target;candidateInventory[toIndex]=moving;
            }
            string reason;
            if (!ValidInventory(candidateInventory,out reason)) return Fail(reason);
            PublishSlots(Inventory,candidateInventory);
            ClearRequirements();
            Message="Eşya yerleştirildi.";NotifyModel();return true;
        }
        static int CountSlots(List<SupplySlot> slots,string id)
        { int n=0;foreach(var slot in slots)if(HasStock(slot)&&slot.Id==id)n+=slot.Count;return n; }
        static int SupportMask(List<SupplySlot> slots)
        {
            int mask=0;foreach(var slot in slots)if(HasStock(slot)){int index=SupportIndexFor(slot.Id);if(index>=0)mask|=1<<index;}return mask;
        }
        static void PublishSlots(List<SupplySlot> actual,List<SupplySlot> candidate)
        { for(int i=0;i<actual.Count;i++)actual[i]=new SupplySlot(candidate[i].Id,candidate[i].Count); }
        static bool ValidInventory(List<SupplySlot> slots,out string reason)
        {
            reason=string.Empty;
            foreach(var slot in slots)if(HasStock(slot)&&(!IsSupply(slot.Id)||slot.Count>StackLimitFor(slot.Id)))
            {reason="Bu eşya envantere sığmıyor.";return false;}
            return true;
        }
    }
}
