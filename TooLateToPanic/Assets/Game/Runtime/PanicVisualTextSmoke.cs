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
    public sealed class PanicVisualTextSmoke : MonoBehaviour
    {
        [Serializable] public sealed class Row { public string id,detail;public bool pass; }
        [Serializable] public sealed class VisibilitySample { public string region,view,occluders;public bool gate;public int activeCue,changedPixels,outsideActorPixels;public Rect actorBounds;public float yaw,pitch,zoom;public Vector3 actor,camera; }
        [Serializable] public sealed class MarkerSample { public string size,edge;public List<Rect> bounds;public List<string> labels; }
        [Serializable] public sealed class MotionSample { public string region;public float direction,travel;public int frames;public Vector3 start,end; }
        [Serializable] public sealed class Report {
            public bool completed,passed;public string error,startedUtc,finishedUtc,scoreBefore,scoreAfter,realBefore,realAfter;
            public float seconds;public int setupWarps,stockFixtures,workTickCalls,movementCalls;
            public string layer="Native production APIs + explicit position/stock/time/render fixtures + genuine IMGUI callback with logical pointer. No OS input or human acceptance.";
            public List<Row> checks=new List<Row>();public List<string> errors=new List<string>(),captures=new List<string>();
            public List<VisibilitySample> visibility=new List<VisibilitySample>();public List<MarkerSample> markers=new List<MarkerSample>();
            public List<MotionSample> motion=new List<MotionSample>();
        }
        readonly Report report=new Report();PanicHomeWorld w;PanicHomeHud hud;PanicActorOcclusion cue;PanicInteractionAudit pointer;
        string file,score,real;float began;bool finished,pending,markers,terminalMarker,fixedCamera;Vector2 point;float markerX=.98f,markerY=.82f,yaw,pitch=40,zoom=5.7f;
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static readonly FieldInfo Active=typeof(PanicInteractionAudit).GetField("activeGuiFixture",BindingFlags.Static|BindingFlags.NonPublic),Point=typeof(PanicInteractionAudit).GetField("queuedLogicalPointer",Hidden);
        static readonly MethodInfo Gui=typeof(PanicHomeHud).GetMethod("OnGUI",Hidden);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"--visual-text-smoke")>=0){var w=FindFirstObjectByType<PanicHomeWorld>();if(w)w.gameObject.AddComponent<PanicVisualTextSmoke>();}}
        IEnumerator Start(){
            w=GetComponent<PanicHomeWorld>();hud=GetComponent<PanicHomeHud>();cue=GetComponent<PanicActorOcclusion>();began=Time.realtimeSinceStartup;Application.runInBackground=true;
            if(!w||string.IsNullOrEmpty(w.EvidencePath)){Application.Quit(2);yield break;}
            file=Path.Combine(w.EvidencePath,"visual-text.json");score=PanicModel.ScorePathOverride;real=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");
            report.startedUtc=DateTime.UtcNow.ToString("o");report.scoreBefore=Hash(score);report.realBefore=Hash(real);Application.logMessageReceived+=Log;
            pointer=gameObject.AddComponent<PanicInteractionAudit>();pointer.enabled=false;
            var stack=new Stack<IEnumerator>();stack.Push(Run());
            while(stack.Count>0&&!finished){bool more=false;object value=null;try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;}catch(Exception e){report.error=e.ToString();break;}if(!more){stack.Pop();continue;}if(value is IEnumerator nested)stack.Push(nested);else yield return value;}
            report.completed=string.IsNullOrEmpty(report.error);Finish();
        }
        IEnumerator Run(){
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;w.FullFlowScriptedInput=true;TextRules();
            C("visibility.shader",cue&&cue.ShaderSupported&&cue.OverlayCount>0,"Resources shader and retained renderer/bones cue; overlayCount="+(cue?cue.OverlayCount:0));
            foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(960,540)}){
                Screen.SetResolution(size.x,size.y,false);yield return Wait(.2f);C("resolution."+size,Screen.width==size.x&&Screen.height==size.y,"Actual framebuffer="+Screen.width+"x"+Screen.height);
                yield return MusicPause(size.x);
                yield return Anchor("fridge","anchorwall",new Vector3(6.5f,.05f,3.25f),false);
                yield return Anchor("bookshelf","anchorfloor",new Vector3(-6,.05f,3.5f),true);
                yield return MarkerCases(size.x+"x"+size.y);
                w.StartDisasterSimulation(DisasterType.Storm);w.InputLocked=true;yield return Place(new Vector3(0,.05f,-17),null);fixedCamera=true;yaw=0;pitch=40;zoom=5.7f;yield return Wait(.1f);yield return Pair("clear-control","clear",true);
                C("actor.clear-control."+size,report.visibility.Last().changedPixels==0,"Clear actual character remains pixel-identical, even if a broad AABB candidate exists");fixedCamera=false;
                foreach(string region in new[]{"plant_west1","plant_west2","plant_east"})yield return Trees(region,size.x+"x"+size.y);
                w.StartJuryDemo();w.InputLocked=true;w.JuryDemo.Tick(300,w.PlayerPosition,true,true);yield return Wait(.1f);
                var snap=w.JurySnapshot;C("summary.steps."+size,snap!=null&&snap.Summary.Contains("Tamamlanan adımlar:")&&!snap.Summary.Contains("Tamamlanan önlemler:"),snap==null?"none":snap.Summary);
                yield return Shot("summary-"+size.x);
            }
            fixedCamera=false;w.ReturnToMenu();yield return Shot("final-title");
        }
        IEnumerator MusicPause(int width){
            w.StartGame(false);w.InputLocked=false;yield return Wait(.15f);var pause=typeof(PanicHomeHud).GetMethod("SetPaused",Hidden);pause.Invoke(hud,new object[]{true});
            yield return Wait(.05f);float clock=w.Model.TimeRemaining;bool before=PanicBackgroundMusic.Enabled;
            point=new Vector2(1172,615);pending=true;float until=Time.realtimeSinceStartup+1;while(pending&&Time.realtimeSinceStartup<until)yield return null;
            C("music.pause-actual-toggle."+width,!pending&&PanicBackgroundMusic.Enabled!=before,"Actual DrawPause button callback through declared logical pointer; no OS input");yield return Wait(.05f);
            C("music.pause-clock."+width,w.InputLocked&&Mathf.Abs(w.Model.TimeRemaining-clock)<.001f,"Music toggle preserves pause lock and preparation clock");yield return Shot("music-pause-"+width);
            point=new Vector2(1172,615);pending=true;until=Time.realtimeSinceStartup+1;while(pending&&Time.realtimeSinceStartup<until)yield return null;
            C("music.pause-toggle-restored."+width,!pending&&PanicBackgroundMusic.Enabled==before,"Actual second callback restores session setting; no PlayerPrefs or score writes");
            pause.Invoke(hud,new object[]{false});yield return Wait(.1f);C("music.pause-resume-clock."+width,!w.InputLocked&&w.Model.TimeRemaining<clock,"Existing resume clock/input contract retained");w.ReturnToMenu();yield return null;
        }
        void TextRules(){
            foreach(var pair in new[]{new KeyValuePair<PanicJuryStage,string>(PanicJuryStage.Onboarding,"İLK ADIMLAR"),new KeyValuePair<PanicJuryStage,string>(PanicJuryStage.Earthquake,"SARSINTI SÜRÜYOR"),new KeyValuePair<PanicJuryStage,string>(PanicJuryStage.EarthquakeResult,"SONUCU İNCELE"),new KeyValuePair<PanicJuryStage,string>(PanicJuryStage.Inspection,"EVİ İNCELE")})
                C("phase-copy."+pair.Key,PanicHomeHud.JuryStagePrompt(pair.Key,PanicJuryShelter.None)==pair.Value,"Actual shared HUD prompt="+PanicHomeHud.JuryStagePrompt(pair.Key,PanicJuryShelter.None));
            C("phase-copy.shelter",PanicHomeHud.JuryStagePrompt(PanicJuryStage.Preparation,PanicJuryShelter.QuakeCover).Contains("İŞARETLİ"),"Real preparation shelter prompt retained");
            try{PanicModel.ScorePathOverride=Path.Combine(w.EvidencePath,"text-"+Guid.NewGuid()+".json");var model=new PanicModel();model.StartRun(true);model.FinishTutorial();C("tutorial.dalga",model.Message.Contains("dalga")&&!model.Message.Contains("wave"),model.Message);}finally{PanicModel.ScorePathOverride=score;}
        }
        IEnumerator Place(Vector3 position,string focus){report.setupWarps++;w.Warp(position);float end=Time.realtimeSinceStartup+.20f;while(Time.realtimeSinceStartup<end){w.SmokeMove(Vector2.zero,Time.deltaTime);yield return null;}w.SmokeFocus(focus);yield return null;}
        void Step(int frames){for(int i=0;i<frames;i++){w.StepWork(.1f,0,false,false,false);report.workTickCalls++;}}
        IEnumerator Anchor(string id,string action,Vector3 position,bool longName){
            fixedCamera=false;w.StartDisasterSimulation(DisasterType.Earthquake);w.InputLocked=true;yield return Place(position,id);w.BeginWork(action,id);yield return Wait(.08f);
            C("anchor.begin."+id,w.WorkActive&&w.Expedition.Work.TotalSteps==2,"Real reachable funded target "+id+"; "+w.Model.Message);
            if(!w.WorkActive)yield break;
            var item=w.Model.Items.Find(i=>i.Id==id);if(longName)item.Label="Salonun uzun kitaplığı · gerçek sabitleme hedefi";
            yield return Wait(.04f);
            C("anchor.identity."+id,hud.CurrentAnchorTargetLabel()==item.Label&&hud.CurrentAnchorSurfaceLabel()==(action=="anchorwall"?"DUVAR ANKRAJI":"ZEMİN ANKRAJI"),hud.CurrentAnchorTargetLabel()+" / "+hud.CurrentAnchorSurfaceLabel());
            C("anchor.text-fit."+id,hud.AnchorTargetTextHeight<=64.1f&&hud.AnchorTargetFontSize>=18,"Native font="+hud.AnchorTargetFontSize+"; measured logical height="+hud.AnchorTargetTextHeight+"; canvasScale="+hud.CanvasScale);
            yield return Shot("anchor-"+Screen.width+"-"+id+"-ready");
            int cash=w.Model.Budget;yield return Click(0);Step(3);yield return Shot("anchor-"+Screen.width+"-"+id+"-middle");
            C("anchor.no-early-debit."+id,w.Expedition.Work.IsInstalling&&w.Model.Budget==cash&&w.Expedition.Count("screws")==2,"Target name/render leaves exact recipe/cost deferred");
            Step(3);yield return Click(1);Step(6);yield return null;
            C("anchor.commit."+id,item.Anchored&&w.Model.Budget==cash-60&&w.Expedition.Count("screws")==0&&w.Expedition.Count("bracket")==0,"Both actual projected screw callbacks and canonical debit");
            yield return Shot("anchor-"+Screen.width+"-"+id+"-complete");
        }
        IEnumerator MarkerCases(string size){
            fixedCamera=false;w.StartGame(false);w.InputLocked=true;yield return Wait(.1f);markers=true;
            foreach(var edge in new[]{new Vector2(.98f,.82f),new Vector2(.02f,.82f),new Vector2(.98f,.1f),new Vector2(.02f,.1f)}){
                markerX=edge.x;markerY=edge.y;yield return Wait(.05f);yield return new WaitForEndOfFrame();
                var bounds=hud.CurrentMarkerBounds.ToList();var labels=hud.CurrentMarkerLabels.ToList();bool free=bounds.Count==3&&labels.Count==3;
                for(int i=0;i<bounds.Count;i++){free&=bounds[i].xMin>=12&&bounds[i].xMax<=1588&&bounds[i].yMin>=142&&bounds[i].yMax<=835;for(int j=0;j<i;j++)free&=!bounds[i].Overlaps(bounds[j]);}
                C("marker.reserve."+size+"."+edge,free,"Native same-frame damage-label fixture: all three actual supplied values retained; bounds="+string.Join(";",bounds));
                report.markers.Add(new MarkerSample{size=size,edge=edge.ToString(),bounds=bounds,labels=labels});
                if(edge.x>.9f&&edge.y>.5f)yield return Shot("markers-"+size);
            }
            markers=false;terminalMarker=true;w.Model.Phase=PanicPhase.GameOver;typeof(PanicHomeWorld).GetField("previousPhase",Hidden).SetValue(w,PanicPhase.GameOver);
            yield return Wait(.05f);yield return new WaitForEndOfFrame();C("terminal.no-E."+size,hud.CurrentMarkerBounds.Count==0,"Actual HUD marker refuses E prompt in terminal phase; explicit phase fixture");terminalMarker=false;w.ReturnToMenu();
        }
        IEnumerator Trees(string region,string size){
            fixedCamera=false;w.StartDisasterSimulation(DisasterType.Flood);w.InputLocked=true;
            Vector3 position=region=="plant_west1"?new Vector3(-10,.05f,4):region=="plant_west2"?new Vector3(-12.3f,.05f,-1.4f):new Vector3(11,.05f,8);
            yield return Place(position,region);foreach(var s in w.Expedition.Inventory){s.Id="";s.Count=0;}w.Expedition.Inventory[0]=new SupplySlot("seedling",1);report.stockFixtures++;
            w.HandleKey(KeyCode.E);Step(6);yield return Wait(.1f);
            C("tree.physical."+region,w.InstalledTreeRows==1&&w.Model.TreeCount==1&&FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None).Count(c=>c.name=="Tree trunk collision")==4,"Four original solid trunks, one kit and30 budget; no collider removal");
            yield return Shot("tree-"+size+"-"+region+"-construction");yield return Wait(1.6f);
            int visible=0;yield return Pair(region,"normal",true);if(report.visibility.Last().changedPixels>8)visible++;
            fixedCamera=true;
            foreach(float y in new[]{0f,90f,180f,270f})foreach(float p in new[]{28f,65f})foreach(float z in new[]{4f,10f}){
                yaw=y;pitch=p;zoom=z;yield return Wait(.045f);yield return Pair(region,"orbit",false);if(report.visibility.Last().changedPixels>8)visible++;
            }
            C("actor.visible-when-occluded."+region,visible>0,"Native exact-pose cue-off/cue-on pixel comparisons with retained geometry; affected views="+visible+"; visual reviewer decides quality");
            if(Screen.width==1600)
            {
                fixedCamera=false;yield return Wait(.1f);Vector3 start=w.PlayerPosition;float regionTravel=0;
                foreach(float sign in new[]{1f,-1f})
                {
                    var motion=new MotionSample{region=region,direction=sign,start=w.PlayerPosition};
                    float end=Time.realtimeSinceStartup+.35f;
                    while(Time.realtimeSinceStartup<end)
                    {
                        Vector3 forward=w.View.transform.forward;forward.y=0;forward.Normalize();Vector3 right=w.View.transform.right;right.y=0;right.Normalize();Vector3 direction=Vector3.forward*sign;
                        Vector3 prior=w.PlayerPosition;w.SmokeMove(new Vector2(Vector3.Dot(direction,right),Vector3.Dot(direction,forward)),Mathf.Min(Time.deltaTime,.04f));Vector3 delta=w.PlayerPosition-prior;delta.y=0;motion.travel+=delta.magnitude;motion.frames++;report.movementCalls++;yield return null;
                    }
                    motion.end=w.PlayerPosition;report.motion.Add(motion);regionTravel+=motion.travel;
                    yield return Pair(region,sign>0?"walking-forward":"walking-return",false);
                    if(sign>0)yield return Shot("tree-1600-"+region+"-walking");
                }
                C("actor.actual-movement."+region,regionTravel>.03f&&w.PlayerControllerEnabled,"Actual measured horizontal controller travel="+regionTravel+"m; no Warp during either movement segment. Paired renders same frame, no OS/run/jump acceptance; start="+start+" end="+w.PlayerPosition);
            }
            fixedCamera=false;w.ReturnToMenu();yield return null;
        }
        void LateUpdate(){if(!fixedCamera||!w||!w.View)return;Vector3 aim=w.PlayerPosition+Vector3.up*.9f;Vector3 offset=Quaternion.Euler(pitch,yaw,0)*Vector3.back*20;w.View.transform.position=aim+offset;w.View.transform.LookAt(aim);w.View.orthographic=true;w.View.orthographicSize=zoom;}
        IEnumerator Pair(string region,string view,bool save){
            yield return new WaitForEndOfFrame();var renderers=cue.Overlays.ToArray();var states=renderers.Select(r=>r.enabled).ToArray();
            Color32[] before=null,after=null;
            try{for(int i=0;i<renderers.Length;i++)renderers[i].enabled=false;before=WorldPixels(save?region+"-"+Screen.width+"-cue-off":null);for(int i=0;i<renderers.Length;i++)renderers[i].enabled=states[i];after=WorldPixels(save?region+"-"+Screen.width+"-cue-on":null);}
            finally{for(int i=0;i<renderers.Length;i++)if(renderers[i])renderers[i].enabled=states[i];}
            Bounds actorBounds=new Bounds(w.PlayerPosition+Vector3.up*.9f,new Vector3(.9f,2f,.9f));
            var body=FindObjectsByType<CharacterController>(FindObjectsSortMode.None).First(c=>c.name=="Survivor");
            foreach(var renderer in body.GetComponentsInChildren<Renderer>())if(!renderer.name.StartsWith("Actor visibility",StringComparison.Ordinal)&&renderer.enabled)actorBounds.Encapsulate(renderer.bounds);
            Vector2 min=Vector2.one*float.MaxValue,max=Vector2.one*float.MinValue;
            for(int k=0;k<8;k++){Vector3 corner=actorBounds.center+Vector3.Scale(actorBounds.extents,new Vector3((k&1)==0?-1:1,(k&2)==0?-1:1,(k&4)==0?-1:1));Vector3 v=w.View.WorldToViewportPoint(corner);min=Vector2.Min(min,new Vector2(v.x*640,v.y*360));max=Vector2.Max(max,new Vector2(v.x*640,v.y*360));}
            Rect projected=Rect.MinMaxRect(min.x-6,min.y-6,max.x+6,max.y+6);int changed=0,outside=0;
            for(int i=0;i<before.Length;i++)if(Math.Abs(before[i].r-after[i].r)+Math.Abs(before[i].g-after[i].g)+Math.Abs(before[i].b-after[i].b)>30){changed++;if(!projected.Contains(new Vector2(i%640,i/640)))outside++;}
            report.visibility.Add(new VisibilitySample{region=region,view=view,gate=cue.IsOccluded,activeCue=cue.ActiveOverlayCount,changedPixels=changed,outsideActorPixels=outside,actorBounds=projected,yaw=yaw,pitch=pitch,zoom=zoom,actor=w.PlayerPosition,camera=w.View.transform.position,occluders=string.Join(";",cue.Occluders)});
            C("actor.depth-only."+region+"."+report.visibility.Count,(cue.ActiveOverlayCount>0||changed==0)&&outside<=4,"640x360 native world pair, same pose/frame; changed="+changed+"; outside retained actor projection="+outside+"; gate="+cue.IsOccluded+"; not OS/human proof");
            if(save)yield return Shot("tree-"+Screen.width+"-"+region+"-normal");Flush();
        }
        Color32[] WorldPixels(string name){
            var rt=new RenderTexture(640,360,24);var texture=new Texture2D(640,360,TextureFormat.RGB24,false);var prior=RenderTexture.active;var cameraPrior=w.View.targetTexture;
            try{w.View.targetTexture=rt;w.View.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,640,360),0,0);texture.Apply();if(name!=null){string path=Path.Combine(w.EvidencePath,name+".png");File.WriteAllBytes(path,texture.EncodeToPNG());report.captures.Add(path);}return texture.GetPixels32();}
            finally{w.View.targetTexture=cameraPrior;RenderTexture.active=prior;rt.Release();Destroy(rt);Destroy(texture);}
        }
        IEnumerator Click(int index){Rect r=hud.WorkClickTargetBounds(index);C("screw.hitrect."+index,r.width==76&&r.height==76,"Actual projected76x76 logical region retained");point=r.center;pending=true;float end=Time.realtimeSinceStartup+1;while(pending&&Time.realtimeSinceStartup<end)yield return null;C("screw.callback."+index,!pending&&w.Expedition.Work.IsInstalling,"Actual OnGUI with declared logical pointer");yield return null;}
        void OnGUI(){
            if(!w||!hud||Event.current.type!=EventType.Repaint)return;
            if(markers)for(int i=0;i<3;i++){Vector3 p=w.View.ViewportToWorldPoint(new Vector3(markerX,markerY,12));hud.DrawWorldMarker(p,"!  "+new[]{"Kitaplık · +12,3%","Buzdolabı · +8,4%","Dolap · +5,2%"}[i],180);}
            if(terminalMarker)hud.DrawWorldMarker(w.PlayerPosition+Vector3.up,"E  Aç",110);
            if(!pending)return;pending=false;var prior=new Event(Event.current);var matrix=GUI.matrix;bool locked=w.InputLocked;
            try{w.InputLocked=false;GUI.matrix=Matrix4x4.identity;Point.SetValue(pointer,point);Active.SetValue(null,pointer);Event.current=new Event{type=EventType.MouseDown,button=0,mousePosition=point};Gui.Invoke(hud,null);}
            finally{w.InputLocked=locked;Active.SetValue(null,null);Event.current=prior;GUI.matrix=matrix;}
        }
        IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
        IEnumerator Shot(string name){yield return new WaitForEndOfFrame();string p=Path.Combine(w.EvidencePath,name+".png");ScreenCapture.CaptureScreenshot(p);report.captures.Add(p);yield return null;Flush();}
        void C(string id,bool pass,string detail){report.checks.Add(new Row{id=id,pass=pass,detail=detail});Debug.Log("VISUAL_TEXT "+(pass?"PASS ":"FAIL ")+id+" "+detail);}
        void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(text+"\n"+stack);}
        void Flush(){if(file!=null)File.WriteAllText(file,JsonUtility.ToJson(report,true));}
        void Update(){if(!finished&&Time.realtimeSinceStartup-began>180){report.error="180s visual watchdog";Finish();}}
        void Finish(){if(finished)return;finished=true;Application.logMessageReceived-=Log;report.scoreAfter=Hash(score);report.realAfter=Hash(real);C("scores.preserved",report.scoreBefore==report.scoreAfter&&report.realBefore==report.realAfter,"Isolated and real scores only hash-read; no real writes");report.passed=report.completed&&report.errors.Count==0&&string.IsNullOrEmpty(report.error)&&report.checks.All(c=>c.pass);report.seconds=Time.realtimeSinceStartup-began;report.finishedUtc=DateTime.UtcNow.ToString("o");Flush();Application.Quit(report.passed?0:1);}
        static string Hash(string p){if(!File.Exists(p))return "absent";using(var s=File.OpenRead(p))using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(s)).Replace("-","");}
    }
}
