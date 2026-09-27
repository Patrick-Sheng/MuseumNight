using System.Collections.Generic;
using DialogueSystem.Data;
using DialogueSystem.Runtime.Narration;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TransitionDialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueContainer dialogue;
    [SerializeField] private NarrativeController narrativeController;

    private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();
    private PlayerInteract nearbyPlayer;

    private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;

    private void Awake()
    {
        if (!GetComponent<BoxCollider2D>().isTrigger)
        {
            Debug.LogError("TransitionDialogueInteractable requires a trigger BoxCollider2D.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled)
            return;

        PlayerInteract player = other.GetComponentInParent<PlayerInteract>();
        if (player == null || !player.CompareTag("Player"))
            return;

        playerColliders.Add(other);
        nearbyPlayer = player;
        nearbyPlayer.SetCurrentInteractable(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!playerColliders.Remove(other) || playerColliders.Count > 0)
            return;

        if (nearbyPlayer != null)
            nearbyPlayer.ClearCurrentInteractable(this);

        nearbyPlayer = null;
    }

    private void OnDisable()
    {
        if (nearbyPlayer != null)
            nearbyPlayer.ClearCurrentInteractable(this);

        nearbyPlayer = null;
        playerColliders.Clear();
    }

    public void Interact()
    {
        if (!isActiveAndEnabled || nearbyPlayer == null || playerColliders.Count == 0 || dialogue == null)
            return;

        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeController>();

        if (narrativeController == null)
        {
            Debug.LogWarning("TransitionDialogueInteractable: dialogue is assigned but no NarrativeController was found.", this);
            return;
        }

        if (!narrativeController.IsNarrating)
            narrativeController.BeginNarration(dialogue, null);
    }
}