using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneShifter : MonoBehaviour
{
    public void LoadStartScene()
    {
        SceneManager.LoadScene("Start");
    }

    public void LoadWordleScene()
    {
        SceneManager.LoadScene("Wordle");
    }

    public void LoadCreditsScene()
    {
        SceneManager.LoadScene("CreditsScene");
    }
}