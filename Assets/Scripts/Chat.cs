using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Chat : MonoBehaviour {

	public Text chatOutput;
	public InputField chatInput;

	public static Chat instance;

	// Use this for initialization
	void Awake() {
		instance = this;

		chatInput.onEndEdit.AddListener((input) => {
			SendChat(input);
			chatInput.text = "";
		});
	}

	public void AddChat(string str) {
		chatOutput.text += str + "\n";
	}

	private void SendChat(string str) {
		if (GameServer.Connected()) {
			GameServer.ChatSend(str);
		}
	}
}
