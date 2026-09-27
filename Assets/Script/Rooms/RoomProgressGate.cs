using UnityEngine;

/// Keeps gatedTransition disabled until the required GameProgress flag is set.
public class RoomProgressGate : MonoBehaviour
{
    [SerializeField] private TransitionTrigger gatedTransition;
    [Tooltip("If true, the transition is enabled until the flag is set, then locked (one-time-only room).")]
    [SerializeField] private bool invert;

    void Update()
    {
        if (gatedTransition != null)
            gatedTransition.enabled = invert != GameProgress.CompletedRoom3Platformer;
    }
}
