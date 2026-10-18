using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Read-only validation of actual Repaint telemetry. Called by the existing isolated UI driver; never attaches or changes gameplay.</summary>
    public static class PanicKeyboardHelpGeometryChecksV57
    {
        const float Epsilon=.01f;
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        static readonly string[] RequiredKeys={"W","A","S","D","↑","←","↓","→","Shift","Space","E","F","Q","T","R","1","2","3","4","Tab","B","J","H","N","Z","X","PageUp","PageDown","Enter","Back","Esc","F1","F2","F3"};
        [Serializable] public sealed class Relation
        {
            public string connectionId,keyId;public Rect key,label,text;public Vector2 measuredText;public int fontSize;
        }
        [Serializable] public sealed class Segment
        {public string connectionId,keyId;public Vector2 start,end;}
        [Serializable] public sealed class DrawReceipt
        {
            public string layer="NATIVE_ACTUAL_REPAINT_TELEMETRY_NOT_HUMAN_ACCEPTANCE",error;
            public int frame,geometryFrame,repaints,width,height;public float scale;public Vector2 offset;
            public bool passed,textTelemetry;public string[] catalogKeys;
            public List<Relation> relations=new List<Relation>();public List<Segment> segments=new List<Segment>();public List<string> failures=new List<string>();
        }
        public static DrawReceipt LastReceipt {get;private set;}
        static object Read(object owner,string name)
        {var p=owner.GetType().GetProperty(name,Flags);if(p!=null)return p.GetValue(owner);var f=owner.GetType().GetField(name,Flags);if(f==null)throw new MissingMemberException(owner.GetType().FullName,name);return f.GetValue(owner);}
        static T Read<T>(object owner,string name){return (T)Read(owner,name);}
        static object[] Items(object value){return ((IEnumerable)value).Cast<object>().ToArray();}
        static bool Has(object owner,string name){return owner.GetType().GetProperty(name,Flags)!=null||owner.GetType().GetField(name,Flags)!=null;}
        static string Key(string value)
        {
            switch((value??"").Replace(" ","").ToUpperInvariant())
            {
                case "ESCAPE":case "ESC":return "Esc";case "BACKSPACE":case "BACK":return "Back";
                case "RETURN":case "KEYPADENTER":case "ENTER":return "Enter";case "LEFTSHIFT":case "RIGHTSHIFT":case "SHIFT":return "Shift";
                case "UPARROW":case "UP":case "↑":return "↑";case "LEFTARROW":case "LEFT":case "←":return "←";
                case "DOWNARROW":case "DOWN":case "↓":return "↓";case "RIGHTARROW":case "RIGHT":case "→":return "→";
                case "PAGEUP":case "PGUP":return "PageUp";case "PAGEDOWN":case "PGDN":return "PageDown";
                case "SPACE":return "Space";case "TAB":return "Tab";default:return (value??"").ToUpperInvariant();
            }
        }
        static bool Finite(float value){return !float.IsNaN(value)&&!float.IsInfinity(value);}
        static bool Finite(Vector2 p){return Finite(p.x)&&Finite(p.y);}
        static bool Valid(Rect r){return Finite(r.x)&&Finite(r.y)&&Finite(r.width)&&Finite(r.height)&&r.width>0&&r.height>0;}
        static bool Near(float a,float b){return Mathf.Abs(a-b)<=Epsilon;}
        static bool Near(Vector2 a,Vector2 b){return Near(a.x,b.x)&&Near(a.y,b.y);}
        static bool OnBorder(Vector2 p,Rect r)
        {return p.x>=r.xMin-Epsilon&&p.x<=r.xMax+Epsilon&&p.y>=r.yMin-Epsilon&&p.y<=r.yMax+Epsilon&&(Near(p.x,r.xMin)||Near(p.x,r.xMax)||Near(p.y,r.yMin)||Near(p.y,r.yMax));}
        static bool Vertical(Vector2 a,Vector2 b){return Near(a.x,b.x);}
        static bool Horizontal(Vector2 a,Vector2 b){return Near(a.y,b.y);}
        static bool Between(float value,float a,float b){return value>=Mathf.Min(a,b)-Epsilon&&value<=Mathf.Max(a,b)+Epsilon;}
        public static bool SegmentsIntersect(Vector2 a,Vector2 b,Vector2 c,Vector2 d)
        {
            bool av=Vertical(a,b),cv=Vertical(c,d);
            if(av&&cv)return Near(a.x,c.x)&&Mathf.Max(Mathf.Min(a.y,b.y),Mathf.Min(c.y,d.y))<=Mathf.Min(Mathf.Max(a.y,b.y),Mathf.Max(c.y,d.y))+Epsilon;
            if(!av&&!cv)return Near(a.y,c.y)&&Mathf.Max(Mathf.Min(a.x,b.x),Mathf.Min(c.x,d.x))<=Mathf.Min(Mathf.Max(a.x,b.x),Mathf.Max(c.x,d.x))+Epsilon;
            if(!av){var p=a;a=c;c=p;p=b;b=d;d=p;}
            return Between(a.x,c.x,d.x)&&Between(c.y,a.y,b.y);
        }
        public static bool CrossesInterior(Vector2 a,Vector2 b,Rect rect)
        {
            if(Vertical(a,b))return a.x>rect.xMin+Epsilon&&a.x<rect.xMax-Epsilon&&Mathf.Min(b.y,a.y)<rect.yMax-Epsilon&&Mathf.Max(b.y,a.y)>rect.yMin+Epsilon;
            return a.y>rect.yMin+Epsilon&&a.y<rect.yMax-Epsilon&&Mathf.Min(b.x,a.x)<rect.xMax-Epsilon&&Mathf.Max(b.x,a.x)>rect.xMin+Epsilon;
        }
        static Rect Pixels(DrawReceipt r,Rect logical){return new Rect(r.offset.x+logical.x*r.scale,r.offset.y+logical.y*r.scale,logical.width*r.scale,logical.height*r.scale);}
        static bool Fits(DrawReceipt r,Rect logical)
        {Rect p=Pixels(r,logical);return Valid(logical)&&p.xMin>=-Epsilon&&p.yMin>=-Epsilon&&p.xMax<=r.width+Epsilon&&p.yMax<=r.height+Epsilon;}
        static void Emit(DrawReceipt receipt,Action<string,bool,string> check,string id,bool pass,string detail)
        {if(!pass)receipt.failures.Add(id+": "+detail);check("ui.v57.f1.geometry."+id,pass,detail);}
        public static bool ValidateCurrentDraw(PanicHomeHud hud,Action<string,bool,string> check)
        {
            var receipt=new DrawReceipt{frame=Time.frameCount,width=Screen.width,height=Screen.height,scale=hud.CanvasScale,offset=hud.CanvasOffset};LastReceipt=receipt;
            try
            {
                receipt.geometryFrame=Read<int>(hud,"KeyboardHelpGeometryDrawFrameV57");receipt.repaints=Read<int>(hud,"KeyboardHelpDrawFramesV56");
                var catalog=Items(Read(hud,"KeyboardHelpCatalogV57"));var geometries=Items(Read(hud,"KeyboardHelpKeyGeometryV57"));
                receipt.catalogKeys=catalog.SelectMany(c=>Items(Read(c,"Keys")).Select(k=>Key(k.ToString()))).Distinct().OrderBy(k=>k,StringComparer.Ordinal).ToArray();
                bool catalogMetadata=catalog.Length>0&&catalog.Select(c=>Read<string>(c,"Id")).Distinct().Count()==catalog.Length&&catalog.All(c=>!string.IsNullOrWhiteSpace(Read<string>(c,"Action"))&&!string.IsNullOrWhiteSpace(Read<string>(c,"Context")));
                foreach(var geometry in geometries)
                {
                    bool measured=Has(geometry,"TextRect")&&Has(geometry,"TextMeasuredSize")&&Has(geometry,"TextFontSize");receipt.textTelemetry=receipt.relations.Count==0?measured:receipt.textTelemetry&&measured;
                    receipt.relations.Add(new Relation{connectionId=Read<string>(geometry,"ConnectionId"),keyId=Key(Read<string>(geometry,"KeyId")),key=Read<Rect>(geometry,"KeyRect"),label=Read<Rect>(geometry,"LabelRect"),
                        text=measured?Read<Rect>(geometry,"TextRect"):new Rect(),measuredText=measured?Read<Vector2>(geometry,"TextMeasuredSize"):Vector2.zero,fontSize=measured?Read<int>(geometry,"TextFontSize"):0});
                }
                foreach(var segment in Items(Read(hud,"KeyboardHelpConnectorSegmentsV57")))receipt.segments.Add(new Segment{connectionId=Read<string>(segment,"ConnectionId"),keyId=Key(Read<string>(segment,"KeyId")),start=Read<Vector2>(segment,"Start"),end=Read<Vector2>(segment,"End")});
                Emit(receipt,check,"actual-repaint",receipt.repaints>0&&receipt.geometryFrame>=receipt.frame-1&&receipt.geometryFrame<=receipt.frame&&Read<float>(hud,"KeyboardHelpAnimationProgressV56")==1&&Read<bool>(hud,"KeyboardHelpVisibleV56"),"Current actual Repaint geometry frame="+receipt.geometryFrame+" observation="+receipt.frame+"; dimensions="+receipt.width+"x"+receipt.height);
                Emit(receipt,check,"all-production-keys",catalogMetadata&&RequiredKeys.All(k=>receipt.catalogKeys.Contains(k))&&receipt.catalogKeys.All(RequiredKeys.Contains),"Expected34 used physical keys; missing="+string.Join(",",RequiredKeys.Where(k=>!receipt.catalogKeys.Contains(k)))+"; actual="+string.Join(",",receipt.catalogKeys)+". F stays listed with its bag-stage context; old F-absence oracle is obsolete; disabled G and unbound keys are excluded.");
                ValidateGeometry(receipt,check,catalog);
                receipt.passed=receipt.failures.Count==0;
            }
            catch(Exception error){receipt.error=error.ToString();Emit(receipt,check,"telemetry-readable",false,receipt.error);receipt.passed=false;}
            return receipt.passed;
        }
        static void ValidateGeometry(DrawReceipt r,Action<string,bool,string> check,object[] catalog)
        {
            var expected=catalog.ToDictionary(c=>Read<string>(c,"Id"),c=>Items(Read(c,"Keys")).Select(k=>Key(k.ToString())).ToArray());
            bool attribution=r.relations.Count>0&&r.segments.Count>0&&r.relations.All(g=>expected.ContainsKey(g.connectionId)&&expected[g.connectionId].Contains(g.keyId));
            var drawn=r.relations.Select(g=>g.keyId).Distinct().ToArray();attribution&=RequiredKeys.All(drawn.Contains);
            foreach(var pair in expected)foreach(string key in pair.Value)attribution&=r.relations.Count(g=>g.connectionId==pair.Key&&g.keyId==key)==1;
            Emit(r,check,"catalog-to-cap-attribution",attribution,"Every catalog key has exactly one actual cap/relation and its own catalog connection id; physical drawn="+drawn.Length+" relations="+r.relations.Count);
            var invalid=new List<string>();
            foreach(var s in r.segments)if(!Finite(s.start)||!Finite(s.end)||Near(s.start,s.end)||(!Vertical(s.start,s.end)&&!Horizontal(s.start,s.end)))invalid.Add(s.connectionId+"/"+s.keyId);
            Emit(r,check,"axis-aligned-nondegenerate",invalid.Count==0,"Actual orthogonal segments="+r.segments.Count+"; invalid="+string.Join(",",invalid));
            var intersections=new List<string>();for(int i=0;i<r.segments.Count;i++)for(int j=i+1;j<r.segments.Count;j++)
            {var a=r.segments[i];var b=r.segments[j];if(a.connectionId!=b.connectionId&&SegmentsIntersect(a.start,a.end,b.start,b.end))intersections.Add(a.connectionId+"/"+a.keyId+" vs "+b.connectionId+"/"+b.keyId);}
            Emit(r,check,"connections-do-not-intersect",intersections.Count==0,"Inclusive endpoint touches and collinear overlap count as intersections between different connection ids; violations="+string.Join(";",intersections.Take(12)));
            var badEndpoints=new List<string>();foreach(var g in r.relations)
            {
                var chain=r.segments.Where(s=>s.connectionId==g.connectionId&&s.keyId==g.keyId).ToArray();bool good=chain.Length>0;
                if(good){good=(OnBorder(chain[0].start,g.key)&&OnBorder(chain[chain.Length-1].end,g.label))||(OnBorder(chain[0].start,g.label)&&OnBorder(chain[chain.Length-1].end,g.key));for(int i=1;i<chain.Length;i++)good&=Near(chain[i-1].end,chain[i].start);}
                if(!good)badEndpoints.Add(g.connectionId+"/"+g.keyId);
            }
            bool allSegmentsOwned=r.segments.All(s=>r.relations.Count(g=>g.connectionId==s.connectionId&&g.keyId==s.keyId)==1);
            Emit(r,check,"own-cap-border-endpoints",allSegmentsOwned&&badEndpoints.Count==0,"Each ordered connection chain starts/ends at its own exact key/label borders and connects continuously; invalid="+string.Join(",",badEndpoints));
            var interior=new List<string>();foreach(var s in r.segments)foreach(var g in r.relations)
                if(CrossesInterior(s.start,s.end,g.key)||CrossesInterior(s.start,s.end,g.label))interior.Add(s.connectionId+"/"+s.keyId+" crosses "+g.connectionId+"/"+g.keyId);
            Emit(r,check,"lines-avoid-cap-card-interiors",interior.Count==0,"Independent open-rectangle/axis-segment intersection; even own cap/card interior forbidden beyond border endpoint; violations="+string.Join(";",interior.Take(12)));
            var overlaps=new List<string>();for(int i=0;i<r.relations.Count;i++)for(int j=i+1;j<r.relations.Count;j++)
            {var a=r.relations[i];var b=r.relations[j];if(RectInteriorsOverlap(a.key,b.key)||(a.connectionId!=b.connectionId&&(RectInteriorsOverlap(a.label,b.label)||RectInteriorsOverlap(a.key,b.label)||RectInteriorsOverlap(a.label,b.key))))overlaps.Add(a.connectionId+"/"+a.keyId+" vs "+b.connectionId+"/"+b.keyId);}
            Emit(r,check,"caps-and-distinct-cards-do-not-overlap",overlaps.Count==0,"Different annotations/caps have disjoint interiors; shared group annotation allowed only within the same catalog connection; violations="+string.Join(";",overlaps.Take(12)));
            bool bounds=r.scale>0&&r.relations.All(g=>Fits(r,g.key)&&Fits(r,g.label)&&Fits(r,g.text));
            Emit(r,check,"all-key-card-text-bounds-fit",bounds,"Every actual cap, annotation and text rectangle fits current framebuffer after real canvas transform="+r.scale+"; required at1600x900and960x540.");
            bool text=r.textTelemetry&&r.relations.All(g=>Finite(g.measuredText)&&g.measuredText.x>0&&g.measuredText.y>0&&g.measuredText.x<=g.text.width+Epsilon&&g.measuredText.y<=g.text.height+Epsilon&&g.fontSize*r.scale>=10f&&g.text.xMin>=g.label.xMin-Epsilon&&g.text.xMax<=g.label.xMax+Epsilon&&g.text.yMin>=g.label.yMin-Epsilon&&g.text.yMax<=g.label.yMax+Epsilon);
            Emit(r,check,"actual-text-measurement-fits",text,"Actual used style text measurements fit declared text rect, pixel font floor10; numerical fit is not human legibility acceptance.");
        }
        static bool RectInteriorsOverlap(Rect a,Rect b)
        {return Mathf.Min(a.xMax,b.xMax)-Mathf.Max(a.xMin,b.xMin)>Epsilon&&Mathf.Min(a.yMax,b.yMax)-Mathf.Max(a.yMin,b.yMin)>Epsilon;}
    }
}
