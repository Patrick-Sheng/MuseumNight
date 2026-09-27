using UnityEngine;

/// Unlocks a TransitionTrigger once every configured statue slider sits on its target slot.
public class StatuePuzzleController : MonoBehaviour
{
    [System.Serializable]
    public class StatueTarget
    {
        public PushableSlider slider;
        [Range(0, 9)] public int targetIndex;
    }

    [SerializeField] private StatueTarget[] statues = new StatueTarget[4];
    [SerializeField] private TransitionTrigger doorTransition;

    void Start()
    {
        // Statues reset to their scene defaults on every reload of Room2, so once the
        // puzzle has been solved, snap them straight back into their solved positions.
        if (GameProgress.Room2StatuePuzzleSolved)
        {
            foreach (StatueTarget statue in statues)
                statue.slider?.SnapTo(statue.targetIndex);
        }
    }

    void Update()
    {
        // Room2 fully reloads (and statues reset to their scene defaults) on each
        // visit, so latch the solved state persistently instead of re-checking
        // live positions alone - otherwise leaving and returning re-locks the door.
        if (!GameProgress.Room2StatuePuzzleSolved && IsSolved())
            GameProgress.Room2StatuePuzzleSolved = true;

        if (doorTransition != null)
            doorTransition.enabled = GameProgress.Room2StatuePuzzleSolved;
    }

    bool IsSolved()
    {
        foreach (StatueTarget statue in statues)
        {
            if (statue.slider == null || statue.slider.CurrentIndex != statue.targetIndex)
                return false;
        }
        return true;
    }
}
