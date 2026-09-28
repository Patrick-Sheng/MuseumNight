using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    public enum PlayerMode { Unchanged, TopDown, Platformer }

    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetEntryId;
    [SerializeField] private bool restorePlayerControl = false;
    [SerializeField] private PlayerMode destinationMode = PlayerMode.Unchanged;
    [SerializeField] private bool blockReturnToPreviousRoom = true;

    public bool IsTransitionAvailable()
    {
        if (!isActiveAndEnabled)
            return false;

        RoomManager manager = RoomManager.Instance;
        if (manager == null)
            return false;

        if (blockReturnToPreviousRoom &&
            !string.IsNullOrWhiteSpace(targetSceneName) &&
            !string.IsNullOrWhiteSpace(manager.PreviousSceneName) &&
            string.Equals(targetSceneName, manager.PreviousSceneName, System.StringComparison.Ordinal))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(targetSceneName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTransition(other);
    }

    // Also handles a door that opens while the player is already in its trigger.
    private void OnTriggerStay2D(Collider2D other)
    {
        TryTransition(other);
    }

    private void TryTransition(Collider2D other)
    {
        // Unity sends 2D trigger callbacks even to disabled MonoBehaviours.
        if (!isActiveAndEnabled) return;

        if (other.CompareTag("Player"))
        {
            RoomManager manager = RoomManager.Instance;
            if (manager == null)
                return;

            if (!IsTransitionAvailable())
                return;

            manager.TryGoToRoom(targetSceneName, targetEntryId, restorePlayerControl, OnArrived, true);
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
