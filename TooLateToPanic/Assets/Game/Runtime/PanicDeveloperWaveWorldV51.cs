using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        public bool CanDeveloperSkipWaveV51
        {
            get { return Model != null && Model.CanDeveloperSkipWaveV51 && !NotebookBlocksInput &&
                !(hud && hud.FreeplayOnboardingOpenV414) && (!CinematicActive || IntroStoryActiveV414 || Model.Phase == PanicPhase.Disaster || Model.Phase == PanicPhase.Aftermath) && !LossAnnounced; }
        }
        public bool DeveloperSkipWaveV51()
        {
            if(!CanDeveloperSkipWaveV51 || Model.IsPaused) return false;
            if(IntroStoryActiveV414) SkipIntroStoryV414();
            CancelWork(); CancelMove(); InventoryOpen=false;
            ResetBagPreparation();
            desiredVelocity=smoothVelocity=Vector3.zero;
            if(!Model.TryDeveloperSkipToDisasterV51()) return false;
            CancelFrame=DiscreteInputFrame=Time.frameCount;
            return true; // Update observes the actual phase and owns the usual cinematic.
        }
    }
    public sealed partial class PanicHomeHud
    {
        public void RequestDeveloperWaveSkipV51()
        {
            if(!World || !World.CanDeveloperSkipWaveV51)
            {
                _developerNotice="F3 · Dalga geçişi serbest oyunda, hazırlık ve hasar gezisinde kullanılabilir.";
                _developerNoticeUntil=Time.unscaledTime+3f;
                return;
            }
            if(_developerMenuOpen) SetDeveloperMenuOpen(false);
            if(_paused) SetPaused(false);
            CancelInventoryDrag(); _helpVisible=false;
            World.DeveloperSkipWaveV51();
            _developerTransitionFrame=Time.frameCount;
        }
    }
}
