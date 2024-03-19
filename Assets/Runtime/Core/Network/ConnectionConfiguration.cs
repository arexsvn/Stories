using System.Collections.Generic;

public class ConnectionConfiguration
{
    public int timeOutMsecs = 5000;
    public string baseUrl = "";
    public Dictionary<string, string> headers = new Dictionary<string, string> {
        {"Content-Type", "application/json"},
        {"Accept", "application/json"}
    };
    public Dictionary<string, string> urlParams = new Dictionary<string, string> {
        {"one", "1"},
        {"two", "2"}
    };
}
