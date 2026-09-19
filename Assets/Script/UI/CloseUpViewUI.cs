using UnityEngine;
using UnityEngine.UI;

public class CloseUpViewUI : MonoBehaviour
{
    public static CloseUpViewUI Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Image displayImage;

    bool isOpen = false;

    void Awake()
    {
        Instance = this;
        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    public void Show(Sprite sprite)
    {
        if (panel == null) return;

        displayImage.sprite = sprite;
        panel.SetActive(true);
        isOpen = true;
    }

    public void Hide()
    {
        if (panel == null) return;

        panel.SetActive(false);
        isOpen = false;
    }

    public bool IsOpen => isOpen;
}
