using UnityEngine;

namespace TooLateToPanic
{
    // Logical1600x900 layout; existing mechanic targets/input stay in their original places.
    public static class PanicWorkKeyHintLayoutV59
    {
        public static Rect HammerPanel { get { return new Rect(456, 620, 688, 218); } }
        public static Rect HammerHints { get { return new Rect(480, 750, 432, 34); } }
        public static Rect HammerNote { get { return new Rect(480, 790, 432, 26); } }
        public static Rect HammerProgress { get { return new Rect(480, 822, 432, 8); } }
        public static Rect MouseHint { get { return new Rect(1122, 320, 384, 60); } }
        public static Rect Alternative { get { return new Rect(1122, 679, 384, 36); } }
        public static Rect HammerCancel { get { return new Rect(934, 751, 184, 42); } }
        public static Rect DirectCancel { get { return new Rect(1122, 729, 384, 58); } }
        public static Rect[] HammerAreas { get { return new[] { HammerHints, HammerNote, HammerProgress, HammerCancel }; } }
        public static Rect[] DirectAreas { get { return new[] { MouseHint, Alternative, DirectCancel }; } }
        public static bool Disjoint(Rect a, Rect b) { return !a.Overlaps(b); }
    }

    public sealed partial class PanicHomeHud
    {
        GUIStyle workHintCapStyleV59, workHintTextStyleV59;
        public int WorkKeyHintsDrawFrameV59 { get; private set; } = -1;
        public string WorkKeyHintsRenderedTextV59 { get; private set; }
        public string WorkKeyHintsRenderedModeV59 { get; private set; }
        public Rect[] WorkKeyHintsRenderedBoundsV59 { get; private set; }
        void EnsureWorkHintStylesV59()
        {
            if (workHintCapStyleV59 != null) return;
            workHintCapStyleV59 = MakeStyle(_condensedFont, 21, FontStyle.Bold, Night, TextAnchor.MiddleCenter);
            workHintTextStyleV59 = MakeStyle(_condensedFont, 21, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
        }
        void WorkKeyCapV59(Rect rect, string key)
        {
            EnsureWorkHintStylesV59();
            Frame(new Rect(rect.x, rect.y + 3f, rect.width, rect.height), new Color(Brass.r * .45f, Brass.g * .45f, Brass.b * .45f), Brass, 1f);
            Fill(rect, Brass); Label(rect, key, workHintCapStyleV59);
        }
        void WorkMouseCapV59(Rect rect)
        {
            WorkKeyCapV59(rect, "");
            Rect mouse = new Rect(rect.center.x - 10, rect.y + 5, 20, rect.height - 10);
            Frame(mouse, Night, Night, 1f);
            Fill(new Rect(mouse.x + 2, mouse.y + 2, 7, mouse.height * .43f), MainText);
            DrawLine(new Vector2(mouse.center.x, mouse.y + 1), new Vector2(mouse.center.x, mouse.y + mouse.height * .5f), Brass, 1f);
            DrawLine(new Vector2(mouse.x + 1, mouse.y + mouse.height * .5f), new Vector2(mouse.xMax - 1, mouse.y + mouse.height * .5f), Brass, 1f);
        }
        void WorkArrowCapV59(Rect rect, bool down)
        {
            WorkKeyCapV59(rect, "");
            Vector2 direction = down ? Vector2.up : Vector2.right, side = new Vector2(-direction.y, direction.x);
            Vector2 tip = rect.center + direction * 9f;
            DrawLine(rect.center - direction * 10f, tip, Night, 2.5f);
            DrawLine(tip, tip - direction * 7f + side * 6f, Night, 2.5f);
            DrawLine(tip, tip - direction * 7f - side * 6f, Night, 2.5f);
        }
        void StampWorkKeyHintsV59(string text, bool hammer = false)
        {
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                WorkKeyHintsDrawFrameV59 = Time.frameCount; WorkKeyHintsRenderedTextV59 = text;
                WorkKeyHintsRenderedModeV59 = hammer ? "hammer-distinct-press" : World.Expedition.Work.UsesLinearDrag ? "power-down-hold" : "clockwise-hold";
                WorkKeyHintsRenderedBoundsV59 = hammer ? PanicWorkKeyHintLayoutV59.HammerAreas : PanicWorkKeyHintLayoutV59.DirectAreas;
            }
        }
        void DrawHammerKeyHintsV59()
        {
            EnsureWorkHintStylesV59(); Rect row = PanicWorkKeyHintLayoutV59.HammerHints;
            WorkMouseCapV59(new Rect(row.x, row.y, 36, 32));
            FittedLabel(new Rect(row.x + 46, row.y, 136, 32), "İŞARETE TIKLA", workHintTextStyleV59, MainText, 20);
            Label(new Rect(row.x + 190, row.y, 36, 32), "veya", ColorText(workHintTextStyleV59, Muted));
            WorkKeyCapV59(new Rect(row.x + 236, row.y, 88, 32), "SPACE");
            Label(new Rect(row.x + 338, row.y, 94, 32), "TEK TEK", workHintTextStyleV59);
            FittedLabel(PanicWorkKeyHintLayoutV59.HammerNote, "Her basış bir vuruş. Basılı tutmak tekrarlamaz.", workHintTextStyleV59, SoftText, 20);
            StampWorkKeyHintsV59("İŞARETLİ BAĞLANTIYA SOL TIK veya SPACE · TEK TEK BAS · Basılı tutmak tekrarlamaz · ESC iptal", true);
        }
        void DrawWorkMouseHintV59(PanicWorkSession work)
        {
            EnsureWorkHintStylesV59(); Rect row = PanicWorkKeyHintLayoutV59.MouseHint;
            WorkMouseCapV59(new Rect(row.x, row.y + 2, 48, 52));
            Label(new Rect(row.x + 65, row.y, row.width - 65, 28), "SOL TUŞU TUT", workHintTextStyleV59);
            FittedLabel(new Rect(row.x + 65, row.y + 29, row.width - 65, 28), work.UsesLinearDrag ? "AŞAĞI SÜRÜKLE" : "SAAT YÖNÜNDE ÇEVİR", workHintTextStyleV59, Brass, 20);
        }
        void DrawWorkAlternativeKeyV59(PanicWorkSession work)
        {
            EnsureWorkHintStylesV59(); Rect row = PanicWorkKeyHintLayoutV59.Alternative;
            WorkArrowCapV59(new Rect(row.x, row.y, 48, 32), work.UsesLinearDrag);
            FittedLabel(new Rect(row.x + 64, row.y, row.width - 64, 32), "ALTERNATİF · BASILI TUT", workHintTextStyleV59, SoftText, 20);
            StampWorkKeyHintsV59("SOL FARE TUŞU TUT · " + (work.UsesLinearDrag ? "AŞAĞI SÜRÜKLE · AŞAĞI OK basılı tut" : "SAAT YÖNÜNDE ÇEVİR · SAĞ OK basılı tut") + " · ESC iptal");
        }
        void DrawWorkCancelKeyV59(Rect rect)
        {
            // The existing Button remains the sole pointer action and retains its original hit rectangle.
            Button(rect, "", false, Line, World.CancelWork);
            EnsureWorkHintStylesV59();
            Label(new Rect(rect.x + 16, rect.y + 2, rect.width - 118, rect.height - 4), "İPTAL", workHintTextStyleV59);
            WorkKeyCapV59(new Rect(rect.xMax - 100, rect.center.y - 15, 60, 30), "ESC");
        }
    }
}
