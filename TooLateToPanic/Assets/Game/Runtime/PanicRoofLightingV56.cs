using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    // Presentation only. No collider, item position, interaction, cost, clock or power mutation.
    public sealed partial class PanicHomeWorld
    {
        sealed class LintelVisualV56
        {
            public Renderer Renderer;
            public BoxCollider Collider;
            public Vector3 Position, Scale, Center, Size;
            public Quaternion Rotation;
            public bool Enabled, Trigger;
            public int Layer;
        }
        readonly List<LintelVisualV56> doorLintelsV56 = new List<LintelVisualV56>();
        GameObject roofSelectionV56;
        Renderer roofSelectionRendererV56;
        bool visualReadabilityReadyV56;

        public bool RoofSelectionFeedbackVisibleV56 { get { return roofSelectionV56 && roofSelectionV56.activeInHierarchy && roofSelectionRendererV56 && roofSelectionRendererV56.enabled; } }
        public int RoofSelectionFeedbackRendererCountV56 { get { return RoofSelectionFeedbackVisibleV56 ? 1 : 0; } }
        public int DoorLintelHiddenRendererCountV56
        {
            get { int n=0;foreach(var entry in doorLintelsV56)if(entry.Renderer&&!entry.Renderer.enabled&&entry.Renderer.GetComponent<PanicPermanentlyHiddenV414>())n++;return n; }
        }
        public int DoorLintelEnabledColliderCountV56
        {
            get { int n=0;foreach(var entry in doorLintelsV56)if(entry.Collider&&entry.Collider.enabled&&entry.Collider.gameObject.activeInHierarchy)n++;return n; }
        }
        public bool DoorLintelsPhysicsPreservedV56
        {
            get
            {
                if(doorLintelsV56.Count==0)return false;
                foreach(var entry in doorLintelsV56)
                {
                    var c=entry.Collider;
                    if(!c||c.enabled!=entry.Enabled||c.isTrigger!=entry.Trigger||c.gameObject.layer!=entry.Layer||
                        !c.transform.localPosition.Equals(entry.Position)||!c.transform.localRotation.Equals(entry.Rotation)||!c.transform.localScale.Equals(entry.Scale)||!c.center.Equals(entry.Center)||!c.size.Equals(entry.Size))return false;
                }
                return true;
            }
        }
        public int RoofFramingRendererCountV56
        {
            get
            {
                if(!hardware.TryGetValue("roof",out var holder)||!holder)return 0;
                int n=0;foreach(var r in holder.GetComponentsInChildren<Renderer>(true))if(r&&r.name.StartsWith("Roof_",StringComparison.Ordinal))n++;return n;
            }
        }
        public string VisualQualityProfileV56 { get { return visualReadabilityReadyV56 ? "Built-In neutral dusk / warm mains / soft sun / preserved V54 floor shadows" : "not initialized"; } }
        public Color VisualAmbientSkyV56 { get { return RenderSettings.ambientSkyColor; } }
        public Color VisualAmbientEquatorV56 { get { return RenderSettings.ambientEquatorColor; } }
        public Color VisualAmbientGroundV56 { get { return RenderSettings.ambientGroundColor; } }
        public float VisualSunIntensityV56 { get { return revisionSun ? revisionSun.intensity : 0; } }

        void InitVisualReadabilityV56()
        {
            if(visualReadabilityReadyV56)return;
            visualReadabilityReadyV56=true;
            foreach(var floor in floorVisuals) HideDoorLintelsV56(floor);
            HideDoorLintelsV56(residenceBathroom);HideDoorLintelsV56(residenceGarage);
            CreateRoofSelectionV56();
            if(View)
            {
                var guard=View.gameObject.AddComponent<PanicRoofVisualGuardV56>();guard.World=this;
                View.allowHDR=true;
            }
            // Tune only existing light owners. Mains registration and breaker remain authoritative.
            foreach(var light in mainsLightsV5)
            {
                if(!light)continue;
                if(light.name.StartsWith("Interior lamp ",StringComparison.Ordinal))
                {light.color=new Color(1f,.85f,.68f);light.intensity=1.45f;light.range=7.8f;}
                else if(light.name=="Service room lamp")
                {light.color=new Color(1f,.84f,.65f);light.intensity=1.6f;light.range=7.2f;}
            }
            if(revisionSun)
            {
                revisionSun.color=new Color(.88f,.90f,.94f);
                revisionSun.transform.rotation=Quaternion.Euler(38,-38,0);
                revisionSun.shadows=LightShadows.Soft;revisionSun.shadowStrength=.64f;
                revisionSun.shadowBias=.028f;revisionSun.shadowNormalBias=.24f;
            }
            QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;
            QualitySettings.shadowProjection=ShadowProjection.StableFit;QualitySettings.shadowCascades=2;
            QualitySettings.shadowDistance=60;QualitySettings.pixelLightCount=8;
            ApplyAmbientReadabilityV56();RefreshVisualReadabilityV56();
        }

        void HideDoorLintelsV56(GameObject root)
        {
            if(!root)return;
            var colliders=root.GetComponentsInChildren<BoxCollider>(true);
            foreach(var r in root.GetComponentsInChildren<Renderer>(true))
            {
                // Window headers, jambs, door leaves, arch trims and other walls are untouched.
                if(!r||!r.name.StartsWith("solid_",StringComparison.Ordinal)||r.name.IndexOf("lintel",StringComparison.Ordinal)<0)continue;
                BoxCollider physical=null;
                foreach(var c in colliders)if(c.name=="Collision_"+r.name){physical=c;break;}
                // Preserve an unmatched asset rather than remove an unverified structure.
                if(!physical){Debug.LogWarning("V56 lintel visual skipped: no exact physical sibling for "+r.name);continue;}
                doorLintelsV56.Add(new LintelVisualV56{Renderer=r,Collider=physical,Enabled=physical.enabled,Trigger=physical.isTrigger,Layer=physical.gameObject.layer,
                    Position=physical.transform.localPosition,Rotation=physical.transform.localRotation,Scale=physical.transform.localScale,Center=physical.center,Size=physical.size});
                if(!r.GetComponent<PanicPermanentlyHiddenV414>())r.gameObject.AddComponent<PanicPermanentlyHiddenV414>();
                r.enabled=false;
            }
        }

        // Called by the existing atmosphere owner after basementBlend has advanced.
        void ApplyAmbientReadabilityV56()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=Color.Lerp(new Color(.42f,.465f,.52f),new Color(.10f,.115f,.14f),basementBlend);
            RenderSettings.ambientEquatorColor=Color.Lerp(new Color(.32f,.345f,.38f),new Color(.075f,.082f,.10f),basementBlend);
            RenderSettings.ambientGroundColor=Color.Lerp(new Color(.20f,.205f,.22f),new Color(.045f,.048f,.06f),basementBlend);
            if(revisionSun)revisionSun.intensity=Mathf.Lerp(.72f,.07f,basementBlend);
        }

        bool RoofSelectionAllowedV56()
        {
            if(!visualReadabilityReadyV56||Model==null||!View||!View.orthographic||!player||
                Model.IsPaused||Model.WaveLedgerOpenV56||Model.KeyboardHelpOpenV56||NotebookBlocksInput||InputLocked||CinematicActive||LossAnnounced||
                InventoryOpen||WorkActive||IsMoving||BagCameraOwnsView||BagCameraTransitionActive||!JuryAllowsMovement||
                (hud&&(hud.DeveloperMenuOpen||hud.DeveloperInputBlockedThisFrame||hud.NotebookModalBlocked)))return false;
            bool playable=Model.Phase==PanicPhase.Preparing||Model.Phase==PanicPhase.Tutorial||Model.Phase==PanicPhase.Inspection;
            return playable&&Selected!=null&&Selected.Kind==ItemKind.Roof&&Selected.Installed&&CanInteract;
        }

        // Also called at camera time, since World.Update deliberately returns early on modals.
        public void RefreshVisualReadabilityV56()
        {
            if(!visualReadabilityReadyV56)return;
            bool selected=RoofSelectionAllowedV56();
            if(roofSelectionV56)roofSelectionV56.SetActive(selected);
            // Reapply renderer-only hiding after any cache-based visibility update.
            foreach(var entry in doorLintelsV56)if(entry.Renderer)entry.Renderer.enabled=false;
            bool cutaway=inside&&!ResidenceCinematicExteriorView&&(!BagCameraOwnsView||BagDollyUsesIsometricCutawayV56);
            if(hardware.TryGetValue("roof",out var holder)&&holder)
                foreach(Transform child in holder.transform)PanicConstructionArt.SetRoofFrameSelectionV56(child.gameObject,selected,cutaway);
            if(constructionPreview&&WorkActive&&workTarget=="roof"&&workAction=="upgrade")
                PanicConstructionArt.SetRoofFrameSelectionV56(constructionPreview,true,cutaway);
        }

        void CreateRoofSelectionV56()
        {
            // Actual roof eave dimensions from the current shell source, independent of its active state.
            float x=8.575f,z=6.575f,y=6.44f;
            var item=Model.Items.Find(value=>value.Kind==ItemKind.Roof);
            if(item!=null)y=item.Position.y+.04f;
            roofSelectionV56=new GameObject("Selected roof architectural boundary V56");
            roofSelectionV56.transform.SetParent(transform,false);roofSelectionV56.transform.position=Vector3.zero;
            var geometry=new PanicRoofCueGeometryV56();
            // Continuous narrow eaves define one architectural object; no fill or floating marker.
            geometry.Box(new Vector3(0,y,-z),new Vector3(x*2,.075f,.075f));
            geometry.Box(new Vector3(0,y,z),new Vector3(x*2,.075f,.075f));
            geometry.Box(new Vector3(-x,y,0),new Vector3(.075f,.075f,z*2));
            geometry.Box(new Vector3(x,y,0),new Vector3(.075f,.075f,z*2));
            // Slightly heavier joined corners remain readable at the wide isometric zoom.
            for(int sx=-1;sx<=1;sx+=2)for(int sz=-1;sz<=1;sz+=2)
            {
                geometry.Box(new Vector3(sx*(x-.7f),y+.025f,sz*z),new Vector3(1.4f,.09f,.12f));
                geometry.Box(new Vector3(sx*x,y+.025f,sz*(z-.7f)),new Vector3(.12f,.09f,1.4f));
            }
            Mesh mesh=geometry.Mesh();roofSelectionV56.AddComponent<MeshFilter>().sharedMesh=mesh;
            var shader=Shader.Find("Unlit/Color");
            if(!shader)shader=Shader.Find("Standard");
            var material=new Material(shader){name="TLTP V56 architectural selected brass",color=new Color(1f,.72f,.23f)};
            var r=roofSelectionV56.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
            roofSelectionRendererV56=r;
            var resources=roofSelectionV56.AddComponent<PanicRoofCueResourcesV56>();resources.Mesh=mesh;resources.Material=material;
            roofSelectionV56.SetActive(false);
        }
    }

    // Does not own time/input/lighting. Keeps cues honest on pause/ledger/notebook frames too.
    public sealed class PanicRoofVisualGuardV56 : MonoBehaviour
    {
        public PanicHomeWorld World;
        void OnPreCull(){if(World&&World.isActiveAndEnabled)World.RefreshVisualReadabilityV56();}
    }
    public sealed class PanicRoofCueResourcesV56 : MonoBehaviour
    {
        public Mesh Mesh;public Material Material;
        void OnDestroy(){if(Mesh)Destroy(Mesh);if(Material)Destroy(Material);}
    }
    // Direct mesh creation avoids CreatePrimitive's transient colliders entirely.
    sealed class PanicRoofCueGeometryV56
    {
        readonly List<Vector3> vertices=new List<Vector3>();readonly List<int> triangles=new List<int>();
        static readonly int[] Faces={0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,0,4,7,0,7,3,1,2,6,1,6,5};
        public void Box(Vector3 center,Vector3 size)
        {
            int first=vertices.Count;Vector3 h=size*.5f;
            vertices.Add(center+new Vector3(-h.x,-h.y,-h.z));vertices.Add(center+new Vector3(h.x,-h.y,-h.z));
            vertices.Add(center+new Vector3(h.x,h.y,-h.z));vertices.Add(center+new Vector3(-h.x,h.y,-h.z));
            vertices.Add(center+new Vector3(-h.x,-h.y,h.z));vertices.Add(center+new Vector3(h.x,-h.y,h.z));
            vertices.Add(center+new Vector3(h.x,h.y,h.z));vertices.Add(center+new Vector3(-h.x,h.y,h.z));
            foreach(int index in Faces)triangles.Add(first+index);
        }
        public Mesh Mesh(){var mesh=new Mesh{name="V56 architectural eave boundary, no collision"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
    }
}
