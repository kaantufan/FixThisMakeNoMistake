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
            Rect heading = new Rect(28, 24, 430, 88);
            Frame(heading, Night, Line, 1f); Region(heading);
            Label(new Rect(46, 34, 394, 20), "SON AŞAMA  /  AFET ÇANTASI", ColorText(_caps, Brass));
            FittedLabel(new Rect(46, 59, 394, 40), World.Model.BagReady ? World.JuryActive ? "Çantan hazır · güvenli alana geç" : "Çantan hazır · sığınağa git" : "Bir su + bir gıda yeterli", _heading, MainText, 18);
            Rect timer = new Rect(650, 24, 300, 88);
            Frame(timer, Night, Brass, 1f); Region(timer);
            Label(new Rect(668, 33, 264, 20), World.JuryActive ? "KORUNMAYA KALAN SÜRE" : "SIĞINAĞA KALAN SÜRE", ColorText(_caps, Brass));
            Label(new Rect(668, 53, 264, 48), Mathf.CeilToInt(World.Model.TimeRemaining).ToString("00") + " sn", _timer);
            Rect essentials = new Rect(1144, 24, 428, 88);
            Frame(essentials, Night, Line, 1f); Region(essentials);
            DrawFinalBagNeed(new Rect(1160, 40, 190, 54), "water", "SU", World.Model.PackedWater);
            DrawFinalBagNeed(new Rect(1364, 40, 190, 54), "food", "GIDA", World.Model.PackedFood);
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
                Rect entering = new Rect(420, 682, 760, 82);
                Frame(entering, Night, Brass, 1f); Region(entering);
                Label(new Rect(442, 694, 716, 50), "AFET ÇANTASINI HAZIRLA", ColorText(_center, MainText));
                return;
            }
            // World positions and stock, never mock values.
            Vector3 goal = PanicHomeWorld.BunkerPosition;
            string instruction = "Bodrumdaki sığınağa git · E ile gir";
            if (World.JuryActive && World.Model.BagReady && World.JurySnapshot != null)
            {
                goal = World.JurySnapshot.TargetPosition;
                instruction = "İşaretli güvenli alana geç · E ile korun";
            }
            if (!World.Model.BagReady)
            {
                float best = float.MaxValue;
                foreach (var supply in World.Expedition.WorldSupplies)
                {
                    if (supply.Count <= 0 || supply.ItemId != "water" && supply.ItemId != "food") continue;
                    if (supply.ItemId == "water" && World.Model.PackedWater > 0 || supply.ItemId == "food" && World.Model.PackedFood > 0) continue;
                    float distance = Vector3.Distance(World.PlayerPosition, supply.Position);
                    if (distance >= best) continue;
                    best = distance; goal = supply.Position;
                    instruction = (supply.ItemId == "water" ? "Suyu al" : "Gıdayı al") + " · E / F ile doğrudan çantana koy";
                }
            }
            Rect guide = new Rect(28, 130, 430, 68);
            Frame(guide, Night, Line, 1f); Region(guide);
            FittedLabel(new Rect(44, 136, 398, 28), instruction, _body, MainText, 14);
            float meters = Vector3.Distance(World.PlayerPosition, goal);
            string level = goal.y < World.PlayerPosition.y - 1.6f ? " · alt katta" : goal.y > World.PlayerPosition.y + 1.6f ? " · üst katta" : " · bu katta";
            Vector3 delta = goal - World.View.transform.position;
            float side = Vector3.Dot(World.View.transform.right, delta.normalized);
            string direction = Vector3.Dot(World.View.transform.forward, delta.normalized) < 0 ? "Arkanda" : side < -.28f ? "Solunda" : side > .28f ? "Sağında" : "Önünde";
            Label(new Rect(44, 166, 398, 22), direction + " · " + meters.ToString("0.0") + " m" + level, ColorText(_tiny, Brass));
            Label(new Rect(788, 434, 24, 24), "+", ColorText(_center, MainText));
            if (!string.IsNullOrEmpty(World.FocusLabel))
            {
                Rect focus = new Rect(530, 490, 540, 74);
                Frame(focus, Night, Line, 1f); Region(focus);
                FittedLabel(new Rect(546, 497, 508, 29), World.FocusLabel, _heading, MainText, 16);
                FittedLabel(new Rect(546, 530, 508, 24), World.ContextHint, _small, Brass, 12);
            }
            Rect controls = new Rect(360, 824, 880, 48);
            Frame(controls, Night, Line, 1f); Region(controls);
            FittedLabel(new Rect(376, 832, 848, 30), "WASD  Yürü     FARE  Bak     E / F  Al     TAB  Çanta     J  Defter     ESC  Duraklat", _small, MainText, 12);
        }
    }
}
