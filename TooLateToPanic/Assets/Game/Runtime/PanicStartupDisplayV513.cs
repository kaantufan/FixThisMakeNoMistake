using System;
using System.Linq;
using UnityEngine;

namespace TooLateToPanic
{
    // Apply once at normal startup, including machines with an old windowed preference.
    // Alt+Enter and later display choices remain owned by the player/Unity.
    public static class PanicStartupDisplayV513
    {
        public static bool AppliedV513 { get; private set; }
        public static bool SkippedForTestDriverV513 { get; private set; }
        public static int RequestedWidthV513 { get; private set; }
        public static int RequestedHeightV513 { get; private set; }
        public static bool ShouldApplyV513(string[] args)
        { return args == null || !args.Any(a => PanicTestLaunchRules.Drivers.Contains(a)); }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ApplyNormalStartup()
        {
            AppliedV513 = false;
            SkippedForTestDriverV513 = !ShouldApplyV513(Environment.GetCommandLineArgs());
            if (SkippedForTestDriverV513) return;
            int width = Display.main.systemWidth, height = Display.main.systemHeight;
            if (width <= 0 || height <= 0)
            { var current = Screen.currentResolution; width = current.width; height = current.height; }
            RequestedWidthV513 = Mathf.Max(640, width);
            RequestedHeightV513 = Mathf.Max(480, height);
            Screen.SetResolution(RequestedWidthV513, RequestedHeightV513, FullScreenMode.FullScreenWindow);
            AppliedV513 = true;
            Debug.Log("NORMAL_STARTUP_FULLSCREEN_V513 " + RequestedWidthV513 + "x" + RequestedHeightV513);
        }
#endif
    }
}
