using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SplashScreen : MonoBehaviour
{
    public SplashGroup[] splashGroups;
    private int splashGroupIndex;
    private SplashGroup lastSplashGroup;
    public void Start()
    {
        splashGroupIndex = -1;
        foreach(SplashGroup splashGroup in splashGroups) {
            splashGroup.StopSplash();
        }
        NextGroup();
    }
    public void NextGroup() {
        if (lastSplashGroup != null) {
            lastSplashGroup.OnFinished -= NextGroup;
        }
        splashGroupIndex++;
        if (splashGroupIndex >= splashGroups.Length) {
            SceneManager.LoadSceneAsync(Constants.CURRENT_SCENE_PATH);
        } else {
            SplashGroup splashGroup = splashGroups[splashGroupIndex];
            lastSplashGroup = splashGroup;
            splashGroup.BeginSplash();
            splashGroup.OnFinished += NextGroup;
        }
    }
}
