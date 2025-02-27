using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ScoreItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text placeText;
    public TMP_Text scoreText;
    public TMP_Text nameText;
    public TMP_Text wordText;
    public GameObject tooltip;
    public TMP_Text tooltipText;
    public Board board;
    private int score;
    public void SetName(string name)
    {
        this.name = name;
        nameText.text = name;
    }
    public string GetName() => name;
    public void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
    }
    public int GetScore() => score;
    public void SetWord(string word)
    {
        wordText.text = word.ToUpper();
    }
    public void SetPlace(int place)
    {
        placeText.text = "#" + place.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null && board != null)
        {
            tooltip.SetActive(true);

            List<string> submittedWords = board.GetEnteredScrabbleWords();
            if (submittedWords.Count > 0)
            {
                tooltipText.text = string.Join("\n", submittedWords);
            }
            else
            {
                tooltipText.text = "No additional words were entered.";
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false);
        }
    }
}
