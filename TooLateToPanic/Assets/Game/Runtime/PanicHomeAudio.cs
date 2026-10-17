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
        string StepSurface()
        {
            Vector3 p = PlayerPosition;
            if (p.y < -1 || p.x > 9 && p.x < 17 && p.z > -.5f && p.z < 7) return "step_concrete";
            if (!InResidence(p)) return "step_grass";
            if (p.y > 2 && p.x > 2 && p.z > 1.8f || p.y < 2 && p.x > 2 && p.z > 2) return "step_tile";
            return "step_wood";
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
            if (bunkerOpen != audioBunkerOpen) { audioBunkerOpen=bunkerOpen; effects.Play("door_metal",.45f); }
            if (audioMessage != Model.Message)
            {
                audioMessage=Model.Message;
                string m=(audioMessage??"").ToLowerInvariant();
                if (m.Contains("eksik") || m.Contains("yetmiyor") || m.Contains("dolu") || m.Contains("gerekiyor")) effects.Play("reject",.22f);
            }
        }
    }
}
