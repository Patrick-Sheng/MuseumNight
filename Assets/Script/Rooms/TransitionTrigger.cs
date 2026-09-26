using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    public enum PlayerMode { Unchanged, TopDown, Platformer }

    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetEntryId;
    [SerializeField] private bool restorePlayerControl = false;
    [SerializeField] private PlayerMode destinationMode = PlayerMode.Unchanged;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Unity sends 2D trigger callbacks even to disabled MonoBehaviours.
        if (!isActiveAndEnabled) return;

        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.TryGoToRoom(targetSceneName, targetEntryId, restorePlayerControl, OnArrived);
        }
    }

    // Runs after RoomManager finishes its own transition (including restoring
    // player control), so this always has the final say on movement mode.
    private void OnArrived(bool success)
    {
        if (!success) return;

        switch (destinationMode)
        {
            case PlayerMode.TopDown:
                PlatformerModeController.ExitPlatformerMode();
                break;
            case PlayerMode.Platformer:
                PlatformerModeController.EnterPlatformerMode();
                break;
        }
    }
}
