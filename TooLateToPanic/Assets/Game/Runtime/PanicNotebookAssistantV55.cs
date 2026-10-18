using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicAssistantStepV55
    {
        public string Label, Detail;
        public bool Complete, Active, Available;
    }
    public sealed class PanicAssistantSnapshotV55
    {
        public bool Available;
        public int CurrentStep, StepCount;
        public string Title, Task, Hint;
        public PanicAssistantStepV55[] Steps;
    }

    // A recommendation, never a quest gate. Progress comes only from production state.
    public sealed partial class PanicHomeWorld
    {
        bool assistantEnabledV55 = true;
        int assistantWaveV55 = -1;
        string assistantTargetV55 = string.Empty;
        public bool AssistantEnabledV55 { get { return assistantEnabledV55; } }
        public string AssistantTargetIdV55 { get { return assistantTargetV55; } }
        public PanicAssistantSnapshotV55 AssistantV55 { get { return CaptureAssistantV55(); } }
        public void SetAssistantEnabledV55(bool value) { assistantEnabledV55 = value; }
        public int LegacyGuidanceVisualCountV55
        {
            get
            {
                int count = 0;
                foreach (var t in GetComponentsInChildren<Transform>(true))
                    if (t.name == "V54 next reachable checkpoint beacon") count++;
                return count;
            }
        }

        void ResetAssistantV55()
        {
            assistantWaveV55 = -1; assistantTargetV55 = string.Empty;
            guidanceStateV54 = null; guidancePlannerV54 = null;
            TickAssistantV55();
        }
        void TickAssistantV55()
        {
            if (Model == null || Expedition == null || Model.Phase == PanicPhase.Title) return;
            if (assistantWaveV55 == Model.Wave) return;
            assistantWaveV55 = Model.Wave;
            assistantTargetV55 = string.Empty;
            PanicItem nearest = null; float best = float.MaxValue;
            foreach (var item in Model.Items)
            {
                string action = PanicItemActionRules.PrimaryAction(item);
                if (action != "anchorwall" && action != "anchorfloor") continue;
                if (!Model.CanBeginJuryWork(action, item.Id)) continue;
                float distance = (item.Position - PlayerPosition).sqrMagnitude;
                if (distance < best || distance == best && nearest != null && string.CompareOrdinal(item.Id, nearest.Id) < 0)
                { best = distance; nearest = item; }
            }
            if (nearest != null) assistantTargetV55 = nearest.Id;
        }
        PanicItem AssistantItemV55()
        { return Model == null ? null : Model.Items.Find(i => i.Id == assistantTargetV55); }
        bool AssistantMaterialsReadyV55(PanicItem item)
        {
            if (item == null || item.Anchored) return true;
            foreach (var cost in PanicExpedition.MaterialCosts(PanicItemActionRules.PrimaryAction(item), item))
                if (!Model.FreeMaterials && Expedition.Count(cost.Id) < cost.Required) return false;
            return true;
        }
        bool AssistantUtilitiesReadyV55()
        { return Model.UtilitySafety.IsComplete("gasoff") && Model.UtilitySafety.IsComplete("wateroff") && Model.UtilitySafety.IsComplete("poweroff"); }

        PanicAssistantSnapshotV55 CaptureAssistantV55()
        {
            var result = new PanicAssistantSnapshotV55 { Title = "Hazırlık yardımcısı", Task = "Defterini gözden geçir", Hint = "J ile aç", CurrentStep = 1, StepCount = 6 };
            string[] labels = { "Malzemeleri topla", "Bir mobilyayı sabitle", "Tesisatı güvene al", "Bir su al", "Bir gıda al", "Güvenli alana geç" };
            result.Steps = new PanicAssistantStepV55[6];
            for (int i = 0; i < 6; i++) result.Steps[i] = new PanicAssistantStepV55 { Label = labels[i], Detail = "Sırası gelince burada görünecek" };
            if (Model == null || Expedition == null) return result;
            bool preparing = Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial;
            result.Available = preparing;
            var item = AssistantItemV55();
            bool anchored = item == null || item.Anchored;
            bool materials = anchored || AssistantMaterialsReadyV55(item);
            bool utility = AssistantUtilitiesReadyV55();
            bool bag = BagFinalStageActive;
            bool[] done = { materials, anchored, utility, Model.PackedWater >= 1, Model.PackedFood >= 1, Model.ReachedShelterV5 };
            string target = item == null ? "Uygun mobilyalar zaten sabit" : item.Label;
            string[] details = {
                anchored ? "Yeni ankraj malzemesi gerekmiyor" : materials ? "Ankraj malzemeleri hazır" : target + " için gereken malzemeler",
                target,
                "Gaz, su vanası ve ana elektrik şalteri",
                "Son 45 saniyede; aldığın su çantana girer",
                "Son 45 saniyede; aldığın gıda çantana girer",
                Model.HasActiveDisasterV54(DisasterType.Flood) ? "Sel var: üst kat panik odası" : "Bodrum sığınağı"
            };
            int current = bag ? !done[3] ? 3 : !done[4] ? 4 : 5 : !done[0] ? 0 : !done[1] ? 1 : 2;
            result.CurrentStep = current + 1;
            for (int i = 0; i < 6; i++)
            {
                var step = result.Steps[i];
                step.Complete = done[i]; step.Available = preparing && (bag ? i >= 3 : i < 3);
                step.Active = step.Available && i == current && !step.Complete;
                step.Detail = bag && i < 3 && !done[i] ? "Önlem süresi bitti; çantana odaklan" : details[i];
            }
            result.Task = result.Steps[current].Complete ? bag ? "Güvenli alandasın" : "Diğer önlemleri gözden geçir" : labels[current];
            result.Hint = bag ? details[current] : current < 2 ? target : "Gaz, su ve elektrik noktalarını kontrol et";
            if (!bag && materials && anchored && utility) result.Hint = "Son 45 saniyede çantanı hazırlayacaksın";
            if (GuidanceRouteValidV54 && !string.IsNullOrEmpty(GuidanceObjectiveIdV54))
            {
                string floor = GuidanceObjectivePositionV54.y > 2 ? "Üst kat" : GuidanceObjectivePositionV54.y < -1 ? "Bodrum" : "Alt kat";
                string spot = GuidanceObjectiveIdV54 == assistantTargetV55 ? target : GuidanceObjectiveLabelV54.Split('·')[0].Trim();
                string checkpoint = GuidanceCheckpointLabelV54.Split('·')[0].Trim();
                result.Hint = floor + " · " + (checkpoint.StartsWith("Kapıyı") || checkpoint.StartsWith("Merdiven") ? checkpoint : spot);
            }
            if (JuryActive && JurySnapshot != null)
            {
                result.Task = JurySnapshot.Hint; result.Hint = "Defterdeki hazırlık notunu takip et";
                result.CurrentStep = result.StepCount = 1;
                result.Steps = new[] { new PanicAssistantStepV55 { Label = result.Task, Detail = result.Hint, Active = preparing, Available = preparing } };
            }
            return result;
        }

        void GuidanceQueueAssistantV55()
        {
            var item = AssistantItemV55();
            var snapshot = CaptureAssistantV55();
            if (snapshot.CurrentStep == 1 && item != null)
            {
                foreach (var cost in PanicExpedition.MaterialCosts(PanicItemActionRules.PrimaryAction(item), item))
                {
                    if (Model.FreeMaterials || Expedition.Count(cost.Id) >= cost.Required) continue;
                    foreach (var source in Expedition.Sources)
                    {
                        if (source.AutoLooted || source.Contents == null || !InteractionObject(source.Id)) continue;
                        bool stock = source.Contents.Exists(c => c != null && c.Id == cost.Id && c.Count > 0);
                        if (stock && !guidanceGoalsV54.Exists(g => g.Id == source.Id))
                            guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = source.Id, Position = source.Position, Floor = source.Floor,
                                Label = source.Label + " · " + PanicExpedition.DisplayName(cost.Id) + " al · E", Action = "materials" });
                    }
                }
                guidanceNoRouteV54 = "Gereken malzemeyi bulamadın; defterden başka önlemleri incele";
            }
            else if (snapshot.CurrentStep == 2 && item != null && !item.Anchored)
            {
                string action = PanicItemActionRules.PrimaryAction(item);
                if (Model.CanBeginJuryWork(action, item.Id) && (Model.InfiniteBudget || Model.Budget >= PanicExpedition.WorkCost(action, item)))
                    guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = item.Id, Position = item.Position,
                        Floor = Mathf.RoundToInt(item.Position.y / 3.2f), Label = item.Label + " · Q seç / 1 sabitle", Action = action });
                guidanceNoRouteV54 = "Bütçeni ve mobilyanın çevresini kontrol et";
            }
            else if (!AssistantUtilitiesReadyV55())
            {
                string[] order = Model.HasActiveDisasterV54(DisasterType.Flood) ? new[] { "wateroff", "poweroff", "gasoff" } : new[] { "gasoff", "wateroff", "poweroff" };
                foreach (var action in order)
                {
                    if (Model.UtilitySafety.IsComplete(action)) continue;
                    string id = PanicUtilitySafetyV414.TargetForAction(action);
                    if (TryUtilityStationPositionV414(id, out var p))
                        guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = id, Position = p, Floor = Mathf.RoundToInt(p.y / 3.2f), Label = PanicUtilitySafetyV414.Label(action) + " · E", Action = action });
                    break;
                }
            }
            guidanceNoRouteV54 = guidanceNoRouteV54 ?? "Önlemlerini gözden geçir";
            GuidanceQueueGoalsV54(guidanceGoalsV54);
        }
    }
}
