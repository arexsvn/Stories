using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

class Desk : CustomSceneController
{
    [Inject]
    readonly UIController _uiController;
    [Inject]
    readonly InboxController _inboxController;
    
    override public void handleHotspotClick(Hotspot hotspot)
    {
        if (hotspot.type == Hotspot.Type.Action)
        {
            _inboxController.closeButtonClicked.AddOnce(handleInboxClose);
            _inboxController.show();
        }
    }

    override public void handleHotspotOver(Hotspot hotspot)
    {
        if (hotspot.type == Hotspot.Type.Action)
        {
            _uiController.setText("Use PC");
        }
    }

    override public void handleHotspotOff(Hotspot hotspot)
    {

    }

    private void handleInboxClose()
    {
        if (_inboxController.showing)
        {
            _inboxController.closeButtonClicked.Remove(handleInboxClose);
            _inboxController.show(false);
            //unpauseGame.Dispatch();
        }
    }
}
