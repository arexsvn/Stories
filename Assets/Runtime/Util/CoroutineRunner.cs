using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    private System.Action _delayedAction;
    private System.Action _delayedUpdateAction;
    private IEnumerator _delayedActionProcess;
    private float _delayedUpdateActionRemainingTime = 0f;

    public void Run(IEnumerator process)
    {
        StartCoroutine(process);
    }

    public void Stop(IEnumerator process)
    {
        StopCoroutine(process);
    }

    public bool PauseDelayedUpdateAction { get; set; } = false;

    public void DelayAction(System.Action action, float seconds)
    {
        _delayedAction = action;
        _delayedActionProcess = delayedActionEnumerator(action, seconds);
        Run(_delayedActionProcess);
    }

    public void DelayUpdateAction(System.Action action, float seconds)
    {
        _delayedUpdateActionRemainingTime = seconds;
        _delayedUpdateAction = action;
        PauseDelayedUpdateAction = false;
    }

    void Update()
    {
        if (_delayedUpdateAction != null && !PauseDelayedUpdateAction)
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

    public void RunDelayedActionNow()
    {
        System.Action action = _delayedAction;

        StopDelayedAction();

        if (action != null)
        {
            action();
        }
    }

    public void RunDelayedUpdateActionNow()
    {
        if (_delayedUpdateAction != null)
        {
            System.Action action = _delayedUpdateAction;
            _delayedUpdateAction = null;
            action();
        }
    }

    public void StopDelayedUpdateAction()
    {
        _delayedUpdateAction = null;
        PauseDelayedUpdateAction = false;
    }

    public void StopDelayedAction()
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

    public bool RunningDelayedAction
    {
        get
        {
            return _delayedActionProcess != null;
        }
    }

    public bool RunningDelayedUpdateAction
    {
        get
        {
            return _delayedUpdateAction != null;
        }
    }
}