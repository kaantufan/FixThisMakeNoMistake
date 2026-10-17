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
        void OnDestroy(){foreach(var m in Materials)if(m)Release(m);if(Soft)Release(Soft);if(Leaf)Release(Leaf);}
        static void Release(Object value){if(Application.isPlaying)Destroy(value);else DestroyImmediate(value);}
    }
}
