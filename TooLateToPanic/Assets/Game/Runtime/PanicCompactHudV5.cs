using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>V5 presentation only. All values come from the active Model/Expedition.</summary>
    public sealed partial class PanicHomeHud
    {
        bool compactControlsExpandedV5;
        int compactControlsInputFrameV5 = -1;
        GUIStyle compactBodyV5, compactValueV5, compactKeyV51;
        public bool CompactControlsExpandedV5 { get { return KeyboardHelpVisibleV56; } }
        public Rect CompactControlsToggleBoundsV5 { get { return new Rect(24, 844, 154, 32); } }
        bool CompactControlsAllowedV5 { get { return World && World.Model != null && !FreeplayOnboardingOpenV414 && !_developerMenuOpen && !_paused && !World.NotebookBlocksInput && !KeyboardHelpBlockingV56 && KeyboardHelpCanOpenV56; } }

        public void ToggleCompactControlsV5()
        {
            if (KeyboardHelpVisibleV56) CloseKeyboardHelpV56(); else OpenKeyboardHelpV56();
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
            DrawCompactStatusDesignV57();
        }

        void DrawCompactControlsV5()
        {
            DrawCompactControlsDesignV57();
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
            DrawCompactKeyboardKeyV51(new Rect(330, 9, 42, 24), "PgUp");
            DrawCompactKeyboardKeyV51(new Rect(380, 9, 42, 24), "PgDn");
            DrawCompactKeyboardRowV51(40, 10, new[] { "1", "2", "3", "4", "", "", "", "", "", "" });
            DrawCompactKeyboardKeyV51(new Rect(330, 40, 92, 26), "Back");
            DrawCompactKeyboardKeyV51(new Rect(10, 70, 42, 26), "Tab");
            DrawCompactKeyboardRowV51(70, 58, new[] { "Q", "W", "E", "R", "T", "", "", "", "", "" });
            DrawCompactKeyboardKeyV51(new Rect(382, 70, 40, 56), "Enter");
            DrawCompactKeyV51(new Rect(10, 100, 42, 26), "", 0);
            DrawCompactKeyboardRowV51(100, 58, new[] { "A", "S", "D", "F", "", "H", "J", "", "" });
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
            if (KeyboardHelpVisibleV56) return KeyboardHelpKeyFamilyV56(key);
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
            CollectKeyboardHelpCapV57(rect,key,family);
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
            DrawCompactRequirementsDesignV57();
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
