using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    [Serializable] public sealed class PanicCameraOrbitCheck { public string name; public bool pass; public PanicCameraOrbitCheck(string name, bool pass) { this.name = name; this.pass = pass; } }
    /// <summary>Pure semantic fixtures; no OS mouse, actual world ray/collision or native visual claims.</summary>
    public static class PanicCameraOrbitChecks
    {
        public static List<PanicCameraOrbitCheck> Run()
        {
            var checks = new List<PanicCameraOrbitCheck>();
            Action<string, bool> check = (name, pass) => checks.Add(new PanicCameraOrbitCheck(name, pass));
            var gesture = new PanicCameraOrbitGesture(); var origin = new Vector2(200, 200);
            gesture.Step(origin, true, true, false, false, .016f);
            var tap = gesture.Step(origin, false, false, true, false, .05f);
            check("short release is one click at press position", tap.Click && tap.ClickPosition == origin && tap.OrbitDelta == Vector2.zero && !gesture.Pressed);
            check("duplicate release cannot click", !gesture.Step(origin, false, false, true, false, .016f).Click);
            gesture.Step(origin, true, true, false, false, .016f);
            var drag = gesture.Step(origin + new Vector2(12, 0), false, true, false, false, .016f);
            var dragRelease = gesture.Step(origin + new Vector2(14, 0), false, false, true, false, .016f);
            check("drag preserves initial and final pixels without click", drag.OrbitDelta.x + dragRelease.OrbitDelta.x == 14f && !drag.Click && !dragRelease.Click);
            gesture.Step(origin, true, true, false, false, .016f);
            gesture.Step(origin, false, true, false, false, .2f);
            check("stationary long hold cannot pick up on release", !gesture.Step(origin, false, false, true, false, .01f).Click);
            gesture.Step(origin, true, true, false, true, .016f);
            check("UI blocked press never arms", !gesture.Pressed);
            gesture.Step(origin, true, true, false, false, .016f);
            gesture.Step(origin, false, true, false, true, .016f);
            check("new gate mid-gesture prevents late click", !gesture.Step(origin, false, false, true, false, .016f).Click);
            gesture.Step(origin, true, true, false, false, .016f);
            gesture.Step(origin, false, true, false, false, float.NaN);
            check("invalid time cancels safely", !gesture.Pressed);
            check("30/60/120fps integrate same full pixel travel", Mathf.Abs(Travel(30) - 720f) < .01f && Mathf.Abs(Travel(60) - 720f) < .01f && Mathf.Abs(Travel(120) - 720f) < .01f);
            var pose = new PanicCameraOrbitAngles(35f);
            pose.AddPixels(new Vector2(100, -10000)); pose.Tick(1f);
            check("pixel yaw has no delta-time multiplier", Mathf.Abs(pose.TargetYaw - 18f) < .001f);
            check("pitch clamps at upper bound", pose.TargetPitch == PanicCameraOrbitAngles.MaxPitch);
            pose.AddPixels(new Vector2(0, 10000));
            check("pitch clamps at lower bound", pose.TargetPitch == PanicCameraOrbitAngles.MinPitch);
            float before = pose.TargetYaw; pose.AddPixels(new Vector2(float.NaN, 1));
            check("invalid pointer delta cannot poison pose", pose.TargetYaw == before);
            var offset = new Vector3(11, 13, -15);
            check("orbit preserves camera offset distance", Mathf.Abs(pose.RotateOffset(offset).magnitude - offset.magnitude) < .001f);
            float yaw30 = SmoothYaw(30), yaw120 = SmoothYaw(120);
            check("smoothing is frame-rate independent", Mathf.Abs(Mathf.DeltaAngle(yaw30, yaw120)) < .01f);
            pose.Reset(35f); gesture.Cancel();
            check("reset clears gesture and pose", !gesture.Pressed && !gesture.Orbiting && pose.TargetYaw == 0 && pose.Yaw == 0 && pose.Pitch == 35f);
            return checks;
        }
        static float Travel(int fps)
        {
            var gesture = new PanicCameraOrbitGesture();
            gesture.Step(Vector2.zero, true, true, false, false, 1f / fps);
            float sum = 0;
            for (int i = 1; i <= fps; i++) sum += gesture.Step(new Vector2(720f * i / fps, 0), false, true, false, false, 1f / fps).OrbitDelta.x;
            sum += gesture.Step(new Vector2(720, 0), false, false, true, false, 0).OrbitDelta.x;
            return sum;
        }
        static float SmoothYaw(int fps)
        {
            var pose = new PanicCameraOrbitAngles(); pose.AddPixels(new Vector2(500, 0));
            for (int i = 0; i < fps; i++) pose.Tick(1f / fps);
            return pose.Yaw;
        }
    }
}
