using DialogueSystem.Runtime.Narration;
using UnityEngine;

// This script handle the input for skipping dialogue, and advance to the next line.
namespace DialogueSystem.Runtime.Interaction
{
    public class DialogueInputHandler : DialogueMonoBehaviour
    {
        private void Awake()
        {
            if (narrativeController == null)
            {
                narrativeController = FindFirstObjectByType<NarrativeController>();
            }
        }

        private void Update()
        {
            SkipDialogueWithInput();
        }
    }
}
