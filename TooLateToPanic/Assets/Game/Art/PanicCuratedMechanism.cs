using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Passive visual clip sampler. World owns state, clocks, pause and every interaction.</summary>
    public sealed class PanicCuratedMechanism : MonoBehaviour
    {
        public GameObject SampleRoot;
        public AnimationClip OpenClip, CloseClip;
        public bool Container;
        public string AssetName;
        [SerializeField] float amount;
        [SerializeField] bool initialized;
        [SerializeField] Transform status, drop, turbine;
        [SerializeField] Transform hatchLid, lockWheel, hydraulicPiston;
        [SerializeField] Vector3 statusScale, dropScale;
        float turbineClock;

        // No Update, physics, audio, input, scoring, model reference or animation events.
        public void SampleState(bool open, bool safe, float dt)
        {
            if (!SampleRoot || !OpenClip || !CloseClip) return;
            // Container amount is door openness. Utility amount is isolation/closedness.
            bool target = Container ? open : safe;
            if (!initialized)
            {
                initialized = true; amount = target ? 1f : 0f;
                foreach (var t in SampleRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "StatusLED_Pivot") { status = t; statusScale = t.localScale; }
                    if (t.name == "WaterDrop_Pivot") { drop = t; dropScale = t.localScale; }
                    if (t.name == "MeterTurbine_Pivot") turbine = t;
                    if (t.name == "HatchLid_Hinge") hatchLid = t;
                    if (t.name == "LockWheel_Pivot") lockWheel = t;
                    if (t.name == "HydraulicPiston_Pivot") hydraulicPiston = t;
                }
            }
            float duration = Mathf.Max(.01f, target ? (Container ? OpenClip : CloseClip).length : (Container ? CloseClip : OpenClip).length);
            amount = Mathf.MoveTowards(amount, target ? 1f : 0f, Mathf.Max(0, dt) / duration);
            var clip = target ? (Container ? OpenClip : CloseClip) : (Container ? CloseClip : OpenClip);
            float phase = target ? amount : 1f - amount;
            clip.SampleAnimation(SampleRoot, Mathf.Clamp01(phase) * clip.length);

            // Explicit 4-stage heavy mechanical kinematics for BunkerHatch (LockWheel spin -> gasket break -> heavy hydraulic lift -> damper bounce)
            if (AssetName == "BunkerHatch" && hatchLid)
            {
                float openNorm = Mathf.Clamp01(amount);
                float lidDeg = EvaluateBunkerHatchLidCurve(openNorm, target);
                float wheelDeg = EvaluateBunkerWheelSpinCurve(openNorm);
                hatchLid.localRotation = Quaternion.Euler(lidDeg, 0f, 0f);
                if (lockWheel) lockWheel.localRotation = Quaternion.Euler(0f, wheelDeg, 0f);
                if (hydraulicPiston)
                {
                    float liftRatio = Mathf.Clamp01(Mathf.Abs(lidDeg) / 96f);
                    hydraulicPiston.localRotation = Quaternion.Euler(liftRatio * 18f, 0f, 0f);
                }
            }

            // Upstream clips omit these status channels. Use the same passive state amount
            // to settle the LED and stop the small water flow without inventing gameplay.
            if (!Container)
            {
                float running = 1 - amount;
                if (status) status.localScale = statusScale * Mathf.Max(.001f, running);
                if (drop) drop.localScale = dropScale * Mathf.Max(.001f, running);
                if (turbine) { turbineClock += Mathf.Max(0, dt) * running * 260f; turbine.localRotation = Quaternion.Euler(0, 0, turbineClock); }
            }
        }

        static float EvaluateBunkerHatchLidCurve(float t, bool opening)
        {
            if (opening)
            {
                // Stage 1 (0..0.22): Lock wheel unlatches; heavy steel lid barely cracks open (0 -> -3 deg)
                if (t < 0.22f)
                {
                    float s = t / 0.22f;
                    return Mathf.Lerp(0f, -3f, s * s);
                }
                // Stage 2 (0.22..0.38): Gasket seal breaks with high resistance (-3 -> -16 deg)
                if (t < 0.38f)
                {
                    float s = (t - 0.22f) / 0.16f;
                    return Mathf.Lerp(-3f, -16f, s * s);
                }
                // Stage 3 (0.38..0.84): Hydraulic gas struts engage & lift heavy mass past balance point (-16 -> -101.5 deg overshoot)
                if (t < 0.84f)
                {
                    float s = (t - 0.38f) / 0.46f;
                    float smooth = s * s * (3f - 2f * s);
                    return Mathf.Lerp(-16f, -101.5f, smooth);
                }
                // Stage 4 (0.84..1.00): Heavy stop-block impact + hydraulic damper recoil (-101.5 -> -93.8 -> -96 deg)
                float settle = (t - 0.84f) / 0.16f;
                if (settle < 0.55f)
                {
                    float b = settle / 0.55f;
                    return Mathf.Lerp(-101.5f, -93.8f, b * (2f - b));
                }
                float r = (settle - 0.55f) / 0.45f;
                return Mathf.Lerp(-93.8f, -96f, r * r * (3f - 2f * r));
            }
            else
            {
                // When closing, t goes from 1 -> 0. Map u = 1 - t (0 -> 1 closing progress)
                float u = 1f - t;
                if (u < 0.65f)
                {
                    float s = u / 0.65f;
                    // Accelerating under heavy gravity
                    return Mathf.Lerp(-96f, -2f, s * s * s);
                }
                if (u < 0.82f)
                {
                    float s = (u - 0.65f) / 0.17f;
                    // Metallic thud + slight upward shudder on rubber gasket
                    return s < 0.45f ? Mathf.Lerp(-2f, 0f, s / 0.45f) : Mathf.Lerp(0f, -2.2f, (s - 0.45f) / 0.55f);
                }
                float lockSettle = (u - 0.82f) / 0.18f;
                return Mathf.Lerp(-2.2f, 0f, lockSettle * lockSettle);
            }
        }

        static float EvaluateBunkerWheelSpinCurve(float t)
        {
            if (t < 0.25f)
            {
                float s = t / 0.25f;
                float smooth = s * s * (3f - 2f * s);
                return Mathf.Lerp(0f, 240f, smooth);
            }
            if (t < 0.40f)
            {
                float s = (t - 0.25f) / 0.15f;
                return Mathf.Lerp(240f, 270f, 1f - (1f - s) * (1f - s));
            }
            return 270f;
        }
    }
}
