using UnityEngine;

public class GameOverImage : MonoBehaviour
{
    public Vector2 startPosition = new Vector2(0, -1000);
    private Vector2 endingPosition;
    private RectTransform rectTransform;
    private bool showingEffect;
    private float effectTimePassed = 0f;
    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        endingPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = startPosition;
    }
    public void Update()
    {
        if (showingEffect)
        {
            effectTimePassed += Time.deltaTime;
            float effectAlpha = effectTimePassed / Constants.GAME_OVER_EFFECT_TIME;
            if (effectAlpha >= 1)
            {
                gameObject.SetActive(false);
                showingEffect = false;
            }
            else
            {
                float positionAlpha = EffectEasingFunction(
                    effectAlpha >= Constants.GAME_OVER_RETURN_ALPHA ? 
                    1 - ((effectAlpha - Constants.GAME_OVER_RETURN_ALPHA) / (1 - Constants.GAME_OVER_RETURN_ALPHA)) :
                    effectAlpha >= Constants.GAME_OVER_REACH_ALPHA ? 1 :
                    effectAlpha / Constants.GAME_OVER_REACH_ALPHA
                );
                rectTransform.anchoredPosition = Helper.Interpolate(startPosition, endingPosition, positionAlpha);
            }
        }   
    }
    public void StartEffect()
    {
        gameObject.SetActive(true);
        effectTimePassed = 0f;
        showingEffect = true;
    }
    private float EffectEasingFunction(float alpha) => Helper.CubicEaseOut(alpha);
}
