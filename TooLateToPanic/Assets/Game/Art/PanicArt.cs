using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Original mesh-authored diorama library. Metres, Y up, +Z back; grounded pivots.</summary>
    public static class PanicArt
    {
        enum M { Ink, Oak, OakLight, OakDark, Cream, Plaster, Teal, Sage, Amber, Orange, Steel, Glass, Linen, Red, Blue, Leaf, Earth, White, Black, Glow }
        static readonly Dictionary<string, Mesh> MeshCache = new Dictionary<string, Mesh>();
        static readonly Dictionary<string, List<Part>> ModelCache = new Dictionary<string, List<Part>>();
        static readonly Dictionary<int, ArtState> States = new Dictionary<int, ArtState>();
        static readonly Dictionary<string, AudioClip> Sounds = new Dictionary<string, AudioClip>();
        static readonly Dictionary<int,VfxState> Effects = new Dictionary<int,VfxState>();
        static readonly Dictionary<string,Material> EffectMaterials = new Dictionary<string,Material>();
        static Material[] Materials;
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        struct Part { public Mesh mesh; public int material; }
        sealed class ArtState
        {
            public GameObject root, geometry, tier, damage;
            public Renderer[] renderers;
            public bool selected;
            public float amount;
            public DisasterType disaster;
            public string id;
        }
        sealed class VfxState
        {
            public DisasterType type;
            public GameObject root;
            public Transform water;
            public readonly List<Transform> particles=new List<Transform>();
            public readonly List<Vector3> origins=new List<Vector3>();
            public readonly List<Vector3> scales=new List<Vector3>();
        }

        public static GameObject Create(string artId)
        {
            EnsureMaterials();
            string id = (artId ?? "cabinet").ToLowerInvariant();
            var root = new GameObject("Art_" + id);
            var geometry = new GameObject("ArtGeometry");
            geometry.transform.SetParent(root.transform, false);
            List<Part> parts;
            if (!ModelCache.TryGetValue(id, out parts))
            {
                var b = new Builder();
                switch (id)
                {
                    case "bookshelf": Bookshelf(b); break;
                    case "sofa": Sofa(b); break;
                    case "table": Table(b); break;
                    case "chair": Chair(b); break;
                    case "fridge": Fridge(b); break;
                    case "cabinet": Cabinet(b); break;
                    case "bed": Bed(b); break;
                    case "plant": Plant(b, Vector3.zero, 1); break;
                    case "tvstand": TvStand(b); break;
                    case "bagstation": BagStation(b); break;
                    case "bunker": Bunker(b); break;
                    case "window": Window(b); break;
                    case "door": Door(b); break;
                    case "wall": Wall(b); break;
                    case "roof": Roof(b); break;
                    case "fireunit": FireUnit(b, Vector3.zero, 1); break;
                    case "water": Water(b, Vector3.zero, 1); break;
                    case "food": Food(b, Vector3.zero, 1); break;
                    case "firstaid": FirstAid(b, Vector3.zero, 1); break;
                    case "flashlight": Flashlight(b, Vector3.zero, 1); break;
                    case "radio": Radio(b, Vector3.zero, 1); break;
                    case "blanket": Blanket(b, Vector3.zero, 1); break;
                    default: Cabinet(b); break;
                }
                parts = b.Finish(id);
                ModelCache[id] = parts;
            }
            Attach(geometry, parts);
            var state = new ArtState { root = root, geometry = geometry, id = id };
            state.renderers = geometry.GetComponentsInChildren<Renderer>();
            States[root.GetInstanceID()] = state;
            return root;
        }

        public static GameObject CreateRoom()
        {
            EnsureMaterials();
            var root = new GameObject("Warm_Oak_Diorama_Room");
            var b = new Builder();
            b.B(new Vector3(0,-.38f,0), new Vector3(10.5f,.55f,8.5f), M.Ink,.09f);
            b.B(new Vector3(0,-.12f,0), new Vector3(10.28f,.12f,8.28f), M.OakDark,.035f);
            b.B(new Vector3(0,-.035f,0), new Vector3(10.1f,.07f,8.1f), M.Oak,.018f);
            // Actual plank geometry, fine dark joints, staggered wood tones.
            for (int z=0;z<20;z++)
            {
                float start=-5;
                int segment=0;
                while(start<4.999f)
                {
                    float length=Mathf.Min(segment==0 && z%2==1 ? 1.2f : 2.5f,5-start);
                    float x=start+length*.5f;
                    M wood = (z*3+segment)%5==0?M.OakLight:((z+segment)%4==0?M.OakDark:M.Oak);
                    b.B(new Vector3(x,.006f,-3.8f+z*.4f), new Vector3(length-.012f,.018f,.389f),wood,.008f);
                    if ((z+segment)%3==0)
                        b.B(new Vector3(x,.017f,-3.75f+z*.4f),new Vector3(length*.59f,.002f,.008f),M.OakDark,.001f);
                    start+=length;segment++;
                }
            }
            // Thin perimeter skirting only: no selectable wall geometry in room shell.
            b.B(new Vector3(0,.075f,3.985f),new Vector3(10,.15f,.075f),M.Cream,.014f);
            b.B(new Vector3(-4.985f,.075f,0),new Vector3(.075f,.15f,8),M.Cream,.014f);
            b.B(new Vector3(0,.065f,-3.985f),new Vector3(10,.13f,.07f),M.OakDark,.014f);
            b.B(new Vector3(4.985f,.065f,0),new Vector3(.07f,.13f,8),M.OakDark,.014f);
            // Quiet geometric woven rug; center and eastern bunker route stay uncluttered.
            b.B(new Vector3(-1.15f,.032f,-.35f),new Vector3(3.9f,.038f,2.65f),M.Cream,.025f);
            b.B(new Vector3(-1.15f,.055f,-.35f),new Vector3(3.64f,.014f,2.4f),M.Teal,.015f);
            for(int i=0;i<7;i++)
                b.B(new Vector3(-1.15f,.065f,-1.31f+i*.32f),new Vector3(3.42f,.005f,.025f),M.Sage,.001f);
            for(int s=-1;s<=1;s+=2)
                for(int i=0;i<18;i++) b.B(new Vector3(-2.89f+i*.205f,.04f,-.35f+s*1.38f),new Vector3(.026f,.012f,.17f),M.Linen,.004f);
            for(int i=0;i<3;i++)
            {
                float x=-2.1f+i*.95f;
                b.B(new Vector3(x,.07f,-.35f),new Vector3(.34f,.004f,.34f),M.Amber,.001f,Quaternion.Euler(0,45,0));
                b.B(new Vector3(x,.074f,-.35f),new Vector3(.19f,.004f,.19f),M.Teal,.001f,Quaternion.Euler(0,45,0));
            }
            // Foundation edge hardware and engraved sill rhythm.
            for(int i=0;i<11;i++)
            {
                b.B(new Vector3(-5+i,.0f,-4.11f),new Vector3(.06f,.025f,.07f),M.Steel,.008f);
                b.B(new Vector3(5.11f,.0f,-4+i*.8f),new Vector3(.07f,.025f,.06f),M.Steel,.008f);
            }
            Attach(root,b.Finish("room"));
            return root;
        }

        public static void SetTier(GameObject target,int tier)
        {
            ArtState s;
            if(!TryState(target,out s)) return;
            if(s.tier) UnityEngine.Object.Destroy(s.tier);
            s.tier=null;
            if(tier<=0) return;
            var g=new GameObject("Reinforcement_T"+tier);
            g.transform.SetParent(target.transform,false);
            var b=new Builder();
            Bounds bounds=LocalBounds(s.geometry);
            Vector3 size=bounds.size;
            Vector3 c=bounds.center;
            float front=bounds.min.z-.018f;
            bool structural=s.id=="wall"||s.id=="door"||s.id=="window"||s.id=="roof";
            if(structural)
            {
                int posts=s.id=="wall"?6:2;
                for(int i=0;i<posts;i++)
                {
                    float x=Mathf.Lerp(bounds.min.x+.12f,bounds.max.x-.12f,posts==1?.5f:(float)i/(posts-1));
                    b.B(new Vector3(x,c.y,front),new Vector3(.075f,size.y*.94f,.075f),tier>1?M.Steel:M.OakDark,.012f);
                    for(int j=0;j<3;j++) b.C(new Vector3(x,bounds.min.y+.15f+j*size.y*.34f,front-.05f),.017f,.018f,M.Amber,8,Quaternion.Euler(90,0,0));
                }
                b.B(new Vector3(c.x,bounds.min.y+size.y*.18f,front-.01f),new Vector3(size.x*.96f,.1f,.075f),M.Teal,.012f);
                if(tier>1)
                {
                    b.B(new Vector3(c.x,bounds.min.y+size.y*.77f,front-.01f),new Vector3(size.x*.96f,.1f,.075f),M.Steel,.012f);
                    b.B(new Vector3(c.x,c.y,front+.022f),new Vector3(size.x*.93f,size.y*.9f,.025f),M.Sage,.012f);
                }
            }
            else
            {
                for(int side=-1;side<=1;side+=2)
                {
                    float x=c.x+side*size.x*.41f;
                    b.B(new Vector3(x,bounds.min.y+.12f,front),new Vector3(.09f,.24f,.07f),M.Steel,.012f);
                    b.C(new Vector3(x,bounds.min.y+.12f,front-.045f),.025f,.022f,M.Amber,8,Quaternion.Euler(90,0,0));
                }
                if(tier>1) b.B(new Vector3(c.x,c.y,front),new Vector3(size.x*.87f,.07f,.06f),M.Teal,.014f);
            }
            Attach(g,b.Finish("tier_"+s.id+"_"+tier));
            s.tier=g;
            Refresh(s);
        }

        public static void SetDamage(GameObject target,float damage,DisasterType disaster)
        {
            ArtState s;
            if(!TryState(target,out s)) return;
            s.amount=Mathf.Clamp01(damage/100f);
            s.disaster=disaster;
            if(s.damage) UnityEngine.Object.Destroy(s.damage);
            s.damage=null;
            s.geometry.transform.localRotation=Quaternion.identity;
            s.geometry.transform.localPosition=Vector3.zero;
            if(s.amount>.035f)
            {
                var g=new GameObject("Damage_"+disaster);
                g.transform.SetParent(target.transform,false);
                var b=new Builder();
                Bounds bounds=LocalBounds(s.geometry);
                if(disaster==DisasterType.Flood)
                {
                    float h=Mathf.Min(bounds.size.y*.33f,.45f);
                    b.B(new Vector3(bounds.center.x,h*.5f,bounds.min.z-.012f),new Vector3(bounds.size.x*.92f,h,.016f),M.Blue,.005f);
                    for(int i=0;i<6;i++) b.B(new Vector3(Mathf.Lerp(bounds.min.x,bounds.max.x,(i+.5f)/6),h+.018f*(i%3),bounds.min.z-.025f),new Vector3(bounds.size.x/8,.027f,.016f),M.Sage,.004f);
                }
                else
                {
                    var rand=new System.Random(3187+s.id.Length);
                    for(int i=0;i<5;i++)
                    {
                        float x=Mathf.Lerp(bounds.min.x,bounds.max.x,.15f+.7f*(float)rand.NextDouble());
                        float y=Mathf.Lerp(bounds.min.y,bounds.max.y,.15f+.7f*(float)rand.NextDouble());
                        b.B(new Vector3(x,y,bounds.min.z-.022f),new Vector3(.025f,Mathf.Min(.4f,bounds.size.y*.22f),.018f),disaster==DisasterType.Fire?M.Black:M.OakDark,.006f,Quaternion.Euler(0,0,i%2==0?27:-33));
                    }
                    if(disaster==DisasterType.Earthquake && s.amount>.35f && s.id!="wall"&&s.id!="window"&&s.id!="door"&&s.id!="roof"&&s.id!="bunker")
                        s.geometry.transform.localRotation=Quaternion.Euler(0,0,s.amount*9);
                }
                Attach(g,b.Finish("damage_"+s.id));
                s.damage=g;
            }
            Refresh(s);
        }

        public static void SetSelected(GameObject target,bool selected)
        {
            ArtState s;
            if(!TryState(target,out s)) return;
            s.selected=selected;
            Refresh(s);
        }

        /// <summary>Register a baked prefab instance before applying upgrade, selection or damage state.</summary>
        public static void Register(GameObject target,string artId)
        {
            if(!target)return;
            EnsureMaterials();
            ArtState existing;
            if(States.TryGetValue(target.GetInstanceID(),out existing)&&existing.root)
            {
                existing.id=(artId??existing.id).ToLowerInvariant();
                return;
            }
            Transform geometry=target.transform.Find("ArtGeometry");
            if(!geometry)return;
            var state=new ArtState
            {
                root=target,
                geometry=geometry.gameObject,
                id=(artId??target.name.Replace("Art_","").Replace("(Clone)","")).ToLowerInvariant(),
                renderers=target.GetComponentsInChildren<Renderer>()
            };
            States[target.GetInstanceID()]=state;
        }

        static bool TryState(GameObject target,out ArtState state)
        {
            state=null;
            if(!target)return false;
            if(States.TryGetValue(target.GetInstanceID(),out state)&&state.root)return true;
            string id=target.name.Replace("Art_","").Replace("(Clone)","").ToLowerInvariant();
            if(id.StartsWith("wall_",StringComparison.Ordinal))id="wall";
            else if(id.StartsWith("window_",StringComparison.Ordinal))id="window";
            Register(target,id);
            return States.TryGetValue(target.GetInstanceID(),out state)&&state.root;
        }
        static void Refresh(ArtState s)
        {
            s.renderers=s.root.GetComponentsInChildren<Renderer>();
            var block=new MaterialPropertyBlock();
            foreach(var r in s.renderers)
            {
                Color original=r.sharedMaterial?r.sharedMaterial.color:Color.white;
                Color tint=original;
                if(s.amount>0)
                {
                    Color hurt=s.disaster==DisasterType.Fire?new Color(.15f,.12f,.1f):(s.disaster==DisasterType.Flood?new Color(.25f,.42f,.43f):new Color(.43f,.36f,.27f));
                    tint=Color.Lerp(original,hurt,s.amount*.72f);
                }
                block.Clear();
                block.SetColor(ColorId,tint);
                Color baseGlow=r.sharedMaterial&&r.sharedMaterial.HasProperty(EmissionId)?r.sharedMaterial.GetColor(EmissionId):Color.black;
                block.SetColor(EmissionId,s.selected?new Color(.34f,.48f,.25f):baseGlow);
                r.SetPropertyBlock(block);
            }
        }

        static void Bookshelf(Builder b)
        {
            b.B(new Vector3(0,1.16f,.19f),new Vector3(1.48f,2.26f,.075f),M.OakDark,.022f);
            for(int side=-1;side<=1;side+=2) b.B(new Vector3(side*.7f,1.17f,0),new Vector3(.11f,2.3f,.49f),M.Oak,.025f);
            b.B(new Vector3(0,2.31f,0),new Vector3(1.54f,.105f,.53f),M.OakLight,.026f);
            for(int j=0;j<5;j++) b.B(new Vector3(0,.1f+j*.52f,0),new Vector3(1.38f,.065f,.47f),j==0?M.OakDark:M.Oak,.016f);
            M[] bookColors={M.Teal,M.Cream,M.Amber,M.Orange,M.Sage,M.Blue};
            for(int row=0;row<4;row++)
                for(int i=0;i<9;i++)
                {
                    if(row==1&&i>5) continue;
                    float h=.25f+(i*7+row*3)%6*.027f;
                    float x=-.56f+i*.127f;
                    float y=.14f+row*.52f+h*.5f;
                    b.B(new Vector3(x,y,-.036f),new Vector3(.09f,h,.31f),bookColors[(i+row*2)%6],.008f,Quaternion.Euler(0,0,i==7?-9:0));
                    b.B(new Vector3(x,y+h*.31f,-.198f),new Vector3(.072f,.012f,.007f),M.Linen,.001f);
                    b.B(new Vector3(x,y-h*.28f,-.198f),new Vector3(.072f,.008f,.007f),M.OakDark,.001f);
                }
            b.B(new Vector3(.48f,.88f,-.03f),new Vector3(.3f,.32f,.32f),M.Linen,.025f);
            b.B(new Vector3(.48f,.9f,-.2f),new Vector3(.1f,.045f,.016f),M.OakDark,.008f);
            for(int side=-1;side<=1;side+=2) b.B(new Vector3(side*.58f,.045f,0),new Vector3(.13f,.09f,.38f),M.Ink,.012f);
        }
        static void Sofa(Builder b)
        {
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) b.B(new Vector3(x*.91f,.1f,z*.33f),new Vector3(.1f,.2f,.11f),M.OakDark,.018f,Quaternion.Euler(0,0,x*-8));
            b.B(new Vector3(0,.28f,0),new Vector3(2.32f,.29f,.94f),M.Ink,.075f);
            b.B(new Vector3(0,.39f,-.01f),new Vector3(2.24f,.22f,.96f),M.Teal,.08f);
            b.B(new Vector3(0,.75f,.35f),new Vector3(2.29f,.68f,.27f),M.Teal,.085f,Quaternion.Euler(-6,0,0));
            for(int side=-1;side<=1;side+=2) b.B(new Vector3(side*1.04f,.62f,-.015f),new Vector3(.25f,.58f,.97f),M.Teal,.095f);
            for(int i=-1;i<=1;i++)
            {
                b.B(new Vector3(i*.62f,.53f,-.08f),new Vector3(.6f,.18f,.69f),M.Sage,.062f);
                b.B(new Vector3(i*.62f,.8f,.27f),new Vector3(.59f,.48f,.16f),M.Teal,.055f,Quaternion.Euler(-9,0,0));
                for(int j=-1;j<=1;j+=2) b.B(new Vector3(i*.62f,.805f+j*.15f,.173f),new Vector3(.46f,.008f,.008f),M.Sage,.002f);
            }
            b.B(new Vector3(-.73f,.77f,.015f),new Vector3(.39f,.4f,.17f),M.Amber,.06f,Quaternion.Euler(-10,0,-14));
            b.B(new Vector3(.74f,.76f,.025f),new Vector3(.39f,.39f,.17f),M.Linen,.06f,Quaternion.Euler(-12,0,15));
            b.B(new Vector3(.47f,.64f,-.23f),new Vector3(.38f,.045f,.43f),M.Orange,.014f);
            for(int i=0;i<4;i++) b.B(new Vector3(.37f+i*.07f,.671f,-.23f),new Vector3(.012f,.005f,.39f),M.Cream,.001f);
        }
        static void Table(Builder b)
        {
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) b.B(new Vector3(x*.59f,.36f,z*.31f),new Vector3(.095f,.72f,.095f),M.OakDark,.02f,Quaternion.Euler(z*4,0,-x*4));
            b.B(new Vector3(0,.66f,0),new Vector3(1.28f,.12f,.69f),M.OakDark,.022f);
            b.B(new Vector3(0,.77f,0),new Vector3(1.55f,.13f,.94f),M.OakLight,.037f);
            for(int z=-1;z<=1;z++) b.B(new Vector3(0,.84f,z*.23f),new Vector3(1.45f,.004f,.009f),M.Oak,.001f);
            b.B(new Vector3(.3f,.852f,.05f),new Vector3(.43f,.018f,.55f),M.Linen,.008f,Quaternion.Euler(0,-9,0));
            b.C(new Vector3(.3f,.883f,.07f),.145f,.032f,M.Cream,20);
            b.C(new Vector3(.3f,.902f,.07f),.113f,.01f,M.OakLight,20);
            Mug(b,new Vector3(-.34f,.85f,-.12f),M.Teal);
            b.B(new Vector3(-.08f,.853f,.22f),new Vector3(.28f,.032f,.2f),M.Orange,.009f,Quaternion.Euler(0,12,0));
            b.B(new Vector3(-.08f,.874f,.22f),new Vector3(.25f,.008f,.18f),M.Cream,.003f,Quaternion.Euler(0,12,0));
        }
        static void Chair(Builder b)
        {
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) b.B(new Vector3(x*.205f,.23f,z*.2f),new Vector3(.055f,.46f,.055f),M.OakDark,.012f,Quaternion.Euler(z*7,0,-x*7));
            b.B(new Vector3(0,.46f,0),new Vector3(.55f,.085f,.55f),M.Oak,.025f);
            b.B(new Vector3(0,.516f,-.015f),new Vector3(.48f,.055f,.46f),M.Sage,.02f);
            for(int x=-1;x<=1;x+=2) b.B(new Vector3(x*.215f,.76f,.22f),new Vector3(.06f,.65f,.06f),M.OakDark,.012f,Quaternion.Euler(-6,0,0));
            b.B(new Vector3(0,.95f,.255f),new Vector3(.54f,.2f,.09f),M.OakLight,.025f,Quaternion.Euler(-6,0,0));
            b.B(new Vector3(0,.76f,.23f),new Vector3(.44f,.07f,.07f),M.Oak,.012f);
        }
        static void Fridge(Builder b)
        {
            b.B(new Vector3(0,.95f,0),new Vector3(.87f,1.84f,.79f),M.Ink,.065f);
            b.B(new Vector3(0,.99f,-.025f),new Vector3(.83f,1.76f,.75f),M.Cream,.055f);
            b.B(new Vector3(0,1.53f,-.413f),new Vector3(.79f,.61f,.075f),M.Linen,.031f);
            b.B(new Vector3(0,.66f,-.413f),new Vector3(.79f,1.06f,.075f),M.Linen,.035f);
            b.B(new Vector3(.285f,1.49f,-.48f),new Vector3(.035f,.32f,.05f),M.Steel,.015f);
            b.B(new Vector3(.285f,.89f,-.48f),new Vector3(.035f,.39f,.05f),M.Steel,.015f);
            b.B(new Vector3(-.18f,1.58f,-.46f),new Vector3(.18f,.13f,.012f),M.Amber,.008f,Quaternion.Euler(0,0,-5));
            b.B(new Vector3(-.13f,1.4f,-.46f),new Vector3(.23f,.13f,.012f),M.Blue,.008f,Quaternion.Euler(0,0,6));
            for(int i=0;i<5;i++) b.B(new Vector3(-.29f+i*.145f,.115f,-.42f),new Vector3(.075f,.027f,.016f),M.Ink,.005f);
            b.C(new Vector3(-.32f,.06f,-.28f),.045f,.12f,M.Ink,12);
            b.C(new Vector3(.32f,.06f,-.28f),.045f,.12f,M.Ink,12);
            b.B(new Vector3(0,1.91f,.05f),new Vector3(.61f,.09f,.52f),M.Oak,.015f);
        }
        static void Cabinet(Builder b)
        {
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) b.B(new Vector3(x*.55f,.1f,z*.185f),new Vector3(.075f,.2f,.075f),M.Ink,.015f);
            b.B(new Vector3(0,.62f,0),new Vector3(1.32f,.94f,.51f),M.Teal,.03f);
            b.B(new Vector3(0,1.12f,0),new Vector3(1.4f,.09f,.58f),M.OakLight,.024f);
            for(int x=-1;x<=1;x+=2)
            {
                b.B(new Vector3(x*.32f,.64f,-.274f),new Vector3(.6f,.83f,.055f),M.Sage,.015f);
                b.B(new Vector3(x*.32f,.64f,-.309f),new Vector3(.45f,.65f,.018f),M.Teal,.012f);
                b.C(new Vector3(x*.09f,.7f,-.338f),.024f,.04f,M.Amber,12,Quaternion.Euler(90,0,0));
            }
            Plant(b,new Vector3(-.42f,1.167f,0),.36f);
            b.B(new Vector3(.26f,1.18f,.06f),new Vector3(.36f,.03f,.25f),M.Orange,.006f);
            b.B(new Vector3(.29f,1.212f,.05f),new Vector3(.31f,.03f,.23f),M.Cream,.006f,Quaternion.Euler(0,-7,0));
        }
        static void Bed(Builder b)
        {
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) b.B(new Vector3(x*.68f,.13f,z*.9f),new Vector3(.12f,.26f,.12f),M.OakDark,.025f);
            b.B(new Vector3(0,.29f,0),new Vector3(1.58f,.3f,2.15f),M.Oak,.04f);
            b.B(new Vector3(0,.9f,.99f),new Vector3(1.64f,1.03f,.12f),M.OakDark,.035f);
            b.B(new Vector3(0,.91f,.914f),new Vector3(1.46f,.72f,.045f),M.Sage,.03f);
            b.B(new Vector3(0,.56f,-.01f),new Vector3(1.51f,.28f,2.02f),M.Linen,.08f);
            b.B(new Vector3(0,.708f,-.3f),new Vector3(1.53f,.035f,1.35f),M.Teal,.015f);
            b.B(new Vector3(0,.64f,-1.01f),new Vector3(1.52f,.22f,.07f),M.Teal,.022f);
            for(int i=0;i<5;i++) b.B(new Vector3(-.57f+i*.285f,.731f,-.31f),new Vector3(.025f,.005f,1.32f),M.Sage,.001f);
            for(int x=-1;x<=1;x+=2) b.B(new Vector3(x*.38f,.765f,.61f),new Vector3(.65f,.19f,.44f),M.Cream,.075f,Quaternion.Euler(0,x*4,0));
            b.B(new Vector3(0,.745f,-.62f),new Vector3(1.54f,.035f,.39f),M.Orange,.015f);
        }
        static void Plant(Builder b,Vector3 p,float s)
        {
            b.C(p+new Vector3(0,.19f*s,0),.22f*s,.38f*s,M.Orange,16);
            b.C(p+new Vector3(0,.36f*s,0),.25f*s,.09f*s,M.Earth,16);
            b.C(p+new Vector3(0,.408f*s,0),.212f*s,.012f*s,M.OakDark,16);
            b.C(p+new Vector3(0,.12f*s,0),.224f*s,.035f*s,M.Cream,16);
            b.C(p+new Vector3(0,.76f*s,0),.018f*s,.73f*s,M.OakDark,8);
            for(int i=0;i<10;i++)
            {
                float angle=i*137.5f;
                float y=.55f+(i%5)*.135f;
                Vector3 direction=Quaternion.Euler(0,angle,0)*new Vector3(.28f,.14f,0);
                b.Beam(p+new Vector3(0,y*s,0),p+(new Vector3(0,y*s,0)+direction*s),.012f*s,M.Leaf);
                b.Leaf(p+(new Vector3(0,y,0)+direction)*s,new Vector3(.22f,.045f,.12f)*s,i%3==0?M.Sage:M.Leaf,Quaternion.Euler(0,angle,i%2==0?22:-18));
            }
        }
        static void TvStand(Builder b)
        {
            for(int side=-1;side<=1;side+=2) b.B(new Vector3(side*.64f,.12f,0),new Vector3(.085f,.24f,.36f),M.Ink,.015f);
            b.B(new Vector3(0,.4f,0),new Vector3(1.64f,.47f,.49f),M.Oak,.025f);
            b.B(new Vector3(0,.425f,-.253f),new Vector3(1.49f,.32f,.02f),M.OakDark,.005f);
            for(int side=-1;side<=1;side+=2)
            {
                b.B(new Vector3(side*.55f,.43f,-.27f),new Vector3(.38f,.32f,.028f),M.Sage,.008f);
                b.C(new Vector3(side*.55f,.45f,-.294f),.018f,.025f,M.Amber,10,Quaternion.Euler(90,0,0));
            }
            b.B(new Vector3(0,.64f,0),new Vector3(1.7f,.075f,.54f),M.OakLight,.018f);
            b.B(new Vector3(0,1.17f,.03f),new Vector3(1.33f,.81f,.09f),M.Ink,.027f);
            b.B(new Vector3(0,1.18f,-.023f),new Vector3(1.23f,.7f,.018f),M.Black,.012f);
            b.B(new Vector3(-.19f,1.36f,-.035f),new Vector3(.67f,.018f,.005f),M.Steel,.001f,Quaternion.Euler(0,0,-20));
            b.C(new Vector3(.57f,.815f,-.035f),.012f,.006f,M.Glow,10,Quaternion.Euler(90,0,0));
            for(int side=-1;side<=1;side+=2) b.Beam(new Vector3(side*.39f,.69f,-.06f),new Vector3(side*.29f,.79f,.03f),.025f,M.Ink);
            b.B(new Vector3(0,.35f,-.27f),new Vector3(.37f,.06f,.17f),M.Ink,.008f);
            b.B(new Vector3(.53f,.694f,-.13f),new Vector3(.07f,.016f,.21f),M.Ink,.007f,Quaternion.Euler(0,-15,0));
        }
        static void BagStation(Builder b)
        {
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) b.B(new Vector3(x*.72f,.39f,z*.25f),new Vector3(.075f,.78f,.075f),M.Ink,.014f);
            b.B(new Vector3(0,.2f,0),new Vector3(1.5f,.07f,.58f),M.Oak,.018f);
            b.B(new Vector3(0,.82f,0),new Vector3(1.74f,.11f,.76f),M.OakLight,.028f);
            for(int i=0;i<4;i++) Water(b,new Vector3(-.59f+i*.16f,.24f,.1f),.48f);
            Food(b,new Vector3(.38f,.24f,.02f),.8f);
            // Expedition pack: front pouches, webbing, carry handle and clasp.
            b.B(new Vector3(-.25f,1.15f,.04f),new Vector3(.58f,.57f,.37f),M.Amber,.085f);
            b.B(new Vector3(-.25f,1.34f,-.02f),new Vector3(.57f,.18f,.39f),M.Orange,.06f);
            b.B(new Vector3(-.25f,1.04f,-.174f),new Vector3(.41f,.25f,.105f),M.Orange,.035f);
            for(int side=-1;side<=1;side+=2)
            {
                b.B(new Vector3(-.25f+side*.18f,1.15f,-.223f),new Vector3(.045f,.47f,.022f),M.OakDark,.006f);
                b.B(new Vector3(-.25f+side*.18f,1.13f,-.243f),new Vector3(.07f,.065f,.015f),M.Steel,.006f);
            }
            b.Beam(new Vector3(-.38f,1.44f,.04f),new Vector3(-.38f,1.5f,.04f),.04f,M.OakDark);
            b.Beam(new Vector3(-.12f,1.44f,.04f),new Vector3(-.12f,1.5f,.04f),.04f,M.OakDark);
            b.B(new Vector3(-.25f,1.5f,.04f),new Vector3(.3f,.04f,.04f),M.OakDark,.01f);
            FirstAid(b,new Vector3(.5f,.88f,.08f),.65f);
            Flashlight(b,new Vector3(.33f,.9f,-.21f),.6f);
            b.B(new Vector3(.63f,.89f,-.17f),new Vector3(.18f,.02f,.25f),M.Cream,.005f,Quaternion.Euler(0,-12,0));
        }
        static void Bunker(Builder b)
        {
            b.B(new Vector3(0,.045f,0),new Vector3(1.86f,.09f,1.66f),M.Ink,.065f);
            b.B(new Vector3(0,.115f,0),new Vector3(1.72f,.12f,1.52f),M.Amber,.035f);
            b.B(new Vector3(0,.17f,0),new Vector3(1.45f,.08f,1.25f),M.Teal,.032f);
            b.B(new Vector3(0,.217f,0),new Vector3(1.25f,.035f,1.05f),M.Steel,.015f);
            for(int side=-1;side<=1;side+=2)
            {
                b.B(new Vector3(side*.74f,.195f,0),new Vector3(.12f,.04f,1.4f),M.Ink,.012f);
                for(int i=0;i<7;i++) b.B(new Vector3(side*.74f,.22f,-.56f+i*.185f),new Vector3(.13f,.012f,.07f),M.Amber,.003f,Quaternion.Euler(0,-35,0));
                b.B(new Vector3(side*.5f,.26f,.57f),new Vector3(.2f,.12f,.12f),M.Teal,.02f);
                b.C(new Vector3(side*.5f,.26f,.57f),.027f,.23f,M.Steel,12,Quaternion.Euler(0,0,90));
            }
            b.C(new Vector3(0,.263f,-.05f),.2f,.035f,M.Ink,24);
            b.C(new Vector3(0,.287f,-.05f),.157f,.016f,M.Steel,24);
            for(int i=0;i<3;i++) b.Beam(new Vector3(0,.303f,-.05f),new Vector3(Mathf.Cos(i*2.094f)*.22f,.303f,-.05f+Mathf.Sin(i*2.094f)*.22f),.026f,M.Teal);
            b.C(new Vector3(0,.305f,-.05f),.045f,.035f,M.Amber,12);
            b.B(new Vector3(0,.25f,-.43f),new Vector3(.39f,.015f,.12f),M.Teal,.01f);
            for(int i=0;i<5;i++) b.B(new Vector3(-.15f+i*.075f,.261f,-.43f),new Vector3(.025f,.008f,.075f),M.Glow,.003f);
        }
        static void Wall(Builder b)
        {
            b.B(new Vector3(0,1.39f,0),new Vector3(10,2.78f,.18f),M.Plaster,.025f);
            b.B(new Vector3(0,.38f,-.105f),new Vector3(10,.71f,.055f),M.Sage,.012f);
            b.B(new Vector3(0,.77f,-.13f),new Vector3(10,.075f,.09f),M.Cream,.015f);
            b.B(new Vector3(0,2.76f,0),new Vector3(10.06f,.1f,.23f),M.OakDark,.025f);
            b.B(new Vector3(0,2.65f,-.113f),new Vector3(10,.075f,.07f),M.Cream,.015f);
            b.B(new Vector3(0,.075f,-.13f),new Vector3(10,.15f,.08f),M.Cream,.014f);
            for(int i=0;i<17;i++) b.B(new Vector3(-4.8f+i*.6f,.4f,-.138f),new Vector3(.025f,.6f,.018f),M.Teal,.003f);
            for(int side=-1;side<=1;side+=2) b.B(new Vector3(side*4.95f,1.4f,0),new Vector3(.1f,2.8f,.24f),M.Cream,.018f);
        }
        static void Window(Builder b)
        {
            b.B(new Vector3(0,1.65f,0),new Vector3(1.65f,1.46f,.16f),M.Ink,.02f);
            b.B(new Vector3(0,1.65f,-.094f),new Vector3(1.45f,1.26f,.025f),M.Glass,.01f);
            for(int side=-1;side<=1;side+=2) b.B(new Vector3(side*.77f,1.65f,-.13f),new Vector3(.13f,1.48f,.14f),M.Cream,.015f);
            for(int side=-1;side<=1;side+=2) b.B(new Vector3(0,1.65f+side*.67f,-.13f),new Vector3(1.66f,.13f,.14f),M.Cream,.015f);
            b.B(new Vector3(0,1.65f,-.13f),new Vector3(.055f,1.26f,.07f),M.Cream,.008f);
            b.B(new Vector3(0,1.65f,-.13f),new Vector3(1.47f,.055f,.07f),M.Cream,.008f);
            b.B(new Vector3(0,.93f,-.2f),new Vector3(1.8f,.1f,.4f),M.OakLight,.02f);
            b.B(new Vector3(0,2.45f,-.16f),new Vector3(2.05f,.045f,.045f),M.Ink,.01f);
            for(int side=-1;side<=1;side+=2)
            {
                b.C(new Vector3(side*1.025f,2.45f,-.16f),.05f,.075f,M.Amber,12,Quaternion.Euler(0,0,90));
                for(int i=0;i<4;i++) b.B(new Vector3(side*(.78f+i*.055f),1.72f,-.23f-i%2*.022f),new Vector3(.075f,1.4f,.06f),i%2==0?M.Linen:M.Cream,.02f);
            }
            b.B(new Vector3(-.36f,1.94f,-.112f),new Vector3(.28f,.028f,.008f),M.White,.001f,Quaternion.Euler(0,0,32));
            b.B(new Vector3(.35f,1.32f,-.112f),new Vector3(.2f,.018f,.008f),M.White,.001f,Quaternion.Euler(0,0,32));
        }
        static void Door(Builder b)
        {
            for(int side=-1;side<=1;side+=2) b.B(new Vector3(side*.6f,1.15f,0),new Vector3(.13f,2.3f,.21f),M.OakDark,.025f);
            b.B(new Vector3(0,2.25f,0),new Vector3(1.33f,.13f,.21f),M.OakLight,.02f);
            b.B(new Vector3(0,1.09f,0),new Vector3(1.08f,2.18f,.12f),M.Teal,.025f);
            for(int row=0;row<2;row++) b.B(new Vector3(0,.58f+row*.98f,-.075f),new Vector3(.84f,.71f,.035f),M.Sage,.015f);
            for(int row=0;row<2;row++) b.B(new Vector3(0,.58f+row*.98f,-.098f),new Vector3(.69f,.56f,.018f),M.Teal,.012f);
            b.B(new Vector3(.39f,1.07f,-.096f),new Vector3(.08f,.21f,.035f),M.Amber,.014f);
            b.B(new Vector3(.32f,1.105f,-.145f),new Vector3(.18f,.035f,.04f),M.Steel,.012f);
            b.B(new Vector3(0,.035f,-.02f),new Vector3(1.2f,.07f,.3f),M.Steel,.018f);
            for(int i=0;i<3;i++) b.B(new Vector3(-.55f,.35f+i*.75f,-.045f),new Vector3(.045f,.13f,.08f),M.Amber,.008f);
            b.C(new Vector3(0,1.9f,-.078f),.022f,.018f,M.Steel,12,Quaternion.Euler(90,0,0));
        }
        static void Roof(Builder b)
        {
            // Selectable edge sample rather than an opaque lid over the playable room.
            b.B(new Vector3(0,.08f,0),new Vector3(2.2f,.16f,.72f),M.Ink,.03f);
            for(int i=0;i<8;i++)
                for(int row=0;row<2;row++) b.B(new Vector3(-.96f+i*.275f,.19f+row*.04f,-.17f+row*.3f),new Vector3(.29f,.08f,.38f),i%3==0?M.Orange:M.Earth,.024f,Quaternion.Euler(-9,0,0));
            b.B(new Vector3(0,.08f,-.4f),new Vector3(2.3f,.15f,.1f),M.Cream,.02f);
            b.B(new Vector3(0,.13f,-.47f),new Vector3(2.32f,.07f,.09f),M.Steel,.025f);
        }
        static void FireUnit(Builder b,Vector3 p,float s)
        {
            b.C(p+new Vector3(0,.29f*s,0),.105f*s,.48f*s,M.Red,20);
            b.C(p+new Vector3(0,.065f*s,0),.107f*s,.07f*s,M.Ink,20);
            b.C(p+new Vector3(0,.53f*s,0),.047f*s,.08f*s,M.Steel,12);
            b.B(p+new Vector3(.04f*s,.59f*s,0),new Vector3(.19f,.045f,.065f)*s,M.Ink,.01f*s);
            b.B(p+new Vector3(0,.32f*s,-.107f*s),new Vector3(.13f,.18f,.012f)*s,M.Cream,.004f*s);
            b.B(p+new Vector3(0,.32f*s,-.117f*s),new Vector3(.025f,.12f,.008f)*s,M.Red,.001f*s);
            b.C(p+new Vector3(-.06f*s,.53f*s,-.04f*s),.039f*s,.025f*s,M.Cream,16,Quaternion.Euler(90,0,0));
            b.Beam(p+new Vector3(.09f*s,.53f*s,0),p+new Vector3(.16f*s,.15f*s,-.01f*s),.024f*s,M.Ink);
        }
        static void Water(Builder b,Vector3 p,float s)
        {
            b.B(p+new Vector3(0,.22f*s,0),new Vector3(.24f,.4f,.2f)*s,M.Blue,.045f*s);
            b.B(p+new Vector3(0,.22f*s,-.105f*s),new Vector3(.19f,.12f,.012f)*s,M.Cream,.004f*s);
            b.C(p+new Vector3(0,.46f*s,0),.055f*s,.09f*s,M.Teal,12);
            b.B(p+new Vector3(0,.422f*s,0),new Vector3(.19f,.045f,.15f)*s,M.Blue,.018f*s);
            b.B(p+new Vector3(0,.49f*s,.06f*s),new Vector3(.16f,.035f,.035f)*s,M.Cream,.01f*s);
            b.B(p+new Vector3(0,.225f*s,-.115f*s),new Vector3(.04f,.07f,.008f)*s,M.Blue,.012f*s,Quaternion.Euler(0,0,45));
        }
        static void Food(Builder b,Vector3 p,float s)
        {
            b.B(p+new Vector3(0,.12f*s,0),new Vector3(.39f,.24f,.28f)*s,M.OakLight,.018f*s);
            b.B(p+new Vector3(0,.245f*s,0),new Vector3(.41f,.025f,.3f)*s,M.Oak,.008f*s);
            b.B(p+new Vector3(0,.13f*s,-.146f*s),new Vector3(.26f,.13f,.012f)*s,M.Cream,.004f*s);
            b.B(p+new Vector3(0,.13f*s,-.156f*s),new Vector3(.13f,.025f,.008f)*s,M.Orange,.002f*s);
            for(int i=0;i<2;i++)
            {
                b.C(p+new Vector3(-.1f*s+i*.19f*s,.34f*s,0),.065f*s,.16f*s,i==0?M.Orange:M.Sage,16);
                b.C(p+new Vector3(-.1f*s+i*.19f*s,.425f*s,0),.066f*s,.012f*s,M.Steel,16);
                b.C(p+new Vector3(-.1f*s+i*.19f*s,.265f*s,0),.066f*s,.012f*s,M.Steel,16);
            }
        }
        static void FirstAid(Builder b,Vector3 p,float s)
        {
            b.B(p+new Vector3(0,.16f*s,0),new Vector3(.43f,.3f,.21f)*s,M.Red,.035f*s);
            b.B(p+new Vector3(0,.16f*s,-.115f*s),new Vector3(.065f,.19f,.018f)*s,M.Cream,.004f*s);
            b.B(p+new Vector3(0,.16f*s,-.115f*s),new Vector3(.19f,.065f,.018f)*s,M.Cream,.004f*s);
            b.B(p+new Vector3(0,.34f*s,0),new Vector3(.2f,.035f,.04f)*s,M.Cream,.01f*s);
            for(int x=-1;x<=1;x+=2) b.B(p+new Vector3(x*.09f*s,.31f*s,0),new Vector3(.035f,.09f,.04f)*s,M.Cream,.006f*s);
        }
        static void Flashlight(Builder b,Vector3 p,float s)
        {
            Quaternion rot=Quaternion.Euler(90,0,0);
            b.C(p+new Vector3(0,.09f*s,0),.06f*s,.32f*s,M.Amber,12,rot);
            b.C(p+new Vector3(0,.09f*s,-.18f*s),.09f*s,.09f*s,M.Ink,16,rot);
            b.C(p+new Vector3(0,.09f*s,-.231f*s),.071f*s,.012f*s,M.Glow,16,rot);
            for(int i=0;i<4;i++) b.C(p+new Vector3(0,.09f*s,-.09f*s+i*.05f*s),.063f*s,.012f*s,M.Ink,12,rot);
            b.B(p+new Vector3(0,.156f*s,.06f*s),new Vector3(.04f,.015f,.055f)*s,M.Orange,.004f*s);
        }
        static void Radio(Builder b,Vector3 p,float s)
        {
            b.B(p+new Vector3(0,.18f*s,0),new Vector3(.46f,.31f,.15f)*s,M.Teal,.025f*s);
            b.C(p+new Vector3(-.095f*s,.17f*s,-.085f*s),.105f*s,.012f*s,M.Ink,24,Quaternion.Euler(90,0,0));
            for(int i=0;i<7;i++) b.B(p+new Vector3(-.095f*s,.095f*s+i*.024f*s,-.098f*s),new Vector3(.15f,.006f,.008f)*s,M.Steel,.001f*s);
            b.B(p+new Vector3(.112f*s,.24f*s,-.084f*s),new Vector3(.14f,.07f,.012f)*s,M.Glass,.004f*s);
            for(int i=0;i<2;i++) b.C(p+new Vector3((.075f+i*.085f)*s,.11f*s,-.092f*s),.025f*s,.022f*s,M.Amber,12,Quaternion.Euler(90,0,0));
            b.Beam(p+new Vector3(.15f*s,.34f*s,.02f*s),p+new Vector3(.23f*s,.71f*s,.02f*s),.012f*s,M.Steel);
            b.B(p+new Vector3(-.04f*s,.37f*s,0),new Vector3(.26f,.035f,.035f)*s,M.Ink,.008f*s);
            for(int side=-1;side<=1;side+=2) b.B(p+new Vector3((-.04f+side*.12f)*s,.34f*s,0),new Vector3(.035f,.1f,.035f)*s,M.Ink,.007f*s);
        }
        static void Blanket(Builder b,Vector3 p,float s)
        {
            b.B(p+new Vector3(0,.07f*s,0),new Vector3(.52f,.14f,.36f)*s,M.Orange,.045f*s);
            b.B(p+new Vector3(0,.15f*s,0),new Vector3(.51f,.07f,.35f)*s,M.Amber,.025f*s);
            for(int side=-1;side<=1;side+=2) b.B(p+new Vector3(side*.16f*s,.1f*s,-.01f*s),new Vector3(.045f,.19f,.38f)*s,M.OakDark,.01f*s);
            for(int i=0;i<4;i++) b.B(p+new Vector3(0,.048f*s+i*.025f*s,-.184f*s),new Vector3(.42f,.006f,.01f)*s,M.Linen,.001f*s);
        }
        static void Mug(Builder b,Vector3 p,M m)
        {
            b.C(p+new Vector3(0,.075f,0),.063f,.15f,m,16);
            b.C(p+new Vector3(0,.153f,0),.052f,.008f,M.OakDark,16);
            b.B(p+new Vector3(.073f,.087f,0),new Vector3(.06f,.075f,.035f),m,.014f);
        }

        static void EnsureMaterials()
        {
            if(Materials!=null&&Materials.Length>0&&Materials[0]) return;
            Color32[] colors={
                new Color32(38,49,48,255),new Color32(172,119,69,255),new Color32(205,157,99,255),new Color32(104,73,50,255),
                new Color32(241,230,199,255),new Color32(219,212,188,255),new Color32(37,88,78,255),new Color32(112,149,120,255),
                new Color32(230,172,54,255),new Color32(191,91,49,255),new Color32(131,150,143,255),new Color32(111,174,183,255),
                new Color32(217,211,188,255),new Color32(176,59,45,255),new Color32(68,129,163,255),new Color32(58,110,68,255),
                new Color32(133,79,53,255),new Color32(245,242,222,255),new Color32(25,35,36,255),new Color32(185,224,167,255)
            };
            Materials=new Material[colors.Length];
            Shader shader=Shader.Find("Standard");
            if(!shader) shader=Shader.Find("Legacy Shaders/Diffuse");
            for(int i=0;i<colors.Length;i++)
            {
                var m=new Material(shader){name="Panic_"+(M)i,color=colors[i]};
                if(m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness",i==(int)M.Steel?.54f:(i==(int)M.Glass?.58f:.19f));
                if(m.HasProperty("_Metallic")) m.SetFloat("_Metallic",i==(int)M.Steel?.58f:.0f);
                m.EnableKeyword("_EMISSION");
                if(m.HasProperty(EmissionId)) m.SetColor(EmissionId,i==(int)M.Glow?(Color)colors[i]*.32f:Color.black);
                Materials[i]=m;
            }
        }
        static void Attach(GameObject parent,List<Part> parts)
        {
            foreach(var part in parts)
            {
                var child=new GameObject(((M)part.material).ToString());
                child.transform.SetParent(parent.transform,false);
                child.AddComponent<MeshFilter>().sharedMesh=part.mesh;
                var r=child.AddComponent<MeshRenderer>();
                r.sharedMaterial=Materials[part.material];
                r.shadowCastingMode=ShadowCastingMode.On;
                r.receiveShadows=true;
            }
        }
        static Bounds LocalBounds(GameObject geometry)
        {
            var filters=geometry.GetComponentsInChildren<MeshFilter>();
            Bounds bounds=new Bounds(Vector3.zero,Vector3.zero);
            bool first=true;
            foreach(var f in filters)
            {
                if(!f.sharedMesh)continue;
                if(first){bounds=f.sharedMesh.bounds;first=false;}else bounds.Encapsulate(f.sharedMesh.bounds);
            }
            return bounds;
        }
        sealed class Builder
        {
            readonly Dictionary<int,List<CombineInstance>> groups=new Dictionary<int,List<CombineInstance>>();
            void Add(M material,Mesh mesh,Vector3 p,Quaternion rotation,Vector3 scale)
            {
                int index=(int)material;
                List<CombineInstance> list;
                if(!groups.TryGetValue(index,out list)){list=new List<CombineInstance>();groups[index]=list;}
                list.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,rotation,scale)});
            }
            public void B(Vector3 p,Vector3 size,M material,float bevel=.02f,Quaternion rotation=default(Quaternion))
            {
                if(rotation.Equals(default(Quaternion)))rotation=Quaternion.identity;
                string key="b"+size.ToString("F4")+bevel.ToString("F4",System.Globalization.CultureInfo.InvariantCulture);
                Mesh mesh;
                if(!MeshCache.TryGetValue(key,out mesh)){mesh=BevelBox(size,bevel);MeshCache[key]=mesh;}
                Add(material,mesh,p,rotation,Vector3.one);
            }
            public void C(Vector3 p,float radius,float height,M material,int segments=16,Quaternion rotation=default(Quaternion))
            {
                if(rotation.Equals(default(Quaternion)))rotation=Quaternion.identity;
                string key="c"+segments;
                Mesh mesh;
                if(!MeshCache.TryGetValue(key,out mesh)){mesh=Cylinder(segments);MeshCache[key]=mesh;}
                Add(material,mesh,p,rotation,new Vector3(radius,height,radius));
            }
            public void Beam(Vector3 a,Vector3 z,float width,M material)
            {
                Vector3 delta=z-a;
                B((a+z)*.5f,new Vector3(width,delta.magnitude,width),material,width*.2f,Quaternion.FromToRotation(Vector3.up,delta.normalized));
            }
            public void Leaf(Vector3 p,Vector3 size,M material,Quaternion rotation)
            {
                Mesh mesh;
                if(!MeshCache.TryGetValue("leaf",out mesh)){mesh=LeafMesh();MeshCache["leaf"]=mesh;}
                Add(material,mesh,p,rotation,size);
            }
            public List<Part> Finish(string name)
            {
                var parts=new List<Part>();
                foreach(var pair in groups)
                {
                    var mesh=new Mesh{name="Panic_"+name+"_"+(M)pair.Key};
                    int vertexCount=0;
                    foreach(var c in pair.Value)vertexCount+=c.mesh.vertexCount;
                    if(vertexCount>65000)mesh.indexFormat=IndexFormat.UInt32;
                    mesh.CombineMeshes(pair.Value.ToArray(),true,true,false);
                    mesh.RecalculateBounds();
                    parts.Add(new Part{mesh=mesh,material=pair.Key});
                }
                return parts;
            }
        }

        static Mesh BevelBox(Vector3 size,float radius)
        {
            Vector3 h=size*.5f;
            float r=Mathf.Clamp(radius,.0001f,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.75f);
            Vector3 inset=h-Vector3.one*r;
            var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
            Action<Vector3[],Vector3> face=(points,normal)=>
            {
                int start=vertices.Count;
                for(int i=0;i<points.Length;i++){vertices.Add(points[i]);uv.Add(new Vector2(points[i].x+points[i].z,points[i].y+points[i].z));}
                bool forward=Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),normal)>0;
                for(int i=1;i<points.Length-1;i++)
                {
                    triangles.Add(start);triangles.Add(start+(forward?i:i+1));triangles.Add(start+(forward?i+1:i));
                }
            };
            // Six broad faces.
            for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2)
            {
                int a=(axis+1)%3,c=(axis+2)%3;
                Vector3[] points=new Vector3[4];
                int[] sa={-1,1,1,-1},sc={-1,-1,1,1};
                for(int i=0;i<4;i++){Vector3 v=Vector3.zero;v[axis]=h[axis]*sign;v[a]=inset[a]*sa[i];v[c]=inset[c]*sc[i];points[i]=v;}
                Vector3 n=Vector3.zero;n[axis]=sign;face(points,n);
            }
            // Twelve edge chamfers.
            for(int axis=0;axis<3;axis++)for(int sa=-1;sa<=1;sa+=2)for(int sc=-1;sc<=1;sc+=2)
            {
                int a=(axis+1)%3,c=(axis+2)%3;
                Vector3 p0=Vector3.zero,p1=Vector3.zero,p2=Vector3.zero,p3=Vector3.zero;
                p0[axis]=-inset[axis];p1[axis]=inset[axis];p2[axis]=inset[axis];p3[axis]=-inset[axis];
                p0[a]=p1[a]=h[a]*sa;p0[c]=p1[c]=inset[c]*sc;
                p2[a]=p3[a]=inset[a]*sa;p2[c]=p3[c]=h[c]*sc;
                Vector3 n=Vector3.zero;n[a]=sa;n[c]=sc;face(new[]{p0,p1,p2,p3},n);
            }
            // Eight trihedral corner caps.
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                face(new[]{new Vector3(h.x*x,inset.y*y,inset.z*z),new Vector3(inset.x*x,h.y*y,inset.z*z),new Vector3(inset.x*x,inset.y*y,h.z*z)},new Vector3(x,y,z));
            var mesh=new Mesh{name="ChamferedPanel"};
            mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
            return mesh;
        }
        static Mesh Cylinder(int segments)
        {
            var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();
            for(int i=0;i<segments;i++)
            {
                float a=i*Mathf.PI*2/segments,c=(i+1)*Mathf.PI*2/segments;
                int k=v.Count;
                v.Add(new Vector3(Mathf.Cos(a),-.5f,Mathf.Sin(a)));v.Add(new Vector3(Mathf.Cos(c),-.5f,Mathf.Sin(c)));
                v.Add(new Vector3(Mathf.Cos(c),.5f,Mathf.Sin(c)));v.Add(new Vector3(Mathf.Cos(a),.5f,Mathf.Sin(a)));
                t.AddRange(new[]{k,k+2,k+1,k,k+3,k+2});
                for(int j=0;j<4;j++)uv.Add(new Vector2((float)(i+(j==1||j==2?1:0))/segments,j>=2?1:0));
                k=v.Count;
                v.Add(new Vector3(0,.5f,0));v.Add(new Vector3(Mathf.Cos(a),.5f,Mathf.Sin(a)));v.Add(new Vector3(Mathf.Cos(c),.5f,Mathf.Sin(c)));
                t.AddRange(new[]{k,k+2,k+1});uv.Add(new Vector2(.5f,.5f));uv.Add(Vector2.zero);uv.Add(Vector2.one);
                k=v.Count;
                v.Add(new Vector3(0,-.5f,0));v.Add(new Vector3(Mathf.Cos(a),-.5f,Mathf.Sin(a)));v.Add(new Vector3(Mathf.Cos(c),-.5f,Mathf.Sin(c)));
                t.AddRange(new[]{k,k+1,k+2});uv.Add(new Vector2(.5f,.5f));uv.Add(Vector2.zero);uv.Add(Vector2.one);
            }
            var mesh=new Mesh{name="FacetedCylinder_"+segments};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }
        static Mesh LeafMesh()
        {
            var mesh=new Mesh{name="BotanicalLeaf"};
            mesh.vertices=new[]{new Vector3(-.5f,0,0),new Vector3(0,.4f,0),new Vector3(0,0,.5f),new Vector3(.5f,0,0),new Vector3(0,0,-.5f),new Vector3(0,-.1f,0)};
            mesh.triangles=new[]{0,2,1,2,3,1,3,4,1,4,0,1,0,5,2,2,5,3,3,5,4,4,5,0};
            mesh.uv=new[]{Vector2.zero,new Vector2(.5f,.5f),Vector2.up,Vector2.one,Vector2.right,new Vector2(.5f,.5f)};
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }

        /// <summary>Mesh-only original disaster presentation. Caller owns placement, timing and Destroy.</summary>
        public static GameObject CreateDisasterVfx(DisasterType type)
        {
            EnsureMaterials();
            var root=new GameObject("DisasterVfx_"+type);
            var state=new VfxState{type=type,root=root};
            Effects[root.GetInstanceID()]=state;
            Mesh soft;
            if(!MeshCache.TryGetValue("vfx_soft",out soft)){soft=BevelBox(Vector3.one,.24f);MeshCache["vfx_soft"]=soft;}
            Mesh flame;
            if(!MeshCache.TryGetValue("vfx_flame",out flame)){flame=FlameMesh();MeshCache["vfx_flame"]=flame;}
            Material water=EffectMaterial("Water",new Color(.16f,.56f,.63f,.33f),true,.05f);
            Material smoke=EffectMaterial("Smoke",new Color(.23f,.27f,.25f,.16f),true,0);
            Material dust=EffectMaterial("Dust",new Color(.68f,.59f,.42f,.2f),true,0);
            Material fire=EffectMaterial("Fire",new Color(1,.39f,.055f,1),false,.85f);
            Material glow=EffectMaterial("FlameCore",new Color(1,.83f,.23f,1),false,.75f);
            if(type==DisasterType.Flood)
            {
                var waterMesh=VfxPart(root,"RisingWater",soft,water,new Vector3(0,.025f,0),new Vector3(9.8f,.035f,7.8f));
                state.water=waterMesh.transform;
                for(int i=0;i<32;i++)
                {
                    Vector3 p=new Vector3(-4.6f+(i*1.371f)%9.2f,.7f+(i*.37f)%2.5f,-3.5f+(i*2.17f)%7);
                    AddVfxParticle(state,VfxPart(root,"Rain_"+i,soft,water,p,new Vector3(.016f,.35f,.016f)));
                }
                for(int i=0;i<8;i++)
                {
                    Vector3 p=new Vector3(-4.3f+(i*1.47f)%8.6f,.07f,-3.3f+(i*2.41f)%6.6f);
                    AddVfxParticle(state,VfxPart(root,"SurfaceGlint_"+i,soft,EffectMaterial("Glint",new Color(.6f,.84f,.82f,.42f),true,.1f),p,new Vector3(.43f,.008f,.025f)));
                }
            }
            else if(type==DisasterType.Fire)
            {
                Vector3[] seats={new Vector3(-4.4f,0,1.7f),new Vector3(-2.6f,0,3.3f),new Vector3(1.7f,0,3.35f)};
                for(int seat=0;seat<seats.Length;seat++)
                {
                    for(int i=0;i<7;i++)
                    {
                        Vector3 p=seats[seat]+new Vector3((i%3-1)*.23f,.02f,(i/3-1)*.2f);
                        AddVfxParticle(state,VfxPart(root,"Flame_"+seat+"_"+i,flame,i%2==0?fire:glow,p,new Vector3(.56f,.8f+i%3*.25f,.56f)));
                    }
                    for(int i=0;i<4;i++)
                    {
                        Vector3 p=seats[seat]+new Vector3(i*.07f,1.1f+i*.4f,0);
                        AddVfxParticle(state,VfxPart(root,"Smoke_"+seat+"_"+i,soft,smoke,p,new Vector3(.53f+i*.13f,.6f+i*.1f,.53f+i*.13f)));
                    }
                }
            }
            else
            {
                for(int i=0;i<22;i++)
                {
                    bool back=i%2==0;
                    Vector3 p=back?new Vector3(-4.6f+(i*.77f)%9.2f,.18f,3.65f):new Vector3(-4.65f,.18f,-3.4f+(i*.93f)%6.8f);
                    AddVfxParticle(state,VfxPart(root,"Dust_"+i,soft,dust,p,new Vector3(.35f+i%3*.12f,.36f+i%4*.09f,.35f+i%3*.12f)));
                }
                for(int i=0;i<18;i++)
                {
                    Vector3 p=new Vector3(-4.55f+(i*1.37f)%9.1f,.08f,-3.6f+(i*2.31f)%7.2f);
                    AddVfxParticle(state,VfxPart(root,"PlasterChip_"+i,soft,Materials[(int)M.Plaster],p,new Vector3(.045f+i%3*.018f,.035f,.06f)));
                }
            }
            AnimateDisasterVfx(root,0,1);
            return root;
        }
        public static void AnimateDisasterVfx(GameObject effect,float time,float intensity=1)
        {
            if(!effect)return;
            VfxState state;if(!Effects.TryGetValue(effect.GetInstanceID(),out state))return;
            intensity=Mathf.Clamp01(intensity);
            if(state.water)
            {
                state.water.localPosition=new Vector3(0,.045f+intensity*.15f+Mathf.Sin(time*1.8f)*.012f,0);
                state.water.localScale=new Vector3(9.8f,.035f+intensity*.05f,7.8f);
            }
            for(int i=0;i<state.particles.Count;i++)
            {
                Transform particle=state.particles[i];if(!particle)continue;
                Vector3 origin=state.origins[i],scale=state.scales[i];
                float phase=time+i*.43f;
                if(state.type==DisasterType.Flood)
                {
                    if(particle.name.StartsWith("Rain",StringComparison.Ordinal))
                    {
                        particle.localPosition=new Vector3(origin.x,Mathf.Repeat(origin.y-time*3.4f,3.25f)+.08f,origin.z);
                        particle.localRotation=Quaternion.Euler(-8,0,-12);
                        particle.localScale=scale*Mathf.Max(.15f,intensity);
                    }
                    else
                    {
                        particle.localPosition=origin+new Vector3(Mathf.Sin(phase)*.12f,intensity*.15f,Mathf.Cos(phase*.7f)*.07f);
                        particle.localScale=Vector3.Scale(scale,new Vector3(.75f+Mathf.Sin(phase*2)*.25f,1,1));
                    }
                }
                else if(state.type==DisasterType.Fire)
                {
                    if(particle.name.StartsWith("Flame",StringComparison.Ordinal))
                    {
                        particle.localScale=Vector3.Scale(scale,new Vector3(.9f+Mathf.Sin(phase*8)*.1f,.7f+Mathf.Sin(phase*11)*.16f+intensity*.4f,.9f+Mathf.Sin(phase*7)*.1f))*Mathf.Max(.15f,intensity);
                        particle.localRotation=Quaternion.Euler(0,i*57+Mathf.Sin(phase*4)*12,Mathf.Sin(phase*9)*5);
                    }
                    else
                    {
                        float drift=Mathf.Repeat(phase*.35f,1);
                        particle.localPosition=origin+new Vector3(Mathf.Sin(phase*.8f)*.17f,drift*.5f,.1f*Mathf.Cos(phase));
                        particle.localScale=scale*(.8f+drift*.45f)*Mathf.Max(.1f,intensity);
                        particle.localRotation=Quaternion.Euler(i*19,phase*9,i*13);
                    }
                }
                else if(particle.name.StartsWith("Dust",StringComparison.Ordinal))
                {
                    particle.localPosition=origin+new Vector3(Mathf.Sin(phase*3)*.1f,Mathf.Repeat(phase*.22f,1)*.35f,Mathf.Cos(phase*2)*.06f);
                    particle.localScale=scale*(.8f+Mathf.Sin(phase*2)*.16f)*Mathf.Max(.1f,intensity);
                    particle.localRotation=Quaternion.Euler(i*13,phase*6,i*17);
                }
                else
                {
                    particle.localPosition=origin+new Vector3(Mathf.Sin(phase*13)*.015f,Mathf.Abs(Mathf.Sin(phase*11))*.035f*intensity,Mathf.Cos(phase*13)*.015f);
                    particle.localRotation=Quaternion.Euler(i*19,i*51,i*29);
                }
            }
        }
        static GameObject VfxPart(GameObject parent,string name,Mesh mesh,Material material,Vector3 p,Vector3 scale)
        {
            var g=new GameObject(name);g.transform.SetParent(parent.transform,false);g.transform.localPosition=p;g.transform.localScale=scale;
            g.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
            return g;
        }
        static void AddVfxParticle(VfxState state,GameObject particle)
        {
            state.particles.Add(particle.transform);state.origins.Add(particle.transform.localPosition);state.scales.Add(particle.transform.localScale);
        }
        static Material EffectMaterial(string key,Color color,bool transparent,float emission)
        {
            Material material;if(EffectMaterials.TryGetValue(key,out material)&&material)return material;
            material=new Material(Materials[0]){name="PanicVfx_"+key,color=color};
            material.SetColor(EmissionId,new Color(color.r,color.g,color.b,1)*emission);
            if(transparent)
            {
                material.SetFloat("_Mode",3);
                material.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);material.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);material.SetInt("_ZWrite",0);
                material.DisableKeyword("_ALPHATEST_ON");material.DisableKeyword("_ALPHAPREMULTIPLY_ON");material.EnableKeyword("_ALPHABLEND_ON");material.renderQueue=3000;
            }
            EffectMaterials[key]=material;return material;
        }
        static Mesh FlameMesh()
        {
            const int sides=8;
            float[] heights={0,.23f,.62f,1};float[] radii={.12f,.29f,.15f,0};
            var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
            for(int ring=0;ring<heights.Length;ring++)for(int i=0;i<sides;i++)
            {
                float angle=i*Mathf.PI*2/sides;
                vertices.Add(new Vector3(Mathf.Cos(angle)*radii[ring]+heights[ring]*heights[ring]*.13f,heights[ring],Mathf.Sin(angle)*radii[ring]));
                uv.Add(new Vector2((float)i/sides,heights[ring]));
            }
            for(int ring=0;ring<3;ring++)for(int i=0;i<sides;i++)
            {
                int a=ring*sides+i,b=ring*sides+(i+1)%sides,c=(ring+1)*sides+i,d=(ring+1)*sides+(i+1)%sides;
                triangles.AddRange(new[]{a,c,b,b,c,d});
            }
            var mesh=new Mesh{name="StylizedFlame"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }

        /// <summary>Original generated mono cues. Keys: click, upgrade, anchor, warning, success, fail, earthquake, flood, fire.</summary>
        public static AudioClip GetSound(string key)
        {
            key=(key??"click").ToLowerInvariant();
            AudioClip existing;if(Sounds.TryGetValue(key,out existing)&&existing)return existing;
            bool ambience=key=="earthquake"||key=="flood"||key=="fire"||key=="storm";
            float seconds=key=="storm"?6f:ambience?3f:(key=="success"?1f:(key=="fail"?.8f:(key=="warning"?.65f:.22f)));
            const int hz=22050;int count=Mathf.CeilToInt(seconds*hz);
            float[] samples=new float[count];var noise=new System.Random(709+key.Length*113);float low=0;
            for(int i=0;i<count;i++)
            {
                float time=(float)i/hz,t=time/seconds;
                float n=(float)noise.NextDouble()*2-1;low=Mathf.Lerp(low,n,.035f);
                float value;
                if(ambience)
                {
                    value=key=="earthquake"?low*2.8f+Mathf.Sin(time*57)*.15f:(key=="flood"?low*1.6f+n*.06f:n*.12f+low*.8f);
                    if(key=="storm")value=(low*2.4f+n*.055f)*(.62f+.3f*Mathf.Sin(time*1.3f)+.08f*Mathf.Sin(time*4.1f));
                    value*=Mathf.Min(1,time*4)*Mathf.Min(1,(seconds-time)*4);
                }
                else
                {
                    float freq=key=="warning"?880:(key=="fail"?220-100*t:(key=="success"?(t<.3f?523:(t<.6f?659:784)):(key=="upgrade"?420+480*t:(key=="anchor"?180:760))));
                    float env=Mathf.Exp(-t*(key=="success"?1.5f:6))*Mathf.Min(1,time*90);
                    value=(Mathf.Sin(2*Mathf.PI*freq*time)*.65f+Mathf.Sin(2*Mathf.PI*freq*2*time)*.12f)*env;
                    if(key=="anchor")value+=n*Mathf.Exp(-time*55)*.5f;
                    if(key=="warning")value*=Mathf.Sin(time*18)>0?1:.15f;
                }
                samples[i]=Mathf.Clamp(value*.24f,-.8f,.8f);
            }
            var clip=AudioClip.Create("Panic_"+key,count,1,hz,false);clip.SetData(samples,0);Sounds[key]=clip;return clip;
        }
    }
}
