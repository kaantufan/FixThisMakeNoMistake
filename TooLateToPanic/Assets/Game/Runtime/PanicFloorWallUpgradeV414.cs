using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Read-only quote. Internal member references and tiers bind completion to the actual started floor.</summary>
    public sealed class PanicFloorWallUpgradePlanV414
    {
        public int Floor { get; private set; }
        public int TargetTier { get; private set; }
        public int WallCount { get { return Members.Length; } }
        public int ChangedWallCount { get; private set; }
        public int BudgetCost { get; private set; }
        public IReadOnlyList<PanicMaterialCost> MaterialCosts { get; private set; }
        public IReadOnlyList<string> WallIds { get; private set; }
        public string FloorLabel { get { return Floor == 0 ? "Zemin Kat" : Floor == -1 ? "Bodrum" : Floor + ". Kat"; } }
        public string Label { get { return FloorLabel + " Duvarlarını Güçlendir"; } }
        public string Detail { get { return ChangedWallCount + " / " + WallCount + " duvar · Hedef: " + TargetTier + ". seviye"; } }
        internal readonly PanicModel Owner;
        internal readonly string SelectedId;
        internal readonly PanicItem[] Members;
        internal readonly int[] Tiers;

        internal PanicFloorWallUpgradePlanV414(PanicModel model, string selectedId, int floor, int target,
            List<PanicItem> members, int changed, int budget, List<PanicMaterialCost> costs)
        {
            Owner = model; SelectedId = selectedId; Floor = floor; TargetTier = target;
            Members = members.ToArray(); Tiers = new int[Members.Length]; var ids = new string[Members.Length];
            for (int i = 0; i < Members.Length; i++) { Tiers[i] = Members[i].Tier; ids[i] = Members[i].Id; }
            WallIds = Array.AsReadOnly(ids); ChangedWallCount = changed; BudgetCost = budget;
            MaterialCosts = costs.AsReadOnly();
        }
    }

    public sealed partial class PanicExpedition
    {
        PanicFloorWallUpgradePlanV414 floorWallWorkV414;
        public PanicFloorWallUpgradePlanV414 ActiveFloorWallUpgradeV414 { get { return floorWallWorkV414; } }
        static int WallFloorV414(PanicItem item) { return Mathf.FloorToInt((item.Position.y + .15f) / 3.2f); }
        static bool ValidWallHeightV414(PanicItem item)
        { return !float.IsNaN(item.Position.y) && !float.IsInfinity(item.Position.y); }
        bool IsFloorWallRequestV414(string action, string targetId)
        { var item = FindItem(targetId); return action == "upgrade" && item != null && item.Kind == ItemKind.Wall; }

        /// <summary>Uses actual Model wall records, never the actor floor, IDs/prefixes or visible mesh membership.</summary>
        public bool TryGetFloorWallUpgradeV414(string targetId, out PanicFloorWallUpgradePlanV414 plan)
        {
            plan = null;
            if (Model == null || Model.Items == null) return false;
            var selected = FindItem(targetId);
            if (selected == null || selected.Kind != ItemKind.Wall || !selected.Installed || !ValidWallHeightV414(selected)) return false;
            int floor = WallFloorV414(selected), minimum = 2;
            var members = new List<PanicItem>(); var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Model.Items)
            {
                if (item == null || item.Kind != ItemKind.Wall) continue;
                if (!ValidWallHeightV414(item)) return false;
                if (WallFloorV414(item) != floor) continue;
                if (!item.Installed || item.Tier < 0 || item.Tier > 2 || string.IsNullOrEmpty(item.Id) || !ids.Add(item.Id)) return false;
                members.Add(item); minimum = Math.Min(minimum, item.Tier);
            }
            if (members.Count == 0 || minimum >= 2) return false;
            members.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            int target = minimum + 1, changed = 0; long budget = 0;
            var totals = new Dictionary<string, long>(StringComparer.Ordinal); var order = new List<string>();
            foreach (var item in members)
            {
                if (item.Tier >= target) continue;
                changed++; budget += WorkCost("upgrade", item);
                foreach (var cost in MaterialCosts("upgrade", item))
                {
                    if (!totals.ContainsKey(cost.Id)) { totals[cost.Id] = 0; order.Add(cost.Id); }
                    totals[cost.Id] += cost.Required;
                    if (totals[cost.Id] > int.MaxValue) return false;
                }
                if (budget > int.MaxValue) return false;
            }
            var costs = new List<PanicMaterialCost>();
            foreach (string id in order) costs.Add(new PanicMaterialCost(id, (int)totals[id]));
            plan = new PanicFloorWallUpgradePlanV414(Model, targetId, floor, target, members, changed, (int)budget, costs);
            return true;
        }

        // Legacy static recipes remain per-item; active callers resolve their own Model target through these APIs.
        public IReadOnlyList<PanicMaterialCost> WorkMaterialCostsV414(string action, string targetId)
        {
            PanicFloorWallUpgradePlanV414 plan;
            return IsFloorWallRequestV414(action, targetId) ?
                TryGetFloorWallUpgradeV414(targetId, out plan) ? plan.MaterialCosts : new PanicMaterialCost[0] :
                MaterialCosts(action, FindItem(targetId));
        }
        public int WorkBudgetCostV414(string action, string targetId)
        {
            PanicFloorWallUpgradePlanV414 plan;
            return IsFloorWallRequestV414(action, targetId) ?
                TryGetFloorWallUpgradeV414(targetId, out plan) ? plan.BudgetCost : 0 : WorkCost(action, FindItem(targetId));
        }
        public IReadOnlyList<PanicMaterialRequirement> WorkRequirementsV414(string action, string targetId)
        { return RequirementsFor(WorkMaterialCostsV414(action, targetId), false); }
        public string PrimaryActionV414(PanicItem item)
        {
            PanicFloorWallUpgradePlanV414 plan;
            if (item != null && item.Kind == ItemKind.Wall)
                return ReferenceEquals(FindItem(item.Id), item) && TryGetFloorWallUpgradeV414(item.Id, out plan) ? "upgrade" : string.Empty;
            return PanicItemActionRules.PrimaryAction(item);
        }
        public int PrimaryShortcutV414(PanicItem item)
        { return item != null && item.Kind == ItemKind.Wall ? PrimaryActionV414(item) == "upgrade" ? 3 : 0 : PanicItemActionRules.PrimaryShortcut(item); }
        public string PrimaryLabelV414(PanicItem item)
        { return item != null && item.Kind == ItemKind.Wall ? PrimaryActionV414(item) == "upgrade" ? "KATI GÜÇLENDİR" : string.Empty : PanicItemActionRules.PrimaryLabel(item); }
        public string WorkTargetLabelV414(string action, string targetId)
        {
            PanicFloorWallUpgradePlanV414 plan;
            if (action == "upgrade" && TryGetFloorWallUpgradeV414(targetId, out plan)) return plan.Label + " · " + plan.Detail;
            var item = FindItem(targetId); return item == null ? string.Empty : item.Label;
        }
        public string FloorWallRequirementLabelV414
        {
            get
            {
                PanicFloorWallUpgradePlanV414 plan;
                return requirementAction == "upgrade" && TryGetFloorWallUpgradeV414(requirementTargetId, out plan) ?
                    plan.FloorLabel + " · " + plan.ChangedWallCount + " duvar → " + plan.TargetTier + ". seviye" : string.Empty;
            }
        }

        bool CanBeginFloorWallUpgradeV414(string targetId)
        {
            PanicFloorWallUpgradePlanV414 plan;
            if (!TryGetFloorWallUpgradeV414(targetId, out plan)) return Fail("Bu katın duvarları tamamlanmış veya toplu güçlendirmeye uygun değil.");
            foreach (var item in plan.Members)
                if (!Model.CanBeginJuryWork("upgrade", item.Id)) return Fail("Bu aşamada kat güçlendirmesi yapılamaz.");
            if ((!Model.InfiniteBudget && Model.Budget < plan.BudgetCost) || (!Model.FreeMaterials && !HasCosts(plan.MaterialCosts)))
                return RequestRequirements("upgrade", targetId);
            return true;
        }
        public bool BeginValidatedWorkV414(string action, string targetId, bool juryDemo)
        {
            if (!CanBeginWork(action, targetId)) return false;
            PanicFloorWallUpgradePlanV414 plan = null;
            if (IsFloorWallRequestV414(action, targetId) && !TryGetFloorWallUpgradeV414(targetId, out plan)) return false;
            CancelFloorWallUpgradeV414();
            Work.Begin(action, targetId, juryDemo);
            if (!Work.Active) return false;
            floorWallWorkV414 = plan;
            if (plan != null) Work.Label = plan.Label + " · " + plan.Detail;
            return true;
        }
        public void CancelFloorWallUpgradeV414() { floorWallWorkV414 = null; }
        bool FloorWallPlanMatchesV414(PanicFloorWallUpgradePlanV414 current)
        {
            var started = floorWallWorkV414;
            if (started == null || !ReferenceEquals(started.Owner, Model) || current.SelectedId != started.SelectedId ||
                current.Floor != started.Floor || current.TargetTier != started.TargetTier || current.WallCount != started.WallCount ||
                current.ChangedWallCount != started.ChangedWallCount || current.BudgetCost != started.BudgetCost) return false;
            for (int i = 0; i < started.Members.Length; i++)
                if (!ReferenceEquals(started.Members[i], current.Members[i]) || started.Tiers[i] != current.Tiers[i]) return false;
            return true;
        }
        bool TryCompleteFloorWallUpgradeV414(string targetId)
        {
            PanicFloorWallUpgradePlanV414 current;
            if (!TryGetFloorWallUpgradeV414(targetId, out current) || !FloorWallPlanMatchesV414(current))
                return Fail("Katın duvarları iş sırasında değişti. Malzemelerin sende kaldı; işi yeniden başlat.");
            foreach (var item in current.Members)
                if (!Model.CanBeginJuryWork("upgrade", item.Id)) return Fail("Bu aşamada iş tamamlanamaz; malzemelerin sende kaldı.");
            if ((!Model.InfiniteBudget && Model.Budget < current.BudgetCost) || (!Model.FreeMaterials && !HasCosts(current.MaterialCosts)))
                return RequestRequirements("upgrade", targetId);

            // Synchronous transaction: every member/cost was checked above; no callbacks until after the full commit.
            ConsumeCosts(current.MaterialCosts);
            if (!Model.InfiniteBudget) Model.Budget -= current.BudgetCost;
            foreach (var item in current.Members) if (item.Tier < current.TargetTier) item.Tier = current.TargetTier;
            Work.Cancel(); CancelFloorWallUpgradeV414(); ClearRequirements();
            Message = current.Label + " tamamlandı · " + current.Detail;
            NotifyModel();
            return true;
        }
    }

    public sealed partial class PanicHomeHud
    {
        string RequirementTitleV414(PanicExpedition expedition)
        { return string.IsNullOrEmpty(expedition.FloorWallRequirementLabelV414) ? RequirementTitle(expedition.RequirementAction) : expedition.FloorWallRequirementLabelV414; }
        bool DrawFloorWallUpgradePreviewV414(PanicItem item)
        {
            if (item.Kind != ItemKind.Wall) return false;
            PanicFloorWallUpgradePlanV414 plan;
            if (!World.Expedition.TryGetFloorWallUpgradeV414(item.Id, out plan) || !World.Model.CanBeginJuryWork("upgrade", item.Id)) return true;
            Rect panel = new Rect(1100, 170, 476, 312);
            Frame(panel, Night, Line, 1f); Region(panel);
            FittedLabel(new Rect(1118, 179, 438, 29), plan.Label, _body, MainText, 16);
            Label(new Rect(1118, 210, 438, 24), plan.Detail, ColorText(_small, Brass));
            Button(new Rect(1118, 239, 438, 52), "KATI GÜÇLENDİR · 3", true, Brass, () => World.QuickAction(3));
            Label(new Rect(1118, 302, 192, 20), "TOPLAM BEDEL", _tiny);
            Label(new Rect(1320, 302, 66, 20), "VAR", _caps);
            Label(new Rect(1390, 302, 66, 20), "GEREK", _caps);
            Label(new Rect(1460, 302, 80, 20), "EKSİK", ColorText(_caps, Brass));
            var requirements = World.Expedition.WorkRequirementsV414("upgrade", item.Id);
            for (int i = 0; i < requirements.Count; i++)
            {
                var row = requirements[i]; float y = 330 + i * 38;
                DrawIcon(new Rect(1118, y, 30, 30), row.Id);
                Label(new Rect(1156, y + 2, 150, 28), PanicExpedition.DisplayName(row.Id), _small);
                DrawFloorWallCostV414(y, row.Owned, row.Required, row.Missing);
            }
            int missing = World.Model.InfiniteBudget ? 0 : Mathf.Max(0, plan.BudgetCost - World.Model.Budget);
            Label(new Rect(1118, 408, 190, 28), "Bütçe · $", _small);
            DrawFloorWallCostV414(406, World.Model.Budget, plan.BudgetCost, missing);
            Label(new Rect(1118, 450, 438, 23), "İş bitince bir kez harcanır. İptal ücretsiz.", _tiny);
            return true;
        }
        void DrawFloorWallCostV414(float y, int owned, int required, int missing)
        {
            Label(new Rect(1320, y, 66, 30), owned.ToString(), _center);
            Label(new Rect(1390, y, 66, 30), required.ToString(), _center);
            Label(new Rect(1460, y, 80, 30), missing.ToString(), ColorText(_center, missing > 0 ? Brass : Sea));
        }
    }
}
