using System.Xml;

public static class DataUtils
{
    public static string GetAttribute(XmlElement xml, string attributeKey, bool trim = false)
    {
        string attribute = xml.GetAttribute(attributeKey);
        if (!string.IsNullOrEmpty(attribute))
        {
            if (trim)
            {
                return attribute.Trim();
            }
            return attribute;
        }
        return null;
    }

    public static bool GetBool(XmlElement xml, string attributeKey)
    {
        string attribute = xml.GetAttribute(attributeKey);
        if (!string.IsNullOrEmpty(attribute))
        {
            attribute = attribute.Trim();
            if (attribute == "1")
            {
                return true;
            }
            return System.Boolean.Parse(attribute);
        }
        return false;
    }
}