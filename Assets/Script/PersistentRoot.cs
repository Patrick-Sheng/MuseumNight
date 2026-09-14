using UnityEngine;

public class PersistentRoot : MonoBehaviour
{
    private static PersistentRoot instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}