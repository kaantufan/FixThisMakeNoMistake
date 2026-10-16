using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicPick : MonoBehaviour { public string Id; }

    public sealed class PanicWorld : MonoBehaviour
    {
        public PanicModel Model { get; private set; }
        public PanicItem Selected { get { return Model == null ? null : Model.Items.Find(x => x.Id == selectedId); } }
        public bool IsMoving { get { return moving; } }
        public string PlacementMessage { get; private set; }
        public bool CanInteract { get { return Selected != null && NearItem(Selected); } }
        public string InteractionHint { get; private set; }
        public Camera View { get; private set; }
        public Vector3 PlayerPosition { get { return player == null ? Vector3.zero : player.position; } }
        public bool Busy { get { return pending != null; } }
        public bool InputLocked;
        public static readonly Vector3 BunkerPosition = new Vector3(3.6f, 0, -2.7f);
        public static readonly Vector3 StationPosition = new Vector3(3.4f, 0, 2.5f);

        readonly Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
        readonly Dictionary<string, string> visualState = new Dictionary<string, string>();
        readonly Dictionary<string, GameObject> anchorBadges = new Dictionary<string, GameObject>();
        readonly Dictionary<string, GameObject> protectionBadges = new Dictionary<string, GameObject>();
        string selectedId;
        Transform player, visualPlayer, content, ghost;
        CharacterController controller;
        Animator animator;
        PanicHud hud;
        PanicEffects effects;
        PanicPhase previousPhase;
        bool moving, placementValid;
        Vector3 moveCandidate;
        float moveRotation, moveOriginalRotation;
        Vector3 moveOriginalPosition;
        Action pending;
        float pendingRemaining, pendingDuration;
        string pendingId, pendingLabel;
        Vector3 cameraBase;
        Quaternion cameraRotation;
        float cameraSize = 7.3f;
        float stepClock;
        GUIStyle worldLabel, worldSmall;
        Material ringMaterial;
        LineRenderer playerRing, moveRing;
        string evidencePath;

        void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 4;
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "--panic-smoke") >= 0) Application.runInBackground = true;
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "--panic-evidence") evidencePath = args[i + 1];
            if (!string.IsNullOrEmpty(evidencePath))
            {
                System.IO.Directory.CreateDirectory(evidencePath);
                PanicModel.ScorePathOverride = System.IO.Path.Combine(evidencePath, "isolated-highscores.json");
            }
            Model = new PanicModel();
            BuildRoom();
            hud = gameObject.AddComponent<PanicHud>();
            hud.World = this;
            effects = gameObject.AddComponent<PanicEffects>();
            effects.World = this;
            previousPhase = Model.Phase;
            if (Array.IndexOf(args, "--panic-smoke") >= 0)
                gameObject.AddComponent<PanicSmoke>();
            Debug.Log("PANIC_READY product=Too Late to Panic! version=0.1.0 humansGenerated=0 offline=true");
        }

        GameObject Art(string id)
        {
            var asset = Resources.Load<GameObject>("Art/" + id);
            GameObject go = asset != null ? Instantiate(asset) : PanicArt.Create(id);
            go.name = id;
            PanicArt.Register(go, id);
            return go;
        }

        void BuildRoom()
        {
            content = new GameObject("Furnished room").transform;
            var room = Resources.Load<GameObject>("Art/room");
            GameObject shell = room != null ? Instantiate(room) : PanicArt.CreateRoom();
            shell.name = "Authored room shell";
            shell.transform.SetParent(content, false);
            var floor = new GameObject("Walkable floor");
            var fc = floor.AddComponent<BoxCollider>();
            fc.size = new Vector3(10.3f, .25f, 8.3f);
            fc.center = new Vector3(0, -.125f, 0);
            foreach (var item in Model.Items)
            {
                GameObject go = Art(item.ArtId);
                go.name = item.Id;
                go.transform.SetParent(content, false);
                go.transform.position = item.Position;
                go.transform.rotation = Quaternion.Euler(0, item.Rotation, 0);
                if (item.Kind == ItemKind.Wall)
                {
                    float length = Mathf.Max(item.Size.x, item.Size.z);
                    bool front = item.Id.Contains("south") || item.Id.Contains("east");
                    go.transform.localScale = new Vector3(length / 10f, front ? .15f : 1f, 1f);
                }
                var pick = go.AddComponent<PanicPick>();
                pick.Id = item.Id == "bunker" ? "@bunker" : item.Id == "bagstation" ? "@bag" : item.Id;
                var collider = go.AddComponent<BoxCollider>();
                Vector3 localSize = LocalSize(item);
                if (item.Kind == ItemKind.Wall)
                {
                    // Art wall is authored along local X. Transform handles cutaway height.
                    collider.size = new Vector3(10f, 2.8f, .23f);
                    collider.center = new Vector3(0, 1.4f, 0);
                }
                else if (item.Kind == ItemKind.Roof)
                {
                    collider.size = new Vector3(2, .4f, .6f);
                    collider.center = new Vector3(0, .25f, 0);
                }
                else if (item.Kind == ItemKind.Window)
                {
                    collider.size = new Vector3(2.1f, 1.65f, .5f);
                    collider.center = new Vector3(0, 1.7f, -.12f);
                }
                else
                {
                    collider.size = localSize;
                    collider.center = new Vector3(0, localSize.y * .5f, 0);
                }
                objects[item.Id] = go;
                visualState[item.Id] = "";
            }
            if (!objects.ContainsKey("bunker"))
            {
                var bunker = Art("bunker"); bunker.transform.position = BunkerPosition; bunker.transform.SetParent(content, true);
                bunker.AddComponent<PanicPick>().Id = "@bunker";
                var bc = bunker.AddComponent<BoxCollider>(); bc.center = new Vector3(0, .12f, 0); bc.size = new Vector3(1.86f, .3f, 1.66f);
            }
            if (!objects.ContainsKey("bagstation"))
            {
                var station = Art("bagstation"); station.transform.SetParent(content, false); station.transform.position = StationPosition;
                station.AddComponent<PanicPick>().Id = "@bag";
                var sc = station.AddComponent<BoxCollider>(); sc.center = new Vector3(0, .7f, 0); sc.size = new Vector3(1.74f, 1.4f, .76f);
            }

            var cameraObject = new GameObject("Isometric camera");
            View = cameraObject.AddComponent<Camera>();
            View.orthographic = true;
            View.orthographicSize = cameraSize;
            View.nearClipPlane = .1f;
            View.farClipPlane = 85;
            View.backgroundColor = new Color(.064f, .105f, .13f);
            View.clearFlags = CameraClearFlags.SolidColor;
            View.allowHDR = false;
            cameraBase = new Vector3(12, 14, -16);
            View.transform.position = cameraBase;
            View.transform.LookAt(new Vector3(0, .4f, 0));
            cameraRotation = View.transform.rotation;
            View.gameObject.AddComponent<AudioListener>();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.76f, .82f, .84f);
            RenderSettings.ambientEquatorColor = new Color(.48f, .56f, .57f);
            RenderSettings.ambientGroundColor = new Color(.22f, .24f, .24f);
            var key = new GameObject("Warm afternoon key").AddComponent<Light>();
            key.type = LightType.Directional;
            key.transform.rotation = Quaternion.Euler(48, -38, 0);
            key.color = new Color(1, .89f, .73f);
            key.intensity = 1.25f;
            key.shadows = LightShadows.Soft;
            key.shadowBias = .025f;
            QualitySettings.shadowDistance = 45;
            QualitySettings.shadowResolution = ShadowResolution.High;
            var fill = new GameObject("Cool window fill").AddComponent<Light>();
            fill.type = LightType.Point; fill.color = new Color(.48f, .73f, .81f); fill.intensity = 1.2f; fill.range = 12; fill.transform.position = new Vector3(-3, 3, 2);
            var lamp = new GameObject("Interior warmth").AddComponent<Light>();
            lamp.type = LightType.Point; lamp.color = new Color(1, .7f, .35f); lamp.intensity = 1.0f; lamp.range = 8; lamp.transform.position = new Vector3(1, 2.6f, 0);

            var po = new GameObject("Existing licensed player (not newly generated)");
            player = po.transform;
            player.position = new Vector3(1.8f, .04f, .65f);
            controller = po.AddComponent<CharacterController>();
            controller.radius = .23f; controller.height = 1.65f; controller.center = new Vector3(0, .84f, 0);
            controller.stepOffset = .12f; controller.skinWidth = .03f;
            var playerPrefab = Resources.Load<GameObject>("Player/ExistingSurvivor");
            if (playerPrefab != null)
            {
                var p = Instantiate(playerPrefab, player);
                p.transform.localPosition = Vector3.zero;
                p.transform.localRotation = Quaternion.identity;
                visualPlayer = p.transform;
                animator = p.GetComponentInChildren<Animator>();
                if (animator != null) animator.applyRootMotion = false;
            }
            else Debug.LogError("Existing player prefab missing; no replacement humanoid generated.");
            playerRing = MakeRing("Player navigation ring", .34f, new Color(.98f, .72f, .23f), .035f);
            playerRing.transform.SetParent(player, false);
            playerRing.transform.localPosition = new Vector3(0, .035f, 0);
            SyncVisuals();
        }

        Vector3 LocalSize(PanicItem item)
        {
            // Data Size is the unrotated model footprint; all placement uses its 90-degree rotation.
            return new Vector3(Mathf.Max(.25f, item.Size.x), Mathf.Max(.25f, item.Size.y), Mathf.Max(.2f, item.Size.z));
        }

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, .1f);
            Model.Tick(dt);
            if (Model.Phase != previousPhase)
            {
                if (moving) CancelMove();
                pending = null;
                if (effects != null) effects.OnPhase(Model.Phase);
                previousPhase = Model.Phase;
                Debug.Log("PANIC_PHASE " + Model.Phase + " wave=" + Model.Wave + " damage=" + Model.TotalDamage.ToString("F1") + " budget=" + Model.Budget);
            }
            UpdateJob(dt);
            bool mayWalk = Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial || Model.Phase == PanicPhase.Inspection;
            if (!InputLocked && mayWalk)
            {
                MovePlayer(dt);
                if (moving) UpdatePlacement();
                else if (Input.GetMouseButtonDown(0) && (hud == null || !hud.PointerOverUi)) Pick();
                if (Input.GetKeyDown(KeyCode.Escape)) { if (moving) CancelMove(); else Select(null); }
                if (Input.GetKeyDown(KeyCode.E) && !moving && !Busy)
                {
                    if (Vector3.Distance(Flat(player.position), BunkerPosition) < 1.9f) Act("bunker");
                    else if (Vector3.Distance(Flat(player.position), StationPosition) < 2.6f) hud.ShowBag();
                    else if (Selected != null) Act(Selected.Kind == ItemKind.Furniture ? "anchor" : "upgrade");
                }
                float scroll = Input.mouseScrollDelta.y;
                if (Mathf.Abs(scroll) > .01f && (hud == null || !hud.PointerOverUi)) cameraSize = Mathf.Clamp(cameraSize - scroll * .28f, 6.1f, 8.4f);
            }
            else if (animator != null) animator.SetFloat("Speed", 0);
            if (visualPlayer != null) visualPlayer.gameObject.SetActive(mayWalk || Model.Phase == PanicPhase.Title);
            InteractionHint = moving ? PlacementMessage : Selected != null ? (CanInteract ? "E: işlem  •  Fare: nesne seç" : "İşlem için nesneye yaklaş") : "WASD: yürü  •  Fare: seç  •  E: kullan  •  Tekerlek: yakınlaştır";
            SyncVisuals();
            UpdateCamera(dt);
        }

        static Vector3 Flat(Vector3 v) { return new Vector3(v.x, 0, v.z); }

        void MovePlayer(float dt)
        {
            Vector3 forward = Flat(View.transform.forward).normalized;
            Vector3 right = Flat(View.transform.right).normalized;
            float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            float z = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            Vector3 direction = Vector3.ClampMagnitude(right * x + forward * z, 1);
            controller.Move((direction * 2.7f + Vector3.down * 3f) * dt);
            Vector3 p = player.position;
            p.x = Mathf.Clamp(p.x, -4.65f, 4.65f); p.z = Mathf.Clamp(p.z, -3.65f, 3.65f);
            if ((p - player.position).sqrMagnitude > .0001f) { controller.enabled = false; player.position = p; controller.enabled = true; }
            if (direction.sqrMagnitude > .01f)
            {
                if (visualPlayer != null) visualPlayer.rotation = Quaternion.Slerp(visualPlayer.rotation, Quaternion.LookRotation(direction), dt * 13);
                stepClock += dt;
                if (stepClock > .37f) { stepClock = 0; if (effects != null) effects.Play("step", .17f); }
            }
            if (animator != null) animator.SetFloat("Speed", direction.magnitude, .1f, dt);
        }

        void UpdateCamera(float dt)
        {
            float t = Time.time;
            Vector3 offset = Vector3.zero;
            Quaternion rot = cameraRotation;
            float targetSize = cameraSize;
            if (Model.Phase == PanicPhase.Disaster)
            {
                targetSize = 6.5f;
                if (Model.Disaster == DisasterType.Earthquake)
                    offset = new Vector3(Mathf.Sin(t * 27) * .055f, Mathf.Cos(t * 31) * .04f, 0);
            }
            if (Model.Phase == PanicPhase.Aftermath || Model.Phase == PanicPhase.Success) targetSize = 6.9f;
            View.transform.position = Vector3.Lerp(View.transform.position, cameraBase + offset, dt * 8);
            View.transform.rotation = rot;
            View.orthographicSize = Mathf.Lerp(View.orthographicSize, targetSize, dt * 4);
        }

        void Pick()
        {
            var hits = Physics.RaycastAll(View.ScreenPointToRay(Input.mousePosition), 100);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                var tag = hit.collider.GetComponentInParent<PanicPick>();
                if (tag == null) continue;
                if (tag.Id == "@bag") { hud.ShowBag(); Model.Notify("AFET ÇANTASI • Alışveriş ve paketleme için masaya yaklaş."); return; }
                if (tag.Id == "@bunker") { Act("bunker"); return; }
                Select(tag.Id); return;
            }
            Select(null);
        }

        public void Select(string id)
        {
            if (moving && id != selectedId) CancelMove();
            if (selectedId != null && objects.TryGetValue(selectedId, out var old)) PanicArt.SetSelected(old, false);
            selectedId = id;
            if (id != null && objects.TryGetValue(id, out var go)) PanicArt.SetSelected(go, true);
            if (Selected != null && effects != null) effects.Play("select", .15f);
        }

        bool NearItem(PanicItem item)
        {
            if (item.Kind == ItemKind.Roof) return true;
            if (objects.TryGetValue(item.Id, out var go))
            {
                var collider = go.GetComponent<Collider>();
                if (collider != null) return Vector3.Distance(Flat(player.position), Flat(collider.ClosestPoint(player.position))) < 1.8f;
            }
            return Vector3.Distance(Flat(player.position), Flat(item.Position)) < 2.3f;
        }

        public void Act(string action)
        {
            if (Busy) { Model.Notify("İşlem devam ediyor: " + pendingLabel); return; }
            if (moving) { Model.Notify("Önce yerleşimi bitir veya iptal et."); return; }
            var item = Selected;
            if (action == "bunker")
            {
                if (Vector3.Distance(Flat(player.position), BunkerPosition) > 1.9f) { Model.Notify("Bunkera yaklaş: odanın ön sağ köşesindeki sarı kapak."); return; }
                if (Model.EnterBunker() && effects != null) effects.Play("bunker", .7f);
                return;
            }
            bool bag = action == "water" || action == "food" || action.StartsWith("pack") || action.StartsWith("support");
            if (bag)
            {
                if (Vector3.Distance(Flat(player.position), StationPosition) > 2.6f) { Model.Notify("Çanta masasına yaklaş: odanın arka sağ köşesi."); return; }
                bool ok = false;
                if (action == "water") ok = Model.BuyWater();
                else if (action == "food") ok = Model.BuyFood();
                else if (action == "packwater") { StartJob("Su paketleniyor", 1, null, () => Model.PackWater()); return; }
                else if (action == "packfood") { StartJob("Gıda paketleniyor", 1, null, () => Model.PackFood()); return; }
                else if (action.StartsWith("packsupport")) ok = Model.PackSupport(int.Parse(action.Substring(11)));
                else if (action.StartsWith("support")) ok = Model.BuySupport(int.Parse(action.Substring(7)));
                if (ok && effects != null) effects.Play("buy", .35f);
                return;
            }
            if (item == null) { Model.Notify("Önce bir eşya veya yapı parçası seç."); return; }
            if (!NearItem(item)) { Model.Notify("İşlem için seçili nesneye yaklaş."); return; }
            string id = item.Id;
            if (action == "installwindow") StartJob("Pencere takılıyor", 3, id, () => Model.InstallWindow(id));
            else if (action == "upgrade") StartJob("Güçlendiriliyor", 3, id, () => Model.Upgrade(id));
            else if (action == "anchor") StartJob(item.Anchored ? "Sabitleme kaldırılıyor" : "Sabitleniyor", 2, id, () => Model.ToggleAnchor(id));
            else if (action == "flood") StartJob("Sel bariyeri takılıyor", 3, id, () => Model.AddFloodGuard(id));
            else if (action == "fire") StartJob("Yangın önlemi kuruluyor", 3, id, () => Model.AddFireProtection(id));
        }

        void StartJob(string label, float seconds, string id, Func<bool> action)
        {
            if (Model.Phase != PanicPhase.Preparing && Model.Phase != PanicPhase.Tutorial) { Model.Notify("Bu aşamada hazırlık işlemi yapılamaz."); return; }
            pendingId = id; pendingDuration = seconds; pendingRemaining = seconds; pendingLabel = label;
            pending = () => { bool ok = action(); if (ok && effects != null) effects.Play("build", .45f); };
            Model.Notify(label + "…");
        }

        void UpdateJob(float dt)
        {
            if (pending == null) return;
            var item = Model.Items.Find(x => x.Id == pendingId);
            if (item != null && !NearItem(item)) { pending = null; Model.Notify("Uzaklaştığın için işlem iptal edildi."); return; }
            if (pendingId == null && Vector3.Distance(Flat(player.position), StationPosition) > 2.6f) { pending = null; Model.Notify("Paketlemek için masanın yanında kal."); return; }
            pendingRemaining -= dt;
            if (pendingRemaining <= 0) { var callback = pending; pending = null; callback(); }
        }

        public void BeginMove()
        {
            var item = Selected;
            if (Busy || moving) return;
            if (item == null || !item.Movable) { Model.Notify("Bu nesne taşınamaz."); return; }
            if (Model.Phase != PanicPhase.Preparing && Model.Phase != PanicPhase.Tutorial) { Model.Notify("Taşıma hazırlık aşamasında yapılır."); return; }
            if (!NearItem(item)) { Model.Notify("Taşımak için nesneye yaklaş."); return; }
            if (item.Anchored) { Model.Notify("Önce sabitlemeyi kaldır."); return; }
            moving = true;
            moveOriginalPosition = item.Position; moveOriginalRotation = item.Rotation;
            moveCandidate = item.Position; moveRotation = item.Rotation;
            ghost = Art(item.ArtId).transform;
            ghost.name = "Placement preview";
            ghost.position = item.Position;
            foreach (var r in ghost.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = new Material(Shader.Find("Standard")); mat.color = new Color(.35f, .95f, .7f, .45f);
                    mat.SetFloat("_Mode", 3); mat.SetInt("_SrcBlend", 5); mat.SetInt("_DstBlend", 10); mat.SetInt("_ZWrite", 0); mat.EnableKeyword("_ALPHABLEND_ON"); mat.renderQueue = 3000; mats[i] = mat;
                }
                r.sharedMaterials = mats;
            }
            objects[item.Id].SetActive(false);
            moveRing = MakeFootprint(item, new Color(.2f, 1, .7f));
            Model.Notify("Fare: yerleştir  •  R: döndür  •  Sol tık: onay  •  Sağ tık/Esc: iptal");
        }

        void UpdatePlacement()
        {
            if (Selected == null) { CancelMove(); return; }
            if (Input.GetKeyDown(KeyCode.R)) moveRotation = Mathf.Repeat(moveRotation + 90, 360);
            Ray ray = View.ScreenPointToRay(Input.mousePosition);
            if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float distance))
            {
                Vector3 p = ray.GetPoint(distance);
                moveCandidate = new Vector3(Mathf.Round(p.x * 4) / 4, 0, Mathf.Round(p.z * 4) / 4);
            }
            ghost.position = moveCandidate; ghost.rotation = Quaternion.Euler(0, moveRotation, 0);
            placementValid = ValidatePlacement(Selected, moveCandidate, moveRotation, out string reason);
            PlacementMessage = placementValid ? "✓ Uygun konum • Sol tıkla bırak" : "✕ " + reason;
            if (moveRing != null) { moveRing.transform.position = moveCandidate + Vector3.up * .05f; moveRing.transform.rotation = Quaternion.Euler(0, moveRotation, 0); moveRing.startColor = moveRing.endColor = placementValid ? new Color(.2f, 1, .65f) : new Color(1, .27f, .16f); }
            if (Input.GetMouseButtonDown(1)) { CancelMove(); return; }
            if (Input.GetMouseButtonDown(0) && (hud == null || !hud.PointerOverUi))
            {
                if (!placementValid) { Model.Notify(PlacementMessage); return; }
                string id = Selected.Id;
                Model.MoveItem(id, moveCandidate, moveRotation);
                EndMove();
                if (effects != null) effects.Play("build", .35f);
            }
        }

        public bool ValidatePlacement(PanicItem item, Vector3 position, float rotation, out string reason)
        {
            Vector2 half = Footprint(item, rotation) * .5f;
            if (Mathf.Abs(position.x) + half.x > 4.7f || Mathf.Abs(position.z) + half.y > 3.7f) { reason = "Oda sınırının içinde kal."; return false; }
            if (Overlap(position, half, StationPosition, new Vector2(.9f, .6f))) { reason = "Çanta masasını açık bırak."; return false; }
            if (Overlap(position, half, BunkerPosition, new Vector2(.95f, .95f))) { reason = "Bunker girişini açık bırak."; return false; }
            if (Overlap(position, half, Flat(player.position), new Vector2(.25f, .25f))) { reason = "Oyuncunun üstüne yerleştiremezsin."; return false; }
            foreach (var other in Model.Items)
            {
                if (other.Id == item.Id || other.Kind != ItemKind.Furniture) continue;
                if (Overlap(position, half, other.Position, Footprint(other, other.Rotation) * .5f + Vector2.one * .06f)) { reason = other.Label + " ile çakışıyor."; return false; }
            }
            if (!RouteOpen(item, position, rotation)) { reason = "Bunkera ulaşan yolu açık bırak."; return false; }
            reason = "Uygun konum"; return true;
        }

        static bool Overlap(Vector3 a, Vector2 ah, Vector3 b, Vector2 bh)
        { return Mathf.Abs(a.x - b.x) < ah.x + bh.x && Mathf.Abs(a.z - b.z) < ah.y + bh.y; }
        static Vector2 Footprint(PanicItem item, float rot)
        { bool swap = Mathf.Abs(Mathf.RoundToInt(rot / 90)) % 2 == 1; return swap ? new Vector2(item.Size.z, item.Size.x) : new Vector2(item.Size.x, item.Size.z); }

        bool RouteOpen(PanicItem candidate, Vector3 position, float rotation)
        {
            const int width = 19, height = 15;
            Func<int, int, Vector3> cell = (x, z) => new Vector3(-4.5f + x * .5f, 0, -3.5f + z * .5f);
            var blocked = new bool[width, height];
            for (int x = 0; x < width; x++) for (int z = 0; z < height; z++)
            {
                Vector3 p = cell(x, z);
                foreach (var other in Model.Items)
                {
                    if (other.Kind != ItemKind.Furniture) continue;
                    Vector3 op = other.Id == candidate.Id ? position : other.Position;
                    float or = other.Id == candidate.Id ? rotation : other.Rotation;
                    if (Overlap(p, new Vector2(.22f, .22f), op, Footprint(other, or) * .5f)) { blocked[x, z] = true; break; }
                }
                if (Overlap(p, new Vector2(.2f, .2f), StationPosition, new Vector2(.75f, .45f))) blocked[x, z] = true;
            }
            int sx = Mathf.Clamp(Mathf.RoundToInt((player.position.x + 4.5f) * 2), 0, width - 1), sz = Mathf.Clamp(Mathf.RoundToInt((player.position.z + 3.5f) * 2), 0, height - 1);
            int tx = Mathf.Clamp(Mathf.RoundToInt((BunkerPosition.x + 4.5f) * 2), 0, width - 1), tz = Mathf.Clamp(Mathf.RoundToInt((BunkerPosition.z + 3.5f) * 2), 0, height - 1);
            var seen = new bool[width, height]; var q = new Queue<Vector2Int>(); q.Enqueue(new Vector2Int(sx, sz)); seen[sx, sz] = true;
            int[] dx = { 1, -1, 0, 0 }, dz = { 0, 0, 1, -1 };
            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (Vector3.Distance(cell(p.x, p.y), BunkerPosition) < 1.65f) return true;
                for (int k = 0; k < 4; k++) { int x = p.x + dx[k], z = p.y + dz[k]; if (x < 0 || z < 0 || x >= width || z >= height || blocked[x, z] || seen[x, z]) continue; seen[x, z] = true; q.Enqueue(new Vector2Int(x, z)); }
            }
            return false;
        }

        public void CancelMove()
        {
            if (!moving) return;
            EndMove();
            Model.Notify("Taşıma iptal edildi; önceki konum korundu.");
        }
        void EndMove()
        {
            moving = false;
            if (ghost != null) { foreach (var r in ghost.GetComponentsInChildren<Renderer>()) foreach (var mat in r.sharedMaterials) Destroy(mat); Destroy(ghost.gameObject); }
            if (moveRing != null) Destroy(moveRing.gameObject);
            if (selectedId != null && objects.TryGetValue(selectedId, out var go)) go.SetActive(true);
            PlacementMessage = "";
        }

        public void StartGame(bool tutorial)
        {
            CancelMove(); Select(null); Model.StartRun(tutorial); ResetPlayer();
            pending = null; previousPhase = Model.Phase;
            if (effects != null) effects.OnPhase(Model.Phase);
            foreach (var pair in visualState.Keys.ToArray()) visualState[pair] = "";
            SyncVisuals();
        }
        public void FinishTutorial() { Model.FinishTutorial(); ResetPlayer(); previousPhase = Model.Phase; if (effects != null) effects.OnPhase(Model.Phase); }
        public void FinishInspection() { Model.FinishInspection(); }
        public void ReturnToMenu() { CancelMove(); pending = null; Select(null); Model.ReturnToTitle(); }
        void ResetPlayer() { controller.enabled = false; player.position = new Vector3(1.8f, .04f, .65f); controller.enabled = true; }
        public void SmokePlacePlayer(Vector3 position) { controller.enabled = false; player.position = position + Vector3.up * .04f; controller.enabled = true; }

        void SyncVisuals()
        {
            foreach (var item in Model.Items)
            {
                if (!objects.TryGetValue(item.Id, out var go)) continue;
                go.transform.position = item.Position;
                go.transform.rotation = Quaternion.Euler(0, item.Rotation, 0);
                if (!moving || item.Id != selectedId) go.SetActive(item.Installed);
                string key = item.Tier + "|" + item.Damage.ToString("F1") + "|" + Model.Disaster + "|" + item.Anchored + "|" + item.FloodProtected + "|" + item.FireProtected + "|" + item.Installed;
                if (visualState[item.Id] == key) continue;
                visualState[item.Id] = key;
                PanicArt.SetTier(go, item.Tier);
                PanicArt.SetDamage(go, item.Damage, Model.Disaster);
                PanicArt.SetSelected(go, item.Id == selectedId);
                if (item.AnchorOwned && !anchorBadges.ContainsKey(item.Id))
                {
                    var anchor = PanicAttachmentArt.CreateAnchors(item.Size);
                    anchor.transform.SetParent(go.transform, false);
                    anchorBadges[item.Id] = anchor;
                }
                if (anchorBadges.TryGetValue(item.Id, out var badge)) badge.SetActive(item.Anchored);
                if (item.FloodProtected && !protectionBadges.ContainsKey(item.Id + "flood"))
                {
                    var guard = PanicAttachmentArt.CreateFloodGuard(item.Size.x + .12f, .75f); guard.name = "Installed flood panel";
                    guard.transform.SetParent(go.transform, false); guard.transform.localPosition = new Vector3(0, .04f, -.2f);
                    protectionBadges[item.Id + "flood"] = guard;
                }
                if (item.FireProtected && !protectionBadges.ContainsKey(item.Id + "fire"))
                {
                    var unit = Art("fireunit"); unit.transform.SetParent(go.transform, false); unit.transform.localPosition = new Vector3(Mathf.Min(item.Size.x * .4f, .7f), .12f, -.25f);
                    protectionBadges[item.Id + "fire"] = unit;
                }
                if (protectionBadges.TryGetValue(item.Id + "flood", out var f)) f.SetActive(item.FloodProtected);
                if (protectionBadges.TryGetValue(item.Id + "fire", out var fire)) fire.SetActive(item.FireProtected);
            }
        }

        LineRenderer MakeRing(string name, float radius, Color color, float thickness)
        {
            var go = new GameObject(name);
            var line = go.AddComponent<LineRenderer>();
            if (ringMaterial == null) ringMaterial = new Material(Shader.Find("Sprites/Default"));
            line.sharedMaterial = ringMaterial; line.useWorldSpace = false; line.loop = true; line.positionCount = 48; line.widthMultiplier = thickness; line.startColor = line.endColor = color;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; line.receiveShadows = false;
            for (int i = 0; i < 48; i++) { float a = i * Mathf.PI * 2 / 48; line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius)); }
            return line;
        }
        LineRenderer MakeFootprint(PanicItem item, Color color)
        {
            var line = MakeRing("Placement bounds", 1, color, .045f);
            line.positionCount = 4;
            Vector3 h = item.Size * .5f;
            line.SetPositions(new[] { new Vector3(-h.x, 0, -h.z), new Vector3(-h.x, 0, h.z), new Vector3(h.x, 0, h.z), new Vector3(h.x, 0, -h.z) });
            return line;
        }

        void OnGUI()
        {
            if (Model == null || View == null || Model.Phase == PanicPhase.Title || Model.Phase == PanicPhase.GameOver) return;
            if (worldLabel == null)
            {
                worldLabel = new GUIStyle(GUI.skin.box); worldLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); worldLabel.fontSize = 12; worldLabel.fontStyle = FontStyle.Bold; worldLabel.alignment = TextAnchor.MiddleCenter; worldLabel.normal.textColor = new Color(.95f, .89f, .72f);
                worldSmall = new GUIStyle(worldLabel); worldSmall.fontSize = 11;
            }
            if (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial || Model.Phase == PanicPhase.Inspection)
            {
                WorldLabel(BunkerPosition + Vector3.up * .25f, "BUNKER  [E]", 116, () => Act("bunker"));
                WorldLabel(StationPosition + Vector3.up * 1.4f, "AFET ÇANTASI", 130, () => hud.ShowBag());
                foreach (var item in Model.Items)
                {
                    if (!item.Installed && item.Kind == ItemKind.Window) WorldLabel(item.Position + Vector3.up * 1.7f, "+ PENCERE · 100", 135, () => Select(item.Id));
                    if (item.Kind == ItemKind.Roof) WorldLabel(item.Position + Vector3.up * .7f, "ÇATI · K" + item.Tier, 100, () => Select(item.Id));
                    if (Model.Phase == PanicPhase.Inspection && item.Contribution > .2f)
                        WorldLabel(item.Position + Vector3.up * Mathf.Max(.5f, item.Size.y), "! " + item.Label + "  +" + item.Contribution.ToString("F1") + "%", 170, () => Select(item.Id));
                }
            }
            if (pending != null)
            {
                float w = Mathf.Min(420, Screen.width * .4f), x = (Screen.width - w) * .5f, y = Screen.height - 90;
                GUI.Box(new Rect(x, y, w, 32), pendingLabel + "  " + Mathf.CeilToInt(pendingRemaining) + " sn", worldLabel);
                Color old = GUI.color; GUI.color = new Color(.24f, .8f, .66f); GUI.DrawTexture(new Rect(x, y + 33, w * (1 - pendingRemaining / pendingDuration), 4), Texture2D.whiteTexture); GUI.color = old;
            }
        }
        void WorldLabel(Vector3 position, string text, float width, Action clicked)
        {
            var p = View.WorldToScreenPoint(position); if (p.z < 0) return;
            Rect rect = new Rect(p.x - width * .5f, Screen.height - p.y, width, 25);
            if (GUI.Button(rect, text, worldLabel)) clicked();
        }
    }
}
