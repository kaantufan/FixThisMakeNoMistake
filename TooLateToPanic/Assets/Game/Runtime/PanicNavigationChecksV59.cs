using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Explicit helper for the root's isolated same-DLL UI driver. No self-attach, OS input,
    /// actor placement, remote interaction, clock/phase writes, physics changes or model Tick.</summary>
    public static class PanicNavigationChecksV59
    {
        [Serializable] public sealed class Observation
        {
            public string id, target, mode, text;
            public int frame, width, height, drawFrame;
            public Rect outline, label;
        }
        [Serializable] public sealed class Receipt
        {
            public string layer = "NATIVE_API_AND_ACTUAL_HUD_REPAINT_NOT_OS_OR_HUMAN";
            public bool complete, osInput, humanAcceptance;
            public List<string> fixtures = new List<string>();
            public List<string> failures = new List<string>();
            public List<Observation> observations = new List<Observation>();
        }
        static IEnumerator Wait(float seconds)
        { float until = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < until) yield return null; }
        static string State(PanicHomeWorld w)
        {
            string result = PanicNotebookVerification.GameplayFingerprint(w) + "|" + w.Model.SecondaryRollCountV5 + "|" +
                w.Model.SecondaryEventSummaryV5 + "|" + w.Model.UtilitySafety.GasClosed + ":" + w.Model.UtilitySafety.WaterClosed + ":" + w.Model.UtilitySafety.PowerOff;
            foreach (var supply in w.Expedition.WorldSupplies) result += "|" + supply.Id + ":" + supply.Count;
            return result;
        }
        static void Check(Receipt r, Action<string, bool, string> record, string id, bool pass, string detail)
        { if (!pass) r.failures.Add(id); record("navigation.v59." + id, pass, detail); }
        static bool InCanvas(Rect r)
        { return r.width > 0f && r.height > 0f && r.xMin >= 0 && r.yMin >= 0 && r.xMax <= 1600.5f && r.yMax <= 900.5f; }
        static void Observe(Receipt r, string id, PanicHomeHud h)
        {
            r.observations.Add(new Observation { id = id, frame = Time.frameCount, width = Screen.width, height = Screen.height,
                target = h.NotebookCursorRenderedIdV59, mode = h.NotebookCursorRenderedModeV59.ToString(),
                text = h.NotebookCursorRenderedTextV59, drawFrame = h.NotebookCursorDrawFrameV59,
                outline = h.NotebookCursorRenderedOutlineV59, label = h.NotebookCursorRenderedLabelV59 });
        }
        static IEnumerator ActualHud(Receipt r, Action<string, bool, string> record, PanicHomeWorld w, PanicHomeHud h,
            string id, PanicNotebookCursorModeV59 mode, Func<string, string, IEnumerator> capture)
        {
            int before = h.NotebookCursorDrawCountV59; float until = Time.realtimeSinceStartup + 2f;
            while (h.NotebookCursorDrawCountV59 <= before && Time.realtimeSinceStartup < until) yield return null;
            yield return new WaitForEndOfFrame();
            bool here = mode == PanicNotebookCursorModeV59.Here;
            Check(r, record, id + ".actual-repaint", h.NotebookCursorDrawFrameV59 >= Time.frameCount - 1 &&
                h.NotebookCursorDrawCountV59 > before && h.NotebookCursorRenderedIdV59 == w.NotebookTargetIdV57 &&
                h.NotebookCursorRenderedModeV59 == mode && h.NotebookCursorRenderedTextV59.Contains(w.NotebookTargetLabelV57) &&
                h.NotebookCursorRenderedTextV59.Contains("DEFTER HEDEFİ") && InCanvas(h.NotebookCursorRenderedLabelV59) &&
                (here ? InCanvas(h.NotebookCursorRenderedOutlineV59) && h.NotebookCursorRenderedTextV59.Contains("BURADA") :
                    h.NotebookCursorRenderedOutlineV59.width == 0 && !h.NotebookCursorRenderedTextV59.Contains("BURADA")),
                "Actual main-HUD Repaint uses manual id/name and correct here/floor mode. Other floors receive no outline and never say BURADA. Pixel quality requires root review.");
            Observe(r, id, h);
            yield return capture("notebook-cursor-v59-" + id,
                "Actual graphical HUD for a real manual source, at the retained production spawn/view. No actor/camera placement, OS input or human readability acceptance.");
        }

        public static IEnumerator Run(PanicHomeWorld w, PanicHomeHud h, Action<string, bool, string> record,
            Func<string, string, IEnumerator> capture, Func<int, int, IEnumerator> resolution)
        {
            var r = new Receipt(); bool priorHelper = w.AssistantEnabledV55, priorInput = w.FullFlowScriptedInput;
            bool priorMotion = w.ReducedFpsMotionV54; int width = Screen.width, height = Screen.height;
            r.fixtures.Add("Public ReturnToMenu/StartGame(false), production onboarding confirmation and SkipIntroStory; retained normal spawn/controller, no Warp or time edit. Public pin API is a declared manual-selection fixture; existing V5.7 notebook pointer suite must also run.");
            r.fixtures.Add("Automatic assistant OFF to prove manual independence; public pause/notebook/inventory/F1/developer modal lifecycles; preferences restored. Resolution 1600x900 and 960x540 with actual frame captures.");
            r.fixtures.Add("One synchronous view-only behind-camera projection fixture rotates the actual camera 180 degrees, calls layout, and restores its exact pose in finally before yielding or rendering. This is layout arithmetic evidence only, never a gameplay camera/render result.");
            try
            {
                if (string.IsNullOrEmpty(w.EvidencePath) || string.IsNullOrEmpty(PanicModel.ScorePathOverride))
                    throw new InvalidOperationException("Navigation checks require the root's isolated evidence driver and score override.");
                h.SetPaused(false); w.ReturnToMenu(); yield return null;
                Check(r, record, "normal-play-label", PanicHomeHud.TitleChoiceLabelV414(0) == "OYNA", "Choice0 retains StartRun semantics; visible normal action is OYNA.");
                Check(r, record, "other-mode-labels", PanicHomeHud.TitleChoiceLabelV414(1) == "KISA OYUN" &&
                    PanicHomeHud.TitleChoiceLabelV414(2) == "AFETİ DENE" && PanicHomeHud.TitleChoiceLabelV414(3) == "NASIL OYNANIR",
                    "The existing short scenario, simulation selector and tutorial remain separate existing actions.");
                w.StartGame(false); w.FullFlowScriptedInput = true; yield return null;
                if (h.FreeplayOnboardingOpenV414) h.ConfirmFreeplayOnboardingV414();
                yield return null; w.SkipIntroStoryV414(); yield return Wait(.6f);
                w.SetAssistantEnabledV55(false); yield return resolution(1600, 900);
                Check(r, record, "no-manual-pin-no-cursor", !w.HasNotebookTargetV57 && !h.NotebookCursorVisibleV59,
                    "An automatic suggestion or near/Q selection alone cannot create a manual cursor.");
                string state = State(w), rng = JsonUtility.ToJson(UnityEngine.Random.state); Vector3 actor = w.PlayerPosition;
                string selected = w.SelectedTargetIdV56, hereId = null, otherId = null;
                foreach (var source in w.Expedition.Sources)
                {
                    if (source.Floor != w.ActiveFloor) { if (otherId == null) otherId = source.Id; continue; }
                    if (w.TryMarkNotebookTargetV57(source.Id) && h.TryGetNotebookCursorLayoutV59(out var layout) && layout.Mode == PanicNotebookCursorModeV59.Here)
                    { hereId = source.Id; break; }
                }
                if (otherId == null) foreach (var source in w.Expedition.Sources) if (source.Floor != w.ActiveFloor) { otherId = source.Id; break; }
                Check(r, record, "real-onscreen-source", hereId != null, "Choose an actual source visible from production spawn without moving the actor or changing the view.");
                if (hereId == null) yield break;
                for (int i = 0; i < 24; i++) h.TryGetNotebookCursorLayoutV59(out var ignored);
                Check(r, record, "pin-and-layout-pure", state == State(w) && rng == JsonUtility.ToJson(UnityEngine.Random.state) &&
                    actor.Equals(w.PlayerPosition) && selected == w.SelectedTargetIdV56 && !w.AssistantEnabledV55,
                    "Pin/layout reads retain real costs, stock, score, utilities, model+Unity RNG evidence, position and independent current near/Q selection; clock intentionally excluded.");
                Check(r, record, "real-world-bounds", w.TryGetNotebookCursorTargetV59(out var id, out var label, out var actualBounds, out var floor) &&
                    id == hereId && floor == w.ActiveFloor && w.TryGetInteractionBounds(id, out var expectedBounds) && actualBounds.Equals(expectedBounds),
                    "Cursor derives its bounds from the existing real interaction-object bounds API and floor from source metadata.");
                yield return ActualHud(r, record, w, h, "here-1600", PanicNotebookCursorModeV59.Here, capture);
                yield return resolution(960, 540); yield return ActualHud(r, record, w, h, "here-960", PanicNotebookCursorModeV59.Here, capture);
                yield return resolution(1600, 900);
                w.SetReducedFpsMotionV54(true); yield return Wait(.2f); float firstGlow = h.NotebookCursorRenderedGlowV59;
                yield return Wait(.25f);
                Check(r, record, "reduced-motion-steady", h.NotebookCursorDrawFrameV59 >= Time.frameCount - 1 &&
                    Mathf.Abs(firstGlow - .85f) < .0001f && Mathf.Abs(h.NotebookCursorRenderedGlowV59 - firstGlow) < .0001f,
                    "Actual reduced-motion preference holds the marker glow steady across rendered wall frames.");
                w.SetReducedFpsMotionV54(priorMotion);

                Vector3 cameraPosition = w.View.transform.position; Quaternion cameraRotation = w.View.transform.rotation;
                try
                {
                    w.View.transform.rotation = cameraRotation * Quaternion.Euler(0f, 180f, 0f);
                    Check(r, record, "behind-camera-direction-layout", h.TryGetNotebookCursorLayoutV59(out var behind) &&
                        behind.Mode == PanicNotebookCursorModeV59.Offscreen && behind.Outline.width == 0 && behind.Heading == "BU YÖNDE" &&
                        behind.Anchor.x >= 32f && behind.Anchor.x <= 1568f && behind.Anchor.y >= 160f && behind.Anchor.y <= 716f,
                        "Declared synchronous camera fixture: behind-camera manual target becomes a bounded directional cue, with no false here/outline. No frame captured in fixture pose.");
                }
                finally { w.View.transform.SetPositionAndRotation(cameraPosition, cameraRotation); }

                h.SetPaused(true); yield return new WaitForEndOfFrame();
                Check(r, record, "pause-hides-retains-pin", !h.NotebookCursorVisibleV59 && !h.NotebookCursorRenderedV59 && w.NotebookTargetIdV57 == hereId,
                    "Actual standard pause removes the cursor but keeps manual intent.");
                h.SetPaused(false); yield return null;
                bool opened = w.Notebook.RequestOpen(); float clock = w.Model.TimeRemaining; yield return Wait(.45f);
                Check(r, record, "notebook-hides-pauses", opened && !h.NotebookCursorVisibleV59 && !h.NotebookCursorRenderedV59 &&
                    w.NotebookTargetIdV57 == hereId && Mathf.Abs(w.Model.TimeRemaining - clock) < .003f,
                    "Actual notebook modal hides the world cursor and its existing timer owner remains effective.");
                w.Notebook.RequestClose(); yield return Wait(.5f);
                w.ToggleInventory(); yield return null;
                Check(r, record, "inventory-hides", w.InventoryOpen && !h.NotebookCursorVisibleV59, "Actual shared inventory modal suppresses locator.");
                if (w.InventoryOpen) w.ToggleInventory(); yield return null;
                bool help = h.OpenKeyboardHelpV56(); yield return null;
                Check(r, record, "f1-hides", help && !h.NotebookCursorVisibleV59, "Actual F1 owner suppresses locator.");
                h.CloseKeyboardHelpV56(); yield return Wait(.5f);
                h.SetDeveloperMenuOpen(true); yield return null;
                Check(r, record, "developer-menu-hides", h.DeveloperMenuOpen && !h.NotebookCursorVisibleV59, "F2 menu appearance alone suppresses locator without applying a developer option.");
                h.SetDeveloperMenuOpen(false); yield return Wait(.2f);

                Check(r, record, "real-other-floor-source", otherId != null && w.TryMarkNotebookTargetV57(otherId), "Use an authored source on a different actual floor; actor remains at spawn.");
                if (otherId != null)
                {
                    yield return ActualHud(r, record, w, h, "other-floor-1600", PanicNotebookCursorModeV59.OtherFloor, capture);
                    yield return resolution(960, 540); yield return ActualHud(r, record, w, h, "other-floor-960", PanicNotebookCursorModeV59.OtherFloor, capture);
                }
                w.ClearNotebookTargetV57(); yield return new WaitForEndOfFrame();
                Check(r, record, "same-clear-removes-locator", !w.HasNotebookTargetV57 && !h.NotebookCursorVisibleV59 && !h.NotebookCursorRenderedV59,
                    "Existing public pin clear removes both cursor and outline without adding another state owner.");
                w.TryMarkNotebookTargetV57(hereId); w.StartGame(false);
                Check(r, record, "same-wave-restart-clears", !w.HasNotebookTargetV57 && !h.NotebookCursorVisibleV59,
                    "Existing production same-wave restart synchronously clears manual intent, including its locator.");
                r.complete = true;
            }
            finally
            {
                h.SetDeveloperMenuOpen(false); h.CloseKeyboardHelpV56(); h.SetPaused(false);
                w.SetAssistantEnabledV55(priorHelper); w.SetReducedFpsMotionV54(priorMotion);
                w.ReturnToMenu(); w.FullFlowScriptedInput = priorInput;
                if (!string.IsNullOrEmpty(w.EvidencePath))
                    File.WriteAllText(Path.Combine(w.EvidencePath, "navigation-v59.json"), JsonUtility.ToJson(r, true));
            }
            yield return resolution(width, height);
        }
    }
}
