using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    // Independent supplementary NATIVE_API audit. Root alone intakes/builds/runs it.
    // Runtime initializer removes any need to edit live World/Awake. --home-evidence is already consumed by World before construction.
    public sealed class PanicE2EPhysicsAudit : MonoBehaviour
    {
        const string Flag="--e2e-physics-audit",ReportName="native-e2e-physics-audit.json";
        const float HardCap=250f;
        [Serializable] sealed class Check { public string caseId,name,detail;public bool passed; }
        [Serializable] sealed class SpeedSample { public string id;public Vector3 start,end;public float wallSeconds,inputSeconds,metresPerWallSecond,metresPerInputSecond,playback;public int frames,clampedFrames; }
        [Serializable] sealed class Report
        {
            public string startedUtc,finishedUtc,profileId,scorePath;
            public string evidenceLayer="NATIVE_API";
            public string limitations="Supplementary native production API audit only. Counted setup warps, natural per-frame SmokeMove and StepWork; input/model clock locked. No OS keys/mouse, no real run-key speed, no complete stair/door route or HUMAN/visual acceptance. Existing door-jam, doors-stairs and interaction smoke remain required. No test Warp during individual measurement/installation/placement segments.";
            public bool completed,passed;public float runtimeSeconds,treeActorDrift,treeMaxPenetration;
            public int explicitSetupWarps,startGameWarps,simulationStartWarps,manualWorkTickFrames;
            public int modelInjectedTicks=0,resourceGrants=0,developerMutations=0;
            public List<Check> checks=new List<Check>(); public List<SpeedSample> speeds=new List<SpeedSample>();
            public List<string> errors=new List<string>(),screenshots=new List<string>(),fixtures=new List<string>();
        }
        readonly Report report=new Report();
        PanicHomeWorld w; CharacterController body;CapsuleCollider query;
        Coroutine sequence;float started;bool finished;
        static float Dt=>Mathf.Max(.0001f,Mathf.Min(Time.unscaledDeltaTime,.08f));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Attach()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),Flag)<0)return;
            var world=FindFirstObjectByType<PanicHomeWorld>();
            if(!world)throw new InvalidOperationException("Physics audit requires the actual Residence World after Awake.");
            world.gameObject.AddComponent<PanicE2EPhysicsAudit>();
        }
        void Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),Flag)<0){enabled=false;return;}
            w=GetComponent<PanicHomeWorld>();started=Time.realtimeSinceStartup;report.startedUtc=DateTime.UtcNow.ToString("o");
            Application.logMessageReceived+=Log;Application.runInBackground=true;
            sequence=StartCoroutine(Guarded(Run()));
        }
        IEnumerator Guarded(IEnumerator task)
        {
            var stack=new Stack<IEnumerator>();stack.Push(task);
            while(stack.Count>0&&!finished)
            {
                bool next;object value=null;
                try{next=stack.Peek().MoveNext();if(next)value=stack.Peek().Current;}
                catch(Exception ex){report.errors.Add(ex.ToString());break;}
                if(!next){stack.Pop();continue;}if(value is IEnumerator nested)stack.Push(nested);else yield return value;
            }
            Finish();
        }
        IEnumerator Run()
        {
            if(!w||string.IsNullOrEmpty(w.EvidencePath))throw new InvalidOperationException("Fresh --home-evidence must be assigned before model construction.");
            report.profileId=Path.GetFileName(Path.GetFullPath(w.EvidencePath).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar));report.scorePath=PanicModel.ScorePathOverride;
            Guid g;string score=Path.GetFullPath(Path.Combine(w.EvidencePath,"isolated-home-scores.json"));
            bool fresh=report.profileId.Length>=36&&Guid.TryParse(report.profileId.Substring(report.profileId.Length-36),out g)&&!string.IsNullOrEmpty(report.scorePath)&&string.Equals(score,Path.GetFullPath(report.scorePath),StringComparison.OrdinalIgnoreCase)&&!File.Exists(score)&&!File.Exists(Path.Combine(w.EvidencePath,ReportName))&&w.Model.Highscores.Count==0;
            C("SYS-018","fresh isolated GUID profile",fresh,report.profileId);if(!fresh)yield break;
            bool single=!Environment.GetCommandLineArgs().Any(a=>a.EndsWith("-smoke",StringComparison.Ordinal)||a=="--interaction-audit");
            C("SYS-018","single opt-in owner",single,"This audit cannot run concurrently with a smoke in one process.");if(!single)yield break;
            w.InputLocked=true;body=FindObjectsByType<CharacterController>(FindObjectsSortMode.None).Single(c=>c.name=="Survivor");
            var holder=new GameObject("E2E independent enabled actor capsule");holder.layer=2;holder.transform.position=Vector3.one*500;holder.transform.localScale=body.transform.lossyScale;
            query=holder.AddComponent<CapsuleCollider>();query.direction=1;query.center=body.center;query.height=body.height;query.radius=body.radius;query.enabled=true;query.isTrigger=true;Physics.SyncTransforms();
            C("MEC-079","retained actual controller dimensions",ControllerContract(),"radius="+body.radius+" height="+body.height+" center="+body.center+" step="+body.stepOffset+" slope="+body.slopeLimit+" skin="+body.skinWidth);
            w.StartGame(false);report.startGameWarps++;yield return Wait(.2f);
            yield return MeasureSpeed("walk cardinal",Vector3.right,new Vector3(-4,.05f,-17));
            yield return MeasureSpeed("walk diagonal",new Vector3(1,0,1).normalized,new Vector3(-4,.05f,-17));
            C("MEC-113","no diagonal speed advantage",report.speeds.Count==2&&Mathf.Abs(report.speeds[0].metresPerInputSecond-report.speeds[1].metresPerInputSecond)<.10f,"cardinal/diagonal actual controller displacement per supplied real-frame dt");
            C("SYS-055","declared run speed and controller/root-motion policy",Mathf.Abs(PanicHomeWorld.RunSpeed-5.67f)<.001f&&ControllerContract()&&body.GetComponentInChildren<Animator>()&&!body.GetComponentInChildren<Animator>().applyRootMotion,"RunSpeed constant only; actual Shift-key 5.67 m/s remains an OS measurement request.");
            yield return PlacementMatrix();
            yield return TreeActorClearance();
            report.completed=true;
        }
        IEnumerator MeasureSpeed(string id,Vector3 direction,Vector3 start)
        {
            Fixture(start,id);yield return Wait(.20f);
            float warm=Time.realtimeSinceStartup+.4f;while(Time.realtimeSinceStartup<warm){Move(direction,Dt);yield return null;}
            var p=new SpeedSample{id=id,start=w.PlayerPosition,wallSeconds=Time.realtimeSinceStartup};report.speeds.Add(p);
            float end=Time.realtimeSinceStartup+1.1f;
            while(Time.realtimeSinceStartup<end){float dt=Dt;if(Time.unscaledDeltaTime>.08f)p.clampedFrames++;p.inputSeconds+=dt;Move(direction,dt);p.frames++;yield return null;}
            p.wallSeconds=Time.realtimeSinceStartup-p.wallSeconds;p.end=w.PlayerPosition;
            Vector3 displacement=p.end-p.start;displacement.y=0;p.metresPerWallSecond=displacement.magnitude/p.wallSeconds;p.metresPerInputSecond=displacement.magnitude/p.inputSeconds;p.playback=w.CharacterPlaybackRate;
            C("MEC-113",id+" real displacement",Mathf.Abs(p.metresPerInputSecond-3.645f)<.12f&&p.frames>10,"m/s supplied frame dt="+p.metresPerInputSecond+"; wall m/s="+p.metresPerWallSecond+"; input dt="+p.inputSeconds+"; wall="+p.wallSeconds+"; clamped="+p.clampedFrames);
            C("MEC-114",id+" playback follows movement",Mathf.Abs(p.playback-1.35f)<.001f,"Animator.speed="+p.playback+"; foot sliding still requires motion review.");
            yield return Wait(.2f);
        }
        IEnumerator PlacementMatrix()
        {
            var item=w.Model.Items.Single(i=>i.Id=="up_cabinet");var source=w.Expedition.Sources.Single(s=>s.Id==item.Id);Vector3 original=item.Position;float rotation=item.Rotation;int budget=w.Model.Budget;
            Fixture(new Vector3(3.1f,3.28f,-3.25f),"placement upper cabinet");yield return Wait(.4f);
            var visual=FindObjectsByType<HomePick>(FindObjectsSortMode.None).Single(p=>p.Id==item.Id).gameObject;var colliders=visual.GetComponentsInChildren<Collider>(true);bool[] enabledStates=colliders.Select(c=>c.enabled).ToArray();
            w.SetFocus(item.Id);Unlocked(()=>{w.BeginMove();return w.IsMoving;});
            C("SYS-019","begin move without altering original physics",w.IsMoving&&colliders.Select((c,i)=>c.enabled==enabledStates[i]).All(v=>v),"Original renderers can hide, physical furniture volume stays solid.");
            C("SYS-019","same-frame commit refused",!Unlocked(()=>w.ConfirmMove()),"Commit cannot share begin frame.");yield return null;
            var candidates=new[]{new{label="wrong floor",p=new Vector3(4.05f,0,-4.5f)},new{label="stair void",p=new Vector3(0,3.2f,2.5f)},new{label="bathroom wall",p=new Vector3(5,3.2f,1.8f)},new{label="balcony door sweep",p=new Vector3(0,3.2f,-5.1f)},new{label="actor capsule",p=new Vector3(w.PlayerPosition.x,3.2f,w.PlayerPosition.z)},new{label="outside house",p=new Vector3(9,3.2f,0)}};
            foreach(var p in candidates){string reason;bool valid=w.ValidatePlacement(item,p.p,rotation,out reason);C("SYS-019","reject "+p.label,!valid&&item.Position==original&&source.Position==original&&w.Model.Budget==budget,reason);}
            Vector3 validPosition=new Vector3(4.05f,3.2f,-4.5f);Vector3 screen=w.View.WorldToScreenPoint(validPosition);
            bool pointer=w.UpdatePlacementPointer(screen,false);w.RotatePlacement(45);bool rotated=w.PlacementValid;
            C("SYS-019","valid production screen ray and rotation",pointer&&rotated&&Mathf.Abs(w.PlacementFloorY-3.2f)<.001f,w.PlacementMessage);
            bool hudBlocked=!w.UpdatePlacementPointer(screen,true)&&!Unlocked(()=>w.ConfirmMove());
            C("SYS-019","HUD confirmation blocked without state loss",hudBlocked&&w.IsMoving&&item.Position==original&&w.Model.Budget==budget,w.PlacementMessage);
            w.CancelMove();yield return null;
            C("SYS-019","cancel restores pose/source/colliders",!w.IsMoving&&item.Position==original&&item.Rotation==rotation&&source.Position==original&&visual.transform.position==original&&colliders.Select((c,i)=>c.enabled==enabledStates[i]).All(v=>v)&&w.Model.Budget==budget,"Original exact transform and source remain.");
            yield return Shot("placement-cancel-upper-floor");
        }
        IEnumerator TreeActorClearance()
        {
            w.StartDisasterSimulation(DisasterType.Flood);report.simulationStartWarps++;yield return Wait(.2f);
            Fixture(new Vector3(-4,-3.15f,3),"real basement pantry kit collection");yield return Wait(.25f);w.SetFocus("basement_pantry");w.HandleKey(KeyCode.E);yield return null;
            var pantry=w.OpenSource;if(pantry==null)throw new InvalidOperationException("Real pantry not interactable for finite tree kits.");
            int index=pantry.Contents.FindIndex(s=>s.Id=="seedling"&&s.Count>0);if(index<0)throw new InvalidOperationException("No finite adult tree-row kit stock.");w.SelectLoot(index);w.HandleKey(KeyCode.F);yield return null;
            C("MEC-116","tree kits collected from actual finite stock",w.Expedition.Count("seedling")==4,"inventory="+w.Expedition.Count("seedling"));
            // Seat 1 is 1.05m from the site: a reachable place at which a real player can start E.
            Vector3 site=new Vector3(-11,0,4),seat=site+PanicConstructionArt.TreeTrunkSeats(0)[1];
            Fixture(seat+Vector3.right*.85f+Vector3.up*.05f,"tree seat approach setup");yield return Wait(.2f);
            yield return WalkTo(seat);yield return Wait(.15f);w.SetFocus("plant_west1");
            C("MEC-116","prospective trunk collision volume reachable through physical movement",Vector2.Distance(new Vector2(w.PlayerPosition.x,w.PlayerPosition.z),new Vector2(seat.x,seat.z))<body.radius+.12f-.006f&&w.CanInteract&&w.IsGrounded,"actor="+w.PlayerPosition+"; future trunk="+seat+"; precondition is capsule overlap volume, not exact trunk center");
            Vector3 actor=w.PlayerPosition;int kitBefore=w.Expedition.Count("seedling"),budgetBefore=w.Model.Budget;w.HandleKey(KeyCode.E);bool began=w.WorkActive;
            float until=Time.realtimeSinceStartup+1.0f;
            while(w.WorkActive&&Time.realtimeSinceStartup<until){w.StepWork(Dt,0,false,false,false);report.manualWorkTickFrames++;Physics.SyncTransforms();report.treeMaxPenetration=Mathf.Max(report.treeMaxPenetration,TrunkOverlap());yield return null;}
            Physics.SyncTransforms();report.treeMaxPenetration=Mathf.Max(report.treeMaxPenetration,TrunkOverlap());
            bool committed=w.Expedition.IsWorkCommitted("plant","plant_west1");
            C("MEC-116","tree installation never commits a trunk inside actor",(!began&&!committed&&kitBefore==w.Expedition.Count("seedling")&&budgetBefore==w.Model.Budget)||(began&&committed&&report.treeMaxPenetration<.006f),"began="+began+"; committed="+committed+"; independent enabled capsule max overlap="+report.treeMaxPenetration+"; actor="+actor+"; kits="+kitBefore+"/"+w.Expedition.Count("seedling")+"; budget="+budgetBefore+"/"+w.Model.Budget);
            yield return Shot("tree-install-at-prospective-trunk-seat");
            for(int i=0;i<10;i++){w.SmokeMove(Vector2.zero,Dt);report.treeActorDrift=Mathf.Max(report.treeActorDrift,Vector3.Distance(actor,w.PlayerPosition));yield return null;}
            C("MEC-116","new tree collider does not push stationary actor",report.treeActorDrift<.025f,"drift after real controller zero-input moves="+report.treeActorDrift);
        }
        float TrunkOverlap()
        {
            Physics.SyncTransforms();float max=0;
            foreach(var trunk in FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None).Where(c=>c.name=="Tree trunk collision"&&c.enabled&&!c.isTrigger))
            {Vector3 direction;float distance;if(Physics.ComputePenetration(trunk,trunk.transform.position,trunk.transform.rotation,query,body.transform.position,body.transform.rotation,out direction,out distance))max=Mathf.Max(max,distance);}
            return max;
        }
        bool ControllerContract()=>body&&body.enabled&&Mathf.Abs(body.height-1.78f)<.001f&&Vector3.Distance(body.center,new Vector3(0,.9f,0))<.001f&&Mathf.Abs(body.radius-.25f)<.001f&&Mathf.Abs(body.stepOffset-.25f)<.001f&&Mathf.Abs(body.skinWidth-.025f)<.001f&&Mathf.Abs(body.slopeLimit-48)<.001f;
        void Fixture(Vector3 p,string purpose){report.explicitSetupWarps++;report.fixtures.Add(purpose+"; "+p);w.Warp(p);Physics.SyncTransforms();}
        bool Unlocked(Func<bool> action){bool previous=w.InputLocked;w.InputLocked=false;try{return action();}finally{w.InputLocked=previous;}}
        void Move(Vector3 direction,float dt){Vector3 f=w.View.transform.forward;f.y=0;f.Normalize();Vector3 r=w.View.transform.right;r.y=0;r.Normalize();w.SmokeMove(new Vector2(Vector3.Dot(direction,r),Vector3.Dot(direction,f)),dt);}
        IEnumerator WalkTo(Vector3 target){float until=Time.realtimeSinceStartup+3f;while(Time.realtimeSinceStartup<until){Vector3 d=target-w.PlayerPosition;d.y=0;if(d.magnitude<.025f)yield break;Move(Vector3.ClampMagnitude(d/.35f,1),Dt);yield return null;}}
        IEnumerator Wait(float seconds){float until=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<until){w.SmokeMove(Vector2.zero,Dt);yield return null;}}
        IEnumerator Shot(string label){yield return new WaitForEndOfFrame();string path=Path.Combine(w.EvidencePath,label+".png");ScreenCapture.CaptureScreenshot(path);report.screenshots.Add(path);float until=Time.realtimeSinceStartup+2;while(Time.realtimeSinceStartup<until){if(File.Exists(path)&&new FileInfo(path).Length>256)yield break;yield return null;}report.errors.Add("Missing actual screenshot "+path);}
        void C(string id,string name,bool passed,string detail){report.checks.Add(new Check{caseId=id,name=name,passed=passed,detail=detail});Debug.Log("E2E_PHYSICS_CHECK "+(passed?"PASS ":"FAIL ")+id+" "+name+" "+detail);}
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(message+"\n"+stack);}
        void Update(){if(!finished&&Time.realtimeSinceStartup-started>HardCap){report.errors.Add("250-second watchdog; unexecuted checks not accepted.");if(sequence!=null)StopCoroutine(sequence);Finish();}}
        void Finish()
        {
            if(finished)return;finished=true;report.finishedUtc=DateTime.UtcNow.ToString("o");report.runtimeSeconds=Time.realtimeSinceStartup-started;
            report.passed=report.completed&&report.errors.Count==0&&report.checks.Count==24&&report.checks.All(c=>c.passed)&&report.screenshots.Count==2;
            if(query)Destroy(query.gameObject);Application.logMessageReceived-=Log;
            if(w&&!string.IsNullOrEmpty(w.EvidencePath))File.WriteAllText(Path.Combine(w.EvidencePath,ReportName),JsonUtility.ToJson(report,true));
            Debug.Log("E2E_PHYSICS_AUDIT "+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count+" errors="+report.errors.Count);Application.Quit(report.passed?0:1);
        }
    }
}
