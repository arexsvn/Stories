using UnityEngine;
using signals;
using UnityEngine.UI;
using DG.Tweening;

public class CameraController
{
    private Camera _camera;
    private string CAMERA_PREFAB = "MainCamera";
    public CameraController()
    {
        init();
    }

    private void init()
    {
        _camera = Object.Instantiate(Resources.Load<Camera>(CAMERA_PREFAB));
    }

    public Camera camera
    {
        get
        {
            return _camera;
        }
    }
}