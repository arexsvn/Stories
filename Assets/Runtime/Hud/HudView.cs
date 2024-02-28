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
    private bool _showing = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        over.Dispatch();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        off.Dispatch();
    }

    public void show(bool show = true)
    {
        _showing = show;
        if (show)
        {
            UITransitions.fadeIn(canvasGroup, gameObject);
        }
        else
        {
            UITransitions.fadeOut(canvasGroup, gameObject, null, false);
        }
    }

    public bool showing
    {
        get
        {
            return _showing;
        }
    }
}
