using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [System.Serializable]
    public class State
    {
        public Color fillColor;
        public Color outlineColor;
    }
    private float alpha = 1.0f;
    public State state { get; private set; }
    public char letter { get; private set; }

    private Image fill;
    private Outline outline;
    private TextMeshProUGUI text;

    private void Awake()
    {
        fill = GetComponent<Image>();
        outline = GetComponent<Outline>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetLetter(char letter)
    {
        this.letter = letter;
        text.text = letter.ToString();
    }

    public void SetState(State state)
    {
        this.state = state;
        fill.color = GetAlphaAppliedColor(state.fillColor);
        outline.effectColor = GetAlphaAppliedColor(state.outlineColor);
        text.color = GetAlphaAppliedColor(text.color);
    }

    public void SetAlpha(float alpha)
    {
        this.alpha = alpha;
        SetState(state);
    }

    private Color GetAlphaAppliedColor(Color color) => Helper.AlphaifyColor(color, alpha);

    public static State scrabbleWordState = new State
    { 
        fillColor = Color.green, 
        outlineColor = Color.black 
    };

}
