using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TooLateToPanic
{
    // Optional root-owned headless Editor hook. Temporary authored art/camera fixtures only;
    // never starts a scene/player, renders, sends input or touches Model/Expedition/live objects.
    public static class PanicManualWorkLabelsEditorProbeV59
    {
        [Serializable] public sealed class Sample
        {
            public string action; public float progress; public Rect hit, instruction, closedBadge;
            public Vector2 instructionMeasured, closedMeasured, closedPoint;
        }
        [Serializable] public sealed class Receipt
        {
            public string layer = "HEADLESS_EDITOR_ACTUAL_ART_PROJECTION_FONT_METRICS_AND_PURE_LAYOUT";
            public string render = "NOT_RUN", osInput = "NOT_RUN", humanAcceptance = "NOT_RUN";
            public bool passed = true; public int checks, states;
            public List<string> failures = new List<string>();
            public List<Sample> samples = new List<Sample>();
        }
        static Vector2 Project(Camera camera, Rect fitted, Vector3 p)
        { Vector3 v = camera.WorldToViewportPoint(p); return new Vector2(fitted.x + v.x * fitted.width, fitted.y + (1f - v.y) * fitted.height); }
        public static Receipt Probe()
        {
            var r = new Receipt();
            void Check(string id, bool pass) { r.checks++; r.passed &= pass; if (!pass) r.failures.Add(id); }
            Font font = Resources.Load<Font>("HUD/BarlowCondensed");
            Check("real-offline-font", font);
            if (!font) return r;
            var style = new GUIStyle { font = font, fontSize = 21, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            Vector2 closedMeasured = style.CalcSize(new GUIContent("KAPALI KONUM"));
            // Same production1100x700 target texture fitted into(90,225,1006,554), without allocating a render target.
            float scale = Mathf.Min(1006f / 1100f, 554f / 700f);
            Vector2 size = new Vector2(1100f, 700f) * scale;
            Rect fitted = new Rect(new Vector2(593f, 502f) - size * .5f, size);
            Rect content = Rect.MinMaxRect(fitted.xMin + 12f, fitted.yMin + 12f, fitted.xMax - 12f, fitted.yMax - 12f);
            GameObject cameraObject = new GameObject("V59 isolated projection fixture camera");
            Camera camera = cameraObject.AddComponent<Camera>(); camera.enabled = false; camera.orthographic = true;
            camera.orthographicSize = .82f; camera.aspect = 1100f / 700f;
            camera.transform.position = new Vector3(100, 101.6f, 98.3f); camera.transform.LookAt(new Vector3(100, 100.35f, 100));
            try
            {
                foreach (string action in new[] { "poweroff", "gasoff", "wateroff" })
                {
                    GameObject rig = PanicWorkArt.CreateWorkRig(action); rig.transform.position = new Vector3(100, 100, 100);
                    try
                    {
                        string text = action == "poweroff" ? "AŞAĞI İNDİR" : "90° SAAT YÖNÜ";
                        Vector2 instructionMeasured = style.CalcSize(new GUIContent(text));
                        for (int step = 0; step <= 90; step++)
                        {
                            float progress = step / 90f; PanicWorkArt.UpdateWorkRig(rig, progress, 0, 0);
                            bool gripPoint = PanicWorkArt.GetClickTargetPosition(rig, 0, out var gripWorld);
                            bool closedPoint = PanicWorkArt.GetControlClosedPosition(rig, out var closedWorld);
                            bool points = gripPoint && closedPoint;
                            Check(action + ".actual-points." + step, points); if (!points) continue;
                            Vector2 grip = Project(camera, fitted, gripWorld), closed = Project(camera, fitted, closedWorld);
                            Rect hit = new Rect(grip.x - 44, grip.y - 44, 88, 88);
                            bool laid = PanicManualWorkLabelPolicyV59.TryLayout(content, hit, closed,
                                PanicManualWorkLabelPolicyV59.BadgeSize(instructionMeasured), PanicManualWorkLabelPolicyV59.BadgeSize(closedMeasured), out var layout);
                            Check(action + ".layout." + step, laid); if (!laid) continue;
                            Check(action + ".nonoverlap." + step, !PanicManualWorkLabelPolicyV59.Inflate(layout.Instruction, 8).Overlaps(layout.ClosedBadge) &&
                                !layout.Instruction.Overlaps(layout.ProtectedGrip) && !layout.ClosedBadge.Overlaps(layout.ProtectedGrip) &&
                                !layout.Instruction.Overlaps(layout.ProtectedClosed) && !layout.ClosedBadge.Overlaps(layout.ProtectedClosed) &&
                                PanicManualWorkLabelPolicyV59.Contains(content, layout.Instruction) && PanicManualWorkLabelPolicyV59.Contains(content, layout.ClosedBadge));
                            Check(action + ".measured-text-fit." + step, layout.Instruction.width >= instructionMeasured.x + 27 && layout.Instruction.height >= instructionMeasured.y &&
                                layout.ClosedBadge.width >= closedMeasured.x + 27 && layout.ClosedBadge.height >= closedMeasured.y);
                            foreach (int width in new[] { 960, 1280, 1600, 1920 })
                            {
                                float canvasScale = width / 1600f;
                                Check(action + ".physical-fit." + step + "." + width, 21f * canvasScale >= 12.59f &&
                                    layout.Instruction.xMin * canvasScale >= 0 && layout.Instruction.xMax * canvasScale <= width &&
                                    layout.ClosedBadge.xMin * canvasScale >= 0 && layout.ClosedBadge.xMax * canvasScale <= width &&
                                    layout.Instruction.yMax * canvasScale <= width * 9f / 16f && layout.ClosedBadge.yMax * canvasScale <= width * 9f / 16f);
                            }
                            r.states++;
                            if (step == 0 || step == 45 || step == 90) r.samples.Add(new Sample { action = action, progress = progress, hit = hit, closedPoint = closed,
                                instruction = layout.Instruction, closedBadge = layout.ClosedBadge, instructionMeasured = instructionMeasured, closedMeasured = closedMeasured });
                        }
                    }
                    finally { UnityEngine.Object.DestroyImmediate(rig); }
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(cameraObject); }
            return r;
        }
        public static void Run()
        {
            if (!Application.isEditor || !Application.isBatchMode) throw new InvalidOperationException("Headless Editor only; not a player component.");
            string path = null; string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--work-labels-v59-probe-out") path = args[i + 1];
            if (string.IsNullOrEmpty(path) || !Path.IsPathRooted(path)) throw new ArgumentException("Explicit absolute receipt path required.");
            Receipt result = Probe(); File.WriteAllText(path, JsonUtility.ToJson(result, true));
            if (!result.passed) throw new InvalidOperationException("Work-label policy probe failed; see receipt.");
            Debug.Log("WORK_LABELS_V59_HEADLESS_POLICY_PASS; render/OS/human NOT_RUN");
        }
    }
}
