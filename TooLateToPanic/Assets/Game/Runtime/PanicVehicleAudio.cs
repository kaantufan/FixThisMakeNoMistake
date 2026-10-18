using UnityEngine;

namespace TooLateToPanic
{
    // Player-distance gain avoids the high isometric camera becoming the distance listener.
    public sealed class PanicVehicleAudio : MonoBehaviour
    {
        PanicHomeWorld world;
        AudioSource engine,siren;
        AudioLowPassFilter filter;
        Vector3 previous;
        bool emergency;
        public void Bind(PanicHomeWorld owner,string sirenId=null)
        {
            world=owner;emergency=!string.IsNullOrEmpty(sirenId);previous=transform.position;
            engine=Source("Quiet vehicle engine",emergency?"engine_diesel":"engine_car");
            if(emergency)siren=Source("Quiet response siren",sirenId);
            filter=gameObject.AddComponent<AudioLowPassFilter>();filter.cutoffFrequency=16000;
        }
        AudioSource Source(string name,string id)
        {
            var a=gameObject.AddComponent<AudioSource>();a.playOnAwake=false;a.loop=true;a.spatialBlend=0;
            a.dopplerLevel=0;a.priority=170;a.volume=0;a.clip=PanicAudioBank.Get(id);return a;
        }
        void LateUpdate()
        {
            if(!world||world.Model==null)return;
            float speed=Time.deltaTime>.00001f?Vector3.Distance(previous,transform.position)/Time.deltaTime:0;
            previous=transform.position;
            bool paused=world.Model.IsPaused||world.NotebookBlocksInput||world.InputLocked||(!Application.isFocused&&!Application.runInBackground);
            Vector3 delta=transform.position-world.PlayerPosition;
            float distance=delta.magnitude;
            float gain=PanicHomeEffects.DistanceGain(distance,3,34)*Mathf.Lerp(1,.04f,world.AudioShelterAmount);
            float pan=world.View?Mathf.Clamp(Vector3.Dot(delta.normalized,world.View.transform.right)*.65f,-.65f,.65f):0;
            filter.cutoffFrequency=Mathf.Lerp(Mathf.Lerp(14000,2200,Mathf.Clamp01(distance/34)),500,world.AudioShelterAmount);
            Mix(engine,paused?0:gain*(speed>.15f?.065f:emergency?.025f:0),pan);
            Mix(siren,paused?0:gain*(speed>.15f?.055f:.018f),pan);
            if(engine)engine.pitch=Mathf.Lerp(.86f,1.08f,Mathf.Clamp01(speed/6));
        }
        void Mix(AudioSource source,float target,float pan)
        {
            if(!source)return;source.panStereo=pan;
            // Pauses are immediate; ordinary approach/departure fades smoothly.
            if(target<=0){source.volume=0;source.Pause();return;}
            source.volume=Mathf.MoveTowards(source.volume,target,Time.unscaledDeltaTime*.15f);
            if(source.clip&&!source.isPlaying){source.UnPause();if(!source.isPlaying)source.Play();}
        }
        void OnDisable(){if(engine){engine.volume=0;engine.Stop();}if(siren){siren.volume=0;siren.Stop();}}
    }
}
