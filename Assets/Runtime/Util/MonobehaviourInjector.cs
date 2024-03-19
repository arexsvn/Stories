using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MonobehaviourInjector
{
    private readonly IObjectResolver _dependencyInjectionContainer;

    public MonobehaviourInjector(IObjectResolver dependencyInjectionContainer)
    {
        _dependencyInjectionContainer = dependencyInjectionContainer;
    }

    private void InjectDependencies<T>(T target)
    {
        if (_dependencyInjectionContainer != null)
        {
            try
            {
                if (target is GameObject gameObject)
                {
                    _dependencyInjectionContainer.InjectGameObject(gameObject);
                }
                else
                {
                    _dependencyInjectionContainer.Inject(target);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MonobehaviourInjector] InjectDependencies : Exception while injecting dependencies '{e.Message}'");
            }
        }
        else
        {
            Debug.LogError("[MonobehaviourInjector] InjectDependencies : Container not available for injection.");
        }
    }
}
