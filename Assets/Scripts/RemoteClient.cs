using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public class RemoteClient {
	// private
	private TcpClient client;
	private NetworkStream stream;
	private GameObject _player = null;
	private Dictionary<int, Func<string, object, int>> pendingRequests = new Dictionary<int, Func<string, object, int>>();
	// events
	public delegate void DisconnectHandler(RemoteClient client);
	public event DisconnectHandler OnDisconnect;
	public delegate void DataHandler(RemoteClient client, string message);
	public event DataHandler OnData;
	public delegate void SpawnHandler(RemoteClient client);
	public event SpawnHandler OnSpawn;
	// constructor
	public RemoteClient(TcpClient tcpClient){
		client = tcpClient;
		stream = tcpClient.GetStream();
		stream.Flush();
		ReadAsync();
	}
	public GameObject Player {
		get {
			return _player;
		}
		set {
			_player = value;
			OnSpawn(this);
		}
	}
	// public functions
	public void Call(string method, Dictionary<string, object> parameters, Func<string, object, int> cb){
		System.Random r = new System.Random();
		int id;
		lock(pendingRequests){
			do {
				id = r.Next(int.MaxValue);
			} while(pendingRequests.ContainsKey(id));
		}
		pendingRequests.Add(id, cb);
		Request request = new Request(method, parameters, id);
		string json = ToJson(request);
		Write(json);
	}
	public void Call(string method, string key, object value, Func<string, object, int> cb){
		Dictionary<string, object> parameter = new Dictionary<string, object>();
		parameter.Add(key, value);
		Call(method, parameter, cb);
	}
	public void Notify(string method, Dictionary<string, object> parameters = null){
		Request notification = new Request(method, parameters);
		string json = ToJson(notification);
		Write(json);
	}
	public void Notify(string method, string key, object value){
		Dictionary<string, object> parameter = new Dictionary<string, object>();
		parameter.Add(key, value);
		Notify(method, parameter);
	}
	private void Write(string message){
		byte[] buf = Encoding.UTF8.GetBytes(message + "\r\n");
		stream.Write(buf, 0, buf.Length);
	}
	public void Close(){
		client.Close();
	}
	// private functions
	private void ReadAsync(){
		byte[] buf = new byte[1024];
		stream.BeginRead(buf, 0, buf.Length, new AsyncCallback((IAsyncResult result) => {
			int bytes = stream.EndRead(result);
			if(bytes == 0){
				OnDisconnect(this);
				return;
			}
			string json = Encoding.UTF8.GetString(buf, 0, bytes);
			ProcessJson(json);
			ReadAsync();
		}), null);
	}
	private void ProcessJson(string json){
		Request request = RequestFromJson(json);
		if(request == null){
			Response response = ResponseFromJson(json);
			if(response != null){
				ProcessResponse(response);
			}
		}
		else {
			// ProcessRequest(request)
		}
	}
	private void ProcessResponse(Response response){
		int a = 1;
	}
	private string ToJson(object obj){
		JsonSerializerSettings settings = new JsonSerializerSettings();
		settings.NullValueHandling = NullValueHandling.Ignore;
		return JsonConvert.SerializeObject(obj, settings);
	}
	private Request RequestFromJson(string json){
		try {
			return JsonConvert.DeserializeObject<Request>(json);
		}
		catch(Exception){
			return null;
		}
	}
	private Response ResponseFromJson(string json){
		try {
			Response message = JsonConvert.DeserializeObject<Response>(json);
			return message;
		}
		catch(Exception e){
			return null;
		}
	}
}
// {"id":1223, "method":"parseInt", "params": {"a":6, "g":7}}
// {"id":1018632913,"error":"Found error!","result":{"a":5}}