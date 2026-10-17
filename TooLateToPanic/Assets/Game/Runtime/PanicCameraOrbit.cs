using UnityEngine;

namespace TooLateToPanic
{
    public readonly struct PanicCameraOrbitInputResult
    {
        public bool Click { get; }
        public Vector2 ClickPosition { get; }
        public Vector2 OrbitDelta { get; }
        public PanicCameraOrbitInputResult(bool click, Vector2 clickPosition, Vector2 orbitDelta)
        { Click = click; ClickPosition = clickPosition; OrbitDelta = orbitDelta; }
    }

    /// <summary>Single right-button owner: a release is either a short click or an orbit, never both.</summary>
    public sealed class PanicCameraOrbitGesture
    {
        public const float DragThresholdPixels = 8f;
        public const float HoldThresholdSeconds = .18f;
        public bool Pressed { get; private set; }
        public bool Orbiting { get; private set; }
        public float HoldSeconds { get; private set; }
        Vector2 pressPosition, lastPosition;

        public PanicCameraOrbitInputResult Step(Vector2 pointer, bool pressed, bool held, bool released, bool blocked, float unscaledDeltaTime)
        {
            if (blocked || !Finite(pointer.x) || !Finite(pointer.y) || !Finite(unscaledDeltaTime) || unscaledDeltaTime < 0)
            { Cancel(); return default; }
            if (pressed)
            {
                Cancel(); Pressed = true; pressPosition = lastPosition = pointer;
                // The frame's delta precedes the observed press; do not count it as hold time.
                if (!released) return default;
            }
            if (!Pressed) return default;
            if (!held && !released) { Cancel(); return default; }
            if (!pressed) HoldSeconds += unscaledDeltaTime;
            bool wasOrbiting = Orbiting;
            if ((pointer - pressPosition).sqrMagnitude >= DragThresholdPixels * DragThresholdPixels || HoldSeconds >= HoldThresholdSeconds)
                Orbiting = true;
            // Preserve initial pixels once when crossing the threshold. Discarding them
            // would make a slow sequence of small frame deltas lose more yaw than 30fps.
            Vector2 delta = Orbiting ? pointer - (wasOrbiting ? lastPosition : pressPosition) : Vector2.zero;
            lastPosition = pointer;
            if (released)
            {
                var result = new PanicCameraOrbitInputResult(!Orbiting, pressPosition, delta);
                Cancel(); return result;
            }
            return new PanicCameraOrbitInputResult(false, pressPosition, delta);
        }

