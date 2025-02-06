using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileVariantOG : MonoBehaviour
{
    [System.Serializable]
    public class StateVariant
    {
        public Color fillColor;
        public Color outlineColor;
    }
    private float alphaVariant = 1.0f;
    public StateVariant stateVariant { get; private set; }
    public char letterVariant { get; private set; }

    private Image fillVariant;
    private Outline outlineVariant;
    private TextMeshProUGUI textVariant;

    private void Awake()
    {
        fillVariant = GetComponent<Image>();
        outlineVariant = GetComponent<Outline>();
        textVariant = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetLetterVariant(char letter)
    {
        this.letterVariant = letter;
        textVariant.text = letter.ToString();
    }

    public void SetStateVariant(StateVariant state)
    {
        this.stateVariant = state;
        fillVariant.color = GetAlphaAppliedColorVariant(state.fillColor);
        outlineVariant.effectColor = GetAlphaAppliedColorVariant(state.outlineColor);
        textVariant.color = GetAlphaAppliedColorVariant(textVariant.color);
    }

    public void SetAlphaVariant(float alpha)
    {
        this.alphaVariant = alpha;
        SetStateVariant(stateVariant);
    }

    private Color GetAlphaAppliedColorVariant(Color color) => Helper.AlphaifyColor(color, alphaVariant);
}