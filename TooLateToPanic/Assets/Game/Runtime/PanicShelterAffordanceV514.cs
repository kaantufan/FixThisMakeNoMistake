using UnityEngine;
using B = TooLateToPanic.PanicHomeArt.Builder;
using M = TooLateToPanic.PanicHomeArt.M;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        GameObject panicRoomAffordanceV514;
        TextMesh panicRoomAffordanceKeyV514;
        Material panicRoomAffordanceLetteringV514;
        int panicRoomAffordanceStateV514 = -1;
        Light panicRoomAlarmV514;
        MeshRenderer[] panicRoomAlarmRenderersV514;
        MaterialPropertyBlock panicRoomAlarmBlockV514;
        Color panicRoomAlarmEmissionV514;
        float panicRoomAlarmClockV514;
        int panicRoomAlarmFrameV514 = -1;
        bool panicRoomAlarmWasActiveV514;

        // Read-only presentation probes. None asserts real focus/range or admission.
        public string PanicRoomAffordanceActionLabelV514 { get { return "Panik odasını kilitle"; } }
        public string PanicRoomAffordanceHintV514 { get { return PanicRoomAffordanceActionLabelV514; } }
        public bool PanicRoomAffordanceShowsKeyV514 { get { return panicRoomAffordanceV514; } }
        public bool PanicRoomAffordanceActionStageV514 { get { return panicRoomAffordanceStateV514 >= 2; } }
        public bool PanicRoomButtonFocusedV514 { get { return panicRoomAffordanceV514 && focusId == "@panicroom"; } }
        public Light PanicRoomAlarmLightV514 { get { return panicRoomAlarmV514; } }
        public float PanicRoomAlarmIntensityV514 { get { return panicRoomAlarmV514 ? panicRoomAlarmV514.intensity : 0; } }
        public bool PanicRoomAlarmOnV514 { get { return panicRoomAlarmV514 && panicRoomAlarmV514.enabled && panicRoomAlarmV514.intensity > .1f; } }
        public float PanicRoomAlarmClockV514 { get { return panicRoomAlarmClockV514; } }
        public Color PanicRoomAlarmEmissionV514 { get { return panicRoomAlarmEmissionV514; } }

        // Root mounts @panicroom at (7.84,3.2,-2.35), yaw90, on the real east wall.
        // This method creates only single-sided visual/button/alarm children. Root alone
        // owns the retained HomePick, unchanged collider dimensions and physical E gate.
        void BuildPanicRoomAffordanceV514(GameObject marker)
        {
            if (!marker || panicRoomAffordanceV514) return;
            PanicHomeArt.EnsurePalette();
            panicRoomAffordanceV514 = new GameObject("V514 panic room emergency button - visual only");
            panicRoomAffordanceV514.transform.SetParent(marker.transform, false);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var letteringTemplate = Resources.Load<Material>("HUD/PanicShelterTextV514");
            if (!letteringTemplate) throw new System.InvalidOperationException("Missing depth-tested shelter text material HUD/PanicShelterTextV514.");
            panicRoomAffordanceLetteringV514 = new Material(letteringTemplate);
            panicRoomAffordanceLetteringV514.name = "V514 depth-tested shelter lettering";
            panicRoomAffordanceLetteringV514.mainTexture = font.material.mainTexture;
            panicRoomAffordanceLetteringV514.color = Color.white;

            var face = new GameObject("Wall-mounted emergency button");
            face.transform.SetParent(panicRoomAffordanceV514.transform, false);
            var b = new B();
            // The housing back is slightly sunk into the real wall. Front normal is local
            // -Z, world -X. No standing post, second side or new collision is produced.
            b.Box(new Vector3(0, 1.35f, 0), new Vector3(.62f, .62f, .09f), M.SafetyAmber, .025f);
            b.Cylinder(new Vector3(0, 1.43f, -.060f), .232f, .03f, M.Charcoal, 32, Quaternion.Euler(90, 0, 0));
            b.Cylinder(new Vector3(0, 1.43f, -.095f), .205f, .065f, M.Red, 32, Quaternion.Euler(90, 0, 0));
            b.Torus(new Vector3(0, 1.43f, -.123f), .178f, .025f, M.Red, Quaternion.Euler(90, 0, 0));
            b.Cylinder(new Vector3(0, 1.43f, -.136f), .178f, .012f, M.Red, 32, Quaternion.Euler(90, 0, 0));
            b.Box(new Vector3(0, 1.105f, -.052f), new Vector3(.56f, .13f, .012f), M.Charcoal, .006f);
            b.Attach(face, "red_mushroom_emergency_button");
            MakeBatteryButtonMaterialsV514(face);
            panicRoomAffordanceKeyV514 = ButtonLabelV514(face.transform, font, "E", new Vector3(0, 1.43f, -.158f), .080f);
            ButtonLabelV514(face.transform, font, "Panik odasını\nkilitle", new Vector3(0, 1.105f, -.062f), .013f);

            var alarmBase = new GameObject("V514 wall battery alarm base");
            alarmBase.transform.SetParent(panicRoomAffordanceV514.transform, false);
            var pedestal = new B();
            pedestal.Box(new Vector3(0, 1.77f, .005f), new Vector3(.36f, .18f, .11f), M.Charcoal, .015f);
            pedestal.Cylinder(new Vector3(0, 1.79f, -.12f), .17f, .065f, M.Charcoal, 24);
            pedestal.Attach(alarmBase, "battery_alarm_mount");
            MakeBatteryButtonMaterialsV514(alarmBase);
            var cap = new GameObject("V514 blinking red alarm cap");
            cap.transform.SetParent(panicRoomAffordanceV514.transform, false);
            var red = new B();
            red.Cylinder(new Vector3(0, 1.925f, -.12f), .135f, .21f, M.Red, 32);
            red.Torus(new Vector3(0, 2.025f, -.12f), .11f, .025f, M.Red, Quaternion.identity);
            red.Cylinder(new Vector3(0, 2.035f, -.12f), .11f, .025f, M.Red, 32);
            red.Attach(cap, "red_alarm_lens");
            MakeBatteryButtonMaterialsV514(cap);
            panicRoomAlarmRenderersV514 = cap.GetComponentsInChildren<MeshRenderer>(true);
            panicRoomAlarmBlockV514 = new MaterialPropertyBlock();
            panicRoomAlarmV514 = new GameObject("V514 blinking red battery alarm light").AddComponent<Light>();
            panicRoomAlarmV514.transform.SetParent(panicRoomAffordanceV514.transform, false);
            panicRoomAlarmV514.transform.localPosition = new Vector3(0, 1.95f, -.15f);
            panicRoomAlarmV514.type = LightType.Point;
            panicRoomAlarmV514.color = new Color(1, .025f, .012f);
            panicRoomAlarmV514.range = 2f;
            panicRoomAlarmV514.shadows = LightShadows.None;
            panicRoomAlarmV514.intensity = 0;
            panicRoomAlarmV514.enabled = false;
            TickPanicRoomAffordanceV514();
        }

        void MakeBatteryButtonMaterialsV514(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
            {
                var material = new Material(renderer.sharedMaterial);
                material.name = "V514 battery button " + renderer.sharedMaterial.name;
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", material.color * .42f);
                }
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        TextMesh ButtonLabelV514(Transform parent, Font font, string text, Vector3 p, float size)
        {
            var go = new GameObject("V514 physical shelter lettering");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = p;
            var label = go.AddComponent<TextMesh>();
            label.font = font; label.fontSize = 48; label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            label.text = text; label.color = new Color(.94f, .95f, .86f);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = panicRoomAffordanceLetteringV514;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return label;
        }

        // Root calls this from ApplyMainsPowerV5/SyncVisuals. It observes production
        // state; the private alarm clock never changes model time, RNG or admission.
        void TickPanicRoomAffordanceV514()
        {
            if (!panicRoomAffordanceV514 || Model == null) return;
            bool floodPreparation = UsesUpperPanicRoomV5 && (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial);
            int state = floodPreparation ? (BagFinalStageActive ? 2 : 1) : 0;
            if (state != panicRoomAffordanceStateV514)
            {
                panicRoomAffordanceStateV514 = state;
                panicRoomAffordanceKeyV514.color = state >= 2 ? new Color(.94f, .95f, .86f) : new Color(.62f, .65f, .59f);
            }

            bool active = floodPreparation && ActiveFloor == 1 && panicRoomAffordanceV514.activeInHierarchy;
            bool blocked = ExperienceFeedbackBlockedV414 || Model.WaveLedgerOpenV56 || Model.KeyboardHelpOpenV56;
            if (active && !panicRoomAlarmWasActiveV514) panicRoomAlarmClockV514 = 0;
            panicRoomAlarmWasActiveV514 = active;
            if (active && !blocked && !ReducedFpsMotionV54 && panicRoomAlarmFrameV514 != Time.frameCount)
                panicRoomAlarmClockV514 += Mathf.Min(Time.deltaTime, .08f);
            panicRoomAlarmFrameV514 = Time.frameCount;
            bool bright = panicRoomAlarmClockV514 % .9f < .45f;
            float intensity = !active ? 0 : ReducedFpsMotionV54 ? .22f : bright ? .6f : .025f;
            panicRoomAlarmV514.enabled = active;
            panicRoomAlarmV514.intensity = intensity;
            panicRoomAlarmEmissionV514 = !active ? new Color(.035f, .002f, .001f) :
                ReducedFpsMotionV54 ? new Color(.35f, .015f, .008f) :
                bright ? new Color(1.1f, .03f, .015f) : new Color(.06f, .003f, .0015f);
            foreach (var renderer in panicRoomAlarmRenderersV514)
            {
                if (!renderer) continue;
                renderer.GetPropertyBlock(panicRoomAlarmBlockV514);
                panicRoomAlarmBlockV514.SetColor("_EmissionColor", panicRoomAlarmEmissionV514);
                renderer.SetPropertyBlock(panicRoomAlarmBlockV514);
                panicRoomAlarmBlockV514.Clear();
            }
        }
    }
}