        public void Cancel() { Pressed = Orbiting = false; HoldSeconds = 0; pressPosition = lastPosition = Vector2.zero; }
        static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    /// <summary>Pixel deltas integrate once; exponential smoothing alone uses delta time.</summary>
    public sealed class PanicCameraOrbitAngles
    {
        public const float MinPitch = 28f;
        public const float MaxPitch = 65f;
        public const float YawDegreesPerPixel = .18f;
        public const float PitchDegreesPerPixel = .12f;
        public float TargetYaw { get; private set; }
        public float TargetPitch { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public PanicCameraOrbitAngles(float basePitch = 35f) { Reset(basePitch); }
        public void Reset(float basePitch)
        {
            TargetYaw = Yaw = 0;
            TargetPitch = Pitch = Mathf.Clamp(Finite(basePitch) ? basePitch : 35f, MinPitch, MaxPitch);
        }
        public void AddPixels(Vector2 delta)
        {
            if (!Finite(delta.x) || !Finite(delta.y)) return;
            TargetYaw = Mathf.Repeat(TargetYaw + delta.x * YawDegreesPerPixel + 180f, 360f) - 180f;
            TargetPitch = Mathf.Clamp(TargetPitch - delta.y * PitchDegreesPerPixel, MinPitch, MaxPitch);
        }
        public void Tick(float unscaledDeltaTime)
        {
            if (!Finite(unscaledDeltaTime) || unscaledDeltaTime <= 0) return;
            float blend = 1f - Mathf.Exp(-12f * unscaledDeltaTime);
            Yaw = Mathf.LerpAngle(Yaw, TargetYaw, blend);
            Pitch = Mathf.Lerp(Pitch, TargetPitch, blend);
        }
        public Vector3 RotateOffset(Vector3 original)
        {
            float length = original.magnitude;
            if (!Finite(length) || length < .001f) return original;
            float yaw = Mathf.Atan2(original.x, original.z) + Yaw * Mathf.Deg2Rad;
            float pitch = Pitch * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(yaw) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Cos(yaw) * Mathf.Cos(pitch)) * length;
        }
        static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    public sealed partial class PanicHomeWorld
    {
        readonly PanicCameraOrbitGesture cameraOrbitGesture = new PanicCameraOrbitGesture();
        readonly PanicCameraOrbitAngles cameraOrbitAngles = new PanicCameraOrbitAngles();
        public bool CameraOrbitDragging { get { return cameraOrbitGesture.Orbiting; } }
        public bool CameraOrbitPressed { get { return cameraOrbitGesture.Pressed; } }
        public float CameraOrbitYaw { get { return cameraOrbitAngles.Yaw; } }
        public float CameraOrbitPitch { get { return cameraOrbitAngles.Pitch; } }

        bool CameraOrbitInputAllowed
        {
            get
            {
                if (Model == null || !View || InputLocked || Model.IsPaused || NotebookBlocksInput || BagCameraOwnsView || WorkActive || InventoryOpen || IsMoving ||
                    CinematicActive || LossAnnounced || !JuryAllowsMovement || CancelFrame == Time.frameCount) return false;
                if (hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame)) return false;
                return Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial || Model.Phase == PanicPhase.Inspection;
            }
        }

        // Call once at the end of Update input handling, before FollowCamera. Doing so
        // observes Tab/pause/work changes and the existing right-down placement cancel.
        void UpdateCameraOrbitInput(bool pointerOverUi)
        {
            if (!Application.isFocused) { cameraOrbitGesture.Cancel(); return; }
            StepCameraOrbitInput(Input.mousePosition, Input.GetMouseButtonDown(1), Input.GetMouseButton(1),
                Input.GetMouseButtonUp(1), pointerOverUi, Time.unscaledDeltaTime);
        }

        /// <summary>Opt-in semantic input path; uses the same production gates and real placement call.</summary>
        public PanicCameraOrbitInputResult StepCameraOrbitInput(Vector2 pointer, bool pressed, bool held, bool released, bool pointerOverUi, float unscaledDeltaTime)
        {
            bool blocked = pointerOverUi || !CameraOrbitInputAllowed || !View || !View.pixelRect.Contains(pointer);
            var result = cameraOrbitGesture.Step(pointer, pressed, held, released, blocked, unscaledDeltaTime);
            cameraOrbitAngles.AddPixels(result.OrbitDelta);
            if (result.Click) BeginMousePlacement(result.ClickPosition, false);
            return result;
        }

        // Call after `Vector3 offset = cameraOffset;` in FollowCamera. Orthographic mode,
        // size, actor position and the main camera-relative movement code stay owned by World.
        void ApplyCameraOrbit(ref Vector3 offset)
        {
            if (Model == null || Model.Phase == PanicPhase.Title || CinematicActive || LossAnnounced) return;
            if (Model.Phase != PanicPhase.Preparing && Model.Phase != PanicPhase.Tutorial && Model.Phase != PanicPhase.Inspection) return;
            cameraOrbitAngles.Tick(Time.unscaledDeltaTime);
            offset = cameraOrbitAngles.RotateOffset(offset);
        }

        void ResetCameraOrbit()
        {
            cameraOrbitGesture.Cancel();
            float flat = new Vector2(cameraOffset.x, cameraOffset.z).magnitude;
            cameraOrbitAngles.Reset(Mathf.Atan2(cameraOffset.y, flat) * Mathf.Rad2Deg);
        }
    }
}
