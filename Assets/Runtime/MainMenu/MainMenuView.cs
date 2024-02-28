using UnityEngine;
using TMPro;

public class MainMenuView : MonoBehaviour
{
    public TextMeshProUGUI title;
    public GameObject buttonContainer;
    public CanvasGroup canvasGroup;

    public void show(bool show = true, float fadeTime = -1)
    {
        float alpha = 1f;
        
        if (!show)
        {
            alpha = 0f;
        }
        
        UITransitions.fade(canvasGroup, alpha, fadeTime, gameObject, null, !show);
    }
}
