using UnityEngine;

/// <summary>Completes the room when its player enters after collecting the objective.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public class RoomExit : MonoBehaviour
{
    [SerializeField] private StealthRoomController room;

    private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;

    private void Awake()
    {
        if (room == null || !GetComponent<BoxCollider2D>().isTrigger)
        {
            Debug.LogError("RoomExit needs a Room reference and a trigger BoxCollider2D.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) => TryExit(other);
    private void OnTriggerStay2D(Collider2D other) => TryExit(other);

    private void TryExit(Collider2D other)
    {
        if (isActiveAndEnabled && room != null && room.IsPlaying && room.HasObjective && room.IsPlayer(other))
            room.CompleteRoom();
    }
}
