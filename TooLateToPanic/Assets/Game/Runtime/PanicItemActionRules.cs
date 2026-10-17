using System;

namespace TooLateToPanic
{
    /// <summary>Shared preparation ownership for HUD, keyboard adapters and authoritative work validation.</summary>
    public static class PanicItemActionRules
    {
        public const string FireStationId = "fire_station";
        public static bool CanReleaseAnchor(PanicItem item)
        { return item != null && item.Installed && item.Anchored && item.AnchorOwned && CanAnchor(item); }
        public static string PrimaryAction(PanicItem item)
        {
            if (item == null || !item.Installed) return string.Empty;
            if (IsStructure(item)) return item.Tier < 2 ? "upgrade" : string.Empty;
            if (!CanAnchor(item) || item.Anchored) return string.Empty;
            return NearWall(item) ? "anchorwall" : "anchorfloor";
        }
        public static string PrimaryLabel(PanicItem item)
        {
            string action = PrimaryAction(item);
            return action == "upgrade" ? "GÜÇLENDİR" : action == "anchorwall" || action == "anchorfloor" ? "SABİTLE" : string.Empty;
        }
        public static int PrimaryShortcut(PanicItem item)
        { string action = PrimaryAction(item); return action == "upgrade" ? 3 : action.Length > 0 ? 1 : 0; }
        public static bool Allows(string action, PanicItem item, string targetId)
        {
            if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(targetId)) return false;
            if (action == "fire") return item == null && targetId == FireStationId;
            if (action == "plant") return item == null && (targetId == "plant_west1" || targetId == "plant_west2" || targetId == "plant_east");
            if (action == "drain") return item == null && (targetId == "drain_west" || targetId == "drain_east");
            if (action == "fence") return item == null && (targetId == "fence_west" || targetId == "fence_east" || targetId == "fence_north");
            if (action == "hose") return item == null && targetId == "hose_station";
            if (item == null || item.Id != targetId || !item.Installed) return false;
            if (action == "upgrade") return IsStructure(item) && item.Tier < 2;
            if (action == "anchorwall" || action == "anchorfloor")
                return CanAnchor(item) && !item.Anchored && (action != "anchorwall" || NearWall(item));
            // No arbitrary furniture fire, generic protection or hidden flood shortcut.
            return false;
        }
        static bool IsStructure(PanicItem item)
        { return item.Kind == ItemKind.Wall || item.Kind == ItemKind.Window || item.Kind == ItemKind.Door || item.Kind == ItemKind.Roof; }
        static bool NearWall(PanicItem item)
        { return Math.Abs(item.Position.x) > 6.3f || Math.Abs(item.Position.z) > 4.5f || Math.Abs(item.Position.x - 2f) < 1.1f; }
        static bool CanAnchor(PanicItem item)
        {
            if (item.Kind != ItemKind.Furniture || !item.Movable || item.Id == "bunker" || item.Id == "bagstation") return false;
            string family = item.ArtId ?? string.Empty;
            return family == "bookshelf" || family == "fridge" || family == "cabinet" || family == "kitchencabinet" ||
                family == "tvstand" || family == "locker" || family == "storagerack";
        }
    }
}
