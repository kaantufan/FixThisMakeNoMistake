using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in QA helper, invoked by the root driver only. Not a production movement/controller replacement.</summary>
    public static class PanicRoofAccessProbeR6
    {
        [Serializable] public sealed class Result
        {
            public string id, layer, scope, modelBeforeSHA256, modelAfterSHA256;
            public bool passed, groundedStart, touchedRoofBarrier, enteredRoofArea;
            public Vector3 start, end;
            public float highestFeet, jumpRise;
            public int frames, sides, above, jumpRequests, barrierContacts, initialOverlaps;
        }
        public static IEnumerator Run(PanicHomeWorld world, Action<Result> record, Action<string,Vector3> fixture)
        {
            var controller = world ? world.GetComponentInChildren<CharacterController>() : null;
            // The actor is a scene root, not a child of World in this project.
            if (world && !controller)
                foreach (var candidate in UnityEngine.Object.FindObjectsByType<CharacterController>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
                    if (Vector3.Distance(candidate.transform.position,world.PlayerPosition)<.01f) { controller=candidate; break; }
            if (!world || !controller || !world.RoofAccessR6 || world.CinematicActive || !controller.enabled || world.Model.IsPaused ||
                world.WorkActive || world.IsMoving || world.InventoryOpen)
            {
                record(new Result { id="roof-r6.setup",passed=false,layer="INVALID_SETUP",scope="Actual enabled controller, installed roof guards and non-cinematic phase required." });
                yield break;
            }
            var guard = world.RoofAccessR6;
            var observer = controller.gameObject.AddComponent<PanicRoofAccessProbeContactR6>();
            bool previousInput = world.FullFlowScriptedInput;
            Vector3 original = world.PlayerPosition,center=controller.center;
            float height=controller.height,radius=controller.radius,step=controller.stepOffset,skin=controller.skinWidth,slope=controller.slopeLimit;
            string modelBefore=ModelSnapshot(world);
            world.FullFlowScriptedInput=true;
            try
            {
                record(new Result { id="roof-r6.permanent-solid-volumes",passed=guard.EnabledBarrierCount==2 && guard.LayerCollisionEnabled &&
                    !guard.MainBarrier.isTrigger && !guard.GarageBarrier.isTrigger && guard.MainBarrier.gameObject.layer==30 && guard.GarageBarrier.gameObject.layer==30,
                    layer="NATIVE_PHYSICS_STATE",scope="Two permanent colliders, actual0-to30 collision matrix; no render-root dependency." });
                // All starts below are legal floor/balcony surfaces. Every movement after setup is
                // production SmokeMove+Jump. No fixture starts on a roof or on top of furniture.
                yield return Attempt(world,controller,observer,guard,"upper-bookshelf",new Vector3(6.2f,3.2f,-.25f),Vector3.right,record,fixture);
                var roofField=typeof(PanicHomeWorld).GetField("roof",BindingFlags.Instance|BindingFlags.NonPublic);
                var roofVisual=roofField!=null?roofField.GetValue(world) as GameObject:null;
                record(new Result { id="roof-r6.solid-during-actual-cutaway",passed=roofVisual&&!roofVisual.activeInHierarchy&&guard.EnabledBarrierCount==2,
                    layer="NATIVE_RENDER_PHYSICS_STATE",scope="After actual upper-room movement: production hides the roof visual; both independent permanent colliders remain enabled. No QA SetActive or forced rendering state." });
                yield return Attempt(world,controller,observer,guard,"upper-window",new Vector3(4,3.2f,-4.8f),Vector3.back,record,fixture);
                yield return Attempt(world,controller,observer,guard,"balcony-eave",new Vector3(-2,3.2f,-7.6f),Vector3.forward,record,fixture);
                yield return Attempt(world,controller,observer,guard,"balcony-chair",new Vector3(-4.4f,3.2f,-7.8f),Vector3.forward,record,fixture);
                yield return Attempt(world,controller,observer,guard,"garage-car",new Vector3(9.3f,0,3.2f),Vector3.right,record,fixture);
                yield return Attempt(world,controller,observer,guard,"garage-front-eave",new Vector3(11.7f,0,-1.15f),Vector3.forward,record,fixture);
                // These two starts are OUTSIDE the roof footprint, in air. Deliberate horizontal
                // physics-only probes isolate the formerly open lateral boundary. They are not
                // normal reachability routes, and no fixture is placed on the roof itself.
                yield return SideProbe(world,controller,observer,guard,"main-west-lateral",new Vector3(guard.MainBarrierBounds.min.x-.8f,6.70f,0),Vector3.right,true,record,fixture);
                yield return SideProbe(world,controller,observer,guard,"garage-east-lateral",new Vector3(guard.GarageBarrierBounds.max.x+.8f,guard.GarageWalkSurfaceY+.10f,3.1f),Vector3.left,false,record,fixture);
                record(new Result { id="roof-r6.controller-contract",passed=controller.height==height && controller.radius==radius && controller.center==center &&
                    controller.stepOffset==step && controller.skinWidth==skin && controller.slopeLimit==slope && controller.enabled,
                    layer="NATIVE_PHYSICS_STATE",scope="Original dimensions/step/slope/skin and enabled controller preserved. Jump/gravity source unchanged." });
                string modelAfter=ModelSnapshot(world);
                record(new Result { id="roof-r6.model-snapshot",passed=modelBefore==modelAfter,modelBeforeSHA256=modelBefore,modelAfterSHA256=modelAfter,
                    layer="NATIVE_MODEL_SNAPSHOT",scope="Budget/wave/phase and every real Model item public field before/after this helper. No clock freeze or Model mutation by helper; caller must keep the existing isolated roof QA phase stable." });
            }
            finally
            {
                fixture("QA_RESTORE_ORIGINAL_POSITION_NOT_A_ROUTE",original);world.Warp(original);
                world.FullFlowScriptedInput=previousInput;UnityEngine.Object.Destroy(observer);
            }
        }

        static IEnumerator Attempt(PanicHomeWorld w,CharacterController controller,PanicRoofAccessProbeContactR6 observer,PanicRoofAccessPhysicsR6 guard,
            string id,Vector3 start,Vector3 direction,Action<Result> record,Action<string,Vector3> fixture)
        {
            fixture("LEGAL_FLOOR_SETUP_ONLY "+id,start);w.Warp(start+Vector3.up*.04f);
            float settle=Time.realtimeSinceStartup+.8f;int settled=0;
            do { w.SmokeMove(Vector2.zero,Dt);settled++;yield return null; } while ((!w.IsGrounded||settled<3)&&Time.realtimeSinceStartup<settle);
            var r=new Result { id="roof-r6.normal-attempt."+id,layer="NATIVE_PHYSICS_ROUTE_ATTEMPT",start=w.PlayerPosition,
                highestFeet=w.PlayerPosition.y,groundedStart=w.IsGrounded,initialOverlaps=InteriorOverlaps(controller),scope="Only this declared floor start onward: real Jump+SmokeMove/controller, no route teleport. No claim that a roof was reached before." };
            if(!r.groundedStart||r.initialOverlaps!=0){r.passed=false;r.layer="INVALID_SETUP";r.scope+=" Actual starting capsule must be grounded and free of interior overlap.";record(r);yield break;}
            observer.ResetContacts();float end=Time.realtimeSinceStartup+1.3f;
            while(Time.realtimeSinceStartup<end)
            {
                if(w.IsGrounded){w.Jump();r.jumpRequests++;}
                w.SmokeMove(Axis(w,direction),Dt);Sample(r,w,controller,guard);yield return null;
            }
            r.end=w.PlayerPosition;r.jumpRise=r.highestFeet-r.start.y;r.barrierContacts=observer.BarrierContacts;r.touchedRoofBarrier=r.barrierContacts>0;
            r.passed=r.frames>=12 && r.jumpRise>.18f && !r.enteredRoofArea;
            r.scope+=" A rejection can be caused by the existing furniture/wall/ceiling; barrierContacts distinguishes the new guard. Not a whole-route or OS acceptance.";
            record(r);
        }

        static IEnumerator SideProbe(PanicHomeWorld w,CharacterController c,PanicRoofAccessProbeContactR6 observer,PanicRoofAccessPhysicsR6 guard,
            string id,Vector3 start,Vector3 direction,bool main,Action<Result> record,Action<string,Vector3> fixture)
        {
            fixture("AIR_OUTSIDE_FOOTPRINT_PHYSICS_PROBE_NOT_REACHABILITY "+id,start);w.Warp(start);observer.ResetContacts();
            var r=new Result { id="roof-r6.physics-probe."+id,layer="NATIVE_PHYSICS_PROBE",start=w.PlayerPosition,highestFeet=w.PlayerPosition.y,
                initialOverlaps=InteriorOverlaps(c),
                scope="Synthetic air start outside roof; horizontal actual CharacterController.Move only. Isolates solid lateral guard, never a playable access route." };
            if(r.initialOverlaps!=0){r.layer="INVALID_SETUP";r.passed=false;record(r);yield break;}
            float end=Time.realtimeSinceStartup+.45f;
            while(Time.realtimeSinceStartup<end)
            {
                Physics.SyncTransforms();c.Move(direction*PanicHomeWorld.WalkSpeed*Dt);Sample(r,w,c,guard);yield return null;
            }
            r.end=w.PlayerPosition;r.barrierContacts=observer.BarrierContacts;r.touchedRoofBarrier=r.barrierContacts>0;
            bool outside=main?r.end.x<guard.MainBarrierBounds.min.x-.10f:r.end.x>guard.GarageBarrierBounds.max.x+.10f;
            r.passed=r.frames>=8 && r.sides>0 && r.touchedRoofBarrier && outside && !r.enteredRoofArea;record(r);
        }
        static void Sample(Result r,PanicHomeWorld w,CharacterController c,PanicRoofAccessPhysicsR6 g)
        {
            r.frames++;r.highestFeet=Mathf.Max(r.highestFeet,w.PlayerPosition.y);
            if((c.collisionFlags&CollisionFlags.Sides)!=0)r.sides++;
            if((c.collisionFlags&CollisionFlags.Above)!=0)r.above++;
            Vector3 p=w.PlayerPosition;
            r.enteredRoofArea|=PlanarContains(g.MainBarrierBounds,p)&&p.y>=PanicRoofAccessPhysicsR6.MainCeilingUnderside+.01f ||
                PlanarContains(g.GarageBarrierBounds,p)&&p.y>=g.GarageWalkSurfaceY-.03f;
        }
        static bool PlanarContains(Bounds b,Vector3 p){return p.x>b.min.x+.02f&&p.x<b.max.x-.02f&&p.z>b.min.z+.02f&&p.z<b.max.z-.02f;}
        static string ModelSnapshot(PanicHomeWorld w)
        {
            var text=new StringBuilder();text.Append(w.Model.Budget).Append('|').Append(w.Model.Wave).Append('|').Append(w.Model.Phase);
            foreach(var item in w.Model.Items)text.Append('|').Append(JsonUtility.ToJson(item));
            using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-","");
        }
        static int InteriorOverlaps(CharacterController c)
        {
            Physics.SyncTransforms();Vector3 scale=c.transform.lossyScale;
            float radius=c.radius*Mathf.Max(Mathf.Abs(scale.x),Mathf.Abs(scale.z));
            float height=Mathf.Max(radius*2,c.height*Mathf.Abs(scale.y));
            Vector3 center=c.transform.TransformPoint(c.center),up=c.transform.up;
            float segment=Mathf.Max(0,height*.5f-radius),innerRadius=Mathf.Max(.01f,radius-c.skinWidth-.005f);int count=0;
            foreach(var hit in Physics.OverlapCapsule(center+up*segment,center-up*segment,innerRadius,~0,QueryTriggerInteraction.Ignore))
                if(hit!=c&&!hit.transform.IsChildOf(c.transform))count++;
            return count;
        }
        static float Dt {get{return Mathf.Clamp(Time.deltaTime,.001f,.08f);}}
        static Vector2 Axis(PanicHomeWorld w,Vector3 direction)
        {
            Vector3 f=w.View.transform.forward,r=w.View.transform.right;f.y=r.y=0;f.Normalize();r.Normalize();direction.y=0;direction.Normalize();
            return Vector2.ClampMagnitude(new Vector2(Vector3.Dot(direction,r),Vector3.Dot(direction,f)),1);
        }
    }
    public sealed class PanicRoofAccessProbeContactR6 : MonoBehaviour
    {
        public int BarrierContacts {get;private set;}
        public void ResetContacts(){BarrierContacts=0;}
        void OnControllerColliderHit(ControllerColliderHit hit)
        {if(hit.collider&&hit.collider.GetComponent<PanicRoofBarrierPartR6>())BarrierContacts++;}
    }
}
