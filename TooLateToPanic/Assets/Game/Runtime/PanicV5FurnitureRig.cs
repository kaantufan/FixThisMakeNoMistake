using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Presentation for a real source. World supplies open state and paused delta; no fake stock or input.</summary>
    public sealed class PanicV5FurnitureRig : MonoBehaviour
    {
        public string ArtId;
        public Vector3 LocalSize;
        public Transform[] LootAnchors=new Transform[0];
        public Transform[] Doors=new Transform[0];
        public float[] OpenAngles=new float[0];
        public Transform[] Drawers=new Transform[0];
        public float[] DrawerDistances=new float[0];
        public Light InteriorLight;
        public Renderer InteriorLightRenderer;
        public Transform TurntablePivot;
        public bool MainsPowered=true;
        public float OpenAmount {get;private set;}
        public bool TargetOpen {get;private set;}
        Quaternion[] rest;
        Vector3[] drawerRest;
        MaterialPropertyBlock lightBlock;
        public void SetOpen(bool open){TargetOpen=open;}
        public void Advance(float dt)
        {
            if(float.IsNaN(dt)||float.IsInfinity(dt)||dt<=0)return;
            if(TurntablePivot&&MainsPowered&&OpenAmount<.05f)TurntablePivot.Rotate(0f,Mathf.Min(.1f,dt)*48f,0f,Space.Self);
            SetOpenAmount(Mathf.MoveTowards(OpenAmount,TargetOpen?1:0,Mathf.Min(.1f,dt)/.24f));
        }
        public void SetOpenAmount(float amount)
        {
            if(float.IsNaN(amount)||float.IsInfinity(amount))amount=0;
            OpenAmount=Mathf.Clamp01(amount);
            if(rest==null||rest.Length!=Doors.Length)
            {rest=new Quaternion[Doors.Length];for(int i=0;i<Doors.Length;i++)rest[i]=Doors[i]?Doors[i].localRotation:Quaternion.identity;}
            if(Drawers!=null&&(drawerRest==null||drawerRest.Length!=Drawers.Length))
            {drawerRest=new Vector3[Drawers.Length];for(int i=0;i<Drawers.Length;i++)drawerRest[i]=Drawers[i]?Drawers[i].localPosition:Vector3.zero;}
            float smooth=OpenAmount*OpenAmount*(3-2*OpenAmount);
            for(int i=0;i<Doors.Length;i++)if(Doors[i])Doors[i].localRotation=rest[i]*Quaternion.Euler(0,(i<OpenAngles.Length?OpenAngles[i]:95)*smooth,0);
            if(Drawers!=null)for(int i=0;i<Drawers.Length;i++)if(Drawers[i])Drawers[i].localPosition=drawerRest[i]+new Vector3(0,0,-(DrawerDistances!=null&&i<DrawerDistances.Length?DrawerDistances[i]:.16f)*smooth);
            bool lit=MainsPowered&&smooth>.02f;
            if(InteriorLight){InteriorLight.enabled=lit;InteriorLight.intensity=lit?.85f*smooth:0f;}
            if(InteriorLightRenderer&&InteriorLightRenderer.sharedMaterial)
            {
                if(lightBlock==null)lightBlock=new MaterialPropertyBlock();
                InteriorLightRenderer.GetPropertyBlock(lightBlock);
                lightBlock.SetColor("_EmissionColor",lit?new Color(1f,.86f,.58f)*(.45f+.75f*smooth):Color.black);
                InteriorLightRenderer.SetPropertyBlock(lightBlock);
                lightBlock.Clear();
            }
        }
    }
}
