using System.Collections.Generic;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        sealed class Neighbor
        {
            public GameObject visual;
            public Animator animator;
            public Vector3 start, end;
            public Vector3 station;
            public Quaternion stationRot;
            public int role;
            public float offset;
        }
        readonly List<Neighbor> neighbors = new List<Neighbor>();
        readonly Dictionary<DisasterType, GameObject> neighborhoodDamage = new Dictionary<DisasterType, GameObject>();
        int exteriorOutcomeCount;
        public int ExteriorNpcCount { get { return neighbors.Count; } }
        public Vector3 ExteriorWalkerPosition { get { return neighbors.Count > 0 && neighbors[0].visual ? neighbors[0].visual.transform.position : Vector3.zero; } }
        public bool ExteriorNpcAnimationReady
        {
            get
            {
                if (neighbors.Count != 4) return false;
                foreach (var neighbor in neighbors)
                {
                    if (!neighbor.visual || !neighbor.animator || !neighbor.animator.runtimeAnimatorController || neighbor.animator.applyRootMotion) return false;
                    var rig = neighbor.visual.GetComponent<PanicCuratedCharacterRig>();
                    var skin = neighbor.visual.GetComponentInChildren<SkinnedMeshRenderer>(true);
                    if (rig)
                    {
                        if (rig.Bones == null || rig.Bones.Length != 19 || rig.BindPositions == null || rig.BindPositions.Length != 19 ||
                            !skin || !skin.sharedMesh || skin.sharedMesh.vertexCount == 0 || skin.sharedMesh.bindposes.Length != 19) return false;
                        var rendererBones = skin.bones;
                        if (rendererBones == null || rendererBones.Length != 19) return false;
                        for (int i = 0; i < 19; i++) if (!rig.Bones[i] || rendererBones[i] != rig.Bones[i]) return false;
                    }
                    else
                    {
                        if (!neighbor.animator.isHuman || !neighbor.animator.avatar || !neighbor.animator.avatar.isValid || !skin || !skin.sharedMesh || skin.sharedMesh.vertexCount == 0) return false;
                        bool idle = false, walk = false, jog = false, jump = false;
                        foreach (var clip in neighbor.animator.runtimeAnimatorController.animationClips)
                        {
                            if (!clip) continue; string name = clip.name;
                            idle |= name.IndexOf("Idle_Loop", System.StringComparison.OrdinalIgnoreCase) >= 0;
                            walk |= name.IndexOf("Walk_Loop", System.StringComparison.OrdinalIgnoreCase) >= 0;
                            jog |= name.IndexOf("Jog_Fwd_Loop", System.StringComparison.OrdinalIgnoreCase) >= 0;
                            jump |= name.IndexOf("Jump_Loop", System.StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        if (!idle || !walk || !jog || !jump) return false;
                    }
                }
                return true;
            }
        }
        public int ActiveExteriorNpcCount { get { int count = 0; foreach (var neighbor in neighbors) if (neighbor.visual && neighbor.visual.activeSelf) count++; return count; } }
        public int ExteriorDamageTypes { get { return neighborhoodDamage.Count; } }
        public bool HelicopterVisible { get { return helicopter && helicopter.activeInHierarchy; } }
        public bool FloorHasVisibleRenderers(int floor)
        {
            if (floor < -1 || floor > 1 || floor + 1 >= floorVisuals.Count) return false;
            foreach (var renderer in floorVisuals[floor + 1].GetComponentsInChildren<Renderer>(true)) if (renderer.enabled && renderer.gameObject.activeInHierarchy) return true;
            return false;
        }
        public bool FloorPhysicalCollidersIntact(int floor)
        {
            if (floor < -1 || floor > 1 || floor + 1 >= floorVisuals.Count) return false;
            var colliders = floorVisuals[floor + 1].GetComponentsInChildren<Collider>(true);
            if (colliders.Length == 0) return false;
            foreach (var collider in colliders) if (!collider.enabled || !collider.gameObject.activeInHierarchy) return false;
            return true;
        }

        void InitExteriorLife()
        {
            if (Model == null) Model = new PanicModel();
            if (neighbors.Count > 0) return;

            Vector3[] stations = {
                new Vector3(-8.0f, 0.085f, -13.5f),
                new Vector3(-2.5f, 0.085f, -11.5f),
                new Vector3(3.0f, 0.085f, -12.0f),
                new Vector3(8.0f, 0.085f, -13.5f)
            };
            Quaternion[] stationRotations = {
                Quaternion.Euler(0, 15, 0),
                Quaternion.Euler(0, -10, 0),
                Quaternion.Euler(0, 20, 0),
                Quaternion.Euler(0, -15, 0)
            };
            Vector3[] a = {
                new Vector3(-12, .085f, -14.9f),
                new Vector3(3, .085f, -14.9f),
                new Vector3(-9.5f, .085f, -23.25f),
                new Vector3(-8.2f, .085f, -23.25f)
            };
            Vector3[] b = {
                new Vector3(-4, .085f, -14.9f),
                new Vector3(8, .085f, -14.9f),
                a[2],
                a[3]
            };

            for (int i = 0; i < a.Length; i++)
            {
                var visual = PanicExteriorArt.CreatePedestrianVisual(i);
                if (!visual) continue;
                visual.name = "Neighborhood resident " + (i + 1);
                visual.transform.position = a[i];
                var animator = visual.GetComponentInChildren<Animator>();
                if (animator)
                {
                    animator.applyRootMotion = false;
                    animator.SetBool("Airborne", false);
                    animator.SetBool("Working", false);
                    animator.SetBool("Kneeling", false);
                }
                if (i >= 2) visual.transform.rotation = Quaternion.Euler(0, i == 2 ? 90 : 270, 0);
                neighbors.Add(new Neighbor {
                    visual = visual,
                    animator = animator,
                    start = a[i],
                    end = b[i],
                    station = stations[i],
                    stationRot = stationRotations[i],
                    role = i,
                    offset = i * 2.7f
                });
            }
        }

        void AddNeighborhoodAftermath(DisasterType type)
        {
            if (neighborhoodDamage.ContainsKey(type)) return;
            var damage = PanicExteriorArt.CreateAftermath(type);
            if (damage) neighborhoodDamage.Add(type, damage);
        }

        static void SafeDestroy(UnityEngine.Object obj)
        {
            if (!obj) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        void ResetExteriorLife()
        {
            foreach (var damage in neighborhoodDamage.Values) SafeDestroy(damage);
            neighborhoodDamage.Clear();
            exteriorOutcomeCount = 0;
        }

        public void TeardownExteriorLife()
        {
            foreach (var neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    if (neighbor.visual)
                    {
                        var ownedComponents = neighbor.visual.GetComponentsInChildren<PanicExteriorOwnedArt>(true);
                        foreach (var owned in ownedComponents)
                        {
                            if (owned != null)
                            {
                                foreach (var mat in owned.Materials) SafeDestroy(mat);
                                owned.Materials.Clear();
                            }
                        }
                    }
                    SafeDestroy(neighbor.visual);
                }
            }
            neighbors.Clear();
            ResetExteriorLife();
        }

        void TickExteriorLife(float dt)
        {
            if (Model == null) Model = new PanicModel();

            bool isAftermathOrInspection = Model.Phase == PanicPhase.Aftermath || Model.Phase == PanicPhase.Inspection || Model.Phase == PanicPhase.Success;
            bool isPatrolling = (Model.Phase == PanicPhase.Preparing || Model.Phase == PanicPhase.Tutorial || Model.Phase == PanicPhase.Title)
                                && !CinematicActive && !LossAnnounced;
            bool active = (isAftermathOrInspection || isPatrolling) && Model.Phase != PanicPhase.GameOver && Model.Phase != PanicPhase.Disaster;

            for (int i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];
                if (!neighbor.visual) continue;

                neighbor.visual.SetActive(active);
                if (!active) continue;

                if (isAftermathOrInspection)
                {
                    neighbor.visual.transform.position = neighbor.station;
                    neighbor.visual.transform.rotation = neighbor.stationRot;
                    if (neighbor.animator)
                    {
                        neighbor.animator.SetFloat("Speed", 0f, 0.15f, Mathf.Max(0.001f, dt));
                    }
                    float cycleTime = residenceTime + neighbor.offset;
                    int actionIndex = Mathf.FloorToInt(cycleTime / 4.0f + i * 1.33f) % 3;
                    ResponderAction targetAction;
                    switch (actionIndex)
                    {
                        case 0: targetAction = ResponderAction.Radio; break;
                        case 1: targetAction = ResponderAction.KneelInspect; break;
                        default: targetAction = ResponderAction.Directing; break;
                    }
                    PanicCharacterArt.SetResponderAction(neighbor.visual, neighbor.animator, targetAction, cycleTime);
                }
                else if (isPatrolling)
                {
                    Vector3 delta = neighbor.end - neighbor.start;
                    float length = delta.magnitude;
                    float speed = length > 0.1f ? 0.58f : 0f;
                    if (speed > 0f)
                    {
                        float walk = residenceTime * speed + neighbor.offset;
                        float position = Mathf.PingPong(walk, length);
                        float sign = Mathf.Repeat(walk, length * 2f) < length ? 1f : -1f;
                        neighbor.visual.transform.position = neighbor.start + delta.normalized * position;
                        Quaternion target = Quaternion.LookRotation(delta.normalized * sign, Vector3.up);
                        neighbor.visual.transform.rotation = Quaternion.Slerp(neighbor.visual.transform.rotation, target, 1f - Mathf.Exp(-dt * 5f));
                    }
                    if (neighbor.animator)
                    {
                        neighbor.animator.SetFloat("Speed", speed, 0.15f, Mathf.Max(0.001f, dt));
                    }
                    PanicCharacterArt.SetMotion(neighbor.visual, neighbor.animator, speed, true, 0f, dt);
                }
            }

            if (JuryActive && JuryDemo != null)
            {
                while (exteriorOutcomeCount < JuryDemo.Outcomes.Count) AddNeighborhoodAftermath(JuryDemo.Outcomes[exteriorOutcomeCount++].Disaster);
            }
            else if (Model.Phase == PanicPhase.Aftermath || Model.Phase == PanicPhase.Inspection && Model.TotalDamage > 0)
            {
                for (int i = 0; i < 4; i++) if (Model.HasActiveDisasterV54((DisasterType)i)) AddNeighborhoodAftermath((DisasterType)i);
            }
            bool flood = Model.HasActiveDisasterV54(DisasterType.Flood) && (Model.Phase == PanicPhase.Disaster || Model.Phase == PanicPhase.Aftermath || Model.Phase == PanicPhase.Inspection || CinematicActive || LossAnnounced) || neighborhoodDamage.ContainsKey(DisasterType.Flood);
            PanicExteriorArt.AnimateLandscape(landscape, residenceTime, flood);
        }
    }
}
