using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using B = TooLateToPanic.PanicHomeArt.Builder;
using M = TooLateToPanic.PanicHomeArt.M;

namespace TooLateToPanic
{
    /// <summary>Authored scenery and V4.14 creek safety barriers; no scores, forces, route AI or disaster evaluation.</summary>
    public static class PanicExteriorArt
    {
        public const float CreekLeft=-21.1f, CreekRight=-16.35f, CreekLength=76f;
        const float RoadNear=-23.5f,RoadFar=-14.5f;
        public readonly struct NeighbourSite
        {
            public readonly Vector3 Position; public readonly float Width,Depth,Height,Yaw; public readonly bool Pale;
            public NeighbourSite(float x,float z,float w,float d,float h,float yaw,bool pale)
            {Position=new Vector3(x,0,z);Width=w;Depth=d;Height=h;Yaw=yaw;Pale=pale;}
        }
        static readonly NeighbourSite[] Sites={
            new NeighbourSite(-25,-3,7,6,4.9f,0,false),new NeighbourSite(26,1,8,7,5.7f,0,true),
            new NeighbourSite(-11,26,8,6,5.9f,0,true),new NeighbourSite(12,28,9,7,5.2f,0,false),
            new NeighbourSite(-26,-32,8,6,5.6f,180,true),new NeighbourSite(1,-33,8,7,5.9f,180,false),
            new NeighbourSite(27,-33,9,6,5.1f,180,true)};
        public static IReadOnlyList<NeighbourSite> NeighbourSites {get;}=Array.AsReadOnly(Sites);
        static float Centre(float z){return (CreekLeft+CreekRight)*.5f+Mathf.Sin(z*.13f)*.18f;}
        static float SurfaceEdge(float z,float level,bool right)
        {
            float bank=Mathf.Clamp01((level+.62f)/.589f);
            return Mathf.Lerp(Centre(z)+(right?1f:-1f),right?CreekRight:CreekLeft,bank);
        }

        public static GameObject CreateAftermath(DisasterType type)
        {
            PanicHomeArt.EnsurePalette();var root=new GameObject("Exterior aftermath - "+type);var b=new B();
            bool flood=type==DisasterType.Flood,fire=type==DisasterType.Fire,storm=type.ToString()=="Storm";
            for(int n=0;n<Sites.Length;n++)
            {
                var s=Sites[n];Quaternion q=Quaternion.Euler(0,s.Yaw,0);
                Func<Vector3,Vector3> at=v=>s.Position+q*v;
                // These are real scenery facade planes, never the hidden floors of the player's house.
                for(int floor=0;floor<2;floor++)
                {
                    float x=(floor==0?-1:1)*s.Width*.28f,y=1.45f+floor*s.Height*.48f,z=-s.Depth*.5f-.115f;
                    if(!flood)
                    {
                        b.Quad(at(new Vector3(x-.33f,y-.30f,z)),at(new Vector3(x-.10f,y-.10f,z)),at(new Vector3(x+.31f,y+.29f,z)),at(new Vector3(x+.035f,y+.08f,z)),M.Black);
                        b.Beam(at(new Vector3(x-.36f,y+.33f,z-.008f)),at(new Vector3(x+.24f,y-.26f,z-.008f)),.016f,M.Stone,.002f);
                    }
                }
                if(flood)
                {
                    float z=-s.Depth*.5f-.105f;
                    b.Box(at(new Vector3(0,.27f,z)),new Vector3(s.Width-.22f,.40f,.014f),M.Earth,.004f,q);
                    for(int i=0;i<7;i++)b.Box(at(new Vector3(-s.Width*.40f+i*s.Width*.13f,.50f+(i%3)*.027f,z-.012f)),new Vector3(s.Width*.12f,.035f,.013f),M.Stone,.003f,q);
                }
                else if(fire)
                {
                    float z=-s.Depth*.5f-.11f,x=-s.Width*.28f;
                    b.Quad(at(new Vector3(x-.68f,.5f,z)),at(new Vector3(x+.72f,.5f,z)),at(new Vector3(x+.45f,s.Height*.78f,z)),at(new Vector3(x-.40f,s.Height*.88f,z)),M.Charcoal);
                    // The branch/window frame stays fixed to its facade; no levitating roof scars.
                    b.Box(at(new Vector3(x,s.Height*.60f,z-.01f)),new Vector3(.065f,s.Height*.55f,.02f),M.Black,.003f,q);
                }
                else
                {
                    // A displaced roof panel follows the actual pitched roof plane (front half).
                    float half=(s.Depth+.65f)*.5f,rise=s.Depth*.26f,roofZ=-s.Depth*.23f;
                    float roofY=s.Height+rise*(1f+roofZ/half)+.09f;
                    b.Box(at(new Vector3(s.Width*.24f,roofY,roofZ)),new Vector3(.75f,.025f,.48f),M.Charcoal,.004f,q*Quaternion.Euler(-Mathf.Atan2(rise,half)*Mathf.Rad2Deg,0,storm?7f:2f));
                }
                for(int i=0;i<4;i++)
                {
                    float x=-s.Width*.35f+i*s.Width*.23f,z=-s.Depth*.5f-.7f-(i%2)*.29f;
                    b.Box(at(new Vector3(x,.045f,z)),new Vector3(.16f+i*.025f,.09f,.20f),flood?M.Earth:fire?M.Charcoal:M.Brick,.016f,q*Quaternion.Euler(0,17+i*31,8));
                    if(storm)b.Beam(at(new Vector3(x-.26f,.04f,z)),at(new Vector3(x+.29f,.07f,z+.16f)),.045f,M.WalnutDark,.006f);
                }
            }
            b.Attach(root,"NeighbourDamage_"+type);return root;
        }

