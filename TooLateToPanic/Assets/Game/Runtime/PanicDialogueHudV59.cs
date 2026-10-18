using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        GUIStyle dialogueBodyV59;
        int dialogueDrawFrameV59 = -1;
        public Rect DialogueSubtitleBoundsV59 { get { return PanicDialogueSourcePolicyV59.CaptionBounds; } }
        public bool DialogueSubtitleVisibleV59 { get { return dialogueDrawFrameV59 == Time.frameCount; } }
        public string DialogueRenderedTextV59 { get; private set; } = "";
        public int DialogueRenderedLinesV59 { get; private set; }
        public float DialogueFontPixelsV59 { get { return 24f * _scale; } }
        public bool DialogueHiddenModalV59
        { get { return _paused || _developerMenuOpen || _helpVisible || FreeplayOnboardingInputBlockedV414 ||
            WaveLedgerVisibleV56 || KeyboardHelpVisibleV56 || AudioSettingsOpenV56 || SimulationSelectorOpen; } }

        void DrawDialogueSubtitleV59()
        {
            if (!World || World.Model == null || string.IsNullOrEmpty(World.PresentationSubtitle) || World.DialogueSuspendedV59 ||
                World.IntroStoryActiveV414 || _paused || _developerMenuOpen || _helpVisible || WaveLedgerVisibleV56 ||
                KeyboardHelpVisibleV56 || FreeplayOnboardingInputBlockedV414 || SimulationSelectorOpen ||
                World.Model.Phase == PanicPhase.Title || World.Model.Phase == PanicPhase.Success || World.Model.Phase == PanicPhase.GameOver ||
                SecondaryAlertVisibleV55 || (!string.IsNullOrEmpty(_developerNotice) && Time.unscaledTime < _developerNoticeUntil)) return;
            EnsureStyles();
            if (dialogueBodyV59 == null)
                dialogueBodyV59 = MakeStyle(_bodyFont, 24, FontStyle.Normal, MainText, TextAnchor.MiddleLeft);
            Rect box = DialogueSubtitleBoundsV59;
            // Existing authored interactive cards always win. This panel never registers a pointer region.
            for (int i = 0; i < _topLevelRegions.Count; i++)
                if (_topLevelRegions[i].Overlaps(box)) return;
            string text = FitDialogueTwoLinesV59(PanicDialogueSessionV59.Clean(World.PresentationSubtitle), box.width - 36f);
            if (text.Length == 0) return;
            DialogueRenderedTextV59 = text;
            DialogueRenderedLinesV59 = text.IndexOf('\n') < 0 ? 1 : 2;
            Frame(box, Night, Line, 1f);
            Fill(new Rect(box.x + 1f, box.y + 15f, 3f, box.height - 30f), Brass);
            dialogueBodyV59.wordWrap = false;
            Label(new Rect(box.x + 18f, box.y + 12f, box.width - 36f, box.height - 24f), text, dialogueBodyV59);
            dialogueDrawFrameV59 = Time.frameCount;
        }
        string FitDialogueTwoLinesV59(string text, float width)
        {
            if (dialogueBodyV59.CalcSize(new GUIContent(text)).x <= width) return text;
            int split = 0;
            for (int i = 0; i < text.Length; i++)
                if (text[i] == ' ' && dialogueBodyV59.CalcSize(new GUIContent(text.Substring(0, i))).x <= width) split = i;
            if (split <= 0) split = Mathf.Min(35, text.Length);
            string first = text.Substring(0, split).Trim(), second = text.Substring(split).Trim();
            if (dialogueBodyV59.CalcSize(new GUIContent(second)).x <= width) return first + "\n" + second;
            while (second.Length > 1 && dialogueBodyV59.CalcSize(new GUIContent(second + "…")).x > width)
            {
                int end = second.LastIndexOf(' ');
                second = end > 0 ? second.Substring(0, end) : second.Substring(0, second.Length - 1);
            }
            return first + "\n" + second.TrimEnd(' ', '…') + "…";
        }
    }
}
