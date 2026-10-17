using System;

namespace TooLateToPanic
{
    public enum PanicNotebookPage { Rooms, Flood, Earthquake, Fire, Storm, Inventory }

    /// <summary>Deterministic, allocation-free animation state. No coroutine, scene or gameplay writes.</summary>
    public sealed class PanicNotebookAnimation
    {
        public const float OpenSeconds = .36f;
        public const float TurnSeconds = .62f;
        public bool TargetOpen { get; private set; }
        public float Openness { get; private set; }
        public bool Visible { get { return Openness > 0f || TargetOpen; } }
        public bool Turning { get; private set; }
        public bool ReducedMotion { get; private set; }
        public PanicNotebookPage CurrentPage { get; private set; }
        public PanicNotebookPage FromPage { get; private set; }
        public PanicNotebookPage ToPage { get; private set; }
        public PanicNotebookPage RequestedPage { get; private set; }
        public float TurnProgress { get; private set; }
        public int TurnDirection { get; private set; } = 1;
        public int CompletedTurns { get; private set; }
        PanicNotebookPage? queued;

        public float OpenEase { get { return Smooth(Openness); } }
        public static float Smooth(float t) { t = Clamp(t, 0, 1); return t * t * (3 - 2 * t); }
        static float Clamp(float x, float lo, float hi) { return x < lo ? lo : x > hi ? hi : x; }
        static bool Valid(PanicNotebookPage page) { return page >= PanicNotebookPage.Rooms && page <= PanicNotebookPage.Inventory; }

        public void SetOpen(bool value)
        {
            TargetOpen = value;
            if (ReducedMotion) { Openness = value ? 1 : 0; if (!value) SettleRequested(); }
            else if (!value && Openness == 0) SettleRequested();
        }

        public void SetReducedMotion(bool value)
        {
            ReducedMotion = value;
            if (value) { Openness = TargetOpen ? 1 : 0; SettleRequested(); }
        }

        public void RequestPage(PanicNotebookPage page)
        {
            if (!Valid(page)) return;
            RequestedPage = page;
            if (ReducedMotion || !Visible) { SettleRequested(); return; }
            if (!Turning) { if (CurrentPage != page) Begin(page); return; }
            if (page == FromPage) { queued = null; TurnDirection = -1; }
            else if (page == ToPage) { queued = null; TurnDirection = 1; }
            else { queued = page; TurnDirection = 1; }
        }

        public void Tick(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds <= 0f) return;
            float change = seconds / OpenSeconds;
            Openness = Clamp(Openness + (TargetOpen ? change : -change), 0, 1);
            if (Openness == 0f && !TargetOpen) { SettleRequested(); return; }
            if (ReducedMotion) return;
            // Only one queued target exists. Consume overshoot so low FPS does not lose requests.
            for (int i = 0; i < 3 && Turning && seconds > 0; i++)
            {
                float distance = TurnDirection > 0 ? 1 - TurnProgress : TurnProgress;
                float untilEnd = distance * TurnSeconds;
                if (seconds < untilEnd)
                {
                    TurnProgress = Clamp(TurnProgress + TurnDirection * seconds / TurnSeconds, 0, 1);
                    break;
                }
                seconds -= untilEnd;
                CurrentPage = TurnDirection > 0 ? ToPage : FromPage;
                if (TurnDirection > 0) CompletedTurns++;
                Turning = false;
                TurnProgress = 0;
                if (queued.HasValue && queued.Value != CurrentPage)
                {
                    var next = queued.Value; queued = null; Begin(next);
                }
                else { queued = null; RequestedPage = CurrentPage; }
            }
        }

        /// <summary>External modal/phase takes ownership immediately; remember the requested page.</summary>
        public void ForceClosed()
        {
            TargetOpen = false; Openness = 0; SettleRequested();
        }

        void Begin(PanicNotebookPage page)
        {
            FromPage = CurrentPage; ToPage = page; TurnProgress = 0; TurnDirection = 1; Turning = true;
        }
        void SettleRequested()
        {
            CurrentPage = FromPage = ToPage = RequestedPage; Turning = false; TurnProgress = 0; queued = null;
        }
    }

    public readonly struct PanicNotebookStrip
    {
        public readonly float Y, Z, Angle, Light;
        public PanicNotebookStrip(float y, float z, float angle, float light) { Y = y; Z = z; Angle = angle; Light = light; }
    }

    /// <summary>Adapted from defter-mockup paperCurve. Fixed-size strip output, finite projection and clip spans.</summary>
    public static class PanicNotebookPaper
    {
        public const int StripCount = 24;
        public const float Perspective = 2200;

        public static void Sample(float progress, float height, PanicNotebookStrip[] result)
        {
            if (result == null || result.Length == 0 || height <= 0) return;
            if (float.IsNaN(progress) || float.IsInfinity(progress)) progress = 0;
            float phase = PanicNotebookAnimation.Smooth(progress);
            float root = 360 * PanicNotebookAnimation.Smooth(phase);
            double sin = Math.Max(0, Math.Sin(Math.PI * phase));
            float curl = Math.Max(0, Math.Min(480 * (float)Math.Pow(sin, .75), 720 - 2 * root));
            float step = height / result.Length, y = 0, z = -6 * phase;
            for (int i = 0; i < result.Length; i++)
            {
                float angle = root + curl * (i + .5f) / result.Length;
                double radians = angle * Math.PI / 180;
                result[i] = new PanicNotebookStrip(y, z, angle, .9f + .12f * (float)Math.Cos(radians - .4));
                y += step * (float)Math.Cos(radians); z += step * (float)Math.Sin(radians);
            }
        }

        public static float Project(float coordinate, float z)
        { return coordinate * Perspective / Math.Max(200, Perspective - z); }

        /// <summary>Intersection hidden behind the stationary page (y >= 0 AND z <= -.15).</summary>
        public static bool HiddenInterval(PanicNotebookStrip point, float length, out float start, out float end)
        {
            start = 0; end = 1;
            if (length <= 0) return false;
            double angle = point.Angle * Math.PI / 180;
            if (!Clip(point.Y, (float)Math.Cos(angle) * length, ref start, ref end)) return false;
            return Clip(-point.Z - .15f, -(float)Math.Sin(angle) * length, ref start, ref end) && end > start;
        }
        static bool Clip(float value, float slope, ref float start, ref float end)
        {
            if (Math.Abs(slope) < .0000001f) return value >= 0;
            float intercept = -value / slope;
            if (slope > 0) start = Math.Max(start, intercept); else end = Math.Min(end, intercept);
            return end > start;
        }
    }
}
