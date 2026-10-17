using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        int audioWorkStep = -1; string audioWorkTarget, audioMessage;
        bool audioWorkApplying, audioBunkerOpen;
        float audioWave = -1;
        readonly System.Collections.Generic.Dictionary<string,float> heardDamage = new System.Collections.Generic.Dictionary<string,float>();
        float nextDamageSound;
        PanicFootstepSurface footstepSurface;
        public float AudioShelterAmount { get { return Vector3.Distance(PlayerPosition,BunkerInteriorPosition)<8 ? 1f : 0f; } }
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
        void TickWorkAudio()
        {
            if (audioWorkTarget != workTarget || audioWorkStep != Expedition.Work.CompletedSteps || !audioWorkApplying && Expedition.Work.IsInstalling)
            {
                if (Expedition.Work.IsInstalling) effects.Play(PanicAudioBank.Work(workAction), .42f);
                audioWorkTarget = workTarget; audioWorkStep = Expedition.Work.CompletedSteps;
            }
            audioWorkApplying = Expedition.Work.IsInstalling;
        }
        void TickHouseAudio()
        {
            if (!effects || Model == null) return;
            bool paused = InputLocked || Model.IsPaused || NotebookBlocksInput || (!Application.isFocused && !Application.runInBackground);
            effects.SetSuspended(paused);
            effects.SetEnvironment(Model.Phase != PanicPhase.Title && !paused, inside, hearthEnabled && inside, helicopter && helicopter.activeInHierarchy);
            if (!WorkActive) { audioWorkTarget = null; audioWorkStep = -1; audioWorkApplying = false; }
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
