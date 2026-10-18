using System;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        AudioSource fpsTensionSourceV55;
        AudioClip fpsTensionClipV55;
        float fpsTensionFadeV55;
        public bool FpsTensionPlayingV55 { get { return fpsTensionSourceV55 && fpsTensionSourceV55.isPlaying; } }
        public float FpsTensionVolumeV55 { get { return FpsTensionPlayingV55 ? fpsTensionSourceV55.volume : 0f; } }
        public AudioClip FpsTensionClipV55 { get { return fpsTensionClipV55; } }
        // Existing background-music owner uses this to yield its two sources for the
        // whole FPS stage, including a modal/transition. No duplicate music bed.
        public bool FpsTensionOwnsMusicV55 { get { return BagFinalStageActive && (BagFirstPersonActive || BagHandoffActiveV51); } }
        public bool FpsTensionAllowedV55
        {
            get
            {
                return isActiveAndEnabled && FpsTensionOwnsMusicV55 && Model != null && Model.Phase == PanicPhase.Preparing &&
                    !ExperienceFeedbackBlockedV414 && !BagCameraTransitionActive && !BagHandoffActiveV51 &&
                    PanicBackgroundMusic.Enabled && PanicBackgroundMusic.Volume > 0f && PanicAudioSettingsV56.EffectiveMusicGain > 0f &&
                    (Application.isFocused || Application.runInBackground);
            }
        }

        public void TickFpsTensionV55(float dt)
        {
            // Forbidden-state gates are immediate: pause/menu/notebook/inventory may
            // never retain a playing source. Every permitted restart fades in 0.25 s.
            if (!FpsTensionAllowedV55) { StopFpsTensionV55(); return; }
            if (float.IsNaN(dt) || float.IsInfinity(dt)) dt = 0f;
            dt = Mathf.Clamp(dt, 0f, .1f);
            InitFpsTensionV55();
            if (!fpsTensionSourceV55.isPlaying)
            {
                fpsTensionFadeV55 = 0f;
                fpsTensionSourceV55.volume = 0f;
                fpsTensionSourceV55.Play();
            }
            fpsTensionFadeV55 = Mathf.MoveTowards(fpsTensionFadeV55, 1f, dt / .25f);
            float gain = Mathf.Clamp(PanicBackgroundMusic.Volume, 0f, .12f) * Mathf.Lerp(.90f, 1.15f, FpsStressIntensityV414);
            if (PresentationSpeechActive) gain *= .55f;
            float deadlineFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Model.TimeRemaining / .30f));
            fpsTensionSourceV55.volume = Mathf.Min(.12f, gain) * PanicAudioSettingsV56.EffectiveMusicGain * Mathf.SmoothStep(0f, 1f, fpsTensionFadeV55) * deadlineFade;
        }

        // Prewarm at normal feedback initialization, before FPS entry, so synthesis
        // cannot introduce a frame hitch when the black handoff reveals the scene.
        void InitFpsTensionV55()
        {
            if (fpsTensionSourceV55) return;
                fpsTensionClipV55 = MakeFpsTensionMusicV55();
                var node = new GameObject("V55 original FPS tension score");
                node.transform.SetParent(transform, false);
                node.AddComponent<PanicFpsTensionGuardV55>().Owner = this;
                fpsTensionSourceV55 = node.AddComponent<AudioSource>();
                fpsTensionSourceV55.clip = fpsTensionClipV55;
                fpsTensionSourceV55.loop = true;
                fpsTensionSourceV55.playOnAwake = false;
                fpsTensionSourceV55.spatialBlend = 0f;
                fpsTensionSourceV55.dopplerLevel = 0f;
                fpsTensionSourceV55.priority = 180;
                fpsTensionSourceV55.volume = 0f;
                fpsTensionSourceV55.ignoreListenerVolume = false;
                fpsTensionSourceV55.ignoreListenerPause = false;
        }

        public void StopFpsTensionV55()
        {
            fpsTensionFadeV55 = 0f;
            if (!fpsTensionSourceV55) return;
            fpsTensionSourceV55.volume = 0f;
            fpsTensionSourceV55.Stop();
        }
        public void DisposeFpsTensionV55()
        {
            StopFpsTensionV55();
            if (fpsTensionSourceV55) Destroy(fpsTensionSourceV55.gameObject);
            if (fpsTensionClipV55) Destroy(fpsTensionClipV55);
            fpsTensionSourceV55 = null;
            fpsTensionClipV55 = null;
        }

        /// <summary>
        /// Original offline instrumental score: bowed harmonic E-minor pedal with a
        /// quiet chromatic tension, plus a 90-BPM pitched plucked ostinato. No noise,
        /// heartbeat, voice, external recording, provider, download, or random seed.
        /// All oscillators and overlapping note tails repeat over exactly 16 seconds.
        /// </summary>
        static AudioClip MakeFpsTensionMusicV55()
        {
            const int rate = 22050, seconds = 16;
            const double duration = seconds, tau = Math.PI * 2.0;
            int[] motif = { 52, 59, 55, 58, 52, 62, 59, 58 };
            int[] pedal = { 40, 47, 55 };
            var data = new float[rate * seconds];
            double peak = 0.0;
            for (int i = 0; i < data.Length; i++)
            {
                double t = i / (double)rate;
                double signal = 0.0;
                for (int p = 0; p < pedal.Length; p++)
                {
                    double f = LoopNoteFrequencyV55(pedal[p], duration);
                    double bow = .82 + .18 * Math.Sin(tau * t * (p + 1) / duration);
                    signal += (.12 / (p + 1)) * bow * (Math.Sin(tau * f * t) + .24 * Math.Sin(tau * f * 2 * t) + .10 * Math.Sin(tau * f * 3 * t));
                }
                // Twenty-four 2/3-second beats. Modular age carries the last note's
                // tail across the loop, so neither the pad nor melody resets at seam.
                for (int step = 0; step < 24; step++)
                {
                    double age = (t - step * (2.0 / 3.0) + duration) % duration;
                    if (age > 1.30) continue;
                    double f = LoopNoteFrequencyV55(motif[step % motif.Length], duration);
                    double envelope = (1 - Math.Exp(-age * 65)) * Math.Exp(-age * 4.2);
                    double tail = Math.Min(1.0, (1.30 - age) / .18);
                    envelope *= tail * tail * (3 - 2 * tail);
                    double tone = Math.Sin(tau * f * t) + .31 * Math.Sin(tau * f * 2 * t) + .09 * Math.Sin(tau * f * 4 * t);
                    signal += .105 * envelope * tone;
                }
                // Smooth saturation limits combined harmonics before a conservative
                // clip-level peak cap; AudioSource then follows existing music volume.
                double sample = Math.Tanh(signal * 1.15) * .72;
                data[i] = (float)sample;
                peak = Math.Max(peak, Math.Abs(sample));
            }
            if (peak > .00001)
            {
                float gain = (float)(.55 / peak);
                for (int i = 0; i < data.Length; i++) data[i] *= gain;
            }
            var clip = AudioClip.Create("V55_Original_EMinor_Tension_16s", data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
        static double LoopNoteFrequencyV55(int midi, double loopSeconds)
        {
            return Math.Round(440.0 * Math.Pow(2.0, (midi - 69) / 12.0) * loopSeconds) / loopSeconds;
        }
    }

    // A disabled World component does not disable sibling components. This guard
    // still stops the source if the world can no longer run its normal tick.
    public sealed class PanicFpsTensionGuardV55 : MonoBehaviour
    {
        public PanicHomeWorld Owner;
        void Update() { if (Owner && !Owner.FpsTensionAllowedV55) Owner.StopFpsTensionV55(); }
        void OnDisable() { if (Owner) Owner.StopFpsTensionV55(); }
    }
}
