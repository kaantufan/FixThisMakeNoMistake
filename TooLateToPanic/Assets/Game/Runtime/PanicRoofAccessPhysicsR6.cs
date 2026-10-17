using System;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        public PanicRoofAccessPhysicsR6 RoofAccessR6 { get; private set; }
        void InitRoofAccessPhysicsR6()
        {
            RoofAccessR6 = PanicRoofAccessPhysicsR6.Install(transform, roof, residenceGarage, controller);
        }
    }

    public sealed class PanicRoofBarrierPartR6 : MonoBehaviour { public string Role; }

    /// <summary>
    /// Static no-entry attic volumes. Keeps the existing ceiling undersides, closes lateral roof access.
    /// No Update, controller/clock/position correction, render ownership or gameplay mutation.
    /// </summary>
    public sealed class PanicRoofAccessPhysicsR6 : MonoBehaviour
    {
        public const int ProtectionLayer = 30; // Existing world interaction/picking masks exclude this layer.
        public const float MainCeilingUnderside = 6.38f;
        const float EdgeOverlap = .035f;
        public Bounds MainRoofArtBounds { get; private set; }
        public Bounds GarageRoofArtBounds { get; private set; }
        public Bounds MainBarrierBounds { get; private set; }
        public Bounds GarageBarrierBounds { get; private set; }
        public float GarageWalkSurfaceY { get; private set; }
        public BoxCollider MainBarrier { get; private set; }
        public BoxCollider GarageBarrier { get; private set; }
        public int ControllerLayer { get; private set; }
        public bool LayerCollisionEnabled { get { return !Physics.GetIgnoreLayerCollision(ControllerLayer, ProtectionLayer); } }
        public int EnabledBarrierCount { get { return (MainBarrier && MainBarrier.enabled && MainBarrier.gameObject.activeInHierarchy ? 1 : 0) +
            (GarageBarrier && GarageBarrier.enabled && GarageBarrier.gameObject.activeInHierarchy ? 1 : 0); } }

        public static PanicRoofAccessPhysicsR6 Install(Transform owner, GameObject actualRoof, GameObject actualGarage, CharacterController actor)
        {
            if (!owner || !actualRoof || !actualGarage || !actor)
                throw new InvalidOperationException("Roof R6 requires the actual residence roof, garage and controller.");
            Transform previous = owner.Find("Permanent roof access physics R6");
            if (previous) return previous.GetComponent<PanicRoofAccessPhysicsR6>();
            Transform garageRoof = actualGarage.transform.Find("Roof");
            if (!garageRoof || !TryMeshBounds(actualRoof.transform, out Bounds roofBounds) || !TryMeshBounds(garageRoof, out Bounds garageBounds))
                throw new InvalidOperationException("Roof R6 could not read the actual roof mesh bounds.");
            var root = new GameObject("Permanent roof access physics R6");
            root.transform.SetParent(owner, true);
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var state = root.AddComponent<PanicRoofAccessPhysicsR6>();
            state.ControllerLayer = actor.gameObject.layer;
            state.MainRoofArtBounds = roofBounds; state.GarageRoofArtBounds = garageBounds;
            state.GarageWalkSurfaceY = garageBounds.max.y;
            foreach (var filter in garageRoof.GetComponentsInChildren<MeshFilter>(true))
                if (filter.name == "solid_garage_roof_deck" && TryMeshBounds(filter.transform, out var deck))
                { state.GarageWalkSurfaceY = deck.max.y; break; }
            // The common top is above the actual main roof/chimney, so a legitimate floor jump
            // cannot land on a new flat cap above the garage. This space is deliberately non-playable.
            float top = Mathf.Max(roofBounds.max.y, 8.65f) + .10f;
            state.MainBarrierBounds = Volume(roofBounds, MainCeilingUnderside, top);
            state.GarageBarrierBounds = Volume(garageBounds, garageBounds.min.y, top);
            state.MainBarrier = state.Add("Main attic and eave no-entry", state.MainBarrierBounds);
            state.GarageBarrier = state.Add("Garage roof no-entry", state.GarageBarrierBounds);
            // The current project matrix enables0↔30. Fail visibly instead of claiming protection
            // if a future project changes it; never change the global collision matrix here.
            if (!state.LayerCollisionEnabled)
                throw new InvalidOperationException("Roof R6 collision layer is ignored by the actual controller layer.");
            return state;
        }

        static Bounds Volume(Bounds footprint, float bottom, float top)
        {
            return new Bounds(new Vector3(footprint.center.x, (bottom + top) * .5f, footprint.center.z),
                new Vector3(footprint.size.x + EdgeOverlap * 2, top - bottom, footprint.size.z + EdgeOverlap * 2));
        }

        BoxCollider Add(string role, Bounds bounds)
        {
            var go = new GameObject(role); go.layer = ProtectionLayer;
            go.transform.SetParent(transform, true); go.transform.SetPositionAndRotation(bounds.center, Quaternion.identity);
            var collider = go.AddComponent<BoxCollider>(); collider.size = bounds.size; collider.isTrigger = false;
            go.AddComponent<PanicRoofBarrierPartR6>().Role = role;
            return collider;
        }

        static bool TryMeshBounds(Transform root, out Bounds bounds)
        {
            bounds = new Bounds(); bool any = false;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter.sharedMesh) continue;
                Bounds local = filter.sharedMesh.bounds;
                for (int x=-1;x<=1;x+=2) for (int y=-1;y<=1;y+=2) for (int z=-1;z<=1;z+=2)
                {
                    Vector3 p = filter.transform.TransformPoint(local.center + Vector3.Scale(local.extents, new Vector3(x,y,z)));
                    if (!any) { bounds = new Bounds(p, Vector3.zero); any = true; } else bounds.Encapsulate(p);
                }
            }
            return any && bounds.size.x > .1f && bounds.size.z > .1f;
        }
    }
}
