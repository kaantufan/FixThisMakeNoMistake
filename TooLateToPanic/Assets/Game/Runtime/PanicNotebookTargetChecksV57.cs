using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Explicit isolated UI-driver helper. No attach/OnGUI/OS input, Warp, Tick or private state writes.</summary>
    public static class PanicNotebookTargetChecksV57
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Observation
        {
            public string id, targetId, targetLabel, rendered, gameplay;
            public bool helperEnabled, marked, bookVisible, blocked;
            public int frame, width, height, repaint;
            public float clock;
            public Rect bounds;
            public Vector3 actor, target;
        }
        [Serializable] public sealed class Receipt
        {
            public string layer = "NATIVE_PRODUCTION_POINTER_MODAL_FRAME_API_WITH_ENUMERATED_FIXTURES_NOT_OS_OR_HUMAN", error;
            public bool complete;
            public List<string> fixtures = new List<string>();
            public List<string> failures = new List<string>();
            public List<Observation> observations = new List<Observation>();
        }
        static object Read(object owner, string name)
        {
            var p = owner.GetType().GetProperty(name, Flags); if (p != null) return p.GetValue(owner);
            var f = owner.GetType().GetField(name, Flags); if (f == null) throw new MissingMemberException(owner.GetType().FullName, name); return f.GetValue(owner);
        }
        static T Read<T>(object owner, string name) { return (T)Read(owner, name); }
        static object Call(object owner, string name, params object[] args)
        { return owner.GetType().GetMethods(Flags).First(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(owner, args); }
        static IEnumerator Wait(float seconds) { float until = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < until) yield return null; }
        static string F(float v) { return v.ToString("R", CultureInfo.InvariantCulture); }
        static string V(Vector3 v) { return F(v.x) + "," + F(v.y) + "," + F(v.z); }
        static string Controller(PanicHomeWorld w)
        {
            var c = Read<CharacterController>(w, "controller");
            return c.GetInstanceID() + "|" + c.enabled + "|" + c.gameObject.layer + "|" + V(c.center) + "|" + F(c.radius) + "|" + F(c.height) + "|" + F(c.skinWidth) + "|" + F(c.stepOffset) + "|" + F(c.slopeLimit) + "|" + V(c.transform.localScale);
        }
        static string State(PanicHomeWorld w)
        {
            var m = w.Model;
            return PanicNotebookVerification.GameplayFingerprint(w) + "|" + m.Phase + "|" + m.TotalDamage + "|" + m.CompletedWaves + "|" + m.LastReward + "|" + m.LastWaveScore + "|" +
                m.SecondaryRollCountV5 + "|" + m.SecondaryEventSummaryV5 + "|" + Read<uint>(m, "secondarySeedV5") + "|" + m.UtilitySafety.GasClosed + ":" + m.UtilitySafety.WaterClosed + ":" + m.UtilitySafety.PowerOff +
                string.Join("|", w.Expedition.WorldSupplies.Select(s => s.Id + ":" + s.Count));
        }
        static void Check(Receipt r, Action<string, bool, string> record, string id, bool pass, string detail)
        { if (!pass) r.failures.Add(id + ": " + detail); record("notebook.target." + id, pass, detail); }
        static Rect Pixels(PanicHomeHud h, Rect logical)
        { return new Rect(h.CanvasOffset.x + logical.x * h.CanvasScale, h.CanvasOffset.y + logical.y * h.CanvasScale, logical.width * h.CanvasScale, logical.height * h.CanvasScale); }
        static bool Fits(Rect p)
        { return p.width > 0 && p.height > 0 && p.xMin >= 0 && p.yMin >= 0 && p.xMax <= Screen.width + .5f && p.yMax <= Screen.height + .5f; }
        static void Observe(Receipt r, string id, PanicHomeWorld w, PanicHomeHud h)
        {
            r.observations.Add(new Observation { id = id, frame = Time.frameCount, width = Screen.width, height = Screen.height,
                targetId = w.NotebookTargetIdV57, targetLabel = w.NotebookTargetLabelV57, marked = w.HasNotebookTargetV57, helperEnabled = w.AssistantEnabledV55,
                rendered = h.AssistantRenderedTextV55, bounds = Pixels(h, h.AssistantRenderedBoundsV55), repaint = h.AssistantHudLastDrawFrameV55,
                actor = w.PlayerPosition, target = w.NotebookTargetPositionV57, clock = w.Model.TimeRemaining, gameplay = State(w), bookVisible = w.Notebook.IsVisible, blocked = w.NotebookBlocksInput });
        }
        static void Pointer(Receipt r, Action<string, bool, string> record, string id, PanicHomeWorld w, PanicNotebookOverlay book, Vector2 logical)
        {
            string before = State(w), rng = JsonUtility.ToJson(UnityEngine.Random.state); Vector3 actor = w.PlayerPosition; string controller = Controller(w);
            Call(book, "ProcessPointer", logical);
            Check(r, record, id + ".synchronous-purity", before == State(w) && rng == JsonUtility.ToJson(UnityEngine.Random.state) && actor.Equals(w.PlayerPosition) && controller == Controller(w),
                "Same production pointer handler; gameplay costs/stock/score/secondary seed+rolls, Unity RNG, actor and exact controller unchanged synchronously. Logical point=" + logical + "; no OS input.");
        }
        static IEnumerator OpenRooms(Receipt r, Action<string, bool, string> record, string id, PanicHomeWorld w, bool preserveMarkedSelection = false)
        {
            var b = w.Notebook; bool opened = b.RequestOpen(); float clock = w.Model.TimeRemaining;
            Check(r, record, id + ".open", opened && b.BlocksInput, "Production RequestOpen owns its modal immediately.");
            float until = Time.realtimeSinceStartup + 2f; while (b.Animation.Openness < 1 && Time.realtimeSinceStartup < until) yield return null;
            if (b.IntroVisibleV55) Check(r, record, id + ".intro-dismiss", b.DismissAssistantIntroV55(), "Production introductory page dismissal only.");
            b.RequestPage(PanicNotebookPage.Rooms);
            if (!preserveMarkedSelection) b.SelectFloor(0); else b.RefreshSnapshot();
            until = Time.realtimeSinceStartup + 2f;
            while ((b.Animation.Turning || b.Animation.CurrentPage != PanicNotebookPage.Rooms || b.Animation.Openness < 1) && Time.realtimeSinceStartup < until) yield return null;
            yield return new WaitForEndOfFrame();
            Check(r, record, id + ".settled-clock-paused", b.Animation.TargetOpen && b.Animation.Openness == 1 && !b.Animation.Turning && b.Snapshot != null && b.Snapshot.Floor == 0 && Mathf.Abs(w.Model.TimeRemaining - clock) < .003f,
                "Actual open/turn animation settled on real floor0 snapshot; ordinary World clock remains paused by notebook.");
        }
        static IEnumerator MarkThroughPointer(Receipt r, Action<string, bool, string> record, string id, PanicHomeWorld w, PanicHomeHud h, int index, bool map)
        {
            yield return OpenRooms(r, record, id, w); var b = w.Notebook;
            Check(r, record, id + ".source-exists", b.Snapshot != null && b.Snapshot.Pins.Length > index, "Use actual floor0 source rows, no synthetic selectedSource assignment.");
            if (b.Snapshot == null || b.Snapshot.Pins.Length <= index) yield break;
            var pin = b.Snapshot.Pins[index]; Rect book = Read<Rect>(b, "Book");
            Rect selection = map ? (Rect)Call(b, "PinRect", pin) : (Rect)Call(b, "SourceRow", index);
            Check(r, record, id + ".far-source", Vector3.Distance(w.PlayerPosition, pin.Position) > 4.8f, "Actual source=" + pin.Id + " at " + pin.Position + "; actor=" + w.PlayerPosition);
            string state = State(w), controller = Controller(w); Vector3 actor = w.PlayerPosition;
            Pointer(r, record, id + (map ? ".map-select" : ".row-select"), w, b, book.position + selection.center);
            Check(r, record, id + ".selected-via-pointer", Read<string>(b, "selectedSource") == pin.Id, "Read actual selectedSource only after production pointer selection; source=" + pin.Id);
            yield return null;
            Rect button = Read<Rect>(b, "InventoryRect");
            Pointer(r, record, id + ".mark-button", w, b, Read<Rect>(b, "Book").position + button.center);
            Check(r, record, id + ".button-closes", !b.Animation.TargetOpen, "Actual mark button requests notebook close; do not call pin API as a substitute.");
            float until = Time.realtimeSinceStartup + 3f;
            while ((b.IsVisible || b.BlocksInput || !w.HasNotebookTargetV57 || w.NotebookTargetIdV57 != pin.Id) && Time.realtimeSinceStartup < until) yield return null;
            yield return new WaitForEndOfFrame();
            Check(r, record, id + ".pending-applied-after-close", !b.IsVisible && !b.BlocksInput && w.HasNotebookTargetV57 && w.NotebookTargetIdV57 == pin.Id && w.NotebookTargetLabelV57 == pin.Label,
                "Actual close animation and ordinary Overlay/World Update apply queued target; no direct/private pin or selectedSource write.");
            Check(r, record, id + ".wall-frames-gameplay-pure", state == State(w) && actor.Equals(w.PlayerPosition) && controller == Controller(w),
                "Mark/close application changes no cost, inventory/bag/source/supply stock, score, item damage/upgrade, utilities, secondary seed/rolls or actor/controller. Clock intentionally excluded after close.");
            Observe(r, id + "-marked", w, h);
        }
        static IEnumerator Hud(Receipt r, Action<string, bool, string> record, PanicHomeWorld w, PanicHomeHud h, int width, Func<string, string, IEnumerator> capture, Func<int, int, IEnumerator> resolution)
        {
            yield return resolution(width, width == 1600 ? 900 : 540); int draws = h.AssistantHudDrawFramesV55;
            float until = Time.realtimeSinceStartup + 2f;
            while ((!h.AssistantHudVisibleV55 || h.AssistantHudDrawFramesV55 <= draws) && Time.realtimeSinceStartup < until) yield return null;
            yield return new WaitForEndOfFrame();
            Rect bounds = Pixels(h, h.AssistantRenderedBoundsV55); string label = w.NotebookTargetLabelV57;
            Check(r, record, "hud.actual-manual-label." + width, h.AssistantHudVisibleV55 && h.AssistantHudLastDrawFrameV55 >= Time.frameCount - 1 && h.AssistantHudDrawFramesV55 > draws &&
                h.AssistantRenderedTextV55.Contains("İŞARETLİ HEDEF") && h.AssistantRenderedTaskV55 == label && h.AssistantRenderedHintV55.Contains(" m") && h.AssistantRenderedHintV55.Contains("J") && Fits(bounds),
                "Actual HUD Repaint contains full manual source label=" + label + ", floor/distance/J hint and in-frame bounds=" + bounds + "; frame=" + h.AssistantHudLastDrawFrameV55);
            Observe(r, "hud-" + width, w, h);
            yield return capture("notebook-target-manual-" + width, "Actual marked-source HUD while automatic assistant remains OFF. Real source label/distance; owner pixel review required. Semantic pointer/API fixture, no OS/human acceptance.");
        }
        public static IEnumerator Run(PanicHomeWorld w, PanicHomeHud h, Action<string, bool, string> record, Func<string, string, IEnumerator> capture, Func<int, int, IEnumerator> resolution)
        {
            var r = new Receipt(); bool priorHelper = w.AssistantEnabledV55, priorScripted = w.FullFlowScriptedInput;
            r.fixtures.Add("One public World.ReturnToMenu/StartGame(false) fresh normal run, retained actual spawn/controller. Confirm existing onboarding and SkipIntroStory production methods; no Warp, movement, model Tick, time/phase/source/award edit or private pin write.");
            r.fixtures.Add("Read private Book/SourceRow/PinRect/InventoryRect geometry; invoke same production ProcessPointer with actual logical row/map/button coordinates. Read selectedSource only to verify handler result. No OS input or human acceptance.");
            r.fixtures.Add("Temporary public helper OFF/ON preference and one public SetFocus(farId) solely to query real CanInteract rejection; exact production RefreshFocus(false) is invoked semantically because scripted input suppresses its periodic physical-input branch. No expected near id is assigned. Preserve actor/controller and authoritative gameplay except preparation clock. Unity RNG purity measured synchronously around pointer/API calls, model secondary RNG across wall frames.");
            r.fixtures.Add("Authorized bounded wave cleanup fixture uses public DeveloperSkipWaveV51 (sticky cheated/protected/unrewarded), actual SkipResidenceCinematic, production ledger open/close acknowledgment, then public World.FinishInspection. No forced success/shelter or private phase/time/award write. Title/new-run cleanup uses public lifecycle methods.");
            try
            {
                Check(r, record, "isolated-existing-ui-driver", !string.IsNullOrEmpty(w.EvidencePath) && Environment.GetCommandLineArgs().Contains("--ui-v57-smoke") && !string.IsNullOrEmpty(PanicModel.ScorePathOverride), "Called only by existing fresh isolated UI smoke driver, never attaches itself.");
                h.SetPaused(false); w.ReturnToMenu(); yield return null; w.StartGame(false); w.FullFlowScriptedInput = true; yield return null;
                if (h.FreeplayOnboardingOpenV414) h.ConfirmFreeplayOnboardingV414(); yield return null; w.SkipIntroStoryV414(); yield return Wait(.25f);
                Check(r, record, "fresh-normal-spawn", w.Model.Phase == PanicPhase.Preparing && w.ActiveFloor == 0 && !w.InputLocked && !w.Model.CheatedRun, "Fresh production start owns spawn; no actor placement or preparation-time fixture.");
                w.SetAssistantEnabledV55(false); yield return resolution(1600, 900);
                yield return MarkThroughPointer(r, record, "helper-off-row", w, h, 0, false);
                string id = w.NotebookTargetIdV57, label = w.NotebookTargetLabelV57; Vector3 position = w.NotebookTargetPositionV57;
                Check(r, record, "manual-target-keeps-helper-off", w.HasNotebookTargetV57 && !w.AssistantEnabledV55, "Manual target is independent of automatic helper preference.");
                string state = State(w), control = Controller(w); Vector3 actor = w.PlayerPosition; float clock = w.Model.TimeRemaining;
                w.SetFocus(id);
                Check(r, record, "far-focus-never-grants-interaction", Vector3.Distance(actor, position) > 4.8f && !w.CanInteract, "Public focus fixture cannot bypass real distance/path/controller interaction gate; no E/collect request.");
                // FullFlowScriptedInput intentionally skips World.Update's physical-input branch,
                // including its periodic RefreshFocus(false). Invoke that exact production selector
                // semantically; keep scripted isolation and never assign an expected nearby id.
                string refocusState = State(w), refocusRng = JsonUtility.ToJson(UnityEngine.Random.state);
                Call(w, "RefreshFocus", false);
                string nearId = w.SelectedTargetIdV56; float nearDistance = w.InteractionDistance(nearId);
                Check(r, record, "production-refocus-synchronous-purity", refocusState == State(w) && refocusRng == JsonUtility.ToJson(UnityEngine.Random.state) && actor.Equals(w.PlayerPosition) && control == Controller(w),
                    "Exact production RefreshFocus(false) semantic call retains model/cost/stock/secondary+Unity RNG and actual actor/controller; no expected-near-id, input, pose, time or private focus write.");
                yield return Wait(.70f);
                Check(r, record, "manual-pin-survives-production-refocus", w.NotebookTargetIdV57 == id && w.NotebookTargetLabelV57 == label && !w.AssistantEnabledV55 &&
                    !string.IsNullOrEmpty(nearId) && nearId != id && nearDistance < 2.8f && w.SelectedTargetIdV56 == nearId,
                    "Actual production candidate/path selector chose different near id=" + nearId + " distance=" + nearDistance + "; current interaction focus=" + w.SelectedTargetIdV56 +
                    "; independent manual pin=" + w.NotebookTargetIdV57 + "; helper stays OFF. Scripted input remains enabled; this is API selection, not automatic physical/OS input.");
                Check(r, record, "closed-clock-and-gameplay", state == State(w) && actor.Equals(w.PlayerPosition) && control == Controller(w) && w.Model.TimeRemaining < clock - .2f,
                    "Actual closed gameplay resumes preparation; stock/cost/score/model RNG/actor/controller unchanged.");
                string pure = State(w), rng = JsonUtility.ToJson(UnityEngine.Random.state);
                for (int i = 0; i < 24; i++) { var a = w.NotebookTargetIdV57; var b = w.NotebookTargetLabelV57; var c = w.NotebookTargetHintV57; }
                Check(r, record, "manual-getters-pure", pure == State(w) && rng == JsonUtility.ToJson(UnityEngine.Random.state), "Repeated target presentation getters do not draw Unity/model RNG or change gameplay.");
                yield return Hud(r, record, w, h, 1600, capture, resolution); yield return Hud(r, record, w, h, 960, capture, resolution);
                yield return resolution(1600, 900); w.SetAssistantEnabledV55(true);
                yield return MarkThroughPointer(r, record, "helper-on-map-replacement", w, h, 1, true);
                string replacement = w.NotebookTargetIdV57;
                Check(r, record, "replacement-keeps-helper-on", w.AssistantEnabledV55 && w.HasNotebookTargetV57 && replacement != id, "Second real map pin replaces manual target and keeps automatic helper ON.");
                string before = State(w), oldPin = w.NotebookTargetIdV57; rng = JsonUtility.ToJson(UnityEngine.Random.state);
                bool invalid = w.TryMarkNotebookTargetV57("@qa-invalid-notebook-target");
                Check(r, record, "invalid-id-preserves-old-pin", !invalid && w.NotebookTargetIdV57 == oldPin && before == State(w) && rng == JsonUtility.ToJson(UnityEngine.Random.state), "Invalid public API request preserves old valid pin and gameplay/RNG.");
                yield return OpenRooms(r, record, "clear-toggle", w, true);
                Check(r, record, "reopen-selects-marked-source", Read<string>(w.Notebook, "selectedSource") == replacement, "Reopened notebook selects the actual marked source for same-button clear.");
                Pointer(r, record, "same-button-clear", w, w.Notebook, Read<Rect>(w.Notebook, "Book").position + Read<Rect>(w.Notebook, "InventoryRect").center);
                Check(r, record, "same-button-clears-only-pin", !w.HasNotebookTargetV57 && w.AssistantEnabledV55, "Same production mark/remove button clears pin without disabling automatic helper.");
                w.Notebook.RequestClose(); yield return Wait(.5f);
                Check(r, record, "clear-releases-modal", !w.Notebook.IsVisible && !w.Notebook.BlocksInput, "Actual close animation releases notebook.");
                Check(r, record, "same-wave-restart-fixture-mark", w.TryMarkNotebookTargetV57(replacement) && w.Model.Wave == 1, "Existing source marked on actual wave1 before direct same-frame restart; no intermediate ReturnToMenu or title wait.");
                w.StartGame(false); bool restartClearedImmediately = !w.HasNotebookTargetV57;
                yield return null; if (h.FreeplayOnboardingOpenV414) h.ConfirmFreeplayOnboardingV414(); yield return null;
                w.SkipIntroStoryV414(); yield return Wait(.25f);
                Check(r, record, "same-wave-restart-clears-immediately", restartClearedImmediately && !w.HasNotebookTargetV57 && w.Model.Wave == 1 && w.Model.Phase == PanicPhase.Preparing,
                    "Public StartGame(false) restarts same Model/wave1 in one frame; explicit lifecycle hook must clear pin even with no observable Title tick. No private pin edit.");
                Check(r, record, "wave-fixture-mark", w.TryMarkNotebookTargetV57(replacement), "Public API mark sets existing real source before authorized wave boundary fixture.");
                int wave = w.Model.Wave; bool skip = w.DeveloperSkipWaveV51(); yield return null;
                Check(r, record, "wave-fixture-sticky-unrewarded", skip && w.Model.CheatedRun && w.Model.DeveloperWaveSkipProtectionV51 && !w.Model.ReachedShelterV5, "Production F3 semantic fixture is protected, sticky score-out and does not invent shelter admission.");
                yield return Wait(.15f); w.SkipResidenceCinematic(); float until = Time.realtimeSinceStartup + 4f;
                while ((w.Model.Phase != PanicPhase.Inspection || w.CinematicActive) && Time.realtimeSinceStartup < until) yield return null;
                Check(r, record, "wave-fixture-real-inspection", w.Model.Phase == PanicPhase.Inspection && w.Model.LastReward == 0, "Actual protected skip resolves real aftermath to inspection, no reward.");
                yield return null; yield return null;
                until = Time.realtimeSinceStartup + 2f; while (!Read<bool>(w, "WaveLedgerMayOpenV56") && Time.realtimeSinceStartup < until) yield return null;
                bool opened = w.Model.WaveLedgerOpenV56 || w.Model.SetWaveLedgerOpenV56(true);
                bool closed = opened && w.Model.SetWaveLedgerOpenV56(false); yield return null;
                Check(r, record, "wave-fixture-production-ledger-ack", opened && closed && !w.Model.WaveLedgerOpenV56 && !Read<bool>(w.Model, "WaveLedgerAwaitingReviewV56"), "Existing actual ledger acknowledged through production open/close only.");
                w.FinishInspection(); yield return Wait(.2f);
                Check(r, record, "next-wave-clears-manual-pin", w.Model.Wave == wave + 1 && w.Model.Phase == PanicPhase.Preparing && !w.HasNotebookTargetV57, "Ordinary new-wave observation clears manual target after public inspection continuation.");
                Check(r, record, "title-fixture-mark", w.TryMarkNotebookTargetV57(replacement), "Existing source marked before title cleanup.");
                w.ReturnToMenu(); yield return null;
                Check(r, record, "title-clears-manual-pin", w.Model.Phase == PanicPhase.Title && !w.HasNotebookTargetV57 && w.NotebookTargetIdV57 == "", "Production title lifecycle clears stale target.");
                w.StartGame(false); yield return null;
                Check(r, record, "new-run-no-stale-pin", !w.HasNotebookTargetV57, "New normal run begins without prior manual target.");
                r.complete = true;
            }
            finally
            {
                w.SetAssistantEnabledV55(priorHelper); w.ReturnToMenu(); w.FullFlowScriptedInput = priorScripted;
                File.WriteAllText(Path.Combine(w.EvidencePath, "notebook-target-v57.json"), JsonUtility.ToJson(r, true));
            }
        }
    }
}
