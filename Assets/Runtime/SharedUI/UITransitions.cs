using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;

public class UITransitions
{
	public const float FADE_TIME = 0.2f;

	public static void fade(GameObject target, CanvasGroup canvasGroup, bool fadeOut = true, bool destroyOnComplete = false, float fadeTime = -1f, bool deactivateOnComplete = true, Action<GameObject> complete = null)
	{
        applyFade(canvasGroup.DOFade, canvasGroup.DOComplete, target, fadeOut, destroyOnComplete, fadeTime, deactivateOnComplete, complete);
    }

    public static void fade(GameObject target, Image image, bool fadeOut = true, bool destroyOnComplete = false, float fadeTime = -1f, bool deactivateOnComplete = true, Action<GameObject> complete = null)
    {
        applyFade(image.DOFade, image.DOComplete, target, fadeOut, destroyOnComplete, fadeTime, deactivateOnComplete, complete);
    }

    public static void fade(GameObject target, TextMeshProUGUI text, bool fadeOut = true, bool destroyOnComplete = false, float fadeTime = -1f, bool deactivateOnComplete = true, Action<GameObject> complete = null)
    {
        applyFade(text.DOFade, text.DOComplete, target, fadeOut, destroyOnComplete, fadeTime, deactivateOnComplete, complete);
    }

    private static void applyFade(Func<float, float, Tween> fadeAction, 
                                  Func<bool, int> completeAction, 
                                  GameObject target, 
                                  bool fadeOut = true, 
                                  bool destroyOnComplete = false, 
                                  float time = -1f, 
                                  bool deactivateOnComplete = true,
                                  Action<GameObject> complete = null)
    {
        if (time == -1f)
        {
            time = FADE_TIME;
        }

        if (fadeOut)
        {
            TweenCallback fadeComplete = delegate
            {
                if (destroyOnComplete)
                {
                    UnityEngine.Object.Destroy(target);
                }
                else if(deactivateOnComplete)
                {
                    target.SetActive(false);
                }

                complete?.Invoke(target);
            };

            completeAction(false);
            fadeAction(0, time).OnComplete(fadeComplete);
        }
        else
        {
            completeAction(false);
            target.SetActive(true);

            if (complete != null)
            {
                fadeAction(1, time).OnComplete(() => complete(target));
            }
            else
            {
                fadeAction(1, time);
            }
        }
    }
}
