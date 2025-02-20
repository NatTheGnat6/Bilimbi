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
            int order = 0;
            for (int j = 0; j < newItems.Length; j++)
            {
                if (j != i && items[j].GetScore() < items[i].GetScore())
                {
                    order++;
                }
            }
            newItems[order] = items[i];
            newItems[order].SetPlace(order + 1);
        }
        return newItems;
    }

    private void CreateScoreItem(string name, int score, string word)
    {
        int totalScoreItems = scoreItems.Length + 1;
        ScoreItem[] newScoreItems = new ScoreItem[totalScoreItems];
        for (int i = 0; i < scoreItems.Length; i++)
        {
            newScoreItems[i] = scoreItems[i];
        }
        ScoreItem scoreItem = Instantiate(scoreItemPrefab);
        scoreItem.SetName(name);
        scoreItem.SetScore(score);
        scoreItem.SetWord(word);
        newScoreItems[totalScoreItems - 1] = scoreItem;
        scoreItems = OrderByScore(newScoreItems);
    }

    private void SaveScore()
    {
        CreateScoreItem(savingName, savingScore, savingWord);
        savingName = "";
        savingScore = -1;
        savingWord = "";
    }
}