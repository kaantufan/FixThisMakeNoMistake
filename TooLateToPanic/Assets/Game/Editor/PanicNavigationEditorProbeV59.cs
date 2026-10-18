using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TooLateToPanic
{
    // Standalone hook for a root-owned headless Editor invocation. No scene/player/render/input work.
    // Put this file under Assets/Editor, then -executeMethod TooLateToPanic.PanicNavigationEditorProbeV59.Run
    // with --navigation-v59-probe-out <absolute receipt path>. This worker has NOT run the hook.
    public static class PanicNavigationEditorProbeV59
    {
        [Serializable] public sealed class Row { public string id, detail; public bool pass; }
        [Serializable] public sealed class Receipt
        {
            public string layer = "HEADLESS_EDITOR_READ_ONLY_PRODUCTION_POLICY_AND_LAYOUT_ARITHMETIC";
            public string render = "NOT_RUN", osInput = "NOT_RUN", humanAcceptance = "NOT_RUN";
            public bool passed = true;
            public List<Row> checks = new List<Row>();
        }
        public static Receipt Probe()
        {
            var r = new Receipt();
            void Check(string id, bool pass, string detail)
            { r.checks.Add(new Row { id = id, pass = pass, detail = detail }); r.passed &= pass; }
            Check("title.normal", PanicHomeHud.TitleChoiceLabelV414(0) == "OYNA", "Actual production normal-play label.");
            Check("title.other-modes", PanicHomeHud.TitleChoiceLabelV414(1) == "KISA OYUN" && PanicHomeHud.TitleChoiceLabelV414(2) == "AFETİ DENE" &&
                PanicHomeHud.TitleChoiceLabelV414(3) == "NASIL OYNANIR", "Existing separate short/simulation/tutorial labels.");
            foreach (PanicPhase phase in Enum.GetValues(typeof(PanicPhase)))
            {
                bool playable = phase == PanicPhase.Preparing || phase == PanicPhase.Tutorial || phase == PanicPhase.Inspection;
                Check("phase." + phase, PanicNotebookCursorPolicyV59.CanShow(phase, true, false) == playable &&
                    !PanicNotebookCursorPolicyV59.CanShow(phase, false, false) && !PanicNotebookCursorPolicyV59.CanShow(phase, true, true),
                    "Only manually selected targets in ordinary playable phases, with no blocking modal.");
            }
            Check("stage.materials", PanicNotebookCursorPolicyV59.TargetVisibleInStage(true, false, false) &&
                !PanicNotebookCursorPolicyV59.TargetVisibleInStage(true, false, true), "Hidden preparation materials receive no false FPS cursor.");
            Check("stage.supplies", !PanicNotebookCursorPolicyV59.TargetVisibleInStage(false, true, false) &&
                PanicNotebookCursorPolicyV59.TargetVisibleInStage(false, true, true), "FPS supplies receive no false preparation cursor.");
            for (int target = -1; target <= 1; target++) for (int actor = -1; actor <= 1; actor++)
            {
                var mode = PanicNotebookCursorPolicyV59.Mode(target, actor, true);
                Check("floor." + target + "." + actor, target == actor ? mode == PanicNotebookCursorModeV59.Here :
                    mode == PanicNotebookCursorModeV59.OtherFloor && PanicNotebookCursorPolicyV59.Heading(mode, target) != "BURADA",
                    "Every authored floor pair: BURADA is reserved for the actual active floor.");
            }
            Rect safe = Rect.MinMaxRect(32, 160, 1568, 716);
            foreach (var p in new[] { new Vector2(-500, 300), new Vector2(2300, 300), new Vector2(800, -500), new Vector2(800, 1800), safe.center })
            {
                Vector2 front = PanicNotebookCursorPolicyV59.EdgeAnchor(p, true, safe, out var frontDirection);
                Vector2 behind = PanicNotebookCursorPolicyV59.EdgeAnchor(p, false, safe, out var behindDirection);
                bool finite = !float.IsNaN(front.x) && !float.IsNaN(front.y) && !float.IsInfinity(front.x) && !float.IsInfinity(front.y);
                Check("edge." + p, finite && front.x >= safe.xMin - .01f && front.x <= safe.xMax + .01f &&
                    front.y >= safe.yMin - .01f && front.y <= safe.yMax + .01f &&
                    (p == safe.center || Vector2.Dot(frontDirection, behindDirection) < -.999f) &&
                    !float.IsNaN(behind.x) && !float.IsInfinity(behind.y), "Cardinal/offscreen/behind-camera arithmetic remains finite and inside the safe canvas.");
            }
            Check("caption.clearance", PanicNotebookCursorPolicyV59.CaptionClearance == new Rect(440, 116, 690, 94),
                "Exact caption-worker rectangle; production placement and line drawing reserve it whenever a subtitle exists.");
            return r;
        }
        public static void Run()
        {
            if (!Application.isEditor || !Application.isBatchMode) throw new InvalidOperationException("Headless Editor hook only; never attaches to a player.");
            string output = null; string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--navigation-v59-probe-out") output = args[i + 1];
            if (string.IsNullOrEmpty(output) || !Path.IsPathRooted(output)) throw new ArgumentException("An explicit absolute probe output path is required.");
            var report = Probe(); File.WriteAllText(output, JsonUtility.ToJson(report, true));
            if (!report.passed) throw new InvalidOperationException("Navigation V5.9 read-only policy probe failed; see receipt.");
            Debug.Log("NAVIGATION_V59_READ_ONLY_POLICY_PASS; graphical render/OS/human NOT_RUN");
        }
    }
}
