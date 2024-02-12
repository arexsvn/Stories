using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InboxListItemView : MonoBehaviour
{
    public InboxMessage message { get; private set; }
    [SerializeField] private TextMeshProUGUI _messageType;
    [SerializeField] private TextMeshProUGUI _messageSubject;
    [SerializeField] private TextMeshProUGUI _expirationTimeText;
    [SerializeField] private TextMeshProUGUI _createdAtText;
    [SerializeField] private Image _claimedPrizeImage;
    [SerializeField] private Image _unClaimedPrizeImage;
    [SerializeField] private Button _button;
    [SerializeField] private Transform _readState;
    [SerializeField] private Transform _unreadState;
    [SerializeField] private Transform _selectedState;
    
    [SerializeField] private Transform _typeSale;
    [SerializeField] private Transform _typeNotice;
    [SerializeField] private Transform _typeReward;
    [SerializeField] private Transform _typeAnnouncement;
    private const string TYPE_SALE = "sale";
    private const string TYPE_NOTICE = "notice";
    private const string TYPE_REWARD = "reward";
    private const string TYPE_ANNOUNCEMENT = "announcement";
    private bool _expires;
    
    public void init(InboxMessage message, Action callback)
    {
        this.message = message;

        _expirationTimeText.text = "";
        
        if (/*!message.Claimed && */message.expirationDate != 0)
        {
            _expires = true;
        }
        
        DateTime createdAt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(message.sentDate);
        _createdAtText.text = createdAt.ToString("d");
        _messageSubject.text = message.subject;
        _messageType.text = message.type;
        _button.onClick.AddListener(() => callback());

        /*
                if (message.Data.hasPrize)
                {
                    _claimedPrizeImage.SafeSetActive(message.Data.Claimed);
                    _unClaimedPrizeImage.SafeSetActive(!message.Data.Claimed);
                }
                else
                {
                    _claimedPrizeImage.SafeSetActive(false);
                    _unClaimedPrizeImage.SafeSetActive(false);
                }
        */
        /*
        Dictionary<string, Transform> types = new Dictionary<string, Transform>();
        types[TYPE_SALE] = _typeSale;
        types[TYPE_NOTICE] = _typeNotice;
        types[TYPE_REWARD] = _typeReward;
        types[TYPE_ANNOUNCEMENT] = _typeAnnouncement;
        foreach (KeyValuePair<string, Transform> type in types)
        {
            type.Value.gameObject.SetActive(type.Key == message.type);
        }
        */
        read(message.read);
    }
/*
    public void Claim(bool claim = true)
    {
        _claimedPrizeImage.SafeSetActive(claim);
        _unClaimedPrizeImage.SafeSetActive(!claim);
    }
    */
    public void read(bool read = true)
    {
        _unreadState.gameObject.SetActive(!read);
        //_readState.gameObject.SetActive(read);
    }

    public void select(bool selected = true)
    {
        _selectedState.gameObject.SetActive(selected);
    }

    private void Update()
    {
        if (_expires)
        {
            if (TimeUtils.currentTime > message.expirationDate)
            {
                _expires = false;
                _expirationTimeText.text = "Expired";
            }
            else
            {
                _expirationTimeText.text = ("Expires in " + TimeUtils.formatTime(message.expirationDate - TimeUtils.currentTime));
            }
        }
    }
}
