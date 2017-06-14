using UnityEngine;
using Newtonsoft.Json;
public class Response {
	[JsonProperty("id", Required = Required.Always)]
	private int id;
	[JsonProperty("result")]
	private object result = null;
	[JsonProperty("error")]
	private string error = null;
	public Response(int id, object result = null, string error = null){
		this.id = id;
		if(error != null){
			this.error = error;
			this.result = null;
		}
		else {
			this.result = result;
			this.error = null;
		}
	}
}