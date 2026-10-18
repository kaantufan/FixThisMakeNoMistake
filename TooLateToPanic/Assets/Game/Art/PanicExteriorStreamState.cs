using System;
using UnityEngine;

namespace TooLateToPanic
{
    // Own filename is required: this component and its references survive landscape prefab baking.
    public sealed class PanicExteriorStreamState:MonoBehaviour
    {
        public Mesh Water,Foam;
        public MeshRenderer WaterRenderer,FoamRenderer;
        [NonSerialized]public bool RuntimeOwned,Initialized;
        [NonSerialized]public float Surface;
        [NonSerialized]public Vector3[] WaterVertices,FoamVertices;

        public void EnsureRuntimeOwnership()
        {
            if(!Application.isPlaying||RuntimeOwned)return;
            Water=Instantiate(Water);Foam=Instantiate(Foam);Water.MarkDynamic();Foam.MarkDynamic();
            WaterRenderer.GetComponent<MeshFilter>().sharedMesh=Water;
            FoamRenderer.GetComponent<MeshFilter>().sharedMesh=Foam;
            WaterRenderer.sharedMaterial=new Material(WaterRenderer.sharedMaterial);
            FoamRenderer.sharedMaterial=new Material(FoamRenderer.sharedMaterial);
            RuntimeOwned=true;Initialized=false;
        }
        void OnDestroy()
        {
            if(!RuntimeOwned)return;
            if(Water)Destroy(Water);if(Foam)Destroy(Foam);
            if(WaterRenderer&&WaterRenderer.sharedMaterial)Destroy(WaterRenderer.sharedMaterial);
            if(FoamRenderer&&FoamRenderer.sharedMaterial)Destroy(FoamRenderer.sharedMaterial);
        }
    }
}
