using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Collected " + gameObject.name);
        GameAudio.Play(GameAudio.Cue.ItemPickup);
        gameObject.SetActive(false);
    }
}
