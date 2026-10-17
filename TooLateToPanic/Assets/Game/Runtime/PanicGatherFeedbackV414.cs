using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    public readonly struct PanicGatherFeedbackRecordV414
    {
        public readonly string Id;public readonly int Amount;public readonly float Age;public readonly Vector3 Origin;
        public PanicGatherFeedbackRecordV414(string id,int amount,float age,Vector3 origin){Id=id;Amount=amount;Age=age;Origin=origin;}
    }
    public sealed partial class PanicHomeWorld
    {
        readonly List<PanicGatherFeedbackRecordV414> gatherFeedbackV414=new List<PanicGatherFeedbackRecordV414>(12);
        int gatherSequenceSeenV414;
        PanicExpedition gatherExpeditionSeenV414;
        PanicMaterialPreviewV414 gatherPreviewsV414;
        GUIStyle gatherTextV414;
        public int GatherFeedbackCountV414 { get { return gatherFeedbackV414.Count; } }
        public int GatherFeedbackLastSequenceV414 { get { return gatherSequenceSeenV414; } }
        public bool GatherFeedbackVisibleV414 { get { return GatherFeedbackCountV414>0&&!ExperienceFeedbackBlockedV414&&Model!=null&&(Model.Phase==PanicPhase.Preparing||Model.Phase==PanicPhase.Tutorial); } }
        public string[] GatherFeedbackItemIdsV414 { get { var ids=new string[gatherFeedbackV414.Count];for(int i=0;i<ids.Length;i++)ids[i]=gatherFeedbackV414[i].Id;return ids; } }
        public int[] GatherFeedbackAmountsV414 { get { var amounts=new int[gatherFeedbackV414.Count];for(int i=0;i<amounts.Length;i++)amounts[i]=gatherFeedbackV414[i].Amount;return amounts; } }
        public int GatherFeedbackCachedAssetCountV414 { get { return gatherPreviewsV414==null?0:gatherPreviewsV414.CachedCount; } }
        public int GatherFeedbackActualRenderCountV414 { get { return gatherPreviewsV414==null?0:gatherPreviewsV414.RenderCount; } }
        public Texture GatherFeedbackAssetTextureV414(string id) { return gatherPreviewsV414==null?null:gatherPreviewsV414.Cached(id); }
        public PanicGatherFeedbackRecordV414[] GatherFeedbackRecordsV414 { get { return gatherFeedbackV414.ToArray(); } }
        void InitGatherFeedbackV414()
        {
            if(gatherPreviewsV414==null)gatherPreviewsV414=new PanicMaterialPreviewV414();
            gatherExpeditionSeenV414=Expedition;gatherSequenceSeenV414=Expedition==null?0:Expedition.GatherSequence;
            gatherTextV414=new GUIStyle{fontSize=16,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft,clipping=TextClipping.Clip};gatherTextV414.normal.textColor=new Color(.94f,.98f,.91f);
        }
        public void TickGatherFeedbackV414(float dt)
        {
            if(Expedition==null)return;
            if(gatherExpeditionSeenV414!=Expedition||Expedition.GatherSequence<gatherSequenceSeenV414)
            {gatherFeedbackV414.Clear();gatherExpeditionSeenV414=Expedition;gatherSequenceSeenV414=Expedition.GatherSequence;return;}
            if(ExperienceFeedbackBlockedV414)return;
            dt=float.IsNaN(dt)||float.IsInfinity(dt)?0:Mathf.Clamp(dt,0,.1f);
            for(int i=gatherFeedbackV414.Count-1;i>=0;i--)
            {
                var p=gatherFeedbackV414[i];float age=p.Age+dt;
                if(age>=1.85f)gatherFeedbackV414.RemoveAt(i);else gatherFeedbackV414[i]=new PanicGatherFeedbackRecordV414(p.Id,p.Amount,age,p.Origin);
            }
            if(Model.Phase!=PanicPhase.Preparing&&Model.Phase!=PanicPhase.Tutorial){gatherFeedbackV414.Clear();gatherSequenceSeenV414=Expedition.GatherSequence;return;}
            if(Expedition.GatherSequence==gatherSequenceSeenV414)return;
            gatherSequenceSeenV414=Expedition.GatherSequence;
            foreach(var reward in Expedition.LastGatherRewards)
            {
                if(string.IsNullOrEmpty(reward.Id)||reward.Required<=0)continue;
                if(gatherFeedbackV414.Count>=12)gatherFeedbackV414.RemoveAt(0);
                gatherFeedbackV414.Add(new PanicGatherFeedbackRecordV414(reward.Id,reward.Required,0,PlayerPosition+Vector3.up*1.35f));
            }
        }
        /// <summary>Drawing does not poll rewards, advance ages, append input rectangles or mutate inventory.</summary>
        public void DrawGatherRewardsV414()
        {
            if(!GatherFeedbackVisibleV414||!View||Event.current==null||Event.current.type!=EventType.Repaint||gatherPreviewsV414==null)return;
            Matrix4x4 matrix=GUI.matrix;Color color=GUI.color;
            try
            {
                GUI.matrix=Matrix4x4.identity;
                float scale=Mathf.Clamp(Mathf.Min(Screen.width/1100f,Screen.height/700f),.72f,1.1f);
                float width=Mathf.Min(268*scale,Screen.width-20),height=44*scale,gap=5*scale;
                float compact=Mathf.Min(1,Mathf.Max(12,Screen.height-96f)/(gatherFeedbackV414.Count*(height+gap)));
                height*=compact;gap*=compact;float allHeight=gatherFeedbackV414.Count*(height+gap);
                for(int i=0;i<gatherFeedbackV414.Count;i++)
                {
                    var p=gatherFeedbackV414[i];Vector3 screen=View.WorldToScreenPoint(p.Origin+Vector3.up*(p.Age*.36f));
                    float anchorX=screen.z>.1f?screen.x+26*scale:Screen.width*.55f;
                    float anchorY=screen.z>.1f?Screen.height-screen.y:Screen.height*.55f;
                    float x=Mathf.Clamp(anchorX,10,Mathf.Max(10,Screen.width-width-10));
                    float top=Mathf.Clamp(anchorY-allHeight*.5f,76*scale,Mathf.Max(76*scale,Screen.height-allHeight-16));
                    Rect card=new Rect(x,top+i*(height+gap),width,height);
                    float alpha=Mathf.Clamp01(p.Age/.10f)*Mathf.Clamp01((1.85f-p.Age)/.35f);
                    GUI.color=new Color(.045f,.095f,.08f,.88f*alpha);GUI.DrawTexture(card,Texture2D.whiteTexture);
                    Texture preview=gatherPreviewsV414.Get(p.Id);float icon=height-6*scale;
                    if(preview){GUI.color=new Color(1,1,1,alpha);GUI.DrawTexture(new Rect(x+3*scale,card.y+3*scale,icon,icon),preview,ScaleMode.ScaleToFit,true);}
                    GUI.color=new Color(1,1,1,alpha);gatherTextV414.fontSize=Mathf.Max(8,Mathf.RoundToInt(16*scale*compact));
                    GUI.Label(new Rect(x+height+4*scale,card.y,width-height-8*scale,height),"+"+p.Amount+" "+PanicExpedition.DisplayName(p.Id),gatherTextV414);
                }
            }
            finally{GUI.matrix=matrix;GUI.color=color;}
        }
        void DisposeGatherFeedbackV414()
        {if(gatherPreviewsV414!=null)gatherPreviewsV414.Dispose();gatherPreviewsV414=null;gatherFeedbackV414.Clear();}
    }
    /// <summary>Cached actual material pickup renders, using the same mesh factory as the world. Never uses guessed item art.</summary>
    public sealed class PanicMaterialPreviewV414 : IDisposable
    {
        readonly Dictionary<string,RenderTexture> cache=new Dictionary<string,RenderTexture>(StringComparer.Ordinal);
        readonly HashSet<string> failed=new HashSet<string>(StringComparer.Ordinal);
        GameObject studio;Camera camera;Light key,fill;bool disposed;
        public int CachedCount { get { return cache.Count; } }
        public int RenderCount { get;private set; }
        public Texture Cached(string id){return id!=null&&cache.TryGetValue(id,out var texture)?texture:null;}
        static bool Supported(string id)
        {switch(id){case "screws":case "bracket":case "plank":case "sandbag":case "seedling":case "hose":case "water":case "food":case "bandage":case "antiseptic":case "firstaid":case "flashlight":case "battery":case "radio":case "blanket":case "extinguisher":return true;default:return false;}}
        public Texture Get(string id)
        {
            if(disposed||string.IsNullOrEmpty(id)||!Supported(id)||failed.Contains(id))return null;
            if(cache.TryGetValue(id,out var found))return found;
            if(cache.Count>=16||Event.current!=null&&Event.current.type!=EventType.Repaint||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return null;
            try{EnsureStudio();var texture=Render(id);cache.Add(id,texture);return texture;}
            catch(Exception ex){failed.Add(id);Debug.LogWarning("V414_GATHER_ACTUAL_PREVIEW_FAILED "+id+": "+ex.Message);return null;}
        }
        void EnsureStudio()
        {
            if(studio)return;studio=new GameObject("V414 actual material preview studio"){hideFlags=HideFlags.HideAndDontSave};studio.transform.position=new Vector3(3400,3400,3400);
            var go=new GameObject("Actual material camera");go.transform.SetParent(studio.transform,false);camera=go.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.nearClipPlane=.01f;camera.farClipPlane=10;camera.cullingMask=1<<29;camera.allowHDR=false;camera.allowMSAA=false;camera.useOcclusionCulling=false;
            key=Lamp("Material preview key",new Vector3(35,-35,0),1.6f,new Color(1,.95f,.86f));fill=Lamp("Material preview fill",new Vector3(20,140,0),.8f,new Color(.75f,.86f,1));
        }
        Light Lamp(string name,Vector3 angles,float intensity,Color color)
        {var go=new GameObject(name);go.transform.SetParent(studio.transform,false);go.transform.rotation=Quaternion.Euler(angles);var light=go.AddComponent<Light>();light.type=LightType.Directional;light.intensity=intensity;light.color=color;light.cullingMask=1<<29;light.shadows=LightShadows.None;light.enabled=false;return light;}
        RenderTexture Render(string id)
        {
            GameObject subject=null;RenderTexture texture=null;RenderTexture previous=RenderTexture.active;Renderer[] renderers=null;
            try
            {
                subject=PanicResidenceArt.CreatePickup(id,0);if(!subject)throw new InvalidOperationException("Material mesh missing");subject.transform.SetParent(studio.transform,false);
                foreach(var t in subject.GetComponentsInChildren<Transform>(true))t.gameObject.layer=29;
                foreach(var c in subject.GetComponentsInChildren<Collider>(true))c.enabled=false;
                renderers=subject.GetComponentsInChildren<Renderer>(true);if(renderers.Length==0)throw new InvalidOperationException("Material has no renderer");
                Bounds bounds=renderers[0].bounds;foreach(var r in renderers){bounds.Encapsulate(r.bounds);r.enabled=false;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
                float longest=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));if(longest<.001f)throw new InvalidOperationException("Material bounds empty");
                camera.transform.position=bounds.center+new Vector3(-.42f,.32f,-1).normalized*(1+longest*2);camera.transform.LookAt(bounds.center);camera.aspect=1;float half=0;
                for(int i=0;i<8;i++){Vector3 p=camera.transform.InverseTransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));half=Mathf.Max(half,Mathf.Max(Mathf.Abs(p.x),Mathf.Abs(p.y)));}
                camera.orthographicSize=Mathf.Max(.02f,half/.86f);
                texture=new RenderTexture(128,128,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="V414 actual material "+id,hideFlags=HideFlags.HideAndDontSave,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};texture.Create();camera.targetTexture=texture;
                key.enabled=fill.enabled=true;foreach(var r in renderers)r.enabled=true;camera.Render();RenderCount++;var result=texture;texture=null;return result;
            }
            finally
            {
                if(renderers!=null)foreach(var r in renderers)if(r)r.enabled=false;if(key)key.enabled=false;if(fill)fill.enabled=false;if(camera)camera.targetTexture=null;RenderTexture.active=previous;
                if(subject)UnityEngine.Object.Destroy(subject);if(texture){texture.Release();UnityEngine.Object.Destroy(texture);}
            }
        }
        public void Dispose()
        {if(disposed)return;disposed=true;foreach(var texture in cache.Values)if(texture){texture.Release();UnityEngine.Object.Destroy(texture);}cache.Clear();failed.Clear();if(studio)UnityEngine.Object.Destroy(studio);studio=null;camera=null;key=null;fill=null;}
    }
}
