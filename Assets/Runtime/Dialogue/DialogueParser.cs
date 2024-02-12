using UnityEngine;
using System.Collections.Generic;
using System.Xml;

public class DialogueTypeMap
{
    private Dictionary<string, DialogueNode.Type> _typeByString;
    private Dictionary<DialogueNode.Type, string> _stringByType;

    public DialogueTypeMap()
    {
        init();
    }

    public string getType(DialogueNode.Type type)
    {
        return _stringByType[type];
    }

    public DialogueNode.Type getType(string type)
    {
        return _typeByString[type];
    }

    private void init()
    {
        _typeByString = new Dictionary<string, DialogueNode.Type>();
        _typeByString["statement"] = DialogueNode.Type.Statement;
        _typeByString["question"] = DialogueNode.Type.Question;
        _typeByString["choice"] = DialogueNode.Type.Choice;
        _typeByString["action"] = DialogueNode.Type.Action;
        _typeByString["description"] = DialogueNode.Type.Description;

        _stringByType = new Dictionary<DialogueNode.Type, string>();
        _stringByType[DialogueNode.Type.Statement] = "statement";
        _stringByType[DialogueNode.Type.Question] = "question";
        _stringByType[DialogueNode.Type.Choice] = "choice";
        _stringByType[DialogueNode.Type.Action] = "action";
        _stringByType[DialogueNode.Type.Description] = "description";
    }
}

public class DialogueParser
{
    private DialogueTypeMap _dialogueTypeMap;
    readonly LocaleManager _localeManager;

    public DialogueParser(LocaleManager localeManager)
    {
        _localeManager = localeManager;

        init();
    }

    private void init()
    {
        _dialogueTypeMap = new DialogueTypeMap();
    }

    public Dialogues parse(string dialoguesData)
    {
        Dictionary<string, Dialogue> dialoguesById = new Dictionary<string, Dialogue>();
        Dictionary<string, LeafNode>  dialogueNodes = new Dictionary<string, LeafNode>();
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(dialoguesData);

        XmlNodeList xmlNodes = xml.SelectNodes(".//dialogue");
        // first extract all dialogue nodes from xml...
        foreach (XmlElement xmlElement in xmlNodes)
        {
            string id = DataUtils.GetAttribute(xmlElement, "id");
            string next = DataUtils.GetAttribute(xmlElement, "next");
            parseDialogueNodes(xmlElement, dialogueNodes);
            Dialogue dialogue = new Dialogue(id, dialogueNodes[next]);
            dialoguesById[id] = dialogue;
        }
        // ... then assign child nodes based on the 'next' attribute which maps to a child's id.
        foreach (KeyValuePair<string, LeafNode> node in dialogueNodes)
        {
            DialogueNode dialogueNode = node.Value.nodeData as DialogueNode;
            if (dialogueNode.next != null)
            {
                node.Value.addChild(dialogueNodes[dialogueNode.next]);
            }
        }

        // extract the top level info for the room.
        XmlElement dialoguesNode = (XmlElement)xml.SelectSingleNode(".//dialogues");
        Dialogues sceneDialogues = new Dialogues(DataUtils.GetAttribute(dialoguesNode, "id"), DataUtils.GetAttribute(dialoguesNode, "start"));
        sceneDialogues.dialogues = dialoguesById;
        return sceneDialogues;
    }

    private void parseDialogueNodes(XmlElement dialogueXml, Dictionary<string, LeafNode> dialogueNodes, LeafNode parent = null)
    {
        foreach (XmlElement xmlNode in dialogueXml)
        {
            DialogueNode.Type type = _dialogueTypeMap.getType(xmlNode.Name);
            LeafNode node = null;

            if (type != DialogueNode.Type.Action)
            {
                node = new LeafNode(parseNode(xmlNode));
            }

            if (type == DialogueNode.Type.Question)
            {
                parseDialogueNodes(xmlNode, dialogueNodes, node);
            }

            if (parent != null)
            {
                parent.addChild(node);
            }
            dialogueNodes[node.id] = node;
        }
    }

    private DialogueNode parseNode(XmlElement xmlNode)
    {
        DialogueNode dialogueNode = new DialogueNode(DataUtils.GetAttribute(xmlNode, "key"), _dialogueTypeMap.getType(xmlNode.Name));
        dialogueNode.character = DataUtils.GetAttribute(xmlNode, "character");
        dialogueNode.emotion = DataUtils.GetAttribute(xmlNode, "emotion");
        dialogueNode.outcome = DataUtils.GetAttribute(xmlNode, "outcome");

        string status = DataUtils.GetAttribute(xmlNode, "status");
        if (status != null)
        {
            dialogueNode.status = float.Parse(status);
        }
        
        dialogueNode.id = DataUtils.GetAttribute(xmlNode, "id");
        if (dialogueNode.key == null)
        {
            dialogueNode.key = xmlNode.InnerXml;
        }

        dialogueNode.next = DataUtils.GetAttribute(xmlNode, "next");
        dialogueNode.text = _localeManager.lookup(dialogueNode.key);
        dialogueNode.label = _localeManager.lookup(DataUtils.GetAttribute(xmlNode, "label"));

        return dialogueNode;
    }

    private string getBaseUrl(XmlNode node, string baseUrl = "")
    {
        if (node != null && node.Name == "group")
        {
            baseUrl = getBaseUrl(node.ParentNode, node.Attributes.GetNamedItem("baseURL").Value) + baseUrl;
        }

        return baseUrl;
    }
}
