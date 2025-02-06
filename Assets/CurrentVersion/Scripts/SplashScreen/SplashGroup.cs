using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public delegate void SplashFinished();
public class SplashGroup : MonoBehaviour
{
    public SplashObject[] splashObjects;
    public event SplashFinished OnFinished;
    private bool beganSplash = false;
    private float splashTime;
    public float splashStartScale = 1.5f;
    public float splashEndScale = 3f;
    public float splashFadeInTime = 1.25f;
    public float splashFadePause = .75f;
    public float splashFadeOutTime = 0.5f;
    public float splashDelay = 0.75f;
    private float splashTotalTime;
    public void StopSplash() {
        foreach (SplashObject splashObject in splashObjects) {
            splashObject.SetActive(false);
        }
        gameObject.SetActive(false);
        splashTime = 0f;
        beganSplash = false;
    }
    public void BeginSplash()
    {
        splashTime = 0f;
        splashTotalTime = splashFadeInTime + splashFadePause + splashFadeOutTime;
        foreach (SplashObject splashObject in splashObjects) {
            splashObject.SetScale(0);
            splashObject.SetColorAlpha(0);
            splashObject.SetActive(true);
        }
        gameObject.SetActive(true);
        beganSplash = true;
    }
    public void Update()
    {
        if (beganSplash) {
            if (splashTime > (splashTotalTime + splashDelay)) {
                beganSplash = false;
                OnFinished?.Invoke();
                Destroy(gameObject);
            } else {
                float scale = Helper.Interpolate(
                    splashStartScale, splashEndScale, splashTime / splashTotalTime
                );
                foreach (SplashObject splashObject in splashObjects) {
                    splashObject.SetScale(scale);
                    splashObject.SetColorAlpha(
                        splashTime <= splashFadeInTime ?
                        SplashEasingFunction(splashTime / splashFadeInTime) :
                        splashTime <= splashFadePause ? 0f : SplashEasingFunction(1 - (
                            (splashTime - splashFadeInTime - splashFadePause)
                            / splashFadeOutTime
                        ))
                    );
                }
                splashTime += Time.deltaTime;
            }
        }
    }
    public float SplashEasingFunction(float alpha) => Helper.CubicEase(alpha);
}
