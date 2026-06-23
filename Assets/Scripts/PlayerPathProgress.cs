using UnityEngine;

public static class PlayerPathProgress
{
    private const string NodeIndexKey = "PathNodeIndex";
    private const string CurrentNodeHPKey = "PathNodeHP";
    private const string PendingPowerKey = "PathPendingPower";

    public static int NodeIndex
    {
        get { return PlayerPrefs.GetInt(NodeIndexKey, 0); }
        set { PlayerPrefs.SetInt(NodeIndexKey, value); }
    }

    public static int CurrentNodeHP
    {
        get { return PlayerPrefs.GetInt(CurrentNodeHPKey, -1); }
        set { PlayerPrefs.SetInt(CurrentNodeHPKey, value); }
    }

    public static int TakePendingPower()
    {
        int power = PlayerPrefs.GetInt(PendingPowerKey, 0);
        PlayerPrefs.SetInt(PendingPowerKey, 0);
        return power;
    }

    public static void AddPendingPower(int amount)
    {
        int current = PlayerPrefs.GetInt(PendingPowerKey, 0);
        PlayerPrefs.SetInt(PendingPowerKey, current + amount);
    }
}
