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
            if (isRoof) return false;
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
                    World.BagCameraTransitionActive || World.BagFirstPersonActive || World.WorkActive || World.InventoryOpen || World.IsMoving ||
                    _helpVisible || SimulationSelectorOpen || !World.CanInteract)
                    return false;
                if (World.Model.Phase != PanicPhase.Preparing && World.Model.Phase != PanicPhase.Tutorial) return false;
                PanicSelectedTargetSnapshotV56 snapshot;
                return World.TryGetSelectedTargetSnapshotV56(out snapshot);
            }
        }

        void DrawSelectedTargetHudV56()
        {
            DrawSelectedTargetDesignV57();
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
