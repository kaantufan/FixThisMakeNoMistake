using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Offline, opt-in short cues. Never advances the model or blocks input.</summary>
    public sealed class PanicPresentationAudio : MonoBehaviour
    {
        sealed class Cue
        {
            public readonly string Id, Text;
            public readonly int Priority;
            public readonly bool Speech;
            public readonly float MaxSeconds, Cooldown;
            public Cue(string id, string text, int priority, bool speech, float maxSeconds, float cooldown)
            { Id = id; Text = text; Priority = priority; Speech = speech; MaxSeconds = maxSeconds; Cooldown = cooldown; }
        }

        static readonly Dictionary<string, Cue> Cues = new Dictionary<string, Cue>
        {
            { "jury_intro", new Cue("jury_intro", PanicKktcStoryV53.ShortFlowPremise(), 10, true, 5f, 0f) },
            { "jury_bag_ready", new Cue("jury_bag_ready", "Çantan hazır. İşaretli güvenli noktaya geç.", 20, true, 5f, 0f) },
            { "jury_secured", new Cue("jury_secured", "Sabitleme tamam. Sağlam bir adım attın.", 25, true, 5f, 0f) },
            { "jury_quake_result", new Cue("jury_quake_result", "Hazırlığın farkını şimdi görebilirsin.", 40, true, 5f, 0f) },
            { "jury_flood", new Cue("jury_flood", "Suya karşı önlemini seç.", 30, true, 4f, 0f) },
            { "jury_fire", new Cue("jury_fire", "Yangına karşı çıkışı güvenli tut.", 30, true, 5f, 0f) },
            { "jury_end", new Cue("jury_end", "Hangi önlem en çok fark yarattı?", 45, true, 5f, 0f) },
            { "pickup_soft", new Cue("pickup_soft", "", 5, false, 1f, .10f) },
            { "anchor_latch", new Cue("anchor_latch", "", 15, false, 1f, .20f) },
            { "objective_tick", new Cue("objective_tick", "", 10, false, 1f, .25f) }
        };

        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly Dictionary<string, float> lastPlayed = new Dictionary<string, float>();
        readonly HashSet<string> spokenThisRun = new HashSet<string>();
        readonly HashSet<string> reportedMissing = new HashSet<string>();
        AudioSource speech, sfx;
        bool configured, pausedByWorld;
        float clock, subtitleRemaining, duck = 1f;
        int currentPriority;

        public bool Muted { get; set; }
        public float SpeechVolume { get; set; } = .78f;
        public float SfxVolume { get; set; } = .40f;
        string fullSubtitle = string.Empty; int revealed; float typingClock;
        public string Subtitle { get; private set; } = string.Empty;
        public bool SpeechActive { get { return configured && speech && speech.clip && (speech.isPlaying || pausedByWorld); } }
        public float EffectsDuckFactor { get { return duck; } }
        public bool HasRealClip(string id) { return FindClip(id) != null; }

        void Awake()
        {
            speech = MakeSource("Short Turkish cues");
            sfx = MakeSource("Short interaction cues");
        }

        AudioSource MakeSource(string sourceName)
        {
            var node = new GameObject(sourceName);
            node.transform.SetParent(transform, false);
            var source = node.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.priority = sourceName == "Short Turkish cues" ? 96 : 128;
            return source;
        }

        public void Configure(bool enabled)
        {
            configured = enabled;
            if (!enabled) ResetRun();
        }

        AudioClip FindClip(string id)
        {
            if (string.IsNullOrEmpty(id) || !Cues.TryGetValue(id, out var cue)) return null;
            if (clips.TryGetValue(id, out var cached)) return cached;
            var clip = Resources.Load<AudioClip>("Audio/Jury/" + id);
            if (clip && (clip.length <= .02f || clip.length > cue.MaxSeconds || clip.loadState == AudioDataLoadState.Failed))
            {
                if (reportedMissing.Add(id)) Debug.LogWarning("JURY_AUDIO_REJECTED " + id + " length=" + clip.length);
                clip = null;
            }
            else if (!clip && reportedMissing.Add(id)) Debug.LogWarning("JURY_AUDIO_MISSING " + id + " (silent fallback)");
            clips[id] = clip;
            return clip;
        }

        public bool TryPlay(string id)
        {
            if (!configured || pausedByWorld || string.IsNullOrEmpty(id) || !Cues.TryGetValue(id, out var cue)) return false;
            if (cue.Speech)
            {
                if (spokenThisRun.Contains(id)) return false;
                // No deferred speech queue. A newer, higher-priority event can replace stale speech.
                if (subtitleRemaining > 0f && currentPriority > cue.Priority) return false;
                CancelSpeech();
                currentPriority = cue.Priority;
                spokenThisRun.Add(id);
                var clip = PanicAudioBank.Get("dialogue_blip");
                fullSubtitle = cue.Text; revealed = 0; typingClock = 0; Subtitle = string.Empty;
                subtitleRemaining = Mathf.Max(3f, cue.Text.Length / 32f + 1.8f);
                if (clip && !Muted) { speech.clip = clip; speech.volume = .20f * PanicAudioSettingsV56.EffectiveSfxGain; }
                return true;
            }

            if (lastPlayed.TryGetValue(id, out float previous) && clock - previous < cue.Cooldown) return false;
            var sound = FindClip(id);
            lastPlayed[id] = clock;
            if (!sound || Muted) return false;
            // A single bounded SFX channel avoids one-shot accumulation under rapid pickup input.
            sfx.Stop();
            sfx.clip = sound;
            sfx.volume = Mathf.Clamp01(SfxVolume) * PanicAudioSettingsV56.EffectiveSfxGain * (SpeechActive ? .62f : 1f);
            sfx.Play();
            return true;
        }

        public void Tick(float dt, bool suspended)
        {
            if (!configured) return;
            if (suspended != pausedByWorld)
            {
                pausedByWorld = suspended;
                if (suspended) { speech.Pause(); sfx.Pause(); }
                else { speech.UnPause(); sfx.UnPause(); }
            }
            if (suspended) return;
            dt = Mathf.Max(0f, dt);
            clock += dt;
            typingClock += dt;
            if (revealed < fullSubtitle.Length && typingClock >= .03125f)
            {
                int before = revealed; revealed = Mathf.Min(fullSubtitle.Length, revealed + Mathf.Max(1, Mathf.FloorToInt(typingClock * 32f))); typingClock %= .03125f;
                Subtitle = fullSubtitle.Substring(0, revealed);
                char c = fullSubtitle[revealed-1];
                if (!Muted && char.IsLetterOrDigit(c) && revealed / 2 != before / 2 && speech && speech.clip)
                { speech.pitch = 1f + (revealed % 3 - 1) * .07f; speech.Play(); }
                if (c == '.' || c == ',' || c == '?' || c == '!') typingClock = -.12f;
            }
            if (Muted) { speech.Stop(); sfx.Stop(); speech.clip = null; }
            subtitleRemaining = Mathf.Max(0f, subtitleRemaining - dt);
            if (subtitleRemaining <= 0f) { Subtitle = string.Empty; currentPriority = 0; }
            bool speaking = speech && speech.isPlaying;
            duck = Mathf.MoveTowards(duck, speaking ? .55f : 1f, dt * (speaking ? 8f : 4f));
            sfx.volume = Mathf.Clamp01(SfxVolume) * PanicAudioSettingsV56.EffectiveSfxGain * (speaking ? .62f : 1f);
            if (speech) speech.volume = .20f * PanicAudioSettingsV56.EffectiveSfxGain;
            if (speech && revealed >= fullSubtitle.Length && !speech.isPlaying) speech.clip = null;
        }

        public void CancelSpeech()
        {
            if (speech) { speech.Stop(); speech.clip = null; }
            Subtitle = string.Empty; fullSubtitle = string.Empty; revealed = 0; typingClock = 0;
            subtitleRemaining = 0f;
            currentPriority = 0;
        }

        public void ResetRun()
        {
            CancelSpeech();
            if (sfx) { sfx.Stop(); sfx.clip = null; }
            spokenThisRun.Clear();
            lastPlayed.Clear();
            clock = 0f;
            pausedByWorld = false;
            duck = 1f;
        }

        void OnDisable() { ResetRun(); }
    }
}
