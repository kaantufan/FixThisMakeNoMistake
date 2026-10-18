using System;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Headless Unity import/source API checks. No scene, player, preferences or gameplay mutations.</summary>
    public static class PanicDialogueSourcePolicyV59
    {
        public static Rect CaptionBounds { get { return new Rect(440f,116f,690f,94f); } }
        public static float FontPixelsAt(int width, int height)
        { return 24f * Mathf.Min(width/1600f,height/900f); }
        public static bool Avoids(Rect reserved) { return !CaptionBounds.Overlaps(reserved); }
        public static void Run(Action<string,bool,string> check)
        {
            if(check == null) throw new ArgumentNullException("check");
            var session = new PanicDialogueSessionV59(); session.BeginScope("source-check");
            check("dialogue.full-line",session.TryStart("intro",PanicKktcStoryV53.ShortFlowPremise(),10)&&session.Text.Length>20,"Complete shared-story caption is available immediately.");
            float life=session.Remaining;
            session.Tick(.1f,true);
            check("dialogue.pause",session.Remaining==life,"Suspended delta cannot age caption.");
            int bits=0;
            for(int i=0;i<14;i++){session.Tick(.1f,false);if(session.TakeBlip(true))bits++;}
            check("dialogue.bit-cap",bits==3&&session.PendingCount==0,"At most three bits; no deferred queue.");
            check("dialogue.dedupe",!session.TryStart("intro","Duplicate",50),"Observed current-event key is used once in scope.");
            session.Reset();check("dialogue.reset",session.Text==""&&session.TryStart("intro","Yeni koşu",10),"New run has no stale text or event dedupe.");
            check("dialogue.length",PanicDialogueSessionV59.Clean(new string('a',400)).Length<=132,"Hard character cap.");
            check("layout.selected-target",Avoids(new Rect(1150,112,426,600)),"20 logical pixel gap to selected target.");
            check("layout.interaction-centre",Avoids(new Rect(550,420,500,230)),"Caption above physical aim point and action hint.");
            check("layout.bottom-controls",Avoids(new Rect(0,824,1600,76)),"Caption does not overlap bottom controls.");
            check("layout.small-font",Mathf.Abs(FontPixelsAt(960,540)-14.4f)<.001f,"24 logical px at960x540. Analytic geometry only; pixels/readability not rendered here.");
            CheckClip("Audio/V59/dialogue_bit",.025f,.18f,check);
            foreach(string id in new[]{"v56_hammer_hit","v56_gas_start","v56_gas_turn","v56_gas_close","v56_gas_cancel",
                "v56_water_start","v56_water_turn","v56_water_close","v56_water_cancel","v56_power_start","v56_power_turn",
                "v56_power_close","v56_power_cancel","v56_gas_flare","v56_pipe_leak","v56_electrical_short","v56_cloth","v56_zodiac_motor","v56_zodiac_water"})
                CheckClip("Audio/Foley/"+id,.02f,10f,check);
            foreach(string id in new[]{"jury_intro","jury_bag_ready","jury_secured","jury_quake_result","jury_flood","jury_fire","jury_end"})
                check("speech.absent."+id,!Resources.Load<AudioClip>("Audio/Jury/"+id),"Intelligible originals remain outside Resources.");
        }
        static void CheckClip(string path,float min,float max,Action<string,bool,string> check)
        {
            var clip=Resources.Load<AudioClip>(path);
            bool valid=clip&&clip.length>=min&&clip.length<=max&&clip.channels==1&&clip.frequency>=22050&&clip.loadState!=AudioDataLoadState.Failed;
            check("import."+path,valid,clip?"Actual imported clip: seconds="+clip.length+", channels="+clip.channels+", rate="+clip.frequency:"Missing imported asset.");
            if(!valid)return;
            if(clip.loadState==AudioDataLoadState.Unloaded)clip.LoadAudioData();
            var data=new float[clip.samples*clip.channels];
            if(!clip.GetData(data,0)){check("pcm."+path,false,"Imported clip PCM inaccessible; no fabricated measurement.");return;}
            float peak=0f;double sum=0;
            foreach(float value in data){peak=Mathf.Max(peak,Mathf.Abs(value));sum+=(double)value*value;}
            double rms=Math.Sqrt(sum/Math.Max(1,data.Length));
            check("pcm."+path,peak>.0001f&&peak<.99f&&rms>.000001,"Actual Unity-decoded PCM peak="+peak+", RMS="+rms+". Does not certify audible output, timbre or human listening.");
        }
    }
}
