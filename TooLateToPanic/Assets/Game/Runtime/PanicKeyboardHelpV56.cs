using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicKeyboardHelpCatalogEntryV57
    {
        public string Id { get; } public IReadOnlyList<string> Keys { get; }
        public string Action { get; } public string Context { get; } public int Family { get; }
        internal PanicKeyboardHelpCatalogEntryV57(string id,string[] keys,string action,string context,int family)
        {Id=id;Keys=Array.AsReadOnly(keys);Action=action;Context=context;Family=family;}
    }
    public sealed class PanicKeyboardHelpGeometryV57
    {
        public string ConnectionId { get; } public string KeyId { get; }
        public Rect KeyRect { get; } public Rect LabelRect { get; } public Rect TextRect { get; }
        public Vector2 TextMeasuredSize { get; } public int TextFontSize { get; } public bool TextFits { get; }
        internal PanicKeyboardHelpGeometryV57(string id,string key,Rect cap,Rect label,Rect text,Vector2 measured,int font,bool fits)
        {ConnectionId=id;KeyId=key;KeyRect=cap;LabelRect=label;TextRect=text;TextMeasuredSize=measured;TextFontSize=font;TextFits=fits;}
    }
    public sealed class PanicKeyboardHelpSegmentV57
    {
        public string ConnectionId { get; } public string KeyId { get; }
        public Vector2 Start { get; } public Vector2 End { get; }
        internal PanicKeyboardHelpSegmentV57(string id,string key,Vector2 a,Vector2 b)
        {ConnectionId=id;KeyId=key;Start=a;End=b;}
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
            get { return LerpKeyboardHelpRectV56(new Rect(24,685,308,147),new Rect(60,190,1480,608),KeyboardHelpEaseV56); }
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
        sealed class KeyboardHelpTileV57
        {
            internal PanicKeyboardHelpCatalogEntryV57 Entry;
            internal Rect Label;
            internal Rect[] Caps;
        }
        static readonly Rect KeyboardHelpMapV57 = new Rect(60,190,1480,608);
        static readonly List<KeyboardHelpTileV57> keyboardHelpTilesV57 = CreateKeyboardHelpMapV57();
        static readonly IReadOnlyList<PanicKeyboardHelpCatalogEntryV57> keyboardHelpCatalogV57 = CreateKeyboardHelpCatalogV57();
        IReadOnlyList<PanicKeyboardHelpGeometryV57> keyboardHelpGeometryV57 = Array.Empty<PanicKeyboardHelpGeometryV57>();
        IReadOnlyList<PanicKeyboardHelpSegmentV57> keyboardHelpSegmentsV57 = Array.Empty<PanicKeyboardHelpSegmentV57>();
        public IReadOnlyList<PanicKeyboardHelpCatalogEntryV57> KeyboardHelpCatalogV57 { get { return keyboardHelpCatalogV57; } }
        public IReadOnlyList<PanicKeyboardHelpGeometryV57> KeyboardHelpKeyGeometryV57 { get { return keyboardHelpGeometryV57; } }
        public IReadOnlyList<PanicKeyboardHelpSegmentV57> KeyboardHelpConnectorSegmentsV57 { get { return keyboardHelpSegmentsV57; } }
        public int KeyboardHelpGeometryDrawFrameV57 { get; private set; } = -1;

        static IReadOnlyList<PanicKeyboardHelpCatalogEntryV57> CreateKeyboardHelpCatalogV57()
        {
            var result = new List<PanicKeyboardHelpCatalogEntryV57>();
            foreach (var tile in keyboardHelpTilesV57) result.Add(tile.Entry);
            return result.AsReadOnly();
        }
        static List<KeyboardHelpTileV57> CreateKeyboardHelpMapV57()
        {
            var map = new List<KeyboardHelpTileV57>();
            Action<string,string,string,string,int,float,float,float,float> add = (id,key,action,context,family,x,y,width,capWidth) =>
            {
                map.Add(new KeyboardHelpTileV57 {
                    Entry = new PanicKeyboardHelpCatalogEntryV57(id,new[]{key},action,context,family),
                    Label = new Rect(x,y+42,width,46),
                    Caps = new[]{new Rect(x+(width-capWidth)*.5f,y,capWidth,32)} });
            };
            add("esc","Esc","Duraklat / geri","Rehberde: kapat",3,80,194,166,90);
            add("f1","F1","Tuş rehberi","Aç / kapat",3,260,194,166,76);
            add("f2","F2","Geliştirici menüsü","Oyun aşaması uygunsa",3,440,194,166,76);
            add("f3","F3","Ödülsüz dalga geç","Skor dışı",3,620,194,166,76);
            add("pageup","PageUp","Kamerayı yakınlaştır","Defter / döküm: geri",3,1180,194,166,142);
            add("pagedown","PageDown","Kamerayı uzaklaştır","Defter / döküm: ileri",3,1370,194,166,150);
            add("1","1","Sabitle / ankrajı çöz","Hazırlıkta",2,80,290,166,76);
            add("2","2","Yangın tüpü hazırla","Ekipman noktasında",2,250,290,166,76);
            add("3","3","Yapıyı güçlendir","Duvar: tüm kat",2,420,290,166,76);
            add("4","4","Seçili eşyayı taşı","Ankrajı çözülmüş eşya",2,590,290,166,76);
            add("back","Back","Geri / işi iptal et","Çantayı da kapatır",3,1300,290,236,190);
            add("tab","Tab","Çantayı aç / kapat","Envanterde de geçerli",3,80,386,156,106);
            add("q","Q","Hedefi değiştir","İzometrik görünümde",2,260,386,166,76);
            add("w","W","İleri yürü / seç","Envanterde: yukarı",1,440,386,166,76);
            add("e","E","Etkileş / dolabı aç","Malzeme: açınca al",2,620,386,166,76);
            add("r","R","Taşırken 45° döndür","Shift+R: ters yön",2,800,386,166,76);
            add("t","T","Ankraj yüzeyi seç","Duvar / zemin",2,980,386,166,76);
            add("enter","Enter","Onayla / sahneyi atla","Gezide: bir adım ilerle",4,1360,386,176,136);
            add("a","A","Sola yürü / seç","Envanterde: sola",1,290,482,166,76);
            add("s","S","Geri yürü / seç","Envanterde: aşağı",1,470,482,166,76);
            add("d","D","Sağa yürü / seç","Envanterde: sağa",1,650,482,166,76);
            add("f","F","Seçili erzağı al","FPS çanta aşamasında",2,830,482,166,76);
            add("h","H","Kısa yardımı aç/kapat","Hazırlıkta",3,1190,482,166,76);
            add("j","J","Defteri aç / kapat","Oyun aşaması uygunsa",3,1370,482,166,76);
            add("shift","Shift","Sol Shift: koş","R ile: tersine döndür",1,80,578,196,132);
            add("z","Z","Önceki malzemeyi seç","Açık dolapta",2,300,578,166,76);
            add("x","X","Sonraki malzemeyi seç","Açık dolapta",2,480,578,166,76);
            add("b","B","Çantayı aç / kapat","Tab ile aynı işlev",3,840,578,166,76);
            add("n","N","Hasar gezisini bitir","Öğreticiyi de bitirir",4,1020,578,166,76);
            add("space","Space","Zıpla / çekiçle vur","Menüde: seçili geliştirici seçeneğini onayla",1,440,674,600,600);
            map.Add(new KeyboardHelpTileV57 {
                Entry = new PanicKeyboardHelpCatalogEntryV57("arrows",new[]{"↑","←","↓","→"},"Yürü / menüde seç","İşte: → vana / ↓ şalter",1),
                Label = new Rect(1200,752,336,46),
                Caps = new[]{new Rect(1320,674,44,30),new Rect(1260,710,44,30),new Rect(1320,710,44,30),new Rect(1380,710,44,30)} });
            return map;
        }
        void BuildKeyboardHelpCalloutsV56() { KeyboardHelpCalloutCountV56 = keyboardHelpCatalogV57.Count; }
        int KeyboardHelpKeyFamilyV56(string key)
        {
            if(key=="PgUp")key="PageUp";else if(key=="PgDn")key="PageDown";
            foreach(var entry in keyboardHelpCatalogV57) foreach(var symbol in entry.Keys)
                if(string.Equals(symbol,key,StringComparison.OrdinalIgnoreCase)) return entry.Family;
            return 0;
        }
        void EnsureKeyboardHelpStylesV56()
        {
            if(keyboardHelpTitleStyleV56!=null)return;
            keyboardHelpTitleStyleV56=new GUIStyle(_heading){fontSize=38,alignment=TextAnchor.MiddleLeft,wordWrap=false};
            keyboardHelpKeyStyleV56=new GUIStyle(_body){fontSize=25,alignment=TextAnchor.MiddleCenter,wordWrap=false};
            keyboardHelpLabelStyleV56=new GUIStyle(_small){font=_condensedFont,fontSize=20,alignment=TextAnchor.MiddleCenter,wordWrap=false};
        }
        Rect KeyboardHelpTransformRectV57(Rect rect)
        {
            Rect bounds=KeyboardHelpBoundsV56;float sx=bounds.width/KeyboardHelpMapV57.width,sy=bounds.height/KeyboardHelpMapV57.height;
            return new Rect(bounds.x+(rect.x-KeyboardHelpMapV57.x)*sx,bounds.y+(rect.y-KeyboardHelpMapV57.y)*sy,rect.width*sx,rect.height*sy);
        }
        Vector2 KeyboardHelpTransformPointV57(Vector2 point)
        { Rect r=KeyboardHelpTransformRectV57(new Rect(point,Vector2.zero));return r.position; }
        static Color KeyboardHelpAccentV57(int family)
        { return family==1?Sea:family==2?Brass:family==4?MainText:SoftText; }
        void DrawKeyboardHelpV56()
        {
            if(!KeyboardHelpVisibleV56)return;
            EnsureCompactStylesV5();EnsureKeyboardHelpStylesV56();keyboardHelpDrawingV56=true;
            float ease=KeyboardHelpEaseV56,labels=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.7f,1f,ease));
            Color incoming=GUI.color;Matrix4x4 outer=GUI.matrix;Rect bounds=KeyboardHelpBoundsV56;
            var geometry=new List<PanicKeyboardHelpGeometryV57>();var segments=new List<PanicKeyboardHelpSegmentV57>();
            var rendered=new StringBuilder("KLAVYE REHBERİ · TÜM KULLANILAN TUŞLAR\nOYUN VE SÜRE DURDU\n");
            try
            {
                Fill(LogicalCanvasBounds,new Color(.025f,.033f,.04f,.88f*ease));Region(LogicalCanvasBounds);
                Frame(KeyboardHelpPanelBoundsV56,Night,Line,1);
                Label(new Rect(76,108,1400,52),"KLAVYE REHBERİ · TÜM TUŞLAR",keyboardHelpTitleStyleV56);
                Label(new Rect(78,161,1400,27),"OYUN DURDU  ·  Tuşlar her zaman görünür; ikinci satır nerede kullanıldığını açıklar.",_small);
                float sx=bounds.width/KeyboardHelpMapV57.width,sy=bounds.height/KeyboardHelpMapV57.height;
                GUI.matrix=outer*Matrix4x4.TRS(new Vector3(bounds.x-KeyboardHelpMapV57.x*sx,bounds.y-KeyboardHelpMapV57.y*sy,0),Quaternion.identity,new Vector3(sx,sy,1));
                int firstMark=_uiRects.Count;
                foreach(var tile in keyboardHelpTilesV57)
                {
                    var entry=tile.Entry;Color accent=KeyboardHelpAccentV57(entry.Family);
                    string text=entry.Action+"\n"+entry.Context;Rect textRect=new Rect(tile.Label.x+6,tile.Label.y+1,tile.Label.width-12,tile.Label.height-2);
                    keyboardHelpLabelStyleV56.fontSize=20;
                    Vector2 measured=keyboardHelpLabelStyleV56.CalcSize(new GUIContent(text));
                    while(keyboardHelpLabelStyleV56.fontSize>18&&(measured.x>textRect.width||measured.y>textRect.height))
                    {keyboardHelpLabelStyleV56.fontSize--;measured=keyboardHelpLabelStyleV56.CalcSize(new GUIContent(text));}
                    Frame(tile.Label,Panel,Line,1);GUI.color=new Color(1,1,1,labels);
                    Label(textRect,text,keyboardHelpLabelStyleV56);
                    rendered.Append(string.Join(" / ",entry.Keys)).Append(" · ").Append(entry.Action).Append(" · ").Append(entry.Context).Append('\n');
                    for(int i=0;i<tile.Caps.Length;i++)
                    {
                        Rect cap=tile.Caps[i];string key=entry.Keys[i];GUI.color=Color.white;
                        Frame(cap,Raised,accent,2);Label(new Rect(cap.x+4,cap.y+1,cap.width-8,cap.height-2),key,ColorText(keyboardHelpKeyStyleV56,MainText));
                        var points=new List<Vector2>();
                        if(entry.Id=="arrows"&&i==0)
                        {points.Add(new Vector2(cap.xMax,cap.center.y));points.Add(new Vector2(1450,cap.center.y));points.Add(new Vector2(1450,tile.Label.y));}
                        else {points.Add(new Vector2(cap.center.x,cap.yMax));points.Add(new Vector2(cap.center.x,tile.Label.y));}
                        for(int p=0;p+1<points.Count;p++)
                        {
                            DrawLine(points[p],points[p+1],new Color(.015f,.012f,.01f,.95f*labels),5f);
                            DrawLine(points[p],points[p+1],new Color(accent.r,accent.g,accent.b,.85f*labels),1.8f);
                            segments.Add(new PanicKeyboardHelpSegmentV57(entry.Id,key,KeyboardHelpTransformPointV57(points[p]),KeyboardHelpTransformPointV57(points[p+1])));
                        }
                        foreach(var point in new[]{points[0],points[points.Count-1]})
                        {Fill(new Rect(point.x-3,point.y-3,6,6),new Color(.015f,.012f,.01f,labels));Fill(new Rect(point.x-1.5f,point.y-1.5f,3,3),new Color(accent.r,accent.g,accent.b,labels));}
                        geometry.Add(new PanicKeyboardHelpGeometryV57(entry.Id,key,KeyboardHelpTransformRectV57(cap),KeyboardHelpTransformRectV57(tile.Label),KeyboardHelpTransformRectV57(textRect),new Vector2(measured.x*sx,measured.y*sy),keyboardHelpLabelStyleV56.fontSize,measured.x<=textRect.width+.01f&&measured.y<=textRect.height+.01f));
                    }
                }
                GUI.matrix=outer;GUI.color=Color.white;
                for(int i=firstMark;i<_uiRects.Count;i++)_uiRects[i]=KeyboardHelpTransformRectV57(_uiRects[i]);
                Rect close=new Rect(1260,804,280,44);Frame(close,Sea,Sea,1);Label(new Rect(close.x+12,close.y+2,close.width-24,close.height-4),"F1 / ESC · OYUNA DÖN",ColorText(_body,Night));Region(close);
                if(KeyboardHelpButtonInputV56&&MousePressed(close)){CloseKeyboardHelpV56();World.PlayUiSound();}
                Label(new Rect(78,810,1150,28),"Kısa bağlantı aynı tuşun açıklamasına gider. Koşullar tuşların ikinci satırındadır.",_small);
                if(Event.current.type==EventType.Repaint&&keyboardHelpRepaintFrameV56!=Time.frameCount)
                {
                    keyboardHelpRepaintFrameV56=Time.frameCount;KeyboardHelpDrawFramesV56++;
                    KeyboardHelpCalloutCountV56=keyboardHelpCatalogV57.Count;KeyboardHelpRenderedTextV56=rendered.ToString();
                    keyboardHelpGeometryV57=geometry.AsReadOnly();keyboardHelpSegmentsV57=segments.AsReadOnly();KeyboardHelpGeometryDrawFrameV57=Time.frameCount;
                }
            }
            finally{GUI.matrix=outer;GUI.color=incoming;keyboardHelpDrawingV56=false;}
        }
    }
}
