using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    IInteractable currentInteractable;

    void Update()
    {
        if (PauseMenu.IsPaused)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable?.Interact();
        }
    }

    public void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    public void ClearCurrentInteractable(IInteractable interactable)
    {
        if (currentInteractable == interactable)
            currentInteractable = null;
    }
}