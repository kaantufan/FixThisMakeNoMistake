using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>
    /// Original Home V2 architecture and practical household objects. Metres, Y up,
    /// frontage -Z. Meshes only: the world owns collision, interaction and visibility.
    /// Baking is supported: each returned hierarchy owns real render meshes/materials.
    /// </summary>
    public static class PanicHomeArt
    {
        internal enum M { Walnut, WalnutLight, WalnutDark, Charcoal, Steel, Plaster, Taupe, Olive, Leaf,
            Lawn, Earth, Concrete, Stone, Slate, Brick, Linen, SafetyAmber, Red, Glass, Black, Road, Mist, FacadeSage, FacadeOchre, FacadeIvory, WarmWindow }
        static Material[] palette;
        static readonly Dictionary<string, Mesh> shapes = new Dictionary<string, Mesh>();
        static readonly Dictionary<Material, Material> recolored = new Dictionary<Material, Material>();
        const float WallHeight = 3.2f, WallThickness = .26f;

        public static GameObject BuildLandscape()
        {
            Init();
            var root = new GameObject("HomeV2_Landscape");
            var b = new Builder();
            // Continuous ground, not a raised model display base. The road continues beyond view.
            // V3: four tight perimeter strips leave the entire house footprint open below ground.
            // Neither soil nor turf can cover the basement/stair when the upper floors are cut away.
            LandscapeRing(b, 160, -.19f, .28f, M.Earth);
            LandscapeRing(b, 48, -.095f, .13f, M.Lawn);
            CreekCutGround(b, new Vector3(0, -.08f, 36), new Vector3(150, .05f, 24), M.Olive);
            CreekCutGround(b, new Vector3(0, -.08f, -45), new Vector3(150, .05f, 45), M.Olive);
            CreekCutGround(b, new Vector3(-45, -.08f, 0), new Vector3(45, .05f, 65), M.Olive);
            CreekCutGround(b, new Vector3(45, -.08f, 0), new Vector3(45, .05f, 65), M.Olive);
            b.Box(new Vector3(0, -.018f, -19), new Vector3(150, .045f, 7.3f), M.Road, .008f);
            for (int s = -1; s <= 1; s += 2)
            {
                if(s<0)
                {
                    b.Box(new Vector3(0,.018f,-23.12f),new Vector3(150,.12f,.68f),M.Concrete,.02f);
                    b.Box(new Vector3(0,-.006f,-22.66f),new Vector3(150,.09f,.14f),M.Stone,.01f);
                }
                else
                {
                    // Garage drive returns to11.7: the dropped kerb spans9.95..13.45.
                    for(int side=0;side<2;side++)
                    {
                        float left=side==0?-75f:13.45f,right=side==0?9.95f:75f;
                        b.Box(new Vector3((left+right)*.5f,.018f,-14.88f),new Vector3(right-left,.12f,.68f),M.Concrete,.02f);
                        b.Box(new Vector3((left+right)*.5f,-.006f,-15.34f),new Vector3(right-left,.09f,.14f),M.Stone,.01f);
                    }
                }
            }
            for (int i = -18; i <= 18; i++)
                b.Box(new Vector3(i * 4, .01f, -19), new Vector3(1.8f, .008f, .1f), M.Linen, .001f);
            // A modest paved path and perimeter strip leave most of the plot as usable garden.
            for (int z = 0; z < 13; z++)
                for (int x = 0; x < 3; x++)
                    b.Box(new Vector3((x - 1) * .56f, -.007f, -6.45f - z * .74f),
                        new Vector3(.545f, .038f, .72f), (x + z) % 4 == 0 ? M.Stone : M.Concrete, .015f);
            for (int x = 0; x < 19; x++)
                b.Box(new Vector3(1.4f + x * .51f, -.007f, -7), new Vector3(.49f, .038f, 1), M.Stone, .015f);
            // Foundation gravel apron: many separate stones and edging, no giant pedestal.
            for (int i = 0; i < 49; i++)
            {
                float x = -8.6f + (i * 1.73f) % 17.2f;
                float z = i % 2 == 0 ? -6.4f : 6.4f;
                b.Ellipsoid(new Vector3(x, -.005f, z), new Vector3(.12f, .055f, .09f), M.Stone, i * 11);
            }
            for (int s = -1; s <= 1; s += 2)
                for (int i = 0; i < 33; i++)
                    b.Ellipsoid(new Vector3(s * 8.4f, -.01f, -6 + i * .38f), new Vector3(.1f, .04f, .14f), M.Stone, i + s);
            // Planted beds around the perimeter. Inner central lawn remains usable.
            for (int side = -1; side <= 1; side += 2)
            {
                b.Box(new Vector3(side * 12.5f, -.013f, 8.8f), new Vector3(3.6f, .045f, 2.3f), M.Earth, .12f);
                for (int i = 0; i < 7; i++)
                    Bush(b, new Vector3(side * 12.5f - 1.35f + i * .44f, .02f, 8.8f + (i % 2) * .5f), .46f, i + 20);
                for (int i = 0; i < 11; i++)
                    b.Box(new Vector3(side * 12.5f - 1.7f + i * .34f, .025f, 7.61f), new Vector3(.325f, .11f, .14f), M.Brick, .025f);
            }
            // Fence has a real 2m opening at the main gate (0,0,-13).
            for (int i = -8; i < 8; i++)
            {
                if (i != -1 && i != 0 && i != 4 && i != 5 && i != 6) Place(root, Create("fence"), new Vector3(i * 2 + 1, 0, -13), 0);
                Place(root, Create("fence"), new Vector3(i * 2 + 1, 0, 15), 0);
            }
            for (int s = -1; s <= 1; s += 2)
            {
                var halfFence = Create("fence"); halfFence.transform.localScale = new Vector3(.5f, 1, 1);
                Place(root, halfFence, new Vector3(s * 1.5f, 0, -13), 0);
            }
            for (int i = 0; i < 14; i++)
                for (int s = -1; s <= 1; s += 2)
                    Place(root, Create("fence"), new Vector3(s * 16, 0, -12 + i * 2), 90);
            var driveFence=Create("fence");driveFence.transform.localScale=new Vector3(.75f,1,1);Place(root,driveFence,new Vector3(8.75f,0,-13),0);
            // Trees kept away from the stair, entry and bunker routes.
            Vector3[] treeSeats = {
                new Vector3(-12,0,11.5f), new Vector3(13,0,11.8f), new Vector3(-13,0,-8),
                new Vector3(-22.8f,0,8), new Vector3(20,0,4), new Vector3(-23,0,21),
                new Vector3(21,0,22), new Vector3(-31,0,-13), new Vector3(32,0,-11),
                new Vector3(-12,0,-29), new Vector3(13,0,-30), new Vector3(3,0,27) };
            for (int i = 0; i < treeSeats.Length; i++)
            {
                var tree = Create("tree");
                tree.transform.localScale = Vector3.one * (.85f + (i % 4) * .09f);
                Place(root, tree, treeSeats[i], i * 71);
            }
            // Neighbor houses are complete silhouettes with pitched roofs, recessed windows and yards.
            foreach(var site in PanicExteriorArt.NeighbourSites)
                Neighbour(b,site.Position,site.Width,site.Depth,site.Height,site.Yaw,site.Pale);
            ExteriorStreetDetails(b);
            var stream=PanicExteriorArt.CreateStream();stream.transform.SetParent(root.transform,false);
            // Utility poles and wires ground the scene in an inhabited street.
            for (int i = -2; i <= 2; i++)
            {
                float x = i * 24;
                b.Cylinder(new Vector3(x, 3.3f, -14.5f), .095f, 6.6f, M.WalnutDark, 9);
                b.Box(new Vector3(x, 5.8f, -14.5f), new Vector3(1.2f, .12f, .14f), M.WalnutDark, .01f);
                for (int k = -1; k <= 1; k += 2)
                    b.Cylinder(new Vector3(x + k * .4f, 6, -14.5f), .055f, .2f, M.Slate, 8);
                if (i < 2) for (int k = -1; k <= 1; k += 2)
                {
                    Vector3 prev = new Vector3(x + k * .4f, 6.1f, -14.5f);
                    for (int t = 1; t <= 8; t++)
                    {
                        float q = t / 8f;
                        Vector3 next = new Vector3(x + k * .4f + 24 * q, 6.1f - Mathf.Sin(q * Mathf.PI) * .65f, -14.5f);
                        b.Beam(prev, next, .018f, M.Charcoal); prev = next;
                    }
                }
            }
            // Distant low rolling silhouettes replace a flat empty backdrop. No lights/sky state is changed.
            for (int i = 0; i < 11; i++)
            {
                float a = i * Mathf.PI * 2 / 11;
                b.Ellipsoid(new Vector3(Mathf.Cos(a) * 65, -4.5f, Mathf.Sin(a) * 65),
                    new Vector3(19 + i % 3 * 4, 10 + i % 4, 14 + i % 2 * 5), M.Olive, 78 + i);
            }
            for (int i = 0; i < 7; i++)
            {
                float a = i * Mathf.PI * 2 / 7;
                b.Ellipsoid(new Vector3(Mathf.Cos(a) * 62, 20 + i % 3 * 2, Mathf.Sin(a) * 62),
                    new Vector3(8 + i % 3, 1.6f, 3.6f), M.Mist, i + 58);
            }
            b.Attach(root, "landscape");
            return root;
        }

        public static GameObject BuildFloor(int floor)
        {
            return BuildFloor(floor, false);
        }

        /// <summary>V3 true cuts the basement stair in ground floor and the balcony entry upstairs.</summary>
        public static GameObject BuildFloor(int floor, bool residenceV3)
        {
            if(residenceV3)return BuildResidenceFloorV414(floor);
            Init();
            floor = floor <= 0 ? 0 : 1;
            var root = new GameObject("HomeV2_Floor" + floor);
            var deck = Group(root, "Floor");
            var back = Group(root, "BackWalls");
            var low = Group(root, "FrontLowWalls");
            var high = Group(root, "FrontHighWalls");
            var divisions = Group(root, "Partitions");
            var north = Group(root, "FacadeNorth"); var west = Group(root, "FacadeWest");
            var south = Group(root, "FacadeSouth"); var east = Group(root, "FacadeEast");
            var detail = new Builder();
            // Physical structural slabs and physical timber strips. Top of all finishes is y=0.
            if (floor == 0 && !residenceV3) Solid(deck, "floor_slab", new Vector3(0, -.12f, 0), new Vector3(16, .204f, 12), M.Concrete, .01f);
            else
            {
                Solid(deck, "floor_left", new Vector3(-4.45f, -.12f, 0), new Vector3(7.1f, .204f, 12), M.Concrete, .005f);
                Solid(deck, "floor_right", new Vector3(4.45f, -.12f, 0), new Vector3(7.1f, .204f, 12), M.Concrete, .005f);
                Solid(deck, "floor_front", new Vector3(0, -.12f, -3), new Vector3(1.8f, .204f, 6), M.Concrete, .005f);
                Solid(deck, "floor_landing", new Vector3(0, -.12f, 5.4f), new Vector3(1.8f, .204f, 1.2f), M.Concrete, .005f);
            }
            for (int row = 0; row < 40; row++)
            {
                float z0 = -6 + row * .3f, z1 = z0 + .3f;
                float x0 = -8;
                int col = 0;
                while (x0 < 7.999f)
                {
                    float x1 = Mathf.Min(8, x0 + (col == 0 && row % 2 == 0 ? 1.18f : 2.35f));
                    bool tile = floor == 0 && z0 >= 1.2f && x0 >= 2.2f;
                    if ((floor == 1 || residenceV3) && row >= 20 && row < 36)
                    {
                        FloorFinish(detail, x0, Mathf.Min(x1, -.9f), z0, z1, row, col, tile);
                        FloorFinish(detail, Mathf.Max(x0, .9f), x1, z0, z1, row, col, tile);
                    }
                    else FloorFinish(detail, x0, x1, z0, z1, row, col, tile);
                    x0 = x1; col++;
                }
            }
            if (floor == 1 || residenceV3)
            {
                // Exposed timber edges make the stair opening legible rather than a painted rectangle.
                detail.Box(new Vector3(-.943f, -.105f, 2.4f), new Vector3(.085f, .21f, 4.8f), M.WalnutDark, .01f);
                detail.Box(new Vector3(.943f, -.105f, 2.4f), new Vector3(.085f, .21f, 4.8f), M.WalnutDark, .01f);
                detail.Box(new Vector3(0, -.105f, 4.842f), new Vector3(1.8f, .21f, .085f), M.WalnutDark, .01f);
                // Both residence openings have side guards; the north landing stays open.
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int i = 0; i < 10; i++)
                        detail.Box(new Vector3(s * 1.01f, .47f, .1f + i * .52f), new Vector3(.043f, .94f, .043f), M.Charcoal, .008f);
                    detail.Box(new Vector3(s * 1.01f, .97f, 2.4f), new Vector3(.08f, .07f, 4.8f), M.Walnut, .018f);
                }
                // Ground south is the next flight's entrance; only upstairs ends here.
                if (floor == 1)
                {
                    for (int i = -2; i <= 2; i++)
                        detail.Box(new Vector3(i * .505f, .47f, -.05f), new Vector3(.043f, .94f, .043f), M.Charcoal, .008f);
                    detail.Box(new Vector3(0, .97f, -.05f), new Vector3(2.1f, .07f, .08f), M.Walnut, .018f);
                }
            }
            detail.Attach(deck, "floor_finish");
            // The same cut geometry is used on both floors, each with its own local origin.
            WallWithWindows(north, "north", false, 6, -8, 8, new[] { -4f, 4f }, 0, WallHeight);
            WallWithWindows(west, "west", true, -8, -6, 6, new[] { -3f, 3f }, 0, WallHeight);
            // South/east near-facing walls split at .45 so a world can hide only their high groups.
            SouthWall(south, 0, .45f, floor == 0 || residenceV3);
            SouthWall(south, .45f, WallHeight, floor == 0 || residenceV3);
            if (residenceV3 && floor == 0)
            {
                // House/garage shared aperture: z1.55..2.85, y0..2.30, center(8.1,0,2.2).
                WallWithWindows(east,"east_south",true,8,-6,1.55f,new[]{-3f},0,WallHeight);
                Segment(east,"east_garage_lintel",true,8,1.55f,2.85f,2.30f,WallHeight,M.Plaster);
                Segment(east,"east_garage_north",true,8,2.85f,6,0,WallHeight,M.Plaster);
            }
            else WallWithWindows(east,"east_full",true,8,-6,6,new[]{-3f,3f},0,WallHeight);
            for (int side = -1; side <= 1; side += 2)
            {
                Place(south, Create("window"), new Vector3(side*4,0,-6),180);
                if (!(residenceV3 && floor == 0 && side == 1))
                    Place(east, Create("window"), new Vector3(8,0,side*3),90);
            }
            // The residence upper bathroom owns its single x=2 west wall and z=2.5 entrance.
            // Keep the original east divider only on the ground floor and legacy non-residence layouts.
            if (!residenceV3 || floor == 0)
            {
                Partition(divisions, "east_south", new Vector3(2, .44f, -2.7f), new Vector3(.14f, .88f, 6.2f));
                Partition(divisions, "east_north", new Vector3(2, .44f, 3.8f), new Vector3(.14f, .88f, 4.4f));
                var trim = new Builder();
                for (int s = -1; s <= 1; s += 2)
                    trim.Box(new Vector3(2, 1.14f, 1 + s * .64f), new Vector3(.2f, 2.28f, .08f), M.WalnutDark, .012f);
                trim.Box(new Vector3(2, 2.3f, 1), new Vector3(.2f, .08f, 1.36f), M.WalnutDark, .012f);
                trim.Attach(divisions, "door_jamb");
            }
            DirectionalTrim(north,"north",floor,residenceV3);
            DirectionalTrim(west,"west",floor,residenceV3);
            DirectionalTrim(south,"south",floor,residenceV3);
            DirectionalTrim(east,"east",floor,residenceV3);
            if (floor == 0)
            {
                var baseMasonry = new Builder();
                for (int row = 0; row < 3; row++)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        float x = -7.75f + i * .525f + (row % 2) * .18f;
                        if (Mathf.Abs(x) < .76f) continue;
                        baseMasonry.Box(new Vector3(x, .045f + row * .105f, -6.145f), new Vector3(.5f, .095f, .05f),
                            i % 5 == 0 ? M.Stone : M.Brick, .012f);
                    }
                    for (int i = 0; i < 23; i++)
                    {
                        if(residenceV3 && -5.75f+i*.51f > 1.30f && -5.75f+i*.51f < 3.10f) continue;
                        baseMasonry.Box(new Vector3(8.145f, .045f + row * .105f, -5.75f + i * .51f),
                            new Vector3(.05f, .095f, .49f), i % 5 == 0 ? M.Stone : M.Brick, .012f);
                    }
                }
                baseMasonry.Attach(low, "brick_foundation");
            }
            return root;
        }

        // V4.14 residence: same structural shell, a relocated shaft, useful rooms and eight windows.
        static GameObject BuildResidenceFloorV414(int floor)
        {
            Init();floor=floor<=0?0:1;
            var root=new GameObject("HomeV414_Floor"+floor);
            var deck=Group(root,"Floor");Group(root,"BackWalls");
            Group(root,"FrontLowWalls");Group(root,"FrontHighWalls");
            var divisions=Group(root,"Partitions");
            var north=Group(root,"FacadeNorth");var west=Group(root,"FacadeWest");
            var south=Group(root,"FacadeSouth");var east=Group(root,"FacadeEast");
            float zs=PanicHouseLayoutV414.StairSouth,zn=PanicHouseLayoutV414.StairNorth;
            Solid(deck,"floor_left",new Vector3(-4.45f,-.12f,0),new Vector3(7.1f,.204f,12),M.Concrete,.005f);
            Solid(deck,"floor_right",new Vector3(4.45f,-.12f,0),new Vector3(7.1f,.204f,12),M.Concrete,.005f);
            Solid(deck,"floor_front",new Vector3(0,-.12f,(-6+zs)*.5f),new Vector3(1.8f,.204f,zs+6),M.Concrete,.005f);
            Solid(deck,"floor_landing",new Vector3(0,-.12f,(zn+6)*.5f),new Vector3(1.8f,.204f,6-zn),M.Concrete,.005f);
            var b=new Builder();
            for(int row=0;row<48;row++)
            {
                float a=-6+row*.25f,z=a+.25f;
                for(int col=0;col<8;col++)
                {
                    float left=-8+col*2,right=left+2;
                    bool tile=floor==0&&left>=2 || floor==1&&left>=2&&a>=1.75f;
                    if(a<zn&&z>zs)
                    {
                        FloorFinish(b,left,Mathf.Min(right,-.9f),a,z,row,col,tile);FloorFinish(b,Mathf.Max(left,.9f),right,a,z,row,col,tile);
                        if(a<zs)FloorFinish(b,Mathf.Max(left,-.9f),Mathf.Min(right,.9f),a,zs,row,col,tile);
                        if(z>zn)FloorFinish(b,Mathf.Max(left,-.9f),Mathf.Min(right,.9f),zn,z,row,col,tile);
                    }
                    else FloorFinish(b,left,right,a,z,row,col,tile);
                }
            }
            for(int side=-1;side<=1;side+=2)
            {
                b.Box(new Vector3(side*.943f,-.105f,(zs+zn)*.5f),new Vector3(.085f,.21f,zn-zs),M.WalnutDark,.01f);
                for(int i=0;i<10;i++)b.Box(new Vector3(side*1.01f,.47f,zs+.1f+i*.52f),new Vector3(.043f,.94f,.043f),M.Charcoal,.008f);
                b.Box(new Vector3(side*1.01f,.97f,(zs+zn)*.5f),new Vector3(.08f,.07f,zn-zs),M.Walnut,.018f);
            }
            if(floor==1)
            {
                for(int i=-2;i<=2;i++)b.Box(new Vector3(i*.505f,.47f,zs-.05f),new Vector3(.043f,.94f,.043f),M.Charcoal,.008f);
                b.Box(new Vector3(0,.97f,zs-.05f),new Vector3(2.1f,.07f,.08f),M.Walnut,.018f);
            }
            b.Attach(deck,"floor_finish");
            WallWithWindows(north,"north",false,6,-8,8,new[]{-4f,4f},0,WallHeight);
            WallWithWindows(west,"west",true,-8,-6,6,new[]{-3f},0,WallHeight);
            // South openings: living-room window below, study window above, retained front/balcony door.
            float window=floor==0?-4:4;
            WallWithWindows(south,"south_left",false,-6,-8,-.65f,window<0?new[]{window}:new float[0],0,WallHeight);
            WallWithWindows(south,"south_right",false,-6,.65f,8,window>0?new[]{window}:new float[0],0,WallHeight);
            Segment(south,"south_lintel",false,-6,-.65f,.65f,2.25f,WallHeight,M.Plaster);
            if(floor==0)
            {
                Segment(east,"east_south",true,8,-6,1.55f,0,WallHeight,M.Plaster);
                Segment(east,"east_garage_lintel",true,8,1.55f,2.85f,2.30f,WallHeight,M.Plaster);
                Segment(east,"east_north",true,8,2.85f,6,0,WallHeight,M.Plaster);
                // Kitchen is a room. A wide living-room/hall arch keeps central circulation open.
                LayoutPartitionDoor(divisions,"kitchen",false,.8f,2,8,3.35f);
                LayoutPartition(divisions,"kitchen_side",true,2,.8f,6);
                LayoutPartitionDoor(divisions,"wc",false,-2.8f,2,4.8f,3.35f);
                LayoutPartitionDoor(divisions,"pantry",false,-2.8f,4.8f,8,6.4f);
                LayoutPartition(divisions,"service_side",true,2,-6,-2.8f);
                LayoutPartition(divisions,"wc_pantry",true,4.8f,-6,-2.8f);
            }
            else
            {
                Segment(east,"east_full",true,8,-6,6,0,WallHeight,M.Plaster);
                LayoutPartitionDoor(divisions,"parent",true,-2.2f,.8f,6,3.9f);
                LayoutPartitionDoor(divisions,"child",true,-2.2f,-6,.8f,-3.2f);
                LayoutPartition(divisions,"bedroom_divider",false,.8f,-8,-2.2f);
                LayoutPartitionDoor(divisions,"study",true,2,-6,1.8f,-2.6f);
            }
            DirectionalTrim(north,"north",floor,true);DirectionalTrim(west,"west",floor,true);
            DirectionalTrim(south,"south",floor,true);DirectionalTrim(east,"east",floor,true);
            return root;
        }

        static void LayoutPartition(GameObject parent,string id,bool alongZ,float axis,float lo,float hi)
        {
            if(hi-lo<.01f)return;
            Vector3 p=alongZ?new Vector3(axis,.48f,(lo+hi)*.5f):new Vector3((lo+hi)*.5f,.48f,axis);
            Vector3 size=alongZ?new Vector3(.16f,.96f,hi-lo):new Vector3(hi-lo,.96f,.16f);
            // Low render mesh is raised to3.2m only in FPS; the world always builds a full-height collider.
            Partition(parent,"v414_partition_"+id,p,size);
        }
        static void LayoutPartitionDoor(GameObject parent,string id,bool alongZ,float axis,float lo,float hi,float door)
        {
            LayoutPartition(parent,id+"_a",alongZ,axis,lo,door-.68f);
            LayoutPartition(parent,id+"_b",alongZ,axis,door+.68f,hi);
            var b=new Builder();
            for(int s=-1;s<=1;s+=2)
            {
                Vector3 p=alongZ?new Vector3(axis,1.12f,door+s*.66f):new Vector3(door+s*.66f,1.12f,axis);
                b.Box(p,alongZ?new Vector3(.20f,2.24f,.045f):new Vector3(.045f,2.24f,.20f),M.WalnutDark,.006f);
            }
            // The lintel's collider belongs above the walk opening, never stretched down to the floor.
            Segment(parent,"v414_lintel_"+id,alongZ,axis,door-.68f,door+.68f,2.28f,3.2f,M.Plaster);
            b.Attach(parent,"v414_door_trim_"+id);
        }

        public static GameObject BuildKitchenDetailsV414()
        {
            Init();var root=new GameObject("HomeV414_Kitchen");var b=new Builder();
            // L-shaped cabinetry is authored against the kitchen's west/north walls.
            // One low collider per cabinet leaves the room's middle and door swing free.
            Solid(root,"v414_counter_west",new Vector3(2.75f,.46f,3.80f),new Vector3(1.0f,.92f,2.7f),M.FacadeIvory,.018f);
            Solid(root,"v414_counter_corner",new Vector3(3.33f,.46f,4.95f),new Vector3(1.15f,.92f,.67f),M.FacadeIvory,.018f);
            b.Box(new Vector3(2.75f,.945f,3.80f),new Vector3(1.06f,.06f,2.76f),M.Stone,.018f);
            b.Box(new Vector3(3.33f,.945f,4.95f),new Vector3(1.20f,.06f,.73f),M.Stone,.018f);
            // Recessed stainless sink, lip and curved-looking segmented tap.
            b.Box(new Vector3(2.75f,.982f,2.97f),new Vector3(.72f,.016f,.62f),M.Steel,.07f);
            b.Box(new Vector3(2.75f,.992f,2.97f),new Vector3(.55f,.018f,.44f),M.Charcoal,.055f);
            b.Box(new Vector3(2.75f,1.001f,2.97f),new Vector3(.43f,.008f,.32f),M.Steel,.04f);
            b.Beam(new Vector3(2.35f,.98f,3.15f),new Vector3(2.35f,1.31f,3.15f),.035f,M.Steel);
            b.Beam(new Vector3(2.35f,1.31f,3.15f),new Vector3(2.60f,1.31f,3.15f),.035f,M.Steel);
            b.Beam(new Vector3(2.60f,1.31f,3.15f),new Vector3(2.60f,1.24f,3.15f),.035f,M.Steel);
            // Four solid dark hob rings and an oven front define the cooking station.
            b.Box(new Vector3(2.75f,.984f,4.38f),new Vector3(.78f,.024f,.66f),M.Charcoal,.025f);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
            {b.Cylinder(new Vector3(2.75f+x*.20f,1.003f,4.38f+z*.17f),.12f,.016f,M.Steel,16);b.Cylinder(new Vector3(2.75f+x*.20f,1.014f,4.38f+z*.17f),.08f,.012f,M.Black,16);}
            b.Box(new Vector3(3.261f,.48f,4.38f),new Vector3(.02f,.55f,.62f),M.Charcoal,.02f);
            b.Box(new Vector3(3.278f,.48f,4.38f),new Vector3(.016f,.36f,.46f),M.Black,.012f);
            b.Box(new Vector3(3.302f,.73f,4.38f),new Vector3(.045f,.035f,.48f),M.Steel,.009f);
            // Door seams and handles follow the visible east cabinet front.
            for(int i=0;i<3;i++)
            {float z=2.75f+i*.65f;b.Box(new Vector3(3.257f,.45f,z),new Vector3(.014f,.78f,.016f),M.WalnutDark,.002f);b.Box(new Vector3(3.279f,.76f,z+.16f),new Vector3(.035f,.035f,.24f),M.Steel,.008f);}
            // A backsplash stays below eye level and does not create a false room partition.
            b.Box(new Vector3(2.10f,1.22f,3.8f),new Vector3(.025f,.44f,2.7f),M.FacadeSage,.008f);
            b.Attach(root,"KitchenCounterSinkHobDetails");return root;
        }

        public static GameObject BuildChildDetailsV414()
        {
            Init();var root=new GameObject("HomeV414_ChildRoomDetails");var b=new Builder();
            // A compact play mat stays north of the bed and away from the east door sweep.
            b.Box(new Vector3(-5.5f,.025f,-1.4f),new Vector3(2.5f,.025f,1.4f),M.FacadeSage,.04f);
            for(int i=0;i<5;i++)b.Box(new Vector3(-6.45f+i*.47f,.042f,-1.4f),new Vector3(.28f,.012f,.85f),i%2==0?M.SafetyAmber:M.Linen,.04f);
            // Original colored wooden blocks on the actual cabinet top, without a new collision target.
            for(int i=0;i<4;i++)b.Box(new Vector3(-3.6f+i*.24f,1.06f,-5.25f),new Vector3(.18f,.20f,.18f),i%2==0?M.Red:M.SafetyAmber,.015f);
            b.Box(new Vector3(-3.12f,1.25f,-5.25f),new Vector3(.18f,.18f,.18f),M.FacadeSage,.015f);
            // A colorful framed kite picture on the child's west wall identifies the room at a glance.
            b.Box(new Vector3(-7.84f,1.50f,-1.2f),new Vector3(.035f,.92f,.80f),M.WalnutLight,.01f);
            b.Box(new Vector3(-7.81f,1.50f,-1.2f),new Vector3(.018f,.80f,.68f),M.Linen,.004f);
            b.Box(new Vector3(-7.79f,1.60f,-1.2f),new Vector3(.012f,.31f,.31f),M.Red,.008f,Quaternion.Euler(45,0,0));
            b.Beam(new Vector3(-7.78f,1.42f,-1.2f),new Vector3(-7.78f,1.17f,-1.04f),.012f,M.Slate);
            b.Attach(root,"ChildPlayMatBlocksKitePicture");return root;
        }

        public static GameObject BuildRoof()
        {
            Init();
            var root = new GameObject("HomeV2_Roof");
            var b = new Builder();
            // All vertices include y=6.4 placement. Do not add another 6.4m root offset.
            Roof(b, Vector3.zero, 17.15f, 13.15f, 6.4f, 8.65f);
            for (int s = -1; s <= 1; s += 2)
            {
                b.TrianglePrism(new Vector3(s * 8.035f, 0, 0), 12.05f, 6.4f, 8.45f, .13f, M.Plaster);
                b.Box(new Vector3(s * 8.25f, 6.43f, 0), new Vector3(.11f, .15f, 13.25f), M.WalnutDark, .018f);
                // Small gable louvres, separate slats with dark recess.
                b.Box(new Vector3(s * 8.115f, 7.23f, 0), new Vector3(.04f, .56f, .75f), M.Charcoal, .018f);
                for (int i = 0; i < 6; i++)
                    b.Box(new Vector3(s * 8.145f, 7.02f + i * .085f, 0), new Vector3(.07f, .035f, .67f), M.Steel, .005f);
            }
            b.Box(new Vector3(4.6f, 8.14f, 2.7f), new Vector3(.74f, 1.85f, .87f), M.Brick, .026f);
            for (int row = 0; row < 9; row++)
                b.Box(new Vector3(4.6f, 7.38f + row * .18f, 2.245f), new Vector3(.75f, .018f, .025f), M.Taupe, .001f);
            b.Box(new Vector3(4.6f, 9.05f, 2.7f), new Vector3(.94f, .12f, 1.07f), M.Concrete, .025f);
            b.Box(new Vector3(4.6f, 9.115f, 2.7f), new Vector3(.47f, .025f, .6f), M.Black, .01f);
            b.Attach(root, "roof");
            return root;
        }

        public static GameObject Create(string id)
        {
            Init();
            id = (id ?? "supplycrate").ToLowerInvariant();
            if (PanicCuratedAssetArt.TryCreateFixture(id, out var curated)) return curated;
            var root = new GameObject("HomeV2_" + id);
            var b = new Builder();
            switch (id)
            {
                case "window": Window(b); break;
                case "door": Door(root, b); break;
                case "tree": Tree(b); break;
                case "drain": Drain(b); break;
                case "fence": Fence(b); break;
                case "hose": Hose(b); break;
                case "toolbox": Toolbox(root, b); break;
                case "medicinecabinet": Cabinet(root, b, .72f, 1.14f, .32f, true, false); break;
                case "cabinet":
                case "kitchencabinet": Cabinet(root, b, 1.48f, .94f, .67f, false, true); break;
                case "fridge": Fridge(root, b); break;
                case "locker": Cabinet(root, b, .76f, 2.08f, .63f, false, false); break;
                case "supplycrate": Crate(root, b); break;
                case "stairs": Stairs(b); break;
                case "workbench": Workbench(b); break;
                case "generator": Generator(b); break;
                default: Crate(root, b); break;
            }
            b.Attach(root, id);
            return root;
        }

        /// <summary>Uses cloned shared materials so original PanicArt material assets remain intact.</summary>
        public static void ApplyPalette(GameObject target)
        {
            if (!target) return;
            Init();
            foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    var original = materials[i];
                    // Authored residence materials carry intentionally distinct fabric, wood, screen and enamel colors.
                    // Never remap them by an old palette's name heuristics (baked assets may have numeric suffixes).
                    if (!original || original.name.StartsWith("HomeV2_", StringComparison.Ordinal) ||
                        original.name.StartsWith("HomeV3_", StringComparison.Ordinal) ||
                        original.name.StartsWith("HomeV4_", StringComparison.Ordinal) ||
                        original.name.StartsWith("M_", StringComparison.Ordinal) ||
                        original.name.StartsWith("Curated", StringComparison.Ordinal)) continue;
                    Material replacement;
                    if (!recolored.TryGetValue(original, out replacement) || !replacement)
                    {
                        string name = original.name.Replace(" (Instance)", "").Replace("Panic_", "").ToLowerInvariant();
                        M color = M.Taupe;
                        if (name.Contains("oaklight")) color = M.WalnutLight;
                        else if (name.Contains("oakdark")) color = M.WalnutDark;
                        else if (name.Contains("oak")) color = M.Walnut;
                        else if (name.Contains("ink") || name.Contains("black")) color = M.Charcoal;
                        else if (name.Contains("plaster") || name.Contains("cream")) color = M.Plaster;
                        else if (name.Contains("teal") || name.Contains("blue")) color = M.Slate;
                        else if (name.Contains("sage") || name.Contains("leaf")) color = M.Olive;
                        else if (name.Contains("amber") || name.Contains("orange") || name.Contains("glow")) color = M.SafetyAmber;
                        else if (name.Contains("steel")) color = M.Steel;
                        else if (name.Contains("glass")) color = M.Glass;
                        else if (name.Contains("linen") || name.Contains("white")) color = M.Linen;
                        else if (name.Contains("red")) color = M.Red;
                        else if (name.Contains("earth")) color = M.Earth;
                        replacement = new Material(original) { name = "HomeV2_Recolored_" + original.name };
                        if (replacement.HasProperty("_Color")) replacement.color = palette[(int)color].color;
                        if (replacement.HasProperty("_Glossiness")) replacement.SetFloat("_Glossiness", color == M.Glass ? .66f : .2f);
                        // Keep emission channels, maps and keywords for existing upgrade/selection art.
                        if (color == M.Glass) Transparent(replacement);
                        recolored[original] = replacement;
                    }
                    materials[i] = replacement; changed = true;
                }
                if (changed) renderer.sharedMaterials = materials;
            }
        }

        static void FloorFinish(Builder b, float x0, float x1, float z0, float z1, int row, int col, bool tile)
        {
            if (x1 - x0 < .014f) return;
            M m = tile ? ((row + col) % 5 == 0 ? M.Taupe : M.Stone) :
                ((row * 3 + col) % 9 == 0 ? M.WalnutLight : ((row + col) % 7 == 0 ? M.WalnutDark : M.Walnut));
            b.Box(new Vector3((x0 + x1) * .5f, -.009f, (z0 + z1) * .5f),
                new Vector3(x1 - x0 - .006f, .018f, z1 - z0 - .006f), m, .002f);
            if (!tile && x1 - x0 > .8f && (row + col) % 4 == 0)
                b.Box(new Vector3((x0 + x1) * .5f, .0005f, z0 + .075f),
                    new Vector3((x1 - x0) * .65f, .001f, .003f), M.WalnutDark, .0001f);
        }

        static void LandscapeRing(Builder b, float span, float y, float thickness, M material)
        {
            float side = (span - 16) * .5f;
            CreekCutGround(b, new Vector3(-(8 + side * .5f), y, 0), new Vector3(side, thickness, span), material);
            CreekCutGround(b, new Vector3(8 + side * .5f, y, 0), new Vector3(side, thickness, span), material);
            float end = (span - 12) * .5f;
            CreekCutGround(b, new Vector3(0, y, -(6 + end * .5f)), new Vector3(16, thickness, end), material);
            CreekCutGround(b, new Vector3(0, y, 6 + end * .5f), new Vector3(16, thickness, end), material);
        }

        static void SouthWall(GameObject parent, float lo, float hi, bool hasEntry)
        {
            // Window bays at +/-4 plus 1.2m entry opening. No segment ever bridges the entry below its lintel.
            float[] edges = { -8, -4.9f, -3.1f, -.6f, .6f, 3.1f, 4.9f, 8 };
            for (int i = 0; i < edges.Length - 1; i++)
            {
                float a = edges[i], z = edges[i + 1];
                bool entry = i == 3 && hasEntry;
                bool window = i == 1 || i == 5;
                if (entry) Segment(parent, "south_lintel", false, -6, a, z, Mathf.Max(lo, 2.25f), hi, M.Plaster);
                else if (window)
                {
                    Segment(parent, "south_sill_" + i, false, -6, a, z, lo, Mathf.Min(hi, .95f), M.Plaster);
                    Segment(parent, "south_header_" + i, false, -6, a, z, Mathf.Max(lo, 2.35f), hi, M.Plaster);
                }
                else Segment(parent, "south_pier_" + i, false, -6, a, z, lo, hi, M.Plaster);
            }
        }

        static void WallWithWindows(GameObject parent, string name, bool alongZ, float fixedCoord,
            float begin, float end, float[] centers, float lo, float hi)
        {
            float cursor = begin;
            for (int i = 0; i < centers.Length; i++)
            {
                float a = centers[i] - .9f, z = centers[i] + .9f;
                Segment(parent, name + "_pier_" + i, alongZ, fixedCoord, cursor, a, lo, hi, M.Plaster);
                Segment(parent, name + "_sill_" + i, alongZ, fixedCoord, a, z, lo, Mathf.Min(hi, .95f), M.Plaster);
                Segment(parent, name + "_header_" + i, alongZ, fixedCoord, a, z, Mathf.Max(lo, 2.35f), hi, M.Plaster);
                cursor = z;
            }
            Segment(parent, name + "_end", alongZ, fixedCoord, cursor, end, lo, hi, M.Plaster);
        }
        static void Segment(GameObject parent, string name, bool alongZ, float fixedCoord,
            float a, float z, float lo, float hi, M material)
        {
            if (hi - lo <= .005f || z - a <= .005f) return;
            Vector3 p = alongZ ? new Vector3(fixedCoord, (lo + hi) * .5f, (a + z) * .5f) :
                new Vector3((a + z) * .5f, (lo + hi) * .5f, fixedCoord);
            Vector3 size = alongZ ? new Vector3(WallThickness, hi - lo, z - a) : new Vector3(z - a, hi - lo, WallThickness);
            Solid(parent, name, p, size, material, .009f);
        }
        static void Partition(GameObject parent, string name, Vector3 p, Vector3 size)
        {
            Solid(parent, name, p, size, M.Taupe, .012f);
            var b = new Builder();
            b.Box(p + Vector3.up * (size.y * .5f), new Vector3(size.x + .035f, .055f, size.z), M.WalnutDark, .01f);
            b.Attach(parent, name + "_cap");
        }
        static void DirectionalTrim(GameObject parent,string side,int floor,bool residence)
        {
            var b=new Builder();
            if(side=="north")
            { b.Box(new Vector3(0,.07f,5.845f),new Vector3(15.75f,.14f,.055f),M.WalnutDark,.008f);
              b.Box(new Vector3(0,3.08f,5.84f),new Vector3(15.75f,.1f,.075f),M.Taupe,.008f); }
            else if(side=="west")
            { b.Box(new Vector3(-7.845f,.07f,0),new Vector3(.055f,.14f,11.75f),M.WalnutDark,.008f);
              b.Box(new Vector3(-7.84f,3.08f,0),new Vector3(.075f,.1f,11.75f),M.Taupe,.008f); }
            else if(side=="south")
              for(int sign=-1;sign<=1;sign+=2)b.Box(new Vector3(sign*4.34f,.07f,-5.845f),new Vector3(7.23f,.14f,.055f),M.WalnutDark,.008f);
            else if(residence&&floor==0)
            { b.Box(new Vector3(7.845f,.07f,(-5.875f+1.55f)*.5f),new Vector3(.055f,.14f,7.425f),M.WalnutDark,.008f);
              b.Box(new Vector3(7.845f,.07f,(2.85f+5.875f)*.5f),new Vector3(.055f,.14f,3.025f),M.WalnutDark,.008f); }
            else b.Box(new Vector3(7.845f,.07f,0),new Vector3(.055f,.14f,11.75f),M.WalnutDark,.008f);
            b.Attach(parent,"DirectionalSkirting_"+side);
        }

        static void InteriorTrim(GameObject parent, bool back)
        {
            var b = new Builder();
            if (back)
            {
                b.Box(new Vector3(0, .07f, 5.845f), new Vector3(15.75f, .14f, .055f), M.WalnutDark, .008f);
                b.Box(new Vector3(-7.845f, .07f, 0), new Vector3(.055f, .14f, 11.75f), M.WalnutDark, .008f);
                b.Box(new Vector3(0, 3.08f, 5.84f), new Vector3(15.75f, .1f, .075f), M.Taupe, .008f);
                b.Box(new Vector3(-7.84f, 3.08f, 0), new Vector3(.075f, .1f, 11.75f), M.Taupe, .008f);
            }
            else
            {
                b.Box(new Vector3(-4.34f, .07f, -5.845f), new Vector3(7.23f, .14f, .055f), M.WalnutDark, .008f);
                b.Box(new Vector3(4.34f, .07f, -5.845f), new Vector3(7.23f, .14f, .055f), M.WalnutDark, .008f);
                b.Box(new Vector3(7.845f, .07f, 0), new Vector3(.055f, .14f, 11.75f), M.WalnutDark, .008f);
            }
            b.Attach(parent, "interior_skirting");
        }

        static void Window(Builder b)
        {
            // Clear glass, not an opaque blue plate. Opening base .95, overall dimensions 1.8 x 1.4.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(s * .859f, 1.65f, 0), new Vector3(.082f, 1.4f, .18f), M.WalnutDark, .012f);
                b.Box(new Vector3(0, 1.65f + s * .659f, 0), new Vector3(1.64f, .082f, .18f), M.WalnutDark, .012f);
                b.Box(new Vector3(s * .423f, 1.65f, -.016f), new Vector3(.775f, 1.208f, .018f), M.Glass, .002f);
            }
            b.Box(new Vector3(0, 1.65f, -.01f), new Vector3(.046f, 1.24f, .12f), M.Walnut, .006f);
            b.Box(new Vector3(0, 1.62f, -.01f), new Vector3(1.64f, .034f, .105f), M.Walnut, .005f);
            b.Box(new Vector3(0, .934f, -.055f), new Vector3(1.98f, .082f, .38f), M.Stone, .015f);
            b.Box(new Vector3(-.065f, 1.48f, -.11f), new Vector3(.027f, .12f, .04f), M.Steel, .007f);
            // Small hinge and drainage hardware are geometry, with no cross-opening backing mesh.
            for (int s = -1; s <= 1; s += 2)
                b.Cylinder(new Vector3(-.802f, 1.65f + s * .43f, -.112f), .018f, .09f, M.Steel, 8);
        }
        static void Door(GameObject root, Builder b)
        {
            // Outer frame straddles the 1.2m opening. Leaf hinge is lower-left at x=-.5.
            for (int s = -1; s <= 1; s += 2)
                b.Box(new Vector3(s * .556f, 1.125f, 0), new Vector3(.085f, 2.25f, .2f), M.WalnutDark, .012f);
            b.Box(new Vector3(0, 2.214f, 0), new Vector3(1.03f, .072f, .2f), M.WalnutDark, .012f);
            b.Box(new Vector3(0, .018f, 0), new Vector3(1.2f, .036f, .23f), M.Steel, .006f);
            var leaf = Group(root, "DoorLeaf"); leaf.transform.localPosition = new Vector3(-.5f, 0, -.005f);
            var d = new Builder();
            d.Box(new Vector3(.5f, 1.08f, 0), new Vector3(1, 2.16f, .085f), M.Walnut, .025f);
            for (int s = -1; s <= 1; s += 2)
            {
                d.Box(new Vector3(.5f, .65f, s * .049f), new Vector3(.75f, .85f, .027f), M.WalnutDark, .018f);
                d.Box(new Vector3(.5f, 1.61f, s * .049f), new Vector3(.75f, .72f, .027f), M.WalnutDark, .018f);
                d.Box(new Vector3(.51f, 1.61f, s * .065f), new Vector3(.63f, .59f, .017f), M.Walnut, .012f);
                d.Box(new Vector3(.51f, .65f, s * .065f), new Vector3(.63f, .72f, .017f), M.Walnut, .012f);
                d.Box(new Vector3(.86f, 1.03f, s * .077f), new Vector3(.07f, .2f, .025f), M.Steel, .015f);
                d.Box(new Vector3(.81f, 1.06f, s * .112f), new Vector3(.18f, .035f, .038f), M.Steel, .012f);
            }
            for (int i = 0; i < 3; i++) d.Cylinder(new Vector3(.015f, .25f + i * .8f, .04f), .023f, .12f, M.Steel, 8);
            d.Attach(leaf, "door_leaf");
        }
        static void Tree(Builder b)
        {
            b.Cone(new Vector3(0, 1.8f, 0), .25f, .13f, 3.6f, M.WalnutDark, 9);
            for (int i = 0; i < 7; i++)
            {
                float a = i * 2.39996f;
                Vector3 p = new Vector3(Mathf.Cos(a) * (1.05f + i % 2 * .22f), 3.5f + i % 3 * .64f, Mathf.Sin(a) * 1.18f);
                b.Beam(new Vector3(0, 2.1f + i * .16f, 0), p, .12f, M.WalnutDark);
                b.Ellipsoid(p + Vector3.up * .48f, new Vector3(1.5f, 1.4f, 1.34f), i % 3 == 0 ? M.Olive : M.Leaf, 83 + i);
            }
            b.Ellipsoid(new Vector3(.1f, 5.42f, -.05f), new Vector3(1.48f, 1.47f, 1.38f), M.Leaf, 38);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3;
                b.Beam(new Vector3(0, .18f, 0), new Vector3(Mathf.Cos(a) * .65f, -.015f, Mathf.Sin(a) * .65f), .13f, M.WalnutDark);
            }
        }
        static void Bush(Builder b, Vector3 p, float scale, int seed)
        {
            b.Ellipsoid(p + Vector3.up * scale * .52f, new Vector3(scale, scale * .74f, scale * .82f), M.Leaf, seed);
            b.Ellipsoid(p + new Vector3(scale * .55f, scale * .42f, .05f), new Vector3(scale * .62f, scale * .54f, scale * .62f), M.Olive, seed + 10);
        }
        static void Fence(Builder b)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(s * .94f, .61f, 0), new Vector3(.12f, 1.22f, .14f), M.WalnutDark, .02f);
                b.Box(new Vector3(s * .94f, 1.24f, 0), new Vector3(.17f, .07f, .18f), M.Walnut, .02f);
            }
            for (int i = 0; i < 2; i++)
                b.Box(new Vector3(0, .31f + i * .55f, .03f), new Vector3(1.88f, .09f, .07f), M.WalnutDark, .01f);
            for (int i = 0; i < 11; i++)
            {
                float x = -.82f + i * .164f;
                b.Box(new Vector3(x, .6f, -.035f), new Vector3(.12f, 1.12f, .065f), i % 4 == 0 ? M.Walnut : M.Olive, .016f);
                for (int k = 0; k < 2; k++)
                    b.Cylinder(new Vector3(x, .31f + k * .55f, -.074f), .011f, .008f, M.Steel, 6, Quaternion.Euler(90, 0, 0));
            }
        }
        static void Drain(Builder b)
        {
            // Dark inset channel plus sloping stone banks, raised excavated-earth edges and removable grate.
            b.Box(new Vector3(0, -.008f, 0), new Vector3(2.55f, .025f, .66f), M.Black, .015f);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0, .014f, s * .34f), new Vector3(2.8f, .095f, .23f), M.Earth, .04f);
                for (int i = 0; i < 12; i++)
                    b.Ellipsoid(new Vector3(-1.24f + i * .224f, .025f, s * .25f), new Vector3(.14f, .07f, .12f), M.Stone, i + s * 13);
                b.Box(new Vector3(0, .062f, s * .17f), new Vector3(2.48f, .035f, .028f), M.Charcoal, .005f);
            }
            for (int i = 0; i < 24; i++)
                b.Box(new Vector3(-1.17f + i * .102f, .067f, 0), new Vector3(.027f, .026f, .36f), M.Steel, .004f);
            b.Box(new Vector3(-1.31f, .026f, 0), new Vector3(.11f, .075f, .54f), M.Stone, .015f);
            b.Box(new Vector3(1.31f, .026f, 0), new Vector3(.11f, .075f, .54f), M.Stone, .015f);
        }
        static void Hose(Builder b)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                b.Beam(new Vector3(s * .32f, .06f, -.22f), new Vector3(s * .32f, .7f, .12f), .048f, M.Charcoal);
                b.Beam(new Vector3(s * .32f, .06f, .28f), new Vector3(s * .32f, .7f, .12f), .048f, M.Charcoal);
                b.Cylinder(new Vector3(0, .65f, s * .16f), .33f, .035f, M.Charcoal, 20, Quaternion.Euler(90, 0, 0));
            }
            for (int i = 0; i < 6; i++)
                b.Torus(new Vector3(0, .65f, -.125f + i * .05f), .235f, .035f, M.Olive, Quaternion.Euler(90, 0, 0));
            b.Beam(new Vector3(.05f, .4f, -.2f), new Vector3(.55f, .08f, -.4f), .055f, M.Olive);
            b.Beam(new Vector3(.55f, .08f, -.4f), new Vector3(.78f, .09f, -.49f), .085f, M.Steel);
            b.Box(new Vector3(.63f, .13f, -.43f), new Vector3(.1f, .16f, .06f), M.SafetyAmber, .01f, Quaternion.Euler(0, 25, -25));
            b.Beam(new Vector3(.34f, .65f, .12f), new Vector3(.47f, .65f, .12f), .045f, M.Steel);
            b.Beam(new Vector3(.47f, .65f, .12f), new Vector3(.47f, .81f, .12f), .045f, M.Steel);
            b.Box(new Vector3(.47f, .83f, .12f), new Vector3(.075f, .12f, .075f), M.Walnut, .015f);
        }
        static void Toolbox(GameObject root, Builder b)
        {
            float w = .9f, h = .43f, d = .48f;
            OpenBox(b, w, h, d, .035f, M.Slate, M.Charcoal);
            for (int s = -1; s <= 1; s += 2)
                b.Box(new Vector3(s * .29f, .36f, -.257f), new Vector3(.07f, .12f, .035f), M.Steel, .01f);
            for (int i = 0; i < 5; i++)
                b.Box(new Vector3(-.3f + i * .14f, .12f + (i % 2) * .03f, 0), new Vector3(.065f, .07f, .31f), i % 2 == 0 ? M.Walnut : M.Steel, .015f,
                    Quaternion.Euler(0, -12 + i * 7, 0));
            var lid = Group(root, "Lid"); lid.transform.localPosition = new Vector3(0, h, d * .5f);
            var l = new Builder();
            l.Box(new Vector3(0, .035f, -d * .5f), new Vector3(w + .015f, .1f, d + .015f), M.Slate, .025f);
            l.Box(new Vector3(0, .09f, -d * .5f), new Vector3(.39f, .025f, .18f), M.Charcoal, .01f);
            l.Beam(new Vector3(-.17f, .1f, -.24f), new Vector3(-.17f, .19f, -.24f), .035f, M.Steel);
            l.Beam(new Vector3(.17f, .1f, -.24f), new Vector3(.17f, .19f, -.24f), .035f, M.Steel);
            l.Beam(new Vector3(-.17f, .19f, -.24f), new Vector3(.17f, .19f, -.24f), .05f, M.Charcoal);
            l.Attach(lid, "toolbox_lid");
        }
        static void Cabinet(GameObject root, Builder b, float width, float height, float depth, bool medical, bool kitchen)
        {
            M body = medical ? M.Linen : (kitchen ? M.Walnut : M.Slate);
            float t = .045f, bottom = kitchen ? .1f : .06f;
            // A true empty volume behind the opening leaf. No solid cabinet cube blocks its contents.
            b.Box(new Vector3(0, bottom + t * .5f, 0), new Vector3(width, t, depth), M.Charcoal, .012f);
            b.Box(new Vector3(0, height - t * .5f, 0), new Vector3(width, t, depth), body, .012f);
            b.Box(new Vector3(0, (height + bottom) * .5f, depth * .5f - t * .5f), new Vector3(width, height - bottom, t), body, .012f);
            for (int s = -1; s <= 1; s += 2)
                b.Box(new Vector3(s * (width - t) * .5f, (height + bottom) * .5f, 0), new Vector3(t, height - bottom, depth), body, .012f);
            int shelves = medical ? 2 : (kitchen ? 1 : 3);
            for (int i = 1; i <= shelves; i++)
            {
                float y = bottom + (height - bottom) * i / (shelves + 1);
                b.Box(new Vector3(0, y, .025f), new Vector3(width - .08f, .03f, depth - .1f), kitchen ? M.WalnutDark : M.Steel, .006f);
                for (int j = 0; j < 3; j++)
                {
                    float x = -width * .28f + j * width * .28f;
                    if (medical)
                    {
                        b.Cylinder(new Vector3(x, y + .1f, -.015f), .045f, .17f, j == 1 ? M.Slate : M.Linen, 10);
                        b.Cylinder(new Vector3(x, y + .194f, -.015f), .036f, .022f, M.Charcoal, 10);
                        b.Box(new Vector3(x, y + .1f, -.06f), new Vector3(.047f, .065f, .005f), M.Taupe, .002f);
                    }
                    else b.Box(new Vector3(x, y + .09f, .035f), new Vector3(width * .19f, .15f, depth * .46f), j == 1 ? M.Linen : M.Olive, .013f);
                }
            }
            if (kitchen)
            {
                b.Box(new Vector3(0, height + .025f, -.025f), new Vector3(width + .08f, .075f, depth + .09f), M.Stone, .022f);
                b.Box(new Vector3(0, .055f, .05f), new Vector3(width - .13f, .11f, depth - .18f), M.WalnutDark, .008f);
            }
            else for (int s = -1; s <= 1; s += 2)
                for (int z = -1; z <= 1; z += 2)
                    b.Box(new Vector3(s * (width * .5f - .075f), .035f, z * (depth * .5f - .07f)), new Vector3(.08f, .07f, .08f), M.Charcoal, .008f);
            var leaf = Group(root, "DoorLeaf");
            leaf.transform.localPosition = new Vector3(-width * .5f + .045f, bottom, -depth * .5f - .016f);
            var d = new Builder();
            float leafW = width - .09f, leafH = height - bottom - .025f;
            d.Box(new Vector3(leafW * .5f, leafH * .5f, 0), new Vector3(leafW, leafH, .045f), body, .015f);
            if (kitchen)
            {
                d.Box(new Vector3(leafW * .5f, leafH * .5f, -.028f), new Vector3(leafW - .14f, leafH - .16f, .015f), M.WalnutDark, .01f);
                d.Box(new Vector3(leafW * .5f, leafH * .5f, -.04f), new Vector3(leafW - .2f, leafH - .22f, .012f), M.Walnut, .008f);
            }
            else if (!medical)
                for (int i = 0; i < 6; i++)
                    d.Box(new Vector3(leafW * .5f, leafH - .2f - i * .055f, -.026f), new Vector3(leafW * .67f, .018f, .012f), M.Charcoal, .003f);
            if (medical)
            {
                d.Box(new Vector3(leafW * .5f, leafH * .56f, -.029f), new Vector3(.075f, .32f, .015f), M.Red, .007f);
                d.Box(new Vector3(leafW * .5f, leafH * .56f, -.03f), new Vector3(.28f, .075f, .018f), M.Red, .007f);
            }
            d.Box(new Vector3(leafW - .08f, leafH * .53f, -.055f), new Vector3(.026f, .17f, .04f), M.Steel, .008f);
            for (int i = 0; i < 2; i++)
                d.Cylinder(new Vector3(.006f, .16f + i * Mathf.Max(.18f, leafH - .32f), .012f), .019f, .09f, M.Steel, 8);
            d.Attach(leaf, "cabinet_leaf");
        }
        static void Crate(GameObject root, Builder b)
        {
            const float width = 1.1f, height = .71f, depth = .8f;
            OpenBox(b, width, height, depth, .045f, M.Walnut, M.WalnutDark);
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0, .13f + i * .15f, -.405f), new Vector3(1.01f, .009f, .008f), M.WalnutDark, .001f);
                b.Box(new Vector3(0, .13f + i * .15f, .405f), new Vector3(1.01f, .009f, .008f), M.WalnutDark, .001f);
            }
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(s * .43f, .36f, -.43f), new Vector3(.095f, .7f, .04f), M.Charcoal, .009f);
                b.Box(new Vector3(s * .565f, .47f, 0), new Vector3(.04f, .05f, .27f), M.Steel, .01f);
            }
            for (int i = 0; i < 3; i++)
                b.Box(new Vector3(-.31f + i * .31f, .2f, 0), new Vector3(.24f, .27f + i % 2 * .1f, .42f), i == 1 ? M.Slate : M.Linen, .02f);
            var lid = Group(root, "Lid"); lid.transform.localPosition = new Vector3(0, height, depth * .5f);
            var d = new Builder();
            for (int i = 0; i < 5; i++)
                d.Box(new Vector3(-.44f + i * .22f, .026f, -depth * .5f), new Vector3(.212f, .052f, .82f), i % 2 == 0 ? M.Walnut : M.WalnutLight, .01f);
            for (int s = -1; s <= 1; s += 2)
                d.Box(new Vector3(s * .43f, .058f, -depth * .5f), new Vector3(.095f, .025f, .83f), M.Charcoal, .006f);
            d.Attach(lid, "crate_lid");
        }
        static void OpenBox(Builder b, float width, float height, float depth, float t, M outer, M inner)
        {
            b.Box(new Vector3(0, t * .5f, 0), new Vector3(width, t, depth), inner, .01f);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(s * (width - t) * .5f, height * .5f, 0), new Vector3(t, height, depth), outer, .01f);
                b.Box(new Vector3(0, height * .5f, s * (depth - t) * .5f), new Vector3(width - t, height, t), outer, .01f);
            }
        }
        static void Fridge(GameObject root, Builder b)
        {
            const float width = .9f, height = 1.9f, depth = .83f, t = .045f;
            // Hollow enamel housing; the front is not backed by an opaque display panel.
            b.Box(new Vector3(0, .09f, 0), new Vector3(width, .12f, depth), M.Charcoal, .018f);
            b.Box(new Vector3(0, height - t * .5f, 0), new Vector3(width, t, depth), M.Linen, .02f);
            b.Box(new Vector3(0, 1.02f, depth * .5f - t * .5f), new Vector3(width, height - .12f, t), M.Linen, .02f);
            for (int s = -1; s <= 1; s += 2)
                b.Box(new Vector3(s * (width - t) * .5f, 1.02f, 0), new Vector3(t, height - .12f, depth), M.Linen, .02f);
            for (int i = 0; i < 4; i++)
            {
                float y = .25f + i * .38f;
                b.Box(new Vector3(0, y, -.015f), new Vector3(.78f, .025f, .69f), M.Steel, .006f);
                b.Box(new Vector3(0, y + .007f, -.361f), new Vector3(.78f, .045f, .025f), M.Linen, .006f);
                if (i < 3)
                    for (int j = 0; j < 3; j++)
                    {
                        float x = -.25f + j * .25f;
                        if (i == 0)
                        {
                            b.Cylinder(new Vector3(x, y + .13f, .06f), .065f, .22f, M.Slate, 12);
                            b.Cylinder(new Vector3(x, y + .252f, .06f), .035f, .035f, M.Charcoal, 10);
                            b.Box(new Vector3(x, y + .13f, -.008f), new Vector3(.08f, .09f, .008f), M.Taupe, .003f);
                        }
                        else b.Box(new Vector3(x, y + .09f, .05f), new Vector3(.17f, .15f, .31f), j % 2 == 0 ? M.Olive : M.Taupe, .018f);
                    }
            }
            // Cooling grille and compressor remain outside the hollow storage volume.
            for (int i = 0; i < 5; i++)
                b.Box(new Vector3(0, .06f + i * .015f, -.423f), new Vector3(.65f, .006f, .016f), M.Steel, .002f);
            var leaf = Group(root, "DoorLeaf");
            leaf.transform.localPosition = new Vector3(-.425f, .13f, -.437f);
            var d = new Builder();
            d.Box(new Vector3(.425f, .865f, 0), new Vector3(.85f, 1.73f, .09f), M.Linen, .028f);
            d.Box(new Vector3(.425f, .865f, .054f), new Vector3(.76f, 1.63f, .035f), M.Taupe, .022f);
            // Interior door pockets swing with the actual leaf and stay behind it when closed.
            for (int i = 0; i < 3; i++)
            {
                float y = .22f + i * .43f;
                d.Box(new Vector3(.425f, y, .11f), new Vector3(.67f, .035f, .13f), M.Linen, .008f);
                d.Box(new Vector3(.425f, y + .09f, .167f), new Vector3(.67f, .16f, .027f), M.Linen, .008f);
                for (int s = -1; s <= 1; s += 2)
                    d.Box(new Vector3(.425f + s * .32f, y + .08f, .11f), new Vector3(.026f, .15f, .13f), M.Linen, .006f);
            }
            d.Box(new Vector3(.738f, .83f, -.073f), new Vector3(.035f, .61f, .065f), M.Steel, .012f);
            d.Box(new Vector3(.18f, 1.48f, -.052f), new Vector3(.16f, .07f, .012f), M.Charcoal, .004f);
            for (int i = 0; i < 2; i++)
                d.Cylinder(new Vector3(.008f, .08f + i * 1.55f, .025f), .022f, .11f, M.Steel, 8);
            d.Attach(leaf, "fridge_door");
        }
        static void Stairs(Builder b)
        {
            // Root adds the physical step colliders. This run occupies x+/- .75, z0..4.8, y0..3.2.
            // Open stringer stair: .064m treads/.028m risers never fill the space below a flight.
            // A second identical flight 3.2m below therefore retains full headroom.
            for (int i = 0; i < 16; i++)
            {
                float top = (i + 1) * .2f, z = i * .3f + .15f;
                b.Box(new Vector3(0, top - .032f, z), new Vector3(1.5f, .064f, .31f), M.Walnut, .012f);
                b.Box(new Vector3(0, top - .115f, z - .142f), new Vector3(1.45f, .17f, .028f), M.WalnutDark, .006f);
                b.Box(new Vector3(0, top + .002f, z - .12f), new Vector3(1.46f, .012f, .035f), M.Charcoal, .003f);
            }
            for (int s = -1; s <= 1; s += 2)
            {
                b.Beam(new Vector3(s * .69f, .04f, .03f), new Vector3(s * .69f, 3.03f, 4.65f), .12f, M.WalnutDark);
                for (int i = 0; i < 8; i++)
                {
                    float z = .15f + i * .6f, y = .2f + i * .4f;
                    b.Box(new Vector3(s * .79f, y + .44f, z), new Vector3(.045f, .88f, .045f), M.Charcoal, .007f);
                }
                b.Beam(new Vector3(s * .79f, 1.11f, .12f), new Vector3(s * .79f, 4.12f, 4.65f), .075f, M.Walnut, .015f);
            }
        }
        static void Workbench(Builder b)
        {
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    b.Box(new Vector3(x * .81f, .42f, z * .3f), new Vector3(.09f, .84f, .09f), M.Charcoal, .012f);
            b.Box(new Vector3(0, .2f, .03f), new Vector3(1.7f, .05f, .58f), M.WalnutDark, .01f);
            for (int i = 0; i < 5; i++)
                b.Box(new Vector3(0, .885f, -.32f + i * .16f), new Vector3(1.98f, .11f, .154f), i % 3 == 0 ? M.WalnutLight : M.Walnut, .012f);
            b.Box(new Vector3(0, 1.4f, .37f), new Vector3(1.87f, .9f, .055f), M.Slate, .012f);
            for (int row = 0; row < 7; row++) for (int i = 0; i < 17; i++)
                b.Cylinder(new Vector3(-.8f + i * .1f, 1.05f + row * .11f, .334f), .012f, .004f, M.Charcoal, 6, Quaternion.Euler(90, 0, 0));
            for (int i = 0; i < 5; i++)
            {
                float x = -.65f + i * .3f;
                b.Box(new Vector3(x, 1.36f, .29f), new Vector3(.045f, .3f + i % 2 * .12f, .045f), M.Walnut, .008f, Quaternion.Euler(0, 0, i % 2 * 8));
                b.Box(new Vector3(x, 1.55f, .29f), new Vector3(.15f, .07f, .06f), M.Steel, .01f);
            }
            b.Box(new Vector3(.65f, .99f, -.22f), new Vector3(.28f, .18f, .23f), M.Steel, .025f);
            b.Box(new Vector3(.65f, 1.1f, -.22f), new Vector3(.38f, .08f, .13f), M.Charcoal, .015f);
            b.Beam(new Vector3(.65f, 1.015f, -.36f), new Vector3(.65f, 1.015f, -.57f), .025f, M.Steel);
            b.Beam(new Vector3(.51f, 1.015f, -.57f), new Vector3(.79f, 1.015f, -.57f), .028f, M.Steel);
            b.Box(new Vector3(-.48f, .98f, -.06f), new Vector3(.4f, .07f, .3f), M.Taupe, .015f);
            b.Box(new Vector3(-.48f, 1.021f, -.06f), new Vector3(.31f, .012f, .22f), M.Linen, .003f);
        }
        static void Generator(Builder b)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                    b.Beam(new Vector3(s * .53f, .08f, z * .32f), new Vector3(s * .53f, .78f, z * .32f), .06f, M.Charcoal);
                b.Beam(new Vector3(s * .53f, .78f, -.32f), new Vector3(s * .53f, .78f, .32f), .06f, M.Charcoal);
                b.Beam(new Vector3(-.53f, .08f, s * .32f), new Vector3(.53f, .08f, s * .32f), .06f, M.Charcoal);
            }
            b.Cylinder(new Vector3(-.12f, .35f, 0), .23f, .65f, M.Steel, 14, Quaternion.Euler(0, 0, 90));
            for (int i = 0; i < 8; i++)
                b.Box(new Vector3(.2f, .31f + i * .027f, .02f), new Vector3(.31f, .012f, .33f), M.Charcoal, .003f);
            b.Box(new Vector3(0, .65f, .04f), new Vector3(.82f, .16f, .46f), M.Olive, .035f);
            b.Cylinder(new Vector3(.23f, .751f, .06f), .052f, .04f, M.Charcoal, 12);
            b.Box(new Vector3(-.27f, .43f, -.286f), new Vector3(.4f, .27f, .05f), M.Slate, .016f);
            for (int i = 0; i < 2; i++)
                b.Cylinder(new Vector3(-.35f + i * .16f, .44f, -.32f), .043f, .015f, M.Black, 12, Quaternion.Euler(90, 0, 0));
            b.Box(new Vector3(-.27f, .52f, -.32f), new Vector3(.16f, .03f, .009f), M.SafetyAmber, .004f);
            b.Beam(new Vector3(.29f, .45f, .12f), new Vector3(.43f, .61f, .23f), .055f, M.Steel);
            b.Cylinder(new Vector3(.42f, .64f, .24f), .08f, .16f, M.Charcoal, 12);
        }

        static void Roof(Builder b, Vector3 offset, float width, float depth, float eave, float ridge)
        {
            float half = depth * .5f, rise = ridge - eave;
            float length = Mathf.Sqrt(half * half + rise * rise);
            float angle = Mathf.Atan2(rise, half) * Mathf.Rad2Deg;
            for (int s = -1; s <= 1; s += 2)
            {
                var rot = Quaternion.Euler(s * angle, 0, 0);
                Vector3 center = offset + new Vector3(0, (eave + ridge) * .5f, s * half * .5f);
                b.Box(center, new Vector3(width, .14f, length), M.Slate, .012f, rot);
                b.Box(offset + new Vector3(0, eave - .04f, s * half), new Vector3(width + .12f, .18f, .12f), M.WalnutDark, .016f);
                for (int i = 0; i <= Mathf.CeilToInt(width / .43f); i++)
                {
                    float x = -width * .5f + i * width / Mathf.CeilToInt(width / .43f);
                    b.Box(center + new Vector3(x, .08f, 0), new Vector3(.022f, .021f, length), M.Charcoal, .003f, rot);
                }
                b.Cylinder(offset + new Vector3(0, eave - .12f, s * (half + .07f)), .08f, width, M.Charcoal, 10, Quaternion.Euler(0, 0, 90));
                b.Cylinder(offset + new Vector3(width * .5f - .16f, eave * .5f, s * (half + .07f)), .047f, eave, M.Charcoal, 10);
            }
            b.Cylinder(offset + Vector3.up * (ridge + .08f), .14f, width + .15f, M.Charcoal, 10, Quaternion.Euler(0, 0, 90));
        }
        // Only visual ground is carved. Physics terrain and the house's open basement contract are unchanged.
        static void CreekCutGround(Builder b,Vector3 centre,Vector3 size,M m,float bevel=.005f)
        {
            float left=centre.x-size.x*.5f,right=centre.x+size.x*.5f,near=centre.z-size.z*.5f,far=centre.z+size.z*.5f;
            float cl=Mathf.Max(left,PanicExteriorArt.CreekLeft),cr=Mathf.Min(right,PanicExteriorArt.CreekRight);
            float cn=Mathf.Max(near,-PanicExteriorArt.CreekLength*.5f),cf=Mathf.Min(far,PanicExteriorArt.CreekLength*.5f);
            if(cl>=cr||cn>=cf){b.Box(centre,size,m,bevel);return;}
            Action<float,float,float,float> slab=(x0,x1,z0,z1)=>{if(x1-x0>.001f&&z1-z0>.001f)b.Box(new Vector3((x0+x1)*.5f,centre.y,(z0+z1)*.5f),new Vector3(x1-x0,size.y,z1-z0),m,bevel);};
            slab(left,cl,near,far);slab(cr,right,near,far);slab(cl,cr,near,cn);slab(cl,cr,cf,far);
        }
        static void ExteriorStreetDetails(Builder b)
        {
            Action<Vector3,Vector3,float> drive=(a,z,width)=>
            {
                Vector3 delta=z-a;Quaternion q=Quaternion.LookRotation(delta.normalized,Vector3.up);
                b.Box((a+z)*.5f,new Vector3(width,.05f,delta.magnitude),M.Concrete,.018f,q);
                Vector3 across=q*Vector3.right*(width*.5f-.06f);
                for(int side=-1;side<=1;side+=2)b.Box((a+z)*.5f+across*side+Vector3.up*.028f,new Vector3(.035f,.004f,delta.magnitude),M.Stone,.001f,q);
            };
            // The bunker is at(11,-7),xmax11.93. This bend keeps its cover and approach route clear.
            drive(new Vector3(11.7f,-.0115f,.13f),new Vector3(11.7f,-.0115f,-2.65f),3.2f);
            drive(new Vector3(11.7f,-.0115f,-2.25f),new Vector3(14,-.0115f,-5.3f),3.2f);
            drive(new Vector3(14,-.0115f,-4.45f),new Vector3(14,-.0115f,-9.1f),3.2f);
            drive(new Vector3(14,-.0115f,-8.8f),new Vector3(11.7f,-.0115f,-11.65f),3.2f);
            drive(new Vector3(11.7f,-.0115f,-11.2f),new Vector3(11.7f,-.0115f,-15.60f),3.2f);
            for(int side=-1;side<=1;side+=2)
            {
                float x=side<0?9.38f:14.12f;
                b.Box(new Vector3(x,.44f,-13),new Vector3(.16f,.88f,.20f),M.Brick,.016f);
                b.Box(new Vector3(x,.91f,-13),new Vector3(.22f,.06f,.26f),M.Linen,.012f);
                b.Box(new Vector3(x,.57f,-13.11f),new Vector3(.045f,.24f,.012f),M.SafetyAmber,.003f);
                float drainX=side<0?10.12f:13.28f;
                b.Box(new Vector3(drainX,.018f,-14.65f),new Vector3(.18f,.025f,.78f),M.Charcoal,.005f);
                for(int i=0;i<7;i++)b.Box(new Vector3(drainX,.032f,-14.95f+i*.1f),new Vector3(.15f,.008f,.025f),M.Steel,.002f);
            }
            // Modular sidewalk joints and two inhabited-street lamps stay clear of entrances.
            for(int i=-23;i<=23;i++)
                for(int side=-1;side<=1;side+=2)
                {
                    float x=i*2f;if(side>0&&x>9.9f&&x<13.5f)continue;
                    b.Box(new Vector3(x,.080f,-19+side*4.12f),new Vector3(.018f,.003f,.60f),M.Stone,.001f);
                }
            foreach(float x in new[]{-10f,18f})
            {
                b.Cylinder(new Vector3(x,1.85f,-14.2f),.055f,3.7f,M.Steel,10);
                b.Box(new Vector3(x+.23f,3.68f,-14.2f),new Vector3(.5f,.065f,.075f),M.Steel,.009f);
                b.Box(new Vector3(x+.44f,3.62f,-14.2f),new Vector3(.28f,.12f,.22f),M.Linen,.015f);
                b.Box(new Vector3(x+.44f,3.55f,-14.2f),new Vector3(.21f,.015f,.16f),M.WarmWindow,.004f);
            }
        }
        static void Neighbour(Builder b, Vector3 p, float width, float depth, float height, float yaw, bool pale)
        {
            // Far scenery only: no collider, doors or runtime components. Facade remains desaturated.
            var q = Quaternion.Euler(0, yaw, 0);
            Action<Vector3, Vector3, M, float> panel = (pos, size, m, radius) => b.Box(p + q * pos, size, m, radius, q);
            panel(new Vector3(0, height * .5f, 0), new Vector3(width, height, depth), pale ? M.FacadeSage : ((Mathf.Abs(p.x)%3)>1?M.FacadeOchre:M.FacadeIvory), .035f);
            panel(new Vector3(0, .2f, 0), new Vector3(width + .08f, .4f, depth + .08f), M.Brick, .02f);
            // Roof geometry is symmetric, so the yaw does not change the building silhouette.
            Roof(b, p, width + .65f, depth + .65f, height, height + depth * .26f);
            for (int floor = 0; floor < 2; floor++)
                for (int side = -1; side <= 1; side += 2)
                    for (int face = -1; face <= 1; face += 2)
                    {
                        Vector3 pos = new Vector3(side * width * .28f, 1.45f + floor * (height * .48f), face * (depth * .5f + .025f));
                        panel(pos, new Vector3(1.15f, 1.25f, .055f), M.Charcoal, .015f);
                        panel(pos + new Vector3(0, 0, face * .033f), new Vector3(.98f, 1.08f, .03f), M.Slate, .008f);
                        panel(pos + new Vector3(0, 0, face * .054f), new Vector3(.05f, 1.12f, .025f), M.Taupe, .003f);
                        panel(pos + new Vector3(0, -.62f, face * .04f), new Vector3(1.29f, .08f, .17f), M.Stone, .01f);
                    }
            panel(new Vector3(0, 1.02f, -depth * .5f - .04f), new Vector3(.9f, 2.04f, .09f), M.WalnutDark, .012f);
            // Recesses and trims, not a single painted rectangle; side walls also have windows.
            for(int floor=0;floor<2;floor++)for(int face=-1;face<=1;face+=2)
            {
                float y=1.45f+floor*(height*.48f);
                for(int side=-1;side<=1;side+=2)
                {
                    float x=side*width*.28f,z=face*(depth*.5f+.09f);
                    for(int edge=-1;edge<=1;edge+=2)
                    {
                        panel(new Vector3(x+edge*.56f,y,z),new Vector3(.075f,1.28f,.07f),M.Linen,.007f);
                        panel(new Vector3(x,y+edge*.61f,z),new Vector3(1.18f,.075f,.07f),M.Linen,.007f);
                    }
                    panel(new Vector3(x,y,face*(depth*.5f+.084f)),new Vector3(.97f,.05f,.018f),M.Taupe,.003f);
                    // A restrained interior blind on alternating windows gives occupation without glow spam.
                    if((floor+side+face+6)%3==0)panel(new Vector3(x,y+.23f,face*(depth*.5f+.077f)),new Vector3(.88f,.35f,.012f),M.WarmWindow,.003f);
                    float sx=side*(width*.5f+.035f),sz=face*depth*.24f;
                    panel(new Vector3(sx,y,sz),new Vector3(.055f,1.25f,1.03f),M.Charcoal,.01f);
                    panel(new Vector3(sx+side*.034f,y,sz),new Vector3(.028f,1.08f,.87f),M.Slate,.005f);
                    for(int edge=-1;edge<=1;edge+=2)
                        panel(new Vector3(sx+side*.05f,y+edge*.59f,sz),new Vector3(.065f,.065f,1.10f),M.Linen,.005f);
                    panel(new Vector3(sx+side*.05f,y,sz),new Vector3(.04f,1.13f,.055f),M.Linen,.003f);
                }
            }
            panel(new Vector3(0,height*.48f,0),new Vector3(width+.10f,.095f,depth+.10f),M.Linen,.01f);
            for(int side=-1;side<=1;side+=2)
            {
                panel(new Vector3(side*(width*.5f-.055f),height*.5f,-depth*.5f-.035f),new Vector3(.13f,height-.25f,.08f),M.Linen,.01f);
                panel(new Vector3(side*.52f,1.08f,-depth*.5f-.10f),new Vector3(.075f,2.17f,.10f),M.Linen,.008f);
            }
            panel(new Vector3(0,2.19f,-depth*.5f-.13f),new Vector3(1.18f,.12f,.30f),M.Stone,.014f);
            panel(new Vector3(.31f,1.0f,-depth*.5f-.10f),new Vector3(.032f,.12f,.036f),M.Steel,.004f);
            panel(new Vector3(.77f,1.65f,-depth*.5f-.085f),new Vector3(.24f,.16f,.055f),M.SafetyAmber,.007f);
            panel(new Vector3(0,.08f,-depth*.5f-.35f),new Vector3(1.45f,.16f,.62f),M.Stone,.018f);
            panel(new Vector3(0,.025f,-depth*.5f-.74f),new Vector3(1.75f,.05f,.28f),M.Concrete,.012f);
            for(int i=0;i<6;i++)
            {
                float x=-width*.5f+i*width/5;
                if(Mathf.Abs(x)<1.12f)continue;
                Bush(b,p+q*new Vector3(x,0,-depth*.5f-1.1f),.5f,81+i);
            }
        }

        static GameObject Group(GameObject parent, string name)
        {
            var go = new GameObject(name); go.transform.SetParent(parent.transform, false); return go;
        }
        static void Place(GameObject parent, GameObject child, Vector3 p, float yaw)
        {
            child.transform.SetParent(parent.transform, false); child.transform.localPosition = p;
            child.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        }
        static void Solid(GameObject parent, string name, Vector3 p, Vector3 size, M material, float radius)
        {
            // Deliberately one mesh per collision piece: a bounds collider cannot bridge a window/door hole.
            var go = Group(parent, "solid_" + name); go.transform.localPosition = p;
            go.AddComponent<MeshFilter>().sharedMesh = BoxMesh(size, radius);
            go.AddComponent<MeshRenderer>().sharedMaterial = palette[(int)material];
        }
        internal static void EnsurePalette() { Init(); }
        static void Init()
        {
            if (palette != null && palette.Length > 0 && palette[0]) return;
            var colors = new[] {
                new Color(.25f,.19f,.145f), new Color(.32f,.25f,.19f), new Color(.16f,.125f,.10f),
                new Color(.105f,.13f,.145f), new Color(.32f,.37f,.38f), new Color(.49f,.47f,.42f),
                new Color(.37f,.355f,.31f), new Color(.24f,.285f,.19f), new Color(.17f,.23f,.15f),
                new Color(.245f,.29f,.205f), new Color(.205f,.17f,.125f), new Color(.39f,.40f,.375f),
                new Color(.34f,.365f,.35f), new Color(.23f,.29f,.32f), new Color(.33f,.25f,.205f),
                new Color(.56f,.54f,.47f), new Color(.65f,.43f,.17f), new Color(.43f,.18f,.14f),
                new Color(.55f,.67f,.68f,.15f), new Color(.045f,.055f,.055f), new Color(.19f,.205f,.205f),
                new Color(.46f,.49f,.49f), new Color(.42f,.55f,.48f), new Color(.66f,.47f,.31f),
                new Color(.72f,.72f,.62f), new Color(.80f,.64f,.36f) };
            Shader shader = Shader.Find("Standard");
            if (!shader) shader = Shader.Find("Legacy Shaders/Diffuse");
            palette = new Material[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                var m = new Material(shader) { name = "HomeV2_" + (M)i, color = colors[i] };
                if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", i == (int)M.Steel ? .38f : .18f);
                if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", i == (int)M.Steel ? .6f : 0);
                if (i == (int)M.Glass) { Transparent(m); m.SetFloat("_Glossiness", .65f); }
                palette[i] = m;
            }
        }
        static void Transparent(Material m)
        {
            if (m.HasProperty("_Mode")) m.SetFloat("_Mode", 3);
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha); m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0); m.DisableKeyword("_ALPHATEST_ON"); m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.EnableKeyword("_ALPHABLEND_ON"); m.renderQueue = 3000;
        }

        internal sealed class Builder
        {
            readonly Dictionary<int, List<CombineInstance>> batches = new Dictionary<int, List<CombineInstance>>();
            void Add(M m, Mesh mesh, Vector3 p, Quaternion q, Vector3 scale)
            {
                List<CombineInstance> group;
                if (!batches.TryGetValue((int)m, out group)) { group = new List<CombineInstance>(); batches[(int)m] = group; }
                group.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(p, q, scale) });
            }
            public void Box(Vector3 p, Vector3 size, M m, float bevel = .015f, Quaternion rotation = default(Quaternion))
            {
                if (rotation.Equals(default(Quaternion))) rotation = Quaternion.identity;
                Add(m, BoxMesh(size, bevel), p, rotation, Vector3.one);
            }
            public void Cylinder(Vector3 p, float radius, float height, M m, int sides = 12, Quaternion rotation = default(Quaternion))
            {
                if (rotation.Equals(default(Quaternion))) rotation = Quaternion.identity;
                string key = "cylinder_" + sides;
                Mesh mesh;
                if (!shapes.TryGetValue(key, out mesh) || !mesh) { mesh = CylinderMesh(1, 1, sides); shapes[key] = mesh; }
                Add(m, mesh, p, rotation, new Vector3(radius, height, radius));
            }
            public void Cone(Vector3 p, float bottom, float top, float height, M m, int sides)
            {
                Add(m, CylinderMesh(bottom, top, sides), p, Quaternion.identity, new Vector3(1, height, 1));
            }
            public void Beam(Vector3 a, Vector3 z, float width, M m, float radius = .006f)
            {
                Vector3 delta = z - a;
                if (delta.sqrMagnitude < .00001f) return;
                Box((a + z) * .5f, new Vector3(width, delta.magnitude, width), m, radius,
                    Quaternion.FromToRotation(Vector3.up, delta.normalized));
            }
            public void Ellipsoid(Vector3 p, Vector3 radii, M m, int seed)
            {
                string key = "crown_" + Mathf.Abs(seed % 19);
                Mesh mesh;
                if (!shapes.TryGetValue(key, out mesh) || !mesh) { mesh = CrownMesh(seed % 19); shapes[key] = mesh; }
                Add(m, mesh, p, Quaternion.identity, radii);
            }
            public void Torus(Vector3 p, float radius, float tube, M m, Quaternion q)
            {
                Add(m, TorusMesh(radius, tube), p, q, Vector3.one);
            }
            public void TrianglePrism(Vector3 p, float depth, float eave, float ridge, float thick, M m)
            {
                var v = new[] { new Vector3(-thick*.5f,eave,-depth*.5f), new Vector3(-thick*.5f,eave,depth*.5f), new Vector3(-thick*.5f,ridge,0),
                    new Vector3(thick*.5f,eave,-depth*.5f), new Vector3(thick*.5f,eave,depth*.5f), new Vector3(thick*.5f,ridge,0) };
                int[] t = { 0,1,2, 3,5,4, 0,3,4, 0,4,1, 1,4,5, 1,5,2, 2,5,3, 2,3,0 };
                Add(m, FinishMesh("GablePrism", v, t), p, Quaternion.identity, Vector3.one);
            }
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,M m)
            {
                Add(m,FinishMesh("ExteriorQuad",new[]{a,b,c,d,a,b,c,d},new[]{0,1,2,0,2,3,4,6,5,4,7,6}),Vector3.zero,Quaternion.identity,Vector3.one);
            }
            public void Attach(GameObject parent, string name)
            {
                foreach (var pair in batches)
                {
                    var mesh = new Mesh { name = "HomeV2_" + name + "_" + (M)pair.Key };
                    int count = 0; foreach (var item in pair.Value) count += item.mesh.vertexCount;
                    if (count > 65000) mesh.indexFormat = IndexFormat.UInt32;
                    mesh.CombineMeshes(pair.Value.ToArray(), true, true, false);
                    mesh.RecalculateBounds();
                    var g = Group(parent, name + "_" + (M)pair.Key);
                    g.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = palette[pair.Key];
                    if (pair.Key == (int)M.Glass) r.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
        }

        static Mesh BoxMesh(Vector3 size, float radius)
        {
            string key = "box_" + size.x.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "_" +
                size.y.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "_" + size.z.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "_" +
                radius.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            Mesh cached;
            if (shapes.TryGetValue(key, out cached) && cached) return cached;
            Vector3 h = size * .5f;
            float r = Mathf.Clamp(radius, .00001f, Mathf.Min(h.x, Mathf.Min(h.y, h.z)) * .7f);
            Vector3 inset = h - Vector3.one * r;
            var v = new List<Vector3>(); var t = new List<int>(); var uv = new List<Vector2>();
            Action<Vector3[], Vector3> face = (points, normal) =>
            {
                int start = v.Count;
                for (int i = 0; i < points.Length; i++) { v.Add(points[i]); uv.Add(new Vector2(points[i].x + points[i].z, points[i].y + points[i].z)); }
                bool forward = Vector3.Dot(Vector3.Cross(points[1] - points[0], points[2] - points[0]), normal) > 0;
                for (int i = 1; i < points.Length - 1; i++)
                { t.Add(start); t.Add(start + (forward ? i : i + 1)); t.Add(start + (forward ? i + 1 : i)); }
            };
            int[] sa = { -1, 1, 1, -1 }, sc = { -1, -1, 1, 1 };
            for (int axis = 0; axis < 3; axis++) for (int s = -1; s <= 1; s += 2)
            {
                int a = (axis + 1) % 3, c = (axis + 2) % 3;
                var points = new Vector3[4];
                for (int i = 0; i < 4; i++)
                { Vector3 p = Vector3.zero; p[axis] = h[axis] * s; p[a] = inset[a] * sa[i]; p[c] = inset[c] * sc[i]; points[i] = p; }
                Vector3 n = Vector3.zero; n[axis] = s; face(points, n);
            }
            for (int axis = 0; axis < 3; axis++) for (int aSign = -1; aSign <= 1; aSign += 2) for (int cSign = -1; cSign <= 1; cSign += 2)
            {
                int a = (axis + 1) % 3, c = (axis + 2) % 3;
                Vector3 p0 = Vector3.zero, p1 = Vector3.zero, p2 = Vector3.zero, p3 = Vector3.zero;
                p0[axis] = p3[axis] = -inset[axis]; p1[axis] = p2[axis] = inset[axis];
                p0[a] = p1[a] = h[a] * aSign; p0[c] = p1[c] = inset[c] * cSign;
                p2[a] = p3[a] = inset[a] * aSign; p2[c] = p3[c] = h[c] * cSign;
                Vector3 n = Vector3.zero; n[a] = aSign; n[c] = cSign; face(new[] { p0, p1, p2, p3 }, n);
            }
            for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
                face(new[] { new Vector3(h.x*x,inset.y*y,inset.z*z),new Vector3(inset.x*x,h.y*y,inset.z*z),new Vector3(inset.x*x,inset.y*y,h.z*z) }, new Vector3(x,y,z));
            var mesh = new Mesh { name = "HomeV2_Bevel" };
            mesh.SetVertices(v); mesh.SetTriangles(t, 0); mesh.SetUVs(0, uv);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds(); shapes[key] = mesh; return mesh;
        }
        static Mesh CylinderMesh(float bottom, float top, int sides)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2 / sides, b = (i + 1) * Mathf.PI * 2 / sides;
                int k = v.Count;
                v.Add(new Vector3(Mathf.Cos(a) * bottom, -.5f, Mathf.Sin(a) * bottom));
                v.Add(new Vector3(Mathf.Cos(b) * bottom, -.5f, Mathf.Sin(b) * bottom));
                v.Add(new Vector3(Mathf.Cos(b) * top, .5f, Mathf.Sin(b) * top));
                v.Add(new Vector3(Mathf.Cos(a) * top, .5f, Mathf.Sin(a) * top));
                t.AddRange(new[] { k,k+2,k+1,k,k+3,k+2 });
                k = v.Count; v.Add(new Vector3(0,.5f,0)); v.Add(new Vector3(Mathf.Cos(a)*top,.5f,Mathf.Sin(a)*top)); v.Add(new Vector3(Mathf.Cos(b)*top,.5f,Mathf.Sin(b)*top));
                t.AddRange(new[] { k,k+2,k+1 });
                k = v.Count; v.Add(new Vector3(0,-.5f,0)); v.Add(new Vector3(Mathf.Cos(a)*bottom,-.5f,Mathf.Sin(a)*bottom)); v.Add(new Vector3(Mathf.Cos(b)*bottom,-.5f,Mathf.Sin(b)*bottom));
                t.AddRange(new[] { k,k+1,k+2 });
            }
            return FinishMesh("HomeV2_Turned", v.ToArray(), t.ToArray());
        }
        static Mesh CrownMesh(int seed)
        {
            const int rings = 7, sides = 11;
            var v = new List<Vector3>(); var t = new List<int>();
            for (int r = 0; r <= rings; r++)
            {
                float latitude = -Mathf.PI * .5f + r * Mathf.PI / rings;
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2 / sides;
                    float perturb = 1 + Mathf.Sin(i * 4.13f + r * 1.79f + seed * .71f) * .085f;
                    v.Add(new Vector3(Mathf.Cos(a) * Mathf.Cos(latitude) * perturb, Mathf.Sin(latitude), Mathf.Sin(a) * Mathf.Cos(latitude) * perturb));
                }
            }
            for (int r = 0; r < rings; r++) for (int i = 0; i < sides; i++)
            {
                int a = r * sides + i, b = r * sides + (i + 1) % sides, c = a + sides, d = b + sides;
                t.AddRange(new[] { a,c,b,b,c,d });
            }
            return FinishMesh("HomeV2_Foliage", v.ToArray(), t.ToArray());
        }
        static Mesh TorusMesh(float radius, float tube)
        {
            const int segments = 24, sides = 7;
            var v = new List<Vector3>(); var t = new List<int>();
            for (int i = 0; i < segments; i++) for (int j = 0; j < sides; j++)
            {
                float a = i * Mathf.PI * 2 / segments, b = j * Mathf.PI * 2 / sides;
                v.Add(new Vector3(Mathf.Cos(a) * (radius + Mathf.Cos(b) * tube), Mathf.Sin(b) * tube, Mathf.Sin(a) * (radius + Mathf.Cos(b) * tube)));
            }
            for (int i = 0; i < segments; i++) for (int j = 0; j < sides; j++)
            {
                int a = i * sides + j, b = i * sides + (j + 1) % sides, c = (i + 1) % segments * sides + j, d = (i + 1) % segments * sides + (j + 1) % sides;
                t.AddRange(new[] { a,b,c,b,d,c });
            }
            return FinishMesh("HomeV2_HoseCoil", v.ToArray(), t.ToArray());
        }
        static Mesh FinishMesh(string name, Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh { name = name };
            mesh.vertices = vertices; mesh.triangles = triangles;
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < uv.Length; i++) uv[i] = new Vector2(vertices[i].x + vertices[i].z, vertices[i].y);
            mesh.uv = uv; mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds(); return mesh;
        }
    }
}
