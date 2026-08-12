using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject speechBubble;

    void Awake()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    public void Interact()
    {
        Debug.Log("Talking to NPC");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteract playerInteract = other.GetComponent<PlayerInteract>();
            playerInteract?.SetCurrentInteractable(this);

            if (speechBubble != null)
                speechBubble.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteract playerInteract = other.GetComponent<PlayerInteract>();
            playerInteract?.ClearCurrentInteractable(this);

            if (speechBubble != null)
                speechBubble.SetActive(false);
        }
    }
}