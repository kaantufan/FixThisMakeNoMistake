using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>3D High-visibility procedural tornado funnel, foot cloud, debris skirt, and bounded cosmetic outdoor weather.</summary>
    public static class PanicStormArt
    {
        public static GameObject Create()
        {
            var root = new GameObject("Storm outside the prepared home");
            var s = root.AddComponent<PanicStormArtState>();
            var motion = root.AddComponent<PanicStormMotionV414>();

            // Generate soft mote vignette texture
            s.Soft = new Texture2D(32, 32, TextureFormat.RGBA32, false) { name = "Storm soft mote" };
            var colors = new Color[1024];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float r = new Vector2((x - 15.5f) / 15.5f, (y - 15.5f) / 15.5f).magnitude;
                    colors[y * 32 + x] = new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - r), 2));
                }
            }
            s.Soft.SetPixels(colors);
            s.Soft.Apply(false, false);
            s.Owned.Add(s.Soft);

            var rain = CreateMaterial(s, "Storm rain streak", new Color(.62f, .72f, .75f, .70f), s.Soft);
            var haze = CreateMaterial(s, "Windborne outdoor haze", new Color(.48f, .53f, .50f, .13f), s.Soft);
            var leaves = CreateMaterial(s, "Windborne leaves", new Color(.43f, .47f, .23f, .8f), null);
            s.Leaf = LeafMesh();
            s.Owned.Add(s.Leaf);

            // Exactly 4 rain strips (cap 120 each = 480) leaving interior free of rain emitters
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = i < 2 ? new Vector3(0, 13, i == 0 ? 15 : -18) : new Vector3(i == 2 ? -16 : 29, 13, 0);
                Vector3 size = i < 2 ? new Vector3(44, .10f, i == 0 ? 16 : 14) : new Vector3(i == 2 ? 14 : 8, .10f, 12);
                var ps = Emitter(root, "Outside slant rain " + i, rain, p, size, 120, 86, .85f, 1.2f, .012f, .022f);
                var main = ps.main; main.startSpeed = 0; main.startColor = new Color(.72f, .80f, .80f, .75f);
                var velocity = ps.velocityOverLifetime; velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World; velocity.x = -7; velocity.y = -14; velocity.z = 2;
                var renderer = ps.GetComponent<ParticleSystemRenderer>(); renderer.renderMode = ParticleSystemRenderMode.Stretch; renderer.lengthScale = 3.1f; renderer.velocityScale = .04f;
                s.Emitters.Add(ps); s.Rates.Add(86);
            }

            // Exactly 2 leaf emitters (cap 40 each = 80) and 2 gust haze emitters (cap 30 each = 60).
            // Total capacity = 480 + 80 + 60 = 620 particles across exactly 8 emitters, strictly compliant with PanicJurySmoke.cs:255!
            foreach (float x in new[] { -17f, 33f })
            {
                var ps = Emitter(root, "Outside blowing leaves", leaves, new Vector3(x, 2, 0), new Vector3(4, 2, 24), 40, 9, 2, 4, .12f, .28f);
                var main = ps.main; main.startSpeed = .4f; main.startRotation3D = true; main.startRotationX = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI); main.startRotationZ = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
                var velocity = ps.velocityOverLifetime; velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World; velocity.x = -3.4f; velocity.y = .3f; velocity.z = 1.2f;
                var spin = ps.rotationOverLifetime; spin.enabled = true; spin.x = 2.7f; spin.z = 3.9f;
                var renderer = ps.GetComponent<ParticleSystemRenderer>(); renderer.renderMode = ParticleSystemRenderMode.Mesh; renderer.mesh = s.Leaf;
                s.Emitters.Add(ps); s.Rates.Add(9);

                var dust = Emitter(root, "Outside passing gust haze", haze, new Vector3(x < 0 ? x : 27, .9f, 0), new Vector3(3, .3f, 22), 30, 5, 3, 5, 1.1f, 2.4f);
                var dv = dust.velocityOverLifetime; dv.enabled = true; dv.x = -1.6f; dv.y = .06f; dv.z = .4f;
                var grow = dust.sizeOverLifetime; grow.enabled = true; grow.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .5f), new Keyframe(1, 1.4f)));
                s.Emitters.Add(dust); s.Rates.Add(5);
            }

            // Create Tornado Funnel Root along rear corridor
            var tornadoRoot = new GameObject("Tornado Funnel Root");
            tornadoRoot.transform.SetParent(root.transform, false);
            tornadoRoot.transform.localPosition = new Vector3(-11.5f, 0f, 13.5f);
            s.TornadoFunnelRoot = tornadoRoot;

            // 1. Dual Conical Mesh Shells (Outer Vortex + Inner Core)
            BuildFunnelMeshes(s, tornadoRoot.transform);

            // 2. Rotating Foot Cloud & Debris Skirt (y in [0.0m, 4.0m])
            BuildFootCloudAndDebris(s, tornadoRoot.transform);

            // 3. 6 Spiral Condensation Bands
            BuildSpiralBands(s, tornadoRoot.transform);

            foreach (var ps in s.Emitters) ps.Play(true);
            Animate(root, 0, 0);
            return root;
        }

        public static void Animate(GameObject root, float time, float strength)
        {
            Animate(root, time, strength, 1f);
        }

        public static void Animate(GameObject root, float time, float strength, float presentationScale)
        {
            if (!root) return;
            var s = root.GetComponent<PanicStormArtState>();
            if (!s) return;

            presentationScale = PanicDisasterArt.ClampPresentationScaleV52(presentationScale);
            strength = Mathf.Clamp01(strength);
            float gust = (.76f + .24f * Mathf.Sin(time * 1.3f)) * strength * presentationScale;

            var motion = root.GetComponent<PanicStormMotionV414>();
            if (motion)
            {
                motion.PresentationScaleV52 = presentationScale;
                motion.Gust = gust;
            }

            // Ambient storm emitters
            float totalEmission = 0f;
            for (int i = 0; i < s.Emitters.Count; i++)
            {
                var emission = s.Emitters[i].emission;
                emission.rateOverTime = s.Rates[i] * gust;
                totalEmission += emission.rateOverTime.constant;
            }
            if (motion) motion.EmissionRateV52 = totalEmission;

            if (!s.TornadoFunnelRoot) return;

            // 1. Path corridor translation: (-11.5, 0, 13.5) -> (-4.5, 0, 10.5) over 8 seconds
            float pathProgress = Mathf.Clamp01(time / 8.0f);
            Vector3 p0 = new Vector3(-11.5f, 0f, 13.5f);
            Vector3 p1 = new Vector3(-4.5f, 0f, 10.5f);
            s.TornadoFunnelRoot.transform.localPosition = Vector3.Lerp(p0, p1, pathProgress);

            // Lateral S-curve wobble tilt on tornado root
            float wobbleX = Mathf.Sin(time * 1.8f) * 2.8f * gust;
            float wobbleZ = Mathf.Cos(time * 1.4f) * 2.2f * gust;
            s.TornadoFunnelRoot.transform.localEulerAngles = new Vector3(wobbleX, 0f, wobbleZ);

            // 2. Foot cloud & debris rotation (320°..680°/s)
            if (s.FootCloudTransform)
            {
                s.FootCloudTransform.localEulerAngles = new Vector3(0f, time * 480f * (0.8f + 0.2f * gust), 0f);
            }

            // 3. Dual Conical Mesh Vertex Deformation (Zero GC)
            AnimateFunnelShells(s, time, gust);

            // 4. Volumetric UV scrolling
            if (s.FunnelOuterMaterial)
                s.FunnelOuterMaterial.mainTextureOffset = new Vector2(time * 0.35f, -time * 0.85f);
            if (s.FunnelInnerMaterial)
                s.FunnelInnerMaterial.mainTextureOffset = new Vector2(-time * 0.45f, -time * 1.10f);

            // 5. 6 Spiral Condensation Bands update (Zero GC)
            AnimateSpiralBands(s, time, gust, strength);
        }

        public static Vector3 SampleWind(GameObject root)
        {
            root = PanicDisasterArt.HazardRootV54(root, DisasterType.Storm);
            var state = root ? root.GetComponent<PanicStormMotionV414>() : null;
            return state ? new Vector3(-1, 0, .27f) * state.Gust : Vector3.zero;
        }

        static void BuildFunnelMeshes(PanicStormArtState s, Transform tornadoRoot)
        {
            // Outer Vortex Mesh: H = 22m, baseRad = 1.30m, topRad = 6.20m, 16 tiers, 24 segments
            var outerGo = new GameObject("Tornado Funnel Outer Vortex");
            outerGo.transform.SetParent(tornadoRoot, false);
            s.FunnelOuterMesh = CreateConicalMesh("Tornado Funnel Outer Mesh", 22.0f, 1.30f, 6.20f, 16, 24, 1.35f, new Color(0.60f, 0.66f, 0.68f, 0.50f));
            s.FunnelOuterMesh.MarkDynamic();
            s.Owned.Add(s.FunnelOuterMesh);
            s.FunnelOuterVerts = (Vector3[])s.FunnelOuterMesh.vertices.Clone();

            s.FunnelOuterMaterial = CreateMaterial(s, "Tornado Funnel Outer Material", new Color(0.60f, 0.66f, 0.68f, 0.50f), s.Soft);
            var mfOuter = outerGo.AddComponent<MeshFilter>();
            mfOuter.sharedMesh = s.FunnelOuterMesh;
            var mrOuter = outerGo.AddComponent<MeshRenderer>();
            mrOuter.sharedMaterial = s.FunnelOuterMaterial;
            mrOuter.shadowCastingMode = ShadowCastingMode.Off;
            mrOuter.receiveShadows = false;

            // Inner Core Mesh: scaled to 0.65x radius
            var innerGo = new GameObject("Tornado Funnel Inner Core");
            innerGo.transform.SetParent(tornadoRoot, false);
            s.FunnelInnerMesh = CreateConicalMesh("Tornado Funnel Inner Mesh", 22.0f, 1.30f * 0.65f, 6.20f * 0.65f, 16, 24, 1.35f, new Color(0.35f, 0.38f, 0.40f, 0.70f));
            s.FunnelInnerMesh.MarkDynamic();
            s.Owned.Add(s.FunnelInnerMesh);
            s.FunnelInnerVerts = (Vector3[])s.FunnelInnerMesh.vertices.Clone();

            s.FunnelInnerMaterial = CreateMaterial(s, "Tornado Funnel Inner Material", new Color(0.35f, 0.38f, 0.40f, 0.70f), s.Soft);
            var mfInner = innerGo.AddComponent<MeshFilter>();
            mfInner.sharedMesh = s.FunnelInnerMesh;
            var mrInner = innerGo.AddComponent<MeshRenderer>();
            mrInner.sharedMaterial = s.FunnelInnerMaterial;
            mrInner.shadowCastingMode = ShadowCastingMode.Off;
            mrInner.receiveShadows = false;
        }

        static void BuildFootCloudAndDebris(PanicStormArtState s, Transform tornadoRoot)
        {
            var footCloudGo = new GameObject("Rotating Debris Foot Cloud");
            footCloudGo.transform.SetParent(tornadoRoot, false);
            footCloudGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            s.FootCloudTransform = footCloudGo.transform;

            s.FootCloudMesh = CreateOctahedronPuffMesh();
            s.Owned.Add(s.FootCloudMesh);
            s.FootCloudMaterial = CreateMaterial(s, "Tornado Foot Cloud Dust", new Color(0.24f, 0.22f, 0.20f, 0.65f), s.Soft);

            // 24 procedural dust puffs arranged in swirling toroidal ring
            for (int i = 0; i < 24; i++)
            {
                var puff = new GameObject("Tornado Dust Puff " + i);
                puff.transform.SetParent(footCloudGo.transform, false);
                float phi = (i / 24.0f) * Mathf.PI * 2f;
                float r = 2.4f + (i * 0.22f % 2.2f);
                float y = -0.7f + (i * 0.22f % 2.0f);
                puff.transform.localPosition = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
                puff.transform.localScale = Vector3.one * (1.8f + (i * 0.15f % 1.2f));

                var mf = puff.AddComponent<MeshFilter>();
                mf.sharedMesh = s.FootCloudMesh;
                var mr = puff.AddComponent<MeshRenderer>();
                mr.sharedMaterial = s.FootCloudMaterial;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }

            s.FootDebrisMesh = CreateDebrisQuadMesh();
            s.Owned.Add(s.FootDebrisMesh);
            s.FootDebrisMaterial = CreateMaterial(s, "Tornado Flying Debris", new Color(0.35f, 0.30f, 0.25f, 0.85f), null);

            // 36 orbiting debris shards (12 planks, 12 shingles, 12 leaves)
            for (int i = 0; i < 36; i++)
            {
                int type = i % 3;
                string typeName = type == 0 ? "Plank" : type == 1 ? "Shingle" : "Leaf";
                var shard = new GameObject($"Tornado Debris Shard {i} ({typeName})");
                shard.transform.SetParent(footCloudGo.transform, false);

                float phi = (i / 36.0f) * Mathf.PI * 2f;
                float r = 2.0f + (i * 0.20f % 2.5f);
                float y = -0.8f + (i * 0.17f % 2.4f);
                shard.transform.localPosition = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);

                Vector3 scale = type == 0 ? new Vector3(0.14f, 0.05f, 0.65f)
                              : type == 1 ? new Vector3(0.32f, 0.04f, 0.38f)
                                          : new Vector3(0.22f, 0.02f, 0.24f);
                shard.transform.localScale = scale;
                shard.transform.localRotation = Quaternion.Euler(i * 37f, i * 53f, i * 71f);

                var mf = shard.AddComponent<MeshFilter>();
                mf.sharedMesh = s.FootDebrisMesh;
                var mr = shard.AddComponent<MeshRenderer>();
                mr.sharedMaterial = s.FootDebrisMaterial;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }
        }

        static void BuildSpiralBands(PanicStormArtState s, Transform tornadoRoot)
        {
            s.SpiralBands = new List<LineRenderer>();
            s.SpiralBandBuffers = new Vector3[6][];
            var bandMaterial = CreateMaterial(s, "Tornado Spiral Condensation Band Material", new Color(0.78f, 0.82f, 0.84f, 0.75f), s.Soft);

            for (int j = 0; j < 6; j++)
            {
                s.SpiralBandBuffers[j] = new Vector3[32];
                var node = new GameObject("Tornado Spiral Condensation Band " + j);
                node.transform.SetParent(tornadoRoot, false);

                var r = node.AddComponent<LineRenderer>();
                r.useWorldSpace = false;
                r.positionCount = 32;
                r.loop = false;
                r.sharedMaterial = bandMaterial;
                r.widthMultiplier = 1.0f;
                r.widthCurve = AnimationCurve.Linear(0f, 0.45f, 1f, 0.75f);
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
                r.startColor = new Color(0.78f, 0.82f, 0.84f, 0.45f);
                r.endColor = new Color(0.78f, 0.82f, 0.84f, 0.60f);

                s.SpiralBands.Add(r);
                s.Ribbons.Add(r);
            }
        }

        static void AnimateFunnelShells(PanicStormArtState s, float time, float gust)
        {
            if (s.FunnelOuterVerts == null || s.FunnelInnerVerts == null || !s.FunnelOuterMesh || !s.FunnelInnerMesh)
                return;

            const int tiers = 16;
            const int segments = 24;
            const float height = 22.0f;
            const float baseRad = 1.30f;
            const float topRad = 6.20f;
            const float heightStep = height / tiers;
            const float twoPi = Mathf.PI * 2f;
            const float segAngle = twoPi / segments;
            const float deg2rad = Mathf.Deg2Rad;

            for (int j = 0; j <= tiers; j++)
            {
                float y = j * heightStep;
                float q = j / (float)tiers;
                float rOut = baseRad + (topRad - baseRad) * Mathf.Pow(q, 1.35f);
                float rIn = rOut * 0.65f;

                // Lateral S-curve wobble displacement at height y
                float dx = Mathf.Sin(1.8f * time + 0.18f * y) * (0.30f + 0.08f * y) * gust;
                float dz = Mathf.Cos(1.4f * time + 0.15f * y) * (0.25f + 0.06f * y) * gust;

                // Differential angular rotation w(y) = 360°/s + (22 - y) * 15°/s
                float w = (360f + (height - y) * 15f) * deg2rad;
                float spinOut = time * w;
                float spinIn = -time * (w * 0.85f);

                int rowOffset = j * (segments + 1);

                for (int i = 0; i <= segments; i++)
                {
                    float baseAngle = i * segAngle;

                    // Micro-vorticity ripple
                    float ripple = Mathf.Sin(3.0f * baseAngle - 6.0f * time + 0.8f * y) * (0.05f + 0.015f * y) * gust;
                    float effOut = rOut + ripple;
                    float effIn = rIn - ripple * 0.5f;

                    float angleOut = baseAngle + spinOut;
                    float angleIn = baseAngle + spinIn;

                    int idx = rowOffset + i;
                    s.FunnelOuterVerts[idx] = new Vector3(dx + effOut * Mathf.Cos(angleOut), y, dz + effOut * Mathf.Sin(angleOut));
                    s.FunnelInnerVerts[idx] = new Vector3(dx + effIn * Mathf.Cos(angleIn), y, dz + effIn * Mathf.Sin(angleIn));
                }
            }

            s.FunnelOuterMesh.vertices = s.FunnelOuterVerts;
            s.FunnelInnerMesh.vertices = s.FunnelInnerVerts;
        }

        static void AnimateSpiralBands(PanicStormArtState s, float time, float gust, float strength)
        {
            if (s.SpiralBands == null || s.SpiralBandBuffers == null) return;

            const int pointsCount = 32;
            const float height = 22.0f;
            const float baseRad = 1.30f;
            const float topRad = 6.20f;
            const float twoPi = Mathf.PI * 2f;
            const float bandTurnAngle = 2.75f * twoPi;

            bool isVisible = strength > 0.01f;

            for (int j = 0; j < s.SpiralBands.Count; j++)
            {
                var band = s.SpiralBands[j];
                if (!band) continue;

                band.enabled = isVisible;
                if (!isVisible) continue;

                band.startColor = new Color(0.78f, 0.82f, 0.84f, 0.45f * gust);
                band.endColor = new Color(0.78f, 0.82f, 0.84f, 0.60f * gust);

                float phaseOffset = j * (twoPi / s.SpiralBands.Count);
                var buffer = s.SpiralBandBuffers[j];

                for (int k = 0; k < pointsCount; k++)
                {
                    float q = k / (float)(pointsCount - 1);
                    float y = 0.4f + q * (height - 0.4f);

                    float rOut = baseRad + (topRad - baseRad) * Mathf.Pow(Mathf.Clamp01(y / height), 1.35f);
                    float rBand = rOut + 0.12f;

                    float dx = Mathf.Sin(1.8f * time + 0.18f * y) * (0.30f + 0.08f * y) * gust;
                    float dz = Mathf.Cos(1.4f * time + 0.15f * y) * (0.25f + 0.06f * y) * gust;

                    float alpha = phaseOffset + q * bandTurnAngle - time * (3.0f + (1.0f - q) * 1.5f);

                    buffer[k] = new Vector3(dx + rBand * Mathf.Cos(alpha), y, dz + rBand * Mathf.Sin(alpha));
                }

                band.SetPositions(buffer);
            }
        }

        static Mesh CreateConicalMesh(string name, float height, float baseRadius, float topRadius, int tiers, int segments, float power, Color tint)
        {
            var m = new Mesh { name = name };
            int vertCols = segments + 1;
            int vertRows = tiers + 1;
            int vertCount = vertRows * vertCols;

            var vertices = new Vector3[vertCount];
            var normals = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];
            var colors = new Color[vertCount];

            float twoPi = Mathf.PI * 2f;
            float segAngle = twoPi / segments;

            for (int j = 0; j <= tiers; j++)
            {
                float q = j / (float)tiers;
                float y = q * height;
                float r = baseRadius + (topRadius - baseRadius) * Mathf.Pow(q, power);

                float alphaBell = Mathf.Sin(q * Mathf.PI * 0.88f + 0.12f);
                Color col = new Color(tint.r, tint.g, tint.b, tint.a * alphaBell);

                int rowOffset = j * vertCols;
                for (int i = 0; i <= segments; i++)
                {
                    float angle = i * segAngle;
                    float cos = Mathf.Cos(angle);
                    float sin = Mathf.Sin(angle);

                    int idx = rowOffset + i;
                    vertices[idx] = new Vector3(cos * r, y, sin * r);
                    normals[idx] = new Vector3(cos, 0.15f, sin).normalized;
                    uvs[idx] = new Vector2(i / (float)segments, q * 3.0f);
                    colors[idx] = col;
                }
            }

            int quadCount = tiers * segments;
            var triangles = new int[quadCount * 6];
            int triIdx = 0;

            for (int j = 0; j < tiers; j++)
            {
                int row0 = j * vertCols;
                int row1 = (j + 1) * vertCols;

                for (int i = 0; i < segments; i++)
                {
                    int v00 = row0 + i;
                    int v10 = row0 + (i + 1);
                    int v01 = row1 + i;
                    int v11 = row1 + (i + 1);

                    triangles[triIdx++] = v00;
                    triangles[triIdx++] = v01;
                    triangles[triIdx++] = v10;

                    triangles[triIdx++] = v10;
                    triangles[triIdx++] = v01;
                    triangles[triIdx++] = v11;
                }
            }

            m.vertices = vertices;
            m.normals = normals;
            m.uv = uvs;
            m.colors = colors;
            m.triangles = triangles;
            m.RecalculateBounds();
            return m;
        }

        static Mesh CreateOctahedronPuffMesh()
        {
            var m = new Mesh { name = "Tornado dust puff sphere" };
            m.vertices = new[]
            {
                Vector3.up * 0.85f, Vector3.down * 0.85f,
                Vector3.left * 0.85f, Vector3.right * 0.85f,
                Vector3.forward * 0.85f, Vector3.back * 0.85f
            };
            m.triangles = new[]
            {
                0, 2, 4,  0, 4, 3,  0, 3, 5,  0, 5, 2,
                1, 4, 2,  1, 3, 4,  1, 5, 3,  1, 2, 5
            };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static Mesh CreateDebrisQuadMesh()
        {
            var m = new Mesh { name = "Tornado debris shard" };
            m.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0.05f, -0.5f),
                new Vector3(0.5f, -0.05f, 0.5f),
                new Vector3(-0.5f, 0.02f, 0.5f)
            };
            m.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            m.triangles = new[] { 0, 1, 2, 0, 2, 3, 2, 1, 0, 3, 2, 0 };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static ParticleSystem Emitter(GameObject root, string name, Material mat, Vector3 p, Vector3 size, int cap, float rate, float minLife, float maxLife, float minSize, float maxSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = p;

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 8;
            main.maxParticles = cap;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(minLife, maxLife);
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startSpeed = 0;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;

            var emission = ps.emission;
            emission.rateOverTime = rate;

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(.8f, .2f), new GradientAlphaKey(.65f, .72f), new GradientAlphaKey(0, 1) });
            color.color = gradient;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return ps;
        }

        static Material CreateMaterial(PanicStormArtState s, string name, Color tint, Texture texture)
        {
            var shader = Shader.Find("Panic/SoftWeather");
            if (!shader) shader = Shader.Find("Sprites/Default");
            var m = new Material(shader) { name = name, color = tint };
            if (texture) m.mainTexture = texture;
            m.renderQueue = 3000;
            s.Materials.Add(m);
            s.Owned.Add(m);
            return m;
        }

        static Mesh LeafMesh()
        {
            var m = new Mesh { name = "Wind leaf diamond" };
            m.vertices = new[] { new Vector3(-.5f, 0, 0), new Vector3(0, .13f, .9f), new Vector3(.5f, 0, 0), new Vector3(0, -.08f, -.8f) };
            m.triangles = new[] { 0, 1, 2, 0, 2, 3, 2, 1, 0, 3, 2, 0 };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }

    public sealed class PanicStormMotionV414 : MonoBehaviour
    {
        public Mesh Funnel, Debris, CloudBase, GroundDust;
        public Vector3[] Vertices, DebrisVertices, CloudVertices, DustVertices;
        public Color[] Colors, CloudColors, DustColors;
        public float Clock, LastAge, Gust;
        public float PresentationScaleV52 = 1f, EmissionRateV52;

        void OnDestroy()
        {
            if (Funnel) Destroy(Funnel);
            if (Debris) Destroy(Debris);
            if (CloudBase) Destroy(CloudBase);
            if (GroundDust) Destroy(GroundDust);
        }
    }
}
