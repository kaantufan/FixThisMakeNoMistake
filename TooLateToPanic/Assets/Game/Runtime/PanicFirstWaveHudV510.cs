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
        public bool FirstWaveCardVisibleV510 { get { return GuidanceExpandedV513 && firstWaveDrawFrameV510 == Time.frameCount; } }
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
            firstWaveTitleV510 = new GUIStyle(_body) { fontSize = 19, fontStyle = FontStyle.Bold, wordWrap = true, alignment = TextAnchor.UpperLeft };
            firstWaveTitleV510.normal.textColor = Brass;
            firstWaveBodyV510 = new GUIStyle(_body) { fontSize = 17, wordWrap = true, alignment = TextAnchor.UpperLeft };
            firstWaveBodyV510.normal.textColor = MainText;
            firstWaveActionV510 = new GUIStyle(firstWaveBodyV510) { fontStyle = FontStyle.Bold };
            firstWaveActionV510.normal.textColor = Sea;
            firstWaveSmallV510 = new GUIStyle(firstWaveBodyV510) { fontSize = 16 };
            firstWaveSmallV510.normal.textColor = SoftText;
        }

        public void DrawFirstWaveOnboardingV510()
        {
            if (!GuidanceExpandedV513 || !FirstWaveCardEligibleV510) return;
            EnsureFirstWaveStylesV510();
            var card = World.FirstWaveOnboardingV510;
            // Work has its real large close-up and existing key hints. Keep the mandatory step visible
            // in the 40px gap above it, without covering the work camera or screw/handle controls.
            if (World.WorkActive)
            {
                var work = World.Expedition.Work;
                float workY = CriticalRecommendedNextYV513 - 4f;
                Rect strip = new Rect(24f, workY, 300f, 40f);
                FirstWaveCardBoundsV510 = strip;
                bool anchor = work.Action == "anchorwall" || work.Action == "anchorfloor";
                bool floorUpgrade = work.Action == "upgrade" && World.Expedition.ActiveFloorWallUpgradeV414 != null;
                FirstWaveRenderedTextV510 = floorUpgrade ?
                    World.Expedition.ActiveFloorWallUpgradeV414.IncludesRoof ? "ÜST KAT + ÇATI · GÜÇLENDİR" : "KAT DUVARLARI · GÜÇLENDİR" :
                    work.IsUtility ? "3 · TESİSAT · İŞİ TAMAMLA" : anchor ?
                    "2 · SABİTLEME · " + work.CompletedSteps + "/" + work.TotalSteps + " VİDA" : "ÖNLEM · " + work.Label;
                FirstWaveContentHeightV510 = 28f;
                Frame(strip, Night, Brass, 1f); Region(strip);
                FittedLabel(new Rect(36f, workY + 6f, 276f, 28f), FirstWaveRenderedTextV510, firstWaveTitleV510, Brass, 16);
                firstWaveDrawFrameV510 = Time.frameCount; return;
            }
            const float x = 24f, width = 300f, inner = 272f;
            float y = CriticalRecommendedNextYV513;
            string title, body, action;
            CompactFirstWaveCopyV510(card, out title, out body, out action);
            string place = (card.Location ?? "").Split('\n')[0];
            float titleH = firstWaveTitleV510.CalcHeight(new GUIContent(title), inner);
            float bodyH = firstWaveBodyV510.CalcHeight(new GUIContent(body), inner);
            float placeH = place.Length > 0 ? firstWaveSmallV510.CalcHeight(new GUIContent(place), inner) + 6f : 0f;
            float actionH = firstWaveActionV510.CalcHeight(new GUIContent(action), inner);
            float height = 36f + titleH + bodyH + placeH + actionH;
            Rect box = new Rect(x, y, width, height);
            FirstWaveCardBoundsV510 = box; FirstWaveContentHeightV510 = height - 24f;
            Frame(box, Night, Brass, 1f); Region(box);
            float line = y + 12f;
            Label(new Rect(x + 14f, line, inner, titleH), title, firstWaveTitleV510); line += titleH + 6f;
            Label(new Rect(x + 14f, line, inner, bodyH), body, firstWaveBodyV510); line += bodyH + 6f;
            if (placeH > 0f) { Label(new Rect(x + 14f, line, inner, placeH - 6f), place, firstWaveSmallV510); line += placeH; }
            Label(new Rect(x + 14f, line, inner, actionH), action, firstWaveActionV510);
            FirstWaveRenderedTextV510 = title + "\n" + body + "\n" + place + "\n" + action;
            firstWaveDrawFrameV510 = Time.frameCount;
        }
        void CompactFirstWaveCopyV510(PanicFirstWaveSnapshotV510 card, out string title, out string body, out string action)
        {
            title = card.Title; action = card.Action;
            switch (card.Step)
            {
                case PanicFirstWaveStepV510.Materials:
                    title = "1 · MALZEME TOPLA"; body = "Parlayan dolaba yaklaş ve aç."; action = "E · Malzeme al"; break;
                case PanicFirstWaveStepV510.Anchor:
                    title = "2 · MOBİLYAYI SABİTLE"; body = "İki vidayı fareyle sırayla sık."; action = "1 · Sabitle"; break;
                case PanicFirstWaveStepV510.Utilities:
                    title = "3 · TESİSATI KAPAT"; body = "Vanaya veya şaltere yaklaş."; action = "E · Kapat"; break;
                case PanicFirstWaveStepV510.Water:
                    title = "4 · SU AL"; body = "İşareti suya getir ve al."; action = "E / F · 1 su al"; break;
                case PanicFirstWaveStepV510.Food:
                    title = "5 · GIDA AL"; body = "İşareti gıdaya getir ve al."; action = "E / F · 1 gıda al"; break;
                case PanicFirstWaveStepV510.Shelter:
                    title = "6 · SIĞINAĞA GİR"; body = World.Model.HasActiveDisasterV54(DisasterType.Flood) ? "Üst kattaki panik odasına git." : "Bodrumdaki sığınağa git."; action = "E · İçeri gir"; break;
                case PanicFirstWaveStepV510.Sheltered:
                    title = "SIĞINAKTASIN"; body = "Afetin geçmesini bekle."; action = "Giriş tamamlandı"; break;
                default:
                    title = "ÖNLEMLERİ GÖZDEN GEÇİR"; body = "Kalan zamanda evi güçlendir."; action = "J · Defteri aç"; break;
            }
        }
    }
}
