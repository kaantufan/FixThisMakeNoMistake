using UnityEngine;
using B = TooLateToPanic.PanicHomeArt.Builder;
using M = TooLateToPanic.PanicHomeArt.M;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        // Visual fittings only. Physical shelter admission, pickups, door collision and costs
        // remain owned by their existing systems. Call once at the end of BuildPanicRoomV5.
        void BuildPanicRoomIdentityV512()
        {
            PanicHomeArt.EnsurePalette();
            var root = new GameObject("V512 protected upper panic room identity");
            root.transform.position = new Vector3(5.25f, 3.2f, 1.35f);
            floorItems.Add(root);
            var b = new B();
            // Low bolted reinforcement behind the retained dry bench; no new enclosure or
            // tall wall obscures the isometric view. World north edge of this room is z=1.8.
            b.Box(new Vector3(0,.5f,.32f),new Vector3(3.25f,.92f,.055f),M.Slate,.015f);
            for(int i=0;i<5;i++)
            {
                float x=-1.5f+i*.75f;
                b.Box(new Vector3(x,.5f,.278f),new Vector3(.045f,.94f,.025f),M.Steel,.006f);
                for(int j=0;j<2;j++)
                    b.Cylinder(new Vector3(x,.12f+j*.76f,.257f),.022f,.012f,M.Steel,8,Quaternion.Euler(90,0,0));
            }
            b.Box(new Vector3(0,.99f,.3f),new Vector3(3.3f,.06f,.09f),M.Steel,.008f);
            // Two shallow protection panels against the existing east wall, clear of the
            // retained bookshelf at x=7.35,z=-.25. They stop below eye level/cutaway line.
            for(int i=0;i<2;i++)
            {
                float z=-4.9f+i*1.2f;
                b.Box(new Vector3(2.57f,.49f,z),new Vector3(.045f,.88f,1.05f),M.Slate,.012f);
                b.Box(new Vector3(2.535f,.95f,z),new Vector3(.04f,.045f,1.08f),M.Steel,.005f);
                for(int s=-1;s<=1;s+=2)
                    b.Box(new Vector3(2.536f,.5f,z+s*.46f),new Vector3(.025f,.84f,.025f),M.Steel,.004f);
            }
            // Closed wall-mounted first-aid box with an unmistakable cross; decorative,
            // distinct from collectible loot and inside the retained bench footprint.
            b.Box(new Vector3(0,1.39f,.24f),new Vector3(.57f,.5f,.12f),M.Linen,.025f);
            b.Box(new Vector3(0,1.39f,.17f),new Vector3(.45f,.4f,.02f),M.Olive,.01f);
            b.Box(new Vector3(0,1.39f,.15f),new Vector3(.25f,.07f,.018f),M.Linen,.003f);
            b.Box(new Vector3(0,1.39f,.15f),new Vector3(.07f,.25f,.018f),M.Linen,.003f);
            // Existing storage below the bench gets a visible protective strap and latch.
            for(int s=-1;s<=1;s+=2)
                b.Box(new Vector3(s*.58f,.18f,-.174f),new Vector3(.06f,.25f,.018f),M.Steel,.004f);
            b.Box(new Vector3(0,.2f,-.182f),new Vector3(.11f,.1f,.025f),M.Charcoal,.008f);
            // Battery lamp housing, not a new dynamic light or simulated power source.
            b.Box(new Vector3(0,1.83f,.23f),new Vector3(.68f,.16f,.14f),M.Charcoal,.018f);
            b.Box(new Vector3(0,1.83f,.15f),new Vector3(.55f,.09f,.025f),M.Linen,.012f);
            b.Box(new Vector3(.26f,1.83f,.128f),new Vector3(.025f,.025f,.01f),M.Olive,.004f);
            // Dedicated emergency communications shelf next to the blankets. The actual
            // retained Yarkin radio is used instead of a new stand-in model.
            b.Box(new Vector3(1.48f,1.08f,.12f),new Vector3(.69f,.055f,.36f),M.Steel,.01f);
            b.Beam(new Vector3(1.26f,.91f,.29f),new Vector3(1.26f,1.05f,-.02f),.035f,M.Steel);
            b.Beam(new Vector3(1.7f,.91f,.29f),new Vector3(1.7f,1.05f,-.02f),.035f,M.Steel);
            b.Attach(root,"bolted_shelter_fittings");
            var radio=PanicCuratedAssetArt.CreateDisplay("EmergencyRadio",.42f);
            if(radio)
            {
                radio.name="V512 emergency radio display - no pickup";
                radio.transform.SetParent(root.transform,false);
                radio.transform.localPosition=new Vector3(1.48f,1.11f,.10f);
            }
            // Sign follows the same floor render ownership as the actual shelter marker.
            var plate=new B();
            plate.Box(new Vector3(0,1.08f,.225f),new Vector3(1.43f,.21f,.028f),M.Olive,.012f);
            plate.Attach(root,"panic_room_identity_plate");
            PanicRoomLabelV512(root.transform,new Vector3(0,1.08f,.205f),"PANİK ODASI",.03f);
            ReinforcePanicRoomDoorVisualV512();
        }

        void ReinforcePanicRoomDoorVisualV512()
        {
            if(!hinges.TryGetValue("door_study",out var hinge)||!hinge.leaf)return;
            // Added after the production hinge collider was created. All skins stay inside
            // the existing handle depth and never add collision or a second hinge owner.
            var skin=new GameObject("V512 bolted steel panic door skin");
            skin.transform.SetParent(hinge.leaf,false);
            var b=new B();
            for(int side=-1;side<=1;side+=2)
            {
                b.Box(new Vector3(.5f,1.08f,side*.085f),new Vector3(.88f,1.98f,.012f),M.Slate,.014f);
                for(int row=0;row<3;row++)
                {
                    float y=.35f+row*.73f;
                    b.Box(new Vector3(.5f,y,side*.097f),new Vector3(.82f,.065f,.012f),M.Steel,.005f);
                    for(int edge=-1;edge<=1;edge+=2)
                        b.Cylinder(new Vector3(.5f+edge*.35f,y,side*.108f),.019f,.01f,M.Charcoal,8,Quaternion.Euler(90,0,0));
                }
                b.Box(new Vector3(.5f,1.55f,side*.103f),new Vector3(.35f,.18f,.012f),M.Olive,.008f);
                b.Box(new Vector3(.5f,1.55f,side*.113f),new Vector3(.17f,.04f,.008f),M.Linen,.002f);
                b.Box(new Vector3(.5f,1.55f,side*.113f),new Vector3(.04f,.12f,.008f),M.Linen,.002f);
            }
            b.Attach(skin,"steel_skin_in_original_door_bounds");
            var frame=new B();
            for(int s=-1;s<=1;s+=2)
                frame.Box(new Vector3(s*.556f,1.125f,-.098f),new Vector3(.075f,2.22f,.012f),M.Steel,.006f);
            frame.Box(new Vector3(0,2.214f,-.098f),new Vector3(1.08f,.065f,.012f),M.Steel,.006f);
            frame.Attach(hinge.frame.gameObject,"V512_steel_panic_door_frame");
        }

        static void PanicRoomLabelV512(Transform parent,Vector3 p,string text,float size)
        {
            var go=new GameObject("V512 shelter identity lettering");
            go.transform.SetParent(parent,false);go.transform.localPosition=p;
            go.transform.localRotation=Quaternion.identity;
            var label=go.AddComponent<TextMesh>();label.text=text;label.fontSize=48;
            label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;
            label.alignment=TextAlignment.Center;label.color=new Color(.94f,.95f,.86f);
            // Default world text ignores depth. Use the already-retained sprite shader
            // so the small plate is hidden by real walls and never floats through the house.
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.font=font;
            var renderer=go.GetComponent<MeshRenderer>();
            var material=new Material(Shader.Find("Sprites/Default"));
            material.mainTexture=font.material.mainTexture;material.color=Color.white;
            renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows=false;
        }
    }
}

