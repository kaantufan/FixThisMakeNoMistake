using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Compact IMGUI presentation and interaction layer for Too Late to Panic!
    /// All game state remains owned by PanicModel/PanicWorld.
    /// </summary>
    public sealed class PanicHud : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private static readonly Color Ink = new Color(0.075f, 0.13f, 0.14f, 1f);
        private static readonly Color Paper = new Color(0.95f, 0.91f, 0.82f, 0.98f);
        private static readonly Color PaperLight = new Color(0.99f, 0.97f, 0.91f, 0.98f);
        private static readonly Color Teal = new Color(0.14f, 0.44f, 0.42f, 1f);
        private static readonly Color TealDark = new Color(0.10f, 0.32f, 0.32f, 1f);
        private static readonly Color Orange = new Color(0.91f, 0.43f, 0.16f, 1f);
        private static readonly Color Muted = new Color(0.37f, 0.42f, 0.39f, 1f);
        private static readonly Color Rule = new Color(0.18f, 0.27f, 0.26f, 0.22f);
        private static readonly string[] SupportNames = { "İlk yardım", "Fener", "Radyo", "Battaniye" };

        public PanicWorld World;

        private readonly List<Rect> _uiRects = new List<Rect>(12);
        private bool _bagOpen;
        private bool _scoresOpen;
        private bool _stylesReady;
        private float _scale = 1f;
        private Vector2 _canvasOffset;

        private GUIStyle _titleStyle;
        private GUIStyle _headingStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _eyebrowStyle;
        private GUIStyle _mutedStyle;
        private GUIStyle _lightStyle;
        private GUIStyle _buttonDarkStyle;
        private GUIStyle _buttonMutedStyle;
        private GUIStyle _clickStyle;
        private GUIStyle _statusCaptionStyle;

        /// <summary>World reads this before handling mouse selection to avoid UI click-through.</summary>
        public bool PointerOverUi
        {
            get
            {
                if (_uiRects.Count == 0 || _scale <= 0f)
                    return false;

                Vector3 mouse = Input.mousePosition;
                Vector2 guiPoint = new Vector2(
                    (mouse.x - _canvasOffset.x) / _scale,
                    (Screen.height - mouse.y - _canvasOffset.y) / _scale);

                for (int i = 0; i < _uiRects.Count; i++)
                {
                    if (_uiRects[i].Contains(guiPoint))
                        return true;
                }
                return false;
            }
        }

        /// <summary>Called by the world when the player uses the physical bag station.</summary>
        public void ShowBag()
        {
            _bagOpen = true;
        }

        private void OnGUI()
        {
            if (World == null || World.Model == null)
                return;

            ConfigureCanvas();
            EnsureStyles();
            _uiRects.Clear();

            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            Color previousContent = GUI.contentColor;
            Color previousBackground = GUI.backgroundColor;
            GUI.matrix = Matrix4x4.TRS(
                new Vector3(_canvasOffset.x, _canvasOffset.y, 0f),
                Quaternion.identity,
                new Vector3(_scale, _scale, 1f));
            GUI.color = Color.white;
            GUI.contentColor = Color.white;

            PanicPhase phase = World.Model.Phase;
            if (phase != PanicPhase.Preparing && phase != PanicPhase.Tutorial)
                _bagOpen = false;
            if (phase == PanicPhase.Title)
            {
                DrawTitle();
            }
            else
            {
                DrawStatusStrip();
                if (phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial)
                {
                    DrawSelectedItemPanel();
                    DrawPreparationFooter();
                    if (_bagOpen)
                        DrawBagPanel();
                }
                else if (phase == PanicPhase.Inspection)
                {
                    DrawInspectionPanel();
                }
                else if (phase == PanicPhase.Disaster || phase == PanicPhase.Aftermath || phase == PanicPhase.Success)
                {
                    DrawCutsceneOverlay(phase);
                }
                else if (phase == PanicPhase.GameOver)
                {
                    DrawGameOverPanel();
                }

            }

            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
            GUI.contentColor = previousContent;
            GUI.backgroundColor = previousBackground;
        }

        private void ConfigureCanvas()
        {
            float sx = Screen.width / ReferenceWidth;
            float sy = Screen.height / ReferenceHeight;
            _scale = Mathf.Min(sx, sy);
            if (_scale <= 0f)
                _scale = 1f;
            _canvasOffset = new Vector2(
                (Screen.width - ReferenceWidth * _scale) * 0.5f,
                (Screen.height - ReferenceHeight * _scale) * 0.5f);
        }

        private void EnsureStyles()
        {
            if (_stylesReady)
                return;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = GUI.skin.font;

            _titleStyle = MakeLabel(font, 52, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
            _headingStyle = MakeLabel(font, 28, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
            _bodyStyle = MakeLabel(font, 22, FontStyle.Normal, Ink, TextAnchor.MiddleLeft);
            _smallStyle = MakeLabel(font, 18, FontStyle.Normal, Ink, TextAnchor.MiddleLeft);
            _eyebrowStyle = MakeLabel(font, 16, FontStyle.Bold, TealDark, TextAnchor.MiddleLeft);
            _mutedStyle = MakeLabel(font, 18, FontStyle.Normal, Muted, TextAnchor.MiddleLeft);
            _lightStyle = MakeLabel(font, 20, FontStyle.Bold, PaperLight, TextAnchor.MiddleCenter);
            _buttonDarkStyle = MakeLabel(font, 18, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
            _buttonMutedStyle = MakeLabel(font, 17, FontStyle.Normal, Muted, TextAnchor.MiddleCenter);
            _statusCaptionStyle = MakeLabel(font, 16, FontStyle.Bold, new Color(.76f, .85f, .80f), TextAnchor.MiddleCenter);
            _lightStyle.wordWrap = false; _buttonDarkStyle.wordWrap = false; _buttonMutedStyle.wordWrap = false;
            _clickStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                fixedHeight = 0f,
                fixedWidth = 0f
            };
            SetButtonBackground(_clickStyle, Texture2D.whiteTexture);
            _stylesReady = true;
        }

        private static GUIStyle MakeLabel(Font font, int size, FontStyle fontStyle, Color color, TextAnchor anchor)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = size,
                fontStyle = fontStyle,
                alignment = anchor,
                wordWrap = true,
                richText = false,
                clipping = TextClipping.Clip
            };
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            style.padding = new RectOffset(0, 0, 0, 0);
            style.margin = new RectOffset(0, 0, 0, 0);
            return style;
        }

        private static void SetButtonBackground(GUIStyle style, Texture2D texture)
        {
            style.normal.background = texture;
            style.hover.background = texture;
            style.active.background = texture;
            style.focused.background = texture;
            style.onNormal.background = texture;
            style.onHover.background = texture;
            style.onActive.background = texture;
            style.onFocused.background = texture;
        }

        private void DrawTitle()
        {
            Rect card = new Rect(66f, 80f, 600f, 916f);
            DrawPanel(card, Paper);
            MarkUi(card);

            Label(new Rect(112f, 124f, 510f, 34f), "EVDEKİ SON HAZIRLIK", _eyebrowStyle);
            Label(new Rect(108f, 165f, 520f, 142f), "Too Late\nto Panic!", _titleStyle);
            DrawRule(new Rect(112f, 326f, 505f, 2f));
            Label(new Rect(112f, 351f, 490f, 96f),
                "Afet yaklaşırken sınırlı zamanını ve bütçeni doğru önlemlere ayır.", _bodyStyle);

            if (DrawButton(new Rect(112f, 486f, 500f, 72f), "Oyuna başla", true, true))
            {
                _bagOpen = false;
                _scoresOpen = false;
                World.StartGame(false);
            }
            if (DrawButton(new Rect(112f, 574f, 500f, 66f), "Uygulamalı öğretici", true, false))
            {
                _bagOpen = false;
                _scoresOpen = false;
                World.StartGame(true);
            }
            if (DrawButton(new Rect(112f, 656f, 500f, 66f), _scoresOpen ? "Yerel puanları gizle" : "Yerel puanlar", true, false))
                _scoresOpen = !_scoresOpen;
            if (DrawButton(new Rect(112f, 738f, 500f, 66f), "Oyundan çık", true, false))
                Application.Quit();

            Label(new Rect(112f, 848f, 500f, 72f), "WASD ile yürü · Fareyle eşyaları incele · Hazırlık sırasında çantanı kontrol et", _mutedStyle);

            if (_scoresOpen)
                DrawHighscores(new Rect(1325f, 160f, 520f, 730f));
            else
            {
                Rect note = new Rect(1360f, 790f, 450f, 145f);
                DrawPanel(note, PaperLight);
                MarkUi(note);
                Label(new Rect(note.x + 28f, note.y + 24f, note.width - 56f, 34f), "ÜÇ AFET · TEK EV", _eyebrowStyle);
                Label(new Rect(note.x + 28f, note.y + 63f, note.width - 56f, 64f), "Deprem, sel ve yangına karşı her dalgada yeni bir plan kur.", _smallStyle);
            }
        }

        private void DrawStatusStrip()
        {
            Rect strip = new Rect(42f, 28f, 1836f, 112f);
            Fill(strip, Ink);
            DrawBorder(strip, new Color(0.36f, 0.51f, 0.47f, 1f), 2f);
            MarkUi(strip);

            PanicModel model = World.Model;
            DrawStatusCell(new Rect(72f, 43f, 280f, 76f), "DALGA", model.Wave.ToString("00"), "Tamamlanan " + model.CompletedWaves);
            DrawStatusCell(new Rect(394f, 43f, 430f, 76f), "YAKLAŞAN AFET", DisasterName(model.Disaster), PhaseName(model.Phase));
            DrawStatusCell(new Rect(884f, 43f, 350f, 76f), "KALAN SÜRE", DisplayTime(model), TimeCaption(model));
            DrawStatusCell(new Rect(1285f, 43f, 280f, 76f), "BÜTÇE", model.Budget.ToString("N0"), "Birim");
            if (model.Phase == PanicPhase.Preparing && model.TimeRemaining <= 60f)
                Fill(new Rect(884f, 124f, 350f, 5f), Orange);

            bool bagPhase = model.Phase == PanicPhase.Preparing || model.Phase == PanicPhase.Tutorial;
            if (bagPhase)
            {
                if (DrawButton(new Rect(1595f, 48f, 234f, 64f), _bagOpen ? "Çantayı kapat" : "ÇANTAYI AÇ", true, true))
                    _bagOpen = !_bagOpen;
            }
            else if (model.Phase == PanicPhase.Inspection)
            {
                if (DrawButton(new Rect(1595f, 48f, 234f, 64f), "HAZIRIM", true, true))
                    World.FinishInspection();
            }
            else if (model.Phase != PanicPhase.GameOver)
            {
                if (DrawButton(new Rect(1595f, 48f, 234f, 64f), "MENÜ", true, false))
                    World.ReturnToMenu();
            }
        }

        private void DrawStatusCell(Rect rect, string caption, string value, string detail)
        {
            Label(new Rect(rect.x, rect.y, rect.width, 22f), caption, _statusCaptionStyle);
            Label(new Rect(rect.x, rect.y + 22f, rect.width, 36f), value, _lightStyle);
            Label(new Rect(rect.x, rect.y + 57f, rect.width, 20f), detail, MakeMutedLightStyle());
        }

        private GUIStyle _mutedLightStyle;
        private GUIStyle MakeMutedLightStyle()
        {
            if (_mutedLightStyle == null)
                _mutedLightStyle = MakeLabel(_lightStyle.font, 15, FontStyle.Normal, new Color(0.80f, 0.86f, 0.82f, 1f), TextAnchor.MiddleCenter);
            return _mutedLightStyle;
        }

        private string TimeCaption(PanicModel model)
        {
            if (model.Phase == PanicPhase.Preparing && model.TimeRemaining <= 60f)
                return "Son dakika · çantayı kontrol et";
            if (model.Phase == PanicPhase.Preparing)
                return "Çanta " + (Mathf.Min(model.PackedWater, model.PackedFood) * 24) + "/" + (model.RequiredDays * 24) + " saat";
            if (model.Phase == PanicPhase.Tutorial)
                return "Öğretici · deneme kaynakları";
            if (model.Phase == PanicPhase.Disaster)
                return "Afet sırası";
            if (model.Phase == PanicPhase.Aftermath)
                return "Afet sonrası";
            if (model.Phase == PanicPhase.Success)
                return "Başarı sahnesi";
            if (model.Phase == PanicPhase.Inspection)
                return "Hasar incelemesi";
            if (model.Phase == PanicPhase.GameOver)
                return "Koşu sona erdi";
            return "Hazırlık süresi";
        }

        private static string DisplayTime(PanicModel model)
        {
            if (model.Phase == PanicPhase.Preparing || model.Phase == PanicPhase.Tutorial)
                return FormatTime(model.TimeRemaining);
            if (model.Phase == PanicPhase.GameOver)
                return "--:--";
            return FormatTime(model.PhaseTime);
        }

        private void DrawSelectedItemPanel()
        {
            Rect panel = new Rect(42f, 164f, 426f, 672f);
            DrawPanel(panel, Paper);
            MarkUi(panel);

            Label(new Rect(72f, 186f, 360f, 30f), World.Model.IsTutorial ? "ÖĞRETİCİ · EŞYA İŞLEMLERİ" : "EVİ HAZIRLA", _eyebrowStyle);
            PanicItem item = World.Selected;
            if (item == null)
            {
                Label(new Rect(72f, 246f, 350f, 74f), "Bir eşyayı incelemek için fareyle seç.", _headingStyle);
                Label(new Rect(72f, 338f, 342f, 88f), "Duvar, kapı, çatı, pencere veya mobilya seçerek geçerli önlemleri gör.", _mutedStyle);
                DrawRule(new Rect(72f, 442f, 352f, 2f));
                Label(new Rect(72f, 466f, 348f, 100f), "Uygun nesneye yaklaşınca işlemler açılır. Taşıma ve sabitleme durumu her dalgada korunur.", _smallStyle);
            }
            else
            {
                Label(new Rect(72f, 235f, 350f, 58f), string.IsNullOrEmpty(item.Label) ? ItemName(item.Kind) : item.Label, _headingStyle);
                Label(new Rect(72f, 296f, 350f, 30f), ItemName(item.Kind) + "  ·  Kademe " + item.Tier, _mutedStyle);
                Label(new Rect(72f, 342f, 352f, 58f), ItemState(item), _bodyStyle);
                DrawRule(new Rect(72f, 413f, 352f, 2f));

                bool itemNear = World.CanInteract;
                bool structural = item.Kind == ItemKind.Wall || item.Kind == ItemKind.Door || item.Kind == ItemKind.Roof || item.Kind == ItemKind.Window;
                if (!item.Installed)
                    DrawActionRow(432f, "Pencere tak", 100, itemNear && World.Model.Budget >= 100, () => World.Act("installwindow"));
                else if (structural && item.Tier < 2)
                {
                    int cost = item.Tier == 0 ? 150 : 250;
                    DrawActionRow(432f, "Güçlendir", cost, itemNear && World.Model.Budget >= cost, () => World.Act("upgrade"));
                }
                if (item.Movable)
                {
                    if (World.IsMoving)
                    {
                        DrawActionRow(502f, "Taşımayı iptal et", 0, true, () => World.CancelMove());
                    }
                    else if (item.Anchored)
                    {
                        DrawActionRow(502f, "Sabitlemeyi kaldır", 0, itemNear, () => World.Act("anchor"));
                    }
                    else
                    {
                        DrawActionRow(502f, "Sabitle", item.AnchorOwned ? 0 : 50, itemNear && (item.AnchorOwned || World.Model.Budget >= 50), () => World.Act("anchor"));
                    }

                    if (!item.Anchored && !World.IsMoving)
                        DrawActionRow(572f, "Taşı", 0, itemNear, () => World.BeginMove());
                }
                if (item.Installed && item.Kind == ItemKind.Window && !item.FloodProtected)
                    DrawActionRow(642f, "Sel koruması", 150, itemNear && World.Model.Budget >= 150, () => World.Act("flood"));
                else if (item.FloodProtected && item.Kind == ItemKind.Window)
                    DrawActionRow(642f, "Sel koruması hazır", 0, false, null);

                if (item.Installed && item.Id != "bunker" && item.Id != "bagstation" && !item.FireProtected)
                    DrawActionRow(712f, "Yangın koruması", 150, itemNear && World.Model.Budget >= 150, () => World.Act("fire"));
            }

            if (!World.CanInteract && item != null && !string.IsNullOrEmpty(World.InteractionHint))
                Label(new Rect(72f, 780f, 350f, 38f), World.InteractionHint, _mutedStyle);
        }

        private void DrawActionRow(float y, string label, int cost, bool enabled, System.Action action)
        {
            string price = cost <= 0 ? "0 birim" : cost.ToString("N0") + " birim";
            Label(new Rect(72f, y, 176f, 52f), label, _smallStyle);
            Label(new Rect(252f, y, 84f, 52f), price, _eyebrowStyle);
            if (DrawButton(new Rect(328f, y + 4f, 108f, 44f), "UYGULA", enabled, true) && action != null)
                action();
        }

        private void DrawPreparationFooter()
        {
            Rect footer = new Rect(492f, 872f, 1390f, 154f);
            DrawPanel(footer, PaperLight);
            MarkUi(footer);

            string hint = World.IsMoving
                ? "Taşıma: fareyle geçerli konumu seç · R ile 90° döndür · sağ tık veya Esc ile iptal"
                : World.Model.Phase == PanicPhase.Tutorial
                    ? "ÖĞRETİCİ: Afeti oku · eşyayı seç · bir önlem dene · çantayı tamamla · bunkera gir"
                : "WASD ile yürü · Fareyle eşyayı seç · Önlemleri panelden uygula · E ile yakındaki bunkera gir";
            Label(new Rect(526f, 893f, 990f, 42f), hint, _bodyStyle);
            string feedback = !string.IsNullOrEmpty(World.Model.Message)
                ? World.Model.Message
                : !string.IsNullOrEmpty(World.PlacementMessage) ? World.PlacementMessage : World.InteractionHint;
            if (!string.IsNullOrEmpty(feedback))
                Label(new Rect(526f, 938f, 990f, 36f), feedback, _mutedStyle);

            if (World.Model.Phase == PanicPhase.Tutorial)
            {
                if (DrawButton(new Rect(1538f, 910f, 300f, 64f), "ÖĞRETİCİYİ BİTİR", true, true))
                    World.FinishTutorial();
            }
            else
            {
                if (DrawButton(new Rect(1538f, 910f, 300f, 64f), "SIĞINAĞA GİR", true, true))
                    World.Act("bunker");
            }
        }

        private void DrawBagPanel()
        {
            Rect panel = new Rect(1472f, 164f, 406f, 672f);
            DrawPanel(panel, Paper);
            MarkUi(panel);
            PanicModel model = World.Model;

            Label(new Rect(1502f, 184f, 340f, 42f), "AFET ÇANTASI", _headingStyle);
            Label(new Rect(1502f, 232f, 340f, 32f), "Gerekli: " + (model.RequiredDays * 24) + " saat", _eyebrowStyle);
            int supportedHours = Mathf.Min(model.PackedWater, model.PackedFood) * 24;
            Label(new Rect(1502f, 265f, 340f, 52f), "Çantan: " + supportedHours + " saat · " + (model.BagReady ? "YETERLİ" : "EKSİK"), model.BagReady ? _bodyStyle : _mutedStyle);
            DrawRule(new Rect(1502f, 326f, 340f, 2f));

            Label(new Rect(1502f, 344f, 340f, 28f), "SU · " + model.PackedWater + "/3 çantada  ·  " + model.WaterStock + " stokta", _smallStyle);
            DrawButton(new Rect(1502f, 378f, 158f, 48f), "Al · 50", model.Budget >= 50, false, () => World.Act("water"));
            DrawButton(new Rect(1684f, 378f, 158f, 48f), "Çantaya koy", model.WaterStock > 0 && model.PackedWater < 3, true, () => World.Act("packwater"));

            Label(new Rect(1502f, 444f, 340f, 28f), "GIDA · " + model.PackedFood + "/3 çantada  ·  " + model.FoodStock + " stokta", _smallStyle);
            DrawButton(new Rect(1502f, 478f, 158f, 48f), "Al · 50", model.Budget >= 50, false, () => World.Act("food"));
            DrawButton(new Rect(1684f, 478f, 158f, 48f), "Çantaya koy", model.FoodStock > 0 && model.PackedFood < 3, true, () => World.Act("packfood"));

            DrawRule(new Rect(1502f, 543f, 340f, 2f));
            Label(new Rect(1502f, 557f, 340f, 28f), "DESTEK · " + CountSupportBits(model.PackedSupportMask) + "/4 · KATEGORİ BAŞI +%5", _eyebrowStyle);
            for (int i = 0; i < 4; i++)
            {
                int mask = 1 << i;
                bool owned = (model.SupportOwnedMask & mask) != 0;
                bool packed = (model.PackedSupportMask & mask) != 0;
                float y = 594f + i * 52f;
                int supportIndex = i;
                Label(new Rect(1502f, y, 154f, 42f), SupportNames[i] + (packed ? " · Çantada" : owned ? " · Alındı" : ""), _smallStyle);
                if (!owned)
                    DrawButton(new Rect(1684f, y, 158f, 42f), "Al · 100", model.Budget >= 100, false, () => World.Act("support" + supportIndex));
                else if (!packed)
                    DrawButton(new Rect(1684f, y, 158f, 42f), "Çantaya koy", true, true, () => World.Act("packsupport" + supportIndex));
                else
                    DrawButton(new Rect(1684f, y, 158f, 42f), "HAZIR", false, true);
            }

            Label(new Rect(1502f, 810f, 340f, 25f), "Satın alma ve paketleme için istasyona yaklaş.", _mutedStyle);
        }

        private void DrawInspectionPanel()
        {
            Rect panel = new Rect(42f, 164f, 486f, 672f);
            DrawPanel(panel, Paper);
            MarkUi(panel);
            PanicModel model = World.Model;

            Label(new Rect(72f, 184f, 426f, 34f), "DALGA SONU İNCELEMESİ", _eyebrowStyle);
            Label(new Rect(72f, 226f, 426f, 58f), "Hasar " + model.TotalDamage.ToString("0.#") + "%", _headingStyle);
            Label(new Rect(72f, 286f, 426f, 34f), "En yüksek hasar kaynakları", _bodyStyle);
            DrawRule(new Rect(72f, 330f, 410f, 2f));

            int first = -1, second = -1, third = -1;
            List<PanicItem> items = model.Items;
            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == null || items[i].Contribution <= 0f)
                        continue;
                    if (first < 0 || items[i].Contribution > items[first].Contribution)
                    {
                        third = second; second = first; first = i;
                    }
                    else if (second < 0 || items[i].Contribution > items[second].Contribution)
                    {
                        third = second; second = i;
                    }
                    else if (third < 0 || items[i].Contribution > items[third].Contribution)
                    {
                        third = i;
                    }
                }
            }
            DrawCauseLine(1, first, items, 352f);
            DrawCauseLine(2, second, items, 406f);
            DrawCauseLine(3, third, items, 460f);

            Label(new Rect(72f, 530f, 410f, 86f), string.IsNullOrEmpty(model.Message) ? "Hasar izlerini odada gezerek incele. Yeni dalgada hazırlık başlamadan önce ev toparlanır." : model.Message, _mutedStyle);
            Label(new Rect(72f, 634f, 410f, 74f), "Ödül: " + model.LastReward.ToString("N0") + " bütçe  ·  Puan: " + model.LastWaveScore.ToString("N0"), _smallStyle);
            if (DrawButton(new Rect(72f, 746f, 410f, 64f), "HAZIRIM · SONRAKİ DALGA", true, true))
                World.FinishInspection();
        }

        private void DrawCauseLine(int rank, int index, List<PanicItem> items, float y)
        {
            if (index < 0 || items == null || index >= items.Count)
            {
                Label(new Rect(72f, y, 410f, 42f), rank + "  —", _mutedStyle);
                return;
            }
            PanicItem item = items[index];
            string name = string.IsNullOrEmpty(item.Label) ? ItemName(item.Kind) : item.Label;
            Label(new Rect(72f, y, 290f, 42f), rank + "  " + name, _smallStyle);
            Label(new Rect(362f, y, 120f, 42f), item.Contribution.ToString("0.#") + "% puan", _eyebrowStyle);
        }

        private void DrawCutsceneOverlay(PanicPhase phase)
        {
            Rect overlay = new Rect(658f, 356f, 604f, 330f);
            DrawPanel(overlay, PaperLight);
            MarkUi(overlay);
            Label(new Rect(696f, 385f, 528f, 34f), "DALGA " + World.Model.Wave.ToString("00") + " · " + DisasterName(World.Model.Disaster).ToUpperInvariant(), _eyebrowStyle);
            Label(new Rect(696f, 432f, 528f, 72f), CutsceneTitle(phase), _headingStyle);
            string detail = !string.IsNullOrEmpty(World.Model.Message) ? World.Model.Message : CutsceneDetail(phase);
            Label(new Rect(696f, 518f, 528f, 92f), detail, _bodyStyle);
            if (phase == PanicPhase.Success)
                Label(new Rect(696f, 620f, 528f, 36f), "+" + World.Model.LastReward.ToString("N0") + " bütçe  ·  " + World.Model.LastWaveScore.ToString("N0") + " puan", _eyebrowStyle);
        }

        private void DrawGameOverPanel()
        {
            Rect panel = new Rect(658f, 290f, 604f, 506f);
            DrawPanel(panel, PaperLight);
            MarkUi(panel);
            Label(new Rect(696f, 320f, 528f, 34f), "KOŞU SONA ERDİ", _eyebrowStyle);
            Label(new Rect(696f, 365f, 528f, 74f), "Bir dahaki sefere daha erken hazırlan.", _headingStyle);
            string reason = !string.IsNullOrEmpty(World.Model.FailureReason) ? World.Model.FailureReason : World.Model.Message;
            Label(new Rect(696f, 456f, 528f, 92f), string.IsNullOrEmpty(reason) ? "Afet hazırlığını tamamlayamadın." : reason, _bodyStyle);
            Label(new Rect(696f, 557f, 528f, 76f), "Tamamlanan dalga: " + World.Model.CompletedWaves + "  ·  Hasar: " + World.Model.TotalDamage.ToString("0.#") + "%\nToplam puan: " + World.Model.Score.ToString("N0"), _smallStyle);
            if (DrawButton(new Rect(696f, 656f, 252f, 62f), "YENİDEN DENE", true, true))
            {
                _bagOpen = false;
                World.StartGame(false);
            }
            if (DrawButton(new Rect(972f, 656f, 252f, 62f), "ANA MENÜ", true, false))
            {
                _bagOpen = false;
                World.ReturnToMenu();
            }
        }

        private void DrawHighscores(Rect panel)
        {
            DrawPanel(panel, PaperLight);
            MarkUi(panel);
            Label(new Rect(panel.x + 34f, panel.y + 28f, panel.width - 68f, 44f), "YEREL PUANLAR", _headingStyle);
            Label(new Rect(panel.x + 34f, panel.y + 82f, panel.width - 68f, 32f), "DALGA ÖNCELİKLİ SIRALAMA", _eyebrowStyle);
            DrawRule(new Rect(panel.x + 34f, panel.y + 130f, panel.width - 68f, 2f));

            List<HighscoreEntry> scores = World.Model.Highscores;
            if (scores == null || scores.Count == 0)
            {
                Label(new Rect(panel.x + 34f, panel.y + 174f, panel.width - 68f, 86f), "Henüz kayıtlı koşu yok. İlk dalgadan sonra skorun burada görünür.", _bodyStyle);
                return;
            }

            int count = Mathf.Min(10, scores.Count);
            for (int i = 0; i < count; i++)
            {
                HighscoreEntry entry = scores[i];
                float y = panel.y + 158f + i * 48f;
                Label(new Rect(panel.x + 34f, y, 52f, 38f), (i + 1).ToString("00"), _eyebrowStyle);
                Label(new Rect(panel.x + 96f, y, 200f, 38f), entry.waves + " dalga", _smallStyle);
                Label(new Rect(panel.x + 300f, y, panel.width - 334f, 38f), entry.score.ToString("N0") + " puan", _smallStyle);
            }
        }

        private void DrawPanel(Rect rect, Color color)
        {
            Fill(rect, color);
            DrawBorder(rect, new Color(Ink.r, Ink.g, Ink.b, 0.75f), 2f);
            DrawBorder(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f), Rule, 1f);
        }

        private void DrawRule(Rect rect)
        {
            Fill(rect, Rule);
        }

        private void DrawBorder(Rect rect, Color color, float thickness)
        {
            Fill(new Rect(rect.x, rect.y, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.y, thickness, rect.height), color);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void Label(Rect rect, string text, GUIStyle style)
        {
            GUI.Label(rect, text ?? string.Empty, style);
        }

        private bool DrawButton(Rect rect, string text, bool enabled, bool primary, System.Action action = null)
        {
            Vector2 pointer = CanvasPointer;
            bool hovered = rect.Contains(pointer);
            Color color;
            if (!enabled)
                color = new Color(0.72f, 0.73f, 0.68f, 0.72f);
            else if (primary)
                color = hovered ? TealDark : Teal;
            else
                color = hovered ? new Color(0.83f, 0.86f, 0.78f, 1f) : new Color(0.88f, 0.87f, 0.78f, 1f);

            Fill(rect, color);
            DrawBorder(rect, primary ? TealDark : Rule, 1.5f);
            bool pressed = false;
            bool oldEnabled = GUI.enabled;
            Color oldBackground = GUI.backgroundColor;
            Color oldContent = GUI.contentColor;
            GUI.enabled = enabled;
            GUI.backgroundColor = new Color(1f, 1f, 1f, 0f);
            GUI.contentColor = Color.clear;
            pressed = GUI.Button(rect, GUIContent.none, _clickStyle);
            GUI.enabled = oldEnabled;
            GUI.backgroundColor = oldBackground;
            GUI.contentColor = oldContent;

            Label(rect, text, primary && enabled ? _lightStyle : enabled ? _buttonDarkStyle : _buttonMutedStyle);
            if (pressed && enabled && action != null)
                action();
            return pressed && enabled;
        }

        private Vector2 CanvasPointer
        {
            get
            {
                Vector3 mouse = Input.mousePosition;
                return new Vector2(
                    (mouse.x - _canvasOffset.x) / _scale,
                    (Screen.height - mouse.y - _canvasOffset.y) / _scale);
            }
        }

        private void MarkUi(Rect rect)
        {
            _uiRects.Add(rect);
        }

        private static string FormatTime(float seconds)
        {
            int whole = Mathf.Max(0, Mathf.CeilToInt(seconds));
            return (whole / 60).ToString("00") + ":" + (whole % 60).ToString("00");
        }

        private static int CountSupportBits(int mask)
        {
            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                if ((mask & (1 << i)) != 0)
                    count++;
            }
            return count;
        }

        private static string DisasterName(DisasterType type)
        {
            switch (type)
            {
                case DisasterType.Earthquake: return "Deprem";
                case DisasterType.Flood: return "Sel";
                case DisasterType.Fire: return "Yangın";
                default: return "Afet";
            }
        }

        private static string PhaseName(PanicPhase phase)
        {
            switch (phase)
            {
                case PanicPhase.Tutorial: return "Uygulamalı öğretici";
                case PanicPhase.Preparing: return "Hazırlık sürüyor";
                case PanicPhase.Disaster: return "Afet yaşanıyor";
                case PanicPhase.Aftermath: return "Hasar değerlendiriliyor";
                case PanicPhase.Success: return "Dalga geçildi";
                case PanicPhase.Inspection: return "Hasar incelemesi";
                case PanicPhase.GameOver: return "Koşu sona erdi";
                default: return "Başlangıç";
            }
        }

        private string CutsceneTitle(PanicPhase phase)
        {
            switch (phase)
            {
                case PanicPhase.Disaster: return DisasterName(World.Model.Disaster) + " sırası";
                case PanicPhase.Aftermath: return "Afet sonrası";
                case PanicPhase.Success: return "Bu dalgayı atlattın!";
                default: return string.Empty;
            }
        }

        private string CutsceneDetail(PanicPhase phase)
        {
            switch (phase)
            {
                case PanicPhase.Disaster:
                    return "Bunkerda güvendesin. Afet etkisi kısa süre içinde sona erecek.";
                case PanicPhase.Aftermath:
                    return "Eve dönerken hasar izlerini incele. Ardından yeni dalga için hazırlan.";
                case PanicPhase.Success:
                    return "Hazırlığın ve çantan bu dalgayı geçmene yardım etti.";
                default:
                    return string.Empty;
            }
        }

        private static string ItemName(ItemKind kind)
        {
            switch (kind)
            {
                case ItemKind.Furniture: return "Mobilya";
                case ItemKind.Wall: return "Duvar";
                case ItemKind.Door: return "Kapı";
                case ItemKind.Roof: return "Çatı";
                case ItemKind.Window: return "Pencere";
                default: return "Nesne";
            }
        }

        private static string ItemState(PanicItem item)
        {
            string state = item.Movable ? (item.Anchored ? "Sabitlenmiş" : "Taşınabilir") : "Yapı parçası";
            if (item.FloodProtected)
                state += " · Sel korumalı";
            if (item.FireProtected)
                state += " · Yangın korumalı";
            return state;
        }
    }
}
