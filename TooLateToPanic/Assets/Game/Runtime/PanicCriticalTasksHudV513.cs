using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        GUIStyle criticalTitleV513, criticalRowV513;
        bool guidanceExpandedV513;
        int guidanceToggleDrawFrameV513 = -1;
        public bool GuidanceExpandedV513 { get { return guidanceExpandedV513; } }
        public bool GuidanceToggleEligibleV513 { get { return CriticalCardEligibleV513 || FirstWaveCardEligibleV510; } }
        public bool GuidanceToggleVisibleV513 { get { return GuidanceToggleEligibleV513 && guidanceToggleDrawFrameV513 == Time.frameCount; } }
        public Rect GuidanceToggleBoundsV513 { get; private set; }
        public void SetGuidanceExpandedV513(bool expanded)
        {
            guidanceExpandedV513 = expanded;
            if (expanded) return;
            CriticalCardBoundsV513 = new Rect();
            CriticalRenderedTextV513 = "";
            FirstWaveCardBoundsV510 = new Rect();
            FirstWaveRenderedTextV510 = "";
            firstWaveDrawFrameV510 = -1;
        }
        void DrawGuidanceToggleV513()
        {
            GuidanceToggleBoundsV513 = new Rect();
            if (!GuidanceToggleEligibleV513) return;
            Rect box = new Rect(24f, 116f, 300f, 38f);
            GuidanceToggleBoundsV513 = box;
            Region(box);
            Button(box, guidanceExpandedV513 ? "GÖREVLER VE REHBER  −" : "GÖREVLER VE REHBER  +",
                false, Brass, () => SetGuidanceExpandedV513(!guidanceExpandedV513));
            guidanceToggleDrawFrameV513 = Time.frameCount;
        }
        float GuidanceCardsTopV513 { get { return 164f; } }
        public Rect CriticalCardBoundsV513 { get; private set; }
        public string CriticalRenderedTextV513 { get; private set; } = "";
        public bool CriticalCardEligibleV513
        {
            get { return World && World.isActiveAndEnabled && World.Model != null && !World.Model.IsTutorial && !World.Model.IsSimulation &&
                !World.Model.IsJuryDemo && !World.JuryActive && (World.Model.Phase == PanicPhase.Preparing || World.Model.Phase == PanicPhase.Disaster) &&
                !_paused && !_developerMenuOpen && !DeveloperInputBlockedThisFrame && !FreeplayOnboardingInputBlockedV414 &&
                !World.NotebookBlocksInput && !World.CinematicActive && !World.IntroStoryActiveV414 && !World.LossAnnounced &&
                !World.InventoryOpen && !World.WorkActive && !World.IsMoving && !World.BagCameraOwnsView && !World.BagCameraTransitionActive && !SimulationSelectorOpen &&
                !WaveLedgerInputBlockedV56 && !KeyboardHelpBlockingV56; }
        }
        void EnsureCriticalStylesV513()
        {
            if (criticalTitleV513 != null) return;
            EnsureStyles();
            criticalTitleV513 = new GUIStyle(_body) { fontSize = 19, fontStyle = FontStyle.Bold, wordWrap = true };
            criticalRowV513 = new GUIStyle(_body) { fontSize = 18, wordWrap = true };
        }
        float CriticalCardHeightV513()
        {
            EnsureCriticalStylesV513(); float height = 47f;
            foreach (var row in World.Model.CriticalTasksV513)
                height += Mathf.Max(23f, criticalRowV513.CalcHeight(new GUIContent(row.Label), 240f)) + 4f;
            return height;
        }
        // No drawing-order dependency: root uses this for both the mandatory card and work strip.
        public float CriticalRecommendedNextYV513
        { get { return GuidanceExpandedV513 && CriticalCardEligibleV513 ? GuidanceCardsTopV513 + CriticalCardHeightV513() + 10f : GuidanceCardsTopV513; } }
        public void DrawCriticalTasksV513()
        {
            CriticalRenderedTextV513 = "";
            if (!GuidanceExpandedV513 || !CriticalCardEligibleV513) { CriticalCardBoundsV513 = new Rect(); return; }
            EnsureCriticalStylesV513();
            Rect box = new Rect(24f, GuidanceCardsTopV513, 300f, CriticalCardHeightV513()); CriticalCardBoundsV513 = box;
            Frame(box, Night, Ember, 1f); Region(box);
            Label(new Rect(38, box.y + 9f, 272, 25), "KRİTİK GÖREVLER", ColorText(criticalTitleV513, Brass));
            float y = box.y + 36f;
            foreach (var row in World.Model.CriticalTasksV513)
            {
                Color ink = row.Complete ? Sea : row.Resolved ? Ember : MainText;
                string marker = row.Complete ? "✓" : row.Resolved ? "!" : "·";
                float h = Mathf.Max(23f, criticalRowV513.CalcHeight(new GUIContent(row.Label), 240f));
                Label(new Rect(38, y, 18, h), marker, ColorText(criticalRowV513, ink));
                Label(new Rect(60, y, 240, h), row.Label, ColorText(criticalRowV513, ink));
                y += h + 4f;
                CriticalRenderedTextV513 += marker + " " + row.Label + "\n";
            }
        }
    }
}
