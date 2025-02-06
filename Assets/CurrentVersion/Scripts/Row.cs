using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class Row : MonoBehaviour
{
    private bool isFading;
    private float fadeDelay = 0;
    private float fadeTimePassed = 0.0f;
    public Tile[] tiles { get; private set; }
    
    public string word
    {
        get
        {
            string word = "";

            for (int i = 0; i < tiles.Length; i++) {
                word += tiles[i].letter;
            }

            return word;
        }
    }

    private void Awake()
    {
        tiles = GetComponentsInChildren<Tile>();
    }

    private void Update() {
        if (isFading) {
            fadeTimePassed += Time.deltaTime;
            if (fadeTimePassed > fadeDelay) {
                float alpha = DisappearEasingFunction(1 - ((fadeTimePassed - fadeDelay) / Constants.ROW_FADE_TIME));
                for (int i = 0; i < tiles.Length; i++) {
                    if (tiles[i] != null) {
                        tiles[i].SetAlpha(alpha);
                    }
                }
                if (alpha < 0) {
                    Destroy(gameObject);
                    isFading = false;
                }
            }
        }
    }

    private float DisappearEasingFunction(float alpha) => Helper.CubicEase(alpha);

    public void Disappear(int delay)
    {
        fadeDelay = delay * Constants.ROW_FADE_DELAY_FACTOR;
        isFading = true;
    }

    public void UpdateTiles(Tile[] tiles)
    {
        this.tiles = tiles;
    }
}