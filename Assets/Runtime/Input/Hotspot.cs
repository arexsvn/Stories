using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Hotspot : MonoBehaviour
{
    public enum Type { Item, Move, Memory, Dialogue, Action };
    public Type type;
    public Scene destination;
    public Item item;
    public string memory;
    public string dialogue;
    //public string id;
    //public Button button;
    public HotspotView view;
    public List<string> hideForOutcomes;
    public List<string> showForOutcomes;
}
