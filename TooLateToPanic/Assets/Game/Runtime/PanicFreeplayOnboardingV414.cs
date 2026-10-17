using UnityEngine;

namespace TooLateToPanic
{
    // Entry UI only: shared standard pause, real normal run, no persistent profile or developer option.
    public sealed partial class PanicHomeHud
    {
        static bool _freeplayOnboardingSeenV414;
        static int _freeplayOnboardingResolutionCountV414;
        bool _freeplayOnboardingOpenV414, _freeplayOnboardingPriorPauseV414;
        int _freeplayOnboardingTransitionFrameV414 = -1;
        GUIStyle _freeplayGuideBodyV414, _freeplayGuideHeadingV414, _freeplayGuideKeyV414;

        public bool FreeplayOnboardingOpenV414 { get { return _freeplayOnboardingOpenV414; } }
        public bool FreeplayOnboardingSeenV414 { get { return _freeplayOnboardingSeenV414; } }
        public int FreeplayOnboardingResolutionCountV414 { get { return _freeplayOnboardingResolutionCountV414; } }
        public bool FreeplayOnboardingInputBlockedV414
        { get { return _freeplayOnboardingOpenV414 || _freeplayOnboardingTransitionFrameV414 == Time.frameCount; } }
        public string PrimaryPlayLabelV414 { get { return TitleChoiceLabelV414(0); } }

        public static string TitleChoiceLabelV414(int choice)
        {
            switch (choice)
            {
                case 0: return "SERBEST OYNA";
                case 1: return "HAZIRLIK SENARYOSU · KISA AKIŞ";
                case 2: return "AFET SİMÜLASYONU";
                case 3: return "ÖĞRETİCİ";
                default: return "ÇIKIŞ";
            }
        }

        // Mouse and title Enter share this action map. Navigation retains the drawn 0/1/(2,3)/4 grid.
        public void ActivateSelectedTitleChoiceV414() { ActivateTitleChoiceV414(_menuChoice); }
        public void ActivateTitleChoiceV414(int choice)
        {
            if (!World || World.Model == null || World.Model.Phase != PanicPhase.Title ||
                FreeplayOnboardingInputBlockedV414 || DeveloperMenuOpen || DeveloperInputBlockedThisFrame) return;
            _menuChoice = Mathf.Clamp(choice, 0, 4);
            switch (_menuChoice)
            {
                case 0: StartRun(); break;
                case 1: StartJuryRun(); break;
                case 2: OpenSimulationSelector(); break;
                case 3: StartTutorial(); break;
                default: Application.Quit(); break;
            }
        }

        // Called by the real World.StartGame path after its intro has been initialized at age zero.
        // Synchronize FIRST: the old Title->Preparing synchronizer otherwise releases our pause next frame.
        public bool BeginFreeplayOnboardingV414()
        {
            if (!World || World.Model == null || _freeplayOnboardingSeenV414 || _freeplayOnboardingOpenV414 ||
                World.Model.Phase != PanicPhase.Preparing || World.Model.IsTutorial || World.Model.IsSimulation ||
                World.Model.IsJuryDemo || World.JuryActive) return false;
            SynchronizeHudPhase();
            _helpVisible = false;
            simulationSelectOpen = false;
            CancelInventoryDrag();
            _freeplayOnboardingPriorPauseV414 = _paused;
            _freeplayOnboardingOpenV414 = true;
            SetPaused(true);
            StampFreeplayOnboardingInputV414();
            return true;
        }

        // The same idempotent resolution is used by the visible button, Enter and Escape.
        public bool ConfirmFreeplayOnboardingV414()
        {
            if (!_freeplayOnboardingOpenV414 || _freeplayOnboardingTransitionFrameV414 == Time.frameCount) return false;
            _freeplayOnboardingOpenV414 = false;
            _freeplayOnboardingSeenV414 = true;
            _freeplayOnboardingResolutionCountV414++;
            _helpVisible = false;
            SetPaused(_freeplayOnboardingPriorPauseV414);
            StampFreeplayOnboardingInputV414();
            return true;
        }

        // Common run/title reset calls this. Abandoning the explanation never marks it seen.
        public void CancelFreeplayOnboardingV414()
        {
            if (!_freeplayOnboardingOpenV414) return;
            _freeplayOnboardingOpenV414 = false;
            _helpVisible = false;
            SetPaused(_freeplayOnboardingPriorPauseV414);
            StampFreeplayOnboardingInputV414();
        }

        void StampFreeplayOnboardingInputV414()
        {
            _freeplayOnboardingTransitionFrameV414 = Time.frameCount;
            if (World) World.ConsumeFreeplayOnboardingInputV414();
        }

        void SynchronizeFreeplayOnboardingPhaseV414(PanicPhase phase)
        {
            if (phase == PanicPhase.Title) _menuChoice = 0;
            if (_freeplayOnboardingOpenV414 && (phase != PanicPhase.Preparing || World.Model.IsSimulation ||
                World.Model.IsJuryDemo || World.Model.IsTutorial)) CancelFreeplayOnboardingV414();
        }

