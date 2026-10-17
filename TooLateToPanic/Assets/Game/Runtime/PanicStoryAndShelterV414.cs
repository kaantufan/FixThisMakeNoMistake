using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicPermanentlyHiddenV414 : MonoBehaviour { }
    public sealed partial class PanicHomeWorld
    {
        public bool IntroStoryActiveV414 { get { return CinematicActive && CinematicKind == "Opening"; } }
        public int IntroStoryResolutionCountV414 { get; private set; }
        public string IntroStoryTextV414 { get { return residenceCinematicAge < 3 ? "Vadidere'de sıradan bir sonbahar sabahı. Ailen güvenli bir yere gitti; evin hazırlığı sana kaldı." :
            residenceCinematicAge < 6 ? "Radyo yaklaşan tehlikeyi haber veriyor. Eşyaları sabitle, tesisatı güvene al ve evini güçlendir." :
            "Önlem almak için iki dakikan var. Son otuz saniyede su ve gıdanı topla, çantanı al ve bodrum sığınağına yetiş."; } }
        public string BunkerSequenceStageV414 { get; private set; } = "idle";
        public bool BunkerBagReleasedV414 { get; private set; }
        public int BunkerSequenceResolutionCountV414 { get; private set; }
        public GameObject BunkerBagObjectV414 { get { return cinematicBagV414; } }
        GameObject cinematicBagV414;
        bool bunkerSequenceResolvedV414;
        Vector3 bagReleasePositionV414;
        Quaternion bagReleaseRotationV414;
        PanicStreetLifeV414 streetLifeV414;

        void InitOverhaulV414()
        {
            AddBox("Permanent roof ceiling V414", new Vector3(0, 6.48f, 0), new Vector3(16f, .2f, 12f));
            streetLifeV414 = gameObject.AddComponent<PanicStreetLifeV414>();
            streetLifeV414.Bind(this);
        }

        void ResetOverhaulRunV414()
        {
            IntroStoryResolutionCountV414 = 0;
            BunkerSequenceResolutionCountV414 = 0;
            BunkerBagReleasedV414 = false;
            BunkerSequenceStageV414 = "idle";
            CleanupBunkerBagV414();
            if (streetLifeV414) streetLifeV414.ResetRun();
        }

        void BeginIntroStoryV414()
        {
            if (Model == null || Model.IsSimulation || Model.IsTutorial) return;
            BeginResidenceSession();
            SetResidenceCinematic("Opening", "Hazırlık için hâlâ zaman var", 9f);
            CinematicShowsActor = false;
        }

        public void SkipIntroStoryV414()
        {
            if (!IntroStoryActiveV414) return;
            IntroStoryResolutionCountV414++;
            ResetResidenceCinematic();
            CancelFrame = DiscreteInputFrame = Time.frameCount;
            ApplyFloorVisibility();
        }

        void TickStoryCaptionV414()
        {
            if (IntroStoryActiveV414) CinematicTitle = IntroStoryTextV414;
        }

        bool StoryShotV414(out Vector3 camera, out Vector3 target, out float fov)
        {
            camera = target = Vector3.zero; fov = 48f;
            if (!IntroStoryActiveV414) return false;
            float age = residenceCinematicAge;
            if (age < 3)
            {
                camera = Vector3.Lerp(new Vector3(25, 17, -28), new Vector3(21, 14, -24), SmoothResidence01(age / 3));
                target = new Vector3(-1, 2, 0);
            }
            else if (age < 6)
            {
                camera = Vector3.Lerp(new Vector3(-12, 10, -13), new Vector3(-10, 8, -11), SmoothResidence01((age - 3) / 3));
                target = new Vector3(-2, .7f, .2f); fov = 56;
            }
            else
            {
                Vector3 bag = Model.Items.Find(i => i.Id == "bagstation")?.Position ?? new Vector3(1.55f, 0, -2.3f);
                camera = Vector3.Lerp(bag + new Vector3(-1.3f, 1.4f, -1.8f), bag + new Vector3(-.8f, 1.15f, -1.3f), SmoothResidence01((age - 6) / 3));
                target = bag + Vector3.up * .35f; fov = 51;
            }
            return true;
        }

        void EnterPresentationSafeBunkerV414()
        {
            if (!controller || !controller.isGrounded) { Model.Notify("Sığınağa girmek için kapak yanında yere bas."); return; }
            if (!BuildBunkerApproachRouteV414(PlayerPosition, BunkerPosition + new Vector3(0, .04f, -1.18f)))
            { Model.Notify("Kapak önüne açık yoldan yaklaş; sığınak girişini boş bırak."); return; }
            Model.EnterBunker();
        }

        void BeginBunkerSequenceV414()
        {
            CleanupBunkerBagV414();
            BunkerSequenceStageV414 = "open";
            BunkerBagReleasedV414 = bunkerSequenceResolvedV414 = false;
            if (!PanicCuratedAssetArt.TryCreateFixture("emergencybackpack", out cinematicBagV414))
                cinematicBagV414 = PanicResidenceArt.Create("bagstation");
            cinematicBagV414.name = "Actual thrown emergency bag V414";
            cinematicBagV414.transform.localScale *= .68f;
            foreach (var c in cinematicBagV414.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            foreach (var body in cinematicBagV414.GetComponentsInChildren<Rigidbody>(true)) { body.isKinematic = true; body.detectCollisions = false; }
            BuildBunkerApproachRouteV414(cinematicActorStart, cinematicHatchApproach);
            SetBunkerLidPoseV414(0f);
        }

        void SetBunkerLidPoseV414(float openness)
        {
            float t = Mathf.Clamp01(openness);
            if (hinges.TryGetValue("bunker", out var h)) { h.open = t > 0; h.lid = true; h.openAngle = 105f * t; }
            if (bunkerLeaf) bunkerLeaf.localRotation = bunkerClosed * Quaternion.Euler(105f * t, 0, 0);
        }

        void TickBunkerSequenceV414()
        {
            float age = residenceCinematicAge;
            if (!BunkerApproachRouteValidV414) { CinematicShowsActor = false; BunkerSequenceStageV414 = "routeblocked"; CleanupBunkerBagV414(); return; }
            Vector3 shaft = BunkerPosition + new Vector3(0, .04f, -.10f);
            if (age < 1.65f)
            {
                BunkerSequenceStageV414 = "open";
                CinematicShowsActor = true;
                player.position = SampleBunkerApproachRouteV414(Mathf.Clamp01(age));
                FaceResidenceActor(BunkerPosition - player.position);
                SetResidenceActorFlags(false, age >= 1f, age < 1f ? .85f : 0);
                SetBunkerLidPoseV414(SmoothResidence01((age - 1f) / .65f));
            }
            else if (age < 2.5f)
            {
                BunkerSequenceStageV414 = "bagthrow";
                CinematicShowsActor = true;
                player.position = cinematicHatchApproach;
                SetResidenceActorFlags(false, false, 0);
                SetBunkerLidPoseV414(1);
            }
            else if (age < 4.05f)
            {
                BunkerSequenceStageV414 = "jump";
                CinematicShowsActor = true;
                if (age < 2.95f)
                {
                    float t = Mathf.Clamp01((age - 2.5f) / .45f);
                    player.position = Vector3.Lerp(cinematicHatchApproach, shaft, SmoothResidence01(t)) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * .26f);
                }
                else
                {
                    float t = SmoothResidence01((age - 2.95f) / 1.1f);
                    player.position = shaft + Vector3.down * (t * 2.45f);
                }
                SetBunkerLidPoseV414(1f - SmoothResidence01((age - 3.8f) / .25f));
                SetResidenceActorFlags(true, true, 0);
            }
            else
            {
                BunkerSequenceStageV414 = "closed";
                CinematicShowsActor = false;
                player.position = cinematicActorEnd;
                SetBunkerLidPoseV414(0);
                SetResidenceActorFlags(false, false, 0);
                if (!bunkerSequenceResolvedV414) { bunkerSequenceResolvedV414 = true; BunkerSequenceResolutionCountV414++; }
            }
            if (cinematicBagV414)
            {
                if (age < 1.85f)
                {
                    var hand = PanicCharacterArt.GetCharacterBone(hero.gameObject, animator, HumanBodyBones.RightHand);
                    cinematicBagV414.transform.SetPositionAndRotation(hand ? hand.position : player.position + new Vector3(.25f, .85f, .2f), Quaternion.Euler(0, facingYaw, -12));
                    bagReleasePositionV414 = cinematicBagV414.transform.position;
                    bagReleaseRotationV414 = cinematicBagV414.transform.rotation;
                }
                else
                {
                    BunkerBagReleasedV414 = true;
                    float flight = age - 1.85f;
                    float t = Mathf.Clamp01(flight / .65f);
                    Vector3 mouth = BunkerPosition + Vector3.up * .76f;
                    cinematicBagV414.transform.position = flight < .3f ? Vector3.Lerp(bagReleasePositionV414, mouth, SmoothResidence01(flight / .3f)) + Vector3.up * (Mathf.Sin(flight / .3f * Mathf.PI) * .25f) :
                        Vector3.Lerp(mouth, BunkerPosition + Vector3.down * 1.8f, SmoothResidence01((flight - .3f) / .35f));
                    cinematicBagV414.transform.rotation = bagReleaseRotationV414 * Quaternion.Euler(t * 160, t * 70, t * 30);
                    cinematicBagV414.SetActive(age < 2.5f);
                }
            }
        }

        bool ApplyCinematicCharacterPoseV414()
        {
            if (!CinematicActive) return false;
            if (CinematicKind == "Disaster" && !Model.IsSimulation)
            {
                if (residenceCinematicAge < 1f) PanicCharacterArt.SetMotion(hero.gameObject, animator, BunkerApproachRouteLengthV414, true, 0, Time.deltaTime);
                else if (residenceCinematicAge < 1.65f) { PanicCharacterArt.SetMotion(hero.gameObject, animator, 0, true, 0, Time.deltaTime); PanicCharacterArt.SetTaskPose(hero.gameObject, animator, "gather", .5f); }
                else if (residenceCinematicAge < 2.5f) PanicCharacterArt.SetCinematicPose(hero.gameObject, "throw", Mathf.Clamp01((residenceCinematicAge - 1.65f) / .85f));
                else PanicCharacterArt.SetCinematicPose(hero.gameObject, "jump", Mathf.Clamp01((residenceCinematicAge - 2.5f) / 1.55f));
            }
            return true;
        }

        void CleanupBunkerBagV414()
        {
            if (cinematicBagV414) Destroy(cinematicBagV414);
            cinematicBagV414 = null;
        }
    }
}
