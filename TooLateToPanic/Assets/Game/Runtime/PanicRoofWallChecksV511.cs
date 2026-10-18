using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in production Model/Expedition checks. Host must provide an isolated ScorePathOverride.
    /// Funded budget, explicit tier states and the named stale/shortage injections are setup fixtures;
    /// work progress and transactions use production APIs. No World, controller, OS or human acceptance.
    /// </summary>
    public static class PanicRoofWallChecksV511
    {
        static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        public static void Run(Action<string, bool, string> check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride))
                throw new InvalidOperationException("Isolated score path required.");
            Scenario(check, "lower", () => LowerFloors(check));
            Scenario(check, "upper", () => UpperProgress(check));
            Scenario(check, "cancel", () => Cancellation(check));
            Scenario(check, "shortage", () => Shortages(check));
            Scenario(check, "stale", () => StaleRoof(check));
            Scenario(check, "mixed", () => MixedTiers(check));
            Scenario(check, "members", () => RoofMembers(check));
            Scenario(check, "direct", () => DirectRoofDenied(check));
        }

        static void LowerFloors(Action<string, bool, string> check)
        {
            foreach (int floor in new[] { -1, 0 })
            {
                var e = New(51100 + floor); string id = Walls(e, floor)[0].Id;
                var q = Quote(e, id, 1, Walls(e, floor), new PanicItem[0], "lower." + floor, check);
                if (q == null) continue;
                check("lower." + floor + ".roof-excluded", q.RoofCount == 0 && q.ChangedRoofCount == 0 && q.RoofIds.Count == 0,
                    "Ground/basement quote retains wall-only membership and cost.");
                Commit(e, id, q, Walls(e, floor), "lower." + floor, check);
            }
        }

        static void UpperProgress(Action<string, bool, string> check)
        {
            var e = New(51110); string id = Walls(e, 1)[0].Id;
            for (int tier = 1; tier <= 2; tier++)
            {
                string key = "upper.t" + tier;
                var q = Quote(e, id, tier, Walls(e, 1), Roofs(e), key, check);
                if (q == null) return;
                var selected = e.Model.Items.Find(i => i.Id == id);
                check(key + ".wall-entry", e.PrimaryActionV414(selected) == "upgrade" && e.PrimaryShortcutV414(selected) == 3,
                    "The upper wall is the available player-facing work entry.");
                Commit(e, id, q, Walls(e, 1).Concat(Roofs(e)).ToArray(), key, check);
            }
            PanicFloorWallUpgradePlanV414 exhausted;
            var before = new Snapshot(e);
            check("upper.max-denied", !e.TryGetFloorWallUpgradeV414(id, out exhausted) &&
                !e.BeginValidatedWorkV414("upgrade", id, false) && before.Unchanged(e),
                "Maxed walls plus maxed roof cannot start or charge another work item.");
        }

        static void Cancellation(Action<string, bool, string> check)
        {
            var e = New(51120); string id = Walls(e, 1)[0].Id; var before = new Snapshot(e);
            bool began = e.BeginValidatedWorkV414("upgrade", id, false);
            check("cancel.begin-free", began && e.ActiveFloorWallUpgradeV414 != null && before.Unchanged(e),
                "Beginning the bundled work spends neither stock nor budget and upgrades no member.");
            bool partial = began && e.Work.PressHammer() && !e.Work.Complete;
            e.Work.Cancel(); e.CancelFloorWallUpgradeV414();
            check("cancel.partial-free", partial && before.Unchanged(e) && !e.Work.Active && e.ActiveFloorWallUpgradeV414 == null,
                "One real hammer stroke, followed by production cancellation, remains free for walls and roof.");
            check("cancel.cannot-commit", !e.TryCompleteWork("upgrade", id) && before.Unchanged(e),
                "A canceled bundle cannot commit.");
            var q = Quote(e, id, 1, Walls(e, 1), Roofs(e), "cancel.restart", check);
            if (q != null) Commit(e, id, q, Walls(e, 1).Concat(Roofs(e)).ToArray(), "cancel.restart", check);
        }

        static void Shortages(Action<string, bool, string> check)
        {
            var e = New(51130); string id = Walls(e, 1)[0].Id;
            var q = Quote(e, id, 1, Walls(e, 1), Roofs(e), "shortage.budget", check);
            if (q == null) return;
            e.Model.Budget = q.BudgetCost - 1; var before = new Snapshot(e);
            check("shortage.budget-begin-atomic", !e.BeginValidatedWorkV414("upgrade", id, false) && before.Unchanged(e) && !e.Work.Active,
                "Budget is one below the full wall-plus-roof quote; no partial installation or charge.");
            check("shortage.budget-payload", e.RequirementBudgetCost == q.BudgetCost && e.RequirementBudgetMissing == 1 &&
                SameCosts(e.CurrentRequirements.Select(r => new PanicMaterialCost(r.Id, r.Required)).ToArray(), q.MaterialCosts),
                "Player shortage payload uses the combined quote and the exact one-cash deficit.");

            e = New(51131, false); id = Walls(e, 1)[0].Id;
            q = Quote(e, id, 1, Walls(e, 1), Roofs(e), "shortage.material", check);
            if (q == null) return;
            e.Model.Budget = q.BudgetCost; before = new Snapshot(e);
            check("shortage.material-begin-atomic", !e.BeginValidatedWorkV414("upgrade", id, false) && before.Unchanged(e) && !e.Work.Active,
                "Real empty construction stock rejects the otherwise affordable bundled quote.");
            check("shortage.material-payload", SameCosts(e.CurrentRequirements.Select(r => new PanicMaterialCost(r.Id, r.Required)).ToArray(), q.MaterialCosts) &&
                e.CurrentRequirements.All(r => r.Owned == 0 && r.Missing == r.Required),
                "All combined material deficits are reported from actual stock.");

            foreach (bool budgetShortage in new[] { true, false })
            {
                e = New(budgetShortage ? 51132 : 51133); id = Walls(e, 1)[0].Id;
                q = Quote(e, id, 1, Walls(e, 1), Roofs(e), budgetShortage ? "shortage.late-budget" : "shortage.late-material", check);
                if (q == null) continue;
                bool began = e.BeginValidatedWorkV414("upgrade", id, false); bool complete = Finish(e);
                if (budgetShortage) e.Model.Budget = q.BudgetCost - 1;
                else
                {
                    // Explicit concurrency fixture: material disappears after work progress, before commit.
                    var consume = typeof(PanicExpedition).GetMethod("Consume", Private);
                    if (consume == null) throw new MissingMethodException("PanicExpedition.Consume");
                    consume.Invoke(e, new object[] { q.MaterialCosts[0].Id, int.MaxValue });
                }
                before = new Snapshot(e);
                check(budgetShortage ? "shortage.late-budget-atomic" : "shortage.late-material-atomic",
                    began && complete && !e.TryCompleteWork("upgrade", id) && before.Unchanged(e),
                    "Affordability is revalidated at the actual commit; every tier and remaining resource stays unchanged.");
            }
        }

        static void StaleRoof(Action<string, bool, string> check)
        {
            foreach (string mutation in new[] { "tier", "uninstalled", "removed", "replaced", "added" })
            {
                var e = New(51140); string id = Walls(e, 1)[0].Id; var roof = Roofs(e)[0];
                bool began = e.BeginValidatedWorkV414("upgrade", id, false); bool complete = Finish(e);
                if (mutation == "tier") roof.Tier = 1;
                else if (mutation == "uninstalled") roof.Installed = false;
                else if (mutation == "removed") e.Model.Items.Remove(roof);
                else if (mutation == "replaced")
                {
                    int index = e.Model.Items.IndexOf(roof);
                    e.Model.Items[index] = NewRoof(roof.Id, roof.Tier, true);
                }
                else e.Model.Items.Add(NewRoof("v511_added_roof", 0, true));
                var before = new Snapshot(e);
                check("stale.roof-" + mutation, began && complete && !e.TryCompleteWork("upgrade", id) && before.Unchanged(e),
                    "Explicit post-start roof " + mutation + " fixture: stale reference/tier/member set rejects without charging or changing other members.");
            }
        }

        static void MixedTiers(Action<string, bool, string> check)
        {
            var e = New(51150); var walls = Walls(e, 1); string id = walls[0].Id;
            foreach (var roof in Roofs(e)) roof.Tier = 2;
            var q = Quote(e, id, 1, walls, Roofs(e), "mixed.roof-ahead", check);
            if (q != null)
            {
                check("mixed.roof-ahead-no-charge", q.ChangedRoofCount == 0 && q.ChangedMemberCount == walls.Length,
                    "Already higher roofs stay in the frozen member set without duplicate charge or demotion.");
                Commit(e, id, q, walls.Concat(Roofs(e)).ToArray(), "mixed.roof-ahead", check);
            }

            e = New(51151); walls = Walls(e, 1); id = walls[0].Id;
            foreach (var wall in walls) wall.Tier = 2;
            for (int tier = 1; tier <= 2; tier++)
            {
                q = Quote(e, id, tier, walls, Roofs(e), "mixed.roof-catchup-t" + tier, check);
                if (q == null) return;
                check("mixed.roof-catchup-t" + tier + ".wall-entry", q.ChangedWallCount == 0 && q.ChangedRoofCount == Roofs(e).Length &&
                    e.PrimaryActionV414(walls[0]) == "upgrade" && e.PrimaryShortcutV414(walls[0]) == 3,
                    "Maxed upper wall remains an entry while only the lagging roof needs this tier.");
                Commit(e, id, q, walls.Concat(Roofs(e)).ToArray(), "mixed.roof-catchup-t" + tier, check);
            }

            e = New(51152); walls = Walls(e, 1); id = walls[0].Id;
            walls[0].Tier = 2; walls[1].Tier = 1;
            foreach (var roof in Roofs(e)) roof.Tier = 1;
            q = Quote(e, id, 1, walls, Roofs(e), "mixed.partial-walls", check);
            if (q != null) Commit(e, id, q, walls.Concat(Roofs(e)).ToArray(), "mixed.partial-walls", check);
        }

        static void RoofMembers(Action<string, bool, string> check)
        {
            var e = New(51160); var extra = NewRoof("v511_other_roof", 0, true);
            extra.Position = new Vector3(17, 12.8f, -3); e.Model.Items.Add(extra);
            var uninstalled = NewRoof("v511_uninstalled_roof", 0, false); e.Model.Items.Add(uninstalled);
            string id = Walls(e, 1)[0].Id;
            var q = Quote(e, id, 1, Walls(e, 1), Roofs(e), "members.all-installed", check);
            if (q == null) return;
            check("members.all-installed-not-id-or-floor", q.RoofIds.Contains(extra.Id) && !q.RoofIds.Contains(uninstalled.Id),
                "All installed roof records participate even with a new ID/height; an uninstalled record is excluded.");
            Commit(e, id, q, Walls(e, 1).Concat(Roofs(e)).ToArray(), "members.all-installed", check);

            e = New(51161); id = Walls(e, 1)[0].Id;
            e.Model.Items.RemoveAll(i => i.Kind == ItemKind.Roof); var before = new Snapshot(e);
            PanicFloorWallUpgradePlanV414 missingRoof;
            check("members.missing-roof-denied", !e.TryGetFloorWallUpgradeV414(id, out missingRoof) &&
                !e.BeginValidatedWorkV414("upgrade", id, false) && before.Unchanged(e),
                "Missing required roof cannot silently become an upper-wall-only transaction.");
        }

        static void DirectRoofDenied(Action<string, bool, string> check)
        {
            foreach (bool free in new[] { false, true })
            foreach (int tier in new[] { 0, 1 })
            {
                var e = New(51170 + tier); var m = e.Model; var roof = Roofs(e)[0]; roof.Tier = tier;
                if (free)
                {
                    m.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, true);
                    m.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true);
                    m.Budget = 0;
                }
                string key = "direct." + (free ? "free" : "paid") + ".t" + tier;
                var before = new Snapshot(e);
                check(key + ".no-player-action", string.IsNullOrEmpty(PanicItemActionRules.PrimaryAction(roof)) &&
                    string.IsNullOrEmpty(PanicItemActionRules.PrimaryLabel(roof)) && PanicItemActionRules.PrimaryShortcut(roof) == 0 &&
                    string.IsNullOrEmpty(e.PrimaryActionV414(roof)) && e.PrimaryShortcutV414(roof) == 0 &&
                    new[] { "upgrade", "anchorwall", "anchorfloor", "flood", "fire" }.All(a => !PanicItemActionRules.Allows(a, roof, roof.Id)),
                    "Roof exposes no independent work action or shortcut, including F2 free options.");
                check(key + ".begin-denied", !e.CanBeginWork("upgrade", roof.Id) &&
                    !e.BeginValidatedWorkV414("upgrade", roof.Id, false) && before.Unchanged(e),
                    "Production preflight denies a direct roof target.");
                // Adversarial completion fixture bypasses preflight only to exercise the commit gate itself.
                e.Work.Begin("upgrade", roof.Id, false); bool complete = Finish(e);
                check(key + ".commit-denied", complete && !e.TryCompleteWork("upgrade", roof.Id) && before.Unchanged(e),
                    "Even a completed raw work-session fixture cannot bypass the authoritative direct-roof gate.");
                e.Work.Cancel(); e.CancelFloorWallUpgradeV414();
                check(key + ".model-denied", !m.Upgrade(roof.Id) && before.Unchanged(e),
                    "Legacy player-style Model.Upgrade cannot independently mutate the roof.");
                var recipe = PanicExpedition.MaterialCosts("upgrade", roof);
                check(key + ".raw-economy-retained", PanicExpedition.WorkCost("upgrade", roof) == (tier == 0 ? 30 : 200) &&
                    recipe.Any(c => c.Id == "plank" && c.Required == (tier == 0 ? 1 : 2)) &&
                    recipe.Any(c => c.Id == "screws" && c.Required == (tier == 0 ? 1 : 4)),
                    "Roof keeps its own actual per-item cost for the bundled transaction.");
            }
            var risk = typeof(PanicModel).GetMethod("BaseRisk", BindingFlags.Static | BindingFlags.NonPublic);
            if (risk == null) throw new MissingMethodException("PanicModel.BaseRisk");
            var item = NewRoof("v511_risk_roof", 0, true);
            check("direct.raw-risk-retained", (float)risk.Invoke(null, new object[] { item, DisasterType.Storm }) == 12f &&
                (float)risk.Invoke(null, new object[] { item, DisasterType.Earthquake }) == 8f &&
                (float)risk.Invoke(null, new object[] { item, DisasterType.Flood }) == 2.5f,
                "Removing roof work selection preserves production roof risk as a separate real model record.");

            var freeBundle = New(51179, false); string wallId = Walls(freeBundle, 1)[0].Id;
            freeBundle.Model.SetDeveloperOption(PanicDeveloperOption.InfiniteBudget, true);
            freeBundle.Model.SetDeveloperOption(PanicDeveloperOption.FreeMaterials, true);
            freeBundle.Model.Budget = 0;
            var q = Quote(freeBundle, wallId, 1, Walls(freeBundle, 1), Roofs(freeBundle), "direct.free-valid-bundle", check);
            if (q == null) return;
            var state = new Snapshot(freeBundle);
            bool began = freeBundle.BeginValidatedWorkV414("upgrade", wallId, false);
            bool committed = Finish(freeBundle) && freeBundle.TryCompleteWork("upgrade", wallId);
            var included = new HashSet<PanicItem>(Walls(freeBundle, 1).Concat(Roofs(freeBundle)));
            check("direct.free-valid-bundle-commits", began && committed && freeBundle.Model.Budget == 0 &&
                state.Stock.All(pair => freeBundle.Count(pair.Key) == pair.Value) && state.OtherStateUnchanged(freeBundle) &&
                state.Tiers.All(pair => pair.Key.Tier == (included.Contains(pair.Key) ? 1 : pair.Value)) && freeBundle.Model.CheatedRun,
                "Existing F2 free options still allow the valid upper-wall-plus-roof bundle, with sticky score isolation.");
        }

        static PanicFloorWallUpgradePlanV414 Quote(PanicExpedition e, string id, int target,
            PanicItem[] walls, PanicItem[] roofs, string key, Action<string, bool, string> check)
        {
            var before = new Snapshot(e); PanicFloorWallUpgradePlanV414 q;
            bool available = e.TryGetFloorWallUpgradeV414(id, out q);
            check(key + ".quote-available", available && q != null, "Actual installed production records; expected target T" + target + ".");
            if (!available || q == null) return null;
            var members = walls.Concat(roofs).ToArray(); var changed = members.Where(i => i.Tier < target).ToArray();
            check(key + ".membership", q.Floor == Mathf.FloorToInt((walls[0].Position.y + .15f) / 3.2f) && q.TargetTier == target && q.WallCount == walls.Length &&
                q.RoofCount == roofs.Length && q.ChangedWallCount == walls.Count(i => i.Tier < target) &&
                q.ChangedRoofCount == roofs.Count(i => i.Tier < target) && q.ChangedMemberCount == changed.Length &&
                SameIds(q.WallIds, walls) && SameIds(q.RoofIds, roofs) && !q.WallIds.Intersect(q.RoofIds).Any(),
                "WallCount/WallIds remain wall-only; roof and changed-member counters describe exactly the installed batch.");
            var costs = changed.SelectMany(i => PanicExpedition.MaterialCosts("upgrade", i)).ToArray();
            var requirements = e.WorkRequirementsV414("upgrade", id);
            check(key + ".exact-quote", q.BudgetCost == changed.Sum(i => PanicExpedition.WorkCost("upgrade", i)) &&
                SameCosts(q.MaterialCosts, costs) && e.WorkBudgetCostV414("upgrade", id) == q.BudgetCost &&
                SameCosts(e.WorkMaterialCostsV414("upgrade", id), costs) &&
                SameCosts(requirements.Select(r => new PanicMaterialCost(r.Id, r.Required)).ToArray(), costs) &&
                requirements.All(r => r.Owned == e.Count(r.Id) && r.Missing == (e.Model.FreeMaterials ? 0 : Math.Max(0, r.Required - r.Owned))),
                "Quote aggregates the actual per-item recipes only for tiers that increase; retained higher tiers cost zero.");
            check(key + ".quote-readonly", before.Unchanged(e), "Querying quote and requirements changes no gameplay state.");
            return q;
        }

        static void Commit(PanicExpedition e, string id, PanicFloorWallUpgradePlanV414 q,
            PanicItem[] members, string key, Action<string, bool, string> check)
        {
            e.Model.Budget = q.BudgetCost; var before = new Snapshot(e);
            bool began = e.BeginValidatedWorkV414("upgrade", id, false);
            check(key + ".begin-no-charge", began && before.Unchanged(e), "Exactly the quoted budget; no prepayment.");
            bool completed = Finish(e); bool committed = completed && e.TryCompleteWork("upgrade", id);
            var included = new HashSet<PanicItem>(members);
            bool tiers = before.Tiers.All(pair => pair.Key.Tier == (included.Contains(pair.Key) ? Math.Max(pair.Value, q.TargetTier) : pair.Value));
            bool stock = before.Stock.All(pair => e.Count(pair.Key) == pair.Value - q.MaterialCosts.Where(c => c.Id == pair.Key).Sum(c => c.Required));
            check(key + ".atomic-exact-commit", began && committed && e.Model.Budget == 0 && stock && tiers && before.OtherStateUnchanged(e) &&
                !e.Work.Active && e.ActiveFloorWallUpgradeV414 == null,
                "One actual completed transaction charges the exact quote once; selected floor/roof advance together, all unrelated items stay unchanged.");
            var after = new Snapshot(e);
            check(key + ".repeat-no-charge", !e.TryCompleteWork("upgrade", id) && after.Unchanged(e),
                "Repeated completion cannot charge or advance any member again.");
        }

        static PanicExpedition New(int seed, bool gather = true)
        {
            var e = PanicEconomyChecksV59.New(seed);
            if (gather) PanicEconomyChecksV59.Gather(e, 14, 1);
            e.Model.Budget = 100000; // Explicit budget fixture, not campaign-affordability acceptance.
            return e;
        }
        static PanicItem[] Walls(PanicExpedition e, int floor)
        { return e.Model.Items.Where(i => i.Kind == ItemKind.Wall && Mathf.FloorToInt((i.Position.y + .15f) / 3.2f) == floor).OrderBy(i => i.Id, StringComparer.Ordinal).ToArray(); }
        static PanicItem[] Roofs(PanicExpedition e)
        { return e.Model.Items.Where(i => i.Kind == ItemKind.Roof && i.Installed).OrderBy(i => i.Id, StringComparer.Ordinal).ToArray(); }
        static PanicItem NewRoof(string id, int tier, bool installed)
        { return new PanicItem { Id = id, Label = "Regression roof", ArtId = "roof", Kind = ItemKind.Roof, Position = new Vector3(0, 6.4f, 0), Tier = tier, Installed = installed }; }
        static bool Finish(PanicExpedition e)
        {
            for (int i = 0; i < 16 && e.Work.Active && !e.Work.Complete; i++)
                if (!e.Work.PressHammer()) return false;
            return e.Work.Active && e.Work.Complete;
        }
        static bool SameIds(IReadOnlyList<string> ids, PanicItem[] members)
        { return ids.Count == members.Length && ids.Distinct(StringComparer.Ordinal).Count() == ids.Count &&
            ids.OrderBy(i => i, StringComparer.Ordinal).SequenceEqual(members.Select(i => i.Id).OrderBy(i => i, StringComparer.Ordinal)); }
        static bool SameCosts(IReadOnlyList<PanicMaterialCost> first, IReadOnlyList<PanicMaterialCost> second)
        {
            var a = first.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.Sum(c => c.Required));
            var b = second.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.Sum(c => c.Required));
            return a.Count == b.Count && a.All(pair => b.ContainsKey(pair.Key) && b[pair.Key] == pair.Value);
        }
        static void Scenario(Action<string, bool, string> check, string id, Action action)
        { try { action(); } catch (Exception ex) { check(id + ".exception", false, ex.GetType().Name + ": " + ex.Message); } }

        sealed class Snapshot
        {
            public readonly int Budget;
            public readonly Dictionary<string, int> Stock;
            public readonly Dictionary<PanicItem, int> Tiers;
            readonly Dictionary<PanicItem, bool> installed;
            readonly string inventory, sources, supplies;
            readonly float time;
            readonly int wave, revision;
            readonly PanicPhase phase;
            readonly bool cheated;
            public Snapshot(PanicExpedition e)
            {
                Budget = e.Model.Budget; Stock = PanicExpedition.MaterialIds.ToDictionary(id => id, id => e.Count(id));
                Tiers = e.Model.Items.ToDictionary(i => i, i => i.Tier);
                installed = e.Model.Items.ToDictionary(i => i, i => i.Installed);
                inventory = Inventory(e); sources = Sources(e); supplies = Supplies(e); time = e.Model.TimeRemaining;
                wave = e.Model.Wave; phase = e.Model.Phase; cheated = e.Model.CheatedRun; revision = e.LootLayoutRevisionV5;
            }
            public bool OtherStateUnchanged(PanicExpedition e)
            { return inventory == Inventory(e) && sources == Sources(e) && supplies == Supplies(e) &&
                installed.Count == e.Model.Items.Count && installed.All(pair => e.Model.Items.Contains(pair.Key) && pair.Key.Installed == pair.Value) &&
                time == e.Model.TimeRemaining && wave == e.Model.Wave &&
                phase == e.Model.Phase && cheated == e.Model.CheatedRun && revision == e.LootLayoutRevisionV5; }
            public bool Unchanged(PanicExpedition e)
            { return Budget == e.Model.Budget && Stock.All(pair => e.Count(pair.Key) == pair.Value) &&
                Tiers.Count == e.Model.Items.Count && Tiers.All(pair => e.Model.Items.Contains(pair.Key) && pair.Key.Tier == pair.Value) && OtherStateUnchanged(e); }
            static string Inventory(PanicExpedition e)
            { return string.Join("|", e.Inventory.Select(s => s == null ? "null" : s.Id + ":" + s.Count)); }
            static string Sources(PanicExpedition e)
            { return string.Join("|", e.Sources.Select(s => s.Id + ":" + s.Open + ":" + s.AutoLooted + ":" +
                string.Join(",", s.Contents.Select(c => c.Id + ":" + c.Count)))); }
            static string Supplies(PanicExpedition e)
            { return string.Join("|", e.WorldSupplies.Select(s => s.Id + ":" + s.Count + ":" + s.Taken)); }
        }
    }
}
