using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public static class PanicCaptionMarkerPolicyV59
    {
        // Include the marker's border, six-pixel breathing room and its trailing dot.
        public static Rect Footprint(Rect marker)
        { return Rect.MinMaxRect(marker.xMin - 6f, marker.yMin - 6f, marker.xMax + 6f, marker.yMax + 14f); }
        public static bool Avoids(Rect marker, Rect caption)
        { return !Footprint(marker).Overlaps(caption); }
        public static bool LeaderAvoids(Vector2 origin, Rect marker, Rect caption)
        {
            Vector2 end = new Vector2(Mathf.Clamp(origin.x, marker.xMin, marker.xMax), Mathf.Clamp(origin.y, marker.yMin, marker.yMax));
            Rect envelope = Rect.MinMaxRect(Mathf.Min(origin.x, end.x) - 2f, Mathf.Min(origin.y, end.y) - 2f,
                Mathf.Max(origin.x, end.x) + 2f, Mathf.Max(origin.y, end.y) + 2f);
            return !envelope.Overlaps(caption);
        }
    }
    public sealed partial class PanicHomeHud
    {
        readonly List<Rect> dialogueClearMarkerRectsV59 = new List<Rect>();
        int dialogueClearMarkerFrameV59 = -1;
        public bool DialogueMarkerClearanceActiveV59
        {
            // Source state is intentional: World.OnGUI may run before the HUD paints its caption.
            // Conservative reservation also covers caption fade/suspension transitions.
            get { return World && !string.IsNullOrEmpty(World.PresentationSubtitle); }
        }
        public IReadOnlyList<Rect> RenderedWorldMarkerBoundsV59
        { get { return dialogueClearMarkerFrameV59 == Time.frameCount ? dialogueClearMarkerRectsV59 : System.Array.Empty<Rect>(); } }
        public bool WorldMarkerAvoidsDialogueV59(Rect marker)
        { return !DialogueMarkerClearanceActiveV59 || PanicCaptionMarkerPolicyV59.Avoids(marker, DialogueSubtitleBoundsV59); }
        bool WorldMarkerLeaderAvoidsDialogueV59(Vector2 origin, Rect marker)
        { return !DialogueMarkerClearanceActiveV59 || PanicCaptionMarkerPolicyV59.LeaderAvoids(origin, marker, DialogueSubtitleBoundsV59); }
        void RecordWorldMarkerBoundsV59(Rect marker)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            if (dialogueClearMarkerFrameV59 != Time.frameCount)
            { dialogueClearMarkerFrameV59 = Time.frameCount; dialogueClearMarkerRectsV59.Clear(); }
            dialogueClearMarkerRectsV59.Add(marker);
        }
    }
}
