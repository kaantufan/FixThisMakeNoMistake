using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Offline captions with a short nonverbal bit signature. Never changes model state or input.</summary>
    public sealed class PanicPresentationAudio : MonoBehaviour
    {
        sealed class Cue
        {
            public readonly string Text;
            public readonly int Priority;
            public readonly float Cooldown;
            public Cue(string text, int priority, float cooldown = 0f)
            { Text = text; Priority = priority; Cooldown = cooldown; }
        }
        static readonly Dictionary<string, Cue> Cues = new Dictionary<string, Cue>
        {
            { "jury_intro", new Cue(PanicKktcStoryV53.ShortFlowPremise(), 10) },
            { "jury_bag_ready", new Cue("Su ve gıda hazır. Kısa akışın güvenli noktasında E ile gir.", 20) },
            { "jury_secured", new Cue("Sabitleme tamam. Sağlam bir adım attın.", 25) },
            { "jury_quake_result", new Cue("Hazırlığın farkını şimdi görebilirsin.", 40) },
            { "jury_flood", new Cue("Suya karşı önlemini seç.", 30) },
            { "jury_fire", new Cue("Yangına karşı çıkışı güvenli tut.", 30) },
            { "jury_end", new Cue("Hangi önlem en çok fark yarattı?", 45) },
            { "pickup_soft", new Cue("", 5, .10f) },
            { "anchor_latch", new Cue("", 15, .20f) },
            { "objective_tick", new Cue("", 10, .25f) }
        };
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly Dictionary<string, float> lastPlayed = new Dictionary<string, float>();
        readonly HashSet<string> reportedMissing = new HashSet<string>();
        readonly PanicDialogueSessionV59 session = new PanicDialogueSessionV59();
        AudioSource speech, sfx;
        AudioClip bit;
        bool configured, pausedByWorld, muted, bitLoaded;
        float clock, duck = 1f;
        public bool Muted
        {
            get { return muted; }
            set { muted = value; if (value) { if (speech) speech.Stop(); if (sfx) sfx.Stop(); } }
        }
        public float SpeechVolume { get; set; } = .78f;
        public float SfxVolume { get; set; } = .40f;
        public string Subtitle { get { return session.Text; } }
        public string SubtitleKeyV59 { get { return session.Key; } }
        public float SubtitleRemainingV59 { get { return session.Remaining; } }
        public int PendingCaptionCountV59 { get { return session.PendingCount; } }
        public int BitDispatchCountV59 { get; private set; }
        public string BitResourceV59 { get; private set; } = "";
        public bool SpeechActive
        { get { return configured && !muted && !pausedByWorld && PanicAudioSettingsV56.EffectiveSfxGain > 0f && speech && speech.isPlaying; } }
        public float EffectsDuckFactor { get { return duck; } }
        // Speech cue assets are deliberately never loaded: spoken originals remain in Deferred.
        public bool HasRealClip(string id)
        { return Cues.TryGetValue(id ?? "", out var cue) && cue.Text.Length > 0 ? FindBit() != null : FindClip(id) != null; }

        void Awake()
        {
            speech = MakeSource("Nonverbal dialogue bits V5.9", 112);
            sfx = MakeSource("Short interaction cues", 128);
        }
        AudioSource MakeSource(string name, int priority)
        {
            var node = new GameObject(name); node.transform.SetParent(transform, false);
            var source = node.AddComponent<AudioSource>();
            source.playOnAwake = false; source.loop = false; source.spatialBlend = 0f;
            source.dopplerLevel = 0f; source.priority = priority;
            source.ignoreListenerPause = false; source.ignoreListenerVolume = false;
            return source;
        }
        public void Configure(bool enabled)
        { configured = enabled; if (!enabled) ResetRun(); }
        AudioClip FindBit()
        {
            if (bitLoaded) return bit;
            bitLoaded = true;
            bit = Resources.Load<AudioClip>("Audio/V59/dialogue_bit");
            if (ValidBit(bit)) BitResourceV59 = "Audio/V59/dialogue_bit";
            else
            {
                bit = Resources.Load<AudioClip>("Audio/Foley/dialogue_blip");
                if (ValidBit(bit)) BitResourceV59 = "Audio/Foley/dialogue_blip";
                else { bit = null; BitResourceV59 = "caption-only: missing bounded nonverbal asset"; }
            }
            return bit;
        }
        static bool ValidBit(AudioClip value)
        { return value && value.length >= .025f && value.length <= .18f && value.loadState != AudioDataLoadState.Failed; }
        AudioClip FindClip(string id)
        {
            if (string.IsNullOrEmpty(id) || !Cues.TryGetValue(id, out var cue) || cue.Text.Length > 0) return null;
            if (clips.TryGetValue(id, out var cached)) return cached;
            var clip = Resources.Load<AudioClip>("Audio/Jury/" + id);
            if (clip && (clip.length <= .02f || clip.length > 1.5f || clip.loadState == AudioDataLoadState.Failed)) clip = null;
            if (!clip && reportedMissing.Add(id)) Debug.LogWarning("PRESENTATION_AUDIO_MISSING " + id + " (silent fallback)");
            clips[id] = clip; return clip;
        }
        public void BeginScopeV59(string value)
        { CancelSpeech(); session.BeginScope(value); lastPlayed.Clear(); }
        public bool TryCaptionV59(string key, string text, int priority = 20, float lifetime = 0f)
        {
            if (!configured || pausedByWorld || !session.TryStart(key, text, priority, lifetime)) return false;
            if (speech) speech.Stop();
            FindBit();
            return true;
        }
        public bool TryPlay(string id)
        {
            if (!configured || pausedByWorld || string.IsNullOrEmpty(id) || !Cues.TryGetValue(id, out var cue)) return false;
            if (cue.Text.Length > 0) return TryCaptionV59(id, cue.Text, cue.Priority);
            if (lastPlayed.TryGetValue(id, out float previous) && clock - previous < cue.Cooldown) return false;
            lastPlayed[id] = clock;
            var sound = FindClip(id);
            if (!sound || muted || PanicAudioSettingsV56.EffectiveSfxGain <= 0f) return false;
            sfx.Stop(); sfx.clip = sound; sfx.volume = Mathf.Clamp01(SfxVolume) * PanicAudioSettingsV56.EffectiveSfxGain;
            sfx.Play(); return true;
        }
        public void Tick(float dt, bool suspended)
        {
            if (!configured) return;
            float gain = PanicAudioSettingsV56.EffectiveSfxGain;
            bool silence = muted || gain <= 0f;
            if (silence) { if (speech) speech.Stop(); if (sfx) sfx.Stop(); }
            if (suspended != pausedByWorld)
            {
                pausedByWorld = suspended;
                // Bits are transient: do not resume half an old bit or old pickup after a modal closes.
                if (suspended) { speech.Stop(); sfx.Stop(); }
            }
            speech.volume = .11f * Mathf.Clamp01(SpeechVolume) * gain;
            sfx.volume = Mathf.Clamp01(SfxVolume) * gain;
            if (suspended) { duck = 1f; return; }
            if (float.IsNaN(dt) || float.IsInfinity(dt)) dt = 0f;
            dt = Mathf.Clamp(dt, 0f, .1f); clock += dt; session.Tick(dt, false);
            bool play = session.TakeBlip(!silence && bit);
            if (play)
            {
                speech.Stop(); speech.clip = bit;
                speech.pitch = 1f + (session.BlipsEmitted % 3 - 1) * .035f;
                speech.Play(); BitDispatchCountV59++;
            }
            duck = Mathf.MoveTowards(duck, SpeechActive ? .82f : 1f, dt * 4f);
        }
        public void CancelSpeech()
        { if (speech) { speech.Stop(); speech.clip = null; } session.Cancel(); duck = 1f; }
        public void ResetRun()
        {
            CancelSpeech(); session.Reset();
            if (sfx) { sfx.Stop(); sfx.clip = null; }
            lastPlayed.Clear(); clock = 0f; pausedByWorld = false; duck = 1f; BitDispatchCountV59 = 0;
        }
        void OnDisable() { ResetRun(); }
    }
}
