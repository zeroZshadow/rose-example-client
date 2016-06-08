using UnityEngine;
using System.Collections;
using System;

using Pb;

public class NetworkManager : MonoBehaviour {
	public NetworkConfig configDev;
	public NetworkConfig configLive;

	public bool development = true;
	public bool printMessages = false;

	public static NetworkConfig config { get; private set; }

	public delegate void OnConnect(bool success);

	// There can only be one (singleton)
	public static NetworkManager Instance { get; private set; }
	void Awake() {
		if (Instance != null) {
			DestroyImmediate(this.gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(this.gameObject);

		// Force production outside of the editor!
#if UNITY_WEBGL && !UNITY_EDITOR
		this.development = false;
#endif
	}

	// Use this for initialization
	IEnumerator Start() {
		// Make sure we run when not focussed
#if !UNITY_WEBGL
		Application.runInBackground = true;
#endif

		// Override dev mode in production if required
		string dev = Parameters.Get("dev");
		if (!string.IsNullOrEmpty(dev)) {
			this.development = true;
		}

		// Select config
		if (this.development) {
			config = configDev;
		} else {
			config = configLive;
		}

		// Connect to master server
		yield return StartCoroutine(ConnectToMaster(config.host));

		// Send a room message
		if (MasterServer.Connected()) {
			// TODO This is where you should send a login
		} else {
			Debug.LogWarning("Not connected to master server");
		}
	}

	public IEnumerator ConnectToMaster(string address) {
		Log("Connecting to " + address);
		WebSocket ws = new WebSocket(new Uri(address));
		yield return StartCoroutine(ws.Connect());

		if (ws.error != null) {
			Debug.LogWarning("Error: " + ws.error);
			ws = null;
			yield break;
		} else {
			Log("Connected to Master");
		}

		if (ws != null) {

			// Start message pump
			MessagePump master = new MessagePump(ws);
			MasterServer.Setup(master, this);
		}
	}

	public IEnumerator ConnectToGame(string address, OnConnect callback) {
		if (GameServer.Connecting || GameServer.Connected()) {
			Debug.LogWarning("Trying to connect to a node while still connected to another node.");
			callback(false);
			yield break;
		}

		GameServer.Connecting = true;

		Log("Connecting to " + address);
		WebSocket ws = new WebSocket(new Uri(address));
		yield return StartCoroutine(ws.Connect());

		if (ws.error != null) {
			Debug.LogError("Error: " + ws.error);
			ws = null;
			yield break;
		} else {
			Log("Connected to Node");
		}

		// Start message pump
		MessagePump game = new MessagePump(ws);
		game.AddDisconnectListener(GameServer.OnDisconnect);
		GameServer.Setup(game, this);

		callback(true);
		GameServer.Connecting = false;
	}

	public static void Log(object message) {
		if (Instance.printMessages) {
			Debug.Log(message);
		}
	}

	public void DisconnectRoom() {
		GameServer.Disconnect();
	}

	void OnApplicationQuit() {
		GameServer.Disconnect();
		MasterServer.Disconnect();
	}
}
