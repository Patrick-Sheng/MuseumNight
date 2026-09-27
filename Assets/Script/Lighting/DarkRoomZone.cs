using UnityEngine;

public class DarkRoomZone : MonoBehaviour
{
    void OnEnable()
    {
        RoomLightingManager.Instance?.EnterDarkRoom(this);
    }

    void OnDisable()
    {
        RoomLightingManager.Instance?.ExitDarkRoom(this);
    }
}
