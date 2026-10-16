using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    // Opt-in only. Root attaches this component for --refinement-smoke --home-evidence <isolated directory>.
    public sealed class PanicRefinementSmoke : MonoBehaviour
    {
        [Serializable] sealed class Check { public string name, utc, detail; public bool passed; }
        [Serializable] sealed class WorkTrace
        {
            public string action, mode, finalHint;
            public int ticks, enterPresses, spacePulses, heldTicks, leftTicks, rightTicks, highestObservedStep;
            public bool holdOnlyBlocked;
        }
        [Serializable] sealed class Report
        {
            public string startedUtc, finishedUtc;
            public string evidenceType = "Native Home V4 affected smoke: semantic E/F input, real scene materials and colliders, icon pixels, three authored work families through the runtime animation/commit path, rendered screenshots, narration silence and disaster AudioSource playback state.";
            public string limitations = "API-driven, not manual keyboard/mouse, auditory or final aesthetic acceptance. Explicit room/source setup warps are used; this is not a physical traversal, campaign, balance, cold-save or full regression test. Only the deadline fixture changes model timer values. No stock, work completion, damage, costs or acceptance thresholds are injected.";
            public bool passed;
            public float runtimeSeconds, meanFps;
            public int narrationPlaybackFrames;
            public List<Check> checks = new List<Check>();
            public List<WorkTrace> work = new List<WorkTrace>();
            public List<string> errors = new List<string>();
            public List<string> screenshots = new List<string>();
            public List<string> artifacts = new List<string>();
        }

        const int ExpectedChecks = 12;
        const float HardCapSeconds = 68f;
        PanicHomeWorld w;
        readonly Report report = new Report();
        readonly List<AudioClip> narrationClips = new List<AudioClip>();
        Coroutine running;
        float started, frameSeconds;
        int frames;
        bool finished;

        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--refinement-smoke") < 0) { enabled = false; return; }
            w = GetComponent<PanicHomeWorld>();
            if (!w || string.IsNullOrEmpty(w.EvidencePath)) { Debug.LogError("REFINEMENT_SMOKE requires --home-evidence and PanicHomeWorld"); enabled = false; return; }
            started = Time.realtimeSinceStartup;
            report.startedUtc = DateTime.UtcNow.ToString("o");
            Application.logMessageReceived += Log;
            Application.runInBackground = true;
            w.InputLocked = true;
            foreach (string id in new[] { "news", "earthquake", "flood", "fire", "aftermath", "deadline" })
            {
                var clip = PanicAtmosphereArt.GetSound(id);
                if (clip) narrationClips.Add(clip);
            }
            running = StartCoroutine(Guarded(Run()));
        }

        IEnumerator Guarded(IEnumerator body)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(body);
            while (stack.Count > 0 && !finished)
            {
                bool next; object current = null;
                try { next = stack.Peek().MoveNext(); if (next) current = stack.Peek().Current; }
                catch (Exception ex) { report.errors.Add(ex.ToString()); break; }
                if (!next) { stack.Pop(); continue; }
                var nested = current as IEnumerator;
                if (nested != null) stack.Push(nested); else yield return current;
            }
            Finish();
        }

        IEnumerator Run()
        {
            string score = Path.Combine(w.EvidencePath, "isolated-home-scores.json");
            bool isolated = !string.IsNullOrEmpty(PanicModel.ScorePathOverride) &&
                Path.GetFullPath(PanicModel.ScorePathOverride) == Path.GetFullPath(score);
            if (!isolated) { CheckIt("isolated score path refuses unsafe run", false, PanicModel.ScorePathOverride); yield break; }
            yield return new WaitForSecondsRealtime(.25f);
            w.StartGame(false);
            yield return new WaitForSecondsRealtime(.3f);
            CheckIt("isolated fresh preparation and unchanged capacities", isolated && w.Model.Phase == PanicPhase.Preparing &&
                w.Model.Score == 0 && w.Expedition.Inventory.Count == 12 && w.Expedition.Bag.Count == 8, PanicModel.ScorePathOverride);

            // StartGame forces SyncVisuals; inspect the resulting live scene materials, not factory previews.
            bool paletteOk = true; var paletteDetails = new List<string>();
            foreach (string id in new[] { "bookshelf", "desk_upper", "table", "tvstand" })
            {
                var go = ItemObject(id);
                var mats = go ? go.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m && m.HasProperty("_Color")).Distinct().ToArray() : new Material[0];
                int colored = mats.Count(m => Saturation(m.color) > .14f);
                int authored = mats.Count(m => m.name.StartsWith("HomeV3_", StringComparison.Ordinal) || m.name.StartsWith("HomeV4_", StringComparison.Ordinal));
                bool ok = go && MeshVertices(go) > 100 && colored > 0 && authored > 0;
                paletteOk &= ok;
                paletteDetails.Add(id + ":mats=" + mats.Length + ",colored=" + colored + ",authored=" + authored + ",names=" + string.Join(",", mats.Select(m => m.name).Take(14).ToArray()));
            }
            CheckIt("authored furniture colors survive StartGame and SyncVisuals", paletteOk, string.Join("; ", paletteDetails.ToArray()));

            // Normal following camera frames: keep the player supported while framing the affected spaces.
            bool supported = true;
            Vector3[] views = { new Vector3(4.9f, 3.28f, 3.05f), new Vector3(-3.8f, 3.28f, .8f), new Vector3(-5.7f, .08f, 3.2f), new Vector3(4.1f, 3.28f, -3.8f) };
            string[] viewNames = { "bathroom-layout", "bed-and-rug", "bookshelf-colors", "desktop-details" };
            var roomDetails = new List<string>();
            for (int i = 0; i < views.Length; i++)
            {
                w.Warp(views[i]); yield return Settle(12); yield return new WaitForSecondsRealtime(.65f);
                bool grounded = w.IsGrounded && Mathf.Abs(w.PlayerPosition.y - (views[i].y - .08f)) < .28f;
                supported &= grounded; roomDetails.Add(viewNames[i] + "=" + w.PlayerPosition + ",supported=" + grounded);
                yield return Shot("refinement-" + viewNames[i]);
            }
            var computer = FindObjectsByType<HomePick>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(p => p.Id == "@computer");
            var rug = GameObject.Find("decor_bed_rug");
            var bathroom = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name == "HomeV4_Bathroom" || t.name == "HomeV3_Bathroom");
            bool fixtures = bathroom && bathroom.GetComponentsInChildren<Collider>(true).Length >= 3;
            bool detail = computer && MeshVertices(computer.gameObject) > 100 && rug && MeshVertices(rug) > 0;
            if (computer) roomDetails.Add("computer=" + BoundsText(computer.gameObject));
            if (rug) roomDetails.Add("rug=" + BoundsText(rug));
            CheckIt("affected rooms have supported views, bathroom collision, rug and real PC detail", supported && fixtures && detail, string.Join("; ", roomDetails.ToArray()));

            var screwIcon = PanicResidenceArt.Icon("screws"); var bracketIcon = PanicResidenceArt.Icon("bracket");
            string screwPixels, bracketPixels;
            bool icons = CheckIcon(screwIcon, "screws", out screwPixels) & CheckIcon(bracketIcon, "bracket", out bracketPixels);
            bool distinct = screwIcon && bracketIcon && !screwIcon.GetPixels32().SequenceEqual(bracketIcon.GetPixels32());
            CheckIt("screws and bracket icons contain visible distinct raster pixels", icons && distinct, screwPixels + "; " + bracketPixels + "; distinct=" + distinct);

            LootSource toolbox = FocusSource("garage_tools"); yield return Settle(10); yield return new WaitForSecondsRealtime(.35f);
            string startingInventory = Slots(w.Expedition.Inventory), startingContents = Slots(toolbox.Contents);
            w.HandleKey(KeyCode.E); bool opened = toolbox.Open;
            w.InputLocked = false; yield return new WaitForSecondsRealtime(.85f); w.InputLocked = true;
            bool stable = w.NearestSource != null && w.NearestSource.Id == toolbox.Id && w.LootVisible;
            w.HandleKey(KeyCode.E); bool closed = !toolbox.Open;
            CheckIt("E focus stays on opened toolbox and same E closes without taking", opened && stable && closed && Slots(w.Expedition.Inventory) == startingInventory && Slots(toolbox.Contents) == startingContents,
                "stable=" + stable + "; canInteract=" + w.CanInteract + "; focus=" + w.FocusLabel);

            w.HandleKey(KeyCode.E); w.SelectLoot(0); yield return new WaitForSecondsRealtime(.35f);
            var tray = GameObject.Find("Visible cupboard contents");
            bool realStock = tray && tray.GetComponentsInChildren<ResidenceLootPick>().Length == toolbox.Contents.Count(s => s.Count > 0) && MeshVertices(tray) > 0;
            yield return Shot("refinement-toolbox-stocked");
            var reference = new PanicExpedition(w.Model); reference.ToggleSource("garage_tools");
            bool exact = reference.GatherAt("garage_tools", 0); w.HandleKey(KeyCode.F);
            exact &= Slots(w.Expedition.Inventory) == Slots(reference.Inventory) && Slots(toolbox.Contents) == Slots(reference.Sources.Find(s => s.Id == toolbox.Id).Contents) && w.GatherItemId == "screws" && w.GatherCount == 4;
            w.SelectLoot(1); exact &= reference.GatherAt("garage_tools", 1); w.HandleKey(KeyCode.F);
            exact &= Slots(w.Expedition.Inventory) == Slots(reference.Inventory) && Slots(toolbox.Contents) == Slots(reference.Sources.Find(s => s.Id == toolbox.Id).Contents) && w.GatherItemId == "bracket" && w.GatherCount == 3;
            CheckIt("F takes exactly selected screws then bracket and real stocked contents render", realStock && exact, "stocked=" + realStock + "; inventory=" + Slots(w.Expedition.Inventory));

            w.SelectLoot(2); w.HandleKey(KeyCode.F); yield return new WaitForSecondsRealtime(.65f);
            string beforeEmptyTake = Slots(w.Expedition.Inventory); w.HandleKey(KeyCode.F);
            yield return new WaitForSecondsRealtime(.12f);
            tray = GameObject.Find("Visible cupboard contents");
            bool noPhantom = toolbox.Contents.All(s => s.Count == 0) && string.IsNullOrEmpty(w.SelectedLootId) &&
                Slots(w.Expedition.Inventory) == beforeEmptyTake && (!tray || tray.GetComponentsInChildren<ResidenceLootPick>().Length == 0);
            yield return Shot("refinement-toolbox-empty");
            CheckIt("empty toolbox has no phantom pickup or stock after repeated F", noPhantom, "contents=" + Slots(toolbox.Contents) + "; selected=" + w.SelectedLootId);
            w.HandleKey(KeyCode.E);

            // Gather real materials for Timing and Balance families, using their exact source indices.
            LootSource pantry = FocusSource("basement_pantry"); yield return Settle(10); w.HandleKey(KeyCode.E);
            w.SelectLoot(pantry.Contents.FindIndex(s => s.Id == "seedling")); w.HandleKey(KeyCode.F);
            w.SelectLoot(pantry.Contents.FindIndex(s => s.Id == "hose")); w.HandleKey(KeyCode.F); w.HandleKey(KeyCode.E);
            var shelf = w.Model.Items.Find(i => i.Id == "bookshelf");
            if (shelf == null) throw new InvalidOperationException("Missing bookshelf fixture");
            FocusItem(shelf); yield return Settle(8);
            string cancelInventory = Slots(w.Expedition.Inventory); int cancelBudget = w.Model.Budget;
            w.HandleKey(KeyCode.Alpha1);
            bool cancelOpened = w.WorkActive;
            for (int n = 0; n < 25 && w.WorkActive; n++) w.StepWork(.04f, 0, true, false, false);
            bool holdBlocked = w.WorkActive && w.Expedition.Work.CompletedSteps == 0 && w.Expedition.Work.Progress < .01f;
            for (int n = 0; n < 30 && w.WorkActive && !w.Expedition.Work.IsApplying; n++)
            {
                var work = w.Expedition.Work; float delta = work.Target - work.Cursor;
                bool aligned = Mathf.Abs(delta) <= work.TargetWidth * .45f;
                w.StepWork(.04f, aligned ? 0 : Mathf.Sign(delta), false, aligned, false);
            }
            bool appliedBeforeCancel = w.WorkActive && w.Expedition.Work.IsApplying;
            if (appliedBeforeCancel) w.StepWork(.04f, w.Expedition.Work.ExpectedDirection, false, false, false);
            w.CancelWork();
            CheckIt("partial work cancel and idle hold cannot spend or complete", cancelOpened && holdBlocked && appliedBeforeCancel && !shelf.Anchored && !w.WorkActive &&
                w.Model.Budget == cancelBudget && Slots(w.Expedition.Inventory) == cancelInventory,
                "opened=" + cancelOpened + "; holdBlocked=" + holdBlocked + "; enteredTask=" + appliedBeforeCancel + "; budget=" + w.Model.Budget);

            int screws = w.Expedition.Count("screws"), brackets = w.Expedition.Count("bracket"), anchorBudget = w.Model.Budget;
            FocusItem(shelf); w.HandleKey(KeyCode.Alpha1);
            var anchorTrace = NewTrace("anchorwall"); anchorTrace.holdOnlyBlocked = holdBlocked;
            yield return SolveWork(anchorTrace, "refinement-work-ratchet-anchor");
            int afterAnchorBudget = w.Model.Budget; string afterAnchorInventory = Slots(w.Expedition.Inventory);
            w.CompleteWork(); w.CompleteWork();
            CheckIt("Ratchet actual alternating strokes commit one anchor exactly once", anchorTrace.mode == "Ratchet" && anchorTrace.leftTicks > 0 && anchorTrace.rightTicks > 0 && anchorTrace.enterPresses >= 4 && anchorTrace.spacePulses >= 4 &&
                shelf.Anchored && shelf.AnchorSurface == "wall" && w.Expedition.Count("screws") == screws - 2 && w.Expedition.Count("bracket") == brackets - 1 && w.Model.Budget == anchorBudget - 60 &&
                w.Model.Budget == afterAnchorBudget && Slots(w.Expedition.Inventory) == afterAnchorInventory,
                "anchored=" + shelf.Anchored + "; budget=" + anchorBudget + "->" + w.Model.Budget + "; ticks=" + anchorTrace.ticks);

            int trees = w.Model.TreeCount, seedlings = w.Expedition.Count("seedling");
            FocusSite("plant_west1", new Vector3(-11, 0, 4)); w.HandleKey(KeyCode.E);
            var timingTrace = NewTrace("plant"); yield return SolveWork(timingTrace, "refinement-work-timing-plant");
            bool timingOk = timingTrace.mode == "Timing" && timingTrace.enterPresses >= 4 && timingTrace.spacePulses >= 4 && w.Model.TreeCount == trees + 1 && w.Expedition.Count("seedling") == seedlings - 1;
            int fireTools = w.Model.FireToolCount, hose = w.Expedition.Count("hose");
            FocusSite("hose_station", new Vector3(10, 0, 3)); w.HandleKey(KeyCode.E);
            var balanceTrace = NewTrace("hose"); yield return SolveWork(balanceTrace, "refinement-work-balance-hose");
            bool balanceOk = balanceTrace.mode == "Balance" && balanceTrace.enterPresses >= 4 && balanceTrace.heldTicks > 30 && (balanceTrace.leftTicks + balanceTrace.rightTicks) > 0 &&
                w.Model.FireToolCount == fireTools + 1 && w.Expedition.Count("hose") == hose - 1;
            CheckIt("Timing taps and Balance steering complete two distinct actual tasks", timingOk && balanceOk && new[] { anchorTrace.mode, timingTrace.mode, balanceTrace.mode }.Distinct().Count() == 3,
                "plant=" + timingOk + "; hose=" + balanceOk + "; modes=" + anchorTrace.mode + "," + timingTrace.mode + "," + balanceTrace.mode);

            // Exercise the real deadline phase ordering; leave all loss, damage and score rules intact.
            w.StartGame(false); w.Model.Disaster = DisasterType.Earthquake;
            w.Model.TimeRemaining = .03f; w.Model.PhaseTime = .03f; w.InputLocked = false;
            float deadline = Time.realtimeSinceStartup;
            while (!w.LossAnnounced && Time.realtimeSinceStartup - deadline < .9f) yield return null;
            bool reveal = w.Model.Phase == PanicPhase.GameOver && w.LossAnnounced && !w.CinematicActive;
            yield return Shot("refinement-deadline-loss-before-cinema");
            yield return new WaitForSecondsRealtime(1.55f);
            bool cinema = w.CinematicActive && w.CinematicKind == "Deadline" && w.CinematicTitle == "Vaktin doldu";
            var quake = Resources.Load<AudioClip>("Audio/earthquake"); if (!quake) quake = PanicArt.GetSound("earthquake");
            var playing = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool disasterPlaying = quake && quake.samples > 0 && playing.Any(a => a.isPlaying && a.clip == quake);
            CheckIt("narration stays silent while real disaster audio plays", report.narrationPlaybackFrames == 0 && !w.HasNarrationPlayback && disasterPlaying,
                "narrationFrames=" + report.narrationPlaybackFrames + "; quake=" + (quake ? quake.name : "missing") + "; disasterPlaying=" + disasterPlaying);
            yield return Shot("refinement-deadline-disaster-camera");
            float resultEnd = Time.realtimeSinceStartup + 7f;
            while (w.CinematicActive && Time.realtimeSinceStartup < resultEnd) yield return null;
            yield return new WaitForSecondsRealtime(.35f);
            bool stableResult = w.Model.Phase == PanicPhase.GameOver && !w.CinematicActive && !string.IsNullOrEmpty(w.Model.FailureReason);
            yield return Shot("refinement-gameover-result");
            CheckIt("deadline loss is visible before cinema and ends in stable result", reveal && cinema && stableResult,
                "reveal=" + reveal + "; cinema=" + cinema + "; result=" + stableResult + "; reason=" + w.Model.FailureReason);
            // Leave a stable rendered result long enough to make the affected smoke 35-55 seconds on target hardware.
            float remaining = 36f - (Time.realtimeSinceStartup - started);
            if (remaining > 0) yield return new WaitForSecondsRealtime(remaining);
        }

        WorkTrace NewTrace(string action)
        {
            var trace = new WorkTrace { action = action, mode = w.WorkActive ? w.Expedition.Work.Mode.ToString() : "not-opened" };
            report.work.Add(trace); return trace;
        }
        IEnumerator SolveWork(WorkTrace trace, string screenshot)
        {
            if (!w.WorkActive) { yield return Shot(screenshot); yield break; }
            yield return new WaitForSecondsRealtime(.2f); yield return Shot(screenshot);
            for (int n = 0; n < 850 && w.WorkActive; n++)
            {
                var work = w.Expedition.Work; float delta = work.Target - work.Cursor;
                float dir = 0; bool held = false, pressed = false, pulse = false;
                if (!work.IsApplying)
                {
                    bool aligned = Mathf.Abs(delta) <= work.TargetWidth * .45f;
                    dir = aligned ? 0 : Mathf.Sign(delta); pressed = aligned;
                }
                else if (work.Mode.ToString() == "Ratchet") { dir = work.ExpectedDirection; pulse = work.ExpectedDirection == 0; held = pulse; }
                else if (work.Mode.ToString() == "Timing") { pulse = Mathf.Abs(delta) <= work.TargetWidth * .6f; held = pulse; }
                else { dir = Mathf.Abs(delta) > .018f ? Mathf.Sign(delta) : 0; held = true; }
                trace.ticks++; if (pressed) trace.enterPresses++; if (pulse) trace.spacePulses++; if (held) trace.heldTicks++;
                if (dir < 0) trace.leftTicks++; if (dir > 0) trace.rightTicks++;
                trace.highestObservedStep = Mathf.Max(trace.highestObservedStep, work.CompletedSteps); trace.finalHint = work.StepHint;
                w.StepWork(.04f, dir, held, pressed, pulse);
                if (n % 3 == 0) yield return null;
            }
            if (w.WorkActive) { report.errors.Add("Work did not complete through semantic inputs: " + trace.action + "; hint=" + w.Expedition.Work.StepHint); w.CancelWork(); }
            yield return null;
        }
        LootSource FocusSource(string id)
        {
            var source = w.Expedition.Sources.Find(s => s.Id == id);
            if (source == null) throw new InvalidOperationException("Missing source " + id);
            w.Warp(source.Position + new Vector3(0, .08f, -1.3f)); w.SetFocus(id); return source;
        }
        void FocusItem(PanicItem item) { w.Warp(item.Position + new Vector3(0, .08f, -1.3f)); w.SetFocus(item.Id); }
        void FocusSite(string id, Vector3 p) { w.Warp(p + new Vector3(0, .08f, -1.3f)); w.SetFocus(id); }
        IEnumerator Settle(int count) { for (int i = 0; i < count; i++) { w.SmokeMove(Vector2.zero, .016f); yield return null; } }
        GameObject ItemObject(string id) { var pick = FindObjectsByType<HomePick>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(p => p.Id == id); return pick ? pick.gameObject : null; }
        static int MeshVertices(GameObject go) { return go ? go.GetComponentsInChildren<MeshFilter>(true).Where(m => m.sharedMesh).Sum(m => m.sharedMesh.vertexCount) : 0; }
        static float Saturation(Color c) { Color.RGBToHSV(c, out float h, out float s, out float v); return v > .09f ? s : 0; }
        static string BoundsText(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true); if (renderers.Length == 0) return "no renderers";
            Bounds bounds = renderers[0].bounds; foreach (var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
            return "center=" + bounds.center + ",size=" + bounds.size;
        }
        bool CheckIcon(Texture2D icon, string id, out string detail)
        {
            if (!icon) { detail = id + "=missing"; return false; }
            var pixels = icon.GetPixels32(); int opaque = pixels.Count(p => p.a > 48); int colored = pixels.Count(p => p.a > 48 && Saturation(p) > .18f);
            int foreground = pixels.Count(p => p.a > 48 && Mathf.Max(p.r, Mathf.Max(p.g, p.b)) > 120);
            int colors = pixels.Where(p => p.a > 48).Select(p => (p.r / 16) + ":" + (p.g / 16) + ":" + (p.b / 16)).Distinct().Count();
            string path = Path.Combine(w.EvidencePath, "refinement-icon-" + id + ".png"); File.WriteAllBytes(path, icon.EncodeToPNG()); report.artifacts.Add(path);
            detail = id + "=" + icon.width + "x" + icon.height + ",visible=" + opaque + ",colored=" + colored + ",foreground=" + foreground + ",colors=" + colors;
            return icon.width >= 64 && icon.height >= 64 && opaque > pixels.Length * .07f && opaque < pixels.Length * .97f && foreground > pixels.Length * .025f && colored > 20 && colors >= 3;
        }
        static string Slots(List<SupplySlot> slots) { return string.Join("|", slots.Select(s => (s.Id ?? "") + ":" + s.Count).ToArray()); }
        IEnumerator Shot(string id)
        {
            yield return new WaitForEndOfFrame();
            string path = Path.Combine(w.EvidencePath, id + ".png"); ScreenCapture.CaptureScreenshot(path); report.screenshots.Add(path);
            yield return null; yield return null;
        }
        void CheckIt(string name, bool passed, string detail = "")
        {
            report.checks.Add(new Check { name = name, passed = passed, utc = DateTime.UtcNow.ToString("o"), detail = detail ?? "" });
            Debug.Log("REFINEMENT_CHECK " + (passed ? "PASS " : "FAIL ") + name + " " + detail);
        }
        void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Update()
        {
            if (finished || !w) return;
            frames++; frameSeconds += Time.unscaledDeltaTime;
            bool narration = w.HasNarrationPlayback || FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None).Any(a => a.isPlaying && a.clip && narrationClips.Contains(a.clip));
            if (narration) report.narrationPlaybackFrames++;
            if (Time.realtimeSinceStartup - started > HardCapSeconds)
            {
                report.errors.Add("Affected smoke exceeded " + HardCapSeconds + " second hard cap; remaining checks were not executed.");
                if (running != null) StopCoroutine(running); Finish();
            }
        }
        void Finish()
        {
            if (finished) return; finished = true;
            report.finishedUtc = DateTime.UtcNow.ToString("o"); report.runtimeSeconds = Time.realtimeSinceStartup - started; report.meanFps = frames / Mathf.Max(.01f, frameSeconds);
            foreach (string path in report.screenshots.Concat(report.artifacts)) if (!File.Exists(path) || new FileInfo(path).Length < 256) report.errors.Add("Missing/empty render artifact: " + path);
            if (report.narrationPlaybackFrames > 0) report.errors.Add("Narration was playing in " + report.narrationPlaybackFrames + " observed frames.");
            report.passed = report.errors.Count == 0 && report.checks.Count == ExpectedChecks && report.checks.All(c => c.passed);
            Application.logMessageReceived -= Log;
            try { File.WriteAllText(Path.Combine(w.EvidencePath, "native-refinement-smoke.json"), JsonUtility.ToJson(report, true)); }
            catch (Exception ex) { Debug.LogError("REFINEMENT_SMOKE report write failed: " + ex); }
            Debug.Log("REFINEMENT_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " errors=" + report.errors.Count + " seconds=" + report.runtimeSeconds.ToString("F1"));
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}
