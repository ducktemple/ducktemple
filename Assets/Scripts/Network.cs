using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;
//using Newtonsoft.Json;
public class Network : MonoBehaviour {
	// private
	private GameManager game;
	private TcpListener server;
	private List<RemoteClient> clients = new List<RemoteClient>();
	// Use this for initialization
	private void Start(){
		try {
			game = GetComponent<GameManager>();
			IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 8888);
			server = new TcpListener(serverEP);
			server.Start();
			game.Post("Server is alive");
			ListenAsync();
		}
		catch(Exception e){
			game.Post(String.Format("Exception: {0}", e));
		}
	}
	private void ListenAsync(){
		server.BeginAcceptTcpClient((IAsyncResult result) => {
			TcpClient tcpClient = server.EndAcceptTcpClient(result);
			RemoteClient client = new RemoteClient(tcpClient);
			client.OnDisconnect += DisconnectHandler;
			client.OnData += DataHandler;
			client.OnSpawn += SpawnHandler;
			game.Spawn(client);
			lock(clients){
				clients.Add(client);
			}
			game.Post("New connection");
			ListenAsync();
		}, null);
	}
	private void DisconnectHandler(RemoteClient client){
		if(client.Player != null){
			game.Despawn(client.Player);
		}
		lock(clients){
			if(clients.IndexOf(client) >= 0){
				clients.Remove(client);
			}
		}
		client.Close();
		game.Post("Client disconnected");
	}
	private void DataHandler(RemoteClient client, string json){
		game.Post(json);
	}
	private void SpawnHandler(RemoteClient client){
		Vector3 position = client.Player.transform.position;
		Dictionary<string, object> parameters = new Dictionary<string, object>();
		parameters.Add("x", position.x);
		parameters.Add("y", position.y);
		parameters.Add("z", position.z);
		client.Call("SetPosition", parameters, (string error, object result) =>{
			return 0;
		});
	}
	private void OnApplicationQuit(){
		server.Stop();
		lock(clients){
			foreach(RemoteClient client in clients){
				client.Close();
			}
		}
	}
}