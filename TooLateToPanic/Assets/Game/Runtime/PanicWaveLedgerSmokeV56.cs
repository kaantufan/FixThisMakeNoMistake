using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Unique opt-in ledger smoke. Sole Unity/build/player owner executes; no OS input is generated.</summary>
    public sealed class PanicWaveLedgerSmokeV56 : MonoBehaviour
    {
        const string Flag="--wave-ledger-v56-smoke";
        [Serializable] public sealed class Row{public string id,status,detail,layer;public int frame;}
        [Serializable] public sealed class Shot{public string path,context;public int width,height,frame;}
        [Serializable] public sealed class Receipt
        {
            public string utc,dllSha256,scorePath,realScoreBefore,realScoreAfter,error;
            public string evidence="Native opt-in model/API + actual graphical framebuffer checks; focused roof/card/lintel/power/F1 visual revision in the same isolated process. Explicit isolated toy item/stock/support/seed/memo fixtures; one production normal World start, production UI semantic methods, production model shelter gate and production skip. Model gate fixture does not certify physical shelter traversal. No OS keyboard/mouse, human play, legibility/fun, listening or campaign acceptance.";
            public string renderLimit="PNG dimensions certify file capture from current graphical surface. Sole Unity owner must inspect actual pixels for clipping/legibility and invalid black surface; GUI bounds/draw counters alone do not certify visual acceptance.";
            public bool complete,passed,osInput,humanAcceptance,listening;
            public float seconds;
            public int isolatedFixtureModels,seedCases,worldProtectionFixtures,worldStockFixtures,worldMemoFixtures,productionStartCalls,productionModelShelterCalls,productionSkipCalls;
            public List<Row> checks=new List<Row>();public List<Shot> captures=new List<Shot>();public List<string> errors=new List<string>();
        }
        readonly Receipt report=new Receipt();PanicHomeWorld w;PanicHomeHud hud;PanicNotebookOverlay book;float began;string realScore;bool finished;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Attach()
        {
            if(!Environment.GetCommandLineArgs().Contains(Flag))return;var owner=FindFirstObjectByType<PanicHomeWorld>();
            if(owner&&!owner.GetComponent<PanicWaveLedgerSmokeV56>())owner.gameObject.AddComponent<PanicWaveLedgerSmokeV56>();
        }
        void Start()
        {
            w=GetComponent<PanicHomeWorld>();hud=GetComponent<PanicHomeHud>();book=w.Notebook;began=Time.realtimeSinceStartup;
            realScore=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");report.utc=DateTime.UtcNow.ToString("o");report.dllSha256=Hash(typeof(PanicModel).Assembly.Location);report.realScoreBefore=Hash(realScore);report.scorePath=PanicModel.ScorePathOverride;
            Application.logMessageReceived+=Log;StartCoroutine(Guard(Run()));
        }
        void Update(){if(!finished&&Time.realtimeSinceStartup-began>180){report.error="180s focused ledger smoke cap";Finish();}}
        void Log(string value,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(value+"\n"+stack);}
        void Check(string id,bool pass,string detail="",string layer="NATIVE_API_FRAME")
        {report.checks.Add(new Row{id=id,status=pass?"PASS":"FAIL",detail=detail,layer=layer,frame=Time.frameCount});Debug.Log("WAVE_LEDGER_V56_CHECK "+(pass?"PASS ":"FAIL ")+id+" "+detail);}
        static T Read<T>(object owner,string name){return PanicWaveLedgerModelChecksV56.Read<T>(owner,name);}
        static object Call(object owner,string name,params object[] args){return PanicWaveLedgerModelChecksV56.Call(owner,name,args);}
        bool Visible{get{return Read<bool>(hud,"WaveLedgerVisibleV56");}}
        bool Open(){return (bool)Call(hud,"OpenWaveLedgerV56");}
        void Close(){Call(hud,"CloseWaveLedgerV56");}
        bool Key(KeyCode key){return (bool)Call(hud,"ProcessWaveLedgerKeyV56",key);}
        bool Isolated()
        {return !string.IsNullOrEmpty(w.EvidencePath)&&!string.IsNullOrEmpty(PanicModel.ScorePathOverride)&&string.Equals(Path.GetFullPath(PanicModel.ScorePathOverride),Path.Combine(Path.GetFullPath(w.EvidencePath),"isolated-home-scores.json"),StringComparison.OrdinalIgnoreCase);}
        string Stamp(){return PanicWaveLedgerModelChecksV56.ModelStamp(w.Model)+"|"+PanicWaveLedgerModelChecksV56.LedgerStamp(PanicWaveLedgerModelChecksV56.Ledger(w.Model));}
        IEnumerator Guard(IEnumerator body)
        {
            var stack=new Stack<IEnumerator>();stack.Push(body);while(stack.Count>0&&!finished)
            {
                bool next;object current=null;try{next=stack.Peek().MoveNext();if(next)current=stack.Peek().Current;}catch(Exception e){report.error=e.ToString();break;}
                if(!next){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;
            }
            if(!finished){report.complete=string.IsNullOrEmpty(report.error);Finish();}
        }
        IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
        IEnumerator Resolution(int width,int height)
        {
            Screen.SetResolution(width,height,FullScreenMode.Windowed);float end=Time.realtimeSinceStartup+2;
            while((Screen.width!=width||Screen.height!=height)&&Time.realtimeSinceStartup<end)yield return null;yield return Wait(.22f);
            Check("framebuffer."+width,Screen.width==width&&Screen.height==height,Screen.width+"x"+Screen.height,"NATIVE_GRAPHICAL_SURFACE");
        }
        IEnumerator Capture(string name,string context)
        {
            yield return new WaitForEndOfFrame();string path=Path.Combine(w.EvidencePath,name+".png");int width=Screen.width,height=Screen.height,frame=Time.frameCount;ScreenCapture.CaptureScreenshot(path);
            float end=Time.realtimeSinceStartup+2;int pw=0,ph=0;while(!PngSize(path,out pw,out ph)&&Time.realtimeSinceStartup<end)yield return null;
            report.captures.Add(new Shot{path=path,context=context,width=pw,height=ph,frame=frame});Check("capture."+name,pw==width&&ph==height,pw+"x"+ph+"; "+context,"NATIVE_FRAMEBUFFER_FILE");
        }
        IEnumerator Bounds(string id)
        {
            int draws=Read<int>(hud,"WaveLedgerDrawFramesV56");yield return Wait(.12f);yield return new WaitForEndOfFrame();
            Rect logical=Read<Rect>(hud,"WaveLedgerPanelBoundsV56"),offsetRect=new Rect(hud.CanvasOffset.x+logical.x*hud.CanvasScale,hud.CanvasOffset.y+logical.y*hud.CanvasScale,logical.width*hud.CanvasScale,logical.height*hud.CanvasScale);
            Check("hud.draw."+id,Visible&&Read<int>(hud,"WaveLedgerDrawFramesV56")>draws,"Actual visible Repaint count increased.","NATIVE_REPAINT_COUNTER");
            Check("hud.bounds."+id,offsetRect.xMin>=0&&offsetRect.yMin>=0&&offsetRect.xMax<=Screen.width+.5f&&offsetRect.yMax<=Screen.height+.5f,"Actual logical panel transformed to framebuffer="+offsetRect,"NATIVE_GUI_LAYOUT");
            Check("hud.rendered-text."+id,!string.IsNullOrWhiteSpace(Read<string>(hud,"WaveLedgerRenderedTextV56")),"Actual last rendered text telemetry.","NATIVE_REPAINT_TEXT");
        }
        IEnumerator Hold(string id,float seconds=.35f)
        {
            string state=Stamp();float prep=w.Model.TimeRemaining,phase=w.Model.PhaseTime;int rolls=w.Model.SecondaryRollCountV5;yield return Wait(seconds);
            Check("clock.hold."+id,PanicWaveLedgerModelChecksV56.Near(prep,w.Model.TimeRemaining)&&PanicWaveLedgerModelChecksV56.Near(phase,w.Model.PhaseTime)&&state==Stamp()&&rolls==w.Model.SecondaryRollCountV5,"Actual wall wait="+seconds+"; preparation="+prep+" -> "+w.Model.TimeRemaining+" phase="+phase+" -> "+w.Model.PhaseTime+"; no Tick/time fixture while measuring.");
        }
        IEnumerator PreparationPauses()
        {
            float before=w.Model.TimeRemaining;yield return Wait(.20f);Check("clock.baseline-advances",w.Model.TimeRemaining<before-.08f,"Actual World Update resumes ordinary clock.");
            hud.SetPaused(true);yield return Hold("pause");Close();Check("pause.spurious-ledger-close-keeps-owner",w.Model.IsPaused,"Closing absent ledger does not resume a pause owner.");hud.SetPaused(false);yield return Wait(.10f);
            Check("notebook.production-open",book.RequestOpen());yield return Wait(.42f);yield return Hold("notebook");Close();Check("notebook.spurious-ledger-close-keeps-owner",book.BlocksInput,"Closing absent ledger leaves notebook input ownership.");book.RequestClose();yield return Wait(.44f);
            hud.SetDeveloperMenuOpen(true);yield return Wait(.10f);Check("developer.production-open",hud.DeveloperMenuOpen&&w.Model.IsPaused);yield return Hold("developer");Close();Check("developer.spurious-ledger-close-keeps-owner",hud.DeveloperMenuOpen&&w.Model.IsPaused);hud.SetDeveloperMenuOpen(false);yield return Wait(.12f);
            // Inventory retains the current live-clock contract; do not silently turn it into a new pause type.
            w.ToggleInventory();yield return Wait(.08f);before=w.Model.TimeRemaining;yield return Wait(.20f);Check("inventory.current-live-clock-preserved",w.InventoryOpen&&w.Model.TimeRemaining<before-.08f,"Existing inventory semantics retained; notebook/pause/developer/result list are distinct owners.");w.ToggleInventory();yield return Wait(.10f);
            hud.SetPaused(true);hud.SetDeveloperMenuOpen(true);yield return Wait(.12f);hud.SetDeveloperMenuOpen(false);Check("pause.nested-developer-restores-prior",w.Model.IsPaused,"Developer opened from pause restores prior pause on close.");yield return Hold("pause-after-developer");hud.SetPaused(false);yield return Wait(.12f);
        }
        IEnumerator ActualWorldLedger()
        {
            var m=w.Model;m.UtilitySafety.Reset();
            foreach(var item in m.Items){item.Tier=2;item.Anchored=item.Kind==ItemKind.Furniture;item.FloodProtected=item.FireProtected=true;report.worldProtectionFixtures++;}
            // Preserve historical check IDs; the transaction now uses actual finite world stock
            // and canonical inventory projections. No legacy model-only stock/owned-bit fixtures.
            var waterSupply=w.Expedition.WorldSupplies.First(s=>s.ItemId=="water"&&!s.Taken);
            var foodSupply=w.Expedition.WorldSupplies.First(s=>s.ItemId=="food"&&!s.Taken);
            var firstaidSupply=w.Expedition.WorldSupplies.First(s=>s.ItemId=="firstaid"&&!s.Taken);
            report.worldStockFixtures+=2;Check("world.real-pack-stock",w.Expedition.TryCollectWorldSupply(waterSupply.Id)&&w.Expedition.TryCollectWorldSupply(foodSupply.Id)&&m.BagReady&&m.PackedWater==w.Expedition.Count("water")&&m.PackedFood==w.Expedition.Count("food"),"Actual finite Expedition collection; direct model API coverage, not physical pickup-route acceptance.");
            report.worldStockFixtures++;Check("world.real-pack-support",w.Expedition.TryCollectWorldSupply(firstaidSupply.Id)&&m.SupportCount==1&&w.Expedition.Count("firstaid")>0,"Actual finite firstaid collection; no owned-bit or manual packing fixture.");
            var target=m.Items.First(i=>i.Installed&&i.Kind==ItemKind.Furniture&&i.ArtId=="bookshelf");
            var memo=Read<Dictionary<string,PanicSecondaryEventV5>>(m,"secondaryRollsV5");memo[m.Wave+":"+(int)DisasterType.Earthquake]=new PanicSecondaryEventV5(PanicSecondaryEventKindV5.GasFlare,target.Id,7.5f,"Gaz vanası açık kaldı: küçük bir alevlenme oldu");report.worldMemoFixtures++;
            int before=m.Budget;report.productionModelShelterCalls++;Check("world.production-model-shelter",m.TryEnterShelterV5(m.RequiredShelterV5),"Explicit typed model gate fixture; no physical approach/ray/path acceptance.");yield return Wait(.12f);
            Check("hud.no-premature-ledger-in-cinematic",!Visible&&PanicWaveLedgerModelChecksV56.Ledger(m)==null,"Disaster presentation remains sole clock owner before result.");
            report.productionSkipCalls++;w.SkipResidenceCinematic();yield return Wait(.25f);
            float until=Time.realtimeSinceStartup+4;while(!Visible&&Time.realtimeSinceStartup<until)yield return null;
            Check("hud.auto-open-stable-inspection",m.Phase==PanicPhase.Inspection&&!w.CinematicActive&&Visible,"Production cinematic skip resolves actual damage/reward once; auto list opens only stable inspection.");
            PanicWaveLedgerModelChecksV56.VerifyLedger("world.normal-committed",m,before,true,(id,pass,detail)=>Check(id,pass,detail));
            var rows=PanicWaveLedgerModelChecksV56.Rows(PanicWaveLedgerModelChecksV56.Ledger(m));
            Check("world.actual-item-and-secondary-rows",rows.Any(r=>Read<string>(r,"Id")==target.Id)&&rows.Any(r=>Read<string>(r,"Id").StartsWith("utility:",StringComparison.Ordinal)),"Real damaged item and actual memoized gas event appear as separate contribution rows.");
            yield return Bounds("1600");yield return Hold("ledger-inspection",.50f);yield return Capture("ledger-v56-result-1600","Normal committed actual damage, separate gas secondary row and integer budget reconciliation.");
            string state=Stamp();int pages=Read<int>(hud,"WaveLedgerPageCountV56");Check("hud.page-count-valid",pages>=1,"Actual rendered pages="+pages);
            for(int page=1;page<pages;page++){Check("hud.page-next."+page,Key(KeyCode.RightArrow));yield return Wait(.08f);Check("hud.page-index."+page,Read<int>(hud,"WaveLedgerPageV56")==page);}
            Check("hud.pages-readonly",state==Stamp(),"Semantic production page navigation keeps budget/items/RNG/supplies/result unchanged.");
            if(pages>1)yield return Capture("ledger-v56-last-page-1600","Actual last page; review complete list and page controls.");
            yield return Resolution(960,540);yield return Bounds("960");yield return Capture("ledger-v56-result-960","Actual narrow framebuffer; sole owner inspects text fit and final continue button.");
            Check("hud.enter-dismiss-semantic",Key(KeyCode.Return)&&!Visible,"Enter dismisses list; explicit later continue owns next-wave transition.");int budget=m.Budget,wave=m.Wave,rolls=m.SecondaryRollCountV5;string ledger=PanicWaveLedgerModelChecksV56.LedgerStamp(PanicWaveLedgerModelChecksV56.Ledger(m));
            for(int i=0;i<16;i++)Close();Check("hud.repeat-close-once",m.Budget==budget&&m.Wave==wave&&m.SecondaryRollCountV5==rolls&&!Visible,"No duplicate credit/RNG/wave advance on repeated close.");yield return Wait(.12f);
            Check("hud.reopen-same-snapshot",Open()&&Visible);yield return Wait(.12f);Check("hud.reopen-readonly",m.Budget==budget&&m.Wave==wave&&m.SecondaryRollCountV5==rolls&&ledger==PanicWaveLedgerModelChecksV56.LedgerStamp(PanicWaveLedgerModelChecksV56.Ledger(m)));yield return Hold("ledger-reopen");
            Check("hud.esc-dismiss-semantic",Key(KeyCode.Escape)&&!Visible,"Escape only dismisses result list.");yield return Wait(.12f);
            hud.SetPaused(true);Check("hud.pause-prevents-ledger-open",!Open()&&w.Model.IsPaused);Close();yield return Hold("inspection-pause");Check("hud.close-preserves-inspection-pause",m.IsPaused);hud.SetPaused(false);yield return Wait(.12f);
            float phase=m.PhaseTime;yield return Wait(.20f);Check("clock.inspection-resumes-after-review",m.PhaseTime<phase-.08f,"Actual inspected-and-dismissed result resumes normal review timer.");
            int completed=m.CompletedWaves;w.SkipResidenceCinematic();m.FinishInspection();m.FinishInspection();yield return Wait(.16f);
            Check("world.continue-next-wave-once",m.Wave==wave+1&&m.Budget==budget&&m.CompletedWaves==completed&&m.SecondaryRollCountV5==rolls,"Actual model FinishInspection twice plus redundant cinematic skip: one wave, no duplicate credit/roll.");
            Check("hud.new-wave-closed",!Visible,"Old result modal does not reopen automatically in a fresh preparing phase.");
        }
        IEnumerator Run()
        {
            yield return Wait(.25f);Check("launch.isolated-guid-profile",Isolated(),PanicModel.ScorePathOverride,"NATIVE_FILE_PATH");if(!Isolated())throw new InvalidOperationException("Refusing ledger smoke outside isolated evidence profile.");
            Check("launch.exactly-one-driver",Environment.GetCommandLineArgs().Count(a=>PanicTestLaunchRules.Drivers.Contains(a))==1,"One registered isolated driver only.");
            PanicWaveLedgerModelChecksV56.Run(w.EvidencePath,(id,pass,detail)=>Check(id,pass,detail,"NATIVE_MODEL_FIXTURE"));report.isolatedFixtureModels=PanicWaveLedgerModelChecksV56.FixtureModels;report.seedCases=PanicWaveLedgerModelChecksV56.SeedCases;
            PanicEconomyChecksV59.Run((id,pass,detail)=>Check("v59.economy."+id,pass,detail,"NATIVE_MODEL_FIXTURE"));
            PanicCompoundChecksV54.Run((id,pass,detail)=>Check(id,pass,detail,"NATIVE_MODEL_FIXTURE"));
            yield return Resolution(1600,900);w.ReturnToMenu();w.StartGame(false);report.productionStartCalls++;w.FullFlowScriptedInput=true;yield return null;
            if(hud.FreeplayOnboardingOpenV414)hud.ConfirmFreeplayOnboardingV414();yield return null;w.SkipIntroStoryV414();yield return Wait(.20f);
            Check("world.start-budget1500",w.Model.Budget==1500,"Actual normal world start budget="+w.Model.Budget);yield return PreparationPauses();yield return PanicVisualReadabilityChecksV56.Run(w,hud,(id,pass,detail)=>Check(id,pass,detail),Capture,Resolution);yield return ActualWorldLedger();
            w.ReturnToMenu();yield return Wait(.10f);Check("world.reset-no-stale-ledger",PanicWaveLedgerModelChecksV56.Ledger(w.Model)==null&&!Visible&&w.Model.Budget==1500);
            Check("finish.runtime-errors",report.errors.Count==0,string.Join("\n",report.errors));
        }
        static bool PngSize(string path,out int width,out int height)
        {width=height=0;try{if(!File.Exists(path)||new FileInfo(path).Length<2048)return false;var b=File.ReadAllBytes(path);if(b.Length<24||b[0]!=137||b[1]!=80||b[2]!=78||b[3]!=71)return false;width=(b[16]<<24)|(b[17]<<16)|(b[18]<<8)|b[19];height=(b[20]<<24)|(b[21]<<16)|(b[22]<<8)|b[23];return width>0&&height>0;}catch(IOException){return false;}}
        static string Hash(string path){if(!File.Exists(path))return "absent";using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
        void Finish()
        {
            if(finished)return;finished=true;report.realScoreAfter=Hash(realScore);Check("scores.real-unchanged",report.realScoreBefore==report.realScoreAfter,report.realScoreBefore+" -> "+report.realScoreAfter,"NATIVE_FILE_HASH");Check("scores.remain-isolated",Isolated(),PanicModel.ScorePathOverride,"NATIVE_FILE_PATH");
            report.seconds=Time.realtimeSinceStartup-began;report.passed=report.complete&&report.errors.Count==0&&report.seedCases==512&&report.checks.Count>=100&&report.checks.All(c=>c.status=="PASS");Application.logMessageReceived-=Log;
            if(!string.IsNullOrEmpty(w.EvidencePath))File.WriteAllText(Path.Combine(w.EvidencePath,"wave-ledger-v56-smoke.json"),JsonUtility.ToJson(report,true));Debug.Log("WAVE_LEDGER_V56_SMOKE "+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count+" seconds="+report.seconds);Application.Quit(report.passed?0:2);
        }
    }
}
