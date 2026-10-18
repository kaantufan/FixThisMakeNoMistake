using System;
using UnityEngine;

namespace TooLateToPanic
{
    // Cosmetic notebook navigation. Interaction focus and all production reach gates stay independent.
    public sealed partial class PanicHomeWorld
    {
        string notebookTargetIdV57, notebookTargetLabelV57;
        Vector3 notebookTargetPositionV57;
        int notebookTargetWaveV57 = -1;

        public bool HasNotebookTargetV57 { get { return RefreshNotebookTargetV57(); } }
        public string NotebookTargetIdV57 { get { return HasNotebookTargetV57 ? notebookTargetIdV57 : string.Empty; } }
        public string NotebookTargetLabelV57 { get { return HasNotebookTargetV57 ? notebookTargetLabelV57 : string.Empty; } }
        public Vector3 NotebookTargetPositionV57 { get { return HasNotebookTargetV57 ? notebookTargetPositionV57 : Vector3.zero; } }
        public string NotebookTargetHintV57
        {
            get
            {
                if (!HasNotebookTargetV57) return string.Empty;
                int meters = Mathf.CeilToInt(Vector3.Distance(PlayerPosition, notebookTargetPositionV57));
                int floor = Mathf.Clamp(Mathf.RoundToInt(notebookTargetPositionV57.y / 3.2f), -1, 1);
                string checkpoint = GuidanceObjectiveIdV54 == notebookTargetIdV57 && GuidanceRouteValidV54 ? GuidanceCheckpointLabelV54 :
                    GuidancePlanningV54 ? "Yol aranıyor" : "Açık yolu kontrol et";
                return PanicNotebookData.FloorName(floor) + " · " + meters + " m · " + checkpoint + " · J: defter / işareti kaldır";
            }
        }

        public bool TryMarkNotebookTargetV57(string id)
        {
            if (Model == null || (Model.Phase != PanicPhase.Preparing && Model.Phase != PanicPhase.Tutorial && Model.Phase != PanicPhase.Inspection)) return false;
            if (!TryResolveNotebookTargetV57(id, out var label, out var position)) return false;
            notebookTargetIdV57 = id; notebookTargetLabelV57 = label;
            notebookTargetPositionV57 = position; notebookTargetWaveV57 = Model.Wave;
            InvalidateNotebookTargetRouteV57();
            return true;
        }

        public void ClearNotebookTargetV57()
        {
            notebookTargetIdV57 = notebookTargetLabelV57 = null;
            notebookTargetPositionV57 = Vector3.zero; notebookTargetWaveV57 = -1;
            InvalidateNotebookTargetRouteV57();
        }

        bool RefreshNotebookTargetV57()
        {
            if (string.IsNullOrEmpty(notebookTargetIdV57)) return false;
            if (Model == null || Model.Phase == PanicPhase.Title || Model.Wave != notebookTargetWaveV57 ||
                !TryResolveNotebookTargetV57(notebookTargetIdV57, out var label, out var position))
            { ClearNotebookTargetV57(); return false; }
            notebookTargetLabelV57 = label; notebookTargetPositionV57 = position;
            return true;
        }

        bool TryResolveNotebookTargetV57(string id, out string label, out Vector3 position)
        {
            label = string.Empty; position = Vector3.zero;
            if (string.IsNullOrEmpty(id) || Expedition == null || !InteractionObject(id)) return false;
            var source = Expedition.Sources.Find(s => s != null && s.Id == id);
            if (source != null) label = source.Label;
            else
            {
                var supply = Expedition.FindWorldSupply(id);
                if (supply == null || supply.Count <= 0) return false;
                label = PanicExpedition.DisplayName(supply.ItemId);
            }
            if (!PositionOf(id, out position) || float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z) ||
                float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z)) return false;
            if (string.IsNullOrWhiteSpace(label)) label = id;
            return true;
        }

        void InvalidateNotebookTargetRouteV57()
        {
            // Read-only planner cache only: never focus, move, open, collect or spend.
            guidanceStateV54 = null; guidanceNextPlanV54 = 0; guidancePlannerV54 = null;
            guidanceRouteV54.Clear(); GuidanceRouteValidV54 = false;
            GuidanceObjectiveIdV54 = GuidanceObjectiveActionV54 = GuidanceCheckpointLabelV54 = string.Empty;
        }
    }
}
