using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Compact Turkish HUD. World owns gameplay input; this class draws it and owns only UI keys.</summary>
    public sealed class PanicHomeHud : MonoBehaviour
    {
        const float CanvasWidth = 1600f;
        const float CanvasHeight = 900f;

        static readonly Color Shade = new Color(.018f, .027f, .034f, .76f);
        static readonly Color Night = new Color(.035f, .052f, .064f, .96f);
        static readonly Color Panel = new Color(.058f, .081f, .091f, .98f);
        static readonly Color Raised = new Color(.086f, .116f, .124f, .98f);
        static readonly Color MainText = new Color(.93f, .94f, .89f, 1f);
        static readonly Color SoftText = new Color(.76f, .81f, .79f, 1f);
        static readonly Color Brass = new Color(.95f, .74f, .45f, 1f);
        static readonly Color Sea = new Color(.50f, .82f, .72f, 1f);
        static readonly Color Ember = new Color(.96f, .20f, .16f, 1f);
        static readonly Color Line = new Color(.48f, .59f, .55f, .24f);
        static readonly string[][] WalkControls = { new[] { "WASD", "YÜRÜ" }, new[] { "SHIFT", "KOŞ" }, new[] { "SPACE", "ZIPLA" }, new[] { "E", "KULLAN" }, new[] { "TAB", "ÇANTA" } };
        static readonly string[][] PrepareControls = { new[] { "WASD", "YÜRÜ" }, new[] { "SHIFT", "KOŞ" }, new[] { "SPACE", "ZIPLA" }, new[] { "E", "KULLAN" }, new[] { "TAB", "ÇANTA" }, new[] { "1", "SABİTLE" }, new[] { "2", "KORU" }, new[] { "3", "GÜÇLENDİR" }, new[] { "SAĞ TIK/4", "TAŞI" } };
        static readonly string[][] LootControls = { new[] { "Z/X", "SEÇ" }, new[] { "F", "AL" }, new[] { "E", "KAPAT" }, new[] { "TAB", "ÇANTA" } };
        static readonly string[][] EmptyLootControls = { new[] { "E", "KAPAT" }, new[] { "TAB", "ÇANTA" },new[] { "Q", "HEDEF" } };
        static readonly string[][] InventoryControls = { new[] { "OKLAR", "SEÇ" }, new[] { "ENTER", "ÇANTAYA" }, new[] { "F", "ÜRET" }, new[] { "BACKSPACE", "GERİ" } };
        static readonly string[][] WorkControls = { new[] { "A/D", "HİZALA" }, new[] { "ENTER", "KİLİTLE" }, new[] { "SPACE", "UYGULA" }, new[] { "ESC", "İPTAL" } };
        static readonly string[][] MoveControls = { new[] { "FARE", "YERLEŞTİR" }, new[] { "SOL TIK", "BIRAK" }, new[] { "TEKER / R", "DÖNDÜR" }, new[] { "SAĞ TIK", "İPTAL" }, new[] { "ESC", "İPTAL" } };
        static readonly string[][] InspectControls = { new[] { "WASD", "İNCELE" }, new[] { "ENTER", "İLERİ" }, new[] { "N", "BİTİR" }, new[] { "ESC", "DURAKLAT" } };

        public PanicHomeWorld World;

        /// <summary>The same condition used by both world and inventory shortage presentation.</summary>
        public bool RequirementsCardVisible
        {
            get
            {
                return World && World.Model != null && World.Expedition != null && !_paused &&
                    !World.CinematicActive && !(World.LossAnnounced && World.LossRevealTime > 0f) &&
                    !World.WorkActive && World.Expedition.HasMissingRequirements;
            }
        }

        public bool PointerOverUi
        {
            get
            {
                if (DeveloperMenuOpen || DeveloperInputBlockedThisFrame) return true;
                if (_uiRects.Count == 0 || _scale <= 0f) return false;
                Vector2 mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                Vector2 point = (mouse - _offset) / _scale;
                for (int i = 0; i < _uiRects.Count; i++)
                    if (_uiRects[i].Contains(point)) return true;
                return false;
            }
        }

        readonly List<Rect> _uiRects = new List<Rect>(48);
        readonly List<PanicItem> _inspectionItems = new List<PanicItem>(4);
        readonly Dictionary<GUIStyle, Dictionary<Color, GUIStyle>> _coloredStyles = new Dictionary<GUIStyle, Dictionary<Color, GUIStyle>>();
        bool _paused;
        bool _priorInputLock;
        bool _helpVisible;
        int _menuChoice;
        int _inspectionStep;
        string _inspectionFocusId;
        PanicPhase _lastPhase = (PanicPhase)(-1);
        float _scale = 1f;
        Vector2 _offset;
        bool _stylesReady;
        GUIStyle _title;
        GUIStyle _heading;
        GUIStyle _body;
        GUIStyle _small;
        GUIStyle _tiny;
        GUIStyle _center;
        GUIStyle _caps;
        Texture2D _dotTexture;

        bool _developerMenuOpen;
        bool _developerPriorPaused;
        bool _developerPriorHelp;
        int _developerTransitionFrame = -1;
        int _developerHotkeyFrame = -1;
        int _developerChoice;
        string _developerNotice;
        float _developerNoticeUntil;

        public bool DeveloperMenuOpen { get { return _developerMenuOpen; } }
        public bool DeveloperInputBlockedThisFrame { get { return _developerTransitionFrame == Time.frameCount; } }

        /// <summary>World calls this before ticking/input. HUD calls the same idempotent path.</summary>
        public void ProcessDeveloperHotkeys()
        {
            if (!World || World.Model == null || _developerHotkeyFrame == Time.frameCount) return;
            _developerHotkeyFrame = Time.frameCount;
            SynchronizeHudPhase();
            if (_developerMenuOpen && !string.IsNullOrEmpty(DeveloperUnavailableReason()))
                SetDeveloperMenuOpen(false);
            if (_developerMenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.Escape))
                    SetDeveloperMenuOpen(false);
            }
            else if (Input.GetKeyDown(KeyCode.F2)) SetDeveloperMenuOpen(true);
        }

        public void ToggleDeveloperMenu() { SetDeveloperMenuOpen(!_developerMenuOpen); }

        public void SetDeveloperMenuOpen(bool value)
        {
            if (value == _developerMenuOpen) return;
            if (value)
            {
                if (!World || World.Model == null) return;
                SynchronizeHudPhase();
                string unavailable = DeveloperUnavailableReason();
                if (!string.IsNullOrEmpty(unavailable))
                {
                    _developerNotice = unavailable;
                    _developerNoticeUntil = Time.unscaledTime + 3f;
                    return;
                }
                _developerPriorPaused = _paused;
                _developerPriorHelp = _helpVisible;
                _helpVisible = false;
                _developerChoice = 0;
                SetPaused(true);
                _developerMenuOpen = true;
                _developerNotice = null;
            }
            else
            {
                _developerMenuOpen = false;
                _helpVisible = _developerPriorHelp;
                SetPaused(_developerPriorPaused);
                // A phase owner may have acquired its own lock while the panel was visible.
                if (World && (World.CinematicActive || World.LossAnnounced)) World.InputLocked = true;
            }
            _developerTransitionFrame = Time.frameCount;
        }

        string DeveloperUnavailableReason()
        {
            if (!World || World.Model == null) return "Geliştirici menüsü henüz hazır değil.";
            if (World.WorkActive) return "Geliştirici: önce mevcut işi bitir veya iptal et.";
            if (World.IsMoving) return "Geliştirici: önce eşyayı bırak veya taşımayı iptal et.";
            if (World.InventoryOpen) return "Geliştirici: önce çantayı kapat, sonra F2'ye bas.";
            if (World.CinematicActive || World.LossAnnounced) return "Geliştirici menüsü bu sahne bitince açılabilir.";
            PanicPhase phase = World.Model.Phase;
            if (phase != PanicPhase.Title && phase != PanicPhase.Preparing &&
                phase != PanicPhase.Tutorial && phase != PanicPhase.Inspection)
                return "Geliştirici menüsü başlık, hazırlık ve hasar gezisinde açılabilir.";
            if (World.CancelFrame == Time.frameCount) return "Geliştirici: iptalden sonra yeniden F2'ye bas.";
            if (phase != PanicPhase.Title && World.InputLocked && !_paused)
                return "Geliştirici menüsü mevcut sahne bittikten sonra açılabilir.";
            return null;
        }

        void SynchronizeHudPhase()
        {
            PanicPhase phase = World.Model.Phase;
            if (phase == _lastPhase) return;
            if (_developerMenuOpen) SetDeveloperMenuOpen(false);
            if (_paused) SetPaused(false);
            if (World.CinematicActive || World.LossAnnounced) World.InputLocked = true;
            _inspectionStep = 0;
            _inspectionFocusId = null;
            _helpVisible = phase == PanicPhase.Tutorial && World.Model.IsTutorial;
            _lastPhase = phase;
        }

        void Update()
        {
            if (!World || World.Model == null) return;
            ProcessDeveloperHotkeys();
            if (DeveloperMenuOpen || DeveloperInputBlockedThisFrame) return;

            // A same-frame work cancel belongs to World. Do not reinterpret that Escape as Pause.
            if (World.CancelFrame == Time.frameCount) return;

            PanicPhase phase = World.Model.Phase;
            SynchronizeHudPhase();

            if (World.WorkActive || World.IsMoving) return; // World owns work and placement keys, including cancel.
            if (World.InventoryOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) World.ToggleInventory();
                return; // World owns selection, Enter packing, and F crafting.
            }

            if (_helpVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                _helpVisible = false;
                return;
            }
            if (_paused)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) SetPaused(false);
                return;
            }

            if (Input.GetKeyDown(KeyCode.H) && (phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial))
                _helpVisible = !_helpVisible;

            bool canPause = phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial || phase == PanicPhase.Inspection;
            if (canPause && Input.GetKeyDown(KeyCode.Escape)) SetPaused(true);
        }

        void SetPaused(bool value)
        {
            if (!World) return;
            if (value && !_paused)
            {
                _priorInputLock = World.InputLocked;
                World.InputLocked = true;
            }
            else if (!value && _paused)
            {
                World.InputLocked = _priorInputLock;
            }
            _paused = value;
        }

        void OnGUI()
        {
            if (!World || World.Model == null) return;
            PrepareCanvas();
            EnsureStyles();
            _uiRects.Clear();

            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            Color oldContent = GUI.contentColor;
            GUI.matrix = Matrix4x4.TRS(new Vector3(_offset.x, _offset.y, 0f), Quaternion.identity,
                new Vector3(_scale, _scale, 1f));
            GUI.color = Color.white;
            GUI.contentColor = Color.white;

            if (World.LossAnnounced && World.LossRevealTime > 0f)
            {
                DrawLossReveal();
            }
            else if (World.CinematicActive)
            {
                DrawCinematic();
            }
            else if (_developerMenuOpen)
            {
                if (World.Model.Phase != PanicPhase.Title) DrawStatusStrip();
                DrawDeveloperMenu();
            }
            else if (World.Model.Phase == PanicPhase.Title)
            {
                DrawTitle();
            }
            else if (_paused)
            {
                DrawStatusStrip();
                DrawPause();
            }
            else if (World.WorkActive)
            {
                DrawWork();
            }
            else if (World.InventoryOpen)
            {
                DrawInventory();
                if (RequirementsCardVisible) DrawInventoryRequirements();
            }
            else if (World.Model.Phase == PanicPhase.GameOver || World.Model.Phase == PanicPhase.Success)
            {
                DrawStatusStrip();
                DrawEndCard();
            }
            else
            {
                DrawStatusStrip();
                if (World.Model.Phase == PanicPhase.Inspection) DrawInspection();
                else DrawContext();
                if (_helpVisible && !World.LootVisible && (World.Model.Phase == PanicPhase.Tutorial || World.Model.Phase == PanicPhase.Preparing))
                    DrawHelpCard();
                if (World.Model.Phase == PanicPhase.Disaster) DrawDisasterCue();
                if (RequirementsCardVisible) DrawRequirements();
                if (World.NotificationVisible) DrawToast(World.ShortNotice);
            }

            if (World.Model.CheatedRun) DrawCheatedRunIndicator();
            if (!_developerMenuOpen && !string.IsNullOrEmpty(_developerNotice) && Time.unscaledTime < _developerNoticeUntil)
            {
                Rect notice = new Rect(410, 110, 780, 44);
                Frame(notice, Night, Brass, 1f);
                Mark(notice);
                Label(new Rect(notice.x + 14, notice.y + 5, notice.width - 28, 34), _developerNotice, _small);
            }

            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
            GUI.contentColor = oldContent;
        }

        void OnDestroy()
        {
            if (_dotTexture) Destroy(_dotTexture);
            _dotTexture = null;
        }

        void OnDisable()
        {
            if (_developerMenuOpen) SetDeveloperMenuOpen(false);
        }

        void PrepareCanvas()
        {
            float sx = Screen.width / CanvasWidth;
            float sy = Screen.height / CanvasHeight;
            _scale = Mathf.Max(.65f, Mathf.Min(sx, sy));
            _offset = new Vector2((Screen.width - CanvasWidth * _scale) * .5f,
                (Screen.height - CanvasHeight * _scale) * .5f);
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!font) font = GUI.skin.font;
            _title = MakeStyle(font, 34, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            _heading = MakeStyle(font, 23, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            _body = MakeStyle(font, 19, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            _small = MakeStyle(font, 16, FontStyle.Normal, SoftText, TextAnchor.MiddleLeft);
            _tiny = MakeStyle(font, 13, FontStyle.Normal, SoftText, TextAnchor.MiddleLeft);
            _center = MakeStyle(font, 17, FontStyle.Bold, MainText, TextAnchor.MiddleCenter);
            _caps = MakeStyle(font, 14, FontStyle.Bold, MainText, TextAnchor.MiddleCenter);
            _stylesReady = true;
        }

        static GUIStyle MakeStyle(Font font, int size, FontStyle weight, Color color, TextAnchor anchor)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = size,
                fontStyle = weight,
                alignment = anchor,
                wordWrap = false,
                clipping = TextClipping.Clip,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            return style;
        }

        void DrawTitle()
        {
            Fill(new Rect(0, 0, CanvasWidth, CanvasHeight), Shade);
            Rect panel = new Rect(250, 142, 1100, 616);
            Frame(panel, Night, Brass, 1.5f);
            Label(new Rect(302, 178, 570, 24), "EVDEKİ SON HAZIRLIK", ColorText(_small, Brass));
            Label(new Rect(298, 210, 540, 94), "TOO LATE\nTO PANIC!", _title);
            Label(new Rect(302, 330, 490, 26), "DEPREM   SEL   YANGIN", ColorText(_small, Brass));

            Button(new Rect(302, 408, 450, 54), "Oyuna başla", _menuChoice == 0, Sea, StartRun);
            Button(new Rect(302, 472, 450, 54), "Öğretici", _menuChoice == 1, Brass, StartTutorial);
            Button(new Rect(302, 536, 450, 54), "Geliştirici · F2", _menuChoice == 2, Brass, ToggleDeveloperMenu);
            Button(new Rect(302, 600, 450, 54), "Çıkış", _menuChoice == 3, SoftText, () => Application.Quit());
            DrawKeycap(new Rect(302, 684, 126, 34), "↑ ↓", "SEÇ", Brass);
            DrawKeycap(new Rect(438, 684, 160, 34), "ENTER", "AÇ", Sea);

            Rect scores = new Rect(835, 184, 456, 500);
            Frame(scores, Panel, Line, 1f);
            Label(new Rect(866, 204, 388, 34), "YEREL SKORLAR", ColorText(_body, Brass));
            List<HighscoreEntry> entries = World.Model.Highscores;
            if (entries == null || entries.Count == 0)
            {
                Label(new Rect(866, 260, 380, 30), "Henüz skor yok", _small);
            }
            else
            {
                int count = Mathf.Min(7, entries.Count);
                for (int i = 0; i < count; i++)
                {
                    HighscoreEntry entry = entries[i];
                    float y = 260 + i * 48;
                    Label(new Rect(866, y, 40, 28), (i + 1).ToString("00"), ColorText(_small, Brass));
                    Label(new Rect(916, y, 170, 28), entry.waves + " dalga", _small);
                    Label(new Rect(1085, y, 166, 28), entry.score.ToString("N0"), _small);
                    Label(new Rect(916, y + 23, 335, 18), entry.date ?? "", _tiny);
                }
            }

            Event e = Event.current;
            if (DeveloperMenuOpen || DeveloperInputBlockedThisFrame) return;
            if (e.type != EventType.KeyDown) return;
            if (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.W)
            {
                _menuChoice = (_menuChoice + 3) % 4;
                e.Use();
            }
            else if (e.keyCode == KeyCode.DownArrow || e.keyCode == KeyCode.S)
            {
                _menuChoice = (_menuChoice + 1) % 4;
                e.Use();
            }
            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                if (_menuChoice == 0) StartRun();
                else if (_menuChoice == 1) StartTutorial();
                else if (_menuChoice == 2) ToggleDeveloperMenu();
                else Application.Quit();
                e.Use();
            }
        }

        void DrawStatusStrip()
        {
            PanicModel model = World.Model;
            Rect bar = new Rect(24, 18, 1552, 72);
            Frame(bar, Night, Line, 1f);
            Label(new Rect(44, 26, 116, 24), "DALGA " + model.Wave.ToString("00"), ColorText(_body, Brass));
            Label(new Rect(166, 26, 24, 24), "◆", ColorText(_body, Ember));
            Label(new Rect(194, 26, 146, 24), DisasterName(model.Disaster), _body);
            Label(new Rect(354, 27, 116, 22), FormatTime(model.TimeRemaining), ColorText(_body,
                model.TimeRemaining < 60 ? Ember : MainText));
            Label(new Rect(483, 29, 112, 20), model.Developer.InfiniteBudget ? "Sınırsız ₺" : model.Budget.ToString("N0") + " ₺", _small);

            DrawStatusIcon(new Rect(620, 27, 26, 26), "water", model.PackedWater);
            DrawStatusIcon(new Rect(694, 27, 26, 26), "food", model.PackedFood);
            DrawBagIcon(new Rect(770, 26, 28, 28), model.BagReady ? Sea : SoftText);
            Label(new Rect(802, 28, 56, 22), model.BagReady ? "HAZIR" : "ÇANTA", ColorText(_tiny, model.BagReady ? Sea : SoftText));

            if (World.RadioEnabled)
            {
                Label(new Rect(870, 27, 86, 22), World.RadioSpeaking ? "RADYO · SES" : "RADYO · AÇIK",
                    ColorText(_tiny, World.RadioSpeaking ? Sea : Brass));
                Label(new Rect(962, 27, 332, 24), Clip(World.RadioHeadline, 42), _tiny);
            }
            else if (World.Model.Phase == PanicPhase.Preparing || World.Model.Phase == PanicPhase.Tutorial)
            {
                Label(new Rect(870, 27, 500, 24), World.Model.Phase == PanicPhase.Tutorial ? "HAZIRLIK" : "EVİ HAZIRLA", _tiny);
            }

            DrawKeycap(new Rect(1304, 25, 122, 34), "TAB", "ÇANTA", Line);
            if (model.Phase == PanicPhase.Preparing || model.Phase == PanicPhase.Tutorial)
                DrawKeycap(new Rect(1434, 25, 118, 34), "H", "YARDIM", Line);
        }

        void DrawStatusIcon(Rect rect, string id, int count)
        {
            Frame(rect, Raised, Line, 1f);
            DrawIcon(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6), id);
            Label(new Rect(rect.x + 30, rect.y - 1, 40, 28), "×" + count, _tiny);
        }

        void DrawBagIcon(Rect rect, Color color)
        {
            Fill(new Rect(rect.x + 5, rect.y + 7, rect.width - 10, rect.height - 8), color);
            Fill(new Rect(rect.x + 10, rect.y + 2, rect.width - 20, 3), color);
            Fill(new Rect(rect.x + 10, rect.y + 2, 3, 8), color);
            Fill(new Rect(rect.xMax - 13, rect.y + 2, 3, 8), color);
            Fill(new Rect(rect.x + 9, rect.y + 12, rect.width - 18, 3), Night);
        }

        void DrawContext()
        {
            if (!World.IsMoving && World.LootVisible && World.OpenSource != null) DrawLootStrip();

            string shortAction = World.IsMoving ? Clip(World.PlacementMessage, 100) : Clip(World.ShortAction, 32);
            if (!string.IsNullOrEmpty(shortAction))
                Label(new Rect(34, 788, 1532, 22), shortAction, ColorText(_small, World.IsMoving && !World.PlacementValid ? Brass : MainText));
            DrawControls(ContextControls());
        }

        void DrawRequirements()
        {
            PanicExpedition expedition = World.Expedition;
            IReadOnlyList<PanicMaterialRequirement> requirements = expedition.CurrentRequirements;
            int count = Mathf.Min(3, requirements.Count);
            bool budgetMissing = expedition.RequirementBudgetMissing > 0;
            Rect panel = new Rect(1050, 112, 516, 48 + count * 94 + (budgetMissing ? 48 : 0));
            Frame(panel, Night, Brass, 1.5f);
            Mark(panel); // Empty card space also belongs to the HUD, never world placement.
            Label(new Rect(panel.x + 18, panel.y + 9, panel.width - 36, 27), RequirementTitle(expedition.RequirementAction), ColorText(_small, Brass));
            for (int i = 0; i < count; i++)
            {
                PanicMaterialRequirement requirement = requirements[i];
                Rect row = new Rect(panel.x + 12, panel.y + 42 + i * 94, panel.width - 24, 88);
                Frame(row, Panel, requirement.Missing > 0 ? Line : Sea, 1f);
                DrawIcon(new Rect(row.x + 5, row.y + 4, 80, 80), requirement.Id);
                Label(new Rect(row.x + 99, row.y + 6, row.width - 108, 25),
                    SlotName(new SupplySlot(requirement.Id, 1)), ColorText(_body, MainText));
                Label(new Rect(row.x + 99, row.y + 34, 150, 25),
                    "Eksik " + requirement.Missing, ColorText(_body, requirement.Missing > 0 ? Brass : Sea));
                Label(new Rect(row.x + 99, row.y + 61, row.width - 108, 21),
                    "Var " + requirement.Owned + " / Gerek " + requirement.Required, _small);
            }
            if (budgetMissing)
            {
                float y = panel.y + 42 + count * 94;
                Label(new Rect(panel.x + 18, y + 3, panel.width - 36, 24),
                    "Bütçe: Eksik " + expedition.RequirementBudgetMissing + " ₺", ColorText(_body, Brass));
                Label(new Rect(panel.x + 18, y + 27, panel.width - 36, 21),
                    "Var " + expedition.RequirementBudgetOwned + " / Gerek " + expedition.RequirementBudgetCost + " ₺", _small);
            }
        }

        void DrawInventoryRequirements()
        {
            // The empty lower-right part of the bag panel keeps slots and all three actions usable.
            PanicExpedition expedition = World.Expedition;
            IReadOnlyList<PanicMaterialRequirement> requirements = expedition.CurrentRequirements;
            int count = Mathf.Min(3, requirements.Count);
            Rect panel = new Rect(792, 542, 560, 110);
            Frame(panel, Panel, Brass, 1f);
            Mark(panel);
            string title = expedition.RequirementBudgetMissing > 0 ?
                "Bütçe: Eksik " + expedition.RequirementBudgetMissing + " ₺ · Var " + expedition.RequirementBudgetOwned + " / Gerek " + expedition.RequirementBudgetCost :
                RequirementTitle(expedition.RequirementAction);
            Label(new Rect(panel.x + 10, panel.y + 3, panel.width - 20, 23), title, ColorText(_tiny, Brass));
            if (count == 0) return;
            float cellWidth = (panel.width - 16) / count;
            for (int i = 0; i < count; i++)
            {
                PanicMaterialRequirement requirement = requirements[i];
                float x = panel.x + 8 + i * cellWidth;
                float iconSize = count > 2 ? 52 : 70;
                DrawIcon(new Rect(x, panel.y + 31, iconSize, iconSize), requirement.Id);
                float textX = x + iconSize + 6;
                float textWidth = cellWidth - iconSize - 10;
                Label(new Rect(textX, panel.y + 27, textWidth, 24),
                    SlotName(new SupplySlot(requirement.Id, 1)), _tiny);
                Label(new Rect(textX, panel.y + 51, textWidth, 24),
                    "Eksik " + requirement.Missing, ColorText(_small, requirement.Missing > 0 ? Brass : Sea));
                Label(new Rect(textX, panel.y + 77, textWidth, 22),
                    "Var " + requirement.Owned + " / Gerek " + requirement.Required, _tiny);
            }
        }

        static string RequirementTitle(string action)
        {
            switch (action)
            {
                case "anchorwall": case "anchorfloor": return "SABİTLEME · EKSİKLER";
                case "upgrade": return "GÜÇLENDİRME · EKSİKLER";
                case "flood": return "SEL BARİYERİ · EKSİKLER";
                case "fire": return "YANGIN KORUMASI · EKSİKLER";
                case "plant": return "FİDAN · EKSİKLER";
                case "drain": return "DRENAJ · EKSİKLER";
                case "fence": return "SEL ÇİTİ · EKSİKLER";
                case "hose": return "HORTUM · EKSİKLER";
                case "craftfirstaid": return "İLK YARDIM · EKSİKLER";
                case "packbattery": return "ÇANTAYA KOYMAK İÇİN PİL GEREK";
                default: return "EKSİKLER";
            }
        }

        void DrawLootStrip()
        {
            LootSource source = World.OpenSource;
            if (source == null || source.Contents == null) return;
            int visible = 0;
            for (int i = 0; i < source.Contents.Count; i++)
                if (HasStock(source.Contents[i])) visible++;
            int count = Mathf.Min(10, visible);
            const float cellWidth = 110f;
            float width = count == 0 ? 590f : Mathf.Max(530f, count * (cellWidth + 8f) + 204f);
            float x = (CanvasWidth - width) * .5f;
            Rect panel = new Rect(x, 625, width, 150);
            Frame(panel, Night, Brass, 1f);
            Label(new Rect(x + 18, 634, width - 36, 24), Clip(source.Label, 36), ColorText(_small, Brass));
            float startX = x + 18;
            const float y = 668;
            if (count == 0)
            {
                // One explicit empty state, rather than a row of anonymous dash slots.
                Frame(new Rect(startX, y + 9, 68, 52), Panel, Line, 2f);
                Fill(new Rect(startX + 8, y + 3, 52, 8), Brass);
                Label(new Rect(startX + 84, y + 2, 280, 28), "BOŞ", ColorText(_heading, MainText));
                Label(new Rect(startX + 84, y + 38, 280, 24), "Malzemeler alındı", _small);
                Rect closeEmpty = new Rect(x + width - 112, y + 17, 94, 46);
                DrawKeycap(closeEmpty, "E", "KAPAT", Brass);
                Mark(closeEmpty);
                if (MousePressed(closeEmpty)) World.ToggleContainer();
                return;
            }
            int drawn = 0;
            for (int i = 0; i < source.Contents.Count && drawn < count; i++)
            {
                SupplySlot slot = source.Contents[i];
                if (!HasStock(slot)) continue;
                Rect cell = new Rect(startX + drawn * (cellWidth + 8f), y, cellWidth, 94);
                bool selected = i == World.LootIndex;
                Frame(cell, selected ? Raised : Panel, selected ? Sea : Line, selected ? 2f : 1f);
                DrawIcon(new Rect(cell.x + 22, cell.y + 3, 66, 64), slot.Id);
                Frame(new Rect(cell.xMax - 36, cell.y + 4, 31, 23), Night, selected ? Sea : Line, 1f);
                Label(new Rect(cell.xMax - 35, cell.y + 3, 30, 23), "×" + slot.Count, ColorText(_tiny, Brass));
                Label(new Rect(cell.x + 5, cell.y + 67, cell.width - 10, 22), SlotName(slot), ColorText(_tiny, selected ? Sea : MainText));
                Mark(cell);
                int index = i;
                if (MousePressed(cell)) World.SelectLoot(index);
                drawn++;
            }
            float actionsX = startX + count * (cellWidth + 8f) + 7f;
            Rect take = new Rect(actionsX, y + 2, 150, 40);
            Rect close = new Rect(actionsX, y + 50, 150, 40);
            DrawKeycap(take, "F", "AL", Sea);
            DrawKeycap(close, "E", "KAPAT", Line);
            Mark(take); Mark(close);
            if (MousePressed(take)) World.TakeLoot();
            if (MousePressed(close)) World.ToggleContainer();
        }

        static bool HasStock(SupplySlot slot)
        {
            return slot != null && slot.Count > 0 && !string.IsNullOrEmpty(slot.Id);
        }

        void DrawInventory()
        {
            Rect panel = new Rect(176, 96, 1248, 706);
            Frame(panel, Night, Brass, 1.5f);
            Label(new Rect(212, 118, 470, 34), "ENVANTER", ColorText(_heading, Brass));
            Label(new Rect(214, 158, 520, 24), "12 YUVA", _tiny);
            Label(new Rect(792, 158, 560, 24), "AFET ÇANTASI  ·  8 YUVA", _tiny);
            Fill(new Rect(768, 158, 1, 490), Line);

            List<SupplySlot> inventory = World.Expedition == null ? null : World.Expedition.Inventory;
            for (int i = 0; i < 12; i++)
            {
                int col = i % 4;
                int row = i / 4;
                Rect cell = new Rect(212 + col * 132, 198 + row * 116, 120, 104);
                SupplySlot slot = inventory != null && i < inventory.Count ? inventory[i] : null;
                bool selected = i == World.InventoryIndex;
                Frame(cell, selected ? Raised : Panel, selected ? Sea : Line, selected ? 2f : 1f);
                DrawSlot(cell, slot, selected);
                Mark(cell);
                int index = i;
                if (MousePressed(cell)) World.InventorySelect(index);
            }

            List<SupplySlot> bag = World.Expedition == null ? null : World.Expedition.Bag;
            for (int i = 0; i < 8; i++)
            {
                int col = i % 4;
                int row = i / 4;
                Rect cell = new Rect(792 + col * 138, 198 + row * 138, 126, 126);
                SupplySlot slot = bag != null && i < bag.Count ? bag[i] : null;
                Frame(cell, Panel, Line, 1f);
                DrawSlot(cell, slot, false);
                Mark(cell);
            }

            PanicModel model = World.Model;
            DrawStatusIcon(new Rect(796, 500, 28, 28), "water", model.PackedWater);
            Label(new Rect(830, 501, 115, 24), model.PackedWater + "/" + model.RequiredDays, _tiny);
            DrawStatusIcon(new Rect(952, 500, 28, 28), "food", model.PackedFood);
            Label(new Rect(986, 501, 115, 24), model.PackedFood + "/" + model.RequiredDays, _tiny);
            Label(new Rect(1128, 501, 205, 24), "DESTEK " + model.SupportCount + "/4", ColorText(_tiny, model.BagReady ? Sea : SoftText));

            SupplySlot selectedSlot = inventory != null && World.InventoryIndex < inventory.Count ? inventory[World.InventoryIndex] : null;
            string selectedName = SlotName(selectedSlot);
            Label(new Rect(212, 572, 520, 30), "SEÇİLİ  " + selectedName, ColorText(_body, MainText));
            DrawKeycap(new Rect(212, 666, 132, 42), "ENTER", "ÇANTAYA", Sea);
            DrawKeycap(new Rect(354, 666, 104, 42), "F", "ÜRET", Brass);
            DrawKeycap(new Rect(468, 666, 142, 42), "BACKSPACE", "GERİ", Line);
            Rect pack = new Rect(212, 666, 132, 42);
            Rect craft = new Rect(354, 666, 104, 42);
            Rect back = new Rect(468, 666, 142, 42);
            if (MousePressed(pack)) World.InventoryUse();
            if (MousePressed(craft)) World.CraftFirstAid();
            if (MousePressed(back)) World.ToggleInventory();
            Label(new Rect(792, 666, 560, 26), model.BagReady ? "ÇANTA HAZIR" : "SU · GIDA · DESTEK", ColorText(_tiny, model.BagReady ? Sea : Brass));
        }

        void DrawSlot(Rect cell, SupplySlot slot, bool selected)
        {
            if (slot == null || slot.Count <= 0 || string.IsNullOrEmpty(slot.Id)) return;
            DrawIcon(new Rect(cell.x + 9, cell.y + 8, cell.width - 18, cell.height - 34), slot.Id);
            Label(new Rect(cell.x + 5, cell.yMax - 25, cell.width - 10, 20), "×" + slot.Count, ColorText(_tiny, selected ? Sea : Brass));
        }

        void DrawWork()
        {
            PanicWorkSession work = World.Expedition.Work;
            Rect panel = new Rect(90, 72, 1420, 748);
            Frame(panel, Night, Brass, 1.5f);
            Rect image = new Rect(124, 108, 1010, 636);
            Fill(image, new Color(.025f, .035f, .04f, 1f));
            if (World.WorkTexture)
                GUI.DrawTexture(new Rect(image.x + 3, image.y + 3, image.width - 6, image.height - 6), World.WorkTexture, ScaleMode.ScaleToFit, false);
            Mark(image);
            Label(new Rect(1160, 110, 310, 30), Clip(work.Label.ToUpperInvariant(), 24), ColorText(_small, Brass));
            Label(new Rect(1160, 146, 280, 24), (work.CompletedSteps + 1) + " / " + work.TotalSteps + "  ·  " + work.StageName, ColorText(_small, MainText));
            for (int i = 0; i < work.TotalSteps; i++)
            {
                Rect step = new Rect(1160 + i * 68, 190, 54, 36);
                bool done = i < work.CompletedSteps;
                Frame(step, done ? new Color(.10f, .25f, .21f, 1f) : Panel, done ? Sea : i == work.CompletedSteps ? Brass : Line, 1f);
                Label(step, done ? "✓" : (i + 1).ToString(), ColorText(_center, done ? Sea : MainText));
            }

            string meter = !work.IsApplying ? "HEDEFLE" : work.Mode == PanicWorkMode.Timing ? "DOĞRU AN" : work.Mode == PanicWorkMode.Balance ? "BASINÇ" : "ÇEVİR";
            Label(new Rect(1160, 256, 274, 24), meter, ColorText(_tiny, SoftText));
            if (work.IsApplying && work.Mode == PanicWorkMode.Ratchet)
            {
                for (int i = 0; i < 3; i++)
                {
                    Rect turn = new Rect(1160 + i * 92, 294, 82, 60);
                    bool done = work.StageProgress >= (i + 1) * .25f;
                    bool current = !done && work.StageProgress >= i * .25f;
                    Frame(turn, done ? Raised : Panel, done ? Sea : current ? Brass : Line, current ? 2f : 1f);
                    int direction = (work.CompletedSteps + i) % 2 == 0 ? -1 : 1;
                    Label(turn, direction < 0 ? "A  ←" : "D  →", ColorText(_center, done ? Sea : current ? Brass : SoftText));
                }
                if (work.ExpectedDirection == 0)
                    Label(new Rect(1160, 364, 274, 28), "SON DOKUNUŞ", ColorText(_small, Sea));
            }
            else
            {
                Rect track = new Rect(1160, 302, 274, 34);
                Fill(track, new Color(.022f, .035f, .04f, 1f));
                float low = Mathf.Clamp01(work.Target - work.TargetWidth);
                float high = Mathf.Clamp01(work.Target + work.TargetWidth);
                Fill(new Rect(track.x + low * track.width, track.y, (high - low) * track.width, track.height), new Color(.20f, .52f, .39f, .95f));
                float targetX = track.x + work.Target * track.width;
                Fill(new Rect(targetX - 2, track.y - 4, 4, track.height + 8), Brass);
                float cursorX = track.x + Mathf.Clamp01(work.Cursor) * track.width;
                bool aligned = Mathf.Abs(work.Cursor - work.Target) <= work.TargetWidth;
                Fill(new Rect(cursorX - 5, track.y - 9, 10, track.height + 18), aligned ? Sea : MainText);
                if (work.IsApplying && work.Mode == PanicWorkMode.Timing)
                    Label(new Rect(1160, 354, 274, 28), aligned ? "ŞİMDİ!  SPACE" : "YEŞİLİ BEKLE", ColorText(_small, aligned ? Sea : SoftText));
                if (work.IsApplying && work.Mode == PanicWorkMode.Balance)
                {
                    DrawBar(new Rect(1160, 363, 274, 13), work.StageProgress, Sea);
                    Label(new Rect(1160, 386, 274, 24), "A / D dengele + Space", ColorText(_tiny, aligned ? Sea : Brass));
                }
            }

            bool aim = !work.IsApplying;
            DrawKeycap(new Rect(1158, 440, 130, 46), "A / D", aim ? "HEDEFLE" : work.Mode == PanicWorkMode.Ratchet ? "ÇEVİR" : "DENGELE", aim || work.Mode != PanicWorkMode.Timing ? Brass : Line);
            DrawKeycap(new Rect(1295, 440, 130, 46), "ENTER", "YERLEŞTİR", aim ? Sea : Line);
            string spaceAction = work.Mode == PanicWorkMode.Balance ? "BASILI TUT" : work.Mode == PanicWorkMode.Ratchet ? "OTURT" : "DOKUN";
            DrawKeycap(new Rect(1158, 500, 267, 48), "SPACE", spaceAction, work.IsApplying ? Sea : Line);
            Label(new Rect(1158, 572, 280, 58), work.StepHint, ColorText(_small, MainText));
            DrawBar(new Rect(1158, 652, 267, 9), work.Progress, Sea);
            DrawKeycap(new Rect(1158, 692, 267, 42), "ESC", "İPTAL", Line);
        }

        void DrawControls(string[][] controls)
        {
            float width = 147f;
            float gap = 8f;
            float total = controls.Length * width + Mathf.Max(0, controls.Length - 1) * gap;
            float x = (CanvasWidth - total) * .5f;
            for (int i = 0; i < controls.Length; i++)
            {
                string[] pair = controls[i];
                Rect key = new Rect(x + i * (width + gap), 822, width, 46);
                DrawKeycap(key, pair[0], pair[1], i == 0 ? Sea : Line);
                Mark(key);
            }
        }

        string[][] ContextControls()
        {
            if (World.WorkActive) return WorkControls;
            if (World.IsMoving) return MoveControls;
            if (World.InventoryOpen) return InventoryControls;
            if (World.LootVisible) return World.OpenSource!=null&&World.OpenSource.Contents.Exists(HasStock)?LootControls:EmptyLootControls;
            if (World.Model.Phase == PanicPhase.Inspection) return InspectControls;
            PanicItem selected = World.Selected;
            bool prepare = World.Model.Phase == PanicPhase.Preparing || World.Model.Phase == PanicPhase.Tutorial;
            return prepare && selected != null && selected.Id != "bunker" && selected.Id != "bagstation" ? PrepareControls : WalkControls;
        }

        void DrawHelpCard()
        {
            Rect panel = new Rect(34, 638, 390, 142);
            Frame(panel, Night, Sea, 1f);
            Label(new Rect(52, 648, 345, 24), "KONTROLLER", ColorText(_tiny, Brass));
            Label(new Rect(52, 676, 346, 24), "WASD Yürü   ·   Space Zıpla", _tiny);
            Label(new Rect(52, 700, 346, 24), "E Aç   ·   F Al   ·   Tab Çanta", _tiny);
            Label(new Rect(52, 724, 346, 24), "H Kapat", ColorText(_tiny, Sea));
        }

        void DrawDisasterCue()
        {
            Rect cue = new Rect(574, 116, 452, 42);
            Frame(cue, new Color(.22f, .055f, .045f, .94f), Ember, 1f);
            Label(new Rect(cue.x + 12, cue.y + 3, cue.width - 24, cue.height - 6), DisasterName(World.Model.Disaster), ColorText(_center, Ember));
        }

        void DrawToast(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            Rect toast = new Rect(518, 612, 564, 42);
            Frame(toast, Panel, Brass, 1f);
            Label(new Rect(toast.x + 14, toast.y + 6, toast.width - 28, 28), Clip(message, 56), _small);
        }

        void DrawLossReveal()
        {
            Fill(new Rect(0, 0, CanvasWidth, CanvasHeight), new Color(.13f, .006f, .008f, .94f));
            Fill(new Rect(0, 0, CanvasWidth, 8), Ember);
            Fill(new Rect(0, CanvasHeight - 8, CanvasWidth, 8), Ember);
            Label(new Rect(180, 348, 1240, 120), "SIĞINAĞA YETİŞEMEDİN", ColorText(_title, Color.white));
            Label(new Rect(180, 482, 1240, 34), "KOŞU BİTTİ", ColorText(_small, new Color(1f, .74f, .71f)));
        }

        void DrawCinematic()
        {
            Fill(new Rect(0, 0, CanvasWidth, 150), new Color(.005f, .008f, .012f, .93f));
            Fill(new Rect(0, CanvasHeight - 160, CanvasWidth, 160), new Color(.005f, .008f, .012f, .93f));
            string title = string.IsNullOrEmpty(World.CinematicTitle) ? "KURTARMA" : Clip(World.CinematicTitle, 70);
            Label(new Rect(90, CanvasHeight - 116, 1420, 50), title, ColorText(_heading, MainText));
            DrawBar(new Rect(90, CanvasHeight - 42, 1420, 3), World.CinematicProgress, Brass);
        }

        void DrawInspection()
        {
            BuildInspectionList();
            Rect panel = new Rect(34, 720, 500, 82);
            Frame(panel, Night, Brass, 1f);
            int total = Mathf.Max(1, _inspectionItems.Count);
            int step = Mathf.Clamp(_inspectionStep, 0, total - 1);
            string target = _inspectionItems.Count > 0 ? Clip(_inspectionItems[step].Label, 26) : "EV";
            Label(new Rect(52, 730, 460, 24), "HASAR " + Mathf.Min(step + 1, total) + "/" + total + "   " + target, ColorText(_small, Brass));
            Label(new Rect(52, 758, 460, 22), "HASAR " + World.Model.TotalDamage.ToString("0.#") + "%", _tiny);
            DrawControls(ContextControls());
            HandleInspectionEvent();
        }

        void BuildInspectionList()
        {
            _inspectionItems.Clear();
            List<PanicItem> source = World.Model.Items;
            if (source == null) return;
            for (int i = 0; i < source.Count; i++)
            {
                PanicItem candidate = source[i];
                if (candidate == null || candidate.Contribution <= .1f) continue;
                int at = _inspectionItems.Count;
                while (at > 0 && candidate.Contribution > _inspectionItems[at - 1].Contribution) at--;
                _inspectionItems.Insert(at, candidate);
                if (_inspectionItems.Count > 4) _inspectionItems.RemoveAt(_inspectionItems.Count - 1);
            }
            if (_inspectionItems.Count == 0) return;
            _inspectionStep = Mathf.Clamp(_inspectionStep, 0, _inspectionItems.Count - 1);
            string nextFocus = _inspectionItems[_inspectionStep].Id;
            if (_inspectionFocusId != nextFocus)
            {
                World.SetFocus(nextFocus);
                _inspectionFocusId = nextFocus;
            }
        }

        void HandleInspectionEvent()
        {
            if (DeveloperMenuOpen || DeveloperInputBlockedThisFrame) return;
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return;
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                if (_inspectionStep + 1 < _inspectionItems.Count) _inspectionStep++;
                else World.FinishInspection();
                e.Use();
            }
            else if (e.keyCode == KeyCode.N)
            {
                World.FinishInspection();
                e.Use();
            }
        }

        void DrawPause()
        {
            Fill(new Rect(0, 0, CanvasWidth, CanvasHeight), new Color(.015f, .025f, .03f, .58f));
            Rect panel = new Rect(500, 210, 600, 480);
            Frame(panel, Night, Brass, 1.5f);
            Mark(panel);
            Label(new Rect(548, 244, 500, 50), "DURAKLATILDI", _title);
            Button(new Rect(548, 322, 500, 54), "Devam et", true, Sea, () => SetPaused(false));
            Button(new Rect(548, 390, 500, 54), "Geliştirici · F2", false, Brass, ToggleDeveloperMenu);
            Button(new Rect(548, 458, 500, 54), "Yeniden başla", false, Brass, RestartRun);
            Button(new Rect(548, 526, 500, 54), "Başlık", false, SoftText, ReturnToMenu);
            DrawKeycap(new Rect(548, 604, 132, 34), "ESC", "GERİ", Line);
        }

        bool DeveloperBudgetAvailable()
        {
            if (!World || World.Model == null) return false;
            PanicPhase phase = World.Model.Phase;
            return phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial || phase == PanicPhase.Inspection;
        }

        void DrawDeveloperMenu()
        {
            Fill(new Rect(0, 0, CanvasWidth, CanvasHeight), new Color(.012f, .022f, .028f, .84f));
            Mark(new Rect(0, 0, CanvasWidth, CanvasHeight));
            Rect panel = new Rect(350, 124, 900, 652);
            Frame(panel, Night, Brass, 1.5f);
            Label(new Rect(388, 148, 640, 44), "GELİŞTİRİCİ", ColorText(_title, Brass));
            Label(new Rect(388, 199, 820, 28), "Seçenekleri tek tek açıp kapatabilirsin.", _body);
            string state = World.Model.Phase == PanicPhase.Title
                ? "Seçimler sonraki koşuya uygulanır. Menüyü açmak skoru etkilemez."
                : "Oyun duraklatıldı. Bir hileyi açarsan bu koşunun skoru kaydedilmez.";
            Label(new Rect(388, 232, 820, 26), state, _small);

            DrawDeveloperToggle(new Rect(388, 278, 824, 80), PanicDeveloperOption.InfiniteBudget,
                "Sınırsız bütçe", "İş ve yükseltmeler bütçeyi azaltmaz.", World.Model.Developer.InfiniteBudget, 0);
            DrawDeveloperToggle(new Rect(388, 368, 824, 80), PanicDeveloperOption.FreeMaterials,
                "Malzeme harcama", "Gerekli malzemeler aranmaz ve tüketilmez.", World.Model.Developer.FreeMaterials, 1);
            DrawDeveloperToggle(new Rect(388, 458, 824, 80), PanicDeveloperOption.FreezePreparationTime,
                "Hazırlık süresini durdur", "Hazırlık sayacı ilerlemez.", World.Model.Developer.FreezePreparationTime, 2);

            bool canAdd = DeveloperBudgetAvailable();
            Rect budget = new Rect(388, 558, 394, 52);
            if (canAdd)
                Button(budget, "+1.000 ₺ bütçe", _developerChoice == 3, Sea, () => World.Model.AddDeveloperBudget(1000));
            else
            {
                Frame(budget, Panel, _developerChoice == 3 ? Brass : Line, 1f);
                Mark(budget);
                Label(budget, "+1.000 ₺ · Koşuda kullanılabilir", ColorText(_center, SoftText));
            }
            string activeState = World.Model.DeveloperAnyEnabled ? "HİLE SEÇENEĞİ AÇIK" : "TÜM SEÇENEKLER KAPALI";
            Label(new Rect(808, 562, 404, 42), activeState, ColorText(_tiny, World.Model.DeveloperAnyEnabled ? Brass : SoftText));
            Label(new Rect(388, 628, 824, 26), World.Model.CheatedRun
                ? "HİLELİ KOŞU · Seçenekleri kapatsan da skor kaydedilmez."
                : "Bir hileyi açmak veya bütçe eklemek koşuyu hileli işaretler.", ColorText(_small, World.Model.CheatedRun ? Brass : SoftText));
            Button(new Rect(918, 686, 294, 48), "Kapat · F2 / Esc", _developerChoice == 4, Sea, () => SetDeveloperMenuOpen(false));
            DrawKeycap(new Rect(388, 690, 190, 38), "↑ ↓", "SEÇ", Line);
            DrawKeycap(new Rect(590, 690, 250, 38), "ENTER / SPACE", "DEĞİŞTİR", Line);
            HandleDeveloperMenuEvent();
        }

        void DrawDeveloperToggle(Rect rect, PanicDeveloperOption option, string title, string description, bool enabled, int choice)
        {
            bool selected = _developerChoice == choice;
            Frame(rect, enabled ? new Color(.08f, .15f, .14f, .98f) : Panel, selected ? Brass : enabled ? Sea : Line, selected ? 2f : 1f);
            Mark(rect);
            Label(new Rect(rect.x + 18, rect.y + 10, rect.width - 170, 30), title, ColorText(_body, enabled ? Sea : MainText));
            Label(new Rect(rect.x + 18, rect.y + 43, rect.width - 170, 25), description, _small);
            Rect toggle = new Rect(rect.xMax - 124, rect.y + 20, 104, 40);
            Frame(toggle, enabled ? new Color(.14f, .30f, .24f, 1f) : Raised, enabled ? Sea : Line, 1f);
            Label(toggle, enabled ? "AÇIK" : "KAPALI", ColorText(_center, enabled ? Sea : SoftText));
            if (MousePressed(rect))
            {
                _developerChoice = choice;
                World.Model.SetDeveloperOption(option, !enabled);
            }
        }

        void HandleDeveloperMenuEvent()
        {
            if (!_developerMenuOpen || DeveloperInputBlockedThisFrame) return;
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return;
            if (e.keyCode == KeyCode.UpArrow)
            {
                _developerChoice = (_developerChoice + 4) % 5;
                e.Use();
            }
            else if (e.keyCode == KeyCode.DownArrow)
            {
                _developerChoice = (_developerChoice + 1) % 5;
                e.Use();
            }
            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Space)
            {
                if (_developerChoice == 0) World.Model.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, !World.Model.Developer.InfiniteBudget);
                else if (_developerChoice == 1) World.Model.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, !World.Model.Developer.FreeMaterials);
                else if (_developerChoice == 2) World.Model.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, !World.Model.Developer.FreezePreparationTime);
                else if (_developerChoice == 3 && DeveloperBudgetAvailable()) World.Model.AddDeveloperBudget(1000);
                else if (_developerChoice == 4) SetDeveloperMenuOpen(false);
                e.Use();
            }
        }

        void DrawCheatedRunIndicator()
        {
            Rect badge = new Rect(24, 875, 524, 22);
            Frame(badge, Night, Brass, 1f);
            Label(badge, "HİLELİ KOŞU · Skor kaydedilmez", ColorText(_tiny, Brass));
        }

        void DrawEndCard()
        {
            Fill(new Rect(0, 0, CanvasWidth, CanvasHeight), new Color(.015f, .025f, .03f, .66f));
            bool won = World.Model.Phase == PanicPhase.Success;
            Rect panel = new Rect(390, 248, 820, 404);
            Frame(panel, Night, won ? Sea : Ember, 1.5f);
            Label(new Rect(444, 292, 720, 42), won ? "EV GÜVENDE" : "KOŞU BİTTİ", ColorText(_heading, won ? Sea : Ember));
            Label(new Rect(444, 348, 720, 36), "DALGA " + World.Model.CompletedWaves + "   ·   PUAN " + World.Model.Score.ToString("N0"), _body);
            Button(new Rect(444, 430, 330, 54), "Yeniden başla", true, Sea, RestartRun);
            Button(new Rect(792, 430, 330, 54), "Başlık", false, Brass, ReturnToMenu);
        }

        void StartRun()
        {
            if (_paused) SetPaused(false);
            _helpVisible = false;
            _inspectionStep = 0;
            World.StartGame(false);
        }

        void StartTutorial()
        {
            if (_paused) SetPaused(false);
            _helpVisible = true;
            _inspectionStep = 0;
            World.StartGame(true);
        }

        void RestartRun()
        {
            if (_paused) SetPaused(false);
            _helpVisible = false;
            _inspectionStep = 0;
            World.StartGame(false);
        }

        void ReturnToMenu()
        {
            if (_paused) SetPaused(false);
            _helpVisible = false;
            World.ReturnToMenu();
        }

        void Button(Rect rect, string text, bool selected, Color accent, Action action)
        {
            Frame(rect, selected ? new Color(.11f, .17f, .17f, .98f) : Raised,
                selected ? accent : Line, selected ? 1.6f : 1f);
            Label(rect, text, ColorText(_center, selected ? accent : MainText));
            Mark(rect);
            if (!_developerMenuOpen && DeveloperInputBlockedThisFrame) return;
            if (MousePressed(rect) && action != null) action();
        }

        void DrawKeycap(Rect rect, string key, string action, Color accent)
        {
            Frame(rect, Panel, accent, 1f);
            float keyWidth = Mathf.Max(_caps.CalcSize(new GUIContent(key)).x + 10f,key.Length*9f+12f);
            float actionWidth = _caps.CalcSize(new GUIContent(action)).x + 10f;
            var keyColor=accent.a<.5f?SoftText:new Color(accent.r,accent.g,accent.b,1f);
            if(keyWidth+actionWidth+10f>rect.width)
            {
                Label(new Rect(rect.x+3,rect.y+1,rect.width-6,rect.height*.5f),key,ColorText(_tiny,keyColor));
                Label(new Rect(rect.x+3,rect.y+rect.height*.5f-1,rect.width-6,rect.height*.5f),action,_caps);
            }
            else
            {
                Label(new Rect(rect.x + 4, rect.y + 1, keyWidth, rect.height - 2), key, ColorText(_caps, keyColor));
                Label(new Rect(rect.x + keyWidth + 5, rect.y + 1, rect.width - keyWidth - 9, rect.height - 2), action, _caps);
            }
        }

        void DrawIcon(Rect rect, string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            Texture2D icon = PanicResidenceArt.Icon(id);
            if (icon) GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
        }

        void DrawDot(Rect rect, Color color)
        {
            if (!_dotTexture)
            {
                const int size = 16;
                _dotTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
                _dotTexture.name = "Home V3 HUD connection dot";
                _dotTexture.hideFlags = HideFlags.HideAndDontSave;
                Color[] pixels = new Color[size * size];
                float center = (size - 1) * .5f;
                float radius = size * .44f;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - center;
                        float dy = y - center;
                        pixels[y * size + x] = dx * dx + dy * dy <= radius * radius ? Color.white : Color.clear;
                    }
                _dotTexture.SetPixels(pixels);
                _dotTexture.Apply(false, true);
            }
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _dotTexture, ScaleMode.StretchToFill, true);
            GUI.color = old;
        }

        void DrawBar(Rect rect, float value, Color color)
        {
            Fill(rect, new Color(.02f, .035f, .04f, 1f));
            Fill(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(value), rect.height), color);
        }

        bool MousePressed(Rect rect)
        {
            Event e = Event.current;
            Vector2 mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            Vector2 point = (mouse - _offset) / _scale;
            if (e.type != EventType.MouseDown || e.button != 0 || !rect.Contains(point)) return false;
            e.Use();
            return true;
        }

        void Label(Rect rect, string text, GUIStyle style)
        {
            Mark(rect);
            GUI.Label(rect, text ?? "", style);
        }

        void Mark(Rect rect) { _uiRects.Add(rect); }

        static void Fill(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        static void Frame(Rect rect, Color fill, Color edge, float thickness)
        {
            Fill(rect, fill);
            Fill(new Rect(rect.x, rect.y, rect.width, thickness), edge);
            Fill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), edge);
            Fill(new Rect(rect.x, rect.y, thickness, rect.height), edge);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), edge);
        }

        GUIStyle ColorText(GUIStyle source, Color color)
        {
            Dictionary<Color, GUIStyle> variants;
            if (!_coloredStyles.TryGetValue(source, out variants))
            {
                variants = new Dictionary<Color, GUIStyle>();
                _coloredStyles[source] = variants;
            }
            GUIStyle style;
            if (variants.TryGetValue(color, out style)) return style;
            style = new GUIStyle(source);
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            variants[color] = style;
            return style;
        }

        static string SlotName(SupplySlot slot)
        {
            if (slot == null || slot.Count <= 0 || string.IsNullOrEmpty(slot.Id)) return "Boş";
            switch (slot.Id)
            {
                case "water": return "Su";
                case "food": return "Gıda";
                case "bandage": return "Bandaj";
                case "antiseptic": return "Antiseptik";
                case "firstaid": return "İlk yardım";
                case "flashlight": return "Fener";
                case "battery": return "Pil";
                case "radio": return "Radyo";
                case "blanket": return "Battaniye";
                case "screws": return "Vida";
                case "bracket": return "Ankraj";
                case "plank": return "Tahta";
                case "sandbag": return "Kum torbası";
                case "seedling": return "Fidan";
                case "hose": return "Hortum";
                case "extinguisher": return "Söndürücü";
                default: return Clip(slot.Id, 14);
            }
        }

        static string Clip(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            value = value.Replace('\n', ' ').Replace('\r', ' ');
            return value.Length <= max ? value : value.Substring(0, Mathf.Max(1, max - 1)) + "…";
        }

        static string FormatTime(float value)
        {
            int seconds = Mathf.Max(0, Mathf.CeilToInt(value));
            return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        }

        static string DisasterName(DisasterType type)
        {
            switch (type)
            {
                case DisasterType.Earthquake: return "DEPREM";
                case DisasterType.Flood: return "SEL";
                case DisasterType.Fire: return "YANGIN";
                default: return "AFET";
            }
        }
    }
}
