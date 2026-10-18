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
            DrawFinalBagStatusDesignV57();
        }

        void DrawFinalBagNeed(Rect rect, string id, string title, int packed)
        {
            DrawIcon(new Rect(rect.x, rect.y, 44, 48), id);
            Label(new Rect(rect.x + 54, rect.y, rect.width - 54, 22), title, _caps);
            Label(new Rect(rect.x + 54, rect.y + 24, rect.width - 54, 28), packed > 0 ? "1 / 1  HAZIR" : "0 / 1  AL", ColorText(_body, packed > 0 ? Sea : Brass));
        }

        void DrawFinalBagHud()
        {
            DrawFinalBagHudDesignV57();
        }
    }
}
