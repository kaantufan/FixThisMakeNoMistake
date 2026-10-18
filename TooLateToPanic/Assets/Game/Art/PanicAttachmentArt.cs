using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Original reusable attachment meshes. Metres; Y up; presentation front -Z.
    /// All factories return origin-local roots with no colliders or gameplay behaviours.
    /// Root may bake the MeshFilter meshes and shared materials into persistent assets.</summary>
    public static class PanicAttachmentArt
    {
        enum Mat { Cream, Steel, Teal, Amber, Rubber, Ink }
        static Material[] materials;
        static readonly Dictionary<string, Mesh[]> cache = new Dictionary<string, Mesh[]>();

        /// <summary>Base-centred flood barrier; X width, Y height. Inputs clamped to .35..4m/.22..2m.
        /// Main assembly is .10m deep; front latches extend to z=-.095m.</summary>
        public static GameObject CreateFloodGuard(float width, float height)
        {
            width = Safe(width, .35f, 4f, 1.3f);
            height = Safe(height, .22f, 2f, .7f);
            string key = "FloodGuard_" + width.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "_" + height.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            Mesh[] meshes;
            if (!cache.TryGetValue(key, out meshes))
            {
                var b = new Builder();
                // Continuous dark gasket, cream side channels and stacked bolted steel boards.
                b.Box(new Vector3(0, height * .5f, .018f), new Vector3(width, height, .052f), .009f, Mat.Rubber);
                b.Box(new Vector3(-width*.5f+.025f,height*.5f,0),new Vector3(.05f,height,.10f),.009f,Mat.Cream);
                b.Box(new Vector3(width*.5f-.025f,height*.5f,0),new Vector3(.05f,height,.10f),.009f,Mat.Cream);
                b.Box(new Vector3(0,.022f,0),new Vector3(width,.044f,.10f),.008f,Mat.Teal);
                b.Box(new Vector3(0,height-.018f,0),new Vector3(width,.036f,.10f),.007f,Mat.Steel);
                int boards = Mathf.Clamp(Mathf.CeilToInt(height/.25f), 2, 5);
                float pitch=(height-.065f)/boards;
                for(int i=0;i<boards;i++)
                {
                    float y=.045f+pitch*(i+.5f);
                    b.Box(new Vector3(0,y,-.022f),new Vector3(width-.095f,pitch-.012f,.044f),.009f, i%2==0?Mat.Steel:Mat.Cream);
                    b.Box(new Vector3(0,y-pitch*.5f+.007f,-.047f),new Vector3(width-.11f,.009f,.008f),.003f,Mat.Rubber);
                    for(int side=-1;side<=1;side+=2)
                    {
                        float x=side*(width*.5f-.092f);
                        b.Cylinder(new Vector3(x,y,-.053f),.014f,.012f,6,Quaternion.Euler(90,0,0),Mat.Steel);
                        b.Box(new Vector3(x,y,-.060f),new Vector3(.015f,.003f,.002f),.0005f,Mat.Ink);
                    }
                }
                for(int side=-1;side<=1;side+=2)
                {
                    float x=side*(width*.5f-.026f), y=height*.55f;
                    b.Box(new Vector3(x,y,-.060f),new Vector3(.036f,.10f,.023f),.005f,Mat.Teal);
                    b.Box(new Vector3(x,y+.016f,-.079f),new Vector3(.025f,.072f,.032f),.006f,Mat.Amber,Quaternion.Euler(0,0,side*12));
                    b.Cylinder(new Vector3(x,y-.028f,-.085f),.012f,.019f,6,Quaternion.Euler(90,0,0),Mat.Steel);
                }
                // Two folded lift grips and the amber identification tab read from the isometric view.
                for(int side=-1;side<=1;side+=2)
                {
                    float x=side*width*.22f;
                    b.Box(new Vector3(x,height-.040f,-.055f),new Vector3(.11f,.018f,.018f),.005f,Mat.Teal);
                }
                b.Box(new Vector3(0,.053f,-.055f),new Vector3(Mathf.Min(.22f,width*.28f),.032f,.014f),.004f,Mat.Amber);
                meshes=b.Finish(key);cache.Add(key,meshes);
            }
            return Attach(key,meshes);
        }

        /// <summary>Four low steel L brackets, hex bolts, fabric straps and amber tension tabs.
        /// localSize is the furniture's local bounding size, centred X/Z and grounded Y.
        /// No obstacle collider is added; hardware protrudes at most .06m outside footprint.</summary>
        public static GameObject CreateAnchors(Vector3 localSize)
        {
            float width=Safe(localSize.x,.24f,6,1),depth=Safe(localSize.z,.24f,6,1);
            float rise=Mathf.Min(.24f,Safe(localSize.y,.15f,5,1)*.34f);
            string key="Anchors_"+width.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"_"+depth.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"_"+rise.ToString("R",System.Globalization.CultureInfo.InvariantCulture);
            Mesh[] meshes;
            if(!cache.TryGetValue(key,out meshes))
            {
                var b=new Builder();
                for(int sx=-1;sx<=1;sx+=2) for(int sz=-1;sz<=1;sz+=2)
                {
                    Vector3 foot=new Vector3(sx*(width*.5f-.052f),.018f,sz*(depth*.5f-.013f));
                    b.Box(foot,new Vector3(.14f,.028f,.11f),.006f,Mat.Steel);
                    b.Box(new Vector3(foot.x,rise*.5f,sz*(depth*.5f-.020f)),new Vector3(.080f,rise,.024f),.005f,Mat.Steel);
                    b.Box(new Vector3(foot.x,rise*.57f,sz*(depth*.5f+.001f)),new Vector3(.044f,rise*.84f,.018f),.004f,Mat.Teal);
                    b.Box(new Vector3(foot.x,rise*.63f,sz*(depth*.5f+.014f)),new Vector3(.061f,.045f,.012f),.004f,Mat.Amber);
                    b.Cylinder(foot+new Vector3(-.035f,.023f,sz*.027f),.018f,.015f,6,Quaternion.identity,Mat.Steel);
                    b.Cylinder(foot+new Vector3(.035f,.023f,sz*.027f),.018f,.015f,6,Quaternion.identity,Mat.Steel);
                    b.Box(foot+new Vector3(-.035f,.031f,sz*.027f),new Vector3(.018f,.002f,.003f),.0005f,Mat.Ink);
                    b.Box(foot+new Vector3(.035f,.031f,sz*.027f),new Vector3(.018f,.002f,.003f),.0005f,Mat.Ink);
                }
                meshes=b.Finish(key);cache.Add(key,meshes);
            }
            return Attach(key,meshes);
        }

        /// <summary>Grounded .36m square relief pictogram, front -Z. bag/bunker/earthquake/flood/fire.</summary>
        public static GameObject CreateSafetySign(string type)
        {
            type=(type??"bag").ToLowerInvariant();
            string key="SafetySign_"+type;
            Mesh[] meshes;
            if(!cache.TryGetValue(key,out meshes))
            {
                var b=new Builder();
                b.Box(new Vector3(0,.18f,0),new Vector3(.36f,.36f,.035f),.022f,Mat.Teal);
                b.Box(new Vector3(0,.18f,-.021f),new Vector3(.321f,.321f,.013f),.012f,Mat.Cream);
                if(type=="bag")
                {
                    b.Box(new Vector3(0,.16f,-.034f),new Vector3(.16f,.19f,.018f),.025f,Mat.Teal);
                    b.Box(new Vector3(0,.273f,-.034f),new Vector3(.08f,.025f,.018f),.008f,Mat.Teal);
                    b.Box(new Vector3(0,.16f,-.047f),new Vector3(.028f,.096f,.010f),.003f,Mat.Cream);
                    b.Box(new Vector3(0,.16f,-.047f),new Vector3(.096f,.028f,.010f),.003f,Mat.Cream);
                }
                else if(type=="bunker")
                {
                    b.Polygon(new[]{new Vector2(-.12f,.16f),new Vector2(-.075f,.27f),new Vector2(.075f,.27f),new Vector2(.12f,.16f),new Vector2(.12f,.08f),new Vector2(-.12f,.08f)},-.033f,.014f,Mat.Teal);
                    b.Box(new Vector3(0,.13f,-.047f),new Vector3(.066f,.10f,.012f),.005f,Mat.Cream);
                    b.Box(new Vector3(0,.07f,-.047f),new Vector3(.24f,.023f,.012f),.004f,Mat.Amber);
                }
                else if(type=="earthquake")
                {
                    b.Polygon(new[]{new Vector2(-.12f,.10f),new Vector2(-.12f,.21f),new Vector2(0,.30f),new Vector2(.12f,.21f),new Vector2(.12f,.10f)},-.034f,.014f,Mat.Teal);
                    b.Polygon(new[]{new Vector2(.018f,.28f),new Vector2(-.030f,.19f),new Vector2(.014f,.19f),new Vector2(-.015f,.09f),new Vector2(.061f,.21f),new Vector2(.014f,.21f)},-.049f,.008f,Mat.Amber);
                    b.Box(new Vector3(0,.067f,-.036f),new Vector3(.25f,.016f,.015f),.003f,Mat.Ink);
                }
                else if(type=="flood")
                {
                    b.Polygon(new[]{new Vector2(-.09f,.20f),new Vector2(0,.31f),new Vector2(.09f,.20f),new Vector2(.065f,.12f),new Vector2(-.065f,.12f)},-.033f,.014f,Mat.Teal);
                    for(int row=0;row<2;row++) for(int col=0;col<4;col++)
                        b.Box(new Vector3(-.09f+col*.060f,.075f+row*.037f+(col%2)*.008f,-.050f),new Vector3(.064f,.013f,.012f),.004f,Mat.Teal,Quaternion.Euler(0,0,col%2==0?12:-12));
                }
                else if(type=="fire")
                {
                    b.Polygon(new[]{new Vector2(0,.31f),new Vector2(.025f,.23f),new Vector2(.070f,.26f),new Vector2(.11f,.15f),new Vector2(.065f,.07f),new Vector2(-.06f,.07f),new Vector2(-.11f,.14f),new Vector2(-.06f,.25f),new Vector2(-.035f,.18f)},-.033f,.014f,Mat.Amber);
                    b.Polygon(new[]{new Vector2(0,.22f),new Vector2(.044f,.12f),new Vector2(.02f,.08f),new Vector2(-.025f,.08f),new Vector2(-.042f,.13f)},-.049f,.010f,Mat.Cream);
                }
                else b.Box(new Vector3(0,.18f,-.034f),new Vector3(.055f,.19f,.018f),.008f,Mat.Amber);
                meshes=b.Finish(key);cache.Add(key,meshes);
            }
            return Attach(key,meshes);
        }

        static float Safe(float value,float minimum,float maximum,float fallback)
        { return float.IsNaN(value)||float.IsInfinity(value)?fallback:Mathf.Clamp(value,minimum,maximum); }

        static GameObject Attach(string name,Mesh[] meshes)
        {
            EnsureMaterials();var root=new GameObject(name);
            for(int i=0;i<meshes.Length;i++) if(meshes[i]!=null)
            {
                var child=new GameObject(((Mat)i).ToString());child.transform.SetParent(root.transform,false);
                child.AddComponent<MeshFilter>().sharedMesh=meshes[i];
                var renderer=child.AddComponent<MeshRenderer>();renderer.sharedMaterial=materials[i];
                renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;
            }
            return root;
        }

        static void EnsureMaterials()
        {
            if(materials!=null)return;
            Color[] colors={new Color(.91f,.86f,.73f),new Color(.46f,.55f,.55f),new Color(.055f,.28f,.28f),new Color(.96f,.61f,.10f),new Color(.034f,.041f,.040f),new Color(.055f,.075f,.08f)};
            materials=new Material[colors.Length];
            Shader shader=Shader.Find("Standard");
            for(int i=0;i<colors.Length;i++)
            {
                var m=new Material(shader);m.name="TLTP_Attachment_"+((Mat)i);m.color=colors[i];
                m.SetFloat("_Metallic",i==(int)Mat.Steel?.72f:.02f);m.SetFloat("_Glossiness",i==(int)Mat.Steel?.43f:.20f);
                materials[i]=m;
            }
        }

        sealed class Builder
        {
            readonly List<Vector3>[] vertices=new List<Vector3>[6];
            readonly List<int>[] indices=new List<int>[6];
            public Builder(){for(int i=0;i<6;i++){vertices[i]=new List<Vector3>();indices[i]=new List<int>();}}
            void Face(Vector3[] points,Mat mat,Vector3 center,Quaternion rotation)
            {
                int m=(int)mat,start=vertices[m].Count;
                Vector3 normal=Vector3.Cross(points[1]-points[0],points[2]-points[0]);
                Vector3 middle=Vector3.zero;for(int i=0;i<points.Length;i++)middle+=points[i];
                if(Vector3.Dot(normal,middle)<0)Array.Reverse(points);
                for(int i=0;i<points.Length;i++)vertices[m].Add(center+rotation*points[i]);
                for(int i=1;i<points.Length-1;i++){indices[m].Add(start);indices[m].Add(start+i);indices[m].Add(start+i+1);}
            }
            public void Box(Vector3 center,Vector3 size,float bevel,Mat mat)
            {Box(center,size,bevel,mat,Quaternion.identity);}
            public void Box(Vector3 center,Vector3 size,float bevel,Mat mat,Quaternion rotation)
            {
                Vector3 h=size*.5f;float d=Mathf.Min(bevel,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.8f);
                Vector3 v=h-Vector3.one*d;
                for(int axis=0;axis<3;axis++)for(int side=-1;side<=1;side+=2)
                {
                    int j=(axis+1)%3,k=(axis+2)%3;var p=new Vector3[4];
                    for(int t=0;t<4;t++){p[t][axis]=side*h[axis];p[t][j]=(t==0||t==3?-1:1)*v[j];p[t][k]=(t<2?-1:1)*v[k];}
                    Face(p,mat,center,rotation);
                }
                for(int axis=0;axis<3;axis++)for(int a=-1;a<=1;a+=2)for(int c=-1;c<=1;c+=2)
                {
                    int j=(axis+1)%3,k=(axis+2)%3;var p=new Vector3[4];
                    for(int t=0;t<4;t++){p[t][axis]=(t==0||t==3?-1:1)*v[axis];p[t][j]=a*(t<2?h[j]:v[j]);p[t][k]=c*(t<2?v[k]:h[k]);}
                    Face(p,mat,center,rotation);
                }
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                    Face(new[]{new Vector3(x*h.x,y*v.y,z*v.z),new Vector3(x*v.x,y*h.y,z*v.z),new Vector3(x*v.x,y*v.y,z*h.z)},mat,center,rotation);
            }
            public void Cylinder(Vector3 center,float radius,float height,int sides,Quaternion rotation,Mat mat)
            {
                for(int i=0;i<sides;i++)
                {
                    float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                    Vector3 p=new Vector3(Mathf.Cos(a)*radius,-height*.5f,Mathf.Sin(a)*radius),q=new Vector3(Mathf.Cos(b)*radius,-height*.5f,Mathf.Sin(b)*radius);
                    Vector3 up=Vector3.up*height;
                    Face(new[]{p,q,q+up,p+up},mat,center,rotation);
                    Face(new[]{Vector3.down*(height*.5f),q,p},mat,center,rotation);
                    Face(new[]{Vector3.up*(height*.5f),p+up,q+up},mat,center,rotation);
                }
            }
            // Relief outline: concave polygon cap uses ear clipping; no text or runtime texture.
            public void Polygon(Vector2[] outline,float front,float depth,Mat mat)
            {
                int m=(int)mat,start=vertices[m].Count,n=outline.Length;
                float area=0;for(int i=0;i<n;i++){Vector2 a=outline[i],b=outline[(i+1)%n];area+=a.x*b.y-b.x*a.y;}
                var loop=new List<int>();for(int i=0;i<n;i++)loop.Add(area>0?i:n-1-i);
                for(int i=0;i<n;i++)vertices[m].Add(new Vector3(outline[i].x,outline[i].y,front));
                for(int i=0;i<n;i++)vertices[m].Add(new Vector3(outline[i].x,outline[i].y,front+depth));
                int guard=n*n;
                while(loop.Count>2&&guard-->0)
                {
                    bool clipped=false;
                    for(int i=0;i<loop.Count;i++)
                    {
                        int ia=loop[(i+loop.Count-1)%loop.Count],ib=loop[i],ic=loop[(i+1)%loop.Count];
                        Vector2 a=outline[ia],b=outline[ib],c=outline[ic];if(Cross(b-a,c-b)<=.0000001f)continue;
                        bool contains=false;for(int j=0;j<loop.Count;j++){int p=loop[j];if(p==ia||p==ib||p==ic)continue;Vector2 v=outline[p];if(Cross(b-a,v-a)>=0&&Cross(c-b,v-b)>=0&&Cross(a-c,v-c)>=0){contains=true;break;}}
                        if(contains)continue;
                        indices[m].Add(start+ic);indices[m].Add(start+ib);indices[m].Add(start+ia);
                        indices[m].Add(start+n+ia);indices[m].Add(start+n+ib);indices[m].Add(start+n+ic);
                        loop.RemoveAt(i);clipped=true;break;
                    }
                    if(!clipped)break;
                }
                for(int i=0;i<n;i++)
                {
                    int a=area>0?i:n-1-i,b=area>0?(i+1)%n:n-1-(i+1)%n;
                    indices[m].Add(start+a);indices[m].Add(start+b);indices[m].Add(start+n+b);
                    indices[m].Add(start+a);indices[m].Add(start+n+b);indices[m].Add(start+n+a);
                }
            }
            static float Cross(Vector2 a,Vector2 b){return a.x*b.y-a.y*b.x;}
            public Mesh[] Finish(string name)
            {
                var result=new Mesh[6];
                for(int i=0;i<6;i++)if(vertices[i].Count>0)
                {
                    var mesh=new Mesh();mesh.name=name+"_"+((Mat)i);mesh.SetVertices(vertices[i]);mesh.SetTriangles(indices[i],0);
                    mesh.RecalculateNormals();mesh.RecalculateBounds();result[i]=mesh;
                }
                return result;
            }
        }
    }
}
