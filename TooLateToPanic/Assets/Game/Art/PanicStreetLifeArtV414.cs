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
        public enum VehicleModelFront { NegativeZ,PositiveZ }
        // ResidenceArt.Car: hood/headlights at -Z, red tail lamps at +Z.
        // All four response factories: cab/headlights at +Z, tail lamps at -Z.
        public const VehicleModelFront CivilianModelFront=VehicleModelFront.NegativeZ;
        public const VehicleModelFront EmergencyModelFront=VehicleModelFront.PositiveZ;
        public static Vector3 LocalVehicleFront(VehicleModelFront modelFront)
        {return modelFront==VehicleModelFront.NegativeZ?Vector3.back:Vector3.forward;}
        public static Quaternion VehicleHeading(Vector3 travelDirection,VehicleModelFront modelFront)
        {
            travelDirection.y=0;
            if(travelDirection.sqrMagnitude<.000001f)return Quaternion.identity;
            // LookRotation owns local +Z; align the authored front, not an assumed +Z bonnet.
            return Quaternion.LookRotation(modelFront==VehicleModelFront.NegativeZ?-travelDirection:travelDirection,Vector3.up);
        }
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
        // V5 replaces the former response proxy shapes with detailed authored service assets.
        public static GameObject EmergencyVehicle(Service service)
        { return PanicV5ResponseArt.CreateVehicle(service); }
        public static GameObject Responder(Service service,int variant)
        { return PanicV5ResponseArt.CreateResponder(service,variant); }
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
