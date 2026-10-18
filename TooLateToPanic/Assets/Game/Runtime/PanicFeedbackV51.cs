using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        public int VisibleGardenMarkersV51
        {
            get { int count=0; foreach(var site in gardenSites) if(site.marker && site.marker.activeInHierarchy) count++; return count; }
        }
        public bool CarriedBagInvisibleV51 { get { return !items.ContainsKey("bagstation") && !residenceTargets.ContainsKey("@bagstation") && !cinematicBagV414; } }
        public bool PrimaryInteractionVisibleV51
        {
            get { return View && Model != null && !Model.IsPaused && !BagCameraOwnsView && !LootVisible && !WorkActive && !IsMoving && !InventoryOpen && !NotebookBlocksInput && !CinematicActive && !LossAnnounced && !InputLocked && !(hud && hud.DeveloperMenuOpen) && CanInteract && !string.IsNullOrEmpty(ShortAction); }
        }
        void RefreshGardenMarkersV51()
        {
            bool outside = player && !InResidence(PlayerPosition) && PlayerPosition.y > -.6f && Model != null &&
                (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial) &&
                !BagCameraOwnsView && !CinematicActive && !LossAnnounced && !NotebookBlocksInput;
            foreach(var site in gardenSites) if(site.marker) site.marker.SetActive(outside && !site.done);
        }
        void DrawPrimaryInteractionV51()
        {
            if(!PrimaryInteractionVisibleV51 || !FocusPosition(out var position)) return;
            var target=InteractionObject(focusId);
            if(target)
            {
                bool any=false; Bounds bounds=new Bounds();
                foreach(var renderer in target.GetComponentsInChildren<Renderer>())
                {
                    if(!renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                    if(!any) { bounds=renderer.bounds; any=true; } else bounds.Encapsulate(renderer.bounds);
                }
                if(any && bounds.size.x<5 && bounds.size.z<5) position=new Vector3(bounds.center.x,bounds.max.y+.14f,bounds.center.z);
                else position += Vector3.up*1.2f;
            }
            else position+=Vector3.up*.65f;
            DrawMarker(position,"E  "+ShortAction,112);
        }
    }
}
