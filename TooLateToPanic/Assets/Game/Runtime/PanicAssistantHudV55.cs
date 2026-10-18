using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>A quiet notebook companion drawn only by the existing main HUD canvas.</summary>
    public sealed partial class PanicHomeHud
    {
        const float AssistantRightV55 = 1576f;
        const float AssistantBottomV55 = 824f;
        const float AssistantWidthV55 = 330f;
        GUIStyle assistantTaskStyleV55, assistantHintStyleV55;
        Rect assistantCardBoundsV55 = new Rect(1246f, 752f, 330f, 72f);
        int assistantDrawFrameV55 = -1;
        int assistantRepaintFrameV55 = -1;

        public Rect AssistantCardBoundsV55 { get { return new Rect(1246f, 752f, 330f, 72f); } }
        public Rect AssistantRenderedBoundsV55 { get { return assistantCardBoundsV55; } }
        public bool AssistantCardVisibleV55 { get { return assistantDrawFrameV55 == Time.frameCount; } }
        public int AssistantHudLastDrawFrameV55 { get { return assistantDrawFrameV55; } }
        public int AssistantHudDrawFramesV55 { get; private set; }
        public bool AssistantHudVisibleV55
        {
            get
            {
                if (!World || World.Model == null || !World.AssistantEnabledV55 || !World.GuidanceVisibleV54 ||
                    _paused || _developerMenuOpen || _helpVisible || DeveloperInputBlockedThisFrame ||
                    FreeplayOnboardingInputBlockedV414 || World.NotebookBlocksInput || World.CinematicActive || World.LossAnnounced ||
                    World.WorkActive || World.InventoryOpen || World.IsMoving || World.BagCameraTransitionActive || SimulationSelectorOpen)
                    return false;
                var assistant = World.AssistantV55;
                return assistant != null && assistant.Available;
            }
        }
        public string AssistantRenderedTaskV55 { get; private set; }
        public string AssistantRenderedHintV55 { get; private set; }
        public string AssistantRenderedTextV55 { get; private set; }
        public float AssistantTaskTextHeightV55 { get; private set; }
        public float AssistantHintTextHeightV55 { get; private set; }

        // Call after the normal playing HUD and in the final-bag branch. Main OnGUI owns
        // matrix, fonts, modal precedence and blackout; there is no second GUI owner here.
        public void DrawAssistantHudV55()
        {
            if (!AssistantHudVisibleV55) return;
            var assistant = World.AssistantV55;
            EnsureAssistantStylesV55();

            string task = CleanAssistantTextV55(string.IsNullOrWhiteSpace(assistant.Task) ? assistant.Title : assistant.Task);
            string hint = CleanAssistantTextV55(assistant.Hint);
            string progress = assistant.StepCount > 0 ? Mathf.Clamp(assistant.CurrentStep, 1, assistant.StepCount) + "/" + assistant.StepCount : "";
            AssistantRenderedTaskV55 = task;
            AssistantRenderedHintV55 = hint;
            AssistantRenderedTextV55 = "DEFTER" + (progress.Length > 0 ? " · " + progress : "") + "\n" + task + (hint.Length > 0 ? "\n" + hint : "");

            Rect card;
            {
                const float textWidth = AssistantWidthV55 - 28f;
                assistantTaskStyleV55.fontSize = 22;
                // Preserve full text. Short production copy stays on one line; a longer
                // objective wraps and the card grows upward, never over the J launcher.
                while (assistantTaskStyleV55.fontSize > 19 && assistantTaskStyleV55.CalcHeight(new GUIContent(task), textWidth) > 25f)
                    assistantTaskStyleV55.fontSize--;
                AssistantTaskTextHeightV55 = Mathf.Max(24f, assistantTaskStyleV55.CalcHeight(new GUIContent(task), textWidth));
                AssistantHintTextHeightV55 = hint.Length > 0 ? Mathf.Max(18f, assistantHintStyleV55.CalcHeight(new GUIContent(hint), textWidth)) : 0f;
                float height = 6f + 16f + 1f + AssistantTaskTextHeightV55 + (hint.Length > 0 ? 1f + AssistantHintTextHeightV55 : 0f) + 6f;
                card = new Rect(AssistantRightV55 - AssistantWidthV55, AssistantBottomV55 - Mathf.Max(72f, height), AssistantWidthV55, Mathf.Max(72f, height));
            }

            // Existing loot/tutorial/action cards have priority. A short notebook badge
            // remains available in their presence instead of covering their controls.
            if (AssistantOverlapsHudV55(card))
            {
                card = new Rect(1432f, 788f, 144f, 36f);
                if (AssistantOverlapsHudV55(card)) return;
            }
            assistantCardBoundsV55 = card;
            assistantDrawFrameV55 = Time.frameCount;
            if (Event.current != null && Event.current.type == EventType.Repaint && assistantRepaintFrameV55 != Time.frameCount)
            {
                assistantRepaintFrameV55 = Time.frameCount;
                AssistantHudDrawFramesV55++;
            }
            bool compact = card.width < 200f;
            if (compact)
            {
                AssistantRenderedTaskV55 = AssistantRenderedHintV55 = "";
                AssistantRenderedTextV55 = "DEFTER" + (progress.Length > 0 ? " · " + progress : "");
            }
            Frame(card, Night, Line, 1f);
            Region(card);
            Fill(new Rect(card.x + 1f, card.y + 10f, 2f, card.height - 20f), new Color(Sea.r, Sea.g, Sea.b, .42f));
            DrawAssistantBookV55(new Rect(card.x + 14f, card.y + (compact ? 10f : 7f), 15f, 14f));
            Label(new Rect(card.x + 37f, card.y + (compact ? 8f : 5f), compact ? card.width - 45f : 110f, 20f),
                "DEFTER" + (compact && progress.Length > 0 ? " · " + progress : ""), ColorText(_tiny, SoftText));
            if (!compact)
            {
                if (progress.Length > 0)
                    Label(new Rect(card.xMax - 52f, card.y + 5f, 38f, 20f), progress, ColorText(_caps, Muted));
                float y = card.y + 23f;
                Label(new Rect(card.x + 14f, y, card.width - 28f, AssistantTaskTextHeightV55), task, assistantTaskStyleV55);
                if (hint.Length > 0)
                    Label(new Rect(card.x + 14f, y + AssistantTaskTextHeightV55 + 1f, card.width - 28f, AssistantHintTextHeightV55), hint, assistantHintStyleV55);
            }
            if (World.Notebook && World.Notebook.CanOpen && MousePressed(card))
            {
                if (World.Notebook.RequestOpen()) World.PlayUiSound();
            }
        }

        void EnsureAssistantStylesV55()
        {
            if (assistantTaskStyleV55 != null) return;
            assistantTaskStyleV55 = new GUIStyle(_body) { wordWrap = true, alignment = TextAnchor.UpperLeft };
            assistantHintStyleV55 = new GUIStyle(_tiny) { wordWrap = true, alignment = TextAnchor.UpperLeft };
            assistantTaskStyleV55.normal.textColor = MainText;
            assistantHintStyleV55.normal.textColor = Muted;
        }

        bool AssistantOverlapsHudV55(Rect card)
        {
            for (int i = 0; i < _topLevelRegions.Count; i++)
                if (_topLevelRegions[i].Overlaps(card)) return true;
            return false;
        }

        static string CleanAssistantTextV55(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "" : text.Replace('\r', ' ').Replace('\n', ' ').Trim();
        }

        void DrawAssistantBookV55(Rect book)
        {
            Color ink = new Color(Sea.r, Sea.g, Sea.b, .78f);
            DrawLine(new Vector2(book.x, book.y + 1f), new Vector2(book.center.x, book.y + 3f), ink, 1f);
            DrawLine(new Vector2(book.center.x, book.y + 3f), new Vector2(book.xMax, book.y + 1f), ink, 1f);
            DrawLine(new Vector2(book.x, book.y + 1f), new Vector2(book.x, book.yMax - 1f), ink, 1f);
            DrawLine(new Vector2(book.xMax, book.y + 1f), new Vector2(book.xMax, book.yMax - 1f), ink, 1f);
            DrawLine(new Vector2(book.x, book.yMax - 1f), new Vector2(book.center.x, book.yMax + 1f), ink, 1f);
            DrawLine(new Vector2(book.center.x, book.yMax + 1f), new Vector2(book.xMax, book.yMax - 1f), ink, 1f);
            DrawLine(new Vector2(book.center.x, book.y + 3f), new Vector2(book.center.x, book.yMax + 1f), ink, 1f);
        }
    }
}
