using UnityEngine;
using UnityEngine.Events;

public class ChestTest : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject speechBubble;
    [SerializeField] Sprite closeUpSprite;
    public UnityEvent someEvent;


    void Awake()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    public void Interact()
    {
        Debug.Log("Opening Chest Test");

        if (someEvent != null)
        {
            someEvent.Invoke();
        }
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