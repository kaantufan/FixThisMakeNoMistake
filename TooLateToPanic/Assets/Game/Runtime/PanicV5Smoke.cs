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
    public sealed class PanicV5Smoke : MonoBehaviour
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
            if(!Environment.GetCommandLineArgs().Contains("--v5-smoke"))return;
            var world=FindFirstObjectByType<PanicHomeWorld>();if(world)world.gameObject.AddComponent<PanicV5Smoke>();
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
        {report.checks.Add(new Row{id=id,status=pass?"PASS":"FAIL",detail=detail,layer=layer,frame=Time.frameCount});Debug.Log("V5_CHECK "+(pass?"PASS ":"FAIL ")+id+" "+detail);}
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
            Check("layout.bathroom-window-removed",!w.Model.Items.Exists(i=>i.Id=="window_north_upper_east"));
            var hits=Physics.RaycastAll(new Vector3(4,4.65f,5),Vector3.forward,2,~0,QueryTriggerInteraction.Ignore);
            Check("layout.bathroom-physical-wall",hits.Any(h=>h.collider.name.Contains("north")&&h.point.z>5.7f),string.Join(";",hits.Select(h=>h.collider.name)),"NATIVE_PHYSICS_RAY");
            Check("quake.seven-neighbour-roots",w.NeighbourQuakeV5&&w.NeighbourQuakeV5.HouseCount==7);
            Check("loot.fourteen-sources",w.Expedition.Sources.Count==14);
            foreach(string id in new[]{"decor_pantry_back","decor_pantry_side","decor_child_cabinet","decor_garage_shelf","decor_basement_shelf","decor_basement_spare_crates"})
            {
                var objects=FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name==id).ToArray();
                Check("loot.unique-real-storage."+id,objects.Length==1&&objects[0].GetComponent<PanicV5FurnitureRig>()&&objects[0].GetComponent<HomePick>(),"actual roots="+objects.Length);
            }
            PanicV5SystemsModelChecks.Run((id,pass,detail)=>Check(id,pass,detail,"NATIVE_MODEL_FIXTURE"));
            w.StartGame(false);w.FullFlowScriptedInput=true;yield return null;if(hud.FreeplayOnboardingOpenV414)hud.ConfirmFreeplayOnboardingV414();
            float introTime=w.Model.TimeRemaining;Vector3 introOrigin=w.PlayerPosition;
            yield return Wait(2f);yield return Capture("00a-opening-context");
            yield return Wait(3f);yield return Capture("00b-opening-precautions");
            yield return Wait(4f);yield return Capture("00c-opening-bag");
            yield return Wait(4f);yield return Capture("00d-opening-shelter");
            Check("intro.position-and-clock-held",Vector3.Distance(introOrigin,w.PlayerPosition)<.02f&&Mathf.Abs(w.Model.TimeRemaining-introTime)<.01f);
            yield return Wait(3.4f);Check("intro.natural-control-return",!w.CinematicActive&&w.PlayerControllerEnabled&&w.Model.TimeRemaining>163);
            yield return StartRun();
            var hero=Field<Transform>(w,"hero");var animator=hero.GetComponentInChildren<Animator>();
            Check("character.retained-humanoid",animator&&animator.isHuman&&animator.avatar&&animator.avatar.isValid&&!animator.applyRootMotion);
            Check("character.real-clips",animator.runtimeAnimatorController.animationClips.Any(c=>c.name.Contains("Walk_Loop"))&&animator.runtimeAnimatorController.animationClips.Any(c=>c.name.Contains("Jump_Loop")));
            yield return Place(new Vector3(-3.1f,0,-3.9f));yield return Capture("01-compact-hud");
            bool controls=hud.CompactControlsExpandedV5;hud.ToggleCompactControlsV5();Check("hud.controls-toggle",hud.CompactControlsExpandedV5!=controls);yield return Capture("02-controls");hud.ToggleCompactControlsV5();
            yield return null;Check("notebook.opens",w.Notebook.RequestOpen());yield return Wait(.6f);yield return Capture("03-notebook");w.Notebook.RequestClose();yield return Wait(.5f);
            Check("power.bound-real-lamps",w.MainsLightCountV5>=6&&w.MainsEmitterCountV5>0,"lamps="+w.MainsLightCountV5+" emitters="+w.MainsEmitterCountV5);
            w.Model.UtilitySafety.Complete("poweroff");yield return Wait(.15f);
            var mains=Field<List<Light>>(w,"mainsLightsV5");
            Check("power.all-domestic-lights-off",mains.All(l=>!l||!l.enabled));
            var emitters=Field<List<Renderer>>(w,"mainsEmittersV5");var block=new MaterialPropertyBlock();
            Check("power.all-domestic-emission-off",emitters.All(r=>{r.GetPropertyBlock(block);return block.GetColor("_EmissionColor").maxColorComponent<.001f;}));
            yield return Capture("04-house-power-off");
            yield return StartRun();
            yield return Place(new Vector3(-5.78f,3.2f,2.0f));w.Model.TimeRemaining=44;report.timeFixtures++;yield return Wait(1.4f);
            Check("bag.real-firstperson-stage",w.BagFirstPersonActive);
            var blanket=w.Expedition.FindWorldSupply("supply_blanket_bed");
            var blanketRoot=Field<Dictionary<string,GameObject>>(w,"worldSupplyObjects")[blanket.Id];
            Aim(blanketRoot.GetComponent<BoxCollider>().bounds.center);yield return Wait(.7f);
            string ray=(string)Call(w,"BagRayTarget");
            string eyeHits=string.Join(";",Physics.RaycastAll(w.View.ViewportPointToRay(new Vector3(.5f,.5f)),2.4f,~((1<<28)|(1<<30)),QueryTriggerInteraction.Collide).OrderBy(h=>h.distance).Select(h=>h.collider.name+"@"+h.distance.ToString("F2")));
            Check("blanket.actual-eye-ray",ray==blanket.Id,"ray="+ray+" supply="+blanket.Position+" actor="+w.PlayerPosition+" hits="+eyeHits,"NATIVE_PHYSICS_RAY");
            int before=w.Expedition.Count("blanket");bool took=w.TakeWorldSupply();
            Check("blanket.actual-world-transaction",took&&w.Expedition.Count("blanket")>before&&blanket.Taken,"No collider disabling or through-wall targeting.");
            yield return Capture("05-blanket-pickup");
            yield return Wait(.4f);Check("blanket.collected-world-visual-gone",!blanketRoot||!blanketRoot.activeInHierarchy,"Finite world object disappears after the bounded pickup flight.");
            yield return StartRun();w.Model.Disaster=DisasterType.Flood;
            yield return Place(new Vector3(-4.5f,-3.2f,1.4f));w.Model.TimeRemaining=44;report.timeFixtures++;yield return Wait(1.4f);
            Aim(PanicHomeWorld.BunkerPosition+Vector3.up*.35f);yield return Wait(.1f);w.SetFocus("bunker");
            Check("flood.reject-basement",!w.TryEnterActiveShelterV5()&&!w.Model.ReachedShelterV5&&w.Model.Phase==PanicPhase.Preparing);
            yield return Place(new Vector3(4.2f,3.2f,-.65f));Aim(PanicHomeWorld.PanicRoomPositionV5+Vector3.up*1.35f);yield return Wait(.2f);
            Check("flood.real-sign-ray",(string)Call(w,"BagRayTarget")=="@panicroom","physical E target in upper room","NATIVE_PHYSICS_RAY");
            w.SetFocus("@panicroom");Check("flood.admit-upper-actual-position",w.TryEnterActiveShelterV5()&&w.Model.ReachedShelterV5&&w.Model.EnteredShelterV5==PanicShelterKindV5.PanicRoom);
            Vector3 entered=w.PlayerPosition;yield return Wait(1.2f);
            Check("flood.cutscene-stays-upstairs",w.PlayerPosition.y>3&&Vector3.Distance(entered,w.PlayerPosition)<.9f&&w.BunkerSequenceStageV414=="upper_room_wait",w.PlayerPosition+" stage="+w.BunkerSequenceStageV414);
            yield return Capture("06-flood-upper-room");w.SkipResidenceCinematic();yield return Wait(.2f);
            yield return StartRun();yield return Place(new Vector3(1.9f,0,-4f));var missed=w.PlayerPosition;
            w.Model.TimeRemaining=.01f;report.timeFixtures++;yield return Wait(1.6f);
            Check("deadline.no-arrival",!w.Model.ReachedShelterV5&&w.BunkerSequenceStageV414!="open"&&Vector3.Distance(missed,w.PlayerPosition)<.2f,"kind="+w.CinematicKind+" actual="+w.PlayerPosition);
            yield return Capture("07-unreached-distinct-cutscene");
            w.StartDisasterSimulation(DisasterType.Earthquake);w.TriggerSimulation();yield return Wait(5.3f);
            Check("quake.neighbours-really-move",w.NeighbourQuakeV5.MovingHouses==7&&w.NeighbourQuakeV5.MaximumOffset>.02f,"moving="+w.NeighbourQuakeV5.MovingHouses+" offset="+w.NeighbourQuakeV5.MaximumOffset);
            yield return Capture("08-neighbour-quake");w.SkipResidenceCinematic();yield return Wait(.2f);
            Check("quake.neighbours-restore",w.NeighbourQuakeV5.MovingHouses==0);
            w.StartDisasterSimulation(DisasterType.Flood);w.TriggerSimulation();yield return Wait(8.5f);
            var street=w.GetComponent<PanicStreetLifeV414>();
            Check("response.flood-zodiac",street.ResponseBoatActive&&street.ZodiacRoot&&street.ZodiacRoot.GetComponent<PanicV5VehicleRig>());
            // The response owner drives actual route roots; observe motion without writing their transforms.
            float responseEnd=Time.realtimeSinceStartup+21f;while(street.ResponderCount<5&&Time.realtimeSinceStartup<responseEnd)yield return null;
            Check("response.four-real-vehicles",street.ResponseVehicleCount>=5,"includes4 road vehicles+boat="+street.ResponseVehicleCount);
            Check("response.distinct-real-crew",street.ResponderCount==5,"responders="+street.ResponderCount);
            foreach(var rig in FindObjectsByType<PanicV5VehicleRig>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                Check("asset.rig."+rig.ServiceId,rig.IsBoat?rig.Propeller&&rig.GetComponentsInChildren<Collider>().Length>0:rig.Wheels.Length>=4&&rig.FlashLights.Length>0&&rig.Doors.Length>0&&rig.GetComponentsInChildren<Collider>().Length>0);
            }
            Photo("09-response-vehicles",new Vector3(3,14,-40),new Vector3(1,1,-21),56);
            foreach(var rig in FindObjectsByType<PanicV5VehicleRig>(FindObjectsSortMode.None))
                if(!rig.IsBoat)Photo("asset-vehicle-"+rig.ServiceId,rig.transform.position+new Vector3(5,3.8f,5),rig.transform.position+Vector3.up*1.1f,50);
            var responseRoot=Field<GameObject>(street,"responseRoot");int crewIndex=0;
            foreach(var actor in responseRoot.GetComponentsInChildren<Animator>())
                Photo("asset-crew-"+crewIndex++,actor.transform.position+new Vector3(2.2f,1.6f,-3.1f),actor.transform.position+Vector3.up*1.05f,46);
            Photo("10-zodiac",street.ZodiacRoot.transform.position+new Vector3(5,3,-5),street.ZodiacRoot.transform.position+Vector3.up*.4f,50);
            Screen.SetResolution(960,540,false);yield return Wait(.4f);yield return Capture("11-compact-960");
            Screen.SetResolution(1600,900,false);yield return Wait(.3f);
            yield return StartRun();yield return Place(new Vector3(4.9f,3.2f,-1.5f));yield return Capture("12-panic-room-hud");
            Photo("13-main-character",w.PlayerPosition+new Vector3(1.8f,1.5f,2.5f),w.PlayerPosition+Vector3.up*1.05f,46);
            Check("finish.runtime-errors",report.errors.Count==0,string.Join("\n",report.errors));
        }
        static string Hash(string path){if(!File.Exists(path))return "absent";using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        void Finish()
        {
            if(finished)return;finished=true;report.realScoreAfter=Hash(realScore);
            Check("scores.real-file-unchanged",report.realScoreBefore==report.realScoreAfter);report.seconds=Time.realtimeSinceStartup-began;
            report.passed=report.complete&&report.errors.Count==0&&report.checks.Count>45&&report.checks.All(c=>c.status=="PASS");
            File.WriteAllText(Path.Combine(w.EvidencePath,"v5-smoke.json"),JsonUtility.ToJson(report,true));Application.logMessageReceived-=Log;
            Debug.Log("V5_SMOKE "+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count+" seconds="+report.seconds);Application.Quit(report.passed?0:2);
        }
    }
}
