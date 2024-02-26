using UnityEngine;
using signals;

public class MainMenuController
{
    public Signal newGame;
    public Signal resumeGame;
    public Signal loadGame;
    public Signal quitGame;
    private MainMenuView _view;
    private static string PREFAB = "UI/MainMenu";
    private static string BUTTON_PREFAB = "UI/MainMenuButton";
    private ButtonView _resumeButton;
    private ButtonView _loadButton;
    readonly UICreator _uiCreator;
    readonly LocaleManager _localeManager;
    private bool _inited;

    public MainMenuController(UICreator uiCreator, LocaleManager localeManager)
    {
        _uiCreator = uiCreator;
        _localeManager = localeManager;
    }

    public void init()
    {
        if (_inited)
        {
            return;
        }
        
        newGame = new Signal();
        resumeGame = new Signal();
        loadGame = new Signal();
        quitGame = new Signal();

        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(PREFAB));
        _view = prefab.GetComponent<MainMenuView>();
        _view.title.text = _localeManager.lookup("application_title");
        _resumeButton = _uiCreator.addButton(_view.buttonContainer, new ButtonData(resumeGame.Dispatch, _localeManager.lookup("application_resume"), null, BUTTON_PREFAB));
        _loadButton = _uiCreator.addButton(_view.buttonContainer, new ButtonData(loadGame.Dispatch, _localeManager.lookup("application_load"), null, BUTTON_PREFAB));
        _uiCreator.addButton(_view.buttonContainer, new ButtonData(newGame.Dispatch, _localeManager.lookup("application_new"), null, BUTTON_PREFAB));
        _uiCreator.addButton(_view.buttonContainer, new ButtonData(quitGame.Dispatch, _localeManager.lookup("application_quit"), null, BUTTON_PREFAB));

        _inited = true;
    }

    public void show(bool show = true, float fadeTime = -1)
    {
        _view.show(show, fadeTime);
    }

    public void allowResume(bool allow)
    {
        _resumeButton.gameObject.SetActive(allow);
    }

    public void allowLoad(bool allow)
    {
        _loadButton.gameObject.SetActive(allow);
    }
}
