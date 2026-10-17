using System;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Actual yarkin lid only. Existing shaft, north hinge, collider and interaction remain authoritative.</summary>
    public sealed class PanicCuratedBunkerLid : MonoBehaviour
    {
        public Transform Wheel;
        public Quaternion WheelRest=Quaternion.identity;
        public static bool Install(GameObject existingHatch)
        {
            if(!existingHatch)return false;
            if(existingHatch.GetComponentInChildren<PanicCuratedBunkerLid>(true))return true;
            Transform leaf=null;foreach(var t in existingHatch.GetComponentsInChildren<Transform>(true))if(t.name=="DoorLeaf"){leaf=t;break;}
            var source=Resources.Load<GameObject>("CuratedBunker414/SM_BunkerHatch");
            if(!leaf||!source){Debug.LogWarning("V414_BUNKER_LID_MISSING sourceOrDoorLeaf");return false;}
            var art=Instantiate(source);art.name="ActualYarkinBunkerLid_V414";art.transform.SetParent(leaf,false);
            bool any=false;Bounds bounds=new Bounds();Transform wheel=null;
            foreach(var r in art.GetComponentsInChildren<Renderer>(true))
            {
                bool moving=false;
                for(var t=r.transform;t&&t!=art.transform.parent;t=t.parent)
                    if(t.name=="HatchLid_Hinge"||t.name=="LockWheel_Pivot")moving=true;
                r.enabled=moving;
                if(!moving)continue;
                var filter=r.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)continue;
                var b=filter.sharedMesh.bounds;
                for(int i=0;i<8;i++)
                {
                    Vector3 p=leaf.InverseTransformPoint(filter.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));
                    if(!any){bounds=new Bounds(p,Vector3.zero);any=true;}else bounds.Encapsulate(p);
                }
                r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            }
            foreach(var t in art.GetComponentsInChildren<Transform>(true))if(t.name=="LockWheel_Pivot")wheel=t;
            if(!any){if(Application.isPlaying)Destroy(art);else DestroyImmediate(art);return false;}
            // DoorLeaf local origin is retained at north z+.57. Closed circular panel is centred south of it.
            float fit=1.08f/Mathf.Max(bounds.size.x,bounds.size.z);
            art.transform.localScale*=fit;
            art.transform.localPosition=new Vector3(0,.035f,-.57f)-bounds.center*fit;
            foreach(var r in leaf.GetComponentsInChildren<Renderer>(true))
                if(!r.transform.IsChildOf(art.transform))r.enabled=false;
            CreateOpenTransitionRing(existingHatch.transform);
            var adapter=existingHatch.AddComponent<PanicCuratedBunkerLid>();adapter.Wheel=wheel;if(wheel)adapter.WheelRest=wheel.localRotation;
            return true;
        }
        static void CreateOpenTransitionRing(Transform parent)
        {
            // Square legacy collar to round source lid. Centre stays an actual 1.02m opening.
            const int count=64;var v=new Vector3[count*2];var triangles=new int[count*6];
            for(int i=0;i<count;i++)
            {
                float a=i*Mathf.PI*2/count,x=Mathf.Cos(a),z=Mathf.Sin(a);
                float radius=Mathf.Min(.665f/Mathf.Max(.0001f,Mathf.Abs(x)),.575f/Mathf.Max(.0001f,Mathf.Abs(z)));
                v[i*2]=new Vector3(x*radius,.193f,z*radius);v[i*2+1]=new Vector3(x*.51f,.193f,z*.51f);
                int n=((i+1)%count)*2,k=i*6;
                triangles[k]=i*2;triangles[k+1]=n+1;triangles[k+2]=n;
                triangles[k+3]=i*2;triangles[k+4]=i*2+1;triangles[k+5]=n+1;
            }
            var mesh=new Mesh{name="V414_Bunker_OpenSquareToCircleRing"};mesh.vertices=v;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
            var g=new GameObject("V414_OpenHatchTransitionRing");g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;
            var material=Resources.Load<Material>("CuratedBunker414/M_DarkMetal");
            if(!material)material=new Material(Shader.Find("Standard")){name="V414_BunkerTransitionSteel",color=new Color(.14f,.17f,.18f)};
            g.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        public void Sample(bool open,float dt)
        {
            if(!Wheel)return;
            Quaternion target=WheelRest*Quaternion.Euler(0,open?-330:0,0);
            Wheel.localRotation=Quaternion.Slerp(Wheel.localRotation,target,1-Mathf.Exp(-Mathf.Max(0,dt)*8));
        }
    }
}
