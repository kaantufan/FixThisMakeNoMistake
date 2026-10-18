using System;
using System.Collections.Generic;
using System.Text;

namespace TooLateToPanic
{
    /// <summary>Bounded presentation state; deliberately has no Unity, model, RNG or input dependency.</summary>
    public sealed class PanicDialogueSessionV59
    {
        public const int MaximumCharacters = 132;
        public const int MaximumBlipsPerCaption = 3;
        readonly HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        string scope = "";
        float age, remaining, clock, lastAccepted = -100f;
        int priority, emittedBlips;
        public string Text { get; private set; } = "";
        public string Key { get; private set; } = "";
        public bool Active { get { return remaining > 0f && Text.Length > 0; } }
        public float Remaining { get { return remaining; } }
        public int BlipsEmitted { get { return emittedBlips; } }
        public int PendingCount { get { return 0; } }

        public void BeginScope(string value)
        {
            value = value ?? "";
            if (scope == value) return;
            Reset(); scope = value;
        }
        public bool TryStart(string key, string text, int importance, float lifetime = 0f)
        {
            if (string.IsNullOrEmpty(key) || seen.Contains(key)) return false;
            string clean = Clean(text);
            if (clean.Length == 0) return false;
            // Equal/lower priority events never replace an active line. No deferred queue.
            if (Active && (importance <= priority || clock - lastAccepted < .65f)) return false;
            Text = clean; Key = key; priority = importance;
            remaining = lifetime > 0f ? Math.Max(2.5f, Math.Min(6.5f, lifetime)) : Math.Max(3.2f, Math.Min(6.5f, 1.8f + clean.Length / 23f));
            age = 0f; emittedBlips = 0; lastAccepted = clock; seen.Add(key);
            return true;
        }
        public void Tick(float dt, bool suspended)
        {
            if (suspended || float.IsNaN(dt) || float.IsInfinity(dt) || dt <= 0f) return;
            // A hitch must not emit a backlog of blips or age text by minutes.
            dt = Math.Min(dt, .1f); clock += dt;
            if (!Active) return;
            age += dt; remaining = Math.Max(0f, remaining - dt);
            if (remaining == 0f) Cancel();
        }
        public bool TakeBlip(bool audible)
        {
            if (!Active || emittedBlips >= MaximumBlipsPerCaption || age < emittedBlips * .22f) return false;
            // Muting consumes due bits; unmuting never replays old character audio.
            emittedBlips++;
            return audible;
        }
        public void Cancel()
        { Text = Key = ""; remaining = age = 0f; priority = 0; emittedBlips = 0; }
        public void Reset()
        { Cancel(); seen.Clear(); scope = ""; clock = 0f; lastAccepted = -100f; }
        public static string Clean(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            var b = new StringBuilder(); bool space = false;
            // Strip markup as well as line breaks: the HUD draws at most two authored lines.
            bool tag = false;
            foreach (char c in value)
            {
                if (c == '<') { tag = true; continue; }
                if (c == '>') { tag = false; continue; }
                if (tag) continue;
                if (char.IsWhiteSpace(c) || char.IsControl(c)) { space = b.Length > 0; continue; }
                if (space) { b.Append(' '); space = false; }
                b.Append(c);
            }
            string clean = b.ToString().Trim();
            if (clean.Length <= MaximumCharacters) return clean;
            int end = clean.LastIndexOf(' ', MaximumCharacters - 2, MaximumCharacters - 2);
            if (end < MaximumCharacters / 2) end = MaximumCharacters - 1;
            return clean.Substring(0, end).TrimEnd() + "…";
        }
    }
}
