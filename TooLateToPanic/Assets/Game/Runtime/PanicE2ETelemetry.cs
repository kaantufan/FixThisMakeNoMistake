using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace TooLateToPanic
{
    // Read-only opt-in observer. Never drives gameplay or attaches in ordinary sessions.
    public sealed class PanicE2ETelemetry : MonoBehaviour
    {
        [Serializable] public sealed class MemorySample
        {
            public float seconds; public string phase, stage; public long managed, allocated, reserved, workingSet;
            public int audioSources, particleSystems; public float preparation; public bool inputLocked, developer;
        }
        [Serializable] public sealed class Report
        {
            public string startedUtc, finishedUtc;
            public string evidenceType="Observed rendered frame intervals and process/memory samples. No gameplay input or state mutation.";
            public string limitations="Frame intervals include observer/instrumentation and window/background scheduling. Not independent GPU timings, not human experience, not proof of every performance scene.";
            public int width,height,frames,hitchesOver100ms,hitchesOver500ms; public bool runInBackground;
            public string gpu,cpu,os,unity; public float seconds,p50ms,p95ms,p99ms,maxms;
            public List<MemorySample> samples=new List<MemorySample>(); public List<string> errors=new List<string>();
        }
        readonly Report report=new Report(); readonly List<float> intervals=new List<float>();
        PanicHomeWorld world; float began,nextSample,nextFlush; string output; bool finished;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--e2e-telemetry")<0)return;
            var w=FindFirstObjectByType<PanicHomeWorld>();if(w)w.gameObject.AddComponent<PanicE2ETelemetry>();
        }
        void Start()
        {
            world=GetComponent<PanicHomeWorld>();
            if(!world||string.IsNullOrEmpty(world.EvidencePath)){enabled=false;Debug.LogError("E2E telemetry requires isolated --home-evidence");return;}
            output=Path.Combine(world.EvidencePath,"e2e-telemetry.json");began=Time.realtimeSinceStartup;
            report.startedUtc=DateTime.UtcNow.ToString("o");report.width=Screen.width;report.height=Screen.height;
            report.gpu=SystemInfo.graphicsDeviceName;report.cpu=SystemInfo.processorType;report.os=SystemInfo.operatingSystem;report.unity=Application.unityVersion;
            report.runInBackground=Application.runInBackground;Application.logMessageReceived+=Log;
        }
        void Update()
        {
            if(finished||string.IsNullOrEmpty(output))return;
            if(!UnityEngine.Rendering.SplashScreen.isFinished)return;
            float ms=Time.unscaledDeltaTime*1000f;
            if(ms>0&&intervals.Count<250000){intervals.Add(ms);if(ms>100)report.hitchesOver100ms++;if(ms>500)report.hitchesOver500ms++;}
            float age=Time.realtimeSinceStartup-began;
            if(age>=nextSample){Sample(age);nextSample=age+5f;}
            if(age>=nextFlush){Flush();nextFlush=age+10f;}
        }
        void Sample(float age)
        {
            long working=0;try{using(var p=System.Diagnostics.Process.GetCurrentProcess())working=p.WorkingSet64;}catch{}
            report.samples.Add(new MemorySample{seconds=age,phase=world.Model.Phase.ToString(),stage=world.JuryDemo==null?"none":world.JuryDemo.Stage.ToString(),
                managed=GC.GetTotalMemory(false),allocated=Profiler.GetTotalAllocatedMemoryLong(),reserved=Profiler.GetTotalReservedMemoryLong(),workingSet=working,
                audioSources=FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length,particleSystems=FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length,
                preparation=world.Model.TimeRemaining,inputLocked=world.InputLocked,developer=world.Model.CheatedRun});
        }
        void Flush()
        {
            if(string.IsNullOrEmpty(output))return;
            report.seconds=Time.realtimeSinceStartup-began;report.frames=intervals.Count;
            if(intervals.Count>0){var ordered=intervals.OrderBy(v=>v).ToArray();report.p50ms=Q(ordered,.50f);report.p95ms=Q(ordered,.95f);report.p99ms=Q(ordered,.99f);report.maxms=ordered[ordered.Length-1];}
            try{File.WriteAllText(output,JsonUtility.ToJson(report,true));}catch(Exception e){Debug.LogWarning("E2E telemetry write: "+e.Message);}
        }
        static float Q(float[] values,float q){return values[Mathf.Clamp(Mathf.CeilToInt(q*values.Length)-1,0,values.Length-1)];}
        void Log(string condition,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(type+": "+condition);}
        void OnApplicationQuit(){if(finished)return;finished=true;report.finishedUtc=DateTime.UtcNow.ToString("o");Flush();Application.logMessageReceived-=Log;}
    }
}
