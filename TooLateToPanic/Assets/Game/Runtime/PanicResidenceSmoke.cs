using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    // Root attaches this component only for --residence-smoke --home-evidence <isolated path>.
    public sealed class PanicResidenceSmoke : MonoBehaviour
    {
        [Serializable] sealed class Check { public string name, utc, detail; public bool passed; }
        [Serializable] sealed class Report
        {
            public string startedUtc, finishedUtc;
            public string evidenceType = "Native Home V3 affected smoke: semantic runtime input, real CharacterController/colliders, timed model phases, rendered screenshots and audio playback state.";
            public string limitations = "API-driven, not manual keyboard/mouse or auditory acceptance. Room screenshots use explicit setup warps; only stair travel and jump use physical movement. One gathered-bag wave, not a nine-wave campaign, balance, cold-save or final art acceptance. Only deadline setup changes the timer; no damage/acceptance thresholds are changed.";
            public bool passed;
            public float runtimeSeconds, meanFps;
            public List<Check> checks = new List<Check>();
            public List<string> errors = new List<string>();
            public List<string> screenshots = new List<string>();
        }

        PanicHomeWorld w;
        readonly Report report = new Report();
        Coroutine run;
        float started, seconds;
        int frames;
        bool finished;

        void Start()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--residence-smoke") < 0) { enabled = false; return; }
            w = GetComponent<PanicHomeWorld>();
            if (!w || string.IsNullOrEmpty(w.EvidencePath)) { Debug.LogError("RESIDENCE_SMOKE requires --home-evidence and PanicHomeWorld"); enabled = false; return; }
            started = Time.realtimeSinceStartup;
            report.startedUtc = DateTime.UtcNow.ToString("o");
            Application.logMessageReceived += Log;
            w.InputLocked = true;
            run = StartCoroutine(Guarded(Run()));
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
            bool isolated = !string.IsNullOrEmpty(PanicModel.ScorePathOverride) && Path.GetFullPath(PanicModel.ScorePathOverride) == Path.GetFullPath(score);
            if (!isolated) { CheckIt("isolated score path refuses unsafe run", false, PanicModel.ScorePathOverride); yield break; }
            yield return new WaitForSecondsRealtime(.4f);
            w.StartGame(false);
            yield return new WaitForSecondsRealtime(.3f);
            CheckIt("isolated fresh preparation and inventory capacities", isolated && w.Model.Phase == PanicPhase.Preparing && w.Model.Score == 0 && w.Expedition.Inventory.Count == 12 && w.Expedition.Bag.Count == 8, PanicModel.ScorePathOverride);
            var voiceDetails = new List<string>(); bool voiceOk = true;
            foreach (string id in new[] { "news", "earthquake", "flood", "fire", "aftermath", "deadline" })
            {
                var clip = PanicAtmosphereArt.GetSound(id);
                bool ok = clip && clip.samples > 0 && clip.channels > 0 && clip.length > .1f && clip.loadState == AudioDataLoadState.Loaded;
                voiceOk &= ok; voiceDetails.Add(id + "=" + (clip ? clip.length.ToString("F2") + "s/" + clip.loadState : "missing"));
            }
            CheckIt("six native voice clips loaded and real start radio speaking", voiceOk && w.RadioEnabled && w.RadioSpeaking, string.Join(", ", voiceDetails.ToArray()) + "; speaking=" + w.RadioSpeaking);

            // Both flights traverse real step colliders, never warp to the destination floor.
            w.Warp(new Vector3(0, -3.12f, -.45f));
            yield return WalkNorth(150);
            CheckIt("physical basement stairs reach ground", w.PlayerPosition.y > -.15f && w.PlayerPosition.y < .5f && w.ActiveFloor == 0, w.PlayerPosition.ToString());
            w.Warp(new Vector3(0, .08f, -.45f));
            yield return WalkNorth(150);
            CheckIt("physical ground stairs reach upper landing", w.PlayerPosition.y > 3.05f && w.PlayerPosition.y < 3.7f && w.ActiveFloor == 1, w.PlayerPosition.ToString());
            w.Warp(new Vector3(-3, .08f, -2));
            yield return Settle(12);
            float baseY = w.PlayerPosition.y, apex = baseY; bool groundedBefore = w.IsGrounded;
            w.HandleKey(KeyCode.Space);
            for (int i = 0; i < 90; i++) { w.SmokeMove(Vector2.zero, .016f); apex = Mathf.Max(apex, w.PlayerPosition.y); yield return null; }
            CheckIt("Space produces gravity jump and landing", groundedBefore && apex > baseY + .6f && Mathf.Abs(w.PlayerPosition.y - baseY) < .15f && w.IsGrounded, "base=" + baseY + "; apex=" + apex + "; landed=" + w.PlayerPosition.y);

            bool roomsOk = true; var roomDetails = new List<string>();
            Vector3[] roomPoints = { new Vector3(-3, .08f, -2), new Vector3(4.3f, 3.28f, 2.6f), new Vector3(-2, -3.12f, -2.5f), new Vector3(11.5f, .08f, 1.1f), new Vector3(-2, 3.28f, -7.5f) };
            string[] roomNames = { "ground", "upper-bathroom", "basement", "garage", "balcony" };
            for (int i = 0; i < roomPoints.Length; i++)
            {
                w.Warp(roomPoints[i]); yield return Settle(12); yield return new WaitForSecondsRealtime(.6f);
                bool supported = Mathf.Abs(w.PlayerPosition.y - (roomPoints[i].y - .08f)) < .25f && w.IsGrounded;
                roomsOk &= supported; roomDetails.Add(roomNames[i] + "=" + w.PlayerPosition + "/supported=" + supported);
                yield return Shot("residence-room-" + roomNames[i]);
            }
            string[] sources = { "fridge", "bath_cabinet", "garage_tools", "basement_pantry", "up_cabinet" };
            roomsOk &= sources.All(id => w.Expedition.Sources.Any(s => s.Id == id && s.Contents.Any(x => x.Count > 0)));
            roomsOk &= w.Model.Items.Any(i => i.Id == "door_balcony") && w.Model.Items.Any(i => i.Id == "bathroom_door");
            CheckIt("five physical room surfaces and functional room sources", roomsOk, string.Join("; ", roomDetails.ToArray()));

            LootSource fridge = FocusSource("fridge"); string empty = Slots(w.Expedition.Inventory), fullFridge = Slots(fridge.Contents);
            w.HandleKey(KeyCode.E); bool opened = fridge.Open; w.HandleKey(KeyCode.E);
            CheckIt("E opens and same E closes without gathering", opened && !fridge.Open && Slots(w.Expedition.Inventory) == empty && Slots(fridge.Contents) == fullFridge);
            w.HandleKey(KeyCode.E); w.SelectLoot(1);
            yield return new WaitForSecondsRealtime(.45f);
            var tray = GameObject.Find("Visible cupboard contents");
            CheckIt("opened container has selected tangible 3D loot", w.LootVisible && w.SelectedLootId == "food" && tray && tray.GetComponentsInChildren<ResidenceLootPick>().Length == 2 && tray.GetComponentsInChildren<MeshFilter>().Any(m => m.sharedMesh && m.sharedMesh.vertexCount > 0));
            yield return Shot("residence-loot-tray-before-take");
            var reference = new PanicExpedition(w.Model); reference.ToggleSource("fridge"); bool referenceGather = reference.GatherAt("fridge", 1);
            w.HandleKey(KeyCode.F);
            CheckIt("F takes exact selected fridge stack matching GatherAt", referenceGather && Slots(w.Expedition.Inventory) == Slots(reference.Inventory) && Slots(fridge.Contents) == Slots(reference.Sources.Find(s => s.Id == "fridge").Contents) && w.Expedition.Count("food") == 2 && w.Expedition.Count("water") == 0 && w.GatherItemId == "food" && w.GatherCount == 2);
            w.SelectLoot(0); w.HandleKey(KeyCode.F);
            LootSource med = FocusSource("bath_cabinet"); w.HandleKey(KeyCode.E); w.SelectLoot(1); w.HandleKey(KeyCode.F);
            reference.ToggleSource("bath_cabinet"); bool medicineGather = reference.GatherAt("bath_cabinet", 1);
            CheckIt("F takes exact medicine stack matching GatherAt", medicineGather && w.Expedition.Count("antiseptic") == reference.Count("antiseptic") && w.Expedition.Count("bandage") == 0 && Slots(med.Contents) == Slots(reference.Sources.Find(s => s.Id == "bath_cabinet").Contents) && med.Contents[0].Count == 3 && w.GatherItemId == "antiseptic");
            w.SelectLoot(0); w.HandleKey(KeyCode.F); w.ToggleInventory(); w.CraftFirstAid();
            CheckIt("first aid craft consumes actual gathered ingredients", w.Expedition.Count("firstaid") == 1 && w.Expedition.Count("bandage") == 1 && w.Expedition.Count("antiseptic") == 1);
            bool packed = Pack("water") && Pack("food") && Pack("firstaid");
            CheckIt("gathered inventory packs a real one-day supported bag", packed && w.Model.BagReady && w.Model.PackedWater == 1 && w.Model.PackedFood == 1 && (w.Model.PackedSupportMask & 1) != 0 && w.Expedition.Bag.Any(x => x.Id == "firstaid" && x.Count == 1));
            yield return Shot("residence-packed-inventory"); w.ToggleInventory();

            FocusSource("garage_tools"); w.HandleKey(KeyCode.E); w.SelectLoot(0); w.HandleKey(KeyCode.F); w.SelectLoot(1); w.HandleKey(KeyCode.F);
            var shelf = w.Model.Items.Find(i => i.Id == "bookshelf");
            int screws = w.Expedition.Count("screws"), brackets = w.Expedition.Count("bracket"), budget = w.Model.Budget;
            w.Warp(shelf.Position + new Vector3(0, .06f, -1.3f)); w.SetFocus(shelf.Id); w.HandleKey(KeyCode.Alpha1);
            bool workOpened = w.WorkActive && w.WorkTexture && w.Expedition.Work.TotalSteps == 4;
            yield return new WaitForSecondsRealtime(.3f); yield return Shot("residence-four-step-anchor");
            var work = w.Expedition.Work;
            for (int n = 0; n < 260 && work.Active && !work.Complete; n++)
            {
                float delta = work.Target - work.Cursor; bool aligned = Mathf.Abs(delta) < .03f;
                work.Tick(.04f, aligned ? 0 : Mathf.Sign(delta), true, aligned); // pressed is the runtime's Enter alignment event.
                if (n % 8 == 0) yield return null;
            }
            int solvedSteps = work.CompletedSteps; bool solved = work.Complete; w.CompleteWork();
            CheckIt("one actual four-step anchor commits materials and budget once", workOpened && solved && solvedSteps == 4 && shelf.Anchored && shelf.AnchorSurface == "wall" && w.Expedition.Count("screws") == screws - 2 && w.Expedition.Count("bracket") == brackets - 1 && w.Model.Budget == budget - 60, "steps=" + solvedSteps + "; anchored=" + shelf.Anchored);

            // Normal timers and phase callbacks run here, preserving collected bag and damage thresholds.
            w.Model.Disaster = DisasterType.Earthquake;
            w.Warp(PanicHomeWorld.BunkerPosition + new Vector3(0, .06f, -1.2f)); w.SetFocus("@bunker"); w.HandleKey(KeyCode.E);
            bool entry = w.Model.Phase == PanicPhase.Disaster && Mathf.Abs(w.Model.PhaseTime - 8) < .05f;
            w.InputLocked = false;
            yield return new WaitForSecondsRealtime(2); yield return Shot("residence-disaster-cinematic");
            yield return WaitPhase(PanicPhase.Aftermath, 8);
            bool aftermath = w.Model.Phase == PanicPhase.Aftermath;
            yield return new WaitForSecondsRealtime(.6f);
            var helicopter = FindObjectsByType<Transform>(FindObjectsSortMode.None).FirstOrDefault(t => t.name == "Mahalle rescue helicopter");
            CheckIt("aftermath spawns a real rescue model with playing rotor", aftermath && helicopter && helicopter.GetComponentsInChildren<MeshFilter>().Any(m => m.sharedMesh && m.sharedMesh.vertexCount > 0) && helicopter.GetComponents<AudioSource>().Any(a => a.isPlaying && a.clip && a.clip.samples > 0));
            yield return Shot("residence-aftermath-rescue");
            yield return WaitPhase(PanicPhase.Success, 5);
            bool success = w.Model.Phase == PanicPhase.Success && w.Model.CompletedWaves == 1;
            yield return new WaitForSecondsRealtime(.65f); yield return Shot("residence-success-bunker-3d");
            yield return WaitPhase(PanicPhase.Inspection, 4);
            w.InputLocked = true;
            CheckIt("gathered bag follows Disaster8 Aftermath4 Success3 Inspection", entry && aftermath && success && w.Model.Phase == PanicPhase.Inspection && shelf.Anchored, "phase=" + w.Model.Phase + "; waves=" + w.Model.CompletedWaves + "; damage=" + w.Model.TotalDamage);

            w.StartGame(false); w.Model.TimeRemaining = .03f; w.Model.PhaseTime = .03f; w.InputLocked = false;
            float deadline = Time.realtimeSinceStartup;
            while (!w.LossAnnounced && Time.realtimeSinceStartup - deadline < .8f) yield return null;
            CheckIt("real deadline reveals loss before any cinematic", w.Model.Phase == PanicPhase.GameOver && w.LossAnnounced && !w.CinematicActive, "elapsed=" + (Time.realtimeSinceStartup - deadline) + "; title=" + w.CinematicTitle);
            yield return Shot("residence-deadline-loss-before-cinema");
            yield return new WaitForSecondsRealtime(1.5f);
            string caption = w.CinematicTitle ?? "";
            bool failCaption = w.CinematicKind == "Deadline" && caption == "Vaktin doldu";
            CheckIt("deadline failure cinematic starts after loss reveal", w.CinematicActive && failCaption, "caption=" + caption + "; progress=" + w.CinematicProgress);
            yield return Shot("residence-deadline-failure-camera");
            yield return new WaitForSecondsRealtime(6);
            CheckIt("failure ends in stable GameOver result UI", w.Model.Phase == PanicPhase.GameOver && !w.CinematicActive && !string.IsNullOrEmpty(w.Model.FailureReason), "phaseTime=" + w.Model.PhaseTime + "; reason=" + w.Model.FailureReason);
            yield return Shot("residence-gameover-stable");
        }

        IEnumerator WalkNorth(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 f = w.View.transform.forward; f.y = 0; f.Normalize(); Vector3 r = w.View.transform.right; r.y = 0; r.Normalize();
                w.SmokeMove(new Vector2(Vector3.Dot(Vector3.forward, r), Vector3.Dot(Vector3.forward, f)), .016f); yield return null;
            }
        }
        IEnumerator Settle(int count) { for (int i = 0; i < count; i++) { w.SmokeMove(Vector2.zero, .016f); yield return null; } }
        IEnumerator WaitPhase(PanicPhase phase, float timeout) { float end = Time.realtimeSinceStartup + timeout; while (w.Model.Phase != phase && Time.realtimeSinceStartup < end) yield return null; }
        LootSource FocusSource(string id) { var s = w.Expedition.Sources.Find(x => x.Id == id); if (s == null) throw new InvalidOperationException("Missing source " + id); w.Warp(s.Position + new Vector3(0, .06f, -1.3f)); w.SetFocus(id); return s; }
        bool Pack(string id) { int index = w.Expedition.Inventory.FindIndex(x => x.Id == id && x.Count > 0); if (index < 0) return false; w.InventorySelect(index); int before = w.Expedition.Count(id); w.InventoryUse(); return w.Expedition.Count(id) == before - 1; }
        static string Slots(List<SupplySlot> slots) { return string.Join("|", slots.Select(s => (s.Id ?? "") + ":" + s.Count).ToArray()); }
        IEnumerator Shot(string id) { yield return new WaitForEndOfFrame(); string path = Path.Combine(w.EvidencePath, id + ".png"); ScreenCapture.CaptureScreenshot(path); report.screenshots.Add(path); yield return null; }
        void CheckIt(string name, bool pass, string detail = "") { report.checks.Add(new Check { name = name, passed = pass, utc = DateTime.UtcNow.ToString("o"), detail = detail ?? "" }); Debug.Log("RESIDENCE_CHECK " + (pass ? "PASS " : "FAIL ") + name + " " + detail); }
        void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Update()
        {
            if (finished || !w) return;
            frames++; seconds += Time.unscaledDeltaTime;
            if (Time.realtimeSinceStartup - started > 74) { CheckIt("affected smoke finishes within 74 second cap", false, "Timed out; remaining checks not executed."); if (run != null) StopCoroutine(run); Finish(); }
        }
        void Finish()
        {
            if (finished) return; finished = true;
            report.finishedUtc = DateTime.UtcNow.ToString("o"); report.runtimeSeconds = Time.realtimeSinceStartup - started; report.meanFps = frames / Mathf.Max(.01f, seconds);
            report.passed = report.errors.Count == 0 && report.checks.Count == 18 && report.checks.All(c => c.passed);
            Application.logMessageReceived -= Log;
            try { File.WriteAllText(Path.Combine(w.EvidencePath, "native-residence-smoke.json"), JsonUtility.ToJson(report, true)); }
            catch (Exception ex) { Debug.LogError("RESIDENCE_SMOKE report write failed: " + ex); }
            Debug.Log("RESIDENCE_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " errors=" + report.errors.Count + " seconds=" + report.runtimeSeconds.ToString("F1"));
            // This component never exists in the user's normal player. Only its opted-in process exits.
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}
