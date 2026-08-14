using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SanityBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SanitySystem sanitySystem;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text levelLabel;

    [Header("Threshold Markers")]
    [SerializeField] private RectTransform stableThresholdMarker;
    [SerializeField] private RectTransform uneasyThresholdMarker;
    [SerializeField] private RectTransform disturbedThresholdMarker;
    [SerializeField, Min(1f)] private float markerWidth = 2f;

    [Header("Level Colours")]
    [SerializeField] private Color stableColour =
        new Color(0.2f, 0.8f, 0.4f);

    [SerializeField] private Color uneasyColour =
        new Color(0.95f, 0.8f, 0.2f);

    [SerializeField] private Color disturbedColour =
        new Color(0.95f, 0.4f, 0.15f);

    [SerializeField] private Color criticalColour =
        new Color(0.7f, 0.05f, 0.1f);

    private void OnEnable()
    {
        if (sanitySystem == null)
            return;

        sanitySystem.SanityChanged += UpdateFill;
        sanitySystem.SanityLevelChanged += UpdateLevelDisplay;
    }

    private void Start()
    {
        if (!ReferencesAreAssigned())
        {
            Debug.LogError(
                "Sanity Bar has missing Inspector references.",
                this
            );

            return;
        }

        UpdateFill(
            sanitySystem.CurrentSanity,
            sanitySystem.MaxSanity
        );

        UpdateLevelDisplay(sanitySystem.CurrentLevel);
        PositionThresholdMarkers();
    }

    private void OnDisable()
    {
        if (sanitySystem == null)
            return;

        sanitySystem.SanityChanged -= UpdateFill;
        sanitySystem.SanityLevelChanged -= UpdateLevelDisplay;
    }

    private bool ReferencesAreAssigned()
    {
        return sanitySystem != null
            && fillImage != null
            && levelLabel != null
            && stableThresholdMarker != null
            && uneasyThresholdMarker != null
            && disturbedThresholdMarker != null;
    }

    private void UpdateFill(float currentSanity, float maxSanity)
    {
        fillImage.fillAmount = maxSanity > 0f
            ? currentSanity / maxSanity
            : 0f;
    }

    private void UpdateLevelDisplay(SanityLevel level)
    {
        levelLabel.text = level.ToString().ToUpperInvariant();

        switch (level)
        {
            case SanityLevel.Stable:
                fillImage.color = stableColour;
                break;

            case SanityLevel.Uneasy:
                fillImage.color = uneasyColour;
                break;

            case SanityLevel.Disturbed:
                fillImage.color = disturbedColour;
                break;

            case SanityLevel.Critical:
                fillImage.color = criticalColour;
                break;
        }
    }

    private void PositionThresholdMarkers()
    {
        PositionMarker(
            stableThresholdMarker,
            sanitySystem.StableThreshold
        );

        PositionMarker(
            uneasyThresholdMarker,
            sanitySystem.UneasyThreshold
        );

        PositionMarker(
            disturbedThresholdMarker,
            sanitySystem.DisturbedThreshold
        );
    }

    private void PositionMarker(
        RectTransform marker,
        float normalizedPosition
    )
    {
        marker.anchorMin = new Vector2(normalizedPosition, 0f);
        marker.anchorMax = new Vector2(normalizedPosition, 1f);
        marker.pivot = new Vector2(0.5f, 0.5f);
        marker.anchoredPosition = Vector2.zero;
        marker.sizeDelta = new Vector2(markerWidth, 0f);
    }
}