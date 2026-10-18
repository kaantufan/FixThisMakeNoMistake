using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>The single imported music owner, including FPS. Never changes gameplay clocks, input or saves.</summary>
    public sealed class PanicBackgroundMusic : MonoBehaviour
    {
        public static bool Enabled { get { return PanicAudioSettingsV56.MusicEnabled; } set { PanicAudioSettingsV56.MusicEnabled = value; } }
        public static float Volume { get; set; } = PanicMusicMixPolicyV59.DefaultVolume;
        public static PanicBackgroundMusic Instance { get; private set; }
        public string CurrentTrack { get; private set; }
        public bool IsAudible { get { return !paused && Enabled && PanicAudioSettingsV56.EffectiveMusicGain>0f && (a&&a.isPlaying&&a.volume>0f||b&&b.isPlaying&&b.volume>0f); } }
        public static readonly string[] TrackIds = { "preparation", "disaster", "fps_tension" };
        readonly Dictionary<string,AudioClip> clips = new Dictionary<string,AudioClip>();
        AudioSource a,b,current,outgoing;
        PanicHomeWorld world;
        bool paused;
        float fadeAge=PanicMusicMixPolicyV59.CrossfadeSeconds,resumeAge=PanicMusicMixPolicyV59.CrossfadeSeconds;
        string outgoingTrack;
        public bool FpsTrackPlayingV59 { get { return !paused&&CurrentTrack=="fps_tension"&&current&&current.isPlaying&&current.volume>0f; } }
        public float FpsTrackVolumeV59 { get { return FpsTrackPlayingV59?current.volume:0f; } }
        public AudioClip FpsTrackClipV59 { get { return CurrentTrack=="fps_tension"&&current?current.clip:LoadTrack("fps_tension"); } }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        { if(FindFirstObjectByType<PanicBackgroundMusic>())return;new GameObject("Background music").AddComponent<PanicBackgroundMusic>(); }
        void Awake()
        {
            Instance=this;a=Source();b=Source();current=a;
            var filter=gameObject.AddComponent<AudioLowPassFilter>();filter.cutoffFrequency=6500f;filter.lowpassResonanceQ=1f;
        }
        AudioSource Source()
        {
            var s=gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=false;s.spatialBlend=0;s.priority=180;s.volume=0;s.dopplerLevel=0;
            s.ignoreListenerPause=false;s.ignoreListenerVolume=false;
            return s;
        }
        public static AudioClip LoadTrack(string id) { return Resources.Load<AudioClip>("Audio/Music/"+id); }
        bool FpsReady { get { return world&&world.BagFirstPersonActive&&world.BagFinalStageActive&&!world.BagCameraTransitionActive&&!world.BagHandoffActiveV51; } }
        void Transition(string id)
        {
            if(!clips.TryGetValue(id,out var clip)){clip=LoadTrack(id);clips[id]=clip;}
            if(!clip)return;
            var next=current==a?b:a;next.Stop();next.clip=clip;next.time=0;next.volume=0;next.Play();
            outgoing=current;outgoingTrack=CurrentTrack;current=next;CurrentTrack=id;fadeAge=0;
        }
        float Gain(string id)
        {
            float gain=PanicMusicMixPolicyV59.Music(id,world.FpsStressIntensityV414,world.ReducedFpsMotionV54,Volume,PanicAudioSettingsV56.EffectiveMusicGain);
            if(world.PresentationSpeechActive)gain*=.82f;
            gain*=Mathf.Lerp(1f,.65f,world.AudioShelterAmount);
            if(world.Model.Phase==PanicPhase.GameOver)gain*=.75f;
            return gain;
        }
        void Update()
        {
            if(!world)world=FindFirstObjectByType<PanicHomeWorld>();
            if(!world||world.Model==null)return;
            bool stop=!world.isActiveAndEnabled||world.InputLocked||world.Model.IsPaused||world.Model.WaveLedgerOpenV56||world.Model.KeyboardHelpOpenV56||world.NotebookBlocksInput||
                !Enabled||Volume<=0f||PanicAudioSettingsV56.EffectiveMusicGain<=0f||(!Application.isFocused&&!Application.runInBackground)||
                (FpsReady&&world.ExperienceFeedbackBlockedV414);
            if(stop!=paused)
            {
                paused=stop;
                if(stop){a.Pause();b.Pause();a.volume=b.volume=0;}
                else {a.UnPause();b.UnPause();resumeAge=0f;}
            }
            if(paused)return;
            string id=PanicMusicMixPolicyV59.Track(world.Model.Phase,FpsReady);
            if(id!=CurrentTrack)Transition(id);
            else if(current.clip&&fadeAge>=PanicMusicMixPolicyV59.CrossfadeSeconds&&(!current.isPlaying||current.time>=current.clip.length-PanicMusicMixPolicyV59.CrossfadeSeconds))Transition(id);
            float dt=Mathf.Clamp(Time.unscaledDeltaTime,0f,.1f);
            fadeAge=Mathf.Min(PanicMusicMixPolicyV59.CrossfadeSeconds,fadeAge+dt);resumeAge=Mathf.Min(PanicMusicMixPolicyV59.CrossfadeSeconds,resumeAge+dt);
            float mix=Mathf.SmoothStep(0,1,fadeAge/PanicMusicMixPolicyV59.CrossfadeSeconds),resume=Mathf.SmoothStep(0,1,resumeAge/PanicMusicMixPolicyV59.CrossfadeSeconds);
            current.volume=Gain(CurrentTrack)*mix*resume;
            if(outgoing){outgoing.volume=Gain(outgoingTrack)*(1f-mix)*resume;if(fadeAge>=PanicMusicMixPolicyV59.CrossfadeSeconds){outgoing.Stop();outgoing=null;outgoingTrack=null;}}
        }
        public void StopFpsForOwnerV59(PanicHomeWorld owner)
        {
            if(world!=owner)return;
            // Phase exit belongs to this owner's normal two-second crossfade.
            // A modal while FPS owns the stage, or owner disable, remains an immediate stop.
            if(owner&&owner.isActiveAndEnabled&&!owner.FpsTensionOwnsMusicV55)return;
            if(outgoingTrack=="fps_tension"&&outgoing){outgoing.Stop();outgoing.volume=0;outgoing=null;outgoingTrack=null;}
            if(CurrentTrack=="fps_tension"&&current){current.Stop();current.volume=0;CurrentTrack=null;}
        }
        void OnDisable() { if(a){a.Stop();a.volume=0;}if(b){b.Stop();b.volume=0;}CurrentTrack=null;outgoing=null;outgoingTrack=null; }
        // Resources.Load clips belong to Unity's asset cache; never Destroy them here.
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
