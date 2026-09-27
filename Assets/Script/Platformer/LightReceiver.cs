using UnityEngine;

public class LightReceiver : MonoBehaviour
{
    [SerializeField] SpriteRenderer indicator;
    [SerializeField] Color litColor = new Color(1f, 0.95f, 0.3f, 1f);
    [SerializeField] Color unlitColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    [Tooltip("Once lit, permanently marks Room3Platformer's puzzle as solved.")]
    [SerializeField] bool completesRoom3Platformer;

    int litFrame = -10;
    bool solved;

    // Lit this frame/the previous one, or permanently once solved.
    public bool IsLit => solved || Time.frameCount - litFrame <= 1;

    public void Illuminate()
    {
        litFrame = Time.frameCount;
        if (completesRoom3Platformer)
        {
            solved = true;
            GameProgress.CompletedRoom3Platformer = true;
        }
    }

    void Update()
    {
        if (indicator != null)
            indicator.color = IsLit ? litColor : unlitColor;
    }
}
