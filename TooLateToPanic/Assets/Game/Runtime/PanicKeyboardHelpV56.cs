using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>F1 explanatory modal reuses the actual physical keyboard layout. Animation uses unscaled time; game pause is model-owned.</summary>
    public sealed partial class PanicHomeHud
    {
        sealed class KeyboardHelpCalloutV56
        {
            internal string Key,Action;internal string[] Symbols;internal Vector2 Anchor;internal Rect Label;
            internal int Family;
        }
        readonly List<KeyboardHelpCalloutV56> keyboardHelpCalloutsV56 = new List<KeyboardHelpCalloutV56>(20);
        bool keyboardHelpTargetV56,keyboardHelpPriorLockV56,keyboardHelpPriorLegacyHelpV56,keyboardHelpDrawingV56;
        float keyboardHelpProgressV56,keyboardHelpLastTimeV56;
        int keyboardHelpProcessFrameV56=-1,keyboardHelpInputFrameV56=-1,keyboardHelpRepaintFrameV56=-1;
        PanicModel keyboardHelpOwnerV56;PanicPhase keyboardHelpPhaseV56;
        GUIStyle keyboardHelpTitleStyleV56,keyboardHelpLabelStyleV56,keyboardHelpKeyStyleV56;
        public bool KeyboardHelpVisibleV56 { get { return keyboardHelpOwnerV56!=null&&(keyboardHelpTargetV56||keyboardHelpProgressV56>0f); } }
        public bool KeyboardHelpBlockingV56 { get { return KeyboardHelpVisibleV56||(World&&World.Model!=null&&World.Model.KeyboardHelpOpenV56)||keyboardHelpInputFrameV56==Time.frameCount; } }
        public float KeyboardHelpAnimationProgressV56 { get { return keyboardHelpProgressV56; } }
        public Rect KeyboardHelpPanelBoundsV56 { get { return new Rect(40,82,1520,778); } }
        public Rect KeyboardHelpBoundsV56
        {
            get { return LerpKeyboardHelpRectV56(new Rect(24,685,308,147),new Rect(360,315,880,420),KeyboardHelpEaseV56); }
        }
        public int KeyboardHelpCalloutCountV56 { get; private set; }
        public int KeyboardHelpDrawFramesV56 { get; private set; }
        public string KeyboardHelpRenderedTextV56 { get; private set; }
        bool KeyboardHelpButtonInputV56 { get { return keyboardHelpDrawingV56&&keyboardHelpInputFrameV56!=Time.frameCount&&keyboardHelpTargetV56; } }
        float KeyboardHelpEaseV56 { get { float t=Mathf.Clamp01(keyboardHelpProgressV56);return t*t*(3f-2f*t); } }
        bool KeyboardHelpCanOpenV56
        {
            get
            {
                if(!World||World.Model==null||World.InputLocked||World.Model.IsPaused||_paused||_developerMenuOpen||
                    DeveloperInputBlockedThisFrame||WaveLedgerInputBlockedV56||World.Model.WaveLedgerOpenV56||
                    World.NotebookBlocksInput||World.InventoryOpen||World.WorkActive||World.IsMoving||
                    World.CinematicActive||World.LossAnnounced||World.BagCameraTransitionActive||FreeplayOnboardingInputBlockedV414) return false;
                var phase=World.Model.Phase;return phase==PanicPhase.Preparing||phase==PanicPhase.Tutorial||phase==PanicPhase.Inspection;
            }
        }
        public bool OpenKeyboardHelpV56()
        {
            if(KeyboardHelpVisibleV56){keyboardHelpTargetV56=true;keyboardHelpInputFrameV56=Time.frameCount;return true;}
            if(!KeyboardHelpCanOpenV56)return false;
            BuildKeyboardHelpCalloutsV56();
            if(!World.Model.SetKeyboardHelpOpenV56(true))return false;
            keyboardHelpOwnerV56=World.Model;keyboardHelpPhaseV56=World.Model.Phase;
            keyboardHelpPriorLockV56=World.InputLocked;keyboardHelpPriorLegacyHelpV56=_helpVisible;
            World.InputLocked=true;_helpVisible=false;keyboardHelpTargetV56=true;keyboardHelpProgressV56=0f;
            keyboardHelpLastTimeV56=Time.unscaledTime;keyboardHelpInputFrameV56=Time.frameCount;
            compactControlsExpandedV5=true;compactControlsInputFrameV5=Time.frameCount;CancelInventoryDrag();return true;
        }
        public void CloseKeyboardHelpV56()
        {
            if(!KeyboardHelpVisibleV56)return;
            keyboardHelpTargetV56=false;keyboardHelpInputFrameV56=Time.frameCount;compactControlsExpandedV5=false;
            // Model pause and InputLocked remain owned until the last closing frame.
            if(keyboardHelpProgressV56<=0f)FinishKeyboardHelpV56(true);
        }
        public bool ProcessKeyboardHelpKeyV56(KeyCode key)
        {
            if(keyboardHelpInputFrameV56==Time.frameCount)return KeyboardHelpBlockingV56;
            if(key==KeyCode.F1){if(KeyboardHelpVisibleV56)CloseKeyboardHelpV56();else return OpenKeyboardHelpV56();return true;}
            if(key==KeyCode.Escape&&KeyboardHelpVisibleV56){CloseKeyboardHelpV56();return true;}
            return KeyboardHelpBlockingV56;
        }
        bool ProcessKeyboardHelpInputV56()
        {
            if(keyboardHelpProcessFrameV56==Time.frameCount)return KeyboardHelpBlockingV56;
            keyboardHelpProcessFrameV56=Time.frameCount;
            if(keyboardHelpOwnerV56!=null)
            {
                if(!World||World.Model!=keyboardHelpOwnerV56||!keyboardHelpOwnerV56.KeyboardHelpOpenV56||World.Model.Phase!=keyboardHelpPhaseV56)
                    FinishKeyboardHelpV56(false); // Phase/reset owner already owns its own input lock.
                else
                {
                    float now=Time.unscaledTime,dt=Mathf.Max(0,now-keyboardHelpLastTimeV56);keyboardHelpLastTimeV56=now;
                    keyboardHelpProgressV56=Mathf.MoveTowards(keyboardHelpProgressV56,keyboardHelpTargetV56?1f:0f,dt/.28f);
                    if(!keyboardHelpTargetV56&&keyboardHelpProgressV56<=0f)FinishKeyboardHelpV56(true);
                }
            }
            if(keyboardHelpInputFrameV56!=Time.frameCount)
            {
                if(Input.GetKeyDown(KeyCode.F1))ProcessKeyboardHelpKeyV56(KeyCode.F1);
                else if(KeyboardHelpVisibleV56&&Input.GetKeyDown(KeyCode.Escape))ProcessKeyboardHelpKeyV56(KeyCode.Escape);
            }
            return KeyboardHelpBlockingV56;
        }
        void FinishKeyboardHelpV56(bool restoreLock)
        {
            if(World&&World.Model==keyboardHelpOwnerV56&&keyboardHelpOwnerV56!=null)
            {
                if(restoreLock&&keyboardHelpOwnerV56.KeyboardHelpOpenV56){World.InputLocked=keyboardHelpPriorLockV56;_helpVisible=keyboardHelpPriorLegacyHelpV56;}
            }
            if(keyboardHelpOwnerV56!=null)keyboardHelpOwnerV56.SetKeyboardHelpOpenV56(false);
            keyboardHelpOwnerV56=null;keyboardHelpTargetV56=false;keyboardHelpProgressV56=0f;
            keyboardHelpInputFrameV56=Time.frameCount;compactControlsExpandedV5=false;
        }
        public void ResetKeyboardHelpPresentationV56() { FinishKeyboardHelpV56(true); }
        static Rect LerpKeyboardHelpRectV56(Rect a,Rect b,float t)
        {return new Rect(Mathf.Lerp(a.x,b.x,t),Mathf.Lerp(a.y,b.y,t),Mathf.Lerp(a.width,b.width,t),Mathf.Lerp(a.height,b.height,t));}
        static Color KeyboardHelpFadeV56(Color color,float alpha){return new Color(color.r,color.g,color.b,color.a*alpha);}
        void AddKeyboardHelpCalloutV56(string key,string action,string[] symbols,Vector2 anchor,Rect label,int family)
        {keyboardHelpCalloutsV56.Add(new KeyboardHelpCalloutV56{Key=key,Action=action,Symbols=symbols,Anchor=anchor,Label=label,Family=family});}
        void BuildKeyboardHelpCalloutsV56()
        {
            keyboardHelpCalloutsV56.Clear();bool bag=World.BagCameraOwnsView,inspection=World.Model.Phase==PanicPhase.Inspection;
            var left=new Rect(60,290,270,64);var right=new Rect(1270,290,270,64);int l=0,r=0;
            Action<string,string,string[],Vector2,int> addLeft=(key,action,symbols,anchor,family)=>{var rect=left;rect.y+=l++*70;AddKeyboardHelpCalloutV56(key,action,symbols,anchor,rect,family);};
            Action<string,string,string[],Vector2,int> addRight=(key,action,symbols,anchor,family)=>{var rect=right;rect.y+=r++*70;AddKeyboardHelpCalloutV56(key,action,symbols,anchor,rect,family);};
            addLeft("W A S D","Hareket et",new[]{"W","A","S","D"},new Vector2(104,100),1);
            addLeft("OK TUŞLARI","Hareket et",new[]{"↑","←","↓","→"},new Vector2(376,174),1);
            addLeft("SHIFT","Koş",new[]{"Shift"},new Vector2(43,143),1);
            if(!bag)addLeft("SPACE",World.WorkActive?"İş eylemi":"Zıpla",new[]{"Space"},new Vector2(194,182),1);
            if(!inspection)addLeft("E",bag?"Etkileş / erzak al":"Etkileş / dolaptan topla",new[]{"E"},new Vector2(136,83),2);
            if(!inspection&&bag)addLeft("F","Seçili erzağı al",new[]{"F"},new Vector2(168,113),2);
            if(!inspection)addRight("TAB / B","Çantayı aç / kapat",new[]{"Tab","B"},new Vector2(31,83),3);
            addRight("J","Defteri aç / kapat",new[]{"J"},new Vector2(264,113),3);
            if(!bag)addRight("Q","Hedef değiştir",new[]{"Q"},new Vector2(72,83),2);
            addRight("ENTER",inspection?"Hasar gezisinde ilerle":"Çanta ekranında onayla",new[]{"Enter"},new Vector2(402,98),4);
            if(!inspection)addRight("BACKSPACE","Pencerede geri / iptal",new[]{"Back"},new Vector2(376,53),3);
            if(inspection)addRight("N","Hasar gezisini bitir",new[]{"N"},new Vector2(256,143),4);
            addRight("F1 / ESC","Rehberi kapat",new[]{"F1","Esc"},new Vector2(74,21),3);
            if(string.IsNullOrEmpty(DeveloperUnavailableReason()))AddKeyboardHelpCalloutV56("F2","Geliştirici menüsü",new[]{"F2"},new Vector2(108,21),new Rect(360,190,260,72),3);
            if(World.CanDeveloperSkipWaveV51)AddKeyboardHelpCalloutV56("F3","Ödülsüz dalga geçişi",new[]{"F3"},new Vector2(142,21),new Rect(640,190,286,72),3);
            var selected=World.Selected;
            if(!bag&&!inspection&&selected!=null&&World.CanInteract)
            {
                int shortcut=PanicItemActionRules.CanReleaseAnchor(selected)?1:World.Expedition.PrimaryShortcutV414(selected);
                if(shortcut>0)AddKeyboardHelpCalloutV56(shortcut.ToString(),PanicItemActionRules.CanReleaseAnchor(selected)?"Sabitlemeyi çöz":World.Expedition.PrimaryLabelV414(selected),new[]{shortcut.ToString()},new Vector2(24+(shortcut-1)*32,53),new Rect(946,190,294,72),2);
                if(selected.Movable&&!selected.Anchored)AddKeyboardHelpCalloutV56("4","Seçili eşyayı taşı",new[]{"4"},new Vector2(120,53),new Rect(946,744,294,58),2);
            }
        }
        int KeyboardHelpKeyFamilyV56(string key)
        {
            foreach(var callout in keyboardHelpCalloutsV56)if(Array.IndexOf(callout.Symbols,key)>=0)return callout.Family;
            return 0;
        }
        void EnsureKeyboardHelpStylesV56()
        {
            if(keyboardHelpTitleStyleV56!=null)return;
            keyboardHelpTitleStyleV56=new GUIStyle(_heading){fontSize=38,alignment=TextAnchor.MiddleLeft,wordWrap=false};
            keyboardHelpKeyStyleV56=new GUIStyle(_body){fontSize=23,alignment=TextAnchor.UpperLeft,wordWrap=false};
            keyboardHelpLabelStyleV56=new GUIStyle(_small){fontSize=22,alignment=TextAnchor.UpperLeft,wordWrap=true};
        }
        void DrawKeyboardHelpV56()
        {
            if(!KeyboardHelpVisibleV56)return;EnsureCompactStylesV5();EnsureKeyboardHelpStylesV56();keyboardHelpDrawingV56=true;
            float ease=KeyboardHelpEaseV56,labels=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.35f,.92f,ease));Color incoming=GUI.color;Rect bounds=KeyboardHelpBoundsV56;
            try
            {
                Fill(new Rect(0,0,CanvasWidth,CanvasHeight),new Color(.025f,.033f,.04f,.88f*ease));Region(new Rect(0,0,CanvasWidth,CanvasHeight));
                GUI.color=new Color(1,1,1,ease);Frame(KeyboardHelpPanelBoundsV56,KeyboardHelpFadeV56(Night,ease),KeyboardHelpFadeV56(Line,ease),1);Label(new Rect(76,110,1110,52),"KLAVYE REHBERİ",keyboardHelpTitleStyleV56);
                Label(new Rect(78,163,1150,28),"OYUN DURDU  ·  Tuşları incele, F1 veya Esc ile kaldığın yerden devam et.",ColorText(_small,SoftText));
                GUI.color=Color.white;int firstMark=_uiRects.Count;Matrix4x4 outer=GUI.matrix;
                GUI.matrix=outer*Matrix4x4.TRS(new Vector3(bounds.x,bounds.y,0),Quaternion.identity,new Vector3(bounds.width/440f,bounds.height/210f,1));
                try{DrawCompactKeyboardBodyV51(new Rect(0,0,440,210));}finally{GUI.matrix=outer;for(int i=firstMark;i<_uiRects.Count;i++){var child=_uiRects[i];_uiRects[i]=new Rect(bounds.x+child.x*bounds.width/440f,bounds.y+child.y*bounds.height/210f,child.width*bounds.width/440f,child.height*bounds.height/210f);}}
                GUI.color=new Color(1,1,1,labels);
                var text=new StringBuilder("KLAVYE REHBERİ · OYUN DURDU\n");
                foreach(var callout in keyboardHelpCalloutsV56)
                {
                    Vector2 key=new Vector2(bounds.x+callout.Anchor.x*bounds.width/440f,bounds.y+callout.Anchor.y*bounds.height/210f);
                    Rect label=callout.Label;Vector2 end=label.xMax<=bounds.x?new Vector2(label.xMax+5,label.center.y):label.xMin>=bounds.xMax?new Vector2(label.xMin-5,label.center.y):new Vector2(label.center.x,label.yMax<bounds.y?label.yMax+5:label.yMin-5);
                    Color accent=callout.Family==1?Sea:callout.Family==2?Brass:callout.Family==4?MainText:SoftText;
                    DrawLine(end,key,new Color(accent.r,accent.g,accent.b,.7f*labels),1.4f);Fill(new Rect(key.x-3,key.y-3,6,6),KeyboardHelpFadeV56(accent,labels));
                    Frame(label,KeyboardHelpFadeV56(Panel,labels),KeyboardHelpFadeV56(Line,labels),1);Label(new Rect(label.x+12,label.y+7,label.width-24,24),callout.Key,ColorText(keyboardHelpKeyStyleV56,accent));
                    FittedLabel(new Rect(label.x+12,label.y+32,label.width-24,label.height-34),callout.Action,keyboardHelpLabelStyleV56,MainText,17);
                    text.Append(callout.Key).Append(" · ").Append(callout.Action).Append('\n');
                }
                GUI.color=new Color(1,1,1,ease);Rect close=new Rect(1260,788,280,48);Frame(close,KeyboardHelpFadeV56(Sea,ease),KeyboardHelpFadeV56(Sea,ease),1);FittedLabel(new Rect(close.x+16,close.y+3,close.width-32,close.height-6),"F1 / ESC · OYUNA DÖN",_body,Night,18);Region(close);
                if(KeyboardHelpButtonInputV56&&MousePressed(close)){CloseKeyboardHelpV56();World.PlayUiSound();}
                Label(new Rect(360,810,850,28),"Yalnız mevcut aşamada kullanılan tuşlar renklidir.",ColorText(_small,SoftText));
                if(Event.current.type==EventType.Repaint&&keyboardHelpRepaintFrameV56!=Time.frameCount){keyboardHelpRepaintFrameV56=Time.frameCount;KeyboardHelpDrawFramesV56++;KeyboardHelpCalloutCountV56=keyboardHelpCalloutsV56.Count;KeyboardHelpRenderedTextV56=text.ToString();}
            }
            finally{GUI.color=incoming;keyboardHelpDrawingV56=false;}
        }
    }
}
