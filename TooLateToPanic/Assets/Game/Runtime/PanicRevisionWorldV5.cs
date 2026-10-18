using System;
using System.Collections.Generic;
using UnityEngine;
using B = TooLateToPanic.PanicHomeArt.Builder;
using M = TooLateToPanic.PanicHomeArt.M;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        public static readonly Vector3 PanicRoomPositionV5 = new Vector3(4.2f, 3.2f, .55f);
        public bool UsesUpperPanicRoomV5 { get { return Model != null && Model.HasActiveDisasterV54(DisasterType.Flood); } }
        public Vector3 ActiveShelterPositionV5 { get { return UsesUpperPanicRoomV5 ? PanicRoomPositionV5 : BunkerPosition; } }
        public string ActiveShelterLabelV5 { get { return UsesUpperPanicRoomV5 ? "Üst kat panik odası" : "Bodrum sığınağı"; } }
        public string ActiveShelterEntryHintV56
        {
            get
            {
                if (Model == null) return string.Empty;
                if (!BagFinalStageActive) return "Çanta hazırlığı son45 saniyede açılır";
                int water = Mathf.Max(0, PanicModel.RequiredSupplyPerWave - Model.PackedWater);
                int food = Mathf.Max(0, PanicModel.RequiredSupplyPerWave - Model.PackedFood);
                string missing = water > 0 ? water + " su" : string.Empty;
                if (food > 0) missing += (missing.Length > 0 ? " + " : string.Empty) + food + " gıda";
                return missing.Length > 0 ? "E · Afeti başlat · Eksik: " + missing : "E · Hazırlığı bitir, afeti başlat";
            }
        }
        public int MainsLightCountV5 { get { return mainsLightsV5.Count; } }
        public int MainsEmitterCountV5 { get { return mainsEmittersV5.Count; } }
        public bool MainsPoweredV5 { get { return Model != null && !Model.UtilitySafety.PowerOff; } }
        public PanicNeighbourQuakeV5 NeighbourQuakeV5 { get; private set; }
        readonly List<Light> mainsLightsV5 = new List<Light>();
        readonly List<Renderer> mainsEmittersV5 = new List<Renderer>();
        MaterialPropertyBlock mainsBlockV5;
        Light panicRoomBeaconV5;
        GameObject secondaryFeedbackV5;
        int secondaryFeedbackWaveV5=-1;
        float secondaryFeedbackAgeV5;

        void ConfigureV5SourceArt()
        {
            foreach(var source in Expedition.Sources)
            {
                switch(source.Id)
                {
                    case "decor_pantry_back":source.ArtId="v5_shelf_large";source.Rotation=180;break;
                    case "decor_pantry_side":source.ArtId="v5_shelf_small";source.Rotation=90;break;
                    case "decor_child_cabinet":source.ArtId="v5_cabinet_wardrobe";source.Rotation=90;break;
                    case "decor_garage_shelf":source.ArtId="v5_cabinet_utility";source.Rotation=90;break;
                    case "decor_basement_shelf":source.ArtId="v5_shelf_large";source.Rotation=180;break;
                    case "decor_basement_spare_crates":source.ArtId="v5_cabinet_pantry";source.Rotation=270;break;
                }
            }
        }

        void AnimateV5Furniture()
        {
            foreach(var source in Expedition.Sources)
            {
                if(hinges.TryGetValue(source.Id,out var hinge))hinge.open=source.Open;
                if(sourceObjects.TryGetValue(source.Id,out var visual)&&visual&&visual.TryGetComponent<PanicV5FurnitureRig>(out var rig)){rig.SetOpen(source.Open);AdvanceSafeFurnitureV5(rig,Mathf.Min(Time.deltaTime,.08f));}
            }
            TickSecondaryFeedbackV5();
        }

        void AdvanceSafeFurnitureV5(PanicV5FurnitureRig rig,float dt)
        {
            float current=rig.OpenAmount;
            rig.Advance(dt);float desired=rig.OpenAmount;
            if(!controller||!controller.enabled||current==desired)return;
            rig.SetOpenAmount(current);float clearance=FurnitureDoorClearanceV5(rig);
            int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Abs(desired-current)*100f/1.25f));float safe=current;
            for(int i=1;i<=steps;i++)
            {
                float candidate=Mathf.Lerp(current,desired,i/(float)steps);rig.SetOpenAmount(candidate);
                float next=FurnitureDoorClearanceV5(rig);
                if(next<.035f&&next<=clearance+.00001f){rig.SetOpenAmount(safe);return;}
                safe=candidate;clearance=next;
            }
        }

        float FurnitureDoorClearanceV5(PanicV5FurnitureRig rig)
        {
            float distance=float.PositiveInfinity;
            Vector3 center=player.TransformPoint(controller.center),actorScale=player.lossyScale;
            float radius=controller.radius*Mathf.Max(Mathf.Abs(actorScale.x),Mathf.Abs(actorScale.z));
            float segment=Mathf.Max(0,controller.height*Mathf.Abs(actorScale.y)*.5f-radius);
            foreach(var door in rig.Doors)if(door)foreach(var box in door.GetComponentsInChildren<BoxCollider>(true))
            {
                if(!box.enabled||box.isTrigger)continue;
                Vector3 scale=box.transform.lossyScale;
                var half=Vector3.Scale(box.size,new Vector3(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z)))*.5f;
                var local=Quaternion.Inverse(box.transform.rotation)*(center-box.transform.TransformPoint(box.center));
                float x=Mathf.Max(0,Mathf.Abs(local.x)-half.x),z=Mathf.Max(0,Mathf.Abs(local.z)-half.z),y=Mathf.Max(0,Mathf.Abs(local.y)-segment-half.y);
                distance=Mathf.Min(distance,Mathf.Sqrt(x*x+y*y+z*z)-radius);
            }
            return distance;
        }

        void TickSecondaryFeedbackV5()
        {
            if(Model.Phase==PanicPhase.Title||Model.Phase==PanicPhase.Preparing||Model.Phase==PanicPhase.Tutorial)
            {
                if(secondaryFeedbackV5){floorItems.Remove(secondaryFeedbackV5);Destroy(secondaryFeedbackV5);}
                secondaryFeedbackV5=null;secondaryFeedbackWaveV5=-1;return;
            }
            if(Model.SecondaryEventsV5.Count>0&&secondaryFeedbackWaveV5!=Model.Wave)
            {
                if(secondaryFeedbackV5){floorItems.Remove(secondaryFeedbackV5);Destroy(secondaryFeedbackV5);}
                secondaryFeedbackWaveV5=Model.Wave;secondaryFeedbackAgeV5=0;
                var incident=Model.SecondaryEventsV5[0];var item=Model.Items.Find(i=>i.Id==incident.TargetId);
                if(item==null)return;
                secondaryFeedbackV5=new GameObject("V5 actual secondary event "+incident.Kind);
                secondaryFeedbackV5.transform.position=item.Position+new Vector3(0,.03f,-.38f);
                var b=new B();
                if(incident.Kind==PanicSecondaryEventKindV5.PipeLeak)
                {
                    b.Ellipsoid(new Vector3(0,.012f,0),new Vector3(.45f,.018f,.3f),M.Glass,71);
                    for(int i=0;i<5;i++)b.Ellipsoid(new Vector3((i-2)*.055f,.16f+i*.095f,.02f),new Vector3(.014f,.028f,.014f),M.Glass,72+i);
                }
                else if(incident.Kind==PanicSecondaryEventKindV5.ElectricalShort)
                {
                    for(int i=0;i<5;i++)b.Beam(new Vector3(0,.32f,0),new Vector3(Mathf.Cos(i*1.26f)*.18f,.32f+Mathf.Sin(i*1.26f)*.18f,-.03f),.012f,M.SafetyAmber);
                    b.Box(new Vector3(0,.025f,0),new Vector3(.3f,.02f,.22f),M.Charcoal,.025f);
                }
                else
                {
                    var flame=new GameObject("V5 secondary flame");flame.transform.SetParent(secondaryFeedbackV5.transform,false);
                    var tongues=new B();
                    for(int i=0;i<3;i++)tongues.Ellipsoid(new Vector3((i-1)*.10f,.18f+i*.025f,0),new Vector3(.06f,.16f+i*.035f,.065f),i==1?M.SafetyAmber:M.Red,83+i);
                    tongues.Attach(flame,"small_gas_flare");
                    var light=flame.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.43f,.10f);light.intensity=.6f;light.range=2;
                    b.Box(new Vector3(0,.012f,0),new Vector3(.45f,.02f,.32f),M.Charcoal,.03f);
                }
                b.Attach(secondaryFeedbackV5,"small_secondary_damage_cue");floorItems.Add(secondaryFeedbackV5);
                Notice(incident.Label);
            }
            if(!secondaryFeedbackV5)return;
            secondaryFeedbackAgeV5+=Mathf.Min(Time.deltaTime,.08f);
            // Cosmetic cues never roll a second event or alter the already-resolved damage.
            if(secondaryFeedbackAgeV5>3.2f)
            {
                var fire=secondaryFeedbackV5.transform.Find("V5 secondary flame");if(fire)fire.gameObject.SetActive(false);
            }
        }

        void AddBedCollisionV5(GameObject root)
        {
            // A whole-bed AABB included the tall headboard above every pickup on the mattress.
            // Two authored physical pieces retain blocking without an invisible wall over blankets.
            var mattress=root.AddComponent<BoxCollider>();mattress.center=new Vector3(0,.39f,0);mattress.size=new Vector3(1.78f,.78f,2.16f);
            var headboard=root.AddComponent<BoxCollider>();headboard.center=new Vector3(0,.76f,1.09f);headboard.size=new Vector3(1.78f,1.10f,.12f);
        }

        void ConfigurePickupColliderV5(GameObject root,GameObject visual)
        {
            var bounds=new Bounds();bool first=true;
            foreach(var r in visual.GetComponentsInChildren<Renderer>(true))
            {
                var b=r.bounds;
                for(int corner=0;corner<8;corner++)
                {
                    var p=root.transform.InverseTransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1)));
                    if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);
                }
            }
            var hit=root.AddComponent<BoxCollider>();
            hit.center=first?new Vector3(0,.12f,0):bounds.center;
            var size=first?new Vector3(.28f,.30f,.28f):bounds.size+Vector3.one*.035f;
            hit.size=new Vector3(Mathf.Max(.16f,size.x),Mathf.Max(.18f,size.y),Mathf.Max(.16f,size.z));hit.isTrigger=true;
        }

        void InitRevisionV5()
        {
            BuildPanicRoomV5();
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (light.name.StartsWith("Interior lamp ", StringComparison.Ordinal) || light.name == "Service room lamp" || light.name == "Bag amber pin light")
                    mainsLightsV5.Add(light);
            var seen = new HashSet<Renderer>();
            foreach (var root in floorItems)
            {
                if (!root || root.name == "@fireplace" || root.name == "@panicroom" || root.name == "bagstation") continue;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (residenceTargets.TryGetValue("@computer", out var pc) && pc && renderer.transform.IsChildOf(pc.transform)) continue;
                    var material = renderer.sharedMaterial;
                    if (material && material.HasProperty("_EmissionColor") && material.GetColor("_EmissionColor").maxColorComponent > .001f && seen.Add(renderer))
                        mainsEmittersV5.Add(renderer);
                }
            }
            NeighbourQuakeV5 = gameObject.AddComponent<PanicNeighbourQuakeV5>();
            NeighbourQuakeV5.Bind(this, landscape);
            ApplyMainsPowerV5();
        }

        void BuildPanicRoomV5()
        {
            PanicHomeArt.EnsurePalette();
            var room = new GameObject("V5 upper flood panic room fittings");
            room.transform.position = new Vector3(5.25f, 3.2f, 1.35f);
            var b = new B();
            // A raised seat, dry storage beneath and two folded emergency blankets.
            b.Box(new Vector3(0,.48f,0),new Vector3(2.65f,.14f,.62f),M.Walnut,.025f);
            for(int s=-1;s<=1;s+=2)
                b.Box(new Vector3(s*1.16f,.23f,0),new Vector3(.09f,.46f,.52f),M.Charcoal,.01f);
            b.Box(new Vector3(0,.17f,.05f),new Vector3(1.9f,.28f,.43f),M.Slate,.025f);
            b.Box(new Vector3(0,.62f,.08f),new Vector3(1.95f,.17f,.42f),M.Linen,.035f);
            for(int i=0;i<2;i++)
            {
                b.Box(new Vector3(.65f,.76f+i*.08f,0),new Vector3(.48f,.075f,.38f),i==0?M.Olive:M.SafetyAmber,.025f);
                b.Box(new Vector3(.65f,.80f+i*.08f,0),new Vector3(.035f,.006f,.39f),M.Linen,.002f);
            }
            // Wall hooks with high visibility vests and a battery emergency lantern.
            for(int s=-1;s<=1;s+=2)
            {
                b.Box(new Vector3(s*.88f,1.43f,.28f),new Vector3(.35f,.54f,.065f),M.SafetyAmber,.025f);
                b.Box(new Vector3(s*.88f,1.34f,.235f),new Vector3(.35f,.055f,.018f),M.Linen,.003f);
                b.Box(new Vector3(s*.88f,1.49f,.235f),new Vector3(.35f,.055f,.018f),M.Linen,.003f);
            }
            b.Attach(room,"raised_dry_bench_and_vests");
            var benchCollider=room.AddComponent<BoxCollider>();benchCollider.center=new Vector3(0,.4f,0);benchCollider.size=new Vector3(2.65f,.78f,.62f);
            floorItems.Add(room);residenceObstacles.Add(benchCollider);
            var marker = new GameObject("@panicroom");marker.transform.position=PanicRoomPositionV5;
            var sign=new B();
            sign.Box(new Vector3(0,1.35f,0),new Vector3(.62f,.52f,.08f),M.Olive,.025f);
            sign.Box(new Vector3(0,1.35f,-.046f),new Vector3(.54f,.44f,.015f),M.Linen,.015f);
            sign.Box(new Vector3(0,1.28f,-.058f),new Vector3(.055f,.18f,.014f),M.Olive,.003f);
            sign.Beam(new Vector3(-.12f,1.4f,-.058f),new Vector3(0,1.52f,-.058f),.035f,M.Olive);
            sign.Beam(new Vector3(0,1.52f,-.058f),new Vector3(.12f,1.4f,-.058f),.035f,M.Olive);
            sign.Box(new Vector3(0,.65f,.02f),new Vector3(.045f,1.3f,.045f),M.Charcoal,.006f);
            sign.Box(new Vector3(0,.035f,.02f),new Vector3(.35f,.07f,.26f),M.Charcoal,.015f);
            sign.Attach(marker,"flood_refuge_sign");
            var pick=marker.AddComponent<HomePick>();pick.Id="@panicroom";pick.Kind="shelter";
            var hit=marker.AddComponent<BoxCollider>();hit.center=new Vector3(0,1.35f,0);hit.size=new Vector3(.64f,.54f,.10f);
            residenceTargets["@panicroom"]=marker;floorItems.Add(marker);
            panicRoomBeaconV5=new GameObject("Panic room battery emergency lamp").AddComponent<Light>();
            panicRoomBeaconV5.transform.position=PanicRoomPositionV5+new Vector3(0,1.72f,0);
            panicRoomBeaconV5.type=LightType.Point;panicRoomBeaconV5.range=3;panicRoomBeaconV5.color=new Color(.55f,1f,.69f);panicRoomBeaconV5.intensity=.25f;
            var door=Model.Items.Find(i=>i.Id=="door_study");if(door!=null)door.Label="Panik odası";
        }

        public bool TryEnterActiveShelterV5()
        {
            if (!BagFinalStageActive) { Notice("Önce önlemlerini tamamla; çanta hazırlığı son45 saniyede açılır.");return false; }
            string expected=UsesUpperPanicRoomV5?"@panicroom":"bunker";
            bool correct=focusId==expected||!UsesUpperPanicRoomV5&&focusId=="@bunker";
            if (!correct) { Notice(UsesUpperPanicRoomV5?"Sel sırasında bodruma inme. Üst kat panik odasına git.":"Bu afette bodrum sığınağına git.");return false; }
            if (!controller||!controller.isGrounded||!SameLevel(ActiveShelterPositionV5)||!CanInteract)
            {Notice(ActiveShelterLabelV5+" girişine açık yoldan yaklaş.");return false;}
            if (!UsesUpperPanicRoomV5&&!BuildBunkerApproachRouteV414(PlayerPosition,BunkerPosition+new Vector3(0,.04f,-1.18f)))
            {Notice("Kapak önünü boş bırak ve açık yoldan yaklaş.");return false;}
            bool accepted=Expedition.EnterShelter();
            return accepted;
        }

        bool UpdateShelterContextV5()
        {
            if(focusId!="@panicroom"&&focusId!="bunker"&&focusId!="@bunker")return false;
            bool correct=UsesUpperPanicRoomV5?focusId=="@panicroom":focusId!="@panicroom";
            FocusLabel=focusId=="@panicroom"?"Üst kat panik odası":"Bodrum sığınağı";
            ShortAction=correct?(BagFinalStageActive?"Afeti başlat":"Çanta aşamasını bekle"):"Diğer güvenli alana git";
            ContextHint=correct?ActiveShelterEntryHintV56:
                UsesUpperPanicRoomV5?"Sel: üst kat panik odasını kullan":"Bu afette bodrum sığınağını kullan";
            return true;
        }

        void ApplyMainsPowerV5()
        {
            if(Model==null)return;
            if(mainsBlockV5==null)mainsBlockV5=new MaterialPropertyBlock();
            bool on=!Model.UtilitySafety.PowerOff;
            foreach(var light in mainsLightsV5)if(light)light.enabled=on;
            foreach(var renderer in mainsEmittersV5)
            {
                if(!renderer||!renderer.sharedMaterial)continue;
                var material=renderer.sharedMaterial;
                renderer.GetPropertyBlock(mainsBlockV5);
                mainsBlockV5.SetColor("_EmissionColor",on?material.GetColor("_EmissionColor"):Color.black);
                if(material.HasProperty("_Color"))
                {
                    Color color=material.color;
                    if(!on)color=Color.Lerp(color,new Color(.025f,.032f,.036f),.94f);
                    mainsBlockV5.SetColor("_Color",color);
                }
                renderer.SetPropertyBlock(mainsBlockV5);mainsBlockV5.Clear();
            }
            if(panicRoomBeaconV5)panicRoomBeaconV5.enabled=UsesUpperPanicRoomV5&&Model.Phase!=PanicPhase.Title;
        }
    }

    /// <summary>Neighbour scenery only. No collider, character, simulation clock or RNG changes.</summary>
    public sealed class PanicNeighbourQuakeV5 : MonoBehaviour
    {
        readonly List<Transform> houses=new List<Transform>();
        readonly List<Vector3> origins=new List<Vector3>();
        readonly List<Quaternion> rotationsV54=new List<Quaternion>();
        PanicHomeWorld world;
        public int HouseCount {get{return houses.Count;}}
        public int MovingHouses {get;private set;}
        public float MaximumOffset {get;private set;}
        public float PresentationScaleV52 {get;private set;} = 1f;
        public float CurrentMaximumOffsetV52 {get;private set;}
        public float CurrentMaximumTiltV52 {get;private set;}
        public void Bind(PanicHomeWorld owner,GameObject landscape)
        {
            RestoreNeighbourPosesV54();
            houses.Clear();origins.Clear();rotationsV54.Clear();
            world=owner;if(!landscape)return;
            foreach(var t in landscape.GetComponentsInChildren<Transform>(true))
                if(t.name.StartsWith("V5 Neighbour house ",StringComparison.Ordinal)){houses.Add(t);origins.Add(t.localPosition);rotationsV54.Add(t.localRotation);}
        }
        void LateUpdate()
        {
            if(!world||world.Model==null){RestoreNeighbourPosesV54();return;}
            if(world.Model.IsPaused||world.NotebookBlocksInput){RestoreNeighbourPosesV54();return;}
            bool active=world.QuakeReadabilityWindowV414;
            PresentationScaleV52=PanicDisasterArt.ClampPresentationScaleV52(world.Model.DisasterPresentationScaleV52);
            float t=world.QuakeReadabilityAgeV414-4.7f;
            float envelope=active?Mathf.SmoothStep(0,1,t/.2f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(7.7f,8f,t+4.7f))):0;
            MovingHouses=0;CurrentMaximumOffsetV52=CurrentMaximumTiltV52=0;
            for(int i=0;i<houses.Count;i++)
            {
                var h=houses[i];if(!h)continue;
                Vector3 shift=new Vector3(Mathf.Sin(t*27+i*.7f)*.07f,Mathf.Sin(t*35+i)*.018f,Mathf.Cos(t*24+i)*.045f)*envelope*PresentationScaleV52;
                h.localPosition=origins[i]+shift;h.localRotation=rotationsV54[i]*Quaternion.Euler(Mathf.Sin(t*24+i)*.38f*envelope*PresentationScaleV52,0,Mathf.Cos(t*28+i)*.55f*envelope*PresentationScaleV52);
                if(shift.sqrMagnitude>.000001f)MovingHouses++;
                MaximumOffset=Mathf.Max(MaximumOffset,shift.magnitude);
                CurrentMaximumOffsetV52=Mathf.Max(CurrentMaximumOffsetV52,shift.magnitude);
                CurrentMaximumTiltV52=Mathf.Max(CurrentMaximumTiltV52,Quaternion.Angle(rotationsV54[i],h.localRotation));
            }
        }
        void RestoreNeighbourPosesV54()
        {
            for(int i=0;i<houses.Count;i++)if(houses[i])
            {houses[i].localPosition=origins[i];houses[i].localRotation=rotationsV54[i];}
            MovingHouses=0;CurrentMaximumOffsetV52=CurrentMaximumTiltV52=0;
        }
        void OnDisable(){RestoreNeighbourPosesV54();}
        void OnDestroy(){RestoreNeighbourPosesV54();}
    }
}
