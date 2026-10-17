using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>V5 presentation only. All values come from the active Model/Expedition.</summary>
    public sealed partial class PanicHomeHud
    {
        bool compactControlsExpandedV5;
        int compactControlsInputFrameV5 = -1;
        GUIStyle compactBodyV5, compactValueV5, compactKeyV51;
        public bool CompactControlsExpandedV5 { get { return compactControlsExpandedV5; } }
        public Rect CompactControlsToggleBoundsV5 { get { return new Rect(24, 844, 154, 32); } }
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
            compactKeyV51 = new GUIStyle(_small) { fontSize = 21, alignment = TextAnchor.MiddleCenter, wordWrap = false };
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
            DrawCompactKeyV51(new Rect(toggle.x + 5, toggle.y + 4, 37, 24), "F1", 3);
            FittedLabel(new Rect(toggle.x + 49, toggle.y + 2, toggle.width - 57, toggle.height - 4), (compactControlsExpandedV5 ? "−" : "+"), compactKeyV51, MainText, 14);
            if (MousePressed(toggle)) { ToggleCompactControlsV5(); World.PlayUiSound(); }
            if (!compactControlsExpandedV5) return;
            const float guideScale = .7f;
            Rect guide = new Rect(0, 0, 440, 210);
            Rect guideBounds = new Rect(24, toggle.y - 12 - guide.height * guideScale, guide.width * guideScale, guide.height * guideScale);
            // Register the actual logical-canvas footprint before entering a local
            // drawing space. PointerOverUi always works in the outer logical canvas.
            Region(guideBounds);
            int firstGuideMark = _uiRects.Count;
            Matrix4x4 incoming = GUI.matrix;
            GUI.matrix = incoming * Matrix4x4.TRS(new Vector3(guideBounds.x, guideBounds.y, 0),
                Quaternion.identity, new Vector3(guideScale, guideScale, 1));
            try { DrawCompactKeyboardBodyV51(guide); }
            finally
            {
                GUI.matrix = incoming;
                // Label/FittedLabel register local child marks. Convert only this
                // guide's new marks so no invisible full-size hit rectangles remain.
                for (int i = firstGuideMark; i < _uiRects.Count; i++)
                {
                    Rect child = _uiRects[i];
                    _uiRects[i] = new Rect(guideBounds.x + child.x * guideScale, guideBounds.y + child.y * guideScale,
                        child.width * guideScale, child.height * guideScale);
                }
            }
        }

        void DrawCompactKeyboardBodyV51(Rect guide)
        {
            Frame(guide, Night, Line, 1);
            // Physical key positions only. Empty dim caps preserve the silhouette;
            // the separate full first-use onboarding owns all action explanations.
            DrawCompactKeyboardKeyV51(new Rect(10, 9, 36, 24), "Esc");
            DrawCompactKeyboardKeyV51(new Rect(60, 9, 28, 24), "F1");
            DrawCompactKeyboardKeyV51(new Rect(94, 9, 28, 24), "F2");
            DrawCompactKeyboardKeyV51(new Rect(128, 9, 28, 24), "F3");
            DrawCompactKeyboardRowV51(40, 10, new[] { "1", "2", "3", "4", "", "", "", "", "", "" });
            DrawCompactKeyboardKeyV51(new Rect(330, 40, 92, 26), "Back");
            DrawCompactKeyboardKeyV51(new Rect(10, 70, 42, 26), "Tab");
            DrawCompactKeyboardRowV51(70, 58, new[] { "Q", "W", "E", "R", "T", "", "", "", "", "" });
            DrawCompactKeyboardKeyV51(new Rect(382, 70, 40, 56), "Enter");
            DrawCompactKeyV51(new Rect(10, 100, 42, 26), "", 0);
            DrawCompactKeyboardRowV51(100, 58, new[] { "A", "S", "D", "F", "", "", "J", "", "" });
            DrawCompactKeyboardKeyV51(new Rect(10, 130, 66, 26), "Shift");
            DrawCompactKeyboardRowV51(130, 82, new[] { "Z", "X", "", "", "B", "N", "" });
            DrawCompactKeyV51(new Rect(314, 130, 40, 26), "", 0);
            DrawCompactKeyV51(new Rect(10, 168, 42, 28), "", 0);
            DrawCompactKeyV51(new Rect(58, 168, 42, 28), "", 0);
            DrawCompactKeyboardKeyV51(new Rect(106, 168, 176, 28), "Space");
            DrawCompactKeyV51(new Rect(288, 168, 30, 28), "", 0);
            DrawCompactKeyboardKeyV51(new Rect(362, 134, 28, 26), "↑");
            DrawCompactKeyboardKeyV51(new Rect(330, 168, 28, 28), "←");
            DrawCompactKeyboardKeyV51(new Rect(362, 168, 28, 28), "↓");
            DrawCompactKeyboardKeyV51(new Rect(394, 168, 28, 28), "→");
        }

        void DrawCompactKeyboardRowV51(float y, float x, string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
                DrawCompactKeyboardKeyV51(new Rect(x + i * 32, y, 28, 26), keys[i]);
        }

        void DrawCompactKeyboardKeyV51(Rect rect, string key)
        {
            DrawCompactKeyV51(rect, key, CompactKeyboardFamilyV51(key));
        }

        int CompactKeyboardFamilyV51(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;
            if (key == "F1") return 3;
            if (key == "F2") return string.IsNullOrEmpty(DeveloperUnavailableReason()) ? 3 : 0;
            if (key == "F3") return World.CanDeveloperSkipWaveV51 ? 3 : 0;
            if (World.CinematicActive || World.LossAnnounced) return key == "Enter" ? 4 : key == "Esc" ? 3 : 0;
            if (World.WorkActive)
            {
                if (key == "Esc" || key == "Back") return 3;
                var work = World.Expedition.Work;
                return work.UsesHammer ? key == "Space" ? 2 : 0 : work.UsesLinearDrag ? key == "↓" ? 2 : 0 : key == "→" ? 2 : 0;
            }
            if (World.IsMoving) return key == "Esc" ? 3 : key == "R" ? 2 : 0;
            bool arrows = key == "↑" || key == "←" || key == "↓" || key == "→";
            if (World.InventoryOpen)
            {
                if (arrows) return 1;
                if (key == "Enter") return 4;
                if (key == "F") return 2;
                return key == "Tab" || key == "B" || key == "Back" || key == "Esc" ? 3 : 0;
            }
            if (World.Model.Phase == PanicPhase.Title) return key == "↑" || key == "↓" ? 1 : key == "Enter" ? 4 : 0;
            if (World.Model.Phase == PanicPhase.GameOver || World.Model.Phase == PanicPhase.Success) return 0;
            if (arrows || key == "W" || key == "A" || key == "S" || key == "D" || key == "Shift") return 1;
            if (key == "Space") return World.BagCameraOwnsView ? 0 : 1;
            if (key == "J" || key == "Esc") return 3;
            if (World.Model.Phase == PanicPhase.Inspection) return key == "Q" ? 2 : key == "Enter" || key == "N" ? 4 : 0;
            if (key == "Tab" || key == "B") return 3;
            if (key == "E" || key == "F") return 2;
            if (World.BagCameraOwnsView) return 0;
            if (key == "Q" || key == "T") return 2;
            if (key == "Z" || key == "X") return World.LootVisible ? 2 : 0;
            var selected = World.Selected;
            if (selected == null || !World.CanInteract) return 0;
            if (key == "4") return selected.Movable && !selected.Anchored ? 2 : 0;
            int shortcut = PanicItemActionRules.CanReleaseAnchor(selected) ? 1 : World.Expedition.PrimaryShortcutV414(selected);
            return shortcut > 0 && key == shortcut.ToString() ? 2 : 0;
        }

        void DrawCompactKeyV51(Rect rect, string key, int family)
        {
            Frame(rect, FreeplayKeyFillV414(family), FreeplayKeyEdgeV414(family), 1f);
            FittedLabel(new Rect(rect.x + 3, rect.y + 1, rect.width - 6, rect.height - 2), key, compactKeyV51, family == 0 ? Muted : MainText, 13);
        }
        void DrawCompactTargetV5()
        {
            // V5.1: the real object owns its projected primary marker again.
            // DrawContext still keeps actual loot, placement and applicable-action controls.
            // Do not repeat the same object label in a large bottom panel.
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
