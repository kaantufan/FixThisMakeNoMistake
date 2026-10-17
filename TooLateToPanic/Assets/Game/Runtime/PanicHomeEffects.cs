using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicHomeEffects : MonoBehaviour
    {
        public PanicHomeWorld World;
        AudioSource oneShot, ambience, roomTone;
        GameObject weather;
        float age;
        float disasterAmbienceVolume = .32f;
        bool warned;
        AudioSource[] voices;
        AudioSource outdoors, hearth, rotor;
        int nextVoice;
        bool suspended;
        readonly System.Collections.Generic.Dictionary<string,float> last = new System.Collections.Generic.Dictionary<string,float>();
        public int PlayedCount { get; private set; }
        public string LastPlayed { get; private set; }
        public Vector3 DisasterVisualImpulseV414 { get { return PanicDisasterArt.SampleVisualImpulse(weather); } }
        void Awake()
        {
            oneShot = gameObject.AddComponent<AudioSource>(); oneShot.volume = .65f;
            ambience = gameObject.AddComponent<AudioSource>(); ambience.loop = true; SetDisasterAmbienceVolume(.32f);
            roomTone = gameObject.AddComponent<AudioSource>(); roomTone.loop = true; roomTone.volume = 0;
            voices = new AudioSource[8];
            for(int i=0;i<voices.Length;i++) { voices[i]=gameObject.AddComponent<AudioSource>(); voices[i].playOnAwake=false; }
            outdoors=MakeLoop(); hearth=MakeLoop(); rotor=MakeLoop();
        }
        AudioSource MakeLoop() { var s=gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=true;s.volume=0;return s; }
        public void SetSuspended(bool value)
        {
            if(suspended==value)return;suspended=value;
            foreach(var s in GetComponents<AudioSource>()) { if(value)s.Pause();else s.UnPause(); }
        }
        void Loop(AudioSource source,string id,float volume)
        {
            if(volume<=0) { source.Stop();return; }
            if(!source.clip)source.clip=PanicAudioBank.Get(id);
            source.volume=volume;
            if(source.clip&&!source.isPlaying&&!suspended)source.Play();
        }
        public void SetEnvironment(bool active,bool inside,bool fire,bool helicopter)
        {
            Loop(roomTone,"roomtone",active&&inside?.035f:0);
            Loop(outdoors,"garden",active&&!inside?.07f:0);
            Loop(hearth,"fireplace",active&&fire?.09f:0);
            Loop(rotor,"rotor",active&&helicopter?.20f:0);
        }
        public void Play(string name, float volume = .5f)
        {
            if(suspended && name!="ui" && name!="reject")return;
            string id=name=="step"?"step_wood":name=="select"||name=="click"?"ui":name=="bunker"?"door_metal":name=="build"?"wood_build":name=="buy"?"pickup_cloth":name=="anchor_release"?"door_toolbox":name;
            if(last.TryGetValue(id,out float time)&&Time.unscaledTime-time<.065f)return;
            var clip=PanicAudioBank.Get(id);
            if(!clip&&id=="objective_tick")clip=Resources.Load<AudioClip>("Audio/Jury/objective_tick");
            if(!clip) { Debug.LogWarning("AUDIO_MISSING "+id);return; }
            last[id]=Time.unscaledTime;
            var source=voices[nextVoice++%voices.Length];source.Stop();source.clip=clip;source.pitch=id.StartsWith("step_")?Random.Range(.94f,1.06f):1;
            source.volume=Mathf.Clamp01(volume)*.7f;source.Play();PlayedCount++;LastPlayed=id;
        }
        public void OnPhase(PanicPhase phase)
        {
            age = 0; warned = false;
            if (phase == PanicPhase.Disaster)
            {
                if (weather) Destroy(weather); weather = PanicDisasterArt.Create(World.Model.Disaster);
                string id = HazardSoundId(World.Model.Disaster);
                ambience.clip = PanicAudioBank.Get(id); SetDisasterAmbienceVolume(.32f); if (ambience.clip) ambience.Play();
                Play("warning",.35f);
            }
            else if (phase == PanicPhase.Aftermath) SetDisasterAmbienceVolume(.14f);
            else if (phase == PanicPhase.Success || phase == PanicPhase.Inspection || phase == PanicPhase.GameOver)
            {
                SetDisasterAmbienceVolume(.07f);
                // The damage tour already owns settled ash/smoke. Stop active fire
                // and its multi-floor scorch mesh before the house returns to cutaway.
                if (phase == PanicPhase.Inspection && World.Model.Disaster == DisasterType.Fire)
                    StopJuryHazard();
                if (phase == PanicPhase.Success) Play("success", .55f);
                if (phase == PanicPhase.GameOver) Play("fail", .6f);
            }
            else { ambience.Stop(); if (weather) Destroy(weather); weather = null; }
        }
        public void StartCinematicWeather()
        {
            if(!World)return;
            if(!weather)weather=PanicDisasterArt.Create(World.Model.Disaster);
            age=0;string id=HazardSoundId(World.Model.Disaster);
            ambience.clip=PanicAudioBank.Get(id);
            SetDisasterAmbienceVolume(.3f);if(ambience.clip)ambience.Play();
        }
        void SetDisasterAmbienceVolume(float volume)
        {
            disasterAmbienceVolume = volume;
            if (ambience) ambience.volume = volume * (World ? World.PresentationEffectsDuckFactor : 1f);
        }
        static string HazardSoundId(DisasterType type)
        {
            switch(type){case DisasterType.Earthquake:return "earthquake";case DisasterType.Flood:return "flood";case DisasterType.Storm:return "storm";default:return "fire";}
        }
        public void StopJuryHazard()
        {
            if (ambience) ambience.Stop();
            if (weather) Destroy(weather);
            weather = null;
        }
        void Update()
        {
            if (!World || World.Model == null) return;
            if (suspended || World.Model.IsPaused || World.NotebookBlocksInput) return;
            age += Time.deltaTime;
            if (World.Model.Phase == PanicPhase.Preparing && World.Model.TimeRemaining <= 60 && !warned) { warned = true; Play("warning", .6f); }
            if (weather) PanicDisasterArt.Animate(weather, age, World.Model.Phase == PanicPhase.Disaster || World.CinematicActive&&World.Model.Phase==PanicPhase.GameOver ? Mathf.Clamp01(age * .4f) : .35f);
            if (ambience) ambience.volume = disasterAmbienceVolume * World.PresentationEffectsDuckFactor;
        }
    }
}
