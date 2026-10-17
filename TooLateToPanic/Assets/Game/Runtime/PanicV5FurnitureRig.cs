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
        public float OpenAmount {get;private set;}
        public bool TargetOpen {get;private set;}
        Quaternion[] rest;
        public void SetOpen(bool open){TargetOpen=open;}
        public void Advance(float dt)
        {
            if(float.IsNaN(dt)||float.IsInfinity(dt)||dt<=0)return;
            SetOpenAmount(Mathf.MoveTowards(OpenAmount,TargetOpen?1:0,Mathf.Min(.1f,dt)/.24f));
        }
        public void SetOpenAmount(float amount)
        {
            if(float.IsNaN(amount)||float.IsInfinity(amount))amount=0;
            OpenAmount=Mathf.Clamp01(amount);
            if(rest==null||rest.Length!=Doors.Length)
            {rest=new Quaternion[Doors.Length];for(int i=0;i<Doors.Length;i++)rest[i]=Doors[i]?Doors[i].localRotation:Quaternion.identity;}
            float smooth=OpenAmount*OpenAmount*(3-2*OpenAmount);
            for(int i=0;i<Doors.Length;i++)if(Doors[i])Doors[i].localRotation=rest[i]*Quaternion.Euler(0,(i<OpenAngles.Length?OpenAngles[i]:95)*smooth,0);
        }
    }
}
