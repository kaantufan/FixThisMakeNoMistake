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
    /// <summary>Opt-in native production API/frame/physics driver. Root alone imports/builds/runs it.</summary>
    public sealed class PanicOverhaul414Smoke : MonoBehaviour
    {
        public const string Flag = "--overhaul414-smoke";
        const float HardCap = 295f;
        const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        static readonly string[] RequiredSections = { "launch", "onboarding", "intro", "character", "layout", "stairs", "outdoor", "ceiling", "work", "utilities", "feedback", "bag", "bunker", "hazards", "street", "models" };
        [Serializable] public sealed class Row { public string id, status, layer, detail; public int frame; }
        [Serializable] public sealed class Frame
        {
            public string label, phase, cinematic, bunkerStage, characterMesh, focus;
            public int frame, width, height, visibleRoofs, allRoofs, activeSupplies, bones, popupCount;
            public float remaining, lidAngle;
            public Vector3 actor, camera, bag;
            public bool bagVisible, bagReleased, fps, intro, controllerEnabled, onboarding, modelPaused, inputLocked;
        }
        [Serializable] public sealed class Route
        {
            public string id; public Vector3 start, target, end;
            public float seconds, highestY, lowestY; public int frames, sides, above, below;
            public bool reached, jump; public string mode;
        }
        [Serializable] public sealed class Fixture { public string kind, detail; public int frame; }
        [Serializable] public sealed class GaitSample
        {
            public string label, mesh, evaluatedClips;public int phase,frame,vertices,changedVertices,changedBones;
            public float secondsSinceWalkStart,secondsSincePrevious,actorTravel,peakLocalVertexDelta,rmsLocalVertexDelta,restRmsLocalVertexDelta;
            public Vector3 actor;
        }
        [Serializable] public sealed class VehicleDirection
        {
            public string label,root,meshFrontSource;public int frame;public Vector3 start,end,authoredLocalFront,worldFront,displacement;
            public float moved,forwardDot;
        }
        sealed class GaitWindow
        {
            public SkinnedMeshRenderer Skin;public Animator Animator;public Vector3[] Rest,Previous;public Quaternion[] Bones;
            public Vector3 ActorStart;public float Start,Next,PreviousTime;public int Captured;
        }
        [Serializable] public sealed class Receipt
        {
            public string startedUtc, finishedUtc, error, dllSha256, realScoreBefore, realScoreAfter, scorePath;
            public string evidence = "Native production APIs, real frame waits/render captures, actual CharacterController/colliders; explicit setup and pose/model fixtures are enumerated. No OS input or human acceptance.";
            public string scope = "Full overhaul";
            public bool complete, passed, osInput = false, humanAcceptance = false;
            public float seconds;
            public int setupWarps, startGameCalls, simulationStarts, explicitTimerFixtures, directControllerProbeFrames, workApiSamples, modelFixtureTicks, runPoseFixtures, bagLookApiSamples;
            public int menuActionCalls, shortFlowStarts, onboardingIntroSkips;
            public List<Row> rows = new List<Row>(); public List<Fixture> fixtures = new List<Fixture>();
            public List<Route> routes = new List<Route>(); public List<Frame> frames = new List<Frame>();
            public List<GaitSample> gaitSamples = new List<GaitSample>();
            public List<VehicleDirection> vehicleDirections = new List<VehicleDirection>();
            public List<string> sections = new List<string>(), errors = new List<string>(), captures = new List<string>();
        }
        readonly Receipt report = new Receipt();
        PanicHomeWorld w; PanicHomeHud hud; CharacterController controller; Transform hero;
        Coroutine sequence; float began; bool finished, guideOnlyR8; string realScore;
        bool lastReached; Camera photoCamera;
        static float Dt { get { return Mathf.Max(.001f, Mathf.Min(Time.unscaledDeltaTime,.06f)); } }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Attach()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (!world) throw new InvalidOperationException("V414 QA needs the production Residence World.");
            if(!world.GetComponent<PanicOverhaul414Smoke>())world.gameObject.AddComponent<PanicOverhaul414Smoke>();
        }
        void Start()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) { enabled=false; return; }
            w=GetComponent<PanicHomeWorld>(); hud=GetComponent<PanicHomeHud>();
            controller=Field<CharacterController>(w,"controller");hero=Field<Transform>(w,"hero");
            guideOnlyR8=Environment.GetCommandLineArgs().Contains("--onboarding-only");
            if(guideOnlyR8){report.scope="Onboarding only; full mechanics NOT_RUN on this DLL";report.evidence="Native first-use guide, modal input/pause and three-resolution framebuffer captures. Retained R7 gameplay regression is separate; OS round is root-owned.";}
            began=Time.realtimeSinceStartup;report.startedUtc=DateTime.UtcNow.ToString("o");
            realScore=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");
            report.realScoreBefore=Hash(realScore);report.scorePath=PanicModel.ScorePathOverride;
            report.dllSha256=Hash(typeof(PanicModel).Assembly.Location);
            Application.runInBackground=true;Application.logMessageReceived+=OnLog;
            sequence=StartCoroutine(Guarded(Run(),false));
        }
        void Update()
        {
            if(!finished&&Time.realtimeSinceStartup-began>HardCap)
            { report.error="295s watchdog; unexecuted required sections are NOT_RUN.";if(sequence!=null)StopCoroutine(sequence);Finish(); }
        }
        void OnLog(string message,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(message+"\n"+trace);}
        IEnumerator Guarded(IEnumerator body,bool section)
        {
            var stack=new Stack<IEnumerator>();stack.Push(body);
            while(stack.Count>0&&!finished)
            {
                bool more;object next=null;
                try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}
                catch(Exception ex){Check(section?"section.exception":"harness.exception",false,ex.ToString());if(!section)report.error=ex.ToString();break;}
                if(!more){stack.Pop();continue;}
                if(next is IEnumerator nested)stack.Push(nested);else yield return next;
            }
            if(!section){report.complete=string.IsNullOrEmpty(report.error);Finish();}
        }
        IEnumerator Section(string name,IEnumerator body)
        { yield return Guarded(body,true);if(name=="models")PanicModel.ScorePathOverride=report.scorePath;report.sections.Add(name); }
        void Check(string id,bool pass,string detail="",string layer="NATIVE_API_FRAME")
        { report.rows.Add(new Row{id=id,status=pass?"PASS":"FAIL",layer=layer,detail=detail,frame=Time.frameCount});Debug.Log("OVERHAUL414 "+(pass?"PASS ":"FAIL ")+id+" "+detail); }
        void FixtureNote(string kind,string detail)
        { report.fixtures.Add(new Fixture{kind=kind,detail=detail,frame=Time.frameCount}); }
        static T Field<T>(object target,string name)
        { var f=target.GetType().GetField(name,Instance);return f==null?default(T):(T)f.GetValue(target); }
        static object Read(object target,string name)
        {
            if(target==null)return null;var p=target.GetType().GetProperty(name,Instance);
            if(p!=null)return p.GetValue(target,null);var f=target.GetType().GetField(name,Instance);return f==null?null:f.GetValue(target);
        }
        static bool Bool(object target,string name){return Read(target,name) is bool b&&b;}
        static int Int(object target,string name){object value=Read(target,name);return value is int n?n:-1;}
        static string Text(object target,string name){return Read(target,name) as string??string.Empty;}
        object Call(object target,string name,params object[] args)
        {
            var method=target.GetType().GetMethods(Instance).FirstOrDefault(m=>m.Name==name&&m.GetParameters().Length==args.Length);
            if(method==null)throw new MissingMethodException(target.GetType().Name,name);return method.Invoke(target,args);
        }
        bool Require(object target,string name,string id)
        {bool present=Read(target,name)!=null;Check(id,present,"Required seam "+name+"; missing is FAIL.");return present;}
        IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
        IEnumerator Place(Vector3 p,string purpose,string focus=null)
        {
            w.CancelWork();w.Warp(p+Vector3.up*.04f);report.setupWarps++;FixtureNote("WARP_SETUP",purpose+" at "+p+"; no route claim before this position.");
            float end=Time.realtimeSinceStartup+1.5f;int frames=0;
            do{w.SmokeMove(Vector2.zero,Dt);frames++;yield return null;}while((!w.IsGrounded||frames<3)&&Time.realtimeSinceStartup<end);
            Check("grounded."+purpose,w.IsGrounded,"Actual enabled controller grounded after setup.","NATIVE_PHYSICS");
            if(focus!=null)w.SetFocus(focus);yield return null;
        }
        Vector2 Axis(Vector3 direction)
        {
            direction.y=0;direction.Normalize();Vector3 f=w.View.transform.forward;f.y=0;f.Normalize();Vector3 r=w.View.transform.right;r.y=0;r.Normalize();
            return Vector2.ClampMagnitude(new Vector2(Vector3.Dot(direction,r),Vector3.Dot(direction,f)),1);
        }
        IEnumerator Walk(string id,Vector3 target,float max=0,float tolerance=.27f,Action sample=null,Func<IEnumerator> capture=null)
        {
            var route=new Route{id=id,start=w.PlayerPosition,target=target,highestY=w.PlayerPosition.y,lowestY=w.PlayerPosition.y,mode="production SmokeMove + real controller; no route warp"};
            float start=Time.realtimeSinceStartup,end=start+(max>0?max:Vector3.Distance(w.PlayerPosition,target)/PanicHomeWorld.WalkSpeed+2.2f);
            while(Time.realtimeSinceStartup<end)
            {
                Vector3 d=target-w.PlayerPosition;float planar=new Vector2(d.x,d.z).magnitude;
                if(planar<tolerance&&Mathf.Abs(d.y)<.42f){route.reached=true;break;}
                w.SmokeMove(Axis(d),Dt);route.frames++;
                route.highestY=Mathf.Max(route.highestY,w.PlayerPosition.y);route.lowestY=Mathf.Min(route.lowestY,w.PlayerPosition.y);
                CountCollision(route,controller.collisionFlags);yield return null;if(sample!=null)sample();if(capture!=null)yield return capture();
            }
            for(int i=0;i<3;i++){w.SmokeMove(Vector2.zero,Dt);yield return null;}
            route.end=w.PlayerPosition;route.seconds=Time.realtimeSinceStartup-start;report.routes.Add(route);lastReached=route.reached;
            Check("route."+id,route.reached,"start="+route.start+" target="+target+" end="+route.end+" seconds="+route.seconds.ToString("F2"),"NATIVE_PHYSICS_ROUTE");
        }
        static void CountCollision(Route route,CollisionFlags flags)
        {if((flags&CollisionFlags.Sides)!=0)route.sides++;if((flags&CollisionFlags.Above)!=0)route.above++;if((flags&CollisionFlags.Below)!=0)route.below++;}
        IEnumerator Push(string id,Vector3 direction,float seconds,bool jump,bool direct=false)
        {
            var r=new Route{id=id,start=w.PlayerPosition,target=w.PlayerPosition+direction*seconds*PanicHomeWorld.WalkSpeed,jump=jump,
                highestY=w.PlayerPosition.y,lowestY=w.PlayerPosition.y,mode=direct?"declared direct CharacterController.Move PHYSICS probe":"production SmokeMove + real Jump"};
            if(jump)w.Jump();float start=Time.realtimeSinceStartup;
            if(direct)FixtureNote("DIRECT_CONTROLLER_PROBE",id+"; synthetic requested displacement tests actual solid collision, not normal input reachability.");
            while(Time.realtimeSinceStartup-start<seconds)
            {
                if(direct){Physics.SyncTransforms();controller.Move(direction*PanicHomeWorld.WalkSpeed*Dt);report.directControllerProbeFrames++;}
                else w.SmokeMove(Axis(direction),Dt);
                r.frames++;r.highestY=Mathf.Max(r.highestY,w.PlayerPosition.y);r.lowestY=Mathf.Min(r.lowestY,w.PlayerPosition.y);CountCollision(r,controller.collisionFlags);yield return null;
            }
            r.end=w.PlayerPosition;r.seconds=Time.realtimeSinceStartup-start;report.routes.Add(r);
        }
        IEnumerator Open(string id)
        {
            var hinges=Field<IDictionary>(w,"hinges");object hinge=hinges!=null&&hinges.Contains(id)?hinges[id]:null;
            Check("door.exists."+id,hinge!=null,"Real production hinge required.");if(hinge==null)yield break;
            w.SetFocus(id);yield return null;
            Check("door.reachable."+id,w.CanInteract,"distance="+w.InteractionDistance(id)+" actor="+w.PlayerPosition);
            if(!Bool(hinge,"open"))w.HandleKey(KeyCode.E);
            yield return Wait(.58f);Check("door.open."+id,Bool(hinge,"open"),"Production E/toggle, no leaf transform write.");
        }
        IEnumerator Shot(string label)
        {
            Observe(label);yield return new WaitForEndOfFrame();string path=Path.Combine(w.EvidencePath,label+".png");
            ScreenCapture.CaptureScreenshot(path);report.captures.Add(path);yield return null;
            float until=Time.realtimeSinceStartup+.6f;while(!File.Exists(path)&&Time.realtimeSinceStartup<until)yield return null;
            Check("capture.file."+label,File.Exists(path)&&new FileInfo(path).Length>1024,path,"NATIVE_CAPTURE_FILE");
        }
        IEnumerator Photo(string label,Vector3 camera,Vector3 target,float size=0,Action captureSample=null)
        {
            yield return new WaitForEndOfFrame();
            if(captureSample!=null)captureSample();
            if(!photoCamera){photoCamera=new GameObject("V414 QA separate capture camera").AddComponent<Camera>();photoCamera.enabled=false;}
            photoCamera.CopyFrom(w.View);photoCamera.enabled=false;photoCamera.transform.position=camera;photoCamera.transform.LookAt(target);
            photoCamera.orthographic=size>0;if(size>0)photoCamera.orthographicSize=size;else photoCamera.fieldOfView=42;
            photoCamera.cullingMask=~((1<<28)|(1<<30));photoCamera.nearClipPlane=.03f;photoCamera.farClipPlane=180;
            var rt=RenderTexture.GetTemporary(1600,900,24,RenderTextureFormat.ARGB32);var prior=RenderTexture.active;
            photoCamera.targetTexture=rt;photoCamera.Render();RenderTexture.active=rt;var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();string path=Path.Combine(w.EvidencePath,label+".png");File.WriteAllBytes(path,texture.EncodeToPNG());
            report.captures.Add(path);Check("capture.file."+label,File.Exists(path)&&new FileInfo(path).Length>1024,path,"NATIVE_CAPTURE_FILE");FixtureNote("NATIVE_RENDER_CAMERA",label+"; separate capture camera, real live production renderers, no UI or native-input claim.");
            photoCamera.targetTexture=null;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);Destroy(texture);Observe(label);
        }
        Renderer[] Roofs()
        {
            var roof=Field<GameObject>(w,"roof");var garage=Field<GameObject>(w,"residenceGarage");
            return (roof?roof.GetComponentsInChildren<Renderer>(true):Array.Empty<Renderer>()).Concat(garage?garage.GetComponentsInChildren<Renderer>(true).Where(r=>Ancestor(r.transform,"Roof",garage.transform)):Array.Empty<Renderer>()).Distinct().ToArray();
        }
        static bool Ancestor(Transform t,string name,Transform stop){for(;t&&t!=stop;t=t.parent)if(t.name==name)return true;return false;}
        float LidAngle(){var leaf=Field<Transform>(w,"bunkerLeaf");return leaf?Quaternion.Angle(Field<Quaternion>(w,"bunkerClosed"),leaf.localRotation):-1;}
        void Observe(string label)
        {
            var roofs=Roofs();var skin=hero?hero.GetComponentInChildren<SkinnedMeshRenderer>(true):null;
            var bag=Read(w,"BunkerBagObjectV414") as GameObject;var roots=Field<Dictionary<string,GameObject>>(w,"worldSupplyObjects");
            report.frames.Add(new Frame{label=label,frame=Time.frameCount,width=Screen.width,height=Screen.height,phase=w.Model.Phase.ToString(),cinematic=w.CinematicKind,
                remaining=w.Model.TimeRemaining,actor=w.PlayerPosition,camera=w.View.transform.position,lidAngle=LidAngle(),allRoofs=roofs.Length,
                visibleRoofs=roofs.Count(r=>r&&r.enabled&&r.gameObject.activeInHierarchy),characterMesh=skin&&skin.sharedMesh?skin.sharedMesh.name:"missing",bones=skin?skin.bones.Length:0,
                activeSupplies=roots==null?-1:roots.Values.Count(g=>g&&g.activeInHierarchy),popupCount=Int(w,"GatherFeedbackCountV414"),intro=Bool(w,"IntroStoryActiveV414"),
                bunkerStage=Text(w,"BunkerSequenceStageV414"),bag=bag?bag.transform.position:Vector3.zero,bagVisible=bag&&bag.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled&&r.gameObject.activeInHierarchy),
                bagReleased=Bool(w,"BunkerBagReleasedV414"),fps=w.BagFirstPersonActive,controllerEnabled=controller.enabled,focus=Field<string>(w,"focusId"),
                onboarding=Bool(hud,"FreeplayOnboardingOpenV414"),modelPaused=w.Model.IsPaused,inputLocked=w.InputLocked});
        }
        IEnumerator Run()
        {
            yield return Section("launch",Launch());
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            yield return Section("onboarding",Onboarding());
            if(guideOnlyR8){w.ReturnToMenu();yield return Wait(.1f);yield return Shot("99-onboarding-only-final-title");yield break;}
            yield return Section("intro",Intro());
            w.InputLocked=false;FixtureNote("CHARACTER_NORMAL_CLOCK", "InputLocked=false during actual character walk/jump; preparation clock runs and root LateUpdate samples real desiredVelocity. No locomotion bypass.");
            float characterClock=w.Model.TimeRemaining;yield return Section("character",Character());
            Check("character.normal-clock-ran",!w.InputLocked&&w.Model.TimeRemaining<characterClock-.1f,"before="+characterClock+" after="+w.Model.TimeRemaining);
            w.InputLocked=true;FixtureNote("CLOCK_LOCK", "InputLocked=true during bounded geometry/API setup; model timer held. Real controller/colliders continue; this segment does not establish normal locomotion animation or uninterrupted campaign acceptance.");
            yield return Section("layout",Layout());yield return Section("stairs",Stairs());
            yield return Section("outdoor",Outdoor());yield return Section("ceiling",Ceiling());
            w.InputLocked=false;FixtureNote("CLOCK_LOCK_END","Restore normal model clock before actual work, feedback, final45 and cinematic sections.");
            yield return Section("work",Work());yield return Section("utilities",Utilities());yield return Section("feedback",Feedback());
            yield return Section("bag",Bag());yield return Section("bunker",Bunker());yield return Section("hazards",Hazards());
            yield return Section("street",Street());yield return Section("models",Models());
            w.ReturnToMenu();yield return Wait(.1f);Check("final.no-normal-cheat-options",!w.Model.CheatedRun&&!w.Model.DeveloperAnyEnabled);
            yield return Shot("99-final-title");
        }
        IEnumerator Launch()
        {
            string profile=Path.GetFileName((w.EvidencePath??"").TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar));Guid guid;
            Check("launch.fresh-guid",profile.Length>=36&&Guid.TryParseExact(profile.Substring(profile.Length-36),"D",out guid));
            Check("launch.flag-registered",PanicTestLaunchRules.Drivers.Contains(Flag));
            Check("launch.score-isolated",!string.IsNullOrEmpty(report.scorePath)&&Path.GetFullPath(report.scorePath)==Path.Combine(Path.GetFullPath(w.EvidencePath),"isolated-home-scores.json")&&!File.Exists(report.scorePath));
            Check("controller.original-dimensions",controller.enabled&&Mathf.Abs(controller.height-1.78f)<.001f&&Mathf.Abs(controller.radius-.25f)<.001f&&Mathf.Abs(controller.skinWidth-.025f)<.001f,"No collision tolerance relaxation.","NATIVE_PHYSICS");
            yield return null;
        }
        IEnumerator Intro()
        {
            w.ReturnToMenu();w.StartGame(false);report.startGameCalls++;w.FullFlowScriptedInput=true;yield return null;
            bool active=Require(w,"IntroStoryActiveV414","intro.active-seam")&&Bool(w,"IntroStoryActiveV414");
            bool count=Require(w,"IntroStoryResolutionCountV414","intro.resolve-seam");
            Check("intro.started-on-normal-entry",active);float clock=w.Model.TimeRemaining;int before=Int(w,"IntroStoryResolutionCountV414");
            yield return Shot("01-intro-story-street");yield return Wait(.75f);
            Check("intro.preparation-clock-frozen",active&&Bool(w,"IntroStoryActiveV414")&&Mathf.Abs(clock-w.Model.TimeRemaining)<.005f,"before="+clock+" after="+w.Model.TimeRemaining);
            FixtureNote("INTRO_NATURAL_REAL_FRAMES","First normal StartGame waits for the actual9s opening to finish. No direct Tick, cinematic-age write or skip.");
            float naturalEnd=Time.realtimeSinceStartup+10f;bool naturalClockHeld=true;
            while(Bool(w,"IntroStoryActiveV414")&&Time.realtimeSinceStartup<naturalEnd)
            {naturalClockHeld&=Mathf.Abs(clock-w.Model.TimeRemaining)<.005f;yield return null;}
            Check("intro.natural-resolves-once",active&&count&&!Bool(w,"IntroStoryActiveV414")&&!w.CinematicActive&&Int(w,"IntroStoryResolutionCountV414")==before+1&&naturalClockHeld&&Mathf.Abs(clock-w.Model.TimeRemaining)<.25f,"Actual natural end, resolution+1, preparation clock held until control returns.");
            float resumed=w.Model.TimeRemaining;yield return Wait(.18f);
            Check("intro.natural-control-and-clock-resume",controller.enabled&&!w.InputLocked&&!w.CinematicActive&&w.Model.Phase==PanicPhase.Preparing&&w.Model.TimeRemaining<resumed-.05f&&Int(w,"IntroStoryResolutionCountV414")==before+1,"Enabled original controller; real preparation clock resumes without resolving twice.");
            w.ReturnToMenu();w.StartGame(false);report.startGameCalls++;w.FullFlowScriptedInput=true;yield return null;
            FixtureNote("INTRO_SKIP_NEW_RUN","Second explicitly counted normal StartGame isolates double-skip from the first natural-end case.");
            before=Int(w,"IntroStoryResolutionCountV414");
            Check("intro.skip-run-started",Bool(w,"IntroStoryActiveV414"));
            var skip=w.GetType().GetMethod("SkipIntroStoryV414",Instance);
            Check("intro.skip-api-present",skip!=null);
            if(skip!=null){skip.Invoke(w,null);skip.Invoke(w,null);}else if(w.CinematicActive){w.SkipResidenceCinematic();FixtureNote("MISSING_INTRO_FALLBACK","Clear existing presentation only to continue QA; required intro checks already FAIL.");}
            yield return null;
            Check("intro.skip-resolves-once",count&&skip!=null&&!Bool(w,"IntroStoryActiveV414")&&Int(w,"IntroStoryResolutionCountV414")==before+1,"before="+before+" after="+Int(w,"IntroStoryResolutionCountV414"));
            Check("normal.120-plus-45",Mathf.Abs(w.Model.TimeRemaining-165f)<.25f&&Mathf.Abs(PanicHomeWorld.BagPreparationSeconds-45f)<.001f,"Initial model remaining="+w.Model.TimeRemaining+"; no accelerated elapsed-time claim.");
            Check("normal.starts-isometric",w.View.orthographic&&!w.BagCameraOwnsView&&!w.WorkActive);
        }
        void ActivateDefaultFreeplayForQa()
        {
            Call(hud,"ActivateSelectedTitleChoiceV414");report.menuActionCalls++;w.FullFlowScriptedInput=true;
            if(w.Model.IsJuryDemo)report.shortFlowStarts++;
            else if(!w.Model.IsSimulation&&(w.Model.Phase==PanicPhase.Preparing||w.Model.Phase==PanicPhase.Tutorial))report.startGameCalls++;
        }
        IEnumerator GuideResolutionCaptureR7(int width,int height,string label)
        {
            float clock=w.Model.TimeRemaining,age=Field<float>(w,"residenceCinematicAge");Vector3 actor=w.PlayerPosition;
            int resolutions=Int(hud,"FreeplayOnboardingResolutionCountV414");
            Screen.SetResolution(width,height,FullScreenMode.Windowed);
            float deadline=Time.realtimeSinceStartup+3;
            while((Screen.width!=width||Screen.height!=height)&&Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;yield return new WaitForEndOfFrame();
            Check("onboarding.resolution."+width+"x"+height,Screen.width==width&&Screen.height==height,"Actual window resolution; no image resampling or QA camera.","NATIVE_RENDER_STATE");
            Check("onboarding.same-paused-guide."+width+"x"+height,Bool(hud,"FreeplayOnboardingOpenV414")&&w.Model.IsPaused&&w.InputLocked&&
                Mathf.Abs(w.Model.TimeRemaining-clock)<.005f&&Mathf.Abs(Field<float>(w,"residenceCinematicAge")-age)<.005f&&Vector3.Distance(actor,w.PlayerPosition)<.005f&&Int(hud,"FreeplayOnboardingResolutionCountV414")==resolutions,
                "Same unacknowledged guide/clock/actor/cinematic across actual surface resize.");
            yield return Shot(label);
            string path=Path.Combine(w.EvidencePath,label+".png");byte[] png=File.Exists(path)?File.ReadAllBytes(path):Array.Empty<byte>();
            Check("onboarding.actual-png-size."+width+"x"+height,png.Length>=24&&PngDimensionR7(png,16)==width&&PngDimensionR7(png,20)==height,"Actual ScreenCapture PNG IHDR dimensions.","NATIVE_CAPTURE_FILE");
        }
        static int PngDimensionR7(byte[] data,int offset)
        {return data[offset]<<24|data[offset+1]<<16|data[offset+2]<<8|data[offset+3];}
        IEnumerator Onboarding()
        {
            bool seams=Require(hud,"FreeplayOnboardingOpenV414","onboarding.open-seam")&Require(hud,"FreeplayOnboardingSeenV414","onboarding.seen-seam")&
                Require(hud,"FreeplayOnboardingResolutionCountV414","onboarding.once-seam")&Require(hud,"PrimaryPlayLabelV414","onboarding.primary-label-seam")&Require(hud,"FreeplayOnboardingInputBlockedV414","onboarding.input-owner-seam");
            bool actions=hud.GetType().GetMethod("ActivateSelectedTitleChoiceV414",Instance)!=null&&hud.GetType().GetMethod("ConfirmFreeplayOnboardingV414",Instance)!=null;
            Check("onboarding.production-actions-present",actions,"Real title Enter/selected action and real ANLADIM BAŞLA acknowledgement must expose shared production actions; no QA-only state mutation shim.");
            if(!seams||!actions)yield break;
            Check("onboarding.freeplay-default-title",w.Model.Phase==PanicPhase.Title&&Field<int>(hud,"_menuChoice")==0&&Text(hud,"PrimaryPlayLabelV414")=="SERBEST OYNA"&&!Bool(hud,"FreeplayOnboardingSeenV414"),"Fresh actual title choice0 and real primary label; keyboard OS Enter remains root-owned.");
            FixtureNote("ONBOARDING_ACTUAL_MENU_ACTION","Invoke shared production selected-title action at untouched fresh choice0, same dispatch used by the title Enter path. No menu field/seen flag/timer/Model.Tick writes and no OS input.");
            ActivateDefaultFreeplayForQa();object openingConfirm=Call(hud,"ConfirmFreeplayOnboardingV414");
            Check("onboarding.opening-frame-does-not-auto-ack",openingConfirm is bool openingAccepted&&!openingAccepted&&Bool(hud,"FreeplayOnboardingOpenV414")&&!Bool(hud,"FreeplayOnboardingSeenV414"),"Real production confirm rejects the title opening frame, preventing title Enter/click from instantly consuming the explanation.");
            yield return null;
            Check("onboarding.first-free-is-normal165",w.Model.Phase==PanicPhase.Preparing&&!w.JuryActive&&!w.Model.IsSimulation&&Mathf.Abs(w.Model.TimeRemaining-165)<.005f&&Mathf.Abs(PanicHomeWorld.BagPreparationSeconds-45)<.001f,"Normal120+45 run; default action must not enter short flow.");
            Check("onboarding.first-free-modal-owns-pause",Bool(hud,"FreeplayOnboardingOpenV414")&&w.Model.IsPaused&&w.InputLocked,"Model pause and world input lock must remain owned by guide through the next phase synchronization.");
            float clock=w.Model.TimeRemaining,cinemaAge=Field<float>(w,"residenceCinematicAge");Vector3 actor=w.PlayerPosition;int budget=w.Model.Budget;string stock=Stock();
            int resolutions=Int(hud,"FreeplayOnboardingResolutionCountV414");
            Check("onboarding.guide-initial1600",Screen.width==1600&&Screen.height==900,"Root launches visible1600x900; smaller surfaces are captured on this same paused guide.","NATIVE_RENDER_STATE");
            yield return Shot("00-first-freeplay-controls-guide");
            yield return GuideResolutionCaptureR7(1280,720,"00-first-freeplay-controls-guide-1280x720");
            yield return GuideResolutionCaptureR7(960,540,"00-first-freeplay-controls-guide-960x540");
            yield return GuideResolutionCaptureR7(1600,900,"00-first-freeplay-controls-guide-1600x900-restored");
            yield return Wait(.6f);
            Check("onboarding.real-frame-clock-input-cinema-freeze",Bool(hud,"FreeplayOnboardingOpenV414")&&w.Model.IsPaused&&w.InputLocked&&Mathf.Abs(w.Model.TimeRemaining-clock)<.005f&&Vector3.Distance(actor,w.PlayerPosition)<.005f&&Mathf.Abs(Field<float>(w,"residenceCinematicAge")-cinemaAge)<.005f,
                "Real.6s frame wait; clock alone is insufficient because intro could also freeze165. No movement adapter or direct clock call used.");
            foreach(var key in new[]{KeyCode.E,KeyCode.F,KeyCode.Space,KeyCode.Tab,KeyCode.B,KeyCode.F2,KeyCode.Alpha1,KeyCode.Alpha3}){w.HandleKey(key);yield return null;}
            Check("onboarding.production-key-dispatch-blocked",Bool(hud,"FreeplayOnboardingOpenV414")&&w.Model.IsPaused&&w.InputLocked&&!w.InventoryOpen&&!w.WorkActive&&!w.IsMoving&&!hud.DeveloperMenuOpen&&!w.BagCameraOwnsView&&w.Model.Budget==budget&&Stock()==stock&&Vector3.Distance(actor,w.PlayerPosition)<.005f&&Int(hud,"FreeplayOnboardingResolutionCountV414")==resolutions,
                "Production HandleKey E/F/Space/Tab/B/F2/1/3 cannot mutate/open other modes. API dispatch is not OS WASD acceptance; source Update pause gate and later root OS remain separate.");
            w.ReturnToMenu();yield return null;yield return null;
            Check("onboarding.unacknowledged-menu-cleanup",w.Model.Phase==PanicPhase.Title&&!Bool(hud,"FreeplayOnboardingOpenV414")&&!w.Model.IsPaused&&!w.InputLocked&&!Bool(hud,"FreeplayOnboardingSeenV414"),"Real menu return releases guide-owned modal/pause/lock; unacknowledged first-use state remains unseen.");
            ActivateDefaultFreeplayForQa();yield return null;
            Check("onboarding.unacknowledged-free-reopens",Bool(hud,"FreeplayOnboardingOpenV414")&&w.Model.IsPaused&&w.InputLocked&&!w.JuryActive&&Mathf.Abs(w.Model.TimeRemaining-165)<.005f,"Second explicitly counted start must still teach an unacknowledged first free session.");
            int before=Int(hud,"FreeplayOnboardingResolutionCountV414");object first=Call(hud,"ConfirmFreeplayOnboardingV414"),second=Call(hud,"ConfirmFreeplayOnboardingV414");
            Check("onboarding.ack-consumes-world-frame",Bool(hud,"FreeplayOnboardingInputBlockedV414")&&w.CancelFrame==Time.frameCount&&Int(w,"DiscreteInputFrame")==Time.frameCount,"Real same-frame input owner remains blocked after confirmation; OS Enter/key-up swallowing is separate root evidence.");
            yield return null;
            Check("onboarding.ack-resolves-once",first is bool accepted&&accepted&&second is bool duplicate&&!duplicate&&!Bool(hud,"FreeplayOnboardingOpenV414")&&Bool(hud,"FreeplayOnboardingSeenV414")&&Int(hud,"FreeplayOnboardingResolutionCountV414")==before+1,
                "Two real production confirm calls; only first resolves. This is the same action as visible ANLADIM BAŞLA/Enter, not a synthetic key event.");
            Check("onboarding.ack-releases-owned-pause",!w.Model.IsPaused&&!w.InputLocked&&Mathf.Abs(w.Model.TimeRemaining-165)<.25f&&!w.Model.CheatedRun&&!w.Model.DeveloperAnyEnabled,"Normal clock remains165 before gameplay; guide use is not cheating.");
            if(Bool(w,"IntroStoryActiveV414")){w.SkipIntroStoryV414();report.onboardingIntroSkips++;FixtureNote("ONBOARDING_REAL_INTRO_SKIP","Production intro skip after guide acknowledgement only, so actual preparation clock resume can be observed without another9s wait. Existing Intro section still covers natural9s separately.");}
            float resumed=w.Model.TimeRemaining;yield return Wait(.18f);
            Check("onboarding.actual-normal-clock-resumes",!Bool(hud,"FreeplayOnboardingOpenV414")&&!w.Model.IsPaused&&!w.InputLocked&&controller.enabled&&w.Model.Phase==PanicPhase.Preparing&&w.Model.TimeRemaining<resumed-.05f,"Real normal frames after modal/intro release; no time assignment or Tick.");
            w.ReturnToMenu();yield return null;ActivateDefaultFreeplayForQa();yield return null;
            Check("onboarding.only-first-free-this-session",Bool(hud,"FreeplayOnboardingSeenV414")&&!Bool(hud,"FreeplayOnboardingOpenV414")&&!w.Model.IsPaused&&!w.JuryActive&&Mathf.Abs(w.Model.TimeRemaining-165)<.25f,"Fresh run after confirmed guide does not reopen it in this process session; no persistent save flag is written by QA.");
            w.ReturnToMenu();yield return null;Call(hud,"ActivateTitleChoiceV414",1);report.menuActionCalls++;if(w.Model.IsJuryDemo)report.shortFlowStarts++;w.FullFlowScriptedInput=true;yield return null;
            Check("onboarding.short-flow-keeps-own-mode",w.JuryActive&&w.Model.IsJuryDemo&&!Bool(hud,"FreeplayOnboardingOpenV414")&&!w.Model.IsPaused&&!w.InputLocked,"Actual separate short-flow UI action remains available and does not inherit freeplay modal/pause.");
            w.ReturnToMenu();yield return null;
            Check("onboarding.final-title-cleanup",w.Model.Phase==PanicPhase.Title&&!Bool(hud,"FreeplayOnboardingOpenV414")&&!w.Model.IsPaused&&!w.InputLocked&&!w.WorkActive&&!w.InventoryOpen&&!hud.DeveloperMenuOpen);
        }
        static Quaternion[] Pose(Transform[] bones){return bones.Select(t=>t?t.localRotation:Quaternion.identity).ToArray();}
        static int Changed(Quaternion[] a,Quaternion[] b){return a.Zip(b,(x,y)=>Quaternion.Angle(x,y)>.12f?1:0).Sum();}
        static Vector3[] BakedLocalVertices(SkinnedMeshRenderer skin)
        {
            var mesh=new Mesh{name="V414 read-only actual gait bake"};
            try{skin.BakeMesh(mesh);return mesh.vertices;}finally{Destroy(mesh);}
        }
        IEnumerator CaptureWalkingGait(GaitWindow gait)
        {
            if(gait.Captured>=6||Time.realtimeSinceStartup<gait.Next)yield break;
            string label="02-walk-phase-"+(gait.Captured+1).ToString("D2")+"-front";
            Vector3 facing=hero.forward;facing.y=0;facing.Normalize();
            yield return Photo(label,w.PlayerPosition+facing*3.3f+Vector3.up*1.15f,w.PlayerPosition+Vector3.up*.95f,0,()=>
            {
                float now=Time.realtimeSinceStartup;Vector3[] current=BakedLocalVertices(gait.Skin),previous=gait.Previous??gait.Rest;
                float sum=0,restSum=0,peak=0;int changed=0;bool same=current.Length==gait.Rest.Length&&current.Length==previous.Length;
                if(same)for(int i=0;i<current.Length;i++)
                {float delta=(current[i]-previous[i]).sqrMagnitude;sum+=delta;restSum+=(current[i]-gait.Rest[i]).sqrMagnitude;peak=Mathf.Max(peak,Mathf.Sqrt(delta));if(delta>.000025f)changed++;}
                var sample=new GaitSample{label=label,mesh=gait.Skin.sharedMesh?gait.Skin.sharedMesh.name:"missing",phase=gait.Captured+1,frame=Time.frameCount,vertices=current.Length,
                    changedVertices=changed,changedBones=Changed(gait.Bones,Pose(gait.Skin.bones)),secondsSinceWalkStart=now-gait.Start,secondsSincePrevious=now-gait.PreviousTime,
                    actorTravel=Vector3.Distance(gait.ActorStart,w.PlayerPosition),peakLocalVertexDelta=peak,rmsLocalVertexDelta=same&&current.Length>0?Mathf.Sqrt(sum/current.Length):0,
                    restRmsLocalVertexDelta=same&&current.Length>0?Mathf.Sqrt(restSum/current.Length):0,actor=w.PlayerPosition};report.gaitSamples.Add(sample);
                sample.evaluatedClips=gait.Animator?string.Join("|",gait.Animator.GetCurrentAnimatorClipInfo(0).Where(c=>c.clip&&c.weight>.05f).Select(c=>c.clip.name+":"+c.weight.ToString("F3"))):"missing";
                Check("character.gait-live-baked-sample."+sample.phase,same&&current.Length>0&&gait.Skin.enabled&&gait.Skin.gameObject.activeInHierarchy&&!w.InputLocked&&sample.actorTravel>.02f,
                    "Actual SMR.BakeMesh local vertices after root LateUpdate; phase="+sample.phase+" vertices="+sample.vertices+" changed="+sample.changedVertices+" peak="+sample.peakLocalVertexDelta+" rms="+sample.rmsLocalVertexDelta+" interval="+sample.secondsSincePrevious+" travel="+sample.actorTravel,"NATIVE_BAKED_SKIN_DEFORMATION");
                gait.Previous=current;gait.PreviousTime=now;gait.Next=now+.12f;gait.Captured++;
            });
        }
        IEnumerator Character()
        {
            yield return Place(new Vector3(0,0,-18),"character-exterior-open-road");
            var skin=hero.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(r=>r.enabled&&r.sharedMesh&&r.bones.Length>0).OrderByDescending(r=>r.sharedMesh.vertexCount).FirstOrDefault();
            var rig=hero.GetComponents<Component>().FirstOrDefault(c=>c&&c.GetType().Name=="PanicCuratedCharacterRig");
            var prefab=Resources.Load<GameObject>("Player/SurvivorV2");var prefabAnimator=prefab?prefab.GetComponentInChildren<Animator>(true):null;
            var animator=Field<Animator>(w,"animator");var clips=animator&&animator.runtimeAnimatorController?animator.runtimeAnimatorController.animationClips:Array.Empty<AnimationClip>();
            Check("character.retained-humanoid-source",!rig&&skin&&skin.sharedMesh&&prefab&&prefabAnimator&&animator&&animator.avatar&&animator.avatar.isValid&&animator.avatar.isHuman&&animator.avatar==prefabAnimator.avatar&&animator.runtimeAnimatorController==prefabAnimator.runtimeAnimatorController&&prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(r=>r.sharedMesh==skin.sharedMesh),
                "Actual retained working humanoid Avatar/controller and shared baked mesh identity. User authorized new appearance on this rig; no workwear-byte/texture/19bone/Meshy oracle.");
            Check("character.retained-clips-controller",new[]{"Idle_Loop","Walk_Loop","Jog_Fwd_Loop","Jump_Loop"}.All(suffix=>clips.Any(c=>c&&c.length>0&&c.name.EndsWith(suffix,StringComparison.OrdinalIgnoreCase)))&&clips.Any(c=>c&&c.length>0&&(c.name.IndexOf("Interact",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("Push",StringComparison.OrdinalIgnoreCase)>=0))&&clips.Any(c=>c&&c.length>0&&c.name.IndexOf("Fixing",StringComparison.OrdinalIgnoreCase)>=0),
                "Actual bound retained UAL Idle/Walk/Jog/Jump/Interact(orPush)/Fixing clips with nonzero duration; clip presence is not OS run acceptance.");
            Check("character.retained-avatar-contract",skin&&skin.bones.Length>19&&animator&&animator.isHuman&&!animator.applyRootMotion&&animator.runtimeAnimatorController,"Real humanoid bones and Avatar drive the skin; root motion stays disabled.");
            Check("character.appearance-materials-present",skin&&skin.sharedMaterials.Length>0&&skin.sharedMaterials.All(m=>m),"Actual visible fallback skin materials exist. Revised clothes/colors/face/hair are user-authorized; human appearance acceptance remains open.");
            FixtureNote("RETAINED_HUMANOID_IDENTITY","User rejected new procedural gait. Avatar="+(animator&&animator.avatar?animator.avatar.name:"missing")+" controller="+(animator&&animator.runtimeAnimatorController?animator.runtimeAnimatorController.name:"missing")+" actualClips="+string.Join("|",clips.Where(c=>c).Select(c=>c.name))+". Appearance may differ from old workwear; humanoid/UAL mechanics must remain real.");
            if(skin&&skin.sharedMesh)
            {
                var weights=skin.sharedMesh.boneWeights;int boneCount=skin.bones.Length;
                Check("character.weights-normalized",weights.Length==skin.sharedMesh.vertexCount&&weights.All(b=>Finite(b.weight0)&&Finite(b.weight1)&&Finite(b.weight2)&&Finite(b.weight3)&&Mathf.Abs(b.weight0+b.weight1+b.weight2+b.weight3-1)<.001f&&ValidBone(b.boneIndex0,b.weight0,boneCount)&&ValidBone(b.boneIndex1,b.weight1,boneCount)&&ValidBone(b.boneIndex2,b.weight2,boneCount)&&ValidBone(b.boneIndex3,b.weight3,boneCount)),"Actual retained skin4weight normalization and real bone index range.");
                var before=Pose(skin.bones);Vector3 p=w.PlayerPosition;int walkDeltas=0;float gaitStart=Time.realtimeSinceStartup;
                var gait=new GaitWindow{Skin=skin,Animator=animator,Rest=BakedLocalVertices(skin),Bones=before,ActorStart=p,Start=gaitStart,PreviousTime=gaitStart,Next=gaitStart+.10f};
                FixtureNote("GAIT_REAL_MOTOR_CAPTURE","Six phase photos target.12s spacing during actual normal-clock SmokeMove on open road. Photo samples actual SMR.BakeMesh local vertices after root LateUpdate, so actor world translation is excluded. No pose, bone, rig-time, renderer or actor-transform write. Actual sample intervals are recorded; separate camera images are NATIVE_RENDER_CAMERA, not OS/UI frames.");
                yield return Walk("character-walk",p+Vector3.right*4.4f,3f,.27f,()=>{walkDeltas=Mathf.Max(walkDeltas,Changed(before,Pose(skin.bones)));},()=>CaptureWalkingGait(gait));
                Check("character.six-actual-walk-phase-captures",gait.Captured==6&&report.gaitSamples.Count==6,"Actual continuous route captures="+gait.Captured+"; phase PNGs and exact intervals are retained. No setup warp inside walk.","NATIVE_PHYSICS_ANIMATION");
                Check("character.actual-baked-skin-walk-deforms",gait.Captured==6&&report.gaitSamples.Skip(1).Count(s=>s.rmsLocalVertexDelta>.001f&&s.changedVertices>=8)>=3&&report.gaitSamples.Skip(1).Any(s=>s.peakLocalVertexDelta>.01f),
                    "Compare consecutive live local baked vertex arrays, excluding world translation: >=3 inter-phase RMS changes above1mm with >=8 vertices each, plus a >1cm peak. Bone-only change cannot satisfy this check.","NATIVE_BAKED_SKIN_DEFORMATION");
                Check("character.motor-walk-deforms",!w.InputLocked&&Vector3.Distance(p,w.PlayerPosition)>1.5f&&walkDeltas>=4,"Normal clock/unlocked real SmokeMove -> root LateUpdate -> live joint changes sampled while walking; deltas="+walkDeltas,"NATIVE_PHYSICS_ANIMATION");
                Check("character.actual-retained-walk-clip-evaluated",report.gaitSamples.Any(s=>s.evaluatedClips!=null&&s.evaluatedClips.IndexOf("Walk",StringComparison.OrdinalIgnoreCase)>=0),"Actual Animator clip weights observed during real SmokeMove and local BakeMesh sampling. No synthetic Animator.Play/Update, rig time or bone writes.");
                FixtureNote("RETAINED_RUN_SCOPE","Bound real UAL Jog clip verified above. This route exercises actual walk/jump; OS Shift/controller run and human gait judgment remain separate, not inferred from clip existence.");
                while(!w.IsGrounded){w.SmokeMove(Vector2.zero,Dt);yield return null;}
                var grounded=Pose(skin.bones);float baseY=w.PlayerPosition.y;w.Jump();bool airborne=false,jumpClipObserved=false;int deltas=0;float peak=baseY;
                float end=Time.realtimeSinceStartup+1.2f;while(Time.realtimeSinceStartup<end){w.SmokeMove(Vector2.zero,Dt);yield return null;peak=Mathf.Max(peak,w.PlayerPosition.y);if(!w.IsGrounded){airborne=true;deltas=Mathf.Max(deltas,Changed(grounded,Pose(skin.bones)));jumpClipObserved|=animator&&animator.GetCurrentAnimatorClipInfo(0).Any(c=>c.clip&&c.weight>.05f&&c.clip.name.IndexOf("Jump",StringComparison.OrdinalIgnoreCase)>=0);}}
                Check("character.actual-jump-deforms",airborne&&peak>baseY+.6f&&deltas>=4,"Real Jump/controller y and root-driven joint changes; peak="+peak,"NATIVE_PHYSICS_ANIMATION");
                Check("character.actual-retained-jump-clip-evaluated",jumpClipObserved,"Real enabled-controller airborne frames must evaluate the actual retained Jump clip; no Animator.Play/Update or pose mutation by QA.","NATIVE_PHYSICS_ANIMATION");
            }
            Vector3 facing=hero.forward;facing.y=0;facing.Normalize();
            FixtureNote("CHARACTER_CAPTURE_FACING","Open-road original live hero. Front/back cameras use actual hero.forward; no renderer, actor rotation, material or overlay override.");
            yield return Photo("03-character-front",w.PlayerPosition+facing*3.3f+Vector3.up*1.15f,w.PlayerPosition+Vector3.up*.95f);
            yield return Photo("04-character-back",w.PlayerPosition-facing*3.3f+Vector3.up*1.15f,w.PlayerPosition+Vector3.up*.95f);
        }
        static bool Finite(float n){return !float.IsNaN(n)&&!float.IsInfinity(n);}
        static bool ValidBone(int index,float weight,int count){return weight<=0||index>=0&&index<count;}
        IEnumerator Layout()
        {
            var windows=w.Model.Items.Where(i=>i.Kind==ItemKind.Window).ToArray();var windowRoots=Field<Dictionary<string,GameObject>>(w,"items");
            Check("layout.eight-real-model-window-identities",windows.Length==8&&windows.Select(i=>i.Id).Distinct().Count()==8&&windows.All(i=>i.Installed&&windowRoots!=null&&windowRoots.TryGetValue(i.Id,out var root)&&root&&root.GetComponentsInChildren<Renderer>(true).Length>0),"Exactly8 distinct installed Model windows with actual production mesh roots, including hidden upper-floor renderers; no visible-from-one-camera count.");
            Check("layout.new-room-door-targets",new[]{"door_wc","door_pantry","door_parent","door_child","door_study","bathroom_door","door_garage_inner"}.All(id=>w.Model.Items.Any(i=>i.Id==id&&i.Installed&&i.Kind==ItemKind.Door)));
            Check("layout.spacious-canonical-partitions",Mathf.Abs(PanicHouseLayoutV414.UpperWestPartition+2.65f)<.001f&&Mathf.Abs(PanicHouseLayoutV414.UpperEastPartition-2.65f)<.001f,"Final spacious layout helper must expose west=-2.65/east=2.65; old narrow helper cannot silently redefine this oracle.");
            string[] upperDoorIds={"door_parent","door_child","door_study","bathroom_door"};
            var upperSeats=PanicHouseLayoutV414.Doors.Where(d=>upperDoorIds.Contains(d.Id)).ToArray();
            Check("layout.spacious-four-canonical-door-models",upperSeats.Length==4&&upperSeats.All(d=>w.Model.Items.Any(i=>i.Id==d.Id&&i.Installed&&i.Kind==ItemKind.Door&&Vector3.Distance(i.Position,d.Position)<.01f&&Mathf.Abs(Mathf.DeltaAngle(i.Rotation,d.Yaw))<.1f)),"Actual four upper Model doors must match canonical helper positions and yaws; room reachability is separately walked with real controller.");
            var bathroomDoor=w.Model.Items.Find(i=>i.Id=="bathroom_door");
            Check("layout.bath-west-door-contract",bathroomDoor!=null&&Vector3.Distance(bathroomDoor.Position,new Vector3(PanicHouseLayoutV414.UpperEastPartition,3.2f,4.6f))<.01f&&Mathf.Abs(Mathf.DeltaAngle(bathroomDoor.Rotation,90))<.1f,"Final spacious west door x2.65 at north landing z4.6, yaw90, gap3.92..5.28; old x2/south and earlier westz2.6 geometry must not be reused.");
            Check("layout.two-distinct-real-beds",new[]{"bed","bed_child"}.All(id=>w.Model.Items.Any(i=>i.Id==id&&i.Installed&&i.Kind==ItemKind.Furniture&&Mathf.Abs(i.Position.y-3.2f)<.01f)));
            var kitchen=FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="HomeV4_Kitchen_v414"||t.name=="HomeV4_Kitchen_v414(Clone)"||t.name=="HomeV414_Kitchen");
            if(!kitchen)kitchen=FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="KitchenCounterSinkHobDetails");
            Check("layout.actual-kitchen-sink-hob-mesh",kitchen&&kitchen.GetComponentsInChildren<MeshFilter>(true).Any(f=>f.sharedMesh&&f.sharedMesh.vertexCount>100),"Actual authored detail mesh present; visual review remains separate.");
            Check("layout.sources-follow-model",w.Expedition.Sources.All(s=>{var item=w.Model.Items.Find(i=>i.Id==s.Id);return item==null||Vector3.Distance(s.Position,item.Position)<.001f;}));
            var bag=w.Model.Items.Find(i=>i.Id=="bagstation");Check("layout.bag-beside-new-stair",bag!=null&&Vector3.Distance(bag.Position,new Vector3(1.55f,.035f,-2.3f))<.01f);
            yield return Place(new Vector3(0,0,-7.5f),"front-entry");yield return Open("door");
            yield return Walk("entry-foyer",new Vector3(0,0,-4.9f));yield return Walk("living-entry",new Vector3(-1.1f,0,-3.9f));
            yield return Walk("living-around-furniture",new Vector3(-2.5f,0,-.3f));yield return Walk("living-north",new Vector3(-4.1f,0,2.4f));
            yield return Place(new Vector3(3.35f,0,-.6f),"kitchen-door-approach");yield return Open("door_inner_ground");
            FixtureNote("FINAL_SPACIOUS_KITCHEN_ROUTE","Dining table now(5,0,2.6), authored top x[4.225,5.775],z[2.13,3.07]. West solid counter ends atx3.25, beginsz2.45: x3.35 cannot take the original CC north. Use x3.7 between unchanged counter/table colliders, then north bandz3.7; all Walk tolerances/dimensions unchanged.");
            foreach(var p in new[]{new Vector3(3.35f,0,1.6f),new Vector3(3.7f,0,1.6f),new Vector3(3.7f,0,3.7f),new Vector3(4.6f,0,3.7f),new Vector3(5.5f,0,4.05f)})yield return Walk("kitchen-"+p.ToString("F1"),p);
            yield return Photo("05-ground-room-overview",new Vector3(13,16,-16),new Vector3(0,.5f,0),12.5f);
            yield return Place(new Vector3(3.35f,0,-1.3f),"wc-door-approach");yield return Open("door_wc");yield return Walk("wc-entry",new Vector3(3.35f,0,-3.75f));yield return Walk("wc-interior",new Vector3(3.35f,0,-4.35f));
            yield return Place(new Vector3(6.4f,0,-1.3f),"pantry-door-approach");yield return Open("door_pantry");yield return Walk("pantry-entry",new Vector3(6.4f,0,-3.65f));yield return Walk("pantry-interior",new Vector3(6.25f,0,-4.35f));
            FixtureNote("FINAL_SPACIOUS_GARAGE_ROUTE","Existing dining group moved. Use canonical north kitchen band z3.7 and the existing E door path, not the old z3.15 shortcut; controller and Walk tolerance stay unchanged.");
            yield return Place(new Vector3(4.4f,0,3.5f),"garage-route-approach");yield return Walk("garage-kitchen-north-band",new Vector3(4.4f,0,3.7f));
            yield return Walk("garage-kitchen-passage",new Vector3(6.9f,0,3.7f));yield return Walk("garage-inner-door-approach",new Vector3(7.15f,0,2.2f));yield return Open("door_garage_inner");
            yield return Walk("garage-through-door",new Vector3(9.15f,0,2.2f));yield return Walk("garage-panel-passage",new Vector3(9.65f,0,3.5f));yield return Walk("garage-panel-approach",new Vector3(9.65f,0,5));
        }
        IEnumerator Stairs()
        {
            float westHall=PanicHouseLayoutV414.UpperWestPartition+.85f,eastHall=PanicHouseLayoutV414.UpperEastPartition-.85f;
            FixtureNote("FINAL_SPACIOUS_UPPER_ROUTES","Final canonical partitions +/-2.65 with real hall routes +/-1.8; retained bathroom entry x3.1 and z4.6 avoids its actual sink. No tolerance or collider changes.");
            yield return Place(new Vector3(0,0,-1.4f),"stairs-ground-approach");
            yield return Walk("stairs-upper-real-flight",new Vector3(0,3.2f,4.55f),5f);Check("stairs.upper-level-reached",lastReached&&w.IsGrounded&&w.ActiveFloor==1,"No warp in ascent route.","NATIVE_PHYSICS_ROUTE");
            yield return Walk("upper-parent-hall",new Vector3(westHall,3.2f,4.6f));yield return Walk("upper-parent-door",new Vector3(westHall,3.2f,3.9f));yield return Open("door_parent");
            yield return Walk("parent-entry",new Vector3(-3.1f,3.2f,3.9f));yield return Walk("parent-interior",new Vector3(-3.7f,3.2f,2.1f));
            yield return Place(new Vector3(westHall,3.2f,4.6f),"upper-west-hall-approach");yield return Walk("west-hall-child-route",new Vector3(westHall,3.2f,-3.2f),4.5f);yield return Open("door_child");
            yield return Walk("child-entry",new Vector3(-3.2f,3.2f,-3.2f));yield return Walk("child-interior",new Vector3(-4,3.2f,-1.5f));
            yield return Place(new Vector3(eastHall,3.2f,4.6f),"upper-east-hall-approach");yield return Walk("east-hall-study-route",new Vector3(eastHall,3.2f,-2.6f),4.5f);yield return Open("door_study");
            yield return Walk("study-entry",new Vector3(3.1f,3.2f,-2.6f));yield return Walk("study-interior",new Vector3(5.2f,3.2f,-3.6f));
            yield return Place(new Vector3(eastHall,3.2f,4.6f),"bath-north-landing-approach");yield return Open("bathroom_door");yield return Walk("bath-west-entry",new Vector3(3.1f,3.2f,4.6f));yield return Walk("bath-interior",new Vector3(4,3.2f,3.5f));
            yield return Photo("06-upper-two-bedrooms-study-bath",new Vector3(13,19,-16),new Vector3(0,3.8f,0),12.5f);
            yield return Place(new Vector3(0,3.2f,-3.6f),"balcony-approach");yield return Walk("balcony-door-approach",new Vector3(0,3.2f,-5.1f));yield return Open("door_balcony");yield return Walk("balcony-outside",new Vector3(0,3.2f,-6.8f));
            yield return Place(new Vector3(-1.5f,0,-2),"basement-ground-approach");yield return Walk("basement-west-hall",new Vector3(-1.5f,0,4.6f),4.5f);yield return Walk("basement-north-landing",new Vector3(0,0,4.45f));yield return Walk("basement-flight-entry",new Vector3(0,0,3.85f));
            yield return Walk("basement-real-descent",new Vector3(0,-3.2f,-1.5f),5f);Check("stairs.basement-level-reached",lastReached&&w.IsGrounded&&w.ActiveFloor==-1,"No warp in descent route.","NATIVE_PHYSICS_ROUTE");
            yield return Walk("basement-south-passage",new Vector3(-2.4f,-3.2f,-1.5f));yield return Walk("basement-shelter-approach",new Vector3(-4.4f,-3.2f,1.9f));
            yield return Shot("07-basement-approach");
        }
        IEnumerator Outdoor()
        {
            var solids=FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(c=>c.name.StartsWith("V414 ")&&(c.name.Contains("bank guard")||c.name.Contains("Bridge railing"))).ToArray();
            Check("creek.solid-bank-and-rail-assets",solids.Length>=6&&solids.All(c=>c.enabled&&c.gameObject.activeInHierarchy&&!c.isTrigger),"count="+solids.Length,"NATIVE_PHYSICS");
            yield return Place(new Vector3(-15.3f,0,4),"creek-east-bank");yield return Push("bank-walk",Vector3.left,1.2f,false);
            Check("creek.walk-rejected",w.PlayerPosition.x>-16.1f&&report.routes.Last().sides>0,"Actual controller sides="+report.routes.Last().sides+" actor="+w.PlayerPosition,"NATIVE_PHYSICS");
            yield return Push("bank-jump",Vector3.left,1.3f,true);Check("creek.jump-rejected",w.PlayerPosition.x>-16.1f&&report.routes.Last().highestY>.5f&&report.routes.Last().sides>0,"No bank entry while real Jump requested.","NATIVE_PHYSICS");
            yield return Shot("08-creek-bank-jump-guard");
            yield return Place(new Vector3(-15.1f,0,-19),"bridge-east-approach");yield return Walk("bridge-west-crossing",new Vector3(-20.55f,0,-19),4f);
            var support=Physics.RaycastAll(w.PlayerPosition+Vector3.up*.25f,Vector3.down,.6f,~(1<<30),QueryTriggerInteraction.Ignore).Where(h=>h.collider!=controller&&!h.transform.IsChildOf(controller.transform)).OrderBy(h=>h.distance).FirstOrDefault();
            Check("bridge.actual-deck-support",lastReached&&w.IsGrounded&&report.routes.Last().lowestY>-.12f&&support.collider&&support.collider.name=="V414 Bridge deck","Actual supporting first collider="+(support.collider?support.collider.name:"missing")+"; no water-floor warp.","NATIVE_PHYSICS_ROUTE");
            yield return Place(new Vector3(-18.5f,0,-16),"bridge-north-rail-approach");yield return Push("bridge-north-walk",Vector3.forward,1.1f,false);
            Check("bridge.north-rail-walk",w.PlayerPosition.z<-14.55f&&report.routes.Last().sides>0,"NATIVE actual rail contact","NATIVE_PHYSICS");
            yield return Push("bridge-north-jump",Vector3.forward,1.25f,true);Check("bridge.north-rail-jump",w.PlayerPosition.z<-14.55f&&report.routes.Last().sides>0&&report.routes.Last().highestY>.5f,"NATIVE real jump rejected by rail","NATIVE_PHYSICS");
            yield return Place(new Vector3(-18.5f,0,-21),"bridge-south-physics-probe");yield return Push("bridge-south-direct-capsule",Vector3.back,1.25f,false,true);
            Check("bridge.south-rail-solid-physics",w.PlayerPosition.z>-23.45f&&w.PlayerPosition.z<-22.5f&&report.routes.Last().sides>0,"Direct CharacterController.Move probe because production world clamp ends atz-21.3; not a normal route claim.","NATIVE_PHYSICS_PROBE");
            yield return Photo("09-bridge-rails",new Vector3(-10,7,-11),new Vector3(-18.6f,.6f,-19),9f);
        }
        IEnumerator Ceiling()
        {
            yield return Place(new Vector3(1.45f,3.2f,4.6f),"upper-roof-rejection");float foot=w.PlayerPosition.y;
            yield return Push("roof-normal-jump",Vector3.zero,1.3f,true);Check("roof.normal-jump-cannot-access",report.routes.Last().highestY>foot+.5f&&report.routes.Last().highestY+controller.height<6.4f,"Normal Jump apex remains below the roof.","NATIVE_PHYSICS");
            yield return Push("roof-upward-capsule-probe",Vector3.up,.8f,false,true);
            var probe=report.routes.Last();Check("roof.ceiling-solid-when-cutaway",probe.above>0&&w.PlayerPosition.y+controller.height<=6.46f,"Direct upward actual controller collision; does not infer a ceiling from a low jump.","NATIVE_PHYSICS_PROBE");
            yield return PanicRoofAccessProbeR6.Run(w,
                row=>Check(row.id,row.passed,JsonUtility.ToJson(row),row.layer),
                (kind,position)=>{report.setupWarps++;FixtureNote(kind,"Roof R6 fixture at "+position+"; setup/probe is not a reached route.");});
            yield return Place(new Vector3(0,0,-9),"roof-exterior-capture");
            yield return Photo("10-roof-inaccessible-exterior",new Vector3(16,16,-18),new Vector3(0,3,0),13f);
        }
        string Stock(PanicExpedition e=null)
        {e=e??w.Expedition;return string.Join("|",PanicExpedition.MaterialIds.Select(id=>id+":"+e.Count(id)));}
        IEnumerator TurnCurrent(float total,bool held=true)
        {
            int cap=80;while(w.WorkActive&&total>.001f&&cap-->0)
            {float step=Mathf.Min(30,total);w.Expedition.Work.ApplyRotation(step*w.Expedition.Work.ExpectedDirection,held);w.StepWork(Dt,0,false,false,false);report.workApiSamples++;total-=step;yield return null;}
        }
        IEnumerator DriveWork()
        {
            float end=Time.realtimeSinceStartup+4f;
            while(w.WorkActive&&Time.realtimeSinceStartup<end)
            {
                if(w.Expedition.Work.UsesHammer){w.StepWork(Dt,0,false,false,true);report.workApiSamples++;yield return null;yield return null;}
                else{int step=w.Expedition.Work.NextClickTarget;w.ClickWorkTarget(step);yield return TurnCurrent(w.Expedition.Work.RequiredDegrees);}
            }
        }
        IEnumerator Work()
        {
            w.InputLocked=false;yield return Place(new Vector3(5.5f,0,4.05f),"work-fridge-approach","fridge");
            foreach(var source in w.Expedition.Sources)if(!source.AutoLooted){w.Expedition.ToggleSource(source.Id);FixtureNote("FINITE_SOURCE_SETUP","Expedition.ToggleSource("+source.Id+") for exact recipes; no inventory/material grant.");}
            var item=w.Model.Items.Find(i=>i.Id=="fridge");int budget=w.Model.Budget;string stock=Stock();
            w.BeginWork("anchorwall","fridge");yield return null;Check("work.real-anchor-started",w.WorkActive&&w.Expedition.Work.TotalSteps==2,w.Model.Message);
            if(!w.WorkActive)yield break;
            float idle=w.Expedition.Work.Progress;for(int k=0;k<8;k++){w.StepWork(Dt,1,true,true,true);report.workApiSamples++;yield return null;}
            Check("work.legacy-held-input-no-progress",Mathf.Abs(w.Expedition.Work.Progress-idle)<.0001f);
            Check("work.first-real-click-target",w.ClickWorkTarget(0)&&w.WorkTargetViewport(0).z>0&&hud.WorkClickTargetBounds(0).width>0);
            yield return TurnCurrent(60,false);Check("work.released-mouse-no-progress",Mathf.Abs(w.Expedition.Work.Progress-idle)<.0001f);
            yield return new WaitForEndOfFrame();
            bool gripReady=w.ManualWorkCenterV414(0,out Vector2 screwGrip);float untouched=w.Expedition.Work.Progress;
            Vector2 screwHold=screwGrip+Vector2.right*28f;w.ProcessManualWorkPointerV414(screwHold,true,true);report.workApiSamples++;
            Check("work.pointer-press-selects-only",gripReady&&Mathf.Abs(w.Expedition.Work.Progress-untouched)<.0001f);
            w.ProcessManualWorkPointerV414(screwGrip+new Vector2(Mathf.Cos(Mathf.PI/3),Mathf.Sin(Mathf.PI/3))*28f,false,true);w.StepWork(Dt,0,false,false,false);report.workApiSamples++;yield return null;
            float turned=w.Expedition.Work.Progress;
            var actualRig=Field<GameObject>(w,"workRig");var actualHead=actualRig?actualRig.transform.Find("Fastener_0/TurningHead"):null;
            Check("work.pointer-real-screw-and-tool-motion",turned>untouched&&actualHead&&Quaternion.Angle(actualHead.localRotation,Quaternion.Euler(0,0,-w.Expedition.Work.StageProgress*270f))<.1f);
            w.ProcessManualWorkPointerV414(screwHold,false,false);w.StepWork(Dt,0,false,false,false);report.workApiSamples++;yield return null;
            Check("work.pointer-release-preserves-progress",Mathf.Abs(w.Expedition.Work.Progress-turned)<.0001f);
            w.Expedition.Work.ApplyRotation(-30,true);w.StepWork(Dt,0,false,false,false);yield return null;
            Check("work.reposition-preserves-progress",Mathf.Abs(w.Expedition.Work.Progress-turned)<.0001f&&turned>0);
            yield return TurnCurrent(240);w.ClickWorkTarget(1);yield return TurnCurrent(240);
            Check("work.nearly-complete-unspent",w.WorkActive&&w.Expedition.Work.Progress>.93f&&stock==Stock()&&budget==w.Model.Budget&&!item.Anchored);
            yield return Shot("11-manual-anchor-near-complete");w.CancelWork();yield return null;
            Check("work.cancel-near-complete-free",!w.WorkActive&&stock==Stock()&&budget==w.Model.Budget&&!item.Anchored);
            var costs=PanicExpedition.MaterialCosts("anchorwall",item).ToArray();var owned=costs.Select(c=>w.Expedition.Count(c.Id)).ToArray();int price=PanicExpedition.WorkCost("anchorwall",item);
            w.BeginWork("anchorwall","fridge");yield return null;yield return DriveWork();
            Check("work.atomic-exact-costs",!w.WorkActive&&item.Anchored&&w.Model.Budget==budget-price&&costs.Select((c,k)=>w.Expedition.Count(c.Id)==owned[k]-c.Required).All(v=>v));
            stock=Stock();budget=w.Model.Budget;w.CompleteWork();w.BeginWork("anchorwall","fridge");yield return null;
            Check("work.anchor-once",!w.WorkActive&&stock==Stock()&&budget==w.Model.Budget);
            yield return Place(new Vector3(-2.6f,0,4.8f),"hammer-wall-approach","wall_north");
            var wall=w.Model.Items.Find(i=>i.Id=="wall_north");PanicFloorWallUpgradePlanV414 floorPlan;
            bool floorQuoted=w.Expedition.TryGetFloorWallUpgradeV414(wall.Id,out floorPlan);
            Check("work.floor-quote",floorQuoted&&floorPlan.Floor==0&&floorPlan.WallCount==4&&floorPlan.ChangedWallCount==4&&floorPlan.TargetTier==1&&floorPlan.BudgetCost==480,"Actual selected Model wall floor; existing four real T0 wall recipes, not a one-wall discount.");
            var floorIds=floorQuoted?floorPlan.WallIds.ToArray():new string[0];int floorTarget=floorQuoted?floorPlan.TargetTier:0;
            var floorMembers=w.Model.Items.Where(i=>floorIds.Contains(i.Id)).ToArray();var allTierBefore=w.Model.Items.ToDictionary(i=>i.Id,i=>i.Tier);
            price=w.Expedition.WorkBudgetCostV414("upgrade",wall.Id);costs=w.Expedition.WorkMaterialCostsV414("upgrade",wall.Id).ToArray();owned=costs.Select(c=>w.Expedition.Count(c.Id)).ToArray();budget=w.Model.Budget;
            w.BeginWork("upgrade","wall_north");yield return null;Check("work.hammer-started",w.WorkActive&&w.Expedition.Work.UsesHammer,w.Model.Message);
            if(w.WorkActive)
            {
                for(int k=0;k<5;k++){w.StepWork(Dt,0,true,true,false);report.workApiSamples++;yield return null;}
                Check("work.holding-space-not-a-press",w.Expedition.Work.CompletedStrokes==0);
                for(int k=0;k<3;k++){w.StepWork(Dt,0,false,false,true);report.workApiSamples++;yield return null;yield return null;}
                Check("work.three-separate-presses-incomplete",w.WorkActive&&w.Expedition.Work.CompletedStrokes==3&&w.Model.Items.All(i=>i.Tier==allTierBefore[i.Id])&&w.Model.Budget==budget&&costs.Select((c,k)=>w.Expedition.Count(c.Id)==owned[k]).All(v=>v));
                yield return Shot("12-hammer-three-presses");string floorCancelStock=Stock();w.CancelWork();yield return null;
                Check("work.floor-near-complete-cancel-free",!w.WorkActive&&Stock()==floorCancelStock&&w.Model.Budget==budget&&w.Model.Items.All(i=>i.Tier==allTierBefore[i.Id])&&w.Expedition.ActiveFloorWallUpgradeV414==null);
                w.BeginWork("upgrade","wall_north");yield return null;Check("work.floor-restart-same-full-cost",w.WorkActive&&w.Expedition.ActiveFloorWallUpgradeV414!=null&&w.Expedition.ActiveFloorWallUpgradeV414.BudgetCost==price);
                for(int k=0;k<3;k++){w.StepWork(Dt,0,false,false,true);report.workApiSamples++;yield return null;yield return null;}
                w.StepWork(Dt,0,false,false,true);report.workApiSamples++;yield return null;
                Check("work.fourth-press-atomic",!w.WorkActive&&floorMembers.Length==4&&floorMembers.All(i=>i.Tier==Math.Max(allTierBefore[i.Id],floorTarget))&&w.Model.Items.Where(i=>!floorIds.Contains(i.Id)).All(i=>i.Tier==allTierBefore[i.Id])&&w.Model.Budget==budget-price&&costs.Select((c,k)=>w.Expedition.Count(c.Id)==owned[k]-c.Required).All(v=>v));
                string floorCommittedStock=Stock();int floorCommittedBudget=w.Model.Budget;w.CompleteWork();
                Check("work.floor-double-complete-noop",Stock()==floorCommittedStock&&w.Model.Budget==floorCommittedBudget&&w.Expedition.ActiveFloorWallUpgradeV414==null);
                Check("work.floor-next-tier-not-free",w.Expedition.WorkBudgetCostV414("upgrade",wall.Id)==720&&!w.Expedition.CanBeginWork("upgrade",wall.Id)&&Stock()==floorCommittedStock&&w.Model.Budget==floorCommittedBudget,"Next full-floor T2 costs720 budget/8planks/12screws; no stock grant or discounted retry.");
            }
            Check("work.normal-cost-route-not-cheated",!w.Model.CheatedRun&&!w.Model.DeveloperAnyEnabled);
            hud.ToggleDeveloperMenu();yield return null;bool menu=hud.DeveloperMenuOpen;hud.ToggleDeveloperMenu();yield return null;Check("developer.menu-alone-no-cheat",menu&&!hud.DeveloperMenuOpen&&!w.Model.CheatedRun);
        }
        bool UtilityHandlePoseR6(GameObject root,string action,float progress)
        {
            var handle=root?root.transform.Find("ValveMotion"):null;if(!handle)return false;
            return action=="poweroff"?Mathf.Abs(handle.localPosition.y-(.74f-.26f*progress))<.0001f&&Quaternion.Angle(handle.localRotation,Quaternion.identity)<.1f:
                Quaternion.Angle(handle.localRotation,Quaternion.Euler(0,0,90f*(1-progress)))<.1f;
        }
        IEnumerator DriveUtilityPointerR6(string action,float fraction)
        {
            yield return new WaitForEndOfFrame();
            bool ready=w.ManualWorkCenterV414(0,out Vector2 grip)&&w.ManualWorkPivotV414(out Vector2 pivot);
            Check("utility.pointer-ready."+action,ready);if(!ready)yield break;
            w.ManualWorkPivotV414(out Vector2 origin);w.ManualWorkClosedGripV414(out Vector2 closed);
            float before=w.Expedition.Work.Progress;w.ProcessManualWorkPointerV414(grip,true,true);report.workApiSamples++;
            Check("utility.pointer-selection-no-progress."+action,Mathf.Abs(w.Expedition.Work.Progress-before)<.0001f);
            Vector2 radial=grip-origin;float initial=Mathf.Atan2(radial.y,radial.x);int steps=Mathf.Max(1,Mathf.CeilToInt(90f*fraction/15f));Vector2 pointer=grip;
            for(int k=1;k<=steps&&w.WorkActive;k++)
            {
                float part=fraction*k/steps;
                pointer=action=="poweroff"?Vector2.Lerp(grip,closed,part):origin+new Vector2(Mathf.Cos(initial+part*Mathf.PI*.5f),Mathf.Sin(initial+part*Mathf.PI*.5f))*radial.magnitude;
                w.ProcessManualWorkPointerV414(pointer,false,true);w.StepWork(Dt,0,false,false,false);report.workApiSamples++;yield return null;
            }
            w.ProcessManualWorkPointerV414(pointer,false,false);report.workApiSamples++;
        }
        IEnumerator Utilities()
        {
            foreach(string action in new[]{"gasoff","wateroff","poweroff"})
            {
                string id=PanicUtilitySafetyV414.TargetForAction(action);Vector3 station;
                bool positioned=w.TryUtilityStationPositionV414(id,out station);Check("utility.world-station."+action,positioned);
                var map=Field<Dictionary<string,GameObject>>(w,"residenceTargets");GameObject root=map!=null&&map.ContainsKey(id)?map[id]:null;
                Check("utility.real-pick-solid-open-pose."+action,root&&root.GetComponent<HomePick>()&&root.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled&&!c.isTrigger)&&!w.UtilityStationClosedV414(id)&&UtilityHandlePoseR6(root,action,0));
                if(!positioned)continue;Vector3 approach=station+(action=="poweroff"?Vector3.back:Vector3.left)*1.1f;
                yield return Place(approach,"utility-"+action,id);int budget=w.Model.Budget;string stock=Stock();w.HandleKey(KeyCode.E);yield return null;
                Check("utility.e-start-and-appropriate-control."+action,w.WorkActive&&w.Expedition.Work.IsUtility&&!Field<GameObject>(w,"handTool")&&w.Expedition.Work.RequiredDegrees==90,w.Model.Message);
                if(!w.WorkActive)continue;
                bool linear=action=="poweroff";
                Check("utility.distinct-control-mode."+action,w.Expedition.Work.UsesLinearDrag==linear&&w.Expedition.Work.UsesRotation!=linear&&!w.Expedition.Work.UsesHammer);
                w.ClickWorkTarget(0);
                bool wrongAdapter=linear?w.Expedition.Work.ApplyRotation(-45,true):w.Expedition.Work.ApplyLinearDisplacement(.5f,true);
                Check("utility.wrong-adapter-no-progress."+action,!wrongAdapter&&w.Expedition.Work.Progress==0);
                for(int k=0;k<3;k++){w.StepWork(Dt,1,true,true,false);report.workApiSamples++;yield return null;}
                Check("utility.idle-no-motion-or-commit."+action,w.Expedition.Work.Progress==0&&!w.UtilityStationClosedV414(id)&&budget==w.Model.Budget&&stock==Stock());
                yield return DriveUtilityPointerR6(action,5f/6f);
                var rig=Field<GameObject>(w,"workRig");
                Check("utility.actual-partial-pose-uncommitted."+action,w.WorkActive&&w.Expedition.Work.Progress>.7f&&w.Expedition.Work.Progress<.99f&&UtilityHandlePoseR6(rig,action,w.Expedition.Work.Progress)&&!w.UtilityStationClosedV414(id)&&budget==w.Model.Budget&&stock==Stock());
                float retained=w.Expedition.Work.Progress;w.StepWork(Dt,0,false,false,false);yield return null;
                Check("utility.release-stops."+action,Mathf.Abs(retained-w.Expedition.Work.Progress)<.0001f);
                yield return Shot("13-utility-"+action+"-partial");w.CancelWork();yield return null;
                Check("utility.cancel-free-world-open."+action,!w.UtilityStationClosedV414(id)&&UtilityHandlePoseR6(map[id],action,0)&&budget==w.Model.Budget&&stock==Stock());
                w.HandleKey(KeyCode.E);yield return null;yield return DriveUtilityPointerR6(action,1);
                Check("utility.real-model-committed."+action,!w.WorkActive&&w.Model.UtilitySafety.IsComplete(action)&&w.UtilityStationClosedV414(id)&&w.Expedition.IsWorkCommitted(action,id)&&UtilityHandlePoseR6(map[id],action,1)&&budget==w.Model.Budget&&stock==Stock());
                w.HandleKey(KeyCode.E);w.BeginWork(action,id);yield return null;
                Check("utility.already-safe-no-repeat."+action,!w.WorkActive&&budget==w.Model.Budget&&stock==Stock());
                yield return Shot("13-utility-"+action);
            }
        }
        IEnumerator Feedback()
        {
            // New normal run restores finite unopened containers; all setup counters remain in this receipt.
            w.ReturnToMenu();w.StartGame(false);report.startGameCalls++;w.FullFlowScriptedInput=true;yield return null;
            var skip=w.GetType().GetMethod("SkipIntroStoryV414",Instance);if(skip!=null)skip.Invoke(w,null);else if(w.CinematicActive)w.SkipResidenceCinematic();yield return null;
            yield return Place(new Vector3(4.65f,0,3.55f),"real-gather-popup","cabinet");
            bool seams=Require(w,"GatherFeedbackCountV414","feedback.count-seam")&Require(w,"GatherFeedbackItemIdsV414","feedback.ids-seam")&Require(w,"GatherFeedbackAmountsV414","feedback.amounts-seam")&Require(w,"GatherFeedbackVisibleV414","feedback.visible-seam");
            int sequence=w.Expedition.GatherSequence,count=Int(w,"GatherFeedbackCountV414");var before=PanicExpedition.MaterialIds.ToDictionary(id=>id,id=>w.Expedition.Count(id));
            w.HandleKey(KeyCode.E);yield return null;
            var rewards=w.Expedition.LastGatherRewards.ToArray();var ids=Read(w,"GatherFeedbackItemIdsV414") as string[];var amounts=Read(w,"GatherFeedbackAmountsV414") as int[];
            bool exact=ids!=null&&amounts!=null&&ids.Length==amounts.Length&&rewards.Length==ids.Length&&rewards.All(r=>{int i=Array.IndexOf(ids,r.Id);return i>=0&&amounts[i]==r.Required&&w.Expedition.Count(r.Id)-before[r.Id]==r.Required;});
            Check("feedback.real-container-reward",w.Expedition.GatherSequence==sequence+1&&rewards.Length>0&&w.Expedition.Sources.Find(s=>s.Id=="cabinet").AutoLooted);
            Check("feedback.actual-visible-icon-count-payload",seams&&Bool(w,"GatherFeedbackVisibleV414")&&Int(w,"GatherFeedbackCountV414")>count&&exact,"Payload must equal actual finite stock deltas, not authored nominal counts.");
            yield return Wait(.12f);yield return Shot("14-gather-actual-plus-counts");
            var previewMethod=w.GetType().GetMethod("GatherFeedbackAssetTextureV414",Instance);
            Check("feedback.actual-material-render-assets",previewMethod!=null&&rewards.Length>0&&rewards.All(r=>previewMethod.Invoke(w,new object[]{r.Id}) is RenderTexture rt&&rt.IsCreated()&&rt.width>=96)&&Int(w,"GatherFeedbackActualRenderCountV414")>=rewards.Length,
                "Actual runtime cached mesh preview render textures; no QA-created substitute icon.","NATIVE_RENDER_STATE");int after=Int(w,"GatherFeedbackCountV414");string stock=Stock();w.ToggleContainer();w.ToggleContainer();yield return null;
            Check("feedback.no-reopen-reroll",w.Expedition.GatherSequence==sequence+1&&Int(w,"GatherFeedbackCountV414")==after&&stock==Stock());
        }
        void Supplies(bool visible,string label)
        {
            var roots=Field<Dictionary<string,GameObject>>(w,"worldSupplyObjects");
            Check("supplies.visibility."+label,roots!=null&&roots.Count>0&&roots.All(pair=>{var supply=w.Expedition.FindWorldSupply(pair.Key);bool expected=visible&&supply!=null&&!supply.Taken;var go=pair.Value;return go&&go.activeInHierarchy==expected&&go.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled&&c.gameObject.activeInHierarchy)==expected&&go.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled&&r.gameObject.activeInHierarchy)==expected;}),"Actual object/renderer/collider states.","NATIVE_RENDER_STATE");
        }
        void Aim(Vector3 target)
        {
            Vector3 d=(target-w.View.transform.position).normalized;
            float yaw=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,pitch=-Mathf.Asin(d.y)*Mathf.Rad2Deg;
            float gain=Mathf.Clamp(w.FpsLookMultiplierV414,.01f,1f);
            w.StepBagLook(new Vector2(Mathf.DeltaAngle(Field<float>(w,"bagYaw"),yaw)/(2*gain),(Field<float>(w,"bagPitch")-pitch)/(1.7f*gain)));report.bagLookApiSamples++;Call(w,"RefreshBagRayFocus");
        }
        string RayDetail(string expected,Vector3 target)
        {
            Physics.SyncTransforms();var hits=Physics.RaycastAll(w.View.ViewportPointToRay(new Vector3(.5f,.5f)),2.4f,~((1<<28)|(1<<30)),QueryTriggerInteraction.Collide)
                .Where(h=>h.collider!=controller&&!h.transform.IsChildOf(controller.transform)).OrderBy(h=>h.distance).ToArray();
            return "expected="+expected+" actualFocus="+Field<string>(w,"focusId")+" actor="+w.PlayerPosition+" camera="+w.View.transform.position+" targetCenter="+target+" direction="+w.View.transform.forward+" CanInteract="+w.CanInteract+" interactionDistance="+w.InteractionDistance(expected)+" torsoPathDiagnostic="+Call(w,"InteractionPathClear",expected,target)+" ray="+string.Join(";",hits.Select(h=>h.collider.name+":"+(h.collider.GetComponentInParent<HomePick>()?.Id??"-")+":"+h.distance.ToString("F4")));
        }
        IEnumerator AimForRay(Vector3 target,string expected,string label)
        {
            FixtureNote("FPS_LOOK_API",label+"; production StepBagLook with actual damping gain, real-frame settling toward actual collider center; no camera pose write, SetFocus or alternate ray.");
            float end=Time.realtimeSinceStartup+1.25f;
            do {Aim(target);yield return null;Call(w,"RefreshBagRayFocus");if(Field<string>(w,"focusId")==expected&&w.CanInteract)break;}while(Time.realtimeSinceStartup<end&&w.BagFirstPersonActive);
            Check("bag.actual-center-ray-focus."+label,Field<string>(w,"focusId")==expected&&w.CanInteract,RayDetail(expected,target),"NATIVE_PRODUCTION_RAY");
        }
        IEnumerator SettleBag()
        {float end=Time.realtimeSinceStartup+3;while((!w.BagFirstPersonActive||w.BagCameraTransitionActive)&&Time.realtimeSinceStartup<end)yield return null;Check("bag.actual-camera-settled",w.BagFirstPersonActive&&!w.BagCameraTransitionActive);}
        IEnumerator Bag()
        {
            Check("normal.clock.unscaled-real-frame-setup",!w.InputLocked&&!w.Model.IsPaused&&!w.Model.DeveloperAnyEnabled&&Mathf.Abs(Time.timeScale-1)<.001f,"No clock freeze, developer option or timescale acceleration during this interval.");
            float clockBefore=w.Model.TimeRemaining,wallBefore=Time.realtimeSinceStartup,heartbeat=wallBefore;
            FixtureNote("REAL_CLOCK_INTERVAL","Allow normal production Model.Tick from remaining="+clockBefore+" down to46.05 using actual frames; no direct Tick/time mutation in this interval.");
            while(w.Model.Phase==PanicPhase.Preparing&&w.Model.TimeRemaining>46.05f&&Time.realtimeSinceStartup-wallBefore<126f)
            {
                if(Time.realtimeSinceStartup-heartbeat>=15){heartbeat=Time.realtimeSinceStartup;Observe("normal-clock-wait");Debug.Log("OVERHAUL414_CLOCK remaining="+w.Model.TimeRemaining.ToString("F2"));}
                yield return null;
            }
            float realSeconds=Time.realtimeSinceStartup-wallBefore,modelSeconds=clockBefore-w.Model.TimeRemaining;
            Check("normal.clock.actual-precaution-duration",w.Model.Phase==PanicPhase.Preparing&&w.Model.TimeRemaining<=46.05f&&w.Model.TimeRemaining>45.5f&&Mathf.Abs(realSeconds-modelSeconds)<1.5f&&clockBefore>160f,
                "Real frame interval="+realSeconds.ToString("F2")+" model decrement="+modelSeconds.ToString("F2")+" start="+clockBefore+" end="+w.Model.TimeRemaining);
            Call(w,"RefreshPreparationVisibility");Supplies(false,"precautions");
            int water=w.Expedition.Count("water");w.SetFocus("supply_water_kitchen");Check("bag.early-pickup-denied",!w.TakeWorldSupply()&&w.Expedition.Count("water")==water&&!w.BeginBagPreparation());
            yield return Place(new Vector3(4.65f,0,3.55f),"bag-same-position-transition");Vector3 actor=w.PlayerPosition;var original=controller;
            w.TickPreparationStages();
            Check("bag.actual-46-still-precautions",!w.BagFinalStageActive&&!w.BagCameraOwnsView&&!w.BeginBagPreparation());
            hud.SetPaused(true);w.Model.TimeRemaining=45;report.explicitTimerFixtures++;FixtureNote("TIMER_BOUNDARY","Paused TimeRemaining=45; exact boundary fixture.");w.TickPreparationStages();yield return Wait(.15f);
            Check("bag.pause-blocks-entry",!w.BagCameraOwnsView&&Mathf.Abs(w.Model.TimeRemaining-45)<.001f);hud.SetPaused(false);w.TickPreparationStages();yield return SettleBag();
            Check("bag.same-controller-and-position",controller==original&&controller.enabled&&Vector3.Distance(actor,w.PlayerPosition)<.005f&&!w.View.orthographic);
            yield return Wait(.12f);
            bool stressSeams=Require(w,"FpsStressIntensityV414","fps.stress-intensity-seam")&Require(w,"FpsVeilOpacityV414","fps.veil-opacity-seam")&Require(w,"FpsFeedbackAudioPlayingV414","fps.audio-state-seam")&Require(w,"FpsStressClockV414","fps.stress-clock-seam");
            Check("fps.actual-stress-finite-and-bounded",stressSeams&&Finite(w.FpsStressIntensityV414)&&w.FpsStressIntensityV414>=0&&w.FpsStressIntensityV414<=1&&Finite(w.FpsVeilOpacityV414)&&w.FpsVeilOpacityV414>=.16f&&w.FpsVeilOpacityV414<=.2201f&&Finite(w.FpsMovementMultiplierV414)&&w.FpsMovementMultiplierV414>=.94f&&w.FpsMovementMultiplierV414<=1&&Finite(w.FpsLookMultiplierV414)&&w.FpsLookMultiplierV414>=.94f&&w.FpsLookMultiplierV414<=1&&w.FpsVeilTextureV414&&w.FpsVeilTextureV414.width>=96&&w.FpsStressVisibleV414,"Actual final45 production stress/veil/texture/damping state. This does not establish human readability or comfort.","NATIVE_API_FRAME");
            Check("fps.actual-local-audio-playing",w.FpsFeedbackAudioPlayingV414,"Actual local heartbeat/breath AudioSource.isPlaying; no listening acceptance.","NATIVE_AUDIO_STATE");
            FixtureNote("FPS_HUD_PAUSE_API","Production HUD.SetPaused(true/false), real .25s frame wait. No OS Escape and no live timer assignment in this pause case.");
            hud.SetPaused(true);yield return null;float pausedTimer=w.Model.TimeRemaining,pausedStress=w.FpsStressClockV414;yield return Wait(.25f);
            Check("fps.pause-clock-veil-audio-stop",w.Model.IsPaused&&Mathf.Abs(w.Model.TimeRemaining-pausedTimer)<.001f&&Mathf.Abs(w.FpsStressClockV414-pausedStress)<.001f&&!w.FpsFeedbackAudioPlayingV414&&!w.FpsStressVisibleV414&&w.FpsVeilOpacityV414==0,"Paused production clock/stress clock remain fixed and local AudioSources stop.","NATIVE_AUDIO_STATE");
            hud.SetPaused(false);yield return Wait(.12f);
            Check("fps.resume-clock-veil-audio",!w.Model.IsPaused&&w.Model.TimeRemaining<pausedTimer-.04f&&w.FpsStressClockV414>pausedStress&&w.FpsStressVisibleV414&&w.FpsFeedbackAudioPlayingV414,"Normal final45 resumes through the same controller/view; no second phase wait.","NATIVE_AUDIO_STATE");
            w.HandleKey(KeyCode.G);Check("bag.no-return-to-precautions",w.BagFirstPersonActive&&!w.FinishBagPreparation());Call(w,"RefreshPreparationVisibility");Supplies(true,"final45");
            Check("bag.containers-auto-closed",w.Expedition.Sources.All(s=>!s.Open));
            foreach(string item in new[]{"water","food"})
            {
                var supply=w.Expedition.WorldSupplies.FirstOrDefault(s=>s.ItemId==item&&!s.Taken);
                Check("bag.real-finite-supply-exists."+item,supply!=null);if(supply==null)continue;
                Vector3 ground=new Vector3(supply.Position.x,supply.Floor*3.2f,supply.Position.z)-Vector3.forward*.95f;
                yield return Place(ground,"fps-pickup-"+item);
                var supplyRoots=Field<Dictionary<string,GameObject>>(w,"worldSupplyObjects");GameObject supplyRoot=supplyRoots!=null&&supplyRoots.TryGetValue(supply.Id,out var actualSupply)?actualSupply:null;
                var pickCollider=supplyRoot?supplyRoot.GetComponent<Collider>():null;
                Check("bag.actual-pick-collider."+item,pickCollider&&pickCollider.enabled&&pickCollider.gameObject.activeInHierarchy,"Actual stable production picking collider, no QA-created substitute.");
                Vector3 targetCenter=pickCollider?pickCollider.bounds.center:supply.Position;yield return AimForRay(targetCenter,supply.Id,item);
                int amount=supply.Count,beforeLoose=w.Expedition.Count(item);string rayBefore=RayDetail(supply.Id,targetCenter);w.HandleKey(KeyCode.E);yield return null;
                int packed=item=="water"?w.Model.PackedWater:w.Model.PackedFood;
                Check("bag.real-ray-E-autopack."+item,supply.Taken&&packed==1&&w.Expedition.Count(item)==beforeLoose+amount-1,"Finite stock consumed; first required unit to real bag, remaining to inventory. "+rayBefore+" packed="+packed+" looseBefore="+beforeLoose+" looseAfter="+w.Expedition.Count(item)+" amount="+amount+" taken="+supply.Taken);
            }
            Check("bag.ready-one-water-one-food",w.Model.BagReady&&w.Expedition.Bag.Where(s=>s.Id=="water").Sum(s=>s.Count)==1&&w.Expedition.Bag.Where(s=>s.Id=="food").Sum(s=>s.Count)==1);
            var hands=Field<GameObject>(w,"bagHands");var handCamera=Field<Camera>(w,"bagHandsCamera");
            Check("hands.new-motor-viewmodel",hands&&hands.name.Contains("V414")&&handCamera&&handCamera.enabled&&hands.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled&&r.gameObject.activeInHierarchy));
            if(hands)
            {
                var joints=hands.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="PIP"||t.name=="DIP"||t.name.EndsWith("_MCP")).ToArray();Check("hands.30-articulated-finger-joints",joints.Length==30);
                yield return new WaitForEndOfFrame();var before=Pose(joints);PanicBagHandsArt.Animate(hands,0,0,.43f,0);var contact=Pose(joints);
                Check("hands.production-grip-changes-joints",Changed(before,contact)>=8,"Explicit production Animate(gather=.43) native pose fixture, not an OS grip claim.");
                FixtureNote("HANDS_POSE_API","Production Animate on live viewmodel at .43 contact; actual pickup above separately uses real finite world supply.");
                var loco=typeof(PanicBagHandsArt).GetMethod("SetLocomotion",Static);Check("hands.locomotion-motor-api",loco!=null);
                if(loco!=null)
                {
                    Transform[] moving=hands.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="HandsBob"||t.name=="Hand_L"||t.name=="Hand_R").ToArray();
                    loco.Invoke(null,new object[]{hands,PanicHomeWorld.WalkSpeed,true,0f,.06f});PanicBagHandsArt.Animate(hands,.12f,.65f,0,0);var walk=moving.Select(t=>t.localPosition).ToArray();
                    loco.Invoke(null,new object[]{hands,PanicHomeWorld.RunSpeed,true,0f,.06f});PanicBagHandsArt.Animate(hands,.28f,1,0,0);var run=moving.Select(t=>t.localPosition).ToArray();
                    loco.Invoke(null,new object[]{hands,PanicHomeWorld.RunSpeed,false,4.5f,.06f});PanicBagHandsArt.Animate(hands,.28f,1,0,0);var jump=moving.Select(t=>t.localPosition).ToArray();
                    Check("hands.run-and-jump-motor-poses",moving.Length==3&&walk.Zip(run,(a,b)=>Vector3.Distance(a,b)>.004f).Any(v=>v)&&run.Zip(jump,(a,b)=>Vector3.Distance(a,b)>.006f).Any(v=>v),"Production hand motor APIs on live transforms; explicitly injected speed/airborne pose fixture.");
                    FixtureNote("HANDS_MOTOR_POSE_API","WalkSpeed/RunSpeed/airborne SetLocomotion + Animate on live hands only; not OS/controller run acceptance.");
                }
            }
            yield return Shot("15-fps-kitchen-hands");
            yield return Place(new Vector3(3.35f,0,-4.0f),"fps-wc-view");Aim(new Vector3(2.8f,1.0f,-5.2f));yield return Shot("16-fps-wc");
            yield return Place(new Vector3(6.3f,0,-4.0f),"fps-pantry-view");Aim(new Vector3(6.4f,1,-5.3f));yield return Shot("17-fps-pantry");
            int budget=w.Model.Budget;string stock=Stock();w.BeginWork("upgrade","wall_south");yield return null;Check("bag.precaution-work-denied",!w.WorkActive&&budget==w.Model.Budget&&stock==Stock());
        }
        IEnumerator Bunker()
        {
            bool seams=Require(w,"BunkerSequenceStageV414","bunker.stage-seam")&Require(w,"BunkerBagReleasedV414","bunker.release-seam")&Require(w,"BunkerSequenceResolutionCountV414","bunker.once-seam");
            var hatch=w.Model.Items.Find(i=>i.Id=="bunker");var itemRoots=Field<Dictionary<string,GameObject>>(w,"items");GameObject hatchRoot=itemRoots!=null&&itemRoots.ContainsKey("bunker")?itemRoots["bunker"]:null;
            Check("bunker.actual-yarkin-lid-intake",hatchRoot&&hatchRoot.GetComponentsInChildren<Component>(true).Any(c=>c&&c.GetType().Name=="PanicCuratedBunkerLid")&&hatchRoot.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="ActualYarkinBunkerLid_V414"));
            Check("bunker.open-transition-ring",hatchRoot&&hatchRoot.GetComponentsInChildren<MeshFilter>(true).Any(f=>f.sharedMesh&&f.sharedMesh.name=="V414_Bunker_OpenSquareToCircleRing"),"Actual ring mesh, not a filled centre collider.");
            yield return Place(PanicHomeWorld.BunkerPosition+Vector3.back*1.65f,"real-fps-bunker-entry");yield return AimForRay(PanicHomeWorld.BunkerPosition+Vector3.up*.14f,"bunker","bunker-entry");
            int before=Int(w,"BunkerSequenceResolutionCountV414");w.HandleKey(KeyCode.E);yield return null;Check("bunker.real-fps-E-entry",w.Model.Phase==PanicPhase.Disaster&&!w.BagCameraOwnsView,w.ContextHint);
            Check("fps.exit-cinematic-stops-audio",w.Model.Phase==PanicPhase.Disaster&&!w.BagCameraOwnsView&&!w.FpsFeedbackAudioPlayingV414&&!w.FpsStressVisibleV414&&w.FpsVeilOpacityV414==0,"Actual FPS E shelter transition invokes production ResetBagPreparation/SuspendExperienceFeedback; no listening claim.","NATIVE_AUDIO_STATE");
            var stages=new List<string>();bool releaseBefore=false;int releases=0;float maxBagMotion=0,maxActorMotion=0;Vector3? firstBag=null;Vector3 firstActor=w.PlayerPosition;
            bool openBeforeThrow=false,lidDuringJump=true,closed=false,bagRendered=false,openCapture=false,airborneCapture=false;
            var head=PanicCharacterArt.GetCharacterBone(hero.gameObject,Field<Animator>(w,"animator"),HumanBodyBones.Head);int jumpFrames=0;float end=Time.realtimeSinceStartup+6.5f;
            while(w.Model.Phase==PanicPhase.Disaster&&Time.realtimeSinceStartup<end)
            {
                string stage=Text(w,"BunkerSequenceStageV414").ToLowerInvariant();bool release=Bool(w,"BunkerBagReleasedV414");if(release&&!releaseBefore)releases++;releaseBefore=release;
                var bag=Read(w,"BunkerBagObjectV414") as GameObject;
                if(bag){if(!firstBag.HasValue)firstBag=bag.transform.position;maxBagMotion=Mathf.Max(maxBagMotion,Vector3.Distance(firstBag.Value,bag.transform.position));bagRendered|=bag.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled&&r.gameObject.activeInHierarchy);}
                maxActorMotion=Mathf.Max(maxActorMotion,Vector3.Distance(firstActor,w.PlayerPosition));
                if(stage=="open")openBeforeThrow|=LidAngle()>40;
                // Follow actual stage/pose, never a stale timeline cutoff. The opening
                // capture is taken at a genuinely raised lid after the approach finishes.
                if(!openCapture&&(stage=="open"||stage=="bagthrow")&&LidAngle()>85)
                {openCapture=true;yield return Shot("18-bunker-lid-fully-open");}
                if(stage=="jump"){jumpFrames++;lidDuringJump&=head&&(head.position.y<PanicHomeWorld.BunkerPosition.y-.10f||LidAngle()>85);}
                if(!airborneCapture&&stage=="jump"&&Field<float>(w,"residenceCinematicAge")>=3.15f)
                {airborneCapture=true;yield return Shot("18-bunker-airborne");}
                if(stage=="closed")closed|=LidAngle()<8;
                if(stage.Length>0&&!stages.Contains(stage)){stages.Add(stage);yield return Shot("18-bunker-"+stage);}
                Observe("bunker-sequence-sample");yield return null;
            }
            int oi=stages.IndexOf("open"),bi=stages.IndexOf("bagthrow"),ji=stages.IndexOf("jump"),ci=stages.IndexOf("closed");
            Check("bunker.production-stage-order",seams&&oi>=0&&bi>oi&&ji>bi&&ci>ji,string.Join(" -> ",stages));
            Check("bunker.actual-midflight-capture",airborneCapture,"Additional actual jump/descent frame after cinematic age3.15; initial18-bunker-jump remains. Image existence/stage labels do not establish human throw/jump acceptance.","NATIVE_CAPTURE_FILE");
            Check("bunker.lid-open-until-head-clears",openBeforeThrow&&openCapture&&jumpFrames>=2&&lidDuringJump&&closed,"Actual leaf stays open while actual Head is above collar; final close after clearance is permitted.");
            Check("bunker.actual-bag-release-and-motion",firstBag.HasValue&&bagRendered&&releases==1&&maxBagMotion>.25f,"bag root exists/visible/moved="+maxBagMotion+" release edges="+releases);
            Check("bunker.actor-trajectory-and-once",maxActorMotion>.8f&&Int(w,"BunkerSequenceResolutionCountV414")==before+1,"actual actor displacement="+maxActorMotion+" resolution before="+before+" after="+Int(w,"BunkerSequenceResolutionCountV414"));
            CheckRoof("Earthquake");yield return Shot("19-earthquake-exterior-roof");yield return HazardMotion(DisasterType.Earthquake);
        }
        void CheckRoof(string label)
        {var roofs=Roofs();Check("roof.exterior."+label,w.CinematicActive&&roofs.Length>0&&roofs.All(r=>r.enabled&&r.gameObject.activeInHierarchy),"roofs="+roofs.Length+" visible="+roofs.Count(r=>r.enabled&&r.gameObject.activeInHierarchy),"NATIVE_RENDER_STATE");}
        GameObject Weather(){var effects=w.GetComponent<PanicHomeEffects>();return effects?Field<GameObject>(effects,"weather"):null;}
        static string MeshSignature(GameObject root)
        {
            if(!root)return "missing";return string.Join("|",root.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh).Select(f=>{var mesh=f.sharedMesh;var v=mesh.vertices;if(v.Length==0)return mesh.name+":empty";Vector3 sample=v[0]+v[v.Length/3]+v[v.Length-1];return mesh.name+":"+sample.ToString("F5");}));
        }
        static Vector3 QuakeImpulse(GameObject root)
        {var method=typeof(PanicDisasterArt).GetMethod("SampleVisualImpulse",Static);return method==null?Vector3.zero:(Vector3)method.Invoke(null,new object[]{root});}
        IEnumerator HazardMotion(DisasterType type)
        {
            var weather=Weather();Check("hazard.actual-production-root."+type,weather&&weather.activeInHierarchy);if(!weather)yield break;
            int renderers=weather.GetComponentsInChildren<Renderer>(true).Count(r=>r.enabled&&r.gameObject.activeInHierarchy);string before=MeshSignature(weather);
            Vector3 impulse=type==DisasterType.Earthquake?QuakeImpulse(weather):Vector3.zero;yield return Wait(.24f);string after=MeshSignature(weather);
            if(type==DisasterType.Earthquake)Check("hazard.quake-pulsed-impulse",impulse.sqrMagnitude>0||QuakeImpulse(weather).sqrMagnitude>0,"Actual production visual impulse samples; no earthquake physics force claim.");
            else Check("hazard.dynamic-geometry."+type,before!=after,"Native mesh vertices changed across actual frames.","NATIVE_RENDER_GEOMETRY");
            Check("hazard.actual-renderers."+type,renderers>0,"enabled renderers="+renderers);
            if(type==DisasterType.Fire)Check("hazard.fire-tapered-sheet-present",weather.GetComponentsInChildren<MeshFilter>(true).Any(f=>f.sharedMesh&&f.name.Contains("flame sheets")),"Actual attached multi-vertex flame mesh.");
            if(type==DisasterType.Storm)Check("hazard.storm-funnel-present",weather.GetComponents<Component>().Any(c=>c&&c.GetType().Name=="PanicStormMotionV414"));
        }
        IEnumerator Hazards()
        {
            foreach(var type in new[]{DisasterType.Flood,DisasterType.Fire,DisasterType.Storm})
            {
                w.ReturnToMenu();w.StartDisasterSimulation(type);report.simulationStarts++;w.FullFlowScriptedInput=true;
                FixtureNote("SIMULATION_PRESENTATION","Production StartDisasterSimulation("+type+") plus Model.EnterBunker; these three cover rendering/response only, not bag gathering or route acceptance.");
                yield return null;w.Model.EnterBunker();float end=Time.realtimeSinceStartup+5f;
                while(Time.realtimeSinceStartup<end&&w.Model.Phase==PanicPhase.Disaster)yield return null;
                CheckRoof(type.ToString());yield return Shot("20-disaster-"+type.ToString().ToLowerInvariant());yield return HazardMotion(type);
                if(type==DisasterType.Fire)
                {
                    float wait=Time.realtimeSinceStartup+10;while(w.Model.Phase!=PanicPhase.Inspection&&Time.realtimeSinceStartup<wait)yield return null;
                    if(w.Model.Phase==PanicPhase.GameOver)w.Model.InspectSimulationDamage();yield return null;
                    Check("hazard.fire-inspection-no-roof-throughflames",w.Model.Phase==PanicPhase.Inspection&&Weather()==null,"Production inspection stops active whole-house fire art; no force clearing by QA.");yield return Shot("21-fire-cutaway-inspection");
                }
            }
        }
        Component StreetComponent()
        {return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(c=>c&&Read(c,"CivilianCount")!=null&&Read(c,"VehicleCount")!=null&&Read(c,"ResponseActive")!=null);}
        static GameObject[] StreetRoots(Component street,string name)
        {
            var list=Field<IList>(street,name);if(list==null)return Array.Empty<GameObject>();var roots=new List<GameObject>();
            foreach(object item in list){var root=Read(item,"Root") as GameObject;if(root)roots.Add(root);}return roots.ToArray();
        }
        static GameObject[] ResponseRoots(Component street,bool crew)
        {
            var list=Field<IList>(street,"response");if(list==null)return Array.Empty<GameObject>();var roots=new List<GameObject>();
            foreach(object item in list){var root=Read(Read(item,crew?"Crew":"Car"),"Root") as GameObject;if(root)roots.Add(root);}return roots.ToArray();
        }
        void CheckVehicleMeshForward(GameObject[] roots,Dictionary<int,Vector3> before,Vector3 localFront,int expected,string label,string source)
        {
            Check("street."+label+".expected-actual-mesh-roots",roots.Length==expected,"Expected actual moving vehicle roots="+expected+"; declared mesh front comes from authored headlight/bonnet geometry, not root+Z assumption.");
            for(int i=0;i<roots.Length;i++)
            {
                var root=roots[i];Vector3 start=root&&before.ContainsKey(root.GetInstanceID())?before[root.GetInstanceID()]:Vector3.zero;
                Vector3 delta=root?root.transform.position-start:Vector3.zero;delta.y=0;
                Vector3 front=root?root.transform.TransformDirection(localFront):Vector3.zero;front.y=0;front.Normalize();
                float moved=delta.magnitude,dot=moved>.0001f?Vector3.Dot(front,delta/moved):-1;
                report.vehicleDirections.Add(new VehicleDirection{label=label,root=root?root.name:"missing",meshFrontSource=source,frame=Time.frameCount,start=start,end=root?root.transform.position:Vector3.zero,authoredLocalFront=localFront,worldFront=front,displacement=delta,moved=moved,forwardDot=dot});
                Check("street."+label+".mesh-forward."+i,root&&before.ContainsKey(root.GetInstanceID())&&moved>.02f&&dot>0&&root.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled&&r.gameObject.activeInHierarchy),
                    "Actual mesh front dot native displacement="+dot+" moved="+moved+" localFront="+localFront+" worldFront="+front+" delta="+delta+" source="+source,"NATIVE_MESH_FORWARD_MOTION");
            }
        }
        IEnumerator VehicleMeshForwardEvidence(GameObject[] roots,Dictionary<int,Vector3> before,Vector3 localFront,int expected,string label,string source)
        {
            float discontinuity=label=="traffic"?20f:50f;
            // The ambient route intentionally wraps its offscreen endpoint. A teleport
            // across that seam is not a velocity sample; observe a fresh short interval.
            for(int retry=0;retry<3&&roots.Any(g=>g&&before.ContainsKey(g.GetInstanceID())&&Vector3.Distance(before[g.GetInstanceID()],g.transform.position)>=discontinuity);retry++)
            {FixtureNote("VEHICLE_LOOP_WRAP_EXCLUDED",label+"; endpoint displacement>="+discontinuity+"m excluded from directional evidence. Resample actual roots after.12s without moving them.");before=roots.Where(g=>g).ToDictionary(g=>g.GetInstanceID(),g=>g.transform.position);yield return Wait(.12f);}
            bool clean=roots.All(g=>g&&before.ContainsKey(g.GetInstanceID())&&Vector3.Distance(before[g.GetInstanceID()],g.transform.position)<discontinuity);
            Check("street."+label+".direction-sample-no-wrap",clean,"Only actual bounded motion intervals may support direction evidence. Missing clean sample fails; no skip-to-PASS.");
            if(clean)CheckVehicleMeshForward(roots,before,localFront,expected,label,source);
        }
        IEnumerator Street()
        {
            w.ReturnToMenu();w.StartGame(false);report.startGameCalls++;w.FullFlowScriptedInput=true;yield return null;
            var skip=w.GetType().GetMethod("SkipIntroStoryV414",Instance);if(skip!=null)skip.Invoke(w,null);else if(w.CinematicActive)w.SkipResidenceCinematic();yield return null;
            var street=StreetComponent();Check("street.required-component",street!=null,"Requires real civilian, vehicle and response observation seams; missing is FAIL.");
            if(!street)yield break;
            Check("street.alive-before-disaster",Int(street,"CivilianCount")>=2&&Int(street,"VehicleCount")>=1&&Int(street,"MovingCivilianCount")>0&&Int(street,"MovingVehicleCount")>0&&!Bool(street,"ResponseActive"),"civilian="+Int(street,"CivilianCount")+" moving="+Int(street,"MovingCivilianCount")+" cars="+Int(street,"VehicleCount"));
            var civilianRoots=StreetRoots(street,"civilians");var trafficRoots=StreetRoots(street,"traffic");
            var before=civilianRoots.Concat(trafficRoots).Distinct().ToDictionary(g=>g.GetInstanceID(),g=>g.transform.position);
            yield return Wait(.8f);int civiliansMoved=civilianRoots.Count(g=>g&&before.ContainsKey(g.GetInstanceID())&&Vector3.Distance(before[g.GetInstanceID()],g.transform.position)>.02f);
            int vehiclesMoved=trafficRoots.Count(g=>g&&before.ContainsKey(g.GetInstanceID())&&Vector3.Distance(before[g.GetInstanceID()],g.transform.position)>.02f);
            Check("street.actual-native-transforms-move",civiliansMoved>0&&vehiclesMoved>0,"Actual civilian roots moved="+civiliansMoved+" traffic roots moved="+vehiclesMoved+"; counts alone are insufficient.");
            yield return VehicleMeshForwardEvidence(trafficRoots,before,Vector3.back,2,"traffic","Actual ResidenceArt.Car bonnet/headlights are local-Z, red tail lamps+Z; source-forward-proof.json binds this geometry. Independent expected axis, not production VehicleHeading output.");
            yield return Photo("22-alive-street-before",new Vector3(-5,5,-28),new Vector3(0,1,-18),14f);
            w.ReturnToMenu();w.StartDisasterSimulation(DisasterType.Fire);report.simulationStarts++;w.FullFlowScriptedInput=true;yield return null;w.Model.EnterBunker();
            float end=Time.realtimeSinceStartup+28;while(!Bool(street,"ResponseActive")&&Time.realtimeSinceStartup<end)yield return null;
            var responseVehicles=ResponseRoots(street,false);var responseBefore=responseVehicles.ToDictionary(g=>g.GetInstanceID(),g=>g.transform.position);
            yield return Wait(5.25f);int arrivingMoved=responseVehicles.Count(g=>g&&responseBefore.ContainsKey(g.GetInstanceID())&&Vector3.Distance(responseBefore[g.GetInstanceID()],g.transform.position)>.02f);
            Check("street.actual-response-vehicles-move",responseVehicles.Length==4&&arrivingMoved==4,"Actual arrival roots moving across real frames="+arrivingMoved+"/4; no speed-only counter claim.","NATIVE_RENDER_TRANSFORM");
            yield return VehicleMeshForwardEvidence(responseVehicles,responseBefore,Vector3.forward,4,"response","Actual four EmergencyVehicle cab/headlights local+Z, rear red lamps-Z; independent authored-axis expectation, not root moved alone.");
            FixtureNote("RESPONSE_REAL_ARRIVAL_WAIT","ResponseActive creates cars before arrival; crews activate only at actual destinations. Wait for4 crews on the existing Fire aftermath, without a new disaster or time mutation.");
            while((Int(street,"ResponseVehicleCount")!=4||Int(street,"ResponderCount")!=4)&&Time.realtimeSinceStartup<end)yield return null;
            var responseRoot=Field<GameObject>(street,"responseRoot");
            Check("street.actual-disaster-response",Bool(street,"ResponseActive")&&Int(street,"ResponseVehicleCount")==4&&Int(street,"ResponderCount")==4&&responseRoot&&responseRoot.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled&&r.gameObject.activeInHierarchy),"Production phase lifecycle and real arrival activate all4 vehicles and crews.");
            string[] services={"Police","Ambulance","Fire","Rescue"};var responseCrews=ResponseRoots(street,true);
            Check("street.four-actual-service-kinds",responseVehicles.Length==4&&responseCrews.Length==4&&services.All(service=>responseVehicles.Count(g=>g&&g.name=="V414 "+service+" response vehicle"&&g.activeInHierarchy&&g.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled&&r.gameObject.activeInHierarchy))==1&&responseCrews.Count(g=>g&&g.name=="V414 "+service+" responder"&&g.activeInHierarchy&&g.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled&&r.gameObject.activeInHierarchy))==1),"Exactly one actual named production vehicle and rendered active crew for Police/Ambulance/Fire/Rescue; visual differentiation remains human review.","NATIVE_RENDER_STATE");
            var crewBefore=responseCrews.ToDictionary(g=>g.GetInstanceID(),g=>g.transform.position);yield return Wait(.65f);
            int crewsMoved=responseCrews.Count(g=>g&&crewBefore.ContainsKey(g.GetInstanceID())&&Vector3.Distance(crewBefore[g.GetInstanceID()],g.transform.position)>.02f);
            Check("street.actual-four-crews-move",responseCrews.Length==4&&crewsMoved==4,"Actual active responder roots moving across real frames="+crewsMoved+"/4.","NATIVE_RENDER_TRANSFORM");
            yield return Photo("23-street-emergency-response",new Vector3(8,7,-30),new Vector3(0,1,-18),16f);
        }
        IEnumerator Models()
        {
            FixtureNote("MODEL_FIXTURES","Exact thresholds and same-seed risk comparisons use isolated new production models. Model.Tick only here; no live run timer, loss or budget tolerance mutation.");
            string original=PanicModel.ScorePathOverride;
            Check("duration45.canonical-normal-contract",PanicModel.NormalPrecautionSeconds==120f&&PanicModel.NormalBagSeconds==45f&&PanicModel.NormalPreparationSeconds==165f&&PanicHomeWorld.BagPreparationSeconds==45f,
                "Explicit120+45=165 contract and World alias; damage/cost/controller rules unchanged.","NATIVE_MODEL_FIXTURE");
            PanicModel.ScorePathOverride=Path.Combine(w.EvidencePath,"duration45-model-timing.json");int timingCheck=0;
            PanicRevisionV412ModelChecks.CheckCampaignTiming((ok,label)=>Check("duration45.model."+(++timingCheck).ToString("D2"),ok,label,"NATIVE_MODEL_FIXTURE"));
            int shortTimingCheck=0;
            PanicRevisionV412ModelChecks.CheckShortFlowTimingR7((ok,label)=>Check("duration45.short."+(++shortTimingCheck).ToString("D2"),ok,label,"NATIVE_MODEL_FIXTURE"));
            report.modelFixtureTicks+=7;PanicModel.ScorePathOverride=original;
            FixtureNote("DURATION45_MODEL_CLOCK_FIXTURE","Separate fresh models:165start,paused120held,119active→46,120active→45,44bag→1,165deadline loss/retry and developer sticky freeze. Seven explicit production Model.Tick calls; not real-time/OS evidence.");
            FixtureNote("DURATION45_SHORT_CLOCK_FIXTURE","Separate short model/expedition: three production flow.Tick calls .1+.1+34.8 reach35elapsed of80 and final45, with270+30reserve unchanged; no bag/anchor/resource grants or World timer writes.");
            foreach(float boundary in new[]{74.99f,75f,75.01f})
            {
                PanicModel.ScorePathOverride=Path.Combine(w.EvidencePath,"model-boundary-"+boundary.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+".json");
                var m=new PanicModel();m.StartSimulation(DisasterType.Earthquake);m.Items.Clear();
                for(int i=0;i<12;i++)m.Items.Add(new PanicItem{Id="boundary-25-table-"+i,Label="Boundary table",ArtId="table",Kind=ItemKind.Furniture,Installed=true,Movable=true,Size=Vector3.one});
                string wall=boundary<75?"boundary-wall-11981":boundary==75?"boundary-wall-45988":"boundary-wall-51684";
                m.Items.Add(new PanicItem{Id=wall,Label="Boundary wall",ArtId="wall",Kind=ItemKind.Wall,Installed=true,Size=Vector3.one});m.EnterBunker();m.Tick(8);report.modelFixtureTicks++;
                Check("model.actual-damage."+boundary.ToString("F2"),Mathf.Abs(m.TotalDamage-boundary)<.002f,"Calculated="+m.TotalDamage.ToString("R"),"NATIVE_MODEL_FIXTURE");m.Tick(4);report.modelFixtureTicks++;
                Check("model.exact-loss-boundary."+boundary.ToString("F2"),boundary>=75?m.Phase==PanicPhase.GameOver:m.Phase==PanicPhase.Inspection,"Actual model phase="+m.Phase,"NATIVE_MODEL_FIXTURE");
            }
            PanicModel.ScorePathOverride=Path.Combine(w.EvidencePath,"model-price-contract.json");
            var priceModel=new PanicModel();priceModel.StartRun(false);var priceExpedition=new PanicExpedition(priceModel);priceExpedition.ToggleSource("garage_tools");
            int priceBudget=priceModel.Budget,priceScrews=priceExpedition.Count("screws"),priceBrackets=priceExpedition.Count("bracket");
            bool priceBegin=priceExpedition.CanBeginWork("anchorwall","bookshelf");priceExpedition.Work.Begin("anchorwall","bookshelf");int priceCap=40;
            while(priceExpedition.Work.Active&&!priceExpedition.Work.Complete&&priceCap-->0){priceExpedition.Work.TryClick(priceExpedition.Work.NextClickTarget);priceExpedition.Work.ApplyRotation(30,true);}
            bool priceDone=priceExpedition.TryCompleteWork("anchorwall","bookshelf");
            Check("model.same-canonical-anchor-cost",priceBegin&&priceDone&&priceModel.Budget==priceBudget-60&&priceExpedition.Count("screws")==priceScrews-2&&priceExpedition.Count("bracket")==priceBrackets-1&&!priceModel.CheatedRun,"Independent native model fixture agrees with world exact recipe; no grants.","NATIVE_MODEL_FIXTURE");
            priceBudget=priceModel.Budget;string priceStock=Stock(priceExpedition);Check("model.same-cost-no-duplicate",!priceExpedition.TryCompleteWork("anchorwall","bookshelf")&&priceBudget==priceModel.Budget&&priceStock==Stock(priceExpedition),"Duplicate completion preserves exact stock and budget.","NATIVE_MODEL_FIXTURE");
            foreach(string action in new[]{"gasoff","wateroff","poweroff"})
            {
                PanicModel.ScorePathOverride=Path.Combine(w.EvidencePath,"model-risk-"+action+".json");var m=new PanicModel();m.StartRun(false);var e=new PanicExpedition(m);
                DisasterType type=action=="wateroff"?DisasterType.Flood:DisasterType.Fire;m.Disaster=type;var calculate=typeof(PanicModel).GetMethod("CalculateDamage",Instance);calculate.Invoke(m,null);float before=m.TotalDamage;
                string target=PanicUtilitySafetyV414.TargetForAction(action);bool begins=e.CanBeginWork(action,target);e.Work.Begin(action,target);int cap=25;
                while(e.Work.Active&&!e.Work.Complete&&cap-->0){e.Work.TryClick(0);if(e.Work.UsesLinearDrag)e.Work.ApplyLinearDisplacement(1f/3f,true);else e.Work.ApplyRotation(30*e.Work.ExpectedDirection,true);}bool committed=e.TryCompleteWork(action,target);calculate.Invoke(m,null);
                Check("model.utility-real-risk-effect."+action,begins&&committed&&m.TotalDamage<before&&!m.CheatedRun,"same-seed damage before="+before+" after="+m.TotalDamage,"NATIVE_MODEL_FIXTURE");
            }
            PanicModel.ScorePathOverride=original;yield return null;
        }
        static string Hash(string path)
        {if(string.IsNullOrEmpty(path)||!File.Exists(path))return "absent";using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        void Finish()
        {
            if(finished)return;finished=true;
            foreach(string section in guideOnlyR8?new[]{"launch","onboarding"}:RequiredSections)if(!report.sections.Contains(section))report.rows.Add(new Row{id="required-section."+section,status="NOT_RUN",layer="NOT_RUN",detail="Required section did not complete.",frame=Time.frameCount});
            report.realScoreAfter=Hash(realScore);Check("scores.real-file-unchanged",report.realScoreBefore==report.realScoreAfter);
            report.seconds=Time.realtimeSinceStartup-began;report.finishedUtc=DateTime.UtcNow.ToString("o");
            report.passed=report.complete&&string.IsNullOrEmpty(report.error)&&report.errors.Count==0&&report.rows.Count>(guideOnlyR8?30:40)&&report.rows.All(r=>r.status=="PASS");
            if(photoCamera)Destroy(photoCamera.gameObject);Application.logMessageReceived-=OnLog;
            if(w&&!string.IsNullOrEmpty(w.EvidencePath))File.WriteAllText(Path.Combine(w.EvidencePath,"overhaul414-smoke.json"),JsonUtility.ToJson(report,true));
            Debug.Log("OVERHAUL414_SMOKE "+(report.passed?"PASS":"FAIL")+" checks="+report.rows.Count+" seconds="+report.seconds.ToString("F2"));Application.Quit(report.passed?0:2);
        }
    }
}
