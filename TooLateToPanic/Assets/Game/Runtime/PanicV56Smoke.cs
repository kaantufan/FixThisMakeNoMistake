using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Opt-in V5.6 production API, framebuffer and technical PCM checks. No OS input.</summary>
    public sealed class PanicV56Smoke : MonoBehaviour
    {
        const string Flag = "--v56-smoke";
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id, status, detail, layer; public int frame; }
        [Serializable] public sealed class Shot { public string path, context; public int width, height, frame; }
        [Serializable] public sealed class AudioProbe
        {
            public string scenario, hazard, clip, phase;
            public int frame, timeSamples, activeListeners, sampleRate;
            public double dspTime;
            public float wallSeconds, cinematicAge, cutoff, gain, sourceRms, listenerRms, listenerVolume;
            public bool playing, sourceMute, sourceVirtual, listenerPaused, exterior, realShelter, physicalBasement;
            public Vector3 playerPosition; public float shelterMix, legacyShelterDistance;
        }
        [Serializable] public sealed class FixtureRecord
        { public string kind, detail; public int frame; }
        [Serializable] public sealed class Receipt
        {
            public string utc, dllSha256, scorePath, realScoreBefore, realScoreAfter, error, preferencePrefix, globalPreferencesBefore, globalPreferencesAfter;
            public string evidence = "Fresh GUID evidence profile. Native production session/input/commit/model/physics/camera/render and source/listener PCM. Explicit actor placements, hazard masks, preparation clocks, budget values, immutable memoized incidents and work binding fixtures are enumerated. Natural Disaster/Aftermath phase crossing is measured separately from Enter skip. No OS input, Windows output capture, physical traversal, listening, human quality or full campaign acceptance.";
            public string renderLimit = "Actual drawn rectangles and PNG files certify numerical layout and framebuffer capture. Root pixel inspection remains necessary for readability and visual acceptance.";
            public bool complete, passed, osInput, listening, humanAcceptance, listenerRestored, settingsRestored;
            public float seconds, listenerBefore, listenerAfter;
            public bool listenerPauseBefore, listenerPauseAfter;
            public int productionStartCalls, containerCalls, supplyCalls, workBeginCalls, workInputCalls, workCommits, skipCalls, ledgerOpenCalls, ledgerCloseCalls, ledgerAcknowledgements;
            public List<FixtureRecord> fixtures = new List<FixtureRecord>();
            public List<Row> checks = new List<Row>();
            public List<Shot> captures = new List<Shot>();
            public List<AudioProbe> audio = new List<AudioProbe>();
            public List<string> errors = new List<string>();
        }
        readonly Receipt report = new Receipt();
        PanicHomeWorld w; PanicHomeHud hud; PanicNotebookOverlay book; PanicHomeEffects fx;
        float began; string realScore; bool finished, listenerHeld, settingsHeld;
        float oldMaster, oldMusic, oldSfx; bool oldMuted, oldMusicEnabled;
        Type settings;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (world) world.gameObject.AddComponent<PanicV56Smoke>();
        }
        void Start()
        {
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); fx = GetComponent<PanicHomeEffects>();
            began = Time.realtimeSinceStartup; realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.utc = DateTime.UtcNow.ToString("o"); report.dllSha256 = Hash(typeof(PanicModel).Assembly.Location);
            report.scorePath = PanicModel.ScorePathOverride; report.realScoreBefore = Hash(realScore);
            report.globalPreferencesBefore = GlobalPreferenceFingerprint();
            report.listenerBefore = AudioListener.volume; report.listenerPauseBefore = AudioListener.pause;
            Application.logMessageReceived += Log; StartCoroutine(Guard(Run()));
        }
        void Update()
        { if (!finished && Time.realtimeSinceStartup - began > 180f) { report.error = "180s V5.6 relevant smoke cap"; Finish(); } }
        void OnDisable() { RestoreState(); }
        void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Check(string id, bool pass, string detail = "", string layer = "NATIVE_API_FRAME")
        {
            report.checks.Add(new Row { id = id, status = pass ? "PASS" : "FAIL", detail = detail, layer = layer, frame = Time.frameCount });
            Debug.Log("V56_CHECK " + (pass ? "PASS " : "FAIL ") + id + " " + detail);
        }
        void Fixture(string kind, string detail)
        { report.fixtures.Add(new FixtureRecord { kind = kind, detail = detail, frame = Time.frameCount }); }
        static object Member(object owner, string name)
        {
            var t = owner.GetType(); var p = t.GetProperty(name, Flags); if (p != null) return p.GetValue(owner);
            var f = t.GetField(name, Flags); if (f != null) return f.GetValue(owner);
            throw new MissingMemberException(t.FullName, name);
        }
        static T Read<T>(object owner, string name) { return (T)Member(owner, name); }
        static object Call(object owner, string name, params object[] args)
        { return owner.GetType().GetMethods(Flags).First(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(owner, args); }
        static void Set(object owner, string name, object value)
        { owner.GetType().GetField(name, Flags).SetValue(owner, value); }
        object Setting(string name) { return settings.GetProperty(name, StaticFlags).GetValue(null); }
        void SettingSet(string name, object value) { settings.GetProperty(name, StaticFlags).SetValue(null, value); }
        object SettingCall(string name, params object[] args)
        { return settings.GetMethods(StaticFlags).First(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(null, args); }
        IEnumerator Guard(IEnumerator body)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(body);
            while (stack.Count > 0 && !finished)
            {
                bool next; object current = null;
                try { next = stack.Peek().MoveNext(); if (next) current = stack.Peek().Current; }
                catch (Exception ex) { report.error = ex.ToString(); break; }
                if (!next) { stack.Pop(); continue; }
                if (current is IEnumerator nested) stack.Push(nested); else yield return current;
            }
            if (!finished) { report.complete = string.IsNullOrEmpty(report.error); Finish(); }
        }
        IEnumerator Wait(float seconds)
        { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
        bool Isolated()
        {
            if (string.IsNullOrEmpty(w.EvidencePath) || string.IsNullOrEmpty(PanicModel.ScorePathOverride)) return false;
            string path = Path.GetFullPath(w.EvidencePath); string leaf = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
            Guid id; return leaf.Length >= 36 && Guid.TryParseExact(leaf.Substring(leaf.Length - 36), "D", out id) &&
                string.Equals(Path.GetFullPath(PanicModel.ScorePathOverride), Path.Combine(path, "isolated-home-scores.json"), StringComparison.OrdinalIgnoreCase);
        }
        IEnumerator Resolution(int width, int height)
        {
            bool stable = false;
            for (int attempt = 0; attempt < 2 && !stable; attempt++)
            {
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
                float end = Time.realtimeSinceStartup + 2.5f, held = 0;
                while (Time.realtimeSinceStartup < end)
                {
                    yield return null;
                    held = Screen.width == width && Screen.height == height ? held + Time.unscaledDeltaTime : 0;
                    if (held >= .4f) { stable = true; break; }
                }
                if (!stable) Fixture("resolution-retry", "Actual OS surface not yet stable at " + width + "x" + height + "; one bounded SetResolution retry");
            }
            Check("framebuffer." + width, stable && Screen.width == width && Screen.height == height, Screen.width + "x" + Screen.height, "NATIVE_GRAPHICAL_SURFACE");
        }
        IEnumerator Capture(string name, string context)
        {
            yield return new WaitForEndOfFrame(); string path = Path.Combine(w.EvidencePath, name + ".png");
            int width = Screen.width, height = Screen.height, frame = Time.frameCount; ScreenCapture.CaptureScreenshot(path);
            float end = Time.realtimeSinceStartup + 2; int pw = 0, ph = 0;
            while (!PngSize(path, out pw, out ph) && Time.realtimeSinceStartup < end) yield return null;
            report.captures.Add(new Shot { path = path, context = context, width = pw, height = ph, frame = frame });
            Check("capture." + name, pw == width && ph == height, context, "NATIVE_FRAMEBUFFER_FILE");
        }
        string MutationFingerprint()
        {
            var m = w.Model;
            return PanicNotebookVerification.GameplayFingerprint(w) + "|" + m.SecondaryRollCountV5 + "|" + m.SecondaryEventSummaryV5 + "|" + m.TotalDamage + "|" + m.ReachedShelterV5;
        }
        string ResolutionFingerprint()
        { var m = w.Model; return m.TotalDamage + "|" + m.PackedWater + "|" + m.PackedFood + "|" + m.Budget + "|" + m.LastReward + "|" + m.LastWaveScore + "|" + m.CompletedWaves + "|" + m.Score + "|" + m.SecondaryRollCountV5 + "|" + m.SecondaryEventSummaryV5; }
        IEnumerator Begin(DisasterType primary, int mask)
        {
            yield return AcknowledgeLedger("before-next-start");
            w.ReturnToMenu(); w.StartGame(false); report.productionStartCalls++; w.FullFlowScriptedInput = true;
            yield return null; if (hud.FreeplayOnboardingOpenV414) hud.ConfirmFreeplayOnboardingV414(); yield return null;
            w.SkipIntroStoryV414(); yield return Wait(.12f); book = w.Notebook;
            if (mask != 1)
            {
                w.Model.Wave = 2; w.Model.Disaster = primary;
                Set(w.Model, "compoundWaveV54", 2); Set(w.Model, "compoundPrimaryV54", primary); Set(w.Model, "compoundMaskV54", mask);
                Fixture("hazard-mask", "Wave=2; primary=" + primary + "; mask=" + mask + "; production hazard model remains authoritative");
                yield return null;
            }
            Check("begin." + report.productionStartCalls, w.Model.Phase == PanicPhase.Preparing && !w.Model.IsPaused && !w.CinematicActive && !w.Model.CheatedRun);
        }
        IEnumerator AcknowledgeLedger(string context)
        {
            object ledger = Member(w.Model, "LastWaveLedgerV56");
            if (ledger == null || w.Model.Phase != PanicPhase.Inspection && w.Model.Phase != PanicPhase.GameOver) yield break;
            if (!Read<bool>(w.Model, "WaveLedgerOpenV56") && !Read<bool>(w.Model, "WaveLedgerAwaitingReviewV56")) yield break;
            // Give the real HUD its stable-phase autopopup opportunity before the production close.
            // Never acknowledge by writing the private reviewed flag or disabling the modal guard.
            yield return null; yield return null;
            float until = Time.realtimeSinceStartup + 2f;
            while (!Read<bool>(w, "WaveLedgerMayOpenV56") && Time.realtimeSinceStartup < until) yield return null;
            bool alreadyOpen = Read<bool>(w.Model, "WaveLedgerOpenV56"), opened = alreadyOpen;
            string before = ResolutionFingerprint();
            if (!alreadyOpen) { opened = (bool)Call(w.Model, "SetWaveLedgerOpenV56", true); report.ledgerOpenCalls++; }
            bool closed = opened && (bool)Call(w.Model, "SetWaveLedgerOpenV56", false); if (opened) report.ledgerCloseCalls++;
            if (closed) report.ledgerAcknowledgements++;
            Fixture("ledger-acknowledgement", context + "; real production API " + (alreadyOpen ? "HUD already opened -> close" : "open -> close") + "; no private review-state write");
            yield return null;
            Check("ledger.ack." + context + "." + report.ledgerAcknowledgements, opened && closed && !Read<bool>(w.Model, "WaveLedgerOpenV56") && !Read<bool>(w.Model, "WaveLedgerAwaitingReviewV56") && before == ResolutionFingerprint(), "Actual ledger acknowledgement before subsequent test controls; damage/supplies/reward/score/RNG unchanged", "NATIVE_PRODUCTION_MODAL_API");
        }
        IEnumerator Place(Vector3 position)
        {
            w.Warp(position); Fixture("actor-warp", position.ToString("F3"));
            for (int i = 0; i < 12; i++) { w.SmokeMove(Vector2.zero, .025f); yield return null; }
        }
        void Aim(Vector3 position)
        {
            var a = Quaternion.LookRotation(position - (w.PlayerPosition + Vector3.up * 1.62f)).eulerAngles;
            float yaw = Read<float>(w, "bagYaw"), pitch = Read<float>(w, "bagPitch");
            w.StepBagLook(new Vector2(Mathf.DeltaAngle(yaw, a.y) / (2 * w.FpsLookMultiplierV414), (pitch - Mathf.DeltaAngle(0, a.x)) / (1.7f * w.FpsLookMultiplierV414)));
            Fixture("actor-view", "Aim real bag input camera at " + position);
        }
        void GatherMaterials()
        {
            foreach (var source in w.Expedition.Sources.ToArray()) if (!source.AutoLooted && !source.Open)
            { w.Expedition.ToggleSource(source.Id); report.containerCalls++; }
        }
        bool BindWork(string action, string target)
        {
            w.CancelWork(); bool active = w.Expedition.BeginValidatedWorkV414(action, target, false); report.workBeginCalls++;
            if (active)
            {
                Set(w, "workAction", action); Set(w, "workTarget", target);
                Call(w, "BeginWorkAudioV56");
                Fixture("work-world-binding", action + "/" + target + ": production validated session bound to world audio/input adapter; bypasses only geometric approach for isolated audio test");
            }
            return active;
        }
        IEnumerator HammerAndUtilities()
        {
            GatherMaterials(); yield return Wait(.1f);
            var target = w.Model.Items.FirstOrDefault(i => i.Installed && i.Kind == ItemKind.Window && i.Tier < 2 && w.Expedition.CanBeginWork("upgrade", i.Id));
            Check("hammer.fixture-real-available-target", target != null, "Finite production material loot; normal budget, no free-material/developer option");
            if (target != null)
            {
                int before = Read<int>(w, "AcceptedHammerAudioCountV56"), dispatchedBefore = Read<int>(w, "WorkAudioDispatchCountV56"), budget = w.Model.Budget;
                var costs = w.Expedition.WorkMaterialCostsV414("upgrade", target.Id).ToArray(); var stocks = costs.ToDictionary(c => c.Id, c => w.Expedition.Count(c.Id));
                int price = w.Expedition.WorkBudgetCostV414("upgrade", target.Id), tier = target.Tier;
                Check("hammer.begin", BindWork("upgrade", target.Id)); yield return null;
                w.StepWork(.02f, 0, false, false, false); yield return null;
                Check("hammer.no-imaginary-begin-hit", Read<int>(w, "AcceptedHammerAudioCountV56") == before && Read<int>(w, "WorkAudioDispatchCountV56") == dispatchedBefore && w.Expedition.Work.CompletedStrokes == 0 && w.Model.Budget == budget);
                for (int stroke = 1; stroke <= 4; stroke++)
                {
                    bool accepted = w.StrikeManualWorkV414(); report.workInputCalls++;
                    Check("hammer.accepted-stroke." + stroke, accepted && Read<int>(w, "AcceptedHammerAudioCountV56") == before + stroke && Read<int>(w, "WorkAudioDispatchCountV56") == dispatchedBefore + stroke && Read<string>(w, "LastWorkAudioCueV56") == "v56_hammer_hit",
                        "Actual production StrikeManualWork adapter; one event for each accepted separate-frame stroke");
                    yield return null;
                }
                Check("hammer.atomic-cost", target.Tier == tier + 1 && w.Model.Budget == budget - price && costs.All(c => w.Expedition.Count(c.Id) == stocks[c.Id] - c.Required));
                report.workCommits++; int hitCount = Read<int>(w, "AcceptedHammerAudioCountV56");
                Check("hammer.inactive-input-no-audio", !w.StrikeManualWorkV414() && Read<int>(w, "AcceptedHammerAudioCountV56") == hitCount);
            }
            foreach (string action in new[] { "gasoff", "wateroff", "poweroff" })
            {
                string prefix = action == "gasoff" ? "v56_gas" : action == "wateroff" ? "v56_water" : "v56_power";
                Check("utility.semantic-work-id." + action, PanicAudioBank.Work(action).StartsWith(prefix, StringComparison.Ordinal), PanicAudioBank.Work(action));
                int budget = w.Model.Budget; string targetId = PanicUtilitySafetyV414.TargetForAction(action);
                int dispatched = Read<int>(w, "WorkAudioDispatchCountV56");
                Check("utility.begin." + action, BindWork(action, targetId) && Read<string>(w, "LastWorkAudioCueV56") == prefix + "_start" && Read<int>(w, "WorkAudioDispatchCountV56") == dispatched + 1); yield return null;
                w.Expedition.Work.TryClick(0); w.StepWork(.02f, 0, false, false, false); yield return null;
                if (w.Expedition.Work.UsesLinearDrag) w.Expedition.Work.ApplyLinearDisplacement(.5f, true); else w.Expedition.Work.ApplyRotation(45f, true);
                report.workInputCalls++; w.StepWork(.02f, 0, false, false, false); yield return null;
                Check("utility.accepted-motion-cue." + action, Read<string>(w, "LastWorkAudioCueV56") == prefix + "_turn" && Read<int>(w, "WorkAudioDispatchCountV56") > dispatched + 1);
                w.CancelWork(); yield return null; Check("utility.cancel-free." + action, w.Model.Budget == budget && !w.Model.UtilitySafety.IsComplete(action) && Read<string>(w, "LastWorkAudioCueV56") == prefix + "_cancel");
                Check("utility.rebegin." + action, BindWork(action, targetId)); yield return null;
                w.Expedition.Work.TryClick(0); w.StepWork(.02f, 0, false, false, false); yield return null;
                if (w.Expedition.Work.UsesLinearDrag) w.Expedition.Work.ApplyLinearDisplacement(1f, true); else w.Expedition.Work.ApplyRotation(90f, true);
                report.workInputCalls++; w.StepWork(.02f, 0, false, false, false); report.workCommits++; yield return null;
                Check("utility.actual-commit." + action, w.Model.UtilitySafety.IsComplete(action) && w.Model.Budget == budget && Read<string>(w, "LastWorkAudioCueV56") == prefix + "_close");
            }
        }
        IEnumerator AssistantBudget()
        {
            GatherMaterials(); yield return Wait(.12f); int budget = w.Model.Budget; string target = w.AssistantTargetIdV55;
            var item = w.Model.Items.Find(i => i.Id == target); Check("assistant.fixture-real-materials", item != null && !item.Anchored && w.AssistantV55.Steps[0].Complete, target);
            w.Model.Budget = 0; Fixture("budget", budget + " -> 0 for insufficient-budget recommendation; finite gathered materials retained");
            string before = MutationFingerprint(); var first = w.AssistantV55;
            Check("assistant.no-budget-selects-free-utility", first.CurrentStep == 3 && !first.Steps[1].Complete && first.Steps[1].Detail.ToLowerInvariant().Contains("bütçe"), first.Task + " / " + first.Steps[1].Detail);
            float until = Time.realtimeSinceStartup + 7;
            while ((w.GuidancePlanningV54 || !w.GuidanceRouteValidV54 || !w.GuidanceObjectiveActionV54.EndsWith("off", StringComparison.Ordinal)) && Time.realtimeSinceStartup < until) yield return null;
            Check("assistant.free-utility-actual-route", w.GuidanceRouteValidV54 && w.GuidanceObjectiveActionV54.EndsWith("off", StringComparison.Ordinal), w.GuidanceObjectiveIdV54 + " / " + w.GuidanceObjectiveActionV54);
            for (int i = 0; i < 16; i++) { var snapshot = w.AssistantV55; }
            Check("assistant.capture-readonly", before == MutationFingerprint());
            w.Model.Budget = budget; Fixture("budget-restore", "Restore prior normal budget=" + budget); yield return Wait(.15f);
            Check("assistant.affordable-returns-anchor", w.AssistantV55.CurrentStep == 2 && !w.AssistantV55.Steps[1].Complete);
            yield return Capture("01-assistant-budget-restored", "Real gathered materials and restored ordinary budget; no developer cheats.");
        }
        IEnumerator MapLabels()
        {
            Check("notebook.open", book.RequestOpen()); yield return Wait(.45f);
            if (Read<bool>(book, "IntroVisibleV55")) Call(book, "DismissAssistantIntroV55"); yield return Wait(.1f);
            foreach (int width in new[] { 960, 1600 })
            {
                yield return Resolution(width, width == 960 ? 540 : 900);
                foreach (int floor in new[] { -1, 0, 1 })
                {
                    book.SelectFloor(floor); yield return Wait(.1f); yield return new WaitForEndOfFrame();
                    var rects = Read<Rect[]>(book, "MapRoomLabelRectsV56"); bool finite = rects.Length > 0 && Time.frameCount - Read<int>(book, "MapRoomLabelDrawFrameV56") <= 1, overlap = false;
                    foreach (var r in rects) finite &= r.width > 0 && r.height > 0 && r.xMin >= 0 && r.yMin >= 0 && r.xMax <= Screen.width && r.yMax <= Screen.height;
                    for (int i = 0; i < rects.Length; i++) for (int j = i + 1; j < rects.Length; j++) overlap |= rects[i].Overlaps(rects[j]);
                    Check("map.drawn-labels-nonoverlap." + width + "." + floor, finite && !overlap, "Actual last rendered room labels=" + rects.Length + "; rects=" + string.Join(" / ", rects.Select(r => r.ToString()).ToArray()), "NATIVE_REPAINT_LAYOUT");
                    if (floor == 0) yield return Capture("02-map-" + width, "Actual floor0 map label layout at " + width + "; root inspect pixels.");
                }
            }
            book.RequestClose(); yield return Wait(.45f);
        }
        IEnumerator Settings()
        {
            settings = typeof(PanicModel).Assembly.GetType("TooLateToPanic.PanicAudioSettingsV56", true);
            report.preferencePrefix = (string)Setting("ProfilePreferencePrefix");
            Check("audio.settings-isolated-prefix", (bool)Setting("IsolatedProfile") && !string.IsNullOrEmpty(report.preferencePrefix), report.preferencePrefix, "NATIVE_PREFERENCE_PROFILE");
            oldMaster = (float)Setting("MasterVolume"); oldMusic = (float)Setting("MusicVolume"); oldSfx = (float)Setting("SfxVolume"); oldMuted = (bool)Setting("Muted"); oldMusicEnabled = (bool)Setting("MusicEnabled"); settingsHeld = true;
            SettingCall("SetVolumes", .63f, .27f, .41f); SettingCall("SetMuted", true); SettingSet("MusicEnabled", false); SettingCall("Save");
            SettingCall("Reload");
            Check("audio.settings-production-persistence", Mathf.Abs((float)Setting("MasterVolume") - .63f) < .001f && Mathf.Abs((float)Setting("MusicVolume") - .27f) < .001f && Mathf.Abs((float)Setting("SfxVolume") - .41f) < .001f && (bool)Setting("Muted") && !(bool)Setting("MusicEnabled"), "Saved/reloaded production settings under isolated prefix only", "NATIVE_ISOLATED_PREFERENCES");
            Check("audio.settings-no-listener-write", AudioListener.volume == report.listenerBefore && AudioListener.pause == report.listenerPauseBefore);
            SettingCall("SetVolumes", 1f, 1f, 1f); SettingCall("SetMuted", false); SettingSet("MusicEnabled", true);
            Fixture("audio-settings", "Temporary isolated master/music/SFX=1, mute=false/music=true for numerical native output samples; initial values restored in finally/OnDisable");
            yield return null;
        }
        IEnumerator BagAndShelter(bool upper, bool incompletePrompt, bool pack = true)
        {
            w.Model.TimeRemaining = 45f; Fixture("preparation-clock", "TimeRemaining=45; native production 2s camera handoff and clocks run thereafter"); PanicBagDollyChecksV56.Prepare(w); w.TickPreparationStages(); yield return PanicBagDollyChecksV56.Observe(w,hud,(id,pass,detail)=>Check(id,pass,detail));
            Check("bag.actual-handoff." + report.productionStartCalls, w.BagFinalStageActive && w.BagFirstPersonActive);
            if (report.productionStartCalls == 2) yield return StableFpsViewV57();
            if (incompletePrompt)
            {
                yield return Place(new Vector3(-4.5f, -3.16f, 1.4f)); Aim(PanicHomeWorld.BunkerPosition + Vector3.up * .35f); yield return Wait(.15f); w.SetFocus("bunker"); Call(w, "UpdateContext");
                string prompt = w.ContextHint + " " + w.ShortAction;
                Check("shelter.incomplete-explicit-consequence", !w.Model.BagReady && prompt.IndexOf("su", StringComparison.OrdinalIgnoreCase) >= 0 && prompt.IndexOf("gıda", StringComparison.OrdinalIgnoreCase) >= 0 && (prompt.IndexOf("bit", StringComparison.OrdinalIgnoreCase) >= 0 || prompt.IndexOf("başla", StringComparison.OrdinalIgnoreCase) >= 0), prompt);
                yield return Capture("03-incomplete-bag-shelter-prompt", "Actual physical shelter view; incomplete bag; E has not been pressed.");
            }
            foreach (string id in pack ? new[] { "water", "food" } : Array.Empty<string>())
            {
                var supply = w.Expedition.WorldSupplies.FirstOrDefault(s => s.ItemId == id && s.Count > 0); int stock = supply == null ? 0 : supply.Count;
                report.supplyCalls++; Check("bag.real-finite-" + id + "." + report.productionStartCalls, supply != null && w.Expedition.TryCollectWorldSupply(supply.Id) && supply.Count < stock);
            }
            yield return Place(upper ? new Vector3(4.2f, 3.24f, -.65f) : new Vector3(-4.5f, -3.16f, 1.4f));
            Aim(w.ActiveShelterPositionV5 + Vector3.up * (upper ? 1.35f : .35f)); yield return Wait(.6f); w.SetFocus(upper ? "@panicroom" : "bunker");
            Check("shelter.real-controller-ray-path-gate." + report.productionStartCalls, w.TryEnterActiveShelterV5() && w.Model.ReachedShelterV5 && w.Model.Phase == PanicPhase.Disaster, "Actor placement/view fixture; real grounded floor/ray/path admission; no traversal acceptance");
            yield return null;
        }
        IEnumerator StableFpsViewV57()
        {
            bool prior = w.ReducedFpsMotionV54;
            Fixture("fps-stable-view", "Same first natural FPS handoff; temporarily observe reduced-motion false and true for .8s each, then restore. No camera/actor/input/time writes or manual Tick. Latest stable-view behavior must be supplied by production owner; not certified by this source patch.");
            try
            {
                foreach (bool reduced in new[] { false, true })
                {
                    w.SetReducedFpsMotionV54(reduced); yield return null; yield return new WaitForEndOfFrame();
                    Vector3 actor = w.PlayerPosition; Quaternion input = w.FpsInputRotationV54; float preparation = w.Model.TimeRemaining;
                    float maxPose = 0, maxFov = 0, maxEyeOffset = 0; int samples = 0; bool active = true, feedback = false; float end = Time.realtimeSinceStartup + .8f;
                    while (Time.realtimeSinceStartup < end)
                    {
                        yield return new WaitForEndOfFrame(); samples++;
                        active &= w.BagFirstPersonActive && !w.BagCameraTransitionActive && w.Model.Phase == PanicPhase.Preparing && !w.Model.IsPaused;
                        maxPose = Mathf.Max(maxPose, Quaternion.Angle(w.View.transform.rotation, w.FpsInputRotationV54));
                        maxFov = Mathf.Max(maxFov, Mathf.Abs(w.View.fieldOfView - 62f));
                        maxEyeOffset = Mathf.Max(maxEyeOffset, Vector3.Distance(w.View.transform.position, w.PlayerPosition + Vector3.up * 1.62f));
                        feedback |= w.FpsStressVisibleV414 && w.FpsFeedbackAudioPlayingV414 && w.FpsVeilTextureV414 && w.FpsVeilOpacityV414 > 0 && w.FpsVeilOpacityV414 < .95f;
                    }
                    string mode = reduced ? "reduced" : "normal";
                    Check("fps.stable-view-no-sway-or-fov." + mode, active && samples >= 8 && maxPose < .05f && maxFov < .001f && maxEyeOffset < .001f,
                        "Actual no-input frames=" + samples + "; max angular error from input=" + maxPose + "; FOV error from62=" + maxFov + "; eye offset=" + maxEyeOffset + ". No forced camera pose during observation.", "NATIVE_ACTUAL_FPS_CAMERA_FRAMES");
                    Check("fps.stable-view-keeps-clock-actor-feedback." + mode, actor.Equals(w.PlayerPosition) && Quaternion.Angle(input, w.FpsInputRotationV54) < .05f && w.Model.TimeRemaining < preparation - .3f && feedback,
                        "Real preparation clock advances with unchanged actor/input; retained nonverbal audio and bounded soft veil stay active. No human comfort/listening claim.", "NATIVE_ACTUAL_FPS_CAMERA_FRAMES");
                }
            }
            finally { w.SetReducedFpsMotionV54(prior); }
        }
        void MemoIncident(DisasterType hazard, PanicSecondaryEventKindV5 kind, string label)
        {
            var baseRisk = typeof(PanicModel).GetMethod("BaseRisk", StaticFlags);
            var target = w.Model.Items.First(i => i.Installed && i.Kind == ItemKind.Furniture && (float)baseRisk.Invoke(null, new object[] { i, hazard }) > 0);
            var memo = Read<Dictionary<string, PanicSecondaryEventV5>>(w.Model, "secondaryRollsV5");
            memo[w.Model.Wave + ":" + (int)hazard] = new PanicSecondaryEventV5(kind, target.Id, 2f, label);
            Fixture("memoized-incident", hazard + "/" + kind + "/" + target.Id + "/2pt; committed only by ordinary damage resolution; no test reroll");
        }
        static float Rms(float[] data) { double sum = 0; foreach (float v in data) sum += v * v; return (float)Math.Sqrt(sum / data.Length); }
        IEnumerator ProbeAudio(string scenario, DisasterType hazard, int samples)
        {
            for (int i = 0; i < samples; i++)
            {
                var source = (AudioSource)Call(fx, "HazardSourceV56", hazard); var data = new float[4096]; var mixed = new float[4096];
                if (source) source.GetOutputData(data, 0); AudioListener.GetOutputData(mixed, 0);
                report.audio.Add(new AudioProbe { scenario = scenario, hazard = hazard.ToString(), clip = source && source.clip ? source.clip.name : "absent", phase = w.Model.Phase.ToString(), frame = Time.frameCount, timeSamples = source ? source.timeSamples : 0, activeListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(a => a.isActiveAndEnabled), sampleRate = AudioSettings.outputSampleRate, dspTime = AudioSettings.dspTime, wallSeconds = Time.realtimeSinceStartup - began, cinematicAge = Read<float>(w, "residenceCinematicAge"), cutoff = (float)Call(fx, "HazardCutoffV56", hazard), gain = source ? source.volume : 0, sourceRms = Rms(data), listenerRms = Rms(mixed), listenerVolume = AudioListener.volume, playing = source && source.isPlaying, sourceMute = source && source.mute, sourceVirtual = source && source.isVirtual, listenerPaused = AudioListener.pause, exterior = Read<bool>(w, "AudioExteriorCameraV56"), realShelter = w.Model.ReachedShelterV5, playerPosition = w.PlayerPosition, physicalBasement = w.AudioPhysicalBasementV56, shelterMix = fx.ShelterMix, legacyShelterDistance = w.AudioLegacyBunkerDistanceV56 });
                yield return Wait(.04f);
            }
        }
        IEnumerator NaturalAudioAndAftermath()
        {
            MemoIncident(DisasterType.Fire, PanicSecondaryEventKindV5.GasFlare, "Gaz vanası açık kaldı: küçük bir alevlenme oldu");
            MemoIncident(DisasterType.Storm, PanicSecondaryEventKindV5.ElectricalShort, "Elektrik açık kaldı: küçük bir kısa devre oluştu");
            yield return BagAndShelter(false, true); if (!w.Model.ReachedShelterV5) yield break;
            AudioListener.volume = 1f; AudioListener.pause = false; listenerHeld = true; Fixture("listener", "Own isolated native player AudioListener volume1/pausefalse; exact initial volume/pause restored on finish/disable");
            while (w.Model.Phase == PanicPhase.Disaster && Read<float>(w, "residenceCinematicAge") < 3.8f) yield return null;
            foreach (var type in new[] { DisasterType.Fire, DisasterType.Storm }) yield return ProbeAudio("inside", type, 5);
            while (w.Model.Phase == PanicPhase.Disaster && !Read<bool>(w, "AudioExteriorCameraV56")) yield return null;
            foreach (var type in new[] { DisasterType.Fire, DisasterType.Storm }) yield return ProbeAudio("exterior", type, 10);
            foreach (var type in new[] { DisasterType.Fire, DisasterType.Storm })
            {
                var inside = report.audio.Where(a => a.hazard == type.ToString() && a.scenario == "inside").ToArray(); var outside = report.audio.Where(a => a.hazard == type.ToString() && a.scenario == "exterior").ToArray();
                Check("audio.shelter-inside-filter." + type, inside.Length == 5 && inside.All(a => a.realShelter && !a.exterior) && inside.Max(a => a.cutoff) < 2000, "Actual near-complete shelter mixing before camera leaves", "NATIVE_SOURCE_FILTER_STATE");
                Check("audio.exterior-filter-and-gain." + type, outside.Length == 10 && outside.All(a => a.realShelter && a.exterior && a.cutoff >= 15000) && outside.Max(a => a.gain) > inside.Max(a => a.gain) * 2f, "Actual4.7s exterior camera predicate, listener still real admitted shelter", "NATIVE_SOURCE_FILTER_STATE");
                Check("audio.exterior-source-pcm." + type, outside.Max(a => a.sourceRms) > .000001f && outside.Max(a => a.listenerRms) > .000001f && outside.Max(a => a.dspTime) > outside.Min(a => a.dspTime) + .2, "Real source GetOutputData and listener GetOutputData plus advancing DSP; no listening/Windows output claim", "NATIVE_SOURCE_AND_LISTENER_PCM");
            }
            var fireSource = (AudioSource)Call(fx, "HazardSourceV56", DisasterType.Fire); float fullGain = fireSource.volume;
            SettingCall("SetVolumes", .5f, 1f, 1f); yield return Wait(.1f);
            Check("audio.master-applied-once-real-source", fullGain > 0 && Mathf.Abs(fireSource.volume / fullGain - .5f) < .02f && AudioListener.volume == 1f, "Actual hazard source gain ratio=" + fireSource.volume / Mathf.Max(.0000001f, fullGain) + "; listener remains1", "NATIVE_SOURCE_GAIN");
            SettingCall("SetMuted", true); yield return Wait(.1f);
            Check("audio.mute-real-hazard-source", fireSource.volume == 0 && AudioListener.volume == 1f, "Master mute operates in app mix, preserving listener fixture", "NATIVE_SOURCE_GAIN");
            SettingCall("SetMuted", false); SettingCall("SetVolumes", 1f, 1f, 1f); yield return Wait(.1f);
            yield return Capture("04-basement-fire-storm-exterior", "Actual naturally aged Fire+Storm disaster camera; actor physically admitted to basement.");
            float until = Time.realtimeSinceStartup + 10; while (w.Model.Phase != PanicPhase.Aftermath && Time.realtimeSinceStartup < until) yield return null; yield return Wait(.15f);
            Check("aftermath.natural-one-root", Read<int>(w, "ResidenceAftermathSetupCountV56") == 1 && Read<int>(w, "ResidenceAftermathWaveV56") == w.Model.Wave && Read<bool>(w, "ResidenceAftermathActiveV56") && Read<bool>(w, "ResidenceRescueActiveV56"));
            int dispatch = Read<int>(w, "SecondaryAudioDispatchCountV56"), incidents = w.Model.SecondaryEventsV5.Count;
            Check("secondary.committed-two-events", incidents == 2 && w.Model.SecondaryDamageV5 > 0, "Memoized immutable2pt FireGas+StormElectric fixtures resolved by natural8s production model");
            Check("secondary.semantic-once-per-event", dispatch == incidents && Read<string>(w, "LastSecondaryAudioCueV56").StartsWith("v56_", StringComparison.Ordinal), "dispatch=" + dispatch + " incidents=" + incidents);
            string state = ResolutionFingerprint(); for (int i = 0; i < 12; i++) Call(w, "TickHouseAudio");
            Check("secondary.repeated-router-no-rng-no-duplicate", ResolutionFingerprint() == state && Read<int>(w, "SecondaryAudioDispatchCountV56") == dispatch);
            yield return new WaitForEndOfFrame(); Rect alert = Read<Rect>(hud, "SecondaryAlertRectV56"); var labels = Read<Rect[]>(hud, "DamageLabelRectsV56");
            Check("alert.marker-reservations-nonoverlap", Read<bool>(hud, "SecondaryAlertVisibleV55") && alert.width > 0 && labels.All(r => !r.Overlaps(alert)), "Alert=" + alert + "; current frame marker reservation rectangles=" + labels.Length, "NATIVE_REPAINT_LAYOUT");
            Check("alert.overlap-query-rejects-covered-rect", !(bool)Call(hud, "WorldMarkerAvoidsAlertV56", alert) && (bool)Call(hud, "WorldMarkerAvoidsAlertV56", new Rect(0, 0, 5, 5)), "Query actual alert rectangle and unrelated small upper-left rectangle", "NATIVE_LAYOUT_QUERY");
            yield return Capture("05-secondary-alert-after-natural", "Actual committed compound incident alert and actual world damage labels; owner inspect readable reasons.");
            int aftermathRoot = Read<int>(w, "ResidenceAftermathRootIdV56"), rescueRoot = Read<int>(w, "ResidenceRescueRootIdV56");
            w.SkipResidenceCinematic(); report.skipCalls++; yield return Wait(.1f); state = ResolutionFingerprint(); int count = Read<int>(w, "ResidenceAftermathSetupCountV56");
            w.SkipResidenceCinematic(); report.skipCalls++; yield return Wait(.1f);
            Check("aftermath.repeated-skip-idempotent", count == 1 && Read<int>(w, "ResidenceAftermathSetupCountV56") == 1 && state == ResolutionFingerprint() && aftermathRoot != 0 && rescueRoot != 0 && Read<int>(w, "ResidenceAftermathRootIdV56") == aftermathRoot && Read<int>(w, "ResidenceRescueRootIdV56") == rescueRoot, "Actual retained aftermath root=" + aftermathRoot + "; rescue root=" + rescueRoot);
            yield return AcknowledgeLedger("natural-aftermath-complete");
        }
        IEnumerator EarlySkipAndCompound()
        {
            yield return Begin(DisasterType.Fire, (1 << (int)DisasterType.Fire) | (1 << (int)DisasterType.Storm)); yield return BagAndShelter(false, false);
            int water = w.Model.PackedWater, food = w.Model.PackedFood; yield return Wait(.35f); w.SkipResidenceCinematic(); report.skipCalls++; yield return Wait(.15f);
            Check("aftermath.disaster-enter-crosses-shared-setup", Read<int>(w, "ResidenceAftermathSetupCountV56") == 1 && Read<bool>(w, "ResidenceAftermathActiveV56") && Read<bool>(w, "ResidenceRescueActiveV56"), "Public SkipResidenceCinematic from real Disaster; no model Tick fixture");
            Check("aftermath.disaster-enter-consumes-once", w.Model.PackedWater == Mathf.Max(0, water - 1) && w.Model.PackedFood == Mathf.Max(0, food - 1));
            string state = ResolutionFingerprint(); int aftermathRoot = Read<int>(w, "ResidenceAftermathRootIdV56"), rescueRoot = Read<int>(w, "ResidenceRescueRootIdV56"); w.SkipResidenceCinematic(); report.skipCalls++; yield return Wait(.12f);
            Check("aftermath.disaster-enter-repeat-stable", state == ResolutionFingerprint() && Read<int>(w, "ResidenceAftermathSetupCountV56") == 1 && aftermathRoot != 0 && rescueRoot != 0 && Read<int>(w, "ResidenceAftermathRootIdV56") == aftermathRoot && Read<int>(w, "ResidenceRescueRootIdV56") == rescueRoot);
            yield return AcknowledgeLedger("early-enter-aftermath-complete");
            yield return Capture("06-enter-skip-aftermath", "Early Disaster skip uses same aftermath/rescue setup; numerical resolution once.");
            yield return Begin(DisasterType.Fire, (1 << (int)DisasterType.Fire) | (1 << (int)DisasterType.Flood));
            int pipeDispatchBefore = Read<int>(w, "SecondaryAudioDispatchCountV56");
            MemoIncident(DisasterType.Flood, PanicSecondaryEventKindV5.PipeLeak, "Su vanası açık kaldı: tesisatta su hasarı oluştu");
            yield return BagAndShelter(true, false);
            while (w.Model.Phase == PanicPhase.Disaster && Read<float>(w, "residenceCinematicAge") < 5.1f) yield return null;
            Check("story.upper-compound-caption", w.Model.ReachedShelterV5 && w.UsesUpperPanicRoomV5 && w.CinematicTitle.IndexOf("sel", StringComparison.OrdinalIgnoreCase) >= 0 && w.CinematicTitle.IndexOf("yangın", StringComparison.OrdinalIgnoreCase) >= 0, w.CinematicTitle);
            yield return Capture("07-upper-compound-caption", "Actual admitted upper room and naturally aged compound Flood+Fire subtitle.");
            w.SkipResidenceCinematic(); report.skipCalls++; yield return Wait(.1f);
            var pipes = w.Model.SecondaryEventsV5.Where(e => e.Kind == PanicSecondaryEventKindV5.PipeLeak).ToArray();
            Check("secondary.pipe-leak-actual-commit", pipes.Length == 1 && pipes[0].DamagePoints > 0 && w.Model.SecondaryDamageV5 > 0,
                "Immutable2pt Flood/PipeLeak memo fixture committed only by real compound damage resolution; committed pipes=" + pipes.Length);
            yield return AcknowledgeLedger("upper-compound-before-boat-pcm");
            // The real result ledger suspends production audio. Observe its ordinary release
            // before sampling the committed cue; do not force pause flags, call Play or Tick.
            float pipeWaitStarted = Time.realtimeSinceStartup, pipeWaitUntil = pipeWaitStarted + 2f;
            while (Time.realtimeSinceStartup < pipeWaitUntil &&
                (w.InputLocked || w.Model.IsPaused || w.Model.WaveLedgerOpenV56 || w.Model.KeyboardHelpOpenV56 || w.NotebookBlocksInput ||
                 Read<int>(w, "SecondaryAudioDispatchCountV56") < pipeDispatchBefore + w.Model.SecondaryEventsV5.Count)) yield return null;
            yield return new WaitForEndOfFrame();
            Check("secondary.pipe-leak-production-audio-unblocked", !w.InputLocked && !w.Model.IsPaused && !w.Model.WaveLedgerOpenV56 && !w.Model.KeyboardHelpOpenV56 && !w.NotebookBlocksInput,
                "Production ledger acknowledged before cue sample; ordinary World frames waited=" + (Time.realtimeSinceStartup - pipeWaitStarted) + "s (bounded2s). No audio Play/Tick or private modal/pause flag writes.", "NATIVE_PRODUCTION_MODAL_RELEASE");
            var pipeClip = PanicAudioBank.Get("v56_pipe_leak");
            var pipeVoices = Read<AudioSource[]>(fx, "voices").Where(s => s && s.clip == pipeClip).ToArray();
            int pipeDispatch = Read<int>(w, "SecondaryAudioDispatchCountV56");
            Check("secondary.pipe-leak-semantic-once", pipeClip && pipeVoices.Length == 1 && Read<Dictionary<string, float>>(fx, "last").ContainsKey("v56_pipe_leak") &&
                pipeDispatch == pipeDispatchBefore + w.Model.SecondaryEventsV5.Count,
                "Actual Foley voice assigned exact v56_pipe_leak AudioClip once; total dispatch delta=" + (pipeDispatch - pipeDispatchBefore) + "; actual committed events=" + w.Model.SecondaryEventsV5.Count + ". Concurrent actual Fire event remains permitted.", "NATIVE_COMMITTED_EVENT_SOURCE_BINDING");
            string pipeState = ResolutionFingerprint(), pipeRng = JsonUtility.ToJson(UnityEngine.Random.state);
            for (int i = 0; i < 12; i++) Call(w, "TickHouseAudio");
            Check("secondary.pipe-leak-repeat-no-rng-no-duplicate", ResolutionFingerprint() == pipeState && JsonUtility.ToJson(UnityEngine.Random.state) == pipeRng && Read<int>(w, "SecondaryAudioDispatchCountV56") == pipeDispatch,
                "Twelve read-only audio router calls preserve actual model resolution/secondary roll count/global Unity RNG and dispatch count.");
            yield return ZodiacFoley();
        }
        MonoBehaviour[] NamedComponents(string name)
        { return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(c => c && c.GetType().Name == name).ToArray(); }
        IEnumerator NpcFoley()
        {
            var npcs = NamedComponents("PanicNpcFoleyV56").Where(n => n.gameObject.activeInHierarchy).ToArray();
            Check("npc.real-scene-bindings", npcs.Length > 0 && npcs.All(n => n.GetComponentsInChildren<AudioSource>(true).Length == 2), "Active production walkers=" + npcs.Length + "; exactly two local sources per NPC", "NATIVE_SCENE_AUDIO_BINDING");
            if (npcs.Length == 0) yield break;
            yield return Place(npcs[0].transform.position + Vector3.right * 2f + Vector3.up * .05f);
            int initial = npcs.Sum(n => Read<int>(n, "FootfallCountV56")); bool played = false; float until = Time.realtimeSinceStartup + 1.6f;
            while (Time.realtimeSinceStartup < until) { played |= npcs.Any(n => Read<bool>(n, "IsPlayingV56") && Read<float>(n, "OutputGainV56") > 0); yield return null; }
            Check("npc.actual-moving-stride-audio", npcs.Sum(n => Read<int>(n, "FootfallCountV56")) > initial && played, "Actual authored moving NPCs; nearby real actor fixture; no synthetic transform movement", "NATIVE_SOURCE_PLAYBACK");
            hud.SetPaused(true); yield return Wait(.25f); int held = npcs.Sum(n => Read<int>(n, "FootfallCountV56")); yield return Wait(.2f);
            Check("npc.pause-stops-and-no-strides", npcs.All(n => !Read<bool>(n, "IsPlayingV56") && Read<float>(n, "OutputGainV56") == 0) && held == npcs.Sum(n => Read<int>(n, "FootfallCountV56")));
            hud.SetPaused(false); yield return Wait(.1f);
        }
        IEnumerator ZodiacFoley()
        {
            var boats = NamedComponents("PanicZodiacAudioV56").Where(n => n.gameObject.activeInHierarchy).ToArray();
            Check("zodiac.real-scene-binding", boats.Length == 1 && boats[0].GetComponentsInChildren<AudioSource>(true).Length == 2, "Actual response boat sources; primary Fire with secondary Flood triggered boat", "NATIVE_SCENE_AUDIO_BINDING");
            if (boats.Length != 1) yield break;
            var boat = boats[0]; yield return Place(boat.transform.position + Vector3.right * 2f + Vector3.up * .08f); yield return Wait(2.6f);
            var motor = Read<AudioSource>(boat, "MotorSourceV56"); var water = Read<AudioSource>(boat, "WaterSourceV56"); float motorRms = 0, waterRms = 0;
            for (int i = 0; i < 8; i++) { var data = new float[2048]; motor.GetOutputData(data, 0); motorRms = Mathf.Max(motorRms, Rms(data)); water.GetOutputData(data, 0); waterRms = Mathf.Max(waterRms, Rms(data)); yield return Wait(.04f); }
            Check("zodiac.actual-motion-source-pcm", Read<bool>(boat, "IsPlayingV56") && motorRms > .000001f && waterRms > .000001f, "MotorRms=" + motorRms + "; waterRms=" + waterRms + "; real authored boat motion", "NATIVE_SOURCE_PCM");
            hud.SetPaused(true); yield return Wait(.25f); Check("zodiac.pause-stops-sources", !Read<bool>(boat, "IsPlayingV56") && Read<float>(boat, "OutputGainV56") == 0 && !motor.isPlaying && !water.isPlaying);
            hud.SetPaused(false); yield return Wait(.25f); Check("zodiac.resume-moving-sources", Read<bool>(boat, "IsPlayingV56") && Read<float>(boat, "OutputGainV56") > 0);
        }
        IEnumerator LongFailure()
        {
            yield return Begin(DisasterType.Fire, (1 << (int)DisasterType.Fire) | (1 << (int)DisasterType.Storm));
            w.Model.Wave = 50; Set(w.Model, "compoundWaveV54", 50);
            Fixture("high-severity-wave", "Wave50 with same Fire+Storm mask; production V5.2 risk curve/75%loss used unchanged; empty real bag intentionally retained");
            yield return null; yield return BagAndShelter(false, false, false); yield return Wait(.25f);
            w.SkipResidenceCinematic(); report.skipCalls++; yield return Wait(.2f);
            yield return AcknowledgeLedger("loss-before-result-render");
            Check("failure.real-empty-bag-and-high-damage", w.Model.Phase == PanicPhase.GameOver && w.Model.TotalDamage >= 75 && w.Model.FailureReason.Length > 130 && w.Model.FailureReason.Contains("1 su") && w.Model.FailureReason.Contains("1 gıda") && w.Model.FailureReason.Contains("%75"), w.Model.FailureReason + "; length=" + w.Model.FailureReason.Length);
            foreach (int width in new[] { 960, 1600 })
            {
                yield return Resolution(width, width == 960 ? 540 : 900); yield return new WaitForEndOfFrame();
                string text = Read<string>(hud, "ResultFailureTextV56"); Rect rect = Read<Rect>(hud, "ResultFailureRectV56"); var style = Read<GUIStyle>(hud, "_wrap");
                float needed = style.CalcHeight(new GUIContent(text), rect.width);
                Check("failure.full-real-reason-rendered." + width, text == w.Model.FailureReason && text.EndsWith("ev kaybedilir.", StringComparison.Ordinal) && needed <= rect.height + .5f, "Actual drawn text length=" + text.Length + "; actual style neededHeight=" + needed + "; actual rect=" + rect, "NATIVE_REPAINT_TEXT_LAYOUT");
                yield return Capture("08-long-failure-" + width, "Real empty-bag+Wave50 production high-damage result; full reason and75%threshold must remain readable.");
            }
        }
        IEnumerator Run()
        {
            yield return Wait(.4f); Check("launch.isolated-guid-profile", Isolated() && PanicTestLaunchRules.Drivers.Contains(Flag));
            Check("launch.graphical", !Application.isBatchMode && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null);
            if (!Isolated()) yield break;
            yield return Settings(); yield return Resolution(1600, 900); yield return Begin(DisasterType.Earthquake, 1);
            yield return AssistantBudget(); yield return MapLabels(); yield return HammerAndUtilities(); yield return NpcFoley();
            yield return Begin(DisasterType.Fire, (1 << (int)DisasterType.Fire) | (1 << (int)DisasterType.Storm)); yield return NaturalAudioAndAftermath();
            yield return EarlySkipAndCompound(); yield return LongFailure(); Check("runtime.no-errors", report.errors.Count == 0, string.Join("\n", report.errors));
        }
        void RestoreState()
        {
            if (settingsHeld)
            {
                SettingCall("SetVolumes", oldMaster, oldMusic, oldSfx); SettingCall("SetMuted", oldMuted); SettingSet("MusicEnabled", oldMusicEnabled); SettingCall("Save");
                report.settingsRestored = Mathf.Abs((float)Setting("MasterVolume") - oldMaster) < .001f && Mathf.Abs((float)Setting("MusicVolume") - oldMusic) < .001f && Mathf.Abs((float)Setting("SfxVolume") - oldSfx) < .001f && (bool)Setting("Muted") == oldMuted && (bool)Setting("MusicEnabled") == oldMusicEnabled; settingsHeld = false;
            }
            if (listenerHeld) { AudioListener.volume = report.listenerBefore; AudioListener.pause = report.listenerPauseBefore; listenerHeld = false; }
            report.listenerAfter = AudioListener.volume; report.listenerPauseAfter = AudioListener.pause;
            report.listenerRestored = report.listenerAfter == report.listenerBefore && report.listenerPauseAfter == report.listenerPauseBefore;
        }
        void Finish()
        {
            if (finished) return; finished = true;
            try { RestoreState(); } catch (Exception ex) { report.error += "\nrestore: " + ex; }
            report.realScoreAfter = Hash(realScore); Check("score.real-file-unchanged", report.realScoreBefore == report.realScoreAfter, report.realScoreBefore + " -> " + report.realScoreAfter, "NATIVE_FILE_HASH");
            report.globalPreferencesAfter = GlobalPreferenceFingerprint(); Check("audio.global-preferences-unchanged", report.globalPreferencesBefore == report.globalPreferencesAfter, "Read-only default namespace existence/value fingerprint before/after; QA wrote only evidence-derived prefix", "NATIVE_GLOBAL_PREFERENCE_READS");
            Check("score.still-isolated", Isolated(), PanicModel.ScorePathOverride, "NATIVE_FILE_PATH");
            Check("listener.exact-state-restored", report.listenerRestored); Check("settings.exact-state-restored", report.settingsRestored);
            report.seconds = Time.realtimeSinceStartup - began; report.passed = report.complete && report.errors.Count == 0 && report.checks.Count >= 50 && report.checks.All(c => c.status == "PASS");
            Application.logMessageReceived -= Log;
            if (!string.IsNullOrEmpty(w.EvidencePath)) File.WriteAllText(Path.Combine(w.EvidencePath, "v56-smoke.json"), JsonUtility.ToJson(report, true));
            Debug.Log("V56_SMOKE " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " seconds=" + report.seconds); Application.Quit(report.passed ? 0 : 2);
        }
        static bool PngSize(string path, out int width, out int height)
        {
            width = height = 0;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length < 2048) return false; var b = File.ReadAllBytes(path);
                if (b.Length < 24 || b[0] != 137 || b[1] != 80 || b[2] != 78 || b[3] != 71) return false;
                width = (b[16] << 24) | (b[17] << 16) | (b[18] << 8) | b[19]; height = (b[20] << 24) | (b[21] << 16) | (b[22] << 8) | b[23]; return width > 0 && height > 0;
            }
            catch (IOException) { return false; }
        }
        static string Hash(string path)
        { if (!File.Exists(path)) return "absent"; using (var h = SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""); }
        static string GlobalPreferenceFingerprint()
        {
            const string prefix = "TLTP.Audio.V56."; string text = "";
            foreach (string key in new[] { "master", "music", "sfx" }) text += key + ":" + PlayerPrefs.HasKey(prefix + key) + ":" + PlayerPrefs.GetFloat(prefix + key, -999f).ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "|";
            foreach (string key in new[] { "mute", "musicEnabled" }) text += key + ":" + PlayerPrefs.HasKey(prefix + key) + ":" + PlayerPrefs.GetInt(prefix + key, -999) + "|";
            return text;
        }
    }
}
