using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    // Original Home V3 art. Host owns placement, camera, gameplay, AudioSources and spotlight.
    // Unity units are metres; Y up, rescue helicopter nose +Z, tail -Z, skid ground Y=0.
    public static class PanicAtmosphereArt
    {
        static readonly Dictionary<string, AudioClip> Sounds = new Dictionary<string, AudioClip>();

        public static GameObject CreateHelicopter()
        {
            var s = NewState("Mahalle rescue helicopter");
            var b = new Batch(s, s.transform);
            var ivory = Mat(s, "Warm rescue ivory", new Color(.82f, .83f, .78f), .46f, .22f);
            var orange = Mat(s, "Rescue vermilion", new Color(.85f, .18f, .065f), .42f, .18f);
            var dark = Mat(s, "Graphite rotor and seals", new Color(.055f, .065f, .072f), .34f, .4f);
            var steel = Mat(s, "Brushed aircraft metal", new Color(.38f, .43f, .44f), .66f, .82f);
            var glass = Mat(s, "Smoky blue cockpit glazing", new Color(.045f, .15f, .19f), .92f, .48f);
            var rubber = Mat(s, "Skid rubber pads", new Color(.035f, .04f, .041f), .1f);
            // Watertight ring-lofted fuselage, with a round nose and tapered tail attachment.
            b.Loft(new[] {
                new Vector3(2.10f,.17f,.28f), new Vector3(1.72f,.68f,.63f),
                new Vector3(.92f,.91f,.85f), new Vector3(-.35f,.93f,.88f),
                new Vector3(-1.35f,.61f,.65f), new Vector3(-1.86f,.22f,.33f)
            }, 1.43f, ivory);
            b.Ellipsoid(new Vector3(0, 1.02f, .05f), new Vector3(.94f, .32f, 1.72f), orange);
            b.Ellipsoid(new Vector3(0, 2.16f, -.45f), new Vector3(.62f, .38f, .92f), ivory);
            b.Box(new Vector3(0, 2.2f, -.5f), new Vector3(1.1f, .16f, 1.2f), orange);
            // Split windshield panels conform to the sloped upper nose.
            b.Quad(new Vector3(.06f,1.48f,2.05f), new Vector3(.72f,1.48f,1.7f), new Vector3(.66f,2.04f,1.15f), new Vector3(.06f,2.15f,1.22f), glass);
            b.Quad(new Vector3(-.72f,1.48f,1.7f), new Vector3(-.06f,1.48f,2.05f), new Vector3(-.06f,2.15f,1.22f), new Vector3(-.66f,2.04f,1.15f), glass);
            b.Beam(new Vector3(0,1.48f,2.08f), new Vector3(0,2.16f,1.22f), .055f, ivory);
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .91f;
                b.Quad(new Vector3(x,1.47f,.5f),new Vector3(x,1.47f,1.12f),new Vector3(x*.79f,2.05f,1.13f),new Vector3(x*.94f,2.11f,.5f),glass);
                b.Box(new Vector3(x*1.015f,1.43f,-.48f),new Vector3(.035f,1.2f,1.16f),dark);
                b.Box(new Vector3(x*1.04f,1.43f,-.48f),new Vector3(.04f,1.12f,1.08f),ivory);
                b.Box(new Vector3(x*1.07f,1.78f,-.48f),new Vector3(.035f,.39f,.84f),glass);
                b.Box(new Vector3(x*1.08f,1.08f,-.48f),new Vector3(.04f,.23f,1.04f),orange);
                b.Beam(new Vector3(x*1.12f,1.46f,-.86f),new Vector3(x*1.12f,1.46f,-.65f),.038f,steel);
                b.Beam(new Vector3(x*1.1f,2.12f,-1.03f),new Vector3(x*1.1f,2.12f,.25f),.025f,steel);
                b.Beam(new Vector3(x*.6f,.96f,-.95f),new Vector3(side*1.12f,.2f,-.95f),.075f,steel);
                b.Beam(new Vector3(x*.6f,.96f,.98f),new Vector3(side*1.12f,.2f,.98f),.075f,steel);
                b.Beam(new Vector3(side*1.12f,.15f,-1.72f),new Vector3(side*1.12f,.15f,1.67f),.085f,dark);
                b.Beam(new Vector3(side*1.12f,.15f,1.67f),new Vector3(side*1.12f,.36f,1.97f),.085f,dark);
                b.Box(new Vector3(side*1.12f,.085f,-1.16f),new Vector3(.13f,.06f,.48f),rubber);
                b.Box(new Vector3(side*1.12f,.085f,.96f),new Vector3(.13f,.06f,.48f),rubber);
                // Original rescue markings: high contrast cross in orange square, no agency logo.
                b.Box(new Vector3(x*1.09f,1.34f,-.15f),new Vector3(.015f,.29f,.29f),orange);
                b.Box(new Vector3(x*1.11f,1.34f,-.15f),new Vector3(.018f,.20f,.06f),ivory);
                b.Box(new Vector3(x*1.12f,1.34f,-.15f),new Vector3(.019f,.06f,.20f),ivory);
                b.Box(new Vector3(side*.59f,2.31f,-.63f),new Vector3(.08f,.24f,.55f),dark);
                for (int vent = 0; vent < 5; vent++) b.Box(new Vector3(side*.64f,2.3f,-.84f+vent*.1f),new Vector3(.03f,.17f,.032f),steel);
            }
            // Tapered boom, fin, horizontal stabilizer, antennae, exhaust and rescue hoist.
            b.Tube(new Vector3(0,1.65f,-1.42f),new Vector3(0,2.15f,-5.2f),.29f,.095f,ivory);
            b.Tube(new Vector3(0,1.65f,-1.42f),new Vector3(0,1.85f,-2.65f),.30f,.22f,orange);
            b.Quad(new Vector3(0,2.0f,-4.7f),new Vector3(0,2.12f,-5.5f),new Vector3(0,3.36f,-5.23f),new Vector3(0,3.42f,-4.95f),orange);
            b.Box(new Vector3(0,2.2f,-4.35f),new Vector3(2.16f,.07f,.48f),ivory);
            b.Box(new Vector3(-.95f,2.23f,-4.35f),new Vector3(.25f,.075f,.49f),orange);
            b.Box(new Vector3(.95f,2.23f,-4.35f),new Vector3(.25f,.075f,.49f),orange);
            b.Tube(new Vector3(.36f,2.2f,-1.0f),new Vector3(.36f,2.3f,-1.54f),.15f,.13f,steel);
            b.Tube(new Vector3(.36f,2.3f,-1.54f),new Vector3(.36f,2.31f,-1.57f),.11f,.11f,dark);
            b.Beam(new Vector3(0,2.5f,.5f),new Vector3(0,3.05f,.39f),.022f,dark);
            b.Beam(new Vector3(-.83f,2.05f,-.15f),new Vector3(-1.38f,2.07f,-.15f),.065f,steel);
            b.Ellipsoid(new Vector3(-1.38f,2.05f,-.15f),new Vector3(.14f,.16f,.17f),orange);
            b.Beam(new Vector3(-1.38f,2.0f,-.15f),new Vector3(-1.38f,1.15f,-.15f),.015f,steel);
            b.Tube(new Vector3(-1.38f,1.15f,-.15f),new Vector3(-1.38f,1.05f,-.15f),.055f,.035f,steel);
            b.Ellipsoid(new Vector3(0,.93f,1.53f),new Vector3(.2f,.19f,.2f),dark);
            b.Ellipsoid(new Vector3(0,.88f,1.68f),new Vector3(.12f,.10f,.07f),Mat(s,"Searchlight lens",new Color(.9f,.83f,.62f),.85f,.2f,new Color(.8f,.7f,.43f)));
            b.Flush();
            var mast = new Batch(s,s.transform); mast.Tube(new Vector3(0,2.3f,-.15f),new Vector3(0,2.98f,-.15f),.09f,.09f,steel); mast.Flush();
            s.MainRotor = Group(s.transform,"MainRotor",new Vector3(0,2.98f,-.15f));
            var rotor = new Batch(s,s.MainRotor);
            rotor.Ellipsoid(Vector3.zero,new Vector3(.28f,.13f,.28f),steel);
            for(int i=0;i<4;i++)
            {
                Quaternion q=Quaternion.Euler(0,i*90,0);
                rotor.Box(q*new Vector3(0,0,2.0f),new Vector3(.24f,.035f,3.36f),dark,q);
                rotor.Box(q*new Vector3(0,.015f,3.48f),new Vector3(.25f,.022f,.28f),orange,q);
            }
            rotor.Flush();
            s.TailRotor = Group(s.transform,"TailRotor",new Vector3(.17f,2.48f,-5.07f));
            var tail = new Batch(s,s.TailRotor);
            for(int i=0;i<3;i++) tail.Box(Quaternion.Euler(i*120,0,0)*new Vector3(0,.38f,0),new Vector3(.04f,.65f,.11f),dark,Quaternion.Euler(i*120,0,0));
            tail.Ellipsoid(Vector3.zero,new Vector3(.1f,.11f,.11f),orange); tail.Flush();
            RotorHaze(s,s.MainRotor,3.64f,false); RotorHaze(s,s.TailRotor,.74f,true);
            Marker(s,"Port red",new Vector3(-1.02f,1.75f,-1.0f),new Color(1,.055f,.025f),0);
            Marker(s,"Starboard green",new Vector3(1.02f,1.75f,-1.0f),new Color(.08f,1,.25f),.5f);
            Marker(s,"Upper beacon",new Vector3(0,2.58f,-.88f),new Color(1,.08f,.025f),.15f);
            Group(s.transform,"SearchlightSocket",new Vector3(0,.86f,1.63f));
            Group(s.transform,"RotorAudioSocket",new Vector3(0,2.75f,-.15f));
            AnimateHelicopter(s.gameObject,0); return s.gameObject;
        }

        public static void AnimateHelicopter(GameObject helicopter,float time)
        {
            if(!helicopter) return; var s=helicopter.GetComponent<PanicAtmosphereArtState>(); if(!s) return;
            if(s.MainRotor) s.MainRotor.localRotation=Quaternion.Euler(0,time*1680f,0);
            if(s.TailRotor) s.TailRotor.localRotation=Quaternion.Euler(time*-4560f,0,0);
            for(int i=0;i<s.Markers.Count;i++)
            {
                var m=s.Markers[i]; float flash=Mathf.Repeat(time*1.6f+m.Phase,1)<.14f?1:.18f;
                m.Material.SetColor("_EmissionColor",m.Color*flash*2.8f);
            }
        }

        public static GameObject CreateFireplaceFlames()
        {
            var s=NewState("Soft hearth flames"); var b=new Batch(s,s.transform);
            var charcoal=Mat(s,"Hearth charcoal",new Color(.06f,.035f,.022f),.1f);
            var glow=Mat(s,"Hearth orange embers",new Color(.22f,.045f,.009f),.1f,0,new Color(.7f,.09f,.008f));
            for(int i=0;i<5;i++)
            {
                float z=(i-2)*.095f;
                b.Tube(new Vector3(-.33f,.09f,z),new Vector3(.33f,.12f,z+.07f),.075f,.06f,charcoal);
                b.Ellipsoid(new Vector3((H(i+40)-.5f)*.5f,.14f,z),new Vector3(.09f,.035f,.04f),glow);
            }
            b.Flush();
            var fire=SoftMat(s,"Soft tapered hearth flame",true,new Color(1,.57f,.12f,.75f));
            var core=SoftMat(s,"Warm hearth core",true,new Color(1,.86f,.42f,.8f));
            var ember=SoftMat(s,"Tiny hearth sparks",false,new Color(1,.44f,.06f,.75f));
            var p=Emitter(s,"Slow rolling orange tongues",fire,new Vector3(0,.12f,0),40,26,.48f,.95f,.13f,.24f);
            FlameEmitter(p,.35f,.8f,.7f);
            var c=Emitter(s,"Short pale flame hearts",core,new Vector3(0,.13f,.04f),20,15,.23f,.46f,.08f,.14f);
            FlameEmitter(c,.19f,.45f,.5f);
            var sparks=Emitter(s,"Quiet sparse embers",ember,new Vector3(0,.17f,0),16,2.8f,.6f,1.1f,.012f,.025f);
            var sparkMain=sparks.main; sparkMain.startSpeed=.8f; var gravity=sparks.main; gravity.gravityModifier=-.035f;
            var lightGo=new GameObject("Hearth warm local light"); lightGo.transform.SetParent(s.transform,false); lightGo.transform.localPosition=new Vector3(0,.42f,.08f);
            s.HearthLight=lightGo.AddComponent<Light>(); s.HearthLight.type=LightType.Point; s.HearthLight.range=3.5f; s.HearthLight.color=new Color(1,.48f,.15f); s.HearthLight.intensity=.9f; s.HearthLight.shadows=LightShadows.None;
            return s.gameObject;
        }

        public static void AnimateFireplace(GameObject flames,float time)
        {
            if(!flames) return; var s=flames.GetComponent<PanicAtmosphereArtState>(); if(!s) return;
            if(s.HearthLight) s.HearthLight.intensity=.78f+.12f*Mathf.Sin(time*8.6f)+.08f*Mathf.Sin(time*17.1f);
        }

        public static GameObject CreateAftermath(DisasterType type)
        {
            var s=NewState("Settled aftermath - "+type); s.Type=type; var b=new Batch(s,s.transform);
            var stone=Mat(s,"Broken warm plaster",new Color(.38f,.35f,.30f),.12f);
            var brick=Mat(s,"Fractured terracotta",new Color(.34f,.18f,.12f),.14f);
            var wood=Mat(s,"Splintered wood",new Color(.25f,.17f,.09f),.16f);
            var soot=Mat(s,"Dry ash and charcoal",new Color(.065f,.065f,.06f),.07f);
            var mud=Mat(s,"Wet mud",new Color(.14f,.14f,.10f),.58f);
            var water=Mat(s,"Shallow settled puddles",new Color(.13f,.23f,.24f),.94f,.24f);
            var leaves=Mat(s,"Wind scattered leaves",new Color(.27f,.31f,.14f),.12f);
            int count=type==DisasterType.Earthquake?62:34;
            for(int i=0;i<count;i++)
            {
                Vector3 p=DebrisPoint(i); float size=.09f+H(i+19)*.19f;
                if(type==DisasterType.Earthquake)
                {
                    b.Ellipsoid(p+Vector3.up*size*.24f,new Vector3(size,.04f+size*.23f,size*.7f),i%3==0?brick:stone);
                    if(i%5==0) b.Box(p+Vector3.up*.07f,new Vector3(.07f,.055f,.48f+H(i+90)*.4f),wood,Quaternion.Euler(7,H(i+5)*360,11));
                }
                else if(type==DisasterType.Flood)
                {
                    Patch(b,p+Vector3.up*.017f,.36f+H(i+3)*.6f,.18f+H(i+7)*.28f,mud,i);
                    if(i%2==0) Patch(b,p+Vector3.up*.026f,.25f+H(i+16)*.8f,.2f+H(i+23)*.5f,water,i+51);
                    if(i%5==0) b.Box(p+Vector3.up*.075f,new Vector3(.1f,.06f,.52f),wood,Quaternion.Euler(4,H(i+19)*360,4));
                }
                else if(type==DisasterType.Storm)
                {
                    b.Box(p+Vector3.up*.045f,new Vector3(.27f,.018f,.1f),leaves,Quaternion.Euler(4,H(i+18)*360,7));
                    if(i%3==0)b.Box(p+Vector3.up*.07f,new Vector3(.52f,.035f,.085f),wood,Quaternion.Euler(5,H(i+5)*360,3));
                    if(i%7==0)b.Ellipsoid(p+Vector3.up*.04f,new Vector3(.22f,.045f,.16f),brick);
                }
                else
                {
                    Patch(b,p+Vector3.up*.023f,.3f+H(i+16)*.55f,.18f+H(i+23)*.4f,soot,i);
                    b.Tube(p+new Vector3(-.16f,.06f,-.07f),p+new Vector3(.19f,.08f,.09f),.055f,.035f,soot);
                    if(i%3==0) b.Ellipsoid(p+Vector3.up*.08f,new Vector3(size,.06f,size*.6f),brick);
                }
            }
            if(type==DisasterType.Earthquake)
            {
                for(int j=0;j<9;j++)
                {
                    Vector3 last=DebrisPoint(j+111)+Vector3.up*.027f;
                    for(int i=0;i<7;i++)
                    {
                        Vector3 next=last+new Vector3((H(j*19+i+80)-.5f)*.45f,0,.16f+H(j*11+i+240)*.2f);
                        Vector3 cross=Vector3.Cross((next-last).normalized,Vector3.up)*(.008f+H(i+j)*.012f);
                        b.Quad(last-cross,last+cross,next+cross,next-cross,soot); last=next;
                    }
                }
            }
            b.Flush();
            var mist=SoftMat(s,type==DisasterType.Fire?"Slow translucent residual smoke":"Thin settled aftermath haze",false,
                type==DisasterType.Fire?new Color(.22f,.23f,.22f,.20f):new Color(.56f,.60f,.58f,.105f));
            int plumes=type==DisasterType.Fire?7:4;
            for(int i=0;i<plumes;i++)
            {
                Vector3 p=DebrisPoint(i+207); p.y=.11f;
                var ps=Emitter(s,"Residual plume "+i,mist,p,20,type==DisasterType.Fire?1.1f:.65f,3,5,.45f,.9f);
                var main=ps.main; main.startSpeed=type==DisasterType.Fire?.18f:.065f; main.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
                var shape=ps.shape; shape.shapeType=ParticleSystemShapeType.Circle; shape.radius=.2f; shape.rotation=new Vector3(90,0,0);
                var vel=ps.velocityOverLifetime; vel.enabled=true; vel.space=ParticleSystemSimulationSpace.Local;
                vel.x=.035f; vel.y=type==DisasterType.Fire?.2f:.08f; vel.z=-.025f;
                var grow=ps.sizeOverLifetime; grow.enabled=true; grow.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.45f),new Keyframe(1,1.7f)));
            }
            if(type==DisasterType.Flood)
            {
                var foam=SoftMat(s,"Quiet water ripple rings",false,new Color(.60f,.69f,.64f,.18f));
                for(int i=0;i<6;i++)
                {
                    var go=Group(s.transform,"Slow ripple "+i,DebrisPoint(i+34)+Vector3.up*.038f); var r=new Batch(s,go);
                    Ring(r,.22f+H(i+4)*.3f,.012f,foam); r.Flush();
                    s.Floating.Add(new PanicAtmosphereArtState.FloatPart { Transform=go,Origin=go.localPosition,Phase=i*.81f,Ripple=true });
                }
            }
            AnimateAftermath(s.gameObject,0); return s.gameObject;
        }

        public static void AnimateAftermath(GameObject aftermath,float time)
        {
            if(!aftermath) return; var s=aftermath.GetComponent<PanicAtmosphereArtState>(); if(!s) return;
            for(int i=0;i<s.Floating.Count;i++)
            {
                var p=s.Floating[i]; if(!p.Transform) continue;
                float cycle=Mathf.Repeat(time*.17f+p.Phase,1);
                p.Transform.localPosition=p.Origin+Vector3.up*(Mathf.Sin(time*.8f+p.Phase)*.002f);
                p.Transform.localScale=Vector3.one*(.76f+cycle*.8f);
            }
        }

        // Voice files should be copied to Assets/.../Resources/Panic/Radio/<id>.wav.
        // Audio is cached, mono, and has no realtime/network dependency. Runtime sources belong to host.
        public static AudioClip GetSound(string id)
        {
            if(string.IsNullOrEmpty(id)) return null; id=id.ToLowerInvariant();
            AudioClip clip; if(Sounds.TryGetValue(id,out clip)&&clip) return clip;
            string voiceId=id.StartsWith("radio_")?id.Substring(6):id;
            if(voiceId=="news"||voiceId=="earthquake"||voiceId=="flood"||voiceId=="fire"||voiceId=="aftermath"||voiceId=="deadline")
            {
                clip=Resources.Load<AudioClip>("Panic/Radio/"+voiceId); if(clip) { clip.LoadAudioData(); Sounds[id]=clip; } return clip;
            }
            const int rate=22050; float seconds=id=="rotor"?4:id=="radio_static"?3:id=="radio_tune"?.75f:.48f;
            if(id!="rotor"&&id!="radio_static"&&id!="radio_tune"&&id!="radio_alert"&&id!="fireplace") return null;
            if(id=="fireplace") seconds=4;
            var data=new float[Mathf.CeilToInt(seconds*rate)]; uint seed=0x2653ab4u; float smooth=0,prev=0;
            for(int i=0;i<data.Length;i++)
            {
                float t=(float)i/rate; seed=seed*1664525u+1013904223u; float noise=((seed>>8)&65535)/32767.5f-1;
                smooth=Mathf.Lerp(smooth,noise,id=="rotor"?.075f:.22f); float value;
                if(id=="rotor")
                {
                    float chop=.35f+.65f*Mathf.Pow(.5f+.5f*Mathf.Sin(t*Mathf.PI*44),3);
                    value=chop*(.24f*Mathf.Sin(t*Mathf.PI*132)+.12f*Mathf.Sin(t*Mathf.PI*220)+.07f*Mathf.Sin(t*Mathf.PI*352)+smooth*.25f);
                }
                else if(id=="radio_static") value=(noise-prev*.72f)*.055f;
                else if(id=="radio_tune") value=(Mathf.Sin(t*Mathf.PI*(820+1500*t))+noise*.9f)*.12f*(1-t/seconds);
                else if(id=="fireplace") value=smooth*.12f+Mathf.Pow(Mathf.Max(0,noise-.94f)*16,2)*.10f;
                else value=Mathf.Sin(t*Mathf.PI*(t<.19f?1046.5f:783.99f))*.14f;
                float edge=Mathf.Min(1,Mathf.Min(t,seconds-t)/.018f);
                data[i]=Mathf.Clamp(value*edge,-.8f,.8f); prev=noise;
            }
            clip=AudioClip.Create("Original Home V3 "+id,data.Length,1,rate,false); clip.SetData(data,0); Sounds[id]=clip; return clip;
        }

        static PanicAtmosphereArtState NewState(string name) { return new GameObject(name).AddComponent<PanicAtmosphereArtState>(); }
        static Transform Group(Transform parent,string name,Vector3 position)
        { var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=position; return go.transform; }
        static Material Mat(PanicAtmosphereArtState s,string name,Color color,float smooth,float metal=0,Color emission=default(Color))
        {
            var m=new Material(Shader.Find("Standard")){name=name,color=color}; m.SetFloat("_Glossiness",smooth); m.SetFloat("_Metallic",metal);
            if(emission.maxColorComponent>0) {m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);}
            s.Owned.Add(m);return m;
        }
        static Material SoftMat(PanicAtmosphereArtState s,string name,bool flame,Color color)
        {
            var shader=Shader.Find("Panic/SoftWeather"); if(!shader) shader=Shader.Find("Sprites/Default");
            var m=new Material(shader){name=name,color=color}; const int size=64; var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=(x+.5f)/size,v=(y+.5f)/size,alpha;
                if(flame)
                {
                    float width=Mathf.Lerp(.77f,.03f,Mathf.Pow(v,.72f)); float bend=Mathf.Sin(v*11)*v*.16f;
                    alpha=Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(u*2-1-bend)/width),1.5f)*Mathf.SmoothStep(0,1,v*12)*(1-Mathf.SmoothStep(.65f,1,v));
                }
                else alpha=Mathf.Pow(Mathf.Clamp01(1-new Vector2(u*2-1,v*2-1).magnitude),1.5f);
                alpha*=.86f+.14f*Mathf.Sin(u*27+Mathf.Sin(v*19)*2);
                if(x==0||y==0||x==size-1||y==size-1)alpha=0;
                pixels[y*size+x]=new Color(1,1,1,alpha);
            }
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name+" original alpha",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            tex.SetPixels(pixels);tex.Apply(false,true);m.mainTexture=tex;m.renderQueue=3000;s.Owned.Add(tex);s.Owned.Add(m);return m;
        }
        static void Marker(PanicAtmosphereArtState s,string name,Vector3 p,Color color,float phase)
        {
            var mat=Mat(s,name,color,.6f,0,color); var b=new Batch(s,s.transform); b.Ellipsoid(p,Vector3.one*.065f,mat);b.Flush();
            s.Markers.Add(new PanicAtmosphereArtState.Marker {Material=mat,Color=color,Phase=phase});
        }
        static void RotorHaze(PanicAtmosphereArtState s,Transform parent,float radius,bool tail)
        {
            var material=SoftMat(s,"Translucent swept rotor arcs",false,new Color(.34f,.38f,.36f,.045f));
            var b=new Batch(s,parent); const int n=64;
            for(int i=0;i<n;i++)
            {
                float a=i*Mathf.PI*2/n,c=(i+1)*Mathf.PI*2/n;
                Vector3 v1=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),v2=new Vector3(Mathf.Cos(c),0,Mathf.Sin(c));
                if(tail){v1=new Vector3(0,v1.x,v1.z);v2=new Vector3(0,v2.x,v2.z);}
                b.Quad(v1*radius*.3f,v1*radius,v2*radius,v2*radius*.3f,material);
            }
            b.Flush();
        }
        static ParticleSystem Emitter(PanicAtmosphereArtState s,string name,Material mat,Vector3 p,int cap,float rate,float lifeMin,float lifeMax,float sizeMin,float sizeMax)
        {
            var go=Group(s.transform,name,p).gameObject; var ps=go.AddComponent<ParticleSystem>(); ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main; main.loop=true;main.duration=5;main.playOnAwake=false;main.maxParticles=cap;main.startLifetime=new ParticleSystem.MinMaxCurve(lifeMin,lifeMax);
            main.startSize=new ParticleSystem.MinMaxCurve(sizeMin,sizeMax);main.startSpeed=.1f;main.startColor=Color.white;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            ps.useAutoRandomSeed=false;ps.randomSeed=(uint)(s.Emitters.Count*173+79);
            var emission=ps.emission;emission.rateOverTime=rate;
            var shape=ps.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(.55f,.03f,.22f);
            var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();
            gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.85f,.15f),new GradientAlphaKey(.6f,.55f),new GradientAlphaKey(0,1)});color.color=gradient;
            var noise=ps.noise;noise.enabled=true;noise.strength=.07f;noise.frequency=.6f;noise.scrollSpeed=.35f;noise.quality=ParticleSystemNoiseQuality.Low;
            var r=ps.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.maxParticleSize=.35f;
            s.Emitters.Add(ps);ps.Play();return ps;
        }
        static void FlameEmitter(ParticleSystem ps,float minimumHeight,float maximumHeight,float speed)
        {
            var main=ps.main;main.startSize3D=true;main.startSizeX=new ParticleSystem.MinMaxCurve(.14f,.28f);main.startSizeY=new ParticleSystem.MinMaxCurve(minimumHeight,maximumHeight);main.startSizeZ=.15f;main.startSpeed=speed;
            var shape=ps.shape;shape.rotation=new Vector3(-90,0,0);shape.scale=new Vector3(.52f,.23f,.025f);
            var v=ps.velocityOverLifetime;v.enabled=true;v.space=ParticleSystemSimulationSpace.Local;v.y=.28f;
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.65f),new Keyframe(.28f,1),new Keyframe(1,.12f)));
            ps.GetComponent<ParticleSystemRenderer>().renderMode=ParticleSystemRenderMode.VerticalBillboard;
        }
        static Vector3 DebrisPoint(int i)
        {
            float x=(H(i*3+71)-.5f)*14.2f,z=(H(i*7+84)-.5f)*10.3f;
            // Preserve the physical central stair opening in the host's house.
            if(Mathf.Abs(x)<1.3f&&z>-.4f)x=(i%2==0?-1:1)*(1.5f+H(i+67)*2);
            if(i%4==0){x=(H(i+76)-.5f)*23;z=-7.2f-H(i+24)*3;}
            return new Vector3(x,.012f,z);
        }
        static float H(int i){return Mathf.Repeat(Mathf.Sin(i*127.17f+31.73f)*43758.5453f,1);}
        static void Patch(Batch b,Vector3 p,float x,float z,Material m,int seed)
        {
            int n=11;for(int i=0;i<n;i++)
            {
                float a=i*Mathf.PI*2/n,c=(i+1)*Mathf.PI*2/n;
                float r=.8f+H(seed*13+i)*.2f,t=.8f+H(seed*13+(i+1)%n)*.2f;
                b.Triangle(p,p+new Vector3(Mathf.Cos(a)*x*r,0,Mathf.Sin(a)*z*r),p+new Vector3(Mathf.Cos(c)*x*t,0,Mathf.Sin(c)*z*t),m);
            }
        }
        static void Ring(Batch b,float radius,float width,Material m)
        {
            for(int i=0;i<40;i++)
            {
                float a=i*Mathf.PI/20,c=(i+1)*Mathf.PI/20;
                Vector3 p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),q=new Vector3(Mathf.Cos(c),0,Mathf.Sin(c));
                b.Quad(p*(radius-width),p*radius,q*radius,q*(radius-width),m);
            }
        }

        // All static elements of a node are merged by material. No primitive colliders are created.
        sealed class Batch
        {
            readonly PanicAtmosphereArtState owner;readonly Transform parent;
            readonly Dictionary<Material,List<CombineInstance>> parts=new Dictionary<Material,List<CombineInstance>>();
            public Batch(PanicAtmosphereArtState state,Transform target){owner=state;parent=target;}
            void Add(Mesh mesh,Vector3 p,Quaternion q,Vector3 scale,Material material)
            {
                List<CombineInstance> list;if(!parts.TryGetValue(material,out list)){list=new List<CombineInstance>();parts.Add(material,list);}
                list.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,q,scale)});owner.Owned.Add(mesh);
            }
            public void Box(Vector3 p,Vector3 size,Material m,Quaternion q=default(Quaternion))
            {
                if(q.Equals(default(Quaternion)))q=Quaternion.identity;
                Vector3[] v={new Vector3(-.5f,-.5f,-.5f),new Vector3(.5f,-.5f,-.5f),new Vector3(.5f,.5f,-.5f),new Vector3(-.5f,.5f,-.5f),new Vector3(-.5f,-.5f,.5f),new Vector3(.5f,-.5f,.5f),new Vector3(.5f,.5f,.5f),new Vector3(-.5f,.5f,.5f)};
                int[] t={0,2,1,0,3,2,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,0,1,5,0,5,4,3,7,6,3,6,2};
                var mesh=Mesh(v,t);Add(mesh,p,q,size,m);
            }
            public void Beam(Vector3 a,Vector3 b,float width,Material m){Box((a+b)*.5f,new Vector3(width,(b-a).magnitude,width),m,Quaternion.FromToRotation(Vector3.up,b-a));}
            public void Ellipsoid(Vector3 p,Vector3 radii,Material m)
            {
                const int rings=10,sides=16;var v=new Vector3[(rings+1)*(sides+1)];var t=new int[rings*sides*6];int k=0;
                for(int y=0;y<=rings;y++)for(int x=0;x<=sides;x++)
                {float a=y*Mathf.PI/rings,b=x*Mathf.PI*2/sides;v[y*(sides+1)+x]=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b));}
                for(int y=0;y<rings;y++)for(int x=0;x<sides;x++){int a=y*(sides+1)+x,b=a+sides+1;t[k++]=a;t[k++]=a+1;t[k++]=b;t[k++]=a+1;t[k++]=b+1;t[k++]=b;}
                Add(Mesh(v,t),p,Quaternion.identity,radii,m);
            }
            public void Tube(Vector3 a,Vector3 b,float bottom,float top,Material m)
            {
                const int sides=12;var v=new Vector3[sides*2+2];var t=new List<int>();
                for(int i=0;i<sides;i++){float angle=i*Mathf.PI*2/sides;v[i]=new Vector3(Mathf.Cos(angle)*bottom,0,Mathf.Sin(angle)*bottom);v[i+sides]=new Vector3(Mathf.Cos(angle)*top,1,Mathf.Sin(angle)*top);}
                v[sides*2]=Vector3.zero;v[sides*2+1]=Vector3.up;
                for(int i=0;i<sides;i++){int j=(i+1)%sides;t.AddRange(new[]{i,i+sides,j+sides,i,j+sides,j,sides*2,i,j,sides*2+1,j+sides,i+sides});}
                Add(Mesh(v,t.ToArray()),a,Quaternion.FromToRotation(Vector3.up,b-a),new Vector3(1,(b-a).magnitude,1),m);
            }
            public void Triangle(Vector3 a,Vector3 b,Vector3 c,Material m){Add(Mesh(new[]{a,b,c,c,b,a},new[]{0,1,2,3,4,5}),Vector3.zero,Quaternion.identity,Vector3.one,m);}
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Material m)
            {
                var mesh=Mesh(new[]{a,b,c,d,d,c,b,a},new[]{0,1,2,0,2,3,4,5,6,4,6,7});mesh.uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1),new Vector2(0,0)};
                Add(mesh,Vector3.zero,Quaternion.identity,Vector3.one,m);
            }
            public void Loft(Vector3[] sections,float centerY,Material m)
            {
                const int sides=20;var v=new Vector3[sections.Length*sides+2];var t=new List<int>();
                for(int i=0;i<sections.Length;i++)for(int j=0;j<sides;j++)
                {float a=j*Mathf.PI*2/sides;v[i*sides+j]=new Vector3(Mathf.Cos(a)*sections[i].y,centerY+Mathf.Sin(a)*sections[i].z,sections[i].x);}
                for(int i=0;i<sections.Length-1;i++)for(int j=0;j<sides;j++)
                {int n=(j+1)%sides,a=i*sides+j,b=i*sides+n,c=(i+1)*sides+j,d=(i+1)*sides+n;t.AddRange(new[]{a,c,b,b,c,d});}
                int front=v.Length-2,back=v.Length-1;v[front]=new Vector3(0,centerY,sections[0].x);v[back]=new Vector3(0,centerY,sections[sections.Length-1].x);
                for(int j=0;j<sides;j++){int n=(j+1)%sides;t.AddRange(new[]{front,j,n,back,(sections.Length-1)*sides+n,(sections.Length-1)*sides+j});}
                Add(Mesh(v,t.ToArray()),Vector3.zero,Quaternion.identity,Vector3.one,m);
            }
            static Mesh Mesh(Vector3[] v,int[] t){var mesh=new Mesh{name="Original atmosphere geometry",vertices=v,triangles=t};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
            public void Flush()
            {
                foreach(var pair in parts)
                {
                    var go=new GameObject(pair.Key.name);go.transform.SetParent(parent,false);
                    var mesh=new Mesh{name=pair.Key.name+" merged",indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh.RecalculateBounds();owner.Owned.Add(mesh);
                    go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=pair.Key;
                    r.shadowCastingMode=pair.Key.renderQueue>=3000?ShadowCastingMode.Off:ShadowCastingMode.On;r.receiveShadows=pair.Key.renderQueue<3000;
                }
                // Temporary source geometry is no longer needed once the batch is merged.
                foreach(var pair in parts)foreach(var ci in pair.Value){owner.Owned.Remove(ci.mesh);UnityEngine.Object.Destroy(ci.mesh);}
                parts.Clear();
            }
        }
    }

    // Host calls animation explicitly; there is no independent Update/camera/model logic.
    public sealed class PanicAtmosphereArtState:MonoBehaviour
    {
        public struct Marker{public Material Material;public Color Color;public float Phase;}
        public struct FloatPart{public Transform Transform;public Vector3 Origin;public float Phase;public bool Ripple;}
        public DisasterType Type;
        public Transform MainRotor,TailRotor;
        public Light HearthLight;
        public readonly List<UnityEngine.Object> Owned=new List<UnityEngine.Object>();
        public readonly List<Marker> Markers=new List<Marker>();
        public readonly List<FloatPart> Floating=new List<FloatPart>();
        public readonly List<ParticleSystem> Emitters=new List<ParticleSystem>();
        void OnDestroy(){for(int i=0;i<Owned.Count;i++)if(Owned[i])Destroy(Owned[i]);Owned.Clear();}
    }
}
