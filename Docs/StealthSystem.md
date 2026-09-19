# Guard, Chase, and Stealth System

## Status and scope

This is a standalone Room 1 mechanics prototype in
`Assets/Scenes/Dev/StealthTest.unity`. The developer reported successful manual
playtesting of patrol, vision, collection, chase, and exit during development.
This guide was checked against the current source and serialized assets; it is
not a new Play-mode test report.

Player references can now resolve the persistent player by tag at startup.
Production retry and exit integration are **not implemented**. Keep using the standalone test scene until the integration
work below is complete. Art, level layout, timings, and difficulty remain
prototype choices.

## Gameplay loop

```text
Avoid the patrolling guard
         |
Approach objective and press E
         |
Item disappears; exactly two pursuers spawn
         |
Run to the exit while avoiding capture
         |
Exit trigger -> Escaped! -> R to replay

Guard sees player / pursuer touches player
         |
Caught! -> R to retry
```

The exit does nothing before collection. Pickup no longer completes the room.
Both capture and completion stop player movement, interaction, guard behaviour,
and spawned pursuers. Retry reloads the saved room scene, restoring its initial
item, positions, and patrol state. The exit currently displays completion rather
than loading another level.

## Components and assets

All six scripts are in `Assets/Script/Stealth/`.

| Component | Responsibility |
| --- | --- |
| `GuardPatrol` | Follow a looping waypoint route and look around at each stop. |
| `GuardVision` | Check player visibility and draw a prototype sight outline. |
| `ObjectiveItem` | Register with the existing E-key interaction and request pickup. |
| `ChaseEnemy` | Move directly toward the player; contact causes capture. |
| `RoomExit` | Complete the room when the player overlaps after pickup. |
| `StealthRoomController` | Own pickup, spawning, capture, completion, and retry. |

Reusable prefabs are in `Assets/Prefab/Stealth/`:

- `Guard.prefab`
- `ObjectiveItem.prefab`
- `ChaseEnemy.prefab`

The room controller and exit are configured scene objects. The controller
automatically discovers all GuardPatrol and GuardVision components in its own
scene in Awake (including inactive objects) and connects detection listeners
while enabled. Use one controller per room scene. Guards in other additively
loaded scenes are not affected. Guards instantiated after discovery are not
automatically registered; this setup supports guards placed/duplicated before play.

Waypoint references inside the same prefab are reusable. Spawn points and room
references belong to the scene. Prefab assets cannot retain references to objects
in an external scene. Player references can be left empty for tag lookup.

## Standalone scene setup

Suggested organization (names are not hardcoded):

```text
StealthTest
├── Grid / floor and walls
├── MainPlayer
├── Main Camera / lighting
├── RoomController
├── GuardPatrolGroup (stationary prefab root; duplicate this whole group)
│   ├── Guard (GuardPatrol, GuardVision, LineRenderer)
│   │   ├── Body
│   │   └── FacingMarker
│   └── PatrolRoute
│       ├── PointA
│       └── PointB
├── ObjectiveItem
├── EnemySpawns
│   ├── SpawnA
│   └── SpawnB
└── RoomExit
```

### Player and walls

The player needs the `Player` tag, `PlayerMovement`, `PlayerInteract`, a solid
Collider2D, and a simulated dynamic Rigidbody2D with zero gravity and frozen Z
rotation. Enable interpolation for smoother rendering.

After the main-branch animation merge, `PlayerMovement` also requires an Animator
on the same GameObject. Assign `Assets/Animation/PlayerAnimation.controller`,
leave Avatar empty, and disable Apply Root Motion. The SpriteRenderer needs a
valid default sprite from `Assets/Sprites/GDG_character.png`.

Walls/cover need solid Collider2D components. A TilemapCollider2D is suitable for
the wall tilemap. Put sight-blocking objects on `VisionObstacle`; keep the floor,
player, and enemies off this layer. Collision layers must also permit player
and pursuer collisions with walls and each other where intended.

### Guard patrol

