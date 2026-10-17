using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in native API/physics/render evidence. Explicit setup fixtures; never OS/human acceptance.</summary>
    public sealed class PanicV52Smoke : MonoBehaviour
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        [Serializable] public sealed class Row {public string id,status,detail,layer;public int frame;}
        [Serializable] public sealed class Receipt
        {
            public string utc,dllSha256,realScoreBefore,realScoreAfter,error;
            public string evidence="Native API, real CharacterController/colliders, framebuffer and Camera.Render. Explicit warp/time/seed/model fixtures; no OS input, human play, listening or full campaign acceptance.";
            public bool complete,passed,osInput=false,humanAcceptance=false;
            public float seconds;public int setupWarps,timeFixtures;
            public List<Row> checks=new List<Row>();public List<string> captures=new List<string>(),errors=new List<string>();
        }
        readonly Receipt report=new Receipt();PanicHomeWorld w;PanicHomeHud hud;float began;string realScore;bool finished;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Attach()
        {
            if(!Environment.GetCommandLineArgs().Contains("--v52-smoke"))return;
            var world=FindFirstObjectByType<PanicHomeWorld>();if(world)world.gameObject.AddComponent<PanicV52Smoke>();
        }
        void Start()
        {
            w=GetComponent<PanicHomeWorld>();hud=GetComponent<PanicHomeHud>();began=Time.realtimeSinceStartup;
            realScore=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");
            report.realScoreBefore=Hash(realScore);report.dllSha256=Hash(typeof(PanicModel).Assembly.Location);report.utc=DateTime.UtcNow.ToString("o");
            Application.logMessageReceived+=Log;StartCoroutine(Guard(Run()));
        }
        void Update(){if(!finished&&Time.realtimeSinceStartup-began>290){report.error="Hard cap reached";Finish();}}
        void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(s+"\n"+stack);}
        void Check(string id,bool pass,string detail="",string layer="NATIVE_API_FRAME")
        {report.checks.Add(new Row{id=id,status=pass?"PASS":"FAIL",detail=detail,layer=layer,frame=Time.frameCount});Debug.Log("V52_CHECK "+(pass?"PASS ":"FAIL ")+id+" "+detail);}
        static T Field<T>(object obj,string name){return (T)obj.GetType().GetField(name,Flags).GetValue(obj);}
        static object Call(object obj,string name,params object[] args)
        {return obj.GetType().GetMethods(Flags).First(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(obj,args);}
        IEnumerator Guard(IEnumerator body)
        {
            var stack=new Stack<IEnumerator>();stack.Push(body);
            while(stack.Count>0&&!finished)
            {
                bool next;object value=null;
                try{next=stack.Peek().MoveNext();if(next)value=stack.Peek().Current;}
                catch(Exception e){report.error=e.ToString();break;}
                if(!next){stack.Pop();continue;}if(value is IEnumerator nested)stack.Push(nested);else yield return value;
            }
            if(!finished){report.complete=string.IsNullOrEmpty(report.error);Finish();}
        }
        IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
        IEnumerator StartRun()
        {
            w.ReturnToMenu();w.StartGame(false);w.FullFlowScriptedInput=true;
            yield return null;
            if(hud.FreeplayOnboardingOpenV414)hud.ConfirmFreeplayOnboardingV414();
            yield return null;w.SkipIntroStoryV414();yield return Wait(.15f);
        }
        IEnumerator Place(Vector3 p)
        {
            w.Warp(p+Vector3.up*.04f);report.setupWarps++;
            for(int i=0;i<10;i++){w.SmokeMove(Vector2.zero,.025f);yield return null;}
            Check("physics.grounded."+report.setupWarps,w.IsGrounded,p.ToString(),"NATIVE_PHYSICS_SETUP");
        }
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();string path=Path.Combine(w.EvidencePath,name+".png");ScreenCapture.CaptureScreenshot(path);
            report.captures.Add(path);yield return Wait(.18f);Check("capture."+name,File.Exists(path)&&new FileInfo(path).Length>2048,path,"NATIVE_FRAMEBUFFER_FILE");
        }
        void Photo(string name,Vector3 camera,Vector3 target,float fov=48)
        {
            var go=new GameObject("V5 QA render camera");var view=go.AddComponent<Camera>();view.enabled=false;view.clearFlags=CameraClearFlags.Skybox;view.cullingMask=w.View.cullingMask;
            view.nearClipPlane=.08f;view.farClipPlane=180;view.fieldOfView=fov;view.transform.position=camera;view.transform.LookAt(target);
            var rt=new RenderTexture(1400,900,24);view.targetTexture=rt;view.Render();var prior=RenderTexture.active;RenderTexture.active=rt;
            var tex=new Texture2D(1400,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1400,900),0,0);tex.Apply();
            string path=Path.Combine(w.EvidencePath,name+".png");File.WriteAllBytes(path,tex.EncodeToPNG());report.captures.Add(path);
            RenderTexture.active=prior;view.targetTexture=null;Destroy(tex);rt.Release();Destroy(rt);Destroy(go);
            Check("render."+name,File.Exists(path)&&new FileInfo(path).Length>2048,path,"NATIVE_CAMERA_RENDER");
        }
        void Aim(Vector3 target)
        {
            Vector3 d=target-(w.PlayerPosition+Vector3.up*1.62f);var e=Quaternion.LookRotation(d).eulerAngles;
            float pitch=Mathf.DeltaAngle(0,e.x),yaw=e.y;
            float currentYaw=Field<float>(w,"bagYaw"),currentPitch=Field<float>(w,"bagPitch");
            w.StepBagLook(new Vector2(Mathf.DeltaAngle(currentYaw,yaw)/(2*w.FpsLookMultiplierV414),(currentPitch-pitch)/(1.7f*w.FpsLookMultiplierV414)));
        }
        IEnumerator Run()
        {
            yield return Wait(.45f);
            Check("launch.isolated",!string.IsNullOrEmpty(w.EvidencePath)&&PanicModel.ScorePathOverride.StartsWith(w.EvidencePath));
            PanicWaveV52ModelChecks.Run((id,pass,detail)=>Check(id,pass,detail,"NATIVE_MODEL_FIXTURE"));
            PanicDeveloperWaveV51Checks.Run((id,pass,detail)=>Check(id,pass,detail,"NATIVE_MODEL_FIXTURE"));
            float quakeScale1=0, quakeNeighbourOffset1=0, floodLevel1=0, fireEmission1=0, ambience1=0;
            foreach(DisasterType hazard in new[]{DisasterType.Earthquake,DisasterType.Flood,DisasterType.Fire})
            foreach(int wave in new[]{1,20})
            {
                yield return StartRun();
                w.Model.Wave=wave;w.Model.Disaster=hazard;report.timeFixtures++;
                hud.RequestDeveloperWaveSkipV51();yield return Wait(5.45f);
                var fx=w.GetComponent<PanicHomeEffects>();float expected=w.Model.DisasterPresentationScaleV52;
                Check("fx.scale."+hazard+".wave"+wave,Mathf.Abs(fx.DisasterPresentationScaleV52-expected)<.0001f,"scale="+fx.DisasterPresentationScaleV52+" damage="+w.Model.DisasterSeverityV52);
                Check("fx.bounded."+hazard+".wave"+wave,expected>=1&&expected<=1.65f&&fx.DisasterAmbienceGainV52<=1.2926f&&fx.DisasterAmbienceVolumeV52<=1,"scale="+expected+" gain="+fx.DisasterAmbienceGainV52);
                Check("fx.real-weather-emits."+hazard+".wave"+wave,fx.DisasterEmissionRateV52>0,"rate="+fx.DisasterEmissionRateV52);
                if(hazard==DisasterType.Earthquake)
                {
                    Check("quake.real-houses."+wave,w.NeighbourQuakeV5.MovingHouses==7&&w.QuakeReadabilityV414&&w.QuakeReadabilityV414.Active&&w.NeighbourQuakeV5.CurrentMaximumOffsetV52>0,"moving="+w.NeighbourQuakeV5.MovingHouses);
                    Check("quake.shared-house-scale."+wave,Mathf.Abs(w.NeighbourQuakeV5.PresentationScaleV52-expected)<.0001f&&Mathf.Abs(w.QuakeReadabilityV414.CurrentPresentationScaleV52-expected)<.0001f);
                    if(wave==1){quakeScale1=expected;quakeNeighbourOffset1=w.NeighbourQuakeV5.CurrentMaximumOffsetV52;}
                    else Check("quake.later-stronger",expected>quakeScale1&&w.NeighbourQuakeV5.CurrentMaximumOffsetV52>quakeNeighbourOffset1*1.1f,"early="+quakeNeighbourOffset1+" late="+w.NeighbourQuakeV5.CurrentMaximumOffsetV52);
                }
                if(hazard==DisasterType.Flood)
                {
                    if(wave==1){floodLevel1=fx.DisasterWaterLevelV52;ambience1=fx.DisasterAmbienceGainV52;}
                    else
                    {
                        Check("flood.later-water-higher",fx.DisasterWaterLevelV52>floodLevel1+.1f&&fx.DisasterWaterPeakHeightV52>floodLevel1,"early="+floodLevel1+" late="+fx.DisasterWaterLevelV52+" actualpeak="+fx.DisasterWaterPeakHeightV52);
                        Check("audio.later-hazard-gain",fx.DisasterAmbienceGainV52>ambience1,"early="+ambience1+" late="+fx.DisasterAmbienceGainV52,"NATIVE_AUDIO_SOURCE_STATE_NOT_LISTENING");
                    }
                }
                if(hazard==DisasterType.Fire)
                {
                    if(wave==1)fireEmission1=fx.DisasterEmissionRateV52;
                    else Check("fire.later-more-emission",fx.DisasterEmissionRateV52>fireEmission1*1.1f&&fx.DisasterFlamePeakHeightV52>0,"early="+fireEmission1+" late="+fx.DisasterEmissionRateV52+" flame="+fx.DisasterFlamePeakHeightV52);
                }
                yield return Capture("wave"+wave+"-"+hazard);
                if(hazard==DisasterType.Earthquake&&wave==20)
                {
                    yield return Wait(6.8f);Check("late-wave.f3-natural-no-loss",w.Model.Phase==PanicPhase.Inspection&&w.Model.LastReward==0&&w.Model.LastWaveScore==0&&w.Model.CheatedRun&&!w.Model.ReachedShelterV5&&w.Model.TotalDamage<=100,"actual total="+w.Model.TotalDamage);
                }
                else { w.SkipResidenceCinematic();yield return Wait(.15f); }
                Check("phase.restores-after-wave."+hazard+wave,!w.CinematicActive&&w.PlayerControllerEnabled&&w.CarriedBagInvisibleV51&&w.NeighbourQuakeV5.MovingHouses==0);
            }
            w.StartDisasterSimulation(DisasterType.Storm);w.Model.Wave=20;report.timeFixtures++;w.TriggerSimulation();yield return Wait(5.3f);
            var storm=w.GetComponent<PanicHomeEffects>();
            Check("simulation.fixed-baseline-intensity",w.Model.DisasterSeverityV52==1&&w.Model.DisasterPresentationScaleV52==1&&Mathf.Abs(storm.DisasterPresentationScaleV52-1)<.0001f);
            yield return Capture("storm-simulation-baseline");w.SkipResidenceCinematic();yield return Wait(.15f);
            Check("finish.runtime-errors",report.errors.Count==0,string.Join("\n",report.errors));
        }
        static string Hash(string path){if(!File.Exists(path))return "absent";using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        void Finish()
        {
            if(finished)return;finished=true;report.realScoreAfter=Hash(realScore);
            Check("scores.qa-write-target-isolated",!string.IsNullOrEmpty(w.EvidencePath)&&PanicModel.ScorePathOverride.StartsWith(w.EvidencePath));report.seconds=Time.realtimeSinceStartup-began;
            report.passed=report.complete&&report.errors.Count==0&&report.checks.Count>45&&report.checks.All(c=>c.status=="PASS");
            File.WriteAllText(Path.Combine(w.EvidencePath,"v52-smoke.json"),JsonUtility.ToJson(report,true));Application.logMessageReceived-=Log;
            Debug.Log("V52_SMOKE "+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count+" seconds="+report.seconds);Application.Quit(report.passed?0:2);
        }
    }
}
