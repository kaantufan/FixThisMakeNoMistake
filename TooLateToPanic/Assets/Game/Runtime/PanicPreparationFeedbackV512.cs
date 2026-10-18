using System;

namespace TooLateToPanic
{
    public static class PanicPreparationFeedbackV512
    {
        // A popup belongs to an explicit, failed work attempt on this target. Narrative
        // text containing words like "yok" or "gerekmiyor" is never a material request.
        public static string ShortageNotice(PanicExpedition expedition, string focus)
        {
            if (expedition == null || string.IsNullOrEmpty(focus) ||
                expedition.RequirementTargetId != focus || !expedition.HasMissingRequirements) return null;
            return expedition.RequirementBudgetMissing > 0 ? "Para yetmiyor" : "Malzeme eksik";
        }
    }

    public sealed partial class PanicHomeWorld
    {
        bool preparationShortageNoticeV512;
        string preparationNoticeTargetV512;
        void TickPreparationNoticeV512()
        {
            string shortage = PanicPreparationFeedbackV512.ShortageNotice(Expedition, focusId);
            if (preparationShortageNoticeV512 && (shortage == null || preparationNoticeTargetV512 != focusId))
                ResetPreparationNoticeV512();
            if (Model.Message == lastNotice) return;
            lastNotice = Model.Message;
            if (shortage != null)
            {
                Notice(shortage);
                preparationShortageNoticeV512 = true; preparationNoticeTargetV512 = focusId;
            }
            else if ((Model.Message ?? string.Empty).IndexOf("dolu", StringComparison.OrdinalIgnoreCase) >= 0) Notice("Yer yok");
        }
        void ResetPreparationNoticeV512()
        {
            preparationShortageNoticeV512 = false; preparationNoticeTargetV512 = null;
            noticeAge = 0; ShortNotice = string.Empty;
        }
        void ClearStalePreparationNoticeV512(string incomingFocus)
        {
            if (Expedition != null && Expedition.RequirementTargetId != incomingFocus) Expedition.ClearRequirements();
            if (preparationShortageNoticeV512 && preparationNoticeTargetV512 != incomingFocus)
                ResetPreparationNoticeV512();
        }
    }
}
