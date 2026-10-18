using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        int dialogueWaveV59 = -1;
        PanicPhase dialoguePhaseV59 = (PanicPhase)(-1);
        string dialogueOpeningV59;
        bool dialogueBagV59, dialogueReadyV59;
        public bool DialogueSuspendedV59
        {
            get
            {
                return Model == null || Model.IsPaused || Model.WaveLedgerOpenV56 || Model.KeyboardHelpOpenV56 ||
                    NotebookBlocksInput || InventoryOpen || WorkActive || IsMoving || LossAnnounced || BagCameraTransitionActive ||
                    (InputLocked && !CinematicActive) || (!Application.isFocused && !Application.runInBackground) ||
                    (hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame || hud.AudioSettingsOpenV56 ||
                        hud.DialogueHiddenModalV59));
            }
        }
        public string PresentationSubtitleKeyV59 { get { return presentationAudio ? presentationAudio.SubtitleKeyV59 : ""; } }
        public int DialogueBlipCountV59 { get { return presentationAudio ? presentationAudio.BitDispatchCountV59 : 0; } }

        // Called by the existing presentation tick, after phase/cinematic ownership has settled.
        void ObserveDialogueV59()
        {
            if (Model == null) return;
            if (Model.Phase == PanicPhase.Title) { ResetDialogueV59(); return; }
            if (!PresentationAudioEnabled) ConfigurePresentationAudio(true);
            if (!presentationAudio || DialogueSuspendedV59) return;
            if (dialogueWaveV59 != Model.Wave)
            {
                dialogueWaveV59 = Model.Wave;
                dialoguePhaseV59 = (PanicPhase)(-1); dialogueOpeningV59 = null;
                dialogueBagV59 = dialogueReadyV59 = false;
                presentationAudio.BeginScopeV59("wave:" + Model.Wave);
            }
            if (IntroStoryActiveV414)
            {
                // The cinematic already renders this exact shared catalog line. Only its bounded bit cue is added.
                string text = IntroStoryTextV414;
                if (dialogueOpeningV59 != text)
                {
                    dialogueOpeningV59 = text;
                    presentationAudio.CancelSpeech();
                    presentationAudio.TryCaptionV59("opening:" + text, text, 35, 4f);
                }
                return;
            }
            if (dialogueOpeningV59 != null)
            { dialogueOpeningV59 = null; presentationAudio.CancelSpeech(); }
            // The short flow already dispatches its own current-stage events; do not narrate campaign directions over it.
            if (JuryActive) return;
            if (dialoguePhaseV59 != Model.Phase)
            {
                dialoguePhaseV59 = Model.Phase;
                presentationAudio.CancelSpeech();
                string key = "phase:" + Model.Phase;
                string text = Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial ?
                    (Model.IsSimulation || Model.IsTutorial ? PanicKktcStoryV53.OpeningPreparationCaption() :
                        Model.ActiveDisasterCountV54 > 1 ? "Kalkanlı · Dalga " + Model.Wave + " · " + Model.ActiveDisasterLabelV54 + ". Evini hazırla." :
                        PanicKktcStoryV53.CampaignWaveNarrative(Model.Wave, Model.Disaster)) :
                    Model.Phase == PanicPhase.Disaster ? CompoundHazardCaptionV54() :
                    Model.Phase == PanicPhase.Inspection ? CompoundAftermathCaptionV54() : null;
                if (text != null) presentationAudio.TryCaptionV59(key, text, Model.Phase == PanicPhase.Disaster ? 50 : 20);
            }
            if (BagFirstPersonActive && !dialogueBagV59)
            {
                dialogueBagV59 = true; presentationAudio.CancelSpeech();
                presentationAudio.TryCaptionV59("bag:started", PanicKktcStoryV53.OpeningBagCaption(), 30);
            }
            if (BagFirstPersonActive && Model.BagReady && !dialogueReadyV59)
            {
                dialogueReadyV59 = true; presentationAudio.CancelSpeech();
                // This reads the actual selected campaign shelter. No route, loot or distant target is exposed.
                presentationAudio.TryCaptionV59("bag:ready", Model.HasActiveDisasterV54(DisasterType.Flood) ?
                    "Su ve gıda hazır. Üst kat panik odasında E ile gir." :
                    "Su ve gıda hazır. Bodrum sığınağında E ile gir.", 40);
            }
        }
        void ResetDialogueV59()
        {
            dialogueWaveV59 = -1; dialoguePhaseV59 = (PanicPhase)(-1); dialogueOpeningV59 = null;
            dialogueBagV59 = dialogueReadyV59 = false;
        }
    }
}
