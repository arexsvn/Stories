using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuView : MonoBehaviour
{
    public TextMeshProUGUI title;
    public GameObject buttonContainer;
    public CanvasGroup canvasGroup;

    public void show(bool show = true, float fadeTime = -1)
    {
        UITransitions.fade(gameObject, canvasGroup, !show, false, fadeTime);
    }
}
