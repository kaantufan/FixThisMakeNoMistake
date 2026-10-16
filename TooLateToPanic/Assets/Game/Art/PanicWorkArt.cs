using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>
    /// Original authored workshop closeups and installed hardware. Metres, Y up, front -Z.
    /// Every visible part is a real MeshFilter/MeshRenderer. No colliders, behaviours,
    /// external textures, scene objects or gameplay state are created by this helper.
    /// Bake meshes/shared materials; preserve named moving transforms when baking rigs.
    /// </summary>
    public static class PanicWorkArt
    {
        enum P { Ink, Steel, SteelDark, Copper, Walnut, WalnutLight, WalnutDark, Indigo,
            Taupe, Plaster, Olive, Leaf, Soil, Stone, Rubber, Red, Paper, Amber, Water, Glass }
        enum Job { AnchorWall, AnchorFloor, Upgrade, Flood, Fire, Plant, Drain, Fence, Hose }
        sealed class RigState
        {
            public GameObject Root;
            public Job Job;
            public Transform Tool, Motion, Secondary, Gauge, Valve, Flow, WateringCan;
            public readonly Transform[] Screws = new Transform[4], Heads = new Transform[4], Stages = new Transform[4];
            public readonly Vector3[] ScrewSeats = new Vector3[4];
            public Vector3 MotionRest, SecondaryRest;
            public Quaternion MotionRotation;
            public float LastProgress, Angle;
        }
        static Material[] palette;
        static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
        static readonly Dictionary<int, RigState> rigs = new Dictionary<int, RigState>();
        static readonly string[] screwNames = { "Fastener_0", "Fastener_1", "Fastener_2", "Fastener_3" };
        static readonly string[] stageNames = { "Stage_0", "Stage_1", "Stage_2", "Stage_3" };
        static readonly Quaternion FrontAxis = Quaternion.Euler(90, 0, 0);

        /// <summary>~1.6m wide work display. Fits the Home V2 closeup camera unchanged.
        /// Four work steps are assumed, matching PanicWorkSession.TotalSteps=4.</summary>
        public static GameObject CreateWorkRig(string action)
        {
            Init();
            Job job = Parse(action);
            var root = new GameObject("PanicWorkRig_" + job);
            var t = root.transform;
            Bench(t, job == Job.Plant || job == Job.Drain);
            switch (job)
            {
                case Job.AnchorWall: WallAnchorJob(t); break;
                case Job.AnchorFloor: FloorAnchorJob(t); break;
                case Job.Upgrade: BraceJob(t); break;
                case Job.Flood: SealJob(t); break;
                case Job.Fire: FireJob(t); break;
                case Job.Plant: PlantJob(t); break;
                case Job.Drain: DrainJob(t); break;
                case Job.Fence: FenceJob(t); break;
                case Job.Hose: HoseJob(t); break;
            }
            // Small practical objects provide scale without floating labels or UI geometry.
            if (job != Job.Plant && job != Job.Drain)
            {
                ScrewTray(t, new Vector3(-.62f, .095f, -.18f));
                FoldedCloth(t, new Vector3(.61f, .087f, -.16f));
            }
            TrimDeadStates();
            var state = ReadState(root, job);
            rigs[root.GetInstanceID()] = state;
            UpdateWorkRig(root, 0, 0, 0);
            return root;
        }

        /// <summary>No per-frame mesh/material creation or hierarchy scans after the first
        /// update of a prefab clone. Motion follows work progress, not a passive time bar.</summary>
        public static void UpdateWorkRig(GameObject rig, float progress, int completedSteps, float time)
        {
            if (!rig) return;
            RigState s;
            if (!rigs.TryGetValue(rig.GetInstanceID(), out s) || s.Root != rig)
            {
                s = ReadState(rig, JobFromName(rig.name));
                rigs[rig.GetInstanceID()] = s;
            }
            float p = Finite(progress) ? Mathf.Clamp01(progress) : 0;
            int done = Mathf.Clamp(completedSteps, 0, 4);
            if (p >= .9999f) done = 4;
            int active = Mathf.Min(done, 3);
            float part = done >= 4 ? 1 : Mathf.Clamp01(p * 4 - done);
            float delta = Mathf.Max(0, p - s.LastProgress);
            s.Angle += delta * 4320;
            s.LastProgress = p;

            // The threaded shafts actually travel into the seated plates; the cross recess
            // and tool rotate about the screw axis. Finished screws remain flush.
            for (int i = 0; i < 4; i++)
            {
                Transform screw = s.Screws[i];
                if (!screw) continue;
                float seated = i < done ? 1 : i == active ? part : 0;
                screw.localPosition = s.ScrewSeats[i] + screw.localRotation * new Vector3(0, 0, -.048f * (1 - seated));
                var head = s.Heads[i];
                if (head) head.localRotation = Quaternion.Euler(0, 0, seated * 1080);
            }

            if (s.Job == Job.Flood)
            {
                Vector3 a, b;
                SealEdge(active, out a, out b);
                if (s.Tool)
                {
                    s.Tool.localPosition = Vector3.Lerp(a, b, part) + new Vector3(0, 0, -.026f);
                    s.Tool.localRotation = Quaternion.Euler(-12, -18, -23);
                }
                for (int i = 0; i < 4; i++) if (s.Stages[i])
                    s.Stages[i].localScale = new Vector3(1, 1, i < done ? 1 : i == active ? Mathf.Max(.001f, part) : .001f);
            }
            else if (s.Job == Job.Plant)
            {
                if (s.Tool)
                {
                    float dig = Mathf.Sin(part * Mathf.PI);
                    s.Tool.localPosition = new Vector3(-.36f + active * .19f, .14f - dig * .06f, -.06f);
                    s.Tool.localRotation = Quaternion.Euler(-14 - dig * 32, 0, 22 - dig * 24);
                }
                float planting = Mathf.SmoothStep(0, 1, Mathf.Clamp01((p - .25f) * 4f));
                if (s.Motion) s.Motion.localPosition = Vector3.Lerp(new Vector3(.42f, .48f, .02f), new Vector3(.07f, .15f, .02f), planting);
                if (s.Secondary) s.Secondary.localScale = new Vector3(1, Mathf.Lerp(.16f, 1, Mathf.Clamp01((p - .50f) * 4f)), 1);
                for (int i = 0; i < 4; i++) if (s.Stages[i])
                    s.Stages[i].localScale = Vector3.one * (i < done ? .42f : i == active ? 1 - part * .58f : 1);
            }
            else if (s.Job == Job.Drain)
            {
                if (s.Tool)
                {
                    s.Tool.localPosition = new Vector3(-.48f + active * .32f, .17f - Mathf.Sin(part * Mathf.PI) * .085f, -.02f);
                    s.Tool.localRotation = Quaternion.Euler(-23 - Mathf.Sin(part * Mathf.PI) * 30, 8, -16);
                }
                for (int i = 0; i < 4; i++) if (s.Stages[i])
                    s.Stages[i].localPosition = new Vector3(-.48f + i * .32f, .11f + (1 - (i < done ? 1 : i == active ? part : 0)) * .12f, .04f);
            }
            else
            {
                Transform fastener = s.Screws[active];
                if (s.Tool && fastener)
                {
                    s.Tool.localPosition = fastener.localPosition + fastener.localRotation * new Vector3(0, 0, -.017f);
                    // A small lean keeps the blade, recess and turning head readable.
                    s.Tool.localRotation = fastener.localRotation * Quaternion.Euler(8, -11, s.Angle);
                }
                if (s.Motion)
                {
                    if (s.Job == Job.Upgrade)
                        s.Motion.localPosition = s.MotionRest + Vector3.right * ((1 - p) * .035f);
                    else if (s.Job == Job.Fire)
                        s.Motion.localRotation = s.MotionRotation * Quaternion.Euler(0, 0, -Mathf.Sin(Mathf.Clamp01((p - .65f) * 4) * Mathf.PI) * 14);
                    else if (s.Job == Job.Hose)
                        s.Motion.localRotation = Quaternion.Euler(0, 0, -p * 42);
                    else if (s.Job == Job.Fence)
                        s.Motion.localPosition = s.MotionRest + Vector3.up * ((1 - p) * .025f);
                }
            }
            if (s.Tool) s.Tool.gameObject.SetActive(done < 4);
            if (s.Flow) s.Flow.gameObject.SetActive(false);
            if (s.WateringCan) s.WateringCan.gameObject.SetActive(false);
            if (s.Secondary && s.Job == Job.Fire)
                s.Secondary.localPosition = s.SecondaryRest + Vector3.left * Mathf.Clamp01(p * 4) * .085f;
        }

        /// <summary>Home V4 input-driven tool pose. The legacy four-argument API remains valid.</summary>
        public static void UpdateWorkRig(GameObject rig, float progress, int completedSteps, float time,
            float cursor, bool isApplying, float toolMotion)
        {
            // Progress may reach 1 during the final .25s stroke, before the fourth
            // stage is committed. Keep the tool visible until that real settling motion ends.
            float visualProgress = completedSteps < 4 ? Mathf.Min(progress, .9998f) : progress;
            UpdateWorkRig(rig, visualProgress, completedSteps, time);
            if (!rig) return;
            RigState s;
            if (!rigs.TryGetValue(rig.GetInstanceID(), out s)) return;
            float aim = Finite(cursor) ? Mathf.Clamp01(cursor) : .5f;
            float motion = Finite(toolMotion) ? Mathf.Clamp(toolMotion, -1f, 1f) : 0f;
            int active = Mathf.Clamp(completedSteps, 0, 3);
            bool finished = completedSteps >= 4;
            float stroke = Mathf.Sin((1f - Mathf.Abs(motion)) * Mathf.PI);
            if (s.Job == Job.Plant || s.Job == Job.Drain)
            {
                if (s.Tool)
                {
                    s.Tool.gameObject.SetActive(!finished && (s.Job != Job.Plant || active == 0 || active == 2));
                    s.Tool.localPosition += new Vector3(isApplying ? 0 : (aim - .5f) * .30f, -stroke * .11f, stroke * .07f);
                    s.Tool.localRotation *= Quaternion.Euler(-stroke * 34f, 0, stroke * 12f);
                }
                if (s.WateringCan)
                {
                    s.WateringCan.gameObject.SetActive(!finished && active == 3);
                    s.WateringCan.localPosition = new Vector3(.39f, .44f - stroke * .06f, -.025f);
                    s.WateringCan.localRotation = Quaternion.Euler(0, 0, 12 + stroke * 38f);
                }
                if (s.Job == Job.Plant && s.Motion && active == 1 && isApplying)
                {
                    s.Motion.localRotation = Quaternion.Euler(0, 0, -stroke * 7f);
                    if (motion > 0f) s.Motion.localPosition = Vector3.Lerp(new Vector3(.42f, .48f, .02f), new Vector3(.07f, .15f, .02f), 1 - motion);
                }
                if (s.Job == Job.Drain && s.Stages[active] && motion > 0f)
                    s.Stages[active].localPosition = new Vector3(-.48f + active * .32f, .11f + motion * .12f, .04f);
            }
            else if (s.Job == Job.Flood)
            {
                if (s.Tool)
                {
                    s.Tool.localPosition += Vector3.right * (isApplying ? 0 : (aim - .5f) * .15f);
                    s.Tool.localRotation *= Quaternion.Euler(-stroke * 18f, 0, stroke * 8f);
                }
            }
            else if (s.Job == Job.Fire || s.Job == Job.Hose)
            {
                // The control is a valve/lever pressure test, rather than a passive screw bar.
                if (s.Tool) s.Tool.gameObject.SetActive(false);
                if (s.Gauge) s.Gauge.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(110, -110, aim));
                if (s.Valve) s.Valve.localRotation = Quaternion.Euler(0, 0, aim * 180 + progress * 270);
                if (s.Motion && s.Job == Job.Fire)
                    s.Motion.localRotation = s.MotionRotation * Quaternion.Euler(0, 0, isApplying ? -Mathf.Abs(motion) * 18 : 0);
                if (s.Motion && s.Job == Job.Hose)
                    s.Motion.localRotation = Quaternion.Euler(0, 0, -progress * 160);
            }
            else if (s.Tool && !finished)
            {
                if (!isApplying) s.Tool.localPosition += new Vector3((aim - .5f) * .16f, .015f, -.045f);
                s.Tool.localRotation *= Quaternion.Euler(0, 0, motion * 20f);
            }
            if (s.Flow)
            {
                bool flow = !finished && isApplying && motion > .05f &&
                    (s.Job == Job.Fire || s.Job == Job.Hose || s.Job == Job.Plant && active == 3 || s.Job == Job.Drain && active == 3);
                s.Flow.gameObject.SetActive(flow);
                if (flow)
                {
                    // Mesh droplets move only while the real test/watering input is active.
                    float flutter = Mathf.Sin(time * 19) * .014f;
                    s.Flow.localScale = new Vector3(1 + flutter, 1 + flutter * .6f, 1);
                }
            }
        }

        /// <summary>Furniture is grounded and centred X/Z. Hardware stays at the upper
        /// BACK (+Z), with real wall shoes, folded L plates, webbing, buckles and screws.
        /// This deliberately contains no feet or floor fixings.</summary>
        public static GameObject CreateWallAnchors(Vector3 size)
        {
            Init();
            float w = Safe(size.x, .3f, 6, 1), h = Safe(size.y, .22f, 5, 1), d = Safe(size.z, .2f, 6, .6f);
            var root = new GameObject("Installed_upper_back_wall_anchors");
            float y = Mathf.Max(.18f, h - Mathf.Min(.20f, h * .18f));
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * w * .32f;
                var mount = New(root.transform, "Wall_tie_" + (side < 0 ? "left" : "right"), new Vector3(x, y, d * .5f));
                Box(mount, "Furniture_shoe", new Vector3(0, 0, .014f), new Vector3(.105f, .18f, .025f), P.SteelDark);
                Box(mount, "Folded_L_return", new Vector3(0, .07f, .070f), new Vector3(.105f, .025f, .14f), P.Steel);
                Box(mount, "Wall_shoe", new Vector3(0, .15f, .125f), new Vector3(.12f, .18f, .023f), P.Steel);
                Between(mount, "Anti_tip_woven_strap", new Vector3(0, -.04f, .036f), new Vector3(0, .20f, .113f), .047f, .009f, P.Olive);
                for (int stitch = 0; stitch < 5; stitch++)
                    Box(mount, "Stitched_webbing_rib", new Vector3(0, -.01f + stitch * .033f, .046f + stitch * .0106f), new Vector3(.048f, .004f, .006f), P.Taupe);
                Box(mount, "Tension_buckle_outer", new Vector3(0, .067f, .089f), new Vector3(.073f, .064f, .014f), P.Copper);
                Box(mount, "Tension_buckle_opening", new Vector3(0, .067f, .099f), new Vector3(.047f, .038f, .009f), P.Ink);
                Box(mount, "Buckle_tongue", new Vector3(0, .067f, .105f), new Vector3(.012f, .049f, .008f), P.Steel);
                // Furniture screws enter -Z; wall screws enter +Z across the standoff.
                FixedScrew(mount, new Vector3(0, -.052f, .034f), Quaternion.Euler(0, 180, 0), .63f);
                FixedScrew(mount, new Vector3(0, .19f, .107f), Quaternion.identity, .63f);
            }
            return root;
        }

        /// <summary>1.1 x 1.25m unit wall hardware, origin centred. Root positions it at
        /// the wall. Tier1 timber diagonal, tier2 steel X, tier3 extra gussets and straps.</summary>
        public static GameObject CreateReinforcement(int tier)
        {
            Init();
            var root = new GameObject("Installed_wall_reinforcement_tier_" + tier);
            if (tier <= 0) return root;
            int level = Mathf.Clamp(tier, 1, 3);
            for (int side = -1; side <= 1; side += 2)
            {
                Box(root.transform, "Bolted_vertical_rail", new Vector3(side * .47f, 0, 0), new Vector3(.10f, 1.22f, .065f), level == 1 ? P.Walnut : P.SteelDark);
                for (int i = 0; i < 3; i++) FixedScrew(root.transform, new Vector3(side * .47f, -.49f + i * .49f, -.043f), Quaternion.identity, .70f);
            }
            Between(root.transform, "Diagonal_load_brace", new Vector3(-.47f, -.53f, -.055f), new Vector3(.47f, .53f, -.055f), level == 1 ? .11f : .075f, .038f, level == 1 ? P.WalnutLight : P.Steel);
            if (level >= 2) Between(root.transform, "Counter_diagonal_load_brace", new Vector3(.47f, -.53f, -.072f), new Vector3(-.47f, .53f, -.072f), .075f, .035f, P.Steel);
            for (int side = -1; side <= 1; side += 2) for (int row = -1; row <= 1; row += 2)
            {
                Vector3 p = new Vector3(side * .47f, row * .53f, -.075f);
                Box(root.transform, "Corner_gusset_plate", p, new Vector3(.18f, .16f, .022f), level >= 3 ? P.Copper : P.SteelDark);
                FixedScrew(root.transform, p + new Vector3(side * .036f, row * .025f, -.02f), Quaternion.identity, .76f);
            }
            if (level >= 3)
            {
                Box(root.transform, "Central_cross_lock", new Vector3(0, 0, -.111f), new Vector3(.19f, .19f, .024f), P.Indigo, Quaternion.Euler(0, 0, 45));
                FixedScrew(root.transform, new Vector3(0, 0, -.133f), Quaternion.identity, .9f);
                for (int side = -1; side <= 1; side += 2)
                    Box(root.transform, "Horizontal_tension_strap", new Vector3(0, side * .33f, -.041f), new Vector3(1.06f, .044f, .018f), P.SteelDark);
            }
            return root;
        }

        static void Bench(Transform p, bool soil)
        {
            Box(p, "Walnut_workbench_edge", new Vector3(0, .025f, 0), new Vector3(1.62f, .07f, .78f), P.WalnutDark);
            Box(p, "Indigo_work_mat", new Vector3(0, .066f, -.025f), new Vector3(1.57f, .018f, .70f), soil ? P.Soil : P.Indigo);
            for (int i = 0; i < 12; i++)
                Box(p, "Worn_mat_edge_tick", new Vector3(-.72f + i * .13f, .077f, -.351f), new Vector3(.035f, .0015f, .009f), P.Taupe);
            if (!soil)
            {
                Box(p, "Scuffed_plaster_backboard", new Vector3(0, .56f, .245f), new Vector3(1.58f, .99f, .045f), P.Plaster);
                Box(p, "Lower_backboard_trim", new Vector3(0, .112f, .212f), new Vector3(1.6f, .067f, .045f), P.Taupe);
                for (int i = 0; i < 11; i++)
                    Box(p, "Plaster_hairline_scuff", new Vector3(-.67f + i * .123f, .25f + ((i * 37) % 65) * .01f, .218f), new Vector3(.003f, .035f + i % 3 * .014f, .002f), P.Taupe, Quaternion.Euler(0, 0, i * 29));
            }
        }

        static void WallAnchorJob(Transform p)
        {
            // A real cabinet top and upper back edge in front of a plaster wall.
            WoodenPanel(p, new Vector3(0, .302f, .015f), new Vector3(1.17f, .42f, .22f));
            Box(p, "Cabinet_top_bevel", new Vector3(0, .525f, -.004f), new Vector3(1.24f, .055f, .27f), P.WalnutLight);
            Box(p, "Cabinet_recessed_door", new Vector3(0, .31f, -.111f), new Vector3(.99f, .285f, .018f), P.WalnutDark);
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .37f;
                Box(p, "Wall_mount_flange", new Vector3(x, .738f, .149f), new Vector3(.15f, .24f, .025f), P.SteelDark);
                Box(p, "Right_angle_top_return", new Vector3(x, .610f, .050f), new Vector3(.15f, .027f, .222f), P.Steel);
                Between(p, "Tension_steel_strap", new Vector3(x, .815f, .12f), new Vector3(x, .559f, -.08f), .075f, .021f, P.Steel);
                Box(p, "Furniture_rear_mount", new Vector3(x, .549f, -.097f), new Vector3(.155f, .11f, .025f), P.SteelDark);
                Ring(p, "Copper_compression_washer", new Vector3(x, .77f, .117f), Vector3.forward, .039f, .005f, P.Copper);
                DynamicScrew(p, side < 0 ? 0 : 2, new Vector3(x, .77f, .111f));
                DynamicScrew(p, side < 0 ? 1 : 3, new Vector3(x, .553f, -.119f));
            }
            Driver(p, false);
        }

        static void FloorAnchorJob(Transform p)
        {
            Box(p, "Slate_floor_sample", new Vector3(0, .105f, .005f), new Vector3(1.26f, .055f, .59f), P.Stone);
            for (int i = 0; i < 4; i++) Box(p, "Floor_grout_joint", new Vector3(-.47f + i * .31f, .135f, .005f), new Vector3(.008f, .001f, .59f), P.Ink);
            WoodenPanel(p, new Vector3(0, .36f, .16f), new Vector3(.79f, .46f, .20f));
            Box(p, "Cabinet_base_rail", new Vector3(0, .18f, .04f), new Vector3(.89f, .09f, .13f), P.WalnutLight);
            Quaternion down = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .40f;
                Box(p, "Folded_vertical_leg", new Vector3(x, .24f, .052f), new Vector3(.16f, .21f, .027f), P.SteelDark);
                Box(p, "Floor_anchor_plate", new Vector3(x, .154f, -.05f), new Vector3(.23f, .027f, .25f), P.Steel);
                FixedScrew(p, new Vector3(x, .27f, .03f), Quaternion.identity, .86f);
                DynamicScrew(p, side < 0 ? 0 : 2, new Vector3(x - .055f, .178f, -.095f), down);
                DynamicScrew(p, side < 0 ? 1 : 3, new Vector3(x + .055f, .178f, -.095f), down);
            }
            Driver(p, false);
        }

        static void BraceJob(Transform p)
        {
            for (int side = -1; side <= 1; side += 2)
                WoodenPanel(p, new Vector3(side * .51f, .564f, .14f), new Vector3(.13f, .86f, .13f));
            Box(p, "Wall_lower_noggin", new Vector3(0, .17f, .13f), new Vector3(1.13f, .09f, .12f), P.WalnutDark);
            Box(p, "Wall_upper_noggin", new Vector3(0, .96f, .13f), new Vector3(1.13f, .09f, .12f), P.WalnutDark);
            var brace = New(p, "JobMotion", Vector3.zero);
            Between(brace, "Steel_diagonal_brace", new Vector3(-.51f, .22f, .029f), new Vector3(.51f, .91f, .029f), .09f, .038f, P.Steel);
            Between(p, "Opposing_timber_brace", new Vector3(.51f, .22f, .08f), new Vector3(-.51f, .91f, .08f), .085f, .038f, P.WalnutLight);
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(i < 2 ? -.51f : .51f, i % 2 == 0 ? .23f : .90f, -.005f);
                Box(p, "Bolted_gusset", pos + Vector3.forward * .019f, new Vector3(.18f, .17f, .025f), P.SteelDark);
                DynamicScrew(p, i, pos);
            }
            Ring(p, "Crossing_reinforcement_ring", new Vector3(0, .563f, -.001f), Vector3.forward, .045f, .006f, P.Copper);
            Driver(p, false);
        }

        static void SealJob(Transform p)
        {
            Box(p, "Window_recess", new Vector3(0, .58f, .17f), new Vector3(1.27f, .81f, .085f), P.Ink);
            Box(p, "Bluegrey_window_pane", new Vector3(0, .58f, .117f), new Vector3(1.09f, .61f, .015f), P.Glass);
            Box(p, "Muted_glass_reflection", new Vector3(-.23f, .60f, .103f), new Vector3(.032f, .51f, .002f), P.Taupe, Quaternion.Euler(0, 0, -32));
            for (int side = -1; side <= 1; side += 2)
            {
                Box(p, "Walnut_window_side_frame", new Vector3(side * .61f, .58f, .068f), new Vector3(.10f, .82f, .12f), P.Walnut);
                Box(p, "Walnut_window_cross_frame", new Vector3(0, .58f + side * .36f, .068f), new Vector3(1.21f, .10f, .12f), P.WalnutLight);
            }
            Box(p, "Window_centre_mullion", new Vector3(0, .58f, .072f), new Vector3(.038f, .64f, .06f), P.WalnutDark);
            Box(p, "Sealant_sill", new Vector3(0, .192f, -.002f), new Vector3(1.36f, .045f, .23f), P.Taupe);
            for (int i = 0; i < 4; i++)
            {
                Vector3 a, b; SealEdge(i, out a, out b);
                var edge = New(p, stageNames[i], a, Quaternion.LookRotation((b - a).normalized));
                Box(edge, "Continuous_rubber_seal", new Vector3(0, 0, Vector3.Distance(a, b) * .5f), new Vector3(.017f, .019f, Vector3.Distance(a, b)), P.Rubber);
                Box(edge, "Fresh_silicone_bead", new Vector3(-.009f, -.007f, Vector3.Distance(a, b) * .5f), new Vector3(.006f, .008f, Vector3.Distance(a, b)), P.Taupe);
            }
            SealantGun(p);
        }

        static void FireJob(Transform p)
        {
            Box(p, "Extinguisher_wall_backplate", new Vector3(.06f, .59f, .13f), new Vector3(.55f, .78f, .035f), P.SteelDark);
            for (int i = 0; i < 4; i++) DynamicScrew(p, i, new Vector3(i < 2 ? -.18f : .30f, i % 2 == 0 ? .29f : .90f, .094f));
            Cyl(p, "Extinguisher_pressure_cylinder", new Vector3(.045f, .49f, -.005f), .15f, .61f, P.Red);
            Sphere(p, "Extinguisher_rounded_shoulder", new Vector3(.045f, .795f, -.005f), new Vector3(.149f, .072f, .149f), P.Red);
            Sphere(p, "Extinguisher_rounded_foot", new Vector3(.045f, .19f, -.005f), new Vector3(.147f, .045f, .147f), P.SteelDark);
            Cyl(p, "Brass_valve_neck", new Vector3(.045f, .86f, -.005f), .044f, .10f, P.Copper);
            Box(p, "Carrying_handle", new Vector3(.10f, .94f, -.003f), new Vector3(.26f, .037f, .045f), P.SteelDark, Quaternion.Euler(0, 0, -8));
            var lever = New(p, "JobMotion", new Vector3(-.047f, .934f, -.010f));
            Box(lever, "Pressure_test_lever", new Vector3(.125f, .034f, 0), new Vector3(.26f, .024f, .042f), P.Steel);
            var pin = New(p, "SecondaryMotion", new Vector3(-.06f, .89f, -.05f));
            Ring(pin, "Safety_pull_pin_ring", Vector3.zero, Vector3.forward, .039f, .005f, P.Copper);
            CylBetween(pin, "Actual_safety_pin", Vector3.zero, new Vector3(.12f, 0, 0), .007f, P.Steel);
            Box(p, "Safety_instruction_paper", new Vector3(.045f, .51f, -.156f), new Vector3(.18f, .25f, .009f), P.Paper);
            for (int i = 0; i < 5; i++) Box(p, "Instruction_rule", new Vector3(.045f, .47f + i * .031f, -.163f), new Vector3(i == 4 ? .09f : .137f, .008f, .003f), P.SteelDark);
            Box(p, "Retaining_clamp_band", new Vector3(.045f, .34f, -.17f), new Vector3(.38f, .045f, .035f), P.SteelDark);
            Tube(p, "Flexible_discharge_hose", new[] { new Vector3(.16f,.87f,-.01f), new Vector3(.29f,.82f,-.07f), new Vector3(.33f,.60f,-.09f), new Vector3(.34f,.32f,-.08f), new Vector3(.43f,.23f,-.10f) }, .016f, P.Rubber);
            CylBetween(p, "Conical_hose_nozzle", new Vector3(.43f, .23f, -.10f), new Vector3(.47f, .40f, -.10f), .026f, P.SteelDark);
            Gauge(p, new Vector3(-.037f, .85f, -.085f), .051f);
            FlowStream(p, new Vector3(.47f, .40f, -.10f), new Vector3(.25f, .45f, -.04f), true);
            Driver(p, true);
        }

        static void PlantJob(Transform p)
        {
            SoilBed(p);
            Sphere(p, "Excavated_planting_hole", new Vector3(.07f, .095f, .02f), new Vector3(.19f, .042f, .155f), P.Ink);
            for (int i = 0; i < 4; i++)
            {
                var mound = New(p, stageNames[i], new Vector3(-.43f + i * .28f, .13f, -.19f));
                Sphere(mound, "Dug_earth_clod", Vector3.zero, new Vector3(.105f, .065f, .08f), i % 2 == 0 ? P.Soil : P.WalnutDark);
            }
            var seedling = New(p, "JobMotion", new Vector3(.42f, .48f, .02f));
            Sphere(seedling, "Intact_root_ball", new Vector3(0, .025f, 0), new Vector3(.08f, .085f, .073f), P.Soil);
            CylBetween(seedling, "Young_tree_stem", new Vector3(0, .04f, 0), new Vector3(-.025f, .47f, .015f), .012f, P.Walnut);
            for (int i = 0; i < 7; i++)
            {
                float side = i % 2 == 0 ? -1 : 1;
                Vector3 a = new Vector3(-.01f, .18f + i * .041f, .006f);
                Vector3 b = a + new Vector3(side * (.13f - i * .008f), .08f, (i % 3 - 1) * .036f);
                CylBetween(seedling, "Leaf_petiol", a, b, .005f, P.Olive);
                Leaf(seedling, b, new Vector3(side * .08f, .035f, -.01f), .10f, i % 2 == 0 ? P.Leaf : P.Olive);
            }
            var fill = New(p, "SecondaryMotion", new Vector3(.07f, .11f, .02f));
            Ring(fill, "Firmed_soil_collar", Vector3.zero, Vector3.up, .15f, .032f, P.Soil);
            WateringCan(p);
            FlowStream(p, new Vector3(.23f, .43f, -.025f), new Vector3(-.16f, -.25f, .04f), false);
            Spade(p);
        }

        static void DrainJob(Transform p)
        {
            SoilBed(p);
            Box(p, "Dug_drainage_channel", new Vector3(0, .105f, .035f), new Vector3(1.38f, .03f, .25f), P.Ink);
            Box(p, "Drain_channel_wet_bottom", new Vector3(0, .119f, .035f), new Vector3(1.33f, .008f, .135f), P.Water);
            for (int side = -1; side <= 1; side += 2) for (int i = 0; i < 9; i++)
                Sphere(p, "Trench_lining_stone", new Vector3(-.66f + i * .165f, .145f, .035f + side * .15f), new Vector3(.078f, .035f, .055f), P.Stone);
            for (int i = 0; i < 4; i++)
            {
                var panel = New(p, stageNames[i], new Vector3(-.48f + i * .32f, .23f, .04f));
                for (int side = -1; side <= 1; side += 2)
                    Box(panel, "Grate_longitudinal_rail", new Vector3(0, 0, side * .11f), new Vector3(.30f, .018f, .023f), P.SteelDark);
                for (int slat = 0; slat < 6; slat++)
                    Box(panel, "Open_drain_grate_bar", new Vector3(-.125f + slat * .05f, 0, 0), new Vector3(.017f, .02f, .24f), P.Steel);
            }
            FlowStream(p, new Vector3(-.62f, .145f, .035f), new Vector3(1.22f, -.015f, 0), false);
            Spade(p);
        }

        static void FenceJob(Transform p)
        {
            var rail = New(p, "JobMotion", Vector3.zero);
            for (int side = -1; side <= 1; side += 2)
            {
                WoodenPanel(p, new Vector3(side * .51f, .56f, .13f), new Vector3(.13f, .84f, .12f));
                Box(rail, "Fence_crossrail", new Vector3(0, .40f + (side + 1) * .175f, .052f), new Vector3(1.20f, .095f, .08f), P.WalnutLight);
            }
            for (int i = 0; i < 5; i++)
            {
                float x = -.42f + i * .21f;
                Box(p, "Indigo_stained_picket", new Vector3(x, .57f, -.013f), new Vector3(.135f, .70f, .055f), i % 2 == 0 ? P.Indigo : P.WalnutDark);
                Box(p, "Picket_capped_tip", new Vector3(x, .932f, -.013f), new Vector3(.135f, .045f, .055f), P.Taupe);
                for (int row = 0; row < 2; row++) FixedScrew(p, new Vector3(x, .40f + row * .35f, -.048f), Quaternion.identity, .52f);
            }
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(i < 2 ? -.56f : .56f, i % 2 == 0 ? .40f : .75f, -.023f);
                Box(p, "Fence_corner_strap", pos + Vector3.forward * .013f, new Vector3(.12f, .15f, .025f), P.SteelDark);
                DynamicScrew(p, i, pos);
            }
            Driver(p, false);
        }

        static void HoseJob(Transform p)
        {
            Box(p, "Hose_reel_backplate", new Vector3(-.11f, .58f, .151f), new Vector3(.86f, .75f, .047f), P.Olive);
            for (int i = 0; i < 4; i++) DynamicScrew(p, i, new Vector3(i < 2 ? -.49f : .27f, i % 2 == 0 ? .27f : .89f, .112f));
            var reel = New(p, "JobMotion", new Vector3(-.11f, .58f, .008f));
            Ring(reel, "Reel_outer_wheel", Vector3.zero, Vector3.forward, .285f, .016f, P.SteelDark);
            for (int i = 0; i < 4; i++)
                Box(reel, "Reel_spoke", Vector3.zero, new Vector3(.55f, .026f, .023f), P.SteelDark, Quaternion.Euler(0, 0, i * 45));
            for (int turn = 0; turn < 4; turn++)
                Ring(reel, "Coiled_fire_hose", new Vector3(0, 0, -.025f - turn * .027f), Vector3.forward, .195f + turn * .015f, .017f, P.Rubber);
            Cyl(reel, "Reel_axle", new Vector3(0, 0, -.05f), .055f, .20f, P.Copper, FrontAxis);
            Box(reel, "Winding_handle_arm", new Vector3(.09f, 0, -.17f), new Vector3(.19f, .027f, .025f), P.Steel);
            Cyl(reel, "Winding_handle_grip", new Vector3(.17f, 0, -.205f), .021f, .09f, P.WalnutDark, FrontAxis);
            Tube(p, "Copper_supply_pipe", new[] { new Vector3(.47f,.14f,.09f),new Vector3(.47f,.66f,.09f),new Vector3(.40f,.72f,.06f),new Vector3(.25f,.72f,.06f) }, .025f, P.Copper);
            var valve = New(p, "ValveMotion", new Vector3(.47f, .48f, .024f));
            Ring(valve, "Valve_handwheel", Vector3.zero, Vector3.forward, .067f, .010f, P.Olive);
            Box(valve, "Valve_wheel_crossbar", new Vector3(0, 0, -.003f), new Vector3(.13f, .014f, .017f), P.Olive);
            Gauge(p, new Vector3(.47f, .65f, .012f), .056f);
            Cyl(p, "Hex_compression_coupler", new Vector3(.47f, .23f, .09f), .041f, .083f, P.Copper, Quaternion.identity, 6);
            Tube(p, "Loose_hose_tail", new[] { new Vector3(.06f,.40f,-.10f), new Vector3(.16f,.22f,-.11f), new Vector3(.36f,.16f,-.13f), new Vector3(.60f,.23f,-.11f) }, .019f, P.Rubber);
            CylBetween(p, "Garden_fire_nozzle", new Vector3(.59f,.23f,-.11f), new Vector3(.64f,.44f,-.11f), .031f, P.Copper);
            for (int i = 0; i < 4; i++) Ring(p, "Nozzle_grip_groove", new Vector3(.60f+i*.009f,.275f+i*.037f,-.11f), new Vector3(.05f,.21f,0), .033f, .004f, P.SteelDark);
            FlowStream(p, new Vector3(.64f, .44f, -.11f), new Vector3(.11f, .43f, -.04f), false);
            Driver(p, true);
        }

        static void DynamicScrew(Transform p, int index, Vector3 seat, Quaternion rotation = default(Quaternion))
        {
            if (rotation == default(Quaternion)) rotation = Quaternion.identity;
            var root = New(p, screwNames[index], seat, rotation);
            ScrewGeometry(root, 1, true);
            // Seat marker records the unextended rest pose for prefab clones.
            New(p, "Seat_" + index, seat, rotation);
        }
        static void FixedScrew(Transform p, Vector3 seat, Quaternion rot, float scale)
        {
            var root = New(p, "Seated_cross_recess_fastener", seat, rot);
            root.localScale = Vector3.one * scale;
            ScrewGeometry(root, scale, false);
        }
        static void ScrewGeometry(Transform p, float scale, bool threaded)
        {
            Cyl(p, "Screw_shaft", new Vector3(0, 0, .028f), .009f, .075f, P.SteelDark, FrontAxis, 12);
            if (threaded) for (int i = 0; i < 6; i++)
                Ring(p, "Visible_screw_thread", new Vector3(0, 0, .006f + i * .01f), Vector3.forward, .011f, .0023f, P.Steel);
            var head = New(p, "TurningHead", Vector3.zero);
            Cyl(head, "Recess_dark_bottom", new Vector3(0, 0, -.002f), .027f, .012f, P.Ink, FrontAxis, 20);
            Ring(head, "Machined_screw_head_rim", new Vector3(0, 0, -.010f), Vector3.forward, .024f, .005f, P.Steel);
            Mesh quadrant = ScrewQuadrant();
            for (int i = 0; i < 4; i++)
                MeshObject(head, "Physical_cross_slot_quadrant", quadrant, P.Steel, new Vector3(0, 0, -.010f), Quaternion.Euler(0, 0, i * 90), Vector3.one);
        }

        static void Driver(Transform p, bool wrench)
        {
            var tool = New(p, "AnimatedTool", Vector3.zero);
            if (wrench)
            {
                Box(tool, "Forged_wrench_handle", new Vector3(.15f, 0, -.014f), new Vector3(.27f, .046f, .019f), P.Steel);
                Box(tool, "Wrench_dark_grip_inlay", new Vector3(.18f, 0, -.027f), new Vector3(.13f, .026f, .008f), P.Indigo);
                Ring(tool, "Open_end_wrench_ring", new Vector3(0, 0, -.011f), Vector3.forward, .044f, .012f, P.Steel);
                Box(tool, "Wrench_jaw_left", new Vector3(-.036f, .032f, -.010f), new Vector3(.028f, .042f, .025f), P.Steel, Quaternion.Euler(0, 0, -18));
                Box(tool, "Wrench_jaw_right", new Vector3(.036f, .032f, -.010f), new Vector3(.028f, .042f, .025f), P.Steel, Quaternion.Euler(0, 0, 18));
                return;
            }
            Cyl(tool, "Magnetic_bit", new Vector3(0, 0, -.019f), .009f, .038f, P.SteelDark, FrontAxis, 8);
            Box(tool, "Cross_driver_tip_A", new Vector3(0, 0, .003f), new Vector3(.020f, .005f, .012f), P.Steel);
            Box(tool, "Cross_driver_tip_B", new Vector3(0, 0, .003f), new Vector3(.005f, .020f, .012f), P.Steel);
            Cyl(tool, "Satin_driver_shaft", new Vector3(0, 0, -.098f), .010f, .13f, P.Steel, FrontAxis, 12);
            Cyl(tool, "Copper_ferrule", new Vector3(0, 0, -.17f), .024f, .035f, P.Copper, FrontAxis);
            Cyl(tool, "Worn_indigo_driver_handle", new Vector3(0, 0, -.277f), .046f, .19f, P.Indigo, FrontAxis);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3;
                Box(tool, "Handle_rubber_flute", new Vector3(Mathf.Cos(a) * .043f, Mathf.Sin(a) * .043f, -.278f), new Vector3(.010f, .009f, .144f), P.Rubber, Quaternion.Euler(0, 0, i * 60));
            }
            Cyl(tool, "Driver_palm_cap", new Vector3(0, 0, -.382f), .039f, .025f, P.WalnutDark, FrontAxis);
        }
        static void SealantGun(Transform p)
        {
            var tool = New(p, "AnimatedTool", Vector3.zero);
            Cyl(tool, "Sealant_tapered_tip", new Vector3(0, 0, -.034f), .010f, .07f, P.Paper, FrontAxis, 12);
            Cyl(tool, "Silicone_cartridge", new Vector3(0, 0, -.19f), .046f, .25f, P.Paper, FrontAxis);
            Box(tool, "Cartridge_olive_label", new Vector3(0, -.043f, -.19f), new Vector3(.056f, .008f, .14f), P.Olive);
            for (int side = -1; side <= 1; side += 2)
                Box(tool, "Metal_gun_cradle", new Vector3(side * .049f, 0, -.195f), new Vector3(.013f, .027f, .29f), P.SteelDark);
            Box(tool, "Gun_trigger_grip", new Vector3(0, -.075f, -.32f), new Vector3(.042f, .14f, .047f), P.Indigo, Quaternion.Euler(-15, 0, 0));
            Box(tool, "Gun_trigger", new Vector3(0, -.071f, -.276f), new Vector3(.018f, .093f, .016f), P.Copper, Quaternion.Euler(18, 0, 0));
            Cyl(tool, "Piston_rod", new Vector3(0, 0, -.36f), .009f, .09f, P.Steel, FrontAxis);
        }
        static void Spade(Transform p)
        {
            var tool = New(p, "AnimatedTool", Vector3.zero);
            Box(tool, "Curved_spade_blade", new Vector3(0, .035f, .008f), new Vector3(.15f, .17f, .026f), P.SteelDark, Quaternion.Euler(-14, 0, 0));
            Box(tool, "Spade_central_rib", new Vector3(0, .053f, -.011f), new Vector3(.024f, .18f, .016f), P.Steel);
            CylBetween(tool, "Ash_spade_shaft", new Vector3(0, .11f, 0), new Vector3(0, .57f, .045f), .018f, P.WalnutLight);
            Ring(tool, "D_handle", new Vector3(0, .63f, .05f), Vector3.forward, .069f, .012f, P.Indigo);
            Box(tool, "Spade_palm_grip", new Vector3(0, .677f, .05f), new Vector3(.11f, .025f, .025f), P.WalnutDark);
        }
        static void SoilBed(Transform p)
        {
            Box(p, "Soil_section", new Vector3(0, .078f, .025f), new Vector3(1.48f, .09f, .59f), P.Soil);
            for (int i = 0; i < 23; i++)
            {
                float x = -.68f + (i * .173f) % 1.36f, z = -.23f + (i * .113f) % .48f;
                Sphere(p, "Soil_aggregate", new Vector3(x, .129f, z), new Vector3(.018f + i % 3 * .007f, .012f, .02f), i % 5 == 0 ? P.Stone : P.WalnutDark);
            }
            for (int i = 0; i < 9; i++)
                Between(p, "Edge_grass_blade", new Vector3(-.64f + i * .16f, .12f, .29f), new Vector3(-.66f + i * .16f, .22f + i % 3 * .015f, .28f), .009f, .003f, P.Olive);
        }
        static void WoodenPanel(Transform p, Vector3 position, Vector3 size)
        {
            Box(p, "Walnut_grain_panel", position, size, P.Walnut);
            for (int i = 0; i < 7; i++)
                Box(p, "Subtle_wood_grain", position + new Vector3(-size.x * .42f + size.x * i * .14f, 0, -size.z * .5f - .0015f), new Vector3(.003f, size.y * (.72f + i % 3 * .08f), .002f), i % 3 == 0 ? P.WalnutLight : P.WalnutDark, Quaternion.Euler(0, 0, i % 2 == 0 ? 1 : -1));
        }
        static void ScrewTray(Transform p, Vector3 pos)
        {
            var tray = New(p, "Reusable_parts_tray", pos);
            Box(tray, "Tray_bottom", Vector3.zero, new Vector3(.22f, .018f, .17f), P.SteelDark);
            for (int side = -1; side <= 1; side += 2)
            {
                Box(tray, "Tray_side", new Vector3(side * .107f, .018f, 0), new Vector3(.013f, .04f, .17f), P.Steel);
                Box(tray, "Tray_end", new Vector3(0, .018f, side * .077f), new Vector3(.22f, .04f, .013f), P.Steel);
            }
            for (int i = 0; i < 3; i++) FixedScrew(tray, new Vector3(-.06f + i * .057f, .026f, -.012f + i % 2 * .038f), Quaternion.LookRotation(Vector3.down, Vector3.forward), .49f);
        }
        static void FoldedCloth(Transform p, Vector3 pos)
        {
            for (int i = 0; i < 3; i++) Box(p, "Folded_oiled_rag", pos + Vector3.up * i * .008f, new Vector3(.21f - i * .013f, .009f, .17f), i == 2 ? P.Olive : P.Taupe, Quaternion.Euler(0, -7, 0));
        }
        static void Gauge(Transform p, Vector3 pos, float radius)
        {
            Cyl(p, "Pressure_gauge_bezel", pos, radius, .025f, P.Copper, FrontAxis);
            Cyl(p, "Pressure_gauge_face", pos + Vector3.back * .015f, radius * .84f, .004f, P.Paper, FrontAxis);
            for (int i = 0; i < 7; i++)
            {
                float a = (-140 + i * 46) * Mathf.Deg2Rad;
                Box(p, "Gauge_engraved_tick", pos + new Vector3(Mathf.Sin(a) * radius * .66f, Mathf.Cos(a) * radius * .66f, -.019f), new Vector3(.003f, .008f, .002f), P.Ink, Quaternion.Euler(0, 0, -a * Mathf.Rad2Deg));
            }
            var needle = New(p, "GaugeNeedlePivot", pos + Vector3.back * .023f);
            Box(needle, "Pressure_needle", new Vector3(0, radius * .22f, 0), new Vector3(.003f, radius * .65f, .002f), P.Red);
        }
        static void WateringCan(Transform p)
        {
            var can = New(p, "WateringCanMotion", new Vector3(.39f, .44f, -.025f));
            Cyl(can, "Teal_watering_can_body", Vector3.zero, .10f, .15f, P.Olive);
            Ring(can, "Open_filling_rim", new Vector3(0, .081f, 0), Vector3.up, .083f, .010f, P.Copper);
            Ring(can, "Carrying_loop", new Vector3(.10f, .04f, 0), Vector3.forward, .086f, .012f, P.Copper);
            CylBetween(can, "Angled_watering_spout", new Vector3(-.07f, -.035f, 0), new Vector3(-.23f, .045f, 0), .018f, P.Olive);
            Sphere(can, "Watering_rose", new Vector3(-.23f, .045f, 0), new Vector3(.032f, .023f, .032f), P.Copper);
        }

        static void FlowStream(Transform p, Vector3 origin, Vector3 direction, bool powder)
        {
            var flow = New(p, "InputFlow", origin);
            for (int i = 0; i < 9; i++)
            {
                float a = (i + 1) / 9f;
                Vector3 position = direction * a + Vector3.down * (a * a * .07f);
                float radius = powder ? .014f + a * .024f : .013f;
                Sphere(flow, powder ? "Test_powder_puff" : "Moving_water_droplet", position,
                    new Vector3(radius, radius * (powder ? 1 : 1.9f), radius), powder ? P.Paper : P.Water);
            }
        }

        static void Leaf(Transform p, Vector3 pos, Vector3 direction, float length, P material)
        {
            var mesh = Cached("Folded_leaf", () => PolygonMesh(new[] { new Vector2(0,0), new Vector2(.25f,.45f), new Vector2(0,1), new Vector2(-.25f,.45f) }, .035f));
            MeshObject(p, "Folded_seedling_leaf", mesh, material, pos, Quaternion.FromToRotation(Vector3.up, direction.normalized), Vector3.one * length);
        }

        static void SealEdge(int edge, out Vector3 a, out Vector3 b)
        {
            const float left = -.555f, right = .555f, low = .255f, high = .905f, z = -.001f;
            switch (edge)
            {
                case 0: a = new Vector3(left, low, z); b = new Vector3(left, high, z); break;
                case 1: a = new Vector3(left, high, z); b = new Vector3(right, high, z); break;
                case 2: a = new Vector3(right, high, z); b = new Vector3(right, low, z); break;
                default: a = new Vector3(right, low, z); b = new Vector3(left, low, z); break;
            }
        }
        static RigState ReadState(GameObject root, Job job)
        {
            var s = new RigState { Root = root, Job = job };
            var t = root.transform;
            s.Tool = t.Find("AnimatedTool"); s.Motion = t.Find("JobMotion"); s.Secondary = t.Find("SecondaryMotion");
            s.Gauge = t.Find("GaugeNeedlePivot"); s.Valve = t.Find("ValveMotion");
            s.Flow = t.Find("InputFlow"); s.WateringCan = t.Find("WateringCanMotion");
            if (s.Motion) { s.MotionRest = s.Motion.localPosition; s.MotionRotation = s.Motion.localRotation; }
            // Initial rig creation may already have applied the unseated pose before a
            // prefab is baked. These parts have an authored zero rest position.
            if (job == Job.Upgrade || job == Job.Fence) s.MotionRest = Vector3.zero;
            if (s.Secondary) s.SecondaryRest = s.Secondary.localPosition;
            for (int i = 0; i < 4; i++)
            {
                s.Screws[i] = t.Find(screwNames[i]); s.Stages[i] = t.Find(stageNames[i]);
                if (s.Screws[i]) s.Heads[i] = s.Screws[i].Find("TurningHead");
                var seat = t.Find("Seat_" + i);
                if (seat) s.ScrewSeats[i] = seat.localPosition;
                else if (s.Screws[i]) s.ScrewSeats[i] = s.Screws[i].localPosition;
            }
            return s;
        }
        static void TrimDeadStates()
        {
            List<int> dead = null;
            foreach (var pair in rigs) if (!pair.Value.Root) { if (dead == null) dead = new List<int>(); dead.Add(pair.Key); }
            if (dead != null) for (int i = 0; i < dead.Count; i++) rigs.Remove(dead[i]);
        }
        static Job Parse(string action)
        {
            switch ((action ?? "anchorwall").ToLowerInvariant())
            {
                case "anchorfloor": return Job.AnchorFloor;
                case "upgrade": return Job.Upgrade;
                case "flood": return Job.Flood;
                case "fire": return Job.Fire;
                case "gardenplant": case "plant": return Job.Plant;
                case "gardendrain": case "drain": return Job.Drain;
                case "gardenfence": case "fence": return Job.Fence;
                case "gardenhose": case "hose": return Job.Hose;
                default: return Job.AnchorWall;
            }
        }
        static Job JobFromName(string name)
        {
            if (name.Contains("AnchorFloor")) return Job.AnchorFloor;
            if (name.Contains("Upgrade")) return Job.Upgrade;
            if (name.Contains("Flood")) return Job.Flood;
            if (name.Contains("Fire")) return Job.Fire;
            if (name.Contains("Plant")) return Job.Plant;
            if (name.Contains("Drain")) return Job.Drain;
            if (name.Contains("Fence")) return Job.Fence;
            if (name.Contains("Hose")) return Job.Hose;
            return Job.AnchorWall;
        }
        static bool Finite(float v) { return !float.IsNaN(v) && !float.IsInfinity(v); }
        static float Safe(float v, float min, float max, float fallback) { return Finite(v) ? Mathf.Clamp(v, min, max) : fallback; }

        static void Init()
        {
            if (palette != null && palette[0]) return;
            Shader shader = Shader.Find("Standard");
            if (!shader) shader = Shader.Find("Legacy Shaders/Diffuse");
            if (!shader) throw new InvalidOperationException("PanicWorkArt requires the Built-In Standard shader.");
            Color[] colors = {
                new Color(.045f,.051f,.056f), new Color(.42f,.46f,.47f), new Color(.17f,.20f,.22f),
                new Color(.45f,.27f,.16f), new Color(.25f,.17f,.12f), new Color(.37f,.27f,.18f),
                new Color(.14f,.105f,.08f), new Color(.14f,.20f,.28f), new Color(.40f,.37f,.32f),
                new Color(.46f,.45f,.40f), new Color(.26f,.31f,.20f), new Color(.29f,.40f,.22f),
                new Color(.18f,.135f,.095f), new Color(.31f,.33f,.33f), new Color(.07f,.085f,.078f),
                new Color(.42f,.13f,.105f), new Color(.66f,.62f,.50f), new Color(.61f,.40f,.17f),
                new Color(.14f,.23f,.26f), new Color(.18f,.27f,.32f) };
            palette = new Material[colors.Length];
            for (int i = 0; i < palette.Length; i++)
            {
                P kind = (P)i;
                var material = new Material(shader) { name = "PanicWork_" + kind, color = colors[i] };
                bool metal = kind == P.Steel || kind == P.SteelDark || kind == P.Copper;
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metal ? .72f : 0);
                if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", metal ? .38f : kind == P.Glass ? .58f : .16f);
                palette[i] = material;
            }
        }
        static Transform New(Transform parent, string name, Vector3 pos, Quaternion rot = default(Quaternion))
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localRotation = rot == default(Quaternion) ? Quaternion.identity : rot;
            return go.transform;
        }
        static Transform MeshObject(Transform p, string name, Mesh mesh, P mat, Vector3 pos, Quaternion rot, Vector3 size)
        {
            var t = New(p, name, pos, rot); t.localScale = size;
            t.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = t.gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial = palette[(int)mat];
            renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true;
            return t;
        }
        static void Box(Transform p, string name, Vector3 pos, Vector3 size, P mat, Quaternion rot = default(Quaternion))
        { MeshObject(p, name, Cached("Bevel_box", BevelBoxMesh), mat, pos, rot == default(Quaternion) ? Quaternion.identity : rot, size); }
        static void Between(Transform p, string name, Vector3 a, Vector3 b, float width, float depth, P mat)
        { Box(p, name, (a + b) * .5f, new Vector3(width, Vector3.Distance(a,b), depth), mat, Quaternion.FromToRotation(Vector3.up, (b-a).normalized)); }
        static void Cyl(Transform p, string name, Vector3 pos, float radius, float length, P mat, Quaternion rot = default(Quaternion), int sides = 20)
        { MeshObject(p, name, Cached("Cylinder_" + sides, () => CylinderMesh(sides)), mat, pos, rot == default(Quaternion) ? Quaternion.identity : rot, new Vector3(radius, length, radius)); }
        static void CylBetween(Transform p, string name, Vector3 a, Vector3 b, float radius, P mat)
        { Cyl(p, name, (a+b)*.5f, radius, Vector3.Distance(a,b), mat, Quaternion.FromToRotation(Vector3.up,(b-a).normalized), 12); }
        static void Sphere(Transform p, string name, Vector3 pos, Vector3 radius, P mat)
        { MeshObject(p, name, Cached("Ellipsoid_12_8", SphereMesh), mat, pos, Quaternion.identity, radius); }
        static void Ring(Transform p, string name, Vector3 pos, Vector3 normal, float radius, float tube, P mat)
        {
            string key = "Torus_" + (tube/radius).ToString("R", CultureInfo.InvariantCulture);
            MeshObject(p, name, Cached(key, () => TorusMesh(tube/radius)), mat, pos, Quaternion.LookRotation(normal.normalized), Vector3.one * radius);
        }
        static void Tube(Transform p, string name, Vector3[] points, float radius, P mat)
        { MeshObject(p, name, TubeMesh(points, radius), mat, Vector3.zero, Quaternion.identity, Vector3.one); }
        static Mesh Cached(string key, Func<Mesh> make)
        {
            Mesh mesh;
            if (!meshes.TryGetValue(key, out mesh) || !mesh) { mesh = make(); mesh.name = "PanicWork_" + key; meshes[key] = mesh; }
            return mesh;
        }
        static Mesh Finish(string name, List<Vector3> v, List<int> tris)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(v); mesh.SetTriangles(tris,0);
            var uv = new List<Vector2>(v.Count); for (int i=0;i<v.Count;i++) uv.Add(new Vector2(v[i].x,v[i].y));
            mesh.SetUVs(0,uv); mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents();
            return mesh;
        }
        static void Face(List<Vector3> v, List<int> t, params Vector3[] points)
        {
            int s=v.Count; v.AddRange(points);
            for(int i=1;i<points.Length-1;i++) { t.Add(s);t.Add(s+i);t.Add(s+i+1); }
        }
        static Mesh BevelBoxMesh()
        {
            var v=new List<Vector3>();var t=new List<int>(); const float h=.5f,b=.43f;
            // Central faces, followed by twelve chamfer strips and eight clipped corners.
            for(int axis=0;axis<3;axis++) for(int sign=-1;sign<=1;sign+=2)
            {
                Vector3 n=Axis(axis)*sign,u=Axis((axis+1)%3),w=Axis((axis+2)%3);
                if(sign<0) u=-u;
                Face(v,t,n*h-u*b-w*b,n*h+u*b-w*b,n*h+u*b+w*b,n*h-u*b+w*b);
            }
            for(int axis=0;axis<3;axis++) for(int s1=-1;s1<=1;s1+=2) for(int s2=-1;s2<=1;s2+=2)
            {
                Vector3 along=Axis(axis)*b,a=Axis((axis+1)%3)*s1,c=Axis((axis+2)%3)*s2;
                Vector3 p0=a*h+c*b-along,p1=a*h+c*b+along,p2=a*b+c*h+along,p3=a*b+c*h-along;
                if(Vector3.Dot(Vector3.Cross(p1-p0,p2-p0),a+c)<0) Face(v,t,p3,p2,p1,p0); else Face(v,t,p0,p1,p2,p3);
            }
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
            {
                Vector3 a=new Vector3(x*h,y*b,z*b),c=new Vector3(x*b,y*h,z*b),d=new Vector3(x*b,y*b,z*h);
                if(Vector3.Dot(Vector3.Cross(c-a,d-a),new Vector3(x,y,z))<0)Face(v,t,d,c,a);else Face(v,t,a,c,d);
            }
            return Finish("Chamfered_physical_box",v,t);
        }
        static Vector3 Axis(int i) { return i==0?Vector3.right:i==1?Vector3.up:Vector3.forward; }
        static Mesh CylinderMesh(int sides)
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                Vector3 p0=new Vector3(Mathf.Cos(a),-.5f,Mathf.Sin(a)),p1=new Vector3(Mathf.Cos(b),-.5f,Mathf.Sin(b));
                Face(v,t,p0,p0+Vector3.up,p1+Vector3.up,p1);
                Face(v,t,new Vector3(0,.5f,0),p1+Vector3.up,p0+Vector3.up);
                Face(v,t,new Vector3(0,-.5f,0),p0,p1);
            }
            return Finish("Closed_machined_cylinder",v,t);
        }
        static Mesh SphereMesh()
        {
            var v=new List<Vector3>();var t=new List<int>(); const int rows=8,cols=12;
            for(int y=0;y<=rows;y++)for(int x=0;x<=cols;x++)
            { float a=x*Mathf.PI*2/cols,b=y*Mathf.PI/rows;v.Add(new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a))); }
            for(int y=0;y<rows;y++)for(int x=0;x<cols;x++)
            {int i=y*(cols+1)+x;t.Add(i);t.Add(i+1);t.Add(i+cols+1);t.Add(i+1);t.Add(i+cols+2);t.Add(i+cols+1);}
            return Finish("Soft_ellipsoid",v,t);
        }
        static Mesh TorusMesh(float ratio)
        {
            var v=new List<Vector3>();var t=new List<int>();const int major=24,minor=6;
            for(int a=0;a<=major;a++)for(int b=0;b<=minor;b++)
            {float u=a*Mathf.PI*2/major,w=b*Mathf.PI*2/minor,r=1+Mathf.Cos(w)*ratio;v.Add(new Vector3(Mathf.Cos(u)*r,Mathf.Sin(u)*r,Mathf.Sin(w)*ratio));}
            for(int a=0;a<major;a++)for(int b=0;b<minor;b++)
            {int i=a*(minor+1)+b;t.Add(i);t.Add(i+minor+1);t.Add(i+1);t.Add(i+1);t.Add(i+minor+1);t.Add(i+minor+2);}
            return Finish("Machined_ring",v,t);
        }
        static Mesh TubeMesh(Vector3[] points,float radius)
        {
            var v=new List<Vector3>();var t=new List<int>();const int sides=10;
            for(int i=0;i<points.Length;i++)
            {
                Vector3 tangent=(points[Mathf.Min(i+1,points.Length-1)]-points[Mathf.Max(0,i-1)]).normalized;
                Vector3 right=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.9f?Vector3.forward:Vector3.up).normalized;
                Vector3 up=Vector3.Cross(right,tangent).normalized;
                for(int j=0;j<sides;j++) {float a=j*Mathf.PI*2/sides;v.Add(points[i]+(right*Mathf.Cos(a)+up*Mathf.Sin(a))*radius);}
            }
            for(int i=0;i<points.Length-1;i++)for(int j=0;j<sides;j++)
            {int a=i*sides+j,b=i*sides+(j+1)%sides,c=a+sides,d=b+sides;t.Add(a);t.Add(c);t.Add(b);t.Add(b);t.Add(c);t.Add(d);}
            // End caps avoid hollow-looking disconnected hose ends.
            int first=v.Count;v.Add(points[0]);int last=v.Count;v.Add(points[points.Length-1]);
            for(int j=0;j<sides;j++){t.Add(first);t.Add(j);t.Add((j+1)%sides);t.Add(last);t.Add((points.Length-1)*sides+(j+1)%sides);t.Add((points.Length-1)*sides+j);}
            return Finish("Authored_bent_hose",v,t);
        }
        static Mesh ScrewQuadrant()
        {
            return Cached("Recessed_screw_quadrant", () =>
            {
                var p=new List<Vector2>();const float r=.020f,s=.0037f;
                p.Add(new Vector2(s,s));
                float start=Mathf.Asin(s/r),end=Mathf.Acos(s/r);
                for(int i=0;i<=6;i++){float a=Mathf.Lerp(start,end,i/6f);p.Add(new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r));}
                return PolygonMesh(p.ToArray(),.006f);
            });
        }
        static Mesh PolygonMesh(Vector2[] points,float depth)
        {
            var v=new List<Vector3>();var t=new List<int>();
            // Convex polygon extrusion; front is -Z and uses reversed winding.
            var front=new Vector3[points.Length];var back=new Vector3[points.Length];
            for(int i=0;i<points.Length;i++){front[i]=new Vector3(points[points.Length-1-i].x,points[points.Length-1-i].y,-depth*.5f);back[i]=new Vector3(points[i].x,points[i].y,depth*.5f);}
            Face(v,t,front);Face(v,t,back);
            for(int i=0;i<points.Length;i++){Vector2 a=points[i],b=points[(i+1)%points.Length];Face(v,t,new Vector3(a.x,a.y,-depth*.5f),new Vector3(b.x,b.y,-depth*.5f),new Vector3(b.x,b.y,depth*.5f),new Vector3(a.x,a.y,depth*.5f));}
            return Finish("Extruded_authored_profile",v,t);
        }
    }
}
