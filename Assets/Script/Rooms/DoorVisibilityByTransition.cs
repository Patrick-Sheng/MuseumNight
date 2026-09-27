using UnityEngine;

/// Shows one of two door GameObjects depending on whether transitionTrigger is currently usable.
public class DoorVisibilityByTransition : MonoBehaviour
{
    [SerializeField] private TransitionTrigger transitionTrigger;
    [SerializeField] private GameObject closedDoorObject;
    [SerializeField] private GameObject openDoorObject;

    private bool? previousAvailableState;

    private void Awake() => Refresh();
    private void Update() => Refresh();

    private void Refresh()
    {
        if (transitionTrigger == null) return;

        bool available = transitionTrigger.IsTransitionAvailable();
        if (previousAvailableState.HasValue && previousAvailableState.Value == available)
            return;

        previousAvailableState = available;

        if (closedDoorObject != null) closedDoorObject.SetActive(!available);
        if (openDoorObject != null) openDoorObject.SetActive(available);
    }
}
