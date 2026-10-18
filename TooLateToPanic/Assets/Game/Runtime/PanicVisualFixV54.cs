using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        readonly List<Renderer> floorFinishRenderersV54 = new List<Renderer>();
        public int FloorFinishRepairCountV54 { get { return floorFinishRenderersV54.Count; } }
        void InitFloorFinishRepairV54()
        {
            floorFinishRenderersV54.Clear();
            foreach (var floor in floorVisuals) if (floor)
                foreach (var r in floor.GetComponentsInChildren<Renderer>(true))
                {
                    // Millimetre-thin bevel/grain boards must receive household shadows,
                    // but must not cast low-angle self-shadow triangles across their seams.
                    if (!r.name.StartsWith("floor_finish_", System.StringComparison.Ordinal) ||
                        r.name.IndexOf("Walnut", System.StringComparison.Ordinal) < 0) continue;
                    r.shadowCastingMode = ShadowCastingMode.Off;
                    r.receiveShadows = true;
                    floorFinishRenderersV54.Add(r);
                }
        }
        public bool FloorFinishSelfShadowSuppressedV54
        {
            get
            {
                if (floorFinishRenderersV54.Count == 0) return false;
                foreach (var r in floorFinishRenderersV54)
                    if (!r || r.shadowCastingMode != ShadowCastingMode.Off || !r.receiveShadows) return false;
                return true;
            }
        }
    }
}
