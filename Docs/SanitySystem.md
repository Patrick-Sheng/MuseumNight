# Sanity System

## Status

The sanity system is a working, manually tested prototype on the
`feat/sanity-system` branch.

Implemented:

- configurable sanity value and percentage-based levels;
- optional continuous sanity drain;
- reusable 2D triggers for gaining or losing sanity;
- numeric and level-change events;
- a prototype HUD with a fill bar, threshold markers, colours, and level text;
- Inspector-configurable actions for each sanity level;
- a URP vignette that strengthens as sanity decreases;
- a disabled uniform-darkness fallback.

The system is demonstrated in
`Assets/Scenes/Dev/SanityTest.unity`. It is not yet integrated into a
production gameplay scene or shared player prefab.

## Architecture

```text
Triggers / story systems
          |
          v
     SanitySystem
          |
          +-- SanityChanged ------> SanityBar
          |                     \-> SanityVignetteEffect
          |
          +-- SanityLevelChanged -> SanityLevelActions
```

`SanitySystem` owns the value and level calculation. UI, vision, and level
actions listen for changes instead of being controlled directly by the core
component.

## Default Configuration

| Setting           | Default |
| ----------------- | ------: |
| Maximum sanity    |     100 |
| Starting sanity   |     100 |
| Continuous drain  | Enabled |
| Drain per second  |       1 |
| Stable minimum    |     75% |
| Uneasy minimum    |     50% |
| Disturbed minimum |     25% |

The resulting default levels are:

| Sanity percentage | Level     |
| ----------------- | --------- |
| 75-100%           | Stable    |
| 50-below 75%      | Uneasy    |
| 25-below 50%      | Disturbed |
| 0-below 25%       | Critical  |

These values are prototype balancing values, not finalized game-design
requirements.

## Public API

Prefer the named methods when the direction of a change is known:

```csharp
sanitySystem.LoseSanity(10f);
sanitySystem.RestoreSanity(10f);
```

Use a signed delta when data or an Inspector field already represents both
directions:

```csharp
sanitySystem.ChangeSanity(-10f); // Lose sanity.
sanitySystem.ChangeSanity(10f);  // Restore sanity.
```

Control continuous drain without exposing the private field:

```csharp
sanitySystem.SetDrainEnabled(false);
sanitySystem.SetDrainEnabled(true);
```

Read current state through read-only properties:

```csharp
float current = sanitySystem.CurrentSanity;
float maximum = sanitySystem.MaxSanity;
float normalized = sanitySystem.NormalizedSanity;
SanityLevel level = sanitySystem.CurrentLevel;
bool isDraining = sanitySystem.DrainEnabled;
```

All value changes are clamped between zero and maximum sanity. Reaching zero
does not currently cause death, a game-over state, or a story event.

## Events

### `SanityChanged`

```csharp
public event Action<float, float> SanityChanged;
```

The arguments are current sanity and maximum sanity. The event runs whenever
the numeric value actually changes. It is intended for smooth responses such
as bars and vision effects.

### `SanityLevelChanged`

```csharp
public event Action<SanityLevel> SanityLevelChanged;
```

The argument is the new level. The event runs only when a threshold is
crossed. It is intended for discrete reactions such as audio changes,
flickering, object activation, or narrative events.

The core component does not broadcast an initial value event from `Awake`.
Current listeners initialize themselves in `Start`, after `SanitySystem` has
initialized its state.

When subscribing from code, unsubscribe when the listener is disabled:

```csharp
private void OnEnable()
{
    sanitySystem.SanityLevelChanged += HandleLevelChanged;
}

private void OnDisable()
{
    sanitySystem.SanityLevelChanged -= HandleLevelChanged;
}
```

## Component Setup

### `SanitySystem`

Add one `SanitySystem` component to the object responsible for sanity state in
the scene. Configure its starting value, maximum, drain, and level thresholds
through the Inspector.

Continuous drain uses `Time.deltaTime`, so it is frame-rate independent and
pauses when the game uses `Time.timeScale = 0`. Story systems can also pause it
explicitly through `SetDrainEnabled`.

### `SanityTrigger`

Add `SanityTrigger` to a trigger area and assign the scene's `SanitySystem`.

- negative `Sanity Change` values remove sanity;
- positive values restore sanity;
- `Trigger Once` consumes the trigger after its first valid activation;
- disabling `Trigger Once` allows it to activate again after the player exits
  and re-enters.

Required Unity configuration:

- the entering object must use the `Player` tag;
- both objects need compatible 2D colliders;
- the trigger's `BoxCollider2D` must use `Is Trigger`;
- at least one participating object needs a `Rigidbody2D`.

### `SanityBar`

The prototype HUD listens to both sanity events. It requires Inspector
references to:

- the `SanitySystem`;
- the filled UI `Image`;
- the TextMesh Pro level label;
- the Stable, Uneasy, and Disturbed threshold-marker RectTransforms.

The bar positions its markers from the configured thresholds rather than
assuming fixed pixel positions.

There is no reusable HUD prefab yet. The test-scene HUD is a reference
implementation, not a final artist-approved design.

### `SanityLevelActions`

This component exposes `UnityEvent` lists for Stable, Uneasy, Disturbed, and
Critical. It allows scene designers to connect public component methods in the
Inspector without changing `SanitySystem`.

`Invoke Current Level On Start` is enabled by default so the correct starting
state is applied when a scene begins.

### `SanityVignetteEffect`

The active vision prototype uses a URP global Volume with a Vignette override.
It requires:

- URP post-processing enabled on the camera;
- a Volume profile containing an active Vignette override;
- Inspector references to the `SanitySystem` and `Volume`.

The default intensity ranges from `0.05` at full sanity to `0.55` at zero
sanity. The effect is centred on the screen, so it assumes the gameplay camera
normally keeps the player near the centre.

`Fallbacks/SanityDarknessEffect.cs` provides a uniform UI-darkness version.
Its test-scene object is disabled. Do not enable both vision effects unless
intentionally testing their combined result.

## Test Scene

Open `Assets/Scenes/Dev/SanityTest.unity` without modifying a shared gameplay
scene.

The scene contains:

- a configured `SanitySystem`;
- a temporary Player-tagged object with 2D physics;
- a one-use sanity-loss trigger;
- the prototype HUD;
- a label demonstrating level `UnityEvent` actions;
- the active vignette and disabled darkness fallback.

Useful manual checks:

1. Increase `Drain Per Second` temporarily to observe transitions quickly.
2. Confirm fill, colour, level text, and vignette respond smoothly.
3. Confirm each level action fires once per transition.
4. Walk the test player through the trigger.
5. Verify `Trigger Once` and repeatable modes.
6. Restore `Drain Per Second` to `1` before saving.

## Current Limitations

- Sanity resets when the scene reloads and does not persist between scenes.
- There is no save/load integration.
- The system is not integrated into the shared player or production scenes.
- The HUD is placeholder UI and has no reusable prefab.
- Thresholds, drain rate, colours, and vignette intensity are not balanced from
  player testing.
- The vignette is centred on the screen rather than tracking an independently
  moving player.
- Torch behaviour, audio distortion, hallucinations, and game-over behaviour
  are not implemented.
- Development diagnostics currently log initialization, level transitions,
  trigger activations, and reaching zero sanity.

## Integration Guidance

Before editing a shared gameplay scene, coordinate with its owner. Prefer a
small integration change that assigns references to these standalone
components rather than modifying dialogue, player, or level-system internals.

The test scene should remain available as an isolated example even after the
system is integrated elsewhere.
