using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;

public class IPManager : MonoBehaviour
{

    public string GetLANIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }


    public void FetchWanIP(Action<string> afterGotten)
    {
        StartCoroutine(this.FetchWanIPCoroutine(afterGotten));
    }

    //Copied and modified from https://gist.github.com/Raziel619/2636dc4c6aaa7f7076432339fa1f8e62
    IEnumerator FetchWanIPCoroutine(Action<string> afterGotten)
    {
        UnityWebRequest www = UnityWebRequest.Get("https://icanhazip.com/");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string result = www.downloadHandler.text;
            //Debug.Log("" + result);
            result = result.Replace("\\r\\n", "").Replace("\\n", "").Trim();
            afterGotten(result);

            Debug.Log("External IP Address = " + result);
        }
    }
}
