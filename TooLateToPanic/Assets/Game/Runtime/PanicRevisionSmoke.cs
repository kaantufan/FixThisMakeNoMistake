using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    // One bounded native batch for the changed contracts. All position fixtures are declared.
    public sealed class PanicRevisionSmoke : MonoBehaviour
    {
        [Serializable] public sealed class Row { public string id, detail; public bool pass; }
        [Serializable] public sealed class Receipt
        {
            public bool complete, passed; public string error; public float seconds;
            public List<string> renderStates=new List<string>();
            public int setupWarps; public string evidence = "Native production APIs, real frame clocks/rays/colliders, explicit position fixtures. No OS input or human acceptance.";
            public List<Row> rows = new List<Row>(); public List<string> errors = new List<string>(), captures = new List<string>();
        }
        readonly Receipt result = new Receipt();
        PanicHomeWorld w; PanicHomeHud hud; float began; bool finished;
        PanicInteractionAudit pointerFixture;
        bool guiPending; EventType guiEvent; Vector2 guiPoint; KeyCode guiKey;
        const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
        static readonly FieldInfo ActivePointer=typeof(PanicInteractionAudit).GetField("activeGuiFixture",BindingFlags.NonPublic|BindingFlags.Static);
        static readonly FieldInfo LogicalPointer=typeof(PanicInteractionAudit).GetField("queuedLogicalPointer",Hidden);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--revision-smoke") < 0) return;
            var world = FindFirstObjectByType<PanicHomeWorld>(); if (world) world.gameObject.AddComponent<PanicRevisionSmoke>();
        }
        IEnumerator Start()
        {
            w = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); began = Time.realtimeSinceStartup;
            pointerFixture=gameObject.AddComponent<PanicInteractionAudit>();pointerFixture.enabled=false;
            Application.runInBackground = true; Application.logMessageReceived += OnLog;
            var stack = new Stack<IEnumerator>(); stack.Push(Run());
            while (stack.Count > 0)
            {
                bool more = false; object next = null;
                try { more = stack.Peek().MoveNext(); if(more)next=stack.Peek().Current; }
                catch(Exception ex){result.error=ex.ToString();Check("harness.exception",false,ex.Message);break;}
                if(!more){stack.Pop();continue;}
                if(next is IEnumerator nested)stack.Push(nested);else yield return next;
            }
            result.complete = string.IsNullOrEmpty(result.error); Finish();
        }
        void OnLog(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)result.errors.Add(message);}
        void Check(string id,bool ok,string detail=""){result.rows.Add(new Row{id=id,pass=ok,detail=detail});}
        IEnumerator Delay(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
        void Warp(Vector3 position){w.Warp(position);result.setupWarps++;}
        IEnumerator Ground()
        {
            float until=Time.realtimeSinceStartup+3;int frames=0;
            while((!w.IsGrounded||frames<3)&&Time.realtimeSinceStartup<until)
            {w.SmokeMove(Vector2.zero,Mathf.Min(Time.deltaTime,.04f));frames++;yield return null;}
            if(!w.IsGrounded)throw new InvalidOperationException("Setup fixture could not ground the actual controller.");
        }
        void OnGUI()
        {
            if(!guiPending||Event.current.type!=EventType.Repaint)return;guiPending=false;
            Event prior=new Event(Event.current);Matrix4x4 matrix=GUI.matrix;
            try
            {
                LogicalPointer.SetValue(pointerFixture,guiPoint);ActivePointer.SetValue(null,pointerFixture);
                Event.current=new Event{type=guiEvent,button=0,mousePosition=guiPoint,keyCode=guiKey};
                typeof(PanicHomeHud).GetMethod("OnGUI",Hidden).Invoke(hud,null);
            }
            catch(Exception ex){result.error=ex.ToString();}
            finally{ActivePointer.SetValue(null,null);Event.current=prior;GUI.matrix=matrix;}
        }
        IEnumerator GuiEvent(EventType type,Vector2 point,KeyCode key=KeyCode.None)
        {
            guiEvent=type;guiPoint=point;guiKey=key;guiPending=true;
            float deadline=Time.realtimeSinceStartup+2;
            while(guiPending&&Time.realtimeSinceStartup<deadline)yield return null;
            if(guiPending){guiPending=false;throw new InvalidOperationException("GUI callback did not render.");}
            yield return null;
        }
        IEnumerator Shot(string name)
        {
            string file=Path.Combine(w.EvidencePath,name+".png");
            result.renderStates.Add(name+" width="+Screen.width+" height="+Screen.height+" frame="+Time.frameCount+" page="+w.Notebook.Animation.CurrentPage+" turning="+w.Notebook.Animation.Turning+" progress="+w.Notebook.Animation.TurnProgress+" bagProgress="+w.BagCameraTransitionProgress);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(file);result.captures.Add(file);yield return null;
        }
        IEnumerator Run()
        {
            if(!w||string.IsNullOrEmpty(w.EvidencePath))throw new InvalidOperationException("Fresh isolated evidence is required.");
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            foreach(string id in PanicRevisionModelChecks.Run())Check("model."+id,true);
            foreach(string asset in new[]{"AirConditioner","BreakerBox","FireCabinet","GasValve","Refrigerator","WaterHeater","WaterValve"})
            {
                var visual=PanicCuratedAssetArt.CreateDisplay(asset,1);
                var mechanism=visual?visual.GetComponent<PanicCuratedMechanism>():null;
                Check("animation.resources."+asset,mechanism&&mechanism.OpenClip&&mechanism.CloseClip,"Native-loaded sanitized prefab and real clips; isolated visual fixture");
                if(mechanism)
                {
                    visual.transform.position=new Vector3(100,100,100);
                    PanicCuratedAssetArt.SyncVisualState(visual,false,false,false,0);
                    var pivots=visual.GetComponentsInChildren<Transform>(true).Where(t=>(t.name.EndsWith("_Pivot")||t.name.EndsWith("_Hinge"))&&t.name!="MeterTurbine_Pivot").ToArray();
                    var baseline=pivots.Select(t=>t.localRotation).ToArray();
                    PanicCuratedAssetArt.SyncVisualState(visual,true,true,false,.3f);
                    Check("animation.sample-moves."+asset,pivots.Where((t,i)=>Quaternion.Angle(t.localRotation,baseline[i])>.5f).Any(),"Explicit .3s phase sample through passive production adapter; not an OS/world interaction claim");
                    PanicCuratedAssetArt.SyncVisualState(visual,false,false,false,2);
                    Check("animation.reverse-settles."+asset,pivots.Select((t,i)=>Quaternion.Angle(t.localRotation,baseline[i])<.1f).All(v=>v));
                }
                if(visual){visual.SetActive(false);Destroy(visual);}
            }
            w.StartGame(false);w.FullFlowScriptedInput=true;yield return BagSettled(true);
            Check("fps.initial-stage",w.BagFirstPersonActive,"Normal free game begins its bag stage through the real cinematic");
            Check("fps.initial-finish",w.FinishBagPreparation());yield return BagSettled(false);
            Check("bunker.basement",w.Model.Items.Find(i=>i.Id=="bunker").Position==PanicHomeWorld.BunkerPosition&&PanicHomeWorld.BunkerPosition.y==-3.2f);
            Check("bag.physical",FindObjectsByType<HomePick>(FindObjectsSortMode.None).Count(p=>p.Id=="@bagstation")==1);
            Check("pickup.3d",FindObjectsByType<HomePick>(FindObjectsSortMode.None).Count(p=>p.Kind=="supply")==w.Expedition.WorldSupplies.Count);
            w.SmokeFocus("cabinet");Warp(w.Expedition.Sources.Find(s=>s.Id=="cabinet").Position+Vector3.back*1.4f);yield return Ground();
            int before=w.Expedition.Count("screws");w.ToggleContainer();yield return Delay(.25f);
            int after=w.Expedition.Count("screws");Check("container.auto-pool",after>before&&!w.LootVisible);
            w.ToggleContainer();w.ToggleContainer();Check("container.no-reroll",w.Expedition.Count("screws")==after);
            float time=w.Model.TimeRemaining;hud.SetPaused(true);yield return Delay(.35f);
            Check("pause.campaign-native",w.Model.IsPaused&&Mathf.Abs(w.Model.TimeRemaining-time)<.001f&&!w.Model.CheatedRun);
            yield return Shot("pause");
            yield return GuiEvent(EventType.KeyDown,Vector2.zero,KeyCode.D);Check("pause.keyboard-right",hud.PauseChoice==1);
            yield return GuiEvent(EventType.KeyDown,Vector2.zero,KeyCode.A);Check("pause.keyboard-left",hud.PauseChoice==0);
            yield return GuiEvent(EventType.KeyDown,Vector2.zero,KeyCode.KeypadEnter);Check("pause.keyboard-confirm",!hud.Paused&&!w.Model.IsPaused);
            Check("title.horizontal-layout",PanicHomeHud.NavigateTitleChoice(2,KeyCode.D)==3&&PanicHomeHud.NavigateTitleChoice(3,KeyCode.A)==2&&PanicHomeHud.NavigateTitleChoice(0,KeyCode.A)==0);
            yield return Delay(.1f);

            // Kitchen wall: actual controller jump and movement cannot cross x=2.
            Warp(new Vector3(1.4f,.04f,-1f));yield return Ground();w.Jump();
            float end=Time.realtimeSinceStartup+.8f;
            while(Time.realtimeSinceStartup<end){MoveWorld(Vector3.right);yield return null;}
            Check("collision.kitchen-jump",w.PlayerPosition.x<1.84f,"Actual CharacterController, one jump attempt; no route warp");
            Warp(new Vector3(1.4f,3.24f,3.5f));yield return Ground();w.Jump();end=Time.realtimeSinceStartup+.8f;
            while(Time.realtimeSinceStartup<end){MoveWorld(Vector3.right);yield return null;}
            Check("collision.bath-jump",w.PlayerPosition.x<1.84f,"Actual upper-floor bathroom west wall");

            Warp(new Vector3(7.15f,.04f,2.2f));yield return Ground();w.SmokeFocus("door_garage_inner");w.ToggleDoor();yield return Delay(.8f);
            end=Time.realtimeSinceStartup+.7f;while(Time.realtimeSinceStartup<end){MoveWorld(Vector3.right);yield return null;}
            Check("garage.internal-route",w.PlayerPosition.x>8.5f,"Real doorway opening and controller traversal; furniture/rails stay physical");
            yield return Shot("garage-interior");
            yield return CheckFacades((GameObject)typeof(PanicHomeWorld).GetField("residenceGarage",Hidden).GetValue(w),"garage",0);
            Warp(new Vector3(0,3.24f,-2));yield return Ground();yield return null;
            var floors=(List<GameObject>)typeof(PanicHomeWorld).GetField("floorVisuals",Hidden).GetValue(w);
            yield return CheckFacades(floors[2],"upper",3.2f);
            var colliders=FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>c.name.StartsWith("Collision_solid_")).ToArray();
            Check("collision.cutaway-keeps-physics",colliders.All(c=>c.enabled&&c.gameObject.activeInHierarchy));

            // First-person begins at the exact grounded actor pose, with the same controller instance.
            var water=w.Expedition.FindWorldSupply("supply_water_kitchen");
            Warp(new Vector3(4.1f,.04f,3.1f));yield return Ground();
            Vector3 actor=w.PlayerPosition;var controller=FindFirstObjectByType<CharacterController>();
            Check("fps.begin",w.BeginBagPreparation());yield return BagSettled(true);
            Check("fps.same-position",Vector3.Distance(actor,w.PlayerPosition)<.00001f&&w.PlayerControllerEnabled&&FindFirstObjectByType<CharacterController>()==controller);
            Check("fps.perspective",w.BagFirstPersonActive&&!w.View.orthographic&&Mathf.Abs(w.View.fieldOfView-62)<.01f);
            var partitions=FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>m.name=="solid_east_south"||m.name=="solid_east_north"||m.name=="solid_bath_west"||m.name=="solid_bath_south_left"||m.name=="solid_bath_south_right").ToArray();
            Check("fps.full-height-partition-bottom",partitions.Length==5&&partitions.All(m=>{var r=m.GetComponent<Renderer>();float floor=m.name.Contains("bath")?3.2f:0;return r&&Mathf.Abs(r.bounds.min.y-floor)<.02f&&Mathf.Abs(r.bounds.max.y-floor-3.2f)<.02f;}),"Actual native renderer bounds, separately from persistent colliders");
            AimAt(water.Position+Vector3.up*.1f);yield return Delay(.12f);
            Check("fps.water-ray-focus",(string)typeof(PanicHomeWorld).GetField("focusId",Hidden).GetValue(w)==water.Id,RaySummary(water));
            yield return Shot("fps-kitchen-hands");
            int stock=w.Expedition.Count("water");int remaining=water.Count;
            w.StepBagLook(new Vector2(80,0));w.HandleKey(KeyCode.F);
            Check("fps.stale-focus-rejected",water.Count==remaining&&w.Expedition.Count("water")==stock,"Turn away and press F in the same frame without fixture focus refresh");
            AimAt(water.Position+Vector3.up*.1f);
            w.HandleKey(KeyCode.E);yield return Delay(.18f);yield return Shot("fps-gather-contact");
            Check("fps.water-diagnostics",water.Count==0,RaySummary(water));
            Check("fps.center-ray-pickup",water.Count==0&&w.Expedition.Count("water")==stock+remaining,"Center viewport ray + production E transaction");
            w.HandleKey(KeyCode.E);Check("fps.no-double-pickup",w.Expedition.Count("water")==stock+remaining);
            int budgetBeforeWork=w.Model.Budget;w.BeginWork("fire",PanicItemActionRules.FireStationId);w.QuickAction(1);
            Check("fps.only-bag-work",!w.WorkActive&&w.Model.Budget==budgetBeforeWork);
            Warp(new Vector3(6.5f,.04f,3.1f));yield return Ground();
            AimAt(w.Model.Items.Find(i=>i.Id=="fridge").Position+Vector3.up*1.05f);yield return Delay(.1f);
            var fridgeMotion=FindObjectsByType<PanicCuratedMechanism>(FindObjectsSortMode.None).FirstOrDefault(m=>m.AssetName=="Refrigerator");
            var door=fridgeMotion?fridgeMotion.SampleRoot.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="CabinetDoor_Hinge"):null;
            Quaternion closed=door?door.localRotation:Quaternion.identity;
            w.HandleKey(KeyCode.E);yield return Delay(.25f);
            Check("animation.fridge-real-open",door&&Quaternion.Angle(closed,door.localRotation)>3&&w.Expedition.Sources.Find(s=>s.Id=="fridge").Open,"Actual world container, center-ray E and real frame clock");
            yield return Shot("fps-fridge-mid-open");
            w.HandleKey(KeyCode.E);yield return Delay(.75f);
            Check("animation.fridge-real-reverse",door&&Quaternion.Angle(closed,door.localRotation)<.2f&&!w.Expedition.Sources.Find(s=>s.Id=="fridge").Open);
            yield return Delay(.5f);yield return Shot("fps-gather-rest");
            var hands=FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault(c=>c.name=="Emergency bag hands overlay camera");
            Check("fps.hands-overlay",hands&&hands.enabled&&hands.cullingMask==(1<<28));
            hud.SetPaused(true);time=w.Model.TimeRemaining;yield return Delay(.3f);
            Check("fps.pause-clock",Mathf.Abs(time-w.Model.TimeRemaining)<.001f&&!hands.enabled);hud.SetPaused(false);yield return Delay(.1f);
            w.ToggleInventory();yield return Delay(.15f);Check("fps.inventory-cursor",w.InventoryOpen&&Cursor.lockState==CursorLockMode.None);
            int slot=w.Expedition.Inventory.FindIndex(s=>s.Id=="water"&&s.Count>0);
            if(slot<0)throw new InvalidOperationException("No collected water for the GUI transaction fixture; see FPS ray diagnostics.");
            yield return GuiEvent(EventType.MouseDown,hud.InventorySlotBounds(false,slot).center);Check("inventory.drag-begins",hud.InventoryDragging);
            yield return GuiEvent(EventType.MouseUp,hud.InventorySlotBounds(true,0).center);
            Check("inventory.drag-to-bag",!hud.InventoryDragging&&w.Expedition.Bag[0].Id=="water"&&w.Model.PackedWater==remaining);
            yield return Shot("inventory-drag-layout");
            yield return GuiEvent(EventType.MouseDown,hud.InventorySlotBounds(true,0).center);
            yield return GuiEvent(EventType.MouseUp,hud.InventorySlotBounds(false,slot).center);
            Check("inventory.drag-unpack",w.Model.PackedWater==0&&w.Expedition.Inventory[slot].Count==remaining);
            w.ToggleInventory();yield return Delay(.1f);

            // The journal uses actual state and does not mutate materials or budget.
            int budget=w.Model.Budget;int materials=w.Expedition.Count("screws");
            w.Notebook.RequestOpen();yield return Delay(.6f);time=w.Model.TimeRemaining;yield return Delay(.3f);
            Check("notebook.modal-clock",w.Notebook.IsVisible&&w.NotebookBlocksInput&&Mathf.Abs(time-w.Model.TimeRemaining)<.001f);
            Check("notebook.live-values",w.Notebook.Snapshot.PackedWater==w.Model.PackedWater&&w.Notebook.Snapshot.InventoryCapacity==12);
            yield return Shot("notebook-rooms");
            w.Notebook.RequestPage(PanicNotebookPage.Flood);yield return Delay(.18f);yield return Shot("notebook-turn-quarter");
            yield return Delay(.16f);yield return Shot("notebook-turn-middle");
            w.Notebook.RequestPage(PanicNotebookPage.Inventory);
            float pageDeadline=Time.realtimeSinceStartup+2*PanicNotebookAnimation.TurnSeconds+.6f;
            while((w.Notebook.Animation.Turning||w.Notebook.Animation.CurrentPage!=PanicNotebookPage.Inventory)&&Time.realtimeSinceStartup<pageDeadline)yield return null;
            Check("notebook.latest-page",w.Notebook.Animation.CurrentPage==PanicNotebookPage.Inventory);
            yield return Shot("notebook-inventory");
            Check("notebook.readonly",budget==w.Model.Budget&&materials==w.Expedition.Count("screws"));
            w.Notebook.RequestClose();yield return Delay(.7f);
            Check("notebook.close-releases",!w.NotebookBlocksInput&&!w.Notebook.IsVisible);
            yield return PanicNotebookVerification.Run(w,w.Notebook,(ok,name)=>Check("notebook.native-helper."+name,ok));
            w.Notebook.RequestOpen();yield return Delay(.5f);
            foreach(PanicNotebookPage page in Enum.GetValues(typeof(PanicNotebookPage)))
            {
                w.Notebook.RequestPage(page);float deadline=Time.realtimeSinceStartup+2*PanicNotebookAnimation.TurnSeconds+.6f;
                while((w.Notebook.Animation.Turning||w.Notebook.Animation.CurrentPage!=page)&&Time.realtimeSinceStartup<deadline)yield return null;
                Check("notebook.render-settled."+page,!w.Notebook.Animation.Turning&&w.Notebook.Animation.CurrentPage==page);
                yield return Shot("notebook-page-"+page);
            }
            w.Notebook.RequestClose();yield return Delay(.5f);
            Screen.SetResolution(960,540,false);yield return Delay(.4f);
            w.Notebook.RequestOpen();w.Notebook.RequestPage(PanicNotebookPage.Inventory);yield return Delay(.9f);
            yield return Shot("notebook-960x540");w.Notebook.RequestClose();yield return Delay(.45f);
            Screen.SetResolution(1600,900,false);yield return Delay(.4f);
            actor=w.PlayerPosition;Check("fps.finish",w.FinishBagPreparation());yield return BagSettled(false);
            Check("fps.return-same-position",!w.BagCameraOwnsView&&w.View.orthographic&&Vector3.Distance(actor,w.PlayerPosition)<.00001f);
            w.SmokeFocus(w.Expedition.WorldSupplies.First(s=>s.Count>0).Id);Check("pickup.iso-stage-reject",!w.TakeWorldSupply());

            Warp(new Vector3(-4,-3.16f,1));yield return Ground();yield return Delay(1.2f);
            float dark=RenderSettings.ambientSkyColor.maxColorComponent;yield return Shot("basement-shelter");
            Check("basement.dark",w.ActiveFloor==-1&&dark<.09f);
            Warp(new Vector3(-3,.04f,-2));yield return Ground();yield return Delay(1.2f);
            Check("basement.restore",RenderSettings.ambientSkyColor.maxColorComponent>.4f);
            yield return Shot("isometric-restored");
            var targets=(Dictionary<string,GameObject>)typeof(PanicHomeWorld).GetField("residenceTargets",Hidden).GetValue(w);
            var hearth=targets["@fireplace"];
            var flame=(GameObject)typeof(PanicHomeWorld).GetField("hearthFlame",Hidden).GetValue(w);
            Check("fireplace.native-model",hearth.GetComponentsInChildren<MeshFilter>(true).Length>10&&flame&&Vector3.Distance(flame.transform.position,hearth.transform.position)<.6f,"Authored arched masonry plus actual linked flame origin; visual frame separately reviewed");
            Check("fireplace.opens-into-room",Vector3.Dot(hearth.transform.TransformDirection(Vector3.back),Vector3.right)>.99f&&Vector3.Distance(flame.transform.position,hearth.transform.TransformPoint(new Vector3(0,.13f,-.22f)))<.001f,"Actual mesh front local-Z faces east into the room; flame is inside its authored opening");
            Warp(hearth.transform.position+Vector3.right+Vector3.forward*1.4f+Vector3.up*.04f);yield return Ground();
            w.SmokeFocus("@fireplace");w.Interact();yield return Delay(.1f);
            Check("fireplace.extinguish",!flame.activeSelf,"Visibility refresh must preserve the switched-off flame across real frames");w.Interact();yield return Delay(.2f);
            Check("fireplace.ignite",flame.activeSelf);
            w.View.transform.position=hearth.transform.position+new Vector3(3,1.8f,-1);w.View.transform.LookAt(hearth.transform.position+Vector3.up*.85f);w.View.orthographicSize=1.5f;
            yield return Shot("fireplace-closeup");

            w.StartJuryDemo();w.FullFlowScriptedInput=true;yield return Delay(.15f);
            float elapsed=w.JuryDemo.ElapsedSeconds;time=w.Model.TimeRemaining;hud.SetPaused(true);yield return Delay(.4f);
            Check("pause.shortflow-native",Mathf.Abs(elapsed-w.JuryDemo.ElapsedSeconds)<.001f&&Mathf.Abs(time-w.Model.TimeRemaining)<.001f);
            hud.SetPaused(false);yield return Delay(.15f);Check("pause.shortflow-resume",w.JuryDemo.ElapsedSeconds>elapsed);
            Check("no-cheat",!w.Model.CheatedRun);
        }
        IEnumerator CheckFacades(GameObject root,string prefix,float floor)
        {
            Vector3 position=w.View.transform.position;Quaternion rotation=w.View.transform.rotation;
            string[] sides={"North","East","South","West"};
            Vector3[] directions={Vector3.forward,Vector3.right,Vector3.back,Vector3.left};
            for(int i=0;i<4;i++)
            {
                w.View.transform.position=w.PlayerPosition+directions[i]*12+Vector3.up*8;w.View.transform.LookAt(w.PlayerPosition+Vector3.up);
                typeof(PanicHomeWorld).GetMethod("ApplyFloorVisibility",Hidden).Invoke(w,null);
                var near=root.transform.Find("Facade"+sides[i]);var far=root.transform.Find("Facade"+sides[(i+2)%4]);
                var nearRenderers=near?near.GetComponentsInChildren<Renderer>(true).Where(r=>r.bounds.max.y>floor+.48f).ToArray():Array.Empty<Renderer>();
                var farRenderers=far?far.GetComponentsInChildren<Renderer>(true).Where(r=>r.bounds.max.y>floor+.48f).ToArray():Array.Empty<Renderer>();
                Check("cutaway."+prefix+"."+sides[i],nearRenderers.Length>0&&nearRenderers.All(r=>!r.enabled)&&farRenderers.Any(r=>r.enabled),"Actual renderer enabled state: near="+nearRenderers.Length+" far="+farRenderers.Length+"; controller/colliders unchanged");
                yield return Shot("cutaway-"+prefix+"-"+sides[i]);
            }
            w.View.transform.SetPositionAndRotation(position,rotation);typeof(PanicHomeWorld).GetMethod("ApplyFloorVisibility",Hidden).Invoke(w,null);
        }
        IEnumerator BagSettled(bool firstPerson)
        {
            float deadline=Time.realtimeSinceStartup+5;
            while((w.BagCameraTransitionActive||w.BagFirstPersonActive!=firstPerson)&&Time.realtimeSinceStartup<deadline)yield return null;
            if(w.BagCameraTransitionActive||w.BagFirstPersonActive!=firstPerson)throw new InvalidOperationException("Bag camera did not settle in5real seconds. Progress="+w.BagCameraTransitionProgress);
            yield return null;
        }
        string RaySummary(PanicWorldSupply supply)
        {
            var hits=Physics.RaycastAll(w.View.ViewportPointToRay(new Vector3(.5f,.5f)),2.4f,~((1<<28)|(1<<30)),QueryTriggerInteraction.Collide);
            Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            return "actor="+w.PlayerPosition+" supply="+supply.Position+" focus="+(string)typeof(PanicHomeWorld).GetField("focusId",Hidden).GetValue(w)+" CanInteract="+w.CanInteract+" floor="+w.ActiveFloor+" mode="+w.BagFirstPersonActive+" ray="+string.Join(";",hits.Select(h=>h.collider.name+":"+(h.collider.GetComponentInParent<HomePick>()?.Id??"-")+":"+h.distance));
        }
        void MoveWorld(Vector3 direction)
        {
            Vector3 forward=w.View.transform.forward;forward.y=0;forward.Normalize();Vector3 right=w.View.transform.right;right.y=0;right.Normalize();
            w.SmokeMove(new Vector2(Vector3.Dot(direction,right),Vector3.Dot(direction,forward)),Mathf.Min(Time.deltaTime,.04f));
        }
        void AimAt(Vector3 point)
        {
            Vector3 d=(point-w.View.transform.position).normalized;
            float yaw=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,pitch=-Mathf.Asin(d.y)*Mathf.Rad2Deg;
            float oldYaw=(float)typeof(PanicHomeWorld).GetField("bagYaw",Hidden).GetValue(w),oldPitch=(float)typeof(PanicHomeWorld).GetField("bagPitch",Hidden).GetValue(w);
            w.StepBagLook(new Vector2(Mathf.DeltaAngle(oldYaw,yaw)/2f,(oldPitch-pitch)/1.7f));
            typeof(PanicHomeWorld).GetMethod("RefreshBagRayFocus",Hidden).Invoke(w,null);
        }
        void Finish()
        {
            if(finished)return;finished=true;result.seconds=Time.realtimeSinceStartup-began;result.passed=result.complete&&result.errors.Count==0&&result.rows.All(r=>r.pass);
            File.WriteAllText(Path.Combine(w.EvidencePath,"revision-smoke.json"),JsonUtility.ToJson(result,true));
            Application.logMessageReceived-=OnLog;Application.Quit(result.passed?0:2);
        }
    }
}
