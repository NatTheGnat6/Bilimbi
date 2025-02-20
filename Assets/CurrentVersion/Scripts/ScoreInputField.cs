using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScoreInputField : InputField, ISelectHandler
{
    public event Helper.Event<String> OnSubmitted;
    [Header("UI")]
    private bool textIsEmpty => string.IsNullOrWhiteSpace(text);
    private bool textIsReset = false;
    protected override void Start()
    {
        onValueChanged.AddListener(ClampText); 
    }
    public void TextReset()
    {
        textIsReset = true;
        text = Constants.SCORE_PLACEHOLDER_TEXT;
        textComponent.color = Helper.ColorFromHex("8E8686");
    }
    public void Submit()
    {
        if (!textIsEmpty && !textIsReset)
        {
            OnSubmitted?.Invoke(text);
        }
    }
    public void ClampText(string newText)
    {
        if (newText.Length > Constants.SCORE_NAME_MAX_LENGTH)
        {
            text = newText.Substring(0, Constants.SCORE_NAME_MAX_LENGTH);
        }
    }
    public override void OnSelect(BaseEventData eventData)
    {
        if (textIsReset)
        {
            textIsReset = false;
            text = "";
            textComponent.color = Helper.ColorFromHex("FFFFFF");
        }
        base.OnSelect(eventData);
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        if (textIsEmpty)
        {
            TextReset();
        }
        base.OnDeselect(eventData);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        Submit();
        base.OnSubmit(eventData);
    }
}
