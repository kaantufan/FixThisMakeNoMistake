using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        LineRenderer interactionOutline;
        readonly Vector3[] interactionCorners=new Vector3[4];
        void UpdateInteractionOutline()
        {
            bool visible=FocusPosition(out var p)&&!CinematicActive&&!LossAnnounced&&!InventoryOpen&&!WorkActive&&
                (Model.Phase==PanicPhase.Preparing||Model.Phase==PanicPhase.Tutorial||Model.Phase==PanicPhase.Inspection);
            if(!visible){if(interactionOutline)interactionOutline.gameObject.SetActive(false);return;}
            if(!interactionOutline)interactionOutline=MakeLine("Current interaction",new[]{Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero},new Color(.4f,.9f,.76f),.025f);
            interactionOutline.gameObject.SetActive(true);
            Vector3 size=Selected!=null?Selected.Size:new Vector3(.8f,0,.65f);size.x=Mathf.Clamp(size.x,.45f,2.4f);size.z=Mathf.Clamp(size.z,.35f,1.8f);
            float yaw=Selected!=null?Selected.Rotation:0;var turn=Quaternion.Euler(0,yaw,0);
            interactionCorners[0]=p+turn*new Vector3(-size.x*.5f,.045f,-size.z*.5f);
            interactionCorners[1]=p+turn*new Vector3(size.x*.5f,.045f,-size.z*.5f);
            interactionCorners[2]=p+turn*new Vector3(size.x*.5f,.045f,size.z*.5f);
            interactionCorners[3]=p+turn*new Vector3(-size.x*.5f,.045f,size.z*.5f);
            interactionOutline.SetPositions(interactionCorners);var color=CanInteract?new Color(.4f,.9f,.76f):new Color(1,.66f,.25f);
            interactionOutline.startColor=color;interactionOutline.endColor=color;
        }
    }
}
