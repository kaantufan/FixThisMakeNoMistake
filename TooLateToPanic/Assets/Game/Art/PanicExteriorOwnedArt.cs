using System.Collections.Generic;
using UnityEngine;
namespace TooLateToPanic
{
    public sealed class PanicExteriorOwnedArt:MonoBehaviour
    {
        public readonly List<Material> Materials=new List<Material>();
        void OnDestroy(){foreach(var m in Materials)if(m){if(Application.isPlaying)Destroy(m);else DestroyImmediate(m);}}
    }
}
