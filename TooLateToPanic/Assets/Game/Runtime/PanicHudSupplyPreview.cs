using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Two cached, lit renders of the actual pickup meshes. Never ticks or owns gameplay.</summary>
    public sealed class PanicHudSupplyPreview : IDisposable
    {
        public const int PreviewLayer = 29;
        public const int Resolution = 256;
        GameObject studio;
        Camera camera;
        Light key, fill;
        RenderTexture water, food;
        bool disposed, failed;
        int renderCount;

        public int CachedPreviewCount { get { return (water ? 1 : 0) + (food ? 1 : 0); } }
        public int RenderCount { get { return renderCount; } }

        public Texture Get(string id)
        {
            if (disposed || failed || (id != "water" && id != "food")) return null;
            if (id == "water" && water) return water;
            if (id == "food" && food) return food;
            // Creation/rendering is a repaint-only, two-time cost, never a Layout event side effect.
            if (Event.current != null && Event.current.type != EventType.Repaint) return null;
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return null;
            try
            {
                EnsureStudio();
                RenderTexture preview = Render(id);
                if (id == "water") water = preview; else food = preview;
                return preview;
            }
            catch (Exception ex)
            {
                failed = true;
                Debug.LogWarning("HUD_SUPPLY_PREVIEW_FAILED " + id + ": " + ex.Message);
                return null;
            }
        }

        void EnsureStudio()
        {
            if (studio) return;
            studio = new GameObject("HUD actual supply preview studio");
            studio.hideFlags = HideFlags.HideAndDontSave;
            studio.transform.position = new Vector3(3000, 3000, 3000);
            studio.layer = PreviewLayer;
            var cameraRoot = new GameObject("HUD preview camera");
            cameraRoot.transform.SetParent(studio.transform, false);
            camera = cameraRoot.AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.nearClipPlane = .01f;
            camera.farClipPlane = 10f;
            camera.cullingMask = 1 << PreviewLayer;
            camera.allowHDR = false;
            camera.allowMSAA = true;
            camera.useOcclusionCulling = false;
            key = StudioLight("Soft key", new Vector3(36, -35, 0), 1.65f, new Color(1f, .95f, .87f));
            fill = StudioLight("Cool fill", new Vector3(20, 145, 0), .8f, new Color(.75f, .88f, 1f));
        }

        Light StudioLight(string name, Vector3 angle, float strength, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(studio.transform, false);
            go.transform.rotation = Quaternion.Euler(angle);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = strength;
            light.cullingMask = 1 << PreviewLayer;
            light.shadows = LightShadows.None;
            light.enabled = false;
            return light;
        }

        RenderTexture Render(string id)
        {
            GameObject subject = null;
            Renderer[] renderers = null;
            RenderTexture texture = null;
            RenderTexture priorActive = RenderTexture.active;
            try
            {
                // Variant0 is the same WaterBottle/CannedFood shown in the world's supply tray.
                subject = PanicResidenceArt.CreatePickup(id, 0);
                if (!subject) throw new InvalidOperationException("Actual pickup mesh is missing.");
                subject.name = "HUD preview " + id + " (actual pickup)";
                subject.transform.SetParent(studio.transform, false);
                subject.transform.localPosition = Vector3.zero;
                subject.transform.localRotation = Quaternion.identity;
                foreach (var tr in subject.GetComponentsInChildren<Transform>(true)) tr.gameObject.layer = PreviewLayer;
                foreach (var collider in subject.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
                foreach (var body in subject.GetComponentsInChildren<Rigidbody>(true)) { body.isKinematic = true; body.detectCollisions = false; }
                renderers = subject.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) throw new InvalidOperationException("Actual pickup has no renderer.");
                Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.enabled = false;
                }
                float longest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                if (longest <= .001f) throw new InvalidOperationException("Actual pickup bounds are empty.");
                Vector3 direction = new Vector3(-.42f, .24f, -1f).normalized;
                camera.transform.position = bounds.center + direction * (1f + longest * 2f);
                camera.transform.LookAt(bounds.center);
                float halfWidth = 0f, halfHeight = 0f;
                for (int mask = 0; mask < 8; mask++)
                {
                    Vector3 corner = bounds.center + Vector3.Scale(bounds.extents,
                        new Vector3((mask & 1) == 0 ? -1 : 1, (mask & 2) == 0 ? -1 : 1, (mask & 4) == 0 ? -1 : 1));
                    Vector3 local = camera.transform.InverseTransformPoint(corner);
                    halfWidth = Mathf.Max(halfWidth, Mathf.Abs(local.x));
                    halfHeight = Mathf.Max(halfHeight, Mathf.Abs(local.y));
                }
                camera.aspect = 1f;
                camera.orthographicSize = Mathf.Max(.02f, Mathf.Max(halfWidth, halfHeight) / .86f);
                texture = new RenderTexture(Resolution, Resolution, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                texture.name = "HUD actual 3D " + id;
                texture.hideFlags = HideFlags.HideAndDontSave;
                texture.antiAliasing = Mathf.Max(1, SystemInfo.GetRenderTextureSupportedMSAASampleCount(new RenderTextureDescriptor(Resolution, Resolution, RenderTextureFormat.ARGB32, 24) { msaaSamples = 4 }));
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.Create();
                camera.targetTexture = texture;
                key.enabled = fill.enabled = true;
                foreach (var renderer in renderers) renderer.enabled = true;
                camera.Render();
                renderCount++;
                RenderTexture result = texture;
                texture = null;
                return result;
            }
            finally
            {
                if (renderers != null) foreach (var renderer in renderers) if (renderer) renderer.enabled = false;
                if (key) key.enabled = false;
                if (fill) fill.enabled = false;
                if (camera) camera.targetTexture = null;
                RenderTexture.active = priorActive;
                if (subject) UnityEngine.Object.Destroy(subject);
                if (texture) { texture.Release(); UnityEngine.Object.Destroy(texture); }
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (water) { water.Release(); UnityEngine.Object.Destroy(water); water = null; }
            if (food) { food.Release(); UnityEngine.Object.Destroy(food); food = null; }
            if (studio) UnityEngine.Object.Destroy(studio);
            studio = null; camera = null; key = null; fill = null;
        }
    }
}
