using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class MemoryParser
{
    public Dictionary<string, Memory> parse(string memoriesData)
    {
        Dictionary<string, Memory> memoriesById = new Dictionary<string, Memory>();
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(memoriesData);

        XmlNodeList xmlNodes = xml.SelectNodes(".//memory");
        // first extract all dialogue nodes from xml...
        foreach (XmlElement xmlElement in xmlNodes)
        {
            string id = DataUtils.GetAttribute(xmlElement, "id").Trim();
            memoriesById[id] = new Memory(id, DataUtils.GetAttribute(xmlElement, "dialoguesId", true), DataUtils.GetAttribute(xmlElement, "sceneId", true));
        }

        return memoriesById;
    }
}
