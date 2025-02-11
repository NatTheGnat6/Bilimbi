using UnityEngine;

public class GlassAnimation : MonoBehaviour
{
    public Animator animator;
    private bool doFlip;
    private float flipTimer;
    private bool doWarning;
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        doFlip = false;
        StopWarning();
    }
    public void Flip()
    {
        Show();
        StopWarning();
        doFlip = true;
        flipTimer = 0f;
    }
    public void StartWarning()
    {
        doWarning = true;
    }
    public void StopWarning()
    {
        doWarning = false;
    }
    public void Update() {
        if (enabled) {
            animator.SetInteger("State", -1);
            if (doFlip) {
                flipTimer += Time.fixedDeltaTime;
                if (flipTimer > 0.1f) {
                    animator.SetInteger("State", 1);
                    if (flipTimer > 0.3f) {
                        doFlip = false;
                        flipTimer = 0f;
                    }
                }
            } else {
                animator.SetInteger("State", doWarning ? 2 : 0);
            }
        }
    }
}
