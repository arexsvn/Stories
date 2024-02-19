using System;
using System.Collections.Generic;
using UnityEngine;

public class UIViewStack : IDisposable
{
    private LinkedList<IUIView> _viewStack;

    public UIViewStack()
    {

    }

    /// <summary>
    /// Pops the topmost screen or dialog view from the stack in the order it was added.
    ///    If the next view down the stack is 'UIView.IsFullscreen=true' only that view will be shown.
    ///    If the next view down the stack is 'UIView.IsFullscreen=false' that view will be shown and it will continue
    ///       going down the stack showing views until it hits the next fullscreen view where it will stop.  This will allow
    ///       all of the dialogs a screen opened to be shown when it is gone back to.
    /// <param name="animate">(optional, default 'true') Animate the opening and closing of views.</param>
    /// </summary>
    public void PopViewFromStack(bool animate = true)
    {
        if (_viewStack.Count < 2)
        {
            return;
        }

        IUIView viewToHide = _viewStack.First.Value;
        viewToHide.Hide(animate);
        _viewStack.RemoveFirst();

        // If the current view is full screen, reactivate all the views lower in the stack until we hit another fullscreen view.
        //   This allows the next fullscreen view plus all its 'child' views to be visible again.
        if (viewToHide.IsFullScreen)
        {
            foreach (IUIView uiView in _viewStack)
            {
                uiView.Show(animate);
                if (uiView.IsFullScreen)
                {
                    break;
                }
            }
        }
        else
        {
            IUIView uiView = _viewStack.First.Value;
            uiView.Show(animate);
        }
    }

    /// <summary>
    /// Add a custom view to the stack. This allows for back button support and caching of custom views or non-MonoBehaviours that implement IUIView.
    /// <param name="view"> An instance of a view to add to the view stack and Show. If the IUIView.IsFullscreen=true it will hide other views.</param>
    /// </summary>
    public T AddViewToStack<T>(T view = default) where T : IUIView
    {
        // Aan instance MUST be passed in to be added to the view stack.
        if (EqualityComparer<T>.Default.Equals(view, default))
        {
            Debug.LogError("UIViewStack :: AddViewToStack : An instance must be passed in.");
            return default(T);
        }

        if (view.IsFullScreen)
        {
            HideAllViews();
        }

        if (_viewStack.First?.Value != (IUIView)view)
        {
            _viewStack.AddFirst(view);
        }

        return view;
    }

    public IUIView AddViewToStack(IUIView view)
    {
        return AddViewToStack<IUIView>(view);
    }

    /// <summary>
    /// Removes a specific view from the view stack.
    /// <param name="assetName">Asset name.</param>
    /// </summary>
    public void RemoveViewFromStack(IUIView view)
    {
        _viewStack.Remove(view);
        // Make sure there are no successive duplicate views in the stack after removing.
        _viewStack = RemoveSuccessiveDuplicates(_viewStack);
    }

    public void RemoveViewFromStack<T>()
    {
        foreach (IUIView view in _viewStack)
        {
            if (view is T)
            {
                RemoveViewFromStack(view);
            }
        }
    }

    /// <summary>
    /// Hide all views.
    /// </summary>
    private void HideAllViews()
    {
        foreach (IUIView view in _viewStack)
        {
            view.Hide(false);
        }
    }

    /// <summary>
    /// Clear the view stack (history of views opened.)
    /// </summary>
    public void ClearViewStack()
    {
        _viewStack.Clear();
    }

    private LinkedList<IUIView> RemoveSuccessiveDuplicates(LinkedList<IUIView> list)
    {
        LinkedList<IUIView> results = new LinkedList<IUIView>();
        foreach (IUIView element in list)
        {
            if (results.Count == 0 || results.Last.Value != element)
                results.AddLast(element);
        }
        return results;
    }

    protected void Dispose(bool disposing)
    {
        if (_viewStack != null)
        {
            _viewStack.Clear();
            _viewStack = null;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
