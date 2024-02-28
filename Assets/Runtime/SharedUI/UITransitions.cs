using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;

public class UITransitions
{
	public const float FADE_TIME = 0.2f;

    public static void fadeIn(CanvasGroup canvasGroup, GameObject target = null, Action<GameObject> complete = null)
    {
        if (target == null)
        {
            target = canvasGroup.gameObject;
        }
        
        applyFade(canvasGroup.DOFade, canvasGroup.DOComplete, target, 1f, -1, complete);
    }

    public static void fadeOut(CanvasGroup canvasGroup, GameObject target = null, Action <GameObject> complete = null, bool deactivateOnComplete = true)
    {
        if (target == null)
        {
            target = canvasGroup.gameObject;
        }

        applyFade(canvasGroup.DOFade, canvasGroup.DOComplete, target, 0f, -1, complete, deactivateOnComplete);
    }

    public static void fade(CanvasGroup canvasGroup, float alpha, float fadeTime = -1f, GameObject target = null, Action < GameObject> complete = null, bool deactivateOnComplete = false)
	{
        if (target == null)
        {
            target = canvasGroup.gameObject;
        }

        applyFade(canvasGroup.DOFade, canvasGroup.DOComplete, target, alpha, fadeTime, complete, deactivateOnComplete);
    }

    public static void fade(Image image, float alpha, float fadeTime = -1f, GameObject target = null, Action < GameObject> complete = null, bool deactivateOnComplete = false)
    {
        if (target == null)
        {
            target = image.gameObject;
        }

        applyFade(image.DOFade, image.DOComplete, target, alpha, fadeTime, complete, deactivateOnComplete);
    }

    public static void fade(TextMeshProUGUI text, float alpha, float fadeTime = -1f, GameObject target = null, Action < GameObject> complete = null, bool deactivateOnComplete = false)
    {
        if (target == null)
        {
            target = text.gameObject;
        }

        applyFade(text.DOFade, text.DOComplete, target, alpha, fadeTime, complete, deactivateOnComplete);
    }

    private static void applyFade(Func<float, float, Tween> fadeAction, 
                                  Func<bool, int> completeAction, 
                                  GameObject target,
                                  float alpha, 
                                  float time = -1f, 
                                  Action<GameObject> complete = null,
                                  bool deactivateOnComplete = false)
    {
        if (time == -1f)
        {
            time = FADE_TIME;
        }

        completeAction(false);

        target.SetActive(true);

        TweenCallback fadeComplete = delegate
        {
            if (alpha == 0f && deactivateOnComplete)
            {
                target.SetActive(false);
            }

            complete?.Invoke(target);
        };

        fadeAction(alpha, time).OnComplete(fadeComplete);
    }
}
