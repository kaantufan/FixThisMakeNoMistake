using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Passive visual clip sampler. World owns state, clocks, pause and every interaction.</summary>
    public sealed class PanicCuratedMechanism : MonoBehaviour
    {
        public GameObject SampleRoot;
        public AnimationClip OpenClip, CloseClip;
        public bool Container;
        public string AssetName;
        [SerializeField] float amount;
        [SerializeField] bool initialized;
        [SerializeField] Transform status, drop, turbine;
        [SerializeField] Vector3 statusScale, dropScale;
        float turbineClock;

        // No Update, physics, audio, input, scoring, model reference or animation events.
        public void SampleState(bool open, bool safe, float dt)
        {
            if (!SampleRoot || !OpenClip || !CloseClip) return;
            // Container amount is door openness. Utility amount is isolation/closedness.
            bool target = Container ? open : safe;
            if (!initialized)
            { initialized=true; amount=target?1f:0f;
              foreach(var t in SampleRoot.GetComponentsInChildren<Transform>(true))
              {if(t.name=="StatusLED_Pivot"){status=t;statusScale=t.localScale;}
               if(t.name=="WaterDrop_Pivot"){drop=t;dropScale=t.localScale;}
               if(t.name=="MeterTurbine_Pivot")turbine=t;} }
            float duration = Mathf.Max(.01f, target ? (Container?OpenClip:CloseClip).length : (Container?CloseClip:OpenClip).length);
            amount=Mathf.MoveTowards(amount,target?1f:0f,Mathf.Max(0,dt)/duration);
            var clip = target ? (Container?OpenClip:CloseClip) : (Container?CloseClip:OpenClip);
            float phase=target?amount:1f-amount;
            clip.SampleAnimation(SampleRoot,Mathf.Clamp01(phase)*clip.length);
            // Upstream clips omit these status channels. Use the same passive state amount
            // to settle the LED and stop the small water flow without inventing gameplay.
            if(!Container)
            {
                float running=1-amount;
                if(status)status.localScale=statusScale*Mathf.Max(.001f,running);
                if(drop)drop.localScale=dropScale*Mathf.Max(.001f,running);
                if(turbine){turbineClock+=Mathf.Max(0,dt)*running*260f;turbine.localRotation=Quaternion.Euler(0,0,turbineClock);}
            }
        }
    }
}
