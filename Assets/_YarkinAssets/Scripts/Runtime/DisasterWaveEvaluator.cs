using System.Collections.Generic;
using UnityEngine;

namespace YarkinAssets.Runtime
{
    /// <summary>
    /// Hazırlık fazı bittiğinde veya Cutscene başladığında sahnedeki tüm DisasterPrepItem bileşenlerini tarar,
    /// toplam İMO hazırlık skorunu hesaplar, sabitlenmemiş eşyaları devirir ve bir sonraki dalgada (Wave)
    /// hangi ikincil afetlerin (Yangın, Sel, Elektrik Çarpması, Koridor Kapanması) tetikleneceğini raporlar.
    /// </summary>
    public class DisasterWaveEvaluator : MonoBehaviour
    {
        [Header("Test / Simülasyon Kontrolleri")]
        public int totalPreparednessScore;
        public List<SecondaryHazardType> triggeredNextWaveHazards = new List<SecondaryHazardType>();

        [ContextMenu("1. Simülasyon: Tüm Eşyaları Sabitle & Vanaları Kapat")]
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

        [ContextMenu("3. Simülasyon: Tüm Eşyaları Sıfırla (Intact)")]
        public void ResetAllItemsToDefault()
        {
            var items = FindObjectsByType<DisasterPrepItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                item.SetSecuredOrClosed(false);
                item.ApplyVisualState(AssetVisualState.DefaultIntact);
            }
            triggeredNextWaveHazards.Clear();
            totalPreparednessScore = 0;
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
    }
}
