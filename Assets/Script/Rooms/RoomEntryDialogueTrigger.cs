using System.Collections;
using System.Collections.Generic;
using DialogueSystem.Data;
using DialogueSystem.Runtime.Narration;
using UnityEngine;

/// <summary>
/// Plays a room entry dialogue when this scene becomes active.
/// Uses runtime lookup for NarrativeController to avoid cross-scene reference issues.
/// </summary>
public class RoomEntryDialogueTrigger : MonoBehaviour
{
    private static readonly HashSet<string> PlayedKeys = new HashSet<string>();

    [SerializeField] private DialogueContainer entryDialogue;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool playOncePerSession = true;
    [SerializeField] private bool waitForRoomTransitionToFinish = true;
    [SerializeField] private float startDelaySeconds;

    private Coroutine playRoutine;

    private void OnEnable()
    {
        if (playOnEnable)
            Trigger();
    }

    public void Trigger()
    {
        if (!isActiveAndEnabled || entryDialogue == null)
            return;

        string key = BuildKey();
        if (playOncePerSession && PlayedKeys.Contains(key))
            return;

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine(key));
    }

    private IEnumerator PlayRoutine(string key)
    {
        if (startDelaySeconds > 0f)
            yield return new WaitForSeconds(startDelaySeconds);

        while (isActiveAndEnabled && (PauseMenu.IsPaused ||
               (waitForRoomTransitionToFinish && RoomManager.Instance != null && RoomManager.Instance.IsTransitioning)))
        {
            yield return null;
        }

        if (!isActiveAndEnabled)
            yield break;

        NarrativeController narrativeController = FindFirstObjectByType<NarrativeController>();
        if (narrativeController == null)
        {
            Debug.LogWarning("RoomEntryDialogueTrigger: No NarrativeController found in loaded scenes.", this);
            yield break;
        }

        while (isActiveAndEnabled && narrativeController.IsNarrating)
            yield return null;

        if (!isActiveAndEnabled)
            yield break;

        if (playOncePerSession)
            PlayedKeys.Add(key);

        narrativeController.BeginNarration(entryDialogue, null);
    }

    private string BuildKey()
    {
        string sceneKey = string.IsNullOrWhiteSpace(gameObject.scene.path)
            ? gameObject.scene.name
            : gameObject.scene.path;
        return sceneKey + "::" + entryDialogue.name;
    }
}