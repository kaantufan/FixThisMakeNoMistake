using System;
using UnityEngine;

namespace TooLateToPanic
{
    public enum PanicFirstWaveStepV510 { Materials, Anchor, Utilities, Ready, Water, Food, Shelter, Sheltered }

    // A fresh read-only projection. No acknowledgement, timer or UI action completes a production step.
    public sealed class PanicFirstWaveSnapshotV510
    {
        public bool Required, GatherComplete, MaterialsReady, AnchorComplete, UtilitiesComplete;
        public bool WaterComplete, FoodComplete, ShelterComplete, Fallback;
        public PanicFirstWaveStepV510 Step;
        public string Title = "", Body = "", Action = "", Location = "", Reason = "";
        public string TargetId = "", TargetLabel = "";
        public Vector3 TargetPosition;
        public int TargetFloor;
    }

    public static class PanicFirstWavePolicyV510
    {
        public const float ShelterPrioritySeconds = 10f;
        public static bool IsRequired(bool armed, int wave, PanicPhase phase, bool tutorial, bool simulation, bool shortFlow)
        { return armed && wave == 1 && phase == PanicPhase.Preparing && !tutorial && !simulation && !shortFlow; }
    }

    public sealed partial class PanicHomeWorld
    {
        bool firstWaveArmedV510;
        int firstWaveGatherBaselineV510;
        string firstWaveAnchorIdV510 = "";
        public bool FirstWaveOnboardingRequiredV510
        {
            get { return Model != null && Expedition != null && PanicFirstWavePolicyV510.IsRequired(firstWaveArmedV510,
                Model.Wave, Model.Phase, Model.IsTutorial, Model.IsSimulation, Model.IsJuryDemo || JuryActive); }
        }
        public PanicFirstWaveSnapshotV510 FirstWaveOnboardingV510 { get { return CaptureFirstWaveOnboardingV510(); } }

        // Root calls after the real Model/Expedition reset. No profile/static seen flag is used.
        public void ResetFirstWaveOnboardingV510()
        {
            firstWaveArmedV510 = Model != null && Expedition != null && Model.Wave == 1 &&
                !Model.IsTutorial && !Model.IsSimulation && !Model.IsJuryDemo && !JuryActive;
            firstWaveGatherBaselineV510 = Expedition == null ? 0 : Expedition.GatherSequence;
            firstWaveAnchorIdV510 = "";
            ChooseFirstWaveAnchorV510();
            InvalidateNotebookTargetRouteV57();
        }

        public void ClearFirstWaveOnboardingV510()
        {
            firstWaveArmedV510 = false;
            firstWaveAnchorIdV510 = "";
        }

        // At end of normal World.Update, immediately BEFORE TickGuidanceV54.
        // Suggestion selection is independent of actual interaction focus; the selection policy owns it.
        public void TickFirstWaveOnboardingV510()
        {
            if (!FirstWaveOnboardingRequiredV510) return;
            ChooseFirstWaveAnchorV510();
        }

        bool FirstWaveAnyAnchorV510()
        {
            if (Model == null) return false;
            foreach (var item in Model.Items)
                if (item != null && item.Installed && item.Kind == ItemKind.Furniture && item.Anchored) return true;
            return false;
        }

        PanicItem FirstWaveAnchorV510()
        { return Model == null ? null : Model.Items.Find(i => i != null && i.Id == firstWaveAnchorIdV510); }

        bool FirstWaveMaterialsReadyV510(PanicItem item)
        {
            if (item == null || Expedition == null) return false;
            foreach (var cost in PanicExpedition.MaterialCosts(PanicItemActionRules.PrimaryAction(item), item))
                if (!Model.FreeMaterials && Expedition.Count(cost.Id) < cost.Required) return false;
            return true;
        }

        bool FirstWaveAnchorPossibleV510(PanicItem item)
        {
            if (item == null || !item.Installed || item.Anchored || item.Kind != ItemKind.Furniture) return false;
            string action = PanicItemActionRules.PrimaryAction(item);
            if ((action != "anchorwall" && action != "anchorfloor") || !Model.CanBeginJuryWork(action, item.Id) ||
                !Model.InfiniteBudget && Model.Budget < PanicExpedition.WorkCost(action, item)) return false;
            foreach (var cost in PanicExpedition.MaterialCosts(action, item))
            {
                if (Model.FreeMaterials) continue;
                int stock = Expedition.Count(cost.Id);
                foreach (var source in Expedition.Sources)
                    if (source != null && !source.AutoLooted && source.Contents != null && InteractionObject(source.Id))
                        foreach (var slot in source.Contents)
                            if (slot != null && slot.Id == cost.Id && slot.Count > 0) stock += slot.Count;
                if (stock < cost.Required) return false;
            }
            return InteractionObject(item.Id);
        }