        public static GameObject CreatePedestrianVisual(int variant)
        {
            var source = Resources.Load<GameObject>("Player/SurvivorV2");
            if (!source)
            {
                Debug.LogWarning("EXTERIOR_NPC_MISSING retained CC0 SurvivorV2 prefab");
                return null;
            }

            int role = ((variant % 4) + 4) % 4;
            string[] roleNames = { "Police", "Paramedic", "Firefighter", "Rescue" };

            var root = UnityEngine.Object.Instantiate(source);
            root.name = "Neighbour responder " + roleNames[role] + " " + variant;

            // Initial physics sanitization pass (T2_R3_4)
            PanicCharacterArt.StripPhysics(root);

            // Humanoid animator configuration (T2_R3_5)
            var animator = root.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }

            var state = root.GetComponent<PanicExteriorOwnedArt>() ?? root.AddComponent<PanicExteriorOwnedArt>();

            // Authentic Milestone 3 responder dressing (T1_R3_1..4, T1_R3_5)
            PanicCharacterArt.DressResponder(root, animator, role);

            // Final physics verification pass
            PanicCharacterArt.StripPhysics(root);

            return root;
        }

        public static GameObject CreateStream()
        {
            PanicHomeArt.EnsurePalette();var root=new GameObject("Flowing neighbourhood creek");var b=new B();
            var zs=new List<float>();for(int i=0;i<=52;i++)zs.Add(-CreekLength*.5f+CreekLength*i/52f);zs.Add(RoadNear);zs.Add(RoadFar);zs.Sort();
            for(int i=0;i<zs.Count-1;i++)
            {
                float a=zs[i],z=zs[i+1],ca=Centre(a),cz=Centre(z);
                // Sloping banks meet a carved opening in the art ground; no flat blue lawn decal.
                b.Quad(new Vector3(CreekLeft,-.031f,a),new Vector3(ca-1f,-.62f,a),new Vector3(cz-1f,-.62f,z),new Vector3(CreekLeft,-.031f,z),M.Earth);
                b.Quad(new Vector3(ca+1f,-.62f,a),new Vector3(CreekRight,-.031f,a),new Vector3(CreekRight,-.031f,z),new Vector3(cz+1f,-.62f,z),M.Earth);
                b.Quad(new Vector3(ca-1,-.62f,a),new Vector3(ca+1,-.62f,a),new Vector3(cz+1,-.62f,z),new Vector3(cz-1,-.62f,z),M.Stone);
                if(a>=RoadNear&&z<=RoadFar)continue;
                if(i%3==0)
                {
                    for(int side=-1;side<=1;side+=2)
                    {
                        float x=side<0?CreekLeft+.13f:CreekRight-.13f;
                        b.Ellipsoid(new Vector3(x,.025f,a),new Vector3(.20f,.10f,.27f),M.Stone,i*7+side);
                        for(int leaf=0;leaf<3;leaf++)b.Beam(new Vector3(x,.02f,a+.3f),new Vector3(x+.08f*(leaf-1),.22f+leaf*.05f,a+.33f+leaf*.035f),.016f,M.Leaf,.002f);
                    }
                }
            }
            // Clear, framed culvert openings where the stream passes under road and sidewalk.
            foreach(float z in new[]{RoadNear-.02f,RoadFar+.02f})
            {
                b.Box(new Vector3((CreekLeft+CreekRight)*.5f,-.02f,z),new Vector3(CreekRight-CreekLeft,.14f,.20f),M.Concrete,.01f);
                for(int side=-1;side<=1;side+=2)b.Box(new Vector3(side<0?CreekLeft+.17f:CreekRight-.17f,-.38f,z),new Vector3(.34f,.70f,.24f),M.Stone,.015f);
            }
            BuildCreekSafetyV414(root,b);
            b.Attach(root,"CreekBanksRocksCulvert");
            var state=root.AddComponent<PanicExteriorStreamState>();state.Water=MakeWater(root,zs);state.Foam=MakeFoam(root);
            state.RuntimeOwned=Application.isPlaying;
            AnimateLandscape(root,0,false);return root;
        }