        bool ProcessFreeplayOnboardingHotkeysV414()
        {
            if (!FreeplayOnboardingInputBlockedV414) return false;
            if (!_freeplayOnboardingOpenV414 || _freeplayOnboardingTransitionFrameV414 == Time.frameCount) return true;
            // Reassert the same standard owner if an external UI attempted to release it while this modal is open.
            if (!_paused || !World.Model.IsPaused) SetPaused(true);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape))
                ConfirmFreeplayOnboardingV414();
            return true;
        }

        void EnsureFreeplayGuideStylesV414()
        {
            if (_freeplayGuideBodyV414 != null) return;
            _freeplayGuideBodyV414 = new GUIStyle(_body) { fontSize = 24, wordWrap = true, alignment = TextAnchor.UpperLeft };
            _freeplayGuideHeadingV414 = new GUIStyle(_heading) { fontSize = 27, wordWrap = true, alignment = TextAnchor.UpperLeft };
            _freeplayGuideKeyV414 = new GUIStyle(_caps) { fontSize = 23, wordWrap = false, alignment = TextAnchor.UpperLeft };
        }

        void FreeplayGuideCardV414(Rect rect, string heading, string body, Color accent)
        {
            Frame(rect, Panel, new Color(accent.r, accent.g, accent.b, .45f), 1);
            Label(new Rect(rect.x + 16, rect.y + 10, rect.width - 32, 30), heading, ColorText(_freeplayGuideKeyV414, accent));
            Label(new Rect(rect.x + 16, rect.y + 40, rect.width - 32, rect.height - 46), body, _freeplayGuideBodyV414);
        }

        void DrawFreeplayOnboardingV414()
        {
            EnsureFreeplayGuideStylesV414();
            Fill(LogicalCanvasBounds, new Color(.025f, .025f, .025f, .86f));
            Rect panel = new Rect(160, 48, 1280, 804);
            Frame(panel, Night, Brass, 1); Region(LogicalCanvasBounds);
            Label(new Rect(204, 78, 1192, 60), "SERBEST OYNA · İLK ADIMLAR", _title);
            Label(new Rect(208, 150, 1184, 42), "OYUN DURAKLATILDI · " + FormatTime(World.Model.TimeRemaining) + " HAZIRLIK SÜRESİ KORUNUYOR", ColorText(_freeplayGuideHeadingV414, Brass));
            Label(new Rect(204, 208, 566, 36), "KONTROLLER", _freeplayGuideHeadingV414);
            Label(new Rect(828, 208, 568, 36), "ÖNCE ÖNLEM, SONRA ÇANTA", _freeplayGuideHeadingV414);

            FreeplayGuideCardV414(new Rect(204, 252, 566, 76), "WASD / OKLAR · SHIFT · SPACE", "Yürü. Shift ile koş; Space ile zıpla.", Sea);
            FreeplayGuideCardV414(new Rect(204, 336, 566, 108), "E / F · ETKİLEŞİM", "E: kapı/dolap aç-kapat. Malzeme otomatik eklenir. Son aşamada E/F: erzak al.", Sea);
            FreeplayGuideCardV414(new Rect(204, 452, 566, 108), "MALZEME VE ÖNLEM", "Yaklaş, uygun işi ekrandan seç. Malzeme ve bütçe gerekir.", Sea);
            FreeplayGuideCardV414(new Rect(204, 568, 566, 76), "TAB · J", "Tab: envanter/çanta. J: hazırlık defteri.", Sea);
            FreeplayGuideCardV414(new Rect(204, 652, 566, 76), "ESC", "Duraklat; açık işlemi kapat veya iptal et.", Sea);

            FreeplayGuideCardV414(new Rect(828, 252, 568, 140), "1 · ÖNLEM — 120 SANİYE", "Dolaplardan malzeme al. Eşyaları sabitle, evi güçlendir ve tesisatı güvene al.", Brass);
            FreeplayGuideCardV414(new Rect(828, 408, 568, 140), "2 · ÇANTA — SON 30 SANİYE", "Kamera otomatik FPS olur. Açıkta duran 1 su + 1 gıdayı E/F ile al; gereken ilk birimler çantaya gider.", Brass);
            FreeplayGuideCardV414(new Rect(828, 564, 568, 164), "3 · BODRUM SIĞINAĞI", "Merdivenden bodruma in. Sığınak kapağına yaklaş ve E ile gir. Hazırlık bitmeden yetiş.", Brass);

            Button(new Rect(204, 744, 1192, 64), "ANLADIM, BAŞLA · ENTER", true, Brass, () => ConfirmFreeplayOnboardingV414());
            Label(new Rect(208, 818, 1184, 26), "Enter veya Esc: bu açıklamayı kapat ve başla. Ardından giriş hikâyesi ve normal oyun devam eder.", _small);
            Event e = Event.current;
            if (e.type == EventType.KeyDown || e.type == EventType.KeyUp)
            {
                if (e.type == EventType.KeyDown && _freeplayOnboardingTransitionFrameV414 != Time.frameCount &&
                    (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Escape))
                    ConfirmFreeplayOnboardingV414();
                e.Use();
            }
        }
    }

    public sealed partial class PanicHomeWorld
    {
        // Keep the private frame setters owned by World; HUD requests one shared-frame consumption.
        internal void ConsumeFreeplayOnboardingInputV414()
        {
            CancelFrame = DiscreteInputFrame = Time.frameCount;
        }
    }
}
