using System.Collections.Generic;
using UnityEngine;

/// <summary>A one-use objective, collected through the existing E-key interaction.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public class ObjectiveItem : MonoBehaviour, IInteractable
{
    [SerializeField] private StealthRoomController room;

    private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();
    private PlayerInteract nearbyPlayer;
    private bool collected;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (room == null || !GetComponent<BoxCollider2D>().isTrigger)
        {
            Debug.LogError("ObjectiveItem needs a Room reference and a BoxCollider2D with Is Trigger enabled.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled || collected) return;

        PlayerInteract player = other.GetComponentInParent<PlayerInteract>();
        if (player == null || !player.CompareTag("Player")) return;
        if (nearbyPlayer != null && nearbyPlayer != player) return;

        playerColliders.Add(other);
        nearbyPlayer = player;
        player.SetCurrentInteractable(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!playerColliders.Remove(other) || playerColliders.Count > 0) return;
        ClearInteraction();
    }

    public void Interact()
    {
        if (!isActiveAndEnabled || collected || nearbyPlayer == null ||
            !nearbyPlayer.isActiveAndEnabled || playerColliders.Count == 0 || !room.IsPlaying) return;

        if (!room.TryCollectObjective()) return;

        collected = true;
        ClearInteraction();
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ClearInteraction();
    }

    private void ClearInteraction()
    {
        if (nearbyPlayer != null) nearbyPlayer.ClearCurrentInteractable(this);
        nearbyPlayer = null;
        playerColliders.Clear();
    }

    private void OnGUI()
    {
        if (collected || nearbyPlayer == null || room == null || !room.IsPlaying) return;

        float scale = Mathf.Clamp(Screen.height / 720f, 0.75f, 2f);
        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize = Mathf.RoundToInt(24f * scale),
            alignment = TextAnchor.MiddleCenter
        };
        float width = Mathf.Min(420f * scale, Screen.width - 20f);
        GUI.Box(new Rect((Screen.width - width) * 0.5f,
            Screen.height - 90f * scale, width, 60f * scale), "Press E to collect", style);
    }
}
