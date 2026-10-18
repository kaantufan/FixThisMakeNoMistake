using System;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Opt-in isolated audio-smoke checks. Executing these does not establish listening/render acceptance.</summary>
    public static class PanicDialogueNativeChecksV59
    {
        public static void Run(PanicPresentationAudio router, Action<string,bool> check)
        {
            if (!router || check == null) return;
            check("v5.9 audio preferences isolated", PanicAudioSettingsV56.IsolatedProfile);
            if (!PanicAudioSettingsV56.IsolatedProfile) return;
            float master = PanicAudioSettingsV56.MasterVolume, music = PanicAudioSettingsV56.MusicVolume, sfx = PanicAudioSettingsV56.SfxVolume;
            bool mute = PanicAudioSettingsV56.Muted;
            try
            {
                PanicAudioSettingsV56.SetVolumes(.5f, music, .4f); PanicAudioSettingsV56.SetMuted(false);
                router.ResetRun(); router.Configure(true); router.Muted = false; router.SpeechVolume = .5f;
                check("v5.9 full caption immediately", router.TryPlay("jury_intro") && router.Subtitle == PanicDialogueSessionV59.Clean(PanicKktcStoryV53.ShortFlowPremise()));
                check("v5.9 no overlapping lower priority", !router.TryCaptionV59("low", "Eski bilgi", 1) && router.PendingCaptionCountV59 == 0);
                router.Tick(.1f, false);
                var sources = router.GetComponentsInChildren<AudioSource>(); AudioSource bits = null;
                foreach (var source in sources) if (source.gameObject.name.Contains("Nonverbal dialogue")) bits = source;
                check("v5.9 real bounded imported bit", bits && bits.clip && bits.clip.length >= .025f && bits.clip.length <= .18f);
                check("v5.9 gain multiplied once", bits && Mathf.Abs(bits.volume - .11f*.5f*.5f*.4f) < .00001f);
                for (int i=0; i<12; i++) router.Tick(.1f,false);
                check("v5.9 bounded three bits", router.BitDispatchCountV59 == 3 && router.Subtitle.Length > 0);
                string text = router.Subtitle; float remaining = router.SubtitleRemainingV59; int count = router.BitDispatchCountV59;
                router.Tick(.1f,true); router.Tick(.1f,true);
                check("v5.9 paused lifetime text bits frozen", router.Subtitle == text && router.SubtitleRemainingV59 == remaining && router.BitDispatchCountV59 == count && !router.SpeechActive);
                check("v5.9 paused events rejected", !router.TryPlay("jury_secured"));
                router.Tick(.1f,false);
                check("v5.9 resume retains complete caption", router.Subtitle == text && router.SubtitleRemainingV59 < remaining);
                PanicAudioSettingsV56.SetMuted(true); router.Tick(.1f,false);
                bool allSilent=true; foreach(var source in sources) allSilent &= !source.isPlaying && source.volume == 0f;
                check("v5.9 global mute immediately silences", allSilent && !router.SpeechActive && router.Subtitle.Length > 0);
                check("v5.9 event dedupe", !router.TryPlay("jury_intro"));
                router.ResetRun(); check("v5.9 new run clears stale text", router.Subtitle == "" && router.PendingCaptionCountV59 == 0);
                check("v5.9 new run can show same current event", router.TryPlay("jury_intro"));
                router.Configure(false); check("v5.9 disable clears subtitle", router.Subtitle == "" && !router.SpeechActive);
            }
            finally
            {
                router.Configure(false);
                PanicAudioSettingsV56.SetVolumes(master,music,sfx); PanicAudioSettingsV56.SetMuted(mute);
            }
        }
    }
}
