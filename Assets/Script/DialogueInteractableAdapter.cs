using UnityEngine;

public class DialogueInteractableAdapter : MonoBehaviour, IInteractable
{
    [Header("Dialogue Data")]
    [SerializeField] private DialogueSystem.Runtime.Narration.NarrativeController narrativeController;
    [SerializeField] private DialogueSystem.Data.DialogueContainer dialogueData;

    [Header("Optional")]
    [SerializeField] private GameObject interactionHint;

    private void Awake()
    {
        if (narrativeController == null)
        {
            narrativeController = FindFirstObjectByType<DialogueSystem.Runtime.Narration.NarrativeController>();
        }

        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    public void Interact()
    {
        if (narrativeController != null && dialogueData != null)
        {
            if (!narrativeController.IsNarrating)
            {
                narrativeController.BeginNarration(dialogueData, null);
            }

            return;
        }

        Debug.LogWarning($"{name}: Assign both NarrativeController and DialogueData.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        var playerInteract = other.GetComponent<PlayerInteract>();
        playerInteract?.SetCurrentInteractable(this);

        if (interactionHint != null)
        {
            interactionHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        var playerInteract = other.GetComponent<PlayerInteract>();
        playerInteract?.ClearCurrentInteractable(this);

        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }
}
