using UnityEngine;
using Pb;
using Google.Protobuf;
using System;

public partial class MasterServer {

	private static void SetupHandlers() {
		// Handle messages
		_pump.AddHandler(MessageType.CreateRoom, HandleCreateRoomResponse);
		_pump.AddHandler(MessageType.JoinRoom, HandleJoinRoomResponse);
		_pump.AddHandler(MessageType.ListRooms, HandleListRoomsResponse);
	}

	private static void HandleCreateRoomResponse(CodedInputStream stream) {
		Pb.CreateRoomResponse response = Pb.CreateRoomResponse.Parser.ParseFrom(stream);

		if (!response.Success) {
			Debug.LogWarningFormat("Failed to allocate room {0}", response.Id);
			return;
		}

		if (string.IsNullOrEmpty(response.Address)) {
			Debug.LogError("Invalid node adress.");
			return;
		}

		string url = string.Format(nodeURLFormat, response.Address);

		// Connect to game node
		_manager.StartCoroutine(_manager.ConnectToGame(url, (success) => {
			//We've connected to the game server, join room
			if (!success) {
				Debug.LogWarning("Failed to connect to game server");
				return;
			}

			// Request the room
			GameServer.CreateRoomRequest(response.Id, response.Authtoken);
		}));
	}

	private static void HandleJoinRoomResponse(CodedInputStream stream) {
		Pb.JoinRoomResponse response = Pb.JoinRoomResponse.Parser.ParseFrom(stream);

		if (!response.Success) {
			Debug.LogWarningFormat("Failed to find room {0}", response.Id);
			return;
		}

		if (string.IsNullOrEmpty(response.Address)) {
			Debug.LogError("Invalid node adress.");
			return;
		}

		string url = string.Format(nodeURLFormat, response.Address);

		// Connect to game node
		_manager.StartCoroutine(_manager.ConnectToGame(url, (success) => {
			// We've connected to the game server, join room
			if (!success) {
				Debug.LogWarning("Failed to connect to game server");
				return;
			}

			// Request the room
			GameServer.JoinRoomRequest(response.Id, response.Authtoken);
		}));
	}

	private static void HandleListRoomsResponse(CodedInputStream stream) {
		Pb.ListRoomsResponse list = Pb.ListRoomsResponse.Parser.ParseFrom(stream);
		Pb.RoomInfo first = null;

		if (list.Rooms.Count == 0) {
			Debug.Log("No rooms found, creating one");
			MasterServer.CreateRoomRequest(list.Region);
			return;
		}

		foreach (Pb.RoomInfo room in list.Rooms) {
			if (first == null) {
				first = room;
			}
			Debug.LogFormat("Room {0}: {1}", room.Id, room.Name);
		}

		// DEBUG Join that first room
		MasterServer.JoinRoomRequest(first.Id);
	}
}
