using UnityEngine;

namespace TooLateToPanic
{
    public struct PanicManualWorkLabelLayoutV59
    {
        public Rect Instruction, ClosedBadge, ProtectedGrip, ProtectedClosed;
        public Vector2 ClosedPoint, LeaderStart, LeaderEnd;
    }

    // Pure logical-canvas geometry. Measured font sizes and the actual production hit/grip
    // projection are passed in; this never changes the viewport, pointer, work or physical rig.
    public static class PanicManualWorkLabelPolicyV59
    {
        public const float Gap = 16f;
        public static Rect Inflate(Rect r, float amount)
        { return Rect.MinMaxRect(r.xMin - amount, r.yMin - amount, r.xMax + amount, r.yMax + amount); }
        public static bool Contains(Rect outer, Rect inner)
        { return inner.width > 0 && inner.height > 0 && inner.xMin >= outer.xMin - .01f && inner.xMax <= outer.xMax + .01f && inner.yMin >= outer.yMin - .01f && inner.yMax <= outer.yMax + .01f; }
        public static Vector2 BadgeSize(Vector2 measured)
        { return new Vector2(Mathf.Ceil(measured.x) + 28f, Mathf.Max(36f, Mathf.Ceil(measured.y) + 12f)); }
        static Rect Clamp(Rect bounds, Vector2 desired, Vector2 size)
        { return new Rect(Mathf.Clamp(desired.x, bounds.xMin, bounds.xMax - size.x), Mathf.Clamp(desired.y, bounds.yMin, bounds.yMax - size.y), size.x, size.y); }
        static bool Place(Rect bounds, Vector2 size, Vector2 desired, Rect a, Rect b, Rect c, out Rect placed)
        {
            placed = new Rect();
            if (size.x > bounds.width || size.y > bounds.height || size.x <= 0 || size.y <= 0) return false;
            Rect preferred = Clamp(bounds, desired, size);
            bool Free(Rect candidate) { return Contains(bounds, candidate) && !candidate.Overlaps(a) && !candidate.Overlaps(b) && (c.width <= 0 || !candidate.Overlaps(c)); }
            if (Free(preferred)) { placed = preferred; return true; }
            float best = float.PositiveInfinity;
            // Finite alternatives around the actual protected controls and at the content edges.
            float[] xs = { desired.x, bounds.xMin, bounds.xMax - size.x, a.xMin - Gap - size.x, a.xMax + Gap, b.xMin - Gap - size.x, b.xMax + Gap, c.xMin - Gap - size.x, c.xMax + Gap };
            float[] ys = { desired.y, bounds.yMin, bounds.yMax - size.y, a.yMin - Gap - size.y, a.yMax + Gap, b.yMin - Gap - size.y, b.yMax + Gap, c.yMin - Gap - size.y, c.yMax + Gap };
            foreach (float x in xs) foreach (float y in ys)
            {
                Rect candidate = Clamp(bounds, new Vector2(x, y), size);
                if (!Free(candidate)) continue;
                float distance = (candidate.position - desired).sqrMagnitude;
                if (distance < best) { best = distance; placed = candidate; }
            }
            return !float.IsPositiveInfinity(best);
        }
        public static bool TryLayout(Rect content, Rect hit, Vector2 closedPoint, Vector2 instructionSize, Vector2 closedSize, out PanicManualWorkLabelLayoutV59 layout)
        {
            layout = new PanicManualWorkLabelLayoutV59();
            // Current ring/progress/direction arrow occupy at most70px from the grip centre.
            Rect grip = Inflate(hit, 30f), closed = new Rect(closedPoint.x - 18f, closedPoint.y - 18f, 36f, 36f);
            Vector2 instructionAt = new Vector2(hit.center.x - instructionSize.x * .5f, Mathf.Max(grip.yMax, closed.yMax) + Gap);
            if (!Place(content, instructionSize, instructionAt, grip, closed, new Rect(), out var instruction)) return false;
            Vector2 badgeAt = new Vector2(Mathf.Max(grip.xMax, closed.xMax) + Gap, closed.yMin - closedSize.y - Gap);
            if (!Place(content, closedSize, badgeAt, grip, closed, Inflate(instruction, Gap), out var badge)) return false;
            Vector2 end = new Vector2(Mathf.Clamp(closedPoint.x, badge.xMin, badge.xMax), Mathf.Clamp(closedPoint.y, badge.yMin, badge.yMax));
            Vector2 delta = end - closedPoint;
            layout = new PanicManualWorkLabelLayoutV59 { Instruction = instruction, ClosedBadge = badge, ProtectedGrip = grip,
                ProtectedClosed = closed, ClosedPoint = closedPoint, LeaderStart = closedPoint + (delta.sqrMagnitude > .001f ? delta.normalized * 13f : Vector2.zero), LeaderEnd = end };
            return true;
        }
    }

