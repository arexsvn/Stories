using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using signals;
using UnityEngine.UI;

public class HotspotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Signal over = new Signal();
    public Signal off = new Signal();
    public Signal click = new Signal();

    public void Start()
    {
        Button button = gameObject.GetComponentInChildren<Button>();
        button.onClick.AddListener(OnClick);

        Image image = button.gameObject.GetComponentInChildren<Image>();
        image.color = new Color(0, 1, 0, 0);
    }

    public void OnDestroy()
    {
        Button button = gameObject.GetComponentInChildren<Button>();
        button.onClick.RemoveListener(OnClick);
        click.RemoveAll();
        over.RemoveAll();
        off.RemoveAll();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        over.Dispatch();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        off.Dispatch();
    }

    private void OnClick()
    {
        click.Dispatch();
    }
}
