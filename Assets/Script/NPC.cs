using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    PlayerInteract playerInRange;

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
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteract playerInteract = other.GetComponent<PlayerInteract>();
            playerInteract?.ClearCurrentInteractable(this);
        }
    }
}