using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicHomeEffects : MonoBehaviour
    {
        public PanicHomeWorld World;
        AudioSource oneShot, ambience, roomTone;
        GameObject weather;
        float age;
        bool warned;
        void Awake()
        {
            oneShot = gameObject.AddComponent<AudioSource>(); oneShot.volume = .65f;
            ambience = gameObject.AddComponent<AudioSource>(); ambience.loop = true; ambience.volume = .32f;
            roomTone = gameObject.AddComponent<AudioSource>(); roomTone.loop = true; roomTone.volume = 0;
        }
        public void Play(string name, float volume = .5f)
        {
            if(PanicHomeWorld.DisasterAudioOnly)return;
            string id = name == "step" ? "woodstep" + Random.Range(0, 4) : name == "select" ? "click" : name;
            var clip = Resources.Load<AudioClip>("Audio/" + id);
            if (!clip) clip = PanicArt.GetSound(name == "step" ? "anchor" : name == "build" || name == "buy" ? "upgrade" : name == "bunker" ? "anchor" : id);
            if (clip) oneShot.PlayOneShot(clip, volume);
        }
        public void OnPhase(PanicPhase phase)
        {
            age = 0; warned = false;
            if (phase == PanicPhase.Disaster)
            {
                if (weather) Destroy(weather); weather = PanicDisasterArt.Create(World.Model.Disaster);
                string id = World.Model.Disaster == DisasterType.Earthquake ? "earthquake" : World.Model.Disaster == DisasterType.Flood ? "flood" : "fire";
                ambience.clip = Resources.Load<AudioClip>("Audio/" + id); if (!ambience.clip) ambience.clip = PanicArt.GetSound(id); ambience.volume = .32f; if (ambience.clip) ambience.Play();
            }
            else if (phase == PanicPhase.Aftermath) ambience.volume = .14f;
            else if (phase == PanicPhase.Success || phase == PanicPhase.Inspection || phase == PanicPhase.GameOver)
            {
                ambience.volume = .07f;
                if (phase == PanicPhase.Success) Play("success", .55f);
                if (phase == PanicPhase.GameOver) Play("fail", .6f);
            }
            else { ambience.Stop(); if (weather) Destroy(weather); weather = null; }
        }
        public void StartCinematicWeather()
        {
            if(!World)return;
            if(!weather)weather=PanicDisasterArt.Create(World.Model.Disaster);
            age=0;string id=World.Model.Disaster==DisasterType.Earthquake?"earthquake":World.Model.Disaster==DisasterType.Flood?"flood":"fire";
            ambience.clip=Resources.Load<AudioClip>("Audio/"+id);if(!ambience.clip)ambience.clip=PanicArt.GetSound(id);
            ambience.volume=.3f;if(ambience.clip)ambience.Play();
        }
        void Update()
        {
            if (!World || World.Model == null) return;
            age += Time.deltaTime;
            if (World.Model.Phase == PanicPhase.Preparing && World.Model.TimeRemaining <= 60 && !warned) { warned = true; Play("warning", .6f); }
            if (weather) PanicDisasterArt.Animate(weather, age, World.Model.Phase == PanicPhase.Disaster || World.CinematicActive&&World.Model.Phase==PanicPhase.GameOver ? Mathf.Clamp01(age * .4f) : .35f);
        }
    }
}
