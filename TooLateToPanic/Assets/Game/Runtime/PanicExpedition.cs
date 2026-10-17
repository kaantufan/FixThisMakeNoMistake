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
        public List<SupplySlot> Contents;
    }

    /// <summary>One canonical material cost. Inventory stock is never stored in a recipe.</summary>
    public readonly struct PanicMaterialCost
    {
        public readonly string Id;
        public readonly int Required;
        public PanicMaterialCost(string id, int required) { Id = id; Required = Mathf.Max(0, required); }
    }

    /// <summary>A read-only snapshot of a recipe against the current loose inventory.</summary>
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
    public sealed class PanicExpedition
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

        public string RequirementAction { get { return requirementAction; } }
        public string RequirementTargetId { get { return requirementTargetId; } }
        public int RequirementBudgetCost { get { return WorkCost(requirementAction, FindItem(requirementTargetId)); } }
        public int RequirementBudgetOwned { get { return Model == null ? 0 : Mathf.Max(0, Model.Budget); } }
        public int RequirementBudgetMissing
        {
            get { return Model != null && Model.InfiniteBudget ? 0 : Mathf.Max(0, RequirementBudgetCost - RequirementBudgetOwned); }
        }
        public IReadOnlyList<PanicMaterialRequirement> CurrentRequirements
        {
            get { return RequirementsFor(MaterialCosts(requirementAction, FindItem(requirementTargetId)), false); }
        }
        public IReadOnlyList<PanicMaterialRequirement> MissingRequirements
        {
            get { return RequirementsFor(MaterialCosts(requirementAction, FindItem(requirementTargetId)), true); }
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
            JuryWaterGatheredCount = 0;
            Inventory = MakeEmptySlots(InventoryCapacity);
            Bag = MakeEmptySlots(BagCapacity);
            Sources = CreateSources();
            sourceBlueprints.Clear();
            committedWork.Clear();
            ClearRequirements();
            for (int i = 0; i < Sources.Count; i++)
                sourceBlueprints[Sources[i].Id] = CloneSlots(Sources[i].Contents);
            Work = new PanicWorkSession();
            Message = "Evdeki dolapları ve depoyu araştır.";
        }

        /// <summary>Finite demo fixture, labelled as prepared stock. Water still requires genuine pickup and packing.</summary>
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
            sourceBlueprints.Clear();
            for (int i = 0; i < Sources.Count; i++)
            {
                Sources[i].Contents.Clear();
                Sources[i].Open = false;
                if (Sources[i].Id == "fridge") Sources[i].Contents.Add(Stack("water", 1));
                sourceBlueprints[Sources[i].Id] = CloneSlots(Sources[i].Contents);
            }
            Message = "Başlangıç: 1 gıda çantada; 1 braket, 2 vida, 2 kum torbası, 1 tahta ve 1 tüp envanterde. Su buzdolabında.";
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
            Message="Başlangıç çantası hazır. Sonlu iş malzemeleri envanterde; evde daha fazlasını arayabilirsin.";
            NotifyModel();
        }

        /// <summary>Refills only renewable consumables after a successfully completed wave.</summary>
        public void OnNewWave()
        {
            if (Model != null && Model.IsJuryDemo) return;
            if (Model != null)
            {
                SetBagCount("water", Model.PackedWater);
                SetBagCount("food", Model.PackedFood);
            }
            for (int i = 0; i < Sources.Count; i++)
            {
                LootSource source = Sources[i];
                List<SupplySlot> baseline;
                if (!sourceBlueprints.TryGetValue(source.Id, out baseline)) continue;
                for (int j = 0; j < baseline.Count; j++)
                {
                    SupplySlot starting = baseline[j];
                    if (!IsRenewable(starting.Id)) continue;
                    SupplySlot current = FindSlot(source.Contents, starting.Id);
                    if (current == null)
                    {
                        source.Contents.Add(new SupplySlot(starting.Id, starting.Count));
                    }
                    else
                    {
                        current.Count = starting.Count;
                    }
                }
            }
            Message = "Yeni dalga başladı. Evdeki gıda, su ve sarf malzemeleri yenilendi.";
            NotifyModel();
        }

        public bool ToggleSource(string id)
        {
            LootSource source = FindSource(id);
            if (source == null) return Fail("Kaynak bulunamadı.");
            source.Open = !source.Open;
            Message = source.Open ? "Açıldı." : "Kapandı.";
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
            if (!CanPrepare()) return Fail("Hazırlıkta topla.");
            LootSource source = FindSource(sourceId);
            if (source == null) return Fail("Kaynak bulunamadı.");
            if (!source.Open) return Fail("Önce E ile aç.");
            if (source.Contents == null || contentIndex < 0 || contentIndex >= source.Contents.Count)
                return Fail("Malzeme seçimi geçersiz.");

            SupplySlot loot = source.Contents[contentIndex];
            if (loot == null || string.IsNullOrEmpty(loot.Id) || loot.Count <= 0) return Fail("Bu yuva boş.");
            int destination = FindStorageSlot(Inventory, loot.Id, loot.Count);
            if (destination < 0) return Fail("Envanter dolu.");

            string gathered = loot.Id;
            int gatheredCount = loot.Count;
            AddToSlot(Inventory[destination], gathered, gatheredCount);
            if (Model.IsJuryDemo && gathered == "water") JuryWaterGatheredCount += gatheredCount;
            loot.Count = 0;
            loot.Id = string.Empty;
            Message = DisplayName(gathered) + " ×" + gatheredCount + " alındı.";
            NotifyModel();
            return true;
        }

        /// <summary>Packs one selected unit, using PanicModel's existing water, food and support state.</summary>
        public bool PackSlot(int index)
        {
            if (!CanPrepare()) return Fail("Afet çantası yalnızca hazırlıkta düzenlenebilir.");
            if (index < 0 || index >= Inventory.Count) return Fail("Envanter yuvası geçersiz.");
            SupplySlot source = Inventory[index];
            if (source == null || string.IsNullOrEmpty(source.Id) || source.Count <= 0)
                return Fail("Bu envanter yuvası boş.");

            string id = source.Id;
            int supportIndex = SupportIndexFor(id);
            int batterySlot = -1;
            if (id == "flashlight" || id == "radio")
            {
                batterySlot = FindInventorySlot("battery", 1);
                if (batterySlot < 0 && (Model == null || !Model.FreeMaterials))
                    return RequestRequirements("packbattery", id);
            }
            if (id == "battery" || id == "screws" || id == "bracket" || id == "plank" ||
                id == "sandbag" || id == "seedling" || id == "hose" || id == "extinguisher")
                return Fail(DisplayName(id) + " hazırlık malzemesi; bunu çantaya koymak yerine kullan.");
            if (id != "water" && id != "food" && supportIndex < 0)
                return Fail(DisplayName(id) + " bu çanta düzeninde taşınamıyor.");

            int bagSlot = FindStorageSlot(Bag, id, 1);
            if (bagSlot < 0) return Fail("Afet çantasında boş yuva yok.");
            if (supportIndex >= 0)
            {
                int bit = 1 << supportIndex;
                if ((Model.PackedSupportMask & bit) != 0)
                    return Fail(DisplayName(id) + " zaten afet çantasında.");
            }

            if (id == "water")
            {
                Model.WaterStock++;
                if (!Model.PackWater())
                {
                    Model.WaterStock = Mathf.Max(0, Model.WaterStock - 1);
                    Message = Model.Message;
                    return false;
                }
            }
            else if (id == "food")
            {
                Model.FoodStock++;
                if (!Model.PackFood())
                {
                    Model.FoodStock = Mathf.Max(0, Model.FoodStock - 1);
                    Message = Model.Message;
                    return false;
                }
            }
            else
            {
                int bit = 1 << supportIndex;
                int previousOwned = Model.SupportOwnedMask;
                Model.SupportOwnedMask |= bit;
                if (!Model.PackSupport(supportIndex))
                {
                    Model.SupportOwnedMask = previousOwned;
                    Message = Model.Message;
                    return false;
                }
            }

            source.Count--;
            if (source.Count == 0) source.Id = string.Empty;
            AddToSlot(Bag[bagSlot], id, 1);
            if (batterySlot >= 0 && (Model == null || !Model.FreeMaterials)) ConsumeAt(batterySlot, 1);
            if (requirementAction == "packbattery" && requirementTargetId == id) ClearRequirements();
            Message = DisplayName(id) + " afet çantasına yerleştirildi.";
            NotifyModel();
            return true;
        }

        public bool CraftFirstAid()
        {
            if (!CanPrepare()) return Fail("İlk yardım seti yalnızca hazırlıkta üretilebilir.");
            IReadOnlyList<PanicMaterialCost> costs = MaterialCosts("craftfirstaid", null);
            bool freeMaterials = Model != null && Model.FreeMaterials;
            if (!freeMaterials && !HasCosts(costs)) return RequestRequirements("craftfirstaid", "firstaid");
            if (!CanStoreAfterConsumption("firstaid", 1, freeMaterials ? new PanicMaterialCost[0] : costs))
                return Fail("İlk yardım seti için envanterde boş bir yuva aç.");

            ConsumeCosts(costs);
            AddOne("firstaid", 1);
            if (requirementAction == "craftfirstaid") ClearRequirements();
            Message = "İlk yardım seti üretildi. Afet çantasına koyabilirsin.";
            NotifyModel();
            return true;
        }

        /// <summary>Counts loose inventory only; packed items are tracked by Bag and PanicModel.</summary>
        public int Count(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Inventory == null) return 0;
            int total = 0;
            for (int i = 0; i < Inventory.Count; i++)
                if (Inventory[i] != null && Inventory[i].Id == itemId) total += Mathf.Max(0, Inventory[i].Count);
            return total;
        }

        public bool Has(string id, int count) { return count <= 0 || Count(id) >= count; }

        /// <summary>Validates a new work attempt without spending stock or budget.</summary>
        public bool CanBeginWork(string action, string targetId)
        {
            if (Model != null && !Model.CanBeginJuryWork(action, targetId)) return Fail("İşaretli hedefe yönel.");
            ClearRequirements();
            if (!CanPrepare()) return Fail("Hazırlık işi yalnızca hazırlıkta yapılır.");
            if (Work != null && Work.Active) return Fail("Önce etkin işi bitir veya iptal et.");
            PanicItem item;
            string resultMessage;
            if (!ValidateWorkTarget(action, targetId, out item, out resultMessage)) return false;
            if ((!Model.InfiniteBudget && Model.Budget < WorkCost(action, item)) ||
                (!Model.FreeMaterials && !HasCosts(MaterialCosts(action, item))))
                return RequestRequirements(action, targetId);
            return true;
        }

        /// <summary>Commits a completed keyboard work session exactly once, after validating all costs and targets.</summary>
        public bool TryCompleteWork(string action, string targetId)
        {
            if (Model != null && !Model.CanBeginJuryWork(action, targetId)) return Fail("Bu iş için süre doldu; malzemelerin sende kaldı.");
            if (!CanPrepare()) return Fail("Hazırlık işi yalnızca hazırlık aşamasında tamamlanabilir.");
            if (Work == null || !Work.Complete || Work.Action != action || Work.TargetId != targetId)
                return Fail("İşi tamamlamak için etkin klavye görevini bitir.");
            if (string.IsNullOrEmpty(targetId)) return Fail("İş hedefi belirtilmedi.");

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
                    if (item != null) item.FireProtected = true;
                    break;
                case "plant": Model.TreeCount++; break;
                case "drain": Model.DrainCount++; break;
                case "fence": Model.FenceCount++; break;
                case "hose": Model.FireToolCount++; break;
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
            return action == "plant" || action == "drain" || action == "fence" || action == "hose";
        }

        private bool ValidateWorkTarget(string action, string targetId, out PanicItem item, out string resultMessage)
        {
            item = FindItem(targetId);
            resultMessage = string.Empty;
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
                    if (Model.TreeCount >= 5) return Fail("Bahçe için beş fidanlık sınırına ulaşıldı.");
                    resultMessage = "Bahçeye fidan dikildi; toprak suyu daha iyi emiyor.";
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
                case "bandage": return "Bandaj";
                case "antiseptic": return "Antiseptik";
                case "firstaid": return "İlk yardım seti";
                case "flashlight": return "El feneri";
                case "battery": return "Pil";
                case "radio": return "Pilli radyo";
                case "blanket": return "Battaniye";
                case "screws": return "Vida";
                case "bracket": return "Ankraj braketi";
                case "plank": return "Tahta";
                case "sandbag": return "Kum torbası";
                case "seedling": return "Fidan";
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
                case "craftfirstaid": return new[] { new PanicMaterialCost("bandage", 2), new PanicMaterialCost("antiseptic", 1) };
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
            List<LootSource> sources = new List<LootSource>(9);
            // Core sources reuse item IDs, allowing World to attach prompts to actual furniture.
            sources.Add(Source("fridge", "Buzdolabı", "fridge", new Vector3(6.5f, 0f, 4.6f), 0,
                Stack("water", 2), Stack("food", 2)));
            sources.Add(Source("cabinet", "Mutfak dolabı", "cabinet", new Vector3(4.5f, 0f, 4.7f), 0,
                Stack("food", 1), Stack("water", 1), Stack("bandage", 1)));
            sources.Add(Source("bookshelf", "Salon kitaplığı", "bookshelf", new Vector3(-6f, 0f, 4.8f), 0,
                Stack("flashlight", 1), Stack("battery", 2), Stack("blanket", 1)));
            sources.Add(Source("up_cabinet", "Üst kat dolabı", "cabinet", new Vector3(4.6f, 3.2f, -4.5f), 1,
                Stack("food", 1), Stack("water", 1), Stack("bandage", 1), Stack("antiseptic", 1)));
            sources.Add(Source("up_bookshelf", "Üst kat kitaplığı", "bookshelf", new Vector3(-6f, 3.2f, -3.8f), 1,
                Stack("radio", 1), Stack("battery", 1), Stack("blanket", 1), Stack("screws", 2)));
            sources.Add(Source("bath_cabinet", "Banyo dolabı", "medicinecabinet", new Vector3(2.24f, 3.2f, 4.15f), 1,
                Stack("bandage", 3), Stack("antiseptic", 2), Stack("battery", 1)));
            sources.Add(Source("garage_tools", "Garaj alet kutusu", "toolbox", new Vector3(14f, 0f, 5f), 0,
                Stack("screws", 4), Stack("bracket", 3), Stack("extinguisher", 1)));
            sources.Add(Source("basement_pantry", "Bodrum kileri", "supplycrate", new Vector3(-5f, -3.2f, 4f), -1,
                Stack("water", 2), Stack("food", 2), Stack("sandbag", 4), Stack("plank", 4), Stack("seedling", 4), Stack("hose", 1), Stack("extinguisher", 1)));
            return sources;
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
            for (int i = 0; i < source.Count; i++) copy.Add(new SupplySlot(source[i].Id, source[i].Count));
            return copy;
        }

        private static bool IsRenewable(string id)
        {
            return id == "water" || id == "food" || id == "bandage" || id == "antiseptic" || id == "battery";
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
            int slot = FindStorageSlot(Inventory, id, count);
            if (slot >= 0) AddToSlot(Inventory[slot], id, count);
        }

        private void SetBagCount(string id, int count)
        {
            SupplySlot slot = FindSlot(Bag, id);
            if (count <= 0)
            {
                if (slot != null) { slot.Id = string.Empty; slot.Count = 0; }
                return;
            }
            if (slot == null)
            {
                int empty = FindStorageSlot(Bag, id, count);
                if (empty >= 0) AddToSlot(Bag[empty], id, count);
                return;
            }
            slot.Count = Mathf.Min(count, StackLimitFor(id));
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

        private void ClearRequirements()
        {
            requirementAction = string.Empty;
            requirementTargetId = string.Empty;
        }

        private bool CanPrepare()
        {
            return Model != null && (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial);
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

    /// <summary>Four short, manual work stages. Only completed work may be committed by Expedition.</summary>
    public sealed class PanicWorkSession
    {
        const float AlignTolerance = .12f;
        const float BalanceSeconds = .95f;
        const float SettleSeconds = .25f;
        bool juryFriendly;
        public int RequiredStrokes { get { return juryFriendly ? 2 : 3; } }
        public int CompletedStrokes { get { return strokes; } }
        float CurrentAlignTolerance { get { return juryFriendly ? .19f : AlignTolerance; } }
        float CurrentBalanceSeconds { get { return juryFriendly ? .65f : BalanceSeconds; } }
        public bool JuryFriendly { get { return juryFriendly; } }
        float stageProgress, stageAge, workAge, settleRemaining, strokeCooldown;
        float feedbackAge, toolMotion, baseTarget;
        int strokes, expectedDirection;
        bool applying, spaceWasHeld;

        public bool Active;
        public string Action;
        public string TargetId;
        public string Label;
        public string Hint;
        public int CompletedSteps;
        public int TotalSteps;
        public float Cursor;
        public float Target;
        public float Progress;
        public float Quality;
        public bool Complete;

        public PanicWorkMode Mode { get; private set; }
        public bool IsApplying { get { return applying; } }
        public int ExpectedDirection { get { return applying && Mode == PanicWorkMode.Ratchet && strokes < RequiredStrokes ? expectedDirection : 0; } }
        public string StepHint { get { return Hint; } }
        public float StageProgress { get { return stageProgress; } }
        public float TargetWidth { get { return juryFriendly ? .19f : !applying ? AlignTolerance : Mode == PanicWorkMode.Timing ? .11f : .12f; } }
        public float ToolMotion { get { return toolMotion; } }
        public string StageName { get { return NameForStage(Action, juryFriendly && CompletedSteps > 0 ? 3 : Mathf.Min(CompletedSteps, 3)); } }

        public void Begin(string action, string targetId) { Begin(action, targetId, false); }

        public void Begin(string action, string targetId, bool juryDemo)
        {
            Cancel();
            juryFriendly = juryDemo;
            if (!IsSupported(action)) return;
            Action = action;
            TargetId = targetId ?? string.Empty;
            Label = LabelFor(action);
            Mode = action == "fire" || action == "hose" ? PanicWorkMode.Balance :
                action == "plant" || action == "drain" || action == "flood" ? PanicWorkMode.Timing : PanicWorkMode.Ratchet;
            TotalSteps = juryFriendly ? 2 : 4;
            Quality = 1f;
            Active = true;
            StartStage();
        }

        /// <param name="direction">-1 A, +1 D, 0 neither. Ratchet requires alternating directions.</param>
        /// <param name="held">Space held. Used only for the actively steered pressure test.</param>
        /// <param name="pressed">One-frame Enter press to seat/align a tool.</param>
        public void Tick(float dt, float direction, bool held, bool pressed)
        {
            Tick(dt, direction, held, pressed, held && !spaceWasHeld);
        }

        /// <param name="pulse">One-frame Space press. Independent of held, for taps and strokes.</param>
        public void Tick(float dt, float direction, bool held, bool pressed, bool pulse)
        {
            spaceWasHeld = held;
            if (!Active || Complete || float.IsNaN(dt) || float.IsInfinity(dt) || dt <= 0f) return;
            dt = Mathf.Min(dt, .1f);
            float input = float.IsNaN(direction) || float.IsInfinity(direction) ? 0f : Mathf.Clamp(direction, -1f, 1f);
            workAge += dt;
            stageAge += dt;
            feedbackAge = Mathf.Max(0, feedbackAge - dt);
            strokeCooldown = Mathf.Max(0, strokeCooldown - dt);
            toolMotion = Mathf.MoveTowards(toolMotion, 0, dt * 3.6f);

            if (settleRemaining > 0f)
            {
                settleRemaining = Mathf.Max(0f, settleRemaining - dt);
                if (settleRemaining <= 0f)
                {
                    CompletedSteps++;
                    if (CompletedSteps >= TotalSteps)
                    {
                        Complete = true;
                        applying = false;
                        Progress = 1f;
                        Hint = "Hazır!";
                        return;
                    }
                    StartStage();
                }
                RefreshProgress();
                return;
            }

            if (!applying)
            {
                Cursor = Mathf.Clamp01(Cursor + input * dt * .82f);
                if (pressed)
                {
                    float error = Mathf.Abs(Cursor - Target);
                    if (error <= CurrentAlignTolerance)
                    {
                        applying = true;
                        stageAge = 0f;
                        strokes = 0;
                        expectedDirection = CompletedSteps % 2 == 0 ? -1 : 1;
                        if (Mode == PanicWorkMode.Timing)
                        {
                            Target = CompletedSteps % 2 == 0 ? .66f : .42f;
                            Cursor = .04f;
                        }
                        else if (Mode == PanicWorkMode.Balance)
                        {
                            baseTarget = Target;
                            // Start in the band; player actively steers against flow drift.
                            Cursor = Target;
                        }
                        if (error > .065f) Quality = Mathf.Max(.45f, Quality - .015f);
                        RefreshHint();
                    }
                    else Feedback("İbreyi yeşile getir", false);
                }
            }
            else if (Mode == PanicWorkMode.Ratchet)
            {
                if (strokes < RequiredStrokes && strokeCooldown <= 0f && input * expectedDirection > .5f)
                {
                    toolMotion = expectedDirection;
                    strokes++;
                    stageProgress = strokes / (float)(RequiredStrokes + 1);
                    expectedDirection *= -1;
                    strokeCooldown = juryFriendly ? .24f : .36f;
                    RefreshHint();
                }
                if (pulse)
                {
                    if (strokes >= RequiredStrokes) SeatStage();
                    else Feedback("Önce A / D ile çevir", false);
                }
            }
            else if (Mode == PanicWorkMode.Timing)
            {
                // A force sweep is independent of progress: waiting or holding never completes it.
                Cursor = .04f + Mathf.PingPong(stageAge * .50f, .92f);
                if (pulse)
                {
                    if (Mathf.Abs(Cursor - Target) <= TargetWidth) SeatStage();
                    else
                    {
                        toolMotion = -.6f;
                        Feedback("Yeşilde Space'e dokun", false);
                    }
                }
            }
            else
            {
                Target = Mathf.Clamp01(baseTarget + Mathf.Sin(stageAge * 1.7f + CompletedSteps) * .055f);
                float drift = Mathf.Sin(workAge * 2.1f + CompletedSteps * 1.3f) * .20f + .075f;
                Cursor = Mathf.Clamp01(Cursor + (input * .70f + drift) * dt);
                bool inBand = Mathf.Abs(Cursor - Target) <= TargetWidth;
                if (held && inBand)
                {
                    stageProgress = Mathf.Min(1f, stageProgress + dt / CurrentBalanceSeconds);
                    toolMotion = .45f + stageProgress * .5f;
                    if (stageProgress >= 1f) SeatStage();
                }
                else
                {
                    if (held && !inBand) stageProgress = Mathf.Max(0, stageProgress - dt * .22f);
                    if (feedbackAge <= 0f) Hint = !inBand ? "A / D ile basıncı dengele" : "Yeşilde Space basılı tut";
                }
            }
            if (feedbackAge <= 0f && settleRemaining <= 0f) RefreshHint();
            RefreshProgress();
        }

        public void Cancel()
        {
            juryFriendly = false;
            Active = false;
            Action = TargetId = Label = Hint = string.Empty;
            CompletedSteps = TotalSteps = strokes = expectedDirection = 0;
            Cursor = Target = Progress = Quality = stageProgress = stageAge = workAge = 0f;
            settleRemaining = strokeCooldown = feedbackAge = toolMotion = baseTarget = 0f;
            applying = Complete = spaceWasHeld = false;
            Mode = PanicWorkMode.Ratchet;
        }

        void StartStage()
        {
            applying = false;
            stageProgress = stageAge = settleRemaining = feedbackAge = 0f;
            strokes = 0;
            Cursor = .5f;
            unchecked
            {
                uint hash = 2166136261u;
                string seed = (Action ?? string.Empty) + "|" + (TargetId ?? string.Empty) + "|" + CompletedSteps;
                for (int i = 0; i < seed.Length; i++) hash = (hash ^ seed[i]) * 16777619u;
                Target = juryFriendly ? ((hash & 1u) == 0u ? .36f : .64f) : ((hash & 1u) == 0u ? .26f : .74f);
            }
            baseTarget = Target;
            RefreshHint();
            RefreshProgress();
        }

        void SeatStage()
        {
            stageProgress = 1f;
            settleRemaining = juryFriendly ? .16f : SettleSeconds;
            toolMotion = 1f;
            Hint = "Yerine oturdu";
        }

        void Feedback(string hint, bool good)
        {
            Hint = hint;
            feedbackAge = .65f;
            if (!good) Quality = Mathf.Max(.35f, Quality - .04f);
        }

        void RefreshHint()
        {
            if (Complete) { Hint = "Hazır!"; return; }
            if (feedbackAge > 0f || settleRemaining > 0f) return;
            if (!applying) Hint = "A / D ile hedefle · Enter";
            else if (Mode == PanicWorkMode.Ratchet) Hint = strokes >= RequiredStrokes ? "Space ile yerine oturt" : expectedDirection < 0 ? "A ile sola çevir" : "D ile sağa çevir";
            else if (Mode == PanicWorkMode.Timing) Hint = "Yeşilde Space'e dokun";
            else Hint = Mathf.Abs(Cursor - Target) > TargetWidth ? "A / D ile basıncı dengele" : "Yeşilde Space basılı tut";
        }

        void RefreshProgress()
        {
            Progress = TotalSteps <= 0 ? 0f : Mathf.Clamp01((CompletedSteps + stageProgress) / TotalSteps);
            if (Complete) Progress = 1f;
        }

        static bool IsSupported(string action)
        {
            return action == "anchorwall" || action == "anchorfloor" || action == "upgrade" ||
                action == "flood" || action == "fire" || action == "plant" || action == "drain" ||
                action == "fence" || action == "hose";
        }

        static readonly string[] PlantStages = { "Çukuru aç", "Fidanı indir", "Toprağı bastır", "Can suyunu ver" };
        static readonly string[] DrainStages = { "Kanalı aç", "Eğimi ver", "Izgarayı yerleştir", "Akışı kontrol et" };
        static readonly string[] FloodStages = { "Sol kenarı kapat", "Üst kenarı kapat", "Sağ kenarı kapat", "Alt kenarı kapat" };
        static readonly string[] FireStages = { "Pimi kontrol et", "Vanayı dene", "Basıncı ayarla", "Hazır konuma al" };
        static readonly string[] HoseStages = { "Bağlantıyı oturt", "Vanayı aç", "Kaçağı denetle", "Basıncı ayarla" };

        static string NameForStage(string action, int index)
        {
            switch (action)
            {
                case "plant": return PlantStages[index];
                case "drain": return DrainStages[index];
                case "flood": return FloodStages[index];
                case "fire": return FireStages[index];
                case "hose": return HoseStages[index];
                default: return "Bağlantı " + (index + 1);
            }
        }

        static string LabelFor(string action)
        {
            switch (action)
            {
                case "anchorwall": return "Duvara sabitle";
                case "anchorfloor": return "Zemine sabitle";
                case "upgrade": return "Yapıyı güçlendir";
                case "flood": return "Sel bariyeri";
                case "fire": return "Yangın tüpü";
                case "plant": return "Fidan dik";
                case "drain": return "Drenaj aç";
                case "fence": return "Sel çiti kur";
                case "hose": return "Hortum bağla";
                default: return "Hazırlık işi";
            }
        }
    }
}
