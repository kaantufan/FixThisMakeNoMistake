using System;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Read-only authored audio policy. No camera, input, timing, RNG or preferences mutation.</summary>
    public static class PanicMusicMixPolicyV59
    {
        public const float CrossfadeSeconds = 2f;
        public const float DefaultVolume = .10f;
        public const float MaximumMusicGain = .24f;
        static float Unit(float value) { return float.IsNaN(value)||float.IsInfinity(value)?0f:Mathf.Clamp01(value); }
        public static string Track(PanicPhase phase,bool fpsReady)
        { return fpsReady&&phase==PanicPhase.Preparing ? "fps_tension" : phase==PanicPhase.Disaster ? "disaster" : "preparation"; }
        public static float Music(string id,float urgency,bool reduced,float volume,float effectiveGain)
        {
            float u=Unit(urgency);
            float authored=id=="fps_tension"?(reduced?Mathf.Lerp(.12f,.16f,u):Mathf.Lerp(.165f,.23f,u)):
                id=="disaster"?.075f:.095f;
            float scale=Unit(volume)/DefaultVolume;
            return Mathf.Min(MaximumMusicGain,authored*scale)*Unit(effectiveGain);
        }
        public static float Heart(float urgency,bool reduced,float effectiveGain)
        { return (reduced?Mathf.Lerp(.14f,.25f,Unit(urgency)):Mathf.Lerp(.24f,.40f,Unit(urgency)))*Unit(effectiveGain); }
        public static float Breath(float urgency,bool reduced,float effectiveGain)
        { return (reduced?Mathf.Lerp(.085f,.16f,Unit(urgency)):Mathf.Lerp(.14f,.27f,Unit(urgency)))*Unit(effectiveGain); }

        // Root calls from its Editor/headless receipt. No AudioSource or GameObject is created or played.
        public static void Run(Action<string,bool,string> check)
        {
            RunPolicy(check);
            CheckClip("Audio/Music/preparation",20f,180f,false,check);
            CheckClip("Audio/Music/fps_tension",20f,180f,false,check);
            CheckClip("Audio/Music/disaster",20f,180f,false,check);
            CheckClip("Audio/Foley/heartbeat",.5f,12f,true,check);
            CheckClip("Audio/V59/panting",3f,12f,true,check);
        }
        public static void RunPolicy(Action<string,bool,string> check)
        {
            if(check==null)throw new ArgumentNullException("check");
            check("mix.route.normal",Track(PanicPhase.Preparing,false)=="preparation","Existing licensed restrained preparation music.");
            check("mix.route.fps",Track(PanicPhase.Preparing,true)=="fps_tension","Actual imported ElevenLabs FPS music; no procedural score.");
            check("mix.route.disaster",Track(PanicPhase.Disaster,false)=="disaster","Existing disaster score stays quieter than the hazard effects.");
            check("mix.fade.two-seconds",CrossfadeSeconds==2f,"One owner with at most two sources during authored crossfade.");
            bool bounded=true,clearer=true,reduced=true,quiet=true;
            for(int i=0;i<=100;i++)
            {
                float u=i/100f;float normal=Music("preparation",u,false,DefaultVolume,1f),fps=Music("fps_tension",u,false,DefaultVolume,1f);
                bounded&=normal>=0&&fps<=MaximumMusicGain&&Heart(u,false,1)<=.40f&&Breath(u,false,1)<=.27f;
                clearer&=fps>normal&&Heart(u,false,1)>=.24f&&Breath(u,false,1)>=.14f;
                reduced&=Music("fps_tension",u,true,DefaultVolume,1)<fps&&Heart(u,true,1)<Heart(u,false,1)&&Breath(u,true,1)<Breath(u,false,1);
                quiet&=Music("fps_tension",u,false,DefaultVolume,0)==0&&Heart(u,false,0)==0&&Breath(u,false,0)==0;
            }
            check("mix.bounded",bounded,"101 urgency samples; authored ceilings music.24, heart.40, panting.27 before player bus gain.");
            check("mix.fps-distinct",clearer,"Imported FPS bed and body feedback have explicitly stronger source gains; perceived intensity requires listening.");
            check("mix.reduced",reduced,"Reduced movement option also softens music and body sound intensity without camera effects.");
            check("mix.mute",quiet,"Zero master/music/SFX effective gain remains exactly silent.");
            check("mix.scale-once",Mathf.Abs(Music("fps_tension",1,false,DefaultVolume,.25f)-Music("fps_tension",1,false,DefaultVolume,1f)*.25f)<.000001f,"Effective preferences multiply once.");
            check("mix.route.stale-fps",Track(PanicPhase.GameOver,true)=="preparation","A stale FPS flag cannot keep the tense track after the run ends.");
        }
        static void CheckClip(string path,float min,float max,bool mono,Action<string,bool,string> check)
        {
            var clip=Resources.Load<AudioClip>(path);
            bool valid=clip&&clip.length>=min&&clip.length<=max&&clip.frequency>=22050&&clip.channels>=1&&clip.channels<=(mono?1:2)&&clip.loadState!=AudioDataLoadState.Failed;
            check("import."+path,valid,clip?"Imported length="+clip.length+", channels="+clip.channels+", rate="+clip.frequency:"Missing real imported asset; no fabricated substitute.");
            if(!valid)return;
            if(clip.loadType==AudioClipLoadType.Streaming)
            {
                check("stream."+path,clip.samples>0&&clip.channels>0,"Actual retained Streaming import. Unity GetData is intentionally not used; offline source codec PCM is separately hashed/measured, playback/listening NOT_RUN.");
                return;
            }
            if(clip.loadState==AudioDataLoadState.Unloaded)clip.LoadAudioData();
            var data=new float[clip.samples*clip.channels];
            if(!clip.GetData(data,0)){check("pcm."+path,false,"Unity decoded PCM inaccessible.");return;}
            double energy=0;float peak=0;
            foreach(float x in data){energy+=(double)x*x;peak=Mathf.Max(peak,Mathf.Abs(x));}
            double rms=Math.Sqrt(energy/Math.Max(1,data.Length));
            check("pcm."+path,rms>.000001&&peak>.0001&&peak<.99f,"Actual Unity PCM peak="+peak+", RMS="+rms+"; no playback/listening/render claim.");
        }
    }
}
