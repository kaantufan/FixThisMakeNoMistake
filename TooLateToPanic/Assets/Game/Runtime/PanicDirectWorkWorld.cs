using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        public const float MovementSpeedMultiplier = 1.35f;
        public const float WalkSpeed = 2.7f * MovementSpeedMultiplier;
        public const float RunSpeed = 4.2f * MovementSpeedMultiplier;
        GameObject fireStation, constructionPreview;
        bool fireStationReady;
        float constructionViewUntil;
        bool constructionOutdoor;
        Vector3 constructionViewAim, constructionViewOffset;
        public bool WorldConstructionVisible { get { return WorkActive && Expedition.Work.IsWorldConstruction; } }
        public bool HasConstructionPreview { get { return constructionPreview; } }
        public int InstalledTreeRows { get { return gardenSites.Count(s => s.action == "plant" && s.done); } }
        public float CharacterPlaybackRate { get { return animator ? animator.speed : 0; } }
        public bool TreeRowWorldBounds(string id, out Bounds bounds)
        {
            bounds = new Bounds(); var site = gardenSites.Find(value => value.id == id && value.action == "plant");
            if (site == null) return false;
            if (site.built)
            {
                bool any = false;
                foreach (var renderer in site.built.GetComponentsInChildren<Renderer>(true))
                { if (!any) { bounds = renderer.bounds; any = true; } else bounds.Encapsulate(renderer.bounds); }
                if (any) return true;
            }
            bounds = PanicConstructionArt.TreeRowBounds(TreeVariant(site)); bounds.center += site.p; return true;
        }
        public Vector3[] TreeRowWorldTrunks(string id)
        {
            var site = gardenSites.Find(value => value.id == id && value.action == "plant");
            if (site == null) return new Vector3[0];
            return PanicConstructionArt.TreeTrunkSeats(TreeVariant(site)).Select(seat => site.p + seat).ToArray();
        }
        public int InstalledMeasureRenderers(string id)
        { return hardware.TryGetValue(id, out var holder) && holder ? holder.GetComponentsInChildren<Renderer>(true).Length : 0; }

        void InitDirectActions() { InitUtilityStationsV414(); RefreshFireStation(true); }
        void ResetDirectActions()
        {
            constructionViewUntil = 0;
            ResetManualWorkPointerV414();
            RefreshUtilityStationsV414(true);
            CancelDirectConstruction();
            constructionOutdoor = false; constructionViewUntil = 0;
            if (fireStation) RefreshFireStation(true);
        }
        void RefreshFireStation(bool force = false)
        {
            RefreshUtilityStationsV414(force);
            bool ready = Expedition != null && Expedition.IsWorkCommitted("fire", PanicItemActionRules.FireStationId);
            if (!force && fireStation && fireStationReady == ready) return;
            if (fireStation)
            {
                floorItems.Remove(fireStation);
                visibilityRenderers.Remove(fireStation.GetInstanceID());
                Destroy(fireStation);
            }
            fireStationReady = ready;
            fireStation = PanicConstructionArt.CreateFireStation(ready);
            fireStation.name = "Fire equipment station";
            fireStation.transform.position = PanicJuryDemo.FireStationPosition;
            fireStation.transform.rotation = Quaternion.identity;
            var collider = fireStation.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, .48f, -.07f); collider.size = new Vector3(.42f, .96f, .28f);
            var pick = fireStation.AddComponent<HomePick>(); pick.Id = PanicItemActionRules.FireStationId; pick.Kind = "station";
            residenceTargets[PanicItemActionRules.FireStationId] = fireStation;
            floorItems.Add(fireStation);
        }

        public Vector3 WorkTargetViewport(int index)
        {
            if (!workCamera || !workRig || !WorkActive || Expedition.Work.IsWorldConstruction) return new Vector3(0, 0, -1);
            return PanicWorkArt.GetClickTargetPosition(workRig, index, out Vector3 position) ? workCamera.WorldToViewportPoint(position) : new Vector3(0, 0, -1);
        }
        public bool ClickWorkTarget(int index)
        {
            if (!WorkActive || InputLocked || CinematicActive || LossAnnounced || CancelFrame == Time.frameCount ||
                DiscreteInputFrame == Time.frameCount || hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame)) return false;
            return Expedition.Work.TryClick(index);
        }

        void StartDirectConstruction(string action, string target)
        {
            CancelDirectConstruction();
            constructionOutdoor = false; constructionViewUntil = 0;
            // Briefly frame outdoor assembly from its own side of the house so
            // a roof cannot hide a whole row appearing behind it. No extra wait.
            if (PositionOf(target, out var buildPosition) && !InResidence(player.position))
            {
                constructionViewAim = buildPosition + Vector3.up;
                constructionOutdoor = true;
                constructionViewOffset = buildPosition.x < -7.6f ? new Vector3(-15,13,-11) :
                    buildPosition.x > 7.6f ? new Vector3(15,13,-11) :
                    buildPosition.z > 5.6f ? new Vector3(11,13,15) : new Vector3(11,13,-15);
                constructionViewUntil = Time.unscaledTime + 1.35f;
            }
            var item = Model.Items.Find(i => i.Id == target);
            if (action == "upgrade" && item != null)
            {
                SetPreviousReinforcementVisible(target, false);
                var next = new PanicItem { Id = item.Id, Kind = item.Kind, Size = item.Size, Position = item.Position, Rotation = item.Rotation, Tier = item.Tier + 1 };
                constructionPreview = PanicConstructionArt.CreateReinforcement(next);
                constructionPreview.transform.position = item.Position;
                constructionPreview.transform.rotation = Quaternion.Euler(0, item.Rotation, 0);
            }
            else if (action == "plant")
            {
                var site = gardenSites.Find(s => s.id == target);
                if (site != null) { constructionPreview = PanicConstructionArt.CreateTreeRow(TreeVariant(site)); constructionPreview.transform.position = site.p; }
            }
            else if (action == "fire")
            {
                constructionPreview = PanicConstructionArt.CreateFireStation(true, false);
                constructionPreview.transform.position = PanicJuryDemo.FireStationPosition;
                constructionPreview.transform.rotation = Quaternion.identity;
            }
            else
            {
                var site = gardenSites.Find(s => s.id == target);
                if (site != null) { constructionPreview = CreateVisual(site.action); constructionPreview.transform.position = site.p; }
            }
            if (constructionPreview)
            {
                constructionPreview.name = "Installing " + action;
                foreach (var collision in constructionPreview.GetComponentsInChildren<Collider>()) collision.enabled = false;
                PanicConstructionArt.AnimateBuild(constructionPreview, 0);
            }
        }
        void AnimateDirectConstruction(float progress)
        {
            if (constructionPreview) PanicConstructionArt.AnimateBuild(constructionPreview, progress);
            if (constructionOutdoor) constructionViewUntil = Time.unscaledTime + .70f;
        }
        bool ApplyConstructionCamera(ref Vector3 aim, ref Vector3 offset)
        {
            if (Time.unscaledTime >= constructionViewUntil || Model.Phase != PanicPhase.Preparing || InventoryOpen || IsMoving || CameraOrbitPressed || CameraOrbitDragging) return false;
            aim = constructionViewAim; offset = constructionViewOffset; return true;
        }
        void CancelDirectConstruction()
        {
            if (workTarget != null) SetPreviousReinforcementVisible(workTarget, true);
            if (constructionPreview) Destroy(constructionPreview); constructionPreview = null;
        }
        void SetPreviousReinforcementVisible(string id, bool visible)
        {
            if (!hardware.TryGetValue(id, out var holder) || !holder) return;
            foreach (Transform child in holder.transform)
                if (child.GetComponent<PanicConstructionArt>()) child.gameObject.SetActive(visible);
        }
        GameObject CreatePreparedPickup(string id, int variant)
        {
            if (id != "seedling") return PanicResidenceArt.CreatePickup(id, variant);
            var kit = new GameObject("Adult tree-row kit");
            var row = PanicConstructionArt.CreateTreeRow(2); row.transform.SetParent(kit.transform, false);
            row.transform.localScale = Vector3.one * .07f; row.transform.localPosition = Vector3.up * .045f;
            var baseBox = GameObject.CreatePrimitive(PrimitiveType.Cube); baseBox.name = "Tree-row transport tray"; baseBox.transform.SetParent(kit.transform, false);
            baseBox.transform.localScale = new Vector3(.39f, .045f, .18f); baseBox.transform.localPosition = Vector3.up * .0225f;
            baseBox.GetComponent<Collider>().enabled = false;
            var material = new Material(Shader.Find("Standard")); material.color = new Color(.30f, .24f, .16f); material.SetFloat("_Glossiness", .08f);
            baseBox.GetComponent<Renderer>().sharedMaterial = material; kit.AddComponent<PanicDirectKitResources>().OwnedMaterial = material;
            return kit;
        }
        int TreeVariant(GardenSite site) { return site.id == "plant_west1" ? 0 : site.id == "plant_west2" ? 1 : 2; }
        GameObject CreateCompletedSite(GardenSite site)
        { return site.action == "plant" ? PanicConstructionArt.CreateTreeRow(TreeVariant(site)) : CreateVisual(site.action); }
        void AddTreeRowColliders(GameObject row, int variant)
        {
            foreach (var position in PanicConstructionArt.TreeTrunkSeats(variant))
            {
                var trunk = new GameObject("Tree trunk collision"); trunk.transform.SetParent(row.transform, false); trunk.transform.localPosition = position;
                var collider = trunk.AddComponent<CapsuleCollider>(); collider.center = Vector3.up * 1.0f; collider.radius = .12f; collider.height = 2.0f;
            }
        }
        bool TreeRowClearOfActor(string target)
        {
            var site = gardenSites.Find(s => s.id == target && s.action == "plant");
            if (site == null || !controller || !controller.enabled) return false;
            // Exact distance between vertical capsule segments, plus the controller's
            // existing skin. Matches the actual four generated trunk collider seats.
            Vector3 center = controller.transform.TransformPoint(controller.center);
            Vector3 scale = controller.transform.lossyScale;
            float radius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float half = Mathf.Max(0, controller.height * Mathf.Abs(scale.y) * .5f - radius);
            float low = center.y - half, high = center.y + half;
            float clearance = radius + .12f + controller.skinWidth;
            foreach (var local in PanicConstructionArt.TreeTrunkSeats(TreeVariant(site)))
            {
                Vector3 trunk = site.p + local;
                float gap = Mathf.Max(0, Mathf.Max(trunk.y + .12f - high, low - (trunk.y + 1.88f)));
                float dx = center.x - trunk.x, dz = center.z - trunk.z;
                if (dx * dx + dz * dz + gap * gap < clearance * clearance) return false;
            }
            return true;
        }
        void ApplyDirectVisibility()
        {
            RefreshGardenMarkersV51();
            bool cutaway = inside && !ResidenceCinematicExteriorView;
            Vector3 side = View ? View.transform.position - PlayerPosition : new Vector3(11, 13, -15);
            foreach (var item in Model.Items)
            {
                if (item.Kind != ItemKind.Wall && item.Kind != ItemKind.Roof) continue;
                if (!hardware.TryGetValue(item.Id, out var holder) || !holder) continue;
                if (cutaway && (item.Kind == ItemKind.Roof || !SameLevel(item.Position)))
                    continue; // The regular floor cache has already hidden these renderers.
                foreach (Transform child in holder.transform) PanicConstructionArt.SetCutawayVisibility(child.gameObject, cutaway, side);
            }
            if (constructionPreview && WorkActive && workAction == "upgrade")
                PanicConstructionArt.SetCutawayVisibility(constructionPreview, cutaway, side);
        }
    }
    public sealed class PanicDirectKitResources : MonoBehaviour
    {
        public Material OwnedMaterial;
        void OnDestroy() { if (OwnedMaterial) Destroy(OwnedMaterial); }
    }
}
