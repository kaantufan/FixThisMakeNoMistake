using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Opt-in isolated assistant production API/framebuffer smoke. Root owns native execution.</summary>
    public sealed class PanicV55Smoke : MonoBehaviour
    {
        const string Flag = "--v55-smoke";
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id,status,detail,layer; public int frame; }
        [Serializable] public sealed class Shot { public string path,context; public int width,height,frame; }
        [Serializable] public sealed class AudioProbe
        {
            public int frame,sampleIndex,clipSamples,clipChannels,clipFrequency,timeSamples,activeListeners,outputSampleRate,dspBufferLength,dspBufferCount;
            public double dspTime;
            public float wallSeconds,sourceOutputRms,listenerOutputRms,masterVolume,sourceVolume,sourcePitch,sourceSpatialBlend,cinematicGain,measuredClipRms;
            public bool sourceExists,sourcePlaying,sourceVirtual,sourceMute,sourceEnabled,sourceNodeActive,listenerPaused,cinematicPredicate,worldCinematic;
            public string phase,clipName,clipLoadState,mixerGroup;
        }
        [Serializable] public sealed class Receipt
        {
            public string utc,dllSha256,realScoreBefore,realScoreAfter,scorePath,error;
            public string evidence="Native isolated production API/model + actual graphical ScreenCapture. Explicit normal StartGame, one clock fixture, production finite-container/finite-supply calls and production validated work/rotation/commit APIs. Controller placement and view fixture only for final real shelter gate. After progression, utility-reset plus one immutable memoized2pt gas-flare fixture tests actual natural resolution/display. Temporary own-player AudioListener1 fixture for technical sourcePCM, restored. No OS keyboard/mouse, continuous traversal, human legibility/fun, listening or full campaign acceptance.";
            public string renderLimit="PNG dimensions certify framebuffer file capture; root must inspect pixels for actual text fit/style. Runtime draw counters certify hidden/visible draw branches, not aesthetic acceptance.";
            public bool complete,passed,osInput,humanAcceptance,listening;
            public float seconds;
            public int timeFixtures,workTimeFixtureTicks,setupWarps,lifecycleFixtures,utilityResetFixtures,memoizedEventFixtures,listenerFixtures,productionStartCalls,productionContainerCalls,productionSupplyCalls,productionWorkInputCalls,productionWorkCommits;
            public float fixtureWorkSeconds;
            public float sampledFpsSeconds,veilMinimum,veilMaximum,rollRange,fovRange,quakeOutputRms;
            public double quakeDspDelta;
            public int quakeAudioSampleFrames;
            public List<AudioProbe> quakeAudioProbes=new List<AudioProbe>();
            public List<Row> checks=new List<Row>(); public List<Shot> captures=new List<Shot>(); public List<string> errors=new List<string>();
        }
        readonly Receipt report=new Receipt();
        PanicHomeWorld w; PanicHomeHud hud; PanicNotebookOverlay book; float began; string realScore; bool finished,qaListenerFixtureActive;float qaListenerPrior;
        void RestoreQaListener(){if(qaListenerFixtureActive){AudioListener.volume=qaListenerPrior;qaListenerFixtureActive=false;}}
        void OnDisable(){RestoreQaListener();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Attach()
        { if(Environment.GetCommandLineArgs().Contains(Flag)){var w=FindFirstObjectByType<PanicHomeWorld>();if(w)w.gameObject.AddComponent<PanicV55Smoke>();} }
        void Start()
        {
            w=GetComponent<PanicHomeWorld>();hud=GetComponent<PanicHomeHud>();book=w.Notebook;began=Time.realtimeSinceStartup;
            realScore=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");
            report.utc=DateTime.UtcNow.ToString("o");report.dllSha256=Hash(typeof(PanicModel).Assembly.Location);report.realScoreBefore=Hash(realScore);report.scorePath=PanicModel.ScorePathOverride;
            Application.logMessageReceived+=Log;StartCoroutine(Guard(Run()));
        }
        void Update(){if(!finished&&Time.realtimeSinceStartup-began>80){report.error="80s feature smoke hard cap";Finish();}}
        void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(s+"\n"+stack);}
        void Check(string id,bool pass,string detail="",string layer="NATIVE_API_FRAME")
        {report.checks.Add(new Row{id=id,status=pass?"PASS":"FAIL",detail=detail,layer=layer,frame=Time.frameCount});Debug.Log("V55_CHECK "+(pass?"PASS ":"FAIL ")+id+" "+detail);}
        static object Member(object owner,string name)
        {var t=owner.GetType();var p=t.GetProperty(name,Flags);return p!=null?p.GetValue(owner):t.GetField(name,Flags).GetValue(owner);}
        static T Read<T>(object owner,string name){return (T)Member(owner,name);}
        static object Invoke(object owner,string name,params object[] args)
        {return owner.GetType().GetMethods(Flags).First(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(owner,args);}
        object Assistant{get{return Member(w,"AssistantV55");}}
        int Step{get{return Read<int>(Assistant,"CurrentStep");}}
        object[] Steps{get{return ((IEnumerable)Member(Assistant,"Steps")).Cast<object>().ToArray();}}
        bool AssistantEnabled{get{return Read<bool>(w,"AssistantEnabledV55");}set{var p=w.GetType().GetProperty("AssistantEnabledV55",Flags);if(p.CanWrite)p.SetValue(w,value);else Invoke(w,"SetAssistantEnabledV55",value);}}
        bool Available{get{return Read<bool>(Assistant,"Available");}}
        bool HudVisible{get{return Read<bool>(hud,"AssistantHudVisibleV55");}}
        int DrawCount{get{return Read<int>(hud,"AssistantHudDrawFramesV55");}}
        string Fingerprint()
        {
            var m=w.Model;string s=PanicNotebookVerification.GameplayFingerprint(w)+"|U:"+m.UtilitySafety.GasClosed+":"+m.UtilitySafety.WaterClosed+":"+m.UtilitySafety.PowerOff+"|S:"+m.ReachedShelterV5+"|P:"+w.PlayerPosition+"|F:"+w.ActiveFloor+"|PH:"+m.Phase+"|R:"+m.SecondaryRollCountV5+"|D:"+m.TotalDamage;
            foreach(var supply in w.Expedition.WorldSupplies)s+="|WS:"+supply.Id+":"+supply.Count+":"+supply.Taken;
            return s;
        }
        bool Isolated(){return !string.IsNullOrEmpty(w.EvidencePath)&&!string.IsNullOrEmpty(PanicModel.ScorePathOverride)&&string.Equals(Path.GetFullPath(PanicModel.ScorePathOverride),Path.Combine(Path.GetFullPath(w.EvidencePath),"isolated-home-scores.json"),StringComparison.OrdinalIgnoreCase);}
        IEnumerator Guard(IEnumerator body)
        {var stack=new Stack<IEnumerator>();stack.Push(body);while(stack.Count>0&&!finished){bool next;object current=null;try{next=stack.Peek().MoveNext();if(next)current=stack.Peek().Current;}catch(Exception e){report.error=e.ToString();break;}if(!next){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;}if(!finished){report.complete=string.IsNullOrEmpty(report.error);Finish();}}
        IEnumerator Wait(float t){float end=Time.realtimeSinceStartup+t;while(Time.realtimeSinceStartup<end)yield return null;}
        IEnumerator Resolution(int width,int height)
        {Screen.SetResolution(width,height,FullScreenMode.Windowed);float end=Time.realtimeSinceStartup+2;while((Screen.width!=width||Screen.height!=height)&&Time.realtimeSinceStartup<end)yield return null;yield return Wait(.25f);Check("framebuffer."+width,Screen.width==width&&Screen.height==height,Screen.width+"x"+Screen.height,"NATIVE_GRAPHICAL_SURFACE");}
        IEnumerator Capture(string name,string context)
        {
            yield return new WaitForEndOfFrame();string path=Path.Combine(w.EvidencePath,name+".png");int width=Screen.width,height=Screen.height,frame=Time.frameCount;ScreenCapture.CaptureScreenshot(path);
            float end=Time.realtimeSinceStartup+2;int pw=0,ph=0;while(!PngSize(path,out pw,out ph)&&Time.realtimeSinceStartup<end)yield return null;
            report.captures.Add(new Shot{path=path,context=context,width=pw,height=ph,frame=frame});Check("capture."+name,pw==width&&ph==height,pw+"x"+ph+"; "+context,"NATIVE_FRAMEBUFFER_FILE");
        }
        IEnumerator HudState(string label,bool visible)
        {
            yield return Wait(.08f);int before=DrawCount;yield return Wait(.10f);yield return new WaitForEndOfFrame();Check("hud."+label,HudVisible==visible&&(visible?DrawCount>before:DrawCount==before),"visible="+HudVisible+" draws="+(DrawCount-before),"NATIVE_REPAINT_COUNTER");
            if(visible)
            {
                Rect r=Read<Rect>(hud,"AssistantRenderedBoundsV55");var offset=hud.CanvasOffset;float scale=hud.CanvasScale;var pixels=new Rect(offset.x+r.x*scale,offset.y+r.y*scale,r.width*scale,r.height*scale);
                Check("hud.bounds."+label,pixels.xMin>=0&&pixels.yMin>=0&&pixels.xMax<=Screen.width&&pixels.yMax<=Screen.height&&!pixels.Overlaps(book.LauncherScreenBounds),"Actual rendered bounds="+pixels+"; notebook launcher="+book.LauncherScreenBounds,"NATIVE_GUI_LAYOUT");
            }
        }
        void SnapshotInvariant(string label)
        {
            string before=Fingerprint();float clock=w.Model.TimeRemaining;int count=0;string first="";
            for(int i=0;i<16;i++) {var a=Assistant;var s=Steps;count=s.Length;if(i==0)first=Read<string>(a,"Task");}
            Check("snapshot.readonly."+label,before==Fingerprint()&&clock==w.Model.TimeRemaining,"16 same-frame real snapshot reads, task="+first);
            Check("snapshot.sequence."+label,count==Read<int>(Assistant,"StepCount")&&count==6&&Step>=1&&Step<=6,"step="+Step+" count="+count);
            var rows=Steps;int active=rows.Count(s=>Read<bool>(s,"Active"));Check("snapshot.single-active."+label,active<=1,"active="+active);
            Check("snapshot.completed-not-active."+label,rows.All(s=>!Read<bool>(s,"Complete")||!Read<bool>(s,"Active")));
        }
        bool Work(string action,string target)
        {
            var e=w.Expedition;int budget=w.Model.Budget,cost=e.WorkBudgetCostV414(action,target);var costs=e.WorkMaterialCostsV414(action,target).ToArray();var before=costs.ToDictionary(c=>c.Id,c=>e.Count(c.Id));
            if(!e.BeginValidatedWorkV414(action,target,false)){Check("work.begin."+action,false,target);return false;}
            for(int g=0;g<64&&e.Work.Active&&!e.Work.Complete;g++)
            {
                if(e.Work.UsesDirectClick)e.Work.TryClick(e.Work.CompletedSteps);
                // Derive the active work contract from the actual production session.
                // Historical V4.9 auto-install sessions need explicit .60s Tick fixtures;
                // current V4.14 manual handles/anchors need genuine displacement APIs.
                if(e.Work.InstallSeconds>.001f)
                {for(int tick=0;tick<7&&!e.Work.Complete;tick++){e.Work.Tick(.1f,0,false,false,false);report.workTimeFixtureTicks++;report.fixtureWorkSeconds+=.1f;}}
                else if(e.Work.UsesLinearDrag)e.Work.ApplyLinearDisplacement(.5f,true);
                else if(e.Work.UsesRotation)e.Work.ApplyRotation(60f*e.Work.ExpectedDirection,true);
                else e.Work.PressHammer();
                report.productionWorkInputCalls++;
            }
            bool committed=e.TryCompleteWork(action,target);if(committed)report.productionWorkCommits++;
            Check("work.atomic."+action,committed&&w.Model.Budget==budget-cost&&costs.All(c=>e.Count(c.Id)==before[c.Id]-c.Required),target+"; canonical costs");return committed;
        }
        IEnumerator FpsTension()
        {
            float start=Time.realtimeSinceStartup,clock=w.Model.TimeRemaining,minVeil=1,maxVeil=0,minRoll=99,maxRoll=-99,minFov=99,maxFov=0;Vector3 position=w.PlayerPosition;Quaternion look=w.FpsInputRotationV54;bool darkShot=false,reliefShot=false;
            while(Time.realtimeSinceStartup-start<9f)
            {
                minVeil=Mathf.Min(minVeil,w.FpsVeilOpacityV414);maxVeil=Mathf.Max(maxVeil,w.FpsVeilOpacityV414);float roll=Mathf.DeltaAngle(0,w.View.transform.eulerAngles.z);minRoll=Mathf.Min(minRoll,roll);maxRoll=Mathf.Max(maxRoll,roll);minFov=Mathf.Min(minFov,w.View.fieldOfView);maxFov=Mathf.Max(maxFov,w.View.fieldOfView);
                if(!darkShot&&w.FpsVeilOpacityV414>=.79f){darkShot=true;yield return Capture("06a-fps-dark-cycle","Actual strong-darkness phase, opacity="+w.FpsVeilOpacityV414);}
                if(!reliefShot&&w.FpsVeilOpacityV414<=.24f){reliefShot=true;yield return Capture("06b-fps-clear-relief","Actual periodic clear-relief phase, opacity="+w.FpsVeilOpacityV414);}
                yield return null;
            }
            report.sampledFpsSeconds=Time.realtimeSinceStartup-start;report.veilMinimum=minVeil;report.veilMaximum=maxVeil;report.rollRange=maxRoll-minRoll;report.fovRange=maxFov-minFov;
            Check("fps.actual-darkness-relief-cycle",minVeil<=.24f&&maxVeil>=.79f&&maxVeil<=.881f,"9s actual opacity range="+minVeil+".."+maxVeil,"NATIVE_RENDER_STATE_FRAMES");
            Check("fps.actual-continuous-bounded-camera",report.rollRange>1&&report.rollRange<4.5f&&report.fovRange>.5f&&report.fovRange<2.3f,"roll range="+report.rollRange+" fov range="+report.fovRange,"NATIVE_CAMERA_FRAMES");
            Check("fps.cosmetics-preserve-real-clock-actor-look",Vector3.Distance(position,w.PlayerPosition)<.01f&&Quaternion.Angle(look,w.FpsInputRotationV54)<.001f&&Mathf.Abs((clock-w.Model.TimeRemaining)-report.sampledFpsSeconds)<.12f,"Natural9s clock advance="+(clock-w.Model.TimeRemaining)+"; wall="+report.sampledFpsSeconds);
            var clip=Read<AudioClip>(w,"FpsTensionClipV55");bool pcm=false;float rms=0;
            if(clip){var data=new float[clip.samples*clip.channels];pcm=clip.GetData(data,0);double sum=0;foreach(var value in data)sum+=value*value;rms=(float)Math.Sqrt(sum/Mathf.Max(1,data.Length));}
            Check("fps.tension-actual-source-clip",Read<bool>(w,"FpsTensionPlayingV55")&&Read<float>(w,"FpsTensionVolumeV55")>0&&Read<float>(w,"FpsTensionVolumeV55")<=.12f&&clip&&pcm&&rms>.01f,"Actual playing source; actual clip PCM RMS="+rms+"; AudioListener="+AudioListener.volume,"NATIVE_SOURCE_AND_CLIP_PCM_NOT_LISTENING");
            Check("fps.one-music-owner",!PanicBackgroundMusic.Instance||PanicBackgroundMusic.Instance.GetComponents<AudioSource>().All(s=>!s.isPlaying),"Actual prior background-music source pair must stop while generated FPS score owns music","NATIVE_AUDIO_SOURCE_STATE");
            hud.SetPaused(true);yield return Wait(.12f);Check("fps.pause-stops-tension",!Read<bool>(w,"FpsTensionPlayingV55"));hud.SetPaused(false);yield return Wait(.30f);Check("fps.unpause-resumes-tension",Read<bool>(w,"FpsTensionPlayingV55"));
            w.SetReducedFpsMotionV54(true);yield return Wait(.15f);var still=w.View.transform.rotation;float fov=w.View.fieldOfView;yield return Wait(.22f);Check("fps.reduced-motion-steady",Quaternion.Angle(still,w.View.transform.rotation)<.001f&&Mathf.Abs(fov-w.View.fieldOfView)<.001f&&Mathf.Abs(w.FpsVeilOpacityV414-.58f)<.001f);w.SetReducedFpsMotionV54(false);
        }
        IEnumerator QuakeAndSecondaryAlert()
        {
            w.Model.UtilitySafety.Reset();report.utilityResetFixtures++;
            var baseRisk=typeof(PanicModel).GetMethod("BaseRisk",BindingFlags.Static|BindingFlags.NonPublic);
            var target=w.Model.Items.First(i=>i.Installed&&i.Kind==ItemKind.Furniture&&(float)baseRisk.Invoke(null,new object[]{i,DisasterType.Earthquake})>0);
            var memo=Read<Dictionary<string,PanicSecondaryEventV5>>(w.Model,"secondaryRollsV5");memo[w.Model.Wave+":"+(int)DisasterType.Earthquake]=new PanicSecondaryEventV5(PanicSecondaryEventKindV5.GasFlare,target.Id,2f,"Gaz vanası açık kaldı: küçük bir alevlenme oldu");report.memoizedEventFixtures++;
            Check("secondary.no-alert-before-real-resolve",!Read<bool>(hud,"SecondaryAlertVisibleV55")&&w.Model.SecondaryEventsV5.Count==0,"Explicit memoized allowed event fixture only; no published result yet");
            var fx=w.GetComponent<PanicHomeEffects>();float rms=0;double dspStart=AudioSettings.dspTime;
            qaListenerPrior=AudioListener.volume;qaListenerFixtureActive=true;report.listenerFixtures++;
            try
            {
                AudioListener.volume=1;yield return Wait(.70f);
                for(int sample=0;sample<16;sample++)
                {
                    var source=Read<AudioSource>(fx,"QuakeCinematicSourceV55");var clip=source?source.clip:null;float[] data=new float[4096],master=new float[4096];float current=0,mixed=0;
                    if(source){source.GetOutputData(data,0);double sum=0;foreach(float v in data)sum+=v*v;current=(float)Math.Sqrt(sum/data.Length);}
                    AudioListener.GetOutputData(master,0);double mixedSum=0;foreach(float v in master)mixedSum+=v*v;mixed=(float)Math.Sqrt(mixedSum/master.Length);rms=Mathf.Max(rms,current);
                    AudioSettings.GetDSPBufferSize(out int bufferLength,out int bufferCount);
                    report.quakeAudioProbes.Add(new AudioProbe{frame=Time.frameCount,sampleIndex=sample,dspTime=AudioSettings.dspTime,wallSeconds=Time.realtimeSinceStartup-began,sourceExists=source,sourcePlaying=source&&source.isPlaying,sourceVirtual=source&&source.isVirtual,sourceMute=source&&source.mute,sourceEnabled=source&&source.enabled,sourceNodeActive=source&&source.gameObject.activeInHierarchy,sourceVolume=source?source.volume:0,sourcePitch=source?source.pitch:0,sourceSpatialBlend=source?source.spatialBlend:0,timeSamples=source?source.timeSamples:0,clipName=clip?clip.name:"absent",clipLoadState=clip?clip.loadState.ToString():"absent",clipSamples=clip?clip.samples:0,clipChannels=clip?clip.channels:0,clipFrequency=clip?clip.frequency:0,mixerGroup=source&&source.outputAudioMixerGroup?source.outputAudioMixerGroup.name:"default",masterVolume=AudioListener.volume,listenerPaused=AudioListener.pause,sourceOutputRms=current,listenerOutputRms=mixed,cinematicPredicate=Read<bool>(fx,"QuakeCinematicAudioPlayingV55"),cinematicGain=Read<float>(fx,"QuakeCinematicAudioGainV55"),measuredClipRms=Read<float>(fx,"QuakeCinematicClipRmsV55"),worldCinematic=w.CinematicActive,phase=w.Model.Phase.ToString(),activeListeners=FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(a=>a.isActiveAndEnabled),outputSampleRate=AudioSettings.outputSampleRate,dspBufferLength=bufferLength,dspBufferCount=bufferCount});
                    report.quakeAudioSampleFrames++;yield return Wait(.04f);
                }
            }
            finally{RestoreQaListener();}
            report.quakeOutputRms=rms;report.quakeDspDelta=AudioSettings.dspTime-dspStart;
            string audioDetail="Listener restored="+AudioListener.volume+"; maxSourceRms="+rms+"; dspDelta="+report.quakeDspDelta+"; observedFrames="+report.quakeAudioSampleFrames+"; states="+JsonUtility.ToJson(report.quakeAudioProbes[report.quakeAudioProbes.Count-1]);
            Check("quake.real-cinematic-source-output",Read<bool>(fx,"QuakeCinematicAudioPlayingV55")&&Read<float>(fx,"QuakeCinematicAudioGainV55")>.1f&&Read<float>(fx,"QuakeCinematicClipRmsV55")>.0001f&&rms>.000001f,audioDetail,"NATIVE_SOURCE_PCM_NOT_LISTENING");
            Check("quake.actual-dsp-advances",report.quakeDspDelta>.5&&report.quakeAudioSampleFrames==16,"Actual DSP delta="+report.quakeDspDelta+"; 16 unforced sequential source/master output observations","NATIVE_AUDIO_DSP_CLOCK");
            float until=Time.realtimeSinceStartup+10;while(w.Model.Phase==PanicPhase.Disaster&&Time.realtimeSinceStartup<until)yield return null;yield return Wait(.10f);
            Check("secondary.actual-committed-event",w.Model.SecondaryEventsV5.Count==1&&w.Model.SecondaryEventsV5[0].Kind==PanicSecondaryEventKindV5.GasFlare&&Mathf.Abs(w.Model.SecondaryDamageV5-2)<.001f,"Natural8s production resolution; explicit2pt memoized RNG fixture");
            Check("secondary.big-alert-live",Read<bool>(hud,"SecondaryAlertVisibleV55")&&Read<string>(hud,"SecondaryAlertTextV55").Contains("Gaz vanası")&&Read<int>(hud,"SecondaryAlertDrawFramesV55")>0,"Actual committed result text and Repaint count");
            int rolls=w.Model.SecondaryRollCountV5;float damage=w.Model.TotalDamage;string summary=w.Model.SecondaryEventSummaryV5;float timer=Read<float>(hud,"secondaryAlertRemainingV55");
            for(int i=0;i<16;i++){var a=Assistant;Invoke(hud,"TickSecondaryAlertV55",0f);}Check("secondary.snapshot-no-reroll-no-duplicate",rolls==w.Model.SecondaryRollCountV5&&damage==w.Model.TotalDamage&&summary==w.Model.SecondaryEventSummaryV5&&Mathf.Abs(timer-Read<float>(hud,"secondaryAlertRemainingV55"))<.001f,"16 read/tick0 calls keep memoized roll, damage and remaining7s alert age");
            yield return Capture("09-committed-utility-alert-1600","Big actual committed gas-flare alert; source event is explicit2pt fixture, production8s resolve and HUD draw are real.");
            hud.SetPaused(true);yield return Wait(.10f);timer=Read<float>(hud,"secondaryAlertRemainingV55");yield return Wait(.25f);Check("secondary.pause-hides-holds-age",!Read<bool>(hud,"SecondaryAlertVisibleV55")&&Mathf.Abs(timer-Read<float>(hud,"secondaryAlertRemainingV55"))<.001f);hud.SetPaused(false);yield return Wait(.16f);Check("secondary.resume-no-retrigger",Read<bool>(hud,"SecondaryAlertVisibleV55")&&Read<float>(hud,"secondaryAlertRemainingV55")<timer&&w.Model.SecondaryRollCountV5==rolls);
            Check("quake.finished-source-cleanup",!Read<bool>(fx,"QuakeCinematicAudioPlayingV55"));
        }
        IEnumerator NotebookPlan()
        {
            string before=Fingerprint();float clock=w.Model.TimeRemaining;
            foreach(int floor in new[]{0,1,-1})
            {
                book.SelectFloor(floor);yield return Wait(.10f);object plan=Member(book,"PlanSnapshotV55");object[] shapes=((IEnumerable)Member(plan,"Shapes")).Cast<object>().ToArray();
                int walls=Read<int>(plan,"WallCount"),doors=Read<int>(plan,"DoorCount"),furniture=Read<int>(plan,"FurnitureCount"),stairs=Read<int>(plan,"StairCount"),decks=shapes.Count(s=>Member(s,"Kind").ToString()=="Floor");
                Check("plan.actual-geometry.floor"+floor,Read<int>(plan,"Floor")==floor&&walls>0&&decks>0&&stairs>0,"Actual collider-derived wall="+walls+" deck="+decks+" door="+doors+" furniture="+furniture+" stair="+stairs,"NATIVE_SCENE_PLAN_DATA");
                Check("plan.counts-match-shapes.floor"+floor,walls==shapes.Count(s=>Member(s,"Kind").ToString()=="Wall")&&doors==shapes.Count(s=>Member(s,"Kind").ToString()=="Door")&&furniture==shapes.Count(s=>Member(s,"Kind").ToString()=="Furniture"));
                if(floor==1)Check("plan.upper-real-bathroom-door",shapes.Any(s=>Member(s,"Kind").ToString()=="Door"&&Read<string>(s,"Id")=="bathroom_door"),"Actual hinge identity bathroom_door, no invented room label");
                Bounds bounds=Read<Bounds>(plan,"WorldBounds");Vector3 p=bounds.center;var center=(Vector2)Invoke(book,"PlanWorldToScreenV55",p);var x=(Vector2)Invoke(book,"PlanWorldToScreenV55",p+Vector3.right)-center;var z=(Vector2)Invoke(book,"PlanWorldToScreenV55",p+Vector3.forward)-center;float scale=Read<float>(book,"PlanWorldScaleV55");
                Check("plan.uniform-unmirrored-metre.floor"+floor,x.x>0&&Mathf.Abs(x.y)<.01f&&z.y<0&&Mathf.Abs(z.x)<.01f&&Mathf.Abs(x.magnitude-z.magnitude)<.01f&&Mathf.Abs(x.magnitude-scale)<.01f,"1m X="+x+" 1m Z="+z+" scale="+scale,"NATIVE_GUI_WORLD_PROJECTION");
                if(w.ActiveFloor==floor)Check("plan.actual-actor-inside",Read<Rect>(book,"PlanScreenBoundsV55").Contains((Vector2)Invoke(book,"PlanWorldToScreenV55",w.PlayerPosition)),"Real actor="+w.PlayerPosition);
                yield return Capture("03a-plan-floor"+floor+"-1600","Actual collider/door/furniture-derived floor plan; floor="+floor+"; root inspect topology and legibility.");
            }
            yield return Resolution(960,540);foreach(int floor in new[]{0,1}){book.SelectFloor(floor);yield return Wait(.08f);yield return Capture("03b-plan-floor"+floor+"-960","Actual narrow framebuffer plan, floor="+floor);}
            yield return Resolution(1600,900);book.SelectFloor(w.ActiveFloor);Check("plan.readonly-real-clock-gameplay",before==Fingerprint()&&Mathf.Abs(clock-w.Model.TimeRemaining)<.005f,"Floor selection/capture kept actual notebook clock gate and all inventory, stock, safety, score, RNG and damage unchanged");
        }
        IEnumerator Run()
        {
            yield return Wait(.35f);Check("launch.isolated",Isolated()&&PanicTestLaunchRules.Drivers.Contains(Flag));Check("launch.graphical",!Application.isBatchMode&&SystemInfo.graphicsDeviceType!=GraphicsDeviceType.Null);if(!Isolated())yield break;
            yield return Resolution(1600,900);w.StartGame(false);report.productionStartCalls++;w.FullFlowScriptedInput=true;yield return null;if(hud.FreeplayOnboardingOpenV414)hud.ConfirmFreeplayOnboardingV414();yield return null;w.SkipIntroStoryV414();yield return Wait(.18f);book=w.Notebook;
            Check("intro.not-auto-open",!book.IsVisible&&!Read<bool>(book,"IntroVisibleV55")&&!Read<bool>(book,"IntroSeenV55"));
            SnapshotInvariant("initial");Check("assistant.enabled-initial",AssistantEnabled&&Available);Check("assistant.first-real-materials",Step==1&&!Read<bool>(Steps[0],"Complete"));
            Check("world.no-legacy-beacon",Read<int>(w,"LegacyGuidanceVisualCountV55")==0,"Runtime scan of actual scene legacy guidance objects/renderers","NATIVE_SCENE_SCAN");
            yield return HudState("iso-visible",true);yield return Capture("01-assistant-iso-1600","Actual production initial assistant card, existing HUD style; root inspect pixels.");
            float clock=w.Model.TimeRemaining;string before=Fingerprint();Check("intro.explicit-open",book.RequestOpen()&&Read<bool>(book,"IntroVisibleV55")&&Read<bool>(book,"IntroSeenV55")&&book.BlocksInput);yield return Wait(.43f);
            Check("intro.clock-and-gameplay-held",before==Fingerprint()&&Mathf.Abs(clock-w.Model.TimeRemaining)<.005f,"Real wall wait .43s; no clock fixture or developer freeze");
            Check("intro.continue-inside-book",book.BookScreenBounds.Contains(Read<Rect>(book,"IntroContinueScreenBoundsV55").center));
            yield return HudState("notebook-hide",false);yield return Capture("02-notebook-intro-1600","First explicit notebook opening, intro printed inside real book.");
            Check("intro.wrong-key-gated",!(bool)Invoke(book,"ProcessAssistantIntroKeyV55",KeyCode.Tab)&&Read<bool>(book,"IntroVisibleV55"));
            Check("intro.enter-continues",(bool)Invoke(book,"ProcessAssistantIntroKeyV55",KeyCode.Return)&&!Read<bool>(book,"IntroVisibleV55")&&book.Animation.TargetOpen&&book.InputBlockedThisFrame);
            Check("intro.continue-idempotent",!(bool)Invoke(book,"DismissAssistantIntroV55")&&book.Animation.TargetOpen);
            yield return Wait(.15f);yield return Capture("03-notebook-sequence-1600","Actual six-step model sequence after Enter semantic API; no OS input.");
            yield return NotebookPlan();
            before=Fingerprint();book.RequestClose();yield return Wait(.43f);Check("intro.repeat-open-no-duplicate",book.RequestOpen()&&!Read<bool>(book,"IntroVisibleV55")&&Read<bool>(book,"IntroSeenV55"));yield return Wait(.42f);book.RequestClose();yield return Wait(.43f);
            Check("notebook.readonly-controls",before==Fingerprint());
            // A separate actual overlay lifecycle instance tests early J-close without rewriting private seen state.
            var go=new GameObject("V55 QA explicit intro-key lifecycle fixture");var probe=go.AddComponent<PanicNotebookOverlay>();probe.Bind(w,()=>false);report.lifecycleFixtures++;
            Check("intro.J-semantic-close",probe.RequestOpen()&&Read<bool>(probe,"IntroVisibleV55")&&(bool)Invoke(probe,"ProcessAssistantIntroKeyV55",KeyCode.J)&&!probe.Animation.TargetOpen);
            Check("intro.J-repeat-not-reintroduced",probe.RequestOpen()&&!Read<bool>(probe,"IntroVisibleV55"));probe.ForceClosed();Destroy(go);yield return null;
            before=Fingerprint();AssistantEnabled=false;yield return HudState("helper-off",false);Check("helper-off.no-gameplay-change",before==Fingerprint()&&!AssistantEnabled);AssistantEnabled=true;yield return HudState("helper-on",true);Check("helper-on.no-gameplay-change",before==Fingerprint());
            hud.SetPaused(true);yield return HudState("pause-hide",false);hud.SetPaused(false);yield return HudState("unpause-visible",true);
            yield return Resolution(960,540);yield return Capture("04-assistant-iso-960","Actual narrow framebuffer; root inspect text fit and HUD overlap.");
            // Real finite material source opens are production model calls; no claimed physical walking/reach acceptance.
            foreach(var source in w.Expedition.Sources.ToArray())if(!source.AutoLooted&&!source.Open){report.productionContainerCalls++;w.Expedition.ToggleSource(source.Id);}
            yield return Wait(.12f);Check("sequence.materials-complete",Read<bool>(Steps[0],"Complete")&&Step==2,"Production finite container opens, no stock writes");
            string target=Read<string>(w,"AssistantTargetIdV55");var item=w.Model.Items.Find(i=>i.Id==target);Check("sequence.stable-real-target",item!=null&&!item.Anchored,target);
            if(item!=null)Work(PanicItemActionRules.PrimaryAction(item),item.Id);yield return Wait(.12f);Check("sequence.real-anchor-advances",Read<bool>(Steps[1],"Complete")&&Step==3,"Actual validated work, manual rotation API, atomic commit");
            foreach(string action in new[]{"gasoff","wateroff","poweroff"})Work(action,PanicUtilitySafetyV414.TargetForAction(action));yield return Wait(.12f);Check("sequence.utilities-real-complete",w.Model.UtilitySafety.CompletedCount==3&&Read<bool>(Steps[2],"Complete"));SnapshotInvariant("precautions-done");
            w.Model.TimeRemaining=45;report.timeFixtures++;w.TickPreparationStages();yield return Wait(2.2f);Check("fps.real-handoff",w.BagFirstPersonActive&&w.BagFinalStageActive);
            yield return HudState("fps-visible",true);Check("sequence.bag-water",Step==4&&!Read<bool>(Steps[3],"Complete"));yield return Capture("05-assistant-fps-water-960","TimeRemaining=45 explicit stage fixture; actual completed production camera handoff and missing water.");
            yield return Resolution(1600,900);yield return Capture("06-assistant-fps-water-1600","Same real missing-water assistant at native 1600x900.");
            yield return FpsTension();
            var water=w.Expedition.WorldSupplies.FirstOrDefault(s=>s.ItemId=="water"&&s.Count>0);int stock=water==null?0:water.Count;report.productionSupplyCalls++;Check("collect.real-water",water!=null&&w.Expedition.TryCollectWorldSupply(water.Id)&&water.Count<stock&&w.Model.PackedWater>=1);yield return Wait(.12f);Check("sequence.real-water-advances",Read<bool>(Steps[3],"Complete")&&Step==5);
            var food=w.Expedition.WorldSupplies.FirstOrDefault(s=>s.ItemId=="food"&&s.Count>0);stock=food==null?0:food.Count;report.productionSupplyCalls++;Check("collect.real-food",food!=null&&w.Expedition.TryCollectWorldSupply(food.Id)&&food.Count<stock&&w.Model.PackedFood>=1);yield return Wait(.12f);Check("sequence.real-food-advances",Read<bool>(Steps[4],"Complete")&&Step==6&&w.Model.BagReady);SnapshotInvariant("shelter");
            yield return Capture("07-assistant-fps-shelter-1600","Production packed water/food complete; physical correct shelter becomes the sole active step.");
            clock=w.Model.TimeRemaining;before=Fingerprint();Check("notebook.fps-open",book.RequestOpen());yield return Wait(.43f);Check("notebook.fps-readonly-clock",before==Fingerprint()&&Mathf.Abs(clock-w.Model.TimeRemaining)<.005f);Check("notebook.fps-stops-tension",!Read<bool>(w,"FpsTensionPlayingV55"));yield return Capture("08-notebook-live-sequence-1600","Six actual progress rows, water and food obtained through production finite collect APIs.");book.RequestClose();yield return Wait(.43f);
            w.Warp(new Vector3(-4.5f,-3.16f,1.4f));report.setupWarps++;for(int i=0;i<12;i++){w.SmokeMove(Vector2.zero,.025f);yield return null;}
            Vector3 d=PanicHomeWorld.BunkerPosition+Vector3.up*.35f-(w.PlayerPosition+Vector3.up*1.62f);var angles=Quaternion.LookRotation(d).eulerAngles;float yaw=Read<float>(w,"bagYaw"),pitch=Read<float>(w,"bagPitch");w.StepBagLook(new Vector2(Mathf.DeltaAngle(yaw,angles.y)/(2*w.FpsLookMultiplierV414),(pitch-Mathf.DeltaAngle(0,angles.x))/(1.7f*w.FpsLookMultiplierV414)));yield return Wait(.7f);w.SetFocus("bunker");
            Check("sequence.real-shelter-gate",w.TryEnterActiveShelterV5()&&w.Model.ReachedShelterV5,"Explicit actor/view fixture at real basement, actual world floor/ray/path gate; traversal not accepted");
            Check("sequence.final-completion-real-state",Read<bool>(Steps[5],"Complete")&&w.Model.ReachedShelterV5);yield return QuakeAndSecondaryAlert();Check("finish.runtime-errors",report.errors.Count==0,string.Join("\n",report.errors));
        }
        static bool PngSize(string path,out int width,out int height)
        {width=height=0;try{if(!File.Exists(path)||new FileInfo(path).Length<2048)return false;var b=File.ReadAllBytes(path);if(b.Length<24||b[0]!=137||b[1]!=80||b[2]!=78||b[3]!=71)return false;width=(b[16]<<24)|(b[17]<<16)|(b[18]<<8)|b[19];height=(b[20]<<24)|(b[21]<<16)|(b[22]<<8)|b[23];return width>0&&height>0;}catch(IOException){return false;}}
        static string Hash(string path){if(!File.Exists(path))return "absent";using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        void Finish()
        {if(finished)return;finished=true;RestoreQaListener();report.realScoreAfter=Hash(realScore);Check("scores.real-unchanged",report.realScoreBefore==report.realScoreAfter,report.realScoreBefore+" -> "+report.realScoreAfter,"NATIVE_FILE_HASH");Check("scores.remain-isolated",Isolated(),PanicModel.ScorePathOverride,"NATIVE_FILE_PATH");report.seconds=Time.realtimeSinceStartup-began;report.passed=report.complete&&report.errors.Count==0&&report.checks.Count>=45&&report.checks.All(c=>c.status=="PASS");Application.logMessageReceived-=Log;if(!string.IsNullOrEmpty(w.EvidencePath))File.WriteAllText(Path.Combine(w.EvidencePath,"v55-smoke.json"),JsonUtility.ToJson(report,true));Debug.Log("V55_SMOKE "+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count+" seconds="+report.seconds);Application.Quit(report.passed?0:2);}
    }
}
