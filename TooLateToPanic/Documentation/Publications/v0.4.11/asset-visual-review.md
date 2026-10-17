# Native V7 fireplace closure review

Scope: readonly review of root-produced completed native/API smoke artifact and actual1600x900fireplace PNG. This reviewer executed no Unity/editor/player, no OS input and no source/art changes. Only this new review note written.

## Decision

**V5-ART-OBS-01 orientation HOLD CLOSED for this exact R7 snapshot.** The new native image visibly presents the intended front arch and cavity instead of the rear dark panel. The completed native/API artifact separately passes correct-room-facing orientation and extinguish/ignite behavior. No further art revision requested from this evidence.

This closes the fireplace orientation finding, not whole-game/OS-input/human-quality acceptance. Earlier V5 observation and R6 failed extinguish record remain historical evidence and are not rewritten.

## Exact identity

- Run: `manager/native-v7-b46fe7d3-0687-4edb-b5af-fdbf6988a43b/`.
- Build recorded in `build-identity.json`: `C:\Users\kaant\Desktop\civilgamejam\TooLateToPanic\TooLateToPanic\Builds\Home-V4.11-r7-20261017\TooLateToPanic.exe`.
- DLL SHA256: `34E3969FBBE5047A6400D9FE253ACE1B2177BEDD07ECBBDA3020D3745008E047`. Reviewer hashed the actual on-disk `TooLateToPanic_Data/Managed/Assembly-CSharp.dll` and matched the root launch identity.
- Launch receipt: `SW_SHOWNOACTIVATE`; `OSInput=false`. No OS/human claim inferred.
- PNG `fireplace-closeup.png` SHA256: `8953E026A59BD7DECDDBD6AC32CD6B686C6514137D662F1626FBD8466F2B37A2`.
- `revision-smoke.json` SHA256: `B15D49F96F139E342C8616107EE9A56D14D146BC51B4913579048136FDE70C44`.
- `build-identity.json` SHA256: `2BAAE23E9D2BEECB24835EDF8442B8FBE29B6B8CCE5957F1A3FC96A62D70ED06`.
- `player.log` SHA256 at review: `4EF1BBC3F04CEA4272D49554DCC26391356829942083A7A5CA389F7470C13A7F`.

## Direct native PNG observations

The open arch is now visible around the brick-lined cavity. The wedge stones, side masonry, layered mantle, vertical grate bars, logs and red ember-effect shapes can be distinguished at the front. The rear lining is behind the opening and no longer presents as the misleading flat exterior-facing panel seen in V5.

The actor is to the right rather than covering the central firebox. A lamp and foreground furniture still occlude portions of the right/lower surround, but leave enough of the arch/cavity/embers visible to verify the facing correction. Warm illumination is visible around the hearth. Visible red ember-effect shapes lie low within the firebox behind the grate; no detached outside-wall flame is demonstrated in this frame.

The still does not show a continuous animated flame cycle or a full off/on sequence. Do not describe it as independently proving temporal flicker, exact particle contacts, thermal realism or listening/human acceptance. Extinguish/ignite behavior is supported by the separate recorded native/API rows below.

## Completed native/API artifact inspected

`complete=true`, `passed=true`, `error=""`, `errors=[]`; **156/156rows PASS** in approximately34.24seconds. Fixture declares10setup Warps and actual production APIs/frame clocks/rays/colliders; it expressly excludes OS input and human acceptance.

| Recorded native row | Result | Recorded scope |
|---|---|---|
| `fireplace.native-model` | PASS | Authored arched masonry plus actual linked flame origin; visual frame separately reviewed |
| `fireplace.opens-into-room` | PASS | Actual mesh front local-Z faces east into the room; flame is inside its authored opening |
| `fireplace.extinguish` | PASS | Visibility refresh must preserve the switched-off flame across real frames |
| `fireplace.ignite` | PASS | No additional row detail. |

`fireplace.extinguish` explicitly checks that visibility refresh preserves the switched-off flame across real frames. `fireplace.ignite` also passes. This supports root's runtime visibility-owner correction; this reviewer read and checked the completed artifact rather than rerunning the test.

Targeted `player.log` scan found0occurrences of both historical signatures: `Quaternion To Matrix conversion failed` and `Non-Legacy animations cannot be sampled`. The smoke runtime-error list is empty. These are bounded artifact checks for this run, not a general guarantee about all gameplay.

## Limits retained

- Readonly native-PNG review and inspection of root-owned native/API evidence only; no new source or player work.
- No extra asset geometry, clip, controller, character or notebook revision. Root reports R7 runtime visibility/fixture changes while art and NotebookR2 stay unchanged; source acceptance must remain tied to the R7 manifest/DLL independently.
- This note does not re-review other V7 frames or close prior hand five-digit readability/physical handle-wrap limitations. Whole campaign, OS keyboard/mouse, user enjoyment and human visual/audio acceptance remain outside this review.
- Packaging/promotion is root-owned; closing this specific visual hold is not by itself a release-promotion action.
