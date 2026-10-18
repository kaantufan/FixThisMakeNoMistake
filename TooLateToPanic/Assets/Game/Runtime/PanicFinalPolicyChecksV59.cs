using System;
using System.Reflection;

namespace TooLateToPanic
{
    /// <summary>Opt-in isolated model checks for the final timing and opening sequence.</summary>
    public static class PanicFinalPolicyChecksV59
    {
        public static void Run(Action<string,bool,string> check)
        {
            if(string.IsNullOrEmpty(PanicModel.ScorePathOverride))throw new InvalidOperationException("Isolated score path required.");
            var e=PanicEconomyChecksV59.New(59001);var m=e.Model;
            check("v59.final.timing",m.TimeRemaining==130&&PanicModel.NormalPrecautionSeconds==90&&PanicModel.NormalBagSeconds==40&&PanicModel.NormalPreparationSeconds==130,"90 seconds precautions,40 seconds bag; one canonical sum.");
            check("v59.final.wave1-earthquake",m.Wave==1&&m.Disaster==DisasterType.Earthquake&&m.ActiveDisasterMaskV54==1,"Actual new normal run.");
            foreach(var supply in e.WorldSupplies)if((supply.ItemId=="water"||supply.ItemId=="food")&&e.Count(supply.ItemId)==0)e.TryCollectWorldSupply(supply.Id);
            m.TryEnterShelterV5(m.RequiredShelterV5);m.Tick(8);m.Tick(4);m.Tick(3);
            m.SetWaveLedgerOpenV56(true);m.SetWaveLedgerOpenV56(false);m.FinishInspection();e.OnNewWave();
            check("v59.final.wave2-fixed-flood-storm",m.Wave==2&&m.Disaster==DisasterType.Flood&&m.ActiveDisasterMaskV54==10&&m.ActiveDisasterCountV54==2&&m.RequiredShelterV5==PanicShelterKindV5.PanicRoom&&m.TimeRemaining==130,"Actual successful wave1 -> wave2; upper shelter remains required for Flood.");
            var select=typeof(PanicModel).GetMethod("SelectCompoundDisastersV54",BindingFlags.Instance|BindingFlags.NonPublic);
            bool fixedAcrossSeeds=true;
            for(uint seed=1;seed<=64;seed++)
            {
                typeof(PanicModel).GetField("compoundSeedV54",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(m,seed*0x9E3779B9u);
                select.Invoke(m,null);fixedAcrossSeeds&=m.Disaster==DisasterType.Flood&&m.ActiveDisasterMaskV54==10;
            }
            check("v59.final.wave2-seed-independent",fixedAcrossSeeds,"64 fixed seed fixtures; no wave2 draw can change Flood+Storm.");
            bool original=true;int union=0,counts=0;
            for(uint seed=1;seed<=64;seed++)for(int wave=3;wave<=20;wave++)
            {
                uint runSeed=unchecked(seed*0x9E3779B9u);m.Wave=wave;
                typeof(PanicModel).GetField("compoundSeedV54",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(m,runSeed);select.Invoke(m,null);
                uint rng=runSeed^unchecked((uint)wave*0x9E3779B9u);int roll=(int)(Next(ref rng)%100u),count=roll<50?1:roll<85?2:3;
                int[] pool={0,1,2,3};int mask=0,primary=-1;
                for(int i=0;i<count;i++){int selected=i+(int)(Next(ref rng)%(uint)(4-i));int value=pool[selected];pool[selected]=pool[i];pool[i]=value;if(i==0)primary=value;mask|=1<<value;}
                original&=m.ActiveDisasterMaskV54==mask&&(int)m.Disaster==primary&&m.ActiveDisasterCountV54==count;union|=mask;counts|=1<<count;
            }
            check("v59.final.wave3plus-original-rng",original&&union==15&&counts==14,"1152 seed/wave fixtures compare original selector arithmetic; four types and1/2/3 hazards preserved.");
        }
        static uint Next(ref uint state){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
    }
}
