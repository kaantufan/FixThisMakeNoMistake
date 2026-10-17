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
    public sealed class PanicV51Smoke : MonoBehaviour
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
            if(!Environment.GetCommandLineArgs().Contains("--v51-smoke"))return;
            var world=FindFirstObjectByType<PanicHomeWorld>();if(world)world.gameObject.AddComponent<PanicV51Smoke>();
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
        {report.checks.Add(new Row{id=id,status=pass?"PASS":"FAIL",detail=detail,layer=layer,frame=Time.frameCount});Debug.Log("V51_CHECK "+(pass?"PASS ":"FAIL ")+id+" "+detail);}
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
            yield return Wait(.5f);
            Check("launch.isolated",!string.IsNullOrEmpty(w.EvidencePath)&&PanicModel.ScorePathOverride.StartsWith(w.EvidencePath));
            PanicBagV51Checks.Run((id,pass,detail)=>Check(id,pass,detail,"NATIVE_MODEL_FIXTURE"));
            PanicDeveloperWaveV51Checks.Run((id,pass,detail)=>Check(id,pass,detail,"NATIVE_MODEL_FIXTURE"));
            Check("timing.normal-120-plus45",PanicModel.NormalPrecautionSeconds==120&&PanicModel.NormalBagSeconds==45&&PanicModel.NormalPreparationSeconds==165);
            yield return StartRun();
            Check("bag.no-floor-object",w.CarriedBagInvisibleV51&&!FindObjectsByType<HomePick>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(p=>p.Id=="@bagstation"||p.Id=="bagstation"));
            Check("bag.no-guide-light",!FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(t=>t.name=="Bag station warm light"));
            w.ToggleInventory();yield return null;Check("bag.carried-tab-access",w.InventoryOpen);w.ToggleInventory();yield return null;
            yield return Place(new Vector3(1.65f,0,-7.5f));
            Check("garden.outdoor-markers",w.VisibleGardenMarkersV51>0);
            yield return Capture("01-outdoor-keyboard");
            bool expanded=hud.CompactControlsExpandedV5;hud.ToggleCompactControlsV5();yield return null;
            Check("keyboard.toggle",hud.CompactControlsExpandedV5!=expanded);yield return Capture("02-keyboard-expanded");
            yield return Place(new Vector3(1.5f,0,4.4f));
            Check("garden.house-hidden",w.VisibleGardenMarkersV51==0);
            w.SetFocus(PanicItemActionRules.FireStationId);yield return null;
            var station=Field<GameObject>(w,"fireStation");var box=station.GetComponent<BoxCollider>();
            Check("fire.real-wall-mount-collision",box.bounds.min.y>.65f&&station.transform.position.z>5.5f,box.bounds.ToString(),"NATIVE_PHYSICS_COLLIDER");
            var mountHits=Physics.RaycastAll(box.bounds.center,Vector3.forward,1f,~0,QueryTriggerInteraction.Ignore);
            Check("fire.real-wall-backing",mountHits.Any(h=>h.collider!=box&&!h.collider.transform.IsChildOf(station.transform)&&h.collider.name.Contains("north")),string.Join(";",mountHits.Select(h=>h.collider.name)),"NATIVE_PHYSICS_RAY");
            Check("interaction.above-real-object",w.PrimaryInteractionVisibleV51&&w.CanInteract,"focus="+w.FocusLabel+" action="+w.ShortAction);
            yield return Capture("03-wall-mount-primary-action");
            yield return Place(new Vector3(-3,-3.2f,0));
            Check("garden.basement-hidden",w.VisibleGardenMarkersV51==0);yield return Capture("04-basement-clear");
            yield return StartRun();
            w.Model.SetDeveloperOption(PanicDeveloperOption.FreeMaterials,true);w.Model.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget,true);w.Model.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime,true);
            string[] actions={"anchorwall","gasoff","wateroff","poweroff","hose","drain"};
            string[] targets={"bookshelf",PanicUtilitySafetyV414.GasId,PanicUtilitySafetyV414.WaterId,PanicUtilitySafetyV414.PowerId,"hose_station","drain_west"};
            Vector3[] seats={new Vector3(-6.2f,0,3.2f),new Vector3(6.3f,0,.25f),new Vector3(6.3f,0,-1.1f),new Vector3(9.6f,0,4.7f),new Vector3(10,0,1.7f),new Vector3(-5,0,-7.3f)};
            for(int a=0;a<actions.Length;a++)
            {
                yield return Place(seats[a]);w.BeginWork(actions[a],targets[a]);yield return Wait(.3f);
                Check("work.starts."+actions[a],w.WorkActive&&w.Expedition.Work.Action==actions[a],w.Model.Message);
                if(!w.WorkActive)continue;
                if(!w.Expedition.Work.UsesHammer)
                {
                    Rect hit=w.ManualWorkTargetBoundsV414(w.Expedition.Work.NextClickTarget);
                    Check("work.actual-circle-target."+actions[a],hit.width>0&&hit.center.x>=90&&hit.center.x<=1096&&hit.center.y>=225&&hit.center.y<=779,hit.ToString());
                }
                int budget=w.Model.Budget;string materials=string.Join(";",w.Expedition.Materials.Select(k=>k.Key+"="+k.Value));
                yield return Capture("05-work-"+actions[a]);
                if(actions[a]=="anchorwall")
                { Screen.SetResolution(960,540,false);yield return Wait(.4f);yield return Capture("05-work-screw-960");Screen.SetResolution(1600,900,false);yield return Wait(.3f); }
                w.CancelWork();yield return null;
                Check("work.cancel-free."+actions[a],!w.WorkActive&&w.Model.Budget==budget&&materials==string.Join(";",w.Expedition.Materials.Select(k=>k.Key+"="+k.Value)));
            }
            w.Model.SetDeveloperOption(PanicDeveloperOption.FreeMaterials,false);w.Model.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget,false);w.Model.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime,false);
            yield return StartRun();yield return Place(new Vector3(-5.78f,3.2f,2.0f));
            w.Model.TimeRemaining=44;report.timeFixtures++;w.TickPreparationStages();
            Vector3 actor=w.PlayerPosition;float clock=w.Model.TimeRemaining;
            yield return Wait(.38f);Check("fps.handoff-full-black-interval",w.BagFullBlackIntervalV51);
            yield return new WaitForEndOfFrame();
            var black=ScreenCapture.CaptureScreenshotAsTexture();float max=0;
            for(int x=1;x<8;x++)for(int y=1;y<5;y++){Color pixel=black.GetPixel(black.width*x/8,black.height*y/5);max=Mathf.Max(max,pixel.maxColorComponent);}
            Destroy(black);Check("fps.actual-black-framebuffer",max<.025f,"maximum sampled RGB="+max,"NATIVE_FRAMEBUFFER_PIXELS");
            yield return Capture("06-real-blackout");
            float age=w.BagHandoffElapsedV51;hud.SetPaused(true);yield return Wait(.25f);
            Check("fps.pause-freezes-handoff",Mathf.Abs(age-w.BagHandoffElapsedV51)<.001f&&Mathf.Abs(clock-w.Model.TimeRemaining)<.02f);
            hud.SetPaused(false);yield return Wait(1.65f);
            Check("fps.handoff-controls-return",w.BagFirstPersonActive&&!w.BagCameraTransitionActive&&w.PlayerControllerEnabled&&!w.InputLocked);
            Check("fps.handoff-keeps-real-actor",Vector3.Distance(actor,w.PlayerPosition)<.02f,actor+" -> "+w.PlayerPosition);
            Check("fps.clock-held-during-handoff",w.Model.TimeRemaining>clock-.35f,"clock="+clock+" after="+w.Model.TimeRemaining);
            Check("fps.black-hold-rendered",w.LastBagHandoffFullBlackSecondsV51>=.57f&&w.BagHandoffFullBlackDrawFramesV51>0,"seconds="+w.LastBagHandoffFullBlackSecondsV51+" frames="+w.BagHandoffFullBlackDrawFramesV51);
            Check("fps.panic-active",w.FpsStressIntensityV414>.31f&&w.FpsVeilOpacityV414>.24f&&w.FpsHeartVolumeV51>0&&w.FpsBreathVolumeV51>0,"stress="+w.FpsStressIntensityV414+" opacity="+w.FpsVeilOpacityV414);
            var blanket=w.Expedition.FindWorldSupply("supply_blanket_bed");var blanketRoot=Field<Dictionary<string,GameObject>>(w,"worldSupplyObjects")[blanket.Id];
            Aim(blanketRoot.GetComponent<BoxCollider>().bounds.center);yield return Wait(.7f);
            Check("bag.blanket-real-ray",(string)Call(w,"BagRayTarget")==blanket.Id,"NATIVE_PHYSICS_RAY");
            int blanketBefore=w.Expedition.Bag.Where(s=>s.Id=="blanket").Sum(s=>s.Count);bool took=w.TakeWorldSupply();
            Check("bag.blanket-auto-packed-world",took&&blanket.Taken&&w.Expedition.Bag.Where(s=>s.Id=="blanket").Sum(s=>s.Count)>blanketBefore);
            yield return Wait(.4f);Check("bag.world-pickup-disappears",!blanketRoot||!blanketRoot.activeInHierarchy);yield return Capture("07-panic-bag-auto");
            yield return StartRun();
            hud.SetDeveloperMenuOpen(true);yield return null;Check("developer.f2-menu",hud.DeveloperMenuOpen);yield return Capture("08-developer-F3-button");
            int wave=w.Model.Wave;hud.RequestDeveloperWaveSkipV51();yield return Wait(.2f);
            Check("developer.menu-button-enters-cinema",!hud.DeveloperMenuOpen&&w.Model.Phase==PanicPhase.Disaster&&w.CinematicActive&&!w.Model.IsPaused&&w.Model.DeveloperWaveSkipProtectionV51&&w.Model.CheatedRun);
            float phaseClock=w.Model.PhaseTime;hud.RequestDeveloperWaveSkipV51();yield return Wait(.15f);
            Check("developer.repeat-no-restart",w.Model.Wave==wave&&w.Model.PhaseTime<phaseClock&&!w.Model.ReachedShelterV5);
            yield return Capture("09-developer-disaster");
            yield return Wait(12.2f);
            Check("developer.survives-real-cinematics",w.Model.Phase==PanicPhase.Inspection&&w.Model.LastReward==0&&w.Model.LastWaveScore==0&&!w.LossAnnounced&&w.Model.TotalDamage>0,"phase="+w.Model.Phase+" damage="+w.Model.TotalDamage);
            Check("bag.no-thrown-cinema-bag",w.CarriedBagInvisibleV51&&!w.BunkerBagObjectV414&&!w.BunkerBagReleasedV414);
            hud.RequestDeveloperWaveSkipV51();yield return Wait(.2f);Check("developer.inspection-to-next-cinema",w.Model.Wave==wave+1&&w.Model.Phase==PanicPhase.Disaster&&w.CinematicActive);
            yield return StartRun();Check("developer.new-run-clears-wave-protection",!w.Model.DeveloperWaveSkipProtectionV51&&!w.Model.CheatedRun&&w.Model.Phase==PanicPhase.Preparing);
            yield return Place(new Vector3(1.5f,0,-4f));w.Model.TimeRemaining=44;report.timeFixtures++;w.TickPreparationStages();yield return Wait(.4f);
            Check("fps.second-handoff-started",w.BagHandoffActiveV51&&w.BagFullBlackIntervalV51);
            hud.RequestDeveloperWaveSkipV51();yield return Wait(.2f);
            Check("fps.dev-skip-clears-blackout",!w.BagHandoffActiveV51&&w.BagBlackoutOpacityV51==0&&w.CinematicActive&&w.Model.Phase==PanicPhase.Disaster&&!w.Model.IsPaused&&!w.InputLocked);
            yield return StartRun();
            w.Model.TimeRemaining=.01f;report.timeFixtures++;yield return Wait(.2f);Check("normal.timeout-still-loses",w.Model.Phase==PanicPhase.GameOver&&!w.Model.ReachedShelterV5);
            yield return StartRun();w.Model.Disaster=DisasterType.Earthquake;yield return Place(new Vector3(-4.5f,-3.2f,1.4f));w.Model.TimeRemaining=44;report.timeFixtures++;w.TickPreparationStages();yield return Wait(2.15f);
            Aim(PanicHomeWorld.BunkerPosition+Vector3.up*.35f);yield return Wait(.7f);w.SetFocus("bunker");
            Check("shelter.real-basement-entry",w.TryEnterActiveShelterV5()&&w.Model.ReachedShelterV5);
            yield return Wait(2f);Check("bag.hidden-during-real-shelter-sequence",w.CarriedBagInvisibleV51&&!w.BunkerBagObjectV414&&!w.BunkerBagReleasedV414&&w.BunkerSequenceStageV414!="bagthrow");yield return Capture("11-no-floor-or-thrown-bag");
            yield return StartRun();yield return Place(new Vector3(1.65f,0,-7.5f));Screen.SetResolution(960,540,false);yield return Wait(.4f);
            if(!hud.CompactControlsExpandedV5)hud.ToggleCompactControlsV5();yield return Capture("10-keyboard-960");
            Check("finish.runtime-errors",report.errors.Count==0,string.Join("\n",report.errors));
        }
        static string Hash(string path){if(!File.Exists(path))return "absent";using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        void Finish()
        {
            if(finished)return;finished=true;report.realScoreAfter=Hash(realScore);
            Check("scores.real-file-unchanged",report.realScoreBefore==report.realScoreAfter);report.seconds=Time.realtimeSinceStartup-began;
            report.passed=report.complete&&report.errors.Count==0&&report.checks.Count>45&&report.checks.All(c=>c.status=="PASS");
            File.WriteAllText(Path.Combine(w.EvidencePath,"v51-smoke.json"),JsonUtility.ToJson(report,true));Application.logMessageReceived-=Log;
            Debug.Log("V51_SMOKE "+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count+" seconds="+report.seconds);Application.Quit(report.passed?0:2);
        }
    }
}
