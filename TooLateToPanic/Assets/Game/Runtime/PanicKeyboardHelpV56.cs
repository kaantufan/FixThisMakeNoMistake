using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace TooLateToPanic
{
    public sealed class PanicKeyboardHelpCatalogEntryV57
    {
        public string Id { get; } public IReadOnlyList<string> Keys { get; }
        public string RepresentativeKey { get; } public string DisplayKeys { get; }
        public string Action { get; } public string Context { get; } public int Family { get; }
        internal PanicKeyboardHelpCatalogEntryV57(string id,string[] keys,string action,string context,int family,string representative,string displayKeys)
        {Id=id;Keys=Array.AsReadOnly(keys);Action=action;Context=context;Family=family;RepresentativeKey=representative;DisplayKeys=displayKeys;}
    }
    public sealed class PanicKeyboardHelpGeometryV57
    {
        public string ConnectionId { get; } public string KeyId { get; }
        public bool HasConnector { get; } public string ConnectorKeyId { get; } public string RenderedCaptionText { get; }
        public Rect KeyRect { get; } public Rect LabelRect { get; } public Rect TextRect { get; }
        public Vector2 TextMeasuredSize { get; } public int TextFontSize { get; } public bool TextFits { get; }
        internal PanicKeyboardHelpGeometryV57(string id,string key,Rect cap,Rect label,Rect text,Vector2 measured,int font,bool fits,string representative,string caption)
        {ConnectionId=id;KeyId=key;KeyRect=cap;LabelRect=label;TextRect=text;TextMeasuredSize=measured;TextFontSize=font;TextFits=fits;HasConnector=key==representative;ConnectorKeyId=representative;RenderedCaptionText=caption;}
    }
    public sealed class PanicKeyboardHelpSegmentV57
    {
        public string ConnectionId { get; } public string KeyId { get; }
        public Vector2 Start { get; } public Vector2 End { get; }
        internal PanicKeyboardHelpSegmentV57(string id,string key,Vector2 a,Vector2 b)
        {ConnectionId=id;KeyId=key;Start=a;End=b;}
    }
    public sealed class PanicKeyboardHelpCapV57
    {
        public string KeyId { get; } public Rect KeyRect { get; } public bool Highlighted { get; }
        internal PanicKeyboardHelpCapV57(string key,Rect rect,bool highlighted){KeyId=key;KeyRect=rect;Highlighted=highlighted;}
    }
    /// <summary>F1 explanatory modal reuses the actual physical keyboard layout. Animation uses unscaled time; game pause is model-owned.</summary>
    public sealed partial class PanicHomeHud
    {
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
        sealed class KeyboardHelpClassicCalloutV57
        {
            internal PanicKeyboardHelpCatalogEntryV57 Entry;
            internal Rect Label;
            internal Vector2[] Points;
        }
        static readonly Rect KeyboardHelpClassicBoundsV57=new Rect(360,315,880,420);
        static readonly List<KeyboardHelpClassicCalloutV57> keyboardHelpClassicV57=CreateKeyboardHelpClassicV57();
        static readonly IReadOnlyList<PanicKeyboardHelpCatalogEntryV57> keyboardHelpCatalogV57=CreateKeyboardHelpCatalogV57();
        IReadOnlyList<PanicKeyboardHelpGeometryV57> keyboardHelpGeometryV57=Array.Empty<PanicKeyboardHelpGeometryV57>();
        IReadOnlyList<PanicKeyboardHelpSegmentV57> keyboardHelpSegmentsV57=Array.Empty<PanicKeyboardHelpSegmentV57>();
        IReadOnlyList<PanicKeyboardHelpCapV57> keyboardHelpAllCapsV57=Array.Empty<PanicKeyboardHelpCapV57>();
        readonly List<PanicKeyboardHelpCapV57> keyboardHelpCollectV57=new List<PanicKeyboardHelpCapV57>(56);
        bool keyboardHelpCollectingV57;int keyboardHelpBlankV57;
        public IReadOnlyList<PanicKeyboardHelpCatalogEntryV57> KeyboardHelpCatalogV57 { get { return keyboardHelpCatalogV57; } }
        public IReadOnlyList<PanicKeyboardHelpGeometryV57> KeyboardHelpKeyGeometryV57 { get { return keyboardHelpGeometryV57; } }
        public IReadOnlyList<PanicKeyboardHelpSegmentV57> KeyboardHelpConnectorSegmentsV57 { get { return keyboardHelpSegmentsV57; } }
        public IReadOnlyList<PanicKeyboardHelpCapV57> KeyboardHelpAllCapsV57 { get { return keyboardHelpAllCapsV57; } }
        public int KeyboardHelpGeometryDrawFrameV57 { get;private set; }=-1;
        public Rect KeyboardHelpCloseBoundsV57 { get { return new Rect(1260,111,280,44); } }
        public Rect KeyboardHelpTitleBoundsV57 { get { return new Rect(76,105,1100,52); } }
        public Rect KeyboardHelpSubtitleBoundsV57 { get { return new Rect(78,158,1150,28); } }
        static IReadOnlyList<PanicKeyboardHelpCatalogEntryV57> CreateKeyboardHelpCatalogV57()
        {var result=new List<PanicKeyboardHelpCatalogEntryV57>();foreach(var row in keyboardHelpClassicV57)result.Add(row.Entry);return result.AsReadOnly();}
        static List<KeyboardHelpClassicCalloutV57> CreateKeyboardHelpClassicV57()
        {
            var result=new List<KeyboardHelpClassicCalloutV57>();
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("escape",new[]{"Esc"},"Duraklat / geri","Rehberde: kapat",3,"Esc","ESC"),Label=new Rect(60,194,236,96),Points=new[]{new Vector2(416,333),new Vector2(416,315),new Vector2(416,294),new Vector2(178,294),new Vector2(178,290)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("actions-1-4",new[]{"1","2","3","4"},"1 Sabitle/çöz · 2 Yangın tüpü\n3 Güçlendir · 4 Taşı","Hazırlık · 2 istasyon · 4 serbest",2,"1","1 / 2 / 3 / 4"),Label=new Rect(304,194,236,96),Points=new[]{new Vector2(408,395),new Vector2(408,387),new Vector2(464,387),new Vector2(464,315),new Vector2(464,298),new Vector2(422,298),new Vector2(422,290)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("help",new[]{"F1"},"Tuş rehberi","Aç / kapat",3,"F1","F1"),Label=new Rect(548,194,236,96),Points=new[]{new Vector2(508,333),new Vector2(508,315),new Vector2(508,292),new Vector2(666,292),new Vector2(666,290)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("developer",new[]{"F2"},"Geliştirici menüsü","Oyun aşaması uygunsa",3,"F2","F2"),Label=new Rect(792,194,236,96),Points=new[]{new Vector2(576,333),new Vector2(576,315),new Vector2(576,296),new Vector2(910,296),new Vector2(910,290)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("skip-wave",new[]{"F3"},"Ödülsüz dalga geç","Skor dışı",3,"F3","F3"),Label=new Rect(1036,194,236,96),Points=new[]{new Vector2(644,333),new Vector2(644,315),new Vector2(644,300),new Vector2(1154,300),new Vector2(1154,290)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("camera-pages",new[]{"PageUp","PageDown"},"PgUp yakın / PgDn uzak","Defter/döküm: geri / ileri",3,"PageUp","PAGE UP / PAGE DOWN"),Label=new Rect(1280,194,236,96),Points=new[]{new Vector2(1062,333),new Vector2(1062,315),new Vector2(1062,304),new Vector2(1262,304),new Vector2(1262,294),new Vector2(1280,294),new Vector2(1280,290)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("select",new[]{"Q"},"Hedefi değiştir","İzometrik görünümde",2,"Q","Q"),Label=new Rect(60,300,270,72),Points=new[]{new Vector2(504,455),new Vector2(504,451),new Vector2(360,451),new Vector2(354,451),new Vector2(354,336),new Vector2(330,336)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("inventory",new[]{"Tab","B"},"Envanteri aç / kapat","Envanterde de geçerli",3,"Tab","TAB / B"),Label=new Rect(60,378,270,72),Points=new[]{new Vector2(380,481),new Vector2(360,481),new Vector2(350,481),new Vector2(350,414),new Vector2(330,414)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("take",new[]{"F"},"Seçili erzağı al","FPS çanta aşamasında",2,"F","F"),Label=new Rect(60,456,270,72),Points=new[]{new Vector2(696,515),new Vector2(696,511),new Vector2(360,511),new Vector2(346,511),new Vector2(346,492),new Vector2(330,492)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("move-wasd",new[]{"W","A","S","D"},"Yürü / envanterde seç","Yönler: ileri / sol / geri / sağ",1,"A","W / A / S / D"),Label=new Rect(60,534,270,72),Points=new[]{new Vector2(504,567),new Vector2(504,571),new Vector2(360,571),new Vector2(342,571),new Vector2(342,570),new Vector2(330,570)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("run",new[]{"Shift"},"Sol Shift: koş","R ile: tersine döndür",1,"Shift","SHIFT"),Label=new Rect(60,612,270,72),Points=new[]{new Vector2(380,601),new Vector2(360,601),new Vector2(334,601),new Vector2(334,648),new Vector2(330,648)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("loot-choice",new[]{"Z","X"},"Z önceki / X sonraki","Açık dolaptaki malzeme",2,"Z","Z / X"),Label=new Rect(60,690,270,72),Points=new[]{new Vector2(552,627),new Vector2(552,639),new Vector2(360,639),new Vector2(338,639),new Vector2(338,726),new Vector2(330,726)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("back",new[]{"Back"},"Geri / işi iptal et","Envanteri de kapatır",3,"Back","BACKSPACE"),Label=new Rect(1270,300,270,72),Points=new[]{new Vector2(1204,421),new Vector2(1240,421),new Vector2(1246,421),new Vector2(1246,336),new Vector2(1270,336)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("interact",new[]{"E"},"Etkileş / dolabı aç","Malzeme: açınca al",2,"E","E"),Label=new Rect(1270,378,270,72),Points=new[]{new Vector2(632,455),new Vector2(632,448),new Vector2(1240,448),new Vector2(1250,448),new Vector2(1250,414),new Vector2(1270,414)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("rotate",new[]{"R"},"Taşırken 45° döndür","Shift+R: ters yön",2,"R","R"),Label=new Rect(1270,456,270,72),Points=new[]{new Vector2(696,455),new Vector2(696,451),new Vector2(1240,451),new Vector2(1266,451),new Vector2(1266,492),new Vector2(1270,492)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("anchor-mode",new[]{"T"},"Ankraj yüzeyi seç","Duvar / zemin",2,"T","T"),Label=new Rect(1270,534,270,72),Points=new[]{new Vector2(760,455),new Vector2(760,454),new Vector2(1240,454),new Vector2(1262,454),new Vector2(1262,570),new Vector2(1270,570)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("confirm",new[]{"Enter"},"Onayla / sahneyi atla","Gezide: bir adım ilerle",4,"Enter","ENTER"),Label=new Rect(1270,612,270,72),Points=new[]{new Vector2(1204,511),new Vector2(1240,511),new Vector2(1258,511),new Vector2(1258,648),new Vector2(1270,648)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("notebook",new[]{"J"},"Defteri aç / kapat","Oyun aşaması uygunsa",3,"J","J"),Label=new Rect(1270,690,270,72),Points=new[]{new Vector2(888,567),new Vector2(888,569),new Vector2(1240,569),new Vector2(1254,569),new Vector2(1254,726),new Vector2(1270,726)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("legacy-help",new[]{"H"},"Kısa yardımı aç/kapat","Hazırlıkta",3,"H","H"),Label=new Rect(60,772,354,74),Points=new[]{new Vector2(824,567),new Vector2(824,571),new Vector2(776,571),new Vector2(776,639),new Vector2(566,639),new Vector2(566,735),new Vector2(566,766),new Vector2(237,766),new Vector2(237,772)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("jump-work",new[]{"Space"},"Zıpla / işte çekiçle vur","Geliştirici menüsü: onay",1,"Space","SPACE"),Label=new Rect(430,772,354,74),Points=new[]{new Vector2(748,707),new Vector2(748,735),new Vector2(748,768),new Vector2(607,768),new Vector2(607,772)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("next-wave",new[]{"N"},"Hasar gezisini bitir","Öğreticiyi de bitirir",4,"N","N"),Label=new Rect(800,772,354,74),Points=new[]{new Vector2(872,627),new Vector2(872,639),new Vector2(930,639),new Vector2(930,735),new Vector2(930,766),new Vector2(977,766),new Vector2(977,772)}});
            result.Add(new KeyboardHelpClassicCalloutV57{Entry=new PanicKeyboardHelpCatalogEntryV57("move-arrows",new[]{"↑","←","↓","→"},"Yürü / menüde seç","İşte: → vana / ↓ şalter",1,"↓","↑ / ← / ↓ / →"),Label=new Rect(1170,772,354,74),Points=new[]{new Vector2(1112,707),new Vector2(1112,735),new Vector2(1112,768),new Vector2(1347,768),new Vector2(1347,772)}});
            return result;
        }
        void BuildKeyboardHelpCalloutsV56(){KeyboardHelpCalloutCountV56=keyboardHelpCatalogV57.Count;}
        static string KeyboardHelpNormalizeV57(string key)
        {return key=="PgUp"?"PageUp":key=="PgDn"?"PageDown":key;}
        int KeyboardHelpKeyFamilyV56(string key)
        {
            key=KeyboardHelpNormalizeV57(key);
            foreach(var entry in keyboardHelpCatalogV57)foreach(var member in entry.Keys)
                if(string.Equals(member,key,StringComparison.OrdinalIgnoreCase))return entry.Family;
            return 0;
        }
        void CollectKeyboardHelpCapV57(Rect rect,string key,int family)
        {
            if(!keyboardHelpCollectingV57||Event.current.type!=EventType.Repaint)return;
            string id=string.IsNullOrEmpty(key)?"Blank"+(++keyboardHelpBlankV57).ToString("D2"):KeyboardHelpNormalizeV57(key);
            Rect bounds=KeyboardHelpBoundsV56;
            keyboardHelpCollectV57.Add(new PanicKeyboardHelpCapV57(id,new Rect(bounds.x+rect.x*bounds.width/440f,bounds.y+rect.y*bounds.height/210f,rect.width*bounds.width/440f,rect.height*bounds.height/210f),family!=0));
        }
        void EnsureKeyboardHelpStylesV56()
        {
            if(keyboardHelpTitleStyleV56!=null)return;
            keyboardHelpTitleStyleV56=new GUIStyle(_heading){fontSize=38,alignment=TextAnchor.MiddleLeft,wordWrap=false};
            keyboardHelpKeyStyleV56=new GUIStyle(_body){fontSize=23,alignment=TextAnchor.MiddleCenter,wordWrap=false};
            keyboardHelpLabelStyleV56=new GUIStyle(_small){font=_condensedFont,fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=false};
        }
        static Color KeyboardHelpAccentV57(int family)
        {return family==1?Sea:family==2?Brass:family==4?MainText:SoftText;}
        Rect KeyboardHelpTransformRectV57(Rect rect)
        {Rect bounds=KeyboardHelpBoundsV56;float scale=bounds.width/880f;return new Rect(bounds.x+(rect.x-360)*scale,bounds.y+(rect.y-315)*scale,rect.width*scale,rect.height*scale);}
        Vector2 KeyboardHelpTransformPointV57(Vector2 point)
        {return KeyboardHelpTransformRectV57(new Rect(point,Vector2.zero)).position;}
        void DrawKeyboardHelpV56()
        {
            if(!KeyboardHelpVisibleV56)return;EnsureCompactStylesV5();EnsureKeyboardHelpStylesV56();keyboardHelpDrawingV56=true;
            float ease=KeyboardHelpEaseV56,labels=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.35f,.92f,ease));Color incoming=GUI.color;
            bool repaint=Event.current.type==EventType.Repaint;
            var geometry=new List<PanicKeyboardHelpGeometryV57>(34);var segments=new List<PanicKeyboardHelpSegmentV57>(100);
            try
            {
                GUI.color=Color.white;Fill(new Rect(0,0,CanvasWidth,CanvasHeight),new Color(.025f,.033f,.04f,.88f*ease));Region(new Rect(0,0,CanvasWidth,CanvasHeight));
                GUI.color=new Color(1,1,1,ease);Frame(KeyboardHelpPanelBoundsV56,Night,Line,1);
                Label(KeyboardHelpTitleBoundsV57,"KLAVYE REHBERİ",keyboardHelpTitleStyleV56);
                Label(KeyboardHelpSubtitleBoundsV57,"OYUN DURDU  ·  Tüm tuşlar görünür; açıklamadaki koşulda kullanılır.",ColorText(_small,SoftText));
                Rect close=KeyboardHelpCloseBoundsV57;Frame(close,Sea,Sea,1);FittedLabel(new Rect(close.x+12,close.y+2,close.width-24,close.height-4),"F1 / ESC · OYUNA DÖN",_body,Night,18);Region(close);
                if(KeyboardHelpButtonInputV56&&MousePressed(close)){CloseKeyboardHelpV56();World.PlayUiSound();}
                Rect bounds=KeyboardHelpBoundsV56;Matrix4x4 outer=GUI.matrix;int firstMark=_uiRects.Count;
                if(repaint){keyboardHelpCollectV57.Clear();keyboardHelpBlankV57=0;}
                keyboardHelpCollectingV57=true;
                GUI.matrix=outer*Matrix4x4.TRS(new Vector3(bounds.x,bounds.y,0),Quaternion.identity,new Vector3(bounds.width/440f,bounds.height/210f,1));
                try{DrawCompactKeyboardBodyV51(new Rect(0,0,440,210));}
                finally
                {
                    keyboardHelpCollectingV57=false;GUI.matrix=outer;
                    for(int i=firstMark;i<_uiRects.Count;i++){Rect child=_uiRects[i];_uiRects[i]=new Rect(bounds.x+child.x*bounds.width/440f,bounds.y+child.y*bounds.height/210f,child.width*bounds.width/440f,child.height*bounds.height/210f);}
                }
                var text=new StringBuilder("KLAVYE REHBERİ · OYUN DURDU\n");float mapScale=bounds.width/880f;
                GUI.color=new Color(1,1,1,labels);
                foreach(var callout in keyboardHelpClassicV57)
                {
                    Color accent=KeyboardHelpAccentV57(callout.Entry.Family);
                    for(int i=1;i<callout.Points.Length;i++)
                    {
                        Vector2 a=KeyboardHelpTransformPointV57(callout.Points[i-1]),b=KeyboardHelpTransformPointV57(callout.Points[i]);
                        DrawLine(a,b,Night,1.8f);DrawLine(a,b,accent,1f);
                        if(repaint)segments.Add(new PanicKeyboardHelpSegmentV57(callout.Entry.Id,callout.Entry.RepresentativeKey,a,b));
                    }
                    Vector2 start=KeyboardHelpTransformPointV57(callout.Points[0]),end=KeyboardHelpTransformPointV57(callout.Points[callout.Points.Length-1]);
                    Fill(new Rect(start.x-2,start.y-2,4,4),accent);Fill(new Rect(end.x-2,end.y-2,4,4),accent);
                    Rect card=KeyboardHelpTransformRectV57(callout.Label);Frame(card,Panel,Line,1);
                    Rect textRect=new Rect(card.x+4*mapScale,card.y+2*mapScale,card.width-8*mapScale,card.height-4*mapScale);
                    var style=new GUIStyle(keyboardHelpLabelStyleV56){fontSize=18};
                    string compact=callout.Entry.DisplayKeys+" · "+callout.Entry.Action+"\n"+callout.Entry.Context;
                    string caption=style.CalcSize(new GUIContent(compact)).x<=callout.Label.width-8?compact:callout.Entry.DisplayKeys+"\n"+callout.Entry.Action+"\n"+callout.Entry.Context;
                    Vector2 measured=style.CalcSize(new GUIContent(caption));measured.y=style.CalcHeight(new GUIContent(caption),callout.Label.width-8);
                    // Apply the same animation transform to the text; settled font remains exactly18.
                    Matrix4x4 beforeText=GUI.matrix;GUI.matrix=beforeText*Matrix4x4.TRS(new Vector3(textRect.x,textRect.y,0),Quaternion.identity,new Vector3(mapScale,mapScale,1));
                    try{GUI.Label(new Rect(0,0,callout.Label.width-8,callout.Label.height-4),caption,ColorText(style,MainText));}finally{GUI.matrix=beforeText;}
                    bool fits=measured.x<=callout.Label.width-8+.01f&&measured.y<=callout.Label.height-4+.01f;
                    foreach(string member in callout.Entry.Keys)foreach(var cap in keyboardHelpCollectV57)if(cap.KeyId==member)
                        geometry.Add(new PanicKeyboardHelpGeometryV57(callout.Entry.Id,member,cap.KeyRect,card,textRect,measured*mapScale,18,fits,callout.Entry.RepresentativeKey,caption));
                    text.Append(caption).Append('\n');
                }
                if(repaint&&keyboardHelpRepaintFrameV56!=Time.frameCount)
                {
                    keyboardHelpRepaintFrameV56=Time.frameCount;KeyboardHelpDrawFramesV56++;KeyboardHelpCalloutCountV56=keyboardHelpClassicV57.Count;KeyboardHelpRenderedTextV56=text.ToString();
                    keyboardHelpGeometryV57=geometry.AsReadOnly();keyboardHelpSegmentsV57=segments.AsReadOnly();keyboardHelpAllCapsV57=new List<PanicKeyboardHelpCapV57>(keyboardHelpCollectV57).AsReadOnly();KeyboardHelpGeometryDrawFrameV57=Time.frameCount;
                }
            }
            finally{GUI.color=incoming;keyboardHelpDrawingV56=false;keyboardHelpCollectingV57=false;}
        }
    }
}
