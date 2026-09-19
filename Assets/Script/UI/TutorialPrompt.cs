using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TutorialPrompt : MonoBehaviour
{
    [SerializeField] float fadeDuration = 0.6f;

    CanvasGroup canvasGroup;
    PlayerMovement player;
    bool hasMoved;
    bool hasSprinted;
    bool dismissed;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        player = PlayerMovement.FindInScene();
    }

    void Update()
    {
        if (dismissed || player == null)
            return;

        if (player.isMoving)
            hasMoved = true;
        if (player.isSprinting)
            hasSprinted = true;

        if (hasMoved && hasSprinted)
        {
            dismissed = true;
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        float startAlpha = canvasGroup.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
