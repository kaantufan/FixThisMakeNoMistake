using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        GUIStyle criticalTitleV513, criticalRowV513, criticalSmallV513;
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
            criticalTitleV513 = new GUIStyle(_body) { fontSize = 21, fontStyle = FontStyle.Bold, wordWrap = true };
            criticalRowV513 = new GUIStyle(_body) { fontSize = 18, wordWrap = true };
            criticalSmallV513 = new GUIStyle(_small) { fontSize = 16, wordWrap = true };
        }
        float CriticalCardHeightV513()
        {
            EnsureCriticalStylesV513(); float height = 54f;
            foreach (var row in World.Model.CriticalTasksV513)
                height += Mathf.Max(26f, Mathf.Max(criticalRowV513.CalcHeight(new GUIContent(row.Label), 204f),
                    criticalSmallV513.CalcHeight(new GUIContent(CriticalConsequenceV513(row)), 120f))) + 5f;
            return height + 38f;
        }
        string CriticalConsequenceV513(PanicCriticalTaskV513 row)
        {
            if (World.Model.DeveloperWaveSkipProtectionV51 && row.PenaltyPoints > 0) return "Ek ceza kapalı";
            if (row.PenaltyPoints > 0) return "Ceza: en çok +" + row.PenaltyPoints.ToString("0");
            return World.Model.DeveloperNoLoss || World.Model.DeveloperWaveSkipProtectionV51 ? "Eksik: ödülsüz" : "Yoksa koşu biter";
        }
        // No drawing-order dependency: root uses this for both the mandatory card and work strip.
        public float CriticalRecommendedNextYV513
        { get { return CriticalCardEligibleV513 ? 116f + CriticalCardHeightV513() + 10f : 116f; } }
        public void DrawCriticalTasksV513()
        {
            CriticalRenderedTextV513 = "";
            if (!CriticalCardEligibleV513) { CriticalCardBoundsV513 = new Rect(); return; }
            EnsureCriticalStylesV513();
            var model = World.Model;
            Rect box = new Rect(24f, 116f, 390f, CriticalCardHeightV513()); CriticalCardBoundsV513 = box;
            Frame(box, Night, Ember, 1f); Region(box);
            Label(new Rect(40, 126, 358, 29), "KRİTİK GÖREVLER", ColorText(criticalTitleV513, Brass));
            float y = 159f;
            foreach (var row in model.CriticalTasksV513)
            {
                Color ink = row.Complete ? Sea : row.Resolved ? Ember : MainText;
                string marker = row.Complete ? "✓" : row.Resolved ? "!" : "·";
                string consequence = CriticalConsequenceV513(row);
                float h = Mathf.Max(26, Mathf.Max(criticalRowV513.CalcHeight(new GUIContent(row.Label), 204),
                    criticalSmallV513.CalcHeight(new GUIContent(consequence), 120)));
                Label(new Rect(40, y, 22, h), marker, ColorText(criticalRowV513, ink));
                Label(new Rect(68, y, 204, h), row.Label, ColorText(criticalRowV513, ink));
                Label(new Rect(278, y + 2, 120, h), consequence, ColorText(criticalSmallV513, row.Complete ? SoftText : Brass));
                y += h + 5;
                CriticalRenderedTextV513 += marker + " " + row.Label + " · " + row.Consequence + "\n";
            }
            string footer = model.DeveloperWaveSkipProtectionV51 ? "F3 koruması: kritik ek hasar uygulanmaz." :
                "Kritik ek hasar toplamı en çok +" + PanicModel.CriticalPenaltyCapV513.ToString("0") + "; ortak hasar sınırı %100.";
            Label(new Rect(40, y, 358, 40), footer, ColorText(criticalSmallV513, SoftText));
        }
    }
}
