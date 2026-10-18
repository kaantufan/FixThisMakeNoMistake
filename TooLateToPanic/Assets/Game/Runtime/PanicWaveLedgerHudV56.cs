using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Read-only presentation of the model's committed wave outcome.</summary>
    public sealed partial class PanicHomeHud
    {
        const float WaveLedgerRowsTopV56 = 310f;
        const float WaveLedgerRowsHeightV56 = 432f;
        const float WaveLedgerRowsWidthV56 = 870f;
        readonly List<int> waveLedgerPageStartsV56 = new List<int>();
        PanicWaveLedgerV56 waveLedgerAutoShownV56, waveLedgerPagedSnapshotV56;
        GUIStyle waveLedgerLabelStyleV56, waveLedgerDetailStyleV56;
        int waveLedgerPageV56, waveLedgerInputFrameV56 = -1, waveLedgerHotkeyFrameV56 = -1;
        int waveLedgerRepaintFrameV56 = -1;
        bool waveLedgerObservedOpenV56, waveLedgerDrawingV56;

        public bool WaveLedgerVisibleV56 { get { return World && World.Model != null && World.Model.WaveLedgerOpenV56; } }
        // The opening/closing frame belongs to the modal, including its Enter/Esc/click.
        public bool WaveLedgerInputBlockedV56 { get { return WaveLedgerVisibleV56 || waveLedgerInputFrameV56 == Time.frameCount; } }
        bool WaveLedgerButtonInputV56 { get { return waveLedgerDrawingV56 && waveLedgerInputFrameV56 != Time.frameCount; } }
        public Rect WaveLedgerPanelBoundsV56 { get { return new Rect(72f, 118f, 1456f, 730f); } }
        public Rect WaveLedgerContinueBoundsV56 { get { return new Rect(1124f, 772f, 376f, 50f); } }
        public Rect WaveLedgerPreviousBoundsV56 { get { return new Rect(100f, 772f, 170f, 50f); } }
        public Rect WaveLedgerNextBoundsV56 { get { return new Rect(526f, 772f, 170f, 50f); } }
        public Rect WaveLedgerReopenBoundsV56
        {
            get { return World && World.Model != null && World.Model.Phase == PanicPhase.Inspection
                ? new Rect(24f, 542f, 392f, 48f) : new Rect(318f, 714f, 942f, 34f); }
        }
        public int WaveLedgerDrawFramesV56 { get; private set; }
        public string WaveLedgerRenderedTextV56 { get; private set; }
        public int WaveLedgerPageV56 { get { return waveLedgerPageV56; } }
        public int WaveLedgerPageCountV56 { get { return Mathf.Max(1, waveLedgerPageStartsV56.Count); } }

        bool WaveLedgerCanOpenV56
        {
            get
            {
                if (!World || World.Model == null || World.Model.LastWaveLedgerV56 == null ||
                    !World.WaveLedgerMayOpenV56 || World.NotebookBlocksInput || KeyboardHelpBlockingV56 || _paused || _developerMenuOpen ||
                    World.WorkActive || World.InventoryOpen || World.IsMoving || FreeplayOnboardingInputBlockedV414)
                    return false;
                return World.Model.Phase == PanicPhase.Inspection || World.Model.Phase == PanicPhase.GameOver;
            }
        }

        public bool OpenWaveLedgerV56()
        {
            if (WaveLedgerVisibleV56) return true;
            if (!WaveLedgerCanOpenV56 || !World.Model.SetWaveLedgerOpenV56(true)) return false;
            waveLedgerAutoShownV56 = World.Model.LastWaveLedgerV56;
            waveLedgerObservedOpenV56 = true;
            waveLedgerPageV56 = 0;
            waveLedgerInputFrameV56 = Time.frameCount;
            CancelInventoryDrag();
            return true;
        }

        public void CloseWaveLedgerV56()
        {
            if (!World || World.Model == null || !World.Model.WaveLedgerOpenV56) return;
            World.Model.SetWaveLedgerOpenV56(false);
            waveLedgerObservedOpenV56 = false;
            waveLedgerInputFrameV56 = Time.frameCount;
        }

        void SynchronizeWaveLedgerV56()
        {
            if (!World || World.Model == null) return;
            var snapshot = World.Model.LastWaveLedgerV56;
            bool open = WaveLedgerVisibleV56;
            if (open != waveLedgerObservedOpenV56)
            {
                waveLedgerObservedOpenV56 = open;
                waveLedgerInputFrameV56 = Time.frameCount;
                if (open) { waveLedgerAutoShownV56 = snapshot; waveLedgerPageV56 = 0; }
            }
            if (snapshot == null)
            {
                waveLedgerAutoShownV56 = waveLedgerPagedSnapshotV56 = null;
                waveLedgerPageStartsV56.Clear();
                waveLedgerPageV56 = 0;
                return;
            }
            // Stable World phase means its cinematic and loss reveal have finished.
            // Opening is presentation only; model Tick owns the independent pause gate.
            if (!open && !ReferenceEquals(snapshot, waveLedgerAutoShownV56) && WaveLedgerCanOpenV56)
                OpenWaveLedgerV56();
        }

        bool ProcessWaveLedgerInputV56()
        {
            SynchronizeWaveLedgerV56();
            if (waveLedgerHotkeyFrameV56 == Time.frameCount) return WaveLedgerInputBlockedV56;
            waveLedgerHotkeyFrameV56 = Time.frameCount;
            if (!WaveLedgerVisibleV56) return WaveLedgerInputBlockedV56;
            if (waveLedgerInputFrameV56 == Time.frameCount) return true;
            foreach (KeyCode key in new[] { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Escape, KeyCode.Backspace,
                KeyCode.RightArrow, KeyCode.PageDown, KeyCode.LeftArrow, KeyCode.PageUp })
                if (Input.GetKeyDown(key) && ProcessWaveLedgerKeyV56(key)) return true;
            int direction = 0;
            if (direction == 0 && Mathf.Abs(Input.mouseScrollDelta.y) > .1f)
                direction = -(int)Mathf.Sign(Input.mouseScrollDelta.y);
            if (direction != 0) ChangeWaveLedgerPageV56(direction);
            return true;
        }

        /// <summary>The same semantic UI-key route used by the normal frame input.</summary>
        public bool ProcessWaveLedgerKeyV56(KeyCode key)
        {
            if (!WaveLedgerVisibleV56 || waveLedgerInputFrameV56 == Time.frameCount) return false;
            if (key == KeyCode.Return || key == KeyCode.KeypadEnter || key == KeyCode.Escape || key == KeyCode.Backspace)
            { CloseWaveLedgerV56(); return true; }
            if (key == KeyCode.RightArrow || key == KeyCode.PageDown)
            { ChangeWaveLedgerPageV56(1); return true; }
            if (key == KeyCode.LeftArrow || key == KeyCode.PageUp)
            { ChangeWaveLedgerPageV56(-1); return true; }
            return false;
        }

        public void ChangeWaveLedgerPageV56(int direction)
        { waveLedgerPageV56 = Mathf.Clamp(waveLedgerPageV56 + direction, 0, WaveLedgerPageCountV56 - 1); }

        void EnsureWaveLedgerStylesV56()
        {
            if (waveLedgerLabelStyleV56 != null) return;
            waveLedgerLabelStyleV56 = new GUIStyle(_body) { wordWrap = true, alignment = TextAnchor.UpperLeft };
            waveLedgerDetailStyleV56 = new GUIStyle(_small) { wordWrap = true, alignment = TextAnchor.UpperLeft };
        }

        float WaveLedgerRowHeightV56(PanicWaveLedgerRowV56 row)
        {
            float title = Mathf.Max(27f, waveLedgerLabelStyleV56.CalcHeight(new GUIContent(row.Label ?? ""), 680f));
            float detail = string.IsNullOrEmpty(row.Detail) ? 0f :
                Mathf.Max(22f, waveLedgerDetailStyleV56.CalcHeight(new GUIContent(row.Detail), 834f));
            return title + detail + 16f;
        }

        void EnsureWaveLedgerPagesV56(PanicWaveLedgerV56 snapshot)
        {
            if (ReferenceEquals(snapshot, waveLedgerPagedSnapshotV56)) return;
            waveLedgerPagedSnapshotV56 = snapshot;
            waveLedgerPageStartsV56.Clear();
            waveLedgerPageStartsV56.Add(0);
            float height = 0f;
            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                float rowHeight = WaveLedgerRowHeightV56(snapshot.Rows[i]);
                if (height > 0f && height + rowHeight > WaveLedgerRowsHeightV56)
                { waveLedgerPageStartsV56.Add(i); height = 0f; }
                height += rowHeight;
            }
            waveLedgerPageV56 = Mathf.Clamp(waveLedgerPageV56, 0, WaveLedgerPageCountV56 - 1);
        }

        void DrawWaveLedgerV56()
        {
            var snapshot = World.Model.LastWaveLedgerV56;
            if (snapshot == null) return;
            EnsureWaveLedgerStylesV56();
            EnsureWaveLedgerPagesV56(snapshot);
            waveLedgerDrawingV56 = true;
            try
            {
                Fill(LogicalCanvasBounds, new Color(.035f, .03f, .025f, .78f));
                Region(LogicalCanvasBounds); // Full canvas owns pointer input, including outside the panel.
                Frame(WaveLedgerPanelBoundsV56, Night, Line, 1f);
                DrawHazardStripe(new Rect(100f, 136f, 226f, 4f), Brass);
                Label(new Rect(100f, 147f, 1090f, 26f), "DALGA " + snapshot.Wave.ToString("00") + "  ·  " + snapshot.DisasterLabel, _tiny);
                Frame(new Rect(1240f, 144f, 260f, 32f), new Color(Petrol.r, Petrol.g, Petrol.b, .25f), Sea, 1f);
                Label(new Rect(1248f, 147f, 244f, 26f), "II  OYUN VE SÜRE DURDU", ColorText(_caps, Sea));
                Label(new Rect(100f, 177f, 1400f, 50f), "HASAR VE BÜTÇE DÖKÜMÜ", _title);
                FittedLabel(new Rect(100f, 231f, 1398f, 29f), snapshot.OutcomeLabel +
                    (snapshot.IsSimulation ? "  ·  SİMÜLASYON" : ""), _body, SoftText, 18);

                Label(new Rect(100f, 272f, 620f, 28f), "EŞYA VE TESİSAT KATKILARI", ColorText(_body, Brass));
                Label(new Rect(723f, 270f, 247f, 31f), snapshot.TotalDamage.ToString("0.0") + " % TOPLAM", ColorText(_right, snapshot.TotalDamage >= 75f ? Ember : Brass));
                var rendered = new StringBuilder();
                rendered.Append("DALGA ").Append(snapshot.Wave).Append(" · ").Append(snapshot.DisasterLabel).Append('\n')
                    .Append(snapshot.OutcomeLabel).Append("\nOYUN VE SÜRE DURDU\n");
                int first = waveLedgerPageStartsV56[waveLedgerPageV56];
                int end = waveLedgerPageV56 + 1 < waveLedgerPageStartsV56.Count ?
                    waveLedgerPageStartsV56[waveLedgerPageV56 + 1] : snapshot.Rows.Count;
                float y = WaveLedgerRowsTopV56;
                if (snapshot.Rows.Count == 0)
                {
                    string empty = snapshot.HasResolvedDamage ? "Bu dalgada hasar katkısı oluşmadı." :
                        "Hazırlık tamamlanamadığı için afet hasarı oluşmadı.";
                    Frame(new Rect(100f, y, WaveLedgerRowsWidthV56, 100f), Panel, Line, 1f);
                    Label(new Rect(118f, y + 25f, 834f, 52f), empty, waveLedgerDetailStyleV56);
                    rendered.Append(empty).Append('\n');
                }
                for (int i = first; i < end; i++)
                {
                    var row = snapshot.Rows[i];
                    float height = WaveLedgerRowHeightV56(row);
                    Frame(new Rect(100f, y, WaveLedgerRowsWidthV56, height - 4f), i % 2 == 0 ? Panel : Night, Line, 1f);
                    float titleHeight = Mathf.Max(27f, waveLedgerLabelStyleV56.CalcHeight(new GUIContent(row.Label ?? ""), 680f));
                    Label(new Rect(118f, y + 6f, 680f, titleHeight), row.Label, waveLedgerLabelStyleV56);
                    Label(new Rect(807f, y + 3f, 145f, 31f), row.DamageContribution.ToString("0.0") + " yp", ColorText(_right, row.DamageContribution > 0f ? Brass : Muted));
                    if (!string.IsNullOrEmpty(row.Detail))
                        Label(new Rect(118f, y + 7f + titleHeight, 834f, height - titleHeight - 16f), row.Detail, waveLedgerDetailStyleV56);
                    rendered.Append(row.Label).Append(" · ").Append(row.DamageContribution.ToString("0.0")).Append(" yp\n")
                        .Append(row.Detail).Append('\n');
                    y += height;
                }

                DrawWaveLedgerBudgetV56(snapshot, rendered);
                if (WaveLedgerPageCountV56 > 1)
                {
                    if (waveLedgerPageV56 > 0) Button(WaveLedgerPreviousBoundsV56, "ÖNCEKİ", false, Brass, () => ChangeWaveLedgerPageV56(-1));
                    else WaveLedgerDisabledButtonV56(WaveLedgerPreviousBoundsV56, "ÖNCEKİ");
                    if (waveLedgerPageV56 + 1 < WaveLedgerPageCountV56) Button(WaveLedgerNextBoundsV56, "SONRAKİ", false, Brass, () => ChangeWaveLedgerPageV56(1));
                    else WaveLedgerDisabledButtonV56(WaveLedgerNextBoundsV56, "SONRAKİ");
                }
                Label(new Rect(282f, 780f, 232f, 33f), "SAYFA " + (waveLedgerPageV56 + 1) + " / " + WaveLedgerPageCountV56, _center);
                Label(new Rect(712f, 773f, 388f, 46f), "yp = toplam hasara yüzde puan katkısı", waveLedgerDetailStyleV56);
                Button(WaveLedgerContinueBoundsV56, World.Model.Phase == PanicPhase.Inspection ? "HASAR GEZİSİNE DÖN" : "SONUCA DÖN", true, Sea, CloseWaveLedgerV56);
                Label(new Rect(100f, 826f, 1400f, 20f), "← → / TEKER: SAYFA   ·   ENTER / ESC: LİSTEYİ KAPAT   ·   Liste kapanınca süre ve kontrol devam eder.", _tiny);
                WaveLedgerRenderedTextV56 = rendered.ToString();
                if (Event.current.type == EventType.Repaint && waveLedgerRepaintFrameV56 != Time.frameCount)
                { waveLedgerRepaintFrameV56 = Time.frameCount; WaveLedgerDrawFramesV56++; }
                Event e = Event.current;
                if (e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.ScrollWheel ||
                    e.type == EventType.KeyDown || e.type == EventType.KeyUp) e.Use();
            }
            finally { waveLedgerDrawingV56 = false; }
        }

        void DrawWaveLedgerBudgetV56(PanicWaveLedgerV56 snapshot, StringBuilder rendered)
        {
            Frame(new Rect(1000f, 272f, 500f, 470f), Panel, Line, 1f);
            Label(new Rect(1020f, 285f, 460f, 32f), "BÜTÇE HESABI", ColorText(_heading, Brass));
            WaveLedgerMoneyRowV56("Sonuç öncesi bakiye", snapshot.BudgetBeforeReward, 330f, "", SoftText, rendered);
            WaveLedgerMoneyRowV56("Temel dalga ödülü", snapshot.BaseReward, 369f, "+", MainText, rendered);
            WaveLedgerMoneyRowV56("Hasardan ödül kesintisi", snapshot.DamageDeduction, 408f, "-", Rust, rendered);
            WaveLedgerMoneyRowV56("Çanta destek bonusu", snapshot.SupportBonus, 447f, "+", Sea, rendered);
            Fill(new Rect(1020f, 488f, 460f, 1f), Line);
            WaveLedgerMoneyRowV56("Hesaplanan ödül", snapshot.CalculatedReward, 496f, "", MainText, rendered);
            WaveLedgerMoneyRowV56("Bakiye sınırı kesintisi", snapshot.CapacityDeduction, 535f, "-", Muted, rendered);
            Frame(new Rect(1016f, 579f, 468f, 48f), new Color(Petrol.r, Petrol.g, Petrol.b, .28f), Sea, 1f);
            WaveLedgerMoneyRowV56("Hesaba geçen ödül", snapshot.ActualCredited, 586f, "+", Sea, rendered);
            WaveLedgerMoneyRowV56("SON BAKİYE", snapshot.BudgetAfterReward, 637f, "", Brass, rendered);
            string explanation = snapshot.RewardExplanation ?? "";
            Label(new Rect(1020f, 682f, 460f, 54f), explanation, waveLedgerDetailStyleV56);
            rendered.Append(explanation).Append('\n');
        }

        void WaveLedgerMoneyRowV56(string label, int value, float y, string sign, Color color, StringBuilder rendered)
        {
            Label(new Rect(1028f, y, 302f, 31f), label, ColorText(_small, color));
            string money = sign + "$ " + value.ToString("N0");
            FittedLabel(new Rect(1326f, y - 2f, 152f, 33f), money, _right, color, 16);
            rendered.Append(label).Append(" · ").Append(money).Append('\n');
        }

        void WaveLedgerDisabledButtonV56(Rect rect, string text)
        {
            Frame(rect, Night, Line, 1f);
            Label(new Rect(rect.x + 22f, rect.y + 2f, rect.width - 42f, rect.height - 4f), text, ColorText(_body, Muted));
            Mark(rect);
        }

        void DrawWaveLedgerReopenV56()
        {
            if (World.Model.LastWaveLedgerV56 == null || World.Model.LastWaveLedgerV56.Wave != World.Model.Wave) return;
            Button(WaveLedgerReopenBoundsV56, "HASAR VE BÜTÇE DÖKÜMÜ", false, Brass, () => OpenWaveLedgerV56());
        }
    }
}
