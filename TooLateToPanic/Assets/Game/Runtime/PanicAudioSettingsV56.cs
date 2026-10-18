using System;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Single persistent in-game mix authority. Never writes OS/AudioListener state or gameplay saves.</summary>
    public static class PanicAudioSettingsV56
    {
        static bool loaded, muted, musicEnabled = true;
        static float master = 1f, music = 1f, sfx = 1f;
        static string prefix;
        public static string ProfilePreferencePrefix { get { EnsureLoaded(); return prefix; } }
        public static bool IsolatedProfile { get { EnsureLoaded(); return prefix != "TLTP.Audio.V56."; } }
        public static float MasterVolume { get { EnsureLoaded(); return master; } set { EnsureLoaded(); value=Unit(value); if(master==value)return; master = value; Save(); } }
        public static float MusicVolume { get { EnsureLoaded(); return music; } set { EnsureLoaded(); value=Unit(value); if(music==value)return; music = value; Save(); } }
        public static float SfxVolume { get { EnsureLoaded(); return sfx; } set { EnsureLoaded(); value=Unit(value); if(sfx==value)return; sfx = value; Save(); } }
        public static bool Muted { get { EnsureLoaded(); return muted; } set { SetMuted(value); } }
        public static bool MusicEnabled { get { EnsureLoaded(); return musicEnabled; } set { EnsureLoaded(); if(musicEnabled==value)return; musicEnabled = value; Save(); } }
        public static float EffectiveSfxGain { get { EnsureLoaded(); return muted ? 0f : master * sfx; } }
        public static float EffectiveMusicGain { get { EnsureLoaded(); return muted || !musicEnabled ? 0f : master * music; } }
        public static void SetVolumes(float masterVolume, float musicVolume, float sfxVolume)
        { EnsureLoaded(); masterVolume=Unit(masterVolume); musicVolume=Unit(musicVolume); sfxVolume=Unit(sfxVolume); if(master==masterVolume&&music==musicVolume&&sfx==sfxVolume)return; master = masterVolume; music = musicVolume; sfx = sfxVolume; Save(); }
        public static void SetMuted(bool value) { EnsureLoaded(); if(muted==value)return; muted = value; Save(); }
        public static void ResetToDefaults() { EnsureLoaded(); master = music = sfx = 1f; muted = false; musicEnabled = true; Save(); }
        static float Unit(float value) { return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Clamp01(value); }
        static void EnsureLoaded() { if (!loaded) Reload(); }
        public static void Reload()
        {
            if (prefix == null)
            {
                prefix = "TLTP.Audio.V56.";
                // Derive isolation before the first read, independent of World Awake order.
                // Existing --home-evidence launch owns its own preferences as well as its score path.
                var args = Environment.GetCommandLineArgs();
                for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--home-evidence")
                {
                    string identity = System.IO.Path.GetFullPath(args[i + 1]).ToUpperInvariant();
                    ulong hash = 14695981039346656037UL;
                    for (int c = 0; c < identity.Length; c++) { hash ^= identity[c]; hash *= 1099511628211UL; }
                    prefix += "QA." + hash.ToString("X16") + ".";
                    break;
                }
            }
            master = Unit(PlayerPrefs.GetFloat(prefix + "master", 1f));
            music = Unit(PlayerPrefs.GetFloat(prefix + "music", 1f));
            sfx = Unit(PlayerPrefs.GetFloat(prefix + "sfx", 1f));
            muted = PlayerPrefs.GetInt(prefix + "mute", 0) != 0;
            musicEnabled = PlayerPrefs.GetInt(prefix + "musicEnabled", 1) != 0;
            loaded = true;
        }
        public static void Save()
        {
            EnsureLoaded();
            PlayerPrefs.SetFloat(prefix + "master", master); PlayerPrefs.SetFloat(prefix + "music", music); PlayerPrefs.SetFloat(prefix + "sfx", sfx);
            PlayerPrefs.SetInt(prefix + "mute", muted ? 1 : 0); PlayerPrefs.SetInt(prefix + "musicEnabled", musicEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
