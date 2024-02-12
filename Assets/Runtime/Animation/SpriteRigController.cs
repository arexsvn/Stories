using System.Collections.Generic;
using UnityEngine;

public class SpriteRigController : MonoBehaviour
{
    public Animator animator;
    private Dictionary<string, SpriteRenderer> _partToRenderer;
    private Dictionary<string, string> _partToSkinMap;
    private Dictionary<string, AttachPoint> _attachPoints;
    private Dictionary<string, Sprite[]> _spriteCache;
    private string _path = "";

    void Start()
    {
        _partToRenderer = new Dictionary<string, SpriteRenderer>();
        _partToSkinMap = new Dictionary<string, string>();
        _attachPoints = new Dictionary<string, AttachPoint>();
        _spriteCache = new Dictionary<string, Sprite[]>();

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer renderer in renderers)
        {
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            _partToRenderer[renderer.sprite.name] = renderer;
            _partToSkinMap[renderer.sprite.name] = renderer.sprite.texture.name;
        }

        AttachPoint[] attachPoints = GetComponentsInChildren<AttachPoint>();
        foreach (AttachPoint attachPoint in attachPoints)
        {
            _attachPoints[attachPoint.Name] = attachPoint;
        }
    }

    public void init(string path)
    {
        _path = path;
    }

    public void addAttachment(string attachment, string attachPointName)
    {
        AttachPoint attachPoint = _attachPoints[attachPointName];
        GameObject attachmentPrefab = Resources.Load<GameObject>(_path + attachment);
        attachPoint.currentAttachment = Instantiate(attachmentPrefab, attachPoint.gameObject.transform);
        attachPoint.currentAttachment.transform.localPosition = new Vector2(attachPoint.offsetX, attachPoint.offsetY);
    }

    public void removeAttachment(string attachPointName)
    {
        AttachPoint attachPoint = _attachPoints[attachPointName];
        if (attachPoint.currentAttachment != null)
        {
            GameObject.Destroy(attachPoint.currentAttachment);
            attachPoint.currentAttachment = null;
        }
    }

    public void changeSkin(string part, string newSpriteName)
    {
        if (_partToSkinMap[part] != newSpriteName)
        {
            _partToSkinMap[part] = newSpriteName;

            if (!_spriteCache.ContainsKey(newSpriteName))
            {
                _spriteCache[newSpriteName] = Resources.LoadAll<Sprite>(_path + newSpriteName);
            }

            foreach (Sprite sprite in _spriteCache[newSpriteName])
            {
                if (sprite.name == part)
                {
                    _partToRenderer[part].sprite = sprite;
                    break;
                }
            }
        }
    }

    public void changeSkin(string newSpriteName)
    {
        if (!_spriteCache.ContainsKey(newSpriteName))
        {
            _spriteCache[newSpriteName] = Resources.LoadAll<Sprite>(_path + newSpriteName);
        }

        foreach (Sprite sprite in _spriteCache[newSpriteName])
        {
            if (sprite != _partToRenderer[sprite.name].sprite)
            {
                _partToRenderer[sprite.name].sprite = sprite;
                break;
            }
        }
    }

    // test
    /*
    private float deltaTime = 0f;
    
    void LateUpdate()
    {
        if (deltaTime > 56f && deltaTime < 100f)
        {
            changeSkin("Head", "cop_angry");
            changeSkin("LeftArm", "cop");
            changeSkin("RightArm", "cop");
            removeAttachment("LeftHand");
            animator.Play("idle");
            deltaTime = 100f;
        }
        else if(deltaTime > 3f && deltaTime < 50f)
        {
            changeSkin("Head", "cop");
            changeSkin("Body", "cop_angry");
            changeSkin("LeftArm", "cop_angry");
            changeSkin("RightArm", "cop_angry");
            addAttachment("Ball", "LeftHand");
            animator.Play("angry");
            deltaTime = 50f;
        }
        else
        {
            deltaTime += Time.deltaTime;
        }
    }
    */
}
