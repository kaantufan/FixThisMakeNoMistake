using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        string secondaryAlertKeyV55 = string.Empty;
        string[] secondaryAlertLinesV55 = Array.Empty<string>();
        float secondaryAlertRemainingV55;
        GUIStyle secondaryAlertBodyV55;
        public bool SecondaryAlertVisibleV55
        {
            get { return secondaryAlertRemainingV55 > 0 && secondaryAlertLinesV55.Length > 0 && World &&
                !World.Model.IsPaused && !World.NotebookBlocksInput && !_paused && !_developerMenuOpen &&
                World.Model.Phase != PanicPhase.Title && !FreeplayOnboardingOpenV414; }
        }
        public int SecondaryAlertDrawFramesV55 { get; private set; }
        public string SecondaryAlertTextV55 { get { return string.Join("\n", secondaryAlertLinesV55); } }
        public Rect SecondaryAlertBoundsV55 { get; private set; }
        public void ResetSecondaryAlertV55()
        { secondaryAlertKeyV55 = string.Empty; secondaryAlertRemainingV55 = 0; secondaryAlertLinesV55 = Array.Empty<string>(); }
        public void TickSecondaryAlertV55(float dt)
        {
            if (!World || World.Model == null) return;
            var m = World.Model;
            if (m.Phase == PanicPhase.Title || m.Phase == PanicPhase.Preparing || m.Phase == PanicPhase.Tutorial)
            { if (m.SecondaryEventsV5.Count == 0) secondaryAlertRemainingV55 = 0; return; }
            // Read the already committed outcome. Never evaluates damage or draws RNG.
            if (m.SecondaryEventsV5.Count > 0 && (m.Phase == PanicPhase.Aftermath || m.Phase == PanicPhase.Inspection || m.Phase == PanicPhase.GameOver || m.Phase == PanicPhase.Success))
            {
                string key = m.Wave + "|" + m.SecondaryEventSummaryV5;
                if (key != secondaryAlertKeyV55)
                {
                    secondaryAlertKeyV55 = key;
                    var lines = new List<string>();
                    foreach (var e in m.SecondaryEventsV5)
                    {
                        string cause = e.Kind == PanicSecondaryEventKindV5.GasFlare ? "Gaz vanası açık kaldı: alevlenme!" :
                            e.Kind == PanicSecondaryEventKindV5.PipeLeak ? "Su vanası açık kaldı: boru sızıntısı!" : "Elektrik kesilmedi: kısa devre!";
                        lines.Add(cause + "  +" + e.DamagePoints.ToString("0.0") + " hasar");
                    }
                    secondaryAlertLinesV55 = lines.ToArray(); secondaryAlertRemainingV55 = 7f;
                }
            }
            if (!m.IsPaused && !World.NotebookBlocksInput && !_developerMenuOpen)
                secondaryAlertRemainingV55 = Mathf.Max(0, secondaryAlertRemainingV55 - Mathf.Clamp(dt, 0, .1f));
        }
        void DrawSecondaryAlertV55()
        {
            if (!SecondaryAlertVisibleV55) return;
            if (secondaryAlertBodyV55 == null) secondaryAlertBodyV55 = new GUIStyle(_body) { fontSize = 25, wordWrap = true };
            float width = 780, height = 51;
            foreach (var line in secondaryAlertLinesV55) height += Mathf.Max(31, secondaryAlertBodyV55.CalcHeight(new GUIContent(line), width - 44));
            var rect = new Rect((1600 - width) * .5f, 112, width, height + 12);
            SecondaryAlertBoundsV55 = rect;
            Frame(rect, Night, Ember, 2);
            Fill(new Rect(rect.x + 1, rect.y + 1, 6, rect.height - 2), Ember);
            Label(new Rect(rect.x + 23, rect.y + 9, rect.width - 46, 33), "!  TESİSAT UYARISI", ColorText(_heading, Brass));
            float y = rect.y + 49;
            foreach (var line in secondaryAlertLinesV55)
            {
                float h = Mathf.Max(31, secondaryAlertBodyV55.CalcHeight(new GUIContent(line), width - 44));
                Label(new Rect(rect.x + 23, y, rect.width - 46, h), line, secondaryAlertBodyV55); y += h;
            }
            if (Event.current.type == EventType.Repaint) SecondaryAlertDrawFramesV55++;
        }
    }
}
