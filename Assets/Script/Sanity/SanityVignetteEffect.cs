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
        if (sanitySystem == null
            || volume == null
            || volume.profile == null
            || !volume.profile.TryGet(out vignette))
        {
            Debug.LogError(
                "Sanity Vignette Effect has missing references or no Vignette override.",
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

    private void OnValidate()
    {
        maximumIntensity = Mathf.Max(
            maximumIntensity,
            minimumIntensity
        );
    }
}
