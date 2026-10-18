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
                Label(new Rect(480, 670, 620, 28), "BAĞLANTI · " + work.CompletedStrokes + " / " + work.RequiredStrokes + " VURUŞ", ColorText(_body, Brass));
                for(int n=0;n<work.RequiredStrokes;n++)
                {
                    Rect nail=new Rect(480+n*108,704,94,42);bool done=n<work.CompletedStrokes,current=n==work.CompletedStrokes;
                    Color accent=done?Sea:current?Brass:Muted;
                    DrawManualRingV51(nail,1f,Night,7f);
                    DrawManualRingV51(nail,1f,accent,current?4f:2f);
                    if(done)DrawCheck(new Rect(nail.center.x-8,nail.center.y-6,16,12),Sea);
                    else Label(new Rect(nail.center.x-18,nail.center.y-14,36,28),(n+1).ToString(),ColorText(_center,accent));
                    if(current)
                    {
                        // Keep the original clickable target and one deliberate strike per press.
                        Region(nail);
                        if(MousePressed(nail)){World.StrikeManualWorkV414();World.PlayUiSound();}
                        DrawManualArrowV51(new Vector2(nail.center.x+33,nail.y+5),new Vector2(nail.center.x+33,nail.yMax-5),Brass);
                    }
                }
                Label(new Rect(480, 753, 432, 23), "Tıkla veya SPACE · Basılı tutmak tekrarlamaz.", _tiny);
                DrawBar(new Rect(480, 788, 432, 8), work.Progress, Sea);
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
            Label(new Rect(110, 191, 950, 30), work.Action=="poweroff"?"KIRMIZI KOLU TUT · AŞAĞI İNDİR":work.IsUtility?"VANA KOLUNU TUT · 90° SAAT YÖNÜNDE ÇEVİR":"SOL TUŞU BASILI TUT · SAAT YÖNÜNDE ÇEVİR", ColorText(_tiny, Brass));
            Label(new Rect(110, 785, 950, 28), work.IsUtility ? "Mevcut tesisatı kapatmak ücretsizdir. İptal: ESC / BACKSPACE" :
                "İş tamamlanınca bir kez harcanır. İptal: ESC / BACKSPACE", _tiny);
            for (int i = 0; i < work.TotalSteps; i++)
            {
                Rect hit = World.ManualWorkTargetBoundsV414(i);
                if (hit.width <= 0f) continue;
                bool done = i < work.CompletedSteps, current = i == work.NextClickTarget;
                Color accent = done ? Sea : current ? World.ManualWorkDraggingV414?Sea:Brass : Muted;
                DrawManualRingV51(hit, 1f, Night, current ? 8 : 5);
                DrawManualRingV51(hit, 1f, accent, current ? 4 : 2);
                if (done) DrawCheck(new Rect(hit.center.x - 10, hit.center.y - 7, 20, 14), Sea);
                else if (current)
                {
                    DrawManualRingV51(new Rect(hit.x + 9, hit.y + 9, hit.width - 18, hit.height - 18), work.StageProgress, Sea, 4);
                    Label(new Rect(hit.center.x - 95, hit.yMax + 12, 190, 28), work.UsesLinearDrag?"AŞAĞI İNDİR":Mathf.RoundToInt(work.RequiredDegrees)+"° SAAT YÖNÜ", ColorText(_center, Brass));
                    if(work.IsUtility&&World.ManualWorkClosedGripV414(out Vector2 end))
                    {
                        DrawManualRingV51(new Rect(end.x-10,end.y-10,20,20),1,Sea,2);
                        Label(new Rect(end.x+15,end.y-14,150,28),"KAPALI KONUM",ColorText(_tiny,Sea));
                    }
                    // A single direction cue stays next to the actual selected grip/head.
                    // Pointer trails and a second large orbit obscure the object in close-up.
                    if(work.UsesLinearDrag)DrawManualArrowV51(new Vector2(hit.xMax+17,hit.center.y-23),new Vector2(hit.xMax+17,hit.center.y+23),Brass);
                    else DrawWorkDirectionV414(hit.center,57,true,Brass);
                }
            }
            const float x = 1122, w = 384;
            Brush(new Rect(x, 190, w, 38), Rust);
            Label(new Rect(x + 14, 190, w - 26, 36), work.IsUtility ? "TESİSAT GÜVENLİĞİ" : "MANUEL MONTAJ", _body);
            FittedLabel(new Rect(x, 248, w, 54), work.Label, _heading, MainText, 20);
            Label(new Rect(x, 326, w, 50), work.Action=="poweroff"?"Kolu aşağı sürükle.":work.IsUtility?"Kolu çeyrek tur çevir.":work.Action=="hose"?"Bağlantıyı tut ve çevir.":"Vida başını tut ve çevir.", _heading);
            Label(new Rect(x, 389, w, 80),work.Action=="poweroff"?"Yukarı: elektrik açık.\nAşağı: elektrik kesik.\nAlternatif: ↓ tuşunu basılı tut.":work.IsUtility?"Kol boruya paralel: açık.\nKol boruya dik: kapalı.\nAlternatif: → tuşunu basılı tut.":work.Action=="hose"?"İşaretli bağlantıyı tut. Saat yönünde yarım tur çevir.\nAlternatif: → tuşunu basılı tut.":"Gerçek vida başını tut. Başın çevresinde saat yönünde döndükçe alet ve vida döner.\nAlternatif: → tuşunu basılı tut.",_wrap);
            Label(new Rect(x, 484, w, 90), work.IsUtility ? PanicUtilitySafetyV414.Effect(work.Action) :
                work.Action=="hose"?"Bırakınca durur; yeniden tutup devam et.\nGeri hareket ilerlemeyi silmez.":"Bırakınca durur; yeniden tutup devam et.\nGeri hareket ilerlemeyi silmez.\nİki vida bitince eşya sabitlenir.", _wrap);
            Label(new Rect(x, 591, w, 39), work.IsUtility || work.Action == "hose" ? Mathf.RoundToInt(work.Progress * 100) + "%" :
                work.CompletedSteps + " / " + work.TotalSteps + " VİDA", _display);
            DrawBar(new Rect(x, 656, w, 9), work.Progress, Sea);
            Button(new Rect(x, 729, w, 58), "İPTAL ET · ESC", false, Line, World.CancelWork);
            return true;
        }
        void DrawWorkDirectionV414(Vector2 center, float radius, bool clockwise, Color color, float degrees=90f)
        {
            // Only one short arrow is needed to teach direction; required travel is
            // already shown numerically and by the real session progress ring.
            degrees=Mathf.Clamp(degrees,30f,90f);
            Vector2 last = Vector2.zero;
            for (int k = 0; k <= Mathf.CeilToInt(degrees/6f); k++)
            {
                float angle = (-60f + (clockwise ? 1 : -1) * Mathf.Min(degrees,k * 6f)) * Mathf.Deg2Rad;
                Vector2 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                if (k > 0) DrawSegmentV414(last, next, color);
                last = next;
            }
            float end = (-60f + (clockwise ? 1 : -1) * degrees) * Mathf.Deg2Rad;
            Vector2 tangent = new Vector2(-Mathf.Sin(end), Mathf.Cos(end)) * (clockwise ? 1 : -1);
            Vector2 side = new Vector2(-tangent.y, tangent.x);
            DrawSegmentV414(last, last - tangent * 13 + side * 7, color);
            DrawSegmentV414(last, last - tangent * 13 - side * 7, color);
        }
        void DrawManualRingV51(Rect rect,float value,Color color,float thickness)
        {
            const int pieces=96;
            float amount=Mathf.Clamp01(value);if(amount<=0f)return;
            Vector2 center=rect.center;
            float radius=Mathf.Min(rect.width,rect.height)*.46f;
            Vector2 last=center+Vector2.up*-radius;
            int count=Mathf.CeilToInt(pieces*amount);
            for(int i=1;i<=count;i++)
            {
                float angle=(Mathf.Min(i/(float)pieces,amount)*360f-90f)*Mathf.Deg2Rad;
                Vector2 next=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                DrawLine(last,next,color,thickness);last=next;
            }
        }
        void DrawManualArrowV51(Vector2 a,Vector2 b,Color color)
        {
            Vector2 direction=(b-a).normalized,side=new Vector2(-direction.y,direction.x);
            DrawSegmentV414(a,b,color);
            DrawSegmentV414(b,b-direction*10+side*6,color);
            DrawSegmentV414(b,b-direction*10-side*6,color);
        }
        void DrawSegmentV414(Vector2 a, Vector2 b, Color color)
        {
            // Shared DrawLine composes the logical segment after the incoming canvas
            // scale/offset. RotateAroundPivot mixed these spaces at small resolutions.
            DrawLine(a,b,color,3f);
        }
    }
}
