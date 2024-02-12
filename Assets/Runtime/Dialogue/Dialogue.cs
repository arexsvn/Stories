using System;

public class Dialogue
{
    public NodeTree tree;
    public string id;
    public Dialogue(string id, LeafNode root)
    {
        this.id = id;
        this.tree = new NodeTree(root);
    }
}

[Serializable]
public class DialogueNode : INodeData
{
    public string character;
    public string next;
    public string text;
    public float status;
    public string emotion;
    public string outcome;
    public string memoryId;
    public string label;
    private string _key;
    private string _id;
    public Type type;
    public enum Type { Choice, Statement, Action, Question, Description };

    public DialogueNode(string key, Type type)
    {
        _key = key;
        this.type = type;
    }

    public string id
    {
        get
        {
            if (_id != null)
            {
                return _id;
            }
            return _key;
        }
        set
        {
            _id = value;
        }
    }

    public string key
    {
        get
        {
            if (_key != null)
            {
                return _key;
            }
            return _id;
        }
        set
        {
            _key = value;
        }
    }
}
