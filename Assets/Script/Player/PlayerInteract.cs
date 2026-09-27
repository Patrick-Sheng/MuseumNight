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
            if (CloseUpViewUI.Instance != null && CloseUpViewUI.Instance.IsOpen)
                CloseUpViewUI.Instance.Hide();
            else
                currentInteractable?.Interact();
        }
    }

    public void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    public void ClearInteraction() => currentInteractable = null;

    public void ClearCurrentInteractable(IInteractable interactable)
    {
        if (currentInteractable == interactable)
            currentInteractable = null;
    }
}
