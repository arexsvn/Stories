using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitView : MonoBehaviour
{
    public Button button;
    public ImageContainerView imageContainer;
    public Transform rigContainer;
    private SpriteRigController _currentRig;
    private static string PORTRAITS_PATH = "images/portraits/";
    private static string RIG_PATH = "CharacterRigs/";
    private static string SEPARATOR = "_";
    private string _characterId;
    private string _emotion;
    private string _asset;

    public void display(string characterId, string emotion = null)
    {
        if (characterId == null)
        {
            Debug.LogError("PortraitView.display : characterId is null.");
            return;
        }

        _characterId = characterId;

        string asset = PORTRAITS_PATH + characterId;

        if (!string.IsNullOrEmpty(emotion))
        {
            _emotion = emotion;
            asset += SEPARATOR + _emotion;
        }
        
        if (_asset != asset)
        {
            _asset = asset;
            imageContainer.image.sprite = Resources.Load<Sprite>(asset);
        }
    }

    public void displayRig(string characterId, string emotion = null)
    {
        if (rigContainer == null)
        {
            rigContainer = gameObject.transform;
        }

        if (characterId == null)
        {
            Debug.LogError("PortraitView.display : characterId is null.");
            return;
        }

        if (_characterId != characterId)
        {
            if (_currentRig != null)
            {
                Object.Destroy(_currentRig.gameObject);
            }

            _characterId = characterId;
            string basePath = RIG_PATH + _characterId + "/";
            _asset = basePath + _characterId;
            SpriteRigController prefab = Resources.Load<SpriteRigController>(_asset);
            _currentRig = Object.Instantiate<SpriteRigController>(prefab, rigContainer);
            _currentRig.init(basePath);
            //_currentRig.transform.localScale = new Vector3(100, 100, 1);
            _currentRig.transform.localPosition = new Vector3(-46.671f, -482.027f, 1);
        }

        if (emotion != _emotion)
        {
            _emotion = emotion;

            if (emotion != null)
            {
                _currentRig.changeSkin(_characterId + SEPARATOR + emotion);
                _currentRig.animator.Play(emotion);
            }
            else
            {
                _currentRig.changeSkin(_characterId);
            }
        }
    }

    public void darken(bool makeDark = true)
    {
        Color32 newColor;

        if (makeDark)
        {
            newColor = new Color32(210, 210, 210, 100);
        }
        else
        {
            newColor = new Color32(255, 255, 255, 255);
        }

        imageContainer.image.color = newColor;
    }

    public string characterId
    {
        get
        {
            return _characterId;
        }
    }

    public string emotion
    {
        get
        {
            return _emotion;
        }
    }
}
