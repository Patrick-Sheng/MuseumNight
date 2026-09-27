using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DoorStateByTransition : MonoBehaviour
{
    [SerializeField] private TransitionTrigger transitionTrigger;
    [SerializeField] private Sprite closedDoorSprite;
    [SerializeField] private Sprite openDoorSprite;

    private SpriteRenderer doorRenderer;
    private bool previousAvailableState;
    private bool hasState;

    private void Awake()
    {
        doorRenderer = GetComponent<SpriteRenderer>();
        RefreshDoorSprite(true);
    }

    private void Update()
    {
        RefreshDoorSprite(false);
    }

    private void RefreshDoorSprite(bool force)
    {
        if (doorRenderer == null || transitionTrigger == null)
            return;

        bool available = transitionTrigger.IsTransitionAvailable();
        if (!force && hasState && available == previousAvailableState)
            return;

        previousAvailableState = available;
        hasState = true;

        Sprite nextSprite = available ? openDoorSprite : closedDoorSprite;
        if (nextSprite != null)
            doorRenderer.sprite = nextSprite;
    }
}