using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Committed damage, not a preview or a second RNG evaluation.</summary>
    public sealed class PanicWaveLedgerRowV56
    {
        public string Id { get; }
        public string Label { get; }
        public string Detail { get; }
        public float DamageContribution { get; }
        internal PanicWaveLedgerRowV56(string id, string label, string detail, float contribution)
        { Id = id; Label = label; Detail = detail; DamageContribution = contribution; }
    }

    public sealed class PanicWaveLedgerV56
    {
        public int Wave { get; }
        public string DisasterLabel { get; }
        public bool IsSimulation { get; }
        public string OutcomeLabel { get; }
        public float TotalDamage { get; }
        public bool HasResolvedDamage { get; }
        public int BudgetBeforeReward { get; }
        public int BaseReward { get; }
        public int DamageDeduction { get; }
        public int SupportBonus { get; }
        public int CalculatedReward { get; }
        public int CapacityDeduction { get; }
        public int ActualCredited { get; }
        public int BudgetAfterReward { get; }
        public string RewardExplanation { get; }
        public IReadOnlyList<PanicWaveLedgerRowV56> Rows { get; }

        internal PanicWaveLedgerV56(PanicModel model, string outcome, string explanation,
            int before, int basis, int deduction, int bonus, List<PanicWaveLedgerRowV56> rows, bool resolved)
        {
            Wave = model.Wave; DisasterLabel = model.ActiveDisasterLabelV54;
            IsSimulation = model.IsSimulation; OutcomeLabel = outcome;
            TotalDamage = model.TotalDamage; HasResolvedDamage = resolved;
            BudgetBeforeReward = before; BaseReward = basis;
            DamageDeduction = deduction; SupportBonus = bonus;
            CalculatedReward = model.LastReward;
            BudgetAfterReward = model.Budget;
            ActualCredited = Math.Max(0, model.Budget - before);
            CapacityDeduction = Math.Max(0, CalculatedReward - ActualCredited);
            RewardExplanation = explanation;
            Rows = rows.AsReadOnly();
        }
    }

    public sealed partial class PanicModel
    {
        public PanicWaveLedgerV56 LastWaveLedgerV56 { get; private set; }
        public bool WaveLedgerOpenV56 { get; private set; }
        bool waveLedgerReviewedV56;
        bool WaveLedgerAwaitingReviewV56 { get { return LastWaveLedgerV56 != null && !waveLedgerReviewedV56; } }

        public bool SetWaveLedgerOpenV56(bool open)
        {
            if (open && (IsJuryDemo || LastWaveLedgerV56 == null || LastWaveLedgerV56.Wave != Wave ||
                (Phase != PanicPhase.Inspection && Phase != PanicPhase.GameOver))) return false;
            if (!open && WaveLedgerOpenV56) waveLedgerReviewedV56 = true;
            WaveLedgerOpenV56 = open;
            return true;
        }

        void ResetWaveLedgerV56()
        { LastWaveLedgerV56 = null; WaveLedgerOpenV56 = false; waveLedgerReviewedV56 = false; }

        void CaptureWaveLedgerV56(string outcome, string explanation, int before, int basis = 0, int deduction = 0, int bonus = 0)
        {
            if (IsJuryDemo || LastWaveLedgerV56 != null && LastWaveLedgerV56.Wave == Wave) return;
            var rows = new List<PanicWaveLedgerRowV56>();
            if (waveResolved)
            {
                // Item.Contribution already contains utility damage. Split it out exactly
                // once so the listed damage contributions still sum to TotalDamage.
                var utilityByItem = new Dictionary<string, float>(StringComparer.Ordinal);
                foreach (var incident in SecondaryEventsV5)
                {
                    float previous;
                    utilityByItem.TryGetValue(incident.TargetId, out previous);
                    utilityByItem[incident.TargetId] = previous + incident.DamagePoints;
                }
                foreach (var item in Items)
                {
                    float utility;
                    utilityByItem.TryGetValue(item.Id, out utility);
                    float ordinary = Mathf.Max(0, item.Contribution - utility - CriticalPenaltyForItemV513(item.Id));
                    if (ordinary > 0)
                        rows.Add(new PanicWaveLedgerRowV56(item.Id, item.Label, "Afetin toplam hasarına katkı", ordinary));
                }
                foreach (var incident in SecondaryEventsV5)
                {
                    var target = FindItem(incident.TargetId);
                    string label = incident.Kind == PanicSecondaryEventKindV5.GasFlare ? "Gaz vanası açık: alevlenme" :
                        incident.Kind == PanicSecondaryEventKindV5.PipeLeak ? "Su vanası açık: boru sızıntısı" : "Elektrik açık: kısa devre";
                    rows.Add(new PanicWaveLedgerRowV56("utility:" + incident.TargetId + ":" + incident.Kind,
                        label, incident.Label + (target == null ? "" : " · " + target.Label), incident.DamagePoints));
                }
                AppendCriticalPenaltyLedgerRowsV513(rows);
                rows.Sort((a, b) => b.DamageContribution.CompareTo(a.DamageContribution));
            }
            LastWaveLedgerV56 = new PanicWaveLedgerV56(this, outcome, explanation, before, basis, deduction, bonus, rows, waveResolved);
        }
    }

    public sealed partial class PanicHomeWorld
    {
        // A phase transition must set up/finish its presentation before a HUD modal
        // can take clock/input ownership. The modal never freezes a pending cutscene.
        public bool WaveLedgerMayOpenV56
        {
            get { return Model != null && previousPhase == Model.Phase && !Model.IsJuryDemo &&
                (Model.Phase == PanicPhase.Inspection || Model.Phase == PanicPhase.GameOver) &&
                !CinematicActive && !LossAnnounced && !InputLocked && !Model.IsPaused && !NotebookBlocksInput; }
        }
    }
}
