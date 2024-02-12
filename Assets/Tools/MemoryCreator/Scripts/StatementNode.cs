using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes
{
    [Color("0099ff")]
    public class StatementNode : CharacterDialogueSerializerNode
    {
        public override void init()
        {
            type = DialogueNode.Type.Statement;
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
            addStatementElement();
        }
    }
}
