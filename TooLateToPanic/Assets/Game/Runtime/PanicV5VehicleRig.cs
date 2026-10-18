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
        public Transform OutboardPivot;
        public Transform SteeringWheel;
        public Transform LadderBasePivot;
        public Transform LadderExtendPivot;
        public Transform RoofMonitorPivot;
        public Transform WinchPivot;
        public Transform CraneBoomPivot;
        public Light[] Headlights = new Light[0];
        public Renderer[] HeadlightRenderers = new Renderer[0];
        public Light[] Taillights = new Light[0];
        public Renderer[] TaillightRenderers = new Renderer[0];
        public Light InteriorLight;
        public Renderer InteriorLightRenderer;
        public bool IsBoat;
        public float WheelRadius = .36f;
        public float SpeedMetresPerSecond { get; private set; }
        public float DoorOpenAmount { get; private set; }
        public float LadderDeployAmount { get; private set; }
        public string ServiceId;
        MaterialPropertyBlock block;
        Quaternion[] closedDoors;
        Quaternion ladderBaseRest, craneRest, outboardRest, steeringRest;
        Vector3 ladderExtendRest;
        bool restsCached;
        float wheelAngle, propellerAngle, winchAngle;

        void CacheRests()
        {
            if (restsCached) return;
            restsCached = true;
            if (LadderBasePivot) ladderBaseRest = LadderBasePivot.localRotation;
            if (LadderExtendPivot) ladderExtendRest = LadderExtendPivot.localPosition;
            if (CraneBoomPivot) craneRest = CraneBoomPivot.localRotation;
            if (OutboardPivot) outboardRest = OutboardPivot.localRotation;
            if (SteeringWheel) steeringRest = SteeringWheel.localRotation;
        }

        public void Sample(float speedMetresPerSecond, float dt, float responseClock, bool paused)
        {
            CacheRests();
            if(block==null)block=new MaterialPropertyBlock();
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0) dt = 0;
            dt = Mathf.Min(.1f, dt);
            SpeedMetresPerSecond = paused ? 0 : Mathf.Clamp(Finite(speedMetresPerSecond), 0, 20);
            float clock = Finite(responseClock);
            if (!paused)
            {
                wheelAngle = Mathf.Repeat(wheelAngle + SpeedMetresPerSecond * dt / Mathf.Max(.08f, WheelRadius) * Mathf.Rad2Deg, 360);
                foreach (var wheel in Wheels) if (wheel) wheel.localRotation = Quaternion.Euler(wheelAngle, 0, 0);
                propellerAngle = Mathf.Repeat(propellerAngle + dt * (SpeedMetresPerSecond > .03f ? 1100f + SpeedMetresPerSecond * 190f : 0), 360);
                if (Propeller) Propeller.localRotation = Quaternion.Euler(0, 0, propellerAngle);
                if (OutboardPivot)
                {
                    float rudder = Mathf.Sin(clock * 1.6f) * (SpeedMetresPerSecond > .05f ? 9f : 2.5f);
                    OutboardPivot.localRotation = outboardRest * Quaternion.Euler(0, rudder, 0);
                }
                if (SteeringWheel)
                {
                    float steer = Mathf.Sin(clock * 1.35f) * (SpeedMetresPerSecond > .05f ? 16f : 0f);
                    SteeringWheel.localRotation = steeringRest * Quaternion.Euler(0, 0, steer);
                }
                bool stationed = DoorOpenAmount > .05f || (SpeedMetresPerSecond < .2f && clock > 2.2f);
                LadderDeployAmount = Mathf.MoveTowards(LadderDeployAmount, stationed ? 1f : 0f, dt * .85f);
                float deploySmooth = LadderDeployAmount * LadderDeployAmount * (3f - 2f * LadderDeployAmount);
                if (LadderBasePivot)
                    LadderBasePivot.localRotation = ladderBaseRest * Quaternion.Euler(-16f * deploySmooth, Mathf.Sin(clock * .45f) * 6f * deploySmooth, 0);
                if (LadderExtendPivot)
                    LadderExtendPivot.localPosition = ladderExtendRest + new Vector3(0, 0, .82f * deploySmooth);
                if (WinchPivot)
                {
                    winchAngle = Mathf.Repeat(winchAngle + dt * (stationed ? 48f : 0f), 360f);
                    WinchPivot.localRotation = Quaternion.Euler(winchAngle, 0, 0);
                }
                if (CraneBoomPivot)
                    CraneBoomPivot.localRotation = craneRest * Quaternion.Euler(-12f * deploySmooth, 0, 0);
            }
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
            bool braking = SpeedMetresPerSecond < .25f;
            if (Headlights != null) foreach (var hl in Headlights) if (hl) { hl.enabled = true; hl.intensity = .88f; }
            if (Taillights != null) foreach (var tl in Taillights) if (tl) { tl.enabled = true; tl.intensity = braking ? .62f : .32f; }
            UpdateInteriorLight();
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
            UpdateInteriorLight();
        }

        void UpdateInteriorLight()
        {
            bool open = DoorOpenAmount > .02f;
            if (InteriorLight)
            {
                InteriorLight.enabled = open;
                InteriorLight.intensity = open ? .95f * DoorOpenAmount : 0f;
            }
            if (InteriorLightRenderer)
            {
                if (block == null) block = new MaterialPropertyBlock();
                block.Clear();
                Color warm = new Color(1f, .90f, .68f);
                block.SetColor("_Color", warm * Mathf.Lerp(.35f, 1f, DoorOpenAmount));
                block.SetColor("_EmissionColor", open ? warm * (1.8f * DoorOpenAmount) : Color.black);
                InteriorLightRenderer.SetPropertyBlock(block);
            }
        }

        public static float FloodSurface(float time)
        { return -.08f + Mathf.Sin(Finite(time) * 1.8f) * .025f; }
        static float Finite(float n) { return float.IsNaN(n) || float.IsInfinity(n) ? 0 : n; }
        void OnDisable()
        {
            foreach (var light in FlashLights) if (light) light.intensity = 0;
            if (Headlights != null) foreach (var light in Headlights) if (light) light.intensity = 0;
            if (Taillights != null) foreach (var light in Taillights) if (light) light.intensity = 0;
            if (InteriorLight) InteriorLight.intensity = 0;
            SpeedMetresPerSecond = 0;
        }
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
