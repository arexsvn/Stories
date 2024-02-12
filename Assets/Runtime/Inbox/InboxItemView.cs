using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InboxItemView : MonoBehaviour
{
    public Button _collect;
    public Button _delete;
    public TextMeshProUGUI messageTitle;
    public TextMeshProUGUI messageBody;
    public Image bannerImage;
    private InboxMessage _message;
    //public Transform prizeContainer;
    //public Transform prizeElement;
    //public Transform prizeClaimedOverlay;
    //public TextMeshProUGUI prizeButtonLabel;
    //public GenericRewardItem genericRewardItem;
    
    public void init(InboxMessage message)
    {
        messageTitle.text = message.subject;
        messageBody.text = message.body;
        _message = message;
        /*
        if (message.Data.hasPrize)
        {
            // TEST!!!!!
            ItemConfiguration itemConfig = new ItemConfiguration();
            itemConfig.Sku = Constants.Currency.HARD;
            itemConfig.Amount = 2;
            addPrize(itemConfig);
            addPrize(itemConfig);
            addPrize(itemConfig);
        }

        if (!string.IsNullOrEmpty(message.image))
        {
            ImageManager.GetInstance().LoadRewardImage(_bannerImage, message.Data.Image, ImageLoaded);
        }
        
        UpdatePrizeState();
        */
    }
    /*
    private void UpdatePrizeState()
    {
        if (_message.Data.hasPrize)
        {
            _collect.SafeSetActive(true);
            _prizeElement.SafeSetActive(true);
            
            if(!_message.Data.Claimed)
            {
                _prizeClaimedOverlay.SafeSetActive(false);
                _collect.onClick.AddListener(collect);
                _prizeButtonLabel.SafeSetText("Claim");
            }
            else
            {
                _collect.enabled = false;
                _prizeClaimedOverlay.SafeSetActive(true);
                _prizeButtonLabel.SafeSetText("Claimed");
            }
        }
        else
        {
            _collect.SafeSetActive(false);
            _prizeElement.SafeSetActive(false);
        }
    }
    
    private void addPrize(ItemConfiguration itemConfiguration)
    {
        _prizeContainer.AddChildPrefab(_genericRewardItem, item => item.Set(itemConfiguration, itemConfiguration.Amount.ToString()));
    }
    */

    /*
    public void deleteMessage()
    {
        if (_message != null)
        {
            List<InboxMessage> messages = new List<InboxMessage>();
            messages.Add(_message);
            DHInboxActions.DeleteMessagesForInbox(messages, delegate(DeleteMessagesForInboxResponse getRewardsForEventResponse)
            {
                if (_callback != null)
                {
                    _callback(getRewardsForEventResponse.InboxMessages);
                }
            });
        }
    }

    private void Update()
    {
        checkPrizeExpired();
    }

    private bool checkPrizeExpired()
    {
        if (_collect.enabled && _message != null && !_message.Data.Claimed && _message.ExpireAt != 0)
        {
            if (TimeUtils.currentTime > _message.ExpireAt)
            {
                _collect.enabled = false;
                _prizeClaimedOverlay.SafeSetActive(true);
                _prizeButtonLabel.SafeSetText("Expired");
                return true;
            }
        }

        return false;
    }
    
    public void collect()
    {
        if (_message != null)
        {
            if (checkPrizeExpired())
            {
                return;
            }

            _message.Data.Claimed = true;
            UpdatePrizeState();
            // TEMP!
            _callbackReward(null, null);
            return;
            List<string> messages = new List<string>();
            messages.Add(_message.CreatedAt.ToString());
            
            DialogGeneric purchasingDialog = UIScreenManager.TopScreenManager.OpenDialog<DialogGeneric>(o => o.Setup("Claiming Reward", "Claiming Reward, Please Wait.")) as DialogGeneric;
            purchasingDialog.allowBack = false;
            
            DHInboxActions.CollectItemsFromInbox(messages, delegate(CollectItemsFromInboxResponse collectItemsFromInboxResponse)
            {
                UIScreenManager.TopScreenManager.CloseDialog(purchasingDialog, true);
                if (_callbackReward != null)
                {
                    _callbackReward(collectItemsFromInboxResponse.InboxMessages, collectItemsFromInboxResponse.Rewards);
                }
            });
        }
    }
    
    private void ImageLoaded(ImageToLoad imageLoaded)
    {
		
    }
    */
}
