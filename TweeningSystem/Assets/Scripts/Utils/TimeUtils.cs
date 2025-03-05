using UnityEngine;

public static class TimeUtils 
{
    public static float GetNormalizedTime(float elapsedTime, float duration)
    {
        return Mathf.Clamp01(elapsedTime / duration);
    }

    public static float MilisecondsToSeconds(float miliseconds)
    {
        return miliseconds / 1000f;
    }
}