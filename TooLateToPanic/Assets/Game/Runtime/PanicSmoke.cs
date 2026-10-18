using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    // Explicit opt-in native smoke, isolated score path. Never runs in a normal player launch.
    public sealed class PanicSmoke : MonoBehaviour
    {
        [Serializable] public class Check { public string name; public bool passed; public string detail; }
        [Serializable] public class Report
        {
            public string utc, evidenceType = "native player with accelerated phase transitions and model actions", limitations = "Not a full manual playthrough or final art acceptance. Normal mouse/keyboard UI verified separately.";
            public bool passed; public List<Check> checks = new List<Check>(); public List<string> runtimeErrors = new List<string>();
            public int artKinds = 22, completedWaves; public long score; public float meanFps;
        }
        PanicWorld world;
        Report report = new Report();
        string folder;
        int frames;
        float duration;

        IEnumerator Start()
        {
            world = GetComponent<PanicWorld>();
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--panic-evidence") folder = args[i + 1];
            if (string.IsNullOrEmpty(folder)) { Debug.LogError("Smoke requires an isolated evidence path."); yield break; }
            Directory.CreateDirectory(folder);
            Application.logMessageReceived += OnLog;
            world.InputLocked = true;
            yield return new WaitForSeconds(1);
            yield return Shot("01-title");
            world.StartGame(true);
            world.Model.BuyWater();
            world.FinishTutorial();
            Add("tutorial resets real economy and score", world.Model.Phase == PanicPhase.Preparing && world.Model.Budget == 1000 && world.Model.Score == 0 && world.Model.WaterStock == 0 && world.Model.CompletedWaves == 0);
            world.Select("roof");
            world.Act("upgrade");
            yield return new WaitForSeconds(3.3f);
            Add("timed roof upgrade costs150 and changes tier", world.Model.Items.Find(x => x.Id == "roof").Tier == 1 && world.Model.Budget == 850);
            world.SmokePlacePlayer(new Vector3(-2.65f, 0, 3.05f));
            world.Select("bookshelf");
            world.Act("anchor");
            yield return new WaitForSeconds(2.3f);
            Add("timed furniture anchoring", world.Model.Items.Find(x => x.Id == "bookshelf").Anchored);
            world.Model.ToggleAnchor("sofa"); world.Model.ToggleAnchor("fridge"); world.Model.ToggleAnchor("cabinet");
            world.SmokePlacePlayer(new Vector3(2.7f, 0, 1.5f));
            world.Act("water"); world.Act("food"); world.Act("packwater");
            yield return new WaitForSeconds(1.15f);
            world.Act("packfood");
            yield return new WaitForSeconds(1.15f);
            Add("station buying and timed packing", world.Model.BagReady && world.Model.PackedWater == 1 && world.Model.PackedFood == 1);
            var chair = world.Model.Items.Find(x => x.Id == "chair");
            Add("placement rejects bunker obstruction", !world.ValidatePlacement(chair, PanicWorld.BunkerPosition, 0, out string blocked), blocked);
            var newPos = new Vector3(1.5f, 0, -1.45f);
            bool valid = world.ValidatePlacement(chair, newPos, 90, out string reason);
            Add("valid interactive placement candidate", valid, reason);
            if (valid) world.Model.MoveItem("chair", newPos, 90);
            yield return Shot("02-prepared-room");

            for (int wave = 1; wave <= 9; wave++)
            {
                var model = world.Model;
                if (model.Phase != PanicPhase.Preparing) { Add("wave progression continues", false, "Stopped at " + model.Phase + " wave " + model.Wave); break; }
                if (wave == 4) { model.AddFloodGuard("window_north"); model.AddFloodGuard("window_west"); }
                if (wave == 7) { model.AddFireProtection("sofa"); model.AddFireProtection("bed"); model.AddFireProtection("cabinet"); }
                while (model.PackedWater < model.RequiredDays && model.Budget >= 50) { if (model.WaterStock == 0) model.BuyWater(); if (!model.PackWater()) break; }
                while (model.PackedFood < model.RequiredDays && model.Budget >= 50) { if (model.FoodStock == 0) model.BuyFood(); if (!model.PackFood()) break; }
                world.SmokePlacePlayer(new Vector3(2.6f, 0, -2.35f));
                world.Select(null); world.Act("bunker");
                Add("bunker starts wave" + wave, model.Phase == PanicPhase.Disaster);
                yield return new WaitForSeconds(1.0f);
                if (wave == 1 || wave == 4 || wave == 7) yield return Shot("disaster-" + model.Disaster);
                model.Tick(model.PhaseTime + .01f);
                yield return new WaitForSeconds(.4f);
                Add("damage contributions sum wave" + wave, Mathf.Abs(model.Items.Sum(x => x.Contribution) - model.TotalDamage) < .02f);
                model.Tick(model.PhaseTime + .01f);
                yield return new WaitForSeconds(.25f);
                Add("prepared wave passes" + wave, model.Phase == PanicPhase.Success, "damage=" + model.TotalDamage.ToString("F2") + " budget=" + model.Budget);
                if (model.Phase != PanicPhase.Success) break;
                model.Tick(model.PhaseTime + .01f);
                yield return new WaitForSeconds(.35f);
                Add("inspection retains visible damage wave" + wave, model.Phase == PanicPhase.Inspection && model.TotalDamage > 0);
                if (wave == 1 || wave == 4 || wave == 7) yield return Shot("inspection-" + model.Disaster);
                Add("wave supplies consumed" + wave, model.PackedWater == 0 && model.PackedFood == 0);
                world.FinishInspection();
                Add("damage reset preserves upgrade" + wave, model.TotalDamage == 0 && model.Items.Find(x => x.Id == "roof").Tier == 1 && model.Items.Find(x => x.Id == "bookshelf").Anchored);
            }
            report.completedWaves = world.Model.CompletedWaves; report.score = world.Model.Score;
            Add("all three disaster types reached", report.completedWaves >= 9);
            world.ReturnToMenu();
            var reload = new PanicModel();
            Add("local highscore survives model reload", reload.Highscores.Any(x => x.waves == report.completedWaves && x.score == report.score && x.waves > 0));
            world.StartGame(false);
            world.Model.Tick(181f);
            Add("late bunker loses run", world.Model.Phase == PanicPhase.GameOver && world.Model.CompletedWaves == 0);
            yield return new WaitForSeconds(.3f);
            yield return Shot("failure-late");
            world.StartGame(false);
            world.Model.EnterBunker();
            world.Model.Tick(12.05f);
            Add("insufficient bag loses even in bunker", world.Model.Phase == PanicPhase.GameOver && world.Model.FailureReason.Contains("Çanta"));
            world.StartGame(false);
            world.Model.BuyWater(); world.Model.BuyFood(); world.Model.PackWater(); world.Model.PackFood();
            world.Model.EnterBunker(); world.Model.Tick(12.05f);
            Add("unprepared house above65 loses", world.Model.Phase == PanicPhase.GameOver && world.Model.TotalDamage > 65f);
            report.meanFps = duration > 0 ? frames / duration : 0;
            report.utc = DateTime.UtcNow.ToString("o");
            report.passed = report.runtimeErrors.Count == 0 && report.checks.All(x => x.passed);
            File.WriteAllText(Path.Combine(folder, "native-smoke.json"), JsonUtility.ToJson(report, true));
            Debug.Log("PANIC_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " errors=" + report.runtimeErrors.Count + " waves=" + report.completedWaves);
            world.ReturnToMenu();
            world.InputLocked = false;
            yield return new WaitForSeconds(1);
            Application.logMessageReceived -= OnLog;
            enabled = false;
        }
        IEnumerator Shot(string name) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(folder, name + ".png")); yield return null; }
        void Add(string name, bool passed, string detail = "") { report.checks.Add(new Check { name = name, passed = passed, detail = detail }); Debug.Log("PANIC_CHECK " + (passed ? "PASS " : "FAIL ") + name + " " + detail); }
        void OnLog(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.runtimeErrors.Add(message + "\n" + stack); }
        void Update() { frames++; duration += Time.unscaledDeltaTime; }
    }
}
