using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Residence camera sequences. This partial only changes presentation: it never
    /// advances or resolves the model and never changes the result of a run.
    /// </summary>
    public sealed partial class PanicHomeWorld
    {
        const float ResidenceLossRevealSeconds = 1.35f;

        public bool CinematicActive { get; private set; }
        public string CinematicTitle { get; private set; }
        public float CinematicProgress { get; private set; }
        public bool LossAnnounced { get; private set; }
        public float LossRevealTime { get; private set; }
        public string CinematicKind { get; private set; }
        public bool CinematicShowsActor { get; private set; }

        float residenceCinematicAge;
        float residenceCinematicDuration;
        float residenceLossClock;
        string residencePendingLossKind;
        bool residenceCinematicSession;
        bool residenceCameraCutPending;
        bool residenceSavedControllerEnabled;
        bool residenceSavedWorking;
        bool residenceSavedKneeling;
        bool residenceSavedCameraOrthographic;
        bool residenceHasAnimatorPose;
        float residenceSavedOrthoSize;
        float residenceSavedFieldOfView;
        Vector3 residenceReturnPosition;
        Quaternion residenceReturnRotation;
        Vector3 residenceSavedCameraPosition;
        Quaternion residenceSavedCameraRotation;
        Vector3 residenceCameraVelocity;
        GameObject residenceFailureCrate;
        GameObject residenceFailureLampObject;
        Transform residenceSavedSupplyDisplay;
        bool residenceSavedSupplyDisplayActive;
        bool residenceSkipInProgress;
        int residenceShotIndex = -1;
        bool ResidenceCinematicExteriorView { get { return CinematicActive && (CinematicKind == "Aftermath" || CinematicKind == "DamageFailure" || CinematicKind == "Opening" && residenceCinematicAge < 3f || CinematicKind == "Disaster" && (Model.IsSimulation || residenceCinematicAge >= 4.7f) || CinematicKind == "Deadline" && residenceCinematicAge >= 2.15f && residenceCinematicAge < 4.15f); } }

        /// <summary>Skip presentation through the same model transitions; never invent an outcome.</summary>
        public void SkipResidenceCinematic()
        {
            if (residenceSkipInProgress) return;
            if (IntroStoryActiveV414) { SkipIntroStoryV414(); return; }
            if (JuryActive)
            {
                if (JuryDemo != null && JuryDemo.SkipCinematic()) RefreshJuryPresentation();
                return;
            }
            if (!CinematicActive) return;
            residenceSkipInProgress = true;
            PanicPhase skippedFromPhase = Model.Phase;
            try
            {
                // Resolve once via the ordinary guarded model path, then stop on real inspection/loss.
                for (int guard = 0; guard < 5; guard++)
                {
                    PanicPhase phase = Model.Phase;
                    if (phase != PanicPhase.Disaster && phase != PanicPhase.Aftermath && phase != PanicPhase.Success && phase != PanicPhase.GameOver) break;
                    if (phase == PanicPhase.GameOver && Model.PhaseTime <= 0f) break;
                    Model.Tick(Mathf.Max(.001f, Model.PhaseTime));
                }
                Expedition.SyncConsumedBagSupplies();
                ResetResidenceCinematic();
                OnPresentationPhase(Model.Phase, skippedFromPhase);
                CancelWork(); CancelMove(); InventoryOpen = false;
                SyncVisuals(true);
                if (effects) effects.OnPhase(Model.Phase);
                previousPhase = Model.Phase;
                UpdateContext();
            }
            finally { residenceSkipInProgress = false; }
        }

        void BeginResidenceCinematic(PanicPhase to, PanicPhase from)
        {
            if (to == PanicPhase.Title || to == PanicPhase.Tutorial ||
                to == PanicPhase.Preparing || to == PanicPhase.Inspection)
            {
                ResetResidenceCinematic();
                return;
            }

            if (to == PanicPhase.GameOver)
            {
                BeginResidenceLossReveal(from);
                return;
            }

            if (to == PanicPhase.Disaster)
            {
                BeginResidenceSession();
                SetResidenceCinematic("Disaster", "Afet başladı", 8f);
                cinematicActorStart = player ? player.position : residenceReturnPosition;
                cinematicHatchApproach = BunkerPosition + new Vector3(0f, .04f, -1.18f);
                cinematicActorEnd = BunkerInteriorPosition + new Vector3(0f, .04f, -.42f);
                CinematicShowsActor = true;
                SetResidenceActorFlags(false, false, 0f);
                if (!Model.IsSimulation) BeginBunkerSequenceV414();
                if (effects) effects.StartCinematicWeather();
                if (Model.IsSimulation) { CinematicShowsActor=false;SetBunkerDoorOpen(false); }
                return;
            }

            if (to == PanicPhase.Aftermath)
            {
                BeginResidenceSession();
                SetResidenceCinematic("Aftermath", "Hasar gezisi", 4f);
                CinematicShowsActor = false;
                if (player && residenceCinematicSession && !Model.IsSimulation)
                    SetResidencePlayerPosition(BunkerInteriorPosition + new Vector3(0f, .04f, -.42f));
                SetBunkerDoorOpen(false);
                return;
            }

            if (to == PanicPhase.Success)
            {
                BeginResidenceSession();
                SetResidenceCinematic("Success", "Güvenli sığınak", 3f);
                SetResidenceBunkerCutaway(true);
                PositionActorInBunker();
                CinematicShowsActor = true;
                SetResidenceActorFlags(false, false, 0f);
                SetBunkerDoorOpen(false);
                return;
            }

            ResetResidenceCinematic();
        }

        Vector3 cinematicActorStart;
        Vector3 cinematicHatchApproach;
        Vector3 cinematicActorEnd;

        void BeginResidenceLossReveal(PanicPhase from)
        {
            BeginResidenceSession();
            string reason = Model != null ? Model.FailureReason ?? string.Empty : string.Empty;
            if ((from == PanicPhase.Preparing || from == PanicPhase.Tutorial) &&
                (reason.Contains("süresi doldu") || reason.Contains("zamanında")))
                residencePendingLossKind = "Deadline";
            else if (reason.Contains("Çanta"))
                residencePendingLossKind = "BagFailure";
            else
                residencePendingLossKind = "DamageFailure";

            CinematicKind = residencePendingLossKind;
            CinematicTitle = ResidenceCinematicTitle(residencePendingLossKind);
            CinematicActive = false;
            CinematicProgress = 0f;
            residenceCinematicAge = 0f;
            residenceCinematicDuration = 6f;
            LossAnnounced = true;
            residenceLossClock = ResidenceLossRevealSeconds;
            LossRevealTime = ResidenceLossRevealSeconds;
            CinematicShowsActor = false;
            SetBunkerDoorOpen(false);
        }

        void BeginResidenceSession()
        {
            if (residenceCinematicSession) return;
            residenceCinematicSession = true;
            residenceReturnPosition = player ? player.position : Vector3.zero;
            residenceReturnRotation = player ? player.rotation : Quaternion.identity;
            residenceSavedControllerEnabled = controller && controller.enabled;
            residenceSavedWorking = animator && animator.GetBool("Working");
            residenceSavedKneeling = animator && animator.GetBool("Kneeling");
            residenceHasAnimatorPose = animator != null;
            if (View)
            {
                residenceSavedCameraOrthographic = View.orthographic;
                residenceSavedOrthoSize = View.orthographicSize;
                residenceSavedFieldOfView = View.fieldOfView;
                residenceSavedCameraPosition = View.transform.position;
                residenceSavedCameraRotation = View.transform.rotation;
            }
            residenceCameraVelocity = Vector3.zero;
        }

        void SetResidenceCinematic(string kind, string title, float duration)
        {
            ResetQuakeReadabilityV414("new cinematic");
            residenceCameraCutPending = true;
            CinematicKind = kind;
            CinematicTitle = title;
            residenceCinematicDuration = Mathf.Max(.1f, duration);
            residenceCinematicAge = 0f;
            CinematicProgress = 0f;
            CinematicActive = true;
            LossAnnounced = false;
            LossRevealTime = 0f;
            residencePendingLossKind = null;
            if (controller) controller.enabled = false;
            if (player) player.position = residenceReturnPosition;
            SetResidenceActorFlags(false, false, 0f);
        }

        void TickResidenceCinematic(float dt)
        {
            if (LossAnnounced)
            {
                residenceLossClock = Mathf.Max(0f, residenceLossClock - dt);
                LossRevealTime = residenceLossClock;
                if (residenceLossClock <= 0f)
                {
                    string kind = string.IsNullOrEmpty(residencePendingLossKind) ? "DamageFailure" : residencePendingLossKind;
                    SetResidenceCinematic(kind, ResidenceCinematicTitle(kind), 6f);
                    BeginResidenceFailureScene(kind);
                }
                return;
            }

            if (!CinematicActive) return;

            residenceCinematicAge += dt;
            CinematicProgress = Mathf.Clamp01(residenceCinematicAge / residenceCinematicDuration);
            if (CinematicKind == "Disaster") TickResidenceDisasterActor();
            TickStoryCaptionV414();
            if (IntroStoryActiveV414 && residenceCinematicAge >= residenceCinematicDuration) { SkipIntroStoryV414(); return; }

            if (residenceCinematicAge >= residenceCinematicDuration)
            {
                residenceCinematicAge = residenceCinematicDuration;
                CinematicProgress = 1f;
                CinematicActive = false;
                if (CinematicKind == "Deadline" || CinematicKind == "BagFailure" || CinematicKind == "DamageFailure")
                {
                    LossAnnounced = false;
                    CinematicShowsActor = CinematicKind != "DamageFailure";
                }
            }
        }

        bool ApplyResidenceCinematicCamera(float dt)
        {
            if (!View || !CinematicActive) return false;

            Vector3 destination;
            Vector3 target;
            float fov = 47f;
            GetResidenceCinematicShot(out destination, out target, out fov);

            int shot = CinematicKind == "Disaster" ? (residenceCinematicAge < 1.65f ? 0 : residenceCinematicAge < 4.7f ? 1 : residenceCinematicAge < (Model.Disaster == DisasterType.Earthquake ? 6.65f : 6.2f) ? 2 : 3) :
                CinematicKind == "Aftermath" ? (residenceCinematicAge < 2.15f ? 4 : 5) : CinematicKind == "Opening" ? (residenceCinematicAge < 3 ? 6 : residenceCinematicAge < 6 ? 7 : 8) : 9;
            if (shot != residenceShotIndex) { residenceShotIndex = shot; residenceCameraCutPending = true; }

            View.orthographic = false;
            View.fieldOfView = fov;
            bool cut=residenceCameraCutPending;
            if(cut){View.transform.position=destination;residenceCameraVelocity=Vector3.zero;residenceCameraCutPending=false;}
            else View.transform.position = Vector3.SmoothDamp(View.transform.position, destination,
                ref residenceCameraVelocity, .18f, 150f, Mathf.Max(.001f, dt));
            Vector3 look = target - View.transform.position;
            if (look.sqrMagnitude > .001f)
            {
                Quaternion rotation = Quaternion.LookRotation(look, Vector3.up);
                View.transform.rotation = cut ? rotation : Quaternion.Slerp(View.transform.rotation, rotation,
                    1f - Mathf.Exp(-12f * dt));
            }
            return true;
        }

        void GetResidenceCinematicShot(out Vector3 camera, out Vector3 target, out float fov)
        {
            if (TryQuakeReadabilityShotV414(out camera, out target, out fov)) return;
            if (StoryShotV414(out camera, out target, out fov)) return;
            float age = residenceCinematicAge;
            fov = 47f;
            if (CinematicKind == "Disaster")
            {
                if (Model.IsSimulation)
                {
                    float shotAge = Mathf.Lerp(4.7f,8f,SmoothResidence01(age/8f));
                    camera=Vector3.Lerp(new Vector3(22,15,-24),new Vector3(18,12,-21),SmoothResidence01(age/8f));
                    target=new Vector3(0,2,0);fov=49;
                    ApplyDisasterShotDirection(shotAge,ref camera,ref target,ref fov);return;
                }
                if (age < 1.65f)
                {
                    Vector3 actor = player ? player.position : BunkerPosition;
                    Vector3 towardHatch = Flat(BunkerPosition - actor).normalized;
                    if (towardHatch.sqrMagnitude < .01f) towardHatch = Vector3.forward;
                    camera = actor - towardHatch * 3.1f + Vector3.up * 1.42f + Vector3.right * 1.25f;
                    target = actor + Vector3.up * .95f + towardHatch * .35f;
                    fov = 52f;
                }
                else if (age < 4.7f)
                {
                    camera = BunkerPosition + new Vector3(3.2f, 2.2f, -3.8f);
                    target = BunkerPosition + new Vector3(0f, .35f, -.15f);
                    fov = 49f;
                }
                else if (age < 6.2f)
                {
                    float t = SmoothResidence01((age - 4.7f) / 1.5f);
                    camera = Vector3.Lerp(new Vector3(22f, 15f, -24f), new Vector3(19f, 13f, -21f), t);
                    target = Vector3.Lerp(new Vector3(0f, 1.6f, 0f), new Vector3(0f, 1.5f, 1.5f), t);
                    fov = 46f;
                }
                else
                {
                    float t = SmoothResidence01((age - 6.2f) / 1.8f);
                    camera = Vector3.Lerp(new Vector3(17f, 8.5f, -18f), new Vector3(14.5f, 7.5f, -16f), t);
                    target = new Vector3(2.5f, 2.1f, -.5f);
                    fov = 48f;
                }
                if (age >= 4.7f) ApplyDisasterShotDirection(age, ref camera, ref target, ref fov);
                return;
            }

            if (CinematicKind == "Aftermath")
            {
                float t = SmoothResidence01(age / 4f);
                if (age < 2.15f)
                {
                    camera = Vector3.Lerp(new Vector3(23f, 14f, -25f), new Vector3(21f, 12f, -22f), SmoothResidence01(age / 2.15f));
                    target = new Vector3(0f, 3.8f, 0f);
                    fov = 55f;
                }
                else
                {
                    float lowT = SmoothResidence01((age - 2.15f) / 1.85f);
                    camera = Vector3.Lerp(new Vector3(18f, 6.5f, -18f), new Vector3(16f, 5.8f, -16.5f), lowT);
                    target = new Vector3(2f, 1.25f, 0f);
                    fov = 50f;
                }
                target.y += Mathf.Sin(t * Mathf.PI) * .08f;
                return;
            }

            if (CinematicKind == "Success")
            {
                float t = SmoothResidence01(age / 3f);
                Vector3 center = BunkerInteriorPosition;
                camera = Vector3.Lerp(center + new Vector3(2.55f, 1.72f, -3.35f),
                    center + new Vector3(-2.45f, 1.78f, -3.05f), t);
                target = center + new Vector3(0f, 1.02f, .15f);
                fov = 46f;
                return;
            }

            if (CinematicKind == "Deadline")
            {
                if (age < 2.15f)
                {
                    camera = BunkerPosition + new Vector3(3.5f, 1.35f, -4.25f);
                    target = BunkerPosition + new Vector3(0f, .95f, -1.6f);
                    fov = 49f;
                }
                else if (age < 4.15f)
                {
                    float t = SmoothResidence01((age - 2.15f) / 2f);
                    camera = Vector3.Lerp(new Vector3(20f, 12f, -22f), new Vector3(-20f, 10.5f, -18f), t);
                    target = new Vector3(0f, 1.5f, 0f);
                    fov = 46f;
                }
                else
                {
                    float t = SmoothResidence01((age - 4.15f) / 1.85f);
                    camera = Vector3.Lerp(BunkerPosition + new Vector3(3.1f, 2.2f, -4f), BunkerPosition + new Vector3(2.5f, 1.65f, -2.9f), t);
                    target = BunkerPosition + new Vector3(0f, .3f, 0f);
                    fov = 52f;
                }
                return;
            }

            if (CinematicKind == "BagFailure")
            {
                float t = SmoothResidence01(age / 6f);
                Vector3 center = BunkerInteriorPosition;
                camera = Vector3.Lerp(center + new Vector3(2.8f, 1.65f, -3.5f),
                    center + new Vector3(-2.5f, 1.62f, -3.25f), t);
                target = center + new Vector3(.32f, .78f, -.15f);
                fov = 46f;
                return;
            }

            // A damage-only failure uses a low exterior pass across the actual damaged house.
            float damageT = SmoothResidence01(age / 6f);
            camera = Vector3.Lerp(new Vector3(20f, 8f, -18f), new Vector3(-17f, 3.1f, -16f), damageT);
            target = new Vector3(0f, 1.15f, 0f);
            fov = 51f;
        }

        void ApplyDisasterShotDirection(float age, ref Vector3 camera, ref Vector3 target, ref float fov)
        {
            bool detail = age >= 6.2f;
            float t = SmoothResidence01(detail ? (age - 6.2f) / 1.8f : (age - 4.7f) / 1.5f);
            if (Model.Disaster == DisasterType.Flood)
            {
                // Keep the creek and actual home together, above the west neighbour roof/canopy.
                camera = detail ? Vector3.Lerp(new Vector3(-27,22,-19),new Vector3(-25.5f,21,-17),t) : Vector3.Lerp(new Vector3(-29,23,-24),new Vector3(-27,22,-22),t);
                target = detail ? new Vector3(-7.5f,.7f,-.5f) : new Vector3(-7,1,0);
                fov = detail ? 56 : 52;
            }
            else if (Model.Disaster.ToString() == "Storm")
            {
                // The real funnel is west/north (-19.2,0..11.5,11), not above the house centre.
                camera = detail ? Vector3.Lerp(new Vector3(23,20,-26),new Vector3(21,19,-24),t) : Vector3.Lerp(new Vector3(29,23,-30),new Vector3(27,22,-28),t);
                target = detail ? new Vector3(-5.5f,5,4.5f) : new Vector3(-6,5.8f,4.5f); fov = detail ? 54 : 56;
            }
            else if (Model.Disaster == DisasterType.Fire)
            {
                if (detail)
                {
                    camera = Vector3.Lerp(new Vector3(11.5f,9,-18),new Vector3(9,8.2f,-15.5f),t);
                    target = new Vector3(4,6.1f,-5.8f); fov = 42;
                }
                else target = new Vector3(0,2.2f,0);
            }
            else
            {
                float envelope = Mathf.Sin(Mathf.Clamp01((age-4.7f)/3.3f)*Mathf.PI);
                camera += effects ? effects.DisasterVisualImpulseV414 * envelope : Vector3.zero;
            }
        }

        void TickResidenceDisasterActor()
        {
            if (Model.IsSimulation) { CinematicShowsActor = false; SetResidenceActorFlags(false, false, 0); return; }
            TickBunkerSequenceV414();
        }

        void BeginResidenceFailureScene(string kind)
        {
            if (kind == "Deadline")
            {
                SetBunkerDoorOpen(false);
                Vector3 outside = BunkerPosition + new Vector3(0f, .04f, -2.05f);
                if (player) player.position = outside;
                FaceResidenceActor(BunkerPosition - outside);
                CinematicShowsActor = true;
                SetResidenceActorFlags(true, true, 0f);
                if (effects) effects.StartCinematicWeather();
                return;
            }

            if (kind == "BagFailure")
            {
                SetResidenceBunkerCutaway(true);
                PrepareEmptyBunkerCrate();
                PositionActorInBunker();
                CinematicShowsActor = true;
                SetResidenceActorFlags(true, true, 0f);
                if (residenceFailureLampObject)
                {
                    var light = residenceFailureLampObject.GetComponent<Light>();
                    if (light) { light.color = new Color(1f, .16f, .09f); light.intensity = 3.1f; }
                }
                return;
            }

            SetBunkerDoorOpen(false);
            CinematicShowsActor = false;
            SetResidenceActorFlags(false, false, 0f);
            if (effects) effects.StartCinematicWeather();
        }

        void PrepareEmptyBunkerCrate()
        {
            if (!residenceBunkerRoom) return;
            residenceSavedSupplyDisplay = residenceBunkerRoom.transform.Find("ShelterBenchSupplies");
            if (residenceSavedSupplyDisplay)
            {
                residenceSavedSupplyDisplayActive = residenceSavedSupplyDisplay.gameObject.activeSelf;
                residenceSavedSupplyDisplay.gameObject.SetActive(false);
            }

            if (!residenceFailureCrate)
            {
                residenceFailureCrate = CreateVisual("supplycrate");
                residenceFailureCrate.name = "Empty shelter crate";
                residenceFailureCrate.transform.SetParent(residenceBunkerRoom.transform, false);
                residenceFailureCrate.transform.localPosition = new Vector3(.95f, 0f, -.35f);
                residenceFailureCrate.transform.localRotation = Quaternion.Euler(0f, -18f, 0f);
                foreach (var renderer in residenceFailureCrate.GetComponentsInChildren<Renderer>(true))
                {
                    string n = renderer.gameObject.name;
                    // The authored crate batches its contents in linen/slate submeshes.
                    if (n.EndsWith("_Linen") || n.EndsWith("_Slate")) renderer.enabled = false;
                }
                Transform lid = residenceFailureCrate.transform.Find("Lid");
                if (lid) lid.localRotation = Quaternion.Euler(-105f, 0f, 0f);
            }

            if (!residenceFailureLampObject)
            {
                residenceFailureLampObject = new GameObject("Bunker red emergency light");
                residenceFailureLampObject.transform.SetParent(residenceBunkerRoom.transform, false);
                residenceFailureLampObject.transform.localPosition = new Vector3(0f, 2.1f, .05f);
                var light = residenceFailureLampObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 8f;
                light.color = new Color(1f, .16f, .09f);
                light.intensity = 3.1f;
            }
        }

        void PositionActorInBunker()
        {
            if (player) player.position = BunkerInteriorPosition + new Vector3(-.22f, .04f, -.55f);
            FaceResidenceActor(Vector3.forward);
        }

        void SetResidenceBunkerCutaway(bool enabled)
        {
            if (!residenceBunkerRoom) return;
            Transform roofGroup = residenceBunkerRoom.transform.Find("Roof");
            Transform frontGroup = residenceBunkerRoom.transform.Find("FrontHighWalls");
            if (roofGroup) roofGroup.gameObject.SetActive(!enabled);
            if (frontGroup) frontGroup.gameObject.SetActive(!enabled);
        }

        void SetResidencePlayerPosition(Vector3 position)
        {
            if (player) player.position = position;
        }

        void FaceResidenceActor(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < .001f) return;
            Quaternion facing = Quaternion.LookRotation(direction.normalized, Vector3.up);
            if (player) player.rotation = facing;
            if (hero) hero.rotation = facing;
        }

        void SetResidenceActorFlags(bool kneeling, bool working, float speed)
        {
            if (!animator) return;
            animator.SetBool("Airborne", false);
            animator.SetBool("Working", working);
            animator.SetBool("Kneeling", kneeling);
            animator.SetFloat("Speed", speed);
        }

        void ResetResidenceCinematic()
        {
            ResetQuakeReadabilityV414("cinematic reset");
            // Explicit cinematic exit only; ordinary locomotion never calls pose restoration.
            if (hero && residenceCinematicSession) PanicCharacterArt.ClearTaskPose(hero.gameObject);
            CleanupBunkerBagV414();
            CinematicActive = false;
            CinematicTitle = string.Empty;
            CinematicProgress = 0f;
            CinematicKind = string.Empty;
            CinematicShowsActor = false;
            LossAnnounced = false;
            LossRevealTime = 0f;
            residenceCinematicAge = 0f;
            residenceCinematicDuration = 0f;
            residenceLossClock = 0f;
            residencePendingLossKind = null;
            SetBunkerDoorOpen(false);
            SetResidenceBunkerCutaway(false);

            if (residenceSavedSupplyDisplay)
                residenceSavedSupplyDisplay.gameObject.SetActive(residenceSavedSupplyDisplayActive);
            if (residenceFailureCrate) Destroy(residenceFailureCrate);
            if (residenceFailureLampObject) Destroy(residenceFailureLampObject);
            residenceFailureCrate = null;
            residenceFailureLampObject = null;
            residenceSavedSupplyDisplay = null;

            if (residenceCinematicSession)
            {
                if (player)
                {
                    bool wasEnabled = controller && controller.enabled;
                    if (controller) controller.enabled = false;
                    player.position = residenceReturnPosition;
                    player.rotation = residenceReturnRotation;
                    if (controller) controller.enabled = residenceSavedControllerEnabled || wasEnabled;
                }
                if (controller && player && !residenceSavedControllerEnabled) controller.enabled = false;
                if (animator && residenceHasAnimatorPose)
                {
                    animator.SetBool("Working", residenceSavedWorking);
                    animator.SetBool("Kneeling", residenceSavedKneeling);
                    animator.SetBool("Airborne", false);
                    animator.SetFloat("Speed", 0f);
                }
                if (View)
                {
                    View.orthographic = residenceSavedCameraOrthographic;
                    View.orthographicSize = residenceSavedOrthoSize;
                    View.fieldOfView = residenceSavedFieldOfView;
                    View.transform.position = residenceSavedCameraPosition;
                    View.transform.rotation = residenceSavedCameraRotation;
                }
                gravitySpeed = -2f;
                desiredVelocity = Vector3.zero;
            }

            residenceCinematicSession = false;
            residenceHasAnimatorPose = false;
        }

        static string ResidenceCinematicTitle(string kind)
        {
            switch (kind)
            {
                case "Disaster": return "Afet başladı";
                case "Aftermath": return "Hasar gezisi";
                case "Success": return "Güvenli sığınak";
                case "Deadline": return "Vaktin doldu";
                case "BagFailure": return "Çanta eksik";
                default: return "Ev hasarı";
            }
        }

        static float SmoothResidence01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - 2f * t);
        }
    }
}
