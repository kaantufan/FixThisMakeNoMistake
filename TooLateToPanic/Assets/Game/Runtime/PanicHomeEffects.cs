using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicHomeEffects : MonoBehaviour
    {
        public PanicHomeWorld World;
        AudioSource oneShot, ambience, roomTone;
        GameObject weather;
        readonly AudioSource[] hazardAmbiencesV54 = new AudioSource[4];
        readonly AudioLowPassFilter[] hazardFiltersV54 = new AudioLowPassFilter[4];
        readonly bool[] hazardAudioActiveV54 = new bool[4];
        public int ActiveHazardVisualCountV54 { get { return PanicDisasterArt.ActiveHazardCountV54(weather); } }
        public int ActiveHazardAudioCountV54
        {
            get { int count=0;for(int i=0;i<4;i++)if(hazardAudioActiveV54[i])count++;return count; }
        }
        public bool HasHazardVisualV54(DisasterType type) { return PanicDisasterArt.HazardRootV54(weather,type); }
        public float HazardEmissionRateV54(DisasterType type)
        { return PanicDisasterArt.SampleEmissionRateV52(PanicDisasterArt.HazardRootV54(weather,type)); }
        public float HazardPresentationScaleV54(DisasterType type)
        { return PanicDisasterArt.SamplePresentationScaleV52(PanicDisasterArt.HazardRootV54(weather,type)); }
        public float HazardAmbienceVolumeV54(DisasterType type)
        { int i=(int)type;return i>=0&&i<4&&hazardAmbiencesV54[i]?hazardAmbiencesV54[i].volume:0f; }
        float age;
        float disasterAmbienceVolume = .32f;
        bool warned;
        AudioSource[] voices;
        AudioLowPassFilter[] voiceFilters;
        AudioSource outdoors, hearth, rotor;
        AudioLowPassFilter hazardFilter,rotorFilter;
        float shelterMix,rotorGain;
        public float ShelterMix { get { return shelterMix; } }
        public float HazardCutoff { get { return hazardFilter ? hazardFilter.cutoffFrequency : 22000; } }
        int nextVoice;
        bool suspended;
        readonly System.Collections.Generic.Dictionary<string,float> last = new System.Collections.Generic.Dictionary<string,float>();
        public int PlayedCount { get; private set; }
        public string LastPlayed { get; private set; }
        public Vector3 DisasterVisualImpulseV414 { get { return PanicDisasterArt.SampleVisualImpulse(weather); } }
        // Actual rendered/audio values for the native V5.2 receipt; no gameplay mutation.
        public float DisasterPresentationScaleV52 { get { return PanicDisasterArt.SamplePresentationScaleV52(weather); } }
        public float DisasterEmissionRateV52 { get { return PanicDisasterArt.SampleEmissionRateV52(weather); } }
        public float DisasterWaterLevelV52 { get { return PanicDisasterArt.SampleWaterLevelV52(weather); } }
        public float DisasterWaterPeakHeightV52 { get { return PanicDisasterArt.SampleWaterPeakHeightV52(weather); } }
        public float DisasterFlamePeakHeightV52 { get { return PanicDisasterArt.SampleFlamePeakHeightV52(weather); } }
        public float DisasterAmbienceGainV52 { get { return 1f + .45f * (PresentationScaleV52 - 1f); } }
        public float DisasterAmbienceVolumeV52
        { get { float sum=0;for(int i=0;i<4;i++)if(hazardAmbiencesV54[i])sum+=hazardAmbiencesV54[i].volume;return sum; } }
        float PresentationScaleV52 { get { return World && World.Model != null ? PanicDisasterArt.ClampPresentationScaleV52(World.Model.DisasterPresentationScaleV52) : 1f; } }
        void Awake()
        {
            oneShot = gameObject.AddComponent<AudioSource>(); oneShot.volume = .65f;
            // Allocated once, at most four audio loops; no AddComponent/clip lookup in Update.
            ambience = hazardAmbiencesV54[0] = MakeLoop();
            for(int i=1;i<4;i++)hazardAmbiencesV54[i]=MakeLoop();
            SetDisasterAmbienceVolume(.22f);
            roomTone = gameObject.AddComponent<AudioSource>(); roomTone.loop = true; roomTone.volume = 0;
            voices = new AudioSource[8];voiceFilters=new AudioLowPassFilter[8];
            for(int i=0;i<voices.Length;i++) { var go=new GameObject("Foley voice "+i);go.transform.SetParent(transform,false);voices[i]=go.AddComponent<AudioSource>(); voices[i].playOnAwake=false;voiceFilters[i]=go.AddComponent<AudioLowPassFilter>();voiceFilters[i].cutoffFrequency=22000; }
            outdoors=MakeLoop(); hearth=MakeLoop(); rotor=MakeLoop();
            hazardFilter=hazardFiltersV54[0]=ambience.gameObject.AddComponent<AudioLowPassFilter>();
            for(int i=1;i<4;i++)hazardFiltersV54[i]=hazardAmbiencesV54[i].gameObject.AddComponent<AudioLowPassFilter>();
            for(int i=0;i<4;i++)hazardFiltersV54[i].cutoffFrequency=22000;
            // Filter on the rotor child only, so footsteps/UI retain clarity.
            rotorFilter=rotor.gameObject.AddComponent<AudioLowPassFilter>();rotorFilter.cutoffFrequency=22000;
        }
        AudioSource MakeLoop() { var go=new GameObject("Environment loop");go.transform.SetParent(transform,false);var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=true;s.volume=0;s.dopplerLevel=0;return s; }
        public void SetSuspended(bool value)
        {
            if(suspended==value)return;suspended=value;
            foreach(var s in GetComponentsInChildren<AudioSource>(true)) { if(value)s.Pause();else s.UnPause(); }
        }
        void Loop(AudioSource source,string id,float volume)
        {
            if(volume<=0) { source.volume=0;source.Stop();return; }
            if(!source.clip)source.clip=PanicAudioBank.Get(id);
            source.volume=volume;
            if(source.clip&&!source.isPlaying&&!suspended)source.Play();
        }
        public void SetEnvironment(bool active,bool inside,bool fire,bool helicopter)
        {
            Loop(roomTone,"roomtone",active&&inside?.025f*(1-shelterMix):0);
            Loop(outdoors,"garden",active&&!inside?.045f*(1-shelterMix):0);
            Loop(hearth,"fireplace",active&&fire?.09f:0);
            float distance=World?World.AudioHelicopterDistance:1000;
            float target=active&&helicopter?.12f*DistanceGain(distance,12,75)*Mathf.Lerp(1,.08f,shelterMix):0;
            rotorGain=Mathf.MoveTowards(rotorGain,target,Time.unscaledDeltaTime*.20f);
            Loop(rotor,"rotor",rotorGain);
            if(rotorFilter)rotorFilter.cutoffFrequency=Mathf.Lerp(12000,550,shelterMix);
        }
        public static float DistanceGain(float distance,float near,float far)
        { float t=Mathf.InverseLerp(near,far,distance);return (1-t)*(1-t); }
        public void Play(string name, float volume = .5f)
        {
            if(suspended && name!="ui" && name!="reject")return;
            string id=name=="step"?"step_wood":name=="select"||name=="click"?"ui":name=="bunker"?"bunker_hatch":name=="build"?"wood_build":name=="buy"?"pickup_cloth":name=="anchor_release"?"door_toolbox":name;
            if(last.TryGetValue(id,out float time)&&Time.unscaledTime-time<.065f)return;
            var clip=PanicAudioBank.Get(id);
            if(!clip&&id=="objective_tick")clip=Resources.Load<AudioClip>("Audio/Jury/objective_tick");
            if(!clip) { Debug.LogWarning("AUDIO_MISSING "+id);return; }
            last[id]=Time.unscaledTime;
            int channel=nextVoice++%voices.Length;var source=voices[channel];bool exteriorImpact=id.StartsWith("impact_")||id=="warning";
            voiceFilters[channel].cutoffFrequency=exteriorImpact?Mathf.Lerp(16000,800,shelterMix):22000;
            if(exteriorImpact)volume*=Mathf.Lerp(1,.18f,shelterMix);
            source.Stop();source.clip=clip;source.pitch=id.StartsWith("step_")?Random.Range(.94f,1.06f):1;
            source.volume=Mathf.Clamp01(volume)*.7f;source.Play();PlayedCount++;LastPlayed=id;
        }
        public void OnPhase(PanicPhase phase)
        {
            age = 0; warned = false;
            if (phase == PanicPhase.Disaster)
            {
                if (weather) { weather.SetActive(false); Destroy(weather); }
                weather = PanicDisasterArt.CreateActiveV54(World.Model);
                StartHazardAmbienceV54(); SetDisasterAmbienceVolume(.22f);
                Play("warning",.35f);
            }
            else if (phase == PanicPhase.Aftermath) SetDisasterAmbienceVolume(.14f);
            else if (phase == PanicPhase.Success || phase == PanicPhase.Inspection || phase == PanicPhase.GameOver)
            {
                SetDisasterAmbienceVolume(.07f);
                // The damage tour already owns settled ash/smoke. Stop active fire
                // and its multi-floor scorch mesh before the house returns to cutaway.
                if (phase == PanicPhase.Inspection && World.Model.HasActiveDisasterV54(DisasterType.Fire))
                {
                    PanicDisasterArt.StopHazardV54(weather, DisasterType.Fire);
                    int fire=(int)DisasterType.Fire;hazardAudioActiveV54[fire]=false;
                    if(hazardAmbiencesV54[fire]){hazardAmbiencesV54[fire].Stop();hazardAmbiencesV54[fire].volume=0;}
                    ApplyHazardAmbienceMixV54();
                }
                if (phase == PanicPhase.Success) Play("success", .55f);
                if (phase == PanicPhase.GameOver) Play("fail", .6f);
            }
            else StopJuryHazard();
        }
        public void StartCinematicWeather()
        {
            if(!World)return;
            if(!weather)weather=PanicDisasterArt.CreateActiveV54(World.Model);
            age=0;StartHazardAmbienceV54();SetDisasterAmbienceVolume(.22f);
        }
        void SetDisasterAmbienceVolume(float volume)
        {
            disasterAmbienceVolume = volume;
            ApplyHazardAmbienceMixV54();
        }
        void StartHazardAmbienceV54()
        {
            for(int i=0;i<4;i++)
            {
                var source=hazardAmbiencesV54[i];source.Stop();source.volume=0;
                bool active=World&&World.Model!=null&&World.Model.HasActiveDisasterV54((DisasterType)i);
                hazardAudioActiveV54[i]=active;
                source.clip=active?PanicAudioBank.Get(HazardSoundId((DisasterType)i)):null;
                if(active&&source.clip&&!suspended)source.Play();
            }
        }
        void ApplyHazardAmbienceMixV54()
        {
            int count=ActiveHazardAudioCountV54;
            // Preserve single-hazard loudness. Up to four simultaneous loops share bounded
            // equal-power gain, rather than each playing at full gain on top of the others.
            float gain=count>0?1f/Mathf.Sqrt(count):0f;
            float volume=disasterAmbienceVolume*DisasterAmbienceGainV52*(World?World.PresentationEffectsDuckFactor:1f)*Mathf.Lerp(1,.18f,shelterMix)*gain;
            float cutoff=Mathf.Lerp(16000,World&&World.AudioHatchOpen?1600:650,shelterMix);
            for(int i=0;i<4;i++)
            {
                if(hazardAmbiencesV54[i])hazardAmbiencesV54[i].volume=hazardAudioActiveV54[i]?volume:0f;
                if(hazardFiltersV54[i])hazardFiltersV54[i].cutoffFrequency=cutoff;
            }
        }
        static string HazardSoundId(DisasterType type)
        {
            switch(type){case DisasterType.Earthquake:return "earthquake";case DisasterType.Flood:return "flood";case DisasterType.Storm:return "storm";default:return "fire";}
        }
        public void StopJuryHazard()
        {
            for(int i=0;i<4;i++)
            {
                hazardAudioActiveV54[i]=false;
                if(hazardAmbiencesV54[i]){hazardAmbiencesV54[i].Stop();hazardAmbiencesV54[i].volume=0;}
            }
            if (weather) { weather.SetActive(false); Destroy(weather); }
            weather = null;
        }
        void OnDestroy() { StopJuryHazard(); }
        void Update()
        {
            if (!World || World.Model == null) return;
            if (suspended || World.Model.IsPaused || World.NotebookBlocksInput) return;
            age += Time.deltaTime;
            if (World.Model.Phase == PanicPhase.Preparing && World.Model.TimeRemaining <= 60 && !warned) { warned = true; Play("warning", .6f); }
            if (weather) PanicDisasterArt.Animate(weather, age, World.Model.Phase == PanicPhase.Disaster || World.CinematicActive&&World.Model.Phase==PanicPhase.GameOver ? Mathf.Clamp01(age * .4f) : .35f, PresentationScaleV52);
            shelterMix=Mathf.MoveTowards(shelterMix,World.AudioShelterAmount,Time.unscaledDeltaTime/1.2f);
            ApplyHazardAmbienceMixV54();
        }
    }
}
