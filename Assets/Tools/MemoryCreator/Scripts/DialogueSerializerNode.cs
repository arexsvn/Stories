using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;
using System.Xml;
using LitJson;
using System;

namespace FlowCanvas.Nodes
{
    public class DialogueSerializerNode : FlowControlNode
    {
        [SerializeField]
        [ExposeField]
        [GatherPortsCallback]
        [MinValue(1)]
        [DelayedField]

        [HideInInspector] protected int _portCount = 1;
        [HideInInspector] protected NodeCanvas.Framework.Node[] _parents;
        [HideInInspector] protected NodeCanvas.Framework.Node[] _children;
        [HideInInspector] public DialogueNode.Type type;
        [HideInInspector] public XmlElement generatedXml;
        [HideInInspector] public string nodeId;

        // shared elements
        public static XmlDocument xml;
        public static XmlElement dialoguesRootXml;
        public static XmlElement dialogueRootXml;
        protected static int _totalNodes = 0;
        protected static int _addedNodes = 0;
        protected static string _memoryId;
        protected static int _totalDialogues = 0;
        protected static XmlAttribute _startingDialogueId;
        protected static DialogueTypeMap _dialogueTypeMap;
        protected static Dictionary<DialogueNode.Type, int> _dialogueTypeCount;
        protected static SortedDictionary<string, string> _strings;

        public virtual void init()
        {
            nodeId = generateId();
        }

        // TODO : move to own instance.
        public void setupStatics()
        {
            _dialogueTypeCount = new Dictionary<DialogueNode.Type, int>();
            _dialogueTypeCount[DialogueNode.Type.Description] = 0;
            _dialogueTypeCount[DialogueNode.Type.Question] = 0;
            _dialogueTypeCount[DialogueNode.Type.Choice] = 0;
            _dialogueTypeCount[DialogueNode.Type.Statement] = 0;
            _dialogueTypeCount[DialogueNode.Type.Action] = 0;

            try
            {
                TextAsset serializedStrings = (TextAsset)Resources.Load(GameController.STRINGS_PATH);
                _strings = JsonMapper.ToObject<SortedDictionary<string, string>>(serializedStrings.text);
            }
            catch(Exception exception)
            {
                Debug.LogError("Error loading strings : " + exception);
            }

            if (_strings == null || _strings.Count == 0)
            {
                _strings = new SortedDictionary<string, string>();
            }

            _dialogueTypeMap = new DialogueTypeMap();
        }

        protected override void RegisterPorts()
        {
            var outs = new List<FlowOutput>();
            for (var i = 0; i < _portCount; i++)
            {
                outs.Add(AddFlowOutput(i.ToString()));
            }
            AddFlowInput("In", (f) =>
            {
                for (var i = 0; i < _portCount; i++)
                {
                    if (!graph.isRunning)
                    {
                        break;
                    }
                    outs[i].Call(f);
                }
            });
        }

        public virtual void processNode()
        {
            _parents = GetParentNodes();
            _children = GetChildNodes();

            generateXML();

            if (string.IsNullOrEmpty(_startingDialogueId.Value) && generatedXml != null && !string.IsNullOrEmpty(generatedXml.InnerXml))
            {
                _startingDialogueId.Value = generatedXml.InnerXml;
            }
        }

        private void createDocumentRoot()
        {
            xml = new XmlDocument();
            dialoguesRootXml = xml.CreateElement("dialogues");
            XmlAttribute attribute = xml.CreateAttribute("id");
            attribute.Value = _memoryId;
            dialoguesRootXml.Attributes.Append(attribute);
            attribute = xml.CreateAttribute("start");
            attribute.Value = generateDialogueId();
            dialoguesRootXml.Attributes.Append(attribute);
            xml.AppendChild(dialoguesRootXml);

            addDialogueRootElement(attribute.Value);
        }

        private void addDialogueRootElement(string dialogueId)
        {
            dialogueRootXml = xml.CreateElement("dialogue");
            XmlAttribute attribute = xml.CreateAttribute("id");
            attribute.Value = dialogueId;
            dialogueRootXml.Attributes.Append(attribute);
            _startingDialogueId = xml.CreateAttribute("next");
            _startingDialogueId.Value = null;
            dialogueRootXml.Attributes.Append(_startingDialogueId);
            dialoguesRootXml.AppendChild(dialogueRootXml);
            _totalDialogues++;
        }

        protected void initXmlDocument()
        {
            if (xml == null)
            {
                createDocumentRoot();
                // all dialogue nodes minus 2 ('start' event node and memory start node.)
                _totalNodes = flowGraph.allNodes.Count - 2;
                _addedNodes = 0;
            }
        }

        protected virtual void generateXML()
        {
            _addedNodes++;
        }

        private string generateId()
        {
            int index = _dialogueTypeCount[type];

            _dialogueTypeCount[type]++;

            return _memoryId + "_" + _dialogueTypeMap.getType(type) + "_" + index;
        }

        private string generateDialogueId()
        {
            return "dialogue_" + _memoryId + "_" + _totalDialogues;
        }
    }
}
