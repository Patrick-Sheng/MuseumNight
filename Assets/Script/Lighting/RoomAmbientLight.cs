using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>Temporarily adjusts the global light while this room is active.</summary>
public class RoomAmbientLight : MonoBehaviour
{
    [Tooltip("Optional override. Otherwise finds the single active Global Light2D.")]
    [SerializeField] private Light2D globalLight;
    [SerializeField, Range(0f, 1f)] private float ambientIntensity = 0.35f;

    private float previousIntensity;
    private float appliedIntensity;
    private bool applied;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void Start()
    {
        // Also supports opening the room directly with a test global light.
        if (SceneManager.GetActiveScene() == gameObject.scene) Apply();
    }

    private void HandleActiveSceneChanged(Scene previous, Scene current)
    {
        // RoomManager activates the destination AFTER unloading the previous room.
        // Waiting for this event prevents retries from capturing the old room's dim value.
        if (current == gameObject.scene) Apply();
    }

    private void Apply()
    {
        if (applied) return;
        if (globalLight == null)
        {
            foreach (Light2D light in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
            {
                if (!light.isActiveAndEnabled || light.lightType != Light2D.LightType.Global) continue;
                if (globalLight != null)
                {
                    Debug.LogError("RoomAmbientLight found multiple global lights. Assign the intended Global Light explicitly.", this);
                    globalLight = null;
                    return;
                }
                globalLight = light;
            }
        }
        if (globalLight == null)
        {
            Debug.LogWarning("RoomAmbientLight needs the persistent Global Light. Start through MainMenu, or assign a test Global Light.", this);
            return;
        }
        previousIntensity = globalLight.intensity;
        appliedIntensity = ambientIntensity;
        globalLight.intensity = appliedIntensity;
        applied = true;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        // The next room may already have set its own intensity while loading additively.
        // In particular, preserve Room 2's DarkRoomZone value instead of overwriting it.
        if (applied && globalLight != null && Mathf.Approximately(globalLight.intensity, appliedIntensity))
            globalLight.intensity = previousIntensity;
        applied = false;
    }
}
