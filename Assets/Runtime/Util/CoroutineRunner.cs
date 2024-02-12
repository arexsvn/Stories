using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    private System.Action _delayedAction;
    private System.Action _delayedUpdateAction;
    private IEnumerator _delayedActionProcess;
    private bool _pauseDelayedUpdateAction = false;
    private float _delayedUpdateActionRemainingTime = 0f;

    public void run(IEnumerator process)
    {
        StartCoroutine(process);
    }

    public void stop(IEnumerator process)
    {
        StopCoroutine(process);
    }

    public bool pauseDelayedUpdateAction
    {
        get
        {
            return _pauseDelayedUpdateAction;
        }
        set
        {
            _pauseDelayedUpdateAction = value;
        }
    }

    public void delayAction(System.Action action, float seconds)
    {
        _delayedAction = action;
        _delayedActionProcess = delayedActionEnumerator(action, seconds);
        run(_delayedActionProcess);
    }

    public void delayUpdateAction(System.Action action, float seconds)
    {
        _delayedUpdateActionRemainingTime = seconds;
        _delayedUpdateAction = action;
        _pauseDelayedUpdateAction = false;
    }

    void Update()
    {
        if (_delayedUpdateAction != null && !_pauseDelayedUpdateAction)
        {
            _delayedUpdateActionRemainingTime -= Time.deltaTime;

            if (_delayedUpdateActionRemainingTime <= 0)
            {
                System.Action action = _delayedUpdateAction;
                _delayedUpdateAction = null;
                action();
            }
        }
    }

    public void runDelayedActionNow()
    {
        System.Action action = _delayedAction;

        stopDelayedAction();

        if (action != null)
        {
            action();
        }
    }

    public void runDelayedUpdateActionNow()
    {
        if (_delayedUpdateAction != null)
        {
            System.Action action = _delayedUpdateAction;
            _delayedUpdateAction = null;
            action();
        }
    }

    public void stopDelayedUpdateAction()
    {
        _delayedUpdateAction = null;
        _pauseDelayedUpdateAction = false;
    }

    public void stopDelayedAction()
    {
        if (_delayedActionProcess != null)
        {
            StopCoroutine(_delayedActionProcess);
            _delayedActionProcess = null;
            _delayedAction = null;
        }
    }

    IEnumerator delayedActionEnumerator(System.Action action, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _delayedActionProcess = null;
        _delayedAction = null;

        action();
    }

    public bool runningDelayedAction
    {
        get
        {
            return _delayedActionProcess != null;
        }
    }

    public bool runningDelayedUpdateAction
    {
        get
        {
            return _delayedUpdateAction != null;
        }
    }
}