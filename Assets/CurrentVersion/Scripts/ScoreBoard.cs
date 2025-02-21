using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using TMPro;
using Mono.Cecil.Cil;
using UnityEngine.InputSystem.OnScreen;
using JetBrains.Annotations;


[DefaultExecutionOrder(-1)]
public class ScoreBoard : MonoBehaviour
{
    public event Helper.Event OnPromptFinished;

    [Header("UI / References")]
    public GameObject promptCanvas;
    public TMP_Text promptScoreText;
    public ScoreInputField promptField;
    public ScoreItem scoreItemPrefab;

    // Saving prompts
    private string savingName;
    private int savingScore;
    private string savingWord;
    private ScoreItem[] scoreItems;

    public void PromptScoreSave(int score, string word)
    {
        savingScore = score;
        savingWord = word;
        promptScoreText.text = Constants.SCORE_DISPLAY_PREFIX + score.ToString();
        promptField.TextReset();
        promptField.OnSubmitted += SubmitSavePrompt;
        promptCanvas.SetActive(true);
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

    private void SaveScore()
    {
        if (savingScore > 0) {
            CreateScoreItem(savingName, savingScore, savingWord);
        }
        savingName = "";
        savingScore = -1;
        savingWord = "";
    }
}