using UnityEngine;
using System.Collections;
using signals;

public class HudController
{
    public Signal<Screens.Name> showScreen;
    private HudView _view;
    private static string PREFAB = "UI/Hud";
    private bool _showing = false;
    private readonly ClockController _clockController;

    public HudController(ClockController clockController)
    {
        _clockController = clockController;
        _clockController.init();
        init();
    }

    private void init()
    {
        showScreen = new Signal<Screens.Name>();

        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(PREFAB));
        _view = prefab.GetComponent<HudView>();
        _view.over.Add(() => show(true));
        _view.off.Add(() => show(false));

        show(false, 0);

        addElement("Journal", Screens.Name.Journal);
        addElement("Main Menu", Screens.Name.MainMenu);
    }

    private void addElement(string label, Screens.Name screenName)
    {
        ButtonView hudElement = Object.Instantiate(_view.hudElement, _view.buttonContainer.transform);
        hudElement.labelText.text = label;
        hudElement.button.onClick.AddListener(()=>hudElementClicked(screenName));
    }

    public void show(bool show = true, float fadeTime = -1)
    {
        _showing = show;
        UITransitions.fade(_view.gameObject, _view.canvasGroup, !show, false, fadeTime, false);
    }

    public void showClock(bool show, double costInMinutes)
    {
        _clockController.showTimeCost(show, costInMinutes);
    }

    public bool showing
    {
        get
        {
            return _showing;
        }
    }

    private void hudElementClicked(Screens.Name screenName)
    {
        showScreen.Dispatch(screenName);
    }
}
