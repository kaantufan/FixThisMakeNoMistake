using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Original local-authored bag preparation gloves, camera +Z, metres. No input/physics/state.</summary>
    public static class PanicBagHandsArt
    {
        const int OverlayLayer=28;
        enum Surface { Sleeve, SleeveSeam, Cuff, Leather, Palm, Back, Stitch }
        static Material[] materials;
        static readonly ConditionalWeakTable<GameObject,Rig> rigs=new ConditionalWeakTable<GameObject,Rig>();
        sealed class Rig
        {
            public Transform Bob;
            public float Jump, Land, Run; public bool WasGrounded=true;
            public readonly Transform[] Hand=new Transform[2], Sleeve=new Transform[2], Upper=new Transform[2];
            public readonly Transform[,] MCP=new Transform[2,5], PIP=new Transform[2,5], DIP=new Transform[2,5];
        }
        public static GameObject Create()
        {
            Init();var root=new GameObject("BagPreparationHands_ClosedGauntletsV54");root.layer=OverlayLayer;
            var rig=new Rig();rig.Bob=Node(root.transform,"HandsBob",Vector3.zero);
            BuildArm(rig,0);BuildArm(rig,1);rigs.Add(root,rig);Animate(root,0,0,0,0);return root;
        }

        /// <param name="gather">Normalized action phase: 0 rest, .42 contact, 1 rest. Root advances it once per gather.</param>
        /// <param name="open">Normalized lid/door action phase: 0 rest, .5 grip, 1 rest. Root owns clock/pause.</param>
        public static void SetLocomotion(GameObject model, float speed, bool grounded, float verticalSpeed, float dt)
        {
            var r=GetRig(model);if(r==null)return;
            dt=Mathf.Clamp(Finite(dt),0,.1f);r.Run=Unit((speed-2.3f)/3f);
            if(grounded&&!r.WasGrounded)r.Land=1; r.WasGrounded=grounded;
            r.Land=Mathf.MoveTowards(r.Land,0,dt*5);r.Jump=grounded?0:Mathf.Clamp(Finite(verticalSpeed),-5,5)/5;
        }

        public static void Animate(GameObject model,float clock,float move,float gather,float open)
        {
            if(!model)return;var r=GetRig(model);if(r==null)return;
            clock=Finite(clock);move=Unit(move);gather=Unit(gather);open=Unit(open);
            float gatherArc=Pulse(gather),openArc=Pulse(open);
            float reach=Mathf.Max(gatherArc,openArc),walk=move*(1-reach);
            float stride=clock*Mathf.Lerp(8,11,r.Run);
            r.Bob.localPosition=new Vector3(Mathf.Sin(stride)*.005f*walk,Mathf.Cos(stride*2)*.005f*walk-r.Jump*.016f-r.Land*.022f,0);
            for(int h=0;h<2;h++)
            {
                float side=h==0?-1:1;bool active=h==1;
                Vector3 p=new Vector3(side*.195f,-.235f,.42f);
                p+=new Vector3(side*Mathf.Sin(clock*8)*.003f,Mathf.Sin(clock*1.5f+h)*.0015f,Mathf.Sin(clock*8+h*Mathf.PI)*.006f*walk);
                // Single right hand reaches; left hand remains low and well separated.
                float extension=active?reach:reach*.10f;
                p+=new Vector3(-side*.062f,.087f,.13f)*extension;
                p+=new Vector3(side*.012f,-.013f,-.032f)*r.Run*walk;
                p+=new Vector3(0,-r.Jump*.010f-r.Land*.018f,Mathf.Sin(stride+h*Mathf.PI)*.016f*walk);
                if(active)p+=new Vector3(.012f,-.014f,-.025f)*Smooth(.58f,.95f,gather)*gatherArc;
                Vector3 angle=new Vector3(12-extension*15,-side*(12-extension*8),side*(8-extension*9));
                if(active)angle+=new Vector3(-openArc*9,openArc*4,-openArc*17);
                r.Hand[h].localPosition=p;r.Hand[h].localRotation=Quaternion.Euler(angle);
                Vector3 elbow=new Vector3(side*.29f,-.34f,.10f);
                PointSleeve(r.Sleeve[h],elbow,p,.40f);
                PointSleeve(r.Upper[h],new Vector3(side*.27f,-.24f,-.16f),elbow,.31f);
                float contact=active?Mathf.Max(Contact(gather),Contact(open)*.83f):.06f*reach;
                for(int d=0;d<4;d++)
                {
                    float relaxed=7+d*3,grip=contact*(1+d*.025f);
                    // Human-like MCP/PIP/DIP chain. Flexion +X bends fingers into the -Y palm.
                    r.MCP[h,d].localRotation=Quaternion.Euler(relaxed+grip*34,side*(1.4f-d)*2.6f,0);
                    r.PIP[h,d].localRotation=Quaternion.Euler(13+grip*48,0,0);
                    r.DIP[h,d].localRotation=Quaternion.Euler(8+grip*29,0,0);
                }
                // Mirrored medial thumb saddle and opposition; no negative mesh scale.
                r.MCP[h,4].localRotation=Quaternion.Euler(12+contact*14,-side*(39-contact*20),-side*(18+contact*8));
                r.PIP[h,4].localRotation=Quaternion.Euler(15+contact*25,0,0);
                r.DIP[h,4].localRotation=Quaternion.Euler(9+contact*20,0,0);
            }
        }
        static float Contact(float phase)
        {return phase<=0||phase>=1?0:Smooth(.20f,.43f,phase)*(1-Smooth(.72f,.96f,phase));}
        static float Pulse(float phase)
        {return phase<=0||phase>=1?0:Smooth(0,.38f,phase)*(1-Smooth(.64f,1,phase));}
        static float Smooth(float a,float b,float v){float t=Mathf.Clamp01((v-a)/(b-a));return t*t*(3-2*t);}
        static float Unit(float v){return Mathf.Clamp01(Finite(v));}
        static float Finite(float v){return float.IsNaN(v)||float.IsInfinity(v)?0:v;}
        static void PointSleeve(Transform sleeve,Vector3 start,Vector3 end,float meshLength)
        {Vector3 d=end-start;sleeve.localPosition=start;sleeve.localRotation=Quaternion.LookRotation(d.normalized,Vector3.up);sleeve.localScale=new Vector3(1,1,d.magnitude/meshLength);}
        static Rig GetRig(GameObject root)
        {
            if(rigs.TryGetValue(root,out var found))return found;
            // Supports a baked/instantiated viewmodel without storing another character rig.
            var r=new Rig();r.Bob=root.transform.Find("HandsBob");if(!r.Bob)return null;
            for(int h=0;h<2;h++)
            {
                string side=h==0?"L":"R";r.Hand[h]=r.Bob.Find("Hand_"+side);r.Sleeve[h]=r.Bob.Find("Forearm_"+side);r.Upper[h]=r.Bob.Find("UpperArm_"+side);
                if(!r.Hand[h]||!r.Sleeve[h]||!r.Upper[h])return null;
                for(int d=0;d<5;d++){r.MCP[h,d]=r.Hand[h].Find("Digit"+d+"_MCP");r.PIP[h,d]=r.MCP[h,d].Find("PIP");r.DIP[h,d]=r.PIP[h,d].Find("DIP");}
            }
            rigs.Add(root,r);return r;
        }
        static void BuildArm(Rig r,int h)
        {
            float side=h==0?-1:1;string label=h==0?"L":"R";
            r.Sleeve[h]=Node(r.Bob,"Forearm_"+label,Vector3.zero);
            var b=new MeshBatch();
            b.Loft(Surface.Sleeve,new[]{0f,.08f,.22f,.34f,.385f,.40f},new[]{.063f,.062f,.050f,.039f,.034f,.032f},new[]{.053f,.052f,.044f,.034f,.030f,.027f},Vector3.zero,18);
            // Seams follow the longitudinal fabric; no giant radial rings or hard sleeve end caps in view.
            b.Tube(Surface.SleeveSeam,new[]{new Vector3(side*.054f,.022f,.03f),new Vector3(side*.043f,.021f,.22f),new Vector3(side*.029f,.018f,.37f)},.0011f,6);
            b.Loft(Surface.Cuff,new[]{.367f,.375f,.392f,.402f},new[]{.034f,.035f,.034f,.031f},new[]{.030f,.031f,.029f,.027f},Vector3.zero,18);
            b.Flush(r.Sleeve[h],"TailoredSleeve");
            r.Upper[h]=Node(r.Bob,"UpperArm_"+label,Vector3.zero);b=new MeshBatch();
            b.Loft(Surface.Sleeve,new[]{0f,.15f,.31f},new[]{.078f,.074f,.062f},new[]{.067f,.061f,.053f},Vector3.zero,18);b.Flush(r.Upper[h],"RelaxedUpperSleeve");
            r.Hand[h]=Node(r.Bob,"Hand_"+label,Vector3.zero);b=new MeshBatch();
            // One closed glove shell, extending four centimetres over the sleeve.
            // The old dorsal ellipsoid intersected the shell and looked like an open
            // chopped wrist. The coloured back panel now shares the shell's faces.
            b.Loft(Surface.Leather,new[]{-.040f,-.016f,.006f,.031f,.058f,.079f,.090f,.095f},
                new[]{.035f,.034f,.030f,.035f,.039f,.038f,.034f,.027f},
                new[]{.030f,.029f,.024f,.024f,.025f,.023f,.020f,.016f},Vector3.zero,24,true);
            b.Ellipsoid(Surface.Leather,new Vector3(-side*.027f,-.009f,.035f),new Vector3(.019f,.018f,.026f));
            b.GlovePanelStitch(-1);b.GlovePanelStitch(1);
            b.Flush(r.Hand[h],"GlovePalmAndDorsum");
            float[] lengths={.071f,.079f,.074f,.059f};float[] widths={.010f,.0105f,.010f,.0085f};
            for(int d=0;d<5;d++)
            {
                bool thumb=d==4;float len=thumb?.063f:lengths[d];float radius=thumb?.012f:widths[d];
                // Index lies on the medial side, little finger on the lateral side.
                Vector3 seat=thumb?new Vector3(-side*.031f,-.005f,.028f):new Vector3(side*(-.028f+d*.019f),-.001f,.083f-(d==3?.008f:0));
                var m=Node(r.Hand[h],"Digit"+d+"_MCP",seat);r.MCP[h,d]=m;
                var pip=Node(m,"PIP",new Vector3(0,0,len*.45f));r.PIP[h,d]=pip;
                var dip=Node(pip,"DIP",new Vector3(0,0,len*.32f));r.DIP[h,d]=dip;
                Phalanx(m,len*.45f,radius,false);Phalanx(pip,len*.32f,radius*.91f,false);Phalanx(dip,len*.23f,radius*.80f,true);
            }
        }
        static void Phalanx(Transform parent,float length,float radius,bool tip)
        {
            var b=new MeshBatch();
            b.Loft(Surface.Leather,new[]{-.003f,0f,length*.65f,length-.002f,length+.001f},new[]{radius*.88f,radius,radius*.95f,radius*.78f,tip?radius*.18f:radius*.72f},new[]{radius*.81f,radius*.85f,radius*.8f,radius*.65f,tip?radius*.18f:radius*.62f},Vector3.zero,12);
            b.Ellipsoid(Surface.Palm,new Vector3(0,-radius*.75f,length*.5f),new Vector3(radius*.71f,.0015f,length*.31f));
            b.Tube(Surface.Cuff,new[]{new Vector3(-radius*.64f,radius*.73f,.001f),new Vector3(0,radius*.89f,.002f),new Vector3(radius*.64f,radius*.73f,.001f)},.0007f,5);
            b.Flush(parent,"GlovePhalanx");
        }
        static Transform Node(Transform parent,string name,Vector3 p)
        {var g=new GameObject(name);g.layer=OverlayLayer;g.transform.SetParent(parent,false);g.transform.localPosition=p;return g.transform;}
        static void Init()
        {
            if(materials!=null&&materials.Length>0&&materials[0])return;
            Color[] c={new Color(.18f,.25f,.32f),new Color(.11f,.16f,.22f),new Color(.16f,.19f,.20f),new Color(.36f,.28f,.19f),new Color(.48f,.40f,.28f),new Color(.27f,.34f,.35f),new Color(.67f,.61f,.46f)};
            materials=new Material[c.Length];var shader=Shader.Find("Standard");
            for(int i=0;i<c.Length;i++){materials[i]=new Material(shader){name="HomeV4_BagHands_"+(Surface)i,color=c[i]};materials[i].SetFloat("_Glossiness",i==(int)Surface.Leather?.25f:.13f);}
        }
        sealed class MeshBatch
        {
            readonly List<Vector3>[] v=new List<Vector3>[7];readonly List<int>[] t=new List<int>[7];
            public MeshBatch(){for(int i=0;i<7;i++){v[i]=new List<Vector3>();t[i]=new List<int>();}}
            void Triangle(Surface s,Vector3 a,Vector3 b,Vector3 c)
            {int i=(int)s,n=v[i].Count;v[i].Add(a);v[i].Add(b);v[i].Add(c);t[i].Add(n);t[i].Add(n+1);t[i].Add(n+2);}
            public void Loft(Surface s,float[] z,float[] rx,float[] ry,Vector3 p,int segments,bool glovePanel=false)
            {
                for(int r=0;r<z.Length-1;r++)for(int j=0;j<segments;j++)
                {float a=j*Mathf.PI*2/segments,b=(j+1)*Mathf.PI*2/segments;
                 Vector3 q=p+new Vector3(Mathf.Cos(a)*rx[r],Mathf.Sin(a)*ry[r],z[r]),w=p+new Vector3(Mathf.Cos(b)*rx[r],Mathf.Sin(b)*ry[r],z[r]);
                 Vector3 e=p+new Vector3(Mathf.Cos(a)*rx[r+1],Mathf.Sin(a)*ry[r+1],z[r+1]),f=p+new Vector3(Mathf.Cos(b)*rx[r+1],Mathf.Sin(b)*ry[r+1],z[r+1]);
                 Surface face=glovePanel&&r>=3&&r<5&&j>=2&&j<10?Surface.Back:s;
                 Triangle(face,q,w,e);Triangle(face,w,f,e);}
                for(int j=0;j<segments;j++)
                {float a=j*Mathf.PI*2/segments,b=(j+1)*Mathf.PI*2/segments;int last=z.Length-1;
                 Triangle(s,p+new Vector3(0,0,z[0]),p+new Vector3(Mathf.Cos(b)*rx[0],Mathf.Sin(b)*ry[0],z[0]),p+new Vector3(Mathf.Cos(a)*rx[0],Mathf.Sin(a)*ry[0],z[0]));
                 Triangle(s,p+new Vector3(0,0,z[last]),p+new Vector3(Mathf.Cos(a)*rx[last],Mathf.Sin(a)*ry[last],z[last]),p+new Vector3(Mathf.Cos(b)*rx[last],Mathf.Sin(b)*ry[last],z[last]));}
            }
            public void GlovePanelStitch(int side)
            {
                // Raised by 0.8mm over the SAME shell rings: no coplanar patch mesh.
                float a=side<0?Mathf.PI*5/6:Mathf.PI/6;
                float[] z={.031f,.058f,.079f},rx={.035f,.039f,.038f},ry={.024f,.025f,.023f};
                var path=new Vector3[3];
                for(int k=0;k<3;k++)path[k]=new Vector3(Mathf.Cos(a)*(rx[k]+.0008f),Mathf.Sin(a)*(ry[k]+.0008f),z[k]);
                Tube(Surface.Stitch,path,.0006f,5);
            }
            public void Ellipsoid(Surface s,Vector3 p,Vector3 radius)
            {
                const int rings=8,segs=14;
                Func<int,int,Vector3> point=(r,j)=>{float a=j*Mathf.PI*2/segs,b=r*Mathf.PI/rings;return p+Vector3.Scale(radius,new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a)));};
                for(int r=0;r<rings;r++)for(int j=0;j<segs;j++){Triangle(s,point(r,j),point(r,j+1),point(r+1,j));Triangle(s,point(r,j+1),point(r+1,j+1),point(r+1,j));}
            }
            public void Tube(Surface s,Vector3[] p,float radius,int segments)
            {
                for(int k=1;k<p.Length;k++)
                {var q=Quaternion.LookRotation((p[k]-p[k-1]).normalized);for(int j=0;j<segments;j++)
                 {float a=j*Mathf.PI*2/segments,b=(j+1)*Mathf.PI*2/segments;Vector3 u=q*new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0),w=q*new Vector3(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius,0);
                  Triangle(s,p[k-1]+u,p[k-1]+w,p[k]+u);Triangle(s,p[k-1]+w,p[k]+w,p[k]+u);}}
            }
            public void Flush(Transform parent,string name)
            {
                for(int i=0;i<7;i++)if(v[i].Count>0)
                {var m=new Mesh{name="HomeV4_"+name+"_"+(Surface)i};m.SetVertices(v[i]);m.SetTriangles(t[i],0);
                 var uv=new Vector2[v[i].Count];for(int n=0;n<uv.Length;n++)uv[n]=new Vector2(v[i][n].x*10,v[i][n].z*10);m.uv=uv;m.RecalculateNormals();
                 var normalSum=new Dictionary<Vector3,Vector3>();var normals=m.normals;
                 for(int n=0;n<v[i].Count;n++){normalSum.TryGetValue(v[i][n],out var sum);normalSum[v[i][n]]=sum+normals[n];}
                 for(int n=0;n<v[i].Count;n++)normals[n]=normalSum[v[i][n]].normalized;
                 m.normals=normals;m.RecalculateBounds();
                 var g=Node(parent,name+"_"+(Surface)i,Vector3.zero).gameObject;g.AddComponent<MeshFilter>().sharedMesh=m;
                 var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=materials[i];r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
            }
        }
    }
}
