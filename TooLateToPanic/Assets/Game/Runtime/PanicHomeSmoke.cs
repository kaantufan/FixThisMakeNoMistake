using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    // Explicit opt-in affected smoke. Normal runs never attach this component.
    public sealed class PanicHomeSmoke : MonoBehaviour
    {
        [Serializable] class Check { public string name, detail; public bool passed; }
        [Serializable] class Report { public string utc, evidenceType = "native Home V2 affected smoke using runtime APIs, real colliders and rendered frames", limitations = "Programmatic input routing; not a human keyboard playthrough, long-run balance or final visual approval."; public bool passed; public List<Check> checks = new List<Check>(); public List<string> errors = new List<string>(); public float meanFps; }
        PanicHomeWorld w; Report report = new Report(); int frames; float seconds;
        IEnumerator Start()
        {
            w = GetComponent<PanicHomeWorld>(); if (string.IsNullOrEmpty(w.EvidencePath)) yield break;
            Application.logMessageReceived += Log; w.InputLocked = true;
            yield return new WaitForSeconds(1); yield return Shot("01-exterior-title");
            w.StartGame(true); w.FinishTutorial();
            CheckIt("tutorial resets real run", w.Model.Phase == PanicPhase.Preparing && w.Model.Score == 0 && w.Expedition.Inventory.Count == 12);
            w.Warp(new Vector3(0, .05f, -7.5f)); w.SetFocus("door_main");
            var main = w.Model.Items.FirstOrDefault(i => i.Kind == ItemKind.Door && i.Position.z < -5);
            if (main != null) { w.SetFocus(main.Id); w.Interact(); }
            yield return new WaitForSeconds(.7f);
            w.Warp(new Vector3(-3, .05f, -3.7f)); yield return new WaitForSeconds(1); yield return Shot("02-ground-floor");
            // Walk the physical 16-step flight; this does not warp to the upper floor.
            w.Warp(new Vector3(0,.08f,-.45f));
            Vector3 f = w.View.transform.forward; f.y = 0; f.Normalize(); Vector3 r = w.View.transform.right; r.y = 0; r.Normalize(); Vector2 up = new Vector2(Vector3.Dot(Vector3.forward,r),Vector3.Dot(Vector3.forward,f));
            for (int i=0;i<150;i++) { w.SmokeMove(up, .016f); yield return null; }
            CheckIt("physical staircase reaches floor two", w.PlayerPosition.y > 3 && w.ActiveFloor == 1, w.PlayerPosition.ToString());
            w.Warp(new Vector3(-3,3.25f,1)); yield return new WaitForSeconds(.6f); yield return Shot("03-upper-floor");
            var exp = w.Expedition;
            Gather("fridge",2); CheckIt("gather from actual opened fridge", exp.Count("water") == 2 && exp.Count("food") == 2 && exp.Sources.Find(s=>s.Id=="fridge").Open);
            int water = exp.Inventory.FindIndex(s=>s.Id=="water"), food=exp.Inventory.FindIndex(s=>s.Id=="food"); exp.PackSlot(water); exp.PackSlot(food);
            Gather("medicinecabinet",2); bool crafted=exp.CraftFirstAid(); CheckIt("first aid crafted from household ingredients", crafted && (exp.Count("firstaid")>0 || (w.Model.PackedSupportMask&1)!=0));
            Gather("toolbox",3);
            w.Warp(new Vector3(-5.6f,.05f,3.3f)); w.SetFocus("bookshelf"); w.QuickAction(1);
            CheckIt("wall work opens active mechanic view", w.WorkActive && w.WorkTexture != null);
            yield return new WaitForSeconds(.4f); yield return Shot("04-wall-anchor-minigame");
            Solve(); w.CompleteWork(); CheckIt("wall anchor consumes materials and persists", w.Model.Items.Find(i=>i.Id=="bookshelf").Anchored && w.Model.Items.Find(i=>i.Id=="bookshelf").AnchorSurface=="wall" && exp.Count("screws")==2);
            w.Warp(new Vector3(-1.8f,.05f,-1.6f)); w.SetFocus("chair"); w.BeginMove(); CheckIt("keyboard furniture placement starts",w.IsMoving); w.CancelMove();
            var chair=w.Model.Items.Find(i=>i.Id=="chair"); bool valid=w.ValidatePlacement(chair,new Vector3(-4,.0f,-3),90,out var reason); CheckIt("placement prevents occupied overlap",!valid,reason);
            Gather("supplycrate",6);
            w.Warp(new Vector3(-10.3f,.05f,4)); w.SetFocus("plant_west1"); w.Interact(); Solve(); w.CompleteWork(); CheckIt("garden planting creates lasting mitigation",w.Model.TreeCount==1);
            w.Warp(new Vector3(9.5f,.05f,-6.5f)); w.ToggleInventory(); yield return new WaitForSeconds(.6f); yield return Shot("05-inventory-garden"); w.ToggleInventory();
            // Render each actual disaster presentation without repeating a full nine-wave campaign.
            foreach (var type in new[]{DisasterType.Earthquake,DisasterType.Flood,DisasterType.Fire})
            {
                w.Model.Disaster=type; w.Model.Phase=PanicPhase.Disaster; w.Model.PhaseTime=12; yield return null;
                yield return new WaitForSeconds(5);
                foreach(var ps in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None)) { var renderer=ps.GetComponent<ParticleSystemRenderer>(); Debug.Log("HOME_WEATHER_DIAGNOSTIC "+type+" emitter="+ps.name+" particles="+ps.particleCount+" shader="+(renderer&&renderer.sharedMaterial?renderer.sharedMaterial.shader.name:"missing")); }
                yield return Shot("disaster-"+type);
                w.Model.Phase=PanicPhase.Preparing; yield return null;
            }
            w.Model.Disaster=DisasterType.Earthquake; w.Model.EnterBunker(); yield return null;
            w.Model.Tick(8.05f); yield return null; w.Model.Tick(4.05f); yield return null;
            CheckIt("gathered bag and preparation survive wave one",w.Model.Phase==PanicPhase.Success && w.Model.CompletedWaves==1,"damage="+w.Model.TotalDamage);
            if(w.Model.Phase==PanicPhase.Success) { w.Model.Tick(3.05f); yield return null; w.Warp(new Vector3(-3,.05f,-2)); yield return new WaitForSeconds(.4f); yield return Shot("06-damage-inspection"); w.FinishInspection(); CheckIt("next wave resets damage but preserves improvements",w.Model.TotalDamage==0 && w.Model.Items.Find(i=>i.Id=="bookshelf").Anchored && w.Model.TreeCount==1 && w.Model.PackedWater==0); }
            CheckIt("dressed survivor has workwear geometry",Resources.Load<GameObject>("Player/SurvivorV2").GetComponentsInChildren<Transform>(true).Any(t=>t.name=="PanicWorkwear_V2"));
            w.StartGame(false); w.Model.Tick(301); CheckIt("missing bunker deadline loses",w.Model.Phase==PanicPhase.GameOver);
            w.StartGame(false); w.Model.EnterBunker(); w.Model.Tick(12.1f); w.Model.Tick(3.1f); CheckIt("insufficient gathered bag loses",w.Model.Phase==PanicPhase.GameOver,w.Model.FailureReason);
            report.utc=DateTime.UtcNow.ToString("o"); report.meanFps=frames/Mathf.Max(.01f,seconds); report.passed=report.errors.Count==0&&report.checks.All(c=>c.passed);
            File.WriteAllText(Path.Combine(w.EvidencePath,"native-home-smoke.json"),JsonUtility.ToJson(report,true));
            Debug.Log("HOME_SMOKE "+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count+" errors="+report.errors.Count);
            w.ReturnToMenu(); w.InputLocked=false; Application.logMessageReceived-=Log; enabled=false;
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--keep-home-smoke")<0) { yield return new WaitForSeconds(.2f); Application.Quit(); }
        }
        void Gather(string id,int stacks) { var s=w.Expedition.Sources.Find(x=>x.Id==id); w.Warp(s.Position+new Vector3(0,.06f,-1.3f)); w.SetFocus(id); if(!s.Open)w.Interact(); for(int i=0;i<stacks;i++)w.Interact(); }
        void Solve() { var work=w.Expedition.Work; for(int i=0;i<1600&&work.Active&&!work.Complete;i++) { float diff=work.Target-work.Cursor; bool aligned=Mathf.Abs(diff)<.035f; work.Tick(.04f,aligned?0:Mathf.Sign(diff),true,aligned); } }
        IEnumerator Shot(string id) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(w.EvidencePath,id+".png")); yield return null; }
        void CheckIt(string name,bool pass,string detail="") { report.checks.Add(new Check{name=name,passed=pass,detail=detail}); Debug.Log("HOME_CHECK "+(pass?"PASS ":"FAIL ")+name+" "+detail); }
        void Log(string s,string stack,LogType type) { if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(s+"\n"+stack); }
        void Update(){frames++;seconds+=Time.unscaledDeltaTime;}
    }
}
