using UnityEngine;

public class DarkRoomZone : MonoBehaviour
{
    void OnEnable()
    {
        RoomLightingManager.Instance?.EnterDarkRoom();
    }

    void OnDisable()
    {
        RoomLightingManager.Instance?.ExitDarkRoom();
    }
}
