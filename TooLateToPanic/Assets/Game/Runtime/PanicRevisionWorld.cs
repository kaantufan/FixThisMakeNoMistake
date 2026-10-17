using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    // World objects and camera adapters share the production expedition. No duplicate inventory.
    public sealed partial class PanicHomeWorld
    {
        readonly Dictionary<string, GameObject> worldSupplyObjects = new Dictionary<string, GameObject>();
        readonly Dictionary<Transform, Vector3> partitionScales = new Dictionary<Transform, Vector3>();
        readonly Dictionary<Transform, Vector3> partitionPositions = new Dictionary<Transform, Vector3>();
        PanicNotebookOverlay revisionNotebook;
        Light revisionSun, shelterBeacon;
        float basementBlend;
        public bool NotebookBlocksInput { get { return revisionNotebook && revisionNotebook.BlocksInput; } }
        public PanicNotebookOverlay Notebook { get { return revisionNotebook; } }

        void InitRevisionWorld()
        {
            View.cullingMask &= ~(1 << 28);
            revisionNotebook = gameObject.AddComponent<PanicNotebookOverlay>();
            revisionNotebook.Bind(this, () => (hud && hud.NotebookModalBlocked) || BagCameraTransitionActive);
            InitBagPreparation();
            var marker = new GameObject("Basement shelter emergency beacon");
            marker.transform.position = BunkerPosition + new Vector3(0, 1.75f, .55f);
            shelterBeacon = marker.AddComponent<Light>(); shelterBeacon.type = LightType.Point;
            shelterBeacon.color = new Color(1f, .36f, .08f); shelterBeacon.range = 4; shelterBeacon.intensity = 0;
            UpdateWorldSupplies();
        }

        public void ProcessNotebookInput() { if (revisionNotebook) revisionNotebook.ProcessInput(); }

        void AddPhysicalBagStation()
        {
            // Reuse the model's bag station: one physical bag, one inventory, one interaction.
            if(!items.TryGetValue("bagstation",out var root)||!root)return;
            var pick=root.GetComponent<HomePick>();pick.Id="@bagstation";pick.Kind="bag";
            residenceTargets["@bagstation"]=root;
        }

        GameObject NormalizeFixtureRoot(GameObject root,string id)
        {
            if(!root||id!="table")return root;
            Vector3 fit=root.transform.localScale;
            foreach(Transform child in root.transform)
            {child.localPosition=Vector3.Scale(child.localPosition,fit);child.localScale=Vector3.Scale(child.localScale,fit);}
            root.transform.localScale=Vector3.one;
            return root;
        }

        void ConfigureRevisionSources()
        {
            if(Expedition==null)return;
            var pantry=Expedition.Sources.Find(s=>s.Id=="basement_pantry");
            if(pantry!=null)pantry.Position=new Vector3(-6.5f,-3.2f,3.8f);
        }

        void ResetRevisionWorld()
        {
            ConfigureRevisionSources();
            ResetBagPreparation();
            if (revisionNotebook) revisionNotebook.RequestClose();
            ClearPickupFocus();
            foreach (var go in worldSupplyObjects.Values) if (go) { go.SetActive(false); Destroy(go); }
            worldSupplyObjects.Clear();
        }

        void UpdateRevisionWorld(float dt)
        {
            UpdateWorldSupplies();
            UpdateBasementAtmosphere(dt);
            bool warn = Model.Phase == PanicPhase.Preparing && Model.TimeRemaining <= 60;
            if (shelterBeacon) shelterBeacon.intensity = warn ? .6f + Mathf.PingPong(residenceTime * 2f, 1.1f) : .08f;
        }

        public void RefreshPreparationVisibility() { UpdateWorldSupplies(); }

        void UpdateWorldSupplies()
        {
            foreach (var supply in Expedition.WorldSupplies)
            {
                AlignSupplySupport(supply);
                if (supply.Count <= 0)
                {
                    if (worldSupplyObjects.TryGetValue(supply.Id, out var gone) && gone) { gone.SetActive(false); Destroy(gone); }
                    worldSupplyObjects.Remove(supply.Id); continue;
                }
                if (!worldSupplyObjects.TryGetValue(supply.Id, out var root) || !root)
                {
                    root = new GameObject(supply.Id); root.transform.position = supply.Position;
                    var visual = CreatePreparedPickup(supply.ItemId, worldSupplyObjects.Count);
                    visual.transform.SetParent(root.transform, false); MakeCosmeticOnly(visual);
                    // A stable, bounded trigger owns picking; cosmetic gather motion cannot move it.
                    var pick = root.AddComponent<HomePick>(); pick.Id = supply.Id; pick.Kind = "supply";
                    var hit = root.AddComponent<BoxCollider>();
                    hit.center = new Vector3(0, .12f, 0); hit.size = new Vector3(.28f, .30f, .28f); hit.isTrigger = true;
                    worldSupplyObjects[supply.Id] = root;
                }
                root.transform.position = supply.Position;
                bool visible = BagFinalStageActive && (!inside || BagCameraOwnsView || supply.Floor == ActiveFloor);
                root.SetActive(visible);
            }
        }

        // Local seats come from the actual current art factories, not whole-prop AABB maxima.
        // Following the support transform also keeps uncollected supplies on moved furniture.
        void AlignSupplySupport(PanicWorldSupply supply)
        {
            string support=null;Vector3 local=Vector3.zero;
            switch(supply.Id)
            {
                case "supply_water_kitchen":support="cabinet";local=new Vector3(-.46f,1.007f,-.22f);break;
                case "supply_food_kitchen":support="cabinet";local=new Vector3(.28f,1.007f,-.22f);break;
                case "supply_flashlight_living":support="table";local=new Vector3(-.56f,1.054f,-.28f);break;
                case "supply_battery_living":support="table";local=new Vector3(.24f,1.054f,.34f);break;
                case "supply_firstaid_bath":support="decor_bath_firstaid_table";local=new Vector3(-.015f,.5715f,-.20f);break;
                case "supply_battery_upper":support="desk_upper";local=new Vector3(.60f,.809f,-.31f);break;
                case "supply_radio_upper":support="desk_upper";local=new Vector3(-1.30f,.025f,-.1f);break;
                case "supply_water_upper":support="desk_upper";local=new Vector3(-1.05f,.025f,.6f);break;
                case "supply_blanket_bed":support="bed";local=new Vector3(-.38f,.755f,-.55f);break;
                case "supply_food_upper":support="bed";local=new Vector3(.35f,.755f,-.65f);break;
                case "supply_water_basement":support="decor_basement_bench";local=new Vector3(-.40f,.809f,-.20f);break;
                case "supply_food_basement":support="decor_basement_bench";local=new Vector3(.38f,.809f,-.20f);break;
            }
            if(support==null)return;
            GameObject go;
            if(!items.TryGetValue(support,out go))go=floorItems.Find(g=>g&&g.name==support);
            if(go)supply.Position=go.transform.TransformPoint(local);
        }

        public bool TakeWorldSupply()
        {
            if(BagFirstPersonActive) RefreshBagRayFocus();
            var supply = Expedition.FindWorldSupply(focusId);
            if (supply == null || supply.Count <= 0) { Notice("Erzakları evdeki eşyalardan al. Malzemeler dolap açıldığında eklenir."); return false; }
            if (!BagFinalStageActive || !BagFirstPersonActive) { Notice("Çanta erzakları son 30 saniyede açılır."); return false; }
            if (Model.IsPaused || InputLocked || NotebookBlocksInput || WorkActive || IsMoving || InventoryOpen || !CanInteract || supply.Floor != ActiveFloor) return false;
            string id = supply.ItemId; int count = supply.Count; Vector3 start = supply.Position;
            if (!Expedition.TryCollectWorldSupply(supply.Id)) { Notice("Envanterde yer yok"); effects.Play("reject", .25f); return false; }
            ClearPickupFocus(); GatherItemId = id; GatherCount = count; GatherFeedback = .5f; gatherPoseTime = .48f;
            lastNotice = Model.Message; BeginPickupPresentation(id, start, 0);
            effects.Play(PanicAudioBank.Pickup(id), .35f); BeginBagHandCue();
            UpdateWorldSupplies(); UpdateContext(); return true;
        }

        static bool IsFullHeightPartition(string name)
        {
            return name == "solid_east_south" || name == "solid_east_north" || name == "solid_bath_west" ||
                name == "solid_bath_south_left" || name == "solid_bath_south_right";
        }

        static bool HasParentNamed(Transform child, string name, Transform stop)
        {
            for (var t = child; t && t != stop; t = t.parent) if (t.name == name) return true;
            return false;
        }

        void ApplyFacadeCutaway(GameObject root, Vector3 actor, float west, float east, float south, float north, float floorY)
        {
            Vector3 side = View.transform.position - actor;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!r || r.bounds.max.y <= floorY + .48f) continue;
                var p = r.bounds.center; bool near = false;
                near |= HasParentNamed(r.transform, "FacadeWest", root.transform) ? side.x < -.1f : p.x < west + .30f && side.x < -.1f;
                near |= HasParentNamed(r.transform, "FacadeEast", root.transform) ? side.x > .1f : p.x > east - .30f && side.x > .1f;
                near |= HasParentNamed(r.transform, "FacadeNorth", root.transform) ? side.z > .1f : p.z > north - .30f && side.z > .1f;
                near |= HasParentNamed(r.transform, "FacadeSouth", root.transform) ? side.z < -.1f : p.z < south + .30f && side.z < -.1f;
                if (near) r.enabled = false;
            }
        }

        void SetFirstPersonPartitions(bool tall)
        {
            foreach (var root in floorVisuals.Concat(new[] { residenceBathroom }))
            {
                if (!root) continue;
                foreach (var mesh in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (!IsFullHeightPartition(mesh.name) || !mesh.sharedMesh) continue;
                    if (!partitionScales.TryGetValue(mesh.transform, out var original)) partitionScales[mesh.transform] = original = mesh.transform.localScale;
                    if (!partitionPositions.TryGetValue(mesh.transform, out var position)) partitionPositions[mesh.transform] = position = mesh.transform.localPosition;
                    float height = mesh.sharedMesh.bounds.size.y;
                    mesh.transform.localPosition=position;mesh.transform.localScale=original;
                    if(tall && height>.1f)
                    {
                        var bounds=mesh.sharedMesh.bounds;
                        Vector3 bottom=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                        float before=mesh.transform.TransformPoint(bottom).y;
                        mesh.transform.localScale=new Vector3(original.x,original.y*3.2f/height,original.z);
                        float after=mesh.transform.TransformPoint(bottom).y;
                        mesh.transform.position+=Vector3.up*(before-after);
                    }
                }
            }
        }

        void UpdateBasementAtmosphere(float dt)
        {
            float target = ActiveFloor == -1 && inside && !ResidenceCinematicExteriorView ? 1f : 0f;
            basementBlend = Mathf.Lerp(basementBlend, target, 1f - Mathf.Exp(-dt * 3.5f));
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(.39f, .44f, .52f), new Color(.035f, .045f, .065f), basementBlend);
            RenderSettings.ambientEquatorColor = Color.Lerp(new Color(.26f, .29f, .34f), new Color(.025f, .029f, .04f), basementBlend);
            RenderSettings.ambientGroundColor = Color.Lerp(new Color(.12f, .135f, .16f), new Color(.018f, .02f, .028f), basementBlend);
            if (revisionSun) revisionSun.intensity = Mathf.Lerp(.75f, .035f, basementBlend);
            if (View) View.clearFlags = ResidenceCinematicExteriorView ? CameraClearFlags.Skybox :
                basementBlend > .2f ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
            if (View) View.backgroundColor = Color.Lerp(new Color(.09f, .13f, .17f), new Color(.009f, .012f, .02f), basementBlend);
        }

        void AnimateCuratedFixtures(float dt)
        {
            foreach (var pair in items)
            {
                if (!pair.Value) continue;
                var source = Expedition.Sources.Find(s => s.Id == pair.Key);
                var item = Model.Items.Find(i => i.Id == pair.Key);
                bool open = source != null ? source.Open : hinges.TryGetValue(pair.Key, out var hinge) && hinge.open;
                PanicCuratedAssetArt.SyncVisualState(pair.Value, open, item != null && (item.FloodProtected || item.FireProtected), item != null && item.Anchored, dt);
            }
            foreach (var pair in sourceObjects)
            {
                if (items.ContainsKey(pair.Key) || !pair.Value) continue;
                var source = Expedition.Sources.Find(s => s.Id == pair.Key);
                PanicCuratedAssetArt.SyncVisualState(pair.Value, source != null && source.Open, false, false, dt);
            }
            // The retained basement heater has no gameplay switch. Its imported running
            // dial is visual only and follows the same pause-owned world clock.
            foreach(var decor in floorItems)
                if(decor && decor.name=="decor_basement_heater")
                    PanicCuratedAssetArt.SyncVisualState(decor,false,false,false,dt);
            foreach (var pair in residenceTargets)
            {
                if (!pair.Value) continue;
                bool on = pair.Key == "@fireplace" ? hearthEnabled : pair.Key == "@radio" ? RadioEnabled : pair.Key == "@computer" && computerEnabled;
                PanicCuratedAssetArt.SyncVisualState(pair.Value, on, false, false, dt);
            }
        }
    }
}
