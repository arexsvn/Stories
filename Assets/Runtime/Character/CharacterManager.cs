using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class CharacterManager
{
    private Dictionary<string, List<CharacterStatus>> _status;
    private Dictionary<string, CharacterBio> _characterBios;
    private const string CHARACTERS_PATH = "data/characters";
    public const string CHARACTER_ID_PLAYER = "player";

    public CharacterManager()
    {
        init();
    }

    public void addStatus(CharacterStatus status)
    {
        if (!_status.ContainsKey(status.characterId))
        {
            _status[status.characterId] = new List<CharacterStatus>();
        }

        _status[status.characterId].Add(status);
    }

    public void clearStatus()
    {
        _status.Clear();
    }

    public float getTotalStatus(string characterId)
    {
        float total = 0;

        if (_status.ContainsKey(characterId))
        {
            foreach(CharacterStatus characterStatus in _status[characterId])
            {
                total += characterStatus.status;
            }
        }

        return total;
    }

    public CharacterBio getBio(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            Debug.LogError("characterId is null or empty.");
            return null;
        }

        if (!_characterBios.ContainsKey(characterId))
        {
            Debug.LogError("characterId '" + characterId + "' not found.");
            return null;
        }

        return _characterBios[characterId];
    }

    private void init()
    {
        _status = new Dictionary<string, List<CharacterStatus>>();

        setupBios();
    }

    private void setupBios()
    {
        TextAsset xml = (TextAsset)Resources.Load(CHARACTERS_PATH);
        CharacterParser characterParser = new CharacterParser();
        _characterBios = characterParser.parse(xml.text);
    }
}

public class CharacterParser
{
    public Dictionary<string, CharacterBio> parse(string characterData)
    {
        Dictionary<string, CharacterBio> characterBios = new Dictionary<string, CharacterBio>();
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(characterData);

        XmlNodeList xmlNodes = xml.SelectNodes(".//character");
        foreach (XmlElement xmlElement in xmlNodes)
        {
            string id = DataUtils.GetAttribute(xmlElement, "id", true);
            string name = DataUtils.GetAttribute(xmlElement, "name");
            CharacterBio bio = new CharacterBio(id, name);
            bio.textColor = DataUtils.GetAttribute(xmlElement, "textColor", true);
            bio.details = new List<CharacterDetail>();
            bio.rig = DataUtils.GetBool(xmlElement, "rig");

            XmlNodeList detailNodes = xmlElement.SelectNodes(".//detail");
            foreach (XmlElement detailElement in detailNodes)
            {
                bio.details.Add(new CharacterDetail(DataUtils.GetAttribute(detailElement, "label"), detailElement.InnerXml));
            }

            characterBios[id] = bio;
        }

        return characterBios;
    }
}

public class CharacterStatus
{
    public string characterId;
    public string dialogueId;
    public float status;

    public CharacterStatus(string characterId, string dialogueId, float status)
    {
        this.characterId = characterId;
        this.dialogueId = dialogueId;
        this.status = status;
    }
}

public class CharacterBio
{
    public string characterId;
    public string characterName;
    public string textColor;
    public bool rig;
    public List<CharacterDetail> details;

    public CharacterBio(string characterId, string characterName)
    {
        this.characterId = characterId;
        this.characterName = characterName;
    }
}

public class CharacterDetail
{
    public string detailLabel;
    public string detailText;

    public CharacterDetail(string detailLabel, string detailText)
    {
        this.detailLabel = detailLabel;
        this.detailText = detailText;
    }
}
