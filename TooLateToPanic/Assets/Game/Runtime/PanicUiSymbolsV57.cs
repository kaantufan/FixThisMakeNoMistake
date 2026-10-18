using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Original offline, font-independent 32-unit pictograms for the Home V5.7 UI.</summary>
    public static class PanicUiSymbolsV57
    {
        private enum Mark
        {
            Play, Continue, Book, Bag, Settings, Exit, Home, Back, Pause, Sound,
            Motion, Check, Clock, Warning, Budget, Damage, Shield, Earthquake,
            Flood, Fire, Storm, Water, Food, Tools, Wood, Metal, Screw, Tape,
            Plastic, Bolt, Notebook, Person, Help
        }

        private struct Stroke
        {
            public Vector2 A, B;
            public Stroke(float x1, float y1, float x2, float y2)
            { A = new Vector2(x1, y1); B = new Vector2(x2, y2); }
        }

        private const int TextureSize = 128;
        private const float Grid = 32f;
        private const float StrokeWidth = 1.95f;
        private static readonly Dictionary<string, Mark> Keys = new Dictionary<string, Mark>(StringComparer.OrdinalIgnoreCase)
        {
            { "play", Mark.Play }, { "continue", Mark.Continue }, { "book", Mark.Book },
            { "bag", Mark.Bag }, { "settings", Mark.Settings }, { "exit", Mark.Exit },
            { "home", Mark.Home }, { "back", Mark.Back }, { "pause", Mark.Pause },
            { "sound", Mark.Sound }, { "motion", Mark.Motion }, { "check", Mark.Check },
            { "clock", Mark.Clock }, { "warning", Mark.Warning }, { "budget", Mark.Budget },
            { "damage", Mark.Damage }, { "shield", Mark.Shield }, { "earthquake", Mark.Earthquake },
            { "flood", Mark.Flood }, { "fire", Mark.Fire }, { "storm", Mark.Storm },
            { "water", Mark.Water }, { "food", Mark.Food }, { "tools", Mark.Tools },
            { "wood", Mark.Wood }, { "metal", Mark.Metal }, { "screw", Mark.Screw },
            { "tape", Mark.Tape }, { "plastic", Mark.Plastic }, { "bolt", Mark.Bolt },
            { "notebook", Mark.Notebook }, { "person", Mark.Person }, { "help", Mark.Help },
            { "shelter", Mark.Shield }
        };
        private static readonly Dictionary<Mark, Texture2D> Cache = new Dictionary<Mark, Texture2D>();

        public static bool Has(string key) { return key != null && Keys.ContainsKey(key); }

        /// <summary>Turkish plain-language meaning; use alongside an icon, never as its tooltip dependency.</summary>
        public static string SymbolName(string key)
        {
            Mark mark;
            if (key == null || !Keys.TryGetValue(key, out mark)) return "";
            switch (mark)
            {
                case Mark.Play: return "Başla"; case Mark.Continue: return "Devam et";
                case Mark.Book: return "Rehber"; case Mark.Bag: return "Çanta";
                case Mark.Settings: return "Ayarlar"; case Mark.Exit: return "Çıkış";
                case Mark.Home: return "Ev"; case Mark.Back: return "Geri";
                case Mark.Pause: return "Duraklat"; case Mark.Sound: return "Ses";
                case Mark.Motion: return "Hareket"; case Mark.Check: return "Tamamlandı";
                case Mark.Clock: return "Süre"; case Mark.Warning: return "Uyarı";
                case Mark.Budget: return "Bütçe"; case Mark.Damage: return "Hasar";
                case Mark.Shield: return "Sığınak"; case Mark.Earthquake: return "Deprem";
                case Mark.Flood: return "Sel"; case Mark.Fire: return "Yangın";
                case Mark.Storm: return "Fırtına"; case Mark.Water: return "Su";
                case Mark.Food: return "Gıda"; case Mark.Tools: return "Aletler";
                case Mark.Wood: return "Ahşap"; case Mark.Metal: return "Metal";
                case Mark.Screw: return "Vida"; case Mark.Tape: return "Bant";
                case Mark.Plastic: return "Plastik"; case Mark.Bolt: return "Cıvata";
                case Mark.Notebook: return "Defter"; case Mark.Person: return "Oyuncu";
                default: return "Yardım";
            }
        }

        /// <summary>Call from OnGUI. Unknown keys draw nothing. Preserves the caller's GUI state.</summary>
        public static void Draw(Rect rect, string key, Color ink)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || rect.width <= 0f || rect.height <= 0f) return;
            Mark mark;
            if (key == null || !Keys.TryGetValue(key, out mark)) return;
            Color color = GUI.color, content = GUI.contentColor;
            Matrix4x4 matrix = GUI.matrix;
            try
            {
                Texture2D texture;
                if (!Cache.TryGetValue(mark, out texture) || texture == null)
                { texture = Rasterize(mark); Cache[mark] = texture; }
                float size = Mathf.Min(rect.width, rect.height);
                Rect square = new Rect(rect.x + (rect.width - size) * .5f, rect.y + (rect.height - size) * .5f, size, size);
                GUI.color = ink;
                GUI.DrawTexture(square, texture, ScaleMode.StretchToFill, true);
            }
            finally { GUI.matrix = matrix; GUI.color = color; GUI.contentColor = content; }
        }

        /// <summary>Optional warm-up outside OnGUI. All later Draw calls reuse these textures.</summary>
        public static void Warmup()
        {
            for (int i = 0; i <= (int)Mark.Help; i++)
            {
                Mark mark = (Mark)i;
                Texture2D texture;
                if (!Cache.TryGetValue(mark, out texture) || texture == null) Cache[mark] = Rasterize(mark);
            }
        }

        /// <summary>Call on the owning HUD's destruction. Shared whiteTexture is never owned or destroyed.</summary>
        public static void Release()
        {
            foreach (Texture2D texture in Cache.Values)
                if (texture != null)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(texture);
                    else UnityEngine.Object.DestroyImmediate(texture);
                }
            Cache.Clear();
        }

        private static void Path(List<Stroke> strokes, params float[] xy)
        {
            for (int i = 2; i + 1 < xy.Length; i += 2)
                strokes.Add(new Stroke(xy[i - 2], xy[i - 1], xy[i], xy[i + 1]));
        }

        private static void Ring(List<Stroke> strokes, float x, float y, float radius)
        {
            const int steps = 32;
            for (int i = 0; i < steps; i++)
            {
                float a = i * Mathf.PI * 2f / steps, b = (i + 1) * Mathf.PI * 2f / steps;
                strokes.Add(new Stroke(x + Mathf.Cos(a) * radius, y + Mathf.Sin(a) * radius,
                    x + Mathf.Cos(b) * radius, y + Mathf.Sin(b) * radius));
            }
        }

        private static void House(List<Stroke> s)
        { Path(s, 4,14, 16,4, 28,14); Path(s, 7,12, 7,27, 25,27, 25,12); }

        private static void Shield(List<Stroke> s)
        { Path(s, 16,4, 26,8, 25,18, 22,24, 16,28, 10,24, 7,18, 6,8, 16,4); }

        private static List<Stroke> Geometry(Mark mark)
        {
            var s = new List<Stroke>(48);
            switch (mark)
            {
                case Mark.Play:
                    Path(s, 11,5, 26,16, 11,27, 11,5); break;
                case Mark.Continue:
                    Path(s, 5,7, 5,25); Path(s, 10,16, 27,16); Path(s, 20,9, 27,16, 20,23); break;
                case Mark.Book:
                    Path(s, 16,9, 12,6, 4,6, 4,25, 12,25, 16,28, 20,25, 28,25, 28,6, 20,6, 16,9, 16,28);
                    Path(s, 8,12, 12,12); Path(s, 20,12, 24,12); break;
                case Mark.Notebook:
                    Path(s, 9,5, 26,5, 26,28, 7,28, 7,5, 9,5); Path(s, 11,5, 11,28);
                    Path(s, 5,10, 9,10); Path(s, 5,17, 9,17); Path(s, 5,24, 9,24);
                    Path(s, 16,12, 22,12); Path(s, 16,18, 22,18); break;
                case Mark.Bag:
                    Path(s, 11,8, 11,5, 21,5, 21,8); Path(s, 9,8, 23,8, 26,12, 26,28, 6,28, 6,12, 9,8);
                    Path(s, 10,18, 22,18, 22,25, 10,25, 10,18); Path(s, 16,12, 16,14); break;
                case Mark.Settings:
                    Ring(s,16,16,4); Path(s, 12,5, 20,5, 21,9, 25,10, 28,14, 25,17, 25,22, 21,24, 20,28, 12,28, 11,24, 7,22, 7,17, 4,14, 7,10, 11,9, 12,5); break;
                case Mark.Exit:
                    Path(s, 15,5, 6,5, 6,27, 15,27); Path(s, 13,16, 28,16); Path(s, 22,10, 28,16, 22,22); break;
                case Mark.Home:
                    House(s); Path(s, 13,27, 13,19, 19,19, 19,27); break;
                case Mark.Back:
                    Path(s, 26,16, 6,16, 15,7); Path(s, 6,16, 15,25); break;
                case Mark.Pause:
                    Path(s, 9,6, 13,6, 13,26, 9,26, 9,6); Path(s, 20,6, 24,6, 24,26, 20,26, 20,6); break;
                case Mark.Sound:
                    Path(s, 5,12, 11,12, 18,6, 18,26, 11,20, 5,20, 5,12);
                    Path(s, 23,11, 25,14, 25,18, 23,21); Path(s, 27,7, 30,12, 30,20, 27,25); break;
                case Mark.Motion:
                    Ring(s,18,6,2); Path(s, 14,12, 18,10, 22,15, 27,15); Path(s, 18,11, 16,20, 10,27);
                    Path(s, 16,19, 22,22, 24,28); Path(s, 14,12, 10,17, 6,17); Path(s, 4,8, 10,8); Path(s, 3,23, 7,23); break;
                case Mark.Check:
                    Path(s, 6,16, 13,23, 27,8); break;
                case Mark.Clock:
                    Ring(s,16,17,11); Path(s, 16,10, 16,17, 22,20); Path(s, 12,3, 20,3); break;
                case Mark.Warning:
                    Path(s, 16,4, 29,27, 3,27, 16,4); Path(s, 16,12, 16,19); Path(s, 16,23, 16,23.3f); break;
                case Mark.Budget:
                    Path(s, 3,7, 29,7, 29,25, 3,25, 3,7); Ring(s,16,16,5);
                    Path(s, 7,11, 7,14); Path(s, 25,18, 25,21); break;
                case Mark.Damage:
                    Shield(s); Path(s, 18,5, 14,13, 20,15, 13,24); break;
                case Mark.Shield:
                    Shield(s); Path(s, 11,16, 16,12, 21,16); Path(s, 12,15, 12,21, 20,21, 20,15); break;
                case Mark.Earthquake:
                    House(s); Path(s, 18,7, 13,15, 19,17, 14,27); Path(s, 2,22, 4,24, 2,27); Path(s, 29,19, 27,22, 30,25); break;
                case Mark.Flood:
                    Path(s, 7,14, 16,6, 25,14); Path(s, 10,12, 10,20); Path(s, 22,12, 22,20); Path(s, 15,14, 18,14, 18,19);
                    Path(s, 3,22, 7,20, 12,22, 17,20, 22,22, 27,20, 29,21);
                    Path(s, 3,28, 7,26, 12,28, 17,26, 22,28, 27,26, 29,27); break;
                case Mark.Fire:
                    Path(s, 17,3, 18,11, 23,8, 23,14, 27,18, 26,24, 22,28, 11,28, 6,24, 5,18, 8,12, 9,18, 13,13, 17,3);
                    Path(s, 16,17, 20,23, 18,28, 14,28, 12,24, 16,17); break;
                case Mark.Storm:
                    Path(s, 3,10, 22,10, 25,8, 25,5, 22,3, 19,5);
                    Path(s, 4,16, 27,16, 29,18, 29,21, 26,23, 23,21);
                    Path(s, 3,23, 15,23, 18,25, 18,28, 15,30, 12,28); break;
                case Mark.Water:
                    Path(s, 16,3, 25,17, 26,21, 24,26, 20,29, 12,29, 8,26, 6,21, 7,17, 16,3);
                    Path(s, 11,20, 12,24, 16,26); break;
                case Mark.Food:
                    Path(s, 16,9, 10,7, 5,11, 4,18, 7,26, 12,29, 16,27, 20,29, 25,26, 28,18, 27,11, 22,7, 16,9);
                    Path(s, 16,9, 17,4, 21,2); Path(s, 16,7, 11,3, 7,3); break;
                case Mark.Tools:
                    Path(s, 5,4, 5,10, 10,14, 23,28, 27,24, 14,11, 13,5, 8,3, 10,9, 7,10, 5,4);
                    Path(s, 22,4, 28,10, 24,14, 18,8, 22,4); Path(s, 19,11, 5,26, 7,28, 21,14); break;
                case Mark.Wood:
                    Path(s, 7,5, 27,5, 25,27, 5,27, 7,5); Path(s, 12,7, 11,15, 8,22); Path(s, 20,7, 18,12, 20,19, 17,25); Ring(s,14,19,2); break;
                case Mark.Metal:
                    Path(s, 4,6, 28,6, 28,11, 20,11, 20,22, 28,22, 28,27, 4,27, 4,22, 12,22, 12,11, 4,11, 4,6); break;
                case Mark.Screw:
                    Path(s, 8,5, 24,5, 24,10, 8,10, 8,5); Path(s, 11,7.5f, 21,7.5f);
                    Path(s, 12,10, 12,24, 16,29, 20,24, 20,10); Path(s, 11,15, 21,12); Path(s, 11,20, 21,17); Path(s, 12,25, 20,22); break;
                case Mark.Tape:
                    Ring(s,15,15,11); Ring(s,15,15,5); Path(s, 25,18, 29,24, 27,28, 18,26); break;
                case Mark.Plastic:
                    Path(s, 12,3, 20,3, 20,7, 23,11, 23,28, 9,28, 9,11, 12,7, 12,3); Path(s, 10,13, 22,13); Path(s, 10,23, 22,23); break;
                case Mark.Bolt:
                    Path(s, 10,4, 22,4, 26,10, 22,16, 10,16, 6,10, 10,4); Ring(s,16,10,3);
                    Path(s, 12,16, 12,28, 20,28, 20,16); Path(s, 12,22, 20,20); Path(s, 12,26, 20,24); break;
                case Mark.Person:
                    Ring(s,16,9,5); Path(s, 6,28, 6,24, 9,19, 16,17, 23,19, 26,24, 26,28); break;
                case Mark.Help:
                    Ring(s,16,16,12); Path(s, 11,11, 13,8, 18,8, 21,11, 20,14, 16,17, 16,20); Path(s, 16,24, 16,24.3f); break;
            }
            return s;
        }

        private static Texture2D Rasterize(Mark mark)
        {
            // Each segment is a capsule with a one-texel antialias fringe. Geometry allocations
            // occur only when creating a cache entry, never on subsequent repaint calls.
            List<Stroke> geometry = Geometry(mark);
            var pixels = new Color32[TextureSize * TextureSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255,255,255,0);
            float scale = TextureSize / Grid, radius = StrokeWidth * scale * .5f;
            foreach (Stroke stroke in geometry)
            {
                Vector2 a = stroke.A * scale, b = stroke.B * scale, ab = b - a;
                float lengthSquared = ab.sqrMagnitude;
                int left = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x,b.x)-radius-1));
                int right = Mathf.Min(TextureSize-1, Mathf.CeilToInt(Mathf.Max(a.x,b.x)+radius+1));
                int top = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y,b.y)-radius-1));
                int bottom = Mathf.Min(TextureSize-1, Mathf.CeilToInt(Mathf.Max(a.y,b.y)+radius+1));
                for (int y = top; y <= bottom; y++)
                    for (int x = left; x <= right; x++)
                    {
                        Vector2 p = new Vector2(x+.5f,y+.5f);
                        float t = lengthSquared > .00001f ? Mathf.Clamp01(Vector2.Dot(p-a,ab)/lengthSquared) : 0f;
                        float distance = (p-a-ab*t).magnitude;
                        byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius+.5f-distance)*255f);
                        int index = (TextureSize-1-y)*TextureSize+x;
                        if (alpha > pixels[index].a) pixels[index].a = alpha;
                    }
            }
            var texture = new Texture2D(TextureSize,TextureSize,TextureFormat.RGBA32,true,true);
            texture.name = "TLTP UI V57 " + mark;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Trilinear;
            texture.SetPixels32(pixels);
            texture.Apply(true,true);
            return texture;
        }
    }
}
