using System;
using System.Collections.Generic;
using UnityEngine;

public class UIUtils
{
    public static void RecycleOrCreateItems<T, V>(Transform container, T prefab, List<V> buttons, Action<T, V> setup) where T : MonoBehaviour
    {
        T[] existingViews = container.GetComponentsInChildren<T>();
        int endIndex = buttons.Count;
        if (existingViews.Length > endIndex)
        {
            endIndex = existingViews.Length;
        }

        for (int n = 0; n < endIndex; n++)
        {
            if (n >= buttons.Count)
            {
                UnityEngine.Object.Destroy(existingViews[n].gameObject);
            }
            else
            {
                T buttonView;
                if (n >= existingViews.Length)
                {
                    buttonView = UnityEngine.Object.Instantiate(prefab, container);
                }
                else
                {
                    // Reuse existing buttons if they exist.
                    buttonView = existingViews[n];
                }

                setup.Invoke(buttonView, buttons[n]);
            }
        }
    }
}
