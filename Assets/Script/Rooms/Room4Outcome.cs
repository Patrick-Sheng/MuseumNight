using UnityEngine;
using UnityEngine.Playables;

/// <summary>Chooses exactly one ending after the Room 4 introduction.</summary>
public class Room4Outcome : MonoBehaviour
{
    [SerializeField] private PlayableDirector winDirector;
    [SerializeField] private PlayableDirector lossDirector;
    [SerializeField] private Room4EndingVisuals endingVisuals;

    [Header("Preview before shard pickups are placed")]
    [SerializeField] private bool usePreviewResult;
    [SerializeField] private bool previewHasAllThree;

    private bool played;

    public void PlayOutcome()
    {
        if (played) return;
        if (winDirector == null || lossDirector == null || endingVisuals == null)
        {
            Debug.LogError("Room4Outcome needs both ending Directors and Ending Visuals assigned.", this);
            return;
        }

        MirrorShardProgress progress = usePreviewResult ? null : MirrorShardProgress.GetOrCreate();
        bool hasAllThree = usePreviewResult ? previewHasAllThree : progress != null && progress.HasAllThree;
        PlayableDirector selected = hasAllThree ? winDirector : lossDirector;

        played = true;
        selected.time = 0;
        selected.Play();
        endingVisuals.Play(hasAllThree);
        Debug.Log("Room 4 outcome: " + (hasAllThree ? "WIN" : "LOSS")
            + (usePreviewResult ? " (preview)" : " (" + (progress != null ? progress.CollectedCount : 0) + "/3 shards)"), this);
    }
}
