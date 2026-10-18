using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in independent accounting checks. All altered stocks, items, seeds and memo entries are explicit model fixtures.</summary>
    public static class PanicWaveLedgerModelChecksV56
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static Action<string,bool,string> check;
        static string directory;
        static int fixture;
        public static int FixtureModels { get; private set; }
        public static int SeedCases { get; private set; }

        public static object Read(object owner,string name)
        {
            var t=owner.GetType();var p=t.GetProperty(name,Flags);if(p!=null)return p.GetValue(owner);
            var f=t.GetField(name,Flags);if(f==null)throw new MissingMemberException(t.FullName,name);return f.GetValue(owner);
        }
        public static T Read<T>(object owner,string name){return (T)Read(owner,name);}
        public static object Call(object owner,string name,params object[] args)
        {return owner.GetType().GetMethods(Flags).First(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(owner,args);}
        static void Set(object owner,string field,object value){owner.GetType().GetField(field,Flags).SetValue(owner,value);}
        public static bool Near(float a,float b){return !float.IsNaN(a)&&!float.IsInfinity(a)&&!float.IsNaN(b)&&!float.IsInfinity(b)&&Mathf.Abs(a-b)<.003f;}
        public static object Ledger(PanicModel m){return Read(m,"LastWaveLedgerV56");}
        public static object[] Rows(object ledger){return ((IEnumerable)Read(ledger,"Rows")).Cast<object>().ToArray();}
        public static string LedgerStamp(object ledger)
        {
            if(ledger==null)return "absent";var s=new StringBuilder();
            foreach(string name in new[]{"Wave","DisasterLabel","IsSimulation","OutcomeLabel","TotalDamage","BudgetBeforeReward","BaseReward","DamageDeduction","SupportBonus","ActualCredited","BudgetAfterReward","RewardExplanation","CapacityDeduction","CalculatedReward","HasResolvedDamage"})s.Append(name).Append('=').Append(Read(ledger,name)).Append('|');
            foreach(var row in Rows(ledger))foreach(string name in new[]{"Id","Label","Detail","DamageContribution"})s.Append(name).Append('=').Append(Read(row,name)).Append('|');
            return s.ToString();
        }
        public static string ModelStamp(PanicModel m)
        {
            var s=new StringBuilder();s.Append(m.Phase).Append('|').Append(m.Wave).Append('|').Append(m.Budget).Append('|').Append(m.CompletedWaves).Append('|').Append(m.Score).Append('|').Append(m.LastReward).Append('|').Append(m.TotalDamage).Append('|').Append(m.PackedWater).Append('|').Append(m.PackedFood).Append('|').Append(m.PackedSupportMask).Append('|').Append(m.SecondaryRollCountV5).Append('|').Append(m.SecondaryDamageV5).Append('|').Append(m.ActiveDisasterMaskV54);
            foreach(var item in m.Items)s.Append('|').Append(item.Id).Append(':').Append(item.Damage).Append(':').Append(item.Contribution);
            foreach(var e in m.SecondaryEventsV5)s.Append('|').Append(e.Kind).Append(':').Append(e.TargetId).Append(':').Append(e.DamagePoints);
            return s.ToString();
        }
        public static void VerifyLedger(string label,PanicModel m,int actualBudgetBefore,bool rewarded,Action<string,bool,string> emit)
        {
            object ledger=Ledger(m);emit(label+".snapshot-exists",ledger!=null,"Snapshot must exist after the committed outcome.");if(ledger==null)return;
            int before=Read<int>(ledger,"BudgetBeforeReward"),after=Read<int>(ledger,"BudgetAfterReward"),credited=Read<int>(ledger,"ActualCredited");
            emit(label+".actual-budget-reconciles",before==actualBudgetBefore&&after==m.Budget&&credited==(long)m.Budget-actualBudgetBefore,"Observed before="+actualBudgetBefore+" after="+m.Budget+"; snapshot="+before+" -> "+after+" credited="+credited);
            int basis=rewarded?Math.Max(0,Mathf.RoundToInt(1600f+100f*Mathf.Max(0,m.Wave-1))):0;
            int afterDamage=rewarded?Math.Max(0,Mathf.RoundToInt(basis*(1f-.5f*m.TotalDamage/100f))):0;
            int calculated=rewarded?Math.Max(0,Mathf.RoundToInt(basis*(1f-.5f*m.TotalDamage/100f)*(1f+.05f*m.SupportCount))):0;
            int deduction=basis-afterDamage,bonus=calculated-afterDamage,capacity=calculated-credited;
            emit(label+".independent-existing-reward-rule",Read<int>(ledger,"BaseReward")==basis&&Read<int>(ledger,"DamageDeduction")==deduction&&Read<int>(ledger,"SupportBonus")==bonus&&Read<int>(ledger,"CalculatedReward")==calculated&&m.LastReward==calculated,"Independent formula uses Wave, actual TotalDamage and actual packed SupportCount="+m.SupportCount+"; base="+basis+" deduction="+deduction+" bonus="+bonus+" expected="+calculated);
            emit(label+".integer-components-reconcile",Read<int>(ledger,"BaseReward")-Read<int>(ledger,"DamageDeduction")+Read<int>(ledger,"SupportBonus")-Read<int>(ledger,"CapacityDeduction")==credited&&Read<int>(ledger,"CapacityDeduction")==capacity,"Base - damage + support - saturation = actual budget delta; capacity="+capacity);
            object[] rows=Rows(ledger);float sum=rows.Sum(r=>Read<float>(r,"DamageContribution"));
            emit(label+".all-rows-reconcile-damage",Near(sum,m.TotalDamage)&&Near(Read<float>(ledger,"TotalDamage"),m.TotalDamage)&&rows.All(r=>Read<float>(r,"DamageContribution")>=0&&!string.IsNullOrWhiteSpace(Read<string>(r,"Label"))),"Rows="+rows.Length+" sum="+sum+"; actual TotalDamage="+m.TotalDamage+". Item rows must exclude separately listed secondary points.");
            emit(label+".result-metadata",Read<int>(ledger,"Wave")==m.Wave&&Read<bool>(ledger,"IsSimulation")==m.IsSimulation&&!string.IsNullOrWhiteSpace(Read<string>(ledger,"OutcomeLabel"))&&!string.IsNullOrWhiteSpace(Read<string>(ledger,"RewardExplanation")),"Wave/mode/outcome/reward explanation.");
        }
        public static void Run(string isolatedEvidence,Action<string,bool,string> emit)
        {
            if(string.IsNullOrWhiteSpace(isolatedEvidence)||string.IsNullOrWhiteSpace(PanicModel.ScorePathOverride))throw new ArgumentException("An isolated GUID evidence profile must be set before model checks.");
            string previous=PanicModel.ScorePathOverride;check=emit;fixture=FixtureModels=SeedCases=0;
            directory=Path.Combine(Path.GetFullPath(isolatedEvidence),"v56-model-fixtures-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
            try{Budgets();Outcomes();SeedSweep();Compound();}finally{PanicModel.ScorePathOverride=previous;directory=null;check=null;}
        }
        static PanicModel New(string name,bool supply=true,bool low=true,bool simulation=false)
        {
            PanicModel.ScorePathOverride=Path.Combine(directory,(++fixture)+"-"+name+".json");var m=new PanicModel();FixtureModels++;
            if(simulation)m.StartSimulation(DisasterType.Earthquake);else m.StartRun(false);
            // Explicit toy item fixture; actual CalculateDamage and outcome guards remain production-owned.
            m.Items.Clear();if(low)m.Items.Add(Item("qa_v56_furniture",ItemKind.Furniture,"bookshelf"));
            Close(m,"gasoff");Close(m,"wateroff");Close(m,"poweroff");
            if(supply&&!simulation){m.WaterStock=m.FoodStock=1;m.PackWater();m.PackFood();}
            return m;
        }
        static PanicItem Item(string id,ItemKind kind,string art)
        {return new PanicItem{Id=id,Label="QA fixture "+id,Kind=kind,ArtId=art,Installed=true,Size=Vector3.one};}
        static void Close(PanicModel m,string action){Call(m.UtilitySafety,"Complete",action);}
        static void Budgets()
        {
            PanicModel.ScorePathOverride=Path.Combine(directory,"initial.json");var m=new PanicModel();FixtureModels++;
            check("model.budget.unstarted-title",m.Phase==PanicPhase.Title,"Unstarted constructor budget="+m.Budget+" is not an active run; starting1500 is asserted at StartRun/reset below.");
            m.StartRun(false);check("model.budget.new-normal",m.Budget==1500,"Normal run budget="+m.Budget);
            m.AddDeveloperBudget(123);m.ReturnToTitle();check("model.budget.return-title-reset",m.Budget==1500,"Reset discards prior developer change.");
            m.StartRun(true);check("model.budget.tutorial",m.Budget==1500,"Tutorial uses same starting budget.");m.ReturnToTitle();m.StartSimulation(DisasterType.Flood);check("model.budget.simulation",m.Budget==1500,"Simulation starting budget.");
            m.ReturnToTitle();m.StartJuryDemo();check("model.budget.short-flow",m.Budget==1500,"Short-flow model starting budget.");
        }
        static void Resolve(string label,PanicModel m,bool rewarded)
        {
            int budget=m.Budget;check(label+".enter-model-shelter",m.TryEnterShelterV5(m.RequiredShelterV5),"Typed MODEL gate only; no world traversal/physical gate claim.");m.Tick(8f);
            check(label+".damage-before-credit",m.Phase==PanicPhase.Aftermath&&m.Budget==budget&&m.LastReward==0,"Actual damage and supply consumption resolve before award.");m.Tick(4f);VerifyLedger(label,m,budget,rewarded,check);
            check(label+".resolved-damage",Read<bool>(Ledger(m),"HasResolvedDamage"),"Actual production disaster resolution occurred.");
            if(m.Phase==PanicPhase.Success)m.Tick(3f);
            string state=ModelStamp(m),ledger=LedgerStamp(Ledger(m));float time=m.PhaseTime;
            bool opened=(bool)Call(m,"SetWaveLedgerOpenV56",true);m.Tick(100f);m.FinishInspection();
            check(label+".ledger-freezes-model",opened&&Read<bool>(m,"WaveLedgerOpenV56")&&state==ModelStamp(m)&&Near(time,m.PhaseTime),"List holds phase/timer/reward/supplies/secondary RNG; FinishInspection refuses while list open.");
            check(label+".repeated-reads-immutable",Enumerable.Range(0,16).All(_=>LedgerStamp(Ledger(m))==ledger),"16 snapshot reads.");Call(m,"SetWaveLedgerOpenV56",false);
        }
        static void Outcomes()
        {
            var zero=New("zero",true,false);Resolve("model.zero-success",zero,true);check("model.zero-success.full-base",zero.TotalDamage==0&&zero.LastReward==1600,"No damage: real credited base1600.");
            var damage=New("damage");Resolve("model.damage-success",damage,true);check("model.damage-success.actual-deduction",damage.TotalDamage>0&&Read<int>(Ledger(damage),"DamageDeduction")>0,"Actual damaged item drives reduced reward.");
            var support=New("support");support.SupportOwnedMask=3;support.PackSupport(0);support.PackSupport(1);Resolve("model.support-success",support,true);check("model.support-success.bonus",support.SupportCount==2&&Read<int>(Ledger(support),"SupportBonus")>0,"Two explicit owned-stock fixtures, real PackSupport APIs, existing +5% each.");
            var cap=New("capacity",true,false);cap.Budget=int.MaxValue-7;Resolve("model.capacity-saturation",cap,true);check("model.capacity-saturation.actual-seven",Read<int>(Ledger(cap),"ActualCredited")==7&&Read<int>(Ledger(cap),"CapacityDeduction")==1593,"Actual saturating budget delta must never claim +1600.");
            var noloss=New("noloss",false);noloss.SetDeveloperOption(PanicDeveloperOption.NoLoss,true);Resolve("model.noloss-no-award",noloss,false);check("model.noloss-no-award.sticky",noloss.CheatedRun&&noloss.CompletedWaves==0&&noloss.Score==0,"Missing supplies remain failure evidence under developer protection.");
            var skip=New("f3",false);int budget=skip.Budget;check("model.f3.production-skip",skip.TryDeveloperSkipToDisasterV51(),"Actual one-wave unranked skip API.");skip.Tick(12f);VerifyLedger("model.f3-no-award",skip,budget,false,check);
            var failure=New("failure",false);Resolve("model.failure-no-award",failure,false);check("model.failure-no-award.actual-loss",failure.Phase==PanicPhase.GameOver,"Missing real required stocks loses normally.");
            var high=New("damage-failure",true,false);for(int i=0;i<20;i++)high.Items.Add(Item("qa_v56_roof_"+i,ItemKind.Roof,"roof"));Resolve("model.damage-failure-no-award",high,false);check("model.damage-failure.threshold",high.TotalDamage>=75f&&high.Phase==PanicPhase.GameOver,"75% threshold remains actual production rule.");
            var sim=New("simulation",true,true,true);Resolve("model.simulation-no-award",sim,false);check("model.simulation-no-award.unranked",sim.IsSimulation&&sim.SimulationComplete&&sim.CompletedWaves==0&&sim.Score==0,"Selectable simulation remains unranked.");
            var deadline=New("deadline",false);budget=deadline.Budget;deadline.Tick(deadline.TimeRemaining+.01f);VerifyLedger("model.deadline-no-award",deadline,budget,false,check);check("model.deadline-no-fake-damage",!Read<bool>(Ledger(deadline),"HasResolvedDamage")&&deadline.TotalDamage==0,"Deadline has no fabricated disaster item damage.");
            object oldLedger=Ledger(damage);string oldStamp=LedgerStamp(oldLedger);damage.Tick(3f);int beforeBudget=damage.Budget,rolls=damage.SecondaryRollCountV5;damage.FinishInspection();int wave=damage.Wave;damage.FinishInspection();
            check("model.continue-once",wave==2&&damage.Wave==wave&&damage.Budget==beforeBudget&&damage.SecondaryRollCountV5==rolls,"Success to inspection then production FinishInspection twice: one new wave, no extra credit or RNG.");
            check("model.prior-snapshot-retained-value",LedgerStamp(oldLedger)==oldStamp,"Retained immutable snapshot remains stable after new-wave reset clears live item damage.");damage.ReturnToTitle();check("model.reset-clears-ledger",Ledger(damage)==null&&!Read<bool>(damage,"WaveLedgerOpenV56")&&damage.Budget==1500,"Title/reset removes stale result and open-list state.");
        }
        static void SeedSweep()
        {
            bool ranges=true,memo=true,closed=true,rowSums=true;int gas=0,water=0,electric=0;float gasMin=99,gasMax=0,waterMin=99,waterMax=0;
            for(uint index=1;index<=512;index++)
            {
                // Spread deterministic fixtures across uint32; adjacent low seeds are correlated under the production xorshift.
                uint seed=unchecked(index*0x9E3779B9u);
                var m=New("seed-"+seed);m.UtilitySafety.Reset();Set(m,"secondarySeedV5",seed);int before=m.Budget;m.TryEnterShelterV5(m.RequiredShelterV5);m.Tick(8f);SeedCases++;
                float damage=m.TotalDamage,secondary=m.SecondaryDamageV5;int rolls=m.SecondaryRollCountV5;string events=string.Join("|",m.SecondaryEventsV5.Select(e=>e.Kind+":"+e.TargetId+":"+e.DamagePoints));
                foreach(var e in m.SecondaryEventsV5)
                {
                    if(e.Kind==PanicSecondaryEventKindV5.GasFlare){gas++;gasMin=Mathf.Min(gasMin,e.DamagePoints);gasMax=Mathf.Max(gasMax,e.DamagePoints);ranges&=e.DamagePoints>=6f&&e.DamagePoints<=10f;}
                    else if(e.Kind==PanicSecondaryEventKindV5.PipeLeak){water++;waterMin=Mathf.Min(waterMin,e.DamagePoints);waterMax=Mathf.Max(waterMax,e.DamagePoints);ranges&=e.DamagePoints>=4f&&e.DamagePoints<=7f;}
                    else{electric++;ranges&=e.DamagePoints>=1.2f&&e.DamagePoints<=2.8f;}
                    var prevented=New("closed-"+seed);prevented.UtilitySafety.Reset();Close(prevented,e.Kind==PanicSecondaryEventKindV5.GasFlare?"gasoff":e.Kind==PanicSecondaryEventKindV5.PipeLeak?"wateroff":"poweroff");Set(prevented,"secondarySeedV5",seed);prevented.TryEnterShelterV5(prevented.RequiredShelterV5);prevented.Tick(8f);closed&=prevented.SecondaryEventsV5.All(p=>p.Kind!=e.Kind);
                }
                Call(m,"CalculateDamage");memo&=m.SecondaryRollCountV5==rolls&&Near(m.TotalDamage,damage)&&Near(m.SecondaryDamageV5,secondary)&&events==string.Join("|",m.SecondaryEventsV5.Select(e=>e.Kind+":"+e.TargetId+":"+e.DamagePoints));
                m.Tick(4f);object ledger=Ledger(m);rowSums&=ledger!=null&&Near(Rows(ledger).Sum(r=>Read<float>(r,"DamageContribution")),m.TotalDamage)&&Read<int>(ledger,"ActualCredited")==m.Budget-before;
            }
            check("model.seed512.actual-event-coverage",gas>0&&water>0&&electric>0,"512 deterministic full-32-bit seed fixtures (index*0x9E3779B9): gas="+gas+" water="+water+" electric="+electric);
            check("model.seed512.strength-ranges",ranges&&gas>0&&water>0&&electric>0,"Actual post-resolution gas="+gasMin+".."+gasMax+" [6,10], water="+waterMin+".."+waterMax+" [4,7], electricity unchanged[1.2,2.8]; one hazard avoids compound weighting/cap.");
            check("model.seed512.closed-kind-prevented",closed,"For each actual triggered seed, same seed with that real utility isolated cannot emit that kind; other open utilities may emit another event.");
            check("model.seed512.memoized-no-reroll",memo,"Repeated actual CalculateDamage keeps roll counter, event membership, points and total.");
            check("model.seed512.rows-and-budget-reconcile",rowSums,"Every outcome rows sum to actual total and actual budget delta equals committed credited value.");
        }
        static void Compound()
        {
            foreach(bool capped in new[]{false,true})
            {
                string label="model.compound-"+(capped?"capped":"weighted");var m=New(label);m.UtilitySafety.Reset();m.Wave=2;m.Disaster=DisasterType.Earthquake;
                Set(m,"compoundWaveV54",2);Set(m,"compoundPrimaryV54",DisasterType.Earthquake);Set(m,"compoundMaskV54",3);
                if(capped)for(int i=0;i<20;i++)m.Items.Add(Item("qa_cap_roof_"+i,ItemKind.Roof,"roof"));
                var cache=Read<Dictionary<string,PanicSecondaryEventV5>>(m,"secondaryRollsV5");
                cache["2:0"]=new PanicSecondaryEventV5(PanicSecondaryEventKindV5.GasFlare,"qa_v56_furniture",7f,"QA memo fixture: actual permitted gas7");
                cache["2:1"]=new PanicSecondaryEventV5(PanicSecondaryEventKindV5.PipeLeak,"qa_v56_furniture",5f,"QA memo fixture: actual permitted water5");
                // Independent attribution oracle includes the V5.9 shared-house composition.
                Call(m,"CalculateSingleDisasterDamageV54",true);float quake=m.TotalDamage;
                m.Disaster=DisasterType.Flood;Call(m,"CalculateSingleDisasterDamageV54",true);float flood=m.TotalDamage;m.Disaster=DisasterType.Earthquake;
                float weighted=quake+.45f*flood;
                float sharedScale=weighted>0?Mathf.Min(100,Mathf.Max(quake,flood)+.03f*weighted/1.45f)/weighted:0;
                // This fixture has no anchor and open electricity: nominal20+25 shares
                // the new40 critical cap, then the same100 household cap as all rows.
                float criticalScale=Mathf.Min(1f,100f/(weighted*sharedScale+40f));
                int before=m.Budget;m.TryEnterShelterV5(m.RequiredShelterV5);m.Tick(8f);float expected=(7f+5f*.45f)*sharedScale*criticalScale;
                check(label+".secondary-weighting",Near(m.SecondaryDamageV5,expected),"Actual memo fixtures7+5*.45 share composition scale="+sharedScale+" and independently expected critical40/household100 scale="+criticalScale+"; expected="+expected+"; secondary="+m.SecondaryDamageV5+" total="+m.TotalDamage);
                m.Tick(4f);VerifyLedger(label,m,before,!capped,check);
                object ledger=Ledger(m);var rows=Rows(ledger);float utilities=rows.Where(r=>Read<string>(r,"Id").StartsWith("utility:",StringComparison.Ordinal)).Sum(r=>Read<float>(r,"DamageContribution"));
                check(label+".separate-secondary-once",Near(utilities,m.SecondaryDamageV5)&&rows.Count(r=>Read<string>(r,"Id").StartsWith("utility:",StringComparison.Ordinal))==2,"Two separate actual weighted secondary rows; no double counting within ordinary item rows.");
                bool rowMutable=rows.Any(r=>r.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance).Any(p=>p.CanWrite));
                check(label+".snapshot-publicly-readonly",!rowMutable&&!ledger.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance).Any(p=>p.CanWrite),"Snapshot and row public properties have no setters.");
            }
        }
    }
}
