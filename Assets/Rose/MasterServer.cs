using Pb;
using System;

public partial class MasterServer {
	private static MessagePump _pump;
	private static NetworkManager _manager;

	private const string nodeURLFormat = "ws://{0}/ws";

	public static void Setup(MessagePump pump, NetworkManager manager) {
		_pump = pump;
		_manager = manager;

		SetupHandlers();

		_manager.StartCoroutine(_pump.ReadPump());
	}

	public static void ListRoomsRequest(string region) {
		// Make a new person message
		Pb.ListRoomsRequest room = new Pb.ListRoomsRequest();
		room.Region = region;

		// Write and send message
		_pump.SendMessage(MessageType.ListRooms, room);
	}

	public static void CreateRoomRequest(string region) {
		// Make a new person message
		Pb.CreateRoomRequest room = new Pb.CreateRoomRequest();
		room.Region = region;

		// Write and send message
		_pump.SendMessage(MessageType.CreateRoom, room);
	}

	public static void JoinRoomRequest(ulong joinid) {
		// Make a new person message
		Pb.JoinRoomRequest room = new Pb.JoinRoomRequest();
		room.Id = joinid;

		// Write and send message
		_pump.SendMessage(MessageType.JoinRoom, room);
	}

	public static bool Connected() {
		return _pump != null;
	}

	public static void Disconnect() {
		if (_pump != null) {
			NetworkManager.Log("Disconnecting from master server");
			_pump.Disconnect();
			_pump = null;
		}
	}
}
