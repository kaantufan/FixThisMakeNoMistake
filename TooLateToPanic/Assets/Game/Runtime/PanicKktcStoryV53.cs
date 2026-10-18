using System.Globalization;

namespace TooLateToPanic
{
    /// <summary>
    /// Presentation copy for KKTC / Güzelyurt / Kalkanlı; people and events are fictional.
    /// This catalog never changes model state, timing, shelter acceptance, damage, rewards, scores or audio.
    /// </summary>
    public static class PanicKktcStoryV53
    {
        public const string LocationBadge = "KKTC · Güzelyurt · Kalkanlı";
        public const string FictionNotice = "Oyundaki kişiler ve afet olayları kurmacadır.";
        public const string MenuLocation = "KKTC · Güzelyurt · Kalkanlı";
        public const string MenuPremise = "Ailen güvenli alanda. Aile evini sıradaki dalgaya hazırla.";
        public const string OpeningTitle = "Kalkanlı'daki evimiz";
        public const string DiaryTitle = "Evimiz için bir not";
        public const string DiaryPersonalNote = "Ailem güvenli alanda. Komşularla yine aynı sofrada buluşmak için evimi hazırlıyorum.";
        public const string CommunityResponse = "Kalkanlı'da komşuluk sürüyor. Yeniden aynı sofrada buluşmak için hazırlanıyoruz.";
        public const string RunEndedCaption = "Ailen güvenli alanda. Bu ev için öğrendiklerini bir sonraki denemeye taşı.";
        public const string ModeShelterBridgeV56 = "Kısa akış noktaları örnektir; serbest oyunda selde üst kat panik odasına, diğer afetlerde bodruma geç.";

        // The existing opening owns its 16-second duration and camera/skip flow.
        // These boundaries only choose copy for the existing four 4-second shots.
        public static string OpeningCaption(float age, bool usesUpperPanicRoom)
        {
            if (age < 4f) return OpeningPlaceCaption();
            if (age < 8f) return OpeningPreparationCaption();
            if (age < 12f) return OpeningBagCaption();
            return OpeningShelterCaption(usesUpperPanicRoom);
        }

        public static string OpeningPlaceCaption()
        {
            return "KKTC, Güzelyurt, Kalkanlı. Ailen güvenli alanda; aile evi sana emanet.";
        }

        public static string OpeningPreparationCaption()
        {
            return PanicModel.NormalPrecautionSeconds.ToString("0", CultureInfo.InvariantCulture) +
                " saniye: malzeme topla, eşyaları sabitle, evini güçlendir.";
        }

        public static string OpeningBagCaption()
        {
            return "Son " + PanicModel.NormalBagSeconds.ToString("0", CultureInfo.InvariantCulture) +
                " saniye göz hizasında: E / F ile erzak al. Bir su ve bir gıda yeter.";
        }

        public static string OpeningShelterCaption(bool usesUpperPanicRoom)
        {
            return usesUpperPanicRoom
                ? "Bu sel dalgasında üst kat panik odasına çık; işaretli yerde E ile gir."
                : "Bu dalgada bodrum sığınağına in; kapakta E ile gir. Selde üst kat panik odası kullanılır.";
        }

        // Campaign-only flavor. Callers keep the model's existing requirements,
        // action hints and actual result values; short flow/simulation do not use
        // the escalating campaign sentence.
        public static string CampaignWaveNarrative(int wave, DisasterType disaster)
        {
            return "Kalkanlı · Dalga " + wave.ToString(CultureInfo.InvariantCulture) + " · " + HazardName(disaster) +
                (wave <= 1 ? ". Aile evinin ilk sınavı." : ". Dalgalar sertleşiyor; evini yeniden hazırla.");
        }

        public static string HazardNarrative(DisasterType disaster)
        {
            switch (disaster)
            {
                case DisasterType.Earthquake:
                    return "Kalkanlı sarsılıyor. Aile evinde yaptığın hazırlık şimdi sınanıyor.";
                case DisasterType.Flood:
                    return "Kalkanlı'nın sokaklarında su yükseliyor. Evdeki önlemler şimdi sınanıyor.";
                case DisasterType.Fire:
                    return "Kalkanlı'da alevlerin ışığı görülüyor. Evdeki önlemler şimdi sınanıyor.";
                case DisasterType.Storm:
                    return "Rüzgâr Kalkanlı'nın çatılarını zorluyor. Evdeki önlemler şimdi sınanıyor.";
                default:
                    return "Kalkanlı'da yeni bir dalga başladı. Evdeki hazırlığın şimdi sınanıyor.";
            }
        }

        // Use Model.ReachedShelterV5, never deadline/no-loss/cinematic state,
        // for reachedShelter. This describes campaign state, not safety advice.
        public static string ShelterResponseCaption(DisasterType disaster, bool reachedShelter)
        {
            if (!reachedShelter)
                return "Sığınağa ulaşamadın. Afet seni bulunduğun yerde yakaladı.";
            return disaster == DisasterType.Flood
                ? "Üst kat panik odasındasın. Kalkanlı'da sel dalgası sürüyor."
                : "Bodrum sığınağındasın. Kalkanlı'da afet dalgası sürüyor.";
        }

        // These lines describe the game's completed hazard only. The caller must
        // retain TotalDamage, FailureReason, reward and protected-run disclosures.
        public static string AftermathNarrative(int wave, DisasterType disaster)
        {
            return "Dalga " + wave.ToString(CultureInfo.InvariantCulture) + " · " + HazardName(disaster) +
                " geride kaldı. Evin durumunu incele; sonraki hazırlığını planla.";
        }

        // Short-flow story has its own physical targets. In particular its fire
        // assembly point must not be replaced with the campaign basement route.
        public static string ShortFlowPremise()
        {
            return "Kalkanlı'daki aile evinde bir önlemle başla. Sonucunu gör, hazırlığını büyüt.";
        }

        public static string HazardName(DisasterType disaster)
        {
            switch (disaster)
            {
                case DisasterType.Earthquake: return "Deprem";
                case DisasterType.Flood: return "Sel";
                case DisasterType.Fire: return "Yangın";
                case DisasterType.Storm: return "Fırtına";
                default: return "Afet";
            }
        }
    }
}
