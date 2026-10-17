using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>V5 presentation only. All values come from the active Model/Expedition.</summary>
    public sealed partial class PanicHomeHud
    {
        bool compactControlsExpandedV5 = true;
        int compactControlsInputFrameV5 = -1;
        GUIStyle compactBodyV5, compactValueV5;
        public bool CompactControlsExpandedV5 { get { return compactControlsExpandedV5; } }
        public Rect CompactControlsToggleBoundsV5 { get { return new Rect(24, 844, 244, 32); } }
        bool CompactControlsAllowedV5 { get { return World && World.Model != null && !FreeplayOnboardingOpenV414 && !_developerMenuOpen && !_paused && !World.NotebookBlocksInput; } }

        public void ToggleCompactControlsV5()
        {
            compactControlsExpandedV5 = !compactControlsExpandedV5;
            compactControlsInputFrameV5 = Time.frameCount;
        }

        void ProcessCompactControlsInputV5()
        {
            if (!CompactControlsAllowedV5) return;
            if (Input.GetKeyDown(KeyCode.F1)) ToggleCompactControlsV5();
        }

        void EnsureCompactStylesV5()
        {
            if (compactBodyV5 != null) return;
            compactBodyV5 = new GUIStyle(_small) { fontSize = 18, wordWrap = true };
            compactValueV5 = new GUIStyle(_heading) { fontSize = 25 };
        }

        void DrawCompactStatusV5()
        {
            EnsureCompactStylesV5();
            var model = World.Model;
            var wave = WaveCardBounds;
            Frame(wave, Night, Line, 1); Region(wave);
            DrawDisasterSymbol(new Rect(wave.x + 14, wave.y + 18, 26, 28), model.Disaster, Brass);
            string stage = World.JuryActive ? "HAZIRLIK" : "DALGA " + model.Wave;
            Label(new Rect(wave.x + 53, wave.y + 8, wave.width - 65, 22), stage + "  ·  " + PhaseName(model.Phase), _tiny);
            FittedLabel(new Rect(wave.x + 53, wave.y + 27, wave.width - 65, 30), DisasterName(model.Disaster), compactValueV5, MainText, 19);

            Rect timer = TimerCardBounds;
            bool preparing = model.Phase == PanicPhase.Preparing || model.Phase == PanicPhase.Tutorial;
            float value = World.JuryActive && World.JurySnapshot != null ? World.JurySnapshot.StageRemainingSeconds : preparing ? PreparationTimerRemaining : model.PhaseTime;
            string timerStage = _paused ? "DURDU" : model.FreezePreparationTime && preparing ? "DONDU" : CampaignPrecautionTimer ? "ÖNLEM" : preparing ? "HAZIRLIK" : PhaseName(model.Phase);
            Color timeColor = preparing && value <= 15 ? Brass : MainText;
            Frame(timer, Night, Line, 1); Region(timer);
            Label(new Rect(timer.x + 14, timer.y + 8, timer.width - 28, 19), timerStage, _tiny);
            Label(new Rect(timer.x + 14, timer.y + 27, timer.width - 28, 29), FormatTime(Mathf.Max(0, value)), ColorText(compactValueV5, timeColor));

            Rect budget = ResourceCardBounds;
            Frame(budget, Night, Line, 1); Region(budget);
            Label(new Rect(budget.x + 14, budget.y + 8, budget.width - 28, 19), "BÜTÇE", _tiny);
            Label(new Rect(budget.x + 14, budget.y + 27, budget.width - 28, 29), model.InfiniteBudget ? "SINIRSIZ" : "$ " + model.Budget.ToString("N0"), ColorText(compactValueV5, Brass));
        }

        void DrawCompactControlsV5()
        {
            if (!CompactControlsAllowedV5) return;
            EnsureCompactStylesV5();
            Rect toggle = CompactControlsToggleBoundsV5;
            Frame(toggle, Night, Line, 1); Region(toggle);
            FittedLabel(new Rect(toggle.x + 10, toggle.y + 2, toggle.width - 20, toggle.height - 4), "F1  TUŞLAR  " + (compactControlsExpandedV5 ? "−" : "+"), _tiny, MainText, 14);
            if (MousePressed(toggle)) { ToggleCompactControlsV5(); World.PlayUiSound(); }
            if (!compactControlsExpandedV5 || World.WorkActive || World.InventoryOpen) return;
            string text;
            if (World.CinematicActive || World.LossAnnounced) text = "ENTER  Sahneyi atla\nESC  Duraklat";
            else if (World.IsMoving) text = "FARE  Yerleştir · SOL TIK  Bırak\nTEKER / R  Döndür\nSAĞ TIK / ESC  İptal";
            else if (World.BagCameraOwnsView) text = "WASD  Yürü · SHIFT  Koş\nFARE  Bak · E / F  Al / Kullan\nTAB  Çanta · J  Defter · ESC  Duraklat";
            else if (World.Model.Phase == PanicPhase.Inspection) text = "WASD  Yürü · Q  Hedef değiştir\nENTER  Sonraki hasar · N  Devam\nJ  Defter · ESC  Duraklat";
            else if (World.Model.Phase == PanicPhase.Title) text = "OKLAR  Menüde seç\nENTER  Başlat · F1  Tuşları gizle";
            else if (World.Model.Phase == PanicPhase.GameOver || World.Model.Phase == PanicPhase.Success) text = "FARE  Sonuç seçeneklerini kullan\nF1  Tuşları gizle";
            else text = "WASD  Yürü · SHIFT  Koş · SPACE  Zıpla\nE  Kullan · Q  Hedef değiştir\nTAB  Çanta · J  Defter · ESC  Duraklat";
            int rows = World.CinematicActive || World.LossAnnounced || World.Model.Phase == PanicPhase.Title || World.Model.Phase==PanicPhase.GameOver || World.Model.Phase==PanicPhase.Success ? 2 : 3;
            Rect guide = new Rect(24, toggle.y - rows * 25 - 13, 352, rows * 25 + 7);
            Frame(guide, Night, Line, 1); Region(guide);
            Label(new Rect(guide.x + 12, guide.y + 5, guide.width - 24, guide.height - 8), text, ColorText(compactBodyV5, SoftText));
        }

        void DrawCompactTargetV5()
        {
            if (string.IsNullOrEmpty(World.FocusLabel) || World.LootVisible || World.IsMoving || World.WorkActive) return;
            Rect target = new Rect(450, 800, 700, 72);
            Frame(target, Night, Line, 1); Region(target);
            FittedLabel(new Rect(target.x + 14, target.y + 6, target.width - 235, 27), World.FocusLabel, _body, MainText, 18);
            FittedLabel(new Rect(target.x + 14, target.y + 37, target.width - 28, 26), World.CanInteract ? World.ContextHint : "Etkileşmek için yaklaş", _small, World.CanInteract ? Brass : Muted, 14);
        }

        void DrawCompactRequirementsV5()
        {
            var expedition = World.Expedition;
            Rect card = RequirementCardBounds;
            Frame(card, Night, Line, 1); Region(card);
            FittedLabel(new Rect(card.x + 12, card.y + 8, card.width - 24, 25), RequirementTitleV414(expedition), _body, Brass, 16);
            float y = card.y + 38;
            foreach (var requirement in expedition.CurrentRequirements)
            {
                if (requirement.Missing <= 0) continue;
                string name = PanicExpedition.DisplayName(requirement.Id);
                FittedLabel(new Rect(card.x + 12, y, card.width - 24, 25), name + "  " + requirement.Owned + "/" + requirement.Required + "  ·  eksik " + requirement.Missing, _small, SoftText, 14);
                y += 29;
                if (y >= card.yMax - 30) break;
            }
            if (expedition.RequirementBudgetMissing > 0)
                FittedLabel(new Rect(card.x + 12, card.yMax - 30, card.width - 24, 24), "Bütçe eksik: $ " + expedition.RequirementBudgetMissing, _small, Brass, 14);
        }

        void DrawCompactJuryPanelV5()
        {
            var state = World.JurySnapshot;
            if (state == null) return;
            if (state.Stage == PanicJuryStage.Result || state.Stage == PanicJuryStage.Complete) { DrawJuryResult(state); return; }
            Rect card = new Rect(24, 100, 392, 146);
            Frame(card, Night, Line, 1); Region(card);
            FittedLabel(new Rect(38, 109, 364, 30), state.Heading, _body, MainText, 18);
            Label(new Rect(38, 142, 364, 54), state.Hint, compactBodyV5);
            if (state.Stage == PanicJuryStage.Earthquake)
                Button(new Rect(38, 203, 364, 30), "ATLA · ENTER", false, Sea, World.SkipResidenceCinematic);
            else if (state.CanAdvance)
                Button(new Rect(38, 203, 364, 30), "DEVAM · ENTER", false, Sea, World.AdvanceJuryDemo);
            else if (state.CanSkipSegment)
                Button(new Rect(38, 203, 364, 30), "DEVAM ET", false, Line, World.SkipJurySegment);
            if (state.AllowsMovement) DrawContext();
            if (RequirementsCardVisible) DrawRequirements();
            if (World.NotificationVisible) DrawToast(World.ShortNotice);
            if ((state.CanAdvance || state.Stage == PanicJuryStage.Earthquake) && !DeveloperInputBlockedThisFrame && World.CancelFrame != Time.frameCount && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            { if (state.Stage == PanicJuryStage.Earthquake) World.SkipResidenceCinematic(); else World.AdvanceJuryDemo(); Event.current.Use(); }
        }
    }
}
