using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    // Cosmetic traffic and response life uses true Model phases. Never writes Model, input, camera or physics.
    public sealed class PanicStreetLifeV414:MonoBehaviour
    {
        sealed class Walker
        {public GameObject Root;public Animator Animator;public Vector3 A,B;public float Offset,Speed;}
        sealed class Car
        {public GameObject Root;public float Offset,Speed;public Vector3 Previous;public PanicStreetLifeArtV414.VehicleModelFront ModelFront;}
        sealed class Response
        {public Car Car;public Walker Crew;public float Delay,Destination;public Transform[] Wheels;public Renderer[] Beacons;}
        readonly List<Walker> civilians=new List<Walker>(2);
        readonly List<Car> traffic=new List<Car>(2),parked=new List<Car>(2);
        readonly List<Response> response=new List<Response>(4);
        readonly List<ParticleSystem> residualSmoke=new List<ParticleSystem>(2);
        readonly GameObject[] wrecks=new GameObject[2];
        readonly MaterialPropertyBlock beaconProperties=new MaterialPropertyBlock();
        GameObject streetRoot,responseRoot;
        PanicHomeWorld world;
        PanicPhase lastPhase;
        float clock,responseClock,hazardClock;
        bool initialized,phaseLatch,responsePending;
        int eventCount;
        public bool HasObservedDisaster {get;private set;}
        public DisasterType LastObservedDisaster {get;private set;}
        public bool ResponseActive {get{return responseRoot&&responseRoot.activeInHierarchy&&response.Count>0;}}
        public int CivilianCount {get{int count=0;foreach(var w in civilians)if(Visible(w.Root))count++;return count;}}
        public int MovingCivilianCount {get{int count=0;foreach(var w in civilians)if(Visible(w.Root)&&w.Speed>.01f)count++;return count;}}
        public int ResponseVehicleCount {get{int count=0;foreach(var r in response)if(Visible(r.Car.Root))count++;return count;}}
        public int VehicleCount {get{int count=ResponseVehicleCount;foreach(var c in traffic)if(Visible(c.Root))count++;foreach(var c in parked)if(Visible(c.Root))count++;foreach(var go in wrecks)if(Visible(go))count++;return count;}}
        public int MovingVehicleCount {get{int count=0;foreach(var c in traffic)if(Visible(c.Root)&&c.Speed>.01f)count++;foreach(var r in response)if(Visible(r.Car.Root)&&r.Car.Speed>.01f)count++;return count;}}
        public int ResponderCount {get{int count=0;foreach(var r in response)if(r.Crew!=null&&Visible(r.Crew.Root))count++;return count;}}
        static bool Visible(GameObject root){return root&&root.activeInHierarchy;}

        public void Bind(PanicHomeWorld owner)
        {
            if(initialized)return;world=owner;if(!world)return;
            streetRoot=new GameObject("V414 street life");streetRoot.transform.SetParent(transform,false);
            for(int i=0;i<2;i++)
            {
                var moving=new Car{Root=PanicStreetLifeArtV414.CivilianCar(i),Offset=i*31,ModelFront=PanicStreetLifeArtV414.CivilianModelFront};moving.Root.transform.SetParent(streetRoot.transform,false);moving.Root.SetActive(false);traffic.Add(moving);
                var stand=new Car{Root=PanicStreetLifeArtV414.CivilianCar(i+2),Offset=i};stand.Root.transform.SetParent(streetRoot.transform,false);stand.Root.transform.position=ParkedPosition(i);stand.Root.transform.rotation=Quaternion.Euler(0,90,0);stand.Root.SetActive(false);parked.Add(stand);
                var npc=PanicExteriorArt.CreatePedestrianVisual(i+8);if(!npc)continue;
                npc.transform.SetParent(streetRoot.transform,false);npc.name="V414 ambient sidewalk resident "+i;npc.SetActive(false);
                civilians.Add(new Walker{Root=npc,Animator=npc.GetComponentInChildren<Animator>(),A=new Vector3(i==0?-14:4,.085f,-23.22f),B=new Vector3(i==0?-4:14,.085f,-23.22f),Offset=i*4.7f});
            }
            initialized=true;ResetRun();
        }
        public void ResetRun()
        {
            clock=responseClock=hazardClock=0;eventCount=0;HasObservedDisaster=false;phaseLatch=responsePending=false;
            if(responseRoot){responseRoot.SetActive(false);Destroy(responseRoot);}responseRoot=null;response.Clear();
            for(int i=0;i<wrecks.Length;i++)if(wrecks[i]){wrecks[i].SetActive(false);Destroy(wrecks[i]);wrecks[i]=null;}
            residualSmoke.Clear();foreach(var c in traffic){c.Speed=0;if(c.Root)c.Root.SetActive(false);}
            foreach(var c in parked)if(c.Root)c.Root.SetActive(false);
            foreach(var walker in civilians){walker.Speed=0;if(walker.Root)walker.Root.SetActive(false);}
            lastPhase=world&&world.Model!=null?world.Model.Phase:PanicPhase.Title;
        }
        public void ObserveDisaster(DisasterType type)
        {
            if(!world||world.Model==null||world.Model.Phase!=PanicPhase.Disaster||world.Model.Disaster!=type||phaseLatch)return;
            phaseLatch=true;HasObservedDisaster=true;LastObservedDisaster=type;responsePending=true;eventCount++;hazardClock=0;
        }
        void Update(){Tick(Time.deltaTime);}
        public void Tick(float dt)
        {
            if(!initialized||!world||world.Model==null)return;
            var model=world.Model;var phase=model.Phase;
            if(phase==PanicPhase.Title&&lastPhase!=PanicPhase.Title)ResetRun();
            if(phase==PanicPhase.Disaster)ObserveDisaster(model.Disaster);else phaseLatch=false;
            bool paused=model.IsPaused||world.NotebookBlocksInput;
            foreach(var ps in residualSmoke)if(ps){var main=ps.main;main.simulationSpeed=paused?0:1;}
            if(paused){FreezeMotion();lastPhase=phase;return;}
            dt=Mathf.Clamp(dt,0,.1f);clock+=dt;
            bool ambient=phase!=PanicPhase.Title&&!HasObservedDisaster&&model.Wave==1&&(phase==PanicPhase.Preparing||phase==PanicPhase.Tutorial);
            foreach(var c in traffic)
            {
                c.Root.SetActive(ambient);c.Speed=ambient?(c.Offset==0?3.6f:3.1f):0;
                if(!ambient)continue;float sign=c.Offset==0?1:-1;
                float x=sign*(Mathf.Repeat(clock*c.Speed+c.Offset,72)-36);
                c.Root.transform.SetPositionAndRotation(new Vector3(x,.035f,sign>0?-17.2f:-20.7f),PanicStreetLifeArtV414.VehicleHeading(Vector3.right*sign,c.ModelFront));
            }
            if(phase==PanicPhase.Disaster)hazardClock+=dt;
            for(int i=0;i<parked.Count;i++)
            {
                var root=parked[i].Root;root.SetActive(phase!=PanicPhase.Title&&!wrecks[i]);
                Vector3 p=ParkedPosition(i);float roll=0,pitch=0;
                if(phase==PanicPhase.Disaster&&model.Disaster==DisasterType.Earthquake)
                {float kick=Mathf.Exp(-Mathf.Repeat(hazardClock+i*.18f,1.45f)*2.8f);roll=Mathf.Sin(hazardClock*24+i)*kick*1.8f;pitch=Mathf.Sin(hazardClock*31+i)*kick; p.y+=Mathf.Abs(Mathf.Sin(hazardClock*31))*kick*.035f;}
                else if(phase==PanicPhase.Disaster&&model.Disaster==DisasterType.Storm)
                {float gust=Mathf.Pow(.5f+.5f*Mathf.Sin(hazardClock*1.08f+Mathf.Sin(hazardClock*.37f)*1.7f),2);roll=-gust*2.4f;}
                root.transform.SetPositionAndRotation(p,Quaternion.Euler(pitch,90,roll));
            }
            foreach(var walker in civilians)
            {walker.Root.SetActive(ambient);if(ambient)MoveWalker(walker,clock,.65f,dt);else walker.Speed=0;}
            if(responsePending&&phase!=PanicPhase.Disaster&&phase!=PanicPhase.Title)
            {responsePending=false;CreateResponse();CreateAffectedCar(LastObservedDisaster);}
            if(ResponseActive)AnimateResponse(dt);
            lastPhase=phase;
        }
        void FreezeMotion()
        {
            foreach(var c in traffic)c.Speed=0;
            foreach(var w in civilians){w.Speed=0;if(w.Animator)w.Animator.speed=0;}
            foreach(var r in response){r.Car.Speed=0;if(r.Crew!=null){r.Crew.Speed=0;if(r.Crew.Animator)r.Crew.Animator.speed=0;}}
        }
        static void MoveWalker(Walker walker,float time,float speed,float dt)
        {
            if(!walker.Root)return;Vector3 delta=walker.B-walker.A;float length=delta.magnitude;if(length<.01f)return;
            float travel=time*speed+walker.Offset,position=Mathf.PingPong(travel,length),sign=Mathf.Repeat(travel,length*2)<length?1:-1;
            walker.Root.transform.position=walker.A+delta.normalized*position;
            walker.Root.transform.rotation=Quaternion.Slerp(walker.Root.transform.rotation,Quaternion.LookRotation(delta.normalized*sign),1-Mathf.Exp(-dt*7));
            walker.Speed=speed;
            if(walker.Animator){walker.Animator.speed=1;walker.Animator.SetFloat("Speed",speed,.12f,Mathf.Max(dt,.001f));walker.Animator.SetBool("Airborne",false);walker.Animator.SetBool("Working",false);}
            PanicCharacterArt.SetMotion(walker.Root,walker.Animator,speed,true,0,dt);
        }
        void CreateResponse()
        {
            if(responseRoot){responseRoot.SetActive(false);Destroy(responseRoot);}response.Clear();
            responseRoot=new GameObject("V414 active emergency response "+LastObservedDisaster);responseRoot.transform.SetParent(streetRoot.transform,false);responseClock=0;
            for(int i=0;i<4;i++)
            {
                var kind=(PanicStreetLifeArtV414.Service)i;var vehicle=PanicStreetLifeArtV414.EmergencyVehicle(kind);vehicle.transform.SetParent(responseRoot.transform,false);
                var car=new Car{Root=vehicle,ModelFront=PanicStreetLifeArtV414.EmergencyModelFront};var crew=PanicStreetLifeArtV414.Responder(kind,i);Walker walker=null;
                if(crew){crew.transform.SetParent(responseRoot.transform,false);crew.SetActive(false);float x=-11+i*7.5f;walker=new Walker{Root=crew,Animator=crew.GetComponentInChildren<Animator>(),A=new Vector3(x-1.6f,.085f,-23.75f),B=new Vector3(x+2.0f,.085f,-24.7f),Offset=i*.8f};}
                var wheels=new List<Transform>();var beacons=new List<Renderer>();
                foreach(var node in vehicle.GetComponentsInChildren<Transform>())if(node.name=="Response wheel")wheels.Add(node);
                foreach(var renderer in vehicle.GetComponentsInChildren<Renderer>())if(renderer.transform.parent&&(renderer.transform.parent.name=="BeaconBlue"||renderer.transform.parent.name=="BeaconRed"))beacons.Add(renderer);
                response.Add(new Response{Car=car,Crew=walker,Delay=i*1.6f,Destination=-11+i*7.5f,Wheels=wheels.ToArray(),Beacons=beacons.ToArray()});
                vehicle.transform.SetPositionAndRotation(new Vector3(38+i*8,.035f,-21.3f),PanicStreetLifeArtV414.VehicleHeading(Vector3.left,car.ModelFront));car.Previous=vehicle.transform.position;
            }
        }
        void AnimateResponse(float dt)
        {
            responseClock+=dt;
            for(int i=0;i<response.Count;i++)
            {
                var r=response[i];float elapsed=Mathf.Max(0,responseClock-r.Delay),start=38+i*8;
                float x=Mathf.MoveTowards(start,r.Destination,elapsed*5.2f),distance=Mathf.Abs(x-r.Destination);
                r.Car.Root.transform.position=new Vector3(x,.035f,-21.3f);r.Car.Speed=dt>.00001f?Vector3.Distance(r.Car.Root.transform.position,r.Car.Previous)/dt:0;r.Car.Previous=r.Car.Root.transform.position;
                foreach(var wheel in r.Wheels)wheel.Rotate(Vector3.right,r.Car.Speed*dt*160,Space.Self);
                for(int b=0;b<r.Beacons.Length;b++)
                {var renderer=r.Beacons[b];float flash=Mathf.Repeat(responseClock*2.5f+b*.5f+i*.17f,1)<.42f?1:.08f;Color color=renderer.transform.parent.name=="BeaconBlue"?new Color(.045f,.2f,1):new Color(1,.08f,.025f);beaconProperties.Clear();beaconProperties.SetColor("_EmissionColor",color*(flash*3));beaconProperties.SetColor("_Color",color*(.35f+.65f*flash));renderer.SetPropertyBlock(beaconProperties);}
                if(r.Crew!=null){r.Crew.Root.SetActive(distance<.1f);if(distance<.1f)MoveWalker(r.Crew,responseClock,.9f,dt);}
            }
        }
        void CreateAffectedCar(DisasterType type)
        {
            int slot=(eventCount-1)%2;
            if(wrecks[slot]){wrecks[slot].SetActive(false);Destroy(wrecks[slot]);}
            var root=PanicStreetLifeArtV414.Wreck(type);root.transform.SetParent(streetRoot.transform,false);root.transform.position=ParkedPosition(slot);
            root.transform.rotation=Quaternion.Euler(type==DisasterType.Earthquake?2:0,90,type==DisasterType.Storm?5:0);wrecks[slot]=root;parked[slot].Root.SetActive(false);
            residualSmoke.Clear();foreach(var go in wrecks)if(go)residualSmoke.AddRange(go.GetComponentsInChildren<ParticleSystem>(true));
        }
        static Vector3 ParkedPosition(int slot){return new Vector3(slot==0?-11:10,.035f,-25.2f);}
        void OnDestroy(){if(streetRoot)Destroy(streetRoot);}
    }
}
