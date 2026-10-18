using System.Collections.Generic;
using UnityEngine;
namespace TooLateToPanic
{
    /// <summary>Releases only explicitly registered runtime clones and procedural cosmetic meshes.</summary>
    [ExecuteAlways]
    public sealed class PanicExteriorOwnedArt : MonoBehaviour
    {
        public readonly List<Material> Materials = new List<Material>();
        public readonly List<Mesh> Meshes = new List<Mesh>();
        public void Own(Material value) { if (value && !Materials.Contains(value)) Materials.Add(value); }
        public void Own(Mesh value) { if (value && !Meshes.Contains(value)) Meshes.Add(value); }
        void OnDestroy()
        {
            var released = new HashSet<int>();
            foreach (var value in Materials) Release(value, released);
            foreach (var value in Meshes) Release(value, released);
            Materials.Clear(); Meshes.Clear();
        }
        static void Release(Object value, HashSet<int> released)
        {
            if (!value || !released.Add(value.GetInstanceID())) return;
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }
    }
}
