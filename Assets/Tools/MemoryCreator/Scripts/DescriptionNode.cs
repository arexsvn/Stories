using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes
{
    [Color("FFA200")]
    public class DescriptionNode : CharacterDialogueSerializerNode
    {
        public override void init()
        {
            type = DialogueNode.Type.Description;
            base.init();
        }

        protected override void RegisterPorts()
        {
            _portCount = 1;
            base.RegisterPorts();
        }

        protected override void generateXML()
        {
            base.generateXML();
            addDescriptionElement();
        }
    }
}
