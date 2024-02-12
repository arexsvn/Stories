using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using signals;
using UnityEngine.UI;

public class ChoiceView : MonoBehaviour
{
    public TextMeshProUGUI textArea;
    public Signal click = new Signal();

    public void Start()
    {
        Button button = gameObject.GetComponentInChildren<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnDestroy()
    {
        Button button = gameObject.GetComponentInChildren<Button>();
        button.onClick.RemoveListener(OnClick);
        click.RemoveAll();
    }

    private void OnClick()
    {
        click.Dispatch();
    }
}
