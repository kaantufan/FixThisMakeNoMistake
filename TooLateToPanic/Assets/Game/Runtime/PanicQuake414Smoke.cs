using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in actual View/ScreenCapture quake presentation audit. Root alone builds and runs.</summary>
    public sealed class PanicQuake414Smoke : MonoBehaviour
    {
        public const string Flag = "--quake414-smoke";
        const float Watchdog = 45f;
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id,status,detail,layer="NATIVE_API_RENDER";public int frame; }
        [Serializable] public sealed class SubjectFrame { public string id;public Vector3 position;public Quaternion rotation; }
        [Serializable] public sealed class ShotFrame
        {
            public string label,path,sha256,phase,shot,resetReason;public int requestedFrame,bytes,subjects,sourceRenderers,proxyRenderers,hiddenSources,sampledFrames,restoreCount,proxyColliders,visibleRoofs,allRoofs;
            public float realtime,age,phaseClock,sampledAge,looseOffset,securedOffset,looseTilt,fov;
            public Vector3 camera,forward;public Quaternion rotation;public bool active,paused,screenCapture=true;
            public List<SubjectFrame> proxies=new List<SubjectFrame>();
            public int housePivotId,houseProxies,roofProxies,houseSources,roofSources;
            public Vector3 housePosition;public Quaternion houseRotation;public float houseOffset,houseTilt,maxHouseOffset,maxHouseTilt;
        }
        [Serializable] public sealed class RootEvidence { public string run,id;public Vector3 modelPosition,sourcePosition;public Quaternion sourceRotation;public Vector3 sourceScale;public int colliders,renderers; }
        [Serializable] public sealed class CaptureManifest { public string dllSha256,testLayer="NATIVE_API_RENDER";public bool osInput=false,humanAcceptance=false;public List<ShotFrame> captures; }
        [Serializable] public sealed class Report
        {
            public string startedUtc,finishedUtc,dllSha256,scorePath,realScoreBefore,realScoreAfter,error;
            public string testLayer="NATIVE_API_RENDER",evidence="Real production simulations, natural frames and actual View ScreenCapture; no OS input or human acceptance.";
            public bool complete,passed,osInput=false,humanAcceptance=false,naturalComplete,pauseComplete,skipComplete;
            public float seconds,maxLooseOffset,maxSecuredOffset,maxLooseTilt;public int simulationStarts,triggerCalls,productionSkipCalls,seriesCaptures,directModelTicks=0,timerWrites=0,ageWrites=0,qaWarps=0;
            public List<Row> rows=new List<Row>();public List<ShotFrame> frames=new List<ShotFrame>();public List<string> errors=new List<string>();public List<RootEvidence> sourceRoots=new List<RootEvidence>();
            public float maxHouseOffset,maxHouseTilt;public int observedHousePoseChanges;
        }
        sealed class RootState { public string Id;public Transform Root;public Vector3 Position,Scale;public Quaternion Rotation; }
        sealed class ColliderState { public Collider Collider;public Vector3 Position,Scale;public Quaternion Rotation;public bool Enabled,Trigger;public string Shape; }
        sealed class Snapshot
        {
            public readonly Dictionary<string,Vector3> ModelPositions=new Dictionary<string,Vector3>();
            public readonly List<RootState> Roots=new List<RootState>();public readonly List<ColliderState> Colliders=new List<ColliderState>();
            public readonly Dictionary<Renderer,bool> Forced=new Dictionary<Renderer,bool>();
        }
        readonly Report report=new Report();readonly HashSet<GameObject> observedDisplays=new HashSet<GameObject>();PanicHomeWorld world;PanicHomeHud hud;float began;bool finished;Coroutine sequence;string realScore;
        static T Field<T>(object owner,string name){var f=owner.GetType().GetField(name,Hidden);return f==null?default(T):(T)f.GetValue(owner);}
        float Age { get { return Field<float>(world,"residenceCinematicAge"); } }
        PanicQuakeReadabilityV414 Module { get { return world.QuakeReadabilityV414; } }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            if(!Environment.GetCommandLineArgs().Contains(Flag))return;
            var w=FindFirstObjectByType<PanicHomeWorld>();if(!w)throw new InvalidOperationException("Quake414 needs the actual Residence World.");
            if(!w.GetComponent<PanicQuake414Smoke>())w.gameObject.AddComponent<PanicQuake414Smoke>();
        }
        void Start()
        {
            if(!Environment.GetCommandLineArgs().Contains(Flag)){enabled=false;return;}
            world=GetComponent<PanicHomeWorld>();hud=GetComponent<PanicHomeHud>();began=Time.realtimeSinceStartup;
            report.startedUtc=DateTime.UtcNow.ToString("o");report.dllSha256=Hash(typeof(PanicModel).Assembly.Location);report.scorePath=PanicModel.ScorePathOverride;
            realScore=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");report.realScoreBefore=Hash(realScore);
            Application.runInBackground=true;Application.logMessageReceived+=OnLog;sequence=StartCoroutine(Guarded(Run()));
        }
        void Update(){if(!finished&&Time.realtimeSinceStartup-began>Watchdog){report.error="45s watchdog; required incomplete sections fail.";if(sequence!=null)StopCoroutine(sequence);Finish();}}
        void OnLog(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(message+"\n"+trace);}
        IEnumerator Guarded(IEnumerator body)
        {
            var stack=new Stack<IEnumerator>();stack.Push(body);
            while(stack.Count>0&&!finished)
            {
                bool more;object next=null;
                try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}
                catch(Exception ex){report.error=ex.ToString();Check("driver.exception",false,report.error);break;}
                if(!more){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;
            }
            if(!finished){report.complete=string.IsNullOrEmpty(report.error);Finish();}
        }
        void Check(string id,bool pass,string detail="")
        {report.rows.Add(new Row{id=id,status=pass?"PASS":"FAIL",detail=detail,frame=Time.frameCount});Debug.Log("QUAKE414 "+(pass?"PASS ":"FAIL ")+id+" "+detail);}
        IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
        static string V(Vector3 p){return p.x.ToString("R",CultureInfo.InvariantCulture)+","+p.y.ToString("R",CultureInfo.InvariantCulture)+","+p.z.ToString("R",CultureInfo.InvariantCulture);}
        static string Shape(Collider c)
        {
            if(c is BoxCollider b)return "box:"+V(b.center)+":"+V(b.size);
            if(c is CapsuleCollider k)return "capsule:"+V(k.center)+":"+k.radius.ToString("R",CultureInfo.InvariantCulture)+":"+k.height.ToString("R",CultureInfo.InvariantCulture)+":"+k.direction;
            if(c is SphereCollider s)return "sphere:"+V(s.center)+":"+s.radius.ToString("R",CultureInfo.InvariantCulture);
            if(c is MeshCollider m)return "mesh:"+(m.sharedMesh?m.sharedMesh.GetInstanceID():0)+":"+m.convex;
            return c.GetType().Name;
        }
        Snapshot TakeSnapshot(string run)
        {
            var result=new Snapshot();var roots=Field<Dictionary<string,GameObject>>(world,"items");
            foreach(var item in world.Model.Items)
            {
                result.ModelPositions[item.Id]=item.Position;
                if(!item.Installed||!item.Movable||item.Kind!=ItemKind.Furniture||roots==null||!roots.TryGetValue(item.Id,out var root)||!root)continue;
                var t=root.transform;result.Roots.Add(new RootState{Id=item.Id,Root=t,Position=t.position,Rotation=t.rotation,Scale=t.localScale});
                var colliders=root.GetComponentsInChildren<Collider>(true);var renderers=root.GetComponentsInChildren<Renderer>(true);
                foreach(var c in colliders)result.Colliders.Add(new ColliderState{Collider=c,Position=c.transform.position,Rotation=c.transform.rotation,Scale=c.transform.localScale,Enabled=c.enabled,Trigger=c.isTrigger,Shape=Shape(c)});
                foreach(var r in renderers)result.Forced[r]=r.forceRenderingOff;
                report.sourceRoots.Add(new RootEvidence{run=run,id=item.Id,modelPosition=item.Position,sourcePosition=t.position,sourceRotation=t.rotation,sourceScale=t.localScale,colliders=colliders.Length,renderers=renderers.Length});
            }
            var floors=Field<List<GameObject>>(world,"floorVisuals");
            if(floors!=null)foreach(var floor in floors)if(floor&&floor.transform.position.y>=-.1f)CaptureHouseSource(result,floor,run);
            foreach(string field in new[]{"roof","residenceGarage","residenceBalcony"})CaptureHouseSource(result,Field<GameObject>(world,field),run);
            foreach(var item in world.Model.Items)
                if(item.Installed&&item.Position.y>=-.1f&&item.Position.y<3.4f&&
                    (item.Kind==ItemKind.Door||item.Kind==ItemKind.Window||item.Kind==ItemKind.Wall)&&
                    (Mathf.Abs(item.Position.x)>7.6f||Mathf.Abs(item.Position.z)>5.6f)&&roots!=null&&roots.TryGetValue(item.Id,out var part))CaptureHouseSource(result,part,run);
            var targets=Field<Dictionary<string,GameObject>>(world,"residenceTargets");
            if(targets!=null&&targets.TryGetValue("@garage",out var garageDoor))CaptureHouseSource(result,garageDoor,run);
            Check(run+".real-source-baseline",result.Roots.Count>0&&result.Colliders.Count>0&&result.Forced.Count>0,"Actual movable furniture roots, original renderer flags, real colliders and all Model.Positions captured before hazard.");return result;
        }
        void CaptureHouseSource(Snapshot s,GameObject root,string run)
        {
            if(!root)return;
            var transforms=new List<Transform>{root.transform};var renderers=root.GetComponentsInChildren<Renderer>(true);var colliders=root.GetComponentsInChildren<Collider>(true);
            transforms.AddRange(renderers.Select(r=>r.transform));
            foreach(var t in transforms)if(!s.Roots.Any(r=>r.Root==t))s.Roots.Add(new RootState{Id="house:"+t.name,Root=t,Position=t.position,Rotation=t.rotation,Scale=t.localScale});
            foreach(var c in colliders)if(!s.Colliders.Any(p=>p.Collider==c))s.Colliders.Add(new ColliderState{Collider=c,Position=c.transform.position,Rotation=c.transform.rotation,Scale=c.transform.localScale,Enabled=c.enabled,Trigger=c.isTrigger,Shape=Shape(c)});
            foreach(var r in renderers)if(!s.Forced.ContainsKey(r))s.Forced[r]=r.forceRenderingOff;
            report.sourceRoots.Add(new RootEvidence{run=run,id="house:"+root.name,sourcePosition=root.transform.position,sourceRotation=root.transform.rotation,sourceScale=root.transform.localScale,colliders=colliders.Length,renderers=renderers.Length});
        }
        bool GeometryUnchanged(Snapshot s)
        {
            return s.ModelPositions.All(p=>world.Model.Items.Any(i=>i.Id==p.Key&&(i.Position-p.Value).sqrMagnitude<1e-10f))&&
                s.Roots.All(r=>r.Root&&(r.Root.position-r.Position).sqrMagnitude<1e-10f&&Quaternion.Angle(r.Root.rotation,r.Rotation)<.005f&&(r.Root.localScale-r.Scale).sqrMagnitude<1e-10f)&&
                s.Colliders.All(c=>c.Collider&&c.Collider.enabled==c.Enabled&&c.Collider.isTrigger==c.Trigger&&Shape(c.Collider)==c.Shape&&(c.Collider.transform.position-c.Position).sqrMagnitude<1e-10f&&Quaternion.Angle(c.Collider.transform.rotation,c.Rotation)<.005f&&(c.Collider.transform.localScale-c.Scale).sqrMagnitude<1e-10f);
        }
        static bool FlagsRestored(Snapshot s){return s.Forced.All(p=>p.Key&&p.Key.forceRenderingOff==p.Value);}
        bool Clean(Snapshot s)
        {
            var q=Module;return q&&!q.Active&&q.ActiveHiddenRendererCount==0&&q.ActiveProxyObjectCount==0&&q.ActiveProxyColliderCount==0&&q.ActiveOwnedAssetCount==0&&!q.DisplayRoot&&!q.HousePivot&&q.ActiveExternalHouseProxyCount==0&&q.RestoreMismatchCount==0&&FlagsRestored(s)&&observedDisplays.All(g=>!g||!g.activeInHierarchy);
        }
        void SampleMaxima()
        {var q=Module;if(!q)return;report.maxLooseOffset=Mathf.Max(report.maxLooseOffset,q.MaximumLooseOffset);report.maxSecuredOffset=Mathf.Max(report.maxSecuredOffset,q.MaximumSecuredOffset);report.maxLooseTilt=Mathf.Max(report.maxLooseTilt,q.MaximumLooseTilt);report.maxHouseOffset=Mathf.Max(report.maxHouseOffset,q.MaximumHouseOffset);report.maxHouseTilt=Mathf.Max(report.maxHouseTilt,q.MaximumHouseTilt);}
        static int ActualVisibleRoofProxies(PanicQuakeReadabilityV414 q)
        {
            if(!q||!q.HousePivot)return 0;var proxies=q.HousePivot.GetComponentsInChildren<MeshRenderer>(true);
            return q.OriginalRoofRenderers.Count(source=>source&&source.GetComponent<MeshFilter>()&&proxies.Any(p=>p&&p.enabled&&p.gameObject.activeInHierarchy&&!p.forceRenderingOff&&
                p.GetComponent<MeshFilter>()&&p.GetComponent<MeshFilter>().sharedMesh==source.GetComponent<MeshFilter>().sharedMesh&&
                (p.transform.localPosition-source.transform.position).sqrMagnitude<1e-8f&&Quaternion.Angle(p.transform.localRotation,source.transform.rotation)<.005f));
        }
        ShotFrame Observe(string label)
        {
            var q=Module;
            var f=new ShotFrame{label=label,requestedFrame=Time.frameCount,realtime=Time.realtimeSinceStartup-began,age=Age,phaseClock=world.Model.PhaseTime,phase=world.Model.Phase.ToString(),
                camera=world.View.transform.position,rotation=world.View.transform.rotation,forward=world.View.transform.forward,fov=world.View.fieldOfView,paused=world.Model.IsPaused,
                active=q&&q.Active,shot=q?q.Shot:"missing",subjects=q?q.SubjectCount:0,sourceRenderers=q?q.SourceRendererCount:0,proxyRenderers=q?q.ProxyRendererCount:0,hiddenSources=q?q.ActiveHiddenRendererCount:0,
                sampledFrames=q?q.SampledFrames:0,restoreCount=q?q.RestoreCount:0,sampledAge=q?q.SampledAge:0,looseOffset=q?q.MaximumLooseOffset:0,securedOffset=q?q.MaximumSecuredOffset:0,looseTilt=q?q.MaximumLooseTilt:0,resetReason=q?q.LastResetReason:"missing",
                proxyColliders=q&&q.DisplayRoot?q.DisplayRoot.GetComponentsInChildren<Collider>(true).Length:0,allRoofs=q?q.OriginalRoofRenderers.Length:0,visibleRoofs=ActualVisibleRoofProxies(q),
                housePivotId=q&&q.HousePivot?q.HousePivot.GetInstanceID():0,housePosition=q&&q.HousePivot?q.HousePivot.position:Vector3.zero,houseRotation=q&&q.HousePivot?q.HousePivot.rotation:Quaternion.identity,
                houseProxies=q?q.ExternalHouseProxyCount:0,roofProxies=q?q.RoofProxyCount:0,houseSources=q?q.OriginalHouseRenderers.Length:0,roofSources=q?q.OriginalRoofRenderers.Length:0,
                houseOffset=q?q.CurrentHouseOffset.magnitude:0,houseTilt=q?q.CurrentHouseTilt:0,maxHouseOffset=q?q.MaximumHouseOffset:0,maxHouseTilt=q?q.MaximumHouseTilt:0};
            if(q&&q.DisplayRoot)foreach(Transform pivot in q.DisplayRoot.transform)if(pivot.name.StartsWith("Quake display copy ",StringComparison.Ordinal))f.proxies.Add(new SubjectFrame{id=pivot.name,position=pivot.position,rotation=pivot.rotation});
            report.frames.Add(f);return f;
        }
        IEnumerator Capture(string label,bool series)
        {
            // Caller has yielded end-of-frame: this is the real active View, never a QA camera.
            var f=Observe(label);f.path=Path.Combine(world.EvidencePath,label+".png");ScreenCapture.CaptureScreenshot(f.path);
            float end=Time.realtimeSinceStartup+.8f;do{yield return null;}while(!File.Exists(f.path)&&Time.realtimeSinceStartup<end);
            bool exists=File.Exists(f.path)&&new FileInfo(f.path).Length>1024;
            if(exists){f.bytes=(int)new FileInfo(f.path).Length;f.sha256=Hash(f.path);if(series)report.seriesCaptures++;}
            Check("capture."+label,exists,f.path+" requestedAge="+f.age.ToString("F3",CultureInfo.InvariantCulture));
        }
        IEnumerator Run()
        {
            string[] args=Environment.GetCommandLineArgs();string reason;
            bool registered=PanicTestLaunchRules.Drivers.Contains(Flag),valid=PanicTestLaunchRules.Validate(args,File.Exists,out reason);
            string leaf=Path.GetFileName((world.EvidencePath??"").TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar));Guid guid;
            Check("launch.registered-sole-driver",registered&&valid&&PanicTestLaunchRules.Drivers.Count(args.Contains)==1,reason);
            bool isolated=!string.IsNullOrEmpty(world.EvidencePath)&&leaf.Length>=36&&Guid.TryParseExact(leaf.Substring(leaf.Length-36),"D",out guid)&&!string.IsNullOrEmpty(report.scorePath)&&Path.GetFullPath(report.scorePath)==Path.Combine(Path.GetFullPath(world.EvidencePath),"isolated-home-scores.json")&&!File.Exists(report.scorePath);
            Check("launch.fresh-guid-isolated-score",isolated,"Opt-in flag must be registered before Model construction. Missing registration cannot touch the real profile.");
            if(!registered||!valid||!isolated)yield break;
            float splashEnd=Time.realtimeSinceStartup+10;while(!UnityEngine.Rendering.SplashScreen.isFinished&&Time.realtimeSinceStartup<splashEnd)yield return null;
            Check("launch.real-view",world.View&&world.View.enabled&&world.View.gameObject.activeInHierarchy&&!world.View.targetTexture&&Screen.width>0&&Screen.height>0);
            world.FullFlowScriptedInput=true;
            yield return Natural();yield return Skip();
            Check("final.no-cheats",!world.Model.CheatedRun&&!world.Model.DeveloperAnyEnabled);
        }
        IEnumerator Natural()
        {
            Check("natural.real-start",world.StartDisasterSimulation(DisasterType.Earthquake));report.simulationStarts++;yield return null;
            Check("natural.simulation-ready",world.Model.IsSimulation&&world.Model.Disaster==DisasterType.Earthquake&&world.Model.Phase==PanicPhase.Preparing&&!hud.FreeplayOnboardingOpenV414&&!world.Model.IsPaused);
            var baseline=TakeSnapshot("natural");world.TriggerSimulation();report.triggerCalls++;yield return null;
            Check("natural.real-trigger",world.Model.Phase==PanicPhase.Disaster,"Production TriggerSimulation only. No direct Model.Tick, cinematic age/timer write, anchored fixture or actor Warp.");
            bool paused=false,exterior=true,geometry=true,proxyColliders=true,hidden=true,actualProxy=true,house=true;float next=4.72f;int index=0,activeSamples=0,previousPivot=0;Vector3 previousPosition=Vector3.zero;Quaternion previousRotation=Quaternion.identity;
            float deadline=Time.realtimeSinceStartup+12;
            while(world.Model.Phase==PanicPhase.Disaster&&Time.realtimeSinceStartup<deadline)
            {
                yield return new WaitForEndOfFrame();var q=Module;if(!q||!q.Active)continue;
                activeSamples++;if(q.DisplayRoot)observedDisplays.Add(q.DisplayRoot);SampleMaxima();geometry&=GeometryUnchanged(baseline);
                proxyColliders&=q.DisplayRoot&&q.DisplayRoot.GetComponentsInChildren<Collider>(true).Length==0;
                hidden&=q.ActiveHiddenRendererCount>0&&q.OriginalSourceRenderers.Length>0&&q.OriginalSourceRenderers.All(r=>r&&r.forceRenderingOff);
                actualProxy&=q.SourceRendererCount>0&&q.ProxyRendererCount>0&&q.DisplayRoot&&q.DisplayRoot.GetComponentsInChildren<Renderer>(true).Any(r=>r.enabled&&r.gameObject.activeInHierarchy&&!r.forceRenderingOff);
                exterior&=Age>=4.7f&&Age<8&&q.Shot=="roofed exterior"&&world.View.transform.position.y>8;
                house&=q.HousePivot&&q.ExternalHouseProxyCount>0&&q.RoofProxyCount>0&&q.OriginalHouseRenderers.Length>0&&q.OriginalRoofRenderers.Length>0&&
                    q.OriginalHouseRenderers.All(r=>r&&baseline.Forced.ContainsKey(r)&&r.forceRenderingOff)&&q.OriginalRoofRenderers.All(r=>r&&baseline.Forced.ContainsKey(r)&&r.forceRenderingOff)&&
                    ActualVisibleRoofProxies(q)==q.OriginalRoofRenderers.Length;
                if(q.HousePivot){int id=q.HousePivot.GetInstanceID();if(id==previousPivot&&((q.HousePivot.position-previousPosition).sqrMagnitude>1e-10f||Quaternion.Angle(q.HousePivot.rotation,previousRotation)>.005f))report.observedHousePoseChanges++;
                    previousPivot=id;previousPosition=q.HousePivot.position;previousRotation=q.HousePivot.rotation;}
                if(Age>=next&&Age<8&&index<24)
                {
                    string label="quake-sequence-"+(++index).ToString("D2");next=Age+.15f;yield return Capture(label,true);
                    var f=report.frames.Last();exterior&=f.shot=="roofed exterior"&&f.allRoofs>0&&f.visibleRoofs==f.allRoofs;
                }
                if(!paused&&Age>=5.5f&&world.Model.Phase==PanicPhase.Disaster){paused=true;yield return Pause(baseline);}
            }
            yield return new WaitForEndOfFrame();
            Check("natural.actual-window-capture-count",report.seriesCaptures>=16&&report.seriesCaptures<=24,"Real4.7–8s sequence PNGs="+report.seriesCaptures+" at target.15s cinematic-age intervals; actual request ages/frames retained.");
            Check("natural.all-window-roofed-exterior",activeSamples>1&&exterior,"All observed active4.7-8s frames and sequence captures use the actual roofed exterior View; no room requirement.");
            Check("natural.actual-house-pivot-and-roof-proxies",activeSamples>1&&house&&report.observedHousePoseChanges>1&&report.maxHouseOffset>0&&report.maxHouseTilt>0,"Independent same-instance HousePivot pose changes="+report.observedHousePoseChanges+", maximum offset="+report.maxHouseOffset+"m tilt="+report.maxHouseTilt+"deg; actual roof proxy mesh/pose matches all original roof sources.");
            Check("natural.active-original-hidden-proxy-visible",activeSamples>0&&hidden&&actualProxy,"Independent actual original forceRenderingOff flags and proxy Renderer hierarchy, not counters alone.");
            Check("natural.proxy-zero-collider",activeSamples>0&&proxyColliders,"Actual temporary display.GetComponentsInChildren<Collider>(true) remains0.");
            Check("natural.source-model-collider-roots-fixed",activeSamples>0&&geometry&&GeometryUnchanged(baseline),"All baseline Model.Positions, furniture and house/roof/garage source roots and renderer transforms, real collider transforms/shapes/enabled/trigger unchanged.");
            Check("natural.loose-visible-motion",report.maxLooseOffset>.1f&&report.maxLooseTilt>6,"Observed max loose offset="+report.maxLooseOffset+"m tilt="+report.maxLooseTilt+"deg. No protected fixture fabricated.");
            Check("natural.real-aftermath",world.Model.Phase==PanicPhase.Aftermath,"Natural production8s disaster, no completion fixture.");
            Check("natural.cleanup-restores-original-flags",Clean(baseline),"Original exact previous forceRenderingOff values restored, actual proxies/owned assets absent, restore mismatch0.");
            float end=Time.realtimeSinceStartup+6;while(world.Model.Phase==PanicPhase.Aftermath&&Time.realtimeSinceStartup<end)yield return null;
            Check("natural.real-inspection",world.Model.Phase==PanicPhase.Inspection&&world.Model.SimulationComplete,"Natural production aftermath completes into simulation inspection.");
            world.ReturnToMenu();yield return new WaitForEndOfFrame();Check("natural.title-cleanup",world.Model.Phase==PanicPhase.Title&&Clean(baseline));report.naturalComplete=true;
        }
        IEnumerator Pause(Snapshot baseline)
        {
            var q=Module;float age=Age,clock=world.Model.PhaseTime,sampled=q.SampledAge;int restores=q.RestoreCount;var oldRoot=q.DisplayRoot;
            hud.SetPaused(true);yield return new WaitForEndOfFrame();
            Check("pause.proxy-restore-not-freeze",world.Model.IsPaused&&world.InputLocked&&Clean(baseline)&&(!oldRoot||!oldRoot.activeInHierarchy)&&q.RestoreCount==restores+1,"Production HUD pause restores sources and removes proxies; proxy pose is not expected to freeze.");
            float end=Time.realtimeSinceStartup+.3f;bool plateau=true;
            while(Time.realtimeSinceStartup<end){yield return null;plateau&=Mathf.Abs(Age-age)<.001f&&Mathf.Abs(world.Model.PhaseTime-clock)<.001f&&Mathf.Abs(q.SampledAge-sampled)<.001f&&Clean(baseline)&&GeometryUnchanged(baseline);}
            Check("pause.real-clock-cinema-plateau",plateau&&world.Model.IsPaused,"Actual.3s realtime, Model.PhaseTime and private residenceCinematicAge plateau; no Tick/time writes.");
            yield return new WaitForEndOfFrame();yield return Capture("quake-pause-source-restored",false);
            hud.SetPaused(false);yield return null;yield return new WaitForEndOfFrame();
            Check("pause.same-age-natural-resume",!world.Model.IsPaused&&!world.InputLocked&&Module&&Module.Active&&Age>age&&Age<age+.25f&&world.Model.PhaseTime<clock&&Module.RestoreCount==restores+1,"Fresh production proxy pass resumes from the preserved age; its counters/maxima may restart.");report.pauseComplete=true;
        }
        IEnumerator Skip()
        {
            Check("skip.real-second-start",world.StartDisasterSimulation(DisasterType.Earthquake));report.simulationStarts++;yield return null;
            var baseline=TakeSnapshot("skip");world.TriggerSimulation();report.triggerCalls++;yield return null;
            float end=Time.realtimeSinceStartup+7;while(world.Model.Phase==PanicPhase.Disaster&&Age<5.1f&&Time.realtimeSinceStartup<end)yield return null;
            yield return new WaitForEndOfFrame();var q=Module;Check("skip.active-real-age",q&&q.Active&&Age>=5.1f&&Age<5.5f,"Actual production cinematic age="+Age+" before production skip.");
            if(q&&q.DisplayRoot)observedDisplays.Add(q.DisplayRoot);
            yield return Capture("quake-second-before-skip",false);int restores=q?q.RestoreCount:-1;
            world.SkipResidenceCinematic();report.productionSkipCalls++;yield return new WaitForEndOfFrame();
            Check("skip.real-action-cleans-once",q&&q.RestoreCount==restores+1&&Clean(baseline)&&!world.CinematicActive&&world.Model.Phase==PanicPhase.Inspection,"Real SkipResidenceCinematic owns its Model transitions (including its internal Tick); this driver does not call Model.Tick or mutate age.");
            Check("skip.source-model-collider-roots-fixed",GeometryUnchanged(baseline));
            world.ReturnToMenu();yield return new WaitForEndOfFrame();Check("skip.title-cleanup",world.Model.Phase==PanicPhase.Title&&Clean(baseline));report.skipComplete=true;
        }
        static string Hash(string path){if(string.IsNullOrEmpty(path)||!File.Exists(path))return "absent";using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        void Finish()
        {
            if(finished)return;finished=true;report.realScoreAfter=Hash(realScore);Check("score.real-file-unchanged",report.realScoreBefore==report.realScoreAfter);
            Check("required.natural-pause-skip-complete",report.naturalComplete&&report.pauseComplete&&report.skipComplete);
            Check("captures.final-files-hash-preserved",report.frames.Count>0&&report.frames.All(f=>!string.IsNullOrEmpty(f.path)&&File.Exists(f.path)&&new FileInfo(f.path).Length==f.bytes&&Hash(f.path)==f.sha256));
            report.seconds=Time.realtimeSinceStartup-began;report.finishedUtc=DateTime.UtcNow.ToString("o");
            report.passed=report.complete&&string.IsNullOrEmpty(report.error)&&report.errors.Count==0&&report.rows.Count>20&&report.rows.All(r=>r.status=="PASS");
            Application.logMessageReceived-=OnLog;
            if(world&&!string.IsNullOrEmpty(world.EvidencePath))
            {
                File.WriteAllText(Path.Combine(world.EvidencePath,"quake414-smoke.json"),JsonUtility.ToJson(report,true));
                File.WriteAllText(Path.Combine(world.EvidencePath,"quake414-capture-manifest.json"),JsonUtility.ToJson(new CaptureManifest{dllSha256=report.dllSha256,captures=report.frames},true));
            }
            Debug.Log("QUAKE414_SMOKE "+(report.passed?"PASS":"FAIL")+" checks="+report.rows.Count+" seconds="+report.seconds.ToString("F2"));Application.Quit(report.passed?0:2);
        }
    }
}
