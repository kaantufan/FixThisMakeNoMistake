using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    // Samples the authored visible floor, including paving drawn over broad garden colliders.
    // Cached horizontal triangles: no new colliders, no input or gameplay mutation.
    public sealed class PanicFootstepSurface
    {
        struct Face { public Vector3 a,b,c; public string sound; }
        readonly Dictionary<Vector2Int,List<Face>> cells=new Dictionary<Vector2Int,List<Face>>();
        const float Cell=2;
        public int FaceCount { get; private set; }
        public void Add(GameObject root)
        {
            if(!root)return;
            foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh=f.sharedMesh;var r=f.GetComponent<Renderer>();
                if(!mesh||!mesh.isReadable||!r)continue;
                var vertices=mesh.vertices;var materials=r.sharedMaterials;
                for(int s=0;s<mesh.subMeshCount;s++)
                {
                    string material=s<materials.Length&&materials[s]?materials[s].name.ToLowerInvariant():"";
                    string sound=MaterialSound(material);if(sound==null)continue;
                    var indices=mesh.GetTriangles(s);
                    for(int i=0;i+2<indices.Length;i+=3)
                    {
                        var a=f.transform.TransformPoint(vertices[indices[i]]);var b=f.transform.TransformPoint(vertices[indices[i+1]]);var c=f.transform.TransformPoint(vertices[indices[i+2]]);
                        var normal=Vector3.Cross(b-a,c-a);if(normal.sqrMagnitude<.0000001f||normal.normalized.y<.65f)continue;
                        // Only walkable elevations; roofs and high decorative meshes are irrelevant.
                        if(Mathf.Min(a.y,Mathf.Min(b.y,c.y))>3.45f)continue;
                        var face=new Face{a=a,b=b,c=c,sound=sound};FaceCount++;
                        int x0=Mathf.FloorToInt(Mathf.Min(a.x,Mathf.Min(b.x,c.x))/Cell),x1=Mathf.FloorToInt(Mathf.Max(a.x,Mathf.Max(b.x,c.x))/Cell);
                        int z0=Mathf.FloorToInt(Mathf.Min(a.z,Mathf.Min(b.z,c.z))/Cell),z1=Mathf.FloorToInt(Mathf.Max(a.z,Mathf.Max(b.z,c.z))/Cell);
                        for(int x=x0;x<=x1;x++)for(int z=z0;z<=z1;z++)
                        {var key=new Vector2Int(x,z);if(!cells.TryGetValue(key,out var list))cells[key]=list=new List<Face>();list.Add(face);}
                    }
                }
            }
        }
        static string MaterialSound(string name)
        {
            if(name.Contains("lawn")||name.Contains("olive")||name.Contains("earth"))return "step_grass";
            if(name.Contains("walnut"))return "step_wood";
            if(name.Contains("stone")||name.Contains("taupe"))return "step_tile";
            if(name.Contains("concrete")||name.Contains("road")||name.Contains("slate")||name.Contains("brick"))return "step_concrete";
            return null;
        }
        public string Sample(Vector3 p,string fallback)
        {
            if(!cells.TryGetValue(new Vector2Int(Mathf.FloorToInt(p.x/Cell),Mathf.FloorToInt(p.z/Cell)),out var faces))return fallback;
            float best=p.y-1.2f;string result=fallback;
            foreach(var f in faces)
            {
                float denominator=(f.b.z-f.c.z)*(f.a.x-f.c.x)+(f.c.x-f.b.x)*(f.a.z-f.c.z);
                if(Mathf.Abs(denominator)<.0000001f)continue;
                float u=((f.b.z-f.c.z)*(p.x-f.c.x)+(f.c.x-f.b.x)*(p.z-f.c.z))/denominator;
                float v=((f.c.z-f.a.z)*(p.x-f.c.x)+(f.a.x-f.c.x)*(p.z-f.c.z))/denominator;
                if(u<-.0001f||v<-.0001f||u+v>1.0001f)continue;
                float y=u*f.a.y+v*f.b.y+(1-u-v)*f.c.y;
                if(y>p.y+.22f||y<best)continue;
                best=y;result=f.sound;
            }
            return result;
        }
    }
}
