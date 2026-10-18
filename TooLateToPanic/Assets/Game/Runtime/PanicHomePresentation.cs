using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Cosmetic feedback only. No inventory, damage, cost, clock or collision mutation.</summary>
    public sealed partial class PanicHomeWorld
    {
        public static readonly Color PresentationOrange = new Color(1f, .48f, .16f, 1f);
        const float PickupFlightSeconds = .30f;
        const int MaxPickupFlights = 3;

        sealed class PickupFlight
        {
            public GameObject Visual;
            public Vector3 Start;
            public float Age;
        }
        sealed class HardwareReveal
        {
            public Transform Target;
            public Vector3 Scale;
            public float Age;
        }
        sealed class VisualPose
        {
            public Transform Target;
            public Vector3 Position, RootPosition;
            public Quaternion Rotation, RootRotation;
        }
        sealed class HeroReaction
        {
            public Transform Root;
            public bool Anchored;
            public float ResolvedDamage;
            public readonly List<VisualPose> Poses = new List<VisualPose>();
        }

        readonly List<PickupFlight> pickupFlights = new List<PickupFlight>(MaxPickupFlights);
        readonly List<HardwareReveal> hardwareReveals = new List<HardwareReveal>(2);
        readonly List<HeroReaction> presentationHeroes = new List<HeroReaction>(2);
        readonly Dictionary<string, bool> presentationAnchored = new Dictionary<string, bool>();
        Transform selectedPickupVisual;
        Vector3 selectedPickupOrigin;
        float selectedPickupAge, quakePresentationAge;
        int heroCaptureFrame;
        bool heroCapturePending, quakeStructuralShakeActive;
        string presentationHeroA = "bookshelf", presentationHeroB = "fridge";
        PanicPresentationAudio presentationAudio;

        public bool PresentationAudioEnabled { get; private set; }
        public bool PresentationSpeechActive { get { return presentationAudio && presentationAudio.SpeechActive; } }
        public string PresentationSubtitle { get { return presentationAudio ? presentationAudio.Subtitle : string.Empty; } }
        public float PresentationEffectsDuckFactor { get { return presentationAudio ? presentationAudio.EffectsDuckFactor : 1f; } }
        public int PresentationPickupCount { get { return pickupFlights.Count; } }
        public int PresentationHeroCount { get { return presentationHeroes.Count; } }

        // Root enables this only after the real clips have been imported and checked.
        public void ConfigurePresentationAudio(bool enabled)
        {
            PresentationAudioEnabled = enabled;
            if (enabled && !presentationAudio)
            {
                presentationAudio = gameObject.AddComponent<PanicPresentationAudio>();
                presentationAudio.Configure(true);
            }
            else if (presentationAudio) presentationAudio.Configure(enabled);
        }

        public bool PlayPresentationCue(string id)
        {
            return PresentationAudioEnabled && presentationAudio && presentationAudio.TryPlay(id);
        }

        // Optional explicit IDs; never searches active scene objects to infer outcomes.
        public void ConfigurePresentationHeroes(string firstId, string secondId)
        {
            RestoreHeroReactions();
            presentationHeroA = firstId;
            presentationHeroB = secondId;
        }

        void SetPickupFocus(Transform visual)
        {
            selectedPickupVisual = visual;
            selectedPickupOrigin = visual.localPosition;
            selectedPickupAge = 0f;
        }

        void ClearPickupFocus()
        {
            if (selectedPickupVisual) selectedPickupVisual.localPosition = selectedPickupOrigin;
            selectedPickupVisual = null;
        }

        Vector3 SelectedPickupPosition(Vector3 fallback)
        {
            return selectedPickupVisual ? selectedPickupVisual.position : fallback;
        }

        static void MakeCosmeticOnly(GameObject visual)
        {
            // IgnoreRaycast applies only to the cosmetic child. Its stable parent owns picking.
            foreach (var node in visual.GetComponentsInChildren<Transform>(true)) node.gameObject.layer = 2;
            foreach (var collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (var body in visual.GetComponentsInChildren<Rigidbody>(true))
            {
                body.isKinematic = true;
                body.detectCollisions = false;
            }
        }

        void BeginPickupPresentation(string supplyId, Vector3 start, int variant)
        {
            // Rapid input stays immediate and bounded: no gameplay/animation queue.
            if (pickupFlights.Count >= MaxPickupFlights)
            {
                if (pickupFlights[0].Visual) { pickupFlights[0].Visual.SetActive(false); Destroy(pickupFlights[0].Visual); }
                pickupFlights.RemoveAt(0);
            }
            var visual = CreatePreparedPickup(supplyId, variant);
            visual.name = "Cosmetic pickup flight " + supplyId;
            visual.transform.SetParent(transform, true);
            visual.transform.position = start;
            visual.transform.localScale = Vector3.one;
            MakeCosmeticOnly(visual);
            pickupFlights.Add(new PickupFlight { Visual = visual, Start = start });
        }

        Vector3 PickupBagTarget()
        {
            if (!player) return transform.position + Vector3.up;
            if (animator && animator.isHuman)
            {
                var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
                if (chest) return chest.position - player.forward * .16f;
            }
            return player.position + Vector3.up * 1.15f - player.forward * .15f;
        }

        void UpdatePresentation(float dt) => TickPresentation(dt);

        void TickPresentation(float dt)
        {
            bool suspended = InputLocked || (hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame));
            if (presentationAudio) presentationAudio.Tick(dt, suspended);
            if (suspended) return;
            dt = Mathf.Max(0f, dt);
            if (selectedPickupVisual)
            {
                selectedPickupAge += dt;
                // A small free pickup visual moves 0..1.2 cm. Its hit box never moves.
                selectedPickupVisual.localPosition = selectedPickupOrigin + Vector3.up *
                    (Mathf.Sin(selectedPickupAge * 3.5f) * .006f + .006f);
            }
            for (int i = pickupFlights.Count - 1; i >= 0; i--)
            {
                var flight = pickupFlights[i];
                if (!flight.Visual) { pickupFlights.RemoveAt(i); continue; }
                flight.Age += dt;
                float t = Mathf.Clamp01(flight.Age / PickupFlightSeconds);
                float eased = t * t * (3f - 2f * t);
                flight.Visual.transform.position = Vector3.Lerp(flight.Start, PickupBagTarget(), eased) +
                    Vector3.up * Mathf.Sin(t * Mathf.PI) * .24f;
                float shrink = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.24f, 1f, t));
                float stretch = Mathf.Sin(t * Mathf.PI) * .12f;
                flight.Visual.transform.localScale = new Vector3(1f - stretch * .45f, 1f + stretch, 1f - stretch * .45f) * shrink;
                if (t >= 1f) { flight.Visual.SetActive(false); Destroy(flight.Visual); pickupFlights.RemoveAt(i); }
            }
            for (int i = hardwareReveals.Count - 1; i >= 0; i--)
            {
                var reveal = hardwareReveals[i];
                if (!reveal.Target) { hardwareReveals.RemoveAt(i); continue; }
                reveal.Age += dt;
                float t = Mathf.Clamp01(reveal.Age / .22f);
                float scale = t < .65f ? Mathf.Lerp(.90f, 1.035f, t / .65f) : Mathf.Lerp(1.035f, 1f, (t - .65f) / .35f);
                reveal.Target.localScale = reveal.Scale * scale;
                if (t >= 1f) { reveal.Target.localScale = reveal.Scale; hardwareReveals.RemoveAt(i); }
            }
            if (Model != null && Model.Phase == PanicPhase.Disaster && (Model.Disaster == DisasterType.Earthquake || Model.HasActiveDisasterV54(DisasterType.Earthquake)))
            {
                quakePresentationAge += dt;
                // SyncVisuals runs after HomeLife in World.Update. Capture on the next frame
                // so an already committed damage pose is preserved rather than fought.
                if (heroCapturePending && Time.frameCount > heroCaptureFrame)
                {
                    heroCapturePending = false;
                    CaptureHeroReaction(presentationHeroA);
                    if (presentationHeroB != presentationHeroA) CaptureHeroReaction(presentationHeroB);
                }
                float onset = Mathf.SmoothStep(0f, 1f, quakePresentationAge / .9f);
                float progress = JuryActive ? Mathf.Clamp01(quakePresentationAge / 8f) : CinematicProgress;
                float finish = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.78f, 1f, progress));
                float envelope = onset * finish;
                ApplyStructuralQuakeShake(quakePresentationAge, envelope);
                foreach (var reaction in presentationHeroes)
                {
                    if (!reaction.Root) continue;
                    float amplitude = reaction.Anchored ? .65f : 3.5f;
                    float wave = Mathf.Sin(quakePresentationAge * 16f) * .72f + Mathf.Sin(quakePresentationAge * 23f) * .28f;
                    // Any damage used here is the already committed model value, not a second evaluation.
                    float lean = reaction.Anchored ? 0f : Mathf.Clamp01(reaction.ResolvedDamage / 100f) * 3.5f;
                    var turn = Quaternion.Euler((wave * amplitude + lean) * onset * finish, 0f, 0f);
                    Vector3 drift = Vector3.right * (wave * (reaction.Anchored ? .002f : .012f) * onset * finish);
                    foreach (var pose in reaction.Poses)
                    {
                        if (!pose.Target) continue;
                        pose.Target.position = reaction.Root.TransformPoint(turn * pose.RootPosition + drift);
                        pose.Target.rotation = reaction.Root.rotation * turn * pose.RootRotation;
                    }
                }
            }
            else if (quakeStructuralShakeActive)
            {
                ResetStructuralQuakeShake();
            }
        }

        static Vector3 GetFloorRestingLocalPosition(GameObject go, int index)
        {
            if (!go) return Vector3.zero;
            if (go.name.StartsWith("House floor", System.StringComparison.Ordinal))
                return new Vector3(0f, (index - 1) * 3.2f, 0f);
            return Vector3.zero;
        }

        void ApplyStructuralQuakeShake(float age, float envelope)
        {
            if (floorVisuals == null || floorVisuals.Count < 3) return;

            quakeStructuralShakeActive = true;

            // Seismic wave synthesis functions:
            float w1_0 = Mathf.Sin(age * 15.2f) * .70f + Mathf.Sin(age * 21.7f) * .30f;
            float w2_0 = Mathf.Cos(age * 13.1f) * .65f + Mathf.Sin(age * 27.4f) * .35f;
            float p_0 = Mathf.Sin(age * 31.4f) * Mathf.Abs(Mathf.Sin(age * 7.5f));

            // Floor 0 (Basement, delay 0.0s)
            if (floorVisuals[0])
            {
                Vector3 rest0 = GetFloorRestingLocalPosition(floorVisuals[0], 0);
                Vector3 offset0 = new Vector3(w1_0 * .05f, p_0 * .025f, w2_0 * .045f) * envelope;
                floorVisuals[0].transform.localPosition = rest0 + offset0;
                floorVisuals[0].transform.localRotation = Quaternion.Euler(w2_0 * .35f * envelope, w1_0 * .20f * envelope, -w1_0 * .40f * envelope);
            }

            // Floor 1 (Ground Floor, phase lag 0.05s)
            float t1 = age - .05f;
            float w1_1 = Mathf.Sin(t1 * 15.2f) * .70f + Mathf.Sin(t1 * 21.7f) * .30f;
            float w2_1 = Mathf.Cos(t1 * 13.1f) * .65f + Mathf.Sin(t1 * 27.4f) * .35f;
            float p_1 = Mathf.Sin(t1 * 31.4f) * Mathf.Abs(Mathf.Sin(t1 * 7.5f));
            if (floorVisuals[1])
            {
                Vector3 rest1 = GetFloorRestingLocalPosition(floorVisuals[1], 1);
                Vector3 offset1 = new Vector3(w1_1 * .08f, p_1 * .030f, w2_1 * .070f) * envelope;
                floorVisuals[1].transform.localPosition = rest1 + offset1;
                floorVisuals[1].transform.localRotation = Quaternion.Euler(w2_1 * .55f * envelope, w1_1 * .30f * envelope, -w1_1 * .60f * envelope);
            }

            // Floor 2 (Upper Floor, phase lag 0.18s)
            float t2 = age - .18f;
            float w1_2 = Mathf.Sin(t2 * 15.2f) * .70f + Mathf.Sin(t2 * 21.7f) * .30f;
            float w2_2 = Mathf.Cos(t2 * 13.1f) * .65f + Mathf.Sin(t2 * 27.4f) * .35f;
            float p_2 = Mathf.Sin(t2 * 31.4f) * Mathf.Abs(Mathf.Sin(t2 * 7.5f));
            if (floorVisuals[2])
            {
                Vector3 rest2 = GetFloorRestingLocalPosition(floorVisuals[2], 2);
                Vector3 offset2 = new Vector3(w1_2 * .14f, p_2 * .038f, w2_2 * .120f) * envelope;
                floorVisuals[2].transform.localPosition = rest2 + offset2;
                floorVisuals[2].transform.localRotation = Quaternion.Euler(w2_2 * 1.05f * envelope, w1_2 * .55f * envelope, -w1_2 * 1.15f * envelope);
            }

            // Roof (Exterior Roof, phase lag 0.32s)
            if (roof)
            {
                float t3 = age - .32f;
                float w1_3 = Mathf.Sin(t3 * 15.2f) * .70f + Mathf.Sin(t3 * 21.7f) * .30f;
                float w2_3 = Mathf.Cos(t3 * 13.1f) * .65f + Mathf.Sin(t3 * 27.4f) * .35f;
                float p_3 = Mathf.Sin(t3 * 31.4f) * Mathf.Abs(Mathf.Sin(t3 * 7.5f));
                Vector3 offset3 = new Vector3(w1_3 * .19f, p_3 * .045f, w2_3 * .160f) * envelope;
                roof.transform.localPosition = offset3;
                roof.transform.localRotation = Quaternion.Euler(w2_3 * 1.40f * envelope, w1_3 * .75f * envelope, -w1_3 * 1.50f * envelope);
            }
        }

        void ResetStructuralQuakeShake()
        {
            if (floorVisuals != null)
            {
                for (int i = 0; i < floorVisuals.Count; i++)
                {
                    if (!floorVisuals[i]) continue;
                    floorVisuals[i].transform.localPosition = GetFloorRestingLocalPosition(floorVisuals[i], i);
                    floorVisuals[i].transform.localRotation = Quaternion.identity;
                }
            }
            if (roof)
            {
                roof.transform.localPosition = Vector3.zero;
                roof.transform.localRotation = Quaternion.identity;
            }
            quakeStructuralShakeActive = false;
        }

        // ROOT HOOK: call once at the end of RebuildHardware(item, go), after actual geometry exists.
        void OnPresentationHardwareRebuilt(PanicItem item, GameObject holder)
        {
            bool known = presentationAnchored.TryGetValue(item.Id, out bool wasAnchored);
            presentationAnchored[item.Id] = item.Anchored;
            if (!known || wasAnchored || !item.Anchored || !holder) return;
            if (holder.GetComponentsInChildren<Collider>(true).Length != 0) return;
            hardwareReveals.Add(new HardwareReveal { Target = holder.transform, Scale = holder.transform.localScale });
            PlayPresentationCue("anchor_latch");
            PlayPresentationCue("jury_secured");
        }

        void OnPresentationPhase(PanicPhase phase, PanicPhase from)
        {
            RestoreHeroReactions();
            ResetStructuralQuakeShake();
            if (presentationAudio) presentationAudio.CancelSpeech();
            if (phase == PanicPhase.Title) { ResetPresentation(); return; }
            if (phase == PanicPhase.Disaster && Model != null && (Model.Disaster == DisasterType.Earthquake || Model.HasActiveDisasterV54(DisasterType.Earthquake)))
            {
                quakePresentationAge = 0f;
                heroCaptureFrame = Time.frameCount;
                heroCapturePending = true;
            }
            if (phase == PanicPhase.Disaster || phase == PanicPhase.GameOver)
            {
                ClearPickupFlights();
                ClearPickupFocus();
            }
        }

        void CaptureHeroReaction(string id)
        {
            if (string.IsNullOrEmpty(id) || !items.TryGetValue(id, out var go) || !go) return;
            if (Model == null || Model.Items == null) return;
            var item = Model.Items.Find(value => value.Id == id);
            if (item == null || !item.Installed) return;
            var reaction = new HeroReaction { Root = go.transform, Anchored = item.Anchored, ResolvedDamage = item.Damage };
            hardware.TryGetValue(id, out var measures);
            foreach (var renderer in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                Transform node = renderer.transform;
                if (node == go.transform || measures && node.IsChildOf(measures.transform)) continue;
                if (node.GetComponentsInChildren<Collider>(true).Length != 0 || node.GetComponentsInChildren<Rigidbody>(true).Length != 0) continue;
                bool nestedRenderer = false;
                for (var ancestor = node.parent; ancestor && ancestor != go.transform; ancestor = ancestor.parent)
                    if (ancestor.GetComponent<Renderer>()) { nestedRenderer = true; break; }
                if (nestedRenderer) continue;
                reaction.Poses.Add(new VisualPose { Target = node, Position = node.localPosition, Rotation = node.localRotation,
                    RootPosition = go.transform.InverseTransformPoint(node.position), RootRotation = Quaternion.Inverse(go.transform.rotation) * node.rotation });
            }
            if (reaction.Poses.Count != 0) presentationHeroes.Add(reaction);
        }

        void RestoreHeroReactions()
        {
            heroCapturePending = false;
            foreach (var reaction in presentationHeroes)
                foreach (var pose in reaction.Poses)
                    if (pose.Target) { pose.Target.localPosition = pose.Position; pose.Target.localRotation = pose.Rotation; }
            presentationHeroes.Clear();
            ResetStructuralQuakeShake();
        }

        void ClearPickupFlights()
        {
            foreach (var flight in pickupFlights) if (flight.Visual) { flight.Visual.SetActive(false); Destroy(flight.Visual); }
            pickupFlights.Clear();
        }

        void ResetPresentation()
        {
            ClearPickupFocus();
            ClearPickupFlights();
            RestoreHeroReactions();
            ResetStructuralQuakeShake();
            foreach (var reveal in hardwareReveals) if (reveal.Target) reveal.Target.localScale = reveal.Scale;
            hardwareReveals.Clear();
            presentationAnchored.Clear();
            if (Model != null && Model.Items != null)
            {
                foreach (var item in Model.Items) presentationAnchored[item.Id] = item.Anchored;
            }
            if (presentationAudio) presentationAudio.ResetRun();
            ClearInteractionOutline();
        }

        void OnDisable()
        {
            SuspendExperienceFeedbackV414();
            // Stop/restore our own presentation only. Unity/root remains lifecycle owner.
            ResetPresentation();
        }
    }
}
