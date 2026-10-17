using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        public void BeginFinalBagStage()
        {
            _helpVisible = false;
            CancelInventoryDrag();
        }

        void DrawFinalBagStatus()
        {
            EnsureCompactStylesV5();
            Rect task = new Rect(24, 20, 356, 62);
            Frame(task, Night, Line, 1); Region(task);
            Label(new Rect(38, 28, 328, 20), "AFET ÇANTASI", _tiny);
            FittedLabel(new Rect(38, 49, 328, 26), World.Model.BagReady ? World.JuryActive ? "Hazır · güvenli alana geç" : "Hazır · " + World.ActiveShelterLabelV5 : "1 su + 1 gıda al", _body, World.Model.BagReady ? Sea : MainText, 18);
            Rect timer = TimerCardBounds;
            Frame(timer, Night, Line, 1); Region(timer);
            Label(new Rect(timer.x + 14, timer.y + 8, timer.width - 28, 19), "KALAN SÜRE", _tiny);
            Label(new Rect(timer.x + 14, timer.y + 27, timer.width - 28, 29), FormatTime(World.Model.TimeRemaining), ColorText(compactValueV5, World.Model.TimeRemaining <= 15 ? Brass : MainText));
            Rect need = new Rect(1300, 20, 276, 62);
            Frame(need, Night, Line, 1); Region(need);
            Label(new Rect(1314, 29, 248, 24), "SU " + Mathf.Min(1, World.Model.PackedWater) + "/1    GIDA " + Mathf.Min(1, World.Model.PackedFood) + "/1", ColorText(_body, World.Model.BagReady ? Sea : Brass));
            Label(new Rect(1314, 55, 248, 20), "TAB · ÇANTAYI DÜZENLE", _tiny);
        }

        void DrawFinalBagNeed(Rect rect, string id, string title, int packed)
        {
            DrawIcon(new Rect(rect.x, rect.y, 44, 48), id);
            Label(new Rect(rect.x + 54, rect.y, rect.width - 54, 22), title, _caps);
            Label(new Rect(rect.x + 54, rect.y + 24, rect.width - 54, 28), packed > 0 ? "1 / 1  HAZIR" : "0 / 1  AL", ColorText(_body, packed > 0 ? Sea : Brass));
        }

        void DrawFinalBagHud()
        {
            DrawFinalBagStatus();
            if (World.BagCameraTransitionActive)
            {
                Rect entering = new Rect(540, 708, 520, 54);
                Frame(entering, Night, Brass, 1f); Region(entering);
                Label(new Rect(556, 715, 488, 38), "AFET ÇANTASINI HAZIRLA", ColorText(_center, MainText));
                return;
            }
            Label(new Rect(788, 434, 24, 24), "+", ColorText(_center, MainText));
            if (!string.IsNullOrEmpty(World.FocusLabel))
            {
                Rect focus = new Rect(610, 490, 380, 64);
                Frame(focus, Night, Line, 1f); Region(focus);
                FittedLabel(new Rect(624, 494, 352, 25), World.FocusLabel, _heading, MainText, 16);
                FittedLabel(new Rect(624, 524, 352, 23), World.ContextHint, _small, Brass, 12);
            }

        }
    }
}
