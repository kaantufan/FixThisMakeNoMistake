using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Explicit observer of normal startup display policy. Never changes display or gameplay.
    /// Keep its flag OUT of PanicTestLaunchRules.Drivers so the actual normal startup policy runs.</summary>
    [DefaultExecutionOrder(10000)]
    public sealed class PanicFullscreenStartupObserverV513 : MonoBehaviour
    {
        const string Flag = "--fullscreen-startup-observe";
        const string ReportName = "fullscreen-startup-observer-v513.json";
        const string CaptureName = "fullscreen-startup-title.png";
        static bool armed;
        static string evidence, realScore, realBefore;
        static readonly List<string> startupErrors = new List<string>();
        [Serializable] public sealed class Row { public string id, detail; public bool pass; }
        [Serializable] public sealed class Report
        {
            public string utc, dllSha256, evidencePath, scorePath, realScoreBefore, realScoreAfter, error;
            public string layer = "CURRENT_DLL_NATIVE_NORMAL_STARTUP_DISPLAY_AND_TITLE_FRAMEBUFFER_OBSERVER";
            public bool complete, passed, OSInput, humanAcceptance, displayMutationByObserver, gameplayMutationByObserver;
            public bool startupApplied, startupSkippedForTestDriver, fullScreen;
            public string fullScreenMode, screenshot, screenshotSha256;
            public int ownProcessId, requestedWidth, requestedHeight, monitorWidth, monitorHeight, screenWidth, screenHeight, captureWidth, captureHeight, nonDark;
            public float seconds;
            public List<Row> checks = new List<Row>();
            public List<string> errors = new List<string>();
            public List<string> boundaries = new List<string>();
        }
        readonly Report report = new Report();
        PanicHomeWorld world;
        PanicModel original;
        PanicPhase initialPhase;
        int initialWave, initialCompleted, initialBudget, initialWater, initialFood;
        long initialScore;
        float initialTime, initialPhaseTime, began;
        bool observationsComplete, finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ValidateAndArm()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!args.Contains(Flag)) return;
            // Never quit or otherwise control the Editor when somebody copies observer arguments there.
            if (Application.isEditor || Application.platform != RuntimePlatform.WindowsPlayer)
            { Debug.LogWarning("FULLSCREEN_OBSERVER_V513 NOT_RUN: Windows standalone player required."); return; }
            string reason;
            if (!ValidateArguments(args, out evidence, out reason))
            { Debug.LogError("FULLSCREEN_OBSERVER_V513 invalid isolated launch: " + reason); Application.Quit(2); return; }
            realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            realBefore = Hash(realScore);
            startupErrors.Clear();
            Application.logMessageReceived += Log;
            armed = true;
        }

        public static bool ValidateArguments(string[] args, out string directory, out string reason)
        {
            directory = reason = string.Empty;
            if (args == null || args.Count(a => a == Flag) != 1)
            { reason = "Exactly one explicit fullscreen observer flag required."; return false; }
            if (args.Any(a => PanicTestLaunchRules.Drivers.Contains(a)))
            { reason = "Observer must not share a registered gameplay driver."; return false; }
            if (args.Count(a => a == "--home-evidence") != 1)
            { reason = "Exactly one --home-evidence argument required."; return false; }
            int at = Array.IndexOf(args, "--home-evidence");
            if (at + 1 >= args.Length || string.IsNullOrWhiteSpace(args[at + 1]) || args[at + 1].StartsWith("--", StringComparison.Ordinal))
            { reason = "Evidence directory value missing."; return false; }
            try
            {
                directory = Path.GetFullPath(args[at + 1]);
                string leaf = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                Guid id;
                if (leaf.Length < 36 || !Guid.TryParseExact(leaf.Substring(leaf.Length - 36), "D", out id))
                { reason = "Fresh GUID evidence profile required."; return false; }
                if (File.Exists(Path.Combine(directory, "isolated-home-scores.json")) || File.Exists(Path.Combine(directory, ReportName)) || File.Exists(Path.Combine(directory, CaptureName)))
                { reason = "Existing score/report/capture fixture refused; use a new GUID."; return false; }
            }
            catch (Exception e) { reason = "Invalid evidence directory: " + e.GetType().Name; return false; }
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!armed) return;
            var owner = FindFirstObjectByType<PanicHomeWorld>();
            if (owner) owner.gameObject.AddComponent<PanicFullscreenStartupObserverV513>();
            else
            {
                var observer = new GameObject("FullscreenStartupObserverV513");
                observer.AddComponent<PanicFullscreenStartupObserverV513>();
            }
        }

        static void Log(string text, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) startupErrors.Add(text + "\n" + stack); }
        static string Hash(string path)
        { if (!File.Exists(path)) return "absent"; using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""); }
        void Check(string id, bool pass, string detail)
        { report.checks.Add(new Row { id = id, pass = pass, detail = detail }); Debug.Log("FULLSCREEN_OBSERVER_V513 " + (pass ? "PASS " : "FAIL ") + id + " " + detail); }

        void Start()
        {
            began = Time.realtimeSinceStartup;
            report.utc = DateTime.UtcNow.ToString("o"); report.evidencePath = evidence;
            report.dllSha256 = Hash(typeof(PanicModel).Assembly.Location);
            report.realScoreBefore = realBefore; report.scorePath = PanicModel.ScorePathOverride;
            report.ownProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            report.boundaries.Add("Opt-in observer only. No registered gameplay driver, display setting, SetResolution/fullScreen setter, model start, clock/phase fixture, controller/input call or developer option is used by this observer.");
            report.boundaries.Add("Actual BeforeSceneLoad production fullscreen policy must run normally. Existing windowed preferences may be overridden by that policy; observer itself only reads. Own explicitly launched observer process exits after its report. No old process is addressed.");
            report.boundaries.Add("Native title framebuffer/display state only; no OS input, natural traversal, human readability/listening or campaign acceptance.");
            world = FindFirstObjectByType<PanicHomeWorld>(); original = world ? world.Model : null;
            Check("observer.not-gameplay-driver", !PanicTestLaunchRules.Drivers.Contains(Flag), "Observer flag must remain outside registered gameplay drivers.");
            Check("observer.isolated-profile", world && !string.IsNullOrEmpty(world.EvidencePath) &&
                Path.GetFullPath(world.EvidencePath) == evidence && report.scorePath == Path.Combine(evidence, "isolated-home-scores.json") && !File.Exists(report.scorePath), report.scorePath ?? "null");
            Check("observer.original-title", original != null && original.Phase == PanicPhase.Title && !original.CheatedRun && !original.DeveloperAnyEnabled,
                "Original World model is still the uncheated title model.");
            if (original != null)
            {
                initialPhase = original.Phase; initialWave = original.Wave; initialCompleted = original.CompletedWaves;
                initialBudget = original.Budget; initialScore = original.Score; initialWater = original.PackedWater; initialFood = original.PackedFood;
                initialTime = original.TimeRemaining; initialPhaseTime = original.PhaseTime;
            }
            StartCoroutine(Guard(Observe()));
        }

        IEnumerator Guard(IEnumerator routine)
        {
            while (true)
            {
                object next;
                try { if (!routine.MoveNext()) break; next = routine.Current; }
                catch (Exception e) { report.error = e.ToString(); break; }
                yield return next;
            }
            Finish();
        }

        IEnumerator Observe()
        {
            float until = Time.realtimeSinceStartup + 1f;
            while (Time.realtimeSinceStartup < until) yield return null;
            report.startupApplied = PanicStartupDisplayV513.AppliedV513;
            report.startupSkippedForTestDriver = PanicStartupDisplayV513.SkippedForTestDriverV513;
            report.requestedWidth = PanicStartupDisplayV513.RequestedWidthV513;
            report.requestedHeight = PanicStartupDisplayV513.RequestedHeightV513;
            report.monitorWidth = Screen.currentResolution.width; report.monitorHeight = Screen.currentResolution.height;
            report.screenWidth = Screen.width; report.screenHeight = Screen.height;
            report.fullScreen = Screen.fullScreen; report.fullScreenMode = Screen.fullScreenMode.ToString();
            Check("startup.production-applied", report.startupApplied && !report.startupSkippedForTestDriver, "Actual normal startup policy applied; no registered test-driver skip.");
            Check("display.borderless-fullscreen", Screen.fullScreen && Screen.fullScreenMode == FullScreenMode.FullScreenWindow, report.fullScreenMode);
            Check("display.native-monitor-resolution", report.requestedWidth > 0 && report.requestedHeight > 0 &&
                report.requestedWidth == report.monitorWidth && report.requestedHeight == report.monitorHeight &&
                report.screenWidth == report.requestedWidth && report.screenHeight == report.requestedHeight,
                "Monitor=" + report.monitorWidth + "x" + report.monitorHeight + "; requested=" + report.requestedWidth + "x" + report.requestedHeight + "; framebuffer=" + report.screenWidth + "x" + report.screenHeight);
            Check("model.no-advance-or-cheat", original != null && world && ReferenceEquals(world.Model, original) &&
                original.Phase == initialPhase && initialPhase == PanicPhase.Title && original.Wave == initialWave && original.CompletedWaves == initialCompleted &&
                original.Budget == initialBudget && original.Score == initialScore && original.PackedWater == initialWater && original.PackedFood == initialFood &&
                original.TimeRemaining == initialTime && original.PhaseTime == initialPhaseTime && !original.CheatedRun && !original.DeveloperAnyEnabled,
                "Observer never starts or advances a run; original title model unchanged.");
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (!texture) throw new InvalidOperationException("Actual title framebuffer unavailable.");
            report.captureWidth = texture.width; report.captureHeight = texture.height;
            report.screenshot = Path.Combine(evidence, CaptureName);
            File.WriteAllBytes(report.screenshot, texture.EncodeToPNG()); report.screenshotSha256 = Hash(report.screenshot);
            var pixels = texture.GetPixels32(); byte min = 255, max = 0;
            for (int i = 0; i < pixels.Length; i += 137)
            { var p = pixels[i]; byte value = (byte)((p.r + p.g + p.b) / 3); if (value > 12) report.nonDark++; min = Math.Min(min, value); max = Math.Max(max, value); }
            Destroy(texture);
            Check("capture.actual-native-title", report.captureWidth == report.screenWidth && report.captureHeight == report.screenHeight && report.nonDark > 80 && max - min > 18,
                "Actual title PNG=" + report.captureWidth + "x" + report.captureHeight + "; nonDark=" + report.nonDark);
            observationsComplete = true;
        }

        void Finish()
        {
            if (finished) return; finished = true;
            report.realScoreAfter = Hash(realScore);
            Check("real-profile.unchanged", report.realScoreBefore == report.realScoreAfter, "Actual real highscore profile before/after hashes match.");
            report.errors.AddRange(startupErrors);
            Check("runtime.no-errors", report.errors.Count == 0, "Runtime errors=" + report.errors.Count);
            report.seconds = Time.realtimeSinceStartup - began;
            report.complete = observationsComplete && string.IsNullOrEmpty(report.error);
            report.passed = report.complete && report.checks.Count > 0 && report.checks.All(r => r.pass);
            Directory.CreateDirectory(evidence);
            File.WriteAllText(Path.Combine(evidence, ReportName), JsonUtility.ToJson(report, true));
            Application.logMessageReceived -= Log;
            Debug.Log("FULLSCREEN_OBSERVER_V513 REPORT complete=" + report.complete + " passed=" + report.passed + " checks=" + report.checks.Count);
            Application.Quit(report.passed ? 0 : 3);
        }
    }
}
