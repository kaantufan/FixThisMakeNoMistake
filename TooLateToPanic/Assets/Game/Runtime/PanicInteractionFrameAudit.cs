using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Root-owned background audit helper. All stock/flag/pointer fixtures are labelled.
    /// Work stages advance only through production StepWork/Tick; no progress/completion fields are written.</summary>
    public static class PanicInteractionFrameAudit
    {
        public static IEnumerator Run(PanicHomeWorld w, PanicHomeHud hud, Action<string, bool, string> check)
        { return new Audit(w, hud, check).Run(); }

        sealed class Audit
        {
            const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
            readonly PanicHomeWorld w;
            readonly PanicHomeHud hud;
            readonly Action<string, bool, string> check;
            readonly FieldInfo settle = typeof(PanicWorkSession).GetField("settleRemaining", Hidden);
            readonly FieldInfo cinema = typeof(PanicHomeWorld).GetField("<CinematicActive>k__BackingField", Hidden);
            CharacterController body;
            bool savedLock, savedScripted, terminalReached;
            int warps;
            int driveFrames;
            float terminalSettle;
            int terminalSteps, terminalTotal;

            public Audit(PanicHomeWorld world, PanicHomeHud homeHud, Action<string, bool, string> callback)
            { w = world; hud = homeHud; check = callback; }

            public IEnumerator Run()
            {
                if (!w || !hud || check == null) throw new ArgumentException("Frame audit requires world, HUD and callback.");
                body = UnityEngine.Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Survivor");
                C("FIXTURE production references", body && settle != null && cinema != null, "Missing reflection metadata/body is a harness failure.");
                if (!body || settle == null || cinema == null) yield break;
                savedLock = w.InputLocked; savedScripted = w.FullFlowScriptedInput;
                w.InputLocked = true; w.FullFlowScriptedInput = true;
                try
                {
                    yield return DiscreteFrames();
                    foreach (bool shortSession in new[] { false, true })
                    foreach (string action in new[] { "anchorwall", "drain", "fire" })
                    {
                        yield return CancelBoundary(action, shortSession, true);
                        yield return CancelBoundary(action, shortSession, false);
                    }
                    C("fixtures preserve controller and finish without active work", body.enabled && !w.WorkActive && !w.IsMoving && !w.InventoryOpen,
                        "Explicit setup Warps=" + warps + "; no OS input, forced Complete/progress, scores or rule weakening.");
                }
                finally
                {
                    if (hud.DeveloperMenuOpen) hud.SetDeveloperMenuOpen(false);
                    w.CancelWork(); w.CancelMove(); if (w.InventoryOpen) w.ToggleInventory();
                    w.InputLocked = savedLock; w.FullFlowScriptedInput = savedScripted;
                }
            }

            IEnumerator ResetSource()
            {
                w.StartGame(false); yield return Wait(.08f);
                if (!body.enabled) throw new InvalidOperationException("FIXTURE body disabled before pantry setup; refused Warp.");
                w.Warp(new Vector3(-4, -3.12f, 3)); warps++; Physics.SyncTransforms(); yield return Wait(.1f);
                w.SetFocus("basement_pantry");
                if (!w.CanInteract) throw new InvalidOperationException("FIXTURE pantry approach not reachable: " + w.PlayerPosition);
            }

            IEnumerator DiscreteFrames()
            {
                yield return ResetSource(); var source = w.NearestSource; string stock = Inventory();
                Dispatch(new[] { KeyCode.Tab, KeyCode.B }, false, false);
                C("Tab+B same frame opens one inventory modal", w.InventoryOpen && !source.Open && stock == Inventory(), "One dispatch, ordered keys Tab/B.");
                w.ToggleInventory(); yield return null;
                Dispatch(new[] { KeyCode.Tab, KeyCode.E }, false, false);
                C("Tab+E same frame cannot leak into container", w.InventoryOpen && !source.Open && stock == Inventory(), "E does not run after Tab opens inventory.");
                w.ToggleInventory(); yield return null;
                w.InputLocked = true; w.DispatchWorldInput(new[] { KeyCode.Tab, KeyCode.E }, true, false);
                C("dispatch InputLocked blocks keys and primary click", !w.InventoryOpen && !source.Open && stock == Inventory(), "Production dispatcher; no InputLocked bypass.");
                bool previousCinema = (bool)cinema.GetValue(w);
                try
                {
                    // A synchronous flag-only fixture isolates this dispatcher guard. No frame,
                    // model phase, controller or presentation sequence runs with this fixture.
                    cinema.SetValue(w, true); Dispatch(new[] { KeyCode.Tab, KeyCode.E }, true, false);
                    C("dispatch cinema guard blocks keys and click", !w.InventoryOpen && !source.Open && stock == Inventory(), "Synchronous CinematicActive flag fixture, restored before next frame.");
                }
                finally { cinema.SetValue(w, previousCinema); }
                w.InputLocked = false; hud.SetDeveloperMenuOpen(true); bool opened = hud.DeveloperMenuOpen;
                w.InputLocked = false; // Isolate developer guard from the modal's separate lock.
                w.DispatchWorldInput(new[] { KeyCode.Tab, KeyCode.E }, true, false);
                C("dispatch developer guard blocks independent of lock", opened && !w.InventoryOpen && !source.Open && stock == Inventory(), "Actual HUD modal; InputLocked=false only synchronously to isolate gate.");
                w.InputLocked = true; hud.SetDeveloperMenuOpen(false);
                w.InputLocked = false; w.DispatchWorldInput(new[] { KeyCode.E }, true, false);
                C("dispatch developer transition frame blocks action", hud.DeveloperInputBlockedThisFrame && !source.Open && stock == Inventory(), "Close and dispatch occur in same frame.");
                w.InputLocked = true; yield return null;
                Dispatch(new[] { KeyCode.E }, false, false); bool sourceOpen = source.Open; int available = source.Contents[0].Count;
                Dispatch(new[] { KeyCode.Q, KeyCode.F }, false, false);
                C("Q+F frame performs focus action without collecting", sourceOpen && source.Open && w.CanInteract && w.NearestSource == source &&
                    Inventory() == stock && source.Contents[0].Count == available, "Pantry is the single local source; Q retains/reselects it, so a leaked F would collect.");

                // Adversarial primary-click fixture sits first on the actual current pointer ray.
                // A click-only control proves it targets food slot 1. It is not OS pointer input.
                GameObject pointer = PointerLoot(1);
                try { Dispatch(new KeyCode[0], true, false); }
                finally { ReleasePointer(pointer); }
                C("FIXTURE primary click control hits selected loot", w.Expedition.Count("food") == 2 && source.Contents[1].Count == 0 && source.Contents[0].Count == available,
                    "Temporary ResidenceLootPick collider at actual Input.mousePosition ray; click-only control.");
                yield return ResetSource(); source = w.NearestSource; Dispatch(new[] { KeyCode.E }, false, false);
                pointer = PointerLoot(1);
                try { Dispatch(new[] { KeyCode.F }, true, false); }
                finally { ReleasePointer(pointer); }
                C("F+primaryClick frame commits only keyboard pickup", w.Expedition.Count("water") == 2 && w.Expedition.Count("food") == 0 && source.Contents[0].Count == 0 && source.Contents[1].Count == 2,
                    "A leaked click would collect food via the proven slot-1 ray fixture.");
            }

            IEnumerator CancelBoundary(string action, bool shortSession, bool legacy)
            {
                w.StartGame(false); yield return Wait(.05f);
                string target = action == "drain" ? "drain_west" : "up_cabinet";
                Vector3 approach = action == "drain" ? new Vector3(-5, .08f, -9.8f) : new Vector3(3.1f, 3.28f, -3.25f);
                if (!body.enabled) throw new InvalidOperationException("FIXTURE body disabled before work setup; refused Warp.");
                w.Warp(approach); warps++; Physics.SyncTransforms(); yield return Wait(.1f); w.SetFocus(target);
                var item = w.Model.Items.Find(i => i.Id == target);
                var recipe = PanicExpedition.MaterialCosts(action, item).ToArray();
                // Finite declared stock fixture. Budget remains the normal start budget;
                // developer free materials/budget options are never used.
                foreach (var slot in w.Expedition.Inventory) { slot.Id = ""; slot.Count = 0; }
                for (int i = 0; i < recipe.Length; i++) { w.Expedition.Inventory[i].Id = recipe[i].Id; w.Expedition.Inventory[i].Count = recipe[i].Required; }
                w.BeginWork(action, target);
                if (shortSession && w.WorkActive)
                    w.Expedition.Work.Begin(action, target, true); // Production two-step session; explicit, not a full short-flow fixture.
                int total = shortSession ? 2 : 4;
                string label = action + "/" + (shortSession ? "short2" : "normal4") + "/" + (legacy ? "old-order-observation" : "cancel-first");
                C("FIXTURE work session " + label, w.WorkActive && w.Expedition.Work.TotalSteps == total && !w.Expedition.Work.Complete && w.CanInteract &&
                    !w.Model.InfiniteBudget && !w.Model.FreeMaterials, "Finite recipe stock + explicit Warp; session began through production APIs; target=" + target);
                if (!w.WorkActive) yield break;
                string initialStock = Inventory(); int budget = w.Model.Budget;
                int expectedCost = PanicExpedition.WorkCost(action, item); string targetState = TargetState(action, item);
                yield return DriveToLastSettle();
                C("real Tick reaches precommit settle " + label, terminalReached && w.WorkActive && !w.Expedition.Work.Complete && terminalSteps == total - 1 &&
                    terminalTotal == total && terminalSettle > 0 && terminalSettle <= .08f && w.Model.Budget == budget && Inventory() == initialStock && TargetState(action, item) == targetState,
                    "steps=" + terminalSteps + "/" + terminalTotal + "; settle=" + terminalSettle.ToString("F5") + "; production input frames=" + driveFrames);
                if (!terminalReached) { w.CancelWork(); yield break; }
                if (legacy)
                {
                    // Exact old order is reproduced as an EXPECTED OBSERVATION. It is not the current input dispatcher.
                    w.StepWork(.08f, 0, false, false, false); w.CancelWork();
                    bool paid = w.Model.Budget == budget - expectedCost && recipe.All(c => w.Expedition.Count(c.Id) == 0) && TargetState(action, item) != targetState;
                    C("expected old-order commit observation " + label, paid && !w.WorkActive,
                        "StepWork(.08) then CancelWork commits before cancel; expected cost=" + expectedCost + "; actual=" + (budget - w.Model.Budget) + "; regression observation only.");
                }
                else
                {
                    w.ProcessWorkInput(.08f, 0, false, false, false, true);
                    bool canceled = !w.WorkActive && w.Model.Budget == budget && Inventory() == initialStock && TargetState(action, item) == targetState && w.CancelFrame == Time.frameCount;
                    C("cancel-first final frame keeps all costs " + label, canceled,
                        "ProcessWorkInput cancel=true; budget delta=" + (budget - w.Model.Budget) + "; CancelFrame=" + w.CancelFrame + "; frame=" + Time.frameCount);
                    Dispatch(new[] { KeyCode.Tab, KeyCode.E }, true, false);
                    C("cancel frame blocks following world dispatch " + label, canceled && !w.InventoryOpen && !w.WorkActive && Inventory() == initialStock && w.Model.Budget == budget,
                        "Same frame Tab/E/primaryClick cannot escape cancellation ownership.");
                }
                yield return null;
            }

            IEnumerator DriveToLastSettle()
            {
                terminalReached = false; driveFrames = 0; terminalSettle = 0; terminalSteps = terminalTotal = 0;
                float deadline = Time.realtimeSinceStartup + 22f;
                while (w.WorkActive && Time.realtimeSinceStartup < deadline && driveFrames < 3000)
                {
                    var work = w.Expedition.Work; float remaining = (float)settle.GetValue(work);
                    if (work.CompletedSteps == work.TotalSteps - 1 && remaining > 0 && remaining <= .08f)
                    { terminalReached = true; terminalSettle = remaining; terminalSteps = work.CompletedSteps; terminalTotal = work.TotalSteps; yield break; }
                    float dt = Mathf.Clamp(Time.unscaledDeltaTime, .0001f, .04f);
                    // Never overshoot the final settle boundary merely because a frame stalled.
                    if (work.CompletedSteps == work.TotalSteps - 1 && remaining > .08f) dt = Mathf.Min(dt, remaining - .06f);
                    float delta = work.Target - work.Cursor, direction = 0; bool held = false, enter = false, pulse = false;
                    if (!work.IsApplying)
                    {
                        bool aligned = Mathf.Abs(delta) <= work.TargetWidth * .4f;
                        direction = aligned ? 0 : Mathf.Sign(delta); enter = aligned;
                    }
                    else if (work.Mode == PanicWorkMode.Ratchet)
                    { direction = work.ExpectedDirection; pulse = work.ExpectedDirection == 0; held = pulse; }
                    else if (work.Mode == PanicWorkMode.Timing)
                    { pulse = Mathf.Abs(delta) <= work.TargetWidth * .45f; held = pulse; }
                    else
                    { direction = Mathf.Abs(delta) > .018f ? Mathf.Sign(delta) : 0; held = true; }
                    w.StepWork(dt, direction, held, enter, pulse); driveFrames++; yield return null;
                }
                if (w.WorkActive)
                {
                    terminalSettle = (float)settle.GetValue(w.Expedition.Work);
                    terminalSteps = w.Expedition.Work.CompletedSteps; terminalTotal = w.Expedition.Work.TotalSteps;
                }
            }

            GameObject PointerLoot(int index)
            {
                Ray ray = w.View.ScreenPointToRay(Input.mousePosition);
                var fixture = new GameObject("AUDIT temporary pointer loot fixture"); fixture.transform.position = ray.GetPoint(.5f);
                fixture.AddComponent<BoxCollider>().size = Vector3.one * .15f;
                fixture.AddComponent<ResidenceLootPick>().Index = index; Physics.SyncTransforms(); return fixture;
            }
            static void ReleasePointer(GameObject fixture)
            { if (!fixture) return; foreach (var c in fixture.GetComponentsInChildren<Collider>()) c.enabled = false; fixture.SetActive(false); UnityEngine.Object.Destroy(fixture); }
            void Dispatch(IEnumerable<KeyCode> keys, bool click, bool ui)
            { bool locked = w.InputLocked; w.InputLocked = false; try { w.DispatchWorldInput(keys, click, ui); } finally { w.InputLocked = locked; } }
            string TargetState(string action, PanicItem item)
            { return action == "drain" ? "drains=" + w.Model.DrainCount : action == "fire" ? "fire=" + w.Model.FireToolCount + ";item=" + item.FireProtected : "anchor=" + item.Anchored; }
            string Inventory() { return string.Join("|", w.Expedition.Inventory.Select(s => s.Id + ":" + s.Count).ToArray()); }
            IEnumerator Wait(float seconds)
            {
                float end = Time.realtimeSinceStartup + seconds;
                while (Time.realtimeSinceStartup < end)
                {
                    if (body.enabled && !w.CinematicActive && !w.LossAnnounced) w.SmokeMove(Vector2.zero, Mathf.Clamp(Time.unscaledDeltaTime, .0001f, .04f));
                    yield return null;
                }
            }
            void C(string name, bool passed, string detail) { check("FRAME " + name, passed, detail); }
        }
    }
}
