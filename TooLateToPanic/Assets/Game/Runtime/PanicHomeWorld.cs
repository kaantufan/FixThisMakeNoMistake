using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed class HomePick : MonoBehaviour { public string Id; public string Kind; }

    public sealed partial class PanicHomeWorld : MonoBehaviour
    {
        public PanicModel Model { get; private set; }
        public PanicExpedition Expedition { get; private set; }
        public PanicItem Selected { get { return Model.Items.Find(i => i.Id == focusId); } }
        public LootSource NearestSource { get { return Expedition.Sources.Find(s => s.Id == focusId); } }
        public string ContextHint { get; private set; }
        public string FocusLabel { get; private set; }
        public string AnchorMode { get; private set; } = "wall";
        public string PlacementMessage { get; private set; }
        public int ActiveFloor { get; private set; }
        public bool InventoryOpen { get; private set; }
        public int InventoryIndex { get; private set; }
        public bool WorkActive { get { return Expedition != null && Expedition.Work.Active; } }
        public bool IsMoving { get { return movingId != null; } }
        public bool InputLocked;
        public int DiscreteInputFrame { get; private set; } = -1;
        // Opt-in route QA owns semantic input only; production clocks and physics stay live.
        public bool FullFlowScriptedInput { get; set; }
        public Camera View { get; private set; }
        public RenderTexture WorkTexture { get; private set; }
        public Vector3 PlayerPosition { get { return player ? player.position : Vector3.zero; } }
        public bool PlayerControllerEnabled { get { return controller && controller.enabled; } }
        public bool CanInteract { get { return player && FocusPosition(out Vector3 p) && InteractionDistance(focusId) < 2.4f && (BagFirstPersonActive ? BagRayTarget() == focusId : InteractionPathClear(focusId,p)); } }
        public static readonly Vector3 BunkerPosition = new Vector3(-4.5f, -3.2f, 3.2f);
        public string EvidencePath { get; private set; }

        sealed class Hinge { public Transform leaf, frame; public Quaternion closed; public Vector3 closedPosition; public bool open; public bool lid; public bool drawer; public bool directionalDoor; public bool closeBlocked; public bool motionBlocked; public Collider collision; public float openAngle=100; public string id; }
        sealed class GardenSite { public string id, label, action; public Vector3 p; public GameObject marker, built; public bool done; }
        readonly Dictionary<string, GameObject> items = new Dictionary<string, GameObject>();
        readonly Dictionary<string, GameObject> sourceObjects = new Dictionary<string, GameObject>();
        readonly Dictionary<string, string> visualKeys = new Dictionary<string, string>();
        readonly Dictionary<string, Hinge> hinges = new Dictionary<string, Hinge>();
        readonly Dictionary<string, GameObject> hardware = new Dictionary<string, GameObject>();
        readonly List<GardenSite> gardenSites = new List<GardenSite>();
        readonly List<GameObject> floorVisuals = new List<GameObject>();
        readonly List<GameObject> floorItems = new List<GameObject>();
        readonly Dictionary<int, Renderer[]> visibilityRenderers = new Dictionary<int, Renderer[]>();
        readonly List<string> focusCandidates = new List<string>();
        readonly Dictionary<string, List<Collider>> wallInteractionColliders = new Dictionary<string, List<Collider>>();
        GameObject landscape, roof, workRig, handTool;
        Transform player, hero;
        CharacterController controller;
        Animator animator;
        PanicHomeHud hud;
        PanicHomeEffects effects;
        Camera workCamera;
        string focusId, movingId, workTarget, workAction;
        Vector3 smoothVelocity, desiredVelocity, moveCandidate;
        float yawVelocity, gravitySpeed, footstepClock, facingYaw, cameraSize = 5.7f;
        float moveRotation, workAge, focusClock, gatherPoseTime;
        PanicPhase previousPhase;
        int previousWave;
        bool inside;
        LineRenderer moveOutline;
        GameObject ghost;
        Material outlineMaterial;
        readonly Vector3 cameraOffset = new Vector3(11, 13, -15);

        void Awake()
        {
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0; QualitySettings.antiAliasing = 4;
            var args = Environment.GetCommandLineArgs();
            if (!PanicTestLaunchRules.Validate(args, System.IO.File.Exists, out var launchReason))
            { Debug.LogWarning("QA_LAUNCH_REJECTED_BEFORE_MODEL: " + launchReason); gameObject.SetActive(false); Application.Quit(2); return; }
            // Process-local output gain only: background QA still exercises AudioSource state/PCM.
            // Normal launches and the opt-in OS observer retain their normal audio output.
            if (args.Any(arg => PanicTestLaunchRules.Drivers.Contains(arg))) AudioListener.volume = 0f;
            bool smoke = Array.IndexOf(args, "--home-smoke") >= 0;
            bool bathroomSmoke = Array.IndexOf(args, "--bathroom-smoke") >= 0;
            bool doorsStairsSmoke = Array.IndexOf(args, "--doors-stairs-smoke") >= 0;
            bool interactionSmoke = Array.IndexOf(args, "--interaction-smoke") >= 0;
            bool doorJamSmoke = Array.IndexOf(args, "--door-jam-smoke") >= 0;
            bool developerSmoke = Array.IndexOf(args, "--developer-smoke") >= 0;
            bool hudConceptSmoke = Array.IndexOf(args, "--hud-concept-smoke") >= 0;
            bool jurySmoke = Array.IndexOf(args, "--jury-smoke") >= 0;
            bool fullFlowSmoke = Array.IndexOf(args, "--fullflow-smoke") >= 0;
            if (Array.IndexOf(args, "--overhaul414-smoke") >= 0) Application.runInBackground = true;
            if (Array.IndexOf(args, "--v5-smoke") >= 0 || Array.IndexOf(args, "--v51-smoke") >= 0 || Array.IndexOf(args, "--v52-smoke") >= 0 || Array.IndexOf(args, "--v53-smoke") >= 0 || Array.IndexOf(args, "--v54-smoke") >= 0 || Array.IndexOf(args, "--v55-smoke") >= 0 || Array.IndexOf(args, "--v56-smoke") >= 0 || Array.IndexOf(args, "--wave-ledger-v56-smoke") >= 0 || Array.IndexOf(args, "--ui-v57-smoke") >= 0 || Array.IndexOf(args, "--yarkin-hybrid-smoke") >= 0) Application.runInBackground = true;
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--home-evidence") EvidencePath = args[i + 1];
            if (!string.IsNullOrEmpty(EvidencePath)) { System.IO.Directory.CreateDirectory(EvidencePath); PanicModel.ScorePathOverride = System.IO.Path.Combine(EvidencePath, "isolated-home-scores.json"); }
            if (fullFlowSmoke)
            {
                if (string.IsNullOrEmpty(EvidencePath)) throw new InvalidOperationException("--fullflow-smoke requires --home-evidence <fresh isolated GUID directory>.");
                if (Array.FindAll(args, a => a.EndsWith("-smoke", StringComparison.Ordinal)).Length != 1)
                    throw new InvalidOperationException("Run only one opt-in smoke per process.");
                Application.runInBackground = true;
            }
            if (jurySmoke)
            {
                if (string.IsNullOrEmpty(EvidencePath)) throw new InvalidOperationException("--jury-smoke requires --home-evidence <fresh isolated GUID directory>.");
                if (smoke || bathroomSmoke || doorsStairsSmoke || interactionSmoke || doorJamSmoke || developerSmoke || hudConceptSmoke || Array.IndexOf(args, "--residence-smoke") >= 0 || Array.IndexOf(args, "--refinement-smoke") >= 0)
                    throw new InvalidOperationException("Run only one opt-in smoke per process.");
                Application.runInBackground = true;
            }
            if (bathroomSmoke)
            {
                if (string.IsNullOrEmpty(EvidencePath)) throw new InvalidOperationException("--bathroom-smoke requires --home-evidence <new isolated directory>.");
                if (smoke || Array.IndexOf(args, "--residence-smoke") >= 0 || Array.IndexOf(args, "--refinement-smoke") >= 0)
                    throw new InvalidOperationException("Run only one opt-in smoke component per process.");
                Application.runInBackground = true;
            }
            if (doorsStairsSmoke)
            {
                if (string.IsNullOrEmpty(EvidencePath)) throw new InvalidOperationException("--doors-stairs-smoke requires --home-evidence <new isolated GUID directory>.");
                if (smoke || bathroomSmoke || Array.IndexOf(args, "--residence-smoke") >= 0 || Array.IndexOf(args, "--refinement-smoke") >= 0)
                    throw new InvalidOperationException("Run only one opt-in smoke component per process.");
                Application.runInBackground = true;
            }
            if (interactionSmoke)
            {
                if (string.IsNullOrEmpty(EvidencePath)) throw new InvalidOperationException("--interaction-smoke requires --home-evidence <new isolated GUID directory>.");
                if (smoke || bathroomSmoke || doorsStairsSmoke || Array.IndexOf(args, "--residence-smoke") >= 0 || Array.IndexOf(args, "--refinement-smoke") >= 0)
                    throw new InvalidOperationException("Run only one opt-in smoke component per process.");
                Application.runInBackground=true;
            }
            if (doorJamSmoke)
            {
                if (string.IsNullOrEmpty(EvidencePath)) throw new InvalidOperationException("--door-jam-smoke requires --home-evidence <new isolated GUID directory>.");
                if (smoke || bathroomSmoke || doorsStairsSmoke || interactionSmoke || Array.IndexOf(args,"--residence-smoke")>=0 || Array.IndexOf(args,"--refinement-smoke")>=0)
                    throw new InvalidOperationException("Run only one opt-in smoke component per process.");
                Application.runInBackground=true;
            }
            if(developerSmoke)
            {
                if(string.IsNullOrEmpty(EvidencePath))throw new InvalidOperationException("--developer-smoke requires --home-evidence <fresh GUID directory>.");
                if(smoke||bathroomSmoke||doorsStairsSmoke||interactionSmoke||doorJamSmoke||Array.IndexOf(args,"--residence-smoke")>=0||Array.IndexOf(args,"--refinement-smoke")>=0)
                    throw new InvalidOperationException("Run only one opt-in smoke per process.");
                Application.runInBackground=true;
            }
            if (hudConceptSmoke)
            {
                if (string.IsNullOrEmpty(EvidencePath)) throw new InvalidOperationException("--hud-concept-smoke requires --home-evidence <fresh GUID directory>.");
                if (smoke || bathroomSmoke || doorsStairsSmoke || interactionSmoke || doorJamSmoke || developerSmoke || Array.IndexOf(args, "--residence-smoke") >= 0 || Array.IndexOf(args, "--refinement-smoke") >= 0)
                    throw new InvalidOperationException("Run only one opt-in smoke per process.");
                Application.runInBackground = true;
            }
            if (smoke) Application.runInBackground = true;
            Model = new PanicModel(); ConfigureResidenceModel(); Expedition = new PanicExpedition(Model); ConfigureRevisionSources();
            BuildWorld();
            hud = gameObject.AddComponent<PanicHomeHud>(); hud.World = this;
            effects = gameObject.AddComponent<PanicHomeEffects>(); effects.World = this;
            InitResidenceLife();
            InitDirectActions();
            InitExteriorLife();
            InitNeighbourDetailsHybrid();
            InitRevisionWorld();
            InitExperienceFeedbackV414();
            InitOverhaulV414();
            InitRevisionV5();
            InitFloorFinishRepairV54();
            InitVisualReadabilityV56();
            InitGuidanceV54();
            previousPhase = Model.Phase; previousWave = Model.Wave;
            if (smoke) gameObject.AddComponent<PanicHomeSmoke>();
            if (Array.IndexOf(args, "--residence-smoke") >= 0) { Application.runInBackground=true; gameObject.AddComponent<PanicResidenceSmoke>(); }
            if (Array.IndexOf(args, "--refinement-smoke") >= 0) { Application.runInBackground=true; gameObject.AddComponent<PanicRefinementSmoke>(); }
            if (bathroomSmoke) gameObject.AddComponent<PanicBathroomSmoke>();
            if (doorsStairsSmoke) gameObject.AddComponent<PanicDoorsStairsSmoke>();
            if (interactionSmoke) gameObject.AddComponent<PanicInteractionSmoke>();
            if (doorJamSmoke) gameObject.AddComponent<PanicDoorJamSmoke>();
            if(developerSmoke)gameObject.AddComponent<PanicDeveloperSmoke>();
            if(hudConceptSmoke)gameObject.AddComponent<PanicHudConceptSmoke>();
            if(jurySmoke)gameObject.AddComponent<PanicJurySmoke>();
            if(fullFlowSmoke)gameObject.AddComponent<PanicFullFlowSmoke>();
            Debug.Log("HOME_V49_READY directClickWork=true reinforcedBuild=true regionalTrees=true retainedSurvivor=true offline=true");
        }

        public GameObject CreateVisual(string id)
        {
            if (id != null && id.StartsWith("v5_", StringComparison.Ordinal)) return PanicV5FurnitureArt.Create(id, true);
            if (ResidenceIds.Contains(id)) { var cached=Resources.Load<GameObject>("ResidenceArt/"+id); return NormalizeFixtureRoot(cached ? Instantiate(cached) : PanicResidenceArt.Create(id), id); }
            bool home = new[] { "window", "door", "fridge", "cabinet", "tree", "drain", "fence", "hose", "toolbox", "medicinecabinet", "kitchencabinet", "locker", "supplycrate", "stairs", "workbench", "generator" }.Contains(id);
            var prefab = Resources.Load<GameObject>((home ? "HomeArt/" : "Art/") + id);
            var go = prefab ? Instantiate(prefab) : home ? PanicHomeArt.Create(id) : PanicArt.Create(id);
            go.name = id;
            PanicArt.Register(go, id);
            PanicHomeArt.ApplyPalette(go);
            return go;
        }

        void BuildWorld()
        {
            var cachedLandscape = Resources.Load<GameObject>("HomeArt/landscape");
            landscape = cachedLandscape ? Instantiate(cachedLandscape) : PanicHomeArt.BuildLandscape(); landscape.name = "Visible neighborhood and garden";
            foreach (var node in landscape.GetComponentsInChildren<Transform>())
            {
                if (node.name == "HomeV2_fence") { var c=node.gameObject.AddComponent<BoxCollider>(); c.center=new Vector3(0,.48f,0); c.size=new Vector3(2,.96f,.18f); }
                if (node.name == "HomeV2_tree") { var c=node.gameObject.AddComponent<CapsuleCollider>(); c.center=new Vector3(0,.9f,0); c.radius=.19f; c.height=1.8f; }
            }
            AddBox("Garden west",new Vector3(-15.75f,-.2f,0),new Vector3(15.5f,.35f,47));
            AddBox("Garden east",new Vector3(15.75f,-.2f,0),new Vector3(15.5f,.35f,47));
            AddBox("Garden south",new Vector3(0,-.2f,-14.75f),new Vector3(16,.35f,17.5f));
            AddBox("Garden north",new Vector3(0,-.2f,14.75f),new Vector3(16,.35f,17.5f));
            for (int level = -1; level < 2; level++)
            {
                var cached = Resources.Load<GameObject>(level<0 ? "ResidenceArt/basement" : "HomeArt/floor" + level);
                var floor = cached ? Instantiate(cached) : level<0 ? PanicResidenceArt.BuildBasement() : PanicHomeArt.BuildFloor(level,true); floor.name = "House floor " + level; floor.transform.position = Vector3.up * (level * 3.2f);
                floorVisuals.Add(floor);
                foreach (var mf in floor.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (!mf.name.StartsWith("solid_", StringComparison.OrdinalIgnoreCase)) continue;
                    var b = mf.sharedMesh.bounds;
                    var col = AddTightCollision(mf,floor.transform);
                    string lower = mf.name.ToLowerInvariant();
                    if (lower.Contains("north") || lower.Contains("west") || lower.Contains("south") || lower.Contains("east"))
                    {
                        var pick = col.gameObject.AddComponent<HomePick>();
                        string side = lower.Contains("north") ? "north" : lower.Contains("west") ? "west" : lower.Contains("south") ? "south" : "east";
                        pick.Id = "wall_" + side + (level == 0 ? "" : level<0 ? "_basement" : "_upper"); pick.Kind = "item";
                        if (!wallInteractionColliders.TryGetValue(pick.Id, out var segments)) wallInteractionColliders[pick.Id] = segments = new List<Collider>();
                        segments.Add(col);
                    }
                }
            }
            // Physical staircase: 16 actual steps, not an instantaneous floor teleport.
            BuildStairFlight(-1); BuildStairFlight(0);
            var cachedRoof = Resources.Load<GameObject>("HomeArt/roof"); roof = cachedRoof ? Instantiate(cachedRoof) : PanicHomeArt.BuildRoof(); roof.name = "Exterior roof";
            BuildObjects();
            BuildResidenceRooms();
            BuildGarden();
            var cam = new GameObject("Following isometric camera"); View = cam.AddComponent<Camera>();
            View.orthographic = true; View.orthographicSize = 11; View.nearClipPlane = .08f; View.farClipPlane = 170;
            View.cullingMask &= ~(1 << 29);
            ResetCameraOrbit();
            View.clearFlags = CameraClearFlags.Skybox; View.backgroundColor = new Color(.09f, .13f, .17f);
            View.allowHDR = true; View.cullingMask = ~(1 << 30);
            View.transform.position = new Vector3(0, 1, -2) + cameraOffset; View.transform.LookAt(new Vector3(0, 1, -2));
            View.gameObject.AddComponent<AudioListener>();
            SetupLighting();
            CreatePlayer();
            CreateWorkCamera();
            ApplyFloorVisibility(); SyncVisuals(true);
        }

        void BuildObjects()
        {
            foreach (var item in Model.Items)
            {
                // The carried bag has no world mesh, collider, light or floor interaction.
                if (item.Id == "bagstation") continue;
                GameObject go;
                if (item.Kind == ItemKind.Wall)
                {
                    go = new GameObject(item.Id); go.transform.position = item.Position;
                }
                else if (item.Kind == ItemKind.Roof)
                {
                    go = new GameObject(item.Id); go.transform.position = item.Position;
                }
                else
                {
                    go = CreateVisual(item.Id=="desk_upper"?"desk":item.ArtId); go.name = item.Id; go.transform.position = item.Position; go.transform.rotation = Quaternion.Euler(0, item.Rotation, 0);
                    var tag = go.AddComponent<HomePick>(); tag.Id = item.Id; tag.Kind = "item";
                    if (item.Kind == ItemKind.Door) BindHinge(go, item.Id, true);
                    else { if(item.ArtId=="bed")AddBedCollisionV5(go);else AddBoundsCollider(go);BindHinge(go,item.Id,false); }
                }
                items[item.Id] = go; visualKeys[item.Id] = ""; floorItems.Add(go);
            }
            foreach (var source in Expedition.Sources)
            {
                GameObject go;
                if (!items.TryGetValue(source.Id, out go))
                {
                    go = CreateVisual(source.ArtId); go.name = source.Id; go.transform.position = source.Position;go.transform.rotation=Quaternion.Euler(0,source.Rotation,0);
                    if(source.Id=="bath_cabinet"){go.transform.position=source.Position+Vector3.up*1.08f;go.transform.rotation=Quaternion.Euler(0,270,0);}
                    if(!source.ArtId.StartsWith("v5_",StringComparison.Ordinal))AddBoundsCollider(go); floorItems.Add(go);
                }
                var pick = go.GetComponent<HomePick>(); if (!pick) pick = go.AddComponent<HomePick>(); pick.Id = source.Id; pick.Kind = "source";
                sourceObjects[source.Id] = go; BindHinge(go, source.Id, false);
            }
            if (!items.ContainsKey("bunker"))
            {
                var bunker = CreateVisual("bunker"); bunker.transform.position = BunkerPosition;
                var p = bunker.AddComponent<HomePick>(); p.Id = "@bunker"; p.Kind = "bunker"; AddBoundsCollider(bunker);
                items["bunker"] = bunker;
            }
        }

        void BuildGarden()
        {
            // Clear strips: full mature crowns stay outside the house, beds and perimeter.
            AddSite("plant_west1", "İç batı ağaç sırası", "plant", new Vector3(-10.8f, 0, 1));
            AddSite("plant_west2", "Dış batı ağaç sırası", "plant", new Vector3(-13.6f, 0, 1));
            AddSite("plant_east", "Kuzey ağaç sırası", "plant", new Vector3(0, 0, 12.3f));
            AddSite("drain_west", "Drenaj kanalı aç", "drain", new Vector3(-5, 0, -8.5f));
            AddSite("drain_east", "Drenaj kanalı aç", "drain", new Vector3(5, 0, -8.5f));
            AddSite("hose_station", "Yangın hortumu kur", "hose", new Vector3(10, 0, 3));
        }

        void AddSite(string id, string label, string action, Vector3 p)
        {
            Vector3 half = new Vector3(.7f, .02f, .5f);
            if (action == "plant") half = PanicConstructionArt.TreeRowBounds(id == "plant_west1" ? 0 : id == "plant_west2" ? 1 : 2).extents;
            var ring = MakeLine(id, new[] { new Vector3(-half.x, .02f, -half.z), new Vector3(half.x, .02f, -half.z), new Vector3(half.x, .02f, half.z), new Vector3(-half.x, .02f, half.z) }, new Color(.51f, .63f, .52f), .025f);
            ring.transform.position = p;
            gardenSites.Add(new GardenSite { id = id, label = label, action = action, p = p, marker = ring.gameObject });
        }

        void SetupLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.39f, .44f, .52f);
            RenderSettings.ambientEquatorColor = new Color(.26f, .29f, .34f);
            RenderSettings.ambientGroundColor = new Color(.12f, .135f, .16f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.17f, .23f, .28f); RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 48; RenderSettings.fogEndDistance = 115;
            var sun = new GameObject("Overcast dusk sun").AddComponent<Light>(); revisionSun = sun; sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(32, -38, 0); sun.color = new Color(.65f, .73f, .87f); sun.intensity = .75f; sun.shadows = LightShadows.Soft; sun.shadowBias = .025f;
            QualitySettings.shadows = ShadowQuality.All; QualitySettings.shadowDistance = 70; QualitySettings.shadowResolution = ShadowResolution.High; QualitySettings.pixelLightCount = 6;
            for (int level = 0; level < 2; level++)
            {
                foreach (var p in new[] { new Vector3(-4, 2.5f, -1), new Vector3(4.8f, 2.5f, 3.8f) })
                {
                    var light = new GameObject("Interior lamp " + level).AddComponent<Light>(); light.type = LightType.Point; light.color = new Color(1, .76f, .54f); light.intensity = 2.2f; light.range = 8; light.transform.position = p + Vector3.up * (level * 3.2f);
                }
            }
            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader) { var sky = new Material(skyShader); sky.SetColor("_SkyTint", new Color(.23f, .3f, .38f)); sky.SetColor("_GroundColor", new Color(.10f, .13f, .16f)); sky.SetFloat("_Exposure", .48f); sky.SetFloat("_AtmosphereThickness", 1.1f); RenderSettings.skybox = sky; }
        }

        void CreatePlayer()
        {
            var root = new GameObject("Survivor"); player = root.transform; player.position = new Vector3(0, .04f, -7.5f);
            controller = root.AddComponent<CharacterController>(); controller.height = 1.78f; controller.center = new Vector3(0, .9f, 0); controller.radius = .25f; controller.stepOffset = .25f; controller.skinWidth = .025f; controller.slopeLimit = 48;
            var prefab = Resources.Load<GameObject>("Player/SurvivorV2");
            if (!prefab) throw new InvalidOperationException("Clothed SurvivorV2 prefab missing.");
            var model = Instantiate(prefab, player); model.transform.localPosition = Vector3.zero; hero = model.transform; animator = model.GetComponentInChildren<Animator>(); animator.applyRootMotion = false;
            facingYaw = 0;
            gameObject.AddComponent<PanicActorOcclusion>().Init(this, player, hero);
        }

        void CreateWorkCamera()
        {
            WorkTexture = new RenderTexture(1100, 700, 24); WorkTexture.name = "Mechanic closeup";
            workCamera = new GameObject("Mechanic inspection camera").AddComponent<Camera>(); workCamera.orthographic = true; workCamera.orthographicSize = .82f; workCamera.clearFlags = CameraClearFlags.SolidColor; workCamera.backgroundColor = new Color(.065f, .075f, .085f); workCamera.cullingMask = 1 << 30; workCamera.targetTexture = WorkTexture;
            workCamera.transform.position = new Vector3(100, 101.6f, 98.3f); workCamera.transform.LookAt(new Vector3(100, 100.35f, 100)); workCamera.enabled = false;
            var light = new GameObject("Worklamp").AddComponent<Light>(); light.type = LightType.Point; light.range = 5; light.intensity = 2.3f; light.color = new Color(1, .82f, .59f); light.cullingMask = 1 << 30; light.transform.position = new Vector3(99, 102, 99);
        }

        void Update()
        {
            if(hud){hud.ProcessDeveloperHotkeys();hud.ProcessFrameHotkeys();}
            ProcessNotebookInput();
            UpdateBagCursor();
            if (Model.IsPaused || Model.WaveLedgerOpenV56 || Model.KeyboardHelpOpenV56 || NotebookBlocksInput)
            {
                SuspendExperienceFeedbackV414();
                desiredVelocity = smoothVelocity = Vector3.zero;
                if (animator) animator.speed = 0f;
                if (presentationAudio) presentationAudio.Tick(0f, true);
                if (effects) effects.SetSuspended(true);
                return;
            }
            bool developerBlocksInput=hud&&(hud.DeveloperMenuOpen||hud.DeveloperInputBlockedThisFrame);
            if (animator && animator.speed == 0f) animator.speed = 1f;
            float dt = Mathf.Min(Time.deltaTime, .08f);
            ProcessBagPreparationInput(dt);
            if (!IntroStoryActiveV414) TickJuryDemo(InputLocked || LossAnnounced || CinematicActive || developerBlocksInput || BagCameraTransitionActive);
            if (!InputLocked && !LossAnnounced && !developerBlocksInput && !IntroStoryActiveV414 && !BagHandoffPausesPreparationClockV51) Model.Tick(dt);
            if (Model.Wave != previousWave) { Expedition.OnNewWave(); previousWave = Model.Wave; }
            if (Model.Phase != previousPhase)
            {
                if (previousPhase == PanicPhase.Disaster && Model.Phase == PanicPhase.Aftermath)
                    Expedition.SyncConsumedBagSupplies();
                CancelMove(); CancelWork(); InventoryOpen = false;
                if (!BagStagePhase) ResetBagPreparation();
                if (effects) effects.OnPhase(Model.Phase);
                if (JuryActive) { ResetResidenceCinematic(); OnPresentationPhase(Model.Phase, previousPhase); }
                else OnResidencePhase(Model.Phase,previousPhase);
                previousPhase = Model.Phase;
                Debug.Log("HOME_PHASE " + Model.Phase + " wave=" + Model.Wave + " floor=" + ActiveFloor);
            }
            TickPreparationStages();
            bool playable = (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial || Model.Phase == PanicPhase.Inspection) && JuryAllowsMovement;
            if (playable && !InputLocked && !CinematicActive && !LossAnnounced && !developerBlocksInput && !BagCameraTransitionActive && CancelFrame != Time.frameCount && !FullFlowScriptedInput)
            {
                if (WorkActive) UpdateWork(dt);
                else if (InventoryOpen) UpdateInventoryKeys();
                else if (IsMoving) UpdateMove(dt);
                else
                {
                    MovePlayer(dt, ReadMove());
                    focusClock -= dt; if (focusClock <= 0) { focusClock = BagFirstPersonActive ? .035f : .12f; RefreshFocus(false); }
                    DispatchWorldInput(new[] { KeyCode.E, KeyCode.F, KeyCode.Q, KeyCode.T, KeyCode.Z, KeyCode.X, KeyCode.Space, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Tab, KeyCode.B, KeyCode.PageUp, KeyCode.PageDown }.Where(Input.GetKeyDown), Input.GetMouseButtonDown(0), hud.PointerOverUi);
                    if (!BagFirstPersonActive && !IsMoving && Mathf.Abs(Input.mouseScrollDelta.y) > .1f && !hud.PointerOverUi) cameraSize = Mathf.Clamp(cameraSize - Input.mouseScrollDelta.y * .35f, 4, 10);
                }
            }
            if (!playable || WorkActive || InventoryOpen) SetMoveAnimation(0, dt);
            if (hero) hero.gameObject.SetActive((!BagCameraOwnsView || BagTransitionShowsBodyV56) && (playable || Model.Phase == PanicPhase.Title || LossAnnounced || CinematicShowsActor));
            ActiveFloor = Mathf.Clamp(Mathf.FloorToInt((player.position.y + .65f) / 3.2f), -1, 1);
            inside = InResidence(player.position);
            UpdateRevisionWorld(dt);
            UpdateResidenceLife(dt);
            TickExteriorLife(dt);
            ObserveJuryPresentation();
            UpdateCameraOrbitInput(hud && hud.PointerOverUi);
            AnimateHinges(dt); SyncVisuals(false); ApplyDirectVisibility(); FollowCamera(dt); ApplyFloorVisibility();
            AnimateCuratedFixtures(dt);
            AnimateV5Furniture();
            UpdateJuryMarker();
            gatherPoseTime = Mathf.Max(0, gatherPoseTime - dt);
            TickHouseAudio();
            TickExperienceFeedbackV414(dt);
            TickGuidanceV54();
            if (hud) hud.TickSecondaryAlertV55(dt);
        }

        void LateUpdate()
        {
            if (!hero || !animator || Model.IsPaused || Model.WaveLedgerOpenV56 || Model.KeyboardHelpOpenV56 || NotebookBlocksInput) return;
            if (ApplyCinematicCharacterPoseV414()) return;
            // Retained V4.13 locomotion is evaluated solely by Animator after MovePlayer's original parameters.
            // Ordinary walking/idle/gather/carry has no procedural sample, Restore, Speed or bone write here.
            if (!WorkActive) return;
            string task = Expedition.Work.IsWorldConstruction ? (workAction == "drain" ? "build-ground" : "build") : workAction;
            PanicCharacterArt.SetTaskPose(hero.gameObject, animator, task, Expedition.Work.Progress);
        }

        Vector2 ReadMove() { return new Vector2((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0), (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0)); }
        static Vector3 Flat(Vector3 p) { return new Vector3(p.x, 0, p.z); }
        void MovePlayer(float dt, Vector2 input)
        {
            Quaternion movementBasisV54 = BagFirstPersonActive ? FpsInputRotationV54 : View.transform.rotation;
            Vector3 forward = Flat(movementBasisV54 * Vector3.forward).normalized, right = Flat(movementBasisV54 * Vector3.right).normalized;
            float speed = Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed;
            speed *= FpsMovementMultiplierV414;
            Vector3 target = Vector3.ClampMagnitude(right * input.x + forward * input.y, 1) * speed;
            desiredVelocity = Vector3.SmoothDamp(desiredVelocity, target, ref smoothVelocity, .075f, 20, dt);
            if (controller.isGrounded && gravitySpeed < 0) gravitySpeed = -2;
            gravitySpeed = Mathf.Max(-20, gravitySpeed - 14 * dt);
            // Hinged leaves are moved by Transform, with autoSyncTransforms disabled.
            // Query the same current door poses in normal play as in overlap checks.
            Physics.SyncTransforms();
            bool groundedBeforeMove = controller.isGrounded; float fallingSpeed = gravitySpeed;
            Vector3 audioMoveStart = player.position;
            controller.Move((desiredVelocity + Vector3.up * gravitySpeed) * dt);
            if (!groundedBeforeMove && controller.isGrounded) effects.Play(StepSurface(), fallingSpeed < -8 ? .55f : .36f);
            Vector3 p = player.position;
            if (p.x < -23f || p.x > 21.3f || Mathf.Abs(p.z) > 21.3f) { p.x = Mathf.Clamp(p.x, -23f, 21.3f); p.z = Mathf.Clamp(p.z, -21.3f, 21.3f); Warp(p); }
            if (desiredVelocity.sqrMagnitude > .025f)
            {
                float targetYaw = Mathf.Atan2(desiredVelocity.x, desiredVelocity.z) * Mathf.Rad2Deg;
                facingYaw = Mathf.SmoothDampAngle(facingYaw, targetYaw, ref yawVelocity, .09f);
                hero.rotation = Quaternion.Euler(0, facingYaw, 0);
                footstepClock += dt * Mathf.Max(.5f, desiredVelocity.magnitude / 2.7f);
                if (footstepClock > .39f && controller.isGrounded && Flat(player.position-audioMoveStart).sqrMagnitude > .00001f) { footstepClock = 0; effects.Play(StepSurface(), speed > WalkSpeed ? .42f : .28f); }
            }
            SetMoveAnimation(desiredVelocity.magnitude / RunSpeed, dt);
            if (animator) animator.SetBool("Airborne", !controller.isGrounded);
        }
        void SetMoveAnimation(float speed, float dt) { if (animator) { animator.SetFloat("Speed", speed, .12f, dt); animator.speed = speed > .02f ? MovementSpeedMultiplier : 1f; } }

        public void HandleKey(KeyCode key)
        {
            if(key==KeyCode.F2){if(hud)hud.ToggleDeveloperMenu();return;}
            if(hud&&(hud.DeveloperMenuOpen||hud.DeveloperInputBlockedThisFrame))return;
            if(Model.IsPaused||NotebookBlocksInput||BagCameraTransitionActive||CancelFrame==Time.frameCount)return;
            if (BagFirstPersonActive && (key == KeyCode.Alpha1 || key == KeyCode.Alpha2 || key == KeyCode.Alpha3 || key == KeyCode.Alpha4 || key == KeyCode.T)) return;
            if (key == KeyCode.Tab || key == KeyCode.B) { ToggleInventory(); return; }
            if (key == KeyCode.PageUp) { cameraSize = Mathf.Max(4, cameraSize - .7f); return; }
            if (key == KeyCode.PageDown) { cameraSize = Mathf.Min(10, cameraSize + .7f); return; }
            if (key == KeyCode.Q) { RefreshFocus(true); return; }
            if (key == KeyCode.T) { ToggleAnchorSurface(); return; }
            if (key == KeyCode.E) { Interact(); return; }
            if (key == KeyCode.F) { TakeLoot(); return; }
            if (key == KeyCode.Z) { SelectLoot(LootIndex-1); return; }
            if (key == KeyCode.X) { SelectLoot(LootIndex+1); return; }
            if (key == KeyCode.Space) { Jump(); return; }
            if (key == KeyCode.Alpha1) QuickAction(1);
            if (key == KeyCode.Alpha2) QuickAction(2);
            if (key == KeyCode.Alpha3) QuickAction(3);
            if (key == KeyCode.Alpha4) QuickAction(4);
        }

        // One discrete world action owns a frame. A newly opened modal must not
        // receive a second shortcut or a click that began in the world.
        public void DispatchWorldInput(IEnumerable<KeyCode> keys, bool primaryClick, bool pointerOverUi)
        {
            bool playable = Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial || Model.Phase == PanicPhase.Inspection;
            if (!playable || !JuryAllowsMovement || InputLocked || Model.IsPaused || NotebookBlocksInput || BagCameraTransitionActive || CinematicActive || LossAnnounced ||
                WorkActive || InventoryOpen || IsMoving || CancelFrame == Time.frameCount ||
                hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame)) return;
            foreach (var key in keys) { DiscreteInputFrame = Time.frameCount; HandleKey(key); return; }
            if (primaryClick && !pointerOverUi) PickMouse();
        }

        void RefreshFocus(bool cycle)
        {
            if (BagFirstPersonActive) { RefreshBagRayFocus(); return; }
            // Candidate membership uses the reachable surface, not a long wall's origin.
            // A blocked selected object stays selected until it leaves range or Q/click changes it.
            Physics.SyncTransforms();
            focusCandidates.Clear();
            foreach (var item in Model.Items)
                if (item.Id != "bagstation" && item.Installed && (item.Kind == ItemKind.Roof || SameLevel(item.Position)) &&
                    !(BagFinalStageActive && Expedition.Sources.Exists(source => source.Id == item.Id)) && InteractionDistance(item.Id) < 2.8f) focusCandidates.Add(item.Id);
            if (!BagFinalStageActive) foreach (var source in Expedition.Sources)
                if (SameLevel(source.Position) && InteractionDistance(source.Id) < 2.8f && !focusCandidates.Contains(source.Id)) focusCandidates.Add(source.Id);
            if (!BagFinalStageActive) foreach (var site in gardenSites)
                if (!site.done && InteractionDistance(site.id) < 2.8f) focusCandidates.Add(site.id);
            foreach (var pair in residenceTargets)
                if (pair.Value && SameLevel(ResidenceFocusPosition(pair.Value)) && InteractionDistance(pair.Key) < 2.8f && !focusCandidates.Contains(pair.Key)) focusCandidates.Add(pair.Key);
            if (!items.ContainsKey("bunker") && InteractionDistance("@bunker") < 2.8f) focusCandidates.Add("@bunker");
            if (BagFinalStageActive) foreach (var supply in Expedition.WorldSupplies)
                if (supply.Count > 0 && supply.Floor == ActiveFloor && InteractionDistance(supply.Id) < 2.4f) focusCandidates.Add(supply.Id);
            focusCandidates.Sort((a, b) => { int score = FocusScore(a).CompareTo(FocusScore(b)); return score != 0 ? score : string.CompareOrdinal(a, b); });
            var available = focusCandidates.Where(id => PositionOf(id, out var point) && InteractionPathClear(id, point)).ToList();
            if (cycle && available.Count > 0) SetFocus(available[(available.IndexOf(focusId) + 1) % available.Count]);
            else if (!focusCandidates.Contains(focusId)) SetFocus(available.Count > 0 ? available[0] : null);
            UpdateContext();
        }
        bool SameLevel(Vector3 p) { return Mathf.Abs(p.y - (ActiveFloor * 3.2f)) < .9f; }
        float DistanceTo(string id) { return InteractionDistance(id); }
        float FocusScore(string id) { var data = Model.Items.Find(i => i.Id == id); return DistanceTo(id) + (data != null && data.Kind == ItemKind.Roof ? 3 : data != null && data.Kind == ItemKind.Wall ? .35f : 0); }

        GameObject InteractionObject(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (id == "@bunker") id = "bunker";
            if (worldSupplyObjects.TryGetValue(id, out var supply) && supply) return supply;
            if (residenceTargets.TryGetValue(id, out var target) && target) return target;
            if (sourceObjects.TryGetValue(id, out var source) && source) return source;
            return items.TryGetValue(id, out var item) && item ? item : null;
        }

        public bool TryGetInteractionBounds(string id, out Bounds bounds)
        {
            bounds = new Bounds(); bool any = false;
            if (string.IsNullOrEmpty(id)) return false;
            if (id == "roof" && PositionOf(id, out var roofPointV56))
            { bounds = new Bounds(roofPointV56 + Vector3.up * .8f, new Vector3(.3f, 1.6f, .3f)); return true; }
            if (wallInteractionColliders.TryGetValue(id, out var segments))
            {
                foreach (var segment in segments) if (segment && segment.enabled && segment.gameObject.activeInHierarchy)
                { if (!any) { bounds = segment.bounds; any = true; } else bounds.Encapsulate(segment.bounds); }
                if (any) return true;
            }
            var root = InteractionObject(id);
            if (root)
            {
                // The actual art bounds outline the loose object, not its generous pick trigger.
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (!renderer || renderer is LineRenderer || !renderer.gameObject.activeInHierarchy || renderer.GetComponent<PanicPermanentlyHiddenV414>()) continue;
                    if (!any) { bounds = renderer.bounds; any = true; } else bounds.Encapsulate(renderer.bounds);
                }
                if (any) return true;
                foreach (var collision in root.GetComponentsInChildren<Collider>(true)) if (collision && collision.enabled && collision.gameObject.activeInHierarchy)
                { if (!any) { bounds = collision.bounds; any = true; } else bounds.Encapsulate(collision.bounds); }
                if (any) return true;
            }
            var site = gardenSites.Find(value => value.id == id);
            if (site != null)
            {
                bounds = site.action == "plant" ? PanicConstructionArt.TreeRowBounds(TreeVariant(site)) : new Bounds(Vector3.zero, new Vector3(1.4f, .05f, 1));
                bounds.center = site.p + Vector3.up * .025f; var size = bounds.size; size.y = .05f; bounds.size = size; return true;
            }
            if (PositionOf(id, out var position)) { bounds = new Bounds(position + Vector3.up * .8f, new Vector3(.3f, 1.6f, .3f)); return true; }
            return false;
        }

        bool InteractionSurfacePoint(string id, Vector3 from, out Vector3 point)
        {
            point = Vector3.zero; float nearest = float.MaxValue; bool found = false;
            IEnumerable<Collider> colliders = null;
            if (id != null && wallInteractionColliders.TryGetValue(id, out var segments)) colliders = segments;
            else { var root = InteractionObject(id); if (root) colliders = root.GetComponentsInChildren<Collider>(true); }
            if (colliders != null) foreach (var collision in colliders)
            {
                if (!collision || !collision.enabled || !collision.gameObject.activeInHierarchy) continue;
                Vector3 seat = collision.ClosestPoint(from); float distance = (seat - from).sqrMagnitude;
                if (distance < nearest) { nearest = distance; point = seat; found = true; }
            }
            if (found) return true;
            if (TryGetInteractionBounds(id, out var bounds)) { point = bounds.ClosestPoint(from); return true; }
            return false;
        }

        public float InteractionDistance(string id)
        { return player && InteractionSurfacePoint(id, player.position + Vector3.up * .9f, out var point) ? Vector3.Distance(point, player.position + Vector3.up * .9f) : 999f; }

        bool InteractionPathClear(string id, Vector3 fallback)
        {
            if (!player) return false;
            Physics.SyncTransforms();
            Vector3 start = player.position + Vector3.up * .9f;
            Vector3 end = InteractionSurfacePoint(id, start, out var surface) ? surface : fallback + Vector3.up * .8f;
            // A frame's nearest lower/side edge can lie in the solid sill or jamb.
            // Aim through the actual window opening center; retain the ordinary first-hit
            // obstruction rule and nearest-surface reach distance for every target.
            var targetItem = Model.Items.Find(item => item.Id == id);
            if (targetItem != null && targetItem.Kind == ItemKind.Window && TryGetInteractionBounds(id, out var windowBounds))
                end = windowBounds.center;
            Vector3 delta = end - start; float distance = delta.magnitude; if (distance < .05f) return true;
            var hits = Physics.RaycastAll(start, delta / distance, distance + .025f, ~(1 << 30), QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider == controller || hit.collider.GetComponentInParent<ResidenceLootPick>() || hit.transform.IsChildOf(player)) continue;
                if (id == "@computer" && residenceTargets.TryGetValue(id, out var device) && device && hit.transform == device.transform.parent) continue;
                var pick = hit.collider.GetComponentInParent<HomePick>();
                if (pick && (pick.Id == id || id == "@bunker" && pick.Id == "bunker" || id == "bagstation" && pick.Id == "@bagstation")) return true;
                return false;
            }
            return true;
        }
        bool FocusPosition(out Vector3 p) { return PositionOf(focusId, out p); }
        bool PositionOf(string id, out Vector3 p)
        {
            p = Vector3.zero; if (id == null) return false;
            if (id == "@bunker") { p = BunkerPosition; return true; }
            var supply = Expedition.FindWorldSupply(id); if (supply != null && supply.Count > 0) { p = supply.Position; return true; }
            if (residenceTargets.TryGetValue(id,out var target) && target) { p=ResidenceFocusPosition(target); return true; }
            var site = gardenSites.Find(s => s.id == id); if (site != null) { p = site.p; return true; }
            var source = Expedition.Sources.Find(s => s.Id == id); if (source != null) { p = source.Position; return true; }
            var item = Model.Items.Find(i => i.Id == id); if (item != null) { p = item.Kind == ItemKind.Roof ? new Vector3(0, ActiveFloor * 3.2f, -4.8f) : item.Position; return true; }
            return false;
        }
        public void SetFocus(string id)
        {
            if (Selected != null && items.TryGetValue(Selected.Id, out var old)) PanicArt.SetSelected(old, false);
            focusId = id;
            if (Selected != null && items.TryGetValue(Selected.Id, out var next)) PanicArt.SetSelected(next, true);
            UpdateContext();
        }
        void UpdateContext()
        {
            if (UpdateShelterContextV5()) return;
            if (UpdateUtilityContextV414()) return;
            UpdateResidenceContext();
        }

        public void Interact()
        {
            if (BagFirstPersonActive) { InteractBagStage(); return; }
            if (TryJuryInteract()) return;
            if (Expedition.FindWorldSupply(focusId) != null) { TakeWorldSupply(); return; }
            if (!CanInteract || WorkActive || IsMoving) { Model.Notify("Etkileşmek için hedefe yaklaş."); return; }
            if (TryInteractUtilityV414()) return;
            if (focusId == "@bunker" || Selected != null && Selected.Id == "bunker")
            {
                if (!BagFinalStageActive) { Notice("Çanta hazırlığı son 45 saniyede başlar; önce evdeki önlemleri tamamla."); return; }
                TryEnterActiveShelterV5(); return;
            }
            if (focusId == "@panicroom") { TryEnterActiveShelterV5(); return; }
            var source = NearestSource;
            if (source != null)
            {
                ToggleContainer(); return;
            }
            if(focusId=="@bagstation") { ToggleInventory(); return; }
            if(focusId=="@radio") { ToggleRadio(); return; }
            if(focusId=="@fireplace") { ToggleHearth(); return; }
            if(focusId=="@computer") { ToggleComputer(); return; }
            if(focusId=="@garage") { if(Model.UtilitySafety.PowerOff){Notice("Ana şalter kapalı; garajın iç kapısını kullan.");return;}garageOpen=!garageOpen; effects.Play("garage",.45f); UpdateContext(); return; }
            if (focusId == PanicItemActionRules.FireStationId) { BeginWork("fire", focusId); return; }
            if(focusId!=null&&focusId.StartsWith("@")&&hinges.ContainsKey(focusId)){ToggleDoor();UpdateContext();return;}
            var site = gardenSites.Find(s => s.id == focusId);
            if (site != null) { if (!site.done) BeginWork(site.action, site.id); return; }
            if (Selected != null && hinges.ContainsKey(Selected.Id)) ToggleDoor();
        }
        public void ToggleDoor()
        {
            if (!CanInteract) return;
            if (hinges.TryGetValue(focusId ?? "", out var hinge))
            {
                // A reopen retraces the existing swing. Pick a new side only once closed;
                // flipping a partly closed leaf to the other branch sweeps across the actor.
                if (!hinge.open && hinge.directionalDoor && Quaternion.Angle(hinge.leaf.localRotation,hinge.closed)<.5f)
                {
                    Vector3 normal=hinge.frame.TransformDirection(hinge.closed*Vector3.forward);
                    float side=Vector3.Dot(player.position-hinge.frame.position,normal);
                    if(Mathf.Abs(side)<.05f)
                    {
                        float motion=Vector3.Dot(desiredVelocity,normal);
                        side=Mathf.Abs(motion)>.05f?-motion:Vector3.Dot(-hero.forward,normal);
                    }
                    hinge.openAngle=side>=0?100:-100;
                }
                hinge.open = !hinge.open; hinge.closeBlocked=hinge.motionBlocked=false; gatherPoseTime = .55f; effects.Play("door_wood", .35f);
            }
        }
        public void ToggleAnchorSurface() { AnchorMode = AnchorMode == "wall" ? "floor" : "wall"; UpdateContext(); }
        public void QuickAction(int index)
        {
            if(BagFinalStageActive || BagCameraOwnsView || Model.IsPaused || NotebookBlocksInput)return;
            if (!CanInteract || Model.Phase == PanicPhase.Inspection) { Model.Notify("Hazırlık sırasında nesneye yaklaş."); return; }
            if (focusId == PanicItemActionRules.FireStationId && index == 2) { BeginWork("fire", focusId); return; }
            var item = Selected; if (item == null) return;
            if (index == 1 && PanicItemActionRules.CanReleaseAnchor(item))
            {
                if (WorkActive || IsMoving || !Model.CanBeginJuryWork("anchorwall", item.Id)) return;
                Model.ToggleAnchor(item.Id); effects.Play("anchor_release",.35f); SyncVisuals(true); UpdateContext(); return;
            }
            if (index != 4 && index != Expedition.PrimaryShortcutV414(item)) { Model.Notify("Bu nesnede uygulanabilir işlem yok."); return; }
            if (index == 1)
            {
                if (item.Anchored) { Model.ToggleAnchor(item.Id); return; }
                if (!item.Movable) { Model.Notify("Bu yapı sabitlenmez; 3 ile güçlendirilir."); return; }
                AnchorMode = NearWall(item.Position) ? "wall" : "floor";
                BeginWork(AnchorMode == "wall" ? "anchorwall" : "anchorfloor", item.Id);
            }
            else if (index == 2) { Model.Notify("Yangın tüpünü ekipman noktasında hazırla."); }
            else if (index == 3) BeginWork("upgrade", item.Id);
            else if (index == 4) BeginMove();
        }
        bool NearWall(Vector3 p) { return Mathf.Abs(p.x) > 6.3f || Mathf.Abs(p.z) > 4.5f || Mathf.Abs(p.x - 2) < 1.1f; }

        public void BeginWork(string action, string target)
        {
            if(BagFinalStageActive || BagCameraOwnsView || Model.IsPaused || NotebookBlocksInput)return;
            if (Model.Phase != PanicPhase.Preparing && Model.Phase != PanicPhase.Tutorial) return;
            if (!PositionOf(target, out var juryTaskPosition) || !SameLevel(juryTaskPosition) || InteractionDistance(target) > 2.7f || !InteractionPathClear(target, juryTaskPosition))
            { Model.Notify("İş için hedefe açık yoldan yaklaş."); return; }
            if (WorkActive || IsMoving) return;
            if (!Expedition.CanBeginWork(action,target)) return;
            if (action == "plant" && !TreeRowClearOfActor(target)) { Model.Notify("Ağaç gövdesinin yerinden biraz uzaklaş, sonra kurulumu başlat."); return; }
            SetFocus(target);
            workAction = action; workTarget = target; workAge = 0;
            ResetManualWorkPointerV414();
            if (!Expedition.BeginValidatedWorkV414(action, target, Model.IsJuryDemo)) { CancelWork(); return; }
            BeginWorkAudioV56();
            InventoryOpen = false;
            if (PositionOf(target, out var taskPosition)) { var toward = Flat(taskPosition-player.position); if (toward.sqrMagnitude>.01f) { facingYaw=Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg; hero.rotation=Quaternion.Euler(0,facingYaw,0); } }
            animator.SetBool("Working", true); animator.SetBool("Kneeling", action == "drain" || action == "anchorfloor");
            var hand = PanicCharacterArt.GetCharacterBone(hero.gameObject, animator, HumanBodyBones.RightHand);
            if (hand && !Expedition.Work.IsUtility && action != "plant") { handTool = PanicCharacterArt.CreateHandTool(Expedition.Work.UsesHammer ? action == "drain" ? "drain" : "hammer" : action); handTool.transform.SetParent(hand,false); handTool.transform.localRotation=Quaternion.Euler(0,180,0); }
            if (!Expedition.Work.IsWorldConstruction)
            { workRig = PanicWorkArt.CreateWorkRig(action); workRig.transform.position = new Vector3(100, 100, 100); LayerRecursive(workRig, 30); workCamera.enabled = true; }
            else { StartDirectConstruction(action, target); Expedition.Work.TryClick(0); }
            var actorCue = GetComponent<PanicActorOcclusion>(); if (actorCue) actorCue.RefreshCandidates();
            Model.Notify(Expedition.Work.StepHint);
        }
        void UpdateWork(float dt)
        {
            UpdateManualWorkV414(dt);
        }
        public void ProcessWorkInput(float dt,float direction,bool held,bool pressed,bool pulse,bool cancel)
        {
            if (cancel) { effects.Play("ui",.20f); CancelWork(); return; }
            StepWork(dt,direction,held,pressed,pulse);
        }
        public void StepWork(float dt,float direction,bool held,bool pressed,bool pulse)
        {
            if(!WorkActive)return;workAge+=dt;
            Expedition.Work.Tick(dt,direction,held,pressed,pulse);
            TickWorkAudio();
            if (handTool && Expedition.Work.UsesHammer && workAction != "drain") handTool.transform.localRotation=Quaternion.Euler(-55f*Expedition.Work.ToolMotion,180,0);
            if(workRig)PanicWorkArt.UpdateWorkRig(workRig,Expedition.Work.Progress,Expedition.Work.CompletedSteps,workAge,Expedition.Work.Cursor,Expedition.Work.IsApplying,Expedition.Work.ToolMotion);
            AnimateDirectConstruction(Expedition.Work.Progress);
            if(Expedition.Work.Complete)CompleteWork();
        }
        public void CompleteWork()
        {
            if (!WorkActive || !Expedition.Work.Complete) return;
            TickWorkAudio();
            // Recheck before the authoritative stock/budget commit, even if the actor
            // was clear when installation began. Never create a solid trunk in it.
            if (workAction == "plant" && !TreeRowClearOfActor(workTarget))
            { CancelWork(); Model.Notify("Ağaç gövdesinin yerinden biraz uzaklaş; malzemelerin sende kaldı."); return; }
            bool done = Expedition.TryCompleteWork(workAction, workTarget);
            if (done)
            {
                CompleteWorkAudioV56();
                var site = gardenSites.Find(s => s.id == workTarget);
                if (site != null)
                {
                    site.done = true; site.marker.SetActive(false); site.built = CreateCompletedSite(site); site.built.transform.position = site.p;
                    if(site.action=="plant") AddTreeRowColliders(site.built, gardenSites.Where(s => s.action == "plant").ToList().IndexOf(site));
                    else if(site.action=="fence"||site.action=="hose") AddBoundsCollider(site.built);
                }
                RefreshFireStation();
                effects.Play("objective_tick", .35f);
            }
            CancelWork(); SyncVisuals(true); UpdateContext();
            var actorCue = GetComponent<PanicActorOcclusion>(); if (actorCue) actorCue.RefreshCandidates();
        }
        public void CancelWork()
        {
            if (Expedition == null) return;
            EndWorkAudioV56();
            ResetManualWorkPointerV414();
            if (WorkActive) CancelFrame=Time.frameCount;
            // One explicit actual-work exit, never an ordinary walk-frame pose restore.
            if (hero && workAction != null) PanicCharacterArt.ClearTaskPose(hero.gameObject);
            CancelDirectConstruction();
            Expedition.Work.Cancel();
            Expedition.CancelFloorWallUpgradeV414();
            if (animator) { animator.SetBool("Working", false); animator.SetBool("Kneeling", false); }
            if (workRig) Destroy(workRig); workRig = null;
            if (handTool) PanicCharacterArt.ReleaseHandTool(handTool); handTool=null;
            if (workCamera) workCamera.enabled = false;
            workAction = null; workTarget = null;
        }

        public void ToggleInventory() { if (WorkActive || IsMoving) return; InventoryOpen = !InventoryOpen; CancelFrame=DiscreteInputFrame=Time.frameCount;desiredVelocity=smoothVelocity=Vector3.zero;effects.Play("bag",.28f); }
        void UpdateInventoryKeys()
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Backspace)) { ToggleInventory(); return; }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) InventoryMove(1);
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) InventoryMove(-1);
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) InventoryMove(4);
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) InventoryMove(-4);
            if (Input.GetKeyDown(KeyCode.Return)) InventoryUse();
            // Prepared first-aid kits are world pickups; there is no first-aid crafting shortcut.
        }
        public void InventoryMove(int delta) { InventoryIndex = (InventoryIndex + delta + 12) % 12; effects.Play("ui",.18f); }
        public void InventorySelect(int index) { InventoryIndex = Mathf.Clamp(index, 0, 11); effects.Play("ui",.18f); }
        public void InventoryUse() { string id=Expedition.Inventory[InventoryIndex].Id; if (Expedition.PackSlot(InventoryIndex)) effects.Play(id == "radio" || id == "flashlight" ? "battery" : "pickup_cloth", .3f); else effects.Play("reject",.25f); }
        public void CraftFirstAid() { if (Expedition.CraftFirstAid()) effects.Play("craft", .4f); else effects.Play("reject",.25f); }

        public void StartGame(bool tutorial)
        {
            if (hud) hud.ResetKeyboardHelpPresentationV56();
            ConfigurePresentationAudio(false); ReleaseJuryController(); ResetJuryPresentation();
            CancelWork(); CancelMove();
            if (Model.Phase != PanicPhase.Title && Model.Phase != PanicPhase.GameOver) Model.ReturnToTitle();
            Model.StartRun(tutorial); ConfigureResidenceModel(); Expedition.Reset(); ResetResidenceLife(); previousWave = Model.Wave; previousPhase = Model.Phase; focusId = null; InventoryOpen = false; ClearGarden(); Warp(new Vector3(0, .05f, -7.5f)); SyncVisuals(true); effects.OnPhase(Model.Phase); QueueRadio(DisasterVoiceId());
            ResetAssistantV55();
            if (hud) hud.ResetSecondaryAlertV55();
            ResetOverhaulRunV414(); BeginIntroStoryV414();
            if (hud) hud.BeginFreeplayOnboardingV414();
        }
        public void FinishTutorial() { CancelWork(); Model.FinishTutorial(); ConfigureResidenceModel(); Expedition.Reset(); ResetResidenceLife(); previousWave = Model.Wave; previousPhase = Model.Phase; ClearGarden(); Warp(new Vector3(0, .05f, -7.5f)); SyncVisuals(true); QueueRadio(DisasterVoiceId()); }
        public void FinishInspection() { if (JuryActive) AdvanceJuryDemo(); else if (Model.IsSimulation) ReturnToMenu(); else Model.FinishInspection(); }
        public void ReturnToMenu() { if(hud)hud.ResetKeyboardHelpPresentationV56(); if(hud)hud.CloseSimulationSelector(); ConfigurePresentationAudio(false); ReleaseJuryController(); ResetJuryPresentation(); CancelWork(); CancelMove(); Model.ReturnToTitle(); ConfigureResidenceModel(); Expedition.Reset(); ResetResidenceLife(); InventoryOpen = false; focusId = null; SyncVisuals(true); }
        void ClearGarden() { foreach (var s in gardenSites) { s.done = false; s.marker.SetActive(true); if (s.built) Destroy(s.built); s.built = null; } }
        public void Warp(Vector3 p) { controller.enabled = false; player.position = p; controller.enabled = true; gravitySpeed = -2; desiredVelocity = Vector3.zero; }
        public void SmokeMove(Vector2 direction, float dt) { MovePlayer(dt, direction); }
        public void SmokeFocus(string id) { SetFocus(id); }

        void PickMouse()
        {
            if(BagFirstPersonActive){RefreshBagRayFocus();if(Expedition.FindWorldSupply(focusId)!=null)TakeWorldSupply();return;}
            Physics.SyncTransforms();
            var hits = Physics.RaycastAll(View.ScreenPointToRay(Input.mousePosition), 150, ~((1 << 28) | (1 << 30)), QueryTriggerInteraction.Collide);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider == controller || hit.transform.IsChildOf(player) || !PlacementPickVisible(hit.collider)) continue;
                var pick = hit.collider.GetComponentInParent<HomePick>();
                if (pick)
                {
                    if (Expedition.FindWorldSupply(pick.Id) != null && !BagFinalStageActive || BagFinalStageActive && Expedition.Sources.Exists(source => source.Id == pick.Id)) return;
                    if (PositionOf(pick.Id, out var position) && (SameLevel(position) || Model.Items.Exists(item => item.Id == pick.Id && item.Kind == ItemKind.Roof))) SetFocus(pick.Id);
                    return;
                }
                // A visible physical obstacle owns this pixel; don't select through it.
                return;
            }
        }
        void BindHinge(GameObject root, string id, bool isDoor)
        {
            if(root.GetComponent<PanicV5FurnitureRig>())return;
            if (!isDoor && PanicCuratedAssetArt.HasAnimatedMechanism(root)) return;
            var leaf = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "DoorLeaf" || t.name == "Lid");
            if (!leaf) { if (isDoor) AddBoundsCollider(root); return; }
            hinges[id] = new Hinge { id = id, leaf = leaf, frame=root.transform, closed = leaf.localRotation, closedPosition=leaf.localPosition,drawer=id=="desk_upper", lid = leaf.name == "Lid", directionalDoor=isDoor&&leaf.name=="DoorLeaf" };
            if (isDoor) { AddBoundsCollider(leaf.gameObject); hinges[id].collision=leaf.GetComponent<Collider>(); }
        }
        void AnimateHinges(float dt)
        {
            foreach (var h in hinges.Values)
            {
                h.closeBlocked=h.motionBlocked=false;
                var s = Expedition.Sources.Find(x => x.Id == h.id); bool open = s != null ? s.Open : h.open;
                if(h.drawer){h.leaf.localPosition=Vector3.Lerp(h.leaf.localPosition,h.closedPosition+(open?Vector3.back*.28f:Vector3.zero),dt*9);continue;}
                Quaternion target = h.closed * Quaternion.Euler(h.lid ? h.openAngle : 0, h.lid ? 0 : h.openAngle, 0);
                Quaternion next=Quaternion.Slerp(h.leaf.localRotation, open ? target : h.closed, dt * 9);
                h.leaf.localRotation = h.directionalDoor ? SafeDoorPose(h,next) : next;
            }
        }
        void ApplyFloorVisibility()
        {
            int viewFloor=CinematicViewFloorV5;Vector3 viewFocus=CinematicViewFocusV5;
            bool cutawayInside = (IntroStoryActiveV414 && residenceCinematicAge >= 4f) || inside && !ResidenceCinematicExteriorView && (!BagCameraOwnsView || BagDollyUsesIsometricCutawayV56);
            for (int i = 0; i < floorVisuals.Count; i++)
            {
                int floor=i-1; bool shown = !cutawayInside || floor == viewFloor;
                SetResidenceRenderersVisible(floorVisuals[i], shown);
                foreach (var t in floorVisuals[i].GetComponentsInChildren<Transform>(true))
                    if (t.name == "FrontHighWalls") t.gameObject.SetActive(true);
                if (shown && cutawayInside && View) ApplyFacadeCutaway(floorVisuals[i], viewFocus, -8, 8, -6, 6, floor * 3.2f);
            }
            if (roof) roof.SetActive(!cutawayInside);
            UpdateResidenceVisibility(cutawayInside);
            foreach (var go in floorItems)
            {
                if (!go || go.name == movingId) continue;
                int itemFloor = Mathf.Clamp(Mathf.FloorToInt((go.transform.position.y + .15f) / 3.2f), -1, 1);
                bool stair = go.transform.parent && go.transform.parent.name.StartsWith("Stair flight ", StringComparison.Ordinal);
                bool shown = !cutawayInside || itemFloor == viewFloor || stair && itemFloor + 1 == viewFloor;
                var data = Model.Items.Find(x => x.Id == go.name);
                if (shown && cutawayInside && data != null && data.Kind == ItemKind.Window && View)
                {
                    Vector3 cameraSide = View.transform.position - viewFocus;
                    if (cameraSide.x < -.1f && data.Position.x < -7.6f || cameraSide.x > .1f && data.Position.x > 7.6f || cameraSide.z > .1f && data.Position.z > 5.6f || cameraSide.z < -.1f && data.Position.z < -5.6f) shown = false;
                }
                go.SetActive(go == hearthFlame ? hearthEnabled : data == null || data.Installed);
                SetResidenceRenderersVisible(go, shown);
            }
            if (hearthLight) hearthLight.enabled = hearthEnabled && (!cutawayInside || ActiveFloor == 0);
            RefreshVisualReadabilityV56();
        }

        void SetResidenceRenderersVisible(GameObject root, bool visible)
        {
            if (!root) return;
            int key = root.GetInstanceID();
            if (!visibilityRenderers.TryGetValue(key, out var renderers))
                visibilityRenderers[key] = renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
                if (renderer) { bool shown = visible && !renderer.GetComponent<PanicPermanentlyHiddenV414>(); if (renderer.enabled != shown) renderer.enabled = shown; }
        }

        void SyncVisuals(bool force)
        {
            foreach (var item in Model.Items)
            {
                if (!items.TryGetValue(item.Id, out var go)) continue;
                go.transform.position = item.Position; go.transform.rotation = Quaternion.Euler(0, item.Rotation, 0);
                var lootSource = Expedition.Sources.Find(s => s.Id == item.Id);
                if (lootSource != null) { lootSource.Position = item.Position; lootSource.Floor = Mathf.RoundToInt(item.Position.y / 3.2f); }
                string state = item.Tier + "|" + item.Anchored + "|" + item.AnchorSurface + "|" + item.FloodProtected + "|" + item.FireProtected + "|" + item.Damage.ToString("F1");
                if (!force && visualKeys.TryGetValue(item.Id, out var old) && old == state) continue;
                visualKeys[item.Id] = state;
                if (item.Kind != ItemKind.Wall && item.Kind != ItemKind.Roof)
                { if(!ResidenceIds.Contains(item.ArtId)){PanicArt.SetTier(go, item.Tier);PanicArt.SetDamage(go, item.Damage, Model.Disaster);} }
                if (item.Damage > 0)
                {
                    var block = new MaterialPropertyBlock();
                    foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                    {
                        if (!renderer.sharedMaterial || !renderer.sharedMaterial.HasProperty("_Color")) continue;
                        renderer.GetPropertyBlock(block); block.SetColor("_Color", Color.Lerp(renderer.sharedMaterial.color, new Color(.16f,.14f,.12f), Mathf.Clamp01(item.Damage / 125f))); renderer.SetPropertyBlock(block);
                    }
                }
                else if (item.Damage <= 0) foreach (var renderer in go.GetComponentsInChildren<Renderer>(true)) renderer.SetPropertyBlock(null);
                RebuildHardware(item, go);
            }
            ApplyComputerPower();
            ApplyMainsPowerV5();
        }
        void RebuildHardware(PanicItem item, GameObject go)
        {
            if (hardware.TryGetValue(item.Id, out var old) && old) Destroy(old);
            var holder = new GameObject("Applied measures"); holder.transform.SetParent(go.transform, false); hardware[item.Id] = holder;
            if (item.Anchored)
            {
                GameObject attachment = PanicResidenceArt.CreateSecuredHardware(item.Size, item.AnchorSurface == "wall");
                attachment.transform.SetParent(holder.transform, false);
            }
            if (item.FloodProtected) { var g = PanicAttachmentArt.CreateFloodGuard(1.82f, .85f); g.transform.SetParent(holder.transform, false); g.transform.localPosition = new Vector3(0, .8f, -.22f); }
            if (item.FireProtected) { var g = CreateVisual("fireunit"); g.transform.SetParent(holder.transform, false); g.transform.localPosition = new Vector3(item.Size.x * .35f, .1f, -.35f); }
            if (item.Tier > 0 && (item.Kind == ItemKind.Wall || item.Kind == ItemKind.Roof || item.Kind == ItemKind.Window || item.Kind == ItemKind.Door))
            {
                var g = PanicConstructionArt.CreateReinforcement(item); g.transform.SetParent(holder.transform, false);
            }
            TryBuildYarkinDamageMarkHybrid(item, holder);
            OnPresentationHardwareRebuilt(item, holder);
            visibilityRenderers.Remove(items[item.Id].GetInstanceID());
        }

        void FollowCamera(float dt)
        {
            if (BagCameraOwnsView) { UpdateBagPreparationCamera(dt); return; }
            if (ApplyResidenceCinematicCamera(dt)) return;
            View.orthographic=true;
            bool title = Model.Phase == PanicPhase.Title;
            Vector3 aim = title ? new Vector3(0, 1.8f, -1) : player.position + Vector3.up * .85f;
            float size = title ? 12.5f : cameraSize;
            if (Model.Phase == PanicPhase.Disaster || Model.Phase == PanicPhase.Aftermath) { aim = new Vector3(0, 1.5f, 0); size = 13; }
            Vector3 offset = cameraOffset;
            ApplyCameraOrbit(ref offset);
            bool constructionShot = ApplyConstructionCamera(ref aim, ref offset);
            if (Model.Phase == PanicPhase.Disaster) { float t = Mathf.Clamp01(1-Model.PhaseTime/8f); offset=Quaternion.Euler(0,Mathf.Lerp(-5,7,t),0)*offset; size=Mathf.Lerp(13,11.5f,t); }
            Vector3 shake = Model.Phase == PanicPhase.Disaster && effects ? effects.DisasterVisualImpulseV414 : Vector3.zero;
            View.transform.position = Vector3.Lerp(View.transform.position, aim + offset + shake, dt * (constructionShot ? 14 : 5));
            View.transform.rotation = Quaternion.LookRotation(-offset.normalized);
            View.orthographicSize = Mathf.Lerp(View.orthographicSize, size, dt * 4);
        }
        void OnGUI()
        {
            if(NotebookBlocksInput || (Model != null && (Model.WaveLedgerOpenV56 || Model.KeyboardHelpOpenV56)))return;
            DrawGatherRewardsV414();
            if(BagCameraOwnsView){DrawBagPreparationOverlay();return;}
            if (BagCameraOwnsView || !View || Model.Phase == PanicPhase.Title || Model.Phase == PanicPhase.GameOver || Model.Phase == PanicPhase.Success || WorkActive || InventoryOpen || CinematicActive || InputLocked || !JuryAllowsMovement || (hud&&hud.DeveloperMenuOpen)) return;
            DrawJuryWorldTarget();
            DrawPrimaryInteractionV51();
            if (Model.Phase == PanicPhase.Inspection)
            {
                foreach (var item in Model.Items.Where(i => i.Contribution > .1f && SameLevel(i.Position)).OrderByDescending(i => i.Contribution).Take(3))
                    DrawMarker(item.Position + Vector3.up * Mathf.Max(.9f, item.Size.y), "!  " + item.Label + " · +" + item.Contribution.ToString("F1") + "%", 90);
            }
        }
        void DrawMarker(Vector3 p, string label, float width) { if (hud) hud.DrawWorldMarker(p, label, width); }
        void AddBox(string name, Vector3 p, Vector3 size) { var go = new GameObject(name); go.transform.position = p; go.AddComponent<BoxCollider>().size = size; }
        void AddBoundsCollider(GameObject go)
        {
            var meshes = go.GetComponentsInChildren<MeshFilter>(true); if (meshes.Length == 0) return;
            Bounds b = new Bounds(); bool any = false;
            foreach (var mf in meshes)
            {
                if (!mf.sharedMesh) continue;
                var mb = mf.sharedMesh.bounds;
                for (int mask = 0; mask < 8; mask++)
                {
                    var v = mb.center + Vector3.Scale(mb.extents, new Vector3((mask & 1) == 0 ? -1 : 1, (mask & 2) == 0 ? -1 : 1, (mask & 4) == 0 ? -1 : 1));
                    v = go.transform.InverseTransformPoint(mf.transform.TransformPoint(v)); if (!any) { b = new Bounds(v, Vector3.zero); any = true; } else b.Encapsulate(v);
                }
            }
            if (any) { var col = go.AddComponent<BoxCollider>(); col.center = b.center; col.size = b.size; }
        }
        LineRenderer MakeLine(string name, Vector3[] points, Color color, float width)
        {
            var line = new GameObject(name).AddComponent<LineRenderer>();
            if (!outlineMaterial) outlineMaterial = new Material(Shader.Find("Sprites/Default"));
            line.sharedMaterial = outlineMaterial; line.useWorldSpace = false; line.loop = true; line.positionCount = points.Length; line.SetPositions(points); line.widthMultiplier = width; line.startColor = line.endColor = color; return line;
        }
        void LayerRecursive(GameObject go, int layer) { foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer; }
    }
}

