using System;
using UnityEngine;

namespace TooLateToPanic
{
    // Coordinates shared by authored floor meshes, retained Model objects and real sources.
    // No controller, clock, price, damage, inventory or scoring policy lives here.
    public static class PanicHouseLayoutV414
    {
        public const float StairSouth = -1f, StairNorth = 3.8f;
        public const float UpperWestPartition = -2.65f, UpperEastPartition = 2.65f;
        public static readonly Vector3 BagPosition = new Vector3(1.55f, .035f, -2.3f);
        public static Vector3 StairOrigin(int level) { return new Vector3(0, level * 3.2f, StairSouth); }
        public static Bounds ShaftBounds(int level) { return new Bounds(new Vector3(0,level*3.2f,(StairSouth+StairNorth)*.5f),new Vector3(1.8f,.24f,StairNorth-StairSouth)); }
        public readonly struct DoorSeat
        {
            public readonly string Id,Label;public readonly Vector3 Position;public readonly float Yaw;
            public DoorSeat(string id,string label,float x,float y,float z,float yaw=0) {Id=id;Label=label;Position=new Vector3(x,y,z);Yaw=yaw;}
        }
        static readonly DoorSeat[] doors={
            new DoorSeat("door_inner_ground","Mutfak",3.35f,0,.8f),
            new DoorSeat("door_wc","Tuvalet",3.35f,0,-2.8f),
            new DoorSeat("door_pantry","Kiler",6.4f,0,-2.8f),
            new DoorSeat("door_parent","Ebeveyn odası",UpperWestPartition,3.2f,3.9f,90),
            new DoorSeat("door_child","Çocuk odası",UpperWestPartition,3.2f,-3.2f,90),
            new DoorSeat("door_study","Çalışma odası",UpperEastPartition,3.2f,-2.6f,90),
            new DoorSeat("bathroom_door","Banyo",UpperEastPartition,3.2f,4.6f,90),
            new DoorSeat("door_balcony","Balkon",0,3.2f,-6),
            new DoorSeat("door_garage_inner","Garaj iç kapısı",8.1f,0,2.2f,90)};
        public static System.Collections.Generic.IReadOnlyList<DoorSeat> Doors {get;}=Array.AsReadOnly(doors);
        public static bool IsPartition(string name) { return name != null && name.StartsWith("solid_v414_partition", StringComparison.Ordinal); }
        public static bool OverlapsStair(Vector3 p, Vector3 size)
        { return p.x-size.x*.5f < 1.12f && p.x+size.x*.5f > -1.12f && p.z-size.z*.5f < StairNorth+.12f && p.z+size.z*.5f > StairSouth-.12f; }

        public static void ApplyModelLayout(PanicModel model)
        {
            Move(model,"door_inner_ground",new Vector3(3.35f,0,.8f),0,"Mutfak");
            Move(model,"bagstation",BagPosition,0,"Afet çantası");
            Move(model,"sofa",new Vector3(-4.35f,0,-.3f),0,"Salon kanepesi");
            Move(model,"table",new Vector3(-4.05f,0,-2.55f),0,"Salon masası");
            Move(model,"chair",new Vector3(-2.55f,0,-2.6f),90,"Sandalye");
            Move(model,"tvstand",new Vector3(-6.3f,0,-4.9f),180,"Televizyon");
            Move(model,"bookshelf",new Vector3(-6.2f,0,4.9f),0,"Salon kitaplığı");
            Move(model,"plant",new Vector3(-6.7f,0,1.7f),0,"Salon bitkisi");
            Move(model,"fridge",new Vector3(6.75f,0,4.95f),0,"Buzdolabı");
            Move(model,"cabinet",new Vector3(4.65f,0,4.95f),0,"Mutfak dolabı");
            Move(model,"bed",new Vector3(-5.4f,3.2f,3.85f),0,"Ebeveyn yatağı");
            Move(model,"up_cabinet",new Vector3(-7.45f,3.2f,2.2f),270,"Ebeveyn dolabı");
            Move(model,"desk_upper",new Vector3(5.25f,3.2f,-4.9f),0,"Çalışma masası");
            Move(model,"up_bookshelf",new Vector3(7.35f,3.2f,-.25f),270,"Çalışma kitaplığı");
            // Reuse eight existing risk IDs. Every visible window now has exactly one Model target.
            Move(model,"window_west_north",new Vector3(-4,0,-5.82f),180,"Salon güney penceresi");
            Move(model,"window_west_upper_north",new Vector3(4,3.2f,-5.82f),180,"Çalışma odası güney penceresi");
            foreach(var door in doors)Move(model,door.Id,door.Position,door.Yaw,door.Label);
        }
        static void Move(PanicModel model,string id,Vector3 p,float yaw,string label)
        { var item=model.Items.Find(i=>i.Id==id);if(item==null)return;item.Position=p;item.Rotation=yaw;item.Label=label; }

        // Call after the expedition creates/resets its source list and before visual construction.
        // Match the actual furniture instead of maintaining a second set of coordinates.
        public static void ConfigureSources(PanicExpedition expedition,PanicModel model)
        {
            foreach(var source in expedition.Sources)
            { var item=model.Items.Find(i=>i.Id==source.Id);if(item!=null){source.Position=item.Position;source.Floor=Mathf.RoundToInt(item.Position.y/3.2f);} }
        }
    }
}
