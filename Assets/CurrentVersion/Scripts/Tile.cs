using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [System.Serializable]
    public enum State
    {
        Empty,
        Occupied,
        Correct,
        WrongSpot,
        Incorrect,
        Locked,
        ValidScrabbleWord
    }
    public Pallette pallette;
    [System.Serializable]
    public class StateColor {
        public Color fillColor;
        public Color outlineColor;
    }
    public Row row;
    private float alpha = 1.0f;
    public State state { get; private set; }
    public char letter { get; private set; }
    private Image fill;
    private Outline outline;
    private TextMeshProUGUI text;
    private Dictionary<State, StateColor> stateToStateColor;

    private void Awake()
    {
        fill = GetComponent<Image>();
        outline = GetComponent<Outline>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        stateToStateColor = new Dictionary<State, StateColor>() {
            {State.Empty, pallette.emptyState},
            {State.Occupied, pallette.occupidedState},
            {State.Correct, pallette.correctState},
            {State.WrongSpot, pallette.wrongSpotState},
            {State.Incorrect, pallette.incorrectState},
            {State.Locked, pallette.lockedState},
            {State.ValidScrabbleWord, pallette.correctState}
        };
        SetState(State.Empty);
    }
    private StateColor GetStateColor(State state)
    {
        return stateToStateColor[state];
    }

    public void SetLetter(char letter)
    {
        this.letter = letter;
        text.text = letter.ToString();
    }

    public void SetState(State state)
    {
        this.state = state;
        StateColor stateColor = GetStateColor(state);
        fill.color = GetAlphaAppliedColor(stateColor.fillColor);
        outline.effectColor = GetAlphaAppliedColor(stateColor.outlineColor);
        text.color = GetAlphaAppliedColor(text.color);
    }

    public void SetSwapAlpha(float alpha)
    {
        alpha = Math.Clamp(alpha, 0, 1);
        transform.localScale = new Vector3(
            transform.localScale.x, (float) Math.Abs(Math.Pow((2 * alpha) - 1, 2)), transform.localScale.z
        );
    }

    public void SetAlpha(float alpha)
    {
        this.alpha = alpha;
        SetState(state);
    }

    private Color GetAlphaAppliedColor(Color color) => Helper.AlphaifyColor(color, alpha);
}