        static void BuildCreekSafetyV414(GameObject root,B b)
        {
            const float height=1.65f;
            float center=(CreekLeft+CreekRight)*.5f,width=CreekRight-CreekLeft;
            // A real bridge deck preserves the street crossing. Rails close both water-facing edges.
            SafetyBox(root,"Bridge deck",new Vector3(center,-.075f,(RoadNear+RoadFar)*.5f),new Vector3(width+.7f,.14f,RoadFar-RoadNear));
            foreach(float z in new[]{RoadNear,RoadFar})
            {
                SafetyBox(root,"Bridge railing",new Vector3(center,height*.5f,z),new Vector3(width+.75f,height,.18f));
                for(int i=0;i<10;i++)b.Box(new Vector3(CreekLeft-.32f+i*(width+.64f)/9,height*.5f,z),new Vector3(.065f,height,.065f),M.Steel,.007f);
                foreach(float y in new[]{.22f,.86f,1.62f})b.Box(new Vector3(center,y,z),new Vector3(width+.75f,.065f,.09f),M.Charcoal,.009f);
            }
            // Continuous solid banks reject walking/jumping into the water from either side.
            // All these colliders are on the landscape, so floor cutaway never disables them.
            foreach(float x in new[]{CreekLeft-.16f,CreekRight+.16f})
            {
                for(int segment=0;segment<2;segment++)
                {
                    float start=segment==0?-CreekLength*.5f:RoadFar,end=segment==0?RoadNear:CreekLength*.5f;
                    SafetyBox(root,"Creek bank guard",new Vector3(x,height*.5f,(start+end)*.5f),new Vector3(.18f,height,end-start+.18f));
                    int count=Mathf.CeilToInt((end-start)/2.3f);
                    for(int i=0;i<=count;i++)b.Box(new Vector3(x,height*.5f,Mathf.Lerp(start,end,(float)i/count)),new Vector3(.11f,height,.11f),M.WalnutDark,.012f);
                    foreach(float y in new[]{.28f,.88f,1.60f})b.Box(new Vector3(x,y,(start+end)*.5f),new Vector3(.07f,.065f,end-start),M.Walnut,.009f);
                }
            }
        }
        static void SafetyBox(GameObject root,string name,Vector3 position,Vector3 size)
        {var go=new GameObject("V414 "+name);go.transform.SetParent(root.transform,false);go.transform.localPosition=position;go.AddComponent<BoxCollider>().size=size;}

