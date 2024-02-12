using System;

public static class StringsUtils
{
	public static string firstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }

	public static bool isNull(string input)
	{
		return string.IsNullOrEmpty(input) || input.ToLower() == "null";
	}
}
