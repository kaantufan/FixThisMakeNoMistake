using System;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        enum BagViewMode { Isometric, Entering, FirstPerson, Leaving }
        BagViewMode bagViewMode;
        float bagViewAge, bagYaw, bagPitch, bagHandAge = 10, bagOpenAge = 10, bagHandClock;
        Vector3 bagShotFrom, bagShotTo, bagIsoOffset, bagTransitionActor;
        Quaternion bagRotationFrom, bagRotationTo, bagIsoRotation;
        float bagIsoSize, bagShotFov;
        Camera bagHandsCamera;
        Light bagHandsFill;
        GameObject bagHands;
        
        public bool BagFirstPersonActive { get { return bagViewMode == BagViewMode.FirstPerson; } }
        public bool BagCameraTransitionActive { get { return bagViewMode == BagViewMode.Entering || bagViewMode == BagViewMode.Leaving; } }
        public bool BagCameraOwnsView { get { return bagViewMode != BagViewMode.Isometric; } }
        public const float BagPreparationSeconds = PanicModel.NormalBagSeconds;
        public bool BagFinalStageActive { get { return Model != null && !Model.IsSimulation &&
            (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial) && Model.TimeRemaining <= BagPreparationSeconds &&
            (!JuryActive || JurySnapshot != null && JurySnapshot.Stage == PanicJuryStage.Preparation); } }
        public bool BagStagePhase { get { return BagFinalStageActive; } }

        // Called after the real clock and phase transition, before any world action.
        public void TickPreparationStages()
        {
            if (!BagFinalStageActive || BagCameraOwnsView || Model.IsPaused || NotebookBlocksInput || InputLocked || CinematicActive || LossAnnounced ||
                hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame)) return;
            CancelWork(); CancelMove(); InventoryOpen = false;
            if (hud) hud.BeginFinalBagStage();
            foreach (var source in Expedition.Sources)
            {
                source.Open = false;
                if (hinges.TryGetValue(source.Id, out var hinge)) hinge.open = false;
                if (sourceObjects.TryGetValue(source.Id, out var sourceRoot) && sourceRoot)
                    PanicCuratedAssetArt.SyncVisualState(sourceRoot, false, false, false, 10f);
                if (items.TryGetValue(source.Id, out var itemRoot) && itemRoot)
                    PanicCuratedAssetArt.SyncVisualState(itemRoot, false, false, false, 10f);
            }
            Expedition.ClearRequirements();
            BeginBagPreparation(); RefreshPreparationVisibility();
            Model.Notify(JuryActive ? "Son 40 saniye · suyunu al, işaretli güvenli alana geç." : "Son 40 saniye · erzaklar çantana girer; " + Model.RequiredShelterDestinationV5 + " git.");
        }
        public Vector3 BagTransitionActorPosition { get { return bagTransitionActor; } }
        public float BagCameraTransitionProgress { get { return Mathf.Clamp01(bagViewAge / (bagViewMode == BagViewMode.Entering ? BagHandoffDurationV51 : 1.1f)); } }

        void InitBagPreparation()
        {
            var root = new GameObject("Emergency bag hands overlay camera");
            bagHandsCamera = root.AddComponent<Camera>(); bagHandsCamera.clearFlags = CameraClearFlags.Depth;
            bagHandsCamera.cullingMask = 1 << 28; bagHandsCamera.depth = View.depth + 1;
            bagHandsCamera.fieldOfView = 62; bagHandsCamera.nearClipPlane = .015f; bagHandsCamera.farClipPlane = 1.6f;
            bagHandsCamera.enabled = false;
            bagHands = PanicBagHandsArt.Create(); bagHands.transform.SetParent(root.transform, false);
            var fill=new GameObject("Hands soft local fill");fill.transform.SetParent(root.transform,false);fill.transform.localPosition=new Vector3(-.35f,.3f,-.15f);
            bagHandsFill=fill.AddComponent<Light>();bagHandsFill.type=LightType.Point;bagHandsFill.cullingMask=1<<28;
            bagHandsFill.range=2;bagHandsFill.intensity=.85f;bagHandsFill.color=new Color(1f,.91f,.80f);bagHandsFill.shadows=LightShadows.None;bagHandsFill.enabled=false;
            foreach (var t in bagHands.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 28;
            foreach (var c in bagHands.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        }

        public bool BeginBagPreparation()
        {
            if (!BagStagePhase || BagCameraOwnsView || InputLocked || Model.IsPaused || NotebookBlocksInput || WorkActive || IsMoving || InventoryOpen || CinematicActive || LossAnnounced || hud && hud.NotebookModalBlocked) return false;
            // The actor and real controller never move during this camera handoff.
            bagTransitionActor = PlayerPosition;
            bagIsoOffset = View.transform.position - PlayerPosition; bagIsoRotation = View.transform.rotation; bagIsoSize = View.orthographicSize;
            bagShotFrom = View.transform.position; bagRotationFrom = View.transform.rotation;
            bagYaw = facingYaw; bagPitch = 3f;
            bagShotTo = PlayerPosition + Vector3.up * 1.62f; bagRotationTo = Quaternion.Euler(bagPitch, bagYaw, 0);
            float distance = Mathf.Max(1, Vector3.Distance(View.transform.position, PlayerPosition + Vector3.up));
            bagShotFov = Mathf.Clamp(2 * Mathf.Atan(bagIsoSize / distance) * Mathf.Rad2Deg, 15, 95);
            bagViewAge = 0; bagViewMode = BagViewMode.Entering;
            BeginBagHandoffDiagnosticsV51();
            desiredVelocity = smoothVelocity = Vector3.zero;
            SetFirstPersonPartitions(false); BeginBagDollyV56(); ClearPickupFocus();
            CancelFrame = DiscreteInputFrame = Time.frameCount;
            return true;
        }

        public bool FinishBagPreparation()
        {
            if (BagFinalStageActive || !BagFirstPersonActive || InventoryOpen || NotebookBlocksInput || Model.IsPaused) return false;
            bagTransitionActor = PlayerPosition;
            bagShotFrom = View.transform.position; bagRotationFrom = View.transform.rotation;
            bagShotTo = PlayerPosition + bagIsoOffset; bagRotationTo = bagIsoRotation;
            bagViewMode = BagViewMode.Leaving; bagViewAge = 0;
            desiredVelocity = smoothVelocity = Vector3.zero; ClearPickupFocus();
            CancelFrame = DiscreteInputFrame = Time.frameCount;
            UpdateBagCursor(); return true;
        }

        void ResetBagPreparation()
        {
            SuspendExperienceFeedbackV414();
            ResetBagDollyV56();
            fpsStressV414 = fpsStressClockV414 = 0; fpsPoseReadyV414 = false;
            bool owned = BagCameraOwnsView;
            bagViewMode = BagViewMode.Isometric; bagViewAge = 0; bagHandAge = 10;
            if (bagHandsCamera) bagHandsCamera.enabled = false;
            if(bagHandsFill)bagHandsFill.enabled=false;
            SetFirstPersonPartitions(false);
            if (owned && View)
            {
                View.orthographic = true; View.orthographicSize = bagIsoSize > 0 ? bagIsoSize : cameraSize;
                View.nearClipPlane = .08f;
                View.transform.SetPositionAndRotation(PlayerPosition + bagIsoOffset, bagIsoRotation);
            }
            if (owned) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }

        void ProcessBagPreparationInput(float dt)
        {
            if (BagCameraOwnsView && !BagStagePhase) { ResetBagPreparation(); return; }
            if (!BagFirstPersonActive || FullFlowScriptedInput) return;
            
            if (InputLocked || InventoryOpen || WorkActive || IsMoving || NotebookBlocksInput || hud.NotebookModalBlocked || !Application.isFocused) return;
            if (Cursor.lockState != CursorLockMode.Locked) return;
            StepBagLook(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")));
        }

        public void StepBagLook(Vector2 mouseDelta)
        {
            if (!BagFirstPersonActive || InputLocked || Model.IsPaused || InventoryOpen || NotebookBlocksInput) return;
            bagYaw += mouseDelta.x * 2f * FpsLookMultiplierV414;
            bagPitch = Mathf.Clamp(bagPitch - mouseDelta.y * 1.7f * FpsLookMultiplierV414, -74, 78);
            facingYaw = bagYaw;
            ApplyBagPoseV414();
        }

        void UpdateBagPreparationCamera(float dt)
        {
            if (BagCameraTransitionActive)
            {
                if (BagHandoffSuspendedV51) { UpdateBagCursor(); return; }
                float previousAge = bagViewAge;
                bagViewAge += float.IsNaN(dt) || float.IsInfinity(dt) ? 0 : Mathf.Clamp(dt, 0, .08f);
                float t = BagCameraTransitionProgress;
                float smooth = t * t * (3f - 2f * t);
                if (bagViewMode == BagViewMode.Entering)
                {
                    RecordBagHandoffStepV51(previousAge, bagViewAge);
                    ApplyBagDollyPoseV56(t);
                }
                else
                {
                    View.transform.SetPositionAndRotation(Vector3.Lerp(bagShotFrom, bagShotTo, smooth), Quaternion.Slerp(bagRotationFrom, bagRotationTo, smooth));
                    View.fieldOfView = Mathf.Lerp(62, bagShotFov, smooth);
                }
                if (t >= 1)
                {
                    if (bagViewMode == BagViewMode.Entering) { bagViewMode = BagViewMode.FirstPerson; View.fieldOfView = 62; RefreshBagRayFocus(); }
                    else { ResetBagPreparation(); ApplyFloorVisibility(); }
                    CancelFrame = DiscreteInputFrame = Time.frameCount;
                }
            }
            else if (BagFirstPersonActive)
            {
                ApplyBagPoseV414();
            }
            if (bagHandsCamera)
            {
                bool visible = BagFirstPersonActive && !InventoryOpen && !NotebookBlocksInput && !Model.IsPaused && !hud.DeveloperMenuOpen;
                bagHandsCamera.enabled = visible;
                if(bagHandsFill)bagHandsFill.enabled=visible;
                bagHandsCamera.transform.SetPositionAndRotation(View.transform.position, View.transform.rotation);
                bagHandsCamera.fieldOfView = View.fieldOfView;
                if (visible)
                {
                    bagHandClock += dt; bagHandAge += dt; bagOpenAge += dt;
                    float gather = bagHandAge < .65f ? Mathf.Clamp01(bagHandAge / .65f) : 0;
                    float open = bagOpenAge < .55f ? Mathf.Clamp01(bagOpenAge / .55f) : 0;
                    PanicBagHandsArt.SetLocomotion(bagHands, desiredVelocity.magnitude, controller.isGrounded, gravitySpeed, dt);
                    PanicBagHandsArt.Animate(bagHands, bagHandClock, Mathf.Clamp01(desiredVelocity.magnitude / RunSpeed), gather, open);
                }
            }
            UpdateBagCursor();
        }

        void BeginBagHandCue() { bagHandAge = 0; }

        void InteractBagStage()
        {
            if (Model.IsPaused || NotebookBlocksInput || InventoryOpen || InputLocked) return;
            if (TryJuryInteract()) return;
            RefreshBagRayFocus();
            if (Expedition.FindWorldSupply(focusId) != null) { TakeWorldSupply(); return; }
            if (!CanInteract) return;
            if (focusId == "@bunker" || focusId == "bunker" || focusId=="@panicroom") { TryEnterActiveShelterV5(); return; }
            if (focusId != null && hinges.TryGetValue(focusId,out var hinge) && hinge.directionalDoor) { bagOpenAge=0;ToggleDoor();return; }
            if (focusId == "@garage") { if(Model.UtilitySafety.PowerOff){Notice("Ana şalter kapalı; garajın iç kapısını kullan.");return;}garageOpen=!garageOpen;bagOpenAge=0;effects.Play("garage",.45f);return; }
            Notice("Erzaklar otomatik çantana girer; ardından "+Model.RequiredShelterDestinationV5+" git. Tab · Çanta");
        }

        void UpdateBagCursor()
        {
            if (!BagCameraOwnsView) return;
            bool locked = BagFirstPersonActive && Application.isFocused && !InputLocked && !Model.IsPaused && !NotebookBlocksInput && !InventoryOpen && !hud.NotebookModalBlocked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
            if ((!BagFirstPersonActive || InputLocked || Model.IsPaused || NotebookBlocksInput || InventoryOpen || hud.NotebookModalBlocked) && bagHandsCamera)
            {bagHandsCamera.enabled = false;if(bagHandsFill)bagHandsFill.enabled=false;}
        }

        void RefreshBagRayFocus()
        {
            focusCandidates.Clear();
            if(BagFirstPersonActive)ApplyBagPoseV414();
            string target = BagRayTarget();
            if (target != null) focusCandidates.Add(target);
            if (focusId != target) SetFocus(target); else UpdateContext();
        }

        string BagRayTarget()
        {
            // Fresh eye ray is also the FPS line-of-sight authority: a torso ray can hit
            // the front of a counter while its loose supplies are visibly reachable above it.
            Physics.SyncTransforms();
            var hits = Physics.RaycastAll(View.ViewportPointToRay(new Vector3(.5f, .5f)), 2.4f, ~((1 << 28) | (1 << 30)), QueryTriggerInteraction.Collide);
            Array.Sort(hits, (a,b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider == controller || hit.transform.IsChildOf(player)) continue;
                var pick = hit.collider.GetComponentInParent<HomePick>();
                if (pick && PositionOf(pick.Id, out var p) && Mathf.Abs(p.y - PlayerPosition.y) < 2.2f)
                {
                    bool supply = Expedition.FindWorldSupply(pick.Id) != null;
                    bool door = hinges.TryGetValue(pick.Id, out var h) && h.directionalDoor;
                    if (supply || door || pick.Id == "@garage" || pick.Id == "@bunker" || pick.Id == "bunker" || pick.Id == "@panicroom") return pick.Id;
                }
                // The first real wall/object stops the ray. There is no through-wall search.
                return null;
            }
            return null;
        }

        // The scaled HUD owns all FPS text and controls; world draws no duplicate panels.
        void DrawBagPreparationOverlay() { }
    }
}
