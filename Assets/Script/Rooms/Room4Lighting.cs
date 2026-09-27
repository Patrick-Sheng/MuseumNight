using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Uses normal ambient lighting without the flashlight during Room 4.</summary>
public class Room4Lighting : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene() == gameObject.scene)
            ApplyLighting();
    }

    private void OnActiveSceneChanged(Scene previous, Scene current)
    {
        // RoomManager activates this room after unloading the previous room.
        if (current == gameObject.scene)
            ApplyLighting();
    }

    private void ApplyLighting()
    {
        RoomLightingManager.Instance?.ExitDarkRoom();
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
}
