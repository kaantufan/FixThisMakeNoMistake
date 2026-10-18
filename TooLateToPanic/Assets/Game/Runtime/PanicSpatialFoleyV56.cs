using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Manual player-relative stereo: the high isometric camera never changes audibility distance.</summary>
    public static class PanicSpatialFoleyV56
    {
        public static bool Suspended(PanicHomeWorld w)
        { return !w || !w.isActiveAndEnabled || w.Model == null || w.Model.Phase == PanicPhase.Title || w.Model.IsPaused || w.Model.WaveLedgerOpenV56 || w.Model.KeyboardHelpOpenV56 || w.NotebookBlocksInput || w.InputLocked || (!Application.isFocused && !Application.runInBackground); }
        public static float Gain(PanicHomeWorld w, Vector3 position, float near, float far, out float pan, out float cutoff)
        {
            Vector3 delta = position - w.PlayerPosition;
            float distance = delta.magnitude, shelter = w.AudioShelterAmount;
            pan = w.View && distance > .001f ? Mathf.Clamp(Vector3.Dot(delta / distance, w.View.transform.right) * .65f, -.65f, .65f) : 0;
            cutoff = Mathf.Lerp(Mathf.Lerp(14000, 2400, Mathf.Clamp01(distance / far)), 550, shelter);
            return PanicHomeEffects.DistanceGain(distance, near, far) * Mathf.Lerp(1f, .04f, shelter);
        }
        public static AudioSource Source(Transform parent, string name, string id, bool loop, out AudioLowPassFilter filter)
        {
            var node = new GameObject(name); node.transform.SetParent(parent, false);
            var s = node.AddComponent<AudioSource>(); s.playOnAwake = false; s.loop = loop; s.spatialBlend = 0f;
            s.dopplerLevel = 0; s.priority = 200; s.volume = 0; s.clip = PanicAudioBank.Get(id);
            s.ignoreListenerVolume = false; s.ignoreListenerPause = false;
            filter = node.AddComponent<AudioLowPassFilter>(); filter.cutoffFrequency = 22000;
            return s;
        }
        public static void Stop(AudioSource source) { if (source) { source.Stop(); source.volume = 0; } }
    }

    /// <summary>Two cached local sources, one footfall per travelled stride, cloth every second stride. No idle speech.</summary>
    public sealed class PanicNpcFoleyV56 : MonoBehaviour
    {
        PanicHomeWorld world;
        AudioSource step, cloth;
        AudioLowPassFilter stepFilter, clothFilter;
        Vector3 previous;
        float travelled, stepGain, clothGain;
        int steps;
        bool bound;
        public bool IsPlayingV56 { get { return step && step.isPlaying || cloth && cloth.isPlaying; } }
        public float OutputGainV56 { get { return (step && step.isPlaying ? step.volume : 0) + (cloth && cloth.isPlaying ? cloth.volume : 0); } }
        public int FootfallCountV56 { get; private set; }
        public void Bind(PanicHomeWorld owner)
        {
            if (bound) return; world = owner; previous = transform.position; bound = true;
            step = PanicSpatialFoleyV56.Source(transform, "V56 NPC footfall", "step_concrete", false, out stepFilter);
            cloth = PanicSpatialFoleyV56.Source(transform, "V56 NPC cloth", "v56_cloth", false, out clothFilter);
        }
        void OnEnable() { previous = transform.position; travelled = 0; }
        void LateUpdate()
        {
            Vector3 now = transform.position, delta = now - previous; previous = now; delta.y = 0;
            if (!bound || PanicSpatialFoleyV56.Suspended(world)) { Stop(); return; }
            float gain = PanicSpatialFoleyV56.Gain(world, now, 2f, 28f, out float pan, out float cutoff);
            step.panStereo = cloth.panStereo = pan; stepFilter.cutoffFrequency = clothFilter.cutoffFrequency = cutoff;
            float settings = PanicAudioSettingsV56.EffectiveSfxGain;
            step.volume = stepGain * gain * settings; cloth.volume = clothGain * gain * settings;
            float distance = delta.magnitude;
            if (distance <= .0001f) return;
            if (distance > .5f) { travelled = 0; return; } // Spawn, path wrap, fixture: never emit a burst of artificial steps.
            travelled += distance;
            if (travelled < .62f) return;
            travelled %= .62f; steps++;
            if (gain <= .0001f || settings <= 0) return;
            string surface = world.FootstepSurfaceAt(now);
            step.clip = PanicAudioBank.Get(surface); stepGain = .075f;
            step.volume = stepGain * gain * settings; step.pitch = steps % 2 == 0 ? .96f : 1.03f;
            if (step.clip) { step.Play(); FootfallCountV56++; }
            if ((steps & 1) == 0 && cloth.clip)
            { clothGain = .050f; cloth.volume = clothGain * gain * settings; cloth.Play(); }
        }
        void Stop() { PanicSpatialFoleyV56.Stop(step); PanicSpatialFoleyV56.Stop(cloth); travelled = 0; }
        void OnDisable() { Stop(); }
    }

    /// <summary>Two bounded looping layers. Only real horizontal boat movement powers motor/water sound.</summary>
    public sealed class PanicZodiacAudioV56 : MonoBehaviour
    {
        PanicHomeWorld world;
        AudioSource motor, water;
        AudioLowPassFilter motorFilter, waterFilter;
        Vector3 previous;
        float motorGain, waterGain;
        bool bound;
        public bool IsPlayingV56 { get { return motor && motor.isPlaying || water && water.isPlaying; } }
        public float OutputGainV56 { get { return (motor && motor.isPlaying ? motor.volume : 0) + (water && water.isPlaying ? water.volume : 0); } }
        public AudioSource MotorSourceV56 { get { return motor; } }
        public AudioSource WaterSourceV56 { get { return water; } }
        public void Bind(PanicHomeWorld owner)
        {
            if (bound) return; world = owner; previous = transform.position; bound = true;
            motor = PanicSpatialFoleyV56.Source(transform, "V56 zodiac motor", "v56_zodiac_motor", true, out motorFilter);
            water = PanicSpatialFoleyV56.Source(transform, "V56 zodiac water", "v56_zodiac_water", true, out waterFilter);
        }
        void OnEnable() { previous = transform.position; }
        void LateUpdate()
        {
            Vector3 now = transform.position, delta = now - previous; previous = now; delta.y = 0;
            if (!bound || PanicSpatialFoleyV56.Suspended(world)) { Stop(); return; }
            float dt = Time.deltaTime;
            float speed = dt > .00001f ? Mathf.Min(2f, delta.magnitude / dt) : 0;
            float gain = PanicSpatialFoleyV56.Gain(world, now, 3f, 38f, out float pan, out float cutoff);
            motor.panStereo = water.panStereo = pan; motorFilter.cutoffFrequency = waterFilter.cutoffFrequency = cutoff;
            float moving = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.025f, .85f, speed));
            motorGain = Mathf.MoveTowards(motorGain, .10f * moving * gain, Mathf.Max(0, dt) * .35f);
            waterGain = Mathf.MoveTowards(waterGain, .075f * moving * gain, Mathf.Max(0, dt) * .35f);
            Mix(motor, motorGain); Mix(water, waterGain);
            motor.pitch = Mathf.Lerp(.84f, 1.08f, moving); water.pitch = Mathf.Lerp(.90f, 1.02f, moving);
        }
        static void Mix(AudioSource source, float raw)
        {
            source.volume = raw * PanicAudioSettingsV56.EffectiveSfxGain;
            if (raw <= .00001f || PanicAudioSettingsV56.EffectiveSfxGain <= 0) { source.Pause(); return; }
            if (source.clip && !source.isPlaying) { source.UnPause(); if (!source.isPlaying) source.Play(); }
        }
        void Stop() { motorGain = waterGain = 0; PanicSpatialFoleyV56.Stop(motor); PanicSpatialFoleyV56.Stop(water); }
        void OnDisable() { Stop(); }
    }
}
