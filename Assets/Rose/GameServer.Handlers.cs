using UnityEngine;
using Pb;
using System.IO;
using System;
using System.Collections.Generic;

using Google.Protobuf.Collections;
using Google.Protobuf;

public partial class GameServer {
	private static void SetupHandlers() {
		// Handle messages
		_pump.AddHandler(MessageType.CreateRoom, HandleCreateRoomResponse);
		_pump.AddHandler(MessageType.JoinRoom, HandleJoinRoomResponse);
		_pump.AddHandler(MessageType.Chat, HandleChat);
	}

	private static void HandleCreateRoomResponse(CodedInputStream stream) {
		Pb.RoomResponse response = Pb.RoomResponse.Parser.ParseFrom(stream);

		// Call Lobby to handle the room creation
		Debug.LogFormat("You have created room {0}", response.Id);
	}

	private static void HandleJoinRoomResponse(CodedInputStream stream) {
		Pb.RoomResponse response = Pb.RoomResponse.Parser.ParseFrom(stream);

		// Call the lobby to handle the room joining
		Debug.LogFormat("You have joined room {0}", response.Id);
	}
	
	private static void HandleChat(CodedInputStream stream) {
		Pb.ChatMessage chat = Pb.ChatMessage.Parser.ParseFrom(stream);

		Debug.LogFormat("Chat message: {0}", chat.Message);
	}
}
