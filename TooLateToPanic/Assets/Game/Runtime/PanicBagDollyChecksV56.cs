using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Same-driver observer around the existing natural handoff. No attach or new run/profile.</summary>
    public static class PanicBagDollyChecksV56
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        static int runs;static bool prepared,priorReduced;
        public static void Prepare(PanicHomeWorld w){priorReduced=w.ReducedFpsMotionV54;prepared=true;if(runs==1)w.SetReducedFpsMotionV54(true);}
        [Serializable] public sealed class Frame
        {
            public int frame;public float wall,age,progress,prep,opacity,fov,projectionX,projectionY,eyeDistance;
            public Vector3 actor,camera;public Quaternion rotation;
            public bool active,fps,body,hands,orthographic,pathClear,paused,reduced;
        }
        [Serializable] public sealed class Trace
        {
            public string evidence="Native real-frame camera observer within the existing v56 smoke handoff. Existing caller sets preparation45 explicitly; this helper changes no actor/controller/time field. One developer-menu suspension and next existing handoff reduced-motion setting are declared. Screenshots require owner pixel review; no OS input, human quality, listening or physical traversal claim.";
            public int run;public string controllerBefore,controllerAfter;public bool reducedFixture,complete;
            public List<Frame> frames=new List<Frame>();public List<string> captures=new List<string>();
        }
        static object Member(object owner,string name){var p=owner.GetType().GetProperty(name,Flags);return p!=null?p.GetValue(owner):owner.GetType().GetField(name,Flags).GetValue(owner);}
        static T Read<T>(object owner,string name){return (T)Member(owner,name);}
        static object Call(object owner,string name,params object[] args){return owner.GetType().GetMethods(Flags).First(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(owner,args);}
        static string F(float f){return f.ToString("R",CultureInfo.InvariantCulture);}
        static string V(Vector3 v){return F(v.x)+","+F(v.y)+","+F(v.z);}
        static string Controller(CharacterController c){return c.GetInstanceID()+"|"+c.enabled+"|"+c.gameObject.layer+"|"+V(c.center)+"|"+F(c.radius)+"|"+F(c.height)+"|"+F(c.skinWidth)+"|"+F(c.stepOffset)+"|"+F(c.slopeLimit)+"|"+V(c.transform.localScale);}
        static Frame Sample(PanicHomeWorld w)
        {
            var camera=w.View;var hands=Read<Camera>(w,"bagHandsCamera");var projection=camera.projectionMatrix;
            return new Frame{frame=Time.frameCount,wall=Time.realtimeSinceStartup,age=w.BagHandoffElapsedV51,progress=w.BagCameraTransitionProgress,prep=w.Model.TimeRemaining,
                opacity=w.BagBlackoutOpacityV51,fov=camera.fieldOfView,projectionX=projection.m00,projectionY=projection.m11,actor=w.PlayerPosition,camera=camera.transform.position,rotation=camera.transform.rotation,
                eyeDistance=Vector3.Distance(camera.transform.position,w.PlayerPosition+Vector3.up*1.62f),active=w.BagCameraTransitionActive,fps=w.BagFirstPersonActive,
                body=Read<bool>(w,"BagDollyBodyVisibleV56"),hands=hands&&hands.enabled,orthographic=camera.orthographic,pathClear=Read<bool>(w,"BagDollyPathClearV56"),paused=w.Model.IsPaused,reduced=w.ReducedFpsMotionV54};
        }
        static void Shot(string path,Trace trace)
        {
            var pixels=ScreenCapture.CaptureScreenshotAsTexture();try{File.WriteAllBytes(path,pixels.EncodeToPNG());trace.captures.Add(path);}finally{UnityEngine.Object.Destroy(pixels);}
        }
        public static IEnumerator Observe(PanicHomeWorld w,PanicHomeHud hud,Action<string,bool,string> check)
        {
            int run=++runs;bool pictures=run==1,oldReduced=prepared?priorReduced:w.ReducedFpsMotionV54;prepared=false;var trace=new Trace{run=run,reducedFixture=run==2};
            var controller=Read<CharacterController>(w,"controller");trace.controllerBefore=Controller(controller);Vector3 actor=w.PlayerPosition,start=w.View.transform.position;float prep=w.Model.TimeRemaining,began=Time.realtimeSinceStartup;
            bool clockHeld=true,actorHeld=true,pathClear=true,noOpaque=true,handsLate=true,pausedOnce=false,pauseHeld=true,modalsBlocked=true;int capture=0;
            float[] milestones={0f,.2f,.45f,.7f,.91f};float until=began+6f;
            try
            {
                while(w.BagCameraTransitionActive&&Time.realtimeSinceStartup<until)
                {
                    yield return new WaitForEndOfFrame();var sample=Sample(w);trace.frames.Add(sample);
                    clockHeld&=Mathf.Abs(prep-sample.prep)<.003f;actorHeld&=actor.Equals(sample.actor)&&trace.controllerBefore==Controller(controller);
                    pathClear&=sample.pathClear;noOpaque&=sample.opacity<=.1201f;handsLate&=!sample.active||!sample.hands;
                    if(pictures&&capture<milestones.Length&&sample.progress>=milestones[capture]){Shot(Path.Combine(w.EvidencePath,"fps-dolly-v56-"+capture+".png"),trace);capture++;}
                    if(!pausedOnce&&sample.progress>=.3f&&sample.progress<.8f)
                    {
                        pausedOnce=true;modalsBlocked&=!(bool)Call(hud,"OpenKeyboardHelpV56")&&!w.Notebook.RequestOpen();hud.SetDeveloperMenuOpen(true);
                        float age=w.BagHandoffElapsedV51,time=w.Model.TimeRemaining;Vector3 camera=w.View.transform.position;Quaternion rotation=w.View.transform.rotation;float end=Time.realtimeSinceStartup+.15f;
                        while(Time.realtimeSinceStartup<end){yield return null;pauseHeld&=w.Model.IsPaused&&Mathf.Abs(w.BagHandoffElapsedV51-age)<.003f&&Mathf.Abs(w.Model.TimeRemaining-time)<.003f&&camera.Equals(w.View.transform.position)&&rotation.Equals(w.View.transform.rotation);}
                        pauseHeld&=!Read<bool>(w,"FpsTensionPlayingV55");hud.SetDeveloperMenuOpen(false);
                    }
                    yield return null;
                }
                yield return new WaitForEndOfFrame();var final=Sample(w);trace.frames.Add(final);if(pictures)Shot(Path.Combine(w.EvidencePath,"fps-dolly-v56-stable.png"),trace);
                var moving=trace.frames.Where(f=>f.active).ToArray();float maxStep=0;bool rotationContinuous=true;
                for(int i=1;i<moving.Length;i++){maxStep=Mathf.Max(maxStep,Vector3.Distance(moving[i-1].camera,moving[i].camera));rotationContinuous&=Quaternion.Angle(moving[i-1].rotation,moving[i].rotation)<35;}
                bool projection=moving.All(f=>!float.IsNaN(f.fov)&&f.fov>=10&&f.fov<=100&&f.projectionX>0&&f.projectionY>0);
                check("dolly.clock-and-actor."+run,clockHeld&&actorHeld&&trace.controllerBefore==Controller(controller),"Actual natural2s frames retain preparation clock, same actor position and exact CharacterController instance/dimensions; no helper Warp/Tick/time mutation.");
                check("dolly.visible-approach."+run,moving.Length>=8&&moving.Any(f=>f.progress<.3f&&f.body)&&moving.Any(f=>f.progress>.9f&&!f.body)&&moving.Last().eyeDistance<moving.First().eyeDistance*.25f&&noOpaque&&handsLate,"Body visible early then hidden near eye; actual camera approaches actor; hands only after handoff; all veil samples<=.12. frames="+moving.Length);
                check("dolly.camera-and-projection."+run,projection&&rotationContinuous&&maxStep<=Mathf.Max(.8f,Vector3.Distance(start,actor+Vector3.up*1.62f)*.18f),"Live pose/FOV/projection trace; maximum observed camera step="+maxStep+"; smooth route must still be reviewed in captured motion/pixels.");
                check("dolly.collision-path."+run,pathClear&&Read<bool>(w,"BagDollyPathClearV56"),"Production collision-safe camera route telemetry checked on each rendered frame; adjustment count="+Read<int>(w,"BagDollyCollisionAdjustmentCountV56")+". No collider changes.");
                check("dolly.pause-and-modal-ownership."+run,pausedOnce&&pauseHeld&&modalsBlocked,"F1/notebook refused during transition; actual developer-menu pause holds camera age/pose and preparation; resumes same handoff.");
                check("dolly.stable-real-fps."+run,!w.BagCameraTransitionActive&&w.BagFirstPersonActive&&!final.orthographic&&final.eyeDistance<.18f&&final.hands&&w.LastBagHandoffElapsedSecondsV51>=1.99f&&w.LastBagHandoffFullBlackSecondsV51<=.001f,"Natural completion at real actor eye, hands and perspective; actual duration2s, no opaque hold. reducedMotion="+w.ReducedFpsMotionV54);
                if(pictures)check("dolly.representative-frame-files",capture==5&&trace.captures.Count==6&&trace.captures.All(p=>File.Exists(p)&&new FileInfo(p).Length>2048),"Five actual transition framebuffer milestones plus stable FPS; owner inspects valid pixels/body/joins/collision visibility.");
                trace.complete=true;
            }
            finally{trace.controllerAfter=Controller(controller);w.SetReducedFpsMotionV54(oldReduced);File.WriteAllText(Path.Combine(w.EvidencePath,"fps-dolly-v56-trace-"+run+".json"),JsonUtility.ToJson(trace,true));}
            yield return null;
        }
    }
}
