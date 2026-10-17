using System;

using System.Collections;

using System.Collections.Generic;

using System.IO;

using System.Linq;

using UnityEngine;



namespace TooLateToPanic

{

    // Opt-in, isolated profile. Validates imported audio and production event hooks;

    // does not claim OS input or human listening acceptance.

    public sealed class PanicAudioSmoke : MonoBehaviour

    {

        [Serializable] public sealed class Result { public bool passed; public int checks, failures; public bool humanListening=false, OSInput=false; public string evidenceType="Native imported clips, AudioSource routing, semantic interaction fixture"; public List<string> results=new List<string>(); }

        Result result=new Result(); PanicHomeWorld world;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]

        static void Attach()

        {

            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--audio-smoke")<0)return;

            new GameObject("Audio smoke isolated").AddComponent<PanicAudioSmoke>();

        }

        void Check(string name,bool pass) { result.checks++;if(!pass)result.failures++;result.results.Add((pass?"PASS ":"FAIL ")+name);Debug.Log("AUDIO_CHECK "+(pass?"PASS ":"FAIL ")+name); }

        IEnumerator Start()

        {

            Application.runInBackground=true;

            yield return null; yield return null;

            world=FindFirstObjectByType<PanicHomeWorld>();

            if(!world||string.IsNullOrEmpty(world.EvidencePath)) { Debug.LogError("Audio smoke requires --home-evidence isolated directory");Application.Quit(2);yield break; }

            string[] ids={"dialogue_blip","step_wood","step_tile","step_grass","step_concrete","door_wood","door_metal","door_fridge","door_cabinet","door_toolbox","garage","screw","wood_build","soil_build","sandbag","hose","pickup_metal","pickup_bottle","pickup_cloth","bag","craft","battery","furniture","jump","computer","ignite","ui","reject","warning","success","fail","impact_wood","impact_metal","impact_glass","earthquake","flood","fire","storm","roomtone","garden","fireplace","rotor","engine_car","engine_diesel","siren_police","siren_ambulance","siren_fire","siren_rescue","heartbeat","bunker_hatch"};

            foreach(string id in ids)

            {

                var clip=PanicAudioBank.Get(id);Check("import "+id,clip&&clip.length>.02f&&clip.loadState!=AudioDataLoadState.Failed);

                if(clip) { clip.LoadAudioData();var data=new float[clip.samples*clip.channels];clip.GetData(data,0);float peak=0,sum=0;foreach(float x in data){peak=Mathf.Max(peak,Mathf.Abs(x));sum+=x*x;}Check("non silent no clipping "+id,peak<.99f&&sum/data.Length>.000000001f); }

            }

            foreach(string id in new[]{"jury_intro","jury_bag_ready","jury_secured","jury_quake_result","jury_flood","jury_fire","jury_end"})Check("spoken Resources absent "+id,!Resources.Load<AudioClip>("Audio/Jury/"+id));

            world.StartGame(false);yield return null;
            var guide=world.GetComponent<PanicHomeHud>();
            if(guide&&guide.FreeplayOnboardingOpenV414){guide.ConfirmFreeplayOnboardingV414();yield return null;}
            if(world.IntroStoryActiveV414){world.SkipIntroStoryV414();yield return null;}
            Check("genuine guide and intro resolved",!world.Model.IsPaused&&!world.InputLocked);
            var surfaces=new[]{
                new Vector3(-10,.05f,0),new Vector3(0,.05f,-8),new Vector3(0,.05f,-19),new Vector3(0,.10f,-14.88f),
                new Vector3(-4,.05f,-2),new Vector3(4,.05f,3),new Vector3(4,3.25f,4),new Vector3(-4,-3.15f,-2),
                new Vector3(12,.05f,2),new Vector3(-65,.05f,60)};
            var expected=new[]{"step_grass","paved","step_concrete","step_concrete","step_wood","step_tile","step_tile","step_concrete","step_concrete","step_concrete"};
            for(int i=0;i<surfaces.Length;i++){
                string actual=world.FootstepSurfaceAt(surfaces[i]);
                File.WriteAllText(Path.Combine(world.EvidencePath,"footstep-trace-"+i+".json"),JsonUtility.ToJson(world.FootstepSurfaceTraceAt(surfaces[i]),true));
                Check("visible footstep "+i+" actual="+actual,expected[i]=="paved"?actual=="step_concrete"||actual=="step_tile":actual==expected[i]);
            }
            var seamOffsets=new[]{new Vector3(-.06f,0,-.06f),new Vector3(-.06f,0,.06f),new Vector3(.06f,0,-.06f),new Vector3(.06f,0,.06f),new Vector3(0,.1f,0)};
            for(int i=0;i<seamOffsets.Length;i++){
                Vector3 probe=new Vector3(-4,.05f,-2)+seamOffsets[i];
                Check("footprint seam stability "+i,world.FootstepSurfaceAt(probe)=="step_wood");
                File.WriteAllText(Path.Combine(world.EvidencePath,"footstep-seam-trace-"+i+".json"),JsonUtility.ToJson(world.FootstepSurfaceTraceAt(probe),true));
            }
            Check("footprint wood side of material boundary",world.FootstepSurfaceAt(new Vector3(3.85f,3.25f,4))=="step_wood");
            Check("footprint tile side of material boundary",world.FootstepSurfaceAt(new Vector3(4.15f,3.25f,4))=="step_tile");
            Check("vehicle gain near middle far",PanicHomeEffects.DistanceGain(3,3,34)>PanicHomeEffects.DistanceGain(18,3,34)&&PanicHomeEffects.DistanceGain(18,3,34)>PanicHomeEffects.DistanceGain(34,3,34)&&PanicHomeEffects.DistanceGain(34,3,34)==0);
            Check("rotor gain near far",PanicHomeEffects.DistanceGain(12,12,75)==1&&PanicHomeEffects.DistanceGain(75,12,75)==0);
            Check("music lower default",Mathf.Abs(PanicBackgroundMusic.Volume-.075f)<.00001f);

            foreach(string id in PanicBackgroundMusic.TrackIds)

            {

                var track=PanicBackgroundMusic.LoadTrack(id);

                Check("music imported "+id,track&&track.length>50&&track.length<70&&track.channels==2);

            }

            yield return new WaitForSecondsRealtime(1.5f);

            var music=FindFirstObjectByType<PanicBackgroundMusic>();

            Check("music phase preparation",music&&music.CurrentTrack=="preparation"&&music.IsAudible);

            PanicBackgroundMusic.Enabled=false;yield return null;

            Check("music toggle off",music&&!music.IsAudible);

            PanicBackgroundMusic.Enabled=true;yield return null;

            Check("music toggle resume",music&&music.IsAudible);

            if(music)

            {

                var sources=music.GetComponents<AudioSource>();

                Check("music owns exactly two sources",sources.Length==2);

                Check("music streaming stereo",PanicBackgroundMusic.TrackIds.All(id=>{var c=PanicBackgroundMusic.LoadTrack(id);return c&&c.loadType==AudioClipLoadType.Streaming&&c.channels==2;}));

                var current=sources.FirstOrDefault(s=>s.isPlaying&&s.clip);

                if(current)current.time=current.clip.length-.9f;

                yield return new WaitForSecondsRealtime(.25f);

                Check("music loop real two-source overlap",sources.Count(s=>s.isPlaying)==2);

                yield return new WaitForSecondsRealtime(1.25f);

                Check("music loop outgoing source stopped",sources.Count(s=>s.isPlaying)==1&&music.IsAudible);

                world.StartDisasterSimulation(DisasterType.Flood);yield return null;world.TriggerSimulation();yield return new WaitForSecondsRealtime(1.5f);

                Check("music real disaster phase",world.Model.Phase==PanicPhase.Disaster&&music.CurrentTrack=="disaster"&&music.IsAudible);

                world.ReturnToMenu();yield return new WaitForSecondsRealtime(1.5f);

                Check("music return calm",music.CurrentTrack=="preparation"&&music.IsAudible&&sources.Count(s=>s.isPlaying)==1);

                world.StartGame(false);yield return null;

            }

            var fx=world.GetComponent<PanicHomeEffects>();

            Check("normal campaign general foley enabled",!PanicHomeWorld.DisasterAudioOnly);

            foreach(string id in ids)

            {

                int before=fx.PlayedCount;fx.Play(id,.12f);Check("production router "+id,fx.PlayedCount==before+1&&fx.LastPlayed==id);

                yield return new WaitForSecondsRealtime(.08f);

            }

            int money=world.Model.Budget;

            world.ToggleInventory();Check("inventory event bag",fx.LastPlayed=="bag");world.ToggleInventory();

            world.Warp(new Vector3(6.5f,.05f,3.3f));world.SetFocus("fridge");yield return null;

            int count=fx.PlayedCount;world.ToggleContainer();Check("fridge event routed",fx.PlayedCount>count&&fx.LastPlayed=="door_fridge");

            Check("audio did not spend budget",world.Model.Budget==money);

            var node=new GameObject("Isolated subtitle audio test");var speech=node.AddComponent<PanicPresentationAudio>();speech.Configure(true);

            Check("subtitle event accepted",speech.TryPlay("jury_intro"));Check("subtitle begins empty",speech.Subtitle.Length==0);

            speech.Tick(.1f,false);Check("typewriter partial",speech.Subtitle.Length>0&&speech.Subtitle.Length<44);

            string frozen=speech.Subtitle;speech.Tick(1,true);Check("pause freezes subtitle",speech.Subtitle==frozen);

            speech.Tick(.2f,false);Check("resume advances subtitle",speech.Subtitle.Length>frozen.Length);

            foreach(var source in node.GetComponentsInChildren<AudioSource>())Check("subtitle source only blip",!source.clip||source.clip.name=="dialogue_blip");

            Check("once per event",!speech.TryPlay("jury_intro"));speech.ResetRun();Check("reset clears subtitle",speech.Subtitle=="");Check("reset allows next run",speech.TryPlay("jury_intro"));

            speech.Configure(false);Check("disable cancels subtitle",speech.Subtitle=="");Destroy(node);

            yield return new WaitForSecondsRealtime(.1f);

            fx.SetSuspended(true);int pausedCount=fx.PlayedCount;fx.Play("screw");Check("paused router blocks world events",fx.PlayedCount==pausedCount);fx.SetSuspended(false);

            result.passed=result.failures==0;

            File.WriteAllText(Path.Combine(world.EvidencePath,"audio-smoke.json"),JsonUtility.ToJson(result,true));

            ScreenCapture.CaptureScreenshot(Path.Combine(world.EvidencePath,"audio-smoke.png"));yield return new WaitForSecondsRealtime(.2f);

            Debug.Log("AUDIO_SMOKE_COMPLETE "+result.passed+" checks="+result.checks);Application.Quit(result.passed?0:1);

        }

    }

}

