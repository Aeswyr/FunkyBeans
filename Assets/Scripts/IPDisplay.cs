using System;
using UnityEngine;
using TMPro;


public class IPDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText;
    // Start is called before the first frame update
    void Start()
    {
        hintText.text = GetIPAddress();
    }

     public string GetIPAddress()
     {
            string url = "http://checkip.dyndns.org";
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] ipAddressWithText = response.Split(':');
            string ipAddressWithHTMLEnd = ipAddressWithText[1].Substring(1);
            string[] ipAddress = ipAddressWithHTMLEnd.Split('<');
            return ipAddress[0];
            
     }
}
