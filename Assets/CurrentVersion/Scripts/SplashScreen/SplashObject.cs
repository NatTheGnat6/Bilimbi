
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SplashObject : MonoBehaviour
{
    public TMP_Text text;
    public Image image;
    public void SetActive(bool isActive) => gameObject.SetActive(isActive);
    public void SetScale(float scale) {
        transform.localScale = new Vector3(scale, scale, transform.localScale.z);
    }
    public void SetColorAlpha(float colorAlpha) {
        if (text != null) {
            text.color = Helper.AlphaifyColor(text.color, colorAlpha);
        }
        if (image != null) {
            image.color = Helper.AlphaifyColor(image.color, colorAlpha);
        }
    }
}
