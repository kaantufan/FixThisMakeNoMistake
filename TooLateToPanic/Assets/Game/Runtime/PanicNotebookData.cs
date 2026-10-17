using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public readonly struct PanicNotebookCheck
    {
        public readonly string Label, Detail;
        public readonly int Done, Total;
        public bool Complete { get { return Total > 0 && Done >= Total; } }
        public PanicNotebookCheck(string label, string detail, int done, int total)
        { Label = label; Detail = detail; Done = Mathf.Max(0, done); Total = Mathf.Max(0, total); }
    }

    public readonly struct PanicNotebookSlot
    {
        public readonly string Id, Label;
        public readonly int Count;
        public PanicNotebookSlot(string id, string label, int count) { Id = id; Label = label; Count = count; }
    }

    public readonly struct PanicNotebookPin
    {
        public readonly string Id, Label;
        public readonly Vector3 Position;
        public readonly bool Open, Empty;
        public PanicNotebookPin(LootSource source)
        {
            Id = source.Id; Label = source.Label; Position = source.Position; Open = source.Open;
            Empty = true;
            if (source.Contents != null) for (int i = 0; i < source.Contents.Count; i++)
                if (source.Contents[i] != null && source.Contents[i].Count > 0) { Empty = false; break; }
        }
        public PanicNotebookPin(PanicWorldSupply source)
        {
            Id = source.Id; Label = PanicExpedition.DisplayName(source.ItemId); Position = source.Position;
            Open = false; Empty = source.Count <= 0;
        }
    }

    /// <summary>Read-only notebook snapshot. Capturing never calls preflight, work, pack or score mutations.</summary>
    public sealed class PanicNotebookSnapshot
    {
        public string Phase, Disaster, Budget, Clock, Note, SelectedLabel, SelectedAction;
        public int Wave, Floor, InventoryUsed, BagUsed, InventoryCapacity, BagCapacity, RequiredDays, PackedWater, PackedFood;
        public int[] Materials;
        public float Damage;
        public bool BagReady, CheatedRun;
        public Vector3 ActorPosition;
        public PanicNotebookSlot[] Inventory, Bag;
        public PanicNotebookPin[] Pins;
        public PanicNotebookCheck[][] Pages;
        public PanicMaterialRequirement[] SelectedRequirements;
        public int SelectedBudgetCost;
    }

    public static class PanicNotebookData
    {
        public static readonly string[] MaterialIds = { "screws", "bracket", "plank", "sandbag", "seedling", "hose", "extinguisher" };

        public static PanicNotebookSnapshot Capture(PanicHomeWorld world, int floor)
        {
            if (!world || world.Model == null || world.Expedition == null) return null;
            var result = Capture(world.Model, world.Expedition, world.Selected, floor, world.JurySnapshot);
            result.ActorPosition = world.PlayerPosition;
            if (world.BagFinalStageActive)
            {
                var supplies = new List<PanicNotebookPin>();
                foreach (var supply in world.Expedition.WorldSupplies)
                    if (supply.Floor == floor && supply.Count > 0) supplies.Add(new PanicNotebookPin(supply));
                result.Pins = supplies.ToArray();
                result.SelectedAction = string.Empty;
                result.SelectedRequirements = new PanicMaterialRequirement[0];
                result.SelectedBudgetCost = 0;
            }
            return result;
        }

        public static PanicNotebookSnapshot Capture(PanicModel model, PanicExpedition expedition, PanicItem selected, int floor, PanicJurySnapshot jury = null)
        {
            if (model == null || expedition == null) return null;
            int anchorTotal = 0, anchors = 0, walls = 0, wallsDone = 0, windows = 0, windowsDone = 0, roofs = 0, roofsDone = 0;
            if (model.Items != null) foreach (var item in model.Items)
            {
                if (item == null || !item.Installed) continue;
                string action = PanicItemActionRules.PrimaryAction(item);
                if (action == "anchorwall" || action == "anchorfloor" || PanicItemActionRules.CanReleaseAnchor(item))
                { anchorTotal++; if (item.Anchored) anchors++; }
                if (item.Kind == ItemKind.Wall) { walls++; if (item.Tier > 0) wallsDone++; }
                if (item.Kind == ItemKind.Window || item.Kind == ItemKind.Door) { windows++; if (item.Tier > 0) windowsDone++; }
                if (item.Kind == ItemKind.Roof) { roofs++; if (item.Tier > 0) roofsDone++; }
            }
            var water = new PanicNotebookCheck("Çantaya su koy", "Bu dalganın su ihtiyacı", model.PackedWater, model.RequiredDays);
            var food = new PanicNotebookCheck("Çantaya gıda koy", "Bu dalganın gıda ihtiyacı", model.PackedFood, model.RequiredDays);
            var firstAid = new PanicNotebookCheck("İlk yardım setini çantaya koy", "Çantadaki destek ekipmanı", (model.PackedSupportMask & 1) != 0 ? 1 : 0, 1);
            var flashlight = new PanicNotebookCheck("Pilli el fenerini hazırla", "Çantadaki destek ekipmanı", (model.PackedSupportMask & 2) != 0 ? 1 : 0, 1);
            var anchor = new PanicNotebookCheck("Uygun mobilyaları sabitle", "Dünyada tamamlanan ankrajlar", anchors, anchorTotal);
            var wall = new PanicNotebookCheck("Duvarları güçlendir", "En az bir kademe güçlendirme", wallsDone, walls);
            var window = new PanicNotebookCheck("Kapı ve pencereleri güçlendir", "En az bir kademe güçlendirme", windowsDone, windows);
            int drains = Committed(expedition, "drain", "drain_west", "drain_east");
            int trees = Committed(expedition, "plant", "plant_west1", "plant_west2", "plant_east");
            var pages = new PanicNotebookCheck[6][];
            pages[(int)PanicNotebookPage.Rooms] = new PanicNotebookCheck[0];
            pages[(int)PanicNotebookPage.Flood] = new[] {
                new PanicNotebookCheck("Bahçe drenajını kur", "Batı ve doğu gerçek drenaj noktaları", drains, 2),
                window, wall, water, food };
            pages[(int)PanicNotebookPage.Earthquake] = new[] { anchor, wall, water, food, firstAid, flashlight };
            pages[(int)PanicNotebookPage.Fire] = new[] {
                new PanicNotebookCheck("Yangın söndürücüyü hazırla", "Yalnız yangın ekipmanı noktasında", expedition.IsWorkCommitted("fire", PanicItemActionRules.FireStationId) ? 1 : 0, 1),
                new PanicNotebookCheck("Bahçe hortumunu hazırla", "Gerçek hortum istasyonunda", expedition.IsWorkCommitted("hose", "hose_station") ? 1 : 0, 1),
                firstAid, flashlight, water, food };
            pages[(int)PanicNotebookPage.Storm] = new[] {
                new PanicNotebookCheck("Bahçe ağaç sıralarını kur", "Üç gerçek bahçe bölgesi", trees, 3),
                new PanicNotebookCheck("Çatıyı güçlendir", "En az bir kademe güçlendirme", roofsDone, roofs), window, anchor,
                new PanicNotebookCheck("Pilli radyoyu çantaya koy", "Çantadaki destek ekipmanı", (model.PackedSupportMask & 4) != 0 ? 1 : 0, 1),
                new PanicNotebookCheck("Su ve gıdayı tamamla", "Bu dalga için yeterli erzak", model.BagReady ? 1 : 0, 1) };
            pages[(int)PanicNotebookPage.Inventory] = new[] { water, food, firstAid, flashlight };
            var pins = new List<PanicNotebookPin>();
            if (expedition.Sources != null) foreach (var source in expedition.Sources)
                if (source != null && source.Floor == floor) pins.Add(new PanicNotebookPin(source));
            var materials = new int[MaterialIds.Length];
            for (int i = 0; i < materials.Length; i++) materials[i] = expedition.Count(MaterialIds[i]);
            string selectedAction = selected == null ? string.Empty : PanicItemActionRules.PrimaryAction(selected);
            var recipe = PanicExpedition.MaterialCosts(selectedAction, selected);
            var requirements = new PanicMaterialRequirement[recipe.Count];
            for (int i = 0; i < recipe.Count; i++) requirements[i] = new PanicMaterialRequirement(recipe[i].Id, recipe[i].Required, expedition.Count(recipe[i].Id));
            bool timedJury = jury != null && jury.Active;
            string note = timedJury ? jury.Hint : model.TimeRemaining > 30 ? "Önlemleri tamamla. Son 30 saniyede çanta hazırlığı başlayacak." : model.BagReady ? "Su ve gıda hazır. Bodrumdaki sığınağa git." :
                "Açıktaki erzakları al: " + Mathf.Max(0, model.RequiredDays - model.PackedWater) + " su ve " + Mathf.Max(0, model.RequiredDays - model.PackedFood) + " gıda gerekli. Temel ihtiyaçlar doğrudan çantana eklenir.";
            return new PanicNotebookSnapshot {
                Phase = PhaseName(model.Phase), Disaster = DisasterName(model.Disaster), Wave = model.Wave,
                Budget = model.InfiniteBudget ? "SINIRSIZ" : model.Budget.ToString("N0"), Damage = Mathf.Clamp(model.TotalDamage, 0, 100),
                Clock = Clock(timedJury ? jury.StageRemainingSeconds : model.TimeRemaining), Note = note,
                Floor = Mathf.Clamp(floor, -1, 1), Inventory = Slots(expedition.Inventory), Bag = Slots(expedition.Bag),
                InventoryUsed = Used(expedition.Inventory), BagUsed = Used(expedition.Bag),
                InventoryCapacity = expedition.Inventory == null ? 0 : expedition.Inventory.Count, BagCapacity = expedition.Bag == null ? 0 : expedition.Bag.Count,
                BagReady = model.BagReady, CheatedRun = model.CheatedRun, Pins = pins.ToArray(), Pages = pages, Materials = materials,
                RequiredDays = model.RequiredDays, PackedWater = model.PackedWater, PackedFood = model.PackedFood,
                SelectedLabel = selected == null ? string.Empty : selected.Label, SelectedAction = selectedAction,
                SelectedRequirements = requirements, SelectedBudgetCost = PanicExpedition.WorkCost(selectedAction, selected)
            };
        }

        public static string PageName(PanicNotebookPage page)
        { switch (page) { case PanicNotebookPage.Rooms: return "Ev planı"; case PanicNotebookPage.Flood: return "Sel"; case PanicNotebookPage.Earthquake: return "Deprem"; case PanicNotebookPage.Fire: return "Yangın"; case PanicNotebookPage.Storm: return "Fırtına"; default: return "Envanter"; } }
        public static string FloorName(int floor) { return floor < 0 ? "Bodrum" : floor > 0 ? "Üst kat" : "Zemin kat"; }
        public static string PhaseName(PanicPhase phase)
        { switch (phase) { case PanicPhase.Preparing: return "HAZIRLIK"; case PanicPhase.Tutorial: return "ALIŞTIRMA"; case PanicPhase.Inspection: return "HASAR GEZİSİ"; case PanicPhase.Disaster: return "AFET"; case PanicPhase.Aftermath: return "AFET SONRASI"; case PanicPhase.GameOver: return "KOŞU BİTTİ"; case PanicPhase.Success: return "TAMAMLANDI"; default: return "BAŞLIK"; } }
        public static string DisasterName(DisasterType type)
        { return type == DisasterType.Earthquake ? "DEPREM" : type == DisasterType.Flood ? "SEL" : type == DisasterType.Fire ? "YANGIN" : "FIRTINA"; }
        public static string Clock(float seconds)
        { int value = Mathf.CeilToInt(Mathf.Max(0, seconds)); return (value / 60).ToString("00") + ":" + (value % 60).ToString("00"); }
        static int Committed(PanicExpedition expedition, string action, params string[] targets)
        { int count = 0; for (int i = 0; i < targets.Length; i++) if (expedition.IsWorkCommitted(action, targets[i])) count++; return count; }
        static int Used(List<SupplySlot> slots)
        { int count = 0; if (slots != null) foreach (var slot in slots) if (slot != null && slot.Count > 0 && !string.IsNullOrEmpty(slot.Id)) count++; return count; }
        static PanicNotebookSlot[] Slots(List<SupplySlot> slots)
        {
            if (slots == null) return new PanicNotebookSlot[0];
            var result = new PanicNotebookSlot[slots.Count];
            for (int i = 0; i < result.Length; i++) { var slot = slots[i]; string id = slot == null ? string.Empty : slot.Id;
                result[i] = new PanicNotebookSlot(id, PanicExpedition.DisplayName(id), slot == null ? 0 : Mathf.Max(0, slot.Count)); }
            return result;
        }
    }
}
