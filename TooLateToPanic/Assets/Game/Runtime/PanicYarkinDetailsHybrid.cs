using System.Collections.Generic;
using UnityEngine;
namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        static readonly Vector3[] NeighbourStationsHybrid = {
            new Vector3(-8f,.085f,-13.5f), new Vector3(-2.5f,.085f,-11.5f),
            new Vector3(3f,.085f,-12f), new Vector3(8f,.085f,-13.5f)
        };
        static readonly float[] NeighbourStationYawsHybrid = {15f,-10f,20f,-15f};
        public int NeighbourDetailsHybridCount
        { get { int count=0; foreach(var n in neighbors) if(n.visual && n.visual.GetComponent<PanicNeighbourDetailsHybrid>()) count++; return count; } }
        public int NeighbourPoseFramesHybrid
        { get { int count=0; foreach(var n in neighbors) if(n.visual && n.visual.TryGetComponent<PanicNeighbourDetailsHybrid>(out var d)) count+=d.PoseApplyCount; return count; } }

        // Exact root intake hook: call once immediately after InitExteriorLife() in existing Awake.
        void InitNeighbourDetailsHybrid()
        {
            var lifetime=GetComponent<PanicNeighbourDetailsLifetimeHybrid>();
            if(!lifetime) lifetime=gameObject.AddComponent<PanicNeighbourDetailsLifetimeHybrid>();
            for(int i=0;i<neighbors.Count && i<4;i++)
            {
                var n=neighbors[i]; if(!n.visual) continue;
                var details=n.visual.GetComponent<PanicNeighbourDetailsHybrid>();
                if(!details) details=n.visual.AddComponent<PanicNeighbourDetailsHybrid>();
                details.Bind(this,n.animator,i);
                lifetime.Track(n.visual);
            }
        }
        // Exact root intake hook inside the EXISTING neighbour loop, after its null check.
        // World.Update remains the single root/route/Speed owner; LateUpdate is cosmetic bones only.
        bool TryTickNeighbourDetailsHybrid(Neighbor neighbor,float dt)
        {
            if(!neighbor.visual || !neighbor.visual.TryGetComponent<PanicNeighbourDetailsHybrid>(out var details)) return false;
            bool response=Model.Phase==PanicPhase.Aftermath || Model.Phase==PanicPhase.Success || Model.Phase==PanicPhase.Inspection;
            if(!response || LossAnnounced)
            {
                details.QueuePose(PanicNeighbourActionHybrid.Idle,0,0);
                return false;
            }
            int role=details.Role;
            neighbor.visual.SetActive(true);
            neighbor.visual.transform.SetPositionAndRotation(NeighbourStationsHybrid[role],Quaternion.Euler(0,NeighbourStationYawsHybrid[role],0));
            if(neighbor.animator) neighbor.animator.SetFloat("Speed",0,.15f,Mathf.Max(.001f,dt));
            float clock=residenceTime+neighbor.offset;
            int step=Mathf.FloorToInt(clock/4f+role*1.33f)%3;
            details.QueuePose(step==0?PanicNeighbourActionHybrid.Radio:step==1?PanicNeighbourActionHybrid.KneelInspect:PanicNeighbourActionHybrid.Directing,clock,dt);
            return true;
        }

        // Exact root intake hook: replaces ONLY the legacy Fresh damage fracture block in RebuildHardware.
        // Uses already committed item damage and primary hazard label. Ledger rows do not split per-item hazards.
        bool TryBuildYarkinDamageMarkHybrid(PanicItem item,GameObject holder)
        {
            if(item==null || !holder || !(item.Damage>4) || item.Kind==ItemKind.Wall || item.Kind==ItemKind.Roof) return false;
            float width=Mathf.Min(1.2f,item.Size.x*.7f),height=Mathf.Min(1.6f,item.Size.y*.8f);
            int variant=StableDamageVariantHybrid(item.Id);
            DisasterType type=Model.Disaster;
            var mark=PanicDamageDecalArt.CreateDamageMark(type,variant,new Vector3(width,height,item.Size.z),holder.transform);
            if(!mark) return false;
            bool floorMark=(type==DisasterType.Flood || type==DisasterType.Storm) && variant>=2;
            mark.transform.localPosition=floorMark?new Vector3(0,.012f,-item.Size.z*.30f):
                item.Kind==ItemKind.Window?new Vector3(0,1f,-.18f):new Vector3(0,.1f,-item.Size.z*.51f);
            foreach(var t in mark.GetComponentsInChildren<Transform>(true)) t.gameObject.layer=2;
            return true;
        }
        static int StableDamageVariantHybrid(string id)
        {
            uint hash=2166136261u;
            if(id!=null) foreach(char c in id) hash=unchecked((hash^c)*16777619u);
            return (int)(hash%4u);
        }
    }

    // Legacy neighbour roots live in the scene rather than under World. Own only the four
    // explicitly modified roots so destroying this World cannot orphan new uniforms/materials.
    [DisallowMultipleComponent]
    public sealed class PanicNeighbourDetailsLifetimeHybrid : MonoBehaviour
    {
        readonly List<GameObject> roots=new List<GameObject>();
        public int TrackedNpcCount { get { return roots.Count; } }
        public void Track(GameObject root) { if(root && !roots.Contains(root)) roots.Add(root); }
        void OnDestroy()
        {
            foreach(var root in roots) if(root)
            { if(Application.isPlaying) Destroy(root); else DestroyImmediate(root); }
            roots.Clear();
        }
    }
}
