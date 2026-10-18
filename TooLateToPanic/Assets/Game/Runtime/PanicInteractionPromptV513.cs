using UnityEngine;
namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        // Pure observation: never invokes work preflight or changes focus/RNG.
        public bool TryInteractionPromptV513(out string key, out string verb, out Vector3 position)
        {
            key = verb = string.Empty; position = Vector3.zero;
            if (Model == null || Expedition == null || !View || Model.IsPaused || InputLocked ||
                NotebookBlocksInput || CinematicActive || LossAnnounced || WorkActive || IsMoving || InventoryOpen || LootVisible ||
                BagCameraTransitionActive || (Model.Phase != PanicPhase.Preparing && Model.Phase != PanicPhase.Tutorial) ||
                !CanInteract || !FocusPosition(out position)) return false;
            bool bag = BagFinalStageActive || BagCameraOwnsView;
            if (focusId == "bunker" || focusId == "@bunker" || focusId == "@panicroom")
            {
                bool correct = UsesUpperPanicRoomV5 ? focusId == "@panicroom" : focusId != "@panicroom";
                if (!bag || !correct) return false;
                key = "E"; verb = "Güvenli alana gir";
            }
            else if (BagFirstPersonActive)
            {
                var supply = Expedition.FindWorldSupply(focusId);
                if (supply != null && supply.Count > 0) { key = "E"; verb = "Al · " + PanicExpedition.DisplayName(supply.ItemId); }
                else if (hinges.TryGetValue(focusId ?? "", out var door) && door.directionalDoor)
                { key = "E"; verb = door.open ? "Kapat" : "Aç"; }
                else if (focusId == "@garage" && !Model.UtilitySafety.PowerOff)
                { key = "E"; verb = garageOpen ? "Kapat" : "Aç"; }
            }
            else if (focusId == "@bagstation") { key = "TAB"; verb = "Çantayı düzenle"; }
            else if (!bag && NearestSource != null) { key = "E"; verb = NearestSource.Open ? "Kapat" : "Aç ve malzemeyi al"; }
            else if (!bag && hinges.TryGetValue(focusId ?? "", out var hinge)) { key = "E"; verb = hinge.open ? "Kapat" : "Aç"; }
            else if (!bag)
            {
                var item = Selected;
                // Roof and decorative items do not acquire an invented action.
                if (item != null && item.Kind == ItemKind.Roof) return false;
                if (item != null && PanicItemActionRules.CanReleaseAnchor(item) && Model.CanBeginJuryWork("anchorwall", item.Id))
                { key = "1"; verb = "Ankrajı çöz"; }
                else if (item != null)
                {
                    string action = Expedition.PrimaryActionV414(item);
                    if (!string.IsNullOrEmpty(action) && Model.CanBeginJuryWork(action, item.Id))
                    { key = Expedition.PrimaryShortcutV414(item).ToString(); verb = Expedition.PrimaryLabelV414(item); }
                }
                if (key.Length == 0 && !string.IsNullOrEmpty(ShortAction))
                {
                    bool physical = NearestSource != null || hinges.ContainsKey(focusId ?? "") ||
                        focusId == "@radio" || focusId == "@fireplace" || focusId == "@computer" ||
                        (focusId == "@garage" && !Model.UtilitySafety.PowerOff);
                    PanicSelectedTargetSnapshotV56 snapshot;
                    bool work = TryGetSelectedTargetSnapshotV56(out snapshot) && !string.IsNullOrEmpty(snapshot.Action) && item == null;
                    if (physical || work) { key = "E"; verb = work ? snapshot.ActionLabel : ShortAction; }
                }
            }
            position += Vector3.up * .9f;
            return key.Length > 0 && !string.IsNullOrWhiteSpace(verb);
        }
    }
}


