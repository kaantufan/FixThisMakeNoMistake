using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    // Cinematic-only path construction. Never moves a collider or changes player dimensions.
    public sealed partial class PanicHomeWorld
    {
        readonly List<Vector3> bunkerApproachRouteV414 = new List<Vector3>();
        public float BunkerApproachRouteLengthV414 { get; private set; }
        public bool BunkerApproachRouteValidV414 { get; private set; }
        public int BunkerApproachWaypointCountV414 { get { return bunkerApproachRouteV414.Count; } }

        struct BunkerRouteRectV414
        {
            public float left, right, south, north;
            public BunkerRouteRectV414(float l, float r, float s, float n)
            { left = l; right = r; south = s; north = n; }
            public bool Contains(Vector3 p)
            { return p.x >= left && p.x <= right && p.z >= south && p.z <= north; }
        }

        public bool BuildBunkerApproachRouteV414(Vector3 start, Vector3 end)
        {
            bunkerApproachRouteV414.Clear();
            bunkerApproachRouteV414.Add(start);
            BunkerApproachRouteLengthV414 = 0;
            BunkerApproachRouteValidV414 = false;
            float radius = controller ? controller.radius * Mathf.Max(Mathf.Abs(controller.transform.lossyScale.x), Mathf.Abs(controller.transform.lossyScale.z)) : .25f;
            float feet = start.y + (controller ? controller.center.y - controller.height * .5f : .01f);
            float head = start.y + (controller ? controller.center.y + controller.height * .5f : 1.79f);
            var collar = new BunkerRouteRectV414(BunkerPosition.x - 1.15f, BunkerPosition.x + 1.15f, BunkerPosition.z - 1.05f, BunkerPosition.z + 1.05f);
            var obstacles = new List<BunkerRouteRectV414>();
            Physics.SyncTransforms();
            items.TryGetValue("bunker", out var hatch);
            residenceTargets.TryGetValue("@bunker", out var extraHatch);
            foreach (var c in FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if (!c || !c.enabled || c.isTrigger || c == controller || (player && c.transform.IsChildOf(player))) continue;
                if ((hatch && (c.transform == hatch.transform || c.transform.IsChildOf(hatch.transform))) ||
                    (extraHatch && (c.transform == extraHatch.transform || c.transform.IsChildOf(extraHatch.transform)))) continue;
                Bounds b = c.bounds;
                // Only actual blockers intersecting the upright actor's height and basement footprint.
                // Low floor slabs/rims are handled separately; ceilings cannot block this ground route.
                if (b.max.y <= feet + .03f || b.min.y >= head || b.max.x < -8 || b.min.x > 8 || b.max.z < -6 || b.min.z > 6) continue;
                float pad = radius + .005f;
                obstacles.Add(new BunkerRouteRectV414(b.min.x - pad, b.max.x + pad, b.min.z - pad, b.max.z + pad));
            }
            var candidates = new List<Vector3>();
            if (collar.Contains(start))
            {
                // Preserve the source point. Escape outward, never snap to an invented corner.
                candidates.Add(new Vector3(collar.left - .03f, start.y, start.z));
                candidates.Add(new Vector3(collar.right + .03f, start.y, start.z));
                candidates.Add(new Vector3(start.x, start.y, collar.south - .03f));
                candidates.Add(new Vector3(start.x, start.y, collar.north + .03f));
                candidates.Sort((a, b) => (a - start).sqrMagnitude.CompareTo((b - start).sqrMagnitude));
            }
            else candidates.Add(start);

            foreach (var escape in candidates)
            {
                if ((escape - start).sqrMagnitude > .000001f &&
                    (!BunkerRouteFurnitureClearV414(start, escape, obstacles) || !BunkerRoutePhysicalCollarClearV414(start, escape, radius))) continue;
                if (!BunkerRoutePointClearV414(escape, collar, obstacles) || !BunkerRoutePointClearV414(end, collar, obstacles)) continue;
                var nodes = new List<Vector3> { escape, end };
                foreach (float x in new[] { -1.22f, 1.22f }) foreach (float z in new[] { -1.18f, 1.18f })
                    BunkerRouteAddNodeV414(nodes, BunkerPosition + new Vector3(x, start.y - BunkerPosition.y, z), collar, obstacles);
                foreach (var o in obstacles)
                {
                    const float gap = .015f;
                    BunkerRouteAddNodeV414(nodes, new Vector3(o.left - gap, start.y, o.south - gap), collar, obstacles);
                    BunkerRouteAddNodeV414(nodes, new Vector3(o.right + gap, start.y, o.south - gap), collar, obstacles);
                    BunkerRouteAddNodeV414(nodes, new Vector3(o.left - gap, start.y, o.north + gap), collar, obstacles);
                    BunkerRouteAddNodeV414(nodes, new Vector3(o.right + gap, start.y, o.north + gap), collar, obstacles);
                }
                int n = nodes.Count;
                var distance = new float[n]; var previous = new int[n]; var visited = new bool[n];
                for (int i = 0; i < n; i++) { distance[i] = float.PositiveInfinity; previous[i] = -1; }
                distance[0] = 0;
                for (int pass = 0; pass < n; pass++)
                {
                    int u = -1;
                    for (int i = 0; i < n; i++) if (!visited[i] && (u < 0 || distance[i] < distance[u])) u = i;
                    if (u < 0 || float.IsPositiveInfinity(distance[u])) break;
                    if (u == 1) break;
                    visited[u] = true;
                    for (int v = 0; v < n; v++)
                    {
                        if (visited[v] || v == u || BunkerRouteCrossesV414(nodes[u], nodes[v], collar) || !BunkerRouteFurnitureClearV414(nodes[u], nodes[v], obstacles)) continue;
                        float d = distance[u] + Vector3.Distance(nodes[u], nodes[v]);
                        if (d < distance[v]) { distance[v] = d; previous[v] = u; }
                    }
                }
                if (float.IsPositiveInfinity(distance[1])) continue;
                var reverse = new List<Vector3>();
                for (int i = 1; i >= 0; i = previous[i]) reverse.Add(nodes[i]);
                for (int i = reverse.Count - 1; i >= 0; i--)
                    if ((bunkerApproachRouteV414[bunkerApproachRouteV414.Count - 1] - reverse[i]).sqrMagnitude > .000001f) bunkerApproachRouteV414.Add(reverse[i]);
                for (int i = 1; i < bunkerApproachRouteV414.Count; i++) BunkerApproachRouteLengthV414 += Vector3.Distance(bunkerApproachRouteV414[i - 1], bunkerApproachRouteV414[i]);
                BunkerApproachRouteValidV414 = true;
                return true;
            }
            // A failed route samples the real start only. Caller must not continue the jump on false.
            return false;
        }

        public Vector3 SampleBunkerApproachRouteV414(float t)
        {
            if (bunkerApproachRouteV414.Count == 0) return PlayerPosition;
            float remaining = Mathf.Clamp01(t) * BunkerApproachRouteLengthV414;
            for (int i = 1; i < bunkerApproachRouteV414.Count; i++)
            {
                float length = Vector3.Distance(bunkerApproachRouteV414[i - 1], bunkerApproachRouteV414[i]);
                if (remaining <= length) return Vector3.Lerp(bunkerApproachRouteV414[i - 1], bunkerApproachRouteV414[i], length > .000001f ? remaining / length : 1);
                remaining -= length;
            }
            return bunkerApproachRouteV414[bunkerApproachRouteV414.Count - 1];
        }

        static void BunkerRouteAddNodeV414(List<Vector3> nodes, Vector3 p, BunkerRouteRectV414 collar, List<BunkerRouteRectV414> obstacles)
        {
            // Reject wall exterior corners. These bounds retain a radius-safe basement floor strip.
            if (p.x < -7.70f || p.x > 7.70f || p.z < -5.70f || p.z > 5.70f || !BunkerRoutePointClearV414(p, collar, obstacles)) return;
            foreach (var old in nodes) if ((old - p).sqrMagnitude < .000001f) return;
            nodes.Add(p);
        }
        static bool BunkerRoutePointClearV414(Vector3 p, BunkerRouteRectV414 collar, List<BunkerRouteRectV414> obstacles)
        {
            if (collar.Contains(p)) return false;
            foreach (var o in obstacles) if (o.Contains(p)) return false;
            return true;
        }
        static bool BunkerRouteFurnitureClearV414(Vector3 a, Vector3 b, List<BunkerRouteRectV414> obstacles)
        { foreach (var o in obstacles) if (BunkerRouteCrossesV414(a, b, o)) return false; return true; }
        static bool BunkerRouteCrossesV414(Vector3 a, Vector3 b, BunkerRouteRectV414 r)
        {
            float lo = 0, hi = 1;
            return BunkerRouteSlabV414(a.x, b.x - a.x, r.left, r.right, ref lo, ref hi) &&
                BunkerRouteSlabV414(a.z, b.z - a.z, r.south, r.north, ref lo, ref hi);
        }
        static bool BunkerRouteSlabV414(float origin, float delta, float min, float max, ref float lo, ref float hi)
        {
            if (Mathf.Abs(delta) < .000001f) return origin >= min && origin <= max;
            float a = (min - origin) / delta, b = (max - origin) / delta;
            if (a > b) { float swap = a; a = b; b = swap; }
            lo = Mathf.Max(lo, a); hi = Mathf.Min(hi, b); return lo <= hi;
        }
        static bool BunkerRoutePhysicalCollarClearV414(Vector3 a, Vector3 b, float radius)
        {
            // Actual outer collar rectangle, with a circular CC offset (not an over-wide square).
            var r = new BunkerRouteRectV414(BunkerPosition.x - .85f, BunkerPosition.x + .85f, BunkerPosition.z - .75f, BunkerPosition.z + .75f);
            if (BunkerRouteCrossesV414(a, b, r)) return false;
            float best = Mathf.Min(BunkerRoutePointRectDistanceV414(a, r), BunkerRoutePointRectDistanceV414(b, r));
            foreach (float x in new[] { r.left, r.right }) foreach (float z in new[] { r.south, r.north })
            {
                var p = new Vector2(x, z); var from = new Vector2(a.x, a.z); var delta = new Vector2(b.x - a.x, b.z - a.z);
                float t = delta.sqrMagnitude > .000001f ? Mathf.Clamp01(Vector2.Dot(p - from, delta) / delta.sqrMagnitude) : 0;
                best = Mathf.Min(best, Vector2.Distance(p, from + delta * t));
            }
            return best >= radius - .001f;
        }
        static float BunkerRoutePointRectDistanceV414(Vector3 p, BunkerRouteRectV414 r)
        { return new Vector2(Mathf.Max(r.left - p.x, 0, p.x - r.right), Mathf.Max(r.south - p.z, 0, p.z - r.north)).magnitude; }
    }
}
