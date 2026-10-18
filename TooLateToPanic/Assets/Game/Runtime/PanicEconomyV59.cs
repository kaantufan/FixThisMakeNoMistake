using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Campaign cash and authored construction stock. No unlocks, rerolls or free work.</summary>
    public static class PanicEconomyV59
    {
        public const int StartingBudget = 1500;

        // The complete residence has 9705 budget of persistent work. Fund it
        // across successful waves. The existing actual-damage deduction and support bonus
        // apply afterwards; failure, simulation and developer protection pay nothing.
        public static float BaseWaveReward(int wave)
        { return 1600f + 100f * Mathf.Max(0f, (float)wave - 1f); }

        public static int UpgradeBudgetCost(PanicItem item)
        {
            if (item == null || item.Tier >= 2) return 0;
            return item.Tier == 0 ? 30 : item.Kind == ItemKind.Wall ? 375 : 200;
        }

        // Supply is generated once at wave restock by the retained finite RNG. This
        // adds two useful units per existing screw/plank stack, without extra searches,
        // containers, inventory slots, purchase shortcuts or a hidden wave gate.
        public static int AuthoredConstructionStock(string id, int count)
        { return count + (id == "screws" || id == "plank" ? 2 : 0); }
    }
}
