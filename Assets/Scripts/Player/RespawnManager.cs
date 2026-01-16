using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    private static List<MovingPlatform> platforms = new();

    public static void RegisterPlatform(MovingPlatform platform)
    {
        if (!platforms.Contains(platform))
            platforms.Add(platform);
    }

    public static void UnregisterPlatform(MovingPlatform platform)
    {
        if (platforms.Contains(platform))
            platforms.Remove(platform);
    }

    public static void ResetWorld()
    {
        foreach (var platform in platforms)
        {
            platform.ResetPlatform();
        }
    }
}
