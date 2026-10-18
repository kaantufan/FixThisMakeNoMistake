#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
namespace TooLateToPanic
{
    public static class PanicNavigationEditorProbeV59
    {
        [Serializable] public sealed class Row { public string id,detail;public bool pass; }
        [Serializable] public sealed class Receipt {public bool passed=true;public List<Row> checks=new List<Row>();}
        public static Receipt Probe()
        {
            var r=new Receipt();void Check(string id,bool pass,string detail){r.checks.Add(new Row{id=id,pass=pass,detail=detail});r.passed&=pass;}
            Check("title.normal",PanicHomeHud.TitleChoiceLabelV414(0)=="OYNA","Retained normal play label.");
            Check("title.other-modes",PanicHomeHud.TitleChoiceLabelV414(1)=="KISA OYUN"&&PanicHomeHud.TitleChoiceLabelV414(2)=="AFETİ DENE"&&PanicHomeHud.TitleChoiceLabelV414(3)=="NASIL OYNANIR","Retained separate existing modes.");
            foreach(PanicPhase phase in Enum.GetValues(typeof(PanicPhase)))
            {bool playable=phase==PanicPhase.Preparing||phase==PanicPhase.Tutorial||phase==PanicPhase.Inspection;
                Check("phase."+phase,PanicNotebookCursorPolicyV59.CanShow(phase,true,false)==playable&&!PanicNotebookCursorPolicyV59.CanShow(phase,false,false)&&!PanicNotebookCursorPolicyV59.CanShow(phase,true,true),"Manual-only playable nonmodal eligibility.");}
            Check("stage.material",PanicNotebookCursorPolicyV59.TargetVisibleInStage(true,false,false)&&!PanicNotebookCursorPolicyV59.TargetVisibleInStage(true,false,true),"Preparation source only.");
            Check("stage.supply",!PanicNotebookCursorPolicyV59.TargetVisibleInStage(false,true,false)&&PanicNotebookCursorPolicyV59.TargetVisibleInStage(false,true,true),"Bag supply only.");
            for(int actor=-1;actor<=1;actor++)for(int target=-1;target<=1;target++)
                Check("local.floor."+actor+"."+target,PanicNotebookLocalGlowPolicyV59.CanShow(true,actor,target,4f)==(actor==target),"Only same-floor local mesh; no other-floor marker.");
            Check("local.outside",!PanicNotebookLocalGlowPolicyV59.CanShow(false,0,0,1),"No global cue outside residence.");
            Check("local.range",PanicNotebookLocalGlowPolicyV59.CanShow(true,0,0,64)&&!PanicNotebookLocalGlowPolicyV59.CanShow(true,0,0,64.001f),"Explicit8m local boundary.");
            Check("local.invalid",!PanicNotebookLocalGlowPolicyV59.CanShow(true,0,0,float.NaN)&&!PanicNotebookLocalGlowPolicyV59.CanShow(true,0,0,float.PositiveInfinity)&&!PanicNotebookLocalGlowPolicyV59.CanShow(true,0,0,-1),"Reject invalid projection/range inputs.");
            Check("caption.clearance",PanicNotebookCursorPolicyV59.CaptionClearance==new Rect(440,116,690,94),"Retained caption reservation; notebook mesh glow uses no screen card.");
            return r;
        }
    }
}
#endif
