using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Read-only presentation policy. The expedition remains the sole stock owner.</summary>
    public static class PanicLootSourceShimmerPolicyV510
    {
        public static bool HasUncollectedStock(LootSource source)
        {
            if (source == null || string.IsNullOrEmpty(source.Id) || source.AutoLooted || source.Contents == null) return false;
            foreach (var slot in source.Contents)
                if (slot != null && slot.Count > 0 && !string.IsNullOrEmpty(slot.Id)) return true;
            return false;
        }

        public static bool CanShow(PanicPhase phase, bool bagFinalStage, bool blocked)
        {
            return !blocked && !bagFinalStage && (phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial);
        }
    }

    public sealed partial class PanicHomeWorld
    {
        PanicLootSourceShimmerPassV510 lootSourceShimmerV510;
        readonly List<Renderer> lootShimmerScratchV510 = new List<Renderer>(64);
        readonly HashSet<Renderer> lootShimmerUniqueV510 = new HashSet<Renderer>();
        readonly Plane[] lootShimmerFrustumV510 = new Plane[6];

        public int LootShimmerEligibleSourceCountV510 { get; private set; }
        public int LootShimmerVisibleSourceCountV510 { get; private set; }
        public int LootShimmerRendererCountV510 { get { return lootSourceShimmerV510 ? lootSourceShimmerV510.RendererCount : 0; } }
        public int LootShimmerMeshDrawCountV510 { get { return lootSourceShimmerV510 ? lootSourceShimmerV510.MeshDrawCount : 0; } }
        public int LootShimmerDrawFrameV510 { get { return lootSourceShimmerV510 ? lootSourceShimmerV510.DrawFrame : -1; } }
        public bool LootShimmerPassReadyV510 { get { return lootSourceShimmerV510 && lootSourceShimmerV510.PassReady; } }
        public bool LootShimmerReducedMotionV510 { get { return ReducedFpsMotionV54 || Notebook && Notebook.Animation.ReducedMotion; } }

        // Root hook: first line of LateUpdate, before early returns. No per-source setup or stock event hook.
        public void TickLootSourceShimmerV510()
        {
            if (!View)
            {
                if (lootSourceShimmerV510) lootSourceShimmerV510.BindWorld(null);
                lootSourceShimmerV510 = null;
                return;
            }
            if (lootSourceShimmerV510 && lootSourceShimmerV510.gameObject != View.gameObject)
                lootSourceShimmerV510.BindWorld(null);
            if (!lootSourceShimmerV510 || lootSourceShimmerV510.gameObject != View.gameObject)
                lootSourceShimmerV510 = View.GetComponent<PanicLootSourceShimmerPassV510>() ?? View.gameObject.AddComponent<PanicLootSourceShimmerPassV510>();
            lootSourceShimmerV510.BindWorld(this);
        }

        public bool LootSourceStockedForShimmerV510(string id)
        {
            if (Expedition == null || Expedition.Sources == null || string.IsNullOrEmpty(id)) return false;
            foreach (var source in Expedition.Sources)
                if (source != null && source.Id == id) return PanicLootSourceShimmerPolicyV510.HasUncollectedStock(source);
            return false;
        }

        // Logical eligibility, including phase/modal gates; no view, range, focus or renderer requirement.
        public bool IsLootSourceGlowEligibleV510(string id)
        {
            return LootSourceShimmerPresentationActiveV510() && LootSourceStockedForShimmerV510(id);
        }

        bool LootSourceShimmerPresentationActiveV510()
        {
            if (!isActiveAndEnabled || Model == null || Expedition == null) return false;
            bool blocked = Model.IsPaused || Model.WaveLedgerOpenV56 || Model.KeyboardHelpOpenV56 ||
                NotebookBlocksInput || InputLocked || CinematicActive || LossAnnounced || InventoryOpen ||
                WorkActive || IsMoving || hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame);
            return PanicLootSourceShimmerPolicyV510.CanShow(Model.Phase, BagFinalStageActive, blocked);
        }

        // Camera-time refresh prevents stale commands after loot, a modal change, disable, or restock.
        // Eligibility is all stocked sources: never the current target, distance, notebook pin or RNG.
        internal void CollectLootSourceShimmerMeshesV510(Camera camera, List<Renderer> output, List<string> sourceIds)
        {
            output.Clear(); sourceIds.Clear(); lootShimmerUniqueV510.Clear();
            LootShimmerEligibleSourceCountV510 = LootShimmerVisibleSourceCountV510 = 0;
            if (!isActiveAndEnabled || !camera || camera != View || !camera.isActiveAndEnabled ||
                Model == null || Expedition == null || Expedition.Sources == null) return;
            if (!LootSourceShimmerPresentationActiveV510()) return;

            GeometryUtility.CalculateFrustumPlanes(camera, lootShimmerFrustumV510);
            foreach (var source in Expedition.Sources)
            {
                if (!PanicLootSourceShimmerPolicyV510.HasUncollectedStock(source)) continue;
                LootShimmerEligibleSourceCountV510++;
                GameObject root = InteractionObject(source.Id);
                if (!root || !root.activeInHierarchy) continue;
                lootShimmerScratchV510.Clear();
                root.GetComponentsInChildren<Renderer>(true, lootShimmerScratchV510);
                int before = output.Count;
                foreach (var renderer in lootShimmerScratchV510)
                {
                    if (!SelectionMeshVisible(renderer) || renderer.forceRenderingOff ||
                        renderer.GetComponent<PanicPermanentlyHiddenV414>() ||
                        !GeometryUtility.TestPlanesAABB(lootShimmerFrustumV510, renderer.bounds)) continue;
                    Mesh mesh = renderer is SkinnedMeshRenderer skin ? skin.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
                    if (mesh && mesh.subMeshCount > 0 && lootShimmerUniqueV510.Add(renderer)) output.Add(renderer);
                }
                if (output.Count <= before) continue;
                LootShimmerVisibleSourceCountV510++;
                sourceIds.Add(source.Id);
            }
        }
    }
}
