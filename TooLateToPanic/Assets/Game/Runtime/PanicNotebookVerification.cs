using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Opt-in helper for the root's native isolated driver. Never starts a run, reads/writes score files,
    /// changes camera/player state or runs automatically. The caller owns the fixture and receipts.
    /// </summary>
    public static class PanicNotebookVerification
    {
        public static IEnumerator Run(PanicHomeWorld world, PanicNotebookOverlay notebook, Action<bool, string> record)
        {
            if (!world || !notebook || world.Model == null || world.Expedition == null)
            { record(false, "Notebook fixture is missing"); yield break; }
            if (!notebook.CanOpen) { record(false, "Notebook fixture needs a playable phase without another modal/work/placement/inventory"); yield break; }
            string before = GameplayFingerprint(world);
            bool priorLock = world.InputLocked, priorCheated = world.Model.CheatedRun;
            PanicPhase phase = world.Model.Phase;
            float priorClock = world.Model.TimeRemaining;
            float priorJuryClock = world.JurySnapshot == null ? 0 : world.JurySnapshot.ElapsedSeconds;
            record(notebook.RequestOpen() && notebook.BlocksInput && notebook.IsVisible, "Open owns input in the request frame");
            yield return new WaitForSecondsRealtime(.42f);
            record(notebook.Animation.Openness == 1f, "Open animation reaches its final pose");
            record(world.InputLocked == priorLock, "Notebook does not steal the existing InputLocked owner");
            record(Mathf.Abs(world.Model.TimeRemaining - priorClock) < .001f && world.Model.Phase == phase, "World model clock is gated while notebook is open");
            if (world.JurySnapshot != null)
                record(Mathf.Abs(world.JurySnapshot.ElapsedSeconds - priorJuryClock) < .001f, "Short-flow clock is gated while notebook is open");
            foreach (PanicNotebookPage page in Enum.GetValues(typeof(PanicNotebookPage)))
            {
                notebook.RequestPage(page); yield return new WaitForSecondsRealtime(.83f);
                record(!notebook.Animation.Turning && notebook.Animation.CurrentPage == page, "Page settles: " + page);
            }
            notebook.RequestPage(PanicNotebookPage.Flood);
            float reverseDeadline=Time.realtimeSinceStartup+2;
            while(notebook.Animation.Turning && notebook.Animation.TurnProgress<.2f && Time.realtimeSinceStartup<reverseDeadline)yield return null;
            record(notebook.Animation.Turning && notebook.Animation.TurnProgress>0 && notebook.Animation.TurnProgress<1,"Reverse fixture is inside a real page turn");
            float turn = notebook.Animation.TurnProgress;
            notebook.RequestPage(PanicNotebookPage.Inventory);
            record(notebook.Animation.TurnDirection == -1 && notebook.Animation.TurnProgress == turn, "Reverse request preserves the current paper pose");
            reverseDeadline=Time.realtimeSinceStartup+PanicNotebookAnimation.TurnSeconds+.6f;
            while(notebook.Animation.Turning && Time.realtimeSinceStartup<reverseDeadline)yield return null;
            record(notebook.Animation.CurrentPage == PanicNotebookPage.Inventory && !notebook.Animation.Turning, "Reverse returns to the source page");
            notebook.RequestPage(PanicNotebookPage.Fire); notebook.RequestPage(PanicNotebookPage.Storm); notebook.RequestPage(PanicNotebookPage.Earthquake);
            yield return new WaitForSecondsRealtime(1.65f);
            record(notebook.Animation.CurrentPage == PanicNotebookPage.Earthquake && !notebook.Animation.Turning, "Latest rapid page request wins");
            notebook.RequestClose(); float openPose = notebook.Animation.Openness;
            notebook.RequestOpen();
            record(notebook.Animation.Openness == openPose && notebook.Animation.TargetOpen, "Close/reopen in one frame preserves pose and ownership");
            notebook.RefreshSnapshot(); var data = notebook.Snapshot;
            record(data != null && data.Budget == (world.Model.InfiniteBudget ? "SINIRSIZ" : world.Model.Budget.ToString("N0")), "Budget comes from the live model");
            bool materials = data != null;
            if (data != null) for (int i = 0; i < PanicNotebookData.MaterialIds.Length; i++)
                materials &= data.Materials[i] == world.Expedition.Count(PanicNotebookData.MaterialIds[i]);
            record(materials, "Notebook materials use the canonical live Count API");
            record(data != null && data.Inventory.Length == world.Expedition.Inventory.Count && data.Bag.Length == world.Expedition.Bag.Count, "Notebook capacities come from real storage");
            record(GameplayFingerprint(world) == before && world.Model.CheatedRun == priorCheated, "Notebook navigation changes no gameplay stock/budget/score/cheat state");
            notebook.SetReducedMotion(true); notebook.RequestClose();
            record(!notebook.IsVisible && notebook.BlocksInput, "Instant close retains the request-frame input gate");
            yield return null;
            record(!notebook.BlocksInput && world.InputLocked == priorLock, "Closed notebook releases only its own gate next frame");
            notebook.SetReducedMotion(false);
        }

        public static string GameplayFingerprint(PanicHomeWorld world)
        {
            var m = world.Model; var e = world.Expedition; var b = new StringBuilder();
            b.Append(m.Wave).Append('|').Append(m.Budget).Append('|').Append(m.Score).Append('|').Append(m.PackedWater).Append('|').Append(m.PackedFood).Append('|').Append(m.PackedSupportMask).Append('|').Append(m.CheatedRun);
            foreach (var slot in e.Inventory) b.Append("|I:").Append(slot == null ? "" : slot.Id).Append(':').Append(slot == null ? 0 : slot.Count);
            foreach (var slot in e.Bag) b.Append("|B:").Append(slot == null ? "" : slot.Id).Append(':').Append(slot == null ? 0 : slot.Count);
            foreach (var item in m.Items) if (item != null) b.Append("|T:").Append(item.Id).Append(':').Append(item.Anchored).Append(':').Append(item.Tier).Append(':').Append(item.Damage);
            foreach (var source in e.Sources) if (source != null)
            {
                b.Append("|S:").Append(source.Id).Append(':').Append(source.Open);
                if (source.Contents != null) foreach (var slot in source.Contents) b.Append(':').Append(slot == null ? "" : slot.Id).Append(':').Append(slot == null ? 0 : slot.Count);
            }
            for (int i = 0; i < PanicNotebookData.MaterialIds.Length; i++) b.Append("|M:").Append(e.Count(PanicNotebookData.MaterialIds[i]));
            return b.ToString();
        }
    }
}
