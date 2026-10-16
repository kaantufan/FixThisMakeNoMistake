using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    // Original runtime art. No colliders, model changes, forces, camera or global lighting changes.
    public static class PanicDisasterArt
    {
        const int WaterCells = 40;
        static readonly int ColorId = Shader.PropertyToID("_Color");

        public static GameObject Create(DisasterType type)
        {
            var root = new GameObject("Disaster art - " + type);
            var s = root.AddComponent<PanicDisasterArtState>();
            s.Type = type;
            var soft = SoftTexture(s, false);
            var mote = ParticleMaterial(s, "Soft moisture / dust", soft, false);
            if (type == DisasterType.Flood) CreateFlood(s, mote);
            else if (type == DisasterType.Earthquake) CreateQuake(s, mote);
            else CreateFire(s, soft);
            Animate(root, 0, 0);
            return root;
        }

        public static void Animate(GameObject root, float age, float intensity)
        {
            if (!root) return;
            var s = root.GetComponent<PanicDisasterArtState>();
            if (!s) return;
            // HomeEffects resets phase age on Aftermath. Keep the visual event continuous.
            float dt = age - s.LastAge;
            if (dt < 0 || dt > .25f) dt = Mathf.Clamp(Time.deltaTime, 0, .1f);
            s.LastAge = age;
            s.Clock += Mathf.Max(0, dt);
            float target = Mathf.Clamp01(intensity);
            s.Strength = Mathf.Lerp(s.Strength, target, 1 - Mathf.Exp(-dt * 6));
            s.Peak = Mathf.Max(s.Peak, target);
            for (int i = 0; i < s.Emitters.Count; i++)
            {
                var e = s.Emitters[i];
                var emission = e.System.emission;
                emission.rateOverTime = e.Rate * s.Strength;
            }
            if (s.Type == DisasterType.Flood) AnimateFlood(s);
            else if (s.Type == DisasterType.Earthquake) AnimateQuake(s);
            else AnimateFire(s);
        }

        static void CreateFlood(PanicDisasterArtState s, Material soft)
        {
            var water = Opaque(s, "Cold flood water", new Color(.13f, .25f, .28f, .57f), .78f);
            water.SetFloat("_Metallic", .16f);
            MakeTransparent(water, true);
            var normal = WaterNormal(s);
            water.SetTexture("_BumpMap", normal); water.SetFloat("_BumpScale", .55f);
            water.EnableKeyword("_NORMALMAP"); water.mainTextureScale = new Vector2(7, 7);
            s.WaterMaterial = water;
            int side = WaterCells + 1;
            s.WaterVertices = new Vector3[side * side];
            s.WaterNormals = new Vector3[side * side];
            var uv = new Vector2[side * side];
            var triangles = new int[WaterCells * WaterCells * 6];
            for (int z = 0; z < side; z++) for (int x = 0; x < side; x++)
            {
                int v = z * side + x;
                s.WaterVertices[v] = new Vector3(-23 + 46f * x / WaterCells, -.24f, -23 + 46f * z / WaterCells);
                s.WaterNormals[v] = Vector3.up; uv[v] = new Vector2((float)x / WaterCells, (float)z / WaterCells);
            }
            int n = 0;
            for (int z = 0; z < WaterCells; z++) for (int x = 0; x < WaterCells; x++)
            {
                int v = z * side + x;
                triangles[n++] = v; triangles[n++] = v + side; triangles[n++] = v + 1;
                triangles[n++] = v + 1; triangles[n++] = v + side; triangles[n++] = v + side + 1;
            }
            s.WaterMesh = MeshObject(s, "Moving transparent water and advancing front", water, s.WaterVertices, triangles, uv);
            s.WaterMesh.normals = s.WaterNormals;
            s.WaterMesh.MarkDynamic(); s.WaterMesh.bounds = new Bounds(Vector3.zero, new Vector3(48, 3, 48));

            s.FoamVertices = new Vector3[72 * 4]; s.FoamColors = new Color[72 * 4];
            var foamUv = QuadUV(72); var foamTriangles = QuadTriangles(72);
            var foamMaterial = ParticleMaterial(s, "Soft irregular current foam", FoamTexture(s), false);
            s.FoamMesh = MeshObject(s, "Broken wave crests and foundation eddies", foamMaterial, s.FoamVertices, foamTriangles, foamUv);
            s.FoamMesh.colors = s.FoamColors; s.FoamMesh.MarkDynamic(); s.FoamMesh.bounds = new Bounds(Vector3.zero, new Vector3(48, 3, 48));
            CreateDebris(s, 28, true);

            // Rain falls only outside the 16 x 12 m house; no indoor rain volume.
            Rain(s, soft, new Vector3(0, 12, 15), new Vector2(46, 17));
            Rain(s, soft, new Vector3(0, 12, -15), new Vector2(46, 17));
            Rain(s, soft, new Vector3(-16, 12, 0), new Vector2(15, 12));
            Rain(s, soft, new Vector3(16, 12, 0), new Vector2(15, 12));

            var dripPoints = new List<Vector3>();
            for (int floor = 0; floor < 2; floor++)
            {
                float y = 2.24f + floor * 3.2f;
                dripPoints.Add(new Vector3(-4, y, 5.91f)); dripPoints.Add(new Vector3(4, y, 5.91f));
                dripPoints.Add(new Vector3(-7.91f, y, -3)); dripPoints.Add(new Vector3(-7.91f, y, 3));
            }
            var drips = Particles(s, "Rain trails on window glass", soft, 140, 50, .35f, .75f, .012f, .023f);
            MeshEmitter(drips, EmitterMesh(s, dripPoints.ToArray(), .65f));
            var dm = drips.main; dm.startSpeed = 0; dm.gravityModifier = .16f;
            var dv = drips.velocityOverLifetime; dv.enabled = true; dv.y = -.7f;
            var dr = drips.GetComponent<ParticleSystemRenderer>(); dr.renderMode = ParticleSystemRenderMode.Stretch; dr.lengthScale = 4; dr.velocityScale = .035f;
            SetGradient(drips, new Color(.6f, .73f, .77f, .0f), new Color(.6f, .73f, .77f, .45f), new Color(.6f, .73f, .77f, 0));

            var spray = Particles(s, "Doorstep and drain splash", soft, 180, 145, .25f, .65f, .035f, .09f);
            MeshEmitter(spray, EmitterMesh(s, new[] { new Vector3(0, .1f, -6.2f), new Vector3(-5, .08f, -8.5f), new Vector3(5, .08f, -8.5f), new Vector3(-8.15f, .08f, 3), new Vector3(8.15f, .08f, -2), new Vector3(4, .08f, 6.15f) }, .8f));
            var sm = spray.main; sm.startSpeed = new ParticleSystem.MinMaxCurve(.4f, 1.2f); sm.gravityModifier = .45f;
            SetGradient(spray, new Color(.65f, .79f, .78f, .25f), new Color(.74f, .83f, .81f, .45f), new Color(.55f, .7f, .72f, 0));
        }

        static void Rain(PanicDisasterArtState s, Material material, Vector3 p, Vector2 size)
        {
            var ps = Particles(s, "Outside rain curtain", material, 520, 370, .72f, 1.25f, .014f, .03f);
            var main = ps.main; main.startSpeed = new ParticleSystem.MinMaxCurve(12, 16); main.gravityModifier = .12f;
            var shape = ps.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Box;
            shape.position = p; shape.rotation = new Vector3(90, 0, 0); shape.scale = new Vector3(size.x, size.y, .12f);
            var renderer = ps.GetComponent<ParticleSystemRenderer>(); renderer.renderMode = ParticleSystemRenderMode.Stretch; renderer.velocityScale = .045f; renderer.lengthScale = 2;
            SetGradient(ps, new Color(.58f, .72f, .78f, 0), new Color(.58f, .72f, .78f, .38f), new Color(.58f, .72f, .78f, 0));
        }

        static void AnimateFlood(PanicDisasterArtState s)
        {
            float t = s.Clock, front = 25 - Mathf.Min(t, 8) * 6.1f;
            // Peak water is retained during aftermath, rather than draining when intensity drops.
            float level = -.08f + s.Peak * .47f;
            for (int i = 0; i < s.WaterVertices.Length; i++)
            {
                var v = s.WaterVertices[i];
                float wet = Wetness(v.x, v.z, front);
                v.y = WaterHeight(v.x, v.z, t, front, level, wet);
                s.WaterVertices[i] = v;
                float a = v.x * .81f + v.z * .56f - t * 2.1f;
                float b = v.x * -.49f + v.z * 1.17f - t * 2.8f;
                s.WaterNormals[i] = new Vector3(-wet * (.038f * .81f * Mathf.Cos(a) - .022f * .49f * Mathf.Cos(b)), 1, -wet * (.038f * .56f * Mathf.Cos(a) + .022f * 1.17f * Mathf.Cos(b))).normalized;
            }
            s.WaterMesh.vertices = s.WaterVertices; s.WaterMesh.normals = s.WaterNormals;
            s.WaterMaterial.mainTextureOffset = new Vector2(t * .016f, -t * .027f);
            s.WaterMaterial.SetTextureOffset("_BumpMap", new Vector2(-t * .023f, t * .032f));
            for (int i = 0; i < 72; i++)
            {
                bool leading = i < 28;
                float x = leading ? -22.5f + i * 1.67f : Mathf.Lerp(-21, 21, Hash(i * 7 + 6));
                float z = leading ? front + Mathf.Sin(x * .42f + t) * .65f : Mathf.Repeat(Hash(i * 13) * 44 - t * .48f + 22, 44) - 22;
                float wet = Wetness(x, z, front);
                float y = WaterHeight(x, z, t, front, level, wet) + .026f;
                float length = leading ? 1.2f : .35f + Hash(i * 11) * .85f;
                float width = leading ? .15f : .06f;
                float angle = leading ? Mathf.Sin(x * .42f + t) * .25f : Hash(i * 5) * 6.28f + t * .035f;
                Vector3 across = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * length;
                Vector3 along = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle)) * width;
                Vector3 center = new Vector3(x, y, z);
                int v = i * 4;
                s.FoamVertices[v] = center - across - along; s.FoamVertices[v + 1] = center - across + along;
                s.FoamVertices[v + 2] = center + across + along; s.FoamVertices[v + 3] = center + across - along;
                float alpha = wet * (leading ? .42f : .16f + .09f * Mathf.Sin(t * 1.3f + i)) * Mathf.Clamp01(s.Peak * 3);
                if (leading && front < -23) alpha = 0;
                Color c = new Color(.66f, .79f, .75f, alpha);
                s.FoamColors[v] = c; s.FoamColors[v + 1] = c; s.FoamColors[v + 2] = c; s.FoamColors[v + 3] = c;
            }
            s.FoamMesh.vertices = s.FoamVertices; s.FoamMesh.colors = s.FoamColors;
            AnimateDebris(s, t, front, level);
        }

        static float Wetness(float x, float z, float front)
        {
            float edge = front + Mathf.Sin(x * .42f) * .7f;
            return Mathf.SmoothStep(0, 1, (z - edge + 1.2f) * .6f);
        }

        static float WaterHeight(float x, float z, float t, float front, float level, float wet)
        {
            float edge = z - front - Mathf.Sin(x * .42f) * .7f;
            float crest = Mathf.Exp(-edge * edge * .8f) * .15f;
            float ripples = Mathf.Sin(x * .81f + z * .56f - t * 2.1f) * .038f + Mathf.Sin(x * -.49f + z * 1.17f - t * 2.8f) * .022f;
            return -.24f + wet * (level + .24f + ripples + crest);
        }

        static void CreateQuake(PanicDisasterArtState s, Material soft)
        {
            var v = new List<Vector3>(); var tr = new List<int>();
            for (int level = 0; level < 2; level++)
            {
                float y = .014f + level * 3.2f;
                FloorCrack(v, tr, new Vector3(-6.8f, y, -3.8f), new Vector3(-2.3f, y, 2.7f), 11, .035f, level * 31);
                FloorCrack(v, tr, new Vector3(3.4f, y, -4.6f), new Vector3(6.5f, y, 4.8f), 12, .029f, level * 19 + 4);
                FloorCrack(v, tr, new Vector3(-4.4f, y, -.9f), new Vector3(-6.6f, y, 1.4f), 6, .018f, level + 29);
                WallCrack(v, tr, new Vector3(-5.05f, .95f + level * 3.2f, 5.86f), 1.6f, false, level + 3);
                WallCrack(v, tr, new Vector3(4.98f, .85f + level * 3.2f, 5.86f), 1.9f, false, level + 18);
                WallCrack(v, tr, new Vector3(-7.86f, .9f + level * 3.2f, -2.06f), 1.7f, true, level + 25);
            }
            var crackMat = Opaque(s, "Hairline broken plaster", new Color(.055f, .052f, .043f), .05f);
            s.Cracks = MeshObject(s, "Persistent floor and window corner fractures", crackMat, v.ToArray(), tr.ToArray(), null);
            s.Cracks.RecalculateNormals();
            s.CrackObject = s.LastObject; s.CrackObject.SetActive(false);
            CreateDebris(s, 76, false);
            var dustSites = new[] { new Vector3(-7.5f, .08f, 4.7f), new Vector3(6.7f, .08f, 5.1f), new Vector3(-6.1f, .08f, -4.7f), new Vector3(6.9f, .08f, -3.1f), new Vector3(-6.4f, 3.28f, 4.7f), new Vector3(6.5f, 3.28f, 4.8f) };
            s.Dust = Particles(s, "Local plaster and floor dust puffs", soft, 450, 38, 1.5f, 3.9f, .15f, .4f);
            MeshEmitter(s.Dust, EmitterMesh(s, dustSites, .95f));
            var main = s.Dust.main; main.startSpeed = new ParticleSystem.MinMaxCurve(.13f, .5f); main.gravityModifier = -.008f;
            var vel = s.Dust.velocityOverLifetime; vel.enabled = true; vel.x = new ParticleSystem.MinMaxCurve(-.2f, .2f); vel.z = new ParticleSystem.MinMaxCurve(-.18f, .18f); vel.y = new ParticleSystem.MinMaxCurve(.12f, .12f);
            AddNoise(s.Dust, .28f, .65f); Grow(s.Dust, .4f, 2.5f);
            SetGradient(s.Dust, new Color(.48f, .44f, .36f, .05f), new Color(.48f, .44f, .36f, .3f), new Color(.4f, .39f, .34f, 0));
            var ceiling = Particles(s, "Fine dust dropping from two ceilings", soft, 210, 32, .65f, 1.7f, .035f, .09f);
            MeshEmitter(ceiling, EmitterMesh(s, new[] { new Vector3(-5, 2.72f, 3.7f), new Vector3(5, 2.7f, 3.5f), new Vector3(-5, 5.9f, -3), new Vector3(4, 5.9f, 3) }, 1.6f));
            var cm = ceiling.main; cm.startSpeed = 0; cm.gravityModifier = .12f;
            SetGradient(ceiling, new Color(.67f, .63f, .53f, .15f), new Color(.58f, .54f, .47f, .45f), new Color(.48f, .44f, .4f, 0));
        }

        static void AnimateQuake(PanicDisasterArtState s)
        {
            if (s.CrackObject && s.Peak > .14f) s.CrackObject.SetActive(true);
            // A few localized impacts, once per event. Rubble then stays where it landed.
            while (s.DustBurst < 4 && s.Clock >= .7f + s.DustBurst * 1.8f)
            {
                if (s.Dust) s.Dust.Emit(46);
                s.DustBurst++;
            }
            AnimateDebris(s, s.Clock, 0, 0);
        }

        static void CreateFire(PanicDisasterArtState s, Texture2D softTexture)
        {
            var flame = ParticleMaterial(s, "Luminous flame", SoftTexture(s, true), true);
            var smokeMat = ParticleMaterial(s, "Dusk smoke", softTexture, false);
            var emberMat = ParticleMaterial(s, "Hot embers", softTexture, true);
            // Localized points: kitchen, a living-room corner and an upstairs window.
            var sites = new[] { new Vector3(6.15f, .36f, 4.35f), new Vector3(-5.8f, .18f, -3.7f), new Vector3(-5.8f, 3.4f, -3.65f), new Vector3(4.25f, 3.45f, 4.9f) };
            var source = EmitterMesh(s, sites, .55f);
            var outer = Particles(s, "Broken orange flame tongues", flame, 240, 145, .6f, 1.15f, .55f, 1.1f);
            MeshEmitter(outer, source);
            var om = outer.main; om.startSpeed = new ParticleSystem.MinMaxCurve(.95f, 1.85f); om.startRotation = new ParticleSystem.MinMaxCurve(-.22f, .22f);
            var ov = outer.velocityOverLifetime; ov.enabled = true; ov.x = .12f; ov.z = -.1f; ov.y = .15f;
            AddNoise(outer, .23f, 1.4f); Grow(outer, .75f, .08f);
            SetGradient(outer, new Color(1, .82f, .3f, 0), new Color(1, .28f, .035f, .72f), new Color(.65f, .06f, .005f, 0));
            var inner = Particles(s, "Pale compact flame cores", flame, 170, 130, .32f, .7f, .24f, .58f);
            MeshEmitter(inner, source);
            var im = inner.main; im.startSpeed = new ParticleSystem.MinMaxCurve(.7f, 1.4f);
            AddNoise(inner, .12f, 1.7f); Grow(inner, .85f, .1f);
            SetGradient(inner, new Color(1, .93f, .63f, .3f), new Color(1, .65f, .16f, .85f), new Color(.9f, .14f, .01f, 0));

            var smoke = Particles(s, "Layered smoke plumes", smokeMat, 400, 55, 4, 7, .42f, .9f);
            smoke.transform.localPosition = Vector3.up * .9f; MeshEmitter(smoke, source);
            var mm = smoke.main; mm.startSpeed = new ParticleSystem.MinMaxCurve(.55f, .95f); mm.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            var mv = smoke.velocityOverLifetime; mv.enabled = true; mv.x = .28f; mv.z = .12f; mv.y = .18f;
            AddNoise(smoke, .42f, .45f); Grow(smoke, .45f, 2.5f);
            SetGradient(smoke, new Color(.13f, .125f, .11f, .03f), new Color(.16f, .16f, .15f, .44f), new Color(.28f, .29f, .28f, 0));
            var spin = smoke.rotationOverLifetime; spin.enabled = true; spin.z = new ParticleSystem.MinMaxCurve(-.16f, .16f);

            var embers = Particles(s, "Rising sparks and falling embers", emberMat, 170, 42, 1.5f, 3.2f, .025f, .065f);
            MeshEmitter(embers, source);
            var em = embers.main; em.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.4f); em.gravityModifier = .04f;
            var ev = embers.velocityOverLifetime; ev.enabled = true; ev.x = new ParticleSystem.MinMaxCurve(-.45f, .7f); ev.y = new ParticleSystem.MinMaxCurve(0, 0); ev.z = new ParticleSystem.MinMaxCurve(-.4f, .35f);
            AddNoise(embers, .4f, .8f);
            SetGradient(embers, new Color(1, .75f, .24f, 0), new Color(1, .34f, .035f, 1), new Color(.6f, .07f, .008f, 0));

            var scorchV = new List<Vector3>(); var scorchT = new List<int>();
            for (int i = 0; i < sites.Length; i++)
            {
                Vector3 p = sites[i]; p.y = p.y > 3 ? 3.217f : .017f;
                int start = scorchV.Count; scorchV.Add(p);
                for (int j = 0; j <= 14; j++)
                {
                    float a = j * Mathf.PI * 2 / 14, r = .64f + Hash(j * 13 + i) * .25f;
                    scorchV.Add(p + new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r * .73f));
                    if (j > 0) { scorchT.Add(start); scorchT.Add(start + j + 1); scorchT.Add(start + j); }
                }
            }
            var charMat = Opaque(s, "Retained small scorch marks", new Color(.045f, .038f, .031f), .035f);
            MeshObject(s, "Burn residue at ignition sites", charMat, scorchV.ToArray(), scorchT.ToArray(), null).RecalculateNormals();
            s.CrackObject = s.LastObject; s.CrackObject.SetActive(false);
            s.FireLights = new Light[3];
            for (int i = 0; i < s.FireLights.Length; i++)
            {
                var go = new GameObject("Local fire flicker"); go.transform.SetParent(s.transform, false); go.transform.localPosition = sites[i] + Vector3.up * .8f;
                var light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = new Color(1, .35f, .085f); light.range = 4.8f;
                light.shadows = LightShadows.None; light.renderMode = LightRenderMode.ForceVertex; light.intensity = 0; s.FireLights[i] = light;
            }
        }

        static void AnimateFire(PanicDisasterArtState s)
        {
            if (s.CrackObject && s.Peak > .25f) s.CrackObject.SetActive(true);
            for (int i = 0; i < s.FireLights.Length; i++)
            {
                float t = s.Clock + i * 4.7f;
                float flicker = .82f + .12f * Mathf.Sin(t * 17.3f) + .08f * Mathf.Sin(t * 10.1f) + .06f * Mathf.Sin(t * 29.7f);
                s.FireLights[i].intensity = s.Strength * 1.35f * flicker;
            }
        }

        static void CreateDebris(PanicDisasterArtState s, int count, bool floating)
        {
            s.Debris = new PanicDisasterArtState.DebrisPiece[count];
            s.DebrisVertices = new Vector3[count * 12]; s.DebrisNormals = new Vector3[count * 12];
            var triangles = new int[count * 12];
            for (int i = 0; i < triangles.Length; i++) triangles[i] = i;
            for (int i = 0; i < count; i++)
            {
                var d = new PanicDisasterArtState.DebrisPiece();
                if (floating)
                {
                    d.Origin = new Vector3(-21 + Hash(i * 3 + 1) * 42, 0, -21 + Hash(i * 7 + 6) * 42);
                    d.Scale = new Vector3(.25f + Hash(i * 9 + 3) * .8f, .04f + Hash(i * 13 + 8) * .09f, .12f + Hash(i * 11 + 2) * .23f);
                }
                else
                {
                    int edge = i % 4; float span = Hash(i * 7 + 1) * 12 - 6;
                    float baseFloor = i % 3 == 0 ? 3.2f : 0;
                    d.Origin = new Vector3(edge < 2 ? (edge == 0 ? -7.6f : 7.6f) : span, baseFloor + 2.6f + Hash(i * 13) * .35f, edge < 2 ? span * .76f : edge == 2 ? -5.5f : 5.5f);
                    d.Floor = baseFloor;
                    d.Scale = Vector3.one * (.07f + Hash(i * 17 + 3) * .17f); d.Scale.y *= .4f + Hash(i * 5) * .5f;
                    d.Velocity = new Vector3((Hash(i * 11 + 2) - .5f) * 1.6f, .35f + Hash(i * 19) * .9f, (Hash(i * 23) - .5f) * 1.2f);
                    d.Delay = .35f + Hash(i * 29 + 5) * 5.6f;
                }
                d.Rotation = new Vector3(Hash(i * 31) * 160, Hash(i * 37 + 1) * 360, Hash(i * 41) * 170);
                s.Debris[i] = d;
            }
            var mat = Opaque(s, floating ? "Waterlogged floating wood" : "Broken plaster and masonry", floating ? new Color(.22f, .18f, .115f) : new Color(.43f, .4f, .33f), .12f);
            s.DebrisMesh = MeshObject(s, floating ? "Batched current driven debris" : "Batched falling and settled masonry", mat, s.DebrisVertices, triangles, null);
            s.DebrisMesh.normals = s.DebrisNormals; s.DebrisMesh.MarkDynamic(); s.DebrisMesh.bounds = new Bounds(Vector3.zero, new Vector3(49, 16, 49));
        }

        static void AnimateDebris(PanicDisasterArtState s, float clock, float front, float waterLevel)
        {
            for (int i = 0; i < s.Debris.Length; i++)
            {
                var d = s.Debris[i]; Vector3 p; Quaternion q;
                if (s.Type == DisasterType.Flood)
                {
                    p = d.Origin; p.x += Mathf.Sin(clock * .17f + i) * .45f; p.z = Mathf.Repeat(p.z - clock * (.24f + Hash(i * 17) * .21f) + 23, 46) - 23;
                    float wet = Wetness(p.x, p.z, front);
                    p.y = WaterHeight(p.x, p.z, clock, front, waterLevel, wet) + .04f;
                    if (wet < .55f) p.y = -.5f;
                    q = Quaternion.Euler(Mathf.Sin(clock * 1.7f + i) * 6, d.Rotation.y + clock * (2 + Hash(i * 5) * 4), Mathf.Sin(clock * 1.3f + i) * 4);
                }
                else
                {
                    float t = Mathf.Max(0, clock - d.Delay);
                    float floor = d.Floor + d.Scale.y * .33f + .025f;
                    float fall = (d.Velocity.y + Mathf.Sqrt(d.Velocity.y * d.Velocity.y + 15.6f * (d.Origin.y - floor))) / 7.8f;
                    float travel = Mathf.Min(t, fall + .22f);
                    p = d.Origin + new Vector3(d.Velocity.x * travel, 0, d.Velocity.z * travel);
                    p.y = t < fall ? d.Origin.y + d.Velocity.y * t - 3.9f * t * t : floor + (t < fall + .22f ? Mathf.Sin((t - fall) / .22f * Mathf.PI) * .085f : 0);
                    if (clock < d.Delay) p.y = -2;
                    q = Quaternion.Euler(d.Rotation + new Vector3(travel * 123, travel * 97, travel * 81));
                }
                // Four flat triangular faces form an asymmetric shard, twelve vertices per piece.
                Vector3 a = new Vector3(-.5f, -.35f, -.42f), b = new Vector3(.5f, -.28f, -.4f), c = new Vector3(.35f, -.28f, .5f), top = new Vector3(-.12f, .65f, .03f);
                int v = i * 12;
                WriteFace(s, v, p, q, d.Scale, a, top, b); WriteFace(s, v + 3, p, q, d.Scale, b, top, c);
                WriteFace(s, v + 6, p, q, d.Scale, c, top, a); WriteFace(s, v + 9, p, q, d.Scale, a, b, c);
            }
            s.DebrisMesh.vertices = s.DebrisVertices; s.DebrisMesh.normals = s.DebrisNormals;
        }

        static void WriteFace(PanicDisasterArtState s, int v, Vector3 p, Quaternion q, Vector3 scale, Vector3 a, Vector3 b, Vector3 c)
        {
            a = q * Vector3.Scale(a, scale); b = q * Vector3.Scale(b, scale); c = q * Vector3.Scale(c, scale);
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            s.DebrisVertices[v] = p + a; s.DebrisVertices[v + 1] = p + b; s.DebrisVertices[v + 2] = p + c;
            s.DebrisNormals[v] = normal; s.DebrisNormals[v + 1] = normal; s.DebrisNormals[v + 2] = normal;
        }

        static void FloorCrack(List<Vector3> v, List<int> tr, Vector3 a, Vector3 b, int steps, float width, int seed)
        {
            Vector3 previous = a;
            for (int i = 1; i <= steps; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, (float)i / steps);
                p.x += (Hash(i * 11 + seed) - .5f) * .45f; p.z += (Hash(i * 17 + seed) - .5f) * .26f;
                Ribbon(v, tr, previous, p, Vector3.up, width * (.35f + .65f * Mathf.Sin((float)i / (steps + 1) * Mathf.PI)));
                if (i % 3 == 1) Ribbon(v, tr, p, p + new Vector3((Hash(i * 19 + seed) - .5f) * 1.1f, 0, .4f), Vector3.up, width * .38f);
                previous = p;
            }
        }

        static void WallCrack(List<Vector3> v, List<int> tr, Vector3 a, float height, bool west, int seed)
        {
            Vector3 prev = a;
            for (int i = 1; i <= 8; i++)
            {
                float lateral = (Hash(i * 9 + seed) - .5f) * .4f;
                Vector3 p = a + Vector3.up * (height * i / 8) + (west ? Vector3.forward : Vector3.right) * lateral;
                Ribbon(v, tr, prev, p, west ? Vector3.right : Vector3.back, .015f + .014f * Hash(i + seed));
                if (i == 4) Ribbon(v, tr, p, p + Vector3.up * .3f + (west ? Vector3.forward : Vector3.right) * .6f, west ? Vector3.right : Vector3.back, .01f);
                prev = p;
            }
        }

        static void Ribbon(List<Vector3> v, List<int> tr, Vector3 a, Vector3 b, Vector3 normal, float width)
        {
            Vector3 w = Vector3.Cross((b - a).normalized, normal) * width; int n = v.Count;
            v.Add(a - w); v.Add(a + w); v.Add(b + w); v.Add(b - w);
            tr.Add(n); tr.Add(n + 1); tr.Add(n + 2); tr.Add(n); tr.Add(n + 2); tr.Add(n + 3);
            // Double-sided small fracture surfaces: back-wall orientation varies.
            tr.Add(n + 2); tr.Add(n + 1); tr.Add(n); tr.Add(n + 3); tr.Add(n + 2); tr.Add(n);
        }

        static ParticleSystem Particles(PanicDisasterArtState s, string name, Material material, int cap, float rate, float lifeMin, float lifeMax, float sizeMin, float sizeMax)
        {
            var go = new GameObject(name); go.transform.SetParent(s.transform, false);
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = true; main.duration = 8; main.playOnAwake = false; main.maxParticles = cap;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax); main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startSpeed = .3f; main.startColor = Color.white; main.simulationSpace = ParticleSystemSimulationSpace.Local; main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            ps.useAutoRandomSeed = false; ps.randomSeed = (uint)(s.Emitters.Count * 131 + 53);
            var emission = ps.emission; emission.enabled = true; emission.rateOverTime = 0;
            var shape = ps.shape; shape.enabled = false;
            var renderer = ps.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = material; renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false; renderer.maxParticleSize = .15f; renderer.enableGPUInstancing = false;
            s.Emitters.Add(new PanicDisasterArtState.Emitter { System = ps, Rate = rate });
            ps.Play(); return ps;
        }

        static void MeshEmitter(ParticleSystem ps, Mesh mesh)
        {
            var shape = ps.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Mesh; shape.mesh = mesh; shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
        }

        static Mesh EmitterMesh(PanicDisasterArtState s, Vector3[] sites, float radius)
        {
            var verts = new Vector3[sites.Length * 4]; var uv = QuadUV(sites.Length); var tr = QuadTriangles(sites.Length);
            for (int i = 0; i < sites.Length; i++)
            {
                int v = i * 4; var p = sites[i];
                verts[v] = p + new Vector3(-radius, 0, -radius); verts[v + 1] = p + new Vector3(-radius, 0, radius);
                verts[v + 2] = p + new Vector3(radius, 0, radius); verts[v + 3] = p + new Vector3(radius, 0, -radius);
            }
            var mesh = new Mesh { name = "Disconnected upward emitter patches" }; mesh.vertices = verts; mesh.triangles = tr; mesh.uv = uv; mesh.RecalculateNormals(); mesh.RecalculateBounds(); s.Owned.Add(mesh); return mesh;
        }

        static void SetGradient(ParticleSystem ps, Color a, Color b, Color c)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(a, 0), new GradientColorKey(b, .25f), new GradientColorKey(c, 1) }, new[] { new GradientAlphaKey(a.a, 0), new GradientAlphaKey(b.a, .22f), new GradientAlphaKey(b.a * .65f, .6f), new GradientAlphaKey(c.a, 1) });
            var color = ps.colorOverLifetime; color.enabled = true; color.color = g;
        }

        static void Grow(ParticleSystem ps, float from, float to)
        {
            var size = ps.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, from), new Keyframe(.25f, Mathf.Lerp(from, to, .5f)), new Keyframe(1, to)));
        }

        static void AddNoise(ParticleSystem ps, float strength, float frequency)
        {
            var noise = ps.noise; noise.enabled = true; noise.quality = ParticleSystemNoiseQuality.Low; noise.strength = strength; noise.frequency = frequency; noise.scrollSpeed = .4f; noise.damping = true;
        }

        static Mesh MeshObject(PanicDisasterArtState s, string name, Material material, Vector3[] vertices, int[] triangles, Vector2[] uv)
        {
            var go = new GameObject(name); go.transform.SetParent(s.transform, false);
            var mesh = new Mesh { name = name }; mesh.vertices = vertices; mesh.triangles = triangles; if (uv != null) mesh.uv = uv;
            mesh.RecalculateBounds(); go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = material;
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = true;
            s.Owned.Add(mesh); s.LastObject = go; return mesh;
        }

        static Material Opaque(PanicDisasterArtState s, string name, Color color, float smoothness)
        {
            var mat = new Material(Shader.Find("Standard")); mat.name = name; mat.SetColor(ColorId, color); mat.SetFloat("_Glossiness", smoothness); s.Owned.Add(mat); return mat;
        }

        static Material ParticleMaterial(PanicDisasterArtState s, string name, Texture2D texture, bool additive)
        {
            // Dedicated straight-alpha pass: no sprite per-draw fields, alpha
            // keywords, flip data, renderer tint or particle shader variants.
            var shader = Shader.Find("Panic/SoftWeather");
            if (!shader) shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader);
            mat.name = name + (additive ? " / luminous soft weather" : " / soft weather");
            mat.SetTexture("_MainTex", texture); mat.SetColor(ColorId, Color.white);
            if (mat.HasProperty("_RendererColor")) mat.SetColor("_RendererColor", Color.white);
            if (mat.HasProperty("_Flip")) mat.SetVector("_Flip", Vector4.one);
            if (mat.HasProperty("_EnableExternalAlpha")) mat.SetFloat("_EnableExternalAlpha", 0);
            mat.renderQueue = (int)RenderQueue.Transparent; mat.enableInstancing = false;
            s.Owned.Add(mat); return mat;
        }

        static Texture2D FoamTexture(PanicDisasterArtState s)
        {
            const int width = 128, height = 32;
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                float u = (x + .5f) / width, v = (y + .5f) / height;
                float center = .5f + Mathf.Sin(u * 21) * .08f + Mathf.Sin(u * 47) * .035f;
                float band = .13f + .045f * Mathf.Sin(u * 33 + 1.7f);
                float edge = Mathf.Clamp01(1 - Mathf.Abs(v - center) / band);
                float ends = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, .12f, u)) * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.83f, 1, u)));
                float broken = .38f + .62f * Mathf.Pow(Mathf.Sin(u * 37 + .8f) * .5f + .5f, .7f);
                float alpha = edge * edge * ends * broken;
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1) alpha = 0;
                pixels[y * width + x] = new Color(1, 1, 1, alpha);
            }
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Original softly broken foam ribbon"; tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear; tex.SetPixels(pixels); tex.Apply(true, true);
            s.Owned.Add(tex); return tex;
        }

        static void MakeTransparent(Material mat, bool premultiply)
        {
            mat.SetFloat("_Mode", premultiply ? 3 : 2); mat.SetInt("_SrcBlend", (int)(premultiply ? BlendMode.One : BlendMode.SrcAlpha));
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha); mat.SetInt("_ZWrite", 0); mat.DisableKeyword("_ALPHATEST_ON");
            if (premultiply) { mat.EnableKeyword("_ALPHAPREMULTIPLY_ON"); mat.DisableKeyword("_ALPHABLEND_ON"); }
            else { mat.EnableKeyword("_ALPHABLEND_ON"); mat.DisableKeyword("_ALPHAPREMULTIPLY_ON"); }
            mat.renderQueue = (int)RenderQueue.Transparent - 10;
        }

        static Texture2D SoftTexture(PanicDisasterArtState s, bool flame)
        {
            const int size = 96; var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float u = (x + .5f) / size, v = (y + .5f) / size;
                float nx = u * 2 - 1, ny = v * 2 - 1;
                float noise = .77f + .13f * Mathf.Sin(u * 31 + Mathf.Sin(v * 17) * 2) + .10f * Mathf.Sin(v * 43 + u * 27);
                float alpha;
                if (flame)
                {
                    float width = Mathf.Lerp(.63f, .065f, Mathf.Pow(v, .8f));
                    float bend = Mathf.Sin(v * 9) * v * .15f;
                    alpha = Mathf.Clamp01(1 - Mathf.Abs(nx - bend) / width);
                    alpha = alpha * alpha * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, .12f, v)) * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.78f, 1, v))) * noise;
                }
                else
                {
                    float radius = Mathf.Sqrt(nx * nx + ny * ny);
                    alpha = Mathf.Pow(Mathf.Clamp01(1 - radius), 1.45f) * noise;
                }
                if (x == 0 || y == 0 || x == size - 1 || y == size - 1) alpha = 0;
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false); tex.name = flame ? "Original soft flame silhouette" : "Original soft irregular puff";
            tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels(pixels); tex.Apply(false, true); s.Owned.Add(tex); return tex;
        }

        static Texture2D WaterNormal(PanicDisasterArtState s)
        {
            const int size = 64; var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float u = (float)x / size * Mathf.PI * 2, v = (float)y / size * Mathf.PI * 2;
                var n = new Vector3(Mathf.Cos(u * 3 + v * 2) * .22f + Mathf.Cos(u * 7 - v * 4) * .13f, Mathf.Cos(v * 5 - u * 2) * .23f, 1).normalized;
                pixels[y * size + x] = new Color(n.x * .5f + .5f, n.y * .5f + .5f, n.z * .5f + .5f, n.x * .5f + .5f);
            }
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true, true); tex.name = "Original periodic ripple normals"; tex.wrapMode = TextureWrapMode.Repeat;
            tex.SetPixels(pixels); tex.Apply(true, true); s.Owned.Add(tex); return tex;
        }

        static int[] QuadTriangles(int count)
        {
            var tr = new int[count * 6];
            for (int i = 0; i < count; i++) { int v = i * 4, n = i * 6; tr[n] = v; tr[n + 1] = v + 1; tr[n + 2] = v + 2; tr[n + 3] = v; tr[n + 4] = v + 2; tr[n + 5] = v + 3; }
            return tr;
        }

        static Vector2[] QuadUV(int count)
        {
            var uv = new Vector2[count * 4];
            for (int i = 0; i < count; i++) { int v = i * 4; uv[v] = new Vector2(0, 0); uv[v + 1] = new Vector2(0, 1); uv[v + 2] = new Vector2(1, 1); uv[v + 3] = new Vector2(1, 0); }
            return uv;
        }

        static float Hash(int value) { return Mathf.Repeat(Mathf.Sin(value * 127.17f + 31.73f) * 43758.5453f, 1); }
    }

    // Per-instance state also owns every runtime-created material, texture and mesh.
    // No Update: only the host's Animate API drives mesh/light/emission changes.
    public sealed class PanicDisasterArtState : MonoBehaviour
    {
        public struct Emitter { public ParticleSystem System; public float Rate; }
        public struct DebrisPiece { public Vector3 Origin, Scale, Velocity, Rotation; public float Delay, Floor; }
        public DisasterType Type;
        public readonly List<Object> Owned = new List<Object>(20);
        public readonly List<Emitter> Emitters = new List<Emitter>(10);
        public float Clock, LastAge, Strength, Peak;
        public Mesh WaterMesh, FoamMesh, DebrisMesh, Cracks;
        public Material WaterMaterial;
        public Vector3[] WaterVertices, WaterNormals, FoamVertices, DebrisVertices, DebrisNormals;
        public Color[] FoamColors;
        public DebrisPiece[] Debris;
        public ParticleSystem Dust;
        public Light[] FireLights;
        public GameObject CrackObject, LastObject;
        public int DustBurst;
        void OnDestroy()
        {
            for (int i = 0; i < Owned.Count; i++) if (Owned[i]) Destroy(Owned[i]);
            Owned.Clear();
        }
    }
}
