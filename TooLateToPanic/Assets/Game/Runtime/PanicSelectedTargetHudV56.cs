using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>A read-only quote from the same recipes used by work preflight and commit.</summary>
    public sealed class PanicSelectedTargetSnapshotV56
    {
        public string Id { get; }
        public string Label { get; }
        public string Action { get; }
        public string ActionLabel { get; }
        public string Hint { get; }
        public string Detail { get; }
        public bool IsRoof { get; }
        public bool IsSupply { get; }
        public IReadOnlyList<PanicMaterialRequirement> Requirements { get; }
        public int BudgetCost { get; }
        public int BudgetOwned { get; }
        public int BudgetMissing { get; }
        internal PanicSelectedTargetSnapshotV56(string id, string label, string action, string actionLabel,
            string hint, string detail, bool roof, bool supply, IReadOnlyList<PanicMaterialRequirement> requirements,
            int cost, int budget, int missing)
        {
            Id = id; Label = label; Action = action; ActionLabel = actionLabel; Hint = hint; Detail = detail;
            IsRoof = roof; IsSupply = supply; Requirements = requirements;
            BudgetCost = cost; BudgetOwned = budget; BudgetMissing = missing;
        }
    }

    public sealed partial class PanicHomeWorld
    {
        public string SelectedTargetIdV56 { get { return focusId ?? string.Empty; } }

        public bool TryGetSelectedTargetSnapshotV56(out PanicSelectedTargetSnapshotV56 snapshot)
        {
            snapshot = null;
            if (Model == null || Expedition == null || string.IsNullOrEmpty(focusId) || !CanInteract) return false;
            string id = focusId, label = FocusLabel ?? string.Empty, action = string.Empty;
            string actionLabel = string.Empty, hint = ContextHint ?? string.Empty, detail = string.Empty;
            bool bag = BagFinalStageActive || BagCameraOwnsView;
            var item = Selected;
            var supply = Expedition.FindWorldSupply(id);
            var source = NearestSource;
            var site = gardenSites.Find(candidate => candidate.id == id);
            bool isRoof = item != null && item.Kind == ItemKind.Roof;
            bool isSupply = supply != null;
            IReadOnlyList<PanicMaterialRequirement> requirements = Array.Empty<PanicMaterialRequirement>();

            if (supply != null)
            {
                if (supply.Count <= 0 || !BagFirstPersonActive) return false;
                label = PanicExpedition.DisplayName(supply.ItemId);
                detail = "Bu noktada kalan: " + supply.Count;
                if (supply.ItemId == "water" || supply.ItemId == "food")
                {
                    int packed = supply.ItemId == "water" ? Model.PackedWater : Model.PackedFood;
                    requirements = new[] { new PanicMaterialRequirement(supply.ItemId, Model.RequiredDays, packed) };
                }
            }
            else if (item != null)
            {
                label = isRoof ? "ÇATI · " + item.Label : item.Label;
                if (!bag)
                {
                    action = Expedition.PrimaryActionV414(item);
                    actionLabel = Expedition.PrimaryLabelV414(item);
                    if (!string.IsNullOrEmpty(action) && Model.CanBeginJuryWork(action, id))
                    {
                        string shortcut = Expedition.PrimaryShortcutV414(item) + " · " + actionLabel;
                        hint = string.IsNullOrWhiteSpace(hint) || string.IsNullOrWhiteSpace(ShortAction) ? shortcut : hint + "   ·   " + shortcut;
                        detail = Expedition.WorkTargetLabelV414(action, id);
                        if (detail == item.Label) detail = string.Empty;
                    }
                    else
                    {
                        action = actionLabel = string.Empty;
                        if (PanicItemActionRules.CanReleaseAnchor(item))
                        { hint = "1 · ANKRAJI ÇÖZ"; detail = "Sabitleme tamamlandı."; }
                        else if (item.Tier > 0) detail = "Güçlendirme seviyesi: " + item.Tier;
                    }
                }
            }
            else
            {
                string utility = PanicUtilitySafetyV414.ActionForTarget(id);
                if (!string.IsNullOrEmpty(utility))
                {
                    label = PanicUtilitySafetyV414.TargetLabel(id);
                    bool complete = Model.UtilitySafety.IsComplete(utility);
                    detail = PanicUtilitySafetyV414.StateLabel(utility, complete);
                    if (!bag && !complete) { action = utility; actionLabel = PanicUtilitySafetyV414.Label(utility); }
                }
                else if (site != null)
                {
                    label = site.label;
                    if (!bag && !site.done && !Expedition.IsWorkCommitted(site.action, id))
                    { action = site.action; actionLabel = site.label; }
                    else detail = site.done ? "Kurulum tamamlandı." : string.Empty;
                }
                else if (id == PanicItemActionRules.FireStationId)
                {
                    label = "Yangın ekipmanı noktası";
                    if (!bag && !Expedition.IsWorkCommitted("fire", id)) { action = "fire"; actionLabel = ShortAction; }
                    else detail = "Yangın tüpü hazır.";
                }
                else if (hinges.TryGetValue(id, out var hinge) && string.IsNullOrWhiteSpace(label))
                { label = hinge.lid ? "Sığınak kapağı" : "Kapı"; hint = "E · " + (hinge.open ? "Kapat" : "Aç"); }
            }

            if (source != null)
            {
                if (bag) return false;
                if (string.IsNullOrWhiteSpace(label)) label = source.Label;
                var stock = new List<string>();
                if (source.Contents != null) foreach (var slot in source.Contents)
                    if (slot != null && slot.Count > 0 && !string.IsNullOrEmpty(slot.Id))
                        stock.Add(PanicExpedition.DisplayName(slot.Id) + " " + slot.Count);
                string contents = stock.Count > 0 ? "Kalan stok: " + string.Join(" · ", stock) : "Malzeme kaynağı boş.";
                detail = string.IsNullOrEmpty(detail) ? contents : detail + "\n" + contents;
            }
            if (string.IsNullOrWhiteSpace(label)) return false;
            if (!string.IsNullOrEmpty(action))
            {
                if (!Model.CanBeginJuryWork(action, id)) { action = actionLabel = string.Empty; }
                else requirements = Expedition.WorkRequirementsV414(action, id);
            }
            int cost = string.IsNullOrEmpty(action) ? 0 : Expedition.WorkBudgetCostV414(action, id);
            int budget = Mathf.Max(0, Model.Budget);
            int missing = Model.InfiniteBudget ? 0 : Mathf.Max(0, cost - budget);
            snapshot = new PanicSelectedTargetSnapshotV56(id, label, action, actionLabel, hint, detail,
                isRoof, isSupply, requirements, cost, budget, missing);
            return true;
        }
    }

    public sealed partial class PanicHomeHud
    {
        GUIStyle selectedTargetTitleStyleV56, selectedTargetBodyStyleV56;
        Rect selectedTargetBoundsV56 = new Rect(1150f, 112f, 426f, 0f);
        int selectedTargetRepaintFrameV56 = -1;
        public Rect SelectedTargetHudBoundsV56 { get { return selectedTargetBoundsV56; } }
        public int SelectedTargetHudDrawFramesV56 { get; private set; }
        public string SelectedTargetRenderedTextV56 { get; private set; }
        public PanicSelectedTargetSnapshotV56 SelectedTargetSnapshotV56
        {
            get { PanicSelectedTargetSnapshotV56 snapshot; return World && World.TryGetSelectedTargetSnapshotV56(out snapshot) ? snapshot : null; }
        }
        public bool SelectedTargetHudVisibleV56
        {
            get
            {
                if (!World || World.Model == null || World.Expedition == null || _paused || World.Model.IsPaused ||
                    _developerMenuOpen || DeveloperInputBlockedThisFrame || FreeplayOnboardingInputBlockedV414 ||
                    World.NotebookBlocksInput || World.CinematicActive || World.LossAnnounced || World.InputLocked ||
                    World.BagCameraTransitionActive || World.WorkActive || World.InventoryOpen || World.IsMoving ||
                    _helpVisible || SimulationSelectorOpen || !World.CanInteract)
                    return false;
                if (World.Model.Phase != PanicPhase.Preparing && World.Model.Phase != PanicPhase.Tutorial) return false;
                PanicSelectedTargetSnapshotV56 snapshot;
                return World.TryGetSelectedTargetSnapshotV56(out snapshot);
            }
        }

        void DrawSelectedTargetHudV56()
        {
            if (!SelectedTargetHudVisibleV56) return;
            var snapshot = SelectedTargetSnapshotV56;
            if (snapshot == null) return;
            if (selectedTargetTitleStyleV56 == null)
            {
                selectedTargetTitleStyleV56 = new GUIStyle(_heading) { wordWrap = true, alignment = TextAnchor.UpperLeft };
                selectedTargetBodyStyleV56 = new GUIStyle(_small) { wordWrap = true, alignment = TextAnchor.UpperLeft };
            }
            const float x = 1150f, width = 426f, inner = width - 36f;
            float titleHeight = Mathf.Max(35f, selectedTargetTitleStyleV56.CalcHeight(new GUIContent(snapshot.Label), inner));
            float hintHeight = string.IsNullOrWhiteSpace(snapshot.Hint) ? 0f :
                Mathf.Max(23f, selectedTargetBodyStyleV56.CalcHeight(new GUIContent(snapshot.Hint), inner));
            float detailHeight = string.IsNullOrWhiteSpace(snapshot.Detail) ? 0f :
                Mathf.Max(23f, selectedTargetBodyStyleV56.CalcHeight(new GUIContent(snapshot.Detail), inner));
            bool showWork = !string.IsNullOrEmpty(snapshot.Action);
            bool showTable = snapshot.Requirements.Count > 0 || showWork;
            int rowCount = snapshot.Requirements.Count + (showWork ? 1 : 0);
            string footer = showWork ? World.Model.FreeMaterials || World.Model.InfiniteBudget ?
                "Geliştirici seçenekleri maliyetlere uygulanır." : "İş bitince bir kez harcanır. İptal ücretsiz." : string.Empty;
            float footerHeight = footer.Length > 0 ? Mathf.Max(23f, selectedTargetBodyStyleV56.CalcHeight(new GUIContent(footer), inner)) : 0f;
            float height = 14f + 22f + titleHeight + (hintHeight > 0 ? hintHeight + 6f : 0f) +
                (detailHeight > 0 ? detailHeight + 6f : 0f) + (showTable ? 33f + rowCount * 33f : 0f) +
                (footerHeight > 0 ? footerHeight + 8f : 0f) + 14f;
            selectedTargetBoundsV56 = new Rect(x, 112f, width, height);
            Frame(selectedTargetBoundsV56, Night, snapshot.IsRoof ? Brass : Line, snapshot.IsRoof ? 2f : 1f);
            Region(selectedTargetBoundsV56);
            Label(new Rect(x + 18f, 124f, inner, 20f), snapshot.IsRoof ? "SEÇİLİ HEDEF · ÇATI" : "SEÇİLİ HEDEF", ColorText(_tiny, Brass));
            if (snapshot.IsRoof)
            {
                DrawLine(new Vector2(1516f, 139f), new Vector2(1536f, 124f), Brass, 3f);
                DrawLine(new Vector2(1536f, 124f), new Vector2(1556f, 139f), Brass, 3f);
            }
            float y = 148f;
            Label(new Rect(x + 18f, y, inner, titleHeight), snapshot.Label, selectedTargetTitleStyleV56);
            y += titleHeight;
            if (hintHeight > 0)
            { Label(new Rect(x + 18f, y, inner, hintHeight), snapshot.Hint, ColorText(selectedTargetBodyStyleV56, Brass)); y += hintHeight + 6f; }
            if (detailHeight > 0)
            { Label(new Rect(x + 18f, y, inner, detailHeight), snapshot.Detail, selectedTargetBodyStyleV56); y += detailHeight + 6f; }
            var rendered = new StringBuilder("SEÇİLİ HEDEF\n").Append(snapshot.Label).Append('\n')
                .Append(snapshot.Hint).Append('\n').Append(snapshot.Detail).Append('\n');
            if (showTable)
            {
                y += 7f;
                Fill(new Rect(x + 18f, y, inner, 1f), Line); y += 7f;
                Label(new Rect(x + 18f, y, 178f, 20f), snapshot.IsSupply ? "ÇANTADAKİ İHTİYAÇ" : "GEREKENLER", _tiny);
                Label(new Rect(x + 199f, y, 57f, 20f), "VAR", _caps);
                Label(new Rect(x + 257f, y, 57f, 20f), "GEREK", _caps);
                Label(new Rect(x + 315f, y, 92f, 20f), "EKSİK", ColorText(_caps, Brass));
                y += 23f;
                foreach (var requirement in snapshot.Requirements)
                {
                    DrawSelectedTargetCostV56(y, PanicExpedition.DisplayName(requirement.Id), requirement.Owned.ToString(),
                        requirement.Required.ToString(), requirement.Missing, rendered);
                    y += 33f;
                }
                if (showWork)
                {
                    DrawSelectedTargetCostV56(y, "Bütçe · $", World.Model.InfiniteBudget ? "∞" : snapshot.BudgetOwned.ToString("N0"),
                        snapshot.BudgetCost.ToString("N0"), snapshot.BudgetMissing, rendered);
                    y += 33f;
                }
            }
            if (footerHeight > 0) Label(new Rect(x + 18f, y + 7f, inner, footerHeight), footer, selectedTargetBodyStyleV56);
            rendered.Append(footer);
            SelectedTargetRenderedTextV56 = rendered.ToString();
            if (Event.current.type == EventType.Repaint && selectedTargetRepaintFrameV56 != Time.frameCount)
            { selectedTargetRepaintFrameV56 = Time.frameCount; SelectedTargetHudDrawFramesV56++; }
        }

        void DrawSelectedTargetCostV56(float y, string name, string owned, string required, int missing, StringBuilder rendered)
        {
            const float x = 1150f;
            FittedLabel(new Rect(x + 18f, y + 1f, 177f, 29f), name, _small, SoftText, 15);
            FittedLabel(new Rect(x + 199f, y + 1f, 57f, 29f), owned, _center, MainText, 14);
            FittedLabel(new Rect(x + 257f, y + 1f, 57f, 29f), required, _center, MainText, 14);
            FittedLabel(new Rect(x + 315f, y + 1f, 92f, 29f), missing.ToString(), _center, missing > 0 ? Brass : Sea, 14);
            rendered.Append(name).Append(" · Var ").Append(owned).Append(" / Gerek ").Append(required).Append(" / Eksik ").Append(missing).Append('\n');
        }
    }
}
