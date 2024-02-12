using System.Xml;

namespace FlowCanvas.Nodes
{
    public class CharacterDialogueSerializerNode : DialogueSerializerNode
    {
        // configurable in visual node editor
        public string character;
        public string text;
        public float characterStatus;
        public string emotion;
        public string outcome;

        private void addElement()
        {
            setupElement();
            generatedXml.InnerText = nodeId;

            if (_children.Length > 0)
            {
                DialogueSerializerNode child = (_children[0] as DialogueSerializerNode);
                XmlAttribute attribute = xml.CreateAttribute("next");
                attribute.Value = child.nodeId;
                generatedXml.Attributes.Append(attribute);
            }
        }

        protected void addChoiceElement()
        {
            addElement();
            // A choice node will only have a single parent, the question node.
            XmlElement parentQuestionXml = (_parents[0] as QuestionNode).generatedXml;
            parentQuestionXml.AppendChild(generatedXml);
        }

        protected void addDescriptionElement()
        {
            addElement();
        }

        protected void addStatementElement()
        {
            addElement();
        }

        protected void addQuestionElement()
        {
            setupElement();
        }

        private void setupElement()
        {
            if (character != null)
            {
                character = character.Trim();
            }

            if (emotion != null)
            {
                emotion = emotion.Trim();
            }

            generatedXml = xml.CreateElement(_dialogueTypeMap.getType(type));
            addNodeAttributes(generatedXml);
            dialogueRootXml.AppendChild(generatedXml);
            _strings[nodeId] = text;

            if (!string.IsNullOrEmpty(outcome))
            {
                _strings[getOutcomeKey()] = outcome;
            }
        }

        private string getOutcomeKey()
        {
            return nodeId + "_outcome";
        }

        protected virtual void addNodeAttributes(XmlElement xmlElement)
        {
            XmlAttribute attribute = xml.CreateAttribute("id");
            attribute.Value = nodeId;
            xmlElement.Attributes.Append(attribute);

            if (!string.IsNullOrEmpty(character))
            {
                attribute = xml.CreateAttribute("character");
                attribute.Value = character;
                xmlElement.Attributes.Append(attribute);
            }

            if (characterStatus != 0.0F)
            {
                attribute = xml.CreateAttribute("charStatus");
                attribute.Value = characterStatus.ToString();
                xmlElement.Attributes.Append(attribute);
            }

            if (!string.IsNullOrEmpty(emotion))
            {
                attribute = xml.CreateAttribute("emotion");
                attribute.Value = emotion;
                xmlElement.Attributes.Append(attribute);
            }

            if (!string.IsNullOrEmpty(outcome))
            {
                attribute = xml.CreateAttribute("outcome");
                attribute.Value = getOutcomeKey();
                xmlElement.Attributes.Append(attribute);
            }
        }
    }
}
