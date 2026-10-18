#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TooLateToPanic
{
    /// <summary>Opt-in Editor-only source/import/model checks followed by a Windows build. Never opens a player.</summary>
    public static class PanicV59HeadlessDelivery
    {
        [Serializable] public sealed class Row { public string group, id, detail; public bool pass; }
        [Serializable] public sealed class Report
        {
            public string utc, unity, error;
            public string layer = "ACTUAL_UNITY_EDITOR_HEADLESS_IMPORT_MODEL_API_AND_LAYOUT_ARITHMETIC";
            public string nativePlayer = "NOT_RUN_USER_STOP", graphicalRender = "NOT_RUN_USER_STOP", osInput = "NOT_RUN_USER_STOP", humanListening = "NOT_RUN", humanCampaign = "NOT_RUN";
            public bool complete, passed, playerLaunched, osInputSent, realProfileMutated;
            public double seconds;
            public int checks, failures;
            public List<Row> rows = new List<Row>();
            public List<string> fixtures = new List<string>();
        }
        static Report report;
        static string group;
        static void Check(string id, bool pass, string detail)
        {
            report.rows.Add(new Row {group=group,id=id,pass=pass,detail=detail});
            report.checks++; if(!pass) report.failures++;
            Debug.Log("V59_HEADLESS " + group + " " + (pass?"PASS ":"FAIL ") + id + " " + detail);
        }
        static void RunGroup(string name, Action action)
        {
            group=name;
            try { action(); } catch(Exception e) { Check("exception",false,e.ToString()); }
        }
        public static void Build()
        {
            if(!Application.isEditor || !Application.isBatchMode) throw new InvalidOperationException("Batch Editor only. No player or OS automation.");
            string[] args=Environment.GetCommandLineArgs(); int index=Array.IndexOf(args,"-v59Evidence");
            if(index<0||index+1>=args.Length||!Path.IsPathRooted(args[index+1]))throw new ArgumentException("Absolute -v59Evidence path required.");
            string output=Path.GetFullPath(args[index+1]);Directory.CreateDirectory(output);
            string previous=PanicModel.ScorePathOverride;
            var watch=Stopwatch.StartNew();
            report=new Report { utc=DateTime.UtcNow.ToString("o"), unity=Application.unityVersion };
            report.fixtures.Add("All model scores use an explicit fresh evidence directory; ScorePathOverride is restored in finally.");
            report.fixtures.Add("Economy scenarios use the actual residence item layout as a declared model fixture; seeded stock selection and work APIs are not a physical/time/controller route or human campaign.");
            report.fixtures.Add("Compound masks, waves, model seeds, memoized utility events and stocks are explicit model fixtures. No OS or new graphical player is launched.");
            report.fixtures.Add("Short cue PCM is read from actual imported Resources AudioClips; retained Streaming music uses real import metadata plus separately measured offline source PCM. Layout uses actual font metrics and projected authored fixtures, not a framebuffer or listening result.");
            try
            {
                PanicModel.ScorePathOverride=Path.Combine(output,"isolated-model-scores.json");
                RunGroup("navigation",()=> { var navigation=PanicNavigationEditorProbeV59.Probe(); foreach(var row in navigation.checks)Check(row.id,row.pass,row.detail); });
                RunGroup("dialogue-import",()=>PanicDialogueSourcePolicyV59.Run(Check));
                RunGroup("utility-work-labels",()=> { var labels=PanicManualWorkLabelsEditorProbeV59.Probe(); File.WriteAllText(Path.Combine(output,"utility-work-labels.json"),JsonUtility.ToJson(labels,true)); Check("actual-font-art-projection-layout",labels.passed,"Actual authored273poses; "+labels.checks+" bounds checks; no framebuffer/OS/human claim. Failures="+string.Join(",",labels.failures)); });
                RunGroup("work-key-hints",()=> { var hints=PanicWorkKeyHintsEditorProbeV59.Probe(); File.WriteAllText(Path.Combine(output,"work-key-hints.json"),JsonUtility.ToJson(hints,true)); Check("actual-font-keycap-layout",hints.passed,hints.checks+" actual-font/layout checks; no framebuffer/OS/human claim. Failures="+string.Join(",",hints.failures)); });
                RunGroup("caption-marker-clearance",()=>PanicCaptionMarkerEditorProbeV59.Run(Check));
                RunGroup("notebook-copy",()=> {
                    Check("turkish-heading",PanicNotebookData.Heading("Ev planı")=="EV PLANI"&&PanicNotebookData.Heading("Fırtına")=="FIRTINA"&&PanicNotebookData.Heading("iı")=="İI","Production UI-only tr-TR casing; original IDs/parsers unchanged.");
                    Check("check-state",PanicNotebookData.CheckState(new PanicNotebookCheck("", "",0,2))=="Yapılmadı"&&PanicNotebookData.CheckState(new PanicNotebookCheck("", "",1,2))=="Kısmen hazır"&&PanicNotebookData.CheckState(new PanicNotebookCheck("", "",2,2))=="Hazır"&&PanicNotebookData.CheckState(new PanicNotebookCheck("", "",0,0))=="Gerekli değil","Production count-derived labels, no gameplay change.");
                    var font=Resources.Load<Font>("HUD/Barlow");var style=new GUIStyle {font=font,fontSize=17};
                    Check("stock-status-font-fit",font&&style.CalcSize(new GUIContent("Toplanmadı")).x<=100f,"Actual offline regular17font; source state column100logical pixels, source name shortened by same18pixels.");
                    var source=new LootSource {Id="qa:pin",Label="fixture",Contents=new List<SupplySlot>{new SupplySlot("plank",1)}};
                    Check("pin-uncollected",PanicNotebookData.PinState(new PanicNotebookPin(source))=="Toplanmadı","Declared stock-only UI fixture; read-only pin state.");
                    source.Contents.Clear();Check("pin-empty",PanicNotebookData.PinState(new PanicNotebookPin(source))=="Tükendi","No remaining stock, uncollected source is exhausted.");
                    source.AutoLooted=true;Check("pin-collected",PanicNotebookData.PinState(new PanicNotebookPin(source))=="Toplandı","Actual AutoLooted flag reflects material collection, independent of cupboard open/closed.");
                });
                RunGroup("economy",()=>PanicEconomyChecksV59.Run(Check));
                RunGroup("final-policy",()=>PanicFinalPolicyChecksV59.Run(Check));
                RunGroup("clock-boundaries",()=>PanicPreparationClockChecksV59.Run(Check));
                RunGroup("music-mix-import",()=>PanicMusicMixPolicyV59.Run(Check));
                RunGroup("v5-systems-regression",()=>PanicV5SystemsModelChecks.Run(Check));
                RunGroup("compound",()=>PanicCompoundChecksV54.Run(Check));
                RunGroup("wave-v52-regression",()=>PanicWaveV52ModelChecks.Run(Check));
                RunGroup("ledger-regression",()=>PanicWaveLedgerModelChecksV56.Run(output,Check));
                RunGroup("unified-inventory-regression",()=>PanicUnifiedInventoryChecks.Run(Check));
                RunGroup("retained-v58-geometry",()=>Check("actual-mesh-avatar-flood",PanicVisualFixesHeadlessV58.ValidateAndSave(Path.Combine(output,"retained-v58")),"Nested actual Editor geometry report retains garage/NPC/flood fixes; no rendered pixels."));
                report.complete=true;
            }
            catch(Exception e){report.error=e.ToString();group="driver";Check("exception",false,e.ToString());}
            finally
            {
                PanicModel.ScorePathOverride=previous;watch.Stop();report.seconds=watch.Elapsed.TotalSeconds;
                report.passed=report.complete&&report.failures==0;
                File.WriteAllText(Path.Combine(output,"headless-v59-report.json"),JsonUtility.ToJson(report,true));
            }
            if(!report.passed){Debug.LogError("V59_HEADLESS_FAILED: "+report.failures+" failing checks. Windows build not started.");EditorApplication.Exit(4);return;}
            Debug.Log("V59_HEADLESS_PASS checks="+report.checks+"; no native player/render/OS/human claim.");
            PanicResidenceBuild.Build();
        }
    }
}
#endif
