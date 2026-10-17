using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Original presentation-only construction. Root owns input, costs, timing and physics.</summary>
    public sealed class PanicConstructionArt : MonoBehaviour
    {
        sealed class Part
        {
            public Transform Transform;
            public Vector3 Position, Scale, Approach;
            public Quaternion Rotation;
        }
        readonly List<Part> parts = new List<Part>();
        readonly List<Mesh> meshes = new List<Mesh>();
        readonly List<Material> materials = new List<Material>();
        Transform high, low;
        string itemId = "";
        ItemKind kind;
        bool row, station;
        float progress = 1;
        public int TreeCount { get; private set; }
        public int Tier { get; private set; }
        public float BuildProgress { get { return progress; } }
        public int AuthoredTriangleCount { get; private set; }
        public int AuthoredRendererCount { get; private set; }
        public int OwnedMaterialCount { get { return materials.Count; } }
        public const float SuggestedBuildSeconds = .60f;
        public const float TreeTrunkRadius = .11f;

        static GameObject New(string name, out PanicConstructionArt state)
        {
            var root = new GameObject(name);
            state = root.AddComponent<PanicConstructionArt>();
            return root;
        }
        Material Material(string name, Color color, float smooth = .2f, float metal = 0)
        {
            var result = new Material(Shader.Find("Standard"));
            result.name = "TLTP_Construct_" + name;
            result.color = color; result.SetFloat("_Glossiness", smooth); result.SetFloat("_Metallic", metal);
            materials.Add(result); return result;
        }
        Material[] StructureMaterials()
        {
            return new[] {
                Material("Warm mineral panel",new Color(.68f,.65f,.57f),.12f),
                Material("Galvanised folded steel",new Color(.38f,.43f,.44f),.32f,.64f),
                Material("Deep steel fasteners",new Color(.13f,.17f,.17f),.28f,.58f),
                Material("Ochre safety witness marks",new Color(.84f,.38f,.12f),.19f),
                Material("Warm rubber gasket",new Color(.075f,.087f,.082f),.10f)
            };
        }

        /// <summary>Fully assembled child, same local frame as item. Never adds a second wall rotation.</summary>
        public static GameObject CreateReinforcement(PanicItem item)
        {
            if (item == null || item.Tier <= 0 || (item.Kind != ItemKind.Wall && item.Kind != ItemKind.Window && item.Kind != ItemKind.Door && item.Kind != ItemKind.Roof)) return null;
            PanicConstructionArt s;
            var root = New("Reinforced_" + item.Id + "_T" + Mathf.Clamp(item.Tier, 1, 2), out s);
            s.itemId = item.Id ?? ""; s.kind = item.Kind; s.Tier = Mathf.Clamp(item.Tier, 1, 2);
            var palette = s.StructureMaterials();
            if (item.Kind == ItemKind.Wall) s.Wall(item, palette);
            else if (item.Kind == ItemKind.Window) s.Window(item, palette);
            else if (item.Kind == ItemKind.Door) s.Door(item, palette);
            else s.Roof(item, palette);
            AnimateBuild(root, 1);
            return root;
        }

        void Wall(PanicItem item, Material[] m)
        {
            float w = Safe(item.Size.x, 1, 30, 16), h = Safe(item.Size.y, 1, 5, 3.2f);
            bool basement = itemId.Contains("basement"), south = itemId.Contains("wall_south");
            bool side = itemId.Contains("wall_west") || itemId.Contains("wall_east");
            var facing = Child(transform, "WallLocalFacing", Vector3.zero);
            if (south) facing.localRotation = Quaternion.Euler(0, 180, 0);
            low = Child(facing, "FrontLowWalls", Vector3.zero);
            high = Child(facing, "FrontHighWalls", Vector3.zero);
            Geo lowSkin = new Geo(), lowSteel = new Geo(), highSkin = new Geo(), highSteel = new Geo(), highTrim = new Geo();
            var cuts = new List<Vector4>();
            if (!basement)
            {
                float c = side ? 3 : 4;
                cuts.Add(new Vector4(-c-.9f,-c+.9f,.95f,2.35f));
                cuts.Add(new Vector4(c-.9f,c+.9f,.95f,2.35f));
                if (south) cuts.Add(new Vector4(-.6f,.6f,0,2.25f));
            }
            var edges = new List<float> { -w*.5f, w*.5f };
            foreach (var cut in cuts) { edges.Add(cut.x); edges.Add(cut.y); }
            edges.Sort();
            for (int i=0;i<edges.Count-1;i++)
            {
                float a=edges[i], b=edges[i+1], mid=(a+b)*.5f;
                bool opening=false;
                foreach(var cut in cuts) if(mid>cut.x+.001f && mid<cut.y-.001f)
                {
                    if(cut.z>.001f) WallRect(a,b,0,cut.z,lowSkin,lowSteel,highSkin,highSteel,highTrim);
                    if(cut.w<h) WallRect(a,b,cut.w,h,lowSkin,lowSteel,highSkin,highSteel,highTrim);
                    opening=true;break;
                }
                if(!opening)WallRect(a,b,0,h,lowSkin,lowSteel,highSkin,highSteel,highTrim);
            }
            AddPart(low,"Mineral_lower_panels",lowSkin,m,new Vector3(0,.10f,-.18f));
            AddPart(low,"Bolted_lower_bindings",lowSteel,m,new Vector3(.14f,.04f,-.12f));
            AddPart(high,"Mineral_upper_panels",highSkin,m,new Vector3(0,.18f,-.15f));
            AddPart(high,"Steel_pier_frames",highSteel,m,new Vector3(-.16f,.08f,-.10f));
            AddPart(high,"Corner_plates_and_witness_marks",highTrim,m,new Vector3(.14f,.12f,-.09f));
        }
        void WallRect(float left,float right,float bottom,float top,Geo lowSkin,Geo lowSteel,Geo highSkin,Geo highSteel,Geo trim)
        {
            if(right-left<.015f || top-bottom<.015f)return;
            float split=Mathf.Clamp(.45f,bottom,top);
            if(split>bottom+.01f)WallPanel(left,right,bottom,split,lowSkin,lowSteel,lowSteel);
            if(top>split+.01f)WallPanel(left,right,split,top,highSkin,highSteel,trim);
        }
        void WallPanel(float a,float b,float lo,float hi,Geo skin,Geo steel,Geo trim)
        {
            float w=b-a,h=hi-lo;
            for(int face=0;face<2;face++)
            {
                float z=face==0?-.18f:.25f;
                float outZ=face==0?-.024f:.024f;
                // Segmented cladding follows only existing solid shell rectangles.
                int panels=Mathf.Max(1,Mathf.CeilToInt(w/1.6f));
                for(int n=0;n<panels;n++)
                {
                    float x=a+w*(n+.5f)/panels, pw=w/panels;
                    skin.Box(new Vector3(x,(lo+hi)*.5f,z),new Vector3(Mathf.Max(.01f,pw-.022f),Mathf.Max(.01f,h-.018f),.022f),0);
                    if(Tier>1 && h>.3f)
                    {
                        steel.Box(new Vector3(x,lo+Mathf.Min(.13f,h*.22f),z+outZ),new Vector3(Mathf.Max(.02f,pw-.07f),.027f,.019f),1);
                        steel.Box(new Vector3(x,hi-Mathf.Min(.13f,h*.22f),z+outZ),new Vector3(Mathf.Max(.02f,pw-.07f),.027f,.019f),1);
                    }
                }
                float margin=Mathf.Min(.08f,w*.16f), rail=Mathf.Min(.055f,w*.20f);
                foreach(float x in new[]{a+margin,b-margin})
                {
                    steel.Box(new Vector3(x,(lo+hi)*.5f,z+outZ),new Vector3(rail,h,.028f),1);
                    if(h>.24f)foreach(float y in new[]{lo+.09f,hi-.09f})
                    {
                        trim.Box(new Vector3(x,y,z+outZ*1.9f),new Vector3(Mathf.Min(.14f,w*.4f),.12f,.016f),2);
                        trim.Bolt(new Vector3(x,y,z+outZ*2.3f),face==0?-1:1,1);
                        trim.Box(new Vector3(x+.028f,y+.028f,z+outZ*2.8f),new Vector3(.035f,.009f,.006f),3);
                    }
                }
                if(h>.65f && w>.4f && Tier>1)
                {
                    steel.Beam(new Vector3(a+margin,lo+.10f,z+outZ),new Vector3(b-margin,hi-.10f,z+outZ),.041f,.024f,1);
                    // Each X brace stays INSIDE one real pier; never crosses a window or doorway.
                    steel.Beam(new Vector3(b-margin,lo+.10f,z+outZ),new Vector3(a+margin,hi-.10f,z+outZ),.041f,.024f,1);
                }
            }
        }

        void Window(PanicItem item,Material[] m)
        {
            float w=Safe(item.Size.x,1,3,1.8f), h=Safe(item.Size.y,.8f,2.5f,1.4f), lo=.95f, cy=lo+h*.5f;
            var frame=new Geo();var fittings=new Geo();var secondary=new Geo();
            for(int side=0;side<2;side++)
            {
                float z=side==0?-.16f:.16f, face=side==0?-1:1;
                for(int sign=-1;sign<=1;sign+=2)
                {
                    frame.Box(new Vector3(sign*(w*.5f-.018f),cy,z),new Vector3(.068f,h+.08f,.055f),1);
                    frame.Box(new Vector3(0,cy+sign*(h*.5f+.005f),z),new Vector3(w+.09f,.062f,.055f),1);
                    frame.Box(new Vector3(sign*(w*.5f-.063f),cy,z+face*.009f),new Vector3(.020f,h-.07f,.014f),4);
                    frame.Box(new Vector3(0,cy+sign*(h*.5f-.052f),z+face*.009f),new Vector3(w-.12f,.020f,.014f),4);
                    for(int vertical=-1;vertical<=1;vertical+=2)
                    {
                        float x=sign*(w*.5f-.07f),y=cy+vertical*(h*.5f-.07f);
                        fittings.Box(new Vector3(x,y,z+face*.039f),new Vector3(.17f,.17f,.023f),2);
                        fittings.Beam(new Vector3(x-sign*.09f,y,z+face*.055f),new Vector3(x,y-vertical*.09f,z+face*.055f),.038f,.018f,1);
                        fittings.Bolt(new Vector3(x,y,z+face*.065f),face,1);
                    }
                }
                if(Tier>1)
                {
                    // Split side shutters stay folded OPEN. The glass/opening remains readable.
                    for(int sign=-1;sign<=1;sign+=2)
                    {
                        float x=sign*(w*.5f+.16f);
                        for(int slat=0;slat<7;slat++)secondary.Box(new Vector3(x,lo+.12f+slat*(h-.24f)/6,z),new Vector3(.25f,.11f,.037f),1,Quaternion.Euler(0,sign*28,0));
                        secondary.Box(new Vector3(x,cy,z+face*.03f),new Vector3(.025f,h-.08f,.023f),2);
                    }
                    secondary.Box(new Vector3(0,cy,z+face*.045f),new Vector3(.029f,h-.08f,.023f),1);
                }
            }
            AddPart(transform,"Reinforced_window_frame",frame,m,new Vector3(0,.12f,-.16f));
            AddPart(transform,"Corner_braces_and_bolts",fittings,m,new Vector3(.12f,0,-.14f));
            AddPart(transform,"Open_shutter_and_safety_mullion",secondary,m,new Vector3(-.12f,.05f,-.15f));
        }
        void Door(PanicItem item,Material[] m)
        {
            // Hardware is fixed to the jamb/header; no closed-leaf overlay can block a swinging door.
            float w=Safe(item.Size.x,.8f,2.0f,1.2f),h=2.25f;
            var frame=new Geo();var fix=new Geo();
            for(int face=-1;face<=1;face+=2)
            {
                float z=face*.145f;
                for(int side=-1;side<=1;side+=2)
                {
                    float x=side*(w*.5f-.045f);
                    frame.Box(new Vector3(x,h*.5f,z),new Vector3(.044f,h,.024f),1);
                    foreach(float y in new[]{.20f,.95f,1.75f,2.13f})
                    {fix.Box(new Vector3(x,y,z+face*.018f),new Vector3(.09f,.16f,.018f),2);fix.Bolt(new Vector3(x,y,z+face*.032f),face,1);}
                    if(Tier>1)fix.Beam(new Vector3(x-side*.12f,2.20f,z),new Vector3(x,2.06f,z),.047f,.022f,1);
                }
                frame.Box(new Vector3(0,2.215f,z),new Vector3(w,.045f,.024f),1);
            }
            AddPart(transform,"Reinforced_jamb_channels",frame,m,new Vector3(0,.08f,-.12f));
            AddPart(transform,"Jamb_tie_plates",fix,m,new Vector3(.10f,.04f,-.08f));
        }
        void Roof(PanicItem item,Material[] m)
        {
            high=Child(transform,"Roof_reinforcement",Vector3.zero);
            var ties=new Geo();var ridge=new Geo();var plates=new Geo();
            float halfZ=Safe(item.Size.z,6,20,12)*13.15f/24f,peak=2.25f;
            var xs=Tier>1?new[]{-6f,0f,6f}:new[]{-6f,6f};
            foreach(float x in xs)for(int sign=-1;sign<=1;sign+=2)
            {
                Vector3 a=new Vector3(x,.09f,sign*halfZ),b=new Vector3(x,peak+.09f,0);
                ties.Beam(a,b,.07f,.030f,1);
                for(int n=0;n<5;n++)
                {
                    Vector3 p=Vector3.Lerp(a,b,n/4f);
                    plates.Box(p+new Vector3(0,.025f,0),new Vector3(.20f,.025f,.18f),2);
                    plates.Cylinder(p+Vector3.up*.045f,.025f,.018f,1,Quaternion.identity,6);
                }
            }
            ridge.Box(new Vector3(0,peak+.105f,0),new Vector3(16.4f,.035f,.18f),1);
            AddPart(high,"Roof_slope_straps",ties,m,new Vector3(0,.20f,0));
            AddPart(high,"Ridge_cap_channel",ridge,m,new Vector3(.15f,.18f,0));
            AddPart(high,"Roof_anchor_plates",plates,m,new Vector3(-.13f,.12f,0));
        }

        /// <summary>Original mature tree rows. Variants0/1 along Z,2 along X. No colliders.</summary>
        public static GameObject CreateTreeRow(int regionVariant)
        {
            int variant=((regionVariant%3)+3)%3;
            PanicConstructionArt s;var root=New("Mature_tree_row_region_"+variant,out s);s.row=true;s.TreeCount=4;
            var palette=new[]{s.Material("Mature bark",new Color(.27f,.20f,.13f),.13f),s.Material("Canopy deep green",new Color(.20f,.31f,.16f),.12f),s.Material("Canopy sun green",new Color(.33f,.42f,.22f),.10f),s.Material("Mulched tree belt",new Color(.25f,.225f,.16f),.08f)};
            var seats=TreeTrunkSeats(variant);
            var mulch=new Geo();
            float length = 2 * TreeRowHalfSpan(variant) + 1.25f;
            if(variant==2)mulch.Box(new Vector3(0,.014f,0),new Vector3(length,.025f,.72f),3);
            else mulch.Box(new Vector3(0,.014f,0),new Vector3(.72f,.025f,length),3);
            s.AddPart(root.transform,"Continuous_mulch_region",mulch,palette,new Vector3(0,-.045f,0));
            for(int i=0;i<seats.Length;i++)
            {
                var g=new Geo();Vector3 p=seats[i];float h=3.65f+((i+variant)%3)*.19f;
                Vector3 crown=p+new Vector3((i%2==0?.06f:-.06f),h*.63f,0);
                g.Cylinder(p+Vector3.up*(h*.30f),TreeTrunkRadius,h*.60f,0,Quaternion.Euler(0,i*37,3*(i%2==0?1:-1)),10);
                for(int r=0;r<4;r++)
                {
                    float a=r*Mathf.PI*.5f+i*.41f;Vector3 foot=p+new Vector3(Mathf.Cos(a)*.29f,.025f,Mathf.Sin(a)*.29f);
                    g.Beam(foot,p+Vector3.up*.19f,.055f,.045f,0);
                }
                for(int branch=0;branch<6;branch++)
                {
                    float a=branch*Mathf.PI/3+i*.49f+variant*.27f;
                    Vector3 end=crown+new Vector3(Mathf.Cos(a)*.45f,.11f+(branch%2)*.27f,Mathf.Sin(a)*.45f);
                    g.Beam(p+Vector3.up*(h*.43f+branch*.055f),end,.057f,.052f,0);
                    float radius=.49f+(branch%3)*.035f;
                    g.Ellipsoid(end+Vector3.up*.27f,new Vector3(radius,.65f,radius*.94f),1+(branch+i)%2,10,7,branch+i*11+variant*7);
                }
                g.Ellipsoid(crown+Vector3.up*.78f,new Vector3(.62f,.72f,.60f),2,11,8,i+41);
                s.AddPart(root.transform,"Mature_tree_"+i,g,palette,new Vector3((i%2==0?-.13f:.13f),-.13f,0));
            }
            AnimateBuild(root,1);return root;
        }
        public static Vector3[] TreeTrunkSeats(int regionVariant)
        {
            int variant=((regionVariant%3)+3)%3;var result=new Vector3[4];
            float half = TreeRowHalfSpan(variant);
            for(int i=0;i<4;i++){float along=-half+i*(half*2/3);result[i]=variant==2?new Vector3(along,0,(i%2==0?-.06f:.06f)):new Vector3((i%2==0?-.06f:.06f),0,along);}
            return result;
        }
        public static float TreeRowHalfSpan(int regionVariant) { return ((regionVariant % 3) + 3) % 3 == 2 ? 6.3f : 4.3f; }
        public static Bounds TreeRowBounds(int regionVariant)
        {
            int variant=((regionVariant%3)+3)%3;
            float length = TreeRowHalfSpan(variant) * 2 + 2.5f;
            return new Bounds(new Vector3(0,2.10f,0),variant==2?new Vector3(length,4.30f,2.50f):new Vector3(2.50f,4.30f,length));
        }

        /// <summary>Grounded wall station, front -Z, back +Z. No fire/model readiness is mutated.</summary>
        public static GameObject CreateFireStation(bool ready, bool includeMount = true)
        {
            PanicConstructionArt s;var root=New("Dedicated_fire_equipment_station",out s);s.station=true;
            var palette=new[]{s.Material("Station folded steel",new Color(.31f,.36f,.35f),.35f,.65f),s.Material("Station rubber",new Color(.07f,.08f,.08f),.12f),s.Material("Safety warm red",new Color(.70f,.18f,.10f),.18f),s.Material("Safety pictogram cream",new Color(.91f,.85f,.69f),.13f)};
            var stand=new Geo();stand.Box(new Vector3(0,.74f,.055f),new Vector3(.28f,1.46f,.035f),0);
            stand.Box(new Vector3(0,.08f,-.035f),new Vector3(.27f,.055f,.23f),0);
            stand.Box(new Vector3(0,.115f,-.035f),new Vector3(.20f,.022f,.15f),1);
            for(int side=-1;side<=1;side+=2)stand.Box(new Vector3(side*.10f,.54f,0),new Vector3(.028f,.34f,.035f),0);
            stand.Box(new Vector3(0,1.28f,.016f),new Vector3(.25f,.30f,.025f),2);
            // A geometric extinguisher pictogram, not tiny provider text.
            stand.Box(new Vector3(0,1.26f,-.004f),new Vector3(.063f,.125f,.009f),3);
            stand.Box(new Vector3(.022f,1.34f,-.004f),new Vector3(.075f,.021f,.009f),3);
            stand.Beam(new Vector3(.047f,1.325f,-.005f),new Vector3(.081f,1.25f,-.005f),.012f,.009f,3);
            if(includeMount)s.AddPart(root.transform,"Fixed_station_mount",stand,palette,new Vector3(0,.08f,.03f));
            // The branch fire cabinet replaces the old exposed cylinder presentation.
            // Its real contained item is visual only; readiness still comes from completion.
            var cabinet=PanicCuratedAssetArt.CreateDisplay("FireCabinet",.74f);
            if(cabinet)
            {
                cabinet.name="Prepared_branch_fire_cabinet";cabinet.transform.SetParent(root.transform,false);
                cabinet.transform.localPosition=new Vector3(0,.13f,-.015f);
                foreach(var child in cabinet.GetComponentsInChildren<Transform>(true))
                    if(child.name=="PF_FireExtinguisher_Mounted")child.gameObject.SetActive(ready);
                s.RememberPart(cabinet.transform,new Vector3(.08f,.10f,-.10f));
            }
            else if(ready)
            {
                GameObject retained;
                if(!PanicCuratedAssetArt.TryCreatePickup("extinguisher",out retained))retained=PanicArt.Create("fireunit");
                if(retained)
                {
                    retained.name="Prepared_retained_extinguisher";retained.transform.SetParent(root.transform,false);
                    retained.transform.localPosition=new Vector3(0,.13f,-.055f);retained.transform.localScale=Vector3.one*1.55f;
                    s.RememberPart(retained.transform,new Vector3(.08f,.10f,-.10f));
                }
            }
            AnimateBuild(root,1);return root;
        }

        public static void AnimateBuild(GameObject root,float normalized01)
        {
            if(!root)return;var s=root.GetComponent<PanicConstructionArt>();if(!s)return;
            float p=float.IsNaN(normalized01)||float.IsInfinity(normalized01)?0:Mathf.Clamp01(normalized01);s.progress=p;
            int count=s.parts.Count;
            for(int i=0;i<count;i++)
            {
                var part=s.parts[i];if(!part.Transform)continue;
                float delay=count<=1?0:(i/(float)(count-1))*.23f;
                float t=Mathf.Clamp01((p-delay)/(1-delay));float eased=t*t*(3-2*t);
                // Reveal near-final, full-size pieces in sequence. In particular mature trees
                // are placed as mature trees, never scaled up from a seedling.
                part.Transform.gameObject.SetActive(t > .0001f);
                part.Transform.localPosition=part.Position+part.Approach*(1-eased);
                part.Transform.localRotation=part.Rotation*Quaternion.Euler(0,0,(i%2==0?-1:1)*(1-eased)*3f);
                part.Transform.localScale=part.Scale;
            }
        }
        public static void SetCutawayVisibility(GameObject root,bool inside,Vector3 worldCameraSide)
        {
            if(!root)return;var s=root.GetComponent<PanicConstructionArt>();if(!s||s.row||s.station)return;
            if(s.kind==ItemKind.Roof){if(s.high)s.high.gameObject.SetActive(!inside);return;}
            if(s.kind!=ItemKind.Wall||!s.high)return;
            bool occludes=s.itemId.Contains("wall_south")||s.itemId.Contains("wall_east")||s.itemId.Contains("wall_west")&&worldCameraSide.x<-.1f||s.itemId.Contains("wall_north")&&worldCameraSide.z>.1f;
            s.high.gameObject.SetActive(!inside||!occludes);
        }

        static Transform Child(Transform parent,string name,Vector3 p)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
        void RememberPart(Transform t,Vector3 approach)
        {parts.Add(new Part{Transform=t,Position=t.localPosition,Rotation=t.localRotation,Scale=t.localScale,Approach=approach});}
        void AddPart(Transform parent,string name,Geo g,Material[] palette,Vector3 approach)
        {
            if(g.Vertices.Count==0)return;
            var t=Child(parent,name,Vector3.zero);var mesh=new Mesh{name="TLTP_Original_"+name};
            if(g.Vertices.Count>65535)mesh.indexFormat=IndexFormat.UInt32;
            mesh.SetVertices(g.Vertices);mesh.SetUVs(0,g.UV);
            var used=new List<Material>();int sub=0;
            foreach(var pair in g.Triangles)if(pair.Value.Count>0)used.Add(palette[pair.Key]);
            mesh.subMeshCount=used.Count;
            foreach(var pair in g.Triangles)if(pair.Value.Count>0){mesh.SetTriangles(pair.Value,sub++);AuthoredTriangleCount+=pair.Value.Count/3;}
            mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.RecalculateTangents();
            t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=t.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterials=used.ToArray();renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;
            meshes.Add(mesh);AuthoredRendererCount++;RememberPart(t,approach);
        }
        void OnDestroy()
        {
            foreach(var mesh in meshes)if(mesh){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}
            foreach(var material in materials)if(material){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
            meshes.Clear();materials.Clear();parts.Clear();
        }
        static float Safe(float v,float min,float max,float fallback){return float.IsNaN(v)||float.IsInfinity(v)?fallback:Mathf.Clamp(Mathf.Abs(v),min,max);}

        sealed class Geo
        {
            public readonly List<Vector3> Vertices=new List<Vector3>();
            public readonly List<Vector2> UV=new List<Vector2>();
            public readonly SortedDictionary<int,List<int>> Triangles=new SortedDictionary<int,List<int>>();
            void Face(int material,params Vector3[] points)
            {
                int start=Vertices.Count;Vertices.AddRange(points);foreach(var p in points)UV.Add(new Vector2(p.x,p.y));
                List<int> tris;if(!Triangles.TryGetValue(material,out tris)){tris=new List<int>();Triangles.Add(material,tris);}
                for(int i=1;i<points.Length-1;i++)
                {
                    if(Vector3.Cross(points[i]-points[0],points[i+1]-points[0]).sqrMagnitude<1e-12f)continue;
                    tris.Add(start);tris.Add(start+i);tris.Add(start+i+1);
                }
            }
            public void Box(Vector3 c,Vector3 size,int material,Quaternion rotation=default(Quaternion))
            {
                Vector3 d=size*.5f;var q=rotation.Equals(default(Quaternion))?Quaternion.identity:rotation;
                Vector3 a=c+q*new Vector3(-d.x,-d.y,-d.z),b=c+q*new Vector3(-d.x,d.y,-d.z),cc=c+q*new Vector3(d.x,d.y,-d.z),dd=c+q*new Vector3(d.x,-d.y,-d.z);
                Vector3 e=c+q*new Vector3(-d.x,-d.y,d.z),f=c+q*new Vector3(-d.x,d.y,d.z),g=c+q*new Vector3(d.x,d.y,d.z),h=c+q*new Vector3(d.x,-d.y,d.z);
                Face(material,a,b,cc,dd);Face(material,h,g,f,e);Face(material,e,f,b,a);Face(material,dd,cc,g,h);Face(material,b,f,g,cc);Face(material,e,a,dd,h);
            }
            public void Beam(Vector3 a,Vector3 b,float width,float depth,int material)
            {if((b-a).sqrMagnitude<.000001f)return;Box((a+b)*.5f,new Vector3(width,Vector3.Distance(a,b),depth),material,Quaternion.FromToRotation(Vector3.up,(b-a).normalized));}
            public void Cylinder(Vector3 c,float radius,float height,int material,Quaternion rotation,int sides=12)
            {
                var q=rotation.Equals(default(Quaternion))?Quaternion.identity:rotation;
                for(int i=0;i<sides;i++)
                {
                    float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                    Vector3 al=c+q*new Vector3(Mathf.Cos(a)*radius,-height*.5f,Mathf.Sin(a)*radius),ah=c+q*new Vector3(Mathf.Cos(a)*radius,height*.5f,Mathf.Sin(a)*radius);
                    Vector3 bl=c+q*new Vector3(Mathf.Cos(b)*radius,-height*.5f,Mathf.Sin(b)*radius),bh=c+q*new Vector3(Mathf.Cos(b)*radius,height*.5f,Mathf.Sin(b)*radius);
                    Face(material,al,ah,bh,bl);Face(material,c+q*Vector3.up*height*.5f,bh,ah);Face(material,c-q*Vector3.up*height*.5f,al,bl);
                }
            }
            public void Bolt(Vector3 p,float facing,int material)
            {
                Cylinder(p,.022f,.018f,material,Quaternion.Euler(90,0,0),6);
                Box(p+new Vector3(0,0,facing*.012f),new Vector3(.021f,.005f,.004f),2);
                Box(p+new Vector3(0,0,facing*.012f),new Vector3(.005f,.021f,.004f),2);
            }
            public void Ellipsoid(Vector3 c,Vector3 radius,int material,int longitude,int latitude,int seed)
            {
                for(int y=0;y<latitude;y++)for(int x=0;x<longitude;x++)
                {
                    float a=-Mathf.PI*.5f+y*Mathf.PI/latitude,b=-Mathf.PI*.5f+(y+1)*Mathf.PI/latitude;
                    float u=x*Mathf.PI*2/longitude,v=(x+1)*Mathf.PI*2/longitude;
                    Face(material,LeafPoint(c,radius,a,u,seed),LeafPoint(c,radius,b,u,seed),LeafPoint(c,radius,b,v,seed),LeafPoint(c,radius,a,v,seed));
                }
            }
            static Vector3 LeafPoint(Vector3 c,Vector3 r,float lat,float lon,int seed)
            {
                float n=1+.06f*Mathf.Sin(lon*3+seed*.73f)*Mathf.Cos(lat*4+seed*.21f);
                return c+Vector3.Scale(new Vector3(Mathf.Cos(lat)*Mathf.Cos(lon),Mathf.Sin(lat),Mathf.Cos(lat)*Mathf.Sin(lon))*n,r);
            }
        }
    }
}
