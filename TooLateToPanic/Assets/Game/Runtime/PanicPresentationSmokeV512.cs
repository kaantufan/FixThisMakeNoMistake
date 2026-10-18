using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    // Isolated production API/framebuffer driver, derived from PanicOnboardingSmokeV510.
    // Actor/camera fixtures are declared; this is not an OS-input or natural traversal claim.
    [DefaultExecutionOrder(10000)]
    public sealed class PanicPresentationSmokeV512 : MonoBehaviour
    {
        const string Flag = "--presentation-v512-smoke";
        const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public class Row { public string id, detail; public bool pass; }
        [Serializable] public class Shot { public string path, sha256, context; public int width, height, nonDark; }
        [Serializable] public class Report
        {
            public string utc, dllSha256, scorePath, realScoreBefore, realScoreAfter, error;
            public string layer = "CURRENT_DLL_NATIVE_PRODUCTION_API_AND_FRAMEBUFFER_WITH_DECLARED_FIXTURES";
            public bool complete, passed, OSInput, humanAcceptance;
            public float seconds;
            public int productionStarts, setupWarps, sourceCalls, workBegins, hammerInputs, workCommits, modelApiRows;
            public List<Row> checks = new List<Row>();
            public List<Shot> captures = new List<Shot>();
            public List<string> fixtures = new List<string>();
            public List<string> errors = new List<string>();
        }
        readonly Report report = new Report();
        PanicHomeWorld w; PanicHomeHud hud;
        float began; bool finished; string realScore;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (world) world.gameObject.AddComponent<PanicPresentationSmokeV512>();
        }
        void Start()
        {
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); began = Time.realtimeSinceStartup;
            if (!w || !hud || string.IsNullOrEmpty(w.EvidencePath)) { Debug.LogError("PRESENTATION_V512 isolated evidence/World/HUD required"); Application.Quit(2); return; }
            report.utc = DateTime.UtcNow.ToString("o"); report.dllSha256 = Hash(typeof(PanicModel).Assembly.Location);
            report.scorePath = PanicModel.ScorePathOverride; realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.realScoreBefore = Hash(realScore);
            Application.logMessageReceived += Log; Application.runInBackground = true;
            StartCoroutine(Guard(Run()));
        }
        void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Check(string id, bool pass, string detail = "")
        { report.checks.Add(new Row { id = id, pass = pass, detail = detail }); Debug.Log("PRESENTATION_V512 " + (pass ? "PASS " : "FAIL ") + id + " " + detail); }
        static string Hash(string path)
        { if (!File.Exists(path)) return "absent"; using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""); }
        static void Set(object target, string name, object value) { target.GetType().GetField(name, Fields).SetValue(target, value); }
        static T Read<T>(object target, string name)
        { var type = target.GetType(); var property = type.GetProperty(name, Fields); return (T)(property != null ? property.GetValue(target) : type.GetField(name, Fields).GetValue(target)); }
        static object Call(object target, string name, params object[] args)
        { return target.GetType().GetMethods(Fields).First(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(target, args); }
        IEnumerator Wait(float seconds) { float until = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < until) yield return null; }
        IEnumerator Resolution(int width, int height)
        { Screen.SetResolution(width, height, FullScreenMode.Windowed); yield return Wait(.3f); Check("resolution." + width, Screen.width == width && Screen.height == height, "Actual framebuffer=" + Screen.width + "x" + Screen.height); }
        IEnumerator Capture(string name, string context)
        {
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (!texture) { Check("capture." + name, false, "Missing framebuffer"); yield break; }
            string path = Path.Combine(w.EvidencePath, name + ".png"); File.WriteAllBytes(path, texture.EncodeToPNG());
            var pixels = texture.GetPixels32(); int nonDark = 0; byte min = 255, max = 0;
            for (int i = 0; i < pixels.Length; i += 137) { var p = pixels[i]; byte l = (byte)((p.r + p.g + p.b) / 3); if (l > 12) nonDark++; min = Math.Min(min, l); max = Math.Max(max, l); }
            Check("capture." + name, nonDark > 80 && max - min > 18, "Actual framebuffer " + texture.width + "x" + texture.height + "; nonDark=" + nonDark);
            report.captures.Add(new Shot { path = path, sha256 = Hash(path), context = context, width = texture.width, height = texture.height, nonDark = nonDark }); Destroy(texture);
        }
        // ROOT_MODEL_CHECK_HOOK: assign before the driver runs, or add a direct current-DLL model check call here.
        public static Action<Action<string, bool, string>> AdditionalModelChecksV512;
        [Serializable] public class CameraSample
        {
            public string name, kind, caption;
            public float age, fov;
            public Vector3 camera, actor;
            public bool controllerEnabled, reachedShelter;
        }
        readonly List<CameraSample> cameraSamples = new List<CameraSample>();
        bool sampleCamera;
        float sampleAge;

        IEnumerator BeginNormal(bool skip = true)
        {
            sampleCamera = false; w.InputLocked = false; hud.SetPaused(false); hud.ResetKeyboardHelpPresentationV56();
            if (w.Notebook != null) w.Notebook.ForceClosed();
            w.ReturnToMenu(); w.StartGame(false); report.productionStarts++; w.FullFlowScriptedInput = true; yield return null;
            if (hud.FreeplayOnboardingOpenV414) hud.ConfirmFreeplayOnboardingV414(); yield return null;
            if (skip) w.SkipIntroStoryV414(); yield return Wait(.12f);
        }
        IEnumerator Run()
        {
            bool isolated = report.scorePath == Path.Combine(w.EvidencePath, "isolated-home-scores.json");
            Check("isolated", isolated && PanicTestLaunchRules.Drivers.Contains(Flag), report.scorePath);
            if (!isolated) throw new InvalidOperationException("Fresh isolated score profile required.");
            report.fixtures.Add("Dedicated graphical V512 profile. No OS input/human/natural traversal claim. Current-DLL roof regression is retained; its model fixtures remain separate from live presentation.");
            report.fixtures.Add("Camera samples explicitly set residenceCinematicAge and InputLocked, then call the current production ApplyResidenceCinematicCamera/TickStoryCaption/ApplyFloorVisibility methods. No replacement camera trajectory or shelter admission. Camera-only Success samples require real physical upper-room admission and are separately labeled from a successful wave outcome.");
            report.fixtures.Add("Lifecycle phase/compound-mask fixtures enter actual Model Disaster; Model.Tick performs real damage/outcome resolution. Existing NoLoss permits unsheltered inspection with no award and no fabricated admission. Actual FinishInspection begins next wave while old response root survives. Paused/modal stale-root reactivation is a declared adversarial presentation fixture.");
            w.FullFlowScriptedInput = true;
            int before = report.checks.Count; PanicRoofWallChecksV511.Run(Check); PanicPresentationChecksV512.Run(Check); report.modelApiRows = report.checks.Count - before;
            PolicyChecks();
            if (AdditionalModelChecksV512 != null) AdditionalModelChecksV512(Check);
            else report.fixtures.Add("ROOT_MODEL_CHECK_HOOK not supplied: no additional custom checks beyond the directly called roof and V512 presentation/model suites.");
            yield return Resolution(1600, 900);
            yield return OpeningViews();
            yield return AnchorableTable();
            yield return ResponseLifecycle();
            yield return UpperRoom();
            yield return ShortFlowExclusion();
            Check("runtime.no-errors", report.errors.Count == 0, string.Join("\n", report.errors));
            File.WriteAllText(Path.Combine(w.EvidencePath, "presentation-v512-camera-samples.json"), JsonUtility.ToJson(new CameraSamples { rows = cameraSamples }, true));
            report.complete = true;
        }
        [Serializable] public class CameraSamples { public List<CameraSample> rows; }
        void PolicyChecks()
        {
            foreach (PanicPhase phase in Enum.GetValues(typeof(PanicPhase)))
            foreach (bool shortHazard in new[] { false, true })
            {
                bool hazard = phase == PanicPhase.Disaster || shortHazard;
                bool recovery = !hazard && new[] { PanicPhase.Aftermath, PanicPhase.Success, PanicPhase.Inspection, PanicPhase.GameOver }.Contains(phase);
                string id = "policy." + phase + ".short=" + shortHazard;
                Check(id + ".hazard", PanicOutdoorSafetyV512.HazardActive(phase, shortHazard) == hazard);
                Check(id + ".recovery", PanicOutdoorSafetyV512.RecoveryAllowed(phase, shortHazard) == recovery);
                Check(id + ".receipt", !PanicOutdoorSafetyV512.RescueAllowed(phase, shortHazard, false) &&
                    PanicOutdoorSafetyV512.RescueAllowed(phase, shortHazard, true) == recovery);
            }
        }
        IEnumerator AnchorableTable()
        {
            sampleCamera = false; w.InputLocked = false;
            w.SetFocus("bookshelf"); w.Expedition.CanBeginWork("anchorfloor", "bookshelf"); yield return null;
            w.Warp(new Vector3(-4.05f, .05f, -1.25f)); report.setupWarps++;
            for (int n = 0; n < 8; n++) { w.SmokeMove(Vector2.zero, .025f); yield return null; }
            w.SetFocus("table"); w.Model.Notify("Önlem gerekiyor; masanı güvene al."); yield return Wait(.3f);
            var table = w.Model.Items.Find(i => i.Id == "table");
            string action = w.Expedition.PrimaryActionV414(table);
            Check("world.anchorable-table-no-false-demand", w.Selected == table && w.CanInteract && PanicItemActionRules.CanAnchor(table) &&
                (action == "anchorwall" || action == "anchorfloor") && PanicItemActionRules.Allows(action, table, table.Id) &&
                w.Expedition.PrimaryShortcutV414(table) == 1 &&
                !w.Expedition.HasMissingRequirements && string.IsNullOrEmpty(w.Expedition.RequirementTargetId) && string.IsNullOrEmpty(w.ShortNotice),
                "Actual table exposes a valid anchor action/shortcut; approach clears earlier bookshelf shortage and generic gerekiyor text never creates a material-demand popup.");
            yield return Capture("anchorable-table-no-false-warning", "Actual nearby table offers real sabitleme action; no automatic material warning or stale requirements from approach/generic narrative wording. Real risk remains; no immunity.");
        }
        IEnumerator OpeningViews()
        {
            yield return BeginNormal(false); w.InputLocked = true;
            var before = new GameplaySnapshot(w);
            Vector3 actor = Read<Vector3>(w, "residenceReturnPosition");
            bool controller = Read<bool>(w, "residenceSavedControllerEnabled");
            int resolved = w.IntroStoryResolutionCountV414;
            foreach (float age in new[] { .2f, 4.1f, 8.1f, 12.1f })
                yield return Sample("intro-" + age.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture), age, "Opening: explicit shot-age fixture, real caption/letterbox and current authored camera.");
            Check("camera.opening-four-angles", cameraSamples.Select(s => s.camera).Distinct().Count() >= 4 && before.Matches(w),
                "Four actual camera poses, same untouched live gameplay snapshot.");
            sampleCamera = false; w.SkipResidenceCinematic();
            Check("camera.opening-skip-preserves-state", !w.CinematicActive && w.IntroStoryResolutionCountV414 == resolved + 1 &&
                before.Matches(w) && Vector3.Distance(actor, w.PlayerPosition) < .001f && controller == w.PlayerControllerEnabled && !w.Model.ReachedShelterV5,
                "Actual skip restores the pre-cinematic saved actor/controller (the presentation controller is deliberately disabled), and never manufactures admission. ExpectedController=" + controller + ";actual=" + w.PlayerControllerEnabled + ";actorDelta=" + Vector3.Distance(actor, w.PlayerPosition));
            w.SkipResidenceCinematic(); Check("camera.opening-skip-idempotent", w.IntroStoryResolutionCountV414 == resolved + 1);
            w.InputLocked = false;
        }
        IEnumerator ResponseLifecycle()
        {
            w.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss, true);
            w.Warp(new Vector3(0, .05f, -9)); report.setupWarps++; yield return null;
            SetDisaster(false); yield return Wait(.12f); ObserveForbidden("first-quake", true);
            yield return Sample("quake-exterior-establish", 4.8f, "First real model Disaster, unsheltered; declared camera age4.8; all owned outside people/response/heli absent.");
            yield return Sample("quake-exterior-detail", 6.3f, "Same unresolved first quake, authored facade-detail angle at6.3; no response actors.");
            sampleCamera = false; AdvanceModel(8.01f); yield return Wait(.35f);
            Check("response.first-aftermath-real-resolution", w.Model.Phase == PanicPhase.Aftermath && w.ResidenceAftermathWaveV56 == w.Model.Wave &&
                w.RecoveryResponseAllowedV512 && w.ResidenceRescueRootIdV56 != 0 && !w.Model.ReachedShelterV5,
                "Actual Model.Tick resolved damage and production aftermath receipt; NoLoss never invents shelter arrival.");
            var street = w.GetComponent<PanicStreetLifeV414>();
            Check("response.first-recovery-enabled", street.ResponseActive && street.ResponseVehicleCount > 0 && w.HelicopterVisible,
                "Current-wave recovery enables actual retained vehicle/heli roots after resolution; responders may still be travelling.");
            int oldHeli = w.ResidenceRescueRootIdV56;
            AdvanceModel(4.01f); yield return Wait(.1f);
            Check("response.inspection-allowed", w.Model.Phase == PanicPhase.Inspection && w.RecoveryResponseAllowedV512 &&
                w.Model.LastReward == 0 && w.Model.CompletedWaves == 0 && !w.Model.ReachedShelterV5,
                "Existing NoLoss inspection is explicitly rewardless and unsheltered.");
            w.Model.SetWaveLedgerOpenV56(true); w.Model.SetWaveLedgerOpenV56(false);
            PanicPhase from = w.Model.Phase; w.Model.FinishInspection(); SyncPhase(from); yield return Wait(.15f);
            Check("response.following-wave-old-root-retained", w.Model.Phase == PanicPhase.Preparing && w.Model.Wave == 2 &&
                w.ResidenceRescueRootIdV56 == oldHeli && oldHeli != 0, "Actual FinishInspection -> next Preparing, same previous rescue object retained.");
            ObserveForbidden("following-preparing", false);

            // Open a real modal before the explicit disaster-state fixture. Update will early-return;
            // late safety must still hide deliberately reactivated old owned roots.
            w.InputLocked = false; bool opened = hud.OpenKeyboardHelpV56(); yield return null;
            Check("response.modal-fixture-opened", opened && w.Model.KeyboardHelpOpenV56, "Real F1 modal opened while Preparing, then retained over the declared phase fixture.");
            SetDisaster(true); ForceOwnedOutdoorRoots(); yield return null; yield return new WaitForEndOfFrame();
            ObserveForbidden("compound-modal-old-state", true);
            hud.ResetKeyboardHelpPresentationV56(); w.Model.SetPaused(true); ForceOwnedOutdoorRoots(); yield return null; yield return new WaitForEndOfFrame();
            ObserveForbidden("compound-paused-old-state", true); w.Model.SetPaused(false); w.InputLocked = true;
            yield return Sample("compound-flood-storm-before-response", 6.3f, "Actual Flood+Storm mask in unresolved model Disaster; previous-wave response roots are inactive, including during pause/modal.");
            sampleCamera = false; AdvanceModel(8.01f); yield return Wait(.35f);
            Check("response.compound-real-aftermath", w.Model.Phase == PanicPhase.Aftermath && w.ResidenceAftermathWaveV56 == 2 &&
                w.RecoveryResponseAllowedV512 && street.ResponseActive && street.ResponseVehicleCount > 0,
                "Actual compound Model.Tick resolved before current-wave response became available.");
            var effects = w.GetComponent<PanicHomeEffects>(); var storm = effects.HazardSourceV56(DisasterType.Storm);
            Check("response.recovery-storm-stopped", !effects.HasHazardVisualV54(DisasterType.Storm) && (!storm || !storm.isPlaying || storm.volume <= .0001f),
                "Actual resolved recovery removes storm hazard before rescuers; flood runoff may remain cosmetic.");
            yield return Sample("compound-flood-storm-after-response", 2.3f, "Real resolved compound aftermath, opposite exterior camera; recovery actors allowed and storm stopped.");
            sampleCamera = false;
            w.InputLocked = true; w.SkipResidenceCinematic(); yield return null;
            Check("camera.skip-real-outcome-no-arrival", w.Model.Phase == PanicPhase.Inspection && !w.Model.ReachedShelterV5 &&
                w.Model.LastReward == 0 && w.ResidenceAftermathWaveV56 == w.Model.Wave,
                "Actual guarded cinematic skip preserves unsheltered, rewardless outcome and once-per-wave aftermath.");
        }
        void SetDisaster(bool compound)
        {
            sampleCamera = false; PanicPhase from = w.Model.Phase;
            w.Model.Disaster = compound ? DisasterType.Flood : DisasterType.Earthquake;
            Set(w.Model, "compoundWaveV54", w.Model.Wave); Set(w.Model, "compoundPrimaryV54", w.Model.Disaster);
            Set(w.Model, "compoundMaskV54", compound ? (1 << (int)DisasterType.Flood) | (1 << (int)DisasterType.Storm) : 1 << (int)DisasterType.Earthquake);
            w.Model.Phase = PanicPhase.Disaster; w.Model.PhaseTime = 8; w.InputLocked = true; SyncPhase(from);
            report.fixtures.Add("MODEL_PHASE_FIXTURE Disaster wave=" + w.Model.Wave + " mask=" + w.Model.ActiveDisasterMaskV54 + "; no shelter mutation; real Model.Tick resolves later.");
        }
        void AdvanceModel(float seconds)
        { sampleCamera = false; w.Model.SetPaused(false); PanicPhase from = w.Model.Phase; w.Model.Tick(seconds); SyncPhase(from); }
        void SyncPhase(PanicPhase from)
        {
            var effects = w.GetComponent<PanicHomeEffects>(); effects.OnPhase(w.Model.Phase);
            Call(w, "OnResidencePhase", w.Model.Phase, from); Set(w, "previousPhase", w.Model.Phase);
            Call(w, "SyncVisuals", true); Call(w, "ApplyFloorVisibility");
        }
        void ObserveForbidden(string id, bool hazard)
        {
            var street = w.GetComponent<PanicStreetLifeV414>(); var effects = w.GetComponent<PanicHomeEffects>();
            var rotor = Read<AudioSource>(effects, "rotor"); var legacy = Read<AudioSource>(w, "rotorAudio");
            var root = Read<GameObject>(street, "responseRoot");
            bool noPeople = !hazard || w.ActiveExteriorNpcCount == 0 && street.CivilianCount == 0 && street.MovingVehicleCount == 0;
            Check("response.forbidden." + id, !w.RecoveryResponseAllowedV512 && (!hazard || w.OutdoorDisasterActiveV512) && noPeople &&
                !street.ResponseActive && street.ResponseVehicleCount == 0 && street.ResponderCount == 0 && (!root || !root.activeInHierarchy) &&
                !w.HelicopterVisible && (!rotor || !rotor.isPlaying && rotor.volume <= .0001f) && Read<float>(effects, "rotorGain") <= .0001f &&
                (!legacy || !legacy.isPlaying && legacy.volume <= .0001f),
                "phase=" + w.Model.Phase + "; outsideNeighbors=" + w.ActiveExteriorNpcCount + "; streetCivilians=" + street.CivilianCount +
                "; responseVehicles=" + street.ResponseVehicleCount + "; responders=" + street.ResponderCount + "; heli=" + w.HelicopterVisible + "; actual rotor silent/stopped.");
        }
        void ForceOwnedOutdoorRoots()
        {
            report.fixtures.Add("ADVERSARIAL_STALE_VISUALS re-enable only existing owned neighbor/civilian/response/heli roots; production LateUpdate safety must hide them without World.Update.");
            foreach (object neighbor in (IEnumerable)Read<object>(w, "neighbors")) { var go = Read<GameObject>(neighbor, "visual"); if (go) go.SetActive(true); }
            var street = w.GetComponent<PanicStreetLifeV414>();
            foreach (object civilian in (IEnumerable)Read<object>(street, "civilians")) { var go = Read<GameObject>(civilian, "Root"); if (go) go.SetActive(true); }
            var root = Read<GameObject>(street, "responseRoot"); if (root) root.SetActive(true);
            var heli = Read<GameObject>(w, "helicopter"); if (heli) heli.SetActive(true);
            w.GetComponent<PanicHomeEffects>().SetEnvironment(true, false, false, true);
        }
        IEnumerator UpperRoom()
        {
            yield return BeginNormal(); w.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss, false);
            w.Model.Disaster = DisasterType.Flood;
            Set(w.Model, "compoundWaveV54", w.Model.Wave); Set(w.Model, "compoundPrimaryV54", DisasterType.Flood);
            Set(w.Model, "compoundMaskV54", 1 << (int)DisasterType.Flood);
            w.Warp(new Vector3(4.2f, 3.25f, -.65f)); report.setupWarps++;
            for (int n = 0; n < 8; n++) { w.SmokeMove(Vector2.zero, .025f); yield return null; }
            Call(w, "ApplyFloorVisibility"); yield return Wait(.6f);
            var identity = GameObject.Find("V512 protected upper panic room identity");
            var marker = FindObjectsByType<HomePick>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(p => p.Id == "@panicroom").ToArray();
            Check("room.identity-real-owner", identity && identity.GetComponentsInChildren<Renderer>(true).Length > 0 &&
                identity.GetComponentsInChildren<Collider>(true).Length == 0 && identity.GetComponentsInChildren<HomePick>(true).Length == 0 && marker.Length == 1,
                "Added room fittings are real rendered decoration; original single marker owns admission, with no new collider/pickup.");
            Check("room.radio-and-label", identity && identity.GetComponentsInChildren<Transform>(true).Any(t => t.name == "V512 emergency radio display - no pickup") &&
                identity.GetComponentsInChildren<TextMesh>(true).Any(t => t.text == "PANİK ODASI"), "Retained emergency radio and Turkish room identity exist in the actual scene.");
            yield return Capture("upper-panic-room-interior", "Real upper panic-room fittings before entry, actual actor/controller floor fixture; no admission claim.");
            w.Model.TimeRemaining = w.Model.PhaseTime = 39f; report.fixtures.Add("BAG_CLOCK_FIXTURE39s; production handoff and physical marker ray admission used below.");
            w.TickPreparationStages(); yield return Wait(2.3f);
            foreach (string item in new[] { "water", "food" })
            {
                var supply = w.Expedition.WorldSupplies.FirstOrDefault(s => s.ItemId == item && s.Count > 0);
                Check("room.finite-supply." + item, supply != null && w.Expedition.TryCollectWorldSupply(supply.Id));
            }
            bool entered = false;
            foreach (Vector3 pose in new[] { new Vector3(4.2f, 3.24f, -.65f), new Vector3(3.7f, 3.24f, -.65f), new Vector3(4.7f, 3.24f, -.65f) })
            {
                w.Warp(pose); report.setupWarps++;
                for (int n = 0; n < 8; n++) { w.SmokeMove(Vector2.zero, .025f); yield return null; }
                Aim(w.ActiveShelterPositionV5 + Vector3.up * 1.35f); yield return Wait(.4f); w.SetFocus("@panicroom");
                if (w.TryEnterActiveShelterV5()) { entered = true; break; }
            }
            report.fixtures.Add("PHYSICAL_UPPER_ENTRY accepted=" + entered + "; actualReached=" + w.Model.ReachedShelterV5 + "; " + w.Model.Message);
            Check("room.admission-truth", w.Model.ReachedShelterV5 == entered, "Only actual TryEnterActiveShelterV5 may set admission; failures stay unadmitted.");
            if (!entered)
            {
                report.fixtures.Add("UPPER_SUCCESS_CAMERA NOT_RUN: actual grounded/ray/path admission rejected at all3 documented room poses; no fake success or admission substituted.");
                yield break;
            }
            yield return null; w.InputLocked = true;
            // Camera-only success fixture is explicitly separate from a successful model outcome.
            Call(w, "BeginResidenceCinematic", PanicPhase.Success, w.Model.Phase);
            report.fixtures.Add("SUCCESS_CAMERA_ONLY_FIXTURE on actual admitted upper actor; Model remains " + w.Model.Phase + ". This is not a completed-wave/reward claim.");
            yield return Sample("upper-admitted-success-medium", .2f, "Real physically admitted upper-room actor; declared Success-camera-only fixture at.2, no wave-result claim.");
            yield return Sample("upper-admitted-success-close", 1.65f, "Same real admitted actor; current closer Success camera at1.65, no synthetic shelter admission or reward.");
            sampleCamera = false;
        }
        void Aim(Vector3 target)
        {
            Vector3 direction = target - (w.PlayerPosition + Vector3.up * 1.62f); var angles = Quaternion.LookRotation(direction).eulerAngles;
            float yaw = Read<float>(w, "bagYaw"), pitch = Read<float>(w, "bagPitch");
            w.StepBagLook(new Vector2(Mathf.DeltaAngle(yaw, angles.y) / (2 * w.FpsLookMultiplierV414),
                (pitch - Mathf.DeltaAngle(0, angles.x)) / (1.7f * w.FpsLookMultiplierV414)));
        }
        IEnumerator ShortFlowExclusion()
        {
            sampleCamera = false; w.InputLocked = false; w.ReturnToMenu(); w.StartJuryDemo(); w.FullFlowScriptedInput = true; yield return null;
            if (hud.FreeplayOnboardingOpenV414) hud.ConfirmFreeplayOnboardingV414(); w.SkipIntroStoryV414();
            foreach (PanicJuryStage stage in new[] { PanicJuryStage.Earthquake, PanicJuryStage.Flood, PanicJuryStage.Fire })
            {
                Call(w.JuryDemo, "BeginStage", stage); Call(w, "RefreshJuryPresentation"); w.InputLocked = true;
                yield return null; yield return new WaitForEndOfFrame();
                Check("response.short-flow-stage." + stage, w.JuryActive && w.JuryDemo.Stage == stage &&
                    (stage == PanicJuryStage.Earthquake ? w.Model.Phase == PanicPhase.Disaster : w.Model.Phase == PanicPhase.Preparing),
                    "Declared actual short-flow BeginStage fixture; Flood/Fire intentionally retain Model.Preparing.");
                ObserveForbidden("short-flow-" + stage, true);
            }
        }
        IEnumerator Sample(string name, float age, string detail)
        {
            w.InputLocked = true; sampleAge = age; sampleCamera = true; var state = new GameplaySnapshot(w);
            Vector3 actor = w.PlayerPosition; bool controller = w.PlayerControllerEnabled;
            yield return null; yield return new WaitForEndOfFrame();
            var camera = w.View; string caption = w.CinematicTitle;
            bool finite = Finite(camera.transform.position) && Finite(camera.transform.forward) && !float.IsNaN(camera.fieldOfView) &&
                !float.IsInfinity(camera.fieldOfView) && camera.fieldOfView >= 35 && camera.fieldOfView <= 70;
            Check("camera.pose." + name, w.CinematicActive && finite && !camera.orthographic && !string.IsNullOrEmpty(caption) &&
                state.Matches(w) && Vector3.Distance(actor, w.PlayerPosition) < .002f && controller == w.PlayerControllerEnabled,
                "age=" + age + "; kind=" + w.CinematicKind + "; position=" + camera.transform.position + "; fov=" + camera.fieldOfView + "; caption=" + caption + "; gameplay/controller/admission preserved.");
            cameraSamples.Add(new CameraSample { name = name, kind = w.CinematicKind, caption = caption, age = age, fov = camera.fieldOfView,
                camera = camera.transform.position, actor = w.PlayerPosition, controllerEnabled = w.PlayerControllerEnabled, reachedShelter = w.Model.ReachedShelterV5 });
            yield return Capture(name, detail);
        }
        static bool Finite(Vector3 value)
        { return !float.IsNaN(value.x) && !float.IsNaN(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.x) && !float.IsInfinity(value.y) && !float.IsInfinity(value.z); }
        void LateUpdate()
        {
            if (!sampleCamera || !w || !w.CinematicActive) return;
            Set(w, "residenceCinematicAge", sampleAge); Call(w, "TickStoryCaptionV414");
            Call(w, "ApplyResidenceCinematicCamera", .025f); Call(w, "ApplyFloorVisibility");
        }
        sealed class GameplaySnapshot
        {
            readonly int budget, wave; readonly PanicPhase phase; readonly bool reached;
            readonly string items, stock;
            public GameplaySnapshot(PanicHomeWorld owner)
            { budget = owner.Model.Budget; wave = owner.Model.Wave; phase = owner.Model.Phase; reached = owner.Model.ReachedShelterV5;
                items = string.Join("|", owner.Model.Items.Select(i => i.Id + ":" + i.Tier + ":" + i.Anchored + ":" + i.Installed + ":" + i.Damage));
                stock = string.Join("|", PanicExpedition.MaterialIds.Select(id => id + ":" + owner.Expedition.Count(id))); }
            public bool Matches(PanicHomeWorld owner)
            { return budget == owner.Model.Budget && wave == owner.Model.Wave && phase == owner.Model.Phase && reached == owner.Model.ReachedShelterV5 &&
                items == string.Join("|", owner.Model.Items.Select(i => i.Id + ":" + i.Tier + ":" + i.Anchored + ":" + i.Installed + ":" + i.Damage)) &&
                stock == string.Join("|", PanicExpedition.MaterialIds.Select(id => id + ":" + owner.Expedition.Count(id))); }
        }

        IEnumerator Guard(IEnumerator body)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(body);
            while (stack.Count > 0 && !finished)
            {
                bool more; object current = null;
                try { more = stack.Peek().MoveNext(); if (more) current = stack.Peek().Current; }
                catch (Exception error) { report.error = error.ToString(); break; }
                if (!more) { stack.Pop(); continue; } if (current is IEnumerator nested) stack.Push(nested); else yield return current;
            }
            Finish();
        }
        void Update() { if (!finished && Time.realtimeSinceStartup - began > 180f) { report.error = "Bounded180s driver timeout"; Finish(); } }
        void Finish()
        {
            if (finished) return; finished = true; sampleCamera = false; hud.SetPaused(false); w.ReturnToMenu();
            report.realScoreAfter = Hash(realScore); Check("profile.real-score-unchanged", report.realScoreBefore == report.realScoreAfter);
            report.seconds = Time.realtimeSinceStartup - began;
            report.passed = report.complete && string.IsNullOrEmpty(report.error) && report.errors.Count == 0 && report.captures.Count >= 10 && report.checks.All(c => c.pass);
            Application.logMessageReceived -= Log; File.WriteAllText(Path.Combine(w.EvidencePath, "presentation-v512-smoke.json"), JsonUtility.ToJson(report, true));
            Debug.Log("PRESENTATION_V512_RESULT " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " captures=" + report.captures.Count); Application.Quit(report.passed ? 0 : 2);
        }
    }
}