        void ChooseFirstWaveAnchorV510()
        {
            if (!FirstWaveOnboardingRequiredV510 || FirstWaveAnyAnchorV510()) return;
            if (FirstWaveAnchorPossibleV510(FirstWaveAnchorV510())) return;
            PanicItem best = null; float nearest = float.MaxValue;
            foreach (var item in Model.Items)
            {
                if (!FirstWaveAnchorPossibleV510(item)) continue;
                float distance = (item.Position - PlayerPosition).sqrMagnitude;
                // Start on the entrance floor. Other floors remain an honest fallback.
                if (Mathf.RoundToInt(item.Position.y / 3.2f) != 0) distance += 1000f;
                if (distance < nearest || distance == nearest && best != null && string.CompareOrdinal(item.Id, best.Id) < 0)
                { best = item; nearest = distance; }
            }
            firstWaveAnchorIdV510 = best == null ? "" : best.Id;
        }

        bool FirstWaveHasInventorySpaceV510(string id)
        {
            int limit = PanicExpedition.StackLimitFor(id);
            foreach (var slot in Expedition.Inventory)
                if (slot == null || slot.Count <= 0 || string.IsNullOrEmpty(slot.Id) || slot.Id == id && slot.Count < limit) return true;
            return false;
        }

        LootSource FirstWaveSourceV510(PanicItem item, bool alreadyGathered)
        {
            LootSource best = null; float nearest = float.MaxValue;
            foreach (var source in Expedition.Sources)
            {
                if (source == null || source.AutoLooted || source.Contents == null || !InteractionObject(source.Id)) continue;
                bool useful = false;
                foreach (var slot in source.Contents)
                {
                    if (slot == null || slot.Count <= 0) continue;
                    foreach (var cost in PanicExpedition.MaterialCosts(PanicItemActionRules.PrimaryAction(item), item))
                        if (cost.Id == slot.Id && (!alreadyGathered || Expedition.Count(cost.Id) < cost.Required)) useful = true;
                }
                if (!useful) continue;
                float distance = (source.Position - PlayerPosition).sqrMagnitude;
                if (source.Floor != ActiveFloor) distance += 1000f;
                if (distance < nearest || distance == nearest && best != null && string.CompareOrdinal(source.Id, best.Id) < 0)
                { best = source; nearest = distance; }
            }
            return best;
        }

        static string FirstWaveFloorV510(int floor)
        { return floor < 0 ? "Bodrum" : floor > 0 ? "Üst kat" : "Zemin kat"; }

        void FirstWaveTargetV510(PanicFirstWaveSnapshotV510 card, string id, string label, Vector3 point, int floor)
        {
            card.TargetId = id; card.TargetLabel = label; card.TargetPosition = point; card.TargetFloor = floor;
            card.Location = FirstWaveFloorV510(floor) + " · " + label;
            if (GuidanceRouteValidV54 && GuidanceObjectiveIdV54 == id && !string.IsNullOrEmpty(GuidanceCheckpointLabelV54))
                card.Location += "\n" + GuidanceCheckpointLabelV54.Split('·')[0].Trim();
        }

        bool FirstWaveUtilityV510(PanicFirstWaveSnapshotV510 card)
        {
            foreach (string action in new[] { "gasoff", "wateroff", "poweroff" })
            {
                if (Model.UtilitySafety.IsComplete(action)) continue;
                string id = PanicUtilitySafetyV414.TargetForAction(action);
                if (!TryUtilityStationPositionV414(id, out var point) || !InteractionObject(id) || !Model.CanBeginJuryWork(action, id)) continue;
                card.Step = PanicFirstWaveStepV510.Utilities;
                card.Title = "3 · TESİSATI GÜVENE AL";
                card.Body = PanicUtilitySafetyV414.Label(action) + ". Yaklaş; E ile işi aç.\n" +
                    (action == "poweroff" ? "Kolu basılı tutup aşağı indir." : "Vana kolunu basılı tutup saat yönünde çevir.");
                card.Action = "E · Tesisat işi ücretsiz";
                FirstWaveTargetV510(card, id, PanicUtilitySafetyV414.TargetLabel(id), point, Mathf.RoundToInt(point.y / 3.2f));
                return true;
            }
            return false;
        }

