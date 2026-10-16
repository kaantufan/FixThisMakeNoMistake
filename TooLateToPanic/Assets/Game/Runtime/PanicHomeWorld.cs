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
        public Camera View { get; private set; }
        public RenderTexture WorkTexture { get; private set; }
        public Vector3 PlayerPosition { get { return player ? player.position : Vector3.zero; } }
        public bool CanInteract { get { return player && FocusPosition(out Vector3 p) && Vector3.Distance(p, player.position) < 2.4f && InteractionPathClear(focusId,p); } }
        public static readonly Vector3 BunkerPosition = new Vector3(11, 0, -7);
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
        readonly List<string> focusCandidates = new List<string>();
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
        GUIStyle markerStyle;
        readonly Vector3 cameraOffset = new Vector3(11, 13, -15);

        void Awake()
        {
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0; QualitySettings.antiAliasing = 4;
            var args = Environment.GetCommandLineArgs();
            bool smoke = Array.IndexOf(args, "--home-smoke") >= 0;
            bool bathroomSmoke = Array.IndexOf(args, "--bathroom-smoke") >= 0;
            bool doorsStairsSmoke = Array.IndexOf(args, "--doors-stairs-smoke") >= 0;
            bool interactionSmoke = Array.IndexOf(args, "--interaction-smoke") >= 0;
            bool doorJamSmoke = Array.IndexOf(args, "--door-jam-smoke") >= 0;
            bool developerSmoke = Array.IndexOf(args, "--developer-smoke") >= 0;
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--home-evidence") EvidencePath = args[i + 1];
            if (!string.IsNullOrEmpty(EvidencePath)) { System.IO.Directory.CreateDirectory(EvidencePath); PanicModel.ScorePathOverride = System.IO.Path.Combine(EvidencePath, "isolated-home-scores.json"); }
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
            if (smoke) Application.runInBackground = true;
            Model = new PanicModel(); ConfigureResidenceModel(); Expedition = new PanicExpedition(Model);
            BuildWorld();
            hud = gameObject.AddComponent<PanicHomeHud>(); hud.World = this;
            effects = gameObject.AddComponent<PanicHomeEffects>(); effects.World = this;
            InitResidenceLife();
            previousPhase = Model.Phase; previousWave = Model.Wave;
            if (smoke) gameObject.AddComponent<PanicHomeSmoke>();
            if (Array.IndexOf(args, "--residence-smoke") >= 0) { Application.runInBackground=true; gameObject.AddComponent<PanicResidenceSmoke>(); }
            if (Array.IndexOf(args, "--refinement-smoke") >= 0) { Application.runInBackground=true; gameObject.AddComponent<PanicRefinementSmoke>(); }
            if (bathroomSmoke) gameObject.AddComponent<PanicBathroomSmoke>();
            if (doorsStairsSmoke) gameObject.AddComponent<PanicDoorsStairsSmoke>();
            if (interactionSmoke) gameObject.AddComponent<PanicInteractionSmoke>();
            if (doorJamSmoke) gameObject.AddComponent<PanicDoorJamSmoke>();
            if(developerSmoke)gameObject.AddComponent<PanicDeveloperSmoke>();
            Debug.Log("HOME_V45_READY developerMenu=true cheatsExcludeScores=true safeDoorMotion=true partialReopenKeepsDirection=true clearEntrance=true mousePlacement=true materialRequirements=true bathroomSouthEntry=true physicalStairRails=true disasterAudioOnly=true offline=true");
        }

        public GameObject CreateVisual(string id)
        {
            if (ResidenceIds.Contains(id)) { var cached=Resources.Load<GameObject>("ResidenceArt/"+id); return cached ? Instantiate(cached) : PanicResidenceArt.Create(id); }
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
                    else { AddBoundsCollider(go);BindHinge(go,item.Id,false); }
                }
                items[item.Id] = go; visualKeys[item.Id] = ""; floorItems.Add(go);
            }
            foreach (var source in Expedition.Sources)
            {
                GameObject go;
                if (!items.TryGetValue(source.Id, out go))
                {
                    go = CreateVisual(source.ArtId); go.name = source.Id; go.transform.position = source.Position;
                    if(source.Id=="bath_cabinet"){go.transform.position=source.Position+Vector3.up*1.08f;go.transform.rotation=Quaternion.Euler(0,270,0);}
                    AddBoundsCollider(go); floorItems.Add(go);
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
            AddSite("plant_west1", "Fidan dik", "plant", new Vector3(-11, 0, 4));
            AddSite("plant_west2", "Fidan dik", "plant", new Vector3(-12.5f, 0, -.5f));
            AddSite("plant_east", "Fidan dik", "plant", new Vector3(11, 0, 9));
            AddSite("drain_west", "Drenaj kanalı aç", "drain", new Vector3(-5, 0, -8.5f));
            AddSite("drain_east", "Drenaj kanalı aç", "drain", new Vector3(5, 0, -8.5f));
            AddSite("fence_west", "Çit kur", "fence", new Vector3(-11, 0, -11));
            AddSite("fence_east", "Çit kur", "fence", new Vector3(11, 0, -11));
            AddSite("fence_north", "Çit kur", "fence", new Vector3(0, 0, 10));
            AddSite("hose_station", "Yangın hortumu kur", "hose", new Vector3(10, 0, 3));
        }

        void AddSite(string id, string label, string action, Vector3 p)
        {
            var ring = MakeLine(id, new[] { new Vector3(-.7f, .02f, -.5f), new Vector3(.7f, .02f, -.5f), new Vector3(.7f, .02f, .5f), new Vector3(-.7f, .02f, .5f) }, new Color(.51f, .63f, .52f), .025f);
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
            var sun = new GameObject("Overcast dusk sun").AddComponent<Light>(); sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(32, -38, 0); sun.color = new Color(.65f, .73f, .87f); sun.intensity = .75f; sun.shadows = LightShadows.Soft; sun.shadowBias = .025f;
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
            if(hud)hud.ProcessDeveloperHotkeys();
            bool developerBlocksInput=hud&&(hud.DeveloperMenuOpen||hud.DeveloperInputBlockedThisFrame);
            float dt = Mathf.Min(Time.deltaTime, .08f);
            if (!InputLocked && !LossAnnounced && !developerBlocksInput) Model.Tick(dt);
            if (Model.Wave != previousWave) { Expedition.OnNewWave(); previousWave = Model.Wave; }
            if (Model.Phase != previousPhase)
            {
                CancelMove(); CancelWork(); InventoryOpen = false;
                if (effects) effects.OnPhase(Model.Phase);
                OnResidencePhase(Model.Phase,previousPhase);
                previousPhase = Model.Phase;
                Debug.Log("HOME_PHASE " + Model.Phase + " wave=" + Model.Wave + " floor=" + ActiveFloor);
            }
            bool playable = Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial || Model.Phase == PanicPhase.Inspection;
            if (playable && !InputLocked && !CinematicActive && !LossAnnounced && !developerBlocksInput)
            {
                if (WorkActive) UpdateWork(dt);
                else if (InventoryOpen) UpdateInventoryKeys();
                else if (IsMoving) UpdateMove(dt);
                else
                {
                    MovePlayer(dt, ReadMove());
                    focusClock -= dt; if (focusClock <= 0) { focusClock = .12f; RefreshFocus(false); }
                    foreach (var key in new[] { KeyCode.E, KeyCode.F, KeyCode.Q, KeyCode.T, KeyCode.Z, KeyCode.X, KeyCode.Space, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Tab, KeyCode.B, KeyCode.PageUp, KeyCode.PageDown }) if (Input.GetKeyDown(key)) HandleKey(key);
                    if (Input.GetMouseButtonDown(0) && !IsMoving && !hud.PointerOverUi) PickMouse();
                    if (Input.GetMouseButtonDown(1) && !IsMoving) BeginMousePlacement(Input.mousePosition,hud.PointerOverUi);
                    if (!IsMoving && Mathf.Abs(Input.mouseScrollDelta.y) > .1f && !hud.PointerOverUi) cameraSize = Mathf.Clamp(cameraSize - Input.mouseScrollDelta.y * .35f, 4, 10);
                }
            }
            if (!playable || WorkActive || InventoryOpen) SetMoveAnimation(0, dt);
            if (hero) hero.gameObject.SetActive(playable || Model.Phase == PanicPhase.Title || LossAnnounced || CinematicShowsActor);
            ActiveFloor = Mathf.Clamp(Mathf.FloorToInt((player.position.y + .65f) / 3.2f), -1, 1);
            inside = InResidence(player.position);
            UpdateResidenceLife(dt);
            AnimateHinges(dt); ApplyFloorVisibility(); SyncVisuals(false); FollowCamera(dt);
            gatherPoseTime = Mathf.Max(0, gatherPoseTime - dt);
        }

        void LateUpdate()
        {
            if (!hero || !animator) return;
            string task = WorkActive ? workAction : gatherPoseTime > 0 ? "gather" : IsMoving ? "carry" : "";
            PanicCharacterArt.SetTaskPose(hero.gameObject, animator, task, WorkActive ? Expedition.Work.Progress : 0);
            PanicCharacterArt.UpdateSecondary(hero.gameObject, desiredVelocity.magnitude, Time.deltaTime);
        }

        Vector2 ReadMove() { return new Vector2((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0), (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0)); }
        static Vector3 Flat(Vector3 p) { return new Vector3(p.x, 0, p.z); }
        void MovePlayer(float dt, Vector2 input)
        {
            Vector3 forward = Flat(View.transform.forward).normalized, right = Flat(View.transform.right).normalized;
            float speed = Input.GetKey(KeyCode.LeftShift) ? 4.2f : 2.7f;
            Vector3 target = Vector3.ClampMagnitude(right * input.x + forward * input.y, 1) * speed;
            desiredVelocity = Vector3.SmoothDamp(desiredVelocity, target, ref smoothVelocity, .075f, 20, dt);
            if (controller.isGrounded && gravitySpeed < 0) gravitySpeed = -2;
            gravitySpeed = Mathf.Max(-20, gravitySpeed - 14 * dt);
            // Hinged leaves are moved by Transform, with autoSyncTransforms disabled.
            // Query the same current door poses in normal play as in overlap checks.
            Physics.SyncTransforms();
            controller.Move((desiredVelocity + Vector3.up * gravitySpeed) * dt);
            Vector3 p = player.position;
            if (Mathf.Abs(p.x) > 21.3f || Mathf.Abs(p.z) > 21.3f) { p.x = Mathf.Clamp(p.x, -21.3f, 21.3f); p.z = Mathf.Clamp(p.z, -21.3f, 21.3f); Warp(p); }
            if (desiredVelocity.sqrMagnitude > .025f)
            {
                float targetYaw = Mathf.Atan2(desiredVelocity.x, desiredVelocity.z) * Mathf.Rad2Deg;
                facingYaw = Mathf.SmoothDampAngle(facingYaw, targetYaw, ref yawVelocity, .09f);
                hero.rotation = Quaternion.Euler(0, facingYaw, 0);
                footstepClock += dt * Mathf.Max(.5f, desiredVelocity.magnitude / 2.7f);
                if (footstepClock > .39f && controller.isGrounded) { footstepClock = 0; effects.Play("step", inside ? .3f : .2f); }
            }
            SetMoveAnimation(desiredVelocity.magnitude / 4.2f, dt);
            if (animator) animator.SetBool("Airborne", !controller.isGrounded);
        }
        void SetMoveAnimation(float speed, float dt) { if (animator) animator.SetFloat("Speed", speed, .12f, dt); }

        public void HandleKey(KeyCode key)
        {
            if(key==KeyCode.F2){if(hud)hud.ToggleDeveloperMenu();return;}
            if(hud&&(hud.DeveloperMenuOpen||hud.DeveloperInputBlockedThisFrame))return;
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

        void RefreshFocus(bool cycle)
        {
            focusCandidates.Clear();
            foreach (var item in Model.Items)
                if (item.Installed && PositionOf(item.Id, out var target) && (item.Kind == ItemKind.Roof || SameLevel(item.Position)) && Vector3.Distance(target, player.position) < (item.Kind == ItemKind.Wall ? 3.4f : 2.7f)) focusCandidates.Add(item.Id);
            foreach (var s in Expedition.Sources) if (SameLevel(s.Position) && Vector3.Distance(s.Position, player.position) < 2.7f && !focusCandidates.Contains(s.Id)) focusCandidates.Add(s.Id);
            foreach (var site in gardenSites) if (!site.done && Vector3.Distance(site.p, player.position) < 2.5f) focusCandidates.Add(site.id);
            foreach(var pair in residenceTargets) if(pair.Value && SameLevel(ResidenceFocusPosition(pair.Value)) && Vector3.Distance(ResidenceFocusPosition(pair.Value),player.position)<2.5f) focusCandidates.Add(pair.Key);
            if (!items.ContainsKey("bunker")&&Vector3.Distance(player.position, BunkerPosition) < 2) focusCandidates.Add("@bunker");
            focusCandidates.RemoveAll(id=>!PositionOf(id,out var p)||!InteractionPathClear(id,p));
            focusCandidates.Sort((a, b) => FocusScore(a).CompareTo(FocusScore(b)));
            if (cycle && focusCandidates.Count > 0) SetFocus(focusCandidates[(focusCandidates.IndexOf(focusId) + 1) % focusCandidates.Count]);
            else if (!focusCandidates.Contains(focusId)) SetFocus(focusCandidates.Count > 0 ? focusCandidates[0] : null);
            UpdateContext();
        }
        bool SameLevel(Vector3 p) { return Mathf.Abs(p.y - (ActiveFloor * 3.2f)) < .9f; }
        float DistanceTo(string id) { return PositionOf(id, out Vector3 p) ? Vector3.Distance(p, player.position) : 999; }
        float FocusScore(string id){var data=Model.Items.Find(i=>i.Id==id);return DistanceTo(id)+(data!=null&&data.Kind==ItemKind.Roof?3:data!=null&&data.Kind==ItemKind.Wall?2:0)-(Expedition.Sources.Exists(s=>s.Id==id)?.25f:0);}
        bool InteractionPathClear(string id,Vector3 p)
        {
            Vector3 start=player.position+Vector3.up*.9f,end=p+Vector3.up*.8f;Vector3 delta=end-start;float distance=delta.magnitude;if(distance<.05f)return true;
            var hits=Physics.RaycastAll(start,delta/distance,distance,~(1<<30),QueryTriggerInteraction.Ignore);Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)
            {
                if(hit.collider==controller||hit.collider.GetComponentInParent<ResidenceLootPick>()||hit.transform.IsChildOf(player))continue;
                if(id=="@computer"&&residenceTargets.TryGetValue(id,out var device)&&device&&hit.transform==device.transform.parent)continue;
                var pick=hit.collider.GetComponentInParent<HomePick>();
                if(pick&&(pick.Id==id||id=="@bunker"&&pick.Id=="bunker"))return true;
                return false;
            }
            return true;
        }
        bool FocusPosition(out Vector3 p) { return PositionOf(focusId, out p); }
        bool PositionOf(string id, out Vector3 p)
        {
            p = Vector3.zero; if (id == null) return false;
            if (id == "@bunker") { p = BunkerPosition; return true; }
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
            UpdateResidenceContext();
        }

        public void Interact()
        {
            if (!CanInteract || WorkActive || IsMoving) { Model.Notify("Etkileşmek için hedefe yaklaş."); return; }
            if (focusId == "@bunker" || Selected != null && Selected.Id == "bunker") { Model.EnterBunker(); return; }
            var source = NearestSource;
            if (source != null)
            {
                ToggleContainer(); return;
            }
            if(focusId=="@radio") { ToggleRadio(); return; }
            if(focusId=="@fireplace") { ToggleHearth(); return; }
            if(focusId=="@computer") { ToggleComputer(); return; }
            if(focusId=="@garage") { garageOpen=!garageOpen; UpdateContext(); return; }
            if(focusId!=null&&focusId.StartsWith("@")&&hinges.ContainsKey(focusId)){ToggleDoor();UpdateContext();return;}
            var site = gardenSites.Find(s => s.id == focusId);
            if (site != null) { BeginWork(site.action, site.id); return; }
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
                hinge.open = !hinge.open; hinge.closeBlocked=hinge.motionBlocked=false; gatherPoseTime = .55f; effects.Play("bunker", .3f);
            }
        }
        public void ToggleAnchorSurface() { AnchorMode = AnchorMode == "wall" ? "floor" : "wall"; UpdateContext(); }
        public void QuickAction(int index)
        {
            if (!CanInteract || Model.Phase == PanicPhase.Inspection) { Model.Notify("Hazırlık sırasında nesneye yaklaş."); return; }
            var item = Selected; if (item == null) return;
            if (index == 1)
            {
                if (item.Anchored) { Model.ToggleAnchor(item.Id); return; }
                if (!item.Movable) { Model.Notify("Bu yapı sabitlenmez; 3 ile güçlendirilir."); return; }
                if (AnchorMode == "wall" && !NearWall(item.Position)) { Model.Notify("Duvara sabitlemek için eşyayı duvara yaklaştır veya T ile zemini seç."); return; }
                BeginWork(AnchorMode == "wall" ? "anchorwall" : "anchorfloor", item.Id);
            }
            else if (index == 2) BeginWork(item.Kind == ItemKind.Window || item.Kind == ItemKind.Door ? "flood" : "fire", item.Id);
            else if (index == 3) BeginWork("upgrade", item.Id);
            else if (index == 4) BeginMove();
        }
        bool NearWall(Vector3 p) { return Mathf.Abs(p.x) > 6.3f || Mathf.Abs(p.z) > 4.5f || Mathf.Abs(p.x - 2) < 1.1f; }

        public void BeginWork(string action, string target)
        {
            if (Model.Phase != PanicPhase.Preparing && Model.Phase != PanicPhase.Tutorial) return;
            if (WorkActive || IsMoving) return;
            if (!Expedition.CanBeginWork(action,target)) return;
            workAction = action; workTarget = target; workAge = 0;
            Expedition.Work.Begin(action, target); InventoryOpen = false;
            if (PositionOf(target, out var taskPosition)) { var toward = Flat(taskPosition-player.position); if (toward.sqrMagnitude>.01f) { facingYaw=Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg; hero.rotation=Quaternion.Euler(0,facingYaw,0); } }
            animator.SetBool("Working", true); animator.SetBool("Kneeling", action == "plant" || action == "drain" || action == "anchorfloor");
            var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (hand) { handTool = PanicCharacterArt.CreateHandTool(action); handTool.transform.SetParent(hand,false); handTool.transform.localRotation=Quaternion.Euler(0,180,0); }
            workRig = PanicWorkArt.CreateWorkRig(action); workRig.transform.position = new Vector3(100, 100, 100); LayerRecursive(workRig, 30); workCamera.enabled = true;
            Model.Notify("İşi tamamla");
        }
        void UpdateWork(float dt)
        {
            float dir = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            StepWork(dt,dir,Input.GetKey(KeyCode.Space),Input.GetKeyDown(KeyCode.Return),Input.GetKeyDown(KeyCode.Space));
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape)) { CancelWork(); return; }
        }
        public void StepWork(float dt,float direction,bool held,bool pressed,bool pulse)
        {
            if(!WorkActive)return;workAge+=dt;Expedition.Work.Tick(dt,direction,held,pressed,pulse);
            if(workRig)PanicWorkArt.UpdateWorkRig(workRig,Expedition.Work.Progress,Expedition.Work.CompletedSteps,workAge,Expedition.Work.Cursor,Expedition.Work.IsApplying,Expedition.Work.ToolMotion);
            if(Expedition.Work.Complete)CompleteWork();
        }
        public void CompleteWork()
        {
            if (!WorkActive || !Expedition.Work.Complete) return;
            bool done = Expedition.TryCompleteWork(workAction, workTarget);
            if (done)
            {
                var site = gardenSites.Find(s => s.id == workTarget);
                if (site != null)
                {
                    site.done = true; site.marker.SetActive(false); site.built = CreateVisual(site.action == "plant" ? "tree" : site.action); site.built.transform.position = site.p;
                    if(site.action=="plant") { var c=site.built.AddComponent<CapsuleCollider>(); c.center=new Vector3(0,.8f,0); c.radius=.16f; c.height=1.6f; }
                    else if(site.action=="fence"||site.action=="hose") AddBoundsCollider(site.built);
                }
                effects.Play("build", .6f);
            }
            CancelWork(); SyncVisuals(true);
        }
        public void CancelWork()
        {
            if (Expedition == null) return;
            if (WorkActive) CancelFrame=Time.frameCount;
            Expedition.Work.Cancel();
            if (animator) { animator.SetBool("Working", false); animator.SetBool("Kneeling", false); }
            if (workRig) Destroy(workRig); workRig = null;
            if (handTool) Destroy(handTool); handTool=null;
            if (workCamera) workCamera.enabled = false;
            workAction = null; workTarget = null;
        }

        public void ToggleInventory() { if (WorkActive || IsMoving) return; InventoryOpen = !InventoryOpen; }
        void UpdateInventoryKeys()
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Backspace)) { InventoryOpen = false; return; }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) InventoryMove(1);
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) InventoryMove(-1);
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) InventoryMove(4);
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) InventoryMove(-4);
            if (Input.GetKeyDown(KeyCode.Return)) InventoryUse();
            if (Input.GetKeyDown(KeyCode.F)) CraftFirstAid();
        }
        public void InventoryMove(int delta) { InventoryIndex = (InventoryIndex + delta + 12) % 12; }
        public void InventorySelect(int index) { InventoryIndex = Mathf.Clamp(index, 0, 11); }
        public void InventoryUse() { if (Expedition.PackSlot(InventoryIndex)) effects.Play("buy", .3f); }
        public void CraftFirstAid() { if (Expedition.CraftFirstAid()) effects.Play("build", .4f); }

        public void StartGame(bool tutorial)
        {
            CancelWork(); CancelMove();
            if (Model.Phase != PanicPhase.Title && Model.Phase != PanicPhase.GameOver) Model.ReturnToTitle();
            Model.StartRun(tutorial); ConfigureResidenceModel(); Expedition.Reset(); ResetResidenceLife(); previousWave = Model.Wave; previousPhase = Model.Phase; focusId = null; InventoryOpen = false; ClearGarden(); Warp(new Vector3(0, .05f, -7.5f)); SyncVisuals(true); effects.OnPhase(Model.Phase); QueueRadio(DisasterVoiceId());
        }
        public void FinishTutorial() { CancelWork(); Model.FinishTutorial(); ConfigureResidenceModel(); Expedition.Reset(); ResetResidenceLife(); previousWave = Model.Wave; previousPhase = Model.Phase; ClearGarden(); Warp(new Vector3(0, .05f, -7.5f)); SyncVisuals(true); QueueRadio(DisasterVoiceId()); }
        public void FinishInspection() { Model.FinishInspection(); }
        public void ReturnToMenu() { CancelWork(); CancelMove(); Model.ReturnToTitle(); ConfigureResidenceModel(); Expedition.Reset(); ResetResidenceLife(); InventoryOpen = false; focusId = null; SyncVisuals(true); }
        void ClearGarden() { foreach (var s in gardenSites) { s.done = false; s.marker.SetActive(true); if (s.built) Destroy(s.built); s.built = null; } }
        public void Warp(Vector3 p) { controller.enabled = false; player.position = p; controller.enabled = true; gravitySpeed = -2; desiredVelocity = Vector3.zero; }
        public void SmokeMove(Vector2 direction, float dt) { MovePlayer(dt, direction); }
        public void SmokeFocus(string id) { SetFocus(id); }

        void PickMouse()
        {
            var hits = Physics.RaycastAll(View.ScreenPointToRay(Input.mousePosition), 150); Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits) { if(!PlacementPickVisible(hit.collider))continue; var loot=hit.collider.GetComponentInParent<ResidenceLootPick>(); if(loot) { SelectLoot(loot.Index); TakeLoot(); return; } var p = hit.collider.GetComponentInParent<HomePick>(); if (p) { SetFocus(p.Id); return; } }
        }
        void BindHinge(GameObject root, string id, bool isDoor)
        {
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
            for (int i = 0; i < floorVisuals.Count; i++)
            {
                int floor=i-1; bool shown = !inside || floor <= ActiveFloor; floorVisuals[i].SetActive(shown);
                foreach (var t in floorVisuals[i].GetComponentsInChildren<Transform>(true))
                    if (t.name == "FrontHighWalls") t.gameObject.SetActive(!inside);
            }
            if (roof) roof.SetActive(!inside && Model.Phase != PanicPhase.Disaster && Model.Phase != PanicPhase.Aftermath);
            UpdateResidenceVisibility();
            foreach (var go in floorItems)
            {
                if (!go || go.name == movingId) continue;
                bool shown = !inside || go.transform.position.y < ActiveFloor * 3.2f + 2.6f;
                var data = Model.Items.Find(x => x.Id == go.name);
                go.SetActive(shown && (data == null || data.Installed));
            }
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
        }
        void RebuildHardware(PanicItem item, GameObject go)
        {
            if (hardware.TryGetValue(item.Id, out var old) && old) Destroy(old);
            var holder = new GameObject("Applied measures"); holder.transform.SetParent(go.transform, false); hardware[item.Id] = holder;
            if (item.Anchored)
            {
                GameObject attachment = item.AnchorSurface == "wall" ? PanicWorkArt.CreateWallAnchors(item.Size) : PanicAttachmentArt.CreateAnchors(item.Size);
                attachment.transform.SetParent(holder.transform, false);
            }
            if (item.FloodProtected) { var g = PanicAttachmentArt.CreateFloodGuard(1.82f, .85f); g.transform.SetParent(holder.transform, false); g.transform.localPosition = new Vector3(0, .8f, -.22f); }
            if (item.FireProtected) { var g = CreateVisual("fireunit"); g.transform.SetParent(holder.transform, false); g.transform.localPosition = new Vector3(item.Size.x * .35f, .1f, -.35f); }
            if ((item.Kind == ItemKind.Wall || item.Kind == ItemKind.Roof) && item.Tier > 0)
            {
                var g = PanicWorkArt.CreateReinforcement(item.Tier); g.transform.SetParent(holder.transform, false); g.transform.localPosition = new Vector3(0, .7f, -.15f);
                if (Mathf.Abs(item.Position.x) > 7) holder.transform.localRotation = Quaternion.Euler(0, item.Position.x < 0 ? 270 : 90, 0);
            }
            if (item.Damage > 4)
            {
                float width = Mathf.Min(1.2f, item.Size.x * .7f), height = Mathf.Min(1.6f, item.Size.y * .8f);
                var crack = MakeLine("Fresh damage fracture", new[] { new Vector3(-width*.4f,height*.85f,-.01f),new Vector3(-width*.1f,height*.62f,-.02f),new Vector3(-width*.2f,height*.42f,-.01f),new Vector3(width*.18f,height*.25f,-.01f),new Vector3(width*.12f,.09f,-.01f) }, new Color(.09f,.075f,.065f), .018f);
                crack.loop = false; crack.transform.SetParent(holder.transform, false); crack.transform.localPosition = new Vector3(0,.1f,-item.Size.z*.51f);
                if (item.Kind == ItemKind.Window) crack.transform.localPosition = new Vector3(0,1.0f,-.18f);
                if (item.Kind == ItemKind.Wall) { crack.transform.localPosition = new Vector3(0,.1f,-.14f); if (Mathf.Abs(item.Position.x)>7) holder.transform.localRotation=Quaternion.Euler(0,item.Position.x<0?270:90,0); }
            }
        }

        void FollowCamera(float dt)
        {
            if (ApplyResidenceCinematicCamera(dt)) return;
            View.orthographic=true;
            bool title = Model.Phase == PanicPhase.Title;
            Vector3 aim = title ? new Vector3(0, 1.8f, -1) : player.position + Vector3.up * .85f;
            float size = title ? 12.5f : cameraSize;
            if (Model.Phase == PanicPhase.Disaster || Model.Phase == PanicPhase.Aftermath) { aim = new Vector3(0, 1.5f, 0); size = 13; }
            Vector3 offset = cameraOffset;
            if (Model.Phase == PanicPhase.Disaster) { float t = Mathf.Clamp01(1-Model.PhaseTime/8f); offset=Quaternion.Euler(0,Mathf.Lerp(-5,7,t),0)*offset; size=Mathf.Lerp(13,11.5f,t); }
            Vector3 shake = Model.Phase == PanicPhase.Disaster && Model.Disaster == DisasterType.Earthquake ? new Vector3(Mathf.Sin(Time.time * 29), Mathf.Cos(Time.time * 31), 0) * .06f : Vector3.zero;
            View.transform.position = Vector3.Lerp(View.transform.position, aim + offset + shake, dt * 5);
            View.transform.rotation = Quaternion.LookRotation(-offset.normalized);
            View.orthographicSize = Mathf.Lerp(View.orthographicSize, size, dt * 4);
        }
        void OnGUI()
        {
            if (!View || Model.Phase == PanicPhase.Title || WorkActive || InventoryOpen || CinematicActive || (hud&&hud.DeveloperMenuOpen)) return;
            if (markerStyle == null) { markerStyle = new GUIStyle(GUI.skin.box); markerStyle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); markerStyle.fontSize = 13; markerStyle.alignment = TextAnchor.MiddleCenter; markerStyle.normal.textColor = new Color(.94f, .88f, .71f); }
            if (!string.IsNullOrEmpty(ShortAction)&&FocusPosition(out var p) && CanInteract) DrawMarker(p + Vector3.up * 1.9f, "E  " + ShortAction, 110);
            if (Model.Phase == PanicPhase.Inspection)
            {
                foreach (var item in Model.Items.Where(i => i.Contribution > .1f && SameLevel(i.Position)).OrderByDescending(i => i.Contribution).Take(8))
                    DrawMarker(item.Position + Vector3.up * Mathf.Max(.9f, item.Size.y), "!  +" + item.Contribution.ToString("F1") + "%", 90);
            }
        }
        void DrawMarker(Vector3 p, string label, float width) { var s = View.WorldToScreenPoint(p); if (s.z > 0) GUI.Label(new Rect(s.x - width / 2, Screen.height - s.y, width, 26), label, markerStyle); }
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

