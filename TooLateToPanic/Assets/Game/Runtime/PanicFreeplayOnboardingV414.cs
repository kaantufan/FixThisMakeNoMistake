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
        GUIStyle _freeplayGuideTitleV414, _freeplayGuideKeyWideV414, _freeplayGuideKeyDimV414, _freeplayGuideKeyDimWideV414;
        GUIStyle _freeplayGuideMutedV414, _freeplayGuideNumberV414;
        bool _freeplayGuideCompactV414;

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
            bool compact = _scale < .8f;
            if (_freeplayGuideBodyV414 != null && _freeplayGuideCompactV414 == compact) return;
            _freeplayGuideCompactV414 = compact;
            Color ink = new Color32(244, 247, 248, 255), muted = new Color32(189, 205, 214, 255);
            _freeplayGuideBodyV414 = new GUIStyle(_small) { font = _bodyFont, fontSize = compact ? 24 : 23, fontStyle = FontStyle.Normal, wordWrap = true, alignment = TextAnchor.MiddleLeft };
            _freeplayGuideBodyV414.normal.textColor = ink;
            _freeplayGuideHeadingV414 = new GUIStyle(_freeplayGuideBodyV414) { fontSize = compact ? 28 : 26, fontStyle = FontStyle.Bold, wordWrap = false };
            _freeplayGuideTitleV414 = new GUIStyle(_freeplayGuideHeadingV414) { fontSize = 34 };
            _freeplayGuideKeyV414 = new GUIStyle(_freeplayGuideHeadingV414) { fontSize = compact ? 36 : 34, alignment = TextAnchor.MiddleCenter };
            _freeplayGuideKeyWideV414 = new GUIStyle(_freeplayGuideKeyV414) { fontSize = 22 };
            _freeplayGuideKeyDimV414 = new GUIStyle(_freeplayGuideKeyV414) { fontSize = compact ? 22 : 20, fontStyle = FontStyle.Normal };
            _freeplayGuideKeyDimV414.normal.textColor = new Color32(169, 182, 191, 255);
            _freeplayGuideKeyDimWideV414 = new GUIStyle(_freeplayGuideKeyDimV414) { fontSize = 18 };
            _freeplayGuideMutedV414 = new GUIStyle(_freeplayGuideBodyV414) { fontSize = compact ? 22 : 21 };
            _freeplayGuideMutedV414.normal.textColor = muted;
            _freeplayGuideNumberV414 = new GUIStyle(_freeplayGuideKeyWideV414) { fontSize = 26 };
            _freeplayGuideNumberV414.normal.textColor = new Color32(16, 25, 31, 255);
        }

        // Family0 is de-emphasized context,1 movement,2 interaction,3 menus,4 confirmation.
        // These are drawn instructions, not interactive exercises or rebinding controls.
        static Color FreeplayKeyFillV414(int family)
        {
            switch (family)
            {
                case 1: return new Color32(22, 70, 77, 255);
                case 2: return new Color32(94, 68, 28, 255);
                case 3: return new Color32(52, 59, 98, 255);
                case 4: return new Color32(39, 89, 66, 255);
                default: return new Color32(38, 54, 64, 255);
            }
        }
        static Color FreeplayKeyEdgeV414(int family)
        {
            switch (family)
            {
                case 1: return new Color32(123, 225, 222, 255);
                case 2: return new Color32(255, 211, 138, 255);
                case 3: return new Color32(188, 198, 255, 255);
                case 4: return new Color32(182, 242, 205, 255);
                default: return new Color32(132, 149, 158, 255);
            }
        }
        void FreeplayKeyboardKeyV414(Rect rect, string label, int family)
        {
            Frame(rect, FreeplayKeyFillV414(family), FreeplayKeyEdgeV414(family), family == 0 ? 1 : 3);
            if (family != 0) Fill(new Rect(rect.x + 7, rect.yMax - 6, rect.width - 14, 2), new Color32(244, 247, 248, 255));
            GUIStyle style = family == 0 ? label.Length > 3 ? _freeplayGuideKeyDimWideV414 : _freeplayGuideKeyDimV414 :
                label.Length > 1 ? _freeplayGuideKeyWideV414 : _freeplayGuideKeyV414;
            Label(new Rect(rect.x + 3, rect.y + 1, rect.width - 6, rect.height - 5), label, style);
        }
        void FreeplayKeyboardRowV414(float y, string[] keys, float[] widths, int[] families)
        {
            float x = 104;
            for (int i = 0; i < keys.Length; i++)
            {
                FreeplayKeyboardKeyV414(new Rect(x, y, widths[i] * 50 - 4, 46), keys[i], families[i]);
                x += widths[i] * 50;
            }
        }
        void FreeplayArrowKeyV414(Rect rect, Vector2 direction)
        {
            FreeplayKeyboardKeyV414(rect, string.Empty, 1);
            Vector2 head = rect.center + direction * 11, side = new Vector2(-direction.y, direction.x);
            Color ink = new Color32(244, 247, 248, 255);
            DrawLine(rect.center - direction * 10, head, ink, 3);
            DrawLine(head, head - direction * 7 + side * 6, ink, 3);
            DrawLine(head, head - direction * 7 - side * 6, ink, 3);
        }
        void FreeplayGuideLegendV414(Rect rect, string text, int family)
        {
            Frame(rect, new Color32(24, 37, 45, 255), new Color32(82, 102, 112, 255), 1);
            Fill(new Rect(rect.x, rect.y, 4, rect.height), FreeplayKeyEdgeV414(family));
            Label(new Rect(rect.x + 14, rect.y + 2, rect.width - 24, rect.height - 4), text, _freeplayGuideBodyV414);
        }
        void FreeplayGuideStepV414(Rect rect, int number, string heading, string body)
        {
            Frame(rect, new Color32(24, 37, 45, 255), new Color32(82, 102, 112, 255), 1);
            Rect mark = new Rect(rect.x + 16, rect.y + 16, 42, 42);
            Fill(mark, new Color32(255, 211, 138, 255));
            Label(mark, number.ToString(), _freeplayGuideNumberV414);
            Label(new Rect(rect.x + 72, rect.y + 14, rect.width - 88, 42), heading, _freeplayGuideHeadingV414);
            Label(new Rect(rect.x + 20, rect.y + 67, rect.width - 40, rect.height - 78), body, _freeplayGuideBodyV414);
        }

        void DrawFreeplayOnboardingV414()
        {
            EnsureFreeplayGuideStylesV414();
            Fill(LogicalCanvasBounds, new Color32(10, 16, 21, 248));
            Frame(new Rect(44, 34, 1512, 842), new Color32(16, 25, 31, 255), new Color32(132, 149, 158, 255), 1);
            Region(LogicalCanvasBounds);
            Label(new Rect(80, 57, 1050, 48), "Serbest oyna · Kontroller", _freeplayGuideTitleV414);
            Frame(new Rect(1172, 58, 352, 44), FreeplayKeyFillV414(1), FreeplayKeyEdgeV414(1), 1);
            Label(new Rect(1184, 63, 328, 34), "Duraklatıldı · " + FormatTime(World.Model.TimeRemaining), _freeplayGuideKeyWideV414);
            Label(new Rect(82, 111, 1436, 31), "Önce evi hazırla, sonra çantanı hazırla ve sığınağa yetiş.", _freeplayGuideBodyV414);

            Frame(new Rect(76, 154, 982, 610), new Color32(17, 27, 34, 255), new Color32(82, 102, 112, 255), 1);
            Frame(new Rect(1080, 154, 444, 610), new Color32(17, 27, 34, 255), new Color32(82, 102, 112, 255), 1);
            Label(new Rect(100, 166, 930, 32), "Klavye · Q düzeni", _freeplayGuideHeadingV414);
            Label(new Rect(1100, 166, 404, 32), "Üç kısa adım", _freeplayGuideHeadingV414);

            FreeplayKeyboardKeyV414(new Rect(104, 208, 72, 36), "Esc", 3);
            for (int i = 0; i < 12; i++)
                FreeplayKeyboardKeyV414(new Rect(212 + i * 46 + (i / 4) * 14, 208, 42, 36), "F" + (i + 1), 0);
            FreeplayKeyboardRowV414(254,
                new[] { "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "Backspace" },
                new[] { 1f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
                new[] { 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            FreeplayKeyboardRowV414(306,
                new[] { "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]", "\\" },
                new[] { 1.5f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1.5f },
                new[] { 3, 2, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            FreeplayKeyboardRowV414(358,
                new[] { "Caps", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "Enter" },
                new[] { 1.75f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2.25f },
                new[] { 0, 1, 1, 1, 2, 0, 0, 3, 0, 0, 0, 0, 4 });
            FreeplayKeyboardRowV414(410,
                new[] { "Shift", "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/", "Shift" },
                new[] { 2.25f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2.75f },
                new[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            FreeplayKeyboardRowV414(462,
                new[] { "Ctrl", "Win", "Alt", "Space", "Alt", "Fn", "Menu", "Ctrl" },
                new[] { 1.25f, 1.25f, 1.25f, 6.25f, 1.25f, 1.25f, 1.25f, 1.25f },
                new[] { 0, 0, 0, 1, 0, 0, 0, 0 });
            Label(new Rect(876, 306, 152, 31), "Oklar", _freeplayGuideMutedV414);
            FreeplayArrowKeyV414(new Rect(926, 358, 44, 46), new Vector2(0, -1));
            FreeplayArrowKeyV414(new Rect(876, 410, 44, 46), new Vector2(-1, 0));
            FreeplayArrowKeyV414(new Rect(926, 410, 44, 46), new Vector2(0, 1));
            FreeplayArrowKeyV414(new Rect(976, 410, 44, 46), new Vector2(1, 0));
            Label(new Rect(876, 474, 152, 30), "Hareket", _freeplayGuideMutedV414);

            FreeplayGuideLegendV414(new Rect(104, 534, 452, 42), "WASD / Oklar · Hareket et", 1);
            FreeplayGuideLegendV414(new Rect(572, 534, 462, 42), "E · Etkileşim   Q · Hedef değiştir", 2);
            FreeplayGuideLegendV414(new Rect(104, 582, 452, 42), "Sol Shift · Koş  /  Space · Zıpla", 1);
            FreeplayGuideLegendV414(new Rect(572, 582, 462, 42), "F · Erzak al (son 45 sn)", 2);
            FreeplayGuideLegendV414(new Rect(104, 630, 930, 42), "Tab · Çanta     J · Defter     F1 · Küçük tuş rehberi     Esc · Duraklat", 3);
            Label(new Rect(104, 681, 930, 28), "1 · Sabitle    3 · Kat duvarlarını güçlendir    Space · İşte vur", _freeplayGuideMutedV414);
            Label(new Rect(104, 718, 930, 32), "Fare: hedef seç · FPS'te fareyle bak, E/F ile erzak al.", _freeplayGuideMutedV414);

            FreeplayGuideStepV414(new Rect(1096, 208, 412, 142), 1, "120 sn · Önlem",
                "Malzeme topla, eşyaları sabitle.\nDuvar ve tesisatı güvene al.");
            FreeplayGuideStepV414(new Rect(1096, 364, 412, 158), 2, "Son 45 sn · Çanta",
                "Kamera otomatik FPS olur.\nE/F ile 1 su + 1 gıda al.");
            FreeplayGuideStepV414(new Rect(1096, 536, 412, 158), 3, "Süre dolmadan · Sığınak",
                "Selde üst kat panik odasına çık.\nDiğer afetlerde bodruma in; E ile gir.");
            Label(new Rect(1100, 714, 408, 34), "Rehber açıkken süre durur.", _freeplayGuideMutedV414);

            Button(new Rect(76, 782, 1448, 58), "Anladım, başla · Enter / Esc", true, new Color32(255, 211, 138, 255), () => ConfirmFreeplayOnboardingV414());
            Label(new Rect(84, 846, 1420, 26), PanicKktcStoryV53.ModeShelterBridgeV56, _freeplayGuideMutedV414);
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
