namespace Tf2Hud.Common.Util;

public static class IEnumerableExtensions
{
    public static T Random<T>(this T[] array)
    {
        return array[System.Random.Shared.Next(array.Length)];
    }
}
