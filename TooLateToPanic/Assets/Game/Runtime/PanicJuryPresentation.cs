using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        PanicJuryStage observedJuryStage = PanicJuryStage.Inactive;
        bool observedJuryBag;
        LineRenderer juryMarker;
        GameObject juryOutcomeAtmosphere;
        int observedOutcomeCount;

        void ResetJuryPresentation()
        {
            observedJuryStage = PanicJuryStage.Inactive; observedJuryBag = false;
            observedOutcomeCount = 0;
            if (juryOutcomeAtmosphere) Destroy(juryOutcomeAtmosphere);
            juryOutcomeAtmosphere = null;
            if (juryMarker) juryMarker.gameObject.SetActive(false);
        }

        // Runs after the phase observer, which cancels a previous phase's voice.
        void ObserveJuryPresentation()
        {
            if (!JuryActive || JurySnapshot == null) return;
            var snapshot = JurySnapshot;
            if (snapshot.Outcomes.Count != observedOutcomeCount)
            {
                observedOutcomeCount = snapshot.Outcomes.Count;
                if (juryOutcomeAtmosphere) Destroy(juryOutcomeAtmosphere);
                juryOutcomeAtmosphere = null;
                var outcome = snapshot.Outcomes[observedOutcomeCount - 1];
                if (outcome.TotalDamage > 0f) juryOutcomeAtmosphere = PanicAtmosphereArt.CreateAftermath(outcome.Disaster);
            }
            if (snapshot.Stage == PanicJuryStage.Inspection && effects) effects.StopJuryHazard();
            if (InputLocked || (hud && (hud.DeveloperMenuOpen || hud.DeveloperInputBlockedThisFrame))) return;
            if (observedJuryStage != snapshot.Stage)
            {
                observedJuryStage = snapshot.Stage;
                string cue = snapshot.Stage == PanicJuryStage.Onboarding ? "jury_intro" :
                    snapshot.Stage == PanicJuryStage.EarthquakeResult ? "jury_quake_result" :
                    snapshot.Stage == PanicJuryStage.Flood ? "jury_flood" :
                    snapshot.Stage == PanicJuryStage.Fire ? "jury_fire" :
                    snapshot.Stage == PanicJuryStage.Result || snapshot.Stage == PanicJuryStage.Complete ? "jury_end" : null;
                if (cue != null) PlayPresentationCue(cue);
            }
            if (!observedJuryBag && (snapshot.Stage == PanicJuryStage.Onboarding || snapshot.Stage == PanicJuryStage.Preparation))
            {
                foreach (var goal in snapshot.Goals)
                    if (goal.Id == "water_packed" && goal.State == PanicJuryGoalState.Complete)
                    { observedJuryBag = true; PlayPresentationCue("objective_tick"); break; }
            }
        }

        void UpdateJuryMarker()
        {
            if (juryOutcomeAtmosphere) PanicAtmosphereArt.AnimateAftermath(juryOutcomeAtmosphere, residenceTime);
            var snapshot = JuryActive ? JurySnapshot : null;
            bool show = snapshot != null && snapshot.HasWorldTarget && snapshot.AllowsMovement && !InputLocked && !WorkActive && !InventoryOpen && !(hud && hud.DeveloperMenuOpen);
            if (!show) { if (juryMarker) juryMarker.gameObject.SetActive(false); return; }
            if (!juryMarker)
            {
                var node = new GameObject("Jury visual target"); node.layer = 2; node.transform.SetParent(transform, false);
                juryMarker = node.AddComponent<LineRenderer>();
                juryMarker.useWorldSpace = false; juryMarker.loop = true; juryMarker.positionCount = 48;
                juryMarker.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                juryMarker.startWidth = juryMarker.endWidth = .045f;
                juryMarker.startColor = juryMarker.endColor = PresentationOrange;
                for (int i = 0; i < 48; i++)
                {
                    float a = i * Mathf.PI * 2f / 48f;
                    juryMarker.SetPosition(i, new Vector3(Mathf.Cos(a) * .75f, .035f, Mathf.Sin(a) * .75f));
                }
                juryMarker.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                juryMarker.receiveShadows = false;
            }
            juryMarker.gameObject.SetActive(true);
            juryMarker.transform.position = JuryGuidancePosition(snapshot, out _);
        }

        Vector3 JuryGuidancePosition(PanicJurySnapshot snapshot, out string label)
        {
            var target = snapshot.TargetPosition;
            if (!SameLevel(target))
            {
                bool up = target.y > PlayerPosition.y;
                label = up ? "MERDİVEN · ÜST KATA ÇIK" : "MERDİVEN · ALT KATA İN";
                if (up && ActiveFloor == 0 && PlayerPosition.z < -6.4f)
                { label = "ÖN KAPI · MERDİVENE GİT"; return new Vector3(0, .05f, -6.8f); }
                return new Vector3(0, ActiveFloor * 3.2f + .05f, up ? -.65f : 5.15f);
            }
            label = snapshot.ShelterKind != PanicJuryShelter.None ? "E  GÜVENLİ NOKTA" :
                snapshot.Stage == PanicJuryStage.Flood ? "DRENAJ · E İLE BAŞLA" :
                snapshot.Stage == PanicJuryStage.Fire ? "TÜPÜ YERLEŞTİR · E" :
                snapshot.Heading;
            return target;
        }

        void DrawJuryWorldTarget()
        {
            var snapshot = JuryActive ? JurySnapshot : null;
            if (snapshot == null || !snapshot.HasWorldTarget || !snapshot.AllowsMovement) return;
            if (snapshot.ShelterKind == PanicJuryShelter.None && focusId == snapshot.TargetId) return;
            Vector3 target = JuryGuidancePosition(snapshot, out string text);
            if (hud) hud.DrawWorldMarker(target + Vector3.up * 1.5f, text, 240, true);
        }
    }
}
