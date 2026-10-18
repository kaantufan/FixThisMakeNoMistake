using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicDirectActionsSmoke : MonoBehaviour
    {
        [Serializable] public sealed class Row { public string name, detail; public bool pass; }
        [Serializable] public sealed class Report
        {
            public bool passed, completed; public int total, passedChecks; public float seconds;
            public string error, scoreBefore, scoreAfter, realScoreBefore, realScoreAfter;
            public string scope = "Opt-in native semantic/IMGUI callbacks with explicit target-position fixtures. Real frame time, exact costs and actual exported art. No OS input or human acceptance.";
            public List<Row> rows = new List<Row>(); public List<string> errors = new List<string>(), captures = new List<string>();
            public List<string> setupSettles = new List<string>();
        }
        readonly Report report = new Report();
        PanicHomeWorld world; PanicHomeHud hud; PanicInteractionAudit pointerFixture;
        float began; string output, realScore; bool finished, pending; Vector2 click;
        static readonly BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
        static readonly FieldInfo ActivePointer = typeof(PanicInteractionAudit).GetField("activeGuiFixture", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly FieldInfo LogicalPointer = typeof(PanicInteractionAudit).GetField("queuedLogicalPointer", Hidden);
        static readonly MethodInfo HudGui = typeof(PanicHomeHud).GetMethod("OnGUI", Hidden);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--direct-actions-smoke") < 0) return;
            var world = FindFirstObjectByType<PanicHomeWorld>();
            if (world) { Application.runInBackground = true; world.gameObject.AddComponent<PanicDirectActionsSmoke>(); }
        }
        IEnumerator Start()
        {
            world = GetComponent<PanicHomeWorld>(); hud = GetComponent<PanicHomeHud>(); began = Time.realtimeSinceStartup;
            if (!world || string.IsNullOrEmpty(world.EvidencePath)) { Debug.LogError("Direct smoke requires isolated evidence"); Application.Quit(2); yield break; }
            output = Path.Combine(world.EvidencePath, "direct-actions.json");
            realScore = Path.Combine(Application.persistentDataPath, "TooLateToPanic", "highscores.json");
            report.scoreBefore = Hash(PanicModel.ScorePathOverride); report.realScoreBefore = Hash(realScore);
            Application.logMessageReceived += OnLog;
            pointerFixture = gameObject.AddComponent<PanicInteractionAudit>(); pointerFixture.enabled = false;
            var bodies = new Stack<IEnumerator>(); bodies.Push(Run());
            while (bodies.Count > 0 && !finished)
            {
                bool more = false; object current = null;
                try { more = bodies.Peek().MoveNext(); if (more) current = bodies.Peek().Current; }
                catch (Exception ex) { report.error = ex.ToString(); Check("harness exception", false, ex.Message); break; }
                if (!more) { bodies.Pop(); continue; }
                if (current is IEnumerator nested) bodies.Push(nested); else yield return current;
            }
            report.completed = string.IsNullOrEmpty(report.error); Finish();
        }
        void OnGUI()
        {
            if (!pending || Event.current.type != EventType.Repaint) return;
            pending = false; Event prior = new Event(Event.current); Matrix4x4 matrix = GUI.matrix;
            try
            {
                GUI.matrix = Matrix4x4.identity;
                LogicalPointer.SetValue(pointerFixture, click); ActivePointer.SetValue(null, pointerFixture);
                Event.current = new Event { type = EventType.MouseDown, button = 0, mousePosition = click };
                HudGui.Invoke(hud, null);
            }
            catch (Exception ex) { report.error = ex.ToString(); }
            finally { ActivePointer.SetValue(null, null); Event.current = prior; GUI.matrix = matrix; }
        }
        IEnumerator Run()
        {
            Check("fresh isolated score profile", report.scoreBefore == "absent" && Guid.TryParse(Path.GetFileName(world.EvidencePath).Substring(Path.GetFileName(world.EvidencePath).Length - 36), out _));
            while (!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            world.FullFlowScriptedInput = true;
            PureWork();
            world.StartJuryDemo(); yield return Place(new Vector3(6.5f, .05f, 3.25f), "fridge");
            world.HandleKey(KeyCode.E); world.HandleKey(KeyCode.F);
            int water = world.Expedition.Inventory.FindIndex(s => s.Id == "water" && s.Count > 0); world.InventorySelect(water); world.InventoryUse();
            yield return ClickLogical(new Vector2(1337,247)); yield return Wait(.12f);
            Check("real anchor offers two direct screw targets", world.WorkActive && world.Expedition.Work.TotalSteps == 2 && !world.Expedition.Work.IsWorldConstruction, "grounded="+world.IsGrounded+"; stage="+world.JuryDemo.Stage+"; "+world.Model.Message);
            Vector3 first = world.WorkTargetViewport(0), second = world.WorkTargetViewport(1);
            Check("actual screw-head projections inside render texture", first.z > 0 && second.z > 0 && first.x > 0 && first.x < 1 && first.y > 0 && first.y < 1 && second.x > 0 && second.x < 1 && second.y > 0 && second.y < 1 && Vector2.Distance(first, second) > .05f, first + "; " + second);
            Check("future screw click rejected", !world.ClickWorkTarget(1) && !world.Expedition.Work.IsInstalling);
            yield return Capture("01-click-screws");
            yield return Click(0);
            Check("genuine OnGUI screw click queues installation", world.Expedition.Work.IsInstalling);
            Check("busy repeated click rejected", !world.ClickWorkTarget(0) && !world.ClickWorkTarget(1));
            yield return Wait(.28f); yield return Capture("02-screw-seating");
            Check("no cost before both screws finish", world.Model.Budget == 1000 && world.Expedition.Count("screws") == 2);
            yield return Wait(.38f); yield return Click(1); yield return Wait(.68f);
            Check("click-to-install commits exact recipe once", Item("fridge").Anchored && world.Model.Budget == 940 && world.Expedition.Count("screws") == 0 && world.Expedition.Count("bracket") == 0);
            world.CompleteWork(); Check("duplicate completion inert", world.Model.Budget == 940);
            yield return Capture("03-anchored");

            world.StartJuryDemo(); yield return Place(new Vector3(6.5f, .05f, 3.25f), "fridge");
            world.QuickAction(1); yield return Wait(.12f); world.ClickWorkTarget(0); yield return Wait(.30f);
            Check("cancel fixture reaches an installing real screw", world.WorkActive && world.Expedition.Work.IsInstalling);
            world.CancelWork(); yield return null;
            Check("cancel midway returns with stock and budget intact", !world.WorkActive && world.Model.Budget == 1000 && world.Expedition.Count("screws") == 2 && world.Expedition.Count("bracket") == 1 && !Item("fridge").Anchored);
            Check("bed has no unrelated protection action", string.IsNullOrEmpty(PanicItemActionRules.PrimaryAction(Item("bed"))) && !world.Expedition.CanBeginWork("fire", "bed"));
            Check("arbitrary fire targets rejected", !world.Expedition.CanBeginWork("fire", "nonexistent_station"));

            world.StartDisasterSimulation(DisasterType.Fire); yield return Place(new Vector3(.9f, .05f, -2.2f), PanicItemActionRules.FireStationId);
            int budget = world.Model.Budget; world.HandleKey(KeyCode.E); yield return Wait(.25f);
            Check("extinguisher uses dedicated in-world station", world.WorldConstructionVisible && world.HasConstructionPreview && !world.Expedition.Work.Complete);
            yield return Capture("04-station-installing"); yield return Wait(.40f);
            Check("station consumes actual extinguisher once", world.Model.FireToolCount == 1 && world.Expedition.IsWorkCommitted("fire", PanicItemActionRules.FireStationId) && world.Expedition.Count("extinguisher") == 0 && world.Model.Budget == budget - 75);
            world.HandleKey(KeyCode.E); Check("completed station cannot duplicate benefit", !world.WorkActive && world.Model.FireToolCount == 1); yield return Capture("05-station-ready");

            yield return Reinforcement("window_north", new Vector3(4f, .05f, 7.1f));
            yield return Reinforcement("wall_north", new Vector3(0, .05f, 7.1f));

            world.StartDisasterSimulation(DisasterType.Flood); yield return Place(new Vector3(-4, -3.15f, 3), "basement_pantry");
            world.HandleKey(KeyCode.E); int kit = world.OpenSource.Contents.FindIndex(s => s.Id == "seedling" && s.Count > 0); world.SelectLoot(kit); world.HandleKey(KeyCode.F);
            Check("adult tree-row kits gathered from real source", world.Expedition.Count("seedling") == 4 && !PanicExpedition.DisplayName("seedling").Contains("Fidan"));
            foreach (var tree in new[] { new { id="plant_west1", point=new Vector3(-10,.05f,4) },new { id="plant_west2",point=new Vector3(-12.3f,.05f,-1.4f) },new { id="plant_east",point=new Vector3(11,.05f,8) } })
            {
                yield return Place(tree.point, tree.id); world.HandleKey(KeyCode.E); yield return Wait(.68f);
                Check("regional mature tree row committed " + tree.id, world.Expedition.IsWorkCommitted("plant", tree.id));
                yield return Capture("tree-" + tree.id);
            }
            Check("three regional rows and twelve physical trunks", world.InstalledTreeRows == 3 && world.Model.TreeCount == 3 && FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None).Count(c => c.name == "Tree trunk collision") == 12 && world.Expedition.Count("seedling") == 1 && world.Model.Budget == 910);
            world.HandleKey(KeyCode.E); Check("completed region cannot charge twice", !world.WorkActive && world.Model.Budget == 910);

            world.ReturnToMenu(); world.StartDisasterSimulation(DisasterType.Storm); yield return Place(new Vector3(0,.05f,-16), null);
            Vector3 right = world.View.transform.right; right.y = 0; right.Normalize(); Vector3 forward = world.View.transform.forward; forward.y = 0; forward.Normalize();
            Vector2 input = new Vector2(Vector3.Dot(Vector3.right, right), Vector3.Dot(Vector3.right, forward));
            float until = Time.realtimeSinceStartup + .3f; while (Time.realtimeSinceStartup < until) { world.SmokeMove(input, Time.deltaTime); yield return null; }
            float x = world.PlayerPosition.x, start = Time.realtimeSinceStartup; until = start + 1f;
            while (Time.realtimeSinceStartup < until) { world.SmokeMove(input, Time.deltaTime); yield return null; }
            float velocity = (world.PlayerPosition.x - x) / (Time.realtimeSinceStartup - start);
            Check("real walking speed increased by 1.35", Mathf.Abs(velocity - PanicHomeWorld.WalkSpeed) < .22f, "measured=" + velocity + "; walk=" + PanicHomeWorld.WalkSpeed);
            Check("run speed and locomotion playback increased together", Mathf.Abs(PanicHomeWorld.RunSpeed - 5.67f) < .001f && Mathf.Abs(world.CharacterPlaybackRate - 1.35f) < .001f);
            var player = FindObjectsByType<Animator>(FindObjectsSortMode.None).First(a => a.transform.parent && a.transform.parent.name == "Survivor");
            Check("humanoid and retained clips valid", player.avatar && player.avatar.isValid && player.avatar.isHuman && !player.applyRootMotion && player.runtimeAnimatorController.animationClips.Length >= 4);
            Check("original character appearance retained after user cancellation", player.transform.Find("PanicWorkwear_V2") && !player.transform.Find("PanicWorkwear_V3") && !player.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Any(m=>m&&m.HasProperty("_MainTex")&&m.mainTexture&&m.mainTexture.name.Contains("Superhero")));
            yield return CharacterPictures();
            world.ReturnToMenu(); yield return Capture("99-final-title");
        }
        void PureWork()
        {
            var work = new PanicWorkSession(); work.Begin("anchorwall", "fridge");
            for (int i=0;i<12;i++) work.Tick(.1f, 1, true, true, true);
            Check("old timing and keyboard sequences never progress work", work.Progress == 0 && work.CompletedSteps == 0 && !work.IsInstalling);
            Check("wrong screw indices rejected", !work.TryClick(-1) && !work.TryClick(1) && !work.TryClick(3));
            Check("first screw click accepted exactly once", work.TryClick(0) && !work.TryClick(0));
            for(int i=0;i<5;i++)work.Tick(.1f,0,false,false,false);
            Check("screw installs for 0.5-0.7 seconds", work.IsInstalling && !work.Complete && Mathf.Abs(work.InstallSeconds-.6f)<.001f);
            for(int i=0;i<2;i++)work.Tick(.1f,0,false,false,false);
            Check("one screw cannot complete both", work.CompletedSteps==1&&!work.Complete&&work.NextClickTarget==1&&!work.TryClick(0));
            work.TryClick(1); for(int i=0;i<7;i++)work.Tick(.1f,0,false,false,false);
            Check("two screw clicks finish installation", work.Complete&&!work.TryClick(1));
        }
        IEnumerator Reinforcement(string id, Vector3 point)
        {
            world.StartDisasterSimulation(DisasterType.Storm); yield return Place(point,id);
            int budget=world.Model.Budget; yield return ClickLogical(new Vector2(1337,247)); yield return Wait(.25f);
            Check("in-world reinforcement assembles " + id, world.WorldConstructionVisible && world.HasConstructionPreview && Item(id).Tier==0, "canInteract="+world.CanInteract+"; position="+world.PlayerPosition+"; "+world.Model.Message);
            yield return Capture(id+"-assembly"); yield return Wait(.40f);
            Check("reinforced asset and exact cost " + id, Item(id).Tier==1 && world.InstalledMeasureRenderers(id)>0 && world.Model.Budget==budget-120 && !world.WorkActive && !world.HasConstructionPreview);
            yield return Capture(id+"-reinforced");
        }
        IEnumerator Place(Vector3 point,string focus)
        {
            world.Warp(point);
            // Warp alone does not ground a CharacterController. Settle through real physics;
            // the short-flow onboarding deliberately requires grounded movement state.
            float started=Time.realtimeSinceStartup,minimum=started+.20f,deadline=started+2f;int frames=0;
            do{world.SmokeMove(Vector2.zero,Mathf.Min(Time.deltaTime,.04f));frames++;yield return null;}
            while(frames<3||((Time.realtimeSinceStartup<minimum||!world.IsGrounded)&&Time.realtimeSinceStartup<deadline));
            report.setupSettles.Add("target="+point+"; actual="+world.PlayerPosition+"; grounded="+world.IsGrounded+"; frames="+frames+"; wall="+(Time.realtimeSinceStartup-started)+"; real controller dt capped.04; no collider/cost changes");
            if(focus!=null)world.SmokeFocus(focus); yield return null;
        }
        IEnumerator Wait(float seconds)
        {
            float until=Time.realtimeSinceStartup+seconds;
            while(Time.realtimeSinceStartup<until){if(world.WorkActive)world.StepWork(Mathf.Min(Time.deltaTime,.08f),0,false,false,false);yield return null;}
        }
        IEnumerator Click(int index)
        {
            Rect rect=hud.WorkClickTargetBounds(index); Check("click target has real projected hit rectangle "+index,rect.width>0&&rect.height>0);
            click=rect.center;pending=true;float until=Time.realtimeSinceStartup+1f;while(pending&&Time.realtimeSinceStartup<until)yield return null;
            Check("native IMGUI callback dispatched "+index,!pending&&string.IsNullOrEmpty(report.error));yield return null;
        }
        IEnumerator ClickLogical(Vector2 position)
        {
            click=position;pending=true;float until=Time.realtimeSinceStartup+1f;
            while(pending&&Time.realtimeSinceStartup<until)yield return null;
            Check("contextual action uses actual IMGUI button",!pending&&string.IsNullOrEmpty(report.error));yield return null;
        }
        IEnumerator Capture(string id)
        {yield return new WaitForEndOfFrame();string path=Path.Combine(world.EvidencePath,id+".png");ScreenCapture.CaptureScreenshot(path);report.captures.Add(path);yield return null;Flush();}
        PanicItem Item(string id){return world.Model.Items.Find(i=>i.Id==id);}
        IEnumerator CharacterPictures()
        {
            var root=Instantiate(Resources.Load<GameObject>("Player/SurvivorV2"));root.transform.position=new Vector3(80,0,80);foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=28;
            var a=root.GetComponent<Animator>();a.applyRootMotion=false;a.cullingMode=AnimatorCullingMode.AlwaysAnimate;a.Update(0);a.enabled=false;
            foreach(var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>())skin.updateWhenOffscreen=true;
            var camera=new GameObject("Character QA camera").AddComponent<Camera>();camera.cullingMask=1<<28;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.16f,.18f,.20f);camera.fieldOfView=35;camera.enabled=false;
            var light=new GameObject("Character QA light").AddComponent<Light>();light.type=LightType.Point;light.range=9;light.intensity=2;light.cullingMask=1<<28;light.transform.position=root.transform.position+new Vector3(2,3,2);
            int mask=world.View.cullingMask;world.View.cullingMask&=~(1<<28);
            try
            {
                yield return null;yield return new WaitForEndOfFrame();
                camera.transform.position=root.transform.position+new Vector3(2.3f,1.6f,3.2f);camera.transform.LookAt(root.transform.position+Vector3.up*.95f);Studio(camera,"character-full-front");
                var head=a.GetBoneTransform(HumanBodyBones.Head);camera.transform.position=head.position+new Vector3(.15f,.03f,.85f);camera.transform.LookAt(head.position+Vector3.up*.025f);Studio(camera,"character-face");
                PanicCharacterArt.SetTaskPose(root,a,"build",.5f);
                // Let Unity refresh the retained skinned-body matrices after the pose.
                yield return null;yield return new WaitForEndOfFrame();
                camera.transform.position=root.transform.position+new Vector3(2.2f,1.5f,3);camera.transform.LookAt(root.transform.position+Vector3.up*.95f);Studio(camera,"character-build-pose");
            }
            finally {world.View.cullingMask=mask;Destroy(root);Destroy(camera.gameObject);Destroy(light.gameObject);}
        }
        void Studio(Camera camera,string id)
        {
            var target=new RenderTexture(900,900,24);var prior=RenderTexture.active;var texture=new Texture2D(900,900,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,900,900),0,0);texture.Apply();string path=Path.Combine(world.EvidencePath,id+".png");File.WriteAllBytes(path,texture.EncodeToPNG());report.captures.Add(path);}
            finally{camera.targetTexture=null;RenderTexture.active=prior;target.Release();Destroy(target);Destroy(texture);}
        }
        void Check(string name,bool pass,string detail=""){report.rows.Add(new Row{name=name,pass=pass,detail=detail});Debug.Log("DIRECT_CHECK "+(pass?"PASS ":"FAIL ")+name+" "+detail);}
        void OnLog(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(text+"\n"+stack);}
        void Update(){if(!finished&&Time.realtimeSinceStartup-began>100f){report.error="100 second watchdog";Finish();}}
        void Flush(){if(output!=null)File.WriteAllText(output,JsonUtility.ToJson(report,true));}
        void Finish()
        {
            if(finished)return;finished=true;Application.logMessageReceived-=OnLog;
            report.scoreAfter=Hash(PanicModel.ScorePathOverride);report.realScoreAfter=Hash(realScore);
            Check("isolated and real score bytes preserved",report.scoreBefore==report.scoreAfter&&report.realScoreBefore==report.realScoreAfter);
            report.total=report.rows.Count;report.passedChecks=report.rows.Count(r=>r.pass);report.seconds=Time.realtimeSinceStartup-began;
            report.passed=report.completed&&report.total==report.passedChecks&&report.errors.Count==0&&string.IsNullOrEmpty(report.error);Flush();
            Debug.Log("DIRECT_ACTIONS_DONE "+report.passedChecks+"/"+report.total+" passed="+report.passed);Application.Quit(report.passed?0:1);
        }
        static string Hash(string path){if(!File.Exists(path))return "absent";using(var s=File.OpenRead(path))using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(s)).Replace("-","");}
    }
}
