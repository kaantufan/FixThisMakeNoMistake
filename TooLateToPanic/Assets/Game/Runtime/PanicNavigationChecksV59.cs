using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TooLateToPanic
{
    // Opt-in current UI driver hook. Declared actor/view fixtures, never OS input or a natural route.
    public static class PanicNavigationChecksV59
    {
        [Serializable] public sealed class Receipt
        {
            public string layer = "NATIVE_LOCAL_ACTUAL_MESH_COMMANDS_AND_FRAMEBUFFERS_NOT_OS_OR_HUMAN";
            public bool complete;
            public List<string> fixtures = new List<string>();
            public List<string> failures = new List<string>();
        }
        static IEnumerator Wait(float seconds)
        { float until=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<until)yield return null; }
        static void Check(Receipt r,Action<string,bool,string> record,string id,bool pass,string detail)
        { if(!pass)r.failures.Add(id);record("navigation.local-v59."+id,pass,detail); }
        static string State(PanicHomeWorld w)
        { return PanicNotebookVerification.GameplayFingerprint(w)+"|"+w.Model.SecondaryRollCountV5+"|"+JsonUtility.ToJson(UnityEngine.Random.state)+"|"+w.SelectedTargetIdV56+"|"+w.PlayerPosition; }
        public static IEnumerator Run(PanicHomeWorld w,PanicHomeHud h,Action<string,bool,string> record,
            Func<string,string,IEnumerator> capture,Func<int,int,IEnumerator> resolution)
        {
            var r=new Receipt();bool priorHelper=w.AssistantEnabledV55,priorInput=w.FullFlowScriptedInput,priorMotion=w.ReducedFpsMotionV54;
            int width=Screen.width,height=Screen.height;
            r.fixtures.Add("Public normal start/skip, manual fridge pin; helper OFF. Explicit actor placements: production exterior spawn, inside(4,.05,1.5), far inside(-7,.05,-4), exterior(4,.05,-8), restore inside. No movement/route acceptance.");
            r.fixtures.Add("Synchronous 180degree camera query restored before yielding. Native surfaces1600x900/960x540; ordinary camera follow and actual depth-tested mesh command buffer. Modal APIs and reduced-motion preference restored.");
            try
            {
                if(string.IsNullOrEmpty(w.EvidencePath)||string.IsNullOrEmpty(PanicModel.ScorePathOverride))throw new InvalidOperationException("Isolated root QA driver required.");
                h.SetPaused(false);w.ReturnToMenu();w.StartGame(false);w.FullFlowScriptedInput=true;yield return null;
                if(h.FreeplayOnboardingOpenV414)h.ConfirmFreeplayOnboardingV414();yield return null;w.SkipIntroStoryV414();yield return Wait(.35f);
                w.SetAssistantEnabledV55(false);yield return resolution(1600,900);
                Check(r,record,"no-pin-no-glow",!w.HasNotebookTargetV57&&!w.NotebookGlowActiveV59,"Ordinary nearby selection cannot create a notebook glow.");
                Check(r,record,"manual-fridge-pin",w.TryMarkNotebookTargetV57("fridge"),"Actual public manual source selection, no automatic target choice.");
                yield return Wait(.1f);
                Check(r,record,"spawn-exterior-no-global-glow",!w.NotebookGlowActiveV59,"Production spawn outside residence receives no notebook marker or glow.");
                yield return capture("local-glow-exterior-1600","Actual exterior spawn with fridge pin; no global card, bracket or arrow.");
                w.Warp(new Vector3(4f,.05f,1.5f));w.SetFocus("door_main");yield return Wait(.65f);
                Check(r,record,"inside-real-geometry",w.NotebookGlowActiveV59&&w.NotebookGlowTargetIdV59=="fridge"&&w.NotebookGlowRendererCountV59>0&&
                    w.TryGetNotebookGlowStateV59(out var id,out var bounds,out var floor)&&w.TryGetInteractionBounds(id,out var expected)&&bounds.Equals(expected)&&floor==w.ActiveFloor,
                    "Same actual floor and local source mesh bounds; manual pin is independent of current door focus.");
                Check(r,record,"no-global-manual-helper",!h.AssistantHudVisibleV55&&!h.AssistantCardVisibleV55,"Even the old small corner manual target card is suppressed; only local mesh glow remains.");
                var pass=w.View.GetComponent<PanicSelectionSilhouette>();yield return new WaitForEndOfFrame();
                Check(r,record,"actual-mesh-pass-submitted",pass&&pass.PassReady&&pass.NotebookRendererCount>0&&pass.NotebookRenderedFrame>=Time.frameCount-1,
                    "Native camera emitted actual mesh mask/composite commands. Submitted draws are not an unobscured-pixel claim; root inspects paired captures.");
                string state=State(w);for(int i=0;i<20;i++)w.TryGetNotebookGlowStateV59(out var ignoredId,out var ignoredBounds,out var ignoredFloor);
                Check(r,record,"read-only-query",state==State(w),"Synchronous before/after actual gameplay, stock, RNG, current focus and actor fingerprint unchanged; clock excluded.");
                yield return capture("local-glow-fridge-1600","Actual local warm surface/mesh contour on pinned fridge; no floating target card or brackets.");
                w.ClearNotebookTargetV57();yield return Wait(.05f);
                Check(r,record,"clear-removes-mesh-pass",!w.NotebookGlowActiveV59&&pass.NotebookRendererCount==0,"Actual public clear removes only notebook list; original near selection/gather ownership retained.");
                yield return capture("local-glow-fridge-unselected-1600","Same actor/view as glow frame, manual pin cleared; compare actual visible surface.");
                w.TryMarkNotebookTargetV57("fridge");yield return resolution(960,540);
                yield return capture("local-glow-fridge-960","Actual same local mesh glow at960x540; no screen locator.");
                w.SetReducedFpsMotionV54(true);yield return Wait(.1f);float a=w.NotebookGlowStrengthV59;yield return Wait(.16f);
                Check(r,record,"reduced-motion-steady",w.NotebookGlowActiveV59&&Mathf.Abs(a-.85f)<.0001f&&Mathf.Abs(w.NotebookGlowStrengthV59-a)<.0001f,"Reduced motion holds actual warm glow strength steady.");
                w.SetReducedFpsMotionV54(priorMotion);
                Quaternion rotation=w.View.transform.rotation;
                try { w.View.transform.rotation=rotation*Quaternion.Euler(0,180,0);
                    Check(r,record,"offscreen-has-no-direction",!w.TryGetNotebookGlowStateV59(out var offId,out var offBounds,out var offFloor),"View-only query rejects target behind camera; no fallback arrow or edge card."); }
                finally { w.View.transform.rotation=rotation; }
                h.SetPaused(true);yield return new WaitForEndOfFrame();
                Check(r,record,"pause-clears",!w.NotebookGlowActiveV59&&pass.NotebookRendererCount==0&&w.HasNotebookTargetV57,"Camera-time guard clears notebook draw during real pause but retains manual intent.");h.SetPaused(false);yield return null;
                w.ToggleInventory();yield return new WaitForEndOfFrame();
                Check(r,record,"inventory-clears",w.InventoryOpen&&!w.NotebookGlowActiveV59&&pass.NotebookRendererCount==0,"Actual inventory hides local glow.");if(w.InventoryOpen)w.ToggleInventory();yield return null;
                bool opened=w.Notebook.RequestOpen();yield return Wait(.3f);
                Check(r,record,"notebook-clears",opened&&!w.NotebookGlowActiveV59&&pass.NotebookRendererCount==0,"Actual notebook modal clears mesh commands.");w.Notebook.RequestClose();yield return Wait(.4f);
                w.TryMarkNotebookTargetV57("basement_pantry");yield return Wait(.1f);
                Check(r,record,"other-floor-no-glow",!w.NotebookGlowActiveV59&&pass.NotebookRendererCount==0,"Different-floor pin has no world label or glow.");
                yield return capture("local-glow-other-floor-960","Ground-floor actor with actual basement pin; no global or other-floor world indicator.");
                w.TryMarkNotebookTargetV57("fridge");w.Warp(new Vector3(-7f,.05f,-4f));yield return Wait(.3f);
                Check(r,record,"far-same-floor-no-glow",!w.NotebookGlowActiveV59&&pass.NotebookRendererCount==0,"Inside same floor beyond8metres receives no local cue.");
                w.Warp(new Vector3(4f,.05f,-8f));yield return Wait(.3f);
                Check(r,record,"outside-no-glow",!w.NotebookGlowActiveV59&&pass.NotebookRendererCount==0,"House pin cannot glow while player is outside.");
                w.Warp(new Vector3(4f,.05f,1.5f));yield return Wait(.45f);
                Check(r,record,"return-inside-resumes",w.NotebookGlowActiveV59&&w.NotebookGlowTargetIdV59=="fridge","Retained manual intent resumes local glow on return; no model mutation.");
                w.StartGame(false);yield return null;
                Check(r,record,"restart-clears",!w.HasNotebookTargetV57&&!w.NotebookGlowActiveV59,"Existing production restart clears manual pin and native mesh list.");
                r.complete=true;
            }
            finally
            {
                h.SetPaused(false);w.SetAssistantEnabledV55(priorHelper);w.SetReducedFpsMotionV54(priorMotion);w.ReturnToMenu();w.FullFlowScriptedInput=priorInput;
                if(!string.IsNullOrEmpty(w.EvidencePath))File.WriteAllText(Path.Combine(w.EvidencePath,"navigation-v59.json"),JsonUtility.ToJson(r,true));
            }
            yield return resolution(width,height);
        }
    }
}
