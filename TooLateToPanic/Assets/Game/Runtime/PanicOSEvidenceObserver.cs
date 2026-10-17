using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in read-only ledger for real OS-input runs. Never drives or changes gameplay.</summary>
    public sealed class PanicOSEvidenceObserver : MonoBehaviour
    {
        [Serializable] sealed class State
        {
            public string utc, phase, stage, disaster, focus, hint, message, failure, inventory, bag;
            public float seconds, dt, preparation, phaseTime, damage, juryElapsed;
            public Vector3 actor;
            public int frame, wave, budget, water, food, completedWaves, reward, workStep, workNext;
            public long score;
            public bool focused, controller, grounded, inputLocked, cinema, loss, inventoryOpen, workActive, moving, cheated, simulationComplete;
            public bool infiniteBudget, freeMaterials, freezeTime, noLoss, developerMenu;
            public string[] keyDown, keyUp, goals;
            public bool leftDown, rightDown;
        }
        [Serializable] sealed class Summary
        {
            public string startedUtc, finishedUtc, profile, realScoreBefore, realScoreAfter, isolatedScoreBefore, isolatedScoreAfter;
            public string method = "Read-only observer of physical Unity Input and production state. No HandleKey/ClickWorkTarget/Model.Tick/Warp/dev mutation or automated gameplay.";
            public int samples, physicalKeyDownEvents, mouseDownEvents;
            public bool applicationQuitObserved;
            public List<string> captures = new List<string>(), errors = new List<string>();
        }
        readonly Summary summary = new Summary();
        readonly KeyCode[] observedKeys = {KeyCode.W,KeyCode.A,KeyCode.S,KeyCode.D,KeyCode.UpArrow,KeyCode.DownArrow,KeyCode.LeftArrow,KeyCode.RightArrow,KeyCode.LeftShift,KeyCode.Space,KeyCode.E,KeyCode.F,KeyCode.Q,KeyCode.T,KeyCode.Z,KeyCode.X,KeyCode.Alpha1,KeyCode.Alpha2,KeyCode.Alpha3,KeyCode.Alpha4,KeyCode.Tab,KeyCode.B,KeyCode.Return,KeyCode.KeypadEnter,KeyCode.Backspace,KeyCode.Escape,KeyCode.N,KeyCode.H,KeyCode.F2};
        PanicHomeWorld world; PanicHomeHud hud; CharacterController body; StreamWriter ledger;
        string output, realScore, summaryPath, previousState; float began, nextSample, nextFlush, lastShot = -10;
        int captureId;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args,"--os-e2e-observe") < 0) return;
            if (args.Any(x=>x.EndsWith("-smoke",StringComparison.Ordinal) || x=="--interaction-audit" || x.StartsWith("--e2e-",StringComparison.Ordinal)))
            { Debug.LogError("OS observer refuses combined gameplay harness flags."); return; }
            var w = FindFirstObjectByType<PanicHomeWorld>(); if(w)w.gameObject.AddComponent<PanicOSEvidenceObserver>();
        }
        void Start()
        {
            world=GetComponent<PanicHomeWorld>();hud=GetComponent<PanicHomeHud>();
            Guid profile;
            string name=world&&!string.IsNullOrEmpty(world.EvidencePath)?Path.GetFileName(world.EvidencePath.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)):string.Empty;
            if(!world||name.Length<36||!Guid.TryParse(name.Substring(name.Length-36),out profile)) { Debug.LogError("OS observer requires fresh isolated GUID --home-evidence.");enabled=false;return; }
            output=world.EvidencePath;summaryPath=Path.Combine(output,"os-observer-summary.json");
            if(File.Exists(summaryPath)||File.Exists(Path.Combine(output,"os-state-ledger.jsonl"))){Debug.LogError("OS observer refuses reused profile.");enabled=false;return;}
            body=FindObjectsByType<CharacterController>(FindObjectsSortMode.None).FirstOrDefault(x=>x.name=="Survivor");
            realScore=Path.Combine(Application.persistentDataPath,"TooLateToPanic","highscores.json");
            summary.startedUtc=DateTime.UtcNow.ToString("o");summary.profile=name;
            summary.realScoreBefore=Hash(realScore);summary.isolatedScoreBefore=Hash(PanicModel.ScorePathOverride);
            began=Time.realtimeSinceStartup;ledger=new StreamWriter(Path.Combine(output,"os-state-ledger.jsonl"),false){AutoFlush=true};
            Application.logMessageReceived+=OnLog;Flush();
        }
        void LateUpdate()
        {
            if(ledger==null||world.Model==null)return;
            var down=observedKeys.Where(Input.GetKeyDown).Select(x=>x.ToString()).ToArray();
            var up=observedKeys.Where(Input.GetKeyUp).Select(x=>x.ToString()).ToArray();
            bool left=Input.GetMouseButtonDown(0),right=Input.GetMouseButtonDown(1);float age=Time.realtimeSinceStartup-began;
            string stage=world.JuryDemo==null?"none":world.JuryDemo.Stage.ToString();
            string state=world.Model.Phase+"/"+stage+"/"+world.Model.Disaster;
            bool change=state!=previousState;
            if(age<nextSample&&down.Length==0&&up.Length==0&&!left&&!right&&!change)return;
            var m=world.Model;var work=world.Expedition.Work;
            var row=new State{utc=DateTime.UtcNow.ToString("o"),seconds=age,dt=Time.unscaledDeltaTime,frame=Time.frameCount,
                phase=m.Phase.ToString(),stage=stage,disaster=m.Disaster.ToString(),actor=world.PlayerPosition,preparation=m.TimeRemaining,phaseTime=m.PhaseTime,
                wave=m.Wave,budget=m.Budget,water=m.PackedWater,food=m.PackedFood,damage=m.TotalDamage,score=m.Score,completedWaves=m.CompletedWaves,reward=m.LastReward,
                focus=world.FocusLabel,hint=world.ContextHint,message=m.Message,failure=m.FailureReason,focused=Application.isFocused,
                controller=world.PlayerControllerEnabled,grounded=body&&body.isGrounded,inputLocked=world.InputLocked,cinema=world.CinematicActive,loss=world.LossAnnounced,
                inventoryOpen=world.InventoryOpen,workActive=world.WorkActive,moving=world.IsMoving,cheated=m.CheatedRun,simulationComplete=m.SimulationComplete,
                infiniteBudget=m.InfiniteBudget,freeMaterials=m.FreeMaterials,freezeTime=m.FreezePreparationTime,noLoss=m.DeveloperNoLoss,developerMenu=hud&&hud.DeveloperMenuOpen,
                workStep=work.CompletedSteps,workNext=work.NextClickTarget,keyDown=down,keyUp=up,leftDown=left,rightDown=right,
                inventory=string.Join(";",world.Expedition.Inventory.Select(x=>x.Id+":"+x.Count)),bag=string.Join(";",world.Expedition.Bag.Select(x=>x.Id+":"+x.Count)),
                juryElapsed=world.JuryDemo==null?0:world.JuryDemo.ElapsedSeconds,
                goals=world.JuryDemo==null?Array.Empty<string>():world.JurySnapshot.Goals.Select(x=>x.Id+"="+x.State).ToArray()};
            ledger.WriteLine(JsonUtility.ToJson(row));summary.samples++;summary.physicalKeyDownEvents+=down.Length;summary.mouseDownEvents+=(left?1:0)+(right?1:0);
            if(change&&age-lastShot>1f){string file="os-stage-"+(++captureId).ToString("D3")+"-"+m.Phase+"-"+stage+".png";ScreenCapture.CaptureScreenshot(Path.Combine(output,file));summary.captures.Add(file);lastShot=age;}
            previousState=state;nextSample=age+.25f;
            if(age>=nextFlush){Flush();nextFlush=age+5f;}
        }
        void Flush(){if(!string.IsNullOrEmpty(summaryPath))File.WriteAllText(summaryPath,JsonUtility.ToJson(summary,true));}
        void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)summary.errors.Add(message+"\n"+stack);}
        void OnApplicationQuit()
        {
            if(ledger==null)return;summary.applicationQuitObserved=true;summary.finishedUtc=DateTime.UtcNow.ToString("o");
            summary.realScoreAfter=Hash(realScore);summary.isolatedScoreAfter=Hash(PanicModel.ScorePathOverride);Flush();ledger.Dispose();ledger=null;Application.logMessageReceived-=OnLog;
        }
        static string Hash(string p){if(string.IsNullOrEmpty(p)||!File.Exists(p))return "absent";using(var s=File.OpenRead(p))using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(s)).Replace("-",string.Empty);}
    }
}
