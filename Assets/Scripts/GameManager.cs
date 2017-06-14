using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour {
	// public
	public Text log;
	public Transform spawnParent;
	public Transform spawnAreaCenter;
	public float spawnAreaRadius = 5;
	public GameObject playerPrefab;
	// private
	private Queue<string> messagesToPost = new Queue<string>();
	private Queue<RemoteClient> playersToSpawn = new Queue<RemoteClient>();
	private Queue<GameObject> playersToDespawn = new Queue<GameObject>();
	// Use this for initialization
	private void Start(){
		
	}
	// Update is called once per frame
	private void Update(){
		// GUI
		lock(messagesToPost){
			while(messagesToPost.Count > 0){
				log.text += "\n" + messagesToPost.Dequeue();
			}
		}
		// Spawn
		lock(playersToSpawn){
			while(playersToSpawn.Count > 0){
				RemoteClient client = playersToSpawn.Dequeue();
				Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * spawnAreaRadius;
				randomPosition.y = 1;
				Vector3 position = randomPosition + spawnAreaCenter.position;
				GameObject player = Instantiate(playerPrefab, position, Quaternion.identity, spawnParent);
				client.Player = player;
			}
		}
		// Despawn
		lock(playersToDespawn){
			while(playersToDespawn.Count > 0){
				Destroy(playersToDespawn.Dequeue());
			}
		}
	}
	public void Post(string message){
		lock(messagesToPost){
			messagesToPost.Enqueue(message);
		}
	}
	public void Spawn(RemoteClient client){
		lock(playersToSpawn){
			playersToSpawn.Enqueue(client);
		}
	}
	public void Despawn(GameObject player){
		lock(playersToDespawn){
			playersToDespawn.Enqueue(player);
		}
	}
}