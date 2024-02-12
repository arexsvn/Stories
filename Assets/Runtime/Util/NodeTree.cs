using UnityEngine;
using System.Collections.Generic;

public class NodeTree
{
    public LeafNode root;

    public NodeTree(LeafNode root)
    {
        this.root = root;
    }
}

public class LeafNode
{
    public string id;
    public List<LeafNode> children;
    public LeafNode parent;
    public INodeData nodeData;

    public LeafNode(INodeData nodeData, string id = null)
    {
        children = new List<LeafNode>();
        this.nodeData = nodeData;

        if (id == null)
        {
            this.id = nodeData.id;
        }
    }

    public LeafNode addChild(LeafNode node)
    {
        node.parent = this;
        children.Add(node);
        return node;
    }

    public LeafNode removeChild(LeafNode node)
    {
        node.parent = null;
        foreach (LeafNode child in children)
        {
            if (child == node)
            {
                children.Remove(child);
                break;
            }
        }
        return node;
    }
}

public interface INodeData
{
    string id { get; }
}
