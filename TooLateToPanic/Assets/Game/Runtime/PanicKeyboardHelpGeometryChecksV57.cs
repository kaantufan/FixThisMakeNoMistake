using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
            public bool hasConnector;public string connectorKeyId,captionText;
        }
        [Serializable] public sealed class Group
        {public string connectionId,representativeKey,displayKeys,action,context;public string[] keys;}
        [Serializable] public sealed class Segment
        {public string connectionId,keyId;public Vector2 start,end;}
        [Serializable] public sealed class Cap
        {public string keyId;public Rect key;public bool highlighted;}
        [Serializable] public sealed class UiObstacle
        {public string id;public Rect bounds;}
        [Serializable] public sealed class DrawReceipt
        {
            public string layer="NATIVE_ACTUAL_REPAINT_CLASSIC_GROUP_REPRESENTATIVES_NOT_HUMAN_ACCEPTANCE",error;
            public string contract="CLASSIC_R5_GROUP_REPRESENTATIVE_V1";
            public int frame,geometryFrame,repaints,width,height;public float scale;public Vector2 offset;
            public float minimumDifferentGroupDistance;
            public bool passed,textTelemetry;public string[] catalogKeys;
            public Rect classicBounds;
            public List<Group> groups=new List<Group>();
            public List<Cap> allCaps=new List<Cap>();
            public List<UiObstacle> uiObstacles=new List<UiObstacle>();
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
                foreach(var group in catalog)receipt.groups.Add(new Group{connectionId=Read<string>(group,"Id"),representativeKey=Key(Read<string>(group,"RepresentativeKey")),displayKeys=Read<string>(group,"DisplayKeys"),action=Read<string>(group,"Action"),context=Read<string>(group,"Context"),keys=Items(Read(group,"Keys")).Select(k=>Key(k.ToString())).ToArray()});
                receipt.catalogKeys=catalog.SelectMany(c=>Items(Read(c,"Keys")).Select(k=>Key(k.ToString()))).Distinct().OrderBy(k=>k,StringComparer.Ordinal).ToArray();
                bool catalogMetadata=catalog.Length>0&&catalog.Select(c=>Read<string>(c,"Id")).Distinct().Count()==catalog.Length&&catalog.All(c=>!string.IsNullOrWhiteSpace(Read<string>(c,"Action"))&&!string.IsNullOrWhiteSpace(Read<string>(c,"Context")));
                foreach(var geometry in geometries)
                {
                    bool measured=Has(geometry,"TextRect")&&Has(geometry,"TextMeasuredSize")&&Has(geometry,"TextFontSize");receipt.textTelemetry=receipt.relations.Count==0?measured:receipt.textTelemetry&&measured;
                    receipt.relations.Add(new Relation{connectionId=Read<string>(geometry,"ConnectionId"),keyId=Key(Read<string>(geometry,"KeyId")),key=Read<Rect>(geometry,"KeyRect"),label=Read<Rect>(geometry,"LabelRect"),
                        text=measured?Read<Rect>(geometry,"TextRect"):new Rect(),measuredText=measured?Read<Vector2>(geometry,"TextMeasuredSize"):Vector2.zero,fontSize=measured?Read<int>(geometry,"TextFontSize"):0,
                        hasConnector=Read<bool>(geometry,"HasConnector"),connectorKeyId=Key(Read<string>(geometry,"ConnectorKeyId")),captionText=Read<string>(geometry,"RenderedCaptionText")});
                }
                foreach(var segment in Items(Read(hud,"KeyboardHelpConnectorSegmentsV57")))receipt.segments.Add(new Segment{connectionId=Read<string>(segment,"ConnectionId"),keyId=Key(Read<string>(segment,"KeyId")),start=Read<Vector2>(segment,"Start"),end=Read<Vector2>(segment,"End")});
                receipt.classicBounds=Read<Rect>(hud,"KeyboardHelpBoundsV56");
                foreach(var cap in Items(Read(hud,"KeyboardHelpAllCapsV57")))receipt.allCaps.Add(new Cap{keyId=Key(Read<string>(cap,"KeyId")),key=Read<Rect>(cap,"KeyRect"),highlighted=Read<bool>(cap,"Highlighted")});
                foreach(string name in new[]{"Title","Subtitle","Close"})receipt.uiObstacles.Add(new UiObstacle{id=name,bounds=Read<Rect>(hud,"KeyboardHelp"+name+"BoundsV57")});
                Emit(receipt,check,"actual-repaint",receipt.repaints>0&&receipt.geometryFrame>=receipt.frame-1&&receipt.geometryFrame<=receipt.frame&&Read<float>(hud,"KeyboardHelpAnimationProgressV56")==1&&Read<bool>(hud,"KeyboardHelpVisibleV56"),"Current actual Repaint geometry frame="+receipt.geometryFrame+" observation="+receipt.frame+"; dimensions="+receipt.width+"x"+receipt.height);
                Emit(receipt,check,"all-production-keys",catalogMetadata&&RequiredKeys.All(k=>receipt.catalogKeys.Contains(k))&&receipt.catalogKeys.All(RequiredKeys.Contains),"Expected34 used physical keys; missing="+string.Join(",",RequiredKeys.Where(k=>!receipt.catalogKeys.Contains(k)))+"; actual="+string.Join(",",receipt.catalogKeys)+". F stays listed with its bag-stage context; old F-absence oracle is obsolete; disabled G and unbound keys are excluded.");
                ValidateGeometry(receipt,check,catalog);
                ValidateClassicCaps(receipt,check);
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
            bool representatives=r.groups.All(group=>group.keys.Length>0&&group.keys.Distinct().Count()==group.keys.Length&&group.keys.Contains(group.representativeKey)&&r.relations.Count(g=>g.connectionId==group.connectionId&&g.hasConnector&&g.keyId==group.representativeKey)==1&&r.relations.Count(g=>g.connectionId==group.connectionId&&g.hasConnector)==1&&r.relations.Where(g=>g.connectionId==group.connectionId).All(g=>g.connectorKeyId==group.representativeKey));
            Emit(r,check,"one-declared-representative-per-group",representatives,"Classic R5 deliberate contract:34 actual caps remain; each explanatory group declares exactly one member cap as its sole connector origin. Nonmembers and phantom branches forbidden.");
            bool captionMembers=r.groups.All(group=>group.keys.All(k=>CaptionKeys(group.displayKeys).Contains(k))&&r.relations.Where(g=>g.connectionId==group.connectionId).All(g=>!string.IsNullOrWhiteSpace(g.captionText)&&group.keys.All(k=>CaptionKeys(g.captionText).Contains(k))));
            Emit(r,check,"rendered-captions-name-every-group-member",captionMembers,"All catalog member key names must occur in both declared DisplayKeys and actual rendered caption text; aliases normalize to physical cap names. Distinct key action wording remains subject to source/pixel review.");
            var invalid=new List<string>();
            foreach(var s in r.segments)if(!Finite(s.start)||!Finite(s.end)||Near(s.start,s.end)||(!Vertical(s.start,s.end)&&!Horizontal(s.start,s.end)))invalid.Add(s.connectionId+"/"+s.keyId);
            Emit(r,check,"axis-aligned-nondegenerate",invalid.Count==0,"Actual orthogonal segments="+r.segments.Count+"; invalid="+string.Join(",",invalid));
            var intersections=new List<string>();for(int i=0;i<r.segments.Count;i++)for(int j=i+1;j<r.segments.Count;j++)
            {var a=r.segments[i];var b=r.segments[j];if(a.connectionId!=b.connectionId&&SegmentsIntersect(a.start,a.end,b.start,b.end))intersections.Add(a.connectionId+"/"+a.keyId+" vs "+b.connectionId+"/"+b.keyId);}
            Emit(r,check,"connections-do-not-intersect",intersections.Count==0,"Inclusive endpoint touches and collinear overlap count as intersections between different connection ids; violations="+string.Join(";",intersections.Take(12)));
            float clearance=float.PositiveInfinity;for(int i=0;i<r.segments.Count;i++)for(int j=i+1;j<r.segments.Count;j++)
            {var a=r.segments[i];var b=r.segments[j];if(a.connectionId!=b.connectionId)clearance=Mathf.Min(clearance,SegmentDistance(a.start,a.end,b.start,b.end));}
            r.minimumDifferentGroupDistance=clearance;
            Emit(r,check,"connections-have-classic-clearance",Finite(clearance)&&clearance>=3f-Epsilon,"Actual different-group centerlines remain at least3 logical units apart; min="+clearance+"; proposed1.8 logical halo gap="+(clearance-1.8f)+". Pixel visibility still needs actual PNG review.");
            var badEndpoints=new List<string>();foreach(var g in r.relations)
            {
                var chain=r.segments.Where(s=>s.connectionId==g.connectionId&&s.keyId==g.keyId).ToArray();bool good=chain.Length>0;
                if(!g.hasConnector){if(chain.Length>0)badEndpoints.Add(g.connectionId+"/"+g.keyId+" unexpected nonrepresentative branch");continue;}
                if(good){good=(OnBorder(chain[0].start,g.key)&&OnBorder(chain[chain.Length-1].end,g.label))||(OnBorder(chain[0].start,g.label)&&OnBorder(chain[chain.Length-1].end,g.key));for(int i=1;i<chain.Length;i++)good&=Near(chain[i-1].end,chain[i].start);}
                if(!good)badEndpoints.Add(g.connectionId+"/"+g.keyId);
            }
            bool allSegmentsOwned=r.segments.All(s=>r.relations.Count(g=>g.hasConnector&&g.connectionId==s.connectionId&&g.keyId==s.keyId)==1);
            Emit(r,check,"own-cap-border-endpoints",allSegmentsOwned&&badEndpoints.Count==0,"Each declared representative chain starts/ends at its own exact key/label borders and connects continuously; nonrepresentatives have no fake chain; invalid="+string.Join(",",badEndpoints));
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
        static float PointSegmentDistance(Vector2 p,Vector2 a,Vector2 b)
        {Vector2 d=b-a;float t=d.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;return (p-(a+d*t)).magnitude;}
        static float SegmentDistance(Vector2 a,Vector2 b,Vector2 c,Vector2 d)
        {return SegmentsIntersect(a,b,c,d)?0:Mathf.Min(Mathf.Min(PointSegmentDistance(a,c,d),PointSegmentDistance(b,c,d)),Mathf.Min(PointSegmentDistance(c,a,b),PointSegmentDistance(d,a,b)));}
        static string[] CaptionKeys(string text)
        {
            text=Regex.Replace(text??"",@"\bPage\s+Up\b","PageUp",RegexOptions.IgnoreCase);
            text=Regex.Replace(text,@"\bPage\s+Down\b","PageDown",RegexOptions.IgnoreCase);
            text=Regex.Replace(text,@"\bWASD\b","W A S D",RegexOptions.IgnoreCase);
            return Regex.Matches(text,@"[A-Za-z0-9]+|[↑←↓→]").Cast<Match>().Select(m=>Key(m.Value)).Distinct().ToArray();
        }
        // Independent fixed oracle: the original DrawCompactKeyboardBodyV51 physical
        // positions. This includes its dim blank caps; production telemetry is not
        // allowed to silently make an obstacle disappear or rearrange the keyboard.
        static List<Cap> OriginalClassicCaps()
        {
            var caps=new List<Cap>();int blank=0;
            Action<float,float,float,float,string> add=(x,y,w,h,key)=>caps.Add(new Cap{keyId=string.IsNullOrEmpty(key)?"BLANK"+(blank++):Key(key),key=new Rect(x,y,w,h)});
            Action<float,float,string[]> row=(y,x,keys)=>{for(int i=0;i<keys.Length;i++)add(x+i*32,y,28,26,keys[i]);};
            add(10,9,36,24,"Esc");add(60,9,28,24,"F1");add(94,9,28,24,"F2");add(128,9,28,24,"F3");add(330,9,42,24,"PageUp");add(380,9,42,24,"PageDown");
            row(40,10,new[]{"1","2","3","4","","","","","",""});add(330,40,92,26,"Back");
            add(10,70,42,26,"Tab");row(70,58,new[]{"Q","W","E","R","T","","","","",""});add(382,70,40,56,"Enter");
            add(10,100,42,26,"");row(100,58,new[]{"A","S","D","F","","H","J","",""});
            add(10,130,66,26,"Shift");row(130,82,new[]{"Z","X","","","B","N",""});add(314,130,40,26,"");
            add(10,168,42,28,"");add(58,168,42,28,"");add(106,168,176,28,"Space");add(288,168,30,28,"");
            add(362,134,28,26,"↑");add(330,168,28,28,"←");add(362,168,28,28,"↓");add(394,168,28,28,"→");
            return caps;
        }
        static bool SameRect(Rect a,Rect b)
        {return Near(a.x,b.x)&&Near(a.y,b.y)&&Near(a.width,b.width)&&Near(a.height,b.height);}
        static void ValidateClassicCaps(DrawReceipt r,Action<string,bool,string> check)
        {
            var original=OriginalClassicCaps();Rect required=new Rect(360,315,880,420);
            bool shape=SameRect(r.classicBounds,required)&&r.allCaps.Count==56&&r.allCaps.Select(c=>c.keyId).Distinct().Count()==56;
            foreach(var cap in original)
            {
                Rect local=cap.key;Rect expected=new Rect(360+local.x*2,315+local.y*2,local.width*2,local.height*2);
                shape&=r.allCaps.Count(c=>SameRect(c.key,expected)&&(RequiredKeys.Contains(cap.keyId)?c.keyId==cap.keyId:!RequiredKeys.Contains(c.keyId)))==1;
            }
            shape&=RequiredKeys.All(k=>r.allCaps.Count(c=>c.keyId==k)==1);
            shape&=r.relations.All(g=>r.allCaps.Count(c=>c.keyId==g.keyId&&SameRect(c.key,g.key))==1);
            Emit(r,check,"original-classic-keyboard-preserved",shape,"Exact original 440x210 physical layout uniformly doubled at(360,315):56 caps=34 used+22 blanks; actual="+r.allCaps.Count+"; same original gaps and silhouette required.");
            Emit(r,check,"all-34-used-caps-always-highlighted",r.allCaps.All(c=>c.highlighted==RequiredKeys.Contains(c.keyId)),"Actual captured DrawCompactKeyV51 family must highlight all34 cataloged used caps and leave all22 blank caps dim, independent of current gameplay context.");
            var violations=new List<string>();foreach(var s in r.segments)foreach(var cap in r.allCaps)
                if(CrossesInterior(s.start,s.end,cap.key))violations.Add(s.connectionId+"/"+s.keyId+" crosses "+cap.keyId);
            Emit(r,check,"lines-avoid-all-56-cap-interiors",violations.Count==0,"Blank caps are obstacles too; no own or other cap interior traversals: "+string.Join(";",violations.Take(12)));
            bool bounds=r.allCaps.All(c=>Fits(r,c.key))&&r.relations.All(g=>!RectInteriorsOverlap(g.label,r.classicBounds));
            Emit(r,check,"all-caps-fit-and-captions-outside-classic-body",bounds,"Every actual physical cap fits the framebuffer; all explanation cards remain outside the original keyboard body.");
            var uiErrors=new List<string>();foreach(var ui in r.uiObstacles)
            {
                if(!Fits(r,ui.bounds))uiErrors.Add(ui.id+" outside framebuffer");
                foreach(var cap in r.allCaps)if(RectInteriorsOverlap(ui.bounds,cap.key))uiErrors.Add(ui.id+" overlaps cap "+cap.keyId);
                foreach(var g in r.relations)if(RectInteriorsOverlap(ui.bounds,g.label))uiErrors.Add(ui.id+" overlaps caption "+g.connectionId);
                foreach(var s in r.segments)if(CrossesInterior(s.start,s.end,ui.bounds))uiErrors.Add(s.connectionId+" crosses "+ui.id);
            }
            for(int i=0;i<r.uiObstacles.Count;i++)for(int j=i+1;j<r.uiObstacles.Count;j++)if(RectInteriorsOverlap(r.uiObstacles[i].bounds,r.uiObstacles[j].bounds))uiErrors.Add(r.uiObstacles[i].id+" overlaps "+r.uiObstacles[j].id);
            Emit(r,check,"header-close-caption-path-obstacles",uiErrors.Count==0,"Actual title/subtitle/close rectangles remain separate from all56 caps, all group cards and all connector paths; "+string.Join(";",uiErrors.Take(12)));
        }
    }
}
