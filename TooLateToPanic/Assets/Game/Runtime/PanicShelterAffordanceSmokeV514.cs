using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    // Opt-in current-DLL physical-ray/production-key/framebuffer QA. Actor poses,
    // Flood mask, 39s clock and finite supply setup are declared fixtures.
    // Never claims OS input, natural discovery/traversal or human readability.
    [DefaultExecutionOrder(10000)]
    public sealed class PanicShelterAffordanceSmokeV514 : MonoBehaviour
    {
        const string ActionLabel = "Panik odasını kilitle";
        const string Flag = "--panic-shelter-affordance-v514-smoke";
        const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        [Serializable] public sealed class Row { public string id, detail; public bool pass; }
        [Serializable] public sealed class Shot { public string path, sha256, context; public int width, height, nonDark; }
        [Serializable] public sealed class AlarmSample
        {
            public string id; public float clock, intensity, actualLightIntensity;
            public bool on, actualLightEnabled; public Color emission, actualMeshEmission; public Vector3 position;
        }
        [Serializable] public sealed class Pose
        {
            public string id, rayTarget, focus, firstHit, promptKey, promptVerb, cueHint;
            public Vector3 actor, eye, forward, target, firstHitPoint;
            public bool grounded, controllerEnabled, canInteract, prompt, reached, cueShowsKey, cueActionStage;
            public int floor; public float interactionDistance; public Rect promptBounds;
        }
        [Serializable] public sealed class Report
        {
            public string utc, dllSha256, scorePath, realScoreBefore, realScoreAfter, error;
            public string layer = "CURRENT_DLL_NATIVE_PHYSICAL_RAY_PRODUCTION_E_AND_FRAMEBUFFER_WITH_DECLARED_FIXTURES";
            public bool complete, passed, OSInput, humanAcceptance;
            public float seconds;
            public int productionStarts, setupWarps, productionKeyCalls, finiteSupplyCalls, forcedFocusCalls;
            public List<Row> checks = new List<Row>();
            public List<Shot> captures = new List<Shot>();
            public List<Pose> poses = new List<Pose>();
            public List<AlarmSample> alarm = new List<AlarmSample>();
            public List<string> fixtures = new List<string>();
            public List<string> errors = new List<string>();
        }
        readonly Report report = new Report();
        PanicHomeWorld w; PanicHomeHud hud; HomePick marker; Collider markerHit;
        float began; bool finished; string realScore;
        string Focus { get { return Read<string>(w, "focusId"); } }
        Vector3 Target { get { return markerHit.bounds.center; } }
        Vector3 ButtonPosition { get { return marker.transform.position; } }
        Vector3 FrontDirection { get { return -marker.transform.forward; } }
        Vector3 FrontPose { get { return ButtonPosition + FrontDirection * 1.8f + Vector3.up * .04f; } }
        Vector3 ObliquePose { get { return ButtonPosition + FrontDirection * 1.7f + Vector3.forward * .45f + Vector3.up * .04f; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!Environment.GetCommandLineArgs().Contains(Flag)) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (world) world.gameObject.AddComponent<PanicShelterAffordanceSmokeV514>();
        }
        void Start()
        {
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); began = Time.realtimeSinceStartup;
            if (!w || !hud || string.IsNullOrEmpty(w.EvidencePath))
            { Debug.LogError("PANIC_SHELTER_V514 requires isolated evidence/World/HUD"); Application.Quit(2); return; }
            report.utc = DateTime.UtcNow.ToString("o"); report.dllSha256 = Hash(typeof(PanicModel).Assembly.Location);
            realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json"); report.realScoreBefore = Hash(realScore);
            string expected = Path.Combine(w.EvidencePath, "isolated-home-scores.json");
            if (!PanicTestLaunchRules.Drivers.Contains(Flag) || PanicModel.ScorePathOverride != expected || File.Exists(expected))
            { Debug.LogError("PANIC_SHELTER_V514 refuses unregistered or reused/nonisolated profile"); Application.Quit(2); return; }
            PanicModel.ScorePathOverride = Path.Combine(w.EvidencePath, "isolated-panic-shelter-v514-" + Guid.NewGuid().ToString("D") + ".json");
            report.scorePath = PanicModel.ScorePathOverride;
            Application.logMessageReceived += Log; Application.runInBackground = true; StartCoroutine(Guard(Run()));
        }
        static string Hash(string path)
        { if (!File.Exists(path)) return "absent"; using (var h = SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""); }
        static void Set(object o, string name, object value) { o.GetType().GetField(name, Members).SetValue(o, value); }
        static T Read<T>(object o, string name)
        { var t = o.GetType(); var p = t.GetProperty(name, Members); return (T)(p != null ? p.GetValue(o) : t.GetField(name, Members).GetValue(o)); }
        static object Call(object o, string name, params object[] args)
        { return o.GetType().GetMethods(Members).First(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(o, args); }
        void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Check(string id, bool pass, string detail = "")
        { report.checks.Add(new Row { id = id, pass = pass, detail = detail }); Debug.Log("PANIC_SHELTER_V514 " + (pass ? "PASS " : "FAIL ") + id + " " + detail); }
        IEnumerator Wait(float seconds) { float until = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < until) yield return null; }
        IEnumerator Resolution(int width, int height)
        { Screen.SetResolution(width, height, FullScreenMode.Windowed); yield return Wait(.35f); Check("resolution." + width, Screen.width == width && Screen.height == height); }
        void Key(KeyCode key) { report.productionKeyCalls++; w.HandleKey(key); }
        void E() { Key(KeyCode.E); }
        void FindMarker()
        {
            var markers = FindObjectsByType<HomePick>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(p => p.Id == "@panicroom").ToArray();
            Check("owner.single-physical-marker." + report.productionStarts, markers.Length == 1,
                "No second shelter owner is accepted; visual mesh/label counts are intentionally unconstrained.");
            if (markers.Length != 1) throw new InvalidOperationException("INVALID_SETUP expected one real panic-room owner");
            marker = markers[0]; markerHit = marker.GetComponent<Collider>();
            if (!markerHit || !markerHit.enabled) throw new InvalidOperationException("INVALID_SETUP physical shelter collider absent");
            Check("owner.real-active-wall-point." + report.productionStarts,
                Vector3.Distance(marker.transform.position, w.ActiveShelterPositionV5) < .001f &&
                Vector3.Distance(marker.transform.position, new Vector3(7.84f, 3.2f, -2.35f)) < .001f &&
                Vector3.Dot(FrontDirection, Vector3.left) > .999f,
                "Declared R2 wall placement; admission, guidance and real marker share actual active destination. Point=" + marker.transform.position + ";front=" + FrontDirection);
        }
        IEnumerator BeginFlood(bool bag = true, bool supplies = false)
        {
            hud.SetPaused(false); hud.ResetKeyboardHelpPresentationV56(); if (w.Notebook != null) w.Notebook.ForceClosed();
            w.InputLocked = false; w.ReturnToMenu(); w.StartGame(false); report.productionStarts++; w.FullFlowScriptedInput = true; yield return null;
            if (hud.FreeplayOnboardingOpenV414) hud.ConfirmFreeplayOnboardingV414(); yield return null;
            w.SkipIntroStoryV414(); yield return Wait(.15f);
            // Declared threat-selection fixture; no real-wave progression or RNG-roll claim.
            w.Model.Disaster = DisasterType.Flood;
            Set(w.Model, "compoundWaveV54", w.Model.Wave); Set(w.Model, "compoundPrimaryV54", DisasterType.Flood);
            Set(w.Model, "compoundMaskV54", 1 << (int)DisasterType.Flood);
            FindMarker();
            if (supplies) foreach (string id in new[] { "water", "food" })
            {
                var s = w.Expedition.WorldSupplies.FirstOrDefault(x => x.ItemId == id && x.Count > 0);
                bool collected = s != null && w.Expedition.TryCollectWorldSupply(s.Id); report.finiteSupplyCalls++;
                Check("setup.finite-supply." + id + "." + report.productionStarts, collected,
                    "Actual finite supply API; this setup does not claim a physical pickup route.");
            }
            if (bag)
            {
                w.Model.TimeRemaining = w.Model.PhaseTime = 39f; w.TickPreparationStages(); yield return Wait(2.3f);
                Check("setup.fps." + report.productionStarts, w.BagFirstPersonActive && w.PlayerControllerEnabled && !w.Model.ReachedShelterV5,
                    "Declared39s clock fixture; actual retained handoff and controller, no arrival write.");
            }
        }
        IEnumerator Place(string id, Vector3 pose, Vector3 look)
        {
            w.Warp(pose); report.setupWarps++; Physics.SyncTransforms();
            for (int n = 0; n < 10; n++) { w.SmokeMove(Vector2.zero, .025f); yield return null; }
            if (w.BagFirstPersonActive)
                for (int n = 0; n < 3; n++) { Aim(look); yield return null; Call(w, "RefreshBagRayFocus"); }
            else { w.View.transform.LookAt(look); yield return null; }
            Call(w, "ApplyFloorVisibility"); yield return Wait(.18f);
            Record(id, look);
        }
        void Aim(Vector3 target)
        {
            Vector3 direction = target - (w.PlayerPosition + Vector3.up * 1.62f); var a = Quaternion.LookRotation(direction).eulerAngles;
            float yaw = Read<float>(w, "bagYaw"), pitch = Read<float>(w, "bagPitch");
            w.StepBagLook(new Vector2(Mathf.DeltaAngle(yaw, a.y) / (2 * w.FpsLookMultiplierV414),
                (pitch - Mathf.DeltaAngle(0, a.x)) / (1.7f * w.FpsLookMultiplierV414)));
        }
        Pose Record(string id, Vector3 target)
        {
            string key, verb; Vector3 point; bool prompt = w.TryInteractionPromptV513(out key, out verb, out point);
            var c = Read<CharacterController>(w, "controller");
            var hits = Physics.RaycastAll(w.View.ViewportPointToRay(new Vector3(.5f, .5f)), 10f, ~((1 << 28) | (1 << 30)), QueryTriggerInteraction.Collide)
                .OrderBy(h => h.distance).Where(h => h.collider != c && !h.transform.IsChildOf(c.transform)).ToArray();
            var p = new Pose { id = id, actor = w.PlayerPosition, eye = w.View.transform.position, forward = w.View.transform.forward, target = target,
                rayTarget = (string)Call(w, "BagRayTarget"), focus = Focus, firstHit = hits.Length == 0 ? "none" : hits[0].collider.name,
                firstHitPoint = hits.Length == 0 ? Vector3.zero : hits[0].point,
                floor = w.ActiveFloor, grounded = c.isGrounded, controllerEnabled = w.PlayerControllerEnabled, canInteract = w.CanInteract,
                prompt = prompt, promptKey = key, promptVerb = verb, promptBounds = hud.InteractionPromptBoundsV513,
                interactionDistance = w.InteractionDistance("@panicroom"), reached = w.Model.ReachedShelterV5,
                cueHint = w.PanicRoomAffordanceHintV514, cueShowsKey = w.PanicRoomAffordanceShowsKeyV514,
                cueActionStage = w.PanicRoomAffordanceActionStageV514 };
            report.poses.Add(p); return p;
        }
        void ExpectPoint(string id)
        {
            var p = Record(id, Target);
            Check("ray." + id, p.grounded && p.controllerEnabled && p.floor == 1 && p.rayTarget == "@panicroom" && p.focus == "@panicroom" && p.canInteract,
                JsonUtility.ToJson(p));
            Check("prompt." + id, p.prompt && p.promptKey == "E" && p.promptVerb == ActionLabel &&
                hud.InteractionPromptTextV513.StartsWith("E · ", StringComparison.Ordinal) && p.promptBounds.width > 0 && p.promptBounds.width <= 250.01f,
                "Real center-ray focus/CanInteract and actual compact Repaint bounds=" + p.promptBounds + ";text=" + hud.InteractionPromptTextV513);
            Check("label.production-context-and-button." + id, w.ShortAction == ActionLabel && w.PanicRoomAffordanceActionLabelV514 == ActionLabel &&
                w.PanicRoomAffordanceHintV514 == ActionLabel && marker.GetComponentsInChildren<TextMesh>(true).Any(t => t.text.Replace("\n", " ") == ActionLabel),
                "Exact canonical action matches actual context, actual E verb, button API and its physical line-wrapped lettering. Label=" + ActionLabel);
            if (id == "front-1600")
            {
                var labels = marker.GetComponentsInChildren<TextMesh>(true).Where(t => t.text == "E" || t.text.Replace("\n", " ") == ActionLabel).ToArray();
                Check("label.actual-alpha-font-shader", labels.Length > 0 && labels.All(t =>
                {
                    var renderer = t.GetComponent<Renderer>(); var material = renderer ? renderer.sharedMaterial : null;
                    return material && material.shader && material.shader.name == "Panic/WorldTextDepthV514" && material.mainTexture;
                }), "Actual physical action/E TextMesh materials use the dedicated atlas-alpha/depth-tested shader and a bound atlas; root must inspect rendered glyphs separately.");
            }
            Check("cue.actual-state." + id, p.cueShowsKey && p.cueActionStage && w.PanicRoomButtonFocusedV514 && !string.IsNullOrWhiteSpace(p.cueHint),
                "Read-only cue state reports current flood/bag availability. Physical button form and conspicuity require review of actual front/oblique frames; mesh/label counts are unconstrained. Hint=" + p.cueHint);
        }
        string GameplayState()
        {
            return w.Model.Phase + "|" + w.Model.Wave + "|" + w.Model.ActiveDisasterMaskV54 + "|" +
                w.Model.TimeRemaining.ToString("R", CultureInfo.InvariantCulture) + "|" + w.Model.PhaseTime.ToString("R", CultureInfo.InvariantCulture) + "|" +
                w.Model.Budget + "|" + w.Model.PackedWater + "|" + w.Model.PackedFood + "|" + w.Model.EnteredShelterV5 + "|" +
                w.Model.CheatedRun + "|" + w.Model.CompletedWaves + "|" + w.Model.SecondaryRollCountV5 + "|" + w.Model.UpgradeDowngradeOutcomesV513.Count + "|" +
                w.Expedition.LootLayoutRevisionV5 + "|" + w.Expedition.GatherSequence + "|" + Focus + "|" + w.PlayerPosition + "|" +
                string.Join(";", w.Expedition.Inventory.Select(s => s.Id + ":" + s.Count)) + "|" +
                string.Join(";", PanicExpedition.MaterialIds.Select(id => id + ":" + w.Expedition.Count(id))) + "|" +
                string.Join(";", w.Expedition.Sources.Select(s => s.Id + ":" + s.Open + ":" + s.AutoLooted + ":" + (s.Contents == null ? "null" : string.Join(",", s.Contents.Select(x => x.Id + ":" + x.Count))))) + "|" +
                string.Join(";", w.Expedition.WorldSupplies.Select(s => s.Id + ":" + s.Count)) + "|" +
                string.Join(";", w.Model.Items.Select(i => i.Id + ":" + i.Tier + ":" + i.Anchored + ":" + i.Position)) + "|" +
                Read<uint>(w.Model, "secondarySeedV5") + "|" + Read<uint>(w.Model, "compoundSeedV54") + "|" + JsonUtility.ToJson(UnityEngine.Random.state) + "|" +
                w.PanicRoomAlarmClockV514.ToString("R", CultureInfo.InvariantCulture);
        }
        void PureQueries(string id)
        {
            string before = GameplayState();
            for (int i = 0; i < 40; i++)
            {
                string k, v; Vector3 p; w.TryInteractionPromptV513(out k, out v, out p);
                var h = w.ActiveShelterEntryHintV56; var c = w.CanInteract;
                var cueHint = w.PanicRoomAffordanceHintV514; var cueKey = w.PanicRoomAffordanceShowsKeyV514;
                var cueAction = w.PanicRoomAffordanceActionStageV514;
                var focused = w.PanicRoomButtonFocusedV514;
                var label = w.PanicRoomAffordanceActionLabelV514; var alarmClock = w.PanicRoomAlarmClockV514;
                var alarmOn = w.PanicRoomAlarmOnV514; var intensity = w.PanicRoomAlarmIntensityV514; var emission = w.PanicRoomAlarmEmissionV514;
            }
            Check("query.no-model-stock-focus-or-rng-mutation." + id, before == GameplayState(),
                "Synchronous production prompt/hint/range reads preserve exact clock, threats, funds, inventory, source stock, tiers, arrival, focus, actor and RNG. Physics.SyncTransforms is allowed.");
        }
        IEnumerator Capture(string name, string context)
        {
            yield return new WaitForEndOfFrame(); CaptureFrame(name, context);
        }
        void CaptureFrame(string name, string context)
        {
            var t = ScreenCapture.CaptureScreenshotAsTexture();
            if (!t) { Check("capture." + name, false, "Missing framebuffer"); return; }
            string path = Path.Combine(w.EvidencePath, name + ".png"); File.WriteAllBytes(path, t.EncodeToPNG());
            var pixels = t.GetPixels32(); int nonDark = 0; byte lo = 255, hi = 0;
            for (int i = 0; i < pixels.Length; i += 137) { var p = pixels[i]; byte l = (byte)((p.r + p.g + p.b) / 3); if (l > 12) nonDark++; lo = Math.Min(lo, l); hi = Math.Max(hi, l); }
            Check("capture." + name, nonDark > 80 && hi - lo > 18, "Actual framebuffer=" + t.width + "x" + t.height + ";nonDark=" + nonDark + ";human readability separate");
            report.captures.Add(new Shot { path = path, sha256 = Hash(path), context = context, width = t.width, height = t.height, nonDark = nonDark }); Destroy(t);
        }
        IEnumerator AlarmPhases()
        {
            yield return Place("alarm-front", FrontPose, Target); ExpectPoint("alarm-front");
            Check("alarm.normal-motion-profile", !w.ReducedFpsMotionV54,
                "Two-phase alarm test uses the ordinary isolated process default; reduced-motion steady mode is a separate preference, never silently disabled here.");
            foreach (bool desired in new[] { true, false })
            {
                float until = Time.realtimeSinceStartup + 3f; bool found = false;
                while (Time.realtimeSinceStartup < until)
                {
                    yield return new WaitForEndOfFrame();
                    if (w.PanicRoomAlarmOnV514 != desired) continue;
                    var light = w.PanicRoomAlarmLightV514;
                    if (!light) break;
                    var sample = new AlarmSample { id = desired ? "bright" : "dim", clock = w.PanicRoomAlarmClockV514,
                        intensity = w.PanicRoomAlarmIntensityV514, actualLightIntensity = light.intensity, actualLightEnabled = light.enabled,
                        on = w.PanicRoomAlarmOnV514, emission = w.PanicRoomAlarmEmissionV514, position = light.transform.position };
                    var renderers = Read<MeshRenderer[]>(w, "panicRoomAlarmRenderersV514");
                    var block = new MaterialPropertyBlock(); bool emissionMatches = renderers != null && renderers.Length > 0;
                    if (renderers != null) foreach (var renderer in renderers)
                    {
                        if (!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy) { emissionMatches = false; continue; }
                        renderer.GetPropertyBlock(block); Color actual = block.GetColor("_EmissionColor");
                        sample.actualMeshEmission = actual;
                        emissionMatches &= Mathf.Abs(actual.r - sample.emission.r) + Mathf.Abs(actual.g - sample.emission.g) +
                            Mathf.Abs(actual.b - sample.emission.b) + Mathf.Abs(actual.a - sample.emission.a) < .001f;
                        block.Clear();
                    }
                    report.alarm.Add(sample); found = true;
                    Check("alarm.actual-light." + sample.id, Mathf.Abs(sample.intensity - sample.actualLightIntensity) < .0001f &&
                        sample.on == (sample.actualLightEnabled && sample.actualLightIntensity > .1f) && sample.intensity >= 0 && sample.intensity <= .6001f &&
                        sample.position.y > Target.y && light.color.r > light.color.g && light.color.r > light.color.b,
                        "Actual red Light component and production intensity/emission observations at same rendered frame: " + JsonUtility.ToJson(sample));
                    Check("alarm.actual-renderer-emission." + sample.id, emissionMatches,
                        "Actual visible alarm mesh MaterialPropertyBlocks match the phase emission; no material write or exact renderer count requirement. " + JsonUtility.ToJson(sample));
                    CaptureFrame("panic-alarm-" + sample.id,
                        "Actual EndOfFrame red alarm " + sample.id + " phase, clock/intensity/emission recorded; no clock or material/light fixture. Human flicker comfort/listening NOT_RUN.");
                    break;
                }
                Check("alarm.phase-observed." + desired, found, "Real graphical frame waited at most3s; no synthetic time/phase/light write.");
            }
            Check("alarm.real-distinct-light-and-emission-phases", report.alarm.Count == 2 && report.alarm[0].on && !report.alarm[1].on &&
                report.alarm[0].actualLightIntensity > report.alarm[1].actualLightIntensity + .2f &&
                report.alarm[0].emission.maxColorComponent > report.alarm[1].emission.maxColorComponent + .01f &&
                report.alarm[1].clock > report.alarm[0].clock,
                "Two different actual animated framebuffer states with real Light and actual emission observations; source/static preview is not substituted.");
        }
        IEnumerator Views(int width)
        {
            Vector3 m = marker.transform.position;
            yield return Place("doorway-" + width, new Vector3(3.3f, 3.24f, -2.6f), Target);
            Check("doorway.no-auto-entry." + width, !w.Model.ReachedShelterV5 && w.Model.Phase == PanicPhase.Preparing);
            yield return Capture("panic-doorway-" + width, "Explicit grounded room doorway pose looking toward real shelter point. Passive discovery/readability image only; normal2.4m interaction range is unchanged.");
            yield return Place("front-" + width, FrontPose, Target); ExpectPoint("front-" + width); PureQueries("front-" + width);
            yield return Wait(.25f); Check("near.no-auto-entry." + width, !w.Model.ReachedShelterV5 && w.Model.Phase == PanicPhase.Preparing);
            yield return Capture("panic-front-near-" + width, "Explicit real grounded front pose; actual center ray and rendered E bubble. No forced focus; no natural-human discovery claim.");
            yield return Place("oblique-" + width, ObliquePose, Target); ExpectPoint("oblique-" + width); PureQueries("oblique-" + width);
            yield return Capture("panic-oblique-near-" + width, "Explicit oblique pose inside the room, looking toward the wall-mounted button. Actual center ray and compact E bubble; exterior wall blocks the rear face.");
            yield return Place("look-away-" + width, FrontPose, FrontPose + FrontDirection * 3f + Vector3.up * 1.35f);
            var away = Record("look-away-test-" + width, Target); string before = GameplayState(); E();
            Check("gate.look-away." + width, away.rayTarget != "@panicroom" && !away.prompt && before == GameplayState() && !w.Model.ReachedShelterV5,
                "No E offer or entry while actual center ray looks away; no sticky focus substitute.");
            yield return Capture("panic-look-away-" + width, "Actual look-away fixture must hide actionable shelter prompt; no through-wall/automatic interaction.");
            yield return Place("wrong-floor-" + width, new Vector3(FrontPose.x, .04f, FrontPose.z), Target);
            var wrongFloor = Record("wrong-floor-test-" + width, Target); E();
            Check("gate.wrong-floor." + width, wrongFloor.floor != 1 && wrongFloor.rayTarget != "@panicroom" && !w.Model.ReachedShelterV5 && w.Model.Phase == PanicPhase.Preparing,
                "Real original controller on ground floor and real center ray; no forced floor or focus.");
            yield return Capture("panic-wrong-floor-" + width, "Wrong-floor real actor/camera fixture; upper room does not grant remote shelter entry.");
        }
        IEnumerator BlockingAndAdmission()
        {
            Vector3 m = marker.transform.position;
            yield return Place("behind-wall", ButtonPosition - FrontDirection * .81f + Vector3.up * .04f, Target);
            var behind = Record("behind-wall-test", Target); E();
            Check("gate.behind-wall-occluded", behind.rayTarget != "@panicroom" && behind.firstHit != "none" &&
                behind.firstHit != marker.gameObject.name && Vector3.Dot(behind.firstHitPoint - Target, -FrontDirection) > .055f &&
                !w.Model.ReachedShelterV5 && w.Model.Phase == PanicPhase.Preparing,
                "Unsupported exterior-upper actor pose may be ungrounded; this is an explicit occlusion probe, not grounded traversal. Actual first hit=" + behind.firstHit + ";grounded=" + behind.grounded);
            yield return Capture("panic-behind-wall", "Actual outside-wall ray/visibility fixture. Wall must block the wall-mounted button; unsupported upper exterior pose is not a reachable or grounded route claim.");
            yield return Place("out-of-range", ButtonPosition + FrontDirection * 3.2f + Vector3.up * .04f, Target); var far = Record("range-test", Target); E();
            Check("gate.out-of-range", far.interactionDistance >= 2.4f && far.rayTarget != "@panicroom" && !w.Model.ReachedShelterV5);
            yield return Place("closed-door", new Vector3(1.93f, 3.24f, -2.6f), new Vector3(2.65f, 4.35f, -2.6f));
            var hinges = (IDictionary)Read<object>(w, "hinges"); var door = hinges["door_study"];
            Check("door.closed-fixture", !Read<bool>(door, "open"), "Actual retained door starts closed; no hinge/collider field write.");
            var closed = Record("closed-door-test", Target);
            Check("gate.closed-door-is-not-shelter", closed.rayTarget == "door_study" && !w.Model.ReachedShelterV5,
                "Actual center ray identifies the closed transit door, never the room behind it.");
            yield return Capture("panic-closed-door", "Real closed door from upper hall; actual E target is transit door. Marker range and first-hit obstruction remain production-owned.");
            E(); yield return Wait(.2f);
            Check("gate.door-e-opens-only-door", Read<bool>(door, "open") && !w.Model.ReachedShelterV5 && w.Model.Phase == PanicPhase.Preparing);
            yield return Place("pause-modal-base", FrontPose, Target); ExpectPoint("pause-modal-base");
            hud.SetPaused(true); string before = GameplayState(); E(); string key, verb; Vector3 point;
            Check("gate.pause", !w.TryInteractionPromptV513(out key, out verb, out point) && before == GameplayState() && !w.Model.ReachedShelterV5);
            float alarmClock = w.PanicRoomAlarmClockV514; yield return Wait(.12f);
            Check("alarm.real-pause-freezes-clock", alarmClock == w.PanicRoomAlarmClockV514);
            hud.SetPaused(false); yield return null;
            bool opened = hud.OpenKeyboardHelpV56(); before = GameplayState(); E();
            Check("gate.real-f1-modal", opened && w.Model.KeyboardHelpOpenV56 && !w.TryInteractionPromptV513(out key, out verb, out point) && before == GameplayState() && !w.Model.ReachedShelterV5);
            alarmClock = w.PanicRoomAlarmClockV514; yield return Wait(.12f);
            Check("alarm.real-f1-modal-freezes-clock", alarmClock == w.PanicRoomAlarmClockV514);
            hud.ResetKeyboardHelpPresentationV56(); yield return null;
            Key(KeyCode.Tab); yield return null; before = GameplayState(); E();
            Check("gate.real-inventory", w.InventoryOpen && !w.TryInteractionPromptV513(out key, out verb, out point) && before == GameplayState() && !w.Model.ReachedShelterV5);
            Key(KeyCode.Tab); yield return null;
            w.Model.UtilitySafety.Complete("poweroff"); Call(w, "ApplyMainsPowerV5"); yield return Wait(.2f);
            Check("power.actual-off", !w.MainsPoweredV5 && w.Model.UtilitySafety.PowerOff); ExpectPoint("poweroff-front"); PureQueries("poweroff-front");
            yield return Capture("panic-front-power-off", "Actual mains-off state via declared completed-utility fixture; physical emergency button must remain readable. No new utility work or listening claim.");
            int water = w.Model.PackedWater, food = w.Model.PackedFood, cash = w.Model.Budget; E();
            Check("admission.front.real-e", w.Model.ReachedShelterV5 && w.Model.EnteredShelterV5 == PanicShelterKindV5.PanicRoom && w.Model.Phase == PanicPhase.Disaster &&
                water == w.Model.PackedWater && food == w.Model.PackedFood && cash == w.Model.Budget,
                "Actual HandleKey(E) refreshes physical ray; correct grounded room starts Disaster without early supply consumption or payment.");
            E(); Check("admission.no-repeat-charge", water == w.Model.PackedWater && food == w.Model.PackedFood && cash == w.Model.Budget);
        }
        IEnumerator Run()
        {
            report.fixtures.Add("Fresh graphical isolated profile. FullFlowScriptedInput suppresses physical input sampling. Production HandleKey(E), StepBagLook and fresh BagRayTarget own positive entry; forcedFocusCalls remains0. Root alone launches this process. No old-player/OS/human/natural-route acceptance.");
            report.fixtures.Add("Explicit normal StartGame+intro skip; Flood/compound-mask selection and39s clock fixtures. R2 wall marker(7.84,3.2,-2.35),yaw90 and actual ActiveShelterPosition agree. Real finite supplyAPI readies inventory; actual controller settles after inside-room pose fixtures. Outside-upper-wall pose is explicitly unsupported/unreachable, only a first-hit occlusion probe. Door hinges/colliders/range/floor/ray/EnteredShelter are never fabricated. Mains-off uses the existing utility completion state as a declared visual fixture.");
            report.fixtures.Add("R2 exact production/button label Panik odasını kilitle. Two real EndOfFrame alarm phases observe actual Light and material-emission API without changing time/material/light state. Pause and F1 modal must freeze presentation clock. Native render/API evidence remains separate from human flash comfort/readability acceptance.");
            report.fixtures.Add("Current policy permits entry with missing supplies and then enforces actual supply-loss outcome. This driver preserves and separately verifies that policy; it never invents a new inventory entry gate.");
            yield return BeginFlood(true, true);
            yield return Resolution(1600, 900); yield return Views(1600);
            yield return Resolution(960, 540); yield return Views(960);
            yield return Resolution(1600, 900); yield return AlarmPhases(); yield return BlockingAndAdmission();
            yield return BeginFlood(true, true);
            yield return Place("oblique-positive-e", ObliquePose, Target); ExpectPoint("oblique-positive-e");
            E(); Check("admission.oblique.real-e", w.Model.ReachedShelterV5 && w.Model.EnteredShelterV5 == PanicShelterKindV5.PanicRoom && w.Model.Phase == PanicPhase.Disaster);
            yield return BeginFlood(false, true);
            yield return Place("precaution-upper", FrontPose, Target);
            E(); Check("gate.before-bag-phase", !w.BagFinalStageActive && !w.Model.ReachedShelterV5 && w.Model.Phase == PanicPhase.Preparing,
                "Actual E during normal precautions cannot end preparation through shelter.");
            Check("cue.before-bag-no-action-stage", w.PanicRoomAffordanceShowsKeyV514 && !w.PanicRoomAffordanceActionStageV514 && !string.IsNullOrWhiteSpace(w.PanicRoomAffordanceHintV514),
                "Physical cap retains E while inactive; no actionable HUD offer or admission is inferred from that printed key. " + w.PanicRoomAffordanceHintV514);
            yield return BeginFlood(true, false);
            yield return Place("missing-supplies", FrontPose, Target); ExpectPoint("missing-supplies");
            Check("supplies.actual-missing-hint", !w.Model.BagReady && w.Model.PackedWater == 0 && w.Model.PackedFood == 0 &&
                w.ActiveShelterEntryHintV56.Contains("1 su") && w.ActiveShelterEntryHintV56.Contains("1 gıda") &&
                w.PanicRoomAffordanceShowsKeyV514 && w.PanicRoomAffordanceActionStageV514 && !string.IsNullOrWhiteSpace(w.PanicRoomAffordanceHintV514), w.ActiveShelterEntryHintV56);
            E(); Check("supplies.current-entry-policy-preserved", w.Model.ReachedShelterV5 && w.Model.Phase == PanicPhase.Disaster && !w.Model.BagReady,
                "Actual entry remains permitted; missing supplies are an outcome-loss gate.");
            w.Model.Tick(8.001f);
            Check("supplies.actual-resolution-loss", w.Model.Phase == PanicPhase.Aftermath && w.Model.FailureReason.Contains("1 su ve 1 gıda eksik"), w.Model.FailureReason);
            yield return BeginFlood(true, true);
            w.Model.Disaster = DisasterType.Earthquake; Set(w.Model, "compoundPrimaryV54", DisasterType.Earthquake); Set(w.Model, "compoundMaskV54", 1 << (int)DisasterType.Earthquake);
            yield return Place("wrong-destination", FrontPose, Target);
            E(); string k, v; Vector3 p;
            Check("gate.wrong-disaster-destination", w.Model.RequiredShelterV5 == PanicShelterKindV5.Basement && !w.TryInteractionPromptV513(out k, out v, out p) &&
                !w.Model.ReachedShelterV5 && w.Model.Phase == PanicPhase.Preparing, "Same real upper marker cannot admit an earthquake requiring the basement.");
            Check("cue.wrong-disaster-no-action-stage", w.PanicRoomAffordanceShowsKeyV514 && !w.PanicRoomAffordanceActionStageV514 && !string.IsNullOrWhiteSpace(w.PanicRoomAffordanceHintV514),
                "Physical cap retains E while wrong-destination action is inactive. " + w.PanicRoomAffordanceHintV514);
            Check("focus.no-forced-focus", report.forcedFocusCalls == 0, "Source contains no SetFocus call; every positive uses actual FPS ray refresh.");
            Check("runtime.no-errors", report.errors.Count == 0, string.Join("\n", report.errors)); report.complete = true;
        }
        IEnumerator Guard(IEnumerator body)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(body);
            while (stack.Count > 0 && !finished)
            {
                bool more; object current = null;
                try { more = stack.Peek().MoveNext(); if (more) current = stack.Peek().Current; }
                catch (Exception e) { report.error = e.ToString(); break; }
                if (!more) { stack.Pop(); continue; } if (current is IEnumerator nested) stack.Push(nested); else yield return current;
            }
            Finish();
        }
        void Update() { if (!finished && Time.realtimeSinceStartup - began > 180f) { report.error = "Bounded180s driver timeout"; Finish(); } }
        void Finish()
        {
            if (finished) return; finished = true;
            try { hud.SetPaused(false); w.ReturnToMenu(); } catch (Exception e) { report.errors.Add("Cleanup:" + e); }
            report.realScoreAfter = Hash(realScore); Check("profile.real-score-unchanged", report.realScoreBefore == report.realScoreAfter);
            report.seconds = Time.realtimeSinceStartup - began;
            report.passed = report.complete && string.IsNullOrEmpty(report.error) && report.errors.Count == 0 && report.captures.Count >= 15 && report.checks.All(c => c.pass);
            Application.logMessageReceived -= Log;
            File.WriteAllText(Path.Combine(w.EvidencePath, "panic-shelter-affordance-v514-smoke.json"), JsonUtility.ToJson(report, true));
            Debug.Log("PANIC_SHELTER_V514_RESULT " + (report.passed ? "PASS" : "FAIL") + " checks=" + report.checks.Count + " captures=" + report.captures.Count);
            Application.Quit(report.passed ? 0 : 2);
        }
    }
}



