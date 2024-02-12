using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvas : MonoBehaviour, IUICanvas
{
    public RectTransform ScreenLayer => _screenLayer;

    public RectTransform DialogLayer => _dialogLayer;

    public Transform BaseContainer => _baseContainer;

    [SerializeField] private RectTransform _screenLayer;
    [SerializeField] private RectTransform _dialogLayer;
    [SerializeField] private Transform _baseContainer;
}
