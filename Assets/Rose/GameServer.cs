
using Google.Protobuf;
using Pb;
using System;

public partial class GameServer {
	private static MessagePump _pump;
	private static NetworkManager _manager;
	public static bool Connecting = false;

	public static void Setup(MessagePump pump, NetworkManager manager) {
		_pump = pump;
		_manager = manager;

		// Handle messages
		SetupHandlers();

		_manager.StartCoroutine(_pump.ReadPump());
	}

	public static void CreateRoomRequest(ulong roomid, ByteString authtoken) {
		// Make a new person message
		Pb.RoomRequest room = new Pb.RoomRequest();
		room.Id = roomid;
		room.Authtoken = authtoken;

		// Write and send message
		_pump.SendMessage(MessageType.CreateRoom, room);
	}

	public static void JoinRoomRequest(ulong roomid, ByteString authtoken) {
		// Make a new person message
		Pb.RoomRequest room = new Pb.RoomRequest();
		room.Id = roomid;
		room.Authtoken = authtoken;

		// Write and send message
		_pump.SendMessage(MessageType.JoinRoom, room);
	}

	public static bool Connected() {
		return _pump != null;
	}

	// Disconnect can be called publicaly to disconnect from a room. OnDisconnect is called when the connection with the pump is lost.
	public static void Disconnect() {
		if (_pump != null) {
			NetworkManager.Log("Disconnecting from game server");
			_pump.Disconnect();
			_pump = null;
		}
	}

	// OnDisconnect is called when the connection with the pump is lost. Disconnect can be called publicaly to disconnect from a room
	internal static void OnDisconnect() {

	}


	public static void ChatSend(string message) {
		// Make a new person message
		Pb.ChatMessage chat = new Pb.ChatMessage();
		chat.Message = message;

		// Write and send message
		_pump.SendMessage(MessageType.Chat, chat);
	}
}
