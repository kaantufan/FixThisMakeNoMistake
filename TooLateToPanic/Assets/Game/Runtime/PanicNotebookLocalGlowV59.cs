using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public static class PanicNotebookLocalGlowPolicyV59
    {
        public const float MaximumDistance = 8f;
        // Pure local policy. Existing HUD modal and source/supply stage policy is applied separately.
        public static bool CanShow(bool inside, int activeFloor, int targetFloor, float distanceSquared)
        {
            return inside && activeFloor == targetFloor && !float.IsNaN(distanceSquared) &&
                !float.IsInfinity(distanceSquared) && distanceSquared >= 0f &&
                distanceSquared <= MaximumDistance * MaximumDistance;
        }
    }

    public sealed partial class PanicHomeWorld
    {
        readonly List<Renderer> notebookGlowMeshesV59 = new List<Renderer>();
        readonly Plane[] notebookGlowFrustumV59 = new Plane[6];
        public bool NotebookGlowActiveV59 { get; private set; }
        public string NotebookGlowTargetIdV59 { get; private set; } = string.Empty;
        public int NotebookGlowRendererCountV59 { get { return notebookGlowMeshesV59.Count; } }
        public float NotebookGlowStrengthV59 { get; private set; }

        // Read-only presentation query: never selects a target, alters focus, or moves the actor.
        // True means eligible visible mesh geometry, not a claim that an occluded GPU pixel was drawn.
        public bool TryGetNotebookGlowStateV59(out string id, out Bounds bounds, out int floor)
        {
            id = string.Empty; bounds = new Bounds(); floor = 0;
            if (FirstWaveOnboardingRequiredV510) return TryGetFirstWaveLocalGlowV510(out id, out bounds, out floor);
            if (!isActiveAndEnabled || !player || !View || !View.isActiveAndEnabled ||
                !hud || !hud.NotebookCursorVisibleV59 ||
                !TryGetNotebookCursorTargetV59(out id, out var label, out bounds, out floor)) return false;
            // The same residence footprint used by actual cutaway visibility includes the attached garage.
            Vector3 target = NotebookTargetPositionV57;
            if (!PanicNotebookLocalGlowPolicyV59.CanShow(InResidence(player.position) && InResidence(target),
                ActiveFloor, floor, (target - player.position).sqrMagnitude)) return false;
            Vector3 projected = View.WorldToViewportPoint(bounds.center);
            if (projected.z <= View.nearClipPlane || float.IsNaN(projected.x) || float.IsInfinity(projected.x) ||
                float.IsNaN(projected.y) || float.IsInfinity(projected.y)) return false;
            GeometryUtility.CalculateFrustumPlanes(View, notebookGlowFrustumV59);
            return GeometryUtility.TestPlanesAABB(notebookGlowFrustumV59, bounds);
        }

        // First line of World.LateUpdate, before all existing early returns. Also refreshed by camera
        // OnPreCull so pause/modal/disabled-world transitions cannot leave an old command behind.
        public void TickNotebookLocalGlowV59()
        {
            notebookGlowMeshesV59.Clear(); NotebookGlowActiveV59 = false;
            NotebookGlowTargetIdV59 = string.Empty; NotebookGlowStrengthV59 = 0f;
            if (TryGetNotebookGlowStateV59(out var id, out var bounds, out var floor))
            {
                GameObject root = InteractionObject(id);
                if (root) foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (!SelectionMeshVisible(renderer) || renderer.GetComponent<PanicPermanentlyHiddenV414>() ||
                        !GeometryUtility.TestPlanesAABB(notebookGlowFrustumV59, renderer.bounds)) continue;
                    Mesh mesh = renderer is SkinnedMeshRenderer skin ? skin.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
                    if (mesh && mesh.subMeshCount > 0) notebookGlowMeshesV59.Add(renderer);
                }
                if (notebookGlowMeshesV59.Count > 0)
                {
                    bool reduced = ReducedFpsMotionV54 || Notebook && Notebook.Animation.ReducedMotion;
                    NotebookGlowStrengthV59 = reduced ? .85f : .85f + .15f * Mathf.Sin(Time.unscaledTime * 2.1f);
                    NotebookGlowTargetIdV59 = id; NotebookGlowActiveV59 = true;
                }
            }
            if (!NotebookGlowActiveV59)
            {
                if (interactionSilhouette) interactionSilhouette.ClearNotebookGlowV59();
                return;
            }
            if (!interactionSilhouette) interactionSilhouette = View.GetComponent<PanicSelectionSilhouette>() ?? View.gameObject.AddComponent<PanicSelectionSilhouette>();
            interactionSilhouette.World = this;
            interactionSilhouette.SetNotebookGlowV59(notebookGlowMeshesV59, new Color(1f, .67f, .28f, NotebookGlowStrengthV59));
        }
    }
}
