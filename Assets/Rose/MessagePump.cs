using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Pb;
using Google.Protobuf;

public class MessagePump {
	public delegate void MessageHandler(CodedInputStream stream);

	private Dictionary<MessageType, MessageHandler> _msgMap;
	private List<System.Action> _disconnectHandlers;
	private WebSocket _ws;

	/// <summary>
	/// Create a message pump for the given websocket
	/// </summary>
	/// <param name="websocket">websocket to pump messages for</param>
	public MessagePump(WebSocket websocket) {
		_ws = websocket;
		_disconnectHandlers = new List<System.Action>();
		_msgMap = new Dictionary<MessageType, MessageHandler>();
	}

	/// <summary>
	///  Add a message type to handle
	/// </summary>
	/// <param name="messageType">Type of message</param>
	/// <param name="messageHandler">Function to handle said message</param>
	public void AddHandler(MessageType messageType, MessageHandler messageHandler) {
		_msgMap[messageType] = messageHandler;
	}

	/// <summary>
	/// Add a disconnect listener
	/// </summary>
	/// <param name="func">Function to be called when disconnected</param>
	public void AddDisconnectListener(System.Action func) {
		_disconnectHandlers.Add(func);
	}

	public IEnumerator ReadPump() {
		// Loop forever, untill an error occurs
		while (_ws != null && _ws.error == null && _ws.IsConnected()) {
			// Read all incoming messages
			while (_ws != null && _ws.error == null && _ws.IsConnected()) {
				// Read bytes
				byte[] incoming = _ws.Recv();
				if (incoming != null && _ws.error == null) {
					// Convert for easy reading
					CodedInputStream input = new CodedInputStream(incoming);

					// Fetch message type
					MessageType msgType = (MessageType)input.ReadUInt32();

					// Handle message if registered
					if (_msgMap.ContainsKey(msgType)) {
						// DEBUG
						NetworkManager.Log("Message of type " + msgType.ToString());

						// Call the function that has to handle this message
						_msgMap[msgType](input);
					} else {
						Debug.LogError("Unknown message type " + msgType);
					}
				} else {
					// The last Recv returned nothing, so we have no more messages to process this frame
					break;
				}
			}

			// Did any errors arise?
			if (_ws.error != null) {
				Debug.LogError("WebSocket Error: " + _ws.error);
				break;
			}

			// Wait untill next frame
			yield return 0;
		}

		// We are disconnected, cleanup
		for (int i = 0; i < _disconnectHandlers.Count; i++) {
			_disconnectHandlers[i]();
		}
		_disconnectHandlers.Clear();
		_ws = null;
	}

	public void SendMessage(MessageType messageType, IMessage message) {
		if (_ws == null) {
			Debug.LogWarning("Trying to send to closed socket");
			return;
		}

		// Calculate the size of the message we're going to send
		int size = message.CalculateSize();

		// Create a MemoryStream with message size + 4 (the type)
		using (MemoryStream stream = new MemoryStream(size + 4)) {
			// Create buffer to write in
			CodedOutputStream codedStream = new CodedOutputStream(stream);

			// Actual message
			codedStream.WriteUInt32((uint)messageType);
			message.WriteTo(codedStream);

			codedStream.Flush();

			// Send
			_ws.Send(stream.ToArray());

			// DEBUG
			NetworkManager.Log("Send " + messageType);
		}

		// Did any errors arise?
		if (_ws.error != null) {
			Debug.LogError("WebSocket error: " + _ws.error);
		}
	}

	public bool IsConnected() {
		return _ws != null;
	}

	public void Disconnect() {
		if (_ws != null) {
			_ws.Close();
		}
	}
}
