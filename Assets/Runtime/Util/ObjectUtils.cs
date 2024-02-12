using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
/// Provides a method for performing a deep copy of an object.
/// Binary Serialization is used to perform the copy.
/// </summary>
public static class ObjectClone
{
    public static T Clone<T>(T source)
    {
        var serialized = JsonUtility.ToJson(source);
        return JsonUtility.FromJson<T>(serialized);
    }
}