using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Collected " + gameObject.name);
        gameObject.SetActive(false);
    }
}
