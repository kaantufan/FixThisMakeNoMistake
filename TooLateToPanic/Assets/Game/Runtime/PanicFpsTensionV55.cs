using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        AudioClip fpsTensionClipV55;
        public bool FpsTensionPlayingV55 { get { return PanicBackgroundMusic.Instance&&PanicBackgroundMusic.Instance.FpsTrackPlayingV59; } }
        public float FpsTensionVolumeV55 { get { return PanicBackgroundMusic.Instance?PanicBackgroundMusic.Instance.FpsTrackVolumeV59:0f; } }
        public AudioClip FpsTensionClipV55 { get { return fpsTensionClipV55; } }
        // Compatibility semantic probe. Music playback now has exactly one owner.
        public bool FpsTensionOwnsMusicV55 { get { return BagFinalStageActive&&(BagFirstPersonActive||BagHandoffActiveV51); } }
        public bool FpsTensionAllowedV55
        { get { return isActiveAndEnabled&&FpsTensionOwnsMusicV55&&Model!=null&&Model.Phase==PanicPhase.Preparing&&
            !ExperienceFeedbackBlockedV414&&!BagCameraTransitionActive&&!BagHandoffActiveV51&&PanicBackgroundMusic.Enabled&&
            PanicBackgroundMusic.Volume>0f&&PanicAudioSettingsV56.EffectiveMusicGain>0f&&(Application.isFocused||Application.runInBackground); } }
        public void TickFpsTensionV55(float dt) { InitFpsTensionV55(); }
        void InitFpsTensionV55() { if(!fpsTensionClipV55)fpsTensionClipV55=PanicBackgroundMusic.LoadTrack("fps_tension"); }
        public void StopFpsTensionV55() { if(PanicBackgroundMusic.Instance)PanicBackgroundMusic.Instance.StopFpsForOwnerV59(this); }
        public void DisposeFpsTensionV55() { StopFpsTensionV55();fpsTensionClipV55=null; }
    }
    public sealed class PanicFpsTensionGuardV55 : MonoBehaviour
    {
        public PanicHomeWorld Owner;
        void Update(){if(Owner&&!Owner.FpsTensionAllowedV55)Owner.StopFpsTensionV55();}
        void OnDisable(){if(Owner)Owner.StopFpsTensionV55();}
    }
}
