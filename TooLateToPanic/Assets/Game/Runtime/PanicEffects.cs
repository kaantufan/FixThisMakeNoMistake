using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicEffects : MonoBehaviour
    {
        public PanicWorld World;
        AudioSource oneShot, ambience, roomTone;
        GameObject disasterVfx;
        float phaseAge;
        bool warned;

        void Awake()
        {
            oneShot = gameObject.AddComponent<AudioSource>();
            oneShot.spatialBlend = 0; oneShot.volume = .65f;
            ambience = gameObject.AddComponent<AudioSource>();
            ambience.spatialBlend = 0; ambience.loop = true; ambience.volume = .25f;
            roomTone = gameObject.AddComponent<AudioSource>();
            roomTone.clip = Resources.Load<AudioClip>("Audio/roomtone");
            roomTone.loop = true; roomTone.volume = .14f;
            if (roomTone.clip != null) roomTone.Play();
        }

        public void Play(string name, float volume = .5f)
        {
            string id = name;
            if (name == "step") id = "woodstep" + Random.Range(0, 4);
            else if (name == "select") id = "click";
            var sound = Resources.Load<AudioClip>("Audio/" + id);
            if (sound == null) sound = PanicArt.GetSound(name == "step" ? "anchor" : name == "build" || name == "buy" ? "upgrade" : name == "bunker" ? "anchor" : id);
            if (sound != null) oneShot.PlayOneShot(sound, volume);
        }

        public void OnPhase(PanicPhase phase)
        {
            phaseAge = 0;
            warned = false;
            if (phase == PanicPhase.Disaster)
            {
                if (disasterVfx != null) Destroy(disasterVfx);
                disasterVfx = PanicArt.CreateDisasterVfx(World.Model.Disaster);
                string soundId = World.Model.Disaster == DisasterType.Earthquake ? "earthquake" : World.Model.Disaster == DisasterType.Flood ? "flood" : "fire";
                ambience.clip = Resources.Load<AudioClip>("Audio/" + soundId);
                if (ambience.clip == null) ambience.clip = PanicArt.GetSound(soundId);
                ambience.Play();
            }
            else if (phase == PanicPhase.Aftermath)
            {
                ambience.volume = .10f;
            }
            else
            {
                ambience.Stop(); ambience.volume = .25f;
                if (disasterVfx != null) Destroy(disasterVfx);
                disasterVfx = null;
                if (phase == PanicPhase.Success) Play("success", .65f);
                if (phase == PanicPhase.GameOver) Play("fail", .65f);
            }
        }

        void Update()
        {
            if (World == null || World.Model == null) return;
            phaseAge += Time.deltaTime;
            var model = World.Model;
            if (model.Phase == PanicPhase.Preparing && model.TimeRemaining <= 60 && !warned)
            {
                warned = true; Play("warning", .65f);
            }
            if (disasterVfx != null)
            {
                float strength = model.Phase == PanicPhase.Disaster ? Mathf.Clamp01(phaseAge * .5f) : .35f;
                PanicArt.AnimateDisasterVfx(disasterVfx, phaseAge, strength);
            }
        }
    }
}
