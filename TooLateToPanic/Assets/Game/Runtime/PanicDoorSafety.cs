using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        public bool IsDoorCloseBlocked(string id)
        {
            return id!=null&&hinges.TryGetValue(id,out var hinge)&&hinge.closeBlocked;
        }

        Quaternion SafeDoorPose(Hinge hinge,Quaternion next)
        {
            Quaternion current=hinge.leaf.localRotation;
            if(!controller||!controller.enabled||!hinge.collision) return next;
            float angle=Quaternion.Angle(current,next);
            // Quaternion.Angle rounds very small per-frame rotations to zero.
            // Exact component equality must gate the no-motion shortcut; otherwise
            // a slowly settling door can stop a degree before physically closing.
            if(current.Equals(next))return current;
            int steps=Mathf.Max(1,Mathf.CeilToInt(angle/1.25f));
            Quaternion safe=current;
            Quaternion parentRotation=hinge.leaf.parent?hinge.leaf.parent.rotation:Quaternion.identity;
            float clearance=DoorPlayerClearance(hinge,parentRotation*current);
            for(int i=1;i<=steps;i++)
            {
                Quaternion candidate=Quaternion.Slerp(current,next,i/(float)steps);
                float candidateClearance=DoorPlayerClearance(hinge,parentRotation*candidate);
                // When already inside the safety margin, permit only movement away.
                // E can therefore reverse a blocked swing without trapping the actor.
                if(candidateClearance<.035f && candidateClearance<=clearance+.00001f)
                {
                    hinge.motionBlocked=true;
                    hinge.closeBlocked=!hinge.open;
                    return safe;
                }
                safe=candidate;
                clearance=candidateClearance;
            }
            return next;
        }

        float DoorPlayerClearance(Hinge hinge,Quaternion worldRotation)
        {
            // Room leaves rotate only about Y. In the future leaf frame the upright
            // controller is a vertical capsule, so segment-to-box distance is exact.
            // Work from explicit controller.center instead of native controller query geometry.
            var box=hinge.collision as BoxCollider;
            if(!box)return float.NegativeInfinity;
            Vector3 scale=hinge.leaf.lossyScale;
            Vector3 half=Vector3.Scale(box.size,new Vector3(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z)))*.5f;
            Vector3 boxCenter=hinge.leaf.position+worldRotation*Vector3.Scale(box.center,scale);
            Vector3 capsuleCenter=player.TransformPoint(controller.center);
            Vector3 local=Quaternion.Inverse(worldRotation)*(capsuleCenter-boxCenter);
            Vector3 actorScale=player.lossyScale;
            float radius=controller.radius*Mathf.Max(Mathf.Abs(actorScale.x),Mathf.Abs(actorScale.z));
            float segmentHalf=Mathf.Max(0,controller.height*Mathf.Abs(actorScale.y)*.5f-radius);
            float x=Mathf.Max(0,Mathf.Abs(local.x)-half.x);
            float z=Mathf.Max(0,Mathf.Abs(local.z)-half.z);
            float y=Mathf.Max(0,Mathf.Abs(local.y)-segmentHalf-half.y);
            return Mathf.Sqrt(x*x+y*y+z*z)-radius;
        }
    }
}
