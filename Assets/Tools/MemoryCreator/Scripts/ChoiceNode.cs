using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;
using System.Xml;

namespace FlowCanvas.Nodes
{
    [Color("00ffcc")]
    public class ChoiceNode : CharacterDialogueSerializerNode
    {
        public string label;

        public override void init()
        {
            type = DialogueNode.Type.Choice;
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
            addChoiceElement();
        }

        protected override void addNodeAttributes(XmlElement xmlElement)
        {
            base.addNodeAttributes(xmlElement);

            XmlAttribute attribute = xml.CreateAttribute("label");

            if (!string.IsNullOrEmpty(label))
            {
                attribute.Value = getLabelKey();
                _strings[getLabelKey()] = label;
            }
            else
            {
                // If a label key hasn't been set use the full text of the choice as the label.
                attribute.Value = nodeId;
            }

            xmlElement.Attributes.Append(attribute);
        }

        private string getLabelKey()
        {
            return nodeId + "_label";
        }
    }
}
