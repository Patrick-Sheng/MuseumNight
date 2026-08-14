using UnityEngine;
using System;

[DisallowMultipleComponent]
public sealed class SanitySystem : MonoBehaviour
{
    [Header("Sanity")]
    [SerializeField, Min(1f)] private float maxSanity = 100f;
    [SerializeField, Min(0f)] private float startingSanity = 100f;

    [Header("Continuous Drain")]
    [SerializeField] private bool drainEnabled = true;
    [SerializeField, Min(0f)] private float drainPerSecond = 1f;

    [Header("Level Thresholds (percentage of maximum)")]
    [SerializeField, Range(0f, 1f)] private float stableMinimum = 0.75f;
    [SerializeField, Range(0f, 1f)] private float uneasyMinimum = 0.50f;
    [SerializeField, Range(0f, 1f)] private float disturbedMinimum = 0.25f;

    private float currentSanity;
    private SanityLevel currentLevel;

    public float CurrentSanity => currentSanity;
    public float MaxSanity => maxSanity;
    public float NormalizedSanity => maxSanity > 0f ? currentSanity / maxSanity : 0f;
    public SanityLevel CurrentLevel => currentLevel;

    public event Action<float, float> SanityChanged;
    public event Action<SanityLevel> SanityLevelChanged;

    private void Awake()
    {
        currentSanity = Mathf.Clamp(startingSanity, 0f, maxSanity);
        currentLevel = CalculateLevel();
        Debug.Log($"Starting sanity: {currentSanity}/{maxSanity}, level: {currentLevel}", this);
    }

    private void Update()
    {
        if (!drainEnabled || currentSanity <= 0f)
            return;

        ChangeSanity(-drainPerSecond * Time.deltaTime);
        if (currentSanity <= 0f)
            Debug.Log("Sanity reached zero.", this);
    }

    public void SetDrainEnabled(bool enabled)
    {
        drainEnabled = enabled;
    }

    public void ChangeSanity(float amount)
    {
        float newSanity = Mathf.Clamp(
            currentSanity + amount,
            0f,
            maxSanity
        );

        if (Mathf.Approximately(newSanity, currentSanity))
            return;

        SanityLevel previousLevel = currentLevel;

        currentSanity = newSanity;
        currentLevel = CalculateLevel();

        SanityChanged?.Invoke(currentSanity, maxSanity);

        if (currentLevel != previousLevel)
        {
            Debug.Log(
                $"Sanity level changed: {previousLevel} → {currentLevel}",
                this
            );

            SanityLevelChanged?.Invoke(currentLevel);
        }
    }

    private SanityLevel CalculateLevel()
    {
        float normalized = NormalizedSanity;

        if (normalized >= stableMinimum)
            return SanityLevel.Stable;

        if (normalized >= uneasyMinimum)
            return SanityLevel.Uneasy;

        if (normalized >= disturbedMinimum)
            return SanityLevel.Disturbed;

        return SanityLevel.Critical;
    }

    private void OnValidate()
    {
        maxSanity = Mathf.Max(1f, maxSanity);
        startingSanity = Mathf.Clamp(startingSanity, 0f, maxSanity);
        drainPerSecond = Mathf.Max(0f, drainPerSecond);

        stableMinimum = Mathf.Clamp01(stableMinimum);
        uneasyMinimum = Mathf.Clamp(uneasyMinimum, 0f, stableMinimum);
        disturbedMinimum = Mathf.Clamp(disturbedMinimum, 0f, uneasyMinimum);
    }
}
