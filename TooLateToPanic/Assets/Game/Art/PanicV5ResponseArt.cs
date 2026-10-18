using System;
using System.Collections.Generic;
using UnityEngine;
using B = TooLateToPanic.PanicHomeArt.Builder;
using M = TooLateToPanic.PanicHomeArt.M;
using Service = TooLateToPanic.PanicStreetLifeArtV414.Service;

namespace TooLateToPanic
{
    /// <summary>Original local V5 response assets. Metres, Y up, +Z vehicle front. No downloaded geometry.</summary>
    public static class PanicV5ResponseArt
    {
        static Vector3 V(float x, float y, float z) { return new Vector3(x, y, z); }
        static GameObject Group(GameObject parent, string name, Vector3 position)
        { var g = new GameObject(name); g.transform.SetParent(parent.transform, false); g.transform.localPosition = position; return g; }

        public static GameObject CreateVehicle(Service service)
        {
            PanicHomeArt.EnsurePalette();
            var root = new GameObject("V5 " + service + " detailed response vehicle");
            var owner = root.AddComponent<PanicV5AssetOwner>(); var rig = root.AddComponent<PanicV5VehicleRig>();
            rig.ServiceId = service.ToString();
            bool police = service == Service.Police, fire = service == Service.Fire;
            float length = police ? 4.52f : fire ? 6.3f : service == Service.Rescue ? 5.5f : 5.55f;
            float width = police ? 1.86f : fire ? 2.16f : 2.02f;
            float cabZ = police ? .12f : length * .29f;
            float roof = police ? 1.60f : fire ? 2.35f : service == Service.Ambulance ? 2.55f : 2.20f;
            M body = fire ? M.Red : service == Service.Rescue ? M.SafetyAmber : M.Linen;
            var b = new B();
            b.Box(V(0, .43f, 0), V(width * .90f, .26f, length * .90f), M.Charcoal, .06f);
            b.Box(V(0, .75f, 0), V(width, .57f, length - .14f), body, .09f);
            b.Box(V(0, .42f, length * .5f), V(width + .10f, .16f, .21f), M.Steel, .04f);
            b.Box(V(0, .43f, -length * .5f), V(width + .08f, .16f, .19f), M.Charcoal, .035f);
            // Independent raised cab, shaped windscreen and bonnet preserve a readable front at isometric distance.
            if (police)
            {
                b.Box(V(0, 1.04f, .05f), V(1.75f, .25f, 2.7f), M.Charcoal, .055f);
                b.Box(V(0, 1.53f, .05f), V(1.46f, .14f, 1.80f), M.Charcoal, .05f);
                b.Quad(V(-.73f, 1.48f, .95f), V(.73f, 1.48f, .95f), V(.85f, 1.04f, 1.35f), V(-.85f, 1.04f, 1.35f), M.Glass);
                b.Quad(V(.73f, 1.48f, -.85f), V(-.73f, 1.48f, -.85f), V(-.85f, 1.04f, -1.32f), V(.85f, 1.04f, -1.32f), M.Glass);
                b.Box(V(0, .99f, 1.66f), V(1.78f, .15f, 1.10f), M.Charcoal, .045f, Quaternion.Euler(-4, 0, 0));
                b.Box(V(0, 1.00f, -1.71f), V(1.76f, .15f, .94f), M.Charcoal, .05f);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Quad(V(s * .88f, 1.09f, -1.05f), V(s * .88f, 1.09f, -.08f), V(s * .73f, 1.49f, -.08f), V(s * .73f, 1.49f, -.72f), M.Glass);
                    b.Box(V(s * .80f, 1.30f, -.05f), V(.075f, .45f, .075f), M.Charcoal, .012f);
                    for (int c = 0; c < 12; c++) b.Box(V(s * .941f, .80f + (c % 2) * .08f, -1.4f + c * .22f), V(.012f, .065f, .15f), M.Slate, .002f);
                }
                b.Beam(V(.57f, 1.62f, -.62f), V(.57f, 2.14f, -.70f), .016f, M.Black);
                b.Box(V(0, .72f, 2.31f), V(1.3f, .30f, .10f), M.Charcoal, .03f);
                for (int s = -1; s <= 1; s += 2) b.Beam(V(s * .58f, .60f, 2.32f), V(s * .58f, .98f, 2.29f), .06f, M.Charcoal);
            }
            else
            {
                b.Box(V(0, 1.13f, cabZ), V(width - .18f, .30f, 1.68f), body, .05f);
                b.Box(V(0, 1.66f, cabZ-.79f), V(width-.20f,.94f,.075f),body,.02f);
                foreach(int side in new[]{-1,1})b.Box(V(side*width*.44f,1.71f,cabZ+.78f),V(.075f,.75f,.095f),body,.012f);
                b.Box(V(0, 2.12f, cabZ), V(width - .07f, .14f, 1.80f), body, .04f);
                b.Quad(V(-width * .40f, 2.04f, cabZ + .84f), V(width * .40f, 2.04f, cabZ + .84f), V(width * .43f, 1.47f, cabZ + .96f), V(-width * .43f, 1.47f, cabZ + .96f), M.Glass);
                b.Box(V(0, 1.22f, length * .5f - .22f), V(width - .06f, .35f, .46f), body, .06f);
                b.Box(V(0, .89f, length * .5f + .02f), V(width * .58f, .29f, .035f), M.Charcoal, .015f);
                for (int row = 0; row < 4; row++) b.Box(V(0, .81f + row * .065f, length * .5f + .047f), V(width * .50f, .014f, .018f), M.Steel, .002f);
                foreach (int side in new[] { -1, 1 })
                {
                    b.Box(V(side * (width * .5f + .03f), .64f, cabZ), V(.18f, .12f, 1.26f), M.Steel, .015f);
                    b.Beam(V(side * width * .47f, 1.81f, cabZ + .58f), V(side * (width * .5f + .22f), 1.81f, cabZ + .58f), .034f, M.Charcoal);
                    b.Box(V(side * (width * .5f + .22f), 1.86f, cabZ + .56f), V(.10f, .28f, .20f), M.Charcoal, .025f);
                }
            }
            // Visible cabin fittings sit behind real glazing, rather than glazing on a solid cab block.
            float seatHeight=police?.91f:1.10f;
            foreach(int side in new[]{-1,1})
            {
                b.Box(V(side*.43f,seatHeight,cabZ-.26f),V(.47f,.18f,.49f),M.Charcoal,.06f);
                b.Box(V(side*.43f,seatHeight+.28f,cabZ-.48f),V(.45f,.52f,.15f),M.Charcoal,.05f,Quaternion.Euler(-8,0,0));
                b.Box(V(side*.43f,seatHeight+.60f,cabZ-.48f),V(.30f,.18f,.12f),M.Charcoal,.04f);
            }
            b.Box(V(0,police?1.06f:1.44f,cabZ+.56f),V(width*.76f,.18f,.24f),M.Charcoal,.04f);
            var steering=Group(root,"V5 SteeringWheel",V(-.43f,police?1.18f:1.57f,cabZ+.38f));var sw=new B();
            sw.Torus(Vector3.zero,.135f,.019f,M.Black,Quaternion.Euler(52,0,0));
            sw.Beam(V(-.11f,0,0),V(.11f,0,0),.021f,M.Charcoal);sw.Attach(steering,"SteeringRimSpoke");rig.SteeringWheel=steering.transform;
            // Axles, curved guards and side running boards are visible below the service body.
            var wheels = new List<Transform>(); float axle = length * (police ? .31f : .315f);
            for (int s = -1; s <= 1; s += 2) foreach (float z in new[] { -axle, axle })
            {
                var wheel = Group(root, "Response wheel", V(s * (width * .5f + .015f), .38f, z)); var wb = new B();
                wb.Cylinder(Vector3.zero, .37f, .25f, M.Black, 24, Quaternion.Euler(0, 0, 90));
                wb.Cylinder(V(s * .018f, 0, 0), .235f, .26f, M.Steel, 20, Quaternion.Euler(0, 0, 90));
                wb.Cylinder(V(s * .15f, 0, 0), .085f, .025f, M.Charcoal, 12, Quaternion.Euler(0, 0, 90));
                for (int n = 0; n < 5; n++) { float a = n * Mathf.PI * 2 / 5; wb.Cylinder(V(s * .151f, Mathf.Sin(a) * .145f, Mathf.Cos(a) * .145f), .026f, .028f, M.Charcoal, 8, Quaternion.Euler(0, 0, 90)); }
                wb.Attach(wheel, "TyreSidewallFiveLugHub"); wheels.Add(wheel.transform);
                var wheelCollision=wheel.AddComponent<BoxCollider>();wheelCollision.size=V(.24f,.66f,.66f);
                b.Box(V(s * width * .5f, .74f, z), V(.15f, .10f, .98f), M.Charcoal, .025f);
            }
            foreach (float z in new[] { -axle, axle }) b.Cylinder(V(0, .38f, z), .075f, width, M.Charcoal, 10, Quaternion.Euler(0, 0, 90));
            rig.Wheels = wheels.ToArray();
            var doors = new List<Transform>(); var angles = new List<float>();
            AddCabDoors(root, b, doors, angles, width, cabZ, police, body);
            if (service == Service.Ambulance) Ambulance(root, b, rig, doors, angles, width, length);
            if (fire) FireEngine(root, b, rig, width, length);
            if (service == Service.Rescue) RescueTruck(root, b, rig, width);
            // Headlight/taillight glass remains separate from the red/blue warning lamps.
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(V(s * width * .37f, .99f, length * .5f + .032f), V(.37f, .19f, .045f), M.WarmWindow, .025f);
                b.Box(V(s * width * .39f, .89f, -length * .5f - .018f), V(.22f, .29f, .045f), M.Red, .015f);
                b.Box(V(s * width * .39f, .68f, -length * .5f - .042f), V(.20f, .055f, .02f), M.SafetyAmber, .004f);
                b.Box(V(s * width * .502f, .66f, -.60f), V(.02f, .08f, .24f), M.SafetyAmber, .006f);
            }
            b.Box(V(0, .55f, length * .5f + .045f), V(.48f, .11f, .025f), M.Linen, .01f);
            b.Box(V(0, .54f, -length * .5f - .055f), V(.48f, .11f, .025f), M.Linen, .01f);
            b.Attach(root, "V5BodyCabSeamsServiceEquipment");
            rig.Doors = doors.ToArray(); rig.DoorAngles = angles.ToArray();
            AddLightbar(root, rig, police ? 1.68f : 2.31f, cabZ, service == Service.Rescue);
            AddHeadlightsAndTaillights(root, rig, width, length);
            BodyCollider(root, V(0, .79f, 0), V(width, .68f, length - .12f));
            BodyCollider(root, V(0, police ? 1.33f : 1.68f, cabZ), V(width - .18f, police ? .50f : 1.12f, police ? 2.18f : 1.70f));
            if (!police) BodyCollider(root, V(0, (1.1f + roof) * .5f, fire?-1.02f:service==Service.Ambulance?-.95f:-.88f), V(width - .10f, roof - 1.1f, fire?4.12f:service==Service.Ambulance?3.55f:2.98f));
            var rb = root.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false; rb.mass = police ? 1800 : fire ? 11000 : 3600;
            rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Refinish(root, owner, service); owner.CaptureGeneratedMeshes(); return root;
        }

        static void AddCabDoors(GameObject root, B bodyBuilder, List<Transform> hinges, List<float> angles, float width, float cabZ, bool police, M body)
        {
            foreach (int s in new[] { -1, 1 })
            {
                float z = police ? .99f : cabZ + .58f;
                var pivot = Group(root, "V5 CabDoorHinge " + s, V(s * width * .5f, police ? 1.04f : 1.44f, z)); var d = new B();
                d.Box(V(0, -.14f, -.44f), V(.055f, police ? .49f : .75f, .86f), body, .018f);
                d.Box(V(0, police?.26f:.35f, -.40f), V(.047f, police?.37f:.42f, .77f), M.Glass, .01f);
                d.Box(V(s * .042f, -.005f, -.68f), V(.033f, .042f, .19f), M.Charcoal, .009f);
                d.Box(V(s * .037f, -.19f, -.43f), V(.017f, .08f, .70f), M.SafetyAmber, .003f);
                d.Attach(pivot, "OpeningCabDoorHandleStripe"); hinges.Add(pivot.transform); angles.Add(-s * 64f);
                // Root collision owns the cab; articulated door art never sweeps into the playable house.
            }
        }

        static void Ambulance(GameObject root, B b, PanicV5VehicleRig rig, List<Transform> doors, List<float> angles, float w, float length)
        {
            b.Box(V(0, 1.72f, -.95f), V(w, 1.55f, 3.55f), M.Linen, .065f);
            b.Box(V(0, 2.52f, -.95f), V(w + .06f, .12f, 3.64f), M.Linen, .035f);
            // Interior stretcher, oxygen cylinder, and medical cabinets inside the rear compartment
            b.Box(V(-.58f, 1.48f, -1.05f), V(.36f, 1.18f, 2.45f), M.Linen, .015f);
            b.Cylinder(V(-.56f, 1.24f, .25f), .095f, .64f, M.Olive, 12);
            b.Box(V(.16f, .98f, -1.15f), V(.56f, .08f, 1.78f), M.Steel, .012f);
            b.Box(V(.16f, 1.07f, -1.15f), V(.52f, .11f, 1.70f), M.SafetyAmber, .022f);
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(V(s * (w * .5f + .013f), 1.44f, -.82f), V(.02f, .18f, 3.18f), M.Red, .005f);
                b.Box(V(s * (w * .5f + .027f), 1.99f, -.74f), V(.025f, .61f, .16f), M.Red, .004f);
                b.Box(V(s * (w * .5f + .03f), 1.99f, -.74f), V(.03f, .16f, .62f), M.Red, .004f);
                b.Box(V(s * (w * .5f + .022f), 1.98f, .26f), V(.025f, .37f, .64f), M.Glass, .018f);
                var rear = Group(root, "V5 AmbulanceRearDoorHinge " + s, V(s * w * .48f, 1.59f, -length * .5f + .05f)); var d = new B();
                d.Box(V(-s * w * .237f, 0, 0), V(w * .468f, 1.61f, .065f), M.Linen, .025f);
                d.Box(V(-s * w * .237f, .33f, -.04f), V(w * .35f, .46f, .018f), M.Glass, .02f);
                d.Box(V(-s * w * .237f, -.25f, -.04f), V(w * .46f, .17f, .02f), M.Red, .003f);
                d.Box(V(-s * w * .43f, -.03f, -.06f), V(.05f, .17f, .03f), M.Charcoal, .007f);
                d.Attach(rear, "RearWindowDoorHandle"); doors.Add(rear.transform); angles.Add(-s * 96f);
            }
            b.Box(V(0, .49f, -length * .5f - .22f), V(1.52f, .11f, .46f), M.Steel, .02f);
            // Small oxygen canister/service hatch rather than a plain white box side.
            b.Box(V(w * .5f + .028f, 1.08f, -1.74f), V(.035f, .42f, .61f), M.Steel, .018f);
            for (int i = 0; i < 4; i++) b.Box(V(w * .5f + .05f, .97f + i * .075f, -1.74f), V(.015f, .012f, .47f), M.Charcoal, .001f);
            // Rear medical compartment ceiling lamp (illuminates when rear doors open)
            var cabinLamp = Group(root, "V5 AmbulanceCabinLight", V(0, 2.38f, -1.10f));
            var clb = new B(); clb.Box(Vector3.zero, V(.34f, .035f, 1.35f), M.WarmWindow, .006f); clb.Attach(cabinLamp, "AmbulanceCabinLampLens");
            var clLight = cabinLamp.AddComponent<Light>(); clLight.type = LightType.Point; clLight.color = new Color(1f, .92f, .74f);
            clLight.range = 3.6f; clLight.intensity = 0f; clLight.enabled = false; clLight.shadows = LightShadows.None; clLight.renderMode = LightRenderMode.ForceVertex;
            rig.InteriorLight = clLight; rig.InteriorLightRenderer = cabinLamp.GetComponentInChildren<Renderer>();
        }

        static void FireEngine(GameObject root, B b, PanicV5VehicleRig rig, float w, float length)
        {
            b.Box(V(0, 1.48f, -1.02f), V(w, 1.32f, 4.12f), M.Red, .045f);
            foreach (int s in new[] { -1, 1 }) for (int bay = 0; bay < 3; bay++)
            {
                float z = -2.08f + bay * 1.08f;
                b.Box(V(s * (w * .5f + .016f), 1.46f, z), V(.045f, .91f, .94f), M.Steel, .014f);
                for (int slat = 0; slat < 9; slat++) b.Box(V(s * (w * .5f + .043f), 1.08f + slat * .095f, z), V(.017f, .012f, .87f), M.Charcoal, .002f);
                b.Box(V(s * (w * .5f + .065f), 1.12f, z), V(.03f, .055f, .29f), M.Black, .008f);
                b.Box(V(s * (w * .5f + .03f), .88f, z), V(.022f, .09f, 1f), M.Linen, .005f);
            }
            b.Box(V(0, 2.20f, -1.02f), V(w - .11f, .10f, 4.17f), M.Charcoal, .02f);
            // Articulated 2-stage telescopic turntable ladder
            var ladder = Group(root, "V5 TelescopicLadderEquipmentMount", V(0, 2.31f, -.72f)); var l = new B();
            l.Cylinder(V(0, -.04f, -1.35f), .38f, .08f, M.Charcoal, 16);
            foreach (int s in new[] { -1, 1 }) l.Beam(V(s * .33f, 0f, -2.13f), V(s * .33f, 0f, 2.23f), .052f, M.Steel);
            for (int i = 0; i < 17; i++) l.Beam(V(-.33f, 0f, -2.1f + i * .265f), V(.33f, 0f, -2.1f + i * .265f), .032f, M.Steel);
            l.Attach(ladder, "DoubleLadderRailsRungs");
            rig.LadderBasePivot = ladder.transform;

            var ladderExt = Group(ladder, "V5 TelescopicLadderExtension", V(0, .13f, 0)); var le = new B();
            foreach (int s in new[] { -1, 1 })
            {
                le.Beam(V(s * .26f, 0f, -2.05f), V(s * .26f, 0f, 2.15f), .044f, M.Steel);
                le.Box(V(s * .26f, 0f, 2.12f), V(.06f, .08f, .22f), M.SafetyAmber, .008f);
            }
            for (int i = 0; i < 16; i++) le.Beam(V(-.26f, 0f, -1.95f + i * .26f), V(.26f, 0f, -1.95f + i * .26f), .028f, M.Steel);
            le.Attach(ladderExt, "TelescopicExtensionSection");
            rig.LadderExtendPivot = ladderExt.transform;

            var monitor = Group(root, "V5 FireRoofWaterMonitor", V(.70f, 2.42f, -1.42f)); var mb = new B();
            mb.Cylinder(Vector3.zero, .20f, .32f, M.Red, 16);
            mb.Beam(V(0, .11f, 0), V(0, .24f, .70f), .12f, M.Steel);
            mb.Attach(monitor, "WaterMonitorCannon");
            rig.RoofMonitorPivot = monitor.transform;

            b.Torus(V(-.70f, 2.30f, -.88f), .28f, .062f, M.SafetyAmber, Quaternion.identity);
            // Real pump gauges and hose couplings face rear, with a climb step underneath.
            b.Box(V(0, 1.30f, -length * .5f - .035f), V(1.52f, .83f, .06f), M.Charcoal, .025f);
            foreach (float x in new[] { -.48f, 0f, .48f })
            {
                b.Cylinder(V(x, 1.56f, -length * .5f - .09f), .10f, .025f, M.Linen, 16, Quaternion.Euler(90, 0, 0));
                b.Beam(V(x, 1.56f, -length * .5f - .108f), V(x + .045f, 1.59f, -length * .5f - .108f), .012f, M.Black);
                b.Cylinder(V(x, 1.14f, -length * .5f - .11f), .09f, .14f, M.Steel, 12, Quaternion.Euler(90, 0, 0));
                b.Torus(V(x, 1.14f, -length * .5f - .20f), .12f, .021f, M.Red, Quaternion.Euler(90, 0, 0));
            }
            b.Box(V(0, .52f, -length * .5f - .23f), V(1.52f, .12f, .47f), M.Steel, .02f);
        }

        static void RescueTruck(GameObject root, B b, PanicV5VehicleRig rig, float w)
        {
            b.Box(V(0, 1.36f, -.88f), V(w, .98f, 2.98f), M.SafetyAmber, .05f);
            foreach (int s in new[] { -1, 1 })
            {
                b.Box(V(s * (w * .5f + .016f), 1.36f, -.87f), V(.026f, .60f, 2.55f), M.Charcoal, .012f);
                for (int slot = 0; slot < 3; slot++) b.Box(V(s * (w * .5f + .035f), 1.36f, -1.72f + slot * .83f), V(.017f, .52f, .75f), M.SafetyAmber, .012f);
                b.Box(V(s * (w * .5f + .047f), 1.08f, -.87f), V(.017f, .075f, 2.5f), M.Linen, .002f);
                foreach (float z in new[] { -2.05f, .22f }) b.Beam(V(s * .82f, 1.84f, z), V(s * .82f, 2.16f, z), .055f, M.Charcoal);
                b.Beam(V(s * .82f, 2.17f, -2.1f), V(s * .82f, 2.17f, .27f), .065f, M.Charcoal);
            }
            for (int i = 0; i < 3; i++)
            {
                float x = -.54f + i * .54f;
                b.Box(V(x, 2.22f, -1.1f), V(.46f, .29f, 1.48f), i == 1 ? M.Olive : M.Slate, .04f);
                b.Box(V(x, 2.39f, -1.1f), V(.47f, .055f, 1.50f), M.Charcoal, .015f);
                foreach (float z in new[] { -1.55f, -.68f }) b.Box(V(x, 2.26f, z), V(.49f, .025f, .055f), M.Steel, .003f);
            }
            b.Torus(V(0, 1.51f, -2.53f), .36f, .10f, M.Black, Quaternion.Euler(90, 0, 0));
            b.Cylinder(V(0, 1.51f, -2.55f), .17f, .055f, M.Steel, 12, Quaternion.Euler(90, 0, 0));
            // Articulated front bumper motorized winch spool + hook
            var winch = Group(root, "V5 RescueWinchPivot", V(0, .64f, 2.91f)); var wb = new B();
            wb.Cylinder(Vector3.zero, .14f, .54f, M.Steel, 14, Quaternion.Euler(0, 0, 90));
            wb.Torus(V(0, -.08f, .05f), .09f, .019f, M.Red, Quaternion.Euler(90, 0, 0));
            wb.Attach(winch, "MotorizedWinchDrumHook");
            rig.WinchPivot = winch.transform;

            // Articulated floodlight & recovery boom mast
            var crane = Group(root, "V5 RescueCraneBoomPivot", V(0, 2.23f, .55f)); var cb = new B();
            foreach (int s in new[] { -1, 1 }) cb.Beam(V(s * .73f, 0, 0), V(s * .73f, .55f, 0), .035f, M.Steel);
            cb.Box(V(0, .56f, 0), V(1.56f, .12f, .16f), M.Steel, .02f);
            cb.Beam(V(0, .05f, -.25f), V(0, .48f, -1.35f), .085f, M.SafetyAmber);
            cb.Attach(crane, "ArticulatedFloodlightAndBoom");
            rig.CraneBoomPivot = crane.transform;
        }

        static void AddHeadlightsAndTaillights(GameObject root, PanicV5VehicleRig rig, float width, float length)
        {
            var hLights = new List<Light>(); var tLights = new List<Light>();
            foreach (int s in new[] { -1, 1 })
            {
                var hlNode = Group(root, "V5 HeadlightSpot " + s, V(s * width * .37f, .99f, length * .5f + .06f));
                hlNode.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
                var hl = hlNode.AddComponent<Light>(); hl.type = LightType.Spot; hl.color = new Color(1f, .95f, .82f);
                hl.range = 10f; hl.spotAngle = 46f; hl.intensity = .88f; hl.shadows = LightShadows.None; hl.renderMode = LightRenderMode.ForceVertex;
                hLights.Add(hl);

                var tlNode = Group(root, "V5 TaillightPoint " + s, V(s * width * .39f, .89f, -length * .5f - .05f));
                var tl = tlNode.AddComponent<Light>(); tl.type = LightType.Point; tl.color = new Color(1f, .10f, .05f);
                tl.range = 3.2f; tl.intensity = .40f; tl.shadows = LightShadows.None; tl.renderMode = LightRenderMode.ForceVertex;
                tLights.Add(tl);
            }
            rig.Headlights = hLights.ToArray();
            rig.Taillights = tLights.ToArray();
        }

        static void AddLightbar(GameObject root, PanicV5VehicleRig rig, float y, float z, bool amber)
        {
            var baseBuilder = new B(); baseBuilder.Box(V(0, y, z), V(1.32f, .085f, .37f), M.Charcoal, .02f); baseBuilder.Attach(root, "WarningLightbarBase");
            var renderers = new List<Renderer>(); var lights = new List<Light>(); var colors = new List<Color>();
            for (int side = -1; side <= 1; side += 2)
            {
                var lamp = Group(root, side < 0 ? "BeaconBlue" : "BeaconRed", V(side * .40f, y + .105f, z)); var b = new B();
                b.Box(Vector3.zero, V(.56f, .13f, .31f), M.Linen, .025f); b.Attach(lamp, "SegmentedWarningLens");
                Color color = amber ? new Color(1, .39f, .035f) : side < 0 ? new Color(.045f, .24f, 1) : new Color(1, .06f, .025f);
                var mat = new Material(Shader.Find("Standard")) { name = "V5 " + rig.ServiceId + " warning lens " + side, color = color };
                mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", color * 2); root.GetComponent<PanicV5AssetOwner>().Owned.Add(mat);
                var r = lamp.GetComponentInChildren<Renderer>(); r.sharedMaterial = mat; renderers.Add(r); colors.Add(color);
                var light = lamp.AddComponent<Light>(); light.type = LightType.Point; light.color = color; light.range = 5f; light.intensity = .6f;
                light.shadows = LightShadows.None; light.renderMode = LightRenderMode.ForceVertex; lights.Add(light);
            }
            rig.FlashRenderers = renderers.ToArray(); rig.FlashLights = lights.ToArray(); rig.FlashColors = colors.ToArray();
        }

        public static GameObject CreateZodiac()
        {
            PanicHomeArt.EnsurePalette(); var root = new GameObject("V5 flood rescue zodiac");
            var owner = root.AddComponent<PanicV5AssetOwner>(); var rig = root.AddComponent<PanicV5VehicleRig>(); rig.IsBoat = true; rig.ServiceId = "FloodZodiac";
            var b = new B();
            b.Box(V(0, .08f, -.12f), V(1.43f, .12f, 3.48f), M.Charcoal, .07f);
            b.Quad(V(-.71f, .10f, 1.56f), V(.71f, .10f, 1.56f), V(.30f, .18f, 2.07f), V(-.30f, .18f, 2.07f), M.Charcoal);
            // Continuous segmented inflatable tubes curve around the bow and leave a usable cockpit.
            foreach (int side in new[] { -1, 1 })
            {
                b.Cylinder(V(side * .85f, .25f, -.28f), .285f, 3.10f, M.SafetyAmber, 20, Quaternion.Euler(90, 0, 0));
                b.Ellipsoid(V(side * .85f, .25f, -1.83f), V(.285f, .285f, .32f), M.SafetyAmber, 6);
                Vector3 a = V(side * .85f, .25f, 1.27f), end = V(side * .16f, .30f, 2.00f);
                Vector3 d = end - a; b.Cylinder((a + end) * .5f, .285f, d.magnitude, M.SafetyAmber, 20, Quaternion.FromToRotation(Vector3.up, d));
                b.Ellipsoid(end, V(.285f, .285f, .285f), M.SafetyAmber, 6);
                for (int i = 0; i < 5; i++)
                {
                    float z = -1.60f + i * .65f;
                    b.Torus(V(side * .85f, .25f, z), .289f, .012f, M.Charcoal, Quaternion.Euler(90, 0, 0));
                    b.Box(V(side * 1.08f, .40f, z), V(.055f, .065f, .13f), M.Black, .012f);
                    if (i < 4) b.Beam(V(side * 1.09f, .44f, z), V(side * 1.09f, .44f, z + .65f), .022f, M.Linen);
                }
                b.Beam(V(side * .72f, .61f, -.90f), V(side * .67f, .61f, 1.17f), .028f, M.Steel);
                b.Box(V(side * .66f, .61f, 1.28f), V(.18f, .04f, .48f), M.Black, .035f);
                b.Box(V(side * .90f, .55f, -.30f), V(.27f, .06f, .82f), M.Charcoal, .025f);
            }
            for (int plank = 0; plank < 10; plank++) b.Box(V(0, .16f, -1.6f + plank * .31f), V(1.30f, .045f, .29f), M.Slate, .007f);
            foreach (float z in new[] { -.90f, .45f }) b.Box(V(0, .41f, z), V(1.46f, .085f, .32f), M.Steel, .02f);
            b.Box(V(0, .34f, -1.80f), V(1.44f, .49f, .09f), M.Charcoal, .025f);
            b.Box(V(.34f, .49f, -.20f), V(.48f, .64f, .50f), M.SafetyAmber, .045f);
            b.Box(V(.34f, .84f, -.20f), V(.49f, .07f, .51f), M.Charcoal, .015f);
            var zSteer = Group(root, "V5 ZodiacSteeringWheel", V(.34f, .91f, -.20f)); var zsb = new B();
            zsb.Torus(Vector3.zero, .13f, .021f, M.Black, Quaternion.Euler(65, 0, 0));
            zsb.Beam(V(-.10f, 0, 0), V(.10f, 0, 0), .018f, M.Steel);
            zsb.Attach(zSteer, "ZodiacHelmWheel"); rig.SteeringWheel = zSteer.transform;
            b.Torus(V(-.49f, .27f, .95f), .31f, .070f, M.Red, Quaternion.identity);
            for (int i = 0; i < 4; i++) { float a = i * Mathf.PI * .5f; b.Box(V(-.49f + Mathf.Cos(a) * .31f, .28f, .95f + Mathf.Sin(a) * .31f), V(.10f, .13f, .13f), M.Linen, .012f, Quaternion.Euler(0, -i * 90, 0)); }
            b.Box(V(-.32f, .32f, -.36f), V(.42f, .25f, .54f), M.Olive, .045f);
            b.Box(V(-.32f, .46f, -.36f), V(.43f, .045f, .55f), M.Charcoal, .012f);
            b.Beam(V(0, .65f, 1.85f), V(0, 1.10f, 1.85f), .027f, M.Steel);
            b.Box(V(0, 1.12f, 1.85f), V(.19f, .10f, .16f), M.WarmWindow, .02f);
            var bowSpotNode = Group(root, "V5 ZodiacBowSpot", V(0, 1.12f, 1.94f));
            bowSpotNode.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            var bowSpot = bowSpotNode.AddComponent<Light>(); bowSpot.type = LightType.Spot; bowSpot.color = new Color(1f, .94f, .78f);
            bowSpot.range = 9.5f; bowSpot.spotAngle = 44f; bowSpot.intensity = .85f; bowSpot.shadows = LightShadows.None; bowSpot.renderMode = LightRenderMode.ForceVertex;
            rig.Headlights = new[] { bowSpot };
            var motor = Group(root, "V5 OutboardMotor", V(0, .27f, -2.02f)); var mb = new B();
            mb.Box(V(0, .34f, 0), V(.54f, .52f, .49f), M.Charcoal, .08f);
            mb.Box(V(0, .38f, -.25f), V(.35f, .14f, .018f), M.Red, .012f);
            mb.Box(V(0, -.18f, .04f), V(.15f, .57f, .19f), M.Steel, .025f);
            mb.Box(V(0, -.46f, -.02f), V(.39f, .035f, .34f), M.Charcoal, .015f); mb.Attach(motor, "OutboardCowlingShaftFin");
            rig.OutboardPivot = motor.transform;
            var prop = Group(motor, "V5 OutboardPropeller", V(0, -.36f, -.20f)); var pb = new B();
            pb.Cylinder(Vector3.zero, .065f, .12f, M.Steel, 10, Quaternion.Euler(90, 0, 0));
            for (int i = 0; i < 3; i++) { float a = i * 120; pb.Box(Quaternion.Euler(0, 0, a) * V(.10f, 0, 0), V(.22f, .067f, .033f), M.Steel, .018f, Quaternion.Euler(12, 0, a + 18)); }
            pb.Attach(prop, "ThreeBladePropeller"); rig.Propeller = prop.transform;
            b.Attach(root, "InflatableTubesRopeCockpitRescueEquipment");
            BodyCollider(root, V(0, .17f, -.10f), V(1.5f, .24f, 3.7f));
            foreach (int s in new[] { -1, 1 }) BodyCollider(root, V(s * .85f, .29f, -.25f), V(.54f, .54f, 3.25f));
            var rb = root.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false; rb.mass = 260; rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            AddNavigationLight(root, V(-1.02f, .61f, .92f), new Color(1, .07f, .03f), owner);
            AddNavigationLight(root, V(1.02f, .61f, .92f), new Color(.03f, .8f, .29f), owner);
            var sternPole = Group(root, "V5 ZodiacSternBeacon", V(.55f, 1.05f, -1.68f));
            var sb = new B(); sb.Beam(V(0, -.55f, 0), V(0, 0, 0), .02f, M.Steel); sb.Cylinder(V(0, .04f, 0), .055f, .08f, M.SafetyAmber, 12); sb.Attach(sternPole, "SternBeaconDome");
            var beaconColor = new Color(1f, .58f, .10f);
            var beaconMat = new Material(Shader.Find("Standard")) { name = "V5 zodiac stern beacon", color = beaconColor };
            beaconMat.EnableKeyword("_EMISSION"); beaconMat.SetColor("_EmissionColor", beaconColor * 1.9f); owner.Owned.Add(beaconMat);
            var sternRen = sternPole.GetComponentInChildren<Renderer>(); if (sternRen) sternRen.sharedMaterial = beaconMat;
            var sternLight = sternPole.AddComponent<Light>(); sternLight.type = LightType.Point; sternLight.color = beaconColor;
            sternLight.range = 6.2f; sternLight.intensity = 1.6f; sternLight.shadows = LightShadows.None; sternLight.renderMode = LightRenderMode.ForceVertex;
            rig.FlashLights = new[] { sternLight }; rig.FlashRenderers = sternRen ? new[] { sternRen } : new Renderer[0]; rig.FlashColors = new[] { beaconColor };
            Refinish(root, owner, Service.Rescue); owner.CaptureGeneratedMeshes(); return root;
        }

        static void AddNavigationLight(GameObject root, Vector3 p, Color color, PanicV5AssetOwner owner)
        {
            var node = Group(root, "V5 NavigationLamp", p); var b = new B(); b.Box(Vector3.zero, V(.10f, .065f, .14f), M.Linen, .015f); b.Attach(node, "NavigationLens");
            var mat = new Material(Shader.Find("Standard")) { name = "V5 zodiac navigation lens", color = color }; mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", color * 1.8f); owner.Owned.Add(mat);
            node.GetComponentInChildren<Renderer>().sharedMaterial = mat;
        }

        public static GameObject CreateResponder(Service service, int variant)
        {
            var root = PanicExteriorArt.CreatePedestrianVisual(variant + 12); if (!root) return null;
            root.name = "V5 " + service + " articulated responder"; var owner = root.AddComponent<PanicV5AssetOwner>();
            // A real nonblocking body volume is available for response-area queries. It cannot push
            // the retained player/controller or become a second actor movement owner.
            var volume=root.AddComponent<CapsuleCollider>();volume.center=V(0,.90f,0);volume.height=1.80f;volume.radius=.25f;volume.isTrigger=true;
            var body=root.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;body.mass=75;
            Color uniform = service == Service.Police ? new Color(.055f, .105f, .205f) : service == Service.Ambulance ? new Color(.03f, .43f, .43f) : service == Service.Fire ? new Color(.29f, .245f, .14f) : new Color(.88f, .30f, .055f);
            var replacements = new Dictionary<Material, Material>();
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var source = mats[i]; if (!source) continue; string n = source.name.ToLowerInvariant();
                    if (!n.Contains("jacket") && !n.Contains("canvas") && !n.Contains("trouser") && !n.Contains("t shirt") && !n.Contains("knit collar")) continue;
                    if (!replacements.TryGetValue(source, out var mat))
                    { mat = new Material(source) { name = "V5 " + service + " uniform " + source.name, color = n.Contains("trouser") ? uniform * .64f : uniform }; mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1); replacements[source] = mat; owner.Owned.Add(mat); }
                    mats[i] = mat;
                }
                r.sharedMaterials = mats;
            }
            var animator = root.GetComponentInChildren<Animator>();
            Transform head = PanicCharacterArt.GetCharacterBone(root, animator, HumanBodyBones.Head);
            Transform chest = PanicCharacterArt.GetCharacterBone(root, animator, HumanBodyBones.Chest);
            Transform hips = PanicCharacterArt.GetCharacterBone(root, animator, HumanBodyBones.Hips);
            if (!head || !chest || !hips) { Debug.LogWarning("V5 responder equipment needs retained humanoid head/chest/hips"); return root; }
            Vector3 h = root.transform.InverseTransformPoint(head.position), c = root.transform.InverseTransformPoint(chest.position), p = root.transform.InverseTransformPoint(hips.position);
            // All decorative geometry is attached to its actual bone without rotating or moving that bone.
            var hat = BoneGroup(root, head, "V5 " + service + " bone-bound headgear"); var hb = new B();
            if (service == Service.Police)
            {
                hb.Cylinder(h + V(0, .12f, 0), .145f, .07f, M.Charcoal, 16);
                hb.Box(h + V(0, .09f, .14f), V(.22f, .025f, .14f), M.Charcoal, .025f, Quaternion.Euler(6, 0, 0));
                hb.Box(h + V(0, .13f, .143f), V(.041f, .045f, .012f), M.SafetyAmber, .009f);
            }
            else
            {
                M helmet = service == Service.Fire ? M.SafetyAmber : service == Service.Rescue ? M.Red : M.Linen;
                if (service != Service.Ambulance)
                {
                    hb.Ellipsoid(h + V(0, .095f, .003f), V(.167f, .13f, .172f), helmet, 4);
                    hb.Cylinder(h + V(0, .054f, .015f), .19f, .034f, helmet, 20);
                    hb.Box(h + V(0, .12f, .167f), V(.075f, .045f, .045f), M.Charcoal, .01f);
                    hb.Box(h + V(0, .12f, .193f), V(.054f, .029f, .014f), M.WarmWindow, .004f);
                    if (service == Service.Fire) hb.Box(h + V(0, -.01f, -.12f), V(.27f, .20f, .075f), M.Charcoal, .035f);
                    var headlampNode = Group(hat, "Headlamp_SpotLight", h + V(0, .12f, .20f));
                    headlampNode.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
                    var headlamp = headlampNode.AddComponent<Light>(); headlamp.type = LightType.Spot;
                    headlamp.color = new Color(1f, .94f, .78f); headlamp.range = 8f; headlamp.spotAngle = 42f;
                    headlamp.intensity = .82f; headlamp.shadows = LightShadows.None; headlamp.renderMode = LightRenderMode.ForceVertex;
                }
                else
                {
                    hb.Cylinder(h + V(0, .12f, 0), .142f, .065f, M.Olive, 16);
                    hb.Box(h + V(0, .10f, .135f), V(.20f, .022f, .11f), M.Olive, .025f);
                    hb.Box(h + V(0, .13f, .142f), V(.042f, .014f, .008f), M.Red, .002f);
                    hb.Box(h + V(0, .13f, .142f), V(.014f, .042f, .008f), M.Red, .002f);
                }
            }
            hb.Attach(hat, "ProfessionHelmetCap"); AttachBoneGeometry(hat, head, root);
            var vest = BoneGroup(root, chest, "V5 " + service + " bone-bound torso equipment"); var vb = new B();
            float front = c.z + .215f;
            // Narrow reflective bands remain recognisable without a broad pale chest plate.
            foreach (int s in new[] { -1, 1 })
            {
                vb.Box(V(c.x + s * .105f, c.y + .01f, front), V(.028f, .30f, .014f), service == Service.Police ? M.Steel : M.SafetyAmber, .004f);
                vb.Box(V(c.x + s * .14f, c.y - .08f, front + .012f), V(.072f, .078f, .022f), M.Charcoal, .008f);
            }
            vb.Box(V(c.x, c.y - .17f, front - .007f), V(.39f, .027f, .018f), service == Service.Fire ? M.SafetyAmber : M.Steel, .004f);
            vb.Box(V(c.x - .17f, c.y + .12f, front + .025f), V(.06f, .105f, .038f), M.Charcoal, .008f);
            vb.Beam(V(c.x - .17f, c.y + .16f, front), V(c.x - .17f, c.y + .26f, front), .010f, M.Black);
            if (service == Service.Police)
                vb.Box(V(c.x - .08f, c.y + .09f, front + .015f), V(.038f, .046f, .010f), M.SafetyAmber, .003f);
            if (service == Service.Ambulance)
            {
                vb.Torus(V(c.x, c.y + .17f, front), .072f, .012f, M.Charcoal, Quaternion.Euler(90, 0, 0));
                vb.Cylinder(V(c.x + .06f, c.y + .02f, front + .015f), .035f, .014f, M.Steel, 12, Quaternion.Euler(90, 0, 0));
            }
            if (service == Service.Fire)
            {
                foreach (int s in new[] { -1, 1 }) vb.Cylinder(V(c.x + s * .105f, c.y, c.z - .24f), .072f, .41f, M.SafetyAmber, 12);
                vb.Box(V(c.x, c.y, c.z - .235f), V(.33f, .30f, .085f), M.Charcoal, .03f);
            }
            vb.Attach(vest, "UniformReflectorsRadioSafetyEquipment"); AttachBoneGeometry(vest, chest, root);
            var belt = BoneGroup(root, hips, "V5 " + service + " bone-bound belt equipment"); var bb = new B();
            bb.Box(p + V(.23f, -.06f, .03f), V(.085f, .17f, .12f), service == Service.Ambulance ? M.Red : M.Charcoal, .018f);
            if (service == Service.Ambulance)
            {
                bb.Box(p + V(.278f, -.06f, .03f), V(.01f, .073f, .016f), M.Linen, .002f);
                bb.Box(p + V(.279f, -.06f, .03f), V(.012f, .016f, .070f), M.Linen, .002f);
            }
            bb.Attach(belt, "MedicKitUtilityPouch"); AttachBoneGeometry(belt, hips, root);
            owner.CaptureGeneratedMeshes(); return root;
        }

        static GameObject BoneGroup(GameObject root, Transform bone, string name)
        { return Group(root, name, Vector3.zero); }
        static void AttachBoneGeometry(GameObject group, Transform bone, GameObject root)
        { group.transform.SetParent(bone, true); }
        static void BodyCollider(GameObject root, Vector3 center, Vector3 size)
        { var c = Group(root, "V5 compound body collision", center).AddComponent<BoxCollider>(); c.size = size; }

        static void Refinish(GameObject root, PanicV5AssetOwner owner, Service service)
        {
            var map = new Dictionary<Material, Material>();
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var source = mats[i]; if (!source || !source.name.StartsWith("HomeV2_")) continue;
                    if (!map.TryGetValue(source, out var material))
                    {
                        material = new Material(source) { name = "V5 " + service + " " + source.name };
                        if (source.name.EndsWith("Linen")) material.color = new Color(.88f, .90f, .87f);
                        if (source.name.EndsWith("Charcoal")) material.color = service == Service.Police ? new Color(.045f, .075f, .12f) : new Color(.075f, .085f, .09f);
                        if (source.name.EndsWith("Red")) material.color = new Color(.70f, .055f, .045f);
                        if (source.name.EndsWith("SafetyAmber")) material.color = new Color(.97f, .36f, .035f);
                        if (source.name.EndsWith("Glass")) { material.color = new Color(.14f, .27f, .31f, .86f); material.SetFloat("_Glossiness", .65f); }
                        if (source.name.EndsWith("Steel")) { material.SetFloat("_Metallic", .65f); material.SetFloat("_Glossiness", .43f); }
                        if (source.name.EndsWith("WarmWindow")) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", new Color(.9f, .77f, .48f) * .7f); }
                        map[source] = material; owner.Owned.Add(material);
                    }
                    mats[i] = material;
                }
                r.sharedMaterials = mats;
            }
        }
    }
}
