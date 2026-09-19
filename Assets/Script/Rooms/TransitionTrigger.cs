using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetEntryId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Unity sends 2D trigger callbacks even to disabled MonoBehaviours.
        if (!isActiveAndEnabled) return;

        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.GoToRoom(targetSceneName, targetEntryId);
        }
    }
}
