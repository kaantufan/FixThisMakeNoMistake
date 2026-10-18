using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicHomeEffects : MonoBehaviour
    {
        public PanicHomeWorld World;
        AudioSource oneShot, ambience, roomTone;
        GameObject weather, floodRunoffV58;
        float floodRunoffAgeV58, floodRunoffScaleV58 = 1f;
        int weatherWaveV58;
        public bool FloodRunoffActiveV58 { get { return floodRunoffV58 || PanicDisasterArt.IsFloodRecedingV58(weather); } }
        public float FloodRunoffElapsedV58 { get { return PanicDisasterArt.SampleFloodRunoffElapsedV58(floodRunoffV58 ? floodRunoffV58 : weather); } }
        public float FloodWetnessAtV58(float x, float z) { return PanicDisasterArt.SampleFloodWetnessV58(floodRunoffV58 ? floodRunoffV58 : weather, x, z); }
        readonly AudioSource[] hazardAmbiencesV54 = new AudioSource[4];
        readonly AudioLowPassFilter[] hazardFiltersV54 = new AudioLowPassFilter[4];
        readonly bool[] hazardAudioActiveV54 = new bool[4];
        AudioSource quakeStructureV55;
        AudioClip quakeWoodV55, quakeMetalV55, measuredQuakeClipV55;
        float quakeClipRmsV55;
        int nextQuakeRattleV55;
        static readonly float[] QuakeRattleTimesV55 = { .4f, 1.5f, 2.65f, 3.75f, 4.9f, 6.05f, 7.25f };
        bool QuakeCinematicWindowV55
        { get { return World && World.Model != null && World.CinematicActive && World.Model.Phase == PanicPhase.Disaster && World.Model.HasActiveDisasterV54(DisasterType.Earthquake); } }
        public bool QuakeCinematicAudioPlayingV55
        { get { var source=hazardAmbiencesV54[(int)DisasterType.Earthquake];return QuakeCinematicWindowV55&&!suspended&&source&&source.enabled&&source.isPlaying&&!source.mute&&source.volume>0; } }
        public float QuakeCinematicAudioGainV55
        { get { var source=hazardAmbiencesV54[(int)DisasterType.Earthquake];return QuakeCinematicWindowV55&&source?source.volume:0; } }
        public float QuakeCinematicClipRmsV55 { get { return quakeClipRmsV55; } }
        public AudioSource QuakeCinematicSourceV55 { get { return hazardAmbiencesV54[(int)DisasterType.Earthquake]; } }
        public AudioSource QuakeStructureSourceV55 { get { return quakeStructureV55; } }
        public int ActiveHazardVisualCountV54 { get { return PanicDisasterArt.ActiveHazardCountV54(weather) + (floodRunoffV58 ? 1 : 0); } }
        public int ActiveHazardAudioCountV54
        {
            get { int count=0;for(int i=0;i<4;i++)if(hazardAudioActiveV54[i])count++;return count; }
        }
        GameObject HazardVisualRootV58(DisasterType type)
        { return type == DisasterType.Flood && floodRunoffV58 ? floodRunoffV58 : PanicDisasterArt.HazardRootV54(weather, type); }
        public bool HasHazardVisualV54(DisasterType type) { return HazardVisualRootV58(type); }
        public float HazardEmissionRateV54(DisasterType type)
        { return PanicDisasterArt.SampleEmissionRateV52(HazardVisualRootV58(type)); }
        public float HazardPresentationScaleV54(DisasterType type)
        { return PanicDisasterArt.SamplePresentationScaleV52(HazardVisualRootV58(type)); }
        public AudioSource HazardSourceV56(DisasterType type) { int i=(int)type;return i>=0&&i<4?hazardAmbiencesV54[i]:null; }
        public float HazardCutoffV56(DisasterType type) { int i=(int)type;return i>=0&&i<4&&hazardFiltersV54[i]?hazardFiltersV54[i].cutoffFrequency:22000f; }
        public float HazardBaseGainV56(DisasterType type) { var s=HazardSourceV56(type);float gain=PanicAudioSettingsV56.EffectiveSfxGain;return s&&gain>0?s.volume/gain:0; }
        public float HazardAmbienceVolumeV54(DisasterType type)
        { int i=(int)type;return i>=0&&i<4&&hazardAmbiencesV54[i]?hazardAmbiencesV54[i].volume:0f; }
        float age;
        float disasterAmbienceVolume = .32f;
        bool warned;
        AudioSource[] voices;
        AudioLowPassFilter[] voiceFilters;
        readonly float[] voiceBaseGainsV56 = new float[8];
        float quakeStructureBaseGainV56;
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
        public float DisasterWaterLevelV52 { get { return PanicDisasterArt.SampleWaterLevelV52(floodRunoffV58 ? floodRunoffV58 : weather); } }
        public float DisasterWaterPeakHeightV52 { get { return PanicDisasterArt.SampleWaterPeakHeightV52(floodRunoffV58 ? floodRunoffV58 : weather); } }
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
            quakeStructureV55=MakeLoop();quakeStructureV55.gameObject.name="Earthquake structural rattle V5.5";
            quakeStructureV55.loop=false;quakeStructureV55.spatialBlend=0f;quakeStructureV55.priority=72;
            hazardFilter=hazardFiltersV54[0]=ambience.gameObject.AddComponent<AudioLowPassFilter>();
            for(int i=1;i<4;i++)hazardFiltersV54[i]=hazardAmbiencesV54[i].gameObject.AddComponent<AudioLowPassFilter>();
            for(int i=0;i<4;i++)hazardFiltersV54[i].cutoffFrequency=22000;
            // Filter on the rotor child only, so footsteps/UI retain clarity.
            rotorFilter=rotor.gameObject.AddComponent<AudioLowPassFilter>();rotorFilter.cutoffFrequency=22000;
        }
        AudioSource MakeLoop() { var go=new GameObject("Environment loop");go.transform.SetParent(transform,false);var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=true;s.volume=0;s.dopplerLevel=0;s.spatialBlend=0f;return s; }
        public void SetSuspended(bool value)
        {
            if(suspended==value)return;suspended=value;
            foreach(var s in GetComponentsInChildren<AudioSource>(true)) { if(value)s.Pause();else s.UnPause(); }
            // A cinematic may be requested while a modal still owns suspension (F3).
            // UnPause alone cannot start a newly assigned clip that has never played.
            if(!value)for(int i=0;i<4;i++)
            {
                var source=hazardAmbiencesV54[i];
                if(hazardAudioActiveV54[i]&&source&&source.clip&&!source.isPlaying)source.Play();
            }
        }
        void Loop(AudioSource source,string id,float volume)
        {
            if(volume<=0) { source.volume=0;source.Stop();return; }
            if(!source.clip)source.clip=PanicAudioBank.Get(id);
            source.volume=volume*PanicAudioSettingsV56.EffectiveSfxGain;
            if(source.clip&&!source.isPlaying&&!suspended)source.Play();
        }
        public void StopRescueAudioV512()
        { rotorGain=0; if(rotor){rotor.volume=0;rotor.Stop();} }
        void StopRecoveryStormV512()
        {
            // Model resolution is complete. Do not keep a live storm around incoming rescue actors.
            PanicDisasterArt.StopHazardV54(weather,DisasterType.Storm);
            int storm=(int)DisasterType.Storm;hazardAudioActiveV54[storm]=false;
            if(hazardAmbiencesV54[storm]){hazardAmbiencesV54[storm].Stop();hazardAmbiencesV54[storm].volume=0;}
        }
        public void SetEnvironment(bool active,bool inside,bool fire,bool helicopter)
        {
            Loop(roomTone,"roomtone",active&&inside?.025f*(1-shelterMix):0);
            Loop(outdoors,"garden",active&&!inside?.045f*(1-shelterMix):0);
            Loop(hearth,"fireplace",active&&fire?.09f:0);
            float distance=World?World.AudioHelicopterDistance:1000;
            float target=active&&helicopter?.12f*DistanceGain(distance,12,75)*Mathf.Lerp(1,.08f,shelterMix):0;
            // Absent/forbidden actor is immediately silent; only an approaching visible actor fades in.
            if(!active||!helicopter)StopRescueAudioV512();
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
            bool committedEvent=id=="v56_hammer_hit"||id=="v56_gas_flare"||id=="v56_pipe_leak"||id=="v56_electrical_short";
            if(!committedEvent&&last.TryGetValue(id,out float time)&&Time.unscaledTime-time<.065f)return;
            var clip=PanicAudioBank.Get(id);
            if(!clip&&id=="objective_tick")clip=Resources.Load<AudioClip>("Audio/Jury/objective_tick");
            if(!clip) { Debug.LogWarning("AUDIO_MISSING "+id);return; }
            last[id]=Time.unscaledTime;
            int channel=nextVoice++%voices.Length;var source=voices[channel];bool exteriorImpact=id.StartsWith("impact_")||id=="warning"||id=="v56_gas_flare"||id=="v56_pipe_leak"||id=="v56_electrical_short";
            float cueShelter=World&&World.AudioExteriorCameraV56?0:shelterMix;
            voiceFilters[channel].cutoffFrequency=exteriorImpact?Mathf.Lerp(16000,800,cueShelter):22000;
            if(exteriorImpact)volume*=Mathf.Lerp(1,.18f,cueShelter);
            source.Stop();source.clip=clip;source.pitch=id.StartsWith("step_")?Random.Range(.94f,1.06f):1;
            voiceBaseGainsV56[channel]=Mathf.Clamp01(volume)*.7f;
            source.volume=voiceBaseGainsV56[channel]*PanicAudioSettingsV56.EffectiveSfxGain;source.Play();PlayedCount++;LastPlayed=id;
        }
        public void OnPhase(PanicPhase phase)
        {
            age = 0; warned = false;
            if (phase == PanicPhase.Disaster)
            {
                if (weather) { weather.SetActive(false); Destroy(weather); }
                ClearFloodRunoffV58();
                weather = PanicDisasterArt.CreateActiveV54(World.Model);
                weatherWaveV58 = World.Model.Wave;
                StartHazardAmbienceV54(); SetDisasterAmbienceVolume(.22f);
                Play("warning",.35f);
            }
            else if (phase == PanicPhase.Aftermath)
            {
                StopRecoveryStormV512();
                SetDisasterAmbienceVolume(.14f);
            }
            else if (phase == PanicPhase.Success || phase == PanicPhase.Inspection || phase == PanicPhase.GameOver)
            {
                if(World&&World.RecoveryResponseAllowedV512)StopRecoveryStormV512();
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
                if (phase == PanicPhase.Inspection) PanicDisasterArt.BeginFloodRecessionV58(weather);
                if (phase == PanicPhase.Success) Play("success", .55f);
                if (phase == PanicPhase.GameOver) Play("fail", .6f);
            }
            else
            {
                // A real next wave may begin while the visual runoff is finishing. Fresh
                // runs/simulations reset to wave1, Title and other phase resets clear it.
                bool nextWave = phase == PanicPhase.Preparing && World && World.Model != null && World.Model.Wave > weatherWaveV58;
                StopJuryHazard(nextWave);
            }
        }
        public void StartCinematicWeather()
        {
            if(!World)return;
            ClearFloodRunoffV58();
            if(!weather) { weather=PanicDisasterArt.CreateActiveV54(World.Model);weatherWaveV58=World.Model.Wave; }
            age=0;StartHazardAmbienceV54();SetDisasterAmbienceVolume(.22f);
        }
        void SetDisasterAmbienceVolume(float volume)
        {
            disasterAmbienceVolume = volume;
            ApplyHazardAmbienceMixV54();
        }
        void StartHazardAmbienceV54()
        {
            nextQuakeRattleV55=0;
            if(quakeStructureV55)quakeStructureV55.Stop();
            for(int i=0;i<4;i++)
            {
                var source=hazardAmbiencesV54[i];source.Stop();source.volume=0;
                bool active=World&&World.Model!=null&&World.Model.HasActiveDisasterV54((DisasterType)i);
                hazardAudioActiveV54[i]=active;
                source.clip=active?PanicAudioBank.Get(HazardSoundId((DisasterType)i)):null;
                if(active&&i==(int)DisasterType.Earthquake)
                {
                    source.priority=64;source.spatialBlend=0f;
                    if(!source.clip)Debug.LogWarning("AUDIO_MISSING earthquake (cinematic rumble unavailable)");
                    else if(measuredQuakeClipV55!=source.clip)
                    { measuredQuakeClipV55=source.clip;quakeClipRmsV55=ReadClipRmsV55(source.clip); }
                    if(!quakeWoodV55)quakeWoodV55=PanicAudioBank.Get("impact_wood");
                    if(!quakeMetalV55)quakeMetalV55=PanicAudioBank.Get("impact_metal");
                    if(!quakeWoodV55||!quakeMetalV55)Debug.LogWarning("AUDIO_MISSING earthquake structural impact_wood/impact_metal");
                }
                if(active&&source.clip&&!suspended)source.Play();
            }
        }
        void ApplyHazardAmbienceMixV54()
        {
            int count=ActiveHazardAudioCountV54;
            // Preserve single-hazard loudness. Up to four simultaneous loops share bounded
            // equal-power gain, rather than each playing at full gain on top of the others.
            float gain=count>0?1f/Mathf.Sqrt(count):0f;
            bool exterior=World&&World.AudioExteriorCameraV56;
            float cameraShelter=exterior?0:shelterMix;
            float volume=disasterAmbienceVolume*DisasterAmbienceGainV52*(World?World.PresentationEffectsDuckFactor:1f)*Mathf.Lerp(1,.18f,cameraShelter)*gain;
            float cutoff=Mathf.Lerp(16000,World&&World.AudioHatchOpen?1600:650,cameraShelter);
            bool cinematicQuake=QuakeCinematicWindowV55&&exterior;
            for(int i=0;i<4;i++)
            {
                // The cinematic camera leaves the bunker. Keeping the player's shelter mix
                // here hid an already bass-heavy real clip at .22*.18=.0396 gain (-52dB RMS).
                // The quake's bounded screen-space layer follows the actual cinematic, while
                // ordinary shelter/environment mixing and concurrent equal-power gain remain.
                bool quake=cinematicQuake&&i==(int)DisasterType.Earthquake;
                float channelVolume=quake?.55f*DisasterAmbienceGainV52*(World?World.PresentationEffectsDuckFactor:1f)*gain:volume;
                if(hazardAmbiencesV54[i])hazardAmbiencesV54[i].volume=hazardAudioActiveV54[i]?Mathf.Clamp01(channelVolume)*PanicAudioSettingsV56.EffectiveSfxGain:0f;
                if(hazardFiltersV54[i])hazardFiltersV54[i].cutoffFrequency=quake?16000:cutoff;
            }
        }
        static float ReadClipRmsV55(AudioClip clip)
        {
            if(!clip)return 0;
            if(clip.loadState==AudioDataLoadState.Unloaded)clip.LoadAudioData();
            int channels=Mathf.Max(1,clip.channels),frames=Mathf.Max(1,4096/channels),total=clip.samples*channels;
            var data=new float[frames*channels];double sum=0;
            for(int offset=0;offset<clip.samples;offset+=frames)
            {
                if(!clip.GetData(data,offset)){Debug.LogWarning("AUDIO_PCM_UNAVAILABLE earthquake");return 0;}
                int count=Mathf.Min(data.Length,(clip.samples-offset)*channels);
                for(int i=0;i<count;i++)sum+=(double)data[i]*data[i];
            }
            return total>0?(float)System.Math.Sqrt(sum/total):0;
        }
        void TickQuakeRattleV55()
        {
            if(!quakeStructureV55)return;
            if(!QuakeCinematicWindowV55)
            { if(quakeStructureV55.isPlaying)quakeStructureV55.Stop();return; }
            if(nextQuakeRattleV55>=QuakeRattleTimesV55.Length||age<QuakeRattleTimesV55[nextQuakeRattleV55])return;
            int index=nextQuakeRattleV55++;
            var clip=(index%3==1)?quakeMetalV55:quakeWoodV55;
            // A missing rumble remains a visible resource failure; impacts never replace it.
            if(!clip||!hazardAmbiencesV54[(int)DisasterType.Earthquake].clip)return;
            float gain=1f/Mathf.Sqrt(Mathf.Max(1,ActiveHazardAudioCountV54));
            quakeStructureV55.Stop();quakeStructureV55.clip=clip;
            quakeStructureV55.pitch=index%3==1?.88f:.78f;
            float cameraShelter=World&&World.AudioExteriorCameraV56?0:shelterMix;
            quakeStructureBaseGainV56=Mathf.Clamp01(.18f*DisasterAmbienceGainV52*(World?World.PresentationEffectsDuckFactor:1f)*gain)*Mathf.Lerp(1,.18f,cameraShelter);
            quakeStructureV55.volume=quakeStructureBaseGainV56*PanicAudioSettingsV56.EffectiveSfxGain;
            quakeStructureV55.Play();
        }
        static string HazardSoundId(DisasterType type)
        {
            switch(type){case DisasterType.Earthquake:return "earthquake";case DisasterType.Flood:return "flood";case DisasterType.Storm:return "storm";default:return "fire";}
        }
        void ClearFloodRunoffV58()
        {
            if (floodRunoffV58) { floodRunoffV58.SetActive(false); Destroy(floodRunoffV58); }
            floodRunoffV58 = null; floodRunoffAgeV58 = 0;
        }
        void PreserveFloodRunoffV58()
        {
            if (!weather) return;
            var flood = PanicDisasterArt.DetachFloodRunoffV58(weather);
            if (!flood) return;
            ClearFloodRunoffV58();
            floodRunoffV58 = flood; floodRunoffAgeV58 = 0;
            floodRunoffScaleV58 = PanicDisasterArt.SamplePresentationScaleV52(flood);
            if (weather == flood) weather = null;
        }
        public void StopJuryHazard(bool smoothFlood = false)
        {
            if (smoothFlood) PreserveFloodRunoffV58(); else ClearFloodRunoffV58();
            if(quakeStructureV55){quakeStructureV55.Stop();quakeStructureV55.volume=0;}
            nextQuakeRattleV55=0;
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
            float settingsGain=PanicAudioSettingsV56.EffectiveSfxGain;
            if(voices!=null)for(int i=0;i<voices.Length;i++)if(voices[i])voices[i].volume=voiceBaseGainsV56[i]*settingsGain;
            if(quakeStructureV55)quakeStructureV55.volume=quakeStructureBaseGainV56*settingsGain;
            if (!World || World.Model == null) return;
            if (suspended || World.Model.IsPaused || World.NotebookBlocksInput) return;
            age += Time.deltaTime;
            if (World.Model.Phase == PanicPhase.Preparing && World.Model.TimeRemaining <= 60 && !warned) { warned = true; Play("warning", .6f); }
            if (floodRunoffV58)
            {
                floodRunoffAgeV58 += Time.deltaTime;
                PanicDisasterArt.Animate(floodRunoffV58, floodRunoffAgeV58, 0, floodRunoffScaleV58);
                if (PanicDisasterArt.SampleFloodRunoffElapsedV58(floodRunoffV58) >= PanicDisasterArt.FloodRunoffSecondsV58) ClearFloodRunoffV58();
            }
            if (weather) PanicDisasterArt.Animate(weather, age, World.Model.Phase == PanicPhase.Disaster || World.CinematicActive&&World.Model.Phase==PanicPhase.GameOver ? Mathf.Clamp01(age * .4f) : .35f, PresentationScaleV52);
            if (weather && PanicDisasterArt.IsFloodRecedingV58(weather) && PanicDisasterArt.SampleFloodRunoffElapsedV58(weather) >= PanicDisasterArt.FloodRunoffSecondsV58)
            {
                bool single = PanicDisasterArt.HazardRootV54(weather, DisasterType.Flood) == weather;
                PanicDisasterArt.StopHazardV54(weather, DisasterType.Flood);
                if (single) weather = null;
                int flood = (int)DisasterType.Flood; hazardAudioActiveV54[flood] = false;
                if (hazardAmbiencesV54[flood]) { hazardAmbiencesV54[flood].Stop(); hazardAmbiencesV54[flood].volume = 0; }
            }
            shelterMix=Mathf.MoveTowards(shelterMix,World.AudioShelterAmount,Time.unscaledDeltaTime/1.2f);
            ApplyHazardAmbienceMixV54();
            TickQuakeRattleV55();
        }
    }
}