    public sealed partial class PanicHomeHud
    {
        GUIStyle manualUtilityLabelStyleV59;
        public Rect ManualUtilityInstructionBoundsV59 { get; private set; }
        public Rect ManualUtilityClosedBadgeBoundsV59 { get; private set; }
        public Vector2 ManualUtilityInstructionMeasuredV59 { get; private set; }
        public Vector2 ManualUtilityClosedMeasuredV59 { get; private set; }
        public int ManualUtilityLabelsDrawFrameV59 { get; private set; } = -1;

        void DrawUtilityWorkLabelsV59(Rect hit, Rect fitted, PanicWorkSession work)
        {
            if (!World.ManualWorkClosedGripV414(out var closed)) return;
            if (manualUtilityLabelStyleV59 == null)
                manualUtilityLabelStyleV59 = MakeStyle(_condensedFont, 21, FontStyle.Bold, MainText, TextAnchor.MiddleCenter);
            string instruction = work.UsesLinearDrag ? "AŞAĞI İNDİR" : Mathf.RoundToInt(work.RequiredDegrees) + "° SAAT YÖNÜ";
            const string closedText = "KAPALI KONUM";
            Vector2 instructionMeasured = manualUtilityLabelStyleV59.CalcSize(new GUIContent(instruction));
            Vector2 closedMeasured = manualUtilityLabelStyleV59.CalcSize(new GUIContent(closedText));
            Rect content = Rect.MinMaxRect(fitted.xMin + 12f, fitted.yMin + 12f, fitted.xMax - 12f, fitted.yMax - 12f);
            if (!PanicManualWorkLabelPolicyV59.TryLayout(content, hit, closed,
                PanicManualWorkLabelPolicyV59.BadgeSize(instructionMeasured), PanicManualWorkLabelPolicyV59.BadgeSize(closedMeasured), out var layout)) return;

            // The goal ring remains exactly at its real closed grip; labels have independent safe areas.
            DrawManualRingV51(new Rect(closed.x - 10f, closed.y - 10f, 20f, 20f), 1f, Sea, 2f);
            DrawLine(layout.LeaderStart, layout.LeaderEnd, new Color(Sea.r, Sea.g, Sea.b, .7f), 1.5f);
            Frame(layout.Instruction, Night, new Color(Brass.r, Brass.g, Brass.b, .65f), 1f);
            Frame(layout.ClosedBadge, Night, new Color(Sea.r, Sea.g, Sea.b, .65f), 1f);
            Label(layout.Instruction, instruction, ColorText(manualUtilityLabelStyleV59, Brass));
            Label(layout.ClosedBadge, closedText, ColorText(manualUtilityLabelStyleV59, Sea));
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                ManualUtilityInstructionBoundsV59 = layout.Instruction; ManualUtilityClosedBadgeBoundsV59 = layout.ClosedBadge;
                ManualUtilityInstructionMeasuredV59 = instructionMeasured; ManualUtilityClosedMeasuredV59 = closedMeasured;
                ManualUtilityLabelsDrawFrameV59 = Time.frameCount;
            }
        }
    }
}