Place the guard prefab on clear floor. The existing explicit Waypoints array
continues to work and takes priority when non-empty. Alternatively, set its size
to zero and assign Route Root: the script reads that container's direct children
in Hierarchy order once at startup. The route loops back to its first point.

To create a reusable guard-plus-route prefab in Unity:

1. Create an empty GuardPatrolGroup with scale (1, 1, 1).
2. Parent the moving Guard and stationary PatrolRoute beneath it as siblings.
   Keep GuardPatrol/GuardVision on the Guard child, not the shared parent.
3. Set GuardPatrol's Waypoints size to zero and assign the sibling PatrolRoute
   to Route Root. Only put waypoint objects directly under PatrolRoute.
4. Save the whole group as a new prefab, for example GuardPatrolGroup, in
   Assets/Prefab/Stealth. This is a grouping workflow, not a new script.
5. Duplicate the entire group to add guards. Each copy retains references to
   its own internal route; move the group to relocate guard and route together,
   then position its individual points to tune that patrol.

Waypoints cannot be children of the moving Guard itself: otherwise they travel
and rotate with it. They can share the stationary group parent. Keep that parent
stationary during gameplay. Existing scene layouts and Guard.prefab do not need
to be replaced to use automatic multiple-guard discovery.

| Setting | Code default | Meaning |
| --- | ---: | --- |
| Move Speed | 2 | World units per second. |
| Turn Speed | 180 | Maximum rotation in degrees per second. |
| Look Duration | 3 | Seconds spent looking around after arrival; zero skips it. |
| Look Angle | 60 | Requested sweep to either side of the arrival facing. |

The sweep goes centre, left, right, centre, subject to Turn Speed. Local +Y
(the yellow facing marker) is forward. Movement uses Transform positions on the
XY plane and preserves the guard's Z position. It does not stop at walls or find
paths: every route segment must be clear.

To inspect the route, enable Scene-view Gizmos and select the **scene Guard
root**. Cyan lines/circles show waypoints; yellow shows facing. Two waypoints
produce one overlapping out-and-back line. Prefab Mode has no scene route.

### Guard vision

Assign `Player Target` to MainPlayer for standalone testing, or leave it empty
to find the active `Player`-tagged object once in Start. Set `Obstacle Layers` to VisionObstacle.
Defaults are View Distance **5** and View Angle **70 degrees** (the full cone).

Visibility requires all three:

1. Target Transform position is within range.
2. It is within half the cone angle on either side of forward.
3. A direct Physics2D ray to that point hits no solid obstacle in the mask.

Trigger colliders do not block sight. Detection tests one target point, not the
whole sprite. It runs in LateUpdate after patrol movement. A LineRenderer draws
an approximate wall-clipped outline using 48 angular segments: yellow normally,
red when visible. Assign its material to Sprites-Default. The script sets its
width to 0.04 and sorting order to 4 during play.

The room controller automatically connects each scene guard's **On Player Detected**
to CatchPlayer at runtime. No per-copy Inspector hookup is needed. Remove old
manual CatchPlayer event entries when convenient; leaving them is harmless because
CatchPlayer ignores repeat calls after the first accepted capture. Keep other
intentional event actions, such as sound effects.
The event fires on becoming visible, not every frame; visibility must be lost
before it can fire again. Once capture disables vision, the outline disappears.

### Objective and controller

Place ObjectiveItem on reachable floor. Its BoxCollider2D must be a trigger;
size it to the desired interaction reach. Assign its Room reference. Collider
size is multiplied by Transform scale, so check the resulting world-space area.

The item uses `IInteractable` and the existing `PlayerInteract` E key. Entering
range registers it and shows a prompt; leaving range or disabling the item clears
the registration. Multiple colliders belonging to the same player are tracked.
Keep its interaction area away from other interactables: the shared interaction
system holds only one current target and does not select the nearest one.

On StealthRoomController, assign:

