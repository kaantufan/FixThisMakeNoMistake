using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    [Serializable]
    public sealed class SupplySlot
    {
        public string Id;
        public int Count;

        public SupplySlot() { Id = string.Empty; Count = 0; }
        public SupplySlot(string id, int count) { Id = id ?? string.Empty; Count = Mathf.Max(0, count); }
    }

    [Serializable]
    public sealed class LootSource
    {
        public string Id;
        public string Label;
        public string ArtId;
        public Vector3 Position;
        public int Floor;
        public bool Open;
        public bool AutoLooted;
        public List<SupplySlot> Contents;
    }

    /// <summary>One canonical material cost. Inventory stock is never stored in a recipe.</summary>
    public readonly struct PanicMaterialCost
    {
        public readonly string Id;
        public readonly int Required;
        public PanicMaterialCost(string id, int required) { Id = id; Required = Mathf.Max(0, required); }
    }

    /// <summary>A recipe snapshot against numeric construction stock or loose bag supplies.</summary>
    public readonly struct PanicMaterialRequirement
    {
        public readonly string Id;
        public readonly int Required;
        public readonly int Owned;
        public readonly int Missing;
        public PanicMaterialRequirement(string id, int required, int owned)
        {
            Id = id; Required = Mathf.Max(0, required); Owned = Mathf.Max(0, owned);
            Missing = Mathf.Max(0, Required - Owned);
        }
        public PanicMaterialRequirement(string id, int required, int owned, int missing)
        {
            Id = id; Required = Mathf.Max(0, required); Owned = Mathf.Max(0, owned);
            Missing = Mathf.Max(0, missing);
        }
    }

    /// <summary>
    /// Run-scoped scavenging, packing, crafting and preparation work state. World owns
    /// proximity, interaction targets, colliders, movement and action presentation.
    /// </summary>
    public sealed partial class PanicExpedition
    {
        private const int InventoryCapacity = 12;
        private const int BagCapacity = 8;
        private const int StackLimit = 5;

        private readonly Dictionary<string, List<SupplySlot>> sourceBlueprints =
            new Dictionary<string, List<SupplySlot>>(StringComparer.Ordinal);
        private readonly HashSet<string> committedWork = new HashSet<string>(StringComparer.Ordinal);
        private string requirementAction = string.Empty;
        private string requirementTargetId = string.Empty;

        public PanicModel Model;
        public List<SupplySlot> Inventory;
        public List<SupplySlot> Bag;
        public List<LootSource> Sources;
        public PanicWorkSession Work;
        public int JuryWaterGatheredCount { get; private set; }
        public string Message;
        readonly List<PanicMaterialCost> lastGatherRewards = new List<PanicMaterialCost>();
        public IReadOnlyList<PanicMaterialCost> LastGatherRewards { get { return lastGatherRewards.AsReadOnly(); } }
        public int GatherSequence { get; private set; }

        public string RequirementAction { get { return requirementAction; } }
        public string RequirementTargetId { get { return requirementTargetId; } }
        public int RequirementBudgetCost { get { return WorkBudgetCostV414(requirementAction, requirementTargetId); } }
        public int RequirementBudgetOwned { get { return Model == null ? 0 : Mathf.Max(0, Model.Budget); } }
        public int RequirementBudgetMissing
        {
            get { return Model != null && Model.InfiniteBudget ? 0 : Mathf.Max(0, RequirementBudgetCost - RequirementBudgetOwned); }
        }
        public IReadOnlyList<PanicMaterialRequirement> CurrentRequirements
        {
            get { return RequirementsFor(WorkMaterialCostsV414(requirementAction, requirementTargetId), false); }
        }
        public IReadOnlyList<PanicMaterialRequirement> MissingRequirements
        {
            get { return RequirementsFor(WorkMaterialCostsV414(requirementAction, requirementTargetId), true); }
        }
        public bool HasMissingRequirements
        {
            get
            {
                return CanPrepare() && !string.IsNullOrEmpty(requirementAction) &&
                    (RequirementBudgetMissing > 0 || MissingRequirements.Count > 0);
            }
        }

        public PanicExpedition(PanicModel model)
        {
            Model = model;
            Reset();
        }

        /// <summary>Starts a fresh run's expedition stock. Model run reset remains World-owned.</summary>
        public void Reset()
        {
            ResetRevisionStock();
            lastGatherRewards.Clear(); GatherSequence = 0;
            JuryWaterGatheredCount = 0;
            Inventory = MakeEmptySlots(InventoryCapacity);
            Bag = MakeEmptySlots(BagCapacity);
            Sources = CreateSources();
            sourceBlueprints.Clear();
            committedWork.Clear();
            CancelFloorWallUpgradeV414();
            ClearRequirements();
            for (int i = 0; i < Sources.Count; i++)
                sourceBlueprints[Sources[i].Id] = CloneSlots(Sources[i].Contents);
            Work = new PanicWorkSession();
            Message = "Dolaplardan yapı malzemesi, odalardan erzak topla.";
        }

        /// <summary>Finite prepared stock plus ordinary one-shot material containers. Water requires genuine pickup.</summary>
        public bool ResetForJuryDemo()
        {
            if (Model == null || !Model.IsJuryDemo) return false;
            Reset();
            AddOne("bracket", 1);
            AddOne("screws", 2);
            AddOne("sandbag", 2);
            AddOne("plank", 1);
            AddOne("extinguisher", 1);
            Model.WaterStock = Model.FoodStock = Model.PackedWater = 0;
            Model.PackedFood = 1;
            SetBagCount("food", 1);
            PrepareShortFlowSupplies();
            Message = "Başlangıç: 1 gıda çantada. Hazır yapı malzemeleri havuzda; dolaplarda fazlası var. Suyu son 30 saniyede al.";
            NotifyModel();
            return true;
        }

        internal void SyncJuryBagAfterDisaster()
        {
            if (Model == null || !Model.IsJuryDemo) return;
            SetBagCount("water", Model.PackedWater);
            SetBagCount("food", Model.PackedFood);
        }

        internal void SyncConsumedBagSupplies()
        {
            if (Model == null) return;
            SetBagCount("water", Model.PackedWater);
            SetBagCount("food", Model.PackedFood);
        }

        public void ResetForSimulation()
        {
            Reset();
            AddOne("bracket",1);AddOne("screws",2);AddOne("plank",2);AddOne("sandbag",2);AddOne("extinguisher",1);
            SetBagCount("water",Model.PackedWater);SetBagCount("food",Model.PackedFood);
            Message="Başlangıç çantası hazır. Sonlu iş malzemeleri havuzda; evde daha fazlasını arayabilirsin.";
            NotifyModel();
        }

        /// <summary>Refills only renewable consumables after a successfully completed wave.</summary>
        public void OnNewWave()
        {
            if (Model != null && Model.IsJuryDemo) return;
            SyncConsumedBagSupplies();
            RefillRenewableWorldSupplies();
            Message = "Yeni dalga başladı. Odalardaki su, gıda ve piller yenilendi.";
            NotifyModel();
        }

        public bool ToggleSource(string id)
        {
            if (!CanPrepare()) return Fail("Dolapları hazırlıkta açabilirsin.");
            LootSource source = FindSource(id);
            if (source == null) return Fail("Kaynak bulunamadı.");
            source.Open = !source.Open;
            lastGatherRewards.Clear();
            var previousStock = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var materialId in MaterialIds) previousStock[materialId] = Count(materialId);
            Message = source.Open ? AutoCollectMaterials(source) : "Kapandı.";
            if (source.Open) foreach (var materialId in MaterialIds)
            {
                int delta = Count(materialId) - previousStock[materialId];
                if (delta > 0) lastGatherRewards.Add(new PanicMaterialCost(materialId, delta));
            }
            if (lastGatherRewards.Count > 0) GatherSequence++;
            NotifyModel();
            return true;
        }

        /// <summary>Legacy harness path: takes the first nonempty stack through the exact-index API.</summary>
        public bool Gather(string sourceId)
        {
            LootSource source = FindSource(sourceId);
            if (source == null) return Fail("Kaynak bulunamadı.");
            if (source.Contents == null) return Fail("Boş.");

            for (int i = 0; i < source.Contents.Count; i++)
            {
                SupplySlot loot = source.Contents[i];
                if (loot == null || string.IsNullOrEmpty(loot.Id) || loot.Count <= 0) continue;
                return GatherAt(sourceId, i);
            }

            return Fail("Boş.");
        }

        /// <summary>Atomically takes the complete selected stack if the source is open and the inventory can hold it.</summary>
        public bool GatherAt(string sourceId, int contentIndex)
        {
            return Fail("Yapı malzemeleri kapak açılınca havuza eklenir. Erzakları odalardan al.");
        }

        /// <summary>Packs one selected unit, using PanicModel's existing water, food and support state.</summary>
        public bool PackSlot(int index)
        {
            if (Inventory == null || index < 0 || index >= Inventory.Count || !HasStock(Inventory[index]))
                return Fail("Bu envanter yuvası boş.");
            int destination = FindStorageSlot(Bag, Inventory[index].Id, 1);
            if (destination < 0) return Fail("Afet çantasında boş yuva yok.");
            return TransferSlots(false, index, true, destination, 1);
        }

        public bool CraftFirstAid()
        {
            return Fail("Hazır ilk yardım setini evde bul ve çantaya koy.");
        }

        /// <summary>Counts numeric construction stock or loose supplies; packed supplies live in Bag.</summary>
        public int Count(string itemId)
        {
            if (IsConstructionMaterial(itemId)) return MaterialCount(itemId);
            if (string.IsNullOrEmpty(itemId) || Inventory == null) return 0;
            int total = 0;
            for (int i = 0; i < Inventory.Count; i++)
                if (Inventory[i] != null && Inventory[i].Id == itemId) total += Mathf.Max(0, Inventory[i].Count);
            return total;
        }

        public bool Has(string id, int count) { return count <= 0 || Count(id) >= count; }

        /// <summary>Run-scoped identity for completed fixed installations; reset with the expedition.</summary>
        public bool IsWorkCommitted(string action, string targetId)
        { return !string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(targetId) && committedWork.Contains(action + "|" + targetId); }

        /// <summary>Validates a new work attempt without spending stock or budget.</summary>
        public bool CanBeginWork(string action, string targetId)
        {
            ClearRequirements();
            if (Model != null && !Model.CanBeginJuryWork(action, targetId)) return Fail("Bu aşamada işaretli önlemi tamamla.");
            if (!CanPrepare()) return Fail("Hazırlık işi yalnızca hazırlıkta yapılır.");
            if (Work != null && Work.Active) return Fail("Önce etkin işi bitir veya iptal et.");
            if (IsFloorWallRequestV414(action, targetId)) return CanBeginFloorWallUpgradeV414(targetId);
            PanicItem item;
            string resultMessage;
            if (!ValidateWorkTarget(action, targetId, out item, out resultMessage)) return false;
            if ((!Model.InfiniteBudget && Model.Budget < WorkCost(action, item)) ||
                (!Model.FreeMaterials && !HasCosts(MaterialCosts(action, item))))
                return RequestRequirements(action, targetId);
            return true;
        }

        /// <summary>Commits a completed direct-click work session exactly once, after validating all costs and targets.</summary>
        public bool TryCompleteWork(string action, string targetId)
        {
            if (Model != null && !Model.CanBeginJuryWork(action, targetId)) return Fail("Bu iş için süre doldu; malzemelerin sende kaldı.");
            if (!CanPrepare()) return Fail("Hazırlık işi yalnızca hazırlık aşamasında tamamlanabilir.");
            if (Work == null || !Work.Complete || Work.Action != action || Work.TargetId != targetId)
                return Fail("İşi tamamlamak için kurulumu bitir.");
            if (string.IsNullOrEmpty(targetId)) return Fail("İş hedefi belirtilmedi.");
            if (floorWallWorkV414 != null || IsFloorWallRequestV414(action, targetId))
                return TryCompleteFloorWallUpgradeV414(targetId);

            PanicItem item;
            string resultMessage;
            if (!ValidateWorkTarget(action, targetId, out item, out resultMessage)) return false;
            IReadOnlyList<PanicMaterialCost> costs = MaterialCosts(action, item);
            int budgetCost = WorkCost(action, item);
            if ((!Model.InfiniteBudget && Model.Budget < budgetCost) ||
                (!Model.FreeMaterials && !HasCosts(costs)))
                return RequestRequirements(action, targetId);

            // Everything above is read-only. Commit stock, budget and target state together.
            ConsumeCosts(costs);
            if (!Model.InfiniteBudget) Model.Budget = Mathf.Max(0, Model.Budget - budgetCost);

            switch (action)
            {
                case "anchorwall":
                    item.Anchored = true; item.AnchorOwned = true; item.AnchorSurface = "wall";
                    break;
                case "anchorfloor":
                    item.Anchored = true; item.AnchorOwned = true; item.AnchorSurface = "floor";
                    break;
                case "upgrade": item.Tier++; break;
                case "flood": item.FloodProtected = true; break;
                case "fire":
                    Model.FireToolCount++;
                    break;
                case "plant": Model.TreeCount++; break;
                case "drain": Model.DrainCount++; break;
                case "fence": Model.FenceCount++; break;
                case "hose": Model.FireToolCount++; break;
                case "gasoff": case "wateroff": case "poweroff": Model.UtilitySafety.Complete(action); break;
            }

            if (IsFixedGardenSite(action)) committedWork.Add(action + "|" + targetId);
            Work.Cancel();
            ClearRequirements();
            Message = resultMessage;
            NotifyModel();
            return true;
        }

        private static bool IsFixedGardenSite(string action)
        {
            return action == "plant" || action == "drain" || action == "fence" || action == "hose" || action == "fire" || PanicUtilitySafetyV414.IsAction(action);
        }

        private bool ValidateWorkTarget(string action, string targetId, out PanicItem item, out string resultMessage)
        {
            item = FindItem(targetId);
            resultMessage = string.Empty;
            if (action == "fence") return Fail("Çit inşası kaldırıldı.");
            if (PanicUtilitySafetyV414.IsAction(action))
            {
                if (!PanicUtilitySafetyV414.Allows(action, targetId) || item != null) return Fail("Bu tesisat işi bu hedefe uygulanmaz.");
                if (Model.UtilitySafety.IsComplete(action) || IsWorkCommitted(action, targetId)) return Fail("Bu tesisat zaten güvenli duruma alındı.");
                resultMessage = PanicUtilitySafetyV414.StateLabel(action, true) + " · " + PanicUtilitySafetyV414.Effect(action);
                return true;
            }
            if (!PanicItemActionRules.Allows(action, item, targetId)) return Fail("Bu hazırlık işi bu hedefe uygulanmaz.");
            if (string.IsNullOrEmpty(targetId)) return Fail("İş hedefi belirtilmedi.");
            if (IsFixedGardenSite(action) && committedWork.Contains(action + "|" + targetId))
                return Fail("Bu hazırlık işi bu turda zaten tamamlandı.");

            switch (action)
            {
                case "anchorwall":
                    if (item == null || !item.Movable || item.Kind != ItemKind.Furniture ||
                        item.Id == "bunker" || item.Id == "bagstation")
                        return Fail("Duvara sabitlemek için taşınabilir bir mobilya seç.");
                    if (item.Anchored) return Fail(item.Label + " zaten sabitlenmiş.");
                    resultMessage = item.Label + " duvar ankrajlarıyla sabitlendi.";
                    break;
                case "anchorfloor":
                    if (item == null || !item.Movable || item.Kind != ItemKind.Furniture ||
                        item.Id == "bunker" || item.Id == "bagstation")
                        return Fail("Zemin ankrajı için taşınabilir bir mobilya seç.");
                    if (item.Anchored) return Fail(item.Label + " zaten sabitlenmiş.");
                    resultMessage = item.Label + " zemine ankrajlarla sabitlendi.";
                    break;
                case "upgrade":
                    if (item == null || !IsStructure(item)) return Fail("Yükseltilecek yapı parçası bulunamadı.");
                    if (!item.Installed) return Fail("Önce bu pencereyi tak.");
                    if (item.Tier >= 2) return Fail(item.Label + " zaten en yüksek kademede.");
                    resultMessage = item.Label + " malzemelerle T" + (item.Tier + 1) + " seviyesine yükseltildi.";
                    break;
                case "flood":
                    if (item == null || (item.Kind != ItemKind.Window && item.Kind != ItemKind.Door))
                        return Fail("Sel bariyeri için bir pencere veya kapı seç.");
                    if (!item.Installed) return Fail("Önce pencereyi tak.");
                    if (item.FloodProtected) return Fail(item.Label + " zaten sel korumalı.");
                    resultMessage = item.Label + " için kum torbalı sel bariyeri kuruldu.";
                    break;
                case "fire":
                    if (Model.FireToolCount >= 3) return Fail("Ev için kurulabilir yangın ekipmanı sınırına ulaşıldı.");
                    if (item != null && item.FireProtected) return Fail(item.Label + " zaten yangın ekipmanıyla korumalı.");
                    if (item != null && (!item.Installed || item.Id == "bunker" || item.Id == "bagstation"))
                        return Fail("Yangın ekipmanı bu noktaya kurulamaz.");
                    resultMessage = "Yangın söndürücü hazırlandı ve müdahale ekipmanına eklendi.";
                    break;
                case "plant":
                    if (Model.TreeCount >= 3) return Fail("Üç ağaç bölgesi de tamamlandı.");
                    resultMessage = "Seçili bahçe bölgesine ağaç sırası kuruldu.";
                    break;
                case "drain":
                    if (Model.DrainCount >= 4) return Fail("Bahçede dört drenaj hattı yeterli.");
                    resultMessage = "Bahçeye suyu evden uzağa taşıyan drenaj hattı açıldı.";
                    break;
                case "fence":
                    if (Model.FenceCount >= 4) return Fail("Bahçe çevresindeki sel çiti tamamlandı.");
                    resultMessage = "Bahçeye alçak sel çiti kuruldu.";
                    break;
                case "hose":
                    if (Model.FireToolCount >= 3) return Fail("Ev için kurulabilir yangın ekipmanı sınırına ulaşıldı.");
                    resultMessage = "Bahçe hortumu yangın müdahale araçlarına eklendi.";
                    break;
                default:
                    return Fail("Bilinmeyen hazırlık işi: " + action + ".");
            }

            return true;
        }

        public static string DisplayName(string id)
        {
            switch (id)
            {
                case "water": return "Su";
                case "food": return "Gıda";
                case "firstaid": return "İlk yardım seti";
                case "flashlight": return "El feneri";
                case "battery": return "Pil";
                case "radio": return "Pilli radyo";
                case "blanket": return "Battaniye";
                case "screws": return "Vida";
                case "bracket": return "Ankraj braketi";
                case "plank": return "Tahta";
                case "sandbag": return "Kum torbası";
                case "seedling": return "Ağaç sırası paketi";
                case "hose": return "Bahçe hortumu";
                case "extinguisher": return "Yangın söndürücü";
                default: return string.IsNullOrEmpty(id) ? "Boş" : id;
            }
        }

        /// <summary>Recipes shared by preflight, completion, shortage payloads and crafting.</summary>
        public static IReadOnlyList<PanicMaterialCost> MaterialCosts(string action, PanicItem item)
        {
            switch (action)
            {
                case "anchorwall":
                case "anchorfloor":
                    return item != null && item.AnchorOwned ? new PanicMaterialCost[0] :
                        new[] { new PanicMaterialCost("bracket", 1), new PanicMaterialCost("screws", 2) };
                case "upgrade":
                    return new[] { new PanicMaterialCost("plank", item != null && item.Tier > 0 ? 2 : 1),
                        new PanicMaterialCost("screws", item != null && item.Tier > 0 ? 3 : 2) };
                case "flood":
                case "drain": return new[] { new PanicMaterialCost("sandbag", 2), new PanicMaterialCost("plank", 1) };
                case "fence": return new[] { new PanicMaterialCost("plank", 3), new PanicMaterialCost("screws", 2), new PanicMaterialCost("sandbag", 1) };
                case "fire": return new[] { new PanicMaterialCost("extinguisher", 1) };
                case "plant": return new[] { new PanicMaterialCost("seedling", 1) };
                case "hose": return new[] { new PanicMaterialCost("hose", 1) };
                case "packbattery": return new[] { new PanicMaterialCost("battery", 1) };
                default: return new PanicMaterialCost[0];
            }
        }

        public static int WorkCost(string action, PanicItem item)
        {
            switch (action)
            {
                case "anchorwall":
                case "anchorfloor": return item != null && item.AnchorOwned ? 0 : 60;
                case "upgrade": return item == null ? 0 : item.Tier == 0 ? 120 : item.Tier == 1 ? 180 : 0;
                case "flood": return 80;
                case "fire": return 75;
                case "plant": return 30;
                case "drain": return 50;
                case "fence": return 60;
                case "hose": return 80;
                default: return 0;
            }
        }

        private static List<LootSource> CreateSources()
        {
            return new List<LootSource> {
                Source("fridge", "Buzdolabı", "fridge", new Vector3(6.5f,0,4.6f),0, Stack("screws",2)),
                Source("cabinet", "Mutfak dolabı", "cabinet", new Vector3(4.5f,0,4.7f),0, Stack("screws",3),Stack("bracket",2)),
                Source("bookshelf", "Salon kitaplığı", "bookshelf", new Vector3(-6,0,4.8f),0,Stack("bracket",1),Stack("screws",2)),
                Source("up_cabinet", "Üst kat dolabı", "cabinet", new Vector3(4.6f,3.2f,-4.5f),1,Stack("plank",2),Stack("screws",2)),
                Source("up_bookshelf", "Üst kat kitaplığı", "bookshelf",new Vector3(-6,3.2f,-3.8f),1,Stack("bracket",1),Stack("screws",2)),
                Source("bath_cabinet", "Banyo dolabı", "medicinecabinet",new Vector3(PanicHouseLayoutV414.UpperEastPartition+.24f,3.2f,2.5f),1,Stack("screws",2)),
                Source("garage_tools", "Garaj alet kutusu", "toolbox",new Vector3(14,0,5),0,Stack("screws",4),Stack("bracket",3),Stack("extinguisher",1)),
                Source("basement_pantry", "Bodrum deposu", "supplycrate",new Vector3(-5,-3.2f,4),-1,Stack("sandbag",4),Stack("plank",4),Stack("seedling",3),Stack("hose",1),Stack("extinguisher",1))
            };
        }

        private static LootSource Source(string id, string label, string artId, Vector3 position, int floor, params SupplySlot[] items)
        {
            return new LootSource
            {
                Id = id, Label = label, ArtId = artId, Position = position,
                Floor = floor, Open = false, Contents = new List<SupplySlot>(items)
            };
        }

        private static SupplySlot Stack(string id, int count) { return new SupplySlot(id, count); }
        private static List<SupplySlot> MakeEmptySlots(int count)
        {
            List<SupplySlot> slots = new List<SupplySlot>(count);
            for (int i = 0; i < count; i++) slots.Add(new SupplySlot());
            return slots;
        }

        private static List<SupplySlot> CloneSlots(List<SupplySlot> source)
        {
            List<SupplySlot> copy = new List<SupplySlot>(source.Count);
            for (int i = 0; i < source.Count; i++) copy.Add(source[i] == null ? new SupplySlot() : new SupplySlot(source[i].Id, source[i].Count));
            return copy;
        }

        private static bool IsRenewable(string id)
        {
            return id == "water" || id == "food" || id == "battery";
        }

        private static int StackLimitFor(string id)
        {
            return id == "flashlight" || id == "radio" || id == "blanket" || id == "firstaid" ||
                id == "hose" || id == "extinguisher" ? 1 : StackLimit;
        }

        private static int FindStorageSlot(List<SupplySlot> slots, string id, int count)
        {
            int limit = StackLimitFor(id);
            if (count <= 0 || count > limit) return -1;
            for (int i = 0; i < slots.Count; i++)
            {
                SupplySlot slot = slots[i];
                if (slot != null && slot.Id == id && slot.Count + count <= limit) return i;
            }
            for (int i = 0; i < slots.Count; i++)
                if (slots[i] == null || slots[i].Count <= 0 || string.IsNullOrEmpty(slots[i].Id)) return i;
            return -1;
        }

        private static void AddToSlot(SupplySlot slot, string id, int count)
        {
            if (slot.Id != id) { slot.Id = id; slot.Count = 0; }
            slot.Count += count;
        }

        private static SupplySlot FindSlot(List<SupplySlot> slots, string id)
        {
            for (int i = 0; i < slots.Count; i++)
                if (slots[i] != null && slots[i].Id == id) return slots[i];
            return null;
        }

        private LootSource FindSource(string id)
        {
            if (Sources == null || string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Sources.Count; i++)
                if (Sources[i] != null && Sources[i].Id == id) return Sources[i];
            return null;
        }

        private PanicItem FindItem(string id)
        {
            if (Model == null || Model.Items == null || string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Model.Items.Count; i++)
                if (Model.Items[i] != null && Model.Items[i].Id == id) return Model.Items[i];
            return null;
        }

        private int FindInventorySlot(string id, int amount)
        {
            for (int i = 0; i < Inventory.Count; i++)
                if (Inventory[i] != null && Inventory[i].Id == id && Inventory[i].Count >= amount) return i;
            return -1;
        }

        private static int SupportIndexFor(string id)
        {
            switch (id)
            {
                case "firstaid": return 0;
                case "flashlight": return 1;
                case "radio": return 2;
                case "blanket": return 3;
                default: return -1;
            }
        }

        private void ConsumeAt(int slotIndex, int count)
        {
            SupplySlot slot = Inventory[slotIndex];
            slot.Count -= count;
            if (slot.Count <= 0) { slot.Count = 0; slot.Id = string.Empty; }
        }

        private void Consume(string id, int count)
        {
            if (IsConstructionMaterial(id)) { materials[id] = Mathf.Max(0, MaterialCount(id) - count); return; }
            for (int i = 0; i < Inventory.Count && count > 0; i++)
            {
                SupplySlot slot = Inventory[i];
                if (slot == null || slot.Id != id || slot.Count <= 0) continue;
                int taken = Mathf.Min(count, slot.Count);
                ConsumeAt(i, taken);
                count -= taken;
            }
        }

        private void AddOne(string id, int count)
        {
            if (IsConstructionMaterial(id)) { AddMaterial(id, count); return; }
            TryAddPartial(Inventory, id, count);
        }

        private void SetBagCount(string id, int count)
        {
            for (int i = 0; i < Bag.Count; i++)
                if (Bag[i] != null && Bag[i].Id == id) { Bag[i].Id = string.Empty; Bag[i].Count = 0; }
            if (count > 0 && !TryAddPartial(Bag, id, count))
                throw new InvalidOperationException("Bag/model capacity invariant violated.");
        }

        private bool CanStoreAfterConsumption(string id, int count, IReadOnlyList<PanicMaterialCost> costs)
        {
            List<SupplySlot> simulated = CloneSlots(Inventory);
            for (int i = 0; i < costs.Count; i++) ConsumeFromSlots(simulated, costs[i].Id, costs[i].Required);
            return FindStorageSlot(simulated, id, count) >= 0;
        }

        private static void ConsumeFromSlots(List<SupplySlot> slots, string id, int count)
        {
            for (int i = 0; i < slots.Count && count > 0; i++)
            {
                SupplySlot slot = slots[i];
                if (slot == null || slot.Id != id || slot.Count <= 0) continue;
                int taken = Mathf.Min(count, slot.Count);
                slot.Count -= taken;
                count -= taken;
                if (slot.Count == 0) slot.Id = string.Empty;
            }
        }

        private bool HasCosts(IReadOnlyList<PanicMaterialCost> costs)
        {
            for (int i = 0; i < costs.Count; i++)
                if (!Has(costs[i].Id, costs[i].Required)) return false;
            return true;
        }

        private void ConsumeCosts(IReadOnlyList<PanicMaterialCost> costs)
        {
            if (Model != null && Model.FreeMaterials) return;
            for (int i = 0; i < costs.Count; i++) Consume(costs[i].Id, costs[i].Required);
        }

        private IReadOnlyList<PanicMaterialRequirement> RequirementsFor(IReadOnlyList<PanicMaterialCost> costs, bool onlyMissing)
        {
            List<PanicMaterialRequirement> requirements = new List<PanicMaterialRequirement>(costs.Count);
            for (int i = 0; i < costs.Count; i++)
            {
                int owned = Count(costs[i].Id);
                PanicMaterialRequirement requirement = Model != null && Model.FreeMaterials
                    ? new PanicMaterialRequirement(costs[i].Id, costs[i].Required, owned, 0)
                    : new PanicMaterialRequirement(costs[i].Id, costs[i].Required, owned);
                if (!onlyMissing || requirement.Missing > 0) requirements.Add(requirement);
            }
            return requirements.AsReadOnly();
        }

        private bool RequestRequirements(string action, string targetId)
        {
            requirementAction = action;
            requirementTargetId = targetId;
            List<string> missing = new List<string>();
            IReadOnlyList<PanicMaterialRequirement> requirements = MissingRequirements;
            for (int i = 0; i < requirements.Count; i++)
                missing.Add(DisplayName(requirements[i].Id) + ": eksik " + requirements[i].Missing);
            if (RequirementBudgetMissing > 0) missing.Add("Bütçe: eksik $ " + RequirementBudgetMissing);
            return Fail(string.Join(" · ", missing.ToArray()));
        }

        /// <summary>Clears only shortage presentation when the world's preparation stage changes.</summary>
        public void ClearRequirements()
        {
            requirementAction = string.Empty;
            requirementTargetId = string.Empty;
        }

        private bool CanPrepare()
        {
            return Model != null && !Model.IsPaused && (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial);
        }

        private static bool IsStructure(PanicItem item)
        {
            return item != null && (item.Kind == ItemKind.Wall || item.Kind == ItemKind.Door ||
                item.Kind == ItemKind.Roof || item.Kind == ItemKind.Window);
        }

        private bool Fail(string text)
        {
            Message = text ?? string.Empty;
            NotifyModel();
            return false;
        }

        private void NotifyModel()
        {
            if (!string.IsNullOrEmpty(requirementAction) && RequirementBudgetMissing == 0 && MissingRequirements.Count == 0)
                ClearRequirements();
            if (Model != null) Model.Notify(Message);
        }
    }

    public enum PanicWorkMode { Ratchet, Timing, Balance }

    /// <summary>Manual input work; elapsed time alone never installs anything. The final transaction owns spending.</summary>
    public sealed class PanicWorkSession
    {
        const int HammerStrokes = 4;
        bool juryFriendly, installing;
        int completedStrokes;
        float stageProgress, toolMotion;
        public bool Active;
        public string Action = string.Empty, TargetId = string.Empty, Label = string.Empty, Hint = string.Empty;
        public int CompletedSteps, TotalSteps;
        public float Cursor, Target, Progress, Quality;
        public bool Complete;
        public PanicWorkMode Mode { get; private set; }
        public bool JuryFriendly { get { return juryFriendly; } }
        public bool UsesDirectClick { get { return UsesRotation || UsesLinearDrag; } }
        public bool UsesLinearDrag { get { return Action == "poweroff"; } }
        public bool UsesRotation { get { return Action == "anchorwall" || Action == "anchorfloor" || Action == "gasoff" || Action == "wateroff" || Action == "hose"; } }
        public bool UsesHammer { get { return !UsesDirectClick; } }
        public bool IsUtility { get { return PanicUtilitySafetyV414.IsAction(Action); } }
        public bool IsWorldConstruction { get { return UsesHammer; } }
        public bool IsInstalling { get { return installing; } }
        public int NextClickTarget { get { return Active && !Complete ? CompletedSteps : -1; } }
        public float InstallSeconds { get { return 0f; } }
        public bool IsApplying { get { return installing && toolMotion > .001f; } }
        public int RequiredStrokes { get { return UsesHammer ? HammerStrokes : 0; } }
        public int CompletedStrokes { get { return completedStrokes; } }
        // Positive is clockwise in the displayed work-camera image.
        public int ExpectedDirection { get { return Action == "poweroff" ? -1 : 1; } }
        public float RequiredDegrees { get { return IsUtility ? 90f : Action == "hose" ? 180f : 270f; } }
        public string StepHint { get { return Hint; } }
        public float StageProgress { get { return stageProgress; } }
        public float TargetWidth { get { return 0f; } }
        public float ToolMotion { get { return toolMotion; } }
        public string StageName { get { return !Active ? string.Empty : Complete ? "Tamamlandı" : IsUtility || Action == "hose" ? Label : UsesHammer ? "Montaj" : "Vida " + (CompletedSteps + 1); } }

        public void Begin(string action, string targetId) { Begin(action, targetId, false); }
        public void Begin(string action, string targetId, bool juryDemo)
        {
            Cancel();
            if (!IsSupported(action) || string.IsNullOrEmpty(targetId)) return;
            Action = action; TargetId = targetId; juryFriendly = juryDemo;
            Label = LabelFor(action); Mode = UsesHammer ? PanicWorkMode.Timing : PanicWorkMode.Ratchet;
            TotalSteps = action == "anchorwall" || action == "anchorfloor" ? 2 : 1;
            Quality = 1f; Active = true;
            installing = UsesHammer;
            RefreshHint();
        }

        /// <summary>Select the current real screw/handle. Selection itself contributes no progress.</summary>
        public bool TryClick(int targetIndex)
        {
            if (!Active || Complete || targetIndex != CompletedSteps || targetIndex < 0 || targetIndex >= TotalSteps) return false;
            installing = true; RefreshHint(); return true;
        }

        /// <summary>Signed angular displacement from the held mouse in the actual work-camera image.</summary>
        public bool ApplyRotation(float clockwiseDegrees, bool leftMouseHeld)
        {
            if (!Active || Complete || !UsesRotation || !installing || !leftMouseHeld ||
                !Finite(clockwiseDegrees) || Mathf.Abs(clockwiseDegrees) < .00001f) return false;
            float signed = clockwiseDegrees * ExpectedDirection;
            if (signed <= 0f) return false; // Repositioning never punishes completed work.
            stageProgress = Mathf.Clamp01(stageProgress + signed / RequiredDegrees);
            Cursor = stageProgress; toolMotion = 1f;
            UpdateProgress();
            if (stageProgress >= .99999f) FinishStep();
            RefreshHint(); return true;
        }
        /// <summary>Signed fraction of the real breaker track. Positive is downward/off; release, reverse and idle do not advance.</summary>
        public bool ApplyLinearDisplacement(float downwardTrackFraction, bool held)
        {
            if (!Active || Complete || !UsesLinearDrag || !installing || !held || !Finite(downwardTrackFraction) || downwardTrackFraction <= 0f) return false;
            stageProgress = Mathf.Clamp01(stageProgress + downwardTrackFraction);
            Cursor = stageProgress; toolMotion = 1f; UpdateProgress();
            if (stageProgress >= .99999f) FinishStep();
            RefreshHint(); return true;
        }
        public bool PressHammer()
        {
            if (!Active || Complete || !UsesHammer) return false;
            installing = true; completedStrokes++;
            stageProgress = Mathf.Clamp01(completedStrokes / (float)HammerStrokes);
            toolMotion = 1f; UpdateProgress();
            if (completedStrokes >= HammerStrokes) FinishStep();
            RefreshHint(); return true;
        }
        public void Tick(float dt, float direction, bool held, bool pressed) { Tick(dt, direction, held, pressed, false); }
        public void Tick(float dt, float direction, bool held, bool pressed, bool pulse)
        {
            if (!Active || Complete || !Finite(dt) || dt <= 0f) return;
            // Legacy direction/held/Enter adapters cannot synthesize mouse rotation.
            toolMotion = Mathf.Max(0f, toolMotion - Mathf.Min(dt, .1f) * 8f);
            if (UsesHammer && pulse) PressHammer();
            RefreshHint();
        }
        void UpdateProgress() { Progress = Mathf.Clamp01((CompletedSteps + stageProgress) / Mathf.Max(1, TotalSteps)); }
        void FinishStep()
        {
            CompletedSteps++; installing = false; stageProgress = 0f;
            Complete = CompletedSteps >= TotalSteps;
            Progress = Complete ? 1f : CompletedSteps / (float)TotalSteps;
        }
        public void Cancel()
        {
            juryFriendly = installing = Active = Complete = false;
            Action = TargetId = Label = Hint = string.Empty;
            completedStrokes = CompletedSteps = TotalSteps = 0;
            stageProgress = toolMotion = Cursor = Target = Progress = Quality = 0f;
            Mode = PanicWorkMode.Ratchet;
        }
        void RefreshHint()
        {
            Hint = Complete ? "Hazır!" : UsesHammer ? "Bağlantıya tıkla veya SPACE · " + completedStrokes + " / " + HammerStrokes :
                Action == "poweroff" ? "Kırmızı kolu tutup aşağı indir. Alternatif: ↓." :
                IsUtility ? "Vana kolunu tutup 90° saat yönünde çevir. Alternatif: →." :
                "İşaretli vida/bağlantıyı tutup saat yönünde çevir. Bırakınca durur. Alternatif: →.";
        }
        static bool Finite(float v) { return !float.IsNaN(v) && !float.IsInfinity(v); }
        static bool IsSupported(string action)
        {
            return action == "anchorwall" || action == "anchorfloor" || action == "upgrade" || action == "flood" ||
                action == "fire" || action == "plant" || action == "drain" || action == "hose" || PanicUtilitySafetyV414.IsAction(action);
        }
        static string LabelFor(string action)
        {
            if (PanicUtilitySafetyV414.IsAction(action)) return PanicUtilitySafetyV414.Label(action);
            switch (action)
            {
                case "anchorwall": return "Duvara sabitle";
                case "anchorfloor": return "Zemine sabitle";
                case "upgrade": return "Yapıyı güçlendir";
                case "flood": return "Sel bariyerini kur";
                case "fire": return "Yangın tüpünü hazırla";
                case "plant": return "Ağaç sırasını kur";
                case "drain": return "Drenajı kur";
                case "hose": return "Hortumu bağla";
                default: return "Hazırlık işi";
            }
        }
    }
}
