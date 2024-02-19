using System.Collections.Generic;
using System;
using signals;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;

public class RequestCreator
{
	WebRequestVO _requestVO;
	// This is simply hardcoded for testing endpoints with salt requirements.  Any endpoint with salt requirements should have those requirements removed 
	//   for use by the companion app.
	private const string BASE_COMM_SALT = "V306425Y1UZ185709UYZXW6YX0V7894W";  
	private CoroutineRunner _coroutineRunner;

	public RequestCreator(CoroutineRunner coroutineRunner, WebRequestVO requestVO)
	{
		_coroutineRunner = coroutineRunner;
		_requestVO = requestVO;
	}
		
	public void sendRequest(WebRequest webRequest)
	{
		string fullUrl;

		if (webRequest.isSecure)
		{
			// we don't include urlVars in the hash generation as they're currently not needed and cause issues with GET requests.
			fullUrl = formatSecureUrl(webRequest.baseUrl, webRequest.webApiUrlOverride, null, webRequest.useSalt);
		} 
		else
		{
			fullUrl = formatUrl(webRequest.baseUrl, webRequest.webApiUrlOverride);
		}

		if (webRequest.urlVars != null)
		{
			string leadingChar;

			foreach(KeyValuePair<string,string> parameter in webRequest.urlVars)
			{
				if (fullUrl.IndexOf("?") == -1)
				{
					leadingChar = "?";
				}
				else
				{
					leadingChar = "&";
				}

				fullUrl += leadingChar + parameter.Key + "=" + parameter.Value;
			}
		}

		UnityWebRequest request;

		if (webRequest.payloadRaw != null)
		{
			request = UnityWebRequest.Put(fullUrl, webRequest.payloadRaw);

			// a hack to allow us to send raw json via POST.  UnityWebRequest.Post and UnityWebRequest.Delete don't handle this properly.
			if (webRequest.requestType == WebRequest.RequestType.Post)
			{
				request.method = "POST";
			}
			else if (webRequest.requestType == WebRequest.RequestType.Delete)
			{
				request.method = "DELETE";
			}
				
			request.SetRequestHeader("Content-Type", "application/json");
		} 
		else
		{
			switch (webRequest.requestType)
			{
				case WebRequest.RequestType.Delete:
					request = UnityWebRequest.Delete(fullUrl);
				break;

				case WebRequest.RequestType.Get:
					request = UnityWebRequest.Get(fullUrl);
				break;

				default:
					#if UNITY_EDITOR
					throw new System.Exception("BPWebRequestCreator :: Invalid RequestType.  'BpWebRequest.payloadRaw' is required for Post and Put types.");
#else
					request = UnityWebRequest.Get(fullUrl);
					break;
#endif
            }
        }

		request.downloadHandler = new DownloadHandlerBuffer();

		webRequest.request = request;
		_coroutineRunner.Run(asyncRequest(webRequest));
	}

	public IEnumerator asyncRequest(WebRequest webRequest)
	{
		yield return webRequest.request.SendWebRequest();
        webRequest.requestComplete();
	}
		
	public string formatUrl(string url, string webApiUrlOverride = null)
	{
		string fullUrl;

		if (webApiUrlOverride != null)
		{
			fullUrl = webApiUrlOverride + url;
		}
		else
		{
			fullUrl = _requestVO.webApiUrl + url;
		}

		return fullUrl;
	}

	public string formatSecureUrl(string url, string webApiUrlOverride = null, Dictionary<string, string> urlVars = null, bool useSalt = false)
	{
		string fullUrl;

		if (webApiUrlOverride != null)
		{
			fullUrl = webApiUrlOverride + url;
		}
		else
		{
			fullUrl = _requestVO.webApiUrl + url;
		}
			
		fullUrl = fullUrl + "?ts=" + TimeUtils.currentTime +
			"&signed_request=" + _requestVO.signedRequest +
			"&game_signed_request=" + _requestVO.gameSignedRequest;

		if (useSalt)
		{
			fullUrl = addHash(fullUrl, BASE_COMM_SALT, urlVars);
		}
			
		return fullUrl;
	}
						
	private string addHash(string url, string saltSeed, Dictionary<string, string> urlVars = null)
	{
		string hdata = "";

		if (urlVars != null)
		{
			foreach(KeyValuePair<string,string> parameter in urlVars)
			{
				hdata += parameter.Value;
			}
		}
			
		System.Random rand = new System.Random();
		// Make hash string
		int hn = (int)(rand.NextDouble() * 9999999);
		string h = getHash(saltSeed, hdata, hn);

		url = url + "&h=" + h + "&hn=" + hn;
		return url;
	}

	private string getHash(string saltSeed, string hdata, int hn)
	{
		return Md5Sum(getSalt(saltSeed) + hdata + getNum(hn));
	}

	private string getSalt(string saltSeed)
	{
		int charPos = saltSeed.Length - 1;
		int charCode;
		List<string> chars = new List<string>();

		do {
			charCode = 90 - (int)(saltSeed[charPos]) + 97;

			if (charCode == 139)
			{
				charCode -= 91;
			}
			else if (charCode >= 130)
			{
				charCode -= 81;
			}

			chars.Add(((char)charCode).ToString());
		}
		while (--charPos >= 0);

		chars.Reverse();

		string result = string.Join("", chars.ToArray());

		return result;
	}

	private int getNum(int hn)
	{
		return ( hn * ( hn % 11 ) );
	}

	private string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);

		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);

		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";

		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}

		return hashString.PadLeft(32, '0');
	}
}

public class WebRequest
{
	public Signal<UnityWebRequest> complete;
	public UnityWebRequest request;
	public bool isSecure;
	// For testing ONLY - Endpoints with salt requirements should be updated to not require it for use with the companion app.  When testing
	//   the BASE_COMM_SALT should be hardcoded to the current browser client salt.
	public bool useSalt = false;
	public string baseUrl;
	public Dictionary<string, string> urlVars;
	public byte[] payloadRaw;
	public enum RequestType {Put, Post, Get, Delete};
	public RequestType requestType = WebRequest.RequestType.Get;
	public string webApiUrlOverride;

	public WebRequest()
	{
		this.complete = new Signal<UnityWebRequest>();
	}

	public void requestComplete()
	{
		this.complete.Dispatch(request);
	}
}

public class WebRequestVO
{
    public string signedRequest;
    public string gameSignedRequest;
    public string webApiUrl;
}
