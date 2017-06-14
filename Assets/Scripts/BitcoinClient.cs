using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using UnityEngine;
public class BitcoinClient : MonoBehaviour {
	// Use this for initialization
	private GameManager game;
	private void Start(){
		game = GetComponent<GameManager>();
		//GetExternalIP();
		GetSeedAddresses();
	}
	// Update is called once per frame
	private void Update(){
		
	}
	private void GetSeedAddresses(){
		string seedAddr = "bitseed.xf2.org";
		Dns.BeginGetHostAddresses(seedAddr, (IAsyncResult result) =>{
			IPAddress[] addrs = Dns.EndGetHostAddresses(result);
			foreach(IPAddress addr in addrs){
				game.Post(addr.ToString());
			}
		}, null);
	}
	private void GetExternalIP(){
		
		using(var client = new WebClient()){
			client.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) => {
				string cleanValue = StripHTML(e.Result);
				string[] chunks = cleanValue.Split(' ');
				string ip = chunks[chunks.Length-1];
				ip = ip.Replace(Environment.NewLine, String.Empty);
				try {
					var addr = IPAddress.Parse(ip);
					game.Post(addr.ToString());
				}
				catch(Exception ex){
					game.Post(ex.ToString());
				}

			};
			client.DownloadStringAsync(new Uri("http://91.198.22.70:8245"));
			//client.DownloadStringAsync(new Uri("http://74.208.43.192"));
		}
	}
	private string StripHTML(string input)
	{
		return Regex.Replace(input, "<.*?>", String.Empty);
	}
}