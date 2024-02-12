using UnityEngine;
using System.Collections;
using System;
using System.Globalization;

public class ClockController
{
    private ClockView _view;
    private static string PREFAB = "UI/Clock";
    private DateTime _gameTime;
    private int _currentDay;
    private bool _endOfDay;

    public ClockController()
    {
        
    }

    public void init()
    {
        _gameTime = new DateTime();
        _currentDay = _gameTime.Day;

        GameObject prefab = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(PREFAB));
        _view = prefab.GetComponent<ClockView>();
        _view.over.Add(handleOver);
        _view.off.Add(() => show(false));

        _view.SetTime(_gameTime);

        show(false, 0);
    }

    public void advanceTime(double minutes)
    {
        _gameTime = _gameTime.AddMinutes(minutes);

        if (_currentDay != _gameTime.Day)
        {
            _endOfDay = true;
        }
    }

    public void beginDay()
    {
        _endOfDay = false;
        _currentDay = _gameTime.Day;
    }

    /*
    public void add(GameObject parent)
    {
        GameObject prefab = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(PREFAB));
        _view = prefab.GetComponent<ClockView>();
        _view.over.Add(() => show(true));
        _view.off.Add(() => show(false));

        show(false, 0);
    }
    */

    public void handleOver()
    {
        return;
        _view.SetTime(_gameTime);
        show(true);
    }

    public void show(bool show = true, float fadeTime = -1)
    {
        UITransitions.fade(_view.gameObject, _view.canvasGroup, !show, false, fadeTime, false);
    }

    public void showTimeCost(bool show, double costInMinutes = 0)
    {
        return;
        
        this.show(show);

        if (costInMinutes != 0)
        {
            _view.SetTime(_gameTime.AddMinutes(costInMinutes));
        }
    }

    public DateTime gameTime
    {
        get
        {
            return _gameTime;
        }
    }

    public bool isEndOfDay
    {
        get
        {
            return _endOfDay;
        }
    }
}
