using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        public const float BagHandoffDurationV51 = 2f;
        // The complete camera trip remains visible; no opaque black interval.
        // Total handoff stays two seconds; the real preparation clock remains suspended.
        public const float BagHandoffFadeOutV51 = .18f;
        public const float BagHandoffBlackEndV51 = BagHandoffFadeOutV51;
        int bagBlackDrawFrameV51 = -1;
        public int BagHandoffStartedCountV51 { get; private set; }
        public int BagHandoffFullBlackDrawFramesV51 { get; private set; }
        public float LastBagHandoffFullBlackSecondsV51 { get; private set; }
        public float LastBagHandoffElapsedSecondsV51 { get; private set; }
        public bool EmergencyBagCarriedV51 { get { return Model != null; } }
        public bool BagHandoffActiveV51 { get { return bagViewMode == BagViewMode.Entering && BagFinalStageActive; } }
        // World.Update gates only Model.Tick with this property. No model time, pause,
        // controller, InputLocked or global timeScale ownership changes here.
        public bool BagHandoffPausesPreparationClockV51 { get { return BagHandoffActiveV51; } }
        public float BagHandoffElapsedV51 { get { return BagHandoffActiveV51 ? bagViewAge : 0; } }
        public float BagBlackoutOpacityV51 { get { return BagHandoffActiveV51 && !BagHandoffSuspendedV51 ? EvaluateBagBlackoutOpacityV51(bagViewAge) * (ReducedFpsMotionV54 ? .5f : 1f) : 0; } }
        public bool BagFullBlackIntervalV51 { get { return BagBlackoutOpacityV51 >= .9999f; } }
        public bool BagHandoffSuspendedV51 { get { return Model == null || Model.IsPaused || NotebookBlocksInput || InputLocked || CinematicActive || LossAnnounced ||
            hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame || hud.NotebookModalBlocked); } }

        public static float EvaluateBagBlackoutOpacityV51(float age)
        {
            if (float.IsNaN(age) || float.IsInfinity(age) || age < 0 || age >= BagHandoffDurationV51) return 0;
            float pulse = Mathf.Sin(Mathf.Clamp01(age / BagHandoffDurationV51) * Mathf.PI);
            return .10f * pulse * pulse;
        }

        void BeginBagHandoffDiagnosticsV51()
        {
            BagHandoffStartedCountV51++;
            BagHandoffFullBlackDrawFramesV51 = 0;
            LastBagHandoffFullBlackSecondsV51 = LastBagHandoffElapsedSecondsV51 = 0;
            bagBlackDrawFrameV51 = -1;
            fpsPoseReadyV414 = false;
        }

        void RecordBagHandoffStepV51(float previousAge, float age)
        {
            LastBagHandoffElapsedSecondsV51 = Mathf.Min(BagHandoffDurationV51, age);
            LastBagHandoffFullBlackSecondsV51 = 0;
        }

        /// <summary>Final pixel-space layer, drawn above all game HUD during the handoff.
        /// Pause/developer/notebook menus suspend and expose their own controls.</summary>
        public void DrawBagBlackoutV51()
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            float opacity = BagBlackoutOpacityV51;
            if (opacity <= 0) return;
            Matrix4x4 matrix = GUI.matrix; Color color = GUI.color; int depth = GUI.depth;
            try
            {
                GUI.matrix = Matrix4x4.identity; GUI.depth = -10000;
                GUI.color = new Color(0, 0, 0, opacity);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
                if (opacity >= .9999f && bagBlackDrawFrameV51 != Time.frameCount)
                { BagHandoffFullBlackDrawFramesV51++; bagBlackDrawFrameV51 = Time.frameCount; }
            }
            finally { GUI.matrix = matrix; GUI.color = color; GUI.depth = depth; }
        }
    }
}
