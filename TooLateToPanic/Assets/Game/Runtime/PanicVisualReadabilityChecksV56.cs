using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>Called only by the sole isolated ledger driver; no attach, launch, OS input or extra profile.</summary>
    public static class PanicVisualReadabilityChecksV56
    {
        [Serializable] public sealed class Sample
        {
            public string id,model,gameplay,cardText,helpText,colliders;
            public Vector3 player;
            public Rect cardPixels,keyboardPixels,panelPixels;
            public int frame,width,height,cardRepaints,helpRepaints,callouts;
            public float preparation,phaseTime,helpProgress;
            public bool canInteract,cardVisible,roofVisible,helpVisible,helpBlocking,helpModelGate;
        }
        [Serializable] public sealed class Detail
        {
            public string evidence="Actual same-process production API/frame checks. Pose, selected focus, roof tier and utility state fixtures are explicit. Tier fixture checks presentation, not a paid work transaction. Breaker fixture checks existing light ownership, not lever traversal. No OS input, listening, human legibility/quality or full campaign claim.";
            public bool complete;
            public List<string> fixtures=new List<string>();
            public List<Sample> samples=new List<Sample>();
            public List<string> lintelInventory=new List<string>();
        }
        static T Read<T>(object owner,string name){return PanicWaveLedgerModelChecksV56.Read<T>(owner,name);}
        static object Call(object owner,string name,params object[] args){return PanicWaveLedgerModelChecksV56.Call(owner,name,args);}
        static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
        static string State(PanicHomeWorld w){return PanicWaveLedgerModelChecksV56.ModelStamp(w.Model)+"|"+PanicNotebookVerification.GameplayFingerprint(w)+"|"+w.Model.UtilitySafety.GasClosed+":"+w.Model.UtilitySafety.WaterClosed+":"+w.Model.UtilitySafety.PowerOff;}
        static bool HelpGate(PanicHomeWorld w,PanicHomeHud hud){return Read<bool>(hud,"KeyboardHelpBlockingV56")&&Read<bool>(w.Model,"KeyboardHelpOpenV56");}
        static Rect Pixels(PanicHomeHud hud,Rect logical){return new Rect(hud.CanvasOffset.x+logical.x*hud.CanvasScale,hud.CanvasOffset.y+logical.y*hud.CanvasScale,logical.width*hud.CanvasScale,logical.height*hud.CanvasScale);}
        static bool Fits(Rect pixels){return pixels.width>0&&pixels.height>0&&pixels.xMin>=0&&pixels.yMin>=0&&pixels.xMax<=Screen.width+.5f&&pixels.yMax<=Screen.height+.5f;}
        static string Requirements(IEnumerable<PanicMaterialRequirement> rows){return string.Join("|",rows.OrderBy(r=>r.Id,StringComparer.Ordinal).Select(r=>r.Id+":"+r.Required+":"+r.Owned+":"+r.Missing));}
        static object Snapshot(PanicHomeHud hud){return PanicWaveLedgerModelChecksV56.Read(hud,"SelectedTargetSnapshotV56");}
        static string Requirements(object snapshot){return Requirements(((IEnumerable)PanicWaveLedgerModelChecksV56.Read(snapshot,"Requirements")).Cast<PanicMaterialRequirement>());}
        static string F(float value){return value.ToString("R",CultureInfo.InvariantCulture);}
        static string V(Vector3 v){return F(v.x)+","+F(v.y)+","+F(v.z);}
        static string Q(Quaternion q){return F(q.x)+","+F(q.y)+","+F(q.z)+","+F(q.w);}
        static string ColliderStamp(IEnumerable<BoxCollider> colliders)
        {
            return string.Join("|",colliders.OrderBy(c=>c.GetInstanceID()).Select(c=>c.GetInstanceID()+":"+c.enabled+":"+c.isTrigger+":"+c.gameObject.layer+":"+c.gameObject.activeInHierarchy+":"+V(c.transform.localPosition)+":"+Q(c.transform.localRotation)+":"+V(c.transform.localScale)+":"+V(c.center)+":"+V(c.size)));
        }
        static Sample Observe(string id,PanicHomeWorld w,PanicHomeHud hud,IEnumerable<BoxCollider> colliders)
        {
            return new Sample{id=id,frame=Time.frameCount,width=Screen.width,height=Screen.height,player=w.PlayerPosition,
                model=PanicWaveLedgerModelChecksV56.ModelStamp(w.Model),gameplay=PanicNotebookVerification.GameplayFingerprint(w),preparation=w.Model.TimeRemaining,phaseTime=w.Model.PhaseTime,
                canInteract=w.CanInteract,cardVisible=Read<bool>(hud,"SelectedTargetHudVisibleV56"),roofVisible=Read<bool>(w,"RoofSelectionFeedbackVisibleV56"),
                cardRepaints=Read<int>(hud,"SelectedTargetHudDrawFramesV56"),cardText=Read<string>(hud,"SelectedTargetRenderedTextV56"),cardPixels=Pixels(hud,Read<Rect>(hud,"SelectedTargetHudBoundsV56")),
                helpVisible=Read<bool>(hud,"KeyboardHelpVisibleV56"),helpBlocking=Read<bool>(hud,"KeyboardHelpBlockingV56"),helpModelGate=Read<bool>(w.Model,"KeyboardHelpOpenV56"),
                helpProgress=Read<float>(hud,"KeyboardHelpAnimationProgressV56"),helpRepaints=Read<int>(hud,"KeyboardHelpDrawFramesV56"),callouts=Read<int>(hud,"KeyboardHelpCalloutCountV56"),
                helpText=Read<string>(hud,"KeyboardHelpRenderedTextV56"),keyboardPixels=Pixels(hud,Read<Rect>(hud,"KeyboardHelpBoundsV56")),panelPixels=Pixels(hud,Read<Rect>(hud,"KeyboardHelpPanelBoundsV56")),colliders=ColliderStamp(colliders)};
        }
        static void Card(string id,PanicHomeWorld w,PanicHomeHud hud,int oldDraws,Action<string,bool,string> check)
        {
            object snapshot=Snapshot(hud);string action=w.Expedition.PrimaryActionV414(w.Selected);var expected=w.Expedition.WorkRequirementsV414(action,w.Selected.Id);int cost=w.Expedition.WorkBudgetCostV414(action,w.Selected.Id);
            bool quote=snapshot!=null&&Read<string>(snapshot,"Id")=="roof"&&Read<bool>(snapshot,"IsRoof")&&Read<string>(snapshot,"Action")==action&&Requirements(snapshot)==Requirements(expected)&&
                Read<int>(snapshot,"BudgetCost")==cost&&Read<int>(snapshot,"BudgetOwned")==w.Model.Budget&&Read<int>(snapshot,"BudgetMissing")==Mathf.Max(0,cost-w.Model.Budget);
            string text=Read<string>(hud,"SelectedTargetRenderedTextV56");
            // V57 puts the roof type in the heading and draws the actual selected item label once.
            bool title=snapshot!=null&&(text.Contains(Read<string>(snapshot,"Label"))||
                Read<bool>(snapshot,"IsRoof")&&text.Split('\n').Contains(w.Selected.Label));
            bool rows=snapshot!=null&&!string.IsNullOrEmpty(text)&&title&&text.Contains("Gerek ")&&text.Contains("Eksik ")&&expected.All(r=>text.Contains(PanicExpedition.DisplayName(r.Id)));
            Rect logical=Read<Rect>(hud,"SelectedTargetHudBoundsV56");
            check("visual.card."+id,quote&&rows&&Read<bool>(hud,"SelectedTargetHudVisibleV56")&&Read<int>(hud,"SelectedTargetHudDrawFramesV56")>oldDraws&&logical.x>=1100&&logical.y>=100&&Fits(Pixels(hud,logical)),
                "Actual Repaint+roof title/requirement rows; exact canonical recipe="+Requirements(expected)+" budget="+cost+"; framebuffer card="+Pixels(hud,logical));
        }
        static void HelpLayout(string id,PanicHomeWorld w,PanicHomeHud hud,int oldDraws,Action<string,bool,string> check)
        {
            Rect keyboard=Read<Rect>(hud,"KeyboardHelpBoundsV56"),panel=Read<Rect>(hud,"KeyboardHelpPanelBoundsV56");string text=Read<string>(hud,"KeyboardHelpRenderedTextV56")??string.Empty;int count=Read<int>(hud,"KeyboardHelpCalloutCountV56");
            var catalog=((IEnumerable)PanicWaveLedgerModelChecksV56.Read(hud,"KeyboardHelpCatalogV57")).Cast<object>().ToArray();
            bool rows=catalog.Length>0&&catalog.All(entry=>text.Contains(Read<string>(entry,"Action"))&&text.Contains(Read<string>(entry,"Context")));
            // Latest user request requires every used key in every phase. The older !text.Contains("F ·") phase oracle is deliberately removed.
            bool geometry=PanicKeyboardHelpGeometryChecksV57.ValidateCurrentDraw(hud,check);
            check("visual.f1."+id,HelpGate(w,hud)&&Read<float>(hud,"KeyboardHelpAnimationProgressV56")==1f&&Read<int>(hud,"KeyboardHelpDrawFramesV56")>oldDraws&&count==catalog.Length&&rows&&geometry&&
                Mathf.Abs(keyboard.center.x-800)<.5f&&keyboard.width>=870&&keyboard.height>=410&&Fits(Pixels(hud,keyboard))&&Fits(Pixels(hud,panel)),
                "Actual Repaint all-phase catalog explanations="+count+"; independent connection/cap/card/text geometry; keyboard="+Pixels(hud,keyboard)+" panel="+Pixels(hud,panel)+". Actual1600/960PNG pixel inspection remains required.");
        }
        static IEnumerator HelpCycle(string id,KeyCode closeKey,PanicHomeWorld w,PanicHomeHud hud,Detail detail,BoxCollider[] colliders,Action<string,bool,string> check,Func<string,string,IEnumerator> capture)
        {
            int draws=Read<int>(hud,"KeyboardHelpDrawFramesV56");float prep=w.Model.TimeRemaining,phase=w.Model.PhaseTime;string state=State(w);Vector3 position=w.PlayerPosition;
            bool open=(bool)Call(hud,"ProcessKeyboardHelpKeyV56",KeyCode.F1);float first=Read<float>(hud,"KeyboardHelpAnimationProgressV56");bool held=open&&HelpGate(w,hud),animated=false;float until=Time.realtimeSinceStartup+1.4f;
            detail.samples.Add(Observe("f1-"+id+"-request",w,hud,colliders));
            while(Read<float>(hud,"KeyboardHelpAnimationProgressV56")<1f&&Time.realtimeSinceStartup<until)
            {
                yield return null;float progress=Read<float>(hud,"KeyboardHelpAnimationProgressV56");animated|=progress>first&&progress<1;
                held&=HelpGate(w,hud)&&PanicWaveLedgerModelChecksV56.Near(prep,w.Model.TimeRemaining)&&PanicWaveLedgerModelChecksV56.Near(phase,w.Model.PhaseTime)&&state==State(w)&&position.Equals(w.PlayerPosition);
            }
            yield return new WaitForEndOfFrame();
            check("visual.f1.opening-holds-"+id,held&&animated&&Read<float>(hud,"KeyboardHelpAnimationProgressV56")==1f,"Real frames through .28s unscaled opening; same preparation/phase/model/inventory/actor stamps; no Tick/time/position fixture during measurement.");
            w.DispatchWorldInput(new[]{KeyCode.E,KeyCode.F,KeyCode.Tab,KeyCode.Alpha3},false,false);yield return Wait(.12f);
            check("visual.f1.open-holds-and-gates-"+id,HelpGate(w,hud)&&state==State(w)&&position.Equals(w.PlayerPosition)&&PanicWaveLedgerModelChecksV56.Near(prep,w.Model.TimeRemaining)&&PanicWaveLedgerModelChecksV56.Near(phase,w.Model.PhaseTime)&&!w.InventoryOpen&&!w.WorkActive&&!Read<bool>(hud,"SelectedTargetHudVisibleV56")&&!Read<bool>(w,"RoofSelectionFeedbackVisibleV56"),"Actual semantic World dispatch blocked while open; current roof card/cue suppressed and real wall wait holds both clocks.");
            HelpLayout(id,w,hud,draws,check);detail.samples.Add(Observe("f1-"+id+"-open-rendered",w,hud,colliders));
            yield return capture("keyboard-help-v56-"+id,"Actual enlarged centered F1 keyboard, Turkish callouts and connecting lines. API/frame fixture; sole Unity owner inspects clipping/legibility/pixels.");
            bool key=(bool)Call(hud,"ProcessKeyboardHelpKeyV56",closeKey);bool closingHeld=key&&HelpGate(w,hud),shrunk=false;until=Time.realtimeSinceStartup+1.4f;
            while(Read<bool>(hud,"KeyboardHelpVisibleV56")&&Time.realtimeSinceStartup<until)
            {
                yield return null;float progress=Read<float>(hud,"KeyboardHelpAnimationProgressV56");
                if(Read<bool>(w.Model,"KeyboardHelpOpenV56"))
                {shrunk|=progress>0&&progress<1;closingHeld&=HelpGate(w,hud)&&state==State(w)&&position.Equals(w.PlayerPosition)&&PanicWaveLedgerModelChecksV56.Near(prep,w.Model.TimeRemaining)&&PanicWaveLedgerModelChecksV56.Near(phase,w.Model.PhaseTime);}
            }
            check("visual.f1.closing-holds-"+id,closingHeld&&shrunk&&!Read<bool>(hud,"KeyboardHelpVisibleV56")&&!Read<bool>(w.Model,"KeyboardHelpOpenV56"),closeKey+" semantic close shrinks through real unscaled frames; clock/model/position stay held while the model gate remains owned.");
            yield return null;float before=w.Model.TimeRemaining;yield return Wait(.22f);
            check("visual.f1.close-resumes-"+id,!Read<bool>(hud,"KeyboardHelpBlockingV56")&&!w.InputLocked&&w.Model.TimeRemaining<before-.08f,"After final shrinking frame, help releases its gate and actual World Update advances preparation again.");
            detail.samples.Add(Observe("f1-"+id+"-closed",w,hud,colliders));
        }
        public static IEnumerator Run(PanicHomeWorld w,PanicHomeHud hud,Action<string,bool,string> check,Func<string,string,IEnumerator> capture,Func<int,int,IEnumerator> resolution)
        {
            var detail=new Detail();Vector3 original=w.PlayerPosition;string focus=Read<string>(w,"focusId");var roof=w.Model.Items.First(i=>i.Id=="roof");int tier=roof.Tier;
            bool gas=w.Model.UtilitySafety.GasClosed,water=w.Model.UtilitySafety.WaterClosed,power=w.Model.UtilitySafety.PowerOff;
            // Floors/rooms are independent scene roots, not children of the World component.
            // Pair within each actual owner: both main floors have a solid_south_lintel name.
            var lintelOwners=Read<List<GameObject>>(w,"floorVisuals").Concat(new[]{Read<GameObject>(w,"residenceBathroom"),Read<GameObject>(w,"residenceGarage")}).Where(root=>root).Distinct().ToArray();
            var lintels=lintelOwners.SelectMany(root=>root.GetComponentsInChildren<Renderer>(true)).Where(r=>r&&r.name.StartsWith("solid_",StringComparison.Ordinal)&&r.name.IndexOf("lintel",StringComparison.Ordinal)>=0).Distinct().ToArray();
            var pairs=lintels.Select(r=>
            {
                var owner=lintelOwners.FirstOrDefault(root=>r.transform.IsChildOf(root.transform));
                return owner?owner.GetComponentsInChildren<BoxCollider>(true).FirstOrDefault(c=>c.name=="Collision_"+r.name):null;
            }).ToArray();
            var boxes=pairs.Where(c=>c).Distinct().ToArray();string baseline=ColliderStamp(boxes);
            bool exactLintelSet=lintelOwners.Length==5&&lintels.Length==12&&pairs.Length==12&&boxes.Length==12&&!string.IsNullOrEmpty(baseline);
            for(int i=0;i<lintels.Length;i++)
            {
                var r=lintels[i];var owner=lintelOwners.FirstOrDefault(root=>r.transform.IsChildOf(root.transform));var c=pairs[i];
                detail.lintelInventory.Add("owner="+(owner?owner.name:"MISSING")+" renderer="+r.name+" rendererId="+r.GetInstanceID()+" parent="+(r.transform.parent?r.transform.parent.name:"scene-root")+" enabled="+r.enabled+" hiddenMarker="+(bool)r.GetComponent<PanicPermanentlyHiddenV414>()+" exactCollider="+(c?c.name:"MISSING")+" colliderFields="+(c?ColliderStamp(new[]{c}):"MISSING"));
            }
            detail.fixtures.Add("One existing World normal run/profile/driver retained. FullFlowScriptedInput stays enabled; no OS key/mouse calls.");
            detail.fixtures.Add("Pose fixtures: saved="+V(original)+"; roof approach=(0,0.04,-4.8); far=(24,0.04,-18); focus forced to roof so distance rejection cannot pass by clearing target.");
            detail.fixtures.Add("Presentation tier fixture: roof Tier "+tier+" ->1 -> "+tier+" via field assignment + existing SyncVisuals; no budget/material transaction claim.");
            detail.fixtures.Add("Power fixture: exact utility state saved, Complete(poweroff) -> Reset -> restore saved three flags; existing ApplyMainsPowerV5/World update owns Light.enabled. No physical lever completion claim.");
            try
            {
                check("visual.lintel.hidden-with-exact-physics",exactLintelSet&&pairs.All(c=>c&&c.enabled&&!c.isTrigger&&c.gameObject.activeInHierarchy)&&lintels.All(r=>!r.enabled&&r.GetComponent<PanicPermanentlyHiddenV414>())&&Read<int>(w,"DoorLintelHiddenRendererCountV56")==lintels.Length&&Read<int>(w,"DoorLintelEnabledColliderCountV56")==boxes.Length&&Read<bool>(w,"DoorLintelsPhysicsPreservedV56"),"Independent scene-root owners="+lintelOwners.Length+" (expected5), exact-name lintels="+lintels.Length+" (expected12), distinct same-owner colliders="+boxes.Length+" (expected12); nonempty exact enabled/trigger/layer/local transform/center/size stamp saved. Actual inventory persisted.");
                check("visual.roof.barriers-retained",w.RoofAccessR6&&w.RoofAccessR6.EnabledBarrierCount==2&&w.RoofAccessR6.LayerCollisionEnabled&&!w.RoofAccessR6.MainBarrier.isTrigger&&!w.RoofAccessR6.GarageBarrier.isTrigger,"Actual main and garage roof protection remain active on controller collision layer.");
                var lights=Read<List<Light>>(w,"mainsLightsV5").Where(l=>l).ToArray();var tuned=lights.Where(l=>l.name.StartsWith("Interior lamp ",StringComparison.Ordinal)||l.name=="Service room lamp").ToArray();var sun=Read<Light>(w,"revisionSun");
                check("visual.lighting.existing-owner-and-shadows",lights.Length>=6&&tuned.Length>0&&tuned.All(l=>l.intensity>=1.4f&&l.range>=7.2f)&&sun&&sun.shadows==LightShadows.Soft&&Mathf.Abs(sun.shadowStrength-.64f)<.001f&&QualitySettings.shadows==ShadowQuality.All&&QualitySettings.shadowCascades==2&&QualitySettings.shadowDistance>=60&&RenderSettings.ambientMode==AmbientMode.Trilight,"Actual registered mains="+lights.Length+" tuned="+tuned.Length+"; current Built-In soft sun/trilight quality, no new light owner.");
                w.Model.UtilitySafety.Reset();Call(w.Model.UtilitySafety,"Complete","poweroff");yield return Wait(.12f);check("visual.breaker.existing-lights-off",lights.All(l=>!l.enabled),"Explicit existing utility completion fixture; same registered Light instances all disabled.");
                w.Model.UtilitySafety.Reset();yield return Wait(.12f);check("visual.breaker.same-lights-on",lights.All(l=>l.enabled)&&Read<List<Light>>(w,"mainsLightsV5").Where(l=>l).SequenceEqual(lights),"Reset fixture restores same registered mains lights; no replacement/new owner.");
                w.Warp(new Vector3(0,.04f,-4.8f));for(int i=0;i<6;i++){w.SmokeMove(Vector2.zero,.025f);yield return null;}w.SmokeFocus("roof");Call(w,"RefreshVisualReadabilityV56");yield return Wait(.20f);
                check("visual.roof.real-reach",w.Selected==roof&&w.CanInteract,"Actual production CanInteract distance/path at explicit roof fixture="+V(w.PlayerPosition)+"; no gate bypass.");
                int draws=Read<int>(hud,"SelectedTargetHudDrawFramesV56");yield return Wait(.12f);yield return new WaitForEndOfFrame();Card("1600",w,hud,draws,check);
                string before=State(w);string notification=w.Model.Message;for(int i=0;i<32;i++){var snapshot=Snapshot(hud);if(snapshot!=null)Requirements(snapshot);}
                check("visual.card.snapshot-pure",before==State(w)&&notification==w.Model.Message,"32 read-only real target/requirements queries preserve inventory, item state, budget, RNG, supplies, result, utilities and notification.");
                var cue=Read<Renderer>(w,"roofSelectionRendererV56");
                check("visual.roof.cue-renderable",Read<bool>(w,"RoofSelectionFeedbackVisibleV56")&&cue&&cue.enabled&&cue.gameObject.activeInHierarchy&&cue.sharedMaterial&&cue.sharedMaterial.shader&&cue.GetComponent<MeshFilter>()&&cue.GetComponent<MeshFilter>().sharedMesh.vertexCount>0&&GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(w.View),cue.bounds),"Actual enabled nonempty selected roof mesh intersects live camera frustum. Screenshot pixels, not this predicate, decide readability.");
                detail.samples.Add(Observe("roof-card-1600",w,hud,boxes));yield return capture("selected-roof-card-v56-1600","Actual roof cue and canonical requirements under right HUD, 1600x900. Explicit selected pose fixture.");
                roof.Tier=1;Call(w,"SyncVisuals",false);yield return Wait(.20f);w.SmokeFocus("roof");Call(w,"RefreshVisualReadabilityV56");yield return new WaitForEndOfFrame();
                var hardware=Read<Dictionary<string,GameObject>>(w,"hardware");var frame=hardware["roof"].GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("Roof_",StringComparison.Ordinal)).ToArray();
                check("visual.roof.upgrade-frame-and-reach",w.CanInteract&&Read<bool>(w,"RoofSelectionFeedbackVisibleV56")&&Read<int>(w,"RoofFramingRendererCountV56")>=5&&frame.Any(r=>r.enabled&&r.gameObject.activeInHierarchy)&&w.RoofAccessR6.EnabledBarrierCount==2,"Explicit Tier1 presentation fixture: live actual timber frame renderers="+frame.Length+"; strict reach and both roof barriers preserved. No paid-work acceptance.");
                detail.samples.Add(Observe("roof-tier1-frame",w,hud,boxes));yield return capture("roof-frame-v56-tier1-1600","Actual tier1 timber frame/cutaway and selected cue; explicit tier fixture. Native owner inspects joins, roof readability and shadows.");
                roof.Tier=tier;Call(w,"SyncVisuals",false);yield return Wait(.12f);w.SmokeFocus("roof");
                yield return resolution(960,540);draws=Read<int>(hud,"SelectedTargetHudDrawFramesV56");yield return Wait(.12f);yield return new WaitForEndOfFrame();Card("960",w,hud,draws,check);
                detail.samples.Add(Observe("roof-card-960",w,hud,boxes));yield return capture("selected-roof-card-v56-960","Actual 960x540 card rows, roof cue and top-right placement. Sole owner inspects text pixels.");
                w.Warp(new Vector3(24,.04f,-18));w.SmokeFocus("roof");Call(w,"RefreshVisualReadabilityV56");yield return Wait(.14f);draws=Read<int>(hud,"SelectedTargetHudDrawFramesV56");yield return Wait(.10f);
                check("visual.roof.distance-hides-card-and-cue",w.Selected==roof&&!w.CanInteract&&Snapshot(hud)==null&&!Read<bool>(hud,"SelectedTargetHudVisibleV56")&&!Read<bool>(w,"RoofSelectionFeedbackVisibleV56")&&Read<int>(hud,"SelectedTargetHudDrawFramesV56")==draws,"Same forced roof target remains selected; actual out-of-range distance blocks fresh snapshot, visible cue/card and further card Repaints.");
                w.Warp(new Vector3(0,.04f,-4.8f));for(int i=0;i<6;i++){w.SmokeMove(Vector2.zero,.025f);yield return null;}w.SmokeFocus("roof");yield return Wait(.12f);
                yield return resolution(1600,900);yield return HelpCycle("1600",KeyCode.F1,w,hud,detail,boxes,check,capture);
                yield return resolution(960,540);yield return HelpCycle("960",KeyCode.Escape,w,hud,detail,boxes,check,capture);
                check("visual.lintel.exact-fields-unchanged",exactLintelSet&&boxes.Length==12&&!string.IsNullOrEmpty(baseline)&&baseline==ColliderStamp(boxes)&&Read<bool>(w,"DoorLintelsPhysicsPreservedV56")&&lintels.All(r=>!r.enabled&&r.GetComponent<PanicPermanentlyHiddenV414>())&&Read<int>(w,"DoorLintelHiddenRendererCountV56")==12&&Read<int>(w,"DoorLintelEnabledColliderCountV56")==12&&w.RoofAccessR6.EnabledBarrierCount==2,"All12 same-owner collider exact fields retained after selection, tier rebuild, far reach, both help transitions/resolutions and power changes; empty-set invariants cannot pass.");
                detail.complete=true;
            }
            finally
            {
                Call(hud,"CloseKeyboardHelpV56");roof.Tier=tier;w.Model.UtilitySafety.Reset();if(gas)Call(w.Model.UtilitySafety,"Complete","gasoff");if(water)Call(w.Model.UtilitySafety,"Complete","wateroff");if(power)Call(w.Model.UtilitySafety,"Complete","poweroff");Call(w,"SyncVisuals",false);w.Warp(original);w.SmokeFocus(focus);Call(w,"RefreshVisualReadabilityV56");
                File.WriteAllText(Path.Combine(w.EvidencePath,"visual-readability-v56-detail.json"),JsonUtility.ToJson(detail,true));
            }
            yield return resolution(1600,900);yield return Wait(.12f);
        }
    }
}
