using UnityEngine;

[System.Serializable]
public class NetworkConfig : ScriptableObject {
	public string host = "ws://localhost:8080/client";
	public string region = "EU";
	public string authKey = "development";
}


