using UnityEngine;

namespace TooLateToPanic
{
    // Drawn by the one existing HUD canvas. No second Update/OnGUI or input owner is introduced.
    public sealed partial class PanicHomeHud
    {
        GUIStyle firstWaveTitleV510, firstWaveBodyV510, firstWaveActionV510, firstWaveSmallV510;
        int firstWaveDrawFrameV510 = -1;
        public Rect FirstWaveCardBoundsV510 { get; private set; }
        public string FirstWaveRenderedTextV510 { get; private set; } = "";
        public float FirstWaveContentHeightV510 { get; private set; }
        public bool FirstWaveCardVisibleV510 { get { return firstWaveDrawFrameV510 == Time.frameCount; } }
        public bool FirstWaveCardEligibleV510
        {
            get { return World && World.isActiveAndEnabled && World.FirstWaveOnboardingRequiredV510 && !_paused && !_developerMenuOpen &&
                !DeveloperInputBlockedThisFrame && !FreeplayOnboardingInputBlockedV414 && !World.NotebookBlocksInput &&
                !World.CinematicActive && !World.IntroStoryActiveV414 && !World.LossAnnounced &&
                !World.InventoryOpen && !World.BagCameraTransitionActive && !SimulationSelectorOpen &&
                !WaveLedgerInputBlockedV56 && !KeyboardHelpBlockingV56; }
        }

        void EnsureFirstWaveStylesV510()
        {
            if (firstWaveTitleV510 != null) return;
            firstWaveTitleV510 = new GUIStyle(_body) { fontSize = 22, fontStyle = FontStyle.Bold, wordWrap = true, alignment = TextAnchor.UpperLeft };
            firstWaveTitleV510.normal.textColor = Brass;
            firstWaveBodyV510 = new GUIStyle(_body) { fontSize = 22, wordWrap = true, alignment = TextAnchor.UpperLeft };
            firstWaveBodyV510.normal.textColor = MainText;
            firstWaveActionV510 = new GUIStyle(firstWaveBodyV510) { fontStyle = FontStyle.Bold };
            firstWaveActionV510.normal.textColor = Sea;
            firstWaveSmallV510 = new GUIStyle(firstWaveBodyV510) { fontSize = 19 };
            firstWaveSmallV510.normal.textColor = SoftText;
        }

        public void DrawFirstWaveOnboardingV510()
        {
            if (!FirstWaveCardEligibleV510) return;
            EnsureFirstWaveStylesV510();
            var card = World.FirstWaveOnboardingV510;
            // Work has its real large close-up and existing key hints. Keep the mandatory step visible
            // in the 40px gap above it, without covering the work camera or screw/handle controls.
            if (World.WorkActive)
            {
                var work = World.Expedition.Work;
                Rect strip = new Rect(24f, 112f, 390f, 40f);
                FirstWaveCardBoundsV510 = strip;
                bool anchor = work.Action == "anchorwall" || work.Action == "anchorfloor";
                FirstWaveRenderedTextV510 = work.IsUtility ? "3 · TESİSAT · İŞİ TAMAMLA" : anchor ?
                    "2 · SABİTLEME · " + work.CompletedSteps + "/" + work.TotalSteps + " VİDA" : "ÖNLEM · " + work.Label;
                FirstWaveContentHeightV510 = 28f;
                Frame(strip, Night, Brass, 1f); Region(strip);
                FittedLabel(new Rect(36f, 118f, 366f, 28f), FirstWaveRenderedTextV510, firstWaveTitleV510, Brass, 18);
                firstWaveDrawFrameV510 = Time.frameCount; return;
            }
            const float x = 24f, y = 116f, width = 390f, inner = 358f;
            string body = card.Body;
            string place = card.Location;
            string status = "İLK DALGA · DEPREM\n" +
                "Malzeme " + (card.GatherComplete ? "✓" : "·") + "  Sabitleme " + (card.AnchorComplete ? "✓" : "·") +
                "  Tesisat " + World.Model.UtilitySafety.CompletedCount + "/3\n" +
                "Su " + (card.WaterComplete ? "1/1" : "0/1") + "  Gıda " + (card.FoodComplete ? "1/1" : "0/1") +
                "  Sığınak " + (card.ShelterComplete ? "✓" : "·");
            float titleH = firstWaveTitleV510.CalcHeight(new GUIContent(card.Title), inner);
            float bodyH = firstWaveBodyV510.CalcHeight(new GUIContent(body), inner);
            float placeH = place.Length > 0 ? firstWaveSmallV510.CalcHeight(new GUIContent(place), inner) + 7f : 0f;
            float reasonH = card.Reason.Length > 0 ? firstWaveSmallV510.CalcHeight(new GUIContent(card.Reason), inner) + 7f : 0f;
            float actionH = firstWaveActionV510.CalcHeight(new GUIContent(card.Action), inner);
            float statusH = firstWaveSmallV510.CalcHeight(new GUIContent(status), inner);
            float height = 34f + titleH + bodyH + placeH + reasonH + actionH + statusH + 26f;
            Rect box = new Rect(x, y, width, height);
            FirstWaveCardBoundsV510 = box; FirstWaveContentHeightV510 = height - 24f;
            Frame(box, Night, Brass, 2f); Region(box);
            Fill(new Rect(x + 1f, y + 12f, 3f, height - 24f), Brass);
            float line = y + 12f;
            Label(new Rect(x + 16f, line, inner, titleH), card.Title, firstWaveTitleV510); line += titleH + 10f;
            Label(new Rect(x + 16f, line, inner, bodyH), body, firstWaveBodyV510); line += bodyH + 8f;
            if (placeH > 0f) { Label(new Rect(x + 16f, line, inner, placeH - 7f), place, firstWaveSmallV510); line += placeH; }
            if (reasonH > 0f) { Label(new Rect(x + 16f, line, inner, reasonH - 7f), card.Reason, firstWaveSmallV510); line += reasonH; }
            Label(new Rect(x + 16f, line, inner, actionH), card.Action, firstWaveActionV510); line += actionH + 9f;
            Fill(new Rect(x + 16f, line, inner, 1f), Line); line += 8f;
            Label(new Rect(x + 16f, line, inner, statusH), status, firstWaveSmallV510);
            FirstWaveRenderedTextV510 = card.Title + "\n" + body + "\n" + place + "\n" + card.Reason + "\n" + card.Action + "\n" + status;
            firstWaveDrawFrameV510 = Time.frameCount;
        }
    }
}
