using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using signals;
using TMPro;
using System;
using System.Globalization;

public class ClockView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI _timeDisplayText;
    public CanvasGroup canvasGroup;
    public Signal over = new Signal();
    public Signal off = new Signal();

    public void OnPointerEnter(PointerEventData eventData)
    {
        over.Dispatch();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        off.Dispatch();
    }

    public void SetTime(DateTime time)
    {
        string timeString = time.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        _timeDisplayText.text = timeString;
    }
}