using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomLightingManager : MonoBehaviour
{
    public static RoomLightingManager Instance { get; private set; }

    [SerializeField] Light2D globalLight;
    [SerializeField] Light2D flashlight;
    [SerializeField] float litIntensity = 1f;
    [SerializeField] float darkIntensity = 0.05f;

    bool isDarkRoom;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void EnterDarkRoom()
    {
        isDarkRoom = true;
        Apply();
    }

    public void ExitDarkRoom()
    {
        isDarkRoom = false;
        Apply();
    }

    void Apply()
    {
        if (globalLight != null)
            globalLight.intensity = isDarkRoom ? darkIntensity : litIntensity;

        if (flashlight != null)
            flashlight.enabled = isDarkRoom;
    }
}
