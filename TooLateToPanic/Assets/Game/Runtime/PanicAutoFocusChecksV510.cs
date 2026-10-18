using System;

namespace TooLateToPanic
{
    // Pure deterministic policy cases. Root calls Run in its current-DLL native suite.
    // These do not certify Unity collider/floor/ray eligibility; real-scene fixtures remain necessary.
    public static class PanicAutoFocusChecksV510
    {
        public static void Run(Action<string, bool, string> check)
        {
            var frontal = new PanicAutoFocusCandidateV510("front", 1.8f, .9f, true);
            var nearerBehind = new PanicAutoFocusCandidateV510("behind", .2f, -.9f, true);
            var nearerBlocked = new PanicAutoFocusCandidateV510("wall-blocked", .1f, 1f, false);
            var nearerFront = new PanicAutoFocusCandidateV510("near-front", .6f, .3f, true);
            Check(check, "front-excludes-nearer-behind", PanicAutoFocusPolicyV510.ChooseNearestFacing(new[] { nearerBehind, frontal }) == "front");
            Check(check, "nearest-frontal-before-alignment", PanicAutoFocusPolicyV510.ChooseNearestFacing(new[] { frontal, nearerFront }) == "near-front");
            Check(check, "occluded-ineligible-excluded", PanicAutoFocusPolicyV510.ChooseNearestFacing(new[] { nearerBlocked, frontal }) == "front");
            Check(check, "behind-only-clears-auto", PanicAutoFocusPolicyV510.ChooseNearestFacing(new[] { nearerBehind }) == null);
            Check(check, "empty-clears-auto", PanicAutoFocusPolicyV510.ChooseNearestFacing(new PanicAutoFocusCandidateV510[0]) == null);
            var a = new PanicAutoFocusCandidateV510("a", 1, .7f, true);
            var b = new PanicAutoFocusCandidateV510("b", 1, .7f, true);
            Check(check, "tie-stable-ordinal", PanicAutoFocusPolicyV510.ChooseNearestFacing(new[] { b, a }) == "a" &&
                PanicAutoFocusPolicyV510.ChooseNearestFacing(new[] { a, b }) == "a");
            Check(check, "equal-distance-facing-tie", PanicAutoFocusPolicyV510.ChooseNearestFacing(new[] { a, new PanicAutoFocusCandidateV510("aligned", 1, .99f, true) }) == "aligned");
            Check(check, "invalid-metrics-excluded", PanicAutoFocusPolicyV510.ChooseNearestFacing(new[] {
                new PanicAutoFocusCandidateV510("nan", float.NaN, 1, true),
                new PanicAutoFocusCandidateV510("infinity", .1f, float.PositiveInfinity, true), frontal }) == "front");
            Check(check, "manual-held-at-rest", PanicAutoFocusPolicyV510.KeepManual(true, true, true, 0, 1, 1));
            Check(check, "manual-idle-jitter-held", PanicAutoFocusPolicyV510.KeepManual(true, true, true, .00001f, .99999f, 1));
            Check(check, "manual-move-invalidates", !PanicAutoFocusPolicyV510.KeepManual(true, true, true, .01f, 1, 1));
            Check(check, "manual-turn-invalidates", !PanicAutoFocusPolicyV510.KeepManual(true, true, true, 0, .98f, 1));
            Check(check, "manual-look-invalidates", !PanicAutoFocusPolicyV510.KeepManual(true, true, true, 0, 1, .98f));
            Check(check, "manual-context-invalidates", !PanicAutoFocusPolicyV510.KeepManual(true, false, true, 0, 1, 1));
            Check(check, "manual-occlusion-invalidates", !PanicAutoFocusPolicyV510.KeepManual(true, true, false, 0, 1, 1));
            Check(check, "manual-unarmed-does-not-pin", !PanicAutoFocusPolicyV510.KeepManual(false, true, true, 0, 1, 1));
        }
        static void Check(Action<string, bool, string> check, string id, bool passed)
        { check("v510.focus.policy." + id, passed, "Pure ranking/latch policy; physical eligibility remains world-owned."); }
    }
}
