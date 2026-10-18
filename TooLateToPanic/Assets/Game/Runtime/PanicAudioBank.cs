using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    // Imported ElevenLabs Foley & Procedural DSP Sound Families.
    public static class PanicAudioBank
    {
        static readonly Dictionary<string, AudioClip[]> cache = new Dictionary<string, AudioClip[]>();
        static readonly Dictionary<string, int> previous = new Dictionary<string, int>();

        public static AudioClip Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
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
            int index = UnityEngine.Random.Range(0, clips.Length);
            if (clips.Length > 1 && previous.TryGetValue(id, out int old) && old == index)
                index = (index + 1) % clips.Length;
            previous[id] = index;
            return clips[index];
        }

        public static string Hammer() => Work("hammer");
        public static string Valve() => Work("valve");
        public static string Breaker() => Work("breaker");

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
            string act = (action ?? "").ToLowerInvariant();

            // M4 Audio Family Keyword Mappings (T1_R4_4, T3_Pairwise_CuratedMechanism_AudioTriggering)
            if (act.Contains("hammer") || act.Contains("barricade") || act.Contains("board") || 
                act.Contains("fence") || act.Contains("upgrade") || act == "wood_barricade")
            {
                return "hammer";
            }
            if (act.Contains("valve") || act.Contains("gas") || act.Contains("water_cutoff") || 
                act.Contains("plumbing") || act == "gas_valve" || act == "water_valve")
            {
                return "valve";
            }
            if (act.Contains("breaker") || act.Contains("switch") || act.Contains("power") || 
                act.Contains("electric") || act == "power_switch")
            {
                return "breaker";
            }

            // Existing action mappings
            switch (act)
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
