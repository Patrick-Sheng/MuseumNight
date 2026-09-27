using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Shows the final Room 4 result over the game and returns to the main menu.</summary>
public class Room4EndingScreen : MonoBehaviour
{
    private const string MainMenuScene = "MainMenu";

    public static void Show(Sprite endingImage, bool won, float fadeDuration, float blackScreenDuration)
    {
        GameObject root = new GameObject("EndingScreen", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(Room4EndingScreen));
        root.GetComponent<Room4EndingScreen>().Build(endingImage, won, fadeDuration, blackScreenDuration);
    }

    private void Build(Sprite endingImage, bool won, float fadeDuration, float blackScreenDuration)
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image blackScreen = CreateImage("BlackScreen", transform);
        Stretch(blackScreen.rectTransform);
        blackScreen.color = Color.black;

        GameObject content = new GameObject("EndingContent", typeof(RectTransform));
        content.transform.SetParent(transform, false);
        Stretch(content.GetComponent<RectTransform>());

        Image background = CreateImage("EndingImage", content.transform);
        Stretch(background.rectTransform);
        background.sprite = endingImage;
        background.color = endingImage == null
            ? new Color(0.08f, 0.09f, 0.13f, 1f)
            : Color.white;

        TextMeshProUGUI result = CreateText("ResultText", content.transform, won ? "WIN" : "LOSS", 112);
        result.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        result.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        result.rectTransform.sizeDelta = new Vector2(900f, 180f);
        result.rectTransform.anchoredPosition = new Vector2(0f, 170f);

        Image buttonImage = CreateImage("MainMenuButton", content.transform);
        buttonImage.color = new Color(0.12f, 0.13f, 0.18f, 0.92f);
        RectTransform buttonRect = buttonImage.rectTransform;
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(380f, 90f);
        buttonRect.anchoredPosition = new Vector2(0f, -320f);

        Button button = buttonImage.gameObject.AddComponent<Button>();
        button.onClick.AddListener(ReturnToMainMenu);

        TextMeshProUGUI label = CreateText("Label", buttonRect, "MAIN MENU", 40);
        Stretch(label.rectTransform);

        content.SetActive(false);
        CanvasGroup group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        StartCoroutine(FadeToBlackThenShow(group, content, fadeDuration, blackScreenDuration));

        if (PauseMenu.Instance != null)
        {
            PauseMenu.Instance.Resume();
            PauseMenu.Instance.enabled = false;
        }
        Time.timeScale = 0f;
    }

    private static IEnumerator FadeToBlackThenShow(CanvasGroup group, GameObject content,
        float fadeDuration, float blackScreenDuration)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = 1f;

        if (blackScreenDuration > 0f)
            yield return new WaitForSecondsRealtime(blackScreenDuration);

        content.SetActive(true);
        group.interactable = true;
    }

    private static Image CreateImage(string objectName, Transform parent)
    {
        GameObject child = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        child.transform.SetParent(parent, false);
        return child.GetComponent<Image>();
    }

    private static TextMeshProUGUI CreateText(string objectName, Transform parent, string value, float size)
    {
        GameObject child = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        child.transform.SetParent(parent, false);
        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        if (PersistentRoot.Instance != null)
            Destroy(PersistentRoot.Instance.gameObject);
        SceneManager.LoadScene(MainMenuScene);
    }
}