| Field | Assign |
| --- | --- |
| Player Movement | Optional assigned player; when empty, finds PlayerMovement on the active Player-tagged object once in Awake. |
| Guard Patrol / Guard Vision | No Inspector fields needed: all guards in the controller's scene are found once in Awake. |
| Enemy Prefab | ChaseEnemy prefab asset from the Project window. |
| First / Second Spawn Point | Two separate scene Transforms on clear floor. |
| Keep Guard Active During Escape | Whether all patrol guards continue after pickup. |

Keep Guard Active During Escape defaults to **true**, also enabled in the saved
test scene when this guide was written. False disables every discovered patrol
and vision component after pickup; it does not turn patrol guards into pursuers.

Pickup validates the enemy prefab and both spawn references before consuming
the item. Missing/invalid setup logs an error and leaves it collectible.
Successful pickup sets HasObjective, creates two enemies, initializes their
player/room references, and places them in the controller's scene. Repeated
pickup requests cannot spawn another pair.

### Pursuers

ChaseEnemy needs an active/enabled component, simulated **dynamic** Rigidbody2D,
and enabled solid CircleCollider2D. Its setup defaults include zero gravity,
frozen rotation, interpolation, and continuous collision detection. Gravity and
rotation constraints are also set in Awake.

Move Speed defaults to **3.5**, compared with the current player speed of **5**.
It uses Rigidbody2D.MovePosition in FixedUpdate. Solid walls block movement,
but the enemy does **not** navigate around them; it can get stuck behind cover.
Spawn points should be distinct, clear of walls, and far enough from the pickup
to allow a fair escape. Keep the prefab asset active but remove any temporary
scene copy used to create it; the controller spawns the actual pair.

Collision enter/stay with the controller's player Rigidbody2D calls CatchPlayer.
This is contact capture, not sight-based detection. StopChasing disables physics
simulation and the component when the room ends.

### Exit and retry

RoomExit needs a trigger BoxCollider2D and its Room reference. Place it at a
reachable doorway/escape area, away from the objective. Enter/stay checks allow
completion only for the controller's player after HasObjective becomes true.
No E press is needed. Do not overlap pickup and exit unless immediate escape is
intended.

Enable the saved test scene in the Build Profiles scene list. R reloads the
controller's scene by path using LoadSceneMode.Single, so save edits **outside
Play mode** before testing. This is a full standalone-scene reset, not a checkpoint
or persistent-player reset. Missing build-list setup produces a visible error.

## Public interface

| API | Behaviour |
| --- | --- |
| `GuardPatrol.FacingDirection` | Read-only current local-up direction. |
| `GuardVision.IsPlayerVisible` | Read-only current visibility result. |
| `GuardVision.AddDetectionListener` / `RemoveDetectionListener` | Runtime detection subscription used by the room controller. |
| `StealthRoomController.IsPlaying` | Enabled/active room with neither terminal outcome. Includes escape phase. |
| `HasObjective`, `IsCaught`, `IsCompleted` | Read-only progress/outcome flags. |
| `TryCollectObjective()` | Returns success; starts the chase once if configured. |
| `CatchPlayer()` | Stops an active room as caught; repeat calls do nothing. |
| `CompleteRoom()` | Stops an active room as completed only after pickup. |
| `RetryRoom()` | Reloads after capture or completion. |
| `IsPlayer(Collider2D)` | Compares the collider's attached Rigidbody2D with the assigned player body. |
| `ChaseEnemy.Initialize(player, owner)` | Called by the spawner to supply runtime references. |
| `ChaseEnemy.StopChasing()` | Stops movement and disables physics/component. |

## Manual regression checklist

Run after changing scripts, physics settings, prefab overrides, or room layout.

1. Open StealthTest directly. Check Console for setup errors.
2. Move and stop in every direction: player remains visible and walls block it.
3. Observe a full patrol cycle and waypoint look sweeps.
4. Test vision ahead, behind, beyond range, and behind solid cover.
5. Get caught; movement/interaction stop; R restores the room. Repeat once.
6. Enter the exit before pickup: no completion.
7. Approach/leave the item: prompt appears/disappears; E outside range does nothing.
8. Collect: item disappears, exactly two pursuers spawn, player remains controllable.
9. Duplicate a guard group before play. Check independent patrol routes, detection
   by either guard, and that capture/completion stops all guards. Confirm all guard
   activity matches Keep Guard Active During Escape. If another room is loaded
   additively, its guards must remain outside this controller's management.
