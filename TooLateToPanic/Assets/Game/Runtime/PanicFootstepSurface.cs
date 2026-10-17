using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    // An 11cm contact patch bridges authored 6mm board/tile seams without changing geometry.
    public sealed class PanicFootstepSurface
    {
        struct Face { public Vector3 a,b,c; public string sound,mesh,material,path; }
        struct Hit { public bool found; public float height; public Face face; }
        [Serializable] public sealed class ProbeTrace
        { public Vector3 position;public bool hit,eligible;public float height;public string sound,mesh,material,path; }
        [Serializable] public sealed class SampleTrace
        {
            public Vector3 position;public string selectedSound,selectedMesh,selectedMaterial,selectedPath,rule;
            public float selectedHeight;public bool fallback;public int cachedFaces;
            public List<ProbeTrace> probes=new List<ProbeTrace>();
        }
        readonly Dictionary<Vector2Int,List<Face>> cells=new Dictionary<Vector2Int,List<Face>>();
        readonly Hit[] hits=new Hit[9];
        readonly int[] votes=new int[4];
        // Centre, four cardinals and four diagonal samples. No actor/room coordinate rules.
        static readonly Vector2[] offsets={Vector2.zero,new Vector2(-.055f,0),new Vector2(.055f,0),new Vector2(0,-.055f),new Vector2(0,.055f),
            new Vector2(-.039f,-.039f),new Vector2(-.039f,.039f),new Vector2(.039f,-.039f),new Vector2(.039f,.039f)};
        static readonly string[] sounds={"step_grass","step_wood","step_concrete","step_tile"};
        const float Cell=2,SupportBand=.008f;
        public int FaceCount { get; private set; }
        public void Add(GameObject root)
        {
            if(!root)return;
            foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh=f.sharedMesh;var r=f.GetComponent<Renderer>();
                if(!mesh||!mesh.isReadable||!r)continue;
                var vertices=mesh.vertices;var materials=r.sharedMaterials;
                string path=f.name;for(Transform t=f.transform.parent;t;t=t.parent)path=t.name+"/"+path;
                for(int s=0;s<mesh.subMeshCount;s++)
                {
                    string material=s<materials.Length&&materials[s]?materials[s].name:"";
                    string sound=MaterialSound(material.ToLowerInvariant());if(sound==null)continue;
                    var indices=mesh.GetTriangles(s);
                    for(int i=0;i+2<indices.Length;i+=3)
                    {
                        var a=f.transform.TransformPoint(vertices[indices[i]]);var b=f.transform.TransformPoint(vertices[indices[i+1]]);var c=f.transform.TransformPoint(vertices[indices[i+2]]);
                        var normal=Vector3.Cross(b-a,c-a);if(normal.sqrMagnitude<.0000001f||normal.normalized.y<.65f)continue;
                        if(Mathf.Min(a.y,Mathf.Min(b.y,c.y))>3.45f)continue;
                        var face=new Face{a=a,b=b,c=c,sound=sound,mesh=mesh.name,material=material,path=path};FaceCount++;
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
        Hit Point(Vector3 p)
        {
            var hit=new Hit{height=p.y-1.2f};
            if(!cells.TryGetValue(new Vector2Int(Mathf.FloorToInt(p.x/Cell),Mathf.FloorToInt(p.z/Cell)),out var faces))return hit;
            foreach(var f in faces)
            {
                float denominator=(f.b.z-f.c.z)*(f.a.x-f.c.x)+(f.c.x-f.b.x)*(f.a.z-f.c.z);
                if(Mathf.Abs(denominator)<.0000001f)continue;
                float u=((f.b.z-f.c.z)*(p.x-f.c.x)+(f.c.x-f.b.x)*(p.z-f.c.z))/denominator;
                float v=((f.c.z-f.a.z)*(p.x-f.c.x)+(f.a.x-f.c.x)*(p.z-f.c.z))/denominator;
                if(u<-.0001f||v<-.0001f||u+v>1.0001f)continue;
                float y=u*f.a.y+v*f.b.y+(1-u-v)*f.c.y;
                if(y>p.y+.22f||y<hit.height)continue;
                hit=new Hit{found=true,height=y,face=f};
            }
            return hit;
        }
        public string Sample(Vector3 p,string fallback){return Select(p,fallback,null);}
        public SampleTrace Trace(Vector3 p,string fallback)
        {var trace=new SampleTrace{position=p,cachedFaces=FaceCount};Select(p,fallback,trace);return trace;}
        string Select(Vector3 p,string fallback,SampleTrace trace)
        {
            float top=float.NegativeInfinity;Array.Clear(votes,0,votes.Length);
            for(int i=0;i<hits.Length;i++)
            {hits[i]=Point(p+new Vector3(offsets[i].x,0,offsets[i].y));if(hits[i].found)top=Mathf.Max(top,hits[i].height);}
            for(int i=0;i<hits.Length;i++)
            {
                var h=hits[i];bool eligible=h.found&&h.height>=top-SupportBand;
                if(eligible)for(int k=0;k<sounds.Length;k++)if(sounds[k]==h.face.sound){votes[k]++;break;}
                if(trace!=null)trace.probes.Add(new ProbeTrace{position=p+new Vector3(offsets[i].x,0,offsets[i].y),hit=h.found,eligible=eligible,
                    height=h.found?h.height:0,sound=h.face.sound,mesh=h.face.mesh,material=h.face.material,path=h.face.path});
            }
            // On an exact equal-area material boundary the harder contact wins deterministically.
            // In the 8mm support band, the 18mm-lower structural slab cannot outvote the finish.
            int winner=-1,count=0;for(int k=0;k<votes.Length;k++)if(votes[k]>0&&votes[k]>=count){winner=k;count=votes[k];}
            string result=winner>=0?sounds[winner]:fallback;
            if(trace!=null)
            {
                trace.selectedSound=result;trace.fallback=winner<0;trace.rule="9-point 11cm footprint; top support band 8mm; majority; harder-contact tie";
                for(int i=0;i<hits.Length;i++)if(hits[i].found&&hits[i].height>=top-SupportBand&&hits[i].face.sound==result)
                {trace.selectedMesh=hits[i].face.mesh;trace.selectedMaterial=hits[i].face.material;trace.selectedPath=hits[i].face.path;trace.selectedHeight=hits[i].height;break;}
            }
            return result;
        }
    }
}
