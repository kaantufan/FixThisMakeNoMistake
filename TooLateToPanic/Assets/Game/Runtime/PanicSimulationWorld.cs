using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        public bool StartDisasterSimulation(DisasterType type)
        {
            ReturnToMenu();
            if(!Model.StartSimulation(type))return false;
            ConfigureResidenceModel(); Expedition.ResetForSimulation(); ResetResidenceLife(); ClearGarden();
            previousWave=Model.Wave; previousPhase=Model.Phase; focusId=null; InventoryOpen=false;
            Warp(new Vector3(0,.05f,-7.5f)); SyncVisuals(true);
            if(effects)effects.OnPhase(Model.Phase); UpdateContext(); return true;
        }

        public void TriggerSimulation()
        {
            if(!Model.IsSimulation||Model.Phase!=PanicPhase.Preparing||CinematicActive||LossAnnounced)return;
            CancelWork();CancelMove();InventoryOpen=false;
            Model.EnterBunker();
        }

        public bool InspectSimulationDamage()
        {
            if (CinematicActive || LossAnnounced) return false;
            return Model.InspectSimulationDamage();
        }
    }
}
