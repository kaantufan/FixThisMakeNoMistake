using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Compact Turkish HUD. World owns gameplay input; this class draws it and owns only UI keys.</summary>
    public sealed partial class PanicHomeHud : MonoBehaviour
    {
        const float CanvasWidth = 1600f;
        const float CanvasHeight = 900f;
        public string ResultFailureTextV56 { get; private set; } = string.Empty;
        public Rect ResultFailureRectV56 { get; private set; }

        // Offline, authored palette from the approved HUD concept.
        static readonly Color Shade = new Color(.055f, .065f, .06f, .65f);
        static readonly Color Night = new Color(.075f, .094f, .092f, .98f);
        static readonly Color Panel = new Color(.12f, .14f, .13f, .98f);
        static readonly Color Raised = new Color(.17f, .20f, .18f, .98f);
        static readonly Color MainText = new Color(.96f, .95f, .91f, 1f);
        static readonly Color SoftText = new Color(.82f, .85f, .81f, 1f);
        static readonly Color Muted = new Color(.78f, .82f, .77f, 1f);
        static readonly Color Brass = new Color(.95f, .75f, .36f, 1f);
        static readonly Color Sea = new Color(.63f, .85f, .76f, 1f);
        static readonly Color Petrol = new Color(.18f, .33f, .29f, 1f);
        static readonly Color Rust = new Color(.96f, .72f, .60f, 1f);
        static readonly Color Ember = new Color(.99f, .65f, .52f, 1f);
        static readonly Color Line = new Color(.70f, .77f, .71f, .32f);
        static readonly string[][] WalkControls = { new[] { "WASD", "YÜRÜ" }, new[] { "SHIFT", "KOŞ" }, new[] { "SPACE", "ZIPLA" }, new[] { "E", "KULLAN" }, new[] { "TAB", "ÇANTA" } };
        static readonly string[][] LootControls = { new[] { "Z/X", "SEÇ" }, new[] { "F", "AL" }, new[] { "E", "KAPAT" }, new[] { "TAB", "ÇANTA" } };
        static readonly string[][] EmptyLootControls = { new[] { "E", "KAPAT" }, new[] { "TAB", "ÇANTA" },new[] { "Q", "HEDEF" } };
        static readonly string[][] InventoryControls = { new[] { "OKLAR", "SEÇ" }, new[] { "ENTER", "ÇANTAYA" }, new[] { "BACKSPACE", "GERİ" } };
        static readonly string[][] WorkControls = { new[] { "SOL TIK", "VİDAYI TAK" }, new[] { "ESC", "İPTAL" } };
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
                    !World.WorkActive && World.CanInteract &&
                    World.Expedition.RequirementTargetId == World.SelectedTargetIdV56 &&
                    !SelectedTargetHudVisibleV56 && World.Expedition.HasMissingRequirements;
            }
        }

        public bool PointerOverUi
        {
            get
            {
                if (FreeplayOnboardingInputBlockedV414 || DeveloperMenuOpen || DeveloperInputBlockedThisFrame) return true;
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
        int _pauseChoice;
        int _hudProcessedFrame = -1;
        public bool Paused { get { return _paused; } }
        int _inspectionStep;
        string _inspectionFocusId;
        PanicPhase _lastPhase = (PanicPhase)(-1);
        float _scale = 1f;
        Vector2 _offset;
        bool _stylesReady;
        GUIStyle _title;
        GUIStyle _heading;
        GUIStyle _body;
        GUIStyle _story;
        GUIStyle _small;
        GUIStyle _tiny;
        GUIStyle _center;
        GUIStyle _caps;
        Texture2D _dotTexture;
        GUIStyle _display;
        GUIStyle _timer;
        GUIStyle _mega;
        GUIStyle _logo;
        Texture2D _brandLogo;
        GUIStyle _wrap;
        GUIStyle _right;
        Font _displayFont;
        Font _condensedFont;
        Font _bodyFont;
        int _timerWave = -1;
        PanicPhase _timerPhase = (PanicPhase)(-1);
        bool _timerPrecautionStage;
        float _timerMaximum = 1f;
        PanicHudSupplyPreview _supplyPreviews;
        GUIStyle _fittedStyle;
        readonly List<Rect> _topLevelRegions = new List<Rect>(16);

        public string ThemeVersion { get { return "V5.1 / Compact keyboard HUD"; } }
        bool CampaignPrecautionTimer { get { return World && World.Model != null && !World.JuryActive &&
            !World.Model.IsJuryDemo && !World.Model.IsSimulation && !World.Model.IsTutorial &&
            World.Model.Phase == PanicPhase.Preparing && World.Model.TimeRemaining > PanicHomeWorld.BagPreparationSeconds; } }
        float PreparationTimerRemaining { get { return CampaignPrecautionTimer ?
            Mathf.Max(0f, World.Model.TimeRemaining - PanicHomeWorld.BagPreparationSeconds) : World.Model.TimeRemaining; } }
        public bool TimerAlarmActive { get { return World && !World.JuryActive && World.Model != null && !World.Model.IsSimulation &&
            (World.Model.Phase == PanicPhase.Preparing || World.Model.Phase == PanicPhase.Tutorial) &&
            PreparationTimerRemaining <= 60f; } }
        public Rect LogicalCanvasBounds { get { return new Rect(0, 0, CanvasWidth, CanvasHeight); } }
        public Rect WaveCardBounds { get { return new Rect(24, 20, 430, 76); } }
        public Rect TimerCardBounds { get { return new Rect(660, 20, 208, 76); } }
        public Rect ResourceCardBounds { get { return new Rect(1332, 20, 244, 76); } }
        public Rect RequirementCardBounds { get { int n = World && World.Expedition != null ?
            Mathf.Min(3, World.Expedition.CurrentRequirements.Count) : 0;
            return new Rect(1220, 106, 356, 46 + n * 29 +
                (World && World.Expedition != null && World.Expedition.RequirementBudgetMissing > 0 ? 30 : 0)); } }
        public float CanvasScale { get { return _scale; } }
        public Vector2 CanvasOffset { get { return _offset; } }
        public int SupplyPreviewCount { get { return _supplyPreviews == null ? 0 : _supplyPreviews.CachedPreviewCount; } }
        public int SupplyPreviewRenderCount { get { return _supplyPreviews == null ? 0 : _supplyPreviews.RenderCount; } }
        public IReadOnlyList<Rect> VisibleHudRegions { get { return _topLevelRegions; } }


        bool _developerMenuOpen;
        bool _developerPriorPaused;
        bool _developerPriorHelp;
        int _developerTransitionFrame = -1;
        int _developerHotkeyFrame = -1;
        int _developerChoice;
        string _developerNotice;
        float _developerNoticeUntil;

        public bool DeveloperMenuOpen { get { return _developerMenuOpen; } }
        public bool DeveloperInputBlockedThisFrame { get { return _developerTransitionFrame == Time.frameCount || compactControlsInputFrameV5 == Time.frameCount || WaveLedgerInputBlockedV56 || KeyboardHelpBlockingV56; } }

        /// <summary>World calls this before ticking/input. HUD calls the same idempotent path.</summary>
        public void ProcessDeveloperHotkeys()
        {
            if (!World || World.Model == null || _developerHotkeyFrame == Time.frameCount) return;
            _developerHotkeyFrame = Time.frameCount;
            if (ProcessKeyboardHelpInputV56()) return;
            SynchronizeHudPhase();
            if (ProcessWaveLedgerInputV56()) return;
            ProcessCompactControlsInputV5();
            if (ProcessFreeplayOnboardingHotkeysV414()) return;
            if (Input.GetKeyDown(KeyCode.F3)) { RequestDeveloperWaveSkipV51(); return; }
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
            if (value && KeyboardHelpBlockingV56) return;
            if (value == _developerMenuOpen) return;
            if (value && FreeplayOnboardingInputBlockedV414) return;
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
                // Cinema owns its movement gate through CinematicActive/LossAnnounced.
                // Do not freeze the model/cinema clock with the HUD pause lock.
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
            SynchronizeFreeplayOnboardingPhaseV414(phase);
            if (_developerMenuOpen) SetDeveloperMenuOpen(false);
            if (_paused) SetPaused(false);
            // World already blocks movement for cinema. InputLocked is only a pause/UI lock.
            _inspectionStep = 0;
            _inspectionFocusId = null;
            _helpVisible = phase == PanicPhase.Tutorial && World.Model.IsTutorial;
            if (phase == PanicPhase.Title) SetGuidanceExpandedV513(false);
            _lastPhase = phase;
        }

        void Update() { ProcessFrameHotkeys(); TickPresentationV513(); }

        public void ProcessFrameHotkeys()
        {
            if (_hudProcessedFrame == Time.frameCount) return;
            if (!World || World.Model == null) return;
            _hudProcessedFrame = Time.frameCount;
            ProcessDeveloperHotkeys();
            if (KeyboardHelpBlockingV56) return;
            if (WaveLedgerInputBlockedV56) return;
            if (FreeplayOnboardingInputBlockedV414) return;
            World.ProcessNotebookInput();
            if (World.NotebookBlocksInput) return;
            if (!World.InventoryOpen) CancelInventoryDrag();
            if (DeveloperMenuOpen || DeveloperInputBlockedThisFrame) return;

            // A same-frame work cancel belongs to World. Do not reinterpret that Escape as Pause.
            if (World.CancelFrame == Time.frameCount) return;

            PanicPhase phase = World.Model.Phase;
            SynchronizeHudPhase();

            if (World.WorkActive || World.IsMoving) return; // World owns work and placement keys, including cancel.
            if (World.InventoryOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) World.ToggleInventory();
                return; // World owns keyboard selection and Enter packing.
            }

            if (_helpVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                _helpVisible = false;
                return;
            }
            if (_paused)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (AudioSettingsOpenV56) CloseAudioSettingsV56();
                    else SetPaused(false);
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.H) && (phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial))
                _helpVisible = !_helpVisible;

            bool canPause = phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial || phase == PanicPhase.Inspection || phase == PanicPhase.Disaster || phase == PanicPhase.Aftermath;
            if (canPause && Input.GetKeyDown(KeyCode.Escape)) SetPaused(true);
        }

        public void SetPaused(bool value)
        {
            if (value && KeyboardHelpBlockingV56) return;
            if (!World) return;
            if (!value) CloseAudioSettingsV56();
            if (value && !_paused)
            {
                _pauseChoice = 0;
                _priorInputLock = World.InputLocked;
                World.InputLocked = true;
            }
            else if (!value && _paused)
            {
                World.InputLocked = _priorInputLock;
            }
            _paused = value;
            if (World.Model != null) World.Model.SetPaused(value);
            CancelInventoryDrag();
        }

        void OnGUI()
        {
            if (!World || World.Model == null || World.NotebookBlocksInput) return;
            SynchronizeWaveLedgerV56();
            PrepareCanvas();
            EnsureStyles();
            _uiRects.Clear();
            _topLevelRegions.Clear();
            DrawFpsVeilV414();

            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            Color oldContent = GUI.contentColor;
            GUI.matrix = Matrix4x4.TRS(new Vector3(_offset.x, _offset.y, 0f), Quaternion.identity,
                new Vector3(_scale, _scale, 1f));
            GUI.color = Color.white;
            GUI.contentColor = Color.white;

            if (KeyboardHelpVisibleV56)
            {
                DrawStatusStrip(); DrawKeyboardHelpV56();
            }
            else if (WaveLedgerVisibleV56)
            {
                DrawStatusStrip();
                DrawWaveLedgerV56();
            }
            else if (FreeplayOnboardingOpenV414)
            {
                DrawFreeplayOnboardingV414();
            }
            else if (_paused && !_developerMenuOpen)
            {
                DrawStatusStrip(); DrawPause();
            }
            else if (World.LossAnnounced && World.LossRevealTime > 0f)
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
            else if (World.WorkActive)
            {
                DrawStatusStrip();
                DrawWork();
            }
            else if (World.InventoryOpen)
            {
                DrawStatusStrip();
                DrawInventory();
                if (RequirementsCardVisible) DrawInventoryRequirements();
            }
            else if (World.BagCameraOwnsView)
            {
                DrawFinalBagHud();
                if (World.NotificationVisible) DrawToast(World.ShortNotice);
            }
            else if (World.JuryActive)
            {
                DrawStatusStrip();
                DrawCompactJuryPanelV5();
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
                DrawSimulationActions();
                DrawTutorialActions();
                if (_helpVisible && !World.FirstWaveOnboardingRequiredV510 && !World.LootVisible && (World.Model.Phase == PanicPhase.Tutorial || World.Model.Phase == PanicPhase.Preparing))
                    DrawHelpCard();
                if (World.Model.Phase == PanicPhase.Disaster) DrawDisasterCue();
                if (RequirementsCardVisible) DrawRequirements();
                if (World.NotificationVisible) DrawToast(World.ShortNotice);
            }

            if (!WaveLedgerInputBlockedV56 && !KeyboardHelpBlockingV56)
            {
                DrawGuidanceToggleV513();
                DrawCriticalTasksV513();
                DrawFirstWaveOnboardingV510();
                DrawSelectedTargetHudV56();
                DrawAssistantHudV55();
                DrawNotebookCursorV59();
                DrawSecondaryAlertV55();
                DrawCompactControlsV5();
                if (!FreeplayOnboardingOpenV414 && !World.CinematicActive) DrawPresentationSubtitle();
                if (World.Model.CheatedRun) DrawCheatedRunIndicator();
                if (!FreeplayOnboardingOpenV414 && !_developerMenuOpen && !string.IsNullOrEmpty(_developerNotice) && Time.unscaledTime < _developerNoticeUntil)
                {
                    Rect notice = new Rect(480, 132, 840, 30);
                    Frame(notice, Night, Brass, 1f);
                    Mark(notice);
                    Label(new Rect(notice.x + 14, notice.y + 3, notice.width - 28, 24), _developerNotice, _small);
                }
            }

            // Single final blackout owner; the full opaque interval also covers HUD cards.
            InteractionAvoidRectV513 = CriticalCardBoundsV513;
            if (GuidanceToggleVisibleV513)
                InteractionAvoidRectV513 = Rect.MinMaxRect(24f, 116f, 324f, Mathf.Max(GuidanceToggleBoundsV513.yMax, CriticalCardBoundsV513.yMax));
            if (FirstWaveCardVisibleV510)
                InteractionAvoidRectV513 = Rect.MinMaxRect(24f, 116f, 324f, Mathf.Max(CriticalCardBoundsV513.yMax, FirstWaveCardBoundsV510.yMax));
            DrawPresentationV513();
            World.DrawBagBlackoutV51();
            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
            GUI.contentColor = oldContent;
        }

        void OnDestroy()
        {
            ResetKeyboardHelpPresentationV56();
            if (_dotTexture) Destroy(_dotTexture);
            _dotTexture = null;
            if (_supplyPreviews != null) _supplyPreviews.Dispose();
            _supplyPreviews = null;
            PanicUiSymbolsV57.Release();
        }

        void OnDisable()
        {
            ResetKeyboardHelpPresentationV56();
            if (_developerMenuOpen) SetDeveloperMenuOpen(false);
            if (WaveLedgerVisibleV56) CloseWaveLedgerV56();
        }

        void PrepareCanvas()
        {
            float sx = Screen.width / CanvasWidth;
            float sy = Screen.height / CanvasHeight;
            _scale = Mathf.Max(.01f, Mathf.Min(sx, sy));
            _offset = new Vector2((Screen.width - CanvasWidth * _scale) * .5f,
                (Screen.height - CanvasHeight * _scale) * .5f);
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!fallback) fallback = GUI.skin.font;
            _displayFont = Resources.Load<Font>("HUD/Oswald");
            _condensedFont = Resources.Load<Font>("HUD/BarlowCondensed");
            _bodyFont = Resources.Load<Font>("HUD/Barlow");
            if (!_displayFont) _displayFont = fallback;
            if (!_condensedFont) _condensedFont = fallback;
            if (!_bodyFont) _bodyFont = fallback;
            _title = MakeStyle(_displayFont, 44, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            _heading = MakeStyle(_displayFont, 29, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            _display = MakeStyle(_displayFont, 32, FontStyle.Normal, MainText, TextAnchor.MiddleLeft);
            _timer = MakeStyle(_displayFont, 64, FontStyle.Normal, MainText, TextAnchor.MiddleCenter);
            _mega = MakeStyle(_displayFont, 142, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            _logo = MakeStyle(_displayFont, 116, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            _body = MakeStyle(_condensedFont, 23, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            _story = MakeStyle(_bodyFont, 26, FontStyle.Normal, MainText, TextAnchor.MiddleLeft);
            _story.wordWrap = true;
            _small = MakeStyle(_bodyFont, 18, FontStyle.Normal, SoftText, TextAnchor.MiddleLeft);
            _tiny = MakeStyle(_condensedFont, 16, FontStyle.Bold, Muted, TextAnchor.MiddleLeft);
            _center = MakeStyle(_condensedFont, 21, FontStyle.Bold, MainText, TextAnchor.MiddleCenter);
            _caps = MakeStyle(_condensedFont, 16, FontStyle.Bold, MainText, TextAnchor.MiddleCenter);
            _wrap = MakeStyle(_bodyFont, 18, FontStyle.Normal, SoftText, TextAnchor.UpperLeft);
            _wrap.wordWrap = true;
            _right = MakeStyle(_displayFont, 26, FontStyle.Normal, MainText, TextAnchor.MiddleRight);
            _fittedStyle = new GUIStyle(_small);
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
            DrawTitleDesignV57();
        }

        void DrawStatusStrip()
        {
            if (World.BagCameraOwnsView) { DrawFinalBagStatus(); return; }
            DrawCompactStatusV5();
        }

        void DrawStatusIcon(Rect rect, string id, int count)
        {
            Frame(rect, Raised, Line, 1f);
            DrawIcon(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6), id);
            Label(new Rect(rect.x + 30, rect.y - 1, 40, 28), "×" + count, _tiny);
        }

        void DrawBagIcon(Rect rect, Color color)
        {
            PanicUiSymbolsV57.Draw(rect, "bag", color);
        }

        void DrawContext()
        {
            if (!World.IsMoving && World.LootVisible && World.OpenSource != null) DrawLootStrip();
            DrawCompactTargetV5();
            if (World.IsMoving && !string.IsNullOrEmpty(World.PlacementMessage))
            {
                Rect context = new Rect(404, 780, 792, 43);
                Color accent = World.PlacementValid ? Sea : Brass;
                Frame(context, Night, accent, 1f); Region(context);
                FittedLabel(new Rect(context.x + 16, context.y + 3, context.width - 32, 36), World.PlacementMessage, _center, MainText, 14);
            }
        }

        void DrawRequirements()
        {
            DrawCompactRequirementsV5();
        }

        void DrawInventoryRequirements()
        {
            PanicExpedition expedition = World.Expedition;
            IReadOnlyList<PanicMaterialRequirement> requirements = expedition.CurrentRequirements;
            int count = Mathf.Min(3, requirements.Count);
            Rect panel = new Rect(824, 647, 660, 100);
            Frame(panel, Night, new Color(Brass.r, Brass.g, Brass.b, .45f), 1f); Mark(panel);
            Label(new Rect(panel.x + 14, panel.y + 4, 632, 24), RequirementTitleV414(expedition) +
                (expedition.RequirementBudgetMissing > 0 ? "  ·  Bütçe eksik " + expedition.RequirementBudgetMissing + " $" : ""), ColorText(_tiny, Brass));
            if (count == 0) return;
            float cellWidth = (panel.width - 24) / count;
            for (int i = 0; i < count; i++)
            {
                PanicMaterialRequirement r = requirements[i];
                float x = panel.x + 12 + i * cellWidth;
                DrawIcon(new Rect(x, panel.y + 31, 48, 48), r.Id);
                Label(new Rect(x + 54, panel.y + 28, cellWidth - 58, 22), SlotName(new SupplySlot(r.Id, 1)), ColorText(_tiny, MainText));
                Label(new Rect(x + 54, panel.y + 51, cellWidth - 58, 20), "Var " + r.Owned + " / Gerek " + r.Required, _tiny);
                Label(new Rect(x + 54, panel.y + 73, cellWidth - 58, 20), "Eksik " + r.Missing, ColorText(_tiny, r.Missing > 0 ? Brass : Sea));
            }
        }

        static string RequirementTitle(string action)
        {
            switch (action)
            {
                case "anchorwall": case "anchorfloor": return "SABİTLEME · EKSİKLER";
                case "upgrade": return "GÜÇLENDİRME · EKSİKLER";
                case "flood": return "SEL BARİYERİ · EKSİKLER";
                case "fire": return "YANGIN TÜPÜ · EKSİKLER";
                case "plant": return "AĞAÇ SIRASI · EKSİKLER";
                case "drain": return "DRENAJ · EKSİKLER";
                case "fence": return "SEL ÇİTİ · EKSİKLER";
                case "hose": return "HORTUM · EKSİKLER";
                case "packbattery": return "EKİPMANI ÇALIŞTIRMAK İÇİN PİL GEREK";
                default: return "EKSİKLER";
            }
        }

        void DrawLootStrip()
        {
            LootSource source = World.OpenSource;
            if (source == null || source.Contents == null) return;
            int visible = 0;
            for (int i = 0; i < source.Contents.Count; i++) if (HasStock(source.Contents[i])) visible++;
            int count = Mathf.Min(10, visible);
            const float cellWidth = 100f;
            float width = count == 0 ? 650f : Mathf.Max(570f, count * 108f + 202f);
            float x = (CanvasWidth - width) * .5f;
            Rect panel = new Rect(x, 608, width, 148);
            Frame(panel, Night, new Color(Sea.r, Sea.g, Sea.b, .48f), 1f); Region(panel);
            Label(new Rect(x + 18, 617, width - 160, 24), Clip(source.Label, 48).ToUpperInvariant(), ColorText(_tiny, Brass));
            Label(new Rect(x + width - 156, 617, 138, 24), count == 0 ? "TAMAMLANDI" : "Z / X  SEÇ", ColorText(_caps, Muted));
            float startX = x + 16;
            const float y = 650;
            if (count == 0)
            {
                Frame(new Rect(startX + 2, y + 5, 66, 66), Panel, Line, 1f);
                DrawBagIcon(new Rect(startX + 17, y + 14, 36, 43), Muted);
                Label(new Rect(startX + 91, y + 3, 340, 32), "BÜTÜN MALZEMELER ALINDI", _body);
                Label(new Rect(startX + 91, y + 39, 320, 25), "Bu depo artık boş.", _small);
                Rect closeEmpty = new Rect(x + width - 140, y + 20, 120, 42);
                DrawKeycap(closeEmpty, "E", "KAPAT", Sea);
                if (MousePressed(closeEmpty)) World.ToggleContainer();
                return;
            }
            int drawn = 0;
            for (int i = 0; i < source.Contents.Count && drawn < count; i++)
            {
                SupplySlot slot = source.Contents[i];
                if (!HasStock(slot)) continue;
                Rect cell = new Rect(startX + drawn * 108f, y, cellWidth, 91);
                bool selected = i == World.LootIndex;
                Frame(cell, selected ? new Color(.145f, .416f, .424f, .35f) : Panel, selected ? Sea : Line, selected ? 2f : 1f);
                if (selected) Fill(new Rect(cell.x + 18, cell.yMax - 3, cell.width - 36, 2), Sea);
                DrawIcon(new Rect(cell.x + 16, cell.y + 3, 64, 59), slot.Id);
                Frame(new Rect(cell.xMax - 34, cell.y + 5, 29, 24), Night, Line, 1f);
                Label(new Rect(cell.xMax - 34, cell.y + 4, 29, 24), slot.Count.ToString(), ColorText(_caps, Brass));
                Label(new Rect(cell.x + 4, cell.y + 62, cell.width - 8, 22), SlotName(slot), ColorText(_caps, selected ? Sea : MainText));
                Mark(cell);
                int index = i;
                if (MousePressed(cell)) World.SelectLoot(index);
                drawn++;
            }
            float actionsX = startX + count * 108f + 6f;
            Rect take = new Rect(actionsX, y + 1, 152, 39);
            Rect close = new Rect(actionsX, y + 48, 152, 39);
            DrawKeycap(take, "F", "SEÇİLİYİ AL", Sea);
            DrawKeycap(close, "E", "KAPAT", Line);
            if (MousePressed(take)) World.TakeLoot();
            if (MousePressed(close)) World.ToggleContainer();
        }

        static bool HasStock(SupplySlot slot)
        {
            return slot != null && slot.Count > 0 && !string.IsNullOrEmpty(slot.Id);
        }

        void DrawInventory()
        {
            Fill(LogicalCanvasBounds, new Color(.055f, .05f, .045f, .42f));
            Rect panel = new Rect(90, 176, 1420, 654);
            Frame(panel, Night, Line, 1f); Region(panel);
            Brush(new Rect(114, 195, 228, 42), Rust);
            Label(new Rect(132, 191, 560, 46), "ENVANTER", _heading);
            Label(new Rect(824, 191, 660, 46), "HAZIRLIK DURUMU", _heading);
            Fill(new Rect(798, 254, 1, 480), Line);
            List<SupplySlot> inventory = World.Expedition == null ? null : World.Expedition.Inventory;
            int capacity = inventory == null ? 0 : inventory.Count;
            UnifiedInventoryLabelsFitV58 = true; UnifiedInventoryLabelProbesV58 = 0;
            Label(new Rect(126, 246, 654, 23), "TOPLADIĞIN EŞYALAR   /   " + UsedSlots(inventory) + " · " + capacity + " YUVA", _tiny);
            Label(new Rect(824, 246, 660, 23), "HER DALGA İÇİN 1 SU + 1 GIDA", _tiny);
            for (int i = 0; i < capacity; i++)
            {
                Rect cell = InventorySlotBounds(false, i);
                SupplySlot slot = inventory[i];
                bool selected = i == World.InventoryIndex;
                Frame(cell, selected ? new Color(.145f, .416f, .424f, .36f) : Panel, selected ? Sea : Line, selected ? 2f : 1f);
                DrawSlot(cell, slot, selected);
                if (selected) Fill(new Rect(cell.x + 18, cell.yMax - 3, cell.width - 36, 2), Sea);
                Mark(cell);
                HandleInventorySlot(cell, false, i);
            }
            Rect explanation = new Rect(824, 286, 660, 211);
            Frame(explanation, Panel, Line, 1f); Mark(explanation);
            Label(new Rect(842, 302, 624, 34), "TOPLADIĞIN ERZAK YANINDA", ColorText(_body, Sea));
            Label(new Rect(842, 350, 624, 102), "Su ve gıda topladığında hazır sayılır.\nHer dalgada birer tane kullanılır.\nFener ve radyo için birer pil gerekir.", _wrap);
            PanicModel model = World.Model;
            DrawInventoryMeter(new Rect(824, 523, 207, 76), "water", "SU", model.PackedWater, model.RequiredDays);
            DrawInventoryMeter(new Rect(1044, 523, 207, 76), "food", "GIDA", model.PackedFood, model.RequiredDays);
            DrawInventoryMeter(new Rect(1264, 523, 220, 76), "firstaid", "DESTEK", model.SupportCount, 4);
            DrawDot(new Rect(827, 620, 9, 9), model.BagReady ? Sea : Brass);
            string readyHint = World.JuryActive ? "ERZAK HAZIR · Kapat ve sıradaki önleme geç" : model.IsSimulation ? "ERZAK HAZIR · Önlemini seç, afeti başlat" : "ERZAK HAZIR · Sığınağa geçebilirsin";
            Label(new Rect(846, 607, 630, 30), model.BagReady ? readyHint :
                "Temel ihtiyaç: 1 su + 1 gıda", ColorText(_body, model.BagReady ? Sea : Brass));
            SupplySlot selectedSlot = inventory != null && World.InventoryIndex >= 0 && World.InventoryIndex < inventory.Count ? inventory[World.InventoryIndex] : null;
            Label(new Rect(126, 661, 650, 22), "SEÇİLİ EŞYA", _tiny);
            FittedLabel(new Rect(126, 684, 650, 28), SlotName(selectedSlot), _body, MainText, 14);
            if (!RequirementsCardVisible)
            {
                Rect info = new Rect(824, 647, 660, 100);
                Frame(info, Panel, Line, 1f);
                Label(new Rect(842, 659, 624, 26), "TEK ENVANTER", ColorText(_tiny, Brass));
                Label(new Rect(842, 693, 624, 42), "Yığınları sürükleyerek birleştir.\nFarklı eşyaların yerini değiştir.", _wrap);
            }
            Rect backButton = new Rect(126, 762, 264, 44);
            DrawKeycap(backButton, "BACKSPACE", "GERİ", Line);
            if (MousePressed(backButton)) World.ToggleInventory();
            Label(new Rect(824, 767, 650, 32), "FARE  Sürükle / bırak    ·    TAB / ESC  Kapat", _tiny);
            DrawInventoryMaterialPool();
            DrawInventoryDrag();
        }

        void DrawSlot(Rect cell, SupplySlot slot, bool selected)
        {
            if (!HasStock(slot))
            {
                Label(new Rect(cell.x, cell.y + 20, cell.width, 52), "·", ColorText(_center, new Color(Muted.r, Muted.g, Muted.b, .35f)));
                return;
            }
            DrawIcon(new Rect(cell.x + 35, cell.y + 5, cell.width - 70, cell.height - 31), slot.Id);
            Frame(new Rect(cell.xMax - 36, cell.y + 7, 28, 24), Night, Line, 1f);
            Label(new Rect(cell.xMax - 36, cell.y + 6, 28, 24), slot.Count.ToString(), ColorText(_caps, selected ? Sea : Brass));
            Rect nameBounds = new Rect(cell.x + 5, cell.yMax - 27, cell.width - 10, 21);
            string name = SlotName(slot);
            FittedLabel(nameBounds, name, _caps, selected ? Sea : SoftText, 10);
            if (World.InventoryOpen)
            {
                UnifiedInventoryLabelProbesV58++;
                UnifiedInventoryLabelsFitV58 &= _fittedStyle.CalcSize(new GUIContent(name)).x <= nameBounds.width + .1f &&
                    _fittedStyle.CalcHeight(new GUIContent(name), nameBounds.width) <= nameBounds.height + .1f;
            }
        }

        void DrawWork()
        {
            if (DrawManualWorkV414()) return;
            PanicWorkSession work = World.Expedition.Work;
            if (work.IsWorldConstruction) { DrawWorldConstruction(work); return; }
            Rect panel = new Rect(66, 164, 1468, 676);
            Frame(panel, Night, new Color(Brass.r, Brass.g, Brass.b, .45f), 1f); Region(panel);
            Rect image = new Rect(88, 186, 1010, 632);
            Frame(image, new Color(.07f, .065f, .06f, 1f), Line, 1f);
            Rect fitted = FittedTextureRect(new Rect(image.x + 2, image.y + 39, image.width - 4, image.height - 78), World.WorkTexture);
            if (World.WorkTexture) GUI.DrawTexture(fitted, World.WorkTexture, ScaleMode.StretchToFill, false);
            Label(new Rect(image.x + 22, image.y + 5, 930, 30), "VİDAYA TIKLA · KENDİLİĞİNDEN TAKILSIN", ColorText(_tiny, Brass));
            string costHint = World.Model.FreeMaterials && World.Model.InfiniteBudget ? "Geliştirici: bu işte malzeme ve bütçe harcanmaz." :
                World.Model.FreeMaterials ? "Geliştirici: malzeme harcanmaz; bütçe iş bitince düşer." :
                World.Model.InfiniteBudget ? "Geliştirici: bütçe harcanmaz; malzeme iş bitince düşer." :
                "İki vida bitince malzeme ve bütçe harcanır. İptal etmek ücretsizdir.";
            Label(new Rect(image.x + 22, image.yMax - 33, 960, 28), costHint, _tiny);
            Mark(image);

            for (int i = 0; i < work.TotalSteps; i++)
            {
                Rect hit = WorkClickTargetBounds(i);
                if (hit.width <= 0 || hit.height <= 0) continue;
                Vector2 center = hit.center;
                bool done = i < work.CompletedSteps, current = i == work.NextClickTarget;
                Color accent = done ? Sea : current ? Brass : Muted;
                DrawRing(hit, 1f, new Color(accent.r, accent.g, accent.b, current || done ? .96f : .35f), current ? 3f : 1.5f);
                if (done) DrawCheck(new Rect(center.x - 10, center.y - 7, 20, 14), Sea);
                else if (current && work.IsInstalling) DrawRing(new Rect(hit.x + 6, hit.y + 6, hit.width - 12, hit.height - 12), work.StageProgress, Sea, 4f);
                else if (current)
                {
                    Rect hint = new Rect(center.x - 57, hit.yMax + 6, 114, 29);
                    Frame(hint, Night, Brass, 1f); Label(hint, "TIKLA", ColorText(_center, Brass));
                    if (!DeveloperInputBlockedThisFrame && !_paused && World.CancelFrame != Time.frameCount && MousePressed(hit)) World.ClickWorkTarget(i);
                }
            }

            const float x = 1122f; const float w = 384f;
            Brush(new Rect(x, 190, w, 38), Rust);
            Label(new Rect(x + 14, 190, w - 26, 36), "SABİTLEME", _body);
            DrawAnchorTargetName();
            Label(new Rect(x, 299, w, 26), CurrentAnchorSurfaceLabel(), ColorText(_tiny, Brass));
            Label(new Rect(x, 332, w, 40), "Vidaya tıkla.", _heading);
            Label(new Rect(x, 382, w, 42), "Her vida otomatik takılır.\nİki vida bitince eşya sabitlenir.", _wrap);
            Label(new Rect(x, 433, w, 38), work.IsInstalling ? "VİDA TAKILIYOR" : "SIRADAKİ VİDA HAZIR", ColorText(_body, work.IsInstalling ? Sea : Brass));
            Label(new Rect(x, 484, w, 70), work.StepHint, _wrap);
            Label(new Rect(x, 591, w, 39), work.CompletedSteps + " / " + work.TotalSteps + " VİDA", _display);
            DrawBar(new Rect(x, 656, w, 9), work.Progress, Sea);
            Button(new Rect(x, 729, w, 58), "İPTAL ET · ESC", false, Line, World.CancelWork);
        }

        void DrawWorldConstruction(PanicWorkSession work)
        {
            Rect panel = new Rect(488, 648, 624, 154);
            Frame(panel, Night, Line, 1f); Region(panel);
            FittedLabel(new Rect(510, 661, 580, 35), work.Label, _heading, MainText, 20);
            Label(new Rect(512, 704, 560, 27), work.StepHint, ColorText(_small, Sea));
            DrawBar(new Rect(512, 753, 398, 8), work.Progress, Sea);
            Button(new Rect(924, 742, 176, 42), "İPTAL · ESC", false, Line, World.CancelWork);
        }

        /// <summary>Actual screw hit region in the1600x900 logical HUD canvas; shared with opt-in callback QA.</summary>
        public Rect WorkClickTargetBounds(int index)
        {
            if (!World || !World.WorkActive || !World.WorkTexture || World.Expedition.Work.IsWorldConstruction ||
                index < 0 || index >= World.Expedition.Work.TotalSteps) return new Rect();
            Vector3 point = World.WorkTargetViewport(index);
            if (float.IsNaN(point.x) || float.IsInfinity(point.x) || float.IsNaN(point.y) || float.IsInfinity(point.y) ||
                float.IsNaN(point.z) || float.IsInfinity(point.z) || point.z <= 0 || point.x < 0 || point.x > 1 || point.y < 0 || point.y > 1) return new Rect();
            Rect fitted = FittedTextureRect(new Rect(90,225,1006,554), World.WorkTexture);
            Vector2 center = new Vector2(fitted.x + point.x * fitted.width, fitted.y + (1f - point.y) * fitted.height);
            const float radius = 38f;
            return new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2);
        }

        static Rect FittedTextureRect(Rect bounds, Texture texture)
        {
            if (!texture || texture.width <= 0 || texture.height <= 0) return bounds;
            float scale = Mathf.Min(bounds.width / texture.width, bounds.height / texture.height);
            Vector2 size = new Vector2(texture.width * scale, texture.height * scale);
            return new Rect(bounds.center.x - size.x * .5f, bounds.center.y - size.y * .5f, size.x, size.y);
        }

        void DrawApplicableAction()
        {
            if (World.IsMoving || World.WorkActive || World.InventoryOpen || RequirementsCardVisible || _paused || DeveloperMenuOpen) return;
            if (World.Model.Phase != PanicPhase.Preparing && World.Model.Phase != PanicPhase.Tutorial) return;
            PanicItem item = World.Selected;
            if (item == null || !World.CanInteract) return;
            bool release = PanicItemActionRules.CanReleaseAnchor(item);
            int shortcut = release ? 1 : World.Expedition.PrimaryShortcutV414(item);
            if (shortcut == 0 || !World.Model.CanBeginJuryWork(release ? "anchorwall" : World.Expedition.PrimaryActionV414(item), item.Id)) return;
            Rect action = new Rect(930, 806, 206, 31);
            Frame(action, Night, Brass, 1); Mark(action);
            FittedLabel(new Rect(action.x + 8, action.y + 2, action.width - 16, action.height - 4), shortcut + " · " + (release ? "ÇÖZ" : World.Expedition.PrimaryLabelV414(item)), _tiny, Brass, 12);
            if (MousePressed(action)) World.QuickAction(shortcut);
        }

        void DrawControls(string[][] controls)
        {
            // V5 keeps one optional corner guide; work/inventory keep their own contextual actions.
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
            if (!prepare || selected == null || !World.CanInteract) return WalkControls;
            var controls = new List<string[]>(WalkControls);
            int shortcut = World.Expedition.PrimaryShortcutV414(selected);
            if (shortcut != 0 && World.Model.CanBeginJuryWork(World.Expedition.PrimaryActionV414(selected), selected.Id)) controls.Add(new[] { shortcut.ToString(), World.Expedition.PrimaryLabelV414(selected) });
            if (selected.Movable && !selected.Anchored) controls.Add(new[] { "SAĞ TIK/4", "TAŞI" });
            return controls.ToArray();
        }

        void DrawHelpCard()
        {
            Rect panel = new Rect(24, 402, 378, 180);
            Frame(panel, Night, new Color(Sea.r, Sea.g, Sea.b, .5f), 1f); Region(panel);
            Brush(new Rect(38, 419, 177, 30), Petrol);
            Label(new Rect(52, 418, 318, 32), "HIZLI HAZIRLIK", _body);
            Label(new Rect(44, 463, 336, 27), "E ile dolabı aç · Malzeme havuza eklenir", _small);
            Label(new Rect(44, 497, 336, 27), "Son 40 sn · su ve gıdayı al", _small);
            Label(new Rect(44, 531, 336, 27), "Nesnenin önlemini seç · H kapat", ColorText(_tiny, Sea));
        }

        void DrawDisasterCue()
        {
            DrawDisasterCueDesignV57();
        }

        void DrawToast(string message)
        {
            DrawToastDesignV57(message);
        }

        void DrawLossReveal()
        {
            Fill(LogicalCanvasBounds, new Color(.13f, .025f, .018f, .83f));
            DrawHazardStripe(new Rect(0, 0, CanvasWidth, 9), Ember);
            DrawHazardStripe(new Rect(0, CanvasHeight - 9, CanvasWidth, 9), Ember);
            DrawWarning(new Rect(753, 211, 94, 94), Ember);
            bool deadline = World.CinematicKind == "Deadline";
            bool bagFailure = World.CinematicKind == "BagFailure";
            Label(new Rect(180, 337, 1240, 40), deadline ? "HAZIRLIK SÜRESİ DOLDU" : bagFailure ? "ÇANTADA EKSİK VAR" : "EVDE AĞIR HASAR VAR", ColorText(_center, Brass));
            Brush(new Rect(226, 400, 1148, 102), Rust);
            Label(new Rect(250, 391, 1100, 119), deadline ? World.Model.IsSimulation ? "SÜRE DOLDU" : "SIĞINAĞA YETİŞEMEDİN" : bagFailure ? "ERZAK YETMEDİ" : "ÖNLEM YETERSİZ", ColorText(_timer, MainText));
            Label(new Rect(180, 548, 1240, 35), World.Model.IsSimulation ? "SİMÜLASYON BİTTİ" : "KOŞU BİTTİ", ColorText(_center, SoftText));
            Region(new Rect(180, 211, 1240, 390));
        }

        void DrawCinematic()
        {
            float appear = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(World.CinematicProgress * 5f));
            float band = Mathf.Lerp(62f, 84f, appear);
            Fill(new Rect(0, 0, CanvasWidth, band), new Color(.035f, .03f, .025f, .94f));
            Fill(new Rect(0, CanvasHeight - 150, CanvasWidth, 150), new Color(.035f, .03f, .025f, .94f));
            Label(new Rect(32, 25, 900, 27), PanicKktcStoryV53.LocationBadge + "  ·  " + World.Model.ActiveDisasterLabelV54, _tiny);
            string title = string.IsNullOrEmpty(World.CinematicTitle) ? "Afetin ardından" : World.CinematicTitle;
            Label(new Rect(410, CanvasHeight - 135, 1140, 96), title, _story);
            DrawBar(new Rect(410, CanvasHeight - 25, 1140, 3), World.CinematicProgress, Brass);
            Region(new Rect(0, 0, CanvasWidth, band));
            Region(new Rect(0, CanvasHeight - 150, CanvasWidth, 150));
            Button(new Rect(1282, 18, 286, 40), "ATLA · ENTER", false, Brass, World.SkipResidenceCinematic);
            if (!DeveloperInputBlockedThisFrame && World.CancelFrame != Time.frameCount && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            { World.SkipResidenceCinematic(); Event.current.Use(); }
        }

        void DrawInspection()
        {
            BuildInspectionList();
            DrawWaveLedgerReopenV56();
            Rect panel = new Rect(24, 606, 392, 118);
            Frame(panel, Night, Line, 1f); Region(panel);
            int total = Mathf.Max(1, _inspectionItems.Count);
            int step = Mathf.Clamp(_inspectionStep, 0, total - 1);
            string target = _inspectionItems.Count > 0 ? Clip(_inspectionItems[step].Label, 36) : "EV";
            if (_inspectionItems.Count > 0)
            {
                int targetFloor = Mathf.Clamp(Mathf.FloorToInt((_inspectionItems[step].Position.y + .65f) / 3.2f), -1, 1);
                if (targetFloor != World.ActiveFloor) target += targetFloor > World.ActiveFloor ? " · ÜST KAT" : " · ALT KAT";
            }
            Brush(new Rect(36, 618, 190, 27), Rust);
            Label(new Rect(44, 618, 352, 28), "HASAR GEZİSİ  " + Mathf.Min(step + 1, total) + "/" + total, _body);
            FittedLabel(new Rect(38, 652, 364, 27), target, _body, MainText, 18);
            Label(new Rect(38, 687, 220, 24), "TOPLAM HASAR", _tiny);
            Label(new Rect(266, 681, 136, 32), World.Model.TotalDamage.ToString("0.#") + "%", ColorText(_right, World.Model.TotalDamage >= 50f ? Brass : Sea));
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
            DrawPauseDesignV57();
        }

        bool DeveloperBudgetAvailable()
        {
            if (!World || World.Model == null) return false;
            PanicPhase phase = World.Model.Phase;
            return phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial || phase == PanicPhase.Inspection;
        }

        void DrawDeveloperMenu()
        {
            Fill(LogicalCanvasBounds, new Color(.05f, .045f, .04f, .78f));
            Mark(LogicalCanvasBounds);
            _topLevelRegions.Clear(); // Underlying status is dimmed by this modal.
            Rect panel = new Rect(314, 108, 972, 696);
            Frame(panel, Night, Line, 1f); Region(panel);
            Brush(new Rect(338, 130, 352, 46), Petrol);
            Label(new Rect(358, 124, 820, 53), "GELİŞTİRİCİ", _title);
            Label(new Rect(352, 195, 876, 30), "Hazırlığı kendi hızında dene.", _body);
            string state = World.Model.Phase == PanicPhase.Title
                ? "Seçimler sonraki koşuya uygulanır. Menüyü açmak skoru etkilemez."
                : World.JuryActive ? "Kontroller durdu; süre devam ediyor. Seçenekler skoru etkiler." : "Oyun durdu. Bir hileyi açarsan bu koşunun skoru kaydedilmez.";
            Label(new Rect(352, 234, 884, 30), state, _small);
            DrawDeveloperToggle(new Rect(352, 284, 896, 70), PanicDeveloperOption.InfiniteBudget,
                "Sınırsız bütçe", "İş ve yükseltmeler bütçeyi azaltmaz.", World.Model.Developer.InfiniteBudget, 0);
            DrawDeveloperToggle(new Rect(352, 364, 896, 70), PanicDeveloperOption.FreeMaterials,
                "Malzeme harcama", "Gerekli malzemeler aranmaz ve tüketilmez.", World.Model.Developer.FreeMaterials, 1);
            DrawDeveloperToggle(new Rect(352, 444, 896, 70), PanicDeveloperOption.FreezePreparationTime,
                "Hazırlık süresini durdur", "Yalnız hazırlık sayacı durur.", World.Model.Developer.FreezePreparationTime, 2);
            DrawDeveloperToggle(new Rect(352, 524, 896, 70), PanicDeveloperOption.NoLoss,
                "Kaybetmeme", "Afetler ve hasar sürer; kayıp ve dalga ödülü uygulanmaz.", World.Model.Developer.NoLoss, 3);
            bool canAdd = DeveloperBudgetAvailable();
            Rect budget = new Rect(352, 610, 408, 42);
            if (canAdd) Button(budget, "+$1.000 BÜTÇE", _developerChoice == 4, Sea, () => World.Model.AddDeveloperBudget(1000));
            else
            {
                Frame(budget, Panel, _developerChoice == 4 ? Brass : Line, 1f); Mark(budget);
                Label(budget, "+$1.000 · Koşuda kullanılabilir", ColorText(_center, Muted));
            }
            if (World.CanDeveloperSkipWaveV51) Button(new Rect(786, 610, 462, 42), "DALGAYI GEÇ · F3", _developerChoice == 5, Brass, RequestDeveloperWaveSkipV51);
            else { Frame(new Rect(786,610,462,42), Panel, Line,1); Label(new Rect(786,610,462,42), "F3 · OYUNDA DALGA GEÇ", ColorText(_center, Muted)); }
            Label(new Rect(352, 665, 896, 27), World.Model.CheatedRun
                ? "HİLELİ KOŞU · Seçenekleri kapatsan da skor kaydedilmez."
                : "F3: afeti izle, kaybetmeden devam et. Dalga ödülü ve skor kaydı verilmez.", ColorText(_small, World.Model.CheatedRun ? Brass : Muted));
            DrawKeycap(new Rect(352, 716, 170, 42), "OKLAR", "SEÇ", Line);
            DrawKeycap(new Rect(534, 716, 284, 42), "ENTER / SPACE", "DEĞİŞTİR", Line);
            Button(new Rect(914, 708, 334, 56), "KAPAT · F2 / ESC", _developerChoice == 6, Sea, () => SetDeveloperMenuOpen(false));
            HandleDeveloperMenuEvent();
        }

        void DrawDeveloperToggle(Rect rect, PanicDeveloperOption option, string title, string description, bool enabled, int choice)
        {
            bool selected = _developerChoice == choice;
            Frame(rect, enabled ? new Color(Petrol.r, Petrol.g, Petrol.b, .24f) : Panel, selected ? Brass : enabled ? Sea : Line, selected ? 2f : 1f);
            Mark(rect);
            Label(new Rect(rect.x + 20, rect.y + 6, rect.width - 190, 29), title, ColorText(_body, MainText));
            Label(new Rect(rect.x + 20, rect.y + 36, rect.width - 190, 25), description, _small);
            Rect toggle = new Rect(rect.xMax - 132, rect.y + 23, 108, 38);
            Frame(toggle, enabled ? Petrol : Night, enabled ? Sea : Line, 1f);
            Label(toggle, enabled ? "AÇIK" : "KAPALI", ColorText(_caps, enabled ? MainText : Muted));
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
                _developerChoice = (_developerChoice + 6) % 7;
                e.Use();
            }
            else if (e.keyCode == KeyCode.DownArrow)
            {
                _developerChoice = (_developerChoice + 1) % 7;
                e.Use();
            }
            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Space)
            {
                if (_developerChoice == 0) World.Model.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, !World.Model.Developer.InfiniteBudget);
                else if (_developerChoice == 1) World.Model.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, !World.Model.Developer.FreeMaterials);
                else if (_developerChoice == 2) World.Model.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, !World.Model.Developer.FreezePreparationTime);
                else if (_developerChoice == 3) World.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss, !World.Model.Developer.NoLoss);
                else if (_developerChoice == 4 && DeveloperBudgetAvailable()) World.Model.AddDeveloperBudget(1000);
                else if (_developerChoice == 5) RequestDeveloperWaveSkipV51();
                else if (_developerChoice == 6) SetDeveloperMenuOpen(false);
                e.Use();
            }
        }

        void DrawCheatedRunIndicator()
        {
            Rect badge = new Rect(24, 879, 395, 20);
            Fill(badge, Night);
            Fill(new Rect(24, 879, 3, 20), Brass);
            Label(new Rect(36, 878, 377, 22), "HİLELİ KOŞU  /  SKOR KAYDEDİLMEZ", ColorText(_tiny, Brass));
        }

        void DrawEndCard()
        {
            Fill(LogicalCanvasBounds, new Color(.055f, .05f, .045f, .66f));
            bool won = World.Model.Phase == PanicPhase.Success;
            Color accent = won ? Sea : Ember;
            Rect panel = new Rect(276, 220, 1048, 536);
            Frame(panel, Night, Line, 1f); Region(panel);
            DrawHazardStripe(new Rect(306, 250, 240, 5), accent);
            Label(new Rect(316, 278, 968, 29), won ? "HAZIRLIĞIN İŞE YARADI" : "BİR SONRAKİ KOŞUYA HAZIRLAN", ColorText(_tiny, accent));
            Brush(new Rect(306, 331, 968, 72), won ? Petrol : Rust);
            Label(new Rect(332, 321, 924, 92), won ? "EV GÜVENDE" : "KOŞU BİTTİ", _timer);
            Label(new Rect(318, 442, 265, 24), "TAMAMLANAN DALGA", _tiny);
            Label(new Rect(318, 474, 265, 57), World.Model.CompletedWaves.ToString("00"), _title);
            Label(new Rect(646, 442, 290, 24), "TOPLAM PUAN", _tiny);
            Label(new Rect(646, 474, 290, 57), World.Model.Score.ToString("N0"), ColorText(_title, Brass));
            Label(new Rect(1000, 442, 260, 24), "EVDEKİ HASAR", _tiny);
            Label(new Rect(1000, 474, 260, 57), World.Model.TotalDamage.ToString("0.#") + "%", ColorText(_title, accent));
            string detail = World.Model.CheatedRun ? "Hileli koşu · Bu skor kaydedilmez." : won ? "Afete hazırlık evini korudu." : World.Model.FailureReason;
            ResultFailureTextV56 = detail;
            ResultFailureRectV56 = new Rect(318, 552, 942, 78);
            Label(ResultFailureRectV56, detail, _wrap);
            DrawWaveLedgerReopenV56();
            if (World.Model.IsSimulation && World.Model.SimulationComplete)
            {
                Button(new Rect(318, 650, 300, 58), "HASARI İNCELE", true, Brass, () => { _inspectionStep = 0; World.InspectSimulationDamage(); });
                Button(new Rect(632, 650, 300, 58), "TEKRAR DENE", false, Sea, RestartRun);
                Button(new Rect(946, 650, 314, 58), "ANA MENÜ", false, Line, ReturnToMenu);
            }
            else
            {
                Button(new Rect(318, 650, 454, 58), "YENİDEN BAŞLA", true, Brass, RestartRun);
                Button(new Rect(790, 650, 470, 58), "ANA MENÜ", false, Sea, ReturnToMenu);
            }
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
            if (World.JuryActive) { StartJuryRun(); return; }
            if (World.Model.IsSimulation) { StartSelectedSimulation(World.Model.Disaster); return; }
            if (_paused) SetPaused(false);
            _helpVisible = false;
            _inspectionStep = 0;
            World.StartGame(false);
        }

        void ReturnToMenu()
        {
            simulationSelectOpen=false;
            if (_paused) SetPaused(false);
            _helpVisible = false;
            World.ReturnToMenu();
        }

        void Button(Rect rect, string text, bool selected, Color accent, Action action)
        {
            DrawButtonDesignV57(rect, text, selected, accent, action);
        }

        void DrawKeycap(Rect rect, string key, string action, Color accent)
        {
            Frame(rect, Night, accent.a > .5f ? new Color(accent.r, accent.g, accent.b, .50f) : Line, 1f);
            Mark(rect);
            float desiredKey = _caps.CalcSize(new GUIContent(key)).x + 16f;
            float actionWidth = _caps.CalcSize(new GUIContent(action)).x;
            float keyWidth = Mathf.Clamp(desiredKey, 28f, Mathf.Max(28f, rect.width - Mathf.Min(actionWidth, rect.width * .55f) - 28f));
            float keyHeight = Mathf.Min(28f, rect.height - 10f);
            Rect cap = new Rect(rect.x + 6, rect.center.y - keyHeight * .5f - 1f, keyWidth, keyHeight);
            bool hot = accent.a > .5f;
            Color capColor = hot ? accent : SoftText;
            RoundFill(new Rect(cap.x, cap.y + 3, cap.width, cap.height), hot ? new Color(accent.r * .5f, accent.g * .5f, accent.b * .5f, 1f) : new Color(.4f, .37f, .33f, 1f));
            RoundFill(cap, capColor);
            FittedLabel(cap, key, _caps, Night, 11);
            Rect label = new Rect(cap.xMax + 9, rect.y + 1, rect.xMax - cap.xMax - 15, rect.height - 2);
            FittedLabel(label, action, _caps, MainText, 11);
        }

        void DrawIcon(Rect rect, string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (id == "seedling") { DrawTreeRowIcon(rect); return; }
            Texture icon = null;
            if (id == "water" || id == "food")
            {
                if (_supplyPreviews == null) _supplyPreviews = new PanicHudSupplyPreview();
                icon = _supplyPreviews.Get(id);
            }
            if (!icon) icon = PanicResidenceArt.Icon(id);
            if (icon) GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
        }

        // Original vector icon: three mature crowns and trunks, matching the regional kit.
        void DrawTreeRowIcon(Rect rect)
        {
            for (int i = 0; i < 3; i++)
            {
                float cx = rect.x + rect.width * (.21f + i * .29f);
                float top = rect.y + rect.height * (i == 1 ? .06f : .17f);
                Fill(new Rect(cx - rect.width * .035f, top + rect.height * .30f, rect.width * .07f, rect.height * .52f), new Color(.48f,.30f,.16f));
                Color leaves = i == 1 ? Sea : Petrol;
                DrawDot(new Rect(cx - rect.width * .17f, top, rect.width * .34f, rect.height * .43f), leaves);
                DrawDot(new Rect(cx - rect.width * .21f, top + rect.height * .19f, rect.width * .42f, rect.height * .34f), leaves);
            }
            Fill(new Rect(rect.x + rect.width * .07f, rect.y + rect.height * .86f, rect.width * .86f, rect.height * .055f), Brass);
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
            RoundFill(rect, new Color(.035f, .03f, .025f, .9f));
            float width = rect.width * Mathf.Clamp01(value);
            if (width > 0f) RoundFill(new Rect(rect.x, rect.y, width, rect.height), color);
        }

        bool MousePressed(Rect rect)
        {
            if (KeyboardHelpBlockingV56 && !KeyboardHelpButtonInputV56) return false;
            if (World && (World.DiscreteInputFrame == Time.frameCount || World.CancelFrame == Time.frameCount)) return false;
            Event e = Event.current;
            // IMGUI already transforms event coordinates through GUI.matrix. Use the
            // position belonging to this click, even if the physical cursor moved.
            Vector2 eventPoint = PanicInteractionAudit.GuiPointer(e.mousePosition);
            if (e.type != EventType.MouseDown || e.button != 0 || !rect.Contains(eventPoint)) return false;
            e.Use();
            return true;
        }

        void DrawTutorialActions()
        {
            if (!World.Model.IsTutorial || World.Model.Phase != PanicPhase.Tutorial ||
                World.LootVisible || World.WorkActive || World.InventoryOpen || World.IsMoving) return;
            Button(new Rect(1120, 754, 456, 54), "ÖĞRETİCİYİ BİTİR · N", false, Sea, FinishTutorialRun);
            Event e = Event.current;
            if (!DeveloperInputBlockedThisFrame && World.CancelFrame != Time.frameCount &&
                e.type == EventType.KeyDown && e.keyCode == KeyCode.N)
            { FinishTutorialRun(); e.Use(); }
        }

        void FinishTutorialRun()
        {
            if (!World || World.Model == null || !World.Model.IsTutorial ||
                World.Model.Phase != PanicPhase.Tutorial || World.WorkActive ||
                World.InventoryOpen || World.IsMoving || World.CinematicActive ||
                World.LossAnnounced || _paused || DeveloperMenuOpen || DeveloperInputBlockedThisFrame ||
                World.CancelFrame == Time.frameCount) return;
            _helpVisible = false; _inspectionStep = 0; _inspectionFocusId = null;
            World.FinishTutorial();
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
            // A quiet, flat surface. No repeated bevel or gloss competes with game information.
            Color opaque = Color.Lerp(new Color(Night.r, Night.g, Night.b, 1f), new Color(fill.r, fill.g, fill.b, 1f), fill.a);
            Fill(new Rect(rect.x, rect.y + 2, rect.width, rect.height), new Color(0, 0, 0, .16f));
            Fill(rect, opaque);
            Fill(new Rect(rect.x, rect.y, rect.width, thickness), edge);
            Fill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), edge);
            Fill(new Rect(rect.x, rect.y, thickness, rect.height), edge);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), edge);
        }


        // World calls this from its own OnGUI; screen projection stays tied to the live camera.
        // Markers never register a UI hit rectangle, so they cannot eat the world's mouse input.
        public void DrawWorldMarker(Vector3 worldPosition, string label, float width, bool guidance = false)
        {
            if (!World || !World.View || _paused || DeveloperMenuOpen || DeveloperInputBlockedThisFrame ||
                World.InventoryOpen || World.WorkActive || World.CinematicActive || World.LossAnnounced) return;
            if ((World.Model.Phase == PanicPhase.GameOver || World.Model.Phase == PanicPhase.Success) && (label ?? "").StartsWith("E  ", StringComparison.Ordinal)) return;
            Vector3 projected = World.View.WorldToScreenPoint(worldPosition);
            if (projected.z <= 0f) return;
            PrepareCanvas(); EnsureStyles();
            Matrix4x4 priorMatrix = GUI.matrix;
            Color priorColor = GUI.color;
            GUI.matrix = Matrix4x4.TRS(new Vector3(_offset.x, _offset.y, 0), Quaternion.identity, new Vector3(_scale, _scale, 1));
            int priorDepth = GUI.depth;
            GUI.depth = 10;
            GUI.color = Color.white;
            Vector2 logical = (new Vector2(projected.x, Screen.height - projected.y) - _offset) / _scale;
            string text = label ?? "";
            string key = text.StartsWith("E  ", StringComparison.Ordinal) ? "E" : text.StartsWith("!", StringComparison.Ordinal) ? "!" : "";
            if (key == "E") text = text.Substring(3);
            else if (key == "!") text = text.Substring(1).Trim();
            float desired = Mathf.Max(width, _small.CalcSize(new GUIContent(text)).x + (key.Length > 0 ? 70f : 28f));
            desired = Mathf.Clamp(desired, 112f, 430f);
            Rect rect = new Rect(Mathf.Clamp(logical.x - desired * .5f, 12f, CanvasWidth - desired - 12f),
                Mathf.Clamp(logical.y, 142f, CanvasHeight - 110f), desired, 41f);
            rect = ReserveWorldMarkerBounds(rect, text);
            // If the finite placement search has no caption-safe slot, this cosmetic popup yields.
            if (!WorldMarkerAvoidsDialogueV59(rect))
            {
                GUI.matrix = priorMatrix; GUI.color = priorColor; GUI.depth = priorDepth;
                return;
            }
            RecordWorldMarkerBoundsV59(rect);
            WorldMarkerLeader(logical, rect);
            Frame(rect, Night, new Color(Brass.r, Brass.g, Brass.b, .55f), 1f);
            if (key.Length > 0)
            {
                Rect cap = new Rect(rect.x + 7, rect.y + 7, 27, 27);
                RoundFill(cap, key == "!" ? Rust : Brass);
                GUI.Label(cap, key, ColorText(_caps, key == "!" ? MainText : Night));
            }
            GUI.Label(new Rect(rect.x + (key.Length > 0 ? 44 : 14), rect.y + 6, rect.width - (key.Length > 0 ? 53 : 28), 29), text, ColorText(_small, MainText));
            DrawDot(new Rect(rect.center.x - 3, rect.yMax + 7, 6, 6), Brass);
            GUI.matrix = priorMatrix;
            GUI.color = priorColor;
            GUI.depth = priorDepth;
        }

        void DrawTimerCard()
        {
            PanicModel model = World.Model;
            bool preparation = model.Phase == PanicPhase.Preparing || model.Phase == PanicPhase.Tutorial;
            bool precautions = CampaignPrecautionTimer;
            bool timed = preparation || model.Phase == PanicPhase.Disaster || model.Phase == PanicPhase.Aftermath || model.Phase == PanicPhase.Inspection;
            float value = preparation ? PreparationTimerRemaining : timed ? model.PhaseTime : 0f;
            if (_timerWave != model.Wave || _timerPhase != model.Phase || _timerPrecautionStage != precautions)
            {
                _timerWave = model.Wave;
                _timerPhase = model.Phase;
                _timerPrecautionStage = precautions;
                _timerMaximum = Mathf.Max(1f, value);
            }
            _timerMaximum = precautions ? PanicModel.NormalPrecautionSeconds : Mathf.Max(_timerMaximum, value);
            Color accent = TimerAlarmActive ? Ember : Sea;
            Rect timer = TimerCardBounds;
            if (TimerAlarmActive)
            {
                float pulse = .12f + .12f * (.5f + .5f * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f));
                RoundFill(new Rect(timer.x - 4, timer.y - 4, timer.width + 8, timer.height + 8), new Color(Ember.r, Ember.g, Ember.b, pulse));
            }
            Frame(timer, Night, TimerAlarmActive ? new Color(Ember.r, Ember.g, Ember.b, .7f) : Line, 1f);
            Region(timer);
            DrawWatch(new Rect(630, 32, 45, 53), TimerAlarmActive ? Ember : Rust);
            string digits = timed ? FormatTime(value) : "--:--";
            Label(new Rect(687, 13, 187, 80), digits, ColorText(_timer, TimerAlarmActive ? Ember : MainText));
            string phase = WaveLedgerVisibleV56 || KeyboardHelpVisibleV56 || _paused ? "DURDU" : preparation && model.FreezePreparationTime ? "DONDU" : precautions ? "GÜÇLENDİRME" : preparation ? "HAZIRLIK" : PhaseName(model.Phase);
            FittedLabel(new Rect(876, 35, 100, 25), phase, _tiny, accent, 12);
            Label(new Rect(876, 60, 100, 22), preparation ? TimerAlarmActive ? "SON 60 SN" : "KALAN SÜRE" : "DURUM", _tiny);
            Rect track = new Rect(635, 99, 330, 6);
            DrawBar(track, value / _timerMaximum, accent);
            if (preparation && _timerMaximum > 60f)
            {
                float notch = track.x + track.width * (60f / _timerMaximum);
                Fill(new Rect(notch - 1, track.y - 3, 2, 12), Brass);
            }
            if (TimerAlarmActive && !_paused && !World.InventoryOpen && !World.WorkActive &&
                (string.IsNullOrEmpty(_developerNotice) || Time.unscaledTime >= _developerNoticeUntil))
            {
                Rect banner = new Rect(588, 140, 424, 44);
                Frame(banner, new Color(.24f, .075f, .05f, .96f), new Color(Ember.r, Ember.g, Ember.b, .55f), 1f);
                DrawWarning(new Rect(601, 151, 20, 20), Ember);
                Label(new Rect(632, 144, 367, 35), precautions ? "GÜÇLENDİRMEYİ BİTİR · SONRA ÇANTA" : model.IsSimulation ? "ÖNLEMİNİ BİTİR · AFETİ BAŞLAT" : "AZ KALDI · SIĞINAĞA YETİŞ!", ColorText(_center, MainText));
                Region(banner);
            }
        }

        void DrawRequirementRow(Rect row, PanicMaterialRequirement requirement)
        {
            bool enough = requirement.Missing <= 0;
            Frame(row, enough ? new Color(Petrol.r, Petrol.g, Petrol.b, .20f) : Panel,
                enough ? new Color(Sea.r, Sea.g, Sea.b, .45f) : Line, 1f);
            DrawIcon(new Rect(row.x + 7, row.y + 5, 63, 63), requirement.Id);
            Label(new Rect(row.x + 97, row.y + 7, 138, 30), SlotName(new SupplySlot(requirement.Id, 1)), _body);
            int pips = Mathf.Min(6, Mathf.Max(1, requirement.Required));
            for (int i = 0; i < pips; i++)
                Fill(new Rect(row.x + 98 + i * 17, row.y + 48, 12, 6), i < requirement.Owned ? Sea : new Color(Muted.r, Muted.g, Muted.b, .25f));
            Label(new Rect(row.x + 238, row.y + 15, 58, 43), requirement.Owned.ToString(), ColorText(_center, enough ? Sea : SoftText));
            Label(new Rect(row.x + 298, row.y + 15, 64, 43), requirement.Required.ToString(), ColorText(_center, SoftText));
            Frame(new Rect(row.x + 370, row.y + 18, 64, 39), enough ? Petrol : new Color(.30f, .16f, .07f, .95f), enough ? Sea : new Color(Brass.r, Brass.g, Brass.b, .45f), 1f);
            Label(new Rect(row.x + 370, row.y + 17, 64, 39), requirement.Missing.ToString(), ColorText(_center, enough ? MainText : Brass));
        }

        void DrawBagCheck(Rect row, string icon, string title, int count, int required)
        {
            DrawIcon(new Rect(row.x, row.y - 1, 37, 37), icon);
            Label(new Rect(row.x + 48, row.y + 1, 96, 30), title, _body);
            Label(new Rect(row.x + 145, row.y + 1, 66, 30), count + "/" + required, ColorText(_center, count >= required ? Sea : Ember));
            if (count >= required) DrawCheck(new Rect(row.xMax - 22, row.y + 11, 16, 12), Sea);
            else DrawWarning(new Rect(row.xMax - 23, row.y + 10, 17, 17), Ember);
        }

        void DrawInventoryMeter(Rect rect, string icon, string title, int count, int required)
        {
            bool enough = count >= required;
            Frame(rect, Panel, Line, 1f);
            DrawIcon(new Rect(rect.x + 9, rect.y + 4, 48, 51), icon);
            Label(new Rect(rect.x + 65, rect.y + 8, rect.width - 155, 24), title, _tiny);
            Label(new Rect(rect.xMax - 86, rect.y + 4, 72, 35), count + "/" + required, ColorText(_right, enough ? Sea : Brass));
            DrawBar(new Rect(rect.x + 13, rect.yMax - 18, rect.width - 26, 5), required > 0 ? (float)count / required : 1f, enough ? Sea : Brass);
        }

        void FittedLabel(Rect rect, string text, GUIStyle source, Color color, int minimumSize)
        {
            if (_fittedStyle == null) _fittedStyle = new GUIStyle(source);
            _fittedStyle.font = source.font;
            _fittedStyle.fontStyle = source.fontStyle;
            _fittedStyle.alignment = source.alignment;
            _fittedStyle.wordWrap = false;
            _fittedStyle.fontSize = source.fontSize;
            _fittedStyle.normal.textColor = color;
            _fittedStyle.hover.textColor = color;
            _fittedStyle.active.textColor = color;
            _fittedStyle.focused.textColor = color;
            float width = source.CalcSize(new GUIContent(text ?? "")).x;
            if (width > rect.width && width > 0f)
                _fittedStyle.fontSize = Mathf.Clamp(Mathf.FloorToInt(source.fontSize * rect.width / width), minimumSize, source.fontSize);
            Label(rect, text, _fittedStyle);
        }

        Rect ClearWorldMarkerBounds(Rect original)
        {
            if (WorldMarkerClear(original)) return original;
            Rect best = original;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < _topLevelRegions.Count; i++)
            {
                Rect obstacle = _topLevelRegions[i];
                if (obstacle.width * obstacle.height > CanvasWidth * CanvasHeight * .65f) continue;
                if (!original.Overlaps(obstacle)) continue;
                for (int edge = 0; edge < 4; edge++)
                {
                    Rect candidate = original;
                    if (edge == 0) candidate.y = obstacle.yMin - candidate.height - 10f;
                    else if (edge == 1) candidate.y = obstacle.yMax + 10f;
                    else if (edge == 2) candidate.x = obstacle.xMin - candidate.width - 10f;
                    else candidate.x = obstacle.xMax + 10f;
                    if (candidate.xMin < 12 || candidate.xMax > CanvasWidth - 12 || candidate.yMin < 142 || candidate.yMax > CanvasHeight - 65) continue;
                    if (!WorldMarkerClear(candidate)) continue;
                    float delta = (candidate.center - original.center).sqrMagnitude;
                    if (delta < distance) { distance = delta; best = candidate; }
                }
            }
            return best;
        }

        bool WorldMarkerClear(Rect marker)
        {
            if (!WorldMarkerAvoidsAlertV56(marker) || !WorldMarkerAvoidsDialogueV59(marker)) return false;
            for (int i = 0; i < _topLevelRegions.Count; i++)
            {
                Rect region = _topLevelRegions[i];
                if (region.width * region.height > CanvasWidth * CanvasHeight * .65f) continue;
                region.xMin -= 6; region.yMin -= 6; region.xMax += 6; region.yMax += 6;
                if (marker.Overlaps(region)) return false;
            }
            return true;
        }

        void DrawResourceCard()
        {
            var model = World.Model;
            Rect resources = ResourceCardBounds;
            Frame(resources, Night, Line, 1f); Region(resources);
            DrawIcon(new Rect(1108, 29, 48, 58), "water");
            Label(new Rect(1161, 29, 43, 39), model.PackedWater.ToString(), ColorText(_display, model.PackedWater >= model.RequiredDays ? Sea : MainText));
            Label(new Rect(1161, 75, 43, 21), "SU", _tiny);
            Fill(new Rect(1207, 39, 1, 58), Line);
            DrawIcon(new Rect(1213, 29, 48, 58), "food");
            Label(new Rect(1266, 29, 43, 39), model.PackedFood.ToString(), ColorText(_display, model.PackedFood >= model.RequiredDays ? Sea : MainText));
            Label(new Rect(1266, 75, 43, 21), "GIDA", _tiny);
            Fill(new Rect(1313, 39, 1, 58), Line);
            DrawBagIcon(new Rect(1334, 38, 32, 37), model.BagReady ? Sea : Muted);
            Label(new Rect(1322, 79, 65, 20), model.BagReady ? "HAZIR" : "ÇANTA", ColorText(_caps, model.BagReady ? Sea : Muted));
            Fill(new Rect(1399, 39, 1, 58), Line);
            FittedLabel(new Rect(1413, 29, 145, 41), model.InfiniteBudget ? "SINIRSIZ" : "$ " + model.Budget.ToString("N0"), _right, Brass, 16);
            Label(new Rect(1420, 77, 133, 21), "BÜTÇE", ColorText(_caps, Muted));
            DrawMaterialPool();
        }

        void Region(Rect rect) { _topLevelRegions.Add(rect); Mark(rect); }
        Vector2 LogicalPointer() { return (new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y) - _offset) / _scale; }
        static int UsedSlots(List<SupplySlot> slots)
        {
            int count = 0;
            if (slots != null) for (int i = 0; i < slots.Count; i++) if (HasStock(slots[i])) count++;
            return count;
        }
        static string WorkModeName(PanicWorkMode mode)
        {
            return mode == PanicWorkMode.Timing ? "ZAMANLAMA" : mode == PanicWorkMode.Balance ? "DENGE" : "VİDALAMA";
        }
        static string PhaseName(PanicPhase phase)
        {
            switch (phase)
            {
                case PanicPhase.Preparing: return "HAZIRLIK";
                case PanicPhase.Tutorial: return "ÖĞRETİCİ";
                case PanicPhase.Disaster: return "AFET";
                case PanicPhase.Aftermath: return "SONUÇ";
                case PanicPhase.Inspection: return "HASAR GEZİSİ";
                case PanicPhase.Success: return "BAŞARI";
                case PanicPhase.GameOver: return "KOŞU BİTTİ";
                default: return "HAZIRLIK";
            }
        }
        static string DisasterHint(DisasterType type)
        {
            switch (type)
            {
                case DisasterType.Earthquake: return "EŞYALARI SABİTLE";
                case DisasterType.Flood: return "SUYA KARŞI ÖNLEM AL";
                case DisasterType.Fire: return "YANGINA KARŞI HAZIRLAN";
                case DisasterType.Storm: return "ÇATIYI GÜÇLENDİR · GEVŞEK EŞYALARI SABİTLE";
                default: return "EVİNİ HAZIRLA";
            }
        }

        static void DrawWatch(Rect rect, Color color)
        {
            Rect face = new Rect(rect.x + rect.width * .10f, rect.y + rect.height * .18f, rect.width * .80f, rect.width * .80f);
            DrawRing(face, 1f, color, 2.5f);
            Fill(new Rect(rect.x + rect.width * .36f, rect.y + rect.height * .03f, rect.width * .28f, 4), color);
            Fill(new Rect(rect.center.x - 2, rect.y + rect.height * .06f, 4, rect.height * .10f), color);
            Vector2 center = face.center;
            DrawLine(center, center + new Vector2(0, -face.height * .27f), color, 2f);
            float angle = Time.unscaledTime * .3f;
            DrawLine(center, center + new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * face.width * .28f, color, 2f);
        }
        static void DrawRing(Rect rect, float value, Color color, float thickness)
        {
            const int pieces = 40;
            Vector2 center = rect.center;
            float radius = Mathf.Min(rect.width, rect.height) * .46f;
            int count = Mathf.RoundToInt(pieces * Mathf.Clamp01(value));
            for (int i = 0; i < count; i++)
            {
                float a = (i / (float)pieces) * Mathf.PI * 2f - Mathf.PI * .5f;
                float b = ((i + .84f) / pieces) * Mathf.PI * 2f - Mathf.PI * .5f;
                DrawLine(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, color, thickness);
            }
        }
        static void DrawLine(Vector2 a, Vector2 b, Color color, float thickness)
        {
            Vector2 delta = b - a;
            if (delta.sqrMagnitude < .001f) return;
            Matrix4x4 matrix = GUI.matrix;
            GUI.matrix = matrix * Matrix4x4.TRS(new Vector3(a.x, a.y, 0),
                Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg), Vector3.one);
            Fill(new Rect(0, -thickness * .5f, delta.magnitude, thickness), color);
            GUI.matrix = matrix;
        }
        static void DrawCheck(Rect rect, Color color)
        {
            PanicUiSymbolsV57.Draw(rect, "check", color);
        }
        static void DrawWarning(Rect rect, Color color)
        {
            PanicUiSymbolsV57.Draw(rect, "warning", color);
        }
        static void DrawDisasterSymbol(Rect rect, DisasterType type, Color color)
        {
            PanicUiSymbolsV57.Draw(rect, DisasterSymbolKeyV57(type), color);
        }
        static void DrawCorners(Rect rect, Color color)
        {
            const float len = 24f;
            Fill(new Rect(rect.x, rect.y, len, 2), color); Fill(new Rect(rect.x, rect.y, 2, len), color);
            Fill(new Rect(rect.xMax - len, rect.y, len, 2), color); Fill(new Rect(rect.xMax - 2, rect.y, 2, len), color);
            Fill(new Rect(rect.x, rect.yMax - 2, len, 2), color); Fill(new Rect(rect.x, rect.yMax - len, 2, len), color);
            Fill(new Rect(rect.xMax - len, rect.yMax - 2, len, 2), color); Fill(new Rect(rect.xMax - 2, rect.yMax - len, 2, len), color);
        }
        static void Brush(Rect rect, Color color)
        {
            // Heading surfaces stay dark while the shared accent is also used as readable text.
            // Keep the existing rectangle and its input/layout ownership; use one quiet accent rule.
            Fill(rect, Color.Lerp(Night, color, .08f));
            Fill(new Rect(rect.x, rect.y, 3f, rect.height), color);
        }
        static void DrawHazardStripe(Rect rect, Color color)
        {
            Fill(rect, new Color(color.r, color.g, color.b, .22f));
            for (float x = rect.x; x < rect.xMax; x += 18f)
                Fill(new Rect(x, rect.y, Mathf.Min(10f, rect.xMax - x), rect.height), color);
        }
        static void RoundFill(Rect rect, Color color)
        {
            if (rect.width <= 0f || rect.height <= 0f || color.a <= 0f) return;
            float radius = Mathf.Min(7f, Mathf.Min(rect.width, rect.height) * .5f);
            Color prior = GUI.color;
            GUI.color = Color.white;
            // Unity's native rounded-rectangle shader avoids seams between nine independently filtered pieces.
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, radius);
            GUI.color = prior;
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
                case "firstaid": return "İlk yardım";
                case "flashlight": return "Fener";
                case "battery": return "Pil";
                case "radio": return "Radyo";
                case "blanket": return "Battaniye";
                case "screws": return "Vida";
                case "bracket": return "Ankraj";
                case "plank": return "Tahta";
                case "sandbag": return "Kum torbası";
                case "seedling": return "Ağaç paketi";
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
                case DisasterType.Storm: return "FIRTINA";
                default: return "AFET";
            }
        }
    }
}
