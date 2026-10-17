using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        string CompoundHazardCaptionV54()
        {
            return Model.ActiveDisasterCountV54 > 1 ? "Kalkanlı · " + Model.ActiveDisasterLabelV54 +
                " aynı anda başladı. Evindeki önlemler sınanıyor." : PanicKktcStoryV53.HazardNarrative(Model.Disaster);
        }
        string CompoundAftermathCaptionV54()
        {
            return Model.ActiveDisasterCountV54 > 1 ? "Dalga " + Model.Wave + " · " + Model.ActiveDisasterLabelV54 +
                " geride kaldı. Evin durumunu incele." : PanicKktcStoryV53.AftermathNarrative(Model.Wave, Model.Disaster);
        }
        GameObject CreateCompoundAftermathV54()
        {
            if (Model.ActiveDisasterCountV54 <= 1) return PanicAtmosphereArt.CreateAftermath(Model.Disaster);
            var root = new GameObject("V54 combined aftermath");
            for (int i = 0; i < 4; i++) if (Model.HasActiveDisasterV54((DisasterType)i))
            {
                var child = PanicAtmosphereArt.CreateAftermath((DisasterType)i);
                if (child) child.transform.SetParent(root.transform, true);
            }
            return root;
        }
    }
}
