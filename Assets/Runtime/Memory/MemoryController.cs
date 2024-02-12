using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryController
{
    private Dictionary<string, Memory> _memories;
    private Dictionary<string, List<string>> _memoryIdsByCharacterId;
    public static string MEMORIES_PATH = "data/memories";
    readonly MemoryParser _memoryParser;

    public MemoryController(MemoryParser memoryParser)
    {
        _memoryParser = memoryParser;
        init();
    }

    public void addOutcome(MemoryOutcome outcome)
    {
        // outcomes from dialogs are not stored here currently.
        if (string.IsNullOrEmpty(outcome.memoryId) || !_memories.ContainsKey(outcome.memoryId) || string.IsNullOrEmpty(outcome.characterId))
        {
            return;
        }

        if (_memories[outcome.memoryId].outcomes == null)
        {
            _memories[outcome.memoryId].outcomes = new List<MemoryOutcome>();
        }
        _memories[outcome.memoryId].outcomes.Add(outcome);

        if (!_memoryIdsByCharacterId.ContainsKey(outcome.characterId))
        {
            _memoryIdsByCharacterId[outcome.characterId] = new List<string>();
        }

        if (!_memoryIdsByCharacterId[outcome.characterId].Contains(outcome.memoryId))
        {
            _memoryIdsByCharacterId[outcome.characterId].Add(outcome.memoryId);
        }
    }

    public void clearMemories()
    {
        foreach (KeyValuePair<string, Memory> pairs in _memories)
        {
            if (pairs.Value.outcomes != null)
            {
                pairs.Value.outcomes.Clear();
            }
        }
        _memoryIdsByCharacterId.Clear();
    }

    public Memory getMemory(string memoryId)
    {
        if (!_memories.ContainsKey(memoryId))
        {
            Debug.LogError("Memory Id '" + memoryId + "' not found. Make sure it exists in memory.xml.");
            return null;
        }
        return _memories[memoryId];
    }

    public List<string> getMemoryIds(string characterId)
    {
        if (_memoryIdsByCharacterId.ContainsKey(characterId))
        {
            return _memoryIdsByCharacterId[characterId];
        }
        return null;
    }

    public List<string> getCharacterIdsWithMemories()
    {
        List<string> characterIds = new List<string>();
        foreach (KeyValuePair<string, List<string>> pairs in _memoryIdsByCharacterId)
        {
            characterIds.Add(pairs.Key);
        }
        return characterIds;
    }

    private void init()
    {
        _memoryIdsByCharacterId = new Dictionary<string, List<string>>();

        TextAsset xml = (TextAsset)Resources.Load(MEMORIES_PATH);
        _memories = _memoryParser.parse(xml.text);
    }
}

public class MemoryOutcome
{
    public string characterId;
    public string outcomeId;
    public string memoryId;

    public MemoryOutcome(string characterId, string outcomeId, string memoryId)
    {
        this.characterId = characterId;
        this.outcomeId = outcomeId;
        this.memoryId = memoryId;
    }
}

public class Memory
{
    public string memoryId;
    public string dialoguesId;
    public string sceneId;
    public List<MemoryOutcome> outcomes;
    public double timeCostInMinutes = 0;//60 * 25;

    public Memory(string memoryId, string dialoguesId, string sceneId)
    {
        this.memoryId = memoryId;
        this.dialoguesId = dialoguesId;
        this.sceneId = sceneId;
    }
}
