using UnityEngine;
using UnityEngine.Events;

public sealed class SanityLevelActions : MonoBehaviour
{
    [SerializeField] private SanitySystem sanitySystem;

    [Tooltip("Invoke the action for the starting level when the scene begins.")]
    [SerializeField] private bool invokeCurrentLevelOnStart = true;

    [Header("Level Actions")]
    [SerializeField] private UnityEvent onStable = new UnityEvent();
    [SerializeField] private UnityEvent onUneasy = new UnityEvent();
    [SerializeField] private UnityEvent onDisturbed = new UnityEvent();
    [SerializeField] private UnityEvent onCritical = new UnityEvent();

    private void OnEnable()
    {
        if (sanitySystem != null)
            sanitySystem.SanityLevelChanged += HandleLevelChanged;
    }

    private void Start()
    {
        if (sanitySystem == null)
        {
            Debug.LogError(
                "Sanity Level Actions needs a Sanity System reference.",
                this
            );

            return;
        }

        if (invokeCurrentLevelOnStart)
            HandleLevelChanged(sanitySystem.CurrentLevel);
    }

    private void OnDisable()
    {
        if (sanitySystem != null)
            sanitySystem.SanityLevelChanged -= HandleLevelChanged;
    }

    private void HandleLevelChanged(SanityLevel level)
    {
        switch (level)
        {
            case SanityLevel.Stable:
                onStable.Invoke();
                break;

            case SanityLevel.Uneasy:
                onUneasy.Invoke();
                break;

            case SanityLevel.Disturbed:
                onDisturbed.Invoke();
                break;

            case SanityLevel.Critical:
                onCritical.Invoke();
                break;
        }
    }
}