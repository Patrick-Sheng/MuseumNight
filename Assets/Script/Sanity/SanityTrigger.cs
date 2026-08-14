using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class SanityTrigger : MonoBehaviour
{
    [SerializeField] private SanitySystem sanitySystem;
    [SerializeField] private float sanityChange = -10f;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnce && hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (sanitySystem == null)
        {
            Debug.LogError(
                "Sanity Trigger needs a Sanity System reference.",
                this
            );
            return;
        }

        sanitySystem.ChangeSanity(sanityChange);
        hasTriggered = true;

        Debug.Log(
            $"Sanity trigger applied {sanityChange} sanity.",
            this
        );
    }
}