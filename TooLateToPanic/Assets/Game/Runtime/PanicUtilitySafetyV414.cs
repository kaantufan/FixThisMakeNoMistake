using System;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Run-owned emergency isolation. Physical work commits these flags through Expedition only.</summary>
    [Serializable]
    public sealed class PanicUtilitySafetyState
    {
        public bool GasClosed { get; private set; }
        public bool WaterClosed { get; private set; }
        public bool PowerOff { get; private set; }
        public int CompletedCount { get { return (GasClosed ? 1 : 0) + (WaterClosed ? 1 : 0) + (PowerOff ? 1 : 0); } }

        public void Reset() { GasClosed = WaterClosed = PowerOff = false; }
        public bool IsComplete(string action)
        { return action == "gasoff" ? GasClosed : action == "wateroff" ? WaterClosed : action == "poweroff" && PowerOff; }

        // Internal is intentional: normal presentation cannot publish a fake completed HUD state.
        internal void Complete(string action)
        {
            if (action == "gasoff") GasClosed = true;
            else if (action == "wateroff") WaterClosed = true;
            else if (action == "poweroff") PowerOff = true;
        }

        /// <summary>
        /// Isolation limits secondary appliance/contents damage. It does not make the
        /// structure earthquake-proof, stop exterior flood water or change the loss limit.
        /// Called on real per-item risk before the model's existing normalization.
        /// </summary>
        public float DamageMultiplier(DisasterType disaster, PanicItem item)
        {
            if (item == null || !item.Installed || item.Kind != ItemKind.Furniture ||
                item.Id == "bunker" || item.Id == "bagstation") return 1f;
            float result = 1f;
            if (GasClosed)
            {
                if (disaster == DisasterType.Fire) result *= .90f;
                else if (disaster == DisasterType.Earthquake) result *= .95f;
            }
            if (WaterClosed && disaster == DisasterType.Flood) result *= .93f;
            if (PowerOff && (disaster == DisasterType.Fire || disaster == DisasterType.Flood)) result *= .92f;
            return result;
        }
    }

    public static class PanicUtilitySafetyV414
    {
        public const string GasId = "utility_gas";
        public const string WaterId = "utility_water";
        public const string PowerId = "utility_power";
        public static bool IsAction(string action)
        { return action == "gasoff" || action == "wateroff" || action == "poweroff"; }
        public static string ActionForTarget(string id)
        { return id == GasId ? "gasoff" : id == WaterId ? "wateroff" : id == PowerId ? "poweroff" : string.Empty; }
        public static string TargetForAction(string action)
        { return action == "gasoff" ? GasId : action == "wateroff" ? WaterId : action == "poweroff" ? PowerId : string.Empty; }
        public static bool Allows(string action, string target)
        { return IsAction(action) && TargetForAction(action) == target; }
        public static string Label(string action)
        { return action == "gasoff" ? "Gaz vanasını kapat" : action == "wateroff" ? "Su vanasını kapat" : "Elektrik şalterini indir"; }
        public static string TargetLabel(string id)
        { return id == GasId ? "Gaz vanası" : id == WaterId ? "Ana su vanası" : id == PowerId ? "Elektrik panosu" : string.Empty; }
        public static string StateLabel(string action, bool complete)
        { return action == "poweroff" ? complete ? "ELEKTRİK KESİK" : "ELEKTRİK AÇIK" : complete ? "VANA KAPALI" : "VANA AÇIK"; }
        public static string Effect(string action)
        {
            return action == "gasoff" ? "Gaz alevlenmesi riskini önler; deprem ve yangında eşya hasarını azaltır." :
                action == "wateroff" ? "Tesisat sızıntısı riskini önler; selde eşya hasarını azaltır." :
                "Ev elektriğini ve kısa devre riskini keser; sel ve yangında eşya hasarını azaltır.";
        }
    }
}
