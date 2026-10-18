using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>
    /// Opt-in native semantic-input audit. Setup Warps and the isolated phase fixture are
    /// explicitly labelled. No OS input, work completion, clock advancement, score writes,
    /// collider disabling, hinge mutation or gameplay-rule relaxation occurs here.
    /// Root owns process/profile isolation, exception capture, completion and reporting.
    /// </summary>
    public static class PanicInteractionWorldAudit
    {
        public static IEnumerator Run(PanicHomeWorld world, PanicHomeHud hud, Action<string, bool, string> check)
        {
            var run = new Audit(world, hud, check);
            return run.Run();
        }

        sealed class Hinge
        {
            public Transform leaf, frame;
            public Quaternion closed;
            public bool open, directional;
            public float Angle { get { return Mathf.DeltaAngle(closed.eulerAngles.y, leaf.localEulerAngles.y); } }
        }

        sealed class Audit
        {
            const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
            readonly PanicHomeWorld w;
            readonly PanicHomeHud hud;
            readonly Action<string, bool, string> check;
            CharacterController body;
            CapsuleCollider actorQuery;
            FieldInfo hingeField, itemsField;
            FieldInfo velocityField;
            string watchedDoor;
            float maximumDoorOverlap;
            int setupWarps;
            bool walkReached;
            bool oldLocked, oldScripted;

            public Audit(PanicHomeWorld world, PanicHomeHud homeHud, Action<string, bool, string> callback)
            { w = world; hud = homeHud; check = callback; }

            public IEnumerator Run()
            {
                if (!w || !hud || check == null) throw new ArgumentException("World audit needs production world, HUD and callback.");
                body = UnityEngine.Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Survivor");
                hingeField = typeof(PanicHomeWorld).GetField("hinges", Private);
                itemsField = typeof(PanicHomeWorld).GetField("items", Private);
                velocityField = typeof(PanicHomeWorld).GetField("desiredVelocity", Private);
                C("FIXTURE production world references", body && hingeField != null && itemsField != null && velocityField != null, "Missing reference is a harness failure, not a gameplay assertion.");
                if (!body || hingeField == null || itemsField == null || velocityField == null) yield break;
                var queryRoot = new GameObject("AUDIT independent enabled actor capsule query"); queryRoot.layer = 2;
                queryRoot.transform.position = Vector3.one * 500; queryRoot.transform.localScale = body.transform.lossyScale;
                actorQuery = queryRoot.AddComponent<CapsuleCollider>(); actorQuery.direction = 1; actorQuery.center = body.center;
                actorQuery.radius = body.radius; actorQuery.height = body.height; actorQuery.isTrigger = true; actorQuery.enabled = true;
                oldLocked = w.InputLocked; oldScripted = w.FullFlowScriptedInput;
                w.FullFlowScriptedInput = true; w.InputLocked = true;
                try
                {
                    string incomingOptions = DeveloperFlags();
                    foreach (PanicDeveloperOption option in Enum.GetValues(typeof(PanicDeveloperOption))) w.Model.SetDeveloperOption(option, false);
                    w.StartGame(false); yield return Settle(.25f);
                    C("FIXTURE clean economy after prior suites", !w.Model.DeveloperAnyEnabled && !w.Model.CheatedRun,
                        "Explicit SetDeveloperOption(false) then StartGame; incoming=" + incomingOptions + "; current=" + DeveloperFlags());
                    C("FIXTURE free-game preparation and enabled body", w.Model.Phase == PanicPhase.Preparing && body.enabled && !w.CinematicActive,
                        "StartGame(false); no game clock Tick shortcut; controller=" + body.enabled);
                    if (w.Model.Phase != PanicPhase.Preparing || !body.enabled || w.CinematicActive) yield break;
                    yield return LootAndFocus();
                    yield return Placement();
                    yield return DoorSides();
                    yield return ModalAndPhaseGates();
                    CameraMath();
                    yield return CameraWorldGates();
                    C("controller rules retained", body.enabled && Mathf.Abs(body.radius - .25f) < .001f && Mathf.Abs(body.height - 1.78f) < .001f &&
                        Mathf.Abs(body.stepOffset - .25f) < .001f && Mathf.Abs(body.skinWidth - .025f) < .001f && Mathf.Abs(body.slopeLimit - 48) < .001f,
                        "Explicit fixture Warps=" + setupWarps + "; no route/OS/whole-campaign claim.");
                }
                finally
                {
                    if (hud.DeveloperMenuOpen) hud.SetDeveloperMenuOpen(false);
                    w.CancelMove(); w.CancelWork();
                    if (w.InventoryOpen) w.ToggleInventory();
                    if (actorQuery) UnityEngine.Object.Destroy(actorQuery.gameObject);
                    w.InputLocked = oldLocked; w.FullFlowScriptedInput = oldScripted;
                }
            }

            IEnumerator LootAndFocus()
            {
                var pantry = w.Expedition.Sources.Single(s => s.Id == "basement_pantry");
                if (!Fixture(new Vector3(-4, -3.12f, 3), "pantry near")) yield break;
                yield return Settle(.2f); w.SetFocus(pantry.Id);
                bool reachable = w.CanInteract; string stock = Inventory(); int index = w.LootIndex;
                Key(KeyCode.F); Key(KeyCode.Z); Key(KeyCode.X);
                C("closed source rejects F and Z/X", reachable && !pantry.Open && !w.LootVisible && Inventory() == stock && w.LootIndex == index,
                    "E/F separation; pantry=" + pantry.Position + "; actor=" + w.PlayerPosition);
                Key(KeyCode.E); yield return Settle(.12f);
                C("E opens exact container without collecting", pantry.Open && w.OpenSource == pantry && Inventory() == stock && w.SelectedLootId == "water", "OpenSource and first physical content agree.");
                Key(KeyCode.X); string x = w.SelectedLootId; Key(KeyCode.Z); string z = w.SelectedLootId;
                Key(KeyCode.Z); string wrapBack = w.SelectedLootId; Key(KeyCode.X); string wrapForward = w.SelectedLootId;
                C("Z/X selects and wraps live source contents", x == "food" && z == "water" && wrapBack == "extinguisher" && wrapForward == "water",
                    "X=" + x + "; Z=" + z + "; wrap=" + wrapBack + "/" + wrapForward);
                int expectedWater = pantry.Contents[0].Count;
                Key(KeyCode.F);
                C("F commits selected complete stack once", expectedWater == 2 && w.Expedition.Count("water") == expectedWater && pantry.Contents[0].Count == 0 &&
                    pantry.Contents[1].Count == 2 && w.GatherItemId == "water" && w.GatherCount == expectedWater,
                    "Empty slot cannot clone; other stack untouched; new focus=" + w.SelectedLootId);
                Key(KeyCode.Z); C("Z skips exhausted source slot", w.SelectedLootId == "extinguisher", "water slot exhausted; selection=" + w.SelectedLootId);

                // Keep the first source open, then move focus to a different closed one.
                string beforeLeak = Inventory(); var other = w.Expedition.Sources.Single(s => s.Id == "up_cabinet");
                w.SetFocus(other.Id); Key(KeyCode.F); Key(KeyCode.X);
                C("source focus cannot leak pickup into another closed source", pantry.Open && !other.Open && w.OpenSource == null && Inventory() == beforeLeak,
                    "Old source remains open but is not current.");
                if (!Fixture(new Vector3(-4, -3.12f, -2), "pantry outside range")) yield break;
                yield return Settle(.1f); w.SetFocus(pantry.Id); bool wasOpen = pantry.Open; Key(KeyCode.E); Key(KeyCode.F);
                C("distant E/F leave open source and stock unchanged", !w.CanInteract && pantry.Open == wasOpen && Inventory() == beforeLeak,
                    "distance=" + Vector3.Distance(pantry.Position, w.PlayerPosition).ToString("F2"));
                if (!Fixture(new Vector3(-4, .08f, 3), "same XZ wrong floor")) yield break;
                yield return Settle(.12f); w.SetFocus(pantry.Id); Key(KeyCode.E); Key(KeyCode.F); Key(KeyCode.Q);
                C("wrong-floor E/F and Q cannot reach pantry", w.ActiveFloor == 0 && pantry.Open == wasOpen && Inventory() == beforeLeak &&
                    (w.NearestSource == null || w.NearestSource.Id != pantry.Id), "ActiveFloor=" + w.ActiveFloor + "; current=" + FocusId());
                if (!Fixture(new Vector3(-4, -3.12f, 3), "pantry exhaust")) yield break;
                yield return Settle(.12f); w.SetFocus(pantry.Id);
                Key(KeyCode.Q);
                var candidates = (List<string>)typeof(PanicHomeWorld).GetField("focusCandidates", Private).GetValue(w);
                var candidateIds = candidates.ToArray(); var visited = new List<string>();
                for (int i = 0; i < candidateIds.Length; i++) { visited.Add(FocusId()); Key(KeyCode.Q); }
                C("Q cycles nearby candidates once without duplicates", candidateIds.Length > 0 && candidateIds.Distinct().Count() == candidateIds.Length &&
                    visited.Distinct().Count() == candidateIds.Length && visited.All(id => candidateIds.Contains(id)) && (candidateIds.Length == 0 || FocusId() == visited[0]),
                    "candidates=" + string.Join(",", candidateIds) + "; visited=" + string.Join(",", visited.ToArray()));
                w.SetFocus(pantry.Id);
                for (int i = 0; i < pantry.Contents.Count + 1; i++) Key(KeyCode.F);
                bool empty = pantry.Contents.All(s => s.Count == 0 && string.IsNullOrEmpty(s.Id));
                string exhausted = Inventory(); Key(KeyCode.F); Key(KeyCode.F); Key(KeyCode.Z); Key(KeyCode.X);
                C("exhausted source and repeated F cannot clone stock", empty && Inventory() == exhausted && w.SelectedLootId == "" && w.Expedition.Count("water") == 2,
                    "Contents=" + SourceStock(pantry) + "; inventory=" + Inventory());
                Key(KeyCode.E); bool closed = !pantry.Open; Key(KeyCode.E);
                C("exhausted container remains operable without respawn", closed && pantry.Open && pantry.Contents.All(s => s.Count == 0) && Inventory() == exhausted,
                    "Actual E close/reopen only; OnNewWave not invoked.");
                Key(KeyCode.E);
            }

            IEnumerator Placement()
            {
                w.StartGame(false); yield return Settle(.15f);
                var item = w.Model.Items.Single(i => i.Id == "up_cabinet");
                var source = w.Expedition.Sources.Single(s => s.Id == item.Id);
                if (!Fixture(new Vector3(3.1f, 3.28f, -3.25f), "upper furniture")) yield break;
                yield return Settle(.4f); w.SetFocus(item.Id);
                var original = ItemVisual(item.Id); Vector3 start = item.Position; float yaw = item.Rotation; int budget = w.Model.Budget;
                var colliders = original.GetComponentsInChildren<Collider>(true); bool[] colliderState = colliders.Select(c => c.enabled).ToArray();
                var renderers = original.GetComponentsInChildren<Renderer>(true); bool[] rendererState = renderers.Select(r => r.enabled).ToArray();
                Vector3 screen = w.View.WorldToScreenPoint(start + Vector3.up * item.Size.y * .5f);
                bool began = Unlocked(() => w.BeginMousePlacement(screen, false));
                bool immediate = Unlocked(() => w.ConfirmMove());
                C("screen-ray begin cannot confirm on same frame", began && w.IsMoving && !immediate && w.Selected != null && w.Selected.Id == item.Id,
                    "screen=" + screen + "; nearest source=" + (w.NearestSource == null ? "none" : w.NearestSource.Id));
                if (!began) yield break;
                yield return null;
                bool sourceOpen = source.Open; string inventory = Inventory(); Key(KeyCode.E); Key(KeyCode.F); Key(KeyCode.Tab);
                w.StepCameraOrbitInput(w.View.pixelRect.center, true, true, false, false, 0);
                C("carried placement owns E/F/Tab and camera gesture", w.IsMoving && !w.InventoryOpen && source.Open == sourceOpen && Inventory() == inventory && !w.CameraOrbitPressed,
                    "Production carry state; no world collect/open/second modal or camera gesture.");
                string reason;
                bool invalid = !w.ValidatePlacement(item, new Vector3(float.NaN, start.y, 0), yaw, out reason) &&
                    !w.ValidatePlacement(item, start + Vector3.up * 3.2f, yaw, out reason) &&
                    !w.ValidatePlacement(item, new Vector3(30, start.y, 0), yaw, out reason) &&
                    !w.ValidatePlacement(item, new Vector3(0, start.y, 2.5f), yaw, out reason) &&
                    !w.ValidatePlacement(item, new Vector3(5, start.y, 1.8f), yaw, out reason) &&
                    !w.ValidatePlacement(item, new Vector3(w.PlayerPosition.x, start.y, w.PlayerPosition.z), yaw, out reason);
                bool rootHeld = colliders.Length > 0 && colliders.All(c => c && c.transform.IsChildOf(original.transform)) &&
                    colliders.Select((c, i) => c.enabled == colliderState[i]).All(b => b) && original.activeInHierarchy && original.transform.position == start;
                C("invalid floor/void/wall/actor and nonfinite placement reject; original collider root retained", invalid && rootHeld && item.Position == start && source.Position == start && w.Model.Budget == budget,
                    "colliders=" + colliders.Length + "; retained original active and enabled states; no commit.");
                Vector3 valid = new Vector3(4.05f, start.y, -4.5f); bool pointer = Pointer(valid, false); w.RotatePlacement(45);
                bool rotation = Mathf.Abs(Mathf.DeltaAngle(yaw + 45, w.PlacementRotation)) < .01f; float rotationBeforeInvalid = w.PlacementRotation;
                w.RotatePlacement(float.PositiveInfinity); w.RotatePlacement(float.NaN);
                bool nonfiniteRotation = w.PlacementRotation == rotationBeforeInvalid;
                Vector3 beforeUi = w.PlacementCandidate; bool overUi = !Pointer(valid, true) && !Unlocked(() => w.ConfirmMove()) && w.PlacementCandidate == beforeUi;
                bool outside = !w.UpdatePlacementPointer(new Vector2(-1, -1), false) && !Unlocked(() => w.ConfirmMove());
                C("rotation, HUD and viewport retain atomic placement", pointer && rotation && nonfiniteRotation && overUi && outside && item.Position == start && w.Model.Budget == budget,
                    "yaw=" + w.PlacementRotation + "; candidate=" + w.PlacementCandidate + "; reason=" + w.PlacementMessage);
                w.CancelMove(); yield return null;
                C("cancel restores original visual, source and colliders for free", !w.IsMoving && item.Position == start && item.Rotation == yaw && source.Position == start && original.transform.position == start &&
                    renderers.Select((r, i) => r.enabled == rendererState[i]).All(b => b) && colliders.Select((c, i) => c.enabled == colliderState[i]).All(b => b) && w.Model.Budget == budget,
                    "CancelFrame=" + w.CancelFrame + "; original root=" + original.name);
                w.SetFocus(item.Id); Unlocked(() => { w.BeginMove(); return w.IsMoving; }); yield return null;
                pointer = Pointer(valid, false); w.RotatePlacement(45); bool committed = Unlocked(() => w.ConfirmMove()); bool repeated = Unlocked(() => w.ConfirmMove());
                C("valid placement commits once and synchronizes storage source", pointer && committed && !repeated && !w.IsMoving && Vector3.Distance(item.Position, valid) < .035f &&
                    source.Position == item.Position && source.Floor == 1 && original.transform.position == item.Position && Mathf.Abs(Mathf.DeltaAngle(yaw + 45, item.Rotation)) < .01f && w.Model.Budget == budget,
                    "item=" + item.Position + "; source=" + source.Position + "; yaw=" + item.Rotation + "; budget=" + w.Model.Budget);
            }

            IEnumerator DoorSides()
            {
                w.StartGame(false); yield return Settle(.2f);
                var doors = w.Model.Items.Where(i => i.Kind == ItemKind.Door).ToArray();
                C("FIXTURE four production swinging doors", doors.Length == 4 && doors.All(d => ReadHinge(d.Id).directional), string.Join(",", doors.Select(d => d.Id).ToArray()));
                foreach (var door in doors)
                {
                    Hinge h = ReadHinge(door.Id); Vector3 origin = h.frame.position;
                    Vector3 normal = h.frame.TransformDirection(h.closed * Vector3.forward).normalized;
                    Vector3 latch = h.frame.TransformDirection(h.closed * Vector3.right).normalized;
                    for (int side = 1; side >= -1; side -= 2)
                    {
                        // The west side of the inner ground door borders the guarded stair shaft.
                        // A normal 1.55m retreat there is an invalid fixture. Use its real landing.
                        Vector3 clear = door.Id == "door_inner_ground" && side < 0 ? origin + normal * side * .6f + latch * 1.05f : origin + normal * side * 1.55f;
                        if (!Fixture(clear + Vector3.up * .08f, door.Id + " close setup")) yield break;
                        yield return Settle(.12f); w.SetFocus(door.Id);
                        if (ReadHinge(door.Id).open) Key(KeyCode.E);
                        yield return Settle(.7f);
                        bool closed = !ReadHinge(door.Id).open && Quaternion.Angle(ReadHinge(door.Id).leaf.localRotation, h.closed) < 1.5f;
                        if (!Fixture(origin + normal * side * .6f + Vector3.up * .08f, door.Id + " side " + side)) yield break;
                        yield return Settle(.12f); w.SetFocus(door.Id); Vector3 approach = w.PlayerPosition;
                        bool reachable = w.CanInteract; yield return Push(-normal * side, .23f);
                        // Stop the commanded SmoothDamp movement while the leaf is still shut.
                        // The old .08s pause left ~.049m of voluntary forward drift after E.
                        yield return Settle(.35f);
                        float before = Vector3.Dot(approach - origin, normal), after = Vector3.Dot(w.PlayerPosition - origin, normal);
                        bool blocked = before * after > 0 && Mathf.Abs(after) > .13f && Mathf.Abs(before) - Mathf.Abs(after) > .025f;
                        var others = HingeStates(door.Id); w.SetFocus(door.Id); Vector3 preOpen = w.PlayerPosition;
                        Vector3 residual = (Vector3)velocityField.GetValue(w);
                        maximumDoorOverlap = DoorOverlap(ReadHinge(door.Id)); watchedDoor = door.Id;
                        Key(KeyCode.E); yield return Settle(.72f); h = ReadHinge(door.Id); watchedDoor = null;
                        Vector3 drift = w.PlayerPosition - preOpen;
                        bool opened = h.open && Mathf.Abs(Mathf.DeltaAngle(side * 100, h.Angle)) < 2.5f;
                        C("door " + door.Id + "/" + side + " closed collision and away opening", closed && reachable && blocked && opened &&
                            residual.magnitude < .002f && drift.magnitude < .04f && maximumDoorOverlap < .006f && Supported(door.Position.y) && HingeStates(door.Id) == others,
                            "closed=" + closed + "; reachable=" + reachable + "; blocked=" + blocked + "; opened=" + opened + "; supported=" + Supported(door.Position.y) +
                            "; signed=" + before.ToString("F4") + "/" + after.ToString("F4") + "; angle=" + h.Angle.ToString("F2") +
                            "; residualVelocity=" + residual.ToString("F6") + "; actorDriftVector=" + drift.ToString("F6") + "; driftAlongNormal=" + Vector3.Dot(drift, normal).ToString("F6") +
                            "; maxIndependentCapsuleOverlap=" + maximumDoorOverlap.ToString("F6") + "; otherHingesUnchanged=" + (HingeStates(door.Id) == others));
                        Vector3 destination = origin - normal * side * .6f;
                        yield return WalkTo(destination, 2.3f);
                        C("door " + door.Id + "/" + side + " real open-plane crossing", walkReached && Supported(door.Position.y) && ReadHinge(door.Id).open &&
                            Mathf.Abs(Mathf.DeltaAngle(side * 100, ReadHinge(door.Id).Angle)) < 2.5f,
                            "No Warp during crossing; actor=" + w.PlayerPosition + "; target=" + destination);
                        // The following is another explicit fixture, never counted as an egress route.
                        if (!Fixture(clear + Vector3.up * .08f, door.Id + " clear close setup")) yield break;
                        yield return Settle(.12f); w.SetFocus(door.Id); Vector3 preClose = w.PlayerPosition; Key(KeyCode.E); yield return Settle(.72f);
                        C("door " + door.Id + "/" + side + " E closes with clear actor", !ReadHinge(door.Id).open && Quaternion.Angle(ReadHinge(door.Id).leaf.localRotation, h.closed) < 2.5f &&
                            Vector3.Distance(preClose, w.PlayerPosition) < .04f && Supported(door.Position.y), "Close setup Warp declared; no forced leaf pose.");
                    }
                }
            }

            IEnumerator ModalAndPhaseGates()
            {
                yield return WorkModal();
                // WorkModal uses another source/floor; restore the independently attributed source fixture.
                w.StartGame(false); yield return Settle(.12f); var s = w.Expedition.Sources.Single(x => x.Id == "basement_pantry");
                if (!Fixture(new Vector3(-4, -3.12f, 3), "developer gate source")) yield break;
                yield return Settle(.12f); w.SetFocus(s.Id);
                // A developer-menu boundary uses actual HUD open/close ownership and transition frame.
                w.InputLocked = false; hud.SetDeveloperMenuOpen(true); bool menuOpened = hud.DeveloperMenuOpen;
                bool openBefore = s.Open; string stock = Inventory(); w.HandleKey(KeyCode.E); w.HandleKey(KeyCode.F); w.HandleKey(KeyCode.Tab);
                C("developer modal blocks E/F/Tab and restores lock ownership", menuOpened && s.Open == openBefore && Inventory() == stock && !w.InventoryOpen && w.InputLocked,
                    "Only menu open; no developer option or budget mutation.");
                hud.SetDeveloperMenuOpen(false); w.HandleKey(KeyCode.E);
                C("developer close transition frame blocks E", !hud.DeveloperMenuOpen && s.Open == openBefore && hud.DeveloperInputBlockedThisFrame, "Same-frame leak guard.");
                w.InputLocked = true; yield return null;
            }

            IEnumerator WorkModal()
            {
                w.StartGame(false); yield return Settle(.12f);
                var tools = w.Expedition.Sources.Single(s => s.Id == "garage_tools");
                if (!Fixture(tools.Position + new Vector3(0, .08f, -1.3f), "real work stock")) yield break;
                yield return Settle(.12f); w.SetFocus(tools.Id); bool canGather = w.CanInteract;
                Key(KeyCode.E); Key(KeyCode.F); Key(KeyCode.F); Key(KeyCode.E);
                var target = w.Model.Items.Single(i => i.Id == "bookshelf");
                if (!Fixture(target.Position + new Vector3(0, .08f, -1.3f), "real work target")) yield break;
                yield return Settle(.12f); w.SetFocus(target.Id); string stock = Inventory(); int budget = w.Model.Budget;
                if (w.AnchorMode != "wall") Key(KeyCode.T);
                bool canTarget = w.CanInteract;
                Key(KeyCode.Alpha1);
                C("FIXTURE work begins through actual collected materials", canGather && w.Expedition.Count("screws") == 4 && w.Expedition.Count("bracket") == 3 && w.WorkActive,
                    "canGather=" + canGather + "; screws=" + w.Expedition.Count("screws") + "; bracket=" + w.Expedition.Count("bracket") + "; target=" + target.Id +
                    "; targetPosition=" + target.Position + "; actor=" + w.PlayerPosition + "; CanInteract=" + canTarget + "; AnchorMode=" + w.AnchorMode +
                    "; WorkActive=" + w.WorkActive + "; phase=" + w.Model.Phase + "; options=" + DeveloperFlags() + "; message=" + w.Model.Message);
                if (!w.WorkActive) yield break;
                bool sourceOpen = w.NearestSource.Open; Key(KeyCode.E); Key(KeyCode.F); Key(KeyCode.Tab); Unlocked(() => { w.BeginMove(); return w.IsMoving; });
                w.StepCameraOrbitInput(w.View.pixelRect.center, true, true, false, false, 0);
                C("active work owns world E/F/Tab/carry and camera input", w.WorkActive && !w.IsMoving && !w.InventoryOpen && !w.CameraOrbitPressed &&
                    w.NearestSource.Open == sourceOpen && stock == Inventory() && w.Model.Budget == budget && !target.Anchored,
                    "Work remained uncompleted; no costs or target state committed.");
                w.CancelWork(); yield return null;
                C("production work cancellation remains atomic", !w.WorkActive && stock == Inventory() && w.Model.Budget == budget && !target.Anchored,
                    "Real CancelWork; no forced Complete flag.");
            }

            void CameraMath()
            {
                var g = new PanicCameraOrbitGesture(); Vector2 origin = new Vector2(80, 90);
                g.Step(origin, true, true, false, false, .03f);
                var click = g.Step(origin + Vector2.right * 7.999f, false, false, true, false, .179f);
                C("camera gesture below drag/hold thresholds is one click", click.Click && click.ClickPosition == origin && click.OrbitDelta == Vector2.zero && !g.Pressed,
                    "7.999 pixels; .179 seconds; press frame elapsed excluded.");
                g.Step(origin, true, true, false, false, .03f);
                var drag = g.Step(origin + Vector2.right * 8, false, true, false, false, 0);
                var release = g.Step(origin + Vector2.right * 10, false, false, true, false, 0);
                C("camera exact drag threshold preserves all pixels and never clicks", drag.OrbitDelta == Vector2.right * 8 && release.OrbitDelta == Vector2.right * 2 && !release.Click && !g.Pressed,
                    "8px boundary; initial delta integrated exactly once.");
                g.Step(origin, true, true, false, false, 0); var hold = g.Step(origin, false, false, true, false, .18f);
                C("camera exact hold threshold is orbit release", !hold.Click && !g.Pressed, ".18s release; no carry begin.");
                g.Step(origin, true, true, false, false, 0); g.Step(origin, false, true, false, true, .1f); var blockedRelease = g.Step(origin, false, false, true, false, .1f);
                g.Step(origin, true, true, false, false, 0); var invalid = g.Step(new Vector2(float.NaN, 1), false, false, true, false, .01f);
                C("camera blocked/nonfinite/abandoned gestures cancel without click", !blockedRelease.Click && !invalid.Click && !g.Pressed,
                    "Blocked mid-gesture then release; NaN pointer.");
                var angles = new PanicCameraOrbitAngles(); angles.AddPixels(new Vector2(100000, 100000));
                bool low = angles.TargetPitch == PanicCameraOrbitAngles.MinPitch && angles.TargetYaw >= -180 && angles.TargetYaw < 180;
                angles.AddPixels(new Vector2(0, -200000)); bool high = angles.TargetPitch == PanicCameraOrbitAngles.MaxPitch;
                float yaw = angles.TargetYaw, pitch = angles.TargetPitch; angles.AddPixels(new Vector2(float.NaN, float.PositiveInfinity)); angles.Tick(float.NaN);
                C("camera angle clamps, wraps and rejects nonfinite deltas", low && high && angles.TargetYaw == yaw && angles.TargetPitch == pitch, "Pitch 28..65; yaw [-180,180).");
                var offsets = new List<Vector3>(); var targets = new List<float>();
                foreach (int fps in new[] { 30, 60, 120 })
                {
                    var a = new PanicCameraOrbitAngles(); a.AddPixels(new Vector2(120, -40));
                    for (int i = 0; i < fps; i++) a.Tick(1f / fps);
                    offsets.Add(a.RotateOffset(new Vector3(11, 13, -15))); targets.Add(a.TargetYaw);
                }
                C("camera smoothing equal elapsed time across 30/60/120 fps", Vector3.Distance(offsets[0], offsets[1]) < .001f && Vector3.Distance(offsets[0], offsets[2]) < .001f &&
                    targets.All(x => Mathf.Abs(x - 21.6f) < .001f), "Same initial pixel delta and one second elapsed; offsets=" + string.Join("/", offsets.Select(v => v.ToString("F3")).ToArray()));
            }

            IEnumerator CameraWorldGates()
            {
                w.StartGame(false); yield return Settle(.15f);
                Vector2 p = w.View.pixelRect.center;
                bool locked = w.InputLocked; w.InputLocked = false;
                w.StepCameraOrbitInput(p, true, true, false, false, 0);
                bool armed = w.CameraOrbitPressed; w.StepCameraOrbitInput(p, false, true, false, true, .02f);
                var release = w.StepCameraOrbitInput(p, false, false, true, false, .02f);
                C("camera production HUD boundary cancels pending release", armed && !release.Click && !w.CameraOrbitPressed && !w.IsMoving, "Semantic pointer/UI flag; real HUD dispatch covered by root.");
                w.StepCameraOrbitInput(p, true, true, false, false, 0); w.InputLocked = true;
                release = w.StepCameraOrbitInput(p, false, false, true, false, .02f);
                C("camera production InputLocked cancels gesture", !release.Click && !w.CameraOrbitPressed, "Public path checks actual world gate.");
                w.InputLocked = false;
                var outside = w.StepCameraOrbitInput(new Vector2(w.View.pixelRect.xMax, p.y), true, true, false, false, 0);
                C("camera viewport maximum edge cannot arm", !w.CameraOrbitPressed && !outside.Click, "Rect maximum boundary is outside.");
                w.ToggleInventory(); w.StepCameraOrbitInput(p, true, true, false, false, 0);
                C("camera inventory modal rejects gesture", w.InventoryOpen && !w.CameraOrbitPressed, "Actual ToggleInventory.");
                w.ToggleInventory();
                foreach (PanicPhase phase in new[] { PanicPhase.Title, PanicPhase.Disaster, PanicPhase.Aftermath, PanicPhase.Success, PanicPhase.GameOver })
                {
                    PanicPhase saved = w.Model.Phase; w.Model.Phase = phase; w.StepCameraOrbitInput(p, true, true, false, false, 0);
                    C("camera rejects phase fixture " + phase, !w.CameraOrbitPressed, "Synchronous enum-only fixture; restored before frame."); w.Model.Phase = saved;
                }
                w.InputLocked = locked;
            }

            bool Fixture(Vector3 p, string label)
            {
                if (!body.enabled) { C("FIXTURE setup body enabled " + label, false, "Cinema/observer disabled controller; refused Warp or SmokeMove."); return false; }
                w.Warp(p); setupWarps++; Physics.SyncTransforms(); return true;
            }
            IEnumerator Settle(float seconds)
            {
                float end = Time.realtimeSinceStartup + seconds;
                while (Time.realtimeSinceStartup < end)
                {
                    // Never re-enable or move a body temporarily owned by presentation.
                    if (body.enabled && !w.CinematicActive && !w.LossAnnounced) w.SmokeMove(Vector2.zero, Dt);
                    if (watchedDoor != null && body.enabled) maximumDoorOverlap = Mathf.Max(maximumDoorOverlap, DoorOverlap(ReadHinge(watchedDoor)));
                    yield return null;
                }
            }
            IEnumerator Push(Vector3 direction, float seconds)
            {
                float end = Time.realtimeSinceStartup + seconds;
                while (Time.realtimeSinceStartup < end) { if (!body.enabled) yield break; Move(direction); yield return null; }
                yield return Settle(.08f);
            }
            IEnumerator WalkTo(Vector3 target, float seconds)
            {
                walkReached = false; float end = Time.realtimeSinceStartup + seconds;
                while (Time.realtimeSinceStartup < end && body.enabled)
                {
                    Vector3 delta = target - w.PlayerPosition; delta.y = 0;
                    if (delta.magnitude < .045f) { walkReached = true; break; }
                    Move(Vector3.ClampMagnitude(delta / .35f, 1)); yield return null;
                }
                yield return Settle(.12f); Vector3 difference = target - w.PlayerPosition; difference.y = 0;
                walkReached &= difference.magnitude < .14f;
            }
            void Move(Vector3 direction)
            {
                Vector3 forward = w.View.transform.forward; forward.y = 0; forward.Normalize();
                Vector3 right = w.View.transform.right; right.y = 0; right.Normalize();
                w.SmokeMove(new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)), Dt);
            }
            static float Dt { get { return Mathf.Clamp(Time.unscaledDeltaTime, .0001f, .04f); } }
            bool Supported(float y) { return body.enabled && w.IsGrounded && Mathf.Abs(w.PlayerPosition.y - y) < .16f; }
            void Key(KeyCode key) { Unlocked(() => { w.HandleKey(key); return true; }); }
            bool Unlocked(Func<bool> action) { bool saved = w.InputLocked; w.InputLocked = false; try { return action(); } finally { w.InputLocked = saved; } }
            bool Pointer(Vector3 p, bool ui) { Vector3 screen = w.View.WorldToScreenPoint(p); return screen.z > 0 && w.UpdatePlacementPointer(screen, ui); }
            GameObject ItemVisual(string id) { return (GameObject)((IDictionary)itemsField.GetValue(w))[id]; }
            string Inventory() { return string.Join("|", w.Expedition.Inventory.Select(s => s.Id + ":" + s.Count).ToArray()); }
            string DeveloperFlags() { return "budget=" + w.Model.InfiniteBudget + ",materials=" + w.Model.FreeMaterials + ",clock=" + w.Model.FreezePreparationTime + ",noLoss=" + w.Model.DeveloperNoLoss; }
            float DoorOverlap(Hinge h)
            {
                if (!body.enabled || !actorQuery) return 0;
                Physics.SyncTransforms(); float maximum = 0;
                foreach (var collider in h.leaf.GetComponentsInChildren<Collider>())
                {
                    if (!collider.enabled || collider.isTrigger || !collider.gameObject.activeInHierarchy) continue;
                    Vector3 direction; float distance;
                    if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, actorQuery, body.transform.position, body.transform.rotation, out direction, out distance))
                        maximum = Mathf.Max(maximum, distance);
                }
                return maximum;
            }
            static string SourceStock(LootSource s) { return string.Join("|", s.Contents.Select(c => c.Id + ":" + c.Count).ToArray()); }
            string FocusId() { return (string)typeof(PanicHomeWorld).GetField("focusId", Private).GetValue(w); }
            Hinge ReadHinge(string id)
            {
                object value = ((IDictionary)hingeField.GetValue(w))[id];
                if (value == null) throw new InvalidOperationException("FIXTURE missing production hinge: " + id);
                Type t = value.GetType();
                return new Hinge { leaf = (Transform)t.GetField("leaf").GetValue(value), frame = (Transform)t.GetField("frame").GetValue(value),
                    closed = (Quaternion)t.GetField("closed").GetValue(value), open = (bool)t.GetField("open").GetValue(value),
                    directional = (bool)t.GetField("directionalDoor").GetValue(value) };
            }
            string HingeStates(string excluded)
            {
                // Logical ownership comparison; target movement must not toggle another hinge.
                var map = (IDictionary)hingeField.GetValue(w); var values = new List<string>();
                foreach (DictionaryEntry entry in map) if ((string)entry.Key != excluded) values.Add(entry.Key + ":" + ReadHinge((string)entry.Key).open);
                values.Sort(StringComparer.Ordinal); return string.Join("|", values.ToArray());
            }
            void C(string name, bool passed, string detail) { check("WORLD " + name, passed, detail); }
        }
    }
}
