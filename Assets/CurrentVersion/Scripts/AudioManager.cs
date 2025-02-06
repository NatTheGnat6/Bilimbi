using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource correctGuessSound;
    public AudioSource wrongGuessSound;
    public AudioSource winSound;
    public AudioSource loseSound;
    public AudioSource buttonSound;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayCorrectGuess() => correctGuessSound.Play();
    public void PlayWrongGuess() => wrongGuessSound.Play();
    public void PlayWin() => winSound.Play();
    public void PlayLose() => loseSound.Play();
    public void PlayButtonSound() => buttonSound.Play();
}