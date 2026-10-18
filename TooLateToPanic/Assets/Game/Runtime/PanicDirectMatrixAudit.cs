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
    // Opt-in background audit. Explicit model/position/logical-pointer fixtures, never OS input.
    public sealed class PanicDirectMatrixAudit : MonoBehaviour
    {
        [Serializable] public sealed class Case { public string id,group,target,status,actual,evidenceLayer; }
        [Serializable] public sealed class Matrix { public List<Case> cases; }
        [Serializable] public sealed class Report {
            public string startedUtc,finishedUtc,scoreBefore,scoreAfter,realScoreBefore,realScoreAfter;
            public bool completed; public int setupWarps,modelFixtures,explicitStockFixtures; public float seconds;
            public List<string> supplementary=new List<string>();
            public string limitations="Production model/API and genuine IMGUI callbacks with explicit logical pointer. Fixture stock/position/time are declared. No OS input, raw platform pointer, human listening/fun or full campaign acceptance.";
            public List<Case> cases; public List<string> errors=new List<string>(),captures=new List<string>();
        }
        sealed class Fixture { public PanicModel M; public PanicExpedition E; }
        PanicHomeWorld w; PanicHomeHud hud; PanicInteractionAudit pointer;
        readonly Report report=new Report(); Matrix matrix; string path,callerScore,realScore;
        bool pending,finished; Vector2 click; EventType kind=EventType.MouseDown; int button;
        float started; Case current;
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static readonly FieldInfo Active=typeof(PanicInteractionAudit).GetField("activeGuiFixture",BindingFlags.Static|BindingFlags.NonPublic);
        static readonly FieldInfo Point=typeof(PanicInteractionAudit).GetField("queuedLogicalPointer",Hidden);
        static readonly MethodInfo Gui=typeof(PanicHomeHud).GetMethod("OnGUI",Hidden);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"--direct-matrix-audit")>=0){var w=FindFirstObjectByType<PanicHomeWorld>();if(w)w.gameObject.AddComponent<PanicDirectMatrixAudit>();}}
        IEnumerator Start(){
            w=GetComponent<PanicHomeWorld>();hud=GetComponent<PanicHomeHud>();started=Time.realtimeSinceStartup;Application.runInBackground=true;
            if(!w||string.IsNullOrEmpty(w.EvidencePath)){Debug.LogError("Matrix requires isolated evidence");Application.Quit(2);yield break;}
            path=Path.Combine(w.EvidencePath,"direct-matrix.json");callerScore=PanicModel.ScorePathOverride;realScore=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");
            report.startedUtc=DateTime.UtcNow.ToString("o");report.scoreBefore=Hash(callerScore);report.realScoreBefore=Hash(realScore);
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--case-matrix");if(i<0||i+1>=args.Length)throw new InvalidOperationException("--case-matrix required");
            matrix=JsonUtility.FromJson<Matrix>(File.ReadAllText(args[i+1]));report.cases=matrix.cases;
            foreach(var c in matrix.cases){c.status="NOT_RUN";c.actual="No applicable execution yet";}
            Application.logMessageReceived+=Log;pointer=gameObject.AddComponent<PanicInteractionAudit>();pointer.enabled=false;
            var stack=new Stack<IEnumerator>();stack.Push(Run());
            while(stack.Count>0&&!finished){bool more=false;object value=null;try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;}catch(Exception e){report.errors.Add(e.ToString());break;}if(!more){stack.Pop();continue;}if(value is IEnumerator nested)stack.Push(nested);else yield return value;}
            report.completed=report.errors.Count==0;Finish();
        }
        IEnumerator Run(){
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            w.FullFlowScriptedInput=true;
            foreach(var c in matrix.cases.Where(c=>c.group=="direct-work-state"||c.group.StartsWith("rules-")||c.group=="economy"||c.group=="developer-mask")){
                current=c;try{RunModel(c);}catch(Exception e){Set(c,false,"Fixture exception: "+e.Message);} }
            PanicModel.ScorePathOverride=callerScore;
            foreach(var c in matrix.cases.Where(c=>c.group=="screw-hit-area"))yield return HitCase(c);
            foreach(var c in matrix.cases.Where(c=>c.group=="screw-input-negative"))yield return Negative(c);
            foreach(var c in matrix.cases.Where(c=>c.group=="screw-invalid-hit"))yield return InvalidHit(c);
            foreach(var c in matrix.cases.Where(c=>c.group=="modal-ownership"||c.group=="same-frame-critical"))yield return Gate(c);
            foreach(var c in matrix.cases.Where(c=>c.group=="construction-art"))yield return Construction(c);
            foreach(var c in matrix.cases.Where(c=>c.group=="tree-region"))yield return Tree(c);
            Warnings();
            w.ReturnToMenu();yield return null;
        }
        void RunModel(Case c){
            if(c.group=="rules-family"){
                var a=c.target.Split('/');string family=a[0],state=a[1];bool installed=state!="uninstalled",movable=state!="immovable",anchored=state.StartsWith("anchored"),near=!state.Contains("away");
                var item=new PanicItem{Id=family,ArtId=family,Kind=ItemKind.Furniture,Installed=installed,Movable=movable,Anchored=anchored,Position=new Vector3(near?7:0,0,0)};
                bool eligible=new[]{"bookshelf","fridge","cabinet","kitchencabinet","tvstand","locker","storagerack"}.Contains(family)&&installed&&movable&&!anchored;
                string expected=eligible?(near?"anchorwall":"anchorfloor"):"";
                bool ok=PanicItemActionRules.PrimaryAction(item)==expected&&PanicItemActionRules.PrimaryLabel(item)==(eligible?"SABİTLE":"")&&PanicItemActionRules.PrimaryShortcut(item)==(eligible?1:0)&&!PanicItemActionRules.Allows("fire",item,item.Id)&&!PanicItemActionRules.Allows("upgrade",item,item.Id)&&PanicItemActionRules.Allows("anchorfloor",item,item.Id)==eligible&&PanicItemActionRules.Allows("anchorwall",item,item.Id)==(eligible&&near);
                Set(c,ok,"Actual shared Rules primary/label/shortcut and allowed/denied actions checked. World/GUI for synthetic family variants remains unexecuted.","PASS_MODEL_ONLY");return;
            }
            if(c.group=="rules-structure"){
                var a=c.target.Split('/');var item=new PanicItem{Id="fixture",Kind=(ItemKind)Enum.Parse(typeof(ItemKind),a[0]),Tier=int.Parse(a[1].Substring(4)),Installed=bool.Parse(a[2].Substring(9))};bool yes=item.Installed&&item.Tier<2;
                Set(c,PanicItemActionRules.PrimaryAction(item)==(yes?"upgrade":"")&&PanicItemActionRules.PrimaryShortcut(item)==(yes?3:0)&&PanicItemActionRules.PrimaryLabel(item)==(yes?"GÜÇLENDİR":"")&&PanicItemActionRules.Allows("upgrade",item,item.Id)==yes&&!PanicItemActionRules.Allows("anchorwall",item,item.Id)&&!PanicItemActionRules.Allows("fire",item,item.Id),"Rules full structure/tier/installed matrix; synthetic variants not all driven through rendered World.","PASS_MODEL_ONLY");return;
            }
            if(c.group=="rules-fixed-crossproduct"){
                var a=c.target.Split('/');string action=a[1]=="empty"?"":a[1];string owner=a[0].StartsWith("plant_")?"plant":a[0].StartsWith("drain_")?"drain":a[0].StartsWith("fence_")?"fence":a[0]=="fire_station"?"fire":"hose";
                Set(c,PanicItemActionRules.Allows(action,null,a[0])==(action==owner),"Allows="+PanicItemActionRules.Allows(action,null,a[0])+"; expected owner="+owner);return;
            }
            if(c.group=="rules-denial"){
                if(new[]{"wrong floor","closed door behind target","out of range"}.Contains(c.target)){c.actual="Requires World proximity/LOS fixture, not implied by pure model";return;}
                var f=New();var it=f.M.Items.Find(x=>x.Id=="bookshelf");string action="anchorwall",id=it.Id;
                if(c.target=="null item and unknown ID")id="unknown";
                if(c.target=="item.Id != targetId"){Set(c,!PanicItemActionRules.Allows(action,it,"other"),"Mismatched identity denies in shared rules","PASS_MODEL_ONLY");return;}
                if(c.target=="known ID removed from model")f.M.Items.Remove(it);
                if(c.target=="uninstalled target")it.Installed=false;
                if(c.target=="already anchored")it.Anchored=true;
                if(c.target=="tier2"){it=f.M.Items.First(x=>x.Kind==ItemKind.Wall);id=it.Id;it.Tier=2;action="upgrade";}
                if(c.target=="wrong near-wall side")it.Position=Vector3.zero;
                Fill(f,action,it);string before=Stock(f);int budget=f.M.Budget;Set(c,!f.E.CanBeginWork(action,id)&&before==Stock(f)&&budget==f.M.Budget,"Expedition authoritative deny + stock/budget unchanged; GUI/World variants not all run","PASS_MODEL_ONLY");return;
            }
            if(c.group=="economy"||c.group=="developer-mask"){Economy(c);return;}
            if(c.group=="direct-work-state"){Work(c);return;}
        }
        Fixture New(){PanicModel.ScorePathOverride=Path.Combine(w.EvidencePath,"matrix-model",(++report.modelFixtures)+".json");var m=new PanicModel();m.StartRun(false);return new Fixture{M=m,E=new PanicExpedition(m)};}
        string Target(Fixture f,string a){return a=="upgrade"?f.M.Items.First(x=>x.Kind==ItemKind.Wall&&x.Installed).Id:a.StartsWith("anchor")?"bookshelf":a=="fire"?"fire_station":a=="plant"?"plant_west1":a=="drain"?"drain_west":a=="fence"?"fence_west":"hose_station";}
        void Fill(Fixture f,string a,PanicItem item){for(int i=0;i<f.E.Inventory.Count;i++)f.E.Inventory[i]=new SupplySlot();int n=0;foreach(var cost in PanicExpedition.MaterialCosts(a,item))f.E.Inventory[n++]=new SupplySlot(cost.Id,cost.Required);}
        static string Stock(Fixture f){return string.Join("|",f.E.Inventory.Select(s=>s.Id+":"+s.Count));}
        static void Tick(PanicWorkSession s,float dt){s.Tick(dt,1,true,true,true);}
        static void Drive(PanicWorkSession s){int cap=30;while(s.Active&&!s.Complete&&cap-->0){if(!s.IsInstalling)s.TryClick(s.NextClickTarget);Tick(s,.1f);}if(!s.Complete)throw new InvalidOperationException("Bounded work driver incomplete");}
        void Economy(Case c){
            var parts=c.target.Split('/');string a=parts[0],variant=parts.Length>1?parts[1]:"";int mask=0;
            if(c.group=="developer-mask"){mask=Convert.ToInt32(parts[0],2);a=parts[1];}
            var f=New();string id=Target(f,a);var item=f.M.Items.Find(x=>x.Id==id);Fill(f,a,item);int cost=PanicExpedition.WorkCost(a,item);f.M.Budget=cost;
            if(c.group=="developer-mask"){
                foreach(PanicDeveloperOption option in Enum.GetValues(typeof(PanicDeveloperOption))){int bit=option==PanicDeveloperOption.InfiniteBudget?1:option==PanicDeveloperOption.FreeMaterials?2:option==PanicDeveloperOption.FreezePreparationTime?4:8;f.M.SetDeveloperOption(option,(mask&bit)!=0);}
                bool budgetFree=(mask&1)!=0,materialFree=(mask&2)!=0;if(budgetFree)f.M.Budget=0;if(materialFree)foreach(var s in f.E.Inventory)s.Count=0;
                string before=Stock(f);bool begin=f.E.CanBeginWork(a,id);f.E.Work.Begin(a,id);Drive(f.E.Work);bool done=f.E.TryCompleteWork(a,id);
                bool ok=begin&&done&&f.M.Budget==0&&f.E.Inventory.All(s=>s.Count==0)&&f.M.CheatedRun==(mask!=0);
                float clock=f.M.TimeRemaining;f.M.Tick(.25f);ok&=(mask&4)!=0?Mathf.Abs(f.M.TimeRemaining-clock)<.0001f:f.M.TimeRemaining<clock;
                foreach(PanicDeveloperOption option in Enum.GetValues(typeof(PanicDeveloperOption)))f.M.SetDeveloperOption(option,false);
                ok&=f.M.CheatedRun==(mask!=0);f.M.ReturnToTitle();f.M.StartRun(false);ok&=!f.M.CheatedRun;
                Set(c,ok,"16-mask API fixture: independent free budget/material; work clock complete; preparation freeze; sticky off/on reset. NoLoss damage/score separately in regression.","PASS_MODEL_ONLY");return;
            }
            if(variant=="one missing material")f.E.Inventory[0].Count--;
            if(variant=="budget cost-1")f.M.Budget--;
            string initial=Stock(f);int money=f.M.Budget;bool allowed=f.E.CanBeginWork(a,id);
            if(variant=="one missing material"||variant=="budget cost-1"){Set(c,!allowed&&initial==Stock(f)&&money==f.M.Budget,"Begin denial atomic; actual budget="+money+", canonical cost="+cost,"PASS_MODEL_ONLY");return;}
            f.E.Work.Begin(a,id);Drive(f.E.Work);bool beforeUnspent=initial==Stock(f)&&money==f.M.Budget;
            if(variant=="final stock changed"){
                f.E.Inventory[0].Count=0;initial=Stock(f);Set(c,allowed&&beforeUnspent&&!f.E.TryCompleteWork(a,id)&&initial==Stock(f)&&money==f.M.Budget,"Final revalidation denied with no partial consumption/target mutation","PASS_MODEL_ONLY");return;
            }
            bool committed=f.E.TryCompleteWork(a,id);initial=Stock(f);int after=f.M.Budget;bool duplicate=!f.E.TryCompleteWork(a,id)&&initial==Stock(f)&&after==f.M.Budget;
            if(variant=="already committed target"){
                Fill(f,a,item);bool maxed=a=="upgrade"?false:!f.E.CanBeginWork(a,id);
                // A tier1 structure legitimately offers a different second upgrade. Same completed Work remains invalid.
                Set(c,committed&&duplicate&&(a=="upgrade"||maxed),"Duplicate completed session inert; upgrade tier1 is a distinct valid next upgrade","PASS_MODEL_ONLY");return;
            }
            Set(c,allowed&&beforeUnspent&&committed&&after==0&&f.E.Inventory.All(s=>s.Count==0)&&duplicate,"Exact canonical recipe and budget charged once at commit, duplicate inert","PASS_MODEL_ONLY");
        }
        void Work(Case c){
            var s=new PanicWorkSession();s.Begin("anchorwall","fixture");bool ok=true;string extra="";
            switch(c.target){
                case "begin-anchor":foreach(bool shortMode in new[]{false,true})foreach(string a in new[]{"anchorwall","anchorfloor"}){s.Begin(a,"fixture",shortMode);ok&=s.Active&&s.TotalSteps==2&&!s.Complete&&!s.IsInstalling&&s.NextClickTarget==0&&s.Progress==0;}break;
                case "begin-world":foreach(string a in new[]{"upgrade","fire","plant","drain","fence","hose"}){s.Begin(a,"fixture");ok&=s.Active&&s.TotalSteps==1&&s.IsWorldConstruction&&!s.IsInstalling&&s.NextClickTarget==0;}extra="Model only; World automatic start separately exercised";break;
                case "unsupported-begin":foreach(string a in new[]{"unknown","",null}){s.Begin(a,"fixture");ok&=!s.Active&&s.Action=="";}s.Begin("upgrade","");ok&=!s.Active;break;
                case "invalid-click":ok=!s.TryClick(-1)&&!s.TryClick(1)&&!s.TryClick(2)&&!s.TryClick(int.MaxValue)&&s.Progress==0;break;
                case "first-click":ok=s.TryClick(0)&&s.IsInstalling&&!s.TryClick(0)&&s.Progress==0;break;
                case "busy-click":s.TryClick(0);Tick(s,.2f);float p=s.Progress;for(int j=0;j<10;j++)ok&=!s.TryClick(0)&&!s.TryClick(1);ok&=s.Progress==p;break;
                case "no-input":for(int j=0;j<30;j++)Tick(s,.1f);ok=s.Progress==0&&!s.IsInstalling&&s.CompletedSteps==0;break;
                case "clock-after-click":s.TryClick(0);Tick(s,.1f);ok=s.Progress>0&&s.IsInstalling&&s.CompletedSteps==0;break;
                case "invalid-dt":s.TryClick(0);foreach(float dt in new[]{0f,-1,float.NaN,float.PositiveInfinity,float.NegativeInfinity})Tick(s,dt);ok=s.Progress==0&&s.IsInstalling;break;
                case "hitch-cap":s.TryClick(0);Tick(s,10);ok=Mathf.Abs(s.Progress-1f/12)<.0001f&&!s.Complete;break;
                case "before-threshold":case "within-epsilon":case "threshold":s.TryClick(0);for(int j=0;j<5;j++)Tick(s,.1f);float remain=c.target=="before-threshold"?.099998f:c.target=="within-epsilon"?.0999995f:.1f;Tick(s,remain);ok=c.target=="before-threshold"?s.IsInstalling&&s.CompletedSteps==0:!s.IsInstalling&&s.CompletedSteps==1&&!s.Complete;extra="Final delta="+remain.ToString("R")+"; progress="+s.Progress.ToString("R");break;
                case "second-ready":s.TryClick(0);for(int j=0;j<6;j++)Tick(s,.1f);ok=s.CompletedSteps==1&&!s.Complete&&!s.TryClick(0)&&s.TryClick(1);break;
                case "second-complete":Drive(s);ok=s.CompletedSteps==2&&s.Complete&&s.Progress==1&&s.NextClickTarget==-1&&!s.TryClick(1);break;
                case "world-complete":s.Begin("plant","fixture");Drive(s);ok=s.CompletedSteps==1&&s.Complete;break;
                case "cancel-ready":case "cancel-mid":foreach(float age in new[]{0f,.3f,.599f}){s.Begin("anchorwall","fixture");if(c.target=="cancel-mid")s.TryClick(0);float remaining=age;while(remaining>0){float dt=Mathf.Min(remaining,.1f);Tick(s,dt);remaining-=dt;}s.Cancel();ok&=!s.Active&&!s.Complete&&!s.IsInstalling&&s.Action==""&&s.TotalSteps==0&&s.Progress==0;}extra="Work state reset; no World geometry/stock claim";break;
                case "cancel-after-commit":var f=New();Fill(f,"anchorwall",f.M.Items.Find(x=>x.Id=="bookshelf"));f.E.Work.Begin("anchorwall","bookshelf");Drive(f.E.Work);ok=f.E.TryCompleteWork("anchorwall","bookshelf");int budget=f.M.Budget;f.E.Work.Cancel();ok&=!f.E.TryCompleteWork("anchorwall","bookshelf")&&budget==f.M.Budget&&!f.E.Work.TryClick(0);break;
                case "new-session":Drive(s);s.Cancel();s.Begin("anchorfloor","other");ok=s.TargetId=="other"&&s.CompletedSteps==0&&s.Progress==0&&!s.TryClick(1);extra="Session reset checked; raw queued same-index event identity not established";break;
                case "flags-not-ready":s.Cancel();ok=s.UsesDirectClick&&!s.Active&&!s.TryClick(0);s.Begin("anchorwall","fixture");s.TryClick(0);ok&=s.NextClickTarget==0&&s.IsInstalling&&!s.TryClick(0);break;
                default:c.actual="Requires World cancel/final-scope mutation fixture; not inferred from Work";return;
            }
            Set(c,ok,"PanicWorkSession actual API. "+extra,c.target=="cancel-mid"||c.target=="new-session"||c.target=="begin-world"?"PASS_MODEL_ONLY":"PASS");
        }
        IEnumerator Ready(int screw){
            w.StartJuryDemo();yield return Place(new Vector3(6.5f,.05f,3.25f),"fridge");w.QuickAction(1);yield return null;
            if(!w.WorkActive)throw new InvalidOperationException("Funded reachable fridge anchor fixture failed: "+w.Model.Message);
            if(screw==1){w.ClickWorkTarget(0);for(int j=0;j<6;j++)w.StepWork(.1f,0,false,false,false);yield return null;}
        }
        IEnumerator Place(Vector3 position,string focus){report.setupWarps++;w.Warp(position);float end=Time.realtimeSinceStartup+.20f;while(Time.realtimeSinceStartup<end){w.SmokeMove(Vector2.zero,Time.deltaTime);yield return null;}w.SmokeFocus(focus);yield return null;}
        IEnumerator HitCase(Case c){
            var a=c.target.Split('/');string edge=string.Join("/",a.Skip(2));var size=a[0].Split('x');Screen.SetResolution(int.Parse(size[0]),int.Parse(size[1]),false);yield return null;yield return Ready(a[1]=="screw1"?1:0);
            int screw=a[1]=="screw1"?1:0;Rect r=hud.WorkClickTargetBounds(screw);float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f),px=1f/scale;
            Vector2 p=r.center;bool expected=true;
            if(edge=="left/top exact")p=new Vector2(r.xMin,r.yMin);
            if(edge=="right/bottom exact"){p=new Vector2(r.xMax,r.yMax);expected=false;}
            if(edge=="outside1 physical px"){p=new Vector2(r.xMin-px,r.center.y);expected=false;}
            if(edge=="inside1 physical px")p=new Vector2(r.xMin+px,r.yMin+px);
            yield return Click(p);bool accepted=w.Expedition.Work.IsInstalling;
            Set(c,r.width>0&&accepted==expected,"Screen="+Screen.width+"x"+Screen.height+" bounds="+r+" logical="+p+" accepted="+accepted+"; explicit logical-pointer callback, not platform coordinate proof","PASS_CALLBACK_ONLY");
            if(edge=="center"&&screw==0)yield return Capture("screw-"+a[0]);w.CancelWork();yield return null;
        }
        IEnumerator Negative(Case c){
            int screw=c.target=="already seated screw"?1:0;yield return Ready(screw);Rect r=hud.WorkClickTargetBounds(0);Vector2 p=r.center;
            if(c.target=="future screw")p=hud.WorkClickTargetBounds(1).center;
            if(c.target=="empty work image")p=new Vector2(110,240);
            if(c.target=="MouseUp")kind=EventType.MouseUp;
            if(c.target=="RMB")button=1;
            if(c.target=="used Event replay")kind=EventType.Used;
            if(c.target=="10 rapid clicks"||c.target=="busy current+next"){
                w.ClickWorkTarget(0);float before=w.Expedition.Work.Progress;bool ok=true;for(int j=0;j<10;j++)ok&=!w.ClickWorkTarget(0)&&!w.ClickWorkTarget(1);
                Set(c,ok&&w.Expedition.Work.Progress==before,"Busy production ClickWorkTarget burst cannot reset/advance work; API fixture","PASS_CALLBACK_ONLY");
            }else{yield return Click(p);Set(c,!w.Expedition.Work.IsInstalling&&w.Model.Budget==1000,"Negative OnGUI event/target, no accepted install or charge","PASS_CALLBACK_ONLY");}
            kind=EventType.MouseDown;button=0;w.CancelWork();yield return null;
        }
        IEnumerator InvalidHit(Case c){
            yield return Ready(0);int index=c.target=="index-1"?-1:c.target=="index2"?2:0;
            if(c.target=="index-1"||c.target=="index2"){Set(c,hud.WorkClickTargetBounds(index).width==0,"Invalid index empty bounds");}
            else if(c.target=="world construction instead of anchor"){
                w.CancelWork();w.StartDisasterSimulation(DisasterType.Fire);yield return Place(new Vector3(.9f,.05f,-2.2f),"fire_station");w.HandleKey(KeyCode.E);Set(c,w.WorkActive&&w.Expedition.Work.IsWorldConstruction&&hud.WorkClickTargetBounds(0).width==0,"Actual world installation has no screw region");
            }else if(c.target=="null texture"){
                var prop=typeof(PanicHomeWorld).GetProperty("WorkTexture");var texture=prop.GetValue(w);prop.SetValue(w,null);Set(c,hud.WorkClickTargetBounds(0).width==0,"Null real work texture fixture");prop.SetValue(w,texture);
            }else{c.status="NOT_RUN_INVALID_RENDER_FIXTURE";c.actual="No production API to create invalid viewport/zero-dimensional texture; source guard inspected separately";}
            w.CancelWork();yield return null;
        }
        IEnumerator Gate(Case c){
            yield return Ready(0);bool ok=false;string layer="PASS_API_ONLY";
            switch(c.target){
                case "InputLocked":w.InputLocked=true;ok=!w.ClickWorkTarget(0);w.InputLocked=false;ok&=w.ClickWorkTarget(0);break;
                case "cancel frame":w.CancelWork();ok=!w.ClickWorkTarget(0);break;
                case "discrete world action frame":typeof(PanicHomeWorld).GetProperty("DiscreteInputFrame").SetValue(w,Time.frameCount);ok=!w.ClickWorkTarget(0);yield return null;ok&=w.ClickWorkTarget(0);break;
                case "phase reset":case "hidden old work screen":w.ReturnToMenu();ok=!w.ClickWorkTarget(0)&&!w.WorkActive;break;
                case "inventory":bool old=w.InventoryOpen;w.ToggleInventory();ok=w.InventoryOpen==old&&w.WorkActive;break;
                case "cancel+Enter":w.ProcessWorkInput(.1f,0,false,true,false,true);ok=!w.WorkActive&&!w.Model.Items.Find(x=>x.Id=="fridge").Anchored&&w.Model.Budget==1000;break;
                default:c.actual="Pending compatible modal/combined-input fixture; not implied by API-only guards";w.CancelWork();yield return null;yield break;
            }
            Set(c,ok,"Production API same-frame/modal boundary; no OS or physical key acceptance",layer);w.CancelWork();yield return null;
        }
        void WorldRecipe(string action,string target){
            report.explicitStockFixtures++;for(int i=0;i<w.Expedition.Inventory.Count;i++)w.Expedition.Inventory[i]=new SupplySlot();
            var item=w.Model.Items.Find(x=>x.Id==target);int n=0;foreach(var cost in PanicExpedition.MaterialCosts(action,item))w.Expedition.Inventory[n++]=new SupplySlot(cost.Id,cost.Required);
        }
        void Steps(int n){for(int i=0;i<n;i++)w.StepWork(.1f,0,false,false,false);}
        IEnumerator Construction(Case c){
            string[] a=c.target.Split('/');string id=a[0]=="Wall"?"wall_north":a[0]=="Window"?"window_north":a[0]=="Door"?"door":"roof";
            Vector3 p=a[0]=="Wall"?new Vector3(0,.05f,7.1f):a[0]=="Window"?new Vector3(4,.05f,7.1f):a[0]=="Door"?new Vector3(0,.05f,-7.3f):new Vector3(0,.05f,-4f);
            w.StartDisasterSimulation(DisasterType.Storm);yield return Place(p,id);var item=w.Model.Items.Find(x=>x.Id==id);
            int startTier=a[1].StartsWith("T1")?1:0;
            if(startTier==1){WorldRecipe("upgrade",id);w.BeginWork("upgrade",id);Steps(6);yield return null;}
            if(item.Tier!=startTier){Set(c,false,"Setup first-tier production upgrade failed: "+w.Model.Message);yield break;}
            WorldRecipe("upgrade",id);int before=w.Model.Budget,cost=PanicExpedition.WorkCost("upgrade",item);w.BeginWork("upgrade",id);Steps(3);yield return null;
            var preview=typeof(PanicHomeWorld).GetField("constructionPreview",Hidden).GetValue(w) as GameObject;
            bool mid=w.WorkActive&&w.WorldConstructionVisible&&preview&&item.Tier==startTier&&w.Model.Budget==before&&preview.GetComponentsInChildren<Collider>(true).All(x=>!x.enabled);
            if(a[2]=="midpoint")Set(c,mid,"Actual World preview at .3s; previous Tier/cost unchanged; preview has no active colliders. Explicit exact recipe fixture; apertures/cutaway require rendered review.","PASS_API_RENDER_PARTIAL");
            else if(a[2]=="cancel"){
                string stock=string.Join("|",w.Expedition.Inventory.Select(s=>s.Id+":"+s.Count));w.CancelWork();yield return null;
                Set(c,mid&&!w.WorkActive&&!w.HasConstructionPreview&&item.Tier==startTier&&w.Model.Budget==before&&stock==string.Join("|",w.Expedition.Inventory.Select(s=>s.Id+":"+s.Count)),"Actual midway cancel: Tier/stock/budget retained, preview removed; previous hardware retained","PASS_API_RENDER_PARTIAL");
            }else{
                Steps(3);yield return null;bool done=item.Tier==startTier+1&&w.Model.Budget==before-cost&&!w.WorkActive&&!w.HasConstructionPreview&&w.InstalledMeasureRenderers(id)>0;
                Set(c,mid&&done,"Actual committed Tier="+item.Tier+"; hardware renderers="+w.InstalledMeasureRenderers(id)+"; exact debit="+cost+"; explicit recipe fixture. Full aperture/hinge collision matrix not implied.","PASS_API_RENDER_PARTIAL");yield return Capture("art-"+a[0]+"-T"+item.Tier);
            }
            w.CancelWork();yield return null;
        }
        IEnumerator Tree(Case c){
            var a=c.target.Split('/');string id=a[0],state=string.Join("/",a.Skip(1));w.StartDisasterSimulation(DisasterType.Flood);
            Vector3 p=id=="plant_west1"?new Vector3(-10,.05f,4):id=="plant_west2"?new Vector3(-12.3f,.05f,-1.4f):new Vector3(11,.05f,8);
            yield return Place(p,id);WorldRecipe("plant",id);int budget=w.Model.Budget;
            if(state=="no kit")w.Expedition.Inventory[0].Count=0;
            w.HandleKey(KeyCode.E);
            if(state=="no kit"){Set(c,!w.WorkActive&&w.Model.Budget==budget&&w.Model.TreeCount==0,"No kit denial through actual World E; no debit/region/trunk mutation");yield break;}
            Steps(3);yield return null;bool mid=w.WorkActive&&w.HasConstructionPreview&&w.Model.TreeCount==0&&w.Model.Budget==budget;
            if(state=="midpoint"){Set(c,mid,"Real .3s tree-row preview, no committed row/cost","PASS_API_RENDER_PARTIAL");w.CancelWork();yield break;}
            if(state=="cancel"){w.CancelWork();yield return null;Set(c,mid&&!w.HasConstructionPreview&&w.Model.TreeCount==0&&w.Expedition.Count("seedling")==1&&w.Model.Budget==budget,"Real mid-install cancel preserves kit/budget, no row/trunk remains","PASS_API_RENDER_PARTIAL");yield break;}
            Steps(3);yield return null;int trunks=FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None).Count(x=>x.name=="Tree trunk collision");bool complete=w.Model.TreeCount==1&&w.InstalledTreeRows==1&&trunks==4&&w.Expedition.Count("seedling")==0&&w.Model.Budget==budget-30;
            if(state=="repeat completed"){w.HandleKey(KeyCode.E);Set(c,complete&&!w.WorkActive&&w.Model.Budget==budget-30&&FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None).Count(x=>x.name=="Tree trunk collision")==4,"Completed region repeated E inert, still four physical trunks");}
            else if(state=="new wave/reset"){w.ReturnToMenu();w.StartDisasterSimulation(DisasterType.Flood);yield return null;Set(c,complete&&w.Model.TreeCount==0&&w.InstalledTreeRows==0&&FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None).Count(x=>x.name=="Tree trunk collision")==0,"Menu/restart clears real row/trunks. Next campaign-wave persistence not exercised.","PASS_RESET_ONLY");}
            else {Set(c,complete,"One actual region commit, four physical trunks, one kit/30 budget. Canopy aesthetics/path atlas not implied.","PASS_API_RENDER_PARTIAL");yield return Capture("row-"+id);}
        }
        void Warnings(){
            string saved=PanicModel.ScorePathOverride;
            foreach(DisasterType type in Enum.GetValues(typeof(DisasterType))){var f=New();f.M.ReturnToTitle();bool start=f.M.StartSimulation(type);f.M.Tick(31);bool ok=start&&f.M.TimeRemaining<=60&&!f.M.Message.ToLowerInvariant().Contains("bunker");report.supplementary.Add("simulation-warning "+type+" "+(ok?"PASS":"FAIL")+" "+f.M.Message);if(!ok)report.errors.Add("Simulation misleading bunker warning: "+type);}
            var campaign=New();campaign.M.TimeRemaining=61;campaign.M.Tick(2);bool normal=campaign.M.Message.Contains("bunker");report.supplementary.Add("campaign-warning "+(normal?"PASS":"FAIL")+" "+campaign.M.Message);if(!normal)report.errors.Add("Campaign bunker warning changed");PanicModel.ScorePathOverride=saved;
        }
        IEnumerator Click(Vector2 point){click=point;pending=true;float end=Time.realtimeSinceStartup+1;while(pending&&Time.realtimeSinceStartup<end)yield return null;if(pending)throw new InvalidOperationException("No OnGUI repaint callback");yield return null;}
        void OnGUI(){if(!pending||Event.current.type!=EventType.Repaint)return;pending=false;var prior=new Event(Event.current);var gm=GUI.matrix;try{GUI.matrix=Matrix4x4.identity;Point.SetValue(pointer,click);Active.SetValue(null,pointer);Event.current=new Event{type=kind,button=button,mousePosition=click};Gui.Invoke(hud,null);}finally{Active.SetValue(null,null);Event.current=prior;GUI.matrix=gm;}}
        IEnumerator Capture(string name){yield return new WaitForEndOfFrame();string p=Path.Combine(w.EvidencePath,name+".png");ScreenCapture.CaptureScreenshot(p);report.captures.Add(p);yield return null;}
        void Set(Case c,bool ok,string detail,string status="PASS"){c.status=ok?status:"FAIL";c.actual=detail;Debug.Log("MATRIX "+c.id+" "+c.status+" "+c.target+" "+detail);if(path!=null)File.WriteAllText(path,JsonUtility.ToJson(report,true));}
        void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(text+"\n"+stack);}
        void Update(){if(!finished&&Time.realtimeSinceStartup-started>240){report.errors.Add("240s watchdog");Finish();}}
        void Finish(){if(finished)return;finished=true;PanicModel.ScorePathOverride=callerScore;Application.logMessageReceived-=Log;report.scoreAfter=Hash(callerScore);report.realScoreAfter=Hash(realScore);report.seconds=Time.realtimeSinceStartup-started;report.finishedUtc=DateTime.UtcNow.ToString("o");if(path!=null)File.WriteAllText(path,JsonUtility.ToJson(report,true));bool pass=report.completed&&report.errors.Count==0&&report.scoreBefore==report.scoreAfter&&report.realScoreBefore==report.realScoreAfter&&!report.cases.Any(c=>c.status=="FAIL");Debug.Log("DIRECT_MATRIX_DONE pass="+pass);Application.Quit(pass?0:1);}
        static string Hash(string p){if(!File.Exists(p))return "absent";using(var f=File.OpenRead(p))using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(f)).Replace("-","");}
    }
}
