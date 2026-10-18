using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Tracks dynamically instantiated procedural meshes and materials on exterior NPC visuals and decals.
    /// [ExecuteAlways] guarantees OnDestroy is invoked during DestroyImmediate in edit/batchmode.
    /// </summary>
    [ExecuteAlways]
    public sealed class PanicExteriorOwnedArt : MonoBehaviour
    {
        public readonly List<Material> Materials = new List<Material>();
        public readonly List<Mesh> Meshes = new List<Mesh>();

        void OnDestroy()
        {
            foreach (var m in Materials)
            {
                if (m)
                {
                    if (Application.isPlaying) Destroy(m);
                    else DestroyImmediate(m);
                }
            }
            Materials.Clear();

            foreach (var mesh in Meshes)
            {
                if (mesh)
                {
                    if (Application.isPlaying) Destroy(mesh);
                    else DestroyImmediate(mesh);
                }
            }
            Meshes.Clear();
        }
    }
}
