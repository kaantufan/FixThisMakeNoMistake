using UnityEngine;
namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        Rect manualWorkViewportV414;
        bool manualWorkDraggingV414, manualWorkAngleValidV414;
        int manualWorkTargetV414=-1;
        float manualWorkAngleV414;
        Vector2 manualWorkLastPointerV414;
        int manualHammerFrameV414=-1;
        public bool ManualWorkDraggingV414 { get { return manualWorkDraggingV414; } }
        public Vector2 ManualWorkPointerV414 { get { return manualWorkLastPointerV414; } }
        public void SetManualWorkViewportV414(Rect value) { manualWorkViewportV414=value; }
        public void ResetManualWorkPointerV414() { manualWorkDraggingV414=manualWorkAngleValidV414=false;manualWorkTargetV414=-1; }
        bool ManualLogicalPointV414(Vector3 p,out Vector2 point)
        {
            point=Vector2.zero;if(manualWorkViewportV414.width<=0||p.z<=0||p.x<0||p.x>1||p.y<0||p.y>1)return false;
            point=new Vector2(manualWorkViewportV414.x+p.x*manualWorkViewportV414.width,manualWorkViewportV414.y+(1-p.y)*manualWorkViewportV414.height);return true;
        }
        // Actual screw head or visible grip. The valve pivot is a separate physical point.
        public bool ManualWorkCenterV414(int index,out Vector2 center)
        { center=Vector2.zero;return WorkActive&&!Expedition.Work.IsWorldConstruction&&ManualLogicalPointV414(WorkTargetViewport(index),out center); }
        public bool ManualWorkPivotV414(out Vector2 center)
        {
            center=Vector2.zero;
            if(WorkActive&&!Expedition.Work.IsUtility&&Expedition.Work.Action!="hose")return ManualWorkCenterV414(Expedition.Work.NextClickTarget,out center);
            return workCamera&&workRig&&PanicWorkArt.GetControlPivotPosition(workRig,out Vector3 point)&&ManualLogicalPointV414(workCamera.WorldToViewportPoint(point),out center);
        }
        public bool ManualWorkClosedGripV414(out Vector2 center)
        { center=Vector2.zero;return workCamera&&workRig&&PanicWorkArt.GetControlClosedPosition(workRig,out Vector3 point)&&ManualLogicalPointV414(workCamera.WorldToViewportPoint(point),out center); }
        public Rect ManualWorkTargetBoundsV414(int index)
        { return ManualWorkCenterV414(index,out Vector2 p)?new Rect(p.x-44,p.y-44,88,88):new Rect(); }
        float PowerDragPixelsV414()
        {
            if(!workCamera||!PanicWorkArt.GetControlPivotPosition(workRig,out Vector3 p))return 1;
            return Mathf.Max(1,Mathf.Abs(workCamera.WorldToViewportPoint(p).y-workCamera.WorldToViewportPoint(p+Vector3.down*.26f).y)*manualWorkViewportV414.height);
        }
        // Same production adapter is available for root API QA; commits remain in StepWork/Expedition.
        public bool ProcessManualWorkPointerV414(Vector2 pointer,bool pressed,bool held)
        {
            if(!WorkActive||!Expedition.Work.UsesDirectClick||InputLocked||CinematicActive||LossAnnounced||Model.IsPaused||NotebookBlocksInput||
                CancelFrame==Time.frameCount||DiscreteInputFrame==Time.frameCount||hud&&(hud.DeveloperMenuOpen||hud.DeveloperInputBlockedThisFrame))
            {ResetManualWorkPointerV414();return false;}
            var work=Expedition.Work;int index=work.NextClickTarget;bool hasGrip=ManualWorkCenterV414(index,out Vector2 grip);
            if(pressed)
            {
                ResetManualWorkPointerV414();if(hasGrip&&ManualWorkTargetBoundsV414(index).Contains(pointer)&&ClickWorkTarget(index))
                {manualWorkDraggingV414=true;manualWorkTargetV414=index;}
                manualWorkLastPointerV414=pointer;
                if(ManualWorkPivotV414(out Vector2 pivot)&&(pointer-pivot).sqrMagnitude>.000001f)
                {manualWorkAngleV414=Mathf.Atan2(pointer.y-pivot.y,pointer.x-pivot.x)*Mathf.Rad2Deg;manualWorkAngleValidV414=true;}
                else manualWorkAngleValidV414=work.Action=="poweroff";
                return false; // Selection never earns progress.
            }
            if(!held||index!=manualWorkTargetV414||!hasGrip){ResetManualWorkPointerV414();return false;}
            if(!manualWorkDraggingV414)return false;
            if(!manualWorkViewportV414.Contains(pointer)){manualWorkAngleValidV414=false;manualWorkLastPointerV414=pointer;return false;}
            float degrees=0;bool valid=true;
            if(work.Action=="poweroff")degrees=(pointer.y-manualWorkLastPointerV414.y)/PowerDragPixelsV414();
            else if(ManualWorkPivotV414(out Vector2 pivot))
            {
                Vector2 delta=pointer-pivot;if(delta.sqrMagnitude>.000001f)
                {float angle=Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg;if(manualWorkAngleValidV414)degrees=Mathf.DeltaAngle(manualWorkAngleV414,angle);manualWorkAngleV414=angle;}
                else valid=false; // Numerical singularity only; no hidden annulus or speed test.
            }
            else valid=false;
            bool changed=manualWorkAngleValidV414&&valid&&(work.UsesLinearDrag?work.ApplyLinearDisplacement(degrees,true):work.ApplyRotation(degrees,true));
            manualWorkAngleValidV414=valid;manualWorkLastPointerV414=pointer;return changed;
        }
        public bool StrikeManualWorkV414()
        {
            if(!WorkActive||!Expedition.Work.UsesHammer||manualHammerFrameV414==Time.frameCount||InputLocked||CinematicActive||LossAnnounced||Model.IsPaused||NotebookBlocksInput||
                CancelFrame==Time.frameCount||DiscreteInputFrame==Time.frameCount||hud&&(hud.DeveloperMenuOpen||hud.DeveloperInputBlockedThisFrame))return false;
            manualHammerFrameV414=Time.frameCount;StepWork(Mathf.Max(.001f,Mathf.Min(Time.deltaTime,.08f)),0,false,false,true);return true;
        }
        void UpdateManualWorkV414(float dt)
        {
            if(!WorkActive)return;
            if(Input.GetKeyDown(KeyCode.Backspace)||Input.GetKeyDown(KeyCode.Escape))
            {ResetManualWorkPointerV414();ProcessWorkInput(dt,0,false,false,false,true);return;}
            bool blocked=InputLocked||CinematicActive||LossAnnounced||Model.IsPaused||NotebookBlocksInput||CancelFrame==Time.frameCount||DiscreteInputFrame==Time.frameCount||
                hud&&(hud.DeveloperMenuOpen||hud.DeveloperInputBlockedThisFrame);
            if(blocked){ResetManualWorkPointerV414();return;}
            var work=Expedition.Work;
            if(work.UsesDirectClick&&hud)
            {
                Vector2 pointer=(new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y)-hud.CanvasOffset)/hud.CanvasScale;
                ProcessManualWorkPointerV414(pointer,Input.GetMouseButtonDown(0),Input.GetMouseButton(0));
                KeyCode alternative=work.Action=="poweroff"?KeyCode.DownArrow:KeyCode.RightArrow;
                if(!manualWorkDraggingV414&&Input.GetKey(alternative)&&ClickWorkTarget(work.NextClickTarget)){if(work.UsesLinearDrag)work.ApplyLinearDisplacement(2f*dt,true);else work.ApplyRotation(work.ExpectedDirection*180f*dt,true);}
            }
            // Four separate conscious presses; no rhythm window or held-Space repeat.
            if(work.UsesHammer&&Input.GetKeyDown(KeyCode.Space))StrikeManualWorkV414();
            else StepWork(dt,0,false,false,false);
        }
    }
}
