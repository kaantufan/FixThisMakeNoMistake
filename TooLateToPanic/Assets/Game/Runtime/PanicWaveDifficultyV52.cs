using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>One campaign progression curve, shared by real damage and presentation.</summary>
    public static class PanicWaveDifficultyV52
    {
        public static float DamageMultiplierForWave(int wave)
        {
            float step = Mathf.Max(0f, (float)wave - 1f);
            return 1f + .08f * Mathf.Min(step, 9f) + .045f * Mathf.Max(0f, step - 9f);
        }
        public static float PresentationMultiplierForWave(int wave)
        {
            float increase = DamageMultiplierForWave(wave) - 1f;
            // Keep motion/audio/particle work bounded while later waves keep escalating.
            return 1f + .65f * (1f - Mathf.Exp(-increase * .8f));
        }
    }
    public sealed partial class PanicModel
    {
        public float DisasterSeverityV52 { get { return IsSimulation || IsTutorial || IsJuryDemo ? 1f : PanicWaveDifficultyV52.DamageMultiplierForWave(Wave); } }
        public float DisasterPresentationScaleV52 { get { return IsSimulation || IsTutorial || IsJuryDemo ? 1f : PanicWaveDifficultyV52.PresentationMultiplierForWave(Wave); } }
    }
}
