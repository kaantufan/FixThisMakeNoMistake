using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    // Cosmetic adapter for the existing two-second bag camera owner. Never moves the actor/controller.
    public sealed partial class PanicHomeWorld
    {
        Matrix4x4 bagDollyProjectionFromV56;
        Vector3 bagDollyControl1V56,bagDollyControl2V56,bagDollyForwardV56;
        readonly Dictionary<Collider,Renderer[]> bagDollyObstacleRenderersV56=new Dictionary<Collider,Renderer[]>();
        float bagDollyStartClockV56,bagDollyClockDriftV56,bagDollyProgressV56,bagDollyProjectionBlendV56,bagDollyFpsStartClockV56=-1;
        bool bagDollyReducedV56,bagDollyShowBodyV56,bagDollyPathClearV56;
        int bagDollyAdjustmentCountV56,bagDollyAppliedFramesV56;
        public bool BagDollyActiveV56 { get { return BagHandoffActiveV51; } }
        public float BagDollyProgressV56 { get { return bagDollyProgressV56; } }
        public bool BagDollyBodyVisibleV56 { get { return hero&&hero.gameObject.activeInHierarchy; } }
        public bool BagDollyHandsVisibleV56 { get { return bagHandsCamera&&bagHandsCamera.enabled; } }
        public bool BagDollyPathClearV56 { get { return bagDollyPathClearV56; } }
        public int BagDollyCollisionAdjustmentCountV56 { get { return bagDollyAdjustmentCountV56; } }
        public float BagDollyPreparationClockDriftV56 { get { return bagDollyClockDriftV56; } }
        public float BagDollyProjectionBlendV56 { get { return bagDollyProjectionBlendV56; } }
        public int BagDollyAppliedFrameCountV56 { get { return bagDollyAppliedFramesV56; } }
        public float BagDollyActorDisplacementV56 { get { return Vector3.Distance(PlayerPosition,bagTransitionActor); } }
        public bool BagTransitionShowsBodyV56 { get { return BagDollyActiveV56&&bagDollyShowBodyV56; } }
        public bool BagDollyUsesIsometricCutawayV56 { get { return BagDollyActiveV56; } }
        public bool BagDollyReducedMotionV56 { get { return bagDollyReducedV56; } }
        public float BagDollyFpsSettleV56 { get { return bagDollyFpsStartClockV56<0?1:DollyEaseV56(Mathf.Clamp01((fpsStressClockV414-bagDollyFpsStartClockV56)/.32f)); } }

        void BeginBagDollyV56()
        {
            bagDollyProgressV56=bagDollyProjectionBlendV56=bagDollyClockDriftV56=0;
            bagDollyStartClockV56=Model.TimeRemaining;bagDollyAppliedFramesV56=bagDollyAdjustmentCountV56=0;
            bagDollyReducedV56=ReducedFpsMotionV54;bagDollyShowBodyV56=true;
            bagDollyFpsStartClockV56=-1;
            bagDollyProjectionFromV56=View.projectionMatrix;
            bagDollyForwardV56=Quaternion.Euler(0,bagYaw,0)*Vector3.forward;
            bagDollyObstacleRenderersV56.Clear();
            // Open the same existing isometric cutaway before testing the cinematic's visible surfaces.
            // Permanent roof/ceiling barriers stay enabled for the real controller throughout.
            ApplyFloorVisibility();
            Vector3 towardsStart=Vector3.ProjectOnPlane(bagShotFrom-bagShotTo,Vector3.up).normalized;
            if(towardsStart.sqrMagnitude<.01f)towardsStart=-bagDollyForwardV56;
            Vector3 right=Vector3.Cross(Vector3.up,bagDollyForwardV56);
            bagDollyPathClearV56=false;
            for(int route=0;route<4;route++)
            {
                Vector3 shoulder=bagDollyReducedV56?Vector3.Lerp(bagShotFrom,bagShotTo,.82f):
                    route==0?bagShotTo-bagDollyForwardV56*1.35f+right*.24f+Vector3.up*.13f:
                    route==1?bagShotTo+towardsStart*1.15f+Vector3.up*.18f:
                    route==2?bagShotTo+towardsStart*.7f+Vector3.up*1.25f:Vector3.Lerp(bagShotFrom,bagShotTo,.82f);
                bagDollyControl1V56=bagDollyReducedV56||route==3?Vector3.Lerp(bagShotFrom,bagShotTo,.34f):
                    Vector3.Lerp(bagShotFrom,shoulder,.46f)+right*.28f+Vector3.up*.30f;
                bagDollyControl2V56=shoulder;
                if(BagDollyRouteClearV56()){bagDollyPathClearV56=true;break;}
                bagDollyAdjustmentCountV56++;
            }
            float depth=Mathf.Max(1f,Vector3.Dot(PlayerPosition+Vector3.up*1.02f-bagShotFrom,View.transform.forward));
            bagShotFov=Mathf.Clamp(2*Mathf.Atan(bagIsoSize/depth)*Mathf.Rad2Deg,15,95);
            if(hero)hero.gameObject.SetActive(true);
        }

        static float DollyEaseV56(float t){t=Mathf.Clamp01(t);return t*t*t*(t*(t*6-15)+10);}
        Vector3 BagDollyPointV56(float t)
        {
            float u=1-t;
            return u*u*u*bagShotFrom+3*u*u*t*bagDollyControl1V56+3*u*t*t*bagDollyControl2V56+t*t*t*bagShotTo;
        }
        bool BagDollyRouteClearV56()
        {
            Vector3 previous=bagShotFrom;
            for(int sample=1;sample<=28;sample++)
            {
                Vector3 point=BagDollyPointV56(sample/28f);
                if(TryBagDollyVisibleHitV56(previous,point,out _))return false;
                previous=point;
            }
            return true;
        }

        bool TryBagDollyVisibleHitV56(Vector3 from,Vector3 to,out RaycastHit obstruction)
        {
            obstruction=new RaycastHit();Vector3 delta=to-from;float distance=delta.magnitude;
            if(distance<.0001f)return false;
            var hits=Physics.SphereCastAll(from,.09f,delta/distance,distance,~((1<<28)|(1<<30)),QueryTriggerInteraction.Ignore);
            Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)
            {
                var c=hit.collider;
                if(!c||c==controller||player&&c.transform.IsChildOf(player)||!c.enabled||!c.gameObject.activeInHierarchy)continue;
                if(!bagDollyObstacleRenderersV56.TryGetValue(c,out var renderers))
                {
                    if(c.name.StartsWith("Collision_",StringComparison.Ordinal)&&c.transform.parent)
                    {
                        var matches=new List<Renderer>();string name=c.name.Substring("Collision_".Length);
                        foreach(var r in c.transform.parent.GetComponentsInChildren<Renderer>(true))if(r.name==name)matches.Add(r);
                        renderers=matches.ToArray();
                    }
                    else renderers=c.GetComponentsInChildren<Renderer>(true);
                    bagDollyObstacleRenderersV56[c]=renderers;
                }
                foreach(var r in renderers)
                {
                    if(!r||!r.enabled||r.forceRenderingOff||!r.gameObject.activeInHierarchy||r.GetComponent<PanicPermanentlyHiddenV414>())continue;
                    var bounds=r.bounds;bounds.Expand(.18f);
                    // Full-height physical partitions may have a deliberately low cutaway mesh.
                    // Only their currently rendered part can obstruct a presentation camera.
                    if(!bounds.Contains(hit.point))continue;
                    obstruction=hit;return true;
                }
            }
            return false;
        }

        void ApplyBagDollyPoseV56(float progress)
        {
            float t=Mathf.Clamp01(progress),eased=DollyEaseV56(t);
            bagDollyProgressV56=t;bagDollyClockDriftV56=Mathf.Abs(Model.TimeRemaining-bagDollyStartClockV56);
            bagDollyAppliedFramesV56++;
            Vector3 position=BagDollyPointV56(eased);
            if(TryBagDollyVisibleHitV56(View.transform.position,position,out var hit))
            {
                // Dynamic visible doors may move after planning; retain a small camera clearance.
                position=View.transform.position+(position-View.transform.position).normalized*Mathf.Max(0,hit.distance-.02f);
                bagDollyAdjustmentCountV56++;bagDollyPathClearV56=false;
            }
            Vector3 target=Vector3.Lerp(bagShotTo,bagShotTo+bagDollyForwardV56*4,DollyEaseV56(Mathf.InverseLerp(.46f,.92f,t)));
            Vector3 look=target-position;
            Quaternion aim=look.sqrMagnitude>.00001f?Quaternion.LookRotation(look,Vector3.up):bagRotationTo;
            Quaternion rotation=Quaternion.Slerp(bagRotationFrom,aim,DollyEaseV56(Mathf.InverseLerp(0,.28f,t)));
            rotation=Quaternion.Slerp(rotation,bagRotationTo,DollyEaseV56(Mathf.InverseLerp(.60f,1,t)));
            if(!bagDollyReducedV56)rotation*=Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI)*.85f);
            View.orthographic=false;View.nearClipPlane=.05f;
            View.fieldOfView=Mathf.Lerp(bagShotFov,62,eased);
            View.transform.SetPositionAndRotation(position,rotation);
            // At the first frame the exact former orthographic matrix is retained. The lens
            // then continuously becomes the matched perspective lens instead of snapping.
            bagDollyProjectionBlendV56=DollyEaseV56(Mathf.InverseLerp(0,.32f,t));
            Matrix4x4 perspective=Matrix4x4.Perspective(View.fieldOfView,View.aspect,View.nearClipPlane,View.farClipPlane),projection=new Matrix4x4();
            for(int i=0;i<16;i++)projection[i]=Mathf.Lerp(bagDollyProjectionFromV56[i],perspective[i],bagDollyProjectionBlendV56);
            View.projectionMatrix=projection;
            bagDollyShowBodyV56=t<.89f&&Vector3.Distance(position,bagShotTo)>1.05f;
            if(hero)hero.gameObject.SetActive(bagDollyShowBodyV56);
            SetBagDollyPartitionsV56(DollyEaseV56(Mathf.InverseLerp(.95f,.15f,Vector3.Distance(position,bagShotTo))));
            if(t>=1){bagDollyFpsStartClockV56=fpsStressClockV414;View.ResetProjectionMatrix();View.transform.SetPositionAndRotation(bagShotTo,bagRotationTo);SetFirstPersonPartitions(true);bagDollyShowBodyV56=false;if(hero)hero.gameObject.SetActive(false);}
        }

        void SetBagDollyPartitionsV56(float amount)
        {
            SetFirstPersonPartitions(false);
            foreach(var entry in partitionScales)
            {
                var node=entry.Key;if(!node||!node.TryGetComponent<MeshFilter>(out var filter)||!filter.sharedMesh)continue;
                var bounds=filter.sharedMesh.bounds;float height=bounds.size.y;if(height<=.1f)continue;
                Vector3 bottom=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                float before=node.TransformPoint(bottom).y;Vector3 scale=entry.Value;
                node.localScale=new Vector3(scale.x,Mathf.Lerp(scale.y,scale.y*3.2f/height,amount),scale.z);
                node.position+=Vector3.up*(before-node.TransformPoint(bottom).y);
            }
        }

        void ResetBagDollyV56()
        {if(View)View.ResetProjectionMatrix();bagDollyShowBodyV56=false;bagDollyFpsStartClockV56=-1;bagDollyObstacleRenderersV56.Clear();}
    }
}
