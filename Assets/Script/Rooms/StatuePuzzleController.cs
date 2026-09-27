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

    void Update()
    {
        if (doorTransition != null)
            doorTransition.enabled = IsSolved();
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
