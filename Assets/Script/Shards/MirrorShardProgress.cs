using UnityEngine;

public enum MirrorShardId { First, Second, Third }

/// <summary>Tracks the three distinct mirror shards while the persistent game is running.</summary>
public class MirrorShardProgress : MonoBehaviour
{
    private readonly bool[] collected = new bool[3];

    public int CollectedCount
    {
        get
        {
            int count = 0;
            foreach (bool shard in collected)
                if (shard) count++;
            return count;
        }
    }

    public bool HasAllThree => CollectedCount == collected.Length;

    public bool IsCollected(MirrorShardId id)
    {
        int index = (int)id;
        return index >= 0 && index < collected.Length && collected[index];
    }

    public static MirrorShardProgress GetOrCreate()
    {
        PersistentRoot root = PersistentRoot.Instance;
        if (root == null)
        {
            Debug.LogError("MirrorShardProgress needs the Persistent scene loaded.");
            return null;
        }

        MirrorShardProgress progress = root.GetComponent<MirrorShardProgress>();
        return progress != null ? progress : root.gameObject.AddComponent<MirrorShardProgress>();
    }

    public void Collect(MirrorShardId id)
    {
        int index = (int)id;
        if (index < 0 || index >= collected.Length)
        {
            Debug.LogError("Invalid mirror shard ID: " + id, this);
            return;
        }
        collected[index] = true;
    }
}
