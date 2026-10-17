using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    // Opt-in isolated model/file checks. Fixture assignments and injected Tick are
    // explicit; this suite is not an OS route, natural campaign or player study.
    public static class PanicE2ESystemModelChecks
    {
        [Serializable] public sealed class Check
        { public string id,cases,detail; public bool passed; }
        [Serializable] public sealed class Report
        {
            public string startedUtc,finishedUtc,layer,root;
            public string limitations="Fixture model assignments and injected Tick. Native JsonUtility only when executed inside Unity; .NET shim run does not certify Unity serialization, rendering, physical input or long campaign.";
            public bool finished; public int passed,failed;
            public List<Check> checks=new List<Check>(); public List<string> errors=new List<string>(),fixtures=new List<string>(),rawJson=new List<string>();
        }
        static Report report; static string root; static int serial;
        static void CheckThat(string id,string cases,bool result,string detail)
        { report.checks.Add(new Check{id=id,cases=cases,passed=result,detail=detail}); if(result)report.passed++;else report.failed++; }
        static void Group(string name,Action action)
        {try{action();}catch(Exception e){report.errors.Add(name+": "+e);CheckThat(name+".unhandled","SYS-051",false,e.ToString());}}
        static string PathFor(string name)
        { var folder=Path.Combine(root,(++serial).ToString("D3")+"-"+Guid.NewGuid().ToString());Directory.CreateDirectory(folder);report.fixtures.Add(name+" => "+folder);return Path.Combine(folder,"highscores.json"); }
        static PanicModel Model(string name,string json=null)
        {var p=PathFor(name);if(json!=null){File.WriteAllText(p,json,new UTF8Encoding(false));try{var raw=JsonUtility.FromJson<RawFile>(json);report.rawJson.Add(name+" schema="+(raw==null?-1:raw.schema)+" entries="+(raw==null||raw.entries==null?-1:raw.entries.Count)+" values="+(raw==null||raw.entries==null?"null":JsonUtility.ToJson(raw)));}catch(Exception ex){report.rawJson.Add(name+" parse="+ex.GetType().Name);}}PanicModel.ScorePathOverride=p;return new PanicModel();}
        static string Hash(string p)
        {if(!File.Exists(p))return "ABSENT";using(var h=SHA256.Create())using(var f=File.OpenRead(p))return BitConverter.ToString(h.ComputeHash(f)).Replace("-","");}
        [Serializable] sealed class RawFile { public int schema=1;public List<HighscoreEntry> entries=null; }
        static string Good="{\"schema\":1,\"entries\":[{\"waves\":2,\"score\":400,\"date\":\"existing\"}]}";
        static void TimeoutResult(PanicModel m)
        { m.CompletedWaves=1;m.Score=100;m.TimeRemaining=.001f;m.Tick(.002f); }
        static void Supply(PanicExpedition e,params object[] items)
        { for(int i=0;i<items.Length;i+=2)e.Inventory[i/2]=new SupplySlot((string)items[i],(int)items[i+1]); }
        static bool Work(PanicExpedition e,string action,string target)
        {
            if(!e.CanBeginWork(action,target))return false;e.Work.Begin(action,target);
            int total=e.Work.TotalSteps;
            for(int i=0;i<total;i++){if(!e.Work.TryClick(i))return false;for(int j=0;j<7;j++)e.Work.Tick(.1f,0,false,false);}
            return e.TryCompleteWork(action,target);
        }
        static object Invoke(PanicModel m,string method,params object[] args)
        {return typeof(PanicModel).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(m,args);}
        public static Report Run(string outputRoot,string layer)
        {
            root=Path.GetFullPath(outputRoot);Directory.CreateDirectory(root);
            report=new Report{startedUtc=DateTime.UtcNow.ToString("o"),layer=layer,root=root};serial=0;
            string previous=PanicModel.ScorePathOverride;
            try
            {
                Group("valid-scores",()=>{
                    var m=Model("absent");CheckThat("scores.absent","SYS-007 MEC-096",m.Highscores.Count==0,"No file => empty; no default real profile access.");
                    var json="{\"schema\":1,\"entries\":[{\"waves\":1,\"score\":900,\"date\":\"low\"},{\"waves\":2,\"score\":10,\"date\":\"first\"},null,{\"waves\":2,\"score\":10,\"date\":\"second\"},{\"waves\":-1,\"score\":2},{\"waves\":2,\"score\":-1},{\"waves\":2147483647,\"score\":9223372036854775807,\"date\":\"max\"}]}";
                    m=Model("stable",json);string before=Hash(PanicModel.ScorePathOverride);
                    CheckThat("scores.valid-filter-stable-order","SYS-007 MEC-096",m.Highscores.Count==4&&m.Highscores[0].date=="max"&&m.Highscores[1].date=="first"&&m.Highscores[2].date=="second"&&m.Highscores[3].date=="low","Negative rows/null ignored; waves then score descending; ties stable; long/int maxima.");
                    m.StartRun(false);CheckThat("scores.load-does-not-rewrite","SYS-007 SYS-010",Hash(PanicModel.ScorePathOverride)==before,"Loading/starting leaves fixture bytes unchanged.");
                    var again=new PanicModel();CheckThat("scores.restart-stable","SYS-007",again.Highscores.Count==4&&again.Highscores[1].date=="first"&&again.Highscores[2].date=="second","Same disk fixture reloaded.");
                });
                Group("json-field-shapes",()=>{
                    var legacy=Model("legacy-date-optional","{\"schema\":1,\"entries\":[{\"waves\":0,\"score\":0}]}");
                    CheckThat("scores.legacy-optional-date","SYS-008",legacy.Highscores.Count==1,"Do not introduce a date-required policy while filtering coerced null rows.");
                    var bad=Model("wrong-type-with-date","{\"schema\":1,\"entries\":[{\"waves\":\"bad\",\"score\":1,\"date\":\"typed-fixture\"}]}");
                    CheckThat("scores.wrong-numeric-type-with-date","SYS-008",bad.Highscores.Count==0,"Present numeric fields must have their declared integer type, independently of date.");
                    bad=Model("wrong-score-type","{\"schema\":1,\"entries\":[{\"waves\":2,\"score\":\"9\",\"date\":\"typed-fixture\"}]}");
                    CheckThat("scores.wrong-score-type","SYS-008",bad.Highscores.Count==0,"Quoted score is not silently converted to integer.");
                    bad=Model("null-row","{\"schema\":1,\"entries\":[null]}");
                    CheckThat("scores.null-row-not-phantom","SYS-008",bad.Highscores.Count==0,"Unity-created default row from JSON null is not a legitimate zero-wave result.");
                    bad=Model("schema-string","{\"schema\":\"1\",\"entries\":[{\"waves\":2,\"score\":9,\"date\":\"typed-fixture\"}]}");
                    CheckThat("scores.schema-is-integer","SYS-008",bad.Highscores.Count==0,"Supported schema has integer type; original file retained.");
                    var extra=Model("unknown-metadata","{\"schema\":1,\"extra\":{\"x\":[1,2]},\"entries\":[{\"waves\":2,\"score\":9,\"date\":\"typed-fixture\",\"extra\":{\"x\":[1,2]}}]}");
                    CheckThat("scores.unknown-metadata-preserved","SYS-008",extra.Highscores.Count==1&&extra.Highscores[0].waves==2&&extra.Highscores[0].score==9,"Unknown metadata is skipped; no new schema or migration.");
                });
                Group("corrupt-scores",()=>{
                    string[] data={"","{","not json","{\"entries\":null}","{}","{\"entries\":\"wrong type\"}","{\"entries\":[{\"waves\":\"bad\",\"score\":1}]}"};
                    for(int i=0;i<data.Length;i++){
                        var m=Model("malformed"+i,data[i]);string p=PanicModel.ScorePathOverride,before=Hash(p);m.StartRun(false);
                        CheckThat("scores.malformed."+i,"SYS-008 MEC-096",m.Highscores.Count==0&&m.Phase==PanicPhase.Preparing&&Hash(p)==before,"Safe start; does not rewrite malformed bytes during load. Input="+data[i]);
                    }
                    var wrong=Model("schema","{\"schema\":99,\"entries\":[{\"waves\":4,\"score\":500,\"date\":\"future-schema\"}]}");
                    CheckThat("scores.unsupported-schema","SYS-008",wrong.Highscores.Count==0,"Expected unsupported schema not to be silently adopted; observed entries="+wrong.Highscores.Count+". Candidate requires explicit product decision/native confirmation.");
                    var utf=PathFor("invalid-utf8");File.WriteAllBytes(utf,new byte[]{255,254,253,0,123});PanicModel.ScorePathOverride=utf;var bad=new PanicModel();
                    CheckThat("scores.invalid-utf8","SYS-008",bad.Highscores.Count==0,"Garbage UTF8 parsed only by active JSON engine; caught by model.");
                    var rows=new StringBuilder("{\"schema\":1,\"entries\":[");for(int i=0;i<14000;i++){if(i>0)rows.Append(',');rows.Append("{\"waves\":1,\"score\":").Append(14000-i).Append(",\"date\":\"2026-10-17\"}");}rows.Append("]}");
                    var watch=Stopwatch.StartNew();var large=Model("large",rows.ToString());watch.Stop();
                    CheckThat("scores.large-bounded-fixture","SYS-008",Encoding.UTF8.GetByteCount(rows.ToString())<=1048576&&large.Highscores.Count==14000,"bytes="+Encoding.UTF8.GetByteCount(rows.ToString())+"; elapsedMs="+watch.Elapsed.TotalMilliseconds+"; no policy time limit invented.");
                });
                Group("atomic-write",()=>{
                    foreach(string fault in new[]{"locked-destination","readonly-destination","locked-temp"}){
                        var m=Model(fault,Good);var p=PanicModel.ScorePathOverride;string before=Hash(p);m.StartRun(false);FileStream held=null;
                        try{
                            if(fault=="locked-destination")held=new FileStream(p,FileMode.Open,FileAccess.Read,FileShare.Read);
                            if(fault=="readonly-destination")File.SetAttributes(p,File.GetAttributes(p)|FileAttributes.ReadOnly);
                            if(fault=="locked-temp")held=new FileStream(p+".tmp",FileMode.Create,FileAccess.ReadWrite,FileShare.None);
                            TimeoutResult(m);
                            CheckThat("atomic."+fault+".preserves","SYS-009 MEC-097",Hash(p)==before&&m.Highscores.Count==2&&m.Message.Contains("yazılamadı"),"Existing valid scoreboard preserved; one in-memory result, write-failure message. tmpExists="+File.Exists(p+".tmp"));
                        }finally{if(held!=null)held.Dispose();File.SetAttributes(p,FileAttributes.Normal);}
                        m.ReturnToTitle();var loaded=new PanicModel();
                        CheckThat("atomic."+fault+".retry-once","SYS-009 MEC-097",loaded.Highscores.Count==2&&Hash(p)!=before,"ReturnToTitle retries exactly one in-memory result after fault removal.");
                    }
                    var residue=Model("stray-temp",Good);string real=PanicModel.ScorePathOverride;File.WriteAllText(real+".tmp","{bad orphan");string original=Hash(real);residue=new PanicModel();
                    CheckThat("atomic.orphan-load","SYS-009",residue.Highscores.Count==1&&Hash(real)==original,"Orphan .tmp is ignored on load, not promoted over real scoreboard.");
                    residue.StartRun(false);TimeoutResult(residue);CheckThat("atomic.orphan-overwrite","SYS-009",!File.Exists(real+".tmp")&&new PanicModel().Highscores.Count==2,"Successful next save replaces stale tmp and consumes it.");
                });
                Group("single-score",()=>{
                    var m=Model("single",Good);m.StartRun(false);TimeoutResult(m);CheckThat("score.record-once","MEC-098",m.Highscores.Count==2,"One fixture campaign result enters table.");m.Tick(1);m.ReturnToTitle();m.ReturnToTitle();CheckThat("score.menu-no-duplicate","MEC-098",new PanicModel().Highscores.Count==2,"Repeated menu/tick doesn't duplicate same result.");
                    m=Model("cheated-removal",Good);m.StartRun(false);TimeoutResult(m);m.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget,true);m.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget,false);
                    CheckThat("score.retroactive-cheat-removal","MEC-098 SYS-011",m.CheatedRun&&m.Highscores.Count==1&&new PanicModel().Highscores.Count==1,"Enabling developer after result retracts this run only.");
                    m=Model("cheated-removal-retry",Good);m.StartRun(false);TimeoutResult(m);var p=PanicModel.ScorePathOverride;var before=Hash(p);
                    using(var held=new FileStream(p,FileMode.Open,FileAccess.Read,FileShare.Read)){m.SetDeveloperOption(PanicDeveloperOption.NoLoss,true);CheckThat("score.removal-fault-preserves","MEC-098 SYS-009",Hash(p)==before&&m.Highscores.Count==1,"Disk remains valid while pending removal is in memory.");}
                    m.ReturnToTitle();CheckThat("score.removal-retry","MEC-098 SYS-009",new PanicModel().Highscores.Count==1,"Pending cheated score removal persists before reset.");
                });
                Group("unranked-modes",()=>{
                    foreach(string mode in new[]{"short","simulation","tutorial"}){
                        var m=Model("unranked-"+mode,Good);var p=PanicModel.ScorePathOverride;var before=Hash(p);
                        if(mode=="short")m.StartJuryDemo();else if(mode=="simulation")m.StartSimulation(DisasterType.Fire);else m.StartRun(true);
                        m.CompletedWaves=3;m.Score=5000;
                        if(mode=="simulation"){m.EnterBunker();m.Tick(20);}m.ReturnToTitle();
                        CheckThat("unranked."+mode,"SYS-010 MEC-099",Hash(p)==before&&new PanicModel().Highscores.Count==1,"Fixture completed state and mode exit do not persist/append score.");
                    }
                });
                Group("developer-matrix",()=>{
                    for(int mask=0;mask<16;mask++){
                        var m=Model("mask"+mask);m.StartRun(false);
                        for(int bit=0;bit<4;bit++)m.SetDeveloperOption((PanicDeveloperOption)bit,(mask&(1<<bit))!=0);
                        var e=new PanicExpedition(m);Supply(e,"bracket",1,"screws",2);var item=m.Items.Find(x=>x.Id=="bookshelf");int budget=m.Budget;
                        bool done=Work(e,"anchorwall",item.Id);bool free=(mask&2)!=0,inf=(mask&1)!=0,freeze=(mask&4)!=0,noLoss=(mask&8)!=0;
                        CheckThat("matrix."+mask+".work-economy","MEC-091 SYS-014",done&&item.Anchored&&m.Budget==budget-(inf?0:60)&&e.Count("bracket")== (free?1:0)&&e.Count("screws")== (free?2:0)&&m.CheatedRun==(mask!=0),"mask="+mask+"; independent budget/material toggles; real two-click work commit.");
                        float clock=m.TimeRemaining;m.Tick(2);CheckThat("matrix."+mask+".prep-clock","MEC-092 SYS-014",Math.Abs(m.TimeRemaining-(freeze?clock:clock-2))<.001,"Freeze only preparing clock.");
                        int bi=m.Budget;Supply(e,"bandage",2,"antiseptic",1);bool crafted=e.CraftFirstAid();CheckThat("matrix."+mask+".craft","MEC-091 SYS-014",crafted&&e.Count("bandage")== (free?2:0)&&e.Count("antiseptic")== (free?1:0)&&m.Budget==bi,"Craft consumes materials only according to FreeMaterials.");
                        e.Inventory[3]=new SupplySlot("flashlight",1);e.Inventory[4]=new SupplySlot("battery",1);bool packed=e.PackSlot(3);CheckThat("matrix."+mask+".battery","MEC-091 SYS-014",packed&&e.Count("battery")== (free?1:0),"Battery ownership follows FreeMaterials, not InfiniteBudget.");
                        m.PackedWater=m.PackedFood=1;m.EnterBunker();m.Tick(2);CheckThat("matrix."+mask+".disaster-clock","MEC-092 SYS-014",m.Phase==PanicPhase.Disaster&&Math.Abs(m.PhaseTime-6)<.001,"Disaster clock advances with freeze enabled.");
                        m.Tick(10);CheckThat("matrix."+mask+".real-consumption","MEC-093 SYS-014",m.TotalDamage>0&&m.PackedWater==0&&m.PackedFood==0&&(!noLoss||m.LastReward==0&&m.LastWaveScore==0),"Real damage/supplies remain under NoLoss; no immunity/reward introduced.");
                        for(int bit=0;bit<4;bit++)m.SetDeveloperOption((PanicDeveloperOption)bit,false);
                        CheckThat("matrix."+mask+".sticky","SYS-011",m.CheatedRun==(mask!=0),"Turning all options off does not erase same-run cheating.");
                        m.ReturnToTitle();m.StartRun(false);CheckThat("matrix."+mask+".clean-new-run","MEC-094 SYS-015",!m.CheatedRun&&m.Budget==1000&&m.CompletedWaves==0,"New run with every option disabled resets transient state.");
                    }
                });
                Group("tutorial-cheat-reset",()=>{
                    foreach(var option in new[]{PanicDeveloperOption.InfiniteBudget,PanicDeveloperOption.FreeMaterials,PanicDeveloperOption.FreezePreparationTime,PanicDeveloperOption.NoLoss}){
                        var m=Model("tutorial-enabled-"+option);m.SetDeveloperOption(option,true);m.StartRun(true);m.FinishTutorial();
                        CheckThat("tutorial.cheat-latch."+option,"SYS-011 SYS-015 MEC-098",m.DeveloperAnyEnabled&&m.CheatedRun,"Active option must mark new campaign run after FinishTutorial; actual CheatedRun="+m.CheatedRun);
                    }
                    var exploit=Model("tutorial-score-bypass",Good);exploit.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget,true);exploit.StartRun(true);exploit.FinishTutorial();TimeoutResult(exploit);
                    CheckThat("tutorial.cheat-score-rejection","SYS-011 MEC-098",exploit.Highscores.Count==1&&new PanicModel().Highscores.Count==1,"Enabled InfiniteBudget after FinishTutorial must not persist ranked result; fixture terminal, not a full campaign.");
                });
                Group("budget-edges",()=>{
                    foreach(PanicPhase phase in Enum.GetValues(typeof(PanicPhase))){var m=Model("budget-"+phase);m.StartRun(false);m.Phase=phase;bool allow=phase==PanicPhase.Preparing||phase==PanicPhase.Tutorial||phase==PanicPhase.Inspection;bool zero=m.AddDeveloperBudget(0),negative=m.AddDeveloperBudget(-1);m.Budget=int.MaxValue-5;bool added=m.AddDeveloperBudget(1000);CheckThat("budget.phase."+phase,"MEC-094",!zero&&!negative&&added==allow&&m.Budget==(allow?int.MaxValue:int.MaxValue-5),"Positive phases plus saturating addition.");}
                });
                Group("boundaries",()=>{
                    foreach(float t in new[]{.0001f,0f,-.0001f}){var m=Model("time-edge");m.StartRun(false);m.TimeRemaining=t;m.Tick(.00005f);CheckThat("boundary.time."+t,"SYS-012",t>0?m.Phase==PanicPhase.Preparing:m.Phase==PanicPhase.GameOver,"Exact <=0 timeout; explicit fixture delta.");}
                    foreach(float damage in new[]{64.999f,65f,65.001f}){var m=Model("damage-edge");m.StartRun(false);m.TotalDamage=damage;var reason=(string)Invoke(m,"BuildFailureReason",true,1,1);CheckThat("boundary.damage."+damage,"SYS-012",(reason.Length>0)==(damage>65),"Source oracle is TotalDamage >65, equality survives; private pure reason invoked.");}
                    for(int days=0;days<3;days++){var m=Model("bag-edge");m.StartRun(false);m.RequiredDays=1;m.PackedFood=m.PackedWater=days;CheckThat("boundary.bag."+days,"SYS-012",m.BagReady==(days>=1),"Required amount minus one / exact / plus one.");}
                    var full=Model("bag-capacity");full.StartRun(false);var e=new PanicExpedition(full);Supply(e,"water",1);for(int i=0;i<e.Bag.Count;i++)e.Bag[i]=new SupplySlot("unknown-fixture",5);CheckThat("boundary.bag-full","SYS-012",!e.PackSlot(0)&&e.Count("water")==1&&full.PackedWater==0,"8 occupied fixture slots reject without item loss.");
                });
                Group("work-atomicity",()=>{
                    var m=Model("cancel");m.StartRun(false);var e=new PanicExpedition(m);Supply(e,"bracket",1,"screws",2);int b=m.Budget;bool can=e.CanBeginWork("anchorwall","bookshelf");e.Work.Begin("anchorwall","bookshelf");e.Work.TryClick(0);e.Work.Tick(.1f,0,false,false);e.Work.Cancel();CheckThat("work.cancel-free","SYS-013",can&&m.Budget==b&&e.Count("screws")==2&&!m.Items.Find(x=>x.Id=="bookshelf").Anchored,"Cancel before completion does not commit.");
                    bool worked=Work(e,"anchorwall","bookshelf");int after=m.Budget;bool duplicate=e.TryCompleteWork("anchorwall","bookshelf");CheckThat("work.single-commit","SYS-013",worked&&!duplicate&&after==b-60&&m.Budget==after&&e.Count("screws")==0,"Successful direct click work commits exact recipe once.");
                    m=Model("missing");m.StartRun(false);e=new PanicExpedition(m);Supply(e,"bracket",1,"screws",1);b=m.Budget;CheckThat("work.one-missing","SYS-013",!e.CanBeginWork("anchorwall","bookshelf")&&e.HasMissingRequirements&&m.Budget==b&&e.Count("screws")==1,"Exact missing count blocks work with no spending.");
                    m=Model("unsupported");m.StartRun(false);e=new PanicExpedition(m);Supply(e,"extinguisher",1);CheckThat("work.unsupported-fire-bed","SYS-013",!e.CanBeginWork("fire","bed")&&e.Count("extinguisher")==1,"Bed cannot receive extinguisher action.");
                    m=Model("busy");m.StartRun(false);e=new PanicExpedition(m);Supply(e,"bracket",1,"screws",2);e.Work.Begin("anchorwall","bookshelf");bool first=e.Work.TryClick(0),repeat=e.Work.TryClick(0),future=e.Work.TryClick(1);CheckThat("work.busy-click","SYS-013",first&&!repeat&&!future&&e.Work.CompletedSteps==0,"Busy/repeated/future click cannot skip accepted0.60s installation.");
                });
                Group("determinism",()=>{
                    string previousDigest=null;for(int i=0;i<3;i++){var m=Model("determinism"+i);m.StartSimulation(DisasterType.Flood);m.EnterBunker();m.Tick(12);var state=m.TotalDamage.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"|"+m.PackedWater+"|"+m.PackedFood+"|"+m.FailureReason+"|"+m.Phase;CheckThat("determinism.same."+i,"SYS-006",previousDigest==null||state==previousDigest,state);previousDigest=state;}
                    var a=Model("determinism-control");a.StartSimulation(DisasterType.Flood);a.Items.ForEach(x=>{x.Anchored=true;x.Tier=2;});a.EnterBunker();a.Tick(12);CheckThat("determinism.changed-protection","SYS-006",!previousDigest.StartsWith(a.TotalDamage.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"|"),"Changing only protection changes damage; shared seed is source constant0x20261016, no invented --seed flag.");
                });
                Group("no-loss-branches",()=>{
                    foreach(bool enabled in new[]{false,true}){var m=Model("timeout-noloss"+enabled);m.StartRun(false);if(enabled)m.SetDeveloperOption(PanicDeveloperOption.NoLoss,true);m.TimeRemaining=.001f;m.Tick(.002f);CheckThat("noloss.timeout."+enabled,"MEC-093",enabled?m.Phase==PanicPhase.Disaster&&m.DeveloperNoLossUsedThisWave:m.Phase==PanicPhase.GameOver,"NoLoss timeout enters real disaster; otherwise GameOver.");if(enabled){m.Tick(12);CheckThat("noloss.review-no-reward","MEC-093",m.TotalDamage>0&&m.Phase==PanicPhase.Inspection&&m.LastReward==0&&m.LastWaveScore==0&&m.CompletedWaves==0,"Real damaging resolution then unranked review.");}}
                    var off=Model("noloss-disabled-before-failure");off.StartRun(false);off.SetDeveloperOption(PanicDeveloperOption.NoLoss,true);off.SetDeveloperOption(PanicDeveloperOption.NoLoss,false);off.EnterBunker();off.Tick(12);CheckThat("noloss.disabled-failure","MEC-093",off.Phase==PanicPhase.GameOver&&off.CheatedRun&&off.DeveloperNoLossUsedThisWave,"Disabled NoLoss does not protect actual failed outcome; sticky score exclusion remains.");
                });
            }
            finally{PanicModel.ScorePathOverride=previous;report.finished=true;report.finishedUtc=DateTime.UtcNow.ToString("o");}
            return report;
        }
    }
}