        void FirstWaveShelterV510(PanicFirstWaveSnapshotV510 card)
        {
            bool flood = Model.HasActiveDisasterV54(DisasterType.Flood);
            card.Step = card.ShelterComplete ? PanicFirstWaveStepV510.Sheltered : PanicFirstWaveStepV510.Shelter;
            card.Title = card.ShelterComplete ? "6 · SIĞINAKTASIN" : "6 · ŞİMDİ SIĞINAĞA GİR";
            card.Body = card.ShelterComplete ? "Gerçek sığınak girişi tamamlandı. Afet sonucunu bekle." :
                flood ? "Üst kata çık. Panik odasının kapısına yaklaş; E ile içeri gir." :
                "Bodrum merdiveninden in. Sığınak kapağını E ile aç; yaklaş ve E ile içeri gir.";
            card.Action = card.ShelterComplete ? "Giriş doğrulandı" : "E · Fiziksel sığınağa gir";
            if (!card.ShelterComplete) FirstWaveTargetV510(card, flood ? "@panicroom" : "bunker",
                flood ? "Panik odası" : "Bodrum sığınağı", flood ? PanicRoomPositionV5 : BunkerPosition, flood ? 1 : -1);
        }

        PanicFirstWaveSnapshotV510 CaptureFirstWaveOnboardingV510()
        {
            var card = new PanicFirstWaveSnapshotV510 { Required = FirstWaveOnboardingRequiredV510 };
            if (!card.Required) return card;
            card.GatherComplete = Expedition.GatherSequence > firstWaveGatherBaselineV510;
            card.AnchorComplete = FirstWaveAnyAnchorV510();
            card.UtilitiesComplete = Model.UtilitySafety.CompletedCount == 3;
            card.WaterComplete = Model.PackedWater >= PanicModel.RequiredSupplyPerWave;
            card.FoodComplete = Model.PackedFood >= PanicModel.RequiredSupplyPerWave;
            card.ShelterComplete = Model.ReachedShelterV5;
            var item = FirstWaveAnchorV510();
            card.MaterialsReady = FirstWaveMaterialsReadyV510(item);
            if (BagFinalStageActive)
            {
                bool urgent = Model.TimeRemaining <= PanicFirstWavePolicyV510.ShelterPrioritySeconds;
                if (card.ShelterComplete || urgent || card.WaterComplete && card.FoodComplete)
                {
                    FirstWaveShelterV510(card);
                    if (urgent && (!card.WaterComplete || !card.FoodComplete))
                    { card.Fallback = true; card.Reason = "Süre az! Erzak eksik kaldı; sığınak yolu öncelikli. Eksik erzak kayba neden olabilir."; }
                    return card;
                }
                string needed = card.WaterComplete ? "food" : "water";
                card.Step = card.WaterComplete ? PanicFirstWaveStepV510.Food : PanicFirstWaveStepV510.Water;
                card.Title = card.WaterComplete ? "5 · BİR GIDA AL" : "4 · FPS BAŞLADI · BİR SU AL";
                card.Body = "Fareyle bak; erzağa yaklaş. Ortadaki işareti üstüne getir, E veya F ile al.\n" +
                    "Topladığın erzak doğrudan envanterde hazır olur.";
                card.Action = "E / F · " + (needed == "water" ? "1 su al" : "1 gıda al");
                if (!FirstWaveHasInventorySpaceV510(needed))
                {
                    FirstWaveShelterV510(card); card.Fallback = true;
                    card.Reason = "Envanter dolu; erzak eksik kaldı. Tab ile aynı tür yığınları birleştirebilirsin. Yer açılmıyorsa sığınağa ilerle; erzak eksikliği kayba neden olabilir.";
                    return card;
                }
                PanicWorldSupply best = null; float nearest = float.MaxValue;
                foreach (var supply in Expedition.WorldSupplies)
                {
                    if (supply == null || supply.Count <= 0 || supply.ItemId != needed || !InteractionObject(supply.Id)) continue;
                    float distance = (supply.Position - PlayerPosition).sqrMagnitude + (supply.Floor == ActiveFloor ? 0f : 1000f);
                    if (distance < nearest) { best = supply; nearest = distance; }
                }
                if (best != null) FirstWaveTargetV510(card, best.Id, needed == "water" ? "Su" : "Gıda", best.Position, best.Floor);
                else
                {
                    FirstWaveShelterV510(card); card.Fallback = true;
                    card.Reason = (needed == "water" ? "Su" : "Gıda") + " stoğu bulunamadı; eksik erzak tamamlanmadı. Sığınağa yönel.";
                }
                return card;
            }
            if (!card.AnchorComplete && item != null && FirstWaveAnchorPossibleV510(item))
            {
                if (!card.GatherComplete || !card.MaterialsReady)
                {
                    var source = FirstWaveSourceV510(item, card.GatherComplete);
                    if (source != null)
                    {
                        card.Step = PanicFirstWaveStepV510.Materials;
                        card.Title = "1 · ÖNCE MALZEME TOPLA";
                        card.Body = "WASD ile eve gir. Parlayan dolaba yaklaş ve E ile aç.\n" +
                            "İçindeki gerçek malzeme otomatik toplanır; sonra bir mobilyayı sabitleyeceksin.";
                        card.Action = "E · Dolabı aç ve malzeme al";
                        FirstWaveTargetV510(card, source.Id, source.Label, source.Position, source.Floor);
                        return card;
                    }
                }
                else
                {
                    card.Step = PanicFirstWaveStepV510.Anchor; card.Title = "2 · BİR MOBİLYAYI SABİTLE";
                    card.Body = "Parlayan mobilyaya yaklaş ve ona dön; yakında otomatik seçilir. 1 ile sabitleme işini aç.\n" +
                        "İşaretli iki vida başını sırayla basılı tutup saat yönünde çevir.";
                    card.Action = "1 · Sabitle · İki gerçek vida";
                    FirstWaveTargetV510(card, item.Id, item.Label, item.Position, Mathf.RoundToInt(item.Position.y / 3.2f));
                    return card;
                }
            }
            if (!card.AnchorComplete)
            {
                card.Fallback = true;
                card.Reason = "Sabitleme için kullanılabilir malzeme veya bütçe yok. Bu adım tamamlanmadı; ücretsiz tesisata geç.";
            }
            if (!card.UtilitiesComplete && FirstWaveUtilityV510(card)) return card;
            card.Step = PanicFirstWaveStepV510.Ready; card.Title = "ÖNLEM SÜRESİNİ DEĞERLENDİR";
            card.Body = "İlk hedefleri gözden geçir; istersen başka eşyaları da koru.\n" +
                "Son 40 saniyede kamera otomatik FPS olur: 1 su + 1 gıda al, sonra bodrum sığınağına gir.";
            card.Action = "J · Defterden kalan önlemlere bak";
            return card;
        }

