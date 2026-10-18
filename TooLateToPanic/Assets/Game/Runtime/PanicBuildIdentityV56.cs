using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    // The visible build label is independent of productName/persistentDataPath.
    // Only this process's own standalone window can be renamed.
    public sealed class PanicBuildIdentityV56 : MonoBehaviour
    {
        public const string WindowTitleV56 = "Too Late to Panic! — Home V5.9";
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        IntPtr window;
        uint processId;
        float nextAttempt;
        readonly StringBuilder caption = new StringBuilder(128);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetWindowTextW")]
        static extern bool SetWindowText(IntPtr handle, string text);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowTextW")]
        static extern int GetWindowText(IntPtr handle, StringBuilder text, int count);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr handle, out uint pid);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (FindFirstObjectByType<PanicBuildIdentityV56>()) return;
            var node = new GameObject("V5.6 visible build identity");
            DontDestroyOnLoad(node); node.AddComponent<PanicBuildIdentityV56>();
        }
        void Awake() { using (var own = Process.GetCurrentProcess()) processId = (uint)own.Id; }
        void Update()
        {
            if (Time.realtimeSinceStartup < nextAttempt) return;
            nextAttempt = Time.realtimeSinceStartup + 1f;
            if (window == IntPtr.Zero)
                using (var own = Process.GetCurrentProcess()) { own.Refresh(); window = own.MainWindowHandle; }
            if (window == IntPtr.Zero) return;
            GetWindowThreadProcessId(window, out uint owner);
            if (owner != processId) { window = IntPtr.Zero; return; }
            caption.Length = 0;
            GetWindowText(window, caption, caption.Capacity);
            if (caption.ToString() != WindowTitleV56) SetWindowText(window, WindowTitleV56);
        }
#endif
    }
}
