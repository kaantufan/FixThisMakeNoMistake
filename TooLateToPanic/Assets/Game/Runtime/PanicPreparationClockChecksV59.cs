using System;

namespace TooLateToPanic
{
    /// <summary>Opt-in actual model-clock boundaries, never a real-time or physical-route claim.</summary>
    public static class PanicPreparationClockChecksV59
    {
        public static void Run(Action<string,bool,string> check)
        {
            if(string.IsNullOrEmpty(PanicModel.ScorePathOverride))throw new InvalidOperationException("Fresh isolated score path required.");
            var m=new PanicModel();m.StartRun(false);
            check("clock.start130",m.TimeRemaining==130f&&m.PhaseTime==130f,"New ordinary run has exactly90+40seconds.");
            m.SetPaused(true);m.Tick(300f);
            check("clock.pause-no-aging",m.TimeRemaining==130f&&!m.CheatedRun,"A production pause does not spend either stage or mark cheating.");
            m.SetPaused(false);m.Tick(89f);
            check("clock.before-bag",m.TimeRemaining==41f&&m.Phase==PanicPhase.Preparing,"After89active modelseconds,one precaution second remains.");
            m.Tick(1f);
            check("clock.bag-boundary40",m.TimeRemaining==40f&&PanicHomeWorld.BagPreparationSeconds==40f,"The actual model clock reaches the shared40second bag gate after90active seconds; no camera claim.");
            m.Tick(39f);
            check("clock.bag-last-second",m.TimeRemaining==1f&&m.Phase==PanicPhase.Preparing,"All39elapsed bag seconds leave one real second.");
            m.Tick(1f);
            check("clock.deadline-real-loss",m.Phase==PanicPhase.GameOver&&m.TimeRemaining==0f&&!m.ReachedShelterV5&&m.Budget==1500&&m.LastReward==0&&m.Score==0&&m.CompletedWaves==0,"Unsheltered130second timeout remains a real unrewarded loss; no fake shelter or budget grant.");
            m.StartRun(false);m.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime,true);m.Tick(90f);
            check("clock.developer-freeze",m.TimeRemaining==130f&&m.CheatedRun,"Existing developer freeze still holds the complete clock and isolates score.");
            m.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime,false);m.Tick(90f);
            check("clock.unfreeze-boundary-sticky",m.TimeRemaining==40f&&m.CheatedRun,"Unfreeze spends the actual90seconds; score exclusion stays sticky.");
        }
    }
}
