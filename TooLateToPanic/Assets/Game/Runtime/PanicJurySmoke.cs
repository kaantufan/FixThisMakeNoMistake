using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in root-owned native API smoke. Never attach to a normal user's session.</summary>
    public sealed class PanicJurySmoke : MonoBehaviour
    {
        [Serializable] public sealed class Entry { public string name; public bool pass; public string detail; }
        [Serializable] public sealed class Report
        {
            public string evidenceType = "Native semantic/API smoke with explicit actor-position and delta-time fixtures";
            public string limitations = "No OS keyboard/mouse, no human route/timing/fun acceptance, no broad campaign/art acceptance. Warp is setup only at named task/shelter positions; reachability between those points is NOT accepted. Direct screws use production ClickWorkTarget plus Tick; world installs use production timed work. No Complete=true writes. Fixed-delta work steps are fixtures, not real0.60s acceptance. Injected 8/80/300 second Tick values test timeline semantics, not elapsed wall-clock play.";
            public string scorePath, scoreBefore, scoreAfter, startedUtc, finishedUtc;
            public bool completed, success;
            public List<string> errors = new List<string>();
            public int passed, total;
            public float wallSeconds;
            public List<Entry> checks = new List<Entry>();
            public List<string> fixtures = new List<string>();
            public string error;
        }
        PanicHomeWorld world;
        Report report;
        float began;
        string reportPath;

        void Check(string name, bool pass, string detail = "")
        {
            report.checks.Add(new Entry { name = name, pass = pass, detail = detail });
            report.total++; if (pass) report.passed++;
            Debug.Log("JURY_CHECK " + (pass ? "PASS " : "FAIL ") + name + " " + detail);
        }

        IEnumerator Start()
        {
            world = GetComponent<PanicHomeWorld>();
            if (!world) { Debug.LogError("Jury smoke needs PanicHomeWorld."); yield break; }
            string evidence = world.EvidencePath;
            string score = PanicModel.ScorePathOverride;
            if (string.IsNullOrEmpty(evidence) || string.IsNullOrEmpty(score) ||
                !Path.GetFullPath(score).StartsWith(Path.GetFullPath(evidence).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            { Debug.LogError("Jury smoke requires an isolated --home-evidence directory and score override within it."); yield break; }
            Directory.CreateDirectory(evidence);
            reportPath = Path.Combine(evidence, "jury-smoke.json");
            report = new Report { scorePath = score, scoreBefore = Hash(score) };
            began = Time.realtimeSinceStartup; report.startedUtc = DateTime.UtcNow.ToString("o");
            Application.logMessageReceived += OnLog;
            while (!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            yield return new WaitForSeconds(.5f);
            var bodies = new Stack<IEnumerator>(); bodies.Push(Run());
            while (bodies.Count > 0)
            {
                object current = null; bool more = false;
                try { var body = bodies.Peek(); more = body.MoveNext(); if (more) current = body.Current; }
                catch (Exception ex) { report.error = ex.ToString(); Check("unhandled exception", false, ex.Message); bodies.Clear(); break; }
                if (!more) { bodies.Pop(); continue; }
                if (current is IEnumerator nested) { bodies.Push(nested); continue; }
                yield return current;
            }
            report.scoreAfter = Hash(score); report.wallSeconds = Time.realtimeSinceStartup - began;
            Check("isolated score bytes unchanged after demo/reset/campaign", report.scoreBefore == report.scoreAfter, report.scoreBefore + " -> " + report.scoreAfter);
            report.completed = true; report.finishedUtc = DateTime.UtcNow.ToString("o");
            report.success = report.passed == report.total && string.IsNullOrEmpty(report.error) && report.errors.Count == 0;
            Application.logMessageReceived -= OnLog;
            File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
            Debug.Log("JURY_SMOKE_DONE " + report.passed + "/" + report.total + " " + reportPath);
            Application.Quit(report.success ? 0 : 1);
        }

        IEnumerator Run()
        {
            yield return Capture("01-title");
            world.StartJuryDemo(); yield return null;
            // The current subtitle contract reveals text on the normal32char/s
            // clock. The event intentionally starts empty; wait for a real tick.
            float subtitleDeadline=Time.realtimeSinceStartup+.5f;
            while(string.IsNullOrEmpty(world.PresentationSubtitle)&&Time.realtimeSinceStartup<subtitleDeadline)yield return null;
            CheckPresentationAssets();
            yield return Capture("02-jury-onboarding");
            CaptureHudSupplyPreviews();
            var model = world.Model; var expedition = world.Expedition;
            Check("isolated demo starts", model.IsJuryDemo && world.JuryDemo != null && world.JuryDemo.IsRunning);
            Check("finite stock and unchanged capacities", expedition.Inventory.Count == 12 && expedition.Bag.Count == 8 &&
                expedition.Count("bracket") == 1 && expedition.Count("screws") == 2 && expedition.Count("sandbag") == 2 &&
                expedition.Count("plank") == 1 && expedition.Count("extinguisher") == 1 && model.Budget == 1000);
            Check("food baseline, water not granted", model.PackedFood == 1 && model.PackedWater == 0 && expedition.JuryWaterGatheredCount == 0 && world.JurySnapshot.StartingFoodIsBaseline);
            report.fixtures.Add("First movement uses production SmokeMove/CharacterController with semantic direction and injected .08 simulation dt per frame; not OS WASD.");
            for (int i = 0; i < 18 && world.JuryDemo.Stage == PanicJuryStage.Onboarding; i++) { world.SmokeMove(Vector2.left, .08f); yield return null; }
            Check("actual controller movement advances onboarding", world.JuryDemo.Stage == PanicJuryStage.Preparation);
            if (world.JuryDemo.Stage != PanicJuryStage.Preparation) yield break;

            yield return Place(new Vector3(6.5f, .05f, 3.25f), "fridge: task proximity fixture, route not tested");
            world.SmokeFocus("fridge"); world.HandleKey(KeyCode.E); yield return Capture("03-curated-loot"); world.HandleKey(KeyCode.F);
            Check("pickup presentation begins without blocking commit", world.PresentationPickupCount == 1 && expedition.JuryWaterGatheredCount == 1);
            yield return null;
            Check("real container E/F pickup", expedition.JuryWaterGatheredCount == 1 && expedition.Count("water") == 1 && model.PackedWater == 0);
            int water = expedition.Inventory.FindIndex(slot => slot.Id == "water" && slot.Count > 0);
            Check("water present in inventory", water >= 0);
            if (water < 0) yield break;
            world.InventorySelect(water); world.InventoryUse(); yield return null;
            Check("real pack consumes inventory once", model.PackedWater == 1 && expedition.Count("water") == 0);
            int budget = model.Budget;
            world.QuickAction(1); yield return null;
            yield return Capture("04-short-manual-work");
            Check("demo work has two real stages", world.WorkActive && expedition.Work.TotalSteps == 2 && expedition.Work.JuryFriendly);
            world.ClickWorkTarget(0); world.StepWork(.05f, 0, false, false, false); world.CancelWork(); yield return null;
            Check("cancel before completion costs nothing", model.Budget == budget && expedition.Count("bracket") == 1 && expedition.Count("screws") == 2 && !Find("fridge").Anchored);
            world.SmokeFocus("fridge"); world.QuickAction(1);
            yield return DriveWork(PanicWorkMode.Ratchet);
            Check("two screw clicks commit exact recipe once", Find("fridge").Anchored && model.Budget == budget - 60 && expedition.Count("bracket") == 0 && expedition.Count("screws") == 0);
            yield return Capture("05-secured-hardware");
            int committed = model.Budget; world.CompleteWork();
            Check("duplicate completion cannot charge", model.Budget == committed);
            Check("wrong-floor quake shelter rejected", !world.JuryDemo.TryReachShelter(PanicJuryShelter.QuakeCover, PanicJuryDemo.QuakeCoverPosition + Vector3.up * 3.2f, true));

            yield return Place(PanicJuryDemo.QuakeCoverPosition, "quake cover: shelter position fixture; approach route not tested");
            world.HandleKey(KeyCode.E); yield return null;
            Check("physical cover E enters quake, no bunker warp", world.JuryDemo.Stage == PanicJuryStage.Earthquake && Vector3.Distance(world.PlayerPosition, PanicJuryDemo.QuakeCoverPosition) < .8f);
            yield return null; yield return null;
            Check("controlled hero visual reactions exist", world.PresentationHeroCount > 0);
            yield return Capture("06-quake");
            Check("quake snapshot from real calculation and supplies consumed", world.JuryDemo.Outcomes.Count == 1 && world.JuryDemo.Outcomes[0].PreparationComplete &&
                world.JuryDemo.Outcomes[0].ShelterReached && world.JuryDemo.Outcomes[0].WaterConsumed == 1 && world.JuryDemo.Outcomes[0].FoodConsumed == 1 && model.PackedWater == 0 && model.PackedFood == 0);
            var quake = world.JuryDemo.Outcomes[0];
            float beforeDamage = quake.TotalDamage;
            report.fixtures.Add("Injected controller Tick(8.01) ends quake semantics; no real eight-second timing/visual acceptance.");
            world.JuryDemo.Tick(8.01f, world.PlayerPosition, true, true); yield return null;
            Check("quake result stable, damage calculated once", world.JuryDemo.Stage == PanicJuryStage.EarthquakeResult && quake.TotalDamage == beforeDamage);
            world.AdvanceJuryDemo(); yield return null;
            Check("flood is an explicit separate vignette", world.JuryDemo.Stage == PanicJuryStage.Flood && model.Disaster == DisasterType.Flood && model.Wave == 1);
            yield return Capture("07-flood");

            yield return Place(new Vector3(5f, .05f, -8.5f), "east drain: task proximity fixture; outside route not tested");
            world.SmokeFocus("drain_east"); world.HandleKey(KeyCode.E);
            yield return DriveWork(PanicWorkMode.Timing);
            Check("direct drain installation commits exact recipe once", model.DrainCount == 1 && model.Budget == 890 && expedition.Count("sandbag") == 0 && expedition.Count("plank") == 0);
            Check("basement cannot be flood refuge", !world.JuryDemo.TryReachShelter(PanicJuryShelter.FloodHighGround, new Vector3(0, -3.2f, 5.15f), true));
            yield return Place(PanicJuryDemo.FloodRefugePosition, "upper landing: refuge position fixture; stair traversal not tested");
            world.HandleKey(KeyCode.E); yield return null;
            Check("upper floor refuge E advances", world.JuryDemo.Stage == PanicJuryStage.Fire && world.JuryDemo.Outcomes.Count == 2 && world.JuryDemo.Outcomes[1].ShelterReached);

            yield return Place(new Vector3(.9f, .05f, -2.2f), "ground fire station: task proximity fixture, no egress route acceptance");
            world.SmokeFocus(PanicItemActionRules.FireStationId); world.HandleKey(KeyCode.E);
            yield return DriveWork(PanicWorkMode.Balance);
            Check("dedicated station commits actual extinguisher recipe once", model.FireToolCount == 1 && expedition.Count("extinguisher") == 0 && model.Budget == 815);
            Check("airborne exit rejected", !world.JuryDemo.TryReachShelter(PanicJuryShelter.FireAssembly, PanicJuryDemo.FireAssemblyPosition, false));
            yield return Place(PanicJuryDemo.FireAssemblyPosition, "outdoor assembly: goal position fixture; egress route not tested");
            world.HandleKey(KeyCode.E); yield return null;
            Check("safe outdoor E advances without chain", world.JuryDemo.Stage == PanicJuryStage.Inspection && world.JuryDemo.Outcomes.Count == 3 && world.JuryDemo.Outcomes[2].ShelterReached);
            world.AdvanceJuryDemo(); world.AdvanceJuryDemo(); yield return null;
            Check("stable bounded results and no normal award", world.JuryDemo.Stage == PanicJuryStage.Complete && model.Score == 0 && model.CompletedWaves == 0 && model.LastReward == 0);
            yield return Capture("08-results");
            bool all = true; foreach (var goal in world.JurySnapshot.Goals) all &= goal.State == PanicJuryGoalState.Complete;
            Check("only actual completed goals marked complete", all);

            world.ReturnToMenu(); world.StartGame(false); yield return null;
            float timer = model.TimeRemaining; model.Tick(1f);
            Check("campaign reset clears demo and prepared stock", !model.IsJuryDemo && world.JuryDemo == null && model.PackedFood == 0 && model.PackedWater == 0 && expedition.Count("bracket") == 0 && model.TimeRemaining < timer);
            expedition.Work.Begin("anchorwall", "fridge");
            Check("normal anchor uses two direct screws", expedition.Work.TotalSteps == 2 && expedition.Work.UsesDirectClick && !expedition.Work.JuryFriendly); world.CancelWork(); world.ReturnToMenu();

            var clockModel = new PanicModel(); var clockExpedition = new PanicExpedition(clockModel); var clock = new PanicJuryDemo(clockModel, clockExpedition);
            clock.Start(); clockModel.SetDeveloperOption(PanicDeveloperOption.FreezePreparationTime, true);
            report.fixtures.Add("Independent isolated Model/Expedition: blocked-input Tick(300) is a timeline/deadline fixture, not human duration acceptance.");
            clock.Tick(300, PanicJuryDemo.SpawnPosition, false, true);
            bool truthful = true; foreach (var goal in clock.Snapshot.Goals) truthful &= goal.State != PanicJuryGoalState.Complete;
            Check("blocked/developer frozen run still finishes inside 270/300", clock.Stage == PanicJuryStage.Complete && clock.ElapsedSeconds <= 270.01f && clock.ElapsedSeconds <= 300f);
            Check("deadline does not fabricate preparation/shelter", truthful && clock.Outcomes.Count == 3 && !clock.Outcomes[0].PreparationComplete && !clock.Outcomes[0].ShelterReached && clockModel.Budget == 1000);
            clock.Stop();
            Check("separate controller stop is clean", !clockModel.IsJuryDemo && clockModel.Phase == PanicPhase.Title && clockExpedition.Count("bracket") == 0);
            foreach (var check in PanicNoLossModelChecks.Run(world.EvidencePath)) Check(check.name, check.pass, check.detail);
            foreach (var check in PanicCameraOrbitChecks.Run()) Check(check.name, check.pass);
            foreach (var check in PanicStormModelChecks.Run(world.EvidencePath)) Check(check.name, check.pass, check.detail);
            yield return CheckBunkerCinematics();
            yield return CheckExteriorAndSimulation();
        }

        void PrepareCinematicRun(bool withSupplies)
        {
            world.ReturnToMenu(); world.StartGame(false);
            // Fixture prevents test writes to scores while leaving no-loss OFF and real rules intact.
            world.Model.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, true);
            world.Model.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, false);
            world.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss, false);
            if (withSupplies) { world.Model.PackedWater = world.Model.RequiredDays; world.Model.PackedFood = world.Model.RequiredDays; }
            report.fixtures.Add("Cinema regression: normal run; supply seed=" + withSupplies + "; budget toggle on/off only marks fixture unranked. No-loss OFF, real model loss/damage transitions.");
        }

        IEnumerator CheckBunkerCinematics()
        {
            PrepareCinematicRun(true); yield return null;
            yield return Place(PanicHomeWorld.BunkerPosition + new Vector3(0,.05f,-1.1f), "bunker entry fixture; approach route not tested");
            world.SmokeFocus("bunker"); world.HandleKey(KeyCode.E); yield return null; yield return null;
            Check("bunker E begins cinema without HUD clock deadlock", world.Model.Phase == PanicPhase.Disaster && world.CinematicActive && !world.InputLocked);
            yield return Capture("09-bunker-skip-visible");
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < 25f && (world.CinematicActive || world.LossAnnounced || world.Model.Phase == PanicPhase.Disaster || world.Model.Phase == PanicPhase.Aftermath || world.Model.Phase == PanicPhase.Success)) yield return null;
            yield return null; yield return null;
            Check("natural bunker cinema finishes on real wall clock", !world.CinematicActive && !world.LossAnnounced && !world.InputLocked && (world.Model.Phase == PanicPhase.Inspection || world.Model.Phase == PanicPhase.GameOver), "wall=" + (Time.realtimeSinceStartup - start).ToString("F2"));
            Check("natural cinema restores controller and orthographic camera", world.PlayerControllerEnabled && world.View.orthographic);
            yield return Capture("10-bunker-natural-finish");

            PrepareCinematicRun(true); yield return null;
            world.Model.EnterBunker(); yield return null; yield return null;
            Check("skip starts from a real active cinema", world.CinematicActive);
            world.SkipResidenceCinematic(); yield return null; yield return null;
            Check("skip reaches truthful terminal phase and releases control", !world.CinematicActive && !world.LossAnnounced && !world.InputLocked && world.PlayerControllerEnabled && world.View.orthographic && world.PresentationHeroCount==0 && world.PresentationPickupCount==0 && (world.Model.Phase == PanicPhase.Inspection || world.Model.Phase == PanicPhase.GameOver));
            long score=world.Model.Score; int waves=world.Model.CompletedWaves, budget=world.Model.Budget;
            world.SkipResidenceCinematic();
            Check("repeated skip cannot duplicate damage or award", world.Model.Score == score && world.Model.CompletedWaves == waves && world.Model.Budget == budget);
            yield return Capture("11-bunker-skipped");

            PrepareCinematicRun(false); yield return null;
            world.Model.EnterBunker(); yield return null; yield return null;
            world.SkipResidenceCinematic(); yield return null; yield return null;
            Check("skip with missing bag preserves actual loss", world.Model.Phase == PanicPhase.GameOver && !string.IsNullOrEmpty(world.Model.FailureReason));

            world.ReturnToMenu(); world.StartGame(false); yield return null; yield return null;
            var hud = world.GetComponent<PanicHomeHud>();
            hud.ToggleDeveloperMenu(); yield return null; yield return null;
            world.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss, true);
            Check("no-loss switch available through existing developer mutation path", hud.DeveloperMenuOpen && world.Model.DeveloperNoLoss && world.Model.CheatedRun);
            yield return Capture("12-developer-no-loss");
            hud.ToggleDeveloperMenu(); yield return null; yield return null;
            world.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss, false);
            world.ReturnToMenu();
            Screen.SetResolution(1280,720,false); yield return new WaitForSeconds(.25f);
            world.StartJuryDemo(); yield return Capture("13-short-play-1280x720");
            world.ReturnToMenu(); Screen.SetResolution(1600,900,false);
            CaptureBakedArtSheet();
        }

        IEnumerator CheckExteriorAndSimulation()
        {
            world.ReturnToMenu(); yield return null;
            var hud=world.GetComponent<PanicHomeHud>();hud.OpenSimulationSelector();
            Check("disaster selector is available from title",hud.SimulationSelectorOpen&&world.Model.Phase==PanicPhase.Title);
            yield return Capture("14-disaster-selector");
            Check("four retained animated neighborhood residents",world.ExteriorNpcCount==4&&world.ActiveExteriorNpcCount==4);
            Vector3 walker=world.ExteriorWalkerPosition;yield return new WaitForSeconds(.55f);
            Check("NPC controllers clips and actual sidewalk movement are active",world.ExteriorNpcAnimationReady&&Vector3.Distance(walker,world.ExteriorWalkerPosition)>.15f);
            world.StartDisasterSimulation(DisasterType.Storm);yield return null;yield return null;
            Check("storm simulation has finite labelled baseline",world.Model.IsSimulation&&world.Model.Disaster==DisasterType.Storm&&world.Model.PackedWater==1&&world.Model.PackedFood==1&&world.Expedition.Count("bracket")==1&&world.Expedition.Count("screws")==2&&!hud.SimulationSelectorOpen);
            world.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss,true);
            Vector3 entry=world.PlayerPosition;world.TriggerSimulation();yield return null;yield return null;
            Check("simulation starts real storm without bunker actor warp",world.Model.Phase==PanicPhase.Disaster&&world.CinematicActive&&!world.CinematicShowsActor&&Vector3.Distance(world.PlayerPosition,entry)<.01f);
            Check("residents leave the visible exterior during a hazard",world.ActiveExteriorNpcCount==0);
            var storm=FindFirstObjectByType<PanicStormArtState>();int stormCapacity=0;
            if(storm)foreach(var emitter in storm.Emitters)stormCapacity+=emitter.main.maxParticles;
            Check("storm loads its own bounded rain wind and debris systems",storm&&storm.Emitters.Count==8&&stormCapacity<=620);
            bool stormAlpha=storm; if(storm)foreach(var material in storm.Materials)stormAlpha &= material&&material.shader&&material.shader.name=="Panic/SoftWeather";
            Check("storm uses retained fixed alpha weather shader",stormAlpha);
            var stormClip=PanicArt.GetSound("storm");
            Check("storm sound is a distinct sustained wind clip",stormClip&&stormClip.length>=5.9f&&stormClip.name=="Panic_storm");
            yield return new WaitForSeconds(3.1f);yield return Capture("15-storm-exterior-cinema");
            int stormParticles=0;if(storm)foreach(var emitter in storm.Emitters)stormParticles+=emitter.particleCount;
            Check("storm actually emits bounded particles",stormParticles>0&&stormParticles<=620,"particles="+stormParticles);
            float started=Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup-started<15&&!world.Model.SimulationComplete)yield return null;
            yield return null;yield return null;
            Check("natural storm reaches stable unranked inspection",world.Model.SimulationComplete&&world.Model.Phase==PanicPhase.Inspection&&world.Model.Wave==1&&world.Model.CompletedWaves==0&&world.Model.Score==0&&world.Model.LastReward==0,"actual wall wait="+(Time.realtimeSinceStartup-started).ToString("F2"));
            Check("storm uses actual supplies and exterior aftermath",world.Model.PackedWater==0&&world.Model.PackedFood==0&&world.ExteriorDamageTypes==1&&world.ActiveExteriorNpcCount==4);
            yield return Capture("16-storm-aftermath");
            CaptureNeighborAftermath("storm");
            yield return Place(PanicJuryDemo.FloodRefugePosition,"upper landing visibility fixture; stairs not traversed in this check");
            Check("upper floor hides lower floors while their solids remain",world.ActiveFloor==1&&world.FloorHasVisibleRenderers(1)&&!world.FloorHasVisibleRenderers(0)&&!world.FloorHasVisibleRenderers(-1)&&world.FloorPhysicalCollidersIntact(0)&&world.FloorPhysicalCollidersIntact(-1)&&world.FloorPhysicalCollidersIntact(1));
            Check("interior view hides the rescue helicopter",!world.HelicopterVisible);
            yield return Capture("17-upper-floor-cutaway");
            float orbitGateDeadline=Time.realtimeSinceStartup+5f;
            while((world.CinematicActive||world.LossAnnounced||world.InputLocked)&&Time.realtimeSinceStartup<orbitGateDeadline)yield return null;
            Quaternion original=world.View.transform.rotation;
            world.StepCameraOrbitInput(new Vector2(800,450),true,true,false,false,.016f);
            world.StepCameraOrbitInput(new Vector2(1120,550),false,true,false,false,.2f);
            var released=world.StepCameraOrbitInput(new Vector2(1120,550),false,false,true,false,.016f);
            float orbitDeadline=Time.realtimeSinceStartup+2f;
            while((Mathf.Abs(world.CameraOrbitYaw)<=40||Quaternion.Angle(original,world.View.transform.rotation)<=30)&&Time.realtimeSinceStartup<orbitDeadline)yield return null;
            Check("real orbit rotates orthographic view without starting placement",!released.Click&&!world.IsMoving&&world.View.orthographic&&Mathf.Abs(world.CameraOrbitYaw)>40&&Quaternion.Angle(original,world.View.transform.rotation)>30,"yaw="+world.CameraOrbitYaw+"; angle="+Quaternion.Angle(original,world.View.transform.rotation)+"; click="+released.Click+"; moving="+world.IsMoving+"; cinema="+world.CinematicActive+"; loss="+world.LossAnnounced+"; locked="+world.InputLocked+"; pixelRect="+world.View.pixelRect+"; natural-clock deadline, no injected camera Tick");
            yield return Capture("18-upper-floor-orbit");
            world.HandleKey(KeyCode.Tab);yield return null;
            float yaw=world.CameraOrbitYaw;
            world.StepCameraOrbitInput(new Vector2(800,450),true,true,false,false,.016f);
            var blocked=world.StepCameraOrbitInput(new Vector2(1200,500),false,false,true,false,.2f);
            Check("inventory blocks orbit and accidental placement",world.InventoryOpen&&!blocked.Click&&Mathf.Abs(world.CameraOrbitYaw-yaw)<1&&!world.IsMoving);
            world.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss,false);
            world.ReturnToMenu();yield return null;yield return null;
            Check("title reset clears simulation orbit aftermath and rescue",!world.Model.IsSimulation&&!world.Model.SimulationComplete&&world.CameraOrbitYaw==0&&world.ExteriorDamageTypes==0&&!world.HelicopterVisible);

            world.StartDisasterSimulation(DisasterType.Flood);yield return null;
            world.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss,true);world.TriggerSimulation();
            yield return new WaitForSeconds(3.1f);yield return Capture("19-flowing-river-flood");
            var stream=FindFirstObjectByType<PanicExteriorStreamState>();
            Check("baked river keeps real texture runtime mesh ownership and flood rise",stream&&stream.Water&&stream.Foam&&stream.RuntimeOwned&&stream.WaterRenderer.sharedMaterial.mainTexture&&stream.Surface>-.12f);
            if(stream){Vector2 offset=stream.WaterRenderer.sharedMaterial.mainTextureOffset;yield return new WaitForSeconds(.25f);Check("river surface actually advances its flow texture",(stream.WaterRenderer.sharedMaterial.mainTextureOffset-offset).sqrMagnitude>.0001f);}
            world.SkipResidenceCinematic();yield return null;yield return null;
            Check("flood selected simulation finishes without another wave",world.Model.IsSimulation&&world.Model.Disaster==DisasterType.Flood&&world.Model.SimulationComplete&&world.Model.Wave==1);
            yield return Capture("20-neighborhood-flood-aftermath");
            CaptureNeighborAftermath("flood");
            world.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss,false);
            world.ReturnToMenu();world.StartDisasterSimulation(DisasterType.Fire);yield return null;
            world.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss,true);world.TriggerSimulation();
            yield return new WaitForSeconds(3.1f);yield return Capture("21-fire-simulation");
            world.SkipResidenceCinematic();yield return null;yield return null;
            Check("fire selection retains separate real result",world.Model.SimulationComplete&&world.Model.Disaster==DisasterType.Fire&&world.Model.Score==0);
            CaptureNeighborAftermath("fire");
            world.FinishInspection();yield return null;
            Check("simulation inspection finish shortcut returns to title",world.Model.Phase==PanicPhase.Title&&!world.Model.IsSimulation&&world.ExteriorDamageTypes==0);
            world.Model.SetDeveloperOption(PanicDeveloperOption.NoLoss,false);world.ReturnToMenu();
            yield return CheckDrivewayRoute();
            CaptureBakedArtSheet(true);
        }

        IEnumerator CheckDrivewayRoute()
        {
            world.StartGame(false);yield return null;
            yield return Place(new Vector3(11.7f,.05f,-15.1f),"one driveway route setup; no Warp between street, gate, bunker detour and garage interior");
            report.fixtures.Add("Driveway uses actual CharacterController and real frame dt. InputLocked isolates unscripted input/model clock only; controller/collision/settings unchanged. Door E is sent with lock released.");
            world.InputLocked=true;
            foreach(var waypoint in new[]{new Vector3(11.7f,0,-11.9f),new Vector3(14,0,-9),new Vector3(14,0,-5.3f),new Vector3(11.7f,0,-2.4f),new Vector3(11.7f,0,-1.1f)})yield return WalkDriveway(waypoint);
            world.InputLocked=false;world.SmokeFocus("@garage");world.HandleKey(KeyCode.E);yield return null;world.InputLocked=true;
            yield return new WaitForSeconds(.8f);
            yield return WalkDriveway(new Vector3(11.7f,0,.7f));
            Check("street gate bunker detour and open garage are physically connected",world.PlayerControllerEnabled&&world.PlayerPosition.z>.4f&&world.PlayerPosition.y>-.15f&&world.PlayerPosition.y<.25f);
            yield return Capture("22-garage-street-route");
            world.InputLocked=false;world.ReturnToMenu();yield return null;
            var hud=world.GetComponent<PanicHomeHud>();
            foreach(var size in new[]{new Vector2Int(1024,768),new Vector2Int(960,540)})
            {
                Screen.SetResolution(size.x,size.y,false);yield return new WaitForSeconds(.25f);
                world.StartDisasterSimulation(DisasterType.Storm);yield return Capture("23-hud-polish-"+size.x+"x"+size.y);
                hud.ToggleDeveloperMenu();yield return Capture("24-developer-polish-"+size.x+"x"+size.y);
                hud.ToggleDeveloperMenu();world.ReturnToMenu();
            }
            Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.25f);
            Check("HUD 3D supply previews stay cached across all modes and resolutions",hud.SupplyPreviewCount==2&&hud.SupplyPreviewRenderCount==2);
        }

        void CaptureNeighborAftermath(string id)
        {
            // Read-only camera on the real persistent neighborhood and its actual aftermath.
            var go=new GameObject("QA neighbor aftermath camera");var camera=go.AddComponent<Camera>();
            camera.CopyFrom(world.View);camera.enabled=false;camera.orthographic=true;camera.orthographicSize=5.2f;camera.aspect=1.6f;
            camera.nearClipPlane=.1f;camera.farClipPlane=65;camera.cullingMask &= ~((1<<29)|(1<<30));
            camera.transform.position=new Vector3(35,8,-11);camera.transform.LookAt(new Vector3(26,2.4f,1));
            var target=new RenderTexture(1600,1000,24);target.Create();camera.targetTexture=target;
            var frame=new Texture2D(1600,1000,TextureFormat.RGB24,false);RenderTexture previous=RenderTexture.active;
            try{camera.Render();RenderTexture.active=target;frame.ReadPixels(new Rect(0,0,1600,1000),0,0);frame.Apply();File.WriteAllBytes(Path.Combine(world.EvidencePath,"neighbor-"+id+"-aftermath.png"),frame.EncodeToPNG());}
            finally{RenderTexture.active=previous;camera.targetTexture=null;target.Release();Destroy(target);Destroy(frame);Destroy(go);}
            report.fixtures.Add("Neighbor closeup "+id+": camera-only real scene render; no building, damage, model or collider mutation.");
        }

        void CaptureHudSupplyPreviews()
        {
            var hud=world.GetComponent<PanicHomeHud>();
            Check("HUD uses two cached actual 3D supply renders",hud.SupplyPreviewCount==2&&hud.SupplyPreviewRenderCount==2);
            var field=typeof(PanicHomeHud).GetField("_supplyPreviews",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var previews=field.GetValue(hud) as PanicHudSupplyPreview;
            if(previews==null){Check("HUD supply preview owner exists",false);return;}
            foreach(string id in new[]{"water","food"})
            {
                var texture=previews.Get(id) as RenderTexture;
                Check("HUD actual mesh preview texture "+id,texture&&texture.width==256&&texture.height==256);
                if(!texture)continue;
                RenderTexture previous=RenderTexture.active;
                var frame=new Texture2D(256,256,TextureFormat.RGB24,false);
                try{RenderTexture.active=texture;frame.ReadPixels(new Rect(0,0,256,256),0,0);frame.Apply();File.WriteAllBytes(Path.Combine(world.EvidencePath,"hud-"+id+"-3d.png"),frame.EncodeToPNG());}
                finally{RenderTexture.active=previous;Destroy(frame);}
            }
        }

        IEnumerator WalkDriveway(Vector3 target)
        {
            float began=Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup-began<7)
            {
                Vector3 delta=target-world.PlayerPosition;delta.y=0;
                if(delta.magnitude<.12f)break;
                Vector3 forward=world.View.transform.forward;forward.y=0;forward.Normalize();
                Vector3 right=world.View.transform.right;right.y=0;right.Normalize();
                Vector3 direction=delta.normalized;
                world.SmokeMove(new Vector2(Vector3.Dot(direction,right),Vector3.Dot(direction,forward)),Mathf.Clamp(Time.unscaledDeltaTime,.008f,.04f));yield return null;
            }
            Vector3 remaining=target-world.PlayerPosition;remaining.y=0;
            Check("physical driveway waypoint "+target,remaining.magnitude<.18f,"actual="+world.PlayerPosition+" wall="+(Time.realtimeSinceStartup-began).ToString("F2"));
        }

        void CaptureBakedArtSheet(bool pickups=false)
        {
            // Inspect the exported baked prefabs, not fresh procedural stand-ins.
            string[] ids = { "bagstation", "bookshelf", "tvstand", "kitchencabinet", "toolbox", "medicinecabinet", "waterheater", "storagerack", "sofa_indigo", "balconychair", "sink", "toolwall" };
            if(pickups)ids=new[]{"water","food","food","food","firstaid","flashlight","battery","radio","blanket","extinguisher"};
            const int tile = 512;
            var atlas = new Texture2D(tile * 4, tile * 3, TextureFormat.RGB24, false);
            var cameraObject = new GameObject("QA isolated art camera");
            var camera = cameraObject.AddComponent<Camera>(); camera.enabled = false;
            camera.orthographic = true; camera.aspect = 1; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.18f,.20f,.22f); camera.cullingMask = 1 << 30;
            camera.nearClipPlane = .01f; camera.farClipPlane = 40;
            var lightObject = new GameObject("QA isolated art key");
            var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional;
            light.intensity = 1.25f; light.color = new Color(1,.94f,.84f); light.cullingMask = 1 << 30;
            lightObject.transform.rotation = Quaternion.Euler(45,-35,0);
            var target = new RenderTexture(tile,tile,24); target.Create(); camera.targetTexture = target;
            RenderTexture priorTarget = RenderTexture.active; bool priorFog = RenderSettings.fog;
            GameObject visual = null;
            try
            {
                RenderSettings.fog = false;
                for (int i=0;i<ids.Length;i++)
                {
                    string name=ids[i]+(pickups&&ids[i]=="food"?"-"+(i-1):"");
                    var prefab = pickups?null:Resources.Load<GameObject>("ResidenceArt/" + ids[i]);
                    if (!pickups&&!prefab) { Check("exported detailed art " + name,false,"baked prefab missing"); continue; }
                    visual = pickups?PanicResidenceArt.CreatePickup(ids[i],ids[i]=="food"?i-1:0):Instantiate(prefab); visual.transform.position = new Vector3(1000,0,1000);
                    foreach(var child in visual.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 30;
                    var renderers = visual.GetComponentsInChildren<Renderer>();
                    bool materials = renderers.Length > 0; Bounds bounds = new Bounds(visual.transform.position,Vector3.zero);
                    if(renderers.Length > 0) bounds = renderers[0].bounds;
                    foreach(var renderer in renderers)
                    {
                        bounds.Encapsulate(renderer.bounds);
                        foreach(var material in renderer.sharedMaterials)
                            materials &= material && material.shader && material.shader.isSupported && material.shader.name != "Hidden/InternalErrorShader";
                    }
                    Check((pickups?"runtime detailed pickup ":"exported detailed art ") + name,materials && bounds.size.sqrMagnitude > .0001f,"renderers="+renderers.Length+" bounds="+bounds.size);
                    camera.orthographicSize = Mathf.Max(.3f,bounds.extents.magnitude*1.12f);
                    camera.transform.position = bounds.center + new Vector3(4,3,-5).normalized*12;
                    camera.transform.LookAt(bounds.center);
                    camera.Render(); RenderTexture.active = target;
                    var frame = new Texture2D(tile,tile,TextureFormat.RGB24,false);
                    frame.ReadPixels(new Rect(0,0,tile,tile),0,0); frame.Apply();
                    File.WriteAllBytes(Path.Combine(world.EvidencePath,(pickups?"pickup-":"art-")+name+".png"),frame.EncodeToPNG());
                    atlas.SetPixels((i%4)*tile,(2-i/4)*tile,tile,tile,frame.GetPixels());
                    Destroy(frame); visual.SetActive(false); Destroy(visual); visual = null;
                }
                atlas.Apply(); File.WriteAllBytes(Path.Combine(world.EvidencePath,(pickups?"pickup-detail-atlas.png":"art-detail-atlas.png")),atlas.EncodeToPNG());
                report.fixtures.Add("Art atlas: 12 exported ResidenceArt prefabs, isolated studio camera and key light; not in-scene interaction or final lighting acceptance. Row-major IDs: "+string.Join(",",ids));
            }
            finally
            {
                if(visual) { visual.SetActive(false); Destroy(visual); }
                RenderSettings.fog = priorFog; RenderTexture.active = priorTarget; camera.targetTexture = null;
                target.Release(); Destroy(target); Destroy(atlas); Destroy(cameraObject); Destroy(lightObject);
            }
        }

        void CheckPresentationAssets()
        {
            string[] ids = {"pickup_soft","anchor_latch","objective_tick"};
            float speechSeconds = 0f;
            foreach (string id in ids)
            {
                var clip = Resources.Load<AudioClip>("Audio/Jury/" + id);
                bool valid = clip && clip.length > .02f && clip.length <= (id.StartsWith("jury_") ? 5f : 1f);
                Check("real imported audio " + id, valid, clip ? clip.length.ToString("F3") + "s " + clip.frequency + "Hz" : "missing");
                if (valid && id.StartsWith("jury_")) speechSeconds += clip.length;
            }
            foreach(string id in new[]{"jury_intro","jury_bag_ready","jury_secured","jury_quake_result","jury_flood","jury_fire","jury_end"})
                Check("spoken clip excluded " + id,!Resources.Load<AudioClip>("Audio/Jury/"+id));
            Check("imported short dialogue blip", PanicAudioBank.Get("dialogue_blip") && PanicAudioBank.Get("dialogue_blip").length < .12f);
            Check("native subtitle router enabled", world.PresentationAudioEnabled && !string.IsNullOrEmpty(world.PresentationSubtitle));
            foreach (string id in new[]{"water","food","firstaid","flashlight","battery","radio","blanket","extinguisher"})
            {
                var visual = PanicResidenceArt.CreatePickup(id, 0); visual.transform.position = new Vector3(100,100,100);
                bool imported = false, materials = true;
                foreach (var child in visual.GetComponentsInChildren<Transform>()) imported |= child.name.StartsWith("Visual_");
                foreach (var renderer in visual.GetComponentsInChildren<Renderer>()) foreach (var mat in renderer.sharedMaterials)
                    materials &= mat && mat.shader && mat.shader.isSupported && mat.shader.name != "Hidden/InternalErrorShader";
                Check("curated presentation binary/material/physics " + id, imported && materials && visual.GetComponentsInChildren<Collider>().Length == 0);
                Destroy(visual);
            }
        }

        IEnumerator Capture(string name)
        {
            yield return new WaitForSeconds(.3f);
            yield return new WaitForEndOfFrame();
            string path = Path.Combine(world.EvidencePath, name + ".png");
            ScreenCapture.CaptureScreenshot(path);
            yield return null;
        }

        IEnumerator Place(Vector3 position, string fixture)
        {
            report.fixtures.Add("Warp setup: " + fixture); world.Warp(position);
            for (int i = 0; i < 4; i++) yield return null;
        }
        PanicItem Find(string id) { return world.Model.Items.Find(item => item.Id == id); }
        IEnumerator DriveWork(PanicWorkMode expected)
        {
            Check("direct installation started " + expected, world.WorkActive && world.Expedition.Work.UsesDirectClick);
            report.fixtures.Add("Direct work uses .04 injected Tick per yielded frame; engine may also tick work. This validates semantic commit only, not real .60 second duration or OS hit areas.");
            int frames = 0;
            while (world.WorkActive && frames++ < 1400)
            {
                var work = world.Expedition.Work;
                if (!work.IsInstalling && !work.IsWorldConstruction) world.ClickWorkTarget(work.NextClickTarget);
                world.StepWork(.04f, 0, false, false, false);
                yield return null;
            }
            Check("direct production work terminates " + expected, !world.WorkActive, "semantic Tick frames=" + frames + "; .04 dt fixture; no Complete flag writes");
            if (world.WorkActive) world.CancelWork();
        }
        void OnLog(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        static string Hash(string path)
        {
            if (!File.Exists(path)) return "absent";
            using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }
    }
}
