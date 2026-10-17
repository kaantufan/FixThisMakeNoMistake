using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        sealed class Neighbor
        {
            public GameObject visual;
            public Animator animator;
            public Vector3 start,end;
            public float offset;
        }
        readonly List<Neighbor> neighbors=new List<Neighbor>();
        readonly Dictionary<DisasterType,GameObject> neighborhoodDamage=new Dictionary<DisasterType,GameObject>();
        int exteriorOutcomeCount;
        public int ExteriorNpcCount { get { return neighbors.Count; } }
        public Vector3 ExteriorWalkerPosition { get { return neighbors.Count>0&&neighbors[0].visual?neighbors[0].visual.transform.position:Vector3.zero; } }
        public bool ExteriorNpcAnimationReady
        {
            get
            {
                if(neighbors.Count!=4)return false;
                foreach(var neighbor in neighbors)
                {
                    if(!neighbor.visual||!neighbor.animator||!neighbor.animator.runtimeAnimatorController||neighbor.animator.applyRootMotion)return false;
                    var rig=neighbor.visual.GetComponent<PanicCuratedCharacterRig>();
                    var skin=neighbor.visual.GetComponentInChildren<SkinnedMeshRenderer>(true);
                    if(rig)
                    {
                        if(rig.Bones==null||rig.Bones.Length!=19||rig.BindPositions==null||rig.BindPositions.Length!=19||
                            !skin||!skin.sharedMesh||skin.sharedMesh.vertexCount==0||skin.sharedMesh.bindposes.Length!=19)return false;
                        var rendererBones=skin.bones;
                        if(rendererBones==null||rendererBones.Length!=19)return false;
                        for(int i=0;i<19;i++)if(!rig.Bones[i]||rendererBones[i]!=rig.Bones[i])return false;
                    }
                    else
                    {
                        if(!neighbor.animator.isHuman||!neighbor.animator.avatar||!neighbor.animator.avatar.isValid||!skin||!skin.sharedMesh||skin.sharedMesh.vertexCount==0)return false;
                        bool idle=false,walk=false,jog=false,jump=false;
                        foreach(var clip in neighbor.animator.runtimeAnimatorController.animationClips)
                        {
                            if(!clip)continue;string name=clip.name;
                            idle|=name.IndexOf("Idle_Loop",System.StringComparison.OrdinalIgnoreCase)>=0;
                            walk|=name.IndexOf("Walk_Loop",System.StringComparison.OrdinalIgnoreCase)>=0;
                            jog|=name.IndexOf("Jog_Fwd_Loop",System.StringComparison.OrdinalIgnoreCase)>=0;
                            jump|=name.IndexOf("Jump_Loop",System.StringComparison.OrdinalIgnoreCase)>=0;
                        }
                        if(!idle||!walk||!jog||!jump)return false;
                    }
                }
                return true;
            }
        }
        public int ActiveExteriorNpcCount { get { int count=0;foreach(var neighbor in neighbors)if(neighbor.visual&&neighbor.visual.activeSelf)count++;return count; } }
        public int ExteriorDamageTypes { get { return neighborhoodDamage.Count; } }
        public bool HelicopterVisible { get { return helicopter&&helicopter.activeInHierarchy; } }
        public bool FloorHasVisibleRenderers(int floor)
        {
            if(floor < -1||floor>1||floor+1>=floorVisuals.Count)return false;
            foreach(var renderer in floorVisuals[floor+1].GetComponentsInChildren<Renderer>(true))if(renderer.enabled&&renderer.gameObject.activeInHierarchy)return true;
            return false;
        }
        public bool FloorPhysicalCollidersIntact(int floor)
        {
            if(floor < -1||floor>1||floor+1>=floorVisuals.Count)return false;
            var colliders=floorVisuals[floor+1].GetComponentsInChildren<Collider>(true);
            if(colliders.Length==0)return false;
            foreach(var collider in colliders)if(!collider.enabled||!collider.gameObject.activeInHierarchy)return false;
            return true;
        }

        void InitExteriorLife()
        {
            Vector3[] a={new Vector3(-12,.085f,-14.9f),new Vector3(3,.085f,-14.9f),new Vector3(-9.5f,.085f,-23.25f),new Vector3(-8.2f,.085f,-23.25f)};
            Vector3[] b={new Vector3(-4,.085f,-14.9f),new Vector3(8,.085f,-14.9f),a[2],a[3]};
            for(int i=0;i<a.Length;i++)
            {
                var visual=PanicExteriorArt.CreatePedestrianVisual(i);
                if(!visual)continue;
                visual.name="Neighborhood resident "+(i+1);visual.transform.position=a[i];
                var animator=visual.GetComponentInChildren<Animator>();
                if(animator){animator.applyRootMotion=false;animator.SetBool("Airborne",false);animator.SetBool("Working",false);animator.SetBool("Kneeling",false);}
                if(i>=2)visual.transform.rotation=Quaternion.Euler(0,i==2?90:270,0);
                neighbors.Add(new Neighbor{visual=visual,animator=animator,start=a[i],end=b[i],offset=i*2.7f});
            }
        }

        void AddNeighborhoodAftermath(DisasterType type)
        {
            if(neighborhoodDamage.ContainsKey(type))return;
            var damage=PanicExteriorArt.CreateAftermath(type);
            if(damage)neighborhoodDamage.Add(type,damage);
        }

        void ResetExteriorLife()
        {
            foreach(var damage in neighborhoodDamage.Values)if(damage)Destroy(damage);
            neighborhoodDamage.Clear();exteriorOutcomeCount=0;
        }

        void TickExteriorLife(float dt)
        {
            bool hazard=Model.Phase==PanicPhase.Disaster||Model.Phase==PanicPhase.Aftermath||CinematicActive||LossAnnounced;
            bool running=Model.Phase!=PanicPhase.GameOver&&!hazard;
            foreach(var neighbor in neighbors)
            {
                if(!neighbor.visual)continue;
                neighbor.visual.SetActive(running);
                if(!running)continue;
                Vector3 delta=neighbor.end-neighbor.start;float length=delta.magnitude;
                float speed=length>.1f?.58f:0;
                if(speed>0)
                {
                    float walk=residenceTime*speed+neighbor.offset;
                    float position=Mathf.PingPong(walk,length);
                    float sign=Mathf.Repeat(walk,length*2)<length?1:-1;
                    neighbor.visual.transform.position=neighbor.start+delta.normalized*position;
                    Quaternion target=Quaternion.LookRotation(delta.normalized*sign,Vector3.up);
                    neighbor.visual.transform.rotation=Quaternion.Slerp(neighbor.visual.transform.rotation,target,1-Mathf.Exp(-dt*5));
                }
                if(neighbor.animator)neighbor.animator.SetFloat("Speed",speed,.15f,Mathf.Max(.001f,dt));
                PanicCharacterArt.SetMotion(neighbor.visual,neighbor.animator,speed,true,0,dt);
            }
            if(JuryActive&&JuryDemo!=null)
            {
                while(exteriorOutcomeCount<JuryDemo.Outcomes.Count)AddNeighborhoodAftermath(JuryDemo.Outcomes[exteriorOutcomeCount++].Disaster);
            }
            else if(Model.Phase==PanicPhase.Aftermath||Model.Phase==PanicPhase.Inspection&&Model.TotalDamage>0)
            { for(int i=0;i<4;i++) if(Model.HasActiveDisasterV54((DisasterType)i)) AddNeighborhoodAftermath((DisasterType)i); }
            bool flood=Model.HasActiveDisasterV54(DisasterType.Flood)&&(hazard||Model.Phase==PanicPhase.Inspection)||neighborhoodDamage.ContainsKey(DisasterType.Flood);
            PanicExteriorArt.AnimateLandscape(landscape,residenceTime,flood);
        }
    }
}
