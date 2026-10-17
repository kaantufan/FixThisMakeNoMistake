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
        bool heroCapturePending;
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
            if (Model.Phase == PanicPhase.Disaster && Model.Disaster == DisasterType.Earthquake)
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
            if (presentationAudio) presentationAudio.CancelSpeech();
            if (phase == PanicPhase.Title) { ResetPresentation(); return; }
            if (phase == PanicPhase.Disaster && Model.Disaster == DisasterType.Earthquake)
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
            foreach (var reveal in hardwareReveals) if (reveal.Target) reveal.Target.localScale = reveal.Scale;
            hardwareReveals.Clear();
            presentationAnchored.Clear();
            if (Model != null) foreach (var item in Model.Items) presentationAnchored[item.Id] = item.Anchored;
            if (presentationAudio) presentationAudio.ResetRun();
            ClearInteractionOutline();
        }

        void OnDisable()
        {
            // Stop/restore our own presentation only. Unity/root remains lifecycle owner.
            ResetPresentation();
        }
    }
}
