using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    // Presentation only. Existing entry, pause, audio and navigation owners remain authoritative.
    public sealed partial class PanicHomeHud
    {
        static readonly Color MenuInkV57 = new Color(.115f, .125f, .123f, 1f);
        static readonly Color MenuPaperV57 = new Color(.91f, .885f, .81f, 1f);
        static readonly Color MenuWhiteV57 = new Color(.97f, .95f, .89f, 1f);
        static readonly Color MenuOchreV57 = new Color(.77f, .60f, .29f, 1f);
        static readonly Color MenuRuleV57 = new Color(.31f, .33f, .30f, .3f);
        static readonly Color MenuDimV57 = new Color(.36f, .39f, .35f, 1f);
        GUIStyle menuTitleV57, menuHeadingV57, menuActionV57, menuTextV57;
        GUIStyle menuNoteV57, menuNumberV57, menuTimeV57, menuRightV57;
        GUIStyle menuSliderTrackV57, menuSliderThumbV57, menuScoreWaveV57;
        public bool MenuScoresFitV57 { get; private set; }
        public int MenuScoreRowCountV57 { get; private set; }
        public int MenuScoreMeasurementFrameV57 { get; private set; }
        public string MenuScoreFitDetailV57 { get; private set; }

        void EnsureMenuStylesV57()
        {
            if (menuTitleV57 != null) return;
            menuTitleV57 = MakeStyle(_displayFont, 44, FontStyle.Bold, MenuInkV57, TextAnchor.MiddleLeft);
            menuHeadingV57 = MakeStyle(_condensedFont, 28, FontStyle.Bold, MenuInkV57, TextAnchor.MiddleLeft);
            menuActionV57 = MakeStyle(_condensedFont, 28, FontStyle.Bold, MenuWhiteV57, TextAnchor.MiddleLeft);
            menuTextV57 = MakeStyle(_bodyFont, 24, FontStyle.Normal, MenuInkV57, TextAnchor.MiddleLeft);
            menuTextV57.wordWrap = true;
            menuNoteV57 = MakeStyle(_condensedFont, 22, FontStyle.Normal, MenuDimV57, TextAnchor.MiddleLeft);
            menuNumberV57 = MakeStyle(_displayFont, 27, FontStyle.Normal, MenuInkV57, TextAnchor.MiddleLeft);
            menuTimeV57 = MakeStyle(_displayFont, 112, FontStyle.Bold, MenuWhiteV57, TextAnchor.MiddleLeft);
            menuRightV57 = MakeStyle(_displayFont, 30, FontStyle.Normal, MenuInkV57, TextAnchor.MiddleRight);
            menuScoreWaveV57 = MakeStyle(_condensedFont, 22, FontStyle.Bold, MenuInkV57, TextAnchor.MiddleLeft);
            menuSliderTrackV57 = MenuBlankSliderStyleV57(GUI.skin.horizontalSlider);
            menuSliderThumbV57 = MenuBlankSliderStyleV57(GUI.skin.horizontalSliderThumb);
            menuSliderTrackV57.fixedHeight = 28f;
            menuSliderThumbV57.fixedWidth = 24f;
            menuSliderThumbV57.fixedHeight = 28f;
        }

        void DrawTitleDesignV57()
        {
            if (simulationSelectOpen) { DrawSimulationSelector(); return; }
            EnsureMenuStylesV57();
            Fill(LogicalCanvasBounds, new Color(.055f, .065f, .06f, .63f));
            MenuSheetV57(new Rect(70, 55, 844, 790), MenuInkV57);
            MenuSheetV57(new Rect(914, 55, 616, 790), MenuPaperV57);
            Fill(new Rect(70, 55, 8, 790), MenuOchreV57);
            Label(new Rect(112, 84, 760, 30), PanicKktcStoryV53.MenuLocation,
                ColorText(menuHeadingV57, MenuPaperV57));
            Fill(new Rect(112, 124, 760, 1), new Color(.91f, .885f, .81f, .22f));
            if (!_brandLogo) _brandLogo = Resources.Load<Texture2D>("HUD/TltpLogo");
            if (_brandLogo)
                GUI.DrawTexture(new Rect(156, 133, 672, 223), _brandLogo, ScaleMode.ScaleToFit, true);
            else
            {
                Label(new Rect(112, 139, 760, 112), "TOO LATE", ColorText(_logo, MenuWhiteV57));
                Label(new Rect(112, 250, 760, 112), "TO PANIC!", ColorText(_logo, MenuOchreV57));
            }
            Label(new Rect(112, 369, 760, 56), PanicKktcStoryV53.MenuPremise,
                ColorText(menuTextV57, MenuWhiteV57));
            Label(new Rect(112, 432, 760, 30), PanicKktcStoryV53.FictionNotice,
                ColorText(menuNoteV57, MenuPaperV57));

            MenuActionV57(new Rect(112, 488, 760, 66), PrimaryPlayLabelV414, "play", _menuChoice == 0, false,
                () => ActivateTitleChoiceV414(0));
            MenuActionV57(new Rect(112, 565, 760, 66), TitleChoiceLabelV414(1), "bag", _menuChoice == 1, false,
                () => ActivateTitleChoiceV414(1));
            MenuActionV57(new Rect(112, 642, 374, 58), TitleChoiceLabelV414(2), "shield", _menuChoice == 2, false,
                () => ActivateTitleChoiceV414(2));
            MenuActionV57(new Rect(498, 642, 374, 58), TitleChoiceLabelV414(3), "book", _menuChoice == 3, false,
                () => ActivateTitleChoiceV414(3));
            MenuActionV57(new Rect(112, 711, 320, 58), TitleChoiceLabelV414(4), "exit", _menuChoice == 4, false,
                () => ActivateTitleChoiceV414(4));
            MenuKeysV57(new Rect(112, 792, 760, 30), "OKLAR / WASD  SEÇ", "ENTER  AÇ", false);

            DrawMenuFieldNotesV57();
            Event e = Event.current;
            if (DeveloperMenuOpen || DeveloperInputBlockedThisFrame) return;
            if (e.type != EventType.KeyDown) return;
            if (IsNavigationKey(e.keyCode))
            {
                _menuChoice = NavigateTitleChoice(_menuChoice, e.keyCode);
                e.Use();
            }
            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                ActivateSelectedTitleChoiceV414();
                e.Use();
            }
        }

        void DrawMenuFieldNotesV57()
        {
            Label(new Rect(958, 88, 528, 32), "EV HAZIRLIK NOTLARI", menuHeadingV57);
            Fill(new Rect(958, 132, 64, 5), MenuOchreV57);
            Label(new Rect(958, 147, 528, 43), "ÖNCE ÖNLEM.", menuTitleV57);
            Label(new Rect(958, 190, 528, 43), "SONRA ÇANTA.", menuTitleV57);
            DrawMenuStepV57(232, "01", "tools", "EVİ HAZIRLA", "Malzeme topla, eşyaları sabitle.");
            DrawMenuStepV57(310, "02", "bag", "ÇANTANI TAMAMLA", "Su ve gıdayı hazırlamayı unutma.");
            DrawMenuStepV57(388, "03", "home", "SIĞINAĞA YETİŞ", "Sel varsa üst kattaki panik odasına.");
            Fill(new Rect(958, 477, 528, 2), MenuInkV57);
            Label(new Rect(958, 490, 328, 37), "YEREL SKORLAR", menuHeadingV57);
            Label(new Rect(1314, 494, 172, 31), "PUAN", ColorText(menuRightV57, MenuDimV57));
            MenuScoresFitV57 = true; MenuScoreRowCountV57 = 0; MenuScoreFitDetailV57 = "";
            if (Event.current.type == EventType.Repaint) MenuScoreMeasurementFrameV57 = Time.frameCount;
            List<HighscoreEntry> entries = World.Model.Highscores;
            if (entries == null || entries.Count == 0)
            {
                PanicUiSymbolsV57.Draw(new Rect(958, 560, 48, 48), "book", MenuDimV57);
                Label(new Rect(1028, 548, 458, 42), "İLK KOŞUNU TAMAMLA", menuHeadingV57);
                Label(new Rect(1028, 594, 458, 60), "Skorların bu bilgisayarda saklanır.", menuTextV57);
                return;
            }
            int count = Mathf.Min(6, entries.Count);
            for (int i = 0; i < count; i++)
            {
                HighscoreEntry entry = entries[i];
                float y = 539 + i * 47;
                if (i == 0) Fill(new Rect(949, y - 2, 546, 44), new Color(.77f, .60f, .29f, .17f));
                Label(new Rect(958, y, 43, 36), (i + 1).ToString("00"), ColorText(menuNumberV57, MenuDimV57));
                Rect waveRect = new Rect(1013, y, 96, 36), dateRect = new Rect(1117, y + 1, 101, 34);
                Rect scoreRect = new Rect(1228, y - 5, 258, 46);
                string waveText = entry.waves + " DALGA", dateText = entry.date ?? "", scoreText = entry.score.ToString("N0");
                FittedLabel(waveRect, waveText, menuScoreWaveV57, MenuInkV57, 20);
                Vector2 waveSize = _fittedStyle.CalcSize(new GUIContent(waveText));
                int waveFont = _fittedStyle.fontSize;
                FittedLabel(dateRect, dateText, menuNoteV57, MenuDimV57, 20);
                Vector2 dateSize = _fittedStyle.CalcSize(new GUIContent(dateText));
                int dateFont = _fittedStyle.fontSize;
                FittedLabel(scoreRect, scoreText, menuRightV57, MenuInkV57, 22);
                Vector2 scoreSize = _fittedStyle.CalcSize(new GUIContent(scoreText));
                bool fits = waveSize.x <= waveRect.width && waveSize.y <= waveRect.height &&
                    dateSize.x <= dateRect.width && dateSize.y <= dateRect.height &&
                    scoreSize.x <= scoreRect.width && scoreSize.y <= scoreRect.height &&
                    Mathf.Min(waveFont, dateFont, _fittedStyle.fontSize) * _scale >= 12f - .001f;
                MenuScoresFitV57 &= fits; MenuScoreRowCountV57++;
                MenuScoreFitDetailV57 += scoreText + " font=" + _fittedStyle.fontSize + " measured=" + scoreSize + " rect=" + scoreRect + " fit=" + fits + "; ";
                Fill(new Rect(958, y + 43, 528, 1), MenuRuleV57);
            }
        }

        void DrawMenuStepV57(float y, string number, string icon, string heading, string detail)
        {
            Label(new Rect(958, y + 4, 38, 37), number, ColorText(menuNumberV57, MenuDimV57));
            PanicUiSymbolsV57.Draw(new Rect(1009, y + 8, 36, 36), icon, MenuInkV57);
            Label(new Rect(1062, y, 424, 33), heading, menuHeadingV57);
            Label(new Rect(1062, y + 35, 424, 32), detail, menuNoteV57);
        }

        void DrawPauseDesignV57()
        {
            EnsureMenuStylesV57();
            if (AudioSettingsOpenV56) { DrawAudioSettingsDesignV57(); return; }
            Fill(LogicalCanvasBounds, new Color(.055f, .065f, .06f, .67f));
            MenuSheetV57(new Rect(100, 80, 466, 740), MenuInkV57);
            MenuSheetV57(new Rect(566, 80, 934, 740), MenuPaperV57);
            Fill(new Rect(100, 80, 8, 740), MenuOchreV57);
            PanicUiSymbolsV57.Draw(new Rect(146, 129, 45, 45), "pause", MenuOchreV57);
            Label(new Rect(208, 128, 310, 44), "SÜRE VE OYUN DURDU", ColorText(menuNoteV57, MenuPaperV57));
            Label(new Rect(146, 204, 374, 68), World.JuryActive ? "KISA BİR MOLA" : "DURAKLATILDI",
                ColorText(menuTitleV57, MenuWhiteV57));
            Fill(new Rect(146, 299, 374, 1), new Color(.91f, .885f, .81f, .22f));
            PanicUiSymbolsV57.Draw(new Rect(148, 345, 40, 40), "clock", MenuOchreV57);
            Label(new Rect(206, 348, 306, 34), "SAYAÇ", ColorText(menuHeadingV57, MenuPaperV57));
            Label(new Rect(142, 392, 378, 132), PauseTimerTextV57(), menuTimeV57);
            Label(new Rect(146, 551, 374, 94), "Hazır olduğunda kaldığın yerden devam et.",
                ColorText(menuTextV57, MenuPaperV57));
            Fill(new Rect(146, 685, 374, 1), new Color(.91f, .885f, .81f, .22f));
            Label(new Rect(146, 714, 374, 58), PanicKktcStoryV53.MenuLocation,
                ColorText(menuNoteV57, MenuPaperV57));

            Label(new Rect(620, 121, 830, 36), "KALDIĞIN YERDEN", menuHeadingV57);
            MenuActionV57(new Rect(620, 181, 830, 68), "DEVAM ET", "continue", _pauseChoice == 0, true, () => SetPaused(false));
            MenuActionV57(new Rect(620, 267, 405, 64), "GELİŞTİRİCİ · F2", "tools", _pauseChoice == 1, true, ToggleDeveloperMenu);
            MenuActionV57(new Rect(1041, 267, 409, 64), "YENİDEN BAŞLA", "back", _pauseChoice == 2, true, RestartRun);
            MenuActionV57(new Rect(620, 349, 830, 64), "BAŞLIĞA DÖN", "home", _pauseChoice == 3, true, ReturnToMenu);
            Fill(new Rect(620, 450, 830, 2), MenuInkV57);
            Label(new Rect(620, 467, 830, 36), "SES VE HAREKET", menuHeadingV57);
            MenuActionV57(new Rect(620, 523, 405, 64), PanicBackgroundMusic.Enabled ? "MÜZİK · AÇIK" : "MÜZİK · KAPALI",
                "sound", _pauseChoice == 4, true, () => PanicBackgroundMusic.Enabled = !PanicBackgroundMusic.Enabled);
            MenuActionV57(new Rect(1041, 523, 409, 64), World.ReducedFpsMotionV54 ? "PANİK EFEKTİ · AZ" : "PANİK EFEKTİ · TAM",
                "motion", _pauseChoice == 5, true, () => World.SetReducedFpsMotionV54(!World.ReducedFpsMotionV54));
            MenuActionV57(new Rect(620, 605, 830, 64), "SES AYARLARI", "settings", _pauseChoice == 6, true, OpenAudioSettingsV56);
            MenuKeysV57(new Rect(620, 736, 830, 32), "OKLAR  SEÇ  /  ENTER  AÇ", "ESC  DEVAM ET", true);
            HandlePauseNavigation();
        }

        void DrawAudioSettingsDesignV57()
        {
            Fill(LogicalCanvasBounds, new Color(.055f, .065f, .06f, .72f));
            MenuSheetV57(new Rect(240, 100, 1120, 700), MenuPaperV57);
            Fill(new Rect(240, 100, 8, 700), MenuOchreV57);
            PanicUiSymbolsV57.Draw(new Rect(288, 145, 42, 42), "sound", MenuInkV57);
            Label(new Rect(354, 135, 938, 66), "SES AYARLARI", menuTitleV57);
            Label(new Rect(288, 213, 1004, 34), "Değişiklikler kaydedilir. Oyun ve süre duruyor.", menuTextV57);
            DrawMenuVolumeRowV57(0, 280, "GENEL", PanicAudioSettingsV56.MasterVolume);
            DrawMenuVolumeRowV57(1, 371, "MÜZİK", PanicAudioSettingsV56.MusicVolume);
            DrawMenuVolumeRowV57(2, 462, "EFEKTLER", PanicAudioSettingsV56.SfxVolume);
            MenuActionV57(new Rect(288, 567, 494, 60), PanicAudioSettingsV56.Muted ? "SES · KAPALI" : "SES · AÇIK",
                "sound", audioChoiceV56 == 3, true, () => PanicAudioSettingsV56.SetMuted(!PanicAudioSettingsV56.Muted));
            MenuActionV57(new Rect(800, 567, 492, 60), PanicAudioSettingsV56.MusicEnabled ? "MÜZİK · AÇIK" : "MÜZİK · KAPALI",
                "sound", audioChoiceV56 == 4, true, () => PanicBackgroundMusic.Enabled = !PanicBackgroundMusic.Enabled);
            Fill(new Rect(288, 661, 1004, 1), MenuRuleV57);
            MenuActionV57(new Rect(288, 694, 348, 58), "GERİ · ESC", "back", audioChoiceV56 == 5, true, CloseAudioSettingsV56);
            MenuKeysV57(new Rect(678, 706, 614, 32), "YUKARI / AŞAĞI  SEÇ", "SOL / SAĞ  SEVİYE", true);
            Label(new Rect(678, 744, 614, 30), "ENTER  AÇ / KAPAT", menuNoteV57);
            Event e = Event.current;
            if (e.type != EventType.KeyDown || DeveloperInputBlockedThisFrame) return;
            if (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.DownArrow)
            { audioChoiceV56 = (audioChoiceV56 + (e.keyCode == KeyCode.UpArrow ? 5 : 1)) % 6; e.Use(); }
            else if (audioChoiceV56 < 3 && (e.keyCode == KeyCode.LeftArrow || e.keyCode == KeyCode.RightArrow))
            {
                float value = AudioVolumeV56(audioChoiceV56) + (e.keyCode == KeyCode.LeftArrow ? -.05f : .05f);
                SetAudioVolumeV56(audioChoiceV56, value); e.Use();
            }
            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                if (audioChoiceV56 == 3) PanicAudioSettingsV56.SetMuted(!PanicAudioSettingsV56.Muted);
                else if (audioChoiceV56 == 4) PanicBackgroundMusic.Enabled = !PanicBackgroundMusic.Enabled;
                else if (audioChoiceV56 == 5) CloseAudioSettingsV56();
                e.Use();
            }
        }

        void DrawMenuVolumeRowV57(int index, float y, string title, float value)
        {
            Rect row = new Rect(288, y, 1004, 73);
            bool selected = audioChoiceV56 == index;
            Fill(row, selected ? new Color(.77f, .60f, .29f, .20f) : new Color(.115f, .125f, .123f, .045f));
            if (selected) Fill(new Rect(row.x, y, 5, row.height), MenuInkV57);
            Region(row);
            Label(new Rect(310, y + 17, 178, 38), title, menuHeadingV57);
            Rect slider = new Rect(520, y + 22, 590, 28);
            float next = GUI.HorizontalSlider(slider, value, 0f, 1f, menuSliderTrackV57, menuSliderThumbV57);
            Mark(slider);
            if (Mathf.Abs(next - value) > .004f) { audioChoiceV56 = index; SetAudioVolumeV56(index, next); }
            // Unity's thumb moves over width minus fixed thumb width. Draw the exact same travel.
            float travel = slider.width - menuSliderThumbV57.fixedWidth;
            float center = slider.x + 12f + travel * Mathf.Clamp01(next);
            Rect rail = new Rect(slider.x + 12f, slider.center.y - 3f, travel, 6f);
            Fill(rail, new Color(.115f, .125f, .123f, .24f));
            if (next > 0f) Fill(new Rect(rail.x, rail.y, travel * Mathf.Clamp01(next), rail.height), MenuInkV57);
            Rect handle = new Rect(center - 12f, slider.y, 24, 28);
            Fill(handle, GUI.enabled ? MenuInkV57 : MenuDimV57);
            Fill(new Rect(handle.x + 3, handle.y + 3, 18, 22), MenuPaperV57);
            Fill(new Rect(center - 1f, handle.y + 8, 2, 12), MenuInkV57);
            Label(new Rect(1148, y + 14, 122, 42), Mathf.RoundToInt(value * 100) + "%", menuRightV57);
        }

        static GUIStyle MenuBlankSliderStyleV57(GUIStyle source)
        {
            GUIStyle style = new GUIStyle(source);
            style.normal.background = null; style.hover.background = null;
            style.active.background = null; style.focused.background = null;
            style.onNormal.background = null; style.onHover.background = null;
            style.onActive.background = null; style.onFocused.background = null;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.border = new RectOffset(0, 0, 0, 0);
            return style;
        }

        string PauseTimerTextV57()
        {
            if (World.JuryActive && World.JurySnapshot != null)
                return FormatTime(World.JurySnapshot.StageRemainingSeconds);
            PanicModel model = World.Model;
            if (model.Phase == PanicPhase.Preparing || model.Phase == PanicPhase.Tutorial)
                return FormatTime(PreparationTimerRemaining);
            if (model.Phase == PanicPhase.Disaster) return FormatTime(model.PhaseTime);
            return "--:--";
        }

        void MenuSheetV57(Rect rect, Color paper)
        {
            Fill(new Rect(rect.x + 5, rect.y + 7, rect.width, rect.height), new Color(0, 0, 0, .25f));
            Fill(rect, paper);
            Region(rect);
        }

        void MenuActionV57(Rect rect, string text, string symbol, bool selected, bool onPaper, Action action)
        {
            selected = selected && GUI.enabled;
            bool hover = GUI.enabled && rect.Contains(LogicalPointer());
            Color ink = !GUI.enabled ? MenuDimV57 : selected ? MenuInkV57 : onPaper ? MenuInkV57 : MenuWhiteV57;
            Color baseColor = selected ? MenuOchreV57 : onPaper ? new Color(.115f, .125f, .123f, hover ? .11f : .055f)
                : new Color(.91f, .885f, .81f, hover ? .16f : .07f);
            Fill(rect, baseColor);
            Fill(new Rect(rect.x, rect.yMax - 1, rect.width, 1), onPaper ? MenuRuleV57 : new Color(.91f, .885f, .81f, .28f));
            if (selected)
            {
                Fill(new Rect(rect.x, rect.y, 6, rect.height), MenuWhiteV57);
                Fill(new Rect(rect.x, rect.y, rect.width, 2), MenuWhiteV57);
                Fill(new Rect(rect.x, rect.yMax - 2, rect.width, 2), MenuWhiteV57);
                Fill(new Rect(rect.xMax - 2, rect.y, 2, rect.height), MenuWhiteV57);
            }
            PanicUiSymbolsV57.Draw(new Rect(rect.x + 21, rect.center.y - 16, 32, 32), symbol, ink);
            FittedLabel(new Rect(rect.x + 72, rect.y + 2, rect.width - 111, rect.height - 4), text,
                menuActionV57, ink, 22);
            if (selected || hover)
            {
                float x = rect.xMax - 23;
                DrawLine(new Vector2(x - 5, rect.center.y - 6), new Vector2(x + 1, rect.center.y), ink, 2f);
                DrawLine(new Vector2(x + 1, rect.center.y), new Vector2(x - 5, rect.center.y + 6), ink, 2f);
            }
            Mark(rect);
            if (!GUI.enabled) return;
            if (!_developerMenuOpen && DeveloperInputBlockedThisFrame && !WaveLedgerButtonInputV56) return;
            if (MousePressed(rect) && action != null) { World.PlayUiSound(); action(); }
        }

        void MenuKeysV57(Rect rect, string first, string second, bool onPaper)
        {
            Color color = onPaper ? MenuDimV57 : MenuPaperV57;
            Label(new Rect(rect.x, rect.y, rect.width * .61f, rect.height), first, ColorText(menuNoteV57, color));
            GUIStyle right = ColorText(menuNoteV57, color);
            TextAnchor previous = right.alignment;
            right.alignment = TextAnchor.MiddleRight;
            Label(new Rect(rect.x + rect.width * .61f, rect.y, rect.width * .39f, rect.height), second, right);
            right.alignment = previous;
        }
    }
}
