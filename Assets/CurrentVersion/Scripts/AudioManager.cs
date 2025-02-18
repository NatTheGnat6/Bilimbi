using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource guessSound;
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

    public void PlayGuess() => guessSound.Play();
    public void PlayWin() => winSound.Play();
    public void PlayLose() => loseSound.Play();
    public void PlayButtonSound() => buttonSound.Play();
}