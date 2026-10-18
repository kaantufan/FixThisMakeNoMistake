using System;
using UnityEngine;

namespace TooLateToPanic
{
    // Headless policy hook; no scene/player/font/render/input work. Root may call from its Editor receipt driver.
    public static class PanicCaptionMarkerEditorProbeV59
    {
        public static void Run(Action<string, bool, string> check)
        {
            Rect caption = PanicDialogueSourcePolicyV59.CaptionBounds;
            check("caption.marker.repro", !PanicCaptionMarkerPolicyV59.Avoids(new Rect(824,162,112,41), caption), "Actual R4 screenshot marker footprint is rejected.");
            check("caption.marker.below", PanicCaptionMarkerPolicyV59.Avoids(new Rect(824,226,112,41), caption), "Popup below the caption retains a clear border and dot.");
            check("caption.marker.trailing-dot", !PanicCaptionMarkerPolicyV59.Avoids(new Rect(824,63,112,41), caption), "A box above the caption is rejected if its trailing dot crosses text.");
            check("caption.marker.leader", !PanicCaptionMarkerPolicyV59.LeaderAvoids(new Vector2(880,180),new Rect(824,268,112,41),caption), "Leader from a world point inside the caption yields instead of crossing the sentence.");
            foreach (int width in new[] { 112,180,430 })
            {
                bool found = false;
                for (int row = -12; row <= 12; row++)
                {
                    Rect candidate = new Rect(800-width*.5f,162+row*53,width,41);
                    if (candidate.yMin >= 142 && candidate.yMax <= 835 && PanicCaptionMarkerPolicyV59.Avoids(candidate,caption)) found=true;
                }
                check("caption.marker.search."+width,found,"Existing bounded marker search retains a legal non-overlapping candidate for each production width.");
            }
        }
    }
}
