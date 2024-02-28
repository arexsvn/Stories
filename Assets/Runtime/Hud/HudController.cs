using UnityEngine;
using signals;

public class HudController
{
    public Signal<Screens.Name> showScreen;
    private HudView _view;
    private readonly ClockController _clockController;
    private readonly AddressablesAssetService _assetService;

    public HudController(ClockController clockController, AddressablesAssetService assetService)
    {
        _clockController = clockController;
        _assetService = assetService;
        init();
    }

    private async void init()
    {
        _clockController.init();

        showScreen = new Signal<Screens.Name>();

        _view = await _assetService.Instantiate<HudView>();
        _view.over.Add(() => show(true));
        _view.off.Add(() => show(false));

        _view.canvasGroup.alpha = 0f;

        addElement("Journal", Screens.Name.Journal);
        addElement("Main Menu", Screens.Name.MainMenu);
    }

    private void addElement(string label, Screens.Name screenName)
    {
        ButtonView hudElement = Object.Instantiate(_view.hudElement, _view.buttonContainer.transform);
        hudElement.labelText.text = label;
        hudElement.button.onClick.AddListener(()=>hudElementClicked(screenName));
    }

    public void show(bool show = true)
    {
        _view.show(show);
    }

    public void showClock(bool show, double costInMinutes)
    {
        _clockController.showTimeCost(show, costInMinutes);
    }

    private void hudElementClicked(Screens.Name screenName)
    {
        showScreen.Dispatch(screenName);
    }
}
