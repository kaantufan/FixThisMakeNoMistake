using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        // Camera-only edit. All times are the existing presentation clock, including pause/skip.
        int ResidenceShotIndexV512()
        {
            float age = residenceCinematicAge;
            if (CinematicKind == "Opening") return 10 + Mathf.Min(3, Mathf.FloorToInt(age / 4f));
            if (CinematicKind == "Disaster") return age < 1.65f ? 20 : age < 4.7f ? 21 : age < 6.2f ? 22 : 23;
            if (CinematicKind == "Aftermath") return age < 2.15f ? 30 : 31;
            if (CinematicKind == "Success") return age < 1.55f ? 40 : 41;
            if (CinematicKind == "DamageFailure") return age < 3.2f ? 50 : 51;
            // Unreached failure remains a continuous view of the actual player position.
            return 60;
        }

        void DirectResidenceShotV512(ref Vector3 camera, ref Vector3 target, ref float fov)
        {
            float age = residenceCinematicAge;
            if (CinematicKind == "Opening")
            {
                if (age < 4f)
                {
                    float t = SmoothResidence01(age / 4f);
                    // Elevated front three-quarter establish: house, garden and street together.
                    camera = Vector3.Lerp(new Vector3(24, 15.5f, -29), new Vector3(21.5f, 14.2f, -26), t);
                    target = Vector3.Lerp(new Vector3(-1, 2.1f, 0), new Vector3(0, 2.3f, .4f), t);
                    fov = 49;
                }
                else if (age < 8f)
                {
                    float t = SmoothResidence01((age - 4f) / 4f);
                    // Cutaway precaution context. Small same-side truck instead of a flying orbit.
                    camera = Vector3.Lerp(new Vector3(-11, 8.7f, -12), new Vector3(-9.7f, 8.1f, -11.3f), t);
                    target = new Vector3(-2.5f, 1.05f, .3f); fov = 50;
                }
                else if (age < 12f)
                {
                    float t = SmoothResidence01((age - 8f) / 4f);
                    Vector3 supplies = new Vector3(4.5f, .3f, 4.7f);
                    camera = supplies + Vector3.Lerp(new Vector3(-1.55f, 1.65f, -2.15f), new Vector3(-1.2f, 1.48f, -1.9f), t);
                    target = supplies + Vector3.up * .48f; fov = 46;
                }
                else
                {
                    float t = SmoothResidence01((age - 12f) / 4f);
                    Vector3 shelter = ActiveShelterPositionV5;
                    camera = shelter + Vector3.Lerp(new Vector3(2.7f, 2.2f, -3.4f), new Vector3(2.35f, 1.95f, -3.05f), t);
                    target = shelter + Vector3.up * .6f; fov = 49;
                }
                return;
            }
            if (CinematicKind == "Disaster" && age >= 4.7f)
            {
                bool detail = age >= 6.2f;
                float t = SmoothResidence01(detail ? (age - 6.2f) / 1.8f : (age - 4.7f) / 1.5f);
                // Keep compound-hazard wide framing and existing hazard-specific subject selection.
                // Quake's baseline still starts/ticks its actual house-motion presentation.
                if (Model.ActiveDisasterCountV54 == 1 && Model.HasActiveDisasterV54(DisasterType.Earthquake))
                {
                    camera = detail ? Vector3.Lerp(new Vector3(14.5f, 9.7f, -21), new Vector3(13.8f, 9.3f, -20.2f), t)
                                    : Vector3.Lerp(new Vector3(21, 13.6f, -27), new Vector3(19.8f, 13, -25.8f), t);
                    target = detail ? new Vector3(1, 2.5f, -.5f) : new Vector3(.5f, 2.35f, 0);
                    fov = detail ? 48 : 50;
                }
                else
                {
                    // Gentle lateral parallax; preserve flood creek/funnel/fire framing and headroom.
                    camera += new Vector3((t - .5f) * (detail ? .55f : .35f), 0, 0);
                }
                return;
            }
            if (CinematicKind == "Aftermath")
            {
                bool detail = age >= 2.15f;
                float t = SmoothResidence01(detail ? (age - 2.15f) / 1.85f : age / 2.15f);
                camera = detail ? Vector3.Lerp(new Vector3(-18, 9.3f, -23), new Vector3(-17, 8.9f, -22), t)
                                : Vector3.Lerp(new Vector3(24, 15.5f, -27), new Vector3(22.8f, 14.7f, -25.8f), t);
                target = detail ? new Vector3(-1.5f, 2.1f, -.4f) : new Vector3(0, 2.8f, 0);
                fov = detail ? 47 : 52; return;
            }
            if (CinematicKind == "Success" && ShelterReachedForPresentationV5)
            {
                // Two quiet views of the admitted actor. Never move actor or manufacture admission.
                Vector3 actor = player ? player.position : ShelterWaitPositionV5;
                bool detail = age >= 1.55f;
                float t = SmoothResidence01(detail ? (age - 1.55f) / 1.45f : age / 1.55f);
                float roomSide = UpperShelterPresentationV5 ? .65f : 1f;
                camera = actor + (detail ? Vector3.Lerp(new Vector3(.95f, 1.65f, -2.85f), new Vector3(.85f, 1.6f, -2.75f), t)
                                           : Vector3.Lerp(new Vector3(1.5f, 1.9f, -3.6f), new Vector3(1.35f, 1.85f, -3.5f), t));
                camera.x = actor.x + (camera.x - actor.x) * roomSide;
                target = actor + Vector3.up * .95f; fov = detail ? 50 : 54; return;
            }
            if (CinematicKind == "DamageFailure")
            {
                bool detail = age >= 3.2f;
                float t = SmoothResidence01(detail ? (age - 3.2f) / 2.8f : age / 3.2f);
                // No long lateral sweep through the building or distant garden geometry.
                camera = detail ? Vector3.Lerp(new Vector3(-18, 9, -23), new Vector3(-16.5f, 8.4f, -21.5f), t)
                                : Vector3.Lerp(new Vector3(22, 14, -26), new Vector3(20, 12.7f, -24), t);
                target = new Vector3(0, 2.1f, 0); fov = detail ? 46 : 51;
            }
        }
    }
}
