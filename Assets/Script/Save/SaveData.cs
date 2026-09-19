using System;

[Serializable]
public class SaveData
{
    public string sceneName;
    public float playerX;
    public float playerY;
    public float playerZ;

    public bool hasSanity;
    public float sanityValue;

    public string savedAtUtc;
}
