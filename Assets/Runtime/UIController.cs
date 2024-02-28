using UnityEngine;
using signals;
using UnityEngine.UI;
using DG.Tweening;

public class UIController
{
    public Signal fadeComplete;
    private GameObject _fadeScreen;
    private const float FADE_TIME = 0.5f;
    readonly UICreator _uiCreator;
    readonly HudController _hudController;
    readonly TextOverlayController _textOverlayController;
    readonly JournalController _journalController;
    readonly IPublisher<ApplicationMessage> _applicationMessagePublisher;

    public UIController(UICreator uiCreator, HudController hudController, TextOverlayController textOverlayController, JournalController journalController, IPublisher<ApplicationMessage> applicationMessagePublisher)
    {
        _uiCreator = uiCreator;
        _hudController = hudController;
        _textOverlayController = textOverlayController;
        _journalController = journalController;
        _applicationMessagePublisher = applicationMessagePublisher;

        init();
    }

    private void init()
    {
        fadeComplete = new Signal();
        _hudController.showScreen.Add(handleShowScreen);
    }

    public void showClock(bool show, double costInMinutes = 0)
    {
        _hudController.showClock(show, costInMinutes);
    }

    public void showHud(bool show = true)
    {
        _hudController.show(show);
        _textOverlayController.show(show);
    }

    public void showJournal(bool show = true)
    {
        if (show)
        {
            _journalController.closeButtonClicked.AddOnce(handleJournalClose);
            pauseGame();
        }
        else
        {
            unPauseGame();
        }
        
        _journalController.show(show);
    }

    public void setText(string text)
    {
        _textOverlayController.show();
        _textOverlayController.setText(text);
    }

    public void showText(bool show = true)
    {
        _textOverlayController.show(show);
    }

    public void fade(bool fadeIn, Color color = default(Color), float fadeTime = -1, float delay = 0f)
    {
        if (fadeTime == -1)
        {
            fadeTime = FADE_TIME;
        }

        if (_fadeScreen == null)
        {
            _fadeScreen = _uiCreator.createFadeScreen();
        }
        else
        {
            _fadeScreen.SetActive(true);
        }

        int initAlpha = 0;
        int finalAlpha = 1;

        if (!fadeIn)
        {
            initAlpha = 1;
            finalAlpha = 0;
        }

        color.a = initAlpha;

        Image image =_fadeScreen.GetComponentInChildren<Image>();
        image.color = color;
		image.DOFade(finalAlpha, fadeTime).SetDelay(delay).OnComplete(delegate () { if (finalAlpha == 0) { _fadeScreen.SetActive(false); } handleFadeComplete(); });
    }

    private void pauseGame()
    {
        _applicationMessagePublisher.Publish(new ApplicationMessage(ApplicationAction.Pause));
    }

    private void unPauseGame()
    {
        _applicationMessagePublisher.Publish(new ApplicationMessage(ApplicationAction.UnPause));
    }

    private void handleShowScreen(Screens.Name screenName)
    {
        switch(screenName)
        {
            case Screens.Name.MainMenu :
                _applicationMessagePublisher.Publish(new ApplicationMessage(ApplicationAction.ShowMainMenu));
                showHud(false);
                break;

            case Screens.Name.Journal:
                showJournal(true);
                showHud(false);
                break;
        }
    }

    private void handleFadeComplete()
    {
        fadeComplete.Dispatch();
    }

    private void handleJournalClose()
    {
        if (_journalController.showing)
        {
            _journalController.show(false);
            unPauseGame();
        }
    }

    private void fitToScreen(GameObject container)
    {
        SpriteRenderer spriteRenderer = container.GetComponent<SpriteRenderer>();

        container.transform.localScale = new Vector3(1, 1, 1);

        float width = spriteRenderer.bounds.size.x;
        float height = spriteRenderer.bounds.size.y;

        float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        container.transform.localScale = new Vector3(worldScreenWidth / width, worldScreenHeight / height, 1);
    }
}

public class Screens
{
    public enum Name { MainMenu, Journal };
}
 
