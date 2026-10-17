using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        Rect manualWorkViewportV414;
        bool manualWorkDraggingV414, manualWorkAngleValidV414;
        int manualWorkTargetV414 = -1;
        float manualWorkAngleV414;
        public void SetManualWorkViewportV414(Rect logicalFittedTexture)
        { manualWorkViewportV414 = logicalFittedTexture; }
        public void ResetManualWorkPointerV414()
        { manualWorkDraggingV414 = manualWorkAngleValidV414 = false; manualWorkTargetV414 = -1; }

        /// <summary>Actual projected head/handle center, in the same logical HUD space as the work texture.</summary>
        public bool ManualWorkCenterV414(int index, out Vector2 center)
        {
            center = Vector2.zero;
            if (!WorkActive || Expedition.Work.IsWorldConstruction || manualWorkViewportV414.width <= 0f) return false;
            Vector3 point = WorkTargetViewport(index);
            if (point.z <= 0 || point.x < 0 || point.x > 1 || point.y < 0 || point.y > 1) return false;
            center = new Vector2(manualWorkViewportV414.x + point.x * manualWorkViewportV414.width,
                manualWorkViewportV414.y + (1f - point.y) * manualWorkViewportV414.height);
            return true;
        }
        public Rect ManualWorkTargetBoundsV414(int index)
        { Vector2 center; return ManualWorkCenterV414(index, out center) ? new Rect(center.x - 44, center.y - 44, 88, 88) : new Rect(); }

        /// <summary>Call instead of the former A/D/Enter/Space UpdateWork body. Never called from IMGUI events.</summary>
        void UpdateManualWorkV414(float dt)
        {
            if (!WorkActive) return;
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
            { ResetManualWorkPointerV414(); ProcessWorkInput(dt, 0, false, false, false, true); return; }
            bool blocked = InputLocked || CinematicActive || LossAnnounced || Model.IsPaused || NotebookBlocksInput ||
                CancelFrame == Time.frameCount || DiscreteInputFrame == Time.frameCount ||
                hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame);
            if (blocked) { ResetManualWorkPointerV414(); return; }
            PanicWorkSession work = Expedition.Work;
            if (work.UsesRotation && hud)
            {
                Vector2 pointer = (new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y) - hud.CanvasOffset) / hud.CanvasScale;
                int index = work.NextClickTarget;
                Vector2 center;
                bool hasCenter = ManualWorkCenterV414(index, out center);
                if (Input.GetMouseButtonDown(0))
                {
                    ResetManualWorkPointerV414();
                    if (hasCenter && ManualWorkTargetBoundsV414(index).Contains(pointer) && ClickWorkTarget(index))
                    { manualWorkDraggingV414 = true; manualWorkTargetV414 = index; }
                }
                if (!Input.GetMouseButton(0) || index != manualWorkTargetV414 || !hasCenter)
                    ResetManualWorkPointerV414();
                else if (manualWorkDraggingV414)
                {
                    Vector2 delta = pointer - center;
                    // Avoid an unstable angle at the exact pivot; allow a generous drag
                    // radius. Resuming/re-entering rebases without gifting a turn.
                    if (delta.sqrMagnitude < 12f * 12f || delta.sqrMagnitude > 210f * 210f || !manualWorkViewportV414.Contains(pointer))
                        manualWorkAngleValidV414 = false;
                    else
                    {
                        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                        if (manualWorkAngleValidV414) work.ApplyRotation(Mathf.DeltaAngle(manualWorkAngleV414, angle), true);
                        manualWorkAngleV414 = angle; manualWorkAngleValidV414 = true;
                    }
                }
            }
            // GetKeyDown counts real releases/represses: holding Space cannot run the job.
            StepWork(dt, 0, false, false, work.UsesHammer && Input.GetKeyDown(KeyCode.Space));
        }
    }
}
