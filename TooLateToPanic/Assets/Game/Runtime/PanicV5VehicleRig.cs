using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Scripted-response art adapter. Never owns a route, Model state, input or actor animation.</summary>
    public sealed class PanicV5VehicleRig : MonoBehaviour
    {
        public Transform[] Wheels = new Transform[0];
        public Transform[] Doors = new Transform[0];
        public float[] DoorAngles = new float[0];
        public Renderer[] FlashRenderers = new Renderer[0];
        public Light[] FlashLights = new Light[0];
        public Color[] FlashColors = new Color[0];
        public Transform Propeller;
        public Transform SteeringWheel;
        public bool IsBoat;
        public float WheelRadius = .36f;
        public float SpeedMetresPerSecond { get; private set; }
        public float DoorOpenAmount { get; private set; }
        public string ServiceId;
        MaterialPropertyBlock block;
        Quaternion[] closedDoors;
        float wheelAngle, propellerAngle;

        public void Sample(float speedMetresPerSecond, float dt, float responseClock, bool paused)
        {
            if(block==null)block=new MaterialPropertyBlock();
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0) dt = 0;
            dt = Mathf.Min(.1f, dt);
            SpeedMetresPerSecond = paused ? 0 : Mathf.Clamp(Finite(speedMetresPerSecond), 0, 20);
            if (!paused)
            {
                wheelAngle = Mathf.Repeat(wheelAngle + SpeedMetresPerSecond * dt / Mathf.Max(.08f, WheelRadius) * Mathf.Rad2Deg, 360);
                foreach (var wheel in Wheels) if (wheel) wheel.localRotation = Quaternion.Euler(wheelAngle, 0, 0);
                propellerAngle = Mathf.Repeat(propellerAngle + dt * (SpeedMetresPerSecond > .03f ? 1100f + SpeedMetresPerSecond * 190f : 0), 360);
                if (Propeller) Propeller.localRotation = Quaternion.Euler(0, 0, propellerAngle);
            }
            float clock = Finite(responseClock);
            for (int i = 0; i < FlashRenderers.Length; i++)
            {
                // Two short alternating pulses with a dark interval; phase clock is frozen by the route owner.
                float p = Mathf.Repeat(clock * 1.75f + (i % 2) * .5f, 1);
                float pulse = p < .13f || p > .20f && p < .32f ? 1f : .08f;
                Color color = i < FlashColors.Length ? FlashColors[i] : new Color(.08f, .32f, 1);
                var renderer = FlashRenderers[i];
                if (renderer)
                {
                    block.Clear(); block.SetColor("_Color", color * Mathf.Lerp(.38f, 1f, pulse));
                    block.SetColor("_EmissionColor", color * (pulse * 3.2f)); renderer.SetPropertyBlock(block);
                }
                if (i < FlashLights.Length && FlashLights[i]) FlashLights[i].intensity = pulse * .72f;
            }
        }

        public void SetDoorsOpen(float amount)
        {
            DoorOpenAmount = Mathf.Clamp01(Finite(amount));
            if (closedDoors == null || closedDoors.Length != Doors.Length)
            {
                closedDoors = new Quaternion[Doors.Length];
                for (int i = 0; i < Doors.Length; i++) closedDoors[i] = Doors[i] ? Doors[i].localRotation : Quaternion.identity;
            }
            for (int i = 0; i < Doors.Length; i++) if (Doors[i])
                Doors[i].localRotation = closedDoors[i] * Quaternion.Euler(0, (i < DoorAngles.Length ? DoorAngles[i] : 85f) * DoorOpenAmount, 0);
        }

        public static float FloodSurface(float time)
        { return -.08f + Mathf.Sin(Finite(time) * 1.8f) * .025f; }
        static float Finite(float n) { return float.IsNaN(n) || float.IsInfinity(n) ? 0 : n; }
        void OnDisable() { foreach (var light in FlashLights) if (light) light.intensity = 0; SpeedMetresPerSecond = 0; }
    }

    /// <summary>Owns only resources produced by the V5 runtime factories; never destroys shared prefabs/palette.</summary>
    public sealed class PanicV5AssetOwner : MonoBehaviour
    {
        public readonly List<Object> Owned = new List<Object>();
        public void CaptureGeneratedMeshes()
        {
            foreach (var f in GetComponentsInChildren<MeshFilter>(true))
                if (f.sharedMesh && f.sharedMesh.name.StartsWith("HomeV2_") && !Owned.Contains(f.sharedMesh)) Owned.Add(f.sharedMesh);
        }
        void OnDestroy()
        {
            foreach (var o in Owned) if (o) { if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }
            Owned.Clear();
        }
    }
}
