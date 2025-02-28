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
        scrabbleWords = new List<string>();
        if (words != null && words.Count > 0)
        {
            scrabbleWords.AddRange(words);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(true);

            RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipRect.parent as RectTransform,
                Input.mousePosition,
                null,
                out localPoint
            );
            tooltipRect.anchoredPosition = localPoint + new Vector2(50, -20);

            if (scrabbleWords != null && scrabbleWords.Count > 0)
            {
                var colorCodedList = new List<string>();
                foreach (string word in scrabbleWords)
                {
                    if (word.Length >= 5)
                    {
                        colorCodedList.Add($"<color=#1E9B37>{word}</color>");
                    }
                    else
                    {
                        colorCodedList.Add($"<color=white>{word}</color>");
                    }
                }

                tooltipText.text = string.Join("\n", colorCodedList);
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
