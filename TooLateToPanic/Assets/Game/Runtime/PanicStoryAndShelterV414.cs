using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicPermanentlyHiddenV414 : MonoBehaviour { }
    public sealed partial class PanicHomeWorld
    {
        public bool IntroStoryActiveV414 { get { return CinematicActive && CinematicKind == "Opening"; } }
        public int IntroStoryResolutionCountV414 { get; private set; }
        public string IntroStoryTextV414
        {
            get
            {
                return PanicKktcStoryV53.OpeningCaption(residenceCinematicAge, UsesUpperPanicRoomV5);
            }
        }
        public string BunkerSequenceStageV414 { get; private set; } = "idle";
        public bool BunkerBagReleasedV414 { get; private set; }
        public int BunkerSequenceResolutionCountV414 { get; private set; }
        public GameObject BunkerBagObjectV414 { get { return cinematicBagV414; } }
        GameObject cinematicBagV414;
        bool bunkerSequenceResolvedV414;
        Vector3 bagReleasePositionV414;
        Quaternion bagReleaseRotationV414;
        Vector3 cinematicBagGripLocalV414;
        PanicStreetLifeV414 streetLifeV414;

        void InitOverhaulV414()
        {
            AddBox("Permanent roof ceiling V414", new Vector3(0, 6.48f, 0), new Vector3(16f, .2f, 12f));
            InitRoofAccessPhysicsR6();
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
            SetResidenceCinematic("Opening", PanicKktcStoryV53.OpeningTitle, 16f);
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
            camera = target = Vector3.zero; fov = 48;
            if (!IntroStoryActiveV414) return false;
            float age = residenceCinematicAge;
            if (age < 4)
            {
                float t = SmoothResidence01(age / 4);
                camera = Vector3.Lerp(new Vector3(25, 17, -28), new Vector3(21, 14, -24), t);
                target = new Vector3(-1, 2, 0); fov = 51;
            }
            else if (age < 8)
            {
                float t = SmoothResidence01((age - 4) / 4);
                camera = Vector3.Lerp(new Vector3(-12, 10, -13), new Vector3(-10, 8, -11), t);
                target = new Vector3(-2, .7f, .2f); fov = 56;
            }
            else if (age < 12)
            {
                Vector3 bag = new Vector3(4.5f, .3f, 4.7f); // Kitchen storage where emergency supplies become available in the final stage.
                camera = bag + Vector3.Lerp(new Vector3(-1.3f, 1.4f, -1.8f), new Vector3(-.8f, 1.15f, -1.3f), SmoothResidence01((age - 8) / 4));
                target = bag + Vector3.up * .35f; fov = 51;
            }
            else
            {
                Vector3 shelter = ActiveShelterPositionV5;
                camera = shelter + Vector3.Lerp(new Vector3(2.7f, 2.2f, -3.4f), new Vector3(2.4f, 1.8f, -2.9f), SmoothResidence01((age - 12) / 4));
                target = shelter + Vector3.up * .55f; fov = 54;
            }
            return true;
        }

        void EnterPresentationSafeBunkerV414()
        {
            TryEnterActiveShelterV5();
        }

        void BeginBunkerSequenceV414()
        {
            CleanupBunkerBagV414();
            BunkerSequenceStageV414 = "open";
            BunkerBagReleasedV414 = bunkerSequenceResolvedV414 = false;
            // Bag stays invisibly carried throughout the shelter sequence.
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
                SetResidenceActorFlags(false, age >= 1f, 0);
                // Cinematic-only approach uses the retained motor's original damped Animator path.
                if(age<1f)SetMoveAnimation(Mathf.Clamp01(BunkerApproachRouteLengthV414/RunSpeed),Time.deltaTime);
                SetBunkerLidPoseV414(SmoothResidence01((age - 1f) / .65f));
            }
            else if (age < 2.5f)
            {
                BunkerSequenceStageV414 = "check_hatch";
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
                    UpdateHeldBunkerBagV414();
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

        void PrepareBunkerBagGripV414()
        {
            cinematicBagGripLocalV414=Vector3.zero;if(!cinematicBagV414)return;
            Transform bag=cinematicBagV414.transform;Bounds bounds=new Bounds();bool any=false;
            foreach(var renderer in cinematicBagV414.GetComponentsInChildren<Renderer>(true))
            {
                if(!renderer.enabled)continue;Bounds b=renderer.bounds;
                for(int i=0;i<8;i++)
                {
                    Vector3 p=bag.InverseTransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));
                    if(!any){bounds=new Bounds(p,Vector3.zero);any=true;}else bounds.Encapsulate(p);
                }
            }
            // CreateDisplay is explicitly bottom-centred. The hand holds near the top, not the bottom pivot.
            if(any)cinematicBagGripLocalV414=new Vector3(bounds.center.x,bounds.max.y-bounds.size.y*.02f,bounds.center.z);
        }

        void UpdateHeldBunkerBagV414()
        {
            if(!cinematicBagV414||BunkerBagReleasedV414||residenceCinematicAge>=1.85f)return;
            var hand=PanicCharacterArt.GetCharacterBone(hero.gameObject,animator,HumanBodyBones.RightHand);
            Vector3 hold=hand?hand.position:player.position+new Vector3(.25f,.85f,.2f);
            Transform bag=cinematicBagV414.transform;bag.rotation=Quaternion.Euler(0,facingYaw,-12);
            bag.position+=hold-bag.TransformPoint(cinematicBagGripLocalV414);
            bagReleasePositionV414=bag.position;bagReleaseRotationV414=bag.rotation;
        }

        bool ApplyCinematicCharacterPoseV414()
        {
            if (!CinematicActive) return false;
            if (CinematicKind == "Disaster" && !Model.IsSimulation && ShelterReachedForPresentationV5 && !UpperShelterPresentationV5)
            {
                if (residenceCinematicAge >= 1f && residenceCinematicAge < 1.65f) PanicCharacterArt.SetTaskPose(hero.gameObject, animator, "gather", .5f);
                else if (residenceCinematicAge >= 1.65f && residenceCinematicAge < 2.5f) PanicCharacterArt.SetTaskPose(hero.gameObject, animator, "gather", .65f);
                else if (residenceCinematicAge >= 2.5f) PanicCharacterArt.SetCinematicPose(hero.gameObject, "jump", Mathf.Clamp01((residenceCinematicAge - 2.5f) / 1.55f));
                // Sync the held cosmetic bag after this exact frame's evaluated hand pose, before real release.
                if(residenceCinematicAge<1.85f)UpdateHeldBunkerBagV414();
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
