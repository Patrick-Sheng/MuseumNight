using UnityEngine;

[RequireComponent(typeof(ObjectiveItem))]
public class TorchObjectiveHook : MonoBehaviour
{
    void Awake()
    {
        GetComponent<ObjectiveItem>().OnCollected.AddListener(GrantTorch);
    }

    void GrantTorch()
    {
        GameProgress.HasTorch = true;
    }
}
