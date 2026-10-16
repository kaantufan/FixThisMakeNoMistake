using UnityEngine;

namespace YarkinAssets.Runtime
{
    /// <summary>
    /// İzometrik görünümde eşyalara canlılık ("Game Feel") ve Cutscene sarsıntı tepkisi veren hafif motion bileşeni.
    /// Input sistemine dokunmaz; dışarıdan metodlarla tetiklenebilir.
    /// </summary>
    [DisallowMultipleComponent]
    public class IsometricItemMotion : MonoBehaviour
    {
        [Header("Hedef Görsel")]
        public Transform targetVisual;

        [Header("Yakınlık Süzülmesi (Proximity Bobbing)")]
        public bool enableIdleBobbing = false;
        public float bobAmplitude = 0.022f;
        public float bobFrequency = 3.2f;

        [Header("Deprem / Cutscene Sarsıntı Tepkisi")]
        public bool isShaking = false;
        public float shakeIntensity = 0.03f;

        private Vector3 _baseLocalPos;
        private Quaternion _baseLocalRot;
        private Vector3 _baseLocalScale;
        private float _phaseOffset;
        private bool _isPlayingPickupArc;
        private float _pickupTimer;
        private Vector3 _pickupStartPos;
        private Vector3 _pickupTargetPos;

        private void Awake()
        {
            if (targetVisual == null)
            {
                var found = transform.Find("Visual");
                targetVisual = found != null ? found : transform;
            }
            _baseLocalPos = targetVisual.localPosition;
            _baseLocalRot = targetVisual.localRotation;
            _baseLocalScale = targetVisual.localScale;
            _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (targetVisual == null) return;

            if (_isPlayingPickupArc)
            {
                UpdatePickupArc(Time.deltaTime);
                return;
            }

            Vector3 posOffset = Vector3.zero;
            Quaternion rotOffset = Quaternion.identity;

            if (enableIdleBobbing)
            {
                float y = Mathf.Sin(Time.time * bobFrequency + _phaseOffset) * bobAmplitude;
                posOffset.y += Mathf.Max(0f, y);
            }

            if (isShaking)
            {
                float t = Time.time * 28f + _phaseOffset;
                var item = GetComponent<DisasterPrepItem>();
                float mult = (item != null && item.isSecuredOrClosed) ? 0.25f : 1.0f;
                posOffset.x += Mathf.Sin(t * 1.3f) * shakeIntensity * mult;
                posOffset.z += Mathf.Cos(t * 1.7f) * shakeIntensity * mult;
                rotOffset = Quaternion.Euler(
                    Mathf.Sin(t * 1.9f) * 6f * mult,
                    0f,
                    Mathf.Cos(t * 2.1f) * 6f * mult
                );
            }

            targetVisual.localPosition = _baseLocalPos + posOffset;
            targetVisual.localRotation = _baseLocalRot * rotOffset;
        }

        /// <summary>
        /// Oyuncu eşyayı topladığında 0.28 saniyede parabolik yay ve Squash-Stretch ile hedefe (çantaya) uçurur.
        /// </summary>
        public void PlayPickupArcTo(Vector3 worldDestination, bool disableOnComplete = true)
        {
            _pickupStartPos = transform.position;
            _pickupTargetPos = worldDestination;
            _pickupTimer = 0f;
            _isPlayingPickupArc = true;
        }

        private void UpdatePickupArc(float dt)
        {
            const float duration = 0.28f;
            _pickupTimer += dt;
            float u = Mathf.Clamp01(_pickupTimer / duration);

            Vector3 flat = Vector3.Lerp(_pickupStartPos, _pickupTargetPos, u);
            flat.y += Mathf.Sin(u * Mathf.PI) * 0.65f;
            transform.position = flat;

            float scaleFactor = u < 0.35f
                ? Mathf.Lerp(1.0f, 1.32f, u / 0.35f)
                : Mathf.Lerp(1.32f, 0.05f, (u - 0.35f) / 0.65f);
            targetVisual.localScale = _baseLocalScale * scaleFactor;

            if (u >= 1.0f)
            {
                _isPlayingPickupArc = false;
                gameObject.SetActive(false);
            }
        }
    }
}