        // Root inserts this before notebook/assistant branches in GuidanceChooseGoalV54.
        bool QueueFirstWaveGuidanceV510()
        {
            if (!FirstWaveOnboardingRequiredV510) return false;
            var card = CaptureFirstWaveOnboardingV510();
            if (card.TargetId.Length > 0)
            {
                guidanceGoalsV54.Add(new GuidanceGoalV54 { Id = card.TargetId, Position = card.TargetPosition,
                    Floor = card.TargetFloor, Label = card.TargetLabel + " · " + card.Action, Action = "first-wave" });
                GuidanceQueueGoalsV54(guidanceGoalsV54);
                guidanceNoRouteV54 = "Açık yolu kontrol et; kapılara yaklaş ve E ile aç.";
            }
            return true;
        }

        // Reuses the depth-tested V59 glow renderer. It never adds arrows, world rings or remote HUD targets.
        bool TryGetFirstWaveLocalGlowV510(out string id, out Bounds bounds, out int floor)
        {
            id = ""; bounds = new Bounds(); floor = 0;
            if (!isActiveAndEnabled || !FirstWaveOnboardingRequiredV510 || !player || !View || !View.isActiveAndEnabled || !hud || !hud.FirstWaveCardEligibleV510 || WorkActive) return false;
            var card = CaptureFirstWaveOnboardingV510();
            if (card.TargetId.Length == 0 || !TryGetInteractionBounds(card.TargetId, out bounds) ||
                !PanicNotebookLocalGlowPolicyV59.CanShow(InResidence(player.position) && InResidence(card.TargetPosition),
                    ActiveFloor, card.TargetFloor, (card.TargetPosition - PlayerPosition).sqrMagnitude)) return false;
            Vector3 projected = View.WorldToViewportPoint(bounds.center);
            if (projected.z <= View.nearClipPlane || float.IsNaN(projected.x) || float.IsInfinity(projected.x) ||
                float.IsNaN(projected.y) || float.IsInfinity(projected.y)) return false;
            GeometryUtility.CalculateFrustumPlanes(View, notebookGlowFrustumV59);
            if (!GeometryUtility.TestPlanesAABB(notebookGlowFrustumV59, bounds)) return false;
            id = card.TargetId; floor = card.TargetFloor; return true;
        }
    }
}
