using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>
    /// Milestone 5 [P2.1]: 12 Varied Disaster Surface Marks & Decals.
    /// Procedural 3D surface marks replacing legacy LineRenderer fractures.
    /// Provides 12 distinct variants across Earthquake, Fire, and Flood/Storm.
    /// Manages lifecycle teardown via PanicExteriorOwnedArt to prevent asset/memory leaks.
    /// </summary>
    public static class PanicDamageDecalArt
    {
        // -----------------------------------------------------------------------------------------
        // Color Palette Constants
        // -----------------------------------------------------------------------------------------
        // Quake Spalled Patch
        public static readonly Color ColorQuakeTerracottaCore = new Color(0.4706f, 0.2314f, 0.1569f, 1.0f); // #783B28
        public static readonly Color ColorQuakePlasterRim     = new Color(0.4314f, 0.3961f, 0.3725f, 1.0f); // #6E655F
        public static readonly Color ColorQuakePlasterMat     = new Color(0.29f, 0.27f, 0.25f, 1.0f);       // #4A4540

        // Fire Carbon & Char
        public static readonly Color ColorFireCarbonCore    = new Color(0.067f, 0.067f, 0.067f); // #111111
        public static readonly Color ColorFireIntenseChar   = new Color(0.051f, 0.047f, 0.043f); // #0D0C0B
        public static readonly Color ColorFireScorchedUmber = new Color(0.165f, 0.118f, 0.078f); // #2A1E14
        public static readonly Color ColorFireEmberAsh      = new Color(0.239f, 0.208f, 0.180f); // #3D352E
        public static readonly Color ColorFirePowderyAsh    = new Color(0.290f, 0.263f, 0.239f); // #4A433D
        public static readonly Color ColorFireGlowingEmber  = new Color(0.769f, 0.251f, 0.055f); // #C4400E

        // Flood Mud & Silt
        public static readonly Color ColorFloodWater       = new Color(0.18f, 0.28f, 0.32f, 0.65f);
        public static readonly Color ColorFloodMud         = new Color(0.20f, 0.14f, 0.09f, 0.98f);
        public static readonly Color ColorFloodTideLine    = new Color(0.26f, 0.20f, 0.14f, 0.95f);
        public static readonly Color ColorFloodSilt        = new Color(0.35f, 0.28f, 0.19f, 0.95f);

        // -----------------------------------------------------------------------------------------
        // Primary Factory Interface
        // -----------------------------------------------------------------------------------------
        public static GameObject CreateDamageMark(DisasterType disaster, int variantIndex, Vector3 surfaceSize, Transform parent)
        {
            // 1. Sanitize surface size to prevent zero or NaN geometry (T2_R5_3)
            float w = Mathf.Max(0.2f, Mathf.Abs(surfaceSize.x));
            float h = Mathf.Max(0.2f, Mathf.Abs(surfaceSize.y));
            float d = Mathf.Max(0.1f, Mathf.Abs(surfaceSize.z));
            Vector3 safeSize = new Vector3(w, h, d);

            // 2. Wrap variant index strictly to [0..3] (T2_R5_2)
            int variant = ((variantIndex % 4) + 4) % 4;

            // 3. Fallback for unknown/unsupported disaster enum values (T2_R5_5)
            DisasterType resolvedDisaster = disaster;
            if (resolvedDisaster != DisasterType.Earthquake &&
                resolvedDisaster != DisasterType.Fire &&
                resolvedDisaster != DisasterType.Flood &&
                resolvedDisaster != DisasterType.Storm)
            {
                resolvedDisaster = DisasterType.Earthquake;
            }

            // 4. Create root GameObject with unique variant name (T1_R5_1..3)
            string markName = GetMarkName(resolvedDisaster, variant);
            var root = new GameObject(markName);

            // 5. Safe parenting (T2_R5_4)
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            // 6. Attach PanicExteriorOwnedArt for leak-free teardown
            var owned = root.AddComponent<PanicExteriorOwnedArt>();

            // 7. Dispatch procedural mesh and material generation
            switch (resolvedDisaster)
            {
                case DisasterType.Fire:
                    CreateFireMark(root, variant, safeSize, owned);
                    break;

                case DisasterType.Flood:
                case DisasterType.Storm:
                    CreateFloodMark(root, variant, safeSize, owned);
                    break;

                case DisasterType.Earthquake:
                default:
                    CreateEarthquakeMark(root, variant, safeSize, owned);
                    break;
            }

            return root;
        }

        public static string GetMarkName(DisasterType disaster, int variant)
        {
            switch (disaster)
            {
                case DisasterType.Fire:
                    switch (variant)
                    {
                        case 0: return "DamageMark_Fire_Var0_VPlume";
                        case 1: return "DamageMark_Fire_Var1_CharredBlister";
                        case 2: return "DamageMark_Fire_Var2_AshScatterRing";
                        case 3: return "DamageMark_Fire_Var3_ThermalSootStreak";
                        default: return "DamageMark_Fire_Var0_VPlume";
                    }

                case DisasterType.Flood:
                case DisasterType.Storm:
                    switch (variant)
                    {
                        case 0: return "DamageMark_Flood_TideLine";
                        case 1: return "DamageMark_Flood_MudSplatter";
                        case 2: return "DamageMark_Flood_ReflectivePuddle";
                        case 3: return "DamageMark_Flood_DoorwaySilt";
                        default: return "DamageMark_Flood_TideLine";
                    }

                case DisasterType.Earthquake:
                default:
                    switch (variant)
                    {
                        case 0: return "DamageMark_Quake_RadialWeb";
                        case 1: return "DamageMark_Quake_SteppedMasonry";
                        case 2: return "DamageMark_Quake_ForkedShear";
                        case 3: return "DamageMark_Quake_SpalledPatch";
                        default: return "DamageMark_Quake_RadialWeb";
                    }
            }
        }

        // =========================================================================================
        // EARTHQUAKE CRACK VARIANTS (4 VARIANTS)
        // =========================================================================================

        static void CreateEarthquakeMark(GameObject root, int variant, Vector3 size, PanicExteriorOwnedArt owned)
        {
            float w = size.x;
            float h = size.y;

            switch (variant)
            {
                case 0:
                    BuildQuakeRadialWeb(root, w, h, owned);
                    break;
                case 1:
                    BuildQuakeSteppedMasonry(root, w, h, owned);
                    break;
                case 2:
                    BuildQuakeForkedShear(root, w, h, owned);
                    break;
                case 3:
                default:
                    BuildQuakeSpalledPatch(root, w, h, owned);
                    break;
            }
        }

        // Variant 0: Radial Web Plaster Crack
        static void BuildQuakeRadialWeb(GameObject root, float w, float h, PanicExteriorOwnedArt owned)
        {
            var mat = CreateStandardMaterial("DamageMat_Quake_Plaster", ColorQuakePlasterMat, 0.12f, 0.0f, owned);

            var v = new List<Vector3>();
            var tr = new List<int>();
            var uv = new List<Vector2>();
            var col = new List<Color>();

            float minDim = Mathf.Min(w, h);
            Vector3 center = new Vector3(HashRange(101, -w * 0.06f, w * 0.06f), HashRange(102, -h * 0.06f, h * 0.06f), -0.0015f);

            // 1. Central crushed impact crater (8-point fan)
            int centerIdx = v.Count;
            v.Add(center);
            uv.Add(new Vector2(0.5f, 0.5f));
            col.Add(new Color(0.12f, 0.11f, 0.10f, 1f));

            int craterSides = 8;
            float craterR = minDim * 0.025f;
            int ringStart = v.Count;
            for (int i = 0; i < craterSides; i++)
            {
                float a = i * Mathf.PI * 2f / craterSides;
                Vector3 p = center + new Vector3(Mathf.Cos(a) * craterR * HashRange(i * 13, 0.8f, 1.2f),
                                                 Mathf.Sin(a) * craterR * HashRange(i * 17, 0.8f, 1.2f), 0f);
                v.Add(p);
                uv.Add(ToUV(p, w, h));
                col.Add(new Color(0.20f, 0.19f, 0.17f, 1f));

                int next = (i + 1) % craterSides;
                AddTri(tr, centerIdx, ringStart + i, ringStart + next);
            }

            // 2. 7 Radiating Spokes
            int spokes = 7;
            var spokeNodePositions = new List<Vector3>[spokes];
            for (int s = 0; s < spokes; s++)
            {
                spokeNodePositions[s] = new List<Vector3>();
                float baseAngle = (s * Mathf.PI * 2f / spokes) + HashRange(s * 19, -0.16f, 0.16f);
                float maxLen = minDim * HashRange(s * 23, 0.38f, 0.46f);

                Vector3 prev = center;
                spokeNodePositions[s].Add(prev);
                int steps = 4;
                for (int step = 1; step <= steps; step++)
                {
                    float frac = (float)step / steps;
                    float curAngle = baseAngle + HashRange(s * 31 + step * 7, -0.15f, 0.15f);
                    float curR = maxLen * (frac + HashRange(s * 41 + step * 11, -0.04f, 0.04f));
                    Vector3 cur = center + new Vector3(Mathf.Cos(curAngle) * curR, Mathf.Sin(curAngle) * curR, 0f);
                    spokeNodePositions[s].Add(cur);

                    float widthA = minDim * Mathf.Lerp(0.024f, 0.005f, (float)(step - 1) / steps);
                    float widthB = minDim * Mathf.Lerp(0.024f, 0.005f, frac);
                    AddRibbon(v, tr, uv, col, prev, cur, widthA, widthB, w, h,
                        new Color(0.18f, 0.17f, 0.16f, 1f), new Color(0.38f, 0.36f, 0.33f, 1f));

                    // Micro-fork branching near outer tips
                    if (step == 3 && Hash(s * 53 + step) > 0.25f)
                    {
                        float forkAngle = curAngle + (s % 2 == 0 ? 0.55f : -0.55f);
                        Vector3 forkEnd = cur + new Vector3(Mathf.Cos(forkAngle), Mathf.Sin(forkAngle), 0f) * (minDim * 0.12f);
                        AddRibbon(v, tr, uv, col, cur, forkEnd, widthB, minDim * 0.003f, w, h,
                            new Color(0.22f, 0.20f, 0.19f, 1f), new Color(0.42f, 0.40f, 0.37f, 0.7f));
                    }
                    prev = cur;
                }
            }

            // 3. Concentric Transverse Web Arcs (2 rings)
            int webRings = 2;
            for (int r = 1; r <= webRings; r++)
            {
                int nodeIdx = r + 1;
                for (int s = 0; s < spokes; s++)
                {
                    if (Hash(s * 67 + r * 83) < 0.15f) continue; // Natural brittle gap

                    int nextSpoke = (s + 1) % spokes;
                    Vector3 pA = spokeNodePositions[s][nodeIdx];
                    Vector3 pB = spokeNodePositions[nextSpoke][nodeIdx];

                    // Catenary sag mid-point
                    Vector3 mid = (pA + pB) * 0.5f;
                    Vector3 sag = (center - mid).normalized * (minDim * 0.02f);
                    Vector3 pMid = mid + sag;

                    float arcWidth = minDim * (r == 1 ? 0.012f : 0.008f);
                    AddRibbon(v, tr, uv, col, pA, pMid, arcWidth, arcWidth, w, h,
                        new Color(0.22f, 0.20f, 0.19f, 1f), new Color(0.40f, 0.38f, 0.35f, 1f));
                    AddRibbon(v, tr, uv, col, pMid, pB, arcWidth, arcWidth, w, h,
                        new Color(0.22f, 0.20f, 0.19f, 1f), new Color(0.40f, 0.38f, 0.35f, 1f));
                }
            }

            var mesh = CompileMesh("DecalMesh_Quake_RadialWeb", v, tr, uv, col, owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        // Variant 1: Stepped Masonry Crack
        static void BuildQuakeSteppedMasonry(GameObject root, float w, float h, PanicExteriorOwnedArt owned)
        {
            var mat = CreateStandardMaterial("DamageMat_Quake_Plaster", ColorQuakePlasterMat, 0.12f, 0.0f, owned);

            var v = new List<Vector3>();
            var tr = new List<int>();
            var uv = new List<Vector2>();
            var col = new List<Color>();

            float minDim = Mathf.Min(w, h);
            float courseH = h / 7.0f;
            float brickW = w / 5.2f;

            Vector3 cur = new Vector3(-w * 0.40f, -h * 0.42f, -0.0015f);
            int courses = 6;

            for (int c = 0; c < courses; c++)
            {
                // Horizontal bed joint step
                float bedSpan = brickW * HashRange(c * 23 + 1, 0.82f, 1.25f);
                Vector3 bedEnd = cur + new Vector3(bedSpan, HashRange(c * 31, -0.004f, 0.004f), 0f);
                float bedWidth = minDim * HashRange(c * 17, 0.020f, 0.028f);

                AddRibbon(v, tr, uv, col, cur, bedEnd, bedWidth, bedWidth, w, h,
                    new Color(0.16f, 0.15f, 0.14f, 1f), new Color(0.48f, 0.45f, 0.41f, 1f));

                // Head joint vertical step
                float headHeight = courseH * HashRange(c * 29 + 3, 0.88f, 1.15f);
                Vector3 headEnd = bedEnd + new Vector3(HashRange(c * 43, -0.003f, 0.003f), headHeight, 0f);
                float headWidth = minDim * HashRange(c * 37, 0.022f, 0.030f);

                AddRibbon(v, tr, uv, col, bedEnd, headEnd, headWidth, headWidth, w, h,
                    new Color(0.16f, 0.15f, 0.14f, 1f), new Color(0.48f, 0.45f, 0.41f, 1f));

                // Secondary horizontal mortar relief branch
                if (c % 2 == 1)
                {
                    Vector3 branchEnd = bedEnd + new Vector3(-brickW * 0.65f, 0f, 0f);
                    AddRibbon(v, tr, uv, col, bedEnd, branchEnd, headWidth * 0.7f, minDim * 0.004f, w, h,
                        new Color(0.20f, 0.19f, 0.18f, 1f), new Color(0.50f, 0.47f, 0.43f, 0.8f));
                }

                // Crumbled mortar chip at step corner
                if (c % 2 == 0)
                {
                    Vector3 chipCorner = bedEnd + new Vector3(minDim * 0.015f, -minDim * 0.012f, 0f);
                    AddTriangle(v, tr, uv, col, bedEnd, bedEnd + Vector3.up * (minDim * 0.02f), chipCorner, w, h,
                        new Color(0.55f, 0.52f, 0.48f, 1f));
                }

                cur = headEnd;
            }

            var mesh = CompileMesh("DecalMesh_Quake_SteppedMasonry", v, tr, uv, col, owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        // Variant 2: Forked Shear Crack
        static void BuildQuakeForkedShear(GameObject root, float w, float h, PanicExteriorOwnedArt owned)
        {
            var mat = CreateStandardMaterial("DamageMat_Quake_Plaster", ColorQuakePlasterMat, 0.12f, 0.0f, owned);

            var v = new List<Vector3>();
            var tr = new List<int>();
            var uv = new List<Vector2>();
            var col = new List<Color>();

            float minDim = Mathf.Min(w, h);
            Vector3 start = new Vector3(-w * 0.44f, -h * 0.38f, -0.0015f);
            Vector3 end = new Vector3(w * 0.42f, h * 0.40f, -0.0015f);

            // Primary Shear Fault with central dilation and en-echelon jog
            int segments = 8;
            Vector3 prev = start;
            var faultPoints = new List<Vector3> { start };
            var faultWidths = new List<float> { minDim * 0.015f };

            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 p = Vector3.Lerp(start, end, t);

                // Stochastic lateral wander
                p.x += (Hash(i * 19 + 7) - 0.5f) * (minDim * 0.08f);
                p.y += (Hash(i * 31 + 13) - 0.5f) * (minDim * 0.06f);

                // En-echelon fault step-over at segment 4
                if (i == 4)
                {
                    p += new Vector3(-0.035f * minDim, 0.040f * minDim, 0f);
                }

                float width = minDim * (0.016f + 0.046f * Mathf.Sin(t * Mathf.PI));
                faultPoints.Add(p);
                faultWidths.Add(width);

                AddRibbon(v, tr, uv, col, prev, p, faultWidths[i - 1], width, w, h,
                    new Color(0.12f, 0.11f, 0.10f, 1f), new Color(0.36f, 0.34f, 0.31f, 1f));
                prev = p;
            }

            // 5 Acute Riedel Shear Branches
            float[] branchFracs = { 0.22f, 0.38f, 0.58f, 0.72f, 0.85f };
            float[] branchAngles = { 0.48f, -0.52f, 0.45f, -0.48f, 0.55f };
            float[] branchLens = { 0.20f, 0.16f, 0.25f, 0.18f, 0.14f };

            for (int b = 0; b < branchFracs.Length; b++)
            {
                int node = Mathf.Clamp(Mathf.RoundToInt(branchFracs[b] * segments), 1, segments - 1);
                Vector3 nodeP = faultPoints[node];
                Vector3 faultDir = (faultPoints[node + 1] - faultPoints[node - 1]).normalized;

                // Rotate fault direction by acute branch angle
                float rad = branchAngles[b];
                Vector3 branchDir = new Vector3(
                    faultDir.x * Mathf.Cos(rad) - faultDir.y * Mathf.Sin(rad),
                    faultDir.x * Mathf.Sin(rad) + faultDir.y * Mathf.Cos(rad), 0f).normalized;

                Vector3 bMid = nodeP + branchDir * (minDim * branchLens[b] * 0.5f);
                Vector3 bEnd = nodeP + branchDir * (minDim * branchLens[b]);

                float startW = faultWidths[node] * 0.6f;
                float midW = startW * 0.5f;
                float endW = minDim * 0.003f;

                AddRibbon(v, tr, uv, col, nodeP, bMid, startW, midW, w, h,
                    new Color(0.18f, 0.17f, 0.16f, 1f), new Color(0.40f, 0.38f, 0.35f, 1f));
                AddRibbon(v, tr, uv, col, bMid, bEnd, midW, endW, w, h,
                    new Color(0.24f, 0.22f, 0.21f, 1f), new Color(0.45f, 0.42f, 0.39f, 0.7f));
            }

            var mesh = CompileMesh("DecalMesh_Quake_ForkedShear", v, tr, uv, col, owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        // Variant 3: Spalled Plaster/Brick Patch (Dual-material submesh)
        static void BuildQuakeSpalledPatch(GameObject root, float w, float h, PanicExteriorOwnedArt owned)
        {
            var plasterMat = CreateStandardMaterial("DamageMat_Quake_PlasterRim", ColorQuakePlasterRim, 0.10f, 0.0f, owned);
            var brickMat   = CreateStandardMaterial("DamageMat_Quake_BrickCore", ColorQuakeTerracottaCore, 0.18f, 0.02f, owned);

            var v = new List<Vector3>();
            var trPlaster = new List<int>();
            var trBrick = new List<int>();
            var uv = new List<Vector2>();
            var col = new List<Color>();

            float minDim = Mathf.Min(w, h);
            Vector3 center = new Vector3(HashRange(201, -w * 0.04f, w * 0.04f), HashRange(202, -h * 0.04f, h * 0.04f), 0f);

            int perimeterCount = 12;
            float rx = w * 0.26f;
            float ry = h * 0.22f;

            // 1. Inner Exposed Terracotta Brick Core (Submesh 1)
            int centerIdx = v.Count;
            Vector3 coreCenter = center + new Vector3(0f, 0f, 0.0025f); // Recessed into substrate
            v.Add(coreCenter);
            uv.Add(ToUV(coreCenter, w, h));
            col.Add(ColorQuakeTerracottaCore);

            int innerStart = v.Count;
            var innerPts = new Vector3[perimeterCount];
            for (int j = 0; j < perimeterCount; j++)
            {
                float phi = j * Mathf.PI * 2f / perimeterCount;
                float rScale = HashRange(j * 17 + 5, 0.78f, 1.22f);
                innerPts[j] = center + new Vector3(Mathf.Cos(phi) * rx * rScale, Mathf.Sin(phi) * ry * rScale, 0.001f);
                v.Add(innerPts[j]);
                uv.Add(ToUV(innerPts[j], w, h));
                col.Add(ColorQuakeTerracottaCore);

                int next = (j + 1) % perimeterCount;
                AddTri(trBrick, centerIdx, innerStart + j, innerStart + next);
            }

            // 2. Bevelled Plaster Crumble Perimeter Rim (Submesh 0)
            int outerStart = v.Count;
            var outerPts = new Vector3[perimeterCount];
            for (int j = 0; j < perimeterCount; j++)
            {
                Vector3 radial = (innerPts[j] - center).normalized;
                float crumbleW = minDim * HashRange(j * 29 + 11, 0.035f, 0.065f);
                outerPts[j] = innerPts[j] + radial * crumbleW + new Vector3(0f, 0f, -0.0025f); // Stands proud
                v.Add(outerPts[j]);
                uv.Add(ToUV(outerPts[j], w, h));
                col.Add(ColorQuakePlasterRim);

                int next = (j + 1) % perimeterCount;
                // Bevel quad between inner core perimeter and outer crumble rim
                AddQuad(trPlaster, innerStart + j, innerStart + next, outerStart + next, outerStart + j);
            }

            // 3. Detached Plaster Shard Chips
            for (int s = 0; s < 5; s++)
            {
                int j = (s * 2 + 1) % perimeterCount;
                Vector3 shardA = outerPts[j];
                Vector3 shardB = shardA + (shardA - center).normalized * (minDim * 0.035f);
                Vector3 shardC = shardA + new Vector3(minDim * 0.02f, -minDim * 0.015f, 0f);
                AddTriangle(v, trPlaster, uv, col, shardA, shardB, shardC, w, h, ColorQuakePlasterRim);
            }

            // 4. Radiating Perimeter Relief Cracks into Plaster
            int[] crackNodes = { 1, 4, 7, 10 };
            for (int k = 0; k < crackNodes.Length; k++)
            {
                int node = crackNodes[k];
                Vector3 cStart = outerPts[node];
                Vector3 cDir = (cStart - center).normalized;
                cDir = Quaternion.Euler(0, 0, HashRange(k * 31, -25f, 25f)) * cDir;
                Vector3 cEnd = cStart + cDir * (minDim * 0.14f);

                AddRibbon(v, trPlaster, uv, col, cStart, cEnd, minDim * 0.009f, minDim * 0.002f, w, h,
                    new Color(0.20f, 0.19f, 0.18f, 1f), new Color(0.48f, 0.46f, 0.42f, 0.7f));
            }

            var mesh = new Mesh { name = "DecalMesh_Quake_SpalledPatch" };
            mesh.vertices = v.ToArray();
            mesh.subMeshCount = 2;
            mesh.SetTriangles(trPlaster.ToArray(), 0);
            mesh.SetTriangles(trBrick.ToArray(), 1);
            mesh.uv = uv.ToArray();
            mesh.colors = col.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            owned.Meshes.Add(mesh);

            var mf = root.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = root.AddComponent<MeshRenderer>();
            mr.sharedMaterials = new Material[] { plasterMat, brickMat };
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = true;
        }

        // =========================================================================================
        // FIRE SOOT & CHAR VARIANTS (4 VARIANTS)
        // =========================================================================================

        static void CreateFireMark(GameObject root, int variant, Vector3 size, PanicExteriorOwnedArt owned)
        {
            switch (variant)
            {
                case 0:
                    BuildFireVPlume(root, size, owned);
                    break;
                case 1:
                    BuildFireCharredBlister(root, size, owned);
                    break;
                case 2:
                    BuildFireAshScatterRing(root, size, owned);
                    break;
                case 3:
                default:
                    BuildFireThermalSootStreak(root, size, owned);
                    break;
            }
        }

        // Variant 0: Upward V-Plume Smoke Stain
        static void BuildFireVPlume(GameObject root, Vector3 size, PanicExteriorOwnedArt owned)
        {
            float w = Mathf.Clamp(Mathf.Abs(size.x) * 0.70f, 0.35f, 1.20f);
            float h = Mathf.Clamp(Mathf.Abs(size.y) * 0.85f, 0.45f, 1.80f);

            const int rows = 8;
            const int cols = 7;
            var vertices = new Vector3[rows * cols];
            var colors = new Color[rows * cols];
            var uvs = new Vector2[rows * cols];
            var triangles = new List<int>((rows - 1) * (cols - 1) * 6);

            for (int r = 0; r < rows; r++)
            {
                float ty = (float)r / (rows - 1);
                float curW = w * Mathf.Lerp(0.12f, 0.82f, Mathf.Pow(ty, 0.70f));
                float vNotch = (ty > 0.65f) ? Mathf.Sin(ty * Mathf.PI) * 0.08f * h : 0f;

                for (int c = 0; c < cols; c++)
                {
                    float s = ((float)c / (cols - 1)) * 2f - 1f; // -1 .. +1
                    float x = s * (curW * 0.5f) + Mathf.Sin(ty * 7f + s * 3f) * 0.012f * w;
                    float y = ty * h - (1f - s * s) * vNotch;
                    float z = -0.004f;

                    int idx = r * cols + c;
                    vertices[idx] = new Vector3(x, y, z);
                    uvs[idx] = new Vector2((s + 1f) * 0.5f, ty);

                    float dx = Mathf.Abs(s);
                    float verticalFade = (ty > 0.80f) ? (1f - (ty - 0.80f) / 0.20f) : 1f;
                    if (r == 0) verticalFade *= 0.5f;

                    Color col;
                    float alpha;
                    if (dx < 0.35f)
                    {
                        col = Color.Lerp(ColorFireCarbonCore, ColorFireScorchedUmber, dx / 0.35f);
                        alpha = 0.95f * (1f - (dx / 0.35f) * 0.28f) * verticalFade;
                    }
                    else
                    {
                        col = Color.Lerp(ColorFireScorchedUmber, ColorFireEmberAsh, (dx - 0.35f) / 0.65f);
                        alpha = 0.68f * (1f - (dx - 0.35f) / 0.65f) * verticalFade;
                    }
                    if (c == 0 || c == cols - 1 || r == rows - 1) alpha = 0.0f;

                    col.a = Mathf.Clamp01(alpha);
                    colors[idx] = col;
                }
            }

            for (int r = 0; r < rows - 1; r++)
            {
                for (int c = 0; c < cols - 1; c++)
                {
                    int v0 = r * cols + c;
                    int v1 = v0 + 1;
                    int v2 = (r + 1) * cols + c;
                    int v3 = v2 + 1;

                    triangles.Add(v0); triangles.Add(v2); triangles.Add(v1);
                    triangles.Add(v1); triangles.Add(v2); triangles.Add(v3);
                }
            }

            var mesh = new Mesh { name = "Mesh_Fire_Var0_VPlume", vertices = vertices, colors = colors, uv = uvs, triangles = triangles.ToArray() };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            owned.Meshes.Add(mesh);

            var mat = CreateFireMaterial("Mat_Fire_VPlume", owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        // Variant 1: Charred Blister Patch
        static void BuildFireCharredBlister(GameObject root, Vector3 size, PanicExteriorOwnedArt owned)
        {
            float rad = Mathf.Clamp(Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.y)) * 0.45f, 0.18f, 0.75f);
            const int sectors = 20;

            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            // Center vertex
            vertices.Add(new Vector3(0, 0, -0.003f));
            colors.Add(new Color(ColorFireIntenseChar.r, ColorFireIntenseChar.g, ColorFireIntenseChar.b, 0.98f));
            uvs.Add(new Vector2(0.5f, 0.5f));

            // Inner ring
            int innerStart = vertices.Count;
            for (int i = 0; i < sectors; i++)
            {
                float ang = i * Mathf.PI * 2f / sectors;
                float r = rad * 0.48f * (1f + 0.14f * Mathf.Sin(3f * ang) + 0.10f * Mathf.Cos(5f * ang));
                vertices.Add(new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, -0.003f));
                colors.Add(new Color(ColorFireScorchedUmber.r, ColorFireScorchedUmber.g, ColorFireScorchedUmber.b, 0.78f));
                uvs.Add(new Vector2(0.5f + Mathf.Cos(ang) * 0.25f, 0.5f + Mathf.Sin(ang) * 0.25f));
            }

            // Outer ring
            int outerStart = vertices.Count;
            for (int i = 0; i < sectors; i++)
            {
                float ang = i * Mathf.PI * 2f / sectors;
                float r = rad * 0.95f * (1f + 0.20f * Mathf.Sin(4f * ang) + 0.12f * Mathf.Cos(7f * ang));
                vertices.Add(new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, -0.003f));
                colors.Add(new Color(ColorFireEmberAsh.r, ColorFireEmberAsh.g, ColorFireEmberAsh.b, 0.00f));
                uvs.Add(new Vector2(0.5f + Mathf.Cos(ang) * 0.5f, 0.5f + Mathf.Sin(ang) * 0.5f));
            }

            // Fan to inner ring
            for (int i = 0; i < sectors; i++)
            {
                int next = (i + 1) % sectors;
                triangles.Add(0);
                triangles.Add(innerStart + i);
                triangles.Add(innerStart + next);
            }

            // Ring between inner and outer
            for (int i = 0; i < sectors; i++)
            {
                int next = (i + 1) % sectors;
                int i0 = innerStart + i;
                int i1 = innerStart + next;
                int o0 = outerStart + i;
                int o1 = outerStart + next;

                triangles.Add(i0); triangles.Add(o0); triangles.Add(i1);
                triangles.Add(i1); triangles.Add(o0); triangles.Add(o1);
            }

            // Raised Blister Chips (Alligatoring Scales)
            Vector2[] chipCenters = new[]
            {
                new Vector2(0, 0),
                new Vector2(rad * 0.22f, rad * 0.05f),
                new Vector2(-rad * 0.20f, rad * 0.12f),
                new Vector2(-rad * 0.15f, -rad * 0.18f),
                new Vector2(rad * 0.18f, -rad * 0.16f),
                new Vector2(rad * 0.02f, rad * 0.24f),
                new Vector2(-rad * 0.05f, -rad * 0.25f)
            };

            for (int c = 0; c < chipCenters.Length; c++)
            {
                Vector2 cp = chipCenters[c];
                float chipSize = rad * (0.09f + (c % 3) * 0.025f);
                int baseIdx = vertices.Count;

                Vector3 p0 = new Vector3(cp.x - chipSize, cp.y - chipSize, -0.0055f);
                Vector3 p1 = new Vector3(cp.x + chipSize, cp.y - chipSize * 0.9f, -0.0050f);
                Vector3 p2 = new Vector3(cp.x + chipSize * 0.9f, cp.y + chipSize, -0.0060f);
                Vector3 p3 = new Vector3(cp.x - chipSize * 0.95f, cp.y + chipSize * 0.85f, -0.0052f);

                vertices.Add(p0); vertices.Add(p1); vertices.Add(p2); vertices.Add(p3);
                Color chipCol = new Color(ColorFireCarbonCore.r * 0.9f, ColorFireCarbonCore.g * 0.9f, ColorFireCarbonCore.b * 0.9f, 0.98f);
                colors.Add(chipCol); colors.Add(chipCol); colors.Add(chipCol); colors.Add(chipCol);
                uvs.Add(new Vector2(0.4f, 0.4f)); uvs.Add(new Vector2(0.6f, 0.4f)); uvs.Add(new Vector2(0.6f, 0.6f)); uvs.Add(new Vector2(0.4f, 0.6f));

                triangles.Add(baseIdx); triangles.Add(baseIdx + 1); triangles.Add(baseIdx + 2);
                triangles.Add(baseIdx); triangles.Add(baseIdx + 2); triangles.Add(baseIdx + 3);
            }

            var mesh = new Mesh { name = "Mesh_Fire_Var1_CharredBlister", vertices = vertices.ToArray(), colors = colors.ToArray(), uv = uvs.ToArray(), triangles = triangles.ToArray() };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            owned.Meshes.Add(mesh);

            var mat = CreateFireMaterial("Mat_Fire_CharredBlister", owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        // Variant 2: Floor Ash/Ember Scatter Ring
        static void BuildFireAshScatterRing(GameObject root, Vector3 size, PanicExteriorOwnedArt owned)
        {
            float rOut = Mathf.Clamp(Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.y)) * 0.48f, 0.20f, 0.80f);
            float rMid = rOut * 0.65f;
            float rIn  = rOut * 0.32f;
            const int segs = 16;

            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            // Center ash vertex
            vertices.Add(new Vector3(0, 0, -0.004f));
            colors.Add(new Color(ColorFirePowderyAsh.r, ColorFirePowderyAsh.g, ColorFirePowderyAsh.b, 0.45f));
            uvs.Add(new Vector2(0.5f, 0.5f));

            // Ring 1: Inner Ash perimeter
            int inStart = vertices.Count;
            for (int i = 0; i < segs; i++)
            {
                float ang = i * Mathf.PI * 2f / segs;
                vertices.Add(new Vector3(Mathf.Cos(ang) * rIn, Mathf.Sin(ang) * rIn, -0.004f));
                colors.Add(new Color(ColorFireScorchedUmber.r, ColorFireScorchedUmber.g, ColorFireScorchedUmber.b, 0.68f));
                uvs.Add(new Vector2(0.5f + Mathf.Cos(ang) * 0.16f, 0.5f + Mathf.Sin(ang) * 0.16f));
            }

            // Ring 2: Scorch Annulus Ridge
            int midStart = vertices.Count;
            for (int i = 0; i < segs; i++)
            {
                float ang = i * Mathf.PI * 2f / segs;
                float r = rMid * (1f + 0.12f * Mathf.Sin(3f * ang) + 0.08f * Mathf.Cos(5f * ang));
                vertices.Add(new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, -0.004f));
                colors.Add(new Color(ColorFireCarbonCore.r, ColorFireCarbonCore.g, ColorFireCarbonCore.b, 0.96f));
                uvs.Add(new Vector2(0.5f + Mathf.Cos(ang) * 0.35f, 0.5f + Mathf.Sin(ang) * 0.35f));
            }

            // Ring 3: Outer Feathered Dispersion
            int outStart = vertices.Count;
            for (int i = 0; i < segs; i++)
            {
                float ang = i * Mathf.PI * 2f / segs;
                float r = rOut * (1f + 0.15f * Mathf.Sin(4f * ang) + 0.10f * Mathf.Cos(6f * ang));
                vertices.Add(new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, -0.004f));
                colors.Add(new Color(ColorFireEmberAsh.r, ColorFireEmberAsh.g, ColorFireEmberAsh.b, 0.00f));
                uvs.Add(new Vector2(0.5f + Mathf.Cos(ang) * 0.5f, 0.5f + Mathf.Sin(ang) * 0.5f));
            }

            // Ash Fan
            for (int i = 0; i < segs; i++)
            {
                triangles.Add(0);
                triangles.Add(inStart + i);
                triangles.Add(inStart + (i + 1) % segs);
            }

            // Scorch Ring Band
            for (int i = 0; i < segs; i++)
            {
                int next = (i + 1) % segs;
                int i0 = inStart + i, i1 = inStart + next;
                int m0 = midStart + i, m1 = midStart + next;

                triangles.Add(i0); triangles.Add(m0); triangles.Add(i1);
                triangles.Add(i1); triangles.Add(m0); triangles.Add(m1);
            }

            // Outer Halo Band
            for (int i = 0; i < segs; i++)
            {
                int next = (i + 1) % segs;
                int m0 = midStart + i, m1 = midStart + next;
                int o0 = outStart + i, o1 = outStart + next;

                triangles.Add(m0); triangles.Add(o0); triangles.Add(m1);
                triangles.Add(m1); triangles.Add(o0); triangles.Add(o1);
            }

            // Charcoal Fragments & Glowing Embers
            for (int k = 0; k < 8; k++)
            {
                float ang = k * Mathf.PI * 2f / 8f + 0.2f * Mathf.Sin(k * 1.7f);
                float dist = Mathf.Lerp(rIn, rMid, 0.45f + (k % 3) * 0.22f);
                Vector2 cp = new Vector2(Mathf.Cos(ang) * dist, Mathf.Sin(ang) * dist);
                float chipSize = rOut * 0.045f;
                int baseIdx = vertices.Count;

                vertices.Add(new Vector3(cp.x - chipSize, cp.y - chipSize, -0.0055f));
                vertices.Add(new Vector3(cp.x + chipSize, cp.y - chipSize, -0.0055f));
                vertices.Add(new Vector3(cp.x + chipSize * 0.8f, cp.y + chipSize, -0.0055f));
                vertices.Add(new Vector3(cp.x - chipSize * 0.8f, cp.y + chipSize, -0.0055f));

                Color emberCol = (k == 1 || k == 5)
                    ? new Color(ColorFireGlowingEmber.r, ColorFireGlowingEmber.g, ColorFireGlowingEmber.b, 0.95f)
                    : new Color(ColorFireIntenseChar.r, ColorFireIntenseChar.g, ColorFireIntenseChar.b, 0.98f);

                colors.Add(emberCol); colors.Add(emberCol); colors.Add(emberCol); colors.Add(emberCol);
                uvs.Add(new Vector2(0.5f, 0.5f)); uvs.Add(new Vector2(0.5f, 0.5f)); uvs.Add(new Vector2(0.5f, 0.5f)); uvs.Add(new Vector2(0.5f, 0.5f));

                triangles.Add(baseIdx); triangles.Add(baseIdx + 1); triangles.Add(baseIdx + 2);
                triangles.Add(baseIdx); triangles.Add(baseIdx + 2); triangles.Add(baseIdx + 3);
            }

            var mesh = new Mesh { name = "Mesh_Fire_Var2_AshScatterRing", vertices = vertices.ToArray(), colors = colors.ToArray(), uv = uvs.ToArray(), triangles = triangles.ToArray() };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            owned.Meshes.Add(mesh);

            var mat = CreateFireMaterial("Mat_Fire_AshScatterRing", owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        // Variant 3: Thermal Soot Streak
        static void BuildFireThermalSootStreak(GameObject root, Vector3 size, PanicExteriorOwnedArt owned)
        {
            float len = Mathf.Clamp(Mathf.Abs(size.x) * 0.85f, 0.40f, 1.60f);
            float totalH = Mathf.Clamp(Mathf.Abs(size.y) * 0.45f, 0.20f, 0.80f);

            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            // 1. Primary Central Flow Ribbon (10 segments, 3 vertices per segment)
            const int segs = 10;
            int mainStart = vertices.Count;
            for (int i = 0; i < segs; i++)
            {
                float tx = (float)i / (segs - 1);
                float x = Mathf.Lerp(-len * 0.5f, len * 0.5f, tx);
                float midY = Mathf.Sin(tx * Mathf.PI * 2f) * totalH * 0.12f;
                float halfW = (totalH * 0.5f) * Mathf.Lerp(0.20f, 0.55f, Mathf.Sin(tx * Mathf.PI));

                float edgeFade = Mathf.Sin(tx * Mathf.PI);
                Color coreCol = new Color(ColorFireCarbonCore.r, ColorFireCarbonCore.g, ColorFireCarbonCore.b, 0.88f * edgeFade);
                Color marginCol = new Color(ColorFireEmberAsh.r, ColorFireEmberAsh.g, ColorFireEmberAsh.b, 0.00f);

                vertices.Add(new Vector3(x, midY - halfW, -0.004f)); colors.Add(marginCol); uvs.Add(new Vector2(tx, 0f));
                vertices.Add(new Vector3(x, midY,         -0.004f)); colors.Add(coreCol);   uvs.Add(new Vector2(tx, 0.5f));
                vertices.Add(new Vector3(x, midY + halfW, -0.004f)); colors.Add(marginCol); uvs.Add(new Vector2(tx, 1f));
            }

            for (int i = 0; i < segs - 1; i++)
            {
                int b0 = mainStart + i * 3;
                int b1 = mainStart + (i + 1) * 3;

                // Lower strip
                triangles.Add(b0);     triangles.Add(b1);     triangles.Add(b0 + 1);
                triangles.Add(b0 + 1); triangles.Add(b1);     triangles.Add(b1 + 1);

                // Upper strip
                triangles.Add(b0 + 1); triangles.Add(b1 + 1); triangles.Add(b0 + 2);
                triangles.Add(b0 + 2); triangles.Add(b1 + 1); triangles.Add(b1 + 2);
            }

            // 2. Upper Flanking Wisp Ribbon
            BuildWispRibbon(vertices, colors, uvs, triangles, len, totalH, +totalH * 0.32f, 0.15f, 0.85f, 0.50f, 1.2f);

            // 3. Lower Flanking Wisp Ribbon
            BuildWispRibbon(vertices, colors, uvs, triangles, len, totalH, -totalH * 0.30f, 0.25f, 0.95f, 0.42f, 2.4f);

            var mesh = new Mesh { name = "Mesh_Fire_Var3_ThermalSootStreak", vertices = vertices.ToArray(), colors = colors.ToArray(), uv = uvs.ToArray(), triangles = triangles.ToArray() };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            owned.Meshes.Add(mesh);

            var mat = CreateFireMaterial("Mat_Fire_ThermalSootStreak", owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        static void BuildWispRibbon(List<Vector3> verts, List<Color> cols, List<Vector2> uvList, List<int> tris,
            float len, float totalH, float yOffset, float tStart, float tEnd, float maxAlpha, float phase)
        {
            const int wispSegs = 6;
            int baseIdx = verts.Count;

            for (int i = 0; i < wispSegs; i++)
            {
                float tLocal = (float)i / (wispSegs - 1);
                float tx = Mathf.Lerp(tStart, tEnd, tLocal);
                float x = Mathf.Lerp(-len * 0.5f, len * 0.5f, tx);
                float midY = yOffset + Mathf.Sin(tx * Mathf.PI * 3f + phase) * totalH * 0.08f;
                float halfW = Mathf.Max(0.002f, totalH * 0.14f * Mathf.Sin(tLocal * Mathf.PI));

                Color wispCore = new Color(ColorFireScorchedUmber.r, ColorFireScorchedUmber.g, ColorFireScorchedUmber.b, maxAlpha * Mathf.Sin(tLocal * Mathf.PI));
                Color wispMargin = new Color(ColorFireEmberAsh.r, ColorFireEmberAsh.g, ColorFireEmberAsh.b, 0.00f);

                verts.Add(new Vector3(x, midY - halfW, -0.004f)); cols.Add(wispMargin); uvList.Add(new Vector2(tx, 0f));
                verts.Add(new Vector3(x, midY,         -0.004f)); cols.Add(wispCore);   uvList.Add(new Vector2(tx, 0.5f));
                verts.Add(new Vector3(x, midY + halfW, -0.004f)); cols.Add(wispMargin); uvList.Add(new Vector2(tx, 1f));
            }

            for (int i = 0; i < wispSegs - 1; i++)
            {
                int b0 = baseIdx + i * 3;
                int b1 = baseIdx + (i + 1) * 3;

                tris.Add(b0);     tris.Add(b1);     tris.Add(b0 + 1);
                tris.Add(b0 + 1); tris.Add(b1);     tris.Add(b1 + 1);
                tris.Add(b0 + 1); tris.Add(b1 + 1); tris.Add(b0 + 2);
                tris.Add(b0 + 2); tris.Add(b1 + 1); tris.Add(b1 + 2);
            }
        }

        // =========================================================================================
        // FLOOD & STORM SURFACE MARKS (4 VARIANTS)
        // =========================================================================================

        static void CreateFloodMark(GameObject root, int variant, Vector3 size, PanicExteriorOwnedArt owned)
        {
            switch (variant)
            {
                case 0:
                    BuildFloodTideLine(root, size, owned);
                    break;
                case 1:
                    BuildFloodMudSplatter(root, size, owned);
                    break;
                case 2:
                    BuildFloodReflectivePuddle(root, size, owned);
                    break;
                case 3:
                default:
                    BuildFloodDoorwaySilt(root, size, owned);
                    break;
            }
        }

        // Variant 0: Horizontal muddy tide line
        static void BuildFloodTideLine(GameObject root, Vector3 size, PanicExteriorOwnedArt owned)
        {
            float width = Mathf.Min(1.6f, size.x * 0.85f);
            float height = Mathf.Min(1.2f, size.y * 0.75f);

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var tris = new List<int>();

            const int segments = 16;
            float dx = width / segments;
            float xStart = -width * 0.5f;

            // Waterline horizontal ribbon
            for (int i = 0; i <= segments; i++)
            {
                float x = xStart + i * dx;
                float normX = (float)i / segments;
                float wave = Mathf.Sin(normX * Mathf.PI * 4f) * 0.035f + Mathf.Cos(normX * Mathf.PI * 8f) * 0.015f;
                float yTop = height * 0.45f + wave;
                float yBot = 0.0f;

                verts.Add(new Vector3(x, yBot, -0.002f));
                verts.Add(new Vector3(x, yTop, -0.002f));

                normals.Add(Vector3.back);
                normals.Add(Vector3.back);

                uvs.Add(new Vector2(normX, 0f));
                uvs.Add(new Vector2(normX, 1f));

                colors.Add(new Color(0.24f, 0.18f, 0.12f, 0.95f)); // Dense sediment at bottom
                colors.Add(new Color(0.28f, 0.22f, 0.15f, 0.70f)); // Feathered watermark at top
            }

            for (int i = 0; i < segments; i++)
            {
                int b = i * 2;
                tris.Add(b); tris.Add(b + 1); tris.Add(b + 3);
                tris.Add(b); tris.Add(b + 3); tris.Add(b + 2);
            }

            // Downward drip tails
            float[] dripXOffsets = { -width * 0.35f, -width * 0.12f, width * 0.08f, width * 0.32f };
            float[] dripLengths = { height * 0.38f, height * 0.55f, height * 0.30f, height * 0.48f };

            for (int d = 0; d < dripXOffsets.Length; d++)
            {
                float cx = dripXOffsets[d];
                float len = dripLengths[d];
                float dripW = 0.035f;

                int baseIdx = verts.Count;
                verts.Add(new Vector3(cx - dripW, 0f, -0.003f));
                verts.Add(new Vector3(cx + dripW, 0f, -0.003f));
                verts.Add(new Vector3(cx, -len, -0.003f));

                normals.Add(Vector3.back); normals.Add(Vector3.back); normals.Add(Vector3.back);
                uvs.Add(new Vector2(0f, 0f)); uvs.Add(new Vector2(1f, 0f)); uvs.Add(new Vector2(0.5f, 1f));

                colors.Add(new Color(0.22f, 0.16f, 0.11f, 0.90f));
                colors.Add(new Color(0.22f, 0.16f, 0.11f, 0.90f));
                colors.Add(new Color(0.22f, 0.16f, 0.11f, 0.05f)); // Fading drip tip

                tris.Add(baseIdx); tris.Add(baseIdx + 1); tris.Add(baseIdx + 2);
            }

            var mesh = CreateMesh("TideLineMesh", verts, normals, uvs, colors, tris, owned);
            var mat = CreateStandardMaterial("TideLineMat", ColorFloodTideLine, 0.18f, 0.04f, owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        // Variant 1: Organic mud splatter cluster
        static void BuildFloodMudSplatter(GameObject root, Vector3 size, PanicExteriorOwnedArt owned)
        {
            float baseRadius = Mathf.Min(0.45f, Mathf.Min(size.x, size.y) * 0.35f);

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var tris = new List<int>();

            // Central splash core (organic polygonal fan)
            verts.Add(new Vector3(0, 0, -0.003f));
            normals.Add(Vector3.back);
            uvs.Add(new Vector2(0.5f, 0.5f));
            colors.Add(new Color(0.18f, 0.13f, 0.09f, 0.98f));

            const int corePoints = 16;
            for (int i = 0; i < corePoints; i++)
            {
                float theta = i * Mathf.PI * 2f / corePoints;
                float rHarmonic = 1f + 0.26f * Mathf.Sin(theta * 3f) + 0.18f * Mathf.Cos(theta * 5f) + 0.12f * Mathf.Sin(theta * 7f);
                float r = baseRadius * rHarmonic;
                float x = Mathf.Cos(theta) * r;
                float y = Mathf.Sin(theta) * r;

                verts.Add(new Vector3(x, y, -0.003f));
                normals.Add(Vector3.back);
                uvs.Add(new Vector2(0.5f + x / (baseRadius * 2f), 0.5f + y / (baseRadius * 2f)));
                colors.Add(new Color(0.20f, 0.14f, 0.09f, 0.85f));
            }

            for (int i = 0; i < corePoints; i++)
            {
                int next = (i + 1) % corePoints;
                tris.Add(0); tris.Add(i + 1); tris.Add(next + 1);
            }

            // Radial projectile droplets
            const int dropletCount = 10;
            for (int d = 0; d < dropletCount; d++)
            {
                float angle = d * Mathf.PI * 2f / dropletCount + Mathf.Sin(d * 1.7f) * 0.2f;
                float dist = baseRadius * (1.35f + Mathf.Repeat(d * 0.37f, 1f) * 0.85f);
                float dropSize = baseRadius * (0.12f + Mathf.Repeat(d * 0.29f, 1f) * 0.14f);

                Vector3 center = new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, -0.003f);
                Vector3 radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f).normalized;
                Vector3 perp = new Vector3(-radial.y, radial.x, 0f) * (dropSize * 0.55f);
                Vector3 forward = radial * (dropSize * 1.1f);

                int start = verts.Count;
                verts.Add(center - forward * 0.6f); // droplet back
                verts.Add(center + perp);           // right
                verts.Add(center + forward);        // droplet forward point
                verts.Add(center - perp);           // left

                normals.Add(Vector3.back); normals.Add(Vector3.back); normals.Add(Vector3.back); normals.Add(Vector3.back);
                uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0)); uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));
                colors.Add(new Color(0.18f, 0.13f, 0.09f, 0.95f));
                colors.Add(new Color(0.18f, 0.13f, 0.09f, 0.95f));
                colors.Add(new Color(0.20f, 0.14f, 0.09f, 0.70f));
                colors.Add(new Color(0.18f, 0.13f, 0.09f, 0.95f));

                tris.Add(start); tris.Add(start + 1); tris.Add(start + 2);
                tris.Add(start); tris.Add(start + 2); tris.Add(start + 3);
            }

            var mesh = CreateMesh("MudSplatterMesh", verts, normals, uvs, colors, tris, owned);
            var mat = CreateStandardMaterial("MudSplatterMat", ColorFloodMud, 0.42f, 0.06f, owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        // Variant 2: Reflective water puddle
        // Meets T1_R5_4 and T2_R5_1 (_Glossiness >= 0.85f, set to 0.92f)
        static void BuildFloodReflectivePuddle(GameObject root, Vector3 size, PanicExteriorOwnedArt owned)
        {
            float rx = Mathf.Min(0.75f, size.x * 0.45f);
            float rz = Mathf.Min(0.55f, size.z * 0.35f);

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var tris = new List<int>();

            // Pool central disc
            verts.Add(new Vector3(0, 0.005f, 0));
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));
            colors.Add(new Color(0.18f, 0.28f, 0.32f, 0.75f));

            const int poolSegments = 24;
            for (int i = 0; i < poolSegments; i++)
            {
                float theta = i * Mathf.PI * 2f / poolSegments;
                float rMod = 1f + 0.10f * Mathf.Sin(theta * 3f) + 0.06f * Mathf.Cos(theta * 5f);
                float x = Mathf.Cos(theta) * rx * rMod;
                float z = Mathf.Sin(theta) * rz * rMod;

                verts.Add(new Vector3(x, 0.005f, z));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(0.5f + x / (rx * 2f), 0.5f + z / (rz * 2f)));
                colors.Add(new Color(0.18f, 0.28f, 0.32f, 0.45f)); // Feathered water meniscus edge
            }

            for (int i = 0; i < poolSegments; i++)
            {
                int next = (i + 1) % poolSegments;
                tris.Add(0); tris.Add(i + 1); tris.Add(next + 1);
            }

            // Concentric capillary ripple rings
            float[] rippleFractions = { 0.45f, 0.75f };
            float ringWidth = 0.035f;

            foreach (var frac in rippleFractions)
            {
                int ringStart = verts.Count;
                for (int i = 0; i < poolSegments; i++)
                {
                    float theta = i * Mathf.PI * 2f / poolSegments;
                    float rMod = 1f + 0.10f * Mathf.Sin(theta * 3f) + 0.06f * Mathf.Cos(theta * 5f);
                    float xIn = Mathf.Cos(theta) * (rx * frac) * rMod;
                    float zIn = Mathf.Sin(theta) * (rz * frac) * rMod;
                    float xOut = Mathf.Cos(theta) * (rx * frac + ringWidth) * rMod;
                    float zOut = Mathf.Sin(theta) * (rz * frac + ringWidth) * rMod;

                    verts.Add(new Vector3(xIn, 0.008f, zIn));
                    verts.Add(new Vector3(xOut, 0.006f, zOut));

                    // Angled normals catch glancing specular highlights
                    normals.Add(new Vector3(Mathf.Cos(theta) * 0.2f, 0.98f, Mathf.Sin(theta) * 0.2f).normalized);
                    normals.Add(new Vector3(-Mathf.Cos(theta) * 0.2f, 0.98f, -Mathf.Sin(theta) * 0.2f).normalized);

                    uvs.Add(new Vector2(0.5f + xIn / (rx * 2f), 0.5f + zIn / (rz * 2f)));
                    uvs.Add(new Vector2(0.5f + xOut / (rx * 2f), 0.5f + zOut / (rz * 2f)));

                    colors.Add(new Color(0.24f, 0.35f, 0.38f, 0.85f));
                    colors.Add(new Color(0.24f, 0.35f, 0.38f, 0.40f));
                }

                for (int i = 0; i < poolSegments; i++)
                {
                    int b = ringStart + i * 2;
                    int nextB = ringStart + ((i + 1) % poolSegments) * 2;
                    tris.Add(b); tris.Add(b + 1); tris.Add(nextB + 1);
                    tris.Add(b); tris.Add(nextB + 1); tris.Add(nextB);
                }
            }

            var mesh = CreateMesh("PuddleMesh", verts, normals, uvs, colors, tris, owned);

            // PBR Material strictly configured for high specularity (_Glossiness >= 0.85f)
            var shader = Shader.Find("Standard");
            if (!shader) shader = Shader.Find("Legacy Shaders/Diffuse");
            var mat = new Material(shader) { name = "ReflectivePuddleMat" };
            mat.SetColor("_Color", ColorFloodWater);
            mat.SetFloat("_Glossiness", 0.92f); // Strict requirement >= 0.85f
            mat.SetFloat("_Metallic", 0.14f);

            // Transparent blending mode setup for Standard PBS
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)BlendMode.One);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)RenderQueue.Transparent;

            owned.Materials.Add(mat);
            AttachMeshRenderer(root, mesh, mat);
        }

        // Variant 3: Doorway silt deposit
        static void BuildFloodDoorwaySilt(GameObject root, Vector3 size, PanicExteriorOwnedArt owned)
        {
            float width = Mathf.Min(1.4f, size.x * 0.8f);
            float length = Mathf.Min(1.0f, size.z * 0.75f);

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var tris = new List<int>();

            const int tiers = 5;
            const int spokes = 12;

            for (int t = 0; t <= tiers; t++)
            {
                float fracT = (float)t / tiers;
                float r = length * Mathf.Pow(fracT, 0.85f);
                float elev = Mathf.Lerp(0.035f, 0.001f, fracT); // Thicker at baseboard, feather edge at perimeter

                for (int s = 0; s <= spokes; s++)
                {
                    float fracS = (float)s / spokes;
                    float angle = Mathf.Lerp(-Mathf.PI * 0.42f, Mathf.PI * 0.42f, fracS);

                    // Scalloped outer tongue variation
                    float scalloped = (t == tiers) ? (1f + 0.12f * Mathf.Sin(fracS * Mathf.PI * 6f)) : 1f;
                    float x = (fracS - 0.5f) * width * (0.35f + 0.65f * fracT) + Mathf.Sin(angle) * r * 0.45f * scalloped;
                    float z = r * Mathf.Cos(angle) * scalloped;

                    verts.Add(new Vector3(x, elev, z));
                    normals.Add(new Vector3(0, 0.95f, -0.3f * fracT).normalized);
                    uvs.Add(new Vector2(fracS, fracT));

                    // Vertex color gradient from wet dense silt to lighter dry edge
                    Color c = Color.Lerp(new Color(0.24f, 0.19f, 0.13f, 0.95f), new Color(0.38f, 0.32f, 0.22f, 0.75f), fracT);
                    colors.Add(c);
                }
            }

            for (int t = 0; t < tiers; t++)
            {
                for (int s = 0; s < spokes; s++)
                {
                    int rowA = t * (spokes + 1);
                    int rowB = (t + 1) * (spokes + 1);

                    int i0 = rowA + s;
                    int i1 = rowA + s + 1;
                    int i2 = rowB + s;
                    int i3 = rowB + s + 1;

                    tris.Add(i0); tris.Add(i2); tris.Add(i1);
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }
            }

            var mesh = CreateMesh("DoorwaySiltMesh", verts, normals, uvs, colors, tris, owned);
            var mat = CreateStandardMaterial("DoorwaySiltMat", ColorFloodSilt, 0.12f, 0.02f, owned);
            AttachMeshRenderer(root, mesh, mat);
        }

        // =========================================================================================
        // MATERIAL & SHADER FACTORY HELPERS
        // =========================================================================================

        static Material CreateStandardMaterial(string name, Color color, float glossiness, float metallic, PanicExteriorOwnedArt owned)
        {
            var shader = Shader.Find("Standard");
            if (!shader) shader = Shader.Find("Legacy Shaders/Diffuse");
            if (!shader) shader = Shader.Find("Sprites/Default");

            var mat = new Material(shader) { name = name };
            mat.SetColor("_Color", color);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", glossiness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

            if (owned != null) owned.Materials.Add(mat);
            return mat;
        }

        static Material CreateFireMaterial(string name, PanicExteriorOwnedArt owned)
        {
            Shader shader = Shader.Find("Panic/SoftWeather");
            if (!shader) shader = Shader.Find("Sprites/Default");
            if (!shader) shader = Shader.Find("Standard");

            var mat = new Material(shader) { name = name };
            if (shader.name == "Standard")
            {
                mat.SetFloat("_Mode", 2); // Fade
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = 3000;
                mat.SetFloat("_Glossiness", 0.04f);
            }
            else
            {
                mat.color = Color.white;
                mat.renderQueue = 3000;
            }

            if (owned != null) owned.Materials.Add(mat);
            return mat;
        }

        // =========================================================================================
        // PROCEDURAL GEOMETRY & MESH HELPERS
        // =========================================================================================

        static Mesh CreateMesh(string name, List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs, List<Color> colors, List<int> tris, PanicExteriorOwnedArt owned)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            if (owned != null) owned.Meshes.Add(mesh);
            return mesh;
        }

        static Mesh CompileMesh(string name, List<Vector3> v, List<int> tr, List<Vector2> uv, List<Color> col, PanicExteriorOwnedArt owned)
        {
            var m = new Mesh { name = name };
            m.vertices = v.ToArray();
            m.triangles = tr.ToArray();
            m.uv = uv.ToArray();
            m.colors = col.ToArray();
            m.RecalculateNormals();
            m.RecalculateBounds();
            if (owned != null) owned.Meshes.Add(m);
            return m;
        }

        static void AttachMeshRenderer(GameObject target, Mesh mesh, Material mat)
        {
            var mf = target.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = target.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = true;
        }

        static void AddRibbon(List<Vector3> v, List<int> tr, List<Vector2> uv, List<Color> col,
            Vector3 a, Vector3 b, float widthA, float widthB, float boundsW, float boundsH,
            Color innerCol, Color outerCol)
        {
            Vector3 dir = b - a;
            float len = dir.magnitude;
            if (len < 1e-4f) return;

            Vector3 uDir = dir / len;
            Vector3 normal = new Vector3(-uDir.y, uDir.x, 0f);

            int n = v.Count;
            Vector3 v0 = a - normal * (widthA * 0.5f);
            Vector3 v1 = a + normal * (widthA * 0.5f);
            Vector3 v2 = b + normal * (widthB * 0.5f);
            Vector3 v3 = b - normal * (widthB * 0.5f);

            v.Add(v0); v.Add(v1); v.Add(v2); v.Add(v3);
            uv.Add(ToUV(v0, boundsW, boundsH));
            uv.Add(ToUV(v1, boundsW, boundsH));
            uv.Add(ToUV(v2, boundsW, boundsH));
            uv.Add(ToUV(v3, boundsW, boundsH));

            col.Add(outerCol); col.Add(outerCol); col.Add(innerCol); col.Add(innerCol);

            tr.Add(n); tr.Add(n + 1); tr.Add(n + 2);
            tr.Add(n); tr.Add(n + 2); tr.Add(n + 3);
        }

        static void AddTriangle(List<Vector3> v, List<int> tr, List<Vector2> uv, List<Color> col,
            Vector3 a, Vector3 b, Vector3 c, float w, float h, Color cCol)
        {
            int n = v.Count;
            v.Add(a); v.Add(b); v.Add(c);
            uv.Add(ToUV(a, w, h)); uv.Add(ToUV(b, w, h)); uv.Add(ToUV(c, w, h));
            col.Add(cCol); col.Add(cCol); col.Add(cCol);
            tr.Add(n); tr.Add(n + 1); tr.Add(n + 2);
        }

        static void AddTri(List<int> tr, int a, int b, int c)
        {
            tr.Add(a); tr.Add(b); tr.Add(c);
        }

        static void AddQuad(List<int> tr, int a, int b, int c, int d)
        {
            tr.Add(a); tr.Add(b); tr.Add(c);
            tr.Add(a); tr.Add(c); tr.Add(d);
        }

        static Vector2 ToUV(Vector3 p, float w, float h)
        {
            return new Vector2((p.x / w) + 0.5f, (p.y / h) + 0.5f);
        }

        static float Hash(int n)
        {
            n = (n << 13) ^ n;
            return ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / (float)0x7fffffff;
        }

        static float HashRange(int n, float min, float max)
        {
            return Mathf.Lerp(min, max, Hash(n));
        }
    }
}
