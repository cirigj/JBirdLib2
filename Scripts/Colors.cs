using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace JBirdLib.Colors
{

    /// <summary>
    /// A color that uses HSV instead of RGB. Can implicitly cast to UnityEngine.Color.
    /// </summary>
    [Serializable]
    public class ColorHSV
    {
        public int h;
        public float s;
        public float v;
        public float a;

        public ColorHSV() {
            h = 0;
            s = 0;
            v = 0;
            a = 0;
        }

        public ColorHSV(ColorHSV cHSV) {
            h = cHSV.h;
            s = cHSV.s;
            v = cHSV.v;
            a = cHSV.a;
        }

        public ColorHSV(int hue, float saturation, float value, float alpha) {
            h = (int)new Angle(hue);
            s = Mathf.Clamp01(saturation);
            v = Mathf.Clamp01(value);
            a = Mathf.Clamp01(alpha);
        }

        public ColorHSV(Color color) {
            h = color.GetHue(out float cMax, out float delta);
            s = cMax == 0f ? 0f : delta / cMax;
            v = cMax;
            a = color.a;
        }

        public static implicit operator Color(ColorHSV hsv) {
            return hsv.ToColor();
        }

        public static implicit operator ColorHSV(Color rgb) {
            return new ColorHSV(rgb);
        }

    }

    /// <summary>
    /// Helper class to convert other color formats to Unity-recognized colors.
    /// </summary>
    public static class ColorHelper
    {

        /// <summary>
        /// Converts a Color to the ColorHSV class.
        /// </summary>
        public static ColorHSV ToHSV(this Color color) {
            return color;
        }

        /// <summary>
        /// Get the hue from an RGB color without converting to HSV.
        /// </summary>
        public static int GetHue(this Color color) {
            return color.GetHue(out _, out _);
        }

        /// <summary>
        /// Get the hue from an RGB color without converting to HSV.
        /// </summary>
        public static int GetHue(this Color color, out float cMax, out float delta) {
            cMax = Mathf.Max(color.r, color.g, color.b);
            float cMin = Mathf.Min(color.r, color.g, color.b);
            delta = cMax - cMin;
            float diff = cMax == color.r ? color.g - color.b : (cMax == color.g ? color.b - color.r : color.r - color.g);
            float offset = cMax == color.r ? 0 : (cMax == color.g ? 2 : 4);
            float hue = delta == 0f ? 0f : offset + diff / delta;
            return ((int)(hue * 60f)).AbsMod(360);
        }

        public static Color ToColor(this ColorHSV colorHSV) {
            float c = colorHSV.v * colorHSV.s;
            float x = c * (1f - Mathf.Abs((colorHSV.h / 60f) % 2 - 1));
            float m = colorHSV.v - c;
            int hueSlice = colorHSV.h / 60;
            int rMod = Mathf.RoundToInt(Mathf.Abs(hueSlice - 2.5f) * 2f);
            float r = m + (rMod == 5 ? c : (rMod == 3 ? x : 0f));
            int gMod = Mathf.RoundToInt(Mathf.Abs(hueSlice - 1.5f) * 2f);
            float g = m + (gMod == 1 ? c : (gMod == 3 ? x : 0f));
            int bMod = Mathf.RoundToInt(Mathf.Abs(hueSlice - 3.5f) * 2f);
            float b = m + (bMod == 1 ? c : (bMod == 3 ? x : 0f));
            return new Color(r, g, b, colorHSV.a);
        }

        public static Color ToColor(this int HexValue) {
            byte r = (byte)((HexValue >> 16) & 0xFF);
            byte g = (byte)((HexValue >> 8) & 0xFF);
            byte b = (byte)((HexValue) & 0xFF);
            return ToColor(new Color32(r, g, b, 255));
        }

        public static string GetHexCode(this Color color) {
            return string.Format("#{0}{1}{2}",
                Mathf.RoundToInt(color.r * 255).ToString("X2"),
                Mathf.RoundToInt(color.g * 255).ToString("X2"),
                Mathf.RoundToInt(color.b * 255).ToString("X2")
            );
        }

        public static Color ToColor(this Color32 color32) {
            return (Color)color32;
        }

        /// <summary>
        /// Create a Color using red, green, and blue values from 0 to 255.
        /// </summary>
        public static Color From255to1(int red, int green, int blue) {
            return new Color(red / 255f, green / 255f, blue / 255f);
        }

        /// <summary>
        /// A container class for holding a color and an amount (for mixing).
        /// </summary>
        [Serializable]
        public class ColorAmount
        {

            public Color color;
            [Range(0f,1f)]
            public float amount;

            public ColorAmount(Color color, float amount) {
                this.color = color;
                this.amount = Mathf.Clamp01(amount);
            }

            public ColorAmount(ColorAmount colorAmount) {
                color = colorAmount.color;
                amount = colorAmount.amount;
            }

        }

        /// <summary>
        /// A container class for holding a color (in HSV) and an amount (for mixing).
        /// </summary>
        [Serializable]
        public class ColorHSVAmount
        {

            public ColorHSV colorHSV;
            public float amount;

            public ColorHSVAmount(ColorHSV colorHSV, float amount) {
                this.colorHSV = colorHSV;
                this.amount = amount;
            }

            public ColorHSVAmount(ColorHSVAmount cHSVA) {
                colorHSV = cHSVA.colorHSV;
                amount = cHSVA.amount;
            }

        }

        /// <summary>
        /// Mixes the specified colors in their respective quantities using RGB.
        /// </summary>
        public static Color MixColors(params ColorAmount[] colors) {
            var composite = colors.Aggregate(new { r = 0f, g = 0f, b = 0f, a = 0f }, (comp, cA) => {
                return new {
                    r = comp.r + cA.color.r * cA.amount,
                    g = comp.g + cA.color.g * cA.amount,
                    b = comp.b + cA.color.b * cA.amount,
                    a = comp.a + cA.color.a * cA.amount
                };
            });
            return new Color(composite.r, composite.g, composite.b, composite.a);
        }

        /// <summary>
        /// Mixes the specified colors in equal quantities using RGB.
        /// </summary>
        public static Color MixColors(params Color[] colors) {
            return MixColors(colors.Select(c => new ColorAmount(c, 1f / colors.Length)).ToArray());
        }

        /// <summary>
        /// Mixes the specified HSV colors in their respective quantities using HSV.
        /// </summary>
        public static Color MixColorsHSV(params ColorHSVAmount[] colorsHSV) {
            var composite = colorsHSV.Aggregate(new { h = 0f, s = 0f, v = 0f, a = 0f }, (comp, cA) => {
                return new {
                    h = comp.h + cA.colorHSV.h * cA.amount,
                    s = comp.s + cA.colorHSV.s * cA.amount,
                    v = comp.v + cA.colorHSV.v * cA.amount,
                    a = comp.a + cA.colorHSV.a * cA.amount
                };
            });
            return new ColorHSV(Mathf.RoundToInt(composite.h), composite.s, composite.v, composite.a);
        }

        /// <summary>
        /// Mixes the specified colors in their respective quantities using HSV.
        /// </summary>
        public static Color MixColorsHSV(params ColorAmount[] colors) {
            return MixColorsHSV(colors.Select(c => new ColorHSVAmount(c.color, c.amount)).ToArray());
        }

        /// <summary>
        /// Mixes the specified HSV colors in their respective quantities using HSV.
        /// </summary>
        public static Color MixColorsHSV(params ColorHSV[] colorsHSV) {
            return MixColorsHSV(colorsHSV.Select(c => new ColorHSVAmount(c, 1f / colorsHSV.Length)).ToArray());
        }

        /// <summary>
        /// Mixes the specified colors in their respective quantities using HSV.
        /// </summary>
        public static Color MixColorsHSV(params Color[] colors) {
            return MixColorsHSV(colors.Select(c => new ColorHSVAmount(c, 1f / colors.Length)).ToArray());
        }

        /// <summary>
        /// Shifts the hue by the specified number of degrees.
        /// </summary>
        public static ColorHSV ShiftHue(this ColorHSV startColor, float degrees) {
            startColor.h = (startColor.h + Mathf.RoundToInt(degrees)).AbsMod(360);
            return startColor;
        }

        #region TODO

        /// <summary>
        /// Make a gradient between two colors (Returns a list of colors blended from startColor to endColor).
        /// </summary>
        /// <returns>A list of colors blended from startColor to endColor.</returns>
        /// <param name="startColor">Start color.</param>
        /// <param name="endColor">End color.</param>
        /// <param name="blendColors">Number of blended colors in the middle (returned list's length will be this value plus two).</param>
        public static List<Color> MakeGradient(Color startColor, Color endColor, int blendColors) {
            List<Color> gradient = new List<Color>();
            float amountPerBlendStep = 1f / (float)(blendColors + 1);
            float blendAmount = 0f;
            gradient.Add(startColor);
            for (int i = 0; i < blendColors; i++) {
                blendAmount += amountPerBlendStep;
                gradient.Add(MixColors(new ColorAmount(startColor, 1f - blendAmount), new ColorAmount(endColor, blendAmount)));
            }
            gradient.Add(endColor);
            return gradient;
        }

        /// <summary>
        /// Make a gradient between two colors using HSV (Returns a list of colors blended from startColor to endColor).
        /// </summary>
        /// <returns>A list of colors blended from startColor to endColor.</returns>
        /// <param name="startColor">Start color.</param>
        /// <param name="endColor">End color.</param>
        /// <param name="blendColors>Number of blended colors in the middle (returned list's length will be this value plus two).</param>
        public static List<Color> MakeGradientHSV(Color startColor, Color endColor, int blendColors) {
            List<Color> gradient = new List<Color>();
            float startHue = startColor.ToHSV().h;
            float endHue = endColor.ToHSV().h;
            float startSat = startColor.ToHSV().s;
            float endSat = endColor.ToHSV().s;
            float startVal = startColor.ToHSV().v;
            float endVal = endColor.ToHSV().v;
            float degreesPerStep = (endHue - startHue);
            if (degreesPerStep > 180f) {
                degreesPerStep = degreesPerStep - 360f;
            }
            if (degreesPerStep < -180f) {
                degreesPerStep = degreesPerStep + 360f;
            }
            float saturationPerStep = (endSat - startSat);
            float valuePerStep = (endVal - startVal);
            degreesPerStep /= (float)(blendColors + 1);
            saturationPerStep /= (float)(blendColors + 1);
            valuePerStep /= (float)(blendColors + 1);
            gradient.Add(startColor);
            ColorHSV colorHSV = startColor.ToHSV();
            for (int i = 0; i < blendColors; i++) {
                colorHSV.ShiftHue(degreesPerStep);
                colorHSV.s += saturationPerStep;
                colorHSV.v += valuePerStep;
                gradient.Add(colorHSV.ToColor());
            }
            gradient.Add(endColor);
            return gradient;
        }
        /// <summary>
        /// Make a rainbow from the start color, shifting the hue a specified number of degrees each step (Returns a list of Colors).
        /// </summary>
        /// <param name="startColor">Start color.</param>
        /// <param name="degreesPerStep">Degrees to shift the hue per step.</param>
        /// <param name="length">Desired length of the list.</param>
        public static List<Color> Rainbowify(Color startColor, float degreesPerStep, int length) {
            List<Color> rainbow = new List<Color>();
            rainbow.Add(startColor);
            ColorHSV colorHSV = startColor.ToHSV();
            for (int i = 0; i < length - 1; i++) {
                colorHSV.ShiftHue(degreesPerStep);
                rainbow.Add(colorHSV.ToColor());
            }
            return rainbow;
        }
        /// <summary>
        /// Make a rainbow from the start color, shifting the hue until it reaches the end hue. (Returns a list of Colors).
        /// </summary>
        /// <param name="startColor">Start color.</param>
        /// <param name="endColor">Hue to stop shifting at (will only use the hue of this color, not value or saturation).</param>
        /// <param name="length">Desired length of the list.</param>
        public static List<Color> Rainbowify(Color startColor, Color endColor, int length) {
            List<Color> rainbow = new List<Color>();
            float startHue = startColor.ToHSV().h;
            float endHue = endColor.ToHSV().h;
            if (endHue < startHue) {
                endHue += 360f;
            }
            float degreesPerStep = (endHue - startHue);
            degreesPerStep /= (float)length - 1;
            rainbow.Add(startColor);
            ColorHSV colorHSV = startColor.ToHSV();
            for (int i = 0; i < length - 1; i++) {
                colorHSV.ShiftHue(degreesPerStep);
                rainbow.Add(colorHSV.ToColor());
            }
            return rainbow;
        }
        /// <summary>
        /// Chroma class (encapsulates hue and saturation).
        /// </summary>
        public class Chroma
        {
            public int hue;
            public float saturation;
            public Chroma() {
                hue = 0;
                saturation = 0;
            }
            public Chroma(int h, float s) {
                hue = h;
                saturation = s;
            }
        }
        /// <summary>
        /// Gets the luminance of a given color.
        /// </summary>
        /// <returns>Luminance of the given color.</returns>
        /// <param name="color">Color.</param>
        /// <param name="useBT709">Uses BT.709 instead of BT.601 if set to true.</param>
        public static float GetLuma(this Color color, bool useBT709 = false) {
            float rVal = 0.299f;
            float gVal = 0.587f;
            float bVal = 0.114f;
            if (useBT709) { //using BT.709 instead of BT.601
                rVal = 0.2126f;
                gVal = 0.7152f;
                bVal = 0.0722f;
            }
            return color.r * rVal + color.g * gVal + color.b * bVal;
        }
        /// <summary>
        /// Returns a color with the specified chroma and luma
        /// </summary>
        /// <returns>Color with the specified chroma and luma.</returns>
        /// <param name="chroma">Chroma.</param>
        /// <param name="luma">Luma.</param>
        /// <param name="useBT709">Uses BT.709 instead of BT.601 if set to true.</param>
        public static Color FromChromaAndLuma(Chroma chroma, float luma, bool useBT709 = false) {
            return FromChromaAndLuma(chroma.hue, chroma.saturation, luma, useBT709);
        }

        #endregion

        /// <summary>
        /// Returns a color with the specified hue, saturation, and luma. Defaults to BT.601.
        /// </summary>
        public static Color FromChromaAndLuma(int hue, float saturation, float luma, bool useBT709 = false) {
            float rVal = useBT709 ? 0.2126f : 0.299f;
            float gVal = useBT709 ? 0.7152f : 0.587f;
            float bVal = useBT709 ? 0.0722f : 0.114f;
            float r = 0f;
            float g = 0f;
            float b = 0f;
            hue = hue.AbsMod(360);
            luma = Mathf.Clamp(luma, 0f, 1f);
            if (hue % 60 == 0) {
                float xVal = 0f;
                xVal += (hue + 60).AbsMod(360) <= 120 ? rVal : 0f;
                xVal += (hue - 60).AbsMod(360) <= 120 ? gVal : 0f;
                xVal += hue.AbsMod(360) >= 180 ? bVal : 0f;
                r = (hue + 60).AbsMod(360) <= 120 ? (luma > xVal ? 1f : luma / xVal) : (luma > xVal ? (luma - xVal) / (1 - xVal) : 0f);
                g = (hue - 60).AbsMod(360) <= 120 ? (luma > xVal ? 1f : luma / xVal) : (luma > xVal ? (luma - xVal) / (1 - xVal) : 0f);
                b = hue.AbsMod(360) >= 180 ? (luma > xVal ? 1f : luma / xVal) : (luma > xVal ? (luma - xVal) / (1 - xVal) : 0f);
            }
            else {
                float fHue = hue;
                if (hue > 0 && hue < 60) { //between red and yellow
                    float huePercentage = (60f / (fHue - 0));
                    float xVal = (rVal * huePercentage + gVal);
                    float x = luma / xVal;
                    r = r > 1f ? 1f : x * huePercentage;
                    g = r > 1f ? (1f / huePercentage) + (((luma - rVal - gVal * g) / (gVal * (1f - g) + bVal)) * (1f - (1f / huePercentage))) : x;
                    b = r > 1f ? (luma - rVal - gVal * g) / (gVal * (1f - g) + bVal) : 0f;
                }
                else if (hue > 60 && hue < 120) { //between yellow and green
                    float huePercentage = (60f / (120 - fHue));
                    float xVal = (gVal * huePercentage + rVal);
                    float x = luma / xVal;
                    g = g > 1f ? 1f : x * huePercentage;
                    r = g > 1f ? (1f / huePercentage) + (((luma - gVal - rVal * r) / (rVal * (1f - r) + bVal)) * (1f - (1f / huePercentage))) : x;
                    b = g > 1f ? (luma - gVal - rVal * r) / (rVal * (1f - r) + bVal) : 0f;
                }
                else if (hue > 120 && hue < 180) { //between green and cyan
                    float huePercentage = (60f / (fHue - 120));
                    float xVal = (gVal * huePercentage + bVal);
                    float x = luma / xVal;
                    g = g > 1f ? 1f : x * huePercentage;
                    b = g > 1f ? (1f / huePercentage) + (((luma - gVal - bVal * b) / (bVal * (1f - b) + rVal)) * (1f - (1f / huePercentage))) : x;
                    r = g > 1f ? (luma - gVal - bVal * b) / (bVal * (1f - b) + rVal) : 0f;
                }
                else if (hue > 180 && hue < 240) { //between cyan and blue
                    float huePercentage = (60f / (240 - fHue));
                    float xVal = (bVal * huePercentage + gVal);
                    float x = luma / xVal;
                    b = b > 1f ? 1f : x * huePercentage;
                    g = b > 1f ? (1f / huePercentage) + (((luma - bVal - gVal * g) / (gVal * (1f - g) + rVal)) * (1f - (1f / huePercentage))) : x;
                    r = b > 1f ? (luma - bVal - gVal * g) / (gVal * (1f - g) + rVal) : 0f;
                }
                else if (hue > 240 && hue < 300) { //between blue and magenta
                    float huePercentage = (60f / (fHue - 240));
                    float xVal = (bVal * huePercentage + rVal);
                    float x = luma / xVal;
                    b = b > 1f ? 1f : x * huePercentage;
                    r = b > 1f ? (1f / huePercentage) + (((luma - bVal - rVal * r) / (rVal * (1f - r) + gVal)) * (1f - (1f / huePercentage))) : x;
                    g = b > 1f ? (luma - bVal - rVal * r) / (rVal * (1f - r) + gVal) : 0f;
                }
                else if (hue > 300 && hue < 360) { //between magenta and red
                    float huePercentage = (60f / (360 - fHue));
                    float xVal = (rVal * huePercentage + bVal);
                    float x = luma / xVal;
                    r = r > 1f ? 1f : x * huePercentage;
                    b = r > 1f ? (1f / huePercentage) + (((luma - rVal - bVal * b) / (bVal * (1f - b) + gVal)) * (1f - (1f / huePercentage))) : x;
                    g = r > 1f ? (luma - rVal - bVal * b) / (bVal * (1f - b) + gVal) : 0f;
                }
            }
            return Color.Lerp(new Color(luma, luma, luma), new Color(r, g, b), saturation);
        }
    }
}