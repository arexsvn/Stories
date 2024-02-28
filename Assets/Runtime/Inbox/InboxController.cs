using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using signals;

public class InboxController
{
    public Signal closeButtonClicked;
    private Dictionary<double, InboxListItemView> _messageListElements;
    private InboxMessage _selectedMessage;
    private int _totalRead = 0;
    private List<InboxMessage> _messages;
    private InboxView _view;
    private bool _showing = false;
    private static string PREFAB = "UI/Inbox/Inbox";

    public InboxController()
    {
        init();
    }

    public void show(bool show = true)
    {
        _showing = show;
        _view.show(show);
    }

    private void init()
    {
        closeButtonClicked = new Signal();

        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(PREFAB));
        _view = prefab.GetComponent<InboxView>();
        _view.deleteReadMessages.onClick.AddListener(verifyDeleteReadMessages);
        _view.closeButton.onClick.AddListener(closeButtonClicked.Dispatch);
        _messageListElements = new Dictionary<double, InboxListItemView>();

        List<InboxMessage> inboxMessages = new List<InboxMessage>();
        InboxMessage message = new InboxMessage();
        message.subject = "Test Message Title";
        message.body = "Some Test Message Text";
        message.sentDate = 1558735824000;
        inboxMessages.Add(message);

        message = new InboxMessage();
        message.subject = "Test Message Title";
        message.body = "More Test Message Text";
        message.sentDate = 1558735824002;
        message.expirationDate = TimeUtils.currentTime + 30000;
        message.type = "reward";
        inboxMessages.Add(message);
        
        message = new InboxMessage();
        message.subject = "Test Message Title";
        message.body = "Other Test Message Text";
        message.sentDate = 1558735824004;
        message.type = "sale";
        inboxMessages.Add(message);

        message = new InboxMessage();
        message.subject = "Test Message Title";
        message.body = "Yet More Test Message Text";
        message.sentDate = 1558735824006;
        message.type = "notice";
        inboxMessages.Add(message);

        message = new InboxMessage();
        message.subject = "Test Message Title";
        message.body = "Last Test Message Text";
        message.sentDate = 1558735824008;
        message.type = "announcement";
        inboxMessages.Add(message);
                
        updateMessages(inboxMessages);
    }

    private void updateMessages(List<InboxMessage> messages, bool autoSelectFirstMessge = true)
    {
        _messages = messages;
        _view.deleteReadMessages.enabled = messages != null && messages.Count > 0;
        _totalRead = 0;
                   
        messages = messages.OrderBy(o => o.sentDate).ToList();
        List<double> inboxListItemKeys = new List<double>(_messageListElements.Keys);
        
        foreach (InboxMessage message in messages)
        {
            inboxListItemKeys.Remove(message.sentDate);
            
            if (message.read)
            {
                _totalRead++;
            }
            
            if (!_messageListElements.ContainsKey(message.sentDate))
            {
                //InboxListItemView inboxListItem = null;// _view.inboxListItemContainer.AddChildPrefab(_view.inboxListItemPrefab, item => item.init(message, () => showMessage(message)));
                InboxListItemView inboxListItem = Object.Instantiate(_view.inboxListItemPrefab, _view.inboxListItemContainer.transform);
                inboxListItem.init(message, () => showMessage(message));
                _messageListElements[message.sentDate] = inboxListItem;
            }
            else
            {
                _messageListElements[message.sentDate].init(message, () => showMessage(message));
            }
        }
        
        // Remove any messages that are currently displayed but not in the list of new messages.
        foreach (double inboxListItemKey in inboxListItemKeys)
        {
            GameObject.Destroy(_messageListElements[inboxListItemKey].gameObject);
            _messageListElements.Remove(inboxListItemKey);
        }

        if (autoSelectFirstMessge)
        {
            showMessage(messages[0]);
        }
    }

    private void verifyDeleteReadMessages()
    {
        /*
        UIScreenManager.TopScreenManager.OpenDialog<DialogGeneric>(o => o.Setup("Delete Read Mail", "Are you sure you'd like to delete " + _totalRead + " read messages?", 
                                                                                new List<ButtonVO> { new ButtonVO("Cancel"), new ButtonVO("Ok", deleteReadMessages) }));
                                                                                */
    }

    private void deleteReadMessages()
    {
        int notDeleted = 0;
        foreach (double inboxListItemKey in new List<double>(_messageListElements.Keys))
        {
            InboxListItemView inboxListItem = _messageListElements[inboxListItemKey];
            if (inboxListItem.message.read)
            {
                // todo, incremement if can't delete because prize not redeemed.
                // notDeleted++;
                _messageListElements.Remove(inboxListItem.message.sentDate);
                GameObject.Destroy(inboxListItem.gameObject);
            }
        }

        _totalRead = notDeleted;

        if (_messageListElements.Count > 0)
        {
            showMessage(_messageListElements.OrderBy(kvp => kvp.Key).First().Value.message);
        }
        else
        {
            _selectedMessage = null;
            _view.deleteReadMessages.enabled = false;
            _view.removeInboxItems();
            updateTotalRead();
        }
    }

    private void updateTotalRead()
    {
        _view.titleText.text = "Inbox (" + (_messageListElements.Count - _totalRead) + "/" + _messageListElements.Count + ")";
    }
    
    private void showMessage(InboxMessage message)
    {
        if (_selectedMessage == null || _selectedMessage != null && _selectedMessage.sentDate != message.sentDate)
        {
            _selectedMessage = message;

            _view.removeInboxItems();

            if (message.read == false)
            {
                message.read = true;
                _totalRead++;
            }

            foreach (KeyValuePair<double, InboxListItemView> inboxListItem in _messageListElements)
            {
                inboxListItem.Value.read(inboxListItem.Value.message.read);
                inboxListItem.Value.select(inboxListItem.Value.message.sentDate == message.sentDate);
            }
        
            updateTotalRead();

            //_view.inboxItemContainer.AddChildPrefab(_view.inboxItemPrefab, item => item.init(message, showMessages));
            InboxItemView inboxItem = Object.Instantiate(_view.inboxItemPrefab, _view.inboxItemContainer.transform);
            inboxItem.init(message);
        }
    }
    
    private void showMessages(List<InboxMessage> inboxMessages)
    {
        updateMessages(inboxMessages);
    }

    public bool showing
    {
        get
        {
            return _showing;
        }
    }
}

public class InboxMessage
{
    public string subject;
    public string body;
    public string type;
    public double sentDate;
    public double expirationDate;
    public bool read;
}
