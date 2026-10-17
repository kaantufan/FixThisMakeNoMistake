#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using YarkinAssets.Runtime;

namespace YarkinAssets.Editor
{
    [CustomEditor(typeof(DisasterPrepItem))]
    [CanEditMultipleObjects]
    public class DisasterPrepItemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var item = (DisasterPrepItem)target;

            if (item.isUtilityValve || item.mechanismKind != ValveMechanismKind.None || item.isSecurableFixture || item.isContainerCabinet || item.cabinetDoorHinge != null)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Fiziksel Mekanik Animasyon & Sabitleme Kontrolü", EditorStyles.boldLabel);

                string stateLabel = item.isSecuredOrClosed
                    ? "Durum: KAPALI / GÜVENLİ (İzole Edilmiş / Sabitlenmiş)"
                    : "Durum: AÇIK / AKTİF (Gaz/Su/Elektrik Akışı Var)";
                EditorGUILayout.LabelField(stateLabel);

                if (item.isUtilityValve || item.mechanismKind != ValveMechanismKind.None || item.isSecurableFixture)
                {
                    Rect r = EditorGUILayout.GetControlRect(false, 18f);
                    EditorGUI.ProgressBar(r, item.CurrentValveProgress, $"Vana / Şalter / Sabitleme İlerlemesi: %{Mathf.RoundToInt(item.CurrentValveProgress * 100f)}");

                    GUI.backgroundColor = item.isSecuredOrClosed ? new Color(0.35f, 0.85f, 0.55f) : new Color(1.0f, 0.55f, 0.25f);
                    string btnText = item.isUtilityValve
                        ? (item.isSecuredOrClosed ? "▶ Vanayı / Musluğu / Şalteri AÇ (Animasyonlu)" : "▶ Vanayı / Musluğu / Şalteri KAPAT (Animasyonlu)")
                        : (item.isSecuredOrClosed ? "▶ Sabitlemeyi & İzolasyonu KALDIR (Animasyonlu)" : "▶ Sabitle & Vana/Şalter İZOLE ET (Animasyonlu)");

                    if (GUILayout.Button(btnText, GUILayout.Height(30f)))
                    {
                        foreach (var t in targets)
                        {
                            if (t is DisasterPrepItem dpi)
                            {
                                Undo.RecordObject(dpi, "Toggle Valve or Fixture State");
                                dpi.ToggleSecuredOrClosed();
                                EditorUtility.SetDirty(dpi);
                            }
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (item.isContainerCabinet || item.cabinetDoorHinge != null)
                {
                    EditorGUILayout.Space(4);
                    Rect rDoor = EditorGUILayout.GetControlRect(false, 18f);
                    EditorGUI.ProgressBar(rDoor, item.CurrentDoorProgress, $"Dolap Kapağı Açıklık Oranı: %{Mathf.RoundToInt(item.CurrentDoorProgress * 100f)}");

                    GUI.backgroundColor = new Color(0.45f, 0.78f, 1.0f);
                    string doorBtn = item.isCabinetDoorOpen
                        ? "▶ Dolap Kapağını KAPAT (Menteşe Animasyonlu)"
                        : "▶ Dolap Kapağını AÇ (Menteşe Animasyonlu)";

                    if (GUILayout.Button(doorBtn, GUILayout.Height(28f)))
                    {
                        foreach (var t in targets)
                        {
                            if (t is DisasterPrepItem dpi)
                            {
                                Undo.RecordObject(dpi, "Toggle Cabinet Door");
                                dpi.ToggleCabinetDoor();
                                EditorUtility.SetDirty(dpi);
                            }
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(6);
            }

            DrawDefaultInspector();

            if (item.IsAnimating)
            {
                Repaint();
            }
        }
    }
}
#endif
