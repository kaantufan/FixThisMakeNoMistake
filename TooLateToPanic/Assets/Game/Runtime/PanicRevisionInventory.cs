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
            int amount = supply.Count;
            // Every emergency item first uses the carried bag's real slots and limits.
            // Candidates own both storage and device battery costs until one publication;
            // overflow stays in inventory or in the finite world stack, never disappears.
            var candidateInventory = CloneSlots(Inventory);
            var candidateBag = CloneSlots(Bag);
            int packed = AutoPackCapacityV51(candidateBag, supply.ItemId, amount);
            int batteryCost = 0;
            if (amount > 0 && IsPoweredBagDeviceV51(supply.ItemId) && CountSlots(candidateBag, supply.ItemId) == 0 && !Model.FreeMaterials)
            {
                // A final packed battery can free the eighth slot when installed.
                if (TryConsumePackingBatteriesV51(candidateInventory, candidateBag, 1))
                {
                    packed = AutoPackCapacityV51(candidateBag, supply.ItemId, amount);
                    if (packed > 0) batteryCost = 1;
                    else { candidateInventory = CloneSlots(Inventory); candidateBag = CloneSlots(Bag); }
                }
                else packed = 0;
            }
            string reason;
            if (packed > 0 && (!TryAddPartial(candidateBag, supply.ItemId, packed) || !ValidBag(candidateBag, out reason)))
            { candidateInventory = CloneSlots(Inventory); candidateBag = CloneSlots(Bag); packed = 0; batteryCost = 0; }
            int loose = Mathf.Min(amount - packed, AvailableSupplyCapacityV5(candidateInventory, supply.ItemId));
            amount = packed + loose;
            if (amount <= 0) return Fail("Envanter dolu; eşya yerinde kaldı.");
            if (loose > 0 && !TryAddPartial(candidateInventory, supply.ItemId, loose))
                return Fail("Envanter dolu; eşya yerinde kaldı.");
            PublishSlots(Inventory, candidateInventory);
            PublishSlots(Bag, candidateBag);
            Model.PackedWater = CountSlots(Bag, "water");
            Model.PackedFood = CountSlots(Bag, "food");
            Model.PackedSupportMask = SupportMask(Bag);
            Model.SupportOwnedMask = SupportMask(Bag) | SupportMask(Inventory);
            supply.Count -= amount;
            LastWorldSupplyCollectedCountV5 = amount;
            LastWorldSupplyPackedCountV51 = packed;
            LastWorldSupplyInventoryCountV51 = loose;
            LastWorldSupplyBatteryCostV51 = batteryCost;
            if (Model.IsJuryDemo && supply.ItemId == "water") JuryWaterGatheredCount += amount;
            Message = packed > 0 ? DisplayName(supply.ItemId) + " ×" + packed + " çantaya kondu." +
                (loose > 0 ? " Kalan " + loose + " envanterde." : string.Empty) : DisplayName(supply.ItemId) + " ×" + amount + " alındı.";
            if (supply.Count > 0) Message += " Kalan " + supply.Count + " yerinde.";
            NotifyModel(); return true;
        }

        static bool IsPoweredBagDeviceV51(string id) { return id == "flashlight" || id == "radio"; }

        static int AutoPackCapacityV51(List<SupplySlot> slots, string id, int requested)
        {
            int capacity = Mathf.Min(requested, AvailableSupplyCapacityV5(slots, id));
            int maximum = id == "water" || id == "food" ? 3 :
                id == "firstaid" || id == "flashlight" || id == "radio" || id == "blanket" ? 1 : int.MaxValue;
            return Mathf.Min(capacity, Mathf.Max(0, maximum - CountSlots(slots, id)));
        }

        /// <summary>A charged device still costs one real battery. Packed batteries remain usable.
        /// Preflight before either consume, including bag-to-inventory device swaps.</summary>
        static bool TryConsumePackingBatteriesV51(List<SupplySlot> inventory, List<SupplySlot> bag, int count)
        {
            if (count <= 0) return true;
            int loose = CountSlots(inventory, "battery"), carried = CountSlots(bag, "battery");
            if ((long)loose + carried < count) return false;
            int fromInventory = Mathf.Min(loose, count);
            ConsumeFromSlots(inventory, "battery", fromInventory);
            ConsumeFromSlots(bag, "battery", count - fromInventory);
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
            if (index<0 || index>=Bag.Count || !HasStock(Bag[index])) return Fail("Bu çanta yuvası boş.");
            int target=FindStorageSlot(Inventory,Bag[index].Id,1);
            return target>=0 ? TransferSlots(true,index,false,target,1) : Fail("Envanter dolu.");
        }

        /// <summary>Exact-slot drop: merge to limit and leave remainder, or swap complete stacks.
        /// Cross-boundary changes validate a candidate bag and publish both arrays/model together.</summary>
        public bool TransferSlots(bool fromBag,int fromIndex,bool toBag,int toIndex,int count=0)
        {
            if (!CanPrepare()) return Fail("Çantayı hazırlıkta düzenleyebilirsin.");
            var source=fromBag?Bag:Inventory; var destination=toBag?Bag:Inventory;
            if (fromIndex<0||fromIndex>=source.Count||toIndex<0||toIndex>=destination.Count||count<0)
                return Fail("Yuva seçimi geçersiz.");
            if (fromBag==toBag && fromIndex==toIndex) return false;
            if (!HasStock(source[fromIndex]) || !IsSupply(source[fromIndex].Id)) return Fail("Bu yuva boş.");
            var candidateInventory=CloneSlots(Inventory);var candidateBag=CloneSlots(Bag);
            var a=fromBag?candidateBag:candidateInventory;var b=toBag?candidateBag:candidateInventory;
            SupplySlot moving=a[fromIndex], target=b[toIndex];
            int requested=count==0?moving.Count:count;
            if (requested>moving.Count) return Fail("Yığında yeterli eşya yok.");
            int moved=requested;
            if (!HasStock(target)||target.Id==moving.Id)
            {
                moved=Mathf.Min(requested,StackLimitFor(moving.Id)-(HasStock(target)?target.Count:0));
                if (moved<=0) return Fail("Bu yığın dolu.");
                if (!HasStock(target)) b[toIndex]=new SupplySlot(moving.Id,moved);else target.Count+=moved;
                moving.Count-=moved;if(moving.Count==0)moving.Id=string.Empty;
            }
            else
            {
                if (requested!=moving.Count) return Fail("Farklı eşya ile yer değiştirmek için tüm yığını taşı.");
                a[fromIndex]=target;b[toIndex]=moving;
            }
            // Newly packed devices consume one real battery each; moving inside bag consumes nothing.
            int batteries=0;
            if (fromBag!=toBag && !Model.FreeMaterials)
            {
                batteries+=Mathf.Max(0,CountSlots(candidateBag,"flashlight")-CountSlots(Bag,"flashlight"));
                batteries+=Mathf.Max(0,CountSlots(candidateBag,"radio")-CountSlots(Bag,"radio"));
                if (!TryConsumePackingBatteriesV51(candidateInventory,candidateBag,batteries))
                    return RequestRequirements("packbattery","battery");
            }
            string reason;
            if (!ValidBag(candidateBag,out reason)) return Fail(reason);
            PublishSlots(Inventory,candidateInventory);PublishSlots(Bag,candidateBag);
            Model.PackedWater=CountSlots(Bag,"water");Model.PackedFood=CountSlots(Bag,"food");
            Model.PackedSupportMask=SupportMask(Bag);Model.SupportOwnedMask=SupportMask(Bag)|SupportMask(Inventory);
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
        static bool ValidBag(List<SupplySlot> slots,out string reason)
        {
            reason=string.Empty;
            foreach(var slot in slots)if(HasStock(slot)&&(!IsSupply(slot.Id)||slot.Count>StackLimitFor(slot.Id)))
            {reason="Bu eşya çantaya sığmıyor.";return false;}
            if(CountSlots(slots,"water")>3||CountSlots(slots,"food")>3)
            {reason="Çantada en fazla üçer günlük su ve gıda taşınabilir.";return false;}
            foreach(var id in new[]{"firstaid","flashlight","radio","blanket"})if(CountSlots(slots,id)>1)
            {reason=DisplayName(id)+" zaten çantada.";return false;}
            return true;
        }
    }
}
