using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Cosmetic visible mesh wash/contour and pickup-only surface glints; Built-In camera passes.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class PanicSelectionSilhouette : MonoBehaviour
    {
        const CameraEvent RenderEvent = CameraEvent.BeforeImageEffects;
        static readonly int MaskId = Shader.PropertyToID("_PanicSelectionMask");
        static readonly int NotebookMaskIdV59 = Shader.PropertyToID("_PanicNotebookGlowMaskV59");
        static readonly int GatherScreenId = Shader.PropertyToID("_PanicGatherScreen");
        static readonly int GatherPhaseId = Shader.PropertyToID("_PanicGatherPhase");
        readonly List<Renderer> selected = new List<Renderer>();
        readonly List<Renderer> gather = new List<Renderer>();
        readonly List<Renderer> notebookV59 = new List<Renderer>();
        readonly Dictionary<Material, Material> masks = new Dictionary<Material, Material>();
        public PanicHomeWorld World;
        Camera view;
        CommandBuffer commands;
        Material composite;
        Material notebookCompositeV59;
        Shader shader;
        Color color;
        Color notebookColorV59;
        bool warned;
        public int SelectedRendererCount { get { return selected.Count; } }
        public int GatherRendererCount { get { return gather.Count; } }
        public bool PassReady { get { return commands != null && composite; } }
        public int NotebookGlowRendererCountV59 { get { return notebookV59.Count; } }
        public int NotebookRendererCount { get { return notebookV59.Count; } }
        public int NotebookRenderedFrame { get { return NotebookGlowDrawFrameV59; } }
        public bool NotebookGlowPassReadyV59 { get { return commands != null && notebookCompositeV59; } }
        public int NotebookGlowDrawFrameV59 { get; private set; } = -1;
        public int NotebookGlowMeshDrawCountV59 { get; private set; }

        // Independent list/material: notebook pin never borrows or replaces nearby selection ownership.
        public void SetNotebookGlowV59(List<Renderer> renderers, Color tint)
        {
            notebookV59.Clear(); notebookV59.AddRange(renderers); notebookColorV59 = tint; enabled = true;
        }
        public void ClearNotebookGlowV59() { notebookV59.Clear(); }

        public void SetSelection(List<Renderer> renderers, Color tint)
        {
            selected.Clear(); selected.AddRange(renderers); color = tint; enabled = true;
        }
        public void SetGather(List<Renderer> renderers)
        {
            gather.Clear(); gather.AddRange(renderers); enabled = true;
        }
        public void ClearSelection() { selected.Clear(); if (commands != null) commands.Clear(); }
        public void ClearGather() { gather.Clear(); if (commands != null) commands.Clear(); }

        bool EnsurePass()
        {
            if (!view) view = GetComponent<Camera>();
            if (!shader) shader = Resources.Load<Shader>("Effects/PanicSelectionSilhouette");
            if (!shader || !shader.isSupported || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat))
            {
                if (!warned) { Debug.LogWarning("Selection/pickup highlight shader unavailable; no bounds/ground fallback is drawn."); warned = true; }
                return false;
            }
            if (!composite) composite = new Material(shader) { name = "Selected asset wash and silhouette", hideFlags = HideFlags.HideAndDontSave };
            if (commands == null)
            {
                commands = new CommandBuffer { name = "Selected asset wash and pickup glints" };
                view.AddCommandBuffer(RenderEvent, commands);
            }
            view.depthTextureMode |= DepthTextureMode.Depth;
            return true;
        }

        Material MaskFor(Material source)
        {
            if (!masks.TryGetValue(source, out var mask) || !mask)
            {
                mask = new Material(shader) { name = "Actual mesh highlight", hideFlags = HideFlags.HideAndDontSave };
                masks[source] = mask;
            }
            var texture = source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
            mask.SetTexture("_MainTex", texture ? texture : Texture2D.whiteTexture);
            if (source.HasProperty("_MainTex")) { mask.SetTextureScale("_MainTex", source.GetTextureScale("_MainTex")); mask.SetTextureOffset("_MainTex", source.GetTextureOffset("_MainTex")); }
            mask.SetColor("_Color", source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white);
            bool cutout = source.IsKeywordEnabled("_ALPHATEST_ON") || source.GetTag("RenderType", false, "") == "TransparentCutout";
            mask.SetFloat("_Cutoff", cutout && source.HasProperty("_Cutoff") ? source.GetFloat("_Cutoff") : source.renderQueue >= 3000 ? .1f : -1f);
            mask.SetFloat("_Cull", source.HasProperty("_Cull") ? source.GetFloat("_Cull") : (float)CullMode.Back);
            return mask;
        }

        int DrawActualMeshes(List<Renderer> renderers, int pass)
        {
            int draws = 0;
            foreach (var renderer in renderers)
            {
                if (!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy || (view.cullingMask & (1 << renderer.gameObject.layer)) == 0) continue;
                Mesh mesh = renderer is SkinnedMeshRenderer skin ? skin.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
                if (!mesh) continue;
                if (pass == 2)
                {
                    Vector3 center = renderer.bounds.center, screen = view.WorldToViewportPoint(center);
                    if (screen.z < view.nearClipPlane) continue;
                    // A tiny screen-space glint is clipped by the actual surface triangles;
                    // the narrow moving sheen still works on hollow/irregular silhouettes.
                    commands.SetGlobalVector(GatherScreenId, new Vector4(screen.x, screen.y, 0, 0));
                    commands.SetGlobalFloat(GatherPhaseId, Mathf.Repeat(Vector3.Dot(center, new Vector3(1.173f, 2.317f, .713f)), 6.283185f));
                }
                var materials = renderer.sharedMaterials;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    if (materials.Length == 0) continue;
                    var source = materials[Mathf.Min(sub, materials.Length - 1)];
                    if (!source) continue;
                    commands.DrawRenderer(renderer, MaskFor(source), sub, pass); draws++;
                }
            }
            return draws;
        }

        void OnPreCull()
        {
            if (commands != null) commands.Clear();
            NotebookGlowMeshDrawCountV59 = 0;
            if (World) World.TickNotebookLocalGlowV59();
            else notebookV59.Clear();
            // World.Update returns early on pause/notebook, so camera-time guard also removes stale cues.
            if (World && (World.Model == null || World.Model.IsPaused || World.NotebookBlocksInput || World.InputLocked || World.CinematicActive || World.InventoryOpen)) return;
            if ((selected.Count == 0 && gather.Count == 0 && notebookV59.Count == 0) || !EnsurePass()) return;
            // Pickup glints render only on the supplied visible pickup meshes, never in space.
            if (gather.Count > 0)
            {
                commands.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                DrawActualMeshes(gather, 2);
            }
            if (notebookV59.Count > 0)
            {
                if (!notebookCompositeV59) notebookCompositeV59 = new Material(shader) { name = "Local notebook mesh glow", hideFlags = HideFlags.HideAndDontSave };
                commands.GetTemporaryRT(NotebookMaskIdV59, -1, -1, 24, FilterMode.Point, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear, 1);
                commands.SetRenderTarget(NotebookMaskIdV59);
                commands.ClearRenderTarget(true, true, Color.clear);
                NotebookGlowMeshDrawCountV59 = DrawActualMeshes(notebookV59, 0);
                if (NotebookGlowMeshDrawCountV59 > 0)
                {
                    notebookCompositeV59.SetColor("_OutlineColor", notebookColorV59);
                    commands.Blit(NotebookMaskIdV59, BuiltinRenderTextureType.CameraTarget, notebookCompositeV59, 1);
                    NotebookGlowDrawFrameV59 = Time.frameCount;
                }
                commands.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                commands.ReleaseTemporaryRT(NotebookMaskIdV59);
            }
            if (selected.Count == 0) return;
            // Independent depth attachment avoids camera/MSAA attachment mismatches.
            commands.GetTemporaryRT(MaskId, -1, -1, 24, FilterMode.Point, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear, 1);
            commands.SetRenderTarget(MaskId);
            commands.ClearRenderTarget(true, true, Color.clear);
            if (DrawActualMeshes(selected, 0) > 0)
            {
                composite.SetColor("_OutlineColor", color);
                commands.Blit(MaskId, BuiltinRenderTextureType.CameraTarget, composite, 1);
            }
            commands.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            commands.ReleaseTemporaryRT(MaskId);
        }

        void OnDisable() { DetachPass(); }
        void DetachPass()
        {
            if (commands == null) return;
            if (view) view.RemoveCommandBuffer(RenderEvent, commands);
            commands.Release(); commands = null;
        }
        void OnDestroy()
        {
            DetachPass();
            if (composite) Destroy(composite);
            if (notebookCompositeV59) Destroy(notebookCompositeV59);
            foreach (var material in masks.Values) if (material) Destroy(material);
            masks.Clear();
        }
    }
}
