using System.Collections.Generic;
using UnityEngine;
namespace TooLateToPanic
{
    public sealed class PanicStormArtState:MonoBehaviour
    {
        public readonly List<ParticleSystem> Emitters=new List<ParticleSystem>();
        public readonly List<float> Rates=new List<float>();
        public readonly List<LineRenderer> Ribbons=new List<LineRenderer>();
        public readonly List<Material> Materials=new List<Material>();
        public Texture2D Soft;public Mesh Leaf;
        // Hybrid resources have one owner here. Core meshes remain owned by PanicStormMotionV414.
        public GameObject YarkinHybridRoot;
        public Mesh YarkinHybridShell, YarkinHybridPuff, YarkinHybridShard;
        public Vector3[] YarkinHybridShellVertices;
        public Color[] YarkinHybridShellColors;
        public Texture2D YarkinHybridTexture;
        public Material YarkinHybridFootMaterial,YarkinHybridShardMaterial;
        public Transform YarkinHybridFoot;
        public readonly List<LineRenderer> YarkinHybridBands=new List<LineRenderer>();
        public Vector3[][] YarkinHybridBandBuffers;
        public Transform[] YarkinHybridShards;

        void OnDestroy()
        {
            foreach(var m in Materials)if(m)Release(m);
            Materials.Clear();
            if(Soft)Release(Soft);if(Leaf)Release(Leaf);
            if(YarkinHybridShell)Release(YarkinHybridShell);
            if(YarkinHybridPuff)Release(YarkinHybridPuff);
            if(YarkinHybridShard)Release(YarkinHybridShard);
            if(YarkinHybridTexture)Release(YarkinHybridTexture);
            Soft=null;Leaf=null;YarkinHybridShell=null;YarkinHybridPuff=null;YarkinHybridShard=null;YarkinHybridTexture=null;
            // Renderers and transforms are children of the existing hazard root and die with it.
            Emitters.Clear();Rates.Clear();Ribbons.Clear();YarkinHybridBands.Clear();
        }
        internal static void Release(Object value){if(Application.isPlaying)Destroy(value);else DestroyImmediate(value);}
    }
}