        static Mesh MakeWater(GameObject root,List<float> zs)
        {
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            for(int i=0;i<zs.Count-1;i++)
            {
                float a=zs[i],z=zs[i+1];if(a>=RoadNear&&z<=RoadFar)continue;int n=vertices.Count;
                vertices.Add(new Vector3(SurfaceEdge(a,-.24f,false),-.24f,a));vertices.Add(new Vector3(SurfaceEdge(a,-.24f,true),-.24f,a));
                vertices.Add(new Vector3(SurfaceEdge(z,-.24f,true),-.24f,z));vertices.Add(new Vector3(SurfaceEdge(z,-.24f,false),-.24f,z));
                uv.Add(new Vector2(0,a/2.8f));uv.Add(new Vector2(1.7f,a/2.8f));uv.Add(new Vector2(1.7f,z/2.8f));uv.Add(new Vector2(0,z/2.8f));
                triangles.AddRange(new[]{n,n+3,n+2,n,n+2,n+1});
            }
            var mesh=new Mesh{name="Creek flowing water"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var mat=new Material(Shader.Find("Standard")){name="Exterior flowing teal water",color=Color.white};mat.SetFloat("_Metallic",.12f);mat.SetFloat("_Glossiness",.72f);
            mat.mainTexture=Resources.Load<Texture2D>("Exterior/StreamFlow");if(!mat.mainTexture)mat.color=new Color(.13f,.36f,.39f);
            var go=new GameObject("Direction readable flowing water");go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;
            root.GetComponent<PanicExteriorStreamState>().WaterRenderer=r;return mesh;
        }
        static Mesh MakeFoam(GameObject root)
        {
            var mesh=new Mesh{name="Bounded moving stream foam"};mesh.vertices=new Vector3[48];var t=new int[72];for(int i=0;i<12;i++){int n=i*4,k=i*6;t[k]=n;t[k+1]=n+3;t[k+2]=n+2;t[k+3]=n;t[k+4]=n+2;t[k+5]=n+1;}mesh.triangles=t;
            var go=new GameObject("Downstream foam marks");go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=new Material(Shader.Find("Standard")){name="Creek foam ivory",color=new Color(.65f,.77f,.71f)};r.shadowCastingMode=ShadowCastingMode.Off;
            root.GetComponent<PanicExteriorStreamState>().FoamRenderer=r;
            mesh.MarkDynamic();return mesh;
        }
        public static void AnimateLandscape(GameObject landscape,float time,bool floodAftermath)
        {
            if(!landscape)return;var s=landscape.GetComponentInChildren<PanicExteriorStreamState>(true);if(!s||!s.WaterRenderer)return;
            s.EnsureRuntimeOwnership();
            float target=floodAftermath?-.08f:-.24f;
            if(!s.Initialized){s.Surface=-.24f;s.Initialized=true;}
            float level=Application.isPlaying?Mathf.MoveTowards(s.Surface,target,Mathf.Max(0,Time.deltaTime)*.1f):target;
            if(s.WaterVertices==null)s.WaterVertices=s.Water.vertices;
            if(Mathf.Abs(s.Surface-level)>.00001f||!Application.isPlaying)
            {
                for(int i=0;i<s.WaterVertices.Length;i++)
                {
                    var v=s.WaterVertices[i];v.x=SurfaceEdge(v.z,level,i%4==1||i%4==2);v.y=level;s.WaterVertices[i]=v;
                }
                s.Water.vertices=s.WaterVertices;s.Water.RecalculateBounds();
            }
            s.Surface=level;
            s.WaterRenderer.sharedMaterial.mainTextureOffset=new Vector2(Mathf.Sin(time*.11f)*.015f,time*(floodAftermath?.34f:.23f));
            if(s.FoamVertices==null)s.FoamVertices=new Vector3[48];var foam=s.FoamVertices;
            for(int i=0;i<12;i++)
            {
                float z=CreekLength*.5f-Mathf.Repeat(i*6.19f+time*(floodAftermath?1.20f:.72f),CreekLength);
                float x=Centre(z)+Mathf.Sin(i*2.4f+time*.3f)*.67f;float width=.18f+(i%3)*.05f;float y=level+.008f;
                if(z>RoadNear-.35f&&z<RoadFar+.35f)y=-1.5f;
                int n=i*4;foam[n]=new Vector3(x-width,y,z-.025f);foam[n+1]=new Vector3(x+width,y,z-.05f);foam[n+2]=new Vector3(x+width*.7f,y,z+.02f);foam[n+3]=new Vector3(x-width*.8f,y,z+.035f);
            }
            s.Foam.vertices=foam;s.Foam.RecalculateNormals();s.Foam.RecalculateBounds();
        }
        static void Remove(UnityEngine.Object obj){if(Application.isPlaying)UnityEngine.Object.Destroy(obj);else UnityEngine.Object.DestroyImmediate(obj);}
    }
}
