using System.Collections.Generic;
using UnityEngine;

/// <summary>An optional player-entry trigger for a reusable ChaseSpawner.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public class ChaseTrigger : MonoBehaviour
{
    [SerializeField] private ChaseSpawner spawner;
    private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();

    private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;

    private void Awake()
    {
        if (spawner == null || !GetComponent<BoxCollider2D>().isTrigger)
        {
            Debug.LogError("ChaseTrigger needs a Spawner reference and a trigger BoxCollider2D.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled) return;
        Rigidbody2D body = other.attachedRigidbody;
        if (body == null || !body.CompareTag("Player")) return;
        if (playerColliders.Add(other) && playerColliders.Count == 1) spawner.Spawn();
    }

    private void OnTriggerExit2D(Collider2D other) => playerColliders.Remove(other);
    private void OnDisable() => playerColliders.Clear();
}
