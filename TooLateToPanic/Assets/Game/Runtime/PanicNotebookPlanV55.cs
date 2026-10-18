using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public enum PanicNotebookPlanKindV55 { Floor, Wall, Stair, Door, Furniture, Window }
    public readonly struct PanicNotebookPlanShapeV55
    {
        public readonly string Id, Label;
        public readonly PanicNotebookPlanKindV55 Kind;
        public readonly IReadOnlyList<Vector3> Corners;
        public readonly Vector3 Pivot,ClosedEnd,CurrentEnd;
        public readonly bool Open;
        public PanicNotebookPlanShapeV55(string id,string label,PanicNotebookPlanKindV55 kind,Vector3[] corners,Vector3 pivot,bool open=false,Vector3 closedEnd=default(Vector3),Vector3 currentEnd=default(Vector3))
        { Id=id;Label=label;Kind=kind;Corners=Array.AsReadOnly(corners);Pivot=pivot;Open=open;ClosedEnd=closedEnd;CurrentEnd=currentEnd; }
    }
    public sealed class PanicNotebookPlanSnapshotV55
    {
        public readonly int Floor,WallCount,DoorCount,FurnitureCount,StairCount;
        public readonly Bounds WorldBounds;
        public readonly Vector3 ActorPosition,ActorForward;
        public readonly IReadOnlyList<PanicNotebookPlanShapeV55> Shapes;
        public PanicNotebookPlanSnapshotV55(int floor,Bounds bounds,Vector3 actor,Vector3 forward,List<PanicNotebookPlanShapeV55> shapes)
        {
            Floor=floor;WorldBounds=bounds;ActorPosition=actor;ActorForward=forward;Shapes=shapes.AsReadOnly();
            foreach(var s in shapes){if(s.Kind==PanicNotebookPlanKindV55.Wall)WallCount++;if(s.Kind==PanicNotebookPlanKindV55.Door)DoorCount++;if(s.Kind==PanicNotebookPlanKindV55.Furniture)FurnitureCount++;if(s.Kind==PanicNotebookPlanKindV55.Stair)StairCount++;}
        }
    }
    public sealed partial class PanicHomeWorld
    {
        /// <summary>Read-only projection of actual authored collision pieces and live object transforms.
        /// Never enables hidden renderers/colliders, creates cameras, or changes any gameplay state.</summary>
        public PanicNotebookPlanSnapshotV55 CaptureNotebookPlanV55(int selectedFloor)
        {
            int f=Mathf.Clamp(selectedFloor,-1,1);float y=f*3.2f;
            var shapes=new List<PanicNotebookPlanShapeV55>();var seen=new HashSet<int>();
            void Architecture(GameObject root)
            {
                if(!root)return;
                foreach(var box in root.GetComponentsInChildren<BoxCollider>(true))
                {
                    if(!box.name.StartsWith("Collision_solid_",StringComparison.Ordinal))continue;
                    var bounds=NotebookBoundsV55(box.transform,new Bounds(box.center,box.size));
                    string n=box.name.ToLowerInvariant();
                    bool deck=(n.Contains("floor_")||n.Contains("_slab")||n.Contains("balcony_deck"))&&bounds.size.y<.5f&&bounds.max.y<y+.23f;
                    bool wall=!deck&&!n.Contains("roof")&&bounds.min.y<=y+.56f&&bounds.max.y>=y+.54f;
                    bool rail=n.Contains("balcony_")&&!n.Contains("deck")&&bounds.min.y<y+1.15f&&bounds.max.y>y+.3f;
                    if(!deck&&!wall&&!rail)continue;
                    shapes.Add(new PanicNotebookPlanShapeV55(n,root==residenceGarage?"Garaj":root==residenceBalcony?"Balkon":"",deck?PanicNotebookPlanKindV55.Floor:PanicNotebookPlanKindV55.Wall,NotebookCornersV55(box.transform,new Bounds(box.center,box.size)),box.transform.TransformPoint(box.center)));
                    seen.Add(box.GetInstanceID());
                }
            }
            if(f+1<floorVisuals.Count)Architecture(floorVisuals[f+1]);
            if(f==0)Architecture(residenceGarage);
            if(f==1){Architecture(residenceBathroom);Architecture(residenceBalcony);}
            bool FurnitureOnSelectedFloor(Bounds bounds)
            {
                // Entity roots may sit at y0 while child art/collision belongs upstairs.
                // Use the physical footprint's actual height and selected real deck union.
                if(bounds.min.y<y-.35f||bounds.min.y>y+1.8f||bounds.max.y<y+.05f)return false;
                foreach(var deck in shapes)if(deck.Kind==PanicNotebookPlanKindV55.Floor)
                {
                    float minX=float.PositiveInfinity,maxX=float.NegativeInfinity,minZ=float.PositiveInfinity,maxZ=float.NegativeInfinity;
                    foreach(var p in deck.Corners){minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);minZ=Mathf.Min(minZ,p.z);maxZ=Mathf.Max(maxZ,p.z);}
                    if(bounds.center.x>=minX-.15f&&bounds.center.x<=maxX+.15f&&bounds.center.z>=minZ-.15f&&bounds.center.z<=maxZ+.15f)return true;
                }
                return false;
            }
            void Fixture(GameObject entity)
            {
                if(!entity)return;bool colliderFound=false;
                foreach(var box in entity.GetComponentsInChildren<BoxCollider>(true))
                {
                    if(!seen.Add(box.GetInstanceID()))continue;colliderFound=true;
                    if(!FurnitureOnSelectedFloor(NotebookBoundsV55(box.transform,new Bounds(box.center,box.size))))continue;
                    shapes.Add(new PanicNotebookPlanShapeV55(entity.name,entity.name,PanicNotebookPlanKindV55.Furniture,NotebookCornersV55(box.transform,new Bounds(box.center,box.size)),entity.transform.position));
                }
                if(colliderFound)return;
                Bounds local=new Bounds();bool anyMesh=false;
                foreach(var mf in entity.GetComponentsInChildren<MeshFilter>(true))
                {
                    if(!mf.sharedMesh)continue;var mb=mf.sharedMesh.bounds;
                    for(int mask=0;mask<8;mask++)
                    {
                        var point=entity.transform.InverseTransformPoint(mf.transform.TransformPoint(mb.center+Vector3.Scale(mb.extents,new Vector3((mask&1)==0?-1:1,(mask&2)==0?-1:1,(mask&4)==0?-1:1))));
                        if(!anyMesh){local=new Bounds(point,Vector3.zero);anyMesh=true;}else local.Encapsulate(point);
                    }
                }
                if(anyMesh&&FurnitureOnSelectedFloor(NotebookBoundsV55(entity.transform,local)))shapes.Add(new PanicNotebookPlanShapeV55(entity.name,entity.name,PanicNotebookPlanKindV55.Furniture,NotebookCornersV55(entity.transform,local),entity.transform.position));
            }
            if(f==1)
            {
                foreach(var root in new[]{residenceBathroom,residenceBalcony})if(root)foreach(Transform child in root.transform)
                    if(child.name.StartsWith("HomeV3_",StringComparison.Ordinal)||child.name.StartsWith("HomeV4_",StringComparison.Ordinal))Fixture(child.gameObject);
            }
            // Use real step colliders, including the arrival flight when upstairs.
            int stairLevel=f==1?0:f;
            foreach(var entity in floorItems)
            {
                if(!entity)continue;var parent=entity.transform.parent;
                if(parent&&parent.name.StartsWith("Stair flight ",StringComparison.Ordinal))
                {
                    if(Mathf.Abs(parent.position.y-stairLevel*3.2f)>.1f)continue;
                    foreach(var box in parent.GetComponentsInChildren<BoxCollider>(true))
                        if(box.name.StartsWith("Step ",StringComparison.Ordinal)&&seen.Add(box.GetInstanceID()))
                            shapes.Add(new PanicNotebookPlanShapeV55(box.name,"Merdiven",PanicNotebookPlanKindV55.Stair,NotebookCornersV55(box.transform,new Bounds(box.center,box.size)),box.transform.position));
                    continue;
                }
                if(entity==residenceBathroom||entity==residenceGarage||entity==residenceBalcony||Mathf.Abs(entity.transform.position.y-y)>.85f)continue;
                var pick=entity.GetComponent<HomePick>();
                var modelItem=pick&&Model!=null?Model.Items.Find(i=>i.Id==pick.Id):null;
                if(modelItem!=null&&(!modelItem.Installed||modelItem.Kind==ItemKind.Door||modelItem.Kind==ItemKind.Window||modelItem.Kind==ItemKind.Wall||modelItem.Kind==ItemKind.Roof))continue;
                if(pick&&hinges.TryGetValue(pick.Id??"",out var hinge)&&hinge.directionalDoor)continue;
                foreach(var box in entity.GetComponentsInChildren<BoxCollider>(true))
                {
                    if(!seen.Add(box.GetInstanceID()))continue;
                    var b=NotebookBoundsV55(box.transform,new Bounds(box.center,box.size));
                    if(b.size.x<.12f||b.size.z<.12f||!FurnitureOnSelectedFloor(b))continue;
                    shapes.Add(new PanicNotebookPlanShapeV55(pick?pick.Id:entity.name,entity.name,PanicNotebookPlanKindV55.Furniture,NotebookCornersV55(box.transform,new Bounds(box.center,box.size)),entity.transform.position));
                }
            }
            foreach(var pair in hinges)
            {
                var h=pair.Value;if(!h.directionalDoor||!h.leaf||!h.collision||Mathf.Abs(h.frame.position.y-y)>.5f)continue;
                var box=h.collision as BoxCollider;if(!box)continue;
                var item=Model==null?null:Model.Items.Find(i=>i.Id==pair.Key);
                var bb=new Bounds(box.center,box.size);
                var localEnd=new Vector3(Mathf.Abs(bb.min.x)>Mathf.Abs(bb.max.x)?bb.min.x:bb.max.x,bb.center.y,bb.center.z);
                var closedEnd=h.frame.TransformPoint(h.closedPosition+h.closed*Vector3.Scale(localEnd,h.leaf.localScale));
                shapes.Add(new PanicNotebookPlanShapeV55(pair.Key,item==null?pair.Key:item.Label,PanicNotebookPlanKindV55.Door,NotebookCornersV55(box.transform,bb),h.leaf.position,h.open,closedEnd,box.transform.TransformPoint(localEnd)));
            }
            if(Model!=null)foreach(var item in Model.Items)
                if(item.Installed&&item.Kind==ItemKind.Window&&Mathf.Abs(item.Position.y-y)<.5f&&items.TryGetValue(item.Id,out var go)&&go)
                {
                    var actual=go.GetComponent<BoxCollider>();if(!actual)continue;
                    shapes.Add(new PanicNotebookPlanShapeV55(item.Id,item.Label,PanicNotebookPlanKindV55.Window,NotebookCornersV55(actual.transform,new Bounds(actual.center,actual.size)),item.Position));
                }
            Bounds area=new Bounds();bool any=false;
            foreach(var shape in shapes)if(shape.Kind==PanicNotebookPlanKindV55.Floor||shape.Kind==PanicNotebookPlanKindV55.Wall||shape.Kind==PanicNotebookPlanKindV55.Door||shape.Kind==PanicNotebookPlanKindV55.Window)
                foreach(var p in shape.Corners){if(!any){area=new Bounds(p,Vector3.zero);any=true;}else area.Encapsulate(p);}
            // Include actual same-floor sources/player in the uniform fit; no fake world extents.
            if(Expedition!=null)foreach(var s in Expedition.Sources)if(s!=null&&s.Floor==f){if(!any){area=new Bounds(s.Position,Vector3.zero);any=true;}else area.Encapsulate(s.Position);}
            if(Expedition!=null&&BagFinalStageActive)foreach(var s in Expedition.WorldSupplies)if(s!=null&&s.Floor==f&&s.Count>0)area.Encapsulate(s.Position);
            if(player&&Mathf.Abs(player.position.y-y)<1.1f)area.Encapsulate(player.position);
            return new PanicNotebookPlanSnapshotV55(f,area,PlayerPosition,player?player.forward:Vector3.forward,shapes);
        }
        static Vector3[] NotebookCornersV55(Transform t,Bounds b)
        {
            return new[]{t.TransformPoint(new Vector3(b.min.x,b.center.y,b.min.z)),t.TransformPoint(new Vector3(b.max.x,b.center.y,b.min.z)),t.TransformPoint(new Vector3(b.max.x,b.center.y,b.max.z)),t.TransformPoint(new Vector3(b.min.x,b.center.y,b.max.z))};
        }
        static Bounds NotebookBoundsV55(Transform t,Bounds b)
        {
            Bounds result=new Bounds(t.TransformPoint(b.center),Vector3.zero);
            for(int mask=0;mask<8;mask++)result.Encapsulate(t.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((mask&1)==0?-1:1,(mask&2)==0?-1:1,(mask&4)==0?-1:1))));
            return result;
        }
    }
}
