using System;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public static class Helper {
    public delegate void Event();
    public static float CubicEase(float alpha) {
        return alpha * alpha * alpha;
    }
    public static float CubicEaseOut(float alpha) {
        return 1 - (float)(Math.Pow(1 - alpha, 3));
    }
    public static float Interpolate(float a, float b, float alpha) {
        return a + ((b - a) * alpha);
    }
    public static Color AlphaifyColor(Color color, float alpha) {
        return new Color(color.r, color.g, color.b, alpha);
    }
    public static Color ColorFromHex(String hex, float alpha) {
        Color newColor;
        if (ColorUtility.TryParseHtmlString("#" + hex, out newColor)) {
            newColor.a = alpha;
            return newColor;
        }
        throw new NullReferenceException();
    }
    public static Color ColorFromHex(String hex) => ColorFromHex(hex, 1);
    public static float Approach(float at, float to, float speed) {
        return to;
    }
}