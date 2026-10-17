using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>V4.14 local rig of the actual defter-mockup Meshy mesh. No controller/input/game state.</summary>
    public sealed class PanicCuratedCharacterRig : MonoBehaviour
    {
        public Transform[] Bones;
        public Vector3[] BindPositions;
        public float Clock;
        public bool WasGrounded=true;
        public float Land;
        public static GameObject Create()
        {
            var source=Resources.Load<GameObject>("CuratedCharacter/DefterSurvivor");
            if(!source)throw new InvalidOperationException("V4.14 actual defter character is missing; old-character fallback refused.");
            var root=new GameObject("CuratedDefterSurvivor_V414");
            var rig=root.AddComponent<PanicCuratedCharacterRig>();
            var imported=Instantiate(source); imported.name="SourceDefterInspection";
            var filters=imported.GetComponentsInChildren<MeshFilter>(true);
            var verts=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            foreach(var f in filters)
            {
                var m=f.sharedMesh;if(!m)continue;
                int offset=verts.Count;var vs=m.vertices;var ns=m.normals;var us=m.uv;
                var matrix=root.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;
                for(int i=0;i<vs.Length;i++){verts.Add(matrix.MultiplyPoint3x4(vs[i]));normals.Add(ns.Length==vs.Length?matrix.inverse.transpose.MultiplyVector(ns[i]).normalized:Vector3.up);uv.Add(us.Length==vs.Length?us[i]:Vector2.zero);}
                for(int s=0;s<m.subMeshCount;s++)foreach(int t in m.GetTriangles(s))tris.Add(offset+t);
            }
            if(Application.isPlaying)Destroy(imported);else DestroyImmediate(imported);
            if(verts.Count==0)throw new InvalidOperationException("Defter mesh has no readable vertices. Enable source FBX Read/Write before bake.");
            var bounds=new Bounds(verts[0],Vector3.zero);foreach(var v in verts)bounds.Encapsulate(v);
            // FBX -90X import already puts source Z upright. Retain its facing and original texture UVs.
            float factor=1.80f/Mathf.Max(.001f,bounds.size.y);Vector3 origin=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            for(int i=0;i<verts.Count;i++)verts[i]=(verts[i]-origin)*factor;
            rig.BuildSkeleton();
            var skin=new Mesh{name="DefterSurvivor_V414_LocallyWeighted"};skin.indexFormat=verts.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16;
            skin.SetVertices(verts);skin.SetNormals(normals);skin.SetUVs(0,uv);skin.SetTriangles(tris,0);skin.RecalculateTangents();
            var bindposes=new Matrix4x4[rig.Bones.Length];for(int i=0;i<bindposes.Length;i++)bindposes[i]=rig.Bones[i].worldToLocalMatrix*root.transform.localToWorldMatrix;
            skin.bindposes=bindposes;
            var weights=new BoneWeight[verts.Count];for(int i=0;i<weights.Length;i++)weights[i]=rig.Weight(verts[i]);skin.boneWeights=weights;skin.RecalculateBounds();
            var renderer=root.AddComponent<SkinnedMeshRenderer>();renderer.sharedMesh=skin;renderer.bones=rig.Bones;renderer.rootBone=rig.Bones[0];renderer.updateWhenOffscreen=true;
            renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;renderer.localBounds=new Bounds(new Vector3(0,.94f,0),new Vector3(2.1f,2.15f,1.5f));
            var mat=new Material(Shader.Find("Standard")){name="DefterSurvivor_OriginalBaseColor",color=Color.white};
            mat.mainTexture=Resources.Load<Texture2D>("CuratedCharacter/DefterSurvivor_BaseColor");mat.SetFloat("_Glossiness",.18f);
            var normal=Resources.Load<Texture2D>("CuratedCharacter/DefterSurvivor_Normal");if(normal){mat.SetTexture("_BumpMap",normal);mat.EnableKeyword("_NORMALMAP");mat.SetFloat("_BumpScale",.55f);}
            renderer.sharedMaterial=mat;
            var animator=root.AddComponent<Animator>();animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            rig.Sample(0,true,0,0);return root;
        }
        void BuildSkeleton()
        {
            string[] names={"pelvis","spine_01","spine_02","neck_01","Head","upperarm_l","lowerarm_l","hand_l","upperarm_r","lowerarm_r","hand_r","thigh_l","calf_l","foot_l","ball_l","thigh_r","calf_r","foot_r","ball_r"};
            int[] parents={-1,0,1,2,3,2,5,6,2,8,9,0,11,12,13,0,15,16,17};
            Vector3[] p={V(0,.90f,0),V(0,1.08f,0),V(0,1.30f,0),V(0,1.52f,0),V(0,1.65f,-.015f),V(-.25f,1.43f,0),V(-.46f,1.23f,0),V(-.635f,1.035f,0),V(.25f,1.43f,0),V(.46f,1.23f,0),V(.635f,1.035f,0),V(-.135f,.89f,0),V(-.145f,.48f,0),V(-.15f,.105f,0),V(-.15f,.055f,.14f),V(.135f,.89f,0),V(.145f,.48f,0),V(.15f,.105f,0),V(.15f,.055f,.14f)};
            Bones=new Transform[names.Length];BindPositions=new Vector3[names.Length];
            for(int i=0;i<Bones.Length;i++){var t=new GameObject(names[i]).transform;t.SetParent(parents[i]<0?transform:Bones[parents[i]],false);t.position=transform.TransformPoint(p[i]);Bones[i]=t;BindPositions[i]=t.localPosition;}
        }
        static Vector3 V(float x,float y,float z){return new Vector3(x,y,z);}
        BoneWeight Weight(Vector3 p)
        {
            // Region gates prevent forearms being attracted to nearby torso and crotch joining both legs.
            float side=p.x<0?-1:1,ax=Mathf.Abs(p.x);
            int[] chain;
            if(ax>.245f&&p.y>.78f&&p.y<1.51f)chain=side<0?new[]{5,6,7}:new[]{8,9,10};
            else if(p.y<.91f)chain=side<0?new[]{0,11,12,13,14}:new[]{0,15,16,17,18};
            else chain=new[]{0,1,2,3,4};
            int a=chain[0],b=chain[0];float best=float.MaxValue,second=float.MaxValue;
            foreach(int i in chain)
            {
                float d=(p-transform.InverseTransformPoint(Bones[i].position)).sqrMagnitude;
                if(i==0&&p.y<.87f&&ax>.055f)d+=.05f;
                if(d<best){second=best;b=a;best=d;a=i;}else if(d<second){second=d;b=i;}
            }
            float wa=1f/(best+.008f),wb=1f/(second+.008f);float n=wa+wb;
            return new BoneWeight{boneIndex0=a,boneIndex1=b,weight0=wa/n,weight1=wb/n};
        }
        public void Sample(float metresPerSecond,bool grounded,float verticalSpeed,float dt)
        {
            if(Bones==null||Bones.Length<19)return;
            dt=Mathf.Clamp(Finite(dt),0,.1f);float speed=Mathf.Clamp(Finite(metresPerSecond),0,12f);
            if(grounded&&!WasGrounded)Land=1;WasGrounded=grounded;
            Land=Mathf.MoveTowards(Land,0,dt*5);
            float moving=Mathf.Clamp01(speed/.45f),run=Mathf.Clamp01((speed-2.3f)/3f);
            // A complete left/right gait advances by travelled metres. No idle-time oscillator drives steps.
            float stance=Mathf.Lerp(.60f,.54f,run),halfStride=Mathf.Lerp(.35f,.47f,run);
            float cycleMetres=2f*halfStride/stance;
            if(grounded&&speed>.015f)Clock=Mathf.Repeat(Clock+dt*speed/cycleMetres*Mathf.PI*2,Mathf.PI*2);
            for(int i=0;i<Bones.Length;i++){Bones[i].localPosition=BindPositions[i];Bones[i].localRotation=Quaternion.identity;}
            float gait=Mathf.Sin(Clock),other=-gait;
            float hipDrop=grounded?(Mathf.Lerp(.10f,.17f,run)*moving+Land*.085f):0;
            Bones[0].localPosition+=new Vector3(gait*.009f*moving,-hipDrop+Mathf.Cos(Clock*2)*.009f*moving,0);
            Bones[2].localRotation=Quaternion.Euler((run*8+Land*13)*moving,gait*3.5f*moving,-gait*2f*moving);
            Bones[4].localRotation=Quaternion.Euler(-run*3,0,gait*.7f*moving);
            // Actual A-pose source shoulders lower first; elbows and wrists then counter-swing visibly.
            float swing=Mathf.Lerp(27,43,run)*moving;
            Bones[5].localRotation=Quaternion.Euler(gait*swing-Land*18,0,41-run*3);
            Bones[8].localRotation=Quaternion.Euler(other*swing-Land*18,0,-41+run*3);
            Bones[6].localRotation=Quaternion.Euler(-12-run*42-Mathf.Max(0,-gait)*8*moving,0,0);
            Bones[9].localRotation=Quaternion.Euler(-12-run*42-Mathf.Max(0,gait)*8*moving,0,0);
            Bones[7].localRotation=Quaternion.Euler(0,0,-3*run);Bones[10].localRotation=Quaternion.Euler(0,0,3*run);
            if(grounded)
            {
                // Each foot has an explicit planted backward stroke and a lifted recovery stroke.
                // Two-bone IK keeps the sole near its ground plane while pelvis/knee heights change.
                SampleStep(11,12,13,Clock/(Mathf.PI*2),stance,halfStride,moving,run);
                SampleStep(15,16,17,Clock/(Mathf.PI*2)+.5f,stance,halfStride,moving,run);
            }
            else
            {
                float rise=Mathf.Clamp(Finite(verticalSpeed)/5f,-1,1);
                Bones[11].localRotation=Quaternion.Euler(-31-rise*10,0,0);Bones[15].localRotation=Quaternion.Euler(15-rise*12,0,0);
                Bones[12].localRotation=Quaternion.Euler(60+rise*8,0,0);Bones[16].localRotation=Quaternion.Euler(38-rise*6,0,0);
                Bones[13].localRotation=Quaternion.Euler(-18,0,0);Bones[17].localRotation=Quaternion.Euler(-12,0,0);
                Bones[5].localRotation=Quaternion.Euler(-28-rise*10,0,27);Bones[8].localRotation=Quaternion.Euler(-28-rise*10,0,-27);
                Bones[6].localRotation=Quaternion.Euler(-28,0,0);Bones[9].localRotation=Quaternion.Euler(-28,0,0);
                Bones[2].localRotation=Quaternion.Euler(7-rise*4,0,0);
            }
        }
        void SampleStep(int upperIndex,int lowerIndex,int footIndex,float phase,float stance,float halfStride,float moving,float run)
        {
            phase=Mathf.Repeat(phase,1f);float forward,lift,pitch;
            if(phase<stance)
            {
                forward=Mathf.Lerp(halfStride,-halfStride,phase/stance);lift=0;pitch=0;
            }
            else
            {
                float swing=(phase-stance)/(1-stance),arc=Mathf.Sin(swing*Mathf.PI);
                forward=Mathf.Lerp(-halfStride,halfStride,swing*swing*(3-2*swing));
                lift=arc*Mathf.Lerp(.15f,.23f,run);pitch=-arc*22f;
            }
            float side=footIndex==13?-1:1;
            Vector3 goal=transform.TransformPoint(new Vector3(side*.15f,.12f+lift*moving,forward*moving));
            Transform upper=Bones[upperIndex],lower=Bones[lowerIndex],foot=Bones[footIndex];
            Vector3 hip=upper.position,direction=goal-hip;
            float a=Vector3.Distance(hip,lower.position),b=Vector3.Distance(lower.position,foot.position);
            float distance=Mathf.Clamp(direction.magnitude,.08f,a+b-.001f);
            direction=direction.sqrMagnitude>.000001f?direction.normalized:-transform.up;
            Vector3 pole=Vector3.ProjectOnPlane(transform.forward,direction).normalized;
            if(pole.sqrMagnitude<.000001f)pole=transform.right;
            float along=(a*a-b*b+distance*distance)/(2*distance);
            Vector3 knee=hip+direction*along+pole*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            upper.rotation=Quaternion.FromToRotation(lower.position-hip,knee-hip)*upper.rotation;
            lower.rotation=Quaternion.FromToRotation(foot.position-lower.position,hip+direction*distance-lower.position)*lower.rotation;
            foot.rotation=transform.rotation*Quaternion.Euler(pitch*moving,0,0);
        }
        public void SampleCinematic(string action,float phase)
        {
            phase=Mathf.Clamp01(Finite(phase));Sample(0,true,0,0);
            float arc=Mathf.Sin(phase*Mathf.PI);
            if((action??"").IndexOf("throw",StringComparison.OrdinalIgnoreCase)>=0)
            {
                Bones[8].localRotation=Quaternion.Euler(-105*arc,0,-30);Bones[9].localRotation=Quaternion.Euler(-30*(1-phase)*arc,0,0);
                Bones[5].localRotation=Quaternion.Euler(-35*arc,0,34);Bones[2].localRotation=Quaternion.Euler(9*arc,-13*arc,0);
            }
            else if((action??"").IndexOf("jump",StringComparison.OrdinalIgnoreCase)>=0)
            {
                Bones[11].localRotation=Quaternion.Euler(-34*arc,0,0);Bones[15].localRotation=Quaternion.Euler(-22*arc,0,0);
                Bones[12].localRotation=Quaternion.Euler(55*arc,0,0);Bones[16].localRotation=Quaternion.Euler(45*arc,0,0);
                Bones[5].localRotation=Quaternion.Euler(-40*arc,0,22);Bones[8].localRotation=Quaternion.Euler(-40*arc,0,-22);
            }
        }
        static float Finite(float v){return float.IsNaN(v)||float.IsInfinity(v)?0:v;}
    }
}
