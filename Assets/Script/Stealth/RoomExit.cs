using UnityEngine;
using System.Collections.Generic;
using DialogueSystem.Data;
using DialogueSystem.Runtime.Narration;

/// <summary>Completes the room when its player enters after collecting the objective.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public class RoomExit : MonoBehaviour, IInteractable
{
    [SerializeField] private StealthRoomController room;
    [Tooltip("Leave empty for the standalone completion screen. Set a scene name for production transitions.")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetEntryId = "1";
    [Header("Blocked Exit Dialogue")]
    [SerializeField] private NarrativeController narrativeController;
    [SerializeField] private DialogueContainer blockedExitDialogue;

    private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();
    private PlayerInteract nearbyPlayer;
    private bool transitionAttempted;

    private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;

    private void Awake()
    {
        if (room == null || !GetComponent<BoxCollider2D>().isTrigger)
        {
            Debug.LogError("RoomExit needs a Room reference and a trigger BoxCollider2D.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled || room == null || !room.IsPlayer(other))
            return;

        playerColliders.Add(other);
        nearbyPlayer = other.GetComponentInParent<PlayerInteract>();
        if (nearbyPlayer != null)
            nearbyPlayer.SetCurrentInteractable(this);

        TryExit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActiveAndEnabled || room == null || !room.IsPlayer(other))
            return;

        playerColliders.Add(other);
        if (nearbyPlayer == null)
            nearbyPlayer = other.GetComponentInParent<PlayerInteract>();

        TryExit(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (room == null || !room.IsPlayer(other))
            return;

        transitionAttempted = false;
        playerColliders.Remove(other);
        if (playerColliders.Count > 0)
            return;

        if (nearbyPlayer != null)
            nearbyPlayer.ClearCurrentInteractable(this);
        nearbyPlayer = null;
    }

    private void TryExit(Collider2D other)
    {
        if (isActiveAndEnabled && room != null && room.IsPlaying && room.HasObjective && room.IsPlayer(other))
        {
            if (string.IsNullOrWhiteSpace(targetSceneName)) room.CompleteRoom();
            else if (!transitionAttempted)
            {
                transitionAttempted = true;
                room.TryExitToRoom(targetSceneName, targetEntryId);
            }
        }
    }

    public void Interact()
    {
        if (!isActiveAndEnabled || room == null || !room.IsPlaying || nearbyPlayer == null || playerColliders.Count == 0)
            return;

        if (room.HasObjective)
        {
            if (string.IsNullOrWhiteSpace(targetSceneName)) room.CompleteRoom();
            else if (!transitionAttempted)
            {
                transitionAttempted = true;
                room.TryExitToRoom(targetSceneName, targetEntryId);
            }
            return;
        }

        PlayBlockedExitDialogue();
    }

    private void PlayBlockedExitDialogue()
    {
        if (blockedExitDialogue == null)
            return;

        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeController>();

        if (narrativeController == null)
        {
            Debug.LogWarning("RoomExit: blockedExitDialogue is assigned but no NarrativeController was found.", this);
            return;
        }

        if (!narrativeController.IsNarrating)
            narrativeController.BeginNarration(blockedExitDialogue, null);
    }
}
