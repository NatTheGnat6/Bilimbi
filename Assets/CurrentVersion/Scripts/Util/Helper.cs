using System;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public static class Helper {
    public delegate void Event();
    public delegate void Event<T>(T param);
    public static float CubicEase(float alpha)
    {
        return alpha * alpha * alpha;
    }
    public static float CubicEaseOut(float alpha)
    {
        return 1 - (float)Math.Pow(1 - alpha, 3);
    }
    public static int GetRandom(int minimum, int maximum)
    {
        return UnityEngine.Random.Range(minimum, maximum);
    }
    public static float Interpolate(float a, float b, float alpha)
    {
        return a + ((b - a) * alpha);
    }
    public static Vector2 Interpolate(Vector2 a, Vector2 b, float alpha)
    {
        return a + ((b - a) * alpha);
    }
    public static Color AlphaifyColor(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
    public static Color ColorFromHex(String hex, float alpha = 1)
    {
        Color newColor;
        if (ColorUtility.TryParseHtmlString("#" + hex, out newColor))
        {
            newColor.a = alpha;
            return newColor;
        }
        throw new NullReferenceException();
    }
    public static float Approach(float at, float to, float speed)
    {
        if (at > to)
        {
            return Mathf.Max(at - speed, to);
        }
        return Mathf.Min(at + speed, to);
    }
    public static Vector2 Approach(Vector2 at, Vector2 to, float speed)
    {
        Vector2 displacement = to - at;
        if (displacement.magnitude <= speed)
        {
            return to;
        }
        displacement.Normalize();
        return new Vector2(at.x + (displacement.x * speed), at.y + (displacement.y * speed));
    }
    public static string FormatTwoZeros(int number)
    {
        if (number < 0) {
            number = 0;
        }
        return (number < 10 ? "0" : "") + number.ToString();
    }
    public static string FormatTimer(float time)
    {
        int minutes = (int) (time / 60);
        int seconds = (int) Mathf.Ceil(time % 60);
        return FormatTwoZeros(minutes) + ":" + FormatTwoZeros(seconds);
    }
}