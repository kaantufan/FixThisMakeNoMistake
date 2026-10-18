using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed class ResidenceLootPick : MonoBehaviour { public int Index; }
    public sealed partial class PanicHomeWorld
    {
        static readonly string[] ResidenceIds={"fireplace","computer","desk","radio","tablelamp","floorlamp","toilet","sink","bathtub","towelrack","washingmachine","waterheater","storagerack","car","toolwall","bicycle","books","houseplant","shoe_rack","coat_rack","curtains","wallart","garagedoor","bunkerhatch","rug","armchair","sofa","sofa_indigo","chair","bed","sidetable","laundrybasket","bookshelf","table","tvstand","plant","locker","toolbox","cabinet","kitchencabinet","medicinecabinet","supplycrate","bagstation"};
        static readonly Vector3 BunkerInteriorPosition=new Vector3(-65,0,60);
        readonly Dictionary<string,GameObject> residenceTargets=new Dictionary<string,GameObject>();
        readonly List<Collider> residenceObstacles=new List<Collider>();
        GameObject residenceGarage,residenceBathroom,residenceBalcony,residenceBunkerRoom,lootTray,helicopter,aftermath,hearthFlame;
        Transform bunkerLeaf,garageLeaf; Vector3 garageLeafClosed; float residenceTime,radioClock,noticeAge,rescueAge,jumpCooldown;
        Quaternion bunkerClosed;
        AudioSource radioVoice,radioNoise,rotorAudio; Light helicopterSearch,hearthLight;
        bool hearthEnabled=true,computerEnabled=true,garageOpen,microwaveOpenV5,minuteVoice;
        string lastNotice,queuedVoice,radioClipId;
        int radioSequence;
        public LootSource OpenSource { get { var s=NearestSource; return s!=null&&s.Open&&CanInteract&&!InventoryOpen&&!WorkActive&&!IsMoving ? s:null; } }
        public int LootIndex {get;private set;}
        public string SelectedLootId {get {var s=OpenSource;return s!=null&&LootIndex>=0&&LootIndex<s.Contents.Count?s.Contents[LootIndex].Id:"";}}
        public bool LootVisible {get {return false;}}
        public float GatherFeedback {get;private set;}
        public int GatherCount {get;private set;}
        public string GatherItemId {get;private set;}
        public string ShortAction {get;private set;}="Aç";
        public bool IsGrounded {get {return controller&&controller.isGrounded;}}
        public string RadioHeadline {get;private set;}="";
        public static bool DisasterAudioOnly {get {return false;}}
        public bool HasNarrationPlayback {get {return PresentationSpeechActive||radioVoice&&radioVoice.isPlaying;}}
        public bool RadioEnabled {get;private set;}=false;
        public bool RadioSpeaking {get {return radioVoice&&radioVoice.isPlaying;}}
        public bool NotificationVisible {get {return noticeAge>0;}}
        public string ShortNotice {get;private set;}
        public int CancelFrame {get;private set;}=-1;

        void ConfigureResidenceModel()
        {
            // V4.14 room graph uses one canonical set of door/item coordinates.
            Model.Items.RemoveAll(i=>i.Id=="door_inner_upper"||i.Id=="window_north_upper_east");
            var loungeSofa=Model.Items.Find(i=>i.Id=="sofa"); if(loungeSofa!=null)loungeSofa.ArtId="sofa_indigo";
            var bunker=Model.Items.Find(i=>i.Id=="bunker");if(bunker!=null){bunker.ArtId="bunkerhatch";bunker.Position=BunkerPosition;}
            var bag=Model.Items.Find(i=>i.Id=="bagstation");if(bag!=null){bag.Label="Afet çantası";bag.Position=PanicHouseLayoutV414.BagPosition;bag.Size=new Vector3(.48f,.62f,.32f);}
            var desk=Model.Items.Find(i=>i.Id=="desk_upper");if(desk!=null){desk.Size=new Vector3(1.75f,.805f,.8f);}
            foreach(var door in PanicHouseLayoutV414.Doors)
                AddResidenceItem(door.Id,door.Label,"door",ItemKind.Door,door.Position,new Vector3(1.2f,2.25f,.2f),door.Yaw);
            AddResidenceItem("bed_child","Çocuk yatağı","bed",ItemKind.Furniture,new Vector3(-5.5f,3.2f,-3.85f),new Vector3(1.64f,1.42f,2.16f));
            var child=Model.Items.Find(i=>i.Id=="bed_child");if(child!=null)child.Movable=true;
            PanicHouseLayoutV414.ApplyModelLayout(Model);
            // The garage rolling door is presentation-driven, not a swinging house leaf.
            for(int side=0;side<4;side++) { string label=new[]{"Kuzey","Batı","Güney","Doğu"}[side]; string id=new[]{"north","west","south","east"}[side]; Vector3 p=side==0?new Vector3(0,-3.2f,5.91f):side==1?new Vector3(-7.91f,-3.2f,0):side==2?new Vector3(0,-3.2f,-5.91f):new Vector3(7.91f,-3.2f,0); AddResidenceItem("wall_"+id+"_basement","Bodrum "+label,"wall",ItemKind.Wall,p,new Vector3(side==1||side==3?12:16,3.2f,.2f),side==1?270:side==3?90:0); }
        }
        void AddResidenceItem(string id,string label,string art,ItemKind kind,Vector3 p,Vector3 size,float rotation=0)
        {if(!Model.Items.Exists(i=>i.Id==id))Model.Items.Add(new PanicItem{Id=id,Label=label,ArtId=art,Kind=kind,Position=p,Size=size,Rotation=rotation,Installed=true});}
        BoxCollider AddTightCollision(MeshFilter mf,Transform owner)
        {
            var c=new GameObject("Collision_"+mf.name);c.transform.SetParent(owner,true);c.transform.position=mf.transform.TransformPoint(mf.sharedMesh.bounds.center);c.transform.rotation=mf.transform.rotation;
            var box=c.AddComponent<BoxCollider>();box.size=Vector3.Scale(mf.sharedMesh.bounds.size,mf.transform.lossyScale);
            if(IsFullHeightPartition(mf.name)||PanicHouseLayoutV414.IsPartition(mf.name)) { var size=box.size;size.y=3.2f;box.size=size;var p=c.transform.position;p.y=owner.position.y+1.6f;c.transform.position=p; }
            return box;
        }
        void BuildStairFlight(int level)
        {
            var root=new GameObject("Stair flight "+level);root.transform.position=PanicHouseLayoutV414.StairOrigin(level);var art=CreateVisual("stairs");art.transform.SetParent(root.transform,false);
            // Only the art follows the floor cutaway. Steps and rails must remain physical
            // while ActiveFloor changes, including when the flight above is hidden.
            floorItems.Add(art);
            for(int i=0;i<16;i++)
            {
                float top=(i+1)*.2f,z=i*.3f+.15f;
                var step=new GameObject("Step "+i);step.transform.SetParent(root.transform,false);step.transform.localPosition=new Vector3(0,top-.09f,z);step.AddComponent<BoxCollider>().size=new Vector3(1.5f,.18f,.3f);
                // Tight, overlapping panels follow every tread. A whole-flight AABB would
                // block the stair below; these leave the original 1.5m walking width clear.
                for(int side=-1;side<=1;side+=2)
                {
                    var rail=new GameObject("Stair rail "+(side<0?"west ":"east ")+i);rail.transform.SetParent(root.transform,false);rail.transform.localPosition=new Vector3(side*.8f,top+.44f,z);rail.AddComponent<BoxCollider>().size=new Vector3(.1f,.96f,.304f);
                }
            }
            // Each arrival floor has an open shaft alongside the stair. These guards
            // stay outside both the travel width and the unobstructed north landing.
            for(int side=-1;side<=1;side+=2)
            {
                var guard=new GameObject("Stair opening rail "+(side<0?"west":"east"));guard.transform.SetParent(root.transform,false);guard.transform.localPosition=new Vector3(side*1.01f,3.685f,2.4f);guard.AddComponent<BoxCollider>().size=new Vector3(.08f,.97f,4.8f);
            }
            // The upper floor has no continuing flight, so its south shaft edge closes.
            // Ground south stays open for the flight to upstairs; lower entry headroom is 3m.
            if(level==0)
            {
                var guard=new GameObject("Stair opening rail south");guard.transform.SetParent(root.transform,false);guard.transform.localPosition=new Vector3(0,3.685f,-.05f);guard.AddComponent<BoxCollider>().size=new Vector3(2.12f,.97f,.1f);
            }
        }
        GameObject Room(string id,Func<GameObject> create,Vector3 p)
        {
            var prefab=Resources.Load<GameObject>("ResidenceArt/"+id);var room=prefab?Instantiate(prefab):create();room.name="HomeV4_"+char.ToUpperInvariant(id[0])+id.Substring(1);room.transform.position=p;
            foreach(var mf in room.GetComponentsInChildren<MeshFilter>(true))if(mf.name.StartsWith("solid_",StringComparison.OrdinalIgnoreCase))AddTightCollision(mf,room.transform);
            return room;
        }
        void BuildResidenceRooms()
        {
            residenceGarage=Room("garage",PanicResidenceArt.BuildGarage,Vector3.zero);
            residenceBathroom=Room("bathroom",PanicResidenceArt.BuildBathroom,Vector3.up*3.2f);floorItems.Add(residenceBathroom);
            foreach(Transform fixture in residenceBathroom.transform)
            {
                if(!fixture.name.StartsWith("HomeV3_",StringComparison.Ordinal)&&!fixture.name.StartsWith("HomeV4_",StringComparison.Ordinal))continue;
                AddBoundsCollider(fixture.gameObject);residenceObstacles.AddRange(fixture.GetComponents<Collider>());
                string key=fixture.name.Contains("toilet")?"@toilet":fixture.name.Contains("sink")?"@sink":null;
                if(key!=null){residenceTargets[key]=fixture.gameObject;var pick=fixture.gameObject.AddComponent<HomePick>();pick.Id=key;BindHinge(fixture.gameObject,key,false);}
            }
            residenceBalcony=Room("balcony",PanicResidenceArt.BuildBalcony,Vector3.zero);
            AddBox("Balcony walk surface",new Vector3(-2,3.13f,-7.5f),new Vector3(7,.14f,3));
            residenceBunkerRoom=Room("bunker_interior",PanicResidenceArt.BuildBunkerInterior,BunkerInteriorPosition);
            var lit=new GameObject("Bunker emergency lamp").AddComponent<Light>();lit.transform.position=BunkerInteriorPosition+new Vector3(0,2.2f,0);lit.type=LightType.Point;lit.color=new Color(1,.65f,.3f);lit.range=8;lit.intensity=2;
            var kitchenDetails=Room("kitchen_v414",PanicHomeArt.BuildKitchenDetailsV414,Vector3.zero);floorItems.Add(kitchenDetails);
            AddDecor("@microwave","v5_microwave",new Vector3(3.48f,.975f,4.82f),0,false);
            var childDetails=Room("child_v414",PanicHomeArt.BuildChildDetailsV414,Vector3.up*3.2f);floorItems.Add(childDetails);
            AddDecor("decor_wc_toilet","toilet",new Vector3(2.9f,.035f,-5.52f),180);
            AddDecor("decor_wc_sink","sink",new Vector3(4.05f,.035f,-5.52f),180);
            AddDecor("decor_pantry_back","storagerack",new Vector3(6.3f,0,-5.4f),180);
            AddDecor("decor_pantry_side","storagerack",new Vector3(7.42f,0,-4.15f),90);
            AddDecor("decor_lounge_armchair","armchair",new Vector3(-6.45f,0,-1.75f),45);
            AddDecor("decor_lounge_side","sidetable",new Vector3(-6.5f,0,2.8f));
            AddDecor("decor_dining_table","table",new Vector3(5.0f,0,2.6f));
            AddDecor("decor_dining_chair_a","chair",new Vector3(5.0f,0,1.55f),180);
            AddDecor("decor_dining_chair_b","chair",new Vector3(6.15f,0,2.6f),90);
            AddDecor("decor_child_books","books",new Vector3(-2.94f,4.21f,.26f),90,false);
            AddDecor("decor_child_cabinet","cabinet",new Vector3(-3.15f,3.2f,-.3f),90);
            AddDecor("decor_parent_sidetable","sidetable",new Vector3(-6.9f,3.2f,4.05f));
            AddDecor("decor_child_sidetable","sidetable",new Vector3(-6.9f,3.2f,-3.8f));
            AddDecor("decor_basement_spare_crates","supplycrate",new Vector3(-6.7f,-3.2f,-1.6f),270);
            AddDecor("decor_basement_seat","chair",new Vector3(5.8f,-3.2f,-3.65f),90);
            AddDecor("@fireplace","fireplace",new Vector3(-7.5f,0,.3f),-90);
            AddDecor("decor_radio_table","sidetable",new Vector3(-3.0f,0,4.95f));
            AddDecor("@radio","radio",new Vector3(-3.0f,.61f,4.95f),0,false);
            AddDecor("@garage","garagedoor",new Vector3(11.7f,0,0));
            garageLeaf=residenceTargets["@garage"].GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="DoorLeaf"||t.name=="GarageDoor");if(garageLeaf)garageLeafClosed=garageLeaf.localPosition;
            AddDecor("decor_car","car",new Vector3(11.8f,0,3.5f));
            AddDecor("decor_toolwall","toolwall",new Vector3(14.9f,0,4.9f),90);
            AddDecor("decor_bicycle","bicycle",new Vector3(8.8f,0,4.4f),90);
            AddDecor("decor_garage_shelf","storagerack",new Vector3(14.6f,0,2.2f),90);
            AddDecor("@laundry","washingmachine",new Vector3(6.7f,-3.2f,4.6f));
            BindHinge(residenceTargets["@laundry"],"@laundry",false);
            AddDecor("decor_basement_heater","waterheater",new Vector3(6.7f,-3.2f,2.4f));
            AddDecor("decor_basement_shelf","storagerack",new Vector3(-6.6f,-3.2f,-3.7f),180);
            AddDecor("decor_basement_bench","desk",new Vector3(4,-3.2f,-3.8f));
            // Keep a full turning/egress strip behind the front door, on both sides.
            AddDecor("decor_entry_shoes","shoe_rack",new Vector3(1.25f,0,-4.4f));
            AddDecor("decor_coats","coat_rack",new Vector3(-1.8f,0,-4.5f));
            AddPhysicalBagStation();
            // The emergency bag is carried invisibly; there is no floor guide lamp.
            AddDecor("decor_bath_firstaid_table","sidetable",new Vector3(3.1f,3.2f,3.5f));
            // Loose floor lamps had no model/work contract; keep those walk areas clear.
            AddDecor("decor_lounge_rug","rug",new Vector3(-4.35f,.025f,-1.65f),0,false);
            AddDecor("decor_bed_rug","rug",new Vector3(-5.4f,3.225f,2.25f),0,false);floorItems[floorItems.Count-1].transform.localScale=new Vector3(.68f,1,.5f);

            AddDecor("decor_bath_plant","houseplant",new Vector3(7,3.2f,2.3f));
            foreach(var id in new[]{"table","desk_upper"})if(items.TryGetValue(id,out var table))
            {
                string art=id=="desk_upper"?"computer":"books";var go=CreateVisual(art);go.transform.SetParent(table.transform,false);go.transform.localPosition=new Vector3(0,id=="desk_upper"?.805f:1.05f,0);
                if(id=="desk_upper"){residenceTargets["@computer"]=go;var pick=go.AddComponent<HomePick>();pick.Id="@computer";AddBoundsCollider(go);}
                if(id=="table"){var lamp=CreateVisual("tablelamp");lamp.transform.SetParent(table.transform,false);lamp.transform.localPosition=new Vector3(.54f,1.05f,.1f);}
            }

            AddDecor("decor_wallart","wallart",new Vector3(-2.7f,1.5f,5.78f),0,false);
            if(items.TryGetValue("bunker",out var bunker))
            {
                PanicCuratedAssetArt.TryReplaceBunkerLid(bunker);
                foreach (var r in bunker.GetComponentsInChildren<Renderer>(true))
                    if (!r.enabled && !r.GetComponent<PanicPermanentlyHiddenV414>()) r.gameObject.AddComponent<PanicPermanentlyHiddenV414>();
                visibilityRenderers.Remove(bunker.GetInstanceID());
                bunkerLeaf=bunker.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="DoorLeaf");
                if(bunkerLeaf)bunkerClosed=bunkerLeaf.localRotation;
            }
        }
        void AddBagStationGuideV414()
        {
            var root=new GameObject("Bag station warm light");root.transform.position=PanicHouseLayoutV414.BagPosition;floorItems.Add(root);
            var lamp=new GameObject("Bag amber pin light").AddComponent<Light>();lamp.transform.SetParent(root.transform,false);lamp.transform.localPosition=new Vector3(0,.9f,0);lamp.type=LightType.Point;lamp.color=new Color(1,.65f,.24f);lamp.range=2.0f;lamp.intensity=.7f;
        }
        Vector3 ResidenceFocusPosition(GameObject target){var p=target.transform.position;p.y=Mathf.Round(p.y/3.2f)*3.2f;return p;}
        void AddDecor(string id,string art,Vector3 p,float rotation=0,bool collide=true)
        {
            // V5 storage props are real finite sources. BuildObjects already created their art/collision.
            if(sourceObjects.TryGetValue(id,out var sourceVisual)&&sourceVisual)
            {sourceVisual.transform.SetPositionAndRotation(p,Quaternion.Euler(0,rotation,0));var source=Expedition.Sources.Find(s=>s.Id==id);if(source!=null){source.Position=p;source.Rotation=rotation;}return;}
            var go=CreateVisual(art);go.name=id;go.transform.position=p;go.transform.rotation=Quaternion.Euler(0,rotation,0);floorItems.Add(go);
            if(collide){if(id=="@garage"){var door=go.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="GarageDoor");if(door)AddBoundsCollider(door.gameObject);}else AddBoundsCollider(go);residenceObstacles.AddRange(go.GetComponents<Collider>());}
            if(id.StartsWith("@")){residenceTargets[id]=go;var pick=go.AddComponent<HomePick>();pick.Id=id;pick.Kind="decor";}
        }
        bool InResidence(Vector3 p){return Mathf.Abs(p.x)<8.05f&&Mathf.Abs(p.z)<6.05f||p.y>2&&p.x>-5.6f&&p.x<1.6f&&p.z<-5.9f&&p.z>-9.1f||p.x>8.1f&&p.x<15.3f&&p.z>-.1f&&p.z<6.3f;}
        void UpdateResidenceVisibility(bool cutawayInside)
        {
            if(residenceBalcony)SetResidenceRenderersVisible(residenceBalcony,!cutawayInside||CinematicViewFloorV5>=1);
            if(residenceGarage)
            {
                bool shown=!cutawayInside||CinematicViewFloorV5==0;
                SetResidenceRenderersVisible(residenceGarage,shown);
                if(shown && cutawayInside) ApplyFacadeCutaway(residenceGarage,CinematicViewFocusV5,8.1f,15.3f,0,6.3f,0);
                if(cutawayInside) foreach(var r in residenceGarage.GetComponentsInChildren<Renderer>(true)) if(HasParentNamed(r.transform,"Roof",residenceGarage.transform))r.enabled=false;
            }
        }
        void InitResidenceLife()
        {
            radioVoice=new GameObject("Radio broadcast").AddComponent<AudioSource>();radioVoice.transform.SetParent(transform,false);radioVoice.playOnAwake=false;radioVoice.spatialBlend=.25f;radioVoice.volume=0;radioVoice.minDistance=8;radioVoice.maxDistance=75;
            radioNoise=gameObject.AddComponent<AudioSource>();radioNoise.playOnAwake=false;radioNoise.spatialBlend=0;radioNoise.volume=0;radioNoise.loop=true;
            if(residenceTargets.TryGetValue("@fireplace",out var hearth)){hearthFlame=PanicAtmosphereArt.CreateFireplaceFlames();hearthFlame.transform.position=hearth.transform.TransformPoint(new Vector3(0,.13f,-.22f));hearthFlame.transform.rotation=hearth.transform.rotation;floorItems.Add(hearthFlame);hearthLight=new GameObject("Hearth warm flicker").AddComponent<Light>();hearthLight.transform.position=hearth.transform.TransformPoint(new Vector3(0,.7f,-.45f));hearthLight.type=LightType.Point;hearthLight.range=5.5f;hearthLight.color=new Color(1,.38f,.12f);hearthLight.intensity=1;}
            foreach(var p in new[]{new Vector3(-4,-.9f,-2),new Vector3(5,-.9f,3),new Vector3(11.7f,2.5f,3)}){var l=new GameObject("Service room lamp").AddComponent<Light>();l.type=LightType.Point;l.range=7;l.intensity=2.3f;l.color=new Color(1,.78f,.55f);l.transform.position=p;}
            lastNotice=Model.Message;ResetResidenceLife();
        }
        public void Jump(){if(!IsGrounded||jumpCooldown>0||WorkActive||InventoryOpen||IsMoving||CinematicActive)return;gravitySpeed=5.8f;jumpCooldown=.3f;effects.Play("jump",.30f);}
        public void ToggleContainer(){if(BagFinalStageActive)return;var s=NearestSource;if(s==null||!CanInteract)return;Expedition.ToggleSource(s.Id);LootIndex=FirstLoot(s);gatherPoseTime=.55f;if(BagFirstPersonActive)bagOpenAge=0;effects.Play(ContainerAudio(s.Id),.35f);if(s.Open)Notice(Model.Message);lastNotice=Model.Message;UpdateContext();}
        static int FirstLoot(LootSource s){for(int i=0;i<s.Contents.Count;i++)if(s.Contents[i].Count>0&&!string.IsNullOrEmpty(s.Contents[i].Id))return i;return 0;}
        public void SelectLoot(int index){var s=OpenSource;if(s==null||s.Contents.Count==0)return;int n=s.Contents.Count;int d=index<LootIndex?-1:1;index=(index%n+n)%n;for(int i=0;i<n;i++){if(s.Contents[index].Count>0&&!string.IsNullOrEmpty(s.Contents[index].Id)){LootIndex=index;return;}index=(index+d+n)%n;}}
        public bool TakeLoot() { return TakeWorldSupply(); }
        void UpdateLootTray()
        {
            // Containers add materials directly; food and equipment remain physical world pickups.
            if(lootTray){Destroy(lootTray);lootTray=null;}
            
        }
        void UpdateResidenceContext()
        {
            var supply=Expedition.FindWorldSupply(focusId);
            if(supply!=null && supply.Count>0)
            {
                FocusLabel=PanicExpedition.DisplayName(supply.ItemId);
                ShortAction=BagFirstPersonActive?"Al":"Çanta hazırlığına geç";
                ContextHint=CanInteract?(BagFirstPersonActive?"E / F · Al · Temel ihtiyaç çantaya eklenir":"Çanta hazırlığı son 45 saniyede başlar"):"";
                return;
            }
            if(focusId=="@bagstation") {FocusLabel="Afet çantası";ShortAction="";ContextHint="TAB · Çantayı düzenle";return;}
            var s=NearestSource;var item=Selected;var site=gardenSites.Find(x=>x.id==focusId);
            FocusLabel=s!=null?s.Label:item!=null?item.Label:site!=null?site.label:focusId=="@radio"?"Radyo":focusId=="@computer"?"Bilgisayar":focusId=="@microwave"?"Mikrodalga":focusId=="@fireplace"?"Şömine":focusId=="@garage"?"Garaj":focusId=="@laundry"?"Çamaşır makinesi":focusId=="@toilet"?"Tuvalet":focusId=="@sink"?"Lavabo":"";
            ShortAction=s!=null?(s.Open?"Kapat":"Aç"):focusId=="@bunker"||item!=null&&item.Id=="bunker"?"Sığın":focusId=="@radio"?(RadioEnabled?"Kapat":"Aç"):focusId=="@garage"?(garageOpen?"Kapat":"Aç"):focusId=="@fireplace"?(hearthEnabled?"Söndür":"Yak"):focusId=="@computer"?(computerEnabled?"Kapat":"Aç"):focusId=="@microwave"?(microwaveOpenV5?"Kapat":"Aç"):focusId!=null&&hinges.ContainsKey(focusId)?(hinges[focusId].open?"Kapat":"Aç"):item!=null&&item.Kind==ItemKind.Door?"Aç / Kapat":site!=null&&!site.done?"Başla":"";
            ContextHint=CanInteract?"E "+ShortAction:"";
            if (focusId == PanicItemActionRules.FireStationId)
            {
                FocusLabel = "Yangın ekipmanı noktası";
                ShortAction = Expedition.IsWorkCommitted("fire", focusId) ? "" : "Yangın söndürücüyü hazırla";
                ContextHint = CanInteract ? (Expedition.IsWorkCommitted("fire", focusId) ? "Yangın tüpü hazır" : "E Yangın söndürücüyü hazırla") : "";
            }
        }
        void ToggleHearth(){hearthEnabled=!hearthEnabled;effects.Play("ignite",hearthEnabled?.38f:.16f);if(hearthFlame)hearthFlame.SetActive(hearthEnabled);if(hearthLight)hearthLight.enabled=hearthEnabled;}
        void ToggleMicrowaveV5(){microwaveOpenV5=!microwaveOpenV5;effects.Play("door_wood",.32f);UpdateContext();}
        void ToggleComputer(){if(Model.UtilitySafety.PowerOff){Notice("Ana şalter kapalı; evde elektrik yok.");return;}computerEnabled=!computerEnabled;effects.Play("computer",.3f);ApplyComputerPower();UpdateContext();}
        void ApplyComputerPower()
        {
            if(!residenceTargets.TryGetValue("@computer",out var pc)||!pc)return;
            bool on=computerEnabled&&!Model.UtilitySafety.PowerOff;var b=new MaterialPropertyBlock();
            foreach(var r in pc.GetComponentsInChildren<Renderer>())
            {
                var m=r.sharedMaterial;if(!m||!m.HasProperty("_EmissionColor")||!(m.name.Contains("_Screen")||m.name.Contains("_Glow")))continue;
                r.GetPropertyBlock(b);b.SetColor("_EmissionColor",on?m.GetColor("_EmissionColor"):Color.black);
                if(m.HasProperty("_Color"))b.SetColor("_Color",on?m.color:new Color(.018f,.025f,.029f));r.SetPropertyBlock(b);b.Clear();
            }
        }
        void Notice(string text){ShortNotice=text;noticeAge=2.2f;}
        public void ToggleRadio(){RadioEnabled=false;radioVoice.Stop();radioNoise.Stop();RadioHeadline="";Notice("Radyo daha sonra");UpdateContext();}
        string DisasterVoiceId(){return Model.Disaster==DisasterType.Earthquake?"earthquake":Model.Disaster==DisasterType.Flood?"flood":"fire";}
        void QueueRadio(string id){queuedVoice=null;radioClock=35;}
        void PlayQueuedRadio()
        {
            if(DisasterAudioOnly||!RadioEnabled||string.IsNullOrEmpty(queuedVoice)||radioVoice.isPlaying)return;var clip=PanicAtmosphereArt.GetSound(queuedVoice);radioClipId=queuedVoice;queuedVoice=null;
            if(clip){radioVoice.clip=clip;radioVoice.Play();radioClock=35;RadioHeadline=radioClipId=="aftermath"?"Kurtarma ekipleri yolda":radioClipId=="deadline"?"Son dakika":radioClipId=="news"?"Yerel haberler":Model.Disaster==DisasterType.Earthquake?"Deprem bekleniyor":Model.Disaster==DisasterType.Flood?"Sel bekleniyor":"Yangın riski";}
        }
        void OnResidencePhase(PanicPhase phase,PanicPhase from)
        {
            BeginResidenceCinematic(phase,from);
            OnPresentationPhase(phase,from);
            if(phase==PanicPhase.Aftermath){if(aftermath)Destroy(aftermath);aftermath=CreateCompoundAftermathV54();StartRescue();QueueRadio("aftermath");}
            if(phase==PanicPhase.Preparing){minuteVoice=false;QueueRadio(DisasterVoiceId());if(aftermath)Destroy(aftermath);aftermath=null;}
        }
        void ResetResidenceLife()
        {
            if (hud) hud.CancelFreeplayOnboardingV414();
            InventoryIndex=0;AnchorMode="wall";focusId=null;
            ResetDirectActions();
            ResetRevisionWorld();
            ResetResidenceCinematic();ResetPresentation();ResetCameraOrbit();ResetExteriorLife();if(lootTray)Destroy(lootTray);lootTray=null;GatherFeedback=0;LootIndex=0;noticeAge=0;minuteVoice=false;radioClock=15;queuedVoice=null;
            if(aftermath)Destroy(aftermath);aftermath=null;if(helicopter)Destroy(helicopter);helicopter=null;if(rotorAudio)Destroy(rotorAudio);rotorAudio=null;radioVoice?.Stop();SetBunkerDoorOpen(false);
            foreach(var h in hinges.Values){h.open=false;h.closeBlocked=h.motionBlocked=false;}garageOpen=false;microwaveOpenV5=false;
        }
        void SetBunkerDoorOpen(bool open)
        {
            // The general hinge animator is the same door owner as the cinematic.
            // The leaf extends toward local -Z from its north hinge: positive X lifts it above the floor.
            if (hinges.TryGetValue("bunker", out var hatch)) { hatch.open = open; hatch.lid = true; hatch.openAngle = 105; }
            if (bunkerLeaf) bunkerLeaf.localRotation = bunkerClosed * Quaternion.Euler(open ? 105 : 0, 0, 0);
        }
        void StartRescue()
        {
            if(!helicopter){helicopter=PanicAtmosphereArt.CreateHelicopter();rotorAudio=helicopter.AddComponent<AudioSource>();rotorAudio.playOnAwake=false;rotorAudio.loop=true;rotorAudio.spatialBlend=.85f;rotorAudio.minDistance=10;rotorAudio.maxDistance=100;rotorAudio.volume=0;helicopterSearch=new GameObject("Rescue search beam").AddComponent<Light>();helicopterSearch.transform.SetParent(helicopter.transform,false);helicopterSearch.transform.localPosition=new Vector3(0,.7f,1);helicopterSearch.transform.localRotation=Quaternion.Euler(80,0,0);helicopterSearch.type=LightType.Spot;helicopterSearch.range=24;helicopterSearch.spotAngle=34;helicopterSearch.intensity=2.6f;helicopterSearch.color=new Color(.9f,.95f,1);}
            rescueAge=9;
        }
        void UpdateResidenceLife(float dt)
        {
            residenceTime+=dt;jumpCooldown=Mathf.Max(0,jumpCooldown-dt);GatherFeedback=Mathf.Max(0,GatherFeedback-dt);noticeAge=Mathf.Max(0,noticeAge-dt);
            if(!InputLocked)TickResidenceCinematic(dt);
            UpdateLootTray();
            UpdateInteractionOutline();
            TickPresentation(dt);
            if(hearthFlame&&hearthEnabled)PanicAtmosphereArt.AnimateFireplace(hearthFlame,residenceTime);if(hearthLight&&hearthEnabled)hearthLight.intensity=1.1f+Mathf.Sin(residenceTime*11)*.12f;
            if(garageLeaf)garageLeaf.localPosition=Vector3.Lerp(garageLeaf.localPosition,garageLeafClosed+Vector3.up*(garageOpen?2.35f:0),dt*5);
            if(residenceTargets.TryGetValue("@radio",out var radio)&&radioVoice)radioVoice.transform.position=radio.transform.position;
            if(Model.Phase!=PanicPhase.Title){radioClock-=dt;if(radioClock<=0&&string.IsNullOrEmpty(queuedVoice)&&!RadioSpeaking){QueueRadio((radioSequence++%3)==0?DisasterVoiceId():"news");}PlayQueuedRadio();}
            if(Model.Phase==PanicPhase.Preparing&&Model.TimeRemaining<=60&&!minuteVoice){minuteVoice=true;QueueRadio("deadline");}
            if(aftermath)PanicAtmosphereArt.AnimateAftermath(aftermath,residenceTime);
            if(helicopter){bool interiorShot=CinematicActive&&(CinematicKind=="Success"||CinematicKind=="BagFailure");bool visible=!interiorShot&&(!inside||ResidenceCinematicExteriorView);helicopter.SetActive(visible);if(visible){rescueAge+=dt;float cycle=Mathf.Repeat(rescueAge,27)/27;helicopter.transform.position=new Vector3(Mathf.Lerp(-31,32,cycle),9.2f+Mathf.Sin(cycle*Mathf.PI*2)*.7f,8-Mathf.Sin(cycle*Mathf.PI)*14);helicopter.transform.rotation=Quaternion.Euler(0,78,Mathf.Sin(rescueAge*.4f)*3);PanicAtmosphereArt.AnimateHelicopter(helicopter,residenceTime);}}
            if(Model.Message!=lastNotice){lastNotice=Model.Message;string m=(Model.Message??"").ToLowerInvariant();if(m.Contains("gereki")||m.Contains("eksik")||m.Contains("yok"))Notice(m.Contains("bütçe")?"Para yetmiyor":"Malzeme eksik");else if(m.Contains("dolu"))Notice("Yer yok");}
        }
    }
}




