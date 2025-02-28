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
    private List<string> scrabbleWords;
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
    public void SetScrabbleWords(List<string> words)
    {
        scrabbleWords = new List<string>(words);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null) // remove '&& board != null'
        {
            tooltip.SetActive(true);

            // Use the local scrabbleWords field, not board.GetEnteredScrabbleWords()
            if (scrabbleWords != null && scrabbleWords.Count > 0)
            {
                tooltipText.text = string.Join("\n", scrabbleWords);
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
