using UnityEngine;

/// <summary>Place one on each shard sprite and choose a distinct ID in the Inspector.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public class MirrorShardPickup : MonoBehaviour
{
    [SerializeField] private MirrorShardId shardId;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnValidate()
    {
        BoxCollider2D pickupCollider = GetComponent<BoxCollider2D>();
        if (pickupCollider != null) pickupCollider.isTrigger = true;
    }

    private void Start()
    {
        MirrorShardProgress progress = MirrorShardProgress.GetOrCreate();
        if (progress != null && progress.IsCollected(shardId))
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        MirrorShardProgress progress = MirrorShardProgress.GetOrCreate();
        if (progress == null) return;

        progress.Collect(shardId);
        gameObject.SetActive(false);
    }
}
