using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using signals;

public class HudView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject buttonContainer;
    public CanvasGroup canvasGroup;
    public Signal over = new Signal();
    public Signal off = new Signal();
    public ButtonView hudElement;

    public void OnPointerEnter(PointerEventData eventData)
    {
        over.Dispatch();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        off.Dispatch();
    }
}
