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
                Vector3 p=i<2?new Vector3(0,13,i==0?15:-18):new Vector3(i==2?-16:32,13,0);
                Vector3 size=i<2?new Vector3(44,.10f,i==0?16:14):new Vector3(i==2?14:8,.10f,12);
                var ps=Emitter(root,"Outside slant rain "+i,rain,p,size,120,86,.85f,1.2f,.012f,.022f);
                var main=ps.main;main.startSpeed=0;main.startColor=new Color(.72f,.80f,.80f,.75f);
                var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=-7;velocity.y=-14;velocity.z=2;
                var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=3.1f;renderer.velocityScale=.04f;
                s.Emitters.Add(ps);s.Rates.Add(86);
            }
            foreach(float x in new[]{-17f,38f})
            {
                var ps=Emitter(root,"Outside blowing leaves",leaves,new Vector3(x,2,0),new Vector3(4,2,24),40,9,2,4,.12f,.28f);
                var main=ps.main;main.startSpeed=.4f;main.startRotation3D=true;main.startRotationX=new ParticleSystem.MinMaxCurve(-Mathf.PI,Mathf.PI);main.startRotationZ=new ParticleSystem.MinMaxCurve(-Mathf.PI,Mathf.PI);
                var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=-3.4f;velocity.y=.3f;velocity.z=1.2f;
                var spin=ps.rotationOverLifetime;spin.enabled=true;spin.x=2.7f;spin.z=3.9f;
                var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Mesh;renderer.mesh=s.Leaf;
                s.Emitters.Add(ps);s.Rates.Add(9);
                var dust=Emitter(root,"Outside passing gust haze",haze,new Vector3(x<0?x:30,.9f,0),new Vector3(3,.3f,22),30,5,3,5,1.1f,2.4f);
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
            CreateFunnelV414(root,s,Material(s,"V414 rotating storm column",Color.white,null),Material(s,"Storm lifted timber and leaves",Color.white,null));
            foreach(var ps in s.Emitters)ps.Play(true);
            Animate(root,0,0);return root;
        }
        public static void Animate(GameObject root,float time,float strength)
        {Animate(root,time,strength,1f);}

        public static void Animate(GameObject root,float time,float strength,float presentationScale)
        {
            if(!root)return;var s=root.GetComponent<PanicStormArtState>();if(!s)return;
            presentationScale=PanicDisasterArt.ClampPresentationScaleV52(presentationScale);
            strength=Mathf.Clamp01(strength);var motion=root.GetComponent<PanicStormMotionV414>();
            float dt=motion?time-motion.LastAge:0;if(dt<0||dt>.25f)dt=Mathf.Clamp(Time.deltaTime,0,.1f);
            if(motion){motion.LastAge=time;motion.Clock+=Mathf.Max(0,dt);time=motion.Clock;}
            float beat=Mathf.Pow(.5f+.5f*Mathf.Sin(time*1.08f+Mathf.Sin(time*.37f)*1.7f),2);
            float gust=(.42f+.58f*beat)*strength,visualGust=gust*presentationScale;
            if(motion){motion.PresentationScaleV52=presentationScale;motion.Gust=visualGust;motion.EmissionRateV52=0;AnimateFunnelV414(motion,strength);}
            for(int i=0;i<s.Emitters.Count;i++)
            {
                var ps=s.Emitters[i];var emission=ps.emission;float rate=s.Rates[i]*visualGust;emission.rateOverTime=rate;
                if(motion)motion.EmissionRateV52+=emission.rateOverTime.constant;
                float sizeScale=1f+.35f*(presentationScale-1f);var main=ps.main;
                main.startSize=i<4?new ParticleSystem.MinMaxCurve(.012f*sizeScale,.022f*sizeScale):ps.name.Contains("leaves")?new ParticleSystem.MinMaxCurve(.12f*sizeScale,.28f*sizeScale):new ParticleSystem.MinMaxCurve(1.1f*sizeScale,2.4f*sizeScale);
                var velocity=ps.velocityOverLifetime;
                // Keep maximum lifetime lateral drift unchanged: outside strips remain outside the house/garage.
                if(i<4){velocity.x=-5.5f-4.0f*gust;velocity.y=-14;velocity.z=.6f+1.5f*gust;}
                else if(ps.name.Contains("leaves")){velocity.x=-3.0f-1.8f*gust;velocity.y=.18f+.3f*gust;velocity.z=.7f+.5f*gust;}
                else {velocity.x=-1.2f-.8f*gust;velocity.z=.25f+.3f*gust;}
            }
            for(int j=0;j<s.Ribbons.Count;j++)
            {
                var r=s.Ribbons[j];r.enabled=strength>.12f;
                r.widthMultiplier=.12f*(1f+.35f*(presentationScale-1f));
                r.startColor=new Color(.72f,.77f,.73f,.06f*visualGust);r.endColor=new Color(.62f,.70f,.65f,.24f*visualGust);
                for(int i=0;i<32;i++)
                {
                    float q=i/31f,a=q*Mathf.PI*3+time*1.2f+j*2.09f,radius=.85f+q*1.9f;
                    r.SetPosition(i,new Vector3(-18.4f+Mathf.Cos(a)*radius,.8f+q*5.2f,10+Mathf.Sin(a)*radius));
                }
            }
        }
                public static Vector3 SampleWind(GameObject root)
        {var state=root?root.GetComponent<PanicStormMotionV414>():null;return state?new Vector3(-1,0,.27f)*state.Gust:Vector3.zero;}
        static void CreateFunnelV414(GameObject root,PanicStormArtState owner,Material haze,Material leaves)
        {
            var s=root.AddComponent<PanicStormMotionV414>();
            s.Funnel=new Mesh{name="Coherent rotating tapered storm funnel"};
            s.Vertices=new Vector3[25*19];s.Colors=new Color[s.Vertices.Length];var uv=new Vector2[s.Vertices.Length];var triangles=new int[24*18*6];int write=0;
            for(int y=0;y<25;y++)for(int a=0;a<19;a++)
            {int v=y*19+a;uv[v]=new Vector2(a/18f,y/24f);if(y<24&&a<18){triangles[write++]=v;triangles[write++]=v+19;triangles[write++]=v+1;triangles[write++]=v+1;triangles[write++]=v+19;triangles[write++]=v+20;}}
            s.Funnel.vertices=s.Vertices;s.Funnel.triangles=triangles;s.Funnel.uv=uv;s.Funnel.colors=s.Colors;s.Funnel.MarkDynamic();s.Funnel.bounds=new Bounds(new Vector3(-19.2f,6,11),new Vector3(10,16,10));
            var go=new GameObject("Peripheral rotating funnel");go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=s.Funnel;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=haze;r.shadowCastingMode=ShadowCastingMode.Off;
            s.CloudBase=FunnelMassV414(root,"Attached rotating storm cloud base",haze,s.Funnel.bounds,out s.CloudVertices,out s.CloudColors);
            s.GroundDust=FunnelMassV414(root,"Ground contact scouring dust",haze,s.Funnel.bounds,out s.DustVertices,out s.DustColors);
            s.Debris=new Mesh{name="Storm rotating coherent leaf and panel debris"};s.DebrisVertices=new Vector3[24*4];var tr=new int[24*6];var duv=new Vector2[24*4];var dc=new Color[24*4];
            for(int i=0;i<24;i++)
            {int v=i*4,t=i*6;tr[t]=v;tr[t+1]=v+1;tr[t+2]=v+2;tr[t+3]=v;tr[t+4]=v+2;tr[t+5]=v+3;duv[v]=Vector2.zero;duv[v+1]=Vector2.up;duv[v+2]=Vector2.one;duv[v+3]=Vector2.right;for(int k=0;k<4;k++)dc[v+k]=i%5==0?new Color(.63f,.55f,.41f,.94f):new Color(.42f,.47f,.29f,.90f);}
            s.Debris.vertices=s.DebrisVertices;s.Debris.triangles=tr;s.Debris.uv=duv;s.Debris.colors=dc;s.Debris.MarkDynamic();s.Debris.bounds=s.Funnel.bounds;
            go=new GameObject("Orbiting outdoor leaf and roof-sheet fragments");go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=s.Debris;r=go.AddComponent<MeshRenderer>();r.sharedMaterial=leaves;r.shadowCastingMode=ShadowCastingMode.Off;
        }
        static Mesh FunnelMassV414(GameObject root,string name,Material material,Bounds bounds,out Vector3[] vertices,out Color[] colors)
        {
            // Both cosmetic masses remain inside the existing 10x16x10m funnel bounds.
            vertices=new Vector3[9*25];colors=new Color[vertices.Length];var uv=new Vector2[vertices.Length];var triangles=new int[8*24*6];int write=0;
            for(int y=0;y<9;y++)for(int a=0;a<25;a++)
            {
                int v=y*25+a;uv[v]=new Vector2(a/24f,y/8f);
                if(y<8&&a<24){triangles[write++]=v;triangles[write++]=v+25;triangles[write++]=v+1;triangles[write++]=v+1;triangles[write++]=v+25;triangles[write++]=v+26;}
            }
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.colors=colors;mesh.triangles=triangles;mesh.uv=uv;mesh.MarkDynamic();mesh.bounds=bounds;
            var node=new GameObject(name);node.transform.SetParent(root.transform,false);node.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=node.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            return mesh;
        }
        static void AnimateFunnelV414(PanicStormMotionV414 s,float strength)
        {
            float time=s.Clock*(1f+.35f*(s.PresentationScaleV52-1f));
            float visualStrength=strength*s.PresentationScaleV52;
            for(int y=0;y<25;y++)for(int a=0;a<19;a++)
            {
                float q=y/24f,angle=a/18f*Mathf.PI*2+time*(2.4f-q*.8f)+q*3.8f;
                float radius=(.72f+Mathf.Pow(q,1.6f)*2.95f)*(1+.07f*Mathf.Sin(angle*3+q*17-time*3));
                int v=y*19+a;float bend=q*q*.45f*Mathf.Sin(time*.28f);
                s.Vertices[v]=new Vector3(-19.2f+bend+Mathf.Cos(angle)*radius,q*11.5f,11+Mathf.Sin(angle)*radius);
                float band=.5f+.5f*Mathf.Sin(angle*3+q*21-time*1.8f);
                float alpha=(.42f+.18f*band)*Mathf.Lerp(.68f,1,Mathf.Sin(q*Mathf.PI))*visualStrength;
                s.Colors[v]=new Color(.22f+.10f*band,.25f+.10f*band,.27f+.10f*band,alpha);
            }
            s.Funnel.vertices=s.Vertices;s.Funnel.colors=s.Colors;
            for(int y=0;y<9;y++)for(int a=0;a<25;a++)
            {
                int v=y*25+a;float q=y/8f,angle=a/24f*Mathf.PI*2+time*.52f;
                float billow=1+.035f*Mathf.Sin(angle*5+q*11-time*.7f);
                float radius=Mathf.Sin(q*Mathf.PI)*4.35f*billow;
                float bend=.25f*Mathf.Sin(time*.28f);
                s.CloudVertices[v]=new Vector3(-19.2f+bend+Mathf.Cos(angle)*radius,11.15f+q*2.55f,11+Mathf.Sin(angle)*radius);
                float shade=.5f+.5f*Mathf.Sin(angle*3+q*9-time*.6f);
                s.CloudColors[v]=new Color(.20f+.08f*shade,.23f+.08f*shade,.25f+.08f*shade,(.46f+.14f*shade)*visualStrength);
                angle=a/24f*Mathf.PI*2-time*2.1f+q*2.3f;
                radius=(.68f+Mathf.Sin(q*Mathf.PI)*2.55f)*(1+.07f*Mathf.Sin(angle*4+time));
                s.DustVertices[v]=new Vector3(-19.2f+Mathf.Cos(angle)*radius,.055f+q*1.85f,11+Mathf.Sin(angle)*radius);
                float edge=Mathf.Sin(q*Mathf.PI);
                s.DustColors[v]=new Color(.46f,.42f,.34f,(.18f+.28f*edge)*edge*visualStrength);
            }
            s.CloudBase.vertices=s.CloudVertices;s.CloudBase.colors=s.CloudColors;
            s.GroundDust.vertices=s.DustVertices;s.GroundDust.colors=s.DustColors;
            for(int i=0;i<24;i++)
            {
                float q=Mathf.Repeat(i/24f+time*.045f,.94f)+.025f,angle=i*2.399f+time*(3-q),radius=.8f+Mathf.Pow(q,1.7f)*3.05f;
                Vector3 center=new Vector3(-19.2f+Mathf.Sin(time*.28f)*q*q*.45f+Mathf.Cos(angle)*radius,q*10.8f,11+Mathf.Sin(angle)*radius);
                Quaternion rotate=Quaternion.Euler(time*83+i*17,time*127+i*53,time*61);
                float size=(i%5==0?.46f:.20f)*strength*(1f+.2f*(s.PresentationScaleV52-1f));int v=i*4;
                s.DebrisVertices[v]=center+rotate*new Vector3(-size,0,-size*.5f);s.DebrisVertices[v+1]=center+rotate*new Vector3(-size,.03f,size*.5f);s.DebrisVertices[v+2]=center+rotate*new Vector3(size,0,size*.5f);s.DebrisVertices[v+3]=center+rotate*new Vector3(size,-.02f,-size*.5f);
            }
            s.Debris.vertices=s.DebrisVertices;
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
    public sealed class PanicStormMotionV414:MonoBehaviour
    {
        public Mesh Funnel,Debris,CloudBase,GroundDust;public Vector3[] Vertices,DebrisVertices,CloudVertices,DustVertices;public Color[] Colors,CloudColors,DustColors;
        public float Clock,LastAge,Gust;
        public float PresentationScaleV52=1f,EmissionRateV52;
        void OnDestroy(){if(Funnel)Destroy(Funnel);if(Debris)Destroy(Debris);if(CloudBase)Destroy(CloudBase);if(GroundDust)Destroy(GroundDust);}
    }
}
