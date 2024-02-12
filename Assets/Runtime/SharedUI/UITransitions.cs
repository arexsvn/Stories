using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class UITransitions
{
	public const float FADE_TIME = 0.2f;

	public static void fade(GameObject target, CanvasGroup canvasGroup, bool fadeOut = true, bool destroyOnComplete = false, float fadeTime = -1f, bool deactivateOnComplete = true)
	{
        applyFade(canvasGroup.DOFade, canvasGroup.DOComplete, target, fadeOut, destroyOnComplete, fadeTime, deactivateOnComplete);
    }

    public static void fade(GameObject target, Image image, bool fadeOut = true, bool destroyOnComplete = false, float fadeTime = -1f, bool deactivateOnComplete = true)
    {
        applyFade(image.DOFade, image.DOComplete, target, fadeOut, destroyOnComplete, fadeTime, deactivateOnComplete);
    }

    public static void fade(GameObject target, TextMeshProUGUI text, bool fadeOut = true, bool destroyOnComplete = false, float fadeTime = -1f, bool deactivateOnComplete = true)
    {
        applyFade(text.DOFade, text.DOComplete, target, fadeOut, destroyOnComplete, fadeTime, deactivateOnComplete);
    }

    private static void applyFade(System.Func<float, float, Tween> fadeAction, 
                                 System.Func<bool, int> completeAction, 
                                 GameObject target, 
                                 bool fadeOut = true, 
                                 bool destroyOnComplete = false, 
                                 float fadeTime = -1f, 
                                 bool deactivateOnComplete = true)
    {
        if (fadeTime == -1f)
        {
            fadeTime = FADE_TIME;
        }

        if (fadeOut)
        {
            TweenCallback fadeComplete = delegate
            {
                if (destroyOnComplete)
                {
                    Object.Destroy(target);
                }
                else if(deactivateOnComplete)
                {
                    target.SetActive(false);
                }
            };

            completeAction(true);
            fadeAction(0, fadeTime).OnComplete(fadeComplete);
        }
        else
        {
            completeAction(true);
            target.SetActive(true);
            fadeAction(1, fadeTime);
        }
    }
}
