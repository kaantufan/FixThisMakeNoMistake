using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        PanicSelectionSilhouette interactionSilhouette;
        readonly List<Renderer> interactionMeshes = new List<Renderer>();
        readonly List<Renderer> gatherAttentionMeshes = new List<Renderer>();

        void ClearInteractionOutline()
        {
            if (interactionSilhouette) { interactionSilhouette.ClearSelection(); interactionSilhouette.ClearGather(); }
        }

        void UpdateInteractionOutline()
        {
            UpdateGatherAttention();
            bool visible = View && FocusPosition(out var p) && CanInteract && !LootVisible && !CinematicActive && !LossAnnounced && !InventoryOpen && !WorkActive && !InputLocked && !NotebookBlocksInput && !(hud && hud.DeveloperMenuOpen) &&
                (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial || Model.Phase == PanicPhase.Inspection);
            // Placement highlights the actual ghost; there is no footprint or bounds fallback.
            if (IsMoving) visible = View && ghost && !InputLocked && !InventoryOpen && !CinematicActive;
            if (!visible) { if (interactionSilhouette) interactionSilhouette.ClearSelection(); return; }
            interactionMeshes.Clear();
            if (IsMoving) AddInteractionMeshes(ghost);
            else if (!string.IsNullOrEmpty(focusId) && wallInteractionColliders.TryGetValue(focusId, out var segments))
            {
                // Collision proxies are siblings of art. Match the exact authored segment,
                // never take all children of the floor/wall parent.
                Renderer nearest = null; float distance = float.MaxValue;
                foreach (var segment in segments)
                {
                    if (!segment || !segment.enabled || !segment.transform.parent) continue;
                    string meshName = segment.name.StartsWith("Collision_", System.StringComparison.Ordinal) ? segment.name.Substring(10) : segment.name;
                    foreach (var mesh in segment.transform.parent.GetComponentsInChildren<MeshFilter>(true))
                    {
                        if (mesh.name != meshName || !mesh.sharedMesh || (mesh.transform.TransformPoint(mesh.sharedMesh.bounds.center) - segment.transform.position).sqrMagnitude > .0001f) continue;
                        var renderer = mesh.GetComponent<Renderer>();
                        if (!SelectionMeshVisible(renderer)) continue;
                        float candidate = (segment.ClosestPoint(player.position + Vector3.up) - player.position - Vector3.up).sqrMagnitude;
                        if (candidate < distance) { distance = candidate; nearest = renderer; }
                    }
                }
                if (nearest) interactionMeshes.Add(nearest);
            }
            else AddInteractionMeshes(InteractionObject(focusId));
            if (interactionMeshes.Count == 0) { if (interactionSilhouette) interactionSilhouette.ClearSelection(); return; }
            if (!interactionSilhouette) interactionSilhouette = View.GetComponent<PanicSelectionSilhouette>() ?? View.gameObject.AddComponent<PanicSelectionSilhouette>();
            interactionSilhouette.World = this;
            interactionSilhouette.SetSelection(interactionMeshes, IsMoving ? (placementValid ? new Color(.36f, .85f, .6f) : new Color(.98f, .36f, .25f)) : PresentationOrange);
        }

        // Physical pickup ownership and phase/count visibility remain in UpdateWorldSupplies.
        // Containers are one-time material auto-loot, not loose gather props, so they get no glint.
        void UpdateGatherAttention()
        {
            gatherAttentionMeshes.Clear();
            bool active = View && Model != null && BagFinalStageActive && !Model.IsPaused && !InputLocked && !NotebookBlocksInput && !CinematicActive && !LossAnnounced && !InventoryOpen && !WorkActive && !IsMoving && !(hud && hud.DeveloperMenuOpen) &&
                (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial);
            if (active) foreach (var supply in Expedition.WorldSupplies)
            {
                if (supply.Count <= 0 || !worldSupplyObjects.TryGetValue(supply.Id, out var root) || !root || !root.activeInHierarchy) continue;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    if (SelectionMeshVisible(renderer)) gatherAttentionMeshes.Add(renderer);
            }
            if (gatherAttentionMeshes.Count == 0) { if (interactionSilhouette) interactionSilhouette.ClearGather(); return; }
            if (!interactionSilhouette) interactionSilhouette = View.GetComponent<PanicSelectionSilhouette>() ?? View.gameObject.AddComponent<PanicSelectionSilhouette>();
            interactionSilhouette.World = this;
            interactionSilhouette.SetGather(gatherAttentionMeshes);
        }
        bool SelectionMeshVisible(Renderer renderer)
        {
            return renderer && renderer.enabled && renderer.gameObject.activeInHierarchy && !(renderer is LineRenderer) && !(renderer is ParticleSystemRenderer) &&
                (renderer is MeshRenderer || renderer is SkinnedMeshRenderer) && (View.cullingMask & (1 << renderer.gameObject.layer)) != 0;
        }

        void AddInteractionMeshes(GameObject root)
        {
            if (!root) return;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                if (SelectionMeshVisible(renderer) && !interactionMeshes.Contains(renderer)) interactionMeshes.Add(renderer);
        }
    }
}
