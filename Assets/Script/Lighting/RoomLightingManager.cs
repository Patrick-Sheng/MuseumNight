using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomLightingManager : MonoBehaviour
{
    public static RoomLightingManager Instance { get; private set; }

    [SerializeField] Light2D globalLight;
    [SerializeField] Light2D flashlight;
    [SerializeField] float litIntensity = 1f;
    [SerializeField] float darkIntensity = 0.05f;

    // Tracks which zone most recently entered, since RoomManager loads the next room
    // before unloading the current one - the outgoing room's OnDisable/ExitDarkRoom
    // would otherwise always fire after the incoming room's OnEnable/EnterDarkRoom
    // and wrongly clobber it back to lit.
    Object activeZone;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void EnterDarkRoom(Object zone)
    {
        activeZone = zone;
        Apply(true);
    }

    public void ExitDarkRoom(Object zone)
    {
        if (activeZone != zone) return;
        activeZone = null;
        Apply(false);
    }

    // Unconditionally forces lit state, ignoring zone ownership - for rooms that must
    // never be dark regardless of which zone last claimed activeZone (e.g. Room4Lighting).
    public void ForceExitDarkRoom()
    {
        activeZone = null;
        Apply(false);
    }

    void Apply(bool isDarkRoom)
    {
        if (globalLight != null)
            globalLight.intensity = isDarkRoom ? darkIntensity : litIntensity;

        if (flashlight != null)
        {
            if (flashlight.enabled != isDarkRoom)
                GameAudio.Play(isDarkRoom ? GameAudio.Cue.FlashlightOn : GameAudio.Cue.FlashlightOff);
            flashlight.enabled = isDarkRoom;
        }
    }
}
