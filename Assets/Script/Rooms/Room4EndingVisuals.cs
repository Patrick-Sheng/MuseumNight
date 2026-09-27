using System.Collections;
using UnityEngine;

/// <summary>Plays the beam and mirror beats after Room 4 chooses an ending.</summary>
public class Room4EndingVisuals : MonoBehaviour
{
    [SerializeField] private SpriteRenderer medusa;
    [SerializeField] private Sprite emptyMirror;
    [SerializeField] private Sprite mirrorOne;
    [SerializeField] private Sprite mirrorTwo;
    [SerializeField] private Sprite mirrorThree;
    [SerializeField] private Sprite beamSprite;
    [SerializeField] private Sprite medusaStone;
    [SerializeField] private Sprite playerStone;

    [Header("Framing")]
    [SerializeField] private Vector2 medusaBeamOffset = new(-0.8f, 0.4f);
    [SerializeField] private Vector2 playerHitOffset = new(0f, 0.5f);
    [SerializeField, Range(0f, 1f)] private float mirrorDistanceFromPlayer = 0.28f;
    [SerializeField] private float mirrorScale = 0.8f;
    [SerializeField] private float beamHeight = 0.55f;

    [Header("Stone position offsets")]
    [SerializeField] private Vector2 playerStoneOffset;
    [SerializeField] private Vector2 medusaStoneOffset;

    [Header("Timing")]
    [SerializeField] private float beamTravelTime = 0.55f;
    [SerializeField] private float mirrorStageTime = 0.45f;
    [SerializeField] private float failedMirrorHold = 0.5f;

    private SpriteRenderer playerRenderer;
    private Animator playerAnimator;
    private Sprite originalPlayerSprite;
    private Vector3 originalPlayerScale;
    private Vector3 originalPlayerPosition;
    private bool playerTurnedToStone;
    private SpriteRenderer mirrorRenderer;
    private SpriteRenderer incomingBeam;
    private SpriteRenderer reflectedBeam;

    public void Play(int shardCount, System.Action onFinished)
    {
        if (medusa == null || emptyMirror == null || mirrorOne == null || mirrorTwo == null ||
            mirrorThree == null || beamSprite == null || medusaStone == null || playerStone == null)
        {
            Debug.LogError("Room4EndingVisuals is missing a sprite or Medusa reference.", this);
            onFinished?.Invoke();
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || !player.TryGetComponent(out playerRenderer))
        {
            Debug.LogError("Room4EndingVisuals needs the persistent Player SpriteRenderer.", this);
            onFinished?.Invoke();
            return;
        }

        playerAnimator = player.GetComponent<Animator>();
        originalPlayerSprite = playerRenderer.sprite;
        originalPlayerScale = playerRenderer.transform.localScale;
        StartCoroutine(PlaySequence(Mathf.Clamp(shardCount, 0, 3), onFinished));
    }

    private IEnumerator PlaySequence(int shardCount, System.Action onFinished)
    {
        Vector3 playerHit = playerRenderer.transform.position + (Vector3)playerHitOffset;
        Vector3 medusaOrigin = medusa.transform.position + (Vector3)medusaBeamOffset;
        Vector3 mirrorPosition = Vector3.Lerp(playerHit, medusaOrigin, mirrorDistanceFromPlayer);
        mirrorPosition.z = playerRenderer.transform.position.z;

        incomingBeam = CreateVisual("MedusaBeam", beamSprite, 8);
        yield return GrowBeam(incomingBeam, medusaOrigin, medusaOrigin, mirrorPosition, beamTravelTime);

        mirrorRenderer = CreateVisual("CutsceneMirror", emptyMirror, 9);
        mirrorRenderer.transform.position = mirrorPosition;
        mirrorRenderer.transform.localScale = Vector3.one * mirrorScale;

        if (shardCount >= 1)
        {
            yield return new WaitForSeconds(mirrorStageTime);
            mirrorRenderer.sprite = mirrorOne;
        }
        if (shardCount >= 2)
        {
            yield return new WaitForSeconds(mirrorStageTime);
            mirrorRenderer.sprite = mirrorTwo;
        }
        if (shardCount == 3)
        {
            yield return new WaitForSeconds(mirrorStageTime);
            mirrorRenderer.sprite = mirrorThree;

            reflectedBeam = CreateVisual("ReflectedBeam", beamSprite, 8);
            yield return GrowBeam(reflectedBeam, mirrorPosition, mirrorPosition, medusaOrigin, beamTravelTime);
            TurnMedusaToStone();
        }
        else
        {
            yield return new WaitForSeconds(failedMirrorHold);
            mirrorRenderer.enabled = false;
            yield return GrowBeam(incomingBeam, medusaOrigin, mirrorPosition, playerHit, beamTravelTime);
            TurnPlayerToStone();
        }

        onFinished?.Invoke();
    }

    private IEnumerator GrowBeam(SpriteRenderer visual, Vector3 origin, Vector3 startEnd,
        Vector3 finalEnd, float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 end = Vector3.Lerp(startEnd, finalEnd, Mathf.Clamp01(elapsed / duration));
            PlaceBeam(visual, origin, end);
            yield return null;
        }
        PlaceBeam(visual, origin, finalEnd);
    }

    private void PlaceBeam(SpriteRenderer visual, Vector3 origin, Vector3 end)
    {
        Vector2 delta = end - origin;
        visual.transform.position = (origin + end) * 0.5f;
        visual.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        visual.transform.localScale = new Vector3(
            Mathf.Max(0.01f, delta.magnitude / beamSprite.bounds.size.x),
            beamHeight / beamSprite.bounds.size.y, 1f);
    }

    private SpriteRenderer CreateVisual(string name, Sprite sprite, int sortingOrder)
    {
        GameObject visual = new GameObject(name);
        visual.transform.SetParent(transform);
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.flipX = sprite == beamSprite; // The beam's bright tip faces its travel direction.
        renderer.sortingLayerID = medusa.sortingLayerID;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private void TurnMedusaToStone()
    {
        SetStoneSpriteAtSameHeight(medusa, medusaStone);
        medusa.transform.position += (Vector3)medusaStoneOffset;
    }

    private void TurnPlayerToStone()
    {
        if (playerAnimator != null) playerAnimator.enabled = false;
        originalPlayerPosition = playerRenderer.transform.position;
        SetStoneSpriteAtSameHeight(playerRenderer, playerStone);
        playerRenderer.transform.position += (Vector3)playerStoneOffset;
        playerTurnedToStone = true;
    }

    private static void SetStoneSpriteAtSameHeight(SpriteRenderer renderer, Sprite stoneSprite)
    {
        float heightRatio = renderer.sprite.bounds.size.y / stoneSprite.bounds.size.y;
        renderer.sprite = stoneSprite;
        Vector3 scale = renderer.transform.localScale;
        renderer.transform.localScale = new Vector3(scale.x * heightRatio, scale.y * heightRatio, scale.z);
    }

    private void OnDisable()
    {
        if (playerTurnedToStone && playerRenderer != null)
        {
            playerRenderer.sprite = originalPlayerSprite;
            playerRenderer.transform.localScale = originalPlayerScale;
            playerRenderer.transform.position = originalPlayerPosition;
            if (playerAnimator != null) playerAnimator.enabled = true;
            playerTurnedToStone = false;
        }
    }
}
