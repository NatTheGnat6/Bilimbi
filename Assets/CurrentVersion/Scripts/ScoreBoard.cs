using UnityEngine;
using TMPro;
using System.Collections.Generic;

[DefaultExecutionOrder(-1)]
public class ScoreBoard : MonoBehaviour
{
    public event Helper.Event OnPromptFinished;

    [Header("UI / References")]
    public GameObject promptCanvas;
    public TMP_Text promptScoreText;
    public ScoreInputField promptField;
    public ScoreItem scoreItemPrefab;

    private string savingName;
    private int savingScore;
    private string savingWord;

    private List<string> savingScrabbleWords;

    private ScoreItem[] scoreItems;

    public void PromptScoreSave(string[,] scoreMatrix)
    {
        savingScore = int.Parse(scoreMatrix[0, 1]);
        savingWord = scoreMatrix[0, 0];
        promptScoreText.text = Constants.SCORE_DISPLAY_PREFIX + savingScore.ToString();
        promptField.TextReset();
        promptField.OnSubmitted += SubmitSavePrompt;
        promptCanvas.SetActive(true);

        savingScrabbleWords = new List<string>();
    }

    public void PromptScoreSave(string[,] scoreMatrix, List<string> scrabbleWords)
    {
        savingScore = int.Parse(scoreMatrix[0, 1]);
        savingWord = scoreMatrix[0, 0];
        
        promptScoreText.text = Constants.SCORE_DISPLAY_PREFIX + savingScore.ToString();
        promptField.TextReset();
        promptField.OnSubmitted += SubmitSavePrompt;
        promptCanvas.SetActive(true);

        savingScrabbleWords = new List<string>(scrabbleWords);
    }


    public void CloseSavePrompt()
    {
        promptCanvas.SetActive(false);
    }

    private void SubmitSavePrompt(string submittedName)
    {
        savingName = submittedName;
        SaveScore();
        CloseSavePrompt();
        OnPromptFinished?.Invoke();
        promptField.OnSubmitted -= SubmitSavePrompt;
    }

    private ScoreItem[] OrderByScore(ScoreItem[] items)
    {
        ScoreItem[] newItems = new ScoreItem[items.Length];
        for (int i = 0; i < newItems.Length; i++)
        {
            ScoreItem adjustItem = items[i];
            int adjustScore = adjustItem.GetScore();
            int order = 0;
            for (int j = 0; j < newItems.Length; j++)
            {
                ScoreItem againstItem = items[j];
                int againstScore = againstItem.GetScore();
                if (!adjustItem.Equals(againstItem) && (
                    adjustScore < againstScore ||  (adjustScore == againstScore && i > j)
                ))
                {
                    order++;
                }
            }
            newItems[order] = adjustItem;
            adjustItem.SetPlace(order + 1);
        }
        return newItems;
    }


    public void CreateScoreItem(string name, int score, string word)
    {
        int totalScoreItems = scoreItems != null ? scoreItems.Length + 1 : 1;
        ScoreItem[] newScoreItems = new ScoreItem[totalScoreItems];
        if (scoreItems != null) {
            for (int i = 0; i < scoreItems.Length; i++)
            {
                newScoreItems[i] = scoreItems[i];
            }
        }
        ScoreItem scoreItem = Instantiate(scoreItemPrefab);
        scoreItem.SetName(name);
        scoreItem.SetScore(score);
        scoreItem.SetWord(word);

        newScoreItems[totalScoreItems - 1] = scoreItem;
        scoreItems = OrderByScore(newScoreItems);
        for (int i = 0; i < scoreItems.Length; i++)
        {
            ScoreItem item = scoreItems[i];
            item.transform.SetParent(null);
            item.transform.SetParent(transform);
        }
        scoreItem.transform.localScale = new Vector3(1, 1, 1);
    }

    private void CreateScoreItem(string name, int score, string word, List<string> scrabbleWords)
    {
        int totalScoreItems = scoreItems != null ? scoreItems.Length + 1 : 1;
        ScoreItem[] newScoreItems = new ScoreItem[totalScoreItems];
        if (scoreItems != null) {
            for (int i = 0; i < scoreItems.Length; i++)
            {
                newScoreItems[i] = scoreItems[i];
            }
        }
        ScoreItem scoreItem = Instantiate(scoreItemPrefab);
        scoreItem.SetName(name);
        scoreItem.SetScore(score);
        scoreItem.SetWord(word);

        if (scrabbleWords != null && scrabbleWords.Count > 0)
        {
            scoreItem.SetScrabbleWords(scrabbleWords);
        }

        newScoreItems[totalScoreItems - 1] = scoreItem;
        scoreItems = OrderByScore(newScoreItems);
        for (int i = 0; i < scoreItems.Length; i++)
        {
            ScoreItem item = scoreItems[i];
            item.transform.SetParent(null);
            item.transform.SetParent(transform);
        }
        scoreItem.transform.localScale = new Vector3(1, 1, 1);
    }

    private void SaveScore()
    {
        if (savingScore > 0) 
        {
            if (savingScrabbleWords != null && savingScrabbleWords.Count > 0)
            {
                CreateScoreItem(savingName, savingScore, savingWord, new List<string>(savingScrabbleWords));
            }
            else
            {
                CreateScoreItem(savingName, savingScore, savingWord);
            }
            savingScrabbleWords = new List<string>();
        }

        savingName = "";
        savingScore = -1;
        savingWord = "";
        savingScrabbleWords = new List<string>();
    }

}