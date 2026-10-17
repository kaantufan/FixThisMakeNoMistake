using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    // Imported ElevenLabs Foley. No synthetic fallback may impersonate a missing asset.
    public static class PanicAudioBank
    {
        static readonly Dictionary<string, AudioClip[]> cache = new Dictionary<string, AudioClip[]>();
        static readonly Dictionary<string, int> previous = new Dictionary<string, int>();
        public static AudioClip Get(string id)
        {
            if (!cache.TryGetValue(id, out var clips))
            {
                var list = new List<AudioClip>();
                var single = Resources.Load<AudioClip>("Audio/Foley/" + id);
                if (single) list.Add(single);
                for (int i = 0; i < 4; i++)
                {
                    var variant = Resources.Load<AudioClip>("Audio/Foley/" + id + "_" + i);
                    if (variant) list.Add(variant);
                }
                cache[id] = clips = list.ToArray();
            }
            if (clips.Length == 0) return null;
            int index = Random.Range(0, clips.Length);
            if (clips.Length > 1 && previous.TryGetValue(id, out int old) && old == index) index = (index + 1) % clips.Length;
            previous[id] = index;
            return clips[index];
        }
        public static string Pickup(string id)
        {
            switch (id)
            {
                case "screws": case "bracket": case "battery": case "extinguisher": return "pickup_metal";
                case "water": case "antiseptic": return "pickup_bottle";
                case "plank": return "wood_build";
                case "sandbag": return "sandbag";
                case "seedling": return "soil_build";
                case "hose": return "hose";
                default: return "pickup_cloth";
            }
        }
        public static string Work(string action)
        {
            switch (action)
            {
                case "anchorwall": case "anchorfloor": return "screw";
                case "plant": case "drain": return "soil_build";
                case "flood": return "sandbag";
                case "hose": return "hose";
                case "fire": return "door_toolbox";
                default: return "wood_build";
            }
        }
    }
}
