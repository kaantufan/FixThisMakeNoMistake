using System.Collections.Generic;
using UnityEngine;

namespace YarkinAssets.Runtime
{
    /// <summary>
    /// Hazırlık fazı bittiğinde veya Cutscene başladığında sahnedeki tüm DisasterPrepItem bileşenlerini tarar,
    /// toplam İMO hazırlık skorunu hesaplar, sabitlenmemiş eşyaları devirir ve bir sonraki dalgada (Wave)
    /// hangi ikincil afetlerin (Yangın, Sel, Elektrik Çarpması, Koridor Kapanması) tetikleneceğini raporlar.
    /// Ayrıca Demo sahnesinde vana, musluk, sigorta ve dolap kapaklarının fiziksel açma/kapama animasyonlarını
    /// canlı olarak test etmek için etkileşim ve kontrol paneli sunar.
    /// </summary>
    public class DisasterWaveEvaluator : MonoBehaviour
    {
        [Header("Test / Simülasyon Kontrolleri")]
        public int totalPreparednessScore;
        public List<SecondaryHazardType> triggeredNextWaveHazards = new List<SecondaryHazardType>();

        [Header("Canlı Mekanik Animasyon Demo Kontrolleri")]
        public bool showInteractiveDemoHud = true;
        public bool autoCycleValvesInDemo = false;
        private float _autoCycleTimer;
        private bool _valvesClosedState;
        private bool _doorsOpenState;

        private void Update()
        {
            if (!Application.isPlaying) return;

            // Mouse sol tık ile sahnedeki herhangi bir vana, musluk, sigorta veya dolaba tıklayarak aç/kapat
            if (Input.GetMouseButtonDown(0) && Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    var item = hit.collider.GetComponentInParent<DisasterPrepItem>();
                    if (item != null)
                    {
                        if (item.isContainerCabinet && !item.isUtilityValve)
                        {
                            item.ToggleCabinetDoor();
                        }
                        else if (item.isUtilityValve || item.isSecurableFixture)
                        {
                            item.ToggleSecuredOrClosed();
                        }
                    }
                }
            }

            // Klavye kısayolları:
            // [1]: Vana / Musluk / Sigortaları Aç-Kapat
            // [2]: Yangın Dolabı & Buzdolabı Kapaklarını Aç-Kapat
            // [3]: Eşya Sabitleme Aparatlarını (L-Braket) Aç-Kapat
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ToggleAllValvesAndBreakers();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ToggleAllCabinetDoors();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ToggleAllSecurableFixtures();
            }

