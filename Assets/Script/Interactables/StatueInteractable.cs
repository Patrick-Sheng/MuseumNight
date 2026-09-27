using UnityEngine;

public class StatueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject speechBubble;
    [SerializeField] Sprite closeUpSprite;

    void Awake()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    public void Interact()
    {
        if (closeUpSprite != null)
            CloseUpViewUI.Instance.Show(closeUpSprite);
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
