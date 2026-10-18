using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicNotebookOverlay
    {
        sealed class RoomLabelV56
        {
            public string Text;
            public Vector2 Anchor;
            public Rect Bounds;
        }
        readonly List<RoomLabelV56> roomLabelsV56 = new List<RoomLabelV56>(12);
        readonly List<Rect> drawnRoomLabelsV56 = new List<Rect>(12);
        PanicNotebookPlanSnapshotV55 labelPlanV56;
        Rect labelMapV56;
        public Rect[] MapRoomLabelRectsV56 { get { return drawnRoomLabelsV56.ToArray(); } }
        public int MapRoomLabelDrawFrameV56 { get; private set; } = -1;
        public int MapRoomLabelUnplacedV56 { get; private set; }

        void DrawRoomLabelsV56(PanicNotebookSnapshot data)
        {
            if (planSnapshotV55 == null) return;
            if (!ReferenceEquals(labelPlanV56, planSnapshotV55) || labelMapV56 != PlanRect)
                LayoutRoomLabelsV56(data);
            bool repaint = Event.current.type == EventType.Repaint;
            if (repaint) { drawnRoomLabelsV56.Clear(); MapRoomLabelDrawFrameV56 = Time.frameCount; }
            foreach (var room in roomLabelsV56)
            {
                Rect rect = room.Bounds;
                Vector2 end = new Vector2(Mathf.Clamp(room.Anchor.x, rect.xMin, rect.xMax), Mathf.Clamp(room.Anchor.y, rect.yMin, rect.yMax));
                if (Vector2.Distance(room.Anchor, end) > 15f)
                    Line(room.Anchor, end, new Color(Green.r, Green.g, Green.b, .38f), 1);
                Fill(rect, new Color(Paper.r, Paper.g, Paper.b, .91f));
                Label(new Rect(rect.x + 4, rect.y, rect.width - 8, rect.height), room.Text, tinyStyle, Green);
                if (repaint)
                {
                    Rect local = rect; local.position += PlanRect.position;
                    drawnRoomLabelsV56.Add(ScreenRect(OffsetToBookV55(local)));
                }
            }
        }
        void LayoutRoomLabelsV56(PanicNotebookSnapshot data)
        {
            labelPlanV56 = planSnapshotV55; labelMapV56 = PlanRect;
            roomLabelsV56.Clear(); MapRoomLabelUnplacedV56 = 0;
            var names = new HashSet<string>(StringComparer.Ordinal);
            var obstacles = new List<Rect>(data.Pins.Length + 2);
            obstacles.Add(new Rect(4, 4, 22, 24)); // North mark.
            foreach (var pin in data.Pins)
            { var pinRect = PinRect(pin); pinRect.position -= PlanRect.position; obstacles.Add(pinRect); }
            foreach (var shape in planSnapshotV55.Shapes)
            {
                if (shape.Kind != PanicNotebookPlanKindV55.Door && shape.Kind != PanicNotebookPlanKindV55.Floor) continue;
                string text = shape.Label;
                if (string.IsNullOrEmpty(text) && floor == -1 && shape.Kind == PanicNotebookPlanKindV55.Floor) text = "Bodrum";
                if (string.IsNullOrEmpty(text)) continue;
                text = text.Replace("Ebeveyn odası", "Ebeveyn").Replace("Çocuk odası", "Çocuk")
                    .Replace(" iç kapısı", "").Replace(" kapısı", "");
                if (!names.Add(text)) continue;
                Vector2 anchor = MapDrawPointV55(shape.Pivot);
                float width = Mathf.Clamp(tinyStyle.CalcSize(new GUIContent(text)).x + 8f, 40f, PlanRect.width - 16f);
                float height = Mathf.Max(22f, tinyStyle.lineHeight + 3f);
                Rect best = new Rect(); float bestScore = float.PositiveInfinity;
                // Bounded nearest-fit placement. The real scene projection is untouched.
                for (int row = -7; row <= 7; row++) for (int col = -8; col <= 8; col++)
                {
                    var candidate = new Rect(anchor.x - width * .5f + col * 22f, anchor.y - height * .5f + row * 20f, width, height);
                    candidate.x = Mathf.Clamp(candidate.x, 5f, PlanRect.width - width - 5f);
                    candidate.y = Mathf.Clamp(candidate.y, 5f, PlanRect.height - height - 5f);
                    Rect padded = candidate; padded.xMin -= 3; padded.xMax += 3; padded.yMin -= 3; padded.yMax += 3;
                    bool free = true;
                    foreach (var obstacle in obstacles) if (padded.Overlaps(obstacle)) { free = false; break; }
                    if (!free) continue;
                    float score = (candidate.center - anchor).sqrMagnitude;
                    if (score < bestScore) { bestScore = score; best = candidate; }
                }
                if (float.IsPositiveInfinity(bestScore)) { MapRoomLabelUnplacedV56++; continue; }
                roomLabelsV56.Add(new RoomLabelV56 { Text = text, Anchor = anchor, Bounds = best });
                obstacles.Add(best);
            }
        }
    }
}
