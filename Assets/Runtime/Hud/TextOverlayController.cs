using UnityEngine;

public class TextOverlayController
{
    private TextOverlayView _view;
    private static string PREFAB = "UI/TextOverlay";
    private bool _showing = false;

    public TextOverlayController()
    {
        init();
    }

    public void init()
    {
        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(PREFAB));
        _view = prefab.GetComponent<TextOverlayView>();
        _view.gameObject.SetActive(false);
        clearText();
    }

    public void setText(string text)
    {
        _view.textField.text = text;
    }

    public void clearText()
    {
        _view.textField.text = "";
    }

    public void show(bool show = true)
    {
        _showing = show;
        _view.show(show);
    }

    public bool showing
    {
        get
        {
            return _showing;
        }
    }
}
