using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        // The Model's physical admission flag is the single truth. Automatic deadline/no-loss
        // transitions never receive an arrival animation, even though they still simulate damage.
        public bool ShelterReachedForPresentationV5 { get { return Model != null && Model.ReachedShelterV5; } }
        public Vector3 FailureActorOriginV5 { get { return residenceReturnPosition; } }
        public int CinematicViewFloorV5 { get { return IntroStoryActiveV414 && residenceCinematicAge >= 12 ? (UsesUpperPanicRoomV5 ? 1 : -1) : ActiveFloor; } }
        public Vector3 CinematicViewFocusV5 { get { return IntroStoryActiveV414 && residenceCinematicAge >= 12 ? ActiveShelterPositionV5 : PlayerPosition; } }
        Vector3 residenceShotBlendStartV5;
        Quaternion residenceShotBlendRotationV5;
        float residenceShotBlendAgeV5 = 1;

        bool UpperShelterPresentationV5 { get { return ShelterReachedForPresentationV5 && UsesUpperPanicRoomV5 && !Model.IsSimulation && !JuryActive; } }
        Vector3 ShelterWaitPositionV5 { get { return UpperShelterPresentationV5 ? cinematicActorStart : BunkerInteriorPosition + new Vector3(0, .04f, -.42f); } }

        void BeginUpperShelterPresentationV5()
        {
            CleanupBunkerBagV414();
            BunkerSequenceStageV414 = "upper_room_wait";
            BunkerBagReleasedV414 = false;
            bunkerSequenceResolvedV414 = true;
            BunkerSequenceResolutionCountV414++;
            // Real E admission already occurred inside the physical upper room. Keep its actual
            // floor/position and retained humanoid idle; do not slide through a door or wall.
            cinematicActorEnd = cinematicActorStart;
            CinematicShowsActor = true;
            SetResidenceActorFlags(false, false, 0);
        }

        void TickShelterPresentationV5()
        {
            if (!player) return;
            if (!ShelterReachedForPresentationV5)
            {
                player.position = cinematicActorStart;
                CinematicShowsActor = residenceCinematicAge < 4.7f;
                BunkerSequenceStageV414 = "exposed_actual_position";
                SetResidenceActorFlags(true, false, 0);
                if (residenceCinematicAge < 4.7f) CinematicTitle = "Sığınağa ulaşamadın. Afet seni bulunduğun yerde yakaladı.";
                return;
            }
            if (!UpperShelterPresentationV5) { TickBunkerSequenceV414(); return; }
            player.position = cinematicActorStart;
            CinematicShowsActor = residenceCinematicAge < 4.7f;
            SetResidenceActorFlags(false, false, 0);
            CinematicTitle = residenceCinematicAge < 4.7f ? "Üst kat panik odasına ulaştın. Selin geçmesini burada bekle." : PanicKktcStoryV53.HazardNarrative(DisasterType.Flood);
        }

        bool TryShelterShotV5(out Vector3 camera, out Vector3 target, out float fov)
        {
            camera = target = Vector3.zero; fov = 52;
            float age = residenceCinematicAge;
            bool unreached = !ShelterReachedForPresentationV5 && !Model.IsSimulation;
            if (CinematicKind == "Deadline" || CinematicKind == "BagFailure" && unreached ||
                CinematicKind == "Disaster" && unreached && age < 4.7f)
            {
                Vector3 actor = residenceReturnPosition;
                Vector3 forward = residenceReturnRotation * Vector3.forward;
                Vector3 right = residenceReturnRotation * Vector3.right;
                float t = SmoothResidence01(age / (CinematicKind == "Disaster" ? 4.7f : 6));
                camera = actor - forward * Mathf.Lerp(2.5f, 1.9f, t) + right * Mathf.Lerp(.9f, 1.25f, t) + Vector3.up * 1.35f;
                target = actor + Vector3.up * .83f;
                return true;
            }
            if (!UpperShelterPresentationV5) return false;
            if (CinematicKind == "Disaster")
            {
                if (age >= 4.7f)
                {
                    camera = Vector3.Lerp(new Vector3(-29, 23, -24), new Vector3(-25.5f, 21, -17), SmoothResidence01((age - 4.7f) / 3.3f));
                    target = new Vector3(-7, 1, 0); fov = 53;
                    return true;
                }
                float t = SmoothResidence01(age / 4.7f);
                camera = cinematicActorStart + Vector3.Lerp(new Vector3(1.25f, 1.65f, -2.65f), new Vector3(1.5f, 1.7f, -2.45f), t);
                target = cinematicActorStart + Vector3.up * .92f;
                fov = 54;
                return true;
            }
            if (CinematicKind == "Success" || CinematicKind == "BagFailure")
            {
                float t = SmoothResidence01(age / residenceCinematicDuration);
                Vector3 wait = ShelterWaitPositionV5;
                camera = wait + Vector3.Lerp(new Vector3(1.25f, 1.65f, -2.65f), new Vector3(1.5f, 1.7f, -2.45f), t);
                target = wait + Vector3.up * .85f;
                return true;
            }
            return false;
        }
    }
}
