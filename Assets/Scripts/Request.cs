using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public class Request {
	[JsonProperty("id")]
	private int? id;
	[JsonProperty("method", Required = Required.Always)]
	private string method;
	[JsonProperty("params")]
	private Dictionary<string, object> parameters = null;
	public Request(string method, Dictionary<string, object> parameters = null, int? id = null){
		this.id = id;
		this.method = method;
		this.parameters = parameters;
	}
}