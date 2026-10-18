using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TooLateToPanic
{
    // Root-owned optional headless Editor policy/font check. No scene/player/render/input work.
    public static class PanicWorkKeyHintsEditorProbeV59
    {
        [Serializable] public sealed class Receipt
        {
            public string layer = "HEADLESS_LAYOUT_AND_REAL_FONT_METRICS_ONLY", render = "NOT_RUN", osInput = "NOT_RUN";
            public bool passed = true; public int checks; public List<string> failures = new List<string>();
        }
        public static Receipt Probe()
        {
            var r = new Receipt();
            void Check(string name, bool value) { r.checks++; r.passed &= value; if (!value) r.failures.Add(name); }
            foreach (var rects in new[] { PanicWorkKeyHintLayoutV59.HammerAreas, PanicWorkKeyHintLayoutV59.DirectAreas })
                for (int i = 0; i < rects.Length; i++)
                {
                    for (int j = i + 1; j < rects.Length; j++) Check("nonoverlap." + i + "." + j, !rects[i].Overlaps(rects[j]));
                    foreach (float scale in new[] { .6f, 1f }) Check("canvas." + scale + "." + i, rects[i].xMin >= 0 && rects[i].xMax <= 1600 && rects[i].yMin >= 0 && rects[i].yMax <= 900 && 20f * scale >= 12f);
                }
            for (int nail = 0; nail < 4; nail++)
            {
                Rect hit = new Rect(480 + nail * 108, 704, 94, 42);
                foreach (var hint in PanicWorkKeyHintLayoutV59.HammerAreas) Check("unchanged-nail-clear." + nail, !hit.Overlaps(hint));
            }
            Check("sidebar-outside-work-image", PanicWorkKeyHintLayoutV59.MouseHint.xMin >= 1122 && PanicWorkKeyHintLayoutV59.Alternative.xMin >= 1122);
            Check("between-progress-and-cancel", PanicWorkKeyHintLayoutV59.Alternative.yMin >= 665 && PanicWorkKeyHintLayoutV59.Alternative.yMax <= 729);
            Font font = Resources.Load<Font>("HUD/BarlowCondensed"); Check("offline-font", font);
            if (font)
            {
                var style = new GUIStyle { font = font, fontSize = 21, fontStyle = FontStyle.Bold };
                Check("space-cap-fit", style.CalcSize(new GUIContent("SPACE")).x <= 88);
                Check("esc-cap-fit", style.CalcSize(new GUIContent("ESC")).x <= 60);
                style.fontSize = 20;
                Check("explicit-target-label-fit", style.CalcSize(new GUIContent("İŞARETE TIKLA")).x <= 136);
                Check("held-mouse-direction-fit", style.CalcSize(new GUIContent("SAAT YÖNÜNDE ÇEVİR")).x <= 319);
                Check("held-alternative-fit", style.CalcSize(new GUIContent("ALTERNATİF · BASILI TUT")).x <= 320);
                Check("no-repeat-note-fit", style.CalcSize(new GUIContent("Her basış bir vuruş. Basılı tutmak tekrarlamaz.")).x <= 432);
            }
            return r;
        }
        public static void Run()
        {
            if (!Application.isEditor || !Application.isBatchMode) throw new InvalidOperationException("Headless Editor only.");
            string output = null; string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--work-key-hints-v59-probe-out") output = args[i + 1];
            if (string.IsNullOrEmpty(output) || !Path.IsPathRooted(output)) throw new ArgumentException("Explicit absolute receipt path required.");
            var report = Probe(); File.WriteAllText(output, JsonUtility.ToJson(report, true));
            if (!report.passed) throw new InvalidOperationException("Work key hints policy/font probe failed.");
        }
    }
}
