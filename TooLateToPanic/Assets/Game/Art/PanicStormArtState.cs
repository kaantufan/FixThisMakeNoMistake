using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    [ExecuteAlways]
    public sealed class PanicStormArtState : MonoBehaviour
    {
        public readonly List<ParticleSystem> Emitters = new List<ParticleSystem>();
        public readonly List<float> Rates = new List<float>();
        public readonly List<LineRenderer> Ribbons = new List<LineRenderer>();
        public readonly List<Material> Materials = new List<Material>();

        public Texture2D Soft;
        public Mesh Leaf;

        // M2 3D High-Visibility Tornado Vortex Geometry & State
        public GameObject TornadoFunnelRoot;
        public Mesh FunnelOuterMesh, FunnelInnerMesh;
        public Vector3[] FunnelOuterVerts, FunnelInnerVerts;
        public Material FunnelOuterMaterial, FunnelInnerMaterial;
        public Transform FootCloudTransform;
        public Mesh FootCloudMesh, FootDebrisMesh;
        public Material FootCloudMaterial, FootDebrisMaterial;
        public List<LineRenderer> SpiralBands = new List<LineRenderer>();
        public Vector3[][] SpiralBandBuffers;
        public List<Object> Owned = new List<Object>();

        public static void Release(Object value)
        {
            if (!value) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        void OnDestroy()
        {
            for (int i = 0; i < Owned.Count; i++)
            {
                if (Owned[i]) Release(Owned[i]);
            }
            Owned.Clear();

            for (int i = 0; i < Materials.Count; i++)
            {
                if (Materials[i]) Release(Materials[i]);
            }
            Materials.Clear();

            if (Soft) { Release(Soft); Soft = null; }
            if (Leaf) { Release(Leaf); Leaf = null; }
            if (FunnelOuterMesh) { Release(FunnelOuterMesh); FunnelOuterMesh = null; }
            if (FunnelInnerMesh) { Release(FunnelInnerMesh); FunnelInnerMesh = null; }
            if (FootCloudMesh) { Release(FootCloudMesh); FootCloudMesh = null; }
            if (FootDebrisMesh) { Release(FootDebrisMesh); FootDebrisMesh = null; }
            if (FunnelOuterMaterial) { Release(FunnelOuterMaterial); FunnelOuterMaterial = null; }
            if (FunnelInnerMaterial) { Release(FunnelInnerMaterial); FunnelInnerMaterial = null; }
            if (FootCloudMaterial) { Release(FootCloudMaterial); FootCloudMaterial = null; }
            if (FootDebrisMaterial) { Release(FootDebrisMaterial); FootDebrisMaterial = null; }

            for (int i = 0; i < Emitters.Count; i++)
            {
                if (Emitters[i]) Emitters[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            Emitters.Clear();
            Rates.Clear();
            Ribbons.Clear();
            SpiralBands.Clear();
        }
    }
}
