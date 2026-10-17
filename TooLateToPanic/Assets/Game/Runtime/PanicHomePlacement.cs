using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Mouse placement on the picked item's original floor. No run-state mutation until confirmation.</summary>
    public sealed partial class PanicHomeWorld
    {
        float placementFloorY;
        Vector3 placementOrigin;
        int placementBeginFrame = -1;
        bool placementValid, placementPointerOverUi, placementHasPointer, placementClickArmed;
        Renderer[] placementOriginalRenderers;
        bool[] placementOriginalRendererStates;
        Collider[] placementSceneColliders;

        public Vector3 PlacementCandidate { get { return moveCandidate; } }
        public bool PlacementValid { get { return IsMoving && placementValid; } }
        public float PlacementRotation { get { return moveRotation; } }
        public float PlacementFloorY { get { return placementFloorY; } }

        public bool BeginMousePlacement(Vector2 screen,bool pointerOverUi)
        {
            if(IsMoving||InputLocked||pointerOverUi||!View||!View.pixelRect.Contains(screen))return false;
            var hits=Physics.RaycastAll(View.ScreenPointToRay(screen),View.farClipPlane,~(1<<30),QueryTriggerInteraction.Ignore);
            Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)
            {
                if(hit.collider==controller||hit.transform.IsChildOf(player))continue;
                if(!PlacementPickVisible(hit.collider))continue;
                var pick=hit.collider.GetComponentInParent<HomePick>();
                if(!pick)return false;
                var item=Model.Items.Find(i=>i.Id==pick.Id);
                if(item==null||!item.Movable)return false;
                SetFocus(item.Id);BeginMove();return IsMoving;
            }
            return false;
        }

        bool PlacementPickVisible(Collider collider)
        {
            var pick=collider.GetComponentInParent<HomePick>();
            if(pick)
            {
                var item=Model.Items.Find(i=>i.Id==pick.Id);
                if(inside&&item!=null&&!SameLevel(item.Position))return false;
                GameObject visual=null;
                if(items.TryGetValue(pick.Id,out var itemVisual))visual=itemVisual;
                else if(sourceObjects.TryGetValue(pick.Id,out var sourceVisual))visual=sourceVisual;
                else if(residenceTargets.TryGetValue(pick.Id,out var decorVisual))visual=decorVisual;
                if(visual)
                {
                    var renderers=visual.GetComponentsInChildren<Renderer>(true);
                    bool visible=false;
                    foreach(var renderer in renderers)if(renderer.enabled&&renderer.gameObject.activeInHierarchy){visible=true;break;}
                    if(renderers.Length>0&&!visible)return false;
                }
            }
            // Cutaway wall colliders remain solid for the actor, but cannot hide visible picks.
            if(!collider.name.StartsWith("Collision_",StringComparison.Ordinal)||!collider.transform.parent)return true;
            string meshName=collider.name.Substring("Collision_".Length);
            foreach(var mesh in collider.transform.parent.GetComponentsInChildren<MeshFilter>(true))
            {
                if(mesh.name!=meshName||!mesh.sharedMesh)continue;
                if((mesh.transform.TransformPoint(mesh.sharedMesh.bounds.center)-collider.transform.position).sqrMagnitude>.0001f)continue;
                var renderer=mesh.GetComponent<Renderer>();
                return renderer&&renderer.enabled&&renderer.gameObject.activeInHierarchy;
            }
            return true;
        }

        public void BeginMove()
        {
            if (IsMoving || WorkActive || InventoryOpen || InputLocked || BagCameraOwnsView || NotebookBlocksInput || Model.IsPaused) return;
            var item = Selected;
            if (item == null || !item.Movable || item.Anchored || !item.Installed || !CanInteract)
            { Model.Notify("Taşımak için yaklaş ve sabitlemeyi kaldır."); return; }
            if (Model.Phase != PanicPhase.Preparing && Model.Phase != PanicPhase.Tutorial)
            { Model.Notify("Eşyalar yalnızca hazırlıkta taşınabilir."); return; }

            movingId = item.Id; effects.Play("furniture",.28f);
            placementOrigin = moveCandidate = item.Position;
            placementFloorY = item.Position.y;
            moveRotation = item.Rotation;
            placementBeginFrame = Time.frameCount;
            placementClickArmed = !Input.GetMouseButton(0);
            placementHasPointer = placementValid = false;
            placementPointerOverUi = hud && hud.PointerOverUi;
            placementSceneColliders = FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            // Keep the original physical volume until commit. Cancel cannot restore furniture on the actor.
            if (items.TryGetValue(item.Id, out var original) && original)
            {
                placementOriginalRenderers = original.GetComponentsInChildren<Renderer>(true);
                placementOriginalRendererStates = new bool[placementOriginalRenderers.Length];
                for (int i = 0; i < placementOriginalRenderers.Length; i++)
                { placementOriginalRendererStates[i] = placementOriginalRenderers[i].enabled; placementOriginalRenderers[i].enabled = false; }
            }
            ghost = CreateVisual(item.Id == "desk_upper" ? "desk" : item.ArtId);
            ghost.name = "Mouse placement preview";
            foreach (var c in ghost.GetComponentsInChildren<Collider>(true)) { c.enabled = false; Destroy(c); }
            foreach (var renderer in ghost.GetComponentsInChildren<Renderer>(true))
            { renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false; }
            // The selected ghost silhouette is drawn by UpdateInteractionOutline.
            PlacementMessage = "Fareyi oyun alanına getir.";
            UpdatePlacementPreview(item);
            Model.Notify("Fare: yerleştir · Sol tık: bırak · Teker/R: döndür · Sağ tık/Esc: iptal");
            if (animator) animator.SetBool("Working", false);
        }

        void UpdateMove(float dt)
        {
            if (Model.Items.Find(i => i.Id == movingId) == null) { CancelMove(); return; }
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
            { effects.Play("ui",.2f); CancelMove(); return; }

            MovePlayer(dt, ReadMove());
            if (Input.GetKeyDown(KeyCode.Space)) Jump();
            bool overUi = hud && hud.PointerOverUi;
            UpdatePlacementPointer(Input.mousePosition, overUi);
            if (!overUi)
            {
                float wheel = Input.mouseScrollDelta.y;
                if (Mathf.Abs(wheel) > .1f) RotatePlacement(Mathf.Sign(wheel) * 45);
                if (Input.GetKeyDown(KeyCode.R)) RotatePlacement(Input.GetKey(KeyCode.LeftShift) ? -45 : 45);
            }
            if (!Input.GetMouseButton(0)) placementClickArmed = true;
            if ((Input.GetMouseButtonDown(0) && placementClickArmed) || Input.GetKeyDown(KeyCode.Return)) ConfirmMove();
        }

        /// <summary>Uses screen pixels, with the same HUD guard as normal gameplay. Returns current validity.</summary>
        public bool UpdatePlacementPointer(Vector2 screen, bool pointerOverUi)
        {
            if (!IsMoving) return false;
            var item = Model.Items.Find(i => i.Id == movingId);
            if (item == null) { CancelMove(); return false; }
            placementPointerOverUi = pointerOverUi;
            placementHasPointer = false;
            if (pointerOverUi) PlacementMessage = "Yerleştirmek için fareyi oyun alanına getir.";
            else if (!View || !PlacementFinite(screen.x) || !PlacementFinite(screen.y) || !View.pixelRect.Contains(screen))
                PlacementMessage = "Fareyi oyun alanına getir.";
            else
            {
                var plane = new Plane(Vector3.up, new Vector3(0, placementFloorY, 0));
                Ray ray = View.ScreenPointToRay(screen);
                if (plane.Raycast(ray, out float distance) && distance >= 0 && distance <= View.farClipPlane)
                {
                    moveCandidate = ray.GetPoint(distance);
                    moveCandidate.y = placementFloorY;
                    placementHasPointer = true;
                }
                else PlacementMessage = "Bu katta bir zemin seç.";
            }
            UpdatePlacementPreview(item);
            return placementValid;
        }

        public void RotatePlacement(float degrees)
        {
            if (!IsMoving || !PlacementFinite(degrees)) return;
            moveRotation = Mathf.Repeat(moveRotation + degrees, 360); effects.Play("furniture",.18f);
            var item = Model.Items.Find(i => i.Id == movingId);
            if (item != null) UpdatePlacementPreview(item);
        }

        /// <summary>Rechecks geometry at commit; one click cannot both begin and commit a move.</summary>
        public bool ConfirmMove()
        {
            if (!IsMoving || Time.frameCount <= placementBeginFrame || InputLocked || WorkActive || InventoryOpen) return false;
            var item = Model.Items.Find(i => i.Id == movingId);
            if (item == null) { CancelMove(); return false; }
            UpdatePlacementPreview(item);
            if (!placementValid) { effects.Play("reject",.25f); return false; }
            if (!Model.MoveItem(movingId, moveCandidate, moveRotation)) { effects.Play("reject",.25f); return false; }
            effects.Play("furniture",.4f);
            CancelMove();
            SyncVisuals(true); // Includes matching loot-source position and floor, without cloning or costs.
            UpdateContext();
            return true;
        }

        void UpdatePlacementPreview(PanicItem item)
        {
            placementValid = placementHasPointer && !placementPointerOverUi;
            if (placementValid)
            {
                placementValid = ValidatePlacement(item, moveCandidate, moveRotation, out string reason);
                PlacementMessage = placementValid ? "Sol tık: bırak · Teker/R: döndür" : reason;
            }
            Color color = placementValid ? new Color(.36f, .85f, .6f) : new Color(.98f, .36f, .25f);
            if (ghost)
            {
                ghost.transform.SetPositionAndRotation(moveCandidate, Quaternion.Euler(0, moveRotation, 0));
                var block = new MaterialPropertyBlock();
                foreach (var renderer in ghost.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.GetPropertyBlock(block);
                    block.SetColor("_Color", color); block.SetColor("_BaseColor", color);
                    renderer.SetPropertyBlock(block);
                }
            }
            // No ground footprint: preserve the actual ghost silhouette only.
        }

        public void CancelMove()
        {
            if (IsMoving) CancelFrame = Time.frameCount;
            if (placementOriginalRenderers != null)
                for (int i = 0; i < placementOriginalRenderers.Length; i++)
                    if (placementOriginalRenderers[i]) placementOriginalRenderers[i].enabled = placementOriginalRendererStates[i];
            placementOriginalRenderers = null; placementOriginalRendererStates = null;
            movingId = null;
            if (ghost)
            {
                foreach (var c in ghost.GetComponentsInChildren<Collider>(true)) c.enabled = false;
                ghost.SetActive(false); Destroy(ghost);
            }
            ghost = null;
            if (moveOutline) { moveOutline.gameObject.SetActive(false); Destroy(moveOutline.gameObject); }
            moveOutline = null;
            placementValid = placementHasPointer = placementPointerOverUi = false;
            placementSceneColliders = null;
            PlacementMessage = "";
            if (animator && !WorkActive) animator.SetBool("Working", false);
        }

        public bool ValidatePlacement(PanicItem item, Vector3 p, float rotation, out string reason)
        {
            if (item == null || !PlacementFinite(p.x) || !PlacementFinite(p.y) || !PlacementFinite(p.z) || !PlacementFinite(rotation))
            { reason = "Geçersiz yerleşim."; return false; }
            float floorY = IsMoving && item.Id == movingId ? placementFloorY : item.Position.y;
            if (Mathf.Abs(p.y - floorY) > .025f)
            { reason = "Eşyayı alındığı katta bırak."; return false; }
            var footprint = RotatedFootprint(item.Size, rotation) * .5f;
            if (Mathf.Abs(p.x) + footprint.x > 7.65f || Mathf.Abs(p.z) + footprint.y > 5.65f)
            { reason = "Eşyayı evin içinde bırak."; return false; }
            if (Mathf.Abs(p.x) < footprint.x + 1.05f && p.z + footprint.y > -1 && p.z - footprint.y < 5.2f)
            { reason = "Merdiven ve geçiş koridorunu açık bırak."; return false; }
            if (!PlacementHasFloor(item, p, rotation, floorY))
            { reason = "Eşyanın tamamı sağlam zeminde olmalı."; return false; }
            if (PlacementCoversActor(item, p, rotation))
            { reason = "Karakterin üzerinde eşya bırakamazsın."; return false; }

            foreach (var other in Model.Items)
            {
                if (other.Id == item.Id || !other.Installed || Mathf.Abs(other.Position.y - p.y) > .8f) continue;
                if (other.Kind == ItemKind.Furniture)
                {
                    var otherHalf = RotatedFootprint(other.Size, other.Rotation) * .5f;
                    if (Mathf.Abs(p.x - other.Position.x) < footprint.x + otherHalf.x + .06f &&
                        Mathf.Abs(p.z - other.Position.z) < footprint.y + otherHalf.y + .06f)
                    { reason = other.Label + " ile çakışıyor."; return false; }
                }
                if (other.Kind == ItemKind.Door)
                {
                    // Reserve either opening direction, even while the real leaf is open.
                    float width = Mathf.Max(1.2f, other.Size.x);
                    if (PlacementSweptBoxes(p, p, new Vector3(item.Size.x * .5f + .04f, item.Size.y * .5f, item.Size.z * .5f + .04f), rotation,
                        other.Position, new Vector3(width * .65f, other.Size.y * .5f, width + .12f), other.Rotation))
                    { reason = "Kapının açılacağı alanı boş bırak."; return false; }
                }
            }
            if (p.z - footprint.y < -4.2f && Mathf.Abs(p.x) < footprint.x + 1.1f)
            { reason = "Çıkış kapısını açık bırak."; return false; }

            Physics.SyncTransforms();
            Vector3 half = new Vector3(item.Size.x * .5f + .025f, Mathf.Max(.05f, item.Size.y * .5f - .04f), item.Size.z * .5f + .025f);
            Vector3 center = p + Vector3.up * (half.y + .065f);
            foreach (var collider in Physics.OverlapBox(center, half, Quaternion.Euler(0, rotation, 0), ~(1 << 30), QueryTriggerInteraction.Ignore))
            {
                if (PlacementIgnores(collider, item.Id)) continue;
                reason = "Duvar veya eşya ile çakışıyor."; return false;
            }

            // Cutaway visibility disables some architectural GameObjects. Their authored boxes still count.
            var colliders = placementSceneColliders ?? FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Vector3 origin = IsMoving && movingId == item.Id ? placementOrigin : item.Position;
            foreach (var collider in colliders)
            {
                if (PlacementIgnores(collider, item.Id) || !(collider is BoxCollider box)) continue;
                PlacementBox(box, out Vector3 obstacleCenter, out Vector3 obstacleHalf, out float obstacleYaw);
                if (!collider.gameObject.activeInHierarchy && PlacementSweptBoxes(center, center, half, rotation, obstacleCenter, obstacleHalf, obstacleYaw))
                { reason = "Duvar veya eşya ile çakışıyor."; return false; }
                bool structure = collider.name.StartsWith("Collision_solid_", StringComparison.Ordinal);
                var pick = collider.GetComponentInParent<HomePick>();
                bool door = pick && Model.Items.Exists(i => i.Id == pick.Id && i.Kind == ItemKind.Door);
                if ((!structure && !door) || obstacleCenter.y + obstacleHalf.y <= floorY + .055f) continue;
                if (PlacementSweptBoxes(origin + Vector3.up * (half.y + .065f), center, half, rotation, obstacleCenter, obstacleHalf, obstacleYaw))
                { reason = "Eşyayı duvarın veya kapının içinden geçiremezsin."; return false; }
            }
            foreach (var source in Expedition.Sources)
            {
                if (source.Id == item.Id || Model.Items.Exists(i => i.Id == source.Id) || Mathf.Abs(source.Position.y - p.y) > .8f) continue;
                if (Mathf.Abs(source.Position.x - p.x) < footprint.x + .6f && Mathf.Abs(source.Position.z - p.z) < footprint.y + .45f)
                { reason = source.Label + " için geçiş bırak."; return false; }
            }
            reason = "Uygun konum"; return true;
        }

        bool PlacementIgnores(Collider collider, string itemId)
        {
            if (!collider || !collider.enabled || collider.isTrigger || collider.gameObject.layer == 30) return true;
            if (collider == controller || player && collider.transform.IsChildOf(player)) return true;
            if (ghost && collider.transform.IsChildOf(ghost.transform)) return true;
            if (items.TryGetValue(itemId, out var original) && original && collider.transform.IsChildOf(original.transform)) return true;
            if (collider.GetComponentInParent<ResidenceLootPick>()) return true;
            var pick = collider.GetComponentInParent<HomePick>();
            return pick && Model.Items.Exists(i => i.Id == pick.Id && !i.Installed);
        }

        bool PlacementHasFloor(PanicItem item, Vector3 p, float yaw, float floorY)
        {
            int level = Mathf.RoundToInt(floorY / 3.2f), index = level + 1;
            if (level < -1 || level > 1 || Mathf.Abs(floorY - level * 3.2f) > .025f || index >= floorVisuals.Count) return false;
            var slabs = floorVisuals[index].GetComponentsInChildren<BoxCollider>(true);
            Quaternion rotation = Quaternion.Euler(0, yaw, 0);
            int nx = Mathf.Max(1, Mathf.CeilToInt(item.Size.x / .3f)), nz = Mathf.Max(1, Mathf.CeilToInt(item.Size.z / .3f));
            for (int x = 0; x <= nx; x++) for (int z = 0; z <= nz; z++)
            {
                Vector3 sample = p + rotation * new Vector3((x / (float)nx - .5f) * item.Size.x, 0, (z / (float)nz - .5f) * item.Size.z);
                bool supported = false;
                foreach (var slab in slabs)
                {
                    if (!slab.enabled || (!slab.name.StartsWith("Collision_solid_floor_", StringComparison.Ordinal) && slab.name != "Collision_solid_basement_slab")) continue;
                    PlacementBox(slab, out Vector3 c, out Vector3 h, out float angle);
                    if (Mathf.Abs(c.y + h.y - floorY) > .12f) continue;
                    var local = Quaternion.Euler(0, -angle, 0) * (sample - c);
                    if (Mathf.Abs(local.x) <= h.x + .001f && Mathf.Abs(local.z) <= h.z + .001f) { supported = true; break; }
                }
                if (!supported) return false;
            }
            return true;
        }

        bool PlacementCoversActor(PanicItem item, Vector3 p, float yaw)
        {
            if (!player || !controller) return false;
            Vector3 body = controller.transform.TransformPoint(controller.center);
            float radius = controller.radius * Mathf.Max(Mathf.Abs(player.lossyScale.x), Mathf.Abs(player.lossyScale.z)) + .08f;
            float height = controller.height * Mathf.Abs(player.lossyScale.y);
            if (body.y + height * .5f < p.y || body.y - height * .5f > p.y + item.Size.y) return false;
            Vector3 local = Quaternion.Euler(0, -yaw, 0) * (body - p);
            float dx = Mathf.Max(0, Mathf.Abs(local.x) - item.Size.x * .5f), dz = Mathf.Max(0, Mathf.Abs(local.z) - item.Size.z * .5f);
            return dx * dx + dz * dz < radius * radius;
        }

        static void PlacementBox(BoxCollider box, out Vector3 center, out Vector3 half, out float yaw)
        {
            center = box.transform.TransformPoint(box.center);
            Vector3 scale = box.transform.lossyScale;
            half = Vector3.Scale(box.size * .5f, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            yaw = box.transform.eulerAngles.y;
        }

        // Continuous 2D box SAT: the moving box's projection must overlap on all four axes at one time.
        static bool PlacementSweptBoxes(Vector3 from, Vector3 to, Vector3 half, float yaw, Vector3 obstacle, Vector3 obstacleHalf, float obstacleYaw)
        {
            if (Mathf.Max(from.y, to.y) + half.y <= obstacle.y - obstacleHalf.y + .001f ||
                Mathf.Min(from.y, to.y) - half.y >= obstacle.y + obstacleHalf.y - .001f) return false;
            Quaternion a = Quaternion.Euler(0, yaw, 0), b = Quaternion.Euler(0, obstacleYaw, 0);
            Vector3 ar = a * Vector3.right, af = a * Vector3.forward, br = b * Vector3.right, bf = b * Vector3.forward;
            float first = 0, last = 1;
            foreach (Vector3 axis in new[] { ar, af, br, bf })
            {
                float extent = half.x * Mathf.Abs(Vector3.Dot(axis, ar)) + half.z * Mathf.Abs(Vector3.Dot(axis, af)) +
                    obstacleHalf.x * Mathf.Abs(Vector3.Dot(axis, br)) + obstacleHalf.z * Mathf.Abs(Vector3.Dot(axis, bf));
                float distance = Vector3.Dot(from - obstacle, axis), speed = Vector3.Dot(to - from, axis);
                if (Mathf.Abs(speed) < .00001f) { if (Mathf.Abs(distance) >= extent) return false; continue; }
                float enter = (-extent - distance) / speed, exit = (extent - distance) / speed;
                if (enter > exit) { float swap = enter; enter = exit; exit = swap; }
                first = Mathf.Max(first, enter); last = Mathf.Min(last, exit);
                if (first > last) return false;
            }
            return true;
        }

        static bool PlacementFinite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        static Vector2 RotatedFootprint(Vector3 size, float rot)
        { float c = Mathf.Abs(Mathf.Cos(rot * Mathf.Deg2Rad)), s = Mathf.Abs(Mathf.Sin(rot * Mathf.Deg2Rad)); return new Vector2(size.x * c + size.z * s, size.x * s + size.z * c); }
        Vector3[] FootprintCorners(PanicItem item)
        { var s = item.Size * .5f; return new[] { new Vector3(-s.x, 0, -s.z), new Vector3(s.x, 0, -s.z), new Vector3(s.x, 0, s.z), new Vector3(-s.x, 0, s.z) }; }
    }
}
