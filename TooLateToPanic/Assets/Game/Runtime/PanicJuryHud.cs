using UnityEngine;

namespace TooLateToPanic
{
    // Uses the delivered V4.6 canvas, fonts, colors, buttons and pointer regions.
    public sealed partial class PanicHomeHud
    {
        public Rect JuryObjectiveBounds { get { return new Rect(24, 176, 430, 240); } }

        public static string JuryStagePrompt(PanicJuryStage stage, PanicJuryShelter shelter)
        {
            if (stage == PanicJuryStage.Earthquake) return "SARSINTI SÜRÜYOR";
            if (stage == PanicJuryStage.EarthquakeResult) return "SONUCU İNCELE";
            if (stage == PanicJuryStage.Inspection) return "EVİ İNCELE";
            if (stage == PanicJuryStage.Onboarding) return "İLK ADIMLAR";
            return shelter != PanicJuryShelter.None ? "İŞARETLİ NOKTADA · E" : "ÖNLEMİNİ SEÇ";
        }

        void StartJuryRun()
        {
            if (_paused) SetPaused(false);
            _helpVisible = false; _inspectionStep = 0;
            World.StartJuryDemo();
        }

        void DrawJuryStatusStrip()
        {
            var snapshot = World.JurySnapshot;
            if (snapshot == null) return;
            Frame(WaveCardBounds, Night, Line, 1f); Region(WaveCardBounds);
            DrawHazardStripe(new Rect(42, 35, 65, 5), Brass);
            Label(new Rect(42, 48, 388, 32), "HAZIRLIK ZAMANI", _heading);
            Label(new Rect(42, 85, 390, 22), "KÜÇÜK ÖNLEM · GÖRÜNÜR SONUÇ", ColorText(_tiny, Brass));

            Frame(TimerCardBounds, Night, Line, 1f); Region(TimerCardBounds);
            Label(new Rect(630, 23, 190, 65), FormatTime(snapshot.StageRemainingSeconds), _timer);
            Label(new Rect(837, 35, 138, 25), "BU BÖLÜM", ColorText(_tiny, Sea));
            Label(new Rect(837, 64, 138, 22), FormatTime(snapshot.ElapsedSeconds) + " GEÇTİ", _tiny);
            DrawBar(new Rect(632, 105, 336, 5), snapshot.ElapsedSeconds / PanicJuryDemo.ContentLimitSeconds, Brass);

            DrawResourceCard();
        }

        void DrawJuryPanel()
        {
            var snapshot = World.JurySnapshot;
            if (snapshot == null) return;
            bool result = snapshot.Stage == PanicJuryStage.Result || snapshot.Stage == PanicJuryStage.Complete;
            if (result) { DrawJuryResult(snapshot); return; }

            Rect panel = JuryObjectiveBounds;
            Frame(panel, Night, Line, 1f); Region(panel);
            FittedLabel(new Rect(42, 188, 390, 40), snapshot.Heading, _heading, MainText, 22);
            Label(new Rect(42, 238, 386, 98), snapshot.Hint, _wrap);
            Label(new Rect(42, 343, 386, 26), JuryStagePrompt(snapshot.Stage, snapshot.ShelterKind), ColorText(_tiny, Brass));
            if (snapshot.Stage == PanicJuryStage.Earthquake)
                Button(new Rect(42, 373, 386, 34), "SAHNEYİ ATLA · ENTER", false, Sea, World.SkipResidenceCinematic);
            else if (snapshot.CanAdvance)
                Button(new Rect(42, 373, 386, 34), "DEVAM · ENTER", false, Sea, World.AdvanceJuryDemo);
            else if (snapshot.CanSkipSegment)
                Button(new Rect(42, 373, 386, 34), "EKSİKLERİMLE DEVAM ET", false, Line, World.SkipJurySegment);
            else Label(new Rect(42, 375, 386, 27), "Önlemini seç. Sonucunu gör.", _small);

            if (snapshot.AllowsMovement) DrawContext();
            if (RequirementsCardVisible) DrawRequirements();
            if (World.NotificationVisible) DrawToast(World.ShortNotice);
            if ((snapshot.CanAdvance || snapshot.Stage == PanicJuryStage.Earthquake) && !DeveloperInputBlockedThisFrame && World.CancelFrame != Time.frameCount && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            { if (snapshot.Stage == PanicJuryStage.Earthquake) World.SkipResidenceCinematic(); else World.AdvanceJuryDemo(); Event.current.Use(); }
        }

        void DrawJuryResult(PanicJurySnapshot snapshot)
        {
            Rect panel = new Rect(278, 166, 1044, 606);
            Fill(LogicalCanvasBounds, new Color(.055f, .05f, .045f, .5f));
            Frame(panel, Night, Line, 1f); Region(panel);
            Label(new Rect(312, 188, 970, 52), "HAZIRLIK FARK YARATIR", _title);
            Label(new Rect(312, 244, 970, 42), snapshot.Summary, _wrap);
            int count = Mathf.Min(10, snapshot.Goals.Count);
            for (int i = 0; i < count; i++)
            {
                var goal = snapshot.Goals[i];
                float x = i < 5 ? 314 : 812, y = 300 + i % 5 * 58;
                string state = goal.State == PanicJuryGoalState.Complete ? "TAMAM" : goal.State == PanicJuryGoalState.Skipped ? "ATLANDI" : "EKSİK";
                Color accent = goal.State == PanicJuryGoalState.Complete ? Sea : Brass;
                Label(new Rect(x, y, 360, 27), goal.Label, _small);
                Label(new Rect(x, y + 28, 360, 21), state, ColorText(_tiny, accent));
            }
            Label(new Rect(314, 610, 960, 30), "Yeniden oyna, başka bir önlem seç ve evindeki farkı gör.", _small);
            Button(new Rect(314, 669, 464, 58), "TEKRAR DENE", true, Brass, StartJuryRun);
            Button(new Rect(798, 669, 488, 58), "ANA MENÜ", false, Sea, ReturnToMenu);
        }

        void DrawPresentationSubtitle()
        {
            DrawDialogueSubtitleV59();
        }
    }
}
