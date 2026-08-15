using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class SanityVignetteEffect : MonoBehaviour
{
    [SerializeField] private SanitySystem sanitySystem;
    [SerializeField] private Volume volume;

    [Header("Vignette Intensity")]
    [Range(0f, 1f)]
    [SerializeField] private float minimumIntensity = 0.05f;

    [Range(0f, 1f)]
    [SerializeField] private float maximumIntensity = 0.55f;

    private Vignette vignette;

    private void Awake()
    {
        if (volume == null
            || volume.profile == null
            || !volume.profile.TryGet(out vignette))
        {
            Debug.LogError(
                "Sanity Vignette Effect needs a Volume profile containing a Vignette.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (sanitySystem != null)
            sanitySystem.SanityChanged += UpdateVignette;
    }

    private void Start()
    {
        if (sanitySystem == null)
        {
            Debug.LogError(
                "Sanity Vignette Effect needs a Sanity System reference.",
                this
            );

            enabled = false;
            return;
        }

        UpdateVignette(
            sanitySystem.CurrentSanity,
            sanitySystem.MaxSanity
        );
    }

    private void OnDisable()
    {
        if (sanitySystem != null)
            sanitySystem.SanityChanged -= UpdateVignette;
    }

    private void UpdateVignette(
        float currentSanity,
        float maxSanity
    )
    {
        float normalizedSanity = maxSanity > 0f
            ? currentSanity / maxSanity
            : 0f;

        float intensity = Mathf.Lerp(
            maximumIntensity,
            minimumIntensity,
            normalizedSanity
        );

        vignette.intensity.Override(intensity);
    }
}