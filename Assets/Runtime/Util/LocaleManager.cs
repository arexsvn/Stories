using System.Collections.Generic;
using LitJson;
using System.Linq;
using UnityEngine;
using System;

public class LocaleManager 
{
	//private var _urlRequest:URLRequest;
	//private var _loader:URLLoader;
	//private string _locale;
	private Dictionary<string, string> _localeStringMap;
	readonly static string REPLACE_DELIMITER = "#";

	public LocaleManager()
	{
		_localeStringMap = new Dictionary<string, string>();
	}

	public void addBundle(string strings)
	{
		try
		{
            Dictionary<string, string> newEntries = JsonMapper.ToObject<Dictionary<string, string>>(strings);
            // merge the new entries into the existing dictionary.  This will overwrite duplicates.
            newEntries.ToList().ForEach(x => _localeStringMap[x.Key] = x.Value);
        }
		catch(Exception ex) 
		{
			Debug.LogError($"LocaleManager :: Exception parsing strings : {ex.Message}");
		}
	}
		
    public string lookup(string key)
    {
        if (key == null || !_localeStringMap.ContainsKey(key))
        {
            //Debug.LogWarning($"Key Not found {key}");
            return key;
        }

        string text = _localeStringMap[key];

        // convert new lines
        if (text != null)
        {
            text = text.Replace("<br>", "\n");
        }

        return text;
    }

	public string lookup(string key, Dictionary<string, string> replace)
	{
		string text = lookup(key);

        foreach (KeyValuePair<string, string> replaceItem in replace)
        {
            string[] sep = new string[] { REPLACE_DELIMITER + replaceItem.Key + REPLACE_DELIMITER };
            string[] textArray = text.Split(sep, System.StringSplitOptions.None);
            text = string.Join(replace[replaceItem.Key], textArray);
        }

		return text;
	}

    public string lookup(string key, string[] replace)
    {
        string text = lookup(key);
        int count = 0;

        foreach (string replaceItem in replace)
        {
            string[] sep = new string[] { REPLACE_DELIMITER + count + REPLACE_DELIMITER };
            string[] textArray = text.Split(sep, System.StringSplitOptions.None);
            text = string.Join(replaceItem, textArray);
            count++;
        }

        return text;
    }
    /*
	private void handleComplete(event:Event)
	{
		_loader.removeEventListener(Event.COMPLETE, handleComplete);
		_loader.removeEventListener(IOErrorEvent.IO_ERROR, handleError);

		_locales[_locale] = JSON.parse(_loader.data as String);
		_callback();
	}

	private void handleError(event:IOErrorEvent)
	{
		// log this
	}
*/
    /*
	public string locale
	{
		get 
		{ 
			return _locale; 
		}
	}

	public List<string> languages
	{
		get 
		{ 
			return _languages; 
		}
	}

	public string defaultLanguage
	{
		get 
		{ 
			return _defaultLanguage; 
		}
	}
	*/
}
