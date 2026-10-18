using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    // Visibility cue only. Original mesh/bones/materials, collision and camera remain authoritative.
    [DefaultExecutionOrder(1000)] // Evaluate after the world/camera LateUpdate has selected this frame's view.
    public sealed class PanicActorOcclusion : MonoBehaviour
    {
        PanicHomeWorld world; Transform actor,hero; Material owned,maskMaterial;
        readonly List<Renderer> originals=new List<Renderer>(),overlays=new List<Renderer>(),masks=new List<Renderer>();
        static readonly float[] BodyHeights={.55f,1.05f,1.55f};
        Renderer[] candidates=new Renderer[0]; float refreshAt; Vector3 refreshedPosition;
        readonly List<string> occluders=new List<string>();
        public bool IsOccluded { get; private set; }
        public int OverlayCount => overlays.Count;
        public int ActiveOverlayCount => overlays.Count(r=>r&&r.enabled&&r.gameObject.activeInHierarchy);
        public IReadOnlyList<string> Occluders => occluders;
        public IReadOnlyList<Renderer> Overlays => overlays;
        public bool ShaderSupported => owned&&owned.shader&&owned.shader.isSupported&&maskMaterial&&maskMaterial.shader&&maskMaterial.shader.isSupported;
        public void RefreshCandidates(){refreshAt=0;}
        public void Init(PanicHomeWorld host,Transform player,Transform model)
        {
            world=host;actor=player;hero=model;
            var shader=Resources.Load<Shader>("Effects/PanicActorOcclusion");
            if(!shader){Debug.LogError("Actor occlusion cue shader missing");enabled=false;return;}
            owned=new Material(shader){name="Owned actor occlusion cue"};
            owned.SetColor("_Color",new Color(1f,.73f,.27f,.66f));
            owned.renderQueue=3020;
            var maskShader=Resources.Load<Shader>("Effects/PanicActorOcclusionMask");
            if(!maskShader){Debug.LogError("Actor visibility stencil shader missing");enabled=false;return;}
            maskMaterial=new Material(maskShader){name="Owned visible actor stencil",renderQueue=3019};
            var source=hero.GetComponentsInChildren<Renderer>(true);
            foreach(var skin in source)
            {
                var skinned=skin as SkinnedMeshRenderer;var filter=skin.GetComponent<MeshFilter>();Mesh mesh=skinned?skinned.sharedMesh:filter?filter.sharedMesh:null;
                if(!mesh)continue;
                originals.Add(skin);masks.Add(Clone(skin,mesh,maskMaterial,"Actor visibility stencil"));overlays.Add(Clone(skin,mesh,owned,"Actor visibility cue"));
            }
        }
        Renderer Clone(Renderer source,Mesh mesh,Material material,string label)
        {
            var go=new GameObject(label);go.layer=source.gameObject.layer;go.transform.SetParent(source.transform,false);Renderer clone;
            if(source is SkinnedMeshRenderer skin){var r=go.AddComponent<SkinnedMeshRenderer>();r.sharedMesh=mesh;r.bones=skin.bones;r.rootBone=skin.rootBone;r.localBounds=skin.localBounds;r.quality=skin.quality;r.updateWhenOffscreen=true;clone=r;}
            else{go.AddComponent<MeshFilter>().sharedMesh=mesh;clone=go.AddComponent<MeshRenderer>();}
            clone.sharedMaterials=Enumerable.Repeat(material,Mathf.Max(1,mesh.subMeshCount)).ToArray();
            clone.shadowCastingMode=ShadowCastingMode.Off;clone.receiveShadows=false;clone.allowOcclusionWhenDynamic=false;clone.lightProbeUsage=LightProbeUsage.Off;clone.reflectionProbeUsage=ReflectionProbeUsage.Off;clone.enabled=false;return clone;
        }
        void LateUpdate()
        {
            occluders.Clear();IsOccluded=false;
            bool playable=world&&world.Model!=null&&(world.Model.Phase==PanicPhase.Preparing||world.Model.Phase==PanicPhase.Tutorial||world.Model.Phase==PanicPhase.Inspection);
            if(!playable||!actor||!hero||!hero.gameObject.activeInHierarchy||!world.View||world.CinematicActive||world.WorkActive&&!world.Expedition.Work.IsWorldConstruction){SetVisible(false);return;}
            // Retained shoes slightly intersect the walk surface. Contact pixels are
            // not an obstacle hiding the actor; keep their original ground appearance.
            if(owned)owned.SetFloat("_MinimumY",actor.position.y+.18f);
            if(Time.unscaledTime>=refreshAt||Vector3.Distance(actor.position,refreshedPosition)>2f)
            {
                refreshAt=Time.unscaledTime+1f;refreshedPosition=actor.position;
                candidates=FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r&&!r.transform.IsChildOf(actor)&&r.gameObject.layer!=28&&r.gameObject.layer!=30&&r.bounds.SqrDistance(actor.position)<400f).ToArray();
            }
            foreach(float height in BodyHeights)
            {
                Vector3 target=actor.position+Vector3.up*height,screen=world.View.WorldToScreenPoint(target);
                if(screen.z<=0||screen.x<0||screen.x>Screen.width||screen.y<0||screen.y>Screen.height)continue;
                Ray ray=world.View.ScreenPointToRay(screen);float distance=Vector3.Dot(target-ray.origin,ray.direction);
                foreach(var r in candidates)
                {
                    if(!r||!r.enabled||!r.gameObject.activeInHierarchy||r.forceRenderingOff)continue;
                    if(r.bounds.IntersectRay(ray,out float hit)&&hit<distance-.18f)
                    {IsOccluded=true;if(occluders.Count<8&&!occluders.Contains(r.name))occluders.Add(r.name);}
                }
            }
            SetVisible(IsOccluded);
        }
        void SetVisible(bool visible){for(int i=0;i<overlays.Count;i++)if(overlays[i]){bool shown=visible&&originals[i]&&originals[i].enabled&&!originals[i].forceRenderingOff&&originals[i].gameObject.activeInHierarchy;overlays[i].enabled=shown;if(masks[i])masks[i].enabled=shown;}}
        void OnDisable(){SetVisible(false);}
        void OnDestroy(){if(owned)Destroy(owned);if(maskMaterial)Destroy(maskMaterial);foreach(var r in overlays)if(r)Destroy(r.gameObject);foreach(var r in masks)if(r)Destroy(r.gameObject);}
    }
}
