using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class RowVariantOG : MonoBehaviour
{
    private bool isFadingVariant;
    private float fadeDelayVariant = 0;
    private float fadeTimePassedVariant = 0.0f;
    public TileVariantOG[] tilesVariant { get; private set; }
    
    public string wordVariant
    {
        get
        {
            string wordVariant = "";

            for (int i = 0; i < tilesVariant.Length; i++) {
                wordVariant += tilesVariant[i].letterVariant;
            }

            return wordVariant;
        }
    }

    private void Awake()
    {
        tilesVariant = GetComponentsInChildren<TileVariantOG>();
    }

    private void Update() {
        if (isFadingVariant) {
            fadeTimePassedVariant += Time.deltaTime;
            if (fadeTimePassedVariant > fadeDelayVariant) {
                float alpha = DisappearEasingFunctionVariant(1 - ((fadeTimePassedVariant - fadeDelayVariant) / Constants.ROW_FADE_TIME));
                for (int i = 0; i < tilesVariant.Length; i++) {
                    if (tilesVariant[i] != null) {
                        tilesVariant[i].SetAlphaVariant(alpha);
                    }
                }
                if (alpha < 0) {
                    Destroy(gameObject);
                    isFadingVariant = false;
                }
            }
        }
    }

    private float DisappearEasingFunctionVariant(float alpha) => Helper.CubicEase(alpha);

    public void DisappearVariant(int delay)
    {
        fadeDelayVariant = delay * Constants.ROW_FADE_DELAY_FACTOR;
        isFadingVariant = true;
    }

    public void UpdateTilesVariant(TileVariantOG[] newTilesVariant)
    {
        this.tilesVariant = newTilesVariant;
    }
}