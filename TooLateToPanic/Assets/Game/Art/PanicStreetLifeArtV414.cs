using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using B=TooLateToPanic.PanicHomeArt.Builder;
using M=TooLateToPanic.PanicHomeArt.M;

namespace TooLateToPanic
{
    // Original offline procedural street props. Cosmetic only: no colliders, score or damage mutation.
    public static class PanicStreetLifeArtV414
    {
        public enum Service { Police,Ambulance,Fire,Rescue }
        public static GameObject CivilianCar(int variant)
        {
            var root=PanicResidenceArt.Create("car");root.name="V414 street car "+variant;
            var owned=root.AddComponent<PanicStreetOwnedArtV414>();
            Color[] colors={new Color(.22f,.36f,.42f),new Color(.54f,.31f,.20f),new Color(.47f,.48f,.39f),new Color(.34f,.29f,.40f)};
            var replacements=new Dictionary<Material,Material>();
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    var source=materials[i];if(!source||!source.name.Contains("Olive"))continue;
                    if(!replacements.TryGetValue(source,out var clone))
                    {clone=new Material(source){name="V414 civilian body "+variant,color=colors[variant%colors.Length]};replacements[source]=clone;owned.Owned.Add(clone);}
                    materials[i]=clone;
                }
                renderer.sharedMaterials=materials;
            }
            return root;
        }
        public static GameObject EmergencyVehicle(Service service)
        {
            PanicHomeArt.EnsurePalette();var root=new GameObject("V414 "+service+" response vehicle");
            var owned=root.AddComponent<PanicStreetOwnedArtV414>();var b=new B();
            bool truck=service==Service.Fire||service==Service.Rescue;
            float length=truck?5.5f:service==Service.Ambulance?4.85f:4.35f;
            M body=service==Service.Fire?M.Red:service==Service.Rescue?M.SafetyAmber:M.Linen;
            b.Box(new Vector3(0,.47f,0),new Vector3(1.92f,.36f,length),M.Charcoal,.08f);
            b.Box(new Vector3(0,.85f,0),new Vector3(1.89f,.65f,length-.18f),body,.065f);
            // Cab at+Z; raised response body and a separate windshield define a real vehicle silhouette.
            b.Box(new Vector3(0,1.40f,length*.27f),new Vector3(1.72f,.65f,1.5f),body,.04f);
            b.Box(new Vector3(0,1.49f,length*.27f+.76f),new Vector3(1.50f,.40f,.035f),M.Glass,.018f);
            b.Box(new Vector3(0,1.18f,length*.5f-.05f),new Vector3(1.75f,.22f,.08f),body,.025f);
            for(int side=-1;side<=1;side+=2)
            {
                b.Box(new Vector3(side*.873f,1.46f,length*.27f),new Vector3(.027f,.42f,1.18f),M.Glass,.01f);
                b.Box(new Vector3(side*.976f,.96f,.08f),new Vector3(.025f,.20f,length-.35f),service==Service.Police?M.Slate:service==Service.Ambulance?M.Red:M.Linen,.004f);
                b.Box(new Vector3(side*.91f,1.01f,length*.13f),new Vector3(.025f,.035f,.24f),M.Steel,.007f);
                b.Beam(new Vector3(side*.92f,1.40f,length*.38f),new Vector3(side*1.10f,1.40f,length*.38f),.035f,M.Charcoal);
                b.Box(new Vector3(side*1.11f,1.43f,length*.38f),new Vector3(.08f,.17f,.14f),M.Charcoal,.01f);
            }
            b.Box(new Vector3(0,.53f,length*.5f),new Vector3(1.99f,.17f,.13f),M.Steel,.025f);
            b.Box(new Vector3(0,.51f,-length*.5f),new Vector3(1.99f,.17f,.13f),M.Steel,.025f);
            b.Box(new Vector3(0,.78f,length*.5f+.015f),new Vector3(.88f,.22f,.025f),M.Charcoal,.008f);
            for(int side=-1;side<=1;side+=2)
            {b.Box(new Vector3(side*.70f,.89f,length*.5f+.025f),new Vector3(.34f,.18f,.045f),M.WarmWindow,.018f);b.Box(new Vector3(side*.76f,.85f,-length*.5f-.025f),new Vector3(.20f,.29f,.035f),M.Red,.01f);}
            if(service==Service.Ambulance)
            {
                b.Box(new Vector3(0,1.46f,-.68f),new Vector3(1.94f,1.05f,2.70f),M.Linen,.045f);
                for(int side=-1;side<=1;side+=2)
                {b.Box(new Vector3(side*.986f,1.43f,-.55f),new Vector3(.02f,.16f,.66f),M.Red,.004f);b.Box(new Vector3(side*.99f,1.43f,-.55f),new Vector3(.024f,.64f,.16f),M.Red,.004f);}
            }
            else if(service==Service.Fire)
            {
                b.Box(new Vector3(0,1.32f,-.70f),new Vector3(1.90f,.85f,2.9f),M.Red,.03f);
                for(int side=-1;side<=1;side+=2)for(int bay=0;bay<3;bay++)
                {float z=-1.65f+bay*.82f;b.Box(new Vector3(side*.968f,1.28f,z),new Vector3(.023f,.62f,.70f),M.Steel,.009f);for(int slat=0;slat<6;slat++)b.Box(new Vector3(side*.984f,1.02f+slat*.10f,z),new Vector3(.012f,.014f,.64f),M.Charcoal,.002f);}
                for(int side=-1;side<=1;side+=2)b.Beam(new Vector3(side*.34f,1.85f,-2.1f),new Vector3(side*.34f,1.85f,1.1f),.06f,M.Steel);
                for(int i=0;i<11;i++)b.Beam(new Vector3(-.34f,1.85f,-2.1f+i*.31f),new Vector3(.34f,1.85f,-2.1f+i*.31f),.04f,M.Steel);
            }
            else if(service==Service.Rescue)
            {b.Box(new Vector3(0,1.18f,-1.0f),new Vector3(1.8f,.60f,2.5f),M.SafetyAmber,.025f);b.Box(new Vector3(0,1.52f,-1),new Vector3(1.70f,.08f,2.3f),M.Charcoal,.01f);for(int i=0;i<3;i++)b.Box(new Vector3(-.5f+i*.5f,1.65f,-1),new Vector3(.42f,.22f,.75f),M.Olive,.025f);}
            b.Attach(root,"ResponseBodyCabMirrorsCompartments");
            foreach(int side in new[]{-1,1})foreach(float z in new[]{-length*.30f,length*.30f})
            {
                var wheel=new GameObject("Response wheel");wheel.transform.SetParent(root.transform,false);wheel.transform.localPosition=new Vector3(side*.99f,.36f,z);var wb=new B();
                wb.Cylinder(Vector3.zero,.34f,.20f,M.Black,20,Quaternion.Euler(0,0,90));wb.Cylinder(new Vector3(side*.025f,0,0),.19f,.215f,M.Steel,16,Quaternion.Euler(0,0,90));wb.Attach(wheel,"TyreHub");
            }
            for(int i=0;i<2;i++)
            {
                var beacon=new GameObject(i==0?"BeaconBlue":"BeaconRed");beacon.transform.SetParent(root.transform,false);beacon.transform.localPosition=new Vector3(i==0?-.40f:.40f,truck?1.94f:1.86f,length*.27f);
                var lb=new B();lb.Box(Vector3.zero,new Vector3(.55f,.13f,.30f),M.Linen,.025f);lb.Attach(beacon,"ResponseLightbar");
                Color c=i==0?new Color(.045f,.20f,1):new Color(1,.08f,.025f);var mat=new Material(Shader.Find("Standard")){name="V414 response beacon "+i,color=c};mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",c*2);owned.Owned.Add(mat);
                foreach(var renderer in beacon.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=mat;
            }
            OwnCombinedMeshes(root,owned);return root;
        }
        public static GameObject Responder(Service service,int variant)
        {
            var root=PanicExteriorArt.CreatePedestrianVisual(variant+12);if(!root)return null;root.name="V414 "+service+" responder";
            var owner=root.GetComponent<PanicExteriorOwnedArt>();Color color=service==Service.Police?new Color(.12f,.19f,.31f):service==Service.Fire?new Color(.49f,.26f,.10f):service==Service.Rescue?new Color(.77f,.35f,.08f):new Color(.76f,.77f,.65f);
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {var materials=renderer.sharedMaterials;for(int i=0;i<materials.Length;i++)if(materials[i]&&materials[i].name.Contains("Neighbour canvas")){var clone=new Material(materials[i]){name="V414 response uniform "+service,color=color};owner.Materials.Add(clone);materials[i]=clone;}renderer.sharedMaterials=materials;}
            return root;
        }
        public static GameObject Wreck(DisasterType type)
        {
            var root=CivilianCar(2);root.name="V414 "+type+" affected parked car";PanicHomeArt.EnsurePalette();var b=new B();
            if(type==DisasterType.Flood)
            {for(int side=-1;side<=1;side+=2)b.Box(new Vector3(side*.973f,.52f,0),new Vector3(.018f,.31f,3.9f),M.Earth,.008f);b.Box(new Vector3(0,.015f,.15f),new Vector3(3.0f,.022f,5.2f),M.Earth,.10f);}
            else if(type==DisasterType.Fire)
            {b.Box(new Vector3(0,.80f,-1.35f),new Vector3(1.73f,.035f,1.43f),M.Charcoal,.02f);b.Box(new Vector3(0,1.42f,.12f),new Vector3(1.52f,.035f,1.77f),M.Black,.02f);b.Box(new Vector3(0,.02f,0),new Vector3(2.9f,.022f,5.0f),M.Charcoal,.10f);AddSmoke(root,new Vector3(.4f,1.0f,-1.25f));}
            else if(type==DisasterType.Storm)
            {b.Beam(new Vector3(-1.5f,.12f,-1),new Vector3(.5f,1.30f,.8f),.16f,M.WalnutDark);b.Beam(new Vector3(-.6f,.8f,-.2f),new Vector3(1.15f,1.55f,-.4f),.08f,M.Walnut);b.Box(new Vector3(-.4f,1.6f,.5f),new Vector3(1.15f,.04f,1.35f),M.Slate,.01f,Quaternion.Euler(9,18,21));}
            else
            {for(int i=0;i<8;i++)b.Box(new Vector3(-1.2f+(i%3)*.62f,.11f,1.3f-(i/3)*.55f),new Vector3(.29f,.20f,.24f),i%2==0?M.Brick:M.Concrete,.018f,Quaternion.Euler(i*11,i*43,7));b.Beam(new Vector3(-.65f,1.45f,-.72f),new Vector3(.55f,1.00f,-.95f),.018f,M.Black);}
            b.Attach(root,"TypeMatchedVehicleDamage");OwnCombinedMeshes(root,root.GetComponent<PanicStreetOwnedArtV414>());return root;
        }
        static void OwnCombinedMeshes(GameObject root,PanicStreetOwnedArtV414 owner)
        {
            // HomeArt.Builder creates unique combined meshes; ResidenceArt's HomeV3 meshes are shared caches.
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
                if(filter.sharedMesh&&filter.sharedMesh.name.StartsWith("HomeV2_")&&!owner.Owned.Contains(filter.sharedMesh))owner.Owned.Add(filter.sharedMesh);
        }
        static void AddSmoke(GameObject root,Vector3 p)
        {
            var owner=root.GetComponent<PanicStreetOwnedArtV414>();var texture=new Texture2D(32,32,TextureFormat.RGBA32,false){name="V414 original smoke puff"};var pixels=new Color[32*32];
            for(int y=0;y<32;y++)for(int x=0;x<32;x++){float r=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;pixels[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),1.8f));}texture.SetPixels(pixels);texture.Apply(false,true);owner.Owned.Add(texture);
            var shader=Shader.Find("Panic/SoftWeather");if(!shader)shader=Shader.Find("Sprites/Default");var mat=new Material(shader){name="V414 parked car residual smoke",mainTexture=texture,color=new Color(.13f,.13f,.12f,.58f)};owner.Owned.Add(mat);
            var go=new GameObject("Residual vehicle smoke");go.transform.SetParent(root.transform,false);go.transform.localPosition=p;var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.maxParticles=36;main.startLifetime=new ParticleSystem.MinMaxCurve(2.8f,4.2f);main.startSize=new ParticleSystem.MinMaxCurve(.35f,.6f);main.startSpeed=.30f;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=ps.emission;emission.rateOverTime=7;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=15;shape.radius=.18f;shape.rotation=new Vector3(-90,0,0);
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.x=.12f;velocity.y=.32f;velocity.z=.08f;
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.6f,1,2));
            var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.8f,.18f),new GradientAlphaKey(0,1)});color.color=gradient;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;ps.Play();
        }
    }
    public sealed class PanicStreetOwnedArtV414:MonoBehaviour
    {
        public readonly List<Object> Owned=new List<Object>();
        void OnDestroy(){foreach(var value in Owned)if(value){if(Application.isPlaying)Destroy(value);else DestroyImmediate(value);}Owned.Clear();}
    }
}
