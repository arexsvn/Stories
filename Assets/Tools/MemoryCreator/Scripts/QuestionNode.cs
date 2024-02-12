using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes
{
    [Color("66ff66")]
    public class QuestionNode : CharacterDialogueSerializerNode
    {
        public int choices = 3;

        public override void init()
        {
            type = DialogueNode.Type.Question;
            base.init();
        }

        protected override void RegisterPorts()
        {
            _portCount = choices;
            base.RegisterPorts();
        }

        protected override void generateXML()
        {
            base.generateXML();
            addQuestionElement();
        }
    }
}
