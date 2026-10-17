using UnityEngine;
using UnityEngine.Rendering;
namespace TooLateToPanic
{
    /// <summary>Bounded, cosmetic outdoor wind/rain. No forces, collision, lightning damage or wave logic.</summary>
    public static class PanicStormArt
    {
        public static GameObject Create()
        {
            var root=new GameObject("Storm outside the prepared home");var s=root.AddComponent<PanicStormArtState>();
            s.Soft=new Texture2D(32,32,TextureFormat.RGBA32,false){name="Storm soft mote"};var colors=new Color[1024];
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {float r=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;colors[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),2));}
            s.Soft.SetPixels(colors);s.Soft.Apply(false,false);
            var rain=Material(s,"Storm rain streak",new Color(.62f,.72f,.75f,.70f),s.Soft);
            var haze=Material(s,"Windborne outdoor haze",new Color(.48f,.53f,.50f,.13f),s.Soft);
            var leaves=Material(s,"Windborne leaves",new Color(.43f,.47f,.23f,.8f),null);
            s.Leaf=LeafMesh();
            // Four outside strips leave the entire16x12m interior free of rain emitters.
            for(int i=0;i<4;i++)
            {
                // Include maximum lifetime drift in placement, so particles cannot cross into the cutaway home/garage.
                Vector3 p=i<2?new Vector3(0,13,i==0?15:-18):new Vector3(i==2?-16:29,13,0);
                Vector3 size=i<2?new Vector3(44,.10f,i==0?16:14):new Vector3(i==2?14:8,.10f,12);
                var ps=Emitter(root,"Outside slant rain "+i,rain,p,size,120,86,.85f,1.2f,.012f,.022f);
                var main=ps.main;main.startSpeed=0;main.startColor=new Color(.72f,.80f,.80f,.75f);
                var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=-7;velocity.y=-14;velocity.z=2;
                var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=3.1f;renderer.velocityScale=.04f;
                s.Emitters.Add(ps);s.Rates.Add(86);
            }
            foreach(float x in new[]{-17f,33f})
            {
                var ps=Emitter(root,"Outside blowing leaves",leaves,new Vector3(x,2,0),new Vector3(4,2,24),40,9,2,4,.12f,.28f);
                var main=ps.main;main.startSpeed=.4f;main.startRotation3D=true;main.startRotationX=new ParticleSystem.MinMaxCurve(-Mathf.PI,Mathf.PI);main.startRotationZ=new ParticleSystem.MinMaxCurve(-Mathf.PI,Mathf.PI);
                var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=-3.4f;velocity.y=.3f;velocity.z=1.2f;
                var spin=ps.rotationOverLifetime;spin.enabled=true;spin.x=2.7f;spin.z=3.9f;
                var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Mesh;renderer.mesh=s.Leaf;
                s.Emitters.Add(ps);s.Rates.Add(9);
                var dust=Emitter(root,"Outside passing gust haze",haze,new Vector3(x<0?x:27,.9f,0),new Vector3(3,.3f,22),30,5,3,5,1.1f,2.4f);
                var dv=dust.velocityOverLifetime;dv.enabled=true;dv.x=-1.6f;dv.y=.06f;dv.z=.4f;
                var grow=dust.sizeOverLifetime;grow.enabled=true;grow.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.5f),new Keyframe(1,1.4f)));
                s.Emitters.Add(dust);s.Rates.Add(5);
            }
            // Subtle limited vortex beside the plot, not over the house or main exit.
            for(int i=0;i<3;i++)
            {
                var node=new GameObject("Peripheral gust ribbon "+i);node.transform.SetParent(root.transform,false);
                var r=node.AddComponent<LineRenderer>();r.useWorldSpace=false;r.positionCount=32;r.loop=false;r.sharedMaterial=haze;r.widthMultiplier=.12f;
                r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;s.Ribbons.Add(r);
            }
            foreach(var ps in s.Emitters)ps.Play(true);
            Animate(root,0,0);return root;
        }
        public static void Animate(GameObject root,float time,float strength)
        {
            if(!root)return;var s=root.GetComponent<PanicStormArtState>();if(!s)return;
            strength=Mathf.Clamp01(strength);float gust=(.76f+.24f*Mathf.Sin(time*1.3f))*strength;
            for(int i=0;i<s.Emitters.Count;i++){var emission=s.Emitters[i].emission;emission.rateOverTime=s.Rates[i]*gust;}
            for(int j=0;j<s.Ribbons.Count;j++)
            {
                var r=s.Ribbons[j];r.enabled=strength>.12f;
                r.startColor=new Color(.72f,.77f,.73f,.02f*gust);r.endColor=new Color(.62f,.70f,.65f,.18f*gust);
                for(int i=0;i<32;i++)
                {
                    float q=i/31f,a=q*Mathf.PI*3+time*1.2f+j*2.09f,radius=.85f+q*1.9f;
                    r.SetPosition(i,new Vector3(-18.4f+Mathf.Cos(a)*radius,.8f+q*5.2f,10+Mathf.Sin(a)*radius));
                }
            }
        }
        static ParticleSystem Emitter(GameObject root,string name,Material mat,Vector3 p,Vector3 size,int cap,float rate,float minLife,float maxLife,float minSize,float maxSize)
        {
            var go=new GameObject(name);go.transform.SetParent(root.transform,false);go.transform.localPosition=p;
            var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.loop=true;main.duration=8;main.maxParticles=cap;main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.startLifetime=new ParticleSystem.MinMaxCurve(minLife,maxLife);main.startSize=new ParticleSystem.MinMaxCurve(minSize,maxSize);main.startSpeed=0;
            var shape=ps.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=size;
            var emission=ps.emission;emission.rateOverTime=rate;
            var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.8f,.2f),new GradientAlphaKey(.65f,.72f),new GradientAlphaKey(0,1)});color.color=gradient;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return ps;
        }
        static Material Material(PanicStormArtState s,string name,Color tint,Texture texture)
        {
            var shader=Shader.Find("Panic/SoftWeather");if(!shader)shader=Shader.Find("Sprites/Default");
            var m=new Material(shader){name=name,color=tint};if(texture)m.mainTexture=texture;
            // Both retained shaders have fixed alpha blending; no stripped runtime variant.
            m.renderQueue=3000;s.Materials.Add(m);return m;
        }
        static Mesh LeafMesh()
        {
            var m=new Mesh{name="Wind leaf diamond"};m.vertices=new[]{new Vector3(-.5f,0,0),new Vector3(0,.13f,.9f),new Vector3(.5f,0,0),new Vector3(0,-.08f,-.8f)};m.triangles=new[]{0,1,2,0,2,3,2,1,0,3,2,0};m.RecalculateNormals();m.RecalculateBounds();return m;
        }
    }
}
