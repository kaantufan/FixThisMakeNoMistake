using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        /// <summary>Root calls at the start of DrawWork. Uses real work/session/camera data.</summary>
        bool DrawManualWorkV414()
        {
            if (!World || !World.WorkActive) return false;
            PanicWorkSession work = World.Expedition.Work;
            if (work.UsesHammer)
            {
                Rect panel = new Rect(456, 620, 688, 192);
                Frame(panel, Night, Line, 1f); Region(panel);
                FittedLabel(new Rect(478, 632, 640, 34), work.Label, _heading, MainText, 20);
                Label(new Rect(480, 675, 620, 31), "SPACE · " + work.CompletedStrokes + " / " + work.RequiredStrokes + " VURUŞ", ColorText(_body, Brass));
                Label(new Rect(480, 713, 620, 28), "Ayrı ayrı bas. İş bitince harcanır; iptal ücretsiz.", _small);
                DrawBar(new Rect(480, 766, 432, 8), work.Progress, Sea);
                Button(new Rect(934, 751, 184, 42), "İPTAL · ESC", false, Line, World.CancelWork);
                return true;
            }
            Rect closeup = new Rect(66, 164, 1468, 676);
            Frame(closeup, Night, Brass, 1f); Region(closeup);
            Rect image = new Rect(88, 186, 1010, 632);
            Frame(image, new Color(.07f,.065f,.06f,1f), Line, 1f);
            Rect fitted = FittedTextureRect(new Rect(90, 225, 1006, 554), World.WorkTexture);
            World.SetManualWorkViewportV414(fitted);
            if (World.WorkTexture) GUI.DrawTexture(fitted, World.WorkTexture, ScaleMode.StretchToFill, false);
            Label(new Rect(110, 191, 950, 30), "SOL TUŞU BASILI TUT · OK YÖNÜNDE ÇEVİR", ColorText(_tiny, Brass));
            Label(new Rect(110, 785, 950, 28), work.IsUtility ? "Mevcut tesisatı kapatmak ücretsizdir. İptal: ESC / BACKSPACE" :
                "İş tamamlanınca bir kez harcanır. İptal: ESC / BACKSPACE", _tiny);
            for (int i = 0; i < work.TotalSteps; i++)
            {
                Rect hit = World.ManualWorkTargetBoundsV414(i);
                if (hit.width <= 0f) continue;
                bool done = i < work.CompletedSteps, current = i == work.NextClickTarget;
                Color accent = done ? Sea : current ? Brass : Muted;
                DrawRing(hit, 1f, accent, current ? 3 : 1);
                if (done) DrawCheck(new Rect(hit.center.x - 10, hit.center.y - 7, 20, 14), Sea);
                else if (current)
                {
                    DrawRing(new Rect(hit.x + 6, hit.y + 6, hit.width - 12, hit.height - 12), work.StageProgress, Sea, 4);
                    Label(new Rect(hit.center.x - 62, hit.yMax + 9, 124, 28), work.ExpectedDirection > 0 ? "SAAT YÖNÜ" : "TERS YÖN", ColorText(_center, Brass));
                    DrawWorkDirectionV414(hit.center, 56, work.ExpectedDirection > 0, Brass);
                }
            }
            const float x = 1122, w = 384;
            Brush(new Rect(x, 190, w, 38), Rust);
            Label(new Rect(x + 14, 190, w - 26, 36), work.IsUtility ? "TESİSAT GÜVENLİĞİ" : "MANUEL MONTAJ", _body);
            FittedLabel(new Rect(x, 248, w, 54), work.Label, _heading, MainText, 20);
            Label(new Rect(x, 326, w, 50), "Basılı tut ve çevir.", _heading);
            Label(new Rect(x, 389, w, 80), work.ExpectedDirection > 0 ? "Farenin sol tuşunu basılı tut.\nParçanın çevresinde saat yönünde dön." :
                "Farenin sol tuşunu basılı tut.\nParçanın çevresinde saatin tersine dön.", _wrap);
            Label(new Rect(x, 484, w, 90), work.IsUtility ? PanicUtilitySafetyV414.Effect(work.Action) :
                "Bırakınca durur. Yanlış yön geri alır.\nİki vida bitince eşya sabitlenir.", _wrap);
            Label(new Rect(x, 591, w, 39), work.IsUtility || work.Action == "hose" ? Mathf.RoundToInt(work.Progress * 100) + "%" :
                work.CompletedSteps + " / " + work.TotalSteps + " VİDA", _display);
            DrawBar(new Rect(x, 656, w, 9), work.Progress, Sea);
            Button(new Rect(x, 729, w, 58), "İPTAL ET · ESC", false, Line, World.CancelWork);
            return true;
        }
        void DrawWorkDirectionV414(Vector2 center, float radius, bool clockwise, Color color)
        {
            Vector2 last = Vector2.zero;
            for (int k = 0; k <= 15; k++)
            {
                float angle = (-135f + (clockwise ? 1 : -1) * k * 18f) * Mathf.Deg2Rad;
                Vector2 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                if (k > 0) DrawSegmentV414(last, next, color);
                last = next;
            }
            float end = (-135f + (clockwise ? 1 : -1) * 270f) * Mathf.Deg2Rad;
            Vector2 tangent = new Vector2(-Mathf.Sin(end), Mathf.Cos(end)) * (clockwise ? 1 : -1);
            Vector2 side = new Vector2(-tangent.y, tangent.x);
            DrawSegmentV414(last, last - tangent * 13 + side * 7, color);
            DrawSegmentV414(last, last - tangent * 13 - side * 7, color);
        }
        void DrawSegmentV414(Vector2 a, Vector2 b, Color color)
        {
            Matrix4x4 before = GUI.matrix; Color tint = GUI.color;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg, a);
            GUI.color = color; GUI.DrawTexture(new Rect(a.x, a.y-1.5f, Vector2.Distance(a,b), 3), Texture2D.whiteTexture);
            GUI.color = tint; GUI.matrix = before;
        }
    }
}
