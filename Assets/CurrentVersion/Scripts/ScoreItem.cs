using TMPro;
using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    public TMP_Text placeText;
    public TMP_Text scoreText;
    public TMP_Text nameText;
    public TMP_Text wordText;
    private int score;
    public void SetName(string name)
    {
        nameText.text = name;
    }
    public void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
    }
    public int GetScore()
    {
        return score;
    }
    public void SetWord(string word)
    {
        wordText.text = word;
    }
}