            if (autoCycleValvesInDemo)
            {
                _autoCycleTimer += Time.deltaTime;
                if (_autoCycleTimer >= 2.2f)
                {
                    _autoCycleTimer = 0f;
                    ToggleAllValvesAndBreakers();
                    ToggleAllCabinetDoors();
                }
            }
        }

        [ContextMenu("1. Simülasyon: Tüm Eşyaları Sabitle & Vanaları Kapat (Animasyonlu)")]
        public void SecureAllFixturesAndValves()
        {
            var items = FindObjectsByType<DisasterPrepItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item.isSecurableFixture || item.isUtilityValve)
                {
                    item.SetSecuredOrClosed(true);
                }
            }
            EvaluateWaveOutcome(applyToppleVisuals: false);
        }

        [ContextMenu("2. Simülasyon: Deprem Cutscene Sonucu (Sabitlenmeyenleri Devir & Dalga Hesapla)")]
        public void SimulateDisasterCutsceneEvaluation()
        {
            EvaluateWaveOutcome(applyToppleVisuals: true);
        }

        [ContextMenu("3. Simülasyon: Tüm Eşyaları Sıfırla (Intact & Açık)")]
        public void ResetAllItemsToDefault()
        {
            var items = FindObjectsByType<DisasterPrepItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                item.SetSecuredOrClosed(false);
                if (item.isContainerCabinet)
                {
                    item.SetCabinetDoorOpen(false);
                }
                item.ApplyVisualState(AssetVisualState.DefaultIntact);
            }
            triggeredNextWaveHazards.Clear();
            totalPreparednessScore = 0;
        }

        [ContextMenu("4. Test: Sadece Vana, Musluk ve Sigortaları Aç/Kapat (Animasyonlu)")]
        public void ToggleAllValvesAndBreakers()
        {
            _valvesClosedState = !_valvesClosedState;
            var items = FindObjectsByType<DisasterPrepItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item.isUtilityValve || item.mechanismKind != ValveMechanismKind.None)
                {
                    item.SetSecuredOrClosed(_valvesClosedState);
                }
            }
        }

        [ContextMenu("5. Test: Dolap ve Buzdolabı Kapaklarını Aç/Kapat (Animasyonlu)")]
        public void ToggleAllCabinetDoors()
        {
            _doorsOpenState = !_doorsOpenState;
            var items = FindObjectsByType<DisasterPrepItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item.isContainerCabinet || item.cabinetDoorHinge != null)
                {
                    item.SetCabinetDoorOpen(_doorsOpenState);
                }
            }
        }

        public void ToggleAllSecurableFixtures()
        {
            var items = FindObjectsByType<DisasterPrepItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item.isSecurableFixture)
                {
                    item.ToggleSecuredOrClosed();
                }
            }
        }

        public void ToggleSpecificItemById(string targetItemId)
        {
            var items = FindObjectsByType<DisasterPrepItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item.itemId == targetItemId)
                {
                    item.ToggleSecuredOrClosed();
                }
            }
        }

        public int EvaluateWaveOutcome(bool applyToppleVisuals)
        {
            totalPreparednessScore = 0;
            triggeredNextWaveHazards.Clear();

            var items = FindObjectsByType<DisasterPrepItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item.isSecurableFixture || item.isUtilityValve)
                {
                    if (item.isSecuredOrClosed)
                    {
                        totalPreparednessScore += item.preparednessScore;
                        item.ApplyVisualState(AssetVisualState.SecuredOrIsolated);
                    }
                    else
                    {
                        if (applyToppleVisuals && item.isSecurableFixture)
                        {
                            item.ApplyVisualState(AssetVisualState.DamagedOrToppled);
                        }
                        if (item.DoesTriggerSecondaryWave(out SecondaryHazardType hazard) && !triggeredNextWaveHazards.Contains(hazard))
                        {
                            triggeredNextWaveHazards.Add(hazard);
                        }
                    }
                }
            }

            Debug.Log($"[DisasterWaveEvaluator] Toplam Hazırlık Puanı: {totalPreparednessScore} | Tetiklenen İkincil Afet Sayısı: {triggeredNextWaveHazards.Count} ({string.Join(", ", triggeredNextWaveHazards)})");
            return totalPreparednessScore;
        }

        [ContextMenu("Test: Sığınak (Bunker) Kaçış Cutscene Oynat")]
        public void TriggerBunkerCutscene()
        {
            var cutscenes = FindObjectsByType<BunkerCutsceneController>(FindObjectsSortMode.None);
            foreach (var ctrl in cutscenes)
            {
                ctrl.PlayCutscene();
            }
        }

        private void OnGUI()
        {
            if (!showInteractiveDemoHud || !Application.isPlaying) return;

            GUI.Box(new Rect(14, 14, 370, 284), "Mekanik Açma/Kapama & Bunker Cutscene Test Paneli");

            GUIStyle subStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            GUI.Label(new Rect(24, 38, 350, 32), "Sahnedeki vanalara, musluğa, sigortaya, dolaplara veya sığınak kapağına tıklayabilir ya da butonları kullanabilirsiniz:", subStyle);

            if (GUI.Button(new Rect(24, 74, 350, 26), "[1] Tüm Vana, Musluk & Sigortaları Aç / Kapat"))
            {
                ToggleAllValvesAndBreakers();
            }
            if (GUI.Button(new Rect(24, 104, 170, 26), "Doğalgaz Vanası (90°)"))
            {
                ToggleSpecificItemById("gas_valve");
            }
            if (GUI.Button(new Rect(204, 104, 170, 26), "Su Vanası (540°) & Musluk"))
            {
                ToggleSpecificItemById("water_valve");
            }
            if (GUI.Button(new Rect(24, 134, 170, 26), "Elektrik Sigorta Panosu"))
            {
                ToggleSpecificItemById("breaker_box");
            }
            if (GUI.Button(new Rect(204, 134, 170, 26), "Termosifon & Klima İzole"))
            {
                ToggleSpecificItemById("water_heater");
                ToggleSpecificItemById("air_conditioner");
            }
            if (GUI.Button(new Rect(24, 164, 350, 26), "[2] Dolap & Zemin Sığınak Kapaklarını Aç / Kapat"))
            {
                ToggleAllCabinetDoors();
            }
            if (GUI.Button(new Rect(24, 194, 350, 26), "[3] Deprem L-Braket Sabitlemelerini Aç / Kapat"))
            {
                ToggleAllSecurableFixtures();
            }
            if (GUI.Button(new Rect(24, 224, 350, 28), "🎬 [4] BUNKER CUTSCENE: Aç -> Çanta At -> Kuyuya Atla"))
            {
                TriggerBunkerCutscene();
            }

            autoCycleValvesInDemo = GUI.Toggle(new Rect(24, 258, 350, 22), autoCycleValvesInDemo, " Otomatik Döngü (Her 2.2 sn'de Aç/Kapat Testi)");
        }
    }
}
