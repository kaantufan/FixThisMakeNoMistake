using System;
using System.Linq;
using System.Reflection;

namespace TooLateToPanic
{
    public static class PanicPresentationChecksV512
    {
        public static void Run(Action<string, bool, string> check)
        {
            if (string.IsNullOrEmpty(PanicModel.ScorePathOverride)) throw new InvalidOperationException("Fresh isolated profile required.");
            var calculate = typeof(PanicModel).GetMethod("CalculateDamage", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (DisasterType disaster in new[] { DisasterType.Earthquake, DisasterType.Flood, DisasterType.Fire, DisasterType.Storm })
            {
                var m = new PanicModel(); bool started = m.StartSimulation(disaster);
                var furniture = m.Items.Where(i => i.Installed && new[] { "table", "bed", "sofa", "chair", "sofa_indigo", "sofa_olive" }.Contains(i.ArtId)).ToArray();
                check("v512.furniture.fixture." + disaster, started && furniture.Any(i => i.ArtId == "table") && furniture.Any(i => i.ArtId == "bed") && furniture.Any(i => i.ArtId == "sofa"), "Actual simulation fixture; original house furniture records.");
                calculate.Invoke(m, null);
                check("v512.furniture.real-risk-retained." + disaster, furniture.All(i => i.Damage > 0 && i.Contribution > 0), "User chose real prevention actions: no damage immunity or risk change.");
                check("v512.furniture.anchor-available." + disaster, furniture.All(i => PanicItemActionRules.CanAnchor(i) && PanicItemActionRules.Allows(PanicItemActionRules.PrimaryAction(i), i, i.Id) && PanicItemActionRules.PrimaryShortcut(i) == 1), "Each requested furniture family has the actual anchor action.");
                check("v512.damage.actionable-retained." + disaster, m.Items.Any(i => PanicItemActionRules.CanAnchor(i) && i.Contribution > 0) && m.Items.Any(i => i.Kind == ItemKind.Roof && i.Contribution > 0), "Actual cabinet/bookshelf/TV and roof risk remains.");
                check("v512.damage.ledger." + disaster, Math.Abs(m.Items.Sum(i => i.Contribution) - m.TotalDamage) < .001f, "Item attribution still sums to the real total.");
                var book = m.Items.Find(i => i.Id == "bookshelf"); float before = book.Contribution;
                book.Anchored = true; book.AnchorSurface = "floor"; calculate.Invoke(m, null);
                check("v512.damage.real-anchor." + disaster, disaster == DisasterType.Fire ? Math.Abs(book.Contribution - before) < .001f : book.Contribution < before, "Existing disaster-specific anchor efficacy; no fake fire protection.");
            }
            int seed = 51210;
            foreach (string id in new[] { "sofa", "table", "chair", "bed", "desk_upper", "bed_child" })
            {
                var entry = PanicEconomyChecksV59.New(seed++); var item = entry.Model.Items.Find(i => i.Id == id);
                string action = entry.PrimaryActionV414(item); PanicEconomyChecksV59.Gather(entry, 14, 1);
                int cash = entry.Model.Budget, cost = PanicExpedition.WorkCost(action, item);
                var stock = PanicExpedition.MaterialIds.ToDictionary(k => k, k => entry.Count(k));
                var recipe = PanicExpedition.MaterialCosts(action, item);
                bool began = entry.BeginValidatedWorkV414(action, id, false);
                check("v512.furniture.begin." + id, began && entry.Work.TotalSteps == 2 && !entry.Work.UsesHammer && !item.Anchored && entry.Model.Budget == cash, "Real two-screw work, no advance payment.");
                if (began) { entry.Work.TryClick(0); entry.Work.ApplyRotation(10000 * entry.Work.ExpectedDirection, true); }
                entry.Work.Cancel(); entry.CancelFloorWallUpgradeV414();
                check("v512.furniture.cancel." + id, !item.Anchored && entry.Model.Budget == cash && stock.All(p => entry.Count(p.Key) == p.Value), "Partial screw cancellation is free.");
                bool done = PanicEconomyChecksV59.Complete(entry, action, id);
                check("v512.furniture.commit." + id, done && item.Anchored && item.AnchorOwned && entry.Model.Budget == cash - cost && stock.All(p => entry.Count(p.Key) == p.Value - recipe.Where(c => c.Id == p.Key).Sum(c => c.Required)), "Actual complete two-screw transaction charges the original recipe once.");
                int paid = entry.Model.Budget;
                check("v512.furniture.duplicate." + id, !entry.TryCompleteWork(action, id) && entry.Model.Budget == paid && !entry.Model.CheatedRun, "Duplicate work cannot charge twice; normal score eligibility.");
            }
            var e = PanicEconomyChecksV59.New(51201); string initial = e.Model.Message;
            check("v512.notice.approach-is-readonly", PanicPreparationFeedbackV512.ShortageNotice(e, "table") == null && e.Model.Message == initial && !e.HasMissingRequirements, "Looking at a target creates no shortage.");
            e.Model.Notify("Bu yatakta sabitleme gerekmiyor; tehlike yok.");
            check("v512.notice.narrative-not-material", PanicPreparationFeedbackV512.ShortageNotice(e, "bed") == null, "Narrative wording is not a material-demand event.");
            int budget = e.Model.Budget; bool refused = !e.CanBeginWork("anchorfloor", "bookshelf");
            check("v512.notice.actual-missing-material", refused && e.HasMissingRequirements && PanicPreparationFeedbackV512.ShortageNotice(e, "bookshelf") == "Malzeme eksik" && e.Model.Budget == budget, "Actual empty-inventory preflight, no payment.");
            check("v512.notice.other-target-no-shortage", PanicPreparationFeedbackV512.ShortageNotice(e, "table") == null, "Previous work target cannot leak a warning onto a table.");
            e.ClearRequirements(); e.Model.Budget = 0; PanicEconomyChecksV59.Gather(e, 14, 1);
            refused = !e.CanBeginWork("anchorfloor", "bookshelf");
            check("v512.notice.actual-missing-budget", refused && PanicPreparationFeedbackV512.ShortageNotice(e, "bookshelf") == "Para yetmiyor", "Actual funded-material / zero-budget fixture.");
            e.ClearRequirements();
            check("v512.notice.cleared", PanicPreparationFeedbackV512.ShortageNotice(e, "bookshelf") == null, "Cleared work shortage stays clear.");
        }
    }
}
