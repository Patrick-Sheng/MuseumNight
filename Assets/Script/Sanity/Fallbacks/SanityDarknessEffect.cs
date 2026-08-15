using UnityEngine;
using UnityEngine.UI;

public sealed class SanityDarknessEffect : MonoBehaviour
{
    [SerializeField] private SanitySystem sanitySystem;
    [SerializeField] private Image darknessOverlay;

    [Range(0f, 1f)]
    [SerializeField] private float maximumDarkness = 0.65f;

    private void Awake()
    {
        if (sanitySystem != null && darknessOverlay != null)
            return;

        Debug.LogError(
            "Sanity Darkness Effect has missing Inspector references.",
            this
        );

        enabled = false;
    }

    private void OnEnable()
    {
        if (sanitySystem != null)
            sanitySystem.SanityChanged += UpdateDarkness;
    }

    private void Start()
    {
        UpdateDarkness(
            sanitySystem.CurrentSanity,
            sanitySystem.MaxSanity
        );
    }

    private void OnDisable()
    {
        if (sanitySystem != null)
            sanitySystem.SanityChanged -= UpdateDarkness;
    }

    private void UpdateDarkness(
        float currentSanity,
        float maxSanity
    )
    {
        float normalizedSanity = maxSanity > 0f
            ? currentSanity / maxSanity
            : 0f;

        float darkness = (1f - normalizedSanity) * maximumDarkness;

        Color overlayColour = darknessOverlay.color;
        overlayColour.a = darkness;
        darknessOverlay.color = overlayColour;
    }
}
