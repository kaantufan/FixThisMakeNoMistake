using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Separate Built-In actual-mesh pass; never borrows selection/notebook buffers or materials.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class PanicLootSourceShimmerPassV510 : MonoBehaviour
    {
        const CameraEvent RenderEvent = CameraEvent.BeforeImageEffects;
        static readonly int TimeId = Shader.PropertyToID("_LootShimmerTimeV510");
        static readonly int ReducedId = Shader.PropertyToID("_LootShimmerReducedV510");
        readonly List<Renderer> meshes = new List<Renderer>(128);
        readonly List<string> sourceIds = new List<string>(16);
        readonly List<Material> sharedMaterials = new List<Material>(8);
        readonly Dictionary<Material, Material> overlays = new Dictionary<Material, Material>();
        PanicHomeWorld world;
        Camera view;
        CommandBuffer commands;
        Shader shader;
        bool warned;
        public int RendererCount { get { return meshes.Count; } }
        public int MeshDrawCount { get; private set; }
        public int DrawFrame { get; private set; } = -1;
        public bool PassReady { get { return commands != null && shader && shader.isSupported; } }
        public IReadOnlyList<string> VisibleSourceIds { get { return sourceIds; } }

        public void BindWorld(PanicHomeWorld value)
        {
            if (world != value) ClearSubmission();
            world = value;
            if (value) enabled = true;
        }

        bool EnsurePass()
        {
            if (!view) view = GetComponent<Camera>();
            if (!shader) shader = Resources.Load<Shader>("Effects/PanicLootSourceShimmerV510");
            if (!shader || !shader.isSupported || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            {
                if (!warned) { Debug.LogWarning("V510 loot source shimmer unavailable; actual objects keep their original materials."); warned = true; }
                return false;
            }
            if (commands == null)
            {
                commands = new CommandBuffer { name = "V510 stocked loot source surface shimmer" };
                view.AddCommandBuffer(RenderEvent, commands);
            }
            // Shared camera depth belongs to Unity/other effects; never clear another owner's flag.
            view.depthTextureMode |= DepthTextureMode.Depth;
            return true;
        }

        Material OverlayFor(Material source)
        {
            if (!overlays.TryGetValue(source, out var overlay) || !overlay)
            {
                overlay = new Material(shader) { name = "V510 stocked source overlay", hideFlags = HideFlags.HideAndDontSave };
                overlays[source] = overlay;
            }
            Texture texture = source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
            overlay.SetTexture("_MainTex", texture ? texture : Texture2D.whiteTexture);
            if (source.HasProperty("_MainTex"))
            {
                overlay.SetTextureScale("_MainTex", source.GetTextureScale("_MainTex"));
                overlay.SetTextureOffset("_MainTex", source.GetTextureOffset("_MainTex"));
            }
            else { overlay.SetTextureScale("_MainTex", Vector2.one); overlay.SetTextureOffset("_MainTex", Vector2.zero); }
            overlay.SetColor("_Color", source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white);
            bool cutout = source.IsKeywordEnabled("_ALPHATEST_ON") || source.GetTag("RenderType", false, "") == "TransparentCutout";
            overlay.SetFloat("_Cutoff", cutout && source.HasProperty("_Cutoff") ? source.GetFloat("_Cutoff") : source.renderQueue >= 3000 ? .1f : -1f);
            overlay.SetFloat("_Cull", source.HasProperty("_Cull") ? source.GetFloat("_Cull") : (float)CullMode.Back);
            return overlay;
        }

        void OnPreCull()
        {
            ClearSubmission();
            if (!view) view = GetComponent<Camera>();
            if (!world || !world.isActiveAndEnabled || !view || world.View != view) return;
            world.CollectLootSourceShimmerMeshesV510(view, meshes, sourceIds);
            if (meshes.Count == 0 || !EnsurePass()) return;
            commands.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            commands.SetGlobalFloat(TimeId, Time.unscaledTime);
            commands.SetGlobalFloat(ReducedId, world.LootShimmerReducedMotionV510 ? 1f : 0f);
            foreach (var renderer in meshes)
            {
                if (!renderer || !renderer.enabled || renderer.forceRenderingOff || !renderer.gameObject.activeInHierarchy ||
                    (view.cullingMask & (1 << renderer.gameObject.layer)) == 0) continue;
                Mesh mesh = renderer is SkinnedMeshRenderer skin ? skin.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
                if (!mesh) continue;
                sharedMaterials.Clear(); renderer.GetSharedMaterials(sharedMaterials);
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    if (sharedMaterials.Count == 0) continue;
                    var source = sharedMaterials[Mathf.Min(sub, sharedMaterials.Count - 1)];
                    if (!source) continue;
                    commands.DrawRenderer(renderer, OverlayFor(source), sub, 0);
                    MeshDrawCount++;
                }
            }
            if (MeshDrawCount > 0) DrawFrame = Time.frameCount;
        }

        void ClearSubmission()
        {
            if (commands != null) commands.Clear();
            meshes.Clear(); sourceIds.Clear(); MeshDrawCount = 0; DrawFrame = -1;
        }

        void OnDisable() { ClearSubmission(); DetachPass(); }
        void DetachPass()
        {
            if (commands == null) return;
            if (view) view.RemoveCommandBuffer(RenderEvent, commands);
            commands.Release(); commands = null;
        }
        void OnDestroy()
        {
            DetachPass();
            foreach (var overlay in overlays.Values) if (overlay) Destroy(overlay);
            overlays.Clear();
        }
    }
}
