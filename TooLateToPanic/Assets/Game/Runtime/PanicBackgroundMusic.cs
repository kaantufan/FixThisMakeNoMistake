using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Nonverbal score. Never changes gameplay clocks, input or save state.</summary>
    public sealed class PanicBackgroundMusic : MonoBehaviour
    {
        public static bool Enabled { get; set; } = true;
        public static float Volume { get; set; } = .075f;
        public static PanicBackgroundMusic Instance { get; private set; }
        public string CurrentTrack { get; private set; }
        public bool IsAudible { get { return !paused && Enabled && (a && a.isPlaying || b && b.isPlaying); } }
        public static readonly string[] TrackIds = { "preparation", "disaster", "fps_tension" };
        readonly Dictionary<string,AudioClip> clips = new Dictionary<string,AudioClip>();
        AudioSource a,b,current,outgoing;
        PanicHomeWorld world;
        bool paused;
        float fadeAge=1.2f;
        const float FadeSeconds=1.2f;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if(FindFirstObjectByType<PanicBackgroundMusic>())return;
            new GameObject("Background music").AddComponent<PanicBackgroundMusic>();
        }
        void Awake()
        {
            Instance=this;a=Source();b=Source();current=a;
        }
        AudioSource Source()
        {
            var s=gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=false;s.spatialBlend=0;s.priority=180;s.volume=0;s.dopplerLevel=0;return s;
        }
        public static AudioClip LoadTrack(string id) { return Resources.Load<AudioClip>("Audio/Music/"+id); }
        string TrackForPhase()
        {
            if(world.BagFirstPersonActive && world.BagFinalStageActive)return "fps_tension";
            if(world.Model.Phase==PanicPhase.Disaster)return "disaster";
            switch(world.Model.Phase)
            {
                case PanicPhase.Aftermath:case PanicPhase.Inspection:case PanicPhase.Success:case PanicPhase.GameOver:return "preparation";
                default:return "preparation";
            }
        }
        void Transition(string id)
        {
            if(!clips.TryGetValue(id,out var clip)) { clip=LoadTrack(id);clips[id]=clip; }
            if(!clip)return;
            // A second source overlaps phase changes and the last 1.2 s of a loop.
            var next=current==a?b:a;next.Stop();next.clip=clip;next.time=0;next.volume=0;next.Play();
            outgoing=current;current=next;CurrentTrack=id;fadeAge=0;
        }
        void Update()
        {
            if(!world)world=FindFirstObjectByType<PanicHomeWorld>();
            if(!world||world.Model==null)return;
            if(world.FpsTensionOwnsMusicV55)
            {
                a.Stop(); b.Stop(); a.volume=b.volume=0;
                CurrentTrack=null; outgoing=null; paused=false;
                return;
            }
            bool stop=world.InputLocked||world.Model.IsPaused||world.NotebookBlocksInput||!Enabled||(!Application.isFocused&&!Application.runInBackground);
            if(stop!=paused)
            {
                paused=stop;
                if(stop){a.Pause();b.Pause();}else{a.UnPause();b.UnPause();}
            }
            if(paused)return;
            string id=TrackForPhase();
            if(id!=CurrentTrack)Transition(id);
            else if(current.clip&&fadeAge>=FadeSeconds&&(!current.isPlaying||current.time>=current.clip.length-FadeSeconds))Transition(id);
            fadeAge=Mathf.Min(FadeSeconds,fadeAge+Time.unscaledDeltaTime);
            float mix=Mathf.SmoothStep(0,1,fadeAge/FadeSeconds);
            float gain=Mathf.Clamp01(Volume)*(world.PresentationSpeechActive?.55f:1f);
            if(CurrentTrack=="fps_tension")gain*=Mathf.Lerp(.85f,1.15f,world.FpsStressIntensityV414);
            gain*=Mathf.Lerp(1,.35f,world.AudioShelterAmount);
            if(world.Model.Phase==PanicPhase.Disaster)gain*=.65f;
            if(world.Model.Phase==PanicPhase.GameOver)gain*=.75f;
            current.volume=gain*mix;
            if(outgoing) { outgoing.volume=gain*(1-mix);if(fadeAge>=FadeSeconds){outgoing.Stop();outgoing=null;} }
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
