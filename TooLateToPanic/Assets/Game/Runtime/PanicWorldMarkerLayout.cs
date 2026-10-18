using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        readonly List<Rect> _worldMarkerReservations=new List<Rect>(8);
        readonly List<string> _worldMarkerLabels=new List<string>(8);
        int _worldMarkerReservationFrame=-1;EventType _worldMarkerReservationEvent;
        GUIStyle _anchorTargetStyle;
        public float AnchorTargetTextHeight { get; private set; }
        public int AnchorTargetFontSize { get; private set; }
        public IReadOnlyList<Rect> CurrentMarkerBounds => _worldMarkerReservationFrame==Time.frameCount?_worldMarkerReservations:new List<Rect>();
        public IReadOnlyList<string> CurrentMarkerLabels => _worldMarkerReservationFrame==Time.frameCount?_worldMarkerLabels:new List<string>();
        public Rect[] DamageLabelRectsV56
        {
            get
            {
                if (_worldMarkerReservationFrame != Time.frameCount) return System.Array.Empty<Rect>();
                var result = new List<Rect>(3);
                for (int i = 0; i < _worldMarkerLabels.Count; i++)
                    if (_worldMarkerLabels[i].Contains(" · +")) result.Add(_worldMarkerReservations[i]);
                return result.ToArray();
            }
        }
        public string CurrentAnchorTargetLabel()
        {
            string id=World.Expedition.Work.TargetId;var target=World.Model.Items.Find(item=>item.Id==id);
            return target!=null?target.Label:"Seçili eşya";
        }
        public string CurrentAnchorSurfaceLabel() => World.Expedition.Work.Action=="anchorwall"?"DUVAR ANKRAJI":"ZEMİN ANKRAJI";
        void DrawAnchorTargetName()
        {
            if(_anchorTargetStyle==null)_anchorTargetStyle=new GUIStyle(_body);
            _anchorTargetStyle.wordWrap=true;_anchorTargetStyle.alignment=TextAnchor.UpperLeft;_anchorTargetStyle.fontSize=24;
            string text=CurrentAnchorTargetLabel();
            while(_anchorTargetStyle.fontSize>18&&_anchorTargetStyle.CalcHeight(new GUIContent(text),384)>64)_anchorTargetStyle.fontSize--;
            AnchorTargetTextHeight=_anchorTargetStyle.CalcHeight(new GUIContent(text),384);AnchorTargetFontSize=_anchorTargetStyle.fontSize;
            Label(new Rect(1122,232,384,64),text,ColorText(_anchorTargetStyle,Sea));
        }
        Rect ReserveWorldMarkerBounds(Rect proposed,string label)
        {
            EventType kind=Event.current!=null?Event.current.type:EventType.Repaint;
            if(_worldMarkerReservationFrame!=Time.frameCount||_worldMarkerReservationEvent!=kind)
            {_worldMarkerReservationFrame=Time.frameCount;_worldMarkerReservationEvent=kind;_worldMarkerReservations.Clear();_worldMarkerLabels.Clear();}
            Rect origin=ClearWorldMarkerBounds(proposed),best=origin;float bestDistance=float.PositiveInfinity;
            for(int row=-12;row<=12;row++)for(int col=-5;col<=5;col++)
            {
                Rect candidate=origin;candidate.x+=col*(origin.width+12f);candidate.y+=row*(origin.height+12f);
                if(candidate.xMin<12||candidate.xMax>CanvasWidth-12||candidate.yMin<142||candidate.yMax>CanvasHeight-65||!WorldMarkerClear(candidate))continue;
                if(World.JuryActive&&candidate.Overlaps(JuryObjectiveBounds))continue;
                if(World.LootVisible&&candidate.yMax>600)continue;
                bool free=true;foreach(var prior in _worldMarkerReservations){Rect occupied=prior;occupied.xMin-=6;occupied.xMax+=6;occupied.yMin-=6;occupied.yMax+=6;if(candidate.Overlaps(occupied)){free=false;break;}}
                if(!free)continue;float distance=(candidate.center-proposed.center).sqrMagnitude;if(distance<bestDistance){bestDistance=distance;best=candidate;}
            }
            _worldMarkerReservations.Add(best);_worldMarkerLabels.Add(label);return best;
        }
        void WorldMarkerLeader(Vector2 origin,Rect rect)
        {
            if (!WorldMarkerLeaderAvoidsDialogueV59(origin, rect)) return;
            if(origin.x<12||origin.x>CanvasWidth-12||origin.y<142||origin.y>CanvasHeight-65)return;
            Vector2 end=new Vector2(Mathf.Clamp(origin.x,rect.xMin,rect.xMax),Mathf.Clamp(origin.y,rect.yMin,rect.yMax));
            Vector2 delta=end-origin;if(delta.magnitude<12)return;
            Matrix4x4 prior=GUI.matrix;Color color=GUI.color;
            GUI.color=new Color(Brass.r,Brass.g,Brass.b,.48f);GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,origin);
            GUI.DrawTexture(new Rect(origin.x,origin.y,delta.magnitude,1.2f),Texture2D.whiteTexture);GUI.matrix=prior;GUI.color=color;
        }
    }
}
