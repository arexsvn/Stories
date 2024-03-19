using System;
using UnityEngine;

public interface IUITransitions
{
    public void FadeIn(CanvasGroup canvasGroup, GameObject target = null, Action<GameObject> complete = null);

    public void FadeOut(CanvasGroup canvasGroup, GameObject target = null, Action<GameObject> complete = null, bool deactivateOnComplete = true);
}
