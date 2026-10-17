using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        sealed class UtilityStationV414
        { public string Id; public Vector3 Position; public float Yaw; public GameObject Root; public bool Closed; }
        readonly Dictionary<string, UtilityStationV414> utilityStationsV414 = new Dictionary<string, UtilityStationV414>();

        /// <summary>Ground-origin placement. Root/layout may override any station before/after initialization.</summary>
        public bool ConfigureUtilityStationV414(string id, Vector3 groundPosition, float yaw)
        {
            if (string.IsNullOrEmpty(PanicUtilitySafetyV414.ActionForTarget(id))) return false;
            UtilityStationV414 station;
            if (!utilityStationsV414.TryGetValue(id, out station))
                utilityStationsV414[id] = station = new UtilityStationV414 { Id = id };
            station.Position = groundPosition; station.Yaw = yaw;
            RebuildUtilityStationV414(station); return true;
        }
        void InitUtilityStationsV414()
        {
            if (!utilityStationsV414.ContainsKey(PanicUtilitySafetyV414.GasId)) ConfigureUtilityStationV414(PanicUtilitySafetyV414.GasId, new Vector3(7.45f, 0, .25f), 90);
            if (!utilityStationsV414.ContainsKey(PanicUtilitySafetyV414.WaterId)) ConfigureUtilityStationV414(PanicUtilitySafetyV414.WaterId, new Vector3(7.45f, 0, -1.10f), 90);
            if (!utilityStationsV414.ContainsKey(PanicUtilitySafetyV414.PowerId)) ConfigureUtilityStationV414(PanicUtilitySafetyV414.PowerId, new Vector3(9.6f, 0, 5.85f), 0);
        }
        void RefreshUtilityStationsV414(bool force = false)
        {
            if (Model == null) return;
            foreach (var station in utilityStationsV414.Values)
            {
                string action = PanicUtilitySafetyV414.ActionForTarget(station.Id);
                bool closed = Model.UtilitySafety.IsComplete(action);
                if (force || !station.Root || closed != station.Closed) RebuildUtilityStationV414(station);
            }
        }
        void RebuildUtilityStationV414(UtilityStationV414 station)
        {
            if (Model == null) return;
            if (station.Root)
            {
                floorItems.Remove(station.Root); visibilityRenderers.Remove(station.Root.GetInstanceID());
                // Destroy is deferred; remove the old blocking/picking surface this frame.
                foreach (var oldCollider in station.Root.GetComponentsInChildren<Collider>()) oldCollider.enabled = false;
                Destroy(station.Root);
            }
            string action = PanicUtilitySafetyV414.ActionForTarget(station.Id);
            station.Closed = Model.UtilitySafety.IsComplete(action);
            station.Root = PanicWorkArt.CreateUtilityStation(action, station.Closed);
            station.Root.name = station.Id;
            station.Root.transform.SetPositionAndRotation(station.Position, Quaternion.Euler(0, station.Yaw, 0));
            var collider = station.Root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, .70f, 0); collider.size = new Vector3(.52f, .94f, .20f);
            var pick = station.Root.AddComponent<HomePick>(); pick.Id = station.Id; pick.Kind = "utility";
            residenceTargets[station.Id] = station.Root; floorItems.Add(station.Root);
        }
        public bool TryUtilityStationPositionV414(string id, out Vector3 position)
        { UtilityStationV414 station; bool found = utilityStationsV414.TryGetValue(id ?? string.Empty, out station); position = found ? station.Position : Vector3.zero; return found; }
        public bool UtilityStationClosedV414(string id)
        { string action = PanicUtilitySafetyV414.ActionForTarget(id); return Model != null && action.Length > 0 && Model.UtilitySafety.IsComplete(action); }

        /// <summary>Call inside the normal World OnGUI visibility/input guard.</summary>
        void DrawUtilityLabelsV414()
        {
            if (Model == null || !player) return;
            foreach (var station in utilityStationsV414.Values)
            {
                if (!station.Root || !SameLevel(station.Position) || Vector3.Distance(player.position, station.Position) > 4.5f) continue;
                string action = PanicUtilitySafetyV414.ActionForTarget(station.Id);
                DrawMarker(station.Position + Vector3.up * 1.35f,
                    PanicUtilitySafetyV414.TargetLabel(station.Id) + " · " + PanicUtilitySafetyV414.StateLabel(action, station.Closed), 270);
            }
        }

        /// <summary>Call before ordinary UpdateResidenceContext. Existing interaction gate owns distance/path checks.</summary>
        bool UpdateUtilityContextV414()
        {
            string action = PanicUtilitySafetyV414.ActionForTarget(focusId);
            if (action.Length == 0) return false;
            bool closed = Model.UtilitySafety.IsComplete(action);
            FocusLabel = PanicUtilitySafetyV414.TargetLabel(focusId) + " · " + PanicUtilitySafetyV414.StateLabel(action, closed);
            ShortAction = closed || BagFinalStageActive ? string.Empty : PanicUtilitySafetyV414.Label(action);
            ContextHint = closed ? "Güvenli durumda · " + PanicUtilitySafetyV414.Effect(action) :
                BagFinalStageActive ? "Önlem aşaması bitti." : CanInteract ? "E · " + ShortAction : "Hedefe yaklaş.";
            return true;
        }
        /// <summary>Call in Interact after its ordinary CanInteract/work/movement gate.</summary>
        bool TryInteractUtilityV414()
        {
            string action = PanicUtilitySafetyV414.ActionForTarget(focusId);
            if (action.Length == 0) return false;
            if (!Model.UtilitySafety.IsComplete(action)) BeginWork(action, focusId);
            else Model.Notify(PanicUtilitySafetyV414.StateLabel(action, true) + " · " + PanicUtilitySafetyV414.Effect(action));
            return true;
        }
    }
}
