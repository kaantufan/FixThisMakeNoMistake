using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        int audioWorkStep = -1, audioWorkStrokesV56;
        string audioWorkTarget, audioMessage;
        bool audioWorkApplying, audioBunkerOpen, audioWorkCommittedV56;
        float audioWorkProgressV56;
        int audioUtilityBucketV56;
        int secondaryAudioWaveV56 = -1;
        string secondaryAudioSummaryV56;
        readonly System.Collections.Generic.HashSet<string> secondaryAudioKeysV56 = new System.Collections.Generic.HashSet<string>();
        public int AcceptedHammerAudioCountV56 { get; private set; }
        public string LastWorkAudioCueV56 { get; private set; }
        public int WorkAudioDispatchCountV56 { get; private set; }
        public int SecondaryAudioDispatchCountV56 { get; private set; }
        public string LastSecondaryAudioCueV56 { get; private set; }
        public bool AudioExteriorCameraV56 { get { return ResidenceCinematicExteriorView; } }
        float audioWave = -1;
        readonly System.Collections.Generic.Dictionary<string,float> heardDamage = new System.Collections.Generic.Dictionary<string,float>();
        float nextDamageSound;
        PanicFootstepSurface footstepSurface;
        public float AudioShelterAmount { get { return EvaluateAudioShelterAmountV56(PlayerPosition); } }
        public float AudioLegacyBunkerDistanceV56 { get { return Vector3.Distance(PlayerPosition, BunkerInteriorPosition); } }
        public bool AudioPhysicalBasementV56 { get { return IsPhysicalBasementAudioV56(PlayerPosition); } }
        public static bool IsPhysicalBasementAudioV56(Vector3 position)
        {
            // Physical house footprint and the lower half of its stairs. V5 admits
            // the real basement; the distant legacy room is only a later cinematic set.
            // This affects sound only and never marks Model.ReachedShelterV5.
            return position.y < -1.6f && Mathf.Abs(position.x) < 8.05f && Mathf.Abs(position.z) < 6.05f;
        }
        public static float EvaluateAudioShelterAmountV56(Vector3 position)
        {
            return IsPhysicalBasementAudioV56(position) || Vector3.Distance(position, BunkerInteriorPosition) < 8f ? 1f : 0f;
        }
        public float AudioHelicopterDistance { get { return helicopter ? Vector3.Distance(PlayerPosition,helicopter.transform.position) : 1000f; } }
        public bool AudioHatchOpen { get { return bunkerLeaf && Quaternion.Angle(bunkerLeaf.localRotation,bunkerClosed)>20; } }
        public string CurrentFootstepSurface { get { return StepSurface(); } }
        string StepSurface() { return FootstepSurfaceAt(PlayerPosition); }
        public string FootstepSurfaceAt(Vector3 position)
        {
            if(footstepSurface==null)
            {
                footstepSurface=new PanicFootstepSurface();footstepSurface.Add(landscape);
                foreach(var floor in floorVisuals)footstepSurface.Add(floor);
                footstepSurface.Add(residenceGarage);footstepSurface.Add(residenceBalcony);footstepSurface.Add(residenceBathroom);
            }
            if(Vector3.Distance(position,BunkerInteriorPosition)<8)return "step_concrete";
            return footstepSurface.Sample(position,InResidence(position)?"step_wood":"step_concrete");
        }
        public PanicFootstepSurface.SampleTrace FootstepSurfaceTraceAt(Vector3 position)
        {
            FootstepSurfaceAt(position); // Ensure the same authored-mesh cache as production footsteps.
            return footstepSurface.Trace(position,InResidence(position)?"step_wood":"step_concrete");
        }
        string ContainerAudio(string id)
        {
            if (id == "fridge") return "door_fridge";
            if (id == "garage_tools") return "door_toolbox";
            if (id == "bath_cabinet") return "door_metal";
            return "door_cabinet";
        }
        public void PlayUiSound(string id = "ui") { if (effects) effects.Play(id,.25f); }
        static string UtilityAudioPrefixV56(string action)
        { return action == "gasoff" ? "v56_gas_" : action == "wateroff" ? "v56_water_" : "v56_power_"; }
        void PlayWorkCueV56(string cue, float volume)
        {
            if (!effects) return;
            int before = effects.PlayedCount; effects.Play(cue, volume);
            if (effects.PlayedCount > before) { LastWorkAudioCueV56 = cue; WorkAudioDispatchCountV56++; }
        }
        // Called only after BeginValidatedWork succeeds. A hammer starts silently.
        public void BeginWorkAudioV56()
        {
            audioWorkTarget = workTarget; audioWorkStep = 0; audioWorkStrokesV56 = 0;
            audioWorkApplying = false; audioWorkProgressV56 = 0; audioUtilityBucketV56 = 0; audioWorkCommittedV56 = false;
            if (effects && Expedition.Work.IsUtility) PlayWorkCueV56(UtilityAudioPrefixV56(workAction) + "start", .23f);
        }
        void TickWorkAudio()
        {
            if (!effects || Expedition == null || !WorkActive) return;
            var work = Expedition.Work;
            if (audioWorkTarget != workTarget) BeginWorkAudioV56();
            if (work.UsesHammer)
            {
                // Accepted strokes are authoritative, including the fourth one before Cancel clears the session.
                int accepted = Mathf.Clamp(work.CompletedStrokes, 0, work.RequiredStrokes);
                while (audioWorkStrokesV56 < accepted)
                { audioWorkStrokesV56++; PlayWorkCueV56("v56_hammer_hit", .42f); AcceptedHammerAudioCountV56++; }
            }
            else if (work.IsUtility)
            {
                int bucket = Mathf.FloorToInt(Mathf.Clamp01(work.Progress) * 4f);
                if (work.Progress > audioWorkProgressV56 + .00001f && (bucket > audioUtilityBucketV56 || audioWorkProgressV56 <= 0))
                    PlayWorkCueV56(PanicAudioBank.Work(workAction), .27f);
                audioUtilityBucketV56 = bucket; audioWorkProgressV56 = work.Progress;
            }
            else if (work.IsInstalling && (audioWorkStep != work.CompletedSteps || !audioWorkApplying))
                PlayWorkCueV56(PanicAudioBank.Work(workAction), .42f);
            audioWorkStep = work.CompletedSteps; audioWorkApplying = work.IsInstalling;
        }
        public void CompleteWorkAudioV56()
        {
            if (audioWorkCommittedV56) return;
            TickWorkAudio(); audioWorkCommittedV56 = true;
            if (effects && PanicUtilitySafetyV414.IsAction(workAction)) PlayWorkCueV56(UtilityAudioPrefixV56(workAction) + "close", .32f);
        }
        public void EndWorkAudioV56()
        {
            if (effects && WorkActive && !audioWorkCommittedV56 && Expedition.Work.IsUtility)
                PlayWorkCueV56(UtilityAudioPrefixV56(workAction) + "cancel", .20f);
            audioWorkTarget = null; audioWorkStep = -1; audioWorkApplying = false;
            audioWorkStrokesV56 = 0; audioWorkProgressV56 = 0; audioWorkCommittedV56 = false;
        }
        void TickSecondaryAudioV56(bool paused)
        {
            if (Model.Phase == PanicPhase.Title || secondaryAudioWaveV56 != Model.Wave)
            { secondaryAudioKeysV56.Clear(); secondaryAudioWaveV56 = Model.Wave; secondaryAudioSummaryV56 = null; }
            if (Model.Phase == PanicPhase.Title || Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial)
            { if (secondaryAudioSummaryV56 != null) { secondaryAudioSummaryV56 = null; secondaryAudioKeysV56.Clear(); } return; }
            if (paused) return;
            string summary = Model.SecondaryEventSummaryV5;
            if (string.IsNullOrEmpty(summary) || summary == secondaryAudioSummaryV56) return;
            secondaryAudioSummaryV56 = summary;
            // Read only committed events. No Roll/Resolve/Evaluate, no extra damage, and no random selection.
            var events = Model.SecondaryEventsV5;
            for (int i = 0; i < events.Count; i++)
            {
                var incident = events[i];
                string key = i + "|" + (int)incident.Kind + "|" + incident.TargetId;
                if (!secondaryAudioKeysV56.Add(key)) continue;
                string cue = incident.Kind == PanicSecondaryEventKindV5.GasFlare ? "v56_gas_flare" :
                    incident.Kind == PanicSecondaryEventKindV5.PipeLeak ? "v56_pipe_leak" : "v56_electrical_short";
                int before = effects.PlayedCount; effects.Play(cue, .46f);
                if (effects.PlayedCount > before) { SecondaryAudioDispatchCountV56++; LastSecondaryAudioCueV56 = cue; }
            }
        }
        void TickHouseAudio()
        {
            if (!effects || Model == null) return;
            bool paused = InputLocked || Model.IsPaused || Model.WaveLedgerOpenV56 || Model.KeyboardHelpOpenV56 || NotebookBlocksInput || (!Application.isFocused && !Application.runInBackground);
            effects.SetSuspended(paused);
            TickSecondaryAudioV56(paused);
            effects.SetEnvironment(Model.Phase != PanicPhase.Title && !paused, inside, hearthEnabled && inside, helicopter && helicopter.activeInHierarchy);
            if (!WorkActive) { audioWorkTarget = null; audioWorkStep = -1; audioWorkApplying = false; audioWorkStrokesV56 = 0; }
            if (Model.Phase == PanicPhase.Title || Model.Phase == PanicPhase.Preparing) heardDamage.Clear();
            else if (!paused && Time.unscaledTime >= nextDamageSound)
            {
                foreach(var item in Model.Items)
                {
                    heardDamage.TryGetValue(item.Id,out float before);
                    if(item.Damage <= before)continue;
                    heardDamage[item.Id]=item.Damage;
                    if(item.Damage-before<5)continue;
                    string id=item.Kind==ItemKind.Window?"impact_glass":item.ArtId=="fridge"?"impact_metal":"impact_wood";
                    effects.Play(id,.28f);nextDamageSound=Time.unscaledTime+.4f;break;
                }
            }
            if (audioWave != Model.Wave) { if(audioWave >= 0 && Model.Phase == PanicPhase.Preparing) effects.Play("objective_tick",.35f); audioWave=Model.Wave; }
            bool bunkerOpen = bunkerLeaf && Quaternion.Angle(bunkerLeaf.localRotation,bunkerClosed) > 20;
            if (bunkerOpen != audioBunkerOpen) { audioBunkerOpen=bunkerOpen; effects.Play("bunker_hatch",.30f); }
            if (audioMessage != Model.Message)
            {
                audioMessage=Model.Message;
                string m=(audioMessage??"").ToLowerInvariant();
                if (m.Contains("eksik") || m.Contains("yetmiyor") || m.Contains("dolu") || m.Contains("gerekiyor")) effects.Play("reject",.22f);
            }
        }
    }
}
