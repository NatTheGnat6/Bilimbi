using UnityEngine;

public static class Helper {
    public static float CubicEase(float alpha) {
        return alpha * alpha * alpha;
    }
    public static float Interpolate(float a, float b, float alpha) {
        return a + ((b - a) * alpha);
    }
    public static Color AlphaifyColor(Color color, float alpha) {
        return new Color(color.r, color.g, color.b, alpha);
    }
}