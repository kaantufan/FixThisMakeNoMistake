using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>
    /// Original Home V3 meshes. Metres, grounded pivots, front -Z. No gameplay components.
    /// Architecture's solid_ mesh children are deliberately separate tight collision pieces.
    /// Furniture uses shared cached meshes/materials; hinges remain separate transforms.
    /// </summary>
    public static class PanicResidenceArt
    {
        enum M { Walnut, Honey, DarkWood, Cream, Olive, Leaf, Terracotta, Indigo, Teal,
            Linen, Mustard, Red, Steel, Chrome, Rubber, Ceramic, Glass, Glow, Screen, Concrete, Brick, Tile, Navy, Sky, Paper, Coral, Mint }
        static Material[] palette;
        static readonly Dictionary<string, Mesh> shapes = new Dictionary<string, Mesh>();
        static readonly Dictionary<string, Mesh> combined = new Dictionary<string, Mesh>();
        static readonly Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();

        /// <summary>Local floor y=0: place root y=-3.2. Central stair volume is kept clear.</summary>
        public static GameObject BuildBasement()
        {
            Init();
            var root = new GameObject("HomeV3_Basement");
            var deck = Group(root, "Floor"); var back = Group(root, "BackWalls");
            var low = Group(root, "FrontLowWalls"); var high = Group(root, "FrontHighWalls");
            Group(root, "Roof");
            Solid(deck, "basement_slab", V(0,-.13f,0), V(16,.24f,12), M.Concrete);
            Solid(back, "basement_north", V(0,1.6f,6), V(16,3.2f,.26f), M.Brick);
            Solid(back, "basement_west", V(-8,1.6f,0), V(.26f,3.2f,12), M.Brick);
            Solid(low, "basement_south_low", V(0,.25f,-6), V(16,.5f,.26f), M.Brick);
            Solid(high, "basement_south_high", V(0,1.85f,-6), V(16,2.7f,.26f), M.Brick);
            Solid(low, "basement_east_low", V(8,.25f,0), V(.26f,.5f,12), M.Brick);
            Solid(high, "basement_east_high", V(8,1.85f,0), V(.26f,2.7f,12), M.Brick);
            var b = new Builder();
            for (int x=0;x<20;x++) for(int z=0;z<15;z++)
                b.Box(V(-7.6f+x*.8f,-.002f,-5.6f+z*.8f),V(.786f,.02f,.786f), (x+z)%7==0?M.Tile:M.Concrete,.004f);
            // Brick joints and lower salt marks retain a distinct, useful cellar identity.
            for(int r=0;r<12;r++)
            {
                float y=.18f+r*.245f;
                b.Box(V(0,y,5.857f),V(15.72f,.014f,.012f),M.Concrete,.001f);
                for(int i=0;i<23;i++) b.Box(V(-7.7f+i*.7f+(r%2)*.32f,y+.12f,5.852f),V(.013f,.23f,.016f),M.Concrete,.001f);
            }
            // Copper/steel service pipes hug the back wall; nothing bridges the stair path.
            for(int i=0;i<2;i++)
            {
                b.Beam(V(-7.55f,2.62f+i*.16f,5.7f),V(7.5f,2.62f+i*.16f,5.7f),.045f,i==0?M.Terracotta:M.Steel);
                b.Beam(V(-7.55f,.2f,5.7f-i*.16f),V(-7.55f,2.65f,5.7f-i*.16f),.045f,M.Steel);
            }
            for(int i=0;i<7;i++) b.Box(V(-7.2f+i*2.4f,2.71f,5.67f),V(.1f,.43f,.045f),M.Steel,.006f);
            b.Box(V(-4.6f,.013f,-1.55f),V(3.4f,.022f,2.15f),M.Indigo,.016f);
            for(int i=0;i<13;i++) b.Box(V(-6.2f+i*.26f,.027f,-1.55f),V(.07f,.006f,2.06f),i%2==0?M.Linen:M.Teal,.001f);
            b.Attach(deck,"CellarTilesServices");
            Place(root,Create("walllamp"),V(-4.4f,1.6f,5.72f),0);
            Place(root,Create("walllamp"),V(4.8f,1.6f,5.72f),0);
            return root;
        }

        /// <summary>World-space y=3.2 deck, x[-5.5,1.5], z[-9,-6]. Place root at zero.</summary>
        public static GameObject BuildBalcony()
        {
            Init(); var root=new GameObject("HomeV3_Balcony");
            var floor=Group(root,"Floor"); var low=Group(root,"FrontLowWalls");
            Group(root,"FrontHighWalls"); Group(root,"Roof");
            Solid(floor,"balcony_deck",V(-2,3.08f,-7.5f),V(7,.22f,3),M.Concrete);
            // Open railing: narrow bars, never a single bounds box across the balcony's interior.
            Solid(low,"balcony_south_rail",V(-2,4.25f,-8.94f),V(6.95f,.065f,.07f),M.DarkWood);
            Solid(low,"balcony_west_rail",V(-5.46f,4.25f,-7.5f),V(.07f,.065f,2.9f),M.DarkWood);
            Solid(low,"balcony_east_rail",V(1.46f,4.25f,-7.5f),V(.07f,.065f,2.9f),M.DarkWood);
            for(int i=0;i<25;i++) Solid(low,"balcony_front_bar_"+i,V(-5.4f+i*.282f,3.73f,-8.94f),V(.032f,1.02f,.032f),M.Steel);
            for(int s=-1;s<=1;s+=2) for(int i=0;i<10;i++)
                Solid(low,"balcony_side_"+s+"_"+i,V(s<0?-5.46f:1.46f,3.73f,-8.8f+i*.29f),V(.032f,1.02f,.032f),M.Steel);
            var b=new Builder();
            for(int i=0;i<28;i++) b.Box(V(-5.375f+i*.25f,3.199f,-7.5f),V(.242f,.022f,2.95f),i%4==0?M.Honey:M.Walnut,.003f);
            for(int i=0;i<7;i++) b.Box(V(-5.2f+i,3.218f,-8.7f),V(.018f,.004f,.025f),M.Steel,.001f);
            for(int i=0;i<2;i++)
            {
                var p=V(-4.65f+i*2.1f,3.23f,-8.6f);
                b.Box(p+V(0,.22f,0),V(1.25f,.44f,.4f),M.Terracotta,.055f);
                b.Box(p+V(0,.45f,0),V(1.33f,.055f,.46f),M.Terracotta,.015f);
                for(int f=0;f<5;f++) Plant(b,p+V(-.45f+f*.22f,.44f,0),.25f,false);
            }
            b.Attach(floor,"DeckPlanters");
            Place(root,Create("balconychair"),V(-4.4f,3.22f,-6.8f),155);
            Place(root,Create("sidetable"),V(-2.85f,3.22f,-7),0);
            return root;
        }

        /// <summary>Upper bath localY0, x[2,8],z[1.8,6]. Root is placed y3.2.</summary>
        public static GameObject BuildBathroom()
        {
            Init(); var root=new GameObject("HomeV4_Bathroom");
            var floor=Group(root,"Floor"); var walls=Group(root,"Partitions");
            var high=Group(root,"FrontHighWalls"); Group(root,"Roof");
            // The only entrance now faces the upper room, away from the stair landing.
            // The west wall is continuous; tight south pieces leave x[2.65,3.85] clear.
            Solid(walls,"bath_west",V(2,.58f,3.9f),V(.13f,1.16f,4.2f),M.Cream);
            Solid(walls,"bath_south_left",V(2.325f,.58f,1.8f),V(.65f,1.16f,.13f),M.Cream);
            Solid(walls,"bath_south_right",V(5.925f,.58f,1.8f),V(4.15f,1.16f,.13f),M.Cream);
            Solid(high,"bath_south_jamb_left",V(2.615f,1.12f,1.8f),V(.07f,2.24f,.2f),M.DarkWood);
            Solid(high,"bath_south_jamb_right",V(3.885f,1.12f,1.8f),V(.07f,2.24f,.2f),M.DarkWood);
            var b=new Builder();
            // One continuous pale field absorbs the former x[2,3] strip, with no border at old x=3.
            b.Box(V(5,.009f,3.9f),V(6,.012f,4.2f),M.Paper,.002f);
            for(int x=0;x<16;x++) for(int z=0;z<11;z++)
                b.Box(V(2.22f+x*.36f,.018f,2.04f+z*.36f),V(.348f,.026f,.348f),
                    x==0||x==15||z==0||z==10?M.Mint:M.Paper,.003f);
            for(int x=0;x<22;x++) for(int y=0;y<4;y++)
            {
                float lo=2.02f+x*.272f,hi=lo+.26f;
                float leftHi=Mathf.Min(hi,2.65f),rightLo=Mathf.Max(lo,3.85f);
                if(leftHi-lo>.01f)b.Box(V((lo+leftHi)*.5f,.14f+y*.25f,1.878f),V(leftHi-lo,.237f,.018f),y==2?M.Teal:M.Ceramic,.002f);
                if(hi-rightLo>.01f)b.Box(V((rightLo+hi)*.5f,.14f+y*.25f,1.878f),V(hi-rightLo,.237f,.018f),y==2?M.Teal:M.Ceramic,.002f);
            }
            b.Box(V(2.325f,1.172f,1.8f),V(.65f,.045f,.16f),M.Honey,.006f);
            b.Box(V(5.925f,1.172f,1.8f),V(4.15f,.045f,.16f),M.Honey,.006f);
            b.Box(V(2,1.172f,3.9f),V(.16f,.045f,4.2f),M.Honey,.006f);
            // The bath mat belongs beside the tub, outside the door swing and main crossing path.
            b.Box(V(6.04f,.046f,4.2f),V(.48f,.024f,1.14f),M.Teal,.04f);
            b.Box(V(6.04f,.06f,4.2f),V(.4f,.004f,1.04f),M.Mint,.022f);
            for(int i=0;i<9;i++)b.Box(V(6.04f,.064f,3.75f+i*.112f),V(.35f,.004f,.014f),M.Linen,.001f);
            // A slim shower rail and toiletries stay against the east wall, above the tub.
            b.Beam(V(7.32f,.82f,4.9f),V(7.32f,2.22f,4.9f),.025f,M.Chrome);
            b.Beam(V(7.32f,2.22f,4.9f),V(7.05f,2.22f,4.9f),.025f,M.Chrome);
            b.Cylinder(V(7.05f,2.205f,4.9f),.065f,.025f,M.Chrome,20);
            b.Box(V(7.3f,1.18f,3.76f),V(.24f,.045f,.6f),M.Honey,.009f);
            Bottle(b,V(7.24f,1.205f,3.61f),.58f,M.Coral);
            Bottle(b,V(7.24f,1.205f,3.88f),.48f,M.Indigo);
            b.Attach(floor,"BathTilesMatAndShower");
            Place(root,Create("sink"),V(4.05f,.035f,5.05f),0);
            Place(root,Create("toilet"),V(5.35f,.035f,5f),0);
            Place(root,Create("bathtub"),V(6.88f,.035f,4.23f),90);
            Place(root,Create("towelrack"),V(5.2f,.04f,1.98f),180);
            return root;
        }

        /// <summary>World x8.2..15.2,z0..6.2, floor y0. Root zero. Garage leaf is a separate Create asset.</summary>
        public static GameObject BuildGarage()
        {
            Init(); var root=new GameObject("HomeV3_Garage");
            var floor=Group(root,"Floor"); var back=Group(root,"BackWalls");
            var low=Group(root,"FrontLowWalls"); var high=Group(root,"FrontHighWalls"); var roof=Group(root,"Roof");
            Solid(floor,"garage_slab",V(11.7f,-.11f,3.1f),V(7,.2f,6.2f),M.Concrete);
            Solid(back,"garage_north",V(11.7f,1.5f,6.2f),V(7,3,.22f),M.Cream);
            Solid(back,"garage_west",V(8.2f,1.5f,3.1f),V(.22f,3,6.2f),M.Cream);
            Solid(low,"garage_east_low",V(15.2f,.25f,3.1f),V(.22f,.5f,6.2f),M.Brick);
            Solid(high,"garage_east_high",V(15.2f,1.75f,3.1f),V(.22f,2.5f,6.2f),M.Cream);
            // Width3m aperture x10.2..13.2, lintel never crosses below y2.4.
            for(int side=0;side<2;side++)
            {
                float x=side==0?9.2f:14.2f;
                Solid(low,"garage_front_low_"+side,V(x,.25f,0),V(2,.5f,.22f),M.Brick);
                Solid(high,"garage_front_high_"+side,V(x,1.75f,0),V(2,2.5f,.22f),M.Cream);
            }
            Solid(high,"garage_front_lintel",V(11.7f,2.7f,0),V(3,.6f,.22f),M.Cream);
            Solid(roof,"garage_roof_deck",V(11.7f,3.06f,3.1f),V(7.36f,.16f,6.56f),M.DarkWood);
            var b=new Builder();
            for(int i=0;i<18;i++) b.Box(V(8.2f+i*.41f,3.161f,3.1f),V(.037f,.037f,6.56f),M.Steel,.005f);
            b.Box(V(11.7f,3.2f,-.17f),V(7.45f,.12f,.1f),M.Indigo,.012f); b.Attach(roof,"StandingSeamRoof");
            var d=new Builder();
            for(int i=0;i<7;i++) d.Box(V(8.7f+i, .004f,3.1f),V(.008f,.012f,6.05f),M.Steel,.001f);
            d.Box(V(11.7f,.008f,3.2f),V(2.5f,.012f,4.8f),M.Tile,.008f);
            for(int i=0;i<10;i++) d.Box(V(10.45f,.016f,1.05f+i*.48f),V(.065f,.009f,.21f),M.Mustard,.001f);
            // Garage track rails hang to each side of the car, not across its walking route.
            for(int s=-1;s<=1;s+=2)
            {
                d.Beam(V(11.7f+s*1.58f,.05f,.15f),V(11.7f+s*1.58f,2.58f,.15f),.055f,M.Steel);
                d.Beam(V(11.7f+s*1.58f,2.58f,.15f),V(11.7f+s*1.58f,2.58f,3.5f),.055f,M.Steel);
            }
            d.Attach(floor,"FloorMarkingsDoorTracks");
            Place(root,Create("walllamp"),V(9.2f,1.65f,6.04f),0);
            Place(root,Create("walllamp"),V(14.2f,1.65f,6.04f),0);
            return root;
        }

        public static GameObject Create(string id)
        {
            Init(); id=(id??"houseplant").ToLowerInvariant();
            var root=new GameObject("HomeV3_"+id); var b=new Builder();
            switch(id)
            {
                case "fireplace": Fireplace(b); break;
                case "computer": Computer(b); break;
                case "desk": Desk(root,b); break;
                case "bookshelf": Bookshelf(b); break;
                case "table": DiningTable(b); break;
                case "tv": case "tvstand": Television(b); break;
                case "toolbox": Toolbox(root,b); break;
                case "cabinet": case "kitchencabinet": StorageCabinet(root,b,1.48f,.94f,.67f,true,false); break;
                case "locker": StorageCabinet(root,b,.76f,2.08f,.63f,false,false); break;
                case "bagstation": StorageCabinet(root,b,1.7f,1.52f,.76f,false,false); break;
                case "medicinecabinet": StorageCabinet(root,b,.72f,.76f,.24f,false,true); break;
                case "supplycrate": SupplyCrate(root,b); break;
                case "radio": Radio(b,Vector3.zero,1); break;
                case "tablelamp": Lamp(b,false); break;
                case "floorlamp": Lamp(b,true); break;
                case "walllamp": WallLamp(b); break;
                case "toilet": Toilet(root,b); break;
                case "sink": Sink(root,b); break;
                case "bathtub": Bathtub(b); break;
                case "towelrack": TowelRack(b); break;
                case "washingmachine": WashingMachine(root,b); break;
                case "waterheater": WaterHeater(b); break;
                case "storagerack": StorageRack(b); break;
                case "car": Car(b); break;
                case "toolwall": ToolWall(b); break;
                case "bicycle": Bicycle(b); break;
                case "books": Books(b,Vector3.zero,1); break;
                case "plant": case "houseplant": Plant(b,Vector3.zero,1,true); break;
                case "shoe_rack": ShoeRack(b); break;
                case "coat_rack": CoatRack(b); break;
                case "curtains": Curtains(b); break;
                case "wallart": WallArt(b); break;
                case "garagedoor": GarageDoor(root,b); break;
                case "sofa": case "sofa_olive": Sofa(b,M.Olive); break;
                case "sofa_indigo": Sofa(b,M.Indigo); break;
                case "armchair": Armchair(b,M.Terracotta); break;
                case "chair": case "balconychair": Chair(b,id=="balconychair"?M.Olive:M.Indigo); break;
                case "sidetable": SideTable(b); break;
                case "bed": Bed(b); break;
                case "rug": Rug(b); break;
                case "laundrybasket": LaundryBasket(b); break;
                case "pendantlamp": PendantLamp(b); break;
                case "bunkerhatch": BunkerHatch(root,b); break;
                default: Plant(b,Vector3.zero,.7f,true); break;
            }
            b.Attach(root,"Prop_"+id); return root;
        }

        public static GameObject CreatePickup(string supplyId)
        {
            Init(); string id=(supplyId??"water").ToLowerInvariant();
            var root=new GameObject("Pickup_"+id); var b=new Builder();
            switch(id)
            {
                case "water": Bottle(b,Vector3.zero,1,M.Teal); break;
                case "food":
                    b.Cylinder(V(0,.105f,0),.075f,.2f,M.Terracotta,20);
                    b.Cylinder(V(0,.208f,0),.078f,.013f,M.Steel,20);
                    b.Cylinder(V(0,.01f,0),.078f,.014f,M.Steel,20);
                    b.Torus(V(0,.22f,0),.025f,.006f,M.Chrome,Quaternion.identity);
                    b.Box(V(0,.105f,-.076f),V(.085f,.105f,.007f),M.Mustard,.004f);
                    b.Box(V(0,.105f,-.082f),V(.03f,.068f,.005f),M.Leaf,.004f); break;
                case "bandage":
                    b.Cylinder(V(0,.085f,0),.085f,.14f,M.Linen,20,Quaternion.Euler(0,0,90));
                    b.Cylinder(V(-.073f,.085f,0),.027f,.008f,M.DarkWood,16,Quaternion.Euler(0,0,90));
                    b.Box(V(.035f,.012f,-.105f),V(.13f,.018f,.16f),M.Cream,.006f);
                    for(int i=0;i<5;i++) b.Box(V(.035f,.024f,-.15f+i*.022f),V(.116f,.003f,.003f),M.Linen,.001f); break;
                case "antiseptic":
                    Bottle(b,Vector3.zero,.66f,M.Terracotta);
                    b.Box(V(0,.115f,-.054f),V(.067f,.068f,.005f),M.Ceramic,.003f); Cross(b,V(0,.115f,-.059f),.052f,.012f,M.Red,false); break;
                case "firstaid":
                    b.Box(V(0,.135f,0),V(.34f,.26f,.16f),M.Cream,.035f);
                    b.Box(V(0,.268f,0),V(.35f,.035f,.17f),M.Red,.012f);
                    Handle(b,V(0,.29f,0),.15f,.06f,M.Steel);
                    Cross(b,V(0,.137f,-.085f),.15f,.037f,M.Red,false);
                    for(int s=-1;s<=1;s+=2) b.Box(V(s*.12f,.235f,-.09f),V(.035f,.053f,.016f),M.Steel,.003f); break;
                case "flashlight":
                    b.Cylinder(V(0,.115f,0),.047f,.19f,M.Indigo,16);
                    b.Cone(V(0,.236f,0),.047f,.075f,.065f,M.Rubber,20);
                    b.Cylinder(V(0,.273f,0),.066f,.013f,M.Glow,20);
                    for(int i=0;i<6;i++) b.Torus(V(0,.04f+i*.023f,0),.047f,.005f,M.Rubber,Quaternion.identity);
                    b.Box(V(0,.18f,-.05f),V(.027f,.035f,.013f),M.Mustard,.004f); break;
                case "battery":
                    for(int s=-1;s<=1;s+=2)
                    {
                        b.Cylinder(V(s*.028f,.075f,0),.023f,.14f,M.Olive,14);
                        b.Cylinder(V(s*.028f,.145f,0),.023f,.021f,M.Mustard,14);
                        b.Cylinder(V(s*.028f,.16f,0),.009f,.012f,M.Chrome,12);
                        b.Box(V(s*.028f,.09f,-.024f),V(.009f,.05f,.003f),M.Linen,.001f);
                    } break;
                case "radio": Radio(b,Vector3.zero,.52f); break;
                case "blanket":
                    for(int i=0;i<4;i++) b.Box(V(0,.027f+i*.043f,0),V(.37f,.044f,.26f),i%2==0?M.Indigo:M.Teal,.018f);
                    for(int i=0;i<7;i++) b.Box(V(-.14f+i*.047f,.18f,0),V(.011f,.004f,.25f),M.Linen,.001f);
                    for(int i=0;i<9;i++) b.Box(V(-.16f+i*.04f,.153f,-.15f),V(.009f,.013f,.065f),M.Mustard,.002f); break;
                case "screws":
                    b.Box(V(0,.055f,0),V(.19f,.105f,.14f),M.Mustard,.007f);
                    b.Box(V(0,.112f,.035f),V(.19f,.012f,.07f),M.Honey,.002f);
                    for(int i=0;i<5;i++)
                    {
                        var p=V(-.065f+i*.033f,.108f,-.034f);
                        b.Cylinder(p+V(0,.026f,0),.007f,.075f,M.Steel,8);
                        b.Cylinder(p+V(0,.064f,0),.016f,.009f,M.Chrome,8);
                        b.Box(p+V(0,.07f,0),V(.019f,.003f,.003f),M.Rubber,.001f);
                    } break;
                case "bracket":
                    b.Box(V(0,.012f,0),V(.22f,.024f,.16f),M.Steel,.004f);
                    b.Box(V(0,.1f,.067f),V(.22f,.2f,.025f),M.Steel,.004f);
                    for(int s=-1;s<=1;s+=2)
                    {
                        b.Cylinder(V(s*.07f,.027f,-.02f),.018f,.005f,M.Rubber,12);
                        b.Cylinder(V(s*.07f,.14f,.05f),.018f,.005f,M.Rubber,12,Quaternion.Euler(90,0,0));
                    } break;
                case "plank":
                    for(int i=0;i<3;i++)
                    {
                        b.Box(V(0,.025f+i*.04f,i*.014f),V(.64f,.038f,.125f),i==1?M.Honey:M.Walnut,.006f);
                        for(int j=0;j<3;j++) b.Box(V(-.04f+j*.03f,.045f+i*.04f,i*.014f-.025f+j*.02f),V(.45f,.002f,.002f),M.DarkWood,.0002f);
                    } break;
                case "sandbag":
                    b.Ellipsoid(V(0,.1f,0),V(.245f,.105f,.15f),M.Linen);
                    b.Box(V(0,.186f,0),V(.4f,.016f,.017f),M.Honey,.004f);
                    for(int i=0;i<10;i++) b.Beam(V(-.18f+i*.04f,.19f,-.013f),V(-.17f+i*.04f,.19f,.013f),.004f,M.DarkWood); break;
                case "seedling":
                    b.Ellipsoid(V(0,.07f,0),V(.095f,.075f,.085f),M.DarkWood);
                    Plant(b,V(0,.1f,0),.34f,false); break;
                case "hose":
                    for(int i=0;i<4;i++) b.Torus(V(0,.022f+i*.027f,0),.12f+i*.006f,.018f,M.Olive,Quaternion.identity);
                    b.Beam(V(.1f,.04f,0),V(.23f,.04f,-.08f),.032f,M.Olive);
                    b.Cylinder(V(.244f,.042f,-.088f),.023f,.065f,M.Mustard,12,Quaternion.Euler(65,0,55)); break;
                case "extinguisher":
                    b.Cylinder(V(0,.19f,0),.085f,.32f,M.Red,20);
                    b.Ellipsoid(V(0,.35f,0),V(.084f,.042f,.084f),M.Red);
                    b.Box(V(.025f,.415f,0),V(.12f,.04f,.04f),M.Rubber,.008f);
                    b.Cylinder(V(0,.39f,0),.022f,.06f,M.Steel,12);
                    b.Box(V(0,.2f,-.085f),V(.085f,.14f,.008f),M.Linen,.004f);
                    Cross(b,V(0,.2f,-.092f),.059f,.017f,M.Red,false);
                    b.Beam(V(.06f,.4f,0),V(.11f,.3f,0),.025f,M.Rubber);
                    b.Beam(V(.11f,.3f,0),V(.11f,.07f,0),.025f,M.Rubber); break;
                default: Bottle(b,Vector3.zero,1,M.Teal); break;
            }
            b.Attach(root,"Pickup_"+id); return root;
        }

        static void Fireplace(Builder b)
        {
            // An open firebox: there is no flat black cube over the opening.
            b.Box(V(0,.085f,0),V(1.85f,.17f,.88f),M.Concrete,.028f);
            b.Box(V(0,.66f,.3f),V(1.57f,1.12f,.15f),M.DarkWood,.014f);
            for(int s=-1;s<=1;s+=2) b.Box(V(s*.67f,.68f,0),V(.3f,1.15f,.72f),M.Terracotta,.025f);
            b.Box(V(0,1.2f,0),V(1.62f,.25f,.75f),M.Brick,.022f);
            b.Box(V(0,1.37f,-.02f),V(1.89f,.13f,.91f),M.Honey,.025f);
            b.Box(V(0,1.8f,.22f),V(1.1f,.76f,.42f),M.Cream,.016f);
            for(int i=0;i<7;i++) b.Beam(V(-.47f+i*.155f,.24f,-.28f),V(-.47f+i*.155f,.55f,-.28f),.021f,M.Steel);
            b.Beam(V(-.5f,.49f,-.28f),V(.5f,.49f,-.28f),.025f,M.Steel);
            for(int i=0;i<4;i++) b.Cylinder(V(-.27f+i*.18f,.26f,.01f),.066f,.65f,M.DarkWood,10,Quaternion.Euler(0,0,80+i*7));
            for(int r=0;r<4;r++) for(int s=-1;s<=1;s+=2)
            {
                b.Box(V(s*.67f,.24f+r*.27f,-.37f),V(.3f,.014f,.012f),M.Honey,.001f);
                b.Box(V(s*.55f,.25f+r*.27f,-.379f),V(.02f,.13f,.01f),M.Cream,.001f);
            }
            Books(b,V(-.49f,1.445f,.06f),.45f);
            b.Cylinder(V(.51f,1.53f,.12f),.072f,.15f,M.Olive,18);
            b.Cylinder(V(.51f,1.61f,.12f),.035f,.02f,M.Glow,16);
        }
        static void Computer(Builder b)
        {
            // Origin is the desktop: y=.805 on Create("desk"), front -Z. All parts fit its .8m depth.
            const float mx=-.16f;
            b.Box(V(mx,.025f,.12f),V(.4f,.05f,.26f),M.Navy,.02f);
            b.Beam(V(mx,.04f,.17f),V(mx,.27f,.2f),.075f,M.Chrome);
            b.Box(V(mx,.51f,.2f),V(.96f,.59f,.065f),M.Navy,.025f);
            b.Box(V(mx,.525f,.162f),V(.865f,.49f,.012f),M.Screen,.008f);
            // Large calm wallpaper, document window and toolbar read from the isometric camera.
            b.Quad(V(mx-.43f,.28f,.153f),V(mx-.43f,.53f,.153f),V(mx-.11f,.68f,.153f),V(mx+.43f,.28f,.153f),M.Sky);
            b.Box(V(mx+.09f,.54f,.145f),V(.4f,.32f,.008f),M.Paper,.008f);
            b.Box(V(mx+.09f,.68f,.138f),V(.4f,.045f,.008f),M.Indigo,.003f);
            b.Box(V(mx+.09f,.31f,.136f),V(.81f,.03f,.007f),M.Navy,.003f);
            for(int i=0;i<3;i++)b.Box(V(mx+.07f,.6f-i*.06f,.135f),V(.24f-i*.025f,.018f,.006f),i==2?M.Teal:M.Sky,.002f);
            for(int i=0;i<4;i++)b.Box(V(mx-.32f+i*.1f,.312f,.129f),V(.035f,.018f,.006f),i==0?M.Mustard:M.Paper,.002f);
            b.Cylinder(V(mx,.243f,.158f),.009f,.009f,M.Glow,10,Quaternion.Euler(90,0,0));
            // Ivory keys, a visible space bar, indigo desk pad, mouse and a two-tone tower.
            b.Box(V(-.17f,.012f,-.22f),V(.85f,.014f,.3f),M.Indigo,.025f);
            b.Box(V(-.25f,.035f,-.22f),V(.61f,.04f,.21f),M.Navy,.013f);
            for(int r=0;r<3;r++)for(int k=0;k<12;k++)
                b.Box(V(-.515f+k*.048f,.06f,-.283f+r*.05f),V(.038f,.012f,.034f),k==0?M.Coral:M.Paper,.004f);
            b.Box(V(-.25f,.06f,-.132f),V(.24f,.012f,.028f),M.Paper,.004f);
            b.Ellipsoid(V(.26f,.038f,-.22f),V(.05f,.035f,.078f),M.Paper);
            b.Box(V(.26f,.073f,-.235f),V(.006f,.006f,.024f),M.Teal,.002f);
            b.Box(V(.62f,.285f,.045f),V(.26f,.57f,.48f),M.Navy,.025f);
            b.Box(V(.62f,.285f,-.2f),V(.22f,.52f,.015f),M.Indigo,.012f);
            b.Box(V(.756f,.3f,.045f),V(.012f,.43f,.34f),M.Sky,.01f);
            b.Cylinder(V(.62f,.47f,-.212f),.022f,.012f,M.Glow,16,Quaternion.Euler(90,0,0));
            for(int i=0;i<6;i++)b.Box(V(.62f,.12f+i*.036f,-.214f),V(.16f,.014f,.012f),M.Rubber,.003f);
            for(int i=0;i<2;i++)b.Box(V(.585f+i*.07f,.4f,-.215f),V(.04f,.015f,.014f),M.Chrome,.002f);
            Cable(b,new[]{V(mx,.3f,.24f),V(mx,.09f,.32f),V(.25f,.015f,.32f),V(.62f,.08f,.27f)},.01f,M.Rubber);
            Cable(b,new[]{V(.26f,.015f,-.16f),V(.32f,.014f,.05f),V(.4f,.014f,.29f)},.006f,M.Rubber);
        }
        static void Desk(GameObject root,Builder b)
        {
            b.Box(V(0,.76f,0),V(1.75f,.09f,.8f),M.Honey,.025f);
            b.Box(V(0,.704f,.02f),V(1.64f,.025f,.7f),M.Walnut,.006f);
            Legs(b,1.56f,.62f,.73f,M.Navy,.065f);
            b.Box(V(0,.48f,.31f),V(1.52f,.26f,.035f),M.Teal,.007f);
            b.Box(V(.57f,.45f,.03f),V(.43f,.49f,.62f),M.Teal,.015f);
            b.Box(V(.57f,.37f,-.29f),V(.38f,.27f,.035f),M.Indigo,.009f);
            Handle(b,V(.57f,.4f,-.32f),.16f,.028f,M.Chrome);
            var lid=Group(root,"Lid"); lid.transform.localPosition=V(.355f,.53f,-.29f);
            var d=new Builder(); d.Box(V(.215f,.07f,0),V(.4f,.15f,.04f),M.Teal,.01f);
            Handle(d,V(.215f,.07f,-.027f),.16f,.028f,M.Chrome);d.Attach(lid,"DeskDrawerFront");
            // Desktop props stay at the left back edge, clear of keyboard and monitor.
            b.Cylinder(V(-.74f,.852f,.23f),.045f,.09f,M.Coral,16);
            b.Torus(V(-.682f,.852f,.23f),.031f,.008f,M.Coral,Quaternion.Euler(90,0,0));
            b.Box(V(-.72f,.815f,-.16f),V(.18f,.018f,.22f),M.Paper,.003f);
            b.Beam(V(-.78f,.832f,-.23f),V(-.66f,.832f,-.1f),.008f,M.Mustard);
        }

        static void Bookshelf(Builder b)
        {
            // Same 1.54 x 2.37 x .53 envelope as the movable/anchorable model item.
            b.Box(V(0,1.18f,.235f),V(1.54f,2.34f,.06f),M.Teal,.012f);
            for(int side=-1;side<=1;side+=2)b.Box(V(side*.73f,1.18f,0),V(.08f,2.34f,.53f),M.Walnut,.014f);
            b.Box(V(0,2.33f,0),V(1.54f,.08f,.53f),M.Honey,.015f);
            b.Box(V(0,.075f,0),V(1.54f,.15f,.53f),M.DarkWood,.012f);
            for(int level=0;level<4;level++)
            {
                float y=.17f+level*.53f;
                b.Box(V(0,y,0),V(1.42f,.055f,.52f),M.Honey,.007f);
                // Uneven groups, page blocks and bold spines are readable from the room view.
                Books(b,V(-.37f,y+.03f,-.035f),1.14f);
                if(level!=1)Books(b,V(.27f,y+.03f,-.035f),level==3?.93f:1.03f);
                else
                {
                    b.Box(V(.35f,y+.11f,-.015f),V(.43f,.16f,.34f),M.Linen,.02f);
                    b.Box(V(.35f,y+.11f,-.19f),V(.17f,.07f,.008f),M.Indigo,.004f);
                    b.Box(V(.35f,y+.11f,-.198f),V(.11f,.014f,.004f),M.Paper,.001f);
                }
            }
            // Two framed family photos create a lived-in upper shelf without new characters.
            for(int i=0;i<2;i++)
            {
                b.Box(V(.36f+i*.17f,1.92f,-.18f),V(.13f,.19f,.025f),M.DarkWood,.006f);
                b.Box(V(.36f+i*.17f,1.92f,-.196f),V(.102f,.158f,.006f),i==0?M.Sky:M.Coral,.002f);
            }
        }
        static void DiningTable(Builder b)
        {
            // Top=1.05m preserves the current table-top decoration and gameplay envelope.
            Legs(b,1.34f,.72f,.96f,M.Walnut,.085f);
            b.Box(V(0,1.0f,0),V(1.55f,.1f,.94f),M.Honey,.035f);
            for(int i=0;i<4;i++)b.Box(V(-.57f+i*.38f,1.052f,0),V(.007f,.003f,.86f),M.Walnut,.001f);
            b.Box(V(0,.82f,0),V(1.36f,.2f,.73f),M.Walnut,.014f);
            b.Box(V(.2f,1.059f,0),V(.48f,.012f,.86f),M.Teal,.012f);
            for(int i=0;i<6;i++)b.Box(V(.01f+i*.074f,1.067f,0),V(.018f,.003f,.82f),M.Linen,.001f);
            b.Cylinder(V(-.52f,1.105f,.19f),.12f,.07f,M.Ceramic,24);
            b.Ellipsoid(V(-.54f,1.16f,.18f),V(.045f,.045f,.045f),M.Coral);
            b.Ellipsoid(V(-.48f,1.16f,.21f),V(.046f,.044f,.046f),M.Mustard);
        }
        static void Television(Builder b)
        {
            // TV + open walnut console fit the old tvstand 1.7 x1.58 x.55 envelope.
            Legs(b,1.46f,.36f,.14f,M.DarkWood,.06f);
            b.Box(V(0,.19f,0),V(1.7f,.09f,.55f),M.Honey,.018f);
            b.Box(V(0,.56f,0),V(1.7f,.08f,.55f),M.Honey,.018f);
            b.Box(V(0,.37f,.24f),V(1.64f,.29f,.05f),M.Teal,.008f);
            for(int x=-1;x<=1;x++)b.Box(V(x*.8f,.37f,0),V(.06f,.29f,.5f),M.Walnut,.008f);
            Books(b,V(-.4f,.235f,-.02f),.75f);
            b.Box(V(.38f,.33f,-.02f),V(.43f,.12f,.31f),M.Indigo,.014f);
            b.Box(V(.38f,.33f,-.181f),V(.21f,.024f,.008f),M.Sky,.003f);
            for(int side=-1;side<=1;side+=2)b.Beam(V(side*.36f,.61f,.02f),V(side*.44f,.7f,.06f),.038f,M.Navy);
            b.Box(V(0,1.13f,.06f),V(1.53f,.86f,.06f),M.Navy,.025f);
            b.Box(V(0,1.145f,.023f),V(1.42f,.735f,.012f),M.Indigo,.01f);
            // A sunset landscape, visibly a picture rather than a dead gray panel.
            b.Cylinder(V(.39f,1.34f,.012f),.13f,.01f,M.Mustard,32,Quaternion.Euler(90,0,0));
            b.Quad(V(-.7f,.78f,.011f),V(-.7f,1.04f,.011f),V(-.25f,1.28f,.011f),V(.49f,.78f,.011f),M.Teal);
            b.Quad(V(-.32f,.78f,.005f),V(.18f,1.1f,.005f),V(.7f,.96f,.005f),V(.7f,.78f,.005f),M.Mint);
            b.Cylinder(V(.66f,.735f,.017f),.008f,.008f,M.Glow,10,Quaternion.Euler(90,0,0));
            b.Box(V(.59f,.611f,-.17f),V(.19f,.023f,.07f),M.Rubber,.006f);
        }
        static void Toolbox(GameObject root,Builder b)
        {
            const float w=.9f,h=.43f,d=.48f,t=.035f;
            OpenStorageBox(b,w,h,d,t,M.Coral,M.Navy);
            b.Box(V(0,.085f,-.249f),V(.83f,.065f,.025f),M.Navy,.006f);
            b.Box(V(0,.31f,-.253f),V(.28f,.09f,.014f),M.Paper,.005f);
            b.Box(V(0,.31f,-.263f),V(.17f,.025f,.008f),M.Navy,.002f);
            for(int side=-1;side<=1;side+=2)
            {
                b.Box(V(side*.29f,.36f,-.262f),V(.065f,.12f,.035f),M.Chrome,.008f);
                b.Box(V(side*.46f,.24f,0),V(.035f,.26f,.32f),M.Navy,.007f);
                for(int j=0;j<2;j++)b.Cylinder(V(side*.465f,.17f+j*.13f,-.11f),.011f,.012f,M.Chrome,8,Quaternion.Euler(0,0,90));
            }
            // Visible open tray: hammer, screwdriver and a distinct yellow screw carton.
            b.Box(V(0,.22f,.015f),V(.78f,.035f,.36f),M.Honey,.006f);
            b.Beam(V(-.28f,.27f,-.12f),V(-.28f,.27f,.12f),.04f,M.Walnut);
            b.Box(V(-.28f,.27f,.13f),V(.18f,.07f,.06f),M.Chrome,.008f);
            b.Beam(V(-.04f,.26f,-.12f),V(.04f,.26f,.13f),.018f,M.Chrome);
            b.Box(V(-.04f,.26f,-.11f),V(.06f,.045f,.13f),M.Indigo,.012f);
            b.Box(V(.24f,.28f,.01f),V(.19f,.1f,.23f),M.Mustard,.009f);
            var lid=Group(root,"Lid");lid.transform.localPosition=V(0,h,d*.5f);
            var l=new Builder();
            l.Box(V(0,.035f,-d*.5f),V(w+.015f,.1f,d+.015f),M.Coral,.025f);
            l.Box(V(0,.09f,-d*.5f),V(.38f,.025f,.18f),M.Navy,.009f);
            Handle(l,V(0,.1f,-d*.5f),.32f,.1f,M.Chrome);
            for(int side=-1;side<=1;side+=2)l.Box(V(side*.38f,.091f,-d*.5f),V(.04f,.025f,.4f),M.Paper,.003f);
            l.Attach(lid,"ToolboxHingedLid");
        }
        static void StorageCabinet(GameObject root,Builder b,float w,float h,float d,bool kitchen,bool medical)
        {
            // Cabinet shells remain hollow behind DoorLeaf; root owns stored gameplay supplies.
            M body=medical?M.Paper:kitchen?M.Teal:M.Indigo;
            float t=.045f,bottom=medical?0:.1f;
            b.Box(V(0,bottom+t*.5f,0),V(w,t,d),M.Honey,.009f);
            b.Box(V(0,h-t*.5f,0),V(w,t,d),body,.009f);
            b.Box(V(0,(h+bottom)*.5f,d*.5f-t*.5f),V(w,h-bottom,t),body,.009f);
            for(int side=-1;side<=1;side+=2)b.Box(V(side*(w-t)*.5f,(h+bottom)*.5f,0),V(t,h-bottom,d),body,.009f);
            int shelves=medical?1:kitchen?1:3;
            for(int i=1;i<=shelves;i++)
            {
                float y=bottom+(h-bottom)*i/(shelves+1);
                b.Box(V(0,y,.015f),V(w-.08f,.03f,d-.07f),medical?M.Sky:M.Honey,.005f);
                if(medical)
                {
                    Bottle(b,V(-.19f,y+.02f,0),.52f,M.Coral);
                    b.Box(V(.12f,y+.075f,0),V(.16f,.12f,.12f),M.Paper,.012f);
                    Cross(b,V(.12f,y+.075f,-.066f),.075f,.02f,M.Red,false);
                }
                else if(!kitchen)
                {
                    b.Box(V(-w*.22f,y+.09f,0),V(w*.3f,.16f,d*.56f),i%2==0?M.Mustard:M.Linen,.014f);
                    b.Box(V(w*.2f,y+.1f,0),V(w*.26f,.18f,d*.58f),i%2==0?M.Teal:M.Coral,.014f);
                }
            }
            if(kitchen)
            {
                b.Box(V(0,h+.025f,0),V(w+.06f,.075f,d+.06f),M.Paper,.022f);
                b.Box(V(0,.06f,.04f),V(w-.12f,.12f,d-.14f),M.DarkWood,.008f);
                b.Cylinder(V(.46f,h+.12f,.1f),.08f,.12f,M.Terracotta,20);
                b.Box(V(-.42f,h+.074f,.04f),V(.27f,.018f,.3f),M.Honey,.009f);
            }
            else if(!medical)Legs(b,w-.14f,d-.14f,.1f,M.DarkWood,.07f);
            var leaf=Group(root,"DoorLeaf");leaf.transform.localPosition=V(-w*.5f+.045f,bottom,-d*.5f-.016f);
            float lw=w-.09f,lh=h-bottom-.025f;var f=new Builder();
            f.Box(V(lw*.5f,lh*.5f,0),V(lw,lh,.045f),body,.016f);
            if(medical)
            {
                f.Box(V(lw*.5f,lh*.5f,-.029f),V(lw-.11f,lh-.1f,.012f),M.Mint,.007f);
                Cross(f,V(lw*.5f,lh*.52f,-.04f),.28f,.075f,M.Red,false);
            }
            else
            {
                f.Box(V(lw*.5f,lh*.5f,-.029f),V(lw-.13f,lh-.16f,.014f),kitchen?M.Mint:M.Navy,.009f);
                if(!kitchen)for(int i=0;i<4;i++)f.Box(V(lw*.5f,lh-.2f-i*.058f,-.042f),V(lw*.56f,.019f,.009f),M.Sky,.003f);
                else f.Box(V(lw*.5f,lh*.5f,-.04f),V(.012f,lh-.16f,.008f),M.Teal,.002f);
            }
            Handle(f,V(lw-.075f,lh*.52f,-.045f),.055f,.065f,M.Chrome);
            for(int i=0;i<2;i++)f.Cylinder(V(.005f,.13f+i*Mathf.Max(.2f,lh-.26f),.012f),.018f,.085f,M.Chrome,10);
            f.Attach(leaf,medical?"MedicineCabinetDoor":kitchen?"KitchenCabinetDoor":w>1?"BagStationDoor":"LockerDoor");
        }
        static void SupplyCrate(GameObject root,Builder b)
        {
            const float w=1.1f,h=.71f,d=.8f;
            OpenStorageBox(b,w,h,d,.045f,M.Honey,M.DarkWood);
            for(int i=0;i<4;i++)
            {
                b.Box(V(0,.13f+i*.15f,-.407f),V(1.01f,.012f,.01f),M.Walnut,.001f);
                b.Box(V(0,.13f+i*.15f,.407f),V(1.01f,.012f,.01f),M.Walnut,.001f);
            }
            for(int side=-1;side<=1;side+=2)
            {
                b.Box(V(side*.39f,.36f,-.415f),V(.085f,.65f,.025f),M.Teal,.005f);
                b.Box(V(side*.39f,.36f,.415f),V(.085f,.65f,.025f),M.Teal,.005f);
                for(int i=0;i<3;i++)b.Cylinder(V(side*.39f,.1f+i*.25f,-.434f),.014f,.009f,M.Chrome,8,Quaternion.Euler(90,0,0));
            }
            b.Box(V(0,.36f,-.43f),V(.36f,.23f,.022f),M.Paper,.007f);
            b.Box(V(0,.36f,-.445f),V(.19f,.045f,.008f),M.Teal,.002f);
            b.Box(V(-.22f,.19f,0),V(.35f,.28f,.45f),M.Indigo,.018f);
            b.Box(V(.24f,.18f,0),V(.31f,.26f,.42f),M.Coral,.018f);
            var lid=Group(root,"Lid");lid.transform.localPosition=V(0,h,d*.5f);var l=new Builder();
            l.Box(V(0,.025f,-d*.5f),V(w+.02f,.065f,d+.02f),M.Honey,.018f);
            for(int side=-1;side<=1;side+=2)l.Box(V(side*.39f,.064f,-d*.5f),V(.085f,.025f,d+.025f),M.Teal,.003f);
            Handle(l,V(0,.066f,-d*.5f),.3f,.055f,M.DarkWood);l.Attach(lid,"CrateHingedLid");
        }
        static void OpenStorageBox(Builder b,float w,float h,float d,float t,M body,M inside)
        {
            b.Box(V(0,t*.5f,0),V(w,t,d),inside,.008f);
            for(int side=-1;side<=1;side+=2)
            {
                b.Box(V(side*(w-t)*.5f,h*.5f,0),V(t,h,d),body,.009f);
                b.Box(V(0,h*.5f,side*(d-t)*.5f),V(w,h,t),body,.009f);
            }
        }

        static void Radio(Builder b,Vector3 p,float s)
        {
            b.Box(p+V(0,.19f,0)*s,V(.55f,.36f,.19f)*s,M.Terracotta,.03f*s);
            b.Box(p+V(-.105f,.19f,-.101f)*s,V(.29f,.27f,.015f)*s,M.Rubber,.018f*s);
            for(int i=0;i<13;i++) b.Box(p+V(-.105f,.07f+i*.02f,-.112f)*s,V(.257f,.006f,.005f)*s,M.Honey,.001f);
            b.Box(p+V(.171f,.27f,-.104f)*s,V(.125f,.068f,.01f)*s,M.Cream,.006f);
            for(int i=0;i<5;i++) b.Box(p+V(.126f+i*.022f,.27f,-.112f)*s,V(.005f,.037f,.006f)*s,M.Rubber,.001f);
            b.Box(p+V(.179f,.27f,-.117f)*s,V(.009f,.057f,.005f)*s,M.Red,.001f);
            for(int i=0;i<2;i++) b.Cylinder(p+V(.14f+i*.065f,.13f,-.12f)*s,.027f*s,.035f*s,M.Chrome,18,Quaternion.Euler(90,0,0));
            Handle(b,p+V(0,.37f,0)*s,.31f*s,.09f*s,M.DarkWood);
            b.Beam(p+V(-.22f,.36f,.025f)*s,p+V(-.32f,.86f,.025f)*s,.009f*s,M.Chrome);
            for(int side=-1;side<=1;side+=2) b.Box(p+V(side*.18f,.012f,0)*s,V(.08f,.025f,.13f)*s,M.Rubber,.004f);
        }
        static void Lamp(Builder b,bool tall)
        {
            float h=tall?1.48f:.5f, r=tall?.29f:.21f;
            b.Cylinder(V(0,.035f,0),tall?.21f:.14f,.07f,M.DarkWood,24);
            b.Cylinder(V(0,h*.49f,0),.025f,h*.94f,M.Steel,16);
            b.Cone(V(0,h,0),r,r*.61f,tall?.44f:.31f,M.Linen,32);
            b.Torus(V(0,h-(tall?.22f:.155f),0),r,.011f,M.Mustard,Quaternion.identity);
            for(int i=0;i<20;i++)
            {
                float a=i*Mathf.PI*2/20;
                b.Beam(V(Mathf.Cos(a)*r,h-(tall?.22f:.155f),Mathf.Sin(a)*r),V(Mathf.Cos(a)*r*.61f,h+(tall?.22f:.155f),Mathf.Sin(a)*r*.61f),.006f,M.Cream);
            }
            b.Ellipsoid(V(0,h-.08f,0),V(.064f,.08f,.064f),M.Glow);
            Cable(b,new[]{V(.02f,.04f,0),V(.15f,.015f,.1f),V(.26f,.012f,.15f),V(.32f,.012f,.1f)},.008f,M.Rubber);
        }
        static void WallLamp(Builder b)
        {
            b.Box(V(0,.25f,.02f),V(.2f,.45f,.08f),M.DarkWood,.025f);
            b.Box(V(0,.26f,-.08f),V(.15f,.32f,.13f),M.Glow,.02f);
            for(int s=-1;s<=1;s+=2) b.Box(V(s*.092f,.26f,-.1f),V(.022f,.38f,.18f),M.Steel,.004f);
            b.Box(V(0,.46f,-.1f),V(.24f,.07f,.23f),M.Steel,.009f);
            b.Box(V(0,.06f,-.1f),V(.24f,.07f,.23f),M.Steel,.009f);
        }
        static void Toilet(GameObject root,Builder b)
        {
            b.Ellipsoid(V(0,.22f,0),V(.18f,.21f,.24f),M.Ceramic);
            b.Ellipsoid(V(0,.43f,-.11f),V(.275f,.16f,.34f),M.Ceramic);
            b.Ellipsoid(V(0,.55f,-.12f),V(.205f,.016f,.26f),M.Rubber);
            b.Box(V(0,.68f,.225f),V(.48f,.58f,.22f),M.Ceramic,.045f);
            b.Box(V(0,.99f,.225f),V(.51f,.045f,.25f),M.Cream,.015f);
            b.Cylinder(V(.13f,1.016f,.225f),.04f,.012f,M.Chrome,16);
            var lid=Group(root,"Lid"); lid.transform.localPosition=V(0,.56f,.1f);
            var d=new Builder(); d.Ellipsoid(V(0,.019f,-.22f),V(.26f,.025f,.32f),M.Cream);
            d.Box(V(0,.022f,0),V(.27f,.045f,.07f),M.Steel,.008f); d.Attach(lid,"ToiletSeatLid");
            b.Cylinder(V(.36f,.48f,.24f),.012f,.8f,M.Chrome,12);
            b.Cylinder(V(.36f,.89f,.24f),.065f,.055f,M.Linen,16,Quaternion.Euler(0,0,90));
        }
        static void Sink(GameObject root,Builder b)
        {
            // Hollow vanity, separate right door: the moving leaf reveals shelves and stored bottles.
            b.Box(V(0,.145f,.025f),V(.85f,.045f,.5f),M.Olive,.008f);
            b.Box(V(0,.8f,.025f),V(.85f,.045f,.5f),M.Olive,.008f);
            b.Box(V(0,.47f,.25f),V(.85f,.65f,.045f),M.Olive,.008f);
            for(int s=-1;s<=1;s+=2)b.Box(V(s*.405f,.47f,.025f),V(.045f,.65f,.5f),M.Olive,.008f);
            b.Box(V(0,.4f,.04f),V(.75f,.03f,.38f),M.Honey,.004f);
            Bottle(b,V(-.19f,.43f,.04f),.64f,M.Terracotta);
            Legs(b,.68f,.34f,.12f,M.DarkWood,.04f);
            b.Box(V(0,.86f,0),V(.94f,.08f,.63f),M.Ceramic,.028f);
            b.Ellipsoid(V(0,.9f,-.025f),V(.32f,.017f,.205f),M.Teal);
            b.Cylinder(V(0,.924f,-.025f),.025f,.01f,M.Chrome,16);
            Faucet(b,V(0,.9f,.2f),.24f);
            var leaf=Group(root,"DoorLeaf"); leaf.transform.localPosition=V(-.385f,.15f,-.24f);
            var d=new Builder(); d.Box(V(.19f,.32f,0),V(.375f,.63f,.045f),M.Olive,.013f);
            Handle(d,V(.29f,.39f,-.03f),.08f,.03f,M.Chrome); d.Attach(leaf,"VanityDoor");
            b.Box(V(.198f,.47f,-.245f),V(.37f,.63f,.045f),M.Olive,.013f);
            Handle(b,V(.1f,.55f,-.275f),.08f,.03f,M.Chrome);
            b.Box(V(0,1.48f,.29f),V(.76f,.78f,.04f),M.DarkWood,.018f);
            b.Box(V(0,1.48f,.263f),V(.67f,.69f,.015f),M.Glass,.004f);
            Bottle(b,V(-.34f,.91f,.11f),.42f,M.Terracotta);
        }
        static void Bathtub(Builder b)
        {
            // Four solid shaped walls and a recessed basin, never a solid top covering the bath.
            b.Box(V(0,.17f,0),V(1.78f,.23f,.79f),M.Ceramic,.08f);
            for(int s=-1;s<=1;s+=2)
            {
                b.Box(V(0,.43f,s*.4f),V(1.77f,.46f,.14f),M.Ceramic,.055f);
                b.Box(V(s*.84f,.43f,0),V(.16f,.46f,.73f),M.Ceramic,.06f);
                b.Box(V(s*.52f,.09f,0),V(.11f,.16f,.52f),M.Steel,.02f);
            }
            b.Box(V(0,.31f,0),V(1.48f,.018f,.63f),M.Teal,.07f);
            b.Cylinder(V(-.61f,.325f,0),.035f,.013f,M.Chrome,16);
            Faucet(b,V(-.76f,.65f,0),.27f);
            for(int i=0;i<2;i++) b.Cylinder(V(-.77f,.7f,-.16f+i*.32f),.026f,.065f,M.Chrome,12);
            b.Box(V(.49f,.682f,0),V(.32f,.045f,.96f),M.Honey,.014f);
            b.Box(V(.49f,.714f,-.09f),V(.18f,.025f,.36f),M.Indigo,.007f);
            b.Cylinder(V(.49f,.77f,.22f),.036f,.1f,M.Linen,16);
        }
        static void TowelRack(Builder b)
        {
            for(int s=-1;s<=1;s+=2) b.Cylinder(V(s*.4f,1.13f,0),.023f,.16f,M.Chrome,12,Quaternion.Euler(90,0,0));
            b.Cylinder(V(0,1.13f,-.08f),.021f,.86f,M.Chrome,16,Quaternion.Euler(0,0,90));
            b.Box(V(-.15f,.91f,-.1f),V(.34f,.47f,.05f),M.Terracotta,.015f);
            b.Box(V(.22f,.96f,-.1f),V(.23f,.36f,.05f),M.Indigo,.012f);
            for(int i=0;i<4;i++) b.Box(V(-.15f,.7f+i*.03f,-.128f),V(.32f,.008f,.005f),M.Linen,.001f);
        }
        static void WashingMachine(GameObject root,Builder b)
        {
            b.Box(V(0,.44f,.315f),V(.77f,.86f,.055f),M.Cream,.012f);
            for(int s=-1;s<=1;s+=2)b.Box(V(s*.36f,.44f,0),V(.055f,.86f,.68f),M.Cream,.012f);
            b.Box(V(0,.045f,0),V(.77f,.07f,.68f),M.Cream,.012f);
            // Facade pieces surround an open drum aperture; the hinge opens into a real cavity.
            b.Box(V(0,.77f,-.325f),V(.69f,.18f,.055f),M.Cream,.01f);
            b.Box(V(0,.067f,-.325f),V(.69f,.12f,.055f),M.Cream,.01f);
            for(int s=-1;s<=1;s+=2)b.Box(V(s*.31f,.4f,-.325f),V(.075f,.55f,.055f),M.Cream,.008f);
            b.Box(V(0,.88f,0),V(.79f,.045f,.7f),M.Ceramic,.012f);
            b.Box(V(0,.75f,-.35f),V(.7f,.14f,.035f),M.Indigo,.012f);
            b.Cylinder(V(.22f,.75f,-.38f),.049f,.045f,M.Chrome,20,Quaternion.Euler(90,0,0));
            b.Box(V(-.17f,.75f,-.374f),V(.23f,.07f,.012f),M.Screen,.005f);
            for(int i=0;i<3;i++) b.Box(V(-.23f+i*.06f,.75f,-.382f),V(.035f,.012f,.007f),M.Glow,.001f);
            b.Torus(V(0,.38f,-.353f),.263f,.022f,M.Rubber,Quaternion.Euler(90,0,0));
            b.Cylinder(V(0,.38f,.13f),.247f,.02f,M.Steel,32,Quaternion.Euler(90,0,0));
            for(int i=0;i<16;i++)
            {
                float a=i*Mathf.PI*2/16;
                b.Beam(V(Mathf.Cos(a)*.245f,.38f+Mathf.Sin(a)*.245f,-.3f),V(Mathf.Cos(a)*.245f,.38f+Mathf.Sin(a)*.245f,.14f),.012f,M.Steel);
            }
            var door=Group(root,"DoorLeaf"); door.transform.localPosition=V(-.245f,.38f,-.37f);
            var d=new Builder();
            d.Torus(V(.245f,0,0),.23f,.031f,M.Chrome,Quaternion.Euler(90,0,0));
            d.Cylinder(V(.245f,0,-.004f),.205f,.022f,M.Glass,32,Quaternion.Euler(90,0,0));
            d.Box(V(.43f,.02f,-.036f),V(.04f,.105f,.035f),M.Indigo,.008f); d.Attach(door,"WasherRoundDoor");
            b.Ellipsoid(V(0,.24f,-.07f),V(.17f,.08f,.16f),M.Terracotta);
            Cable(b,new[]{V(.2f,.7f,.35f),V(.28f,.52f,.43f),V(.32f,.12f,.43f),V(.44f,.06f,.4f)},.025f,M.Rubber);
            b.Box(V(-.18f,.93f,.08f),V(.35f,.06f,.29f),M.Teal,.02f);
            b.Box(V(-.18f,.984f,.08f),V(.34f,.05f,.28f),M.Linen,.015f);
            Bottle(b,V(.23f,.91f,.05f),.62f,M.Terracotta);
        }
        static void WaterHeater(Builder b)
        {
            b.Cylinder(V(0,1.01f,0),.32f,1.6f,M.Cream,32);
            b.Ellipsoid(V(0,1.82f,0),V(.32f,.1f,.32f),M.Ceramic);
            b.Cylinder(V(0,.21f,0),.335f,.08f,M.Steel,28);
            for(int s=-1;s<=1;s+=2)
            {
                b.Beam(V(s*.16f,.08f,0),V(s*.16f,.24f,0),.06f,M.Steel);
                b.Beam(V(s*.16f,1.84f,0),V(s*.16f,2.12f,0),.033f,s<0?M.Indigo:M.Red);
                b.Beam(V(s*.16f,2.12f,0),V(s*.43f,2.12f,0),.033f,M.Steel);
            }
            b.Box(V(0,1.23f,-.325f),V(.24f,.21f,.035f),M.Indigo,.012f);
            b.Cylinder(V(0,1.24f,-.35f),.067f,.02f,M.Cream,24,Quaternion.Euler(90,0,0));
            b.Beam(V(0,1.24f,-.368f),V(.035f,1.27f,-.368f),.008f,M.Red);
            b.Box(V(0,.74f,-.327f),V(.21f,.18f,.012f),M.Mustard,.006f);
            b.Torus(V(.32f,.48f,0),.06f,.009f,M.Red,Quaternion.Euler(90,0,0));
        }
        static void StorageRack(Builder b)
        {
            for(int s=-1;s<=1;s+=2) for(int z=-1;z<=1;z+=2)
                b.Box(V(s*.68f,1.04f,z*.26f),V(.045f,2.08f,.045f),M.Indigo,.006f);
            for(int level=0;level<4;level++)
            {
                float y=.12f+level*.57f;
                b.Box(V(0,y,0),V(1.45f,.055f,.61f),M.Honey,.008f);
                for(int i=0;i<5;i++)
                {
                    float x=-.55f+i*.27f;
                    if(level==0) Bottle(b,V(x,y+.03f,0),.88f,M.Teal);
                    else if(level==1)
                    {
                        b.Cylinder(V(x,y+.14f,0),.1f,.23f,i%2==0?M.Terracotta:M.Olive,18);
                        b.Cylinder(V(x,y+.26f,0),.102f,.018f,M.Mustard,18);
                        b.Box(V(x,y+.14f,-.1f),V(.1f,.1f,.008f),M.Linen,.004f);
                    }
                    else if(level==2)
                    {
                        b.Box(V(x,y+.14f,0),V(.23f,.24f,.41f),i%2==0?M.Olive:M.Indigo,.013f);
                        b.Box(V(x,y+.15f,-.21f),V(.13f,.07f,.006f),M.Linen,.003f);
                        b.Box(V(x,y+.27f,0),V(.08f,.009f,.42f),M.Honey,.001f);
                    }
                    else Books(b,V(x,y+.03f,0),.36f);
                }
            }
            b.Beam(V(-.65f,.2f,.28f),V(.65f,1.84f,.28f),.026f,M.Steel);
        }
        static void Car(Builder b)
        {
            // Compact estate: four independent tyres and shaped bonnet, glass cabin, hatch and body sills.
            b.Box(V(0,.42f,0),V(1.73f,.24f,4.13f),M.Rubber,.12f);
            b.Box(V(0,.65f,-.05f),V(1.9f,.46f,4.35f),M.Olive,.14f);
            b.Box(V(0,.89f,-1.34f),V(1.8f,.18f,1.47f),M.Olive,.065f,Quaternion.Euler(-4,0,0));
            b.Box(V(0,.95f,1.57f),V(1.79f,.24f,.92f),M.Olive,.09f);
            // Explicit trapezoid cabin silhouette avoids a rectangular toy-block body.
            b.Prism(new[]{V(-.81f,.88f,-.75f),V(.81f,.88f,-.75f),V(.69f,1.53f,-.27f),V(-.69f,1.53f,-.27f),
                V(-.82f,.9f,1.35f),V(.82f,.9f,1.35f),V(.7f,1.53f,.99f),V(-.7f,1.53f,.99f)},M.Olive);
            b.Box(V(0,1.555f,.35f),V(1.43f,.08f,1.36f),M.Olive,.055f);
            b.Quad(V(-.745f,.98f,-.69f),V(.745f,.98f,-.69f),V(.64f,1.48f,-.32f),V(-.64f,1.48f,-.32f),M.Glass);
            b.Quad(V(-.74f,.99f,1.37f),V(-.64f,1.47f,1.04f),V(.64f,1.47f,1.04f),V(.74f,.99f,1.37f),M.Glass);
            for(int s=-1;s<=1;s+=2)
            {
                float x=s*.82f;
                b.Quad(V(x,.99f,-.62f),V(s*.7f,1.47f,-.24f),V(s*.7f,1.47f,.29f),V(x,.99f,.29f),M.Glass);
                b.Quad(V(x,.99f,.4f),V(s*.7f,1.47f,.4f),V(s*.7f,1.47f,.94f),V(x,.99f,1.25f),M.Glass);
                b.Beam(V(s*.84f,.97f,.34f),V(s*.71f,1.51f,.34f),.06f,M.Rubber);
                b.Box(V(s*.954f,.71f,.33f),V(.015f,.32f,.018f),M.DarkWood,.002f);
                b.Box(V(s*.954f,.51f,.24f),V(.018f,.021f,3.51f),M.Steel,.003f);
                for(int z=-1;z<=1;z+=2)
                {
                    float wz=z*1.35f;
                    b.Torus(V(s*.91f,.39f,wz),.275f,.095f,M.Rubber,Quaternion.Euler(0,0,90));
                    b.Cylinder(V(s*.996f,.39f,wz),.225f,.04f,M.Steel,24,Quaternion.Euler(0,0,90));
                    b.Cylinder(V(s*1.021f,.39f,wz),.078f,.045f,M.Chrome,18,Quaternion.Euler(0,0,90));
                    for(int i=0;i<7;i++)
                    {
                        float a=i*Mathf.PI*2/7;
                        b.Beam(V(s*1.023f,.39f,wz),V(s*1.025f,.39f+Mathf.Cos(a)*.18f,wz+Mathf.Sin(a)*.18f),.029f,M.Chrome);
                    }
                }
                for(int j=0;j<2;j++) b.Box(V(s*.956f,.87f,-.26f+j*1.11f),V(.04f,.035f,.18f),M.Chrome,.008f);
                b.Box(V(s*1.045f,1.04f,-.5f),V(.2f,.13f,.23f),M.Olive,.035f);
                b.Box(V(s*1.06f,1.04f,-.37f),V(.16f,.095f,.012f),M.Glass,.005f);
                b.Box(V(s*.58f,.71f,-2.194f),V(.45f,.21f,.035f),M.Glow,.035f);
                b.Box(V(s*.64f,.78f,2.155f),V(.31f,.25f,.04f),M.Red,.025f);
            }
            for(int i=0;i<6;i++) b.Box(V(-.34f+i*.135f,.65f,-2.21f),V(.075f,.11f,.028f),M.Rubber,.004f);
            for(int z=-1;z<=1;z+=2)
            {
                b.Box(V(0,.45f,z*2.16f),V(1.64f,.1f,.1f),M.Chrome,.025f);
                b.Box(V(0,.64f,z*2.225f),V(.48f,.12f,.012f),M.Linen,.006f);
                b.Box(V(0,.64f,z*2.236f),V(.35f,.018f,.005f),M.Indigo,.001f);
            }
            // Door seams, fuel flap, wipers and roof rails make it read as a car at gameplay distance.
            b.Box(V(.958f,.81f,1.52f),V(.016f,.17f,.19f),M.DarkWood,.015f);
            for(int s=-1;s<=1;s+=2) b.Beam(V(s*.35f,1f,-.7f),V(s*.05f,1.15f,-.59f),.017f,M.Rubber);
            for(int s=-1;s<=1;s+=2) b.Beam(V(s*.55f,1.62f,-.16f),V(s*.55f,1.62f,.96f),.043f,M.Steel);
            b.Beam(V(.51f,1.6f,.8f),V(.55f,1.93f,.95f),.009f,M.Rubber);
        }

        /// <summary>Small cinematic shelter at local zero. Root may place this off-map without connecting gameplay paths.</summary>
        public static GameObject BuildBunkerInterior()
        {
            Init(); var root=new GameObject("HomeV3_BunkerInterior");
            var floor=Group(root,"Floor"); var back=Group(root,"BackWalls");
            var high=Group(root,"FrontHighWalls"); var roof=Group(root,"Roof");
            Solid(floor,"shelter_floor",V(0,-.1f,0),V(4,.2f,3),M.Concrete);
            Solid(back,"shelter_back",V(0,1.3f,1.5f),V(4,2.6f,.24f),M.Concrete);
            Solid(back,"shelter_west",V(-2,1.3f,0),V(.24f,2.6f,3),M.Concrete);
            Solid(high,"shelter_east",V(2,1.3f,0),V(.24f,2.6f,3),M.Concrete);
            Solid(high,"shelter_front_left",V(-1.25f,1.3f,-1.5f),V(1.5f,2.6f,.24f),M.Concrete);
            Solid(high,"shelter_front_right",V(1.25f,1.3f,-1.5f),V(1.5f,2.6f,.24f),M.Concrete);
            Solid(roof,"shelter_ceiling",V(0,2.72f,0),V(4,.2f,3),M.Concrete);
            var b=new Builder();
            b.Box(V(-1.55f,.46f,.15f),V(.5f,.09f,2.1f),M.Honey,.02f);
            b.Box(V(-1.79f,.75f,.15f),V(.055f,.52f,2.1f),M.Walnut,.012f);
            for(int z=-1;z<=1;z+=2) b.Box(V(-1.55f,.23f,.15f+z*.8f),V(.43f,.44f,.06f),M.Steel,.008f);
            b.Box(V(.95f,.52f,1.08f),V(1.32f,.065f,.57f),M.Honey,.01f);
            for(int s=-1;s<=1;s+=2) b.Box(V(.95f+s*.53f,.26f,1.08f),V(.045f,.52f,.42f),M.Steel,.005f);
            Radio(b,V(.7f,.56f,1.05f),.85f);
            for(int i=0;i<3;i++) Bottle(b,V(.45f+i*.24f,.03f,1),.95f,M.Teal);
            for(int i=0;i<2;i++)
            {
                b.Box(V(1.48f,.2f,-.55f+i*.52f),V(.65f,.4f,.46f),M.Olive,.018f);
                for(int j=0;j<3;j++) b.Box(V(1.48f,.065f+j*.12f,-.79f+i*.52f),V(.6f,.065f,.024f),M.Honey,.004f);
            }
            b.Box(V(.3f,1.9f,1.36f),V(.8f,.32f,.028f),M.Olive,.008f);
            Cross(b,V(.3f,1.9f,1.34f),.22f,.06f,M.Linen,false);
            b.Box(V(1.6f,1.12f,1.34f),V(.34f,.44f,.13f),M.Cream,.015f);
            Cross(b,V(1.6f,1.12f,1.265f),.23f,.06f,M.Red,false);
            b.Attach(root,"ShelterBenchSupplies");
            Place(root,Create("walllamp"),V(-.65f,1.65f,1.32f),0);
            Place(root,Create("walllamp"),V(1.35f,1.65f,1.32f),0);
            return root;
        }
        static void BunkerHatch(GameObject root,Builder b)
        {
            // Four-sided collar and recessed shaft stay open beneath the moving hatch.
            for(int s=-1;s<=1;s+=2)
            {
                b.Box(V(s*.775f,.07f,0),V(.15f,.14f,1.5f),M.Concrete,.025f);
                b.Box(V(0,.07f,s*.675f),V(1.4f,.14f,.15f),M.Concrete,.025f);
                b.Box(V(s*.705f,.15f,0),V(.065f,.075f,1.27f),M.Rubber,.01f);
                b.Box(V(0,.15f,s*.603f),V(1.35f,.075f,.065f),M.Rubber,.01f);
                b.Box(V(s*.68f,-.27f,0),V(.055f,.75f,1.2f),M.DarkWood,.004f);
                b.Box(V(0,-.27f,s*.58f),V(1.35f,.75f,.055f),M.DarkWood,.004f);
            }
            for(int i=0;i<3;i++)b.Beam(V(-.45f,-.05f-i*.22f,.52f),V(.45f,-.05f-i*.22f,.52f),.03f,M.Steel);
            for(int s=-1;s<=1;s+=2) for(int z=-1;z<=1;z+=2)
                b.Cylinder(V(s*.69f,.2f,z*.59f),.025f,.025f,M.Chrome,8);
            var leaf=Group(root,"DoorLeaf"); leaf.transform.localPosition=V(0,.195f,.57f);
            var d=new Builder();
            d.Box(V(0,.02f,-.57f),V(1.32f,.07f,1.13f),M.Olive,.025f);
            for(int i=0;i<4;i++) d.Box(V(-.45f+i*.3f,.06f,-.57f),V(.04f,.025f,1.04f),M.Steel,.005f);
            d.Torus(V(0,.15f,-.57f),.23f,.025f,M.Mustard,Quaternion.identity);
            d.Cylinder(V(0,.13f,-.57f),.045f,.13f,M.Steel,16);
            for(int i=0;i<4;i++)
            {
                float a=i*Mathf.PI*.5f; d.Beam(V(0,.15f,-.57f),V(Mathf.Cos(a)*.23f,.15f,-.57f+Mathf.Sin(a)*.23f),.025f,M.Mustard);
            }
            for(int s=-1;s<=1;s+=2) d.Cylinder(V(s*.46f,.02f,0),.055f,.2f,M.Steel,16,Quaternion.Euler(0,0,90));
            d.Attach(leaf,"HingedHatchWheel");
            b.Box(V(0,.148f,-.7f),V(.36f,.018f,.12f),M.Linen,.004f);
            Cross(b,V(0,.161f,-.7f),.09f,.025f,M.Olive,true);
        }
        static void ToolWall(Builder b)
        {
            b.Box(V(0,1.15f,0),V(1.7f,1.18f,.07f),M.Honey,.018f);
            for(int x=0;x<15;x++) for(int y=0;y<9;y++)
                b.Cylinder(V(-.75f+x*.105f,.67f+y*.12f,-.04f),.009f,.009f,M.DarkWood,8,Quaternion.Euler(90,0,0));
            for(int i=0;i<4;i++)
            {
                float x=-.61f+i*.36f;
                b.Beam(V(x,.85f,-.075f),V(x,1.36f,-.075f),.035f,i%2==0?M.Indigo:M.Terracotta);
                if(i==0) b.Box(V(x,1.4f,-.075f),V(.27f,.1f,.095f),M.Steel,.012f);
                else if(i==1) b.Torus(V(x,1.43f,-.075f),.07f,.022f,M.Steel,Quaternion.Euler(90,0,0));
                else if(i==2)
                {
                    b.Box(V(x,1.42f,-.075f),V(.08f,.13f,.035f),M.Steel,.004f);
                    b.Box(V(x,1.48f,-.075f),V(.11f,.025f,.035f),M.Steel,.002f);
                }
                else b.Cone(V(x,1.4f,-.075f),.035f,.008f,.18f,M.Steel,10);
            }
            b.Box(V(0,.56f,-.11f),V(1.8f,.07f,.3f),M.DarkWood,.01f);
            b.Box(V(.47f,.68f,-.11f),V(.32f,.18f,.19f),M.Indigo,.014f);
            Handle(b,V(.47f,.78f,-.11f),.18f,.06f,M.Steel);
        }
        static void Bicycle(Builder b)
        {
            Quaternion wheel=Quaternion.Euler(90,0,0); // Bicycle lies in XY plane: local front is +X.
            for(int s=-1;s<=1;s+=2)
            {
                var c=V(s*.62f,.36f,0);
                b.Torus(c,.32f,.035f,M.Rubber,wheel);
                b.Torus(c,.29f,.012f,M.Chrome,wheel);
                b.Cylinder(c,.045f,.13f,M.Steel,12,Quaternion.Euler(90,0,0));
                for(int i=0;i<12;i++)
                {
                    float a=i*Mathf.PI*2/12; b.Beam(c,c+V(Mathf.Cos(a)*.29f,Mathf.Sin(a)*.29f,0),.006f,M.Steel);
                }
            }
            Vector3 crank=V(-.12f,.38f,0),seat=V(-.27f,.86f,0),neck=V(.43f,.9f,0),rear=V(-.62f,.36f,0),front=V(.62f,.36f,0);
            foreach(var pair in new[]{new[]{rear,seat},new[]{seat,crank},new[]{crank,rear},new[]{seat,neck},new[]{crank,neck},new[]{neck,front}})
                b.Beam(pair[0],pair[1],.037f,M.Terracotta);
            b.Beam(seat,V(-.29f,.99f,0),.028f,M.Chrome);
            b.Box(V(-.31f,1.02f,0),V(.28f,.07f,.17f),M.Rubber,.025f);
            b.Beam(neck,V(.39f,1.08f,0),.027f,M.Chrome);
            b.Beam(V(.39f,1.08f,-.23f),V(.39f,1.08f,.23f),.026f,M.Chrome);
            for(int s=-1;s<=1;s+=2) b.Cylinder(V(.39f,1.08f,s*.22f),.027f,.15f,M.Rubber,12,Quaternion.Euler(90,0,0));
            b.Torus(crank,.11f,.017f,M.Chrome,wheel);
            b.Beam(crank+V(0,0,-.09f),crank+V(.12f,.02f,-.09f),.02f,M.Steel);
            b.Box(crank+V(.14f,.02f,-.12f),V(.1f,.035f,.14f),M.Rubber,.006f);
            b.Beam(crank+V(-.05f,0,0),V(-.62f,.36f,.012f),.012f,M.Rubber);
            b.Beam(V(-.2f,.4f,.01f),V(-.3f,.04f,.17f),.018f,M.Steel);
            Cable(b,new[]{V(.39f,1.08f,-.18f),V(.52f,.95f,-.12f),V(.5f,.67f,-.03f),front},.007f,M.Rubber);
        }
        static void Books(Builder b,Vector3 p,float s)
        {
            M[] colors={M.Indigo,M.Terracotta,M.Olive,M.Mustard,M.Teal};
            for(int i=0;i<5;i++)
            {
                float h=.22f+(i%3)*.04f,x=-.17f+i*.083f;
                b.Box(p+V(x,h*.5f,.015f)*s,V(.07f,h,.18f)*s,colors[i],.003f*s);
                b.Box(p+V(x,h*.5f,.105f)*s,V(.055f,h-.02f,.005f)*s,M.Linen,.001f);
                for(int j=0;j<2;j++) b.Box(p+V(x,.05f+j*(h-.1f),-.08f)*s,V(.054f,.009f,.006f)*s,M.Honey,.001f);
            }
        }
        static void Plant(Builder b,Vector3 p,float s,bool pot)
        {
            float baseY=pot?.36f:0;
            if(pot)
            {
                b.Cone(p+V(0,.17f,0)*s,.16f*s,.23f*s,.34f*s,M.Terracotta,24);
                b.Torus(p+V(0,.335f,0)*s,.229f*s,.018f*s,M.Honey,Quaternion.identity);
                b.Cylinder(p+V(0,.335f,0)*s,.213f*s,.02f*s,M.DarkWood,24);
            }
            b.Beam(p+V(0,baseY,0)*s,p+V(.02f,baseY+.82f,0)*s,.018f*s,M.Walnut);
            for(int i=0;i<9;i++)
            {
                float a=i*2.4f,h=baseY+.17f+i*.07f;
                Vector3 tip=V(Mathf.Cos(a)*(.22f+i*.009f),h+.1f,Mathf.Sin(a)*(.22f+i*.009f));
                b.Beam(p+V(0,h,0)*s,p+tip*s,.009f*s,M.Leaf);
                b.Leaf(p+(tip+V(0,.03f,0))*s,V(Mathf.Cos(a)*.23f,.12f,Mathf.Sin(a)*.23f)*s,i%2==0?M.Olive:M.Leaf);
            }
        }
        static void ShoeRack(Builder b)
        {
            Legs(b,1.05f,.32f,.58f,M.DarkWood,.045f);
            for(int level=0;level<3;level++)
            {
                float y=.08f+level*.23f; b.Box(V(0,y,0),V(1.18f,.035f,.42f),M.Honey,.007f);
                for(int i=0;i<4;i++)
                {
                    b.Ellipsoid(V(-.43f+i*.28f,y+.06f,-.02f),V(.08f,.055f,.16f),i%2==0?M.Indigo:M.Terracotta);
                    b.Box(V(-.43f+i*.28f,y+.027f,-.02f),V(.16f,.018f,.33f),M.Cream,.016f);
                    for(int k=0;k<3;k++) b.Box(V(-.43f+i*.28f,y+.11f,-.03f+k*.025f),V(.07f,.004f,.005f),M.Linen,.001f);
                }
            }
        }
        static void CoatRack(Builder b)
        {
            b.Cylinder(V(0,.05f,0),.33f,.09f,M.DarkWood,24);
            b.Cylinder(V(0,.94f,0),.041f,1.8f,M.Walnut,16);
            for(int i=0;i<4;i++)
            {
                float a=i*Mathf.PI*.5f;
                b.Beam(V(0,1.61f,0),V(Mathf.Cos(a)*.28f,1.83f,Mathf.Sin(a)*.28f),.026f,M.Honey);
                b.Ellipsoid(V(Mathf.Cos(a)*.28f,1.83f,Mathf.Sin(a)*.28f),V(.046f,.043f,.046f),M.Honey);
            }
            b.Box(V(-.1f,1.35f,.19f),V(.35f,.72f,.1f),M.Indigo,.045f);
            for(int s=-1;s<=1;s+=2) b.Box(V(-.1f+s*.21f,1.45f,.18f),V(.13f,.44f,.11f),M.Indigo,.035f,Quaternion.Euler(0,0,s*16));
            b.Box(V(.19f,1.4f,-.06f),V(.14f,.83f,.07f),M.Mustard,.015f);
            for(int i=0;i<3;i++) b.Box(V(.19f,1.12f+i*.08f,-.104f),V(.14f,.025f,.006f),M.Terracotta,.002f);
        }
        static void Curtains(Builder b)
        {
            b.Cylinder(V(0,2.4f,.04f),.025f,2.15f,M.DarkWood,16,Quaternion.Euler(0,0,90));
            for(int s=-1;s<=1;s+=2)
            {
                for(int i=0;i<8;i++)
                {
                    float x=s*(.62f+i*.047f);
                    b.Cylinder(V(x,1.43f,-.03f+(i%2)*.027f),.043f,1.82f,i%2==0?M.Terracotta:M.Linen,10);
                    b.Torus(V(x,2.38f,.04f),.041f,.008f,M.Steel,Quaternion.Euler(90,0,0));
                }
                b.Box(V(s*.8f,.92f,-.08f),V(.39f,.065f,.1f),M.Mustard,.012f);
                b.Ellipsoid(V(s*1.13f,2.4f,.04f),V(.055f,.055f,.055f),M.Honey);
            }
        }
        static void WallArt(Builder b)
        {
            b.Box(V(0,.44f,0),V(1.06f,.88f,.065f),M.DarkWood,.012f);
            b.Box(V(0,.44f,-.04f),V(.92f,.74f,.016f),M.Linen,.003f);
            b.Box(V(0,.44f,-.052f),V(.8f,.62f,.008f),M.Indigo,.002f);
            b.Cylinder(V(.18f,.6f,-.061f),.105f,.008f,M.Mustard,32,Quaternion.Euler(90,0,0));
            b.Quad(V(-.4f,.15f,-.062f),V(-.4f,.32f,-.062f),V(-.1f,.49f,-.062f),V(.3f,.15f,-.062f),M.Terracotta);
            b.Quad(V(-.17f,.15f,-.065f),V(.11f,.39f,-.065f),V(.4f,.3f,-.065f),V(.4f,.15f,-.065f),M.Olive);
            for(int i=0;i<4;i++) b.Box(V(-.2f+i*.15f,.2f,-.071f),V(.06f,.008f,.005f),M.Linen,.001f);
        }
        static void GarageDoor(GameObject root,Builder b)
        {
            for(int s=-1;s<=1;s+=2) b.Box(V(s*1.54f,1.22f,0),V(.08f,2.44f,.18f),M.Steel,.01f);
            b.Box(V(0,2.43f,0),V(3.16f,.08f,.18f),M.Steel,.01f);
            // Local root origin is at ground, centered in aperture. Move GarageDoor upward 2.4m to open.
            var moving=Group(root,"GarageDoor"); var leaf=Group(moving,"DoorLeaf");
            leaf.transform.localPosition=V(0,2.36f,0);
            var d=new Builder();
            for(int i=0;i<7;i++)
            {
                float y=-2.18f+i*.335f;
                d.Box(V(0,y,0),V(2.97f,.324f,.085f),i%2==0?M.Indigo:M.Teal,.012f);
                for(int j=0;j<5;j++) d.Box(V(-1.2f+j*.6f,y,-.048f),V(.51f,.21f,.012f),M.Indigo,.007f);
                for(int s=-1;s<=1;s+=2) d.Box(V(s*1.34f,y,.054f),V(.08f,.11f,.03f),M.Steel,.004f);
            }
            Handle(d,V(0,-1.83f,-.07f),.32f,.06f,M.Chrome);
            d.Box(V(0,-.12f,-.05f),V(.48f,.15f,.02f),M.Linen,.008f);
            d.Attach(leaf,"SegmentedDoor");
        }
        static void Sofa(Builder b,M fabric)
        {
            for(int s=-1;s<=1;s+=2) for(int z=-1;z<=1;z+=2) b.Box(V(s*1.04f,.12f,z*.36f),V(.1f,.22f,.1f),M.DarkWood,.015f);
            b.Box(V(0,.3f,0),V(2.35f,.3f,.97f),fabric,.08f);
            b.Box(V(0,.77f,.36f),V(2.31f,.74f,.23f),fabric,.08f,Quaternion.Euler(-7,0,0));
            for(int s=-1;s<=1;s+=2) b.Box(V(s*1.12f,.61f,0),V(.26f,.54f,1),fabric,.085f);
            for(int i=0;i<3;i++)
            {
                b.Box(V(-.7f+i*.7f,.52f,-.07f),V(.67f,.17f,.76f),fabric,.065f);
                b.Box(V(-.7f+i*.7f,.76f,.215f),V(.65f,.5f,.17f),fabric,.065f,Quaternion.Euler(-8,0,0));
                b.Box(V(-.7f+i*.7f,.605f,-.41f),V(.58f,.009f,.012f),M.Linen,.003f);
            }
            b.Box(V(-.79f,.83f,.04f),V(.39f,.39f,.16f),M.Terracotta,.055f,Quaternion.Euler(-8,0,-14));
            b.Box(V(.78f,.83f,.04f),V(.39f,.39f,.16f),M.Mustard,.055f,Quaternion.Euler(-8,0,13));
            b.Box(V(.54f,.633f,-.11f),V(.47f,.034f,.7f),M.Indigo,.014f);
            for(int i=0;i<7;i++) b.Box(V(.35f+i*.06f,.652f,-.11f),V(.016f,.004f,.67f),M.Linen,.001f);
            for(int i=0;i<7;i++) b.Box(V(.35f+i*.06f,.53f,-.48f),V(.014f,.2f,.025f),M.Indigo,.004f);
        }
        static void Armchair(Builder b,M fabric)
        {
            Legs(b,.66f,.61f,.21f,M.DarkWood,.07f);
            b.Box(V(0,.38f,0),V(.92f,.28f,.86f),fabric,.075f);
            b.Box(V(0,.79f,.3f),V(.9f,.72f,.2f),fabric,.075f,Quaternion.Euler(-9,0,0));
            for(int s=-1;s<=1;s+=2) b.Box(V(s*.42f,.59f,0),V(.2f,.4f,.9f),fabric,.065f);
            b.Box(V(0,.57f,-.06f),V(.67f,.16f,.66f),M.Mustard,.06f);
            b.Box(V(0,.87f,.17f),V(.4f,.36f,.15f),M.Indigo,.05f);
        }
        static void Chair(Builder b,M fabric)
        {
            Legs(b,.42f,.42f,.46f,M.DarkWood,.045f);
            b.Box(V(0,.47f,0),V(.55f,.07f,.55f),M.Honey,.02f);
            b.Box(V(0,.53f,0),V(.49f,.08f,.49f),fabric,.03f);
            for(int s=-1;s<=1;s+=2) b.Beam(V(s*.22f,.45f,.22f),V(s*.22f,1.04f,.28f),.045f,M.Walnut);
            for(int i=0;i<3;i++) b.Box(V(0,.72f+i*.13f,.255f),V(.49f,.075f,.055f),M.Honey,.012f);
        }
        static void SideTable(Builder b)
        {
            b.Cylinder(V(0,.54f,0),.36f,.055f,M.Honey,32);
            for(int i=0;i<3;i++)
            {
                float a=i*Mathf.PI*2/3;
                b.Beam(V(Mathf.Cos(a)*.2f,.51f,Mathf.Sin(a)*.2f),V(Mathf.Cos(a)*.27f,.02f,Mathf.Sin(a)*.27f),.047f,M.DarkWood);
            }
            b.Cylinder(V(.06f,.61f,-.02f),.045f,.09f,M.Ceramic,20);
            b.Torus(V(.12f,.61f,-.02f),.035f,.009f,M.Ceramic,Quaternion.Euler(90,0,0));
            b.Box(V(-.1f,.579f,.08f),V(.22f,.023f,.29f),M.Indigo,.004f);
        }
        static void Bed(Builder b)
        {
            Legs(b,1.35f,1.9f,.27f,M.DarkWood,.09f);
            b.Box(V(0,.28f,0),V(1.68f,.22f,2.16f),M.Walnut,.025f);
            b.Box(V(0,.51f,0),V(1.59f,.28f,2.07f),M.Linen,.08f);
            b.Box(V(0,.69f,-.28f),V(1.62f,.1f,1.52f),M.Indigo,.04f);
            b.Box(V(0,.748f,.35f),V(1.64f,.035f,.24f),M.Teal,.01f);
            for(int i=0;i<12;i++) b.Box(V(-.73f+i*.13f,.748f,-.45f),V(.018f,.005f,1.12f),M.Linen,.001f);
            for(int s=-1;s<=1;s+=2) b.Box(V(s*.38f,.72f,.73f),V(.65f,.19f,.42f),s<0?M.Cream:M.Mustard,.075f);
            b.Box(V(0,.76f,1.09f),V(1.78f,1.1f,.12f),M.Honey,.04f);
            for(int i=0;i<8;i++) b.Box(V(-.71f+i*.2f,.82f,1.02f),V(.05f,.81f,.025f),M.Walnut,.006f);
        }
        static void Rug(Builder b)
        {
            b.Box(V(0,.016f,0),V(3.4f,.03f,2.25f),M.Linen,.025f);
            b.Box(V(0,.035f,0),V(3.16f,.009f,2.01f),M.Terracotta,.008f);
            b.Box(V(0,.043f,0),V(2.9f,.006f,1.75f),M.Indigo,.005f);
            for(int x=-1;x<=1;x++)
            {
                b.Box(V(x*.83f,.05f,0),V(.5f,.006f,.5f),M.Mustard,.002f,Quaternion.Euler(0,45,0));
                b.Box(V(x*.83f,.055f,0),V(.27f,.005f,.27f),M.Teal,.002f,Quaternion.Euler(0,45,0));
            }
            for(int i=0;i<33;i++) for(int s=-1;s<=1;s+=2) b.Box(V(-1.6f+i*.1f,.02f,s*1.18f),V(.016f,.014f,.17f),M.Linen,.003f);
            for(int i=0;i<20;i++) b.Box(V(0,.059f,-.87f+i*.092f),V(2.88f,.002f,.004f),M.Linen,.001f);
        }
        static void LaundryBasket(Builder b)
        {
            b.Cone(V(0,.24f,0),.25f,.31f,.47f,M.Honey,24);
            for(int i=0;i<18;i++)
            {
                float a=i*Mathf.PI*2/18;
                b.Beam(V(Mathf.Cos(a)*.251f,.025f,Mathf.Sin(a)*.251f),V(Mathf.Cos(a)*.31f,.47f,Mathf.Sin(a)*.31f),.013f,M.DarkWood);
            }
            for(int i=0;i<7;i++) b.Torus(V(0,.04f+i*.065f,0),.255f+i*.008f,.009f,M.Linen,Quaternion.identity);
            b.Torus(V(0,.48f,0),.31f,.023f,M.Honey,Quaternion.identity);
            b.Ellipsoid(V(-.05f,.47f,0),V(.26f,.075f,.24f),M.Indigo);
            b.Box(V(.08f,.43f,-.24f),V(.21f,.27f,.055f),M.Terracotta,.03f);
        }
        static void PendantLamp(Builder b)
        {
            b.Cylinder(V(0,.96f,0),.012f,1.05f,M.Rubber,10);
            b.Cone(V(0,.36f,0),.36f,.08f,.32f,M.Indigo,32);
            b.Cone(V(0,.355f,0),.34f,.07f,.3f,M.Linen,32);
            b.Torus(V(0,.2f,0),.35f,.012f,M.Mustard,Quaternion.identity);
            b.Ellipsoid(V(0,.26f,0),V(.08f,.07f,.08f),M.Glow);
        }

        /// <summary>Cached original raster illustrations. Textures stay readable for root-owned baking.</summary>
        public static Texture2D Icon(string supplyId)
        {
            string id=(supplyId??"water").ToLowerInvariant(); Texture2D cached;
            if(icons.TryGetValue(id,out cached)&&cached) return cached;
            var c=new IconCanvas(96);
            Color ink=new Color(.1f,.15f,.18f), cream=new Color(.96f,.91f,.78f), red=new Color(.85f,.25f,.19f),
                teal=new Color(.23f,.67f,.67f), gold=new Color(.93f,.66f,.25f), olive=new Color(.49f,.64f,.3f),
                blue=new Color(.29f,.4f,.67f), steel=new Color(.7f,.76f,.77f), brown=new Color(.52f,.31f,.18f);
            // Transparent corners allow use on both dark HUD cards and world selection panels.
            c.RoundRect(4,5,92,92,14,new Color(.03f,.06f,.08f,.42f));
            c.RoundRect(5,3,91,89,13,new Color(.16f,.2f,.22f,.96f));
            switch(id)
            {
                case "water": case "antiseptic":
                    c.RoundRect(29,25,67,79,8,id=="water"?teal:brown);
                    c.Rect(36,17,60,30,id=="water"?teal:brown); c.RoundRect(34,12,62,23,3,id=="water"?blue:cream);
                    c.Rect(30,42,66,65,cream); c.Rect(34,30,38,40,new Color(.7f,.91f,.9f));
                    if(id=="water") {c.Circle(48,53,9,teal);c.Polygon(new[]{48f,38,40,51,56,51},teal);}
                    else {c.Rect(44,45,52,62,red);c.Rect(39,50,57,57,red);} break;
                case "food":
                    c.RoundRect(25,24,71,77,7,red); c.Rect(25,27,71,32,steel); c.Rect(25,70,71,76,steel);
                    c.RoundRect(31,36,65,65,3,gold); c.Circle(48,50,10,olive); c.Line(43,49,55,56,3,cream);
                    c.Circle(48,22,8,steel);c.Circle(48,22,4,ink);break;
                case "bandage":
                    c.RoundRect(18,35,65,68,7,cream);c.Rect(38,59,75,76,cream);c.Circle(27,51,16,cream);c.Circle(27,51,7,brown);
                    for(int i=0;i<5;i++)c.Line(40+i*6,40,40+i*6,64,1,new Color(.72f,.68f,.57f));break;
                case "firstaid":
                    c.RoundRect(17,29,79,75,7,cream);c.RoundRect(35,16,61,34,5,steel);c.Rect(41,22,55,29,ink);
                    c.Rect(18,33,78,39,red);c.Rect(43,44,53,66,red);c.Rect(37,50,59,60,red);break;
                case "flashlight":
                    c.RoundRect(36,37,60,79,4,blue);c.Polygon(new[]{31f,34,36,50,60,50,65,34},ink);
                    c.Rect(30,23,66,35,steel);c.Rect(34,20,62,25,gold);c.Rect(43,48,53,57,gold);
                    for(int i=0;i<4;i++)c.Rect(37,62+i*4,59,64+i*4,ink);break;
                case "battery":
                    for(int i=0;i<2;i++){int x=25+i*27;c.RoundRect(x,24,x+20,76,4,olive);c.Rect(x,24,x+20,39,gold);c.Rect(x+6,18,x+14,24,steel);c.Rect(x+8,45,x+12,59,cream);c.Rect(x+3,50,x+17,54,cream);}break;
                case "radio":
                    c.Line(26,32,19,11,3,steel);c.RoundRect(15,31,81,77,7,red);c.RoundRect(21,38,53,70,4,ink);
                    for(int i=0;i<7;i++)c.Rect(23,41+i*4,51,42+i*4,brown);
                    c.Rect(59,39,74,48,cream);c.Circle(62,60,5,steel);c.Circle(74,60,4,gold);
                    c.Line(33,30,33,23,4,steel);c.Line(33,23,66,23,4,steel);c.Line(66,23,66,30,4,steel);break;
                case "blanket":
                    for(int i=0;i<3;i++)c.RoundRect(18,31+i*12,78,48+i*12,5,i%2==0?blue:teal);
                    for(int i=0;i<7;i++)c.Rect(24+i*8,32,27+i*8,70,cream);
                    for(int i=0;i<8;i++)c.Rect(21+i*8,71,24+i*8,79,gold);break;
                case "screws":
                    // Two oversized diagonal screws remain distinct in a 32px toolbox slot.
                    c.RoundRect(13,51,83,79,4,gold);c.Rect(20,56,76,73,brown);
                    c.Rect(40,58,58,71,cream);c.Line(43,64,55,64,3,ink);
                    c.Line(28,25,47,60,9,ink);c.Line(29,24,48,59,6,steel);
                    c.Line(19,27,38,17,11,ink);c.Line(20,26,37,18,7,steel);
                    c.Line(24,25,33,20,2,ink);
                    c.Line(34,34,42,32,2,ink);c.Line(38,43,46,41,2,ink);c.Line(43,52,50,50,2,ink);
                    c.Line(60,26,69,58,9,ink);c.Line(60,25,69,57,6,steel);
                    c.Line(50,27,70,22,11,ink);c.Line(51,26,69,22,7,steel);c.Line(55,26,65,23,2,ink);
                    c.Line(60,35,68,33,2,ink);c.Line(62,43,70,41,2,ink);c.Line(64,51,72,49,2,ink);break;
                case "bracket":
                    // Warm highlights and four open bolt holes describe a bent metal angle.
                    c.Polygon(new[]{20f,15,44,15,44,58,82,58,82,81,20,81},ink);
                    c.Polygon(new[]{24f,18,41,18,41,62,78,62,78,77,24,77},steel);
                    c.Rect(24,18,29,75,cream);c.Rect(29,62,77,66,cream);
                    c.Circle(33,30,5,ink);c.Circle(33,48,5,ink);c.Circle(53,70,4,ink);c.Circle(69,70,4,ink);
                    c.Line(41,59,48,66,3,gold);break;
                case "plank":
                    for(int i=0;i<3;i++)
                    {
                        int x=12+i*4,y=20+i*20;
                        c.RoundRect(x,y,x+65,y+17,2,ink);c.Rect(x+2,y+2,x+63,y+15,i==1?gold:brown);
                        c.Line(x+8,y+7,x+53,y+7,2,cream);c.Line(x+19,y+12,x+59,y+12,1,ink);
                        c.Circle(x+11,y+12,2,ink);c.Rect(x+60,y+3,x+63,y+14,cream);
                    }break;
                case "sandbag":
                    c.RoundRect(16,31,80,75,15,cream);c.Line(24,33,72,33,4,brown);
                    for(int i=0;i<9;i++)c.Line(27+i*5,29,29+i*5,37,1,gold);
                    c.Line(22,61,26,46,2,gold);c.Line(74,61,70,46,2,gold);break;
                case "seedling":
                    c.Circle(48,70,14,brown);c.Line(48,65,48,23,4,olive);
                    c.Polygon(new[]{47f,44,22,27,24,47,47,54},olive);c.Polygon(new[]{49f,34,70,17,73,36,49,45},teal);c.Polygon(new[]{49f,56,73,41,76,59,49,65},olive);break;
                case "hose":
                    for(int i=0;i<3;i++){c.Ring(46,51,28-i*7,4,olive);}c.Line(46,79,73,70,5,olive);c.Line(73,70,80,57,7,gold);break;
                case "extinguisher":
                    c.RoundRect(32,31,63,79,9,red);c.Rect(40,23,55,34,steel);c.Rect(44,18,69,25,ink);
                    c.Rect(36,47,59,67,cream);c.Rect(45,50,51,63,red);c.Rect(41,54,55,60,red);
                    c.Line(64,24,73,33,4,ink);c.Line(73,33,73,69,4,ink);break;
                default:c.Circle(48,48,24,gold);c.Circle(48,48,13,olive);break;
            }
            var texture=new Texture2D(96,96,TextureFormat.RGBA32,false){name="HomeV4_Icon_"+id,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            texture.SetPixels(c.Pixels); texture.Apply(false,false); icons[id]=texture; return texture;
        }

        static Vector3 V(float x,float y,float z){return new Vector3(x,y,z);}
        static GameObject Group(GameObject parent,string name)
        {
            var go=new GameObject(name);go.transform.SetParent(parent.transform,false);return go;
        }
        static void Place(GameObject parent,GameObject child,Vector3 p,float yaw)
        {
            child.transform.SetParent(parent.transform,false);child.transform.localPosition=p;child.transform.localRotation=Quaternion.Euler(0,yaw,0);
        }
        static void Solid(GameObject parent,string name,Vector3 p,Vector3 size,M m)
        {
            var go=Group(parent,"solid_"+name);go.transform.localPosition=p;
            go.AddComponent<MeshFilter>().sharedMesh=BoxMesh(size,.004f);
            go.AddComponent<MeshRenderer>().sharedMaterial=palette[(int)m];
        }
        static void Legs(Builder b,float width,float depth,float h,M m,float thick)
        {
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)b.Box(V(x*width*.5f,h*.5f,z*depth*.5f),V(thick,h,thick),m,.008f);
        }
        static void Handle(Builder b,Vector3 p,float w,float h,M m)
        {
            b.Beam(p+V(-w*.5f,0,0),p+V(-w*.5f,h,-.025f),.016f,m);
            b.Beam(p+V(w*.5f,0,0),p+V(w*.5f,h,-.025f),.016f,m);
            b.Beam(p+V(-w*.5f,h,-.025f),p+V(w*.5f,h,-.025f),.018f,m);
        }
        static void Cable(Builder b,Vector3[] points,float width,M m)
        {for(int i=1;i<points.Length;i++)b.Beam(points[i-1],points[i],width,m);}
        static void Cross(Builder b,Vector3 p,float s,float thickness,M m,bool horizontal)
        {
            b.Box(p,horizontal?V(s,.008f,thickness):V(s,thickness,.008f),m,.001f);
            b.Box(p,horizontal?V(thickness,.008f,s):V(thickness,s,.008f),m,.001f);
        }
        static void Faucet(Builder b,Vector3 p,float h)
        {
            b.Beam(p,p+V(0,h,0),.027f,M.Chrome);b.Beam(p+V(0,h,0),p+V(0,h,-.15f),.027f,M.Chrome);
            b.Cylinder(p+V(0,h-.02f,-.15f),.024f,.045f,M.Chrome,16);
            b.Box(p+V(0,h+.025f,0),V(.1f,.023f,.026f),M.Chrome,.005f);
        }
        static void Bottle(Builder b,Vector3 p,float s,M label)
        {
            b.Cylinder(p+V(0,.135f,0)*s,.065f*s,.23f*s,M.Teal,20);
            b.Cone(p+V(0,.265f,0)*s,.065f*s,.026f*s,.06f*s,M.Teal,20);
            b.Cylinder(p+V(0,.31f,0)*s,.027f*s,.04f*s,M.Indigo,16);
            b.Cylinder(p+V(0,.135f,0)*s,.067f*s,.087f*s,label,20);
            b.Box(p+V(0,.135f,-.069f)*s,V(.067f,.06f,.005f)*s,M.Linen,.002f);
            for(int i=0;i<3;i++)b.Torus(p+V(0,.04f+i*.024f,0)*s,.064f*s,.003f*s,M.Cream,Quaternion.identity);
        }
        static void Init()
        {
            if(palette!=null&&palette.Length>0&&palette[0])return;
            Color[] colors={new Color(.34f,.22f,.13f),new Color(.63f,.43f,.23f),new Color(.16f,.12f,.095f),
                new Color(.76f,.73f,.64f),new Color(.37f,.43f,.24f),new Color(.18f,.32f,.2f),
                new Color(.65f,.31f,.22f),new Color(.21f,.27f,.43f),new Color(.2f,.45f,.44f),
                new Color(.83f,.78f,.66f),new Color(.8f,.56f,.22f),new Color(.68f,.2f,.15f),
                new Color(.4f,.44f,.44f),new Color(.65f,.71f,.7f),new Color(.075f,.09f,.095f),
                new Color(.85f,.85f,.77f),new Color(.19f,.32f,.35f,.72f),new Color(.98f,.74f,.36f),
                new Color(.21f,.45f,.49f),new Color(.45f,.45f,.4f),new Color(.42f,.3f,.23f),new Color(.66f,.69f,.59f),
                new Color(.105f,.17f,.23f),new Color(.34f,.61f,.71f),new Color(.94f,.9f,.79f),
                new Color(.78f,.32f,.2f),new Color(.52f,.69f,.56f)};
            Shader shader=Shader.Find("Standard");if(!shader)shader=Shader.Find("Legacy Shaders/Diffuse");
            palette=new Material[colors.Length];
            for(int i=0;i<colors.Length;i++)
            {
                var mat=new Material(shader){name="HomeV4_"+(M)i,color=colors[i]};
                bool metal=i==(int)M.Steel||i==(int)M.Chrome;
                if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",metal?.72f:0);
                if(mat.HasProperty("_Glossiness"))mat.SetFloat("_Glossiness",i==(int)M.Chrome?.64f:i==(int)M.Ceramic?.51f:i==(int)M.Glass?.62f:.23f);
                if(i==(int)M.Glow||i==(int)M.Screen)
                {mat.EnableKeyword("_EMISSION");if(mat.HasProperty("_EmissionColor"))mat.SetColor("_EmissionColor",colors[i]*(i==(int)M.Glow?1.2f:.45f));}
                if(i==(int)M.Glass)
                {
                    mat.SetFloat("_Mode",3);mat.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);mat.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite",0);mat.EnableKeyword("_ALPHABLEND_ON");mat.renderQueue=3000;
                }
                palette[i]=mat;
            }
        }

        sealed class Builder
        {
            readonly Dictionary<int,List<CombineInstance>> batches=new Dictionary<int,List<CombineInstance>>();
            void Add(M m,Mesh mesh,Vector3 p,Quaternion q,Vector3 scale)
            {
                List<CombineInstance> list;if(!batches.TryGetValue((int)m,out list)){list=new List<CombineInstance>();batches[(int)m]=list;}
                list.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,q,scale)});
            }
            public void Box(Vector3 p,Vector3 size,M m,float bevel=.012f,Quaternion rotation=default(Quaternion))
            {if(rotation==default(Quaternion))rotation=Quaternion.identity;Add(m,BoxMesh(size,bevel),p,rotation,Vector3.one);}
            public void Cylinder(Vector3 p,float r,float h,M m,int sides=16,Quaternion rotation=default(Quaternion))
            {
                if(rotation==default(Quaternion))rotation=Quaternion.identity;
                Add(m,TurnedMesh(1,1,sides),p,rotation,V(r,h,r));
            }
            public void Cone(Vector3 p,float bottom,float top,float h,M m,int sides)
            {Add(m,TurnedMesh(bottom,top,sides),p,Quaternion.identity,V(1,h,1));}
            public void Beam(Vector3 a,Vector3 z,float w,M m)
            {
                Vector3 delta=z-a;if(delta.sqrMagnitude<.000001f)return;
                Box((a+z)*.5f,V(w,delta.magnitude,w),m,Mathf.Min(w*.18f,.004f),Quaternion.FromToRotation(Vector3.up,delta.normalized));
            }
            public void Ellipsoid(Vector3 p,Vector3 radii,M m)
            {Add(m,SphereMesh(),p,Quaternion.identity,radii);}
            public void Torus(Vector3 p,float radius,float tube,M m,Quaternion rotation)
            {Add(m,TorusMesh(radius,tube),p,rotation,Vector3.one);}
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,M m)
            {Add(m,FinishMesh("ResidencePanel",new[]{a,b,c,d,a,b,c,d},new[]{0,1,2,0,2,3,4,6,5,4,7,6}),Vector3.zero,Quaternion.identity,Vector3.one);}
            public void Prism(Vector3[] v,M m)
            {
                int[] t={0,3,2,0,2,1,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,3,7,6,3,6,2,0,1,5,0,5,4};
                Add(m,FinishMesh("ResidenceShapedShell",v,t),Vector3.zero,Quaternion.identity,Vector3.one);
            }
            public void Leaf(Vector3 p,Vector3 direction,M m)
            {
                Vector3 end=p+direction,side=Vector3.Cross(direction.normalized,Vector3.up)*direction.magnitude*.3f;
                Vector3 mid=p+direction*.48f+V(0,.035f,0);
                var v=new[]{p,mid+side,end,mid-side,mid+V(0,.035f,0)};
                Add(m,FinishMesh("ResidenceFoldedLeaf",v,new[]{0,1,4,1,2,4,2,3,4,3,0,4,4,1,0,4,2,1,4,3,2,4,0,3}),Vector3.zero,Quaternion.identity,Vector3.one);
            }
            public void Attach(GameObject parent,string name)
            {
                foreach(var pair in batches)
                {
                    string key=name+"_"+pair.Key;Mesh mesh;
                    if(!combined.TryGetValue(key,out mesh)||!mesh)
                    {
                        mesh=new Mesh{name="HomeV3_"+key};int count=0;foreach(var part in pair.Value)count+=part.mesh.vertexCount;
                        if(count>65000)mesh.indexFormat=IndexFormat.UInt32;
                        mesh.CombineMeshes(pair.Value.ToArray(),true,true,false);mesh.RecalculateBounds();combined[key]=mesh;
                    }
                    var g=Group(parent,name+"_"+(M)pair.Key);g.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=g.AddComponent<MeshRenderer>();renderer.sharedMaterial=palette[pair.Key];
                    if(pair.Key==(int)M.Glass)renderer.shadowCastingMode=ShadowCastingMode.Off;
                }
            }
        }
        static Mesh BoxMesh(Vector3 size,float radius)
        {
            string key="box_"+size.ToString("R")+"_"+radius.ToString("R",System.Globalization.CultureInfo.InvariantCulture);Mesh cached;
            if(shapes.TryGetValue(key,out cached)&&cached)return cached;
            Vector3 h=size*.5f;float r=Mathf.Clamp(radius,.00001f,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.7f);Vector3 inner=h-Vector3.one*r;
            var verts=new List<Vector3>();var tris=new List<int>();
            Action<Vector3[],Vector3> face=(points,n)=>
            {
                int start=verts.Count;verts.AddRange(points);bool forward=Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),n)>0;
                for(int i=1;i<points.Length-1;i++){tris.Add(start);tris.Add(start+(forward?i:i+1));tris.Add(start+(forward?i+1:i));}
            };
            int[] aSign={-1,1,1,-1},bSign={-1,-1,1,1};
            for(int axis=0;axis<3;axis++)for(int s=-1;s<=1;s+=2)
            {
                int a=(axis+1)%3,b=(axis+2)%3;var points=new Vector3[4];
                for(int i=0;i<4;i++){Vector3 p=Vector3.zero;p[axis]=h[axis]*s;p[a]=inner[a]*aSign[i];p[b]=inner[b]*bSign[i];points[i]=p;}
                Vector3 n=Vector3.zero;n[axis]=s;face(points,n);
            }
            for(int axis=0;axis<3;axis++)for(int s=-1;s<=1;s+=2)for(int t=-1;t<=1;t+=2)
            {
                int a=(axis+1)%3,b=(axis+2)%3;Vector3 p0=Vector3.zero,p1=Vector3.zero,p2=Vector3.zero,p3=Vector3.zero;
                p0[axis]=p3[axis]=-inner[axis];p1[axis]=p2[axis]=inner[axis];
                p0[a]=p1[a]=h[a]*s;p0[b]=p1[b]=inner[b]*t;p2[a]=p3[a]=inner[a]*s;p2[b]=p3[b]=h[b]*t;
                Vector3 n=Vector3.zero;n[a]=s;n[b]=t;face(new[]{p0,p1,p2,p3},n);
            }
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                face(new[]{V(h.x*x,inner.y*y,inner.z*z),V(inner.x*x,h.y*y,inner.z*z),V(inner.x*x,inner.y*y,h.z*z)},V(x,y,z));
            cached=FinishMesh("ResidenceBeveledJoinery",verts.ToArray(),tris.ToArray());shapes[key]=cached;return cached;
        }
        static Mesh TurnedMesh(float bottom,float top,int sides)
        {
            string key="turned_"+bottom.ToString("R")+"_"+top.ToString("R")+"_"+sides;Mesh cached;
            if(shapes.TryGetValue(key,out cached)&&cached)return cached;
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;int k=v.Count;
                v.Add(V(Mathf.Cos(a)*bottom,-.5f,Mathf.Sin(a)*bottom));v.Add(V(Mathf.Cos(b)*bottom,-.5f,Mathf.Sin(b)*bottom));
                v.Add(V(Mathf.Cos(b)*top,.5f,Mathf.Sin(b)*top));v.Add(V(Mathf.Cos(a)*top,.5f,Mathf.Sin(a)*top));
                t.AddRange(new[]{k,k+2,k+1,k,k+3,k+2});k=v.Count;
                v.Add(V(0,.5f,0));v.Add(V(Mathf.Cos(a)*top,.5f,Mathf.Sin(a)*top));v.Add(V(Mathf.Cos(b)*top,.5f,Mathf.Sin(b)*top));t.AddRange(new[]{k,k+2,k+1});k=v.Count;
                v.Add(V(0,-.5f,0));v.Add(V(Mathf.Cos(a)*bottom,-.5f,Mathf.Sin(a)*bottom));v.Add(V(Mathf.Cos(b)*bottom,-.5f,Mathf.Sin(b)*bottom));t.AddRange(new[]{k,k+1,k+2});
            }
            cached=FinishMesh("ResidenceTurnedMetal",v.ToArray(),t.ToArray());shapes[key]=cached;return cached;
        }
        static Mesh SphereMesh()
        {
            Mesh cached;if(shapes.TryGetValue("sphere",out cached)&&cached)return cached;
            const int rings=10,sides=20;var v=new List<Vector3>();var t=new List<int>();
            for(int r=0;r<=rings;r++)for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,lat=-Mathf.PI*.5f+r*Mathf.PI/rings;
                v.Add(V(Mathf.Cos(a)*Mathf.Cos(lat),Mathf.Sin(lat),Mathf.Sin(a)*Mathf.Cos(lat)));
            }
            for(int r=0;r<rings;r++)for(int i=0;i<sides;i++)
            {int a=r*sides+i,b=r*sides+(i+1)%sides,c=a+sides,d=b+sides;t.AddRange(new[]{a,c,b,b,c,d});}
            cached=FinishMesh("ResidenceSoftUpholstery",v.ToArray(),t.ToArray());shapes["sphere"]=cached;return cached;
        }
        static Mesh TorusMesh(float radius,float tube)
        {
            string key="torus_"+radius.ToString("R")+"_"+tube.ToString("R");Mesh cached;if(shapes.TryGetValue(key,out cached)&&cached)return cached;
            const int rings=32,sides=8;var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<rings;i++)for(int j=0;j<sides;j++)
            {float a=i*Mathf.PI*2/rings,b=j*Mathf.PI*2/sides;v.Add(V(Mathf.Cos(a)*(radius+Mathf.Cos(b)*tube),Mathf.Sin(b)*tube,Mathf.Sin(a)*(radius+Mathf.Cos(b)*tube)));}
            for(int i=0;i<rings;i++)for(int j=0;j<sides;j++)
            {int a=i*sides+j,b=i*sides+(j+1)%sides,c=((i+1)%rings)*sides+j,d=((i+1)%rings)*sides+(j+1)%sides;t.AddRange(new[]{a,b,c,b,d,c});}
            cached=FinishMesh("ResidenceRoundHardware",v.ToArray(),t.ToArray());shapes[key]=cached;return cached;
        }
        static Mesh FinishMesh(string name,Vector3[] v,int[] t)
        {
            var mesh=new Mesh{name=name};mesh.vertices=v;mesh.triangles=t;var uv=new Vector2[v.Length];
            for(int i=0;i<v.Length;i++)uv[i]=new Vector2(v[i].x+v[i].z,v[i].y+v[i].z);
            mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }

        sealed class IconCanvas
        {
            readonly int size;public readonly Color[] Pixels;
            public IconCanvas(int size){this.size=size;Pixels=new Color[size*size];}
            void Put(int x,int y,Color color){if(x>=0&&x<size&&y>=0&&y<size)Pixels[(size-1-y)*size+x]=color;}
            public void Rect(int x0,int y0,int x1,int y1,Color color)
            {for(int y=Mathf.Max(0,y0);y<Mathf.Min(size,y1);y++)for(int x=Mathf.Max(0,x0);x<Mathf.Min(size,x1);x++)Put(x,y,color);}
            public void Circle(int cx,int cy,int r,Color color)
            {for(int y=cy-r;y<=cy+r;y++)for(int x=cx-r;x<=cx+r;x++)if((x-cx)*(x-cx)+(y-cy)*(y-cy)<=r*r)Put(x,y,color);}
            public void Ring(int cx,int cy,int r,int thick,Color color)
            {for(int y=cy-r;y<=cy+r;y++)for(int x=cx-r;x<=cx+r;x++){int d=(x-cx)*(x-cx)+(y-cy)*(y-cy);if(d<=r*r&&d>=(r-thick)*(r-thick))Put(x,y,color);}}
            public void RoundRect(int x0,int y0,int x1,int y1,int r,Color color)
            {
                Rect(x0+r,y0,x1-r,y1,color);Rect(x0,y0+r,x1,y1-r,color);
                Circle(x0+r,y0+r,r,color);Circle(x1-r-1,y0+r,r,color);Circle(x0+r,y1-r-1,r,color);Circle(x1-r-1,y1-r-1,r,color);
            }
            public void Line(int x0,int y0,int x1,int y1,int w,Color color)
            {
                int count=Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0));
                for(int i=0;i<=count;i++){float q=count==0?0:(float)i/count;Circle(Mathf.RoundToInt(Mathf.Lerp(x0,x1,q)),Mathf.RoundToInt(Mathf.Lerp(y0,y1,q)),Mathf.Max(1,w/2),color);}
            }
            public void Polygon(float[] points,Color color)
            {
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    bool inside=false;int n=points.Length/2;
                    for(int i=0,j=n-1;i<n;j=i++)
                    {
                        float xi=points[i*2],yi=points[i*2+1],xj=points[j*2],yj=points[j*2+1];
                        if((yi>y)!=(yj>y)&&x<(xj-xi)*(y-yi)/(yj-yi)+xi)inside=!inside;
                    }
                    if(inside)Put(x,y,color);
                }
            }
        }
    }
}
