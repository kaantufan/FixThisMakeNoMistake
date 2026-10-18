using System;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        PanicExperienceFeedbackOwnerV414 feedbackOwnerV414;
        PanicStressAudioV414 stressAudioV414;
        Texture2D fpsVeilV414;
        float fpsStressV414,fpsStressClockV414;
        Quaternion fpsPoseV414;
        bool fpsPoseReadyV414;
        public bool ReducedFpsMotionV54 { get; private set; }
        // Locomotion reads the player's unmodified look, while selection uses View's
        // displayed centre ray. Cosmetic roll cannot steer the CharacterController.
        public Quaternion FpsInputRotationV54 { get { return Quaternion.Euler(bagPitch,bagYaw,0); } }
        public float StressIntensityV414 { get { return fpsStressV414; } }
        public bool StressAudioActiveV414 { get { return FpsFeedbackAudioPlayingV414; } }
        public float PoseSwayAmplitudeV414 { get { return FpsStressVisibleV414&&!ReducedFpsMotionV54 ? Mathf.Lerp(1.5f,2.15f,fpsStressV414) : 0; } }
        public float FpsStressIntensityV414 { get { return fpsStressV414; } }
        public float FpsVeilOpacityV414 { get { return FpsStressVisibleV414 ? EvaluateFpsVeilOpacityV54(fpsStressClockV414,fpsStressV414,ReducedFpsMotionV54) * BagDollyFpsSettleV56 : 0; } }
        public float FpsMovementMultiplierV414 { get { return FpsStressVisibleV414 ? Mathf.Lerp(1,.94f,fpsStressV414) : 1; } }
        public float FpsLookMultiplierV414 { get { return FpsStressVisibleV414 ? Mathf.Lerp(1,.94f,fpsStressV414) : 1; } }
        public float FpsStressClockV414 { get { return fpsStressClockV414; } }
        public Quaternion FpsDisplayedRotationV414 { get { return fpsPoseV414; } }
        public bool FpsFeedbackAudioPlayingV414 { get { return stressAudioV414!=null&&stressAudioV414.Playing; } }
        public float FpsHeartVolumeV51 { get { return stressAudioV414 == null ? 0 : stressAudioV414.HeartVolume; } }
        public float FpsBreathVolumeV51 { get { return stressAudioV414 == null ? 0 : stressAudioV414.BreathVolume; } }
        public Texture2D FpsVeilTextureV414 { get { return fpsVeilV414; } }
        public bool ExperienceFeedbackBlockedV414 { get { return Model==null||Model.IsPaused||NotebookBlocksInput||InventoryOpen||InputLocked||CinematicActive||LossAnnounced||WorkActive||
            hud&&(hud.DeveloperMenuOpen||hud.DeveloperInputBlockedThisFrame||hud.NotebookModalBlocked); } }
        public bool FpsStressVisibleV414 { get { return BagFirstPersonActive&&BagFinalStageActive&&!ExperienceFeedbackBlockedV414; } }
        public static float EvaluateDeadlineStressV414(float seconds)
        { if(float.IsNaN(seconds)||float.IsInfinity(seconds))return 0;float t=1-Mathf.Clamp01(seconds/PanicModel.NormalBagSeconds);return t*t*(3-2*t); }

        public void SetReducedFpsMotionV54(bool value)
        { ReducedFpsMotionV54=value;fpsPoseReadyV414=false; }

        // Continuous deterministic camera-only tension. The controller still reads
        // FpsInputRotationV54; the displayed centre ray follows the displayed pose.
        // Reduced motion removes sway/FOV movement and keeps a steady veil.
        public static Vector3 EvaluateFpsSwayDegreesV54(float clock,float urgency,bool reducedMotion)
        {
            if(reducedMotion||float.IsNaN(clock)||float.IsInfinity(clock))return Vector3.zero;
            urgency=FiniteUnitV54(urgency);
            float strength=Mathf.Lerp(.70f,1f,urgency);
            return new Vector3((Mathf.Sin(clock*1.37f)*.78f+Mathf.Sin(clock*25.13f)*.17f)*strength,
                (Mathf.Sin(clock*.93f+.8f)*.28f+Mathf.Sin(clock*23.24f+.4f)*.10f)*strength,
                (Mathf.Sin(clock*1.12f)*1.82f+Mathf.Sin(clock*26.08f)*.33f)*strength);
        }
        public static float EvaluateFpsFovV54(float clock,float urgency,bool reducedMotion)
        {
            if(reducedMotion||float.IsNaN(clock)||float.IsInfinity(clock))return 62;
            return 62+Mathf.Sin(clock*1.66f)*Mathf.Lerp(.55f,1.1f,FiniteUnitV54(urgency));
        }
        public static float EvaluateFpsVeilOpacityV54(float clock,float urgency,bool reducedMotion)
        {
            if(float.IsNaN(clock)||float.IsInfinity(clock))clock=0;
            if(reducedMotion)return .58f;
            // A 7.2 s cycle has about 1.4 s of clear relief, joined with slow
            // one-second ramps. Darkness never flashes or blankets the centre.
            float phase=Mathf.Repeat(clock+3.6f,7.2f);
            float dark=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.70f,1.75f,phase))*
                (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(5.45f,6.50f,phase)));
            return Mathf.Lerp(.22f,Mathf.Lerp(.80f,.88f,FiniteUnitV54(urgency)),dark);
        }
        static float FiniteUnitV54(float value)
        {return float.IsNaN(value)||float.IsInfinity(value)?0:Mathf.Clamp01(value);}

        void InitExperienceFeedbackV414()
        {
            if(feedbackOwnerV414)return;
            feedbackOwnerV414=gameObject.AddComponent<PanicExperienceFeedbackOwnerV414>();feedbackOwnerV414.Owner=this;
            stressAudioV414=new PanicStressAudioV414(transform);
            InitFpsTensionV55();
            fpsVeilV414=new Texture2D(96,96,TextureFormat.RGBA32,false,true){name="V54_ClearCentrePanicVignette",hideFlags=HideFlags.HideAndDontSave,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            var pixels=new Color32[96*96];
            for(int y=0;y<96;y++)for(int x=0;x<96;x++)
            {
                float nx=(x/95f-.5f)*2,ny=(y/95f-.5f)*2;
                float radial=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.28f,1.02f,Mathf.Sqrt(nx*nx+ny*ny)));
                byte alpha=(byte)Mathf.RoundToInt(Mathf.Lerp(.025f,1,radial)*255);
                pixels[y*96+x]=new Color32(6,9,13,alpha);
            }
            fpsVeilV414.SetPixels32(pixels);fpsVeilV414.Apply(false,true);
            InitGatherFeedbackV414();
        }
        public void TickExperienceFeedbackV414(float dt)
        {
            if(!feedbackOwnerV414)InitExperienceFeedbackV414();
            dt=float.IsNaN(dt)||float.IsInfinity(dt)?0:Mathf.Clamp(dt,0,.1f);
            TickFpsTensionV55(dt);
            TickGatherFeedbackV414(dt);
            if((!BagFirstPersonActive&&!BagHandoffActiveV51)||!BagFinalStageActive)
            {fpsStressV414=0;fpsStressClockV414=0;fpsPoseReadyV414=false;stressAudioV414.Stop();return;}
            if(ExperienceFeedbackBlockedV414||dt<=0){stressAudioV414.Stop();return;}
            fpsStressClockV414+=dt;
            // Audible onset while the view is black, then sustained urgency that rises
            // with the real remaining bag clock. Presentation is bounded separately;
            // it never changes the controller, preparation timing or model outcome.
            float onset = .68f * (1 - Mathf.SmoothStep(0, 1, fpsStressClockV414 / 5f));
            fpsStressV414=Mathf.Max(.32f+.68f*EvaluateDeadlineStressV414(Model.TimeRemaining),onset);
            stressAudioV414.Tick(fpsStressV414);
        }
        public void SuspendExperienceFeedbackV414()
        {if(stressAudioV414!=null)stressAudioV414.Stop();StopFpsTensionV55();}
        public void DisposeExperienceFeedbackV414()
        {
            DisposeFpsTensionV55();
            if(stressAudioV414!=null)stressAudioV414.Dispose();stressAudioV414=null;
            if(fpsVeilV414)Destroy(fpsVeilV414);fpsVeilV414=null;
            DisposeGatherFeedbackV414();fpsStressV414=0;fpsPoseReadyV414=false;
        }
        /// <summary>Only called by existing bag camera owner; same displayed transform is used by the selection ray.</summary>
        public void ApplyBagPoseV414()
        {
            if(!BagFirstPersonActive||!View)return;
            if(!fpsPoseReadyV414||!ExperienceFeedbackBlockedV414)
            {
                Vector3 sway=FpsStressVisibleV414?EvaluateFpsSwayDegreesV54(fpsStressClockV414,fpsStressV414,ReducedFpsMotionV54):Vector3.zero;
                // Every same-frame look/ray request uses the new input immediately.
                // The stress clock advances only in TickExperienceFeedbackV414.
                fpsPoseV414=FpsInputRotationV54*Quaternion.Euler(sway*BagDollyFpsSettleV56);fpsPoseReadyV414=true;
                View.fieldOfView=FpsStressVisibleV414?Mathf.Lerp(62,EvaluateFpsFovV54(fpsStressClockV414,fpsStressV414,ReducedFpsMotionV54),BagDollyFpsSettleV56):62;
            }
            View.transform.SetPositionAndRotation(PlayerPosition+Vector3.up*1.62f,fpsPoseV414);
        }
    }
    public sealed partial class PanicHomeHud
    {
        /// <summary>Invoke before scaled FPS cards. Pixel-space draw restores every global GUI state it changes.</summary>
        public void DrawFpsVeilV414()
        {
            if(!World||!World.FpsStressVisibleV414||!World.FpsVeilTextureV414||Event.current==null||Event.current.type!=EventType.Repaint)return;
            Matrix4x4 matrix=GUI.matrix;Color color=GUI.color;int depth=GUI.depth;
            try{GUI.matrix=Matrix4x4.identity;GUI.color=new Color(1,1,1,World.FpsVeilOpacityV414);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),World.FpsVeilTextureV414,ScaleMode.StretchToFill,true);}
            finally{GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;}
        }
    }
    public sealed class PanicExperienceFeedbackOwnerV414 : MonoBehaviour
    {
        public PanicHomeWorld Owner;
        void OnDisable(){if(Owner)Owner.SuspendExperienceFeedbackV414();}
        void OnDestroy(){if(!ReferenceEquals(Owner,null))Owner.DisposeExperienceFeedbackV414();Owner=null;}
    }
    /// <summary>Imported heartbeat plus retained original nonverbal breath. Never spoken voice.</summary>
    public sealed class PanicStressAudioV414 : IDisposable
    {
        GameObject root;AudioSource heart,breath;AudioClip heartClip,breathClip;bool ownsHeart;
        public bool Playing { get { return heart&&heart.isPlaying||breath&&breath.isPlaying; } }
        public float HeartVolume { get { return heart&&heart.isPlaying?heart.volume:0; } }
        public float BreathVolume { get { return breath&&breath.isPlaying?breath.volume:0; } }
        public PanicStressAudioV414(Transform owner)
        {
            root=new GameObject("V414 local breath and heartbeat");root.transform.SetParent(owner,false);
            heartClip=PanicAudioBank.Get("heartbeat");ownsHeart=!heartClip;if(ownsHeart)heartClip=MakeHeart();breathClip=MakeBreath();heart=Source("Soft double heartbeat",heartClip);breath=Source("Soft nonverbal breath",breathClip);
        }
        AudioSource Source(string name,AudioClip clip)
        {
            var go=new GameObject(name);go.transform.SetParent(root.transform,false);var a=go.AddComponent<AudioSource>();a.clip=clip;a.loop=true;a.playOnAwake=false;a.spatialBlend=0;a.volume=0;a.priority=190;a.dopplerLevel=0;return a;
        }
        public void Tick(float urgency)
        {
            if(!heart||!breath)return;urgency=Mathf.Clamp01(urgency);
            heart.volume=Mathf.Lerp(.10f,.23f,urgency)*PanicAudioSettingsV56.EffectiveSfxGain;heart.pitch=Mathf.Lerp(1f,1.42f,urgency);
            breath.volume=Mathf.Lerp(.035f,.075f,urgency)*PanicAudioSettingsV56.EffectiveSfxGain;breath.pitch=Mathf.Lerp(.96f,1.30f,urgency);
            if(!heart.isPlaying)heart.Play();if(!breath.isPlaying)breath.Play();
        }
        public void Stop(){if(heart)heart.Stop();if(breath)breath.Stop();}
        static AudioClip MakeHeart()
        {
            const int rate=22050;var data=new float[rate];
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate;
                float first=t<.16f?Mathf.Sin(t*Mathf.PI*2*58)*Mathf.Exp(-t*29)*Mathf.Sin(Mathf.Clamp01(t/.012f)*Mathf.PI*.5f):0;
                float b=t-.19f;float second=b>=0&&b<.14f?Mathf.Sin(b*Mathf.PI*2*71)*Mathf.Exp(-b*33)*Mathf.Sin(Mathf.Clamp01(b/.010f)*Mathf.PI*.5f)*.65f:0;
                data[i]=(first+second)*.55f;
            }
            var clip=AudioClip.Create("V414_OriginalSoftHeartbeat",data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        static AudioClip MakeBreath()
        {
            const int rate=22050;var data=new float[rate*3];uint seed=0x414BA6u;float low=0;
            for(int i=0;i<data.Length;i++)
            {
                seed=1664525u*seed+1013904223u;float noise=((seed>>8)/16777215f)*2-1;low=Mathf.Lerp(low,noise,.09f);
                float t=i/(float)rate;float inhale=Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/1.28f));float exhale=Mathf.Sin(Mathf.PI*Mathf.Clamp01((t-1.40f)/1.45f));
                float envelope=t<1.28f?inhale*.65f:t>1.40f?exhale:0;data[i]=low*envelope*.32f;
            }
            var clip=AudioClip.Create("V414_OriginalNonverbalBreath",data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        public void Dispose()
        {Stop();if(root)UnityEngine.Object.Destroy(root);if(ownsHeart&&heartClip)UnityEngine.Object.Destroy(heartClip);if(breathClip)UnityEngine.Object.Destroy(breathClip);root=null;heart=null;breath=null;heartClip=null;breathClip=null;}
    }
}
