using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    // Read-only recommendations. Only the existing interaction/work/shelter paths commit progress.
    public sealed partial class PanicHomeWorld
    {
        const float GuidanceGridV54 = .5f;
        const int GuidanceMaxExpandedV54 = 2600;
        const int GuidanceMaxPlansV54 = 8;
        readonly List<Vector3> guidanceRouteV54 = new List<Vector3>();
        readonly Dictionary<int, bool> guidanceClearV54 = new Dictionary<int, bool>();
        readonly Dictionary<long, bool> guidanceEdgesV54 = new Dictionary<long, bool>();
        readonly Dictionary<int, Vector3> guidanceStairsV54 = new Dictionary<int, Vector3>();
        readonly Dictionary<int, List<int>> guidanceStairLinksV54 = new Dictionary<int, List<int>>();
        readonly Collider[] guidanceOverlapV54 = new Collider[96];
        readonly RaycastHit[] guidanceHitsV54 = new RaycastHit[96];
        readonly Dictionary<Collider, string> guidanceDoorsV54 = new Dictionary<Collider, string>();
        readonly List<GuidanceGoalV54> guidanceGoalsV54 = new List<GuidanceGoalV54>();
        readonly List<List<GuidanceGoalV54>> guidancePlanGroupsV54 = new List<List<GuidanceGoalV54>>();
        System.Collections.IEnumerator guidancePlannerV54;
        GuidanceGoalV54 guidanceSearchGoalV54;
        List<Vector3> guidanceSearchRouteV54;
        float guidanceSearchLengthV54;
        string guidanceNoRouteV54;
        float guidanceNextPlanV54;
        Vector3 guidancePlannedFromV54;
        string guidanceStateV54;
        int guidancePlansV54;
        LineRenderer guidanceBeaconV54;
        Material guidanceMaterialV54;
        GUIStyle guidanceTextV54, guidanceSmallV54, guidanceArrowV54;

        sealed class GuidanceGoalV54
        {
            public string Id, Label, Action;
            public Vector3 Position;
            public int Floor;
            public bool IsInteraction = true;
            public float ReachEnvelope = 2.4f;
        }
        sealed class GuidanceSearchV54
        {
            public int Key, Parent;
            public float Cost, Estimate;
            public bool Closed;
        }

        public bool GuidanceVisibleV54 { get; private set; }
        public bool GuidanceRouteValidV54 { get; private set; }
        public string GuidanceObjectiveIdV54 { get; private set; } = string.Empty;
        public string GuidanceObjectiveActionV54 { get; private set; } = string.Empty;
        public string GuidanceObjectiveLabelV54 { get; private set; } = string.Empty;
        public string GuidanceCheckpointLabelV54 { get; private set; } = string.Empty;
        public Vector3 GuidanceObjectivePositionV54 { get; private set; }
        public Vector3 GuidanceCheckpointPositionV54 { get; private set; }
        public int GuidanceWaypointCountV54 { get { return guidanceRouteV54.Count; } }
        public float GuidanceRouteLengthV54 { get; private set; }
        public int GuidanceExpandedNodesV54 { get; private set; }
        public double LastGuidancePlanMillisecondsV54 { get; private set; }
        public double GuidancePeakSliceMillisecondsV54 { get; private set; }
        public bool GuidancePlanningV54 { get { return guidancePlannerV54 != null; } }
        public Vector3 GuidanceWaypointV54(int index)
        { return guidanceRouteV54.Count == 0 ? PlayerPosition : guidanceRouteV54[Mathf.Clamp(index, 0, guidanceRouteV54.Count - 1)]; }
        // Read-only native diagnostic hooks; these do not alter controller/collider/model state.
        public bool ProbeGuidanceFloorSupportV54(Vector3 footPosition) { return GuidanceHasSupportV54(footPosition); }
        public bool ProbeGuidanceClearanceV54(Vector3 from, Vector3 to, bool allowDoorPortal, out string blockingDoor)
        { Physics.SyncTransforms(); return GuidanceBodyClearV54(from, to, allowDoorPortal, out blockingDoor); }

        public void InitGuidanceV54()
        {
            if (guidanceBeaconV54) return;
            var root = new GameObject("V54 next reachable checkpoint beacon");
            root.transform.SetParent(transform, false);
            guidanceBeaconV54 = root.AddComponent<LineRenderer>();
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
            if (shader) { guidanceMaterialV54 = new Material(shader); guidanceMaterialV54.color = new Color(.27f, .92f, .81f); guidanceBeaconV54.sharedMaterial = guidanceMaterialV54; }
            guidanceBeaconV54.useWorldSpace = true;
            guidanceBeaconV54.startWidth = guidanceBeaconV54.endWidth = .035f;
            guidanceBeaconV54.startColor = guidanceBeaconV54.endColor = new Color(.27f, .92f, .81f, .95f);
            guidanceBeaconV54.shadowCastingMode = ShadowCastingMode.Off;
            guidanceBeaconV54.receiveShadows = false;
            guidanceBeaconV54.enabled = false;
            // This art has no collider, picking target, light, audio, or input ownership.
            foreach (var pair in hinges)
                if (pair.Value.directionalDoor && pair.Value.leaf)
                    foreach (var c in pair.Value.leaf.GetComponentsInChildren<Collider>(true)) guidanceDoorsV54[c] = pair.Key;
            GuidanceBuildStairsV54();
        }

        bool GuidanceCanShowV54()
        {
            return Model != null && Expedition != null && player && View &&
                (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial) &&
                !Model.IsPaused && !NotebookBlocksInput && !InputLocked && !CinematicActive && !LossAnnounced &&
                !BagCameraTransitionActive && !InventoryOpen && !WorkActive && !IsMoving && JuryAllowsMovement &&
                !(hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame));
        }

        // Hook once at the end of the normal Update, after hinges/visibility and actual model progress.
        public void TickGuidanceV54()
        {
            if (!guidanceBeaconV54) InitGuidanceV54();
            GuidanceVisibleV54 = GuidanceCanShowV54();
            if (!GuidanceVisibleV54) { if (guidanceBeaconV54) guidanceBeaconV54.enabled = false; return; }
            string state = GuidanceStateKeyV54();
            bool changed = state != guidanceStateV54;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            bool planWork = guidancePlannerV54 != null;
            if (changed || guidancePlannerV54 == null && Time.unscaledTime >= guidanceNextPlanV54 &&
                ((PlayerPosition - guidancePlannedFromV54).sqrMagnitude > .36f || !GuidanceRouteValidV54))
            {
                guidanceStateV54 = state; guidanceNextPlanV54 = Time.unscaledTime + 1.0f;
                guidancePlannedFromV54 = PlayerPosition; guidanceClearV54.Clear(); guidanceEdgesV54.Clear(); guidancePlansV54 = 0;
                GuidanceExpandedNodesV54 = 0;
                LastGuidancePlanMillisecondsV54 = GuidancePeakSliceMillisecondsV54 = 0;
                Physics.SyncTransforms();
                GuidanceChooseGoalV54();
                guidancePlannerV54 = guidancePlanGroupsV54.Count > 0 ? GuidancePlanBatchV54() : null;
                planWork = true;
            }
            if (guidancePlannerV54 != null && !guidancePlannerV54.MoveNext()) guidancePlannerV54 = null;
            watch.Stop();
            if (planWork)
            {
                LastGuidancePlanMillisecondsV54 += watch.Elapsed.TotalMilliseconds;
                GuidancePeakSliceMillisecondsV54 = Math.Max(GuidancePeakSliceMillisecondsV54, watch.Elapsed.TotalMilliseconds);
            }
            GuidanceChooseCheckpointV54();
            GuidanceUpdateBeaconV54();
        }

        string GuidanceStateKeyV54()
        {
            var b = new System.Text.StringBuilder();
            b.Append(Model.Wave).Append('|').Append((int)Model.Phase).Append('|').Append(BagFinalStageActive)
                .Append('|').Append(Model.PackedWater).Append('|').Append(Model.PackedFood).Append('|').Append(Model.Budget)
                .Append('|').Append(Model.UtilitySafety.CompletedCount).Append('|').Append(Model.ReachedShelterV5)
                .Append('|').Append(Model.HasActiveDisasterV54(DisasterType.Flood))
                .Append('|').Append(Model.HasActiveDisasterV54(DisasterType.Fire))
                .Append('|').Append(Model.HasActiveDisasterV54(DisasterType.Earthquake))
                .Append('|').Append(Model.HasActiveDisasterV54(DisasterType.Storm));
            foreach (var id in PanicExpedition.MaterialIds) b.Append('|').Append(Expedition.Count(id));
            foreach (var i in Model.Items) if (i.Kind == ItemKind.Furniture) b.Append('|').Append(i.Id).Append(':').Append(i.Anchored).Append(':').Append(i.Position);
            foreach (var s in Expedition.Sources) b.Append('|').Append(s.Id).Append(':').Append(s.AutoLooted);
            foreach (var s in Expedition.WorldSupplies) b.Append('|').Append(s.Id).Append(':').Append(s.Count);
            foreach (var h in hinges) if (h.Value.directionalDoor) b.Append('|').Append(h.Key).Append(':').Append(h.Value.open);
            if (JuryActive && JurySnapshot != null) b.Append('|').Append(JurySnapshot.TargetId).Append('|').Append(JurySnapshot.Hint);
            return b.ToString();
        }

        void GuidanceChooseGoalV54()
        {
            guidanceRouteV54.Clear(); GuidanceRouteValidV54 = false; GuidanceRouteLengthV54 = 0;
            GuidanceObjectiveIdV54 = GuidanceObjectiveActionV54 = string.Empty;
            GuidanceObjectiveLabelV54 = "Önlemlerini gözden geçir";
            guidanceGoalsV54.Clear(); guidancePlanGroupsV54.Clear(); guidanceNoRouteV54 = "Açık yolu kontrol et";
            // Short flow owns its real objective sequence, including its existing safe areas.
            if (JuryActive && JurySnapshot != null && JurySnapshot.HasWorldTarget)
            {
                var j = JurySnapshot;
                guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = j.TargetId, Position = j.TargetPosition,
                    Floor = Mathf.RoundToInt(j.TargetPosition.y / 3.2f), Label = j.Hint, Action = "shortflow" });
                GuidanceQueueGoalsV54(guidanceGoalsV54); return;
            }
            if (BagFinalStageActive)
            {
                if (Model.ReachedShelterV5) { GuidanceObjectiveLabelV54 = "Güvenli alandasın"; return; }
                string needed = Model.PackedWater < 1 ? "water" : Model.PackedFood < 1 ? "food" : string.Empty;
                if (needed.Length > 0)
                {
                    foreach (var s in Expedition.WorldSupplies)
                        if (s.Count > 0 && s.ItemId == needed && GuidanceSupplyCanPackV54(needed) && InteractionObject(s.Id))
                            guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = s.Id, Position = s.Position, Floor = s.Floor,
                                Label = needed == "water" ? "1 su al · E / F" : "1 gıda al · E / F", Action = "collect" });
                    guidanceNoRouteV54 = GuidanceSupplyCanPackV54(needed) ?
                            (needed == "water" ? "Ulaşılabilir su stoğu yok · çevreni kontrol et" : "Ulaşılabilir gıda stoğu yok · çevreni kontrol et") :
                            "Çantada yer aç · Tab";
                    GuidanceQueueGoalsV54(guidanceGoalsV54);
                    if (guidancePlanGroupsV54.Count == 0) GuidanceObjectiveLabelV54 = guidanceNoRouteV54;
                    return;
                }
                bool flood = Model.HasActiveDisasterV54(DisasterType.Flood);
                guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = flood ? "@panicroom" : "bunker", Floor = flood ? 1 : -1,
                    Position = flood ? PanicRoomPositionV5 : BunkerPosition,
                    Label = flood ? "Üst kat panik odasına gir · E" : "Bodrum sığınağına gir · E", Action = "shelter" });
                guidanceNoRouteV54 = "Sığınak yolunu aç · kapı ve eşyaları kontrol et";
                GuidanceQueueGoalsV54(guidanceGoalsV54);
                return;
            }
            // A quake makes anchoring the first recommendation. Other hazards first expose their real utility controls.
            bool quake = Model.HasActiveDisasterV54(DisasterType.Earthquake);
            if (quake) GuidanceQueueAnchorsV54();
            GuidanceQueueUtilitiesV54();
            if (!quake) GuidanceQueueAnchorsV54();
            guidanceNoRouteV54 = "Önlemlerini kontrol et · son 45 saniyede çantanı hazırla";
            if (guidancePlanGroupsV54.Count == 0) GuidanceObjectiveLabelV54 = guidanceNoRouteV54;
        }

        bool GuidanceSupplyCanPackV54(string id)
        {
            // Collection auto-packs actual bag slots. Loose inventory alone does not satisfy BagReady.
            if (Expedition.Bag == null) return false;
            foreach (var s in Expedition.Bag) if (s == null || s.Count <= 0 || string.IsNullOrEmpty(s.Id) || s.Id == id && s.Count < 3) return true;
            return false;
        }

        void GuidanceQueueUtilitiesV54()
        {
            guidanceGoalsV54.Clear();
            string[] actions = Model.HasActiveDisasterV54(DisasterType.Flood) ? new[] { "wateroff", "poweroff", "gasoff" } :
                Model.HasActiveDisasterV54(DisasterType.Fire) ? new[] { "gasoff", "poweroff", "wateroff" } : new[] { "gasoff", "wateroff", "poweroff" };
            foreach (var action in actions)
            {
                string id = PanicUtilitySafetyV414.TargetForAction(action);
                if (Model.UtilitySafety.IsComplete(action) || Expedition.IsWorkCommitted(action, id) || !Model.CanBeginJuryWork(action, id)) continue;
                if (!TryUtilityStationPositionV414(id, out var p)) continue;
                guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = id, Position = p, Floor = Mathf.RoundToInt(p.y / 3.2f),
                    Label = PanicUtilitySafetyV414.Label(action) + " · E", Action = action });
            }
            GuidanceQueueGoalsV54(guidanceGoalsV54);
        }

        void GuidanceQueueAnchorsV54()
        {
            var anchors = new List<PanicItem>();
            foreach (var i in Model.Items)
            {
                string a = PanicItemActionRules.PrimaryAction(i);
                if ((a == "anchorwall" || a == "anchorfloor") && PanicItemActionRules.Allows(a, i, i.Id) &&
                    Model.CanBeginJuryWork(a, i.Id) && (Model.InfiniteBudget || Model.Budget >= PanicExpedition.WorkCost(a, i))) anchors.Add(i);
            }
            anchors.Sort((a, b) => { int d = (a.Position - PlayerPosition).sqrMagnitude.CompareTo((b.Position - PlayerPosition).sqrMagnitude); return d != 0 ? d : string.CompareOrdinal(a.Id, b.Id); });
            guidanceGoalsV54.Clear();
            // An owned released anchor has a genuinely free recipe; use canonical costs for all others.
            foreach (var i in anchors)
            {
                string action = PanicItemActionRules.PrimaryAction(i); bool have = true;
                foreach (var cost in PanicExpedition.MaterialCosts(action, i)) if (!Model.FreeMaterials && Expedition.Count(cost.Id) < cost.Required) have = false;
                if (have) guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = i.Id, Position = i.Position, Floor = Mathf.RoundToInt(i.Position.y / 3.2f), Label = i.Label + " · Q seç / 1 sabitle", Action = action });
            }
            GuidanceQueueGoalsV54(guidanceGoalsV54);
            guidanceGoalsV54.Clear();
            foreach (var i in anchors)
            {
                foreach (var cost in PanicExpedition.MaterialCosts(PanicItemActionRules.PrimaryAction(i), i))
                {
                    if (Model.FreeMaterials || Expedition.Count(cost.Id) >= cost.Required) continue;
                    foreach (var s in Expedition.Sources)
                    {
                        if (s.AutoLooted || s.Open || s.Contents == null || !InteractionObject(s.Id)) continue;
                        bool stock = false;
                        foreach (var c in s.Contents) if (c != null && c.Id == cost.Id && c.Count > 0) { stock = true; break; }
                        if (stock && !guidanceGoalsV54.Exists(g => g.Id == s.Id)) guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = s.Id, Position = s.Position,
                            Floor = s.Floor, Label = s.Label + " · " + PanicExpedition.DisplayName(cost.Id) + " al · E", Action = "materials" });
                    }
                }
            }
            GuidanceQueueGoalsV54(guidanceGoalsV54);
        }

        void GuidanceQueueGoalsV54(List<GuidanceGoalV54> goals)
        {
            if (goals.Count == 0) return;
            goals.Sort((a, b) => { int d = (a.Position - PlayerPosition).sqrMagnitude.CompareTo((b.Position - PlayerPosition).sqrMagnitude); return d != 0 ? d : string.CompareOrdinal(a.Id, b.Id); });
            foreach (var goal in goals)
            {
                if (TryGetInteractionBounds(goal.Id, out var bounds))
                    goal.ReachEnvelope = 2.4f + bounds.extents.magnitude + Vector3.Distance(bounds.center, goal.Position);
            }
            guidancePlanGroupsV54.Add(new List<GuidanceGoalV54>(goals));
            if (guidancePlanGroupsV54.Count == 1) GuidanceObjectiveLabelV54 = goals[0].Label;
        }

        System.Collections.IEnumerator GuidancePlanBatchV54()
        {
            foreach (var goals in guidancePlanGroupsV54)
            {
                if (guidancePlansV54 >= GuidanceMaxPlansV54) break;
                var search = GuidancePlanV54(goals);
                while (search.MoveNext()) yield return null;
                if (guidanceSearchGoalV54 == null) continue;
                var chosen = guidanceSearchGoalV54;
                GuidanceObjectiveIdV54 = chosen.Id; GuidanceObjectiveActionV54 = chosen.Action;
                GuidanceObjectiveLabelV54 = chosen.Label; GuidanceObjectivePositionV54 = chosen.Position;
                guidanceRouteV54.Clear(); guidanceRouteV54.AddRange(guidanceSearchRouteV54);
                GuidanceRouteLengthV54 = guidanceSearchLengthV54; GuidanceRouteValidV54 = true;
                yield break;
            }
            GuidanceObjectiveLabelV54 = guidanceNoRouteV54;
        }

        static int GuidanceGridKeyV54(int x, int z, int floor) { return ((floor + 1) * 96 + z + 44) * 96 + x + 46; }
        static void GuidanceDecodeV54(int key, out int x, out int z, out int floor)
        { x = key % 96 - 46; int v = key / 96; z = v % 96 - 44; floor = v / 96 - 1; }
        Vector3 GuidancePointV54(int key)
        {
            if (key < 0) return guidanceStairsV54[key];
            GuidanceDecodeV54(key, out int x, out int z, out int f);
            return new Vector3(x * GuidanceGridV54, f * 3.2f + .045f, z * GuidanceGridV54);
        }
        bool GuidanceGridInsideV54(int x, int z, int f)
        { return f >= -1 && f <= 1 && x >= -45 && x <= 42 && z >= -42 && z <= 42; }
        bool GuidanceNodeClearV54(int key)
        {
            if (guidanceClearV54.TryGetValue(key, out bool clear)) return clear;
            Vector3 p = GuidancePointV54(key);
            bool support = key < 0 || GuidanceHasSupportV54(p);
            clear = support && GuidanceBodyClearV54(p, p, true, out _);
            guidanceClearV54[key] = clear; return clear;
        }
        bool GuidanceHasSupportV54(Vector3 p)
        {
            int n = Physics.RaycastNonAlloc(p + Vector3.up * .22f, Vector3.down, guidanceHitsV54, .5f, ~(1 << 30), QueryTriggerInteraction.Ignore);
            if (n >= guidanceHitsV54.Length) return false;
            for (int i = 0; i < n; i++)
            {
                var h = guidanceHitsV54[i];
                if (!h.collider || GuidanceIgnoreActorV54(h.collider) || h.normal.y < .65f) continue;
                if (Mathf.Abs(h.point.y - (p.y - .045f)) <= .17f) return true;
            }
            return false;
        }
        bool GuidanceIgnoreActorV54(Collider c)
        { return c == controller || c.transform.IsChildOf(player) || c.gameObject.layer == 28 || c.gameObject.layer == 30; }
        bool GuidanceIsTreadV54(Collider c)
        { return c.name.StartsWith("Step ", StringComparison.Ordinal) && c.transform.parent && c.transform.parent.name.StartsWith("Stair flight ", StringComparison.Ordinal); }
        bool GuidanceBodyClearV54(Vector3 a, Vector3 b, bool allowDoors, out string door)
        {
            door = null;
            float radius = controller ? controller.radius + .035f : .285f;
            float height = controller ? controller.height : 1.78f;
            float distance = Vector3.Distance(a, b); int samples = Mathf.Max(1, Mathf.CeilToInt(distance / .18f));
            if (samples > 36) return false;
            for (int s = 0; s <= samples; s++)
            {
                Vector3 p = Vector3.Lerp(a, b, s / (float)samples);
                int count = Physics.OverlapCapsuleNonAlloc(p + Vector3.up * (radius + .055f), p + Vector3.up * (height - radius), radius,
                    guidanceOverlapV54, ~(1 << 30), QueryTriggerInteraction.Ignore);
                if (count >= guidanceOverlapV54.Length) return false;
                for (int i = 0; i < count; i++)
                {
                    var c = guidanceOverlapV54[i];
                    if (!c || GuidanceIgnoreActorV54(c) || GuidanceIsTreadV54(c) && c.bounds.max.y <= p.y + .25f) continue;
                    if (guidanceDoorsV54.TryGetValue(c, out var id))
                    { if (allowDoors) continue; door = id; return false; }
                    return false;
                }
            }
            return true;
        }

        void GuidanceLinkV54(int a, int b)
        {
            if (!guidanceStairLinksV54.TryGetValue(a, out var list)) guidanceStairLinksV54[a] = list = new List<int>();
            if (!list.Contains(b)) list.Add(b);
        }
        void GuidanceBuildStairsV54()
        {
            guidanceStairsV54.Clear(); guidanceStairLinksV54.Clear();
            for (int floor = -1; floor <= 0; floor++)
            {
                var flight = GameObject.Find("Stair flight " + floor);
                if (!flight) continue; // No physical flight means no invented vertical edge.
                int lower = GuidanceGridKeyV54(0, Mathf.RoundToInt((PanicHouseLayoutV414.StairSouth - .5f) / GuidanceGridV54), floor);
                int upper = GuidanceGridKeyV54(0, Mathf.RoundToInt((PanicHouseLayoutV414.StairNorth + .7f) / GuidanceGridV54), floor + 1);
                int previous = lower;
                for (int i = 0; i < 16; i++)
                {
                    Transform step = flight.transform.Find("Step " + i);
                    var box = step ? step.GetComponent<BoxCollider>() : null;
                    if (!box) break;
                    int key = -1 - (floor + 1) * 16 - i;
                    guidanceStairsV54[key] = new Vector3(box.bounds.center.x, box.bounds.max.y + .045f, box.bounds.center.z);
                    GuidanceLinkV54(previous, key); GuidanceLinkV54(key, previous); previous = key;
                    if (i == 15) { GuidanceLinkV54(previous, upper); GuidanceLinkV54(upper, previous); }
                }
            }
        }

        int GuidanceStartNodeV54()
        {
            int best = int.MaxValue; float distance = 2.4f;
            foreach (var pair in guidanceStairsV54)
            {
                float d = Vector3.Distance(PlayerPosition, pair.Value);
                if (d < distance && GuidanceNodeClearV54(pair.Key) && GuidanceBodyClearV54(PlayerPosition, pair.Value, true, out _)) { best = pair.Key; distance = d; }
            }
            int f = Mathf.Clamp(Mathf.RoundToInt(PlayerPosition.y / 3.2f), -1, 1);
            int cx = Mathf.RoundToInt(PlayerPosition.x / GuidanceGridV54), cz = Mathf.RoundToInt(PlayerPosition.z / GuidanceGridV54);
            for (int z = cz - 2; z <= cz + 2; z++) for (int x = cx - 2; x <= cx + 2; x++)
            {
                if (!GuidanceGridInsideV54(x, z, f)) continue;
                int key = GuidanceGridKeyV54(x, z, f); Vector3 p = GuidancePointV54(key); float d = Vector3.Distance(PlayerPosition, p);
                if (d < distance && GuidanceNodeClearV54(key) && GuidanceBodyClearV54(PlayerPosition, p, true, out _)) { best = key; distance = d; }
            }
            return best;
        }

        bool GuidanceGoalReachV54(Vector3 p, GuidanceGoalV54 goal)
        {
            if (Mathf.Abs(p.y - goal.Floor * 3.2f) > .4f) return false;
            if (Vector3.Distance(p, goal.Position) > goal.ReachEnvelope + 1.8f) return false;
            Vector3 from = p + Vector3.up * (goal.Action == "collect" ? 1.5f : .9f);
            Vector3 end = goal.Position + Vector3.up * .6f;
            if (goal.IsInteraction && InteractionSurfacePoint(goal.Id, from, out var surface)) end = surface;
            if (Vector3.Distance(from, end) > (goal.Action == "shelter" ? 1.75f : 1.9f)) return false;
            Vector3 delta = end - from; float length = delta.magnitude;
            int n = Physics.RaycastNonAlloc(from, length > .001f ? delta / length : Vector3.forward, guidanceHitsV54, length, ~(1 << 30), QueryTriggerInteraction.Ignore);
            if (n >= guidanceHitsV54.Length) return false;
            var target = InteractionObject(goal.Id);
            for (int i = 0; i < n; i++)
            {
                var c = guidanceHitsV54[i].collider;
                if (!c || GuidanceIgnoreActorV54(c) || target && (c.transform == target.transform || c.transform.IsChildOf(target.transform))) continue;
                return false;
            }
            return true;
        }

        float GuidanceHeuristicV54(Vector3 point, List<GuidanceGoalV54> goals)
        {
            float result = float.MaxValue;
            foreach (var goal in goals)
                result = Mathf.Min(result, Mathf.Max(0, Vector3.Distance(point, goal.Position) - goal.ReachEnvelope - 1.8f));
            return result;
        }
        bool GuidanceEdgeClearV54(int a, int b)
        {
            // Signed stair keys are offset so both directions share one collision receipt.
            uint u = (uint)(Mathf.Min(a, b) + 64), v = (uint)(Mathf.Max(a, b) + 64);
            long key = ((long)u << 32) | v;
            if (guidanceEdgesV54.TryGetValue(key, out bool clear)) return clear;
            clear = GuidanceBodyClearV54(GuidancePointV54(a), GuidancePointV54(b), true, out _);
            guidanceEdgesV54[key] = clear; return clear;
        }
        System.Collections.IEnumerator GuidancePlanV54(List<GuidanceGoalV54> goals)
        {
            guidanceSearchRouteV54 = null; guidanceSearchGoalV54 = null; guidanceSearchLengthV54 = 0; guidancePlansV54++;
            int start = GuidanceStartNodeV54(); if (start == int.MaxValue) yield break;
            var search = new Dictionary<int, GuidanceSearchV54>(); var open = new List<GuidanceSearchV54>();
            var first = new GuidanceSearchV54 { Key = start, Parent = int.MaxValue, Cost = 0, Estimate = GuidanceHeuristicV54(GuidancePointV54(start), goals) };
            search[start] = first; open.Add(first);
            int expanded = 0; GuidanceSearchV54 reached = null;
            var neighbors = new List<int>(10);
            var slice = System.Diagnostics.Stopwatch.StartNew(); int sliceNodes = 0;
            while (open.Count > 0 && expanded < GuidanceMaxExpandedV54)
            {
                // Physics stays on the Unity thread. Suspend the search instead of blocking one frame.
                if (sliceNodes >= 64 || slice.Elapsed.TotalMilliseconds >= 2.0)
                { yield return null; slice.Restart(); sliceNodes = 0; }
                int pick = 0;
                for (int i = 1; i < open.Count; i++) if (open[i].Estimate < open[pick].Estimate || open[i].Estimate == open[pick].Estimate && open[i].Key < open[pick].Key) pick = i;
                var current = open[pick]; open.RemoveAt(pick); if (current.Closed) continue;
                current.Closed = true; expanded++; sliceNodes++; Vector3 p = GuidancePointV54(current.Key);
                foreach (var goal in goals) if (GuidanceGoalReachV54(p, goal)) { reached = current; guidanceSearchGoalV54 = goal; break; }
                if (reached != null) break;
                neighbors.Clear();
                if (current.Key >= 0)
                {
                    GuidanceDecodeV54(current.Key, out int x, out int z, out int floor);
                    for (int dz = -1; dz <= 1; dz++) for (int dx = -1; dx <= 1; dx++)
                        if ((dx != 0 || dz != 0) && GuidanceGridInsideV54(x + dx, z + dz, floor)) neighbors.Add(GuidanceGridKeyV54(x + dx, z + dz, floor));
                }
                if (guidanceStairLinksV54.TryGetValue(current.Key, out var stairLinks)) neighbors.AddRange(stairLinks);
                foreach (int key in neighbors)
                {
                    if (search.TryGetValue(key, out var known) && known.Closed || !GuidanceNodeClearV54(key)) continue;
                    Vector3 q = GuidancePointV54(key);
                    if (!GuidanceEdgeClearV54(current.Key, key)) continue;
                    float cost = current.Cost + Vector3.Distance(p, q);
                    if (known != null && cost >= known.Cost) continue;
                    if (known == null) { known = new GuidanceSearchV54 { Key = key }; search[key] = known; open.Add(known); }
                    known.Cost = cost; known.Parent = current.Key; known.Estimate = cost + GuidanceHeuristicV54(q, goals);
                }
            }
            GuidanceExpandedNodesV54 += expanded;
            if (reached == null) yield break;
            var reverse = new List<Vector3>(); var walk = reached;
            while (walk != null) { reverse.Add(GuidancePointV54(walk.Key)); if (walk.Parent == int.MaxValue) break; walk = search[walk.Parent]; }
            guidanceSearchRouteV54 = new List<Vector3> { guidancePlannedFromV54 };
            for (int i = reverse.Count - 1; i >= 0; i--) guidanceSearchRouteV54.Add(reverse[i]);
            for (int i = 1; i < guidanceSearchRouteV54.Count; i++) guidanceSearchLengthV54 += Vector3.Distance(guidanceSearchRouteV54[i - 1], guidanceSearchRouteV54[i]);
        }

        void GuidanceChooseCheckpointV54()
        {
            GuidanceCheckpointLabelV54 = string.Empty;
            if (!GuidanceRouteValidV54 || guidanceRouteV54.Count == 0) return;
            // Progress follows the actor on the retained route; no reaching/completion flag is written.
            int nearest = 0; float best = float.MaxValue;
            for (int i = 0; i < guidanceRouteV54.Count; i++)
            { float d = (guidanceRouteV54[i] - PlayerPosition).sqrMagnitude; if (d < best) { nearest = i; best = d; } }
            int chosen = nearest;
            for (int i = nearest + 1; i < guidanceRouteV54.Count; i++)
            {
                var p = guidanceRouteV54[i];
                if (Vector3.Distance(PlayerPosition, p) > 3.4f) break;
                if (!GuidanceBodyClearV54(PlayerPosition, p, false, out var door))
                {
                    if (!string.IsNullOrEmpty(door)) GuidanceCheckpointLabelV54 = "Kapıyı aç · E";
                    break;
                }
                chosen = i;
            }
            GuidanceCheckpointPositionV54 = guidanceRouteV54[chosen];
            if (GuidanceCheckpointLabelV54.Length == 0)
            {
                float vertical = GuidanceCheckpointPositionV54.y - PlayerPosition.y;
                if (Mathf.Abs(vertical) > .22f) GuidanceCheckpointLabelV54 = vertical > 0 ? "Merdivenden çık" : "Merdivenden in";
                else if (chosen < guidanceRouteV54.Count - 1) GuidanceCheckpointLabelV54 = "Açık yolu takip et";
                else GuidanceCheckpointLabelV54 = GuidanceObjectiveLabelV54;
            }
        }

        bool GuidanceBeaconVisibleV54(Vector3 p)
        {
            if (!View || Mathf.Abs(p.y - PlayerPosition.y) > 1.1f) return false;
            Vector3 delta = p - View.transform.position;
            int n = Physics.RaycastNonAlloc(View.transform.position, delta.normalized, guidanceHitsV54, Mathf.Max(0, delta.magnitude - .12f), ~(1 << 30), QueryTriggerInteraction.Ignore);
            if (n >= guidanceHitsV54.Length) return false;
            for (int i = 0; i < n; i++) if (guidanceHitsV54[i].collider && !GuidanceIgnoreActorV54(guidanceHitsV54[i].collider)) return false;
            return true;
        }
        void GuidanceUpdateBeaconV54()
        {
            if (!guidanceBeaconV54) return;
            Vector3 p = GuidanceCheckpointPositionV54 + Vector3.up * .10f;
            guidanceBeaconV54.enabled = GuidanceVisibleV54 && GuidanceRouteValidV54 && GuidanceBeaconVisibleV54(p + Vector3.up * .18f);
            if (!guidanceBeaconV54.enabled) return;
            guidanceBeaconV54.positionCount = 22;
            float radius = .24f + .025f * Mathf.Sin(Time.unscaledTime * 3f);
            for (int i = 0; i <= 18; i++)
            { float angle = i * Mathf.PI * 2 / 18; guidanceBeaconV54.SetPosition(i, p + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius)); }
            guidanceBeaconV54.SetPosition(19, p); guidanceBeaconV54.SetPosition(20, p + Vector3.up * .52f);
            guidanceBeaconV54.SetPosition(21, p + new Vector3(.10f, .42f, 0));
        }

        // Hook from exactly one OnGUI owner in both views, before any bag overlay early return.
        public void DrawGuidanceV54()
        {
            if (!GuidanceVisibleV54 || !GuidanceCanShowV54())
            { if (guidanceBeaconV54) guidanceBeaconV54.enabled = false; return; }
            Matrix4x4 matrix = GUI.matrix; Color color = GUI.color; int depth = GUI.depth;
            GUI.matrix = Matrix4x4.identity; GUI.depth = -16;
            if (guidanceTextV54 == null)
            {
                guidanceTextV54 = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true, padding = new RectOffset(10, 10, 7, 4) };
                guidanceTextV54.normal.textColor = new Color(.92f, .96f, .95f);
                guidanceSmallV54 = new GUIStyle(guidanceTextV54) { fontSize = 12, padding = new RectOffset(10, 10, 0, 4) };
                guidanceSmallV54.normal.textColor = new Color(.38f, .91f, .81f);
                guidanceArrowV54 = new GUIStyle(guidanceSmallV54) { fontSize = 24, padding = new RectOffset(), alignment = TextAnchor.MiddleCenter };
            }
            float width = Mathf.Min(320, Screen.width - 32);
            Rect card = new Rect(16, Screen.height - 140, width, 70);
            GUI.color = new Color(.035f, .075f, .09f, .91f); GUI.DrawTexture(card, Texture2D.whiteTexture); GUI.color = Color.white;
            GUI.Label(new Rect(card.x, card.y, width, 40), GuidanceObjectiveLabelV54, guidanceTextV54);
            string hint = GuidanceRouteValidV54 ? GuidanceCheckpointLabelV54 + " · " + Vector3.Distance(PlayerPosition, GuidanceCheckpointPositionV54).ToString("0.0") + " m" : string.Empty;
            GUI.Label(new Rect(card.x, card.y + 43, width, 25), hint, guidanceSmallV54);
            if (GuidanceRouteValidV54)
            {
                Vector3 projected = View.WorldToScreenPoint(GuidanceCheckpointPositionV54 + Vector3.up * .55f);
                Vector2 center = new Vector2(Screen.width * .5f, Screen.height * .5f);
                Vector2 dir = new Vector2(projected.x - center.x, Screen.height - projected.y - center.y);
                if (projected.z < 0) dir = -dir;
                if (dir.sqrMagnitude < 4) dir = Vector2.up;
                Vector2 pos = new Vector2(Mathf.Clamp(center.x + dir.x, 30, Screen.width - 30), Mathf.Clamp(center.y + dir.y, 100, Screen.height - 170));
                GUIUtility.RotateAroundPivot(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, pos);
                GUI.Label(new Rect(pos.x - 12, pos.y - 12, 26, 26), ">", guidanceArrowV54);
            }
            GUI.matrix = matrix; GUI.color = color; GUI.depth = depth;
        }
    }
}