10. Let each pursuer catch the player in separate runs; verify retry removes both
    spawned objects and restores the objective.
11. Escape with the item: completion appears and enemies stop. R allows another run.
12. Temporarily omit a spawn reference: pickup logs an error without consuming
    the objective. Restore the reference before saving.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| Player vanishes when stationary | SpriteRenderer's default Sprite must resolve. Idle has no Motion and Write Defaults is enabled; a missing default can reappear only while walking clips supply sprites. |
| PlayerMovement reports a null Animator | Add Animator on the same object and assign PlayerAnimation.controller. |
| Patrol route not visible | Select scene Guard root, enable Gizmos, and check waypoint references. |
| Pink/missing cone | Assign a compatible LineRenderer material; check sorting and Player Target. |
| Guard sees through walls | Correct obstacle mask/layer, enabled solid 2D colliders, and no trigger-only cover. |
| Item cannot be collected | Room reference, Player tag, PlayerInteract, trigger overlap, valid chase setup, and Console errors. |
| Pursuers do not appear | Prefab asset/reference, active component, dynamic simulated body, solid circle collider, and both spawn references. |
| Pursuer gets stuck at cover | Expected direct-pursuit limitation; adjust route/layout or plan navigation separately. |
| R does not reload | Saved scene enabled in the Build Profiles list; room must be caught/completed. |

## Production integration still required

Main's `PersistentRoot` preserves the player/camera/UI hierarchy.
`RoomManager.GoToRoom(sceneName, entryPointId)` loads rooms additively and places
that existing player at an EntryPoint. The standalone components above are not
yet adapted to this flow.

Before transferring the mechanics into production Room1:

1. Coordinate changes to shared Room1/Persistent assets with their owners.
2. Leave Player Movement and Player Target empty to use the implemented startup
   lookup. Persistent must supply exactly one active Player-tagged object before
   the room loads, with PlayerMovement on that object. Explicit Inspector references
   take priority. Lookup runs only once, before validation; missing players produce
   a setup error and disable the affected component. ChaseEnemy still receives its
   player from the room controller. No per-frame search or late-spawn retry is used.
3. Include only room-owned content in Room1, avoiding duplicate player, camera,
   shared UI, and lighting objects supplied by Persistent.
4. Replace Single-scene retry with the room transition/reset flow and explicitly
   restore player movement, interaction, and Rigidbody2D.simulated. The persistent
   player survives scene reloads, including its disabled state after capture.
5. Gate the real exit transition on pickup, restore player control as needed,
   and call the shared room transition API. Merely adding TransitionTrigger would
   bypass the objective gate; the current RoomExit only displays completion.
6. Align entry IDs. At documentation time, fresh startup requests `default_spawn`
   while Room1's existing EntryPoint is `1`; these need a deliberate shared fix.
7. Test starting through MainMenu, save/continue, pause/resume, capture/retry,
   and leaving/re-entering Room1. Objective and chase progress are not saved by
   this system; decide intended reset/persistence behaviour before integration.

## Known prototype limits

- Multiple scene-placed patrol guards, one objective phase, and exactly two spawned
  pursuers. Runtime-spawned patrol guards require additional registration support.
- No guard pathfinding, pursuer obstacle routing, suspicion meter, or chase timeout.
- No explicit tie-break rule for exit/contact events in the same physics step;
  the first accepted terminal outcome wins. Pickup runs in Update before vision's
  LateUpdate; same-frame detection/pickup behaviour needs playtesting.
- Point-based detection and an approximate cone outline can differ at tight edges.
- OnGUI prompts/results are temporary and need final Canvas/art treatment later.
- Standalone testing is supported; persistent-player, production transitions, and
  save/continue integration are outstanding.
