using UnityEngine;
using ParadoxNotion.Design;
using System.Xml;
using System.IO;
using LitJson;
using System.Text;
using System.Linq;

namespace FlowCanvas.Nodes
{
    [Color("FF0000")]
    public class MemoryStartNode : DialogueSerializerNode
    {
        public static MemoryStartNode instance; // exposing this to an editor script.
        public string memoryId;
        public string sceneId;
        public string memoryTitle;
        public string memoryLabel;

        protected override void RegisterPorts()
        {
            instance = this;
            _portCount = 1;
            base.RegisterPorts();
        }

        // This is called from an editor script to update memory data and dialogue strings before playing.
        public void processAndSerializeNodes()
        {
            memoryId = memoryId.Trim();

            if (sceneId != null)
            {
                sceneId = sceneId.Trim();
            }
            
            _memoryId = memoryId;
            setupStatics();
            initXmlDocument();
            addMemoryIdIfNew();
            addMemoryStrings();
            
            initNodesAndSerialize();
        }

        public override void processNode()
        {

        }

        private void addMemoryStrings()
        {
            // add new string entries
            if (!string.IsNullOrEmpty(memoryTitle))
            {
                _strings[memoryId + "_title"] = memoryTitle;
            }

            if (!string.IsNullOrEmpty(memoryLabel))
            {
                _strings[memoryId + "_hotspot_label"] = memoryLabel;
            }
        }

        private void addMemoryIdIfNew()
        {
            if (_memoryId == null)
            {
                return;
            }

            TextAsset rawText = (TextAsset)Resources.Load(MemoryController.MEMORIES_PATH);
            XmlDocument memoryXml = new XmlDocument();
            XmlElement newMemoryNode = null;
            memoryXml.LoadXml(rawText.text);

            XmlNodeList xmlNodes = memoryXml.SelectNodes(".//memory");
            // first extract all dialogue nodes from xml...
            foreach (XmlElement xmlElement in xmlNodes)
            {
                string id = DataUtils.GetAttribute(xmlElement, "id").Trim();
                if (id != null && id == memoryId)
                {
                    newMemoryNode = xmlElement;
                    newMemoryNode.RemoveAllAttributes();
                    break;
                }
            }

            // if we haven't found the memory id, add it.
            if (newMemoryNode == null)
            {
                newMemoryNode = memoryXml.CreateElement("memory");
            }
           
            XmlAttribute attribute = memoryXml.CreateAttribute("id");
            attribute.Value = memoryId;
            newMemoryNode.Attributes.Append(attribute);
            // For now we'll keep these the same until there is a good use case to override.
            attribute = memoryXml.CreateAttribute("dialoguesId");
            attribute.Value = memoryId;
            newMemoryNode.Attributes.Append(attribute);
            if (!string.IsNullOrEmpty(sceneId))
            {
                attribute = memoryXml.CreateAttribute("sceneId");
                attribute.Value = sceneId;
                newMemoryNode.Attributes.Append(attribute);
            }

            XmlElement memoriesRoot = memoryXml.GetElementsByTagName("memories")[0] as XmlElement;
            memoriesRoot.AppendChild(newMemoryNode);

            string fileName = getResourcesPath() + MemoryController.MEMORIES_PATH + ".xml";
            memoryXml.Save(fileName);
        }

        private void initNodesAndSerialize()
        {
            foreach (NodeCanvas.Framework.Node node in flowGraph.allNodes)
            {
                if (node is DialogueSerializerNode)
                {
                    (node as DialogueSerializerNode).init();
                }
            }

            foreach (NodeCanvas.Framework.Node node in flowGraph.allNodes)
            {
                if (node is DialogueSerializerNode)
                {
                    (node as DialogueSerializerNode).processNode();
                }
            }

            createXMLFile();
            createStringsFile();
        }

        private void createXMLFile()
        {
            string fileName = getResourcesPath() + DialogueController.DIALOGUE_PATH + _memoryId + ".xml";
            xml.Save(fileName);
        }

        public string getResourcesPath()
        {
            return Application.dataPath + "/Resources/";
        }

        private void createStringsFile()
        {
            StringBuilder json = new StringBuilder();
            JsonWriter jsonWriter = new JsonWriter(json);
            jsonWriter.PrettyPrint = true;

            JsonMapper.ToJson(_strings, jsonWriter);

            json.Replace("\\u2019", "'");
            json.Replace("\\u201D", "'");
            json.Replace("\\u2018", "'");
            json.Replace("\\u201C", "'");
            json.Replace("\\u2026", "...");
            
            string fileName = getResourcesPath() + GameController.STRINGS_PATH + ".json";
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(json);
                }
            }
        }
    }
}
