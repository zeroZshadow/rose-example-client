var LibraryWebSockets = {
  $webSocketInstances: [],

  SocketCreate: function(url)
  {
    var str = Pointer_stringify(url);
    var socket = {
      socket: new WebSocket(str),
      buffer: new Uint8Array(0),
      error: null,
      messages: []
    }
    
    // Add on message callback
    socket.socket.onmessage = function (e) {
      // Todo: handle other data types?
      if (e.data instanceof Blob)
      {
        var reader = new FileReader();
        reader.addEventListener("loadend", function() {
          var array = new Uint8Array(reader.result);
          // Push buffer on messages stack for this socket
          socket.messages.push(array);
        });
        
        // Read message into array buffer
        reader.readAsArrayBuffer(e.data);
      } else {
        trace("Warning: message type unsupported");
      }
    };
    
    // Add on close callback
    socket.socket.onclose = function (e) {
      if (e.code != 1000)
      {
        if (e.reason != null && e.reason.length > 0)
          socket.error = e.reason;
        else
        {
          switch (e.code)
          {
            case 1001: 
              socket.error = "Endpoint going away.";
              break;
            case 1002: 
              socket.error = "Protocol error.";
              break;
            case 1003: 
              socket.error = "Unsupported message.";
              break;
            case 1005: 
              socket.error = "No status.";
              break;
            case 1006: 
              socket.error = "Abnormal disconnection.";
              break;
            case 1009: 
              socket.error = "Data frame too large.";
              break;
            default:
              socket.error = "Error "+e.code;
          }
        }
      }
    }
    
    // Add socket to active sockets list
    var instance = webSocketInstances.push(socket) - 1;
    return instance;
  },

  SocketState: function (socketInstance)
  {
    var socket = webSocketInstances[socketInstance];
    return socket.socket.readyState;
  },

  SocketError: function (socketInstance, ptr, bufsize)
  {
    var socket = webSocketInstances[socketInstance];
    if (socket.error == null)
      return 0;
    
    var str = socket.error.slice(0, Math.max(0, bufsize - 1));
    writeStringToMemory(str, ptr, false);
    return 1;
  },

  SocketSend: function (socketInstance, ptr, length)
  {
    var socket = webSocketInstances[socketInstance];
    socket.socket.send(HEAPU8.buffer.slice(ptr, ptr+length));
  },

  SocketRecvLength: function(socketInstance)
  {
    var socket = webSocketInstances[socketInstance];
    // If we have no messages recieved, return 0
    if (socket.messages.length == 0)
      return 0;
    
    // Else return the length of the first package on the stack
    return socket.messages[0].length;
  },

  SocketRecv: function (socketInstance, ptr, length)
  {
    var socket = webSocketInstances[socketInstance];
    if (socket.messages.length == 0)
      return 0;
    if (socket.messages[0].length > length)
      return 0;
    HEAPU8.set(socket.messages[0], ptr);
    socket.messages = socket.messages.slice(1);
  },

  SocketClose: function (socketInstance)
  {
    var socket = webSocketInstances[socketInstance];
    socket.socket.close();
  }
};

autoAddDeps(LibraryWebSockets, '$webSocketInstances');
mergeInto(LibraryManager.library, LibraryWebSockets);
